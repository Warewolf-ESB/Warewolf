using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Infragistics.Documents.Excel.CalcEngine;
using Infragistics.Documents.Excel.Serialization;


using Infragistics.Shared; 


namespace Infragistics.Documents.Excel.FormulaUtilities.CalcEngine
{
	// MD 6/18/12 - TFS102878
	// Refactored these to clean them up a bit and account for multi-sheet references.
	#region Old Code

	// MD 3/30/11 - TFS69969
	//internal sealed class ExternalRegionCalcReference : ExcelRefBase
	//{
	//    #region Member Variables

	//    private string elementName;

	//    // MD 4/12/11 - TFS67084
	//    // Use short instead of int so we don't have to cast.
	//    //private int firstColumnIndex;
	//    private short firstColumnIndex;

	//    private int firstRowIndex;

	//    // MD 4/12/11 - TFS67084
	//    // Use short instead of int so we don't have to cast.
	//    //private int lastColumnIndex;
	//    private short lastColumnIndex;

	//    private int lastRowIndex;
	//    private CellAddressRange range;
	//    private ExcelCalcValue value;
	//    private WorksheetReferenceExternal worksheetReference; 

	//    #endregion // Member Variables

	//    #region Constructor

	//    // MD 4/12/11 - TFS67084
	//    // Use short instead of int so we don't have to cast.
	//    //public ExternalRegionCalcReference(WorksheetReference worksheetReference, int firstRowIndex, int lastRowIndex, int firstColumnIndex, int lastColumnIndex)
	//    public ExternalRegionCalcReference(WorksheetReferenceExternal worksheetReference, int firstRowIndex, int lastRowIndex, short firstColumnIndex, short lastColumnIndex)
	//    {
	//        this.worksheetReference = worksheetReference;
	//        this.firstColumnIndex = firstColumnIndex;
	//        this.firstRowIndex = firstRowIndex;
	//        this.lastColumnIndex = lastColumnIndex;
	//        this.lastRowIndex = lastRowIndex;

	//        // MD 11/30/11 - TFS96468
	//        // Instead of creating the array now, which may take up a lot of memory and may not be needed, use a proxy class
	//        // which will lazily create the array.
	//        #region Old Code

	//        //ExcelCalcValue[,] valuesArray = new ExcelCalcValue[(lastColumnIndex - firstColumnIndex) + 1, (lastRowIndex - firstRowIndex) + 1];

	//        //// MD 4/12/11 - TFS67084
	//        //// Use short instead of int so we don't have to cast.
	//        ////for (int columnIndex = firstColumnIndex; columnIndex <= lastColumnIndex; columnIndex++)
	//        //for (short columnIndex = firstColumnIndex; columnIndex <= lastColumnIndex; columnIndex++)
	//        //{
	//        //    for (int rowIndex = firstRowIndex; rowIndex <= lastRowIndex; rowIndex++)
	//        //    {
	//        //        // MD 4/19/11 - TFS72977
	//        //        // The array indexes always start at 0 even if the region doesn't start at row 0 or column 0.
	//        //        //valuesArray[columnIndex, rowIndex] = new ExcelCalcValue(this.worksheetReference.GetCachedValue(rowIndex, columnIndex));
	//        //        // MD 11/23/11
	//        //        // Found while fixing TFS96468
	//        //        // We should have a singleton value to represent a null value. It wastes space to create a new calc value for each null value.
	//        //        //valuesArray[columnIndex - firstColumnIndex, rowIndex - firstRowIndex] = new ExcelCalcValue(this.worksheetReference.GetCachedValue(rowIndex, columnIndex));
	//        //        object cachedValue = this.worksheetReference.GetCachedValue(rowIndex, columnIndex);
	//        //        ExcelCalcValue calcValue;
	//        //        if (cachedValue == null)
	//        //            calcValue = ExcelCalcValue.Empty;
	//        //        else
	//        //            calcValue = new ExcelCalcValue(cachedValue);

	//        //        valuesArray[columnIndex - firstColumnIndex, rowIndex - firstRowIndex] = calcValue;
	//        //    }
	//        //}

