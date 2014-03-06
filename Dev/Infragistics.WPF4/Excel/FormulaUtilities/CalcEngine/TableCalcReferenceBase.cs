using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Infragistics.Documents.Excel.CalcEngine;
using Infragistics.Documents.Excel.FormulaUtilities.Tokens;

namespace Infragistics.Documents.Excel.FormulaUtilities.CalcEngine
{
	// MD 2/24/12 - 12.1 - Table Support
	internal abstract class TableCalcReferenceBase : RegionCalcReferenceBase
	{
		#region Member Variables

		private StructuredTableReferenceKeywordType? _firstAreaKeyword;
		private StructuredTableReferenceKeywordType? _lastAreaKeyword;
		private WorksheetRow _rowOfFormulaOwner;

		#endregion Member Variables

		#region Constructor

		public TableCalcReferenceBase(
			WorksheetRow rowOfFormulaOwner,
			StructuredTableReferenceKeywordType? firstAreaKeyword, 
			StructuredTableReferenceKeywordType? lastAreaKeyword)
		{
			Debug.Assert(firstAreaKeyword != StructuredTableReferenceKeywordType.ThisRow || lastAreaKeyword.HasValue == false,
				"This reference should never have been created with StructuredTableReferenceKeywordType.ThisRow and another keyword.");
			Debug.Assert(lastAreaKeyword != StructuredTableReferenceKeywordType.ThisRow,
				"This reference should never have been created with a last keyword of StructuredTableReferenceKeywordType.ThisRow.");
			Debug.Assert(firstAreaKeyword.HasValue || lastAreaKeyword.HasValue == false,
				"In the lastAreaKeyword has a value, so should the firstAreaKeyword.");

			_firstAreaKeyword = firstAreaKeyword;
			_lastAreaKeyword = lastAreaKeyword;
			_rowOfFormulaOwner = rowOfFormulaOwner;
			this.ValueInternal = new ExcelCalcValue(new TableReferenceArrayProxy(this));
		}

		#endregion Constructor

		#region Base Class Overrides

		#region ResolveReference

		// MD 4/10/12 - TFS108678
		//public override ExcelRefBase ResolveReference(IExcelCalcReference formulaOwner, TokenClass expectedTokenClass, out ExcelCalcErrorValue errorValue)
		public override ExcelRefBase ResolveReference(IExcelCalcReference formulaOwner, TokenClass expectedTokenClass, bool splitArraysForValueParameters, out ExcelCalcErrorValue errorValue)
		{
			if (formulaOwner != null && _firstAreaKeyword == StructuredTableReferenceKeywordType.ThisRow)
			{
				CellCalcReference cellReference = formulaOwner as CellCalcReference;
				if (cellReference != null && cellReference.Row != _rowOfFormulaOwner)
				{
					TableCalcReferenceBase clonedReference = this.Clone(cellReference.Row);

					// MD 4/10/12 - TFS108678
					//clonedReference.ResolveReference(formulaOwner, expectedTokenClass, out errorValue);
					clonedReference.ResolveReference(formulaOwner, expectedTokenClass, splitArraysForValueParameters, out errorValue);

					return clonedReference;
				}
			}

			// MD 4/10/12 - TFS108678
			//return base.ResolveReference(formulaOwner, expectedTokenClass, out errorValue);
			return base.ResolveReference(formulaOwner, expectedTokenClass, splitArraysForValueParameters, out errorValue);
		}

		#endregion // ResolveReference

		#region Value

		public sealed override ExcelCalcValue Value
		{
			get { return base.Value; }
			set
			{
				Debug.Assert(value == null, "The value should never be evaluated for a TableColumnCalcReference.");
			}
		}

		#endregion Value

		#region Workbook

		public sealed override Workbook Workbook
		{
			get 
			{
				WorksheetTable table = this.Table;
				if (table == null)
					return null;

				return table.Workbook; 
			}
		}

		#endregion Workbook

