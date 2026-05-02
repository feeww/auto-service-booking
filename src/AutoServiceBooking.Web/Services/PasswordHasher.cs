using System.Security.Cryptography;

namespace AutoServiceBooking.Web.Services
{
    public class PasswordHasher : IPasswordHasher
    {
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int Iterations = 100000;

        public string HashPassword(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, HashSize);

            string saltText = Convert.ToBase64String(salt);
            string hashText = Convert.ToBase64String(hash);

            return Iterations + "." + saltText + "." + hashText;
        }

        public bool VerifyPassword(string password, string passwordHash)
        {
            string[] parts = passwordHash.Split('.', 3);

            if (parts.Length != 3){
                return false;
            }

            if (!int.TryParse(parts[0], out int iterations)){
                return false;
            }

            byte[] salt = Convert.FromBase64String(parts[1]);
            byte[] expectedHash = Convert.FromBase64String(parts[2]);
            byte[] actualHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expectedHash.Length);

            return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
        }
    }
}