	//        //this.value = new ExcelCalcValue(valuesArray);

	//        #endregion // Old Code
	//        this.value = new ExcelCalcValue(new ExternalRegionValuesArray(this));

	//        this.range = new CellAddressRange(
	//            new CellAddress(firstRowIndex, false, firstColumnIndex, false),
	//            new CellAddress(lastRowIndex, false, lastColumnIndex, false));
	//        this.elementName =
	//            this.worksheetReference.ReferenceName +
	//            "!" +
	//            // MD 4/12/11 - TFS67084
	//            // Moved away from using WorksheetCell objects.
	//            //this.range.ToString(null, false, CellReferenceMode.A1, WorkbookFormat.Excel2007);
	//            // MD 2/20/12 - 12.1 - Table Support
	//            //this.range.ToString(null, -1, false, CellReferenceMode.A1, WorkbookFormat.Excel2007);
	//            this.range.ToString(-1, -1, false, CellReferenceMode.A1, WorkbookFormat.Excel2007);
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
	//        get { return this.firstColumnIndex; }
	//    }

	//    public override bool ContainsReference(IExcelCalcReference inReference)
	//    {
	//        ExternalCellCalcReference externalCellReference = inReference as ExternalCellCalcReference;
	//        if (externalCellReference != null)
	//        {
	//            if (this.worksheetReference != externalCellReference.WorksheetReference)
	//                return false;

	//            if (externalCellReference.ColumnIndex < this.firstColumnIndex || this.lastColumnIndex < externalCellReference.ColumnIndex)
	//                return false;

	//            if (externalCellReference.RowIndex < this.firstRowIndex || this.lastRowIndex < externalCellReference.RowIndex)
	//                return false;

	//            return true;
	//        }

	//        ExternalRegionCalcReference externalRegionReference = inReference as ExternalRegionCalcReference;
	//        if (externalRegionReference != null)
	//        {
	//            if (this.worksheetReference != externalRegionReference.worksheetReference)
	//                return false;

	//            if (externalRegionReference.lastColumnIndex < this.firstColumnIndex || this.lastColumnIndex < externalRegionReference.firstColumnIndex)
	//                return false;

	//            if (externalRegionReference.lastRowIndex < this.firstRowIndex || this.lastRowIndex < externalRegionReference.firstRowIndex)
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

	//        ExternalRegionCalcReference externalRegionReference = ExcelCalcEngine.GetResolvedReference(reference) as ExternalRegionCalcReference;

	//        if (externalRegionReference == null)
	//            return false;

	//        return this == externalRegionReference;
	//    }

	//    public override int GetHashCode()
	//    {
	//        return this.worksheetReference.GetHashCode() ^
	//            this.firstColumnIndex ^
	//            this.lastColumnIndex ^
	//            this.firstRowIndex ^
	//            this.lastRowIndex;
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

	//    public int FirstColumnIndex
	//    {
	//        get { return this.firstColumnIndex; }
	//    }

	//    public int FirstRowIndex
	//    {
	//        get { return this.firstRowIndex; }
	//    }

	//    public int LastColumnIndex
	//    {
	//        get { return this.lastColumnIndex; }
	//    }

	//    public int LastRowIndex
	//    {
	//        get { return this.lastRowIndex; }
	//    }

	//    public WorksheetReferenceExternal WorksheetReference
	//    {
	//        get { return this.worksheetReference; }
	//    } 

	//    #endregion // Properties


	//    // MD 11/30/11 - TFS96468
	//    #region ExternalRegionValuesArray class

	//    // MD 12/1/11 - TFS96113
	//    // Derive from ArrayProxy so this class can be treated like all other arrays in the ExcelCalcValue
	//    //internal class ExternalRegionValuesArray
	//    internal class ExternalRegionValuesArray : ArrayProxy
	//    {
	//        #region Member Variable

	//        private ExternalRegionCalcReference associatedRegion;
	//        private ExcelCalcValue[,] cachedValuesArray;

	//        #endregion // Member Variable

