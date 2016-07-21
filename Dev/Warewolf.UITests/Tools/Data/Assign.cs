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
        [TestMethod]
        public void AssignToolUITest()
        {
            Uimap.Click_New_Workflow_Ribbon_Button();
            Uimap.Drag_Toolbox_MultiAssign_Onto_DesignSurface();
            Uimap.Open_Assign_Tool_Large_View();
            Uimap.Enter_Text_Into_Assign_Large_View_Row1_Variable_Textbox_As_SomeVariable();
            Uimap.Click_Assign_Tool_Large_View_Done_Button();
            Uimap.Open_Assign_Tool_Qvi_Large_View();
            Uimap.Click_Save_Ribbon_Button_to_Open_Save_Dialog();
            Uimap.Enter_Servicename_As_SomeWorkflow();
            Uimap.Click_SaveDialog_YesButton();
            Uimap.Enter_SomeWorkflow_Into_Explorer_Filter();

            /**TODO: Re-introduce these units before WOLF-1923 can be moved to done.
            Uimap.Click_Debug_Ribbon_Button();
            Uimap.Click_Debug_Input_Dialog_Debug_ButtonParams.AssignToolDebugOutputExists = true;
            Uimap.Click_DebugInput_DebugButton();
            **/
        }

        #region Additional test attributes

        // You can use the following additional attributes as you write your tests:

        //Use TestInitialize to run code before running each test 
        [TestInitialize]
        public void MyTestInitialize()
        {
            Uimap.SetGlobalPlaybackSettings();
            Uimap.WaitIfStudioDoesNotExist();
            Console.WriteLine("Test \"" + TestContext.TestName + "\" starting on " + System.Environment.MachineName);
        }

        //Use TestCleanup to run code after each test has run
        [TestCleanup]
        public void MyTestCleanup()
        {
            //Action Unit: Try close any hanging save dialogs
            if (Uimap.SaveDialogWindow.CancelButton.Exists)
            {
                Uimap.Click_SaveDialog_CancelButton();
            }
            Uimap.RightClick_Explorer_Localhost_First_Item();
            Uimap.Select_Delete_FromExplorerContextMenu();
            Uimap.Click_MessageBox_Yes();
            Uimap.Enter_SomeWorkflow_Into_Explorer_FilterParams.FirstItemExists = false;
            Uimap.Enter_SomeWorkflow_Into_Explorer_Filter();
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
