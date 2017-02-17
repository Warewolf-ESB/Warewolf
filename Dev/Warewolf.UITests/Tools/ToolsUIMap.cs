using TechTalk.SpecFlow;
using Warewolf.UITests.DialogsUIMapClasses;

namespace Warewolf.UITests.Tools.ToolsUIMapClasses
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Keyboard = Microsoft.VisualStudio.TestTools.UITesting.Keyboard;
    using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;
    using MouseButtons = System.Windows.Forms.MouseButtons;
    using System.Drawing;
    using System.Windows.Input;
    using ExplorerUIMapClasses;

    public partial class ToolsUIMap
    {
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

        public void Click_SelectFilesButton_On_SMTPEmailTool_LargeView()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SMTPEmail.LargeViewContent.SelectFilesButton);
            Assert.IsTrue(DialogsUIMap.SelectFilesWindow.Exists);
        }

        public void Click_SelectFilesButton_On_ExchangeEmailTool_LargeView()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ExchangeEmail.LargeViewContent.SelectFilesButton);
            Assert.IsTrue(DialogsUIMap.SelectFilesWindow.Exists);
        }

        [When(@"I Click RequireAllFieldsToMatch CheckBox")]
        public void Click_RequireAllFieldsToMatch_CheckBox()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FindRecordsIndex.LargeViewContentCustom.RequireAllFieldsToMatchCheckBox.Checked = true;
            Keyboard.SendKeys(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FindRecordsIndex.LargeViewContentCustom.RequireAllFieldsToMatchCheckBox, "{Tab}", ModifierKeys.None);
        }

        [Given(@"I Click Read Done Button")]
        [When(@"I Click Read Done Button")]
        [Then(@"I Click Read Done Button")]
        public void Click_Read_Done_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FileRead.DoneButton, new Point(35, 6));
        }

        [Given(@"I Click ReadFolder Done Button")]
        [When(@"I Click ReadFolder Done Button")]
        [Then(@"I Click ReadFolder Done Button")]
        public void Click_ReadFolder_Done_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FolderRead.DoneButton, new Point(35, 6));
        }

        [Given(@"I Click Postgre Done Button")]
        [When(@"I Click Postgre Done Button")]
        [Then(@"I Click Postgre Done Button")]
        public void Click_Postgre_Done_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PostgreSqlActivitCustom.DoneButton, new Point(36, 11));
        }

        [Given(@"I Click PrefixContainsInvalidText Hyperlink")]
        [When(@"I Click PrefixContainsInvalidText Hyperlink")]
        [Then(@"I Click PrefixContainsInvalidText Hyperlink")]
        public void Click_PrefixContainsInvalidText_Hyperlink()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PrefixcontainsinvaliText.PrefixcontainsinvaliHyperlink, new Point(30, 4));
        }

        [Given(@"I Click New Source Button From ODBC Tool")]
        [When(@"I Click New Source Button From ODBC Tool")]
        [Then(@"I Click New Source Button From ODBC Tool")]
        public void Click_NewSourceButton_From_ODBCTool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ODBCDatabaseActivCustom.LargeView.NewSourceButton, new Point(30, 4));
        }

        [Given(@"I Click New Source Button From Oracle Tool")]
        [When(@"I Click New Source Button From Oracle Tool")]
        [Then(@"I Click New Source Button From Oracle Tool")]
        public void Click_NewSourceButton_From_OracleTool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.OracleDatabaseActCustom.LargeView.NewSourceButton, new Point(30, 4));
        }

        [Given(@"I Click New Source Button From PostgreSQL Tool")]
        [When(@"I Click New Source Button From PostgreSQL Tool")]
        [Then(@"I Click New Source Button From PostgreSQL Tool")]
        public void Click_NewSourceButton_From_PostgreSQLTool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PostgreSqlActivitCustom.LargeView.NewSourceButton);
        }

        [Given(@"I Click New Source Button From MySQL Tool")]
        [When(@"I Click New Source Button From MySQL Tool")]
        [Then(@"I Click New Source Button From MySQL Tool")]
        public void Click_NewSourceButton_From_MySQLTool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MySqlDatabase.LargeView.NewSourceButton);
        }

        [Given(@"I Click New Source Button From SQLServer Tool")]
        [When(@"I Click New Source Button From SQLServer Tool")]
        [Then(@"I Click New Source Button From SQLServer Tool")]
        public void Click_NewSourceButton_From_SqlServerTool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlServerDatabase.LargeView.NewDbSourceButton, new Point(16, 9));
        }

        [Given(@"I Click New Source Button From WCF Tool")]
        [When(@"I Click New Source Button From WCF Tool")]
        [Then(@"I Click New Source Button From WCF Tool")]
        public void Click_NewSourceButton_From_WCFTool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WcfService.LargeView.NewButton);
        }

        [Given(@"I Click New Source Button From RabbitMQConsume Tool")]
        [When(@"I Click New Source Button From RabbitMQConsume Tool")]
        [Then(@"I Click New Source Button From RabbitMQConsume Tool")]
        public void Click_NewSourceButton_From_RabbitMQConsumeTool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.RabbitMQConsume.LargeViewContentCustom.NewSourceButton);
        }

        public void Click_NewSourceButton_From_RabbitMQPublishTool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.RabbitMQPublish.LargeViewContentCustom.NewSourceButton);
        }

        [Given(@"I Click New Source Button From COMDLLPlugin Tool")]
        [When(@"I Click New Source Button From COMDLLPlugin Tool")]
        [Then(@"I Click New Source Button From COMDLLPlugin Tool")]
        public void Click_NewSourceButton_From_COMDLLPluginTool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ComDll.LargeView.NewSourceButton);
        }

        [Given(@"I Click New Source Button From DotNetDLLPlugin Tool")]
        [When(@"I Click New Source Button From DotNetDLLPlugin Tool")]
        [Then(@"I Click New Source Button From DotNetDLLPlugin Tool")]
        public void Click_NewSourceButton_From_DotNetDLLPluginTool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.NewSourceButton);
        }

        [Given(@"I Click New Source Button From SQLBulkInsert Tool")]
        [When(@"I Click New Source Button From SQLBulkInsert Tool")]
        [Then(@"I Click New Source Button From SQLBulkInsert Tool")]
        public void Click_NewSource_From_SqlBulkInsertTool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlBulkInsert.LargeViewContentCustom.DatabaseComboBox);
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlBulkInsert.LargeViewContentCustom.DatabaseComboBox.NewDatabaseSource);
        }

        [Given(@"I Click New Source Button From ExchangeSend Tool")]
        [When(@"I Click New Source Button From ExchangeSend Tool")]
        [Then(@"I Click New Source Button From ExchangeSend Tool")]
        public void Click_NewSourceButton_From_ExchangeSendTool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ExchangeEmail.LargeViewContent.NewSourceButton);
        }

        [Given(@"I Click New Source Button From HttpWebDelete Tool")]
        [When(@"I Click New Source Button From HttpWebDelete Tool")]
        [Then(@"I Click New Source Button From HttpWebDelete Tool")]
        public void Click_NewSourceButton_From_HttpWebDeleteTool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete.LargeView.NewSourceButton);
        }

        [Given(@"I Click New Source Button From HttpWebGet Tool")]
        [When(@"I Click New Source Button From HttpWebGet Tool")]
        [Then(@"I Click New Source Button From HttpWebGet Tool")]
        public void Click_NewSourceButton_From_HttpWebGetTool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebGet.LargeView.NewSourceButton);
        }

        [Given(@"I Click New Source Button From HttpWebPost Tool")]
        [When(@"I Click New Source Button From HttpWebPost Tool")]
        [Then(@"I Click New Source Button From HttpWebPost Tool")]
        public void Click_NewSourceButton_From_HttpWebPostTool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPost.LargeView.NewSourceButton);
        }

        [Given(@"I Click New Source Button From HttpWebPut Tool")]
        [When(@"I Click New Source Button From HttpWebPut Tool")]
        [Then(@"I Click New Source Button From HttpWebPut Tool")]
        public void Click_NewSourceButton_From_HttpWebPutTool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPut.LargeView.NewSourceButton);
        }

        [Given(@"I Click New Source Button From DropboxDelete Tool")]
        [When(@"I Click New Source Button From DropboxDelete Tool")]
        [Then(@"I Click New Source Button From DropboxDelete Tool")]
        public void Click_NewSourceButton_From_DropboxDeleteTool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DropboxDelete.LargeViewContent.NewSourceButton);
        }

        [Given(@"I Click New Source Button From DropboxDownload Tool")]
        [When(@"I Click New Source Button From DropboxDownload Tool")]
        [Then(@"I Click New Source Button From DropboxDownload Tool")]
        public void Click_NewSourceButton_From_DropboxDownloadTool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DropboxDownload.LargeViewContent.NewSourceButton);
        }

        [Given(@"I Click New Source Button From DropboxListContents Tool")]
        [When(@"I Click New Source Button From DropboxListContents Tool")]
        [Then(@"I Click New Source Button From DropboxListContents Tool")]
        public void Click_NewSourceButton_From_DropboxListContentsTool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DropboxFileList.LargeViewContent.NewSourceButton);
        }

        [Given(@"I Click New Source Button From DropboxUpload Tool")]
        [When(@"I Click New Source Button From DropboxUpload Tool")]
        [Then(@"I Click New Source Button From DropboxUpload Tool")]
        public void Click_NewSourceButton_From_DropboxUploadTool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DropboxUpload.LargeViewContent.NewSourceButton);
        }

        [Given(@"I Click New Source Button From SMTPSend Tool")]
        [When(@"I Click New Source Button From SMTPSend Tool")]
        [Then(@"I Click New Source Button From SMTPSend Tool")]
        public void Click_NewSource_From_SMTPSendTool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SMTPEmail.LargeViewContent.SourceComboBox);
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SMTPEmail.LargeViewContent.SourceComboBox.NewEmailSource);
        }

        [Given(@"I Click New Source Button From SharepointCopyFile Tool")]
        [When(@"I Click New Source Button From SharepointCopyFile Tool")]
        [Then(@"I Click New Source Button From SharepointCopyFile Tool")]
        public void Click_NewSource_From_SharepointCopyFileTool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCopyFile.LargeView.Server);
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCopyFile.LargeView.Server.NewSharePointSource);
        }

        [Given(@"I Click New Source Button From SharepointCreateListItem Tool")]
        [When(@"I Click New Source Button From SharepointCreateListItem Tool")]
        [Then(@"I Click New Source Button From SharepointCreateListItem Tool")]
        public void Click_NewSource_From_SharepointCreateListItemTool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCreateListItem.LargeView.Server);
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCreateListItem.LargeView.Server.NewSharePointSource);
        }

        [Given(@"I Click New Source Button From SharepointDeleteFile Tool")]
        [When(@"I Click New Source Button From SharepointDeleteFile Tool")]
        [Then(@"I Click New Source Button From SharepointDeleteFile Tool")]
        public void Click_NewSource_From_SharepointDeleteFileTool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointDeleteFile.LargeView.Server);
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointDeleteFile.LargeView.Server.NewSharePointSource);
        }

        [Given(@"I Click New Source Button From SharepointDeleteListItem Tool")]
        [When(@"I Click New Source Button From SharepointDeleteListItem Tool")]
        [Then(@"I Click New Source Button From SharepointDeleteListItem Tool")]
        public void Click_NewSource_From_SharepointDeleteListItemTool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointDeleteListItem.LargeView.Server);
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointDeleteListItem.LargeView.Server.NewSharePointSource);
        }

        [Given(@"I Click New Source Button From SharepointDownloadFile Tool")]
        [When(@"I Click New Source Button From SharepointDownloadFile Tool")]
        [Then(@"I Click New Source Button From SharepointDownloadFile Tool")]
        public void Click_NewSource_From_SharepointDownloadFileTool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointDownloadFile.LargeView.Server);
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointDownloadFile.LargeView.Server.NewSharePointSource);
        }

        [Given(@"I Click New Source Button From SharepointUploadFile Tool")]
        [When(@"I Click New Source Button From SharepointUploadFile Tool")]
        [Then(@"I Click New Source Button From SharepointUploadFile Tool")]
        public void Click_NewSource_From_SharepointUploadFileTool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUploadFile.LargeView.Server);
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUploadFile.LargeView.Server.NewSharePointSource);
        }

        [Given(@"I Click New Source Button From SharepointMoveFile Tool")]
        [When(@"I Click New Source Button From SharepointMoveFile Tool")]
        [Then(@"I Click New Source Button From SharepointMoveFile Tool")]
        public void Click_NewSource_From_SharepointMoveFileTool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointMoveFile.LargeView.Server);
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointMoveFile.LargeView.Server.NewSharePointSource);
        }

        [Given(@"I Click New Source Button From SharepointReadFolder Tool")]
        [When(@"I Click New Source Button From SharepointReadFolder Tool")]
        [Then(@"I Click New Source Button From SharepointReadFolder Tool")]
        public void Click_NewSource_From_SharepointReadFolderTool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointReadFolder.LargeView.Server);
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointReadFolder.LargeView.Server.NewSharePointSource);
        }

        [Given(@"I Click New Source Button From SharepointReadListItem Tool")]
        [When(@"I Click New Source Button From SharepointReadListItem Tool")]
        [Then(@"I Click New Source Button From SharepointReadListItem Tool")]
        public void Click_NewSource_From_SharepointReadListItemTool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointReadListItem.LargeView.Server);
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointReadListItem.LargeView.Server.NewSharePointSource);
        }

        [Given(@"I Click New Source Button From SharepointUpdateListItem Tool")]
        [When(@"I Click New Source Button From SharepointUpdateListItem Tool")]
        [Then(@"I Click New Source Button From SharepointUpdateListItem Tool")]
        public void Click_NewSource_From_SharepointUpdateListItemTool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUpdateListItem.LargeView.Server);
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUpdateListItem.LargeView.Server.NewSharePointSource);
        }

        [Given(@"I Click GET Web Large View Done Button With Invalid Large View")]
        [When(@"I Click GET Web Large View Done Button With Invalid Large View")]
        [Then(@"I Click GET Web Large View Done Button With Invalid Large View")]
        public void Click_GET_Web_Large_View_Done_Button_With_Invalid_Large_View()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebGet.DoneButton, new Point(33, 11));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Error1.Exists, "Error not exist after clicking large view done button on invalid large view.");
        }

        [Given(@"I Click FormatNumber Done Button")]
        [When(@"I Click FormatNumber Done Button")]
        [Then(@"I Click FormatNumber Done Button")]
        public void Click_FormatNumber_Done_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FormatNumber.DoneButton, new Point(36, 11));
        }

        [Given(@"I Click EndThisWF On XPath LargeView")]
        [When(@"I Click EndThisWF On XPath LargeView")]
        [Then(@"I Click EndThisWF On XPath LargeView")]
        public void Click_EndThisWF_On_XPath_LargeView()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.XPath.LargeViewContentCustom.OnErrorCustom.OnErrorGroup.EndthisworkflowCheckBox.Checked = true;
            Keyboard.SendKeys(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.XPath.LargeViewContentCustom.OnErrorCustom.OnErrorGroup.EndthisworkflowCheckBox, "{Tab}", ModifierKeys.None);
        }

        [Given(@"I Click ExpandAndStepIn NestedWorkflow")]
        [When(@"I Click ExpandAndStepIn NestedWorkflow")]
        [Then(@"I Click ExpandAndStepIn NestedWorkflow")]
        public void Click_ExpandAndStepIn_NestedWorkflow()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.SubWorkflow.Expanded = true;
        }

        [Given(@"I Click EditSharepointSource Button From SharePointUpdate")]
        [When(@"I Click EditSharepointSource Button From SharePointUpdate")]
        [Then(@"I Click EditSharepointSource Button From SharePointUpdate")]
        public void Click_EditSharepointSource_Button_From_SharePointUpdate()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUpdateListItem.SmallView.EditSourceButton, new Point(98, 12));
        }

        [Given(@"I Click EditSharepointSource Button FromSharePointDelete")]
        [When(@"I Click EditSharepointSource Button FromSharePointDelete")]
        [Then(@"I Click EditSharepointSource Button FromSharePointDelete")]
        public void Click_EditSharepointSource_Button_FromSharePointDelete()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointDeleteFile.SmallView.EditSourceButton, new Point(98, 12));
        }

        [Given(@"I Click EditSharepointSource Button FromSharePointRead")]
        [When(@"I Click EditSharepointSource Button FromSharePointRead")]
        [Then(@"I Click EditSharepointSource Button FromSharePointRead")]
        public void Click_EditSharepointSource_Button_FromSharePointRead()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointReadListItem.SmallView.EditSourceButton, new Point(98, 12));
        }

        [Given(@"I Click EditSharepointSource Button")]
        [When(@"I Click EditSharepointSource Button")]
        [Then(@"I Click EditSharepointSource Button")]
        public void Click_EditSharepointSource_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCreateListItem.SmallView.EditSourceButton, new Point(98, 12));
        }

        [Given(@"I Click Delete Done Button")]
        [When(@"I Click Delete Done Button")]
        [Then(@"I Click Delete Done Button")]
        public void Click_Delete_Done_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathDelete.DoneButton, new Point(35, 6));
        }

        [Given(@"I Click Debug Output Calculate Cell")]
        [When(@"I Click Debug Output Calculate Cell")]
        [Then(@"I Click Debug Output Calculate Cell")]
        public void Click_Debug_Output_Calculate_Cell()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.Step1.CalculateButton, new Point(24, 10));
            Assert.AreEqual("IsPrimarySelection=True IsSelection=True IsCurrentLocation=null IsCurrentContext=" +
                            "null IsBreakpointEnabled=null IsBreakpointBounded=null ValidationState=Valid ", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Calculate.ItemStatus, "Calculate tool small view is not selected.");
        }

        [Given(@"I Click Debug Output GenericResource Name")]
        [When(@"I Click Debug Output GenericResource Name")]
        [Then(@"I Click Debug Output GenericResource Name")]
        public void Click_Debug_Output_GenericResource_Name()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DrawHighlight();
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.ServiceTreeItem.GenericResourceButton, new Point(24, 8));
            Assert.AreEqual("GenericResource", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.Workflow1ciremoteText.DisplayText, "GenericResource remote workflow tab is not open.");
        }

        [Given(@"I Click Debug Output BaseConvert Cell")]
        [When(@"I Click Debug Output BaseConvert Cell")]
        [Then(@"I Click Debug Output BaseConvert Cell")]
        public void Click_Debug_Output_BaseConvert_Cell()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.Step1.BaseConversion1Button, new Point(33, 7));
            Assert.AreEqual("IsPrimarySelection=True IsSelection=True IsCurrentLocation=null IsCurrentContext=" +
                            "null IsBreakpointEnabled=null IsBreakpointBounded=null ValidationState=Valid ", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.BaseConvert.ItemStatus, "Base conversion small view is not selected.");
        }

        [Given(@"I Click Debug Output Assign Cell")]
        [When(@"I Click Debug Output Assign Cell")]
        [Then(@"I Click Debug Output Assign Cell")]
        public void Click_Debug_Output_Assign_Cell()
        {
            Assert.AreEqual("[[SomeVariable]]", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.Step1.VariableTextbox2.DisplayText, "Wrong variable name in debug output");
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.Step1.Assign1Button, new Point(21, 9));
            Assert.AreEqual("IsPrimarySelection=True IsSelection=True IsCurrentLocation=null IsCurrentContext=" +
                            "null IsBreakpointEnabled=null IsBreakpointBounded=null ValidationState=Valid ", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.ItemStatus, "Multiassign small view is not selected.");
        }

        [Given(@"I Click Assign Tool QviLarge Preview")]
        [When(@"I Click Assign Tool QviLarge Preview")]
        [Then(@"I Click Assign Tool QviLarge Preview")]
        public void Click_Assign_Tool_QviLarge_Preview()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.QuickVariableInputContent.PreviewCustom.PreviewGroup.PreviewButton, new Point(30, 4));
        }

        [Given(@"I Click Assign Tool Large View Done Button With Row1 Variable Textbox As SomeInvalidVariableName")]
        [When(@"I Click Assign Tool Large View Done Button With Row1 Variable Textbox As SomeInvalidVariableName")]
        [Then(@"I Click Assign Tool Large View Done Button With Row1 Variable Textbox As SomeInvalidVariableName")]
        public void Click_Assign_Tool_Large_View_Done_Button_With_Row1_Variable_Textbox_As_SomeInvalidVariableName()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.DoneButton, new Point(35, 6));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Error1.Hyperlink.Exists, "Error popup does not exist on flowchart designer.");
            Assert.AreEqual("", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem1.ScrollViewerPane.NameTextbox.Text, "Variable list scalar row 1 textbox text is not blank with invalid variable.");
        }

        [Given(@"I click AssignObject Done")]
        [When(@"I click AssignObject Done")]
        [Then(@"I click AssignObject Done")]
        public void click_AssignObject_Done()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.AssignObject.DoneButton, new Point(18, 10));
        }

        [Given(@"I Click Calculate Large View Done Button")]
        [When(@"I Click Calculate Large View Done Button")]
        [Then(@"I Click Calculate Large View Done Button")]
        public void Click_CalculateTool_DoneButton()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Calculate.DoneButton, new Point(45, 8));
        }

        [Given(@"I Click Rename Done Button")]
        [When(@"I Click Rename Done Button")]
        [Then(@"I Click Rename Done Button")]
        public void Click_Rename_Done_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathRename.DoneButton, new Point(35, 6));
        }

        [Given(@"I Click Assign Tool CollapseAll")]
        [When(@"I Click Assign Tool CollapseAll")]
        [Then(@"I Click Assign Tool CollapseAll")]
        public void Click_Assign_Tool_CollapseAll()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.CollapseAllToggleButton.Exists, "Expand all button does not exist");
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.CollapseAllToggleButton.Pressed = true;
        }

        [Given(@"I Click Assign Tool ExpandAll")]
        [When(@"I Click Assign Tool ExpandAll")]
        [Then(@"I Click Assign Tool ExpandAll")]
        public void Click_Assign_Tool_ExpandAll()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ExpandAllToggleButton.Exists, "Expand all button does not exist");
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ExpandAllToggleButton.Pressed = true;
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.Exists, "Assign tool large view on the design surface does not exist");
        }

        [Given(@"I Click Assign Tool Large View Done Button")]
        [When(@"I Click Assign Tool Large View Done Button")]
        [Then(@"I Click Assign Tool Large View Done Button")]
        public void Click_Assign_Tool_Large_View_Done_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.DoneButton, new Point(35, 6));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.OpenQuickVariableInpToggleButton.Exists, "QVI toggle button does not exist in assign tool small view after clicking done button on large view.");
        }

        [Given(@"I Assign Value To Variable With Assign Tool Small View Row 2")]
        [When(@"I Assign Value To Variable With Assign Tool Small View Row 2")]
        [Then(@"I Assign Value To Variable With Assign Tool Small View Row 2")]
        public void Assign_Value_To_Variable_With_Assign_Tool_Small_View_Row_2()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row2.VariableCell.IntellisenseCombobox.Textbox.Text = "[[SomeOtherVariable]]";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row2.ValueCell.IntellisenseCombobox.Textbox.Text = "100";
        }

        [Given(@"I Assign Value To Variable With Assign Tool Small View Row 1")]
        [When(@"I Assign Value To Variable With Assign Tool Small View Row 1")]
        [Then(@"I Assign Value To Variable With Assign Tool Small View Row 1")]
        public void Assign_Value_To_Variable_With_Assign_Tool_Small_View_Row_1()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row1.VariableCell.IntellisenseCombobox.Textbox.Text = "[[SomeVariable]]";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row1.ValueCell.IntellisenseCombobox.Textbox.Text = "50";
        }

        public void Drag_Toolbox_AssignObject_Onto_Sequence_LargeTool()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Assign Object";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Sequence.SequenceLargeView.AddModeNewActivity.EnsureClickable(new Point(155, 22));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.DataTools.AssignObject, new Point(13, 17));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Sequence.SequenceLargeView.AddModeNewActivity.UISacdVerticalConnectoCustom);
        }
        public void Drag_Toolbox_AssignObject_Onto_Sequence_SmallTool()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Assign Object";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Sequence.EnsureClickable(new Point(155, 22));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.DataTools.AssignObject, new Point(13, 17));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Sequence);
        }

        public void Drag_Toolbox_Decision_Onto_Sequence_SmallTool()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Decision";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Sequence.EnsureClickable(new Point(155, 22));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.FlowTools.Decision, new Point(13, 17));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Sequence);
        }

        public void Drag_Toolbox_Switch_Onto_Sequence_SmallTool()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Switch";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Sequence.EnsureClickable(new Point(155, 22));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.FlowTools.Switch, new Point(13, 17));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Sequence);
        }

        public void Drag_Toolbox_Switch_Onto_Sequence_LargeTool()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Switch";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Sequence.SequenceLargeView.AddModeNewActivity.EnsureClickable(new Point(155, 22));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.FlowTools.Switch, new Point(13, 17));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Sequence.SequenceLargeView.AddModeNewActivity.UISacdVerticalConnectoCustom);
        }

        public void Drag_Toolbox_Decision_Onto_Sequence_LargeTool()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Decision";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Sequence.SequenceLargeView.AddModeNewActivity.EnsureClickable(new Point(155, 22));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.FlowTools.Decision, new Point(13, 17));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Sequence.SequenceLargeView.AddModeNewActivity.UISacdVerticalConnectoCustom);
        }

        public void Enter_Person_Name_On_Assign_Object_tool()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.AssignObject.LargeView.DataGrid.Row1.FieldCell.FieldNameComboBox.TextEdit.Text = "[[@Person.Name]]";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.AssignObject.LargeView.DataGrid.Row1.FieldValueCell.FieldValueComboBox.TextEdit.Text = "Bob";
        }

        public void Enter_Values_Into_Case_Conversion_Tool()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.CaseConvert.SmallViewContentCustom.SmallDataGridTable.Row1.ValueCell.ValueComboBox.TextEdit.Text = "res";
        }

        public void Connect_Assign_to_Next_tool()
        {
            Mouse.Hover(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(200, 220));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(300, 220));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, 0, 44);
        }

        public void Enter_Values_Into_Replace_Tool_Large_View()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Replace.SmallViewContentCustom.InFiledsComboBox.TextEdit.Text = "[[rec().a]]";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Replace.SmallViewContentCustom.FindComboBox.TextEdit.Text = "u";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Replace.SmallViewContentCustom.ReplaceComboBox.TextEdit.Text = "o";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Replace.SmallViewContentCustom.ResultComboBox.TextEdit.Text = "res";
        }

        public void Enter_Values_Into_FindIndex_Tool()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FindIndex.SmallViewContentCustom.InFieldComboBox.TextEdit.Text = "SomeLongString";
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FindIndex.SmallViewContentCustom.IndexComboBox, new Point(85, 13));
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FindIndex.SmallViewContentCustom.IndexComboBox, new Point(62, 19));
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FindIndex.SmallViewContentCustom.CharactersComboBox.TextEdit.Text = "r";
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FindIndex.SmallViewContentCustom.CharactersComboBox.TextEdit, new Point(45, 2));
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FindIndex.SmallViewContentCustom.CharactersComboBox.TextEdit, new Point(39, 12));
            Keyboard.SendKeys(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FindIndex.SmallViewContentCustom.CharactersComboBox.TextEdit, "{Escape}", ModifierKeys.None);
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FindIndex.SmallViewContentCustom.ResultComboBox.TextEdit.Text = "res";
        }

        public void Enter_Text_Into_Copy_Tool()
        {
            var resourcesFolder = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources\Acceptance Tests";
            var resourcesFolderCopy = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources\Acceptance Tests_Copy";

            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathCopy.LargeViewContentCustom.FileOrFolderComboBox.TextEdit.Text = resourcesFolder;
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathCopy.LargeViewContentCustom.DestinationComboBox.TextEdit.Text = resourcesFolderCopy;
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathCopy.LargeViewContentCustom.ResultComboBox.TextEdit.Text = "[[results]]";
        }

        public void Enter_Text_Into_CommentTool(string comment)
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Comment.LargeViewContentCustom.CommentComboBox.TextEdit.Text = comment;
        }

        public void Enter_Text_Into_Delete_Tool()
        {
            var resourcesFolder = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources\Acceptance Tests_Copy";

            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathDelete.LargeViewContentCustom.FileOrFolderComboBox.TextEdit.Text = resourcesFolder;
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathDelete.LargeViewContentCustom.ResultComboBox.TextEdit.Text = "[[results]]";
        }

        public void Enter_Text_Into_Write_Tool()
        {
            var file = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources\Acceptance Tests\Test File.txt";

            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FileWrite.LargeViewContentCustom.FileNameComboBox.TextEdit.Text = file;
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FileWrite.LargeViewContentCustom.ContentsComboBox.TextEdit.Text = "Some Content";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FileWrite.LargeViewContentCustom.OverwriteRadioButton.Selected = true;
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FileWrite.LargeViewContentCustom.ResultComboBox.TextEdit.Text = "[[results]]";
        }

        public void Enter_Text_Into_Move_Tool()
        {
            var resourcesFolder = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources\Acceptance Tests_Copy";
            var destinationFolder = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources\Acceptance Tests";

            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathMove.LargeViewContentCustom.FileOrFolderComboBox.TextEdit.Text = resourcesFolder;
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathMove.LargeViewContentCustom.DestinationComboBox.TextEdit.Text = destinationFolder;
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathMove.LargeViewContentCustom.OverwriteCheckBox.Checked = true;
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathMove.LargeViewContentCustom.ResultComboBox.TextEdit.Text = "[[results]]";
        }

        public void Enter_Text_Into_Zip_Tool()
        {
            var resourcesFolder = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources\Acceptance Tests";
            var destinationFolder = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources\Acceptance Tests_Zip";

            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Zip.LargeViewContentCustom.FileOrFolderComboBox.TextEdit.Text = resourcesFolder;
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Zip.LargeViewContentCustom.DestinationComboBox.TextEdit.Text = destinationFolder;
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Zip.LargeViewContentCustom.OverwriteCheckBox.Checked = true;
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Zip.LargeViewContentCustom.ResultComboBox.TextEdit.Text = "[[results]]";
        }

        public void Enter_Text_Into_UnZip_Tool()
        {
            var resourcesFolder = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources\Acceptance Tests";
            var unZipFolder = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources\Acceptance Tests_UnZip";

            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.UnZip.LargeViewContentCustom.UnZipNameComboBox.TextEdit.Text = resourcesFolder;
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.UnZip.LargeViewContentCustom.DestinationComboBox.TextEdit.Text = unZipFolder;
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.UnZip.LargeViewContentCustom.OverwriteCheckBox.Checked = true;
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.UnZip.LargeViewContentCustom.ResultComboBox.TextEdit.Text = "[[results]]";
        }

        public void Enter_Text_Into_Rename_Tool()
        {
            var resourcesFolder = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources\Acceptance Tests";

            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathRename.LargeViewContentCustom.FileOrFolderComboBox.TextEdit.Text = resourcesFolder;
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathRename.LargeViewContentCustom.NewNameComboBox.TextEdit.Text = "Acceptance Tests_New";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathRename.LargeViewContentCustom.OverwriteCheckBox.Checked = true;
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathRename.LargeViewContentCustom.ResultComboBox.TextEdit.Text = "[[results]]";
        }

        public void Enter_Text_Into_ReadFolder_Tool()
        {
            var resourcesFolder = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources\Acceptance Tests";

            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FolderRead.LargeViewContentCustom.DirectoryComboBox.TextEdit.Text = resourcesFolder;
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FolderRead.LargeViewContentCustom.FilesFoldersRadioButton.Selected = true;
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FolderRead.LargeViewContentCustom.ResultComboBox.TextEdit.Text = "[[results]]";
        }

        public void Enter_Text_Into_Read_Tool()
        {
            var resourcesFolder = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources\Acceptance Tests";
            var file = resourcesFolder + @"\" + "Hello World" + ".xml";

            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FileRead.LargeViewContentCustom.FileNameComboBox.TextEdit.Text = file;
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FileRead.LargeViewContentCustom.ResultComboBox.TextEdit.Text = "[[results]]";
        }

        public void Enter_Text_Into_Create_Tool()
        {
            var resourcesFolder = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources\Acceptance Tests_Create";

            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathCreate.LargeViewContentCustom.FileNameoComboBox.TextEdit.Text = resourcesFolder;
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathCreate.LargeViewContentCustom.OverwriteCheckBox.Checked = true;
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathCreate.LargeViewContentCustom.ResultComboBox.TextEdit.Text = "[[results]]";
        }

        public void Enter_Values_Into_Data_Split_Tool()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataSplit.SmallViewContentCustom.SourceStringComboBox.TextEdit.Text = "some long string here";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataSplit.SmallViewContentCustom.SmallDataGridTable.Row1.ValueCell.ValueComboBox.TextEdit.Text = "[[res]]";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataSplit.SmallViewContentCustom.SmallDataGridTable.Row1.AtIndexCell.AtIndexComboBox.TextEdit.Text = "5";
        }

        public void Drag_Toolbox_Switch_Onto_Foreach()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Switch";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ForEach.SmallView.DropActivityHereCustom.EnsureClickable(new Point(155, 22));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.FlowTools.Switch, new Point(13, 17));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ForEach.SmallView.DropActivityHereCustom);
        }

        public void Drag_Toolbox_Decision_Onto_Foreach()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Decision";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ForEach.SmallView.DropActivityHereCustom.EnsureClickable(new Point(155, 22));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.FlowTools.Decision, new Point(13, 17));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ForEach.SmallView.DropActivityHereCustom);
        }

        public void Drag_Toolbox_AssignObject_Onto_Foreach()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Assign Object";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ForEach.SmallView.DropActivityHereCustom.EnsureClickable(new Point(155, 22));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.DataTools.AssignObject, new Point(13, 17));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ForEach.SmallView.DropActivityHereCustom);
        }

        public void Enter_Recordset_On_Length_tool()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Length.SmallViewContentCustom.RecordsetComboBox.TextEdit.Text = "[[rec()]]";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Length.SmallViewContentCustom.ResultComboBox.TextEdit.Text = "[[result]]";
        }

        public void Enter_Text_Into_Assign_QviLarge_View()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.QuickVariableInputContent.QviVariableListBoxEdit.Text = "varOne,varTwo,varThree";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.QuickVariableInputContent.QviSplitOnCharacterEdit.Text = ",";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.QuickVariableInputContent.PrefixEdit.Text = "some(<).";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.QuickVariableInputContent.SuffixEdit.Text = "_suf";
        }

        public void Enter_Recordset_On_SortRecorsds_tool()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SortRecords.SmallViewContentCustom.SortFieldComboBox.TextEdit.Text = "[[rec().a]]";
        }

        public void Enter_Recordset_On_UniqueRecorsds_tool()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Unique.SmallViewContentCustom.InFieldsComboBox.TextEdit.Text = "[[rec().a]],[[rec().b]],[[rec().c]]";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Unique.SmallViewContentCustom.ReturnFieldsComboBox.TextEdit.Text = "[[rec().b]]";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Unique.SmallViewContentCustom.ResultsComboBox.TextEdit.Text = "[[rec().c]]";

        }

        public void Enter_Recordset_On_CountRecordset_tool()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.CountRecordset.SmallViewContentCustom.RecorsetComboBox.TextEdit.Text = "[[Recordset()]]";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.CountRecordset.SmallViewContentCustom.ResultComboBox.TextEdit.Text = "[[Result]]";
        }

        public void Enter_Person_Age_On_Assign_Object_tool()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.AssignObject.LargeView.DataGrid.Row2.FieldCell.FieldNameComboBox.TextEdit.Text = "[[@Person.Age]]";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.AssignObject.LargeView.DataGrid.Row2.FieldValueCell.FieldValueComboBox.TextEdit.Text = "10";
        }

        [Given(@"I Enter variable text as ""(.*)"" and value text as ""(.*)"" into assign row 1")]
        [When(@"I Enter variable text as ""(.*)"" and value text as ""(.*)"" into assign row 1")]
        [Then(@"I Enter variable text as ""(.*)"" and value text as ""(.*)"" into assign row 1")]
        public void Enter_Variable_And_Value_Into_Assign(string VariableText, string ValueText)
        {
            Enter_Variable_And_Value_Into_Assign(VariableText, ValueText, 1);
        }

        public void Enter_Variable_And_Value_Into_Assign(string VariableText, string ValueText, int RowNumber)
        {
            switch (RowNumber)
            {
                case 2:
                    Assign_Value_To_Variable_With_Assign_Tool_Small_View_Row_2();
                    Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row3.Exists, "Assign row 3 does not exist after enter data into row 2.");
                    break;
                default:
                    Assign_Value_To_Variable_With_Assign_Tool_Small_View_Row_1();
                    Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row2.Exists, "Assign row 2 does not exist after enter data into row 1.");
                    break;
            }
        }

        public void Remove_Assign_Row_1_With_Context_Menu()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row1.RowNumberCell.Text, MouseButtons.Right, ModifierKeys.None, new Point(5, 5));
            Mouse.Click(UIMap.MainStudioWindow.DesignSurfaceContextMenu.DeleteRowMenuItem, MouseButtons.Left, ModifierKeys.None, new Point(6, 6));
            Assert.IsFalse(UIMap.ControlExistsNow(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row3), "Assign tool row 3 still exists after deleting row 1.");
        }
        public void Delete_HelloWorld_With_Context_Menu()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.HelloWorldWorkFlow, MouseButtons.Right, ModifierKeys.None, new Point(115, 10));
            Mouse.Click(UIMap.MainStudioWindow.DesignSurfaceContextMenu.Delete, new Point(27, 18));
        }

        public void Delete_Assign_With_Context_Menu()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign, MouseButtons.Right, ModifierKeys.None, new Point(115, 10));
            Mouse.Click(UIMap.MainStudioWindow.DesignSurfaceContextMenu.Delete, new Point(27, 18));
            Assert.IsFalse(UIMap.ControlExistsNow(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView), "Assign tool still exists on design surface after deleting with context menu.");
        }

        public void Click_Sharepoint_RefreshButton_From_SharepointRead()
        {
            var refreshButton = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointReadListItem.SmallView.RefreshButton;
            Mouse.Click(refreshButton);
        }

        public void Click_Sharepoint_RefreshButton()
        {
            var refreshButton = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCreateListItem.SmallView.RefreshButton;
            Mouse.Click(refreshButton);
        }

        public void Click_Sharepoint_RefreshButton_From_SharepointUpdate()
        {
            var refreshButton = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUpdateListItem.SmallView.RefreshButton;
            Mouse.Click(refreshButton);
        }

        [Given(@"I Select AppData From MethodList")]
        [When(@"I Select AppData From MethodList")]
        [Then(@"I Select AppData From MethodList")]
        public void Select_AppData_From_MethodList_From_UpdateTool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUpdateListItem.SmallView.MethodList, new Point(174, 7));
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUpdateListItem.SmallView.MethodList.UIAppdataListItem, new Point(43, 15));
        }


        [Given(@"I Select Acceptance Test in delete")]
        [When(@"I Select Acceptance Test in delete")]
        [Then(@"I Select Acceptance Test in delete")]
        public void Select_AcceptanceTestin_From_DeleteListItemsTool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointDeleteListItem.SmallView.MethodList, new Point(119, 7));
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointDeleteListItem.SmallView.MethodList.UIAcceptanceTesting_CrListItem, new Point(114, 13));
        }

        [Given(@"I Select AppData From MethodList")]
        [When(@"I Select AppData From MethodList")]
        [Then(@"I Select AppData From MethodList")]
        public void Select_AppData_From_MethodList_From_DeleteTool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointDeleteListItem.SmallView.MethodList, new Point(174, 7));
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointDeleteListItem.SmallView.MethodList.UIAppdataListItem, new Point(43, 15));
        }

        public void Click_Assign_Tool_Remove_Variable_From_Tool()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.Exists, "Assign tool large view on the design surface does not exist");
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.LargeView.DataGrid.Row1.VariableCell.IntellisenseCombobox.Textbox.Text = "[[SomeOtherVariable]]";
            Keyboard.SendKeys(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.LargeView.DataGrid.Row1.VariableCell.IntellisenseCombobox.Textbox, "{Right}{Tab}", ModifierKeys.None);
            Assert.AreEqual("[[Some$Invalid%Variable]]", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.LargeView.DataGrid.Row1.VariableCell.IntellisenseCombobox.Textbox.Text, "Multiassign small view row 1 variable textbox text does not equal \"[[Some$Invalid%Variable]]\".");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.LargeView.DataGrid.Row1.VariableCell.IntellisenseCombobox.Textbox.Exists, "Assign large view row 1 variable textbox does not exist");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.SearchTextbox.Exists, "Variable filter textbox does not exist");
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.SearchTextbox.Text = "Other";
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.SearchTextbox.ClearSearchButton.Exists, "Variable clear filter button does not exist");
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.SearchTextbox.ClearSearchButton, new Point(8, 13));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem1.ScrollViewerPane.NameTextbox.DeleteButton.Exists, "Variable delete does not exist");
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem1.ScrollViewerPane.NameTextbox.DeleteButton, new Point(9, 8));
        }

        [Given("I Click Subworkflow Done Button")]
        [When("I Click Subworkflow Done Button")]
        [Then("I Click Subworkflow Done Button")]
        public void Click_Subworkflow_Done_Button()
        {
            Assert.IsTrue(
                MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView
                    .DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ExternalWorkFlow
                    .DoneButton.Exists, "Done button does not exist afer dragging dice service onto design surface");
            Mouse.Click(
                MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView
                    .DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ExternalWorkFlow
                    .DoneButton, new Point(53, 16));
        }

        public void Enter_Recordset_values()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.LargeView.DataGrid.Row1.VariableCell.IntellisenseCombobox.Textbox.Text = "[[rec().a]]";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.LargeView.DataGrid.Row1.ValueCell.IntellisenseCombobox.Textbox.Text = "5";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.LargeView.DataGrid.Row2.VariableCell.IntellisenseCombobox.Textbox.Text = "[[rec().b]]";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.LargeView.DataGrid.Row2.ValueCell.IntellisenseCombobox.Textbox.Text = "10";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.LargeView.DataGrid.Row3.VariableCell.IntellisenseCombobox.Textbox.Text = "[[var]]";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.LargeView.DataGrid.Row3.ValueCell.IntellisenseCombobox.Textbox.Text = "1";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.LargeView.DataGrid.Row4.VariableCell.IntellisenseCombobox.Textbox.Text = "[[mr()]]";
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem1.Exists, "var does not exist in the variable explorer");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.RecordsetTreeItem.TreeItem1.Field1.Exists, "rec().a does not exist in the variable explorer");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.RecordsetTreeItem.TreeItem1.Field2.Exists, "rec().b does not exist in the variable explorer");
        }

        public void Scroll_DownThenUp_On_DataMergeTool_SmallView()
        {
            Mouse.MoveScrollWheel(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataMerge.SmallView.DataGrid, -1);
        }

        [When(@"I Click System Information Tool Done Button")]
        public void Click_System_Information_Tool_Done_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.GatherSystemInfo.DoneButton, new Point(35, 6));
        }

        [When(@"I Click UnZip Done Button")]
        public void Click_UnZip_Done_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.UnZip.DoneButton, new Point(35, 6));
        }

        [When(@"I Click SQL Server Large View Done Button")]
        public void Click_SQL_Server_Large_View_Done_Button()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlServerDatabase.DoneButton.Exists, "SQL Server large view done button does not exist.");
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlServerDatabase.DoneButton, new Point(35, 6));
        }

        [When(@"I Click SQL Server Large View Generate Outputs")]
        public void Click_SQL_Server_Large_View_Generate_Outputs()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlServerDatabase.LargeView.GenerateOutputsButton.Exists, "SQL Server large view does not contain a generate outputs button.");
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlServerDatabase.LargeView.GenerateOutputsButton, new Point(7, 7));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlServerDatabase.LargeView.TestInputsTable.Row1.TestDataCell.TestDataComboBox.TestDataTextbox.Exists, "SQL Server large view test inputs row 1 test data textbox does not exist.");
        }

        [When(@"I Click SQL Server Large View Test Inputs Button")]
        public void Click_SQL_Server_Large_View_Test_Inputs_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlServerDatabase.LargeView.TestInputsButton, new Point(21, 11));
        }

        [When(@"I Click SQL Server Large View Test Inputs Done Button")]
        public void Click_SQL_Server_Large_View_Test_Inputs_Done_Button()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlServerDatabase.LargeView.TestInputsDoneButton.Exists, "SQL Server large view test inputs done button does not exist.");
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlServerDatabase.LargeView.TestInputsDoneButton, new Point(35, 6));
        }

        [When(@"I Click SqlBulkInsert Done Button")]
        public void Click_SqlBulkInsert_Done_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlBulkInsert.DoneButton, new Point(35, 6));
        }

        [When(@"I Click Start Node")]
        public void Click_Start_Node()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.StartNode, new Point(29, 76));
        }

        [When(@"I Click WebRequest Tool Large View Done Button")]
        public void Click_WebRequest_Tool_Large_View_Done_Button()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebRequest.DoneButton, new Point(35, 6));
        }

        [When(@"I Click Write Done Button")]
        public void Click_Write_Done_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FileWrite.DoneButton, new Point(35, 6));
        }

        [When(@"I Click Variable IsInput")]
        public void Click_Variable_IsInput()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem1.InputCheckbox.Checked = true;
        }

        [When(@"I Click VariableList Scalar Row1 IsInputCheckbox")]
        public void Click_VariableList_Scalar_Row1_IsInputCheckbox()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem1.InputCheckbox.Checked = true;
        }

        [When(@"I Click VariableList Scalar Row2 IsInputCheckbox")]
        public void Click_VariableList_Scalar_Row2_IsInputCheckbox()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem2.InputCheckbox.Checked = true;
        }

        [When(@"I Click VariableList Recordset Row1 IsInputCheckbox")]
        public void Click_VariableList_Recordset_Row1_IsInputCheckbox()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.RecordsetTreeItem.TreeItem1.Field1.InputCheckbox.Checked = true;
        }

        [When(@"I Click VariableList Scalar Row1 Delete Button")]
        public void Click_VariableList_Scalar_Row1_Delete_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem1.ScrollViewerPane.NameTextbox.DeleteButton.Image, new Point(5, 8));
        }

        public void DeleteAssign_FromContextMenu()
        {
            var point = new Point();
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.TryGetClickablePoint(out point));
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign, MouseButtons.Right, ModifierKeys.None, new Point(115, 10));
            Mouse.Click(UIMap.MainStudioWindow.DesignSurfaceContextMenu.Delete, new Point(27, 18));
            Assert.IsFalse(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.TryGetClickablePoint(out point));
        }

        [When(@"I Click Assign tool VariableTextbox")]
        public void Click_Assign_tool_VariableTextbox()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row1.VariableCell.IntellisenseCombobox.Textbox);
        }

        [When(@"I Click Assign tool ValueTextbox")]
        public void Click_Assign_tool_ValueTextbox()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row1.ValueCell.IntellisenseCombobox.Textbox);
        }

        [When(@"I Click Zip Done Button")]
        public void Click_Zip_Done_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Zip.DoneButton, new Point(35, 6));
        }

        [When(@"I Close Data Merge LargeView")]
        public void Open_DataMergeTool_SmallView()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataMerge, new Point(257, 7));
        }

        [When(@"I CopyAndPaste Decision Tool On The Designer")]
        public void CopyAndPaste_Decision_Tool_On_The_Designer()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Decision, MouseButtons.Right, ModifierKeys.None, new Point(64, 15));
            Mouse.Click(UIMap.MainStudioWindow.DesignSurfaceContextMenu.Copy, new Point(64, 15));
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, MouseButtons.Right, ModifierKeys.None, new Point(64, 15));
            Mouse.Click(UIMap.MainStudioWindow.DesignSurfaceContextMenu.Paste, new Point(64, 15));
        }

        public void Enter_Values_Into_DataMergeTool()
        {
            var row1InputVariabComboBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataMerge.LargeView.DataGrid.Row.InputCell.Row1InputVariabComboBox;
            var row1UsingComboBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataMerge.LargeView.DataGrid.Row.UsingCell.Row1UsingComboBox;
            var row2InputVariabComboBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataMerge.LargeView.DataGrid.Row2.InputCell.Row2InputVariabComboBox;
            var row2UsingComboBox = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataMerge.LargeView.DataGrid.Row2.UsingCell.Row2UsingComboBox;

            row1InputVariabComboBox.TextEdit.Text = "VarA";
            row1UsingComboBox.TextEdit.Text = "1";
            row2InputVariabComboBox.TextEdit.Text = "VarB";
            row2UsingComboBox.TextEdit.Text = "2";

            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataMerge.LargeView.DataGrid.Row3.MergeTypeCell.Row4MergeTypeComboBox, ModifierKeys.None);
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataMerge.LargeView.DataGrid.Row3.MergeTypeCell.Row4MergeTypeComboBox.NewLineListItem, ModifierKeys.None);
        }

        [Given(@"I Drag Toolbox Comment Onto Switch Left Arm On DesignSurface")]
        [When(@"I Drag Toolbox Comment Onto Switch Left Arm On DesignSurface")]
        [Then(@"I Drag Toolbox Comment Onto Switch Left Arm On DesignSurface")]
        public void First_Drag_Toolbox_Comment_Onto_Switch_Left_Arm_On_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Comment";
            var switchLeftAutoConnector = new Point(250, 200);
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(switchLeftAutoConnector);
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.UtilityTools.Comment, new Point(16, 25));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, switchLeftAutoConnector);
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector2.Exists, "Second connector does not exist on design surface after drop onto autoconnector.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Comment.Exists, "Comment tool does not exist on the design surface after drag and drop from the toolbox.");
        }

        public void Resize_Assign_LargeTool()
        {
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.DoneButton.ItemIndicator, new Point(13, 17));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(500, 562));
        }

        [Given(@"I Drag Toolbox Comment Onto Switch Right Arm On DesignSurface")]
        [When(@"I Drag Toolbox Comment Onto Switch Right Arm On DesignSurface")]
        [Then(@"I Drag Toolbox Comment Onto Switch Right Arm On DesignSurface")]
        public void Then_Drag_Toolbox_Comment_Onto_Switch_Right_Arm_On_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(360, 200));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.UtilityTools.Comment, new Point(16, 25));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(360, 200));
            Assert.IsTrue(DialogsUIMap.DecisionOrSwitchDialog.DoneButton.Exists, "Switch case dialog done button does not exist after dragging onto switch case arm.");
        }

        [When(@"I DisplayStartNodeContextMenu")]
        public void DisplayStartNodeContextMenu()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.StartNode, MouseButtons.Right, ModifierKeys.None, new Point(179, 31));
        }

        [When(@"I First Drag Toolbox Comment Onto Switch Left Arm On DesignSurface")]
        public void WhenIFirstDragToolboxCommentOntoSwitchLeftArmOnDesignSurface()
        {
            First_Drag_Toolbox_Comment_Onto_Switch_Left_Arm_On_DesignSurface();
        }

        [When(@"I Then Drag Toolbox Comment Onto Switch Right Arm On DesignSurface")]
        public void WhenIThenDragToolboxCommentOntoSwitchRightArmOnDesignSurface()
        {
            var switchRightAutoConnector = new Point(360, 200);
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(switchRightAutoConnector);
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.UtilityTools.Comment, new Point(16, 25));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, switchRightAutoConnector);
            Assert.IsTrue(DialogsUIMap.DecisionOrSwitchDialog.Exists, "DecisionSwitch Dialog did not open");
            Mouse.Click(DialogsUIMap.DecisionOrSwitchDialog.DoneButton, new Point(34, 10));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector3.Exists, "Third connector does not exist on design surface after drop onto autoconnector.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Comment.Exists, "Comment tool does not exist on the design surface after drag and drop from the toolbox.");
        }

        [When(@"I Drag Dice Onto Dice On The DesignSurface")]
        public void Drag_Dice_Onto_Dice_On_The_DesignSurface()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(353, 479));
            Mouse.StartDragging(ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.FirstSubItem, new Point(49, 10));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(353, 479));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector2.Exists, "Second connector does not exist on design surface.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ExternalWorkFlow.DoneButton.Exists, "Done button does not exist afer dragging dice service onto design surface");
        }

        [When(@"I Drag DotNet DLL Connector Onto DesignSurface")]
        public void Drag_DotNetDLLConnector_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "DotNet DLL";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(308, 127));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.ResourceTools.DotNetDLL, new Point(16, 25));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(308, 127));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Com DLL Connector Onto DesignSurface")]
        public void Drag_ComDLLConnector_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Com DLL";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(308, 127));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.ResourceTools.ComDLL, new Point(16, 25));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(308, 127));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag WCF Service Connector Onto DesignSurface")]
        public void Drag_WCFServiceConnector_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "WCF";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(308, 127));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.ResourceTools.WCF, new Point(16, 25));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(308, 127));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Explorer Localhost First Item Onto Workflow Design Surface")]
        public void Drag_Explorer_Localhost_First_Item_Onto_Workflow_Design_Surface()
        {
            Assert.IsTrue(ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.Exists, "No items to drag found in the explorer tree.");
            Mouse.Click(ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem, new Point(64, 5));
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(307, 128));
            Mouse.StartDragging(ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem, new Point(64, 5));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 128));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Explorer Localhost First Items First Sub Item Onto Workflow Design Surface")]
        public void Drag_Explorer_Localhost_First_Items_First_Sub_Item_Onto_Workflow_Design_Surface()
        {
            Assert.IsTrue(ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.FirstSubItem.Exists, "No items to drag found in the explorer tree.");
            Mouse.Click(ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.FirstSubItem, new Point(90, 10));
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(307, 128));
            Mouse.StartDragging(ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.FirstSubItem, new Point(90, 10));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 128));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Explorer Localhost Second Items First Sub Item Onto Workflow Design Surface")]
        public void Drag_Explorer_Localhost_Second_Item_Onto_Workflow_Design_Surface()
        {
            Assert.IsTrue(ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.SecondItem.Exists, "No items to drag found in the explorer tree.");
            Mouse.Click(ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.SecondItem, new Point(90, 10));
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(307, 128));
            Mouse.StartDragging(ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.SecondItem, new Point(90, 10));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 128));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }


        [Given(@"I Drag Explorer Remote GenericResource Onto Workflow Design Surface")]
        [When(@"I Drag Explorer Remote GenericResource Onto Workflow Design Surface")]
        [Then(@"I Drag Explorer Remote GenericResource Onto Workflow Design Surface")]
        public void Drag_Explorer_Remote_workflow1_Onto_Workflow_Design_Surface()
        {
            if (ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.FirstRemoteServer.FirstItem.ItemEdit.Text != "GenericResource")
                ExplorerUIMap.Click_Explorer_Refresh_Button();
            Assert.IsTrue(ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.FirstRemoteServer.FirstItem.ItemEdit.Text == "GenericResource", "Explorer first remote server first item is not Generic Resource.");
            Mouse.Click(ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.FirstRemoteServer.FirstItem, new Point(64, 5));
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(307, 128));
            Mouse.StartDragging(ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.FirstRemoteServer.FirstItem, new Point(64, 5));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 128));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SubWorkflow.Exists, "Workflow on the design surface does not exist");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [Given(@"I Drag Explorer workflow Onto Workflow Design Surface")]
        [When(@"I Drag Explorer workflow Onto Workflow Design Surface")]
        [Then(@"I Drag Explorer workflow Onto Workflow Design Surface")]
        public void Drag_Explorer_workflow_Onto_Workflow_Design_Surface()
        {
            Assert.IsTrue(ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.Exists, "Explorer first remote server does not contain any items.");
            Mouse.Click(ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem, new Point(64, 5));
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(307, 128));
            Mouse.StartDragging(ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem, new Point(64, 5));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 128));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.HelloWorldWorkFlow.Exists, "Workflow on the design surface does not exist");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I change Hello World input variable")]
        public void Change_HelloWorld_InputVariable()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.HelloWorldWorkFlow.ServiceDesignerLargeView.InputsDataGridTable.InputsGridRowOne.InputsGridRowOneCell.InputsAutoCompleteTextBox.InputsAutoCompleteTextBoxText.Text = "NewName";
            Keyboard.SendKeys("{Tab}", ModifierKeys.None);
            Assert.AreEqual("[[NewName]]", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.HelloWorldWorkFlow.ServiceDesignerLargeView.InputsDataGridTable.InputsGridRowOne.InputsGridRowOneCell.InputsAutoCompleteTextBox.InputsAutoCompleteTextBoxText.Text);
        }

        [When(@"I change Hello World output variable")]
        public void Change_HelloWorld_OutputVariable()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.HelloWorldWorkFlow.ServiceDesignerLargeView.OutputsDataGridTable.OutputsGridRowOne.OutputsGridRowOneCell.OutputsAutoCompleteTextBox.OutputsAutoCompleteTextBoxText.Text = "NewMessage";
            Keyboard.SendKeys("{Tab}", ModifierKeys.None);
            Assert.AreEqual("[[NewMessage]]", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.HelloWorldWorkFlow.ServiceDesignerLargeView.OutputsDataGridTable.OutputsGridRowOne.OutputsGridRowOneCell.OutputsAutoCompleteTextBox.OutputsAutoCompleteTextBoxText.Text);
        }

        [When(@"I open ""(.*)"" in Remote Connection Integration")]
        public void WhenIOpenInRemoteConnectionIntegration(string resourceName)
        {
            Assert.IsTrue(ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.FirstRemoteServer.FirstItem.Exists, "Explorer first remote server does not contain any items.");
            Mouse.DoubleClick(ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.FirstRemoteServer.FirstItem, new Point(64, 5));
        }

        [When(@"I Drag Toolbox AggregateCalculate Onto DesignSurface")]
        public void Drag_Toolbox_AggregateCalculate_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Aggregate Calculate";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(307, 128));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.UtilityTools.AggregateCalculate, new Point(13, 17));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 128));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox AssignObject Onto DesignSurface")]
        public void Drag_Toolbox_AssignObject_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Assign Object";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(307, 128));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.DataTools.AssignObject, new Point(13, 17));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 128));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Base Conversion Onto DesignSurface")]
        public void Drag_Toolbox_Base_Conversion_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Base Convert";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(303, 128));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.DataTools.BaseConvert, new Point(12, 12));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(303, 128));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Calculate Onto DesignSurface")]
        public void Drag_Toolbox_Calculate_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Calculate";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(305, 131));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.UtilityTools.Calculate, new Point(59, -17));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(305, 131));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Case Conversion Onto DesignSurface")]
        public void Drag_Toolbox_Case_Conversion_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Case Convert";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(303, 130));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.DataTools.CaseConvert, new Point(19, 13));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(303, 130));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox CMD Line Onto DesignSurface")]
        public void Drag_Toolbox_CMD_Line_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "CMD Script";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(305, 122));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.ScriptingTools.CMDScript, new Point(19, 19));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(305, 122));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Comment Onto DesignSurface")]
        public void Drag_Toolbox_Comment_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Comment";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(308, 129));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.UtilityTools.Comment, new Point(40, 15));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(308, 129));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Copy Onto DesignSurface")]
        public void Drag_Toolbox_Copy_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Copy";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(310, 129));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.FileAndFTP.Copy, new Point(19, -3));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(310, 129));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Count Records Onto DesignSurface")]
        public void Drag_Toolbox_Count_Records_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Count";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(307, 125));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.RecordsetTools.Count, new Point(13, 18));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 125));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Create Onto DesignSurface")]
        public void Drag_Toolbox_Create_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Create";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(308, 131));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.FileAndFTP.Create, new Point(9, 16));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(308, 131));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Data Merge Onto DesignSurface")]
        public void Drag_Toolbox_Data_Merge_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Data Merge";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(305, 133));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.DataTools.DataMerge, new Point(54, 23));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(305, 133));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Data Split Onto DesignSurface")]
        public void Drag_Toolbox_Data_Split_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Data Split";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(308, 129));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.DataTools.DataSplit, new Point(3, 8));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(308, 129));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [Given(@"I Drag Toolbox Date And Time Onto DesignSurface")]
        [When(@"I Drag Toolbox Date And Time Onto DesignSurface")]
        [Then(@"I Drag Toolbox Date And Time Onto DesignSurface")]
        public void Drag_Toolbox_Date_And_Time_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Date Time";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(304, 127));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.UtilityTools.DateTime, new Point(20, -1));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(304, 127));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox DateTime Difference Onto DesignSurface")]
        public void Drag_Toolbox_DateTime_Difference_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Date Time Diff";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(306, 131));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.UtilityTools.DateTimeDifference, new Point(48, 7));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(306, 131));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Decision Onto DesignSurface")]
        public void Drag_Toolbox_Decision_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Decision";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(309, 128));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.FlowTools.Decision, new Point(16, 11));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(309, 128));
        }

        [When(@"I Drag Toolbox Delete Onto DesignSurface")]
        public void Drag_Toolbox_Delete_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Delete";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(306, 125));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.FileAndFTP.Delete, new Point(13, 9));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(306, 125));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Delete Record Onto DesignSurface")]
        public void Drag_Toolbox_Delete_Record_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Delete";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(309, 128));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.RecordsetTools.Delete, new Point(43, 6));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(309, 128));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Dropbox Delete Onto DesignSurface")]
        public void Drag_Toolbox_Dropbox_Delete_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Dropbox";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(307, 127));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.StorageDropbox.Delete, new Point(240, 550));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 127));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Dropbox Download Onto DesignSurface")]
        public void Drag_Toolbox_Dropbox_Download_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Dropbox";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(307, 129));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.StorageDropbox.Download, new Point(16, 6));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 129));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Dropbox FileList Onto DesignSurface")]
        public void Drag_Toolbox_Dropbox_FileList_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Dropbox";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(306, 128));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.StorageDropbox.ListContents, new Point(124, 550));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(306, 128));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Dropbox Upload Onto DesignSurface")]
        public void Drag_Toolbox_Dropbox_Upload_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Dropbox";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(308, 126));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.StorageDropbox.Upload, new Point(66, 550));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(308, 126));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DropboxUpload.Exists, "Dropbox upload tool does not exist on design surface after dragging in from the toolbox.");
        }

        [When(@"I Drag Toolbox Exchange Send Onto DesignSurface")]
        public void Drag_Toolbox_Exchange_Send_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Exchange Send";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(308, 129));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.Email.ExchangeSend, new Point(16, -39));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(308, 129));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Find Index Onto DesignSurface")]
        public void Drag_Toolbox_Find_Index_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Find Index";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(305, 131));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.DataTools.FindIndex, new Point(9, 5));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(305, 131));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Find Record Index Onto DesignSurface")]
        public void Drag_Toolbox_Find_Record_Index_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Find Records";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(307, 130));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.RecordsetTools.FindRecords, new Point(8, 8));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 130));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox For Each Onto DesignSurface")]
        public void Drag_Toolbox_For_Each_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "ForEach";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(307, 129));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.LoopTools.ForEach, new Point(40, 19));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 129));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Format Number Onto DesignSurface")]
        public void Drag_Toolbox_Format_Number_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Format Number";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(305, 131));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.UtilityTools.FormatNumber, new Point(18, 11));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(305, 131));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox HTTPGETWebTool Onto DesignSurface")]
        public void Drag_HTTPGETWebTool_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "GET";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(306, 126));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.HTTPWebMethods.GET, new Point(16, 25));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(306, 126));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox HTTPPOSTWebTool Onto DesignSurface")]
        public void Drag_HTTPPOSTWebTool_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "POST";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(306, 128));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.HTTPWebMethods.POST, new Point(20, 35));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(306, 128));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox HTTPPUTWebTool Onto DesignSurface")]
        public void Drag_HTTPPUTWebTool_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "PUT";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(306, 126));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.HTTPWebMethods.PUT, new Point(16, 25));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(306, 126));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox HTTPDELETEWebTool Onto DesignSurface")]
        public void Drag_HTTPDELETEWebTool_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "DELETE";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(306, 126));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.HTTPWebMethods.DELETE, new Point(16, 25));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(306, 126));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Javascript Onto DesignSurface")]
        public void Drag_Toolbox_Javascript_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Javascript";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(307, 130));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.ScriptingTools.JavaScript, new Point(49, 17));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 130));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox JSON Onto DesignSurface")]
        public void Drag_Toolbox_JSON_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Create JSON";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(305, 127));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.UtilityTools.CreateJSON, new Point(0, 10));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(305, 127));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.CreateJson.Exists, "Create JSON tool on the design surface does not exist");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Length Onto DesignSurface")]
        public void Drag_Toolbox_Length_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Length";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(308, 125));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.RecordsetTools.Length, new Point(16, 6));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(308, 125));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Move Onto DesignSurface")]
        public void Drag_Toolbox_Move_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Move";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(306, 129));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.FileAndFTP.Move, new Point(32, 4));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(306, 129));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [Given(@"I Drag Toolbox MultiAssign Onto DesignSurface")]
        [When(@"I Drag Toolbox MultiAssign Onto DesignSurface")]
        [Then(@"I Drag Toolbox MultiAssign Onto DesignSurface")]
        public void Drag_Toolbox_MultiAssign_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Assign";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(307, 128));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.DataTools.MultiAssign, new Point(13, 17));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 128));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [Given(@"I Make Workflow Savable")]
        [When(@"I Make Workflow Savable")]
        [Then(@"I Make Workflow Savable")]
        public void Make_Workflow_Savable_By_Dragging_Start()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.StartNode.Exists, "Start Node does not Exist");
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(307, 128));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.StartNode);
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 128));
        }

        [Given(@"I Make Workflow Savable And Then Save")]
        [When(@"I Make Workflow Savable And Then Save")]
        [Then(@"I Make Workflow Savable And Then Save")]
        public void Make_Workflow_Savable_Then_Save()
        {
            var startNode = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.StartNode;
            Assert.IsTrue(startNode.Exists, "Start Node does not Exist");
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(307, 128));
            Mouse.StartDragging(startNode);
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 128));
            Assert.IsTrue(UIMap.MainStudioWindow.SideMenuBar.SaveButton.Enabled, "Make Workflow Savable was unsucessful.");
            Keyboard.SendKeys(startNode, "S", (ModifierKeys.Control));
        }

        [When(@"I Drag Toolbox MySql Database Onto DesignSurface")]
        public void Drag_Toolbox_MySql_Database_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "MySQL";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(306, 130));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.Database.MySQL, new Point(10, 15));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(306, 130));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox ODBC Dtatbase Onto DesignSurface")]
        public void Drag_Toolbox_ODBC_Dtatbase_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "ODBC";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(306, 130));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.Database.ODBC, new Point(10, 15));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(306, 130));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Oracle Database Onto DesignSurface")]
        public void Drag_Toolbox_Oracle_Database_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Oracle";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(306, 130));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.Database.Oracle, new Point(11, 20));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(306, 130));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox PostgreSql Onto DesignSurface")]
        public void Drag_Toolbox_PostgreSql_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Postgre";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(306, 130));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.Database.Postgre, new Point(10, 15));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(306, 130));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Python Onto DesignSurface")]
        public void Drag_Toolbox_Python_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Python";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(307, 130));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.ScriptingTools.Python, new Point(49, 17));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 130));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox RabbitMqConsume Onto DesignSurface")]
        public void Drag_Toolbox_RabbitMqConsume_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "RabbitMq Consume";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(309, 128));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.UtilityTools.RabbitMQConsume, new Point(16, 11));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(309, 128));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox RabbitMqPublish Onto DesignSurface")]
        public void Drag_Toolbox_RabbitMqPublish_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "RabbitMq Publish";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(309, 128));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.UtilityTools.RabbitMQPublish, new Point(16, 11));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(309, 128));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Random Onto DesignSurface")]
        public void Drag_Toolbox_Random_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Random";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(308, 127));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.UtilityTools.Random, new Point(9, -21));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(308, 127));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Read File Onto DesignSurface")]
        public void Drag_Toolbox_Read_File_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Read File";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(304, 125));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.FileAndFTP.ReadFile, new Point(12, 15));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(304, 125));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Read Folder Onto DesignSurface")]
        public void Drag_Toolbox_Read_Folder_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Read Folder";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(305, 129));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.FileAndFTP.ReadFolder, new Point(14, 3));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(305, 129));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Rename Onto DesignSurface")]
        public void Drag_Toolbox_Rename_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Rename";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(305, 128));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.FileAndFTP.Rename, new Point(6, 11));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(305, 128));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Replace Onto DesignSurface")]
        public void Drag_Toolbox_Replace_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Replace";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(306, 121));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.DataTools.Replace, new Point(16, 10));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(306, 121));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Ruby Onto DesignSurface")]
        public void Drag_Toolbox_Ruby_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Ruby";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(307, 130));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.ScriptingTools.Ruby, new Point(49, 17));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 130));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox SelectAndApply Onto DesignSurface")]
        public void Drag_Toolbox_SelectAndApply_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Select and apply";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(307, 129));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.LoopTools.Selectandapply, new Point(40, 19));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 129));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging select and apply tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Sequence Onto DesignSurface")]
        public void Drag_Toolbox_Sequence_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Sequence";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(305, 131));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.FlowTools.Sequence, new Point(18, -12));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(305, 131));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Service Picker Onto DesignSurface")]
        public void Drag_Toolbox_Service_Picker_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Service";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(304, 126));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.ResourceTools.Service, new Point(50, 5));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(304, 126));
        }

        [Given(@"I Drag Toolbox Sharepoint CopyFile Onto DesignSurface")]
        [When(@"I Drag Toolbox Sharepoint CopyFile Onto DesignSurface")]
        [Then(@"I Drag Toolbox Sharepoint CopyFile Onto DesignSurface")]
        public void Drag_Toolbox_Sharepoint_CopyFile_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Copy File";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(307, 128));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.SharepointTools.CopyFile, new Point(10, 16));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 128));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Sharepoint Create Onto DesignSurface")]
        public void Drag_Toolbox_Sharepoint_Create_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Create List Item";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(311, 128));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.SharepointTools.CreateListItems, new Point(10, 16));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(311, 128));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }


        [When(@"I Drag Toolbox Sharepoint Delete List Item Onto DesignSurface")]
        public void Drag_Toolbox_Sharepoint_DeleteListItem_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Delete List Item";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(306, 131));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.SharepointTools.DeleteListItems, new Point(16, 5));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(306, 131));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Sharepoint Delete File Onto DesignSurface")]
        public void Drag_Toolbox_Sharepoint_DeleteFile_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Delete File";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(306, 131));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.SharepointTools.DeleteFile, new Point(16, 5));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(306, 131));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Sharepoint Download File Onto DesignSurface")]
        public void Drag_Toolbox_Sharepoint_Download_File_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Download";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(307, 129));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.SharepointTools.DownloadFile, new Point(124, 593));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 129));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Sharepoint MoveFile Onto DesignSurface")]
        public void Drag_Toolbox_Sharepoint_MoveFile_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Move";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(311, 128));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.SharepointTools.MoveFile, new Point(10, 16));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(311, 128));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Sharepoint Read Folder Onto DesignSurface")]
        public void Drag_Toolbox_Sharepoint_Read_Folder_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Read";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(303, 128));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.SharepointTools.ReadFolder, new Point(13, 15));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(303, 128));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Sharepoint Read List Item Onto DesignSurface")]
        public void Drag_Toolbox_Sharepoint_Read_List_Item_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Read List Item";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(303, 128));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.SharepointTools.ReadListItems, new Point(13, 15));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(303, 128));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Sharepoint Update Onto DesignSurface")]
        public void Drag_Toolbox_Sharepoint_Update_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Update List Item";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(300, 127));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.SharepointTools.UpdateListItems, new Point(17, 9));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(300, 127));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Sharepoint UploadFile Onto DesignSurface")]
        public void Drag_Toolbox_Sharepoint_UploadFile_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Upload";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(311, 128));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.SharepointTools.UploadFile, new Point(10, 16));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(311, 128));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox SMTP Email Onto DesignSurface")]
        public void Drag_Toolbox_SMTP_Email_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "SMTP Send";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(308, 129));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.Email.SMTPSend, new Point(16, -39));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(308, 129));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Sort Record Onto DesignSurface")]
        public void Drag_Toolbox_Sort_Record_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Sort";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(300, 122));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.RecordsetTools.Sort, new Point(7, 8));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(300, 122));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox SQL Bulk Insert Onto DesignSurface")]
        public void Drag_Toolbox_SQL_Bulk_Insert_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "SQL Bulk Insert";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(304, 129));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.Database.SQLBulkInsert, new Point(10, 15));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(304, 129));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox SQL Server Tool Onto DesignSurface")]
        public void Drag_Toolbox_SQL_Server_Tool_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "SQL Server";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(304, 127));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.Database.SQLServer, new Point(10, -7));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(304, 127));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Switch Onto DesignSurface")]
        public void Drag_Toolbox_Switch_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Switch";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(303, 126));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.FlowTools.Switch, new Point(22, 30));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(303, 126));
        }

        [When(@"I Drag Toolbox System Information Onto DesignSurface")]
        public void Drag_Toolbox_System_Information_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Sys Info";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(304, 129));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.UtilityTools.SysInfo, new Point(8, 12));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(304, 129));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Unique Records Onto DesignSurface")]
        public void Drag_Toolbox_Unique_Records_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Unique";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(304, 133));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.RecordsetTools.UniqueRecords, new Point(43, 6));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(304, 133));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Unzip Onto DesignSurface")]
        public void Drag_Toolbox_Unzip_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Unzip";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(306, 128));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.FileAndFTP.UnZip, new Point(15, 15));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(306, 128));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Web Request Onto DesignSurface")]
        public void Drag_Toolbox_Web_Request_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Web Request";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(308, 128));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.UtilityTools.WebRequest, new Point(14, 3));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(308, 128));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Write File Onto DesignSurface")]
        public void Drag_Toolbox_Write_File_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Write File";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(306, 132));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.FileAndFTP.WriteFile, new Point(10, 18));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(306, 132));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox XPath Onto DesignSurface")]
        public void Drag_Toolbox_XPath_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "XPath";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(307, 123));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.UtilityTools.XPath, new Point(12, -13));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(307, 123));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Drag Toolbox Zip Onto DesignSurface")]
        public void Drag_Toolbox_Zip_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "Zip";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(306, 131));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.FileAndFTP.Zip, new Point(16, 4));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(306, 131));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
        }

        [When(@"I Enter Duplicate workflow name")]
        public void Enter_Duplicate_workflow_name(string name)
        {
            DialogsUIMap.SaveDialogWindow.ServiceNameTextBox.Text = name;
        }

        [When(@"I Enter InputDebug value")]
        public void Enter_InputDebug_value()
        {
            Assert.IsTrue(UIMap.MainStudioWindow.DebugInputDialog.TabItemsTabList.InputDataTab.InputsTable.Row1.Exists, "InputData row does not exist.");
            Assert.IsTrue(UIMap.MainStudioWindow.DebugInputDialog.TabItemsTabList.InputDataTab.InputsTable.Row1.InputValueCell.InputValueComboboxl.InputValueText.Exists, "InputData row does not exist.");
            UIMap.MainStudioWindow.DebugInputDialog.TabItemsTabList.InputDataTab.InputsTable.Row1.InputValueCell.InputValueComboboxl.InputValueText.Text = "100";
        }

        [When(@"I Enter Public As Windows Group")]
        public void Enter_Public_As_Windows_Group()
        {
            UIMap.FindWindowsGroupTextbox(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1).Text = "Public";
        }

        [When(@"I Enter Sharepoint Server Path From OnCopyFile Tool")]
        public void Enter_Sharepoint_Server_Path_From_OnCopyFile_Tool()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCopyFile.SmallView.FromDirectoryComboBox.TextEdit.Text = "clocks.dat";
        }

        [When(@"I Enter Sharepoint Server Path From OnMoveFile Tool")]
        public void Enter_Sharepoint_Server_Path_From_OnMoveFile_Tool()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointMoveFile.SmallView.FromDirectoryComboBox.TextEdit.Text = "clocks.dat";
        }

        [When(@"I Enter Sharepoint Server Path From OnUpload Tool")]
        public void Enter_Sharepoint_Server_Path_From_OnUpload_Tool()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUploadFile.SmallView.LocalPathFromIntellisenseCombobox.Textbox.Text = "clocks.dat";
        }

        [When(@"I Enter Sharepoint Server Path To OnCopyFile Tool")]
        public void Enter_Sharepoint_Server_Path_To_OnCopyFile_Tool()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCopyFile.SmallView.PathDirectoryComboBox.TextEdit.Text = "TestFolder/clocks.dat";
        }

        [When(@"I Enter Sharepoint Server Path To OnMoveFile Tool")]
        public void Enter_Sharepoint_Server_Path_To_OnMoveFile_Tool()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointMoveFile.SmallView.PathDirectoryComboBox.TextEdit.Text = "TestFolder/clocks.dat";
        }

        [When(@"I Enter Sharepoint Server Path To OnUpload Tool")]
        public void Enter_Sharepoint_Server_Path_To_OnUpload_Tool()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUploadFile.SmallView.ServerPathToIntellisenseCombobox.Textbox.Text = "TestFolder/clocks.dat";
        }

        [When(@"I Enter Sharepoint ServerSource ServerName")]
        public void Enter_Sharepoint_ServerSource_ServerName()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SharepointServerSourceTab.SharepointServerSourceView.SharepointView.ServerNameEdit.Text = "http://rsaklfsvrsharep/";
        }

        [When(@"I Enter Sharepoint ServerSource User Credentials")]
        public void Enter_Sharepoint_ServerSource_User_Credentials()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SharepointServerSourceTab.SharepointServerSourceView.SharepointView.UserNameTextBox.Text = "IntegrationTester";
            UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SharepointServerSourceTab.SharepointServerSourceView.SharepointView.PasswordTextBox.Text = "I73573r0";
        }

        [When(@"I Enter SomeData Into Base Convert Large View Row1 Value Textbox")]
        public void Enter_SomeData_Into_BaseConvertTool_Row1ValueTextbox()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.BaseConvert.LargeView.DataGrid.Row1.Cell.Listbox.ValueTextbox.Text = "SomeData";
        }

        [When(@"I Enter SomeVariable Into Calculate Large View Function Textbox")]
        public void Enter_SomeVariable_Into_CalculateTool_FXCombobox()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Calculate.LargeView.FxCombobox.TextEdit.Text = "[[SomeVariable]]";
        }

        [When(@"I Enter Text Into Assign Large View Row1 Variable Textbox As SomeInvalidVariableName")]
        public void Enter_Text_Into_Assign_Large_View_Row1_Variable_Textbox_As_SomeInvalidVariableName()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.LargeView.DataGrid.Row1.VariableCell.IntellisenseCombobox.Textbox.Text = "[[Some$Invalid%Variable]]";
            Assert.AreEqual("[[Some$Invalid%Variable]]", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.LargeView.DataGrid.Row1.VariableCell.IntellisenseCombobox.Textbox.Text, "Multiassign small view row 1 variable textbox text does not equal \"[[Some$Invalid" +
                    "%Variable]]\".");
        }

        [Given(@"I Enter Text Into Assign Large View Row1 Variable Textbox As SomeVariable")]
        [When(@"I Enter Text Into Assign Large View Row1 Variable Textbox As SomeVariable")]
        [Then(@"I Enter Text Into Assign Large View Row1 Variable Textbox As SomeVariable")]
        public void Enter_Text_Into_Assign_Large_View_Row1_Variable_Textbox_As_SomeVariable()
        {
            Keyboard.SendKeys(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.LargeView.DataGrid.Row1.VariableCell.IntellisenseCombobox.Textbox, "{Home}", ModifierKeys.Shift);
            Keyboard.SendKeys(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.LargeView.DataGrid.Row1.VariableCell.IntellisenseCombobox.Textbox, "[[Some{Down}{Enter}Variable]]", ModifierKeys.None);
            Assert.AreEqual("[[SomeVariable]]", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.LargeView.DataGrid.Row1.VariableCell.IntellisenseCombobox.Textbox.Text, "Assign large view row1 variable textbox text does not equal \"[[SomeVariable]]\" af" +
                    "ter selecting it from the intellisense using the keyboard.");
        }

        [When(@"I Enter Text Into Assign Large View Row1 Variable Textbox As SomeVariable On Unpinned Tab")]
        public void Enter_Text_Into_Assign_Large_View_Row1_Variable_Textbox_As_SomeVariable_On_Unpinned_Tab()
        {
            Keyboard.SendKeys(UIMap.MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.LargeView.DataGrid.Row1.VariableCell.IntellisenseCombobox.Textbox, "[[Some{Down}{Enter}Variable]]", ModifierKeys.None);
            Assert.AreEqual("[[SomeVariable]]", UIMap.MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.LargeView.DataGrid.Row1.VariableCell.IntellisenseCombobox.Textbox.Text, "Assign large view row1 variable textbox text does not equal \"[[SomeVariable]]\" on" +
                    " unpinned tab after selecting it from the intellisense using the keyboard.");
        }

        [When(@"I Enter Text Into Assign Small View Row1 Value Textbox As SomeVariable Using Click Intellisense Suggestion")]
        public void Enter_Text_Into_Assign_Small_View_Row1_Value_Textbox_As_SomeVariable_Using_Click_Intellisense_Suggestion()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row1.VariableCell.IntellisenseCombobox.Textbox.Text = "[[";
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row1.VariableCell.IntellisenseCombobox.ListItem, new Point(39, 10));
            Assert.AreEqual("[[SomeVariable]]", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row1.VariableCell.IntellisenseCombobox.Textbox.Text, "Multiassign small view row 1 variable textbox text does not equal \"[[SomeVariable" +
                    "]]\".");
        }

        [When(@"I Enter Text Into Assign Small View Row1 Value Textbox As SomeVariable UsingIntellisense")]
        public void Enter_Text_Into_Assign_Small_View_Row1_Value_Textbox_As_SomeVariable_UsingIntellisense()
        {
            Keyboard.SendKeys(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row1.VariableCell.IntellisenseCombobox.Textbox, "[[{Down}{Enter}", ModifierKeys.None);
            Assert.AreEqual("[[SomeVariable]]", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row1.VariableCell.IntellisenseCombobox.Textbox.Text, "Multiassign small view row 1 variable textbox text does not equal \"[[SomeVariable" +
                    "]]\".");
        }

        [When(@"I Enter Vaiablelist Items")]
        public void Enter_Vaiablelist_Items()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem1.ScrollViewerPane.NameTextbox, new Point(62, 3));
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem1.ScrollViewerPane.NameTextbox.Text = "varableA";
            Keyboard.SendKeys(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem1.ScrollViewerPane.NameTextbox, "{CapsLock}", ModifierKeys.None);
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem2.ScrollViewerPane.NameTextbox, new Point(82, 2));
            Keyboard.SendKeys(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem2.ScrollViewerPane.NameTextbox, "{CapsLock}", ModifierKeys.None);
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem2.ScrollViewerPane.NameTextbox.Text = "variableB";
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem3.ScrollViewerPane.NameTextbox, new Point(84, 2));
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem3.ScrollViewerPane.NameTextbox.Text = "VariableC";
            Keyboard.SendKeys(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem3.ScrollViewerPane.NameTextbox, "{CapsLock}", ModifierKeys.None);
        }

        [When(@"I Filter variables")]
        public void Filter_variables()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.SearchTextbox.FilterText.Exists, "Variable filter textbox does not exist");
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.SearchTextbox, new Point(89, 7));
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.SearchTextbox.Text = "Other";
        }

        [When(@"I Open AggregateCalculate Tool large view")]
        public void Open_AggregateCalculateTool_Largeview()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.AggregateCalculat, new Point(136, 13));
        }

        [When(@"I Open Assign Tool Large View")]
        public void Open_AssignTool_LargeView()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign, new Point(145, 5));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.Exists, "Assign tool large view on the design surface does not exist");
        }

        [When(@"I Add Variables The Variable List")]
        [Given(@"I Add Variables The Variable List")]
        [Then(@"I Add Variables The Variable List")]
        public void Add_Variables(string variables)
        {
            var strings = variables.Split(',');
            if (!string.IsNullOrEmpty(strings[0]))
                MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem1.ScrollViewerPane.NameTextbox.Text = strings?[0];
            if (!string.IsNullOrEmpty(strings[1]))
                MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem2.ScrollViewerPane.NameTextbox.Text = strings?[1];
            if (!string.IsNullOrEmpty(strings[2]))
                MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem3.ScrollViewerPane.NameTextbox.Text = strings?[2];
            if (!string.IsNullOrEmpty(strings[3]))
                MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.VariableTreeItem.TreeItem4.ScrollViewerPane.NameTextbox.Text = strings?[3];
        }

        [When(@"I Add Recordset The Recordset List")]
        [Given(@"I Add Recordset The Recordset List")]
        [Then(@"I Add Recordset The Recordset List")]
        public void Add_Recordsets(string variables)
        {
            var strings = variables.Split(',');
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.RecordsetTreeItem.TreeItem1.ScrollViewerPane.NameTextbox.Text = strings?[0];
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.RecordsetTreeItem.TreeItem2.ScrollViewerPane.NameTextbox.Text = strings?[1];
        }


        [When(@"I Add Recordset Fields The Recordset List")]
        [Given(@"I Add Recordset Fields The Recordset List")]
        [Then(@"I Add Recordset Fields The Recordset List")]
        public void Add_Recordsets_Fields(string variables)
        {
            var strings = variables.Split(',');
            if (!string.IsNullOrEmpty(strings[0]))
                MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.RecordsetTreeItem.TreeItem2.Field1.ScrollViewerPane.NameTextbox.Text = strings?[0];
            if (!string.IsNullOrEmpty(strings[1]))
                MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.RecordsetTreeItem.TreeItem2.Field2.ScrollViewerPane.NameTextbox.Text = strings?[1];
            if (!string.IsNullOrEmpty(strings[0]))
                MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.RecordsetTreeItem.TreeItem1.Field1.ScrollViewerPane.NameTextbox.Text = strings?[0];
            if (!string.IsNullOrEmpty(strings[1]))
                MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.RecordsetTreeItem.TreeItem1.Field2.ScrollViewerPane.NameTextbox.Text = strings?[1];
            if (!string.IsNullOrEmpty(strings[2]))
                MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.RecordsetTreeItem.TreeItem1.Field3.ScrollViewerPane.NameTextbox.Text = strings?[2];
            if (!string.IsNullOrEmpty(strings[3]))
                MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.VariableTree.RecordsetTreeItem.TreeItem1.Field4.ScrollViewerPane.NameTextbox.Text = strings?[3];
        }

        [Given(@"I Sort Variable List")]
        [When(@"I Sort Variable List")]
        [Then(@"I Sort Variable List")]
        public void Click_Sort_Variable_List()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.Variables.DatalistView.Sort);
        }

        [When(@"I Open Assign Tool On Unpinned Tab Large View")]
        public void Open_Assign_Tool_On_Unpinned_Tab_Large_View()
        {
            Mouse.DoubleClick(UIMap.MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign, new Point(145, 5));
            Assert.IsTrue(UIMap.MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.Exists, "Assign tool large view on the unpinned tab design surface does not exist");
            Assert.IsTrue(UIMap.MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.LargeView.DataGrid.Row1.VariableCell.IntellisenseCombobox.Exists, "Assign large view row 1 variable textbox does not exist after openning large view" +
                    " with a double click on an unpinned tab.");
        }

        [When(@"I Open AssignObject Large Tool")]
        public void Open_AssignObject_Large_Tool()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.AssignObject, new Point(159, 11));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.AssignObject.DoneButton.Exists, "Done button does not exist after dragging Assign Object tool on to the workflow surface");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.AssignObject.OpenQuickVariableInput.Exists, "OpenQuickVariableInput button does not exist after dragging Assign Object tool on to the workflow surface");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.AssignObject.LargeView.DataGrid.Row1.Exists, "Row1 does not exist after dragging Assign Object tool on to the workflow surface");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.AssignObject.LargeView.OnError.OnErrorGroup.Exists, "OnErrorGroup does not exist after dragging Assign Object tool on to the workflow surface");
        }

        [When(@"I Open Base Conversion Tool Large View")]
        public void Open_BaseConvertTool_LargeView()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.BaseConvert, new Point(160, 15));
        }

        [Given(@"I Click Base Convert Large View Done Button")]
        [When(@"I Click Base Convert Large View Done Button")]
        [Then(@"I Click Base Convert Large View Done Button")]
        public void Click_BaseConvertTool_LargeView_DoneButton()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.BaseConvert.DoneButton, new Point(36, 11));
        }

        [When(@"I Open Calculate Tool Large View")]
        public void Open_Calculate_Tool_Large_View()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Calculate, new Point(105, 7));
        }

        [When(@"I Open Case Conversion Tool Large View")]
        public void Open_CaseConvertTool_LargeView()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.CaseConvert, new Point(136, 13));
        }

        [When(@"I Open Context Menu On Design Surface")]
        public void Open_Context_Menu_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, MouseButtons.Right, ModifierKeys.None, new Point(304, 286));
        }

        [When(@"I Open Create JSON Large View")]
        public void Open_CreateJSON_LargeView()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.CreateJson, new Point(124, 9));
        }

        [When(@"I Open Copy Tool Large View")]
        public void Open_CopyTool_LargeView()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathCopy, new Point(144, 11));
        }

        [When(@"I Open Create Tool Large View")]
        public void Open_CreateTool_LargeView()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathCreate, new Point(118, 13));
        }

        [When(@"I Open DeleteWeb Tool Large View")]
        public void Open_DeleteWebTool_LargeView()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete, new Point(145, 5));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete.LargeView.Exists, "Web delete large view does not exist on the design surface");
        }

        [When(@"I Open Delete Tool Large View")]
        public void Open_DeleteTool_LargeView()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathDelete, new Point(118, 13));
        }

        [When(@"I Open Move Tool Large View")]
        public void Open_MoveTool_LargeView()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathMove, new Point(125, 6));
        }

        [When(@"I Open Read File Tool Large View")]
        public void Open_ReadFileTool_LargeView()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FileRead, new Point(120, 5));
        }

        [When(@"I Open Read Folder Tool Large View")]
        public void Open_ReadFolderTool_LargeView()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FolderRead, new Point(138, 14));
        }

        [When(@"I Open Rename Tool Large View")]
        public void Open_RenameTool_LargeView()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathRename, new Point(159, 11));
        }

        [Given(@"I Open Unzip Tool Large View")]
        [When(@"I Open Unzip Tool Large View")]
        [Then(@"I Open Unzip Tool Large View")]
        public void Open_UnzipTool_LargeView()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.UnZip, new Point(102, 14));
        }

        [Given(@"I Open Write File Tool Large View")]
        [When(@"I Open Write File Tool Large View")]
        [Then(@"I Open Write File Tool Large View")]
        public void Open_WriteFileTool_LargeView()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FileWrite, new Point(149, 13));
        }

        [Given(@"I Open Zip Tool Large View")]
        [When(@"I Open Zip Tool Large View")]
        [Then(@"I Open Zip Tool Large View")]
        public void Open_ZipTool_LargeView()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Zip, new Point(124, 12));
        }

        [When(@"I Open Data Merge Large View")]
        public void Open_DataMerge_LargeView()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataMerge, new Point(185, 9));
        }
        public void Click_DataMerge_DoneButton()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataMerge.DoneButton);
        }

        [When(@"I Open Data Split Large View")]
        public void Open_DataSplit_LargeView()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataSplit, new Point(203, 10));
        }

        [When(@"I Open DateTime LargeView")]
        public void Open_DateTime_LargeView()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DateTime, new Point(145, 7));
        }

        [When(@"I Open DateTimeDiff LargeView")]
        public void Open_DateTimeDiff_LargeView()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DateTimeDifference, new Point(145, 7));
        }

        [When(@"I Open Decision Large View")]
        public void Open_Decision_LargeView()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Decision.DrawHighlight();
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Decision);
            Assert.IsTrue(DialogsUIMap.DecisionOrSwitchDialog.Exists, "Decision Dialog does not exist after opening large Decision view");
        }

        [When(@"I Open ForEach Large View")]
        public void Open_ForEach_LargeView()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ForEach, new Point(131, 14));
        }

        [When(@"I Open SelectAndApply Large View")]
        public void Open_SelectAndApply_LargeView()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SelectAndApply, new Point(129, 10));
        }

        [When(@"I Open CountRecords Large View")]
        public void Open_CountRecords_LargeView()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.CountRecordset, new Point(130, 11));
        }

        [When(@"I Open DeleteRecords Large View")]
        public void Open_DeleteRecords_LargeView()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DeleteRecord, new Point(133, 9));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DeleteRecord.DoneButton.Exists, "Done button does not exist after opening Delete Record large view");
        }

        [When(@"I Open Find Record Index Tool Large View")]
        public void Open_FindRecordIndexTool_LargeView()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FindRecordsIndex, new Point(172, 5));
        }

        [When(@"I Open Length Tool Large View")]
        public void Open_LengthTool_LargeView()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Length, new Point(136, 13));
        }

        [Given(@"I Open SortRecords Large View")]
        [When(@"I Open SortRecords Large View")]
        [Then(@"I Open SortRecords Large View")]
        public void Open_SortRecords_LargeView()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SortRecords, new Point(114, 13));
        }

        [Given(@"I Open UniqueRecords Large View")]
        [When(@"I Open UniqueRecords Large View")]
        [Then(@"I Open UniqueRecords Large View")]
        public void Open_UniqueRecords_LargeView()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Unique, new Point(134, 10));
        }

        [When(@"I Collapse GetWebTool Large View to Small View With Double Click")]
        public void WebGetTool_ChangeView_With_DoubleClick()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebGet, new Point(201, 9));
        }

        [When(@"I Collapse PostWebTool Large View to Small View With Double Click")]
        public void WebPostTool_ChangeView_With_DoubleClick()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPost, new Point(201, 9));
        }

        [When(@"I Collapse PutWebTool Large View to Small View With Double Click")]
        public void WebPutTool_ChangeView_With_DoubleClick()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPut, new Point(201, 9));
        }

        public void WebDeleteTool_ChangeView_With_DoubleClick()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete, new Point(201, 9));
        }

        [When(@"I Select Test Source From GET Web Large View Source Combobox")]
        public void Select_GETWebTool_Source_From_SourceCombobox()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebGet.LargeView.SourcesComboBox, new Point(175, 9));
            Mouse.Click(UIMap.MainStudioWindow.WebServerSourceComboboxListItem2, new Point(163, 17));
        }

        [When(@"I Select Test Source From POST Web Large View Source Combobox")]
        public void Select_POSTWebTool_Source_From_SourceCombobox()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPost.LargeView.SourcesComboBox, new Point(175, 9));
            Mouse.Click(UIMap.MainStudioWindow.WebServerSourceComboboxListItem3, new Point(163, 17));
        }

        [When(@"I Select Test Source From PUT Web Large View Source Combobox")]
        public void Select_PUTWebTool_Source_From_SourceCombobox()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPut.LargeView.SourcesComboBox, new Point(175, 9));
            Mouse.Click(UIMap.MainStudioWindow.WebServerSourceComboboxListItem4, new Point(163, 17));
        }

        [When(@"I Select Test Source From DELETE Web Large View Source Combobox")]
        public void Select_DELETEWebTool_Source_From_SourceCombobox()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete.LargeView.SourcesComboBox, new Point(175, 9));
            Mouse.Click(UIMap.MainStudioWindow.WebServerSourceComboboxListItem1, new Point(163, 17));
        }

        [Given(@"I Click GET Web Large View Generate Outputs")]
        [When(@"I Click GET Web Large View Generate Outputs")]
        [Then(@"I Click GET Web Large View Generate Outputs")]
        public void Click_GETWebTool_GenerateOutputsButton()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebGet.LargeView.GenerateOutputsButton, new Point(7, 7));
        }

        [Given(@"I Click POST Web Large View Generate Outputs")]
        [When(@"I Click POST Web Large View Generate Outputs")]
        [Then(@"I Click POST Web Large View Generate Outputs")]
        public void Click_POSTWebTool_GenerateOutputsButton()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPost.LargeView.GenerateOutputsButton, new Point(7, 7));
        }

        [Given(@"I Click PUT Web Large View Generate Outputs")]
        [When(@"I Click PUT Web Large View Generate Outputs")]
        [Then(@"I Click PUT Web Large View Generate Outputs")]
        public void Click_PUTWebTool_GenerateOutputsButton()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPut.LargeView.GenerateOutputsButton, new Point(7, 7));
        }

        [Given(@"I Click DELETE Web Large View Generate Outputs")]
        [When(@"I Click DELETE Web Large View Generate Outputs")]
        [Then(@"I Click DELETE Web Large View Generate Outputs")]
        public void Click_DELETEWebTool_GenerateOutputsButton()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete.LargeView.GenerateOutputsButton, new Point(7, 7));
        }

        [Given(@"I Click GET Web Large View Done Button")]
        [When(@"I Click GET Web Large View Done Button")]
        [Then(@"I Click GET Web Large View Done Button")]
        public void Click_GETWebTool_Done_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebGet.DoneButton, new Point(33, 11));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebGet.SmallView.Exists, "Web GET small view does not exist after clicking large view done button.");
        }

        [Given(@"I Click POST Web Large View Done Button")]
        [When(@"I Click POST Web Large View Done Button")]
        [Then(@"I Click POST Web Large View Done Button")]
        public void Click_POSTWebTool_Done_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPost.DoneButton, new Point(33, 11));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPost.SmallView.Exists, "Web POST small view does not exist after clicking large view done button.");
        }

        [Given(@"I Click PUT Web Large View Done Button")]
        [When(@"I Click PUT Web Large View Done Button")]
        [Then(@"I Click PUT Web Large View Done Button")]
        public void Click_PUTWebTool_Done_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPut.DoneButton, new Point(33, 11));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPut.SmallView.Exists, "Web PUT small view does not exist after clicking large view done button.");
        }

        [Given(@"I Click DELETE Web Large View Done Button")]
        [When(@"I Click DELETE Web Large View Done Button")]
        [Then(@"I Click DELETE Web Large View Done Button")]
        public void Click_DELETEWebTool_Done_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete.DoneButton, new Point(33, 11));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete.SmallView.Exists, "Web DELETE small view does not exist after clicking large view done button.");
        }

        [Given(@"I Click GET Web Large View Test Inputs Button")]
        [When(@"I Click GET Web Large View Test Inputs Button")]
        [Then(@"I Click GET Web Large View Test Inputs Button")]
        public void Click_GETWebTool_TestInputsButton()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebGet.LargeView.TestButton, new Point(21, 11));
        }

        [Given(@"I Click POST Web Large View Test Inputs Button")]
        [When(@"I Click POST Web Large View Test Inputs Button")]
        [Then(@"I Click POST Web Large View Test Inputs Button")]
        public void Click_POSTWebTool_TestInputsButton()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPost.LargeView.TestButton, new Point(21, 11));
        }

        [Given(@"I Click PUT Web Large View Test Inputs Button")]
        [When(@"I Click PUT Web Large View Test Inputs Button")]
        [Then(@"I Click PUT Web Large View Test Inputs Button")]
        public void Click_PUTWebTool_TestInputsButton()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPut.LargeView.TestButton, new Point(21, 11));
        }

        [Given(@"I Click DELETE Web Large View Test Inputs Button")]
        [When(@"I Click DELETE Web Large View Test Inputs Button")]
        [Then(@"I Click DELETE Web Large View Test Inputs Button")]
        public void Click_DELETEWebTool_TestInputsButton()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete.LargeView.TestButton, new Point(21, 11));
        }

        [Given(@"I Enter invalid data on the DELETE Web Large View")]
        [When(@"I Enter invalid data on the DELETE Web Large View")]
        [Then(@"I Enter invalid data on the DELETE Web Large View")]
        public void Enter_Invalid_Data_DELETEWebTool()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext
                .WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart
                .WebDelete.LargeView.Table.ItemRow1.HeaderCell.HeaderComboBox.TextEdit.Text = "stupidData";
        }

        [Given(@"I Click GET Web Tool Outputs Done Button")]
        [When(@"I Click GET Web Tool Outputs Done Button")]
        [Then(@"I Click GET Web Tool Outputs Done Button")]
        public void Click_GETWebTool_Outputs_DoneButton()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebGet.LargeView.DoneButton, new Point(35, 6));
        }

        [Given(@"I Click POST Web Tool Outputs Done Button")]
        [When(@"I Click POST Web Tool Outputs Done Button")]
        [Then(@"I Click POST Web Tool Outputs Done Button")]
        public void Click_POSTWebTool_Outputs_DoneButton()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPost.LargeView.DoneButton, new Point(35, 6));
        }

        [Given(@"I Click PUT Web Tool Outputs Done Button")]
        [When(@"I Click PUT Web Tool Outputs Done Button")]
        [Then(@"I Click PUT Web Tool Outputs Done Button")]
        public void Click_PUTWebTool_Outputs_DoneButton()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPut.LargeView.DoneButton, new Point(35, 6));
        }

        [Given(@"I Click DELETE Web Tool Outputs Done Button")]
        [When(@"I Click DELETE Web Tool Outputs Done Button")]
        [Then(@"I Click DELETE Web Tool Outputs Done Button")]
        public void Click_DELETEWebTool_Outputs_DoneButton()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete.LargeView.DoneButton, new Point(35, 6));
        }

        [When(@"I Double Click DotNetDLL Tool to Change View")]
        public void DotNetDLLTool_ChangeView_With_DoubleClick()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll, new Point(238, 16));
        }

        [When(@"I Double Click ComDLL Tool to Change View")]
        public void ComDLLTool_ChangeView_With_DoubleClick()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ComDll, new Point(238, 16));
        }

        [When(@"I Double Click WCFService Tool to Change View")]
        public void WCFServiceTool_ChangeView_With_DoubleClick()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WcfService, new Point(238, 16));
        }

        [When(@"I Double Click DropboxDelete Tool to Change View")]
        public void DropboxDeleteTool_ChangeView_With_DoubleClick()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DropboxDelete, new Point(174, 12));
        }

        [When(@"I Double Click DropboxListContents Tool to Change View")]
        public void DropboxListContentsTool_ChangeView_With_DoubleClick()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DropboxFileList, new Point(166, 9));
        }

        [When(@"I Double Click DropboxUpload Tool to Change View")]
        public void DropboxUploadTool_ChangeView_With_DoubleClick()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DropboxUpload, new Point(151, 8));
        }

        [When(@"I Double Click DropboxDownload Tool to Change View")]
        public void DropboxDownloadTool_ChangeView_With_DoubleClick()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DropboxDownload, new Point(174, 14));
        }

        [When(@"I Double Click ExchangeSend Tool to Change View")]
        public void ExchangeSendTool_ChangeView_With_DoubleClick()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ExchangeEmail, new Point(168, 11));
        }

        [When(@"I Open CMD Line Tool Large View")]
        public void Open_CMDLineTool_LargeView()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ExecuteCommandLine, new Point(174, 10));
        }

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

        [When(@"I Open Ruby Large View")]
        public void Open_Ruby_LargeView()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Ruby, new Point(116, 12));
        }

        [Given(@"I Right Click On The Folder Count")]
        [When(@"I Right Click On The Folder Count")]
        [Then(@"I Right Click On The Folder Count")]
        public void Right_Click_On_The_Folder_Count()
        {
            Mouse.Click(ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem, MouseButtons.Right, ModifierKeys.None, new Point(211, 8));
        }

        [When(@"I Open Find Index Tool Large View")]
        public void Open_FindIndexTool_LargeView()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FindIndex, new Point(147, 11));
        }

        [When(@"I Open Json Tool Large View")]
        public void Open_Json_Tool_Large_View()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.CreateJson, new Point(158, 13));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.CreateJson.Exists, "JSON tool large view on the design surface does not exist");
        }

        [When(@"I Open FormatNumber Tool Large View")]
        public void Open_FormatNumberToll_LargeView()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FormatNumber, new Point(145, 5));
        }

        [When(@"I Double Click MySqlDatabase Tool to Change View")]
        public void MySqlDatabaseTool_ChangeView_With_DoubleClick()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MySqlDatabase, new Point(238, 15));
        }

        [When(@"I Double Click ODBCDatabase Tool to Change View")]
        public void ODBCDatabaseTool_ChangeView_With_DoubleClick()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ODBCDatabaseActivCustom, new Point(145, 5));
        }

        [When(@"I Double Click OracleDatabase Tool to Change View")]
        public void OracleDatabaseTool_ChangeView_With_DoubleClick()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.OracleDatabaseActCustom, new Point(145, 5));
        }

        [When(@"I Double Click PostgreSqlDatabase Tool to Change View")]
        public void PostgreSqlDatabaseTool_ChangeView_With_DoubleClick()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PostgreSqlActivitCustom, new Point(173, 14));
        }

        [When(@"I Double Click SqlServerDatabase Tool to Change View")]
        public void SqlServerDatabaseTool_ChangeView_With_DoubleClick()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlServerDatabase, new Point(145, 5));
        }

        [Given(@"I Double Click SqlBulkInsert Tool to Change View")]
        [When(@"I Double Click SqlBulkInsert Tool to Change View")]
        [Then(@"I Double Click SqlBulkInsert Tool to Change View")]
        public void SqlBulkInsertTool_ChangeView_With_DoubleClick()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlBulkInsert, new Point(157, 6));
        }

        [When(@"I Open RabbitMqConsume LargeView")]
        public void Open_RabbitMqConsume_LargeView()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.RabbitMQConsume, new Point(145, 7));
        }

        [When(@"I Open RabbitMqPublish LargeView")]
        public void Open_RabbitMqPublish_LargeView()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.RabbitMQPublish, new Point(145, 7));
        }

        [When(@"I Open Random Large Tool")]
        public void Open_RandomTool_LargeView()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Random, new Point(145, 7));
        }

        [When(@"I Open Replace Tool Large View")]
        public void Open_ReplaceTool_LargeView()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Replace, new Point(159, 11));
        }

        [When(@"I Double Click Sequence Tool to Change View")]
        public void SequenceTool_ChangeView_With_DoubleClick()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Sequence, new Point(139, 12));
        }

        [When(@"I Open Sharepoint Copy Tool Large View")]
        [Then(@"I Open Sharepoint Copy Tool Large View")]
        [Given(@"I Open Sharepoint Copy Tool Large View")]
        public void Open_SharepointCopyTool_LargeView()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCopyFile, new Point(230, 11));
        }

        [When(@"I Open Sharepoint Create Tool Large View")]
        public void Open_SharepointCreateTool_LargeView()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCreateListItem, new Point(195, 11));
        }

        [When(@"I Open Sharepoint Delete List Item Tool Large View")]
        public void Open_SharepointDeleteListItemTool_LargeView()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointDeleteListItem, new Point(218, 11));
        }

        [When(@"I Open Sharepoint Delete File Tool Large View")]
        public void Open_SharepointDeleteFileTool_LargeView()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointDeleteFile, new Point(218, 11));
        }

        [Given(@"I Open Sharepoint Download File Tool Large View")]
        [When(@"I Open Sharepoint Download File Tool Large View")]
        [Then(@"I Open Sharepoint Download File Tool Large View")]
        public void Open_SharepointDownloadFileTool_LargeView()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointDownloadFile, new Point(185, 9));
        }

        [Given(@"I Open Sharepoint MoveFile Tool Large View")]
        [When(@"I Open Sharepoint MoveFile Tool Large View")]
        [Then(@"I Open Sharepoint MoveFile Tool Large View")]
        public void Open_SharepointMoveFileTool_LargeView()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointMoveFile, new Point(230, 11));
        }

        [Given(@"I Open Sharepoint Read Folder Tool Large View")]
        [When(@"I Open Sharepoint Read Folder Tool Large View")]
        [Then(@"I Open Sharepoint Read Folder Tool Large View")]
        public void Open_SharepointReadFolderTool_LargeView()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointReadFolder, new Point(195, 7));
        }

        [Given(@"I Open Sharepoint Read List Item Tool Large View")]
        [When(@"I Open Sharepoint Read List Item Tool Large View")]
        [Then(@"I Open Sharepoint Read List Item Tool Large View")]
        public void Open_SharepointReadListItemTool_LargeView()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointReadListItem, new Point(195, 7));
        }

        [Given(@"I Open Sharepoint Update List Item Tool Large View")]
        [When(@"I Open Sharepoint Update List Item Tool Large View")]
        [Then(@"I Open Sharepoint Update List Item Tool Large View")]
        public void Open_SharepointUpdateListItemTool_LargeView()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUpdateListItem, new Point(230, 11));
        }

        [Given(@"I Open Sharepoint Upload Tool Large View")]
        [When(@"I Open Sharepoint Upload Tool Large View")]
        [Then(@"I Open Sharepoint Upload Tool Large View")]
        public void Open_SharepointUploadTool_LargeView()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUploadFile, new Point(230, 11));
        }

        [Given(@"I Open SMTP Email Tool Large View")]
        [When(@"I Open SMTP Email Tool Large View")]
        [Then(@"I Open SMTP Email Tool Large View")]
        public void Open_SMTPSendTool_LargeView()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SMTPEmail, new Point(168, 11));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SMTPEmail.LargeViewContent.Exists, "Email Tool large view does not exist on the design surface");
        }

        [Given(@"I Open System Information Tool Large View")]
        [When(@"I Open System Information Tool Large View")]
        [Then(@"I Open System Information Tool Large View")]
        public void Open_SystemInformationTool_LargeView()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.GatherSystemInfo, new Point(145, 5));
        }

        [Given(@"I Open WebRequest LargeView")]
        [When(@"I Open WebRequest LargeView")]
        [Then(@"I Open WebRequest LargeView")]
        public void Open_WebRequestTool_LargeView()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebRequest, new Point(126, 13));
        }

        [Given(@"I Open Xpath Tool Large View")]
        [When(@"I Open Xpath Tool Large View")]
        [Then(@"I Open Xpath Tool Large View")]
        public void Open_XpathTool_LargeView()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.XPath, new Point(113, 12));
        }

        [Given(@"I Open Assign Tool QVI View")]
        [When(@"I Open Assign Tool QVI View")]
        [Then(@"I Open Assign Tool QVI View")]
        public void Open_AssignTool_QVIView()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.OpenQuickVariableInpToggleButton.Pressed = true;
        }

        [Given(@"I Open AssignObject QVI View")]
        [When(@"I Open AssignObject QVI View")]
        [Then(@"I Open AssignObject QVI View")]
        public void Open_AssignObject_QVIView()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.AssignObject.OpenQuickVariableInput.Pressed = true;
        }

        [Given(@"I Open Base Conversion Tool QVI View")]
        [When(@"I Open Base Conversion Tool QVI View")]
        [Then(@"I Open Base Conversion Tool QVI View")]
        public void Open_BaseConvertTool_QVIView()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.BaseConvert.OpenQuickVariableInpToggleButton.Pressed = true;
        }

        [Given(@"I Open Case Conversion Tool QVI View")]
        [When(@"I Open Case Conversion Tool QVI View")]
        [Then(@"I Open Case Conversion Tool QVI View")]
        public void Open_CaseConvertTool_QVIView()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.CaseConvert.OpenQuickVariableInpToggleButton.Pressed = true;
        }

        [Given(@"I Open Data Merge Tool QVI View")]
        [When(@"I Open Data Merge Tool QVI View")]
        [Then(@"I Open Data Merge Tool QVI View")]
        public void Open_DataMergeTool_QVIView()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataMerge.QVIToggleButton.Pressed = true;
        }

        [Given(@"I Open Data Split Tool QVI View")]
        [When(@"I Open Data Split Tool QVI View")]
        [Then(@"I Open Data Split Tool QVI View")]
        public void Open_DataSplitTool_QVIView()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataSplit.OpenQuickVariableInpToggleButton.Pressed = true;
        }

        [Given(@"I Open Json Tool QVI View")]
        [When(@"I Open Json Tool QVI View")]
        [Then(@"I Open Json Tool QVI View")]
        public void Open_JSONTool_QVIView()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.CreateJson.OpenQuickVariableInpToggleButton.Pressed = true;
        }

        [Given(@"I Open SQL Bulk Insert Tool QVI View")]
        [When(@"I Open SQL Bulk Insert Tool QVI View")]
        [Then(@"I Open SQL Bulk Insert Tool QVI View")]
        public void Open_SQLBulkInsertTool_QVIView()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlBulkInsert.QVIToggleButton.Pressed = true;
        }

        [Given(@"I Open System Information Tool QVI View")]
        [When(@"I Open System Information Tool QVI View")]
        [Then(@"I Open System Information Tool QVI View")]
        public void Open_SystemInformationTool_QVIView()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.GatherSystemInfo.OpenQuickVariableInpToggleButton.Pressed = true;
        }

        [Given(@"I Open Xpath Tool Qvi Large View")]
        [When(@"I Open Xpath Tool Qvi Large View")]
        [Then(@"I Open Xpath Tool Qvi Large View")]
        public void Open_XpathTool_QVIView()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.XPath.OpenQuickVariableInpToggleButton.Pressed = true;
        }

        [Given(@"I RightClick BaseConvert OnDesignSurface")]
        [When(@"I RightClick BaseConvert OnDesignSurface")]
        [Then(@"I RightClick BaseConvert OnDesignSurface")]
        public void RightClick_BaseConvert_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.BaseConvert, MouseButtons.Right, ModifierKeys.None, new Point(148, 12));
        }

        [Given(@"I RightClick Calculate OnDesignSurface")]
        [When(@"I RightClick Calculate OnDesignSurface")]
        [Then(@"I RightClick Calculate OnDesignSurface")]
        public void RightClick_Calculate_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Calculate, MouseButtons.Right, ModifierKeys.None, new Point(144, 10));
        }

        [Given(@"I RightClick STACKOVERFLOWTESTWORKFLOW OnDesignSurface")]
        [When(@"I RightClick STACKOVERFLOWTESTWORKFLOW OnDesignSurface")]
        [Then(@"I RightClick STACKOVERFLOWTESTWORKFLOW OnDesignSurface")]
        public void RightClick_StackOverFlowService_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.stackOverflowTestWF, MouseButtons.Right, ModifierKeys.None, new Point(181, 11));

        }

        public void RightClick_AssignOnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.AssignObject, MouseButtons.Right, ModifierKeys.None, new Point(156, 11));

        }

        [Given(@"I RightClick CaseConvert OnDesignSurface")]
        [When(@"I RightClick CaseConvert OnDesignSurface")]
        [Then(@"I RightClick CaseConvert OnDesignSurface")]
        public void RightClick_CaseConvert_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.CaseConvert, MouseButtons.Right, ModifierKeys.None, new Point(156, 10));
        }

        [Given(@"I RightClick Comment OnDesignSurface")]
        [When(@"I RightClick Comment OnDesignSurface")]
        [Then(@"I RightClick Comment OnDesignSurface")]
        public void RightClick_Comment_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Comment, MouseButtons.Right, ModifierKeys.None, new Point(121, 10));
        }

        [Given(@"I RightClick Copy OnDesignSurface")]
        [When(@"I RightClick Copy OnDesignSurface")]
        [Then(@"I RightClick Copy OnDesignSurface")]
        public void RightClick_Copy_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathCopy, MouseButtons.Right, ModifierKeys.None, new Point(104, 10));
        }

        [Given(@"I RightClick CountRecords OnDesignSurface")]
        [When(@"I RightClick CountRecords OnDesignSurface")]
        [Then(@"I RightClick CountRecords OnDesignSurface")]
        public void RightClick_CountRecords_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.CountRecordset, MouseButtons.Right, ModifierKeys.None, new Point(131, 10));
        }

        [Given(@"I RightClick CreateJSON OnDesignSurface")]
        [When(@"I RightClick CreateJSON OnDesignSurface")]
        [Then(@"I RightClick CreateJSON OnDesignSurface")]
        public void RightClick_CreateJSON_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.CreateJson, MouseButtons.Right, ModifierKeys.None, new Point(128, 9));
        }

        [Given(@"I RightClick CreateTool OnDesignSurface")]
        [When(@"I RightClick CreateTool OnDesignSurface")]
        [Then(@"I RightClick CreateTool OnDesignSurface")]
        public void RightClick_CreateTool_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathCreate, MouseButtons.Right, ModifierKeys.None, new Point(108, 14));
        }

        [Given(@"I RightClick DataMerge OnDesignSurface")]
        [When(@"I RightClick DataMerge OnDesignSurface")]
        [Then(@"I RightClick DataMerge OnDesignSurface")]
        public void RightClick_DataMerge_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataMerge, MouseButtons.Right, ModifierKeys.None, new Point(140, 7));
        }

        [Given(@"I RightClick DataSplit OnDesignSurface")]
        [When(@"I RightClick DataSplit OnDesignSurface")]
        [Then(@"I RightClick DataSplit OnDesignSurface")]
        public void RightClick_DataSplit_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataSplit, MouseButtons.Right, ModifierKeys.None, new Point(153, 6));
        }

        [Given(@"I RightClick DateTime OnDesignSurface")]
        [When(@"I RightClick DateTime OnDesignSurface")]
        [Then(@"I RightClick DateTime OnDesignSurface")]
        public void RightClick_DateTime_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DateTime, MouseButtons.Right, ModifierKeys.None, new Point(145, 13));
        }

        [Given(@"I RightClick DateTimeDifference OnDesignSurface")]
        [When(@"I RightClick DateTimeDifference OnDesignSurface")]
        [Then(@"I RightClick DateTimeDifference OnDesignSurface")]
        public void RightClick_DateTimeDifference_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DateTimeDifference, MouseButtons.Right, ModifierKeys.None, new Point(174, 10));
        }

        [Given(@"I RightClick Decision OnDesignSurface")]
        [When(@"I RightClick Decision OnDesignSurface")]
        [Then(@"I RightClick Decision OnDesignSurface")]
        public void RightClick_Decision_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Decision, MouseButtons.Right, ModifierKeys.None, new Point(28, 22));
        }

        [Given(@"I RightClick Delete OnDesignSurface")]
        [When(@"I RightClick Delete OnDesignSurface")]
        [Then(@"I RightClick Delete OnDesignSurface")]
        public void RightClick_Delete_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathDelete, MouseButtons.Right, ModifierKeys.None, new Point(100, 10));
        }

        [Given(@"I RightClick DeleteRecord OnDesignSurface")]
        [When(@"I RightClick DeleteRecord OnDesignSurface")]
        [Then(@"I RightClick DeleteRecord OnDesignSurface")]
        public void RightClick_DeleteRecord_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DeleteRecord, MouseButtons.Right, ModifierKeys.None, new Point(116, 9));
        }

        [When(@"I RightClick DotNetDllConnector OnDesignSurface")]
        public void RightClick_DotNetDllConnector_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll, MouseButtons.Right, ModifierKeys.None, new Point(164, 10));
        }

        [Given(@"I RightClick DropboxFileOperation OnDesignSurface")]
        [When(@"I RightClick DropboxFileOperation OnDesignSurface")]
        [Then(@"I RightClick DropboxFileOperation OnDesignSurface")]
        public void RightClick_DropboxFileOperation_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DropboxDownload, MouseButtons.Right, ModifierKeys.None, new Point(181, 11));
        }

        [Given(@"I RightClick Email OnDesignSurface")]
        [When(@"I RightClick Email OnDesignSurface")]
        [Then(@"I RightClick Email OnDesignSurface")]
        public void RightClick_Email_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SMTPEmail, MouseButtons.Right, ModifierKeys.None, new Point(129, 11));
        }

        [Given(@"I RightClick ExecuteCommandLine OnDesignSurface")]
        [When(@"I RightClick ExecuteCommandLine OnDesignSurface")]
        [Then(@"I RightClick ExecuteCommandLine OnDesignSurface")]
        public void RightClick_ExecuteCommandLine_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ExecuteCommandLine, MouseButtons.Right, ModifierKeys.None, new Point(165, 13));
        }

        [When(@"I RightClick FindIndex OnDesignSurface")]
        public void RightClick_FindIndex_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FindIndex, MouseButtons.Right, ModifierKeys.None, new Point(113, 8));
        }

        [When(@"I RightClick FindRecordIndex OnDesignSurface")]
        public void RightClick_FindRecordIndex_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FindRecordsIndex, MouseButtons.Right, ModifierKeys.None, new Point(191, 11));
        }

        [When(@"I RightClick ForEach OnDesignSurface")]
        public void RightClick_ForEach_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ForEach, MouseButtons.Right, ModifierKeys.None, new Point(137, 9));
        }

        [When(@"I RightClick FormatNumber OnDesignSurface")]
        public void RightClick_FormatNumber_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FormatNumber, MouseButtons.Right, ModifierKeys.None, new Point(143, 9));
        }

        [When(@"I RightClick Length OnDesignSurface")]
        public void RightClick_Length_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Length, MouseButtons.Right, ModifierKeys.None, new Point(97, 10));
        }

        [When(@"I RightClick Move OnDesignSurface")]
        public void RightClick_Move_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathMove, MouseButtons.Right, ModifierKeys.None, new Point(98, 11));
        }

        [When(@"I RightClick MySQLConnector OnDesignSurface")]
        public void RightClick_MySQLConnector_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MySqlDatabase, MouseButtons.Right, ModifierKeys.None, new Point(202, 10));
        }

        [When(@"I RightClick New Workflow Tab")]
        public void RightClick_New_Workflow_Tab()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab, MouseButtons.Right, ModifierKeys.None, new Point(63, 18));
        }

        [When(@"I RightClick Random OnDesignSurface")]
        public void RightClick_Random_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Random, MouseButtons.Right, ModifierKeys.None, new Point(107, 13));
        }

        [When(@"I RightClick ReadFile OnDesignSurface")]
        public void RightClick_ReadFile_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FileRead, MouseButtons.Right, ModifierKeys.None, new Point(99, 14));
        }

        [When(@"I RightClick ReadFolder OnDesignSurface")]
        public void RightClick_ReadFolder_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FolderRead, MouseButtons.Right, ModifierKeys.None, new Point(115, 12));
        }

        [When(@"I RightClick Rename OnDesignSurface")]
        public void RightClick_Rename_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PathRename, MouseButtons.Right, ModifierKeys.None, new Point(103, 7));
        }

        [When(@"I RightClick Replace OnDesignSurface")]
        public void RightClick_Replace_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Replace, MouseButtons.Right, ModifierKeys.None, new Point(100, 7));
        }

        [When(@"I RightClick Sequence OnDesignSurface")]
        public void RightClick_Sequence_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Sequence, MouseButtons.Right, ModifierKeys.None, new Point(119, 8));
        }

        [When(@"I RightClick SharepointCreateListItem OnDesignSurface")]
        public void RightClick_SharepointCreateListItem_OnDesignSurface()
        {
            Mouse.Click(UIMap.MainStudioWindow.DesignSurfaceContextMenu.Copy, MouseButtons.Right, ModifierKeys.None, new Point(199, 12));
        }

        [When(@"I RightClick SharepointDelete OnDesignSurface")]
        public void RightClick_SharepointDelete_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointDeleteFile, MouseButtons.Right, ModifierKeys.None, new Point(217, 8));
        }

        [When(@"I RightClick SharepointRead OnDesignSurface")]
        public void RightClick_SharepointRead_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointReadListItem, MouseButtons.Right, ModifierKeys.None, new Point(203, 9));
        }

        [When(@"I RightClick SharepointUpdate OnDesignSurface")]
        public void RightClick_SharepointUpdate_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUpdateListItem, MouseButtons.Right, ModifierKeys.None, new Point(210, 5));
        }

        [When(@"I RightClick SortRecords OnDesignSurface")]
        public void RightClick_SortRecords_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SortRecords, MouseButtons.Right, ModifierKeys.None, new Point(118, 8));
        }

        [When(@"I RightClick SQLConnector OnDesignSurface")]
        public void RightClick_SQLConnector_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlBulkInsert, MouseButtons.Right, ModifierKeys.None, new Point(143, 6));
        }

        [When(@"I RightClick SqlServerConnector OnDesignSurface")]
        public void RightClick_SqlServerConnector_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlServerDatabase, MouseButtons.Right, ModifierKeys.None, new Point(198, 8));
        }

        [When(@"I RightClick Switch OnDesignSurface")]
        public void RightClick_Switch_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Switch, MouseButtons.Right, ModifierKeys.None, new Point(46, 15));
        }

        [When(@"I RightClick Unzip OnDesignSurface")]
        public void RightClick_Unzip_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlServerDatabase.LargeView.ActionsCombobox, MouseButtons.Right, ModifierKeys.None, new Point(101, 10));
        }

        [When(@"I RightClick WebRequest OnDesignSurface")]
        public void RightClick_WebRequest_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebRequest, MouseButtons.Right, ModifierKeys.None, new Point(165, 8));
        }

        [When(@"I RightClick WriteFile OnDesignSurface")]
        public void RightClick_WriteFile_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FileWrite, MouseButtons.Right, ModifierKeys.None, new Point(96, 12));
        }

        [When(@"I RightClick XPath OnDesignSurface")]
        public void RightClick_XPath_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.XPath, MouseButtons.Right, ModifierKeys.None, new Point(99, 8));
        }

        [When(@"I RightClick Zip OnDesignSurface")]
        public void RightClick_Zip_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Zip, MouseButtons.Right, ModifierKeys.None, new Point(95, 12));
        }

        [When(@"I Select AcceptanceTestin create")]
        public void Select_AcceptanceTestin_create()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCreateListItem.SmallView.MethodList, new Point(119, 7));
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCreateListItem.SmallView.MethodList.UIAcceptanceTesting_CrListItem, new Point(114, 13));
        }

        [When(@"I Select Action")]
        public void Select_Action()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ActionMethodListBoxList.ActivitiesDesignListItem.ActivitiesExpander.ActivitiesDesignButton.ActionsComboBox, new Point(216, 7));
            Mouse.Click(UIMap.MainStudioWindow.getName, new Point(137, 7));
        }

        [When(@"I Select AppData From MethodList")]
        public void Select_AppData_From_MethodList()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCreateListItem.SmallView.MethodList, new Point(174, 7));
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCreateListItem.SmallView.MethodList.UIAppdataListItem, new Point(43, 15));
        }

        [When(@"I Select AppData From MethodList From ReadTool")]
        public void Select_AppData_From_MethodList_From_ReadTool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointReadListItem.SmallView.MethodList, new Point(174, 7));
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointReadListItem.SmallView.MethodList.UIAppdataListItem, new Point(43, 15));
        }

        [When(@"I Select First Item From DotNet DLL Large View Source Combobox")]
        public void Select_First_Item_From_DotNet_DLL_Large_View_Source_Combobox()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.SourcesComboBox, new Point(175, 9));
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.SourcesComboBox.ListItem1, new Point(163, 17));
        }

        [When(@"I Select Classname From DotNet DLL Large View Classname Combobox")]
        public void Select_DotNet_Dll_Classname()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ClassNameComboBox, new Point(175, 9));
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ClassNameComboBox.HumalClassListItem, new Point(163, 17));
        }

        public void Click_DotNet_DLL_Large_View_FirstAction_Combobox()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ActionMethodListBoxList.ActivitiesDesignListItem.ActivitiesExpander.ActivitiesDesignButton.ActionsComboBox, new Point(175, 9));
        }

        public void Click_DotNet_DLL_Large_View_SecondAction_Combobox()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ActionMethodListBoxList.ActivitiesDesignListItem2.ActivitiesExpander.ActivitiesDesignButton.ActionsComboBox, new Point(175, 9));
        }

        public void Click_DotNet_DLL_Large_View_ThirdAction_Combobox()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ActionMethodListBoxList.ActivitiesDesignListItem.ActivitiesExpander.ActivitiesDesignButton.ActionsComboBox, new Point(175, 9));
        }

        public void Click_DotNet_DLL_Large_View_Constructor_Combobox()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.CtorExpander.ActivitiesDesignButton.ConstructorsComboBox, new Point(175, 9));
        }

        public void Select_DotNet_DLL_Large_View_Constructor_With_One_Parameter_From_Constructor_Combobox()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.CtorExpander.ActivitiesDesignButton.ConstructorsComboBox.CtorSystemStringListItem, new Point(175, 9));
        }
        public void I_Expand_Costructor_Tree()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.OneParamCtorExpander.Expanded = true;
        }
        public void Select_DotNet_DLL_Large_View_SetName_Action_For_FirstAction_From_Actions_Combobox()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ActionMethodListBoxList.ActivitiesDesignListItem.ActivitiesExpander.ActivitiesDesignButton.ActionsComboBox.Set_NameListItem, new Point(175, 9));
        }
        public void Select_DotNet_DLL_Large_View_SetName_Action_For_SecondAction_From_Actions_Combobox()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ActionMethodListBoxList.ActivitiesDesignListItem2.ActivitiesExpander.ActivitiesDesignButton.ActionsComboBox.Set_NameListItem, new Point(175, 9));
        }
        public void Select_DotNet_DLL_Large_View_SetName_Action_For_ThirdAction_From_Actions_Combobox()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ActionMethodListBoxList.ActivitiesDesignListItem.ActivitiesExpander.ActivitiesDesignButton.ActionsComboBox.Set_NameListItem, new Point(175, 9));
        }
        public void Click_Refresh_Combobox_Button_On_Third_Action_Combobox()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ActionMethodListBoxList.ActivitiesDesignListItem.ActivitiesExpander.ActivitiesDesignButton.ActionsComboBox.Set_NameListItem, new Point(175, 9));
        }

        public void Click_Delete_Button_On_Second_Action_Combobox()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ActionMethodListBoxList.ActivitiesDesignListItem2.SetActivitiesExpander.ActivitiesDesignButton.DeleteActionButton);
        }

        [When(@"I Select Empty Constructor From DotNet DLL Large View Constructor Combobox")]
        public void Select_DotNet_Dll_Default_Costructor()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.CtorExpander.ActivitiesDesignButton.ConstructorsComboBox, new Point(175, 9));
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.CtorExpander.ActivitiesDesignButton.ConstructorsComboBox.CtorListItem, new Point(163, 17));
        }
        public void Select_First_Action_From_Dotnet_Dll_LargeView_Action_Dropdown()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ActionMethodListBoxList.ActivitiesDesignListItem.ActivitiesExpander.ActivitiesDesignButton.ActionsComboBox, new Point(175, 9));
            Mouse.Click(UIMap.MainStudioWindow.getName, new Point(163, 17));
        }

        [When(@"I Select GetCountries From SQL Server Large View Action Combobox")]
        public void Select_GetCountries_From_SQL_Server_Large_View_Action_Combobox()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlServerDatabase.LargeView.ActionsCombobox, new Point(216, 7));
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlServerDatabase.LargeView.ActionsCombobox.GetCountriesListItem, new Point(137, 7));
            Assert.AreEqual("dbo.GetCountries", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlServerDatabase.LargeView.ActionsCombobox.SelectedItem, "GetCountries is not selected in SQL server large view action combobox.");
        }

        [When(@"I Select Namespace")]
        public void Select_Namespace()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ClassNameComboBox, new Point(216, 7));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ClassNameComboBox.ComboboxlistItemAsSystemObject.Exists, "System.Random item does not exist in the DotNet DLL tool ClassName dropdown");
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ClassNameComboBox.ComboboxlistItemAsSystemObject, new Point(137, 7));
        }

        [When(@"I Select Months From AddTime Type")]
        public void Select_Months_From_AddTime_Type()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DateTime.SmallViewContentCustom.AddTimeTypeComboBox, new Point(175, 9));
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DateTime.SmallViewContentCustom.AddTimeTypeComboBox.Months, new Point(163, 17));
        }

        [When(@"I Select NewSQLServerDatabaseSource FromSqlServerTool")]
        public void Select_NewSQLServerDatabaseSource_From_SqlServerTool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlServerDatabase.LargeView.NewDbSourceButton, new Point(16, 13));
        }

        [When(@"I Select NewSharepointSource FromServer Lookup")]
        public void Select_NewSharepointSource_FromServer_Lookup()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCreateListItem.SmallView.Server, new Point(107, 13));
            Keyboard.SendKeys(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCreateListItem.SmallView.Server, "{Down}{Enter}", ModifierKeys.None);
        }

        [When(@"I Select OutputIn Days")]
        public void Select_OutputIn_Days()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DateTimeDifference.SmallViewContentCustom.OutputInComboBox, new Point(119, 7));
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DateTimeDifference.SmallViewContentCustom.OutputInComboBox.Days, new Point(114, 13));
        }

        [When(@"I Select Round Up")]
        public void Select_Round_Up()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FormatNumber.LargeViewContentCustom.RoundingComboBox, new Point(119, 7));
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FormatNumber.LargeViewContentCustom.RoundingComboBox.RoungUP, new Point(114, 13));
        }

        [When(@"I Select RoundingType None")]
        public void Select_RoundingType_None()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FormatNumber.LargeViewContentCustom.RoundingComboBox, new Point(119, 7));
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FormatNumber.LargeViewContentCustom.RoundingComboBox.None, new Point(114, 13));
        }

        [When(@"I Select RoundingType Normal")]
        public void Select_RoundingType_Normal()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FormatNumber.LargeViewContentCustom.RoundingComboBox, new Point(119, 7));
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.FormatNumber.LargeViewContentCustom.RoundingComboBox.Normal, new Point(114, 13));
        }

        [When(@"I Select SharepointTestServer")]
        public void Select_SharepointTestServer()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCreateListItem.SmallView.Server, new Point(98, 12));
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCreateListItem.SmallView.Server.SharepointTestServer, new Point(67, 13));
        }

        [When(@"I Select SharepointTestServer From SharepointRead Tool")]
        public void Select_SharepointTestServer_From_SharepointRead_Tool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointReadListItem.SmallView.Server, new Point(98, 12));
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointReadListItem.SmallView.Server.SharepointTestServer, new Point(67, 13));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointReadListItem.SmallView.EditSourceButton.Enabled, "edit sharepoint source is disabled after selecting a source");
        }

        [When(@"I Select SharepointTestServer From SharepointUpdateListItem Tool")]
        public void Select_SharepointTestServer_From_SharepointUpdateListItem_Tool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUpdateListItem.SmallView.Server, new Point(98, 12));
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUpdateListItem.SmallView.Server.SharepointTestServer, new Point(67, 13));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUpdateListItem.SmallView.EditSourceButton.Enabled, "edit sharepoint source is disabled after selecting a source");
        }

        [When(@"I Select Source From DotnetTool")]
        public void Select_Source_From_DotnetTool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.SourcesComboBox, new Point(119, 7));
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.SourcesComboBox.DotNetSource, new Point(114, 13));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ClassNameComboBox.Enabled, "ClassNameComboBox is not Enabled after selecting a source");
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ClassNameComboBox, new Point(119, 7));
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ClassNameComboBox.AssemblyLocationGACCListItem, new Point(114, 13));
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ActionMethodListBoxList.ActivitiesDesignListItem.ActivitiesExpander.ActivitiesDesignButton.ActionsComboBox, new Point(119, 7));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.GenerateOutputsButton.Enabled, "GenerateOutputsButton is not Enabled after selecting an Action");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.LargeDataGridTable.Row1.Enabled, "InputsDataGridTable is not Enabled after selecting an Action");
            Mouse.Click(UIMap.MainStudioWindow.SideMenuBar.SaveButton, new Point(10, 5));
        }

        [When(@"I Select SystemObject From DotNet DLL Large View Namespace Combobox")]
        public void Select_SystemObject_From_DotNet_DLL_Large_View_Namespace_Combobox()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ClassNameComboBox, new Point(216, 7));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ClassNameComboBox.ComboboxlistItemAsSystemObject.Exists, "System.Random item does not exist in the DotNet DLL tool ClassName dropdown");
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ClassNameComboBox.ComboboxlistItemAsSystemObject, new Point(137, 7));
            Assert.AreEqual("{\"AssemblyLocation\":\"C:\\\\Windows\\\\Microsoft.NET\\\\Framework64\\\\v4.0.30319\\\\mscorli" +
                            "b.dll\",\"AssemblyName\":\"mscorlib.dll\",\"FullName\":\"System.Object\",\"MethodName\":nul" +
                            "l}", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ClassNameComboBox.SelectedItem, "System.Object is not selected in DotNet DLL tool large view namespace combobox.");
        }

        [When(@"I Select SystemRandom From DotNet DLL Large View Namespace Combobox")]
        public void Select_SystemRandom_From_DotNet_DLL_Large_View_Namespace_Combobox()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ClassNameComboBox, new Point(216, 7));
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ClassNameComboBox.SystemRandomListItem, new Point(137, 7));
            Assert.AreEqual("{\"AssemblyLocation\":\"C:\\\\Windows\\\\Microsoft.NET\\\\Framework64\\\\v4.0.30319\\\\mscorli" +
                            "b.dll\",\"AssemblyName\":\"mscorlib.dll\",\"FullName\":\"System.Random\",\"MethodName\":nul" +
                            "l}", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ClassNameComboBox.SelectedItem, "System.Random is not selected in DotNet DLL tool large view namespace combobox.");
        }

        [When(@"I Select ToString From DotNet DLL Large View Action Combobox")]
        public void Select_ToString_From_DotNet_DLL_Large_View_Action_Combobox()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ActionMethodListBoxList.ActivitiesDesignListItem.ActivitiesExpander.ActivitiesDesignButton.ActionsComboBox, new Point(216, 7));
        }

        [When(@"I Select TSTCIREMOTE From Server Source Wizard Dropdownlist")]
        public void Select_TSTCIREMOTE_From_Server_Source_Wizard_Dropdownlist()
        {
            Mouse.Click(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext.NewServerSource.AddressComboBox.TSTCIREMOTE, new Point(70, 19));
            Assert.AreEqual("TST-CI-REMOTE", UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext.NewServerSource.AddressComboBox.AddressEditBox.Text, "Server source address textbox text does not equal TST-CI-REMOTE");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext.NewServerSource.TestConnectionButton.Exists, "Server source wizard does not contain a test connection button");
        }

        [When(@"I Select UITestingDBSource From SQL Server Large View Source Combobox")]
        public void Select_UITestingDBSource_From_SQL_Server_Large_View_Source_Combobox()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlServerDatabase.LargeView.SourcesCombobox, new Point(216, 7));
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlServerDatabase.LargeView.SourcesCombobox.UITestingDBSourceListItem, new Point(137, 7));
            Assert.AreEqual("UITestingDBSource", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlServerDatabase.LargeView.SourcesCombobox.SelectedItem, "SQL Server large view source combobox selected item is not equal to UITestingDBSource.");
        }

        [When(@"I Select UITestingSource From Web Server Large View Source Combobox")]
        public void Select_UITestingSource_From_Web_Server_Large_View_Source_Combobox()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete.LargeView.SourcesComboBox, new Point(216, 7));
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete.LargeView.SourcesComboBox.UITesting, new Point(137, 7));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete.LargeView.EditSourceButton.Enabled, "Delete Web large view source combobox EDIT button is disabled.");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete.LargeView.GenerateOutputsButton.Enabled, "Delete Web large view source combobox GenerateOutput button is disabled.");
        }

        [When(@"I Select Zip Compression")]
        public void Select_Zip_Compression()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Zip.LargeViewContentCustom.SelectedCompressComboBox, new Point(119, 7));
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Zip.LargeViewContentCustom.SelectedCompressComboBox.NormalDefault, new Point(114, 13));
        }

        [When(@"I Type 0 Into SQL Server Large View Inputs Row1 Data Textbox")]
        public void Type_0_Into_SQL_Server_Large_View_Inputs_Row1_Data_Textbox()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlServerDatabase.LargeView.InputsTable.Row1.DataCell.DataCombobox.DataTextbox.Text = "0";
            Assert.AreEqual("0", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlServerDatabase.LargeView.InputsTable.Row1.DataCell.DataCombobox.DataTextbox.Text, "SQL Server large view inputs row 1 data textbox text is not equal to S");
        }

        [When(@"I Type 0 Into SQL Server Large View Test Inputs Row1 Test Data Textbox")]
        public void Type_0_Into_SQL_Server_Large_View_Test_Inputs_Row1_Test_Data_Textbox()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlServerDatabase.LargeView.TestInputsTable.Row1.TestDataCell.TestDataComboBox.TestDataTextbox.Text = "0";
            Assert.AreEqual("0", MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlServerDatabase.LargeView.TestInputsTable.Row1.TestDataCell.TestDataComboBox.TestDataTextbox.Text, "SQL Server large view test inputs row 1 test data textbox text is not equal to S");
        }

        [When(@"I Type The Testing Site into Web POST Source Wizard Address Textbox")]
        public void Type_The_Testing_Site_into_Web_POST_Source_Wizard_Address_Textbox()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceTab.WorkSurfaceContext.AddressTextbox.Text = "http://rsaklfsvrtfsbld:9810/api/products/Post";
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceTab.WorkSurfaceContext.TestConnectionButton.Enabled, "New web source wizard test connection button is not enabled after entering a valid web post address.");
        }

        [When(@"I Type The Testing Site into Web DELETE Source Wizard Address Textbox")]
        public void Type_The_Testing_Site_into_Web_DELETE_Source_Wizard_Address_Textbox()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceTab.WorkSurfaceContext.AddressTextbox.Text = "http://rsaklfsvrtfsbld:9810/api/products/Delete";
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceTab.WorkSurfaceContext.TestConnectionButton.Enabled, "New web source wizard test connection button is not enabled after entering a valid web delete address.");
        }

        [When(@"I Type The Testing Site into Web PUT Source Wizard Address Textbox")]
        public void Type_The_Testing_Site_into_Web_PUT_Source_Wizard_Address_Textbox()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceTab.WorkSurfaceContext.AddressTextbox.Text = "http://rsaklfsvrtfsbld:9810/api/products/Put";
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceTab.WorkSurfaceContext.TestConnectionButton.Enabled, "New web source wizard test connection button is not enabled after entering a valid web put address.");
        }

        [When(@"I Click Decision Large View Match Type Combobox")]
        public void Click_Decision_Large_View_Match_Type_Combobox()
        {
            Mouse.Click(DialogsUIMap.DecisionOrSwitchDialog.LargeView.Table.Row1.MatchTypeCell.MatchTypeCombobox, new Point(5, 5));
        }

        [When(@"I Make Workflow Savable")]
        public void Make_Workflow_Savable()
        {
            Drag_Toolbox_Comment_Onto_DesignSurface();
        }

        public void Move_Assign_Message_Tool_On_The_Design_Surface()
        {
            var multiAssign1 = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign;
            multiAssign1.EnsureClickable(new Point(90, 7));
            Mouse.StartDragging(multiAssign1, new Point(94, 11));
            Mouse.StopDragging(multiAssign1, 70, 3);
        }

        public int Expand_Comment_Tool_Size()
        {
            var defaultHeight = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Comment.LargeViewContentCustom.Height;
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Comment.LargeViewContentCustom.EnsureClickable(new Point(226, 335));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Comment.LargeViewContentCustom.ItemResizer);
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Comment.LargeViewContentCustom, new Point(0, 350));
            var newHeight = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Comment.LargeViewContentCustom.Height;

            Assert.IsTrue(newHeight > defaultHeight, "Comment tool height has not changed after dragging the resize indicator downward.");
            return newHeight;
        }

        [Then(@"I Click workflow tab")]
        [Given(@"I Click workflow tab")]
        [When(@"I Click workflow tab")]
        public void ThenIClickWorkflowWizardTab()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab);
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Exists);
        }

        [Then(@"I Enter ""(.*)"" in the Assign message tool")]
        [When(@"I Enter ""(.*)"" in the Assign message tool")]
        [Given(@"I Enter ""(.*)"" in the Assign message tool")]
        public void ThenIEnterInTheAssignMessageTool(string message)
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.SmallView.DataGrid.Row1.ValueCell.IntellisenseCombobox.Textbox.Text = message;
        }

        [Given(@"I Save Workflow Using Shortcut")]
        [When(@"I Save Workflow Using Shortcut")]
        [Then(@"I Save Workflow Using Shortcut")]
        public void Save_Workflow_Using_Shortcut()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(74, 8));
            Keyboard.SendKeys(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, "S", (ModifierKeys.Control));
        }

        [Then(@"There is an error")]
        public void TheArdonerhasAnError()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Adornert_numbernText.Exists, "The tool is valid and did not throw any errors on Test Clicked.");
        }

        [Given(@"I RightClick Ardoner Hyperlink")]
        [When(@"I RightClick Ardoner Hyperlink")]
        [Then(@"I RightClick Ardoner Hyperlink")]
        public void RightClick_Adorner_Control()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Adornert_numbernText.NumbernHyperlink, MouseButtons.Right, ModifierKeys.None, new Point(88, 12));
        }

        public void Click_COMDLLPluginTool_LargeView_NewSourceButton()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ComDll.LargeView.NewSourceButton);
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.COMPlugInSourceTab.Exists, "New COM Plugin source wizard tab does not exist after clicking new source from COM DLL Tool");
        }

        public void Click_DotNetPluginTool_LargeView_NewSourceButton()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.NewSourceButton);
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DotNetPluginSourceTab.Exists, "New DotNet Plugin source wizard tab does not exist after clicking new source from DotNet DLL Tool");
        }

        [When(@"I Drag Start Node")]
        public void Drag_Start_Node()
        {
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.StartNode, new Point(186, 30));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.StartNode, 1400, 968);
        }

        [When(@"I Drag POSTWebTool Onto DesignSurface")]
        public void Drag_POSTWebTool_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "POST";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(306, 128));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.HTTPWebMethods.POST, new Point(20, 35));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(306, 128));
            UIMap.Click_Clear_Toolbox_Filter_Clear_Button();
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPost.LargeView.Exists, "Web Post Request large view does not exist on the design surface.");
        }

        [Given(@"I Open Switch Tool Large View")]
        [When(@"I Open Switch Tool Large View")]
        [Then(@"I Open Switch Tool Large View")]
        public void Open_Switch_Tool_Large_View()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Switch, new Point(39, 35));
            Assert.IsTrue(DialogsUIMap.DecisionOrSwitchDialog.Enabled, "Switch dialog does not exist after opening switch large view");
        }

        [When(@"I Drag PUTWebTool Onto DesignSurface")]
        public void Drag_PUTWebTool_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "PUT";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(306, 126));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.HTTPWebMethods.PUT, new Point(16, 25));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(306, 126));
            UIMap.Click_Clear_Toolbox_Filter_Clear_Button();
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPut.LargeView.Exists, "Put Web connector large view does not exist on the design surface after drag and drop from toolbox.");
        }

        [When(@"I Drag DELETEWebTool Onto DesignSurface")]
        public void Drag_DELETEWebTool_Onto_DesignSurface()
        {
            UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text = "DELETE";
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.EnsureClickable(new Point(306, 126));
            Mouse.StartDragging(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.ToolListBox.HTTPWebMethods.DELETE, new Point(16, 25));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart, new Point(306, 126));
            UIMap.Click_Clear_Toolbox_Filter_Clear_Button();
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete.LargeView.Exists, "Delete Web Tool large view does not exist on the design surface after drag and drop from toolbox.");
        }

        [When(@"I Collapse GetWebTool Large View to Small View With Double Click")]
        public void Collapse_GETWebTool_LargeView_To_SmallView()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebGet.LargeView.Exists, "Cannot collapse Web GET tool to small view, large view does not exist.");
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebGet, new Point(201, 9));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebGet.SmallView.Exists, "Web GET small view does not exist after collapsing the large view with a double click.");
        }

        [When(@"I Collapse PostWebTool Large View to Small View With Double Click")]
        public void Collapse_POSTWebTool_LargeView_To_SmallView()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPost.LargeView.Exists, "Cannot collapse Web POST tool to small view, large view does not exist.");
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPost, new Point(201, 9));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPost.SmallView.Exists, "Web POST small view does not exist after collapsing the large view with a double click.");
        }

        [When(@"I Collapse PutWebTool Large View to Small View With Double Click")]
        public void Collapse_PUTWebTool_LargeView_To_SmallView()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPut.LargeView.Exists, "Cannot collapse Web PUT tool to small view, large view does not exist.");
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPut, new Point(201, 9));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPut.SmallView.Exists, "Web PUT small view does not exist after collapsing the large view with a double click.");
        }

        [When(@"I Collapse DeleteWebTool Large View to Small View With Double Click")]
        public void Collapse_DELETEWebTool_LargeView_To_SmallView()
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete.LargeView.Exists, "Cannot collapse Web DELETE tool to small view, large view does not exist.");
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete, new Point(201, 9));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete.SmallView.Exists, "Web DELETE small view does not exist after collapsing the large view with a double click.");
        }

        public void I_Expand_First_Action_Tree()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.ActionMethodListBoxList.ActivitiesDesignListItem.SetActivitiesExpander.Expanded = true;
        }

        public void Select_Source_From_MySQLTool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MySqlDatabase.LargeView.SourcesComboBox);
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MySqlDatabase.LargeView.SourcesComboBox.MySQLSourceFromToolListItem);
        }

        public void Select_Source_From_ODBCTool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ODBCDatabaseActivCustom.LargeView.SourcesComboBox);
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ODBCDatabaseActivCustom.LargeView.SourcesComboBox.ODBCSourceFromToolListItem);
        }

        public void Select_Source_From_OracleTool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.OracleDatabaseActCustom.LargeView.SourcesComboBox);
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.OracleDatabaseActCustom.LargeView.SourcesComboBox.OracleSourceFromToolListItem);
        }

        public void Select_Source_From_PostgreTool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PostgreSqlActivitCustom.LargeView.SourcesComboBox);
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PostgreSqlActivitCustom.LargeView.SourcesComboBox.PostgreSQLSourceFromListItem);
        }

        public void Select_Source_From_SQLServerTool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlServerDatabase.LargeView.SourcesCombobox);
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlServerDatabase.LargeView.SourcesCombobox.SQLServerSourceFromTListItem);
        }

        public void Select_Source_From_ComDLLTool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ComDll.LargeView.SourcesCombobox);
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ComDll.LargeView.SourcesCombobox.COMPluginSourceToEditListItem);
        }

        public void Select_Source_From_DotNetDLLTool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.SourcesComboBox);
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.SourcesComboBox.DotNetPluginSource);
        }

        public void Select_New_Source_From_DotNetDLLTool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.SourcesComboBox);
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.SourcesComboBox.NewDotNetPluginSource);
        }

        public void Select_Source_From_EmailTool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SMTPEmail.LargeViewContent.SourceComboBox);
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SMTPEmail.LargeViewContent.SourceComboBox.UIHostlocalhostUserNamListItem);
        }

        public void Select_Source_From_RabbitMQConsumeTool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.RabbitMQConsume.LargeViewContentCustom.SourceComboBox);
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.RabbitMQConsume.LargeViewContentCustom.SourceComboBox.RabbitMQSourceFromToolListItem);
        }

        public void Select_Source_From_SharepointCopyFileTool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCopyFile.LargeView.Server);
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCopyFile.LargeView.Server.SharepointSourceFromToolListItem);
        }

        public void Select_Source_From_DELETEWebTool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete.LargeView.SourcesComboBox);
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete.LargeView.SourcesComboBox.WebServiceSourceFromToolListItem);
        }

        public void Select_Source_From_ExchangeSendTool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ExchangeEmail.LargeViewContent.SourcesComboBox);
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ExchangeEmail.LargeViewContent.SourcesComboBox.Dev2CommonInterfacesListItem);
        }

        public void Click_EditSourceButton_On_MySQLTool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MySqlDatabase.LargeView.EditSourceButton);
        }

        public void Click_EditSourceButton_On_ODBCTool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ODBCDatabaseActivCustom.LargeView.EdistSourceButton);
        }

        public void Click_EditSourceButton_On_OracleTool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.OracleDatabaseActCustom.LargeView.EditSourceButton);
        }

        public void Click_EditSourceButton_On_PostgreSQLTool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.PostgreSqlActivitCustom.LargeView.EditSourceButton);
        }

        public void Click_EditSourceButton_On_SQLServerTool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SqlServerDatabase.LargeView.EditSourceButton);
        }
        public void Click_EditSourceButton_On_ComDLLTool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ComDll.LargeView.EditSourceButton);
        }
        public void Click_EditSourceButton_On_DotNetDLLTool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DotNetDll.LargeView.EditSourceButton);
        }

        public void Click_EditSourceButton_On_EmailTool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SMTPEmail.LargeViewContent.EditSourceButton);
        }

        public void Click_EditSourceButton_On_RabbitMQConsumeTool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.RabbitMQConsume.LargeViewContentCustom.EditSourceButton);
        }

        public void Click_EditSourceButton_On_RabbitMQPublishTool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.RabbitMQPublish.LargeViewContentCustom.EditSourceButton);
        }

        public void Click_EditSourceButton_On_SharepointCopyFileTool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCopyFile.LargeView.EditSourceButton);
        }

        public void Click_EditSourceButton_On_DELETEWebTool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete.LargeView.EditSourceButton);
        }

        public void Click_EditSourceButton_On_ExchangeSendTool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ExchangeEmail.LargeViewContent.ItemButton);
        }

        public void Click_EditSourceButton_On_ExchangeSendToolSmallView()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ExchangeEmail.SmallViewContent.ItemButton);
        }
    }
}
