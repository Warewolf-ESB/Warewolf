/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Activities;
using Dev2.Activities.Designers2.AggregateCalculate;
using Dev2.Activities.Designers2.BaseConvert;
using Dev2.Activities.Designers2.Calculate;
using Dev2.Activities.Designers2.CaseConvert;
using Dev2.Activities.Designers2.CommandLine;
using Dev2.Activities.Designers2.Comment;
using Dev2.Activities.Designers2.Copy;
using Dev2.Activities.Designers2.Create;
using Dev2.Activities.Designers2.CreateJSON;
using Dev2.Activities.Designers2.DataMerge;
using Dev2.Activities.Designers2.DataSplit;
using Dev2.Activities.Designers2.Delete;
using Dev2.Activities.Designers2.DropBox2016.Delete;
using Dev2.Activities.Designers2.DropBox2016.Download;
using Dev2.Activities.Designers2.DropBox2016.DropboxFile;
using Dev2.Activities.Designers2.DropBox2016.Upload;
using Dev2.Activities.Designers2.Email;
using Dev2.Activities.Designers2.ExchangeEmail;
using Dev2.Activities.Designers2.FindIndex;
using Dev2.Activities.Designers2.FindRecordsMultipleCriteria;
using Dev2.Activities.Designers2.Foreach;
using Dev2.Activities.Designers2.FormatNumber;
using Dev2.Activities.Designers2.GatherSystemInformation;
using Dev2.Activities.Designers2.GetWebRequest;
using Dev2.Activities.Designers2.GetWebRequest.GetWebRequestWithTimeout;
using Dev2.Activities.Designers2.Move;
using Dev2.Activities.Designers2.MultiAssign;
using Dev2.Activities.Designers2.MySqlDatabase;
using Dev2.Activities.Designers2.ODBC;
using Dev2.Activities.Designers2.Oracle;
using Dev2.Activities.Designers2.PostgreSql;
using Dev2.Activities.Designers2.RabbitMQ.Consume;
using Dev2.Activities.Designers2.RabbitMQ.Publish;
using Dev2.Activities.Designers2.Random;
using Dev2.Activities.Designers2.ReadFile;
using Dev2.Activities.Designers2.ReadFolder;
using Dev2.Activities.Designers2.Rename;
using Dev2.Activities.Designers2.Replace;
using Dev2.Activities.Designers2.Script;
using Dev2.Activities.Designers2.SelectAndApply;
using Dev2.Activities.Designers2.Sequence;
using Dev2.Activities.Designers2.Service;
using Dev2.Activities.Designers2.SharePointCopyFile;
using Dev2.Activities.Designers2.SharePointDeleteFile;
using Dev2.Activities.Designers2.SharePointFileDownload;
using Dev2.Activities.Designers2.SharePointFileUpload;
using Dev2.Activities.Designers2.SharepointFolderRead;
using Dev2.Activities.Designers2.SharepointListCreate;
using Dev2.Activities.Designers2.SharepointListDelete;
using Dev2.Activities.Designers2.SharepointListRead;
using Dev2.Activities.Designers2.SharepointListUpdate;
using Dev2.Activities.Designers2.SharePointMoveFile;
using Dev2.Activities.Designers2.SortRecords;
using Dev2.Activities.Designers2.SqlBulkInsert;
using Dev2.Activities.Designers2.SqlServerDatabase;
using Dev2.Activities.Designers2.UniqueRecords;
using Dev2.Activities.Designers2.Unzip;
using Dev2.Activities.Designers2.WCFEndPoint;
using Dev2.Activities.Designers2.Web_Service_Delete;
using Dev2.Activities.Designers2.Web_Service_Post;
using Dev2.Activities.Designers2.Web_Service_Put;
using Dev2.Activities.Designers2.WebServiceGet;
using Dev2.Activities.Designers2.WriteFile;
using Dev2.Activities.Designers2.XPath;
using Dev2.Activities.Designers2.Zip;
using Dev2.Activities.DropBox2016.DeleteActivity;
using Dev2.Activities.DropBox2016.DownloadActivity;
using Dev2.Activities.DropBox2016.DropboxFileActivity;
using Dev2.Activities.DropBox2016.UploadActivity;
using Dev2.Activities.Exchange;
using Dev2.Activities.RabbitMQ.Consume;
using Dev2.Activities.RabbitMQ.Publish;
using Dev2.Activities.SelectAndApply;
using Dev2.Activities.Sharepoint;
using Dev2.Activities.WcfEndPoint;
using Dev2.Studio.ViewModels.Workflow;
using System;
using System.Activities.Presentation;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using Dev2.Activities.Designers2.ComDLL;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Dev2.Activities.Designers2.MultiAssignObject;
using Dev2.Activities.Scripting;
using Dev2.Activities.DateAndTime;
using Dev2.Activities.Designers2.CountRecords;
using Dev2.Activities.Designers2.RecordsLength;
using Dev2.Activities.Designers2.DeleteRecords;
using Dev2.Activities.Designers2.DateTime;
using Dev2.Activities.Designers2.DateTimeDifference;
using Dev2.Activities.Designers2.Net_DLL;
using Dev2.Activities.Designers2.Net_Dll_Enhanced;
using Dev2.Activities.Designers2.Web_Service_Get;
using Dev2.Activities.Designers2.Script_Javascript;
using Dev2.Activities.Designers2.Script_Ruby;
using Dev2.Activities.Designers2.Script_Python;

