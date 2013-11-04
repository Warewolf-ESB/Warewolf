using System;
using System.Collections.Generic;
using Dev2.Activities;
using Dev2.Activities.Designers2.BaseConvert;
using Dev2.Activities.Designers2.Calculate;
using Dev2.Activities.Designers2.CommandLine;
using Dev2.Activities.Designers2.Comment;
using Dev2.Activities.Designers2.Copy;
using Dev2.Activities.Designers2.CountRecords;
using Dev2.Activities.Designers2.Create;
using Dev2.Activities.Designers2.DateTime;
using Dev2.Activities.Designers2.DateTimeDifference;
using Dev2.Activities.Designers2.Delete;
using Dev2.Activities.Designers2.DeleteRecords;
using Dev2.Activities.Designers2.Email;
using Dev2.Activities.Designers2.FindIndex;
using Dev2.Activities.Designers2.FindRecordsMultipleCriteria;
using Dev2.Activities.Designers2.FormatNumber;
using Dev2.Activities.Designers2.GetWebRequest;
using Dev2.Activities.Designers2.Move;
using Dev2.Activities.Designers2.MultiAssign;
using Dev2.Activities.Designers2.Random;
using Dev2.Activities.Designers2.ReadFile;
using Dev2.Activities.Designers2.ReadFolder;
using Dev2.Activities.Designers2.Rename;
using Dev2.Activities.Designers2.Replace;
using Dev2.Activities.Designers2.Script;
using Dev2.Activities.Designers2.Service;
using Dev2.Activities.Designers2.SortRecords;
using Dev2.Activities.Designers2.SqlBulkInsert;
using Dev2.Activities.Designers2.UniqueRecords;
using Dev2.Activities.Designers2.Unzip;
using Dev2.Activities.Designers2.WriteFile;
using Dev2.Activities.Designers2.Zip;
using Dev2.Studio.ViewModels.Workflow;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Studio.ActivityDesigners
{
    public static class ActivityDesignerHelper
    {
        public static void AddDesignerAttributes(WorkflowDesignerViewModel workflowVm)
        {
            var designerAttributes = new Dictionary<Type, Type>
            {
                { typeof(DsfMultiAssignActivity), typeof(MultiAssignDesigner) },
                { typeof(DsfDateTimeActivity), typeof(DateTimeDesigner) },
                { typeof(DsfWebGetRequestActivity), typeof(GetWebRequestDesigner) },
                { typeof(DsfFindRecordsMultipleCriteriaActivity), typeof(FindRecordsMultipleCriteriaDesigner) },
                { typeof(DsfSqlBulkInsertActivity), typeof(SqlBulkInsertDesigner) },
                { typeof(DsfSortRecordsActivity), typeof(SortRecordsDesigner) },
                { typeof(DsfCountRecordsetActivity), typeof(CountRecordsDesigner) },
                { typeof(DsfDeleteRecordActivity), typeof(DeleteRecordsDesigner) },
                { typeof(DsfUniqueActivity), typeof(UniqueRecordsDesigner) },
                { typeof(DsfCalculateActivity), typeof(CalculateDesigner) },
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
                { typeof(DsfDateTimeDifferenceActivity), typeof(DateTimeDifferenceDesigner) },
                { typeof(DsfSendEmailActivity), typeof(EmailDesigner) },
                { typeof(DsfIndexActivity), typeof(FindIndexDesigner) },
                { typeof(DsfRandomActivity), typeof(RandomDesigner) },
                { typeof(DsfReplaceActivity), typeof(ReplaceDesigner) },
                { typeof(DsfScriptingActivity), typeof(ScriptDesigner) },

                //{ typeof(DsfActivity), typeof(DsfActivityDesigner) },
                //{ typeof(DsfDatabaseActivity), typeof(DsfActivityDesigner) },
                { typeof(DsfActivity), typeof(ServiceDesigner) },
                { typeof(DsfDatabaseActivity), typeof(ServiceDesigner) },
                //{ typeof(DsfCommentActivity), typeof(DsfCommentActivityDesigner) },
                { typeof(DsfAssignActivity), typeof(DsfAssignActivityDesigner) },
                //{typeof (TransformActivity), typeof (DsfTransformActivityDesigner)},
                { typeof(DsfForEachActivity), typeof(DsfForEachActivityDesigner) },
                { typeof(DsfWebPageActivity), typeof(DsfWebPageActivityDesigner) },
                { typeof(DsfWebSiteActivity), typeof(DsfWebSiteActivityDesigner) },
                { typeof(DsfDataSplitActivity), typeof(DsfDataSplitActivityDesigner) },
                //{ typeof(DsfPathCreate), typeof(DsfPathCreateDesigner) },
                //{ typeof(DsfFileRead), typeof(DsfFileReadDesigner) },
                //{ typeof(DsfFileWrite), typeof(DsfFileWriteDesigner) },
                // { typeof(DsfFolderRead), typeof(DsfFolderReadDesigner) },
                //{ typeof(DsfPathCopy), typeof(DsfPathCopyDesigner) },
                //{ typeof(DsfPathDelete), typeof(DsfPathDeleteDesigner) },
                //{ typeof(DsfPathMove), typeof(DsfPathMoveDesigner) },
                //{ typeof(DsfPathRename), typeof(DsfPathRenameDesigner) },
                //{ typeof(DsfZip), typeof(DsfZipDesigner) },
                //{ typeof(DsfUnZip), typeof(DsfUnzipDesigner) },
                //{ typeof(DsfDateTimeDifferenceActivity), typeof(DsfDateTimeDifferenceActivityDesigner) },
                { typeof(DsfCaseConvertActivity), typeof(DsfCaseConvertActivityDesigner) },
                //{ typeof(DsfReplaceActivity), typeof(DsfReplaceActivityDesigner) },
                //{ typeof(DsfIndexActivity), typeof(DsfIndexActivityDesigner) },
                { typeof(DsfDataMergeActivity), typeof(DsfDataMergeActivityDesigner) },
                //                    {typeof (DsfRemoveActivity), typeof (DsfRemoveActivityDesigner)},
                //                    {typeof (DsfTagCountActivity), typeof (DsfTagCountActivityDesigner)},
                //                    {typeof (AssertActivity), typeof (DsfAssertActivityDesigner)},
                //                    {typeof (DsfFileForEachActivity), typeof (DsfFileForEachActivityDesigner)},
                //                    {typeof (DsfCheckpointActivity), typeof (DsfCheckpointActivityDesigner)},
                { typeof(DsfFindRecordsActivity), typeof(DsfFindRecordsActivityDesigner) },
                //{ typeof(DsfNumberFormatActivity), typeof(DsfNumberFormatActivityDesigner) },
                //{ typeof(DsfExecuteCommandLineActivity), typeof(DsfExecuteCommandLineActivityDesigner) },
                { typeof(DsfGatherSystemInformationActivity), typeof(DsfGatherSystemInformationActivityDesigner) },
                //{ typeof(DsfRandomActivity), typeof(DsfRandomActivityDesigner) },
                //{ typeof(DsfSendEmailActivity), typeof(DsfSendEmailActivityDesigner) },
                { typeof(DsfScriptingJavaScriptActivity), typeof(DsfScriptingJavaScriptDesigner) },
                //{ typeof(DsfScriptingActivity), typeof(DsfScriptingActivityDesigner) },
                { typeof(DsfXPathActivity), typeof(DsfXPathActivityDesigner) },
                // Travis.Frisinger : 25.09.2012 - Removed Http Activity as it is out of sync with the current release 1 plans
            };

            workflowVm.InitializeDesigner(designerAttributes);
        }
    }
}
