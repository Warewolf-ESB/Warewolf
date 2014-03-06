using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.ComponentModel;
using Infragistics.Documents.Excel.Serialization;


using Infragistics.Shared;


namespace Infragistics.Documents.Excel
{
	// MD 3/15/12 - TFS104581
	internal class WorksheetColumnOwnedFormatProxy : WorksheetCellFormatProxy
	{
		#region Constructor

		public WorksheetColumnOwnedFormatProxy(GenericCachedCollectionEx<WorksheetCellFormatData> collection, WorksheetColumn column)
			: base(
			column.Worksheet.GetColumnBlock(column.IndexInternal).CellFormat,
			collection,
			column)
		{
		}

		#endregion // Constructor

		#region Base Class Overrides

		#region AfterSet

		public override void AfterSet(GenericCachedCollection<WorksheetCellFormatData> collection)
		{
			base.AfterSet(collection);

			// After the cell format is changed, save it back to the column block and try to merge it with neighboring blocks
			// Then save the element back from the block if a merge did take place.
			this.GetColumnBlock().CellFormat = this.element;
			WorksheetColumnBlock block = ((WorksheetColumn)this.Owner).OnAfterColumnChange();
			this.element = block.CellFormat;
		}

		#endregion // AfterSet

		#region BeforeSet

		public override GenericCachedCollection<WorksheetCellFormatData> BeforeSet(bool willModifyElement)
		{
			// Split the block before changing any format values so we ensure that the block is only used for a single column.
			// Then save the element back from the split block.
			WorksheetColumnBlock block = ((WorksheetColumn)this.Owner).OnBeforeColumnChange();
			this.element = block.CellFormat;

			// Then call the base implementation, which may clone the current element, so we should save it back to the block
			// because when the Element is asked for during the set operation, we want to get back the cloned element.
			GenericCachedCollection<WorksheetCellFormatData> collection = base.BeforeSet(willModifyElement);
			block.CellFormat = element;

			return collection;
		}

		#endregion // BeforeSet

		#region Element

		public override WorksheetCellFormatData Element
		{
			get
			{
				// Always make sure we have the latest element when it is requested.
				this.element = this.GetColumnBlock().CellFormat;
				return this.element;
			}
		}

		#endregion // Element

		#endregion // Base Class Overrides

		#region Methods

		#region GetColumnBlock

		private WorksheetColumnBlock GetColumnBlock()
		{
			WorksheetColumn column = (WorksheetColumn)this.Owner;
			return column.Worksheet.GetColumnBlock(column.IndexInternal);
		}

		#endregion // GetColumnBlock

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