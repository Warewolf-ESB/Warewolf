using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UITests.Tools.ToolsUIMapClasses;

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
            UIMap.Filter_Explorer("Hello World");
            UIMap.Duplicate_FirstResource_From_ExplorerContextMenu();
            ToolsUIMap.Enter_Duplicate_workflow_name("Duplicated_HelloWorld");
            UIMap.Click_Duplicate_From_Duplicate_Dialog();
        }
        
        [TestMethod]
        [TestCategory("Explorer")]
        public void DuplicateFolder_ThenAddsNewFolderItem()
        {
            UIMap.Filter_Explorer("Examples");
            UIMap.Duplicate_FirstResource_From_ExplorerContextMenu();
            ToolsUIMap.Enter_Duplicate_workflow_name("Duplicated_ExampleFolder");
            UIMap.Click_Duplicate_From_Duplicate_Dialog();
        }

        [TestMethod]
        [TestCategory("Explorer")]
        public void DuplicateFolder_And_Use_Same_Name_Shows_Error()
        {
            const string serviceName = "DuplicateFolderNameError";
            UIMap.Click_Duplicate_From_ExplorerContextMenu(serviceName);
            Assert.IsTrue(UIMap.SaveDialogWindow.ErrorLabel.Exists, "Sve Error dialog does not exist after clicking Duplicate");
            UIMap.Click_SaveDialog_CancelButton();
        }

        [TestMethod]
        [TestCategory("Explorer")]
        public void DuplicateWorkflow_Updates_The_Workflow_Display_Name()
        {
            UIMap.Click_Duplicate_From_ExplorerContextMenu("Hello World");
            const string newName = "HelloWorld2";
            ToolsUIMap.Enter_Duplicate_workflow_name(newName);
            UIMap.Click_Duplicate_From_Duplicate_Dialog();
            UIMap.Filter_Explorer(newName);
            Assert.AreEqual(newName, UIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.ItemEdit.Text, "First Item is not the same as Filtered input.");
            UIMap.DoubleClick_Explorer_Localhost_First_Item();
            Assert.AreEqual(ToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.BreadcrumbbarList.HelloWorld2ListItem.DisplayText, newName);
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

        ToolsUIMap ToolsUIMap
        {
            get
            {
                if (_ToolsUIMap == null)
                {
                    _ToolsUIMap = new ToolsUIMap();
                }

                return _ToolsUIMap;
            }
        }

        private ToolsUIMap _ToolsUIMap;

        #endregion
    }
}
