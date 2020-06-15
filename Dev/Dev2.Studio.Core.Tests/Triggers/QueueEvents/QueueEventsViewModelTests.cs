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
using Dev2.Common.Interfaces.Resources;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.ConnectionHelpers;
using Dev2.Core.Tests.Environments;
using Dev2.Dialogs;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Studio.Interfaces;
using Dev2.Triggers.QueueEvents;
using Dev2.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using Warewolf.Data;
using Warewolf.Options;
using Warewolf.Trigger.Queue;
using Warewolf.Triggers;
using Warewolf.UI;

namespace Dev2.Core.Tests.Triggers.QueueEvents
{
    [TestClass]
    [TestCategory("Studio Triggers Queue Core")]
    [DoNotParallelize]
    public class QueueEventsViewModelTests
    {
        Guid _workflowResourceId = Guid.NewGuid();
        Mock<IResourcePickerDialog> _mockResourcePickerDialog;

        [TestInitialize]
        public void SetupForTest()
        {
            AppUsageStats.LocalHost = "http://localhost:3142";
            var mockShellViewModel = new Mock<IShellViewModel>();
            var lcl = new Mock<IServer>();
            lcl.Setup(a => a.DisplayName).Returns("Localhost");
            mockShellViewModel.Setup(x => x.LocalhostServer).Returns(lcl.Object);
            mockShellViewModel.Setup(x => x.ActiveServer).Returns(new Mock<IServer>().Object);
            var connectControlSingleton = new Mock<IConnectControlSingleton>();
            var explorerTooltips = new Mock<IExplorerTooltips>();

            CustomContainer.Register(mockShellViewModel.Object);
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

            var mockExplorerTreeItem = new Mock<IExplorerTreeItem>();
            mockExplorerTreeItem.Setup(explorerItem => explorerItem.ResourceId).Returns(_workflowResourceId);
            mockExplorerTreeItem.Setup(explorerItem => explorerItem.ResourcePath).Returns("ResourcePath");

            _mockResourcePickerDialog = new Mock<IResourcePickerDialog>();
            _mockResourcePickerDialog.Setup(resourcePicker => resourcePicker.SelectedResource).Returns(mockExplorerTreeItem.Object);
            _mockResourcePickerDialog.Setup(resourcePicker => resourcePicker.ShowDialog(mockServer.Object)).Returns(true);

            return new QueueEventsViewModel(mockServer.Object, mockExternalExecutor.Object, _mockResourcePickerDialog.Object);
        }

