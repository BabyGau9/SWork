
using SWork.Data.DTO.Wallet.ManagementWalletDTO;

namespace SWork.Service.Services
{
    public class WalletService : IWalletService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public WalletService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<WalletResponseDTO> CreateWalletAsync(WalletCreateDTO dto)
        {
            var walletRepository = _unitOfWork.GenericRepository<Wallet>();

            // Kiểm tra xem ví đã tồn tại cho UserID này chưa
            var existingWallet = await walletRepository.GetFirstOrDefaultAsync(w => w.UserID == dto.UserID);
            if (existingWallet != null)
            {
                throw new InvalidOperationException($"Ví cho người dùng có ID '{dto.UserID}' đã tồn tại.");
            }

            var wallet = _mapper.Map<Wallet>(dto);
            wallet.Balance = 0;
            wallet.LastUpdated = DateTime.Now;

            await walletRepository.InsertAsync(wallet);
            await _unitOfWork.SaveChangeAsync(); // Lưu thay đổi qua UnitOfWork

            return _mapper.Map<WalletResponseDTO>(wallet);
        }

        public async Task<bool> AddToWalletAsync(string userId, decimal amount, string description, string transactionType)
        {
            if (amount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), "Số tiền nạp phải là số dương.");
            }

            var walletRepository = _unitOfWork.GenericRepository<Wallet>();
            var wallet = await walletRepository.GetFirstOrDefaultAsync(w => w.UserID == userId);
            if (wallet == null)
            {
                return false;
            }

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                wallet.Balance += amount;
                wallet.LastUpdated = DateTime.Now;
                walletRepository.Update(wallet); // Đánh dấu là đã thay đổi

                // Tạo giao dịch thông qua phương thức nội bộ
                await AddTransactionInternalAsync(wallet.WalletID, amount, description, transactionType);

                await _unitOfWork.SaveChangeAsync();
                await _unitOfWork.CommitTransactionAsync();
                return true;
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return false; // Hoặc ném lại ngoại lệ
            }
        }



        public async Task<bool> DeductFromWalletAsync(string userId, decimal amount, string? description, string? transactionType)
        {
            if (amount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), "Số tiền trừ phải là số dương.");
            }

            var walletRepository = _unitOfWork.GenericRepository<Wallet>();
            var wallet = await walletRepository.GetFirstOrDefaultAsync(w => w.UserID == userId);
            if (wallet == null)
            {
                return false;
            }

            if (wallet.Balance < amount)
            {
                return false; // Không đủ số dư
            }

          
            try
            {
                wallet.Balance -= amount;
                wallet.LastUpdated = DateTime.Now;
                walletRepository.Update(wallet);

                // Tạo giao dịch thông qua phương thức nội bộ
                await AddTransactionInternalAsync(wallet.WalletID, amount, description, transactionType);
                return true;
            }
            catch (Exception)
            {
                return false; // Hoặc ném lại ngoại lệ
            }
        }

        public Task<bool> DeleteWalletAsync(int walletId)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<WalletResponseDTO>> GetAllWalletsAsync()
        {
            var walletRepository = _unitOfWork.GenericRepository<Wallet>();
            var wallets = await walletRepository.GetAll().ToListAsync(); // Sử dụng .GetAll() từ IGenericRepository và .ToListAsync() từ Linq.Async
            return _mapper.Map<IEnumerable<WalletResponseDTO>>(wallets);
        }

        public async Task<WalletResponseDTO> GetWalletByIdAsync(int walletId)
        {
            var walletRepository = _unitOfWork.GenericRepository<Wallet>();
            var wallet = await walletRepository.GetByIdAsync(walletId);
            return _mapper.Map<WalletResponseDTO>(wallet);
        }

        public async Task<WalletResponseDTO> GetWalletByUserIdAsync(string userId)
        {
            var walletRepository = _unitOfWork.GenericRepository<Wallet>();
            var wallet = await walletRepository.GetFirstOrDefaultAsync(w => w.UserID == userId);
            return _mapper.Map<WalletResponseDTO>(wallet); throw new NotImplementedException();
        }


        public async Task<WalletResponseDTO> UpdateWalletAsync(int walletId, WalletUpdateDTO updateDto)
        {
            var walletRepository = _unitOfWork.GenericRepository<Wallet>();
            var wallet = await walletRepository.GetByIdAsync(walletId);
            if (wallet == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy ví với ID: {walletId}");
            }

            decimal oldBalance = wallet.Balance;
            wallet.Balance = updateDto.Balance;
            wallet.LastUpdated = DateTime.Now;

            _unitOfWork.GenericRepository<Wallet>().Update(wallet); // Cập nhật entity qua repository

            // Ghi nhận một giao dịch cho việc điều chỉnh thủ công này
            var transactionType = "MANUAL_ADJUSTMENT";
            var amountChange = updateDto.Balance - oldBalance;

            // Bắt đầu một transaction của UnitOfWork để đảm bảo tính nhất quán
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                if (amountChange > 0)
                {
                    // Call internal method to add transaction (ensure it uses the same UoW transaction)
                    await AddTransactionInternalAsync(wallet.WalletID, amountChange, updateDto.Description ?? "Điều chỉnh tăng số dư ví thủ công.", transactionType);
                }
                else if (amountChange < 0)
                {
                    // Call internal method to add transaction
                    await AddTransactionInternalAsync(wallet.WalletID, Math.Abs(amountChange), updateDto.Description ?? "Điều chỉnh giảm số dư ví thủ công.", transactionType);
                }

                await _unitOfWork.SaveChangeAsync(); // Lưu cả Wallet và WalletTransaction
                await _unitOfWork.CommitTransactionAsync();
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw; // Ném lại lỗi để caller xử lý
            }

            return _mapper.Map<WalletResponseDTO>(wallet);
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
