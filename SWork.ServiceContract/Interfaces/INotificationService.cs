using SWork.Data.DTO.NotificationDTO;

namespace SWork.ServiceContract.Interfaces
{
    public interface INotificationService
    {
        Task<NotificationResponseDTO> CreateNotificationAsync(CreateNotificationDTO dto);
        Task<IEnumerable<NotificationResponseDTO>> GetUserNotificationsAsync(string userId);
        Task<NotificationResponseDTO> MarkAsReadAsync(int notificationId);
        Task<bool> MarkAllAsReadAsync(string userId);
        Task<int> GetUnreadCountAsync(string userId);
        Task<bool> DeleteNotificationAsync(int notificationId);
        Task SendRealTimeNotificationAsync(string userId, string title, string message);
    }
} 