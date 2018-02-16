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
    public class CreateConflictRowList
    {
        readonly IConflictModelFactory modelFactoryCurrent;
        readonly IConflictModelFactory modelFactoryDifferent;

        public CreateConflictRowList(IConflictModelFactory modelFactoryCurrent, IConflictModelFactory modelFactoryDifferent)
        {
            this.modelFactoryCurrent = modelFactoryCurrent;
            this.modelFactoryDifferent = modelFactoryDifferent;
        }

        public List<ToolConflictRow> CreateList(ConflictTreeNode[] currentTree, ConflictTreeNode[] diffTree)
        {
            var toolConflictRowList = new List<ToolConflictRow>();
            var maxCount = Math.Max(currentTree.Length, diffTree.Length);
            int indexDiff = 0;
            int indexCurr = 0;

            while (indexDiff < maxCount)
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

                bool diffFoundInCurrent = currentTree.Contains(diff);
                bool currFoundInDifferent = diffTree.Contains(current);

                var toolConflictRow = BuildToolConflictRow(current, diff, diffFoundInCurrent, currFoundInDifferent);
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

        private ToolConflictRow BuildToolConflictRow(ConflictTreeNode current, ConflictTreeNode diff, bool diffFoundInCurrent, bool currFoundInDifferent)
        {
            bool found = diffFoundInCurrent && currFoundInDifferent;

            IToolConflictItem currentViewModel = null;
            IToolConflictItem diffViewModel = null;
            if (!found)
            {
                if (!diffFoundInCurrent)
                {
                    currentViewModel = ToolConflictItem.EmptyConflictItem();
                }
                if (!currFoundInDifferent)
                {
                    diffViewModel = ToolConflictItem.EmptyConflictItem();
                }
            }

            if (currentViewModel == null)
            {
                currentViewModel = modelFactoryCurrent.CreateToolModelConfictItem(current);
            }
            if (diffViewModel == null)
            {
                diffViewModel = modelFactoryDifferent.CreateToolModelConfictItem(diff);
            }
            currentViewModel.AllowSelection = !(diffViewModel is ToolConflictItem.Empty);
            diffViewModel.AllowSelection = !(currentViewModel is ToolConflictItem.Empty);

            var connectors = GetConnectorConflictRows(current, diff);

            var toolConflictRow = ToolConflictRow.CreateConflictRow(currentViewModel, diffViewModel, connectors);
            return toolConflictRow;
        }

        private static List<IConnectorConflictRow> GetConnectorConflictRows(ConflictTreeNode current, ConflictTreeNode diff)
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
                    row.CurrentArmConnector = new ConnectorConflictItem(row.UniqueId, Description, Guid.Parse(SourceUniqueId), Guid.Parse(DestinationUniqueId), Key);
                }
                else
                {
                    row.CurrentArmConnector = ConnectorConflictItem.EmptyConflictItem();
                }
                if (armConnectorsDiff != null && index < armConnectorsDiff.Count)
                {
                    var (Description, Key, SourceUniqueId, DestinationUniqueId) = armConnectorsDiff[index];
                    row.DifferentArmConnector = new ConnectorConflictItem(row.UniqueId, Description, Guid.Parse(SourceUniqueId), Guid.Parse(DestinationUniqueId), Key);
                }
                else
                {
                    row.DifferentArmConnector = ConnectorConflictItem.EmptyConflictItem();
                }
                row.CurrentArmConnector.AllowSelection = !(row.DifferentArmConnector is ConnectorConflictItem.Empty);
                row.DifferentArmConnector.AllowSelection = !(row.CurrentArmConnector is ConnectorConflictItem.Empty);
                rows.Add(row);
            }
            return rows;
        }
    }

    public class ConflictRowList : IEnumerable<IConflictRow>
    {
        readonly ConflictTreeNode[] currentTree;
        readonly ConflictTreeNode[] diffTree;
        readonly List<ToolConflictRow> toolConflictRowList;

        public ConflictRowList(IConflictModelFactory modelFactoryCurrent, IConflictModelFactory modelFactoryDifferent, List<ConflictTreeNode> currentTree, List<ConflictTreeNode> diffTree)
        {
            this.currentTree = currentTree.ToArray();
            this.diffTree = diffTree.ToArray();

            var createConflictRowList = new CreateConflictRowList(modelFactoryCurrent, modelFactoryDifferent);
            toolConflictRowList = createConflictRowList.CreateList(this.currentTree, this.diffTree);
        }

        // TODO: Set enabled / disabled connector state
        //       Add new switch connector template to allow checkbox

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerator<IConflictRow> GetEnumerator()
        {
            var startToolRow = CreateStartRow(currentTree[0], diffTree[0]);
            yield return startToolRow;
            foreach (var connectorRow in startToolRow.Connectors)
            {
                yield return connectorRow;
            }

            foreach (var toolRow in toolConflictRowList)
            {
                yield return toolRow;

                foreach (var connectorRow in toolRow.Connectors)
                {
                    yield return connectorRow;
                }
            }
        }

        ToolConflictRow _cacheStartToolRow;
        private ToolConflictRow CreateStartRow(ConflictTreeNode current, ConflictTreeNode diff)
        {
            if (_cacheStartToolRow != null)
            {
                return _cacheStartToolRow;
            }

            var mergeIcon = Application.Current.TryFindResource("System-StartNode") as ImageSource;
            var toolConflictItem = ToolConflictItem.NewStartConflictItem(mergeIcon);

            var row = ToolConflictRow.CreateStartRow(toolConflictItem, toolConflictItem);
            CreateStartNodeConnectors(row, current, diff);

            return _cacheStartToolRow = row;
        }

        static void CreateStartNodeConnectors(ToolConflictRow toolConflictRow, ConflictTreeNode current, ConflictTreeNode diff)
        {
            if (toolConflictRow.Connectors != null && toolConflictRow.Connectors.Any())
            {
                return;
            }
            const string key = "Start";
            var emptyGuid = Guid.Empty;
            var row = new ConnectorConflictRow
            {
                Key = key,
                ContainsStart = true
            };
            row.CurrentArmConnector = new ConnectorConflictItem(row.UniqueId, "Start -> " + current.Activity.GetDisplayName(), emptyGuid, Guid.Parse(current.UniqueId), key);
            row.DifferentArmConnector = new ConnectorConflictItem(row.UniqueId, "Start -> " + diff.Activity.GetDisplayName(), emptyGuid, Guid.Parse(diff.UniqueId), key);

            toolConflictRow.Connectors = new List<IConnectorConflictRow> { row };
        }

        public IToolConflictRow GetStartToolRow() => toolConflictRowList[0];

        public IToolConflictItem GetToolItemFromId(Guid id, bool isCurrent) => isCurrent
                                                                                        ? toolConflictRowList.FirstOrDefault(tool => tool.CurrentViewModel.UniqueId == id).CurrentViewModel
                                                                                        : toolConflictRowList.FirstOrDefault(tool => tool.DiffViewModel.UniqueId == id).DiffViewModel;

        public IConnectorConflictItem GetConnectorItemFromToolId(Guid id, bool isCurrent) => isCurrent
                                                                                        ? toolConflictRowList.FirstOrDefault(tool => tool.CurrentViewModel.UniqueId == id)
                                                                                          .Connectors.FirstOrDefault(con => con.CurrentArmConnector?.SourceUniqueId == id).CurrentArmConnector
                                                                                        : toolConflictRowList.FirstOrDefault(tool => tool.DiffViewModel.UniqueId == id)
                                                                                          .Connectors.FirstOrDefault(con => con.DifferentArmConnector?.SourceUniqueId == id).DifferentArmConnector;

        public int Count => toolConflictRowList.Count;

        public ToolConflictRow this[int key]
        {
            get
            {
                return toolConflictRowList[key];
            }
            set
            {
                toolConflictRowList[key] = value;
            }
        }
    }
}
