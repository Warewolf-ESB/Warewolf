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
using System.IO;
using System.Text;
using Dev2.Communication;
using Dev2.Studio.Interfaces;
using Dev2.Studio.Interfaces.Enums;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;

namespace Dev2.Core.Tests.Workspaces
{
    [TestClass]
    [TestCategory("Studio Workspaces Core")]
    public class WorkspaceItemRepositoryTests
    {
        #region Static Class Init

        static string _testDir;

        [ClassInitialize]
        public static void MyClassInit(TestContext context)
        {
            _testDir = context.DeploymentDirectory;
        }

        #endregion

        #region WorkspaceItems

        [TestMethod]
        public void WorkspaceItemRepositoryWorkspaceItemsExpectedInvokesReadFirstTime()
        {

            var mockConn = new Mock<IEnvironmentConnection>();
            mockConn.Setup(c => c.IsConnected).Returns(true);
            var msg = new ExecuteMessage { HasError = false };
            msg.SetMessage("Workspace item updated");

            var payload = JsonConvert.SerializeObject(msg);
            mockConn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>())).Returns(new StringBuilder(payload)).Verifiable();

            var model = CreateModel(ResourceType.Service, mockConn, out string resourceName, out Guid workspaceID, out Guid serverID);

            var repositoryPath = GetUniqueRepositoryPath();

            // Create repository file with one item in it
            var repository = new WorkspaceItemRepository(repositoryPath);
            repository.AddWorkspaceItem(model.Object);

            // Now create a new repository from the previous file
            repository = new WorkspaceItemRepository(repositoryPath);

            // Access items for the first time
            var items = repository.WorkspaceItems;

