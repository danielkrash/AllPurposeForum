using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.General;
using System.Diagnostics;
using System.Text;
using System.Text.Encodings.Web;
using System;

namespace AllPurposeForum.Helpers
{
    public static class Utils
    {
        public static ValidationProblem CreateValidationProblem(string errorCode, string errorDescription)
        {
            return TypedResults.ValidationProblem(new Dictionary<string, string[]>
            {
                { errorCode, new[] { errorDescription } }
            });
        }

        public static string TimeAgo(DateTime? dateTime)
        {
            if (!dateTime.HasValue)
            {
                return "Unknown";
            }

            var timeSpan = DateTime.UtcNow - dateTime.Value;

            if (timeSpan.TotalSeconds < 60)
            {
                return $"{(int)timeSpan.TotalSeconds} seconds ago";
            }
            if (timeSpan.TotalMinutes < 60)
            {
                return $"{(int)timeSpan.TotalMinutes} minutes ago";
            }
            if (timeSpan.TotalHours < 24)
            {
                return $"{(int)timeSpan.TotalHours} hours ago";
            }
            if (timeSpan.TotalDays < 30)
            {
                return $"{(int)timeSpan.TotalDays} days ago";
            }
            if (timeSpan.TotalDays < 365)
            {
                return $"{(int)(timeSpan.TotalDays / 30)} months ago";
            }
            return $"{(int)(timeSpan.TotalDays / 365)} years ago";
        }

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
                    newDescriptions = new[] { error.Description };
                }

                errorDictionary[error.Code] = newDescriptions;
            }

            return TypedResults.ValidationProblem(errorDictionary);
        }
    }
}
