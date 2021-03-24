﻿#pragma warning disable S1481, S101, CC0091, S1226, S100, CC0044, CC0045, CC0021, CC0022, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001, IDE0019, CC0105, RECS008, CA2202, RECS005, IDE0016
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Linq;
using Dev2.Services.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;




namespace Warewolf.Security.Encryption
{
    [TestClass]
    public class DPAPIWrapperTests
    {

        readonly string message = "This is the secret message to encrypt.";

        [TestMethod]
        [Owner("Kerneels Roos")]
        [TestCategory("ServerPermissionsSecurity")]
        public void EncryptDecryptTest()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var encrypted = SecurityEncryption.Encrypt(message);
            encrypted.Should().NotBeNullOrEmpty();
            encrypted.Should().NotBeNullOrWhiteSpace();
            encrypted.Should().NotBeSameAs(message);
            encrypted.Should().NotContain(message);
            SecurityEncryption.Decrypt(encrypted).Should().Be(message);
        }

        [TestMethod]
        [Owner("Kerneels Roos")]
        [TestCategory("ServerPermissionsSecurity")]
        public void EncryptDecryptFailsIfAlreadyPerformedTest()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var encrypted = SecurityEncryption.Encrypt(message);
            var x = encrypted.Where(o => encrypted.Where(u => u == o).Count() > 1).First();  // find first char that appears more than once
            var y = encrypted.Where(o => o != x).First();  // find the first char not equal to x
            var tamperedEncrypted = encrypted.Replace(x, y);
            try
            {
                var decrypted = SecurityEncryption.Decrypt(tamperedEncrypted);
            }
            catch (Exception e)
            {
                e.GetType().Should().Be(typeof(System.Security.Cryptography.CryptographicException));
            }
            try
            {
                SecurityEncryption.Decrypt(message);
            }
            catch (Exception e)
            {
                e.GetType().Should().Be(typeof(ArgumentException));
            }
        }


    }
}
