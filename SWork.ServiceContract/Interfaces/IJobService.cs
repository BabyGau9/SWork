namespace SWork.ServiceContract.Interfaces
{
    public interface IJobService
    {
        Task<Pagination<Job>> GetPaginatedJobAsync(
           int pageIndex,
           int pageSize,
           Expression<Func<Job, bool>>? predicate = null,
           Expression<Func<Job, object>>? orderBy = null,
           bool isDescending = false);

        Task<Job> GetJobByIdAsync(int jobId);
        Task CreateJobAsync(CreateJobDTO jobDto, string userId);
        Task UpdateJobAsync(int jobId, UpdateJobDTO jobdto, string userId);
        Task DeleteJobAsync(int jobId, string userId);
        Task<Pagination<JobSearchResponseDTO>> GetActiveJobDtosAsync(int pageIndex, int pageSize);
        Task<Pagination<JobSearchResponseDTO>> GetJobByIdDtosAsync(string userId, int pageIndex, int pageSize);
        Task<Pagination<JobSearchResponseDTO>> GetJobMarkByIdAsync(string userId, int pageIndex, int pageSize);
        Task<Pagination<Job>> GetJobsByEmployerIdAsync(string userId, int pageIndex, int pageSize);
    }
}
