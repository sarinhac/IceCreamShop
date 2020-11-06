using System.Security.Cryptography;
using System.Text;

namespace IceCreamSystem.Services
{
    public class HashService
    {
        public static string HashPassword(string password)
        {
            //return Crypto.HashPassword(password); //RFC 2898

            //return Crypto.Hash(password, "md5"); //md5 -> 128 bits in 32 caracteres it's smaller than "sha1"

            //return Crypto.Hash(password, "sha1"); //sha1 -> SHA1's smaller bit size, it has become more susceptible to attacks if compare with sha256 | OUTPUT SIZE (BITS): 160 (5 × 32)

            //return Crypto.Hash(password); //default = sha256  | OUTPUT SIZE (BITS): 256 (8 × 32) 

            var bytes = Encoding.UTF8.GetBytes(password);

            using (var hash = SHA512.Create())
            {
                var hashedInputBytes = hash.ComputeHash(bytes);

                // Convert to text
                // StringBuilder Capacity is 128, because 512 bits / 8 bits in byte * 2 symbols for byte 
                var hashedInputStringBuilder = new StringBuilder(128);
                foreach (var b in hashedInputBytes)
                    hashedInputStringBuilder.Append(b.ToString("X2"));
                return hashedInputStringBuilder.ToString();
            }
        }

        public static bool ValidatePassword(string password, string correctHash)
        {
            password = HashPassword(password);
            return correctHash.Equals(password);
        }
    }
}