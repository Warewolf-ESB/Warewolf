
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Dev2.Common.Interfaces.Data.TO;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Scheduler.Interfaces;
using Dev2.Communication;
using Dev2.DataList.Contract;
using Dev2.Settings.Scheduler;
using Dev2.Studio.Core.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

// ReSharper disable InconsistentNaming
namespace Dev2.Core.Tests.Settings
{
    [TestClass]
    public class ClientScheduledResourceTests
    {
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ClientScheduledResourceModel_GetScheduledResources")]
        public void ClientScheduledResourceModel_GetScheduledResources_ReturnsCollectionOfIScheduledResource()
        {
            //------------Setup for test--------------------------
            var resources = new ObservableCollection<IScheduledResource>();
            var scheduledResourceForTest = new ScheduledResourceForTest();
            resources.Add(scheduledResourceForTest);
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            var serializeObject = serializer.SerializeToBuilder(resources);
            var mockEnvironmentModel = new Mock<IEnvironmentModel>();
            var mockConnection = new Mock<IEnvironmentConnection>();
            mockConnection.Setup(connection => connection.IsConnected).Returns(true);
            mockConnection.Setup(connection => connection.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(serializeObject);
            mockConnection.Setup(connection => connection.WorkspaceID).Returns(Guid.NewGuid());
            mockEnvironmentModel.Setup(model => model.Connection).Returns(mockConnection.Object);
            var clientScheduledResourceModel = new ClientScheduledResourceModel(mockEnvironmentModel.Object);
            //------------Execute Test---------------------------
            var scheduledResources = clientScheduledResourceModel.GetScheduledResources();
            //------------Assert Results-------------------------
            Assert.AreEqual(1, scheduledResources.Count);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ClientScheduledResourceModel_DeleteScheduledResource")]
        public void ClientScheduledResourceModel_DeleteScheduledResource_CallsCommunicationsController()
        {
            //------------Setup for test--------------------------
            var scheduledResourceForTest = new ScheduledResourceForTest();
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            var serializeObject = serializer.SerializeToBuilder(scheduledResourceForTest);
            var esbPayLoad = new EsbExecuteRequest { ServiceName = "DeleteScheduledResourceService" };
            esbPayLoad.AddArgument("Resource", serializeObject);
            var mockEnvironmentModel = new Mock<IEnvironmentModel>();
            var mockConnection = new Mock<IEnvironmentConnection>();
            mockConnection.Setup(connection => connection.IsConnected).Returns(true);
            mockConnection.Setup(connection => connection.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Verifiable();
            mockConnection.Setup(connection => connection.WorkspaceID).Returns(Guid.NewGuid());
            mockEnvironmentModel.Setup(model => model.Connection).Returns(mockConnection.Object);
            var clientScheduledResourceModel = new ClientScheduledResourceModel(mockEnvironmentModel.Object);
            //------------Execute Test---------------------------
            clientScheduledResourceModel.DeleteSchedule(scheduledResourceForTest);
            //------------Assert Results-------------------------
            mockConnection.Verify(connection => connection.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Once());
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ClientScheduledResourceModel_SaveScheduledResource")]
        public void ClientScheduledResourceModel_SaveScheduledResource_CallsCommunicationsController()
        {
            //------------Setup for test--------------------------
            var scheduledResourceForTest = new ScheduledResourceForTest();
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            var serializeObject = serializer.SerializeToBuilder(scheduledResourceForTest);
            var esbPayLoad = new EsbExecuteRequest { ServiceName = "AddScheduledResourceService" };
            esbPayLoad.AddArgument("Resource", serializeObject);
            var mockEnvironmentModel = new Mock<IEnvironmentModel>();
            var mockConnection = new Mock<IEnvironmentConnection>();
            mockConnection.Setup(connection => connection.IsConnected).Returns(true);
            mockConnection.Setup(connection => connection.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Verifiable();
            mockConnection.Setup(connection => connection.WorkspaceID).Returns(Guid.NewGuid());
            mockEnvironmentModel.Setup(model => model.Connection).Returns(mockConnection.Object);
            var clientScheduledResourceModel = new ClientScheduledResourceModel(mockEnvironmentModel.Object);
            //------------Execute Test---------------------------
            string errorMessage;
            var saved = clientScheduledResourceModel.Save(scheduledResourceForTest, out errorMessage);
            //------------Assert Results-------------------------
            mockConnection.Verify(connection => connection.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Once());
            Assert.IsTrue(saved);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ClientScheduledResourceModel_SaveScheduledResource")]
        public void ClientScheduledResourceModel_SaveScheduledResource_HasError_CallsCommunicationsController()
        {
            //------------Setup for test--------------------------
            var scheduledResourceForTest = new ScheduledResourceForTest();
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            var serializeObject = serializer.SerializeToBuilder(scheduledResourceForTest);
            var esbPayLoad = new EsbExecuteRequest { ServiceName = "AddScheduledResourceService" };
            esbPayLoad.AddArgument("Resource", serializeObject);
            var returnMessage = new ExecuteMessage { HasError = true, Message = new StringBuilder("Error occurred") };
            var serializedReturnMessage = serializer.SerializeToBuilder(returnMessage);
            var mockEnvironmentModel = new Mock<IEnvironmentModel>();
            var mockConnection = new Mock<IEnvironmentConnection>();
            mockConnection.Setup(connection => connection.IsConnected).Returns(true);
            mockConnection.Setup(connection => connection.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Verifiable();
            mockConnection.Setup(connection => connection.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(serializedReturnMessage);
            mockConnection.Setup(connection => connection.WorkspaceID).Returns(Guid.NewGuid());
            mockEnvironmentModel.Setup(model => model.Connection).Returns(mockConnection.Object);
            var clientScheduledResourceModel = new ClientScheduledResourceModel(mockEnvironmentModel.Object);
            //------------Execute Test---------------------------
            string errorMessage;
            var saved = clientScheduledResourceModel.Save(scheduledResourceForTest, out errorMessage);
            //------------Assert Results-------------------------
            mockConnection.Verify(connection => connection.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Once());
            Assert.IsFalse(saved);
            Assert.AreEqual("Error occurred", errorMessage);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ClientScheduledResourceModel_CreateHistory")]
        public void ClientScheduledResourceModel_CreateHistory_ReturnsListOfIResourceHistory()
        {
            //------------Setup for test--------------------------
            var scheduledResourceForTest = new ScheduledResourceForTest();
            var resourceHistory = new ResourceHistoryForTest();
            var listOfHistoryResources = new List<IResourceHistory> { resourceHistory };
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            var serializeObject = serializer.SerializeToBuilder(listOfHistoryResources);
            var mockEnvironmentModel = new Mock<IEnvironmentModel>();
            var mockConnection = new Mock<IEnvironmentConnection>();
            mockConnection.Setup(connection => connection.IsConnected).Returns(true);
            mockConnection.Setup(connection => connection.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(serializeObject);
            mockConnection.Setup(connection => connection.WorkspaceID).Returns(Guid.NewGuid());
            mockEnvironmentModel.Setup(model => model.Connection).Returns(mockConnection.Object);
            var clientScheduledResourceModel = new ClientScheduledResourceModel(mockEnvironmentModel.Object);
            //------------Execute Test---------------------------
            var resourceHistories = clientScheduledResourceModel.CreateHistory(scheduledResourceForTest);
            //------------Assert Results-------------------------
            mockConnection.Verify(connection => connection.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Once());
            Assert.AreEqual(1, resourceHistories.Count);

        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ClientScheduledResourceModel_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ClientScheduledResourceModel_Constructor_NullEnvironmentModel_ThrowsException()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            // ReSharper disable AssignNullToNotNullAttribute
            // ReSharper disable ObjectCreationAsStatement
            new ClientScheduledResourceModel(null);
            // ReSharper restore ObjectCreationAsStatement
            // ReSharper restore AssignNullToNotNullAttribute
            //------------Assert Results-------------------------
        }
    }

    public class ResourceHistoryForTest : IResourceHistory
    {
        #region Implementation of IResourceHistory

        // ReSharper disable UnusedAutoPropertyAccessor.Local
        public string WorkflowOutput { get; private set; }
        public IList<IDebugState> DebugOutput { get; private set; }
        public IEventInfo TaskHistoryOutput { get; private set; }
        public string UserName { get; set; }

        #endregion
    }

    internal class ScheduledResourceForTest : IScheduledResource
    {
        #region Implementation of IScheduledResource

        public ScheduledResourceForTest()
        {
            Errors = new ErrorResultTO();
        }

        /// <summary>
        /// Property to check if the scheduled resouce is saved
        /// </summary>
        public bool IsDirty { get; set; }
        /// <summary>
        ///     Schedule Name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Represents the old name of the task
        /// </summary>
        public string OldName { get; set; }
        /// <summary>
        ///     Schedule Status
        /// </summary>
        public SchedulerStatus Status { get; set; }
        /// <summary>
        ///     The next time that this schedule will run
        /// </summary>
        public DateTime NextRunDate { get; set; }
        /// <summary>
        ///     Trigger
        /// </summary>
        public IScheduleTrigger Trigger { get; set; }
        /// <summary>
        /// NumberOfHistoryToKeep
        /// </summary>
        public int NumberOfHistoryToKeep { get; set; }
        /// <summary>
        /// The workflow that we will run
        /// </summary>
        public string WorkflowName { get; set; }


        /// <summary>
        /// The workflow that we will run
        /// </summary>
        public Guid ResourceId { get; set; }
        /// <summary>
        /// If a schedule is missed execute as soon as possible
        /// </summary>
        public bool RunAsapIfScheduleMissed { get; set; }
        public bool AllowMultipleIstances { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public IErrorResultTO Errors { get; set; }
        public bool IsNew { get; set; }

        #endregion
    }
}
