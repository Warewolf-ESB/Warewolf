﻿using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UI.Tests.DialogsUIMapClasses;
using Warewolf.UI.Tests.EmailSource.EmailSourceUIMapClasses;
using Warewolf.UI.Tests.WorkflowTab.Tools.Email.EmailToolsUIMapClasses;
using Warewolf.UI.Tests.WorkflowTab.WorkflowTabUIMapClasses;

namespace Warewolf.UI.Tests.WorkflowTab.Tools.Email
{
    [CodedUITest]
    public class SMTP_Send
    {
        const string SourceName = "EmailSourceFromTool";

        [TestMethod]
        [TestCategory("Email Tools")]
        public void SMTPSendTool_Small_And_LargeView_Then_NewSource_UITest()
        {
            Assert.IsTrue(EmailToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SMTPEmail.Exists, "SMTP Email tool does not exist after dragging in from the toolbox");
            // Small View
            Assert.IsTrue(EmailToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SMTPEmail.SmallViewContent.Sources.Enabled, "Source Combobox is not enabled.");
            Assert.IsFalse(EmailToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SMTPEmail.SmallViewContent.ItemButton.Enabled, "Item Button is not enabled.");
            Assert.IsTrue(EmailToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SMTPEmail.SmallViewContent.ToComboBox.Enabled, "To Combobox is not enabled.");
            Assert.IsTrue(EmailToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SMTPEmail.SmallViewContent.SubjectComboBox.Enabled, "Subject Combobox is not enabled.");
            Assert.IsTrue(EmailToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SMTPEmail.SmallViewContent.BodyComboBox.Enabled, "Body Combobox is not enabled.");
            Assert.IsTrue(EmailToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SMTPEmail.SmallViewContent.ResultComboBox.Enabled, "Result Combobox is not enabled.");
            // Large View
            EmailToolsUIMap.Open_SMTPSendTool_LargeView();
            Assert.IsTrue(EmailToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SMTPEmail.LargeViewContent.Exists, "Email Tool large view does not exist on the design surface");
            Assert.IsTrue(EmailToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SMTPEmail.LargeViewContent.Sources.Enabled, "Source Combobox is not enabled.");
            Assert.IsFalse(EmailToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SMTPEmail.LargeViewContent.Edit.Enabled, "EditSource Button is not enabled.");
            Assert.IsTrue(EmailToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SMTPEmail.LargeViewContent.FromAddressComboBox.Enabled, "From Address Combobox is not enabled.");
            Assert.IsTrue(EmailToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SMTPEmail.LargeViewContent.PasswordTextbox.Enabled, "Password Textbox is not enabled.");
            Assert.IsTrue(EmailToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SMTPEmail.LargeViewContent.TestButton.Enabled, "Test Button is not enabled.");
            Assert.IsTrue(EmailToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SMTPEmail.LargeViewContent.ToComboBox.Enabled, "To Combobox is not enabled.");
            Assert.IsTrue(EmailToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SMTPEmail.LargeViewContent.CCComboBox.Enabled, "CC Combobox is not enabled.");
            Assert.IsTrue(EmailToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SMTPEmail.LargeViewContent.BCCComboBox.Enabled, "BCC Combobox is not enabled.");
            Assert.IsTrue(EmailToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SMTPEmail.LargeViewContent.PriorityComboBox.Enabled, "Priority Combobox is not enabled.");
            Assert.IsTrue(EmailToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SMTPEmail.LargeViewContent.SubjectComboBox.Enabled, "Subject Combobox is not enabled.");
            Assert.IsTrue(EmailToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SMTPEmail.LargeViewContent.AttachmentsComboBox.Enabled, "Attachments Combobox is not enabled.");
            Assert.IsTrue(EmailToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SMTPEmail.LargeViewContent.SelectFilesButton.Enabled, "Select Files Button is not enabled.");
            Assert.IsTrue(EmailToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SMTPEmail.LargeViewContent.HTMLCheckBox.Enabled, "HTML Checkbox is not enabled.");
            Assert.IsTrue(EmailToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SMTPEmail.LargeViewContent.BodyComboBox.Enabled, "Body Combobox is not enabled.");
            Assert.IsTrue(EmailToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SMTPEmail.LargeViewContent.ResultComboBox.Enabled, "Result Combobox is not enabled.");
            // New Source
            EmailToolsUIMap.Click_NewSource_From_SMTPSendTool();
            Assert.IsTrue(EmailSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.EmailSourceTab.Exists, "Email Source Tab does not exist");
            Assert.IsTrue(EmailSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.EmailSourceTab.SendTestModelsCustom.HostTextBoxEdit.Exists, "Host textbox does not exist after opening Email source tab");
            Assert.IsTrue(EmailSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.EmailSourceTab.SendTestModelsCustom.UserNameTextBoxEdit.Exists, "Username textbox does not exist after opening Email source tab");
            Assert.IsTrue(EmailSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.EmailSourceTab.SendTestModelsCustom.PasswordTextBoxEdit.Exists, "Password textbox does not exist after opening Email source tab");
            Assert.IsTrue(EmailSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.EmailSourceTab.SendTestModelsCustom.PortTextBoxEdit.Exists, "Port textbox does not exist after opening Email source tab");
            Assert.IsTrue(EmailSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.EmailSourceTab.SendTestModelsCustom.TimeoutTextBoxEdit.Exists, "Timeout textbox does not exist after opening Email source tab");
            EmailSourceUIMap.Enter_Text_Into_EmailSource_Tab();
            EmailSourceUIMap.Click_EmailSource_TestConnection_Button();
            Assert.IsTrue(EmailSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.EmailSourceTab.SendTestModelsCustom.ToTextBoxEdit.ItemImage.Exists, "Connection test Failed");
            Assert.IsTrue(UIMap.MainStudioWindow.SideMenuBar.SaveButton.Enabled, "Save ribbon button is not enabled after successfully testing new source.");
            UIMap.Save_With_Ribbon_Button_And_Dialog(SourceName);
            EmailSourceUIMap.Click_Close_EmailSource_Tab();
            //Edit Source
            EmailToolsUIMap.Open_SMTPSendTool_LargeView();
            Assert.IsTrue(EmailToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SMTPEmail.LargeViewContent.Exists, "Email Tool large view does not exist on the design surface");
            EmailToolsUIMap.Select_Source_From_EmailTool();
            Assert.IsTrue(EmailToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SMTPEmail.LargeViewContent.Edit.Enabled, "Edit Source Button is not enabled after selecting source.");
            EmailToolsUIMap.Click_EditSourceButton_On_EmailTool();
            Assert.IsTrue(EmailSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.EmailSourceTab.Exists, "Email Source Tab does not exist");
            EmailSourceUIMap.Edit_Timeout_On_EmailSource();
            EmailSourceUIMap.Click_EmailSource_TestConnection_Button();
            UIMap.Click_Save_Ribbon_Button_With_No_Save_Dialog();
            EmailSourceUIMap.Click_Close_EmailSource_Tab();
            EmailToolsUIMap.Open_SMTPSendTool_LargeView();
            Assert.IsTrue(EmailToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SMTPEmail.LargeViewContent.Exists, "Email Tool large view does not exist on the design surface");
            EmailToolsUIMap.Click_EditSourceButton_On_EmailTool();
            Assert.AreEqual("2000", EmailSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.EmailSourceTab.SendTestModelsCustom.TimeoutTextBoxEdit.Text);
        }

