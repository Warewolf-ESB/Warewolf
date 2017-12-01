using System;
using System.Collections.Generic;
using Dev2.Activities;
using Dev2.Activities.Designers2.AggregateCalculate;
using Dev2.Activities.Designers2.BaseConvert;
using Dev2.Activities.Designers2.Calculate;
using Dev2.Activities.Designers2.CaseConvert;
using Dev2.Activities.Designers2.ComDLL;
using Dev2.Activities.Designers2.CommandLine;
using Dev2.Activities.Designers2.Comment;
using Dev2.Activities.Designers2.Copy;
using Dev2.Activities.Designers2.CountRecordsNullHandler;
using Dev2.Activities.Designers2.Create;
using Dev2.Activities.Designers2.CreateJSON;
using Dev2.Activities.Designers2.DataMerge;
using Dev2.Activities.Designers2.DataSplit;
using Dev2.Activities.Designers2.DateTime;
using Dev2.Activities.Designers2.DateTimeDifference;
using Dev2.Activities.Designers2.Delete;
using Dev2.Activities.Designers2.DeleteRecordsNullHandler;
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
using Dev2.Activities.Designers2.Move;
using Dev2.Activities.Designers2.MultiAssign;
using Dev2.Activities.Designers2.MultiAssignObject;
using Dev2.Activities.Designers2.MySqlDatabase;
using Dev2.Activities.Designers2.Net_Dll_Enhanced;
using Dev2.Activities.Designers2.ODBC;
using Dev2.Activities.Designers2.Oracle;
using Dev2.Activities.Designers2.PostgreSql;
using Dev2.Activities.Designers2.RabbitMQ.Consume;
using Dev2.Activities.Designers2.RabbitMQ.Publish;
using Dev2.Activities.Designers2.Random;
using Dev2.Activities.Designers2.ReadFile;
using Dev2.Activities.Designers2.ReadFolder;
using Dev2.Activities.Designers2.RecordsLengthNullHandler;
using Dev2.Activities.Designers2.Rename;
using Dev2.Activities.Designers2.Replace;
using Dev2.Activities.Designers2.Script;
using Dev2.Activities.Designers2.Script_Javascript;
using Dev2.Activities.Designers2.Script_Python;
using Dev2.Activities.Designers2.Script_Ruby;
using Dev2.Activities.Designers2.SelectAndApply;
using Dev2.Activities.Designers2.Sequence;
using Dev2.Activities.Designers2.Service;
using Dev2.Activities.Designers2.SharepointFolderRead;
using Dev2.Activities.Designers2.SharepointListCreate;
using Dev2.Activities.Designers2.SharepointListDelete;
using Dev2.Activities.Designers2.SharepointListRead;
using Dev2.Activities.Designers2.SharepointListUpdate;
using Dev2.Activities.Designers2.SharePointCopyFile;
using Dev2.Activities.Designers2.SharePointDeleteFile;
using Dev2.Activities.Designers2.SharePointFileDownload;
using Dev2.Activities.Designers2.SharePointFileUpload;
using Dev2.Activities.Designers2.SharePointMoveFile;
using Dev2.Activities.Designers2.SortRecords;
using Dev2.Activities.Designers2.SqlBulkInsert;
using Dev2.Activities.Designers2.SqlServerDatabase;
using Dev2.Activities.Designers2.UniqueRecords;
using Dev2.Activities.Designers2.Unzip;
using Dev2.Activities.Designers2.WCFEndPoint;
using Dev2.Activities.Designers2.Web_Service_Delete;
using Dev2.Activities.Designers2.Web_Service_Get;
using Dev2.Activities.Designers2.Web_Service_Post;
using Dev2.Activities.Designers2.Web_Service_Put;
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
using Dev2.Activities.Scripting;
using Dev2.Activities.SelectAndApply;
using Dev2.Activities.Sharepoint;
using Dev2.Activities.WcfEndPoint;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Dev2.Activities.Designers2.Decision;
using Dev2.Activities.Designers2.Switch;
using Dev2.Activities.Designers2.WebServiceGet;
using Dev2.Activities.Designers2.GetWebRequest.GetWebRequestWithTimeout;
using Dev2.Activities.DateAndTime;

namespace Dev2
{
    public static class DesignerAttributeMap
    {
        
