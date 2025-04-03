using Microsoft.AspNetCore.Mvc;
using BookingManagement.Services.Interfaces;
using BookingManagement.Services.DTOs;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System;
using BookingManagement.Repositories.UnitOfWork;

namespace BookingManagement.Admin.MVC.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly IUnitOfWork _unitOfWork;

        public UserController(IUserService userService, IUnitOfWork unitOfWork)
        {
            _userService = userService;
            _unitOfWork = unitOfWork;
        }

        // GET: User
        public async Task<IActionResult> Index()
        {
            var users = await _userService.GetAllUsersAsync();
            return View(users);
        }

        // GET: User/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: User/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Roles = await _unitOfWork.Roles.GetAllAsync();
            return View();
        }

        // POST: User/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserDto createUserDto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _userService.CreateUserAsync(createUserDto);
                    return RedirectToAction(nameof(Index));
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "Đã xảy ra lỗi khi tạo người dùng. Vui lòng thử lại sau.");
                }
            }

            ViewBag.Roles = await _unitOfWork.Roles.GetAllAsync();
            return View(createUserDto);
        }

        // GET: User/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var updateUserDto = new UpdateUserDto
            {
                UserId = user.UserId,
                Email = user.Email,
                FullName = user.FullName,
                RoleId = user.RoleId
            };

            ViewBag.Roles = await _unitOfWork.Roles.GetAllAsync();
            return View(updateUserDto);
        }

        // POST: User/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateUserDto updateUserDto)
        {
            if (id != updateUserDto.UserId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _userService.UpdateUserAsync(updateUserDto);
                    return RedirectToAction(nameof(Index));
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "Đã xảy ra lỗi khi cập nhật người dùng. Vui lòng thử lại sau.");
                }
            }

            ViewBag.Roles = await _unitOfWork.Roles.GetAllAsync();
            return View(updateUserDto);
        }

        // GET: User/ChangePassword/5
        public async Task<IActionResult> ChangePassword(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var changePasswordDto = new ChangePasswordDto
            {
                UserId = user.UserId
            };

            return View(changePasswordDto);
        }

        // POST: User/ChangePassword/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(int id, ChangePasswordDto changePasswordDto)
        {
            if (id != changePasswordDto.UserId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _userService.ChangePasswordAsync(changePasswordDto);
                    TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
                    return RedirectToAction(nameof(Details), new { id = changePasswordDto.UserId });
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "Đã xảy ra lỗi khi đổi mật khẩu. Vui lòng thử lại sau.");
                }
            }

            return View(changePasswordDto);
        }

        // POST: User/ChangeStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatus(int id, bool isActive)
        {
            try
            {
                await _userService.ChangeUserStatusAsync(id, isActive);
                TempData["SuccessMessage"] = isActive ? "Kích hoạt tài khoản thành công!" : "Vô hiệu hóa tài khoản thành công!";
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi thay đổi trạng thái tài khoản. Vui lòng thử lại sau.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
