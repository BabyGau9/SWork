using SWork.Common.Middleware;
using SWork.Data.DTO.JobDTO;
using SWork.ServiceContract.ICloudinaryService;
using SWork.ServiceContract.Interfaces;

namespace SWork.Service.Services
{
    public class JobService(IUnitOfWork unitOfWork, IMapper mapper, ICloudinaryImageService cloudinaryImageService, IWalletService walletService, INotificationService notificationService) : IJobService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;
        private readonly ICloudinaryImageService _cloudinaryImageService = cloudinaryImageService;
        private readonly IWalletService _walletService = walletService;
        private readonly INotificationService _notificationService = notificationService;
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
            if (subscription == null) throw new BadRequestException("Gói bài viết không tồn tại.Vui lòng chọn lại!");

            var employer = await _unitOfWork.GenericRepository<Employer>().GetFirstOrDefaultAsync(a => a.UserID == userId);
            if (employer == null) throw new ForbiddenException("Bạn không có quyền tạo mới công việc.");

            var wallet = await _unitOfWork.GenericRepository<Wallet>().GetFirstOrDefaultAsync(a => a.UserID == userId);
            if (wallet == null) throw new NotFoundException("Ví không tồn tại.");


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
                    throw new BadRequestException("Số dư trong ví không đủ. Vui lòng quét mã để đăng bài");
                }
                else if (wallet.Balance >= subscription.Price)
                {
                    var description = $"- {subscription.Price}đ cho đăng bài tuyển dụng với gói '{subscription.SubscriptionName}'";
                    await _walletService.DeductFromWalletAsync(userId, subscription.Price, "", "SUCCESS");
                    job.Status = "ACTIVE";
                    await _unitOfWork.GenericRepository<Job>().InsertAsync(job);
                    await _unitOfWork.SaveChangeAsync();
                    await _unitOfWork.CommitTransactionAsync();

                    // Gửi notification cho students phù hợp
                    //await SendJobNotificationToSuitableStudents(job);
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
            if (subscription == null) throw new NotFoundException("Gói bài viết không tồn tại.Vui lòng chọn lại!");

            var job = await _unitOfWork.GenericRepository<Job>().GetFirstOrDefaultAsync(a => a.JobID == jobId);
            if (job == null) throw new NotFoundException("Bài viết không tồn tại.");

            if (job.Status == "ISACTIVE") throw new BadRequestException("Bài viết đã hết hạn!");

            var employer = await _unitOfWork.GenericRepository<Employer>().GetFirstOrDefaultAsync(a => a.UserID == userId);

            if (job.EmployerID != employer.EmployerID) throw new ForbiddenException("Bạn không có quyền chỉnh sửa bài viết này!");


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
            if (job == null) throw new NotFoundException("Bài viết không tồn tại!");

            if (status == null) throw new BadRequestException("Trạng thái không được hỗ trợ!");

            var validStatuses = new[] { "ACTIVE", "PENDINGPAYMENT", "EXPIRED", "DELETED" };
            if (!validStatuses.Contains(status))
                throw new BadRequestException("Trạng thái không được hỗ trợ!");

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
            if (job == null) throw new NotFoundException("Bài viết không tồn tại!");

            var employer = await _unitOfWork.GenericRepository<Employer>().GetFirstOrDefaultAsync(a => a.UserID == userId);
            if (job.EmployerID != employer.EmployerID) throw new NotFoundException("Bạn không có xóa bài viết này!");

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
                throw new NotFoundException("Job not found");
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
                includeProperties: "Subscription",
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
            Expression<Func<Job, bool>> predicate = job => job.Status == "ACTIVE";

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
                JobID = job.JobID,
                SubscriptionID = job.JobID,
                Category = job.Title,
                Title = job.Location,
                CreatedAt = job.CreatedAt,
                EndDate = job.EndDate,
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
            if (employer == null) throw new NotFoundException("Bạn cần đăng nhập hoặc tạo tài khoản trước khi xem danh sách!");

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
                JobID = job.JobID,
                SubscriptionID = job.JobID,
                Category = job.Title,
                Title = job.Location,
                CreatedAt = job.CreatedAt,
                EndDate = job.EndDate,
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
            if (student == null) throw new NotFoundException("Bạn cần đăng nhập hoặc tạo tài khoản trước khi xem danh sách!");

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
                JobID = job.JobID,
                SubscriptionID = job.JobID,
                Category = job.Category,
                Title = job.Title,
                CreatedAt = job.CreatedAt,
                EndDate = job.EndDate,
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
            if (employer == null) throw new NotFoundException("Không tìm thấy thông tin nhà tuyển dụng!");

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
        public async Task<Pagination<JobSearchResponseDTO>> SearchJobByFieldsAsync(string? category, string? title, string? location,
                                                                                   decimal? minSalary, decimal? maxSalary,
                                                                                   int pageIndex, int pageSize)
        {
            // 1. Predicate lọc theo điều kiện
            Expression<Func<Job, bool>> predicate = job =>
                (string.IsNullOrEmpty(category) || job.Category.Contains(category)) &&
                (string.IsNullOrEmpty(title) || job.Title.Contains(title)) &&
                (string.IsNullOrEmpty(location) || job.Location.Contains(location)) &&
                (!minSalary.HasValue || job.Salary >= minSalary.Value) &&
                (!maxSalary.HasValue || job.Salary <= maxSalary.Value) &&
                job.Status == "ACTIVE";

            // 2. Lấy danh sách job theo trang
            var paginatedJobs = await GetPaginatedJobAsync(
          pageIndex,
          pageSize,
          predicate
      );

            // 3. Map sang DTO và sắp xếp theo subscription
            var subscriptionPriority = new Dictionary<string, int>
    {
        { "Enterprise", 1 },
        { "Featured", 2 },
        { "Premium", 3 },
        { "Standard", 4 }
    };

            // 3. Map sang DTO
            var dtoList = paginatedJobs.Items.Select(job => new JobSearchResponseDTO
            {
                JobID = job.JobID,
                SubscriptionID = job.SubscriptionID, 
                Category = job.Category,
                Title = job.Title,
                CreatedAt = job.CreatedAt,
                EndDate = job.EndDate,
                Description = job.Description,
                Requirements = job.Requirements,
                Location = job.Location,
                Salary = job.Salary,
                WorkingHours = job.WorkingHours,
                SubscriptionName = job.Subscription?.SubscriptionName 
            }).OrderBy(dto => dto.SubscriptionName != null && subscriptionPriority.ContainsKey(dto.SubscriptionName)? subscriptionPriority[dto.SubscriptionName]: int.MaxValue).ToList();

            // 4. Trả về kết quả
            return new Pagination<JobSearchResponseDTO>
            {
                Items = dtoList,
                TotalItemsCount = paginatedJobs.TotalItemsCount,
                PageIndex = paginatedJobs.PageIndex,
                PageSize = paginatedJobs.PageSize
            };
        }

        //private async Task SendJobNotificationToSuitableStudents(Job job)
        //{
        //    try
        //    {
        //        // Lấy danh sách students có thể phù hợp với job
        //        // Có thể dựa trên location, skills, major, etc.
        //        var suitableStudents = await _unitOfWork.GenericRepository<Student>()
        //            .GetAllAsync(s => 
        //                (string.IsNullOrEmpty(job.Location) || s.Location.Contains(job.Location)) ||
        //                (string.IsNullOrEmpty(job.Requirements) || s.Skills.Contains(job.Requirements)),
        //                null);

        //        // Giới hạn số lượng notification để tránh spam
        //        var studentsToNotify = suitableStudents.Take(50).ToList();

        //        foreach (var student in studentsToNotify)
        //        {
        //            var notificationDto = new SWork.Data.DTO.NotificationDTO.CreateNotificationDTO
        //            {
        //                UserID = student.UserID,
        //                Title = "Công việc mới phù hợp",
        //                Message = $"Có công việc mới phù hợp với bạn: '{job.Title}' tại {job.Location} - {job.Salary:N0} VND"
        //            };
        //            await _notificationService.CreateNotificationAsync(notificationDto);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log error nhưng không throw để không ảnh hưởng đến việc tạo job
        //        Console.WriteLine($"Error sending job notifications: {ex.Message}");
        //    }
        //}
    }
}
