using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.Tools
{
    [CodedUITest]
    public class Exchange_Send
    {
        const string SourceName = "ExchangeSourceFromTool";

        [TestMethod]
        [TestCategory("Tools")]
        public void ExchangeSendTool_Small_And_LargeView_Then_NewSource_UITest()
        {

            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ExchangeEmail.Exists, "Exchange Email tool does not exist after dragging in from the toolbox");
            // Small View
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ExchangeEmail.SmallViewContent.SourcesComboBox.Enabled, "Source Combobox is not enabled.");
            Assert.IsFalse(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ExchangeEmail.SmallViewContent.ItemButton.Enabled, "Item Button  is not enabled.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ExchangeEmail.SmallViewContent.NewSourceButton.Enabled, "New Source Button is not enabled.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ExchangeEmail.SmallViewContent.ToComboBox.Enabled, "To Combobox is not enabled.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ExchangeEmail.SmallViewContent.SubjectComboBox.Enabled, "Subject Combobox is not enabled.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ExchangeEmail.SmallViewContent.BodyComboBox.Enabled, "Body Combobox is not enabled.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ExchangeEmail.SmallViewContent.ResultComboBox.Enabled, "Result Combobox is not enabled.");
            // Large View
            UIMap.ExchangeSendTool_ChangeView_With_DoubleClick();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ExchangeEmail.LargeViewContent.SourcesComboBox.Enabled, "Source Combobox is not enabled.");
            Assert.IsFalse(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ExchangeEmail.LargeViewContent.ItemButton.Enabled, "Item Button is not enabled.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ExchangeEmail.LargeViewContent.NewSourceButton.Enabled, "New Source Button is not enabled.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ExchangeEmail.LargeViewContent.ToComboBox.Enabled, "To Combobox is not enabled.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ExchangeEmail.LargeViewContent.CCComboBox.Enabled, "CC Combobox is not enabled.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ExchangeEmail.LargeViewContent.BCCComboBox.Enabled, "BCC Combobox is not enabled.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ExchangeEmail.LargeViewContent.SubjectComboBox.Enabled, "Subject Combobox is not enabled.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ExchangeEmail.LargeViewContent.AttachmentsComboBox.Enabled, "Attachments Combobox is not enabled.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ExchangeEmail.LargeViewContent.BodyComboBox.Enabled, "Body Combobox is not enabled.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ExchangeEmail.LargeViewContent.TestButton.Enabled, "Test Button is not enabled.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ExchangeEmail.LargeViewContent.ResultComboBox.Enabled, "Result Combobox is not enabled.");
            // New Source
            UIMap.Click_NewSourceButton_From_ExchangeSendTool();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ExchangeSourceTab.Exists, "Exchange Source Tab does not exist.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ExchangeSourceTab.SendTestModelsCustom.AutoDiscoverUrlTxtBox.Exists, "Host textbox does not exist after opening Email source tab");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ExchangeSourceTab.SendTestModelsCustom.UserNameTextBox.Exists, "Username textbox does not exist after opening Email source tab");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ExchangeSourceTab.SendTestModelsCustom.PasswordTextBox.Exists, "Password textbox does not exist after opening Email source tab");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ExchangeSourceTab.SendTestModelsCustom.ToTextBox.Exists, "Port textbox does not exist after opening Email source tab");
            UIMap.Enter_Text_Into_Exchange_Tab();
            UIMap.Click_ExchangeSource_TestConnection_Button();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ExchangeSourceTab.SendTestModelsCustom.ToTextBox.ItemImage.Exists, "Connection test Failed");
            Assert.IsTrue(UIMap.MainStudioWindow.SideMenuBar.SaveButton.Enabled, "Save ribbon button is not enabled after successfully testing new source.");
            UIMap.Save_With_Ribbon_Button_And_Dialog(SourceName);
            UIMap.Click_ExchangeSource_CloseTabButton();
            //Edit Source
            UIMap.ExchangeSendTool_ChangeView_With_DoubleClick();
            UIMap.Select_Source_From_ExchangeSendTool();
            UIMap.ExchangeSendTool_ChangeView_With_DoubleClick();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ExchangeEmail.SmallViewContent.ItemButton.Enabled, "Edit Source Button is not enabled after selecting source.");
            UIMap.Click_EditSourceButton_On_ExchangeSendToolSmallView();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ExchangeSourceTab.Exists, "Exchange Source Tab does not exist.");
            UIMap.Edit_Timeout_On_ExchangeSource();
            UIMap.Click_ExchangeSource_TestConnection_Button();
            UIMap.Click_Save_Ribbon_Button_With_No_Save_Dialog();
            UIMap.Click_ExchangeSource_CloseTabButton();
            UIMap.Click_EditSourceButton_On_ExchangeSendToolSmallView();
            Assert.AreEqual("2000", UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ExchangeSourceTab.SendTestModelsCustom.TimeoutTextBoxEdit.Text);
        }

        [TestMethod]
        [TestCategory("Tools")]
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
                UIMap.ExchangeSendTool_ChangeView_With_DoubleClick();
                UIMap.Click_SelectFilesButton_On_ExchangeEmailTool_LargeView();
                UIMap.Select_Attachments_From_SelectFilesWindow();
                Assert.IsFalse(string.IsNullOrEmpty(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ExchangeEmail.LargeViewContent.AttachmentsComboBox.TextEdit.Text), "File Directory Textbox is empty");
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
            UIMap.Drag_Toolbox_Exchange_Send_Onto_DesignSurface();
        }
        
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        private TestContext testContextInstance;

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

        #endregion
    }
}
