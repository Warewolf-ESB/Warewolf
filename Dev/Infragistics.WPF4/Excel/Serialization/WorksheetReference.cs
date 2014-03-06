using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Infragistics.Documents.Excel.CalcEngine;
using Infragistics.Documents.Excel.FormulaUtilities;
using Infragistics.Documents.Excel.FormulaUtilities.CalcEngine;
using Infragistics.Documents.Excel.Serialization.BIFF8.BiffRecords;





namespace Infragistics.Documents.Excel.Serialization
{
	// MD 6/13/12 - CalcEngineRefactor
	// Split WorksheetReference into a base class and multiple derived classes.
	#region Old Code

	//#if DEBUG
	//    /// <summary>
	//    /// Represents a formula's reference to a worksheet, whether the worksheet exists or not.
	//    /// </summary>
	//    [DebuggerDisplay( "WorksheetReference: {ReferenceName}" )]
	//#endif
	//    internal class WorksheetReference
	//    {
	//        #region Member Variables

	//        private bool isUsedForExternalNamedReferences;

	//        // MD 3/30/11 - TFS69969
	//        private SortedList<int, RowValues> rowValues;
	//        private SortedList<RegionAddress, ExternalRegionCalcReference> regionReferences;

	//        private WorkbookReferenceBase workbook;
	//        private int worksheetIndex;

	//        #endregion Member Variables

	//        #region Constructor

	//        public WorksheetReference( WorkbookReferenceBase workbook, int worksheetIndex, bool isUsedForExternalNamedReferences )
	//        {
	//            this.workbook = workbook;
	//            this.worksheetIndex = worksheetIndex;
	//            this.isUsedForExternalNamedReferences = isUsedForExternalNamedReferences;
	//        }

	//        #endregion Constructor

	//        #region Properties

	//        #region IsUsedForExternalNamedReferences

	//#if DEBUG
	//        /// <summary>
	//        /// Gets the value which indicates whether this reference occurs in the formula for an 
	//        /// external named reference.
	//        /// </summary> 
	//#endif
	//        public bool IsUsedForExternalNamedReferences
	//        {
	//            get { return this.isUsedForExternalNamedReferences; }
	//        }

	//        #endregion IsUsedForExternalNamedReferences

	//        #region Name

	//#if DEBUG
	//        /// <summary>
	//        /// Gets the name of the referenced worksheet.
	//        /// </summary> 
	//#endif
	//        public string Name
	//        {
	//            get { return this.workbook.GetWorksheetName( this.worksheetIndex ); }
	//        }

	//        #endregion Name

	//        #region ReferenceName
	//#if DEBUG
	//        /// <summary>
	//        /// Gets the reference string to use when creating the string representation of a formula.
	//        /// </summary> 
	//#endif
	//        public string ReferenceName
	//        {
	//            get
	//            {
	//                // MBS 8/18/08 - Excel 2007 Format
	//                //if ( this.worksheetIndex < 0 )
	//                //    return FormulaParser.ReferenceErrorValue;
	//                //
	//                //return this.workbook.GetWorksheetReferenceString( this.worksheetIndex );
	//                return this.GetReferenceName(null);
	//            }
	//        }
	//        #endregion ReferenceName

	//        #region Workbook

	//#if DEBUG
	//        /// <summary>
	//        /// Gets the reference to the workbook the worksheet belongs to, whether or not the workbook exists.
	//        /// </summary> 
	//#endif
	//        public WorkbookReferenceBase Workbook
	//        {
	//            get { return this.workbook; }
	//        }

	//        #endregion Workbook

	//        #region WorksheetIndex

	//#if DEBUG
	//        /// <summary>
	//        /// Gets the index of the worksheet in the workbook, or -1 if the worksheet doesn't exist anymore.
	//        /// </summary> 
	//#endif
	//        public int WorksheetIndex
	//        {
	//            get { return this.worksheetIndex; }
	//        }

	//        #endregion WorksheetIndex

	//        #endregion Properties

	//        #region Methods

	//        // MD 3/30/11 - TFS69969
	//        #region GetCachedValue

	//        // MD 4/12/11 - TFS67084
	//        // Use short instead of int so we don't have to cast.
	//        //public object GetCachedValue(int rowIndex, int columnIndex)
	//        public object GetCachedValue(int rowIndex, short columnIndex)
	//        {
	//            if (this.rowValues != null)
	//            {
	//                RowValues currentRowValues;
	//                if (this.rowValues.TryGetValue(rowIndex, out currentRowValues))
	//                    return currentRowValues.GetCachedValue(columnIndex);
	//            }

	//            return null;
	//        }

	//        #endregion // GetCachedValue

	//        // MD 3/30/11 - TFS69969
	//        #region GetCalcReference

	//        // MD 4/12/11 - TFS67084
	//        // Use short instead of int so we don't have to cast.
	//        //public object GetCalcReference(int rowIndex, int columnIndex)
	//        //{
	//        //    return this.GetRowValues(rowIndex).GetCalcReference(columnIndex);
	//        //}
	//        public object GetCalcReference(int rowIndex, short columnIndex)
	//        {
	//            return this.GetRowValues(rowIndex).GetCalcReference(columnIndex);
	//        }

