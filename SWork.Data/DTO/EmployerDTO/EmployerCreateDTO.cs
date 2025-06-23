using System.ComponentModel.DataAnnotations;

namespace SWork.Data.DTO.EmployerDTO
{
    public class EmployerCreateDTO
    {
        [Required(ErrorMessage = "Company name is required")]
        [StringLength(100)]
        public string CompanyName { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [StringLength(100)]
        public string Website { get; set; }

        [StringLength(100)]
        public string Industry { get; set; }

        [StringLength(100)]
        public string CompanySize { get; set; }

        [StringLength(500)]
        public string? LogoUrl { get; set; }

        [StringLength(200)]
        public string Location { get; set; }
    }
}