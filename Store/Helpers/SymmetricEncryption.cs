using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Store.Helpers
{
    public static class SymmetricEncryption
    {
        // Generate a random key for AES encryption
        private static readonly byte[] Key = Encoding.UTF8.GetBytes("KeyKeyKeyKeyKeyK"); // Must be 16, 24, or 32 bytes long
        private static readonly byte[] IV = Encoding.UTF8.GetBytes("KeyKeyKeyKeyKeyK");  // 16 bytes IV (Initialization Vector)

        // Encrypt a string using AES
        public static string Encrypt(string plainText)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = Key;
                aes.IV = IV;
                aes.Mode = CipherMode.CBC; // Use CBC mode

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                        cryptoStream.Write(plainBytes, 0, plainBytes.Length);
                        cryptoStream.FlushFinalBlock();

                        byte[] cipherBytes = memoryStream.ToArray();
                        return Convert.ToBase64String(cipherBytes);
                    }
                }
            }
        }

        // Decrypt an encrypted string using AES
        public static string Decrypt(string cipherText)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = Key;
                aes.IV = IV;
                aes.Mode = CipherMode.CBC; // Use CBC mode

                using (MemoryStream memoryStream = new MemoryStream(Convert.FromBase64String(cipherText)))
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Read))
                    {
                        byte[] plainBytes = new byte[cipherText.Length];
                        int decryptedByteCount = cryptoStream.Read(plainBytes, 0, plainBytes.Length);

                        return Encoding.UTF8.GetString(plainBytes, 0, decryptedByteCount);
                    }
                }
            }
        }
    }

}