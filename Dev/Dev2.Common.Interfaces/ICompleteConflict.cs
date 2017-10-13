using System;
using System.Collections.ObjectModel;

namespace Dev2.Common.Interfaces
{
    public interface ICompleteConflict
    {
        IMergeToolModel CurrentViewModel { get; set; }
        IMergeToolModel DiffViewModel { get; set; }
        ObservableCollection<ICompleteConflict> Children { get; set; }
        ICompleteConflict Parent { get; set; }
        Guid UniqueId { get; set; }
        bool HasConflict { get; set; }
        bool IsMergeExpanderEnabled { get; set; }
        bool IsMergeExpanded { get; set; }
        ICompleteConflict GetNextConflict();
    }
}