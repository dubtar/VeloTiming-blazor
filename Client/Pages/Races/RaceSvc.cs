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
	}

	internal class RaceSvc: IRaceSvc
	{
		private readonly GrpcChannel channel;

		public RaceSvc(GrpcChannel channel)
		{
			this.channel = channel;
		}

		public Task<ICollection<Race>> GetAllRaces()
		{
			throw new System.NotImplementedException();
		}

		public Task<Race> GetRace(int raceId)
		{
			var client = new Proto.Races.RacesClient(channel);
			var req = new GetRaceRequest { raceId = raceId };
			var res = await client.getRace()
		}

		public async Task<ICollection<Race>> GetAllRaces()
		{
			var client = new Proto.Races.RacesClient(channel);
			var req = await client.getRacesAsync(new GetRacesRequest());
			return req.Races;
		}
	}
}
