using Microsoft.AspNetCore.SignalR;

namespace SmartLogAnalyzer.Core.Hubs
{
    public class ErrorHub : Hub
    {
        public async Task SendErrorUpdate(string errorJson)
        {
            await Clients.All.SendAsync("ReceiveErrorUpdate", errorJson);
        }
    }
}