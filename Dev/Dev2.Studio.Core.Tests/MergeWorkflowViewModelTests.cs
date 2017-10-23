using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Dev2.Studio.Interfaces;
using System.Text;
using Dev2.Studio.Core.Interfaces;
using System.Windows;
using Dev2.Common.Interfaces.Studio.Controller;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using System.Collections.Generic;
using System;
using System.Activities;
using Dev2.Studio.Core.Activities.Utils;
using System.Activities.Presentation.Model;
using Dev2.ViewModels.Merge;
using Warewolf.MergeParser;
using Dev2.Activities;
using Dev2.Communication;
using Dev2.Common.Interfaces.Infrastructure.Events;
using Dev2.Common.Interfaces;

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
            var mockParseServiceForDifferences = new Mock<IServiceDifferenceParser>();

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
            Dev2JsonSerializer jsonSerializer = new Dev2JsonSerializer();
            Mock<IContextualResourceModel> currentResourceModel = Dev2MockFactory.SetupResourceModelMock();
            StringBuilder assignExampleBuilder = new StringBuilder(assignExample.ToString(System.Xml.Linq.SaveOptions.DisableFormatting));
            currentResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(assignExampleBuilder);
            IResourceDefinationCleaner resourceDefination = new ResourceDefinationCleaner();
            var cleanDef = resourceDefination.GetResourceDefinition(true, currentResourceModel.Object.ID, assignExampleBuilder);
            var msg = jsonSerializer.Deserialize<ExecuteMessage>(cleanDef);
            currentResourceModel.Setup(resModel => resModel.DisplayName).Returns("Hello World");
            currentResourceModel.Setup(resModel => resModel.Environment.ResourceRepository.FetchResourceDefinition(It.IsAny<IServer>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>()))
                .Returns(new ExecuteMessage() { Message = msg.Message, HasError = false });

            currentResourceModel.Setup(p => p.Environment.Connection.ServerEvents).Returns(new Mock<IEventPublisher>().Object);


            Mock<IContextualResourceModel> differenceResourceModel = Dev2MockFactory.SetupResourceModelMock();
            differenceResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(assignExampleBuilder);
            differenceResourceModel.Setup(resModel => resModel.DisplayName).Returns("Hello World");

            var mergeWorkflowViewModel = new MergeWorkflowViewModel(currentResourceModel.Object, differenceResourceModel.Object, false);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.IsNotNull(mergeWorkflowViewModel.CurrentConflictModel);
            Assert.IsNotNull(mergeWorkflowViewModel.DifferenceConflictModel);
        }

        private static List<ModelItem> CreateChanges(ref Guid assignId, ref Guid foreachId)
        {
            var dsfMultiAssignActivity = new DsfMultiAssignActivity()
            {
                UniqueID = assignId.ToString(),
                FieldsCollection = new List<ActivityDTO>()
                {
                    new ActivityDTO("a","a",1),
                    new ActivityDTO("a","a",2)
                }
            };
            var dsfForEachActivity = new DsfForEachActivity()
            {
                UniqueID = foreachId.ToString(),
                DataFunc = new ActivityFunc<string, bool>()
                {
                    Handler = new DsfDateTimeActivity()
                }
            };
            var assignOne = ModelItemUtils.CreateModelItem(dsfMultiAssignActivity);
            var forEach = ModelItemUtils.CreateModelItem(dsfForEachActivity);
            var currentChanges = new List<ModelItem>()
            {
                assignOne,forEach
            };
            return currentChanges;
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

            Mock<IContextualResourceModel> currentResourceModel = Dev2MockFactory.SetupResourceModelMock();
            var assignExample = XML.XmlResource.Fetch("Utility - Assign");
            StringBuilder assignExampleBuilder = new StringBuilder(assignExample.ToString(System.Xml.Linq.SaveOptions.DisableFormatting));
            currentResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(assignExampleBuilder);
            IResourceDefinationCleaner resourceDefination = new ResourceDefinationCleaner();
            Dev2JsonSerializer jsonSerializer = new Dev2JsonSerializer();
            var cleanDef = resourceDefination.GetResourceDefinition(true, currentResourceModel.Object.ID, assignExampleBuilder);
            var msg = jsonSerializer.Deserialize<ExecuteMessage>(cleanDef);
            currentResourceModel.Setup(resModel => resModel.DisplayName).Returns("Hello World");
            currentResourceModel.Setup(resModel => resModel.Environment.ResourceRepository.FetchResourceDefinition(It.IsAny<IServer>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>()))
              .Returns(new ExecuteMessage() { Message = msg.Message, HasError = false });
            currentResourceModel.Setup(p => p.Environment.Connection.ServerEvents).Returns(new Mock<IEventPublisher>().Object);

            var assignExampleDiff = XML.XmlResource.Fetch("Utility - Assign_Confilct");
            Mock<IContextualResourceModel> differenceResourceModel = Dev2MockFactory.SetupResourceModelMock();
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
            ResourceDefinationCleaner resourceDefinationCleaner = new ResourceDefinationCleaner();
            var resource = resourceDefinationCleaner.GetResourceDefinition(true, Guid.Empty, new StringBuilder(assignExample.ToString(System.Xml.Linq.SaveOptions.DisableFormatting)));
            Dev2JsonSerializer dev2JsonSerializer = new Dev2JsonSerializer();
            var msg = dev2JsonSerializer.Deserialize<ExecuteMessage>(resource);

            Mock<IContextualResourceModel> currentResourceModel = Dev2MockFactory.SetupResourceModelMock();
            currentResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(msg.Message);
            currentResourceModel.Setup(resModel => resModel.DisplayName).Returns("Hello World");
            currentResourceModel.Setup(resModel => resModel.Environment.ResourceRepository.FetchResourceDefinition(It.IsAny<IServer>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>()))
              .Returns(new ExecuteMessage() { Message = msg.Message, HasError = false });
            currentResourceModel.Setup(p => p.Environment.Connection.ServerEvents).Returns(new Mock<IEventPublisher>().Object);

            Mock<IContextualResourceModel> differenceResourceModel = Dev2MockFactory.SetupResourceModelMock();
            differenceResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(msg.Message);
            differenceResourceModel.Setup(resModel => resModel.DisplayName).Returns("Hello World");

            var mergeWorkflowViewModel = new MergeWorkflowViewModel(currentResourceModel.Object, differenceResourceModel.Object, false);
            //---------------Assert Precondition----------------
            Assert.AreNotSame(currentResourceModel, differenceResourceModel);
            //---------------Execute Test ----------------------
            Assert.IsNotNull(mergeWorkflowViewModel.CurrentConflictModel);
            Assert.IsNotNull(mergeWorkflowViewModel.DifferenceConflictModel);
            //---------------Test Result -----------------------
            Assert.AreEqual(7, mergeWorkflowViewModel.Conflicts.Count);

        }
    }
}
