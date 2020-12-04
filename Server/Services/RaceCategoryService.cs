using Grpc.Core;
using System;
using System.Threading.Tasks;
using VeloTiming.Proto;
using Google.Protobuf.WellKnownTypes;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace VeloTiming.Server.Services
{
	public class RaceCategoryService : RaceCategories.RaceCategoriesBase
	{
		private readonly Data.RacesDbContext dbContext;

		public RaceCategoryService(Data.RacesDbContext dbContext)
		{
			this.dbContext = dbContext;
		}

		public override async Task<RaceCategory> get(GetCategoryRequest request, ServerCallContext context)
		{
			var RaceCategory = await dbContext.RaceCategories.FindAsync(request.RaceCategoryId);
			if (RaceCategory == null) throw new ArgumentException($"RaceCategory not found by Id: '{request.RaceCategoryId}'");
			return ToProtoRaceCategory(RaceCategory);
		}

		public override async Task<GetRaceCategoriesByRaceResponse> getByRace(GetCategoriesByRaceRequest request, ServerCallContext context)
		{
			var raceCategories = await dbContext.RaceCategories.Where(c => c.RaceId == request.RaceId).ToListAsync();

			var response = new GetRaceCategoriesByRaceResponse();
			response.Categories.AddRange(raceCategories.Select(ToProtoRaceCategory));
			return response;
		}

		public override async Task<RaceCategory> add(AddRaceCategoryRequest request, ServerCallContext context)
		{
			var raceCategory = new Data.RaceCategory()
			{
				RaceId = request.RaceId
			};
			UpdateRaceCategory(raceCategory, request.Category);

			dbContext.RaceCategories.Add(raceCategory);
			await dbContext.SaveChangesAsync();

			return ToProtoRaceCategory(raceCategory);
		}

		public override async Task<RaceCategory> update(RaceCategory request, ServerCallContext context)
		{
			Data.RaceCategory raceCategory = await dbContext.RaceCategories.FindAsync(request.Id);
			if (raceCategory == null) throw new ArgumentException($"RaceCategory not found by Id: '{request.Id}");

			UpdateRaceCategory(raceCategory, request);
			await dbContext.SaveChangesAsync();

			return ToProtoRaceCategory(raceCategory);
		}

		private void UpdateRaceCategory(Data.RaceCategory raceCategory, RaceCategory request)
		{
			raceCategory.Name = request.Name;
			raceCategory.Code = request.Code;
			raceCategory.MaxYearOfBirth = request.MaxYearOfBirth;
			raceCategory.MinYearOfBirth = request.MinYearOfBirth;
			raceCategory.Sex = request.Sex == Sex.Any ? null : request.Sex == Sex.Male ? Data.Sex.Male : Data.Sex.Female;
		}

		public override async Task<Empty> delete(DeleteCategoryRequest request, ServerCallContext context)
		{
			var raceCategory = await dbContext.RaceCategories.FindAsync(request.RaceCategoryId);
			if (raceCategory != null)
			{
				dbContext.Remove(raceCategory);
				await dbContext.SaveChangesAsync();
			}

			return new Empty();
		}

		private static RaceCategory ToProtoRaceCategory(Data.RaceCategory r)
		{
			return new RaceCategory
			{
				Id = r.Id,
				Name = r.Name,
				Code = r.Code,
				Sex = r.Sex == null ? Sex.Any : r.Sex == Data.Sex.Male ? Sex.Male : Sex.Female,
				MinYearOfBirth = r.MinYearOfBirth,
				MaxYearOfBirth = r.MaxYearOfBirth
			};
		}
	}
}