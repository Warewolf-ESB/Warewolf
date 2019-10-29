using System.Windows.Input;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Win32.TaskScheduler;
using Warewolf.UI.Tests.DialogsUIMapClasses;
using Warewolf.UI.Tests.Explorer.ExplorerUIMapClasses;
using Warewolf.UI.Tests.Scheduler.SchedulerUIMapClasses;
using Warewolf.UI.Tests.Settings.SettingsUIMapClasses;
using Warewolf.UI.Tests.WorkflowTab.WorkflowTabUIMapClasses;

namespace Warewolf.UI.Tests.Scheduler
{
    [CodedUITest]
    public class SchedulerTest
    {
        const string newassignwf = "NewAssignWf";
        const string taskFolderName = "Warewolf";

        [TestMethod]
        [DeploymentItem(@"lib\win32\x86\git2-6311e88.dll", @"lib\win32\x86")]
        [DeploymentItem(@"lib\win32\x64\git2-6311e88.dll", @"lib\win32\x64")]
        [TestCategory("Scheduler")]
        public void Create_SchedulerTask_From_SidebarRibbonButton_UITests()
        {
            UIMap.Click_Settings_RibbonButton();
            SettingsUIMap.Set_FirstResource_ResourcePermissions("GenericResource", "Public", true, true, true);
            UIMap.Click_Scheduler_RibbonButton(); 
            Assert.IsTrue(SchedulerUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SchedulerTab.Exists, "SchedulerNewTask Tab does not exist.");
            //Assert NewScheduleTask Controls
            SchedulerUIMap.Create_Scheduler_Using_Shortcut();
            Assert.IsTrue(SchedulerUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SchedulerTab.ContentDockManager.SchedulerView.EditTriggerButton.Exists, "EditTrigger Button does not exist.");
            Assert.IsTrue(SchedulerUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SchedulerTab.ContentDockManager.SchedulerView.NameTextbox.Exists, "Name Textbox does not exist.");
            Assert.IsTrue(SchedulerUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SchedulerTab.ContentDockManager.SchedulerView.EnabledRadioButton.Exists, "Enabled RadioButton does not exist.");
            Assert.IsTrue(SchedulerUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SchedulerTab.ContentDockManager.SchedulerView.DisabledRadioButton.Exists, "Disabled RadioButton does not exist.");
            Assert.IsTrue(SchedulerUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SchedulerTab.ContentDockManager.SchedulerView.WorkflowNameTextBox.Exists, "Workflow Textbox does not exist.");
            Assert.IsTrue(SchedulerUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SchedulerTab.ContentDockManager.SchedulerView.ResourcePickerButton.Exists, "Resource Picker Button does not exist.");
            Assert.IsTrue(SchedulerUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SchedulerTab.ContentDockManager.SchedulerView.RunTaskCheckBox.Exists, "RunTask Checkbox does not exist.");
            Assert.IsTrue(SchedulerUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SchedulerTab.ContentDockManager.SchedulerView.NumOfHistoryTextBoxEdit.Exists, "NumOfHistory Textbox does not exist.");
            Assert.IsTrue(SchedulerUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SchedulerTab.ContentDockManager.SchedulerView.UserNameTextBoxEdit.Exists, "Username Textbox does not exist.");
            Assert.IsTrue(SchedulerUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SchedulerTab.ContentDockManager.SchedulerView.PasswordTextbox.Exists, "Password Textbox does not exist.");
            Assert.IsTrue(SchedulerUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SchedulerTab.ContentDockManager.SchedulerView.HistoryTable.Exists, "History Table does not exist.");
            //Create GenericResource Schedule Task
            SchedulerUIMap.Click_Scheduler_ResourcePickerButton();
            Assert.IsTrue(DialogsUIMap.ServicePickerDialog.Exists, "Service Picker Window does not exist.");
            DialogsUIMap.Filter_ServicePicker_Explorer("GenericResource");
            DialogsUIMap.Click_Service_Picker_Dialog_First_Service_In_Explorer();
            DialogsUIMap.Click_Service_Picker_Dialog_OK();
            Assert.AreEqual("GenericResource", SchedulerUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SchedulerTab.ContentDockManager.SchedulerView.NameTextbox.Text);
            SchedulerUIMap.Enter_LocalSchedulerAdminCredentials_Into_SchedulerTab();
            UIMap.Click_Save_RibbonButton();
            Assert.IsTrue(SchedulerUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SchedulerTab.ContentDockManager.SchedulerView.SchedulesList.GenericResourceListItem.Exists, "Generic Resource schedule did not save.");
            SchedulerUIMap.Click_HelloWorldSchedule_EnableOrDisableCheckbox();
            SchedulerUIMap.Click_Schedule_EraseSchedulerButton();
            DialogsUIMap.Click_MessageBox_Yes();
            SchedulerUIMap.Click_SchedulerTab_CloseButton();
        }

