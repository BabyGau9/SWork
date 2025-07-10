using SWork.Data.DTO.ReviewDTO;
using SWork.Data.Entities;
using SWork.RepositoryContract.IUnitOfWork;
using SWork.ServiceContract.Interfaces;
using AutoMapper;
using SWork.RepositoryContract.Interfaces;

namespace SWork.Service.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IReviewRepository _reviewRepository;

        public ReviewService(IUnitOfWork unitOfWork, IMapper mapper, IReviewRepository reviewRepository)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _reviewRepository = reviewRepository;
        }

        private void ValidateReview(int rating, string comment, string reviewerId, string revieweeId)
        {
            if (rating < 1 || rating > 5)
                throw new ArgumentException("Rating phải từ 1 đến 5");
            if (string.IsNullOrWhiteSpace(comment))
                throw new ArgumentException("Comment không được để trống");
            if (reviewerId == revieweeId)
                throw new ArgumentException("Reviewer và Reviewee không được trùng nhau");
        }

        public async Task<IEnumerable<ReviewDTO>> GetAllAsync()
        {
            var reviews = await _reviewRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<ReviewDTO>>(reviews);
        }

        public async Task<ReviewDTO> GetByIdAsync(int id)
        {
            var review = await _unitOfWork.GenericRepository<Review>().GetFirstOrDefaultAsync(r => r.Review_id == id);
            if (review == null) throw new KeyNotFoundException("Review không tồn tại");
            return _mapper.Map<ReviewDTO>(review);
        }

        public async Task<IEnumerable<ReviewDTO>> GetReviewsByUserIdAsync(string userId)
        {
            var reviews = await _reviewRepository.GetReviewsByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<ReviewDTO>>(reviews);
        }

        public async Task<IEnumerable<ReviewDTO>> GetReviewsByJobIdAsync(int jobId)
        {
            // 1. Lấy tất cả Application của Job này
            var applications = await _unitOfWork.GenericRepository<Application>()
                .GetAllAsync(a => a.JobID == jobId, null);
            var applicationIds = applications.Select(a => a.ApplicationID).ToList();

            // 2. Lấy tất cả Review có ApplicationID thuộc các Application vừa lấy
            var reviews = await _unitOfWork.GenericRepository<Review>()
                .GetAllAsync(r => applicationIds.Contains(r.ApplicationID ?? 0), null);

            // 3. Map sang DTO nếu cần
            return _mapper.Map<IEnumerable<ReviewDTO>>(reviews);
        }

        public async Task<ReviewDTO> CreateReviewAsync(CreateReviewDTO dto, string reviewerId)
        {
            var application = await _unitOfWork.GenericRepository<Application>().GetFirstOrDefaultAsync(a => a.ApplicationID == dto.ApplicationId, "Student,Job");
            if (application == null) throw new KeyNotFoundException("Application không tồn tại");
            if (application.Status != "FINISHED") throw new InvalidOperationException("Chỉ có thể review khi công việc đã hoàn thành");

            var job = await _unitOfWork.GenericRepository<Job>().GetFirstOrDefaultAsync(j => j.JobID == application.JobID);
            if (job == null) throw new KeyNotFoundException("Job không tồn tại");
            var employer = await _unitOfWork.GenericRepository<Employer>().GetFirstOrDefaultAsync(e => e.EmployerID == job.EmployerID);
            if (employer == null) throw new KeyNotFoundException("Employer không tồn tại");

            string revieweeId;
            if (application.Student.UserID == reviewerId)
            {
                revieweeId = employer.UserID;
            }
            else if (employer.UserID == reviewerId)
            {
                revieweeId = application.Student.UserID;
            }
            else
            {
                throw new UnauthorizedAccessException("Bạn không có quyền review cho application này");
            }

            ValidateReview(dto.Rating, dto.Comment, reviewerId, revieweeId);
            var review = new Review
            {
                Reviewer_id = reviewerId,
                Reviewee_id = revieweeId,
                ApplicationID = dto.ApplicationId,
                Rating = dto.Rating,
                Comment = dto.Comment,
                CreatedAt = DateTime.UtcNow,
                IsOpen = true
            };
            await _unitOfWork.GenericRepository<Review>().InsertAsync(review);
            await _unitOfWork.SaveChangeAsync();
            return _mapper.Map<ReviewDTO>(review);
        }

        public async Task<ReviewDTO> UpdateReviewAsync(UpdateReviewDTO dto, string reviewerId)
        {
            var review = await _unitOfWork.GenericRepository<Review>().GetFirstOrDefaultAsync(r => r.Review_id == dto.Id);
            if (review == null) throw new KeyNotFoundException("Review không tồn tại");
            if (review.Reviewer_id != reviewerId) throw new UnauthorizedAccessException("Bạn không có quyền sửa review này");
            ValidateReview(dto.Rating, dto.Comment, reviewerId, review.Reviewee_id);
            review.Rating = dto.Rating;
            review.Comment = dto.Comment;
            _unitOfWork.GenericRepository<Review>().Update(review);
            await _unitOfWork.SaveChangeAsync();
            return _mapper.Map<ReviewDTO>(review);
        }

        public async Task<bool> DeleteReviewAsync(int id, string reviewerId)
        {
            var review = await _unitOfWork.GenericRepository<Review>().GetFirstOrDefaultAsync(r => r.Review_id == id);
            if (review == null) throw new KeyNotFoundException("Review không tồn tại");
            if (review.Reviewer_id != reviewerId) throw new UnauthorizedAccessException("Bạn không có quyền xóa review này");
            _unitOfWork.GenericRepository<Review>().Delete(review);
            await _unitOfWork.SaveChangeAsync();
            return true;
        }
    }
}