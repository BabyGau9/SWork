
using AutoMapper;
using SWork.Data.DTO.JobDTO;
using SWork.Data.DTO.SubDTO;
using SWork.Data.DTO.UserDTO;
using SWork.Data.Entities;
using SWork.Data.DTO.AuthDTO;
using SWork.Data.DTO.StudentDTO;
using SWork.Data.DTO.CVDTO;
using SWork.Data.DTO.EmployerDTO;
using SWork.Data.DTO.ApplicationDTO;
using SWork.Data.DTO.JobBookMarkDTO;
using SWork.Data.DTO.Wallet.ManagementWalletDTO;
using SWork.Data.DTO.Wallet.TransactionDTO;
using SWork.Data.DTO.InterviewDTO;

namespace SWork.Common.Helper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            //User mapper
            //CreateMap<ApplicationUser, UserProfileUpdateDTO>();

            // User
            CreateMap<UserRegisterDTO, ApplicationUser>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email)) // Sửa lỗi: ánh xạ Email từ Email
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
            .ForMember(dest => dest.AvatarUrl, opt => opt.MapFrom(src => src.AvatarUrl))
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.UtcNow)) // Tự động đặt CreatedDate
            .ForMember(dest => dest.UpdateAt, opt => opt.MapFrom(src => DateTime.UtcNow)) // Tự động đặt UpdateAt
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true)) // Mặc định IsActive = true
            .ForMember(dest => dest.Rating, opt => opt.MapFrom(src => 0.0)) // Mặc định Rating = 0
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore()) // Bỏ qua PasswordHash (xử lý riêng bằng UserManager)
            .ForMember(dest => dest.Student, opt => opt.Ignore()) // Bỏ qua mối quan hệ
            .ForMember(dest => dest.Employer, opt => opt.Ignore()) // Bỏ qua mối quan hệ
            .ForMember(dest => dest.Wallet, opt => opt.Ignore()); // Bỏ qua mối quan hệ

            CreateMap<UserRegisterDTO, ApplicationUser>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email));

            CreateMap<ApplicationUser, UserDTO>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber));

            // Student
            CreateMap<StudentCreateDTO, Student>()
            .ForMember(dest => dest.StudentID, opt => opt.Ignore()); 
            CreateMap<UserRegisterDTO, ApplicationUser>();
            CreateMap<ApplicationUser, UserResponseDTO>();
            CreateMap<Student, StudentResponseDTO>();

            CreateMap<EmployerCreateDTO, Employer>();
            CreateMap<Employer, EmployerResponseDTO>()
                .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Company_name))
                .ForMember(dest => dest.Location, opt => opt.MapFrom(src => src.Location));
            // Job
            CreateMap<CreateJobDTO, Job>().ReverseMap();

            CreateMap<CreateInterviewDTO, Interview>()       // input
           .ReverseMap();                               // ⇄ cho phép Interview -> CreateInterviewDTO

            CreateMap<Interview, InterviewDTO>();
            //Subscription
            CreateMap<SubDTO, Subscription>().ReverseMap();

            //Resume
            CreateMap<UpdateResumeDTO, Resume>() .ReverseMap();
            CreateMap<CreateResumeDTO, Resume>().ReverseMap();

            //Application
            CreateMap<RequestApplyDTO, Application>().ReverseMap();

            //BookMark
            CreateMap<MarkDTO, JobBookmark>().ReverseMap();

            //Wallet
            CreateMap<WalletCreateDTO, Wallet>() .ReverseMap();
            CreateMap<WalletResponseDTO, Wallet>().ReverseMap();

            //WalletTransaction
            CreateMap<WalletTransactionCreateDTO, WalletTransaction>().ReverseMap();
            CreateMap<WalletTransactionResponseDTO, WalletTransaction>().ReverseMap();

            CreateMap<CreateInterviewDTO, Interview>()
                .ForMember(dest => dest.ScheduledTime,
                    opt => opt.MapFrom(src => src.ScheduledTime.ToUniversalTime()))
                .ForMember(dest => dest.Status,
                    opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt,
                    opt => opt.Ignore());

        }
       
    }
}
