using System.ComponentModel.DataAnnotations;

namespace SWork.Data.DTO.Wallet.TransactionDTO
{
    public class WalletTransactionCreateDTO //Tạo 1 giao dịch mới
    {
        [Required]
        public int WalletID { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
        public decimal Amount { get; set; }
        [Required]
        public string SubscriptionName { get; set; }
        [Required]
        [StringLength(50)]
        public string TransactionType { get; set; } // 'DEPOSIT', 'WITHDRAWAL', 'PAYMENT_JOB_POST'

        [StringLength(500)]
        public string Description { get; set; }

        [StringLength(100)]
        public string BuyerName {  get; set; }

        public long OrderCode { get; set; }
    }
}
