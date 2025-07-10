using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SWork.Data.Entities;

namespace SWork.Data.Models
{
    public class SWorkDbContext : IdentityDbContext<ApplicationUser>
    {
        public SWorkDbContext()
        {
        }

        public SWorkDbContext(DbContextOptions<SWorkDbContext> options) : base(options)
        {
        }

        public virtual DbSet<ApplicationUser> ApplicationUsers { get; set; } = null!;
        public DbSet<Student> Students { get; set; }
        public DbSet<Employer> Employers { get; set; }
        public DbSet<Resume> Resumes { get; set; }
        public DbSet<Job> Jobs { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<Application> Applications { get; set; }
        public DbSet<Interview> Interviews { get; set; }
        public DbSet<JobBookmark> JobBookmarks { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<WalletTransaction> WalletTransactions { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed Roles
            modelBuilder.Entity<IdentityRole>().HasData(
                new IdentityRole { Id = Guid.NewGuid().ToString(), Name = "Admin", NormalizedName = "ADMIN" },
                new IdentityRole { Id = Guid.NewGuid().ToString(), Name = "Employer", NormalizedName = "EMPLOYER" },
                new IdentityRole { Id = Guid.NewGuid().ToString(), Name = "Student", NormalizedName = "STUDENT" }
            );

            // RefreshToken
            modelBuilder.Entity<RefreshToken>()
                .HasOne(rt => rt.User)
                .WithMany()
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // 1-1 Relationships
            modelBuilder.Entity<Student>()
                .HasOne(s => s.User)
                .WithOne(u => u.Student)
                .HasForeignKey<Student>(s => s.UserID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Employer>()
                .HasOne(e => e.User)
                .WithOne(u => u.Employer)
                .HasForeignKey<Employer>(e => e.UserID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Wallet>()
                .HasOne(w => w.User)
                .WithOne(u => u.Wallet)
                .HasForeignKey<Wallet>(w => w.UserID)
                .OnDelete(DeleteBehavior.Cascade);

            // 1-n Relationships
            modelBuilder.Entity<Student>()
                .HasMany(s => s.Resumes)
                .WithOne(r => r.Student)
                .HasForeignKey(r => r.StudentID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Application>()
                .HasOne(a => a.Student)
                .WithMany(s => s.Applications)
                .HasForeignKey(a => a.StudentID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Application>()
                .HasOne(a => a.Job)
                .WithMany(j => j.Applications)
                .HasForeignKey(a => a.JobID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Application>()
                .HasOne(a => a.Resume)
                .WithMany(r => r.Applications)
                .HasForeignKey(a => a.ResumeID)
                .OnDelete(DeleteBehavior.SetNull);

            // Resume - Template (1-n)
            modelBuilder.Entity<Resume>()
                .HasOne(r => r.Student)
                .WithMany(s => s.Resumes)
                .HasForeignKey(r => r.StudentID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Resume>()
                .HasMany(r => r.Applications)
                .WithOne(a => a.Resume)
                .HasForeignKey(a => a.ResumeID)
                .OnDelete(DeleteBehavior.SetNull);

            // JobBookmark
            modelBuilder.Entity<JobBookmark>()
                .HasKey(jb => jb.BookmarkID);

            modelBuilder.Entity<JobBookmark>()
                .HasOne(jb => jb.Student)
                .WithMany(s => s.JobBookmarks)
                .HasForeignKey(jb => jb.StudentID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<JobBookmark>()
                .HasOne(jb => jb.Job)
                .WithMany(j => j.JobBookmarks)
                .HasForeignKey(jb => jb.JobID)
                .OnDelete(DeleteBehavior.Cascade);

            // Application - Review
            modelBuilder.Entity<Application>()
                .HasMany(a => a.Reviews)
                .WithOne(r => r.Application)
                .HasForeignKey(r => r.ApplicationID)
                .OnDelete(DeleteBehavior.SetNull);

            // ApplicationUser - Notification
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserID)
                .OnDelete(DeleteBehavior.Cascade);

            // Report - ApplicationUser
            modelBuilder.Entity<Report>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserID)
                .OnDelete(DeleteBehavior.Cascade);

            // Review configuration


            modelBuilder.Entity<Review>()
                .HasOne(r => r.Reviewer)
                .WithMany()
                .HasForeignKey(r => r.Reviewer_id)
                .OnDelete(DeleteBehavior.Restrict); 

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Reviewee)
                .WithMany()
                .HasForeignKey(r => r.Reviewee_id)
                .OnDelete(DeleteBehavior.Restrict); 

            // Configure decimal precision
            modelBuilder.Entity<Job>()
                .Property(j => j.Salary)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Wallet>()
                .Property(w => w.Balance)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Subscription>()
                .Property(s => s.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<WalletTransaction>()
                .Property(w => w.Amount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Interview>()
                .Property(i => i.Status)
                .HasConversion<string>();

        }
    }
}