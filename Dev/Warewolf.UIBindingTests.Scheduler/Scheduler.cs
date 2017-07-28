using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.ObjectModel;
using CubicOrange.Windows.Forms.ActiveDirectory;
using Dev2.Common.Interfaces.Scheduler.Interfaces;
using Dev2.Scheduler;
using Dev2.Settings.Scheduler;
using Dev2.Studio.Interfaces;
using Dev2.Threading;
using Moq;
using System.Windows;
using Dev2.Common.Interfaces.Studio.Controller;
using Caliburn.Micro;

namespace Warewolf.UIBindingTests.Scheduler
{
    [TestClass]
    public class Scheduler
    {
        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("SchedulerViewModel_CreateNewTask")]
        public void SchedulerViewModel_CreateNewTask_ShouldAddTaskToListWithDefaultSettings_OnlyAllowOneDirtyTask()
        {
            //------------Setup for test--------------------------
            var popupController = new Mock<IPopupController>();
            var env = new Mock<IServer>();
            env.Setup(a => a.IsConnected).Returns(true);
            var svr = new Mock<IServer>();
            svr.Setup(a => a.IsConnected).Returns(true);
            var schedulerViewModel = new SchedulerViewModel(new Mock<IEventAggregator>().Object, new Mock<DirectoryObjectPickerDialog>().Object, popupController.Object, new SynchronousAsyncWorker(), svr.Object, a => env.Object);
            var resources = new ObservableCollection<IScheduledResource> { new ScheduledResource("bob", SchedulerStatus.Enabled, DateTime.MaxValue, new Mock<IScheduleTrigger>().Object, "c", Guid.NewGuid().ToString()) { NumberOfHistoryToKeep = 1 }, new ScheduledResource("dave", SchedulerStatus.Enabled, DateTime.MaxValue, new Mock<IScheduleTrigger>().Object, "c", Guid.NewGuid().ToString()) };

            var resourceModel = new Mock<IScheduledResourceModel>();
            resourceModel.Setup(c => c.ScheduledResources).Returns(resources);
            schedulerViewModel.ScheduledResourceModel = resourceModel.Object;
            schedulerViewModel.Server = svr.Object;
            if (Application.Current != null)
            {
                Application.Current.Shutdown();
            }
            //------------Execute Test---------------------------

            Assert.AreEqual(2, schedulerViewModel.TaskList.Count);

            schedulerViewModel.NewCommand.Execute(null);
            Assert.AreEqual(3, schedulerViewModel.TaskList.Count);
            Assert.AreEqual("New Task1", schedulerViewModel.TaskList[1].Name);
            Assert.AreEqual("New Task1", schedulerViewModel.TaskList[1].OldName);
            Assert.IsTrue(schedulerViewModel.TaskList[1].IsDirty);
            Assert.AreEqual(SchedulerStatus.Enabled, schedulerViewModel.TaskList[1].Status);
            Assert.AreEqual(string.Empty, schedulerViewModel.TaskList[1].WorkflowName);
            Assert.AreEqual(schedulerViewModel.SelectedTask, schedulerViewModel.TaskList[1]);

            schedulerViewModel.NewCommand.Execute(null);
            //------------Assert Results-------------------------
            Assert.AreEqual(3, schedulerViewModel.TaskList.Count);
            popupController.Verify(a => a.Show("Please save currently edited Task(s) before creating a new one.", "Save before continuing", MessageBoxButton.OK, MessageBoxImage.Error, null, false, true, false, false, false, false));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("SchedulerViewModel_CreateNewTask")]
        public void SchedulerViewModel_CreateNewTask_ServerDown_ShouldShowPopup()
        {
            //------------Setup for test--------------------------
            var popupController = new Mock<IPopupController>();
            popupController.Setup(c => c.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>(), "", false, true, false, false, false, false)).Returns(MessageBoxResult.OK).Verifiable();
            var env = new Mock<IServer>();
            env.Setup(a => a.IsConnected).Returns(true);
            var svr = new Mock<IServer>();
            svr.Setup(a => a.IsConnected).Returns(true);
            var mockConnection = new Mock<IEnvironmentConnection>();
            mockConnection.Setup(connection => connection.DisplayName).Returns("localhost");
            env.Setup(a => a.Connection).Returns(mockConnection.Object);

            var schedulerViewModel = new SchedulerViewModel(new Mock<IEventAggregator>().Object, new Mock<DirectoryObjectPickerDialog>().Object, popupController.Object, new SynchronousAsyncWorker(), svr.Object, a => env.Object);
            var resources = new ObservableCollection<IScheduledResource> { new ScheduledResource("bob", SchedulerStatus.Enabled, DateTime.MaxValue, new Mock<IScheduleTrigger>().Object, "c", Guid.NewGuid().ToString()) { NumberOfHistoryToKeep = 1 }, new ScheduledResource("dave", SchedulerStatus.Enabled, DateTime.MaxValue, new Mock<IScheduleTrigger>().Object, "c", Guid.NewGuid().ToString()) };
            schedulerViewModel.CurrentEnvironment = env.Object;
            var resourceModel = new Mock<IScheduledResourceModel>();
            resourceModel.Setup(c => c.ScheduledResources).Returns(resources);
            schedulerViewModel.ScheduledResourceModel = resourceModel.Object;
            schedulerViewModel.Server = svr.Object;
            if (Application.Current != null)
            {
                Application.Current.Shutdown();
            }
            //------------Execute Test---------------------------

            Assert.AreEqual(2, schedulerViewModel.TaskList.Count);

            env.Setup(a => a.IsConnected).Returns(false);

            schedulerViewModel.CurrentEnvironment = env.Object;

            schedulerViewModel.NewCommand.Execute(null);
            //------------Assert Results-------------------------
            popupController.Verify(a => a.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>(), "", false, true, false, false, false, false));
        }
    }
}
