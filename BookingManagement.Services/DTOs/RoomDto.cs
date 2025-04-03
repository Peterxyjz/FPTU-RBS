using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingManagement.Services.DTOs
{
    public class RoomDto
    {
        [Required(ErrorMessage = "Số phòng là bắt buộc")]
        [Display(Name = "Số phòng")]
        public int RoomNumber { get; set; }

        [Required(ErrorMessage = "Tên phòng là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên phòng không được vượt quá 100 ký tự")]
        [Display(Name = "Tên phòng")]
        public string RoomName { get; set; } = null!;

        [Required(ErrorMessage = "Sức chứa là bắt buộc")]
        [Range(1, 1000, ErrorMessage = "Sức chứa phải từ 1 đến 1000")]
        [Display(Name = "Sức chứa")]
        public int Capacity { get; set; }

        [Required(ErrorMessage = "Loại phòng là bắt buộc")]
        [Display(Name = "Loại phòng")]
        public string RoomType { get; set; } = null!;

        [Display(Name = "Tòa nhà")]
        public string? Building { get; set; }

        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [Display(Name = "URL hình ảnh")]
        public string? ImageUrl { get; set; }
        
        [Display(Name = "Hình ảnh")]
        public IFormFile? ImageFile { get; set; }

        [Required(ErrorMessage = "Trạng thái là bắt buộc")]
        [Display(Name = "Trạng thái")]
        public int Status { get; set; }

        [Display(Name = "Hoạt động")]
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
