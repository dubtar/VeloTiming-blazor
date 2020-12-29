using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VeloTiming.Server.Data
{
	public class Race
	{
		public Race(string name, DateTime date)
		{
			Name = name;
			Date = date;
		}
		public int Id { get; set; }
		[Required, MaxLength(50)]
		public string Name { get; set; }
		public DateTime Date { get; set; }
		public virtual ICollection<RaceCategory> Categories { get; set; } = null!;
		public virtual ICollection<Rider> Riders { get; set; } = null!;

		public virtual ICollection<Start> Starts { get; set; } = null!;

		public string? Description { get; set; }
	}
}
