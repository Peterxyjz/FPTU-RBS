using BookingManagement.Repositories.Interfaces;
using BookingManagement.Repositories.Models;
using BookingManagement.Repositories.UnitOfWork;
using BookingManagement.Services.DTOs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace BookingManagement.Services.Services
{
    public interface IAuthService
    {
        Task<(bool Success, string Role)> AuthenticateAsync(string email, string password, HttpContext httpContext, bool rememberMe = false);
        Task<RegisterResultDto> RegisterUserAsync(RegisterDto registerDto);
        Task SignOutAsync(HttpContext httpContext);
        Task<User?> GetUserByEmailAsync(string email);
        Task<Role?> GetRoleByIdAsync(int roleId);
    }

    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IUnitOfWork _unitOfWork;

        public AuthService(IUserRepository userRepository, IRoleRepository roleRepository, IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            var users = await _userRepository.FindAsync(u => u.Email == email && u.IsActive == true);
            return users.FirstOrDefault();
        }

        public async Task<Role?> GetRoleByIdAsync(int roleId)
        {
            return await _roleRepository.GetByIdAsync(roleId);
        }

        public async Task<(bool Success, string Role)> AuthenticateAsync(string email, string password, HttpContext httpContext, bool rememberMe = false)
        {
            // Tìm user theo email
            var user = await GetUserByEmailAsync(email);
            
            // Kiểm tra user tồn tại và mật khẩu đúng
            // Lưu ý: Trong môi trường production, nên sử dụng hashing
            if (user == null || user.Password != password)
                return (false, string.Empty);

            // Lấy thông tin role
            var role = await GetRoleByIdAsync(user.RoleId);
            if (role == null)
                return (false, string.Empty);

            // Tạo claims cho user
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Role, role.RoleName)
            };

            // Tạo ClaimsIdentity
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            // Sign in
            await httpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = rememberMe, // Chỉ ghi nhớ nếu rememberMe = true
                    ExpiresUtc = rememberMe ? DateTime.UtcNow.AddDays(30) : null
                });

            return (true, role.RoleName);
        }

        public async Task<RegisterResultDto> RegisterUserAsync(RegisterDto registerDto)
        {
            // Kiểm tra email đã tồn tại
            var existingUser = await GetUserByEmailAsync(registerDto.Email);
            if (existingUser != null)
            {
                return new RegisterResultDto
                {
                    Success = false,
                    Message = "Email đã tồn tại trong hệ thống"
                };
            }

            // Lấy default Role cho User (RoleId = 2 cho User)
            var userRoles = await _roleRepository.FindAsync(r => r.RoleName == "User");
            int roleId = userRoles.FirstOrDefault()?.RoleId ?? 2; // Mặc định là 2 nếu không tìm thấy

            // Tạo user mới
            var newUser = new User
            {
                Email = registerDto.Email,
                Password = registerDto.Password, // Trong thực tế, mật khẩu nên được hash
                FullName = registerDto.FullName,
                RoleId = roleId,
                IsActive = true,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            await _userRepository.AddAsync(newUser);
            await _unitOfWork.CompleteAsync();

            return new RegisterResultDto
            {
                Success = true,
                Message = "Đăng ký tài khoản thành công",
                UserId = newUser.UserId
            };
        }

        public async Task SignOutAsync(HttpContext httpContext)
        {
            await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }
}