	//        #region Constructor

	//        public ExternalRegionValuesArray(ExternalRegionCalcReference associatedRegion)
	//        {
	//            this.associatedRegion = associatedRegion;
	//        }

	//        #endregion // Constructor

	//        // MD 12/1/11 - TFS96113
	//        #region GetIteratorHelper

	//        internal override IEnumerable<KeyValuePair<int, ExcelCalcValue>> GetIteratorHelper(int dimension, int index)
	//        {
	//            short firstColumnIndex = (short)this.associatedRegion.FirstColumnIndex;
	//            int firstRowIndex = this.associatedRegion.FirstRowIndex;
	//            short lastColumnIndex = (short)this.associatedRegion.LastColumnIndex;
	//            int lastRowIndex = this.associatedRegion.LastRowIndex;
	//            WorksheetReferenceExternal worksheetReference = this.associatedRegion.WorksheetReference;

	//            switch (dimension)
	//            {
	//                case 0:
	//                    {
	//                        // MD 12/9/11 - TFS97567
	//                        // I confused the differences between the absolute and relative indexes here. I have fixed the errors and also 
	//                        // renamed the variables for clarity. 
	//                        // Also, I found a one-off error here because we should be iterating up to and including lastRowIndex.
	//                        //short columnIndex = (short)index;
	//                        //for (int i = firstRowIndex; i < lastRowIndex; i++)
	//                        //{
	//                        //    WorksheetReference.RowValues rowValues = worksheetReference.TryGetRowValues(i);
	//                        //    if (rowValues != null)
	//                        //        yield return new KeyValuePair<int, ExcelCalcValue>(i, new ExcelCalcValue(rowValues.GetCachedValue(columnIndex)));
	//                        //}
	//                        short columnIndexAbsolute = (short)(firstColumnIndex + index);
	//                        for (int rowIndexAbsolute = firstRowIndex; rowIndexAbsolute <= lastRowIndex; rowIndexAbsolute++)
	//                        {
	//                            WorksheetReferenceExternal.RowValues rowValues = worksheetReference.TryGetRowValues(rowIndexAbsolute);
	//                            if (rowValues != null)
	//                            {
	//                                yield return new KeyValuePair<int, ExcelCalcValue>(
	//                                    rowIndexAbsolute - firstRowIndex,
	//                                    new ExcelCalcValue(rowValues.GetCachedValue(columnIndexAbsolute)));
	//                            }
	//                        }
	//                    }
	//                    break;

	//                case 1:
	//                    {
	//                        // MD 12/9/11 - TFS97567
	//                        // I confused the differences between the absolute and relative indexes here. I have fixed the errors and also 
	//                        // renamed the variables for clarity. 
	//                        // Also, I found a one-off error here because we should be iterating up to and including lastColumnIndex.
	//                        //WorksheetReference.RowValues rowValues = worksheetReference.TryGetRowValues(index);
	//                        //if (rowValues != null)
	//                        //{
	//                        //    for (short i = firstColumnIndex; i < lastColumnIndex; i++)
	//                        //        yield return new KeyValuePair<int, ExcelCalcValue>(i, new ExcelCalcValue(rowValues.GetCachedValue(i)));
	//                        //}
	//                        int rowIndexAbsolute = firstRowIndex + index;
	//                        WorksheetReferenceExternal.RowValues rowValues = worksheetReference.TryGetRowValues(rowIndexAbsolute);
	//                        if (rowValues != null)
	//                        {
	//                            for (short columnIndexAbsolute = firstColumnIndex; columnIndexAbsolute <= lastColumnIndex; columnIndexAbsolute++)
	//                            {
	//                                yield return new KeyValuePair<int, ExcelCalcValue>(
	//                                    columnIndexAbsolute - firstColumnIndex,
	//                                    new ExcelCalcValue(rowValues.GetCachedValue(columnIndexAbsolute)));
	//                            }
	//                        }
	//                    }
	//                    break;

	//                default:
	//                    this.ThrowOutOfBoundsException();
	//                    break;
	//            }
	//        }

