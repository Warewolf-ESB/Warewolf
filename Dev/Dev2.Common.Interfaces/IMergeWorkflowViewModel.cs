using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Media;

namespace Dev2.Common.Interfaces
{
    public interface IMergeWorkflowViewModel : IDisposable, INotifyPropertyChanged, IUpdatesHelp
    {
        bool CanSave { get; set; }
        bool IsDirty { get; }
        string DisplayName { get; set; }
        void Save();
        IConflictViewModel CurrentConflictViewModel { get; set; }
        IConflictViewModel DifferenceConflictViewModel { get; set; }
        ObservableCollection<ICompleteConflict> Conflicts { get; set; }
    }

    public interface IConflictViewModel
    {
        string WorkflowName { get; set; }
        IMergeToolModel MergeToolModel { get; set; }
        void GetDataList();
    }

    public interface ICompleteConflict
    {
        IMergeToolModel CurrentViewModel { get; set; }
        IMergeToolModel DiffViewModel { get; set; }
        ObservableCollection<ICompleteConflict> Children { get; set; }
        public Guid UniqueId { get; set; }
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
        Guid UniqueId { get; set; }
    }
}
