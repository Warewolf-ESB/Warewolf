/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.Scheduler.Interfaces;
using Dev2.Communication;
using Dev2.Studio.Interfaces;
using Dev2.Triggers.Scheduler;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;


namespace Dev2.Core.Tests.Triggers
{
    [TestClass]
    [TestCategory("Studio ViewModels Triggers")]
    public class ClientScheduledResourceTests
    {
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ClientScheduledResourceModel")]
        public void ClientScheduledResourceModel_GetScheduledResources_ReturnsCollectionOfIScheduledResource()
        {
            //------------Setup for test--------------------------
            var resources = new ObservableCollection<IScheduledResource>();
            var scheduledResourceForTest = new ScheduledResourceForTest();
            resources.Add(scheduledResourceForTest);
            var serializer = new Dev2JsonSerializer();
            var serializeObject = serializer.SerializeToBuilder(resources);
            var mockEnvironmentModel = new Mock<IServer>();
            var mockConnection = new Mock<IEnvironmentConnection>();
            mockConnection.Setup(connection => connection.IsConnected).Returns(true);
            mockConnection.Setup(connection => connection.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<int>())).Returns(serializeObject);
            mockConnection.Setup(connection => connection.WorkspaceID).Returns(Guid.NewGuid());
            mockEnvironmentModel.Setup(model => model.Connection).Returns(mockConnection.Object);
            var clientScheduledResourceModel = new ClientScheduledResourceModel(mockEnvironmentModel.Object, () => { });
            //------------Execute Test---------------------------
            var scheduledResources = clientScheduledResourceModel.GetScheduledResources();
            //------------Assert Results-------------------------
            Assert.AreEqual(1, scheduledResources.Count);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ClientScheduledResourceModel")]
        public void ClientScheduledResourceModel_DeleteScheduledResource_CallsCommunicationsController()
        {
            //------------Setup for test--------------------------
            var scheduledResourceForTest = new ScheduledResourceForTest();
            var serializer = new Dev2JsonSerializer();
            var serializeObject = serializer.SerializeToBuilder(scheduledResourceForTest);
            var esbPayLoad = new EsbExecuteRequest { ServiceName = "DeleteScheduledResourceService" };
            esbPayLoad.AddArgument("Resource", serializeObject);
            var mockEnvironmentModel = new Mock<IServer>();
            var mockConnection = new Mock<IEnvironmentConnection>();
            mockConnection.Setup(connection => connection.IsConnected).Returns(true);
            mockConnection.Setup(connection => connection.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<int>())).Verifiable();
            mockConnection.Setup(connection => connection.WorkspaceID).Returns(Guid.NewGuid());
            mockEnvironmentModel.Setup(model => model.Connection).Returns(mockConnection.Object);
            var clientScheduledResourceModel = new ClientScheduledResourceModel(mockEnvironmentModel.Object, () => { });
            //------------Execute Test---------------------------
            clientScheduledResourceModel.DeleteSchedule(scheduledResourceForTest);
            //------------Assert Results-------------------------
            mockConnection.Verify(connection => connection.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<int>()), Times.Once());
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ClientScheduledResourceModel")]
        public void ClientScheduledResourceModel_SaveScheduledResource_CallsCommunicationsController()
        {
            //------------Setup for test--------------------------
            var scheduledResourceForTest = new ScheduledResourceForTest();
            var serializer = new Dev2JsonSerializer();
            var serializeObject = serializer.SerializeToBuilder(scheduledResourceForTest);
            var esbPayLoad = new EsbExecuteRequest { ServiceName = "AddScheduledResourceService" };
            esbPayLoad.AddArgument("Resource", serializeObject);
            var mockEnvironmentModel = new Mock<IServer>();
            var mockConnection = new Mock<IEnvironmentConnection>();
            mockConnection.Setup(connection => connection.IsConnected).Returns(true);
            mockConnection.Setup(connection => connection.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<int>())).Verifiable();
            mockConnection.Setup(connection => connection.WorkspaceID).Returns(Guid.NewGuid());
            mockEnvironmentModel.Setup(model => model.Connection).Returns(mockConnection.Object);
            var clientScheduledResourceModel = new ClientScheduledResourceModel(mockEnvironmentModel.Object, () => { });
            //------------Execute Test---------------------------
            var saved = clientScheduledResourceModel.Save(scheduledResourceForTest, out string errorMessage);
            //------------Assert Results-------------------------
            mockConnection.Verify(connection => connection.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<int>()), Times.Once());
            Assert.IsTrue(saved);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ClientScheduledResourceModel")]
        public void ClientScheduledResourceModel_SaveScheduledResource_HasError_CallsCommunicationsController()
        {
            //------------Setup for test--------------------------
            var scheduledResourceForTest = new ScheduledResourceForTest();
            var serializer = new Dev2JsonSerializer();
            var serializeObject = serializer.SerializeToBuilder(scheduledResourceForTest);
            var esbPayLoad = new EsbExecuteRequest { ServiceName = "AddScheduledResourceService" };
            esbPayLoad.AddArgument("Resource", serializeObject);
            var returnMessage = new ExecuteMessage { HasError = true, Message = new StringBuilder("Error occurred") };
            var serializedReturnMessage = serializer.SerializeToBuilder(returnMessage);
            var mockEnvironmentModel = new Mock<IServer>();
            var mockConnection = new Mock<IEnvironmentConnection>();
            mockConnection.Setup(connection => connection.IsConnected).Returns(true);
            mockConnection.Setup(connection => connection.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<int>())).Verifiable();
            mockConnection.Setup(connection => connection.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<int>())).Returns(serializedReturnMessage);
            mockConnection.Setup(connection => connection.WorkspaceID).Returns(Guid.NewGuid());
            mockEnvironmentModel.Setup(model => model.Connection).Returns(mockConnection.Object);
            var clientScheduledResourceModel = new ClientScheduledResourceModel(mockEnvironmentModel.Object, () => { });
            //------------Execute Test---------------------------
            var saved = clientScheduledResourceModel.Save(scheduledResourceForTest, out string errorMessage);
            //------------Assert Results-------------------------
            mockConnection.Verify(connection => connection.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<int>()), Times.Once());
            Assert.IsFalse(saved);
            Assert.AreEqual("Error occurred", errorMessage);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ClientScheduledResourceModel")]
        public void ClientScheduledResourceModel_CreateHistory_ReturnsListOfIResourceHistory()
        {
            //------------Setup for test--------------------------
            var scheduledResourceForTest = new ScheduledResourceForTest();
            var resourceHistory = new ResourceHistoryForTest();
            var listOfHistoryResources = new List<IResourceHistory> { resourceHistory };
            var serializer = new Dev2JsonSerializer();
            var serializeObject = serializer.SerializeToBuilder(listOfHistoryResources);
            var mockEnvironmentModel = new Mock<IServer>();
            var mockConnection = new Mock<IEnvironmentConnection>();
            mockConnection.Setup(connection => connection.IsConnected).Returns(true);
            mockConnection.Setup(connection => connection.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<int>())).Returns(serializeObject);
            mockConnection.Setup(connection => connection.WorkspaceID).Returns(Guid.NewGuid());
            mockEnvironmentModel.Setup(model => model.Connection).Returns(mockConnection.Object);
            var clientScheduledResourceModel = new ClientScheduledResourceModel(mockEnvironmentModel.Object, () => { });
            //------------Execute Test---------------------------
            var resourceHistories = clientScheduledResourceModel.CreateHistory(scheduledResourceForTest);
            //------------Assert Results-------------------------
            mockConnection.Verify(connection => connection.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<int>()), Times.Once());
            Assert.AreEqual(1, resourceHistories.Count);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ClientScheduledResourceModel")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ClientScheduledResourceModel_Constructor_NullEnvironmentModel_ThrowsException()
        {
            new ClientScheduledResourceModel(null, () => { });
        }
    }
}
