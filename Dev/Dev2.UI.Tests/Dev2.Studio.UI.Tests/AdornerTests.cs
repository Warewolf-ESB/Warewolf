using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Dev2.CodedUI.Tests;
using Dev2.CodedUI.Tests.TabManagerUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.DocManagerUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.ToolboxUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.WorkflowDesignerUIMapClasses;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Studio.UI.Tests
{
    [CodedUITest]
    public class AdornerTests
    {
        #region TabManager UI Map

        public TabManagerUIMap TabManagerUIMap
        {
            get
            {
                if (_tabManagerUIMap == null)
                {
                    _tabManagerUIMap = new TabManagerUIMap();
                }

                return _tabManagerUIMap;
            }
        }

        private TabManagerUIMap _tabManagerUIMap;

        #endregion TabManager UI Map

        #region WorkflowDesigner UI Map

        public WorkflowDesignerUIMap WorkflowDesignerUIMap
        {
            get
            {
                if (_workflowDesignerUIMap == null)
                {
                    _workflowDesignerUIMap = new WorkflowDesignerUIMap();
                }

                return _workflowDesignerUIMap;
            }
        }

        private WorkflowDesignerUIMap _workflowDesignerUIMap;

        #endregion WorkflowDesigner UI Map

        public void CreateWorkflow()
        {
            Keyboard.SendKeys(DocManagerUIMap.UIBusinessDesignStudioWindow, "{CTRL}W");
        }

        #region DocManager UI Map

        public DocManagerUIMap DocManagerUIMap
        {
            get
            {
                if ((_docManagerMap == null))
                {
                    _docManagerMap = new DocManagerUIMap();
                }

                return _docManagerMap;
            }
        }

        private DocManagerUIMap _docManagerMap;

        #endregion

        #region Toolbox UI Map

        public ToolboxUIMap ToolboxUIMap
        {
            get
            {
                if ((_toolboxUIMap == null))
                {
                    _toolboxUIMap = new ToolboxUIMap();
                }

                return _toolboxUIMap;
            }
        }

        private ToolboxUIMap _toolboxUIMap;

        #endregion

        public void DoCleanup(string workflowName, bool clickNo = false)
        {
            try
            {
                // Test complete - Delete itself  
                if (clickNo)
                {
                    TabManagerUIMap.CloseTab_Click_No(workflowName);
                }
                else
                {
                    TabManagerUIMap.CloseTab(workflowName);
                }
            }
            catch (Exception e)
            {
                // Log it so the UI Test still passes...
                Trace.WriteLine(e.Message);
            }
        }

        [TestMethod]
        [TestCategory("UITest")]
        [Description("for bug 9969- large view adorners")]
        [Owner("Jurie")]
        public void DateTimeLargeViewAdornerTest()
        {
            #region Test Strings

            string input = "14.10.1988";
            string inputFormat = "dd.mm.yyyy";           
            string result = "[[Result]]";

            #endregion

            // Create the workflow
            CreateWorkflow();

            //// Get some design surface
            UITestControl theTab = TabManagerUIMap.FindTabByName("Unsaved 1");
            Point requiredPoint = WorkflowDesignerUIMap.GetPointUnderStartNode(theTab);
            requiredPoint.Offset(20, 50);

            // Drag the tool onto the workflow
            DocManagerUIMap.ClickOpenTabPage("Toolbox");
            UITestControl theControl = ToolboxUIMap.FindToolboxItemByAutomationId("DateTime");
            ToolboxUIMap.DragControlToWorkflowDesigner(theControl, requiredPoint);
            //Open the adorner
            WorkflowDesignerUIMap.Adorner_ClickLargeView(theTab);
            //Get all the textboxes in the adorner
            List<UITestControl> adornerAllTextBoxes = WorkflowDesignerUIMap.Adorner_GetAllTextBoxes(theTab);
            if (adornerAllTextBoxes.Count == 5)
            {
                //Fill text into the textboxes
                Mouse.Click(adornerAllTextBoxes[0]);
                SendKeys.SendWait(input);
                Mouse.Click(adornerAllTextBoxes[1]);
                SendKeys.SendWait(inputFormat);
        
                Mouse.Click(adornerAllTextBoxes[4]);
                SendKeys.SendWait(result);
            }
            //Close the adorner
            WorkflowDesignerUIMap.Adorner_ClickLargeView(theTab);

            //Get all the textboxes from the tool
            List<UITestControl> dateTimeAllTextBoxes = WorkflowDesignerUIMap.Tool_GetAllTextBoxes(theTab, "Date and Time(DsfDateTimeActivityDesigner)", "Uia.DsfDateTimeActivityTemplate");

            //Check the the text entered in the adorner is the same as what is on the tool now
            Assert.AreEqual(5,dateTimeAllTextBoxes.Count,"The wrong number of textboxes was returned for the date tine tool");
            Assert.AreEqual(input,(dateTimeAllTextBoxes[0] as WpfEdit).Text,"The text entered in the adorner is not the same as what is shown on the tool");
            Assert.AreEqual(inputFormat, (dateTimeAllTextBoxes[1] as WpfEdit).Text, "The text entered in the adorner is not the same as what is shown on the tool");
            Assert.AreEqual(result, (dateTimeAllTextBoxes[4] as WpfEdit).Text, "The text entered in the adorner is not the same as what is shown on the tool");

            #region Do Clean Up

            TestBase tb = new TestBase();
            tb.DoCleanup("Unsaved 1",true);

            #endregion
        }
    }
}
