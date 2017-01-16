using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.Tools
{
    [CodedUITest]
    public class SMTP_Send
    {
        [TestMethod]
		[TestCategory("Tools")]
        public void SMTPSendToolUITest()
        {
            UIMap.Drag_Toolbox_SMTP_Email_Onto_DesignSurface();
            UIMap.Open_SMTP_Email_Tool_Large_View();
        }

        [TestMethod]
        [TestCategory("Tools")]
        public void SMTPMultipleAttachmentsUITest()
        {
            UIMap.Drag_Toolbox_SMTP_Email_Onto_DesignSurface();
            UIMap.Open_SMTP_Email_Tool_Large_View();
            UIMap.Click_SelectFilesButton_On_SMTPEmailTool_LargeView();
            UIMap.Select_Attachments_From_SelectFilesWindow();
            Assert.IsFalse(string.IsNullOrEmpty(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowWizardTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SMTPEmail.LargeView.AttachmentsTextComboBox.TextEdit.Text));
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
            UIMap.InitializeABlankWorkflow();
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            UIMap.Click_Close_Workflow_Tab_Button();
            UIMap.Click_MessageBox_No();
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
