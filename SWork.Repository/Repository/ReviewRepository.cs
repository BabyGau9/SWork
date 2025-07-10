
using Microsoft.EntityFrameworkCore;
using SWork.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SWork.Repository.Repository
{
    public class ReviewRepository(SWorkDbContext context) : GenericRepository<Review>(context), IReviewRepository
    {
        public async Task<IEnumerable<Review>> GetAllAsync()
        {
            return await _context.Set<Review>()
                .Include(r => r.Reviewer)
                .Include(r => r.Application)
                .ToListAsync();
        }

        public async Task<IEnumerable<Review>> GetReviewsByUserIdAsync(string userId)
        {
            return await _context.Set<Review>()
                .Include(r => r.Reviewer)
                .Include(r => r.Application)
                .Where(r => r.Reviewee_id == userId)
                .ToListAsync();
        }
    }
}
