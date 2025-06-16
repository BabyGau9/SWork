using System.Collections.Generic;
using System.Threading.Tasks;
using SWork.Data.DTO;
using SWork.Data.Entities;

namespace SWork.RepositoryContract.Interfaces
{
    public interface IInterviewRepository : IGenericRepository<Interview>
    {
        Task<Interview> GetByIdAsync(int id);
        Task<IEnumerable<Interview>> GetAllAsync();
        Task<IEnumerable<Interview>> GetByApplicationIdAsync(int applicationId);
        Task<IEnumerable<Interview>> GetByStudentIdAsync(int studentId);
        Task<IEnumerable<Interview>> GetByEmployerIdAsync(int employerId);
        Task<Interview> AddAsync(Interview interview);
        Task<Interview> UpdateAsync(Interview interview);
        Task<bool> DeleteAsync(int id);
    }
}
