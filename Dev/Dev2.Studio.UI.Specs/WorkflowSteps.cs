using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Dev2.Studio.UI.Tests;
using Dev2.Studio.UI.Tests.UIMaps.Activities;
using Dev2.Studio.UI.Tests.Utils;
using Microsoft.VisualStudio.TestTools.UITest.Extension;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;

namespace Dev2.Studio.UI.Specs
{
    [Binding]
    public class WorkflowSteps : UIMapBase
    {

        const string Explorer = "Z3d0e8544bdbd4fbc8b0369ecfce4e928,Explorer,UI_ExplorerPane_AutoID,UI_ExplorerControl_AutoID,TheNavigationView";
        const string Toolbox = "UI_ToolboxPane_AutoID,UI_ToolboxControl_AutoID";
        const string Worksurface = "UI_SplitPane_AutoID,UI_TabManager_AutoID,Dev2.Studio.ViewModels.Workflow.WorkflowDesignerViewModel,Dev2.Studio.ViewModels.WorkSurface.WorkSurfaceContextViewModel,WorkflowDesignerView,UserControl_1,scrollViewer,ActivityTypeDesigner,WorkflowItemPresenter,Unsaved 1(FlowchartDesigner)";
        const string DebugOutput = "Z746a647dd6004001a7df7a7ca0ac65d1,Z96bb9badc4b148518ea4eff80920f8d9,OutputPane,DebugOutput,DebugOutputTree";
        // const string ToolBoxSearch = ToolBox+",PART_SearchBox";

        // const string ToolMultiAssign = ToolBox + ",PART_Tools,Data,Unlimited.Applications.BusinessDesignStudio.Activities.DsfMultiAssignActivity";


        [BeforeTestRun]
        public static void SetupForTest()
        {
            Bootstrap.ResolvePathsToTestAgainst(!ContextForTests.IsLocal ? Path.Combine(ContextForTests.DeploymentDirectory) : null);
            Playback.Initialize();

        }

        [When(@"I debug ""(.*)"" in ""(.*)""")]
        public void WhenIDebugIn(string workflowName, string folderName)
        {
            ExplorerUIMap.DoubleClickWorkflow(workflowName, "TRAV");
            RibbonUIMap.ClickDebug();
            DebugUIMap.ClickExecute();
            OutputUIMap.WaitForExecution(100);
        }

        [Then(@"debug contains")]
        public void ThenDebugContains(Table table)
        {
            UITestControl lastStep = OutputUIMap.GetLastStep();
            string workflowStepName = OutputUIMap.GetStepName(lastStep);
            Assert.AreEqual("TravsTestFlow", workflowStepName);
            var tableRow = table.Rows[0];
            var labelValue = tableRow["Label"];
            var debugValues = tableRow["Value"].Split(' ');
            List<string> outputs = new[] { labelValue }.ToList();
            outputs.AddRange(debugValues);
            Assert.IsTrue(OutputUIMap.AssertDebugOutputContains(lastStep, outputs.ToArray()));

        }


        [When(@"I click new ""(.*)""")]
        public void WhenIClickNew(string type)
        {
            switch(type)
            {
                case "Workflow":
                    RibbonUIMap.CreateNewWorkflow();
                    break;
                case "Database Service":
                    RibbonUIMap.ClickNewDbWebService();
                    break;
            }
        }

        [Given(@"I click new ""(.*)""")]
        public void GivenIClickNew(string p0)
        {
            WhenIClickNew(p0);
        }

        [When(@"I drag on a ""(.*)""")]
        public void WhenIDragOnA(string p0)
        {
            var dsfActivityUiMap = new DsfMultiAssignUiMap(false, false) { TheTab = TabManagerUIMap.GetActiveTab() };
            dsfActivityUiMap.DragToolOntoDesigner();
            ScenarioContext.Current.Add(p0, dsfActivityUiMap);
        }

