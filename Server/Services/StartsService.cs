using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using VeloTiming.Proto;

namespace VeloTiming.Server.Services
{
	public class StartsService : Starts.StartsBase
	{
		private readonly Data.RacesDbContext dbContext;

		public StartsService(Data.RacesDbContext dbContext)
		{
			this.dbContext = dbContext;
		}
		public override async Task<GetStartsByRaceResponse> getByRace(GetStartsByRaceRequest request, ServerCallContext context)
		{

			var starts = await dbContext.Starts.Include(s => s.Categories).Where(c => c.RaceId == request.RaceId).ToListAsync();

			var response = new GetStartsByRaceResponse();
			response.Starts.AddRange(starts.Select(ToProtoStart));
			return response;
		}

		public override async Task<Start> add(AddStartRequest request, ServerCallContext context)
		{
			var start = new Data.Start()
			{
				RaceId = request.RaceId
			};
			UpdateStart(start, request.Start);

			dbContext.Starts.Add(start);
			await dbContext.SaveChangesAsync();

			return ToProtoStart(start);
		}

		private void UpdateStart(Data.Start entity, Start model)
		{
			// update only properties edited in form
			entity.DelayMarksAfterStartMinutes = model.DelayMarksAfterStartMinutes;
			entity.Name = model.Name;
			entity.PlannedStart = model.PlannedStart?.ToDateTime();
			entity.Type = model.Type.FromProto();
			Utils.UpdateCollection(entity.Categories, model.Categories,
				(e, m) => e.Category.Id == m.Id,
				(m) => new Data.StartCategory { Start = entity, Category = dbContext.RaceCategories.Find(m.Id) }
				);
		}

		public static Start ToProtoStart(Data.Start start)
		{
			var result = new Start
			{
				DelayMarksAfterStartMinutes = start.DelayMarksAfterStartMinutes,
				End = start.End.ToTimestamp(),
				Name = start.Name,
				Id = start.Id,
				PlannedStart = start.PlannedStart.ToTimestamp(),
				RealStart = start.RealStart.ToTimestamp(),
				Type = start.Type.ToProto()
			};
			result.Categories.AddRange(start.Categories.Select(c => new Start.Types.Category
			{
				Id = c.Category.Id,
				Code = c.Category.Code,
				Name = c.Category.Name
			}));
			return result;
		}
	}
}
