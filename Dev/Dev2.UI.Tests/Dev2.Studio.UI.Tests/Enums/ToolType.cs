using System;
using System.ComponentModel;

namespace Dev2.Studio.UI.Tests.Enums
{
    public enum ToolType
    {
        [Description("FlowDecision")]
        [ToolboxName("Decision")]
        Decision,
        [Description("FlowSwitch")]
        [ToolboxName("Switch")]
        Switch,
        [Description("DsfForEachActivity")]
        [ToolboxName("For Each")]
        ForEach,
        [Description("DsfCountRecordsetActivity")]
        [ToolboxName("Count")]
        Count,
        [Description("DsfDeleteRecordActivity")]
        [ToolboxName("Delete")]
        Delete,
        [Description("DsfFindRecordsMultipleCriteriaActivity")]
        [ToolboxName("Find")]
        Find,
        [Description("DsfSortRecordsActivity")]
        [ToolboxName("Sort")]
        Sort,
        [Description("DsfUniqueActivity")]
        [ToolboxName("Unique")]
        Unique,
        [Description("DsfSqlBulkInsertActivity")]
        [ToolboxName("SQL Bulk Insert")]
        SqlBulkInsert,
        [Description("DsfMultiAssignActivity")]
        [ToolboxName("Assign")]
        Assign,
        [Description("DsfBaseConvertActivity")]
        [ToolboxName("Base Convert")]
        BaseConvert,
        [Description("DsfCaseConvertActivity")]
        [ToolboxName("Case Convert")]
        CaseConvert,
        [Description("DsfDataMergeActivity")]
        [ToolboxName("Data Merge")]
        DataMerge,
        [Description("DsfDataSplitActivity")]
        [ToolboxName("Data Split")]
        DataSplit,
        [Description("DsfIndexActivity")]
        [ToolboxName("Find Index")]
        FindIndex,
        [Description("DsfReplaceActivity")]
        [ToolboxName("Replace")]
        Replace,
        [Description("DsfWorkflowActivity")]
        [ToolboxName("Workflow")]
        Workflow,
        [Description("DsfServiceActivity")]
        [ToolboxName("Service")]
        Service,
        [Description("DsfCalculateActivity")]
        [ToolboxName("Calculate")]
        Calculate,
        [Description("DsfNumberFormatActivity")]
        [ToolboxName("Format Number")]
        FormatNumber,
        [Description("DsfRandomActivity")]
        [ToolboxName("Random")]
        Random,
        [Description("DsfDateTimeActivity")]
        [ToolboxName("Date and Time")]
        DateAndTime,
        [Description("DsfDateTimeDifferenceActivity")]
        [ToolboxName("Date and Time Difference")]
        DateAndTimeDifference,
        [Description("DsfSendEmailActivity")]
        [ToolboxName("Email")]
        Email,
        [Description("DsfGatherSystemInformationActivity")]
        [ToolboxName("System Information")]
        SystemInformation,
        [Description("DsfXPathActivity")]
        [ToolboxName("XPath")]
        XPath,
        [Description("DsfCommentActivity")]
        [ToolboxName("Comment")]
        Comment,
        [Description("DsfWebGetRequestActivity")]
        [ToolboxName("Web Request")]
        WebRequest,
        [Description("DsfPathCreate")]
        [ToolboxName("Create")]
        Create,
        [Description("DsfPathCopy")]
        [ToolboxName("Copy")]
        Copy,
        [Description("DsfPathMove")]
        [ToolboxName("Move")]
        Move,
        [Description("DsfPathDelete")]
        [ToolboxName("Delete")]
        DeleteFile,
        [Description("DsfPathFileRead")]
        [ToolboxName("Read File")]
        ReadFile,
        [Description("DsfPathFileWrite")]
        [ToolboxName("Write File")]
        WriteFile,
        [Description("DsfPathFolderRead")]
        [ToolboxName("Read Folder")]
        ReadFolder,
        [Description("DsfPathRename")]
        [ToolboxName("Rename")]
        Rename,
        [Description("DsfUnZip")]
        [ToolboxName("Unzip")]
        Unzip,
        [Description("DsfZip")]
        [ToolboxName("Zip")]
        Zip,
        [Description("DsfExecuteCommandLineActivity")]
        [ToolboxName("CMD Line")]
        CMDLine,
        [Description("DsfScriptingActivity")]
        [ToolboxName("Script")]
        Script,
    }

    public class ToolboxNameAttribute : Attribute
    {
        public ToolboxNameAttribute(string toolboxName)
        {
            ToolboxName = toolboxName;
        }

        public string ToolboxName { get; set; }
    }

}
