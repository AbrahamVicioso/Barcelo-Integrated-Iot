using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;

namespace Authentication.Api.Utils.Commons
{
    public static class IdentityResultExtension
    {
        public static ValidationProblem ToValidationProblem(this IdentityResult result)
        {
            var errorDictionary = new Dictionary<string, string[]>(1);

            foreach (var error in result.Errors)
            {
                string[] newDescriptions;

                if (errorDictionary.TryGetValue(error.Code, out var desriptions))
                {
                    newDescriptions = new string[desriptions.Length + 1];
                    Array.Copy(desriptions, newDescriptions, desriptions.Length);
                    newDescriptions[desriptions.Length] = error.Description;
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
