using Microsoft.AspNetCore.SignalR;
using SWork.ServiceContract.Interfaces;
using SWork.API.Hubs;

namespace SWork.API.Services
{
    public class NotificationHubService : INotificationHub
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationHubService(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendNotificationAsync(string userId, string title, string message)
        {
            var notificationData = new
            {
                title = title,
                message = message,
                timestamp = DateTime.Now
            };

            await _hubContext.Clients.Group(userId).SendAsync("ReceiveNotification", notificationData);
        }
    }
} 