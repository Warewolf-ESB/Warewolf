
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections.Generic;
using System.IO;
using Dev2.Warewolf.Security.Encryption;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;

namespace Dev2.Warewolf.Security.Tests.Encryption
{
    [TestClass]
    public class AESThenHMACTests
    {
        // ReSharper disable InconsistentNaming

        private string message = "This is the secret message to encrypt.",
            //key = "TheSecretKeyToUse",
            //path = @"Security\Data\logo.bmp",
            password = "TheSecretPasswordToUse";

        [TestMethod]
        [Owner("Kerneels Roos")]
        [TestCategory("EncryptDecrypt")]
        public void EncryptDecryptTest()
        {
            //------------Setup for test--------------------------
            SetupAESThenHMAC();
            //------------Execute Test---------------------------
            string encrypted = AESThenHMAC.SimpleEncryptWithPassword(message, password);
            encrypted.Should().NotBeNullOrEmpty();
            encrypted.Should().NotBeSameAs(message);
            encrypted.Should().NotBeNullOrWhiteSpace();
            encrypted.Should().NotContain(message);
            encrypted.Should().NotContain(password);
            AESThenHMAC.SimpleDecryptWithPassword(encrypted, password).Should().Be(message);
        }

        /*
        [TestMethod]
        [Owner("Kerneels Roos")]
        [TestCategory("EncryptDecryptSteganography")]
        public void EncryptDecryptSteganographyTest()
        {
            //------------Setup for test--------------------------
            SetupAESThenHMAC();
            //------------Execute Test---------------------------
            string encrypted = AESThenHMAC.SimpleEncryptWithPassword(message, password);
            File.Copy(path,path + ".test.bmp");
            Steganography.HideMessage(encrypted,new imagebi,keyStream);
            AESThenHMAC.SimpleDecryptWithPassword(encrypted, password).Should().Be(message);
        }
        */

        private void SetupAESThenHMAC()
        {
            //AESThenHMAC.
        }

        // ReSharper restore InconsistentNaming
    }
}
