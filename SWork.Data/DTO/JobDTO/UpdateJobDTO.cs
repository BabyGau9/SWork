using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace SWork.Data.DTO.JobDTO
{
    public class UpdateJobDTO
    {
        [StringLength(200)]
        public string? Title { get; set; }

        [StringLength(2000)]
        public string? Description { get; set; }

        [StringLength(1000)]
        public string? Requirements { get; set; }

        [StringLength(500)]
        public string? Location { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? Salary { get; set; }

        [RegularExpression("^(ACTIVE|INACTIVE)$")]
        public string? Status { get; set; }

        [StringLength(100)]
        public string? WorkingHours { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public int? EmployerID { get; set; }
        public int? SubscriptionID { get; set; }
        
        [StringLength(100)]
        public string? Category { get; set; }

        public IFormFile? Image { get; set; }
    }
}