		#endregion Base Class Overrides

		#region Methods

		#region Clone

		protected abstract TableCalcReferenceBase Clone(WorksheetRow formulaOwnerRow);

		#endregion // Clone

		#region GetTableRegion

		private WorksheetRegion GetTableRegion(WorksheetTable table, StructuredTableReferenceKeywordType keywordType)
		{
			switch (keywordType)
			{
				case StructuredTableReferenceKeywordType.All:
					return table.WholeTableRegion;

				case StructuredTableReferenceKeywordType.Data:
					return table.DataAreaRegion;

				case StructuredTableReferenceKeywordType.Headers:
					return table.HeaderRowRegion;

				case StructuredTableReferenceKeywordType.Totals:
					return table.TotalsRowRegion;

				case StructuredTableReferenceKeywordType.ThisRow:
					{
						if (_rowOfFormulaOwner == null)
							return null;

						WorksheetRegion dataRegion = table.DataAreaRegion;
						if (dataRegion == null)
							return null;

						if (dataRegion.Worksheet == null)
						{
							Utilities.DebugFail("This is unexpected.");
							return null;
						}

						if (_rowOfFormulaOwner.Index < dataRegion.FirstRow || dataRegion.LastRow < _rowOfFormulaOwner.Index)
							return null;

						return dataRegion.Worksheet.GetCachedRegion(
							_rowOfFormulaOwner.Index, dataRegion.FirstColumn,
							_rowOfFormulaOwner.Index, dataRegion.LastColumn);
					}

				default:
					Utilities.DebugFail("Unknown StructuredTableReferenceKeywordType: " + keywordType);
					goto case StructuredTableReferenceKeywordType.Data;
			}
		}

		#endregion // GetTableRegion

		#region EqualsHelper

		protected bool EqualsHelper(TableCalcReferenceBase other)
		{
			if (_firstAreaKeyword != other._firstAreaKeyword || _lastAreaKeyword != other._lastAreaKeyword)
				return false;

			if (_firstAreaKeyword == StructuredTableReferenceKeywordType.ThisRow)
			{
				if (_rowOfFormulaOwner != other._rowOfFormulaOwner)
					return false;
			}

			return true;
		}

		#endregion // EqualsHelper

		#endregion // Methods

		#region Properties

		#region FirstAreaKeyword

		protected StructuredTableReferenceKeywordType? FirstAreaKeyword
		{
			get { return _firstAreaKeyword; }
		}

		#endregion // FirstAreaKeyword

		#region LastAreaKeyword

		protected StructuredTableReferenceKeywordType? LastAreaKeyword
		{
			get { return _lastAreaKeyword; }
		}

		#endregion // LastAreaKeyword

		#region Table

		public abstract WorksheetTable Table { get; }

		#endregion // Table

		#region TableRegion

		protected WorksheetRegion TableRegion
		{
			get
			{
				WorksheetTable table = this.Table;
				if (table == null)
					return null;

				if (this.FirstAreaKeyword.HasValue == false)
					return table.DataAreaRegion;

				WorksheetRegion firstRegion = this.GetTableRegion(table, this.FirstAreaKeyword.Value);
				if (this.LastAreaKeyword.HasValue == false)
					return firstRegion;

				WorksheetRegion lastRegion = this.GetTableRegion(table, this.LastAreaKeyword.Value);
				return WorksheetRegion.Union(firstRegion, lastRegion);
			}
		}

		#endregion // TableRegion

		#endregion // Properties


		#region TableReferenceArrayProxy class

		internal class TableReferenceArrayProxy : RegionArrayProxyBase
		{
			private TableCalcReferenceBase associatedReference;

			public TableReferenceArrayProxy(TableCalcReferenceBase associatedReference)
			{
				this.associatedReference = associatedReference;
			}

			protected override WorksheetRegion Region
			{
				get { return this.associatedReference.Region; }
			}
		}

		#endregion // TableReferenceArrayProxy class
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