using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VeloTiming.Server.Data
{
	public class Race
	{
		public int Id { get; set; }
		[Required, MaxLength(50)]
		public string Name { get; set; }
		public DateTime Date { get; set; }
		public virtual ICollection<RaceCategory> Categories { get; set; }
		public virtual ICollection<Rider> Riders { get; set; }

		public virtual ICollection<Start> Starts { get; set; }

		public string Description { get; set; }
	}
}
