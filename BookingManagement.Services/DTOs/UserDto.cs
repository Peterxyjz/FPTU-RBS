using System;
using System.ComponentModel.DataAnnotations;

namespace BookingManagement.Services.DTOs
{
    public class UserDto
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Định dạng email không hợp lệ")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        public string FullName { get; set; } = null!;

        public int RoleId { get; set; }
        public string RoleName { get; set; } = null!;

        public bool? IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateUserDto
    {
        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Định dạng email không hợp lệ")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        public string FullName { get; set; } = null!;

        [Required(ErrorMessage = "Vai trò là bắt buộc")]
        public int RoleId { get; set; }
    }

    public class UpdateUserDto
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Định dạng email không hợp lệ")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        public string FullName { get; set; } = null!;

        [Required(ErrorMessage = "Vai trò là bắt buộc")]
        public int RoleId { get; set; }
    }

    public class ChangePasswordDto
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "Mật khẩu mới là bắt buộc")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        public string NewPassword { get; set; } = null!;
    }
}
