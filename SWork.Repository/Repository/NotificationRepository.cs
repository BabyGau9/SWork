using SWork.Data.Entities;
using SWork.RepositoryContract.Interfaces;
using SWork.Repository.Basic;
using System.Linq;

namespace SWork.Repository.Repository
{
    public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
    {
        public NotificationRepository(SWorkDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Notification>> GetUserNotificationsAsync(string userId)
        {
            var notifications = await GetAllAsync(n => n.UserID == userId);
            return notifications.OrderByDescending(n => n.CreatedAt);
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            var notifications = await GetAllAsync(n => n.UserID == userId && !n.IsRead);
            return notifications.Count;
        }

        public async Task<bool> MarkAsReadAsync(int notificationId)
        {
            var notification = await GetByIdAsync(notificationId);
            if (notification == null)
                return false;

            notification.IsRead = true;
            Update(notification);
            return true;
        }

        public async Task<bool> MarkAllAsReadAsync(string userId)
        {
            var notifications = await GetAllAsync(n => n.UserID == userId && !n.IsRead);
            foreach (var notification in notifications)
            {
                notification.IsRead = true;
                Update(notification);
            }
            return true;
        }
    }
} 