using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VeloTiming.Server.Data
{
	public class Result
	{
		public string Id { get; set; } = Guid.NewGuid().ToString();
		public DateTime? Time { get; set; }
		public string? TimeSource { get; set; }
		public string? Name { get; set; }
		public string? Number { get; set; }
		public string? NumberSource { get; set; }
		public bool IsIgnored { get; set; }
		public DateTime CreatedOn { get; private set; }
		public int Lap { get; set; }
		public int Place { get; set; }

		[ForeignKey("Start")]
		public int StartId { get; set; }
		public virtual Start Start { get; set; } = null!;

		internal static Result Create(ITimeService timeService, int startId)
		{
			return new Result { CreatedOn = timeService.Now, StartId = startId };
		}
	}
}
