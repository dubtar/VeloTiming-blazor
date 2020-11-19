using Grpc.Core;
using System;
using System.Threading.Tasks;
using VeloTiming.Data;
using Google.Protobuf.WellKnownTypes;

namespace VeloTiming.Services
{
	public class RacesService : Races.RacesBase
	{
		public override Task<GetRacesResponse> getRaces(GetRacesRequest request, ServerCallContext context)
		{
			Race[] races = new[]
			{
				new Race { Id = 1, Name = "Test", Description = "Descr",
					Date = Timestamp.FromDateTime(DateTime.UtcNow)
				}
			};
			var result = new GetRacesResponse { };
			result.Races.Add(races);

			return Task.FromResult(result);
		}

	}
}