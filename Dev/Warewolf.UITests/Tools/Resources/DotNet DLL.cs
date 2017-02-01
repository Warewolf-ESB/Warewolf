﻿using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;



namespace Warewolf.UITests.Tools.Resources
{
    [CodedUITest]
    public class DotNet_DLL_UITests
    {
        [TestMethod]
		[TestCategory("Resource Tools")]
        public void DotNetDLLTool_LargeView_And_SmallView_UITest()
        {
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.SourcesComboBox.Exists, "Source combobox does not exist on DotNet DLL tool large view.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ClassNameComboBox.Exists, "Class name textbox does not exist on DotNet DLL tool large view.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.DoneButton.Exists, "Done button does not exist on DotNet DLL tool large view.");
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
            UIMap.Drag_DotNet_DLL_Connector_Onto_DesignSurface();
            UIMap.Click_NewSourceButton_From_DotNetDLLPluginTool();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DotNetPluginSourceTab.Exists, "DotNet Plugin Source Tab does not exsist.");
        }


        [TestMethod]
        [TestCategory("Resource Tools")]
        public void Selecting_Source_Enables_ClassName_UITests()
        {
            UIMap.Select_First_Item_From_DotNet_DLL_Large_View_Source_Combobox();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ClassNameComboBox.Enabled);
        }

        [TestMethod]
        [TestCategory("Resource Tools")]
        public void Selecting_Classname_Enabled_Constructor_And_Actions_UITests()
        {
            UIMap.Select_First_Item_From_DotNet_DLL_Large_View_Source_Combobox();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ClassNameComboBox.Enabled);
            UIMap.Select_DotNet_Dll_Classname();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.CtorExpander.ActivitiesDesignButton.ConstructorsComboBox.Enabled);
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ActionMethodListBoxList.ActivitiesDesignListItem.ActivitiesExpander.ActivitiesDesignButton.ActionsComboBox.Enabled);
        }

        [TestMethod]
        [TestCategory("Resource Tools")]
        public void Selecting_Classname_Loads_Class_Constructors_UITests()
        {
            UIMap.Select_First_Item_From_DotNet_DLL_Large_View_Source_Combobox();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ClassNameComboBox.Enabled);
            UIMap.Select_DotNet_Dll_Classname();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.CtorExpander.ActivitiesDesignButton.ConstructorsComboBox.Enabled);
            UIMap.Click_DotNet_DLL_Large_View_Constructor_Combobox();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.CtorExpander.ActivitiesDesignButton.ConstructorsComboBox.CtorListItem.Exists);
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.CtorExpander.ActivitiesDesignButton.ConstructorsComboBox.CtorSystemStringListItem.Exists);
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.CtorExpander.ActivitiesDesignButton.ConstructorsComboBox.CtorSystemStringSystListItem.Exists);
        }

        [TestMethod]
        [TestCategory("Resource Tools")]
        public void Selecting_Classname_Loads_Actions_UITests()
        {
            UIMap.Select_First_Item_From_DotNet_DLL_Large_View_Source_Combobox();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ClassNameComboBox.Enabled);
            UIMap.Select_DotNet_Dll_Classname();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.CtorExpander.ActivitiesDesignButton.ConstructorsComboBox.Enabled);
            UIMap.Click_DotNet_DLL_Large_View_FirstAction_Combobox();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ActionMethodListBoxList.ActivitiesDesignListItem.ActivitiesExpander.ActivitiesDesignButton.ActionsComboBox.Exists);
        }

        [TestMethod]
        [TestCategory("Resource Tools")]
        public void Selecting_Classname_AndCtor_With_Parameters_Shows_Inputs_Grid_UITests()
        {
            UIMap.Select_First_Item_From_DotNet_DLL_Large_View_Source_Combobox();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ClassNameComboBox.Enabled);
            UIMap.Select_DotNet_Dll_Classname();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.CtorExpander.ActivitiesDesignButton.ConstructorsComboBox.Enabled);
            UIMap.Click_DotNet_DLL_Large_View_Constructor_Combobox();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.CtorExpander.ActivitiesDesignButton.ConstructorsComboBox.CtorSystemStringListItem.Exists);
            UIMap.Select_DotNet_DLL_Large_View_Constructor_With_One_Parameter_From_Constructor_Combobox();
            UIMap.I_Expand_Costructor_Tree();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.LargeDataGridTable.Row1.Exists);
        }

        [TestMethod]
        [TestCategory("Resource Tools")]
        public void Selecting_Action_With_Paramerters_Shows_Inputs_Grid_UITests()
        {
            UIMap.Select_First_Item_From_DotNet_DLL_Large_View_Source_Combobox();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ClassNameComboBox.Enabled);
            UIMap.Select_DotNet_Dll_Classname();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.CtorExpander.ActivitiesDesignButton.ConstructorsComboBox.Enabled);
            UIMap.Click_DotNet_DLL_Large_View_FirstAction_Combobox();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ActionMethodListBoxList.ActivitiesDesignListItem.ActivitiesExpander.ActivitiesDesignButton.ActionsComboBox.Set_NameListItem.Exists);
            UIMap.Select_DotNet_DLL_Large_View_SetName_Action_For_FirstAction_From_Actions_Combobox();
            UIMap.I_Expand_First_Action_Tree();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ActionMethodListBoxList.ActivitiesDesignListItem.SetActivitiesExpander.ActionOneLargeDataGridTable.Exists);
        }

        [TestMethod]
        [TestCategory("Resource Tools")]
        public void Selecting_Action_Then_Clear_Removes_Empty_Actions()
        {
            UIMap.Select_First_Item_From_DotNet_DLL_Large_View_Source_Combobox();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ClassNameComboBox.Enabled);
            UIMap.Select_DotNet_Dll_Classname();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.CtorExpander.ActivitiesDesignButton.ConstructorsComboBox.Enabled);
            UIMap.Click_DotNet_DLL_Large_View_FirstAction_Combobox();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ActionMethodListBoxList.ActivitiesDesignListItem.ActivitiesExpander.ActivitiesDesignButton.ActionsComboBox.Set_NameListItem.Exists);
            UIMap.Select_DotNet_DLL_Large_View_SetName_Action_For_FirstAction_From_Actions_Combobox();
            UIMap.Click_DotNet_DLL_Large_View_SecondAction_Combobox();
            UIMap.Select_DotNet_DLL_Large_View_SetName_Action_For_SecondAction_From_Actions_Combobox();
            UIMap.Click_Delete_Button_On_Second_Action_Combobox();
            Assert.IsFalse(UIMap.ControlExistsNow(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ActionMethodListBoxList.ActivitiesDesignListItem2.SetActivitiesExpander));
        }

        [TestMethod]
        [TestCategory("Resource Tools")]
        public void Selecting_Classname_And_Action_Creates_New_Blank_Action_Dropbox_UITests()
        {
            UIMap.Select_First_Item_From_DotNet_DLL_Large_View_Source_Combobox();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ClassNameComboBox.Enabled);
            UIMap.Select_DotNet_Dll_Classname();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.CtorExpander.ActivitiesDesignButton.ConstructorsComboBox.Enabled);
            UIMap.Click_DotNet_DLL_Large_View_Constructor_Combobox();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.CtorExpander.ActivitiesDesignButton.ConstructorsComboBox.CtorSystemStringListItem.Exists);
            UIMap.Select_DotNet_DLL_Large_View_Constructor_With_One_Parameter_From_Constructor_Combobox();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ActionMethodListBoxList.ActivitiesDesignListItem.ActivitiesExpander.ActivitiesDesignButton.ActionsComboBox.Enabled);
            UIMap.Select_DotNet_DLL_Large_View_SetName_Action_For_FirstAction_From_Actions_Combobox();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ActionMethodListBoxList.ActivitiesDesignListItem2.ActivitiesExpander.ActivitiesDesignButton.ActionsComboBox.Enabled);
        }
        
        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
            UIMap.Create_New_Workflow_In_LocalHost_With_Shortcut();
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
