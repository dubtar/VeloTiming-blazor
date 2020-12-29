using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using VeloTiming.Server.Data;

namespace VeloTiming.Server.Services
{
	public class NumberService: Proto.Numbers.NumbersBase
	{
		private readonly RacesDbContext dbContext;

		public NumberService(RacesDbContext dbContext)
		{
			this.dbContext = dbContext;
		}
		public override async Task<Proto.GetAllNumbersResponse> GetAllNumbers(Empty request, ServerCallContext context)
		{
			var numbers = await dbContext.Numbers.OrderBy(n => n.Id).ToListAsync();

			var response = new Proto.GetAllNumbersResponse();
			response.Numbers.AddRange(numbers.Select(ToNumberProto));
			return response;
		}

		public override async Task<Empty> DeleteNumber(Proto.Number request, ServerCallContext context)
		{
			var number = await dbContext.Numbers.FindAsync(request.Id);
			if (number != null)
			{
				dbContext.Remove(number);
				await dbContext.SaveChangesAsync();
			}
			return new Empty();
		}

		public override async Task<Empty> UpdateNumber(Proto.Number request, ServerCallContext context)
		{
			var number = await dbContext.Numbers.FindAsync(request.Id);
			if (number == null)
			{
				number = new Number(request.Id, "");
				dbContext.Add(number);
			}
			number.NumberRfids = request.Rfids;
			await dbContext.SaveChangesAsync();
			return new Empty();
		}

		private Proto.Number ToNumberProto(Number number)
		{
			return new Proto.Number
			{
				Id = number.Id ?? string.Empty,
				Rfids = number.NumberRfids ?? string.Empty
			};
		}
	}
}
