using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace SWork.Data.Entities
{
    public class Job
    {
        [Key]
        public int JobID { get; set; }  
        public int EmployerID { get; set; }
        public int? SubscriptionID { get; set; }  
        public string Category { get; set; }   
        public string Title { get; set; } 
        public string Description { get; set; }  
        public string Requirements { get; set; }  
        public string Location { get; set; } 
        [Precision(18, 2)]
        public decimal Salary { get; set; } 
        public string WorkingHours { get; set; }  
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; } = "ACTIVE";
        public string? ImageUrl { get; set; }   // setup image null
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("EmployerID")]
        public virtual Employer Employer { get; set; }
        [ForeignKey("SubscriptionID")]
        public virtual Subscription Subscription { get; set; }

        public virtual ICollection<Application> Applications { get; set; }
        public virtual ICollection<JobBookmark> JobBookmarks { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }
    }
}