	//        #endregion // GetIteratorHelper

	//        // MD 12/1/11 - TFS96113
	//        #region GetLength

	//        public override int GetLength(int dimension)
	//        {
	//            switch (dimension)
	//            {
	//                case 0:
	//                    short firstColumnIndex = (short)this.associatedRegion.FirstColumnIndex;
	//                    short lastColumnIndex = (short)this.associatedRegion.LastColumnIndex;
	//                    return (lastColumnIndex - firstColumnIndex) + 1;

	//                case 1:
	//                    int firstRowIndex = this.associatedRegion.FirstRowIndex;
	//                    int lastRowIndex = this.associatedRegion.LastRowIndex;
	//                    return (lastRowIndex - firstRowIndex) + 1;

	//                default:
	//                    this.ThrowOutOfBoundsException();
	//                    return -1;
	//            }
	//        }

	//        #endregion // GetLength

	//        // MD 12/1/11 - TFS96113
	//        #region GetValue

	//        internal override ExcelCalcValue GetValue(int x, int y)
	//        {
	//            if (x < 0 || this.GetLength(0) <= x)
	//                this.ThrowOutOfBoundsException();

	//            if (y < 0 || this.GetLength(1) <= y)
	//                this.ThrowOutOfBoundsException();

	//            short columnIndex = (short)(this.associatedRegion.FirstColumnIndex + x);
	//            int rowIndex = this.associatedRegion.FirstRowIndex + y;
	//            return new ExcelCalcValue(this.associatedRegion.WorksheetReference.GetCachedValue(rowIndex, columnIndex));
	//        }

	//        #endregion // GetValue

	//        #region ToArray

	//        // MD 12/1/11 - TFS96113
	//        // This is defined on the base class, so we need to override it.
	//        //public ExcelCalcValue[,] ToArray()
	//        internal override ExcelCalcValue[,] ToArray()
	//        {
	//            if (this.cachedValuesArray == null)
	//            {
	//                short firstColumnIndex = (short)this.associatedRegion.FirstColumnIndex;
	//                int firstRowIndex = this.associatedRegion.FirstRowIndex;
	//                short lastColumnIndex = (short)this.associatedRegion.LastColumnIndex;
	//                int lastRowIndex = this.associatedRegion.LastRowIndex;
	//                WorksheetReferenceExternal worksheetReference = this.associatedRegion.WorksheetReference;

	//                this.cachedValuesArray = new ExcelCalcValue[(lastColumnIndex - firstColumnIndex) + 1, (lastRowIndex - firstRowIndex) + 1];

	//                for (short columnIndex = firstColumnIndex; columnIndex <= lastColumnIndex; columnIndex++)
	//                {
	//                    for (int rowIndex = firstRowIndex; rowIndex <= lastRowIndex; rowIndex++)
	//                    {
	//                        object cachedValue = worksheetReference.GetCachedValue(rowIndex, columnIndex);
	//                        ExcelCalcValue calcValue;
	//                        if (cachedValue == null)
	//                            calcValue = ExcelCalcValue.Empty;
	//                        else
	//                            calcValue = new ExcelCalcValue(cachedValue);

	//                        this.cachedValuesArray[columnIndex - firstColumnIndex, rowIndex - firstRowIndex] = calcValue;
	//                    }
	//                }
	//            }

	//            return this.cachedValuesArray;
	//        }

	//        #endregion // ToArray
	//    }

	//    #endregion // ExternalRegionValuesArray class
	//}

	#endregion // Old Code
	internal sealed class ExternalRegionCalcReference : ExcelRefBase
	{
		#region Member Variables

		private WorksheetRegionAddress address;
		private ExcelCalcValue value;
		private WorksheetReferenceExternal worksheetReference;

		#endregion // Member Variables

		#region Constructor

		public ExternalRegionCalcReference(WorksheetReferenceExternal worksheetReference, WorksheetRegionAddress address)
		{
			this.worksheetReference = worksheetReference;
			this.address = address;
			this.value = new ExcelCalcValue(new ExternalRegionValuesArray(this));
		}

