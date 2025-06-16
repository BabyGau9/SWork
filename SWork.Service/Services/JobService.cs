using SWork.Data.DTO.JobDTO;
using SWork.ServiceContract.ICloudinaryService;

namespace SWork.Service.Services
{
    public class JobService(IUnitOfWork unitOfWork, IMapper mapper, ICloudinaryImageService cloudinaryImageService, IWalletService walletService) : IJobService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;
        private readonly ICloudinaryImageService _cloudinaryImageService = cloudinaryImageService;
        private readonly IWalletService _walletService = walletService;
        public async Task<Pagination<Job>> GetPaginatedJobAsync(
            int pageIndex,
            int pageSize,
            Expression<Func<Job, bool>>? predicate = null,
            Expression<Func<Job, object>>? orderBy = null,
            bool isDescending = false)
        {
            try
            {
                var result = await _unitOfWork.GenericRepository<Job>().GetPaginationAsync(
                    predicate = predicate,
                    includeProperties: "Subscription",
                    pageIndex: pageIndex,
                    pageSize: pageSize,
                    orderBy: orderBy ?? (p => p.JobID),
                    isDescending: isDescending);

                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task CreateJobAsync(CreateJobDTO jobDto, string userId)
        {
            var subscription = await _unitOfWork.GenericRepository<Subscription>().GetFirstOrDefaultAsync(a => a.SubscriptionID == jobDto.SubscriptionID);
            if (subscription == null) throw new Exception("Gói bài viết không tồn tại.Vui lòng chọn lại!");

            var employer = await _unitOfWork.GenericRepository<Employer>().GetFirstOrDefaultAsync(a => a.UserID == userId);
            if (employer == null) throw new Exception("Bạn không có quyền tạo mới công việc.");

            var wallet = await _unitOfWork.GenericRepository<Wallet>().GetFirstOrDefaultAsync(a => a.UserID == userId);
            if (wallet == null) throw new Exception("Ví không tồn tại.");


            jobDto.EmployerID = employer.EmployerID;
            var job = _mapper.Map<Job>(jobDto);
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                if (jobDto.Image != null)
                {
                    string imageUrl = await _cloudinaryImageService.UploadImageAsync(jobDto.Image, "job-images");
                    job.ImageUrl = imageUrl;
                }
                // so sánh wallet.balana >= subscription.price
                if (wallet.Balance < subscription.Price)
                {
                    throw new Exception("Số dư trong ví không đủ. Vui lòng quét mã để đăng bài");
                }
                else if (wallet.Balance >= subscription.Price)
                {
                    var description = $"- {subscription.Price}đ cho đăng bài tuyển dụng với gói '{subscription.SubscriptionName}'";
                    await _walletService.DeductFromWalletAsync(userId, subscription.Price, "", "Success");
                    job.Status = "ACTIVE";
                    await _unitOfWork.GenericRepository<Job>().InsertAsync(job);
                    await _unitOfWork.SaveChangeAsync();
                    await _unitOfWork.CommitTransactionAsync();
                }
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
        public async Task UpdateJobAsync(int jobId, UpdateJobDTO jobdto, string userId)
        {

            var subscription = await _unitOfWork.GenericRepository<Subscription>().GetFirstOrDefaultAsync(a => a.SubscriptionID == jobdto.SubscriptionID);
            if (subscription == null) throw new Exception("Gói bài viết không tồn tại.Vui lòng chọn lại!");

            var job = await _unitOfWork.GenericRepository<Job>().GetFirstOrDefaultAsync(a => a.JobID == jobId);
            if (job == null) throw new Exception("Bài viết không tồn tại.");

            if (job.Status == "IsActive") throw new Exception("Bài viết đã hết hạn.");

            var employer = await _unitOfWork.GenericRepository<Employer>().GetFirstOrDefaultAsync(a => a.UserID == userId);

            if (job.EmployerID != employer.EmployerID) throw new Exception("Bạn không có quyền chỉnh sửa bài viết này.");


            await _unitOfWork.BeginTransactionAsync();

            try
            {
                // update properties 
                if (!string.IsNullOrWhiteSpace(jobdto.Title)) job.Title = jobdto.Title;
                if (!string.IsNullOrWhiteSpace(jobdto.Description)) job.Description = jobdto.Description;
                if (!string.IsNullOrWhiteSpace(jobdto.Requirements)) job.Requirements = jobdto.Requirements;
                if (!string.IsNullOrWhiteSpace(jobdto.Location)) job.Location = jobdto.Location;
                if (jobdto.Salary.HasValue) job.Salary = jobdto.Salary.Value;
                if (!string.IsNullOrWhiteSpace(jobdto.Status)) job.Status = jobdto.Status;
                if (!string.IsNullOrWhiteSpace(jobdto.WorkingHours)) job.WorkingHours = jobdto.WorkingHours;
                if (jobdto.StartDate.HasValue) job.StartDate = jobdto.StartDate.Value;
                if (jobdto.EndDate.HasValue) job.EndDate = jobdto.EndDate.Value;
                if (jobdto.SubscriptionID.HasValue) job.SubscriptionID = jobdto.SubscriptionID.Value;
                //update image
                if (jobdto.Image != null)
                {
                    if (!string.IsNullOrEmpty(job.ImageUrl))
                    {
                        string publicId = _cloudinaryImageService.ExtractPublicIdFromUrl(job.ImageUrl);
                        //  Console.WriteLine("Extracted publicId: " + publicId);
                        await _cloudinaryImageService.DeleteImageAsync(publicId);
                    }
                    string imageUrl = await _cloudinaryImageService.UploadImageAsync(jobdto.Image, "job-images");
                    job.ImageUrl = imageUrl;
                }
                _unitOfWork.GenericRepository<Job>().Update(job);
                await _unitOfWork.SaveChangeAsync();
                await _unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
        public async Task UpdateJobStatusAsync(int jobId, string status)
        {
            var job = await _unitOfWork.GenericRepository<Job>().GetFirstOrDefaultAsync(a => a.JobID == jobId);
            if (job == null) throw new Exception("Bài viết không tồn tại.");

            if (status == null) throw new Exception("Trạng thái không được hỗ trợ.");

            var validStatuses = new[] { "ACTIVE", "PENDINGPAYMENT", "EXPIRED", "DELETED" };
            if (!validStatuses.Contains(status))
                throw new Exception("Trạng thái không được hỗ trợ.");

            job.Status = status;
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                _unitOfWork.GenericRepository<Job>().Update(job);
                await _unitOfWork.SaveChangeAsync();
                await _unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
           
        }
        public async Task DeleteJobAsync(int jobId, string userId)
        {
            var job = await _unitOfWork.GenericRepository<Job>().GetFirstOrDefaultAsync(a => a.JobID == jobId);
            if (job == null) throw new Exception("Bài viết không tồn tại.");

            var employer = await _unitOfWork.GenericRepository<Employer>().GetFirstOrDefaultAsync(a => a.UserID == userId);
            if (job.EmployerID != employer.EmployerID) throw new Exception("Bạn không có xóa bài viết này.");

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                if (!string.IsNullOrEmpty(job.ImageUrl))
                {
                    string publicId = _cloudinaryImageService.ExtractPublicIdFromUrl(job.ImageUrl);
                    await _cloudinaryImageService.DeleteImageAsync(publicId);
                }

                _unitOfWork.GenericRepository<Job>().Delete(job);
                await _unitOfWork.SaveChangeAsync();
                await _unitOfWork.CommitTransactionAsync();
                //Hub
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
        public async Task<Job> GetJobByIdAsync(int jobId)
        {
            var job = await _unitOfWork.GenericRepository<Job>().GetByIdAsync(jobId);
            if (job == null)
                throw new Exception("Job not found");
            return job;
        }
        public async Task<Pagination<Job>> SearchJobAsync(JobSearchRequestDTO filter, int jobCategory, int pageIndex, int pageSize)
        {
            Expression<Func<Job, bool>> predicate = job =>
            (string.IsNullOrEmpty(filter.Keyword) ||
            job.Title.Contains(filter.Keyword) ||
            job.Description.Contains(filter.Keyword) ||
            job.Requirements.Contains(filter.Keyword)) &&
            (string.IsNullOrEmpty(filter.Location) || job.Location.Contains(filter.Location)) &&
            (!filter.MinSalary.HasValue || job.Salary >= filter.MinSalary.Value) &&
            (!filter.MaxSalary.HasValue || job.Salary <= filter.MaxSalary.Value);

            var result = await _unitOfWork.GenericRepository<Job>().GetPaginationAsync(
                predicate: predicate,
                includeProperties: " Subscription",
                pageIndex: pageIndex,
                pageSize: pageSize,
                orderBy: job => new
                {
                    Type = job.Subscription.SubscriptionName,
                    CreateAt = job.CreatedAt
                },
                isDescending: true
           );
            return result;
        }
        public async Task<Pagination<JobSearchResponseDTO>> GetActiveJobDtosAsync(int pageIndex, int pageSize)
        {
            // B1: Lọc theo Status = "Active"
            Expression<Func<Job, bool>> predicate = job => job.Status == "Active";

            // B2: Gọi hàm gốc lấy danh sách Job (pagination)
            var paginatedJobs = await GetPaginatedJobAsync(
                pageIndex,
                pageSize,
                predicate,
                orderBy: j => j.JobID,
                isDescending: true
            );

            // B3: Chuyển đổi từ Job -> JobSearchResponseDTO
            var dtoList = paginatedJobs.Items.Select(job => new JobSearchResponseDTO
            {
                SubscriptionID = job.JobID,
                Category = job.Title,
                Title = job.Location,
                Description = job.Description,
                Requirements = job.Requirements,
                Location = job.Location,
                Salary = job.Salary,
                WorkingHours = job.WorkingHours
            }).ToList();

            // B4: Trả về Pagination của DTO
            return new Pagination<JobSearchResponseDTO>
            {
                Items = dtoList,
                TotalItemsCount = paginatedJobs.TotalItemsCount,
                PageIndex = paginatedJobs.PageIndex,
                PageSize = paginatedJobs.PageSize
            };
        }
        public async Task<Pagination<JobSearchResponseDTO>> GetJobByIdDtosAsync(string userId, int pageIndex, int pageSize)
        {
            var employer = await _unitOfWork.GenericRepository<Employer>().GetFirstOrDefaultAsync(a => a.UserID == userId);
            if (employer == null) throw new Exception("Bạn cần đăng nhập hoặc tạo tài khoản trước khi xem danh sách.");

            Expression<Func<Job, bool>> predicate = job => job.EmployerID == employer.EmployerID;

            var paginatedJobs = await GetPaginatedJobAsync(
                pageIndex,
                pageSize,
                predicate,
                orderBy: j => j.JobID,
                isDescending: true
            );

            var dtoList = paginatedJobs.Items.Select(job => new JobSearchResponseDTO
            {
                SubscriptionID = job.JobID,
                Category = job.Title,
                Title = job.Location,
                Description = job.Description,
                Requirements = job.Requirements,
                Location = job.Location,
                Salary = job.Salary,
                WorkingHours = job.WorkingHours
            }).ToList();

            return new Pagination<JobSearchResponseDTO>
            {
                Items = dtoList,
                TotalItemsCount = paginatedJobs.TotalItemsCount,
                PageIndex = paginatedJobs.PageIndex,
                PageSize = paginatedJobs.PageSize
            };
        }
        public async Task<Pagination<JobSearchResponseDTO>> GetJobMarkByIdAsync(string userId, int pageIndex, int pageSize)
        {
            // 1. Lấy thông tin student từ userId
            var student = await _unitOfWork.GenericRepository<Student>().GetFirstOrDefaultAsync(s => s.UserID == userId);
            if (student == null) throw new Exception("Bạn cần đăng nhập hoặc tạo tài khoản trước khi xem danh sách.");

            // 2. Lấy danh sách JobID đã bookmark bởi student
            var bookmarks = await _unitOfWork.GenericRepository<JobBookmark>().GetAllAsync(b => b.StudentID == student.StudentID, null);

            var bookmarkedJobIds = bookmarks.Select(b => b.JobID).ToList();

            // 3. Lọc danh sách Job theo danh sách JobID đã bookmark
            Expression<Func<Job, bool>> predicate = job => bookmarkedJobIds.Contains(job.JobID);


            var paginatedJobs = await GetPaginatedJobAsync(
                pageIndex,
                pageSize,
                predicate,
                orderBy: j => j.JobID,
                isDescending: true
            );

            // 4. Map sang DTO
            var dtoList = paginatedJobs.Items.Select(job => new JobSearchResponseDTO
            {
                SubscriptionID = job.JobID,
                Category = job.Category,
                Title = job.Title,
                Description = job.Description,
                Requirements = job.Requirements,
                Location = job.Location,
                Salary = job.Salary,
                WorkingHours = job.WorkingHours
            }).ToList();

            // 5. Trả về kết quả phân trang
            return new Pagination<JobSearchResponseDTO>
            {
                Items = dtoList,
                TotalItemsCount = paginatedJobs.TotalItemsCount,
                PageIndex = paginatedJobs.PageIndex,
                PageSize = paginatedJobs.PageSize
            };
        }
        public async Task<Pagination<Job>> GetJobsByEmployerIdAsync(string userId, int pageIndex, int pageSize)
        {
            // 1. Lấy thông tin employer từ userId
            var employer = await _unitOfWork.GenericRepository<Employer>().GetFirstOrDefaultAsync(e => e.UserID == userId);
            if (employer == null) throw new Exception("Không tìm thấy thông tin nhà tuyển dụng.");

            // 2. Lọc danh sách Job theo EmployerID
            Expression<Func<Job, bool>> predicate = job => job.EmployerID == employer.EmployerID;

            // 3. Gọi hàm gốc lấy danh sách Job (pagination)
            var result = await GetPaginatedJobAsync(
                pageIndex,
                pageSize,
                predicate,
                orderBy: j => j.CreatedAt,
                isDescending: true
            );

            return result;
        }
    }
}
