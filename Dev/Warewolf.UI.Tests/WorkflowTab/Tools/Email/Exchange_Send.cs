﻿using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UI.Tests.DialogsUIMapClasses;
using Warewolf.UI.Tests.ExchangeSource.ExchangeSourceUIMapClasses;
using Warewolf.UI.Tests.WorkflowTab.Tools.Email.EmailToolsUIMapClasses;
using Warewolf.UI.Tests.WorkflowTab.WorkflowTabUIMapClasses;

namespace Warewolf.UI.Tests.Tools
{
    [CodedUITest]
    public class Exchange_Send
    {
        const string SourceName = "ExchangeSourceFromTool";

        [TestMethod]
        [TestCategory("Email Tools")]
        public void ExchangeSendTool_Small_And_LargeView_Then_NewSource_UITest()
        {
            Assert.IsTrue(EmailToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ExchangeEmail.Exists, "Exchange Email tool does not exist after dragging in from the toolbox");
            // Small View
            Assert.IsTrue(EmailToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ExchangeEmail.SmallViewContent.SourcesComboBox.Enabled, "Source Combobox is not enabled.");
            Assert.IsFalse(EmailToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ExchangeEmail.SmallViewContent.ItemButton.Enabled, "Item Button  is not enabled.");
            Assert.IsTrue(EmailToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ExchangeEmail.SmallViewContent.NewSourceButton.Enabled, "New Source Button is not enabled.");
            Assert.IsTrue(EmailToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ExchangeEmail.SmallViewContent.ToComboBox.Enabled, "To Combobox is not enabled.");
            Assert.IsTrue(EmailToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ExchangeEmail.SmallViewContent.SubjectComboBox.Enabled, "Subject Combobox is not enabled.");
            Assert.IsTrue(EmailToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ExchangeEmail.SmallViewContent.BodyComboBox.Enabled, "Body Combobox is not enabled.");
            Assert.IsTrue(EmailToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ExchangeEmail.SmallViewContent.ResultComboBox.Enabled, "Result Combobox is not enabled.");
            // Large View
            EmailToolsUIMap.ExchangeSendTool_ChangeView_With_DoubleClick();
            Assert.IsTrue(EmailToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ExchangeEmail.LargeViewContent.SourcesComboBox.Enabled, "Source Combobox is not enabled.");
            Assert.IsFalse(EmailToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ExchangeEmail.LargeViewContent.ItemButton.Enabled, "Item Button is not enabled.");
            Assert.IsTrue(EmailToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ExchangeEmail.LargeViewContent.NewSourceButton.Enabled, "New Source Button is not enabled.");
            Assert.IsTrue(EmailToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ExchangeEmail.LargeViewContent.ToComboBox.Enabled, "To Combobox is not enabled.");
            Assert.IsTrue(EmailToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ExchangeEmail.LargeViewContent.CCComboBox.Enabled, "CC Combobox is not enabled.");
            Assert.IsTrue(EmailToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ExchangeEmail.LargeViewContent.BCCComboBox.Enabled, "BCC Combobox is not enabled.");
            Assert.IsTrue(EmailToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ExchangeEmail.LargeViewContent.SubjectComboBox.Enabled, "Subject Combobox is not enabled.");
            Assert.IsTrue(EmailToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ExchangeEmail.LargeViewContent.AttachmentsComboBox.Enabled, "Attachments Combobox is not enabled.");
            Assert.IsTrue(EmailToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ExchangeEmail.LargeViewContent.BodyComboBox.Enabled, "Body Combobox is not enabled.");
            Assert.IsTrue(EmailToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ExchangeEmail.LargeViewContent.TestButton.Enabled, "Test Button is not enabled.");
            Assert.IsTrue(EmailToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ExchangeEmail.LargeViewContent.ResultComboBox.Enabled, "Result Combobox is not enabled.");
            Assert.IsTrue(EmailToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ExchangeEmail.LargeViewContent.HtmlBodyCheckBox.Enabled, "HTML Body Combobox is not enabled.");
            // New Source
            EmailToolsUIMap.Click_NewSourceButton_From_ExchangeSendTool();
            Assert.IsTrue(ExchangeSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ExchangeSourceTab.Exists, "Exchange Source Tab does not exist.");
            Assert.IsTrue(ExchangeSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ExchangeSourceTab.SendTestModelsCustom.AutoDiscoverUrlTxtBox.Exists, "Host textbox does not exist after opening Email source tab");
            Assert.IsTrue(ExchangeSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ExchangeSourceTab.SendTestModelsCustom.UserNameTextBox.Exists, "Username textbox does not exist after opening Email source tab");
            Assert.IsTrue(ExchangeSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ExchangeSourceTab.SendTestModelsCustom.PasswordTextBox.Exists, "Password textbox does not exist after opening Email source tab");
            Assert.IsTrue(ExchangeSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ExchangeSourceTab.SendTestModelsCustom.ToTextBox.Exists, "Port textbox does not exist after opening Email source tab");
            ExchangeSourceUIMap.Enter_Text_Into_Exchange_Tab();
            ExchangeSourceUIMap.Click_ExchangeSource_TestConnection_Button();
            System.Drawing.Point point;
            Assert.IsFalse(ExchangeSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ExchangeSourceTab.SendTestModelsCustom.ToTextBox.ItemImage.TryGetClickablePoint(out point), "Connection test Passed");
        }

