using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Dev2.Studio.Interfaces;
using System.Text;
using System.Windows;
using Dev2.Common.Interfaces.Studio.Controller;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using System.Collections.Generic;
using System;
using Warewolf.MergeParser;
using Dev2.Activities;
using Dev2.Communication;
using Dev2.Common.Interfaces.Infrastructure.Events;
using Dev2.Common.Interfaces;
using Dev2.Common;
using System.Linq;
using Dev2.Common.ExtMethods;
using Dev2.Studio.Interfaces.DataList;
using Dev2.Studio.Core.Interfaces;
using Dev2.ViewModels.Merge;

namespace Dev2.Core.Tests
{
    [TestClass]
    public class MergeWorkflowViewModelTests
    {
        [TestInitialize]
        public void InitializeTest()
        {
            var mockApplicationAdapter = new Mock<IApplicationAdaptor>();
            var mockPopupController = new Mock<IPopupController>();
            var mockServer = new Mock<IServer>();
            var mockShellViewModel = new Mock<IShellViewModel>();
            var mockServerRepository = new Mock<IServerRepository>();

            mockApplicationAdapter.Setup(a => a.Current).Returns(Application.Current);

            mockServer.Setup(a => a.GetServerVersion()).Returns("1.0.0.0");
            mockShellViewModel.Setup(a => a.ActiveServer).Returns(mockServer.Object);

            mockServerRepository.Setup(a => a.ActiveServer).Returns(mockServer.Object);
            mockServerRepository.Setup(a => a.IsLoaded).Returns(true);

            CustomContainer.Register(mockApplicationAdapter.Object);
            CustomContainer.Register(mockPopupController.Object);
            CustomContainer.Register(mockServer.Object);
            CustomContainer.Register(mockShellViewModel.Object);
            CustomContainer.Register(mockServerRepository.Object);
            CustomContainer.Register<IActivityParser>(new ActivityParser());
            CustomContainer.Register<IServiceDifferenceParser>(new ServiceDifferenceParser());

        }

        [TestMethod]
        public void Initialize_GivenIsNewNoEmptyConflicts_ShouldSetCurrentObjects()
        {
            //---------------Set up test pack-------------------
            var assignExample = XML.XmlResource.Fetch("Utility - Assign");
            var jsonSerializer = new Dev2JsonSerializer();
            var currentResourceModel = Dev2MockFactory.SetupResourceModelMock();
            var assignExampleBuilder = new StringBuilder(assignExample.ToString(System.Xml.Linq.SaveOptions.DisableFormatting));
            currentResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(assignExampleBuilder);
            IResourceDefinationCleaner resourceDefination = new ResourceDefinationCleaner();
            var cleanDef = resourceDefination.GetResourceDefinition(true, currentResourceModel.Object.ID, assignExampleBuilder);
            var msg = jsonSerializer.Deserialize<ExecuteMessage>(cleanDef);
            currentResourceModel.Setup(resModel => resModel.DisplayName).Returns("Hello World");
            currentResourceModel.Setup(resModel => resModel.Environment.ResourceRepository.FetchResourceDefinition(It.IsAny<IServer>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>()))
                .Returns(new ExecuteMessage { Message = msg.Message, HasError = false });

            currentResourceModel.Setup(p => p.Environment.Connection.ServerEvents).Returns(new Mock<IEventPublisher>().Object);


            var differenceResourceModel = Dev2MockFactory.SetupResourceModelMock();
            differenceResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(assignExampleBuilder);
            differenceResourceModel.Setup(resModel => resModel.DisplayName).Returns("Hello World");

            var mergeWorkflowViewModel = new MergeWorkflowViewModel(currentResourceModel.Object, differenceResourceModel.Object, false);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.IsNotNull(mergeWorkflowViewModel.CurrentConflictModel);
            Assert.IsNotNull(mergeWorkflowViewModel.DifferenceConflictModel);
        }

        [TestMethod]
        public void Initialize_GivenHasConflicts_ShouldPassThrough()
        {
            //---------------Set up test pack-------------------
            var applicationAdapter = new Mock<IApplicationAdaptor>();
            applicationAdapter.Setup(a => a.Current).Returns(Application.Current);
            CustomContainer.Register(applicationAdapter.Object);

            var mockServer = new Mock<IServer>();
            var mockshell = new Mock<IShellViewModel>();
            mockshell.Setup(a => a.ActiveServer).Returns(mockServer.Object);
            mockServer.Setup(a => a.GetServerVersion()).Returns("1.0.0.0");
            CustomContainer.Register(mockServer.Object);
            CustomContainer.Register(mockshell.Object);

            var currentResourceModel = Dev2MockFactory.SetupResourceModelMock();
            var assignExample = XML.XmlResource.Fetch("Utility - Assign");
            var assignExampleBuilder = new StringBuilder(assignExample.ToString(System.Xml.Linq.SaveOptions.DisableFormatting));
            currentResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(assignExampleBuilder);
            IResourceDefinationCleaner resourceDefination = new ResourceDefinationCleaner();
            var jsonSerializer = new Dev2JsonSerializer();
            var cleanDef = resourceDefination.GetResourceDefinition(true, currentResourceModel.Object.ID, assignExampleBuilder);
            var msg = jsonSerializer.Deserialize<ExecuteMessage>(cleanDef);
            currentResourceModel.Setup(resModel => resModel.DisplayName).Returns("Hello World");
            currentResourceModel.Setup(resModel => resModel.Environment.ResourceRepository.FetchResourceDefinition(It.IsAny<IServer>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>()))
              .Returns(new ExecuteMessage { Message = msg.Message, HasError = false });
            currentResourceModel.Setup(p => p.Environment.Connection.ServerEvents).Returns(new Mock<IEventPublisher>().Object);

            var assignExampleDiff = XML.XmlResource.Fetch("Utility - Assign_Confilct");
            var differenceResourceModel = Dev2MockFactory.SetupResourceModelMock();
            differenceResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(new StringBuilder(assignExampleDiff.ToString(System.Xml.Linq.SaveOptions.DisableFormatting)));
            differenceResourceModel.Setup(resModel => resModel.DisplayName).Returns("Hello World");

            var mergeWorkflowViewModel = new MergeWorkflowViewModel(currentResourceModel.Object, differenceResourceModel.Object, false);
            //---------------Assert Precondition----------------
            Assert.AreNotSame(currentResourceModel, differenceResourceModel);
            //---------------Execute Test ----------------------
            Assert.IsNotNull(mergeWorkflowViewModel.CurrentConflictModel);
            Assert.IsNotNull(mergeWorkflowViewModel.DifferenceConflictModel);
            //---------------Test Result -----------------------
            var mergeToolModels = mergeWorkflowViewModel.CurrentConflictModel;
            var differenceViewModel = mergeWorkflowViewModel.DifferenceConflictModel;
            Assert.AreNotSame(mergeToolModels, differenceViewModel);
        }

        [TestMethod]
        public void Initialize_GivenHasConflictsAndForEach_ShouldPassThrough()
        {
            //---------------Set up test pack-------------------
            var applicationAdapter = new Mock<IApplicationAdaptor>();
            applicationAdapter.Setup(a => a.Current).Returns(Application.Current);
            CustomContainer.Register(applicationAdapter.Object);

            var mockServer = new Mock<IServer>();
            var mockshell = new Mock<IShellViewModel>();
            mockshell.Setup(a => a.ActiveServer).Returns(mockServer.Object);
            mockServer.Setup(a => a.GetServerVersion()).Returns("1.0.0.0");
            CustomContainer.Register(mockServer.Object);
            CustomContainer.Register(mockshell.Object);
            var assignExample = XML.XmlResource.Fetch("Loop Constructs - For Each");
            var resourceDefinationCleaner = new ResourceDefinationCleaner();
            var resource = resourceDefinationCleaner.GetResourceDefinition(true, Guid.Empty, new StringBuilder(assignExample.ToString(System.Xml.Linq.SaveOptions.DisableFormatting)));
            var dev2JsonSerializer = new Dev2JsonSerializer();
            var msg = dev2JsonSerializer.Deserialize<ExecuteMessage>(resource);

            var currentResourceModel = Dev2MockFactory.SetupResourceModelMock();
            currentResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(msg.Message);
            currentResourceModel.Setup(resModel => resModel.DisplayName).Returns("Hello World");
            currentResourceModel.Setup(resModel => resModel.Environment.ResourceRepository.FetchResourceDefinition(It.IsAny<IServer>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>()))
              .Returns(new ExecuteMessage { Message = msg.Message, HasError = false });
            currentResourceModel.Setup(p => p.Environment.Connection.ServerEvents).Returns(new Mock<IEventPublisher>().Object);

            var differenceResourceModel = Dev2MockFactory.SetupResourceModelMock();
            differenceResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(msg.Message);
            differenceResourceModel.Setup(resModel => resModel.DisplayName).Returns("Hello World");

            var mergeWorkflowViewModel = new MergeWorkflowViewModel(currentResourceModel.Object, differenceResourceModel.Object, false);
            //---------------Assert Precondition----------------
            Assert.AreNotSame(currentResourceModel, differenceResourceModel);
            //---------------Execute Test ----------------------
            Assert.IsNotNull(mergeWorkflowViewModel.CurrentConflictModel);
            Assert.IsNotNull(mergeWorkflowViewModel.DifferenceConflictModel);
            //---------------Test Result -----------------------
            Assert.AreEqual(13, mergeWorkflowViewModel.Conflicts.Count);
        }

