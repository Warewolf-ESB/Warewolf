using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Keyboard = Microsoft.VisualStudio.TestTools.UITesting.Keyboard;
using System.Windows.Input;
using MouseButtons = System.Windows.Forms.MouseButtons;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using System;
using Microsoft.VisualStudio.TestTools.UITest.Extension;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UITests.WorkflowTab.WorkflowTabUIMapClasses;
using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;
using System.Drawing;
using System.IO;
using TechTalk.SpecFlow;
using Warewolf.UITests.Common;
using Warewolf.UITests.ExplorerUIMapClasses;
using Warewolf.UITests.DialogsUIMapClasses;

namespace Warewolf.UITests.WorkflowTesting.WorkflowServiceTestingUIMapClasses
{
    [Binding]
    public partial class WorkflowServiceTestingUIMap
    {
        [Given("I Drag Dice Roll Example Onto DesignSurface")]
        [When("I Drag Dice Roll Example Onto DesignSurface")]
        [Then("I Drag Dice Roll Example Onto DesignSurface")]
        public void Drag_Dice_Roll_Example_Onto_DesignSurface()
        {
            ExplorerUIMap.Filter_Explorer("Dice Roll");
            WorkflowTabUIMap.Drag_Explorer_Localhost_Second_Item_Onto_Workflow_Design_Surface();
        }

        [When(@"I Enter RunAsUser Username And Password")]
        public void Enter_RunAsUser_Username_And_Password()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.UsernameTextBoxEdit.Text = "testuser";
            Keyboard.SendKeys(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.UsernameTextBoxEdit, "{Tab}", ModifierKeys.None);
            Keyboard.SendKeys(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.PasswordTextBoxEdit, "a1cbgHEVu098QBN0jqs55wYP/bLfpGNMxw2YxtLIgKOALxPfITSBDjNERdIi/KEq", true);
        }

