using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VeloTiming.Server.Data;

namespace VeloTiming.Server.Logic
{
	public interface IResultLogic
	{
		Task AddOrUpdateResult(Result result);
	}

	public class ResultLogic : IResultLogic
	{
		private readonly RacesDbContext dataContext;

		public ResultLogic(RacesDbContext dataContext)
		{
			this.dataContext = dataContext;
		}

		public async Task AddOrUpdateResult(Result result)
		{
			var existing = await dataContext.Results.AnyAsync(r => r.Id == result.Id);
			if (existing)
			{
				dataContext.Update(result);
			}
			else
				dataContext.Add(result);
			await dataContext.SaveChangesAsync();
		}
	}
}