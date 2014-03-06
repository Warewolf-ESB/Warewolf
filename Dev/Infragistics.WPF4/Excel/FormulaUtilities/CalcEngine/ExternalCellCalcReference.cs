using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Infragistics.Documents.Excel.CalcEngine;
using Infragistics.Documents.Excel.Serialization;

namespace Infragistics.Documents.Excel.FormulaUtilities.CalcEngine
{
	// MD 6/18/12 - TFS102878
	// Refactored these to clean them up a bit and account for multi-sheet references.
	#region Old Code

	// MD 3/30/11 - TFS69969
	//internal sealed class ExternalCellCalcReference : ExcelRefBase
	//{
	//    #region Member Variables

	//    private CellAddress address;

	//    // MD 4/12/11 - TFS67084
	//    // Use short instead of int so we don't have to cast.
	//    //private int columnIndex;
	//    private short columnIndex;

	//    private string elementName;
	//    private int rowIndex;
	//    private ExcelCalcValue value;
	//    private WorksheetReferenceExternal worksheetReference; 

	//    #endregion // Member Variables

	//    #region Constructor

	//    // MD 4/12/11 - TFS67084
	//    // Use short instead of int so we don't have to cast.
	//    //public ExternalCellCalcReference(WorksheetReference worksheetReference, int rowIndex, int columnIndex)
	//    public ExternalCellCalcReference(WorksheetReferenceExternal worksheetReference, int rowIndex, short columnIndex)
	//    {
	//        this.worksheetReference = worksheetReference;
	//        this.rowIndex = rowIndex;
	//        this.columnIndex = columnIndex;

	//        this.value = new ExcelCalcValue(this.worksheetReference.GetCachedValue(this.rowIndex, this.columnIndex));
	//        this.address = new CellAddress(this.rowIndex, false, this.columnIndex, false);
	//        this.elementName =
	//            this.worksheetReference.ReferenceName +
	//            "!" +
	//            // MD 4/12/11 - TFS67084
	//            // Moved away from using WorksheetCell objects.
	//            //address.ToString(null, CellReferenceMode.A1, WorkbookFormat.Excel2007);
	//            // MD 2/20/12 - 12.1 - Table Support
	//            //address.ToString(null, -1, CellReferenceMode.A1, WorkbookFormat.Excel2007);
	//            address.ToString(-1, -1, CellReferenceMode.A1, WorkbookFormat.Excel2007);
	//    }

	//    #endregion // Constructor

	//    #region Base Class Overrides

	//    // MD 4/12/11 - TFS67084
	//    // Moved away from using WorksheetCell objects.
	//    //public override WorksheetCell Cell
	//    //{
	//    //    get { return null; }
	//    //}

	//    // MD 4/12/11 - TFS67084
	//    public override short ColumnIndex
	//    {
	//        get { return this.columnIndex; }
	//    }

	//    public override bool ContainsReference(IExcelCalcReference inReference)
	//    {
	//        IExcelCalcReference reference = ExcelCalcEngine.GetResolvedReference(inReference);

	//        ExternalCellCalcReference externalCellReference = reference as ExternalCellCalcReference;
	//        if (externalCellReference != null)
	//        {
	//            if (this.worksheetReference != externalCellReference.WorksheetReference)
	//                return false;

	//            if (this.rowIndex != externalCellReference.rowIndex)
	//                return false;

	//            if (this.columnIndex != externalCellReference.columnIndex)
	//                return false;

	//            return true;
	//        }

	//        ExternalRegionCalcReference externalRegionReference = reference as ExternalRegionCalcReference;
	//        if (externalRegionReference != null)
	//        {
	//            if (this.worksheetReference != externalRegionReference.WorksheetReference)
	//                return false;

	//            if (externalRegionReference.LastColumnIndex < this.columnIndex || this.columnIndex < externalRegionReference.FirstColumnIndex)
	//                return false;

	//            if (externalRegionReference.LastRowIndex < this.rowIndex || this.rowIndex < externalRegionReference.FirstRowIndex)
	//                return false;

	//            return true;
	//        }

	//        return false;
	//    }

	//    public override string ElementName
	//    {
	//        get { return this.elementName; }
	//    }

	//    public override bool Equals(object obj)
	//    {
	//        IExcelCalcReference reference = obj as IExcelCalcReference;

	//        if (reference == null)
	//            return false;

	//        ExternalCellCalcReference externalCellReference = ExcelCalcEngine.GetResolvedReference(reference) as ExternalCellCalcReference;

	//        if (externalCellReference == null)
	//            return false;

	//        return this == externalCellReference;
	//    }

	//    public override int GetHashCode()
	//    {
	//        return this.worksheetReference.GetHashCode() ^
	//            this.columnIndex ^
	//            this.rowIndex;
	//    }

	//    public override bool IsSubsetReference(IExcelCalcReference inReference)
	//    {
	//        return false;
	//    }

	//    // MD 4/12/11 - TFS67084
	//    public override WorksheetRow Row
	//    {
	//        get { return null; }
	//    }

	//    public override ExcelCalcValue Value
	//    {
	//        get { return this.value; }
	//        set
	//        {
	//            Utilities.DebugFail("Cannot set the value of this reference.");
	//        }
	//    }

