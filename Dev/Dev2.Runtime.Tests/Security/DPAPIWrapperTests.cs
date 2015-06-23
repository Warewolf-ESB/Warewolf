
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
using System.Threading;
using System.Drawing;
using System.IO;
using Dev2.Common.Interfaces.Security;
using Dev2.Communication;
using Dev2.Runtime.Services.Security.Encryption;
using Dev2.Services.Security;
using Microsoft.AspNet.SignalR.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using FluentAssertions;
using Dev2.Runtime.Services.Security;

namespace Dev2.Tests.Runtime.Security
{
    [TestClass]
    public class DPAPIWrapperTests
    {
        // ReSharper disable InconsistentNaming

        private string message = "This is the secret message to encrypt.";

        [TestMethod]
        [Owner("Kerneels Roos")]
        [TestCategory("EncryptDecrypt")]
        public void EncryptDecryptTest()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            string encrypted = DPAPIWrapper.Encrypt(message);
            encrypted.Should().NotBeNullOrEmpty();
            encrypted.Should().NotBeNullOrWhiteSpace();
            encrypted.Should().NotBeSameAs(message);
            encrypted.Should().NotContain(message);
            DPAPIWrapper.Decrypt(encrypted).Should().Be(message);
        }


        // ReSharper restore InconsistentNaming
    }
}