	//        // MD 4/12/11 - TFS67084
	//        // Use short instead of int so we don't have to cast.
	//        //public object GetCalcReference(int firstRowIndex, int lastRowIndex, int firstColumnIndex, int lastColumnIndex)
	//        public object GetCalcReference(int firstRowIndex, int lastRowIndex, short firstColumnIndex, short lastColumnIndex)
	//        {
	//            if (this.regionReferences == null)
	//                this.regionReferences = new SortedList<RegionAddress, ExternalRegionCalcReference>();

	//            RegionAddress address = new RegionAddress(firstRowIndex, lastRowIndex, firstColumnIndex, lastColumnIndex);
	//            ExternalRegionCalcReference reference;
	//            if (this.regionReferences.TryGetValue(address, out reference) == false)
	//            {
	//                reference = new ExternalRegionCalcReference(this, firstRowIndex, lastRowIndex, firstColumnIndex, lastColumnIndex);
	//                this.regionReferences[address] = reference;
	//            }

	//            return reference;
	//        }

	//        #endregion // GetCalcReference

	//        // MBS 8/18/08 - Excel 2007 Format
	//        #region GetReferenceName
	//        public string GetReferenceName(Dictionary<WorkbookReferenceBase, int> externalReferences)
	//        {
	//            if (this.worksheetIndex < 0)
	//                return FormulaParser.ReferenceErrorValue;

	//            return this.workbook.GetWorksheetReferenceString(this.worksheetIndex, externalReferences);
	//        }
	//        #endregion //GetReferenceName

	//        // MD 3/30/11 - TFS69969
	//        #region GetRowValues

	//        public RowValues GetRowValues(int rowIndex)
	//        {
	//            if (this.rowValues == null)
	//                this.rowValues = new SortedList<int, RowValues>();

	//            RowValues currentRowValues;
	//            if (this.rowValues.TryGetValue(rowIndex, out currentRowValues) == false)
	//            {
	//                currentRowValues = new RowValues(this, rowIndex);
	//                this.rowValues[rowIndex] = currentRowValues;
	//            }

	//            return currentRowValues;
	//        }

	//        #endregion // GetRowValues

	//        // MD 3/30/11 - TFS69969
	//        #region SetCachedValue

	//        // MD 4/12/11 - TFS67084
	//        // Use short instead of int so we don't have to cast.
	//        //public void SetCachedValue(int rowIndex, int columnIndex, object value)
	//        //{
	//        //    this.GetRowValues(rowIndex).SetCachedValue(columnIndex, value);
	//        //}
	//        public void SetCachedValue(int rowIndex, short columnIndex, object value)
	//        {
	//            this.GetRowValues(rowIndex).SetCachedValue(columnIndex, value);
	//        }

	//        #endregion // SetCachedValue

	//        // MD 12/1/11 - TFS96113
	//        #region TryGetRowValues

	//        public RowValues TryGetRowValues(int rowIndex)
	//        {
	//            if (this.rowValues != null)
	//            {
	//                RowValues currentRowValues;
	//                if (this.rowValues.TryGetValue(rowIndex, out currentRowValues))
	//                    return currentRowValues;
	//            }

	//            return null;
	//        }

	//        #endregion // TryGetRowValues

	//        #endregion //Methods


	//        // MD 3/30/11 - TFS69969
	//        #region RegionAddress

	//        // MD 4/19/11 - TFS72977
	//        //private struct RegionAddress
	//        private struct RegionAddress : IComparable<RegionAddress>
	//        {
	//            private int firstColumnIndex;
	//            private int firstRowIndex;
	//            private int lastColumnIndex;
	//            private int lastRowIndex;

	//            public RegionAddress(int firstRowIndex, int lastRowIndex, int firstColumnIndex, int lastColumnIndex)
	//            {
	//                this.firstColumnIndex = firstColumnIndex;
	//                this.firstRowIndex = firstRowIndex;
	//                this.lastColumnIndex = lastColumnIndex;
	//                this.lastRowIndex = lastRowIndex;
	//            }

	//            public override bool Equals(object obj)
	//            {
	//                if ((obj is RegionAddress) == false)
	//                    return false;

	//                RegionAddress other = (RegionAddress)obj;

	//                if (this.firstColumnIndex != other.firstColumnIndex)
	//                    return false;

	//                if (this.firstRowIndex != other.firstRowIndex)
	//                    return false;

	//                if (this.lastColumnIndex != other.lastColumnIndex)
	//                    return false;

	//                if (this.lastRowIndex != other.lastRowIndex)
	//                    return false;

	//                return true;
	//            }

	//            public override int GetHashCode()
	//            {
	//                return
	//                    this.firstColumnIndex.GetHashCode() ^
	//                    this.firstRowIndex.GetHashCode() ^
	//                    this.lastColumnIndex.GetHashCode() ^
	//                    this.lastRowIndex.GetHashCode();
	//            }

	//            // MD 4/19/11 - TFS72977
	//            #region IComparable<RegionAddress> Members

	//            public int CompareTo(RegionAddress other)
	//            {
	//                int result = this.firstColumnIndex - other.firstColumnIndex;
	//                if (result != 0)
	//                    return result;

	//                result = this.firstRowIndex - other.firstRowIndex;
	//                if (result != 0)
	//                    return result;

	//                result = this.lastColumnIndex - other.lastColumnIndex;
	//                if (result != 0)
	//                    return result;

	//                result = this.lastRowIndex - other.lastRowIndex;
	//                if (result != 0)
	//                    return result;

