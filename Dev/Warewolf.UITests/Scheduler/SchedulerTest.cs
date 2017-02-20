using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UITests.DialogsUIMapClasses;
using Warewolf.UITests.Scheduler.SchedulerUIMapClasses;

namespace Warewolf.UITests.Scheduler
{
    [CodedUITest]
    public class SchedulerTest
    {
        [TestMethod]
        [TestCategory("Scheduler")]
        public void Create_SchedulerTask_From_SidebarRibbonButton_UITests()
        {
            Assert.IsTrue(SchedulerUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SchedulerTab.Exists, "SchedulerNewTask Tab does not exist.");
            //Assert NewScheduleTask Controls
            SchedulerUIMap.Create_Scheduler_Using_Shortcut();
            Assert.IsTrue(SchedulerUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SchedulerTab.WorkSurfaceContext.SchedulerView.EditTriggerButton.Exists, "EditTrigger Button does not exist.");
            Assert.IsTrue(SchedulerUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SchedulerTab.WorkSurfaceContext.SchedulerView.NameTextbox.Exists, "Name Textbox does not exist.");
            Assert.IsTrue(SchedulerUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SchedulerTab.WorkSurfaceContext.SchedulerView.EnabledRadioButton.Exists, "Enabled RadioButton does not exist.");
            Assert.IsTrue(SchedulerUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SchedulerTab.WorkSurfaceContext.SchedulerView.DisabledRadioButton.Exists, "Disabled RadioButton does not exist.");
            Assert.IsTrue(SchedulerUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SchedulerTab.WorkSurfaceContext.SchedulerView.WorkflowNameTextBox.Exists, "Workflow Textbox does not exist.");
            Assert.IsTrue(SchedulerUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SchedulerTab.WorkSurfaceContext.SchedulerView.ResourcePickerButton.Exists, "Resource Picker Button does not exist.");
            Assert.IsTrue(SchedulerUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SchedulerTab.WorkSurfaceContext.SchedulerView.RunTaskCheckBox.Exists, "RunTask Checkbox does not exist.");
            Assert.IsTrue(SchedulerUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SchedulerTab.WorkSurfaceContext.SchedulerView.NumOfHistoryTextBoxEdit.Exists, "NumOfHistory Textbox does not exist.");
            Assert.IsTrue(SchedulerUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SchedulerTab.WorkSurfaceContext.SchedulerView.UserNameTextBoxEdit.Exists, "Username Textbox does not exist.");
            Assert.IsTrue(SchedulerUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SchedulerTab.WorkSurfaceContext.SchedulerView.PasswordTextbox.Exists, "Password Textbox does not exist.");
            Assert.IsTrue(SchedulerUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SchedulerTab.WorkSurfaceContext.SchedulerView.HistoryTable.Exists, "History Table does not exist.");
            //Create Hello World Schedule Task
            SchedulerUIMap.Click_Scheduler_ResourcePickerButton();
            Assert.IsTrue(DialogsUIMap.ServicePickerDialog.Exists, "Service Picker Window does not exist.");
            DialogsUIMap.Filter_ServicePicker_Explorer("GenericResource");
            DialogsUIMap.Click_Service_Picker_Dialog_First_Service_In_Explorer();
            DialogsUIMap.Click_Service_Picker_Dialog_OK();
            Assert.AreEqual("GenericResource", SchedulerUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SchedulerTab.WorkSurfaceContext.SchedulerView.NameTextbox.Text);
            SchedulerUIMap.Enter_LocalSchedulerAdminCredentials_Into_SchedulerTab();
            UIMap.Click_Save_RibbonButton();
            Assert.IsTrue(SchedulerUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SchedulerTab.WorkSurfaceContext.SchedulerView.SchedulesList.GenericResourceListItem.Exists, "Generic Resource schedule did not save.");
            SchedulerUIMap.Click_HelloWorldSchedule_EnableOrDisableCheckbox();
            SchedulerUIMap.Click_HelloWorldSchedule_EraseSchedulerButton();
            DialogsUIMap.Click_MessageBox_Yes();
            Assert.IsFalse(SchedulerUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SchedulerTab.WorkSurfaceContext.SchedulerView.SchedulesList.GenericResourceListItem.Exists, "Generic Resource schedule did not delete.");
            SchedulerUIMap.Click_SchedulerTab_CloseButton();
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
            UIMap.Click_Scheduler_RibbonButton();
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

        #endregion
    }
}
