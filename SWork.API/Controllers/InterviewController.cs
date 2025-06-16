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

        //[HttpPut("{id}")]
        //[Authorize(Roles = "Employer")]
        //public async Task<ActionResult<InterviewDTO>> UpdateInterview(int id, UpdateInterviewDTO dto)
        //{
        //    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        //    var interview = await _interviewService.UpdateInterviewAsync(id, dto, userId);
        //    return Ok(interview);
        //}

        //[HttpGet("{id}")]
        //public async Task<ActionResult<InterviewDTO>> GetInterview(int id)
        //{
        //    var interview = await _interviewService.GetInterviewByIdAsync(id);
        //    return Ok(interview);
        //}

        //[HttpPost("{id}/cancel")]
        //[Authorize(Roles = "Employer")]
        //public async Task<ActionResult> CancelInterview(int id)
        //{
        //    var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        //    var result = await _interviewService.CancelInterviewAsync(id, userId);
        //    return Ok(result);
        //}

        //[HttpPost("{id}/accept")]
        //[Authorize(Roles = "Student")]
        //public async Task<ActionResult> AcceptInterview(int id)
        //{
        //    var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        //    var result = await _interviewService.AcceptInterviewAsync(id, userId);
        //    return Ok(result);
        //}

        //[HttpPost("{id}/reject")]
        //[Authorize(Roles = "Student")]
        //public async Task<ActionResult> RejectInterview(int id)
        //{
        //    var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        //    var result = await _interviewService.RejectInterviewAsync(id, userId);
        //    return Ok(result);
        //}

        //[HttpGet("student")]
        //[Authorize(Roles = "Student")]
        //public async Task<ActionResult> GetInterviewsForStudent([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        //{
        //    var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        //    var (interviews, totalCount) = await _interviewService.GetInterviewsForStudentAsync(userId, pageNumber, pageSize);
        //    return Ok(new { interviews, totalCount });
        //}

        //[HttpGet("employer")]
        //[Authorize(Roles = "Employer")]
        //public async Task<ActionResult> GetInterviewsForEmployer([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        //{
        //    var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        //    var (interviews, totalCount) = await _interviewService.GetInterviewsForEmployerAsync(userId, pageNumber, pageSize);
        //    return Ok(new { interviews, totalCount });
        //}

        //[HttpGet("application/{applicationId}")]
        //[Authorize(Roles = "Employer")]
        //public async Task<ActionResult> GetInterviewsForApplication(int applicationId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        //{
        //    var (interviews, totalCount) = await _interviewService.GetInterviewRelatedApplicationForEmployerAsync(applicationId, pageNumber, pageSize);
        //    return Ok(new { interviews, totalCount });
        //}
    }
} 