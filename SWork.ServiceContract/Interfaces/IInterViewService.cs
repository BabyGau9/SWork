

using SWork.Data.DTO.InterviewDTO;

namespace SWork.ServiceContract.Interfaces
{
    public interface IInterviewService
    {
        Task<CreateInterviewDTO> CreateInterviewAsync(CreateInterviewDTO dto, string userId);
        //Task<InterviewDTO> CreateInterviewAsync(CreateInterviewDTO dto);

        //Task<InterviewDTO> UpdateInterviewAsync(long interviewId, UpdateInterviewDTO dto);

        //Task<InterviewDTO> GetInterviewDetailsAsync(long interviewId);

        //Task<InterviewDTO> CancelInterviewAsync(long interviewId);

        //Task<InterviewDTO> AcceptInterviewAsync(long interviewId);

        //Task<InterviewDTO> RejectInterviewAsync(long interviewId);

        //Task<IEnumerable<InterviewDTO>> GetInterviewsByApplicationAsync(long applicationId);

        //Task<IEnumerable<InterviewDTO>> GetStudentInterviewsAsync();
    }
}
