using System.ComponentModel.DataAnnotations;

namespace SWork.Data.DTO.NotificationDTO
{
    public class NotificationDTO
    {
        public int NotificationID { get; set; }
        public string UserID { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateNotificationDTO
    {
        [Required]
        public string UserID { get; set; }
        
        [Required]
        public string Title { get; set; }
        
        [Required]
        public string Message { get; set; }
    }

    public class NotificationResponseDTO
    {
        public int NotificationID { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class MarkAsReadDTO
    {
        public int NotificationID { get; set; }
    }
} 