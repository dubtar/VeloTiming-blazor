using Grpc.Core;
using System;
using System.Threading.Tasks;
using VeloTiming.Proto;
using Google.Protobuf.WellKnownTypes;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace VeloTiming.Services
{
	public class RacesService : Races.RacesBase
	{
		private readonly Server.Data.RacesDbContext dbContext;

		public RacesService(Server.Data.RacesDbContext dbContext)
		{
			this.dbContext = dbContext;
		}

		public override async Task<GetRacesResponse> getRaces(Empty _, ServerCallContext context)
		{
			var races = await dbContext.Races.ToListAsync();

			var response = new GetRacesResponse();
			response.Races.AddRange(races.Select(ToProtoRace));
			return response;
		}

		public override async Task<Race> getRace(GetRaceRequest request, ServerCallContext context)
		{
			var race = await dbContext.Races.FindAsync(request.RaceId);
			if (race == null) throw new ArgumentException($"Race not found by Id: '{request.RaceId}'");
			return ToProtoRace(race);
		}

		public override async Task<Race> updateRace(Race request, ServerCallContext context)
		{
			Server.Data.Race race;
			if (request.Id > 0)
			{
				race = await dbContext.Races.FindAsync(request.Id);
				if (race == null) throw new ArgumentException($"Race not found by Id: '{request.Id}");
			} else
			{
				race = new Server.Data.Race();
				dbContext.Races.Add(race);
			}
			race.Name = request.Name;
			race.Description = request.Description;
			race.Date = request.Date.ToDateTime();

			await dbContext.SaveChangesAsync();

			return ToProtoRace(race);
		}

		public override async Task<Empty> deleteRace(DeleteRaceRequest request, ServerCallContext context)
		{
			var race = await dbContext.Races.FindAsync(request.RaceId);
			if (race != null)
			{
				dbContext.Remove(race);
				await dbContext.SaveChangesAsync();
			}

			return new Empty();
		}

		private static Race ToProtoRace(Server.Data.Race r)
		{
			return new Race
			{
				Id = r.Id,
				Name = r.Name,
				Description = r.Description,
				Date = Timestamp.FromDateTime(DateTime.SpecifyKind(r.Date, DateTimeKind.Utc))
			};
		}

	}
}