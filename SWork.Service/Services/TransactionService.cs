
using SWork.Data.DTO.Wallet.TransactionDTO;

namespace SWork.Service.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public TransactionService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<WalletTransactionResponseDTO> CreateTransactionAsync(WalletTransactionCreateDTO createDto)
        {
         
            createDto.OrderCode = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(); ;
            await _unitOfWork.BeginTransactionAsync();
            try
            {
               
                var transaction = _mapper.Map<WalletTransaction>(createDto);
                transaction.CreatedAt = DateTime.Now;
                transaction.TransactionType = "PENDING";
                var transactionRepository = _unitOfWork.GenericRepository<WalletTransaction>();
                await transactionRepository.InsertAsync(transaction);
                await _unitOfWork.SaveChangeAsync(); // Lưu thay đổi
                await _unitOfWork.CommitTransactionAsync();

                return _mapper.Map<WalletTransactionResponseDTO>(transaction);
            }
            catch
            {
                await  _unitOfWork.RollbackTransactionAsync();
                throw;
            }

          
        }

        public async Task<bool> DeleteTransactionAsync(int transactionId)
        {
            var transactionRepository = _unitOfWork.GenericRepository<WalletTransaction>();
            var transaction = await transactionRepository.GetByIdAsync(transactionId);
            if (transaction == null)
            {
                return false;
            }
            transactionRepository.Delete(transaction);
            await _unitOfWork.SaveChangeAsync(); // Lưu thay đổi
            return true;
        }

        public async Task<WalletTransactionResponseDTO?> GetTransactionByIdAsync(int transactionId)
        {
            var transactionRepository = _unitOfWork.GenericRepository<WalletTransaction>();
            var transaction = await transactionRepository.GetByIdAsync(transactionId);
            return _mapper.Map<WalletTransactionResponseDTO>(transaction);
        }

        public async Task<IEnumerable<WalletTransactionResponseDTO>> GetTransactionsByWalletIdAsync(int walletId, WalletTransactionFilterDTO? filter = null)
        {
            var transactionRepository = _unitOfWork.GenericRepository<WalletTransaction>();
            IQueryable<WalletTransaction> query = transactionRepository.GetAll().Where(t => t.WalletID == walletId);

            if (filter != null)
            {
                if (!string.IsNullOrEmpty(filter.TransactionType))
                {
                    query = query.Where(t => t.TransactionType == filter.TransactionType);
                }
                if (filter.StartDate.HasValue)
                {
                    query = query.Where(t => t.CreatedAt >= filter.StartDate.Value);
                }
                if (filter.EndDate.HasValue)
                {
                    query = query.Where(t => t.CreatedAt <= filter.EndDate.Value.AddDays(1).AddTicks(-1));
                }
                if (filter.MinAmount.HasValue)
                {
                    query = query.Where(t => t.Amount >= filter.MinAmount.Value);
                }
                if (filter.MaxAmount.HasValue)
                {
                    query = query.Where(t => t.Amount <= filter.MaxAmount.Value);
                }

                // Phân trang
                query = query.Skip((filter.PageNumber - 1) * filter.PageSize).Take(filter.PageSize);
            }

            var transactions = await query.OrderByDescending(t => t.CreatedAt).ToListAsync();
            return _mapper.Map<IEnumerable<WalletTransactionResponseDTO>>(transactions);
        }
        private async Task AddTransactionInternalAsync(int walletId, decimal amount, string description, string transactionType)
        {
            var transactionRepository = _unitOfWork.GenericRepository<WalletTransaction>();
            var transaction = new WalletTransaction
            {
                WalletID = walletId,
                Amount = amount,
                TransactionType = transactionType,
                Description = description,
                CreatedAt = DateTime.Now
            };
            await transactionRepository.InsertAsync(transaction);
            // Không gọi SaveChangeAsync ở đây, vì nó sẽ được gọi ở cuối transaction của UnitOfWork
        }
    }
}
