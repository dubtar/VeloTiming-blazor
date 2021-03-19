using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;

namespace VeloTiming.Server.Data
{
	public class RacesDbContext : DbContext
	{
		public RacesDbContext(DbContextOptions<RacesDbContext> options) : base(options) { }
		public DbSet<Race> Races => Set<Race>();
		public DbSet<Number> Numbers => Set<Number>();
		public DbSet<RaceCategory> RaceCategories => Set<RaceCategory>();
		public DbSet<Rider> Riders => Set<Rider>();
		public DbSet<Start> Starts => Set<Start>();
		public DbSet<Result> Results => Set<Result>();

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			// Apply local datetime kind to store as UTC and read as local back
			var dateTimeConverter = new ValueConverter<DateTime, DateTime>(v => v.ToUniversalTime(), v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
			foreach (var entityType in modelBuilder.Model.GetEntityTypes())
			{
				foreach (var property in entityType.GetProperties())
				{
					if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
						property.SetValueConverter(dateTimeConverter);
				}
			}
		}
	}
}
