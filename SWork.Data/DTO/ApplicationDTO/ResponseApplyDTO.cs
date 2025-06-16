

using System.ComponentModel.DataAnnotations;

namespace SWork.Data.DTO.ApplicationDTO
{
    public class ResponseApplyDTO
    {
        [Required]
        public int STT { get; set; }

        [Required]
        public string StudentName { get; set; }
        public int? JobID { get; set; }

        [Required]
        public string JobName { get; set; }

        public int? ResumeID { get; set; }

        [Required]
        public string Coverletter { get; set; }
        [Required]
        public string Status { get; set; } 
        [Required]
        public DateTime AppliedAt { get; set; } 

        public DateTime? UpdateAt { get; set; }
    }
}
