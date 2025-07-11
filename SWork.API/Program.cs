using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SWork.API.DependencyInjection;
using SWork.API.Hubs;
using SWork.Common.Helper;
using SWork.Common.Middleware;
using SWork.Data.Entities;
using SWork.Data.Models;
using SWork.Service.CloudinaryService;

namespace SWork.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add logging
            builder.Logging.AddConsole();
            builder.Logging.AddDebug();

            // Get version
            var fullVersion = Assembly.GetExecutingAssembly()
                                      .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                                      .InformationalVersion ?? "1.0.0";
            var version = fullVersion.Split('+')[0];

            // Add Controllers
            builder.Services.AddControllers();

            // Add SignalR with configuration
            builder.Services.AddSignalR(options =>
            {
                options.EnableDetailedErrors = true;
            });

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
                   .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                   .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                   .AddEnvironmentVariables();

            // Log cấu hình đã load (chỉ ở môi trường Development)
            if (builder.Environment.IsDevelopment())
            {
                var configLogger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger("Configuration");
                LogConfiguration(builder.Configuration, configLogger);
            }

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
                    policy.SetIsOriginAllowed(origin =>
                    {
                        var allowedOrigins = new[]
                        {
                            "https://student-work-fe.vercel.app",
                            "https://www.swork.website",
                            "http://localhost:3000",
                            "http://localhost:3001",
                            "https://localhost:3000",
                            "https://localhost:3001"
                        };
                        return allowedOrigins.Contains(origin) || origin.StartsWith("http://localhost:") || origin.StartsWith("https://localhost:");
                    })
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
                });
            });

            var app = builder.Build();

            // Đảm bảo database và tất cả bảng được tạo/đồng bộ theo migration
            using (var scope = app.Services.CreateScope())
            {
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                var dbContext = scope.ServiceProvider.GetRequiredService<SWorkDbContext>();
                try
                {
                    logger.LogInformation("Applying migrations (if any) to ensure database schema is up to date...");
                    dbContext.Database.Migrate();
                    logger.LogInformation("Database is up to date!");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error during database migration: {Message}", ex.Message);
                    throw;
                }
            }

            app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

            // Swagger works in ALL environments
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", $"SWork API {version}");
                c.RoutePrefix = "swagger"; // URL: /swagger
            });

            app.UseHttpsRedirection();
            app.UseCors();

            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            // Map SignalR Hub with CORS
            app.MapHub<NotificationHub>("/notificationHub")
               .RequireCors(policy => policy
                   .SetIsOriginAllowed(origin =>
                   {
                       var allowedOrigins = new[]
                       {
                           "https://student-work-fe.vercel.app",
                           "https://www.swork.website",
                           "http://localhost:3000",
                           "http://localhost:3001",
                           "https://localhost:3000",
                           "https://localhost:3001"
                       };
                       return allowedOrigins.Contains(origin) || origin.StartsWith("http://localhost:") || origin.StartsWith("https://localhost:");
                   })
                   .AllowAnyHeader()
                   .AllowAnyMethod()
                   .AllowCredentials());

            app.Run();
        }

        // Hàm log cấu hình
        private static void LogConfiguration(IConfiguration config, ILogger logger, string parentKey = "")
        {
            foreach (var child in config.GetChildren())
            {
                var key = string.IsNullOrEmpty(parentKey) ? child.Key : $"{parentKey}:{child.Key}";
                if (child.GetChildren().Any())
                {
                    LogConfiguration(child, logger, key);
                }
                else
                {
                    logger.LogInformation("{Key} = {Value}", key, child.Value);
                }
            }
        }
    }

}
