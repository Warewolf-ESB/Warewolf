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
using Dev2.Common.Interfaces.Queue;
using Dev2.Common.Interfaces.Resources;
using Dev2.Data.TO;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Studio.Interfaces;
using Dev2.Triggers.QueueEvents;
using Dev2.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using Warewolf.Core;
using Warewolf.Options;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Interfaces.Trigger;
using Dev2.Triggers;

namespace Dev2.Studio.Tests.ViewModels.Tasks.QueueEvents
{
    [TestClass]
    public class QueueEventsViewModelTests
    {
        [TestMethod]
        [TestCategory(nameof(QueueEventsViewModel))]
        [Owner("Pieter Terblanche")]
        public void QueueEventsViewModel_QueueEvents()
        {
            var mockServer = new Mock<IServer>();
            var queueEventsViewModel = new QueueEventsViewModel(mockServer.Object);

            Assert.IsNull(queueEventsViewModel.QueueEvents);
            Assert.IsNull(queueEventsViewModel.SelectedQueueEvent);

            var queue1 = "Queue1";
            var queue2 = "Queue2";

            var queueEvents = new System.Collections.ObjectModel.ObservableCollection<string>
            {
                queue1,
                queue2
            };

            queueEventsViewModel.QueueEvents = queueEvents;
            queueEventsViewModel.SelectedQueueEvent = queue2;

            Assert.IsNotNull(queueEventsViewModel.QueueEvents);
            Assert.AreEqual(2, queueEventsViewModel.QueueEvents.Count);

            Assert.IsNotNull(queueEventsViewModel.SelectedQueueEvent);
            Assert.AreEqual(queue2, queueEventsViewModel.SelectedQueueEvent);
        }

        [TestMethod]
        [TestCategory(nameof(QueueEventsViewModel))]
        [Owner("Pieter Terblanche")]
        public void QueueEventsViewModel_QueueSources()
        {
            var queueSourceID1 = Guid.NewGuid();
            var queueSourceName1 = "QueueSource1";

            var queueSourceID2 = Guid.NewGuid();
            var queueSourceName2 = "QueueSource2";

            var queueSource1 = new Resource { ResourceID = queueSourceID1, ResourceName = queueSourceName1 };
            var queueSource2 = new Resource { ResourceID = queueSourceID2, ResourceName = queueSourceName2 };

            var expectedList = new List<IResource>
            {
                queueSource1, queueSource2
            };

            var mockServer = new Mock<IServer>();
            var mockResourceRepository = new Mock<IResourceRepository>();
            mockResourceRepository.Setup(resourceRepository => resourceRepository.FindResourcesByType<IQueueSource>(mockServer.Object)).Returns(expectedList);

            mockServer.Setup(server => server.ResourceRepository).Returns(mockResourceRepository.Object);

            var queueEventsViewModel = new QueueEventsViewModel(mockServer.Object);

            Assert.IsNotNull(queueEventsViewModel.QueueSources);
            Assert.IsNull(queueEventsViewModel.SelectedQueueSource);

            Assert.IsNotNull(queueEventsViewModel.QueueSources);
            Assert.AreEqual(2, queueEventsViewModel.QueueSources.Count);
            Assert.AreEqual(queueSourceID1, queueEventsViewModel.QueueSources[0].ResourceID);
            Assert.AreEqual(queueSourceName1, queueEventsViewModel.QueueSources[0].ResourceName);
            Assert.AreEqual(queueSourceID2, queueEventsViewModel.QueueSources[1].ResourceID);
            Assert.AreEqual(queueSourceName2, queueEventsViewModel.QueueSources[1].ResourceName);
        }

