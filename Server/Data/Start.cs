using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace VeloTiming.Server.Data
{
	public class Start
	{
		public int Id { get; set; }
		public string Name { get; set; } = "";
		public DateTime? PlannedStart { get; set; }
		public DateTime? RealStart { get; set; }
		public DateTime? End { get; set; }
		public int RaceId { get; set; }
		[ForeignKey("RaceId")]
		public virtual Race Race { get; set; } = null!;
		public virtual ICollection<StartCategory> Categories { get; set; } = null!;
		public bool IsActive { get; set; }
		public int DelayMarksAfterStartMinutes { get; set; }
		public StartType Type { get; set; }
	}


	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum StartType
	{
		[Description("Групповая кругами")]
		Laps = 1,
		[Description("Разделка")]
		TimeTrial = 2
		// Criterium
	}

	public class StartCategory
	{
		public int Id { get; set; }
		public virtual RaceCategory Category { get; set; } = null!;
		public virtual Start Start { get; set; } = null!;
	}
}