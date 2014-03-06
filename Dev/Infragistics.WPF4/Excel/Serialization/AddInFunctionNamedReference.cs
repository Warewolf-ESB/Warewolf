using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Infragistics.Documents.Excel.FormulaUtilities.CalcEngine;
using System.Globalization;
using Infragistics.Documents.Excel.CalcEngine;

namespace Infragistics.Documents.Excel.Serialization
{
	internal class AddInFunctionNamedReference : NamedReferenceBase
	{
		#region Member Variables

		private AddInFunctionsWorkbookReference workbook;

		#endregion Member Variables

		#region Constructor

		internal AddInFunctionNamedReference( AddInFunctionsWorkbookReference workbook, object scope )
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
			get
			{
				Utilities.DebugFail("We shouldn't be getting the CalcReference of an AddInFunctionNamedReference");
				return null;
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

		#region ToString

		public override string ToString()
		{
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

		#region WorkbookReference






		internal override WorkbookReferenceBase WorkbookReference
		{
			get { return this.workbook; }
		}

		#endregion WorkbookReference

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