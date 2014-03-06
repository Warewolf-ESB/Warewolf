using System;
using System.Collections.Generic;

namespace Infragistics.Documents.Excel
{
	// MD 7/23/10 - TFS35969
	internal class RowColumnLoadOnDemandTree<T> : LoadOnDemandTree<T> where T : RowColumnBase
	{
		#region Member Variables

		protected Worksheet worksheet; 

		#endregion // Member Variables

		#region Constructor

		public RowColumnLoadOnDemandTree(RowColumnCollectionBase<T> owner)
			: base(owner)
		{
			this.worksheet = owner.Worksheet;
		} 

		#endregion // Constructor

		#region GetDistanceToBeginningOfItems

		protected void GetDistanceToBeginningOfItems(
			int firstItemIndex,
			int lastItemIndex,
			bool ignoreHidden,
			out int distanceToFirstItem,
			out int distanceToLastItem,
			int defaultItemExtent,
			int unallocatedItemExtent)
		{
			distanceToFirstItem = 0;
			distanceToLastItem = 0;

			if (firstItemIndex == 0 && lastItemIndex == 0)
				return;

			// Determine the amount of distance we should add for missing nodes.
			int missingNodeHeight = unallocatedItemExtent * RowColumnLoadOnDemandTree<T>.BTreeLoadFactor;

			// Since we only want to get to the beginning of the items, decrease the lastItemIndex so it points 
			// to the last items whose extent we want in the accumulation.
			lastItemIndex--;

			int startIndexOfNodeContainingItem;

			bool foundFirstItem = false;
			if (firstItemIndex == 0)
			{
				foundFirstItem = true;
				startIndexOfNodeContainingItem = lastItemIndex - lastItemIndex % RowColumnLoadOnDemandTree<T>.BTreeLoadFactor;
			}
			else
			{
				// Since we only want to get to the beginning of the items, decrease the lastItemIndex so it points 
				// to the last items whose extent we want in the accumulation.
				firstItemIndex--;
				startIndexOfNodeContainingItem = firstItemIndex - firstItemIndex % RowColumnLoadOnDemandTree<T>.BTreeLoadFactor;
			}

			// This keeps track of where the next node's start index should be, so if the next node is passed the assumed next node,
			// we can add in the distance for the amount of missing nodes in between the two consecutive nodes.
			int nextNodeStartIndex = 0;

			int missingNodes;
			int missingNodesHeight;

			// Enumerate over the nodes in order
			foreach (RowColumnBinaryTreeNode<T> currentNode in this.GetNodesEnumerator())
			{
				// Determine the distance skipped over from the last node to this node.
				missingNodes = (Math.Min(startIndexOfNodeContainingItem, currentNode.FirstItemIndex) - nextNodeStartIndex) / RowColumnLoadOnDemandTree<T>.BTreeLoadFactor;
				missingNodesHeight = missingNodes * missingNodeHeight;
				distanceToFirstItem += foundFirstItem ? 0 : missingNodesHeight;
				distanceToLastItem += missingNodesHeight;

				// If we have passed the node containing the current item, the item and its node are unallocated.
				if (startIndexOfNodeContainingItem < currentNode.FirstItemIndex)
				{
					// If we are looking for the first item, estimate the distance to it and update the startIndexOfNodeContainingItem
					// to point to the node containing the last item.
					if (foundFirstItem == false)
					{
						distanceToFirstItem += unallocatedItemExtent * (firstItemIndex - startIndexOfNodeContainingItem);

						foundFirstItem = true;
						startIndexOfNodeContainingItem = lastItemIndex - lastItemIndex % RowColumnLoadOnDemandTree<T>.BTreeLoadFactor;
					}

					// If we were looking for the last item or we just found the first item and the last item is in the same unallocated 
					// node, estimate the distance to it and return.
					if (startIndexOfNodeContainingItem < currentNode.FirstItemIndex)
					{
						distanceToLastItem += unallocatedItemExtent * (lastItemIndex - startIndexOfNodeContainingItem);
						return;
					}
				}

				// If the item we are looking for is in the current node, find the distance to that item in the node.
				if (currentNode.FirstItemIndex == startIndexOfNodeContainingItem)
				{
					// If we are looking for the first item, get the distance to it and update the startIndexOfNodeContainingItem
					// to point to the node containing the last item.
					if (foundFirstItem == false)
					{
						distanceToFirstItem += RowColumnLoadOnDemandTree<T>.GetDistanceToItemInNode(
							firstItemIndex,
							ignoreHidden,
							currentNode,
							defaultItemExtent,
							unallocatedItemExtent);

						foundFirstItem = true;
						startIndexOfNodeContainingItem = lastItemIndex - lastItemIndex % RowColumnLoadOnDemandTree<T>.BTreeLoadFactor;
					}

					// If we were looking for the last item or we just found the first item and the last item is also in the current
					// node, get the distance to it and return.
					if (currentNode.FirstItemIndex == startIndexOfNodeContainingItem)
					{
						distanceToLastItem += RowColumnLoadOnDemandTree<T>.GetDistanceToItemInNode(
							lastItemIndex,
							ignoreHidden,
							currentNode,
							defaultItemExtent,
							unallocatedItemExtent);

						return;
					}
					// MD 2/7/11
					// If we just found the node with the first item and the node with the last item is in a subsequent node, 
					// we have to add in the current node's extent.
					else if (currentNode.FirstItemIndex < startIndexOfNodeContainingItem)
					{
						// If this node is before the node containing the last item, add in the distance for it.
						distanceToLastItem += currentNode.GetTotalExtent(ignoreHidden, defaultItemExtent, unallocatedItemExtent);
					}
				}
				else if (currentNode.FirstItemIndex < startIndexOfNodeContainingItem)
				{
					// If this node is before the node containing the item we are looking for, add in the distance for it.
					int nodeHeight = currentNode.GetTotalExtent(ignoreHidden, defaultItemExtent, unallocatedItemExtent);
					distanceToFirstItem += foundFirstItem ? 0 : nodeHeight;
					distanceToLastItem += nodeHeight;
				}

				// Update the index of the next node we should expect.
				nextNodeStartIndex = currentNode.FirstItemIndex + RowColumnLoadOnDemandTree<T>.BTreeLoadFactor;
			}

			// If we went through all nodes without finding the items, estimate the distance to them now.
			if (foundFirstItem == false)
				distanceToFirstItem += (firstItemIndex - nextNodeStartIndex + 1) * unallocatedItemExtent;

			distanceToLastItem += (lastItemIndex - nextNodeStartIndex + 1) * unallocatedItemExtent;
		}

