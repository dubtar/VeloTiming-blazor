using System;

namespace VeloTiming.Server.Data
{
	public interface ITimeService
	{
		DateTime Now { get; }

	}

	public class TimeService : ITimeService
	{
		public DateTime Now => DateTime.UtcNow;
	}
}