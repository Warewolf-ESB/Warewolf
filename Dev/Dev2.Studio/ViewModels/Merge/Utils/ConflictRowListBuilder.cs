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

        public ConflictRowListBuilder(IConflictModelFactory modelFactoryCurrent, IConflictModelFactory modelFactoryDifferent)
        {
            _modelFactoryCurrent = modelFactoryCurrent;
            _modelFactoryDifferent = modelFactoryDifferent;
        }

        public List<ToolConflictRow> CreateList(ConflictRowList list, ConflictTreeNode[] currentTree, ConflictTreeNode[] diffTree)
        {
            var toolConflictRowList = new List<ToolConflictRow>();
            var maxCount = Math.Max(currentTree.Length, diffTree.Length);
            int indexDiff = 0;
            int indexCurr = 0;

            for (int i = 0; i < maxCount; i++)
            {
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

                var toolConflictRow = BuildToolConflictRow(list, current, diff, diffFoundInCurrent, currFoundInDifferent);
                toolConflictRow.IsMergeVisible = toolConflictRow.HasConflict;
                toolConflictRowList.Add(toolConflictRow);

                if (diffFoundInCurrent)
                {
                    indexCurr++;
                }
                if (currFoundInDifferent)
                {
                    indexDiff++;
                }
            }
            return toolConflictRowList;
        }

        private ToolConflictRow BuildToolConflictRow(ConflictRowList list, ConflictTreeNode current, ConflictTreeNode diff, bool diffFoundInCurrent, bool currFoundInDifferent)
        {
            bool found = diffFoundInCurrent && currFoundInDifferent;

            IToolConflictItem currentToolConflictItem = null;
            IToolConflictItem diffToolConflictItem = null;
            if (!found)
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

                currentToolConflictItem = new ToolConflictItem(list, ConflictRowList.Column.Current);
                _modelFactoryCurrent.CreateToolModelConfictItem(currentToolConflictItem, current);
            }
            if (diffToolConflictItem == null)
            {
                diffToolConflictItem = new ToolConflictItem(list, ConflictRowList.Column.Different);
                _modelFactoryDifferent.CreateToolModelConfictItem(diffToolConflictItem, diff);
            }
            //currentToolConflictItem.AllowSelection = !(diffToolConflictItem is ToolConflictItem.Empty)
            //diffToolConflictItem.AllowSelection = !(currentToolConflictItem is ToolConflictItem.Empty)

            var connectors = GetConnectorConflictRows(list, currentToolConflictItem, diffToolConflictItem, current, diff);

            var toolConflictRow = ToolConflictRow.CreateConflictRow(currentToolConflictItem, diffToolConflictItem, connectors);
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
                    row.CurrentArmConnector = ConnectorConflictItem.EmptyConflictItem();
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
                    row.DifferentArmConnector = ConnectorConflictItem.EmptyConflictItem();
                }
                var sameSourceAndDestination = row.CurrentArmConnector.SourceUniqueId != row.DifferentArmConnector.SourceUniqueId
                                    || row.CurrentArmConnector.DestinationUniqueId != row.DifferentArmConnector.DestinationUniqueId;
                var eitherEmpty = row.CurrentArmConnector is ConnectorConflictItem.Empty
                                    || row.DifferentArmConnector is ConnectorConflictItem.Empty;
                row.HasConflict = !eitherEmpty && sameSourceAndDestination;
                rows.Add(row);
            }
            return rows;
        }
    }
}
