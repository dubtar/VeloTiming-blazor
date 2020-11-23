using System;
using System.ComponentModel.DataAnnotations;

namespace VeloTiming.Server.Data
{
	public class Race
	{
		public int Id { get; set; }
		[Required, MaxLength(50)]
		public string Name { get; set; }
		public DateTime Date { get; set; }
		public string Description { get; set; }
	}
}
