using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SWork.Data.Entities;
using SWork.Data.Enum;
using SWork.RepositoryContract.Interfaces;
using SWork.ServiceContract.Interfaces;
using SWork.Data.DTO.NotificationDTO;

namespace SWork.Service.Services
{
    public class InterviewReminderService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<InterviewReminderService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(5); // Kiểm tra mỗi 5 phút
        private readonly TimeSpan _reminderTime = TimeSpan.FromMinutes(30); // Nhắc nhở 30 phút trước

        public InterviewReminderService(
            IServiceProvider serviceProvider,
            ILogger<InterviewReminderService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Interview Reminder Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckUpcomingInterviews();
                    await Task.Delay(_checkInterval, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in Interview Reminder Service");
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
            }
        }

        private async Task CheckUpcomingInterviews()
        {
            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            try
            {
                var now = DateTime.Now;
                var reminderTime = now.Add(_reminderTime);

                // Lấy danh sách interviews sắp diễn ra trong 30 phút
                var upcomingInterviews = await unitOfWork.GenericRepository<Interview>()
                    .GetAllAsync(i => 
                        i.Status == InterviewStatus.SCHEDULED &&
                        i.ScheduledTime >= now &&
                        i.ScheduledTime <= reminderTime ,
                        //&&!i.ReminderSent, // Giả sử có field này để track
                        null);

                foreach (var interview in upcomingInterviews)
                {
                    await SendInterviewReminder(interview, notificationService, unitOfWork);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking upcoming interviews");
            }
        }

        private async Task SendInterviewReminder(Interview interview, INotificationService notificationService, IUnitOfWork unitOfWork)
        {
            try
            {
                // Lấy thông tin application và student
                var application = await unitOfWork.GenericRepository<Application>()
                    .GetFirstOrDefaultAsync(a => a.ApplicationID == interview.ApplicationID);
                
                if (application == null) return;

                var student = await unitOfWork.GenericRepository<Student>()
                    .GetFirstOrDefaultAsync(s => s.StudentID == application.StudentID);
                
                if (student == null) return;

                // Gửi notification nhắc nhở
                var notificationDto = new CreateNotificationDTO
                {
                    UserID = student.UserID,
                    Title = "Nhắc nhở phỏng vấn",
                    Message = $"Bạn có cuộc phỏng vấn trong 30 phút tại {interview.Location}. Link: {interview.MeetingLink}"
                };

                await notificationService.CreateNotificationAsync(notificationDto);

                // Đánh dấu đã gửi nhắc nhở (nếu có field này)
                // interview.ReminderSent = true;
                // await unitOfWork.GenericRepository<Interview>().Update(interview);
                // await unitOfWork.SaveChangeAsync();

                _logger.LogInformation($"Sent interview reminder for interview {interview.InterviewID}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending reminder for interview {interview.InterviewID}");
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Interview Reminder Service stopped");
            await base.StopAsync(cancellationToken);
        }
    }
} 