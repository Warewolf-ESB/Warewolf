﻿using System;
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
    }

    public interface ICompleteConflict
    {
        IMergeToolModel CurrentViewModel { get; set; }
        IMergeToolModel DiffViewModel { get; set; }
        ObservableCollection<ICompleteConflict> Children { get; set; }
        Guid UniqueId { get; set; }
        bool HasConflict { get; set; }
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
        string ParentDescription { get; set; }
        bool HasParent { get; set; }
    }
}
