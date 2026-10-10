using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using QRCodeAttendance.Identity;
using QRCodeAttendance.Implementation.BackgroundJobs;
using QRCodeAttendance.Implementation.Repositories;
using QRCodeAttendance.Implementation.Services;
using QRCodeAttendance.Interface.Repositories;
using QRCodeAttendance.Interface.Services;
using QRCodeAttendance.Models.Configuration;
using QRCodeAttendance.Models.Entities;
using QRCodeAttendance.Persistence.QRCodeAttendanceDb;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.



builder.Services.Scan(scan => scan
    .FromApplicationDependencies(a => a.FullName!.StartsWith("QRCodeAttendance"))
    .AddClasses(c => c.Where(t => t.Name.EndsWith("Service")))
        .AsImplementedInterfaces()
        .WithScopedLifetime()
    .AddClasses(c => c.Where(t => t.Name.EndsWith("Repository")))
        .AsImplementedInterfaces()
        .WithScopedLifetime());



builder.Services.Configure<BrevoOptions>(builder.Configuration.GetSection("Brevo"));
builder.Services.AddHttpClient<IEmailService, BrevoEmailService>();

//Add Database Context
builder.Services.AddDbContext<QRCodeDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("QRCodeDbContext")));

// Services
// builder.Services.AddControllersWithViews();
// builder.Services.AddScoped<IUserRepository, UserRepository>();
// builder.Services.AddScoped<IUserService, UserService>();
// builder.Services.AddScoped<IRoleRepository, RoleRepository>();
// builder.Services.AddScoped<IStudentRepository, StudentRepository>();
// builder.Services.AddScoped<IStudentService, StudentService>();
// builder.Services.AddScoped<IInstructorRepository, InstructorRepository>();
// builder.Services.AddScoped<IInstructorService, InstructorService>();
// builder.Services.AddScoped<ISessionRepository, SessionRepository>();
// builder.Services.AddScoped<ISessionService, SessionService>();
// builder.Services.AddScoped<IAttendanceRepository, AttendanceRepository>();
// builder.Services.AddScoped<IAttendanceService, AttendanceService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddHostedService<QrCodeRotationWorker>();
// builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
// builder.Services.AddControllersWithViews();


builder.Services.AddScoped<IUserStore<User>, QRCodeAttendance.Identity.UserStore>();
builder.Services.AddScoped<IRoleStore<Role>, RoleStore>();
builder.Services.AddIdentity<User, Role>()
    .AddDefaultTokenProviders();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(config =>
{
    config.LoginPath = "/User/Login";
    config.LogoutPath = "/User/Logout";
    config.Cookie.Name = "QRCodeAttendance";
    config.ExpireTimeSpan = TimeSpan.FromMinutes(15);
    config.SlidingExpiration = true;
});

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

var supportedCultures = new[]
{
    new CultureInfo("en"),
    new CultureInfo("en-NG"),
    new CultureInfo("yo"),
    new CultureInfo("yo-NG"),
    new CultureInfo("ha"),
    new CultureInfo("ha-NG"),
    new CultureInfo("ig"),
    new CultureInfo("ig-NG"),
    new CultureInfo("fr"),
    new CultureInfo("es")
};

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("en-NG");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

builder.Services.AddControllersWithViews()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();
builder.Services.AddAuthorization();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// if (builder.Environment.IsDevelopment())
// {
//     app.UseHttpsRedirection();
// }

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRequestLocalization();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Apply database migrations automatically on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<QRCodeDbContext>();
    dbContext.Database.Migrate();
}

//using (var scope = app.Services.CreateScope())
//{
    //var dbContext = scope.ServiceProvider.GetRequiredService<QRCodeDbContext>();
    //dbContext.Database.ExecuteSqlRaw(
        //"ALTER TABLE \"Invitations\" ADD COLUMN IF NOT EXISTS \"IsEmailVerified\" boolean NOT NULL DEFAULT false;"
  //  );
//}
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<QRCodeDbContext>();
    dbContext.Database.Migrate();

    dbContext.Database.ExecuteSqlRaw(
     "ALTER TABLE IF EXISTS \"Invitations\" ADD COLUMN IF NOT EXISTS \"IsEmailVerified\" boolean NOT NULL DEFAULT false;"
 );
    dbContext.Database.ExecuteSqlRaw(
           "ALTER TABLE IF EXISTS \"Invitations\" ADD COLUMN IF NOT EXISTS \"OtpExpiresAt\" timestamp with time zone NULL;"
       );
}


app.Run();
