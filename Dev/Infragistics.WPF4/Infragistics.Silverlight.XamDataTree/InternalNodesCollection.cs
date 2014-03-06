using Infragistics.Collections;

namespace Infragistics.Controls.Menus
{
    internal class InternalNodesCollection : CollectionBase<XamDataTreeNode>
    {
        #region Properties

        public NodesManager RootNodesManager
        {
            get;
            set;
        }

        #endregion // Proeprties

        #region Methods

        private int RecursiveGetCount(NodesManager manager)
        {
            int count = manager.FullNodeCount;

            foreach (NodesManager childManager in manager.VisibleChildManagers)
                count += this.RecursiveGetCount(childManager);

            return count;

        }

        private static int GetOffsetIndex(NodesManager manager)
        {
            int offset = 0;
            while (manager.ParentNode != null)
            {
                offset += manager.ParentNode.Manager.ResolveIndexForNode(manager.ParentNode) + 1;
                manager = manager.ParentNode.Manager;
            }
            return offset;
        }

        private XamDataTreeNode GetNodeForIndex(int index, NodesManager currentManager, int currentOffset, int additionalOffset)
        {
            int childOffset = 0;

            foreach (NodesManager visibleChildManager in currentManager.VisibleChildManagers)
            {
                int offset = InternalNodesCollection.GetOffsetIndex(visibleChildManager) + childOffset + additionalOffset;

                if (index < offset)
                    return currentManager.ResolveNodeForIndex(index - currentOffset - childOffset);

                int range = this.RecursiveGetCount(visibleChildManager);

                if (index >= offset && index < (offset + range))
                {
                    return this.GetNodeForIndex(index, visibleChildManager, offset, childOffset + additionalOffset);
                }
                childOffset += range;
            }

            return currentManager.ResolveNodeForIndex(index - currentOffset - childOffset);
        }

        private bool GetIndexOfNode(NodesManager currentManager, XamDataTreeNode node, ref int index)
        {
            if (currentManager == node.Manager)
            {
                NodesManager manager = node.Manager;
                if (manager != null)
                {
                    int resolvedIndex = manager.ResolveIndexForNode(node);
                    int actualIndex = resolvedIndex;
                    foreach (NodesManager childManager in currentManager.VisibleChildManagers)
                    {
                        int offsetIndex = manager.ResolveIndexForNode(childManager.ParentNode);
                        if (resolvedIndex > offsetIndex)
                            actualIndex += RecursiveGetCount(childManager);
                    }

                    // Add the Calculated Index + the Offset of the Manager
                    index += actualIndex + GetOffsetIndex(currentManager);
                    return true;
                }
                return false;
            }
            else
            {
                foreach (NodesManager childManager in currentManager.VisibleChildManagers)
                {
                    if (this.GetIndexOfNode(childManager, node, ref index))
                    {
                        return true;
                    }
                    else
                    {
                        index += childManager.FullNodeCount;
                    }
                }
                return false;
            }
        }

        #endregion // Methods

        #region Overrides

        protected override int GetCount()
        {
            return this.RecursiveGetCount(this.RootNodesManager);
        }

        protected override XamDataTreeNode GetItem(int index)
        {
            if (this.RootNodesManager.VisibleChildManagers.Count == 0)
            {
                return this.RootNodesManager.Nodes[index];
            }
            else
            {
                return this.GetNodeForIndex(index, this.RootNodesManager, 0, 0);
            }
        }

        public override int IndexOf(XamDataTreeNode item)
        {
            int index = 0;
            this.GetIndexOfNode(this.RootNodesManager, item, ref index);
            return index;
        }

        #endregion // Overrides
    }
}

#region Copyright (c) 2001-2012 Infragistics, Inc. All Rights Reserved
/* ---------------------------------------------------------------------*
*                           Infragistics, Inc.                          *
*              Copyright (c) 2001-2012 All Rights reserved               *
*                                                                       *
*                                                                       *
* This file and its contents are protected by United States and         *
* International copyright laws.  Unauthorized reproduction and/or       *
* distribution of all or any portion of the code contained herein       *
* is strictly prohibited and will result in severe civil and criminal   *
* penalties.  Any violations of this copyright will be prosecuted       *
* to the fullest extent possible under law.                             *
*                                                                       *
* THE SOURCE CODE CONTAINED HEREIN AND IN RELATED FILES IS PROVIDED     *
* TO THE REGISTERED DEVELOPER FOR THE PURPOSES OF EDUCATION AND         *
* TROUBLESHOOTING. UNDER NO CIRCUMSTANCES MAY ANY PORTION OF THE SOURCE *
* CODE BE DISTRIBUTED, DISCLOSED OR OTHERWISE MADE AVAILABLE TO ANY     *
* THIRD PARTY WITHOUT THE EXPRESS WRITTEN CONSENT OF INFRAGISTICS, INC. *
*                                                                       *
* UNDER NO CIRCUMSTANCES MAY THE SOURCE CODE BE USED IN WHOLE OR IN     *
* PART, AS THE BASIS FOR CREATING A PRODUCT THAT PROVIDES THE SAME, OR  *
* SUBSTANTIALLY THE SAME, FUNCTIONALITY AS ANY INFRAGISTICS PRODUCT.    *
*                                                                       *
* THE REGISTERED DEVELOPER ACKNOWLEDGES THAT THIS SOURCE CODE           *
* CONTAINS VALUABLE AND PROPRIETARY TRADE SECRETS OF INFRAGISTICS,      *
* INC.  THE REGISTERED DEVELOPER AGREES TO EXPEND EVERY EFFORT TO       *
* INSURE ITS CONFIDENTIALITY.                                           *
*                                                                       *
* THE END USER LICENSE AGREEMENT (EULA) ACCOMPANYING THE PRODUCT        *
* PERMITS THE REGISTERED DEVELOPER TO REDISTRIBUTE THE PRODUCT IN       *
* EXECUTABLE FORM ONLY IN SUPPORT OF APPLICATIONS WRITTEN USING         *
* THE PRODUCT.  IT DOES NOT PROVIDE ANY RIGHTS REGARDING THE            *
* SOURCE CODE CONTAINED HEREIN.                                         *
*                                                                       *
* THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE.              *
* --------------------------------------------------------------------- *
*/
#endregion Copyright (c) 2001-2012 Infragistics, Inc. All Rights Reserved