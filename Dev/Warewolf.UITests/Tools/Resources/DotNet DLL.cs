﻿using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.Tools.Resources
{
    [CodedUITest]
    public class DotNet_DLL
    {
        [TestMethod]
		[TestCategory("Resource Tools")]
        public void DotNetDLLTool_LargeViewUITest()
        {
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ClassNameComboBox.Exists, "Class name textbox does not exist on DotNet DLL tool large view after openning it by double clicking the small view.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ActionsComboBox.Exists, "Actions Combobox does not exist on DotNet DLL tool large view after openning it by double clicking the small view.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.InputsTable.Exists, "Inputs Table does not exist on DotNet DLL tool large view after openning it by double clicking the small view.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.DoneButton.Exists, "Done button does not exist on DotNet DLL tool large view after openning it by double clicking the small view.");
        }

        [TestMethod]
        [TestCategory("Resource Tools")]
        public void DotNetDLLTool_SmallViewUITest()
        {
            UIMap.Collapse_DotNet_DLL_Connector_Tool_Large_View_to_Small_View_With_Double_Click();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.SmallView.Exists, "DotNet DLL tool small view does not exist after double clicking tool large view.");
        }

        [TestMethod]
        [TestCategory("Resource Tools")]
        public void Click_DotNetDLLTool_LargeView_NewSourceButton_UITests()
        {
            UIMap.Click_DotNetPluginTool_LargeView_NewSourceButton();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DotNetPluginSourceTab.WorkSurfaceContext.AssemblyComboBox.Enabled, "Assembly Combobox is not enabled");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DotNetPluginSourceTab.WorkSurfaceContext.AssemblyDirectoryButton.Enabled, "Assembly Combobox Button is not enabled");
            Assert.IsFalse(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DotNetPluginSourceTab.WorkSurfaceContext.ConfigFileComboBox.Enabled, "Config File Combobox is enabled");
            Assert.IsFalse(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DotNetPluginSourceTab.WorkSurfaceContext.ConfigFileDirectoryButton.Enabled, "Config File Combobox Button is enabled");
            Assert.IsFalse(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DotNetPluginSourceTab.WorkSurfaceContext.GACAssemblyComboBox.Enabled, "GAC Assembly Combobox is enabled");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DotNetPluginSourceTab.WorkSurfaceContext.GACAssemblyDirectoryButton.Enabled, "GAC Assembly Combobox Button is not enabled");
        }


        [TestMethod]
        [TestCategory("Resource Tools")]
        public void Selecting_Classname_Loads_Actions_UITests()
        {
            UIMap.Select_First_Item_From_DotNet_DLL_Large_View_Source_Combobox();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ClassNameComboBox.Enabled);
            UIMap.Select_DotNet_Dll_Classname();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ConstructorsComboBoxComboBox.Enabled);
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ActionsComboBox.Enabled);
            UIMap.Click_DotNet_DLL_Large_View_Action_Combobox();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ActionsComboBox.Item1.Exists);
        }
        [TestMethod]
        [TestCategory("Resource Tools")]
        public void Selecting_Classname_Loads_Class_Constructors_UITests()
        {
            UIMap.Select_First_Item_From_DotNet_DLL_Large_View_Source_Combobox();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ClassNameComboBox.Enabled);
            UIMap.Select_DotNet_Dll_Classname();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ConstructorsComboBoxComboBox.Enabled);
            UIMap.Click_DotNet_DLL_Large_View_Constructor_Combobox();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ConstructorsComboBoxComboBox.CtorItem1.Exists);
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ConstructorsComboBoxComboBox.CtorItem2.Exists);
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ConstructorsComboBoxComboBox.CtorItem3.Exists);
        }

        [TestMethod]
        [TestCategory("Resource Tools")]
        public void Selecting_Classname_AndCtor_With_Parameters_Shows_Inputs_Grid_UITests()
        {
            UIMap.Select_First_Item_From_DotNet_DLL_Large_View_Source_Combobox();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ClassNameComboBox.Enabled);
            UIMap.Select_DotNet_Dll_Classname();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ConstructorsComboBoxComboBox.Enabled);
            UIMap.Click_DotNet_DLL_Large_View_Constructor_Combobox();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ConstructorsComboBoxComboBox.CtorItem2.Exists);
            UIMap.Select_DotNet_DLL_Large_View_Constructor_With_One_Parameter_From_Constructor_Combobox();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.LargeDataGridTable.Row1.Exists);
        }

        //[TestMethod]
        //[TestCategory("Resource Tools")]
        //public void Selecting_Classname_Loads_Actions_UITests()
        //{
        //    UIMap.Select_First_Item_From_DotNet_DLL_Large_View_Source_Combobox();
        //    Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ClassNameComboBox.Enabled);
        //    UIMap.Select_DotNet_Dll_Classname();
        //    Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ConstructorsComboBoxComboBox.Enabled);
        //    UIMap.Select_DotNet_Dll_Default_Costructor();
        //}

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
            UIMap.Click_New_Workflow_Ribbon_Button();
            UIMap.Drag_DotNet_DLL_Connector_Onto_DesignSurface();
        }

        UIMap UIMap
        {
            get
            {
                if ((_UIMap == null))
                {
                    _UIMap = new UIMap();
                }

                return _UIMap;
            }
        }

        private UIMap _UIMap;

        #endregion
    }
}
