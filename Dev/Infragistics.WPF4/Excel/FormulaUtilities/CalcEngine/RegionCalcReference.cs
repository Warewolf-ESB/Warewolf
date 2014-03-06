using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Diagnostics;
using Infragistics.Documents.Excel.CalcEngine;

namespace Infragistics.Documents.Excel.FormulaUtilities.CalcEngine
{
	// MD 2/24/12 - 12.1 - Table Support
	// Split this class into a base and derived class.
	//internal sealed class RegionCalcReference : ExcelRefBase
	internal sealed class RegionCalcReference : RegionCalcReferenceBase
	{
		#region Member Variables

		// MD 11/1/10 - TFS56976
		// We will now be doing many dictionary lookups of this object, so cache the hash code so we don't have to go to the region's hashcode each time.
		private int cachedHashCode;

		private WorksheetRegion region;

		// MD 2/24/12 - 12.1 - Table Support
		// Moved to base.
		//private IExcelCalcReferenceCollection referencesCollection;

		#endregion Member Variables

		#region Constructor

		public RegionCalcReference( WorksheetRegion region )
		{
			// MD 12/1/11 - TFS96113
			// Instead of storing arrays for rectangular regions of values we now use an ArrayProxy.
			//this.ValueInternal = new ExcelCalcValue( new ExcelCalcValue[ region.Width, region.Height ] );
			this.ValueInternal = new ExcelCalcValue(new RegionArrayProxy(region));

			this.region = region;

			// MD 11/1/10 - TFS56976
			// We will now be doing many dictionary lookups of this object, so cache the hash code so we don't have to go to the region's hashcode each time.
			this.cachedHashCode = this.region.GetHashCode();
		} 

		#endregion Constructor

		#region Base Class Overrides

		// MD 2/24/12 - 12.1 - Table Support
		#region Moved To Base

		//// MD 4/12/11 - TFS67084
		//// Moved away from using WorksheetCell objects.
		////#region Cell

		////public override WorksheetCell Cell
		////{
		////    get { return this.region.TopLeftCell; }
		////}

		////#endregion Cell

		//// MD 4/12/11 - TFS67084
		//#region ColumnIndex

		//public override short ColumnIndex
		//{
		//    get { return this.region.FirstColumnInternal; }
		//}

		//#endregion  // ColumnIndex

		//#region ContainsReference

		//public override bool ContainsReference( IExcelCalcReference inReference )
		//{
		//    // MD 2/24/12 - 12.1 - Table Support
		//    // Moved this code to a helper method so it could be used elsewhere.
		//    #region Moved

		//    //// MD 4/12/11 - TFS67084
		//    //// Moved away from using WorksheetCell objects.
		//    ////WorksheetCell otherCell = inReference.Context as WorksheetCell;
		//    ////if ( otherCell != null )
		//    ////    return this.region.Contains( otherCell );
		//    //CellCalcReference otherCellReference = inReference.Context as CellCalcReference;
		//    //if (otherCellReference != null)
		//    //{
		//    //    WorksheetRow otherCellRow = otherCellReference.Row;
		//    //    short otherCellColumn = otherCellReference.ColumnIndex;
		//    //    return this.region.Contains(otherCellRow, otherCellColumn);
		//    //}

		//    //WorksheetRegion otherRegion = inReference.Context as WorksheetRegion;
		//    //if ( otherRegion != null )
		//    //    return this.region.IntersectsWith( otherRegion );

		//    //List<WorksheetRegion> otherRegionGroup = inReference.Context as List<WorksheetRegion>;
		//    //if ( otherRegionGroup != null )
		//    //{
		//    //    foreach ( WorksheetRegion otherRegion2 in otherRegionGroup )
		//    //    {
		//    //        if ( this.region.IntersectsWith( otherRegion2 ) )
		//    //            return true;
		//    //    }

		//    //    return false;
		//    //}

		//    //return false;

		//    #endregion // Moved
		//    return CalcUtilities.ContainsReferenceHelper(this.region, inReference);
		//} 

		//#endregion ContainsReference

		//#region Context

		//public override object Context
		//{
		//    get { return this.region; }
		//}

		//#endregion Context

		#endregion // Moved To Base

		#region ElementName

