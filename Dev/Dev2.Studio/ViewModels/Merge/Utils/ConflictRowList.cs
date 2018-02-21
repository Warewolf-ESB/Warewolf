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
    
    public class ConflictRowList : IEnumerable<IConflictRow>
    {
        readonly ConflictTreeNode[] currentTree;
        readonly ConflictTreeNode[] diffTree;
        readonly List<ToolConflictRow> toolConflictRowList;

        public ConflictRowList(IConflictModelFactory modelFactoryCurrent, IConflictModelFactory modelFactoryDifferent, List<ConflictTreeNode> currentTree, List<ConflictTreeNode> diffTree)
        {
            this.currentTree = currentTree.ToArray();
            this.diffTree = diffTree.ToArray();

            var createConflictRowList = new ConflictRowListBuilder(modelFactoryCurrent, modelFactoryDifferent);
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

        public IToolConflictItem GetToolItemFromIdCurrent(Guid id) => toolConflictRowList.FirstOrDefault(tool => tool.CurrentViewModel.UniqueId == id).CurrentViewModel;
        public IToolConflictItem GetToolItemFromIdDifferent(Guid id) => toolConflictRowList.FirstOrDefault(tool => tool.DiffViewModel.UniqueId == id).DiffViewModel;

        public IConnectorConflictItem GetConnectorItemFromToolIdCurrent(Guid id)
        {
            var foundTool = toolConflictRowList.FirstOrDefault(tool => tool.CurrentViewModel.UniqueId == id);
            if (foundTool == null)
            {
                return null;
            }
            var connector = foundTool.Connectors.FirstOrDefault(con => con.CurrentArmConnector?.SourceUniqueId == id);
            if (connector == null)
            {
                return null;
            }
            return connector.CurrentArmConnector;
        }
        public IConnectorConflictItem GetConnectorItemFromToolIdDifferent(Guid id)
        {
            var foundTool = toolConflictRowList.FirstOrDefault(tool => tool.DiffViewModel.UniqueId == id);
            if (foundTool == null)
            {
                return null;
            }
            var connector = foundTool.Connectors.FirstOrDefault(con => con.DifferentArmConnector?.SourceUniqueId == id);
            if (connector == null)
            {
                return null;
            }

            return connector.DifferentArmConnector;
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
