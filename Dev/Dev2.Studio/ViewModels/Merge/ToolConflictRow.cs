﻿/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using Dev2.Common.Interfaces;

namespace Dev2.ViewModels.Merge
{
    public class ToolConflictRow : ConflictRow, IToolConflictRow
    {
        bool _isMergeExpanded;
        bool _isContainerTool;
        bool _isEmptyItemSelected;
        bool _isMergeVisible;

        internal static ToolConflictRow CreateConflictRow(IToolConflictItem currentToolConflictItem, IToolConflictItem diffToolConflictItem, List<IConnectorConflictRow> connectors) => new ToolConflictRow
        {
            CurrentViewModel = currentToolConflictItem,
            DiffViewModel = diffToolConflictItem,
            Connectors = connectors,
            HasConflict = !currentToolConflictItem.Equals(diffToolConflictItem)
        };

        internal static ToolConflictRow CreateStartRow(IToolConflictItem currentToolConflictItem, IToolConflictItem diffToolConflictItem) => new ToolConflictRow
        {
            CurrentViewModel = currentToolConflictItem,
            DiffViewModel = diffToolConflictItem,
            ContainsStart = true,
            IsMergeVisible = false,
            HasConflict = false
        };

        public IToolConflictItem CurrentViewModel { get; set; }
        public override IConflictItem Current => CurrentViewModel;
        public IToolConflictItem DiffViewModel { get; set; }
        public override IConflictItem Different => DiffViewModel;

        public override bool IsChecked { get; set; }

        public bool IsMergeVisible
        {
            get => _isMergeVisible;
            set
            {
                _isMergeVisible = value;
                OnPropertyChanged(() => IsMergeVisible);
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

        public override bool ContainsStart { get; set; }

        public override bool IsEmptyItemSelected
        {
            get => _isEmptyItemSelected;
            set
            {
                _isEmptyItemSelected = value;
                OnPropertyChanged(() => IsEmptyItemSelected);
            }
        }

        public IEnumerable<IConnectorConflictRow> Connectors { get; set; }
    }
}
