using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UITests.DialogsUIMapClasses;
using Warewolf.UITests.ExplorerUIMapClasses;
using Warewolf.UITests.WorkflowTab.WorkflowTabUIMapClasses;

// ReSharper disable InconsistentNaming

namespace Warewolf.UITests
{
    [CodedUITest]
    public class DuplicateExplorerResource
    {
        [TestMethod]
        [TestCategory("Explorer")]
        public void DuplicateResource_ThenAddsNewItemItem()
        {
            ExplorerUIMap.Filter_Explorer("Hello World");
            ExplorerUIMap.Duplicate_FirstResource_From_ExplorerContextMenu();
            WorkflowTabUIMap.Enter_Duplicate_workflow_name("Duplicated_HelloWorld");
            DialogsUIMap.Click_Duplicate_From_Duplicate_Dialog();
        }
        
        [TestMethod]
        [TestCategory("Explorer")]
        public void DuplicateFolder_ThenAddsNewFolderItem()
        {
            ExplorerUIMap.Filter_Explorer("Examples");
            ExplorerUIMap.Duplicate_FirstResource_From_ExplorerContextMenu();
            WorkflowTabUIMap.Enter_Duplicate_workflow_name("Duplicated_ExampleFolder");
            DialogsUIMap.Click_Duplicate_From_Duplicate_Dialog();
        }

        [TestMethod]
        [TestCategory("Explorer")]
        public void DuplicateFolder_And_Use_Same_Name_Shows_Error()
        {
            const string serviceName = "DuplicateFolderNameError";
            ExplorerUIMap.Click_Duplicate_From_ExplorerContextMenu(serviceName);
            Assert.IsTrue(DialogsUIMap.SaveDialogWindow.ErrorLabel.Exists, "Sve Error dialog does not exist after clicking Duplicate");
            DialogsUIMap.Click_SaveDialog_CancelButton();
        }

        [TestMethod]
        [TestCategory("Explorer")]
        public void DuplicateWorkflow_Updates_The_Workflow_Display_Name()
        {
            ExplorerUIMap.Click_Duplicate_From_ExplorerContextMenu("Hello World");
            const string newName = "HelloWorld2";
            WorkflowTabUIMap.Enter_Duplicate_workflow_name(newName);
            DialogsUIMap.Click_Duplicate_From_Duplicate_Dialog();
            ExplorerUIMap.Filter_Explorer(newName);
            Assert.AreEqual(newName, ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.ItemEdit.Text, "First Item is not the same as Filtered input.");
            ExplorerUIMap.DoubleClick_Explorer_Localhost_First_Item();
            Assert.AreEqual(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.BreadcrumbbarList.HelloWorld2ListItem.DisplayText, newName);
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
    }
}
