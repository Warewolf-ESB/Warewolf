using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UITests.DotNetPluginSource.DotNetPluginSourceUIMapClasses;
using Warewolf.UITests.ExplorerUIMapClasses;
using Warewolf.UITests.WorkflowTab.Tools.Resources.ResourcesToolsUIMapClasses;
using Warewolf.UITests.WorkflowTab.WorkflowTabUIMapClasses;



namespace Warewolf.UITests.WorkflowTab.Tools.Resources
{
    [CodedUITest]
    public class DotNet_DLL_UITests
    {
        [TestMethod]
		[TestCategory("Resource Tools")]
        public void DotNetDLLTool_Small_And_LargeView_Then_NewSource_UITest()
        {
            Assert.IsTrue(ResourcesToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.Exists, "DotNet DLL tool does not exist on the design surface after dragging in from the toolbox.");
            //Small View
            ResourcesToolsUIMap.DotNetDLLTool_ChangeView_With_DoubleClick();
            Assert.IsTrue(ResourcesToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.SmallView.Exists, "DotNet DLL tool small view does not exist after double clicking tool large view.");
            //Large View
            ResourcesToolsUIMap.DotNetDLLTool_ChangeView_With_DoubleClick();
            Assert.IsTrue(ResourcesToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.SourcesComboBox.Exists, "Sources Combobox does not exist on DotNet DLL tool large view after openning it by double clicking the small view.");
            Assert.IsTrue(ResourcesToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.EditSourceButton.Exists, "EditSource Button does not exist on DotNet DLL tool large view after openning it by double clicking the small view.");
            Assert.IsTrue(ResourcesToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.NewSourceButton.Exists, "NewSource Button does not exist on DotNet DLL tool large view after openning it by double clicking the small view.");
            Assert.IsTrue(ResourcesToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ClassNameComboBox.Exists, "ClassName Combobox does not exist on DotNet DLL tool large view after openning it by double clicking the small view.");
            Assert.IsTrue(ResourcesToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ClassNameRefreshButton.Exists, "ClassName Refresh Button does not exist on DotNet DLL tool large view after openning it by double clicking the small view.");
            Assert.IsTrue(ResourcesToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.OnErrorPane.Exists, "OnError Pane does not exist on DotNet DLL tool large view after openning it by double clicking the small view.");
            Assert.IsTrue(ResourcesToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.DoneButton.Exists, "Done button does not exist on DotNet DLL tool large view after openning it by double clicking the small view.");
            //New Source
            ResourcesToolsUIMap.Click_NewSourceButton_From_DotNetDLLPluginTool();
            Assert.IsTrue(DotNetPluginSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DotNetPluginSourceTab.WorkSurfaceContext.AssemblyComboBox.Enabled, "Assembly Combobox is not enabled");
            Assert.IsTrue(DotNetPluginSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DotNetPluginSourceTab.WorkSurfaceContext.AssemblyDirectoryButton.Enabled, "Assembly Combobox Button is not enabled");
            Assert.IsFalse(DotNetPluginSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DotNetPluginSourceTab.WorkSurfaceContext.ConfigFileComboBox.Enabled, "Config File Combobox is enabled");
            Assert.IsFalse(DotNetPluginSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DotNetPluginSourceTab.WorkSurfaceContext.ConfigFileDirectoryButton.Enabled, "Config File Combobox Button is enabled");
            Assert.IsFalse(DotNetPluginSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DotNetPluginSourceTab.WorkSurfaceContext.GACAssemblyComboBox.Enabled, "GAC Assembly Combobox is enabled");
            Assert.IsTrue(DotNetPluginSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DotNetPluginSourceTab.WorkSurfaceContext.GACAssemblyDirectoryButton.Enabled, "GAC Assembly Combobox Button is not enabled");
            const string newDll = @"C:\ProgramData\Warewolf\Resources\TestingDotnetDllCascading.dll";
            DotNetPluginSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DotNetPluginSourceTab.WorkSurfaceContext.AssemblyComboBox.TextEdit.Text = newDll;
            UIMap.Save_With_Ribbon_Button_And_Dialog("NewDotnetPluginSource");
            DotNetPluginSourceUIMap.Click_Close_DotNetPlugin_Source_Tab();
            ResourcesToolsUIMap.DotNetDLLTool_ChangeView_With_DoubleClick();
            ResourcesToolsUIMap.Select_New_Source_From_DotNetDLLTool();
            ResourcesToolsUIMap.Click_EditSourceButton_On_DotNetDLLTool();
            Assert.AreEqual(newDll, DotNetPluginSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DotNetPluginSourceTab.WorkSurfaceContext.AssemblyComboBox.TextEdit.Text, "Assembly is not equal to updated text.");
        }

