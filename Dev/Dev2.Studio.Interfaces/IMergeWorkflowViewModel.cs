using System;
using System.Collections.Generic;
using System.ComponentModel;
using Dev2.Common.Interfaces;
using Dev2.Studio.Interfaces.DataList;

namespace Dev2.Studio.Interfaces
{
    public interface IMergeWorkflowViewModel : IDisposable, INotifyPropertyChanged, IUpdatesHelp
    {
        bool CanSave { get; set; }
        bool IsDirty { get; }
        string DisplayName { get; set; }
        void Save();
        IWorkflowDesignerViewModel WorkflowDesignerViewModel { get; set; }
        IDataListViewModel DataListViewModel { get; set; }
        bool HasMergeStarted { get; set; }
        bool HasWorkflowNameConflict { get; set; }
        bool HasVariablesConflict { get; set; }
        IConflictModelFactory CurrentConflictModel { get; set; }
        IConflictModelFactory DifferenceConflictModel { get; set; }
        LinkedList<ICompleteConflict> Conflicts { get; set; }
        bool IsVariablesEnabled { get; set; }
        bool IsMergeExpanderEnabled { get; set; }
    }
}