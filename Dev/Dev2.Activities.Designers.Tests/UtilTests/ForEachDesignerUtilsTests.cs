using System.Activities.Presentation.Model;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Windows;
using Dev2.Activities.Utils;
using Dev2.Studio.Core.Activities.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Unlimited.Applications.BusinessDesignStudio.Activities;

// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace
namespace Dev2.Core.Tests.Activities
{
    [TestClass]
    public class ForEachDesignerUtilsTests
    {
        [TestInitialize]
        public void MyTestInitialize()
        {
            Castle.DynamicProxy.Generators.AttributesToAvoidReplicating.Add(typeof(System.Security.Permissions.UIPermissionAttribute));
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ForeachActivityDesignerUtils_LimitDragDropOptions")]
        public void ForeachActivityDesignerUtils_LimitDragDropOptions_NoFormats_EnableDrop()
        {
            //------------Setup for test--------------------------
            var forEachUtils = new ForeachActivityDesignerUtils();
            var dataObject = new Mock<IDataObject>();
            dataObject.Setup(o => o.GetFormats()).Returns(new string[] { });
            //------------Execute Test---------------------------
            var dropEnabled = forEachUtils.LimitDragDropOptions(dataObject.Object);
            //------------Assert Results-------------------------
            Assert.IsTrue(dropEnabled);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ForeachActivityDesignerUtils_LimitDragDropOptions")]
        public void ForeachActivityDesignerUtils_LimitDragDropOptions_NoWorkflowItemTypeNameFormatAndNoModelItemFormat_EnableDrop()
        {
            //------------Setup for test--------------------------
            var forEachUtils = new ForeachActivityDesignerUtils();
            var dataObject = new Mock<IDataObject>();
            dataObject.Setup(o => o.GetFormats()).Returns(new[] { "SomeOtherFormat" });
            //------------Execute Test---------------------------
            var dropEnabled = forEachUtils.LimitDragDropOptions(dataObject.Object);
            //------------Assert Results-------------------------
            Assert.IsTrue(dropEnabled);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ForeachActivityDesignerUtils_LimitDragDropOptions")]
        public void ForeachActivityDesignerUtils_LimitDragDropOptions_ModelItemFormat_Decision_DropPrevented()
        {
            //------------Setup for test--------------------------
            var forEachUtils = new ForeachActivityDesignerUtils();
            var dataObject = new Mock<IDataObject>();
            dataObject.Setup(o => o.GetFormats()).Returns(new[] { "ModelItemFormat" });
            dataObject.Setup(o => o.GetData(It.IsAny<string>())).Returns("Decision");
            //------------Execute Test---------------------------
            var dropEnabled = forEachUtils.LimitDragDropOptions(dataObject.Object);
            //------------Assert Results-------------------------
            Assert.IsFalse(dropEnabled);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("ForeachActivityDesignerUtils_LimitDragDropOptions")]
        public void ForeachActivityDesignerUtils_LimitDragDropOptions_ModelItemsFormat_WithDecisionAndCount_DropPrevented()
        {
            //------------Setup for test--------------------------
            var forEachUtils = new ForeachActivityDesignerUtils();
            var dataObject = new Mock<IDataObject>();
            dataObject.Setup(o => o.GetFormats()).Returns(new[] { "ModelItemsFormat" });
            var modelItemList = new List<ModelItem> { ModelItemUtils.CreateModelItem(new DsfCountRecordsetActivity()), ModelItemUtils.CreateModelItem(new FlowDecision()) };
            dataObject.Setup(o => o.GetData(It.IsAny<string>())).Returns(modelItemList);
            //------------Execute Test---------------------------
            var dropEnabled = forEachUtils.LimitDragDropOptions(dataObject.Object);
            //------------Assert Results-------------------------
            Assert.IsFalse(dropEnabled);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("ForeachActivityDesignerUtils_LimitDragDropOptions")]
        public void ForeachActivityDesignerUtils_LimitDragDropOptions_ModelItemsFormat_WithSwitchAndCount_DropPrevented()
        {
            //------------Setup for test--------------------------
            var forEachUtils = new ForeachActivityDesignerUtils();
            var dataObject = new Mock<IDataObject>();
            dataObject.Setup(o => o.GetFormats()).Returns(new[] { "ModelItemsFormat" });
            var modelItemList = new List<ModelItem> { ModelItemUtils.CreateModelItem(new DsfCountRecordsetActivity()), ModelItemUtils.CreateModelItem(new FlowSwitch<string>()) };
            dataObject.Setup(o => o.GetData(It.IsAny<string>())).Returns(modelItemList);
            //------------Execute Test---------------------------
            var dropEnabled = forEachUtils.LimitDragDropOptions(dataObject.Object);
            //------------Assert Results-------------------------
            Assert.IsFalse(dropEnabled);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ForeachActivityDesignerUtils_LimitDragDropOptions")]
        public void ForeachActivityDesignerUtils_LimitDragDropOptions_ModelItemFormat_Switch_DropPrevented()
        {
            //------------Setup for test--------------------------
            var forEachUtils = new ForeachActivityDesignerUtils();
            var dataObject = new Mock<IDataObject>();
            dataObject.Setup(o => o.GetFormats()).Returns(new[] { "ModelItemFormat" });
            dataObject.Setup(o => o.GetData(It.IsAny<string>())).Returns("Switch");
            //------------Execute Test---------------------------
            var dropEnabled = forEachUtils.LimitDragDropOptions(dataObject.Object);
            //------------Assert Results-------------------------
            Assert.IsFalse(dropEnabled);
        }
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ForeachActivityDesignerUtils_LimitDragDropOptions")]
        public void ForeachActivityDesignerUtils_LimitDragDropOptions_ModelItemFormat_NotDecision_DropNotPrevented()
        {
            //------------Setup for test--------------------------
            var forEachUtils = new ForeachActivityDesignerUtils();
            var dataObject = new Mock<IDataObject>();
            dataObject.Setup(o => o.GetFormats()).Returns(new[] { "ModelItemFormat" });
            dataObject.Setup(o => o.GetData(It.IsAny<string>())).Returns("Act");
            //------------Execute Test---------------------------
            var dropEnabled = forEachUtils.LimitDragDropOptions(dataObject.Object);
            //------------Assert Results-------------------------
            Assert.IsTrue(dropEnabled);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ForeachActivityDesignerUtils_LimitDragDropOptions")]
        public void ForeachActivityDesignerUtils_LimitDragDropOptions_ModelItemFormat_NotSwitch_DropNotPrevented()
        {
            //------------Setup for test--------------------------
            var forEachUtils = new ForeachActivityDesignerUtils();
            var dataObject = new Mock<IDataObject>();
            dataObject.Setup(o => o.GetFormats()).Returns(new[] { "ModelItemFormat" });
            dataObject.Setup(o => o.GetData(It.IsAny<string>())).Returns("Activity");
            //------------Execute Test---------------------------
            var dropEnabled = forEachUtils.LimitDragDropOptions(dataObject.Object);
            //------------Assert Results-------------------------
            Assert.IsTrue(dropEnabled);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ForeachActivityDesignerUtils_LimitDragDropOptions")]
        public void ForeachActivityDesignerUtils_LimitDragDropOptions_WorkflowItemTypeNameFormat_Decision_DropPrevented()
        {
            //------------Setup for test--------------------------
            var forEachUtils = new ForeachActivityDesignerUtils();
            var dataObject = new Mock<IDataObject>();
            dataObject.Setup(o => o.GetFormats()).Returns(new[] { "WorkflowItemTypeNameFormat" });
            dataObject.Setup(o => o.GetData(It.IsAny<string>())).Returns("Decision");
            //------------Execute Test---------------------------
            var dropEnabled = forEachUtils.LimitDragDropOptions(dataObject.Object);
            //------------Assert Results-------------------------
            Assert.IsFalse(dropEnabled);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ForeachActivityDesignerUtils_LimitDragDropOptions")]
        public void ForeachActivityDesignerUtils_LimitDragDropOptions_WorkflowItemTypeNameFormat_Switch_DropPrevented()
        {
            //------------Setup for test--------------------------
            var forEachUtils = new ForeachActivityDesignerUtils();
            var dataObject = new Mock<IDataObject>();
            dataObject.Setup(o => o.GetFormats()).Returns(new[] { "WorkflowItemTypeNameFormat" });
            dataObject.Setup(o => o.GetData(It.IsAny<string>())).Returns("Switch");
            //------------Execute Test---------------------------
            var dropEnabled = forEachUtils.LimitDragDropOptions(dataObject.Object);
            //------------Assert Results-------------------------
            Assert.IsFalse(dropEnabled);
        }
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ForeachActivityDesignerUtils_LimitDragDropOptions")]
        public void ForeachActivityDesignerUtils_LimitDragDropOptions_WorkflowItemTypeNameFormat_NotDecision_DropNotPrevented()
        {
            //------------Setup for test--------------------------
            var forEachUtils = new ForeachActivityDesignerUtils();
            var dataObject = new Mock<IDataObject>();
            dataObject.Setup(o => o.GetFormats()).Returns(new[] { "WorkflowItemTypeNameFormat" });
            dataObject.Setup(o => o.GetData(It.IsAny<string>())).Returns("Act");
            //------------Execute Test---------------------------
            var dropEnabled = forEachUtils.LimitDragDropOptions(dataObject.Object);
            //------------Assert Results-------------------------
            Assert.IsTrue(dropEnabled);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ForeachActivityDesignerUtils_LimitDragDropOptions")]
        public void ForeachActivityDesignerUtils_LimitDragDropOptions_WorkflowItemTypeNameFormat_NotSwitch_DropNotPrevented()
        {
            //------------Setup for test--------------------------
            var forEachUtils = new ForeachActivityDesignerUtils();
            var dataObject = new Mock<IDataObject>();
            dataObject.Setup(o => o.GetFormats()).Returns(new[] { "WorkflowItemTypeNameFormat" });
            dataObject.Setup(o => o.GetData(It.IsAny<string>())).Returns("Activity");
            //------------Execute Test---------------------------
            var dropEnabled = forEachUtils.LimitDragDropOptions(dataObject.Object);
            //------------Assert Results-------------------------
            Assert.IsTrue(dropEnabled);
        }



        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ForeachActivityDesignerUtils_LimitDragDropOptions")]
        public void ForeachActivityDesignerUtils_LimitDragDropOptions_ModelItemFormat_DecisionModelItem_DropPrevented()
        {
            //------------Setup for test--------------------------
            var forEachUtils = new ForeachActivityDesignerUtils();
            var dataObject = new Mock<IDataObject>();
            dataObject.Setup(o => o.GetFormats()).Returns(new[] { "ModelItemFormat" });
            var modelItem = ModelItemUtils.CreateModelItem(new FlowDecision());
            dataObject.Setup(o => o.GetData(It.IsAny<string>())).Returns(modelItem);
            //------------Execute Test---------------------------
            var dropEnabled = forEachUtils.LimitDragDropOptions(dataObject.Object);
            //------------Assert Results-------------------------
            Assert.IsFalse(dropEnabled);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ForeachActivityDesignerUtils_LimitDragDropOptions")]
        public void ForeachActivityDesignerUtils_LimitDragDropOptions_ModelItemFormat_SwitchModelItem_DropPrevented()
        {
            //------------Setup for test--------------------------
            var forEachUtils = new ForeachActivityDesignerUtils();
            var dataObject = new Mock<IDataObject>();
            dataObject.Setup(o => o.GetFormats()).Returns(new[] { "ModelItemFormat" });
            var modelItem = ModelItemUtils.CreateModelItem(new FlowSwitch<string>());
            dataObject.Setup(o => o.GetData(It.IsAny<string>())).Returns(modelItem);
            //------------Execute Test---------------------------
            var dropEnabled = forEachUtils.LimitDragDropOptions(dataObject.Object);
            //------------Assert Results-------------------------
            Assert.IsFalse(dropEnabled);
        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ForeachActivityDesignerUtils_LimitDragDropOptions")]
        public void ForeachActivityDesignerUtils_LimitDragDropOptions_ModelItemFormat_NotSwitchDecision_DropNotPrevented()
        {
            //------------Setup for test--------------------------
            var forEachUtils = new ForeachActivityDesignerUtils();
            var dataObject = new Mock<IDataObject>();
            dataObject.Setup(o => o.GetFormats()).Returns(new[] { "ModelItemFormat" });
            var modelItem = ModelItemUtils.CreateModelItem(new DsfMultiAssignActivity());
            dataObject.Setup(o => o.GetData(It.IsAny<string>())).Returns(modelItem);
            //------------Execute Test---------------------------
            var dropEnabled = forEachUtils.LimitDragDropOptions(dataObject.Object);
            //------------Assert Results-------------------------
            Assert.IsTrue(dropEnabled);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ForeachActivityDesignerUtils_LimitDragDropOptions")]
        public void ForeachActivityDesignerUtils_LimitDragDropOptions_WorkflowItemTypeNameFormat_DecisionModelItem_DropPrevented()
        {
            //------------Setup for test--------------------------
            var forEachUtils = new ForeachActivityDesignerUtils();
            var dataObject = new Mock<IDataObject>();
            dataObject.Setup(o => o.GetFormats()).Returns(new[] { "WorkflowItemTypeNameFormat" });
            var modelItem = ModelItemUtils.CreateModelItem(new FlowDecision());
            dataObject.Setup(o => o.GetData(It.IsAny<string>())).Returns(modelItem);
            //------------Execute Test---------------------------
            var dropEnabled = forEachUtils.LimitDragDropOptions(dataObject.Object);
            //------------Assert Results-------------------------
            Assert.IsFalse(dropEnabled);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ForeachActivityDesignerUtils_LimitDragDropOptions")]
        public void ForeachActivityDesignerUtils_LimitDragDropOptions_WorkflowItemTypeNameFormat_SwitchModelItem_DropPrevented()
        {
            //------------Setup for test--------------------------
            var forEachUtils = new ForeachActivityDesignerUtils();
            var dataObject = new Mock<IDataObject>();
            dataObject.Setup(o => o.GetFormats()).Returns(new[] { "WorkflowItemTypeNameFormat" });
            var modelItem = ModelItemUtils.CreateModelItem(new FlowSwitch<string>());
            dataObject.Setup(o => o.GetData(It.IsAny<string>())).Returns(modelItem);
            //------------Execute Test---------------------------
            var dropEnabled = forEachUtils.LimitDragDropOptions(dataObject.Object);
            //------------Assert Results-------------------------
            Assert.IsFalse(dropEnabled);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ForeachActivityDesignerUtils_LimitDragDropOptions")]
        public void ForeachActivityDesignerUtils_LimitDragDropOptions_WorkflowItemTypeNameFormat_NotSwitchDecisionModelItem_DropNotPrevented()
        {
            //------------Setup for test--------------------------
            var forEachUtils = new ForeachActivityDesignerUtils();
            var dataObject = new Mock<IDataObject>();
            dataObject.Setup(o => o.GetFormats()).Returns(new[] { "WorkflowItemTypeNameFormat" });
            var modelItem = ModelItemUtils.CreateModelItem(new DsfMultiAssignActivity());
            dataObject.Setup(o => o.GetData(It.IsAny<string>())).Returns(modelItem);
            //------------Execute Test---------------------------
            var dropEnabled = forEachUtils.LimitDragDropOptions(dataObject.Object);
            //------------Assert Results-------------------------
            Assert.IsTrue(dropEnabled);
        }

    }
}
