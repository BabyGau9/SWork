using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SWork.Data.DTO.InterviewDTO;
using SWork.ServiceContract.Interfaces;
using System.Security.Claims;

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
            try{
                var employerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var interview = await _interviewService.CreateInterviewAsync(dto, employerId);
                return Ok(interview);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }   

        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<InterviewResponseDTO>>> GetAll()
        {
            var interviews = await _interviewService.GetAllAsync();
            return Ok(interviews);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<InterviewResponseDTO>> GetById(int id)
        {
            var interview = await _interviewService.GetByIdAsync(id);
            if (interview == null)
                return NotFound();

            return Ok(interview);
        }

        [HttpGet("application/{applicationId}")]
        public async Task<ActionResult<IEnumerable<InterviewResponseDTO>>> GetByApplicationId(int applicationId)
        {
            var interviews = await _interviewService.GetByApplicationIdAsync(applicationId);
            return Ok(interviews);
        }

        [HttpGet("student/{studentId}")]
        public async Task<ActionResult<IEnumerable<InterviewResponseDTO>>> GetByStudentId(int studentId)
        {
            var interviews = await _interviewService.GetByStudentIdAsync(studentId);
            return Ok(interviews);
        }

        [HttpGet("employer/{employerId}")]
        public async Task<ActionResult<IEnumerable<InterviewResponseDTO>>> GetByEmployerId(int employerId)
        {
            var interviews = await _interviewService.GetByEmployerIdAsync(employerId);
            return Ok(interviews);
        }
        /// <summary>
        /// Update Status After Interview (SCHEDULED = 1, ACCEPTED = 2, REJECTED = 3, CANCELLED = 4, COMPLETED = 5, PENDING = 6)
        /// </summary>
        /// <param name="id"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPut("{id}/status")]
        public async Task<ActionResult<InterviewResponseDTO>> UpdateStatus(int id, [FromBody] UpdateInterviewDTO dto)
        {
            try
            {
                var result = await _interviewService.UpdateInterviewStatusAsync(id, dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Update Status Before Interview (SCHEDULED = 1, ACCEPTED = 2, REJECTED = 3, CANCELLED = 4, COMPLETED = 5, PENDING = 6)
        /// </summary>
        /// <param name="id"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPut("{id}/status-before")]
        public async Task<ActionResult<InterviewResponseDTO>> UpdateStatusBefore(int id, [FromBody] UpdateInterviewDTO dto)
        {
            try
            {
                var result = await _interviewService.UpdateInterviewStatusBeforeAsync(id, dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

    }
} 