        private Mock<IServer> SetupForTriggerQueueView(Resource resource)
        {
            Mock<IResource> _mockQueueSource;
            Guid _queueResourceId = Guid.NewGuid();


            var mockServer = new Mock<IServer>();
            var mockOption = new Mock<IOption>();
            var mockInputs = new Mock<ICollection<IServiceInputBase>>();
            var triggerQueue = new TriggerQueue
            {
                Name = "TestTriggerQueueName",
                QueueSourceId = _queueResourceId,
                QueueName = "TestQueue",
                WorkflowName = "TestWorkflow",
                Concurrency = 1000,
                UserName = "Bob",
                Password = "123456",
                Options = new IOption[] { mockOption.Object },
                QueueSinkId = _queueResourceId,
                DeadLetterQueue = "TestDeadLetterQueue",
                DeadLetterOptions = new IOption[] { mockOption.Object },
                Inputs = mockInputs.Object
            };

            List<ITriggerQueue> expectedTriggers = new List<ITriggerQueue>
            {
                triggerQueue
            };

            string[] tempValues = new string[3];
            tempValues[0] = "value1";
            tempValues[1] = "value2";
            tempValues[2] = "value3";

            List<IOption> expectedOptions = SetupOptionsView();
            var expectedQueueNames = new Dictionary<string, string[]>
            {
                { "QueueNames", tempValues }
            };
            var queueSource2 = new Mock<IResource>();
            _mockQueueSource = new Mock<IResource>();
            _mockQueueSource.Setup(source => source.ResourceID).Returns(_queueResourceId);
            var expectedList = new List<IResource>
            {
                _mockQueueSource.Object, queueSource2.Object
            };
            var mockResourceRepository = new Mock<IResourceRepository>();
            if (resource == null)
            {
                mockResourceRepository.Setup(resourceRepository => resourceRepository.FindOptions(mockServer.Object, _mockQueueSource.Object)).Returns(expectedOptions);
            }
            else
            {
                mockResourceRepository.Setup(resourceRepository => resourceRepository.FindOptions(mockServer.Object, resource)).Returns(expectedOptions);
            }

            mockResourceRepository.Setup(resourceRepository => resourceRepository.FindResourcesByType<IQueueSource>(mockServer.Object)).Returns(expectedList);
            mockResourceRepository.Setup(resourceRepository => resourceRepository.GetTriggerQueueHistory(Guid.NewGuid())).Returns(new List<IExecutionHistory>());
            mockResourceRepository.Setup(resourceRepository => resourceRepository.FindAutocompleteOptions(mockServer.Object, _mockQueueSource.Object)).Returns(expectedQueueNames);            
            mockResourceRepository.Setup(resourceRepository => resourceRepository.FetchTriggerQueues()).Returns(expectedTriggers);

            var dataList = "<DataList><Name>Test</Name><Surname>test1</Surname><Person><Name>sdas</Name><Surname>asdsad</Surname></Person></DataList>";
            var mockResourceModel = new Mock<IContextualResourceModel>();
            mockResourceModel.Setup(resourceModel => resourceModel.DataList).Returns(dataList);

            mockResourceRepository.Setup(resourceRepository => resourceRepository.LoadContextualResourceModel(_workflowResourceId)).Returns(mockResourceModel.Object);

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
            Assert.AreEqual(2, queueEventsViewModel.Queues.Count);
            Assert.IsNull(queueEventsViewModel.SelectedQueue);

            var queue1 = new TriggerQueueViewForTesting(mockServer.Object);
            var queue2 = new TriggerQueueViewForTesting(mockServer.Object);

            queueEventsViewModel.Queues = new ObservableCollection<TriggerQueueView>
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

            Assert.AreEqual(3, queueEventsViewModel.Queues.Count);
            Assert.AreEqual("TestTriggerQueueName", queueEventsViewModel.Queues[0].NameForDisplay);
            Assert.IsFalse(queueEventsViewModel.Queues[0].NewQueue);

            Assert.AreEqual("Queue 1 *", queueEventsViewModel.Queues[1].NameForDisplay);
            Assert.IsTrue(queueEventsViewModel.Queues[1].NewQueue);

            Assert.IsTrue(queueEventsViewModel.Queues[2].NewQueue);
        }

        [TestMethod]
        [TestCategory(nameof(QueueEventsViewModel))]
        [Owner("Pieter Terblanche")]
        public void QueueEventsViewModel_QueueEvents_AddNew_ShouldNotAddNewItem()
        {
            var mockPopupController = new Mock<IPopupController>();
            mockPopupController.Setup(popupController => popupController.Show(Warewolf.Studio.Resources.Languages.Core.TriggerQueueSaveEditedTestsMessage, Warewolf.Studio.Resources.Languages.Core.TriggerQueueSaveEditedQueueHeader, MessageBoxButton.OK, MessageBoxImage.Error, null, false, true, false, false, false, false));
            CustomContainer.Register(mockPopupController.Object);

            var queueEventsViewModel = CreateViewModel();

            queueEventsViewModel.NewCommand.Execute(null);

            Assert.AreEqual(3, queueEventsViewModel.Queues.Count);
            Assert.AreEqual("TestTriggerQueueName", queueEventsViewModel.Queues[0].NameForDisplay);
            Assert.IsFalse(queueEventsViewModel.Queues[0].NewQueue);

            Assert.AreEqual("Queue 1 *", queueEventsViewModel.Queues[1].NameForDisplay);
            Assert.IsTrue(queueEventsViewModel.Queues[1].NewQueue);

            Assert.IsTrue(queueEventsViewModel.Queues[2].NewQueue);

            queueEventsViewModel.NewCommand.Execute(null);

            mockPopupController.Verify(popupController => popupController.Show(Warewolf.Studio.Resources.Languages.Core.TriggerQueueSaveEditedTestsMessage, Warewolf.Studio.Resources.Languages.Core.TriggerQueueSaveEditedQueueHeader, MessageBoxButton.OK, MessageBoxImage.Error, null, false, true, false, false, false, false), Times.Once);
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

            Assert.AreEqual(3, queueEventsViewModel.Queues.Count);

            queueEventsViewModel.DeleteCommand.Execute(null);
            Assert.AreEqual(2, queueEventsViewModel.Queues.Count);

            Assert.AreEqual("TestTriggerQueueName", queueEventsViewModel.Queues[0].NameForDisplay);
            Assert.IsFalse(queueEventsViewModel.Queues[0].NewQueue);
        }

