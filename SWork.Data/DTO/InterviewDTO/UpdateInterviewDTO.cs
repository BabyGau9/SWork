using System.ComponentModel.DataAnnotations;
using SWork.Data.Enum;

namespace SWork.Data.DTO.InterviewDTO
{
    public class UpdateInterviewDTO
    {
        [Required]
        public InterviewStatus NewStatus { get; set; }
    }
}
