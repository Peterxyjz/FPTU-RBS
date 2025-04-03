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
        private readonly IConfiguration _configuration;

        public RoomController(IRoomService roomService, IWebHostEnvironment webHostEnvironment, IConfiguration configuration)
        {
            _roomService = roomService;
            _webHostEnvironment = webHostEnvironment;
            _configuration = configuration;
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
                    // Lấy đường dẫn thư mục chia sẻ từ cấu hình
                    var sharedImagesPath = _configuration["SharedImagesFolderPath"];
                    var uploadsFolder = Path.Combine(sharedImagesPath, "rooms");
                    
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
                    // Sử dụng đường dẫn tương đối virtual để dùng chung giữa các ứng dụng
                    roomDto.ImageUrl = $"/shared-images/rooms/{uniqueFileName}";
                    
                    // Không cần sao chép ảnh nữa vì đã dùng thư mục chung
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
                        // Lấy đường dẫn thư mục chia sẻ từ cấu hình
                        var sharedImagesPath = _configuration["SharedImagesFolderPath"];
                        var uploadsFolder = Path.Combine(sharedImagesPath, "rooms");
                        
                        // Sẽ lưu ảnh với tên là [roomNumber].[extension]
                        var uniqueFileName = $"{roomDto.RoomNumber}{Path.GetExtension(roomDto.ImageFile.FileName)}";
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        // Xóa file cũ nếu có (từ thư mục chung)
                        if (!string.IsNullOrEmpty(roomDto.ImageUrl))
                        {
                            // Trích xuất tên file từ ImageUrl
                            var oldFileName = Path.GetFileName(roomDto.ImageUrl);
                            var oldImagePath = Path.Combine(sharedImagesPath, "rooms", oldFileName);
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

                        // Cập nhật ImageUrl là đường dẫn tương đối sử dụng cùng quy ước
                        roomDto.ImageUrl = $"/shared-images/rooms/{uniqueFileName}";
                        
                        // Không cần sao chép ảnh nữa vì đã dùng thư mục chung
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
            if (room != null)
            {
                // Thay đổi thành soft delete - chỉ set IsActive = false
                room.IsActive = false;
                await _roomService.UpdateRoomAsync(room);
            }
            
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
                // Chỉ chấp nhận status là 1 hoặc 2
                if (status != 1 && status != 2)
                {
                    status = 1; // Mặc định là Hoạt động nếu có giá trị không hợp lệ
                }
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

            // Chỉ chấp nhận status là 1 (Hoạt động) hoặc 2 (Bảo trì)
            if (status != 1 && status != 2)
            {
                return BadRequest(new { success = false, message = "Trạng thái không hợp lệ" });
            }

            room.Status = status;
            await _roomService.UpdateRoomAsync(room);
            
            return Ok(new { success = true, message = "Cập nhật trạng thái thành công" });
        }

        // API: Room/Reactivate/5 - Kích hoạt lại phòng đã bị vô hiệu hóa
        [HttpPost]
        public async Task<IActionResult> Reactivate(int id)
        {
            var room = await _roomService.GetRoomByIdAsync(id);
            if (room == null)
            {
                return NotFound();
            }

            room.IsActive = true;
            await _roomService.UpdateRoomAsync(room);
            
            return Ok(new { success = true, message = "Kích hoạt lại phòng thành công" });
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
        
        // Method Sao chép ảnh sang Razor project đã được loại bỏ vì chúng ta đã dùng thư mục chung

        // GET: Room/Archived
        public async Task<IActionResult> Archived()
        {
            // Lấy danh sách các phòng đã bị vô hiệu hóa (IsActive = false)
            var archivedRooms = await _roomService.GetArchivedRoomsAsync();
            return View(archivedRooms);
        }
    }
}