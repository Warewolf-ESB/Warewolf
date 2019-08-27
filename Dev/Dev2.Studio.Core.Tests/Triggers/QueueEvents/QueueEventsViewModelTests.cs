/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Data.TO;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Common.Interfaces.Threading;
using Dev2.ConnectionHelpers;
using Dev2.Core.Tests.Environments;
using Dev2.Dialogs;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Studio.Interfaces;
using Dev2.Threading;
using Dev2.Triggers;
using Dev2.Triggers.QueueEvents;
using Dev2.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using Warewolf.Options;
using Warewolf.Trigger;

namespace Dev2.Core.Tests.Triggers.QueueEvents
{
    [TestClass]
    [TestCategory("Studio Triggers Queue Core")]
    public class QueueEventsViewModelTests
    {
        [TestInitialize]
        public void SetupForTest()
        {
            AppUsageStats.LocalHost = "http://localhost:3142";
            var shell = new Mock<IShellViewModel>();
            var lcl = new Mock<IServer>();
            lcl.Setup(a => a.DisplayName).Returns("Localhost");
            shell.Setup(x => x.LocalhostServer).Returns(lcl.Object);
            shell.Setup(x => x.ActiveServer).Returns(new Mock<IServer>().Object);
            var connectControlSingleton = new Mock<IConnectControlSingleton>();
            var explorerTooltips = new Mock<IExplorerTooltips>();

            CustomContainer.Register(shell.Object);
            CustomContainer.Register(new Mock<Microsoft.Practices.Prism.PubSubEvents.IEventAggregator>().Object);
            CustomContainer.Register(connectControlSingleton.Object);
            CustomContainer.Register(explorerTooltips.Object);

            var targetEnv = EnviromentRepositoryTest.CreateMockEnvironment(EnviromentRepositoryTest.Server1Source);
            var serverRepo = new Mock<IServerRepository>();
            serverRepo.Setup(r => r.All()).Returns(new[] { targetEnv.Object });
            CustomContainer.Register(serverRepo.Object);
        }

        QueueEventsViewModel CreateViewModel()
        {
            var mockServer = SetupForTriggerQueueView(null);
            var mockExternalExecutor = new Mock<IExternalProcessExecutor>();
            
            var mockResourcePickerDialog = new Mock<IResourcePickerDialog>();
            return new QueueEventsViewModel(mockServer.Object, mockExternalExecutor.Object, mockResourcePickerDialog.Object);
        }

        private static Mock<IServer> SetupForTriggerQueueView(Resource resource)
        {
            var mockServer = new Mock<IServer>();

            List<IOption> expectedOptions = SetupOptionsView();

            var mockResourceRepository = new Mock<IResourceRepository>();
            if (resource == null)
            {
                var queueSource = new Mock<IResource>();
                mockResourceRepository.Setup(resourceRepository => resourceRepository.FindOptions(mockServer.Object, queueSource.Object)).Returns(expectedOptions);
            }
            else
            {
                mockResourceRepository.Setup(resourceRepository => resourceRepository.FindOptions(mockServer.Object, resource)).Returns(expectedOptions);
            }

            mockServer.Setup(server => server.ResourceRepository).Returns(mockResourceRepository.Object);
            return mockServer;
        }

        private static List<IOption> SetupOptionsView()
        {
            var expectedOptionBool = new OptionBool
            {
                Name = "bool",
                Value = false
            };
            var expectedOptionInt = new OptionInt
            {
                Name = "int",
                Value = 10
            };
            var expectedOptionAutocompletebox = new OptionAutocomplete
            {
                Name = "auto",
                Value = "new text"
            };
            var expectedOptions = new List<IOption>
            {
                expectedOptionBool,
                expectedOptionInt,
                expectedOptionAutocompletebox
            };
            return expectedOptions;
        }

