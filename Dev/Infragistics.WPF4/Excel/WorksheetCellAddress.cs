using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using System.ComponentModel;
using Infragistics.Documents.Excel.FormulaUtilities;

namespace Infragistics.Documents.Excel
{
	// MD 4/12/11 - TFS67084
	internal struct WorksheetCellAddress
	{
		public static readonly WorksheetCellAddress InvalidReference = new WorksheetCellAddress(-1, -1);

		#region Member Variables

		private short columnIndex;

		// MD 3/27/12 - 12.1 - Table Support
		//private WorksheetRow row;
		private int rowIndex;

		#endregion  // Member Variables

		#region Constructor

		// MD 3/27/12 - 12.1 - Table Support
		//public WorksheetCellAddress(WorksheetRow row, short columnIndex)
		//{
		//    this.row = row;
		//    this.columnIndex = columnIndex;
		//}
		public WorksheetCellAddress(int rowIndex, short columnIndex)
		{
			this.rowIndex = rowIndex;
			this.columnIndex = columnIndex;
		}

		#endregion  // Constructor

		#region Base Class Overrides

		#region Equals

		public override bool Equals(object obj)
		{
			if ((obj is WorksheetCellAddress) == false)
				return false;

			WorksheetCellAddress other = (WorksheetCellAddress)obj;

			// MD 3/27/12 - 12.1 - Table Support
			//if (this.row != other.row)
			if (this.rowIndex != other.rowIndex)
				return false;

			return this.columnIndex == other.columnIndex;
		}

		#endregion  // Equals

		#region GetHashCode

		public override int GetHashCode()
		{
			// MD 3/27/12 - 12.1 - Table Support
			//return this.row.GetHashCode() ^ this.columnIndex.GetHashCode();
			return this.rowIndex ^ (this.columnIndex << 16);
		}

		#endregion  // GetHashCode

		#region ToString

		public override string ToString()
		{
			return this.ToString(false, false, Workbook.LatestFormat, CellReferenceMode.A1);
		}

		#endregion // ToString

		#endregion  // Base Class Overrides

		#region Operators

		#region ==

		public static bool operator ==(WorksheetCellAddress a, WorksheetCellAddress b)
		{
			return
				a.columnIndex == b.columnIndex &&
				a.rowIndex == b.rowIndex;
		}

		#endregion // ==

		#region !=

		public static bool operator !=(WorksheetCellAddress a, WorksheetCellAddress b)
		{
			return !(a == b);
		}

		#endregion // !=

		#endregion // Operators

		#region Methods

		#region ToString

		public string ToString(bool useRelativeRow, bool useRelativeColumn, WorkbookFormat format, CellReferenceMode cellReferenceMode)
		{
			if (this.IsValid == false)
				return FormulaParser.ReferenceErrorValue;

			return CellAddress.GetCellReferenceString(
				this.rowIndex, this.columnIndex, useRelativeRow, useRelativeColumn,
				format, this.rowIndex, this.columnIndex, false, cellReferenceMode);
		}

		#endregion // ToString

		#endregion // Methods

		#region Properties

		#region ColumnIndex

		public short ColumnIndex
		{
			get { return this.columnIndex; }
		}

		#endregion  // ColumnIndex

		#region IsValid

		public bool IsValid
		{
			get { return this.rowIndex >= 0; }
		}

		#endregion // IsValid

		// MD 3/27/12 - 12.1 - Table Support
		#region Old Code

		//#region Row

		//public WorksheetRow Row
		//{
		//    get { return this.row; }
		//}

		//#endregion  // Row

		#endregion // Old Code
		#region RowIndex

		public int RowIndex
		{
			get { return this.rowIndex; }
			set { this.rowIndex = value; }
		}

		#endregion  // Row

		#endregion  // Properties
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