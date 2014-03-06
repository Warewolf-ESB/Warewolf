using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Diagnostics;
using Infragistics.Documents.Excel.CalcEngine;
using Infragistics.Documents.Excel.FormulaUtilities.CalcEngine;

namespace Infragistics.Documents.Excel
{
	// MD 11/1/10 - TFS56976





	// MD 1/23/12 - TFS99849
	// Made the RegionLookupTable a generic collection instead of a collection of RegionCalcReferences so it could be used in other places.
	//internal class RegionLookupTable
	internal class RegionLookupTable<T>
	{
		#region Member Variables

		// MD 1/23/12 - TFS99849
		// Made the RegionLookupTable a generic collection instead of a collection of RegionCalcReferences so it could be used in other places.
		//private FixedLengthSegmentTree<RegionCalcReference> columnLookupTable;
		//private FixedLengthSegmentTree<RegionCalcReference> rowLookupTable;
		private FixedLengthSegmentTree<T> columnLookupTable;
		private FixedLengthSegmentTree<T> rowLookupTable;

		// MD 3/22/12 - TFS105885
		// We should verify that regions added a removed to the table are part of the same worksheet.
		private Worksheet worksheet;

		#endregion // Member Variables

		#region Constrcutor

		// MD 3/22/12 - TFS105885
		//public RegionLookupTable()
		public RegionLookupTable(Worksheet worksheet)
		{
			// MD 3/22/12 - TFS105885
			this.worksheet = worksheet;

			// MD 1/23/12 - TFS99849
			// Made the RegionLookupTable a generic collection instead of a collection of RegionCalcReferences so it could be used in other places.
			//this.columnLookupTable = new FixedLengthSegmentTree<RegionCalcReference>(Workbook.MaxExcel2007ColumnCount);
			//this.rowLookupTable = new FixedLengthSegmentTree<RegionCalcReference>(Workbook.MaxExcel2007RowCount);
			this.columnLookupTable = new FixedLengthSegmentTree<T>(Workbook.MaxExcel2007ColumnCount);
			this.rowLookupTable = new FixedLengthSegmentTree<T>(Workbook.MaxExcel2007RowCount);
		} 

		#endregion // Constructor

		#region Methods

		#region Add






		// MD 1/23/12 - TFS99849
		// Added a region parameter to the Add method because Add now takes a generic item.
		//internal void Add(RegionCalcReference regionCalcReference)
		//{
		//    WorksheetRegion region = regionCalcReference.Region;
		//
		//    // Add the reference to the lookup table for each dimension.
		//    this.columnLookupTable.Insert(regionCalcReference, region.FirstColumn, region.LastColumn);
		//    this.rowLookupTable.Insert(regionCalcReference, region.FirstRow, region.LastRow);
		//}
		internal void Add(WorksheetRegion region, T item)
		{
			// MD 3/22/12 - TFS105885
			if (region.Worksheet != this.worksheet)
			{
				Utilities.DebugFail("This region is from a different worksheet.");
				return;
			}

			// MD 3/22/12 - TFS105885
			// Moved all code to the new overload.
			this.Add(region.Address, item);
		}

		// MD 3/22/12 - TFS105885
		internal void Add(WorksheetRegionAddress address, T item)
		{
			// Add the reference to the lookup table for each dimension.
			// MD 3/13/12 - 12.1 - Table Support
			//this.columnLookupTable.Insert(item, region.FirstColumn, region.LastColumn);
			//this.rowLookupTable.Insert(item, region.FirstRow, region.LastRow);
			this.columnLookupTable.Insert(item, address.FirstColumnIndex, address.LastColumnIndex);
			this.rowLookupTable.Insert(item, address.FirstRowIndex, address.LastRowIndex);
		}

		#endregion // Add

		// MD 1/23/12 - TFS99849
		#region Clear

		public void Clear()
		{
			this.columnLookupTable.Clear();
			this.rowLookupTable.Clear();
		}

		#endregion // Clear

		// MD 1/23/12 - TFS99849
		#region GetBoundingIndexes

		public void GetBoundingIndexes(out int firstRowIndex, out int firstColumnIndex, out int lastRowIndex, out int lastColumnIndex)
		{
			this.rowLookupTable.GetBoundingIndexes(out firstRowIndex, out lastRowIndex);
			this.columnLookupTable.GetBoundingIndexes(out firstColumnIndex, out lastColumnIndex);
		}

		#endregion // GetBoundingIndexes

		#region GetItemsContainingCell






