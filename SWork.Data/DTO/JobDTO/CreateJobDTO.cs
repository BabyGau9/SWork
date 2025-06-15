using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SWork.Data.DTO.JobDTO
{
    public class CreateJobDTO
    {
        [JsonIgnore]
        public int EmployerID { get; set; }

        [Required]
        public int SubscriptionID { get; set; }

        [Required(ErrorMessage = "Category is required.")]
        [StringLength(100, ErrorMessage = "Category cannot exceed 100 characters.")]
        public string Category { get; set; }

        [Required(ErrorMessage = "Job title is required.")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
        public string Title { get; set; }

        [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters.")]
        public string? Description { get; set; }

        [StringLength(1000, ErrorMessage = "Requirements cannot exceed 1000 characters.")]
        public string? Requirements { get; set; }

        [Required(ErrorMessage = "Location is required.")]
        [StringLength(500, ErrorMessage = "Location cannot exceed 500 characters.")]
        public string Location { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Salary must be a positive value.")]
        public decimal Salary { get; set; }

        [Required]
        [RegularExpression("^(Active|InActive)$", ErrorMessage = "Status must be either 'Active' or 'InActive'.")]
        public string Status { get; set; } = "InActive";

        [StringLength(100, ErrorMessage = "Working hours cannot exceed 100 characters.")]
        public string? WorkingHours { get; set; }
        public DateTime? StartDate { get; set; } = null;
        public DateTime? EndDate { get; set; } = null;
        // setup image null
        public IFormFile? Image { get; set; }
    }
}
