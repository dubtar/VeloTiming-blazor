using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VeloTiming.Proto;
using VeloTiming.Server.Data;
using VeloTiming.Server.Logic;

namespace VeloTiming.Server.Services
{
	public class MainService : Main.MainBase
	{
		private readonly IRaceLogic raceLogic;
		private readonly RacesDbContext dataContext;

		public MainService(IRaceLogic raceLogic, RacesDbContext dbContext)
		{
			this.raceLogic = raceLogic;
			dataContext = dbContext;
		}

		public override async Task<Empty> setActiveStart(SetActiveStartRequest request, ServerCallContext context)
		{
			var startId = request.StartId;
			var currentRace = raceLogic.GetRaceInfo();
			if (currentRace != null)
			{
				if (currentRace?.StartId != startId)
					throw new Exception("Другой заезд уже активен");
			}
			else
			{
				try
				{
					var start = await dataContext.Starts.Include(s => s.Race)
						.FirstOrDefaultAsync(s => s.Id == startId);
					if (start == null) throw new Exception($"Start not found by id {startId}");
					start.IsActive = true;

					await dataContext.SaveChangesAsync();
					await raceLogic.SetActiveStart(start,
						await RaceLogic.BuildNumbersDictionary(dataContext, start.Id));
				}
				catch (KeyNotFoundException)
				{
					throw new Exception($"Start not found by id: {startId}");
				}
			}
			return new Empty();
		}

		public override Task<GetRaceInfoResponse> GetRaceInfo(Empty request, ServerCallContext context)
		{
			var result = new GetRaceInfoResponse
			{
				RaceInfo = raceLogic.GetRaceInfo().ToProto()
			};
			return Task.FromResult(result);
		}

		public override async Task<Empty> DeactivateStart(Empty request, ServerCallContext context)
		{
			await raceLogic.SetActiveStart(null, null);
			return new Empty();
		}
	}
}
