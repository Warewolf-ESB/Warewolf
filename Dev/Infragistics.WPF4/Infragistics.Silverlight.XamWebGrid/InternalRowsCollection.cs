using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;
using Infragistics.Collections;

namespace Infragistics.Controls.Grids.Primitives
{
	internal class InternalRowsCollection : CollectionBase<RowBase>
	{
		#region Properties

		public RowsManager RootRowsManager
		{
			get;
			set;
		}

		#endregion // Proeprties

		#region Methods

		private int RecursiveGetCount(RowsManagerBase manager)
		{
			int count = manager.FullRowCount;

			foreach (RowsManagerBase childManager in manager.VisibleChildManagers)
				count += this.RecursiveGetCount(childManager);

			return count;

		}

		private static int GetOffsetIndex(RowsManagerBase manager)
		{
			int offset = 0;
			while (manager.ParentRow != null)
			{
				offset += manager.ParentRow.Manager.ResolveIndexForRow(manager.ParentRow) + 1;
				manager = manager.ParentRow.Manager;
			}
			return offset;
		}

		private RowBase GetRowForIndex(int index, RowsManagerBase currentManager, int currentOffset, int additionalOffset)
		{
			int childOffset = 0;

			foreach (RowsManagerBase visibleChildManager in currentManager.VisibleChildManagers)
			{
				int offset = InternalRowsCollection.GetOffsetIndex(visibleChildManager) + childOffset + additionalOffset;

				if (index < offset)
					return currentManager.ResolveRowForIndex(index - currentOffset - childOffset);

				int range = this.RecursiveGetCount(visibleChildManager);

				if (index >= offset && index < (offset + range))
				{
					return this.GetRowForIndex(index, visibleChildManager, offset, childOffset + additionalOffset);
				}
				childOffset += range;
			}

			return currentManager.ResolveRowForIndex(index - currentOffset - childOffset);
		}

		private bool GetIndexOfRow(RowsManagerBase currentManager, RowBase row, ref int index)
		{
			if (currentManager == row.Manager)
			{
				RowsManager manager = row.Manager as RowsManager;
				if (manager != null)
				{
					int resolvedIndex = manager.ResolveIndexForRow(row);
					int actualIndex = resolvedIndex;
					foreach (RowsManagerBase childManager in currentManager.VisibleChildManagers)
					{
						int offsetIndex = manager.ResolveIndexForRow(childManager.ParentRow);
						if (resolvedIndex > offsetIndex)
							actualIndex += RecursiveGetCount(childManager);
					}

					// Add the Calculated Index + the Offset of the Manager
					index += actualIndex + GetOffsetIndex(currentManager);
					return true;
				}
				ChildBandRowsManager cbRowsManager = row.Manager as ChildBandRowsManager;
				if (cbRowsManager != null)
				{
					int resolvedIndex = cbRowsManager.ResolveIndexForRow(row);
					int actualIndex = resolvedIndex;
					foreach (RowsManagerBase childManager in currentManager.VisibleChildManagers)
					{
						int offsetIndex = cbRowsManager.ResolveIndexForRow(childManager.ParentRow);
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
				foreach (RowsManagerBase childManager in currentManager.VisibleChildManagers)
				{
					if (this.GetIndexOfRow(childManager, row, ref index))
						return true;
					else
					{
						if (childManager is ChildBandRowsManager)
							index += childManager.FullRowCount;
						if(childManager is RowsManager)
							index += childManager.FullRowCount; //  RecursiveGetCount(childManager);
					}
				}

				return false;
			}
		}

		#endregion // Methods

		#region Overrides

		protected override int GetCount()
		{
			return this.RecursiveGetCount(this.RootRowsManager);
		}

		protected override RowBase GetItem(int index)
		{
			if (this.RootRowsManager.VisibleChildManagers.Count == 0)
			{
				return this.RootRowsManager.Rows[index];
			}
			else
			{
				return this.GetRowForIndex(index, this.RootRowsManager, 0, 0);
			}
		}

		public override int IndexOf(RowBase item)
		{
			int index = 0;
			this.GetIndexOfRow(this.RootRowsManager, item, ref index);
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