﻿#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Security.Cryptography;
using System.Text;

namespace Warewolf.Security.Encryption
{
    public static class DpapiWrapper
    {
        const DataProtectionScope DataProtectionScope = System.Security.Cryptography.DataProtectionScope.LocalMachine;

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
            if(string.IsNullOrEmpty(input) || string.IsNullOrWhiteSpace(input))
            {
                return input;
            }

            if (input.IsBase64() && input.CanBeDecrypted())
            {
                return input;
            }

            return Encrypt(input);
        }
        /// <summary>
        /// Encrypts a given password and returns the encrypted data
        /// as a base64 string.
        /// </summary>
        /// <param name="plainText">An unencrypted string that needs
        /// to be secured.</param>
        /// <returns>A base64 encoded string that represents the encrypted
        /// binary data.
        /// </returns>
        /// <remarks>This solution is not really secure as we are
        /// keeping strings in memory. If runtime protection is essential,
        /// <see cref="SecureString"/> should be used.</remarks>
        /// <exception cref="ArgumentNullException">If <paramref name="plainText"/>
        /// is a null reference.</exception>
        public static string Encrypt(string plainText)
        {
            if (plainText == null)
            {
                throw new ArgumentNullException(nameof(plainText));
            }

            //encrypt data
            var data = Encoding.Unicode.GetBytes(plainText);
            var encrypted = ProtectedData.Protect(data, null, DataProtectionScope);

            //return as base64 string
            return Convert.ToBase64String(encrypted);
        }

        /// <summary>
        /// Decrypts a given string.
        /// </summary>
        /// <param name="cipher">A base64 encoded string that was created
        /// through the <see cref="Encrypt(string)"/> or
        /// <see cref="Encrypt(string)"/> extension methods.</param>
        /// <returns>The decrypted string.</returns>
        /// <remarks>Keep in mind that the decrypted string remains in memory
        /// and makes your application vulnerable per se. If runtime protection
        /// is essential, <see cref="SecureString"/> should be used.</remarks>
        /// <exception cref="ArgumentNullException">If <paramref name="cipher"/>
        /// is a null reference.</exception>
        public static string Decrypt(string cipher)
        {
            if (cipher == null)
            {
                throw new ArgumentNullException(nameof(cipher));
            }

            if (!cipher.IsBase64())
            {
                throw new ArgumentException("cipher must be base64 encoded");
            }

            //parse base64 string
            var data = Convert.FromBase64String(cipher);

            //decrypt data
            var decrypted = ProtectedData.Unprotect(data, null, DataProtectionScope);
            return Encoding.Unicode.GetString(decrypted);
        }

        /// <summary>
        /// Decrypts a given string.
        /// </summary>
        /// <param name="cipher">A base64 encoded string that was created
        /// through the <see cref="Encrypt(string)"/> or
        /// <see cref="Encrypt(string)"/> extension methods.</param>
        /// <returns>The decrypted string.</returns>
        /// <remarks>Keep in mind that the decrypted string remains in memory
        /// and makes your application vulnerable per se. If runtime protection
        /// is essential, <see cref="SecureString"/> should be used.</remarks>
        /// <exception cref="ArgumentNullException">If <paramref name="cipher"/>
        /// is a null reference.</exception>
        public static bool CanBeDecrypted(this string cipher)
        {
            if(string.IsNullOrEmpty(cipher))
            {
                return false;
            }

            if (!cipher.IsBase64())
            {
                return false;
            }

            //parse base64 string
            var data = Convert.FromBase64String(cipher);

            //decrypt data
            try
            {
                ProtectedData.Unprotect(data, null, DataProtectionScope);
            }
            catch(Exception)
            {
                return false;
            }
            return true;
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