		public override string ElementName
		{
			get { return this.region.ToString( CellReferenceMode.A1, true ); }
		}

		#endregion ElementName

		// MD 12/1/11 - TFS96113
		// This is no longer needed now that we use a derived ArrayProxy for regions references.
		#region Removed

		//#region EnsureArrayValuesCreated

		//public override void EnsureArrayValuesCreated()
		//{
		//    ExcelCalcValue value = this.ValueInternal;
		//    ExcelCalcValue[ , ] array = (ExcelCalcValue[ , ])value.Value;

		//    if ( array[ 0, 0 ] != null )
		//        return;

		//    this.ValueInternal = new ExcelCalcValue( CalcUtilities.GetReferenceValuesForRegion( this.region ) );
		//}

		//#endregion EnsureArrayValuesCreated

		#endregion // Removed

		#region Equals

		public override bool Equals( object obj )
		{
			IExcelCalcReference reference = obj as IExcelCalcReference;

			if ( reference == null )
				return false;

			RegionCalcReference regionReference = ExcelCalcEngine.GetResolvedReference( reference ) as RegionCalcReference;

			if ( regionReference == null )
				return false;

			return this.Region == regionReference.Region;
		}

		#endregion Equals

		#region GetHashCode

		public override int GetHashCode()
		{
			// MD 11/1/10 - TFS56976
			// Use the cached hashcode now.
			//return this.Region.GetHashCode();
			return this.cachedHashCode;
		}

		#endregion GetHashCode

		// MD 2/24/12 - 12.1 - Table Support
		#region Moved To Base

		//#region IsEnumerable

		//public override bool IsEnumerable
		//{
		//    get { return true; }
		//}

		//#endregion IsEnumerable

		//#region IsSubsetReference

		//public override bool IsSubsetReference( IExcelCalcReference inReference )
		//{
		//    /*
		//    IExcelCalcReference resolvedReference = UltraCalcEngine.GetResolvedReference( inReference );

		//    if ( resolvedReference is UCReference )
		//        return false;

		//    WorksheetRegion[] regionGroup = CalcUtilities.GetRegionGroup( resolvedReference.Context );

		//    if ( regionGroup.Length == 0 )
		//        return false;

		//    foreach ( WorksheetRegion otherRegion in regionGroup )
		//    {
		//        if ( this.region.Contains( otherRegion ) == false )
		//            return false;
		//    }

		//    return true;
		//    */

		//    Utilities.DebugFail( "This seems to only be called on formula owners and an instance of RegionCalcReference cannot own a formula." );
		//    return false;
		//} 

		//#endregion IsSubsetReference

		//#region References

		//public override IExcelCalcReferenceCollection References
		//{
		//    get
		//    {
		//        if ( this.referencesCollection == null )
		//            this.referencesCollection = new RegionReferencesCollection( this );

		//        return this.referencesCollection;
		//    }
		//}

		//#endregion References

		//#region ResolveReferenceForArrayFormula

		//protected override ExcelRefBase ResolveReferenceForArrayFormula( CellCalcReference formulaOwner, out ExcelCalcErrorValue errorValue )
		//{
		//    return CalcUtilities.GetSingleReference( formulaOwner, this.region, out errorValue );
		//} 

		//#endregion ResolveReferenceForArrayFormula

		//// MD 4/12/11 - TFS67084
		//#region Row

		//public override WorksheetRow Row
		//{
		//    get { return this.region.TopRow; }
		//}

		//#endregion  // Row

		#endregion // Moved To Base

		#endregion Base Class Overrides

		#region Properties

		#region Region

		// MD 2/24/12 - 12.1 - Table Support
		//public WorksheetRegion Region
		public override WorksheetRegion Region
		{
			get { return this.region; }
		}

		#endregion Region 

		#endregion Properties


		// MD 12/1/11 - TFS96113
		#region RegionArrayProxy class

		// MD 2/24/12 - 12.1 - Table Support
		// Split this class into a base and derived class.
		#region Refactored

		//internal class RegionArrayProxy : ArrayProxy
		//{
		//    private ExcelCalcValue[,] array;
		//    private WorksheetRegion region;

		//    public RegionArrayProxy(WorksheetRegion region)
		//    {
		//        this.region = region;
		//    }

