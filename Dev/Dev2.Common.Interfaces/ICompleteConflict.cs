using System;
using System.Collections.Generic;

namespace Dev2.Common.Interfaces
{
    public interface ICompleteConflict
    {
        IMergeToolModel CurrentViewModel { get; set; }
        IMergeToolModel DiffViewModel { get; set; }
        LinkedList<ICompleteConflict> Children { get; set; }
        ICompleteConflict Parent { get; set; }
        Guid UniqueId { get; set; }
        bool HasConflict { get; set; }
        bool IsMergeExpanderEnabled { get; set; }
        bool IsMergeExpanded { get; set; }
        bool IsChecked { get; set; }
        bool IsContainerTool { get; set; }
        ICompleteConflict GetNextConflict();
        LinkedListNode<ICompleteConflict> Find(ICompleteConflict itemToFind);
        bool All(Func<ICompleteConflict, bool> check);
    }
}