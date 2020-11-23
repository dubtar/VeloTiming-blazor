using Microsoft.EntityFrameworkCore;

namespace VeloTiming.Server.Data
{
	public class RacesDbContext : DbContext
	{
		public RacesDbContext(DbContextOptions<RacesDbContext> options) : base(options) { }
		public DbSet<Race> Races { get; set; }
	}
}
