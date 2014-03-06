using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Infragistics.Documents.Excel.CalcEngine;

namespace Infragistics.Documents.Excel.FormulaUtilities.CalcEngine
{
	// MD 2/24/12 - 12.1 - Table Support
	internal sealed class TableColumnCalcReference : TableCalcReferenceBase
	{
		#region Member Variables

		private WorksheetTableColumn _column;

		#endregion Member Variables

		#region Constructor

		public TableColumnCalcReference(
			WorksheetRow rowOfFormulaOwner,
			WorksheetTableColumn column, 
			StructuredTableReferenceKeywordType? firstAreaKeyword, 
			StructuredTableReferenceKeywordType? lastAreaKeyword)
			: base(rowOfFormulaOwner, firstAreaKeyword, lastAreaKeyword)
		{
			Debug.Assert(column != null && column.Table != null, "There should be a valid column and table here.");
			_column = column;
		}

		#endregion Constructor

		#region Base Class Overrides

		#region Clone

		protected override TableCalcReferenceBase Clone(WorksheetRow formulaOwnerRow)
		{
			return new TableColumnCalcReference(formulaOwnerRow, _column, this.FirstAreaKeyword, this.LastAreaKeyword);
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
					return string.Format("{0}[{1}]", table.Name, _column.Name);

				// We can use a hard-coded comma here because the string is not shown the the user. It is only used for ID purposes in the calc engine.
				string firstKeyword = FormulaParser.GetKeywordText(this.FirstAreaKeyword.Value);
				if (this.LastAreaKeyword.HasValue == false)
					return string.Format("{0}[[{1}],[{2}]]", table.Name, firstKeyword, _column.Name);

				string lastKeyword = FormulaParser.GetKeywordText(this.LastAreaKeyword.Value);
				return string.Format("{0}[[{1}],[{2}],[{3}]]", table.Name, firstKeyword, lastKeyword, _column.Name);
			}
		}

		#endregion ElementName

		#region Equals

		public override bool Equals(object obj)
		{
			IExcelCalcReference reference = obj as IExcelCalcReference;

			if (reference == null)
				return false;

			TableColumnCalcReference other = ExcelCalcEngine.GetResolvedReference(reference) as TableColumnCalcReference;

			if (other == null)
				return false;

			return
				_column == other._column &&
				this.EqualsHelper(other);
		}

		#endregion Equals

		#region GetHashCode

		public override int GetHashCode()
		{
			return _column.GetHashCode() ^
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

				return _column.GetColumnPortionOfTableRegion(tableRegion);
			}
		}

		#endregion // Region

		#region Table

		public override WorksheetTable Table
		{
			get
			{
				Debug.Assert(_column.Table != null, "The column has been removed from the table.");
				return _column.Table;
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