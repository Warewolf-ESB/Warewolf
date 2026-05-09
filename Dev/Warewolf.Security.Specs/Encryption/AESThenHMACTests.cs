#pragma warning disable S1481, S101, CC0091, S1226, S100, CC0044, CC0045, CC0021, CC0022, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001, IDE0019, CC0105, RECS008, CA2202, RECS005, IDE0016
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
using System.Text;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.Security.Encryption
{
    [TestClass]
    public class AESThenHMACTests
    {
        const string _message = "Hello, Warewolf!";
        const string _password = "correct-horse-battery-staple!";

        static byte[] NewCryptKey() => AESThenHMAC.NewKey();
        static byte[] NewAuthKey()  => AESThenHMAC.NewKey();

        // ── NewKey ────────────────────────────────────────────────────────────

        [TestMethod]
        [Owner("Copilot")]
        [TestCategory("ServerPermissionsSecurity")]
        public void NewKey_ReturnsBytesOfCorrectLength()
        {
            var key = AESThenHMAC.NewKey();
            key.Should().NotBeNull();
            key.Length.Should().Be(AESThenHMAC.KeyBitSize / 8);
        }

        [TestMethod]
        [Owner("Copilot")]
        [TestCategory("ServerPermissionsSecurity")]
        public void NewKey_ReturnsDifferentValuesOnEachCall()
        {
            var key1 = AESThenHMAC.NewKey();
            var key2 = AESThenHMAC.NewKey();
            key1.Should().NotBeEquivalentTo(key2);
        }

        // ── SimpleEncrypt / SimpleDecrypt (string overloads) ─────────────────

        [TestMethod]
        [Owner("Copilot")]
        [TestCategory("ServerPermissionsSecurity")]
        public void SimpleEncryptDecrypt_RoundTrip_ReturnsOriginalMessage()
        {
            var cryptKey = NewCryptKey();
            var authKey  = NewAuthKey();

            var encrypted = AESThenHMAC.SimpleEncrypt(_message, cryptKey, authKey);
            var decrypted = AESThenHMAC.SimpleDecrypt(encrypted, cryptKey, authKey);

            decrypted.Should().Be(_message);
        }

        [TestMethod]
        [Owner("Copilot")]
        [TestCategory("ServerPermissionsSecurity")]
        public void SimpleEncrypt_ProducesBase64Output_DifferentFromPlaintext()
        {
            var encrypted = AESThenHMAC.SimpleEncrypt(_message, NewCryptKey(), NewAuthKey());

            encrypted.Should().NotBe(_message);
            Convert.FromBase64String(encrypted).Should().NotBeNullOrEmpty();
        }

        [TestMethod]
        [Owner("Copilot")]
        [TestCategory("ServerPermissionsSecurity")]
        public void SimpleEncrypt_ProducesDifferentCiphertextEachCall_DueToRandomIV()
        {
            var cryptKey = NewCryptKey();
            var authKey  = NewAuthKey();

            var enc1 = AESThenHMAC.SimpleEncrypt(_message, cryptKey, authKey);
            var enc2 = AESThenHMAC.SimpleEncrypt(_message, cryptKey, authKey);

            enc1.Should().NotBe(enc2);
        }

        [TestMethod]
        [Owner("Copilot")]
        [TestCategory("ServerPermissionsSecurity")]
        public void SimpleEncrypt_EmptySecretMessage_ThrowsArgumentException()
        {
            Action act = () => AESThenHMAC.SimpleEncrypt("", NewCryptKey(), NewAuthKey());
            act.Should().Throw<ArgumentException>().WithParameterName("secretMessage");
        }

        [TestMethod]
        [Owner("Copilot")]
        [TestCategory("ServerPermissionsSecurity")]
        public void SimpleEncrypt_NullSecretMessage_ThrowsArgumentException()
        {
            Action act = () => AESThenHMAC.SimpleEncrypt((string)null, NewCryptKey(), NewAuthKey());
            act.Should().Throw<ArgumentException>().WithParameterName("secretMessage");
        }

        [TestMethod]
        [Owner("Copilot")]
        [TestCategory("ServerPermissionsSecurity")]
        public void SimpleEncrypt_NullCryptKey_ThrowsArgumentException()
        {
            Action act = () => AESThenHMAC.SimpleEncrypt(_message, null, NewAuthKey());
            act.Should().Throw<ArgumentException>().WithParameterName("cryptKey");
        }

        [TestMethod]
        [Owner("Copilot")]
        [TestCategory("ServerPermissionsSecurity")]
        public void SimpleEncrypt_WrongSizeCryptKey_ThrowsArgumentException()
        {
            Action act = () => AESThenHMAC.SimpleEncrypt(_message, new byte[8], NewAuthKey());
            act.Should().Throw<ArgumentException>().WithParameterName("cryptKey");
        }

        [TestMethod]
        [Owner("Copilot")]
        [TestCategory("ServerPermissionsSecurity")]
        public void SimpleEncrypt_NullAuthKey_ThrowsArgumentException()
        {
            Action act = () => AESThenHMAC.SimpleEncrypt(_message, NewCryptKey(), null);
            act.Should().Throw<ArgumentException>().WithParameterName("authKey");
        }

        [TestMethod]
        [Owner("Copilot")]
        [TestCategory("ServerPermissionsSecurity")]
        public void SimpleDecrypt_EmptyEncryptedMessage_ThrowsArgumentException()
        {
            Action act = () => AESThenHMAC.SimpleDecrypt("", NewCryptKey(), NewAuthKey());
            act.Should().Throw<ArgumentException>().WithParameterName("encryptedMessage");
        }

        [TestMethod]
        [Owner("Copilot")]
        [TestCategory("ServerPermissionsSecurity")]
        public void SimpleDecrypt_WhitespaceEncryptedMessage_ThrowsArgumentException()
        {
            Action act = () => AESThenHMAC.SimpleDecrypt("   ", NewCryptKey(), NewAuthKey());
            act.Should().Throw<ArgumentException>().WithParameterName("encryptedMessage");
        }

        [TestMethod]
        [Owner("Copilot")]
        [TestCategory("ServerPermissionsSecurity")]
        public void SimpleDecrypt_TamperedCiphertext_ReturnsNull()
        {
            var cryptKey  = NewCryptKey();
            var authKey   = NewAuthKey();
            var encrypted = AESThenHMAC.SimpleEncrypt(_message, cryptKey, authKey);

            // Flip a byte in the middle of the ciphertext
            var bytes = Convert.FromBase64String(encrypted);
            bytes[bytes.Length / 2] ^= 0xFF;
            var tampered = Convert.ToBase64String(bytes);

            var result = AESThenHMAC.SimpleDecrypt(tampered, cryptKey, authKey);
            result.Should().BeNull();
        }

        [TestMethod]
        [Owner("Copilot")]
        [TestCategory("ServerPermissionsSecurity")]
        public void SimpleDecrypt_WrongCryptKey_ReturnsNullOrThrows()
        {
            var authKey   = NewAuthKey();
            var encrypted = AESThenHMAC.SimpleEncrypt(_message, NewCryptKey(), authKey);

            // HMAC check will fail first → null
            var result = AESThenHMAC.SimpleDecrypt(encrypted, NewCryptKey(), authKey);
            result.Should().BeNull();
        }

        [TestMethod]
        [Owner("Copilot")]
        [TestCategory("ServerPermissionsSecurity")]
        public void SimpleDecrypt_WrongAuthKey_ReturnsNull()
        {
            var cryptKey  = NewCryptKey();
            var encrypted = AESThenHMAC.SimpleEncrypt(_message, cryptKey, NewAuthKey());

            var result = AESThenHMAC.SimpleDecrypt(encrypted, cryptKey, NewAuthKey());
            result.Should().BeNull();
        }

        [TestMethod]
        [Owner("Copilot")]
        [TestCategory("ServerPermissionsSecurity")]
        public void SimpleDecrypt_MessageTooShort_ReturnsNull()
        {
            var cryptKey = NewCryptKey();
            var authKey  = NewAuthKey();
            // A payload shorter than HMAC tag + IV will fail the length check
            var tinyPayload = Convert.ToBase64String(new byte[10]);
            var result = AESThenHMAC.SimpleDecrypt(tinyPayload, cryptKey, authKey);
            result.Should().BeNull();
        }

        // ── SimpleEncrypt / SimpleDecrypt with NonSecretPayload ───────────────

        [TestMethod]
        [Owner("Copilot")]
        [TestCategory("ServerPermissionsSecurity")]
        public void SimpleEncryptDecrypt_WithNonSecretPayload_RoundTrip()
        {
            var cryptKey       = NewCryptKey();
            var authKey        = NewAuthKey();
            var nonSecret      = Encoding.UTF8.GetBytes("header");
            var nonSecretLen   = nonSecret.Length;

            var encrypted = AESThenHMAC.SimpleEncrypt(_message, cryptKey, authKey, nonSecret);
            var decrypted = AESThenHMAC.SimpleDecrypt(encrypted, cryptKey, authKey, nonSecretLen);

            decrypted.Should().Be(_message);
        }

        // ── SimpleEncryptWithPassword / SimpleDecryptWithPassword ─────────────

        [TestMethod]
        [Owner("Copilot")]
        [TestCategory("ServerPermissionsSecurity")]
        public void SimpleEncryptDecryptWithPassword_RoundTrip_ReturnsOriginalMessage()
        {
            var encrypted = AESThenHMAC.SimpleEncryptWithPassword(_message, _password);
            var decrypted = AESThenHMAC.SimpleDecryptWithPassword(encrypted, _password);

            decrypted.Should().Be(_message);
        }

        [TestMethod]
        [Owner("Copilot")]
        [TestCategory("ServerPermissionsSecurity")]
        public void SimpleEncryptWithPassword_EmptyMessage_ThrowsArgumentException()
        {
            Action act = () => AESThenHMAC.SimpleEncryptWithPassword("", _password);
            act.Should().Throw<ArgumentException>().WithParameterName("secretMessage");
        }

        [TestMethod]
        [Owner("Copilot")]
        [TestCategory("ServerPermissionsSecurity")]
        public void SimpleEncryptWithPassword_NullMessage_ThrowsArgumentException()
        {
            Action act = () => AESThenHMAC.SimpleEncryptWithPassword((string)null, _password);
            act.Should().Throw<ArgumentException>().WithParameterName("secretMessage");
        }

        [TestMethod]
        [Owner("Copilot")]
        [TestCategory("ServerPermissionsSecurity")]
        public void SimpleEncryptWithPassword_PasswordTooShort_ThrowsArgumentException()
        {
            Action act = () => AESThenHMAC.SimpleEncryptWithPassword(_message, "short");
            act.Should().Throw<ArgumentException>().WithParameterName("password");
        }

        [TestMethod]
        [Owner("Copilot")]
        [TestCategory("ServerPermissionsSecurity")]
        public void SimpleEncryptWithPassword_NullPassword_ThrowsArgumentException()
        {
            Action act = () => AESThenHMAC.SimpleEncryptWithPassword(_message, null);
            act.Should().Throw<ArgumentException>().WithParameterName("password");
        }

        [TestMethod]
        [Owner("Copilot")]
        [TestCategory("ServerPermissionsSecurity")]
        public void SimpleDecryptWithPassword_EmptyMessage_ThrowsArgumentException()
        {
            Action act = () => AESThenHMAC.SimpleDecryptWithPassword("", _password);
            act.Should().Throw<ArgumentException>().WithParameterName("encryptedMessage");
        }

        [TestMethod]
        [Owner("Copilot")]
        [TestCategory("ServerPermissionsSecurity")]
        public void SimpleDecryptWithPassword_PasswordTooShort_ThrowsArgumentException()
        {
            var encrypted = AESThenHMAC.SimpleEncryptWithPassword(_message, _password);
            Action act = () => AESThenHMAC.SimpleDecryptWithPassword(encrypted, "short");
            act.Should().Throw<ArgumentException>().WithParameterName("password");
        }

        [TestMethod]
        [Owner("Copilot")]
        [TestCategory("ServerPermissionsSecurity")]
        public void SimpleDecryptWithPassword_WrongPassword_ReturnsNull()
        {
            var encrypted = AESThenHMAC.SimpleEncryptWithPassword(_message, _password);
            var result    = AESThenHMAC.SimpleDecryptWithPassword(encrypted, "wrong-password-12345");

            result.Should().BeNull();
        }

        [TestMethod]
        [Owner("Copilot")]
        [TestCategory("ServerPermissionsSecurity")]
        public void SimpleEncryptWithPassword_ProducesDifferentCiphertextEachCall_DueToRandomSalt()
        {
            var enc1 = AESThenHMAC.SimpleEncryptWithPassword(_message, _password);
            var enc2 = AESThenHMAC.SimpleEncryptWithPassword(_message, _password);

            enc1.Should().NotBe(enc2);
        }
    }
}
