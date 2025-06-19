using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SWork.Data.DTO.Wallet.ManagementWalletDTO;
using SWork.ServiceContract.Interfaces;

namespace SWork.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WalletsController : ControllerBase
    {
        private readonly IWalletService _walletService;

        public WalletsController(IWalletService walletService)
        {
            _walletService = walletService;
        }

        [HttpGet]
        [Authorize(Roles ="Admin")]
        public async Task<IActionResult> GetAllWallets()
        {
            var wallets = await _walletService.GetAllWalletsAsync();
            return Ok(wallets);
        }

        [HttpGet("{walletId}")]
        [Authorize(Roles ="Admin,Employer")]
        public async Task<IActionResult> GetWalletById(int walletId)
        {
            var wallet = await _walletService.GetWalletByIdAsync(walletId);
            return Ok(wallet);
        }
        [HttpGet("user/{userId}")]

        [Authorize(Roles = "Admin,Employer")]
        public async Task<IActionResult> GetWalletByUserId(string userId)
        {
            var wallet = await _walletService.GetWalletByUserIdAsync(userId);
            return Ok(wallet);
        }

        [HttpPut("{walletId}")]
        [Authorize(Roles = "Admin")] // Giới hạn quyền
        public async Task<IActionResult> UpdateWallet(int walletId, [FromBody] WalletUpdateDTO dto)
        {
            var updated = await _walletService.UpdateWalletAsync(walletId, dto);
            return Ok(updated);
        }


    }
}
