using System;




namespace Infragistics.Documents.Excel

{
	// MD 7/23/10 - TFS35969
	internal abstract class RowColumnBinaryTreeNode<T> : LoadOnDemandTree<T>.BinaryTreeNode 
		where T : RowColumnBase
	{
		#region Member Variables

		private int totalExtent = -1;
		private int totalExtentIgnoreHidden = -1;

		#endregion // Member Variables

		#region Constructor

		public RowColumnBinaryTreeNode(int index, IBinaryTreeNodeOwner<T> parent, LoadOnDemandTree<T> tree)
			: base(index, parent, tree) { } 

		#endregion // Constructor

		#region Methods

		#region CalculateTotalHeight

		public int CalculateCumulativeHeight(int numberOfItems, bool ignoreHidden, int defaultItemExtent, int unallocatedItemExtent)
		{
			int totalHeight = 0;

			for (int i = 0; i < numberOfItems; i++)
			{
				T item = this.values[i];

				if (item == null)
				{
					totalHeight += unallocatedItemExtent;
					continue;
				}

				if (ignoreHidden == false && item.Hidden)
					continue;

				int extent = this.GetItemExtent(item, i);

				if (extent < 0)
					totalHeight += defaultItemExtent;
				else
					totalHeight += extent;
			}

			return totalHeight;
		}

		#endregion // CalculateTotalHeight

		#region GetItemExtent

		protected abstract int GetItemExtent(T item, int relativeIndex);

		#endregion // GetItemExtent

		#region GetTotalExtent

		public int GetTotalExtent(bool ignoreHidden, int defaultRowHeight, int unallocatedRowHeight)
		{
			if (ignoreHidden)
			{
				if (this.totalExtentIgnoreHidden < 0)
					this.totalExtentIgnoreHidden = this.CalculateCumulativeHeight(LoadOnDemandTree<T>.BTreeLoadFactor, true, defaultRowHeight, unallocatedRowHeight);

				return this.totalExtentIgnoreHidden;
			}

			if (this.totalExtent < 0)
				this.totalExtent = this.CalculateCumulativeHeight(LoadOnDemandTree<T>.BTreeLoadFactor, false, defaultRowHeight, unallocatedRowHeight);

			return this.totalExtent;
		}

		#endregion // GetTotalExtent

		#region ResetExtentCache

		public virtual void ResetExtentCache(bool onlyVisibilityChanged)
		{
			this.totalExtent = -1;

			if (onlyVisibilityChanged == false)
				this.totalExtentIgnoreHidden = -1;
		}

		#endregion // ResetExtentCache 

		#endregion // Methods
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