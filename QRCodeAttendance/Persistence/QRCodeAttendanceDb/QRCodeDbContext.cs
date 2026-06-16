using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using QRCodeAttendance.Models.Entities;

namespace QRCodeAttendance.Persistence.QRCodeAttendanceDb
{
    public class QRCodeDbContext : DbContext
    {
        public QRCodeDbContext(DbContextOptions<QRCodeDbContext> options) : base(options){}
        
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Role> Roles { get; set; } = null!;
        public DbSet<Student> Students { get; set; } = null!;
        public DbSet<Instructor> Instructors { get; set; } = null!;
        public DbSet<Invitation> Invitations { get; set; } = null!;
        public DbSet<Admin> Admins { get; set; } = null!;
        public DbSet<Session> Sessions { get; set; } = null!;
        public DbSet<Attendance> Attendances { get; set; } = null!;
        public DbSet<QRCodeTokenHistory> QRCodeTokenHistories { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            
            builder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Role>()
                .HasIndex(r => r.Name)
                .IsUnique();

            builder.Entity<Invitation>()
                .HasIndex(i => i.InvitationCodeHash)
                .IsUnique();

            builder.Entity<Invitation>()
                .Property(i => i.InvitationCodeHash)
                .IsRequired();

            builder.Entity<Invitation>()
                .Property(i => i.Status)
                .HasConversion<string>();


            builder.Entity<Admin>()
                .HasOne(a => a.User)
                .WithOne()
                .HasForeignKey<Admin>(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Admin>()
                .Property(a => a.Gender)
                .HasConversion<string>();

            SeedRoleAndAdminData(builder);

   
            builder.Entity<Attendance>()
                .HasOne(a => a.Student)
                .WithMany(s => s.Attendances)
                .HasForeignKey(a => a.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Attendance>()
                .HasOne(a => a.ClassSession)
                .WithMany(s => s.Attendances)
                .HasForeignKey(a => a.SessionId);

            builder.Entity<Attendance>()
                .HasIndex(a => new { a.StudentId, a.SessionId })
                .IsUnique();

            builder.Entity<Session>()
                .HasOne(s => s.Instructor)
                .WithMany(i => i.Sessions)
                .HasForeignKey(s => s.InstructorId);

            builder.Entity<QRCodeTokenHistory>()
                .Property(h => h.Token)
                .HasMaxLength(64);

            builder.Entity<QRCodeTokenHistory>()
                .HasOne(h => h.Session)
                .WithMany(s => s.QRCodeTokenHistories)
                .HasForeignKey(h => h.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<QRCodeTokenHistory>()
                .HasIndex(h => new { h.SessionId, h.Token })
                .IsUnique();

            builder.Entity<Instructor>().Property(a => a.Gender).HasConversion<string>();
            builder.Entity<Instructor>().Property(a => a.Department).HasConversion<string>();
            
            builder.Entity<Student>().Property(a => a.Gender).HasConversion<string>();
            builder.Entity<Student>().Property(a => a.Department).HasConversion<string>();
            builder.Entity<Student>().Property(a => a.StudentLevel).HasConversion<string>();

            builder.Entity<Attendance>().Property(a => a.Status).HasConversion<string>();
            
            builder.Entity<Session>().Property(a => a.Department).HasConversion<string>();
            builder.Entity<Session>().Property(a => a.Level).HasConversion<string>();
            builder.Entity<Session>().Property(a => a.IsActive).HasConversion<string>();
        }

        private void SeedRoleAndAdminData(ModelBuilder modelBuilder)
        {
            
            var studentRoleId = Guid.Parse("c8f2e5ab-9f34-4b93-9b7c-1a5986d79e42");
            var instructorRoleId = Guid.Parse("d9719e67-53f4-4f9c-bdb2-4c3956789abc");
            var adminRoleId = Guid.Parse("a184ef12-34cd-56ef-78ab-9012345678cd");
            
            var adminUserId = Guid.Parse("f47ac10b-58cc-4372-a567-0e02b2c3d479");
            var adminProfileId = Guid.Parse("b231da64-78ef-49cd-ba12-0987654321ab");


            modelBuilder.Entity<Role>().HasData(
                new Role
                {
                    Id = studentRoleId,
                    Name = "Student",
                    CreatedDate = DateTime.SpecifyKind(new DateTime(2026, 04, 25), DateTimeKind.Utc)
                },
                new Role
                {
                    Id = instructorRoleId,
                    Name = "Instructor",
                    CreatedDate = DateTime.SpecifyKind(new DateTime(2026, 04, 25), DateTimeKind.Utc)
                },
                new Role
                {
                    Id = adminRoleId,
                    Name = "Admin",
                    CreatedDate = DateTime.SpecifyKind(new DateTime(2026, 04, 25), DateTimeKind.Utc)
                }
            );

            var adminUser = new User
            {
                Id = adminUserId,
                Email = "admin@gmail.com",
                UserName = "admin@gmail.com",
                PasswordHash = "AQAAAAIAAYagAAAAEJjieFsJGM2Xgr+WpuS3juOABbBCvbqSvpym4WzP/SDMuvGz6qH+EFgm19l8SUHUGA==",
                RoleId = adminRoleId,
                EmailConfirmed = true,
                CreatedDate = DateTime.SpecifyKind(new DateTime(2025, 11, 10), DateTimeKind.Utc),

            };

            var adminProfile = new Admin
                {
                    Id = adminProfileId,
                    UserId = adminUserId,
                    FirstName = "Admin",
                    LastName = "QRCode",
                    Email = "admin@gmail.com",
                    Gender = Models.Enums.Gender.Male,
                    Address = "Ogun State, Nigeria",
                    PhoneNumber = "+23470456780",
                    DateOfBirth = DateTime.SpecifyKind(new DateTime(1998, 03, 04), DateTimeKind.Utc),
                    CreatedDate = DateTime.SpecifyKind(new DateTime(2026, 05, 30), DateTimeKind.Utc)
                };

            modelBuilder.Entity<Admin>().HasData(adminProfile);
            modelBuilder.Entity<User>().HasData(adminUser);
        }
    }

    public class QRCodeDbContextFactory : IDesignTimeDbContextFactory<QRCodeDbContext>
    {
        public QRCodeDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<QRCodeDbContext>();
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var connectionString = configuration.GetConnectionString("QRCodeDbContext");
            optionsBuilder.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 0)));

            return new QRCodeDbContext(optionsBuilder.Options);
        }
    }
}
