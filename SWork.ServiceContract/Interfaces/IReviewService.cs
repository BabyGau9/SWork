using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SWork.Data.DTO.ReviewDTO;

namespace SWork.ServiceContract.Interfaces
{
    public interface IReviewService
    {
        Task<IEnumerable<ReviewDTO>> GetAllAsync();
        Task<ReviewDTO> GetByIdAsync(int id);
        Task<IEnumerable<ReviewDTO>> GetReviewsByUserIdAsync(string userId);
        Task<IEnumerable<ReviewDTO>> GetReviewsByJobIdAsync(int jobId);
        Task<ReviewDTO> CreateReviewAsync(CreateReviewDTO dto, string reviewerId);
        Task<ReviewDTO> UpdateReviewAsync(UpdateReviewDTO dto, string reviewerId);
        Task<bool> DeleteReviewAsync(int id, string reviewerId);
    }
}
