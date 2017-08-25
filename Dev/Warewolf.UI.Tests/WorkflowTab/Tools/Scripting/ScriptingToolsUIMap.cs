using TechTalk.SpecFlow;
using Warewolf.UI.Tests.DialogsUIMapClasses;
using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;
using MouseButtons = System.Windows.Forms.MouseButtons;
using System.Drawing;
using System.Windows.Input;
using Warewolf.UI.Tests.Explorer.ExplorerUIMapClasses;
using Warewolf.UI.Tests.DotNetPluginSource.DotNetPluginSourceUIMapClasses;
using Warewolf.UI.Tests.WorkflowTab.WorkflowTabUIMapClasses;
using Warewolf.UI.Tests.ComPluginSource.ComPluginSourceUIMapClasses;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UI.Tests.WorkflowTab.Tools.Scripting.ScriptingToolsUIMapClasses
{
    [Binding]
    public partial class ScriptingToolsUIMap
    {
        [When(@"I Open Javascript Large View")]
        public void Open_Javascript_LargeView()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Javascript, new Point(115, 14));
        }

        [When(@"I Open Python Large View")]
        public void Open_Python_LargeView()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Python, new Point(117, 9));
        }

        [When(@"I Click Python Attachment Button")]
        public void Click_Python_Attachment_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Python.LargeView.AttachFileButton);
            Assert.IsTrue(DialogsUIMap.SelectFilesWindow.Exists, "The Select Files Window is expected to be visible");
        }


        [When(@"I Select Python File")]
        public void Select_Python_File()
        {
            Mouse.DoubleClick(DialogsUIMap.SelectFilesWindow.DrivesDataTree.CTreeItem.AttachmentsForEmailFolder);
            DialogsUIMap.SelectFilesWindow.DrivesDataTree.CTreeItem.AttachmentsForEmailFolder.attachment1.PythonCheckBox.Checked = true;
            Assert.IsNotNull(DialogsUIMap.SelectFilesWindow.FileNameTextBox.Text, "Files Name is empty even after selecting a File..");
            Mouse.Click(DialogsUIMap.SelectFilesWindow.SelectButton);
        }


        [When(@"I Open Ruby Large View")]
        public void Open_Ruby_LargeView()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Ruby, new Point(116, 12));
        }

        [When(@"I Open CMD Line Tool Large View")]
        public void Open_CMDLineTool_LargeView()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ExecuteCommandLine, new Point(174, 10));
        }

        [Given(@"I RightClick ExecuteCommandLine OnDesignSurface")]
        [When(@"I RightClick ExecuteCommandLine OnDesignSurface")]
        [Then(@"I RightClick ExecuteCommandLine OnDesignSurface")]
        public void RightClick_ExecuteCommandLine_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ExecuteCommandLine, MouseButtons.Right, ModifierKeys.None, new Point(165, 13));
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

        ComPluginSourceUIMap ComPluginSourceUIMap
        {
            get
            {
                if (_ComPluginSourceUIMap == null)
                {
                    _ComPluginSourceUIMap = new ComPluginSourceUIMap();
                }

                return _ComPluginSourceUIMap;
            }
        }

        private ComPluginSourceUIMap _ComPluginSourceUIMap;

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
    }
}
