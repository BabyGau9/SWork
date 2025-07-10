
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;


namespace SWork.Data.Entities
{
    public class Review
    {
        [Key]
        public int Review_id { get; set; }
        public string Reviewer_id { get; set; }
        public string Reviewee_id { get; set; }
        public int? ApplicationID { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public bool IsOpen{ get; set; } = true; 
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("Reviewer_id")]
        public ApplicationUser Reviewer { get; set; }
        [ForeignKey("Reviewee_id")]
        public ApplicationUser Reviewee { get; set; }
        [ForeignKey("ApplicationID")]
        public virtual Application Application { get; set; }
    }
}