		//    internal override bool ContainsReferences
		//    {
		//        get { return true; }
		//    }

		//    internal override IEnumerable<KeyValuePair<int, ExcelCalcValue>> GetIteratorHelper(int dimension, int index)
		//    {
		//        short firstColumnIndex = this.region.FirstColumnInternal;
		//        int firstRowIndex = this.region.FirstRow;
		//        short lastColumnIndex = this.region.LastColumnInternal;
		//        int lastRowIndex = this.region.LastRow;

		//        switch (dimension)
		//        {
		//            case 0:
		//                {
		//                    // MD 1/3/12 - TFS98766
		//                    // I confused the differences between the absolute and relative indexes here. I have fixed the errors and also 
		//                    // renamed the variables for clarity. 
		//                    //short columnIndex = (short)index;
		//                    //using (IEnumerator<WorksheetRow> rows = this.region.Worksheet.Rows.GetEnumeratorHelper(firstRowIndex, lastRowIndex))
		//                    //{
		//                    //    while (rows.MoveNext())
		//                    //    {
		//                    //        WorksheetRow row = rows.Current;
		//                    //
		//                    //        CellCalcReference cellCalcReference;
		//                    //        if (row.TryGetCellCalcReference(columnIndex, out cellCalcReference))
		//                    //            yield return new KeyValuePair<int, ExcelCalcValue>(row.Index, new ExcelCalcValue(cellCalcReference));
		//                    //    }
		//                    //}
		//                    // 1/11/12 - TFS99216
		//                    // If the cell is referenced as part of a region, it may not have a CellCalcReference, but we should return it if it 
		//                    // has a value. That logic is now in the ShouldIterateCell method.
		//                    //short columnIndexAbsolute = (short)(firstColumnIndex + index);
		//                    //using (IEnumerator<WorksheetRow> rows = this.region.Worksheet.Rows.GetEnumeratorHelper(firstRowIndex, lastRowIndex))
		//                    //{
		//                    //    while (rows.MoveNext())
		//                    //    {
		//                    //        WorksheetRow row = rows.Current;
		//                    //
		//                    //        CellCalcReference cellCalcReference;
		//                    //        if (row.TryGetCellCalcReference(columnIndexAbsolute, out cellCalcReference))
		//                    //            yield return new KeyValuePair<int, ExcelCalcValue>(
		//                    //                row.Index - firstRowIndex,
		//                    //                new ExcelCalcValue(cellCalcReference));
		//                    //    }
		//                    //}
		//                    short columnIndexAbsolute = (short)(firstColumnIndex + index);
		//                    foreach (WorksheetRow row in this.region.Worksheet.Rows.GetItemsInRange(firstRowIndex, lastRowIndex))
		//                    {
		//                        CellCalcReference cellCalcReference;
		//                        if (RegionArrayProxy.ShouldIterateCell(row, columnIndexAbsolute, out  cellCalcReference))
		//                        {
		//                            yield return new KeyValuePair<int, ExcelCalcValue>(
		//                               row.Index - firstRowIndex,
		//                               new ExcelCalcValue(cellCalcReference));
		//                        }
		//                    }
		//                }
		//                break;

		//            case 1:
		//                {
		//                    // MD 1/3/12 - TFS98766
		//                    // I confused the differences between the absolute and relative indexes here. I have fixed the errors and also 
		//                    // renamed the variables for clarity. 
		//                    //WorksheetRow row = this.region.Worksheet.Rows.GetIfCreated(index);
		//                    //for (short i = firstColumnIndex; i <= lastColumnIndex; i++)
		//                    //{
		//                    //    CellCalcReference cellCalcReference;
		//                    //    if (row.TryGetCellCalcReference(i, out cellCalcReference))
		//                    //        yield return new KeyValuePair<int, ExcelCalcValue>(i, new ExcelCalcValue(cellCalcReference));
		//                    //}
		//                    int rowIndexAbsolute = firstRowIndex + index;
		//                    WorksheetRow row = this.region.Worksheet.Rows.GetIfCreated(rowIndexAbsolute);

		//                    // MD 1/12/12
		//                    // Found while fixing TFS99279
		//                    // If the row is null, we have nothing to iterate.
		//                    if (row == null)
		//                        yield break;

