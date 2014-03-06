using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Infragistics.Documents.Excel
{
	// MD 11/1/10 - TFS56976





	internal class FixedLengthSegmentTree<T>
	{
		#region Member Variables

		private SegmentTreeNode head;
		private int maxValue; 

		// MD 3/22/12 - TFS105885
		// Note: this is not the number of regions in the tree, because a region can exist on multiple nodes.
		// It is the sum of the counts of regions stored on all nodes.
		private int totalCount;

		#endregion // Member Variables

		#region Constructor

		public FixedLengthSegmentTree(int maxValue)
		{
			this.maxValue = maxValue;
		} 

		#endregion // Constructor

		#region Methods

		// MD 1/23/12 - TFS99849
		#region Clear

		public void Clear()
		{
			this.head = null;
		}

		#endregion // Clear

		#region CreateNode



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		private SegmentTreeNode CreateNode(int segmentStartValue, int segmentEndValue, int minimumNodeWidth)
		{
			while (true)
			{
				int nodeStartValue = segmentStartValue - (segmentStartValue % minimumNodeWidth);
				int nodeEndValue = nodeStartValue + minimumNodeWidth - 1;

				if (this.maxValue <= nodeEndValue)
				{
					Utilities.DebugFail("The end value of the node is out of range.");
					return null;
				}

				// The node start value is guaranteed to be less than the segment start value, so just check to make sure the node end value is
				// greater than or equal to the segment end value.
				if (segmentEndValue <= nodeEndValue)
					return new SegmentTreeNode(nodeStartValue, nodeEndValue);

				// If not, double the width of the node and try again.
				minimumNodeWidth *= 2;
			}
		}

		#endregion // CreateNode

		// MD 1/23/12 - TFS99849
		#region GetBoundingIndexes

		public void GetBoundingIndexes(out int firstValue, out int lastValue)
		{
			firstValue = -1;
			lastValue = -1;

			if (this.head != null)
			{
				this.head.GetBoundaryValue(true, ref firstValue);
				this.head.GetBoundaryValue(false, ref lastValue);
			}
			else
			{
				firstValue = -1;
				lastValue = -1;
			}
		}

		#endregion // GetBoundingIndexes

		// MD 1/23/12 - TFS99849
		#region GetItemsContainingRange






		public List<T> GetItemsContainingRange(int start, int end)
		{
			List<T> items = null;

			// Only go into the head node if the range for the head node fully contains the specified range.
			if (this.head != null && this.head.nodeStartValue <= start && end <= this.head.nodeEndValue)
				this.head.GetItemsContainingRange(start, end, ref items);

			return items;
		}

		#endregion // GetItemsContainingRange

		#region GetItemsContainingValue






		public List<T> GetItemsContainingValue(int value)
		{
			List<T> items = null;

			// MD 1/23/12
			// Found while fixing TFS99849
			// The head may not cover the value, so only start the recursion into it if the head covers the item.
			//if (this.head != null)
			if (this.head != null && this.head.nodeStartValue <= value && value <= this.head.nodeEndValue)
				this.head.GetItemsContainingValue(value, ref items);

			return items;
		}

		#endregion // GetItemsContainingValue

		// MD 1/23/12 - TFS99849
		#region GetItemsIntersectingWithRange






		public List<T> GetItemsIntersectingWithRange(int start, int end)
		{
			List<T> items = null;

			// Only go into the head node if the range for the head node intersects with the specified range.
			if (this.head != null && start <= this.head.nodeEndValue && this.head.nodeStartValue <= end)
				this.head.GetItemsIntersectingWithRange(start, end, ref items);

			return items;
		}

		#endregion // GetItemsIntersectingWithRange

		#region Insert



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		public void Insert(T item, int segmentStartValue, int segmentEndValue)
		{
			if (this.head == null)
			{
				// If there is no head node, create it.
				const int DefaultStartingNodeWidth = 8;
				this.head = this.CreateNode(segmentStartValue, segmentEndValue, DefaultStartingNodeWidth);
			}
			else if (segmentStartValue < this.head.nodeStartValue || this.head.nodeEndValue < segmentEndValue)
			{
				// If the current head node is not wide enough for the segment of the value being inserted, create a new head node which is wide 
				// enough for both the new segment as well as the existing tree stored under the head node.
				int headNodeWidth = this.head.nodeEndValue - this.head.nodeStartValue + 1;
				SegmentTreeNode oldHead = this.head;

				this.head = this.CreateNode(
					Math.Min(segmentStartValue, this.head.nodeStartValue),
					Math.Max(segmentEndValue, this.head.nodeEndValue),
					headNodeWidth);

				// Ass the old tree into the new head node so it will be a subtree of it.
				this.head.SetExistingChildNode(oldHead);
			}

			// Insert the value into the head node.
			// MD 3/22/12 - TFS105885
			//this.head.Insert(item, segmentStartValue, segmentEndValue);
			this.head.Insert(this, item, segmentStartValue, segmentEndValue);
		} 

		#endregion // Insert

		#region Remove



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		public void Remove(T item, int segmentStartValue, int segmentEndValue)
		{
			// MD 3/22/12 - TFS105885
			//this.head.Remove(item, segmentStartValue, segmentEndValue);
			if (this.head != null)
				this.head.Remove(this, item, segmentStartValue, segmentEndValue);
		}

		#endregion // Remove

		#endregion // Methods

		#region Properties

		// MD 3/22/12 - TFS105885
		#region IsEmpty

		public bool IsEmpty
		{
			get { return this.totalCount == 0; }
		}

		#endregion // IsEmpty

		#endregion // Properties


		#region SegmentTreeNode class

		private class SegmentTreeNode
		{
			#region Member Variables

			internal int nodeStartValue;
			internal int nodeEndValue;

			private List<T> itemsCoveringSubtree;

			private SegmentTreeNode leftChild;
			private SegmentTreeNode rightChild; 

			#endregion // Member Variables

			#region Constructor

			public SegmentTreeNode(int nodeStartValue, int nodeEndValue)
			{
				this.nodeStartValue = nodeStartValue;
				this.nodeEndValue = nodeEndValue;
			} 

			#endregion // Constructor

			#region Methods

			// MD 1/23/12 - TFS99849
			#region GetBoundaryValue

			public void GetBoundaryValue(bool getStartBoundary, ref int value)
			{
				// First try to get the boundary value in the child node.
				SegmentTreeNode childNode = getStartBoundary ? this.leftChild : this.rightChild;
				if (childNode != null)
					childNode.GetBoundaryValue(getStartBoundary, ref value);

				// If we didn't find the boundary in the child nodes and there is at least one item on this node, 
				// this node contains the boundary.
				if (value < 0 && this.itemsCoveringSubtree != null && this.itemsCoveringSubtree.Count != 0)
				{
					value = getStartBoundary ? this.nodeStartValue : this.nodeEndValue;
					return;
				}

				// If we didn't find anything on the correct side or the node itself, go into the other side.
				childNode = getStartBoundary ? this.rightChild : this.leftChild;
				if (childNode != null)
					childNode.GetBoundaryValue(getStartBoundary, ref value);
			}

			#endregion // GetBoundaryValue

			// MD 1/23/12 - TFS99849
			#region GetItemsContainingRange






			public void GetItemsContainingRange(int start, int end, ref List<T> items)
			{
				// If this node does not contains the full range, stop the search. This node and nothing below it can contain the range.
				if (start < this.nodeStartValue || this.nodeEndValue < end)
					return;

				if (this.itemsCoveringSubtree != null && this.itemsCoveringSubtree.Count > 0)
				{
					if (items == null)
						items = new List<T>();

					items.AddRange(this.itemsCoveringSubtree);
				}

				// If this node represent a single value, it is a leaf node, so just return.
				if (this.nodeStartValue == this.nodeEndValue)
					return;

				int midIndex = (this.nodeEndValue + this.nodeStartValue) / 2;

				bool isStartInLeftNode = start <= midIndex;
				bool isEndInLeftNode = end <= midIndex;

				// If both the start and end are in the same subtree, recursively go into that subtree.
				if (isStartInLeftNode == isEndInLeftNode)
				{
					if (isStartInLeftNode)
					{
						if (this.leftChild != null)
							this.leftChild.GetItemsContainingRange(start, end, ref items);
					}
					else
					{
						if (this.rightChild != null)
							this.rightChild.GetItemsContainingRange(start, end, ref items);
					}

					return;
				}

				Debug.Assert(isStartInLeftNode && isEndInLeftNode == false, "Something is wrong here.");

				// Otherwise, find the two sets of items containing each end point of the range and the intersection
				// of those sets contains the full range.
				if (this.leftChild == null || this.rightChild == null)
					return;

				List<T> itemsConveringStart = null;
				this.leftChild.GetItemsContainingValue(start, ref itemsConveringStart);
				if (itemsConveringStart == null)
					return;

				List<T> itemsCoveringEnd = null;
				this.rightChild.GetItemsContainingValue(end, ref itemsCoveringEnd);
				if (itemsCoveringEnd == null)
					return;

				List<T> rangesCoveringBothEndPoints = Utilities.IntersetLists(itemsConveringStart, itemsCoveringEnd);
				if (rangesCoveringBothEndPoints == null || rangesCoveringBothEndPoints.Count == 0)
					return;

				if (items == null)
					items = new List<T>();

				items.AddRange(rangesCoveringBothEndPoints);
			}

			#endregion // GetItemsContainingRange

			#region GetItemsContainingValue






			public void GetItemsContainingValue(int value, ref List<T> items)
			{
				// If we hit this node, all segments on this node contain the value, so add all items to the list.
				if (this.itemsCoveringSubtree != null && this.itemsCoveringSubtree.Count > 0)
				{
					if (items == null)
						items = new List<T>();

					items.AddRange(this.itemsCoveringSubtree);
				}

				// If this node represent a single value, it is a leaf node, so just return.
				if (this.nodeStartValue == this.nodeEndValue)
					return;

				int midIndex = (this.nodeEndValue + this.nodeStartValue) / 2;

				// If the value is in the range of the left node, also get the values in that node, otherwise, get the values in the right child.
				if (value <= midIndex)
				{
					if (this.leftChild != null)
						this.leftChild.GetItemsContainingValue(value, ref items);
				}
				else
				{
					if (this.rightChild != null)
						this.rightChild.GetItemsContainingValue(value, ref items);
				}
			}

			#endregion // GetItemsContainingValue

			// MD 1/23/12 - TFS99849
			#region GetItemsIntersectingWithRange






			public void GetItemsIntersectingWithRange(int start, int end, ref List<T> items)
			{
				// If this node does not intersect with the range at all, return;
				if (end < this.nodeStartValue || this.nodeEndValue < start)
					return;

				if (this.itemsCoveringSubtree != null && this.itemsCoveringSubtree.Count > 0)
				{
					if (items == null)
						items = new List<T>();

					items.AddRange(this.itemsCoveringSubtree);
				}

				// If this node represent a single value, it is a leaf node, so just return.
				if (this.nodeStartValue == this.nodeEndValue)
					return;

				int midIndex = (this.nodeEndValue + this.nodeStartValue) / 2;

				// If the range partially covers the right subtree, recursively call into it.
				if (start <= midIndex && this.leftChild != null)
					this.leftChild.GetItemsIntersectingWithRange(start, end, ref items);

				// If the range partially covers the left subtree, recursively call into it.
				if (midIndex < end && this.rightChild != null)
					this.rightChild.GetItemsIntersectingWithRange(start, end, ref items);
			}

			#endregion // GetItemsIntersectingWithRange

			#region Insert



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

			// MD 3/22/12 - TFS105885
			//public void Insert(T item, int segmentStartValue, int segmentEndValue)
			public void Insert(FixedLengthSegmentTree<T> owner, T item, int segmentStartValue, int segmentEndValue)
			{
				// If the segment associated with the item completely covers this node's range (and therefore, its subtree's range), store the 
				// item on the node and don't go any lower in the subtree.
				if (segmentStartValue <= this.nodeStartValue && this.nodeEndValue <= segmentEndValue)
				{
					if (this.itemsCoveringSubtree == null)
						this.itemsCoveringSubtree = new List<T>();

					this.itemsCoveringSubtree.Add(item);

					// MD 3/22/12 - TFS105885
					owner.totalCount++;
					return;
				}

				int midValue = (this.nodeEndValue + this.nodeStartValue) / 2;

				// If the segment doesn't cover the entire node, but it does intersect with the left child's range, insert it into the left child.
				if (segmentStartValue <= midValue)
				{
					// Create the left node if necessary.
					if (this.leftChild == null)
						this.leftChild = new FixedLengthSegmentTree<T>.SegmentTreeNode(this.nodeStartValue, midValue);

					// MD 3/22/12 - TFS105885
					//this.leftChild.Insert(item, segmentStartValue, segmentEndValue);
					this.leftChild.Insert(owner, item, segmentStartValue, segmentEndValue);
				}

				// If the segment doesn't cover the entire node, but it does intersect with the right child's range, insert it into the right child.
				if (midValue < segmentEndValue)
				{
					// Create the right node if necessary.
					if (this.rightChild == null)
						this.rightChild = new FixedLengthSegmentTree<T>.SegmentTreeNode(midValue + 1, this.nodeEndValue);

					// MD 3/22/12 - TFS105885
					//this.rightChild.Insert(item, segmentStartValue, segmentEndValue);
					this.rightChild.Insert(owner, item, segmentStartValue, segmentEndValue);
				}
			} 

			#endregion // Insert

			#region Remove



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

			// MD 3/22/12 - TFS105885
			//public void Remove(T item, int segmentStartValue, int segmentEndValue)
			public void Remove(FixedLengthSegmentTree<T> owner, T item, int segmentStartValue, int segmentEndValue)
			{
				// If the segment associated with the item completely covers this node's range (and therefore, its subtree's range), the item was 
				// stored on this node, so remove it from this node. It was not stored on any children of this node, so we can stop here.
				if (segmentStartValue <= this.nodeStartValue && this.nodeEndValue <= segmentEndValue)
				{
					if (this.itemsCoveringSubtree != null)
					{
						// MD 3/22/12 - TFS105885
						//this.itemsCoveringSubtree.Remove(item);
						if (this.itemsCoveringSubtree.Remove(item))
							owner.totalCount--;
					}
					return;
				}

				int midValue = (this.nodeEndValue + this.nodeStartValue) / 2;

				// If the segment doesn't cover the entire node, but it does intersect with the left child's range, remove it from the left child.
				if (segmentStartValue <= midValue)
				{
					if (this.leftChild != null)
					{
						// MD 3/22/12 - TFS105885
						//this.leftChild.Remove(item, segmentStartValue, segmentEndValue);
						this.leftChild.Remove(owner, item, segmentStartValue, segmentEndValue);
					}
				}

				// If the segment doesn't cover the entire node, but it does intersect with the right child's range, remove it from the right child.
				if (midValue < segmentEndValue)
				{
					if (this.rightChild != null)
					{
						// MD 3/22/12 - TFS105885
						//this.rightChild.Remove(item, segmentStartValue, segmentEndValue);
						this.rightChild.Remove(owner, item, segmentStartValue, segmentEndValue);
					}
				}
			}

			#endregion // Remove

			#region SetExistingChildNode






			public void SetExistingChildNode(SegmentTreeNode existingNode)
			{
				int midValue = (this.nodeEndValue + this.nodeStartValue) / 2;

				if (existingNode.nodeEndValue <= midValue)
				{
					if (existingNode.nodeStartValue == this.nodeStartValue && existingNode.nodeEndValue == midValue)
					{
						this.leftChild = existingNode;
					}
					else
					{
						this.leftChild = new FixedLengthSegmentTree<T>.SegmentTreeNode(this.nodeStartValue, midValue);
						this.leftChild.SetExistingChildNode(existingNode);
					}
				}
				else if (midValue < existingNode.nodeStartValue)
				{
					if (existingNode.nodeStartValue == (midValue + 1) && existingNode.nodeEndValue == this.nodeEndValue)
					{
						this.rightChild = existingNode;
					}
					else
					{
						this.rightChild = new FixedLengthSegmentTree<T>.SegmentTreeNode(midValue + 1, this.nodeEndValue);
						this.rightChild.SetExistingChildNode(existingNode);
					}
				}
			}

			#endregion // SetExistingChildNode

			#endregion // Methods
		} 

		#endregion // SegmentTreeNode class
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