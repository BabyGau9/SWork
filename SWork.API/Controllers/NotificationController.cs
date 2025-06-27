using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SWork.Data.DTO.NotificationDTO;
using SWork.ServiceContract.Interfaces;
using System.Security.Claims;

namespace SWork.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetUserNotifications()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var notifications = await _notificationService.GetUserNotificationsAsync(userId);
            return Ok(notifications);
        }

        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var count = await _notificationService.GetUnreadCountAsync(userId);
            return Ok(new { unreadCount = count });
        }

        [HttpPost("mark-as-read/{notificationId}")]
        public async Task<IActionResult> MarkAsRead(int notificationId)
        {
            var notification = await _notificationService.MarkAsReadAsync(notificationId);
            return Ok(notification);
        }

        [HttpPost("mark-all-as-read")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _notificationService.MarkAllAsReadAsync(userId);
            return Ok(new { success = result });
        }

        [HttpDelete("{notificationId}")]
        public async Task<IActionResult> DeleteNotification(int notificationId)
        {
            var result = await _notificationService.DeleteNotificationAsync(notificationId);
            return Ok(new { success = result });
        }

        [HttpPost("send")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SendNotification([FromBody] CreateNotificationDTO dto)
        {
            var notification = await _notificationService.CreateNotificationAsync(dto);
            return Ok(notification);
        }
    }
} 