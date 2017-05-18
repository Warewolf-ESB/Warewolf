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
using System.Collections.Generic;
using System.Text;
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces.Enums;
using Dev2.Communication;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

// ReSharper disable InconsistentNaming
namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    public class DeleteAllTestsTest
    {
        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GetAuthorizationContextForService_Returns_Contribute()
        {
            //------------Setup for test-------------------------
            DeleteAllTests deleteAllTests = new DeleteAllTests();
            //------------Execute Test---------------------------
            var authorizationContextForService = deleteAllTests.GetAuthorizationContextForService();
            //------------Assert Results-------------------------
            Assert.AreEqual(AuthorizationContext.Contribute, authorizationContextForService);
        }
        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void CreateServiceEntry_Returns_New_Dynamic_Service_DeleteAllTestsService()
        {
            //------------Setup for test-------------------------
            DeleteAllTests deleteAllTests = new DeleteAllTests();
            //------------Execute Test---------------------------
            var dynamicService = deleteAllTests.CreateServiceEntry();
            var handleType = deleteAllTests.HandlesType();
            //------------Assert Results-------------------------
            Assert.IsNotNull(dynamicService);
            Assert.IsFalse(string.IsNullOrEmpty(handleType));
            Assert.AreEqual(handleType, dynamicService.Name);
        }
        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GetResourceID_GivenEmptyArgs_Returns_EmptyGuid()
        {
            //------------Setup for test-------------------------
            DeleteAllTests deleteAllTests = new DeleteAllTests();
            //------------Execute Test---------------------------
            var requestArgs = new Dictionary<string, StringBuilder>();
            var resourceID = deleteAllTests.GetResourceID(requestArgs);
            //------------Assert Results-------------------------
            Assert.IsNotNull(resourceID);
            Assert.AreEqual(Guid.Empty, resourceID);
        }
        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GetResourceID_GivenSomeArgs_Returns_Id()
        {
            //------------Setup for test-------------------------
            DeleteAllTests deleteAllTests = new DeleteAllTests();
            var resId = Guid.NewGuid();
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(resId);
            //------------Execute Test---------------------------
            var requestArgs = new Dictionary<string, StringBuilder>
            {
                { "resourceID", stringBuilder }
            };
            var resourceID = deleteAllTests.GetResourceID(requestArgs);
            //------------Assert Results-------------------------
            Assert.IsNotNull(resourceID);
            Assert.AreEqual(resId, resourceID);
        }
        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Execute_GivenNoArgs_Exception()
        {
            //------------Setup for test-------------------------
            DeleteAllTests deleteAllTests = new DeleteAllTests();
            var workspaceMock = new Mock<IWorkspace>();
            //------------Execute Test---------------------------
            var requestArgs = new Dictionary<string, StringBuilder>();
            var executeResults = deleteAllTests.Execute(requestArgs, workspaceMock.Object);
            Dev2JsonSerializer jsonSerializer = new Dev2JsonSerializer();
            Assert.IsNotNull(executeResults);
            var deserializedResults = jsonSerializer.Deserialize<CompressedExecuteMessage>(executeResults);
            //------------Assert Results-------------------------
            Assert.IsNotNull(deserializedResults);
            Assert.IsTrue(deserializedResults.HasError);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Execute_GivenSomeArgs_Returns_Id()
        {
            //------------Setup for test-------------------------
            DeleteAllTests deleteAllTests = new DeleteAllTests();
            var workspaceMock = new Mock<IWorkspace>();
            //------------Execute Test---------------------------
            var value = new List<string> { "Test1", "Test2" }.SerializeToJsonStringBuilder();
            var requestArgs = new Dictionary<string, StringBuilder>
            {
                { "excludeList", value }
            };
            var executeResults = deleteAllTests.Execute(requestArgs, workspaceMock.Object);
            Dev2JsonSerializer jsonSerializer = new Dev2JsonSerializer();
            Assert.IsNotNull(executeResults);
            var deserializedResults = jsonSerializer.Deserialize<CompressedExecuteMessage>(executeResults);
            //------------Assert Results-------------------------
            Assert.IsNotNull(deserializedResults);
            Assert.IsFalse(deserializedResults.HasError);
            var message = CompressedExecuteMessage.Decompress(deserializedResults.Message.ToString());
            Assert.AreEqual("Test reload succesful", message);
        }
    }
}