	//                return 0;
	//            }

	//            #endregion
	//        }

	//        #endregion // RegionAddress

	//        // MD 3/30/11 - TFS69969
	//        #region RowValues class

	//        public class RowValues
	//        {
	//            // MD 4/12/11 - TFS67084
	//            // Use short instead of int so we don't have to cast.
	//            //private SortedList<int, object> cellValues;
	//            //private SortedList<int, ExternalCellCalcReference> calcReferences;
	//            private SortedList<short, object> cellValues;
	//            private SortedList<short, ExternalCellCalcReference> calcReferences;

	//            private WorksheetReference owner;
	//            private int rowIndex;

	//            public RowValues(WorksheetReference owner, int rowIndex)
	//            {
	//                this.owner = owner;
	//                this.rowIndex = rowIndex;
	//            }

	//            // MD 4/12/11 - TFS67084
	//            // Use short instead of int so we don't have to cast.
	//            //public object GetCachedValue(int columnIndex)
	//            public object GetCachedValue(short columnIndex)
	//            {
	//                if (this.cellValues != null)
	//                {
	//                    object value;
	//                    if (this.cellValues.TryGetValue(columnIndex, out value))
	//                        return value;
	//                }

	//                return null;
	//            }

	//            // MD 4/12/11 - TFS67084
	//            // Use short instead of int so we don't have to cast.
	//            //public object GetCalcReference(int columnIndex)
	//            public object GetCalcReference(short columnIndex)
	//            {
	//                if (this.calcReferences == null)
	//                {
	//                    // MD 4/12/11 - TFS67084
	//                    // Use short instead of int so we don't have to cast.
	//                    //this.calcReferences = new SortedList<int, ExternalCellCalcReference>();
	//                    this.calcReferences = new SortedList<short, ExternalCellCalcReference>();
	//                }

	//                ExternalCellCalcReference reference;
	//                if (this.calcReferences.TryGetValue(columnIndex, out reference) == false)
	//                {
	//                    reference = new ExternalCellCalcReference(this.owner, this.rowIndex, columnIndex);
	//                    this.calcReferences[columnIndex] = reference;
	//                }

	//                return reference;
	//            }

	//            // MD 4/12/11 - TFS67084
	//            // Use short instead of int so we don't have to cast.
	//            //public void SetCachedValue(int columnIndex, object value)
	//            //{
	//            //    if (this.cellValues == null)
	//            //        this.cellValues = new SortedList<int, object>();
	//            //
	//            //    this.cellValues[columnIndex] = value;
	//            //}
	//            public void SetCachedValue(short columnIndex, object value)
	//            {
	//                if (this.cellValues == null)
	//                    this.cellValues = new SortedList<short, object>();

	//                this.cellValues[columnIndex] = value;
	//            }
	//        }

	//        #endregion // RowValues class
	//    }

	#endregion // Old Code

	#region WorksheetReference class







	internal abstract class WorksheetReference
	{
		#region Member Variables

		private WorkbookReferenceBase workbookReference;

		#endregion Member Variables

		#region Constructor

		protected WorksheetReference(WorkbookReferenceBase workbook)
		{
			this.workbookReference = workbook;
		}

		#endregion Constructor

		#region Methods

		#region Connect

		internal WorksheetReference Connect(FormulaContext context)
		{
			if (this.IsConnected)
				return this;

			if (this.WorkbookReference == null)
			{
				if (context.Workbook == null)
					return this;

				return this.Connect(context.Workbook.CurrentWorkbookReference);
			}

			return this.Connect(this.WorkbookReference.Connect(context));
		}

		internal virtual WorksheetReference Connect(WorkbookReferenceBase workbookReference)
		{
			return this;
		}

		#endregion // Connect

		public abstract WorksheetReference Disconnect();
		public abstract IExcelCalcReference GetCalcReference(WorksheetCellAddress cellAddress);
		public abstract IExcelCalcReference GetCalcReference(WorksheetRegionAddress regionAddress);

		#region GetReferenceName

		public abstract string GetReferenceName(Dictionary<WorkbookReferenceBase, int> externalReferences);

		#endregion //GetReferenceName

		#endregion //Methods

		#region Properties

		#region FirstWorksheetIndex






		public abstract int FirstWorksheetIndex { get; }

		#endregion FirstWorksheetIndex

		#region GetNamedReference

		public abstract NamedReferenceBase GetNamedReference(string name);

		#endregion // GetNamedReference

		#region IsConnected

		public virtual bool IsConnected
		{
			get { return true; }
		}

		#endregion // IsConnected

		#region IsExternal

		public abstract bool IsExternal { get; }

		#endregion // IsExternal

		#region IsMultiSheet

		public abstract bool IsMultiSheet { get; }

		#endregion // IsMultiSheet

		#region LastWorksheetIndex







		public virtual int LastWorksheetIndex
		{
			get { return this.FirstWorksheetIndex; }
		}

		#endregion // LastWorksheetIndex

		#region NamedReferenceScope

		public virtual object NamedReferenceScope
		{
			get { return this; }
		}

		#endregion // NamedReferenceScope

		#region ReferenceName






		public string ReferenceName
		{
			get { return this.GetReferenceName(null); }
		}

		#endregion ReferenceName

		#region WorkbookReference






		public WorkbookReferenceBase WorkbookReference
		{
			get { return this.workbookReference; }
		}

		#endregion WorkbookReference