        [TestMethod]
        [TestCategory("Resource Tools")]
        public void DotNetDLLTool_EditSource_UITest()
        {
            ResourcesToolsUIMap.Select_Source_From_DotNetDLLTool();
            Assert.IsTrue(ResourcesToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.EditSourceButton.Enabled, "Edit Source Button is not enabled after selecting source.");
            ResourcesToolsUIMap.Click_EditSourceButton_On_DotNetDLLTool();
            Assert.IsFalse(string.IsNullOrEmpty(DotNetPluginSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DotNetPluginSourceTab.WorkSurfaceContext.AssemblyComboBox.TextEdit.Text), "Assembly Combobox is not enabled");
            string newDll = @"C:\ProgramData\Warewolf\Resources\TestingDotnetDllCascading2.dll";
            DotNetPluginSourceUIMap.Change_Dll_And_Save(newDll);
            DotNetPluginSourceUIMap.Click_Close_DotNetPlugin_Source_Tab();
            ResourcesToolsUIMap.DotNetDLLTool_ChangeView_With_DoubleClick();
            ResourcesToolsUIMap.Click_EditSourceButton_On_DotNetDLLTool();
            Assert.AreEqual(newDll, DotNetPluginSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DotNetPluginSourceTab.WorkSurfaceContext.AssemblyComboBox.TextEdit.Text, "Assembly is not equal to updated text.");
        }


        [TestMethod]
        [TestCategory("Resource Tools")]
        public void Selecting_Source_Enables_ClassName_UITests()
        {
            ResourcesToolsUIMap.Select_First_Item_From_DotNet_DLL_Large_View_Source_Combobox();
            Assert.IsTrue(ResourcesToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ClassNameComboBox.Enabled);
        }

        [TestMethod]
        [TestCategory("Resource Tools")]
        public void Selecting_Classname_Enabled_Constructor_And_Actions_UITests()
        {
            ResourcesToolsUIMap.Select_First_Item_From_DotNet_DLL_Large_View_Source_Combobox();
            Assert.IsTrue(ResourcesToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ClassNameComboBox.Enabled);
            ResourcesToolsUIMap.Select_DotNet_Dll_Classname();
            Assert.IsTrue(ResourcesToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.CtorExpander.ActivitiesDesignButton.ConstructorsComboBox.Enabled);
            Assert.IsTrue(ResourcesToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ActionMethodListBoxList.ActivitiesDesignListItem.ActivitiesExpander.ActivitiesDesignButton.ActionsComboBox.Enabled);
        }

        [TestMethod]
        [TestCategory("Resource Tools")]
        public void Selecting_Classname_Loads_Class_Constructors_UITests()
        {
            ResourcesToolsUIMap.Select_First_Item_From_DotNet_DLL_Large_View_Source_Combobox();
            Assert.IsTrue(ResourcesToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ClassNameComboBox.Enabled);
            ResourcesToolsUIMap.Select_DotNet_Dll_Classname();
            Assert.IsTrue(ResourcesToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.CtorExpander.ActivitiesDesignButton.ConstructorsComboBox.Enabled);
            ResourcesToolsUIMap.Click_DotNet_DLL_Large_View_Constructor_Combobox();
            Assert.IsTrue(ResourcesToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.CtorExpander.ActivitiesDesignButton.ConstructorsComboBox.CtorListItem.Exists);
            Assert.IsTrue(ResourcesToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.CtorExpander.ActivitiesDesignButton.ConstructorsComboBox.CtorSystemStringListItem.Exists);
            Assert.IsTrue(ResourcesToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.CtorExpander.ActivitiesDesignButton.ConstructorsComboBox.CtorSystemStringSystListItem.Exists);
        }

