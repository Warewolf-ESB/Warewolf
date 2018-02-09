/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Common.Interfaces;

namespace Dev2.ViewModels.Merge
{
    public class ToolConflictRow : ConflictRow, IToolConflictRow
    {
        bool _isMergeExpanded;
        bool _hasConflict;
        bool _hasNodeArmConflict;
        IEnumerator<IToolConflictRow> _conflictEnumerator;
        bool _isContainerTool;
        bool _isStartNode;
        bool _isEmptyItemSelected;

        public ToolConflictRow()
        {
            Children = new LinkedList<IToolConflictRow>();
        }

        public IToolModelConflictItem CurrentViewModel { get; set; }
        public override IConflictItem Current => CurrentViewModel;
        public IToolModelConflictItem DiffViewModel { get; set; }
        public override IConflictItem Different => DiffViewModel;

        public LinkedList<IToolConflictRow> Children { get; set; }
        public IToolConflictRow Parent { get; set; }
        public override Guid UniqueId { get; set; }

        public override bool IsChecked { get; set; }

        public override bool HasConflict
        {
            get => _hasConflict;
            set
            {
                _hasConflict = value;
                OnPropertyChanged(() => HasConflict);
            }
        }

        public bool HasNodeArmConflict
        {
            get => _hasNodeArmConflict;
            set
            {
                _hasNodeArmConflict = value;
                OnPropertyChanged(() => HasNodeArmConflict);
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

        public bool IsContainerTool
        {
            get => _isContainerTool;
            set
            {
                _isContainerTool = value;
                OnPropertyChanged(() => IsContainerTool);
            }
        }

        public bool IsStartNode
        {
            get => _isStartNode;
            set
            {
                _isStartNode = value;
                OnPropertyChanged(() => IsStartNode);
            }
        }

        public override bool IsEmptyItemSelected
        {
            get => _isEmptyItemSelected;
            set
            {
                _isEmptyItemSelected = value;
                OnPropertyChanged(() => IsEmptyItemSelected);
            }
        }

        public IToolConflictRow GetNext()
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
                    var nextConflict = current.GetNext();
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

        public LinkedListNode<IToolConflictRow> Find(IToolConflictRow itemToFind)
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

        public bool All(Func<IToolConflictRow, bool> check)
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
