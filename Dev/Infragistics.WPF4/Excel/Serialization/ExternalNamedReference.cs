using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Infragistics.Documents.Excel.CalcEngine;
using Infragistics.Documents.Excel.FormulaUtilities.CalcEngine;
using System.Globalization;

namespace Infragistics.Documents.Excel.Serialization
{





	internal class ExternalNamedReference : NamedReferenceBase
	{
		#region Member Variables

		// MD 3/30/11 - TFS69969
		private ExternalNamedCalcReference calcReference;

		private ExternalWorkbookReference workbook;

		#endregion Member Variables

		#region Constructor

		public ExternalNamedReference( ExternalWorkbookReference workbook, object scope )
			: base( scope, false )
		{
			this.workbook = workbook;
		}

		#endregion Constructor

		#region Base Class Overrides

		// MD 3/30/11 - TFS69969
		#region CalcReference

		internal override IExcelCalcReference CalcReference
		{
			get { return this.CalcReferenceInternal; }
		}

		private ExcelRefBase CalcReferenceInternal
		{
			get
			{
				if (this.calcReference == null)
				{
					this.calcReference = new ExternalNamedCalcReference(this);
					this.workbook.TargetWorkbook.AddReference(this.calcReference);
				}

				return this.calcReference;
			}
		}

		#endregion // CalcReference

		// MD 4/6/12 - TFS101506
		#region Culture

		internal override CultureInfo Culture
		{
			get { return this.workbook.TargetWorkbook.CultureResolved; }
		}

		#endregion // Culture

		// MD 7/9/08 - Excel 2007 Format
		#region CurrentFormat

		internal override WorkbookFormat CurrentFormat
		{
			// MD 3/30/11 - TFS69969
			// The workbook reference no longer keeps a reference to the serialization manager, because it is stored after loading,
			// so instead go to the workbook that target contains the target of the reference.
			//get { return this.workbook.Manager.Workbook.CurrentFormat; }
			get { return this.workbook.TargetWorkbook.CurrentFormat; }
		} 

		#endregion CurrentFormat

		// MD 3/30/11 - TFS69969
		#region OnFormulaChanged

		internal override void OnFormulaChanged()
		{
			if (this.FormulaInternal == null || this.FormulaInternal.PostfixTokenList.Count == 0)
				return;

			this.CalcReferenceInternal.SetAndCompileFormula(this.FormulaInternal, false);
		}

		#endregion OnFormulaChanged

		#region ToString

		public override string ToString()
		{
			return this.ToString(null);
		}

		internal override string ToString(Dictionary<WorkbookReferenceBase, int> externalReferences)
		{
			if (this.Scope is WorkbookReferenceBase)
				return this.workbook.GetWorkbookReferenceString(externalReferences) + this.Name;

			WorksheetReference worksheet = this.Scope as WorksheetReference;

			if (worksheet != null)
				return worksheet.GetReferenceName(externalReferences) + this.Name;

			Utilities.DebugFail("Unknown scope.");
			return this.Name;
		}

        #endregion ToString

		// MD 2/22/12 - 12.1 - Table Support
		#region Workbook

		internal override Workbook Workbook
		{
			get { return null; }
		}

		#endregion // Workbook
        
		// MD 10/8/07 - BR27172
		#region WorkbookReference






		internal override WorkbookReferenceBase WorkbookReference
		{
			get { return this.workbook; }
		}

		#endregion WorkbookReference

		#endregion Base Class Overrides

		#region Properties

		// MD 10/8/07 - BR27172
		// This has been replaced by the virtual WorkbookReference property on the base class.
		//#region Workbook
		//
		//public ExternalWorkbookReference Workbook
		//{
		//    get { return this.workbook; }
		//}
		//
		//#endregion Workbook

		#endregion Properties
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