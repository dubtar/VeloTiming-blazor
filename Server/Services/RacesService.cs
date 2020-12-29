using Grpc.Core;
using System;
using System.Threading.Tasks;
using VeloTiming.Proto;
using Google.Protobuf.WellKnownTypes;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace VeloTiming.Server.Services
{
	public class RacesService : Races.RacesBase
	{
		private readonly Data.RacesDbContext dbContext;

		public RacesService(Data.RacesDbContext dbContext)
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
				race.Name = request.Name;
				race.Date = request.Date.ToDateTime();
			}
			else
			{
				race = new Data.Race(request.Name, request.Date.ToDateTime());
				dbContext.Races.Add(race);
			}
			race.Description = request.Description;

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

		public override async Task<ImportRidersResponse> importRiders(ImportRidersRequest request, ServerCallContext context)
		{
			int added = 0, failed = 0, existed = 0;
			int raceId = request.RaceId;

			const string SEPARATOR = ";";

			var race = await dbContext.Races.Include(r => r.Categories).FirstOrDefaultAsync(r => r.Id == raceId);
			if (race == null) throw new Exception($"Race not found by Id: {raceId}");

			var allRiders = await dbContext.Riders.Where(r => r.RaceId == raceId).ToListAsync();

			var rows = request.Content.Split('\n');
			if (request.SkipFirstRow)
				rows = rows.Skip(1).ToArray();

			var columnTypes = request.Columns;
			foreach (var row in rows)
			{
				var items = row.Split(SEPARATOR);
				Data.Rider rider = new Data.Rider() { RaceId = request.RaceId };
				for (var i = 0; i < columnTypes.Count; i++)
				{
					if (items.Length <= i) break;
					string value = items[i];
					if (!string.IsNullOrWhiteSpace(value))
					{
						switch (columnTypes[i])
						{
							case RiderImportColumnType.City: rider.City = value; break;
							case RiderImportColumnType.Firstlastname:
								var splited = value.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
								if (splited.Length == 2)
								{
									rider.FirstName = splited[0].Trim();
									rider.LastName = splited[1].Trim();
								}
								break;
							case RiderImportColumnType.Firstname: rider.FirstName = value.Trim(); break;
							case RiderImportColumnType.Lastname: rider.LastName = value.Trim(); break;
							case RiderImportColumnType.Lastfirstname:
								splited = value.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
								if (splited.Length == 2)
								{
									rider.LastName = splited[0].Trim();
									rider.FirstName = splited[1].Trim();
								}
								break;
							case RiderImportColumnType.Sex:
								rider.Sex = items[i].StartsWith("ж", StringComparison.CurrentCultureIgnoreCase) ?
									Data.Sex.Female : Data.Sex.Male;
								break;
							case RiderImportColumnType.Team: rider.Team = value; break;
							case RiderImportColumnType.Year:
								if (int.TryParse(value, out int year) && year > 1900 && year < DateTime.Now.Year)
									rider.YearOfBirth = year;
								break;
						}
					}
				}
				if (string.IsNullOrWhiteSpace(rider.FirstName) || string.IsNullOrWhiteSpace(rider.LastName))
					failed++;
				else
				{
					var existingRider = allRiders.FirstOrDefault(r => r.FirstName.Equals(rider.FirstName, StringComparison.CurrentCultureIgnoreCase)
						&& r.LastName.Equals(rider.LastName, StringComparison.CurrentCultureIgnoreCase));
					if (existingRider == null)
					{
						// determine category
						rider.Category = race.Categories.FirstOrDefault(cat =>
							cat != null &&
							(cat.MinYearOfBirth != null || cat.MaxYearOfBirth != null) // skip categories without years
							&&
								(cat.Sex == null || cat.Sex == rider.Sex) &&
								(cat.MinYearOfBirth == null || cat.MinYearOfBirth <= rider.YearOfBirth) &&
								(cat.MaxYearOfBirth == null || cat.MaxYearOfBirth >= rider.YearOfBirth)
						);
						dbContext.Add(rider);
						added++;
					}
					else
					{
						// update only City and Team if set
						if (!string.IsNullOrWhiteSpace(rider.City))
							existingRider.City = rider.City;
						if (!string.IsNullOrWhiteSpace(rider.Team))
							existingRider.Team = rider.Team;
						dbContext.Update(existingRider);
						existed++;
					}
				}
			}

			await dbContext.SaveChangesAsync();

			string result = $"Всего: {added + existed + failed}Добавлено: {added}";
			if (existed > 0)
				result += $", повторов: {existed}";
			if (failed > 0)
				result += $", игнорировано: {failed}";
			return new ImportRidersResponse()
			{
				Result = result
			};
		}

		private static Race ToProtoRace(Data.Race r)
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