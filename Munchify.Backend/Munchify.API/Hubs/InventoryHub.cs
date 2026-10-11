using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Munchify.API.Hubs
{
    public class InventoryHub : Hub
    {
        // Clients can call this to verify connection
        public async Task Ping()
        {
            await Clients.Caller.SendAsync("Pong", "SignalR Connected Successfully!");
        }
    }
}
