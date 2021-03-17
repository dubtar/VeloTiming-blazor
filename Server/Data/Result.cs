using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace VeloTiming.Server.Data
{
	public sealed class Result
	{
		public string Id { get; set; } = Guid.NewGuid().ToString();
		public DateTime? Time { get; set; }
		public string? TimeSource { get; set; }
		public string? Name { get; set; }
		public string? Number { get; set; }
		public string? NumberSource { get; set; }
		public bool IsIgnored { get; set; }
		public IList<MarkData> Data { get; set; } = new List<MarkData>();
		public DateTime CreatedOn { get; private set; }
		public int Lap { get; set; }
		public int Place { get; set; }

		[ForeignKey("Start")]
		public int StartId { get; set; }
		public Start Start { get; set; } = null!;

		internal static Result Create(ITimeService timeService, int startId)
		{
			return new Result { CreatedOn = timeService.Now, StartId = startId };
		}
	}

	public class MarkData
	{
		public DateTime? Time { get; set; }
		public string? Number { get; set; }
		public string Source { get; set; } = "";
		public DateTime CreatedOn { get; set; }

		public override bool Equals(object? obj)
		{
			if (obj != null && obj is MarkData b)
			{
				return Time == b.Time && Number == b.Number && Source == b.Source && b.CreatedOn == CreatedOn;
			}
			return false;
		}
		public override int GetHashCode()
		{
			return HashCode.Combine(Time, Number, Source, CreatedOn);
		}

		internal MarkData Copy()
		{
			return new MarkData { Time = Time, Number = Number, Source = Source, CreatedOn = CreatedOn };
		}
	}
}
