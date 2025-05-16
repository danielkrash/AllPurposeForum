using AllPurposeForum.Data;
using AllPurposeForum.Data.Models;
using AllPurposeForum.Exeptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol;

namespace AllPurposeForum.Services.Implementation
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public UserService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        public async Task<IdentityResult> AddToRoleAsync(string userId, string roleName)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return IdentityResult.Failed(new IdentityError
                    {
                        Code = "UserNotFound",
                        Description = $"The user with id '{userId}' does not exist."
                    });
                }

                var roleExists = await _roleManager.RoleExistsAsync(roleName);
                if (!roleExists)
                {
                    return IdentityResult.Failed(new IdentityError
                    {
                        Code = "RoleNotFound",
                        Description = $"The role '{roleName}' does not exist."
                    });
                }

                return await _userManager.AddToRoleAsync(user, roleName);
            }
            catch (UserNotFoundException ex)
            {
                // Log the exception or handle it as needed
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "UserNotFound",
                    Description = ex.Message
                });
            }
        }

        public async Task<IdentityResult> DeleteUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "UserNotFound",
                    Description = $"The user with id '{userId}' does not exist."
                });
            }

            var result = await _userManager.DeleteAsync(user);
            return result;
        }

        public ApplicationUser? GetUserByIdAsync(string userId)
        {
            var user = _context.Users.Find(userId);
            return user;
        }

        public Task<IList<string>> GetUserRolesAsync(string userId)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityResult> RegisterUserAsync(string email, string password)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityResult> RemoveFromRoleAsync(string userId, string roleName)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityResult> UpdateUserAsync(string userId, string newEmail, string newPassword)
        {
            throw new NotImplementedException();
        }
        public async Task<IdentityResult> LockOutUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId); // Await the task to get the actual user object
            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "UserNotFound",
                    Description = $"The user with id '{userId}' does not exist."
                });
            }
            else
            {
                var result = await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));
                if (result.Succeeded)
                {
                    Console.WriteLine($"User {user.UserName} locked out successfully.");
                }
                else
                {
                    Console.WriteLine($"Failed to lock out user {user.UserName}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
                Console.WriteLine($"User {user.UserName} locked out until {DateTimeOffset.UtcNow.AddYears(100)}");
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "UserNotFound",
                    Description = $"The user with id '{userId}' does not exist."
                });
            }
        }
    }
}
