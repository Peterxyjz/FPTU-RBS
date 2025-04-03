using BookingManagement.Repositories.Models;
using BookingManagement.Repositories.UnitOfWork;
using BookingManagement.Services.DTOs;
using BookingManagement.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookingManagement.Services.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var users = await _unitOfWork.Users.GetAllAsync();
            return users.Select(MapToUserDto);
        }

        public async Task<UserDto?> GetUserByIdAsync(int id)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            return user != null ? MapToUserDto(user) : null;
        }

        public async Task<UserDto?> GetUserByEmailAsync(string email)
        {
            var user = await _unitOfWork.Users.GetByEmailAsync(email);
            return user != null ? MapToUserDto(user) : null;
        }

        public async Task<IEnumerable<UserDto>> GetUsersByRoleAsync(int roleId)
        {
            var users = await _unitOfWork.Users.GetUsersByRoleAsync(roleId);
            return users.Select(MapToUserDto);
        }

        public async Task<UserDto> CreateUserAsync(CreateUserDto createUserDto)
        {
            // Kiểm tra email đã tồn tại
            if (await _unitOfWork.Users.IsEmailExistsAsync(createUserDto.Email))
            {
                throw new InvalidOperationException("Email đã tồn tại trong hệ thống");
            }

            var user = new User
            {
                Email = createUserDto.Email,
                Password = createUserDto.Password, // Trong thực tế, nên mã hóa mật khẩu
                FullName = createUserDto.FullName,
                RoleId = createUserDto.RoleId,
                IsActive = true,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.CompleteAsync();

            // Lấy thông tin user vừa tạo (bao gồm role)
            var createdUser = await _unitOfWork.Users.GetByIdAsync(user.UserId);
            return MapToUserDto(createdUser!);
        }

        public async Task<UserDto> UpdateUserAsync(UpdateUserDto updateUserDto)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(updateUserDto.UserId);
            if (user == null)
            {
                throw new InvalidOperationException("Không tìm thấy người dùng");
            }

            // Kiểm tra nếu email thay đổi và đã tồn tại
            if (user.Email != updateUserDto.Email && await _unitOfWork.Users.IsEmailExistsAsync(updateUserDto.Email))
            {
                throw new InvalidOperationException("Email đã tồn tại trong hệ thống");
            }

            user.Email = updateUserDto.Email;
            user.FullName = updateUserDto.FullName;
            user.RoleId = updateUserDto.RoleId;
            user.UpdatedAt = DateTime.Now;

            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.CompleteAsync();

            // Lấy thông tin user vừa cập nhật (bao gồm role)
            var updatedUser = await _unitOfWork.Users.GetByIdAsync(user.UserId);
            return MapToUserDto(updatedUser!);
        }

        public async Task<bool> ChangePasswordAsync(ChangePasswordDto changePasswordDto)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(changePasswordDto.UserId);
            if (user == null)
            {
                throw new InvalidOperationException("Không tìm thấy người dùng");
            }

            user.Password = changePasswordDto.NewPassword; // Trong thực tế, nên mã hóa mật khẩu
            user.UpdatedAt = DateTime.Now;

            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.CompleteAsync();

            return true;
        }

        public async Task<bool> ChangeUserStatusAsync(int userId, bool isActive)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException("Không tìm thấy người dùng");
            }

            user.IsActive = isActive;
            user.UpdatedAt = DateTime.Now;

            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.CompleteAsync();

            return true;
        }

        public async Task<bool> IsEmailExistsAsync(string email)
        {
            return await _unitOfWork.Users.IsEmailExistsAsync(email);
        }

        private UserDto MapToUserDto(User user)
        {
            return new UserDto
            {
                UserId = user.UserId,
                Email = user.Email,
                FullName = user.FullName,
                RoleId = user.RoleId,
                RoleName = user.Role?.RoleName ?? "Unknown",
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };
        }
    }
}