		//                    for (short columnIndexAbsolute = firstColumnIndex; columnIndexAbsolute <= lastColumnIndex; columnIndexAbsolute++)
		//                    {
		//                        CellCalcReference cellCalcReference;

		//                        // 1/11/12 - TFS99216
		//                        // If the cell is referenced as part of a region, it may not have a CellCalcReference, but we should return it if it 
		//                        // has a value. That logic is now in the ShouldIterateCell method.
		//                        //if (row.TryGetCellCalcReference(columnIndexAbsolute, out cellCalcReference))
		//                        //    yield return new KeyValuePair<int, ExcelCalcValue>(
		//                        //        columnIndexAbsolute - firstColumnIndex,
		//                        //        new ExcelCalcValue(cellCalcReference));
		//                        if (RegionArrayProxy.ShouldIterateCell(row, columnIndexAbsolute, out  cellCalcReference))
		//                        {
		//                            yield return new KeyValuePair<int, ExcelCalcValue>(
		//                               columnIndexAbsolute - firstColumnIndex,
		//                               new ExcelCalcValue(cellCalcReference));
		//                        }
		//                    }
		//                }
		//                break;

		//            default:
		//                this.ThrowOutOfBoundsException();
		//                break;
		//        }
		//    }

		//    public override int GetLength(int dimension)
		//    {
		//        switch (dimension)
		//        {
		//            case 0:
		//                return this.region.Width;

		//            case 1:
		//                return this.region.Height;

		//            default:
		//                this.ThrowOutOfBoundsException();
		//                return -1;
		//        }
		//    }

		//    internal override ExcelCalcValue GetValue(int x, int y)
		//    {
		//        if (x < 0 || this.region.Width <= x)
		//            this.ThrowOutOfBoundsException();

		//        if (y < 0 || this.region.Height <= y)
		//            this.ThrowOutOfBoundsException();

		//        short columnIndex = (short)(this.region.FirstColumnInternal + x);
		//        int rowIndex = this.region.FirstRow + y;
		//        return new ExcelCalcValue(this.region.Worksheet.Rows[rowIndex].GetCellCalcReference(columnIndex));
		//    }

		//    internal override ExcelCalcValue[,] ToArray()
		//    {
		//        if (this.array == null)
		//            this.array = CalcUtilities.GetReferenceValuesForRegion(this.region);

		//        return this.array;
		//    }

		//    // 1/11/12 - TFS99216
		//    private static bool ShouldIterateCell(WorksheetRow row, short columnIndex, out CellCalcReference cellCalcReference)
		//    {
		//        WorksheetCellBlock cellBlock;
		//        bool forceGetCalcReference = row.TryGetCellBlock(columnIndex, out cellBlock) &&
		//            cellBlock.DoesCellHaveData(row, columnIndex);

		//        if (forceGetCalcReference)
		//        {
		//            cellCalcReference = row.GetCellCalcReference(columnIndex);
		//            return true;
		//        }

		//        return row.TryGetCellCalcReference(columnIndex, out cellCalcReference);
		//    }
		//}

		#endregion // Refactored
		internal class RegionArrayProxy : RegionArrayProxyBase
		{
			private WorksheetRegion region;

			public RegionArrayProxy(WorksheetRegion region)
			{
				Debug.Assert(region != null, "The region cannot be null.");
				this.region = region;
			}

			protected override WorksheetRegion Region
			{
				get { return this.region; }
			}
		}

		#endregion // RegionArrayProxy class

		// MD 2/24/12 - 12.1 - Table Support
		#region Moved To Base

		//#region RegionReferencesCollection class

		//private class RegionReferencesCollection
		//    : IExcelCalcReferenceCollection
		//{
		//    #region Member Variables

		//    private RegionCalcReference regionReference;

		//    #endregion Member Variables

		//    #region Constructor

		//    internal RegionReferencesCollection( RegionCalcReference regionReference )
		//    {
		//        this.regionReference = regionReference;
		//    }

		//    #endregion Constructor

		//    #region Interfaces

		//    #region IEnumerable Members

		//    IEnumerator IEnumerable.GetEnumerator()
		//    {
		//        ExcelCalcNumberStack numberStack = this.regionReference.NumberStack;
		//        IExcelCalcReference formulaOwner = numberStack == null ? null : numberStack.FormulaOwner;