		#endregion Properties
	}

	#endregion // WorksheetReference class


	#region WorksheetReferenceSingle class






	internal abstract class WorksheetReferenceSingle : WorksheetReference
	{
		#region Constructor

		protected WorksheetReferenceSingle(WorkbookReferenceBase workbook)
			: base(workbook) { }

		#endregion Constructor

		#region Base Class Overrides

		#region Disconnect

		public override WorksheetReference Disconnect()
		{
			string fileName = null;

			WorkbookReferenceBase workbookReference = this.WorkbookReference;
			if (workbookReference != null)
				fileName = workbookReference.FileName;

			return new WorksheetReferenceSingleUnconnected(fileName, this.Name);
		}

		#endregion // Disconnect

		#region GetReferenceName

		public override string GetReferenceName(Dictionary<WorkbookReferenceBase, int> externalReferences)
		{
			return this.WorkbookReference.GetWorksheetReferenceString(this.FirstWorksheetIndex, this.LastWorksheetIndex, externalReferences);
		}

		#endregion //GetReferenceName

		#region IsMultiSheet

		public sealed override bool IsMultiSheet
		{
			get { return false; }
		}

		#endregion // IsMultiSheet

		#endregion // Base Class Overrides

		#region Methods

		public abstract IExcelCalcReference GetMultiSheetCalcReference(WorksheetReference lastSheetReference, WorksheetCellAddress cellAddress);
		public abstract IExcelCalcReference GetMultiSheetCalcReference(WorksheetReference lastSheetReference, WorksheetRegionAddress regionAddress);

		#endregion // Methods

		#region Properties

		#region Name






		public abstract string Name { get; }

		#endregion Name

		#endregion // Properties
	}

	#endregion // WorksheetReferenceSingle class


	#region WorksheetReferenceError class






	internal class WorksheetReferenceError : WorksheetReferenceSingle
	{
		#region Constructor

		public WorksheetReferenceError(WorkbookReferenceBase workbook)
			: base(workbook) { }

		#endregion // Constructor

		#region Base Class Overrides

		#region Connect

		internal override WorksheetReference Connect(WorkbookReferenceBase workbookReference)
		{
			return workbookReference.GetWorksheetReference(this.FirstWorksheetIndex);
		}

		#endregion // Connect

		#region FirstWorksheetIndex






		public override int FirstWorksheetIndex
		{
			get { return EXTERNSHEETRecord.SheetCannotBeFoundIndex; }
		}

		#endregion FirstWorksheetIndex

		#region GetCalcReference

		public override IExcelCalcReference GetCalcReference(WorksheetCellAddress cellAddress)
		{
			return ExcelReferenceError.Instance;
		}

		public override IExcelCalcReference GetCalcReference(WorksheetRegionAddress regionAddress)
		{
			return ExcelReferenceError.Instance;
		}

		#endregion // GetCalcReference

		#region GetMultiSheetCalcReference

		public override IExcelCalcReference GetMultiSheetCalcReference(WorksheetReference lastSheetReference, WorksheetCellAddress cellAddress)
		{
			return ExcelReferenceError.Instance;
		}

		public override IExcelCalcReference GetMultiSheetCalcReference(WorksheetReference lastSheetReference, WorksheetRegionAddress regionAddress)
		{
			return ExcelReferenceError.Instance;
		}

		#endregion // GetMultiSheetCalcReference

		#region GetNamedReference

		public override NamedReferenceBase GetNamedReference(string name)
		{
			Utilities.DebugFail("I don't think this should every be called.");
			return null;
		}

		#endregion // GetNamedReference

		#region IsConnected

		public override bool IsConnected
		{
			get { return this.WorkbookReference.IsConnected; }
		}

		#endregion // IsConnected

		#region IsExternal

		public override bool IsExternal
		{
			get { return this.WorkbookReference.IsExternal; }
		}

		#endregion // IsExternal

		#region Name






		public override string Name
		{
			get { return FormulaParser.ReferenceErrorValue; }
		}

		#endregion // Name

		#endregion // Base Class Overrides
	}

	#endregion // WorksheetReferenceError class

	#region WorksheetReferenceExternal class






	internal class WorksheetReferenceExternal : WorksheetReferenceSingle
	{
		#region Member Variables

		private SortedList<int, RowValues> rowValues;
		private SortedList<WorksheetRegionAddress, ExternalRegionCalcReference> regionReferences;
		private int worksheetIndex;

		#endregion // Member Variables

		#region Constructor

		public WorksheetReferenceExternal(WorkbookReferenceBase workbook, int worksheetIndex)
			: base(workbook)
		{
			this.worksheetIndex = worksheetIndex;
		}

		#endregion // Constructor

		#region Base Class Overrides

		#region FirstWorksheetIndex






		public override int FirstWorksheetIndex
		{
			get { return this.worksheetIndex; }
		}

		#endregion FirstWorksheetIndex

		#region GetCalcReference

		public override IExcelCalcReference GetCalcReference(WorksheetCellAddress cellAddress)
		{
			return this.GetRowValues(cellAddress.RowIndex).GetCalcReference(cellAddress.ColumnIndex);
		}

