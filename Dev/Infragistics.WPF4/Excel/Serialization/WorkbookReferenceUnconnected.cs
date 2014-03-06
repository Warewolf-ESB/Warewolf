using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Collections.ObjectModel;
using Infragistics.Documents.Excel.FormulaUtilities;
using Infragistics.Documents.Excel.Serialization.BIFF8.BiffRecords;
using System.Globalization;

namespace Infragistics.Documents.Excel.Serialization
{
	// MD 6/16/12 - CalcEngineRefactor
	internal class WorkbookReferenceUnconnected : WorkbookReferenceBase
	{
		#region Member Variables

		private string workbookFileName;

		#endregion // Member Variables

		#region Constructor

		public WorkbookReferenceUnconnected(string workbookFileName)
			: base(null)
		{
			this.workbookFileName = workbookFileName;
		}

		#endregion // Constructor

		#region Base Class Overrides

		#region Connect

		internal override WorkbookReferenceBase Connect(FormulaContext context)
		{
			if (context.Workbook == null)
				return this;

			return context.Workbook.GetWorkbookReference(this.workbookFileName);
		}

		#endregion // Connect

		#region CreateNamedReference

		public override NamedReferenceBase CreateNamedReference(string name, object scope)
		{
			Utilities.DebugFail("This should not be called.");
			return null;
		}

		#endregion // CreateNamedReference

		#region CreateWorksheetReference

		public override WorksheetReferenceSingle CreateWorksheetReference(int worksheetIndex)
		{
			if (worksheetIndex == EXTERNSHEETRecord.SheetCannotBeFoundIndex)
				return new WorksheetReferenceError(this);

			if (worksheetIndex == EXTERNSHEETRecord.WorkbookLevelReferenceIndex)
				return new WorksheetReferenceToWorkbook(this);

			Utilities.DebugFail("This should not be called.");
			return null;
		}

		#endregion // CreateWorksheetReference

		#region Disconnect

		internal override WorkbookReferenceBase Disconnect()
		{
			return this;
		}

		#endregion // Disconnect

		#region FileName

		public override string FileName
		{
			get { return this.workbookFileName; }
		}

		#endregion // FileName

		#region GetWorksheetName

		public override string GetWorksheetName(int worksheetIndex)
		{
			if (worksheetIndex == EXTERNSHEETRecord.SheetCannotBeFoundIndex)
				return FormulaParser.ReferenceErrorValue;

			if (worksheetIndex == EXTERNSHEETRecord.WorkbookLevelReferenceIndex)
				return null;

			Utilities.DebugFail("This should not be called.");
			return worksheetIndex.ToString();
		}

		#endregion // GetWorksheetName

		#region GetWorksheetReference

		public override WorksheetReferenceSingle GetWorksheetReference(string worksheetName)
		{
			if (worksheetName == null)
				return this.GetWorksheetReference(EXTERNSHEETRecord.WorkbookLevelReferenceIndex);

			if (String.Compare(worksheetName, FormulaParser.ReferenceErrorValue, CultureInfo.CurrentCulture, CompareOptions.IgnoreCase) == 0)
				return this.GetWorksheetReference(EXTERNSHEETRecord.SheetCannotBeFoundIndex);

			return new WorksheetReferenceSingleUnconnected(this, worksheetName);
		}

		#endregion // GetWorksheetReference

		#region GetWorksheetReferenceString

		public override string GetWorksheetReferenceString(int firstWorksheetIndex, int lastWorksheetIndex, Dictionary<WorkbookReferenceBase, int> externalReferences)
		{
			if (firstWorksheetIndex == lastWorksheetIndex)
				return Utilities.CreateReferenceString(this.FileName, this.GetWorksheetName(firstWorksheetIndex));

			return Utilities.CreateReferenceString(this.FileName, this.GetWorksheetName(firstWorksheetIndex), this.GetWorksheetName(lastWorksheetIndex));
		}

		#endregion // GetWorksheetReferenceString

		#region IsConnected

		public override bool IsConnected
		{
			get { return false; }
		}

		#endregion // IsConnected

		#region IsExternal

		public override bool IsExternal
		{
			get { return this.workbookFileName != null; }
		}

		#endregion // IsExternal

		#region NamedReferenceFormulaType

		protected override FormulaType NamedReferenceFormulaType
		{
			get
			{
				if (this.IsExternal)
					return FormulaType.ExternalNamedReferenceFormula;

				return FormulaType.NamedReferenceFormula;
			}
		}

		#endregion // NamedReferenceFormulaType

		#endregion // Base Class Overrides
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