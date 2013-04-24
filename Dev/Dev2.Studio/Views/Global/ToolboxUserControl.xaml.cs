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
                category.Add(new ToolboxItemWrapper(typeof(DsfFlowDecisionActivity), "/Images/question_and_answer.png", "Decision"));
                category.Add(new ToolboxItemWrapper(typeof(Sequence), "/Images/blockdevice.png", "Sequence"));
                category.Add(new ToolboxItemWrapper(typeof(DsfFlowSwitchActivity), "/images/branch_element.png", "Switch"));
            }


            category = GetToolboxCategoryByName("Loop Constructs");

            if (category != null)
            {
                category.Add(new ToolboxItemWrapper(typeof(DsfForEachActivity), "/images/Loop.png", "For Each"));
            }


            category = GetToolboxCategoryByName("Recordset");
            if (category != null)
            {
                category.Add(new ToolboxItemWrapper(typeof(DsfCountRecordsetActivity), "/images/counter.png", "Count Records"));
                category.Add(new ToolboxItemWrapper(typeof(DsfDeleteRecordActivity), "/images/DeleteRecordIcon.png", "Delete Record"));
                category.Add(new ToolboxItemWrapper(typeof(DsfFindRecordsActivity), "/images/find.png", "Find Records"));
                category.Add(new ToolboxItemWrapper(typeof(DsfSortRecordsActivity), "/images/sorting.png", "Sort Records"));
            }


            category = GetToolboxCategoryByName("Utility");
            if (category != null)
            {
                category.Add(new ToolboxItemWrapper(typeof(DsfMultiAssignActivity), "/images/assign.png", "Assign"));
                category.Add(new ToolboxItemWrapper(typeof(DsfBaseConvertActivity), "/images/Base_Convert.png", "Base Conversion"));
                category.Add(new ToolboxItemWrapper(typeof(DsfCalculateActivity), "/images/calculator.png", "Calculate"));
                category.Add(new ToolboxItemWrapper(typeof(DsfCaseConvertActivity), "/images/CaseConversion.png", "Case Conversion"));
                category.Add(new ToolboxItemWrapper(typeof(DsfCommentActivity), "/images/comment_add.png", "Comment"));
                category.Add(new ToolboxItemWrapper(typeof(DsfDataMergeActivity), "/images/DataMergeToolIcon.png", "Data Merge"));
                category.Add(new ToolboxItemWrapper(typeof(DsfDataSplitActivity), "/images/split.png", "Data Split"));
                category.Add(new ToolboxItemWrapper(typeof(DsfDateTimeActivity), "/images/calendar-day.png", "Date and Time"));
                category.Add(new ToolboxItemWrapper(typeof(DsfDateTimeDifferenceActivity), "/images/DateTimeDiff.png", "Date and Time Difference"));
                category.Add(new ToolboxItemWrapper(typeof(DsfNumberFormatActivity), "/images/FormatNumber.png", "Format Number"));
                // TODO PBI 8291
                //category.Add(new ToolboxItemWrapper(typeof(DsfWebPageActivity), "/images/User.png", "Human Interface"));
                category.Add(new ToolboxItemWrapper(typeof(DsfIndexActivity), "/images/IndexToolIcon.png", "Find Index"));
                category.Add(new ToolboxItemWrapper(typeof(DsfReplaceActivity), "/images/ReplaceIcon.png", "Replace"));
            }


            category = GetToolboxCategoryByName("File and Folder");
            if (category != null)
            {
                category.Add(new ToolboxItemWrapper(typeof(DsfPathCopy), "/Images/copy.png", "Copy"));
                category.Add(new ToolboxItemWrapper(typeof(DsfPathCreate), "/Images/createfileorfolder.png", "Create"));
                category.Add(new ToolboxItemWrapper(typeof(DsfPathDelete), "/Images/delete.png", "Delete"));
                category.Add(new ToolboxItemWrapper(typeof(DsfPathMove), "/Images/move.png", "Move"));
                category.Add(new ToolboxItemWrapper(typeof(DsfFileRead), "/Images/fileread.png", "Read File"));
                category.Add(new ToolboxItemWrapper(typeof(DsfFolderRead), "/Images/folderread.png", "Read Folder"));
                category.Add(new ToolboxItemWrapper(typeof(DsfPathRename), "/Images/rename.png", "Rename"));
                category.Add(new ToolboxItemWrapper(typeof(DsfUnZip), "/Images/unzip.png", "Unzip"));
                category.Add(new ToolboxItemWrapper(typeof(DsfFileWrite), "/Images/writefile.png", "Write"));
                category.Add(new ToolboxItemWrapper(typeof(DsfZip), "/Images/zip.png", "Zip"));

            }


            category = GetToolboxCategoryByName("Scripting");
            if (category != null)
            {
                category.Add(new ToolboxItemWrapper(typeof(DsfExecuteCommandLineActivity), "/Images/CmdToolIcon.png", "CMD Line"));
            }

            //Massimo.Guerrera:17-04-17 - Added for PBI 9000
            category = GetToolboxCategoryByName("Resources");
            if (category != null)
            {
                category.Add(new ToolboxItemWrapper(typeof(DsfWorkflowActivity), "/Images/workflowservice2.png", "Workflow"));
                category.Add(new ToolboxItemWrapper(typeof(DsfServiceActivity), "/Images/workerservice.png", "Service"));                
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
