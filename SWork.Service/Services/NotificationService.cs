using SWork.Data.DTO.NotificationDTO;
using SWork.Data.Entities;
using SWork.RepositoryContract.Interfaces;
using SWork.ServiceContract.Interfaces;
using SWork.RepositoryContract.IUnitOfWork;
using AutoMapper;

namespace SWork.Service.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationRepository _notificationRepository;
        private readonly IMapper _mapper;
        private readonly INotificationHub _notificationHub;

        public NotificationService(
            IUnitOfWork unitOfWork,
            INotificationRepository notificationRepository,
            IMapper mapper,
            INotificationHub notificationHub)
        {
            _unitOfWork = unitOfWork;
            _notificationRepository = notificationRepository;
            _mapper = mapper;
            _notificationHub = notificationHub;
        }

        public async Task<NotificationResponseDTO> CreateNotificationAsync(CreateNotificationDTO dto)
        {
            var notification = new Notification
            {
                UserID = dto.UserID,
                Title = dto.Title,
                Message = dto.Message,
                IsRead = false,
                CreatedAt = DateTime.Now
            };

            await _notificationRepository.InsertAsync(notification);
            await _unitOfWork.SaveChangeAsync();

            // Gửi real-time notification qua SignalR
            await SendRealTimeNotificationAsync(dto.UserID, dto.Title, dto.Message);

            return _mapper.Map<NotificationResponseDTO>(notification);
        }

        public async Task<IEnumerable<NotificationResponseDTO>> GetUserNotificationsAsync(string userId)
        {
            var notifications = await _notificationRepository.GetUserNotificationsAsync(userId);
            return _mapper.Map<IEnumerable<NotificationResponseDTO>>(notifications);
        }

        public async Task<NotificationResponseDTO> MarkAsReadAsync(int notificationId)
        {
            var success = await _notificationRepository.MarkAsReadAsync(notificationId);
            if (!success)
                throw new Exception("Không tìm thấy notification");

            await _unitOfWork.SaveChangeAsync();

            var notification = await _notificationRepository.GetByIdAsync(notificationId);
            return _mapper.Map<NotificationResponseDTO>(notification);
        }

        public async Task<bool> MarkAllAsReadAsync(string userId)
        {
            var result = await _notificationRepository.MarkAllAsReadAsync(userId);
            await _unitOfWork.SaveChangeAsync();
            return result;
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await _notificationRepository.GetUnreadCountAsync(userId);
        }

        public async Task<bool> DeleteNotificationAsync(int notificationId)
        {
            var notification = await _notificationRepository.GetByIdAsync(notificationId);
            if (notification == null)
                return false;

             _notificationRepository.Delete(notification);
            await _unitOfWork.SaveChangeAsync();
            return true;
        }

        public async Task SendRealTimeNotificationAsync(string userId, string title, string message)
        {
            await _notificationHub.SendNotificationAsync(userId, title, message);
        }
    }
} 