        [TestMethod]
        [TestCategory(nameof(QueueEventsViewModel))]
        [Owner("Pieter Terblanche")]
        public void QueueEventsViewModel_DeadLetterQueueSources()
        {
            var queueSourceID1 = Guid.NewGuid();
            var queueSourceName1 = "QueueSource1";

            var queueSourceID2 = Guid.NewGuid();
            var queueSourceName2 = "QueueSource2";

            var queueSource1 = new Resource { ResourceID = queueSourceID1, ResourceName = queueSourceName1 };
            var queueSource2 = new Resource { ResourceID = queueSourceID2, ResourceName = queueSourceName2 };

            var expectedList = new List<IResource>
            {
                queueSource1, queueSource2
            };

            var mockServer = new Mock<IServer>();
            var mockResourceRepository = new Mock<IResourceRepository>();
            mockResourceRepository.Setup(resourceRepository => resourceRepository.FindResourcesByType<IQueueSource>(mockServer.Object)).Returns(expectedList);

            mockServer.Setup(server => server.ResourceRepository).Returns(mockResourceRepository.Object);

            var queueEventsViewModel = new QueueEventsViewModel(mockServer.Object);

            Assert.IsNotNull(queueEventsViewModel.DeadLetterQueueSources);
            Assert.IsNull(queueEventsViewModel.SelectedDeadLetterQueueSource);

            Assert.IsNotNull(queueEventsViewModel.DeadLetterQueueSources);
            Assert.AreEqual(2, queueEventsViewModel.DeadLetterQueueSources.Count);
            Assert.AreEqual(queueSourceID1, queueEventsViewModel.DeadLetterQueueSources[0].ResourceID);
            Assert.AreEqual(queueSourceName1, queueEventsViewModel.DeadLetterQueueSources[0].ResourceName);
            Assert.AreEqual(queueSourceID2, queueEventsViewModel.DeadLetterQueueSources[1].ResourceID);
            Assert.AreEqual(queueSourceName2, queueEventsViewModel.DeadLetterQueueSources[1].ResourceName);
        }

