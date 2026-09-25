using InventoryManagementSystem.Services;
using InventoryManagementSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagementSystem.Controllers
{
    public class UsersController : Controller
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        // GET: /Users
        public async Task<IActionResult> Index(string? search)
        {
            var users = await _userService.GetUsersAsync(search);

            ViewBag.Search = search;

            return View(users);
        }

        // GET: /Users/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);

            if (user == null)
                return NotFound();

            return View(user);
        }

        // GET: /Users/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Roles = await _userService.GetRolesAsync();

            return View(new UserCreateViewModel());
        }

        // POST: /Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = await _userService.GetRolesAsync();
                return View(model);
            }

            bool created = await _userService.CreateUserAsync(model);

            if (!created)
            {
                ModelState.AddModelError(
                    "",
                    "Username or email already exists."
                );

                ViewBag.Roles = await _userService.GetRolesAsync();

                return View(model);
            }

            TempData["SuccessMessage"] = "User created successfully.";

            return RedirectToAction(nameof(Index));
        }

        // GET: /Users/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);

            if (user == null)
                return NotFound();

            var model = new UserEditViewModel
            {
                Id = user.Id,
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                RoleId = user.RoleId,
                IsActive = user.IsActive
            };

            ViewBag.Roles = await _userService.GetRolesAsync();

            return View(model);
        }

        // POST: /Users/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = await _userService.GetRolesAsync();
                return View(model);
            }

            bool updated = await _userService.UpdateUserAsync(model);

            if (!updated)
            {
                ModelState.AddModelError(
                    "",
                    "Username or email already exists."
                );

                ViewBag.Roles = await _userService.GetRolesAsync();

                return View(model);
            }

            TempData["SuccessMessage"] = "User updated successfully.";

            return RedirectToAction(nameof(Index));
        }

        // POST: /Users/ToggleStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            bool updated = await _userService.ToggleUserStatusAsync(id);

            if (!updated)
                return NotFound();

            TempData["SuccessMessage"] = "User status updated.";

            return RedirectToAction(nameof(Index));
        }
    }
}