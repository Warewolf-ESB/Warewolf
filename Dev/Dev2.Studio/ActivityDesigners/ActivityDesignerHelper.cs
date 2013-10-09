using System;
using System.Collections.Generic;
using Dev2.Activities;
using Dev2.Activities.Designers2.DateTime;
using Dev2.Activities.Designers2.FindRecordsMultipleCriteria;
using Dev2.Activities.Designers2.GetWebRequest;
using Dev2.Activities.Designers2.MultiAssign;
using Dev2.Activities.Designers2.SqlBulkInsert;
using Dev2.Studio.ActivityDesigners.Scripting;
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
                    {typeof (DsfMultiAssignActivity), typeof (MultiAssignDesigner)},
                    {typeof (DsfDateTimeActivity), typeof (DateTimeDesigner)},
                    {typeof (DsfWebGetRequestActivity), typeof (GetWebRequestDesigner)},
                    {typeof (DsfFindRecordsMultipleCriteriaActivity), typeof (FindRecordsMultipleCriteriaDesigner)},
                    {typeof (DsfSqlBulkInsertActivity), typeof (SqlBulkInsertDesigner)},

                    {typeof (DsfActivity), typeof (DsfActivityDesigner)},
                    {typeof (DsfDatabaseActivity), typeof (DsfActivityDesigner)},
                    {typeof (DsfCommentActivity), typeof (DsfCommentActivityDesigner)},
                    {typeof (DsfAssignActivity), typeof (DsfAssignActivityDesigner)},
                    //{typeof (TransformActivity), typeof (DsfTransformActivityDesigner)},
                    {typeof (DsfForEachActivity), typeof (DsfForEachActivityDesigner)},
                    {typeof (DsfWebPageActivity), typeof (DsfWebPageActivityDesigner)},
                    {typeof (DsfWebSiteActivity), typeof (DsfWebSiteActivityDesigner)},
                    {typeof (DsfCountRecordsetActivity), typeof (DsfCountRecordsetActivityDesigner)},
                    {typeof (DsfSortRecordsActivity), typeof (DsfSortRecordsActivityDesigner)},
                    {typeof (DsfDataSplitActivity), typeof (DsfDataSplitActivityDesigner)},
                    {typeof (DsfPathCreate), typeof (DsfPathCreateDesigner)},
                    {typeof (DsfFileRead), typeof (DsfFileReadDesigner)},
                    {typeof (DsfFileWrite), typeof (DsfFileWriteDesigner)},
                    {typeof (DsfFolderRead), typeof (DsfFolderReadDesigner)},
                    {typeof (DsfPathCopy), typeof (DsfPathCopyDesigner)},
                    {typeof (DsfPathDelete), typeof (DsfPathDeleteDesigner)},
                    {typeof (DsfPathMove), typeof (DsfPathMoveDesigner)},
                    {typeof (DsfPathRename), typeof (DsfPathRenameDesigner)},
                    {typeof (DsfZip), typeof (DsfZipDesigner)},
                    {typeof (DsfUnZip), typeof (DsfUnzipDesigner)},
                    {typeof (DsfCalculateActivity), typeof (DsfCalculateActivityDesigner)},
                    {typeof (DsfDateTimeDifferenceActivity), typeof (DsfDateTimeDifferenceActivityDesigner)},
                    {typeof (DsfCaseConvertActivity), typeof (DsfCaseConvertActivityDesigner)},
                    {typeof (DsfBaseConvertActivity), typeof (DsfBaseConvertActivityDesigner)},
                    {typeof (DsfReplaceActivity), typeof (DsfReplaceActivityDesigner)},
                    {typeof (DsfIndexActivity), typeof (DsfIndexActivityDesigner)},
                    {typeof (DsfDeleteRecordActivity), typeof (DsfDeleteRecordActivityDesigner)},
                    {typeof (DsfDataMergeActivity), typeof (DsfDataMergeActivityDesigner)},
//                    {typeof (DsfRemoveActivity), typeof (DsfRemoveActivityDesigner)},
//                    {typeof (DsfTagCountActivity), typeof (DsfTagCountActivityDesigner)},
//                    {typeof (AssertActivity), typeof (DsfAssertActivityDesigner)},
//                    {typeof (DsfFileForEachActivity), typeof (DsfFileForEachActivityDesigner)},
//                    {typeof (DsfCheckpointActivity), typeof (DsfCheckpointActivityDesigner)},
                    {typeof (DsfFindRecordsActivity), typeof (DsfFindRecordsActivityDesigner)},
                    {typeof (DsfNumberFormatActivity), typeof (DsfNumberFormatActivityDesigner)},
                    {typeof (DsfExecuteCommandLineActivity), typeof (DsfExecuteCommandLineActivityDesigner)},
                    {typeof (DsfGatherSystemInformationActivity), typeof (DsfGatherSystemInformationActivityDesigner)},
                    {typeof (DsfRandomActivity), typeof (DsfRandomActivityDesigner)},
                    {typeof (DsfSendEmailActivity), typeof (DsfSendEmailActivityDesigner)},
                    {typeof (DsfScriptingJavaScriptActivity), typeof (DsfScriptingJavaScriptDesigner)},
                    {typeof (DsfScriptingActivity), typeof (DsfScriptingActivityDesigner)},
                    {typeof (DsfXPathActivity), typeof (DsfXPathActivityDesigner)},
                    {typeof (DsfUniqueActivity), typeof (DsfUniqueActivityDesigner)},
                    // Travis.Frisinger : 25.09.2012 - Removed Http Activity as it is out of sync with the current release 1 plans
                };

            workflowVm.InitializeDesigner(designerAttributes);
        }
    }
}
