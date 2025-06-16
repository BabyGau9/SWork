using SWork.Data.DTO.CVDTO;

namespace SWork.ServiceContract.Interfaces
{
    public interface IResumeService
    {
        Task<Pagination<Resume>> GetPaginatedResumeAsync(
           int pageIndex,
           int pageSize,
           Expression<Func<Resume, bool>>? predicate = null,
           Expression<Func<Resume, object>>? orderBy = null,
           bool isDescending = false);
        Task<Resume> GetResumeByIdAsync(int resumId);
        Task<Resume> CreateResumeAsync(CreateResumeDTO resumDto, string userID);
        Task<Resume> UpdateResumeAsync(int id,UpdateResumeDTO resum, string userID);
        Task DeleteResumeAsync(int resumId, string userID);

        Task<Pagination<Resume>> SearchResumeAsync(string? nameResume, int? studentId, int pageIndex, int pageSize);
    }

}
