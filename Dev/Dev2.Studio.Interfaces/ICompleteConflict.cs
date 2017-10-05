using System;
using System.Collections.ObjectModel;
using Dev2.Common.Interfaces;

namespace Dev2.Studio.Interfaces
{
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
    public delegate void ConflictModelChanged(object sender, IConflictModelFactory args);
}