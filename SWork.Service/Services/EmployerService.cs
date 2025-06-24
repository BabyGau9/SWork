using Microsoft.AspNetCore.Identity;
using SWork.Data.DTO.EmployerDTO;
using SWork.RepositoryContract.Basic;

namespace SWork.Service
{
    public class EmployerService : IEmployerService
    {
        private readonly IGenericRepository<Employer> _employerRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private const string DefaultIncludes = "User,Jobs";
        public EmployerService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
            _employerRepository = _unitOfWork.GenericRepository<Employer>();
        }

        public async Task<EmployerResponseDTO> GetEmployerByIdAsync(int id)
        {
            var employer = await _unitOfWork.GenericRepository<Employer>().GetByIdAsync(id);
            if (employer == null)
                return null;

            return _mapper.Map<EmployerResponseDTO>(employer);
        }

        public async Task<EmployerResponseDTO> GetEmployerByUserIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException($"Không tìm thấy người dùng với USER ID {userId}!");

            if (!user.EmailConfirmed)
                throw new InvalidOperationException("Email người dùng chưa được xác nhận!");

            var employer = await _employerRepository.GetFirstOrDefaultAsync(
                s => s.UserID == userId
                );
            if (employer == null)
                throw new KeyNotFoundException($"Không tìm thấy nhà tuyển dụng với USER ID {userId}!");

            return _mapper.Map<EmployerResponseDTO>(employer);
        }

        public async Task<IEnumerable<EmployerResponseDTO>> GetAllEmployersAsync()
        {
            var employers = await _employerRepository.GetAllAsync(e => true,DefaultIncludes);
            return employers.Select(e => _mapper.Map<EmployerResponseDTO>(e));
        }

        public async Task<EmployerResponseDTO> CreateEmployerAsync(EmployerCreateDTO employerDto, string userId)
        {
            // Validate input
            if (employerDto == null)
                throw new ArgumentNullException(nameof(employerDto));

            if (string.IsNullOrWhiteSpace(employerDto.CompanyName))
                throw new ArgumentException("Không được để trống tên công ty!");

            // Validate user
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException($"Không tìm thấy người dùng với USER ID {userId}!");

            if (!user.EmailConfirmed)
                throw new InvalidOperationException("Email người dùng chưa được xác nhận!");

            var userRoles = await _userManager.GetRolesAsync(user);
            if (!userRoles.Contains("Employer"))
                throw new InvalidOperationException("Người dùng không có vai trò là nhà tuyển dụng!");

            // Check if employer already exists
            var existingEmployer = await _employerRepository.GetFirstOrDefaultAsync(e => e.UserID == userId);
            if (existingEmployer != null)
                throw new InvalidOperationException("Employer already exists for this user");

            // Create new employer
            var employer = new Employer
            {
                UserID = userId,
                Company_name = employerDto.CompanyName,
                Industry = employerDto.Industry ?? string.Empty,
                CompanySize = employerDto.CompanySize ?? string.Empty,
                Website = employerDto.Website ?? string.Empty,
                Description = employerDto.Description ?? string.Empty,
                LogoUrl = string.Empty,
                Location = employerDto.Location ?? string.Empty
            };

            await _employerRepository.InsertAsync(employer);
            await _unitOfWork.SaveChangeAsync();

            return _mapper.Map<EmployerResponseDTO>(employer);
        }
        public async Task<EmployerResponseDTO> UpdateEmployerAsync(int id, EmployerCreateDTO employerDto, string userId)
        {
            // Verify user exists and is confirmed
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException($"Người dùng không tồn tại User ID {userId} !");

            if (!user.EmailConfirmed)
                throw new InvalidOperationException("Email người dùng chưa xác nhận!");

            var employer = await _employerRepository.GetByIdAsync(id);
            if (employer == null)
                throw new KeyNotFoundException($"Nhà tuyển dụng không tồn tại với ID {id}!");

            if (employer.UserID != userId)
                throw new UnauthorizedAccessException("Không có quyền truy cập!");

            _mapper.Map(employerDto, employer);
            _employerRepository.Update(employer);
            await _unitOfWork.SaveChangeAsync();

            return _mapper.Map<EmployerResponseDTO>(employer);
        }

        public async Task<bool> DeleteEmployerAsync(int id)
        {
            var employer = await _employerRepository.GetByIdAsync(id);
            if (employer == null)
                throw new KeyNotFoundException($"Không có nhà tuyển dụng với ID {id}!");

            _employerRepository.Delete(employer);
            await _unitOfWork.SaveChangeAsync();

            return true;
        }

        public async Task<IEnumerable<EmployerResponseDTO>> GetEmployersByIndustryAsync(string industry)
        {
            if (string.IsNullOrWhiteSpace(industry))
                throw new ArgumentException("Ngành nghề không được để trống!");

            var employers = await _employerRepository.GetAllAsync(
                e => e.Industry.ToLower().Contains(industry.ToLower()),
                DefaultIncludes
            );
            return employers.Select(e => _mapper.Map<EmployerResponseDTO>(e));
        }

        public async Task<IEnumerable<EmployerResponseDTO>> GetEmployersByCompanySizeAsync(string size)
        {
            if (string.IsNullOrWhiteSpace(size))
                throw new ArgumentException("Quy mô không được để trống!");

            var employers = await _employerRepository.GetAllAsync(
                e => e.CompanySize.ToLower().Contains(size.ToLower()),
                DefaultIncludes
            );
            return employers.Select(e => _mapper.Map<EmployerResponseDTO>(e));
        }
    }
} 