namespace SWork.ServiceContract.Interfaces
{
    public interface INotificationHub
    {
        Task SendNotificationAsync(string userId, string title, string message);
    }
} 