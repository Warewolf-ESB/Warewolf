/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Security.Encryption;

namespace Warewolf.Security.Tests
{
    [TestClass]
    public class SecurityEncryptionTests
    {
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(SecurityEncryption))]
        public void SecurityEncryption_Encrypt_Success()
        {
            var user = SecurityEncryption.Encrypt("currentPrincipal.Identity.Name");
            Assert.IsTrue(user.IsBase64());
            Assert.IsTrue(user.CanBeDecrypted());
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(SecurityEncryption))]
        public void SecurityEncryption_EncryptIfDecrypted_Success()
        {
            var user = SecurityEncryption.EncryptIfDecrypted("currentPrincipal.Identity.Name");
            Assert.IsTrue(user.IsBase64());
            Assert.IsTrue(user.CanBeDecrypted());
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(SecurityEncryption))]
        public void SecurityEncryption_DecryptIfEncrypted_Success()
        {
            var encryptUser = SecurityEncryption.Encrypt("currentPrincipal.Identity.Name");
            var decryptuser = SecurityEncryption.DecryptIfEncrypted(encryptUser);
            Assert.AreEqual("currentPrincipal.Identity.Name", decryptuser);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(SecurityEncryption))]
        public void SecurityEncryption_Decrypt_Success()
        {
            var encryptUser = SecurityEncryption.Encrypt("currentPrincipal.Identity.Name");
            var decryptuser = SecurityEncryption.Decrypt(encryptUser);
            Assert.AreEqual("currentPrincipal.Identity.Name", decryptuser);
        }
    }
}