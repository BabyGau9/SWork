using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SWork.Data.DTO.JobDTO;
using SWork.ServiceContract.Interfaces;
using System.Security.Claims;

namespace SWork.API.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class JobController : Controller
    {
        private readonly IJobService _jobService;

        public JobController(IJobService jobService)
        {
            _jobService = jobService;
        }
        [HttpGet("pagination")]
        public async Task<IActionResult> GetPaginatedJobs(int pageIndex = 1, int pageSize = 10)
        {
            try
            {
                var jobs = await _jobService.GetPaginatedJobAsync(pageIndex, pageSize);
                return Ok(jobs);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("getById/{id}")]
        public async Task<IActionResult> GetJobById(int id)
        {
            try
            {
                var job = await _jobService.GetJobByIdAsync(id);
                return Ok(job);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("create")]
        [Authorize(Roles = "Employer")]
        public async Task<IActionResult> CreateJob([FromForm] CreateJobDTO dto)
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            try
            {
                await _jobService.CreateJobAsync(dto, userId);
                return Ok("Job created successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("update/{id}")]
        [Authorize(Roles = "Employer")]
        public async Task<IActionResult> UpdateJob(int id, [FromForm] UpdateJobDTO dto)
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            try
            {
                await _jobService.UpdateJobAsync(id, dto, userId);

                return Ok("Job updated successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpDelete("delete/{id}")]
        [Authorize(Roles = "Employer")]
        public async Task<IActionResult> DeleteJob(int id)
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            try
            {
                await _jobService.DeleteJobAsync(id, userId);
                return Ok("Job deleted successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("employer-jobs")]
        [Authorize(Roles = "Employer")]
        public async Task<IActionResult> GetEmployerJobs(int pageIndex = 1, int pageSize = 10)
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            try
            {
                var jobs = await _jobService.GetJobsByEmployerIdAsync(userId, pageIndex, pageSize);
                return Ok(jobs);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