        [TestMethod]
        [TestCategory(nameof(QueueEventsViewModel))]
        [Owner("Pieter Terblanche")]
        public void QueueEventsViewModel_QueueNames()
        {
            var queueSourceID2 = Guid.NewGuid();
            var queueSourceName2 = "QueueSource2";

            var queueSource2 = new Resource
            {
                ResourceID = queueSourceID2,
                ResourceName = queueSourceName2
            };

            string[] tempValues = new string[3];
            tempValues[0] = "value1";
            tempValues[1] = "value2";
            tempValues[2] = "value3";

            var expectedQueueNames = new Dictionary<string, string[]>
            {
                { "QueueNames", tempValues }
            };

            List<IOption> expectedOptions = SetupOptionsView();

            var mockServer = new Mock<IServer>();
            var mockResourceRepository = new Mock<IResourceRepository>();
            mockResourceRepository.Setup(resourceRepository => resourceRepository.FindAutocompleteOptions(mockServer.Object, queueSource2)).Returns(expectedQueueNames);
            mockResourceRepository.Setup(resourceRepository => resourceRepository.FindOptions(mockServer.Object, queueSource2)).Returns(expectedOptions);

            mockServer.Setup(server => server.ResourceRepository).Returns(mockResourceRepository.Object);

            var queueEventsViewModel = new QueueEventsViewModel(mockServer.Object)
            {
                SelectedQueueSource = queueSource2
            };

            Assert.IsNotNull(queueEventsViewModel.SelectedQueueSource);
            Assert.AreEqual(queueSource2, queueEventsViewModel.SelectedQueueSource);
            Assert.IsNotNull(queueEventsViewModel.QueueNames);
            Assert.AreEqual(3, queueEventsViewModel.QueueNames.Count);
            Assert.AreEqual("value1", queueEventsViewModel.QueueNames[0].Value);
            Assert.AreEqual("value2", queueEventsViewModel.QueueNames[1].Value);
            Assert.AreEqual("value3", queueEventsViewModel.QueueNames[2].Value);

            Assert.AreEqual(3, queueEventsViewModel.Options.Count);

            var optionOne = queueEventsViewModel.Options[0].DataContext as OptionBool;
            Assert.IsNotNull(optionOne);
            Assert.AreEqual("bool", optionOne.Name);
            Assert.IsFalse(optionOne.Value);
            Assert.IsTrue(optionOne.Default);

            var optionTwo = queueEventsViewModel.Options[1].DataContext as OptionInt;
            Assert.IsNotNull(optionTwo);
            Assert.AreEqual("int", optionTwo.Name);
            Assert.AreEqual(10, optionTwo.Value);
            Assert.AreEqual(0, optionTwo.Default);

            var optionThree = queueEventsViewModel.Options[2].DataContext as OptionAutocomplete;
            Assert.IsNotNull(optionThree);
            Assert.AreEqual("auto", optionThree.Name);
            Assert.AreEqual("new text", optionThree.Value);
            Assert.AreEqual(1, optionThree.Suggestions.Count());
            Assert.AreEqual("", optionThree.Suggestions[0]);
            Assert.AreEqual("", optionThree.Default);

            Assert.IsNull(queueEventsViewModel.QueueName);

            queueEventsViewModel.QueueName = "value1";

            Assert.IsNotNull(queueEventsViewModel.QueueName);
            Assert.AreEqual("value1", queueEventsViewModel.QueueName);
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
        public void QueueEventsViewModel_DeadLetterQueues()
        {
            var queueSourceID2 = Guid.NewGuid();
            var queueSourceName2 = "QueueSource2";

            var queueSource2 = new Resource { ResourceID = queueSourceID2, ResourceName = queueSourceName2 };

            string[] tempValues = new string[3];
            tempValues[0] = "value1";
            tempValues[1] = "value2";
            tempValues[2] = "value3";

            var expectedQueueNames = new Dictionary<string, string[]>
            {
                { "QueueNames", tempValues }
            };

            List<IOption> expectedOptions = SetupOptionsView();

            var mockApplicationAdapter = new Mock<IApplicationAdaptor>();
            mockApplicationAdapter.Setup(p => p.TryFindResource(It.IsAny<string>())).Verifiable();
            CustomContainer.Register(mockApplicationAdapter.Object);

            var mockServer = new Mock<IServer>();
            var mockResourceRepository = new Mock<IResourceRepository>();
            mockResourceRepository.Setup(resourceRepository => resourceRepository.FindAutocompleteOptions(mockServer.Object, queueSource2)).Returns(expectedQueueNames);
            mockResourceRepository.Setup(resourceRepository => resourceRepository.FindOptions(mockServer.Object, queueSource2)).Returns(expectedOptions);

            mockServer.Setup(server => server.ResourceRepository).Returns(mockResourceRepository.Object);

            var queueEventsViewModel = new QueueEventsViewModel(mockServer.Object)
            {
                SelectedDeadLetterQueueSource = queueSource2
            };

            Assert.IsNotNull(queueEventsViewModel.SelectedDeadLetterQueueSource);
            Assert.AreEqual(queueSource2, queueEventsViewModel.SelectedDeadLetterQueueSource);
            Assert.IsNotNull(queueEventsViewModel.DeadLetterQueues);
            Assert.AreEqual(3, queueEventsViewModel.DeadLetterQueues.Count);
            Assert.AreEqual("value1", queueEventsViewModel.DeadLetterQueues[0].Value);
            Assert.AreEqual("value2", queueEventsViewModel.DeadLetterQueues[1].Value);
            Assert.AreEqual("value3", queueEventsViewModel.DeadLetterQueues[2].Value);

            Assert.AreEqual(3, queueEventsViewModel.DeadLetterOptions.Count);

            var optionOne = queueEventsViewModel.DeadLetterOptions[0].DataContext as OptionBool;
            Assert.IsNotNull(optionOne);
            Assert.AreEqual("bool", optionOne.Name);
            Assert.IsFalse(optionOne.Value);
            Assert.IsTrue(optionOne.Default);

            var optionOneTemplate = queueEventsViewModel.DeadLetterOptions[0].DataTemplate;
            mockApplicationAdapter.Verify(model => model.TryFindResource("OptionBoolStyle"), Times.Once());

            var optionTwo = queueEventsViewModel.DeadLetterOptions[1].DataContext as OptionInt;
            Assert.IsNotNull(optionTwo);
            Assert.AreEqual("int", optionTwo.Name);
            Assert.AreEqual(10, optionTwo.Value);
            Assert.AreEqual(0, optionTwo.Default);

            var optionTwoTemplate = queueEventsViewModel.DeadLetterOptions[1].DataTemplate;
            mockApplicationAdapter.Verify(model => model.TryFindResource("OptionIntStyle"), Times.Once());

            var optionThree = queueEventsViewModel.DeadLetterOptions[2].DataContext as OptionAutocomplete;
            Assert.IsNotNull(optionThree);
            Assert.AreEqual("auto", optionThree.Name);
            Assert.AreEqual("new text", optionThree.Value);
            Assert.AreEqual(1, optionThree.Suggestions.Count());
            Assert.AreEqual("", optionThree.Suggestions[0]);
            Assert.AreEqual("", optionThree.Default);

            var optionThreeTemplate = queueEventsViewModel.DeadLetterOptions[2].DataTemplate;
            mockApplicationAdapter.Verify(model => model.TryFindResource("OptionAutocompleteStyle"), Times.Once());

            Assert.IsNull(queueEventsViewModel.DeadLetterQueue);

            queueEventsViewModel.DeadLetterQueue = "value1";

            Assert.IsNotNull(queueEventsViewModel.DeadLetterQueue);
            Assert.AreEqual("value1", queueEventsViewModel.DeadLetterQueue);
        }

        [TestMethod]
        [TestCategory(nameof(QueueEventsViewModel))]
        [Owner("Pieter Terblanche")]
        public void QueueEventsViewModel_WorkflowName()
        {
            var mockServer = new Mock<IServer>();
            var queueEventsViewModel = new QueueEventsViewModel(mockServer.Object);

            Assert.IsNull(queueEventsViewModel.WorkflowName);

            var workflowName = "Workflow1";
            queueEventsViewModel.WorkflowName = workflowName;

            Assert.AreEqual(workflowName, queueEventsViewModel.WorkflowName);
        }

        [TestMethod]
        [TestCategory(nameof(QueueEventsViewModel))]
        [Owner("Pieter Terblanche")]
        public void QueueEventsViewModel_Concurrency()
        {
            var mockServer = new Mock<IServer>();
            var queueEventsViewModel = new QueueEventsViewModel(mockServer.Object);

            Assert.AreEqual(0, queueEventsViewModel.Concurrency);

            var concurrency = 5;
            queueEventsViewModel.Concurrency = concurrency;

            Assert.AreEqual(concurrency, queueEventsViewModel.Concurrency);
        }

        [TestMethod]
        [TestCategory(nameof(QueueEventsViewModel))]
        [Owner("Pieter Terblanche")]
        public void QueueEventsViewModel_QueueEvents_AddNew_And_Delete()
        {
            var mockServer = new Mock<IServer>();
            var queueEventsViewModel = new QueueEventsViewModel(mockServer.Object);

            Assert.IsNull(queueEventsViewModel.QueueEvents);

            queueEventsViewModel.QueueEvents = new System.Collections.ObjectModel.ObservableCollection<string>();
            queueEventsViewModel.NewCommand.Execute(null);

            Assert.IsNotNull(queueEventsViewModel.QueueEvents);
            Assert.AreEqual(1, queueEventsViewModel.QueueEvents.Count);

            queueEventsViewModel.DeleteCommand.Execute(null);
            Assert.AreEqual(0, queueEventsViewModel.QueueEvents.Count);
        }

        [TestMethod]
        [TestCategory(nameof(QueueEventsViewModel))]
        [Owner("Pieter Terblanche")]
        public void QueueEventsViewModel_QueueEvents_Inputs()
        {
            var mockServer = new Mock<IServer>();
            var queueEventsViewModel = new QueueEventsViewModel(mockServer.Object);

            Assert.IsNotNull(queueEventsViewModel.Inputs);

            var inputs = new ObservableCollection<IServiceInput>();
            inputs.Add(new ServiceInput("name1", "value1"));
            inputs.Add(new ServiceInput("name2", "value2"));

            queueEventsViewModel.Inputs = inputs;

            var inputsAsList = queueEventsViewModel.Inputs.ToList();

            Assert.AreEqual(2, queueEventsViewModel.Inputs.Count);
            Assert.AreEqual("name1", inputsAsList[0].Name);
            Assert.AreEqual("value1", inputsAsList[0].Value);
            Assert.AreEqual("name2", inputsAsList[1].Name);
            Assert.AreEqual("value2", inputsAsList[1].Value);
        }

        [TestMethod]
        [TestCategory(nameof(QueueEventsViewModel))]
        [Owner("Pieter Terblanche")]
        public void QueueEventsViewModel_QueueEvents_PasteResponseVisible()
        {
            var mockServer = new Mock<IServer>();
            var queueEventsViewModel = new QueueEventsViewModel(mockServer.Object);

            Assert.IsFalse(queueEventsViewModel.PasteResponseVisible);

            queueEventsViewModel.PasteResponseVisible = true;

            Assert.IsTrue(queueEventsViewModel.PasteResponseVisible);
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

            var queueEventsViewModel = new QueueEventsViewModel(mockServer.Object, mockExternalProcessExecutor.Object, new SynchronousAsyncWorker());

            queueEventsViewModel.QueueStatsCommand.Execute(null);

            mockExternalProcessExecutor.Verify(externalProcessExecutor => externalProcessExecutor.OpenInBrowser(uri), Times.Once);
        }

        [TestMethod]
        [TestCategory(nameof(QueueEventsViewModel))]
        [Owner("Pieter Terblanche")]
        public void QueueEventsViewModel_QueueEvents_PasteResponseCommand()
        {
            var mockServer = new Mock<IServer>();

            var queueEventsViewModel = new QueueEventsViewModel(mockServer.Object);
            Assert.IsFalse(queueEventsViewModel.PasteResponseVisible);

            queueEventsViewModel.PasteResponseCommand.Execute(null);
            Assert.IsTrue(queueEventsViewModel.PasteResponseVisible);
        }

        [TestMethod]
        [TestCategory(nameof(QueueEventsViewModel))]
        [Owner("Pieter Terblanche")]
        public void QueueEventsViewModel_QueueEvents_TestCommand()
        {
            var mockServer = new Mock<IServer>();

            var queueEventsViewModel = new QueueEventsViewModel(mockServer.Object);
            Assert.IsNull(queueEventsViewModel.TestResults);
            Assert.IsFalse(queueEventsViewModel.IsTesting);
            Assert.IsFalse(queueEventsViewModel.IsTestResultsEmptyRows);

            queueEventsViewModel.TestCommand.Execute(null);

            Assert.IsTrue(queueEventsViewModel.TestResultsAvailable);
            Assert.IsFalse(queueEventsViewModel.IsTestResultsEmptyRows);
            Assert.IsFalse(queueEventsViewModel.IsTesting);
            Assert.IsTrue(queueEventsViewModel.TestPassed);
            Assert.IsFalse(queueEventsViewModel.TestFailed);
        }

        [TestMethod]
        [TestCategory(nameof(QueueEventsViewModel))]
        [Owner("Candice Daniel")]
        public void QueueEventsViewModel_AccountName()
        {
            var _accountNameChanged = false;
            var mockServer = new Mock<IServer>();
            var queueEventsViewModel = new QueueEventsViewModel(mockServer.Object);
            var queuedResourceForTest = new QueuedResourceForTest();
            queueEventsViewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "AccountName")
                {
                    _accountNameChanged = true;
                }
            };
            queueEventsViewModel.SelectedQueue = queuedResourceForTest;
            queueEventsViewModel.ShowError(Warewolf.Studio.Resources.Languages.Core.QueueEventsLoginErrorMessage);
            Assert.AreEqual(Warewolf.Studio.Resources.Languages.Core.QueueEventsLoginErrorMessage, queueEventsViewModel.Error);
            //------------Execute Test-------------------------- -
            queueEventsViewModel.AccountName = "someAccountName";
            //------------Assert Results-------------------------
            Assert.AreEqual("someAccountName", queueEventsViewModel.AccountName);
            Assert.AreEqual("", queueEventsViewModel.Error);
            Assert.AreEqual("someAccountName", queueEventsViewModel.SelectedQueue.UserName);
            Assert.IsTrue(_accountNameChanged);

