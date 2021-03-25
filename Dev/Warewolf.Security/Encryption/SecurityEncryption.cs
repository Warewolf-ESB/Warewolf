/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU General Public License <http://www.gnu.org/licenses/gpl.html>
*/

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Warewolf.Security.Encryption
{
    public static class SecurityEncryption
    {
        const string InitVector = "@1B2c3D4e5F6g7H8";
        const string PassPhrase = "Pas5pr@se";
        const string SaltValue = "s@1tValue";

        public static string Encrypt(string plainText)
        {
            var initVectorBytes = Encoding.ASCII.GetBytes(InitVector);
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText.Trim());

            var keyBytes = GenerateByteKey();

            var symmetricKey = new RijndaelManaged {Mode = CipherMode.CBC};
            var encryptor = symmetricKey.CreateEncryptor(keyBytes, initVectorBytes);
            byte[] cipherTextBytes;
            using (var memoryStream = new MemoryStream())
            {
                using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                {
                    cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                    cryptoStream.FlushFinalBlock();
                }

                cipherTextBytes = memoryStream.ToArray();
            }

            var cipherText = Convert.ToBase64String(cipherTextBytes);
            return cipherText;
        }

        public static string Decrypt(string cipherText)
        {
            var initVectorBytes = Encoding.ASCII.GetBytes(InitVector);
            var cipherTextBytes = Convert.FromBase64String(cipherText);

            var keyBytes = GenerateByteKey();

            var symmetricKey = new RijndaelManaged {Mode = CipherMode.CBC};
            var decryptor = symmetricKey.CreateDecryptor(keyBytes, initVectorBytes);
            byte[] plainTextBytes;
            int decryptedByteCount;
            using (var memoryStream = new MemoryStream(cipherTextBytes))
            {
                using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                {
                    plainTextBytes = new byte[cipherTextBytes.Length];
                    decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                }
            }

            var decrypted = Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
            return decrypted;
        }

        private static byte[] GenerateByteKey()
        {
            var key = PassPhrase + "|" + SaltValue;
            var passwordBytes = Encoding.ASCII.GetBytes(key);
            var hmac = new HMACSHA256(passwordBytes);
            var keyBytes = new byte[hmac.HashSize / 8];
            return keyBytes;
        }

        public static bool CanBeDecrypted(this string cipher)
        {
            if (string.IsNullOrEmpty(cipher))
            {
                return false;
            }

            if (!cipher.IsBase64())
            {
                return false;
            }

            try
            {
                Decrypt(cipher);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public static string DecryptIfEncrypted(string input)
        {
            if (string.IsNullOrEmpty(input) || string.IsNullOrWhiteSpace(input) || !input.IsBase64())
            {
                return input;
            }

            return Decrypt(input);
        }

        public static string EncryptIfDecrypted(string input)
        {
            if (string.IsNullOrEmpty(input) || string.IsNullOrWhiteSpace(input))
            {
                return input;
            }

            if (input.IsBase64() && input.CanBeDecrypted())
            {
                return input;
            }

            return Encrypt(input);
        }

        public static bool IsBase64(this string base64String)
        {
            if (base64String.Contains(" ") || base64String.Contains("\t") || base64String.Contains("\r") || base64String.Contains("\n"))
            {
                return false;
            }

            if (string.IsNullOrEmpty(base64String) || base64String.Length % 4 != 0)
            {
                return false;
            }

            try
            {
                Convert.FromBase64String(base64String);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}