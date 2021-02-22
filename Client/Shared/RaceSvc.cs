using Grpc.Net.Client;
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
	}

	internal class RaceSvc: IRaceSvc, IAsyncDisposable
	{
		private readonly GrpcChannel channel;
		private readonly HubConnection hubConnection;

		private readonly BehaviorSubject<RaceInfo?> RaceInfo = new BehaviorSubject<RaceInfo?>(null);
		public IObservable<RaceInfo?> GetRaceInfoSubject() => RaceInfo;

		private readonly BehaviorSubject<Result[]> Results = new BehaviorSubject<Result[]>(new Result[0]);
		public IObservable<Result[]> GetResultsSubscription() => Results;

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

		private Races.RacesClient? _client;
		private Races.RacesClient Client =>
			_client ??= new Races.RacesClient(channel);

		private Main.MainClient? _mainClient;
		private Main.MainClient MainClient =>
			_mainClient ??= new Main.MainClient(channel);

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


		private Starts.StartsClient? _startsClient;
		private Starts.StartsClient StartsClient =>
			_startsClient ??= new Starts.StartsClient(channel);

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
			RaceInfo.OnNext(race);
		}

		public async Task DeactivateStart()
		{
			if (RaceInfo.Value != null)
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
			var results = Results.Value.ToList();
			var newResTime = GetResultTime(result);
			for (var i = 0; i < results.Count; i++)
			{
				var curRes = results[i];
				var curResTime = GetResultTime(curRes);
				if (curResTime > newResTime)
				{
					results.Insert(i, result);
					inserted = true;
					break;
				}
			}
			if (!inserted)
			{
				results.Add(result);
			}
			Results.OnNext(results.ToArray());
		}

		private void ResultUpdated(Result result)
		{
			var results = Results.Value.ToList();
			var ind = results.FindIndex(r => r.Id == result.Id);
			if (ind >= 0)
			{
				var timeChanged = GetResultTime(result) != GetResultTime(results[ind]);
				results[ind] = result;
				if (timeChanged) results.Sort((a, b) => (int)(GetResultTime(a).Ticks - GetResultTime(b).Ticks));
				Results.OnNext(results.ToArray());
			}
		}

		public static DateTime GetResultTime(Result result)
		{
			return result.Time?.ToDateTime() ?? result.CreatedOn.ToDateTime();
		}

		public async Task LoadResults()
		{
			var results = await MainClient.GetResultsAsync(new Google.Protobuf.WellKnownTypes.Empty());
			Results.OnNext(results.Results.ToArray());
		}
	}
}
