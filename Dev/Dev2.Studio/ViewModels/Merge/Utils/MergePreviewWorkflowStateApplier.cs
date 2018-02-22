/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Studio.Interfaces;
using System;

namespace Dev2.ViewModels.Merge.Utils
{
    /// <summary>
    /// Apply selection state from a list of IConflict items
    /// to the Merge Preview Workflow
    /// </summary>
    public class MergePreviewWorkflowStateApplier
    {
        readonly IMergePreviewWorkflowDesignerViewModel _mergePreviewWorkflowDesignerViewModel;
        readonly ConflictRowList _conflictList;
        public MergePreviewWorkflowStateApplier(ConflictRowList conflictList, IMergePreviewWorkflowDesignerViewModel mergePreviewWorkflowDesignerViewModel)
        {
            _mergePreviewWorkflowDesignerViewModel = mergePreviewWorkflowDesignerViewModel;
            _conflictList = conflictList;
            RegisterEventHandlerForConflictItemChanges();
            Apply();
        }

        public void RegisterEventHandlerForConflictItemChanges()
        {
            foreach (var conflictRow in _conflictList)
            {
                var innerConflictRow = conflictRow;

                if (!(conflictRow.Current is ToolConflictItem.Empty))
                {
                    conflictRow.Current.NotifyIsCheckedChanged += (current, isChecked) =>
                    {
                        Handler(current, innerConflictRow);
                    };
                }
                if (!(conflictRow.Different is ToolConflictItem.Empty))
                {
                    conflictRow.Different.NotifyIsCheckedChanged += (diff, isChecked) =>
                    {
                        Handler(diff, innerConflictRow);
                    };
                }
            }
        }

        public void Apply()
        {
            foreach (var conflictRow in _conflictList)
            {
                Handler(conflictRow.Current, conflictRow);
            }
        }

        private void Handler(IConflictItem changedItem, IConflictRow row)
        {
            if (changedItem is IToolConflictItem toolItem)
            {
                if (changedItem is ToolConflictItem.Empty)
                {
                    return;
                }
                if (row.ContainsStart)
                {
                    return;
                }
                ToolModelHandler(toolItem);
            }
            else if (changedItem is IConnectorConflictItem connectorItem && row is IConnectorConflictRow connectorRow)
            {
                
                ConnectorHandler(connectorItem, connectorRow);
            }
            else
            {
                Dev2Logger.Error("Only ConflictRow and ConflictItem are supported", GlobalConstants.WarewolfError);
                // TODO: Remove?
                throw new NotImplementedException("Only ConflictRow and ConflictItem are supported");
            }
        }
        private void ToolModelHandler(IToolConflictItem changedItem)
        {
            if (changedItem.IsChecked)
            {
                AddActivity(changedItem);
            }
            else
            {
                RemoveActivity(changedItem);
            }
        }

        private void AddActivity(IToolConflictItem toolModelConflictItem)
        {
            _mergePreviewWorkflowDesignerViewModel.AddItem(toolModelConflictItem);
            if (!(toolModelConflictItem.InboundConnectors is null) && toolModelConflictItem.InboundConnectors.Count != 0)
            {
                var sourceUniqueId = toolModelConflictItem.InboundConnectors[0].SourceUniqueId;
                var destinationUniqueId = toolModelConflictItem.UniqueId;
                var key = toolModelConflictItem.InboundConnectors[0].Key;
                LinkActivities(sourceUniqueId, destinationUniqueId, key);
            }
            // TODO: if there is a connector that IsChecked then connect it back to this tool

        }

        private void RemoveActivity(IToolConflictItem toolModelConflictItem)
        {
            _mergePreviewWorkflowDesignerViewModel.RemoveItem(toolModelConflictItem);
        }

        private void ConnectorHandler(IConnectorConflictItem changedItem, IConnectorConflictRow row)
        {
            if (row.ContainsStart)
            {
                _mergePreviewWorkflowDesignerViewModel.RemoveStartNodeConnection();
                LinkStartNode(row);
                return;
            }
            if (changedItem.IsChecked)
            {
                AddAndLinkActivity(changedItem, row);
            }
            else
            {
                DeLinkActivities(changedItem.SourceUniqueId, changedItem.DestinationUniqueId, changedItem.Key);
            }
        }

        private void LinkStartNode(IConnectorConflictRow row)
        {
            var startToolRow = _conflictList.GetStartToolRow();

            var toolConflictItem = row.Different.IsChecked ? startToolRow.DiffViewModel : startToolRow.CurrentViewModel;

            toolConflictItem.SetAutoChecked();
            AddActivity(toolConflictItem);

            _mergePreviewWorkflowDesignerViewModel.LinkStartNode(toolConflictItem);
        }

        private void AddAndLinkActivity(IConnectorConflictItem changedItem, IConnectorConflictRow row)
        {
            var isCurrent = changedItem.DestinationUniqueId.Equals(row.CurrentArmConnector.DestinationUniqueId);

            var toolConflictItem = isCurrent ? _conflictList.GetToolItemFromIdCurrent(changedItem.DestinationUniqueId)
                                             : _conflictList.GetToolItemFromIdDifferent(changedItem.DestinationUniqueId);
            if (!(toolConflictItem is null))
            {
                toolConflictItem.SetAutoChecked();

                //if !toolConflictItem.AllowSelection
                //
                //    var connectorConflictItem = isCurrent ?
                //        conflictList.GetConnectorItemFromToolIdCurrent(toolConflictItem.UniqueId) :
                //        conflictList.GetConnectorItemFromToolIdDifferent(toolConflictItem.UniqueId)
                //    connectorConflictItem.AllowSelection = true
                //

                AddActivity(toolConflictItem);
                LinkActivities(changedItem.SourceUniqueId, changedItem.DestinationUniqueId, changedItem.Key);
            }
        }

        private void LinkActivities(Guid SourceUniqueId, Guid DestinationUniqueId, string Key)
        {
            _mergePreviewWorkflowDesignerViewModel?.LinkActivities(SourceUniqueId, DestinationUniqueId, Key);
        }

        private void DeLinkActivities(Guid SourceUniqueId, Guid DestinationUniqueId, string Key)
        {
            _mergePreviewWorkflowDesignerViewModel?.DeLinkActivities(SourceUniqueId, DestinationUniqueId, Key);
        }
    }
}
