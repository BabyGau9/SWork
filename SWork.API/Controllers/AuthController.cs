using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SWork.Common.Helper;
using SWork.Data.Entities;
using SWork.ServiceContract.Interfaces;
using AutoMapper;
using SWork.Data.DTO.UserDTO;
using SWork.Data.DTO.AuthDTO;

namespace SWork.API.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    [EnableRateLimiting("default")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly IMapper _mapper;

        public AuthController(IAuthService authService, UserManager<ApplicationUser> userManager, IEmailService emailService, IMapper mapper)
        {
            _authService = authService;
            _userManager = userManager;
            _emailService = emailService;
            _mapper = mapper;
        }

        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [AllowAnonymous]
        public async Task<ActionResult<APIResponse>> Register(UserRegisterDTO dto)
        {
            var response = new APIResponse();
            if (!ModelState.IsValid)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Result = ModelState;
                return BadRequest(response);
            }

            var result = await _authService.RegisterAsync(dto);
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(result);
            var confirmationLink = Url.Action(nameof(ConfirmEmail), "Auth", new { token = token, email = result.Email }, Request.Scheme);

            _emailService.SendEmailConfirmation(result.Email, confirmationLink);

            // Map to UserResponseDTO
            var userResponse = _mapper.Map<UserResponseDTO>(result);

            response.IsSuccess = true;
            response.StatusCode = HttpStatusCode.OK;
            response.Result = userResponse;

            return Ok(response);
        }

        [HttpGet("confirm-email")]
        [AllowAnonymous]
        public async Task<ActionResult<APIResponse>> ConfirmEmail([FromQuery] string token, [FromQuery] string email)
        {
            var response = new APIResponse();
            try
            {
                var result = await _authService.ConfirmEmail(email, token);
                if (result)
                {
                    response.IsSuccess = true;
                    response.StatusCode = HttpStatusCode.OK;
                    return Ok(response);
                }
                response.IsSuccess = false;
                response.ErrorMessages.Add("Không thể xác nhận email của bạn!");
                response.StatusCode = HttpStatusCode.BadRequest;
                return BadRequest(response);
            }
            catch (Exception e)
            {
                response.IsSuccess = false;
                response.ErrorMessages.Add(e.Message);
                response.StatusCode = HttpStatusCode.BadRequest;
                return BadRequest(response);
            }
        }

        /// <summary>
        /// Login for user
        /// </summary>
        /// <param name="loginRequestDTO"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO loginRequestDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var authResult = await _authService.LoginAsync(loginRequestDTO);

            return authResult.Status switch
            {
                AuthStatus.Success => Ok(new APIResponse
                {
                    IsSuccess = true,
                    StatusCode = HttpStatusCode.OK,
                    Result = authResult.LoginResponse
                }),
                AuthStatus.UserNotFound => Unauthorized(new APIResponse
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.Unauthorized,
                    ErrorMessages = new List<string> { authResult.Message }
                }),
                AuthStatus.InvalidCredentials => Unauthorized(new APIResponse
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.Unauthorized,
                    ErrorMessages = new List<string> { authResult.Message }
                }),
                AuthStatus.EmailNotConfirmed => new ObjectResult(new APIResponse
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.Forbidden,
                    ErrorMessages = new List<string> { authResult.Message }
                })
                {
                    StatusCode = StatusCodes.Status403Forbidden
                },
                _ => StatusCode(StatusCodes.Status500InternalServerError, new APIResponse
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.InternalServerError,
                    ErrorMessages = new List<string> { "Đã có lỗi xảy ra trong quá trình xác thực." }
                }),
            };
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout([FromBody] LogoutRequestDTO dto)
        {
            if (string.IsNullOrEmpty(dto.RefreshToken))
                return BadRequest("Cần phải có refresh token!");
            await _authService.LogoutAsync(dto.RefreshToken);
            return Ok(new { message = "Đăng xuất thành công" });
        }
    }
}