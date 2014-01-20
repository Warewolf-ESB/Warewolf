using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Dev2.Communication;
using Dev2.Diagnostics;
using Dev2.Messages;
using Dev2.Providers.Events;
using Dev2.Studio.AppResources.Comparers;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.ViewModels;
using Dev2.Studio.ViewModels.Diagnostics;
using Dev2.Studio.ViewModels.Workflow;
using Dev2.Studio.ViewModels.WorkSurface;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
// ReSharper disable InconsistentNaming
namespace Dev2.Core.Tests.ViewModelTests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class WorkSurfaceContextViewModelTests
    {
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("WorkSurfaceContextViewModel_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WorkSurfaceContextViewModel_Constructor_NullWorkSurfaceKey_ExpectException()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(null, new Mock<IWorkSurfaceViewModel>().Object);
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
            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(new WorkSurfaceKey(), null);
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
            var connectedEventArgs = new ConnectedEventArgs();
            connectedEventArgs.IsConnected = false;
            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(workSurfaceKey, workSurfaceViewModel);
            workSurfaceContextViewModel.DebugOutputViewModel.DebugStatus = DebugStatus.Executing;
            //------------Execute Test---------------------------
            mockEnvironmentModel.Raise(model => model.IsConnectedChanged += null, connectedEventArgs);
            //------------Assert Results-------------------------
            Assert.AreEqual(DebugStatus.Finished, workSurfaceContextViewModel.DebugOutputViewModel.DebugStatus);
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
            var connectedEventArgs = new ConnectedEventArgs();
            connectedEventArgs.IsConnected = true;
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
            var newName = "RenamedResource";
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
            var inputDataViewModel = new WorkflowInputDataViewModel(mockServiceDebugInfoModel.Object, workSurfaceContextViewModel.DebugOutputViewModel.SessionID) { Parent = workSurfaceContextViewModel };
            WorkflowInputDataViewModel temp = inputDataViewModel;
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
            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(workSurfaceKey, workSurfaceViewModel.Object);
            var mockResourceModel = new Mock<IContextualResourceModel>();
            mockResourceModel.SetupGet(p => p.Environment).Returns(environmentModel);
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
    }
}
