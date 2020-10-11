using System.Security.Cryptography;
using System.Web.Helpers;

namespace IceCreamSystem.Services
{
    public class HashService
    {
        private static string GetRandomSalt()
        {
            return Crypto.GenerateSalt(12);
        }

        public static string HashPassword(string password)
        {
            //password =  GetRandomSalt() + password;

            //return Crypto.HashPassword(password); //RFC 2898
            
            //return Crypto.Hash(password, "md5"); //md5 -> 128 bits in 32 caracteres it's smaller than "sha1"

            //return Crypto.Hash(password, "sha1"); //sha1 -> SHA1's smaller bit size, it has become more susceptible to attacks if compare with sha256 | OUTPUT SIZE (BITS): 160 (5 × 32)

            return Crypto.Hash(password); //default = sha256  | OUTPUT SIZE (BITS): 256 (8 × 32) 
        }

        public static bool ValidatePassword(string password, string correctHash)
        {
            password = HashPassword(password);
            return correctHash.Equals(password);
        }
    }
}