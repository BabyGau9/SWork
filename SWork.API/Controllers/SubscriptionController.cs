using Microsoft.AspNetCore.Mvc;
using SWork.Data.DTO.SubDTO;
using SWork.ServiceContract.Interfaces;

namespace SWork.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubscriptionController : ControllerBase
    {
        private readonly ISubscriptionService _subscriptionService;

        public SubscriptionController(ISubscriptionService subscriptionService)
        {
            _subscriptionService = subscriptionService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _subscriptionService.GetAllSubscriptionAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _subscriptionService.GetSubscriptionByIdAsync(id);
            return Ok(result);
        }

        [HttpGet("search")]
        public async Task<IActionResult> GetByName([FromQuery] string name)
        {
            var result = await _subscriptionService.GetSubscriptionByName(name);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SubDTO dto)
        {
            await _subscriptionService.CreateSubscriptionAsync(dto);
            return Ok("Đã tạo thành công gói đăng ký!");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] SubDTO dto)
        {
            var existing = await _subscriptionService.GetSubscriptionByIdAsync(id);
            if (existing == null)
                return NotFound("Không tìm thấy gói đăng ký!");

            // Map manually or use AutoMapper before update
            existing.SubscriptionName = dto.SubscriptionName;
            existing.Description = dto.Description;
            existing.Price = dto.Price;
            existing.DurationDays = dto.DurationDays;
            existing.IsActive = dto.IsActive;

            await _subscriptionService.UpdateSubscriptionAsync(existing);
            return Ok("đã cập nhật thành công gói đăng ký!");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _subscriptionService.DeleteSubscriptionAsync(id);
            return Ok("Đã xóa thành công gói đăng ký!");
        }
    }
}
