using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using AllPurposeForum.Data;
using AllPurposeForum.Data.DTO;
using AllPurposeForum.Data.Models;
using AllPurposeForum.Helpers;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.General;

namespace AllPurposeForum.Web.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IEmailSender<ApplicationUser> _emailSender;
    private readonly IServiceProvider _serviceProvider;
    private readonly UserManager<ApplicationUser> _userManager;

    public AuthController(IServiceProvider serviceProvider, ApplicationDbContext context,
        UserManager<ApplicationUser> userManager, IEmailSender<ApplicationUser> emailSender)
    {
        _serviceProvider = serviceProvider;
        _context = context;
        _userManager = userManager;
        _emailSender = emailSender;
    }

    [HttpPost("register")]
    public async Task<Results<Ok, ValidationProblem>> Register([FromBody] RequestRegistrationDTO registration)
    {
        if (!_userManager.SupportsUserEmail)
            throw new NotSupportedException("This endpoint requires a user store with email support.");

        var userStore = _serviceProvider.GetRequiredService<IUserStore<ApplicationUser>>();
        var emailStore = (IUserEmailStore<ApplicationUser>)userStore;
        var email = registration.Email;

        if (string.IsNullOrEmpty(email) || !new EmailAddressAttribute().IsValid(email))
            return Utils.CreateValidationProblem(
                IdentityResult.Failed(_userManager.ErrorDescriber.InvalidEmail(email)));

        var user = new ApplicationUser();
        await userStore.SetUserNameAsync(user, email, CancellationToken.None);
        await emailStore.SetEmailAsync(user, email, CancellationToken.None);
        var result = await _userManager.CreateAsync(user, registration.Password);
        await _userManager.AddToRoleAsync(user, "user");
        await _userManager.ConfirmEmailAsync(user, await _userManager.GenerateEmailConfirmationTokenAsync(user));
        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        await _userManager.ConfirmEmailAsync(user, code);
        await _context.SaveChangesAsync();

        if (!result.Succeeded) return Utils.CreateValidationProblem(result);
        return TypedResults.Ok();
    }

    [HttpPost("login")]
    public async Task<Results<Ok<AccessTokenResponse>, EmptyHttpResult, ProblemHttpResult>> Login(
        [FromBody] LoginRequest login, [FromQuery] bool? useCookies, [FromQuery] bool? useSessionCookies)
    {
        var signInManager = _serviceProvider.GetRequiredService<SignInManager<ApplicationUser>>();
        var useCookieScheme = useCookies == true || useSessionCookies == true;
        var isPersistent = useCookies == true && useSessionCookies != true;
        signInManager.AuthenticationScheme =
            useCookieScheme ? IdentityConstants.ApplicationScheme : IdentityConstants.BearerScheme;

        var result = await signInManager.PasswordSignInAsync(login.Email, login.Password, isPersistent, true);

        if (result.RequiresTwoFactor)
        {
            if (!string.IsNullOrEmpty(login.TwoFactorCode))
                result = await signInManager.TwoFactorAuthenticatorSignInAsync(login.TwoFactorCode, isPersistent,
                    isPersistent);
            else if (!string.IsNullOrEmpty(login.TwoFactorRecoveryCode))
                result = await signInManager.TwoFactorRecoveryCodeSignInAsync(login.TwoFactorRecoveryCode);
        }

        if (!result.Succeeded)
            return TypedResults.Problem(result.ToString(), statusCode: StatusCodes.Status401Unauthorized);
        return TypedResults.Empty;
    }

    [HttpPost("refresh")]
    public async Task<Results<Ok<AccessTokenResponse>, UnauthorizedHttpResult, SignInHttpResult, ChallengeHttpResult>>
        Refresh([FromBody] RefreshRequest refreshRequest)
    {
        var timeProvider = _serviceProvider.GetRequiredService<TimeProvider>();
        var bearerTokenOptions = _serviceProvider.GetRequiredService<IOptionsMonitor<BearerTokenOptions>>();
        var signInManager = _serviceProvider.GetRequiredService<SignInManager<IdentityApplicationUser>>();
        var refreshTokenProtector =
            bearerTokenOptions.Get(IdentityConstants.BearerScheme).RefreshTokenProtector;
        var refreshTicket = refreshTokenProtector.Unprotect(refreshRequest.RefreshToken);

        // Reject the /refresh attempt with a 401 if the token expired or the security stamp validation fails
        if (refreshTicket?.Properties?.ExpiresUtc is not { } expiresUtc ||
            timeProvider.GetUtcNow() >= expiresUtc ||
            await signInManager.ValidateSecurityStampAsync(refreshTicket.Principal) is not { } user)
            return TypedResults.Challenge();

        var newPrincipal = await signInManager.CreateUserPrincipalAsync(user);
        return TypedResults.SignIn(newPrincipal, authenticationScheme: IdentityConstants.BearerScheme);
    }

    [HttpPost("confirm-email")]
    public async Task<Results<ContentHttpResult, UnauthorizedHttpResult>> ConfirmEmail([FromQuery] string userId,
        [FromQuery] string? changedEmail)
    {
        if (await _userManager.FindByIdAsync(userId) is not { } user)
            // We could respond with a 404 instead of a 401 like Identity UI, but that feels like unnecessary information.
            return TypedResults.Unauthorized();
        IdentityResult result;

        var code = changedEmail is not null
            ? await _userManager.GenerateChangeEmailTokenAsync(user, changedEmail)
            : await _userManager.GenerateEmailConfirmationTokenAsync(user);

        if (string.IsNullOrEmpty(changedEmail))
        {
            result = await _userManager.ConfirmEmailAsync(user, code);
        }
        else
        {
            // As with Identity UI, email and user name are one and the same. So when we update the email,
            // we need to update the user name.
            result = await _userManager.ChangeEmailAsync(user, changedEmail, code);

            if (result.Succeeded) result = await _userManager.SetUserNameAsync(user, changedEmail);
        }

        if (!result.Succeeded) return TypedResults.Unauthorized();

        return TypedResults.Text("Thank you for confirming your email.");
    }

    [HttpPost("forgot-password")]
    public async Task<Results<Ok, ValidationProblem>> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return Utils.CreateValidationProblem(
                IdentityResult.Failed(_userManager.ErrorDescriber.InvalidEmail(request.Email)));
        var code = await _userManager.GeneratePasswordResetTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

        await _emailSender.SendPasswordResetCodeAsync(user, request.Email,
            HtmlEncoder.Default.Encode(code));
        // Send email with the link
        // await _emailSender.SendEmailAsync(request.Email, "Reset Password", $"Please reset your password by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");
        return TypedResults.Ok();
    }

    [HttpPost("reset-password")]
    public async Task<Results<Ok, ValidationProblem>> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user is null || !await _userManager.IsEmailConfirmedAsync(user))
            // Don't reveal that the user does not exist or is not confirmed, so don't return a 200 if we would have
            // returned a 400 for an invalid code given a valid user email.
            return Utils.CreateValidationProblem(IdentityResult.Failed(_userManager.ErrorDescriber.InvalidToken()));

        IdentityResult result;
        try
        {
            var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.ResetCode));
            result = await _userManager.ResetPasswordAsync(user, code, request.NewPassword);
        }
        catch (FormatException)
        {
            result = IdentityResult.Failed(_userManager.ErrorDescriber.InvalidToken());
        }

        if (!result.Succeeded) return Utils.CreateValidationProblem(result);

        return TypedResults.Ok();
    }

    [HttpPost("info")]
    public async Task<Results<Ok<InfoResponse>, ValidationProblem, NotFound>> Info(IHttpContextAccessor context,
        [FromBody] InfoRequest infoRequest)
    {
        if (context?.HttpContext?.User is not { } claimsPrincipal)
            return TypedResults.NotFound();

        if (await _userManager.GetUserAsync(claimsPrincipal) is not { } user)
            return TypedResults.NotFound();

        EmailAddressAttribute emailAddressAttribute = new();

        if (!string.IsNullOrEmpty(infoRequest.NewEmail) && !emailAddressAttribute.IsValid(infoRequest.NewEmail))
            return Utils.CreateValidationProblem(
                IdentityResult.Failed(_userManager.ErrorDescriber.InvalidEmail(infoRequest.NewEmail)));

        if (!string.IsNullOrEmpty(infoRequest.NewPassword))
        {
            if (string.IsNullOrEmpty(infoRequest.OldPassword))
                return Utils.CreateValidationProblem("OldPasswordRequired",
                    "The old password is required to set a new password. If the old password is forgotten, use /resetPassword.");

            var changePasswordResult =
                await _userManager.ChangePasswordAsync(user, infoRequest.OldPassword, infoRequest.NewPassword);
            if (!changePasswordResult.Succeeded) return Utils.CreateValidationProblem(changePasswordResult);
        }

        if (!string.IsNullOrEmpty(infoRequest.NewEmail))
        {
            var email = await _userManager.GetEmailAsync(user);
            await _userManager.SetEmailAsync(user, infoRequest.NewEmail);
        }

        return TypedResults.Ok(await CreateInfoResponseAsync(user, _userManager));
    }

    [HttpGet("info")]
    public async Task<Results<Ok<InfoResponse>, ValidationProblem, NotFound>> Info(IHttpContextAccessor context)
    {
        if (context?.HttpContext?.User is not { } claimsPrincipal)
            return TypedResults.NotFound();

        if (await _userManager.GetUserAsync(claimsPrincipal) is not { } user)
            return TypedResults.NotFound();

        return TypedResults.Ok(await CreateInfoResponseAsync(user, _userManager));
    }

    [HttpPost("logout")]
    public async Task<Results<Ok, UnauthorizedHttpResult>> Logout([FromQuery] bool? useCookies)
    {
        var signInManager = _serviceProvider.GetRequiredService<SignInManager<ApplicationUser>>();
        var useCookieScheme = useCookies == true;
        signInManager.AuthenticationScheme =
            useCookieScheme ? IdentityConstants.ApplicationScheme : IdentityConstants.BearerScheme;
        await signInManager.SignOutAsync();
        return TypedResults.Ok();
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

    private static async Task<InfoResponse> CreateInfoResponseAsync(ApplicationUser user,
        UserManager<ApplicationUser> userManager)
    {
        return new InfoResponse
        {
            Email = await userManager.GetEmailAsync(user) ??
                    throw new NotSupportedException("Users must have an email."),
            IsEmailConfirmed = await userManager.IsEmailConfirmedAsync(user)
        };
    }
}