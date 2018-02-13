﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using Dev2.Common.Interfaces;
using Dev2.Studio.Interfaces.DataList;

namespace Dev2.Studio.Interfaces
{
    public interface IMergeWorkflowViewModel : IDisposable, INotifyPropertyChanged, IUpdatesHelp
    {
        bool CanSave { get; }
        bool IsDirty { get; }
        string DisplayName { get; set; }
        void Save();
        IMergePreviewWorkflowDesignerViewModel MergePreviewWorkflowDesignerViewModel { get; set; }
        IDataListViewModel DataListViewModel { get; set; }
        bool HasMergeStarted { get; set; }
        bool HasWorkflowNameConflict { get; set; }
        bool HasVariablesConflict { get; set; }
        IEnumerable<IConflictRow> Conflicts { get; set; }
        bool IsVariablesEnabled { get; set; }
    }
}