        [TestMethod]
        [TestCategory(nameof(QueueEventsViewModel))]
        [Owner("Pieter Terblanche")]
        public void QueueEventsViewModel_Queues()
        {
            Mock<IServer> mockServer = SetupForTriggerQueueView(null);
            var queueEventsViewModel = new QueueEventsViewModel(mockServer.Object);

            Assert.IsNotNull(queueEventsViewModel.Queues);
            Assert.AreEqual(1, queueEventsViewModel.Queues.Count);
            Assert.IsNull(queueEventsViewModel.SelectedQueue);

            var queue1 = new TriggerQueueViewForTesting(mockServer.Object);
            var queue2 = new TriggerQueueViewForTesting(mockServer.Object);

            queueEventsViewModel.Queues = new ObservableCollection<TriggerQueueView>()
            {
                queue1,
                queue2
            };

            queueEventsViewModel.SelectedQueue = queue2;

            Assert.IsNotNull(queueEventsViewModel.Queues);
            Assert.AreEqual(2, queueEventsViewModel.Queues.Count);

            Assert.IsNotNull(queueEventsViewModel.SelectedQueue);
            Assert.AreEqual(queue2, queueEventsViewModel.SelectedQueue);
        }

        [TestMethod]
        [TestCategory(nameof(QueueEventsViewModel))]
        [Owner("Pieter Terblanche")]
        public void QueueEventsViewModel_QueueEvents_AddNew_ShouldAddNewItem()
        {
            var queueEventsViewModel = CreateViewModel();

            queueEventsViewModel.NewCommand.Execute(null);

            Assert.AreEqual(2, queueEventsViewModel.Queues.Count);
            //TODO: Verify that one item is a new item and the other is the "Create New" item
        }

        [TestMethod]
        [TestCategory(nameof(QueueEventsViewModel))]
        [Owner("Pieter Terblanche")]
        public void QueueEventsViewModel_QueueEvents_Delete_ShouldDeleteSelectedItem()
        {
            Mock<IServer> mockServer = SetupForTriggerQueueView(null);
            var queueEventsViewModel = CreateViewModel();
            var triggerQueueView = new TriggerQueueViewForTesting(mockServer.Object);

            queueEventsViewModel.Queues.Add(triggerQueueView);
            queueEventsViewModel.SelectedQueue = triggerQueueView;

            Assert.AreEqual(2, queueEventsViewModel.Queues.Count);

            queueEventsViewModel.DeleteCommand.Execute(null);
            Assert.AreEqual(1, queueEventsViewModel.Queues.Count);

            //TODO: Verify that one item is the "Create New" item
        }

        [TestMethod]
        [TestCategory(nameof(QueueEventsViewModel))]
        [Owner("Hagashen Naidu")]
        public void QueueEventsViewModel_QueueEvents_AddNew_Should_FirePropertyChangedEventForQueuesProperty()
        {
            var queueEventsViewModel = CreateViewModel();
            var propertyChangedFired = false;
            queueEventsViewModel.Queues.CollectionChanged += (object sender, NotifyCollectionChangedEventArgs e) =>
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    propertyChangedFired = true;
                }
            };

            queueEventsViewModel.NewCommand.Execute(null);

