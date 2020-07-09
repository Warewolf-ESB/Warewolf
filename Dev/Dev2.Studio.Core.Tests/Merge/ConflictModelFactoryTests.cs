using Dev2.Activities;
using Dev2.Activities.Designers2.Decision;
using Dev2.Activities.Designers2.Service;
using Dev2.Activities.Designers2.Switch;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Communication;
using Dev2.Data.SystemTemplates.Models;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Interfaces;
using Dev2.ViewModels.Merge;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Text;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Core.Tests.Merge
{
    [TestClass]
    public class ConflictModelFactoryTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ConflictModelFactory_Constructor_DefaultConstruction_ShouldHaveChildren()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var conflictModelFactory = new ConflictModelFactory();
            //------------Assert Results-------------------------
            Assert.IsNotNull(conflictModelFactory);
            Assert.IsTrue(string.IsNullOrEmpty(conflictModelFactory.ServerName));
            Assert.IsTrue(string.IsNullOrEmpty(conflictModelFactory.WorkflowName));
            Assert.IsNull(conflictModelFactory.DataListViewModel);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ConflictModelFactory_AutoProperties_tests()
        {
            //------------Setup for test--------------------------
            var model = new ConflictModelFactory();
            //------------Execute Test---------------------------
            Assert.AreEqual(default(string), model.ServerName);
            Assert.AreEqual(default(string), model.WorkflowName);
            //------------Assert Results-------------------------

            model.ServerName = "";
            model.WorkflowName = "";

            Assert.AreNotEqual(default(string), model.ServerName);
            Assert.AreNotEqual(default(string), model.WorkflowName);

        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ConflictModelFactory_IsVariablesChecked_DefaultConstruction_ShouldBeFalse()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var completeConflict = new ConflictModelFactory();
            var wasCalled = false;
            completeConflict.PropertyChanged += (a, b) =>
            {
                if (b.PropertyName == "IsVariablesChecked")
                {
                    wasCalled = true;
                }
            };
            //------------Assert Results-------------------------
            Assert.IsNotNull(completeConflict);
            Assert.IsFalse(completeConflict.IsVariablesChecked);
            completeConflict.IsVariablesChecked = true;
            Assert.IsTrue(wasCalled);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ConflictModelFactory_IsWorkflowNameChecked_DefaultConstruction_ShouldBeFalse()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var completeConflict = new ConflictModelFactory();
            var wasCalled = false;
            completeConflict.PropertyChanged += (a, b) =>
            {
                if (b.PropertyName == "IsWorkflowNameChecked")
                {
                    wasCalled = true;
                }
            };
            //------------Assert Results-------------------------
            Assert.IsNotNull(completeConflict);
            Assert.IsFalse(completeConflict.IsWorkflowNameChecked);
            completeConflict.IsWorkflowNameChecked = true;
            Assert.IsTrue(wasCalled);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ConflictModelFactory_GivenAssignConflictNode_ShouldReturnMergeToolModel()
        {
            //------------Setup for test--------------------------
            var adapter = new Mock<IApplicationAdaptor>();
            adapter.Setup(p => p.TryFindResource(It.IsAny<object>())).Returns(new object());
            CustomContainer.Register(adapter.Object);
            var node = new Mock<IConflictTreeNode>();
            var contextualResource = new Mock<IContextualResourceModel>();
            var value = new DsfMultiAssignActivity();
            var assignStep = new FlowStep
            {
                Action = value
            };
            node.Setup(p => p.Activity).Returns(value);
            var toolConflictItem = new ToolConflictItem(new ViewModels.Merge.Utils.ConflictRowList(new Mock<IConflictModelFactory>().Object, new Mock<IConflictModelFactory>().Object,new List<ConflictTreeNode>(), new List<ConflictTreeNode>()),ViewModels.Merge.Utils.ConflictRowList.Column.Current);
            //------------Execute Test---------------------------
            var completeConflict = new ConflictModelFactory(toolConflictItem, contextualResource.Object, node.Object);
            //------------Assert Results-------------------------
            Assert.IsNotNull(completeConflict);
            adapter.Verify(p => p.TryFindResource(It.IsAny<object>()));
        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetDataList_GivenResourceModel_ShouldReturnMergeToolModel()
        {
            //------------Setup for test--------------------------
            var adapter = new Mock<IApplicationAdaptor>();
            adapter.Setup(p => p.TryFindResource(It.IsAny<object>())).Returns(new object());
            CustomContainer.Register(adapter.Object);
            var node = new Mock<IConflictTreeNode>();
            var contextualResource = new Mock<IContextualResourceModel>();
            var value = new DsfMultiAssignActivity();
            var assignStep = new FlowStep
            {
                Action = value
            };
            node.Setup(p => p.Activity).Returns(value);
            var assignExample = XML.XmlResource.Fetch("Utility - Assign");
            var jsonSerializer = new Dev2JsonSerializer();
            var currentResourceModel = Dev2MockFactory.SetupResourceModelMock();
            var assignExampleBuilder = new StringBuilder(assignExample.ToString(System.Xml.Linq.SaveOptions.DisableFormatting));
            currentResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(assignExampleBuilder);
            currentResourceModel.Setup(resModel => resModel.DisplayName).Returns("Hello World");
            var toolConflictItem = new ToolConflictItem(new ViewModels.Merge.Utils.ConflictRowList(new Mock<IConflictModelFactory>().Object, new Mock<IConflictModelFactory>().Object, new List<ConflictTreeNode>(), new List<ConflictTreeNode>()), ViewModels.Merge.Utils.ConflictRowList.Column.Current);
            //------------Execute Test---------------------------
            var completeConflict = new ConflictModelFactory(toolConflictItem, contextualResource.Object, node.Object);
            //------------Assert Results-------------------------
            Assert.IsNotNull(completeConflict);
            completeConflict.GetDataList(currentResourceModel.Object);

            Assert.AreEqual(7, completeConflict.DataListViewModel.DataList.Count);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetDataList_GivenEmptyResourceModel_ShouldReturnReturnEmpty()
        {
            //------------Setup for test--------------------------
            var adapter = new Mock<IApplicationAdaptor>();
            adapter.Setup(p => p.TryFindResource(It.IsAny<object>())).Returns(new object());
            CustomContainer.Register(adapter.Object);
            var node = new Mock<IConflictTreeNode>();
            var contextualResource = new Mock<IContextualResourceModel>();
            var value = new DsfMultiAssignActivity();
            var assignStep = new FlowStep
            {
                Action = value
            };
            node.Setup(p => p.Activity).Returns(value);
            var assignExample = XML.XmlResource.Fetch("AssignOutput");
            var jsonSerializer = new Dev2JsonSerializer();
            var currentResourceModel = Dev2MockFactory.SetupResourceModelMock();
            var assignExampleBuilder = new StringBuilder(assignExample.ToString(System.Xml.Linq.SaveOptions.DisableFormatting));
            currentResourceModel.Setup(resModel => resModel.WorkflowXaml).Returns(assignExampleBuilder);
            currentResourceModel.Setup(resModel => resModel.DisplayName).Returns("Hello World");
            currentResourceModel.Setup(resModel => resModel.DataList).Returns("");
            var toolConflictItem = new ToolConflictItem(new ViewModels.Merge.Utils.ConflictRowList(new Mock<IConflictModelFactory>().Object, new Mock<IConflictModelFactory>().Object, new List<ConflictTreeNode>(), new List<ConflictTreeNode>()), ViewModels.Merge.Utils.ConflictRowList.Column.Current);
            //------------Execute Test---------------------------
            var completeConflict = new ConflictModelFactory(toolConflictItem, contextualResource.Object, node.Object);
            //------------Assert Results-------------------------
            Assert.IsNotNull(completeConflict);
            completeConflict.GetDataList(currentResourceModel.Object);

            Assert.AreEqual(2, completeConflict.DataListViewModel.DataList.Count);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ConflictModelFactory_GivenDsfDecisionConflictNode_ShouldReturnMergeToolModel()
        {
            //------------Setup for test--------------------------
            var adapter = new Mock<IApplicationAdaptor>();
            adapter.Setup(p => p.TryFindResource(It.IsAny<object>())).Returns(new object());
            CustomContainer.Register(adapter.Object);
            var node = new Mock<IConflictTreeNode>();
            var contextualResource = new Mock<IContextualResourceModel>();
            var dev2DecisionStack = new Dev2DecisionStack
            {
                TheStack = new List<Dev2Decision>(),
                TrueArmText = "a",
                FalseArmText = "a",
                DisplayText = "a",
                Mode = Dev2DecisionMode.AND
            };
            var serializer = new Dev2JsonSerializer();
            var serialize = serializer.Serialize(dev2DecisionStack);
            var condition = new DsfFlowDecisionActivity
            {
                ExpressionText = serialize
            };
            var value = new DsfDecision(condition)
            {
                Conditions = dev2DecisionStack
            };
            var assignStep = new FlowStep
            {
                Action = value
            };
            node.Setup(p => p.Activity).Returns(value);
            var toolConflictItem = new ToolConflictItem(new ViewModels.Merge.Utils.ConflictRowList(new Mock<IConflictModelFactory>().Object, new Mock<IConflictModelFactory>().Object, new List<ConflictTreeNode>(), new List<ConflictTreeNode>()), ViewModels.Merge.Utils.ConflictRowList.Column.Current);
            //------------Execute Test---------------------------
            var completeConflict = new ConflictModelFactory(toolConflictItem, contextualResource.Object, node.Object);
            //------------Assert Results-------------------------
            Assert.IsNotNull(completeConflict);
            adapter.Verify(p => p.TryFindResource(It.IsAny<object>()));
            var mergeToolModel = completeConflict.CreateModelItem(toolConflictItem, node.Object);
            Assert.AreEqual("a", mergeToolModel.MergeDescription);
            Assert.AreEqual(typeof(DecisionDesignerViewModel).FullName, ((ToolConflictItem)mergeToolModel).ActivityDesignerViewModel.GetType().FullName);
        }
        
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ConflictModelFactory_GivenServiceConflictNode_ShouldReturnMergeToolModel()
        {
            //------------Setup for test--------------------------
            var adapter = new Mock<IApplicationAdaptor>();
            var severRepo = new Mock<IServerRepository>();
            severRepo.Setup(p => p.ActiveServer).Returns(new Mock<IServer>().Object);
            CustomContainer.Register(severRepo.Object);
            adapter.Setup(p => p.TryFindResource(It.IsAny<object>())).Returns(new object());
            CustomContainer.Register(adapter.Object);
            var node = new Mock<IConflictTreeNode>();
            var contextualResource = new Mock<IContextualResourceModel>();
            var value = new DsfActivity();
            var assignStep = new FlowStep
            {
                Action = value
            };
            var currentResourceModel = Dev2MockFactory.SetupResourceModelMock();
            node.Setup(p => p.Activity).Returns(value);
            contextualResource.Setup(p => p.Environment.ResourceRepository.LoadContextualResourceModel(It.IsAny<Guid>())).Returns(currentResourceModel.Object);
            var toolConflictItem = new ToolConflictItem(new ViewModels.Merge.Utils.ConflictRowList(new Mock<IConflictModelFactory>().Object, new Mock<IConflictModelFactory>().Object, new List<ConflictTreeNode>(), new List<ConflictTreeNode>()), ViewModels.Merge.Utils.ConflictRowList.Column.Current);
            //------------Execute Test---------------------------

            var completeConflict = new ConflictModelFactory(toolConflictItem, contextualResource.Object, node.Object);
            //------------Assert Results-------------------------
            Assert.IsNotNull(completeConflict);
            adapter.Verify(p => p.TryFindResource(It.IsAny<object>()));
            var mergeToolModel = completeConflict.CreateModelItem(toolConflictItem, node.Object);
            Assert.AreEqual("DsfActivity", mergeToolModel.MergeDescription);
            Assert.AreEqual(typeof(ServiceDesignerViewModel).FullName, ((ToolConflictItem)mergeToolModel).ActivityDesignerViewModel.GetType().FullName);
        }
        
        [TestMethod]
        [Owner("Candice Daniel")]
        public void ConflictModelFactory_GivenServiceConflictNode_NullResourceID()
        {
            //------------Setup for test--------------------------
            var adapter = new Mock<IApplicationAdaptor>();
            var severRepo = new Mock<IServerRepository>();
            severRepo.Setup(p => p.ActiveServer).Returns(new Mock<IServer>().Object);
            CustomContainer.Register(severRepo.Object);
            adapter.Setup(p => p.TryFindResource(It.IsAny<object>())).Returns(new object());
            CustomContainer.Register(adapter.Object);
            var node = new Mock<IConflictTreeNode>();
            var contextualResource = new Mock<IContextualResourceModel>();
            var value = new DsfActivity();
            var assignStep = new FlowStep
            {
                Action = value
            };
            var currentResourceModel = Dev2MockFactory.SetupResourceModelMock();
            node.Setup(p => p.Activity).Returns(value);
            contextualResource.Setup(r => r.Environment.ResourceRepository.LoadContextualResourceModel(Guid.Empty)).Returns((IContextualResourceModel)null);
            var toolConflictItem = new ToolConflictItem(new ViewModels.Merge.Utils.ConflictRowList(new Mock<IConflictModelFactory>().Object, new Mock<IConflictModelFactory>().Object, new List<ConflictTreeNode>(), new List<ConflictTreeNode>()), ViewModels.Merge.Utils.ConflictRowList.Column.Current);
            //------------Execute Test---------------------------

            var completeConflict = new ConflictModelFactory(toolConflictItem, contextualResource.Object, node.Object);
            //------------Assert Results-------------------------
            Assert.IsNotNull(completeConflict);
            adapter.Verify(p => p.TryFindResource(It.IsAny<object>()));
            var mergeToolModel = completeConflict.CreateModelItem(toolConflictItem, node.Object);
            Assert.AreEqual("DsfActivity", mergeToolModel.MergeDescription);
            Assert.AreEqual(typeof(ServiceDesignerViewModel).FullName, ((ToolConflictItem)mergeToolModel).ActivityDesignerViewModel.GetType().FullName);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ConflictModelFactory_GivenDsfSwitchConflictNode_ShouldReturnMergeToolModel()
        {
            //------------Setup for test--------------------------
            var adapter = new Mock<IApplicationAdaptor>();
            adapter.Setup(p => p.TryFindResource(It.IsAny<object>())).Returns(new object());
            CustomContainer.Register(adapter.Object);
            var node = new Mock<IConflictTreeNode>();
            var contextualResource = new Mock<IContextualResourceModel>();
            var dev2DecisionStack = new Dev2DecisionStack
            {
                TheStack = new List<Dev2Decision>(),
                TrueArmText = "a",
                FalseArmText = "a",
                DisplayText = "a",
                Mode = Dev2DecisionMode.AND
            };
            var serializer = new Dev2JsonSerializer();
            var serialize = serializer.Serialize(dev2DecisionStack);
            var condition = new DsfFlowSwitchActivity
            {
                ExpressionText = serialize
            };
            var value = new DsfSwitch(condition)
            {
                Switch = "bbb",
                Switches = new Dictionary<string, IDev2Activity>
                {
                    {"a", new DsfCalculateActivity() },
                    {"b", new DsfCalculateActivity() }
                }
            };
            var assignStep = new FlowStep
            {
                Action = value
            };
            node.Setup(p => p.Activity).Returns(value);
            var toolConflictItem = new ToolConflictItem(new ViewModels.Merge.Utils.ConflictRowList(new Mock<IConflictModelFactory>().Object, new Mock<IConflictModelFactory>().Object, new List<ConflictTreeNode>(), new List<ConflictTreeNode>()), ViewModels.Merge.Utils.ConflictRowList.Column.Current);
            //------------Execute Test---------------------------
            var completeConflict = new ConflictModelFactory(toolConflictItem, contextualResource.Object, node.Object);
            //------------Assert Results-------------------------
            Assert.IsNotNull(completeConflict);
            var mergeToolModel = completeConflict.CreateModelItem(toolConflictItem, node.Object);
            Assert.AreEqual("bbb", mergeToolModel.MergeDescription);
            Assert.AreEqual(typeof(SwitchDesignerViewModel).FullName, ((ToolConflictItem)mergeToolModel).ActivityDesignerViewModel.GetType().FullName);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ConflictModelFactory_GivenNullConflictNodeActivity_ShouldReturnNull()
        {
            //------------Setup for test--------------------------
            var adapter = new Mock<IApplicationAdaptor>();
            adapter.Setup(p => p.TryFindResource(It.IsAny<object>())).Returns(new object());
            CustomContainer.Register(adapter.Object);
            var node = new Mock<IConflictTreeNode>();
            var contextualResource = new Mock<IContextualResourceModel>();

            node.Setup(p => p.Activity).Returns(new DsfCalculateActivity());
            var toolConflictItem = new ToolConflictItem(new ViewModels.Merge.Utils.ConflictRowList(new Mock<IConflictModelFactory>().Object, new Mock<IConflictModelFactory>().Object, new List<ConflictTreeNode>(), new List<ConflictTreeNode>()), ViewModels.Merge.Utils.ConflictRowList.Column.Current);
            //------------Execute Test---------------------------
            var completeConflict = new ConflictModelFactory(toolConflictItem, contextualResource.Object, node.Object);
            //------------Assert Results-------------------------
            var mergeToolModel = completeConflict.CreateModelItem(toolConflictItem, node.Object);
            Assert.IsNotNull(mergeToolModel);
        }
    }
}