	//    public override Workbook Workbook
	//    {
	//        get { return this.worksheetReference.WorkbookReference.TargetWorkbook; }
	//    }

	//    #endregion // Base Class Overrides

	//    #region Properties

	//    // MD 4/12/11 - TFS67084
	//    // This is now defined on the base class
	//    //public int ColumnIndex
	//    //{
	//    //    get { return this.columnIndex; }
	//    //}

	//    public int RowIndex
	//    {
	//        get { return this.rowIndex; }
	//    }

	//    public WorksheetReference WorksheetReference
	//    {
	//        get { return this.worksheetReference; }
	//    } 

	//    #endregion // Properties
	//}

	#endregion // Old Code
	internal sealed class ExternalCellCalcReference : ExcelRefBase
	{
		#region Member Variables

		private WorksheetCellAddress address;
		private ExcelCalcValue value;
		private WorksheetReferenceExternal worksheetReference;

		#endregion // Member Variables

		#region Constructor

		public ExternalCellCalcReference(WorksheetReferenceExternal worksheetReference, int rowIndex, short columnIndex)
		{
			this.worksheetReference = worksheetReference;
			this.address = new WorksheetCellAddress(rowIndex, columnIndex);
			this.value = new ExcelCalcValue(this.worksheetReference.GetCachedValue(this.address.RowIndex, this.address.ColumnIndex));
		}

		#endregion // Constructor

		#region Base Class Overrides

		public override short ColumnIndex
		{
			get { return this.address.ColumnIndex; }
		}

		public override bool ContainsReference(IExcelCalcReference inReference)
		{
			IExcelCalcReference reference = ExcelCalcEngine.GetResolvedReference(inReference);

			ExternalCellCalcReference externalCellReference = reference as ExternalCellCalcReference;
			if (externalCellReference != null)
			{
				return
					this.worksheetReference == externalCellReference.worksheetReference &&
					this.address == externalCellReference.address;
			}

			ExternalRegionCalcReference externalRegionReference = reference as ExternalRegionCalcReference;
			if (externalRegionReference != null)
			{
				return
					this.worksheetReference == externalRegionReference.WorksheetReference &&
					externalRegionReference.Address.Contains(this.address);
			}

			MultiSheetExternalCellCalcReference multiSheetExternalCellReference = reference as MultiSheetExternalCellCalcReference;
			if (multiSheetExternalCellReference != null)
			{
				return
					this.worksheetReference.WorkbookReference == multiSheetExternalCellReference.FirstWorksheetReference.WorkbookReference &&
					multiSheetExternalCellReference.FirstWorksheetReference.FirstWorksheetIndex <= this.worksheetReference.FirstWorksheetIndex &&
					this.worksheetReference.FirstWorksheetIndex <= multiSheetExternalCellReference.LastWorksheetReference.FirstWorksheetIndex &&
					this.address == multiSheetExternalCellReference.Address;
			}

			MultiSheetExternalRegionCalcReference multiSheetExternalRegionReference = reference as MultiSheetExternalRegionCalcReference;
			if (multiSheetExternalRegionReference != null)
			{
				return
					this.worksheetReference.WorkbookReference == multiSheetExternalRegionReference.FirstWorksheetReference.WorkbookReference &&
					multiSheetExternalRegionReference.FirstWorksheetReference.FirstWorksheetIndex <= this.worksheetReference.FirstWorksheetIndex &&
					this.worksheetReference.FirstWorksheetIndex <= multiSheetExternalRegionReference.LastWorksheetReference.FirstWorksheetIndex &&
					multiSheetExternalRegionReference.Address.Contains(this.address);
			}

			return false;
		}

		public override string ElementName
		{
			get
			{
				return
					this.worksheetReference.ReferenceName +
					this.address.ToString(false, false, this.worksheetReference.WorkbookReference.TargetWorkbook.CurrentFormat, CellReferenceMode.A1);
			}
		}

		public override bool Equals(object obj)
		{
			IExcelCalcReference reference = obj as IExcelCalcReference;
			if (reference == null)
				return false;

			ExternalCellCalcReference externalCellReference = ExcelCalcEngine.GetResolvedReference(reference) as ExternalCellCalcReference;
			if (externalCellReference == null)
				return false;

			return 
				this.worksheetReference == externalCellReference.worksheetReference &&
				this.address == externalCellReference.address;
		}

		public override int GetHashCode()
		{
			return 
				this.worksheetReference.GetHashCode() ^
				this.address.GetHashCode() << 1;
		}

		public override bool IsSubsetReference(IExcelCalcReference inReference)
		{
			Utilities.DebugFail("This seems to only be called on formula owners and an instance of ExternalCellCalcReference cannot own a formula.");
			return false;
		}

		public override WorksheetRow Row
		{
			get { return null; }
		}

		public override ExcelCalcValue Value
		{
			get { return this.value; }
			set
			{
				Utilities.DebugFail("Cannot set the value of this reference.");
			}
		}

		public override Workbook Workbook
		{
			get { return this.worksheetReference.WorkbookReference.TargetWorkbook; }
		}

		#endregion // Base Class Overrides

		#region Properties

		public WorksheetCellAddress Address
		{
			get { return this.address; }
		}

		public WorksheetReference WorksheetReference
		{
			get { return this.worksheetReference; }
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