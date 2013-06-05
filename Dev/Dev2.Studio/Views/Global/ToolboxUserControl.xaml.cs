using System.Windows;
using System.Windows.Input;
using Dev2.Activities;
using System;
using System.Activities.Presentation.Toolbox;
using System.Activities.Statements;
using System.Linq;
using System.Windows.Controls;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Unlimited.Applications.BusinessDesignStudio.Views
{
    /// <summary>
    /// Interaction logic for ToolboxUserControl.xaml
    /// </summary>
    /// 
    public partial class ToolboxUserControl : UserControl
    {

        public ToolboxUserControl()
        {
            InitializeComponent();
            BuildToolbox();
        }

        public void BuildToolbox()
        {

            var category = GetToolboxCategoryByName("Control Flow");
            if (category != null)
            {
                category.Add(new ToolboxItemWrapper(typeof(DsfFlowDecisionActivity), "/Images/ToolDecision-32.png", "Decision"));
                //category.Add(new ToolboxItemWrapper(typeof(Sequence), "/Images/ToolSequence-32.png", "Sequence"));
                category.Add(new ToolboxItemWrapper(typeof(DsfFlowSwitchActivity), "/images/ToolSwitch-32.png", "Switch"));
            }


            category = GetToolboxCategoryByName("Loop Constructs");

            if (category != null)
            {
                category.Add(new ToolboxItemWrapper(typeof(DsfForEachActivity), "/images/ToolForEach-32.png", "For Each"));
            }


            category = GetToolboxCategoryByName("Recordset");
            if (category != null)
            {
                category.Add(new ToolboxItemWrapper(typeof(DsfCountRecordsetActivity), "/images/ToolCountRecords-32.png", "Count"));
                category.Add(new ToolboxItemWrapper(typeof(DsfDeleteRecordActivity), "/images/ToolDeleteRecord-32.png", "Delete"));
                category.Add(new ToolboxItemWrapper(typeof(DsfFindRecordsActivity), "/images/ToolFindRecords-32.png", "Find"));
                category.Add(new ToolboxItemWrapper(typeof(DsfSortRecordsActivity), "/images/ToolSortRecords-32.png", "Sort"));
            }


            category = GetToolboxCategoryByName("Utility");
            if (category != null)
            {
                category.Add(new ToolboxItemWrapper(typeof(DsfMultiAssignActivity), "/images/ToolAssign-32.png", "Assign"));
                category.Add(new ToolboxItemWrapper(typeof(DsfBaseConvertActivity), "/images/ToolBaseConversion-32.png", "Base Conversion"));
                category.Add(new ToolboxItemWrapper(typeof(DsfCalculateActivity), "/images/ToolCalculate-32.png", "Calculate"));
                category.Add(new ToolboxItemWrapper(typeof(DsfNumberFormatActivity), "/images/ToolFormatNumber-32.png", "Format Number"));
                category.Add(new ToolboxItemWrapper(typeof(DsfRandomActivity), "/images/ToolRandom-32.png", "Random"));
                category.Add(new ToolboxItemWrapper(typeof(DsfCaseConvertActivity), "/images/ToolCaseConversion-32.png", "Case Conversion"));                
                category.Add(new ToolboxItemWrapper(typeof(DsfDataMergeActivity), "/images/ToolDataMerge-32.png", "Data Merge"));
                category.Add(new ToolboxItemWrapper(typeof(DsfDataSplitActivity), "/images/ToolDataSplit-32.png", "Data Split"));
                category.Add(new ToolboxItemWrapper(typeof(DsfDateTimeActivity), "/images/ToolDateTime-32.png", "Date and Time"));
                category.Add(new ToolboxItemWrapper(typeof(DsfDateTimeDifferenceActivity), "/images/ToolDateTimeDifference-32.png", "Date and Time Difference"));
                category.Add(new ToolboxItemWrapper(typeof(DsfSendEmailActivity), "/images/ToolSendEmail-32.png", "Email"));
                
                // TODO PBI 8291
                //category.Add(new ToolboxItemWrapper(typeof(DsfWebPageActivity), "/images/User.png", "Human Interface"));
                category.Add(new ToolboxItemWrapper(typeof(DsfIndexActivity), "/images/ToolFindIndex-32.png", "Find Index"));
                
                category.Add(new ToolboxItemWrapper(typeof(DsfReplaceActivity), "/images/ToolReplace-32.png", "Replace"));
                category.Add(new ToolboxItemWrapper(typeof(DsfGatherSystemInformationActivity), "/images/ToolSystemInformation-32.png", "System Information"));
                category.Add(new ToolboxItemWrapper(typeof(DsfCommentActivity), "/images/ToolComment-32.png", "Comment"));
            }


            category = GetToolboxCategoryByName("File and Folder");
            if (category != null)
            {
                category.Add(new ToolboxItemWrapper(typeof(DsfPathCreate), "/Images/ToolFileFolderCreate-32.png", "Create"));
                category.Add(new ToolboxItemWrapper(typeof(DsfPathCopy), "/Images/ToolFileFolderCopy-32.png", "Copy"));
                category.Add(new ToolboxItemWrapper(typeof(DsfPathMove), "/Images/ToolFileFolderMove-32.png", "Move"));
                category.Add(new ToolboxItemWrapper(typeof(DsfPathDelete), "/Images/ToolFileFolderDelete-32.png", "Delete"));                
                category.Add(new ToolboxItemWrapper(typeof(DsfFileRead), "/Images/ToolFileFolderRead-32.png", "Read File"));
                category.Add(new ToolboxItemWrapper(typeof(DsfFileWrite), "/Images/ToolFileFolderWrite-32.png", "Write File"));
                category.Add(new ToolboxItemWrapper(typeof(DsfFolderRead), "/Images/ToolFileFolderReadFolder-32.png", "Read Folder"));
                category.Add(new ToolboxItemWrapper(typeof(DsfPathRename), "/Images/ToolFileFolderRename-32.png", "Rename"));
                category.Add(new ToolboxItemWrapper(typeof(DsfUnZip), "/Images/ToolFileFolderUnzip-32.png", "Unzip"));                
                category.Add(new ToolboxItemWrapper(typeof(DsfZip), "/Images/ToolFileFolderZip-32.png", "Zip"));
            }

            category = GetToolboxCategoryByName("Scripting");
            if (category != null)
            {
                category.Add(new ToolboxItemWrapper(typeof(DsfExecuteCommandLineActivity), "/Images/ToolCMDScript-32.png", "CMD Line"));
                category.Add(new ToolboxItemWrapper(typeof(DsfScriptingJavaScriptActivity), "/Images/ToolJavaScript-32.png", "JavaScript"));
            }

            //Massimo.Guerrera:17-04-17 - Added for PBI 9000
            category = GetToolboxCategoryByName("Resources");
            if (category != null)
            {
                category.Add(new ToolboxItemWrapper(typeof(DsfWorkflowActivity), "/Images/Workflow-32.png", "Workflow"));
                category.Add(new ToolboxItemWrapper(typeof(DsfServiceActivity), "/Images/ToolService-32.png", "Service"));                
            }
        }

        public void AddActivity(DsfActivity activity)
        {
            AddCategoryIfNotExists("Action Activities");
            var category = GetToolboxCategoryByName("Action Activities");
            if (category != null)
            {
                category.Tools.Add(new ToolboxItemWrapper(typeof(DsfActivity), activity.IconPath, activity.ToolboxFriendlyName));
            }
            tools.UpdateLayout();
        }

        public ToolboxCategory GetToolboxCategoryByName(string categoryName)
        {
            return tools.Categories.FirstOrDefault(c => c.CategoryName.Equals(categoryName, StringComparison.InvariantCultureIgnoreCase));
        }

        private void AddCategoryIfNotExists(string categoryName)
        {
            var category = GetToolboxCategoryByName(categoryName);
            if (category == null)
            {
                tools.Categories.Add(new ToolboxCategory(categoryName));
            }
        }                    
    }
}
