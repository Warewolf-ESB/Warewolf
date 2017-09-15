using System.Collections.ObjectModel;
using System.Windows.Media;

namespace Dev2.Common.Interfaces
{
    public interface IMergeWorkflowViewModel
    {
        IConflictViewModel CurrentConflictViewModel { get; set; }
        IConflictViewModel DifferenceConflictViewModel { get; set; }
        ObservableCollection<CompleteConflict> Conflicts { get; set; }
    }

    public interface IConflictViewModel
    {
        string WorkflowName { get; set; }
        IMergeToolModel MergeToolModel { get; set; }
    }

    public class CompleteConflict
    {
        public IMergeToolModel CurrentViewModel { get; set; }
        public IMergeToolModel DiffViewModel { get; set; }
    }

    public interface IMergeToolModel
    {
        bool IsMergeExpanderEnabled { get; set; }
        bool IsMergeExpanded { get; set; }
        ImageSource MergeIcon { get; set; }
        string MergeDescription { get; set; }
        bool IsMergeChecked { get; set; }
        bool IsVariablesChecked { get; set; }
        ObservableCollection<IMergeToolModel> Children { get; set; }
    }
}
