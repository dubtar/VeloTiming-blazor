using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace VeloTiming.Hubs
{
    public class RfidHub : Hub
    {
        public async Task RfidFound(string rfidId)
        {
            await Clients.All.SendAsync("RfidFound", rfidId);
        }
    }
}