        [When(@"I enter into the ""(.*)""")]
        public void WhenIEnterIntoThe(string p0, Table table)
        {
            var dsfActivityUiMap = ScenarioContext.Current.Get<DsfMultiAssignUiMap>(p0);
            int index = 0;
            foreach(var tableRow in table.Rows)
            {
                dsfActivityUiMap.EnterTextIntoVariable(index, tableRow["Variable"]);
                dsfActivityUiMap.EnterTextIntoValue(index, tableRow["Value"]);
                index++;
            }
        }

        [Then(@"""(.*)"" contains")]
        public void ThenContains(string p0, Table table)
        {
            var dsfActivityUiMap = ScenarioContext.Current.Get<DsfMultiAssignUiMap>(p0);
            int index = 0;

            foreach(var tableRow in table.Rows)
            {
                Assert.AreEqual(tableRow["Variable"], dsfActivityUiMap.GetTextFromVariable(index));
                Assert.AreEqual(tableRow["Value"], dsfActivityUiMap.GetTextFromValue(index));
                index++;
            }
        }

        [When(@"the new Database Service wizard is visible")]
        public void WhenTheNewDatabaseServiceWizardIsVisible()
        {
            if(!WizardsUIMap.TryWaitForWizard(2500))
            {
                Assert.Fail("Database Service wizard not visible");
            }
            else
            {
                Assert.AreEqual("localhost (http://localhost:3142/dsf)", WizardsUIMap.GetRightTitleText());
            }
        }

        [When(@"I create a new Database Source ""(.*)""")]
        public void WhenICreateANewDatabaseSource(string sourceName)
        {
            DatabaseServiceWizardUIMap.ClickNewDbSource();
            DatabaseServiceWizardUIMap.CreateDbSource("RSAKLFSVRGENDEV", sourceName);
        }

        [When(@"I create a database service ""(.*)""")]
        public void WhenICreateADatabaseService(string serviceName)
        {
            DatabaseServiceWizardUIMap.CreateDbService(serviceName);
        }

        [Then(@"""(.*)"" is in the explorer")]
        public void ThenIsInTheExplorer(string resourceName)
        {
            Assert.IsTrue(ExplorerUIMap.ValidateHasResource(resourceName));
            ExplorerUIMap.RightClickDeleteResource(resourceName, "UNASSIGNED", "localhost");
            Bootstrap.DeleteService(resourceName);
        }



        [Then(@"a new tab is created")]
        public void ThenANewTabIsCreated()
        {
            var uiTestControl = TabManagerUIMap.GetActiveTab();
            Assert.IsNotNull(uiTestControl);
        }

        [Then(@"the tab name contains ""(.*)""")]
        public void ThenTheTabNameContains(string p0)
        {
            var activeTabName = TabManagerUIMap.GetActiveTabName();
            StringAssert.Contains(activeTabName, p0);
        }

        [Then(@"start node is visible")]
        public void ThenStartNodeIsVisible()
        {
            var uiTestControl = TabManagerUIMap.GetActiveTab();
            var findStartNode = WorkflowDesignerUIMap.FindStartNode(uiTestControl);
            Assert.IsNotNull(findStartNode);
        }

        [When(@"I send ""(.*)"" to ""(.*)""")]
        [Given(@"I send ""(.*)"" to ""(.*)""")]
        public void WhenISendTo(string textToSend, string automationIds)
        {
            var correctedAutoIds = GetCorrect(automationIds).Split(',');
            var controlToSendData = VisualTreeWalker.GetControlFromRoot(true, 0, correctedAutoIds);

            if(!string.IsNullOrEmpty(automationIds))
            {
                Mouse.Click(controlToSendData, new Point(5, 5));
                SendKeys.SendWait("{HOME}");
                SendKeys.SendWait("+{END}");
                SendKeys.SendWait("{DELETE}");
            }

            Playback.Wait(100);
            SendKeys.SendWait(textToSend);
        }

        [Given(@"I click ""(.*)""")]
        [When(@"I click ""(.*)""")]
        [Then(@"I click ""(.*)""")]
        public void GivenIClick(string automationIds)
        {
            var automationIDs = GetCorrect(automationIds).Split(',');
            var controlToClick = VisualTreeWalker.GetControlFromRoot(true, 0, automationIDs);

            Mouse.Click(controlToClick, new Point(5, 5));
            Playback.Wait(2000);
        }

