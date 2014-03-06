using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Infragistics.Documents.Excel.CalcEngine;

namespace Infragistics.Documents.Excel.FormulaUtilities.CalcEngine
{
	// MD 2/24/12 - 12.1 - Table Support
	internal sealed class TableColumnRangeCalcReference : TableCalcReferenceBase
	{
		#region Member Variables

		private WorksheetTableColumn _firstColumn;
		private WorksheetTableColumn _lastColumn;

		#endregion Member Variables

		#region Constructor

		public TableColumnRangeCalcReference(
			WorksheetRow rowOfFormulaOwner,
			WorksheetTableColumn firstColumn,
			WorksheetTableColumn lastColumn,
			StructuredTableReferenceKeywordType? firstAreaKeyword,
			StructuredTableReferenceKeywordType? lastAreaKeyword)
			: base(rowOfFormulaOwner, firstAreaKeyword, lastAreaKeyword)
		{
			Debug.Assert(firstColumn.Table != null && firstColumn.Table == lastColumn.Table, "The columns are not from the same table.");
			_firstColumn = firstColumn;
			_lastColumn = lastColumn;
		}

		#endregion Constructor

		#region Base Class Overrides

		#region Clone

		protected override TableCalcReferenceBase Clone(WorksheetRow formulaOwnerRow)
		{
			return new TableColumnRangeCalcReference(formulaOwnerRow, _firstColumn, _lastColumn, this.FirstAreaKeyword, this.LastAreaKeyword);
		}

		#endregion // Clone

		#region ElementName

		public override string ElementName
		{
			get
			{
				WorksheetTable table = this.Table;
				if (table == null)
					return ExcelReferenceError.Instance.ElementName;

				if (this.FirstAreaKeyword.HasValue == false)
					return string.Format("{0}[[{1}]:[{2}]]", table.Name, _firstColumn.Name, _lastColumn.Name);

				// We can use a hard-coded comma here because the string is not shown the the user. It is only used for ID purposes in the calc engine.
				string firstKeyword = FormulaParser.GetKeywordText(this.FirstAreaKeyword.Value);
				if (this.LastAreaKeyword.HasValue == false)
					return string.Format("{0}[[{1}],[{2}]:[{3}]]", table.Name, firstKeyword, _firstColumn.Name, _lastColumn.Name);

				string lastKeyword = FormulaParser.GetKeywordText(this.LastAreaKeyword.Value);
				return string.Format("{0}[[{1}],[{2}],[{3}]:[{4}]]", table.Name, firstKeyword, lastKeyword, _firstColumn.Name, _lastColumn.Name);
			}
		}

		#endregion ElementName

		#region Equals

		public override bool Equals(object obj)
		{
			IExcelCalcReference reference = obj as IExcelCalcReference;

			if (reference == null)
				return false;

			TableColumnRangeCalcReference other = ExcelCalcEngine.GetResolvedReference(reference) as TableColumnRangeCalcReference;

			if (other == null)
				return false;

			return
				_firstColumn == other._firstColumn &&
				_lastColumn == other._lastColumn && 
				this.EqualsHelper(other);
		}

		#endregion Equals

		#region GetHashCode

		public override int GetHashCode()
		{
			return _firstColumn.GetHashCode() ^
				_lastColumn.GetHashCode() ^
				this.FirstAreaKeyword.GetHashCode() ^
				this.LastAreaKeyword.GetHashCode();
		}

		#endregion GetHashCode

		#region Region

		public override WorksheetRegion Region
		{
			get
			{
				WorksheetRegion tableRegion = this.TableRegion;
				if (tableRegion == null)
					return null;

				WorksheetRegion firstColumnRegion = _firstColumn.GetColumnPortionOfTableRegion(tableRegion);
				WorksheetRegion lastColumnRegion = _lastColumn.GetColumnPortionOfTableRegion(tableRegion);
				return WorksheetRegion.Union(firstColumnRegion, lastColumnRegion);
			}
		}

		#endregion // Region

		#region Table

		public override WorksheetTable Table
		{
			get
			{
				// Use the last column here because if it is still in the table, so is the first column.
				Debug.Assert(_lastColumn.Table != null, "One or more columns have been removed from the table.");
				return _lastColumn.Table;
			}
		}

		#endregion // Table

		#endregion Base Class Overrides
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