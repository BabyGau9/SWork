using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Reflection;
using System.Text;
using Microsoft.OpenApi.Models;
using SWork.Data.Entities;
using SWork.Data.Models;
using SWork.Common.Helper;
using SWork.API.DependencyInjection;
using SWork.Service.CloudinaryService;
using Microsoft.Extensions.Configuration;
using SWork.Common.Middleware;
using SWork.API.Hubs;

namespace SWork.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            // Get version
            var fullVersion = Assembly.GetExecutingAssembly()
                                      .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                                      .InformationalVersion ?? "1.0.0";
            var version = fullVersion.Split('+')[0];

            // Add Controllers
            builder.Services.AddControllers();

            // Add SignalR
            builder.Services.AddSignalR();

            // DbContext
            builder.Services.AddDbContext<SWorkDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Email Settings
            builder.Services.Configure<EmailSetting>(builder.Configuration.GetSection("EmailSettings"));

            // Repositories & Services
            builder.Services.AddSWorkDependencies(builder.Configuration);

            //Config handle execption
            builder.Services.AddTransient<GlobalExceptionHandlerMiddleware>();

            // Identity
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 6;

                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<SWorkDbContext>()
            .AddDefaultTokenProviders();

            // JWT Authentication
            var jwtSecret = builder.Configuration["JWT:Key"];
            if (string.IsNullOrEmpty(jwtSecret))
            {
                throw new ArgumentNullException(nameof(jwtSecret), "JWT Secret cannot be null or empty.");
            }

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
                };

                // Configure JWT Bearer for SignalR
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/notificationHub"))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            builder.Services.AddAuthorization();

            // AutoMapper
            builder.Services.AddAutoMapper(typeof(MappingProfile));

            // Swagger config
            builder.Services.AddSwaggerGen(opt =>
            {
                opt.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "SWork API - " + version,
                    Version = version
                });

                opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: 'Bearer {token}'",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer"
                });

                opt.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });

            });

            // config appsettings.Development
            builder.Configuration
                   .SetBasePath(Directory.GetCurrentDirectory())
                   .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                   .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                   .AddEnvironmentVariables();

            // config CloundinarySettings
            builder.Services.Configure<CloudinarySettings>(
                builder.Configuration.GetSection("CloudinarySettings"));

            // --- Registering PayOSSettings ---
            builder.Services.Configure<PayOSSettings>(builder.Configuration.GetSection(PayOSSettings.SectionName));

            // --- Registering AppConfig ---
            builder.Services.Configure<AppConfig>(builder.Configuration.GetSection(AppConfig.SectionName));


            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.WithOrigins("http://localhost:3000")
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                });
            });

            var app = builder.Build();

            app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

            // Swagger works in ALL environments
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", $"SWork API {version}");
                c.RoutePrefix = "swagger"; // URL: /swagger
            });

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseCors();
            app.MapControllers();

            // Map SignalR Hub
            app.MapHub<NotificationHub>("/notificationHub");

            app.Run();
        }
    }
}
