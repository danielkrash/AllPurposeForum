using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using AllPurposeForum.Data.Models;
using System.Linq;
using System.Threading.Tasks;
using AllPurposeForum.Web.Models; // Assuming your ViewModels are here
using Microsoft.EntityFrameworkCore; // For ToListAsync()

namespace AllPurposeForum.Web.Controllers
{
    [Authorize(Roles = "Admin")] // Only Admins can access this controller
    public class UserManagementController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserManagementController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: UserManagement
        public async Task<IActionResult> Index(string sortByRole)
        {
            ViewData["RoleSortParam"] = string.IsNullOrEmpty(sortByRole) ? "role_desc" : "";

            var users = await _userManager.Users.ToListAsync();
            var userViewModels = new List<UserViewModel>();
            foreach (var user in users)
            {
                userViewModels.Add(new UserViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    Roles = await _userManager.GetRolesAsync(user) // Switched to await
                });
            }

            if (!string.IsNullOrEmpty(sortByRole))
            {
                // Ensure Roles is not null before trying to access FirstOrDefault
                userViewModels = userViewModels.OrderBy(u => u.Roles?.FirstOrDefault()).ToList();
                if (sortByRole == "role_desc")
                {
                    userViewModels = userViewModels.OrderByDescending(u => u.Roles?.FirstOrDefault()).ToList();
                }
            }
            else
            {
                // Default sort by username if no role sort is specified
                userViewModels = userViewModels.OrderBy(u => u.UserName).ToList();
            }

            return View(userViewModels);
        }

        // GET: UserManagement/CreateUser
        public async Task<IActionResult> CreateUser()
        {
            var allRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            var model = new CreateUserViewModel
            {
                Roles = allRoles ?? new List<string?>()
            };
            return View(model);
        }

        // POST: UserManagement/CreateUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(CreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.UserName, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password!);

                if (result.Succeeded)
                {
                    if (model.UserRoles != null && model.UserRoles.Any())
                    {
                        var rolesToAdd = model.UserRoles.Where(r => r != null).Select(r => r!).ToList();
                        if (rolesToAdd.Any())
                        {
                           var roleResult = await _userManager.AddToRolesAsync(user, rolesToAdd);
                           if (!roleResult.Succeeded)
                           {
                                // Log errors or add to ModelState if role assignment fails
                                foreach (var error in roleResult.Errors)
                                {
                                    ModelState.AddModelError(string.Empty, error.Description);
                                }
                                // Repopulate roles for the view
                                model.Roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync() ?? new List<string?>();
                                return View(model);
                           }
                        }
                    }
                    TempData["SuccessMessage"] = "User created successfully.";
                    return RedirectToAction(nameof(Index));
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            // If we got this far, something failed, redisplay form
            model.Roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync() ?? new List<string?>();
            return View(model);
        }

        // GET: UserManagement/Edit/5
        public async Task<IActionResult> EditUser(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            var allRoles = await _roleManager.Roles.ToListAsync();

            var model = new EditUserViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Roles = allRoles.Select(r => r.Name).ToList(),
                UserRoles = userRoles.ToList()
            };

            return View(model);
        }

        // POST: UserManagement/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(model.Id);
                if (user == null)
                {
                    return NotFound();
                }

                user.UserName = model.UserName;
                user.Email = model.Email;
                // Update other user properties if needed

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    // Add errors to ModelState
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    // Repopulate roles for the view if returning due to error
                    model.Roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync() ?? new List<string?>();
                    return View(model);
                }

                // Role management
                var currentRoles = await _userManager.GetRolesAsync(user);
                var userRolesModel = model.UserRoles ?? new List<string?>(); // Ensure not null

                var rolesToRemove = currentRoles.Except(userRolesModel.Where(r => r != null).Select(r => r!)).ToList();
                var rolesToAdd = userRolesModel.Where(r => r != null).Select(r => r!).Except(currentRoles).ToList();

                if (rolesToRemove.Any())
                {
                    await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                }
                if (rolesToAdd.Any())
                {
                    await _userManager.AddToRolesAsync(user, rolesToAdd);
                }

                TempData["SuccessMessage"] = "User updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            // Repopulate roles for the view if model state is invalid
            model.Roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync() ?? new List<string?>();
            return View(model);
        }

        // POST: UserManagement/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction(nameof(Index));
            }

            // Prevent admin from deleting themselves (optional, but good practice)
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser != null && currentUser.Id == user.Id)
            {
                TempData["ErrorMessage"] = "You cannot delete your own account.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "User deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Error deleting user: " + string.Join(", ", result.Errors.Select(e => e.Description));
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
