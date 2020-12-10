using System;
using System.Linq;
using System.Threading.Tasks;
using VeloTiming.Proto;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Google.Protobuf.WellKnownTypes;

namespace VeloTiming.Server.Services
{
	public class RidersService: Proto.Riders.RidersBase
	{
		private readonly Data.RacesDbContext dbContext;

		public RidersService(Data.RacesDbContext dbContext)
		{
			this.dbContext = dbContext;
		}
		public override async Task<Rider> get(GetRiderRequest request, ServerCallContext context)
		{
			var rider = await dbContext.Riders.Include(r => r.Category).FirstOrDefaultAsync(r => r.Id == request.RiderId);
			if (rider == null) throw new ArgumentException($"Rider not found by Id: '{request.RiderId}'");
			return ToProtoRider(rider);
		}

		public override async Task<GetRidersByRaceResponse> getByRace(GetRidersByRaceRequest request, ServerCallContext context)
		{
			var riders = await dbContext.Riders.Where(c => c.RaceId == request.RaceId).Include(r => r.Category).ToListAsync();

			var response = new GetRidersByRaceResponse();
			response.Riders.AddRange(riders.Select(ToProtoRider));
			return response;
		}

		public override async Task<Rider> add(AddRiderRequest request, ServerCallContext context)
		{
			var rider = new Data.Rider()
			{
				RaceId = request.RaceId
			};
			await UpdateRider(rider, request.Rider);

			dbContext.Riders.Add(rider);
			await dbContext.SaveChangesAsync();

			return ToProtoRider(rider);
		}

		public override async Task<Rider> update(Rider request, ServerCallContext context)
		{
			Data.Rider Rider = await dbContext.Riders.FindAsync(request.Id);
			if (Rider == null) throw new ArgumentException($"Rider not found by Id: '{request.Id}");

			await UpdateRider(Rider, request);
			await dbContext.SaveChangesAsync();

			return ToProtoRider(Rider);
		}

		private async Task UpdateRider(Data.Rider rider, Rider request)
		{
			rider.Number = request.Number;
			rider.FirstName = request.FirstName;
			rider.LastName = request.LastName;
			rider.Sex = Data.SexConverter.FromProto(request.Sex);
			rider.YearOfBirth = request.YearOfBirth;
			rider.Category = string.IsNullOrEmpty(request.Category) ? null :
				await dbContext.RaceCategories.FirstOrDefaultAsync(c => c.RaceId == rider.RaceId && c.Code == request.Category);
			rider.City = request.City;
			rider.Team = request.Team;
		}

		public override async Task<Empty> delete(DeleteRiderRequest request, ServerCallContext context)
		{
			var Rider = await dbContext.Riders.FindAsync(request.RiderId);
			if (Rider != null)
			{
				dbContext.Remove(Rider);
				await dbContext.SaveChangesAsync();
			}

			return new Empty();
		}

		private static Rider ToProtoRider(Data.Rider r)
		{
			return new Rider
			{
				Id = r.Id,
				Number = r.Number ?? "",
				FirstName = r.FirstName ?? "",
				LastName = r.LastName ?? "",
				Sex = Data.SexConverter.ToProto(r.Sex),
				YearOfBirth = r.YearOfBirth,
				Category = r.Category?.Code ?? "",
				CategoryName = r.Category?.Name ?? "",
				City = r.City ?? "",
				Team = r.Team ?? ""
			};
		}
	}
}
