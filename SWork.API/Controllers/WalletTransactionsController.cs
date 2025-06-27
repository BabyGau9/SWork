using Microsoft.AspNetCore.Mvc;
using SWork.Data.DTO.Wallet.TransactionDTO;
using SWork.ServiceContract.Interfaces;

namespace SWork.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WalletTransactionsController : ControllerBase
    {
        private readonly ITransactionService _transactionService;

        public WalletTransactionsController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateTransaction([FromBody] WalletTransactionCreateDTO dto)
        {
            var result = await _transactionService.CreateTransactionAsync(dto);
            return CreatedAtAction(nameof(GetTransactionById), new { transactionId = result.TransactionID }, result);
        }

        // DELETE: api/WalletTransactions/5
        [HttpDelete("{transactionId}")]
        public async Task<IActionResult> DeleteTransaction(int transactionId)
        {
            var success = await _transactionService.DeleteTransactionAsync(transactionId);
            return success ? Ok(new { message = "Xóa giao dịch thành công!" }) : NotFound();
        }

        // GET: api/WalletTransactions/5
        [HttpGet("{transactionId}")]
        public async Task<IActionResult> GetTransactionById(int transactionId)
        {
            var result = await _transactionService.GetTransactionByIdAsync(transactionId);
            return result != null ? Ok(result) : NotFound();
        }

        // GET: api/WalletTransactions/wallet/3?TransactionType=SUCCESS&MinAmount=100000
        [HttpGet("wallet/{walletId}")]
        public async Task<IActionResult> GetTransactionsByWalletId(int walletId, [FromQuery] WalletTransactionFilterDTO filter)
        {
            var transactions = await _transactionService.GetTransactionsByWalletIdAsync(walletId, filter);
            return Ok(transactions);
        }

    }
}