		#endregion // Constructor

		#region Base Class Overrides

		public override short ColumnIndex
		{
			get { return this.address.FirstColumnIndex; }
		}

		public override bool ContainsReference(IExcelCalcReference inReference)
		{
			IExcelCalcReference reference = ExcelCalcEngine.GetResolvedReference(inReference);

			ExternalCellCalcReference externalCellReference = reference as ExternalCellCalcReference;
			if (externalCellReference != null)
			{
				return
					this.worksheetReference == externalCellReference.WorksheetReference &&
					this.address.Contains(externalCellReference.Address);
			}

			ExternalRegionCalcReference externalRegionReference = reference as ExternalRegionCalcReference;
			if (externalRegionReference != null)
			{
				return
					this.worksheetReference == externalRegionReference.worksheetReference &&
					this.address.IntersectsWith(externalRegionReference.address);
			}

			MultiSheetExternalCellCalcReference multiSheetExternalCellReference = reference as MultiSheetExternalCellCalcReference;
			if (multiSheetExternalCellReference != null)
			{
				return
					this.worksheetReference.WorkbookReference == multiSheetExternalCellReference.FirstWorksheetReference.WorkbookReference &&
					multiSheetExternalCellReference.FirstWorksheetReference.FirstWorksheetIndex <= this.worksheetReference.FirstWorksheetIndex &&
					this.worksheetReference.FirstWorksheetIndex <= multiSheetExternalCellReference.LastWorksheetReference.FirstWorksheetIndex &&
					this.address.Contains(multiSheetExternalCellReference.Address);
			}

			MultiSheetExternalRegionCalcReference multiSheetExternalRegionReference = reference as MultiSheetExternalRegionCalcReference;
			if (multiSheetExternalRegionReference != null)
			{
				return
					this.worksheetReference.WorkbookReference == multiSheetExternalRegionReference.FirstWorksheetReference.WorkbookReference &&
					multiSheetExternalRegionReference.FirstWorksheetReference.FirstWorksheetIndex <= this.worksheetReference.FirstWorksheetIndex &&
					this.worksheetReference.FirstWorksheetIndex <= multiSheetExternalRegionReference.LastWorksheetReference.FirstWorksheetIndex &&
					this.address.IntersectsWith(multiSheetExternalRegionReference.Address);
			}

			return false;
		}

		public override string ElementName
		{
			get 
			{
				return
					this.worksheetReference.ReferenceName +
					this.address.ToString(false, false, this.Workbook.CurrentFormat, CellReferenceMode.A1);
			}
		}

		public override bool Equals(object obj)
		{
			IExcelCalcReference reference = obj as IExcelCalcReference;
			if (reference == null)
				return false;

			ExternalRegionCalcReference externalRegionReference = ExcelCalcEngine.GetResolvedReference(reference) as ExternalRegionCalcReference;
			if (externalRegionReference == null)
				return false;

			return
				this.worksheetReference == externalRegionReference.worksheetReference &&
				this.address == externalRegionReference.address;
		}

		public override int GetHashCode()
		{
			return this.worksheetReference.GetHashCode() ^
				this.address.GetHashCode() << 1;
		}

