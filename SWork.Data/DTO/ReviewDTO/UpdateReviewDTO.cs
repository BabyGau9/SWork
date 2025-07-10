using System.ComponentModel.DataAnnotations;

namespace SWork.Data.DTO.ReviewDTO
{
    public class UpdateReviewDTO
    {
        public int Id { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
    }
} 