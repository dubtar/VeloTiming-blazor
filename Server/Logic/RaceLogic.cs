﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VeloTiming.Server.Data;
using VeloTiming.Server.Hubs;
using VeloTiming.Server.Services;

namespace VeloTiming.Server.Logic
{
	public interface IRaceLogic
	{
		void StartRun(DateTime realStart);
		RaceInfo? GetRaceInfo();
		IEnumerable<Result> GetResults();
		void AddTime(DateTime time, string source);
		void AddNumber(string number, string source);
		void UpdateMark(Result mark);
		Task SetActiveStart(Start? start, Dictionary<string, string>? numbers = null);
		void AddNumberAndTime(string id, DateTime time, string v);
	}

	public class RaceLogic : IRaceLogic
	{
		private RaceInfo? race;
		private readonly List<Result> results = new List<Result>();
		private readonly IServiceProvider serviceProvider;
		private readonly IHubContext<ResultHub, IResultHub> hub;
		private readonly IBackgroundTaskQueue taskQueue;
		private readonly ITimeService timeService;

		public RaceLogic(IServiceProvider serviceProvider)
		{
			this.serviceProvider = serviceProvider;

			hub = serviceProvider.GetService<IHubContext<ResultHub, IResultHub>>() ?? throw new Exception("Cannot resolve IResultHub");
			taskQueue = serviceProvider.GetService<IBackgroundTaskQueue>() ?? throw new Exception("Cannot resolve IBackgroundTaskQueue");
			timeService = serviceProvider.GetService<ITimeService>() ?? throw new Exception("Cannot resolve ITimeService");
			_ = Init();
		}

		public RaceInfo? GetRaceInfo()
		{
			return race;
		}
		private async Task Init()
		{
			using var serviceScope = serviceProvider.CreateScope();
			try
			{
				var dataContext = serviceScope.ServiceProvider.GetRequiredService<RacesDbContext>();
				var activeStart = await dataContext.Starts.Include(s => s.Race).FirstOrDefaultAsync(s => s.IsActive);
				if (activeStart != null)
				{
					var riders = await BuildNumbersDictionary(dataContext, activeStart.Id);
					race = new RaceInfo(activeStart, riders);
					// Load Marks
					results.Clear();
					results.AddRange(dataContext.Results.Where(r => r.StartId == activeStart.Id));
				}
			}
			catch (Exception ex)
			{
				await Console.Error.WriteLineAsync(ex.ToString());
			}
		}

		public static async Task<Dictionary<string, string>> BuildNumbersDictionary(RacesDbContext dataContext, int startId)
		{
			var categoryIds = await dataContext.Set<StartCategory>().Where(s => s.Start.Id == startId).Select(s => s.Category.Id).ToArrayAsync() ?? Array.Empty<int>();
			var riders = await dataContext.Set<Rider>()
				.Where(r => r.Category != null && categoryIds.Contains(r.Category.Id) && !string.IsNullOrEmpty(r.Number))
				.ToDictionaryAsync(r => r.Number, r => $"{r.LastName} {r.FirstName}");
			return riders;
		}

		public async Task SetActiveStart(Start? start, Dictionary<string, string>? numbers = null)
		{
			if (start?.Id == race?.StartId) return;

			if (race != null)
			{
				// remove active flag on active start and set end date
				using var scope = serviceProvider.CreateScope();
				var dataContext = scope.ServiceProvider.GetRequiredService<RacesDbContext>();
				var startEntity = await dataContext.Starts.FindAsync(race.StartId);
				if (startEntity != null)
				{
					startEntity.IsActive = false;
					startEntity.End = DateTime.UtcNow;
					await dataContext.SaveChangesAsync();
				}
				race = null;
			}
			if (start == null)
				race = null;
			else
			{
				if (numbers == null)
				{
					using var scope = serviceProvider.CreateScope();
					var dataContext = scope.ServiceProvider.GetRequiredService<RacesDbContext>();
					numbers = await BuildNumbersDictionary(dataContext, start.Id);
				}

				race = new RaceInfo(start, numbers);
			}
			await hub.Clients.All.ActiveStart(race.ToProto());
		}