namespace Dev2.Studio.ActivityDesigners
{
    public static class ActivityDesignerHelper
    {
        public static readonly Dictionary<Type, Type> DesignerAttributes = new Dictionary<Type, Type>
            {
                { typeof(DsfMultiAssignActivity), typeof(MultiAssignDesignerViewModel) },
                { typeof(DsfMultiAssignObjectActivity), typeof(MultiAssignObjectDesignerViewModel) },
                { typeof(DsfDateTimeActivity), typeof(DateTimeDesignerViewModel) },
            { typeof(DsfWebGetRequestWithTimeoutActivity), typeof(GetWebRequestDesignerViewModel) },
                { typeof(DsfWebGetRequestActivity), typeof(GetWebRequestDesignerViewModel) },
                { typeof(DsfFindRecordsMultipleCriteriaActivity), typeof(FindRecordsMultipleCriteriaDesignerViewModel) },
                { typeof(DsfSqlBulkInsertActivity), typeof(SqlBulkInsertDesignerViewModel) },
                { typeof(DsfSortRecordsActivity), typeof(SortRecordsDesignerViewModel) },
                { typeof(DsfCountRecordsetNullHandlerActivity), typeof(CountRecordsDesignerViewModel) },
                { typeof(DsfCountRecordsetActivity), typeof(CountRecordsDesignerViewModel) },
                { typeof(DsfRecordsetLengthActivity), typeof(RecordsLengthDesignerViewModel) },
                { typeof(DsfRecordsetNullhandlerLengthActivity), typeof(RecordsLengthDesignerViewModel) },
                { typeof(DsfDeleteRecordNullHandlerActivity), typeof(DeleteRecordsDesignerViewModel) },
                { typeof(DsfDeleteRecordActivity), typeof(DeleteRecordsDesignerViewModel) },
            { typeof(DsfUniqueActivity), typeof(UniqueRecordsDesignerViewModel) },
                { typeof(DsfCalculateActivity), typeof(CalculateDesignerViewModel) },
                { typeof(DsfAggregateCalculateActivity), typeof(AggregateCalculateDesignerViewModel) },
                { typeof(DsfBaseConvertActivity), typeof(BaseConvertDesignerViewModel) },
                { typeof(DsfNumberFormatActivity), typeof(FormatNumberDesignerViewModel) },
                { typeof(DsfPathCopy), typeof(CopyDesignerViewModel) },
                { typeof(DsfPathCreate), typeof(CreateDesignerViewModel) },
                { typeof(DsfPathMove), typeof(MoveDesignerViewModel) },
                { typeof(DsfPathDelete), typeof(DeleteDesignerViewModel) },
                { typeof(DsfFileRead), typeof(ReadFileDesignerViewModel) },
                { typeof(DsfFileWrite), typeof(WriteFileDesignerViewModel) },
                { typeof(DsfFolderRead), typeof(ReadFolderDesignerViewModel) },
                { typeof(DsfPathRename), typeof(RenameDesignerViewModel) },
                { typeof(DsfUnZip), typeof(UnzipDesignerViewModel) },
                { typeof(DsfZip), typeof(ZipDesignerViewModel) },
                { typeof(DsfExecuteCommandLineActivity), typeof(CommandLineDesignerViewModel) },
                { typeof(DsfCommentActivity), typeof(CommentDesignerViewModel) },
                { typeof(DsfSequenceActivity), typeof(SequenceDesignerViewModel) },
                { typeof(DsfDateTimeDifferenceActivity), typeof(DateTimeDifferenceDesignerViewModel) },
                { typeof(DsfSendEmailActivity), typeof(EmailDesignerViewModel) },
                { typeof(DsfIndexActivity), typeof(FindIndexDesignerViewModel) },
                { typeof(DsfRandomActivity), typeof(RandomDesignerViewModel) },
                { typeof(DsfReplaceActivity), typeof(ReplaceDesignerViewModel) },
                { typeof(DsfScriptingActivity), typeof(ScriptDesignerViewModel) },
                { typeof(DsfJavascriptActivity), typeof(JavaScriptDesignerViewModel) },
                { typeof(DsfRubyActivity), typeof(RubyDesignerViewModel) },
                { typeof(DsfPythonActivity), typeof(PythonDesignerViewModel) },
                { typeof(DsfForEachActivity), typeof(ForeachDesignerViewModel) },
                { typeof(DsfCaseConvertActivity), typeof(CaseConvertDesignerViewModel) },
                { typeof(DsfDataMergeActivity), typeof(DataMergeDesignerViewModel) },
                { typeof(DsfDataSplitActivity), typeof(DataSplitDesignerViewModel) },
                { typeof(DsfGatherSystemInformationActivity), typeof(GatherSystemInformationDesignerViewModel) },
                { typeof(DsfXPathActivity), typeof(XPathDesignerViewModel) },
                { typeof(DsfActivity), typeof(ServiceDesignerViewModel) },
                { typeof(DsfSqlServerDatabaseActivity), typeof(SqlServerDatabaseDesignerViewModel) },
                { typeof(DsfMySqlDatabaseActivity), typeof(MySqlDatabaseDesignerViewModel) },
                { typeof(DsfOracleDatabaseActivity), typeof(OracleDatabaseDesignerViewModel) },
                  { typeof(DsfODBCDatabaseActivity), typeof(ODBCDatabaseDesignerViewModel) },
                  { typeof(DsfPostgreSqlActivity), typeof(PostgreSqlDatabaseDesignerViewModel) },
                {typeof(DsfExchangeEmailActivity),typeof(ExchangeEmailDesignerViewModel) },
                { typeof(DsfEnhancedDotNetDllActivity), typeof(DotNetDllEnhancedViewModel) },
                { typeof(DsfComDllActivity), typeof(ComDllViewModel) },
                { typeof(DsfWebGetActivity), typeof(WebServiceGetViewModel) },
                { typeof(DsfWebPostActivity), typeof(WebServicePostViewModel) },
                { typeof(DsfWebDeleteActivity), typeof(WebServiceDeleteViewModel) },
                { typeof(DsfWebPutActivity), typeof(WebServicePutViewModel) },
                { typeof(DsfDropBoxUploadActivity), typeof(DropBoxUploadViewModel) },
                { typeof(DsfDropBoxDownloadActivity), typeof(DropBoxDownloadViewModel) },
                 { typeof(DsfDropBoxDeleteActivity), typeof(DropBoxDeleteViewModel) },
                 { typeof(DsfDropboxFileListActivity), typeof(DropBoxFileListDesignerViewModel) },
                { typeof(DsfWebserviceActivity), typeof(ServiceDesignerViewModel) },
                { typeof(DsfPluginActivity), typeof(ServiceDesignerViewModel) },
                { typeof(DsfCreateJsonActivity), typeof(CreateJsonDesignerViewModel) },
                { typeof(SharepointReadListActivity), typeof(SharepointListReadDesignerViewModel) },
                { typeof(SharepointCreateListItemActivity), typeof(SharepointListCreateDesignerViewModel) },
                { typeof(SharepointDeleteListItemActivity), typeof(SharepointListDeleteDesignerViewModel) },
                { typeof(SharepointUpdateListItemActivity), typeof(SharepointListUpdateDesignerViewModel) },
                { typeof(SharepointReadFolderItemActivity), typeof(SharePointReadFolderDesignerViewModel) },
                { typeof(SharepointFileUploadActivity), typeof(SharePointFileUploadDesignerViewModel) },
                { typeof(SharepointFileDownLoadActivity), typeof(SharePointFileDownLoadDesignerViewModel) },
                { typeof(SharepointCopyFileActivity), typeof(SharePointCopyFileDesignerViewModel) },
                { typeof(SharepointDeleteFileActivity), typeof(SharePointDeleteFileDesignerViewModel) },
                { typeof(SharepointMoveFileActivity), typeof(SharePointMoveFileDesignerViewModel) },
                { typeof(DsfWcfEndPointActivity),typeof(WcfEndPointViewModel)},
                { typeof(DsfPublishRabbitMQActivity), typeof(RabbitMQPublishDesignerViewModel) },
                { typeof(DsfSelectAndApplyActivity), typeof(SelectAndApplyDesignerViewModel) },
                { typeof(DsfConsumeRabbitMQActivity), typeof(RabbitMQConsumeDesignerViewModel) },
            };
        private static Hashtable _hashTable;