        [TestMethod]
        [TestCategory(nameof(QueueEventsViewModel))]
        [Owner("Pieter Terblanche")]
        public void QueueEventsViewModel_QueueEvents_AddWorkflow_ShouldDeleteSelectedItem()
        {
            Mock<IServer> mockServer = SetupForTriggerQueueView(null);
            var queueEventsViewModel = CreateViewModel();
            var triggerQueueView = new TriggerQueueViewForTesting(mockServer.Object);

            queueEventsViewModel.Queues.Add(triggerQueueView);
            queueEventsViewModel.SelectedQueue = triggerQueueView;

            queueEventsViewModel.AddWorkflowCommand.Execute(null);
            Assert.AreEqual(_workflowResourceId, queueEventsViewModel.SelectedQueue.ResourceId);
            Assert.AreEqual("ResourcePath", queueEventsViewModel.SelectedQueue.WorkflowName);
            Assert.IsNotNull(queueEventsViewModel.SelectedQueue.Inputs);
            Assert.AreEqual(0, queueEventsViewModel.SelectedQueue.Inputs.Count);
        }

        [TestMethod]
        [TestCategory(nameof(QueueEventsViewModel))]
        [Owner("Pieter Terblanche")]
        public void QueueEventsViewModel_UpdateHelpDescriptor()
        {
            var queueEventsViewModel = CreateViewModel();
            queueEventsViewModel.UpdateHelpDescriptor("This is a help text");
            Assert.AreEqual("This is a help text", queueEventsViewModel.HelpText);
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
        public void QueueEventsViewModel_QueueEvents_ViewQueueStats()
        {
            Uri uri = new Uri("https://www.rabbitmq.com/blog/tag/statistics/");

            var mockServer = SetupForTriggerQueueView(null);
            var mockExternalExecutor = new Mock<IExternalProcessExecutor>();

            var mockResourcePickerDialog = new Mock<IResourcePickerDialog>();
            var queueEventsViewModel =  new QueueEventsViewModel(mockServer.Object, mockExternalExecutor.Object, mockResourcePickerDialog.Object);
            queueEventsViewModel.QueueStatsCommand.Execute(null);

            mockExternalExecutor.Verify(externalProcessExecutor => externalProcessExecutor.OpenInBrowser(uri), Times.Once);
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
        public void QueueEventsViewModel_QueueEvents_Save_Empty_Queue()
        {
            var resourceId = Guid.NewGuid();
            var resource = new Resource
            {
                ResourceID = resourceId
            };

            var mockPopupController = new Mock<IPopupController>();
            mockPopupController.Setup(popupController => popupController.Show(Warewolf.Studio.Resources.Languages.Core.TriggerQueuesSaveQueueNameEmpty, Warewolf.Studio.Resources.Languages.Core.TriggerQueuesSaveErrorHeader, MessageBoxButton.OK, MessageBoxImage.Error, string.Empty, false, true, false, false, false, false));
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

            mockPopupController.Verify(popupController => popupController.Show(Warewolf.Studio.Resources.Languages.Core.TriggerQueuesSaveQueueNameEmpty, Warewolf.Studio.Resources.Languages.Core.TriggerQueuesSaveErrorHeader, MessageBoxButton.OK, MessageBoxImage.Error, string.Empty, false, true, false, false, false, false), Times.Once);
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
            queueEventsViewModel.SelectedQueue.QueueName = "QueueName";

            var isSaved = queueEventsViewModel.Save();
            Assert.IsFalse(isSaved);

            mockPopupController.Verify(popupController => popupController.Show(Warewolf.Studio.Resources.Languages.Core.TriggerQueuesSaveWorkflowNotSelected, Warewolf.Studio.Resources.Languages.Core.TriggerQueuesSaveErrorHeader, MessageBoxButton.OK, MessageBoxImage.Error, string.Empty, false, true, false, false, false, false), Times.Once);
        }

        [TestMethod]
        [TestCategory(nameof(QueueEventsViewModel))]
        [Owner("Pieter Terblanche")]
        public void QueueEventsViewModel_QueueEvents_Save_Empty_OnError_QueueSource()
        {
            var resourceId = Guid.NewGuid();
            var resource = new Resource
            {
                ResourceID = resourceId
            };

            var mockPopupController = new Mock<IPopupController>();
            mockPopupController.Setup(popupController => popupController.Show(Warewolf.Studio.Resources.Languages.Core.TriggerQueuesSaveQueueSinkNotSelected, Warewolf.Studio.Resources.Languages.Core.TriggerQueuesSaveErrorHeader, MessageBoxButton.OK, MessageBoxImage.Error, string.Empty, false, true, false, false, false, false));
            CustomContainer.Register(mockPopupController.Object);

            Mock<IServer> mockServer = SetupForTriggerQueueView(resource);
            var queueEventsViewModel = CreateViewModel();
            queueEventsViewModel.SelectedQueue = new TriggerQueueViewForTesting(mockServer.Object)
            {
                SelectedQueueSource = resource
            };
            queueEventsViewModel.SelectedQueue.QueueSourceId = resourceId;
            queueEventsViewModel.SelectedQueue.QueueName = "QueueName";
            queueEventsViewModel.SelectedQueue.WorkflowName = "WorkflowName";

            var isSaved = queueEventsViewModel.Save();
            Assert.IsFalse(isSaved);

            mockPopupController.Verify(popupController => popupController.Show(Warewolf.Studio.Resources.Languages.Core.TriggerQueuesSaveQueueSinkNotSelected, Warewolf.Studio.Resources.Languages.Core.TriggerQueuesSaveErrorHeader, MessageBoxButton.OK, MessageBoxImage.Error, string.Empty, false, true, false, false, false, false), Times.Once);
        }

        [TestMethod]
        [TestCategory(nameof(QueueEventsViewModel))]
        [Owner("Pieter Terblanche")]
        public void QueueEventsViewModel_QueueEvents_Save_Empty_OnError_Queue()
        {
            var resourceId = Guid.NewGuid();
            var resource = new Resource
            {
                ResourceID = resourceId
            };

            var mockPopupController = new Mock<IPopupController>();
            mockPopupController.Setup(popupController => popupController.Show(Warewolf.Studio.Resources.Languages.Core.TriggerQueuesSaveOnErrorQueueNameEmpty, Warewolf.Studio.Resources.Languages.Core.TriggerQueuesSaveErrorHeader, MessageBoxButton.OK, MessageBoxImage.Error, string.Empty, false, true, false, false, false, false));
            CustomContainer.Register(mockPopupController.Object);

            Mock<IServer> mockServer = SetupForTriggerQueueView(resource);
            var queueEventsViewModel = CreateViewModel();
            queueEventsViewModel.SelectedQueue = new TriggerQueueViewForTesting(mockServer.Object)
            {
                SelectedQueueSource = resource
            };
            queueEventsViewModel.SelectedQueue.QueueSourceId = resourceId;
            queueEventsViewModel.SelectedQueue.QueueName = "QueueName";
            queueEventsViewModel.SelectedQueue.WorkflowName = "WorkflowName";
            queueEventsViewModel.SelectedQueue.QueueSinkId = resourceId;

            var isSaved = queueEventsViewModel.Save();
            Assert.IsFalse(isSaved);

            mockPopupController.Verify(popupController => popupController.Show(Warewolf.Studio.Resources.Languages.Core.TriggerQueuesSaveOnErrorQueueNameEmpty, Warewolf.Studio.Resources.Languages.Core.TriggerQueuesSaveErrorHeader, MessageBoxButton.OK, MessageBoxImage.Error, string.Empty, false, true, false, false, false, false), Times.Once);
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
            queueEventsViewModel.SelectedQueue.QueueName = "QueueName";
            queueEventsViewModel.SelectedQueue.WorkflowName = "Workflow";
            queueEventsViewModel.SelectedQueue.QueueSinkId = resourceId;
            queueEventsViewModel.SelectedQueue.DeadLetterQueue = "DeadLetterQueue";

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
            public bool Equals(ITriggerQueue other)
            {
                return !IsDirty;
            }
        }
        public class OptionViewForTesting : OptionView
        {
            public OptionViewForTesting(IOption option)
                : base(option, () => { })
            {
            }
        }
    }
}