        public void Select_First_Test()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test1, new Point(80, 10));
        }

        [When(@"I Enter Text Into Workflow Tests OutPutTable Row1 Value Textbox As CodedUITest")]
        public void Enter_Text_Into_Workflow_Tests_OutPutTable_Row1_Value_Textbox_As_CodedUITest()
        {
            Keyboard.SendKeys(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestOutputsTable.Row1.Cell.IntellisenseComboBox.Textbox, "Helo User", ModifierKeys.None);
            Assert.AreEqual("Hello User", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestOutputsTable.Row1.Cell.IntellisenseComboBox.Textbox.Text, "Workflow tests output tabe row 1 value textbox text does not equal Helo User afte" +
                    "r typing that in.");
        }

        [When(@"I Enter Text Into Workflow Tests Row1 Value Textbox As CodedUITest")]
        public void Enter_Text_Into_Workflow_Tests_Row1_Value_Textbox_As_CodedUITest()
        {
            Keyboard.SendKeys(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestInputsTable.Row1.Cell.IntellisenseComboBox.Textbox, "User", ModifierKeys.None);
            Assert.AreEqual("User", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestInputsTable.Row1.Cell.IntellisenseComboBox.Textbox.Text, "Workflow tests row 1 value textbox text does not equal User after typing that in." +
                    "");
        }

        [When(@"I Select User From RunTestAs")]
        public void Select_User_From_RunTestAs()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.UserRadioButton.Selected = true;
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.UsernameTextBoxEdit.Exists, "Username textbox does not exist after clicking RunAsUser radio button");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.PasswordTextBoxEdit.Exists, "Password textbox does not exist after clicking RunAsUser radio button");
            Assert.IsTrue(UIMap.MainStudioWindow.SideMenuBar.SaveButton.Enabled, "Save Ribbon Menu buton is disabled after changing test");
        }

        [Given(@"I Click The Create ""(.*)""th test Button")]
        [When(@"I Click The Create ""(.*)""th test Button")]
        [Then(@"I Click The Create ""(.*)""th test Button")]
        public void GivenIClickTheCreateThTestButton(int testIntance)
        {
            Click_Create_New_Tests(true, testIntance);
        }
        
        [Given(@"I Execute Workflow Using DebugRun Button")]
        [When(@"I Execute Workflow Using DebugRun Button")]
        [Then(@"I Execute Workflow Using DebugRun Button")]
        public void ThenIExecuteWorkflowUsingDebugRunButton()
        {
            UIMap.MainStudioWindow.SideMenuBar.RunAndDebugButton.DrawHighlight();
            Mouse.Click(UIMap.MainStudioWindow.SideMenuBar.RunAndDebugButton);
            Mouse.Click(UIMap.MainStudioWindow.DebugInputDialog.DebugF6Button);
        }

        [Then(@"I click Run ""(.*)""th test expecting ""(.*)""")]
        [When(@"I click Run ""(.*)""th test expecting ""(.*)""")]
        [Given(@"I click Run ""(.*)""th test expecting ""(.*)""")]
        public void ThenIClickRunThTestExpecting(int testInstance, string status)
        {
            var statusEnum = GetStatus(status);
            Click_Run_Test_Button(statusEnum, testInstance);
        }

        [Then(@"The Test step in now ""(.*)""")]
        [When(@"The Test step in now ""(.*)""")]
        [Given(@"The Test step in now ""(.*)""")]
        public void ThenTheTestStepInNow(string status)
        {
            Assert.AreEqual(TestResultEnum.Invalid, GetStatus(status));
        }

        private TestResultEnum GetStatus(string status)
        {
            if (status == "Pending")
                return TestResultEnum.Pending;
            else if (status == "Invalid")
                return TestResultEnum.Invalid;
            else if (status == "Fail")
                return TestResultEnum.Fail;
            else
                return TestResultEnum.Pass;
        }

        [When(@"I Click Test Tab")]
        [Then(@"I Click Test Tab")]
        [Given(@"I Click Test Tab")]
        public void WhenIClickTestTab()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab);
        }

        [Given(@"That The First Test ""(.*)"" Unsaved Star")]
        [When(@"The First Test ""(.*)"" Unsaved Star")]
        [Then(@"The First Test ""(.*)"" Unsaved Star")]
        public void Assert_Workflow_Testing_Tab_First_Test_Has_Unsaved_Star(string HasHasNot)
        {
            Assert_Workflow_Testing_Tab_First_Test_Has_Unsaved_Star(HasHasNot == "Has");
        }

        [Then(@"Test Tab Is Open")]
        [Given(@"Test Tab Is Open")]
        [When(@"Test Tab Is Open")]
        public void Test_Tab_Is_Open()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.Exists, "Test tab does not exist.");
        }

        public void Click_Output_Step()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.UserControl_1Custom.ScrollViewerPane.ActivityBuilderCustom.WorkflowItemPresenteCustom.FlowchartCustom.MultiAssign, new Point(77, 8));
        }

        public void Click_Decision_Step()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.UserControl_1Custom.ScrollViewerPane.ActivityBuilderCustom.WorkflowItemPresenteCustom.FlowchartCustom.Decision.DisplayNameEdit.DrawHighlight();
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.UserControl_1Custom.ScrollViewerPane.ActivityBuilderCustom.WorkflowItemPresenteCustom.FlowchartCustom.Decision.DisplayNameEdit, new Point(77, 8));
        }

        [Then(@"I Click Run all tests button")]
        [When(@"I Click Run all tests button")]
        [Given(@"I Click Run all tests button")]
        public void ThenIClickRunAllTestsButton()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.RunAllButton);
        }

        [Then(@"Test tab is open")]
        [Given(@"Test tab is open")]
        [When(@"Test tab is open")]
        public void ThenTestTabIsOpen()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.Exists);
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.Exists);
        }

        [When(@"I Click AssigName From DesignSurface")]
        public void Click_AssigName_From_DesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.UserControl_1Custom.ScrollViewerPane.ActivityBuilderCustom.WorkflowItemPresenteCustom.FlowchartCustom.MultiAssign);
        }

        public void Click_MockRadioButton_On_Decision_TestStep()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.DecisionTreeItem.DecisionAssert.SmallDataGridTable.ColumnHeadersPrHeader.MockOrAssert.MockRadioButton, new Point(5, 5));
        }

        public void Click_MockRadioButton_On_Assign_TestStep()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.AssignToNameTreeItem.AssignAssert.SmallDataGridTable.ColumnHeadersPrHeader.MockOrAssert.MockRadioButton, new Point(5, 5));
        }
        public void Click_MockRadioButton_On_TestStep()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.SetOutputTreeItem.OutputMessageAssert.SmallDataGridTable.ColumnHeadersPrHeader.MockOrAssert.MockRadioButton, new Point(5, 5));
        }

        public void Click_MockRadioButton_On_AssignValue_TestStep()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.DecisionTreeItem.DecisionAssert.SmallDataGridTable.ColumnHeadersPrHeader.MockOrAssert.MockRadioButton, new Point(5, 5));
        }

        public void Try_Click_Create_New_Tests()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.CreateTest.CreateTestButton, new Point(158, 10));
        }

        public void Click_Delete_On_AssignValue_TestStep()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.OutputMessageStep.OutputStepHeader.Delete.DrawHighlight();
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.OutputMessageStep.OutputStepHeader.Delete.Enabled);
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.OutputMessageStep.OutputStepHeader.Delete);
        }

        public void Expand_DotnetDll_ByClickingCheckbox(bool isChecked)
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.DotnetDllTreeItem.ExpansionIndicatorCheckBox);
        }

        public void SetConstructorAssertValue(string value)
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.DotnetDllTreeItem.ConstructorExpander.StepOutputs_ctor_Table.ItemRow.Cell2.AssertValue_id1tyComboBox.TextEdit.Text = value;
        }

        public void SetConstructorVariable(string value)
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.DotnetDllTreeItem.ConstructorExpander.StepOutputs_ctor_Table.ItemRow.Cell.AssertValue_humanEdit.Text = value;
        }

        public void Click_TestViewDotNet_DLL_Constructor_DeleteButton()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.DotnetDllTreeItem.ConstructorExpander.UIWarewolfStudioViewMoButton.DeleteButton);
        }

        public void Click_TestViewDotNet_DLL_FavouriteFood_DeleteButton()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.DotnetDllTreeItem.FavouriteFoodsExpander.WarewolfStudioViewMoButton.DeleteButton);
        }

        public void Assert_Test_Has_Unsaved_Star(string test, bool HasStar)
        {
            if (test == "1st")
                Assert.AreEqual(HasStar, MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test1.TestNameDisplay.DisplayText.Contains("*"), "1st test title does not contain unsaved star.");
            if (test == "2nd")
                Assert.AreEqual(HasStar, MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test2.TestNameDisplay.DisplayText.Contains("*"), "2nd test title does not contain unsaved star.");
            if (test == "3rd")
                Assert.AreEqual(HasStar, MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test3.TestNameDisplay.DisplayText.Contains("*"), "3rd test title does not contain unsaved star.");
            if (test == "4th")
                Assert.AreEqual(HasStar, MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test4.TestNameDisplay.DisplayText.Contains("*"), "4th test title does not contain unsaved star.");
        }

        [Given(@"That The Second Test ""(.*)"" Unsaved Star")]
        [When(@"The Second Test ""(.*)"" Unsaved Star")]
        [Then(@"The Second Test ""(.*)"" Unsaved Star")]
        public void Assert_Workflow_Testing_Tab_Second_Test_Has_Unsaved_Star(string HasHasNot)
        {
            Assert.AreEqual((HasHasNot == "Has"), MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.TabDescription.DisplayText.Contains("*"), "Test tab title does not contain unsaved star.");
            Assert.AreEqual((HasHasNot == "Has"), MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test5.TestNameDisplay.DisplayText.Contains("*"), "Second test title does not contain unsaved star.");
        }

        [Given(@"That The Second Added Test ""(.*)"" Unsaved Star")]
        [When(@"The Second Added Test ""(.*)"" Unsaved Star")]
        [Then(@"The Second Added Test ""(.*)"" Unsaved Star")]
        public void Assert_Workflow_Testing_Tab_Second_Added_Test_Has_Unsaved_Star(string HasHasNot)
        {
            Assert.AreEqual((HasHasNot == "Has"), MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.TabDescription.DisplayText.Contains("*"), "Test tab title does not contain unsaved star.");
            Assert.AreEqual((HasHasNot == "Has"), MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test5.TestNameDisplay.DisplayText.Contains("*"), "Second Added test title does not contain unsaved star.");
        }

        [Given(@"I Click The Create a New Test Button")]
        [When(@"I Click The Create a New Test Button")]
        [Then(@"I Click The Create a New Test Button")]
        public void Click_Workflow_Testing_Tab_Create_New_Test_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.CreateTest.CreateTestButton, new Point(158, 10));
        }

        [Given("The First Test Exists")]
        [When("The First Test Exists")]
        [Then("The First Test Exists")]
        public void Assert_Workflow_Testing_Tab_First_Test_Exists()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test1.Exists, "No first test on workflow testing tab.");
        }

        [Given("The Added Test Exists")]
        [When("The Added Test Exists")]
        [Then("The Added Test Exists")]
        public void Assert_Workflow_Testing_Tab_Added_Test_Exists()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test4.Exists, "No 4th test on workflow testing tab.");
        }

        [Given(@"The ""(.*)"" Added Test Exists")]
        [When(@"The ""(.*)"" Added Test Exists")]
        [Then(@"The ""(.*)"" Added Test Exists")]
        public void ThenTheAddedTestExists(string test)
        {
            if (test == "1st")
                Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test1.Exists, "No 1st test on workflow testing tab.");
            if (test == "2nd")
                Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test2.Exists, "No 2nd test on workflow testing tab.");
            if (test == "3rd")
                Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test3.Exists, "No 3rd test on workflow testing tab.");
            if (test == "4th")
                Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test4.Exists, "No 4th test on workflow testing tab.");
        }

        [Given(@"The ""(.*)"" Added Test ""(.*)"" Unsaved Star")]
        [When(@"The ""(.*)"" Added Test ""(.*)"" Unsaved Star")]
        [Then(@"The ""(.*)"" Added Test ""(.*)"" Unsaved Star")]
        public void TheAddedTestUnsavedStar(string test, string star)
        {
            Assert_Workflow_Testing_Tab_Added_Test_Has_Unsaved_Star(star == "Has");
            Assert_Test_Has_Unsaved_Star(test, star == "Has");
        }


        [Given("The Second Test Exists")]
        [When("The Second Test Exists")]
        [Then("The Second Test Exists")]
        public void Assert_Workflow_Testing_Tab_Second_Test_Exists()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test2.Exists, "No 2nd test on workflow testing tab.");
        }

        [Given("The Second Added Test Exists")]
        [When("The Second Added Test Exists")]
        [Then("The Second Added Test Exists")]
        public void Assert_Workflow_Testing_Tab_Second_Added_Test_Exists()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test5.Exists, "No 5th Added test on workflow testing tab.");
        }

        [Given("I Toggle First Test Enabled")]
        [When("I Toggle First Test Enabled")]
        [Then("I Toggle First Test Enabled")]
        public void Toggle_Workflow_Testing_Tab_First_Test_Enabled()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test1.TestEnabledSelector, new Point(10, 10));
        }

        [Given("I Toggle First Added Test Enabled")]
        [Then("I Toggle First Added Test Enabled")]
        [When("I Toggle First Added Test Enabled")]
        public void Toggle_Workflow_Testing_Tab_First_Added_Test_Enabled()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test4.TestEnabledSelector, new Point(10, 10));
        }

        [Given(@"I Toggle ""(.*)"" Added Test Enabled")]
        [When(@"I Toggle ""(.*)"" Added Test Enabled")]
        [Then(@"I Toggle ""(.*)"" Added Test Enabled")]
        public void WhenIToggleAddedTestEnabled(string test)
        {
            if (test == "1st")
                Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test1.TestEnabledSelector, new Point(10, 10));
            if (test == "2nd")
                Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test2.TestEnabledSelector, new Point(10, 10));
            if (test == "3rd")
                Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test3.TestEnabledSelector, new Point(10, 10));
            if (test == "4th")
                Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test4.TestEnabledSelector, new Point(10, 10));
        }

        [Given("I Click Test (.*) Run Button")]
        [When("I Click Test (.*) Run Button")]
        [Then("I Click Test (.*) Run Button")]
        public void Click_Test_Run_Button(int index)
        {
            switch (index)
            {
                default:
                    Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test1.RunButton, new Point(10, 10));
                    break;
                case 2:
                    Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test2.RunButton, new Point(10, 10));
                    break;
                case 3:
                    Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test3.RunButton, new Point(10, 10));
                    break;
                case 4:
                    Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test4.RunButton, new Point(10, 10));
                    break;
            }
        }
        [Then(@"I delete Test(.*) as a Cleanup step")]
        public void ThenIDeleteTestAsACleanupStep(int p0)
        {
            Click_EnableDisable_This_Test_CheckBox(true, 4);
            Click_Delete_Test_Button(4);
            DialogsUIMap.Click_MessageBox_Yes();
            Click_Close_Tests_Tab();
        }

        [Given("I Click First Test Delete Button")]
        [When("I Click First Test Delete Button")]
        [Then("I Click First Test Delete Button")]
        public void Click_First_Test_Delete_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test1.DeleteButton, new Point(10, 10));
        }

        [Given(@"I Click First Test Run Button")]
        [When(@"I Click First Test Run Button")]
        [Then(@"I Click First Test Run Button")]
        public void Click_First_Test_Run_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test1.RunButton, new Point(10, 10));
        }

        [Given(@"I wait for output spinner")]
        [When(@"I wait for output spinner")]
        [Then(@"I wait for output spinner")]
        public void WhenIWaitForOutputSpinner()
        {
            UIMap.WaitForSpinner(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.StatusBar.Spinner);
        }

        [Given("I Click Run All Button")]
        [When("I Click Run All Button")]
        [Then("I Click Run All Button")]
        public void Click_Workflow_Testing_Tab_Run_All_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.RunAllButton, new Point(35, 10));
        }

        [Given(@"That The First Test ""(.*)"" Passing")]
        [When(@"The First Test ""(.*)"" Passing")]
        [Then(@"The First Test ""(.*)"" Passing")]
        public void Assert_Workflow_Testing_Tab_First_Test_Is_Passing(string IsIsNot)
        {
            Assert_Workflow_Testing_Tab_First_Test_Is_Passing(IsIsNot == "Is");
        }

        [Given(@"That The Second Test ""(.*)"" Passing")]
        [When(@"The Second Test ""(.*)"" Passing")]
        [Then(@"The Second Test ""(.*)"" Passing")]
        public void Assert_Workflow_Testing_Tab_Second_Test_Is_Passing(string IsIsNot)
        {
            Assert_Workflow_Testing_Tab_Second_Test_Is_Passing(IsIsNot == "Is");
        }

        [Given(@"That The Third Test ""(.*)"" Passing")]
        [When(@"The Third Test ""(.*)"" Passing")]
        [Then(@"The Third Test ""(.*)"" Passing")]
        public void Assert_Workflow_Testing_Tab_Third_Test_Is_Passing(string IsIsNot)
        {
            Assert_Workflow_Testing_Tab_Third_Test_Is_Passing(IsIsNot == "Is");
        }

        public void Assert_Workflow_Testing_Tab_First_Test_Is_Passing(bool passing = true)
        {
            Assert.AreEqual(passing, MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test1.Passing.Exists, (passing ? "First test is not passing." : "First test is passing."));
        }

        public void Assert_Workflow_Testing_Tab_Second_Test_Is_Passing(bool passing = true)
        {
            Assert.AreEqual(passing, MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test2.Passing.Exists, (passing ? "Second test is not passing." : "Second test is passing."));
        }

        public void Assert_Workflow_Testing_Tab_Third_Test_Is_Passing(bool passing = true)
        {
            Assert.AreEqual(passing, MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test3.Passing.Exists, (passing ? "Third test is not passing." : "Third test is passing."));
        }

        [Given(@"That The First Test ""(.*)"" Invalid")]
        [When(@"The First Test ""(.*)"" Invalid")]
        [Then(@"The First Test ""(.*)"" Invalid")]
        public void Assert_Workflow_Testing_Tab_First_Test_Is_Invalid(string IsIsNot)
        {
            Assert_Workflow_Testing_Tab_First_Test_Is_Invalid(IsIsNot == "Is");
        }

        public void Assert_Workflow_Testing_Tab_First_Test_Is_Invalid(bool invalid = true)
        {
            Assert.AreEqual(invalid, MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test1.Invalid.Exists, (invalid ? "First test is not invalid." : "First test is invalid."));
        }

        [Given(@"I delete Second Added Test")]
        [When(@"I delete Second Added Test")]
        [Then(@"I delete Second Added Test")]
        public void ThenIDeleteSecondAddedTest()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test5.TestEnabledSelector, new Point(10, 10));
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test5.DeleteButton, new Point(10, 10));
            DialogsUIMap.Click_MessageBox_Yes();
        }

        [Given(@"I delete Added Test")]
        [When(@"I delete Added Test")]
        [Then(@"I delete Added Test")]
        public void ThenIDeleteAddedTest()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test4.DeleteButton, new Point(10, 10));
            DialogsUIMap.Click_MessageBox_Yes();
        }

        [Given(@"That The Added Test ""(.*)"" Unsaved Star")]
        [When(@"That The Added Test ""(.*)"" Unsaved Star")]
        [Then(@"The Added ""(.*)"" Unsaved Star")]
        public void Assert_Workflow_Testing_Tab_Added_Test_Has_Unsaved_Star(string HasHasNot)
        {
            Assert_Workflow_Testing_Tab_Added_Test_Has_Unsaved_Star(HasHasNot == "Has");
        }

        [Given(@"The Added Test ""(.*)"" Unsaved Star")]
        [When(@"The Added Test ""(.*)"" Unsaved Star")]
        [Then(@"The Added Test ""(.*)"" Unsaved Star")]
        public void ThenTheAddedTestUnsavedStar(string p0)
        {
            Assert_Workflow_Testing_Tab_Added_Test_Has_Unsaved_Star(p0 == "Has");
        }

        public void Assert_Workflow_Testing_Tab_Added_Test_Has_Unsaved_Star(bool HasStar)
        {
            Assert.AreEqual(HasStar, MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.TabDescription.DisplayText.Contains("*"), "Test tab title does not contain unsaved star.");
        }

        public void Assert_Workflow_Testing_Tab_First_Test_Has_Unsaved_Star(bool HasStar)
        {
            Assert.AreEqual(HasStar, MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.TabDescription.DisplayText.Contains("*"), "Test tab title does not contain unsaved star.");
            Assert.AreEqual(HasStar, MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test4.TestNameDisplay.DisplayText.Contains("*"), "First test title does not contain unsaved star.");
        }

        public void TryRemoveTests()
        {
            WpfList testsListBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList;
            if (testsListBox.GetContent().Length >= 6)
            {
                Select_Test(3);
                Point point;
                if (MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test5.TestEnabledSelector.Checked && MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test5.TestEnabledSelector.TryGetClickablePoint(out point))
                    Click_EnableDisable_This_Test_CheckBox(true, 5);
                Click_Delete_Test_Button(5);
                DialogsUIMap.Click_MessageBox_Yes();
            }
            if (testsListBox.GetContent().Length >= 5)
            {
                Select_Test(3);
                Point point;
                if (MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test4.TestEnabledSelector.Checked && MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test4.TestEnabledSelector.TryGetClickablePoint(out point))
                    Click_EnableDisable_This_Test_CheckBox(true, 4);
                Click_Delete_Test_Button(4);
                DialogsUIMap.Click_MessageBox_Yes();
            }
            if (testsListBox.GetContent().Length >= 4)
            {
                Select_Test(3);
                Point point;
                if (MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test3.TestEnabledSelector.Checked && MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test3.TestEnabledSelector.TryGetClickablePoint(out point))
                    Click_EnableDisable_This_Test_CheckBox(true, 3);
                Click_Delete_Test_Button(3);
                DialogsUIMap.Click_MessageBox_Yes();
            }
            if (testsListBox.GetContent().Length >= 3)
            {
                Select_Test(2);
                Point point;
                if (MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test2.TestEnabledSelector.Checked && MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test2.TestEnabledSelector.TryGetClickablePoint(out point))
                    Click_EnableDisable_This_Test_CheckBox(true, 2);
                Click_Delete_Test_Button(2);
                DialogsUIMap.Click_MessageBox_Yes();
            }
            if (testsListBox.GetContent().Length >= 2)
            {
                Select_Test();
                Point point;
                if (MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test1.TestEnabledSelector.Checked && MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test1.TestEnabledSelector.TryGetClickablePoint(out point))
                    Click_EnableDisable_This_Test_CheckBox(true);
                Click_Delete_Test_Button();
                DialogsUIMap.Click_MessageBox_Yes();
            }
            Click_Close_Tests_Tab();
        }

        [Given(@"Dirty is ""(.*)"" When I Click EnableDisable test ""(.*)""")]
        [When(@"Dirty is ""(.*)"" When I Click EnableDisable test ""(.*)""")]
        [Then(@"Dirty is ""(.*)"" When I Click EnableDisable test ""(.*)""")]
        public void ThenDirtyIsWhenIClickEnableDisableTest(string dirty, int test)
        {
            if (dirty.ToUpper() == "NO")
                Click_EnableDisable_This_Test_CheckBox(testInstance: test);
            else
                Click_EnableDisable_This_Test_CheckBox(true, testInstance: test);
        }

        [Then(@"I Click EnableDisable Test (.*), dirty ""(.*)""")]
        public void ThenIClickEnableDisableTestDirty(int instance, string dirty)
        {
            Click_EnableDisable_This_Test_CheckBox(dirty == "true", instance);
        }

        public void Click_EnableDisable_This_Test_CheckBox(bool nameContainsStar = false, int testInstance = 1)
        {
            var currentTest = GetCurrentTest(testInstance);
            var testRunState = GetTestRunState(testInstance, currentTest);
            var selectedTestDeleteButton = GetSelectedTestDeleteButton(currentTest, testInstance);
            var beforeClick = testRunState.Checked;

            Mouse.Click(testRunState);
            Assert_Display_Text_ContainStar(Tab, nameContainsStar, testInstance);
            Assert_Display_Text_ContainStar(Test, nameContainsStar, testInstance);
        }

        private void AssertTestResults(TestResultEnum expectedTestResultEnum, int instance, WpfListItem currentTest)
        {
            switch (expectedTestResultEnum)
            {
                case TestResultEnum.Invalid:
                    TestResults.GetSelectedTestInvalidResult(currentTest, instance);
                    break;
                case TestResultEnum.Pending:
                    TestResults.GetSelectedTestPendingResult(currentTest, instance);
                    break;
                case TestResultEnum.Pass:
                    TestResults.GetSelectedTestPassingResult(currentTest, instance);
                    break;
                case TestResultEnum.Fail:
                    TestResults.GetSelectedTestFailingResult(currentTest, instance);
                    break;
            }
        }

        private WpfCheckBox GetTestRunState(int testInstance, WpfListItem test)
        {
            WpfCheckBox value;
            switch (testInstance)
            {
                case 2:
                    var test2 = test as Test2;
                    value = test2.TestEnabledSelector;
                    break;
                case 3:
                    var test3 = test as Test3;
                    value = test3.TestEnabledSelector;
                    break;
                case 4:
                    var test4 = test as Test4;
                    value = test4.TestEnabledSelector;
                    break;
                default:
                    var test1 = test as Test1;
                    value = test1.TestEnabledSelector;
                    break;
            }
            return value;
        }

        private WpfText GetTestNameDisplayText(int instance, WpfListItem test)
        {
            WpfText property;
            switch (instance)
            {
                case 2:
                    var test2 = test as Test2;
                    property = test2.TestNameDisplay;
                    break;
                case 3:
                    var test3 = test as Test3;
                    property = test3.TestNameDisplay;
                    break;
                case 4:
                    var test4 = test as Test4;
                    property = test4.TestNameDisplay;
                    break;
                default:
                    var test1 = test as Test1;
                    property = test1.TestNameDisplay;
                    break;
            }

            return property;
        }

        [Given(@"I Delete Test ""(.*)""")]
        [When(@"I Delete Test ""(.*)""")]
        [Then(@"I Delete Test ""(.*)""")]
        public void Click_Delete_Test_Button(int testInstance = 1)
        {
            var currentTest = GetCurrentTest(testInstance);
            var selectedTestDeleteButton = GetSelectedTestDeleteButton(currentTest, testInstance);
            Mouse.Click(selectedTestDeleteButton);
            Assert.IsTrue(DialogsUIMap.MessageBoxWindow.Exists, "Delete Confirmation MessageBox did not Open");
        }

        [Given(@"I Click Close Tests Tab")]
        [When(@"I Click Close Tests Tab")]
        [Then(@"I Click Close Tests Tab")]
        [Given(@"I Click Close Tests Tab")]
        public void Click_Close_Tests_Tab()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.CloseButton, new Point(11, 5));
        }

        public WpfListItem GetCurrentTest(int testInstance)
        {
            WpfListItem test;
            switch (testInstance)
            {
                case 2:
                    test = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test2;
                    break;
                case 3:
                    test = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test3;
                    break;
                case 4:
                    test = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test4;
                    break;
                default:
                    test = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test1;
                    break;
            }
            return test;
        }

        public WpfButton GetSelectedTestRunButton(WpfListItem test, int testInstance = 1)
        {
            WpfButton value;
            switch (testInstance)
            {
                case 2:
                    var test2 = test as Test2;
                    value = test2.RunButton;
                    break;
                case 3:
                    var test3 = test as Test3;
                    value = test3.RunButton;
                    break;
                case 4:
                    var test4 = test as Test4;
                    value = test4.RunButton;
                    break;
                default:
                    var test1 = test as Test1;
                    value = test1.RunButton;
                    break;
            }
            return value;
        }

        public WpfButton GetSelectedTestDeleteButton(WpfListItem test, int testInstance = 1)
        {
            WpfButton value;
            switch (testInstance)
            {
                case 2:
                    var test2 = test as Test2;
                    value = test2.DeleteButton;
                    break;
                case 3:
                    var test3 = test as Test3;
                    value = test3.DeleteButton;
                    break;
                case 4:
                    var test4 = test as Test4;
                    value = test4.DeleteButton;
                    break;
                default:
                    var test1 = test as Test1;
                    value = test1.DeleteButton;
                    break;
            }
            return value;
        }

        public void Assert_Display_Text_ContainStar(string control, bool containsStar, int instance = 1)
        {
            WpfList testsListBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList;
            var test = GetCurrentTest(instance);
            string description = string.Empty;
            if (control == "Tab")
            {
                description = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.TabDescription.DisplayText;
                if (containsStar)
                    Assert.IsTrue(description.Contains("*"), description + " DOES NOT contain a Star");
                else
                    Assert.IsFalse(description.Contains("*"), description + " contains a Star");
            }
            else if (control == "Test")
            {
                description = GetTestNameDisplayText(instance, test).DisplayText;
                if (containsStar)
                    Assert.IsTrue(description.Contains("*"), description + " DOES NOT contain a Star");
                else
                    Assert.IsFalse(description.Contains("*"), description + " contains a Star");
            }

            if (containsStar)
                Assert.IsTrue(description.Contains("*"), "Description does not contain *");
            else
                Assert.IsFalse(description.Contains("*"), "Description contains *");
            if (instance == 0)
            {
                var descriptions = testsListBox.GetContent();
                Assert.IsFalse(descriptions.Contains("*"), "Description contains *");
            }
        }

        const string Tab = "Tab";
        const string Test = "Test";
        public void Click_Create_New_Tests(bool nameContainsStar = false, int testInstance = 1)
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.CreateTest.CreateTestButton, new Point(158, 10));

            var currentTest = GetCurrentTest(testInstance);
            var testEnabledSelector = GetTestRunState(testInstance, currentTest).Checked;
            var testNeverRun = GetSelectedTestNeverRunDisplay(currentTest, testInstance);

            Assert.AreEqual("Never run", testNeverRun.DisplayText);
            AssertTestResults(TestResultEnum.Pending, testInstance, currentTest);
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestNameText.Exists, string.Format("Test{0} Name textbox does not exist after clicking Create New Test", testInstance));
            Assert.IsTrue(testEnabledSelector, string.Format("Test {0} is diabled after clicking Create new test from context menu", testInstance));

            Assert_Display_Text_ContainStar(Tab, nameContainsStar, testInstance);
            Assert_Display_Text_ContainStar(Test, nameContainsStar, testInstance);
        }

        public void Click_Duplicate_Test_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.DuplicateButton, new Point(14, 10));
        }

        public void Assert_Test_Result(string result)
        {
            WpfText passing = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test1.Passing;
            WpfText invalid = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test1.Invalid;
            WpfText failing = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test1.Failing;
            WpfText pending = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestsListboxList.Test1.Pending;
            if (result == "Passing")
                Assert.IsTrue(passing.Exists, "Test is not passing");
            if (result == "Failing")
                Assert.IsTrue(failing.Exists, "Test is not failing");
            if (result == "Invalid")
                Assert.IsTrue(invalid.Exists, "Test is not invalid");
            if (result == "Pending")
                Assert.IsTrue(pending.Exists, "Test is not pending");
        }

        public void ClickConstructorMockRadio(bool isChecked)
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.DotnetDllTreeItem.ConstructorExpander.StepOutputs_ctor_Table.ColumnHeadersPrHeader.ColumnHeader.UIMockRadioButton.Selected = isChecked;
        }

        public void ClickFavouriteMockRadio(bool isChecked)
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.StepTestDataTreeTree.DotnetDllTreeItem.FavouriteFoodsExpander.StepOutputs_FavouTable.ColumnHeadersPrHeader.ItemColumnHeader.MockRadioButton.Selected = isChecked;
        }

        [Given(@"I Update Test Name To ""(.*)""")]
        [When(@"I Update Test Name To ""(.*)""")]
        [Then(@"I Update Test Name To ""(.*)""")]
        public void Update_Test_Name(string overrideName = null)
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestNameTextbox, new Point(59, 16));
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestNameTextbox.Text = "";
            if (!string.IsNullOrEmpty(overrideName))
                MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestNameTextbox.Text = overrideName;
            else
                MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestNameTextbox.Text = "Dice_Test";
        }

        public void Save_Tets_With_Shortcut()
        {
            var testsTabPage = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestNameTextbox;
            Keyboard.SendKeys(testsTabPage, "S", (ModifierKeys.Control));
        }

        private static WpfText GetSelectedTestRunTimeDisplay(WpfListItem test, int instance)
        {
            WpfText testRunTimeDisplay;
            switch (instance)
            {
                case 2:
                    var test2 = test as Test2;
                    testRunTimeDisplay = test2.RunTimeDisplay;
                    break;
                case 3:
                    var test3 = test as Test3;
                    testRunTimeDisplay = test3.RunTimeDisplay;
                    break;
                case 4:
                    var test4 = test as Test4;
                    testRunTimeDisplay = test4.RunTimeDisplay;
                    break;
                default:
                    var test1 = test as Test1;
                    testRunTimeDisplay = test1.RunTimeDisplay;
                    break;
            }
            return testRunTimeDisplay;
        }

        private static WpfText GetSelectedTestNeverRunDisplay(WpfListItem test, int instance)
        {
            WpfText neverRunDisplay;
            switch (instance)
            {
                case 2:
                    var test2 = test as Test2;
                    neverRunDisplay = test2.NeverRunDisplay;
                    break;
                case 3:
                    var test3 = test as Test3;
                    neverRunDisplay = test3.NeverRunDisplay;
                    break;
                case 4:
                    var test4 = test as Test4;
                    neverRunDisplay = test4.NeverRunDisplay;
                    break;
                default:
                    var test1 = test as Test1;
                    neverRunDisplay = test1.NeverRunDisplay;
                    break;
            }
            return neverRunDisplay;
        }

        public void Click_Run_Test_Button(TestResultEnum? expectedTestResultEnum = null, int instance = 1)
        {
            var currentTest = GetCurrentTest(instance);
            Keyboard.SendKeys(MainStudioWindow, "{F5}", ModifierKeys.None);
            if (expectedTestResultEnum != null)
                AssertTestResults(expectedTestResultEnum.Value, instance, currentTest);
        }

        public void Enter_Text_Into_Workflow_Tests_Output_Row1_Value_Textbox_As_CodedUITest()
        {
            WpfEdit textbox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.TestOutputsTable.Row1.Cell.IntellisenseComboBox.Textbox;

            var helloUser = "Hello User.";
            Keyboard.SendKeys(textbox, helloUser, ModifierKeys.None);

            // Verify that the 'Text' property of 'Text' text box equals 'User'
            Assert.AreEqual(helloUser, textbox.Text, "Workflow tests output row 1 value textbox text does not equal 'Hello User' after typing that in.");
        }

        public void Select_Test(int instance = 1)
        {
            var currentTest = GetCurrentTest(instance);
            Mouse.Click(currentTest);
        }

        public void Click_RunAll_Button(string BrokenRule = null)
        {
            string DuplicateNameError = "DuplicateNameError";
            string UnsavedResourceError = "UnsavedResourceError";
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.RunAllButton, new Point(35, 10));
            Assert.AreEqual("Window", DialogsUIMap.MessageBoxWindow.ControlType.ToString(), "Messagebox does not exist after clicking RunAll button");

            if (!string.IsNullOrEmpty(BrokenRule))
            {
                if (BrokenRule.ToUpper().Equals(UnsavedResourceError))
                    Assert.AreEqual("Please save currently edited Test(s) before running the tests.", DialogsUIMap.MessageBoxWindow.UIPleasesavecurrentlyeText.DisplayText, "Message is not Equal to Please save currently edited Test(s) before running the t" +
                            "ests.");
                if (BrokenRule.ToUpper().Equals(DuplicateNameError))
                    Assert.AreEqual("Please save currently edited Test(s) before running the tests.", DialogsUIMap.MessageBoxWindow.UIPleasesavecurrentlyeText.DisplayText, "Messagebox does not show duplicated name error");
            }
        }

        [When(@"I Click Decision On Workflow Service Test View")]
        public void Click_Decision_On_Workflow_Service_Test_View()
        {
            Point point;
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.UserControl_1Custom.ScrollViewerPane.ActivityBuilderCustom.WorkflowItemPresenteCustom.FlowchartCustom.Decision.WaitForControlCondition((uitestcontrol) => { return uitestcontrol.TryGetClickablePoint(out point); }, 60000);
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.UserControl_1Custom.ScrollViewerPane.ActivityBuilderCustom.WorkflowItemPresenteCustom.FlowchartCustom.Decision);
        }

        #region UIMaps
        UIMap UIMap
        {
            get
            {
                if (_UIMap == null)
                {
                    _UIMap = new UIMap();
                }

                return _UIMap;
            }
        }

        private UIMap _UIMap;

        WorkflowTabUIMap WorkflowTabUIMap
        {
            get
            {
                if (_WorkflowTabUIMap == null)
                {
                    _WorkflowTabUIMap = new WorkflowTabUIMap();
                }

                return _WorkflowTabUIMap;
            }
        }

        private WorkflowTabUIMap _WorkflowTabUIMap;

        ExplorerUIMap ExplorerUIMap
        {
            get
            {
                if (_ExplorerUIMap == null)
                {
                    _ExplorerUIMap = new ExplorerUIMap();
                }

                return _ExplorerUIMap;
            }
        }

        private ExplorerUIMap _ExplorerUIMap;

        DialogsUIMap DialogsUIMap
        {
            get
            {
                if (_DialogsUIMap == null)
                {
                    _DialogsUIMap = new DialogsUIMap();
                }

                return _DialogsUIMap;
            }
        }

        private DialogsUIMap _DialogsUIMap;

        #endregion

        /// <summary>
        /// PinUnpinOutPutButton
        /// </summary>
        public void PinUnpinOutPutButton()
        {
            #region Variable Declarations
            WpfButton uIUnpinBtnButton = this.MainStudioWindow.SplitPane_AutoIDCustom.UIUI_TabManager_AutoIDTabList.UIDev2ViewModelsStudioTabPage.UIDev2StudioViewModelsCustom.UIContentDockManagerCustom.UIOUTPUTCustom.UIUnpinBtnButton;
            #endregion

            // Click 'unpinBtn' button
            Mouse.Click(uIUnpinBtnButton, new Point(12, 14));
        }

        public void Click_DecisionOn_Service_TestView()
        {
            Mouse.Click(MainStudioWindow.SplitPane_AutoIDCustom.UIUI_TabManager_AutoIDTabList.UIDev2ViewModelsStudioTabPage.UIDev2StudioViewModelsCustom.UIContentDockManagerCustom.UIUI_ServiceTestView_ACustom.UIUserControl_1Custom.UIScrollViewerPane.UIActivityBuilderCustom.UIWorkflowItemPresenteCustom.UIFlowchartCustom.UIFlowDecisionCustom);
        }
    }
}
