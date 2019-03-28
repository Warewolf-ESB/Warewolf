#pragma warning disable
﻿/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using Dev2.Common.Interfaces;
using Dev2.Studio.Interfaces;

namespace Dev2.ViewModels.Merge.Utils
{
    /// <summary>
    /// Manage the selection of items in a
    /// list of IConflictRow entries.
    /// </summary>
    public class ConflictListStateApplier
    {
        readonly ConflictRowList _conflictRowList;
        readonly IConflictModelFactory _conflictModelFactory;
        public ConflictListStateApplier(IConflictModelFactory conflictModelFactory, ConflictRowList conflicts)
        {
            this._conflictModelFactory = conflictModelFactory;
            this._conflictRowList = conflicts;
            SetInitialStates();
            RegisterToolEventListeners();
        }

        private void SetInitialStates()
        {
            foreach (var row in _conflictRowList)
            {
                if (row is IConnectorConflictRow connectorRow)
                {
                    SetInitialConnectorRowState(connectorRow);
                    SetConnectorAllowSelection(connectorRow.CurrentArmConnector);
                    SetConnectorAllowSelection(connectorRow.DifferentArmConnector);
                }
            }
        }

        private static void SetInitialConnectorRowState(IConnectorConflictRow row)
        {
            bool sourceConflict = row.CurrentArmConnector.SourceUniqueId != row.DifferentArmConnector.SourceUniqueId;
            bool destinationConflict = row.CurrentArmConnector.DestinationUniqueId != row.DifferentArmConnector.DestinationUniqueId;

            IToolConflictItem destinationToolCurrent = null;
            IToolConflictItem destinationToolDifferent = null;
            if (!(row.CurrentArmConnector is ConnectorConflictItem.Empty))
            {
                destinationToolCurrent = row.CurrentArmConnector.DestinationConflictItem();
            }
            if (!(row.DifferentArmConnector is ConnectorConflictItem.Empty))
            {
                destinationToolDifferent = row.DifferentArmConnector.DestinationConflictItem();
            }

            var destinationToolConflict = destinationToolCurrent != null && destinationToolDifferent != null && !destinationToolCurrent.Equals(destinationToolDifferent);
            var keyConflict = row.CurrentArmConnector.Key != row.DifferentArmConnector.Key;
            row.HasConflict = sourceConflict || destinationConflict || destinationToolConflict || keyConflict;
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
                    SetConnectorAllowSelection(connector);
                }
            }
        }

        private static void SetConnectorAllowSelection(IConnectorConflictItem connector)
        {
            if (connector is ConnectorConflictItem.Empty)
            {
                return;
            }
            var sourceItem = connector.SourceConflictItem();
            var destinationItem = connector.DestinationConflictItem();

            bool allow;
            if (connector.Key == "Start")
            {
                allow = destinationItem != null && destinationItem.IsInWorkflow;
            }
            else
            {
                allow = sourceItem != null && sourceItem.IsInWorkflow && destinationItem != null && destinationItem.IsInWorkflow;
            }
            connector.AllowSelection = allow;
        }

        public void SetConnectorSelectionsToCurrentState()
        {
            this._conflictModelFactory.IsWorkflowNameChecked = true;
            this._conflictModelFactory.IsVariablesChecked = true;

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
            SetInitialStates();
        }
    }
}
