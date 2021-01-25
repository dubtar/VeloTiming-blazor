using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;
using VeloTiming.Server.Data;
using VeloTiming.Server.Logic;

namespace VeloTiming.Server.Hubs
{
	public interface IResultHub
	{
		Task ActiveStart(RaceInfo? race);
		Task RaceStarted(RaceInfo race);
		Task ResultAdded(Result mark);
		Task ResultUpdated(Result mark);

	}
	public class ResultHub : Hub<IResultHub>
	{
		public readonly IRaceLogic raceService;
		public ResultHub(IRaceLogic raceService)
		{
			this.raceService = raceService;
		}

		// Methods
		public void AddTime(DateTime time, string source)
		{
			raceService.AddTime(time, source);
		}

		public void AddNumber(string number, string source)
		{
			raceService.AddNumber(number, source);
		}
	}
}