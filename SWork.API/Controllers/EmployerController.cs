using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SWork.ServiceContract.Interfaces;
using SWork.Data.DTO.EmployerDTO;
using System.Security.Claims;

namespace SWork.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployerController : ControllerBase
    {
        private readonly IEmployerService _employerService;

        public EmployerController(IEmployerService employerService)
        {
            _employerService = employerService;
        }

        /// <summary>
        /// Get employer by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEmployerById(int id)
        {
            var employer = await _employerService.GetEmployerByIdAsync(id);
            if (employer == null)
                return NotFound($"Không tìm thấy nhà tuyển dụng với ID là {id}!");

            return Ok(employer);
        }

        /// <summary>
        /// Get employer by user ID
        /// </summary>
        [HttpGet("user/{userId}")]
        [Authorize]
        public async Task<IActionResult> GetEmployerByUserId(string userId)
        {
            var employer = await _employerService.GetEmployerByUserIdAsync(userId);
            if (employer == null)
                return NotFound($"Không tìm thấy nhà tuyển dụng với USER ID là {userId}!");

            return Ok(employer);
        }

        /// <summary>
        /// Get all employers
        /// </summary>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllEmployers()
        {
            var employers = await _employerService.GetAllEmployersAsync();
            return Ok(employers);
        }

        /// <summary>
        /// Create new employer
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateEmployer([FromBody] EmployerCreateDTO employerDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? User.FindFirst("sub")?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("Không thể xác định userId từ token!");
                
                var employer = await _employerService.CreateEmployerAsync(employerDto, userId);
                return CreatedAtAction(nameof(GetEmployerById), new { id = employer.EmployerID }, employer);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Update employer information
        /// </summary>
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateEmployer(int id, [FromBody] EmployerCreateDTO employerDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userId = User.FindFirst("sub")?.Value ?? User.Identity?.Name;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("Không thể xác định userId từ token!");
                
                var employer = await _employerService.UpdateEmployerAsync(id, employerDto, userId);
                return Ok(employer);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Delete employer
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteEmployer(int id)
        {
            var result = await _employerService.DeleteEmployerAsync(id);
            if (!result)
                return NotFound($"Không tìm thấy nhà tuyển dụng với ID là {id}!");

            return Ok(new { message = "Xóa nhà tuyển dụng thành công!" });
        }

        /// <summary>
        /// Get employers by industry
        /// </summary>
        [HttpGet("industry/{industry}")]
        [Authorize]
        public async Task<IActionResult> GetEmployersByIndustry(string industry)
        {
            try
            {
                var employers = await _employerService.GetEmployersByIndustryAsync(industry);
                return Ok(employers);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Có lỗi xảy ra khi tìm kiếm nhà tuyển dụng theo ngành");
            }
        }

        [HttpGet("size/{size}")]
        [Authorize]
        public async Task<IActionResult> GetEmployersByCompanySize(string size)
        {
            var employers = await _employerService.GetEmployersByCompanySizeAsync(size);
            return Ok(employers);
        }
    }
} 