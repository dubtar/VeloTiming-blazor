﻿using Grpc.Net.Client;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using VeloTiming.Proto;

namespace VeloTiming.Client
{
	internal interface IRaceSvc
	{
		Task<IList<Race>> GetAllRaces();
		Task<Race> GetRace(int raceId);

		Task<int> UpdateRace(Race race);
		Task DeleteRace(int raceId);

		Task<IList<RaceCategory>> GetRaceCategories(int raceId);

		Task<string> ImportRiders(ImportRidersRequest request);
		Task<IList<Start>> GetStarts(int raceId);
		Task DeleteStart(int startId);
		Task<Start> AddStart(int raceId, Start start);
		Task<Start> UpdateStart(Start start);

		Task SetActiveStart(int startId);
		Task DeactivateStart();
		IObservable<RaceInfo?> GetRaceInfoSubject();
		IObservable<Result[]> GetResultsSubscription();
		Task MakeStart(int startId);
		void AddTime(string source);
		void AddNumber(string number, string source);
	}

	internal class RaceSvc: IRaceSvc, IAsyncDisposable
	{
		private readonly GrpcChannel channel;
		private readonly HubConnection hubConnection;

		private readonly BehaviorSubject<RaceInfo?> raceInfo = new BehaviorSubject<RaceInfo?>(null);
		public IObservable<RaceInfo?> GetRaceInfoSubject() => raceInfo;

		private readonly BehaviorSubject<Result[]> results = new BehaviorSubject<Result[]>(new Result[0]);
		public IObservable<Result[]> GetResultsSubscription() => results;

		public RaceSvc(GrpcChannel channel, NavigationManager navigationManager)
		{
			this.channel = channel;

			hubConnection = new HubConnectionBuilder()
				.WithUrl(navigationManager.ToAbsoluteUri("/resultHub"))
				.WithAutomaticReconnect()
				.Build();

			hubConnection.On<RaceInfo?>("ActiveStart", SetActiveStart);
			hubConnection.On<RaceInfo?>("RaceStarted", SetActiveStart);

			hubConnection.On<Result>("ResultAdded", ResultAdded);
			hubConnection.On<Result>("ResultUpdated", ResultUpdated);

			_ = hubConnection.StartAsync();
			_ = LoadRaceInfo();
			_ = LoadResults();
		}

		private Races.RacesClient? client;
		private Races.RacesClient Client =>
			client ??= new Races.RacesClient(channel);

		private Main.MainClient? mainClient;
		private Main.MainClient MainClient =>
			mainClient ??= new Main.MainClient(channel);

		public async Task<Race> GetRace(int raceId)
		{
			var req = new GetRaceRequest() { RaceId = raceId };
			var res = await Client.getRaceAsync(req);
			return res;
		}

		public async Task<IList<Race>> GetAllRaces()
		{
			var req = await Client.getRacesAsync(new Google.Protobuf.WellKnownTypes.Empty());
			return req.Races;
		}

		public async Task<int> UpdateRace(Race race)
		{
			var res = await Client.updateRaceAsync(race);
			return res.Id;
		}

		public async Task DeleteRace(int raceId)
		{
			await Client.deleteRaceAsync(new DeleteRaceRequest { RaceId = raceId });
		}
		public async Task<IList<RaceCategory>> GetRaceCategories(int raceId)
		{
			var res = await new RaceCategories.RaceCategoriesClient(channel)
				.getByRaceAsync(new GetCategoriesByRaceRequest { RaceId = raceId });
			return res.Categories;
		}

		public async Task<string> ImportRiders(ImportRidersRequest request)
		{
			var res = await Client.importRidersAsync(request);
			return res.Result;
		}


		private Starts.StartsClient? startsClient;
		private Starts.StartsClient StartsClient =>
			startsClient ??= new Starts.StartsClient(channel);

		public async Task<IList<Start>> GetStarts(int raceId)
		{
			var res = await StartsClient.getByRaceAsync(new GetStartsByRaceRequest { RaceId = raceId });
			return res.Starts;
		}

		public async Task DeleteStart(int startId)
		{
			await StartsClient.deleteAsync(new DeleteStartRequest { StartId = startId });
		}

		public async Task<Start> UpdateStart(Start start)
		{
			return await StartsClient.updateAsync(start);
		}
		public async Task<Start> AddStart(int raceId, Start start)
		{
			return await StartsClient.addAsync(new AddStartRequest { RaceId = raceId, Start = start });
		}

		public async Task SetActiveStart(int startId)
		{
			await MainClient.setActiveStartAsync(new SetActiveStartRequest { StartId = startId });
		}

		private void SetActiveStart(RaceInfo? race)
		{
			raceInfo.OnNext(race);
			_ = LoadResults();
		}

		public async Task DeactivateStart()
		{
			if (raceInfo.Value != null)
				await MainClient.DeactivateStartAsync(new Google.Protobuf.WellKnownTypes.Empty());
		}

		private async Task<RaceInfo?> LoadRaceInfo()
		{
			var result = await MainClient.GetRaceInfoAsync(new Google.Protobuf.WellKnownTypes.Empty());
			SetActiveStart(result.RaceInfo);
			return result.RaceInfo;
		}
		public ValueTask DisposeAsync()
		{
			return hubConnection.DisposeAsync();
		}

		public async Task MakeStart(int startId)
		{
			await MainClient.MakeStartAsync(new MakeStartRequest { StartId = startId });
		}

		private void ResultAdded(Result result)
		{
			bool inserted = false;
			var res = this.results.Value.ToList();
			var newResTime = GetResultTime(result);
			for (var i = 0; i < res.Count; i++)
			{
				var curRes = res[i];
				var curResTime = GetResultTime(curRes);
				if (curResTime > newResTime)
				{
					res.Insert(i, result);
					inserted = true;
					break;
				}
			}
			if (!inserted)
			{
				res.Add(result);
			}
			this.results.OnNext(res.ToArray());
		}

		private void ResultUpdated(Result result)
		{
			var res = this.results.Value.ToList();
			var ind = res.FindIndex(r => r.Id == result.Id);
			if (ind >= 0)
			{
				var timeChanged = GetResultTime(result) != GetResultTime(res[ind]);
				res[ind] = result;
				if (timeChanged) res.Sort((a, b) => (int)(GetResultTime(a).Ticks - GetResultTime(b).Ticks));
				this.results.OnNext(res.ToArray());
			}
		}

		private static DateTime GetResultTime(Result result)
		{
			return result.Time?.ToDateTime() ?? result.CreatedOn.ToDateTime();
		}

		private async Task LoadResults()
		{
			var res = await MainClient.GetResultsAsync(new Google.Protobuf.WellKnownTypes.Empty());
			this.results.OnNext(res.Results.ToArray());
		}
		public void AddTime(string source)
		{
			_ = hubConnection.SendAsync("AddTime", DateTime.UtcNow, source);
		}

		public void AddNumber(string number, string source)
		{
			_ = hubConnection.SendAsync("AddNumber", number, source);
		}
	}
}
