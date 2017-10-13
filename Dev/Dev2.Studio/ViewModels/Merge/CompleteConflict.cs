using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Common.Interfaces;
using Microsoft.Practices.Prism.Mvvm;

namespace Dev2.ViewModels.Merge
{
    public class CompleteConflict : BindableBase, ICompleteConflict
    {
        private bool _isMergeExpanded;
        private bool _isMergeExpanderEnabled;
        private bool _hasConflict;
        private IEnumerator<ICompleteConflict> _conflictEnumerator;

        public CompleteConflict()
        {
            Children = new LinkedList<ICompleteConflict>();
        }

        public IMergeToolModel CurrentViewModel { get; set; }
        public IMergeToolModel DiffViewModel { get; set; }
        public LinkedList<ICompleteConflict> Children { get; set; }
        public ICompleteConflict Parent { get; set; }
        public Guid UniqueId { get; set; }

        public bool IsChecked { get; set; }

        public bool HasConflict
        {
            get => _hasConflict;
            set
            {
                _hasConflict = value;
                OnPropertyChanged(() => HasConflict);
            }
        }

        public bool IsMergeExpanderEnabled
        {
            get => _isMergeExpanderEnabled;
            set
            {
                _isMergeExpanderEnabled = value;
                OnPropertyChanged(() => IsMergeExpanderEnabled);
            }
        }

        public bool IsMergeExpanded
        {
            get => _isMergeExpanded;
            set
            {
                _isMergeExpanded = value;
                OnPropertyChanged(() => IsMergeExpanded);
            }
        }

        public ICompleteConflict GetNextConflict()
        {
            if (_conflictEnumerator == null)
            {
                _conflictEnumerator = Children.GetEnumerator();
            }
            if (_conflictEnumerator.Current != null)
            {
                var current = _conflictEnumerator.Current;
                if (current.Children.Count > 0)
                {
                    var nextConflict = current.GetNextConflict();
                    if (nextConflict != null)
                    {
                        return nextConflict;
                    }
                }
            }
            if (_conflictEnumerator.MoveNext())
            {
                var current = _conflictEnumerator.Current;
                return current;
            }
            return null;
        }

        public LinkedListNode<ICompleteConflict> Find(ICompleteConflict itemToFind)
        {
            var linkedConflict = Children.Find(itemToFind);
            if (linkedConflict != null)
            {
                return linkedConflict;
            }
            foreach (var completeConflict in Children)
            {
                var childItem = completeConflict.Find(itemToFind);
                if (childItem != null)
                {
                    return childItem;
                }
            }
            return null;
        }

        public bool All(Func<ICompleteConflict, bool> check)
        {
            var current = Children.All(check);
            var childrenMatch = true;
            foreach (var completeConflict in Children)
            {
                childrenMatch &= completeConflict.All(check);
            }
            return current && childrenMatch;
        }
    }
}
