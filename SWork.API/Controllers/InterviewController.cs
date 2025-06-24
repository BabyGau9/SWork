using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SWork.Data.DTO.InterviewDTO;
using SWork.ServiceContract.Interfaces;
using System.Security.Claims;
using SWork.Common.Helper;
using System.Net;

namespace SWork.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class InterviewController : ControllerBase
    {
        private readonly IInterviewService _interviewService;

        public InterviewController(IInterviewService interviewService)
        {
            _interviewService = interviewService;
        }

        [HttpPost]
        [Authorize(Roles = "Employer")]
        public async Task<ActionResult> CreateInterview([FromForm] CreateInterviewDTO dto)
        {
            var response = new APIResponse();
            try{
                var employerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var interview = await _interviewService.CreateInterviewAsync(dto, employerId);
                response.Result = interview;
                response.StatusCode = HttpStatusCode.OK;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.BadRequest;
                response.ErrorMessages.Add(ex.Message ?? "Đã xảy ra lỗi khi tạo cuộc phỏng vấn.");
                return BadRequest(response);
            }   
        }

        [HttpGet]
        public async Task<ActionResult<APIResponse>> GetAll()
        {
            var response = new APIResponse();
            try
            {
                var interviews = await _interviewService.GetAllAsync();
                response.Result = interviews;
                response.StatusCode = HttpStatusCode.OK;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.ErrorMessages.Add(ex.Message ?? "Đã xảy ra lỗi khi lấy danh sách phỏng vấn.");
                return StatusCode(500, response);
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<APIResponse>> GetById(int id)
        {
            var response = new APIResponse();
            try
            {
                var interview = await _interviewService.GetByIdAsync(id);
                if (interview == null)
                {
                    response.IsSuccess = false;
                    response.StatusCode = HttpStatusCode.NotFound;
                    response.ErrorMessages.Add("Không tìm thấy cuộc phỏng vấn!");
                    return NotFound(response);
                }
                response.Result = interview;
                response.StatusCode = HttpStatusCode.OK;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.ErrorMessages.Add(ex.Message ?? "Đã xảy ra lỗi khi lấy thông tin phỏng vấn.");
                return StatusCode(500, response);
            }
        }

        [HttpGet("application/{applicationId}")]
        public async Task<ActionResult<APIResponse>> GetByApplicationId(int applicationId)
        {
            var response = new APIResponse();
            try
            {
                var interviews = await _interviewService.GetByApplicationIdAsync(applicationId);
                response.Result = interviews;
                response.StatusCode = HttpStatusCode.OK;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.ErrorMessages.Add(ex.Message ?? "Đã xảy ra lỗi khi lấy danh sách phỏng vấn theo đơn ứng tuyển.");
                return StatusCode(500, response);
            }
        }

        [HttpGet("student/{studentId}")]
        public async Task<ActionResult<APIResponse>> GetByStudentId(int studentId)
        {
            var response = new APIResponse();
            try
            {
                var interviews = await _interviewService.GetByStudentIdAsync(studentId);
                response.Result = interviews;
                response.StatusCode = HttpStatusCode.OK;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.ErrorMessages.Add(ex.Message ?? "Đã xảy ra lỗi khi lấy danh sách phỏng vấn theo sinh viên.");
                return StatusCode(500, response);
            }
        }

        [HttpGet("employer/{employerId}")]
        public async Task<ActionResult<APIResponse>> GetByEmployerId(int employerId)
        {
            var response = new APIResponse();
            try
            {
                var interviews = await _interviewService.GetByEmployerIdAsync(employerId);
                response.Result = interviews;
                response.StatusCode = HttpStatusCode.OK;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.ErrorMessages.Add(ex.Message ?? "Đã xảy ra lỗi khi lấy danh sách phỏng vấn theo nhà tuyển dụng.");
                return StatusCode(500, response);
            }
        }
        /// <summary>
        /// Update Status After Interview (SCHEDULED = 1, ACCEPTED = 2, REJECTED = 3, CANCELLED = 4, COMPLETED = 5, PENDING = 6)
        /// </summary>
        /// <param name="id"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPut("{id}/status")]
        public async Task<ActionResult<APIResponse>> UpdateStatus(int id, [FromBody] UpdateInterviewDTO dto)
        {
            var response = new APIResponse();
            try
            {
                var result = await _interviewService.UpdateInterviewStatusAsync(id, dto);
                response.Result = result;
                response.StatusCode = HttpStatusCode.OK;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.BadRequest;
                response.ErrorMessages.Add(ex.Message ?? "Đã xảy ra lỗi khi cập nhật trạng thái phỏng vấn.");
                return BadRequest(response);
            }
        }

        /// <summary>
        /// Update Status Before Interview (SCHEDULED = 1, ACCEPTED = 2, REJECTED = 3, CANCELLED = 4, COMPLETED = 5, PENDING = 6)
        /// </summary>
        /// <param name="id"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPut("{id}/status-before")]
        public async Task<ActionResult<APIResponse>> UpdateStatusBefore(int id, [FromBody] UpdateInterviewDTO dto)
        {
            var response = new APIResponse();
            try
            {
                var result = await _interviewService.UpdateInterviewStatusBeforeAsync(id, dto);
                response.Result = result;
                response.StatusCode = HttpStatusCode.OK;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.BadRequest;
                response.ErrorMessages.Add(ex.Message ?? "Đã xảy ra lỗi khi cập nhật trạng thái phỏng vấn.");
                return BadRequest(response);
            }
        }

    }
} 