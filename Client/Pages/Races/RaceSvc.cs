using Grpc.Net.Client;
using System.Collections.Generic;
using System.Threading.Tasks;
using VeloTiming.Proto;

namespace VeloTiming.Client.Pages.Races
{
	internal interface IRaceSvc
	{
		Task<ICollection<Race>> GetAllRaces();
		Task<Race> GetRace(int raceId);

		Task<int> UpdateRace(Race race);
		Task DeleteRace(int raceId);
	}

	internal class RaceSvc: IRaceSvc
	{
		private readonly GrpcChannel channel;

		public RaceSvc(GrpcChannel channel)
		{
			this.channel = channel;
		}

		private Proto.Races.RacesClient Client =>
			new Proto.Races.RacesClient(channel);

		public async Task<Race> GetRace(int raceId)
		{
			var req = new GetRaceRequest() { RaceId = raceId };
			var res = await Client.getRaceAsync(req);
			return res;
		}

		public async Task<ICollection<Race>> GetAllRaces()
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
	}
}
