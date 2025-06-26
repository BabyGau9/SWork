using Microsoft.AspNetCore.Mvc;
using Net.payOS.Types;
using SWork.Data.DTO.Wallet.TransactionDTO;
using SWork.ServiceContract.Interfaces;
using SWork.Data.DTO.NotificationDTO;

namespace SWork.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : Controller
    {
        private readonly IPayOSService _payOsService;
        private readonly ITransactionService _transactionService;
        private readonly INotificationService _notificationService;

        public PaymentController(ITransactionService transactionService, IPayOSService payOsService, INotificationService notificationService)
        {
            _transactionService = transactionService;
            _payOsService = payOsService;
            _notificationService = notificationService;
        }

        [HttpPost("payos/link-payment")]
        public async Task<IActionResult> CreatePaymentPayOSLink(WalletTransactionCreateDTO model)
        {
            try
            {
                var url = await _payOsService.CreatePaymentLink(model);
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
                
                //// Gửi notification dựa trên kết quả thanh toán
                //if (webhookData.Status == "SUCCESS")
                //{
                //    var notificationDto = new CreateNotificationDTO
                //    {
                //        UserID = webhookData.CustomerId, // Giả sử CustomerId là UserID
                //        Title = "Thanh toán thành công",
                //        Message = $"Giao dịch {webhookData.TransactionId} đã được xử lý thành công với số tiền {webhookData.Amount:N0} VND"
                //    };
                //    await _notificationService.CreateNotificationAsync(notificationDto);
                //}
                //else if (webhookData.Status == "FAILED")
                //{
                //    var notificationDto = new CreateNotificationDTO
                //    {
                //        UserID = webhookData.CustomerId,
                //        Title = "Thanh toán thất bại",
                //        Message = $"Giao dịch {webhookData.TransactionId} đã thất bại. Vui lòng thử lại."
                //    };
                //    await _notificationService.CreateNotificationAsync(notificationDto);
                //}
                
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }

        }
    }
}