		public override IExcelCalcReference GetCalcReference(WorksheetRegionAddress regionAddress)
		{
			if (this.regionReferences == null)
				this.regionReferences = new SortedList<WorksheetRegionAddress, ExternalRegionCalcReference>();

			ExternalRegionCalcReference reference;
			if (this.regionReferences.TryGetValue(regionAddress, out reference) == false)
			{
				reference = new ExternalRegionCalcReference(this, regionAddress);
				this.regionReferences[regionAddress] = reference;
			}

			return reference;
		}

		#endregion // GetCalcReference

		#region GetMultiSheetCalcReference

		public override IExcelCalcReference GetMultiSheetCalcReference(WorksheetReference lastSheetReference, WorksheetCellAddress cellAddress)
		{
			WorksheetReferenceExternal lastSheetExternal = lastSheetReference as WorksheetReferenceExternal;
			if (lastSheetExternal == null)
			{
				Utilities.DebugFail("The other reference should also be an external worksheet.");
				return null;
			}

			return new MultiSheetExternalCellCalcReference(this, lastSheetExternal, cellAddress);
		}

		public override IExcelCalcReference GetMultiSheetCalcReference(WorksheetReference lastSheetReference, WorksheetRegionAddress regionAddress)
		{
			WorksheetReferenceExternal lastSheetExternal = lastSheetReference as WorksheetReferenceExternal;
			if (lastSheetExternal == null)
			{
				Utilities.DebugFail("The other reference should also be an external worksheet.");
				return null;
			}

			return new MultiSheetExternalRegionCalcReference(this, lastSheetExternal, regionAddress);
		}

		#endregion // GetMultiSheetCalcReference

		#region GetNamedReference

		public override NamedReferenceBase GetNamedReference(string name)
		{
			return this.WorkbookReference.GetNamedReference(name, this, true);
		}

		#endregion // GetNamedReference

		#region IsExternal

		public override bool IsExternal
		{
			get { return true; }
		}

		#endregion // IsExternal

		#region Name






		public override string Name
		{
			get { return this.WorkbookReference.GetWorksheetName(this.worksheetIndex); }
		}

		#endregion // Name

		#endregion // Base Class Overrides

		#region Methods

		#region GetCachedValue

		public object GetCachedValue(int rowIndex, short columnIndex)
		{
			if (this.rowValues != null)
			{
				RowValues currentRowValues;
				if (this.rowValues.TryGetValue(rowIndex, out currentRowValues))
					return currentRowValues.GetCachedValue(columnIndex);
			}

			return null;
		}

		#endregion // GetCachedValue

		#region GetRowValues

		public RowValues GetRowValues(int rowIndex)
		{
			if (this.rowValues == null)
				this.rowValues = new SortedList<int, RowValues>();

			RowValues currentRowValues;
			if (this.rowValues.TryGetValue(rowIndex, out currentRowValues) == false)
			{
				currentRowValues = new RowValues(this, rowIndex);
				this.rowValues[rowIndex] = currentRowValues;
			}

			return currentRowValues;
		}

		#endregion // GetRowValues

		#region SetCachedValue

		public void SetCachedValue(int rowIndex, short columnIndex, object value)
		{
			this.GetRowValues(rowIndex).SetCachedValue(columnIndex, value);
		}

		#endregion // SetCachedValue

		#region TryGetRowValues

		public RowValues TryGetRowValues(int rowIndex)
		{
			if (this.rowValues != null)
			{
				RowValues currentRowValues;
				if (this.rowValues.TryGetValue(rowIndex, out currentRowValues))
					return currentRowValues;
			}

			return null;
		}

		#endregion // TryGetRowValues

		#endregion // Methods

		#region Properties

		// MD 7/10/12 - TFS116306
		#region CachedRowValues

		public IEnumerable<RowValues> CachedRowValues
		{
			get
			{
				if (this.rowValues == null)
					return new RowValues[0];

				return this.rowValues.Values; 
			}
		}

		#endregion // CachedRowValues

		// MD 7/10/12 - TFS116306
		#region CachedRowValuesCount

		public int CachedRowValuesCount
		{
			get
			{
				if (this.rowValues == null)
					return 0;

				return this.rowValues.Count;
			}
		}

		#endregion // CachedRowValuesCount

		#endregion // Properties

		#region RowValues class

		public class RowValues
		{
			private SortedList<short, object> cellValues;
			private SortedList<short, ExternalCellCalcReference> calcReferences;

			private WorksheetReferenceExternal owner;
			private int rowIndex;

			public RowValues(WorksheetReferenceExternal owner, int rowIndex)
			{
				this.owner = owner;
				this.rowIndex = rowIndex;
			}

			public object GetCachedValue(short columnIndex)
			{
				if (this.cellValues != null)
				{
					object value;
					if (this.cellValues.TryGetValue(columnIndex, out value))
						return value;
				}

				return null;
			}

			// MD 7/10/12 - TFS116306
			public IEnumerable<KeyValuePair<short, object>> GetCachedValues()
			{
				if (this.cellValues == null)
					yield break;

				foreach (KeyValuePair<short, object> pair in this.cellValues)
					yield return pair;
			}

			public IExcelCalcReference GetCalcReference(short columnIndex)
			{
				if (this.calcReferences == null)
					this.calcReferences = new SortedList<short, ExternalCellCalcReference>();

				ExternalCellCalcReference reference;
				if (this.calcReferences.TryGetValue(columnIndex, out reference) == false)
				{
					reference = new ExternalCellCalcReference(this.owner, this.rowIndex, columnIndex);
					this.calcReferences[columnIndex] = reference;
				}

				return reference;
			}

