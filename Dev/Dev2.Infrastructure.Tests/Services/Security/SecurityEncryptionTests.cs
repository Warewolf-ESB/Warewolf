#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2025 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Dev2.Services.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Infrastructure.Tests.Services.Security
{
    [TestClass]
    public class SecurityEncryptionTests
    {
        [TestMethod]
        [Owner("Frontend Team")]
        [TestCategory(nameof(SecurityEncryption))]
        public void SecurityEncryption_Decrypt_WithTestValue_ShouldReturnCorrectPlainText()
        {
            // Arrange - Test values provided by backend team
            const string encrypted = "t8fHrQDdgI6BNzmH1X4wTAptT9zCcJAVxe/n4z2ej/Y+PR+zkqHe/mQuYBqMUKuA";
            const string expectedPlain = "Server=localhost;Database=TestDB;";

            // Act
            var decrypted = SecurityEncryption.Decrypt(encrypted);

            // Assert
            Assert.AreEqual(expectedPlain, decrypted, "Decrypted value should match the expected plain text");
        }

        [TestMethod]
        [Owner("Frontend Team")]
        [TestCategory(nameof(SecurityEncryption))]
        public void SecurityEncryption_TryDecrypt_WithTestValue_ShouldReturnCorrectPlainText()
        {
            // Arrange - Test values provided by backend team
            const string encrypted = "t8fHrQDdgI6BNzmH1X4wTAptT9zCcJAVxe/n4z2ej/Y+PR+zkqHe/mQuYBqMUKuA";
            const string expectedPlain = "Server=localhost;Database=TestDB;";

            // Act
            var decrypted = SecurityEncryption.TryDecrypt(encrypted);

            // Assert
            Assert.AreEqual(expectedPlain, decrypted, "Decrypted value should match the expected plain text");
        }

        [TestMethod]
        [Owner("Frontend Team")]
        [TestCategory(nameof(SecurityEncryption))]
        public void SecurityEncryption_EncryptAndDecrypt_RoundTrip_ShouldReturnOriginalValue()
        {
            // Arrange
            const string original = "Server=myserver;Database=mydb;User=admin;Password=secret123;";

            // Act
            var encrypted = SecurityEncryption.Encrypt(original);
            var decrypted = SecurityEncryption.Decrypt(encrypted);

            // Assert
            Assert.AreEqual(original, decrypted, "Round-trip encryption/decryption should return original value");
            Assert.AreNotEqual(original, encrypted, "Encrypted value should differ from original");
        }

        [TestMethod]
        [Owner("Frontend Team")]
        [TestCategory(nameof(SecurityEncryption))]
        public void SecurityEncryption_Decrypt_RemovesTrailingNullBytes()
        {
            // Arrange
            const string plainText = "Short";

            // Act
            var encrypted = SecurityEncryption.Encrypt(plainText);
            var decrypted = SecurityEncryption.Decrypt(encrypted);

            // Assert
            Assert.AreEqual(plainText, decrypted, "Decrypted value should not contain trailing null bytes");
            Assert.IsFalse(decrypted.Contains("\0"), "Decrypted value should not contain null characters");
        }

        [TestMethod]
        [Owner("Frontend Team")]
        [TestCategory(nameof(SecurityEncryption))]
        public void SecurityEncryption_TryDecrypt_WithInvalidBase64_ReturnsOriginalValue()
        {
            // Arrange
            const string invalidValue = "This is not base64!@#$%";

            // Act
            var result = SecurityEncryption.TryDecrypt(invalidValue);

            // Assert
            Assert.AreEqual(invalidValue, result, "TryDecrypt should return original value when decryption fails");
        }

        [TestMethod]
        [Owner("Frontend Team")]
        [TestCategory(nameof(SecurityEncryption))]
        public void SecurityEncryption_TryDecrypt_WithPlainText_ReturnsOriginalValue()
        {
            // Arrange
            const string plainText = "Server=localhost;Database=TestDB;";

            // Act
            var result = SecurityEncryption.TryDecrypt(plainText);

            // Assert
            Assert.AreEqual(plainText, result, "TryDecrypt should return original value when input is not encrypted");
        }

        [TestMethod]
        [Owner("Frontend Team")]
        [TestCategory(nameof(SecurityEncryption))]
        public void SecurityEncryption_Decrypt_WithConnectionStrings_AllDatabaseTypes()
        {
            // Arrange - Test various database connection string formats
            var testStrings = new[]
            {
                "Data Source=sqlserver,1433;Initial Catalog=TestDB;User ID=sa;Password=Pass123;Connection Timeout=30",
                "Server=mysqlserver;Port=3306;Database=TestDB;Uid=root;Pwd=Pass123;Connect Timeout=30;SslMode=none;",
                "User Id=admin;Password=Pass123;Data Source=oracleserver:1521;Database=TestDB;Connection Timeout=30;",
                "Host=pgserver;Port=5432;Username=postgres;Password=Pass123;Database=TestDB;Timeout=30",
                "DSN=MyODBCDataSource;"
            };

            foreach (var connectionString in testStrings)
            {
                // Act
                var encrypted = SecurityEncryption.Encrypt(connectionString);
                var decrypted = SecurityEncryption.Decrypt(encrypted);

                // Assert
                Assert.AreEqual(connectionString, decrypted, $"Round-trip failed for: {connectionString}");
            }
        }
    }
}