        [TestMethod]
        [DeploymentItem(@"lib\win32\x86\git2-6311e88.dll", @"lib\win32\x86")]
        [DeploymentItem(@"lib\win32\x64\git2-6311e88.dll", @"lib\win32\x64")]
        [TestCategory("Scheduler Delete Is Disabled")]
        public void Delete_SchedulerTask_Button_Enables_When_Task_IsDisabled_UITests()
        {
            UIMap.Click_Scheduler_RibbonButton();
            Mouse.Click(SchedulerUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SchedulerTab.ContentDockManager.SchedulerView.SchedulesList.AllToolsResourceListItem.EnableorDisablethescCheckBox);
            Assert.IsTrue(SchedulerUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SchedulerTab.ContentDockManager.SchedulerView.SchedulesList.AllToolsResourceListItem.DeleteButton.Exists, "Delete Button does not exist on Scheduler Tab.");
            Mouse.Click(SchedulerUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SchedulerTab.ContentDockManager.SchedulerView.SchedulesList.AllToolsResourceListItem.EnableorDisablethescCheckBox);
            Assert.IsFalse(UIMap.ControlExistsNow(SchedulerUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SchedulerTab.ContentDockManager.SchedulerView.SchedulesList.AllToolsResourceListItem.DeleteButton));
        }

        [TestMethod]
        [DeploymentItem(@"lib\win32\x86\git2-6311e88.dll", @"lib\win32\x86")]
        [DeploymentItem(@"lib\win32\x64\git2-6311e88.dll", @"lib\win32\x64")]
        [TestCategory("Scheduler Delete Task")]
        public void Delete_SchedulerTask_Removes_Task_From_List_UITests()
        {
            UIMap.Click_Scheduler_RibbonButton();
            Assert.IsTrue(SchedulerUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SchedulerTab.ContentDockManager.SchedulerView.SchedulesList.AllToolsResourceListItem.Exists, "This test expects an existing scheduled task to exist using \"Generic Resource\".");
            Mouse.Click(SchedulerUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SchedulerTab.ContentDockManager.SchedulerView.SchedulesList.AllToolsResourceListItem.EnableorDisablethescCheckBox);
            Assert.IsTrue(SchedulerUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SchedulerTab.ContentDockManager.SchedulerView.SchedulesList.AllToolsResourceListItem.DeleteButton.Enabled, "Delete Button not enabled on Scheduler Tab after unchecking enabled checkbox.");
            Mouse.Click(SchedulerUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SchedulerTab.ContentDockManager.SchedulerView.SchedulesList.AllToolsResourceListItem.DeleteButton);
            DialogsUIMap.Click_MessageBox_Yes();
            Assert.IsFalse(UIMap.ControlExistsNow(SchedulerUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SchedulerTab.ContentDockManager.SchedulerView.SchedulesList.AllToolsResourceListItem), "A new item was not deleted correctly.");
        }