		public IEnumerable<Result> GetResults()
		{
			lock (results)
			{
				return results.ToArray();
			}
		}

		private bool RaceIsRunning()
		{
			return race is { Start: {} };
		}

		public void AddTime(DateTime time, string source)
		{
			if (RaceIsRunning())
			{
				time = time.ToUniversalTime();
				taskQueue.QueueBackgroundWorkItem((_) =>
					ProcessAddMark(time, null, source)
				);
			}
		}

		public void AddNumber(string number, string source)
		{
			if (RaceIsRunning())
				taskQueue.QueueBackgroundWorkItem((_) =>
					ProcessAddMark(null, number, source)
				);
		}

		public void AddNumberAndTime(string number, DateTime time, string source)
		{
			if (RaceIsRunning())
				taskQueue.QueueBackgroundWorkItem((_) =>
					ProcessAddMark(time, number, source)
				);
		}

		public void UpdateMark(Result mark)
		{
			taskQueue.QueueBackgroundWorkItem((token) =>
				ProcessUpdateMark(mark, token)
			);
		}

		public void StartRun(DateTime date)
		{
			if (race is { Start: null })
			{
				race.Start = date;
				lock (results)
					results.Clear();
				_ = SendRaceStarted();
			}
		}
		#region send signalr messages to clients
		private async Task SendRaceStarted()
		{
			if (race != null)
				await hub.Clients.All.RaceStarted(race.ToProto()!);
		}

		private async Task SendResultAdded(Result result)
		{
			await hub.Clients.All.ResultAdded(result.ToProto());
		}

		private async Task SendResultUpdated(Result result)
		{
			await hub.Clients.All.ResultUpdated(result.ToProto());
		}
		#endregion

		#region Process inputs methods. Main logic is here
		private const int MARKS_MERGE_SECONDS = 30;
		private Task ProcessAddMark(DateTime? time, string? number, string source)
		{
			var logTask = WriteLog(time, number, source);

			if (race == null) throw new Exception("Race not started");
			string? riderName = null;
			if (!string.IsNullOrEmpty(number) && !race.Numbers.TryGetValue(number, out riderName))
			{
				// Number is not in race numbers - ignore mark
				return Task.CompletedTask;
			}

			var markTime = time ?? timeService.Now;
			// Do not add mark if less minute from start than delay
			if (time.HasValue && race.Start.HasValue && (time.Value - race.Start.Value).TotalMinutes < race.DelayMarksAfterStartMinutes)
				return Task.CompletedTask;

			Task? task = null;
			lock (results)
			{
				var leftTime = markTime.AddSeconds(-MARKS_MERGE_SECONDS);
				var rightTime = markTime.AddSeconds(5);
				var nearbyResults = results.SkipWhile(m => (m.Time ?? m.CreatedOn) < leftTime).TakeWhile(m => (m.Time ?? m.CreatedOn) <= rightTime);
				bool reorder = false;
				bool added = false;
				Result? result = null;
				// determine type of a mark:
				if (time.HasValue && string.IsNullOrWhiteSpace(number))
				{
					// if only time with empty number 
					// then search for number without time and set, or add new time
					result = nearbyResults.FirstOrDefault(r => !r.Time.HasValue);
					if (result == null)
					{
						results.Add(result = Result.Create(timeService, race.StartId));// add new time without number
						added = true;
					}
					result.Time = time;
					result.TimeSource = source;
					reorder = true;
				}
				else if (!time.HasValue && !string.IsNullOrWhiteSpace(number))
				{
					// if number without time - then search for time without number and set. Or 
					result = nearbyResults.FirstOrDefault(r => string.IsNullOrWhiteSpace(r.Number));
					if (result == null)
					{
						results.Add(result = Result.Create(timeService, race.StartId));
						reorder = added = true;
					}
					result.Number = number;
					result.NumberSource = source;
				}
				else if (time.HasValue && !string.IsNullOrWhiteSpace(number))
				{
					// if have both time and number - search if number already exists 
					result = nearbyResults.FirstOrDefault(r => r.Number == number);
					if (result == null)
					{
						result = Result.Create(timeService, race.StartId);
						result.Number = number;
						result.NumberSource = source;
						results.Add(result);
						reorder = added = true;
					}
					// TODO: update time based on source priority
					result.Time = time;
					result.TimeSource = source;
				}

				if (result != null)
				{
					if (riderName != null) result.Name = riderName;
					if (reorder)
						results.Sort((a, b) => (a.Time ?? a.CreatedOn.AddSeconds(MARKS_MERGE_SECONDS)).CompareTo(b.Time ?? b.CreatedOn.AddSeconds(MARKS_MERGE_SECONDS)));
					ProcessPlace(result);

					task = added ? SendResultAdded(result) : SendResultUpdated(result);
					task = Task.WhenAll(task, StoreResult(result));
				}
			}
			return task == null ? logTask : Task.WhenAll(task, logTask);
		}

