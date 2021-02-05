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
using System.Collections.Generic;
using System.Text;
using Dev2.Common.Interfaces.Enums;
using Dev2.Runtime.ESB.Management.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Dev2.Communication;
using Dev2.Common;
using Dev2.Runtime.Interfaces;
using Dev2.Workspaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.DB;
using Warewolf.Core;

namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    public class SaveWebServiceTests
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SaveWebService))]
        public void SaveWebService_GetResourceID_ShouldReturnEmptyGuid()
        {
            //------------Setup for test--------------------------
            var saveWebService = new SaveWebService();
            //------------Execute Test---------------------------
            var resId = saveWebService.GetResourceID(new Dictionary<string, StringBuilder>());
            //------------Assert Results-------------------------
            Assert.AreEqual(Guid.Empty, resId);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SaveWebService))]
        public void SaveWebService_GetAuthorizationContextForService_ShouldReturnContext()
        {
            //------------Setup for test--------------------------
            var saveWebService = new SaveWebService();
            //------------Execute Test---------------------------
            var resId = saveWebService.GetAuthorizationContextForService();
            //------------Assert Results-------------------------
            Assert.AreEqual(AuthorizationContext.Contribute, resId);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SaveWebService))]
        public void SaveWebService_SavePluginService_HandlesType_ExpectName()
        {
            //------------Setup for test--------------------------
            var saveWebService = new SaveWebService();
            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.AreEqual("SaveWebService", saveWebService.HandlesType());
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SaveWebService))]
        public void SaveWebService_Execute_NullValues_ErrorResult()
        {
            //------------Setup for test--------------------------
            var saveWebService = new SaveWebService();
            var serializer = new Dev2JsonSerializer();
            //------------Execute Test---------------------------
            var jsonResult = saveWebService.Execute(null, null);
            var result = serializer.Deserialize<ExecuteMessage>(jsonResult);
            //------------Assert Results-------------------------
            Assert.IsTrue(result.HasError);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SaveWebService))]
        public void SaveWebService_Execute_ResourceIDNotPresent_ErrorResult()
        {
            //------------Setup for test--------------------------
            var values = new Dictionary<string, StringBuilder> {{"item", new StringBuilder()}};
            var saveWebService = new SaveWebService();
            var serializer = new Dev2JsonSerializer();
            //------------Execute Test---------------------------
            var jsonResult = saveWebService.Execute(values, null);
            var result = serializer.Deserialize<ExecuteMessage>(jsonResult);
            //------------Assert Results-------------------------
            Assert.IsTrue(result.HasError);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SaveWebService))]
        public void SaveWebService_Execute_ResourceIDNotGuid_ErrorResult()
        {
            //------------Setup for test--------------------------
            var values = new Dictionary<string, StringBuilder> {{"resourceID", new StringBuilder("ABCDE")}};
            var saveWebService = new SaveWebService();
            var serializer = new Dev2JsonSerializer();
            //------------Execute Test---------------------------
            var jsonResult = saveWebService.Execute(values, null);
            var result = serializer.Deserialize<ExecuteMessage>(jsonResult);
            //------------Assert Results-------------------------
            Assert.IsTrue(result.HasError);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SaveWebService))]
        public void SaveWebService_Execute_TestDefinitionsNotInValues_ErrorResult()
        {
            //------------Setup for test--------------------------
            var values = new Dictionary<string, StringBuilder>
                {{"resourceID", new StringBuilder(Guid.NewGuid().ToString())}};
            var saveWebService = new SaveWebService();
            var serializer = new Dev2JsonSerializer();
            //------------Execute Test---------------------------
            var jsonResult = saveWebService.Execute(values, null);
            var result = serializer.Deserialize<ExecuteMessage>(jsonResult);
            //------------Assert Results-------------------------
            Assert.IsTrue(result.HasError);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SaveWebService))]
        public void SaveWebService_Execute_ItemToDeleteNotListOfServiceTestTO_ErrorResult()
        {
            //------------Setup for test--------------------------
            var values = new Dictionary<string, StringBuilder>
            {
                {"resourceID", new StringBuilder(Guid.NewGuid().ToString())},
                {"testDefinitions", new StringBuilder("This is not deserializable to ServerExplorerItem")}
            };
            var saveWebService = new SaveWebService();
            var serializer = new Dev2JsonSerializer();
            //------------Execute Test---------------------------
            var jsonResult = saveWebService.Execute(values, null);
            var result = serializer.Deserialize<ExecuteMessage>(jsonResult);
            //------------Assert Results-------------------------
            Assert.IsTrue(result.HasError);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SaveWebService))]
        public void SaveWebService_Execute_GivenNullSource_ShouldReturnNullSourceMsg()
        {
            //------------Setup for test--------------------------
            var serializer = new Dev2JsonSerializer();
            var inputs = new Dictionary<string, StringBuilder>();
            var resourceId = Guid.NewGuid();
            var saveWebService = new SaveWebService();

            var resourceCatalog = new Mock<IResourceCatalog>();
            resourceCatalog.Setup(catalog => catalog.GetResource(GlobalConstants.ServerWorkspaceID, resourceId))
                .Returns(default(Dev2.Common.Interfaces.Data.IResource));
            var ws = new Mock<IWorkspace>();

            var serviceDef = new WebServiceDefinition
            {
                Source = new WebServiceSourceDefinition(),
                OutputMappings = new List<IServiceOutputMapping>(),
            };
            inputs.Add("Webservice", serializer.SerializeToBuilder(serviceDef));
            //------------Execute Test---------------------------
            var stringBuilder = saveWebService.Execute(inputs, ws.Object);
            //------------Assert Results-------------------------
            var msg = serializer.Deserialize<ExecuteMessage>(stringBuilder);
            Assert.AreEqual("Value cannot be null.\r\nParameter name: source", msg.Message.ToString());
        }
    }
}