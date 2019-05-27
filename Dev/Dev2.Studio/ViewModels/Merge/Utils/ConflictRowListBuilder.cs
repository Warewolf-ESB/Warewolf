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

using System.Collections.Generic;
using Dev2.Common.Interfaces;
using Dev2.Common;
using System;
using Dev2.Studio.Interfaces;
using System.Linq;

namespace Dev2.ViewModels.Merge.Utils
{
    public class ConflictRowListBuilder
    {
        readonly IConflictModelFactory _modelFactoryCurrent;
        readonly IConflictModelFactory _modelFactoryDifferent;
        const int MAX_WORKFLOW_ITEMS = 10000;
        ConflictRowList _list;
        IToolConflictItem currentToolConflictItem;
        IToolConflictItem diffToolConflictItem;
        int indexDiff;
        int indexCurr;
        ConnectorConflictRow row;

        public ConflictRowListBuilder(IConflictModelFactory modelFactoryCurrent, IConflictModelFactory modelFactoryDifferent)
        {
            _modelFactoryCurrent = modelFactoryCurrent;
            _modelFactoryDifferent = modelFactoryDifferent;
        }

        public List<ToolConflictRow> CreateList(ConflictRowList list, ConflictTreeNode[] currentTree, ConflictTreeNode[] diffTree)
        {
            var _currentTree = currentTree;
            var _diffTree = diffTree;
            _list = list;

            var toolConflictRowList = new List<ToolConflictRow>();
            indexDiff = 0;
            indexCurr = 0;

            for (int i = 0; i <= MAX_WORKFLOW_ITEMS; i++)
            {
                if (i == MAX_WORKFLOW_ITEMS)
                {
                    Dev2Logger.Error("ConflictRowListBuilder.CreateList: createlist expected to advance", GlobalConstants.WarewolfError);
                    throw new Exception("createlist expected to advance");
                }
                ConflictTreeNode current = null;
                ConflictTreeNode diff = null;

                current = GetConflictTreeNode(true, indexCurr, indexDiff, _currentTree);
                diff = GetConflictTreeNode(false, indexCurr, indexDiff, _diffTree);
                if (current == null && diff == null)
                {
                    break;
                }
                currentToolConflictItem = null;
                diffToolConflictItem = null;
                bool diffFoundInCurrent = _currentTree.Contains(diff);
                bool currFoundInDifferent = _diffTree.Contains(current);

                if (diffFoundInCurrent && currFoundInDifferent)
                {
                    diff = _diffTree.FirstOrDefault(o => o.UniqueId == current.UniqueId);
                    current = _currentTree.FirstOrDefault(o => o.UniqueId == current.UniqueId);
                }
                GetToolConflictItems(current, diff, diffFoundInCurrent, currFoundInDifferent);

                var toolConflictRow = BuildToolConflictRow(list, current, diff);
                toolConflictRow.IsMergeVisible = toolConflictRow.HasConflict;
                toolConflictRowList.Add(toolConflictRow);
            }
            return toolConflictRowList;
        }
        private static ConflictTreeNode GetConflictTreeNode(bool IsCurrent, int indexCurr, int indexDiff, ConflictTreeNode[] nodes)
        {
            ConflictTreeNode node = null;
            if (IsCurrent)
            {
                if (indexCurr < nodes.Length)
                {
                    node = nodes[indexCurr];
                }
            }
            else
            {
                if (indexDiff < nodes.Length)
                {
                    node = nodes[indexDiff];
                }
            }
            return node;
        }

