using System.Collections.Generic;
using Dev2.Common.Interfaces;
using System.Collections;
using Dev2.Common;
using System;

namespace Dev2.ViewModels.Merge.Utils
{
    public class ToolModelConflictRowList : IEnumerable<IConflictRow>
    {
        readonly ConflictTreeNode[] currentTree;
        readonly ConflictTreeNode[] diffTree;
        readonly ConflictModelFactory modelFactoryCurrent;
        readonly ConflictModelFactory modelFactoryDifferent;
        readonly List<IConflictRow> list;

        public ToolModelConflictRowList(ConflictModelFactory modelFactoryCurrent, ConflictModelFactory modelFactoryDifferent, List<ConflictTreeNode> currentTree, List<ConflictTreeNode> diffTree)
        {
            this.modelFactoryCurrent = modelFactoryCurrent;
            this.modelFactoryDifferent = modelFactoryDifferent;
            this.currentTree = currentTree.ToArray();
            this.diffTree = diffTree.ToArray();
            list = new List<IConflictRow>();
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
                list.Add(CreateConflictRow(current, diff));
            }
        }

        public IEnumerator<IConflictRow> GetEnumerator()
        {
            
            yield break;
        }

        IConflictRow CreateConflictRow(ConflictTreeNode current, ConflictTreeNode diff)
        {
            var row = new ToolConflictRow();
            
            var id = Guid.Parse(current.UniqueId);
            row.UniqueId = id;
            row.CurrentViewModel = modelFactoryCurrent.CreateToolModelConfictItem(current);
            row.DiffViewModel = modelFactoryDifferent.CreateToolModelConfictItem(diff);



            row.CurrentViewModel.Container = row;
            row.DiffViewModel.Container = row;

            return new ToolConflictRow
            {
                
            };
        }

        IEnumerator IEnumerable.GetEnumerator()
        {

            yield break;
        }

        public int Count => 0;
        public int IndexOf(IConflictRow conflict) => 0;

        public IConflictRow GetNextConlictToUpdate(IConflictRow container)
        {
            return null;
        }
    }
}
