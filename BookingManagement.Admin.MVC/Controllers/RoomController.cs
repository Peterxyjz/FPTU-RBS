using BookingManagement.Admin.MVC.Models;
using BookingManagement.Repositories.Models;
using BookingManagement.Services.DTOs;
using BookingManagement.Services.Extensions;
using BookingManagement.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BookingManagement.Admin.MVC.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class RoomController : Controller
    {
        private readonly IRoomService _roomService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public RoomController(IRoomService roomService, IWebHostEnvironment webHostEnvironment)
        {
            _roomService = roomService;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Room
        public async Task<IActionResult> Index()
        {
            var rooms = await _roomService.GetAllRoomsAsync();
            return View(rooms);
        }

        // GET: Room/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var room = await _roomService.GetRoomByIdAsync(id);
            if (room == null)
            {
                return NotFound();
            }

            return View(room);
        }

        // GET: Room/Create
        public async Task<IActionResult> Create()
        {
            await PrepareViewBags();
            return View();
        }

        // POST: Room/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RoomDto roomDto)
        {
            if (ModelState.IsValid)
            {
                // Xử lý upload hình ảnh
                if (roomDto.ImageFile != null && roomDto.ImageFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "rooms");
                    // Trước khi lưu, chúng ta cần đảm bảo roomId (là roomNumber) đã có giá trị
                    // Sẽ lưu ảnh với tên là [roomNumber].[extension]
                    var uniqueFileName = $"{roomDto.RoomNumber}{Path.GetExtension(roomDto.ImageFile.FileName)}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Tạo thư mục nếu không tồn tại
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await roomDto.ImageFile.CopyToAsync(fileStream);
                    }

                    // Cập nhật ImageUrl là đường dẫn tương đối
                    roomDto.ImageUrl = $"/images/rooms/{uniqueFileName}";
                    
                    // Sao chép ảnh sang dự án User.Razor
                    await CopyImageToRazorProject(uniqueFileName);
                }

                var room = roomDto.ToEntity();
                await _roomService.AddRoomAsync(room);
                
                return RedirectToAction(nameof(Index));
            }

            await PrepareViewBags();
            return View(roomDto);
        }

        // GET: Room/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var room = await _roomService.GetRoomByIdAsync(id);
            if (room == null)
            {
                return NotFound();
            }

            var roomDto = room.ToDto();
            await PrepareViewBags();
            return View(roomDto);
        }

        // POST: Room/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, RoomDto roomDto)
        {
            if (id != roomDto.RoomNumber)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Xử lý upload hình ảnh
                    if (roomDto.ImageFile != null && roomDto.ImageFile.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "rooms");
                        // Sẽ lưu ảnh với tên là [roomNumber].[extension]
                        var uniqueFileName = $"{roomDto.RoomNumber}{Path.GetExtension(roomDto.ImageFile.FileName)}";
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        // Xóa file cũ nếu có
                        if (!string.IsNullOrEmpty(roomDto.ImageUrl))
                        {
                            var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, roomDto.ImageUrl.TrimStart('/'));
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        // Tạo thư mục nếu không tồn tại
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await roomDto.ImageFile.CopyToAsync(fileStream);
                        }

                        // Cập nhật ImageUrl là đường dẫn tương đối
                        roomDto.ImageUrl = $"/images/rooms/{uniqueFileName}";
                        
                        // Sao chép ảnh sang dự án User.Razor
                        await CopyImageToRazorProject(uniqueFileName);
                    }

                    var room = roomDto.ToEntity();
                    await _roomService.UpdateRoomAsync(room);
                   
                }
                catch (DbUpdateConcurrencyException)
                {
                    var roomExists = await RoomExists(id);
                    if (!roomExists)
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            await PrepareViewBags();
            return View(roomDto);
        }

        // GET: Room/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var room = await _roomService.GetRoomByIdAsync(id);
            if (room == null)
            {
                return NotFound();
            }

            return View(room);
        }

        // POST: Room/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var room = await _roomService.GetRoomByIdAsync(id);
            if (room != null && !string.IsNullOrEmpty(room.ImageUrl))
            {
                // Xóa file ảnh trong Admin.MVC
                var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, room.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
                
                // Xóa file ảnh trong User.Razor
                var fileName = Path.GetFileName(room.ImageUrl);
                var razorImagePath = Path.Combine(_webHostEnvironment.ContentRootPath, "..", "BookingManagement.User.Razor", "wwwroot", "images", "rooms", fileName);
                if (System.IO.File.Exists(razorImagePath))
                {
                    System.IO.File.Delete(razorImagePath);
                }
            }
            
            await _roomService.DeleteRoomAsync(id);
            
            
            return RedirectToAction(nameof(Index));
        }

        // GET: Room/Filter
        public async Task<IActionResult> Filter(string building = null, string roomType = null, int? status = null)
        {
            IEnumerable<Room> rooms;

            if (!string.IsNullOrEmpty(building))
            {
                rooms = await _roomService.GetRoomsByBuildingAsync(building);
            }
            else if (!string.IsNullOrEmpty(roomType))
            {
                rooms = await _roomService.GetRoomsByTypeAsync(roomType);
            }
            else if (status.HasValue)
            {
                rooms = await _roomService.GetRoomsByStatusAsync(status.Value);
            }
            else
            {
                rooms = await _roomService.GetAllRoomsAsync();
            }

            await PrepareViewBags();
            ViewBag.CurrentBuilding = building;
            ViewBag.CurrentRoomType = roomType;
            ViewBag.CurrentStatus = status;

            return View("Index", rooms);
        }

        // API: Room/UpdateStatus/5?status=2
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, int status)
        {
            var room = await _roomService.GetRoomByIdAsync(id);
            if (room == null)
            {
                return NotFound();
            }

            room.Status = status;
            await _roomService.UpdateRoomAsync(room);
            
            return Ok(new { success = true, message = "Cập nhật trạng thái thành công" });
        }

        private async Task<bool> RoomExists(int roomNumber)
        {
            var room = await _roomService.GetRoomByIdAsync(roomNumber);
            return room != null;
        }

        private async Task PrepareViewBags()
        {
            // Lấy danh sách các loại phòng hiện có
            var existingRoomTypes = await _roomService.GetRoomTypesAsync();
            
            // Kết hợp với danh sách loại phòng mặc định
            var roomTypes = existingRoomTypes
                .Union(RoomExtensions.GetDefaultRoomTypes())
                .Distinct()
                .OrderBy(t => t);

            ViewBag.RoomTypes = new SelectList(roomTypes);

            // Lấy danh sách các tòa nhà hiện có
            var existingBuildings = await _roomService.GetBuildingsAsync();
            
            // Kết hợp với danh sách tòa nhà mặc định
            var buildings = existingBuildings
                .Union(RoomExtensions.GetDefaultBuildings())
                .Distinct()
                .OrderBy(b => b);

            ViewBag.Buildings = new SelectList(buildings);

            // Danh sách trạng thái
            ViewBag.Statuses = new SelectList(RoomExtensions.GetRoomStatusList(), "Key", "Value");
        }
        
        // Sao chép ảnh sang Razor project
        private async Task CopyImageToRazorProject(string uniqueFileName)
        {
            try
            {
                // Đường dẫn đến file ảnh trong Admin.MVC
                var sourcePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "rooms", uniqueFileName);
                
                // Đường dẫn đến thư mục của User.Razor project
                var razorProjectPath = Path.Combine(_webHostEnvironment.ContentRootPath, "..", "BookingManagement.User.Razor", "wwwroot", "images", "rooms");
                
                // Tạo thư mục nếu chưa tồn tại
                if (!Directory.Exists(razorProjectPath))
                {
                    Directory.CreateDirectory(razorProjectPath);
                }
                
                // Đường dẫn đến file ảnh đích trong User.Razor
                var destinationPath = Path.Combine(razorProjectPath, uniqueFileName);
                
                // Sao chép file
                using (var sourceStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read))
                using (var destinationStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write))
                {
                    await sourceStream.CopyToAsync(destinationStream);
                }
            }
            catch (Exception ex)
            {
                // Log lỗi nhưng không ném ra exception
                Console.WriteLine($"Error copying image to Razor project: {ex.Message}");
            }
        }
    }
}