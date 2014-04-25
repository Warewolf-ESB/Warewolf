///////////////////////////////////////////////////////////////////////////////
// SAMPLE: Symmetric key encryption and decryption using Rijndael algorithm.
// 
// To run this sample, create a new Visual C# project using the Console
// Application template and replace the contents of the Class1.cs file with
// the code below.
//
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED 
// WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
// 
// Copyright (C) 2002 Obviex(TM). All rights reserved.
// 

#region

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

#endregion

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    ///     This class uses a symmetric key algorithm (Rijndael/AES) to encrypt and
    ///     decrypt data. As long as encryption and decryption routines use the same
    ///     parameters to generate the keys, the keys are guaranteed to be the same.
    ///     The class uses static functions with duplicate code to make it easier to
    ///     demonstrate encryption and decryption logic. In a real-life application,
    ///     this may not be the most efficient way of handling encryption, so - as
    ///     soon as you feel comfortable with it - you may want to redesign this class.
    /// </summary>
    public class SecurityEncryption
    {
        const string InitVector = "@1B2c3D4e5F6g7H8"; // Must be 16 bytes  
        const string PassPhrase = "Pas5pr@se"; // Any string  
        const string SaltValue = "s@1tValue"; // Any string  
        const string HashAlgorithm = "SHA1"; // Can also be "MD5", "SHA1" is stronger  
        const int PasswordIterations = 2; // Can be any number, usually 1 or 2         
        const int KeySize = 256;

        /// <summary>
        ///     Encrypts specified plaintext using Rijndael symmetric key algorithm
        ///     and returns a base64-encoded result.
        /// </summary>
        /// <param name="plainText">
        ///     Plaintext value to be encrypted.
        /// </param>
        /// <returns>
        ///     Encrypted value formatted as a base64-encoded string.
        /// </returns>
        public static string Encrypt(string plainText)
        {
            // Convert strings into byte arrays.
            // Let us assume that strings only contain ASCII codes.
            // If strings include Unicode characters, use Unicode, UTF7, or UTF8 
            // encoding.
            var initVectorBytes = Encoding.ASCII.GetBytes(InitVector);
            var saltValueBytes = Encoding.ASCII.GetBytes(SaltValue);

            // Convert our plaintext into a byte array.
            // Let us assume that plaintext contains UTF8-encoded characters.
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);

            // First, we must create a password, from which the key will be derived.
            // This password will be generated from the specified passphrase and 
            // salt value. The password will be created using the specified hash 
            // algorithm. Password creation can be done in several iterations.
            var password = new PasswordDeriveBytes(
                PassPhrase,
                saltValueBytes,
                HashAlgorithm,
                PasswordIterations);

            // Use the password to generate pseudo-random bytes for the encryption
            // key. Specify the size of the key in bytes (instead of bits).
#pragma warning disable 612,618
            var keyBytes = password.GetBytes(KeySize / 8);
#pragma warning restore 612,618

            // Create uninitialized Rijndael encryption object.
            var symmetricKey = new RijndaelManaged { Mode = CipherMode.CBC };

            // It is reasonable to set encryption mode to Cipher Block Chaining
            // (CBC). Use default options for other symmetric key parameters.

            // Generate encryptor from the existing key bytes and initialization 
            // vector. Key size will be defined based on the number of the key 
            // bytes.
            var encryptor = symmetricKey.CreateEncryptor(
                keyBytes,
                initVectorBytes);

            // Define memory stream which will be used to hold encrypted data.
            using(var memoryStream = new MemoryStream())
            {

                // Define cryptographic stream (always use Write mode for encryption).
                using(var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                {
                    // Start encrypting.
                    cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);

                    // Finish encrypting.
                    cryptoStream.FlushFinalBlock();

                    // Convert our encrypted data from a memory stream into a byte array.
                    var cipherTextBytes = memoryStream.ToArray();

                    // Close both streams.
                    memoryStream.Close();
                    cryptoStream.Close();

                    // Convert encrypted data into a base64-encoded string.
                    var cipherText = Convert.ToBase64String(cipherTextBytes);

                    // Return encrypted string.
                    return cipherText;
                }
            }
        }

        /// <summary>
        ///     Decrypts specified ciphertext using Rijndael symmetric key algorithm.
        /// </summary>
        /// <param name="cipherText">
        ///     Base64-formatted ciphertext value.
        /// </param>
        /// <returns>
        ///     Decrypted string value.
        /// </returns>
        /// <remarks>
        ///     Most of the logic in this function is similar to the Encrypt
        ///     logic. In order for decryption to work, all parameters of this function
        ///     - except cipherText value - must match the corresponding parameters of
        ///     the Encrypt function which was called to generate the
        ///     ciphertext.
        /// </remarks>
        public static string Decrypt(string cipherText)
        {
            // Convert strings defining encryption key characteristics into byte
            // arrays. Let us assume that strings only contain ASCII codes.
            // If strings include Unicode characters, use Unicode, UTF7, or UTF8
            // encoding.
            var initVectorBytes = Encoding.ASCII.GetBytes(InitVector);
            var saltValueBytes = Encoding.ASCII.GetBytes(SaltValue);

            // Convert our ciphertext into a byte array.
            var cipherTextBytes = Convert.FromBase64String(cipherText);

            // First, we must create a password, from which the key will be 
            // derived. This password will be generated from the specified 
            // passphrase and salt value. The password will be created using
            // the specified hash algorithm. Password creation can be done in
            // several iterations.
            var password = new PasswordDeriveBytes(
                PassPhrase,
                saltValueBytes,
                HashAlgorithm,
                PasswordIterations);

            // Use the password to generate pseudo-random bytes for the encryption
            // key. Specify the size of the key in bytes (instead of bits).
#pragma warning disable 612,618
            var keyBytes = password.GetBytes(KeySize / 8);
#pragma warning restore 612,618

            // Create uninitialized Rijndael encryption object.
            var symmetricKey = new RijndaelManaged { Mode = CipherMode.CBC };

            // It is reasonable to set encryption mode to Cipher Block Chaining
            // (CBC). Use default options for other symmetric key parameters.

            // Generate decryptor from the existing key bytes and initialization 
            // vector. Key size will be defined based on the number of the key 
            // bytes.
            var decryptor = symmetricKey.CreateDecryptor(
                keyBytes,
                initVectorBytes);

            // Define memory stream which will be used to hold encrypted data.
            using(var memoryStream = new MemoryStream(cipherTextBytes))
            {

                // Define cryptographic stream (always use Read mode for encryption).
                using(var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                {

                    // Since at this point we don't know what the size of decrypted data
                    // will be, allocate the buffer long enough to hold ciphertext;
                    // plaintext is never longer than ciphertext.
                    var plainTextBytes = new byte[cipherTextBytes.Length];

                    // Start decrypting.
                    var decryptedByteCount = cryptoStream.Read(plainTextBytes,
                        0,
                        plainTextBytes.Length);

                    // Close both streams.
                    memoryStream.Close();
                    cryptoStream.Close();

                    // Convert decrypted data into a string. 
                    // Let us assume that the original plaintext string was UTF8-encoded.
                    var plainText = Encoding.UTF8.GetString(plainTextBytes,
                        0,
                        decryptedByteCount);

                    // Return decrypted string.   
                    return plainText;
                }
            }
        }
    }
}