namespace SWork.Data.DTO.AuthDTO
{
    public enum AuthStatus
    {
        Success,
        InvalidCredentials,
        EmailNotConfirmed,
        UserNotFound
    }

    public class AuthResultDTO
    {
        public AuthStatus Status { get; set; }
        public string Message { get; set; }
        public LoginResponseDTO LoginResponse { get; set; }
    }
}