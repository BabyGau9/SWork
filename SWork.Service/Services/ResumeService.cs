

using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace SWork.Service.Services
{
    public class ResumeService(IUnitOfWork unitOfWork, IMapper mapper) : IResumeService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;
        public async Task<Resume> CreateResumeAsync(CreateResumeDTO resumDto, string userID)
        {
            var student = await _unitOfWork.GenericRepository<Student>().GetFirstOrDefaultAsync(a => a.UserID == userID);
            if (student == null) throw new Exception("Bạn cần đăng nhập hoặc đang kí trước khi tạo CV.");

            resumDto.StudentID = student.StudentID;

            var resum = _mapper.Map<Resume>(resumDto);
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                await _unitOfWork.GenericRepository<Resume>().InsertAsync(resum);
                await _unitOfWork.SaveChangeAsync();
                await _unitOfWork.CommitTransactionAsync();
                return resum;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw new Exception($"Error creating resume: {ex.Message}", ex);
            }
        }

        public async Task DeleteResumeAsync(int resumId, string userId)
        {
            var student = await _unitOfWork.GenericRepository<Student>().GetFirstOrDefaultAsync(a => a.UserID == userId);
            if (student == null) throw new Exception("Bạn cần đăng nhập hoặc đang kí trước khi tạo CV.");

            var studentExsit = await _unitOfWork.GenericRepository<Resume>().GetFirstOrDefaultAsync(a => a.StudentID == student.StudentID);
            if (studentExsit == null) throw new Exception("Bạn không có quyền xóa CV này.");

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var resume = await _unitOfWork.GenericRepository<Resume>().GetByIdAsync(resumId);
                if (resume == null)
                    throw new Exception("Resume not found");

                _unitOfWork.GenericRepository<Resume>().Delete(resume);
                await _unitOfWork.SaveChangeAsync();
                await _unitOfWork.CommitTransactionAsync();
                //Hub
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }



        public async Task<Resume> GetResumeByIdAsync(int resumId)
        {
            var resum = await _unitOfWork.GenericRepository<Resume>().GetByIdAsync(resumId);
            if (resum == null)
                throw new Exception("Resume not found");
            return resum;
        }

        public async Task<Pagination<Resume>> SearchResumeAsync(string? nameResume, int? studentId, int pageIndex, int pageSize)
        {

            Expression<Func<Resume, bool>> predicate = resume =>
                (string.IsNullOrEmpty(nameResume) || resume.ResumeType.Contains(nameResume)) &&
                 (!studentId.HasValue || resume.StudentID == studentId.Value);

            var result = await _unitOfWork.GenericRepository<Resume>().GetPaginationAsync(
                predicate: predicate,
                includeProperties: "Student",
                pageIndex: pageIndex,
                pageSize: pageSize
           );
            return result;
        }

        public async Task<Resume> UpdateResumeAsync(int id, UpdateResumeDTO resumDto, string userId)
        {

            var student = await _unitOfWork.GenericRepository<Student>().GetFirstOrDefaultAsync(a => a.UserID == userId);
            if (student == null) throw new Exception("Bạn cần đăng nhập hoặc đang kí trước khi tạo CV.");

            var studentExsit = await _unitOfWork.GenericRepository<Resume>().GetFirstOrDefaultAsync(a => a.StudentID == student.StudentID);
            if (studentExsit == null) throw new Exception("Bạn không có quyền chỉnh sửa CV này.");

            var existingResume = await _unitOfWork.GenericRepository<Resume>().GetFirstOrDefaultAsync(a => a.ResumeID == id);
            if (existingResume == null)
                throw new Exception("CV không tồn tại.");

            if (!string.IsNullOrWhiteSpace(resumDto.FullName))
                existingResume.FullName = resumDto.FullName;

            if (!string.IsNullOrWhiteSpace(resumDto.JobTitle))
                existingResume.JobTitle = resumDto.JobTitle;

            if (!string.IsNullOrWhiteSpace(resumDto.FileURL))
                existingResume.FileURL = resumDto.FileURL;

            if (!string.IsNullOrWhiteSpace(resumDto.Email))
                existingResume.Email = resumDto.Email;

            if (!string.IsNullOrWhiteSpace(resumDto.PhoneNumber))
                existingResume.PhoneNumber = resumDto.PhoneNumber;

            if (!string.IsNullOrWhiteSpace(resumDto.Address))
                existingResume.Address = resumDto.Address;

            if (!string.IsNullOrWhiteSpace(resumDto.Introduction))
                existingResume.Introduction = resumDto.Introduction;

            if (!string.IsNullOrWhiteSpace(resumDto.Education))
                existingResume.Education = resumDto.Education;

            if (!string.IsNullOrWhiteSpace(resumDto.Experience))
                existingResume.Experience = resumDto.Experience;

            if (!string.IsNullOrWhiteSpace(resumDto.Skills))
                existingResume.Skills = resumDto.Skills;

            if (!string.IsNullOrWhiteSpace(resumDto.Languages))
                existingResume.Languages = resumDto.Languages;

            if (!string.IsNullOrWhiteSpace(resumDto.Awards))
                existingResume.Awards = resumDto.Awards;

            if (!string.IsNullOrWhiteSpace(resumDto.Certificates))
                existingResume.Certificates = resumDto.Certificates;

            existingResume.UpdatedAt = DateTime.Now;


            await _unitOfWork.BeginTransactionAsync();
            try
            {


                _unitOfWork.GenericRepository<Resume>().Update(existingResume);
                await _unitOfWork.SaveChangeAsync();
                await _unitOfWork.CommitTransactionAsync();
                return existingResume;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
        public async Task<Pagination<Resume>> GetPaginatedResumeAsync(
            int pageIndex,
            int pageSize,
            Expression<Func<Resume, bool>>? predicate = null,
            Expression<Func<Resume, object>>? orderBy = null,
            bool isDescending = false)
        {
            try
            {
                var result = await _unitOfWork.GenericRepository<Resume>().GetPaginationAsync(
                    predicate = predicate,
                    includeProperties: "Student,ResumeTemplate",
                    pageIndex: pageIndex,
                    pageSize: pageSize,
                    orderBy: r => r.ResumeID,
                    isDescending: isDescending);

                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
