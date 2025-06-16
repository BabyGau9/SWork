
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SWork.Data.Entities
{
    public class Resume
    {
        [Key]
        public int ResumeID { get; set; }
        public int StudentID { get; set; }
        public string? ResumeType { get; set; } // 'UPLOADED', 'CREATED'
        public string? FileURL { get; set; }
        public bool IsDefault { get; set; } = false;
        public string FullName { get; set; }
        public string JobTitle { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string Introduction { get; set; }
        public string Education { get; set; }
        public string Experience { get; set; }
        public string Skills { get; set; }
        public string Languages { get; set; }
        public string Awards { get; set; }
        public string Certificates { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("StudentID")]
        [JsonIgnore]
        public virtual Student Student { get; set; }
        public virtual ICollection<Application> Applications { get; set; }
    }
}
