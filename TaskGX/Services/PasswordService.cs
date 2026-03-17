using System.Text.RegularExpressions;

namespace TaskGX.API.Services
{
    public static partial class PasswordService
    {
        public static string Hash(string senha)
        {
            if (string.IsNullOrWhiteSpace(senha))
                throw new ArgumentException("Senha inválida.");

            return BCrypt.Net.BCrypt.HashPassword(senha);
        }

        public static bool Verify(string senhaDigitada, string hashArmazenado)
        {
            return BCrypt.Net.BCrypt.Verify(senhaDigitada, hashArmazenado);
        }

        public static bool IsValid(string senha)
        {
            if (string.IsNullOrWhiteSpace(senha) || senha.Length < 8) return false;
            if (!UppercaseRegex().IsMatch(senha)) return false;
            if (!LowercaseRegex().IsMatch(senha)) return false;
            if (!DigitRegex().IsMatch(senha)) return false;
            if (!SpecialCharacterRegex().IsMatch(senha)) return false;
            return true;
        }

        [GeneratedRegex("[A-Z]")]
        private static partial Regex UppercaseRegex();

        [GeneratedRegex("[a-z]")]
        private static partial Regex LowercaseRegex();

        [GeneratedRegex("[0-9]")]
        private static partial Regex DigitRegex();

        [GeneratedRegex("[!@#$%^&*(),.?\\\":{}|<>_]")]
        private static partial Regex SpecialCharacterRegex();
    }
}