        [TestMethod]
        [DeploymentItem(@"lib\win32\x86\git2-6311e88.dll", @"lib\win32\x86")]
        [DeploymentItem(@"lib\win32\x64\git2-6311e88.dll", @"lib\win32\x64")]
        [TestCategory("Scheduler")]
        [DeploymentItem("Microsoft.Win32.TaskScheduler.dll")]
        public void Open_SchedulerTask_For_New_Workflow_Schedule_UITests()
        {
            var ts = new TaskService();
            var taskFolder = ts.GetFolder(taskFolderName);
            taskFolder.DeleteTask(newassignwf, false);
            ts.Dispose();

            UIMap.Click_Settings_RibbonButton();
            SettingsUIMap.Set_FirstResource_ResourcePermissions("NewAssignWf", "Public", true, true, true);

            ExplorerUImap.Filter_Explorer(newassignwf);
            ExplorerUImap.Open_Explorer_First_Item_With_Double_Click();
            Assert.IsTrue(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.StartNode.Exists);
            WorkflowTabUIMap.DisplayStartNodeContextMenu();
            WorkflowTabUIMap.Click_Scheduler_StartNode_Context_Item();
            Assert.IsTrue(SchedulerUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SchedulerTab.ContentDockManager.SchedulerView.SchedulesList.Exists, "Scheduled items list doe not exist");
            Assert.IsTrue(UIMap.MainStudioWindow.SideMenuBar.SaveButton.Enabled);
            Assert.IsTrue(SchedulerUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SchedulerTab.ContentDockManager.SchedulerView.SchedulesList.NewAssignWfSchedule.NewAssignWfText.DisplayText.Contains("*"));

            Mouse.Click(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab);
            WorkflowTabUIMap.DisplayStartNodeContextMenu();
            WorkflowTabUIMap.Click_Scheduler_StartNode_Context_Item();
            DialogsUIMap.Click_MessageBox_OK();
            Assert.IsTrue(SchedulerUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SchedulerTab.ContentDockManager.SchedulerView.SchedulesList.Exists, "Scheduled items list doe not exist");
            Assert.IsTrue(SchedulerUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SchedulerTab.ContentDockManager.SchedulerView.SchedulesList.NewAssignWfSchedule.Exists, "NewAssignWfSchedule was not created.");
            Assert.IsTrue(UIMap.MainStudioWindow.SideMenuBar.SaveButton.Enabled);
            Assert.IsTrue(SchedulerUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SchedulerTab.ContentDockManager.SchedulerView.SchedulesList.NewAssignWfSchedule.NewAssignWfText.DisplayText.Contains("*"));

            SchedulerUIMap.Enter_LocalSchedulerAdminCredentials_Into_SchedulerTab();
            Assert.IsFalse(UIMap.ControlExistsNow(DialogsUIMap.MessageBoxWindow));

            Mouse.Click(UIMap.MainStudioWindow.SideMenuBar.SaveButton);

            Mouse.Click(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab);
            WorkflowTabUIMap.DisplayStartNodeContextMenu();
            WorkflowTabUIMap.Click_Scheduler_StartNode_Context_Item();
            Assert.IsTrue(SchedulerUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SchedulerTab.ContentDockManager.SchedulerView.SchedulesList.Exists, "Scheduled items list doe not exist");
            Assert.IsTrue(SchedulerUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SchedulerTab.ContentDockManager.SchedulerView.SchedulesList.NewAssignWfSchedule.Exists, "NewAssignWfSchedule was not created.");
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
        }

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

        SettingsUIMap SettingsUIMap
        {
            get
            {
                if (_SettingsUIMap == null)
                {
                    _SettingsUIMap = new SettingsUIMap();
                }

                return _SettingsUIMap;
            }
        }

        private SettingsUIMap _SettingsUIMap;

        SchedulerUIMap SchedulerUIMap
        {
            get
            {
                if (_SchedulerUIMap == null)
                {
                    _SchedulerUIMap = new SchedulerUIMap();
                }

                return _SchedulerUIMap;
            }
        }

        private SchedulerUIMap _SchedulerUIMap;
        ExplorerUIMap ExplorerUImap
        {
            get
            {
                if (_explorerUImap == null)
                {
                    _explorerUImap = new ExplorerUIMap();
                }

                return _explorerUImap;
            }
        }

        private ExplorerUIMap _explorerUImap;
        WorkflowTabUIMap WorkflowTabUIMap
        {
            get
            {
                if (_workflowTabUIMap == null)
                {
                    _workflowTabUIMap = new WorkflowTabUIMap();
                }

                return _workflowTabUIMap;
            }
        }

        private WorkflowTabUIMap _workflowTabUIMap;

        #endregion
    }
}