        void GetToolConflictItems(ConflictTreeNode current, ConflictTreeNode diff, bool diffFoundInCurrent, bool currFoundInDifferent)
        {

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
                    currentToolConflictItem = new ToolConflictItem(_list, ConflictRowList.Column.Current);
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
                    diffToolConflictItem = new ToolConflictItem(_list, ConflictRowList.Column.Different);
                    _modelFactoryDifferent.CreateModelItem(diffToolConflictItem, diff);
                }
                indexDiff++;
            }
        }

        private ToolConflictRow BuildToolConflictRow(ConflictRowList list, ConflictTreeNode current, ConflictTreeNode diff)
        {
            var currentConnectorConflictTreeNode = currentToolConflictItem is ToolConflictItem.Empty ? null : current;
            var diffConnectorConflictTreeNode = diffToolConflictItem is ToolConflictItem.Empty ? null : diff;
            var connectors = GetConnectorConflictRows(list, currentToolConflictItem, diffToolConflictItem, currentConnectorConflictTreeNode, diffConnectorConflictTreeNode);

            var diffToolConflictItem_override = diffToolConflictItem;
            if (currentToolConflictItem.Activity != null && diffToolConflictItem.Activity != null && currentToolConflictItem.Activity.Equals(diffToolConflictItem.Activity))
            {
                diffToolConflictItem_override = currentToolConflictItem.Clone();
            }

            var toolConflictRow = ToolConflictRow.CreateConflictRow(currentToolConflictItem, diffToolConflictItem_override, connectors);
            return toolConflictRow;
        }

        private List<IConnectorConflictRow> GetConnectorConflictRows(ConflictRowList list, IToolConflictItem currentConflictItem, IToolConflictItem diffConflictItem, ConflictTreeNode current, ConflictTreeNode diff)
        {
            var rows = new List<IConnectorConflictRow>();
            int index = 0;
            var armConnectorsCurrent = new List<(string Description, string Key, string SourceUniqueId, string DestinationUniqueId)>();
            var armConnectorsDiff = new List<(string Description, string Key, string SourceUniqueId, string DestinationUniqueId)>();

            SetConnectorConflict(current, diff, out armConnectorsCurrent, out armConnectorsDiff, out int maxCount);

            for (; index < maxCount; index++)
            {
                row = new ConnectorConflictRow();
                row.CurrentArmConnector = new ConnectorConflictItem.Empty(row.UniqueId);
                if (armConnectorsCurrent != null && index < armConnectorsCurrent.Count)
                {
                    SetCurrentConnectorConflict(armConnectorsCurrent[index], list, currentConflictItem);
                }
                row.DifferentArmConnector = new ConnectorConflictItem.Empty(row.UniqueId);
                if (armConnectorsDiff != null && index < armConnectorsDiff.Count)
                {
                    SetDiffConnectorConflict(armConnectorsDiff[index], list, diffConflictItem);
                }
                var sameSourceAndDestination = row.CurrentArmConnector.SourceUniqueId != row.DifferentArmConnector.SourceUniqueId || row.CurrentArmConnector.DestinationUniqueId != row.DifferentArmConnector.DestinationUniqueId;
                row.HasConflict = sameSourceAndDestination;
                rows.Add(row);
            }
            return rows;
        }
        private void SetCurrentConnectorConflict((string Description, string Key, string SourceUniqueId, string DestinationUniqueId) armConnectorsCurrent, ConflictRowList list, IToolConflictItem currentConflictItem)
        {

            if (armConnectorsCurrent.DestinationUniqueId == Guid.Empty.ToString())
            {
                row.CurrentArmConnector = new ConnectorConflictItem.Empty(row.UniqueId);
            }
            else
            {
                var connector = new ConnectorConflictItem(list, ConflictRowList.Column.Current, row.UniqueId, armConnectorsCurrent.Description, Guid.Parse(armConnectorsCurrent.SourceUniqueId), Guid.Parse(armConnectorsCurrent.DestinationUniqueId), armConnectorsCurrent.Key);
                row.CurrentArmConnector = connector;
                if (!(currentConflictItem is ToolConflictItem.Empty))
                {
                    currentConflictItem.OutboundConnectors.Add(connector);
                }
            }
        }
        private void SetDiffConnectorConflict((string Description, string Key, string SourceUniqueId, string DestinationUniqueId) armConnectorsCurrent, ConflictRowList list, IToolConflictItem diffConflictItem)
        {
            if (armConnectorsCurrent.DestinationUniqueId == Guid.Empty.ToString())
            {
                row.CurrentArmConnector = new ConnectorConflictItem.Empty(row.UniqueId);
            }
            else
            {
                var connector = new ConnectorConflictItem(list, ConflictRowList.Column.Different, row.UniqueId, armConnectorsCurrent.Description, Guid.Parse(armConnectorsCurrent.SourceUniqueId), Guid.Parse(armConnectorsCurrent.DestinationUniqueId), armConnectorsCurrent.Key);
                row.DifferentArmConnector = connector;
                if (!(diffConflictItem is ToolConflictItem.Empty))
                {
                    diffConflictItem.OutboundConnectors.Add(connector);
                }
            }
        }
        private static void SetConnectorConflict(ConflictTreeNode current, ConflictTreeNode diff, out List<(string Description, string Key, string SourceUniqueId, string DestinationUniqueId)> armConnectorsCurrent, out List<(string Description, string Key, string SourceUniqueId, string DestinationUniqueId)> armConnectorsDiff, out int maxCount)
        {

            armConnectorsCurrent = current?.Activity.ArmConnectors().OrderBy(i => i.Key ?? "").ToList();
            armConnectorsDiff = diff?.Activity.ArmConnectors().OrderBy(i => i.Key ?? "").ToList();

            maxCount = current == null
                ? armConnectorsDiff.Count()
                : diff == null
                ? armConnectorsCurrent.Count()
                : Math.Max(armConnectorsCurrent.Count, armConnectorsDiff.Count);
        }
    }
}