        [When(@"I drag ""(.*)"" onto ""(.*)""")]
        public void WhenIDragOnto(string dragItemAutoIds, string dragDestinationAutoIds)
        {
            var correcteddDagItemAutoIds = GetCorrect(dragItemAutoIds).Split(',');
            var correctedDragDestinationAutoIds = GetCorrect(dragDestinationAutoIds).Split(',');

            var itemToDrag = VisualTreeWalker.GetControlFromRoot(true, 0, correcteddDagItemAutoIds);

            var dragDestinationItem = VisualTreeWalker.GetControlFromRoot(true, 0, correctedDragDestinationAutoIds);

            Mouse.Click(itemToDrag, new Point(15, 15));
            var clickablePoint = itemToDrag.GetClickablePoint();
            Mouse.StartDragging(itemToDrag, clickablePoint);

            var boundingRectangle = dragDestinationItem.BoundingRectangle;
            Mouse.StopDragging(boundingRectangle.X, boundingRectangle.Y - 400);
            //Playback.PlaybackSettings.WaitForReadyLevel = WaitForReadyLevel.AllThreads;
            Playback.Wait(100);
        }

        [When(@"I double click ""(.*)""")]
        [Given(@"I double click ""(.*)""")]
        public void WhenIDoubleClick(string itemToDoubleClickAutoIds)
        {
            var correcteddItemToDoubleClickAutoIds = GetCorrect(itemToDoubleClickAutoIds).Split(',');
            var itemToDoubleClick = VisualTreeWalker.GetControlFromRoot(true, 0, correcteddItemToDoubleClickAutoIds);
            var clickablePoint = itemToDoubleClick.GetClickablePoint();
            Mouse.DoubleClick(itemToDoubleClick, clickablePoint);

        }

        [Then(@"""(.*)"" is visible")]
        public void ThenIsVisible(string itemToFindAutoIds)
        {
            var correctedditemToFindAutoIds = GetCorrect(itemToFindAutoIds).Split(',');
            var itemFound = VisualTreeWalker.GetControlFromRoot(true, 0, correctedditemToFindAutoIds);

            Assert.IsNotNull(itemFound);

        }

        [Then(@"""(.*)"" is not visible")]
        public void ThenIsNotVisible(string itemToFindAutoIds)
        {
            var correctedditemToFindAutoIds = GetCorrect(itemToFindAutoIds).Split(',');
            var itemFound = VisualTreeWalker.GetControlFromRoot(true, 0, correctedditemToFindAutoIds);

            Assert.IsNull(itemFound);
        }

        [Given(@"I wait (.*)")]
        public void GivenIWait(int waitTime)
        {
            Playback.Wait(waitTime);
        }

        [Given(@"I wait")]
        public void GivenIWait()
        {
            Playback.PlaybackSettings.WaitForReadyLevel = WaitForReadyLevel.AllThreads;
            Playback.Wait(500);
            Playback.PlaybackSettings.WaitForReadyLevel = WaitForReadyLevel.UIThreadOnly;
        }

        string GetCorrect(string automationIds)
        {
            var fieldInfos = typeof(WorkflowSteps).GetFields(BindingFlags.NonPublic | BindingFlags.Static);
            var consts = fieldInfos.Where(fi => fi.IsLiteral).ToList();
            var replace = automationIds;
            foreach(var fieldInfo in consts)
            {
                var rawConstantValue = fieldInfo.GetRawConstantValue() as string;
                if(rawConstantValue != null)
                {
                    replace = replace.Replace(fieldInfo.Name.ToUpper(), rawConstantValue);
                }
            }
            return replace;
        }

        [AfterScenario]
        public void TestCleanUp()
        {
            TabManagerUIMap.CloseAllTabs();
            Bootstrap.Teardown();
            Playback.Cleanup();
        }
    }
}
