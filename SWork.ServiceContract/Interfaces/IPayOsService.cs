

using Net.payOS.Types;
using SWork.Data.DTO.Wallet.TransactionDTO;

namespace SWork.ServiceContract.Interfaces
{
    public interface IPayOSService
    {
        Task<string> CreatePaymentLink(string userId, WalletTransactionCreateDTO model);
        Task HandlePaymentWebhook(WebhookType webhookData);

        // Gửi link cho payos để callback data về web
        Task<string> ConfirmWebhook(string url);
    }
}
