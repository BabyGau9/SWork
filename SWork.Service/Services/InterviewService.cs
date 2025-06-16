
using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using SWork.Data.DTO.InterviewDTO;
using SWork.Data.Entities;
using SWork.Data.Enum;

namespace SWork.Service.Services
{
    public class InterviewService : IInterviewService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuthService _authService;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;

        public InterviewService(IUnitOfWork unitOfWork, IAuthService authService, IMapper mapper, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _authService = authService;
            _mapper = mapper;
            _userManager = userManager; 
        }

        public async Task<CreateInterviewDTO> CreateInterviewAsync(CreateInterviewDTO dto, string userId)

        {
            var user = await _unitOfWork.GenericRepository<ApplicationUser>().GetFirstOrDefaultAsync(a => a.Id == userId);
            var employer = await _unitOfWork.GenericRepository<Employer>().GetFirstOrDefaultAsync(a => a.UserID == userId);
            if (employer == null || user == null) throw new Exception("Bạn cần đăng nhập hoặc tạo tài khoản trước khi ứng tuyển.");

            var app = await _unitOfWork.GenericRepository<Application>().GetFirstOrDefaultAsync(a => a.ApplicationID == dto.ApplicationID);
            if (app == null) throw new Exception("Application không tồn tại");

            var interview = new Interview
            {
                ApplicationID = dto.ApplicationID,
                ScheduledTime = dto.ScheduledTime,
                Location = dto.Location,
                MeetingLink = dto.MeetingLink,
                Note = dto.Note,
                Status = InterviewStatus.PENDING
            };

            await _unitOfWork.GenericRepository<Interview>().InsertAsync(interview);
            await _unitOfWork.SaveChangeAsync();

            return _mapper.Map<CreateInterviewDTO>(interview);
        }



        //public async Task<InterviewDTO> CreateInterviewAsync(CreateInterviewDTO dto, int applicationId)
        //{
        //    var application = await _unitOfWork.GenericRepository<Application>().GetFirstOrDefaultAsync(a => a.ApplicationID == applicationId);
        //    if (application == null)
        //        throw new Exception("Application not found");

        //    //// Check if employer has permission for this application
        //    //if (!await _authService.HasPermissionForApplicationAsync(application))
        //    //    throw new UnauthorizedException("You don't have permission to create interview for this application");

        //    var interview = new Interview
        //    {
        //        ApplicationId = dto.ApplicationID,
        //        ScheduledTime = dto.ScheduledTime,
        //        Location = dto.Location,
        //        MeetingLink = dto.MeetingLink,
        //        Notes = dto.Note,
        //        Status = InterviewStatus.Scheduled
        //    };

        //    await _unitOfWork.GenericRepository<Interview>().InsertAsync(interview);
        //    await _unitOfWork.SaveChangeAsync();

        //    return _mapper.Map<InterviewDTO>(interview);
        //}

        //public async Task<InterviewDTO> UpdateInterviewAsync(long interviewId, UpdateInterviewDTO dto)
        //{
        //    var interview = await _unitOfWork.InterviewRepository.GetByIdAsync(interviewId);
        //    if (interview == null)
        //        throw new NotFoundException("Interview not found");

        //    // Check if employer has permission and is the creator
        //    if (!await _authService.HasPermissionForApplicationAsync(interview.Application))
        //        throw new UnauthorizedException("You don't have permission to update this interview");

        //    // Check if interview can be updated
        //    if (interview.Status != InterviewStatus.Scheduled)
        //        throw new InvalidOperationException("Cannot update interview that is not in Scheduled status");

        //    interview.ScheduledTime = dto.ScheduledTime;
        //    interview.Location = dto.Location;
        //    interview.MeetingLink = dto.MeetingLink;
        //    interview.Notes = dto.Notes;
        //    interview.UpdatedAt = DateTime.UtcNow;

        //    _unitOfWork.InterviewRepository.Update(interview);
        //    await _unitOfWork.SaveChangesAsync();

        //    return _mapper.Map<InterviewDTO>(interview);
        //}

        //public async Task<InterviewDTO> GetInterviewDetailsAsync(long interviewId)
        //{
        //    var interview = await _unitOfWork.InterviewRepository.GetByIdAsync(interviewId);
        //    if (interview == null)
        //        throw new NotFoundException("Interview not found");

        //    // Check if user has permission to view
        //    if (!await _authService.HasPermissionToViewInterviewAsync(interview))
        //        throw new UnauthorizedException("You don't have permission to view this interview");