			public void SetCachedValue(short columnIndex, object value)
			{
				if (this.cellValues == null)
					this.cellValues = new SortedList<short, object>();

				this.cellValues[columnIndex] = value;
			}

			// MD 7/10/12 - TFS116306
			public int CachedValueCount
			{
				get
				{
					if (this.cellValues == null)
						return 0;

					return this.cellValues.Count;
				}
			}

			// MD 7/10/12 - TFS116306
			public int RowIndex
			{
				get { return this.rowIndex; }
			}
		}

		#endregion // RowValues class
	}

	#endregion // WorksheetReferenceExternal class

	#region WorksheetReferenceLocal class






	internal class WorksheetReferenceLocal : WorksheetReferenceSingle
	{
		#region Member Variables

		private Worksheet worksheet;

		#endregion // Member Variables

		#region Constructor

		public WorksheetReferenceLocal(CurrentWorkbookReference workbook, Worksheet worksheet)
			: base(workbook)
		{
			this.worksheet = worksheet;
			Debug.Assert(this.worksheet != null, "The worksheet should not be null.");
		}

		#endregion // Constructor

		#region Base Class Overrides

		#region FirstWorksheetIndex






		public override int FirstWorksheetIndex
		{
			get
			{
				if (this.worksheet == null)
					return -1;

				return this.worksheet.Index;
			}
		}

		#endregion FirstWorksheetIndex

		#region GetCalcReference

		public override IExcelCalcReference GetCalcReference(WorksheetCellAddress cellAddress)
		{
			if (this.worksheet == null || this.worksheet.Workbook == null)
				return ExcelReferenceError.Instance;

			return this.worksheet.Rows[cellAddress.RowIndex].GetCellCalcReference(cellAddress.ColumnIndex);
		}

		public override IExcelCalcReference GetCalcReference(WorksheetRegionAddress regionAddress)
		{
			if (this.worksheet == null || this.worksheet.Workbook == null)
				return ExcelReferenceError.Instance;

			WorksheetRegion region = this.worksheet.GetCachedRegion(regionAddress);
			return region.CalcReference;
		}

		#endregion // GetCalcReference

		#region GetMultiSheetCalcReference

		public override IExcelCalcReference GetMultiSheetCalcReference(WorksheetReference lastSheetReference, WorksheetCellAddress cellAddress)
		{
			WorksheetReferenceLocal lastSheetLocal = lastSheetReference as WorksheetReferenceLocal;
			if (lastSheetLocal == null)
			{
				Utilities.DebugFail("The other reference should also be a local worksheet.");
				return null;
			}

			return new MultiSheetCellCalcReference(this.worksheet, lastSheetLocal.worksheet, cellAddress);
		}

		public override IExcelCalcReference GetMultiSheetCalcReference(WorksheetReference lastSheetReference, WorksheetRegionAddress regionAddress)
		{
			WorksheetReferenceLocal lastSheetLocal = lastSheetReference as WorksheetReferenceLocal;
			if (lastSheetLocal == null)
			{
				Utilities.DebugFail("The other reference should also be a local worksheet.");
				return null;
			}

			return new MultiSheetRegionCalcReference(this.worksheet, lastSheetLocal.worksheet, regionAddress);
		}

		#endregion // GetMultiSheetCalcReference

		#region GetNamedReference

		public override NamedReferenceBase GetNamedReference(string name)
		{
			if (this.worksheet == null || this.worksheet.Workbook == null)
				return null;

			return this.worksheet.Workbook.NamedReferences.Find(name, this.worksheet);
		}

		#endregion // GetNamedReference

		#region IsExternal

		public override bool IsExternal
		{
			get { return false; }
		}

		#endregion // IsExternal

		#region Name






		public override string Name
		{
			get
			{
				if (this.worksheet == null)
					return FormulaParser.ReferenceErrorValue;

				return this.worksheet.Name;
			}
		}

		#endregion // Name

		#region NamedReferenceScope

		public override object NamedReferenceScope
		{
			get { return this.worksheet; }
		}

		#endregion // NamedReferenceScope

		#endregion // Base Class Overrides

		#region Worksheet

		public Worksheet Worksheet
		{
			get { return this.worksheet; }
		}

		#endregion // Worksheet
	}

	#endregion // WorksheetReferenceLocal class

	#region WorksheetReferenceSingleUnconnected class



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

	internal class WorksheetReferenceSingleUnconnected : WorksheetReferenceSingle
	{
		#region Member Variables

		private string name;

		#endregion // Member Variables

		#region Constructor

		public WorksheetReferenceSingleUnconnected(string workbookFileName, string name)
			: this(new WorkbookReferenceUnconnected(workbookFileName), name) { }

		public WorksheetReferenceSingleUnconnected(WorkbookReferenceUnconnected workbookReference, string name)
			: base(workbookReference)
		{
			this.name = name;
		}

		#endregion // Constructor

		#region Base Class Overrides

		#region Connect

		internal override WorksheetReference Connect(WorkbookReferenceBase workbookReference)
		{
			return workbookReference.GetWorksheetReference(this.name);
		}

		#endregion // Connect

		#region Disconnect

		public override WorksheetReference Disconnect()
		{
			return this;
		}

		#endregion // Disconnect

		#region Equals

