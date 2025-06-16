using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SWork.Data.Enum;

namespace SWork.Data.DTO.InterviewDTO
{
    public class InterviewResponseDTO
    {
        public int InterviewID { get; set; }
        public int ApplicationID { get; set; }
        public DateTime ScheduledTime { get; set; }
        public int Duration_minutes { get; set; }
        public string Location { get; set; }
        public string MeetingLink { get; set; }
        public string Note { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        // Additional properties for related data
        public string JobTitle { get; set; }
        public string CompanyName { get; set; }
        public string StudentName { get; set; }
    }
}
