using BookingManagement.Repositories.Models;
using BookingManagement.Services.DTOs;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingManagement.Services.Interfaces
{
    public interface IAuthService
    {
        Task<(bool Success, string Role)> AuthenticateAsync(string email, string password, HttpContext httpContext, bool rememberMe = false);
        Task<RegisterResultDto> RegisterUserAsync(RegisterDto registerDto);
        Task SignOutAsync(HttpContext httpContext);
        Task<User?> GetUserByEmailAsync(string email);
        Task<Role?> GetRoleByIdAsync(int roleId);
    }
}
