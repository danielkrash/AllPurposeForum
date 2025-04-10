using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using AllPurposeForum.Data;
using AllPurposeForum.Data.DTO;
using Microsoft.AspNetCore.Identity.UI.Services;
using AllPurposeForum.Areas.Identity.Pages.Account;
using static TorchSharp.torch.nn;

namespace AllPurposeForum.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public AuthController(IServiceProvider serviceProvider, ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _serviceProvider = serviceProvider;
            _context = context;
            _userManager = userManager;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RequestRegistrationDTO registration)
        {

            if (!_userManager.SupportsUserEmail)
                throw new NotSupportedException("This endpoint requires a user store with email support.");

            var userStore = _serviceProvider.GetRequiredService<IUserStore<IdentityUser>>();
            var emailStore = (IUserEmailStore<IdentityUser>)userStore;
            var email = registration.Email;

            if (string.IsNullOrEmpty(email) || !new EmailAddressAttribute().IsValid(email))
                return ValidationProblem(CreateValidationProblemDetails(IdentityResult.Failed(_userManager.ErrorDescriber.InvalidEmail(email))));

            var user = new IdentityUser();
            await userStore.SetUserNameAsync(user, email, CancellationToken.None);
            await emailStore.SetEmailAsync(user, email, CancellationToken.None);
            var result = await _userManager.CreateAsync(user, registration.Password);
            await _userManager.AddToRoleAsync(user, "user");

            await _context.SaveChangesAsync();

            if (!result.Succeeded) return ValidationProblem(CreateValidationProblemDetails(result));
            
            return Ok();
        }

        private static ValidationProblemDetails CreateValidationProblemDetails(string errorCode, string errorDescription)
        {
            return new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                { errorCode, new[] { errorDescription } }
            });
        }

        private static ValidationProblemDetails CreateValidationProblemDetails(IdentityResult result)
        {
            var errorDictionary = new Dictionary<string, string[]>(1);

            foreach (var error in result.Errors)
            {
                string[] newDescriptions;

                if (errorDictionary.TryGetValue(error.Code, out var descriptions))
                {
                    newDescriptions = new string[descriptions.Length + 1];
                    Array.Copy(descriptions, newDescriptions, descriptions.Length);
                    newDescriptions[descriptions.Length] = error.Description;
                }
                else
                {
                    newDescriptions = new[] { error.Description };
                }

                errorDictionary[error.Code] = newDescriptions;
            }

            return new ValidationProblemDetails(errorDictionary);
        }

        private static async Task<InfoResponse> CreateInfoResponseAsync(IdentityUser user, UserManager<IdentityUser> userManager)
        {
            return new InfoResponse
            {
                Email = await userManager.GetEmailAsync(user) ?? throw new NotSupportedException("Users must have an email."),
                IsEmailConfirmed = await userManager.IsEmailConfirmedAsync(user)
            };
        }
    }
}
