using System;
using System.Globalization;

namespace Infragistics.Documents.Excel.FormulaUtilities
{
	internal class FormulaContext
	{
		#region Member Variables

		private readonly CellReferenceMode cellReferenceMode = CellReferenceMode.A1;
		private readonly CultureInfo culture;
		private readonly WorkbookFormat format = Workbook.LatestFormat;
		private readonly Formula formula;
		private readonly WorksheetCellAddress owningCellAddress = WorksheetCellAddress.InvalidReference;
		private readonly Workbook workbook;
		private readonly Worksheet worksheet;

		#endregion // Member Variables

		#region Constructor

		public FormulaContext(Workbook workbook)
		{
			this.workbook = workbook;

			if (this.workbook != null)
			{
				this.format = this.workbook.CurrentFormat;
				this.culture = this.workbook.CultureResolved;
			}
			else
			{
				this.culture = CultureInfo.CurrentCulture;
			}
		}

		public FormulaContext(Workbook workbook, Formula formula)
			: this(workbook ?? formula.Workbook)
		{
			this.formula = formula;
			this.worksheet = this.formula.Worksheet;

			if (this.formula.OwningCellRow != null)
				this.owningCellAddress = new WorksheetCellAddress(this.formula.OwningCellRow.Index, this.formula.OwningCellColumnIndex);

			if (this.workbook == null)
				this.format = formula.CurrentFormat;
		}

		public FormulaContext(Formula formula, CellReferenceMode cellReferenceMode, CultureInfo culture)
			: this(null, formula)
		{
			this.culture = culture;
			this.cellReferenceMode = cellReferenceMode;
		}

		public FormulaContext(Workbook workbook, Worksheet worksheet, WorksheetRow owningCellRow, short owningCellColumnIndex, Formula formula)
			: this(workbook)
		{
			this.formula = formula;
			this.worksheet = worksheet;

			if (owningCellRow != null)
				this.owningCellAddress = new WorksheetCellAddress(owningCellRow.Index, owningCellColumnIndex);
		}

		public FormulaContext(Worksheet worksheet, int owningCellRowIndex, short owningCellColumnIndex, WorkbookFormat format, Formula formula)
			: this(worksheet == null ? null : worksheet.Workbook)
		{
			this.format = format;
			this.formula = formula;
			this.worksheet = worksheet;

			if (0 <= owningCellRowIndex && 0 <= owningCellColumnIndex)
				this.owningCellAddress = new WorksheetCellAddress(owningCellRowIndex, owningCellColumnIndex);
		}

		#endregion // Constructor

		#region Properties

		public CellReferenceMode CellReferenceMode
		{
			get { return this.cellReferenceMode; }
		}

		public CultureInfo Culture
		{
			get { return this.culture; }
		}

		public WorkbookFormat Format
		{
			get { return this.format; }
		}

		public Formula Formula
		{
			get { return this.formula; }
		}

		public WorksheetCellAddress OwningCellAddress
		{
			get { return this.owningCellAddress; }
		}

		public WorksheetRow OwningRow
		{
			get
			{
				if (this.worksheet == null || this.owningCellAddress == WorksheetCellAddress.InvalidReference)
					return null;

				return this.worksheet.Rows[this.owningCellAddress.RowIndex];
			}
		}

		public Workbook Workbook
		{
			get { return this.workbook; }
		}

		public Worksheet Worksheet
		{
			get { return this.worksheet; }
		}

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