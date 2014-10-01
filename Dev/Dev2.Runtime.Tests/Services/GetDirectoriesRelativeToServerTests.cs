
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Text;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Data.ServiceModel;
using Dev2.DynamicServices;
using Dev2.Explorer;
using Dev2.Interfaces;
using Dev2.Runtime.ESB.Management.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

// ReSharper disable InconsistentNaming
namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class GetDirectoriesRelativeToServerTests
    {

        #region Execute

        [TestMethod]
        [Owner("Huggs")]
        [ExpectedException(typeof(InvalidDataContractException))]
        public void GetDirectoriesRelativeToServer_UnitTest_ExecuteWithNullValues_ExpectedInvalidDataContractException()
        {
            var esb = new GetDirectoriesRelativeToServer();
            var actual = esb.Execute(null, null);
            Assert.AreEqual(string.Empty, actual);
        }

        [TestMethod]
        [Owner("Huggs")]
        [ExpectedException(typeof(InvalidDataContractException))]
        public void GetDirectoriesRelativeToServer_UnitTest_ExecuteWithNoDirectoryInValues_ExpectedInvalidDataContractException()
        {

            var esb = new GetDirectoriesRelativeToServer();
            var actual = esb.Execute(new Dictionary<string, StringBuilder> { { "DebugFilePath", null } }, null);
            Assert.AreEqual(string.Empty, actual);
        }

        [TestMethod]
        [Owner("Huggs")]
        [ExpectedException(typeof(InvalidDataContractException))]
        public void GetDirectoriesRelativeToServer_UnitTest_ExecuteWithNullDirectory_ExpectedInvalidDataContractException()
        {

            var esb = new RenameResourceCategory();
            var actual = esb.Execute(new Dictionary<string, StringBuilder> { { "Directory", null } }, null);
            Assert.AreEqual(string.Empty, actual);
        }

        [TestMethod]
        [Owner("Huggs")]
        [ExpectedException(typeof(InvalidDataContractException))]
        public void GetDirectoriesRelativeToServer_UnitTest_ExecuteWithBlankDirectory_ExpectInvalidDataContractException()
        {

            var esb = new GetDirectoriesRelativeToServer();
            var actual = esb.Execute(new Dictionary<string, StringBuilder> { { "Directory", new StringBuilder() } }, null);
            Assert.AreEqual(string.Empty, actual);
        }

        [TestMethod]
        [Owner("Huggs")]
        public void GetDirectoriesRelativeToServer_UnitTest_ExecuteWithDirectory_ExpectDirectoryStructure()
        {
            //----------------Setup---------------------------------------------
            var esb = new GetDirectoriesRelativeToServer();
            Mock<IExplorerServerResourceRepository> mockRepo = new Mock<IExplorerServerResourceRepository>();
            ServerExplorerItem serverExplorerItem = new ServerExplorerItem();
            serverExplorerItem.ResourceType = ResourceType.Server;
            ServerExplorerItem levelOneFolder = new ServerExplorerItem();
            levelOneFolder.ResourceType = ResourceType.Folder;
            levelOneFolder.DisplayName = "Test1";
            levelOneFolder.ResourcePath = "Test1";
            serverExplorerItem.Children.Add(levelOneFolder);
            IExplorerItem levelOneFolderTwo = new ServerExplorerItem();
            levelOneFolderTwo.ResourceType = ResourceType.Folder;
            levelOneFolderTwo.DisplayName = "Test2";
            levelOneFolderTwo.ResourcePath = "Test2";
            ServerExplorerItem levelTwoFolderInFolderTwo = new ServerExplorerItem();
            levelTwoFolderInFolderTwo.ResourceType = ResourceType.Folder;
            levelTwoFolderInFolderTwo.DisplayName = "InnerTest2";
            levelTwoFolderInFolderTwo.ResourcePath = levelOneFolderTwo.ResourcePath + "\\InnerTest2";
            levelOneFolderTwo.Children.Add(levelTwoFolderInFolderTwo);
            serverExplorerItem.Children.Add(levelOneFolderTwo);
            mockRepo.Setup(repository => repository.Load(ResourceType.Folder, It.IsAny<string>())).Returns(serverExplorerItem);
            esb.ServerExplorerRepo = mockRepo.Object;
            //----------------Execute------------------------------------------------
            var actual = esb.Execute(new Dictionary<string, StringBuilder> { { "Directory", new StringBuilder("Resources") } }, null);
            //----------------Assert Results-----------------------------------------
            Assert.AreNotEqual(string.Empty, actual);
            const string expected = @"<JSON>{
  ""$type"": ""Dev2.Runtime.ESB.Management.Services.JsonTreeNode, Dev2.Runtime.Services"",
  ""title"": ""Root"",
  ""isFolder"": true,
  ""key"": ""root"",
  ""isLazy"": false,
  ""children"": [
    {
      ""$type"": ""Dev2.Runtime.ESB.Management.Services.JsonTreeNode, Dev2.Runtime.Services"",
      ""title"": ""Test1"",
      ""isFolder"": true,
      ""key"": ""Test1"",
      ""isLazy"": false,
      ""children"": []
    },
    {
      ""$type"": ""Dev2.Runtime.ESB.Management.Services.JsonTreeNode, Dev2.Runtime.Services"",
      ""title"": ""Test2"",
      ""isFolder"": true,
      ""key"": ""Test2"",
      ""isLazy"": false,
      ""children"": [
        {
          ""$type"": ""Dev2.Runtime.ESB.Management.Services.JsonTreeNode, Dev2.Runtime.Services"",
          ""title"": ""InnerTest2"",
          ""isFolder"": true,
          ""key"": ""Test2\\\\InnerTest2"",
          ""isLazy"": false,
          ""children"": []
        }
      ]
    }
  ]
}</JSON>
";
            var actuals = actual.ToString().Trim();
            Assert.AreEqual(expected.Trim(), actuals);
        }

        #endregion

        #region HandlesType

        [TestMethod]
        [Owner("Huggs")]
        public void GetDirectoriesRelativeToServer_UnitTest_HandlesType_ExpectedGetDirectoriesRelativeToServerService()
        {
            var esb = new GetDirectoriesRelativeToServer();
            var result = esb.HandlesType();
            Assert.AreEqual("GetDirectoriesRelativeToServerService", result);
        }

        #endregion

        #region CreateServiceEntry

        [TestMethod]
        [Owner("Huggs")]
        public void GetDirectoriesRelativeToServer_UnitTest_CreateServiceEntry_ExpectedReturnsDynamicService()
        {
            var esb = new GetDirectoriesRelativeToServer();
            var result = esb.CreateServiceEntry();
            Assert.AreEqual(esb.HandlesType(), result.Name);
            Assert.AreEqual("<DataList><Directory ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>", result.DataListSpecification);
            Assert.AreEqual(1, result.Actions.Count);

            var serviceAction = result.Actions[0];
            Assert.AreEqual(esb.HandlesType(), serviceAction.Name);
            Assert.AreEqual(enActionType.InvokeManagementDynamicService, serviceAction.ActionType);
            Assert.AreEqual(esb.HandlesType(), serviceAction.SourceMethod);
        }

        #endregion
    }
}
