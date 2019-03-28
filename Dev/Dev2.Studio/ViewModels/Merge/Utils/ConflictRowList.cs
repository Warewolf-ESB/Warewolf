#pragma warning disable
﻿/*
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
        readonly List<ToolConflictRow> _toolConflictRowList;

        public ConflictRowList(IConflictModelFactory modelFactoryCurrent, IConflictModelFactory modelFactoryDifferent, List<ConflictTreeNode> currentTree, List<ConflictTreeNode> diffTree)
        {
            var _currentTree = currentTree.ToArray();
            var _diffTree = diffTree.ToArray();

            var createConflictRowList = new ConflictRowListBuilder(modelFactoryCurrent, modelFactoryDifferent);
            _toolConflictRowList = createConflictRowList.CreateList(this, _currentTree, _diffTree);

            ConflictTreeNode currentNode = null;
            ConflictTreeNode diffNode = null;

            if(_currentTree.Length > 0)
            {
                currentNode = _currentTree[0];
            }
            if (_diffTree.Length > 0)
            {
                diffNode = _diffTree[0];
            }
            CreateStartRow(currentNode, diffNode);
            BindInboundConnections();
            Ready = true;
        }

        void BindInboundConnections()
        {
            List<IConnectorConflictItem> FindConnectorsForTool(IToolConflictItem conflictItem, Column column)
            {
                var inboundConnectors = new List<IConnectorConflictItem>();

                if (column == Column.Current)
                {
                    inboundConnectors.AddRange(_cacheStartToolRow.Connectors
                                                        .Where(item => item.CurrentArmConnector.DestinationUniqueId == conflictItem.UniqueId)
                                                        .Select(item => item.CurrentArmConnector));
                }
                else
                {
                    inboundConnectors.AddRange(_cacheStartToolRow.Connectors
                                                        .Where(item => item.DifferentArmConnector.DestinationUniqueId == conflictItem.UniqueId)
                                                        .Select(item => item.DifferentArmConnector));
                }

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
            yield return _cacheStartToolRow;
            foreach (var connectorRow in _cacheStartToolRow.Connectors)
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
        private void CreateStartRow(ConflictTreeNode current, ConflictTreeNode diff)
        {
            if (_cacheStartToolRow != null)
            {
                return;
            }

            ImageSource mergeIcon = null;
            if (Application.Current != null)
            {
                // TODO: replace Application.Current.TryFindResource with a mockable resource loader
                mergeIcon = Application.Current.TryFindResource("System-StartNode") as ImageSource;
            }

            var currentConflictItem = ToolConflictItem.NewStartConflictItem(this, Column.Current, mergeIcon);
            var differentConflictItem = ToolConflictItem.NewStartConflictItem(this, Column.Current, mergeIcon);

            var startRow = ToolConflictRow.CreateStartRow(currentConflictItem, differentConflictItem);
            startRow.Connectors = CreateStartNodeConnectors(currentConflictItem, differentConflictItem, current, diff);

            _cacheStartToolRow = startRow;
        }

        IList<IConnectorConflictRow> CreateStartNodeConnectors(IToolConflictItem currentConflictItem, IToolConflictItem differentConflictItem, ConflictTreeNode current, ConflictTreeNode diff)
        {
            const string key = "Start";
            var emptyGuid = Guid.Empty;
            var row = new ConnectorConflictRow
            {
                Key = key,
                ContainsStart = true
            };
            if (current == null)
            {
                row.CurrentArmConnector = new ConnectorConflictItem.Empty(emptyGuid);
            }
            else
            {
                var connector = new ConnectorConflictItem(this, Column.Current, row.UniqueId, "Start -> " + current.Activity.GetDisplayName(), emptyGuid, Guid.Parse(current.UniqueId), key);
                row.CurrentArmConnector = connector;
                currentConflictItem.OutboundConnectors.Add(connector);
            }
            if (diff == null)
            {
                row.DifferentArmConnector = new ConnectorConflictItem.Empty(emptyGuid);
            }
            else
            {
                var connector = new ConnectorConflictItem(this, Column.Different, row.UniqueId, "Start -> " + diff.Activity.GetDisplayName(), emptyGuid, Guid.Parse(diff.UniqueId), key);
                row.DifferentArmConnector = connector;
                differentConflictItem.OutboundConnectors.Add(connector);
            }
            return new List<IConnectorConflictRow> { row };
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

        public int Count
        {
            get
            {
                var enumerator = GetEnumerator();
                int i = 0;
                while (enumerator.MoveNext())
                {
                    i++;
                }
                return i;
            }
        }

        public IConflictRow this[int key]
        {
            get
            {
                var enumerator = GetEnumerator();
                int i = 0;
                while (enumerator.MoveNext())
                {
                    if (i == key)
                    {
                        return enumerator.Current;
                    }
                    i++;
                }
                throw new IndexOutOfRangeException();
            }
        }

        public bool Ready
        {
            get; private set;
        }
    }
}
