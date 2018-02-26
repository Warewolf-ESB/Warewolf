/*
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
using System.Collections;
using Dev2.Common;
using System;
using Dev2.Studio.Interfaces;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace Dev2.ViewModels.Merge.Utils
{
    public class ConflictRowListBuilder
    {
        readonly IConflictModelFactory _modelFactoryCurrent;
        readonly IConflictModelFactory _modelFactoryDifferent;
        const int MAX_WORKFLOW_ITEMS = 10000;

        public ConflictRowListBuilder(IConflictModelFactory modelFactoryCurrent, IConflictModelFactory modelFactoryDifferent)
        {
            _modelFactoryCurrent = modelFactoryCurrent;
            _modelFactoryDifferent = modelFactoryDifferent;
        }

        public List<ToolConflictRow> CreateList(ConflictRowList list, ConflictTreeNode[] currentTree, ConflictTreeNode[] diffTree)
        {
            var toolConflictRowList = new List<ToolConflictRow>();
            int indexDiff = 0;
            int indexCurr = 0;

            for (int i = 0; i <= MAX_WORKFLOW_ITEMS; i++)
            {
                if (i == MAX_WORKFLOW_ITEMS)
                {
                    throw new Exception("createlist expected to advance");
                }
                ConflictTreeNode current = null;
                ConflictTreeNode diff = null;

                if (indexCurr < currentTree.Length)
                {
                    current = currentTree[indexCurr];
                }
                if (indexDiff < diffTree.Length)
                {
                    diff = diffTree[indexDiff];
                }
                if (current == null && diff == null)
                {
                    break;
                }

                bool diffFoundInCurrent = currentTree.Contains(diff);
                bool currFoundInDifferent = diffTree.Contains(current);

                #region get tool conflict item
                IToolConflictItem currentToolConflictItem = null;
                IToolConflictItem diffToolConflictItem = null;
                if (!diffFoundInCurrent && !currFoundInDifferent)
                {
                    //This is a guard clause

                    // NOTE: if we want to allow a tool that was deleted to not conflict with a tool that was added
                    // this is where it would be implemented
                }
                else
                {
                    if (!diffFoundInCurrent)
                    {
                        currentToolConflictItem = ToolConflictItem.EmptyConflictItem();
                    }
                    if (!currFoundInDifferent)
                    {
                        diffToolConflictItem = ToolConflictItem.EmptyConflictItem();
                    }
                }

                if (currentToolConflictItem == null)
                {
                    if (current == null)
                    {
                        currentToolConflictItem = ToolConflictItem.EmptyConflictItem();
                    }
                    else
                    {
                        currentToolConflictItem = new ToolConflictItem(list, ConflictRowList.Column.Current);
                        _modelFactoryCurrent.CreateModelItem(currentToolConflictItem, current);
                    }
                    indexCurr++;
                }
                if (diffToolConflictItem == null)
                {
                    if (diff == null)
                    {
                        diffToolConflictItem = ToolConflictItem.EmptyConflictItem();
                    }
                    else
                    {
                        diffToolConflictItem = new ToolConflictItem(list, ConflictRowList.Column.Different);
                        _modelFactoryDifferent.CreateModelItem(diffToolConflictItem, diff);
                    }
                    indexDiff++;
                }
                #endregion

                var toolConflictRow = BuildToolConflictRow(list, current, diff, currentToolConflictItem, diffToolConflictItem);
                toolConflictRow.IsMergeVisible = toolConflictRow.HasConflict;
                toolConflictRowList.Add(toolConflictRow);
            }
            return toolConflictRowList;
        }

        static ToolConflictRow BuildToolConflictRow(ConflictRowList list, ConflictTreeNode current, ConflictTreeNode diff, IToolConflictItem currentToolConflictItem, IToolConflictItem diffToolConflictItem)
        {
            var currentConnectorConflictTreeNode = currentToolConflictItem is ToolConflictItem.Empty ? null : current;
            var diffConnectorConflictTreeNode = diffToolConflictItem is ToolConflictItem.Empty ? null : diff;
            var connectors = GetConnectorConflictRows(list, currentToolConflictItem, diffToolConflictItem, currentConnectorConflictTreeNode, diffConnectorConflictTreeNode);

            var diffToolConflictItem_override = diffToolConflictItem;
            if (currentToolConflictItem.Activity != null && diffToolConflictItem.Activity != null && currentToolConflictItem.Activity.Equals(diffToolConflictItem.Activity))
            {
                // TODO: the diff conflictitem has now changed. We need to set all connectors that refer to the diff tool
                // so that they are also referencing the currentTool
                diffToolConflictItem_override = currentToolConflictItem.Clone();
            }

            var toolConflictRow = ToolConflictRow.CreateConflictRow(currentToolConflictItem, diffToolConflictItem_override, connectors);
            return toolConflictRow;
        }

        private static List<IConnectorConflictRow> GetConnectorConflictRows(ConflictRowList list, IToolConflictItem currentConflictItem, IToolConflictItem diffConflictItem, ConflictTreeNode current, ConflictTreeNode diff)
        {
            var rows = new List<IConnectorConflictRow>();
            int index = 0;
            var armConnectorsCurrent = current?.Activity.ArmConnectors();
            var armConnectorsDiff = diff?.Activity.ArmConnectors();
            var maxCount = current == null
                ? armConnectorsDiff.Count
                : diff == null
                ? armConnectorsCurrent.Count
                : Math.Max(armConnectorsCurrent.Count, armConnectorsDiff.Count);
            for (; index < maxCount; index++)
            {
                var row = new ConnectorConflictRow();
                if (armConnectorsCurrent != null && index < armConnectorsCurrent.Count)
                {
                    var (Description, Key, SourceUniqueId, DestinationUniqueId) = armConnectorsCurrent[index];
                    var connector = new ConnectorConflictItem(list, ConflictRowList.Column.Current, row.UniqueId, Description, Guid.Parse(SourceUniqueId), Guid.Parse(DestinationUniqueId), Key);
                    row.CurrentArmConnector = connector;
                    if (!(currentConflictItem is ToolConflictItem.Empty))
                    {
                        currentConflictItem.OutboundConnectors.Add(connector);
                    }
                }
                else
                {
                    row.CurrentArmConnector = new ConnectorConflictItem.Empty(row.UniqueId);
                }

                if (armConnectorsDiff != null && index < armConnectorsDiff.Count)
                {
                    var (Description, Key, SourceUniqueId, DestinationUniqueId) = armConnectorsDiff[index];
                    var connector = new ConnectorConflictItem(list, ConflictRowList.Column.Different, row.UniqueId, Description, Guid.Parse(SourceUniqueId), Guid.Parse(DestinationUniqueId), Key);
                    row.DifferentArmConnector = connector;
                    if (!(diffConflictItem is ToolConflictItem.Empty))
                    {
                        diffConflictItem.OutboundConnectors.Add(connector);
                    }
                }
                else
                {
                    row.DifferentArmConnector = new ConnectorConflictItem.Empty(row.UniqueId);
                }
                var sameSourceAndDestination = row.CurrentArmConnector.SourceUniqueId != row.DifferentArmConnector.SourceUniqueId
                                    || row.CurrentArmConnector.DestinationUniqueId != row.DifferentArmConnector.DestinationUniqueId;
                row.HasConflict = sameSourceAndDestination;
                rows.Add(row);
            }
            return rows;
        }
    }
}
