using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using System.ComponentModel;


using Infragistics.Shared; 


namespace Infragistics.Documents.Excel
{
	// MD 4/12/11 - TFS67084
	internal sealed class WorksheetCellOwnedFormatProxy : WorksheetCellFormatProxy
	{
		#region Member Variables

		private WorksheetCellFormatData[] cellFormatBlock;

		// MD 3/5/12 - 12.1 - Table Support
		private int cellShiftHistoryVersion;

		private short columnIndex;

		#endregion // Member Variables

		#region Constructors

		// MD 1/29/12 - 12.1 - Cell Format Updates
		// This is not used.
		//public WorksheetCellOwnedFormatProxy(WorksheetCellFormatCollection parentCollection, WorksheetRow row, short columnIndex, WorksheetCellFormatData[] cellFormatBlock)
		//    : base(parentCollection, row)
		//{
		//    this.columnIndex = columnIndex;
		//    this.cellFormatBlock = cellFormatBlock;
		//}

		public WorksheetCellOwnedFormatProxy(WorksheetCellFormatData element, WorksheetCellFormatCollection parentCollection, WorksheetRow row, short columnIndex, WorksheetCellFormatData[] cellFormatBlock)
			: base(element, parentCollection, row)
		{
			this.columnIndex = columnIndex;
			this.cellFormatBlock = cellFormatBlock;

			// MD 3/5/12 - 12.1 - Table Support
			this.cellShiftHistoryVersion = row.Worksheet.CellShiftHistoryVersion;
		} 

		#endregion // Constructors

		#region Base Class Overrides

		#region AfterSet

		// MD 12/21/11 - 12.1 - Table Support
		// The proxies no longer store a reference to the collection. It is stored on the element, so it needs to be passed into the AfterSet.
		//public override void AfterSet()
		//{
		//    base.AfterSet();
		public override void AfterSet(GenericCachedCollection<WorksheetCellFormatData> collection)
		{
			base.AfterSet(collection);

			this.UpdateCellFormatKeyOnCell();
		}

		#endregion // AfterSet

		#region BeforeSet

		// MD 12/21/11 - 12.1 - Table Support
		// The proxies no longer store a reference to the collection. It is stored on the element, so it needs to be returned from the BeforeSet.
		//public override void BeforeSet(bool needsElementAfter)
		public override GenericCachedCollection<WorksheetCellFormatData> BeforeSet(bool needsElementAfter)
		{
			// MD 3/5/12 - 12.1 - Table Support
			this.VerifyCellAddress();

			// MD 12/21/11 - 12.1 - Table Support
			// The proxies no longer store a reference to the collection. It is stored on the element.
			//if (this.Collection != null && this.Row.Worksheet.Workbook == null)
			//    this.Collection = null;

			if (needsElementAfter == false)
			{
				// MD 12/21/11 - 12.1 - Table Support
				//base.BeforeSet(needsElementAfter);
				//return;
				return base.BeforeSet(needsElementAfter);
			}

			// MD 12/21/11 - 12.1 - Table Support
			//base.BeforeSet(needsElementAfter);
			GenericCachedCollection<WorksheetCellFormatData> collection = base.BeforeSet(needsElementAfter);

			this.UpdateCellFormatKeyOnCell();

			// MD 12/21/11 - 12.1 - Table Support
			return collection;
		}

		#endregion // BeforeSet

		#region Element

		public override WorksheetCellFormatData Element
		{
			get
			{
				// MD 3/5/12 - 12.1 - Table Support
				this.VerifyCellAddress();

				this.element = this.cellFormatBlock[this.columnIndex % WorksheetRow.CellBlockSize];
				return this.element;
			}
		}

		#endregion // Element

		#endregion // Base Class Overrides

		#region Methods

		#region UpdateCellFormatKeyOnCell

		private void UpdateCellFormatKeyOnCell()
		{
			this.cellFormatBlock[columnIndex % WorksheetRow.CellBlockSize] = this.element;
		}

		#endregion // UpdateCellFormatKeyOnCell

		// MD 3/5/12 - 12.1 - Table Support
		#region VerifyCellAddress

		private void VerifyCellAddress()
		{
			if (this.cellFormatBlock != null)
			{
				WorksheetRow originalRow = (WorksheetRow)this.Owner;
				short originalColumnIndex = this.columnIndex;

				WorksheetRow row = (WorksheetRow)this.Owner;
				Worksheet worksheet = row.Worksheet;
				if (worksheet == null || worksheet.VerifyCellAddress(ref row, ref this.columnIndex, ref this.cellShiftHistoryVersion) == false)
				{
					this.SetOwner(null);
					this.columnIndex = -1;
					this.cellFormatBlock = null;
					this.element = null;
				}
				else if (row != originalRow || this.columnIndex != originalColumnIndex)
				{
					this.SetOwner(row);
					this.cellFormatBlock = row.GetCellFormatBlock(this.columnIndex);
					this.element = row.GetCellFormatInternal(this.cellFormatBlock, this.columnIndex).Element;
				}
			}

			if (this.cellFormatBlock == null)
				throw new InvalidOperationException(SR.GetString("LE_ArgumentException_CellShiftedOffWorksheet"));
		}

		#endregion // VerifyCellAddress

		#endregion // Methods

		#region Properties

		#region ColumnIndex

		public short ColumnIndex
		{
			get { return this.columnIndex; }
		}

		#endregion // ColumnIndex

		#region ElementInternal

		public WorksheetCellFormatData ElementInternal
		{
			get { return this.element; }
			set { this.element = value; }
		}

		#endregion // ElementInternal

		// MD 3/5/12 - 12.1 - Table Support
		// This is not used anywhere.
		#region Removed

		//#region Row

		//public WorksheetRow Row
		//{
		//    get { return (WorksheetRow)this.Owner; }
		//}

		//#endregion // Row

		#endregion // Removed

		#endregion // Properties
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