        [TestMethod]
        [TestCategory("Resource Tools")]
        public void Selecting_Classname_Loads_Actions_UITests()
        {
            ResourcesToolsUIMap.Select_First_Item_From_DotNet_DLL_Large_View_Source_Combobox();
            Assert.IsTrue(ResourcesToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ClassNameComboBox.Enabled);
            ResourcesToolsUIMap.Select_DotNet_Dll_Classname();
            Assert.IsTrue(ResourcesToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.CtorExpander.ActivitiesDesignButton.ConstructorsComboBox.Enabled);
            ResourcesToolsUIMap.Click_DotNet_DLL_Large_View_FirstAction_Combobox();
            Assert.IsTrue(ResourcesToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ActionMethodListBoxList.ActivitiesDesignListItem.ActivitiesExpander.ActivitiesDesignButton.ActionsComboBox.Exists);
        }

        [TestMethod]
        [TestCategory("Resource Tools")]
        public void Selecting_Classname_AndCtor_With_Parameters_Shows_Inputs_Grid_UITests()
        {
            ResourcesToolsUIMap.Select_First_Item_From_DotNet_DLL_Large_View_Source_Combobox();
            Assert.IsTrue(ResourcesToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ClassNameComboBox.Enabled);
            ResourcesToolsUIMap.Select_DotNet_Dll_Classname();
            Assert.IsTrue(ResourcesToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.CtorExpander.ActivitiesDesignButton.ConstructorsComboBox.Enabled);
            ResourcesToolsUIMap.Click_DotNet_DLL_Large_View_Constructor_Combobox();
            Assert.IsTrue(ResourcesToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.CtorExpander.ActivitiesDesignButton.ConstructorsComboBox.CtorSystemStringListItem.Exists);
            ResourcesToolsUIMap.Select_DotNet_DLL_Large_View_Constructor_With_One_Parameter_From_Constructor_Combobox();
            ResourcesToolsUIMap.I_Expand_Costructor_Tree();
            Assert.IsTrue(ResourcesToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.LargeDataGridTable.Row1.Exists);
        }

        [TestMethod]
        [TestCategory("Resource Tools")]
        public void Selecting_Action_With_Paramerters_Shows_Inputs_Grid_UITests()
        {
            ResourcesToolsUIMap.Select_First_Item_From_DotNet_DLL_Large_View_Source_Combobox();
            Assert.IsTrue(ResourcesToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ClassNameComboBox.Enabled);
            ResourcesToolsUIMap.Select_DotNet_Dll_Classname();
            Assert.IsTrue(ResourcesToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.CtorExpander.ActivitiesDesignButton.ConstructorsComboBox.Enabled);
            ResourcesToolsUIMap.Click_DotNet_DLL_Large_View_FirstAction_Combobox();
            Assert.IsTrue(ResourcesToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ActionMethodListBoxList.ActivitiesDesignListItem.ActivitiesExpander.ActivitiesDesignButton.ActionsComboBox.Set_NameListItem.Exists);
            ResourcesToolsUIMap.Select_DotNet_DLL_Large_View_SetName_Action_For_FirstAction_From_Actions_Combobox();
            ResourcesToolsUIMap.I_Expand_First_Action_Tree();
            Assert.IsTrue(ResourcesToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ActionMethodListBoxList.ActivitiesDesignListItem.SetActivitiesExpander.ActionOneLargeDataGridTable.Exists);
        }

