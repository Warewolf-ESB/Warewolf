using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Caliburn.Micro;
using Dev2.Communication;
using Dev2.Diagnostics;
using Dev2.Messages;
using Dev2.Providers.Events;
using Dev2.Services.Security;
using Dev2.Settings.Scheduler;
using Dev2.Studio.AppResources.Comparers;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.Models;
using Dev2.Studio.Core.ViewModels;
using Dev2.Studio.ViewModels.Diagnostics;
using Dev2.Studio.ViewModels.WorkSurface;
using Dev2.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;

// ReSharper disable InconsistentNaming
namespace Dev2.Core.Tests.ViewModelTests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class WorkSurfaceContextViewModelTests
    {
        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            AppSettings.LocalHost = "http://localhost:3142";
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("WorkSurfaceContextViewModel_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WorkSurfaceContextViewModel_Constructor_NullWorkSurfaceKey_ExpectException()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            // ReSharper disable ObjectCreationAsStatement
            new WorkSurfaceContextViewModel(null, new Mock<IWorkSurfaceViewModel>().Object);
            // ReSharper restore ObjectCreationAsStatement
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("WorkSurfaceContextViewModel_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WorkSurfaceContextViewModel_Constructor_NullWorkSurfaceViewModel_ExpectException()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            // ReSharper disable ObjectCreationAsStatement
            new WorkSurfaceContextViewModel(new WorkSurfaceKey(), null);
            // ReSharper restore ObjectCreationAsStatement
            //------------Assert Results-------------------------
        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("WorkSurfaceContextViewModel_Constructor")]
        public void WorkSurfaceContextViewModel_Constructor_ValidArguments_DebugOutputViewModelNotNull()
        {
            //------------Setup for test--------------------------
            CompositionInitializer.InitializeForMeflessBaseViewModel();
            var workSurfaceKey = new WorkSurfaceKey();
            var mockWorkSurfaceViewModel = new Mock<IWorkflowDesignerViewModel>();
            var mockedConn = new Mock<IEnvironmentConnection>();
            mockedConn.Setup(conn => conn.ServerEvents).Returns(new Mock<IEventPublisher>().Object);
            var mockEnvironmentModel = new Mock<IEnvironmentModel>();
            mockEnvironmentModel.Setup(model => model.Connection).Returns(mockedConn.Object);
            var environmentModel = mockEnvironmentModel.Object;
            mockWorkSurfaceViewModel.Setup(model => model.EnvironmentModel).Returns(environmentModel);
            var workSurfaceViewModel = mockWorkSurfaceViewModel.As<IWorkSurfaceViewModel>().Object;
            //------------Execute Test---------------------------
            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(workSurfaceKey, workSurfaceViewModel);
            //------------Assert Results-------------------------
            Assert.IsNotNull(workSurfaceContextViewModel);
            Assert.IsNotNull(workSurfaceContextViewModel.DebugOutputViewModel);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("WorkSurfaceContextViewModel_Constructor")]
        public void WorkSurfaceContextViewModel_Constructor_SchedularWorksurfaceContext_DebugOutputViewModelNotNull()
        {
            //------------Setup for test--------------------------
            CompositionInitializer.InitializeForMeflessBaseViewModel();
            var workSurfaceKey = new WorkSurfaceKey();
            workSurfaceKey.WorkSurfaceContext = WorkSurfaceContext.Scheduler;
            var mockWorkSurfaceViewModel = new SchedulerViewModel();
            //------------Execute Test---------------------------
            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(workSurfaceKey, mockWorkSurfaceViewModel);
            //------------Assert Results-------------------------
            Assert.IsNotNull(workSurfaceContextViewModel);
            Assert.IsNotNull(workSurfaceContextViewModel.DebugOutputViewModel);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("WorkSurfaceContextViewModel_EnvironmentModelIsConnectedChanged")]
        public void WorkSurfaceContextViewModel_EnvironmentModelIsConnectedChanged_False_DebugStatusFinished()
        {
            //------------Setup for test--------------------------
            CompositionInitializer.InitializeForMeflessBaseViewModel();
            var workSurfaceKey = new WorkSurfaceKey();
            var mockWorkSurfaceViewModel = new Mock<IWorkflowDesignerViewModel>();
            var mockedConn = new Mock<IEnvironmentConnection>();
            mockedConn.Setup(conn => conn.ServerEvents).Returns(new Mock<IEventPublisher>().Object);
            var mockEnvironmentModel = new Mock<IEnvironmentModel>();
            mockEnvironmentModel.Setup(model => model.Connection).Returns(mockedConn.Object);
            var environmentModel = mockEnvironmentModel.Object;
            mockWorkSurfaceViewModel.Setup(model => model.EnvironmentModel).Returns(environmentModel);
            var workSurfaceViewModel = mockWorkSurfaceViewModel.As<IWorkSurfaceViewModel>().Object;
            var connectedEventArgs = new ConnectedEventArgs { IsConnected = false };
            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(workSurfaceKey, workSurfaceViewModel) { DebugOutputViewModel = { DebugStatus = DebugStatus.Executing } };
            //------------Execute Test---------------------------
            mockEnvironmentModel.Raise(model => model.IsConnectedChanged += null, connectedEventArgs);
            //------------Assert Results-------------------------
            Assert.AreEqual(DebugStatus.Finished, workSurfaceContextViewModel.DebugOutputViewModel.DebugStatus);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("WorkSurfaceContextViewModel_HandleDebugOutputMessage")]
        public void WorkSurfaceContextViewModel_DebugOutputMessage_DebugStateHasData_OnlyOneRootItemIsDisplayed()
        {
            //------------Setup for test--------------------------
            CompositionInitializer.InitializeForMeflessBaseViewModel();
            var workSurfaceKey = new WorkSurfaceKey();
            workSurfaceKey.WorkSurfaceContext = WorkSurfaceContext.Scheduler;
            var mockWorkSurfaceViewModel = new Mock<IWorkflowDesignerViewModel>();
            var mockedConn = new Mock<IEnvironmentConnection>();
            mockedConn.Setup(conn => conn.ServerEvents).Returns(new Mock<IEventPublisher>().Object);
            var mockEnvironmentModel = new Mock<IEnvironmentModel>();
            mockEnvironmentModel.Setup(model => model.Connection).Returns(mockedConn.Object);
            var environmentModel = mockEnvironmentModel.Object;
            mockWorkSurfaceViewModel.Setup(model => model.EnvironmentModel).Returns(environmentModel);
            var workSurfaceViewModel = mockWorkSurfaceViewModel.As<IWorkSurfaceViewModel>().Object;
            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(workSurfaceKey, workSurfaceViewModel) { DebugOutputViewModel = { DebugStatus = DebugStatus.Executing } };

            const string message = @"[{""ID"":""db2bd5e6-6894-44ca-8ce4-f99441a4e612"",""ParentID"":""00000000-0000-0000-0000-000000000000"",""ServerID"":""b2037c1e-5482-4d5d-9e9a-bd8023385cb7"",""EnvironmentID"":""00000000-0000-0000-0000-000000000000"",""ClientID"":""00000000-0000-0000-0000-000000000000"",""StateType"":2,""DisplayName"":""Count Records"",""HasError"":false,""ErrorMessage"":"""",""Version"":"""",""Name"":""Count Records"",""ActivityType"":1,""Duration"":""00:00:00.0390000"",""DurationString"":""PT0.039S"",""StartTime"":""2014-03-20T15:10:58.3312466+02:00"",""EndTime"":""2014-03-20T15:10:58.3702466+02:00"",""Inputs"":[],""Outputs"":[{""ResultsList"":[{""Type"":1,""Label"":"""",""Variable"":""[[res]]"",""Operator"":""="",""Value"":""10"",""GroupName"":null,""GroupIndex"":0,""MoreLink"":null}]}],""Server"":""5852ba5d-434c-4866-91cf-b7c1ebf38747"",""WorkspaceID"":""00000000-0000-0000-0000-000000000000"",""OriginalInstanceID"":""14e13cb3-9868-45ef-908c-a6fb960697f8"",""OriginatingResourceID"":""00000000-0000-0000-0000-000000000000"",""IsSimulation"":false,""Message"":null,""NumberOfSteps"":0,""Origin"":"""",""ExecutionOrigin"":0,""ExecutionOriginDescription"":null,""ExecutingUser"":null,""SessionID"":""00000000-0000-0000-0000-000000000000""}]";
            var tmp = JsonConvert.DeserializeObject<IList<DebugState>>(message);
            //------------Execute Test---------------------------
            workSurfaceContextViewModel.Handle(new DebugOutputMessage(tmp));
            //------------Assert Results-------------------------
            Assert.AreEqual(1, workSurfaceContextViewModel.DebugOutputViewModel.RootItems.Count);
        }

        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("WorkSurfaceContextViewModel_HandleDebugOutputMessage")]
        public void WorkSurfaceContextViewModel_DebugOutputMessage_DebugStateHasNoData_RootItemsIsZero()
        {
            //------------Setup for test--------------------------
            CompositionInitializer.InitializeForMeflessBaseViewModel();
            var workSurfaceKey = new WorkSurfaceKey();
            workSurfaceKey.WorkSurfaceContext = WorkSurfaceContext.Scheduler;
            var mockWorkSurfaceViewModel = new Mock<IWorkflowDesignerViewModel>();
            var mockedConn = new Mock<IEnvironmentConnection>();
            mockedConn.Setup(conn => conn.ServerEvents).Returns(new Mock<IEventPublisher>().Object);
            var mockEnvironmentModel = new Mock<IEnvironmentModel>();
            mockEnvironmentModel.Setup(model => model.Connection).Returns(mockedConn.Object);
            var environmentModel = mockEnvironmentModel.Object;
            mockWorkSurfaceViewModel.Setup(model => model.EnvironmentModel).Returns(environmentModel);
            var workSurfaceViewModel = mockWorkSurfaceViewModel.As<IWorkSurfaceViewModel>().Object;
            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(workSurfaceKey, workSurfaceViewModel) { DebugOutputViewModel = { DebugStatus = DebugStatus.Executing } };
            //------------Execute Test---------------------------
            workSurfaceContextViewModel.Handle(new DebugOutputMessage(new List<DebugState>()));
            //------------Assert Results-------------------------
            Assert.AreEqual(0, workSurfaceContextViewModel.DebugOutputViewModel.RootItems.Count);
        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("WorkSurfaceContextViewModel_EnvironmentModelIsConnectedChanged")]
        public void WorkSurfaceContextViewModel_EnvironmentModelIsConnectedChanged_True_DebugStatusNotChanged()
        {
            //------------Setup for test--------------------------
            var mockedConn = new Mock<IEnvironmentConnection>();
            mockedConn.Setup(conn => conn.ServerEvents).Returns(new Mock<IEventPublisher>().Object);
            var mockEnvironmentModel = new Mock<IEnvironmentModel>();
            mockEnvironmentModel.Setup(model => model.Connection).Returns(mockedConn.Object);
            var environmentModel = mockEnvironmentModel.Object;
            var workSurfaceContextViewModel = CreateWorkSurfaceContextViewModel(environmentModel);
            var connectedEventArgs = new ConnectedEventArgs { IsConnected = true };
            workSurfaceContextViewModel.DebugOutputViewModel.DebugStatus = DebugStatus.Executing;
            //------------Execute Test---------------------------
            mockEnvironmentModel.Raise(model => model.IsConnectedChanged += null, connectedEventArgs);
            //------------Assert Results-------------------------
            Assert.AreEqual(DebugStatus.Executing, workSurfaceContextViewModel.DebugOutputViewModel.DebugStatus);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("WorkSurfaceContextViewModel_SetDebugStatus")]
        public void WorkSurfaceContextViewModel_SetDebugStatus_StatusConfigure_ClearsDebugOutputViewModel()
        {
            //------------Setup for test--------------------------
            WorkSurfaceContextViewModel workSurfaceContextViewModel = CreateWorkSurfaceContextViewModel();
            var mockDebugState = new Mock<IDebugState>();
            mockDebugState.Setup(state => state.StateType).Returns(StateType.All);
            mockDebugState.Setup(m => m.SessionID).Returns(workSurfaceContextViewModel.DebugOutputViewModel.SessionID);
            workSurfaceContextViewModel.DebugOutputViewModel.Append(mockDebugState.Object);
            //------------Precondition----------------------------
            Assert.AreEqual(1, workSurfaceContextViewModel.DebugOutputViewModel.ContentItemCount);
            //------------Execute Test---------------------------
            workSurfaceContextViewModel.SetDebugStatus(DebugStatus.Configure);
            //------------Assert Results-------------------------
            Assert.AreEqual(0, workSurfaceContextViewModel.DebugOutputViewModel.ContentItemCount);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("WorkSurfaceContextViewModel_SetDebugStatus")]
        public void WorkSurfaceContextViewModel_SetDebugStatus_StatusFinished_DebugStatusFinished()
        {
            //------------Setup for test--------------------------
            WorkSurfaceContextViewModel workSurfaceContextViewModel = CreateWorkSurfaceContextViewModel();
            workSurfaceContextViewModel.DebugOutputViewModel.DebugStatus = DebugStatus.Executing;
            //------------Execute Test---------------------------
            workSurfaceContextViewModel.SetDebugStatus(DebugStatus.Finished);
            //------------Assert Results-------------------------
            Assert.AreEqual(DebugStatus.Finished, workSurfaceContextViewModel.DebugOutputViewModel.DebugStatus);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("WorkSurfaceContextViewModel_IsEnvironmentConnected")]
        public void WorkSurfaceContextViewModel_IsEnvironmentConnected_WhenEnvironmentConnected_ReturnsTrue()
        {
            //------------Setup for test--------------------------
            var mockedConn = new Mock<IEnvironmentConnection>();
            mockedConn.Setup(conn => conn.ServerEvents).Returns(new Mock<IEventPublisher>().Object);

            var mockEnvironmentModel = new Mock<IEnvironmentModel>();
            mockEnvironmentModel.Setup(model => model.Connection).Returns(mockedConn.Object);
            mockEnvironmentModel.Setup(model => model.IsConnected).Returns(true);
            var environmentModel = mockEnvironmentModel.Object;
            var workSurfaceContextViewModel = CreateWorkSurfaceContextViewModel(environmentModel);

            //------------Execute Test---------------------------
            var isEnvironmentConnected = workSurfaceContextViewModel.IsEnvironmentConnected();
            //------------Assert Results-------------------------
            Assert.IsTrue(isEnvironmentConnected);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("WorkSurfaceContextViewModel_IsEnvironmentConnected")]
        public void WorkSurfaceContextViewModel_IsEnvironmentConnected_WhenEnvironmentNotConnected_ReturnsFalse()
        {
            //------------Setup for test--------------------------
            var mockedConn = new Mock<IEnvironmentConnection>();
            mockedConn.Setup(conn => conn.ServerEvents).Returns(new Mock<IEventPublisher>().Object);

            var mockEnvironmentModel = new Mock<IEnvironmentModel>();
            mockEnvironmentModel.Setup(model => model.Connection).Returns(mockedConn.Object);
            mockEnvironmentModel.Setup(model => model.IsConnected).Returns(false);
            var environmentModel = mockEnvironmentModel.Object;
            var workSurfaceContextViewModel = CreateWorkSurfaceContextViewModel(environmentModel);

            //------------Execute Test---------------------------
            var isEnvironmentConnected = workSurfaceContextViewModel.IsEnvironmentConnected();
            //------------Assert Results-------------------------
            Assert.IsFalse(isEnvironmentConnected);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("WorkSurfaceContextViewModel_IsEnvironmentConnected")]
        public void WorkSurfaceContextViewModel_IsEnvironmentConnected_WhenEnvironmentNull_ReturnsFalse()
        {
            //------------Setup for test--------------------------
            var workSurfaceContextViewModel = CreateWorkSurfaceContextViewModel(null);

            //------------Execute Test---------------------------
            var isEnvironmentConnected = workSurfaceContextViewModel.IsEnvironmentConnected();
            //------------Assert Results-------------------------
            Assert.IsFalse(isEnvironmentConnected);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("WorkSurfaceContextViewModel_HandleUpdateDisplayName")]
        public void WorkSurfaceContextViewModel_HandleUpdateDisplayName_NewName_ContextualResourceModelNameChanged()
        {
            var WorksurfaceResourceID = Guid.NewGuid();
            const string newName = "RenamedResource";
            string actualNewName = null;
            var mockResourceModel = new Mock<IContextualResourceModel>();
            mockResourceModel.Setup(model => model.ID).Returns(WorksurfaceResourceID);
            mockResourceModel.SetupSet(model => model.ResourceName = It.IsAny<string>()).Callback<string>(value => { actualNewName = value; });
            var workSurfaceContextViewModel = CreateWorkSurfaceContextViewModel(null, mockResourceModel);
            //------------Execute Test---------------------------
            workSurfaceContextViewModel.Handle(new UpdateWorksurfaceDisplayName(WorksurfaceResourceID, "Worksurface Resource Name", newName));

            // Assert ContextualResourceModel Name Changed
            Assert.IsNotNull(actualNewName);
            Assert.AreEqual(actualNewName, newName, "Tab title not updated");
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("WorkSurfaceContextViewModel_BindToModel")]
        public void WorkSurfaceContextViewModel_BindToModel_CallsBindToModelOnWorkSurfaceViewModel()
        {
            //------------Setup for test--------------------------
            CompositionInitializer.InitializeForMeflessBaseViewModel();
            var workSurfaceKey = new WorkSurfaceKey();
            var mockWorkSurfaceViewModel = new Mock<IWorkflowDesignerViewModel>();
            var mockedConn = new Mock<IEnvironmentConnection>();
            mockedConn.Setup(conn => conn.ServerEvents).Returns(new Mock<IEventPublisher>().Object);
            var mockEnvironmentModel = new Mock<IEnvironmentModel>();
            mockEnvironmentModel.Setup(model => model.Connection).Returns(mockedConn.Object);
            var environmentModel = mockEnvironmentModel.Object;
            mockWorkSurfaceViewModel.Setup(model => model.EnvironmentModel).Returns(environmentModel);
            mockWorkSurfaceViewModel.Setup(m => m.BindToModel()).Verifiable();
            var workSurfaceViewModel = mockWorkSurfaceViewModel.As<IWorkSurfaceViewModel>();
            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(workSurfaceKey, workSurfaceViewModel.Object);
            //------------Execute Test---------------------------
            workSurfaceContextViewModel.BindToModel();
            //------------Assert---------------------------------
            mockWorkSurfaceViewModel.Verify(m => m.BindToModel(), Times.Once());
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("WorkSurfaceContextViewModel_Debug")]
        public void WorkSurfaceContextViewModel_Debug_CallsBindToModelOnWorkSurfaceViewModel()
        {
            //------------Setup for test--------------------------
            CompositionInitializer.InitializeForMeflessBaseViewModel();
            var workSurfaceKey = new WorkSurfaceKey();
            var mockWorkSurfaceViewModel = new Mock<IWorkflowDesignerViewModel>();
            var mockedConn = new Mock<IEnvironmentConnection>();
            mockedConn.Setup(conn => conn.ServerEvents).Returns(new Mock<IEventPublisher>().Object);
            var mockEnvironmentModel = new Mock<IEnvironmentModel>();
            mockEnvironmentModel.Setup(model => model.Connection).Returns(mockedConn.Object);
            mockEnvironmentModel.SetupGet(p => p.IsConnected).Returns(true);
            var mockRepository = new Mock<IResourceRepository>();
            mockRepository.Setup(m => m.Save(It.IsAny<IResourceModel>())).Verifiable();
            mockEnvironmentModel.SetupGet(p => p.ResourceRepository).Returns(mockRepository.Object);
            var environmentModel = mockEnvironmentModel.Object;
            mockWorkSurfaceViewModel.Setup(model => model.EnvironmentModel).Returns(environmentModel);
            mockWorkSurfaceViewModel.Setup(m => m.BindToModel()).Verifiable();
            var workSurfaceViewModel = mockWorkSurfaceViewModel.As<IWorkSurfaceViewModel>();
            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(workSurfaceKey, workSurfaceViewModel.Object);
            var mockResourceModel = new Mock<IContextualResourceModel>();
            mockResourceModel.SetupGet(p => p.Environment).Returns(environmentModel);
            mockResourceModel.Setup(m => m.UserPermissions).Returns(Permissions.Contribute);

            //------------Execute Test---------------------------
            workSurfaceContextViewModel.Debug(mockResourceModel.Object, true);
            //------------Assert---------------------------------
            mockWorkSurfaceViewModel.Verify(m => m.BindToModel(), Times.Once());
            mockRepository.Verify(m => m.Save(It.IsAny<IResourceModel>()), Times.Once());
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("WorkSurfaceContextViewModel_GetServiceInputDataFromUser")]
        public void WorkSurfaceContextViewModel_GetServiceInputDataFromUser()
        {
            //------------Setup for test--------------------------
            CompositionInitializer.InitializeForMeflessBaseViewModel();
            var mockWorkSurfaceViewModel = new Mock<IWorkflowDesignerViewModel>();
            var mockedConn = new Mock<IEnvironmentConnection>();
            mockedConn.Setup(conn => conn.ServerEvents).Returns(new Mock<IEventPublisher>().Object);
            var mockEnvironmentModel = new Mock<IEnvironmentModel>();
            mockEnvironmentModel.Setup(model => model.Connection).Returns(mockedConn.Object);
            var environmentModel = mockEnvironmentModel.Object;
            mockWorkSurfaceViewModel.Setup(model => model.EnvironmentModel).Returns(environmentModel);
            mockWorkSurfaceViewModel.Setup(m => m.BindToModel()).Verifiable();
            //workSurfaceContextViewModel.WorkSurfaceViewModel = new Mock<IWorkSurfaceViewModel>().Object;
            //------------Execute Test---------------------------
            var mockServiceDebugInfoModel = new Mock<IServiceDebugInfoModel>();
            mockServiceDebugInfoModel.SetupGet(p => p.ServiceInputData).Returns(It.IsAny<string>);
            mockServiceDebugInfoModel.SetupGet(p => p.RememberInputs).Returns(It.IsAny<bool>);
            var mockResourceModel = new Mock<IContextualResourceModel>();
            mockResourceModel.SetupGet(p => p.DataList).Returns(It.IsAny<string>);
            mockResourceModel.SetupGet(p => p.ResourceName).Returns(It.IsAny<string>);
            mockResourceModel.SetupGet(p => p.WorkflowXaml).Returns(It.IsAny<StringBuilder>);
            mockResourceModel.SetupGet(p => p.ID).Returns(It.IsAny<Guid>);
            mockResourceModel.SetupGet(p => p.ServerID).Returns(It.IsAny<Guid>);

            mockServiceDebugInfoModel.SetupGet(p => p.ResourceModel).Returns(mockResourceModel.Object);
            //------------Assert---------------------------------
            //mockWorkSurfaceViewModel.Verify(m => m.BindToModel(), Times.Once());
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("WorkSurfaceContextViewModel_Handle")]
        public void WorkSurfaceContextViewModel_Handle_DebugResourceMessage_CallsBindModelAndSave()
        {
            //------------Setup for test--------------------------
            CompositionInitializer.InitializeForMeflessBaseViewModel();
            var workSurfaceKey = new WorkSurfaceKey();
            var mockWorkSurfaceViewModel = new Mock<IWorkflowDesignerViewModel>();
            var mockedConn = new Mock<IEnvironmentConnection>();
            mockedConn.Setup(conn => conn.ServerEvents).Returns(new Mock<IEventPublisher>().Object);
            var mockEnvironmentModel = new Mock<IEnvironmentModel>();
            mockEnvironmentModel.Setup(model => model.Connection).Returns(mockedConn.Object);
            mockEnvironmentModel.SetupGet(p => p.IsConnected).Returns(true);
            var mockRepository = new Mock<IResourceRepository>();
            mockRepository.Setup(m => m.Save(It.IsAny<IResourceModel>())).Verifiable();
            mockEnvironmentModel.SetupGet(p => p.ResourceRepository).Returns(mockRepository.Object);
            var environmentModel = mockEnvironmentModel.Object;
            mockWorkSurfaceViewModel.Setup(model => model.EnvironmentModel).Returns(environmentModel);
            mockWorkSurfaceViewModel.Setup(m => m.BindToModel()).Verifiable();
            var workSurfaceViewModel = mockWorkSurfaceViewModel.As<IWorkSurfaceViewModel>();
            var mockResourceModel = new Mock<IContextualResourceModel>();
            mockResourceModel.SetupGet(p => p.Environment).Returns(environmentModel);
            mockResourceModel.Setup(m => m.UserPermissions).Returns(Permissions.Contribute);
            mockWorkSurfaceViewModel.Setup(model => model.ResourceModel).Returns(mockResourceModel.Object);
            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(workSurfaceKey, workSurfaceViewModel.Object);
            //------------Execute Test---------------------------
            workSurfaceContextViewModel.Handle(new DebugResourceMessage(mockResourceModel.Object));
            //------------Assert---------------------------------
            mockWorkSurfaceViewModel.Verify(m => m.BindToModel(), Times.Once());
            mockRepository.Verify(m => m.Save(It.IsAny<IResourceModel>()), Times.Once());
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("WorkSurfaceContextViewModel_Handle")]
        public void WorkSurfaceContextViewModel_Handle_ExecuteResourceMessage_CallsBindModelAndSave()
        {
            //------------Setup for test--------------------------
            CompositionInitializer.InitializeForMeflessBaseViewModel();
            var workSurfaceKey = new WorkSurfaceKey();
            var mockWorkSurfaceViewModel = new Mock<IWorkflowDesignerViewModel>();
            var mockedConn = new Mock<IEnvironmentConnection>();
            mockedConn.Setup(conn => conn.ServerEvents).Returns(new Mock<IEventPublisher>().Object);
            var mockEnvironmentModel = new Mock<IEnvironmentModel>();
            mockEnvironmentModel.Setup(model => model.Connection).Returns(mockedConn.Object);
            mockEnvironmentModel.SetupGet(p => p.IsConnected).Returns(true);
            var mockRepository = new Mock<IResourceRepository>();
            mockRepository.Setup(m => m.Save(It.IsAny<IResourceModel>())).Verifiable();
            mockEnvironmentModel.SetupGet(p => p.ResourceRepository).Returns(mockRepository.Object);
            var environmentModel = mockEnvironmentModel.Object;
            mockWorkSurfaceViewModel.Setup(model => model.EnvironmentModel).Returns(environmentModel);
            mockWorkSurfaceViewModel.Setup(m => m.BindToModel()).Verifiable();
            var workSurfaceViewModel = mockWorkSurfaceViewModel.As<IWorkSurfaceViewModel>();
            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(workSurfaceKey, workSurfaceViewModel.Object);
            var mockResourceModel = new Mock<IContextualResourceModel>();
            mockResourceModel.SetupGet(p => p.Environment).Returns(environmentModel);
            mockResourceModel.Setup(m => m.UserPermissions).Returns(Permissions.Contribute);
            //------------Execute Test---------------------------
            workSurfaceContextViewModel.Handle(new ExecuteResourceMessage(mockResourceModel.Object));
            //------------Assert---------------------------------
            mockWorkSurfaceViewModel.Verify(m => m.BindToModel(), Times.Once());
            mockRepository.Verify(m => m.Save(It.IsAny<IResourceModel>()), Times.Once());
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("WorkSurfaceContextViewModel_Handle")]
        public void WorkSurfaceContextViewModel_Handle_SaveResourceMessage_CallsBindModelAndSave()
        {
            //------------Setup for test--------------------------
            CompositionInitializer.InitializeForMeflessBaseViewModel();
            var workSurfaceKey = new WorkSurfaceKey();
            var mockWorkSurfaceViewModel = new Mock<IWorkflowDesignerViewModel>();
            var mockedConn = new Mock<IEnvironmentConnection>();
            mockedConn.Setup(conn => conn.ServerEvents).Returns(new Mock<IEventPublisher>().Object);
            var mockEnvironmentModel = new Mock<IEnvironmentModel>();
            mockEnvironmentModel.Setup(model => model.Connection).Returns(mockedConn.Object);
            mockEnvironmentModel.SetupGet(p => p.IsConnected).Returns(true);
            var mockRepository = new Mock<IResourceRepository>();
            mockRepository.Setup(c => c.FetchResourceDefinition(It.IsAny<IEnvironmentModel>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new ExecuteMessage());
            mockRepository.Setup(m => m.Save(It.IsAny<IResourceModel>())).Verifiable();
            mockRepository.Setup(c => c.SaveToServer(It.IsAny<IResourceModel>())).Returns(new ExecuteMessage());
            mockEnvironmentModel.SetupGet(p => p.ResourceRepository).Returns(mockRepository.Object);
            var environmentModel = mockEnvironmentModel.Object;
            mockWorkSurfaceViewModel.Setup(model => model.EnvironmentModel).Returns(environmentModel);
            mockWorkSurfaceViewModel.Setup(m => m.BindToModel()).Verifiable();
            var workSurfaceViewModel = mockWorkSurfaceViewModel.As<IWorkSurfaceViewModel>();
            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(workSurfaceKey, workSurfaceViewModel.Object);
            var mockResourceModel = new Mock<IContextualResourceModel>();
            mockResourceModel.SetupGet(p => p.Environment).Returns(environmentModel);
            mockResourceModel.Setup(m => m.UserPermissions).Returns(Permissions.Contribute);

            //------------Execute Test---------------------------
            workSurfaceContextViewModel.Handle(new SaveResourceMessage(mockResourceModel.Object, false, false));
            //------------Assert---------------------------------
            mockWorkSurfaceViewModel.Verify(m => m.BindToModel(), Times.Once());
            mockRepository.Verify(m => m.Save(It.IsAny<IResourceModel>()), Times.Once());
        }

        static WorkSurfaceContextViewModel CreateWorkSurfaceContextViewModel(IEnvironmentModel environmentModel, Mock<IContextualResourceModel> ResourceModel = null)
        {
            CompositionInitializer.InitializeForMeflessBaseViewModel();
            var workSurfaceKey = new WorkSurfaceKey();
            var mockWorkSurfaceViewModel = new Mock<IWorkflowDesignerViewModel>();
            var mockResourceModel = ResourceModel ?? new Mock<IContextualResourceModel>();
            mockResourceModel.Setup(model => model.Environment).Returns(environmentModel);
            mockWorkSurfaceViewModel.Setup(model => model.EnvironmentModel).Returns(environmentModel);
            mockWorkSurfaceViewModel.Setup(model => model.ResourceModel).Returns(mockResourceModel.Object);
            var workSurfaceViewModel = mockWorkSurfaceViewModel.As<IWorkSurfaceViewModel>().Object;
            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(workSurfaceKey, workSurfaceViewModel);
            return workSurfaceContextViewModel;
        }

        static WorkSurfaceContextViewModel CreateWorkSurfaceContextViewModel()
        {
            var mockedConn = new Mock<IEnvironmentConnection>();
            mockedConn.Setup(conn => conn.ServerEvents).Returns(new Mock<IEventPublisher>().Object);

            var mockEnvironmentModel = new Mock<IEnvironmentModel>();
            mockEnvironmentModel.Setup(model => model.Connection).Returns(mockedConn.Object);

            var environmentModel = mockEnvironmentModel.Object;
            return CreateWorkSurfaceContextViewModel(environmentModel);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("WorkSurfaceContextViewModel_DebugCommand")]
        public void WorkSurfaceContextViewModel_DebugCommand_UserHasAdministratorPermissions_CanExecuteIsTrue()
        {
            Verify_DebugCommand_CanExecute(Permissions.Administrator, true);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("WorkSurfaceContextViewModel_DebugCommand")]
        public void WorkSurfaceContextViewModel_DebugCommand_UserHasContributePermissions_CanExecuteIsTrue()
        {
            Verify_DebugCommand_CanExecute(Permissions.Contribute, true);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("WorkSurfaceContextViewModel_DebugCommand")]
        public void WorkSurfaceContextViewModel_DebugCommand_UserHasViewAndExecutePermissions_CanExecuteIsTrue()
        {
            Verify_DebugCommand_CanExecute(Permissions.View | Permissions.Execute, true);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("WorkSurfaceContextViewModel_DebugCommand")]
        public void WorkSurfaceContextViewModel_DebugCommand_UserHasViewPermissions_CanExecuteIsFalse()
        {
            Verify_DebugCommand_CanExecute(Permissions.View, false);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("WorkSurfaceContextViewModel_DebugCommand")]
        public void WorkSurfaceContextViewModel_DebugCommand_UserHasExecutePermissions_CanExecuteIsFalse()
        {
            Verify_DebugCommand_CanExecute(Permissions.Execute, false);
        }

        void Verify_DebugCommand_CanExecute(Permissions userPermissions, bool expected)
        {
            //------------Setup for test--------------------------
            var workSurfaceContextViewModel = CreateWorkSurfaceContextViewModel(userPermissions);

            //------------Execute Test---------------------------
            var actualDebug = workSurfaceContextViewModel.DebugCommand.CanExecute(null);
            var actualQuickDebug = workSurfaceContextViewModel.QuickDebugCommand.CanExecute(null);

            //------------Assert Results-------------------------
            Assert.AreEqual(expected, actualDebug);
            Assert.AreEqual(expected, actualQuickDebug);
        }

        static WorkSurfaceContextViewModel CreateWorkSurfaceContextViewModel(Permissions userPermissions)
        {
            CompositionInitializer.InitializeForMeflessBaseViewModel();

            var mockedConn = new Mock<IEnvironmentConnection>();
            mockedConn.Setup(conn => conn.ServerEvents).Returns(new Mock<IEventPublisher>().Object);

            var authService = new Mock<IAuthorizationService>();
            authService.Setup(s => s.IsAuthorized(It.IsAny<AuthorizationContext>(), It.IsAny<string>())).Returns(true);

            var mockEnvironmentModel = new Mock<IEnvironmentModel>();
            mockEnvironmentModel.Setup(model => model.Connection).Returns(mockedConn.Object);
            mockEnvironmentModel.Setup(model => model.IsConnected).Returns(true);
            mockEnvironmentModel.Setup(model => model.AuthorizationService).Returns(authService.Object);

            var environmentModel = mockEnvironmentModel.Object;

            var resourceModel = new ResourceModel(environmentModel)
            {
                ID = Guid.NewGuid(),
                ResourceName = "TestResource" + Guid.NewGuid(),
                UserPermissions = userPermissions
            };

            var mockWorkSurfaceViewModel = new Mock<IWorkflowDesignerViewModel>();
            mockWorkSurfaceViewModel.Setup(model => model.EnvironmentModel).Returns(environmentModel);
            mockWorkSurfaceViewModel.Setup(model => model.ResourceModel).Returns(resourceModel);

            var workSurfaceViewModel = mockWorkSurfaceViewModel.As<IWorkSurfaceViewModel>().Object;

            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(new WorkSurfaceKey(), workSurfaceViewModel)
            {
                DebugOutputViewModel = { DebugStatus = DebugStatus.Ready }
            };

            workSurfaceContextViewModel.DebugCommand.UpdateContext(environmentModel, resourceModel);
            workSurfaceContextViewModel.QuickDebugCommand.UpdateContext(environmentModel, resourceModel);

            return workSurfaceContextViewModel;
        }


        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("WorkSurfaceContextViewModel_DebugCommand")]
        public void WorkSurfaceContextViewModel_DebugCommand_UserHasNoContributePermissions_SaveIsNotInvoked()
        {
            Verify_DebugCommand_SaveIsInvoked(Permissions.Execute, 0);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("WorkSurfaceContextViewModel_DebugCommand")]
        public void WorkSurfaceContextViewModel_DebugCommand_UserHasContributePermissions_SaveIsInvoked()
        {
            Verify_DebugCommand_SaveIsInvoked(Permissions.Contribute, 1);
        }

        void Verify_DebugCommand_SaveIsInvoked(Permissions userPermissions, int saveHitCount)
        {
            //------------Setup for test--------------------------
            var expected = saveHitCount;
            var workSurfaceContextViewModel = CreateWorkSurfaceContextViewModel(userPermissions);

            var resourceRepo = new Mock<IResourceRepository>();
            resourceRepo.Setup(r => r.Save(It.IsAny<IResourceModel>())).Verifiable();

            var environmentModel = Mock.Get(workSurfaceContextViewModel.ContextualResourceModel.Environment);
            var mockConnection = new Mock<IEnvironmentConnection>();
            mockConnection.Setup(connection => connection.IsConnected).Returns(true);
            environmentModel.Setup(model => model.Connection).Returns(mockConnection.Object);
            environmentModel.Setup(model => model.ResourceRepository).Returns(resourceRepo.Object);


            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            workSurfaceContextViewModel.DebugCommand.Execute(null);
            resourceRepo.Verify(r => r.Save(It.IsAny<IResourceModel>()), Times.Exactly(expected));

            expected += saveHitCount;

            workSurfaceContextViewModel.QuickDebugCommand.Execute(null);
            resourceRepo.Verify(r => r.Save(It.IsAny<IResourceModel>()), Times.Exactly(expected));
        }
    }

    public class TestWorkSurfaceContextViewModel : WorkSurfaceContextViewModel
    {
        public TestWorkSurfaceContextViewModel(WorkSurfaceKey workSurfaceKey, IWorkSurfaceViewModel workSurfaceViewModel)
            : base(workSurfaceKey, workSurfaceViewModel)
        {
        }

        public TestWorkSurfaceContextViewModel(IEventAggregator eventPublisher, WorkSurfaceKey workSurfaceKey, IWorkSurfaceViewModel workSurfaceViewModel)
            : base(eventPublisher, workSurfaceKey, workSurfaceViewModel)
        {
        }

        public int SaveHitCount { get; private set; }

        protected override bool Save(IContextualResourceModel resource, bool isLocalSave, bool addToTabManager = true, bool isStudioShutdown = false)
        {
            SaveHitCount++;
            return true;
        }

    }
}
