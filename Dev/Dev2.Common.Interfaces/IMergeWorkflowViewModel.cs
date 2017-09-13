using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace Dev2.Common.Interfaces
{
    public interface IMergeWorkflowViewModel
    {
        IConflictViewModel CurrentConflictViewModel { get; set; }
        IConflictViewModel DifferenceConflictViewModel { get; set; }
    }

    public interface IConflictViewModel
    {
        string WorkflowName { get; set; }
        string WorkflowLocation { get; set; }
        ObservableCollection<IMergeViewModel> MergeConflicts { get; set; }
    }

    public interface ICurrentConflictViewModel : IConflictViewModel
    {
        IMergeViewModel MergeViewModel { get; set; }
    }

    public interface IDifferenceConflictViewModel : IConflictViewModel
    {
        IMergeViewModel MergeViewModel { get; set; }
    }

    public interface IMergeViewModel
    {
        List<string> FieldCollection { get; set; }
        bool IsMergeExpanderEnabled { get; set; }
        bool IsMergeExpanded { get; set; }
        ImageSource MergeIcon { get; set; }
        string MergeDescription { get; set; }
        bool IsMergeChecked { get; set; }
        void SetMergeIcon(Type type);
    }
}
