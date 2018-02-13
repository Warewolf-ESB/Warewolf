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

namespace Dev2.ViewModels.Merge.Utils
{
    public class ToolModelConflictRowList : IEnumerable<IConflictRow>
    {
        readonly ConflictTreeNode[] currentTree;
        readonly ConflictTreeNode[] diffTree;
        readonly IConflictModelFactory modelFactoryCurrent;
        readonly IConflictModelFactory modelFactoryDifferent;
        readonly List<ToolConflictRow> toolConflictRowList;

        public ToolModelConflictRowList(IConflictModelFactory modelFactoryCurrent, IConflictModelFactory modelFactoryDifferent, List<ConflictTreeNode> currentTree, List<ConflictTreeNode> diffTree)
        {
            this.modelFactoryCurrent = modelFactoryCurrent;
            this.modelFactoryDifferent = modelFactoryDifferent;
            this.currentTree = currentTree.ToArray();
            this.diffTree = diffTree.ToArray();

            toolConflictRowList = new List<ToolConflictRow>();
            CreateList();
        }

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
            var row = new ToolConflictRow();

            var id = Guid.Parse(current.UniqueId);
            row.UniqueId = id;
            row.CurrentViewModel = modelFactoryCurrent.CreateToolModelConfictItem(current);
            row.DiffViewModel = modelFactoryDifferent.CreateToolModelConfictItem(diff);

            row.Connectors = GenerateConnectorConflictRows(current, diff);

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
                    row.CurrentArmConnector = new ConnectorConflictItem(Description, Guid.Parse(SourceUniqueId), Guid.Parse(DestinationUniqueId), Key);
                }
                else
                {
                    // Do we absolutely require empty ConflictItems in our rows?
                }
                if (index < armConnectorsDiff.Count)
                {
                    var (Description, Key, SourceUniqueId, DestinationUniqueId) = armConnectorsDiff[index];
                    row.DifferentArmConnector = new ConnectorConflictItem(Description, Guid.Parse(SourceUniqueId), Guid.Parse(DestinationUniqueId), Key);
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
            const string description = "Start";
            var row = new ToolConflictRow
            {
                UniqueId = Guid.Empty,
                CurrentViewModel = new ToolConflictItem { MergeDescription = description },
                DiffViewModel = new ToolConflictItem { MergeDescription = description }
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
                UniqueId = emptyGuid,
                Key = key,
                CurrentArmConnector = new ConnectorConflictItem("Start -> " + current.Activity.GetDisplayName(), emptyGuid, Guid.Parse(current.UniqueId), key),
                DifferentArmConnector = new ConnectorConflictItem("Start -> " + diff.Activity.GetDisplayName(), emptyGuid, Guid.Parse(diff.UniqueId), key)
            };
            toolConflictRow.Connectors = new List<IConnectorConflictRow> { row };
        }

        public IToolConflictRow GetStartToolRow() => toolConflictRowList[0];

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
