/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Security.Encryption;

namespace Dev2.Common.Tests
{
    /// <summary>
    /// Base class that configures <see cref="DpapiWrapper"/> to use AES-256-CBC
    /// instead of DPAPI before each test, enabling tests to run on platforms where
    /// DPAPI is unavailable (e.g. Linux CI agents).
    /// </summary>
    [TestClass]
    public abstract class DpapiTestBase
    {
        private static readonly byte[] _aesKey = Encoding.UTF8.GetBytes("WarewolfTestHardcodedAESKey12345");

        static DpapiTestBase() => InstallHooks();

        [TestInitialize]
        public void SetupAesEncryption() => InstallHooks();

        private static void InstallHooks()
        {
            DpapiWrapper.AesEncryptHook = plainText =>
            {
                using var aes = Aes.Create();
                aes.Key = _aesKey;
                aes.GenerateIV();
                using var encryptor = aes.CreateEncryptor();
                var data = Encoding.Unicode.GetBytes(plainText);
                var cipher = encryptor.TransformFinalBlock(data, 0, data.Length);
                var result = new byte[aes.IV.Length + cipher.Length];
                aes.IV.CopyTo(result, 0);
                cipher.CopyTo(result, aes.IV.Length);
                return "WFAES::" + Convert.ToBase64String(result);
            };

            DpapiWrapper.AesDecryptHook = cipher =>
            {
                var payload = Convert.FromBase64String(cipher.Substring("WFAES::".Length));
                var iv = payload[..16];
                var cipherBytes = payload[16..];
                using var aes = Aes.Create();
                aes.Key = _aesKey;
                aes.IV = iv;
                using var decryptor = aes.CreateDecryptor();
                var data = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
                return Encoding.Unicode.GetString(data);
            };
        }
    }
}
