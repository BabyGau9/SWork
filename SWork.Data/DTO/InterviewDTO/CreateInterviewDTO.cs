using System.ComponentModel.DataAnnotations;
using SWork.Data.Enum;

namespace SWork.Data.DTO.InterviewDTO
{
    public class CreateInterviewDTO
    {
        [Required]
        public int ApplicationID { get; set; }

        [Required]
        public DateTime ScheduledTime { get; set; }

        [Required]
        [Range(15, 480, ErrorMessage = "Duration must be between 15 and 480 minutes")]
        public int Duration_minutes { get; set; }

        [Required]
        [StringLength(200)]
        public string Location { get; set; }

        [Required]
        [StringLength(500)]
        public string MeetingLink { get; set; }

        [StringLength(1000)]
        public string Note { get; set; }
    }
} 