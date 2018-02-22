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
        public enum Column
        {
            Current,
            Different
        }
        readonly ConflictTreeNode[] _currentTree;
        readonly ConflictTreeNode[] _diffTree;
        readonly List<ToolConflictRow> _toolConflictRowList;

        public ConflictRowList(IConflictModelFactory modelFactoryCurrent, IConflictModelFactory modelFactoryDifferent, List<ConflictTreeNode> currentTree, List<ConflictTreeNode> diffTree)
        {
            _currentTree = currentTree.ToArray();
            _diffTree = diffTree.ToArray();

            var createConflictRowList = new ConflictRowListBuilder(modelFactoryCurrent, modelFactoryDifferent);
            _toolConflictRowList = createConflictRowList.CreateList(this, _currentTree, _diffTree);
            BindInboundConnections();
            Ready = true;
        }


        void BindInboundConnections()
        {
            List<IConnectorConflictItem> FindConnectorsForTool(IToolConflictItem conflictItem, Column column)
            {
                var inboundConnectors = new List<IConnectorConflictItem>();
                foreach (var row in _toolConflictRowList)
                {
                    if (column == Column.Current)
                    {
                        var toolConnectors = row.Connectors
                                                        .Where(item => item.CurrentArmConnector.DestinationUniqueId == conflictItem.UniqueId)
                                                        .Select(item => item.CurrentArmConnector);
                        inboundConnectors.AddRange(toolConnectors);
                    } else
                    {
                        var toolConnectors = row.Connectors
                                                        .Where(item => item.DifferentArmConnector.DestinationUniqueId == conflictItem.UniqueId)
                                                        .Select(item => item.DifferentArmConnector);
                        inboundConnectors.AddRange(toolConnectors);
                    }
                }
                return inboundConnectors;
            }
            foreach (var row in _toolConflictRowList)
            {
                if (!(row.CurrentViewModel is ToolConflictItem.Empty))
                {
                    row.CurrentViewModel.InboundConnectors = FindConnectorsForTool(row.CurrentViewModel, Column.Current);
                }
                if (!(row.DiffViewModel is ToolConflictItem.Empty))
                {
                    row.DiffViewModel.InboundConnectors = FindConnectorsForTool(row.DiffViewModel, Column.Different);
                }
            }
        }

        internal bool ActivityIsInWorkflow(IDev2Activity activity)
        {
            foreach (var row in _toolConflictRowList)
            {
                var current = row.CurrentViewModel;
                if (current.Activity != null && current.Activity.Equals(activity) && current.IsChecked)
                {
                    return true;
                }
                var diff = row.DiffViewModel;
                if (diff.Activity != null && diff.Activity.Equals(activity) && diff.IsChecked)
                {
                    return true;
                }
            }
            return false;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerator<IConflictRow> GetEnumerator()
        {
            var startToolRow = CreateStartRow(_currentTree[0], _diffTree[0]);
            yield return startToolRow;
            foreach (var connectorRow in startToolRow.Connectors)
            {
                yield return connectorRow;
            }

            foreach (var toolRow in _toolConflictRowList)
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
            var toolConflictItem = ToolConflictItem.NewStartConflictItem(this, Column.Current, mergeIcon);

            var row = ToolConflictRow.CreateStartRow(toolConflictItem, new ToolConflictItem.Empty());
            CreateStartNodeConnectors(row, current, diff);

            return _cacheStartToolRow = row;
        }

        void CreateStartNodeConnectors(ToolConflictRow toolConflictRow, ConflictTreeNode current, ConflictTreeNode diff)
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
            row.CurrentArmConnector = new ConnectorConflictItem(this, Column.Current, row.UniqueId, "Start -> " + current.Activity.GetDisplayName(), emptyGuid, Guid.Parse(current.UniqueId), key);
            row.DifferentArmConnector = new ConnectorConflictItem(this, Column.Different, row.UniqueId, "Start -> " + diff.Activity.GetDisplayName(), emptyGuid, Guid.Parse(diff.UniqueId), key);

            toolConflictRow.Connectors = new List<IConnectorConflictRow> { row };
        }

        public IToolConflictRow GetStartToolRow() => _toolConflictRowList[0];

        public IToolConflictItem GetToolItemFromIdCurrent(Guid id) => _toolConflictRowList.FirstOrDefault(tool => tool.CurrentViewModel.UniqueId == id)?.CurrentViewModel;
        public IToolConflictItem GetToolItemFromIdDifferent(Guid id) => _toolConflictRowList.FirstOrDefault(tool => tool.DiffViewModel.UniqueId == id)?.DiffViewModel;

        public IConnectorConflictItem GetConnectorItemFromToolIdCurrent(Guid id)
        {
            var foundTool = _toolConflictRowList.FirstOrDefault(tool => tool.CurrentViewModel.UniqueId == id);
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
            var foundTool = _toolConflictRowList.FirstOrDefault(tool => tool.DiffViewModel.UniqueId == id);
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

        public int Count => _toolConflictRowList.Count;

        public ToolConflictRow this[int key]
        {
            get
            {
                return _toolConflictRowList[key];
            }
            set
            {
                _toolConflictRowList[key] = value;
            }
        }

        public bool Ready
        {
            get; private set;
        }
    }
}
