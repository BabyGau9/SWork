
namespace SWork.Data.DTO.JobDTO
{
    public class JobSearchResponseDTO
    {
        public int JobID { get; set; }
        public int? SubscriptionID { get; set; }
        public string Category { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Requirements { get; set; }
        public string Location { get; set; }
        public decimal Salary { get; set; }
        public string WorkingHours { get; set; }
    }
}
