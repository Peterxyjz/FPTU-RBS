using BookingManagement.Repositories.Models;
using BookingManagement.Services.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookingManagement.Services.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<UserDto?> GetUserByIdAsync(int id);
        Task<UserDto?> GetUserByEmailAsync(string email);
        Task<IEnumerable<UserDto>> GetUsersByRoleAsync(int roleId);
        Task<UserDto> CreateUserAsync(CreateUserDto createUserDto);
        Task<UserDto> UpdateUserAsync(UpdateUserDto updateUserDto);
        Task<bool> ChangePasswordAsync(ChangePasswordDto changePasswordDto);
        Task<bool> ChangeUserStatusAsync(int userId, bool isActive);
        Task<bool> IsEmailExistsAsync(string email);
    }
}
