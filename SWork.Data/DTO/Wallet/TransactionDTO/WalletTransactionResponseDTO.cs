

namespace SWork.Data.DTO.Wallet.TransactionDTO
{
    public class WalletTransactionResponseDTO //Trả về thông tin giao dịch
    {
        public int TransactionID { get; set; }
        public int WalletID { get; set; }
        public decimal Amount { get; set; }
        public string TransactionType { get; set; }
        public string Description { get; set; }

        public long OrderCode { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
