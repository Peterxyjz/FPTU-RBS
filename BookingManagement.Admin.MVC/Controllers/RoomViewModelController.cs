using BookingManagement.Admin.MVC.Models;
using BookingManagement.Repositories.Models;
using BookingManagement.Services.DTOs;
using BookingManagement.Services.Extensions;
using BookingManagement.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookingManagement.Admin.MVC.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class RoomViewModelController : Controller
    {
        private readonly IRoomService _roomService;

        public RoomViewModelController(IRoomService roomService)
        {
            _roomService = roomService;
        }

        public IActionResult Index()
        {
            // Sử dụng RoomDto để đảm bảo namespace được nhận diện
            var roomDto = new RoomDto
            {
                RoomName = "Test Room",
                Capacity = 30,
                RoomType = "Classroom",
                Building = "Alpha",
                Description = "Test Description",
                Status = 1,
                IsActive = true
            };

            return View(roomDto);
        }
    }
}
