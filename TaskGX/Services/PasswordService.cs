using System;
using BCrypt.Net;

namespace TaskGX.API.Services
{
    public static class PasswordService
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
            if (senha.Length < 8) return false;
            if (!System.Text.RegularExpressions.Regex.IsMatch(senha, "[A-Z]")) return false;
            if (!System.Text.RegularExpressions.Regex.IsMatch(senha, "[a-z]")) return false;
            if (!System.Text.RegularExpressions.Regex.IsMatch(senha, "[0-9]")) return false;
            if (!System.Text.RegularExpressions.Regex.IsMatch(senha, @"[!@#$%^&*(),.?""':{}|<>_]")) return false;
            return true;
        }
    }
}
