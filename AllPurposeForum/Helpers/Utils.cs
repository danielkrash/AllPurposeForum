using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.General;
using System.Diagnostics;
using System.Text;
using System.Text.Encodings.Web;

namespace AllPurposeForum.Helpers
{
    public static class Utils
    {


        public static ValidationProblem CreateValidationProblem(string errorCode, string errorDescription)
        {
            return TypedResults.ValidationProblem(new Dictionary<string, string[]>
        {
            { errorCode, [errorDescription] }
        });
        }

//        public static async Task Confirm(IdentityApplicationUser user, UserManager<IdentityApplicationUser> userManager, HttpContext context, IServiceProvider serviceProvider,
//    string email, bool isChange = false
//)
//        {
//            var user
//            if (await userManager.FindByIdAsync(userId) is not { } user)
//                // We could respond with a 404 instead of a 401 like Identity UI, but that feels like unnecessary information.
//                return TypedResults.Unauthorized();

//            try
//            {
//                code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
//            }
//            catch (FormatException)
//            {
//                return TypedResults.Unauthorized();
//            }

//            IdentityResult result;

//            if (string.IsNullOrEmpty(changedEmail))
//            {
//                result = await userManager.ConfirmEmailAsync(user, code);
//            }
//            else
//            {
//                // As with Identity UI, email and user name are one and the same. So when we update the email,
//                // we need to update the user name.
//                result = await userManager.ChangeEmailAsync(user, changedEmail, code);

//                if (result.Succeeded) result = await userManager.SetUserNameAsync(user, changedEmail);
//            }

//            if (!result.Succeeded) return TypedResults.Unauthorized();

//            return TypedResults.Text("Thank you for confirming your email.");
//        }

        public static ValidationProblem CreateValidationProblem(IdentityResult result)
        {
            // We expect a single error code and description in the normal case.
            // This could be golfed with GroupBy and ToDictionary, but perf! :P
            Debug.Assert(!result.Succeeded);
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
                    newDescriptions = [error.Description];
                }

                errorDictionary[error.Code] = newDescriptions;
            }

            return TypedResults.ValidationProblem(errorDictionary);
        }
    }
}
