using SWork.Repository.Repository;
using SWork.Repository.UnitOfWork;
using SWork.RepositoryContract.Interfaces;
using SWork.RepositoryContract.IUnitOfWork;


namespace SWork.API.DependencyInjection
{
    public static class RepositoryDI
    {
        public static IServiceCollection AddRepositoryDependencies(this IServiceCollection services)
        {

            //DI Helpers
            services.AddTransient(typeof(IRepository<>), typeof(Repository<>));
            services.AddTransient<IEmailRepository, EmailRepository>();
            services.AddTransient<IUserRepository, UserRepository>();
            services.AddTransient<IRefreshTokenRepository, RefreshTokenRepository>();

            // DI Entities
            services.AddTransient<IStudentRepository, StudentRepository>();
            services.AddTransient<IJobRepository, JobRepository>();
            services.AddTransient<ISubscriptionRepository, SubscriptionRepository>();
            services.AddTransient<IResumeRepository, ResumeRepository>();
            services.AddTransient<IStudentRepository, StudentRepository>();
            services.AddTransient<IEmployerRepository, EmployerRepository>();
            services.AddTransient<IResumeRepository, ResumeRepository>();
            services.AddTransient<IApplicationRepository, ApplicationRepository>();
            services.AddTransient<IJobBookMarkRepository, JobBookMarkRepository>();
            services.AddTransient<IInterviewRepository, InterviewReposiory>();
            services.AddTransient<IReviewRepository, ReviewRepository>();
            services.AddTransient<INotificationRepository, NotificationRepository>();

            // DI UnitOfWork
            services.AddTransient<IUnitOfWork, UnitOfWork>();
            return services;
        }
    }
}
