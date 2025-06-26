using Microsoft.AspNetCore.Identity;
using SWork.Common.Middleware;
using SWork.Data.DTO.ApplicationDTO;
using SWork.Data.Enum;
using SWork.ServiceContract.Interfaces;

namespace SWork.Service.Services
{
    public class ApplicationService : IApplicationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly INotificationService _notificationService;

        public ApplicationService(IUnitOfWork unitOfWork, IMapper mapper, UserManager<ApplicationUser> userManager, INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
            _notificationService = notificationService;
        }
        public async Task<ResponseApplyDTO> CreateApplicationAsync(RequestApplyDTO apply, string userId)
        {
            var user = await _unitOfWork.GenericRepository<ApplicationUser>().GetFirstOrDefaultAsync(a => a.Id == userId);
            var student = await _unitOfWork.GenericRepository<Student>().GetFirstOrDefaultAsync(a => a.UserID == userId);
            if (student == null || user == null) throw new NotFoundException("Bạn cần đăng nhập hoặc tạo tài khoản trước khi ứng tuyển!");

            var job = await _unitOfWork.GenericRepository<Job>().GetFirstOrDefaultAsync(a => a.JobID == apply.JobID);
            if (job == null) throw new NotFoundException("Công việc không tồn tại. Vui lòng chọn công việc khác để ứng tuyển!");

            if (job.Status == "ISACTIVE") throw new BadRequestException("Công việc đã hết hạn không thể ứng tuyển. Vui lòng chọn công việc khác để ứng tuyển!");

            apply.StudentID = student.StudentID;
            apply.Status = "PENDING";
            apply.AppliedAt = DateTime.Now;
            apply.UpdateAt = null;

            var application = _mapper.Map<Application>(apply);

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                await _unitOfWork.GenericRepository<Application>().InsertAsync(application);
                await _unitOfWork.SaveChangeAsync();
                await _unitOfWork.CommitTransactionAsync();

                // Gửi notification cho employer về application mới
                var employer = await _unitOfWork.GenericRepository<Employer>().GetFirstOrDefaultAsync(e => e.EmployerID == job.EmployerID);
                if (employer != null)
                {
                    var notificationDto = new SWork.Data.DTO.NotificationDTO.CreateNotificationDTO
                    {
                        UserID = employer.UserID,
                        Title = "Đơn ứng tuyển mới",
                        Message = $"Có đơn ứng tuyển mới cho công việc '{job.Title}' từ {user.UserName}"
                    };
                    await _notificationService.CreateNotificationAsync(notificationDto);
                }

                ResponseApplyDTO res = new()
                {
                    STT = application.ApplicationID,
                    StudentName = user.UserName,
                    JobID = application.JobID,
                    JobName = job.Title,
                    Status = application.Status,
                    ResumeID = application.ResumeID,
                    Coverletter = application.Coverletter,
                    AppliedAt = application.AppliedAt,
                    UpdateAt = application.UpdatedAt,
                };
                return res;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<ResponseApplyDTO> UpdateApplicationAsync(StatusOfApplyDTO applyDto, string userId)
        {
            // check user has exist
            var user = await _unitOfWork.GenericRepository<ApplicationUser>().GetFirstOrDefaultAsync(a => a.Id == userId);
            var employer = await _unitOfWork.GenericRepository<Employer>().GetFirstOrDefaultAsync(a => a.UserID == userId);
            if (employer == null || user == null) throw new NotFoundException("Bạn cần đăng nhập hoặc tạo tài khoản trước khi xét duyệt hồ sơ!");

            // check application has exist
            var application = await _unitOfWork.GenericRepository<Application>().GetFirstOrDefaultAsync(a => a.ApplicationID == applyDto.ApplicationID);
            if (application == null) throw new NotFoundException("Hồ sơ không tồn tại!");

            var student = await _unitOfWork.GenericRepository<Student>().GetFirstOrDefaultAsync(a => a.StudentID == application.StudentID);
            var Astudent = await _unitOfWork.GenericRepository<ApplicationUser>().GetFirstOrDefaultAsync(a => a.Id == student.UserID);

            // check this job has exist  
            var job = await _unitOfWork.GenericRepository<Job>().GetFirstOrDefaultAsync(a => a.JobID == application.JobID);
            if (job.Status == "ISACTIVE") throw new BadRequestException("Bài viết đã hết hạn. Không thể thực hiện xét duyệt hồ sơ!");

            if (job.EmployerID != employer.EmployerID) throw new ForbiddenException("Bạn không có quyền xét duyệt hồ sơ này!");

            if (!IsValidStatusTransition(application.Status, applyDto.Status))
                throw new BadRequestException("Trạng thái xét duyệt không hợp lệ!");

            var oldStatus = application.Status;
            application.Status = applyDto.Status;
            application.UpdatedAt = DateTime.Now;
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                _unitOfWork.GenericRepository<Application>().Update(application);
                await _unitOfWork.SaveChangeAsync();
                await _unitOfWork.CommitTransactionAsync();

                // Gửi notification cho student về thay đổi status
                var notificationDto = new SWork.Data.DTO.NotificationDTO.CreateNotificationDTO
                {
                    UserID = student.UserID,
                    Title = "Cập nhật đơn ứng tuyển",
                    Message = GetStatusUpdateMessage(oldStatus, applyDto.Status, job.Title)
                };
                await _notificationService.CreateNotificationAsync(notificationDto);

                ResponseApplyDTO res = new()
                {
                    STT = application.ApplicationID,
                    StudentName = Astudent.UserName,
                    JobID = application.JobID,
                    JobName = job.Title,
                    Status = application.Status,
                    ResumeID = application.ResumeID,
                    Coverletter = application.Coverletter,
                    AppliedAt = application.AppliedAt,
                    UpdateAt = application.UpdatedAt,
                };
                return res;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<ResponseApplyDTO> GetApplicationByIdAsync(int applyId, string userId)
        {
            // get application
            var application = await _unitOfWork.GenericRepository<Application>().GetFirstOrDefaultAsync(a => a.ApplicationID == applyId);
            if (application == null) throw new NotFoundException("Hồ sơ không tồn tại!");

            // get job
            var job = await _unitOfWork.GenericRepository<Job>().GetFirstOrDefaultAsync(a => a.JobID == application.JobID);

            // get student
            var student1 = await _unitOfWork.GenericRepository<Student>().GetFirstOrDefaultAsync(a => a.StudentID == application.StudentID);            // get student from application
            var student2 = await _unitOfWork.GenericRepository<ApplicationUser>().GetFirstOrDefaultAsync(a => a.Id == student1.UserID);  //get student from applicationUser

            //get employer
            var employer = await _unitOfWork.GenericRepository<Employer>().GetFirstOrDefaultAsync(a => a.EmployerID == job.EmployerID);

            // get role admin
            var currentUser = await _unitOfWork.GenericRepository<ApplicationUser>().GetFirstOrDefaultAsync(a => a.Id == userId);
            var userRole = await _userManager.GetRolesAsync(currentUser);

            // check valid application
            bool isStudentOwner = student1 != null;
            bool isEmployerOwner = employer != null;
            bool isAdmin = userRole.Contains("Admin");



            if (isStudentOwner || isEmployerOwner || isAdmin)
            {
                ResponseApplyDTO res = new()
                {
                    STT = application.ApplicationID,
                    StudentName = student2.UserName,
                    JobID = application.JobID,
                    JobName = job.Title,
                    Status = application.Status,
                    ResumeID = application.ResumeID,
                    Coverletter = application.Coverletter,
                    AppliedAt = application.AppliedAt
                };
                return res;
            }
            else
            {
                throw new NotFoundException("Hồ sơ không tồn tại.");
            }
        }

        public async Task<Pagination<Application>> GetPaginatedApplicationAsync(
             int pageIndex,
             int pageSize,
             Expression<Func<Application, bool>>? predicate = null,
             Expression<Func<Application, object>>? orderBy = null,
             bool isDescending = false)
        {
            try
            {
                var result = await _unitOfWork.GenericRepository<Application>().GetPaginationAsync(
                    predicate = predicate,
                    includeProperties: null,
                    pageIndex: pageIndex,
                    pageSize: pageSize,
                    orderBy: orderBy ?? (p => p.ApplicationID),
                    isDescending: isDescending);

                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<Pagination<ResponseApplyDTO>> GetApplyRelatedJobForEmployer(string userId, int jobId, int pageIndex, int pageSize)
        {
            // Lấy Employer từ userId
            var employer = await _unitOfWork.GenericRepository<Employer>()
                .GetFirstOrDefaultAsync(e => e.UserID == userId);
            if (employer == null)
                throw new NotFoundException("Bạn cần đăng nhập hoặc tạo tài khoản trước khi xem.");

            // Lấy Job
            var job = await _unitOfWork.GenericRepository<Job>().GetFirstOrDefaultAsync(j => j.JobID == jobId);

            if (job == null || job.EmployerID != employer.EmployerID)
                throw new NotFoundException("Bạn không có quyền xem hồ sơ ứng tuyển cho công việc này.");

            // Lọc các application theo JobID
            Expression<Func<Application, bool>> predicate = application => application.JobID == jobId;

            var paginatedApplications = await GetPaginatedApplicationAsync(
                pageIndex,
                pageSize,
                predicate,
                orderBy: a => a.ApplicationID,
                isDescending: true
            );

            // Lấy thông tin student tương ứng
            var studentIds = paginatedApplications.Items.Select(a => a.StudentID).Distinct().ToList();
            var students = await _unitOfWork.GenericRepository<Student>()
                .GetAllAsync(s => studentIds.Contains(s.StudentID), null);
            var users = await _unitOfWork.GenericRepository<ApplicationUser>()
                .GetAllAsync(u => students.Select(s => s.UserID).Contains(u.Id), null);

            // Map kết quả
            List<ResponseApplyDTO> dtoList = paginatedApplications.Items.Select(application =>
            {
                var student = students.FirstOrDefault(s => s.StudentID == application.StudentID);
                var studentUser = users.FirstOrDefault(u => u.Id == student?.UserID);

                return new ResponseApplyDTO
                {
                    STT = application.ApplicationID,
                    StudentName = studentUser?.UserName,
                    JobID = application.JobID,
                    JobName = job.Title,
                    Status = application.Status,
                    ResumeID = application.ResumeID,
                    Coverletter = application.Coverletter,
                    AppliedAt = application.AppliedAt,
                    UpdateAt = application.UpdatedAt
                };
            }).ToList();

            return new Pagination<ResponseApplyDTO>
            {
                Items = dtoList,
                TotalItemsCount = paginatedApplications.TotalItemsCount,
                PageIndex = paginatedApplications.PageIndex,
                PageSize = paginatedApplications.PageSize
            };
        }

        public async Task<Pagination<ResponseApplyDTO>> GetApplyJobForStudent(string userId, int pageIndex, int pageSize)
        {
            // Lấy Employer từ userId
            var student = await _unitOfWork.GenericRepository<Student>()
                .GetFirstOrDefaultAsync(e => e.UserID == userId);
            if (student == null)
                throw new NotFoundException("Bạn cần đăng nhập hoặc tạo tài khoản trước khi xem.");

            // Lọc các application theo JobID
            Expression<Func<Application, bool>> predicate = application => application.StudentID == student.StudentID;

            var paginatedApplications = await GetPaginatedApplicationAsync(
                pageIndex,
                pageSize,
                predicate,
                orderBy: a => a.ApplicationID,
                isDescending: true
            );
            // Lấy danh sách Job và sinh viên liên quan
            var jobIds = paginatedApplications.Items.Select(a => a.JobID).Distinct().ToList();
            var jobs = await _unitOfWork.GenericRepository<Job>()
                .GetAllAsync(j => jobIds.Contains(j.JobID), null);

            var jobDict = jobs.ToDictionary(j => j.JobID, j => j);

            // Lấy thông tin ApplicationUser của sinh viên
            var studentUser = await _unitOfWork.GenericRepository<ApplicationUser>().GetFirstOrDefaultAsync(u => u.Id == student.UserID);

            // sắp xếp
            var statusOrder = new Dictionary<ApplicationStatus, int>
            {
                [ApplicationStatus.APPROVED] = 0,
                [ApplicationStatus.PENDING] = 1,
                [ApplicationStatus.WORKING] = 3,
                [ApplicationStatus.REJECTED] = 4,
                [ApplicationStatus.FINISHED] = 5,
                [ApplicationStatus.DELETE] = 6
            };


            // Map kết quả
            List<ResponseApplyDTO> dtoList = paginatedApplications.Items
                .OrderBy(a =>
                {
                    return Enum.TryParse<ApplicationStatus>(a.Status, out var statusEnum)
                        && statusOrder.TryGetValue(statusEnum, out var order)
                        ? order
                        : int.MaxValue;
                })
                .Select(application =>
            {
                jobDict.TryGetValue(application.JobID, out var job);

                return new ResponseApplyDTO
                {
                    STT = application.ApplicationID,
                    StudentName = studentUser?.UserName,
                    JobID = application.JobID,
                    JobName = job.Title,
                    Status = application.Status,
                    ResumeID = application.ResumeID,
                    Coverletter = application.Coverletter,
                    AppliedAt = application.AppliedAt,
                    UpdateAt = application.UpdatedAt
                };
            }).ToList();

            return new Pagination<ResponseApplyDTO>
            {
                Items = dtoList,
                TotalItemsCount = paginatedApplications.TotalItemsCount,
                PageIndex = paginatedApplications.PageIndex,
                PageSize = paginatedApplications.PageSize
            };
        }
        private bool IsValidStatusTransition(string currentStatus, string newStatus)
        {
            return currentStatus switch
            {
                "PENDING" => newStatus == "REJECTED" || newStatus == "APPROVED",
                "APPROVED" => newStatus == "WORKING" || newStatus == "REJECTED",
                "WORKING" => newStatus == "FINISHED",
                _ => false,
            };
        }

        private async Task TryCreateInterviewAsync(Application application, int employerId)
        {
            var interview = new Interview
            {
                ApplicationID = application.ApplicationID,
            //    ScheduledTime = null,
            //   Duration_minutes = null,
                Location = null,
                MeetingLink = null,
                Note = null,
                Status = InterviewStatus.PENDING,
                CreatedAt = DateTime.Now,
            };
            await _unitOfWork.GenericRepository<Interview>().InsertAsync(interview);
            await _unitOfWork.SaveChangeAsync();
        }

        private string GetStatusUpdateMessage(string oldStatus, string newStatus, string jobTitle)
        {
            return newStatus switch
            {
                "APPROVED" => $"Đơn ứng tuyển cho công việc '{jobTitle}' đã được chấp nhận!",
                "REJECTED" => $"Đơn ứng tuyển cho công việc '{jobTitle}' đã bị từ chối.",
                "WORKING" => $"Bạn đã bắt đầu làm việc cho công việc '{jobTitle}'.",
                "FINISHED" => $"Công việc '{jobTitle}' đã hoàn thành.",
                "INVITED" => $"Bạn đã được mời phỏng vấn cho công việc '{jobTitle}'.",
                _ => $"Trạng thái đơn ứng tuyển cho công việc '{jobTitle}' đã được cập nhật: {newStatus}"
            };
        }
    }
}
