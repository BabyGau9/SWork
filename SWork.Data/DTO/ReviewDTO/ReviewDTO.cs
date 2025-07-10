using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWork.Data.DTO.ReviewDTO
{
    public class ReviewDTO
    {
        public int Id { get; set; }
        public string ReviewerId { get; set; }
        public string RevieweeId { get; set; }
        public int Rating { get; set; } // Số sao
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ApplicationID { get; set; }
    }
}
