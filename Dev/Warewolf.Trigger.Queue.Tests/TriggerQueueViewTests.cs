/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Data.TO;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.Resources;
using Dev2.Common.Interfaces.Threading;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.ConnectionHelpers;
using Dev2.Core.Tests.Environments;
using Dev2.Data.Interfaces.Enums;
using Dev2.Studio.Core;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Models.DataList;
using Dev2.Studio.Interfaces;
using Dev2.Studio.ViewModels.DataList;
using Dev2.Threading;
using Dev2.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Warewolf.Options;
using Warewolf.UI;
using System.Collections.ObjectModel;
using Dev2.Studio.Interfaces.DataList;
using Warewolf.Triggers;
using Warewolf.Data;
using Warewolf.Auditing;

namespace Warewolf.Trigger.Queue.Tests
{
    [TestClass]
    [DoNotParallelize]//Ashley: Use of DI containers to decouple the UI
    public class TriggerQueueViewTests
    {
        Mock<IResource> _mockQueueSource;
        Guid _queueResourceId = Guid.NewGuid();

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
        TriggerQueueView CreateViewModel()
        {
            _mockQueueSource = new Mock<IResource>();
            _mockQueueSource.Setup(source => source.ResourceID).Returns(_queueResourceId);
            var queueSource2 = new Mock<IResource>();

            var expectedList = new List<IResource>
            {
                _mockQueueSource.Object, queueSource2.Object
            };

            var mockAsyncWorker = new Mock<IAsyncWorker>();

            var mockServer = new Mock<IServer>();
            var mockResourceRepository = new Mock<IResourceRepository>();
            mockResourceRepository.Setup(resourceRepository => resourceRepository.FindResourcesByType<IQueueSource>(mockServer.Object)).Returns(expectedList);
            mockResourceRepository.Setup(resourceRepository => resourceRepository.GetTriggerQueueHistory(Guid.NewGuid())).Returns(new List<IExecutionHistory>());
            mockResourceRepository.Setup(resourceRepository => resourceRepository.FindOptions(mockServer.Object, _mockQueueSource.Object)).Returns(SetupOptionsView());

            mockServer.Setup(server => server.ResourceRepository).Returns(mockResourceRepository.Object);
            return new TriggerQueueView(mockServer.Object, new SynchronousAsyncWorker());
        }
        [TestMethod]
        [TestCategory(nameof(TriggerQueueView))]
        [Owner("Pieter Terblanche")]
        public void TriggerQueueView_QueueSources()
        {
            var triggerQueueView = CreateViewModel();

            Assert.IsNotNull(triggerQueueView.QueueSources);
            Assert.IsNull(triggerQueueView.SelectedQueueSource);

            Assert.IsNotNull(triggerQueueView.QueueSources);
            Assert.AreEqual(2, triggerQueueView.QueueSources.Count);
        }

