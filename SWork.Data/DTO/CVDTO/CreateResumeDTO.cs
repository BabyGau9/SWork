

using System.ComponentModel.DataAnnotations;

namespace SWork.Data.DTO.CVDTO
{
    public class CreateResumeDTO
    {
        public int StudentID { get; set; }
        public int? TemplateID { get; set; }
        public bool IsDefault { get; set; } = false;

        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string FullName { get; set; }

        public string? ResumeType { get; set; }
        public string? FileURL { get; set; }

        [Required]
        [StringLength(60, MinimumLength = 2)]
        public string JobTitle { get; set; }

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
        public DateTime CreatedAt { get; set; } = DateTime.Now;

    }
}
