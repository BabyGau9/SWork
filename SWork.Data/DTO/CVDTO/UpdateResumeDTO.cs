
using System.ComponentModel.DataAnnotations;

namespace SWork.Data.DTO.CVDTO
{
    public class UpdateResumeDTO
    {

        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string FullName { get; set; }

        [Required]
        [StringLength(60, MinimumLength = 2)]
        public string JobTitle { get; set; }

        public string? FileURL { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }

        [Required]
        [RegularExpression(@"^\+?\d{8,15}$")]
        public string PhoneNumber { get; set; }


        [StringLength(200)]
        public string Address { get; set; }

        [StringLength(1000)]
        public string Introduction { get; set; }

        [StringLength(2000)]
        public string Education { get; set; }

        [StringLength(2000)]
        public string Experience { get; set; }

        [StringLength(2000)]
        public string Skills { get; set; }

        [StringLength(2000)]
        public string Languages { get; set; }

        [StringLength(2000)]
        public string Awards { get; set; }

        [StringLength(2000)]
        public string Certificates { get; set; }
        public DateTime UpdatedAt { get; set; } =  DateTime.Now;
    }
}
