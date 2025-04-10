using Microsoft.AspNetCore.Identity;

namespace AllPurposeForum.Services
{
    public interface IUserService
    {
        Task<IdentityResult> RegisterUserAsync(string email, string password);
        Task<IdentityResult> UpdateUserAsync(string userId, string newEmail, string newPassword);
        Task<IdentityResult> DeleteUserAsync(string userId);
        IdentityUser? GetUserByIdAsync(string userId);
        Task<IList<string>> GetUserRolesAsync(string userId);
        Task<IdentityResult> AddToRoleAsync(string userId, string roleName);
        Task<IdentityResult> RemoveFromRoleAsync(string userId, string roleName);
        Task<IdentityResult> LockOutUser(string userId);
    }
}