        [TestMethod]
        [TestCategory("Resource Tools")]
        public void Selecting_Action_Then_Clear_Removes_Empty_Actions()
        {
            ResourcesToolsUIMap.Select_First_Item_From_DotNet_DLL_Large_View_Source_Combobox();
            Assert.IsTrue(ResourcesToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ClassNameComboBox.Enabled);
            ResourcesToolsUIMap.Select_DotNet_Dll_Classname();
            Assert.IsTrue(ResourcesToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.CtorExpander.ActivitiesDesignButton.ConstructorsComboBox.Enabled);
            ResourcesToolsUIMap.Click_DotNet_DLL_Large_View_FirstAction_Combobox();
            Assert.IsTrue(ResourcesToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ActionMethodListBoxList.ActivitiesDesignListItem.ActivitiesExpander.ActivitiesDesignButton.ActionsComboBox.Set_NameListItem.Exists);
            ResourcesToolsUIMap.Select_DotNet_DLL_Large_View_SetName_Action_For_FirstAction_From_Actions_Combobox();
            ResourcesToolsUIMap.Click_DotNet_DLL_Large_View_SecondAction_Combobox();
            ResourcesToolsUIMap.Select_DotNet_DLL_Large_View_SetName_Action_For_SecondAction_From_Actions_Combobox();
            ResourcesToolsUIMap.Click_Delete_Button_On_Second_Action_Combobox();
            Assert.IsFalse(UIMap.ControlExistsNow(ResourcesToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ActionMethodListBoxList.ActivitiesDesignListItem2.SetActivitiesExpander));
        }

        [TestMethod]
        [TestCategory("Resource Tools")]
        public void Selecting_Classname_And_Action_Creates_New_Blank_Action_Dropbox_UITests()
        {
            ResourcesToolsUIMap.Select_First_Item_From_DotNet_DLL_Large_View_Source_Combobox();
            Assert.IsTrue(ResourcesToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ClassNameComboBox.Enabled);
            ResourcesToolsUIMap.Select_DotNet_Dll_Classname();
            Assert.IsTrue(ResourcesToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.CtorExpander.ActivitiesDesignButton.ConstructorsComboBox.Enabled);
            ResourcesToolsUIMap.Click_DotNet_DLL_Large_View_Constructor_Combobox();
            Assert.IsTrue(ResourcesToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.CtorExpander.ActivitiesDesignButton.ConstructorsComboBox.CtorSystemStringListItem.Exists);
            ResourcesToolsUIMap.Select_DotNet_DLL_Large_View_Constructor_With_One_Parameter_From_Constructor_Combobox();
            Assert.IsTrue(ResourcesToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ActionMethodListBoxList.ActivitiesDesignListItem.ActivitiesExpander.ActivitiesDesignButton.ActionsComboBox.Enabled);
            ResourcesToolsUIMap.Select_DotNet_DLL_Large_View_SetName_Action_For_FirstAction_From_Actions_Combobox();
            Assert.IsTrue(ResourcesToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ActionMethodListBoxList.ActivitiesDesignListItem2.ActivitiesExpander.ActivitiesDesignButton.ActionsComboBox.Enabled);
        }
        
        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
            ExplorerUIMap.Create_New_Workflow_In_LocalHost_With_Shortcut();
            WorkflowTabUIMap.Drag_DotNetDLLConnector_Onto_DesignSurface();
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

        DotNetPluginSourceUIMap DotNetPluginSourceUIMap
        {
            get
            {
                if (_DotNetPluginSourceUIMap == null)
                {
                    _DotNetPluginSourceUIMap = new DotNetPluginSourceUIMap();
                }

                return _DotNetPluginSourceUIMap;
            }
        }

        private DotNetPluginSourceUIMap _DotNetPluginSourceUIMap;

        ResourcesToolsUIMap ResourcesToolsUIMap
        {
            get
            {
                if (_ResourcesToolsUIMap == null)
                {
                    _ResourcesToolsUIMap = new ResourcesToolsUIMap();
                }

                return _ResourcesToolsUIMap;
            }
        }

        private ResourcesToolsUIMap _ResourcesToolsUIMap;

        #endregion
    }
}
