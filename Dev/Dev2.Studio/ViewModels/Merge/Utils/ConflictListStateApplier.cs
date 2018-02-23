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
using Dev2.Common.Interfaces;

namespace Dev2.ViewModels.Merge.Utils
{
    /// <summary>
    /// Manage the selection of items in a
    /// list of IConflictRow entries.
    /// </summary>
    public class ConflictListStateApplier
    {
        readonly ConflictRowList _conflictRowList;
        // TODO: this state applier should also be used to disable Connectors that are unavailable
        public ConflictListStateApplier(ConflictRowList conflicts)
        {
            this._conflictRowList = conflicts;
            RegisterToolEventListeners();
        }

        private void RegisterToolEventListeners()
        {
            foreach (var row in _conflictRowList)
            {
                if (row is IToolConflictRow toolRow)
                {
                    if (!(toolRow.CurrentViewModel is ToolConflictItem.Empty))
                    {
                        toolRow.CurrentViewModel.NotifyIsCheckedChanged += NotifyIsCheckedChangedHandler;
                    }
                    if (!(toolRow.DiffViewModel is ToolConflictItem.Empty))
                    {
                        toolRow.DiffViewModel.NotifyIsCheckedChanged += NotifyIsCheckedChangedHandler;
                    }
                }
            }
        }

        private static void NotifyIsCheckedChangedHandler(IConflictItem conflictItem, bool arg2)
        {
            if (conflictItem is IToolConflictItem toolConflictItem)
            {
                var connectors = new List<IConnectorConflictItem>();
                if (toolConflictItem.InboundConnectors != null)
                {
                    connectors.AddRange(toolConflictItem.InboundConnectors);
                }
                if (toolConflictItem.OutboundConnectors != null)
                {
                    connectors.AddRange(toolConflictItem.OutboundConnectors);
                }
                foreach (var connector in connectors)
                {
                    var sourceItem = connector.SourceConflictItem();
                    var destinationItem = connector.DestinationConflictItem();

                    var allow = sourceItem != null && sourceItem.IsInWorkflow
                            && destinationItem != null && destinationItem.IsInWorkflow;

                    connector.AllowSelection = allow;
                    if (!connector.AllowSelection)
                    {
                        connector.IsChecked = false;
                    }
                }
            }
        }

        public void SetConnectorSelectionsToCurrentState()
        {
            foreach (var row in _conflictRowList)
            {
                if (row.Current is IToolConflictItem toolConflictItem && !(row.Current is ToolConflictItem.Empty))
                {
                    toolConflictItem.IsChecked = true;
                    foreach (var connector in toolConflictItem.OutboundConnectors)
                    {
                        connector.DestinationConflictItem().IsChecked = true;
                        connector.IsChecked = true;
                    }
                }
            }
        }
        
    }
}