		// MD 4/12/11 - TFS67084
		// Moved away from using WorksheetCell objects.
		//public List<RegionCalcReference> GetRegionsContainingCell(WorksheetCell cell)
		// MD 1/23/12 - TFS99849
		// Made the RegionLookupTable a generic collection instead of a collection of RegionCalcReferences so it could be used in other places.
		// Also, renamed this method for clarity.
		//public List<RegionCalcReference> GetRegionsContainingCell(WorksheetRow row, short columnIndex)
		public List<T> GetItemsContainingCell(int rowIndex, int columnIndex)
		{
			// Find all regions containing the cell's column.

			// MD 4/12/11 - TFS67084
			// This is passed into the method now.
			//int columnIndex = cell.ColumnIndex;

			// MD 1/23/12 - TFS99849
			// Made the RegionLookupTable a generic collection instead of a collection of RegionCalcReferences so it could be used in other places.
			//List<RegionCalcReference> columnIntersectingRegions = this.columnLookupTable.GetItemsContainingValue(columnIndex);
			List<T> columnIntersectingRegions = this.columnLookupTable.GetItemsContainingValue(columnIndex);
			if (columnIntersectingRegions == null)
				return null;

			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
			//WorksheetRow cellRow = cell.Row;
			//int rowIndex = cellRow.Index;
			// MD 1/23/12 - TFS99849
			// This is now passed in.
			//int rowIndex = row.Index;

			// Find all regions containing the cell's row.
			// MD 1/23/12 - TFS99849
			// Made the RegionLookupTable a generic collection instead of a collection of RegionCalcReferences so it could be used in other places.
			//List<RegionCalcReference> rowIntersectingRegions = this.rowLookupTable.GetItemsContainingValue(rowIndex);
			List<T> rowIntersectingRegions = this.rowLookupTable.GetItemsContainingValue(rowIndex);
			if (rowIntersectingRegions == null)
				return null;

			// Find the regions which exist in both lists.
			return Utilities.IntersetLists(rowIntersectingRegions, columnIntersectingRegions);
		}

		#endregion // GetItemsContainingCell

		// MD 1/23/12 - TFS99849
		#region GetItemsContainingRange






		// MD 3/13/12 - 12.1 - Table Support
		//public List<T> GetItemsContainingRange(WorksheetRegion region)
		public List<T> GetItemsContainingRange(WorksheetRegionAddress region)
		{
			// MD 3/13/12 - 12.1 - Table Support
			//List<T> columnIntersectingRegions = this.columnLookupTable.GetItemsContainingRange(region.FirstColumnInternal, region.LastColumnInternal);
			List<T> columnIntersectingRegions = this.columnLookupTable.GetItemsContainingRange(region.FirstColumnIndex, region.LastColumnIndex);

			if (columnIntersectingRegions == null)
				return null;

			// MD 3/13/12 - 12.1 - Table Support
			//List<T> rowIntersectingRegions = this.rowLookupTable.GetItemsContainingRange(region.FirstRow, region.LastRow);
			List<T> rowIntersectingRegions = this.rowLookupTable.GetItemsContainingRange(region.FirstRowIndex, region.LastRowIndex);

			if (rowIntersectingRegions == null)
				return null;

			// Find the regions which exist in both lists.
			return Utilities.IntersetLists(rowIntersectingRegions, columnIntersectingRegions);
		}

		#endregion // GetItemsContainingRange

		// MD 1/23/12 - TFS99849
		#region GetItemsIntersectingWithRange






		// MD 3/13/12 - 12.1 - Table Support
		//public List<T> GetItemsIntersectingWithRange(WorksheetRegion region)
		//{
		//    return this.GetItemsIntersectingWithRange(region.FirstRow, region.FirstColumnInternal, region.LastRow, region.LastColumnInternal);
		//}
		public List<T> GetItemsIntersectingWithRange(WorksheetRegionAddress region)
		{
			return this.GetItemsIntersectingWithRange(region.FirstRowIndex, region.FirstColumnIndex, region.LastRowIndex, region.LastColumnIndex);
		}






		public List<T> GetItemsIntersectingWithRange(int firstRow, int firstColumn, int lastRow, int lastColumn)
		{
			List<T> columnIntersectingRegions = this.columnLookupTable.GetItemsIntersectingWithRange(firstColumn, lastColumn);
			if (columnIntersectingRegions == null)
				return null;

			List<T> rowIntersectingRegions = this.rowLookupTable.GetItemsIntersectingWithRange(firstRow, lastRow);
			if (rowIntersectingRegions == null)
				return null;

			// Find the regions which exist in both lists.
			return Utilities.IntersetLists(rowIntersectingRegions, columnIntersectingRegions);
		}

		#endregion // GetItemsIntersectingWithRange

		#region Remove






		// MD 1/23/12 - TFS99849
		// Added a region parameter to the Remove method because Remove now takes a generic item.
		//internal void Remove(RegionCalcReference regionCalcReference)
		//{
		//    WorksheetRegion region = regionCalcReference.Region;
		//
		//    // Removes the reference from the lookup table for each dimension.
		//    this.columnLookupTable.Remove(regionCalcReference, region.FirstColumn, region.LastColumn);
		//    this.rowLookupTable.Remove(regionCalcReference, region.FirstRow, region.LastRow);
		//}
		internal void Remove(WorksheetRegion region, T item)
		{
			// MD 3/22/12 - TFS105885
			if (region.Worksheet != this.worksheet)
			{
				Utilities.DebugFail("This region is from a different worksheet.");
				return;
			}

			// MD 3/22/12 - TFS105885
			// Moved all code to the new overload.
			this.Remove(region.Address, item);
		}

		// MD 3/22/12 - TFS105885
		internal void Remove(WorksheetRegionAddress address, T item)
		{
			// Remove the reference from the lookup table for each dimension.
			this.columnLookupTable.Remove(item, address.FirstColumnIndex, address.LastColumnIndex);
			this.rowLookupTable.Remove(item, address.FirstRowIndex, address.LastRowIndex);
		}

		#endregion // Remove

		#endregion // Methods

		#region Properties

		// MD 3/22/12 - TFS105885
		#region IsEmpty

		public bool IsEmpty
		{
			get { return this.columnLookupTable.IsEmpty; }
		}

		#endregion // IsEmpty

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