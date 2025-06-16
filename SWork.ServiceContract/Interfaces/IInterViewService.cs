using System.Collections.Generic;
using System.Threading.Tasks;
using SWork.Data.DTO;
using SWork.Data.DTO.InterviewDTO;

namespace SWork.ServiceContract.Interfaces
{
    public interface IInterviewService
    {
        Task<CreateInterviewDTO> CreateInterviewAsync(CreateInterviewDTO dto, string userId);
        Task<InterviewResponseDTO> GetByIdAsync(int id);
        Task<IEnumerable<InterviewResponseDTO>> GetAllAsync();
        Task<IEnumerable<InterviewResponseDTO>> GetByApplicationIdAsync(int applicationId);
        Task<IEnumerable<InterviewResponseDTO>> GetByStudentIdAsync(int studentId);
        Task<IEnumerable<InterviewResponseDTO>> GetByEmployerIdAsync(int employerId);
        Task<InterviewResponseDTO> UpdateInterviewStatusAsync(int interviewId, UpdateInterviewDTO dto);

        Task<InterviewResponseDTO> UpdateInterviewStatusBeforeAsync(int interviewId, UpdateInterviewDTO dto);
    }
}