		#endregion // GetDistanceToBeginningOfItems

		#region GetDistanceToItemInNode

		private static int GetDistanceToItemInNode(
			int itemIndex, 
			bool ignoreHidden, 
			RowColumnBinaryTreeNode<T> currentNode, 
			int defaultItemExtent, 
			int unallocatedItemExtent)
		{
			// Determine the number of item whose extents need to be added.
			int countOfItemsInNode = itemIndex - currentNode.FirstItemIndex + 1;

			// If all the items need to be totaled, get the total extent of the node because it may be cached.
			if (countOfItemsInNode == RowColumnLoadOnDemandTree<T>.BTreeLoadFactor)
				return currentNode.GetTotalExtent(ignoreHidden, defaultItemExtent, unallocatedItemExtent);

			// Otherwise, total up the extents of the items.
			return currentNode.CalculateCumulativeHeight(
				countOfItemsInNode,
				ignoreHidden,
				defaultItemExtent,
				unallocatedItemExtent);
		} 

		#endregion // GetDistanceToItemInNode

		#region ResetExtentCache

		public void ResetExtentCache(bool onlyVisibilityChanged)
		{
			// Clear the cache on all nodes.
			foreach (RowColumnBinaryTreeNode<T> node in this.GetNodesEnumerator())
				node.ResetExtentCache(onlyVisibilityChanged);
		}

		public void ResetExtentCache(int index, bool onlyVisibilityChanged)
		{
			if (this.head == null)
				return;

			// Get the node which has to be reset.
			FindState state;
			RowColumnBinaryTreeNode<T> node = (RowColumnBinaryTreeNode<T>)this.head.FindOrAdd(index, false, this, out state);

			// If it was found, reset the cache on the node.
			if (state == FindState.ValueFound)
				node.ResetExtentCache(onlyVisibilityChanged);
		} 

		#endregion // ResetExtentCache
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