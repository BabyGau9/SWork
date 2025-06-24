using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using SWork.Data.DTO.InterviewDTO;
using SWork.Data.Entities;
using SWork.Data.Enum;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SWork.Data.DTO;
using SWork.RepositoryContract.Interfaces;
using SWork.ServiceContract.Interfaces;
using SWork.Common.Middleware;

namespace SWork.Service.Services
{
    public class InterviewService : IInterviewService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuthService _authService;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IInterviewRepository _interviewRepository;
        private readonly IApplicationRepository _applicationRepository;

        public InterviewService(IUnitOfWork unitOfWork, IAuthService authService, IMapper mapper, UserManager<ApplicationUser> userManager, IInterviewRepository interviewRepository, IApplicationRepository applicationRepository)
        {
            _unitOfWork = unitOfWork;
            _authService = authService;
            _mapper = mapper;
            _userManager = userManager;
            _interviewRepository = interviewRepository;
            _applicationRepository = applicationRepository;
        }

        public async Task<CreateInterviewDTO> CreateInterviewAsync(CreateInterviewDTO dto, string userId)
        {
            var user = await _unitOfWork.GenericRepository<ApplicationUser>().GetFirstOrDefaultAsync(a => a.Id == userId);
            var employer = await _unitOfWork.GenericRepository<Employer>().GetFirstOrDefaultAsync(a => a.UserID == userId);
            if (employer == null || user == null) throw new NotFoundException("Bạn cần đăng nhập hoặc tạo tài khoản trước khi ứng tuyển.");

            var app = await _unitOfWork.GenericRepository<Application>().GetFirstOrDefaultAsync(a => a.ApplicationID == dto.ApplicationID);
            if (app == null) throw new NotFoundException("Application không tồn tại");

            var interview = new Interview
            {
                ApplicationID = dto.ApplicationID,
                ScheduledTime = dto.ScheduledTime,
                Duration_minutes = dto.Duration_minutes,
                Location = dto.Location,
                MeetingLink = dto.MeetingLink,
                Note = dto.Note,
                Status = InterviewStatus.PENDING
            };

            await _unitOfWork.GenericRepository<Interview>().InsertAsync(interview);
            await _unitOfWork.SaveChangeAsync();

            return _mapper.Map<CreateInterviewDTO>(interview);
        }

        public async Task<InterviewResponseDTO> GetByIdAsync(int id)
        {
            var interview = await _interviewRepository.GetByIdAsync(id);
            return _mapper.Map<InterviewResponseDTO>(interview);
        }

        public async Task<IEnumerable<InterviewResponseDTO>> GetAllAsync()
        {
            var interviews = await _interviewRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<InterviewResponseDTO>>(interviews);
        }

        public async Task<IEnumerable<InterviewResponseDTO>> GetByApplicationIdAsync(int applicationId)
        {
            var interviews = await _interviewRepository.GetByApplicationIdAsync(applicationId);
            return _mapper.Map<IEnumerable<InterviewResponseDTO>>(interviews);
        }

        public async Task<IEnumerable<InterviewResponseDTO>> GetByStudentIdAsync(int studentId)
        {
            var interviews = await _interviewRepository.GetByStudentIdAsync(studentId);
            return _mapper.Map<IEnumerable<InterviewResponseDTO>>(interviews);
        }

        public async Task<IEnumerable<InterviewResponseDTO>> GetByEmployerIdAsync(int employerId)
        {
            var interviews = await _interviewRepository.GetByEmployerIdAsync(employerId);
            return _mapper.Map<IEnumerable<InterviewResponseDTO>>(interviews);
        }

        public async Task<InterviewResponseDTO> UpdateInterviewStatusAsync(int interviewId, UpdateInterviewDTO dto)
        {
            var interview = await _interviewRepository.GetByIdAsync(interviewId);
            if (interview == null)
                throw new Exception("Không tìm thấy cuộc phỏng vấn!");

            if (interview.Status != InterviewStatus.SCHEDULED)
                throw new Exception("Chỉ có thể cập nhật các cuộc phỏng vấn đã lên lịch!");

            // Update interview status
            interview.Status = dto.NewStatus;
            await _interviewRepository.UpdateAsync(interview);

            // Update application status based on interview status
            var application = await _applicationRepository.GetByIdAsync(interview.ApplicationID);
            if (application == null)
                throw new Exception("Không tìm thấy đơn ứng tuyển nào!");

            application.Status = dto.NewStatus switch
            {
                InterviewStatus.ACCEPTED => ApplicationStatus.WORKING.ToString(),
                InterviewStatus.REJECTED => ApplicationStatus.REJECTED.ToString(),
                _ => application.Status
            };

            _applicationRepository.Update(application);
            await _unitOfWork.SaveChangeAsync();

            return _mapper.Map<InterviewResponseDTO>(interview);
        }

        public async Task<InterviewResponseDTO> UpdateInterviewStatusBeforeAsync(int interviewId, UpdateInterviewDTO dto)
        {
            var interview = await _interviewRepository.GetByIdAsync(interviewId);
            if (interview == null)
                throw new Exception("Không tìm thấy cuộc phỏng vấn nào!");

            // Update interview status
            interview.Status = dto.NewStatus;
            await _interviewRepository.UpdateAsync(interview);

            // Update application status based on interview status
            var application = await _applicationRepository.GetByIdAsync(interview.ApplicationID);
            if (application == null)
                throw new Exception("Không tìm thấy đơn ứng tuyển nào!");

            _applicationRepository.Update(application);
            // Lưu thay đổi vào database
            await _unitOfWork.SaveChangeAsync();

            return _mapper.Map<InterviewResponseDTO>(interview);
        }
    }
} 