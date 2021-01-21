using Grpc.Net.Client;
using System.Collections.Generic;
using System.Threading.Tasks;
using VeloTiming.Proto;

namespace VeloTiming.Client.Pages.Races
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
	}

	internal class RaceSvc: IRaceSvc
	{
		private readonly GrpcChannel channel;

		public RaceSvc(GrpcChannel channel)
		{
			this.channel = channel;
		}

		private Proto.Races.RacesClient? _client;
		private Proto.Races.RacesClient Client =>
			_client ??= new Proto.Races.RacesClient(channel);

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
			var res = await new Proto.RaceCategories.RaceCategoriesClient(channel)
				.getByRaceAsync(new GetCategoriesByRaceRequest { RaceId = raceId });
			return res.Categories;
		}

		public async Task<string> ImportRiders(ImportRidersRequest request)
		{
			var res = await Client.importRidersAsync(request);
			return res.Result;
		}


		private Proto.Starts.StartsClient? _startsClient;
		private Proto.Starts.StartsClient StartsClient =>
			_startsClient ??= new Proto.Starts.StartsClient(channel);

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
			await StartsClient.setActiveStartAsync(new SetActiveStartRequest { StartId = startId });
		}
	}
}