		public override bool Equals(object obj)
		{
			if (Object.ReferenceEquals(this, obj))
				return true;

			WorksheetReferenceSingleUnconnected other = obj as WorksheetReferenceSingleUnconnected;
			if (other == null)
				return false;

			return
				Object.Equals(this.WorkbookReference, other.WorkbookReference) &&
				this.name == other.name;
		}

		#endregion // Equals

		#region FirstWorksheetIndex

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public override int FirstWorksheetIndex
		{
			get
			{
				Utilities.DebugFail("The WorksheetReferenceSingleUnconnected.FirstWorksheetIndex should not be requested.");
				return 0;
			}
		}

		#endregion // FirstWorksheetIndex

		#region GetCalcReference

		public override IExcelCalcReference GetCalcReference(WorksheetCellAddress cellAddress)
		{
			return ExcelReferenceError.Instance;
		}

		public override IExcelCalcReference GetCalcReference(WorksheetRegionAddress regionAddress)
		{
			return ExcelReferenceError.Instance;
		}

		#endregion // GetCalcReference

		#region GetHashCode

		public override int GetHashCode()
		{
			return this.name.GetHashCode();
		}

		#endregion // GetHashCode

		#region GetMultiSheetCalcReference

		public override IExcelCalcReference GetMultiSheetCalcReference(WorksheetReference lastSheetReference, WorksheetCellAddress cellAddress)
		{
			return ExcelReferenceError.Instance;
		}

		public override IExcelCalcReference GetMultiSheetCalcReference(WorksheetReference lastSheetReference, WorksheetRegionAddress regionAddress)
		{
			return ExcelReferenceError.Instance;
		}

		#endregion // GetMultiSheetCalcReference

		#region GetNamedReference

		public override NamedReferenceBase GetNamedReference(string name)
		{
			return null;
		}

		#endregion // GetNamedReference

		#region GetReferenceName

		public override string GetReferenceName(Dictionary<WorkbookReferenceBase, int> externalReferences)
		{
			return Utilities.CreateReferenceString(this.WorkbookReference.FileName, this.name, null);
		}

		#endregion //GetReferenceName

		#region IsConnected

		public override bool IsConnected
		{
			get { return false; }
		}

		#endregion // IsConnected

		#region IsExternal

		public override bool IsExternal
		{
			get { return this.WorkbookReference.IsExternal; }
		}

		#endregion // IsExternal

		#region Name






		public override string Name
		{
			get { return this.name; }
		}

		#endregion // Name

		#endregion // Base Class Overrides
	}

	#endregion // WorksheetReferenceSingleUnconnected class

	#region WorksheetReferenceToWorkbook class







	internal class WorksheetReferenceToWorkbook : WorksheetReferenceSingle
	{
		#region Constructor

		public WorksheetReferenceToWorkbook(WorkbookReferenceBase workbook)
			: base(workbook) { }

		#endregion // Constructor

		#region Base Class Overrides

		#region Connect

		internal override WorksheetReference Connect(WorkbookReferenceBase workbookReference)
		{
			return workbookReference.GetWorksheetReference(this.FirstWorksheetIndex);
		}

		#endregion // Connect

		#region FirstWorksheetIndex






		public override int FirstWorksheetIndex
		{
			get { return EXTERNSHEETRecord.WorkbookLevelReferenceIndex; }
		}

		#endregion FirstWorksheetIndex

		#region GetCalcReference

		public override IExcelCalcReference GetCalcReference(WorksheetCellAddress cellAddress)
		{
			Utilities.DebugFail("This type of calc reference is not valid for a workbook level reference.");
			return ExcelReferenceError.Instance;
		}

		public override IExcelCalcReference GetCalcReference(WorksheetRegionAddress regionAddress)
		{
			Utilities.DebugFail("This type of calc reference is not valid for a workbook level reference.");
			return ExcelReferenceError.Instance;
		}

		#endregion // GetCalcReference

		#region GetMultiSheetCalcReference

		public override IExcelCalcReference GetMultiSheetCalcReference(WorksheetReference lastSheetReference, WorksheetCellAddress cellAddress)
		{
			Utilities.DebugFail("This type of calc reference is not valid for a workbook level reference.");
			return ExcelReferenceError.Instance;
		}

		public override IExcelCalcReference GetMultiSheetCalcReference(WorksheetReference lastSheetReference, WorksheetRegionAddress regionAddress)
		{
			Utilities.DebugFail("This type of calc reference is not valid for a workbook level reference.");
			return ExcelReferenceError.Instance;
		}

		#endregion // GetMultiSheetCalcReference

		#region GetNamedReference

		public override NamedReferenceBase GetNamedReference(string name)
		{
			return this.WorkbookReference.GetNamedReference(name, this.WorkbookReference.WorkbookScope, true);
		}

		#endregion // GetNamedReference

		#region IsConnected

		public override bool IsConnected
		{
			get { return this.WorkbookReference.IsConnected; }
		}

		#endregion // IsConnected

		#region IsExternal

		public override bool IsExternal
		{
			get { return this.WorkbookReference.IsExternal; }
		}

		#endregion // IsExternal

		#region Name






		public override string Name
		{
			get { return null; }
		}

		#endregion // Name

		#region NamedReferenceScope

		public override object NamedReferenceScope
		{
			get { return this.WorkbookReference.WorkbookScope; }
		}

		#endregion // NamedReferenceScope

		#endregion // Base Class Overrides
	}

