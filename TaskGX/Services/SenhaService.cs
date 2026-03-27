using System.Text.RegularExpressions;

namespace TaskGX.API.Services
{
    public static partial class SenhaService
    {
        public static string GerarHash(string senha)
        {
            if (string.IsNullOrWhiteSpace(senha))
                throw new ArgumentException("Senha invalida.");

            return BCrypt.Net.BCrypt.HashPassword(senha);
        }

        public static bool Verificar(string senhaInformada, string hashArmazenado)
        {
            return BCrypt.Net.BCrypt.Verify(senhaInformada, hashArmazenado);
        }

        public static bool EhValida(string senha)
        {
            if (string.IsNullOrWhiteSpace(senha) || senha.Length < 8)
                return false;

            if (!RegexMaiuscula().IsMatch(senha))
                return false;

            if (!RegexMinuscula().IsMatch(senha))
                return false;

            if (!RegexDigito().IsMatch(senha))
                return false;

            if (!RegexCaractereEspecial().IsMatch(senha))
                return false;

            return true;
        }

        [GeneratedRegex("[A-Z]")]
        private static partial Regex RegexMaiuscula();

        [GeneratedRegex("[a-z]")]
        private static partial Regex RegexMinuscula();

        [GeneratedRegex("[0-9]")]
        private static partial Regex RegexDigito();

        [GeneratedRegex("[!@#$%^&*(),.?\\\":{}|<>_]")]
        private static partial Regex RegexCaractereEspecial();
    }
}
