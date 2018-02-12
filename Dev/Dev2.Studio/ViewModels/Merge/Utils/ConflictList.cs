using System.Collections.Generic;
using Dev2.Common.Interfaces;
using System.Collections;
using Dev2.Common;
using System;
using Dev2.Studio.Interfaces;
using System.Linq;
using System.Activities.Statements;

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
                    var connectorCurr = armConnectorsCurrent[index];
                    row.CurrentArmConnector =
                        new ConnectorConflictItem(connectorCurr.Description,
                            Guid.Parse(connectorCurr.SourceUniqueId), Guid.Parse(connectorCurr.DestinationUniqueId),
                            connectorCurr.Key);
                }
                else
                {
                    // Do we absolutely require empty ConflictItems in our rows?
                }
                if (index < armConnectorsDiff.Count)
                {
                    var connectorDiff = armConnectorsDiff[index];
                    row.DifferentArmConnector = new ConnectorConflictItem(connectorDiff.Description,
                            Guid.Parse(connectorDiff.SourceUniqueId), Guid.Parse(connectorDiff.DestinationUniqueId),
                            connectorDiff.Key);
                }
                else
                {
                    // Do we absolutely require empty ConflictItems in our rows?
                }
                yield return row;
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



        public IEnumerator<IConflictRow> GetEnumerator()
        {
            // TODO: return the _real_ rows and the connectors in their correct order
            foreach (var toolRow in toolConflictRowList)
            {
                yield return toolRow;

                foreach (var connectorRow in toolRow.Connectors)
                {
                    yield return connectorRow;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int Count => toolConflictRowList.Count;
        public int IndexOf(IConflictRow conflict) => 0;

        // TODO: remove?
        public IConflictRow GetNextConlictToUpdate(IConflictRow container)
        {
            return null;
        }


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
