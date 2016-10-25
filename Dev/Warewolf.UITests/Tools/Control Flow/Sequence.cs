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
            Uimap.Open_Sequence_Large_tool_View();
        }

        [TestMethod]
		[TestCategory("Tools")]
        public void ToolDesigners_SequenceLargeView_DraggingNonDecision_Allowed()
        {
            Uimap.Open_Sequence_Large_tool_View();
            Uimap.Drag_Toolbox_AssignObject_Onto_Sequence_LargeTool();
        }

        [TestMethod]
		[TestCategory("Tools")]
        public void ToolDesigners_SequenceLargeView_DraggingSwitch_NotAllowed_UITest()
        {         
            Uimap.Open_Sequence_Large_tool_View();
            Uimap.Drag_Toolbox_Switch_Onto_Sequence_LargeTool();
            Assert.IsTrue(Uimap.MessageBoxWindow.Exists);
            Assert.AreEqual("Drop not allowed", Uimap.MessageBoxWindow.DropnotallowedText.DisplayText
                , "Error message is not about being unable to drop switch onto the Sequence tool");
            Uimap.Click_MessageBox_OK();
        }

        [TestMethod]
		[TestCategory("Tools")]
        public void ToolDesigners_SequenceLargeView_DraggingDecision_NotAllowed_UITest()
        {         
            Uimap.Open_Sequence_Large_tool_View();
            Uimap.Drag_Toolbox_Decision_Onto_Sequence_LargeTool();
            Assert.IsTrue(Uimap.MessageBoxWindow.Exists);
            Assert.AreEqual("Drop not allowed", Uimap.MessageBoxWindow.DropnotallowedText.DisplayText
                , "Error message is not about being unable to drop decision onto the Sequence tool");
            Uimap.Click_MessageBox_OK();
        }
        

        [TestMethod]
		[TestCategory("Tools")]
        public void ToolDesigners_SequenceSmallView_DraggingNonDecision_Allowed()
        {            
            Uimap.Drag_Toolbox_AssignObject_Onto_Sequence_SmallTool();
        }

        [TestMethod]
		[TestCategory("Tools")]
        public void ToolDesigners_SequenceSmallView_DraggingSwitch_NotAllowed_UITest()
        {
            Uimap.Drag_Toolbox_Switch_Onto_Sequence_SmallTool();
            Assert.IsTrue(Uimap.MessageBoxWindow.Exists);
            Assert.AreEqual("Drop not allowed", Uimap.MessageBoxWindow.DropnotallowedText.DisplayText
                , "Error message is not about being unable to drop switch onto the Sequence tool");
            Uimap.Click_MessageBox_OK();
        }

        [TestMethod]
		[TestCategory("Tools")]
        public void ToolDesigners_SequenceSmallView_DraggingDecision_NotAllowed_UITest()
        {
            Uimap.Drag_Toolbox_Decision_Onto_Sequence_SmallTool();
            Assert.IsTrue(Uimap.MessageBoxWindow.Exists);
            Assert.AreEqual("Drop not allowed", Uimap.MessageBoxWindow.DropnotallowedText.DisplayText
                , "Error message is not about being unable to drop decision onto the Sequence tool");
            Uimap.Click_MessageBox_OK();
        }
        

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            Uimap.SetPlaybackSettings();
#if !DEBUG
            Uimap.CloseHangingDialogs();
#endif
            Uimap.Click_New_Workflow_Ribbon_Button();
            Uimap.Drag_Toolbox_Sequence_Onto_DesignSurface();
        }

        UIMap Uimap
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