	#endregion // WorksheetReferenceToWorkbook class


	// MD 6/18/12 - TFS102878
	#region WorksheetReferenceMulti class







	internal class WorksheetReferenceMulti : WorksheetReference
	{
		#region Member Variables

		private WorksheetReferenceSingle firstWorksheetReference;
		private WorksheetReferenceSingle lastWorksheetReference;

		#endregion // Member Variables

		#region Constructor

		public WorksheetReferenceMulti(WorksheetReferenceSingle firstWorksheetReference, WorksheetReferenceSingle lastWorksheetReference)
			: base(firstWorksheetReference.WorkbookReference)
		{
			Debug.Assert(firstWorksheetReference.WorkbookReference == lastWorksheetReference.WorkbookReference, "The worksheets should be from the same workbook.");
			Debug.Assert(firstWorksheetReference.GetType() == lastWorksheetReference.GetType(), "The worksheets should be the same type.");

			this.firstWorksheetReference = firstWorksheetReference;
			this.lastWorksheetReference = lastWorksheetReference;
		}

		#endregion // Constructor

		#region Base Class Overrides

		#region Connect

		internal override WorksheetReference Connect(WorkbookReferenceBase workbookReference)
		{
			return workbookReference.GetWorksheetReference(this.firstWorksheetReference.Name, this.lastWorksheetReference.Name);
		}

		#endregion // Connect

		#region Disconnect

		public sealed override WorksheetReference Disconnect()
		{
			if (this.IsConnected == false)
				return this;

			string fileName = null;

			WorkbookReferenceBase workbookReference = this.WorkbookReference;
			if (workbookReference != null)
				fileName = workbookReference.FileName;

			return new WorkbookReferenceUnconnected(fileName).GetWorksheetReference(this.firstWorksheetReference.Name, this.lastWorksheetReference.Name);
		}

		#endregion // Disconnect

		#region FirstWorksheetIndex

		public override int FirstWorksheetIndex
		{
			get { return this.firstWorksheetReference.FirstWorksheetIndex; }
		}

		#endregion // FirstWorksheetIndex

		#region GetCalcReference

		public override IExcelCalcReference GetCalcReference(WorksheetCellAddress cellAddress)
		{
			int firstIndex = this.FirstWorksheetIndex;
			int lastIndex = this.LastWorksheetIndex;

			if (firstIndex == -1 || firstIndex == lastIndex)
				return this.firstWorksheetReference.GetCalcReference(cellAddress);

			return this.firstWorksheetReference.GetMultiSheetCalcReference(this.lastWorksheetReference, cellAddress);
		}

		#endregion // GetCalcReference

		#region GetCalcReference

		public override IExcelCalcReference GetCalcReference(WorksheetRegionAddress regionAddress)
		{
			int firstIndex = this.FirstWorksheetIndex;
			int lastIndex = this.LastWorksheetIndex;

			if (firstIndex == -1 || firstIndex == lastIndex)
				return this.firstWorksheetReference.GetCalcReference(regionAddress);

			return this.firstWorksheetReference.GetMultiSheetCalcReference(this.lastWorksheetReference, regionAddress);
		}

		#endregion // GetCalcReference

		#region GetNamedReference

		public override NamedReferenceBase GetNamedReference(string name)
		{
			Utilities.DebugFail("This should never be called.");
			return null;
		}

		#endregion // GetNamedReference

		#region GetReferenceName

		public override string GetReferenceName(Dictionary<WorkbookReferenceBase, int> externalReferences)
		{
			if (this.IsConnected)
				return this.WorkbookReference.GetWorksheetReferenceString(this.FirstWorksheetIndex, this.LastWorksheetIndex, externalReferences);

			return Utilities.CreateReferenceString(this.WorkbookReference.FileName, this.firstWorksheetReference.Name, this.lastWorksheetReference.Name);
		}

		#endregion //GetReferenceName

		#region IsConnected

		public override bool IsConnected
		{
			get { return this.firstWorksheetReference.IsConnected; }
		}

		#endregion // IsConnected

		#region IsExternal

		public override bool IsExternal
		{
			get { return this.firstWorksheetReference.IsExternal; }
		}

		#endregion // IsExternal

		#region IsMultiSheet

		public override bool IsMultiSheet
		{
			get { return true; }
		}

		#endregion // IsMultiSheet

		#region LastWorksheetIndex

		public override int LastWorksheetIndex
		{
			get { return this.lastWorksheetReference.FirstWorksheetIndex; }
		}

		#endregion // LastWorksheetIndex

		#region NamedReferenceScope

		public override object NamedReferenceScope
		{
			get
			{
				Utilities.DebugFail("A multi-sheet reference should never be used as the scope for a named reference.");
				return this.firstWorksheetReference.NamedReferenceScope;
			}
		}

		#endregion // NamedReferenceScope

		#endregion // Base Class Overrides

		#region Properties

		#region FirstWorksheetReference

		public WorksheetReferenceSingle FirstWorksheetReference
		{
			get { return this.firstWorksheetReference; }
		}

		#endregion // FirstWorksheetReference

		#region LastWorksheetReference

		public WorksheetReferenceSingle LastWorksheetReference
		{
			get { return this.lastWorksheetReference; }
		}

		#endregion // LastWorksheetReference

		#endregion // Properties
	}

	#endregion // WorksheetReferenceMulti class
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