        //    return _mapper.Map<InterviewDTO>(interview);
        //}

        //public async Task<InterviewDTO> CancelInterviewAsync(long interviewId)
        //{
        //    var interview = await _unitOfWork.InterviewRepository.GetByIdAsync(interviewId);
        //    if (interview == null)
        //        throw new NotFoundException("Interview not found");

        //    // Check if employer has permission and is the creator
        //    if (!await _authService.HasPermissionForApplicationAsync(interview.Application))
        //        throw new UnauthorizedException("You don't have permission to cancel this interview");

        //    // Check if interview can be cancelled
        //    if (interview.Status != InterviewStatus.Scheduled)
        //        throw new InvalidOperationException("Cannot cancel interview that is not in Scheduled status");

        //    interview.Status = InterviewStatus.Cancelled;
        //    interview.UpdatedAt = DateTime.UtcNow;

        //    _unitOfWork.InterviewRepository.Update(interview);
        //    await _unitOfWork.SaveChangesAsync();

        //    return _mapper.Map<InterviewDTO>(interview);
        //}

        //public async Task<InterviewDTO> AcceptInterviewAsync(long interviewId)
        //{
        //    var interview = await _unitOfWork.InterviewRepository.GetByIdAsync(interviewId);
        //    if (interview == null)
        //        throw new NotFoundException("Interview not found");

        //    // Check if student has permission
        //    if (!await _authService.IsStudentOfApplicationAsync(interview.Application))
        //        throw new UnauthorizedException("Only the student can accept the interview");

        //    // Check if interview can be accepted
        //    if (interview.Status != InterviewStatus.Scheduled)
        //        throw new InvalidOperationException("Cannot accept interview that is not in Scheduled status");

        //    interview.Status = InterviewStatus.Accepted;
        //    interview.UpdatedAt = DateTime.UtcNow;

        //    _unitOfWork.InterviewRepository.Update(interview);
        //    await _unitOfWork.SaveChangesAsync();

        //    return _mapper.Map<InterviewDTO>(interview);
        //}

        //public async Task<InterviewDTO> RejectInterviewAsync(long interviewId)
        //{
        //    var interview = await _unitOfWork.InterviewRepository.GetByIdAsync(interviewId);
        //    if (interview == null)
        //        throw new NotFoundException("Interview not found");

        //    // Check if student has permission
        //    if (!await _authService.IsStudentOfApplicationAsync(interview.Application))
        //        throw new UnauthorizedException("Only the student can reject the interview");

        //    // Check if interview can be rejected
        //    if (interview.Status != InterviewStatus.Scheduled)
        //        throw new InvalidOperationException("Cannot reject interview that is not in Scheduled status");

        //    interview.Status = InterviewStatus.Rejected;
        //    interview.UpdatedAt = DateTime.UtcNow;

        //    _unitOfWork.InterviewRepository.Update(interview);
        //    await _unitOfWork.SaveChangesAsync();

        //    return _mapper.Map<InterviewDTO>(interview);
        //}

        //public async Task<IEnumerable<InterviewDTO>> GetInterviewsByApplicationAsync(long applicationId)
        //{
        //    var application = await _unitOfWork.ApplicationRepository.GetByIdAsync(applicationId);
        //    if (application == null)
        //        throw new NotFoundException("Application not found");

        //    // Check if user has permission to view
        //    if (!await _authService.HasPermissionToViewApplicationAsync(application))
        //        throw new UnauthorizedException("You don't have permission to view interviews for this application");

        //    var interviews = await _unitOfWork.InterviewRepository
        //        .GetAll()
        //        .Include(i => i.Application)
        //        .Where(i => i.ApplicationId == applicationId)
        //        .OrderByDescending(i => i.ScheduledTime)
        //        .ToListAsync();

        //    return _mapper.Map<IEnumerable<InterviewDTO>>(interviews);
        //}

        //public async Task<IEnumerable<InterviewDTO>> GetStudentInterviewsAsync()
        //{
        //    var currentStudent = await _authService.GetCurrentStudentAsync();
        //    var interviews = await _unitOfWork.InterviewRepository
        //        .GetAll()
        //        .Include(i => i.Application)
        //        .Where(i => i.Application.StudentId == currentStudent.Id)
        //        .OrderByDescending(i => i.ScheduledTime)
        //        .ToListAsync();

        //    return _mapper.Map<IEnumerable<InterviewDTO>>(interviews);
        //}
    }
} 