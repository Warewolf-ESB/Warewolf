using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using Warewolf.UI.Tests.WorkflowTab.Tools.Scripting.ScriptingToolsUIMapClasses;
using Warewolf.UI.Tests.WorkflowTab.WorkflowTabUIMapClasses;

namespace Warewolf.UI.Tests.WorkflowTab.Tools.Scripting
{
    [CodedUITest]
    public class Python
    {
        [TestMethod]
		[TestCategory("Tools")]
        public void PytonScriptTool_Small_And_LargeView_UITest()
        {
            Assert.IsTrue(ScriptingToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Python.Exists, "Python tool on the design surface does not exist");
            //Small View
            Assert.IsTrue(ScriptingToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Python.SmallView.ScriptIntellisenseCombobox.Exists, "Python script textbox does not exist after dragging on tool from the toolbox.");
            Assert.IsTrue(ScriptingToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Python.SmallView.ResultIntellisenseCombobox.Exists, "Python result textbox does not exist after dragging on tool from the toolbox.");
            //Large View
            ScriptingToolsUIMap.Open_Python_LargeView();
            Assert.IsTrue(ScriptingToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Python.LargeView.AttachmentsIntellisenseCombobox.Exists, "Python Attachments textbox does not exist after openning large view with a double click.");
            Assert.IsTrue(ScriptingToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Python.LargeView.AttachFileButton.Exists, "Python Attach File Button does not exist after openning large view with a double click.");
            Assert.IsTrue(ScriptingToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Python.LargeView.EscapesequencesCheckBox.Exists, "Python escape sequences checkbox does not exist after openning large view with a double click.");
            Assert.IsTrue(ScriptingToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Python.LargeView.ScriptIntellisenseCombobox.Exists, "Python script textbox does not exist after openning large view with a double click.");
            Assert.IsTrue(ScriptingToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Python.LargeView.ResultIntellisenseCombobox.Exists, "Python result textbox does not exist after openning large view with a double click.");
            Assert.IsTrue(ScriptingToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Python.LargeView.OnErrorPane.Exists, "Python OnError pane does not exist after openning large view with a double click.");
            Assert.IsTrue(ScriptingToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Python.DoneButton.Exists, "Python Done button does not exist after openning large view with a double click.");
        }


        [TestMethod]
        [TestCategory("Tools")]
        public void PytonScriptTool_LargeView_SelectFile_UITest()
        {
            var fileName = @"C:\.Python\testPython.py";
            if (!Directory.Exists(Path.GetDirectoryName(fileName)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(fileName));
            }
            if (!File.Exists(fileName))
            {
                File.Create(fileName);
            }
            string[] ProblemDirs = { @"C:\$SysReset", @"C:\$Windows.~BT", @"C:\$Windows.~WS" };
            foreach (var dir in ProblemDirs)
            {
                if (Directory.Exists(dir))
                {
                    Directory.Delete(dir, true);
                }
            }

            try
            {
                Assert.IsTrue(ScriptingToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Python.Exists, "Python tool on the design surface does not exist");
                //Small View
                Assert.IsTrue(ScriptingToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Python.SmallView.ScriptIntellisenseCombobox.Exists, "Python script textbox does not exist after dragging on tool from the toolbox.");
                Assert.IsTrue(ScriptingToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Python.SmallView.ResultIntellisenseCombobox.Exists, "Python result textbox does not exist after dragging on tool from the toolbox.");
                //Large View
                ScriptingToolsUIMap.Open_Python_LargeView();
                ScriptingToolsUIMap.Click_Python_Attachment_Button();
                ScriptingToolsUIMap.Select_Python_File();
                Assert.IsNotNull(ScriptingToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Python.LargeView.AttachmentsIntellisenseCombobox.Textbox, "Python Include File is expecting to have a value");
            }
            finally
            {
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }
                if (Directory.Exists(Path.GetDirectoryName(fileName)))
                {
                    Directory.Delete(Path.GetDirectoryName(fileName));
                }
            }
        }


        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
            UIMap.InitializeABlankWorkflow();
            WorkflowTabUIMap.Drag_Toolbox_Python_Onto_DesignSurface();
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

        ScriptingToolsUIMap ScriptingToolsUIMap
        {
            get
            {
                if (_ScriptingToolsUIMap == null)
                {
                    _ScriptingToolsUIMap = new ScriptingToolsUIMap();
                }

                return _ScriptingToolsUIMap;
            }
        }

        private ScriptingToolsUIMap _ScriptingToolsUIMap;

        #endregion
    }
}
