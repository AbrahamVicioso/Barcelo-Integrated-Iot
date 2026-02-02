using System.Security.Cryptography;
using System.Text;

namespace Authentication.Api.Utils
{
    public static class RandomPasswordGenerator
    {
        private const string LowercaseLetters = "abcdefghijklmnopqrstuvwxyz";
        private const string UppercaseLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string Digits = "0123456789";
        private const string SpecialCharacters = "!@#$%^&*";
        private const string AllCharacters = LowercaseLetters + UppercaseLetters + Digits + SpecialCharacters;

        public static string Generate(int length = 10)
        {
            if (length < 8)
                length = 8;
            if (length > 20)
                length = 20;

            var result = new StringBuilder();
            var random = new Random();

            // Ensure at least one of each required type
            result.Append(LowercaseLetters[random.Next(LowercaseLetters.Length)]);
            result.Append(UppercaseLetters[random.Next(UppercaseLetters.Length)]);
            result.Append(Digits[random.Next(Digits.Length)]);
            result.Append(SpecialCharacters[random.Next(SpecialCharacters.Length)]);

            // Fill the rest with random characters
            for (int i = 4; i < length; i++)
            {
                result.Append(AllCharacters[random.Next(AllCharacters.Length)]);
            }

            // Shuffle the result
            return ShuffleString(result.ToString());
        }

        private static string ShuffleString(string input)
        {
            var random = new Random();
            var array = input.ToCharArray();
            int n = array.Length;

            for (int i = n - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                var temp = array[i];
                array[i] = array[j];
                array[j] = temp;
            }

            return new string(array);
        }
    }
}
