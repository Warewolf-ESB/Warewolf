/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

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
using Dev2.Activities.DateAndTime;
using Dev2.Activities.Designers2.ExchangeNewEmail;
using Dev2.Activities.Designers2.ReadFolderNew;
using Dev2.Activities.Designers2.AdvancedRecordset;
using Dev2.Activities.Designers2.DeleteRecords;
using Dev2.Activities.RedisCache;
using Dev2.Activities.Designers2.RedisCache;
using Dev2.Activities.RedisRemove;
using Dev2.Activities.Designers2.RedisRemove;
using Dev2.Activities.Designers2.RedisCounter;
using Dev2.Activities.RedisCounter;
using Dev2.Activities.Designers2.Gate;
using Dev2.Activities.Designers2.RabbitMQ.Publish2;
using Dev2.Activities.Designers2.ReadFileWithBase64;

namespace Dev2
{
    public static class DesignerAttributeMap
    {
        public static readonly Dictionary<Type, Type> DesignerAttributes = new Dictionary<Type, Type>
            {
                { typeof(DsfMultiAssignActivity), typeof(MultiAssignDesignerViewModel) },
                { typeof(DsfMultiAssignObjectActivity), typeof(MultiAssignObjectDesignerViewModel) },
                { typeof(DsfDotNetMultiAssignActivity), typeof(MultiAssignDesignerViewModel) },
                { typeof(DsfDotNetMultiAssignObjectActivity), typeof(MultiAssignObjectDesignerViewModel) },
                { typeof(DsfDateTimeActivity), typeof(DateTimeDesignerViewModel) },
                { typeof(DsfDotNetDateTimeActivity), typeof(DateTimeDesignerViewModel) },
                { typeof(DsfWebGetRequestWithTimeoutActivity), typeof(GetWebRequestDesignerViewModel) },
                { typeof(DsfWebGetRequestActivity), typeof(GetWebRequestDesignerViewModel) },
                { typeof(DsfFindRecordsMultipleCriteriaActivity), typeof(FindRecordsMultipleCriteriaDesignerViewModel) },
                { typeof(DsfSqlBulkInsertActivity), typeof(SqlBulkInsertDesignerViewModel) },
                { typeof(DsfSortRecordsActivity), typeof(SortRecordsDesignerViewModel) },
                { typeof(DsfCountRecordsetNullHandlerActivity), typeof(CountRecordsDesignerViewModel) },
                { typeof(DsfCountRecordsetActivity), typeof(CountRecordsDesignerViewModel) },
                { typeof(DsfRecordsetLengthActivity), typeof(RecordsLengthDesignerViewModel) },
                { typeof(DsfRecordsetNullhandlerLengthActivity), typeof(RecordsLengthDesignerViewModel) },
                { typeof(DsfDeleteRecordNullHandlerActivity), typeof(DeleteRecordsNullHandlerDesignerViewModel) },
                { typeof(DsfDeleteRecordActivity), typeof(DeleteRecordsDesignerViewModel) },
                { typeof(DsfUniqueActivity), typeof(UniqueRecordsDesignerViewModel) },
                { typeof(DsfCalculateActivity), typeof(CalculateDesignerViewModel) },
                { typeof(DsfAggregateCalculateActivity), typeof(AggregateCalculateDesignerViewModel) },
                { typeof(DsfDotNetCalculateActivity), typeof(CalculateDesignerViewModel) },
                { typeof(DsfDotNetAggregateCalculateActivity), typeof(AggregateCalculateDesignerViewModel) },
                { typeof(DsfBaseConvertActivity), typeof(BaseConvertDesignerViewModel) },
                { typeof(DsfNumberFormatActivity), typeof(FormatNumberDesignerViewModel) },
                { typeof(DsfPathCopy), typeof(CopyDesignerViewModel) },
                { typeof(DsfPathCreate), typeof(CreateDesignerViewModel) },
                { typeof(DsfPathMove), typeof(MoveDesignerViewModel) },
                { typeof(DsfPathDelete), typeof(DeleteDesignerViewModel) },
                { typeof(DsfFileRead), typeof(ReadFileDesignerViewModel) },
                { typeof(FileReadWithBase64), typeof(ReadFileWithBase64DesignerViewModel) },
                { typeof(DsfFileWrite), typeof(WriteFileDesignerViewModel) },
                { typeof(DsfFileWriteWithBase64), typeof(WriteFileDesignerViewModel) },
                { typeof(DsfFolderRead), typeof(ReadFolderDesignerViewModel) },
                { typeof(DsfFolderReadActivity), typeof(ReadFolderNewDesignerViewModel) },
                { typeof(DsfPathRename), typeof(RenameDesignerViewModel) },
                { typeof(DsfUnZip), typeof(UnzipDesignerViewModel) },
                { typeof(DsfZip), typeof(ZipDesignerViewModel) },
                { typeof(DsfExecuteCommandLineActivity), typeof(CommandLineDesignerViewModel) },
                { typeof(DsfCommentActivity), typeof(CommentDesignerViewModel) },
                { typeof(DsfSequenceActivity), typeof(SequenceDesignerViewModel) },
                { typeof(DsfDateTimeDifferenceActivity), typeof(DateTimeDifferenceDesignerViewModel) },
                { typeof(DsfDotNetDateTimeDifferenceActivity), typeof(DateTimeDifferenceDesignerViewModel) },
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
                { typeof(DsfDotNetGatherSystemInformationActivity), typeof(GatherSystemInformationDesignerViewModel) },
                { typeof(DsfXPathActivity), typeof(XPathDesignerViewModel) },
                { typeof(DsfActivity), typeof(ServiceDesignerViewModel) },
                { typeof(DsfSqlServerDatabaseActivity), typeof(SqlServerDatabaseDesignerViewModel) },
                { typeof(DsfMySqlDatabaseActivity), typeof(MySqlDatabaseDesignerViewModel) },
                { typeof(DsfOracleDatabaseActivity), typeof(OracleDatabaseDesignerViewModel) },
                { typeof(DsfODBCDatabaseActivity), typeof(ODBCDatabaseDesignerViewModel) },
                { typeof(DsfPostgreSqlActivity), typeof(PostgreSqlDatabaseDesignerViewModel) },
                { typeof(DsfExchangeEmailActivity), typeof(ExchangeEmailDesignerViewModel) },
                { typeof(DsfExchangeEmailNewActivity), typeof(ExchangeNewEmailDesignerViewModel) },
                { typeof(DsfEnhancedDotNetDllActivity), typeof(DotNetDllEnhancedViewModel) },
                { typeof(DsfComDllActivity), typeof(ComDllViewModel) },
                { typeof(DsfWebGetActivity), typeof(WebServiceGetViewModel) },
                { typeof(DsfWebGetActivityWithBase64), typeof(WebServiceGetViewModel) },
                { typeof(DsfWebPostActivity), typeof(WebServicePostViewModel) },
                { typeof(DsfWebDeleteActivity), typeof(WebServiceDeleteViewModel) },
                { typeof(DsfWebPutActivity), typeof(WebServicePutViewModel) },
                { typeof(DsfDropBoxUploadActivity), typeof(DropBoxUploadViewModel) },
                { typeof(DsfDropBoxDownloadActivity), typeof(DropBoxDownloadViewModel) },
                { typeof(DsfDropBoxDeleteActivity), typeof(DropBoxDeleteViewModel) },
                { typeof(DsfDropboxFileListActivity), typeof(DropBoxFileListDesignerViewModel) },
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
                { typeof(PublishRabbitMQActivity), typeof(RabbitMQPublishDesignerViewModel2) },
                { typeof(DsfPublishRabbitMQActivity), typeof(RabbitMQPublishDesignerViewModel) },
                { typeof(DsfSelectAndApplyActivity), typeof(SelectAndApplyDesignerViewModel) },
                { typeof(DsfConsumeRabbitMQActivity), typeof(RabbitMQConsumeDesignerViewModel) },
                { typeof(DsfDecision), typeof(DecisionDesignerViewModel) },
                { typeof(DsfSwitch), typeof(SwitchDesignerViewModel) },
                { typeof(AdvancedRecordsetActivity), typeof(AdvancedRecordsetDesignerViewModel) },
                { typeof(RedisCacheActivity), typeof(RedisCacheDesigner) },
                { typeof(RedisRemoveActivity), typeof(RedisRemoveDesigner) },
                { typeof(RedisCounterActivity), typeof(RedisCounterDesigner) },
                { typeof(GateActivity), typeof(GateDesignerViewModel) }
            };
    }
}
