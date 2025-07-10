using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Net.payOS;
using SWork.Common.Helper;
using SWork.Data.Entities;
using SWork.Service;
using SWork.Service.CloudinaryService;
using SWork.Service.Services;
using SWork.ServiceContract.ICloudinaryService;
using SWork.ServiceContract.Interfaces;
using SWork.API.Services;

namespace SWork.API.DependencyInjection
{
    public static class ServiceDI
    {
        public static IServiceCollection AddServiceDependencies(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<IAuthService, AuthService>();
            services.AddTransient<IEmailService, EmailService>();
            services.AddScoped<APIResponse>();

            // DI service entities
            services.AddTransient<ISubscriptionService, SubscriptionService>();
            services.AddTransient<IJobService, JobService>();
            services.AddTransient<IInterviewService, InterviewService>();
            services.AddTransient<IResumeService, ResumeService>();
            services.AddScoped<IStudentService, StudentService>();
            services.AddScoped<IEmployerService, EmployerService>();
            services.AddTransient<IApplicationService, ApplicationService>();
            services.AddTransient<IJobBookMarkService, JobBookMarkService>();
            services.AddTransient<IWalletService, WalletService>();
            services.AddTransient<ITransactionService, TransactionService>();
            services.AddTransient<IPayOSService, PayOSService>();
            services.AddTransient<INotificationService, NotificationService>();
            services.AddTransient<INotificationHub, NotificationHubService>();
            services.AddTransient<IReviewService, ReviewService>();
            
            // Background Services
            services.AddHostedService<InterviewReminderService>();
            
            //Cloudinary
            services.AddScoped<ICloudinaryImageService, CloudinaryImageService>();

            // Config PayOS
            services.Configure<PayOSSettings>(configuration.GetSection("PayOs"));

            services.AddSingleton(sp =>
            {
                var settings = sp.GetRequiredService<IOptions<PayOSSettings>>().Value;
                if (string.IsNullOrEmpty(settings.ClientID) ||
                    string.IsNullOrEmpty(settings.APIKey) ||
                    string.IsNullOrEmpty(settings.ChecksumKey))
                {
                    throw new InvalidOperationException("PayOS configuration is missing or incomplete");
                }
                return new PayOS(settings.ClientID, settings.APIKey, settings.ChecksumKey);
            });

            return services;
        }

    }
}