        public static Hashtable GetDesignerHashTable()
        {
            if (_hashTable == null)
            {
                _hashTable = new Hashtable
                {
                    {WorkflowDesignerColors.FontFamilyKey, Application.Current.Resources["DefaultFontFamily"]},
                    {WorkflowDesignerColors.FontWeightKey, Application.Current.Resources["DefaultFontWeight"]},
                    {WorkflowDesignerColors.RubberBandRectangleColorKey, Application.Current.Resources["DesignerBackground"]},
                    {
                        WorkflowDesignerColors.WorkflowViewElementBackgroundColorKey,
                        Application.Current.Resources["WorkflowBackgroundBrush"]
                    },
                    {
                        WorkflowDesignerColors.WorkflowViewElementSelectedBackgroundColorKey,
                        Application.Current.Resources["WorkflowBackgroundBrush"]
                    },
                    {
                        WorkflowDesignerColors.WorkflowViewElementSelectedBorderColorKey,
                        Application.Current.Resources["WorkflowSelectedBorderBrush"]
                    },
                    {
                        WorkflowDesignerColors.DesignerViewShellBarControlBackgroundColorKey,
                        Application.Current.Resources["ShellBarViewBackground"]
                    },
                    {
                        WorkflowDesignerColors.DesignerViewShellBarColorGradientBeginKey,
                        Application.Current.Resources["ShellBarViewBackground"]
                    },
                    {
                        WorkflowDesignerColors.DesignerViewShellBarColorGradientEndKey,
                        Application.Current.Resources["ShellBarViewBackground"]
                    },
                    {WorkflowDesignerColors.OutlineViewItemSelectedTextColorKey, Application.Current.Resources["SolidWhite"]},
                    {
                        WorkflowDesignerColors.OutlineViewItemHighlightBackgroundColorKey,
                        Application.Current.Resources["DesignerBackground"]
                    },
                };
            }
            return _hashTable;
        }

        public static void AddDesignerAttributes(WorkflowDesignerViewModel workflowVm) => AddDesignerAttributes(workflowVm, false);
        public static void AddDesignerAttributes(WorkflowDesignerViewModel workflowVm, bool liteInit)
        {
            workflowVm.InitializeDesigner(DesignerAttributes, liteInit);
        }
    }
}
