
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;


namespace SWork.Data.Entities
{
    public class WalletTransaction
    {
        [Key]
        public int TransactionID { get; set; }
        public int WalletID { get; set; }
        public decimal Amount { get; set; }
        public string TransactionType { get; set; } // 'DEPOSIT', 'WITHDRAWAL', 'PAYMENT'
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public long? OrderCode {  get; set; }

        // Navigation properties
        [ForeignKey("WalletID")]
        public virtual Wallet Wallet { get; set; }
    }
}
