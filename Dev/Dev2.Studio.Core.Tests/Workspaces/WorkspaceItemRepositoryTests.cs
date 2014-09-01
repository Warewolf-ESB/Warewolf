using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using Dev2.Communication;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;

// ReSharper disable InconsistentNaming
namespace Dev2.Core.Tests.Workspaces
{
    // BUG 9492 - 2013.06.08 - TWR : added
    [TestClass]
    [ExcludeFromCodeCoverage]
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
            string resourceName;
            Guid workspaceID;
            Guid serverID;

            var mockConn = new Mock<IEnvironmentConnection>();
            mockConn.Setup(c => c.IsConnected).Returns(true);
            ExecuteMessage msg = new ExecuteMessage { HasError = false };
            msg.SetMessage("Workspace item updated");

            var payload = JsonConvert.SerializeObject(msg);
            mockConn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new StringBuilder(payload)).Verifiable();

            var model = CreateModel(ResourceType.Service, mockConn, out resourceName, out workspaceID, out serverID);

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
        [Description("Update workspace item IsWorkflowSaved based on the resource")]
        [Owner("Huggs")]
        public void WorkspaceItemRepository_UnitTest_UpdateWorkspaceItemIsWorkflowSaved_ExpectSetsWorkspaceItemIsWorkflowSavedFalse()
        {
            string resourceName;
            Guid workspaceID;
            Guid serverID;


            var mockConn = new Mock<IEnvironmentConnection>();
            mockConn.Setup(c => c.IsConnected).Returns(true);
            ExecuteMessage msg = new ExecuteMessage();
            msg.SetMessage("Workspace item updated");
            var payload = JsonConvert.SerializeObject(msg);
            mockConn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new StringBuilder(payload)).Verifiable();

            var model = CreateModel(ResourceType.Service, mockConn, out resourceName, out workspaceID, out serverID);

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
            string resourceName;
            Guid workspaceID;
            Guid serverID;



            var mockConn = new Mock<IEnvironmentConnection>();
            mockConn.Setup(c => c.IsConnected).Returns(true);
            ExecuteMessage msg = new ExecuteMessage();
            msg.SetMessage("Workspace item updated");
            var payload = JsonConvert.SerializeObject(msg);
            mockConn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new StringBuilder(payload)).Verifiable();

            var model = CreateModel(ResourceType.Service, mockConn, out resourceName, out workspaceID, out serverID);


            var repository = new WorkspaceItemRepository(GetUniqueRepositoryPath());
            repository.AddWorkspaceItem(model.Object);

            Assert.AreEqual(1, repository.WorkspaceItems.Count);

            repository.AddWorkspaceItem(model.Object);
            Assert.AreEqual(1, repository.WorkspaceItems.Count);
        }

        [TestMethod]
        public void WorkspaceItemRepositoryAddWorkspaceItemWithNewModelExpectedAddsAndAssignsWorkspaceID()
        {
            string resourceName;
            Guid workspaceID;
            Guid serverID;


            var mockConn = new Mock<IEnvironmentConnection>();
            mockConn.Setup(c => c.IsConnected).Returns(true);
            ExecuteMessage msg = new ExecuteMessage();
            msg.SetMessage("Workspace item updated");
            var payload = JsonConvert.SerializeObject(msg);
            mockConn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new StringBuilder(payload)).Verifiable();

            var model = CreateModel(ResourceType.Service, mockConn, out resourceName, out workspaceID, out serverID);

            var repository = new WorkspaceItemRepository(GetUniqueRepositoryPath());
            repository.AddWorkspaceItem(model.Object);

            Assert.AreEqual(1, repository.WorkspaceItems.Count);
            Assert.AreEqual(workspaceID, repository.WorkspaceItems[0].WorkspaceID);
        }

        [TestMethod]
        public void WorkspaceItemRepositoryAddWorkspaceItemWithNewModelExpectedAddsAndAssignsServerID()
        {
            string resourceName;
            Guid workspaceID;
            Guid serverID;


            var mockConn = new Mock<IEnvironmentConnection>();
            mockConn.Setup(c => c.IsConnected).Returns(true);
            ExecuteMessage msg = new ExecuteMessage();
            msg.SetMessage("Workspace item updated");
            var payload = JsonConvert.SerializeObject(msg);
            mockConn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new StringBuilder(payload)).Verifiable();

            var model = CreateModel(ResourceType.Service, mockConn, out resourceName, out workspaceID, out serverID);

            var repository = new WorkspaceItemRepository(GetUniqueRepositoryPath());
            repository.AddWorkspaceItem(model.Object);

            Assert.AreEqual(1, repository.WorkspaceItems.Count);
            Assert.AreEqual(serverID, repository.WorkspaceItems[0].ServerID);
        }

        [TestMethod]
        public void WorkspaceItemRepositoryAddWorkspaceItemWithNewModelExpectedAddsAndAssignsServiceName()
        {
            string resourceName;
            Guid workspaceID;
            Guid serverID;


            var mockConn = new Mock<IEnvironmentConnection>();
            mockConn.Setup(c => c.IsConnected).Returns(true);
            ExecuteMessage msg = new ExecuteMessage();
            msg.SetMessage("Workspace item updated");
            var payload = JsonConvert.SerializeObject(msg);
            mockConn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new StringBuilder(payload)).Verifiable();

            var model = CreateModel(ResourceType.Service, mockConn, out resourceName, out workspaceID, out serverID);

            var repository = new WorkspaceItemRepository(GetUniqueRepositoryPath());
            repository.AddWorkspaceItem(model.Object);

            Assert.AreEqual(1, repository.WorkspaceItems.Count);
            Assert.AreEqual(resourceName, repository.WorkspaceItems[0].ServiceName);
        }

        [TestMethod]
        public void WorkspaceItemRepositoryAddWorkspaceItemWithNewServiceModelExpectedAddsAndAssignsServiceServiceType()
        {
            string resourceName;
            Guid workspaceID;
            Guid serverID;

            var mockConn = new Mock<IEnvironmentConnection>();
            mockConn.Setup(c => c.IsConnected).Returns(true);
            ExecuteMessage msg = new ExecuteMessage();
            msg.SetMessage("Workspace item updated");
            var payload = JsonConvert.SerializeObject(msg);
            mockConn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new StringBuilder(payload)).Verifiable();

            var model = CreateModel(ResourceType.Service, mockConn, out resourceName, out workspaceID, out serverID);

            var repository = new WorkspaceItemRepository(GetUniqueRepositoryPath());
            repository.AddWorkspaceItem(model.Object);

            Assert.AreEqual(1, repository.WorkspaceItems.Count);
            Assert.AreEqual(WorkspaceItem.ServiceServiceType, repository.WorkspaceItems[0].ServiceType);
        }

        [TestMethod]
        public void WorkspaceItemRepositoryAddWorkspaceItemWithNewSourceModelExpectedAddsAndAssignsSourceServiceType()
        {
            string resourceName;
            Guid workspaceID;
            Guid serverID;

            var mockConn = new Mock<IEnvironmentConnection>();
            mockConn.Setup(c => c.IsConnected).Returns(true);
            ExecuteMessage msg = new ExecuteMessage();
            msg.SetMessage("Workspace item updated");
            var payload = JsonConvert.SerializeObject(msg);
            mockConn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new StringBuilder(payload)).Verifiable();

            var model = CreateModel(ResourceType.Source, mockConn, out resourceName, out workspaceID, out serverID);

            var repository = new WorkspaceItemRepository(GetUniqueRepositoryPath());
            repository.AddWorkspaceItem(model.Object);

            Assert.AreEqual(1, repository.WorkspaceItems.Count);
            Assert.AreEqual(WorkspaceItem.SourceServiceType, repository.WorkspaceItems[0].ServiceType);
        }

        [TestMethod]
        public void WorkspaceItemRepositoryAddWorkspaceItemWithNewModelExpectedInvokesWrite()
        {
            string resourceName;
            Guid workspaceID;
            Guid serverID;


            var mockConn = new Mock<IEnvironmentConnection>();
            mockConn.Setup(c => c.IsConnected).Returns(true);
            ExecuteMessage msg = new ExecuteMessage();
            msg.SetMessage("Workspace item updated");
            var payload = JsonConvert.SerializeObject(msg);
            mockConn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new StringBuilder(payload)).Verifiable();

            var model = CreateModel(ResourceType.Service, mockConn, out resourceName, out workspaceID, out serverID);

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
            Guid workspaceID = Guid.NewGuid();
            Guid serverID = Guid.NewGuid();
            Guid envID = Guid.NewGuid();

            var mockConn = new Mock<IEnvironmentConnection>();
            mockConn.Setup(c => c.IsConnected).Returns(true);
            ExecuteMessage msg = new ExecuteMessage();
            msg.SetMessage("Workspace item updated");
            var payload = JsonConvert.SerializeObject(msg);
            mockConn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new StringBuilder(payload)).Verifiable();

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

        #region UpdateWorkspaceItem

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WorkspaceItemRepositoryUpdateWorkspaceItemWithNullModelExpectedThrowsArgumentNullException()
        {
            var repository = new WorkspaceItemRepository(GetUniqueRepositoryPath());
            repository.UpdateWorkspaceItem(null, false);
        }

        [TestMethod]
        public void WorkspaceItemRepositoryUpdateWorkspaceItemWithNonExistingModelExpectedDoesNothing()
        {
            string resourceName;
            Guid workspaceID;
            Guid serverID;

            var mockConn = new Mock<IEnvironmentConnection>();
            mockConn.Setup(c => c.IsConnected).Returns(true);
            ExecuteMessage msg = new ExecuteMessage();
            msg.SetMessage("");
            var payload = JsonConvert.SerializeObject(msg);
            mockConn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new StringBuilder(payload)).Verifiable();

            var model = CreateModel(ResourceType.Service, mockConn, out resourceName, out workspaceID, out serverID);

            var repository = new WorkspaceItemRepository(GetUniqueRepositoryPath());

            var result = repository.UpdateWorkspaceItem(model.Object, true);
            Assert.IsTrue(string.IsNullOrEmpty(result.Message.ToString()));
        }

        [TestMethod]
        public void WorkspaceItemRepositoryUpdateWorkspaceItemWithExistingModelExpectedInvokesExecuteCommand()
        {
            const string ExpectedResult = "Workspace item updated";
            string resourceName;
            Guid workspaceID;
            Guid serverID;

            var mockConn = new Mock<IEnvironmentConnection>();
            mockConn.Setup(c => c.IsConnected).Returns(true);
            ExecuteMessage msg = new ExecuteMessage();
            msg.SetMessage("Workspace item updated");
            var payload = JsonConvert.SerializeObject(msg);
            mockConn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new StringBuilder(payload)).Verifiable();

            var model = CreateModel(ResourceType.Service, mockConn, out resourceName, out workspaceID, out serverID);

            #region Setup ImportService - GRRR!


            #endregion

            var repository = new WorkspaceItemRepository(GetUniqueRepositoryPath());
            repository.AddWorkspaceItem(model.Object);

            var result = repository.UpdateWorkspaceItem(model.Object, true);
            mockConn.Verify(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Once());
            Assert.AreEqual(ExpectedResult, result.Message.ToString());
        }

        #endregion

        #region Remove

        [TestMethod]
        public void WorkspaceItemRepositoryRemoveWithNonExistingModelExpectedDoesNothing()
        {
            string resourceName;
            Guid workspaceID;
            Guid serverID;

            var mockConn = new Mock<IEnvironmentConnection>();
            mockConn.Setup(c => c.IsConnected).Returns(true);
            ExecuteMessage msg = new ExecuteMessage();
            msg.SetMessage("Workspace item updated");
            var payload = JsonConvert.SerializeObject(msg);
            mockConn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new StringBuilder(payload)).Verifiable();

            var model = CreateModel(ResourceType.Service, mockConn, out resourceName, out workspaceID, out serverID);

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
            string resourceName;
            Guid workspaceID;
            Guid serverID;

            var mockConn = new Mock<IEnvironmentConnection>();
            mockConn.Setup(c => c.IsConnected).Returns(true);
            ExecuteMessage msg = new ExecuteMessage();
            msg.SetMessage("Workspace item updated");
            var payload = JsonConvert.SerializeObject(msg);
            mockConn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new StringBuilder(payload)).Verifiable();
            var mockResourceRepo = new Mock<IResourceRepository>();
            mockResourceRepo.Setup(resourceRepository => resourceRepository.DeleteResourceFromWorkspace(It.IsAny<IContextualResourceModel>()));
            var model = CreateModel(ResourceType.Service, mockConn, out resourceName, out workspaceID, out serverID, mockResourceRepo);

            var repository = new WorkspaceItemRepository(GetUniqueRepositoryPath());
            repository.AddWorkspaceItem(model.Object);
            Assert.AreEqual(1, repository.WorkspaceItems.Count);

            repository.Remove(model.Object);
            Assert.AreEqual(0, repository.WorkspaceItems.Count);
            mockResourceRepo.Verify(resourceRepository => resourceRepository.DeleteResourceFromWorkspace(It.IsAny<IContextualResourceModel>()), Times.Once());
        }

        [TestMethod]
        public void WorkspaceItemRepositoryRemoveWithExistingModelExpectedInvokesWrite()
        {
            string resourceName;
            Guid workspaceID;
            Guid serverID;

            var mockConn = new Mock<IEnvironmentConnection>();
            mockConn.Setup(c => c.IsConnected).Returns(true);
            ExecuteMessage msg = new ExecuteMessage();
            msg.SetMessage("Workspace item updated");
            var payload = JsonConvert.SerializeObject(msg);
            mockConn.Setup(c => c.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new StringBuilder(payload)).Verifiable();
            var mockResourceRepo = new Mock<IResourceRepository>();
            mockResourceRepo.Setup(resourceRepository => resourceRepository.DeleteResourceFromWorkspace(It.IsAny<IContextualResourceModel>()));
            var model = CreateModel(ResourceType.Service, mockConn, out resourceName, out workspaceID, out serverID, mockResourceRepo);

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
            mockResourceRepo.Verify(resourceRepository => resourceRepository.DeleteResourceFromWorkspace(It.IsAny<IContextualResourceModel>()), Times.Once());
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

            var env = new Mock<IEnvironmentModel>();
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

            var env = new Mock<IEnvironmentModel>();
            env.Setup(e => e.ID).Returns(envID);
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
