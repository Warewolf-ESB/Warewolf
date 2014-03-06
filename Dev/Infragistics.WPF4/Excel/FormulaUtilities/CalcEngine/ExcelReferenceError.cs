using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Infragistics.Documents.Excel.CalcEngine;

namespace Infragistics.Documents.Excel.FormulaUtilities.CalcEngine
{
	internal sealed class ExcelReferenceError : ExcelRefBase
	{
		#region Static Members

		public static readonly ExcelReferenceError Instance = new ExcelReferenceError(); 

		#endregion Static Members

		#region Constructor

		private ExcelReferenceError()
		{
			base.ValueInternal = new ExcelCalcValue( new ExcelCalcErrorValue( ExcelCalcErrorCode.Reference ) );
		} 

		#endregion Constructor

		#region Base Class Overrides

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

		public override bool ContainsReference( IExcelCalcReference inReference )
		{
			return false;
		} 

		#endregion ContainsReference

		#region ElementName

		public override string ElementName
		{
			get { return FormulaParser.ReferenceErrorValue; }
		}

		#endregion ElementName

		#region Equals

		public override bool Equals( object obj )
		{
			return obj is ExcelReferenceError;
		}

		#endregion Equals

		#region GetHashCode

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		#endregion GetHashCode

		#region IsSubsetReference

		public override bool IsSubsetReference( IExcelCalcReference inReference )
		{
			Utilities.DebugFail( "This seems to only be called on formula owners and an instance of ExcelReferenceError cannot own a formula." );
			return false;
		} 

		#endregion IsSubsetReference

		// MD 4/12/11 - TFS67084
		#region Row

		public override WorksheetRow Row
		{
			get { return null; }
		}

		#endregion  // Row

		#region ValueInternal

		protected override ExcelCalcValue ValueInternal 
		{
			set { Utilities.DebugFail( "We should never be solving a formula for the reference error." ); }
		}

		#endregion ValueInternal 

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