/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using Warewolf.Security.Encryption;

// ReSharper disable InconsistentNaming

// ReSharper disable once CheckNamespace
namespace Dev2.Warewolf.Security.Encryption
{
    [TestClass]
    public class DPAPIWrapperTests
    {

        private readonly string message = "This is the secret message to encrypt.";

        [TestMethod]
        [Owner("Kerneels Roos")]
        [TestCategory("ServerPermissionsSecurity")]
        public void EncryptDecryptTest()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            string encrypted = DpapiWrapper.Encrypt(message);
            encrypted.Should().NotBeNullOrEmpty();
            encrypted.Should().NotBeNullOrWhiteSpace();
            encrypted.Should().NotBeSameAs(message);
            encrypted.Should().NotContain(message);
            DpapiWrapper.Decrypt(encrypted).Should().Be(message);
        }

        [TestMethod]
        [Owner("Kerneels Roos")]
        [TestCategory("ServerPermissionsSecurity")]
        public void EncryptDecryptFailsIfAlreadyPerformedTest()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            string encrypted = DpapiWrapper.Encrypt(message);
            char x = encrypted.Where(o => encrypted.Where(u => u == o).Count() > 1).First();  // find first char that appears more than once
            char y = encrypted.Where(o => o != x).First();  // find the first char not equal to x
            string tamperedEncrypted = encrypted.Replace(x, y);
            try
            {
                string decrypted = DpapiWrapper.Decrypt(tamperedEncrypted);
            }
            catch (Exception e)
            {
                e.GetType().Should().Be(typeof(System.Security.Cryptography.CryptographicException));
            }
            try
            {
                DpapiWrapper.Decrypt(message);
            }
            catch (Exception e)
            {
                e.GetType().Should().Be(typeof(ArgumentException));
            }
        }

        // ReSharper restore InconsistentNaming
    }
}