		public override bool IsSubsetReference(IExcelCalcReference inReference)
		{
			Utilities.DebugFail("This seems to only be called on formula owners and an instance of ExternalRegionCalcReference cannot own a formula.");
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

		public WorksheetRegionAddress Address
		{
			get { return this.address; }
		}

		public WorksheetReferenceExternal WorksheetReference
		{
			get { return this.worksheetReference; }
		}

		#endregion // Properties


		#region ExternalRegionValuesArray class

		internal class ExternalRegionValuesArray : ArrayProxy
		{
			#region Member Variable

			private ExternalRegionCalcReference associatedRegion;
			private ExcelCalcValue[,] cachedValuesArray;

			#endregion // Member Variable

			#region Constructor

			public ExternalRegionValuesArray(ExternalRegionCalcReference associatedRegion)
			{
				this.associatedRegion = associatedRegion;
			}

			#endregion // Constructor

			#region GetIteratorHelper

			internal override IEnumerable<KeyValuePair<int, ExcelCalcValue>> GetIteratorHelper(int dimension, int index)
			{
				WorksheetRegionAddress address = this.associatedRegion.Address;
				WorksheetReferenceExternal worksheetReference = this.associatedRegion.WorksheetReference;

				switch (dimension)
				{
					case 0:
						{
							short columnIndexAbsolute = (short)(address.FirstColumnIndex + index);
							for (int rowIndexAbsolute = address.FirstRowIndex; rowIndexAbsolute <= address.LastRowIndex; rowIndexAbsolute++)
							{
								WorksheetReferenceExternal.RowValues rowValues = worksheetReference.TryGetRowValues(rowIndexAbsolute);
								if (rowValues != null)
								{
									yield return new KeyValuePair<int, ExcelCalcValue>(
										rowIndexAbsolute - address.FirstRowIndex,
										new ExcelCalcValue(rowValues.GetCachedValue(columnIndexAbsolute)));
								}
							}
						}
						break;

					case 1:
						{
							int rowIndexAbsolute = address.FirstRowIndex + index;
							WorksheetReferenceExternal.RowValues rowValues = worksheetReference.TryGetRowValues(rowIndexAbsolute);
							if (rowValues != null)
							{
								for (short columnIndexAbsolute = address.FirstColumnIndex; columnIndexAbsolute <= address.LastColumnIndex; columnIndexAbsolute++)
								{
									yield return new KeyValuePair<int, ExcelCalcValue>(
										columnIndexAbsolute - address.FirstColumnIndex,
										new ExcelCalcValue(rowValues.GetCachedValue(columnIndexAbsolute)));
								}
							}
						}
						break;

					default:
						this.ThrowOutOfBoundsException();
						break;
				}
			}

			#endregion // GetIteratorHelper

			#region GetLength

			public override int GetLength(int dimension)
			{
				switch (dimension)
				{
					case 0:
						return this.associatedRegion.Address.Width;

					case 1:
						return this.associatedRegion.Address.Height;

					default:
						this.ThrowOutOfBoundsException();
						return -1;
				}
			}

			#endregion // GetLength

			#region GetValue

			internal override ExcelCalcValue GetValue(int x, int y)
			{
				if (x < 0 || this.GetLength(0) <= x)
					this.ThrowOutOfBoundsException();

				if (y < 0 || this.GetLength(1) <= y)
					this.ThrowOutOfBoundsException();

				WorksheetRegionAddress address = this.associatedRegion.Address;
				short columnIndex = (short)(address.FirstColumnIndex + x);
				int rowIndex = address.FirstRowIndex + y;
				return new ExcelCalcValue(this.associatedRegion.WorksheetReference.GetCachedValue(rowIndex, columnIndex));
			}

			#endregion // GetValue

			#region ToArray

			internal override ExcelCalcValue[,] ToArray()
			{
				if (this.cachedValuesArray == null)
				{
					WorksheetRegionAddress address = this.associatedRegion.Address;
					WorksheetReferenceExternal worksheetReference = this.associatedRegion.WorksheetReference;

					this.cachedValuesArray = new ExcelCalcValue[address.Width, address.Height];

					for (short columnIndex = address.FirstColumnIndex; columnIndex <= address.LastColumnIndex; columnIndex++)
					{
						for (int rowIndex = address.FirstRowIndex; rowIndex <= address.LastRowIndex; rowIndex++)
						{
							object cachedValue = worksheetReference.GetCachedValue(rowIndex, columnIndex);
							ExcelCalcValue calcValue;
							if (cachedValue == null)
								calcValue = ExcelCalcValue.Empty;
							else
								calcValue = new ExcelCalcValue(cachedValue);

							this.cachedValuesArray[columnIndex - address.FirstColumnIndex, rowIndex - address.FirstRowIndex] = calcValue;
						}
					}
				}

				return this.cachedValuesArray;
			}

			#endregion // ToArray
		}

		#endregion // ExternalRegionValuesArray class
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