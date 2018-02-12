using System.Collections.Generic;
using Dev2.Common.Interfaces;
using System.Collections;
using Dev2.Common;
using System;
using Dev2.Studio.Interfaces;

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

            row.CurrentViewModel.Container = row;
            row.DiffViewModel.Container = row;
            row.HasConflict = true;
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