        [TestMethod]
        [TestCategory(nameof(TriggerQueueView))]
        [Owner("Pieter Terblanche")]
        public void TriggerQueueView_DeadLetterQueueSources()
        {
            var triggerQueueView = CreateViewModel();
            Assert.IsNotNull(triggerQueueView.DeadLetterQueueSources);
            Assert.IsNull(triggerQueueView.SelectedDeadLetterQueueSource);

            Assert.IsNotNull(triggerQueueView.DeadLetterQueueSources);
            Assert.AreEqual(2, triggerQueueView.DeadLetterQueueSources.Count);
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
        [TestCategory(nameof(TriggerQueueView))]
        [Owner("Pieter Terblanche")]
        public void TriggerQueueView_QueueNames()
        {
            var resourceId = Guid.NewGuid();
            var queueSource = new Mock<IResource>();
            queueSource.Setup(source => source.ResourceID).Returns(resourceId);

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
            mockResourceRepository.Setup(resourceRepository => resourceRepository.FindAutocompleteOptions(mockServer.Object, queueSource.Object)).Returns(expectedQueueNames);
            mockResourceRepository.Setup(resourceRepository => resourceRepository.FindOptions(mockServer.Object, queueSource.Object)).Returns(expectedOptions);

            mockServer.Setup(server => server.ResourceRepository).Returns(mockResourceRepository.Object);

            var triggerQueueView = new TriggerQueueView(mockServer.Object)
            {
                SelectedQueueSource = queueSource.Object
            };

            Assert.IsNotNull(triggerQueueView.SelectedQueueSource);
            Assert.AreEqual(queueSource.Object, triggerQueueView.SelectedQueueSource);
            Assert.AreEqual(resourceId, triggerQueueView.QueueSourceId);
            Assert.IsNotNull(triggerQueueView.QueueNames);
            Assert.AreEqual(0, triggerQueueView.QueueNames.Count);

            Assert.AreEqual(3, triggerQueueView.Options.Count);

            var optionOne = triggerQueueView.Options[0].DataContext as OptionBool;
            Assert.IsNotNull(optionOne);
            Assert.AreEqual("bool", optionOne.Name);
            Assert.IsFalse(optionOne.Value);
            Assert.IsTrue(optionOne.Default);

            var optionTwo = triggerQueueView.Options[1].DataContext as OptionInt;
            Assert.IsNotNull(optionTwo);
            Assert.AreEqual("int", optionTwo.Name);
            Assert.AreEqual(10, optionTwo.Value);
            Assert.AreEqual(0, optionTwo.Default);
        }

        [TestMethod]
        [TestCategory(nameof(TriggerQueueView))]
        [Owner("Pieter Terblanche")]
        public void TriggerQueueView_DeadLetterQueues()
        {
            var resourceId = Guid.NewGuid();
            var mockQueueSource = new Mock<IResource>();
            mockQueueSource.Setup(resource => resource.ResourceID).Returns(resourceId);

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
            mockResourceRepository.Setup(resourceRepository => resourceRepository.FindAutocompleteOptions(mockServer.Object, mockQueueSource.Object)).Returns(expectedQueueNames);
            mockResourceRepository.Setup(resourceRepository => resourceRepository.FindOptions(mockServer.Object, mockQueueSource.Object)).Returns(expectedOptions);

            mockServer.Setup(server => server.ResourceRepository).Returns(mockResourceRepository.Object);

            var triggerQueueView = new TriggerQueueView(mockServer.Object)
            {
                SelectedDeadLetterQueueSource = mockQueueSource.Object
            };

            Assert.IsNotNull(triggerQueueView.SelectedDeadLetterQueueSource);
            Assert.AreEqual(mockQueueSource.Object, triggerQueueView.SelectedDeadLetterQueueSource);
            Assert.AreEqual(resourceId, triggerQueueView.QueueSinkId);
            Assert.IsNotNull(triggerQueueView.DeadLetterQueues);
            Assert.AreEqual(0, triggerQueueView.DeadLetterQueues.Count);

            Assert.AreEqual(3, triggerQueueView.DeadLetterOptions.Count);

            var optionOne = triggerQueueView.DeadLetterOptions[0].DataContext as OptionBool;
            Assert.IsNotNull(optionOne);
            Assert.AreEqual("bool", optionOne.Name);
            Assert.IsFalse(optionOne.Value);
            Assert.IsTrue(optionOne.Default);

            var optionOneTemplate = triggerQueueView.DeadLetterOptions[0].DataTemplate;
            mockApplicationAdapter.Verify(model => model.TryFindResource("OptionBoolStyle"), Times.Once());

            var optionTwo = triggerQueueView.DeadLetterOptions[1].DataContext as OptionInt;
            Assert.IsNotNull(optionTwo);
            Assert.AreEqual("int", optionTwo.Name);
            Assert.AreEqual(10, optionTwo.Value);
            Assert.AreEqual(0, optionTwo.Default);

            var optionTwoTemplate = triggerQueueView.DeadLetterOptions[1].DataTemplate;
            mockApplicationAdapter.Verify(model => model.TryFindResource("OptionIntStyle"), Times.Once());

            var optionThree = triggerQueueView.DeadLetterOptions[2].DataContext as OptionAutocomplete;
            Assert.IsNotNull(optionThree);
            Assert.AreEqual("auto", optionThree.Name);
            Assert.AreEqual("new text", optionThree.Value);
            Assert.IsNull(optionThree.Suggestions);
            Assert.AreEqual("", optionThree.Default);

            var optionThreeTemplate = triggerQueueView.DeadLetterOptions[2].DataTemplate;
            mockApplicationAdapter.Verify(model => model.TryFindResource("OptionAutocompleteStyle"), Times.Once());
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(TriggerQueueView))]
        public void TriggerQueueView_Equals_Other_IsNull_Expect_False()
        {
            var triggerQueueView = CreateViewModel();
            var equals = triggerQueueView.Equals(null);
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(TriggerQueueView))]
        public void TriggerQueueView_ReferenceEquals_Match_Expect_True()
        {
            var mockServer = new Mock<IServer>();
            var triggerQueueView = new TriggerQueueView(mockServer.Object)
            {
                Concurrency = 1
            };
            var otherTriggerQueueView = triggerQueueView;
            var equals = triggerQueueView.Equals(otherTriggerQueueView);
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(TriggerQueueView))]
        public void TriggerQueueView_Equals_MisMatch_Expect_False()
        {
            var mockServer = new Mock<IServer>();

            var resourceId = Guid.NewGuid();
            var queueSinkResourceId = Guid.NewGuid();

            var mockOption = new Mock<IOption>();
            var option = new OptionViewForTesting(mockOption.Object);
            var mockInputs = new Mock<ICollection<IServiceInputBase>>();

            var triggerQueueView = new TriggerQueueView(mockServer.Object)
            {
                QueueSourceId = resourceId,
                QueueName = "Queue",
                WorkflowName = "Workflow",
                Concurrency = 100,
                UserName = "Bob",
                Password = "123456",
                Options = new ObservableCollection<OptionView> { option },
                QueueSinkId = queueSinkResourceId,
                DeadLetterQueue = "DeadLetterQueue",
                DeadLetterOptions = new ObservableCollection<OptionView> { option },
                Inputs = mockInputs.Object
            };

            var otherTriggerQueueView = new TriggerQueueView(mockServer.Object)
            {
                Concurrency = 2
            };
            var equals = triggerQueueView.Equals(otherTriggerQueueView);
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(TriggerQueueView))]
        public void TriggerQueueView_Defaults_For_Coverage_To_Remove()
        {
            var mockServer = new Mock<IServer>();
            var mockErrorResultTO = new Mock<IErrorResultTO>();
            var resourceId = Guid.NewGuid();
            var triggerQueueView = new TriggerQueueView(mockServer.Object)
            {
                TriggerId = resourceId,
                ResourceId = resourceId,
                OldQueueName = "OldName",
                Enabled = true,
                Errors = mockErrorResultTO.Object,
                TriggerQueueName = "TriggerQueueName",
                NameForDisplay = "NameForDisplay",
                IsNewQueue = true
            };

            Assert.AreEqual(resourceId, triggerQueueView.TriggerId);
            Assert.AreEqual(resourceId, triggerQueueView.ResourceId);
            Assert.AreEqual("OldName", triggerQueueView.OldQueueName);
            Assert.IsTrue(triggerQueueView.Enabled);
            Assert.IsNotNull(triggerQueueView.Errors);
            Assert.IsTrue(triggerQueueView.IsNewQueue);
            Assert.AreEqual("TriggerQueueName", triggerQueueView.TriggerQueueName);
            Assert.AreEqual("NameForDisplay", triggerQueueView.NameForDisplay);
            Assert.IsTrue(triggerQueueView.IsNewQueue);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(TriggerQueueView))]
        public void TriggerQueueView_Item_IsDirty_True()
        {
            var mockServer = new Mock<IServer>();
            var resourceId = Guid.NewGuid();
            var triggerQueueView = new TriggerQueueView(mockServer.Object)
            {
                ResourceId = resourceId,
                OldQueueName = "OldName",
                Enabled = true,
                TriggerQueueName = "TriggerQueueName",
                IsNewQueue = true
            };

            var triggerQueueViewItem = new TriggerQueueView(mockServer.Object)
            {
                ResourceId = resourceId,
                OldQueueName = "OldName",
                Enabled = true,
                TriggerQueueName = "TriggerQueueName",
                IsNewQueue = true,
                Item = triggerQueueView
            };

            Assert.AreEqual("TriggerQueueName *", triggerQueueViewItem.NameForDisplay);
        }

        [TestMethod]
        [TestCategory(nameof(TriggerQueueView))]
        [Owner("Pieter Terblanche")]
        public void TriggerQueueView_WorkflowName()
        {
            var triggerQueueView = CreateViewModel();

            Assert.IsNull(triggerQueueView.WorkflowName);

            var workflowName = "Workflow1";
            triggerQueueView.WorkflowName = workflowName;

            Assert.AreEqual(workflowName, triggerQueueView.WorkflowName);
        }

        [TestMethod]
        [TestCategory(nameof(TriggerQueueView))]
        [Owner("Pieter Terblanche")]
        public void TriggerQueueView_Concurrency()
        {
            var triggerQueueView = CreateViewModel();

            Assert.AreEqual(0, triggerQueueView.Concurrency);

            var concurrency = 5;
            triggerQueueView.Concurrency = concurrency;

            Assert.AreEqual(concurrency, triggerQueueView.Concurrency);
        }

        [TestMethod]
        [TestCategory(nameof(TriggerQueueView))]
        [Owner("Pieter Terblanche")]
        public void TriggerQueueView_Concurrency_Negative_NotAllowed()
        {
            var triggerQueueView = CreateViewModel();

            Assert.AreEqual(0, triggerQueueView.Concurrency);

            triggerQueueView.Concurrency = -5;

            Assert.AreEqual(0, triggerQueueView.Concurrency);
        }

        [TestMethod]
        [TestCategory(nameof(TriggerQueueView))]
        [Owner("Pieter Terblanche")]
        public void TriggerQueueView_UserName()
        {
            var triggerQueueView = CreateViewModel();

            //------------Execute Test---------------------------
            Assert.IsNull(triggerQueueView.UserName);

            triggerQueueView.UserName = "someAccountName";
            //------------Assert Results-------------------------
            Assert.AreEqual("someAccountName", triggerQueueView.UserName);
        }

        [TestMethod]
        [TestCategory(nameof(TriggerQueueView))]
        [Owner("Pieter Terblanche")]
        public void TriggerQueueView_Password()
        {
            var triggerQueueView = CreateViewModel();

            //------------Execute Test---------------------------
            Assert.IsNull(triggerQueueView.Password);
            triggerQueueView.Password = "somePassword";
            //------------Assert Results-------------------------
            Assert.AreEqual("somePassword", triggerQueueView.Password);
        }

        IContextualResourceModel CreateItemModelResourceModel(bool isConnected = true)
        {
            var moqModel = new Mock<IContextualResourceModel>();
            moqModel.SetupAllProperties();
            moqModel.Setup(model => model.DisplayName).Returns("My WF");
            moqModel.Setup(model => model.Environment.Connection.IsConnected).Returns(isConnected);
            moqModel.Setup(model => model.Environment.IsConnected).Returns(isConnected);
            moqModel.Setup(model => model.Environment.Connection.WebServerUri).Returns(new Uri("http://rsaklf/bob"));
            moqModel.Setup(model => model.Category).Returns("My WF");
            moqModel.Setup(model => model.Environment.IsLocalHost).Returns(isConnected);
            moqModel.Setup(model => model.ResourceName).Returns("My WF");

            var dataListViewModel = new DataListViewModel();
            dataListViewModel.InitializeDataListViewModel(moqModel.Object);
            dataListViewModel.Add(new ScalarItemModel("Name", enDev2ColumnArgumentDirection.Input));
            dataListViewModel.Add(new ScalarItemModel("Surname", enDev2ColumnArgumentDirection.Input));

            var recordSetItemModel = new RecordSetItemModel("Person", enDev2ColumnArgumentDirection.Input);
            var recordSetFieldItemModels = new ObservableCollection<IRecordSetFieldItemModel>
            {
                new RecordSetFieldItemModel("Name", recordSetItemModel, enDev2ColumnArgumentDirection.Input),
                new RecordSetFieldItemModel("Surname", recordSetItemModel, enDev2ColumnArgumentDirection.Input),
            };
            recordSetItemModel.Children = recordSetFieldItemModels;
            dataListViewModel.RecsetCollection.Add(recordSetItemModel);
            DataListSingleton.SetDataList(dataListViewModel);

            dataListViewModel.WriteToResourceModel();
            return moqModel.Object;
        }
        IContextualResourceModel CreateComplexItemModelResourceModel(bool isConnected = true)
        {
            var moqModel = new Mock<IContextualResourceModel>();
            moqModel.SetupAllProperties();
            moqModel.Setup(model => model.DisplayName).Returns("My WF");
            moqModel.Setup(model => model.Environment.Connection.IsConnected).Returns(isConnected);
            moqModel.Setup(model => model.Environment.IsConnected).Returns(isConnected);
            moqModel.Setup(model => model.Environment.Connection.WebServerUri).Returns(new Uri("http://rsaklf/bob"));
            moqModel.Setup(model => model.Category).Returns("My WF");
            moqModel.Setup(model => model.Environment.IsLocalHost).Returns(isConnected);
            moqModel.Setup(model => model.ResourceName).Returns("My WF");
            var dataListViewModel = new DataListViewModel();
            dataListViewModel.InitializeDataListViewModel(moqModel.Object);

            var complexObject = new ComplexObjectItemModel("Person", null, enDev2ColumnArgumentDirection.Input);
            complexObject.Children.Add(new ComplexObjectItemModel("Name", complexObject, enDev2ColumnArgumentDirection.Input));
            complexObject.Children.Add(new ComplexObjectItemModel("Surname", complexObject, enDev2ColumnArgumentDirection.Input));
            dataListViewModel.Add(complexObject);

            dataListViewModel.WriteToResourceModel();
            return moqModel.Object;
        }

        [TestMethod]
        [TestCategory(nameof(TriggerQueueView))]
        [Owner("Candice Daniel")]
        [DoNotParallelize]
        public void TriggerQueueView_QueueEvents_Get_Xml_Datalist_InputsFromWorkflow_VerifyCommand_Success()
        {
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.Show(StringResources.DataInput_Error, StringResources.DataInput_Error_Title, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error, string.Empty, false, true, false, false, false, false));
            CustomContainer.Register(popupController.Object);
            var mockServer = new Mock<IServer>();
            var mockErrorResultTO = new Mock<IErrorResultTO>();
            var contextualResourceModel = CreateItemModelResourceModel();
            var mockResourceRepository = new Mock<IResourceRepository>();
            mockResourceRepository.Setup(rr => rr.LoadContextualResourceModel(It.IsAny<Guid>())).Returns(contextualResourceModel);
            mockServer.Setup(server => server.ResourceRepository).Returns(mockResourceRepository.Object);

            var triggerQueueView = new TriggerQueueView(mockServer.Object)
            {
                TriggerId = Guid.NewGuid(),
                ResourceId = contextualResourceModel.ID,
                QueueSourceId = Guid.NewGuid(),
                Errors = mockErrorResultTO.Object,
                QueueName = "Queue",
                WorkflowName = "Workflow"
            };
            Assert.IsNotNull(triggerQueueView.Errors);
            Assert.IsNull(triggerQueueView.VerifyResults);
            Assert.IsFalse(triggerQueueView.IsVerifying);
            Assert.IsFalse(triggerQueueView.IsVerifyResultsEmptyRows);

            triggerQueueView.VerifyResults = "<DataList><Name>Test</Name><Surname>test1</Surname><Person><Name>sdas</Name><Surname>asdsad</Surname></Person></DataList>";
            triggerQueueView.GetInputsFromWorkflow();
            triggerQueueView.VerifyCommand.Execute(null);

            Assert.AreEqual(4, triggerQueueView.Inputs.Count);

            var inputs = triggerQueueView.Inputs.ToList();
            Assert.AreEqual("Name", inputs[0].Name);
            Assert.AreEqual("Test", inputs[0].Value);

            Assert.AreEqual("Surname", inputs[1].Name);
            Assert.AreEqual("test1", inputs[1].Value);

            Assert.AreEqual("Person(1).Name", inputs[2].Name);
            Assert.AreEqual("sdas", inputs[2].Value);

            Assert.AreEqual("Person(1).Surname", inputs[3].Name);
            Assert.AreEqual("asdsad", inputs[3].Value);

            Assert.IsTrue(triggerQueueView.VerifyResultsAvailable);
            Assert.IsFalse(triggerQueueView.IsVerifyResultsEmptyRows);
            Assert.IsFalse(triggerQueueView.IsVerifying);
            Assert.IsTrue(triggerQueueView.VerifyPassed);
            Assert.IsFalse(triggerQueueView.VerifyFailed);

            popupController.Verify(controller => controller.Show(StringResources.DataInput_Error, StringResources.DataInput_Error_Title, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error, string.Empty, false, true, false, false, false, false), Times.Never);
        }

        [TestMethod]
        [TestCategory(nameof(TriggerQueueView))]
        [Owner("Candice Daniel")]
        [DoNotParallelize]
        public void TriggerQueueView_QueueEvents_Get_Xml_DataList_InputsFromWorkflow_VerifyCommand_InvalidData_ShowInvalidDataPopupMessage()
        {
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.Show(StringResources.DataInput_Error, StringResources.DataInput_Error_Title, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error, string.Empty, false, true, false, false, false, false));
            CustomContainer.Register(popupController.Object);

            var mockServer = new Mock<IServer>();
            var mockErrorResultTO = new Mock<IErrorResultTO>();
            var contextualResourceModel = CreateItemModelResourceModel();
            var mockResourceRepository = new Mock<IResourceRepository>();
            mockResourceRepository.Setup(rr => rr.LoadContextualResourceModel(It.IsAny<Guid>())).Returns(contextualResourceModel);
            mockServer.Setup(server => server.ResourceRepository).Returns(mockResourceRepository.Object);

            var triggerQueueView = new TriggerQueueView(mockServer.Object)
            {
                TriggerId = Guid.NewGuid(),
                ResourceId = contextualResourceModel.ID,
                QueueSourceId = Guid.NewGuid(),
                QueueName = "Queue",
                WorkflowName = "Workflow"
            };
            Assert.IsNull(triggerQueueView.VerifyResults);
            Assert.IsFalse(triggerQueueView.IsVerifying);
            Assert.IsFalse(triggerQueueView.IsVerifyResultsEmptyRows);

            triggerQueueView.VerifyResults = "<DataList><Name>asdasd</Name>asdasdasd</Surname><bob><name>sdas</name><surname>asdsad</surname></bob><Person><Name></Name><Surname></Surname></Person></DataList>";
            triggerQueueView.GetInputsFromWorkflow();
            triggerQueueView.VerifyCommand.Execute(null);

            Assert.IsFalse(triggerQueueView.VerifyResultsAvailable);
            Assert.IsFalse(triggerQueueView.IsVerifyResultsEmptyRows);
            Assert.IsFalse(triggerQueueView.IsVerifying);
            Assert.IsFalse(triggerQueueView.VerifyPassed);
            Assert.IsTrue(triggerQueueView.VerifyFailed);
            popupController.Verify(controller => controller.Show(StringResources.DataInput_Error, StringResources.DataInput_Error_Title, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error, string.Empty, false, true, false, false, false, false), Times.Once);
        }

        [TestMethod]
        [TestCategory(nameof(TriggerQueueView))]
        [Owner("Candice Daniel")]
        public void TriggerQueueView_QueueEvents_Get_Json_Datalist_InputsFromWorkflow_VerifyCommand_Success()
        {
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.Show(StringResources.DataInput_Error, StringResources.DataInput_Error_Title, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error, string.Empty, false, true, false, false, false, false));
            CustomContainer.Register(popupController.Object);
            var mockServer = new Mock<IServer>();
            var contextualResourceModel = CreateItemModelResourceModel();
            var mockResourceRepository = new Mock<IResourceRepository>();
            mockResourceRepository.Setup(rr => rr.LoadContextualResourceModel(It.IsAny<Guid>())).Returns(contextualResourceModel);
            mockServer.Setup(server => server.ResourceRepository).Returns(mockResourceRepository.Object);

            var triggerQueueView = new TriggerQueueView(mockServer.Object)
            {
                TriggerId = Guid.NewGuid(),
                ResourceId = contextualResourceModel.ID,
                QueueSourceId = Guid.NewGuid(),
                QueueName = "Queue",
                WorkflowName = "Workflow"
            };

            Assert.IsNull(triggerQueueView.VerifyResults);
            Assert.IsFalse(triggerQueueView.IsVerifying);
            Assert.IsFalse(triggerQueueView.IsVerifyResultsEmptyRows);

            var json = "{\"Name\": \"test\",\"Surname\": \"test\" }";
            triggerQueueView.VerifyResults = json;
            triggerQueueView.GetInputsFromWorkflow();
            triggerQueueView.VerifyCommand.Execute(null);

            popupController.Verify(controller => controller.Show(StringResources.DataInput_Error, StringResources.DataInput_Error_Title, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error, string.Empty, false, true, false, false, false, false), Times.Never);

            Assert.IsTrue(triggerQueueView.VerifyResultsAvailable);
            Assert.IsFalse(triggerQueueView.IsVerifyResultsEmptyRows);
            Assert.IsFalse(triggerQueueView.IsVerifying);
            Assert.IsTrue(triggerQueueView.VerifyPassed);
            Assert.IsFalse(triggerQueueView.VerifyFailed);
        }

        [TestMethod]
        [TestCategory(nameof(TriggerQueueView))]
        [Owner("Candice Daniel")]
        public void TriggerQueueView_QueueEvents_Get_ComplexObject_InputsFromWorkflow_VerifyCommand_Success()
        {
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.Show(StringResources.DataInput_Error, StringResources.DataInput_Error_Title, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error, string.Empty, false, true, false, false, false, false));
            CustomContainer.Register(popupController.Object);
            var mockServer = new Mock<IServer>();
            var contextualResourceModel = CreateComplexItemModelResourceModel();
            var mockResourceRepository = new Mock<IResourceRepository>();
            mockResourceRepository.Setup(rr => rr.LoadContextualResourceModel(It.IsAny<Guid>())).Returns(contextualResourceModel);
            mockServer.Setup(server => server.ResourceRepository).Returns(mockResourceRepository.Object);

            var triggerQueueView = new TriggerQueueView(mockServer.Object)
            {
                TriggerId = Guid.NewGuid(),
                ResourceId = contextualResourceModel.ID,
                QueueSourceId = Guid.NewGuid(),
                QueueName = "Queue",
                WorkflowName = "Workflow"
            };

            Assert.IsNull(triggerQueueView.VerifyResults);
            Assert.IsFalse(triggerQueueView.IsVerifying);
            Assert.IsFalse(triggerQueueView.IsVerifyResultsEmptyRows);

            var json = "{\"Person\": {\"Name\": \"test\",\"Surname\": \"test\" }}";
            triggerQueueView.VerifyResults = json;
            triggerQueueView.GetInputsFromWorkflow();
            triggerQueueView.VerifyCommand.Execute(null);

            Assert.AreEqual(1, triggerQueueView.Inputs.Count);
            var inputs = triggerQueueView.Inputs.ToList();
            Assert.AreEqual("@Person", inputs[0].Name);
            Assert.AreEqual("{\r\n  \"Name\": \"test\",\r\n  \"Surname\": \"test\"\r\n}", inputs[0].Value);

            Assert.IsTrue(triggerQueueView.VerifyResultsAvailable);
            Assert.IsFalse(triggerQueueView.IsVerifyResultsEmptyRows);
            Assert.IsFalse(triggerQueueView.IsVerifying);
            Assert.IsTrue(triggerQueueView.VerifyPassed);
            Assert.IsFalse(triggerQueueView.VerifyFailed);

            popupController.Verify(controller => controller.Show(StringResources.DataInput_Error, StringResources.DataInput_Error_Title, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error, string.Empty, false, true, false, false, false, false), Times.Never);
        }

        [TestMethod]
        [TestCategory(nameof(TriggerQueueView))]
        [Owner("Pieter Terblanche")]
        public void TriggerQueueView_History_IsHistoryExpanded_False()
        {
            var triggerQueueView = CreateViewModel();

            Assert.IsFalse(triggerQueueView.IsHistoryExpanded);
            Assert.AreEqual(0, triggerQueueView.History.Count);
        }

        [TestMethod]
        [TestCategory(nameof(TriggerQueueView))]
        [Owner("Pieter Terblanche")]
        public void TriggerQueueView_History_IsHistoryExpanded_True()
        {
            var resourceId = Guid.NewGuid();

            var executionInfo = new ExecutionInfo(DateTime.Now, DateTime.Now-DateTime.UtcNow, DateTime.Today, QueueRunStatus.Success, Guid.NewGuid(),"");

            var history = new List<IExecutionHistory>
            {
                new ExecutionHistory(resourceId, "output", executionInfo, "username")
            };

            var mockServer = new Mock<IServer>();
            var mockResourceRepository = new Mock<IResourceRepository>();
            mockResourceRepository.Setup(resourceRepository => resourceRepository.GetTriggerQueueHistory(resourceId)).Returns(history);

            mockServer.Setup(server => server.ResourceRepository).Returns(mockResourceRepository.Object);

            var triggerQueueView = new TriggerQueueView(mockServer.Object, new SynchronousAsyncWorker())
            {
                TriggerId = resourceId,
                ResourceId = resourceId,
                IsHistoryExpanded = true
            };

            Assert.IsNotNull(triggerQueueView.History);
            Assert.AreEqual(1, triggerQueueView.History.Count);
            Assert.IsFalse(triggerQueueView.IsProgressBarVisible);
            mockResourceRepository.Verify(resourceRepository => resourceRepository.GetTriggerQueueHistory(resourceId), Times.Exactly(1));
        }

        [TestMethod]
        [TestCategory(nameof(TriggerQueueView))]
        [Owner("Pieter Terblanche")]
        public void TriggerQueueView_MapEntireMessage()
        {
            var triggerQueueView = CreateViewModel();

            Assert.IsTrue(triggerQueueView.MapEntireMessage);

            triggerQueueView.MapEntireMessage = false;

            Assert.IsFalse(triggerQueueView.MapEntireMessage);
        }

        [TestMethod]
        [TestCategory(nameof(TriggerQueueView))]
        [Owner("Pieter Terblanche")]
        public void TriggerQueueView_ToModel()
        {
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

            var triggerQueueView = CreateViewModel();

            triggerQueueView.ToModel(triggerQueue);

            Assert.AreEqual(triggerQueue.Name, triggerQueueView.TriggerQueueName);
            Assert.AreEqual(triggerQueue.QueueSourceId, triggerQueueView.SelectedQueueSource.ResourceID);
            Assert.AreEqual(triggerQueue.QueueName, triggerQueueView.QueueName);
            Assert.AreEqual(triggerQueue.WorkflowName, triggerQueueView.WorkflowName);
            Assert.AreEqual(triggerQueue.Concurrency, triggerQueueView.Concurrency);
            Assert.AreEqual(triggerQueue.UserName, triggerQueueView.UserName);
            Assert.AreEqual(triggerQueue.Password, triggerQueueView.Password);
            Assert.AreEqual(triggerQueue.Options.Count(), triggerQueueView.Options.Count);
            Assert.AreEqual(triggerQueue.QueueSinkId, triggerQueueView.SelectedDeadLetterQueueSource.ResourceID);
            Assert.AreEqual(triggerQueue.DeadLetterQueue, triggerQueueView.DeadLetterQueue);
            Assert.AreEqual(triggerQueue.DeadLetterOptions.Count(), triggerQueueView.DeadLetterOptions.Count);
            Assert.AreEqual(triggerQueue.Inputs.Count, triggerQueueView.Inputs.Count);
        }

        [TestMethod]
        [TestCategory(nameof(TriggerQueueView))]
        [Owner("Pieter Terblanche")]
        public void TriggerQueueView_SetItem()
        {
            var mockServer = new Mock<IServer>();

            var resourceId = Guid.NewGuid();
            var queueSinkResourceId = Guid.NewGuid();

            var mockOption = new Mock<IOption>();
            var option = new OptionViewForTesting(mockOption.Object);
            var mockInputs = new Mock<ICollection<IServiceInputBase>>();

            var triggerQueueView = new TriggerQueueView(mockServer.Object)
            {
                QueueSourceId = resourceId,
                QueueName = "Queue",
                WorkflowName = "Workflow",
                Concurrency = 100,
                UserName = "Bob",
                Password = "123456",
                Options = new ObservableCollection<OptionView> { option },
                QueueSinkId = queueSinkResourceId,
                DeadLetterQueue = "DeadLetterQueue",
                DeadLetterOptions = new ObservableCollection<OptionView> { option },
                Inputs = mockInputs.Object
            };

            triggerQueueView.SetItem();

            Assert.AreEqual(triggerQueueView.QueueSourceId, triggerQueueView.Item.QueueSourceId);
            Assert.AreEqual(triggerQueueView.QueueName, triggerQueueView.Item.QueueName);
            Assert.AreEqual(triggerQueueView.WorkflowName, triggerQueueView.Item.WorkflowName);
            Assert.AreEqual(triggerQueueView.Concurrency, triggerQueueView.Item.Concurrency);
            Assert.AreEqual(triggerQueueView.UserName, triggerQueueView.Item.UserName);
            Assert.AreEqual(triggerQueueView.Password, triggerQueueView.Item.Password);
            Assert.AreEqual(triggerQueueView.Options.Count, triggerQueueView.Item.Options.Count);
            Assert.AreEqual(triggerQueueView.QueueSinkId, triggerQueueView.Item.QueueSinkId);
            Assert.AreEqual(triggerQueueView.DeadLetterQueue, triggerQueueView.Item.DeadLetterQueue);
            Assert.AreEqual(triggerQueueView.DeadLetterOptions.Count, triggerQueueView.Item.DeadLetterOptions.Count);
            Assert.AreEqual(triggerQueueView.Inputs.Count, triggerQueueView.Item.Inputs.Count);
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