            queueEventsViewModel.AccountName = "someAccountName";
            Assert.AreEqual("someAccountName", queueEventsViewModel.AccountName);
        }

        [TestMethod]
        [TestCategory(nameof(QueueEventsViewModel))]
        [Owner("Candice Daniel")]
        public void QueueEventsViewModel_Password_SetPassword()
        {
            var _passwordChanged = false;
            //------------Setup for test--------------------------
            var mockServer = new Mock<IServer>();
            var queueEventsViewModel = new QueueEventsViewModel(mockServer.Object);
            var queuedResourceForTest = new QueuedResourceForTest();
            queueEventsViewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "Password")
                {
                    _passwordChanged = true;
                }
            };
            queueEventsViewModel.SelectedQueue = queuedResourceForTest;
            queueEventsViewModel.ShowError(Warewolf.Studio.Resources.Languages.Core.QueueEventsLoginErrorMessage);
            Assert.AreEqual(Warewolf.Studio.Resources.Languages.Core.QueueEventsLoginErrorMessage, queueEventsViewModel.Error);
            //------------Execute Test---------------------------
            queueEventsViewModel.Password = "somePassword";
            //------------Assert Results-------------------------
            Assert.AreEqual("somePassword", queueEventsViewModel.Password);
            Assert.AreEqual("", queueEventsViewModel.Error);
            Assert.IsTrue(_passwordChanged);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(QueueEventsViewModel))]
        public void QueueEventsViewModel_AccountName_SetAccountName_SelectedTaskNull_NothingChangedOnTask()
        {
            //------------Setup for test--------------------------
            var _accountNameChanged = false;

            var mockServer = new Mock<IServer>();
            var queueEventsViewModel = new QueueEventsViewModel(mockServer.Object);
            var queuedResourceForTest = new QueuedResourceForTest();
            queueEventsViewModel.SelectedQueue = queuedResourceForTest;

            queueEventsViewModel.ShowError(Warewolf.Studio.Resources.Languages.Core.QueueEventsLoginErrorMessage);
            Assert.AreEqual(Warewolf.Studio.Resources.Languages.Core.QueueEventsLoginErrorMessage, queueEventsViewModel.Error);
            queueEventsViewModel.AccountName = "someAccountName";
            queueEventsViewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "AccountName")
                {
                    _accountNameChanged = true;
                }
            };
            //--------------Assert Preconditions------------------
            Assert.AreEqual("", queueEventsViewModel.Error);
            var queueResource = queueEventsViewModel.SelectedQueue;
            Assert.AreEqual("someAccountName", queueResource.UserName);
            //------------Execute Test---------------------------
            queueEventsViewModel.SelectedQueue = null;
            queueEventsViewModel.AccountName = "another account name";
            //------------Assert Results-------------------------
            Assert.IsNull(queueEventsViewModel.SelectedQueue);
            Assert.AreEqual("someAccountName", queueResource.UserName);
            Assert.IsFalse(_accountNameChanged);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(QueueEventsViewModel))]
        public void QueueEventsViewModel_ConnectionError_SetAndClearError_ValidErrorSetAndClear()
        {
            //------------Setup for test--------------------------
            var mockServer = new Mock<IServer>();
            var queueEventsViewModel = new QueueEventsViewModel(mockServer.Object);
            var queuedResourceForTest = new QueuedResourceForTest();
            queueEventsViewModel.SelectedQueue = queuedResourceForTest;
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
        [Owner("Candice Daniel")]
        [TestCategory(nameof(QueueEventsViewModel))]
        public void QueueEventsViewModel_History_Get_ShouldCallCreateHistoryOnQueueResourceModel()
        {
            //------------Setup for test--------------------------
            var queueEventsViewModel = new QueueEventsViewModel(new Mock<IServer>().Object);
            var queuedResourceForTest = new QueuedResourceForTest();
            queueEventsViewModel.SelectedQueue = queuedResourceForTest;

            var activeItem = new TabItem { Header = "History" };
            queueEventsViewModel.ActiveItem = activeItem;
            var mockQueueResourceModel = new Mock<IQueueResourceModel>();
            var histories = new List<IExecutionHistory> { new Mock<IExecutionHistory>().Object };
            mockQueueResourceModel.Setup(model => model.CreateHistory(It.IsAny<ITriggerQueue>())).Returns(histories);
            queueEventsViewModel.QueueResourceModel = mockQueueResourceModel.Object;
            queueEventsViewModel.SelectedQueue = new Mock<ITriggerQueueView>().Object;
            //------------Execute Test---------------------------
            var resourceHistories = queueEventsViewModel.History;
            //------------Assert Results-------------------------
          //  mockQueueResourceModel.Verify(model => model.CreateHistory(It.IsAny<IQueueResource>()), Times.Once());
            Assert.IsNotNull(resourceHistories);
            Assert.IsTrue(queueEventsViewModel.IsHistoryTabVisible);
            Assert.IsFalse(queueEventsViewModel.IsProgressBarVisible);
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(QueueEventsViewModel))]
        public void QueueEventsViewModel_Status_SetStatus()
        {
            //------------Setup for test--------------------------
            var queueEventsViewModel = new QueueEventsViewModel(new Mock<IServer>().Object);
            var queuedResourceForTest = new QueuedResourceForTest();
            queueEventsViewModel.SelectedQueue = queuedResourceForTest;
            //------------Execute Test---------------------------
            Assert.IsFalse(queueEventsViewModel.SelectedQueue.IsDirty);
            queueEventsViewModel.Status = QueueStatus.Disabled;
            //------------Assert Results-------------------------
            Assert.AreEqual(QueueStatus.Disabled, queueEventsViewModel.Status);

            queueEventsViewModel.Status = QueueStatus.Disabled;
            Assert.AreEqual(QueueStatus.Disabled, queueEventsViewModel.Status);
        }
        class QueuedResourceForTest : ITriggerQueueView
        {
            bool _isNewItem;
            bool _isDirty;
            string _queueName;

            public QueuedResourceForTest()
            {
                Errors = new ErrorResultTO();
            }

            public bool IsDirty
            {
                get => _isDirty;
                set => _isDirty = value;
            }
            public string Name { get; set; }
            public string OldName { get; set; }
            public QueueStatus Status { get; set; }
            public DateTime NextRunDate { get; set; }
            public int NumberOfHistoryToKeep { get; set; }
            public string WorkflowName { get; set; }
            public Guid ResourceId { get; set; }
            public string UserName { get; set; }
            public string Password { get; set; }
            public IErrorResultTO Errors { get; set; }
            public bool IsNew { get; set; }
            public bool IsNewItem
            {
                get => _isNewItem;
                set => _isNewItem = value;
            }
            public string NameForDisplay { get; private set; }
            public string QueueName
            {
                get => _queueName;
                set => _queueName = value;
            }

            public IResource QueueSource { get; set; }
            public int Concurrency { get; set; }
            public IOption[] Options { get; set; }
            public IResource QueueSink { get; set; }
            public string DeadLetterQueue { get; set; }
            public IOption[] DeadLetterOptions { get; set; }
            public ICollection<IServiceInput> Inputs { get; set; }

            public void SetItem(ITriggerQueue item)
            {
            }
            public bool Equals(ITriggerQueue other)
            {
                return !IsDirty;
            }
        }     
    }
}
