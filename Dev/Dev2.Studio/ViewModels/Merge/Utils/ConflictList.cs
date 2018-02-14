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
    public class ToolConflictRowList : IEnumerable<IConflictRow>
    {
        readonly ConflictTreeNode[] currentTree;
        readonly ConflictTreeNode[] diffTree;
        readonly IConflictModelFactory modelFactoryCurrent;
        readonly IConflictModelFactory modelFactoryDifferent;
        readonly List<ToolConflictRow> toolConflictRowList;

        public ToolConflictRowList(IConflictModelFactory modelFactoryCurrent, IConflictModelFactory modelFactoryDifferent, List<ConflictTreeNode> currentTree, List<ConflictTreeNode> diffTree)
        {
            this.modelFactoryCurrent = modelFactoryCurrent;
            this.modelFactoryDifferent = modelFactoryDifferent;
            this.currentTree = currentTree.ToArray();
            this.diffTree = diffTree.ToArray();

            toolConflictRowList = new List<ToolConflictRow>();
            CreateList();
        }

        // TODO: Link tools not working as yet, only link start node is working
        //       Set enabled / disabled connector state
        //       Add new switch connector template to allow checkbox
        //       Possible that the Handler is being created before the yield row return happens?

        void CreateList()
        {
            int index = 0;
            var maxCount = Math.Max(currentTree.Length, diffTree.Length);
            for (; index < maxCount; index++)
            {
                ConflictTreeNode current = null;
                ConflictTreeNode diff = null;
                if (index < currentTree.Length)
                {
                    current = currentTree[index];
                }
                if (index < diffTree.Length)
                {
                    diff = diffTree[index];
                }
                // TODO: add an event for when items are added to the list so that we can listen for events on each Item in the row without adding an event listener in here.
                toolConflictRowList.Add(CreateConflictRow(current, diff));
            }
        }

        ToolConflictRow CreateConflictRow(ConflictTreeNode current, ConflictTreeNode diff)
        {
            var row = new ToolConflictRow
            {
                CurrentViewModel = modelFactoryCurrent.CreateToolModelConfictItem(current),
                DiffViewModel = modelFactoryDifferent.CreateToolModelConfictItem(diff),
                Connectors = GenerateConnectorConflictRows(current, diff)
            };

            return row;
        }

        internal static IEnumerable<IConnectorConflictRow> GenerateConnectorConflictRows(ConflictTreeNode current, ConflictTreeNode diff)
        {
            int index = 0;
            var armConnectorsCurrent = current.Activity.ArmConnectors();
            var armConnectorsDiff = diff.Activity.ArmConnectors();
            var maxCount = Math.Max(armConnectorsCurrent.Count, armConnectorsDiff.Count);
            for (; index < maxCount; index++)
            {
                var row = new ConnectorConflictRow();
                if (index < armConnectorsCurrent.Count)
                {
                    var (Description, Key, SourceUniqueId, DestinationUniqueId) = armConnectorsCurrent[index];
                    row.CurrentArmConnector = new ConnectorConflictItem(row.UniqueId, Description, Guid.Parse(SourceUniqueId), Guid.Parse(DestinationUniqueId), Key);
                }
                else
                {
                    // Do we absolutely require empty ConflictItems in our rows?
                }
                if (index < armConnectorsDiff.Count)
                {
                    var (Description, Key, SourceUniqueId, DestinationUniqueId) = armConnectorsDiff[index];
                    row.DifferentArmConnector = new ConnectorConflictItem(row.UniqueId, Description, Guid.Parse(SourceUniqueId), Guid.Parse(DestinationUniqueId), Key);
                }
                else
                {
                    // Do we absolutely require empty ConflictItems in our rows?
                }
                yield return row;
            }
        }

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
            var toolConflictItem = new ToolConflictItem
            {
                MergeDescription = "Start",
                MergeIcon = Application.Current.TryFindResource("System-StartNode") as ImageSource
            };
            var row = new ToolConflictRow
            {
                CurrentViewModel = toolConflictItem,
                DiffViewModel = toolConflictItem,
                IsStartNode = true,
            };
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
                IsStartNode = true
            };
            row.CurrentArmConnector = new ConnectorConflictItem(row.UniqueId, "Start -> " + current.Activity.GetDisplayName(), emptyGuid, Guid.Parse(current.UniqueId), key);
            row.DifferentArmConnector = new ConnectorConflictItem(row.UniqueId, "Start -> " + diff.Activity.GetDisplayName(), emptyGuid, Guid.Parse(diff.UniqueId), key);

            toolConflictRow.Connectors = new List<IConnectorConflictRow> { row };
        }

        public IToolConflictRow GetStartToolRow() => toolConflictRowList[0];

        public IToolConflictItem GetToolItemFromId(Guid id, bool isCurrent)
        {
            IToolConflictItem toolConflictItem;
            if (isCurrent)
            {
                toolConflictItem = toolConflictRowList.FirstOrDefault(tool => tool.CurrentViewModel.UniqueId == id).CurrentViewModel;
            }
            else
            {
                toolConflictItem = toolConflictRowList.FirstOrDefault(tool => tool.DiffViewModel.UniqueId == id).DiffViewModel;
            }
            return toolConflictItem;
        }

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
