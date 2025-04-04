using BookingManagement.Repositories.Extensions;
using BookingManagement.Repositories.Interfaces;
using BookingManagement.Services.Extensions;
using BookingManagement.Services.Interfaces;
using BookingManagement.Services.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.FileProviders;
using BookingManagement.Admin.MVC.Hubs;
using BookingManagement.Admin.MVC.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Đăng ký Repository service
builder.Services.AddRepositories(builder.Configuration);

// Đăng ký các services
// Add all application services
builder.Services.AddApplicationServices();

// Đăng ký AdminSignalRService để làm việc với BookingHub
builder.Services.AddScoped<ISignalRService, AdminSignalRService>();

// Đăng ký SignalR
builder.Services.AddSignalR();

// Cấu hình Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.HttpOnly = true;
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.LoginPath = "/Login";
        options.LogoutPath = "/Login/Logout";
        options.AccessDeniedPath = "/Login/AccessDenied";
        options.SlidingExpiration = true;
    });

// Cấu hình Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Cấu hình để phục vụ file từ thư mục chia sẻ
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(builder.Configuration["SharedImagesFolderPath"]),
    RequestPath = "/shared-images"
});

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}");
    
// Map SignalR hub
app.MapHub<BookingHub>("/bookingHub");
// Map tương thích với đường dẫn của user để đảm bảo kết nối chéo
app.MapHub<BookingHub>("/notificationHub");

app.Run();
