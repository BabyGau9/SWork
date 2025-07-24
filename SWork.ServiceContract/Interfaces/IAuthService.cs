

namespace SWork.ServiceContract.Interfaces
{
    public interface IAuthService
    {
        Task<ApplicationUser> RegisterAsync(UserRegisterDTO dto);
        Task<bool> ConfirmEmail(string email, string token);
        Task<AuthResultDTO> LoginAsync(LoginRequestDTO loginRequestDTO);
        Task LogoutAsync(string refreshToken);
        Task<bool> HasPermissionForApplicationAsync(int applicationId, string employerId);
        Task<bool> ChangePasswordAsync(string userId, ChangePasswordDTO dto);
        Task<bool> ResetPasswordAsync(ResetPasswordDTO dto);
        Task<bool> SendForgotPasswordEmailAsync(string email, string resetLinkBaseUrl);
    }
}