            Assert.AreEqual(1, items.Count);
            Assert.AreEqual(workspaceID, items[0].WorkspaceID);
            Assert.AreEqual(serverID, items[0].ServerID);
            Assert.AreEqual(resourceName, items[0].ServiceName);
            Assert.AreEqual(WorkspaceItem.ServiceServiceType, items[0].ServiceType);
        }

        [TestMethod]
        [Description("UpdateMode workspace item IsWorkflowSaved based on the resource")]
        [Owner("Huggs")]
        public void WorkspaceItemRepository_UnitTest_UpdateWorkspaceItemIsWorkflowSaved_ExpectSetsWorkspaceItemIsWorkflowSavedFalse()
        {


            var mockConn = new Mock<IEnvironmentConnection>();
            mockConn.Setup(c => c.IsConnected).Returns(true);
            var msg = new ExecuteMessage();
            msg.SetMessage("Workspace item updated");
            var payload = JsonConvert.SerializeObject(msg);
            mockConn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>())).Returns(new StringBuilder(payload)).Verifiable();

            var model = CreateModel(ResourceType.Service, mockConn, out string resourceName, out Guid workspaceID, out Guid serverID);

            model.Setup(resourceModel => resourceModel.IsWorkflowSaved).Returns(true);
            var repositoryPath = GetUniqueRepositoryPath();

            // Create repository file with one item in it
            var repository = new WorkspaceItemRepository(repositoryPath);
            repository.AddWorkspaceItem(model.Object);

            // Now create a new repository from the previous file
            repository = new WorkspaceItemRepository(repositoryPath);

            // Access items for the first time
            var items = repository.WorkspaceItems;

            Assert.AreEqual(1, items.Count);
            Assert.AreEqual(workspaceID, items[0].WorkspaceID);
            Assert.AreEqual(serverID, items[0].ServerID);
            Assert.AreEqual(resourceName, items[0].ServiceName);
            Assert.AreEqual(WorkspaceItem.ServiceServiceType, items[0].ServiceType);
            Assert.IsTrue(items[0].IsWorkflowSaved);

            model.Setup(resourceModel => resourceModel.IsWorkflowSaved).Returns(false);
            repository.UpdateWorkspaceItemIsWorkflowSaved(model.Object);
            Assert.IsFalse(items[0].IsWorkflowSaved);
        }

        #endregion

        #region AddWorkspaceItem

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WorkspaceItemRepositoryAddWorkspaceItemWithNullModelExpectedThrowsArgumentNullException()
        {
            var repository = new WorkspaceItemRepository(GetUniqueRepositoryPath());
            repository.AddWorkspaceItem(null);
        }

        [TestMethod]
        public void WorkspaceItemRepositoryAddWorkspaceItemWithExistingModelExpectedDoesNothing()
        {



            var mockConn = new Mock<IEnvironmentConnection>();
            mockConn.Setup(c => c.IsConnected).Returns(true);
            var msg = new ExecuteMessage();
            msg.SetMessage("Workspace item updated");
            var payload = JsonConvert.SerializeObject(msg);
            mockConn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>())).Returns(new StringBuilder(payload)).Verifiable();

            var model = CreateModel(ResourceType.Service, mockConn, out string resourceName, out Guid workspaceID, out Guid serverID);


            var repository = new WorkspaceItemRepository(GetUniqueRepositoryPath());
            repository.AddWorkspaceItem(model.Object);

            Assert.AreEqual(1, repository.WorkspaceItems.Count);

            repository.AddWorkspaceItem(model.Object);
            Assert.AreEqual(1, repository.WorkspaceItems.Count);
        }

        [TestMethod]
        public void WorkspaceItemRepositoryAddWorkspaceItemWithNewModelExpectedAddsAndAssignsWorkspaceID()
        {


            var mockConn = new Mock<IEnvironmentConnection>();
            mockConn.Setup(c => c.IsConnected).Returns(true);
            var msg = new ExecuteMessage();
            msg.SetMessage("Workspace item updated");
            var payload = JsonConvert.SerializeObject(msg);
            mockConn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>())).Returns(new StringBuilder(payload)).Verifiable();

            var model = CreateModel(ResourceType.Service, mockConn, out string resourceName, out Guid workspaceID, out Guid serverID);

            var repository = new WorkspaceItemRepository(GetUniqueRepositoryPath());
            repository.AddWorkspaceItem(model.Object);

            Assert.AreEqual(1, repository.WorkspaceItems.Count);
            Assert.AreEqual(workspaceID, repository.WorkspaceItems[0].WorkspaceID);
        }

        [TestMethod]
        public void WorkspaceItemRepositoryAddWorkspaceItemWithNewModelExpectedAddsAndAssignsServerID()
        {


            var mockConn = new Mock<IEnvironmentConnection>();
            mockConn.Setup(c => c.IsConnected).Returns(true);
            var msg = new ExecuteMessage();
            msg.SetMessage("Workspace item updated");
            var payload = JsonConvert.SerializeObject(msg);
            mockConn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>())).Returns(new StringBuilder(payload)).Verifiable();

            var model = CreateModel(ResourceType.Service, mockConn, out string resourceName, out Guid workspaceID, out Guid serverID);

            var repository = new WorkspaceItemRepository(GetUniqueRepositoryPath());
            repository.AddWorkspaceItem(model.Object);

            Assert.AreEqual(1, repository.WorkspaceItems.Count);
            Assert.AreEqual(serverID, repository.WorkspaceItems[0].ServerID);
        }

        [TestMethod]
        public void WorkspaceItemRepositoryAddWorkspaceItemWithNewModelExpectedAddsAndAssignsServiceName()
        {


            var mockConn = new Mock<IEnvironmentConnection>();
            mockConn.Setup(c => c.IsConnected).Returns(true);
            var msg = new ExecuteMessage();
            msg.SetMessage("Workspace item updated");
            var payload = JsonConvert.SerializeObject(msg);
            mockConn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>())).Returns(new StringBuilder(payload)).Verifiable();

            var model = CreateModel(ResourceType.Service, mockConn, out string resourceName, out Guid workspaceID, out Guid serverID);

            var repository = new WorkspaceItemRepository(GetUniqueRepositoryPath());
            repository.AddWorkspaceItem(model.Object);

            Assert.AreEqual(1, repository.WorkspaceItems.Count);
            Assert.AreEqual(resourceName, repository.WorkspaceItems[0].ServiceName);
        }

        [TestMethod]
        public void WorkspaceItemRepositoryAddWorkspaceItemWithNewServiceModelExpectedAddsAndAssignsServiceServiceType()
        {

            var mockConn = new Mock<IEnvironmentConnection>();
            mockConn.Setup(c => c.IsConnected).Returns(true);
            var msg = new ExecuteMessage();
            msg.SetMessage("Workspace item updated");
            var payload = JsonConvert.SerializeObject(msg);
            mockConn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>())).Returns(new StringBuilder(payload)).Verifiable();

            var model = CreateModel(ResourceType.Service, mockConn, out string resourceName, out Guid workspaceID, out Guid serverID);

            var repository = new WorkspaceItemRepository(GetUniqueRepositoryPath());
            repository.AddWorkspaceItem(model.Object);

            Assert.AreEqual(1, repository.WorkspaceItems.Count);
            Assert.AreEqual(WorkspaceItem.ServiceServiceType, repository.WorkspaceItems[0].ServiceType);
        }

        [TestMethod]
        public void WorkspaceItemRepositoryAddWorkspaceItemWithNewSourceModelExpectedAddsAndAssignsSourceServiceType()
        {

            var mockConn = new Mock<IEnvironmentConnection>();
            mockConn.Setup(c => c.IsConnected).Returns(true);
            var msg = new ExecuteMessage();
            msg.SetMessage("Workspace item updated");
            var payload = JsonConvert.SerializeObject(msg);
            mockConn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>())).Returns(new StringBuilder(payload)).Verifiable();

            var model = CreateModel(ResourceType.Source, mockConn, out string resourceName, out Guid workspaceID, out Guid serverID);

            var repository = new WorkspaceItemRepository(GetUniqueRepositoryPath());
            repository.AddWorkspaceItem(model.Object);

            Assert.AreEqual(1, repository.WorkspaceItems.Count);
            Assert.AreEqual(WorkspaceItem.SourceServiceType, repository.WorkspaceItems[0].ServiceType);
        }

        [TestMethod]
        public void WorkspaceItemRepositoryAddWorkspaceItemWithNewModelExpectedInvokesWrite()
        {


            var mockConn = new Mock<IEnvironmentConnection>();
            mockConn.Setup(c => c.IsConnected).Returns(true);
            var msg = new ExecuteMessage();
            msg.SetMessage("Workspace item updated");
            var payload = JsonConvert.SerializeObject(msg);
            mockConn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>())).Returns(new StringBuilder(payload)).Verifiable();

            var model = CreateModel(ResourceType.Service, mockConn, out string resourceName, out Guid workspaceID, out Guid serverID);

            var repositoryPath = GetUniqueRepositoryPath();
            Assert.IsFalse(File.Exists(repositoryPath));

            var repository = new WorkspaceItemRepository(repositoryPath);
            repository.AddWorkspaceItem(model.Object);
            Assert.IsTrue(File.Exists(repositoryPath));
        }

        //Added by Massimo.Guerrera this will ensure that when saving a remote workflow that it will save to the right workspace
        [TestMethod]
        public void WorkspaceItemRepositoryAddWorkspaceItemWithNewModelWithSameNameExpectedInvokesWrite()
        {
            var workspaceID = Guid.NewGuid();
            var serverID = Guid.NewGuid();
            var envID = Guid.NewGuid();

            var mockConn = new Mock<IEnvironmentConnection>();
            mockConn.Setup(c => c.IsConnected).Returns(true);
            var msg = new ExecuteMessage();
            msg.SetMessage("Workspace item updated");
            var payload = JsonConvert.SerializeObject(msg);
            mockConn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>())).Returns(new StringBuilder(payload)).Verifiable();

            var model1 = CreateModel(ResourceType.Service, mockConn, workspaceID, serverID, envID);
            workspaceID = Guid.NewGuid();
            serverID = Guid.NewGuid();
            envID = Guid.NewGuid();
            var model2 = CreateModel(ResourceType.Service, mockConn, workspaceID, serverID, envID);

            var repositoryPath = GetUniqueRepositoryPath();
            Assert.IsFalse(File.Exists(repositoryPath));

            var repository = new WorkspaceItemRepository(repositoryPath);
            repository.AddWorkspaceItem(model1.Object);
            repository.AddWorkspaceItem(model2.Object);
            Assert.IsTrue(repository.WorkspaceItems.Count == 2);
            Assert.IsTrue(File.Exists(repositoryPath));
        }

        #endregion

        #region Remove

        [TestMethod]
        public void WorkspaceItemRepositoryRemoveWithNonExistingModelExpectedDoesNothing()
        {

            var mockConn = new Mock<IEnvironmentConnection>();
            mockConn.Setup(c => c.IsConnected).Returns(true);
            var msg = new ExecuteMessage();
            msg.SetMessage("Workspace item updated");
            var payload = JsonConvert.SerializeObject(msg);
            mockConn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>())).Returns(new StringBuilder(payload)).Verifiable();

            var model = CreateModel(ResourceType.Service, mockConn, out string resourceName, out Guid workspaceID, out Guid serverID);

            var repository = new WorkspaceItemRepository(GetUniqueRepositoryPath());
            repository.AddWorkspaceItem(model.Object);
            Assert.AreEqual(1, repository.WorkspaceItems.Count);

            model.Setup(m => m.ResourceName).Returns("Test_" + Guid.NewGuid());

            repository.Remove(model.Object);
            Assert.AreEqual(1, repository.WorkspaceItems.Count);
        }

        [TestMethod]
        public void WorkspaceItemRepositoryRemoveWithExistingModelExpectedRemovesItem()
        {

            var mockConn = new Mock<IEnvironmentConnection>();
            mockConn.Setup(c => c.IsConnected).Returns(true);
            var msg = new ExecuteMessage();
            msg.SetMessage("Workspace item updated");
            var payload = JsonConvert.SerializeObject(msg);
            mockConn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>())).Returns(new StringBuilder(payload)).Verifiable();
            var mockResourceRepo = new Mock<IResourceRepository>();
            mockResourceRepo.Setup(resourceRepository => resourceRepository.DeleteResourceFromWorkspaceAsync(It.IsAny<IContextualResourceModel>()));
            var model = CreateModel(ResourceType.Service, mockConn, out string resourceName, out Guid workspaceID, out Guid serverID, mockResourceRepo);

            var repository = new WorkspaceItemRepository(GetUniqueRepositoryPath());
            repository.AddWorkspaceItem(model.Object);
            Assert.AreEqual(1, repository.WorkspaceItems.Count);

            repository.Remove(model.Object);
            Assert.AreEqual(0, repository.WorkspaceItems.Count);
            mockResourceRepo.Verify(resourceRepository => resourceRepository.DeleteResourceFromWorkspaceAsync(It.IsAny<IContextualResourceModel>()), Times.Once());
        }

        [TestMethod]
        public void WorkspaceItemRepositoryRemoveWithExistingModelExpectedInvokesWrite()
        {

            var mockConn = new Mock<IEnvironmentConnection>();
            mockConn.Setup(c => c.IsConnected).Returns(true);
            var msg = new ExecuteMessage();
            msg.SetMessage("Workspace item updated");
            var payload = JsonConvert.SerializeObject(msg);
            mockConn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>())).Returns(new StringBuilder(payload)).Verifiable();
            var mockResourceRepo = new Mock<IResourceRepository>();
            mockResourceRepo.Setup(resourceRepository => resourceRepository.DeleteResourceFromWorkspaceAsync(It.IsAny<IContextualResourceModel>()));
            var model = CreateModel(ResourceType.Service, mockConn, out string resourceName, out Guid workspaceID, out Guid serverID, mockResourceRepo);

            var repositoryPath = GetUniqueRepositoryPath();
            Assert.IsFalse(File.Exists(repositoryPath));

            var repository = new WorkspaceItemRepository(repositoryPath);
            repository.AddWorkspaceItem(model.Object);
            if(File.Exists(repositoryPath))
            {
                File.Delete(repositoryPath);
            }
            repository.Remove(model.Object);
            Assert.IsTrue(File.Exists(repositoryPath));
            mockResourceRepo.Verify(resourceRepository => resourceRepository.DeleteResourceFromWorkspaceAsync(It.IsAny<IContextualResourceModel>()), Times.Once());
        }


        #endregion

        #region CreateModel

        static Mock<IContextualResourceModel> CreateModel(ResourceType resourceType, Mock<IEnvironmentConnection> mockConnection, out string resourceName, out Guid workspaceID, out Guid serverID, Mock<IResourceRepository> resourceRepoMock = null)
        {
            resourceName = "Test_" + Guid.NewGuid();

            workspaceID = Guid.NewGuid();
            serverID = Guid.NewGuid();

            if(mockConnection == null)
            {
                mockConnection = new Mock<IEnvironmentConnection>();
            }
            mockConnection.Setup(c => c.WorkspaceID).Returns(workspaceID);
            mockConnection.Setup(c => c.ServerID).Returns(serverID);

            var env = new Mock<IServer>();
            env.Setup(c => c.Connection).Returns(mockConnection.Object);
            var model = new Mock<IContextualResourceModel>();
            model.Setup(m => m.Environment).Returns(env.Object);
            model.Setup(m => m.ResourceName).Returns(resourceName);
            model.Setup(m => m.ResourceType).Returns(resourceType);

            model.Setup(c => c.Environment.Connection).Returns(mockConnection.Object);
            model.Setup(c => c.Environment.ResourceRepository).Returns(resourceRepoMock == null ? new Mock<IResourceRepository>().Object : resourceRepoMock.Object);
            return model;
        }

        static Mock<IContextualResourceModel> CreateModel(ResourceType resourceType, Mock<IEnvironmentConnection> mockConnection, Guid workspaceID, Guid serverID, Guid envID)
        {
            var resourceName = "Test_" + Guid.NewGuid();

            mockConnection.Setup(c => c.WorkspaceID).Returns(workspaceID);
            mockConnection.Setup(c => c.ServerID).Returns(serverID);

            var env = new Mock<IServer>();
            env.Setup(e => e.EnvironmentID).Returns(envID);
            env.Setup(environmentModel => environmentModel.Connection).Returns(mockConnection.Object);
            var model = new Mock<IContextualResourceModel>();
            model.Setup(m => m.Environment).Returns(env.Object);
            model.Setup(m => m.ResourceName).Returns(resourceName);
            model.Setup(m => m.ResourceType).Returns(resourceType);
            model.Setup(m => m.ID).Returns(Guid.NewGuid());

            model.Setup(c => c.Environment.Connection).Returns(mockConnection.Object);
            return model;
        }
        #endregion

        #region GetUniqueRepositoryPath

        static string GetUniqueRepositoryPath()
        {
            return Path.Combine(_testDir, string.Format("WorkspaceItems{0}.xml", Guid.NewGuid()));
        }

        #endregion
    }
}