		private static Task WriteLog(DateTime? time, string? number, string source)
		{
			var val = (time == null ? "" : $"Time: {time} ") + (number == null ? "" : $"Number: {number}");
			return File.AppendAllLinesAsync("marks.log", new[]
			{
				$"{DateTime.UtcNow}: {source} {val}"
			});
		}

		private void ProcessPlace(Result result, int? index = null)
		{
			index ??= results.IndexOf(result);
			if (index < 0) return;
			if (string.IsNullOrEmpty(result.Number))
			{
				result.Lap = -1;
				result.Place = -1;
				return;
			}
			int lap = 0;
			List<int> places = new();
			for (int i = 0; i < index; i++)
			{
				var res = results[i];
				if (res.IsIgnored || string.IsNullOrEmpty(res.Number)) continue;
				int curLap;
				if (res.Number == result.Number)
					curLap = lap++;
				else
					curLap = res.Lap - 1;
				if (places.Count <= curLap)
					places.Add(1);
				else
					places[curLap]++;
			}
			result.Lap = lap + 1;
			result.Place = places.Count > lap ? places[lap] + 1 : 1;
			// recalculate all results after that
			if (results.Count > index.Value + 1)
				ProcessPlace(results[index.Value + 1], index.Value + 1);
		}

		private async Task StoreResult(Result result)
		{
			if (race == null) return;
			using var scope = serviceProvider.CreateScope();
			var resultService = scope.ServiceProvider.GetService<IResultLogic>() ?? throw new Exception($"Cannot resolve {nameof(IResultLogic)}");
			await resultService.AddOrUpdateResult(result);
		}

		// ReSharper disable twice UnusedParameter.Local
		private Task ProcessUpdateMark(Result mark, System.Threading.CancellationToken token)
		{
			lock (results)
			{
				// TODO
			}
			return Task.CompletedTask;
		}
	}
	#endregion
}

public class RaceInfo
{
	public RaceInfo(Start start, Dictionary<string, string> numbers)
	{
		RaceId = start.RaceId;
		StartId = start.Id;
		RaceName = start.Race.Name;
		StartName = start.Name;
		Start = start.RealStart;
		DelayMarksAfterStartMinutes = start.DelayMarksAfterStartMinutes;
		Numbers = new ReadOnlyDictionary<string, string>(numbers);
		Type = start.Type;
	}

	public int RaceId { get; }
	public int StartId { get; }
	public string RaceName { get; }
	public string StartName { get; }
	public DateTime? Start { get; set; }
	public int DelayMarksAfterStartMinutes { get; }
	public ReadOnlyDictionary<string, string> Numbers { get; }
	public StartType Type { get; }
}