		//        // MD 4/12/11 - TFS67084
		//        // Moved away from using WorksheetCell objects.
		//        //WorksheetCell targetCell = formulaOwner == null ? null : formulaOwner.Context as WorksheetCell;
		//        //WorksheetDataTable dataTable = targetCell == null ? null : targetCell.AssociatedDataTable;
		//        WorksheetRow targetCellRow = null;
		//        WorksheetDataTable dataTable = null;
		//        short targetCellColumnIndex = -1;
		//        if (formulaOwner != null)
		//        {
		//            CellCalcReference targetCellReference = formulaOwner.Context as CellCalcReference;
		//            targetCellRow = targetCellReference.Row;
		//            targetCellColumnIndex = targetCellReference.ColumnIndex;

		//            dataTable = targetCellRow.GetCellValueInternal(targetCellColumnIndex) as WorksheetDataTable;
		//        }

		//        // MD 4/12/11 - TFS67084
		//        // Moved away from using WorksheetCell objects.
		//        //foreach ( WorksheetCell cell in this.Region.GetCreatedCells() )
		//        //{
		//        //    CellCalcReference cellReference = cell.CalcReference;
		//        //
		//        //    // Reset the AddDynamicReferences flag on the cell reference just in case it was set before.
		//        //    cellReference.AddDynamicReferences = false;
		//        //
		//        //    // If we are iterating a group of references and one of them is a row or column input cell for a data table,
		//        //    // the cell returned should actually be the corresponding external cell in tne data table for the cell whose 
		//        //    // value is being calculated. If this replacement occurs, the returned cell reference is automatically a
		//        //    // dynamic reference.
		//        //    if ( dataTable != null && CalcUtilities.PerformDataTableReferenceReplacement( ref cellReference, targetCell, dataTable ) )
		//        //        cellReference.AddDynamicReferences = true;
		//        //
		//        //    cellReference.ExpectedParameterClass = this.regionReference.ExpectedParameterClass;
		//        //    cellReference.NumberStack = this.regionReference.NumberStack;
		//        //
		//        //    yield return cellReference;
		//        //}
		//        WorksheetRegion region = this.Region;

		//        // MD 1/14/12 - TFS99375
		//        // There is now a much faster way to iterate the created rows in a range.
		//        //for (int rowIndex = region.FirstRow; rowIndex <= region.LastRow; rowIndex++)
		//        //{
		//        //    WorksheetRow row = region.Worksheet.Rows.GetIfCreated(rowIndex);
		//        //    if (row == null)
		//        //        continue;
		//        foreach (WorksheetRow row in region.Worksheet.Rows.GetItemsInRange(region.FirstRow, region.LastRow))
		//        {
		//            for (short columnIndex = region.FirstColumnInternal; columnIndex <= region.LastColumnInternal; columnIndex++)
		//            {
		//                CellCalcReference cellReference;
		//                if (row.TryGetCellCalcReference(columnIndex, true, out cellReference) == false)
		//                    continue;

		//                // Reset the AddDynamicReferences flag on the cell reference just in case it was set before.
		//                cellReference.AddDynamicReferences = false;

		//                // If we are iterating a group of references and one of them is a row or column input cell for a data table,
		//                // the cell returned should actually be the corresponding external cell in the data table for the cell whose 
		//                // value is being calculated. If this replacement occurs, the returned cell reference is automatically a
		//                // dynamic reference.
		//                if (dataTable != null && CalcUtilities.PerformDataTableReferenceReplacement(ref cellReference, targetCellRow, targetCellColumnIndex, dataTable))
		//                    cellReference.AddDynamicReferences = true;

		//                cellReference.ExpectedParameterClass = this.regionReference.ExpectedParameterClass;
		//                cellReference.NumberStack = this.regionReference.NumberStack;

		//                yield return cellReference;
		//            }
		//        }
		//    }

		//    #endregion

		//    #endregion Interfaces

		//    #region Internal Properties

		//    #region Region

		//    internal WorksheetRegion Region
		//    {
		//        get
		//        {
		//            return this.regionReference.Region;
		//        }
		//    }

		//    #endregion Region

		//    #endregion Internal Properties
		//}

		//#endregion RegionReferencesCollection class

		#endregion // Moved To Base
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