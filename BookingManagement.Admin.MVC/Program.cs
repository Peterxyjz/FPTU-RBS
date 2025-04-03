using BookingManagement.Repositories.Extensions;
using BookingManagement.Repositories.Interfaces;
using BookingManagement.Services.Interfaces;
using BookingManagement.Services.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Đăng ký Repository service
builder.Services.AddRepositories(builder.Configuration);

// Đăng ký các services
builder.Services.AddScoped<BookingManagement.Services.Interfaces.IAuthService, BookingManagement.Services.Services.AuthService>();
builder.Services.AddScoped<BookingManagement.Services.Interfaces.IUserService, BookingManagement.Services.Services.UserService>();
builder.Services.AddScoped<BookingManagement.Services.Interfaces.IRoomService, BookingManagement.Services.Services.RoomService>();

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

app.Run();
