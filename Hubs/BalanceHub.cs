using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace TechMobileBE.Hubs
{
    public class BalanceHub : Hub
    {
        private readonly ILogger<BalanceHub> _logger;

        public BalanceHub(ILogger<BalanceHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            if (exception != null)
            {
                _logger.LogError(exception, "Client disconnected with error: {ConnectionId}", Context.ConnectionId);
            }
            else
            {
                _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task<string> SendBalanceData(string balanceData)
        {
            _logger.LogInformation("Sending balance data from {ConnectionId}: {BalanceData}", Context.ConnectionId, balanceData);
            await Clients.All.SendAsync("ReceiveBalanceData", balanceData);
            return "Data sent successfully";
        }
    }
}