        [TestMethod]
        public void ProcessCurrent_GivenEmptyConflicts_ShouldAddIntoMainConflictList()
        {
            //---------------Set up test pack-------------------
            var applicationAdapter = new Mock<IApplicationAdaptor>();
            CustomContainer.Register(applicationAdapter.Object);

            var mockServer = new Mock<IServer>();
            var mockshell = new Mock<IShellViewModel>();
            mockshell.Setup(a => a.ActiveServer).Returns(mockServer.Object);
            mockServer.Setup(a => a.GetServerVersion()).Returns("1.0.0.0");
            CustomContainer.Register(mockServer.Object);
            CustomContainer.Register(mockshell.Object);
            var assignExample = XML.XmlResource.Fetch("Loop Constructs - For Each");
            var resourceDefinationCleaner = new ResourceDefinationCleaner();
            var resource = resourceDefinationCleaner.GetResourceDefinition(true, Guid.Empty, new StringBuilder(assignExample.ToString(System.Xml.Linq.SaveOptions.DisableFormatting)));
            var dev2JsonSerializer = new Dev2JsonSerializer();
            var msg = dev2JsonSerializer.Deserialize<ExecuteMessage>(resource);

            var currentResourceModel = Dev2MockFactory.SetupResourceModelMock();
            currentResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(msg.Message);
            currentResourceModel.Setup(resModel => resModel.DisplayName).Returns("Hello World");
            currentResourceModel.Setup(resModel => resModel.Environment.ResourceRepository.FetchResourceDefinition(It.IsAny<IServer>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>()))
              .Returns(new ExecuteMessage { Message = msg.Message, HasError = false });
            currentResourceModel.Setup(p => p.Environment.Connection.ServerEvents).Returns(new Mock<IEventPublisher>().Object);

            var differenceResourceModel = Dev2MockFactory.SetupResourceModelMock();
            differenceResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(msg.Message);
            differenceResourceModel.Setup(resModel => resModel.DisplayName).Returns("Hello World");

            var mergeWorkflowViewModel = new MergeWorkflowViewModel(currentResourceModel.Object, differenceResourceModel.Object, false);
            var methodToRun = typeof(MergeWorkflowViewModel).GetMethod("ProcessCurrentItem", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(methodToRun);
            //ConflictTreeNode(IDev2Activity act, Point location)
            //---------------Execute Test ----------------------
            var conflicts = new List<IConflict>();
            var currentTree = new List<ConflictTreeNode>();
            currentTree.Add(new ConflictTreeNode(new DsfCalculateActivity(), new Point()));
            var armConnectorConflicts = new List<IArmConnectorConflict>();
            methodToRun.Invoke(mergeWorkflowViewModel, new object[] { currentResourceModel.Object, conflicts, armConnectorConflicts, currentTree[0] });
            //---------------Test Result -----------------------
            Assert.AreEqual(1, conflicts.Count);
            Assert.AreEqual(1, currentTree.Count);
        }

        [TestMethod]
        public void ProcessDiff_GivenEmptyConflicts_ShouldAddIntoMainConflictList()
        {
            //---------------Set up test pack-------------------
            var applicationAdapter = new Mock<IApplicationAdaptor>();
            CustomContainer.Register(applicationAdapter.Object);

            var mockServer = new Mock<IServer>();
            var mockshell = new Mock<IShellViewModel>();
            mockshell.Setup(a => a.ActiveServer).Returns(mockServer.Object);
            mockServer.Setup(a => a.GetServerVersion()).Returns("1.0.0.0");
            CustomContainer.Register(mockServer.Object);
            CustomContainer.Register(mockshell.Object);
            var assignExample = XML.XmlResource.Fetch("Loop Constructs - For Each");
            var resourceDefinationCleaner = new ResourceDefinationCleaner();
            var resource = resourceDefinationCleaner.GetResourceDefinition(true, Guid.Empty, new StringBuilder(assignExample.ToString(System.Xml.Linq.SaveOptions.DisableFormatting)));
            var dev2JsonSerializer = new Dev2JsonSerializer();
            var msg = dev2JsonSerializer.Deserialize<ExecuteMessage>(resource);

            var currentResourceModel = Dev2MockFactory.SetupResourceModelMock();
            currentResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(msg.Message);
            currentResourceModel.Setup(resModel => resModel.DisplayName).Returns("Hello World");
            currentResourceModel.Setup(resModel => resModel.Environment.ResourceRepository.FetchResourceDefinition(It.IsAny<IServer>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>()))
              .Returns(new ExecuteMessage { Message = msg.Message, HasError = false });
            currentResourceModel.Setup(p => p.Environment.Connection.ServerEvents).Returns(new Mock<IEventPublisher>().Object);

            var differenceResourceModel = Dev2MockFactory.SetupResourceModelMock();
            differenceResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(msg.Message);
            differenceResourceModel.Setup(resModel => resModel.DisplayName).Returns("Hello World");

            var mergeWorkflowViewModel = new MergeWorkflowViewModel(currentResourceModel.Object, differenceResourceModel.Object, false);
            var methodToRun = typeof(MergeWorkflowViewModel).GetMethod("ProcessDiffItem", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(methodToRun);
            //---------------Execute Test ----------------------
            var conflicts = new List<IConflict>();
            var currentTree = new List<ConflictTreeNode>();
            currentTree.Add(new ConflictTreeNode(new DsfCalculateActivity(), new Point()));
            var armConnectorConflicts = new List<IArmConnectorConflict>();
            methodToRun.Invoke(mergeWorkflowViewModel, new object[] { currentResourceModel.Object, conflicts, armConnectorConflicts, currentTree[0] });
            //---------------Test Result -----------------------
            Assert.AreEqual(1, conflicts.Count);
            Assert.AreEqual(1, currentTree.Count);
        }

        [TestMethod]
        public void ShowArmConnectors_GivenEmptyConflicts_PassThrough()
        {
            //---------------Set up test pack-------------------
            var applicationAdapter = new Mock<IApplicationAdaptor>();
            CustomContainer.Register(applicationAdapter.Object);

            var mockServer = new Mock<IServer>();
            var mockshell = new Mock<IShellViewModel>();
            mockshell.Setup(a => a.ActiveServer).Returns(mockServer.Object);
            mockServer.Setup(a => a.GetServerVersion()).Returns("1.0.0.0");
            CustomContainer.Register(mockServer.Object);
            CustomContainer.Register(mockshell.Object);
            var assignExample = XML.XmlResource.Fetch("Loop Constructs - For Each");
            var resourceDefinationCleaner = new ResourceDefinationCleaner();
            var resource = resourceDefinationCleaner.GetResourceDefinition(true, Guid.Empty, new StringBuilder(assignExample.ToString(System.Xml.Linq.SaveOptions.DisableFormatting)));
            var dev2JsonSerializer = new Dev2JsonSerializer();
            var msg = dev2JsonSerializer.Deserialize<ExecuteMessage>(resource);

            var currentResourceModel = Dev2MockFactory.SetupResourceModelMock();
            currentResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(msg.Message);
            currentResourceModel.Setup(resModel => resModel.DisplayName).Returns("Hello World");
            currentResourceModel.Setup(resModel => resModel.Environment.ResourceRepository.FetchResourceDefinition(It.IsAny<IServer>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>()))
              .Returns(new ExecuteMessage { Message = msg.Message, HasError = false });
            currentResourceModel.Setup(p => p.Environment.Connection.ServerEvents).Returns(new Mock<IEventPublisher>().Object);

            var differenceResourceModel = Dev2MockFactory.SetupResourceModelMock();
            differenceResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(msg.Message);
            differenceResourceModel.Setup(resModel => resModel.DisplayName).Returns("Hello World");

            using (var mergeWorkflowViewModel = new MergeWorkflowViewModel(currentResourceModel.Object, differenceResourceModel.Object, false))
            {
                var methodToRun = typeof(MergeWorkflowViewModel).GetMethod("ShowArmConnectors", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                //---------------Assert Precondition----------------
                Assert.IsNotNull(methodToRun);
                //---------------Execute Test ----------------------
                var conflicts = new List<IConflict>
                                {
                                    new ArmConnectorConflict()
                                };
                var armConnectorConflicts = new List<IArmConnectorConflict>();
                methodToRun.Invoke(null, new object[] { conflicts, armConnectorConflicts });
                //---------------Test Result -----------------------
                Assert.AreEqual(1, conflicts.Count);
            }
        }

        [TestMethod]
        public void FindMatchingConnector_GivenSameIds_Returnstrue()
        {
            //---------------Set up test pack-------------------
            var applicationAdapter = new Mock<IApplicationAdaptor>();
            CustomContainer.Register(applicationAdapter.Object);

            var mockServer = new Mock<IServer>();
            var mockshell = new Mock<IShellViewModel>();
            mockshell.Setup(a => a.ActiveServer).Returns(mockServer.Object);
            mockServer.Setup(a => a.GetServerVersion()).Returns("1.0.0.0");
            CustomContainer.Register(mockServer.Object);
            CustomContainer.Register(mockshell.Object);
            var assignExample = XML.XmlResource.Fetch("Loop Constructs - For Each");
            var resourceDefinationCleaner = new ResourceDefinationCleaner();
            var resource = resourceDefinationCleaner.GetResourceDefinition(true, Guid.Empty, new StringBuilder(assignExample.ToString(System.Xml.Linq.SaveOptions.DisableFormatting)));
            var dev2JsonSerializer = new Dev2JsonSerializer();
            var msg = dev2JsonSerializer.Deserialize<ExecuteMessage>(resource);

            var currentResourceModel = Dev2MockFactory.SetupResourceModelMock();
            currentResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(msg.Message);
            currentResourceModel.Setup(resModel => resModel.DisplayName).Returns("Hello World");
            currentResourceModel.Setup(resModel => resModel.Environment.ResourceRepository.FetchResourceDefinition(It.IsAny<IServer>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>()))
              .Returns(new ExecuteMessage { Message = msg.Message, HasError = false });
            currentResourceModel.Setup(p => p.Environment.Connection.ServerEvents).Returns(new Mock<IEventPublisher>().Object);

            var differenceResourceModel = Dev2MockFactory.SetupResourceModelMock();
            differenceResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(msg.Message);
            differenceResourceModel.Setup(resModel => resModel.DisplayName).Returns("Hello World");

            var toolUniqueId = new Guid();
            var conflicts = new List<IConflict> { new ToolConflict { UniqueId = toolUniqueId } };

            using (var mergeWorkflowViewModel = new MergeWorkflowViewModel(currentResourceModel.Object, differenceResourceModel.Object, false))
            {
                var methodToRun = typeof(MergeWorkflowViewModel).GetMethod("FindMatchingConnector", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                //---------------Assert Precondition----------------
                Assert.IsNotNull(methodToRun);
                //---------------Execute Test ----------------------
                var aaaa = methodToRun.Invoke(null, new object[] { toolUniqueId.ToString(), conflicts.Where(s => s is IToolConflict).Select(a => a.UniqueId.ToString()) });
                //---------------Test Result -----------------------
                Assert.IsTrue(Convert.ToBoolean(aaaa));
            }
        }

        [TestMethod]
        public void AddToTempConflictList_Givenconflicts_AddsItems()
        {
            //---------------Set up test pack-------------------
            var applicationAdapter = new Mock<IApplicationAdaptor>();
            CustomContainer.Register(applicationAdapter.Object);

            var mockServer = new Mock<IServer>();
            var mockshell = new Mock<IShellViewModel>();
            mockshell.Setup(a => a.ActiveServer).Returns(mockServer.Object);
            mockServer.Setup(a => a.GetServerVersion()).Returns("1.0.0.0");
            CustomContainer.Register(mockServer.Object);
            CustomContainer.Register(mockshell.Object);
            var assignExample = XML.XmlResource.Fetch("Loop Constructs - For Each");
            var resourceDefinationCleaner = new ResourceDefinationCleaner();
            var resource = resourceDefinationCleaner.GetResourceDefinition(true, Guid.Empty, new StringBuilder(assignExample.ToString(System.Xml.Linq.SaveOptions.DisableFormatting)));
            var dev2JsonSerializer = new Dev2JsonSerializer();
            var msg = dev2JsonSerializer.Deserialize<ExecuteMessage>(resource);

            var currentResourceModel = Dev2MockFactory.SetupResourceModelMock();
            currentResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(msg.Message);
            currentResourceModel.Setup(resModel => resModel.DisplayName).Returns("Hello World");
            currentResourceModel.Setup(resModel => resModel.Environment.ResourceRepository.FetchResourceDefinition(It.IsAny<IServer>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>()))
              .Returns(new ExecuteMessage { Message = msg.Message, HasError = false });
            currentResourceModel.Setup(p => p.Environment.Connection.ServerEvents).Returns(new Mock<IEventPublisher>().Object);

            var differenceResourceModel = Dev2MockFactory.SetupResourceModelMock();
            differenceResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(msg.Message);
            differenceResourceModel.Setup(resModel => resModel.DisplayName).Returns("Hello World");
            var b = new ArmConnectorConflict();
            var conflicts = new List<IConflict>(new[] { b });
            var itemsToAdd = new List<IConflict>();
            var armConnectorConflicts = new ArmConnectorConflict();
            using (var mergeWorkflowViewModel = new MergeWorkflowViewModel(currentResourceModel.Object, differenceResourceModel.Object, false))
            {
                var methodToRun = typeof(MergeWorkflowViewModel).GetMethod("AddToTempConflictList", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                //---------------Assert Precondition----------------
                Assert.IsNotNull(methodToRun);
                //---------------Execute Test ----------------------
                var id1 = new Guid().ToString();
                var id2 = new Guid().ToString();
                var aaaa = methodToRun.Invoke(null, new object[] { conflicts, itemsToAdd, armConnectorConflicts });
                //---------------Test Result -----------------------
                Assert.AreEqual(0, itemsToAdd.Count());
            }
        }

        [TestMethod]
        public void AddDiffArmConnectors_GivenDoeseNotContainsItem_AddsZeroItems()
        {
            //---------------Set up test pack-------------------
            var applicationAdapter = new Mock<IApplicationAdaptor>();
            CustomContainer.Register(applicationAdapter.Object);

            var mockServer = new Mock<IServer>();
            var mockshell = new Mock<IShellViewModel>();
            mockshell.Setup(a => a.ActiveServer).Returns(mockServer.Object);
            mockServer.Setup(a => a.GetServerVersion()).Returns("1.0.0.0");
            CustomContainer.Register(mockServer.Object);
            CustomContainer.Register(mockshell.Object);
            var assignExample = XML.XmlResource.Fetch("Loop Constructs - For Each");
            var resourceDefinationCleaner = new ResourceDefinationCleaner();
            var resource = resourceDefinationCleaner.GetResourceDefinition(true, Guid.Empty, new StringBuilder(assignExample.ToString(System.Xml.Linq.SaveOptions.DisableFormatting)));
            var dev2JsonSerializer = new Dev2JsonSerializer();
            var msg = dev2JsonSerializer.Deserialize<ExecuteMessage>(resource);

            var currentResourceModel = Dev2MockFactory.SetupResourceModelMock();
            currentResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(msg.Message);
            currentResourceModel.Setup(resModel => resModel.DisplayName).Returns("Hello World");
            currentResourceModel.Setup(resModel => resModel.Environment.ResourceRepository.FetchResourceDefinition(It.IsAny<IServer>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>()))
              .Returns(new ExecuteMessage { Message = msg.Message, HasError = false });
            currentResourceModel.Setup(p => p.Environment.Connection.ServerEvents).Returns(new Mock<IEventPublisher>().Object);

            var differenceResourceModel = Dev2MockFactory.SetupResourceModelMock();
            differenceResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(msg.Message);
            differenceResourceModel.Setup(resModel => resModel.DisplayName).Returns("Hello World");
            //AddDiffArmConnectors(List < IArmConnectorConflict > armConnectorConflicts, ConflictTreeNode treeItem, Guid id)
            var b = new ArmConnectorConflict();
            var calcActivity = new DsfCalculateActivity();
            var conflictTreeNode = new ConflictTreeNode(calcActivity, new Point());
            var guid = calcActivity.UniqueID.ToGuid(); ;
            var itemsToAdd = new List<IConflict>();
            var armConnectorConflicts = new List<IArmConnectorConflict>();
            using (var mergeWorkflowViewModel = new MergeWorkflowViewModel(currentResourceModel.Object, differenceResourceModel.Object, false))
            {
                var methodToRun = typeof(MergeWorkflowViewModel).GetMethod("AddDiffArmConnectors", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                //---------------Assert Precondition----------------
                Assert.IsNotNull(methodToRun);
                //---------------Execute Test ----------------------
                var id1 = new Guid().ToString();
                var id2 = new Guid().ToString();
                var aaaa = methodToRun.Invoke(mergeWorkflowViewModel, new object[] { armConnectorConflicts, conflictTreeNode, guid });
                //---------------Test Result -----------------------
                Assert.AreEqual(0, itemsToAdd.Count());
            }
        }

        [TestMethod]
        public void AddDiffArmConnectors_GivenContainsItem_AddsItems()
        {
            //---------------Set up test pack-------------------
            var applicationAdapter = new Mock<IApplicationAdaptor>();
            CustomContainer.Register(applicationAdapter.Object);

            var mockServer = new Mock<IServer>();
            var mockshell = new Mock<IShellViewModel>();
            mockshell.Setup(a => a.ActiveServer).Returns(mockServer.Object);
            mockServer.Setup(a => a.GetServerVersion()).Returns("1.0.0.0");
            CustomContainer.Register(mockServer.Object);
            CustomContainer.Register(mockshell.Object);
            var assignExample = XML.XmlResource.Fetch("Loop Constructs - For Each");
            var resourceDefinationCleaner = new ResourceDefinationCleaner();
            var resource = resourceDefinationCleaner.GetResourceDefinition(true, Guid.Empty, new StringBuilder(assignExample.ToString(System.Xml.Linq.SaveOptions.DisableFormatting)));
            var dev2JsonSerializer = new Dev2JsonSerializer();
            var msg = dev2JsonSerializer.Deserialize<ExecuteMessage>(resource);

            var currentResourceModel = Dev2MockFactory.SetupResourceModelMock();
            currentResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(msg.Message);
            currentResourceModel.Setup(resModel => resModel.DisplayName).Returns("Hello World");
            currentResourceModel.Setup(resModel => resModel.Environment.ResourceRepository.FetchResourceDefinition(It.IsAny<IServer>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>()))
              .Returns(new ExecuteMessage { Message = msg.Message, HasError = false });
            currentResourceModel.Setup(p => p.Environment.Connection.ServerEvents).Returns(new Mock<IEventPublisher>().Object);

            var differenceResourceModel = Dev2MockFactory.SetupResourceModelMock();
            differenceResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(msg.Message);
            differenceResourceModel.Setup(resModel => resModel.DisplayName).Returns("Hello World");
            var b = new ArmConnectorConflict();
            var activity = new Mock<IDev2Activity>();
            var arms = new List<(string Description, string Key, string SourceUniqueId, string DestinationUniqueId)>();
            var iniqueId = Guid.NewGuid().ToString();
            arms.Add(("a -> b", "a", iniqueId, Guid.NewGuid().ToString()));
            activity.Setup(p => p.ArmConnectors()).Returns(arms);
            activity.Setup(p => p.UniqueID).Returns(iniqueId);
            var conflictTreeNode = new ConflictTreeNode(activity.Object, new Point());
            var itemsToAdd = new List<IConflict>();
            var armConnectorConflicts = new List<IArmConnectorConflict>();
            using (var mergeWorkflowViewModel = new MergeWorkflowViewModel(currentResourceModel.Object, differenceResourceModel.Object, false))
            {
                var methodToRun = typeof(MergeWorkflowViewModel).GetMethod("AddDiffArmConnectors", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                //---------------Assert Precondition----------------
                Assert.IsNotNull(methodToRun);
                //---------------Execute Test ----------------------
                var aaaa = methodToRun.Invoke(mergeWorkflowViewModel, new object[] { armConnectorConflicts, conflictTreeNode, iniqueId.ToGuid() });
                //---------------Test Result -----------------------
                Assert.AreEqual(1, armConnectorConflicts.Count());
                var hasConflict = armConnectorConflicts.Single().HasConflict;
                var isArmSelectionAllowed = armConnectorConflicts.Single().CurrentArmConnector.IsArmSelectionAllowed;
                var isArmSelectionAllowed1 = armConnectorConflicts.Single().DifferentArmConnector.IsArmSelectionAllowed;
                Assert.IsTrue(hasConflict);
                Assert.IsFalse(isArmSelectionAllowed);
                Assert.IsFalse(isArmSelectionAllowed1);
            }
        }

        [TestMethod]
        public void AddDiffArmConnectors_GivenContainsItem_ExpectNoItemsAdded()
        {
            //---------------Set up test pack-------------------
            var applicationAdapter = new Mock<IApplicationAdaptor>();
            CustomContainer.Register(applicationAdapter.Object);

            var mockServer = new Mock<IServer>();
            var mockshell = new Mock<IShellViewModel>();
            mockshell.Setup(a => a.ActiveServer).Returns(mockServer.Object);
            mockServer.Setup(a => a.GetServerVersion()).Returns("1.0.0.0");
            CustomContainer.Register(mockServer.Object);
            CustomContainer.Register(mockshell.Object);
            var assignExample = XML.XmlResource.Fetch("Loop Constructs - For Each");
            var resourceDefinationCleaner = new ResourceDefinationCleaner();
            var resource = resourceDefinationCleaner.GetResourceDefinition(true, Guid.Empty, new StringBuilder(assignExample.ToString(System.Xml.Linq.SaveOptions.DisableFormatting)));
            var dev2JsonSerializer = new Dev2JsonSerializer();
            var msg = dev2JsonSerializer.Deserialize<ExecuteMessage>(resource);

            var currentResourceModel = Dev2MockFactory.SetupResourceModelMock();
            currentResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(msg.Message);
            currentResourceModel.Setup(resModel => resModel.DisplayName).Returns("Hello World");
            currentResourceModel.Setup(resModel => resModel.Environment.ResourceRepository.FetchResourceDefinition(It.IsAny<IServer>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>()))
              .Returns(new ExecuteMessage { Message = msg.Message, HasError = false });
            currentResourceModel.Setup(p => p.Environment.Connection.ServerEvents).Returns(new Mock<IEventPublisher>().Object);

            var differenceResourceModel = Dev2MockFactory.SetupResourceModelMock();
            differenceResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(msg.Message);
            differenceResourceModel.Setup(resModel => resModel.DisplayName).Returns("Hello World");
            var b = new ArmConnectorConflict();
            var activity = new Mock<IDev2Activity>();
            var arms = new List<(string Description, string Key, string SourceUniqueId, string DestinationUniqueId)>();
            var iniqueId = Guid.NewGuid().ToString();
            arms.Add(("a -> b", iniqueId, iniqueId, Guid.NewGuid().ToString()));
            activity.Setup(p => p.ArmConnectors()).Returns(arms);
            activity.Setup(p => p.UniqueID).Returns(iniqueId);
            b.Key = iniqueId;
            b.UniqueId = iniqueId.ToGuid();
            b.CurrentArmConnector = new MergeArmConnectorConflict("", "", "", "", b);
            var conflictTreeNode = new ConflictTreeNode(activity.Object, new Point());
            var itemsToAdd = new List<IConflict>();
            var armConnectorConflicts = new List<IArmConnectorConflict>();
            armConnectorConflicts.Add(b);
            using (var mergeWorkflowViewModel = new MergeWorkflowViewModel(currentResourceModel.Object, differenceResourceModel.Object, false))
            {
                var methodToRun = typeof(MergeWorkflowViewModel).GetMethod("AddDiffArmConnectors", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var nextMethodToRun = typeof(MergeWorkflowViewModel).GetMethod("GetMatchingConflictParent", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                //---------------Assert Precondition----------------
                Assert.IsNotNull(methodToRun);
                Assert.IsNotNull(nextMethodToRun);
                //---------------Execute Test ----------------------
                var aaaa = methodToRun.Invoke(mergeWorkflowViewModel, new object[] { armConnectorConflicts, conflictTreeNode, iniqueId.ToGuid() });
                var bbbb = nextMethodToRun.Invoke(mergeWorkflowViewModel, new object[] { });
                //---------------Test Result -----------------------
                Assert.AreEqual(1, armConnectorConflicts.Count());

                var hasConflict = armConnectorConflicts.Single().HasConflict;
                var isArmSelectionAllowed = armConnectorConflicts.Single().CurrentArmConnector.IsArmSelectionAllowed;
                var isArmSelectionAllowed1 = armConnectorConflicts.Single().DifferentArmConnector.IsArmSelectionAllowed;
                Assert.IsTrue(hasConflict);
                Assert.IsFalse(isArmSelectionAllowed);
                Assert.IsFalse(isArmSelectionAllowed1);
            }
        }

        [TestMethod]
        public void AddArmConnectors_GivenContainsItem_ExpectNoItemsAdded()
        {
            //---------------Set up test pack-------------------
            var applicationAdapter = new Mock<IApplicationAdaptor>();
            CustomContainer.Register(applicationAdapter.Object);

            var mockServer = new Mock<IServer>();
            var mockshell = new Mock<IShellViewModel>();
            mockshell.Setup(a => a.ActiveServer).Returns(mockServer.Object);
            mockServer.Setup(a => a.GetServerVersion()).Returns("1.0.0.0");
            CustomContainer.Register(mockServer.Object);
            CustomContainer.Register(mockshell.Object);
            var assignExample = XML.XmlResource.Fetch("Loop Constructs - For Each");
            var resourceDefinationCleaner = new ResourceDefinationCleaner();
            var resource = resourceDefinationCleaner.GetResourceDefinition(true, Guid.Empty, new StringBuilder(assignExample.ToString(System.Xml.Linq.SaveOptions.DisableFormatting)));
            var dev2JsonSerializer = new Dev2JsonSerializer();
            var msg = dev2JsonSerializer.Deserialize<ExecuteMessage>(resource);

            var currentResourceModel = Dev2MockFactory.SetupResourceModelMock();
            currentResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(msg.Message);
            currentResourceModel.Setup(resModel => resModel.DisplayName).Returns("Hello World");
            currentResourceModel.Setup(resModel => resModel.Environment.ResourceRepository.FetchResourceDefinition(It.IsAny<IServer>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>()))
              .Returns(new ExecuteMessage { Message = msg.Message, HasError = false });
            currentResourceModel.Setup(p => p.Environment.Connection.ServerEvents).Returns(new Mock<IEventPublisher>().Object);

            var differenceResourceModel = Dev2MockFactory.SetupResourceModelMock();
            differenceResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(msg.Message);
            differenceResourceModel.Setup(resModel => resModel.DisplayName).Returns("Hello World");
            var b = new ArmConnectorConflict();
            var activity = new Mock<IDev2Activity>();
            var arms = new List<(string Description, string Key, string SourceUniqueId, string DestinationUniqueId)>();
            var iniqueId = Guid.NewGuid().ToString();
            arms.Add(("a -> b", iniqueId, iniqueId, Guid.NewGuid().ToString()));
            activity.Setup(p => p.ArmConnectors()).Returns(arms);
            activity.Setup(p => p.UniqueID).Returns(iniqueId);
            b.Key = iniqueId;
            b.UniqueId = iniqueId.ToGuid();
            b.CurrentArmConnector = new MergeArmConnectorConflict("", "", "", "", b);
            var conflictTreeNode = new ConflictTreeNode(activity.Object, new Point());
            var itemsToAdd = new List<IConflict>();
            var armConnectorConflicts = new List<IArmConnectorConflict>();
            armConnectorConflicts.Add(b);
            using (var mergeWorkflowViewModel = new MergeWorkflowViewModel(currentResourceModel.Object, differenceResourceModel.Object, false))
            {
                var methodToRun = typeof(MergeWorkflowViewModel).GetMethod("AddArmConnectors", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                //---------------Assert Precondition----------------
                Assert.IsNotNull(methodToRun);
                //---------------Execute Test ----------------------
                var aaaa = methodToRun.Invoke(mergeWorkflowViewModel, new object[] { armConnectorConflicts, conflictTreeNode, iniqueId.ToGuid() });
                //---------------Test Result -----------------------
                Assert.AreEqual(1, armConnectorConflicts.Count());
                var hasConflict = armConnectorConflicts.Single().HasConflict;
                var isArmSelectionAllowed = armConnectorConflicts.Single().CurrentArmConnector.IsArmSelectionAllowed;
                Assert.IsFalse(hasConflict);
                Assert.IsFalse(isArmSelectionAllowed);
            }
        }

        [TestMethod]
        public void EmptyMergeArmConnectorConflict_GivenContainter_ExpectReturnsCorrectly()
        {
            //---------------Set up test pack-------------------
            var applicationAdapter = new Mock<IApplicationAdaptor>();
            CustomContainer.Register(applicationAdapter.Object);

            var mockServer = new Mock<IServer>();
            var mockshell = new Mock<IShellViewModel>();
            mockshell.Setup(a => a.ActiveServer).Returns(mockServer.Object);
            mockServer.Setup(a => a.GetServerVersion()).Returns("1.0.0.0");
            CustomContainer.Register(mockServer.Object);
            CustomContainer.Register(mockshell.Object);
            var assignExample = XML.XmlResource.Fetch("Loop Constructs - For Each");
            var resourceDefinationCleaner = new ResourceDefinationCleaner();
            var resource = resourceDefinationCleaner.GetResourceDefinition(true, Guid.Empty, new StringBuilder(assignExample.ToString(System.Xml.Linq.SaveOptions.DisableFormatting)));
            var dev2JsonSerializer = new Dev2JsonSerializer();
            var msg = dev2JsonSerializer.Deserialize<ExecuteMessage>(resource);

            var currentResourceModel = Dev2MockFactory.SetupResourceModelMock();
            currentResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(msg.Message);
            currentResourceModel.Setup(resModel => resModel.DisplayName).Returns("Hello World");
            currentResourceModel.Setup(resModel => resModel.Environment.ResourceRepository.FetchResourceDefinition(It.IsAny<IServer>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>()))
              .Returns(new ExecuteMessage { Message = msg.Message, HasError = false });
            currentResourceModel.Setup(p => p.Environment.Connection.ServerEvents).Returns(new Mock<IEventPublisher>().Object);

            var differenceResourceModel = Dev2MockFactory.SetupResourceModelMock();
            differenceResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(msg.Message);
            differenceResourceModel.Setup(resModel => resModel.DisplayName).Returns("Hello World");
            var b = new ArmConnectorConflict();
            var activity = new Mock<IDev2Activity>();
            var arms = new List<(string Description, string Key, string SourceUniqueId, string DestinationUniqueId)>();
            var iniqueId = Guid.NewGuid().ToString();
            arms.Add(("a", iniqueId, iniqueId, Guid.NewGuid().ToString()));
            activity.Setup(p => p.ArmConnectors()).Returns(arms);
            activity.Setup(p => p.UniqueID).Returns(iniqueId);
            b.Key = iniqueId;
            b.UniqueId = iniqueId.ToGuid();
            b.CurrentArmConnector = new MergeArmConnectorConflict("", "", "", "", b);
            var conflictTreeNode = new ConflictTreeNode(activity.Object, new Point());
            var itemsToAdd = new List<IConflict>();
            var armConnectorConflicts = new List<IArmConnectorConflict>();
            armConnectorConflicts.Add(b);
            using (var mergeWorkflowViewModel = new MergeWorkflowViewModel(currentResourceModel.Object, differenceResourceModel.Object, false))
            {
                var methodToRun = typeof(MergeWorkflowViewModel).GetMethod("EmptyMergeArmConnectorConflict", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                //---------------Assert Precondition----------------
                Assert.IsNotNull(methodToRun);
                //---------------Execute Test ----------------------
                var aaaa = methodToRun.Invoke(null, new object[] { b.UniqueId, b }) as MergeArmConnectorConflict;
                //---------------Test Result -----------------------
                Assert.AreEqual(iniqueId, aaaa.SourceUniqueId);
                Assert.AreEqual(Guid.Empty.ToString(), aaaa.DestinationUniqueId);
                Assert.AreSame(b, aaaa.Container);
            }
        }

        [TestMethod]
        public void EmptyConflictViewModel_GivenContainter_ExpectReturnsCorrectly()
        {
            //---------------Set up test pack-------------------
            var applicationAdapter = new Mock<IApplicationAdaptor>();
            CustomContainer.Register(applicationAdapter.Object);

            var mockServer = new Mock<IServer>();
            var mockshell = new Mock<IShellViewModel>();
            mockshell.Setup(a => a.ActiveServer).Returns(mockServer.Object);
            mockServer.Setup(a => a.GetServerVersion()).Returns("1.0.0.0");
            CustomContainer.Register(mockServer.Object);
            CustomContainer.Register(mockshell.Object);
            var assignExample = XML.XmlResource.Fetch("Loop Constructs - For Each");
            var resourceDefinationCleaner = new ResourceDefinationCleaner();
            var resource = resourceDefinationCleaner.GetResourceDefinition(true, Guid.Empty, new StringBuilder(assignExample.ToString(System.Xml.Linq.SaveOptions.DisableFormatting)));
            var dev2JsonSerializer = new Dev2JsonSerializer();
            var msg = dev2JsonSerializer.Deserialize<ExecuteMessage>(resource);

            var currentResourceModel = Dev2MockFactory.SetupResourceModelMock();
            currentResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(msg.Message);
            currentResourceModel.Setup(resModel => resModel.DisplayName).Returns("Hello World");
            currentResourceModel.Setup(resModel => resModel.Environment.ResourceRepository.FetchResourceDefinition(It.IsAny<IServer>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>()))
              .Returns(new ExecuteMessage { Message = msg.Message, HasError = false });
            currentResourceModel.Setup(p => p.Environment.Connection.ServerEvents).Returns(new Mock<IEventPublisher>().Object);

            var differenceResourceModel = Dev2MockFactory.SetupResourceModelMock();
            differenceResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(msg.Message);
            differenceResourceModel.Setup(resModel => resModel.DisplayName).Returns("Hello World");
            var b = new ArmConnectorConflict();
            var activity = new Mock<IDev2Activity>();
            var arms = new List<(string Description, string Key, string SourceUniqueId, string DestinationUniqueId)>();
            var iniqueId = Guid.NewGuid().ToString();
            arms.Add(("a", iniqueId, iniqueId, Guid.NewGuid().ToString()));
            activity.Setup(p => p.ArmConnectors()).Returns(arms);
            activity.Setup(p => p.UniqueID).Returns(iniqueId);
            b.Key = iniqueId;
            b.UniqueId = iniqueId.ToGuid();
            b.CurrentArmConnector = new MergeArmConnectorConflict("", "", "", "", b);
            var conflictTreeNode = new ConflictTreeNode(activity.Object, new Point());
            var itemsToAdd = new List<IConflict>();
            var armConnectorConflicts = new List<IArmConnectorConflict>();
            armConnectorConflicts.Add(b);
            var wfDesignerVm = new Mock<IWorkflowDesignerViewModel>();
            using (var mergeWorkflowViewModel = new MergeWorkflowViewModel(currentResourceModel.Object, differenceResourceModel.Object, false))
            {
                var methodToRun = typeof(MergeWorkflowViewModel).GetMethod("EmptyConflictViewModel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                //---------------Assert Precondition----------------
                Assert.IsNotNull(methodToRun);
                //---------------Execute Test ----------------------
                var aaaa = methodToRun.Invoke(null, new object[] { b.UniqueId, wfDesignerVm.Object }) as MergeToolModel;
                //---------------Test Result -----------------------
                Assert.AreEqual(b.UniqueId, aaaa.UniqueId);
                Assert.AreEqual(null, aaaa.ModelItem);
                Assert.AreEqual(false, aaaa.IsMergeEnabled);
                Assert.AreEqual(false, aaaa.IsMergeVisible);
            }
        }

        [TestMethod]
        public void ResetToolConflict_Given_MergeToolModel_VerifyCalls()
        {
            //---------------Set up test pack-------------------
            var applicationAdapter = new Mock<IApplicationAdaptor>();
            CustomContainer.Register(applicationAdapter.Object);

            var mockServer = new Mock<IServer>();
            var mockshell = new Mock<IShellViewModel>();
            mockshell.Setup(a => a.ActiveServer).Returns(mockServer.Object);
            mockServer.Setup(a => a.GetServerVersion()).Returns("1.0.0.0");
            CustomContainer.Register(mockServer.Object);
            CustomContainer.Register(mockshell.Object);
            var assignExample = XML.XmlResource.Fetch("Loop Constructs - For Each");
            var resourceDefinationCleaner = new ResourceDefinationCleaner();
            var resource = resourceDefinationCleaner.GetResourceDefinition(true, Guid.Empty, new StringBuilder(assignExample.ToString(System.Xml.Linq.SaveOptions.DisableFormatting)));
            var dev2JsonSerializer = new Dev2JsonSerializer();
            var msg = dev2JsonSerializer.Deserialize<ExecuteMessage>(resource);

            var currentResourceModel = Dev2MockFactory.SetupResourceModelMock();
            currentResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(msg.Message);
            currentResourceModel.Setup(resModel => resModel.DisplayName).Returns("Hello World");
            currentResourceModel.Setup(resModel => resModel.Environment.ResourceRepository.FetchResourceDefinition(It.IsAny<IServer>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>()))
              .Returns(new ExecuteMessage { Message = msg.Message, HasError = false });
            currentResourceModel.Setup(p => p.Environment.Connection.ServerEvents).Returns(new Mock<IEventPublisher>().Object);

            var differenceResourceModel = Dev2MockFactory.SetupResourceModelMock();
            differenceResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(msg.Message);
            differenceResourceModel.Setup(resModel => resModel.DisplayName).Returns("Hello World");

            var mergeToolModel = new Mock<IMergeToolModel>();
            mergeToolModel.Setup(p => p.DisableEvents()).Verifiable();
            var wfDesignerVm = new Mock<IWorkflowDesignerViewModel>();
            wfDesignerVm.Setup(p => p.RemoveItem(It.IsAny<IMergeToolModel>())).Verifiable();
            using (var mergeWorkflowViewModel = new MergeWorkflowViewModel(currentResourceModel.Object, differenceResourceModel.Object, false))
            {
                mergeWorkflowViewModel.WorkflowDesignerViewModel = wfDesignerVm.Object;
                var methodToRun = typeof(MergeWorkflowViewModel).GetMethod("ResetToolConflict", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                //---------------Assert Precondition----------------
                Assert.IsNotNull(methodToRun);
                //---------------Execute Test ----------------------
                var aaaa = methodToRun.Invoke(mergeWorkflowViewModel, new object[] { mergeToolModel.Object });
                //---------------Test Result -----------------------
                mergeToolModel.VerifyAll();
            }
        }

        [TestMethod]
        public void SetDisplayName_Given_IsDirtyFalse_ExpectAstericsRemoved()
        {
            //---------------Set up test pack-------------------
            var applicationAdapter = new Mock<IApplicationAdaptor>();
            CustomContainer.Register(applicationAdapter.Object);

            var mockServer = new Mock<IServer>();
            var mockshell = new Mock<IShellViewModel>();
            mockshell.Setup(a => a.ActiveServer).Returns(mockServer.Object);
            mockServer.Setup(a => a.GetServerVersion()).Returns("1.0.0.0");
            CustomContainer.Register(mockServer.Object);
            CustomContainer.Register(mockshell.Object);
            var assignExample = XML.XmlResource.Fetch("Loop Constructs - For Each");
            var resourceDefinationCleaner = new ResourceDefinationCleaner();
            var resource = resourceDefinationCleaner.GetResourceDefinition(true, Guid.Empty, new StringBuilder(assignExample.ToString(System.Xml.Linq.SaveOptions.DisableFormatting)));
            var dev2JsonSerializer = new Dev2JsonSerializer();
            var msg = dev2JsonSerializer.Deserialize<ExecuteMessage>(resource);

            var currentResourceModel = Dev2MockFactory.SetupResourceModelMock();
            currentResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(msg.Message);
            currentResourceModel.Setup(resModel => resModel.DisplayName).Returns("Hello World");
            currentResourceModel.Setup(resModel => resModel.Environment.ResourceRepository.FetchResourceDefinition(It.IsAny<IServer>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>()))
              .Returns(new ExecuteMessage { Message = msg.Message, HasError = false });
            currentResourceModel.Setup(p => p.Environment.Connection.ServerEvents).Returns(new Mock<IEventPublisher>().Object);

            var differenceResourceModel = Dev2MockFactory.SetupResourceModelMock();
            differenceResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(msg.Message);
            differenceResourceModel.Setup(resModel => resModel.DisplayName).Returns("Hello World");

            using (var mergeWorkflowViewModel = new MergeWorkflowViewModel(currentResourceModel.Object, differenceResourceModel.Object, false))
            {
                var methodToRun = typeof(MergeWorkflowViewModel).GetMethod("SetDisplayName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                //---------------Assert Precondition----------------
                Assert.IsNotNull(methodToRun);
                //---------------Execute Test ----------------------
                var aaaa = methodToRun.Invoke(mergeWorkflowViewModel, new object[] { false });
                //---------------Test Result -----------------------
                var hasAsterics = mergeWorkflowViewModel.DisplayName.Contains("*");
                Assert.IsFalse(hasAsterics);
            }
        }

        [TestMethod]
        public void SetupBindings_Given_CurrentConflictModelIsNull_ShouldSetupNewCurrentConflictModel()
        {
            //---------------Set up test pack-------------------
            var applicationAdapter = new Mock<IApplicationAdaptor>();
            CustomContainer.Register(applicationAdapter.Object);

            var mockServer = new Mock<IServer>();
            var mockshell = new Mock<IShellViewModel>();
            mockshell.Setup(a => a.ActiveServer).Returns(mockServer.Object);
            mockServer.Setup(a => a.GetServerVersion()).Returns("1.0.0.0");
            CustomContainer.Register(mockServer.Object);
            CustomContainer.Register(mockshell.Object);
            var assignExample = XML.XmlResource.Fetch("Loop Constructs - For Each");
            var resourceDefinationCleaner = new ResourceDefinationCleaner();
            var resource = resourceDefinationCleaner.GetResourceDefinition(true, Guid.Empty, new StringBuilder(assignExample.ToString(System.Xml.Linq.SaveOptions.DisableFormatting)));
            var dev2JsonSerializer = new Dev2JsonSerializer();
            var msg = dev2JsonSerializer.Deserialize<ExecuteMessage>(resource);

            var currentResourceModel = Dev2MockFactory.SetupResourceModelMock();
            currentResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(msg.Message);
            currentResourceModel.Setup(resModel => resModel.DisplayName).Returns("Hello World");
            currentResourceModel.Setup(resModel => resModel.Environment.ResourceRepository.FetchResourceDefinition(It.IsAny<IServer>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>()))
              .Returns(new ExecuteMessage { Message = msg.Message, HasError = false });
            currentResourceModel.Setup(p => p.Environment.Connection.ServerEvents).Returns(new Mock<IEventPublisher>().Object);

            var differenceResourceModel = Dev2MockFactory.SetupResourceModelMock();
            differenceResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(msg.Message);
            differenceResourceModel.Setup(resModel => resModel.DisplayName).Returns("Hello World");

            var mergeToolModel = new Mock<IToolConflict>();
            mergeToolModel.SetupGet(p => p.CurrentViewModel).Returns(new Mock<IMergeToolModel>().Object);
            var wfDesignerVm = new Mock<IWorkflowDesignerViewModel>();
            wfDesignerVm.Setup(p => p.RemoveItem(It.IsAny<IMergeToolModel>())).Verifiable();
            using (var mergeWorkflowViewModel = new MergeWorkflowViewModel(currentResourceModel.Object, differenceResourceModel.Object, false))
            {
                mergeWorkflowViewModel.CurrentConflictModel = null;
                mergeWorkflowViewModel.WorkflowDesignerViewModel = wfDesignerVm.Object;
                var methodToRun = typeof(MergeWorkflowViewModel).GetMethod("SetupBindings", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                //---------------Assert Precondition----------------
                Assert.IsNotNull(methodToRun);
                //---------------Execute Test ----------------------
                var aaaa = methodToRun.Invoke(mergeWorkflowViewModel, new object[] { currentResourceModel.Object, differenceResourceModel.Object, mergeToolModel.Object });
                //---------------Test Result -----------------------
                mergeToolModel.VerifyAll();
                Assert.IsNotNull(mergeWorkflowViewModel.CurrentConflictModel);
            }
        }

        [TestMethod]
        public void SetupBindings_Given_DifferenceConflictModelIsNull_ShouldSetupNewDifferenceConflictModel()
        {
            //---------------Set up test pack-------------------
            var applicationAdapter = new Mock<IApplicationAdaptor>();
            CustomContainer.Register(applicationAdapter.Object);

            var mockServer = new Mock<IServer>();
            var mockshell = new Mock<IShellViewModel>();
            mockshell.Setup(a => a.ActiveServer).Returns(mockServer.Object);
            mockServer.Setup(a => a.GetServerVersion()).Returns("1.0.0.0");
            CustomContainer.Register(mockServer.Object);
            CustomContainer.Register(mockshell.Object);
            var assignExample = XML.XmlResource.Fetch("Loop Constructs - For Each");
            var resourceDefinationCleaner = new ResourceDefinationCleaner();
            var resource = resourceDefinationCleaner.GetResourceDefinition(true, Guid.Empty, new StringBuilder(assignExample.ToString(System.Xml.Linq.SaveOptions.DisableFormatting)));
            var dev2JsonSerializer = new Dev2JsonSerializer();
            var msg = dev2JsonSerializer.Deserialize<ExecuteMessage>(resource);

            var currentResourceModel = Dev2MockFactory.SetupResourceModelMock();
            currentResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(msg.Message);
            currentResourceModel.Setup(resModel => resModel.DisplayName).Returns("Hello World");
            currentResourceModel.Setup(resModel => resModel.Environment.ResourceRepository.FetchResourceDefinition(It.IsAny<IServer>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>()))
              .Returns(new ExecuteMessage { Message = msg.Message, HasError = false });
            currentResourceModel.Setup(p => p.Environment.Connection.ServerEvents).Returns(new Mock<IEventPublisher>().Object);

            var differenceResourceModel = Dev2MockFactory.SetupResourceModelMock();
            differenceResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(msg.Message);
            differenceResourceModel.Setup(resModel => resModel.DisplayName).Returns("Hello World");

            var mergeToolModel = new Mock<IToolConflict>();
            mergeToolModel.SetupGet(p => p.DiffViewModel).Returns(new Mock<IMergeToolModel>().Object);
            var wfDesignerVm = new Mock<IWorkflowDesignerViewModel>();
            wfDesignerVm.Setup(p => p.RemoveItem(It.IsAny<IMergeToolModel>())).Verifiable();
            using (var mergeWorkflowViewModel = new MergeWorkflowViewModel(currentResourceModel.Object, differenceResourceModel.Object, false))
            {
                mergeWorkflowViewModel.DifferenceConflictModel = null;
                mergeWorkflowViewModel.WorkflowDesignerViewModel = wfDesignerVm.Object;
                var methodToRun = typeof(MergeWorkflowViewModel).GetMethod("SetupBindings", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                //---------------Assert Precondition----------------
                Assert.IsNotNull(methodToRun);
                //---------------Execute Test ----------------------
                var aaaa = methodToRun.Invoke(mergeWorkflowViewModel, new object[] { currentResourceModel.Object, differenceResourceModel.Object, mergeToolModel.Object });
                //---------------Test Result -----------------------
                mergeToolModel.VerifyAll();
                Assert.IsNotNull(mergeWorkflowViewModel.DifferenceConflictModel);
            }
        }

        [TestMethod]
        public void ResetToolEvents_Given_MergeToolModel_VerifyCalls()
        {
            //---------------Set up test pack-------------------
            var applicationAdapter = new Mock<IApplicationAdaptor>();
            CustomContainer.Register(applicationAdapter.Object);

            var mockServer = new Mock<IServer>();
            var mockshell = new Mock<IShellViewModel>();
            mockshell.Setup(a => a.ActiveServer).Returns(mockServer.Object);
            mockServer.Setup(a => a.GetServerVersion()).Returns("1.0.0.0");
            CustomContainer.Register(mockServer.Object);
            CustomContainer.Register(mockshell.Object);
            var assignExample = XML.XmlResource.Fetch("Loop Constructs - For Each");
            var resourceDefinationCleaner = new ResourceDefinationCleaner();
            var resource = resourceDefinationCleaner.GetResourceDefinition(true, Guid.Empty, new StringBuilder(assignExample.ToString(System.Xml.Linq.SaveOptions.DisableFormatting)));
            var dev2JsonSerializer = new Dev2JsonSerializer();
            var msg = dev2JsonSerializer.Deserialize<ExecuteMessage>(resource);

            var currentResourceModel = Dev2MockFactory.SetupResourceModelMock();
            currentResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(msg.Message);
            currentResourceModel.Setup(resModel => resModel.DisplayName).Returns("Hello World");
            currentResourceModel.Setup(resModel => resModel.Environment.ResourceRepository.FetchResourceDefinition(It.IsAny<IServer>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>()))
              .Returns(new ExecuteMessage { Message = msg.Message, HasError = false });
            currentResourceModel.Setup(p => p.Environment.Connection.ServerEvents).Returns(new Mock<IEventPublisher>().Object);

            var differenceResourceModel = Dev2MockFactory.SetupResourceModelMock();
            differenceResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(msg.Message);
            differenceResourceModel.Setup(resModel => resModel.DisplayName).Returns("Hello World");

            var mergeToolModel = new Mock<IMergeToolModel>();
            var mergeToolModelInternal = new Mock<IMergeToolModel>();
            mergeToolModelInternal.Setup(p => p.DisableEvents()).Verifiable();

            var wfDesignerVm = new Mock<IWorkflowDesignerViewModel>();
            wfDesignerVm.Setup(p => p.RemoveItem(It.IsAny<IMergeToolModel>())).Verifiable();
            using (var mergeWorkflowViewModel = new MergeWorkflowViewModel(currentResourceModel.Object, differenceResourceModel.Object, false))
            {
                mergeWorkflowViewModel.WorkflowDesignerViewModel = wfDesignerVm.Object;
                var conflict = mergeWorkflowViewModel.Conflicts.FirstOrDefault() as ToolConflict;
                var lastConflict = mergeWorkflowViewModel.Conflicts.Last(c => c is ToolConflict);
                var bbb = lastConflict as ToolConflict;
                bbb.CurrentViewModel = mergeToolModelInternal.Object;
                bbb.DiffViewModel = mergeToolModelInternal.Object;
                bbb.HasConflict = true;
                mergeToolModel.Setup(p => p.Container).Returns(conflict);

                var methodToRun = typeof(MergeWorkflowViewModel).GetMethod("ResetToolEvents", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                //---------------Assert Precondition----------------
                Assert.IsNotNull(methodToRun);
                //---------------Execute Test ----------------------
                var aaaa = methodToRun.Invoke(mergeWorkflowViewModel, new object[] { mergeToolModel.Object });
                //---------------Test Result -----------------------
                mergeToolModelInternal.Verify(p => p.DisableEvents(), Times.Exactly(2));
            }
        }

        [TestMethod]
        public void ExpandPreviousItems_Given_IToolConflict_VerifyCalls()
        {
            //---------------Set up test pack-------------------
            var applicationAdapter = new Mock<IApplicationAdaptor>();
            CustomContainer.Register(applicationAdapter.Object);

            var mockServer = new Mock<IServer>();
            var mockshell = new Mock<IShellViewModel>();
            mockshell.Setup(a => a.ActiveServer).Returns(mockServer.Object);
            mockServer.Setup(a => a.GetServerVersion()).Returns("1.0.0.0");
            CustomContainer.Register(mockServer.Object);
            CustomContainer.Register(mockshell.Object);
            var assignExample = XML.XmlResource.Fetch("Loop Constructs - For Each");
            var resourceDefinationCleaner = new ResourceDefinationCleaner();
            var resource = resourceDefinationCleaner.GetResourceDefinition(true, Guid.Empty, new StringBuilder(assignExample.ToString(System.Xml.Linq.SaveOptions.DisableFormatting)));
            var dev2JsonSerializer = new Dev2JsonSerializer();
            var msg = dev2JsonSerializer.Deserialize<ExecuteMessage>(resource);

            var currentResourceModel = Dev2MockFactory.SetupResourceModelMock();
            currentResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(msg.Message);
            currentResourceModel.Setup(resModel => resModel.DisplayName).Returns("Hello World");
            currentResourceModel.Setup(resModel => resModel.Environment.ResourceRepository.FetchResourceDefinition(It.IsAny<IServer>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>()))
              .Returns(new ExecuteMessage { Message = msg.Message, HasError = false });
            currentResourceModel.Setup(p => p.Environment.Connection.ServerEvents).Returns(new Mock<IEventPublisher>().Object);

            var differenceResourceModel = Dev2MockFactory.SetupResourceModelMock();
            differenceResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(msg.Message);
            differenceResourceModel.Setup(resModel => resModel.DisplayName).Returns("Hello World");

            var toolConflict = new Mock<IToolConflict>();
            toolConflict.Setup(p => p.Parent).Returns(new ToolConflict()).Verifiable();
            using (var mergeWorkflowViewModel = new MergeWorkflowViewModel(currentResourceModel.Object, differenceResourceModel.Object, false))
            {
                var methodToRun = typeof(MergeWorkflowViewModel).GetMethod("ExpandPreviousItems", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                //---------------Assert Precondition----------------
                Assert.IsNotNull(methodToRun);
                //---------------Execute Test ----------------------
                var aaaa = methodToRun.Invoke(mergeWorkflowViewModel, new object[] { toolConflict.Object });
                //---------------Test Result -----------------------
                toolConflict.VerifyAll();
            }
        }

        [TestMethod]
        public void All_GivenFunc_ExpectReturnsCorrectly()
        {
            //---------------Set up test pack-------------------
            var applicationAdapter = new Mock<IApplicationAdaptor>();
            CustomContainer.Register(applicationAdapter.Object);

            var mockServer = new Mock<IServer>();
            var mockshell = new Mock<IShellViewModel>();
            mockshell.Setup(a => a.ActiveServer).Returns(mockServer.Object);
            mockServer.Setup(a => a.GetServerVersion()).Returns("1.0.0.0");
            CustomContainer.Register(mockServer.Object);
            CustomContainer.Register(mockshell.Object);
            var assignExample = XML.XmlResource.Fetch("Loop Constructs - For Each");
            var resourceDefinationCleaner = new ResourceDefinationCleaner();
            var resource = resourceDefinationCleaner.GetResourceDefinition(true, Guid.Empty, new StringBuilder(assignExample.ToString(System.Xml.Linq.SaveOptions.DisableFormatting)));
            var dev2JsonSerializer = new Dev2JsonSerializer();
            var msg = dev2JsonSerializer.Deserialize<ExecuteMessage>(resource);

            var currentResourceModel = Dev2MockFactory.SetupResourceModelMock();
            currentResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(msg.Message);
            currentResourceModel.Setup(resModel => resModel.DisplayName).Returns("Hello World");
            currentResourceModel.Setup(resModel => resModel.Environment.ResourceRepository.FetchResourceDefinition(It.IsAny<IServer>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>()))
              .Returns(new ExecuteMessage { Message = msg.Message, HasError = false });
            currentResourceModel.Setup(p => p.Environment.Connection.ServerEvents).Returns(new Mock<IEventPublisher>().Object);

            var differenceResourceModel = Dev2MockFactory.SetupResourceModelMock();
            differenceResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(msg.Message);
            differenceResourceModel.Setup(resModel => resModel.DisplayName).Returns("Hello World");
            var b = new ArmConnectorConflict();
            var activity = new Mock<IDev2Activity>();
            var arms = new List<(string Description, string Key, string SourceUniqueId, string DestinationUniqueId)>();
            var iniqueId = Guid.NewGuid().ToString();
            arms.Add(("a", iniqueId, iniqueId, Guid.NewGuid().ToString()));
            activity.Setup(p => p.ArmConnectors()).Returns(arms);
            activity.Setup(p => p.UniqueID).Returns(iniqueId);
            b.Key = iniqueId;
            b.UniqueId = iniqueId.ToGuid();
            Func<IConflict, bool> check = (c) => { if (c.UniqueId == Guid.Empty) { return false; } return true; };
            using (var mergeWorkflowViewModel = new MergeWorkflowViewModel(currentResourceModel.Object, differenceResourceModel.Object, false))
            {
                var methodToRun = typeof(MergeWorkflowViewModel).GetMethod("All", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                //---------------Assert Precondition----------------
                Assert.IsNotNull(methodToRun);
                //---------------Execute Test ----------------------
                var aaaa = (bool)methodToRun.Invoke(mergeWorkflowViewModel, new object[] { check });
                //---------------Test Result -----------------------
                Assert.IsTrue(aaaa);
            }
        }

        [TestMethod]
        public void Save_VerifyCalls()
        {
            //---------------Set up test pack-------------------
            var applicationAdapter = new Mock<IApplicationAdaptor>();
            CustomContainer.Register(applicationAdapter.Object);

            var mockServer = new Mock<IServer>();
            var mockshell = new Mock<IShellViewModel>();
            mockshell.Setup(a => a.ActiveServer).Returns(mockServer.Object);
            mockServer.Setup(a => a.GetServerVersion()).Returns("1.0.0.0");
            CustomContainer.Register(mockServer.Object);
            CustomContainer.Register(mockshell.Object);
            var assignExample = XML.XmlResource.Fetch("Loop Constructs - For Each");
            var resourceDefinationCleaner = new ResourceDefinationCleaner();
            var resource = resourceDefinationCleaner.GetResourceDefinition(true, Guid.Empty, new StringBuilder(assignExample.ToString(System.Xml.Linq.SaveOptions.DisableFormatting)));
            var dev2JsonSerializer = new Dev2JsonSerializer();
            var msg = dev2JsonSerializer.Deserialize<ExecuteMessage>(resource);

            var currentResourceModel = Dev2MockFactory.SetupResourceModelMock();
            currentResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(msg.Message);
            currentResourceModel.Setup(resModel => resModel.DisplayName).Returns("Hello World");
            currentResourceModel.Setup(resModel => resModel.Environment.ResourceRepository.FetchResourceDefinition(It.IsAny<IServer>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>()))
              .Returns(new ExecuteMessage { Message = msg.Message, HasError = false });
            currentResourceModel.Setup(p => p.Environment.Connection.ServerEvents).Returns(new Mock<IEventPublisher>().Object);

            var differenceResourceModel = Dev2MockFactory.SetupResourceModelMock();
            differenceResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(msg.Message);
            differenceResourceModel.Setup(resModel => resModel.DisplayName).Returns("Hello World");
            var b = new ArmConnectorConflict();
            var activity = new Mock<IDev2Activity>();
            var arms = new List<(string Description, string Key, string SourceUniqueId, string DestinationUniqueId)>();
            var iniqueId = Guid.NewGuid().ToString();
            arms.Add(("a", iniqueId, iniqueId, Guid.NewGuid().ToString()));
            activity.Setup(p => p.ArmConnectors()).Returns(arms);
            activity.Setup(p => p.UniqueID).Returns(iniqueId);
            b.Key = iniqueId;
            b.UniqueId = iniqueId.ToGuid();
            Func<IConflict, bool> check = (c) => { if (c.UniqueId == Guid.Empty) { return false; } return true; };
            using (var mergeWorkflowViewModel = new MergeWorkflowViewModel(currentResourceModel.Object, differenceResourceModel.Object, false))
            {
                //---------------Assert Precondition----------------
                //---------------Execute Test ----------------------
                mergeWorkflowViewModel.Save();
                //---------------Test Result -----------------------
                currentResourceModel.Verify(p => p.Environment.ResourceRepository.SaveToServer(currentResourceModel.Object, It.IsAny<string>()));
            }
        }

        [TestMethod]
        public void Save_GivenHasWorkflowNameConflict_VerifyCalls()
        {
            //---------------Set up test pack-------------------
            var applicationAdapter = new Mock<IApplicationAdaptor>();
            CustomContainer.Register(applicationAdapter.Object);

            var mockServer = new Mock<IServer>();
            var mockshell = new Mock<IShellViewModel>();
            mockshell.Setup(a => a.ActiveServer).Returns(mockServer.Object);
            mockServer.Setup(a => a.GetServerVersion()).Returns("1.0.0.0");
            CustomContainer.Register(mockServer.Object);
            CustomContainer.Register(mockshell.Object);
            var assignExample = XML.XmlResource.Fetch("Loop Constructs - For Each");
            var resourceDefinationCleaner = new ResourceDefinationCleaner();
            var resource = resourceDefinationCleaner.GetResourceDefinition(true, Guid.Empty, new StringBuilder(assignExample.ToString(System.Xml.Linq.SaveOptions.DisableFormatting)));
            var dev2JsonSerializer = new Dev2JsonSerializer();
            var msg = dev2JsonSerializer.Deserialize<ExecuteMessage>(resource);

            var currentResourceModel = Dev2MockFactory.SetupResourceModelMock();
            currentResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(msg.Message);
            currentResourceModel.Setup(resModel => resModel.DisplayName).Returns("Hello World");
            currentResourceModel.Setup(resModel => resModel.Environment.ResourceRepository.FetchResourceDefinition(It.IsAny<IServer>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>()))
              .Returns(new ExecuteMessage { Message = msg.Message, HasError = false });
            currentResourceModel.Setup(p => p.Environment.Connection.ServerEvents).Returns(new Mock<IEventPublisher>().Object);

            var differenceResourceModel = Dev2MockFactory.SetupResourceModelMock();
            differenceResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(msg.Message);
            differenceResourceModel.Setup(resModel => resModel.DisplayName).Returns("Hello World");
            var b = new ArmConnectorConflict();
            var activity = new Mock<IDev2Activity>();
            var arms = new List<(string Description, string Key, string SourceUniqueId, string DestinationUniqueId)>();
            var iniqueId = Guid.NewGuid().ToString();
            arms.Add(("a", iniqueId, iniqueId, Guid.NewGuid().ToString()));
            activity.Setup(p => p.ArmConnectors()).Returns(arms);
            activity.Setup(p => p.UniqueID).Returns(iniqueId);
            b.Key = iniqueId;
            b.UniqueId = iniqueId.ToGuid();
            Func<IConflict, bool> check = (c) => { if (c.UniqueId == Guid.Empty) { return false; } return true; };
            currentResourceModel.Setup(p => p.Environment.ExplorerRepository.UpdateManagerProxy.Rename(It.IsAny<Guid>(), It.IsAny<string>()));
            using (var mergeWorkflowViewModel = new MergeWorkflowViewModel(currentResourceModel.Object, differenceResourceModel.Object, false))
            {
                //---------------Assert Precondition----------------
                mergeWorkflowViewModel.HasWorkflowNameConflict = true;
                //---------------Execute Test ----------------------
                mergeWorkflowViewModel.Save();
                //---------------Test Result -----------------------
                currentResourceModel.Verify(p => p.Environment.ExplorerRepository.UpdateManagerProxy.Rename(currentResourceModel.Object.ID, currentResourceModel.Object.ResourceName));
                currentResourceModel.Verify(p => p.Environment.ResourceRepository.SaveToServer(currentResourceModel.Object, It.IsAny<string>()));
            }
        }

        [TestMethod]
        public void Save_GiventHasVariableConflict_VerifyCalls_current_Is_Selected()
        {
            //---------------Set up test pack-------------------
            var applicationAdapter = new Mock<IApplicationAdaptor>();
            CustomContainer.Register(applicationAdapter.Object);

            var mockServer = new Mock<IServer>();
            var mockshell = new Mock<IShellViewModel>();
            mockshell.Setup(a => a.ActiveServer).Returns(mockServer.Object);
            mockServer.Setup(a => a.GetServerVersion()).Returns("1.0.0.0");
            CustomContainer.Register(mockServer.Object);
            CustomContainer.Register(mockshell.Object);
            var assignExample = XML.XmlResource.Fetch("Loop Constructs - For Each");
            var resourceDefinationCleaner = new ResourceDefinationCleaner();
            var resource = resourceDefinationCleaner.GetResourceDefinition(true, Guid.Empty, new StringBuilder(assignExample.ToString(System.Xml.Linq.SaveOptions.DisableFormatting)));
            var dev2JsonSerializer = new Dev2JsonSerializer();
            var msg = dev2JsonSerializer.Deserialize<ExecuteMessage>(resource);

            var currentResourceModel = Dev2MockFactory.SetupResourceModelMock();
            currentResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(msg.Message);
            currentResourceModel.Setup(resModel => resModel.DisplayName).Returns("Hello World");
            currentResourceModel.Setup(resModel => resModel.Environment.ResourceRepository.FetchResourceDefinition(It.IsAny<IServer>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>()))
              .Returns(new ExecuteMessage { Message = msg.Message, HasError = false });
            currentResourceModel.Setup(p => p.Environment.Connection.ServerEvents).Returns(new Mock<IEventPublisher>().Object);

            var differenceResourceModel = Dev2MockFactory.SetupResourceModelMock();
            differenceResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(msg.Message);
            differenceResourceModel.Setup(resModel => resModel.DisplayName).Returns("Hello World");
            var b = new ArmConnectorConflict();
            var activity = new Mock<IDev2Activity>();
            var arms = new List<(string Description, string Key, string SourceUniqueId, string DestinationUniqueId)>();
            var iniqueId = Guid.NewGuid().ToString();
            arms.Add(("a", iniqueId, iniqueId, Guid.NewGuid().ToString()));
            activity.Setup(p => p.ArmConnectors()).Returns(arms);
            activity.Setup(p => p.UniqueID).Returns(iniqueId);
            b.Key = iniqueId;
            b.UniqueId = iniqueId.ToGuid();
            Func<IConflict, bool> check = (c) => { if (c.UniqueId == Guid.Empty) { return false; } return true; };
            currentResourceModel.Setup(p => p.Environment.ExplorerRepository.UpdateManagerProxy.Rename(It.IsAny<Guid>(), It.IsAny<string>()));
            var currentFactory = new Mock<IConflictModelFactory>();
            currentFactory.Setup(p => p.DataListViewModel.WriteToResourceModel()).Verifiable();
            currentFactory.Setup(p => p.IsVariablesChecked).Returns(true).Verifiable();
            var diffFactory = new Mock<IConflictModelFactory>();
            diffFactory.Setup(p => p.DataListViewModel.WriteToResourceModel()).Verifiable();
            using (var mergeWorkflowViewModel = new MergeWorkflowViewModel(currentResourceModel.Object, differenceResourceModel.Object, false))
            {
                mergeWorkflowViewModel.CurrentConflictModel = currentFactory.Object;
                mergeWorkflowViewModel.DifferenceConflictModel = diffFactory.Object;
                //---------------Assert Precondition----------------
                mergeWorkflowViewModel.HasVariablesConflict = true;
                //---------------Execute Test ----------------------
                mergeWorkflowViewModel.Save();
                //---------------Test Result -----------------------
                currentFactory.Verify(p => p.DataListViewModel.WriteToResourceModel());
                currentFactory.Verify(p => p.IsVariablesChecked);
                currentResourceModel.VerifySet(q => q.DataList = It.IsAny<string>());
                currentResourceModel.Verify(p => p.Environment.ResourceRepository.SaveToServer(currentResourceModel.Object, It.IsAny<string>()));
            }
        }

        [TestMethod]
        public void Save_GivenHasVariableConflict_VerifyCalls_Diff_Is_Selected()
        {
            //---------------Set up test pack-------------------
            var applicationAdapter = new Mock<IApplicationAdaptor>();
            CustomContainer.Register(applicationAdapter.Object);

            var mockServer = new Mock<IServer>();
            var mockshell = new Mock<IShellViewModel>();
            mockshell.Setup(a => a.ActiveServer).Returns(mockServer.Object);
            mockServer.Setup(a => a.GetServerVersion()).Returns("1.0.0.0");
            CustomContainer.Register(mockServer.Object);
            CustomContainer.Register(mockshell.Object);
            var assignExample = XML.XmlResource.Fetch("Loop Constructs - For Each");
            var resourceDefinationCleaner = new ResourceDefinationCleaner();
            var resource = resourceDefinationCleaner.GetResourceDefinition(true, Guid.Empty, new StringBuilder(assignExample.ToString(System.Xml.Linq.SaveOptions.DisableFormatting)));
            var dev2JsonSerializer = new Dev2JsonSerializer();
            var msg = dev2JsonSerializer.Deserialize<ExecuteMessage>(resource);

            var currentResourceModel = Dev2MockFactory.SetupResourceModelMock();
            currentResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(msg.Message);
            currentResourceModel.Setup(resModel => resModel.DisplayName).Returns("Hello World");
            currentResourceModel.Setup(resModel => resModel.Environment.ResourceRepository.FetchResourceDefinition(It.IsAny<IServer>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>()))
              .Returns(new ExecuteMessage { Message = msg.Message, HasError = false });
            currentResourceModel.Setup(p => p.Environment.Connection.ServerEvents).Returns(new Mock<IEventPublisher>().Object);

            var differenceResourceModel = Dev2MockFactory.SetupResourceModelMock();
            differenceResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(msg.Message);
            differenceResourceModel.Setup(resModel => resModel.DisplayName).Returns("Hello World");
            var b = new ArmConnectorConflict();
            var activity = new Mock<IDev2Activity>();
            var arms = new List<(string Description, string Key, string SourceUniqueId, string DestinationUniqueId)>();
            var iniqueId = Guid.NewGuid().ToString();
            arms.Add(("a", iniqueId, iniqueId, Guid.NewGuid().ToString()));
            activity.Setup(p => p.ArmConnectors()).Returns(arms);
            activity.Setup(p => p.UniqueID).Returns(iniqueId);
            b.Key = iniqueId;
            b.UniqueId = iniqueId.ToGuid();
            Func<IConflict, bool> check = (c) => { if (c.UniqueId == Guid.Empty) { return false; } return true; };
            currentResourceModel.Setup(p => p.Environment.ExplorerRepository.UpdateManagerProxy.Rename(It.IsAny<Guid>(), It.IsAny<string>()));
            var currentFactory = new Mock<IConflictModelFactory>();
            var diffFactory = new Mock<IConflictModelFactory>();
            diffFactory.Setup(p => p.DataListViewModel.WriteToResourceModel()).Verifiable();
            diffFactory.Setup(p => p.IsVariablesChecked).Returns(true).Verifiable();
            using (var mergeWorkflowViewModel = new MergeWorkflowViewModel(currentResourceModel.Object, differenceResourceModel.Object, false))
            {
                mergeWorkflowViewModel.CurrentConflictModel = currentFactory.Object;
                mergeWorkflowViewModel.DifferenceConflictModel = diffFactory.Object;
                //---------------Assert Precondition----------------
                mergeWorkflowViewModel.HasVariablesConflict = true;
                //---------------Execute Test ----------------------
                mergeWorkflowViewModel.Save();
                //---------------Test Result -----------------------
                diffFactory.Verify(p => p.DataListViewModel.WriteToResourceModel());
                currentFactory.Verify(p => p.IsVariablesChecked);
                currentResourceModel.VerifySet(q => q.DataList = It.IsAny<string>());
                currentResourceModel.Verify(p => p.Environment.ResourceRepository.SaveToServer(currentResourceModel.Object, It.IsAny<string>()));
            }
        }

        [TestMethod]
        public void Save_Throws_VerifyCalls()
        {
            //---------------Set up test pack-------------------
            var applicationAdapter = new Mock<IApplicationAdaptor>();
            CustomContainer.Register(applicationAdapter.Object);

            var mockServer = new Mock<IServer>();
            var mockshell = new Mock<IShellViewModel>();
            mockshell.Setup(a => a.ActiveServer).Returns(mockServer.Object);
            mockServer.Setup(a => a.GetServerVersion()).Returns("1.0.0.0");
            CustomContainer.Register(mockServer.Object);
            CustomContainer.Register(mockshell.Object);
            var assignExample = XML.XmlResource.Fetch("Loop Constructs - For Each");
            var resourceDefinationCleaner = new ResourceDefinationCleaner();
            var resource = resourceDefinationCleaner.GetResourceDefinition(true, Guid.Empty, new StringBuilder(assignExample.ToString(System.Xml.Linq.SaveOptions.DisableFormatting)));
            var dev2JsonSerializer = new Dev2JsonSerializer();
            var msg = dev2JsonSerializer.Deserialize<ExecuteMessage>(resource);

            var currentResourceModel = Dev2MockFactory.SetupResourceModelMock();
            currentResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(msg.Message);
            currentResourceModel.Setup(resModel => resModel.DisplayName).Returns("Hello World");
            currentResourceModel.Setup(resModel => resModel.Environment.ResourceRepository.FetchResourceDefinition(It.IsAny<IServer>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>()))
              .Returns(new ExecuteMessage { Message = msg.Message, HasError = false });
            currentResourceModel.Setup(p => p.Environment.Connection.ServerEvents).Returns(new Mock<IEventPublisher>().Object);

            var differenceResourceModel = Dev2MockFactory.SetupResourceModelMock();
            differenceResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(msg.Message);
            differenceResourceModel.Setup(resModel => resModel.DisplayName).Returns("Hello World");
            var b = new ArmConnectorConflict();
            var activity = new Mock<IDev2Activity>();
            var arms = new List<(string Description, string Key, string SourceUniqueId, string DestinationUniqueId)>();
            var iniqueId = Guid.NewGuid().ToString();
            arms.Add(("a", iniqueId, iniqueId, Guid.NewGuid().ToString()));
            activity.Setup(p => p.ArmConnectors()).Returns(arms);
            activity.Setup(p => p.UniqueID).Returns(iniqueId);
            b.Key = iniqueId;
            b.UniqueId = iniqueId.ToGuid();
            Func<IConflict, bool> check = (c) => { if (c.UniqueId == Guid.Empty) { return false; } return true; };
            currentResourceModel.Setup(p => p.Environment.ExplorerRepository.UpdateManagerProxy.Rename(It.IsAny<Guid>(), It.IsAny<string>())).Throws(new Exception());
            using (var mergeWorkflowViewModel = new MergeWorkflowViewModel(currentResourceModel.Object, differenceResourceModel.Object, false))
            {
                //---------------Assert Precondition----------------
                mergeWorkflowViewModel.HasWorkflowNameConflict = true;
                //---------------Execute Test ----------------------
                mergeWorkflowViewModel.Save();
                //---------------Test Result -----------------------
                currentResourceModel.Verify(p => p.Environment.ExplorerRepository.UpdateManagerProxy.Rename(currentResourceModel.Object.ID, currentResourceModel.Object.ResourceName));
                currentResourceModel.Verify(p => p.Environment.ResourceRepository.Save(currentResourceModel.Object), Times.Never());
            }
        }

        [TestMethod]
        public void UpdateHelpDescriptor_PassThrough_VerifyCall()
        {
            //---------------Set up test pack-------------------
            var applicationAdapter = new Mock<IApplicationAdaptor>();
            CustomContainer.Register(applicationAdapter.Object);

            var mockServer = new Mock<IServer>();
            var mockshell = new Mock<IShellViewModel>();
            mockshell.Setup(a => a.ActiveServer).Returns(mockServer.Object);
            mockServer.Setup(a => a.GetServerVersion()).Returns("1.0.0.0");
            CustomContainer.Register(mockServer.Object);
            CustomContainer.Register(mockshell.Object);
            var assignExample = XML.XmlResource.Fetch("Loop Constructs - For Each");
            var resourceDefinationCleaner = new ResourceDefinationCleaner();
            var resource = resourceDefinationCleaner.GetResourceDefinition(true, Guid.Empty, new StringBuilder(assignExample.ToString(System.Xml.Linq.SaveOptions.DisableFormatting)));
            var dev2JsonSerializer = new Dev2JsonSerializer();
            var msg = dev2JsonSerializer.Deserialize<ExecuteMessage>(resource);

            var currentResourceModel = Dev2MockFactory.SetupResourceModelMock();
            currentResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(msg.Message);
            currentResourceModel.Setup(resModel => resModel.DisplayName).Returns("Hello World");
            currentResourceModel.Setup(resModel => resModel.Environment.ResourceRepository.FetchResourceDefinition(It.IsAny<IServer>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>()))
              .Returns(new ExecuteMessage { Message = msg.Message, HasError = false });
            currentResourceModel.Setup(p => p.Environment.Connection.ServerEvents).Returns(new Mock<IEventPublisher>().Object);

            var differenceResourceModel = Dev2MockFactory.SetupResourceModelMock();
            differenceResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(msg.Message);
            differenceResourceModel.Setup(resModel => resModel.DisplayName).Returns("Hello World");
            mockshell.Setup(p => p.HelpViewModel.UpdateHelpText("aaa")).Verifiable();
            var b = new ArmConnectorConflict();
            var activity = new Mock<IDev2Activity>();
            var arms = new List<(string Description, string Key, string SourceUniqueId, string DestinationUniqueId)>();
            var iniqueId = Guid.NewGuid().ToString();
            arms.Add(("a", iniqueId, iniqueId, Guid.NewGuid().ToString()));
            activity.Setup(p => p.ArmConnectors()).Returns(arms);
            activity.Setup(p => p.UniqueID).Returns(iniqueId);
            b.Key = iniqueId;
            b.UniqueId = iniqueId.ToGuid();
            Func<IConflict, bool> check = (c) => { if (c.UniqueId == Guid.Empty) { return false; } return true; };
            using (var mergeWorkflowViewModel = new MergeWorkflowViewModel(currentResourceModel.Object, differenceResourceModel.Object, false))
            {
                mergeWorkflowViewModel.UpdateHelpDescriptor("aaa");
                mockshell.Verify(p => p.HelpViewModel.UpdateHelpText("aaa"));
            }
        }

        [TestMethod]
        public void IsVariablesEnabled_Test_propertyChanges()
        {
            //---------------Set up test pack-------------------
            var applicationAdapter = new Mock<IApplicationAdaptor>();
            CustomContainer.Register(applicationAdapter.Object);

            var mockServer = new Mock<IServer>();
            var mockshell = new Mock<IShellViewModel>();
            mockshell.Setup(a => a.ActiveServer).Returns(mockServer.Object);
            mockServer.Setup(a => a.GetServerVersion()).Returns("1.0.0.0");
            CustomContainer.Register(mockServer.Object);
            CustomContainer.Register(mockshell.Object);
            var assignExample = XML.XmlResource.Fetch("Loop Constructs - For Each");
            var resourceDefinationCleaner = new ResourceDefinationCleaner();
            var resource = resourceDefinationCleaner.GetResourceDefinition(true, Guid.Empty, new StringBuilder(assignExample.ToString(System.Xml.Linq.SaveOptions.DisableFormatting)));
            var dev2JsonSerializer = new Dev2JsonSerializer();
            var msg = dev2JsonSerializer.Deserialize<ExecuteMessage>(resource);

            var currentResourceModel = Dev2MockFactory.SetupResourceModelMock();
            currentResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(msg.Message);
            currentResourceModel.Setup(resModel => resModel.DisplayName).Returns("Hello World");
            currentResourceModel.Setup(resModel => resModel.Environment.ResourceRepository.FetchResourceDefinition(It.IsAny<IServer>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>()))
              .Returns(new ExecuteMessage { Message = msg.Message, HasError = false });
            currentResourceModel.Setup(p => p.Environment.Connection.ServerEvents).Returns(new Mock<IEventPublisher>().Object);

            var differenceResourceModel = Dev2MockFactory.SetupResourceModelMock();
            differenceResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(msg.Message);
            differenceResourceModel.Setup(resModel => resModel.DisplayName).Returns("Hello World");
            mockshell.Setup(p => p.HelpViewModel.UpdateHelpText("aaa")).Verifiable();
            var b = new ArmConnectorConflict();
            var activity = new Mock<IDev2Activity>();
            var arms = new List<(string Description, string Key, string SourceUniqueId, string DestinationUniqueId)>();
            var iniqueId = Guid.NewGuid().ToString();
            arms.Add(("a", iniqueId, iniqueId, Guid.NewGuid().ToString()));
            activity.Setup(p => p.ArmConnectors()).Returns(arms);
            activity.Setup(p => p.UniqueID).Returns(iniqueId);
            b.Key = iniqueId;
            b.UniqueId = iniqueId.ToGuid();
            using (var mergeWorkflowViewModel = new MergeWorkflowViewModel(currentResourceModel.Object, differenceResourceModel.Object, false))
            {
                mergeWorkflowViewModel.UpdateHelpDescriptor("aaa");
                mockshell.Verify(p => p.HelpViewModel.UpdateHelpText("aaa"));
                var wasCalled = false;

                Assert.IsFalse(mergeWorkflowViewModel.IsVariablesEnabled);
                mergeWorkflowViewModel.PropertyChanged += (a, p) =>
                {
                    if (p.PropertyName == "IsVariablesEnabled")
                    {
                        wasCalled = true;
                    }
                };

                mergeWorkflowViewModel.IsVariablesEnabled = true;
                Assert.IsTrue(wasCalled);
            }
        }

        [TestMethod]
        public void HasVariablesConflict_Test_propertyChanges()
        {
            //---------------Set up test pack-------------------
            var applicationAdapter = new Mock<IApplicationAdaptor>();
            CustomContainer.Register(applicationAdapter.Object);

            var mockServer = new Mock<IServer>();
            var mockshell = new Mock<IShellViewModel>();
            mockshell.Setup(a => a.ActiveServer).Returns(mockServer.Object);
            mockServer.Setup(a => a.GetServerVersion()).Returns("1.0.0.0");
            CustomContainer.Register(mockServer.Object);
            CustomContainer.Register(mockshell.Object);
            var assignExample = XML.XmlResource.Fetch("Loop Constructs - For Each");
            var resourceDefinationCleaner = new ResourceDefinationCleaner();
            var resource = resourceDefinationCleaner.GetResourceDefinition(true, Guid.Empty, new StringBuilder(assignExample.ToString(System.Xml.Linq.SaveOptions.DisableFormatting)));
            var dev2JsonSerializer = new Dev2JsonSerializer();
            var msg = dev2JsonSerializer.Deserialize<ExecuteMessage>(resource);

            var currentResourceModel = Dev2MockFactory.SetupResourceModelMock();
            currentResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(msg.Message);
            currentResourceModel.Setup(resModel => resModel.DisplayName).Returns("Hello World");
            currentResourceModel.Setup(resModel => resModel.Environment.ResourceRepository.FetchResourceDefinition(It.IsAny<IServer>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>()))
              .Returns(new ExecuteMessage { Message = msg.Message, HasError = false });
            currentResourceModel.Setup(p => p.Environment.Connection.ServerEvents).Returns(new Mock<IEventPublisher>().Object);

            var differenceResourceModel = Dev2MockFactory.SetupResourceModelMock();
            differenceResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(msg.Message);
            differenceResourceModel.Setup(resModel => resModel.DisplayName).Returns("Hello World");
            mockshell.Setup(p => p.HelpViewModel.UpdateHelpText("aaa")).Verifiable();
            var b = new ArmConnectorConflict();
            var activity = new Mock<IDev2Activity>();
            var arms = new List<(string Description, string Key, string SourceUniqueId, string DestinationUniqueId)>();
            var iniqueId = Guid.NewGuid().ToString();
            arms.Add(("a", iniqueId, iniqueId, Guid.NewGuid().ToString()));
            activity.Setup(p => p.ArmConnectors()).Returns(arms);
            activity.Setup(p => p.UniqueID).Returns(iniqueId);
            b.Key = iniqueId;
            b.UniqueId = iniqueId.ToGuid();
            using (var mergeWorkflowViewModel = new MergeWorkflowViewModel(currentResourceModel.Object, differenceResourceModel.Object, false))
            {
                mergeWorkflowViewModel.UpdateHelpDescriptor("aaa");
                mockshell.Verify(p => p.HelpViewModel.UpdateHelpText("aaa"));
                var wasCalled = false;

                Assert.IsFalse(mergeWorkflowViewModel.HasVariablesConflict);
                mergeWorkflowViewModel.PropertyChanged += (a, p) =>
                {
                    if (p.PropertyName == "HasVariablesConflict")
                    {
                        wasCalled = true;
                    }
                };

                mergeWorkflowViewModel.HasVariablesConflict = true;
                Assert.IsTrue(wasCalled);
            }
        }

        [TestMethod]
        public void HasWorkflowNameConflict_Test_propertyChanges()
        {
            //---------------Set up test pack-------------------
            var applicationAdapter = new Mock<IApplicationAdaptor>();
            CustomContainer.Register(applicationAdapter.Object);

            var mockServer = new Mock<IServer>();
            var mockshell = new Mock<IShellViewModel>();
            mockshell.Setup(a => a.ActiveServer).Returns(mockServer.Object);
            mockServer.Setup(a => a.GetServerVersion()).Returns("1.0.0.0");
            CustomContainer.Register(mockServer.Object);
            CustomContainer.Register(mockshell.Object);
            var assignExample = XML.XmlResource.Fetch("Loop Constructs - For Each");
            var resourceDefinationCleaner = new ResourceDefinationCleaner();
            var resource = resourceDefinationCleaner.GetResourceDefinition(true, Guid.Empty, new StringBuilder(assignExample.ToString(System.Xml.Linq.SaveOptions.DisableFormatting)));
            var dev2JsonSerializer = new Dev2JsonSerializer();
            var msg = dev2JsonSerializer.Deserialize<ExecuteMessage>(resource);

            var currentResourceModel = Dev2MockFactory.SetupResourceModelMock();
            currentResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(msg.Message);
            currentResourceModel.Setup(resModel => resModel.DisplayName).Returns("Hello World");
            currentResourceModel.Setup(resModel => resModel.Environment.ResourceRepository.FetchResourceDefinition(It.IsAny<IServer>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>()))
              .Returns(new ExecuteMessage { Message = msg.Message, HasError = false });
            currentResourceModel.Setup(p => p.Environment.Connection.ServerEvents).Returns(new Mock<IEventPublisher>().Object);

            var differenceResourceModel = Dev2MockFactory.SetupResourceModelMock();
            differenceResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(msg.Message);
            differenceResourceModel.Setup(resModel => resModel.DisplayName).Returns("Hello World");
            mockshell.Setup(p => p.HelpViewModel.UpdateHelpText("aaa")).Verifiable();
            var b = new ArmConnectorConflict();
            var activity = new Mock<IDev2Activity>();
            var arms = new List<(string Description, string Key, string SourceUniqueId, string DestinationUniqueId)>();
            var iniqueId = Guid.NewGuid().ToString();
            arms.Add(("a", iniqueId, iniqueId, Guid.NewGuid().ToString()));
            activity.Setup(p => p.ArmConnectors()).Returns(arms);
            activity.Setup(p => p.UniqueID).Returns(iniqueId);
            b.Key = iniqueId;
            b.UniqueId = iniqueId.ToGuid();
            using (var mergeWorkflowViewModel = new MergeWorkflowViewModel(currentResourceModel.Object, differenceResourceModel.Object, false))
            {
                mergeWorkflowViewModel.UpdateHelpDescriptor("aaa");
                mockshell.Verify(p => p.HelpViewModel.UpdateHelpText("aaa"));
                var wasCalled = false;

                Assert.IsFalse(mergeWorkflowViewModel.HasWorkflowNameConflict);
                mergeWorkflowViewModel.PropertyChanged += (a, p) =>
                {
                    if (p.PropertyName == "HasWorkflowNameConflict")
                    {
                        wasCalled = true;
                    }
                };

                mergeWorkflowViewModel.HasWorkflowNameConflict = true;
                Assert.IsTrue(wasCalled);
            }
        }

        [TestMethod]
        public void DataListViewModel_Test_propertyChanges()
        {
            //---------------Set up test pack-------------------
            var applicationAdapter = new Mock<IApplicationAdaptor>();
            CustomContainer.Register(applicationAdapter.Object);

            var mockServer = new Mock<IServer>();
            var mockshell = new Mock<IShellViewModel>();
            mockshell.Setup(a => a.ActiveServer).Returns(mockServer.Object);
            mockServer.Setup(a => a.GetServerVersion()).Returns("1.0.0.0");
            CustomContainer.Register(mockServer.Object);
            CustomContainer.Register(mockshell.Object);
            var assignExample = XML.XmlResource.Fetch("Loop Constructs - For Each");
            var resourceDefinationCleaner = new ResourceDefinationCleaner();
            var resource = resourceDefinationCleaner.GetResourceDefinition(true, Guid.Empty, new StringBuilder(assignExample.ToString(System.Xml.Linq.SaveOptions.DisableFormatting)));
            var dev2JsonSerializer = new Dev2JsonSerializer();
            var msg = dev2JsonSerializer.Deserialize<ExecuteMessage>(resource);

            var currentResourceModel = Dev2MockFactory.SetupResourceModelMock();
            currentResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(msg.Message);
            currentResourceModel.Setup(resModel => resModel.DisplayName).Returns("Hello World");
            currentResourceModel.Setup(resModel => resModel.Environment.ResourceRepository.FetchResourceDefinition(It.IsAny<IServer>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>()))
              .Returns(new ExecuteMessage { Message = msg.Message, HasError = false });
            currentResourceModel.Setup(p => p.Environment.Connection.ServerEvents).Returns(new Mock<IEventPublisher>().Object);

            var differenceResourceModel = Dev2MockFactory.SetupResourceModelMock();
            differenceResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(msg.Message);
            differenceResourceModel.Setup(resModel => resModel.DisplayName).Returns("Hello World");
            mockshell.Setup(p => p.HelpViewModel.UpdateHelpText("aaa")).Verifiable();
            var b = new ArmConnectorConflict();
            var activity = new Mock<IDev2Activity>();
            var arms = new List<(string Description, string Key, string SourceUniqueId, string DestinationUniqueId)>();
            var iniqueId = Guid.NewGuid().ToString();
            arms.Add(("a", iniqueId, iniqueId, Guid.NewGuid().ToString()));
            activity.Setup(p => p.ArmConnectors()).Returns(arms);
            activity.Setup(p => p.UniqueID).Returns(iniqueId);
            b.Key = iniqueId;
            b.UniqueId = iniqueId.ToGuid();
            using (var mergeWorkflowViewModel = new MergeWorkflowViewModel(currentResourceModel.Object, differenceResourceModel.Object, false))
            {
                mergeWorkflowViewModel.UpdateHelpDescriptor("aaa");
                mockshell.Verify(p => p.HelpViewModel.UpdateHelpText("aaa"));
                var wasCalled = false;

                Assert.IsNotNull(mergeWorkflowViewModel.DataListViewModel);
                mergeWorkflowViewModel.PropertyChanged += (a, p) =>
                {
                    if (p.PropertyName == "DataListViewModel")
                    {
                        wasCalled = true;
                    }
                };

                var mockDataList = new Mock<IDataListViewModel>().Object;
                mergeWorkflowViewModel.DataListViewModel = mockDataList;
                Assert.AreSame(mockDataList, mergeWorkflowViewModel.DataListViewModel);

                Assert.IsTrue(wasCalled);
            }
        }

        [TestMethod]
        public void HasMergeStarted_Test_propertyChanges()
        {
            //---------------Set up test pack-------------------
            var applicationAdapter = new Mock<IApplicationAdaptor>();
            CustomContainer.Register(applicationAdapter.Object);

            var mockServer = new Mock<IServer>();
            var mockshell = new Mock<IShellViewModel>();
            mockshell.Setup(a => a.ActiveServer).Returns(mockServer.Object);
            mockServer.Setup(a => a.GetServerVersion()).Returns("1.0.0.0");
            CustomContainer.Register(mockServer.Object);
            CustomContainer.Register(mockshell.Object);
            var assignExample = XML.XmlResource.Fetch("Loop Constructs - For Each");
            var resourceDefinationCleaner = new ResourceDefinationCleaner();
            var resource = resourceDefinationCleaner.GetResourceDefinition(true, Guid.Empty, new StringBuilder(assignExample.ToString(System.Xml.Linq.SaveOptions.DisableFormatting)));
            var dev2JsonSerializer = new Dev2JsonSerializer();
            var msg = dev2JsonSerializer.Deserialize<ExecuteMessage>(resource);

            var currentResourceModel = Dev2MockFactory.SetupResourceModelMock();
            currentResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(msg.Message);
            currentResourceModel.Setup(resModel => resModel.DisplayName).Returns("Hello World");
            currentResourceModel.Setup(resModel => resModel.Environment.ResourceRepository.FetchResourceDefinition(It.IsAny<IServer>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>()))
              .Returns(new ExecuteMessage { Message = msg.Message, HasError = false });
            currentResourceModel.Setup(p => p.Environment.Connection.ServerEvents).Returns(new Mock<IEventPublisher>().Object);

            var differenceResourceModel = Dev2MockFactory.SetupResourceModelMock();
            differenceResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(msg.Message);
            differenceResourceModel.Setup(resModel => resModel.DisplayName).Returns("Hello World");
            mockshell.Setup(p => p.HelpViewModel.UpdateHelpText("aaa")).Verifiable();
            var b = new ArmConnectorConflict();
            var activity = new Mock<IDev2Activity>();
            var arms = new List<(string Description, string Key, string SourceUniqueId, string DestinationUniqueId)>();
            var iniqueId = Guid.NewGuid().ToString();
            arms.Add(("a", iniqueId, iniqueId, Guid.NewGuid().ToString()));
            activity.Setup(p => p.ArmConnectors()).Returns(arms);
            activity.Setup(p => p.UniqueID).Returns(iniqueId);
            b.Key = iniqueId;
            b.UniqueId = iniqueId.ToGuid();
            using (var mergeWorkflowViewModel = new MergeWorkflowViewModel(currentResourceModel.Object, differenceResourceModel.Object, false))
            {
                mergeWorkflowViewModel.UpdateHelpDescriptor("aaa");
                mockshell.Verify(p => p.HelpViewModel.UpdateHelpText("aaa"));
                var wasCalled = false;

                Assert.IsFalse(mergeWorkflowViewModel.HasMergeStarted);
                mergeWorkflowViewModel.PropertyChanged += (a, p) =>
                {
                    if (p.PropertyName == "HasMergeStarted")
                    {
                        wasCalled = true;
                    }
                };

                mergeWorkflowViewModel.HasMergeStarted = true;

                Assert.IsTrue(wasCalled);
                Assert.IsFalse(mergeWorkflowViewModel.IsDirty);
            }
        }
    }
}