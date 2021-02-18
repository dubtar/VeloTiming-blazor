using Grpc.Net.Client;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
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
		Task MakeStart();
	}

	internal class RaceSvc: IRaceSvc, IAsyncDisposable
	{
		private readonly GrpcChannel channel;
		private readonly HubConnection hubConnection;
		private RaceInfo? activeStart;

		private readonly BehaviorSubject<RaceInfo?> RaceInfo = new BehaviorSubject<RaceInfo?>(null);
		public IObservable<RaceInfo?> GetRaceInfoSubject() => RaceInfo;

		public RaceSvc(GrpcChannel channel, NavigationManager navigationManager)
		{
			this.channel = channel;

			hubConnection = new HubConnectionBuilder()
				.WithUrl(navigationManager.ToAbsoluteUri("/resultHub"))
				.WithAutomaticReconnect()
				.Build();

			hubConnection.On<RaceInfo?>("ActiveStart", SetActiveStart);
			hubConnection.On<RaceInfo?>("RaceStarted", SetActiveStart);

			// hubConnection.On<Result>("ResultAdded", ResultAdded);
			// hubConnection.On<Result>("ResultUpdated", ResultUpdated);

			_ = hubConnection.StartAsync();
			_ = GetRaceInfo();
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
			activeStart = race;
			RaceInfo.OnNext(activeStart);
		}

		public async Task DeactivateStart()
		{
			if (activeStart != null)
				await MainClient.DeactivateStartAsync(new Google.Protobuf.WellKnownTypes.Empty());
		}

		public async Task<RaceInfo?> GetRaceInfo()
		{
			var result = await MainClient.GetRaceInfoAsync(new Google.Protobuf.WellKnownTypes.Empty());
			SetActiveStart(result.RaceInfo);
			return activeStart;
		}
		public ValueTask DisposeAsync()
		{
			return hubConnection.DisposeAsync();
		}

		public Task MakeStart()
		{
			// TODO
			return Task.CompletedTask;
		}
	}
}
