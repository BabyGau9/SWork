using Microsoft.AspNetCore.SignalR;
using SWork.Data.DTO.NotificationDTO;
using SWork.RepositoryContract.Interfaces;
using System.Security.Claims;
using System.Linq;

namespace SWork.API.Hubs
{
    public class NotificationHub : Hub
    {
        private static readonly Dictionary<string, string> _userConnections = new();
        private readonly INotificationRepository _notificationRepository;

        public NotificationHub(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                _userConnections[userId] = Context.ConnectionId;
                await Groups.AddToGroupAsync(Context.ConnectionId, userId);

                // Lấy danh sách notification chưa đọc
                var unreadNotifications = await _notificationRepository.GetUserNotificationsAsync(userId);
                var unreadList = unreadNotifications.Where(n => !n.IsRead)
                    .Select(n => new
                    {
                        n.NotificationID,
                        n.Title,
                        n.Message,
                        n.CreatedAt,
                        n.IsRead
                    }).ToList();

                // Gửi về client qua sự kiện UnreadNotifications
                await Clients.Caller.SendAsync("UnreadNotifications", unreadList);
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId) && _userConnections.ContainsKey(userId))
            {
                _userConnections.Remove(userId);
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinUserGroup(string userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);
        }

        public async Task LeaveUserGroup(string userId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
        }
    }
} 