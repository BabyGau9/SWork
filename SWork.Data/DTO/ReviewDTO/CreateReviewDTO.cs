namespace SWork.Data.DTO.ReviewDTO
{
    public class CreateReviewDTO
    {
        public int ApplicationId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
    }
} 