        public static readonly Dictionary<Type, Type> DesignerAttributes = new Dictionary<Type, Type>
            {
                { typeof(DsfDotNetMultiAssignActivity), typeof(MultiAssignDesigner) },
                { typeof(DsfDotNetMultiAssignObjectActivity), typeof(MultiAssignObjectDesigner) },{ typeof(DsfMultiAssignActivity), typeof(MultiAssignDesigner) },
                { typeof(DsfMultiAssignObjectActivity), typeof(MultiAssignObjectDesigner) },
                { typeof(DsfWebGetRequestWithTimeoutActivity), typeof(GetWebRequestWithTimeOutDesigner) },
                { typeof(DsfWebGetRequestActivity), typeof(GetWebRequestDesigner) },
                { typeof(DsfFindRecordsMultipleCriteriaActivity), typeof(FindRecordsMultipleCriteriaDesigner) },
                { typeof(DsfSqlBulkInsertActivity), typeof(SqlBulkInsertDesigner) },
                { typeof(DsfSortRecordsActivity), typeof(SortRecordsDesigner) },
                { typeof(DsfCountRecordsetNullHandlerActivity), typeof(CountRecordsDesigner) },
                { typeof(DsfCountRecordsetActivity), typeof(CountRecordsDesigner) },
                { typeof(DsfRecordsetLengthActivity), typeof(RecordsLengthDesigner) },
                { typeof(DsfRecordsetNullhandlerLengthActivity), typeof(RecordsLengthDesigner) },
                { typeof(DsfDeleteRecordNullHandlerActivity), typeof(DeleteRecordsDesigner) },
                { typeof(DsfDeleteRecordActivity), typeof(DeleteRecordsDesigner) },
                { typeof(DsfDotNetDateTimeActivity),typeof(Activities.Designers2.DateTimStandard.DateTimeDesigner) },
                { typeof(DsfDotNetDateTimeDifferenceActivity),typeof(Activities.Designers2.DateTimeDifferenceStandard.DateTimeDifferenceDesigner) },
                { typeof(DsfDateTimeActivity),typeof(DateTimeDesigner) },
                { typeof(DsfDateTimeDifferenceActivity),typeof(DateTimeDifferenceDesigner) },
                { typeof(DsfDotNetGatherSystemInformationActivity),typeof(GatherSystemInformationDesigner) },
                { typeof(DsfUniqueActivity), typeof(UniqueRecordsDesigner) },
                { typeof(DsfCalculateActivity), typeof(CalculateDesigner) },
                { typeof(DsfAggregateCalculateActivity), typeof(AggregateCalculateDesigner) },
                { typeof(DsfDotNetCalculateActivity), typeof(CalculateDesigner) },
                { typeof(DsfDotNetAggregateCalculateActivity), typeof(AggregateCalculateDesigner) },
                { typeof(DsfBaseConvertActivity), typeof(BaseConvertDesigner) },
                { typeof(DsfNumberFormatActivity), typeof(FormatNumberDesigner) },
                { typeof(DsfPathCopy), typeof(CopyDesigner) },
                { typeof(DsfPathCreate), typeof(CreateDesigner) },
                { typeof(DsfPathMove), typeof(MoveDesigner) },
                { typeof(DsfPathDelete), typeof(DeleteDesigner) },
                { typeof(DsfFileRead), typeof(ReadFileDesigner) },
                { typeof(DsfFileWrite), typeof(WriteFileDesigner) },
                { typeof(DsfFolderRead), typeof(ReadFolderDesigner) },
                { typeof(DsfPathRename), typeof(RenameDesigner) },
                { typeof(DsfUnZip), typeof(UnzipDesigner) },
                { typeof(DsfZip), typeof(ZipDesigner) },
                { typeof(DsfExecuteCommandLineActivity), typeof(CommandLineDesigner) },
                { typeof(DsfCommentActivity), typeof(CommentDesigner) },
                { typeof(DsfSequenceActivity), typeof(SequenceDesigner) },
                { typeof(DsfSendEmailActivity), typeof(EmailDesigner) },
                { typeof(DsfIndexActivity), typeof(FindIndexDesigner) },
                { typeof(DsfRandomActivity), typeof(RandomDesigner) },
                { typeof(DsfReplaceActivity), typeof(ReplaceDesigner) },
                { typeof(DsfScriptingActivity), typeof(Activities.Designers2.Script.ScriptDesigner) },
                { typeof(DsfJavascriptActivity), typeof(Activities.Designers2.Script_Javascript.ScriptDesigner) },
                { typeof(DsfRubyActivity), typeof(Activities.Designers2.Script_Ruby.ScriptDesigner) },
                { typeof(DsfPythonActivity), typeof(Activities.Designers2.Script_Python.ScriptDesigner) },
                { typeof(DsfForEachActivity), typeof(ForeachDesigner) },
                { typeof(DsfCaseConvertActivity), typeof(CaseConvertDesigner) },
                { typeof(DsfDataMergeActivity), typeof(DataMergeDesigner) },
                { typeof(DsfDataSplitActivity), typeof(DataSplitDesigner) },
                { typeof(DsfXPathActivity), typeof(XPathDesigner) },
                { typeof(DsfActivity), typeof(ServiceDesigner) },
                { typeof(DsfSqlServerDatabaseActivity), typeof(SqlServerDatabaseDesigner) },
                { typeof(DsfMySqlDatabaseActivity), typeof(MySqlDatabaseDesigner) },
                { typeof(DsfOracleDatabaseActivity), typeof(OracleDatabaseDesigner) },
                { typeof(DsfODBCDatabaseActivity), typeof(ODBCDatabaseDesigner) },
                { typeof(DsfPostgreSqlActivity), typeof(PostgreSqlDatabaseDesigner) },
                { typeof(DsfExchangeEmailActivity),typeof(ExchangeEmailDesigner) },
                { typeof(DsfDotNetDllActivity), typeof(Activities.Designers2.Net_DLL.DotNetDllDesigner) },
                { typeof(DsfEnhancedDotNetDllActivity), typeof(DotNetDllDesigner) },
                { typeof(DsfComDllActivity), typeof(ComDllDesigner) },
                { typeof(DsfWebGetActivity), typeof(WebServiceGetDesigner) },
                { typeof(DsfWebPostActivity), typeof(WebServicePostDesigner) },
                { typeof(DsfWebDeleteActivity), typeof(WebServiceDeleteDesigner) },
                { typeof(DsfWebPutActivity), typeof(WebServicePutDesigner) },
                { typeof(DsfDropBoxUploadActivity), typeof(DropBoxUploadDesigner) },
                { typeof(DsfDropBoxDownloadActivity), typeof(DropBoxDownloadDesigner) },
                { typeof(DsfDropBoxDeleteActivity), typeof(DropBoxDeleteDesigner) },
                { typeof(DsfDropboxFileListActivity), typeof(DropBoxFileListDesigner) },
                { typeof(DsfWebserviceActivity), typeof(ServiceDesigner) },
                { typeof(DsfPluginActivity), typeof(ServiceDesigner) },
                { typeof(DsfCreateJsonActivity), typeof(CreateJsonDesigner) },
                { typeof(SharepointReadListActivity), typeof(SharepointListReadDesigner) },
                { typeof(SharepointCreateListItemActivity), typeof(SharepointListCreateDesigner) },
                { typeof(SharepointDeleteListItemActivity), typeof(SharepointListDeleteDesigner) },
                { typeof(SharepointUpdateListItemActivity), typeof(SharepointListUpdateDesigner) },
                { typeof(SharepointReadFolderItemActivity), typeof(SharePointReadFolderDesigner) },
                { typeof(SharepointFileUploadActivity), typeof(SharePointFileUploadDesigner) },
                { typeof(SharepointFileDownLoadActivity), typeof(SharePointFileDownLoadDesigner) },
                { typeof(SharepointCopyFileActivity), typeof(SharePointCopyFileDesigner) },
                { typeof(SharepointDeleteFileActivity), typeof(SharePointDeleteFileDesigner) },
                { typeof(SharepointMoveFileActivity), typeof(SharePointMoveFileDesigner) },
                { typeof(DsfWcfEndPointActivity),typeof(WcfEndPointDesigner)},
                { typeof(DsfPublishRabbitMQActivity), typeof(RabbitMQPublishDesigner) },
                { typeof(DsfSelectAndApplyActivity), typeof(SelectAndApplyDesigner) },
                { typeof(DsfConsumeRabbitMQActivity), typeof(RabbitMQConsumeDesigner) },
                { typeof(DsfDecision), typeof(DecisionDesignerViewModel) },
                { typeof(DsfSwitch), typeof(SwitchDesignerViewModel) },
            };
    }
}
