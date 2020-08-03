#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
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

namespace Dev2.Services.Security
{
    public class SecurityEncryption
    {
        const string InitVector = "@1B2c3D4e5F6g7H8";
        const string PassPhrase = "Pas5pr@se";
        const string SaltValue = "s@1tValue";
        const string HashAlgorithm = "SHA1";
        const int PasswordIterations = 2;
        const int KeySize = 256;

        protected SecurityEncryption()
        {
        }

        public static string Encrypt(string plainText)
        {
            var initVectorBytes = Encoding.ASCII.GetBytes(InitVector);
            var saltValueBytes = Encoding.ASCII.GetBytes(SaltValue);
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            var password = new PasswordDeriveBytes(
                PassPhrase,
                saltValueBytes,
                HashAlgorithm,
                PasswordIterations);
#pragma warning disable 612,618
            var keyBytes = password.GetBytes(KeySize / 8);
#pragma warning restore 612,618
            var symmetricKey = new RijndaelManaged { Mode = CipherMode.CBC, Padding = PaddingMode.Zeros };
            var encryptor = symmetricKey.CreateEncryptor(
                keyBytes,
                initVectorBytes);
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
            var saltValueBytes = Encoding.ASCII.GetBytes(SaltValue);
            var cipherTextBytes = Convert.FromBase64String(cipherText);
            var password = new PasswordDeriveBytes(
                PassPhrase,
                saltValueBytes,
                HashAlgorithm,
                PasswordIterations);
#pragma warning disable 612,618
            var keyBytes = password.GetBytes(KeySize / 8);
#pragma warning restore 612,618            
            var symmetricKey = new RijndaelManaged { Mode = CipherMode.CBC, Padding = PaddingMode.Zeros };
            var decryptor = symmetricKey.CreateDecryptor(
                keyBytes,
                initVectorBytes);
            byte[] plainTextBytes;
            using (var memoryStream = new MemoryStream(cipherTextBytes))
            {
                using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Write))
                {
                    cryptoStream.Write(cipherTextBytes, 0, cipherTextBytes.Length);
                }
                plainTextBytes = memoryStream.ToArray();
            }
            var plainText = Encoding.UTF8.GetString(plainTextBytes);  
            return plainText;
        }
    }
}