        [TestMethod]
        [TestCategory("Email Tools")]
        public void ExchangeSendTool_HtmlBody_Checkbox_Is_Selected_UITest()
        {
            Assert.IsTrue(EmailToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SMTPEmail.Exists, "Smtp Email tool does not exist after dragging in from the toolbox");
            // Large View
            EmailToolsUIMap.Open_SMTPSendTool_LargeView();
            Assert.IsTrue(EmailToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SMTPEmail.LargeViewContent.Exists, "Email Tool large view does not exist on the design surface");
            Assert.IsTrue(EmailToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SMTPEmail.LargeViewContent.HtmlBodyCheckBox.Enabled, "HTML Body Combobox is not enabled.");
            EmailToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SMTPEmail.LargeViewContent.HtmlBodyCheckBox.Checked = true;
            EmailToolsUIMap.Open_SMTPSendTool_LargeView();
            Assert.IsTrue(EmailToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SMTPEmail.SmallViewContent.Exists);
            EmailToolsUIMap.Open_SMTPSendTool_LargeView();
            Assert.IsTrue(EmailToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SMTPEmail.LargeViewContent.Exists, "Email Tool large view does not exist on the design surface");
            Assert.IsTrue(EmailToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SMTPEmail.LargeViewContent.HtmlBodyCheckBox.Checked, "HTML Body Combobox is not checked after changing to small view.");
        }
        [TestMethod]
        [TestCategory("Email Tools")]
        public void SMTPMultipleAttachmentsUITest()
        {
            string folderName = @"c:\$$AttachmentsForEmail";
            string filePath1 = @"C:\$$AttachmentsForEmail\attachment1.txt";
            string filePath2 = @"C:\$$AttachmentsForEmail\attachment2.txt";
            try
            {
                UIMap.CreateFolderForAttachments(folderName);
                UIMap.CreateAttachmentsForTest(filePath1);
                UIMap.CreateAttachmentsForTest(filePath2);
                EmailToolsUIMap.Open_SMTPSendTool_LargeView();
                EmailToolsUIMap.Click_SelectFilesButton_On_SMTPEmailTool_LargeView();
                DialogsUIMap.Select_Attachments_From_SelectFilesWindow();
                Assert.IsFalse(string.IsNullOrEmpty(EmailToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SMTPEmail.LargeViewContent.AttachmentsComboBox.TextEdit.Text), "File Directory Textbox is empty");
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
            WorkflowTabUIMap.Drag_Toolbox_SMTP_Email_Onto_DesignSurface();
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

        EmailSourceUIMap EmailSourceUIMap
        {
            get
            {
                if (_EmailSourceUIMap == null)
                {
                    _EmailSourceUIMap = new EmailSourceUIMap();
                }

                return _EmailSourceUIMap;
            }
        }

        private EmailSourceUIMap _EmailSourceUIMap;

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
