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
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    public class FetchRemoteDebugMessagesTests
    {
        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(FetchRemoteDebugMessages))]
        public void FetchRemoteDebugMessages_HandlesType_ReturnsServiceName()
        {
            Assert.AreEqual("FetchRemoteDebugMessagesService", new FetchRemoteDebugMessages().HandlesType());
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(FetchRemoteDebugMessages))]
        public void FetchRemoteDebugMessages_CreateServiceEntry_NamedAfterHandlesType()
        {
            var service = new FetchRemoteDebugMessages();

            var entry = service.CreateServiceEntry();

            Assert.AreEqual(service.HandlesType(), entry.Name);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(FetchRemoteDebugMessages))]
        [ExpectedException(typeof(InvalidDataContractException))]
        public void FetchRemoteDebugMessages_Execute_MissingInvokerID_Throws()
        {
            var service = new FetchRemoteDebugMessages();

            service.Execute(new Dictionary<string, StringBuilder>(), new Mock<IWorkspace>().Object);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(FetchRemoteDebugMessages))]
        public void FetchRemoteDebugMessages_Execute_EmptyGuidInvokerID_ReturnsEmptyStringBuilder()
        {
            var service = new FetchRemoteDebugMessages();
            var values = new Dictionary<string, StringBuilder>
            {
                { "InvokerID", new StringBuilder(Guid.Empty.ToString()) }
            };

            var result = service.Execute(values, new Mock<IWorkspace>().Object);

            Assert.AreEqual(string.Empty, result.ToString());
        }
    }
}