            Assert.IsTrue(propertyChangedFired);
        }

        [TestMethod]
        [TestCategory(nameof(QueueEventsViewModel))]
        [Owner("Pieter Terblanche")]
        public void QueueEventsViewModel_QueueEvents_PasteResponse()
        {
            var mockServer = new Mock<IServer>();
            var queueEventsViewModel = new QueueEventsViewModel(mockServer.Object);

            Assert.IsNull(queueEventsViewModel.PasteResponse);

            queueEventsViewModel.PasteResponse = "Paste Response";

            Assert.AreEqual("Paste Response", queueEventsViewModel.PasteResponse);
        }

        [TestMethod]
        [TestCategory(nameof(QueueEventsViewModel))]
        [Owner("Pieter Terblanche")]
        public void QueueEventsViewModel_QueueEvents_ViewQueueStats()
        {
            Uri uri = new Uri("https://www.rabbitmq.com/blog/tag/statistics/");

            var mockServer = new Mock<IServer>();
            var mockExternalProcessExecutor = new Mock<IExternalProcessExecutor>();
            mockExternalProcessExecutor.Setup(externalProcessExecutor => externalProcessExecutor.OpenInBrowser(uri)).Verifiable();

            var queueEventsViewModel = new QueueEventsViewModel(mockServer.Object, mockExternalProcessExecutor.Object, new Mock<IResourcePickerDialog>().Object);

            queueEventsViewModel.QueueStatsCommand.Execute(null);

            mockExternalProcessExecutor.Verify(externalProcessExecutor => externalProcessExecutor.OpenInBrowser(uri), Times.Once);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(QueueEventsViewModel))]
        public void QueueEventsViewModel_ConnectionError_SetAndClearError_ValidErrorSetAndClear()
        {
            //------------Setup for test--------------------------
            Mock<IServer> mockServer = SetupForTriggerQueueView(null);
            var queueEventsViewModel = new QueueEventsViewModel(mockServer.Object);
            var triggerQueueView = new TriggerQueueViewForTesting(mockServer.Object);
            queueEventsViewModel.SelectedQueue = triggerQueueView;
            //------------Execute Test---------------------------
            queueEventsViewModel.SetConnectionError();
            //------------Assert Results-------------------------
            Assert.AreEqual(Warewolf.Studio.Resources.Languages.Core.QueueConnectionError, queueEventsViewModel.ConnectionError);
            Assert.IsTrue(queueEventsViewModel.HasConnectionError);

            queueEventsViewModel.ClearConnectionError();

            Assert.AreEqual("", queueEventsViewModel.ConnectionError);
            Assert.IsFalse(queueEventsViewModel.HasConnectionError);
        }

        [TestMethod]
        [TestCategory(nameof(QueueEventsViewModel))]
        [Owner("Pieter Terblanche")]
        public void QueueEventsViewModel_QueueEvents_Save_Empty_QueueSource()
        {
            var mockPopupController = new Mock<IPopupController>();
            mockPopupController.Setup(popupController => popupController.Show(Warewolf.Studio.Resources.Languages.Core.TriggerQueuesSaveQueueSourceNotSelected, Warewolf.Studio.Resources.Languages.Core.TriggerQueuesSaveErrorHeader, MessageBoxButton.OK, MessageBoxImage.Error, string.Empty, false, true, false, false, false, false));
            CustomContainer.Register(mockPopupController.Object);

            Mock<IServer> mockServer = SetupForTriggerQueueView(null);
            var queueEventsViewModel = CreateViewModel();
            queueEventsViewModel.SelectedQueue = new TriggerQueueViewForTesting(mockServer.Object);

            var isSaved = queueEventsViewModel.Save();
            Assert.IsFalse(isSaved);

            mockPopupController.Verify(popupController => popupController.Show(Warewolf.Studio.Resources.Languages.Core.TriggerQueuesSaveQueueSourceNotSelected, Warewolf.Studio.Resources.Languages.Core.TriggerQueuesSaveErrorHeader, MessageBoxButton.OK, MessageBoxImage.Error, string.Empty, false, true, false, false, false, false), Times.Once);
        }

        [TestMethod]
        [TestCategory(nameof(QueueEventsViewModel))]
        [Owner("Pieter Terblanche")]
        public void QueueEventsViewModel_QueueEvents_Save_Empty_WorkflowName()
        {
            var resourceId = Guid.NewGuid();
            var resource = new Resource
            {
                ResourceID = resourceId
            };

            var mockPopupController = new Mock<IPopupController>();
            mockPopupController.Setup(popupController => popupController.Show(Warewolf.Studio.Resources.Languages.Core.TriggerQueuesSaveWorkflowNotSelected, Warewolf.Studio.Resources.Languages.Core.TriggerQueuesSaveErrorHeader, MessageBoxButton.OK, MessageBoxImage.Error, string.Empty, false, true, false, false, false, false));
            CustomContainer.Register(mockPopupController.Object);

            Mock<IServer> mockServer = SetupForTriggerQueueView(resource);
            var queueEventsViewModel = CreateViewModel();
            queueEventsViewModel.SelectedQueue = new TriggerQueueViewForTesting(mockServer.Object)
            {
                SelectedQueueSource = resource
            };
            queueEventsViewModel.SelectedQueue.QueueSourceId = resourceId;

            var isSaved = queueEventsViewModel.Save();
            Assert.IsFalse(isSaved);

            mockPopupController.Verify(popupController => popupController.Show(Warewolf.Studio.Resources.Languages.Core.TriggerQueuesSaveWorkflowNotSelected, Warewolf.Studio.Resources.Languages.Core.TriggerQueuesSaveErrorHeader, MessageBoxButton.OK, MessageBoxImage.Error, string.Empty, false, true, false, false, false, false), Times.Once);
        }

        [TestMethod]
        [TestCategory(nameof(QueueEventsViewModel))]
        [Owner("Pieter Terblanche")]
        public void QueueEventsViewModel_QueueEvents_Save_Successful()
        {
            var resourceId = Guid.NewGuid();
            var resource = new Resource
            {
                ResourceID = resourceId
            };

            var mockPopupController = new Mock<IPopupController>();
            mockPopupController.Setup(popupController => popupController.Show(Warewolf.Studio.Resources.Languages.Core.TriggerQueuesSaveWorkflowNotSelected, Warewolf.Studio.Resources.Languages.Core.TriggerQueuesSaveErrorHeader, MessageBoxButton.OK, MessageBoxImage.Error, string.Empty, false, true, false, false, false, false));
            CustomContainer.Register(mockPopupController.Object);

            Mock<IServer> mockServer = SetupForTriggerQueueView(resource);
            var queueEventsViewModel = CreateViewModel();
            queueEventsViewModel.SelectedQueue = new TriggerQueueViewForTesting(mockServer.Object)
            {
                SelectedQueueSource = resource
            };
            queueEventsViewModel.SelectedQueue.QueueSourceId = resourceId;
            queueEventsViewModel.SelectedQueue.WorkflowName = "Workflow";

            var isSaved = queueEventsViewModel.Save();
            Assert.IsTrue(isSaved);
            Assert.IsFalse(queueEventsViewModel.SelectedQueue.IsNewQueue);
        }

        private class TriggerQueueViewForTesting : TriggerQueueView
        {
            bool _isNewItem;
            bool _isDirty;
            string _queueName;

            public TriggerQueueViewForTesting(IServer server)
                : base(server)
            {
            }

            public new bool IsDirty
            {
                get => _isDirty;
                set => _isDirty = value;
            }
            public new Guid TriggerId { get; set; }
            public new string TriggerQueueName { get; set; }
            public new string OldQueueName { get; set; }
            public new bool Enabled { get; set; }
            public new string WorkflowName { get; set; }
            public new Guid ResourceId { get; set; }
            public new string UserName { get; set; }
            public new string Password { get; set; }
            public new IErrorResultTO Errors { get; set; }
            public new bool IsNewQueue
            {
                get => _isNewItem;
                set => _isNewItem = value;
            }
            public new string NameForDisplay { get; set; }
            public new string QueueName
            {
                get => _queueName;
                set => _queueName = value;
            }

            public new Guid QueueSourceId { get; set; }
            public new int Concurrency { get; set; }
            public new IOption[] Options { get; set; }
            public new Guid QueueSinkId { get; set; }
            public new string DeadLetterQueue { get; set; }
            public new IOption[] DeadLetterOptions { get; set; }
            public new ICollection<IServiceInput> Inputs { get; set; }

            public void SetItem(ITriggerQueue item)
            {
            }
            public new bool Equals(ITriggerQueue other)
            {
                return !IsDirty;
            }
        }
    }
}
