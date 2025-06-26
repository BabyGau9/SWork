using SWork.Common.Middleware;
using SWork.Data.DTO.JobBookMarkDTO;

namespace SWork.Service.Services
{
    public class JobBookMarkService(IUnitOfWork unitOfWork, IMapper mapper) : IJobBookMarkService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;
        public async Task<bool> AddJobBookMark(string userId, MarkDTO dto)
        {
            var student = await _unitOfWork.GenericRepository<Student>().GetFirstOrDefaultAsync(a => a.UserID == userId);
            if (student == null) throw new NotFoundException("Bạn cần đăng nhập hoặc tạo tài khoản trước khi xem mục này!");

            var job = await _unitOfWork.GenericRepository<Job>().GetFirstOrDefaultAsync(a => a.JobID == dto.JobID);
            if (job == null) throw new NotFoundException("Công việc không tồn tại!");

            dto.StudentID = student.StudentID;

            var mark = _mapper.Map<JobBookmark>(dto);

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                await _unitOfWork.GenericRepository<JobBookmark>().InsertAsync(mark);
                await _unitOfWork.SaveChangeAsync();
                await _unitOfWork.CommitTransactionAsync();

                return true;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
            
        }
        public async Task<bool> RemoveJobBookMark(string userId, int markId)
        {
            var student = await _unitOfWork.GenericRepository<Student>().GetFirstOrDefaultAsync(a => a.UserID == userId);
            if (student == null) throw new NotFoundException("Bạn cần đăng nhập hoặc tạo tài khoản trước khi xem mục này!");

            var mark = await _unitOfWork.GenericRepository<JobBookmark>().GetFirstOrDefaultAsync(a => a.BookmarkID == markId);
            if (mark == null) throw new NotFoundException("Công việc yêu thích không tồn tại!");

            if (mark.StudentID != student.StudentID) throw new ForbiddenException("Bạn không có quyền xóa công việc yêu thích này!");

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                _unitOfWork.GenericRepository<JobBookmark>().Delete(mark);
                await _unitOfWork.SaveChangeAsync();
                await _unitOfWork.CommitTransactionAsync();

                return true;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
    }
}
