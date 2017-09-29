using System;
using System.Activities.Statements;
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

        bool HasMergeStarted { get; set; }
        bool HasWorkflowNameConflict { get; set; }
        bool HasVariablesConflict { get; set; }
        IConflictModelFactory CurrentConflictModel { get; set; }
        IConflictModelFactory DifferenceConflictModel { get; set; }
        ObservableCollection<ICompleteConflict> Conflicts { get; set; }
        bool IsVariablesEnabled { get; set; }
        bool IsMergeExpanderEnabled { get; set; }
    }

    public delegate void ConflictModelChanged(object sender, IConflictModelFactory args);
    public delegate void ModelToolChanged(object sender, IMergeToolModel args);

    public interface ICompleteConflict
    {
        IMergeToolModel CurrentViewModel { get; set; }
        IMergeToolModel DiffViewModel { get; set; }
        ObservableCollection<ICompleteConflict> Children { get; set; }
        Guid UniqueId { get; set; }
        bool HasConflict { get; set; }
        bool IsMergeExpanderEnabled { get; set; }
        bool IsMergeExpanded { get; set; }
    }

    public interface IMergeToolModel
    {
        ImageSource MergeIcon { get; set; }
        string MergeDescription { get; set; }
        bool IsMergeChecked { get; set; }
        IMergeToolModel Parent { get; set; }
        ObservableCollection<IMergeToolModel> Children { get; set; }
        Guid UniqueId { get; set; }
        string ParentDescription { get; set; }
        bool HasParent { get; set; }
        event ModelToolChanged SomethingModelToolChanged;
        FlowNode ActivityType { get; set; }
    }
}
