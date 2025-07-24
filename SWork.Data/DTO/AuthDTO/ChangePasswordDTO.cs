using System.ComponentModel.DataAnnotations;

namespace SWork.Data.DTO.AuthDTO
{
    public class ChangePasswordDTO
    {
        [Required]
        public string OldPassword { get; set; }
        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string NewPassword { get; set; }
    }
} 