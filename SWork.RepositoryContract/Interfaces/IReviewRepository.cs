using SWork.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SWork.RepositoryContract.Interfaces
{
    public interface  IReviewRepository : IGenericRepository<Review>
    {
        Task<IEnumerable<Review>> GetAllAsync();
        Task<IEnumerable<Review>> GetReviewsByUserIdAsync(string userId);
    }
}
