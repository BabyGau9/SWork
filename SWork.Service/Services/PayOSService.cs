

// create an alias for the PayOs class from the Net . payOs namesapce
using Net.payOS;
using Net.payOS.Types;
using SWork.Data.DTO.Wallet.TransactionDTO;

namespace SWork.Service.Services
{
    public class PayOSService : IPayOSService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly PayOS _payOS;
        private readonly ITransactionService _transactionService;
        private readonly IMapper _mapper;

        public PayOSService(IUnitOfWork unitOfWork, PayOS payOS, ITransactionService transactionService, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _payOS = payOS;
            _transactionService = transactionService;
            _mapper = mapper;
        }

        public async Task<string> ConfirmWebhook(string url)
        {
            try
            {
                return await _payOS.confirmWebhook(url);

            }
            catch (Exception exception)
            {

                Console.WriteLine(exception);
                return exception.Message;
            }
        }

        public async Task<string> CreatePaymentLink(string userID, WalletTransactionCreateDTO model)
        {
            try
            {
                var description = "SWork website";
                var transaction = await _transactionService.CreateTransactionAsync(model);

                if (transaction == null)
                {
                    throw new Exception("Tạo giao dịch không thành công.");
                }

                //Tạo thông tin sản phẩm để hiển thị ở web thanh toán
                ItemData item = new ItemData(model.SubscriptionName, 1, (int)model.Amount);

                List<ItemData> items = new List<ItemData> { item };

                var baseUrl = "https://www.swork.website/job/add"; // show giao dien khi giao dich success/fail

                PaymentData paymentData = new PaymentData(orderCode: transaction.OrderCode,
                                                            amount: item.price,
                                                            description: description,
                                                            items,
                                                            $"{baseUrl}",
                                                            $"{baseUrl}"
                                                         );
                CreatePaymentResult createPayment = await _payOS.createPaymentLink(paymentData);
                return createPayment.checkoutUrl;
            }
            catch
            {
                throw;
            }
        }

        public async Task HandlePaymentWebhook(WebhookType webhookData)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                WebhookData data = _payOS.verifyPaymentWebhookData(webhookData);

                var transactionRepo = _unitOfWork.GenericRepository<WalletTransaction>();

                var transaction = await _unitOfWork.GenericRepository<WalletTransaction>()
                                                   .GetFirstOrDefaultAsync(_ => _.OrderCode == data.orderCode);
                if (transaction is null || transaction.TransactionType == "SUCCESS")
                {
                    return;
                }
                transaction.TransactionType = webhookData.success ? "SUCCESS" : "FAIL";

                transactionRepo.Update(transaction);

                await _unitOfWork.SaveChangeAsync();
                await _unitOfWork.CommitTransactionAsync();
                // return transactionDTO;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }



    }
}
