using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
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
            public DbSet<Session> Sessions { get; set; } = null!;
            public DbSet<Attendance> Attendances { get; set; } = null!;
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

                builder.Entity<Role>().HasData(
                        new Role
                        {
                            Id = Guid.Parse("c8f2e5ab-9f34-4b93-9b7c-1a5986d79e42"),
                            Name = "Student",
                            CreatedDate = DateTime.Now
                        },
                        new Role
                        {
                            Id = Guid.Parse("d9719e67-53f4-4f9c-bdb2-4c3956789abc"),
                            Name = "Instructor",
                            CreatedDate = DateTime.Now
                        }
                    );


                builder.Entity<Attendance>()
                    .HasOne(a => a.Student)
                    .WithMany(s => s.Attendances)
                    .HasForeignKey(a => a.StudentId)
                    .OnDelete(DeleteBehavior.Cascade);

                builder.Entity<Attendance>()
                    .HasOne(a => a.ClassSession)
                    .WithMany(s => s.Attendances)
                    .HasForeignKey(a => a.SessionId);

                builder.Entity<Session>()
                    .HasOne(s => s.Instructor)
                    .WithMany(i => i.Sessions)
                    .HasForeignKey(s => s.InstructorId);


                builder.Entity<Instructor>()
                .Property(a => a.Gender)
                .HasConversion<string>();

                builder.Entity<Instructor>()
                .Property(a => a.Department)
                .HasConversion<string>();

                builder.Entity<Student>()
                .Property(a => a.Gender)
                .HasConversion<string>();

                builder.Entity<Student>()
                .Property(a => a.Department)
                .HasConversion<string>();

                builder.Entity<Student>()
                .Property(a => a.StudentLevel)
                .HasConversion<string>();

                builder.Entity<Attendance>()
                .Property(a => a.Status)
                .HasConversion<string>();

                builder.Entity<Session>()
                .Property(a => a.Department)
                .HasConversion<string>();

                 builder.Entity<Session>()
                .Property(a => a.Level)
                .HasConversion<string>();

                 builder.Entity<Session>()
                .Property(a => a.IsActive)
                .HasConversion<string>();
            
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

                    optionsBuilder.UseMySql(
                        connectionString,
                        new MySqlServerVersion(new Version(8, 0, 0))
                    );

                    return new QRCodeDbContext(optionsBuilder.Options);
                }
            }

    }
}