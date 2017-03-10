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
using Warewolf.UITests.WorkflowTesting.WorkflowServiceTestingUIMapClasses;
using Warewolf.UITests.DialogsUIMapClasses;

namespace Warewolf.UITests.Scheduler.SchedulerUIMapClasses
{
    [Binding]
    public partial class SchedulerUIMap
    {
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

        WorkflowServiceTestingUIMap WorkflowServiceTestingUIMap
        {
            get
            {
                if (_WorkflowServiceTestingUIMap == null)
                {
                    _WorkflowServiceTestingUIMap = new WorkflowServiceTestingUIMap();
                }

                return _WorkflowServiceTestingUIMap;
            }
        }

        private WorkflowServiceTestingUIMap _WorkflowServiceTestingUIMap;

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

        public void Create_Scheduler_Using_Shortcut()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SchedulerTab.WorkSurfaceContext.SchedulerView.SchedulesList, new Point(151, 13));
            Keyboard.SendKeys(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SchedulerTab.WorkSurfaceContext.SchedulerView.SchedulesList, "N", ModifierKeys.Control);
        }

        [When(@"I Enter LocalSchedulerAdmin Credentials Into Scheduler Tab")]
        public void Enter_LocalSchedulerAdminCredentials_Into_SchedulerTab()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SchedulerTab.WorkSurfaceContext.SchedulerView.UserNameTextBoxEdit.Text = @"Warewolf Administrators\IntegrationTester";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SchedulerTab.WorkSurfaceContext.SchedulerView.PasswordTextbox.Text = "I73573r0";
        }

        [When(@"I Click Scheduler Create New Task Button")]
        public void Click_Scheduler_NewTaskButton()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SchedulerTab.WorkSurfaceContext.SchedulerView.SchedulesList.ScheduleNewTaskListItem.SchedulerNewTaskButton, new Point(151, 13));
        }

        public void Click_SchedulerTab_CloseButton()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SchedulerTab.CloseButton);
        }

        [When(@"I Click Hello World Erase Schedule Button")]
        public void Click_HelloWorldSchedule_EraseSchedulerButton()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SchedulerTab.WorkSurfaceContext.SchedulerView.SchedulesList.GenericResourceListItem.EraseScheduleButton, new Point(6, 16));
        }

        [When(@"I Click Scheduler Enable Disable Checkbox Button")]
        public void Click_HelloWorldSchedule_EnableOrDisableCheckbox()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SchedulerTab.WorkSurfaceContext.SchedulerView.SchedulesList.GenericResourceListItem.EnableOrDisableCheckBox);
        }

        [When(@"I Click Scheduler ResourcePicker Button")]
        public void Click_Scheduler_ResourcePickerButton()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SchedulerTab.WorkSurfaceContext.SchedulerView.ResourcePickerButton, new Point(14, 13));
        }
    }
}
