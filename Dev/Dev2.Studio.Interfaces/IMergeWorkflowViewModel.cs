using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Dev2.Common.Interfaces;

namespace Dev2.Studio.Interfaces
{
    public interface IMergeWorkflowViewModel : IDisposable, INotifyPropertyChanged, IUpdatesHelp
    {
        bool CanSave { get; set; }
        bool IsDirty { get; }
        string DisplayName { get; set; }
        void Save();
        IWorkflowDesignerViewModel WorkflowDesignerViewModel { get; set; }

        bool HasMergeStarted { get; set; }
        bool HasWorkflowNameConflict { get; set; }
        bool HasVariablesConflict { get; set; }
        IConflictModelFactory CurrentConflictModel { get; set; }
        IConflictModelFactory DifferenceConflictModel { get; set; }
        ObservableCollection<ICompleteConflict> Conflicts { get; set; }
        bool IsVariablesEnabled { get; set; }
        bool IsMergeExpanderEnabled { get; set; }
    }
}