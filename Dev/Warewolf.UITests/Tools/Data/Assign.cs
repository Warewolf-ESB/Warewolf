using System;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.Tools.Data
{
    /// <summary>
    /// Summary description for WorkflowDesignSurface
    /// </summary>
    [CodedUITest]
    public class Assign
    {
        const string WorkflowName = "SomeWorkflow";

        [TestMethod]
		[TestCategory("Tools")]
        public void AssignToolUITest()
        {
            Uimap.Click_New_Workflow_Ribbon_Button();
            Uimap.Drag_Toolbox_MultiAssign_Onto_DesignSurface();
            Uimap.Open_Assign_Tool_Large_View();
            Uimap.Enter_Text_Into_Assign_Large_View_Row1_Variable_Textbox_As_SomeInvalidVariableName();
            Uimap.Click_Assign_Tool_Large_View_Done_Button_With_Row1_Variable_Textbox_As_SomeInvalidVariableName();
            Uimap.Enter_Text_Into_Assign_Large_View_Row1_Variable_Textbox_As_SomeVariable();
            Uimap.Click_Assign_Tool_Large_View_Done_Button();
            Uimap.Enter_Text_Into_Assign_Small_View_Row1_Value_Textbox_As_SomeVariable_UsingIntellisense();
            Uimap.Enter_Text_Into_Assign_Small_View_Row1_Value_Textbox_As_SomeVariable_Using_Click_Intellisense_Suggestion();
            Uimap.Open_Assign_Tool_Qvi_Large_View();
            Uimap.Save_With_Ribbon_Button_And_Dialog(WorkflowName);
            Uimap.Click_Debug_Ribbon_Button();
            Uimap.Click_DebugInput_Debug_Button();
            Uimap.WaitForControlNotVisible(Uimap.MainStudioWindow.DockManager.SplitPaneRight.DebugOutput.StatusBar.Spinner);
            Uimap.Click_Debug_Output_Assign_Cell();
            Uimap.Assert_URL_exist();
            Uimap.Click_Assign_Tool_url();
            Uimap.Click_Assign_Tool_ExpandAll();
        }

        #region Additional test attributes
        
        [TestInitialize]
        public void MyTestInitialize()
        {
            Uimap.SetGlobalPlaybackSettings();
            Uimap.WaitForStudioStart();
            Console.WriteLine("Test \"" + TestContext.TestName + "\" starting on " + System.Environment.MachineName);
        }
        
        [TestCleanup]
        public void MyTestCleanup()
        {
            Playback.PlaybackError -= Uimap.OnError;
            Uimap.TryCloseHangingSaveDialog();
            Uimap.TryRemoveFromExplorer(WorkflowName);
            Uimap.TryClearToolboxFilter();
            Uimap.TryCloseWorkflowTabs();
            Uimap.TryCloseHangingDebugInputDialog();
        }
        
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        private TestContext testContextInstance;

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