        [TestMethod]
        [TestCategory("Email Tools")]
        public void ExchangeSendTool_HtmlBody_Checkbox_Is_Selected_UITest()
        {
            Assert.IsTrue(EmailToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ExchangeEmail.Exists, "Exchange Email tool does not exist after dragging in from the toolbox");
            // Large View
            EmailToolsUIMap.ExchangeSendTool_ChangeView_With_DoubleClick();
            Assert.IsTrue(EmailToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ExchangeEmail.LargeViewContent.HtmlBodyCheckBox.Enabled, "HTML Body Combobox is not enabled.");
            EmailToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ExchangeEmail.LargeViewContent.HtmlBodyCheckBox.Checked = true;
            EmailToolsUIMap.ExchangeSendTool_ChangeView_With_DoubleClick();
            Assert.IsTrue(EmailToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ExchangeEmail.SmallViewContent.Exists);
            EmailToolsUIMap.ExchangeSendTool_ChangeView_With_DoubleClick();
            Assert.IsTrue(EmailToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ExchangeEmail.LargeViewContent.HtmlBodyCheckBox.Checked, "HTML Body Combobox is not checked after changing to small view.");
        }

        [TestMethod]
        [TestCategory("Email Tools")]
        public void ExchangeMultipleAttachments_LargeViewUITest()
        {
            string folderName = @"c:\$$AttachmentsForEmail";
            string filePath1 = @"C:\$$AttachmentsForEmail\attachment1.txt";
            string filePath2 = @"C:\$$AttachmentsForEmail\attachment2.txt";
            try
            {
                UIMap.CreateFolderForAttachments(folderName);
                UIMap.CreateAttachmentsForTest(filePath1);
                UIMap.CreateAttachmentsForTest(filePath2);
                EmailToolsUIMap.ExchangeSendTool_ChangeView_With_DoubleClick();
                EmailToolsUIMap.Click_SelectFilesButton_On_ExchangeEmailTool_LargeView();
                DialogsUIMap.Select_Attachments_From_SelectFilesWindow();
                Assert.IsFalse(string.IsNullOrEmpty(EmailToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ExchangeEmail.LargeViewContent.AttachmentsComboBox.TextEdit.Text), "File Directory Textbox is empty");
                UIMap.RemoveTestFiles(filePath1, filePath2, folderName);
            }
            catch
            {
                UIMap.RemoveTestFiles(filePath1, filePath2, folderName);
            }
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
            UIMap.InitializeABlankWorkflow();
            WorkflowTabUIMap.Drag_Toolbox_Exchange_Send_Onto_DesignSurface();
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

        ExchangeSourceUIMap ExchangeSourceUIMap
        {
            get
            {
                if (_ExchangeSourceUIMap == null)
                {
                    _ExchangeSourceUIMap = new ExchangeSourceUIMap();
                }

                return _ExchangeSourceUIMap;
            }
        }

        private ExchangeSourceUIMap _ExchangeSourceUIMap;

        EmailToolsUIMap EmailToolsUIMap
        {
            get
            {
                if (_EmailToolsUIMap == null)
                {
                    _EmailToolsUIMap = new EmailToolsUIMap();
                }

                return _EmailToolsUIMap;
            }
        }

        private EmailToolsUIMap _EmailToolsUIMap;

        #endregion
    }
}
