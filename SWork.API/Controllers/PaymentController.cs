using Microsoft.AspNetCore.Mvc;
using Net.payOS.Types;
using SWork.Data.DTO.Wallet.TransactionDTO;
using SWork.ServiceContract.Interfaces;
using System.Security.Claims;


namespace SWork.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : Controller
    { 
        private readonly IWalletService _walletService;
        private readonly IPayOSService _payOsService;
        private readonly ITransactionService _transactionService;
        private readonly INotificationService _notificationService;

        public PaymentController(ITransactionService transactionService, IPayOSService payOsService, INotificationService notificationService, IWalletService walletService)
        {
            _transactionService = transactionService;
            _payOsService = payOsService;
            _notificationService = notificationService;
            _walletService = walletService;
        }

        [HttpPost("payos/link-payment")]
        public async Task<IActionResult> CreatePaymentPayOSLink(WalletTransactionCreateDTO model)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var url = await _payOsService.CreatePaymentLink(userId, model);
                return Ok(url);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("payos/confirm-webhook")]
        public async Task<IActionResult> ConfirmWebhook(string url)
        {
            try
            {
                var confirm = await _payOsService.ConfirmWebhook(url);
                return Ok(confirm);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        [HttpPost("payos/handle-webhook")]
        public async Task<IActionResult> HandleWebhook(WebhookType webhookData)
        {
            try
            {
                await _payOsService.HandlePaymentWebhook(webhookData);
                if(webhookData.success == true)
                {
                    var amount = webhookData.data.amount;
                    long orderCode = (long)webhookData.data.orderCode;

                    
                    await _walletService.AddToWalletAsync(orderCode, $"Bạn đã nạp {amount} từ PayOS", "DEPOSIT");

                }
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }

        }
    }
}
