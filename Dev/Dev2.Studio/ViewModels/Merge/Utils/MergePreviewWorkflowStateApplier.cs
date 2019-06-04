#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
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
                var exception = new NotImplementedException("Only ConflictRow and ConflictItem are supported");
                Dev2Logger.Error("Unsupported ConflictRow and ConflictItem", exception, GlobalConstants.WarewolfError);
                throw exception;
            }
        }

        private void ToolModelHandler(IToolConflictItem changedItem)
        {
            if (changedItem.IsChecked)
            {
                if (!changedItem.IsAddedToWorkflow)
                {
                    AddActivity(changedItem);
                }                
            }
            else
            {
                RemoveActivity(changedItem);
            }
        }

        private void AddActivity(IToolConflictItem toolModelConflictItem)
        {
            _mergePreviewWorkflowDesignerViewModel.AddItem(toolModelConflictItem);
            if (!(toolModelConflictItem.InboundConnectors is null) && toolModelConflictItem.InboundConnectors.Count > 0)
            {                
                var inboundConnectors = toolModelConflictItem.InboundConnectors;
                foreach (var inboundConnector in inboundConnectors)
                {
                    if (inboundConnector.IsChecked)
                    {
                        var sourceUniqueId = inboundConnector.SourceUniqueId;
                        var destinationUniqueId = toolModelConflictItem.UniqueId;
                        var key = inboundConnector.Key;
                        LinkActivities(sourceUniqueId, destinationUniqueId, key);
                    }
                }
            }
        }

        private void RemoveActivity(IToolConflictItem toolModelConflictItem)
        {
            _mergePreviewWorkflowDesignerViewModel.RemoveItem(toolModelConflictItem);
        }

        private void ConnectorHandler(IConnectorConflictItem changedItem, IConnectorConflictRow row)
        {
            if (changedItem is ConnectorConflictItem.Empty)
            {
                return;
            }

            if (row.ContainsStart && changedItem.IsChecked)
            {
                _mergePreviewWorkflowDesignerViewModel.RemoveStartNodeConnection();
                LinkStartNode(changedItem);
                return;
            }
            if (changedItem.IsChecked)
            {
                AddAndLinkActivity(changedItem);
            }
            else
            {
                DeLinkActivities(changedItem.SourceUniqueId, changedItem.DestinationUniqueId, changedItem.Key);
            }
        }

        private void LinkStartNode(IConnectorConflictItem changedItem)
        {
            var toolConflictItem = changedItem.DestinationConflictItem();

            toolConflictItem.SetAutoChecked();
            AddActivity(toolConflictItem);

            _mergePreviewWorkflowDesignerViewModel.LinkStartNode(toolConflictItem);
        }

        private void AddAndLinkActivity(IConnectorConflictItem changedItem)
        {
            var sourceConflictItem = changedItem.SourceConflictItem();
            var destinationConflictItem = changedItem.DestinationConflictItem();
            if (!(sourceConflictItem is null) && !(destinationConflictItem is null))
            {
                if (changedItem.IsChecked)
                {
                    LinkActivities(sourceConflictItem.UniqueId, destinationConflictItem.UniqueId, changedItem.Key);
                }
                else
                {
                    DeLinkActivities(sourceConflictItem.UniqueId, destinationConflictItem.UniqueId, changedItem.Key);
                }
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
