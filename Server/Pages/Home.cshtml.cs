using System.Linq;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VeloTiming.Server.Data;

namespace VeloTiming.Server
{
	public class HomeModel : PageModel
	{
		public HomeModel(RacesDbContext dbContext)
		{
			this.dbContext = dbContext;
		}
		public int RacesCount;
		private readonly RacesDbContext dbContext;

		public void OnGet()
		{
			RacesCount = dbContext.Races.Count();
		}
	}
}
