using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.Tools.Control_Flow
{
    [CodedUITest]
    public class Sequence
    {
        [TestMethod]
		[TestCategory("Tools")]
        public void SequenceTool_OpenLargeViewUITest()
        {            
            UIMap.Open_Sequence_Large_tool_View();
        }

        [TestMethod]
		[TestCategory("Tools")]
        public void ToolDesigners_SequenceLargeView_DraggingNonDecision_Allowed()
        {
            UIMap.Open_Sequence_Large_tool_View();
            UIMap.Drag_Toolbox_AssignObject_Onto_Sequence_LargeTool();
        }

        [TestMethod]
		[TestCategory("Tools")]
        public void ToolDesigners_SequenceLargeView_DraggingSwitch_NotAllowed_UITest()
        {         
            UIMap.Open_Sequence_Large_tool_View();
            UIMap.Drag_Toolbox_Switch_Onto_Sequence_LargeTool();
            Assert.IsTrue(UIMap.MessageBoxWindow.Exists, "Message box does not exist");
            UIMap.Click_Drop_Not_Allowed_MessageBox_OK();
        }

        [TestMethod]
		[TestCategory("Tools")]
        public void ToolDesigners_SequenceLargeView_DraggingDecision_NotAllowed_UITest()
        {         
            UIMap.Open_Sequence_Large_tool_View();
            UIMap.Drag_Toolbox_Decision_Onto_Sequence_LargeTool();
            Assert.IsTrue(UIMap.MessageBoxWindow.Exists, "Message box does not exist");
            UIMap.Click_Drop_Not_Allowed_MessageBox_OK();
        }
        

        [TestMethod]
		[TestCategory("Tools")]
        public void ToolDesigners_SequenceSmallView_DraggingNonDecision_Allowed()
        {            
            UIMap.Drag_Toolbox_AssignObject_Onto_Sequence_SmallTool();
        }

        [TestMethod]
		[TestCategory("Tools")]
        public void ToolDesigners_SequenceSmallView_DraggingSwitch_NotAllowed_UITest()
        {
            UIMap.Drag_Toolbox_Switch_Onto_Sequence_SmallTool();
            Assert.IsTrue(UIMap.MessageBoxWindow.Exists, "Message box does not exist");
            UIMap.Click_Drop_Not_Allowed_MessageBox_OK();
        }

        [TestMethod]
		[TestCategory("Tools")]
        public void ToolDesigners_SequenceSmallView_DraggingDecision_NotAllowed_UITest()
        {
            UIMap.Drag_Toolbox_Decision_Onto_Sequence_SmallTool();
            Assert.IsTrue(UIMap.MessageBoxWindow.Exists, "Message box does not exist");
            UIMap.Click_Drop_Not_Allowed_MessageBox_OK();
        }
        

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
            UIMap.Click_New_Workflow_Ribbon_Button();
            UIMap.Drag_Toolbox_Sequence_Onto_DesignSurface();
        }

        UIMap UIMap
        {
            get
            {
                if (_uiMap == null)
                {
                    _uiMap = new UIMap();
                }

                return _uiMap;
            }
        }

        private UIMap _uiMap;

        #endregion
    }
}
