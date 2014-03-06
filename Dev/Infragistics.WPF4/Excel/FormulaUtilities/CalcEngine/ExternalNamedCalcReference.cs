using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Infragistics.Documents.Excel.CalcEngine;
using Infragistics.Documents.Excel.Serialization;

namespace Infragistics.Documents.Excel.FormulaUtilities.CalcEngine
{
	// MD 3/30/11 - TFS69969
	internal sealed class ExternalNamedCalcReference : NamedCalcReferenceBase
	{
		#region Member Variables

		private ExternalNamedReference namedReference;

		#endregion Member Variables

		#region Constructor

		public ExternalNamedCalcReference(ExternalNamedReference namedReference)
		{
			this.namedReference = namedReference;
		}

		#endregion Constructor

		#region Base Class Overrides

		#region CanOwnFormula

		public override bool CanOwnFormula
		{
			get { return true; }
		}

		#endregion CanOwnFormula

		// MD 4/12/11 - TFS67084
		// Moved away from using WorksheetCell objects.
		//#region Cell

		//public override WorksheetCell Cell
		//{
		//    get { return null; }
		//}

		//#endregion Cell

		// MD 4/12/11 - TFS67084
		#region ColumnIndex

		public override short ColumnIndex
		{
			get { return -1; }
		}

		#endregion  // ColumnIndex

		#region ContainsReference

		public override bool ContainsReference(IExcelCalcReference inReference)
		{
			ExternalNamedCalcReference namedCalcReference = ExcelCalcEngine.GetResolvedReference(inReference) as ExternalNamedCalcReference;

			if (namedCalcReference == null)
				return false;

			return namedCalcReference.namedReference == this.namedReference;
		}

		#endregion ContainsReference

		#region Context

		public override object Context
		{
			get { return this.namedReference; }
		}

		#endregion Context

		#region ElementName

		public override string ElementName
		{
			get { return this.namedReference.ToString(); }
		}

		#endregion ElementName

		#region Equals

		public override bool Equals(object obj)
		{
			IExcelCalcReference reference = obj as IExcelCalcReference;

			if (reference == null)
				return false;

			ExternalNamedCalcReference externalNamedCalcReference = ExcelCalcEngine.GetResolvedReference(reference) as ExternalNamedCalcReference;

			if (externalNamedCalcReference == null)
				return false;

			return this.namedReference == externalNamedCalcReference.namedReference;
		}

		#endregion Equals

		#region GetHashCode

		public override int GetHashCode()
		{
			return this.namedReference.GetHashCode();
		}

		#endregion GetHashCode

		#region IsSubsetReference

		public override bool IsSubsetReference(IExcelCalcReference inReference)
		{
			ExternalNamedCalcReference externalNamedCalcReference = ExcelCalcEngine.GetResolvedReference(inReference) as ExternalNamedCalcReference;

			if (externalNamedCalcReference == null)
				return false;

			return externalNamedCalcReference.namedReference == this.namedReference;
		}

		#endregion IsSubsetReference

		// MD 4/12/11 - TFS67084
		#region Row

		public override WorksheetRow Row
		{
			get { return null; }
		}

		#endregion  // Row

		#region Value

		public override ExcelCalcValue Value
		{
			get
			{
				ExcelCalcValue value = this.ValueInternal;

				if (value == null)
				{
					value = new ExcelCalcValue(this.Formula);
					this.ValueInternal = value;
				}

				return value;
			}
			set
			{
				Debug.Assert(value == null, "The value should never be evaluated for a named reference.");
			}
		}

		#endregion Value

		#region Workbook

		public override Workbook Workbook
		{
			get { return this.namedReference.WorkbookReference.TargetWorkbook; }
		}

		#endregion Workbook

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