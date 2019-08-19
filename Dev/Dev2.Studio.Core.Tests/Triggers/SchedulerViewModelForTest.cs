using Caliburn.Micro;
using CubicOrange.Windows.Forms.ActiveDirectory;
using Dev2.Common.Interfaces.Scheduler.Interfaces;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Common.Interfaces.Threading;
using Dev2.Dialogs;
using Dev2.Studio.Interfaces;
using Dev2.TaskScheduler.Wrappers;
using Dev2.Triggers.Scheduler;
using Microsoft.Win32.TaskScheduler;
using Moq;
using System;
using System.Threading.Tasks;

namespace Dev2.Core.Tests.Triggers
{
    class DummySchedulerViewModelForTest : SchedulerViewModel
    {
        public DummySchedulerViewModelForTest(IEventAggregator eventPublisher, DirectoryObjectPickerDialog directoryObjectPicker, IPopupController popupController, IAsyncWorker asyncWorker, IServer server, Func<IServer, IServer> toEnvironmentModel, Task<IResourcePickerDialog> getResourcePicker)
                                            : base(eventPublisher, directoryObjectPicker, popupController, asyncWorker, server, toEnvironmentModel, getResourcePicker)
        {
            SelectedTask = new Mock<IScheduledResource>().Object;
        }
    }

    class MySchedulerTaskManager : SchedulerTaskManager
    {
        internal MySchedulerTaskManager(IServer server, SchedulerViewModel vm, Task<IResourcePickerDialog> resourcePickerDialog)
                                 : this(server, vm, resourcePickerDialog, new Mock<IShellViewModel>().Object)
        {
        }
        internal MySchedulerTaskManager(IServer server, SchedulerViewModel vm, Task<IResourcePickerDialog> resourcePickerDialog, IShellViewModel shellVm)
                                 : base(vm, resourcePickerDialog)
        {

        }


    }

    class SchedulerViewModelForTest : SchedulerViewModel
    {
        public SchedulerViewModelForTest(IServer env) : base(a => env)
        {
            SchedulerTaskManager = new ScheduleTaskManagerStub(env, this, System.Threading.Tasks.Task.FromResult(new Mock<IResourcePickerDialog>().Object));
        }

        public SchedulerViewModelForTest(IServer env, IEventAggregator eventPublisher, DirectoryObjectPickerDialog directoryObjectPicker, IPopupController popupController, IAsyncWorker asyncWorker)
            : base(eventPublisher, directoryObjectPicker, popupController, asyncWorker, new Mock<IServer>().Object, a => new Mock<IServer>().Object)
        {
            SchedulerTaskManager = new ScheduleTaskManagerStub(env, this, System.Threading.Tasks.Task.FromResult(new Mock<IResourcePickerDialog>().Object));
        }
        #region Overrides of SchedulerViewModel

        public bool GetCredentialsCalled
        {
            get
            {
                var stub = SchedulerTaskManager as ScheduleTaskManagerStub;
                return stub != null && stub.GetCredentialsCalled;
            }
        }

        public bool ShowSaveErrorDialogCalled
        {
            get;
            private set;
        }

        public void CallShowErrorDialog(string error)
        {
            base.ShowSaveErrorDialog(error);
        }
        protected internal override void ShowSaveErrorDialog(string error)
        {
            ShowSaveErrorDialogCalled = true;
        }


        #endregion
    }

    class ScheduleTaskManagerStub : MySchedulerTaskManager
    {
        public ScheduleTaskManagerStub(IServer server, SchedulerViewModel schedulerViewModel, Task<IResourcePickerDialog> getResourcePicker)
            : base(server, schedulerViewModel, getResourcePicker)
        {
        }

        public bool GetCredentialsCalled
        {
            get;
            private set;
        }

        protected override void GetCredentials(IScheduledResource scheduledResource)
        {
            if (string.IsNullOrEmpty(scheduledResource.UserName) || string.IsNullOrEmpty(scheduledResource.Password))
            {
                GetCredentialsCalled = true;
                scheduledResource.UserName = "some username";
                scheduledResource.Password = "some password";
            }
        }

        protected override IScheduleTrigger ShowEditTriggerDialog()
        {
            var dailyTrigger = new DailyTrigger { StartBoundary = new DateTime(2013, 04, 01, 02, 21, 25) };
            return SchedulerFactory.CreateTrigger(TaskState.Disabled, new Dev2Trigger(null, dailyTrigger));
        }
    }

}
