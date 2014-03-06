using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Infragistics.Documents.Excel.Serialization.BIFF8.BiffRecords;
using Infragistics.Documents.Excel.FormulaUtilities;

namespace Infragistics.Documents.Excel.Serialization
{
	internal class AddInFunctionsWorkbookReference : WorkbookReferenceBase
	{
		public const string AddInFunctionsWorkbookName = "<AddInFunctions>";

		#region Constructor

		public AddInFunctionsWorkbookReference(Workbook targetWorkbook)
			: base(targetWorkbook) { }

		#endregion Constructor

		#region Base Class Overrides

		#region CreateNamedReference

		public override NamedReferenceBase CreateNamedReference( string name, object scope )
		{
			AddInFunctionNamedReference namedReference = new AddInFunctionNamedReference( this, scope );
			
			// MD 3/22/11 - TFS67606
			// When creating a named reference internally, don't validate the name. We are either loading or resolving named 
			// references in formula tokens which have already had the names validated.
			//namedReference.Name = name;
			namedReference.SetNameInternal(name, false);

			return namedReference;
		}

		#endregion CreateNamedReference

		#region CreateWorksheetReference

		public override WorksheetReferenceSingle CreateWorksheetReference(int worksheetIndex)
		{
			Debug.Assert(worksheetIndex == EXTERNSHEETRecord.WorkbookLevelReferenceIndex, "The index is not valid for the AddInFunctionsWorkbookReference.");
			return new WorksheetReferenceToWorkbook(this);
		}

		#endregion // CreateWorksheetReference

		#region FileName

		public override string FileName
		{
			get { return AddInFunctionsWorkbookReference.AddInFunctionsWorkbookName; }
		}

		#endregion FileName

		#region GetWorksheetName

		public override string GetWorksheetName(int worksheetIndex)
		{
			if (worksheetIndex == EXTERNSHEETRecord.SheetCannotBeFoundIndex)
				return FormulaParser.ReferenceErrorValue;

			if (worksheetIndex == EXTERNSHEETRecord.WorkbookLevelReferenceIndex)
				return null;

			return worksheetIndex.ToString();
		}

		#endregion GetWorksheetName

		#region GetWorksheetReference

		public override WorksheetReferenceSingle GetWorksheetReference(string worksheetName)
		{
			Utilities.DebugFail("This should never be called.");
			return null;
		}

		#endregion GetWorksheetReference

		#region GetWorksheetReferenceString

		public override string GetWorksheetReferenceString(int firstWorksheetIndex, int lastWorksheetIndex, Dictionary<WorkbookReferenceBase, int> externalReferences)
		{
			return null;
		}

        #endregion GetWorksheetReferenceString

		// MD 6/13/12 - CalcEngineRefactor
		#region IsExternal

		public override bool IsExternal
		{
			get { return false; }
		}

		#endregion // IsExternal

        #region NamedReferenceFormulaType

        protected override FormulaType NamedReferenceFormulaType
		{
			get { return FormulaType.Formula; }
		}

		#endregion NamedReferenceFormulaType

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