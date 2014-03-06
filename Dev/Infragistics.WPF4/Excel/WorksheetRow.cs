using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.ComponentModel;
using System.Collections;
using Infragistics.Documents.Excel.CalcEngine;
using Infragistics.Documents.Excel.FormulaUtilities;
using Infragistics.Documents.Excel.FormulaUtilities.CalcEngine;
using Infragistics.Documents.Excel.Serialization;



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

using Infragistics.Shared;
using System.Drawing;
using System.Windows.Forms;
using System.Globalization;


namespace Infragistics.Documents.Excel
{
	/// <summary>
	/// Represents a row in a Microsoft Excel worksheet.
	/// </summary>
	[DebuggerDisplay( "Row: Index - {Index}" )]



	public

		 class WorksheetRow : RowColumnBase
	{
		#region Constants

		// MD 4/12/11 - TFS67084
		internal const int CellBlockSize = 32; 

		#endregion  // Constants

		#region Member Variables

		private WorksheetCellCollection cells;

		// MD 3/16/09 - TFS14252
		// Only 2 bytes are needed to describe the height. To cut down on the memory footprint, 
		// this has been changed to a short.
		//private int height = -1;
		private short height = -1;

		// MD 4/12/11 - TFS67084
		// All cell data is now stored on the WorksheetRow or Worksheet itself. 
		// We will no longer use WorksheetCell instances to hold data.
		// MD 1/31/12 - TFS100573
		// The block will now store its own index so we can use a sorted array instead of a SortedList<T>, which
		// has two arrays instead of one.
		//private SortedList<short, WorksheetCellBlock> cellBlocks;
		private WorksheetCellBlock[] cellBlocks;

		private SortedList<short, CellCalcReference[]> cellCalcReferences;
		private SortedList<short, WorksheetCellFormatData[]> cellFormatsForCells;

		private List<Formula> cellOwnedFormulas;

		private short resolvedRowHeight;

		// MD 3/15/12 - TFS104581
		// Moved these members from the base class because they are no longer needed for columns.
		private bool hidden;
		private readonly int index;
		private byte outlineLevel;

		#region Serialization Cache

		// MD 7/26/10 - TFS34398
		// We will now store these on the serialization manager in a WorksheetRowSerializationCache instance.
		//// These are only valid when the row's worksheet is about to be saved
		//private bool hasCollapseIndicator;
		//private bool hasData;
		//private int firstCell;
		//private int firstCellInUndefinedTail;

		#endregion Serialization Cache

		#endregion Member Variables

		#region Constructor

		// MD 3/15/12 - TFS104581
		//internal WorksheetRow( Worksheet worksheet, int index )
		//    : base( worksheet, index ) 
		//{
		internal WorksheetRow(Worksheet worksheet, int index)
			: base(worksheet)
		{
			this.index = index;

			// MD 2/27/12
			// Found while implementing 12.1 - Table Support
			// This will improve performance while filtering because we won't have to measure rows in GetResolvedHeight until the wrap 
			// text or font properties change on cells
			resolvedRowHeight = (short)worksheet.DefaultRowHeightResolved;
		}

		#endregion Constructor

		#region Base Class Overrides

		// MD 3/15/12 - TFS104581
		#region CreateCellFormatProxy

		internal override WorksheetCellFormatProxy CreateCellFormatProxy(GenericCachedCollectionEx<WorksheetCellFormatData> cellFormatCollection)
		{
			return new WorksheetCellFormatProxy(cellFormatCollection, this);
		}

		#endregion // CreateCellFormatProxy

		// MD 3/22/12 - TFS104630
		#region GetAdjacentFormatForBorderResolution

		internal override WorksheetCellFormatData GetAdjacentFormatForBorderResolution(WorksheetCellFormatProxy sender, CellFormatValue borderValue)
		{
			WorksheetCellOwnedFormatProxy cellOwnedProxy = sender as WorksheetCellOwnedFormatProxy;
			if (cellOwnedProxy != null)
			{
				WorksheetCellFormatData adjacentCellFormat = null;

				switch (borderValue)
				{
					case CellFormatValue.TopBorderColorInfo:
					case CellFormatValue.TopBorderStyle:
						{
							if (this.Index == 0)
								return null;

							WorksheetRow row = this.Worksheet.Rows.GetIfCreated(this.Index - 1);
							adjacentCellFormat = this.Worksheet.GetCellFormatElementReadOnly(row, cellOwnedProxy.ColumnIndex, borderValue);
							break;
						}

					case CellFormatValue.BottomBorderColorInfo:
					case CellFormatValue.BottomBorderStyle:
						{
							if (this.Index == this.Worksheet.Rows.MaxCount - 1)
								return null;

							WorksheetRow row = this.Worksheet.Rows.GetIfCreated(this.Index + 1);
							adjacentCellFormat = this.Worksheet.GetCellFormatElementReadOnly(row, cellOwnedProxy.ColumnIndex, borderValue);
							break;
						}

					case CellFormatValue.LeftBorderColorInfo:
					case CellFormatValue.LeftBorderStyle:
						if (cellOwnedProxy.ColumnIndex == 0)
							return null;

						adjacentCellFormat = this.Worksheet.GetCellFormatElementReadOnly(this, (short)(cellOwnedProxy.ColumnIndex - 1), borderValue);
						break;

					case CellFormatValue.RightBorderColorInfo:
					case CellFormatValue.RightBorderStyle:
						if (cellOwnedProxy.ColumnIndex == this.Worksheet.Columns.MaxCount - 1)
							return null;

						adjacentCellFormat = this.Worksheet.GetCellFormatElementReadOnly(this, (short)(cellOwnedProxy.ColumnIndex + 1), borderValue);
						break;

					default:
						Utilities.DebugFail("Unknown edge border value.");
						return null;
				}

				return adjacentCellFormat;
			}
			else
			{
				switch (borderValue)
				{
					case CellFormatValue.TopBorderColorInfo:
					case CellFormatValue.TopBorderStyle:
						{
							if (this.Index == 0)
								return null;

							WorksheetRow row = this.Worksheet.Rows.GetIfCreated(this.Index - 1);
							if (row == null || row.HasCellFormat == false)
								return null;

							return row.CellFormatInternal.Element;
						}

					case CellFormatValue.BottomBorderColorInfo:
					case CellFormatValue.BottomBorderStyle:
						{
							if (this.Index == this.Worksheet.Rows.MaxCount - 1)
								return null;

							WorksheetRow row = this.Worksheet.Rows.GetIfCreated(this.Index + 1);
							if (row == null || row.HasCellFormat == false)
								return null;

							return row.CellFormatInternal.Element;
						}

					case CellFormatValue.LeftBorderColorInfo:
					case CellFormatValue.LeftBorderStyle:
					case CellFormatValue.RightBorderColorInfo:
					case CellFormatValue.RightBorderStyle:
						return null;

					default:
						Utilities.DebugFail("Unknown edge border value.");
						return null;
				}
			}
		}

		#endregion // GetAdjacentFormatForBorderResolution

		// MD 3/5/10 - TFS26342
		#region GetExtentInTwips



#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

		internal override int GetExtentInTwips(bool ignoreHidden)
		{
			// MD 2/27/12
			// Found while implementing 12.1 - Table Support
			//return this.Worksheet.GetRowHeightInTwips(this.Index, ignoreHidden);
			return this.Worksheet.GetRowHeightInTwips(this, ignoreHidden);
		}

		#endregion // GetExtentInTwips

		#region HasDataIgnoreHidden







		internal override bool HasDataIgnoreHidden
		{
			get
			{
				// If the base has data, the row has data
				if ( base.HasDataIgnoreHidden )
					return true;

				// If the height of the row is non-default, the row has data
				if ( this.height >= 0 && this.height != this.Worksheet.DefaultRowHeight )
					return true;

				// If the row has a collapse indicator, the row has data
				// MD 7/26/10 - TFS34398
				// This can't eb checked here anymore, so the caller will check it.
				//if ( this.hasCollapseIndicator )
				//    return true;

				return false;
			}
		}

		#endregion HasDataIgnoreHidden

		// MD 3/15/12 - TFS104581
		#region HiddenInternal

		internal override bool HiddenInternal
		{
			get { return this.hidden; }
			set { this.hidden = value; }
		}

		#endregion // HiddenInternal

		// MD 3/15/12 - TFS104581
		#region Index

		/// <summary>
		/// Gets the 0-based index of the row in the worksheet.
		/// </summary>
		/// <value>The 0-based index of the row in the worksheet.</value>
		public override int Index
		{
			get { return this.index; }
		}

		#endregion // Index

		#region OnCellFormatValueChanged

		// MD 10/21/10 - TFS34398
		// We need to pass along options to the handlers of the cell format value change.
		//internal override void OnCellFormatValueChanged(CellFormatValue value)
		// MD 4/12/11 - TFS67084
		// We need to pass along the sender now because some object own multiple cell formats.
		//internal override void OnCellFormatValueChanged(CellFormatValue value, CellFormatValueChangedOptions options)
		// MD 4/18/11 - TFS62026
		// The proxy will now pass along all values that changed in one operation as opposed to one at a time.
		//internal override void OnCellFormatValueChanged(WorksheetCellFormatProxy sender, CellFormatValue value, CellFormatValueChangedOptions options)
		internal override void OnCellFormatValueChanged(WorksheetCellFormatProxy sender, IList<CellFormatValue> values, CellFormatValueChangedOptions options)
		{
			// MD 4/12/11 - TFS67084
			// If the cell format is for a cell, call off to OnCellOwnedFormatValueChanged.
			WorksheetCellOwnedFormatProxy cellOwnedProxy = sender as WorksheetCellOwnedFormatProxy;
			if (cellOwnedProxy != null)
			{
				this.OnCellOwnedFormatValueChanged(cellOwnedProxy, values, options);
				return;
			}

			// MD 1/18/11 - TFS62762
			// If one of the format values affecting the height of cells with wrapped text has changed, 
			// we should reset the calculated height of the row, because it might change.
			// MD 4/18/11 - TFS62026
			// The proxy will now pass along all values that changed in one operation as opposed to one at a time.
			//if (WorksheetRow.DoesAffectCachedHeightWithWrappedText(value))
			if (WorksheetRow.DoesAffectCachedHeightWithWrappedText(values))
				this.ResetCachedHeightWithWrappedText();

			// MD 4/18/11 - TFS62026
			// Refactored to deal with multiple values.
			#region Refactored

			//// If the value was change to the default, we don't have to push anything down to the cells.
			//if (this.CellFormatInternal.IsValueDefault(value))
			//    return;

			//switch (value)
			//{
			//    case CellFormatValue.BottomBorderColor:
			//    case CellFormatValue.BottomBorderStyle:
			//        WorksheetRow nextRow = (this.Index + 1) < Workbook.GetMaxRowCount(this.Worksheet.CurrentFormat)
			//            ? this.Worksheet.Rows.GetIfCreated(this.Index + 1)
			//            : null;

			//        // MD 10/21/10 - TFS34398
			//        // If we should prevent syncing the borders of adjacent cells, skip the next block of code and go directly to the code which copies all other cell values.
			//        if ((options & CellFormatValueChangedOptions.PreventAdjacentBorderSyncronization) != 0)
			//            goto default;

			//        // Reset properties for the border of adjacent cells which have the border common with this row.
			//        if (nextRow != null)
			//        {
			//            // MD 4/12/11 - TFS67084
			//            // Moved away from using WorksheetCell objects.
			//            //foreach (WorksheetCell cell in nextRow.Cells)
			//            //{
			//            //    if (cell.HasCellFormat)
			//            //        cell.CellFormatInternal.ResetValue(Utilities.GetOppositeBorderValue(value));
			//            //}
			//            foreach (short columnIndex in nextRow.GetColumnIndexesWithCellFormats())
			//                nextRow.GetCellFormatInternal(columnIndex).ResetValue(Utilities.GetOppositeBorderValue(value));
			//        }

			//        // Copy top and bottom border values to the cells.
			//        goto default;

			//    case CellFormatValue.TopBorderColor:
			//    case CellFormatValue.TopBorderStyle:
			//        WorksheetRow previousRow = this.Index > 0
			//            ? this.Worksheet.Rows.GetIfCreated(this.Index - 1)
			//            : null;

			//        // MD 10/21/10 - TFS34398
			//        // If we should prevent syncing the borders of adjacent cells, skip the next block of code and go directly to the code which copies all other cell values.
			//        if ((options & CellFormatValueChangedOptions.PreventAdjacentBorderSyncronization) != 0)
			//            goto default;

			//        // Reset properties for the border of adjacent cells which have the border common with this row.
			//        if (previousRow != null)
			//        {
			//            // MD 4/12/11 - TFS67084
			//            // Moved away from using WorksheetCell objects.
			//            //foreach (WorksheetCell cell in previousRow.Cells)
			//            //{
			//            //    if (cell.HasCellFormat)
			//            //        cell.CellFormatInternal.ResetValue(Utilities.GetOppositeBorderValue(value));
			//            //}
			//            foreach (short columnIndex in previousRow.GetColumnIndexesWithCellFormats())
			//                previousRow.GetCellFormatInternal(columnIndex).ResetValue(Utilities.GetOppositeBorderValue(value));
			//        }

			//        // Copy top and bottom border values to the cells.
			//        goto default;

			//    // Left and right borders are not used for rows, so ignore them.
			//    case CellFormatValue.LeftBorderColor:
			//    case CellFormatValue.LeftBorderStyle:
			//    case CellFormatValue.RightBorderColor:
			//    case CellFormatValue.RightBorderStyle:
			//        break;

			//    default:
			//        // Copy all other values to the cells.
			//        // MD 4/12/11 - TFS67084
			//        // Moved away from using WorksheetCell objects.
			//        //foreach (WorksheetCell cell in this.Cells)
			//        //{
			//        //    if (cell.HasCellFormat)
			//        //        Utilities.CopyCellFormatValue(this.CellFormatInternal, cell.CellFormatInternal, value);
			//        //}
			//        foreach (short columnIndex in this.GetColumnIndexesWithCellFormats())
			//            Utilities.CopyCellFormatValue(this.CellFormatInternal, this.GetCellFormatInternal(columnIndex), value);
			//        break;
			//} 

			#endregion // Refactored
			for (int i = 0; i < values.Count; i++)
			{
				CellFormatValue value = values[i];

				// If the value was change to the default, we don't have to push anything down to the cells.
				if (this.CellFormatInternal.IsValueDefault(value))
					continue;

				// MD 9/28/11 - TFS88683
				// Cache this because we need it in almost all cases.
				object formatValue = sender.GetValue(value);

				switch (value)
				{
					// MD 3/1/12 - 12.1 - Table Support
					#region Refactored

					//case CellFormatValue.BottomBorderColorInfo:
					//case CellFormatValue.BottomBorderStyle:
					//    // MD 9/28/11 - TFS88683
					//    // Moved this below. We don't need to get the next row if we are going to bail out.
					//    //WorksheetRow nextRow = (this.Index + 1) < Workbook.GetMaxRowCount(this.Worksheet.CurrentFormat)
					//    //    ? this.Worksheet.Rows.GetIfCreated(this.Index + 1)
					//    //    : null;

					//    // MD 10/21/10 - TFS34398
					//    // If we should prevent syncing the borders of adjacent cells, skip the next block of code and go directly to the code which copies all other cell values.
					//    if ((options & CellFormatValueChangedOptions.PreventAdjacentBorderSyncronization) != 0)
					//        goto default;

					//    // MD 9/28/11 - TFS88683
					//    // Moved from above. We don't need to get the next row if we are going to bail out.
					//    WorksheetRow nextRow = (this.Index + 1) < Workbook.GetMaxRowCount(this.Worksheet.CurrentFormat)
					//        ? this.Worksheet.Rows.GetIfCreated(this.Index + 1)
					//        : null;

					//    // Reset properties for the border of adjacent cells which have the border common with this row.
					//    if (nextRow != null)
					//    {
					//        // MD 9/28/11 - TFS88683
					//        // Cache this so we don't need to get it multiple times below.
					//        CellFormatValue oppositeValue = Utilities.GetOppositeBorderValue(value);

					//        // MD 9/28/11 - TFS88683
					//        // We need to sync up overlapping borders on neighboring rows.
					//        if (nextRow.HasCellFormat)
					//            nextRow.CellFormatInternal.SetValue(oppositeValue, formatValue);

					//        // MD 4/12/11 - TFS67084
					//        // Moved away from using WorksheetCell objects.
					//        //foreach (WorksheetCell cell in nextRow.Cells)
					//        //{
					//        //    if (cell.HasCellFormat)
					//        //        cell.CellFormatInternal.ResetValue(Utilities.GetOppositeBorderValue(value));
					//        //}
					//        foreach (CellFormatContext cellFormatContext in nextRow.GetCellFormatsOnCells())
					//        {
					//            // MD 9/28/11 - TFS88683
					//            // Instead of resetting overlapping borders, we will now keep them synced.
					//            //cellFormatContext.GetProxy(nextRow).ResetValue(Utilities.GetOppositeBorderValue(value));
					//            cellFormatContext.GetProxy(nextRow).SetValue(oppositeValue, formatValue);
					//        }
					//    }

					//    // Copy top and bottom border values to the cells.
					//    goto default;

					//case CellFormatValue.TopBorderColorInfo:
					//case CellFormatValue.TopBorderStyle:
					//    // MD 9/28/11 - TFS88683
					//    // Moved this below. We don't need to get the previous row if we are going to bail out.
					//    //WorksheetRow previousRow = this.Index > 0
					//    //    ? this.Worksheet.Rows.GetIfCreated(this.Index - 1)
					//    //    : null;

					//    // MD 10/21/10 - TFS34398
					//    // If we should prevent syncing the borders of adjacent cells, skip the next block of code and go directly to the code which copies all other cell values.
					//    if ((options & CellFormatValueChangedOptions.PreventAdjacentBorderSyncronization) != 0)
					//        goto default;

					//    // MD 9/28/11 - TFS88683
					//    // Moved from above. We don't need to get the previous row if we are going to bail out.
					//    WorksheetRow previousRow = this.Index > 0
					//        ? this.Worksheet.Rows.GetIfCreated(this.Index - 1)
					//        : null;

					//    // Reset properties for the border of adjacent cells which have the border common with this row.
					//    if (previousRow != null)
					//    {
					//        // MD 9/28/11 - TFS88683
					//        // Cache this so we don't need to get it multiple times below.
					//        CellFormatValue oppositeValue = Utilities.GetOppositeBorderValue(value);

					//        // MD 9/28/11 - TFS88683
					//        // We need to sync up overlapping borders on neighboring rows.
					//        if (previousRow.HasCellFormat)
					//            previousRow.CellFormatInternal.SetValue(oppositeValue, formatValue);

					//        // MD 4/12/11 - TFS67084
					//        // Moved away from using WorksheetCell objects.
					//        //foreach (WorksheetCell cell in previousRow.Cells)
					//        //{
					//        //    if (cell.HasCellFormat)
					//        //        cell.CellFormatInternal.ResetValue(Utilities.GetOppositeBorderValue(value));
					//        //}
					//        foreach (CellFormatContext cellFormatContext in previousRow.GetCellFormatsOnCells())
					//        {
					//            // MD 9/28/11 - TFS88683
					//            // Instead of resetting overlapping borders, we will now keep them synced.
					//            //cellFormatContext.GetProxy(previousRow).ResetValue(Utilities.GetOppositeBorderValue(value));
					//            cellFormatContext.GetProxy(previousRow).SetValue(oppositeValue, formatValue);
					//        }
					//    }

					//    // Copy top and bottom border values to the cells.
					//    goto default;

					#endregion // Refactored
					case CellFormatValue.BottomBorderColorInfo:
					case CellFormatValue.BottomBorderStyle:
					case CellFormatValue.TopBorderColorInfo:
					case CellFormatValue.TopBorderStyle:
						{
							// If we should prevent syncing the borders of adjacent cells, skip the next block of code and go directly to the code which copies all other cell values.
							if ((options & CellFormatValueChangedOptions.PreventAdjacentBorderSyncronization) != 0)
								goto default;

							int? adjacentRowIndex = null;
							if (value == CellFormatValue.TopBorderColorInfo || value == CellFormatValue.TopBorderStyle)
							{
								if (0 < this.Index)
									adjacentRowIndex = this.Index - 1;
							}
							else
							{
								int maxRowIndex = this.Worksheet.Rows.MaxCount - 1;
								if (this.Index < maxRowIndex)
									adjacentRowIndex = this.Index + 1;
							}

							WorksheetRow adjacentRow = adjacentRowIndex.HasValue
								? this.Worksheet.Rows.GetIfCreated(adjacentRowIndex.Value)
								: null;

							// Copy the borders to adjacent cells which have the border common with this row.
							if (adjacentRow != null)
							{
								// Cache this so we don't need to get it multiple times below.
								CellFormatValue oppositeValue = Utilities.GetOppositeBorderValue(value);

								// We need to sync up overlapping borders on neighboring rows.
								if (adjacentRow.HasCellFormat)
								{
									// MD 3/22/12 - TFS104630
									// We are now going back to resetting overlapping borders instead of syncing them, because that it what 
									// Microsoft seems to do internally based on looking at what is saved in their file formats. But we may also
									// need to take the associated border value from the overlapping cell format. This is all done in a helper
									// method now.
									//adjacentRow.CellFormatInternal.SetValue(oppositeValue, formatValue);
									Utilities.SynchronizeOverlappingBorderProperties(sender, adjacentRow.CellFormatInternal, value, formatValue, options);
								}

								// MD 3/22/12 - TFS104630
								CellFormatValue associatedOppositeValue = Utilities.GetAssociatedBorderValue(oppositeValue);

								foreach (CellFormatContext cellFormatContext in
									adjacentRow.GetCellFormatsForRowFormatValueSynchronization(oppositeValue))
								{
									// MD 3/22/12 - TFS104630
									// We are now going back to resetting overlapping borders instead of syncing them, but we also need to reset
									// the associated property. And we should only reset them when the existing property doesn't match.
									//cellFormatContext.GetProxy(adjacentRow).SetValue(oppositeValue, formatValue);
									WorksheetCellFormatProxy cellFormat = cellFormatContext.GetProxy(adjacentRow);
									if (cellFormat.GetValue(oppositeValue) != formatValue)
									{
										cellFormat.ResetValue(oppositeValue, CellFormatValueChangedOptions.PreventAdjacentBorderSyncronization);
										cellFormat.ResetValue(associatedOppositeValue, CellFormatValueChangedOptions.PreventAdjacentBorderSyncronization);
									}
								}
							}

							// Copy top and bottom border values to the cells.
							goto default;
						}

					// Left and right borders are not used for rows, so ignore them.
					case CellFormatValue.LeftBorderColorInfo:
					case CellFormatValue.LeftBorderStyle:
					case CellFormatValue.RightBorderColorInfo:
					case CellFormatValue.RightBorderStyle:
						break;

					default:
						{
						// Copy all other values to the cells.
						// MD 4/12/11 - TFS67084
						// Moved away from using WorksheetCell objects.
						//foreach (WorksheetCell cell in this.Cells)
						//{
						//    if (cell.HasCellFormat)
						//        Utilities.CopyCellFormatValue(this.CellFormatInternal, cell.CellFormatInternal, value);
						//}
						// MD 10/26/11 - TFS91546
						// We have to do some special merging logic for the diagonals.
						//foreach (CellFormatContext cellFormatContext in this.GetCellFormatsOnCells())
						//{
						//    // MD 9/28/11 - TFS88683
						//    // Use the cached format value so we don't need to get it in the CopyCellFormatValue method.
						//    //Utilities.CopyCellFormatValue(this.CellFormatInternal, cellFormatContext.GetProxy(this), value);
						//    cellFormatContext.GetProxy(this).SetValue(value, formatValue);
						//}
						#region Determine whether to remove diagonals
      
						bool removeDiagonalDown = false;
						bool removeDiagonalUp = false;
						DiagonalBorders newDiagonalBorders = DiagonalBorders.None;
						if (value == CellFormatValue.DiagonalBorders)
						{
							newDiagonalBorders = (DiagonalBorders)formatValue;
							DiagonalBorders previousDiagonalBorders = sender.Element.PreviousDiagonalBorders;

							// MD 12/22/11 - 12.1 - Table Support
							// There is now a bit for when any value is set, so we can't check for zero anymore.
							//removeDiagonalDown =
							//    (newDiagonalBorders & DiagonalBorders.DiagonalDown) == 0 &&
							//    (previousDiagonalBorders & DiagonalBorders.DiagonalDown) != 0;
							//
							//removeDiagonalUp =
							//    (newDiagonalBorders & DiagonalBorders.DiagonalUp) == 0 &&
							//    (previousDiagonalBorders & DiagonalBorders.DiagonalUp) != 0;
							removeDiagonalDown =
								Utilities.IsDiagonalDownSet(newDiagonalBorders) == false &&
								Utilities.IsDiagonalDownSet(previousDiagonalBorders);

							removeDiagonalUp =
								Utilities.IsDiagonalUpSet(newDiagonalBorders) == false &&
								Utilities.IsDiagonalUpSet(previousDiagonalBorders);
						}
    
						#endregion  // Determine whether to remove diagonals

						// MD 3/1/12 - 12.1 - Table Support
						//foreach (CellFormatContext cellFormatContext in this.GetCellFormatsOnCells())
						foreach (CellFormatContext cellFormatContext in this.GetCellFormatsForRowFormatValueSynchronization(value))
						{
							// MD 1/8/12 - 12.1 - Cell Format Updates
							// Changes to the row format should only be pushed to the merged cells if they are one row tall.
							WorksheetMergedCellsRegion associatedMergedCellRegion = this.GetCellAssociatedMergedCellsRegionInternal(cellFormatContext.ColumnIndex);
							if (associatedMergedCellRegion != null && associatedMergedCellRegion.Height != 1)
								continue;

							WorksheetCellFormatProxy cellFormat = cellFormatContext.GetProxy(this);

							object resolvedFormatValue = formatValue;
							if (value == CellFormatValue.DiagonalBorders)
							{
								DiagonalBorders resolvedDiagonalBorders = newDiagonalBorders | cellFormat.DiagonalBorders;

								// MD 12/22/11 - 12.1 - Table Support
								// There is now a bit for when any value is set, so we can't remove all bits from each diagonal value.
								//if (removeDiagonalDown)
								//    resolvedDiagonalBorders &= ~DiagonalBorders.DiagonalDown;
								//
								//if (removeDiagonalUp)
								//    resolvedDiagonalBorders &= ~DiagonalBorders.DiagonalUp;
								if (removeDiagonalDown)
									Utilities.RemoveDiagonalDownBit(ref resolvedDiagonalBorders);

								if (removeDiagonalUp)
									Utilities.RemoveDiagonalUpBit(ref resolvedDiagonalBorders);

								resolvedFormatValue = resolvedDiagonalBorders;
							}

							cellFormat.SetValue(value, resolvedFormatValue);
						}
						}
						break;
				}
			}
		} 

		#endregion // OnCellFormatValueChanged

		// MD 7/23/10 - TFS35969
		#region OnHiddenChanged

		internal override void OnHiddenChanged()
		{
			base.OnHiddenChanged();
			this.Worksheet.Rows.ResetHeightCache(this.Index, true);
		} 

		#endregion // OnHiddenChanged

		// MD 3/15/12 - TFS104581
		#region OutlineLevelInternal

		internal override byte OutlineLevelInternal
		{
			get { return this.outlineLevel; }
			set { this.outlineLevel = value; }
		}

		#endregion // OutlineLevelInternal

		// MD 7/2/08 - Excel 2007 Format
		#region VerifyFormatLimits

		internal override void VerifyFormatLimits( FormatLimitErrors limitErrors, WorkbookFormat testFormat )
		{
			base.VerifyFormatLimits( limitErrors, testFormat );

			Workbook workbook = this.Worksheet.Workbook;

			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
			//if ( this.HasCells )
			//    this.Cells.VerifyFormatLimits( limitErrors, testFormat );
			foreach (CellDataContext cellDataContext in this.GetCellsWithData())
			{
				// MD 3/2/12 - 12.1 - Table Support
				//this.VerifyFormatLimitsOnCell(cellDataContext.ColumnIndex, limitErrors, testFormat);
				this.VerifyFormatLimitsOnCell(workbook, cellDataContext, limitErrors, testFormat);
			}

			int maxRowIndex = Workbook.GetMaxRowCount( testFormat ) - 1;

			if ( maxRowIndex < this.Index )
                limitErrors.AddError(String.Format(SR.GetString("LE_FormatLimitError_MaxRowIndex"), this.Index, maxRowIndex));
		} 

		#endregion VerifyFormatLimits

		#endregion Base Class Overrides

		#region Methods

		#region Public Methods

		// MD 4/12/11 - TFS67084
		#region ApplyCellFormula

		/// <summary>
		/// Applies a formula to the cell at the specified column index.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// <paramref name="formula"/> is parsed based on the <see cref="CellReferenceMode"/> of the <see cref="Workbook"/>
		/// to which the row belongs. If the row's <see cref="Worksheet"/> has been removed from its parent collection,
		/// the A1 CellReferenceMode will be used to parse the formula.
		/// </p>
		/// </remarks>
		/// <param name="columnIndex">The 0-based index of the cell within the <see cref="WorksheetRow"/>.</param>
		/// <param name="formula">The formula to parse and apply to the cell.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="columnIndex"/> is less than zero or greater than or equal to the number of columns in the worksheet.
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="formula"/> is null or empty.
		/// </exception>
		/// <exception cref="FormulaParseException">
		/// <paramref name="formula"/> is not a valid formula.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// The cell is part of an array formula or data table which is not confined to just the cell.
		/// </exception>
		/// <seealso cref="GetCellFormula"/>
		/// <seealso cref="WorksheetCell.ApplyFormula"/>
		public void ApplyCellFormula(int columnIndex, string formula)
		{
			Utilities.VerifyColumnIndex(this.Worksheet, columnIndex, "columnIndex");

			short columnIndexValue = (short)columnIndex;

			Workbook workbook = this.Worksheet.Workbook;
			bool addToParsedR1C1Formulas = false;

			if (workbook != null && workbook.CellReferenceMode == CellReferenceMode.R1C1)
			{
				Formula existingFormula;
				if (workbook.ParsedR1C1Formulas.TryGetValue(formula, out existingFormula))
				{
					existingFormula.ApplyTo(this, columnIndexValue);
					return;
				}

				addToParsedR1C1Formulas = true;
			}

			Formula parsedFormula = Formula.Parse(formula, workbook);

			if (addToParsedR1C1Formulas)
				workbook.ParsedR1C1Formulas.Add(formula, parsedFormula);

			parsedFormula.ApplyTo(this, columnIndexValue);
		}

		#endregion  // ApplyCellFormula

		// MD 4/12/11 - TFS67084
		#region GetCellAssociatedDataTable

		/// <summary>
		/// Gets the data table to which the cell at the specified index belongs.
		/// </summary>
		/// <param name="columnIndex">The 0-based index of the cell within the <see cref="WorksheetRow"/>.</param>
		/// <remarks>
		/// <p class="body">
		/// The cells in the left-most column and top-most row of the data table will return null for the associated data table.
		/// </p>
		/// <p class="body">
		/// If a data table is associated with the cell, getting the value of the cell with <see cref="GetCellValue(int)"/> 
		/// will return the calculated value for the cell.
		/// </p>
		/// </remarks>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="columnIndex"/> is less than zero or greater than or equal to the number of columns in the worksheet.
		/// </exception>
		/// <returns>The data table to which the cell belongs or null if the cell does not belong to a data table.</returns>
		/// <seealso cref="Excel.Worksheet.DataTables"/>
		/// <seealso cref="WorksheetDataTableCollection.Add"/>
		/// <seealso cref="GetCellValue(int)"/>
		/// <seealso cref="WorksheetCell.AssociatedDataTable"/>
		public WorksheetDataTable GetCellAssociatedDataTable(int columnIndex)
		{
			Utilities.VerifyColumnIndex(this.Worksheet, columnIndex, "columnIndex");
			return this.GetCellValueRaw((short)columnIndex) as WorksheetDataTable;
		}

		#endregion  // GetCellAssociatedDataTable

		// MD 4/12/11 - TFS67084
		#region GetCellAssociatedMergedCellsRegion

		/// <summary>
		/// Gets the merged cells region which contains the cell at the specified index, or null if the cell is not merged.
		/// </summary>
		/// <param name="columnIndex">The 0-based index of the cell within the <see cref="WorksheetRow"/>.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="columnIndex"/> is less than zero or greater than or equal to the number of columns in the worksheet.
		/// </exception>
		/// <returns>The merged cells region which contains the cell at the specified index, or null if the cell is not merged.</returns>
		/// <seealso cref="WorksheetCell.AssociatedMergedCellsRegion"/>
		public WorksheetMergedCellsRegion GetCellAssociatedMergedCellsRegion(int columnIndex)
		{
			Utilities.VerifyColumnIndex(this.Worksheet, columnIndex, "columnIndex");
			return this.GetCellAssociatedMergedCellsRegionInternal((short)columnIndex);
		}

		internal WorksheetMergedCellsRegion GetCellAssociatedMergedCellsRegionInternal(short columnIndex)
		{
			if (this.Worksheet.HasCellOwnedAssociatedMergedCellsRegions == false)
				return null;

			// MD 3/27/12 - 12.1 - Table Support
			//WorksheetCellAddress address = new WorksheetCellAddress(this, columnIndex);
			WorksheetCellAddress address = new WorksheetCellAddress(this.Index, columnIndex);

			WorksheetMergedCellsRegion associatedMergedCellsRegion;
			if (this.Worksheet.CellOwnedAssociatedMergedCellsRegions.TryGetValue(address, out associatedMergedCellsRegion))
				return associatedMergedCellsRegion;

			return null;
		}

		#endregion  // GetCellAssociatedMergedCellsRegion

		// MD 12/7/11 - 12.1 - Table Support
		#region GetCellAssociatedTable

		/// <summary>
		/// Gets the <see cref="WorksheetTable"/> to which the cell at the specified index belongs.
		/// </summary>
		/// <param name="columnIndex">The 0-based index of the cell within the <see cref="WorksheetRow"/>.</param>
		/// <remarks>
		/// <p class="body">
		/// A cell belongs to a table if it exists in any area of the table. It can be a header cell, total cell, or a cell in the data area.
		/// </p>
		/// </remarks>
		/// <seealso cref="WorksheetTable"/>
		/// <seealso cref="Excel.Worksheet.Tables"/>

		[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelTables)]

		public WorksheetTable GetCellAssociatedTable(int columnIndex)
		{
			Utilities.VerifyColumnIndex(this.Worksheet, columnIndex, "columnIndex");
			return this.GetCellAssociatedTableInternal((short)columnIndex);
		}

		internal WorksheetTable GetCellAssociatedTableInternal(short columnIndex)
		{
			
			WorksheetTableCollection tables = this.Worksheet.Tables;
			for (int i = 0; i < tables.Count; i++)
			{
				WorksheetTable table = tables[i];
				Debug.Assert(table.Worksheet == this.Worksheet, "The table does not belong to the Worksheet of the row.");

				if (table.Contains(this, columnIndex))
					return table;
			}

			return null;
		}

		#endregion // GetCellAssociatedTable

		// MD 4/12/11 - TFS67084
		#region GetCellBoundsInTwips

		/// <summary>
		/// Gets the bounds of the cell at the specified column index in twips (1/20th of a point).
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The bounds returned by this method are only valid with the current configuration of the worksheet.
		/// If any rows or columns before the cell are resized, these bounds will no longer reflect the 
		/// position of the cell.
		/// </p>
		/// </remarks>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="columnIndex"/> is less than zero or greater than or equal to the number of columns in the worksheet.
		/// </exception>
		/// <returns>The bounds of the cell at the specified column index on its worksheet.</returns>
		/// <seealso cref="WorksheetCell.GetBoundsInTwips()"/>
		public Rectangle GetCellBoundsInTwips(int columnIndex)
		{
			Utilities.VerifyColumnIndex(this.Worksheet, columnIndex, "columnIndex");
			return this.GetCellBoundsInTwipsInternal((short)columnIndex);
		}

		internal Rectangle GetCellBoundsInTwipsInternal(short columnIndex)
		{
			return this.GetCellBoundsInTwipsInternal(columnIndex, PositioningOptions.None);
		}

		/// <summary>
		/// Gets the bounds of the cell at the specified column index in twips (1/20th of a point).
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The bounds returned by this method are only valid with the current configuration of the worksheet.
		/// If any rows or columns before the cell are resized, these bounds will no longer reflect the 
		/// position of the cell.
		/// </p>
		/// </remarks>
		/// <param name="columnIndex">The 0-based index of the cell within the <see cref="WorksheetRow"/>.</param>
		/// <param name="options">The options to use when getting the bounds of the cell.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="columnIndex"/> is less than zero or greater than or equal to the number of columns in the worksheet.
		/// </exception>
		/// <returns>The bounds of the cell at the specified column index on its worksheet.</returns>
		/// <seealso cref="WorksheetCell.GetBoundsInTwips(PositioningOptions)"/>
		public Rectangle GetCellBoundsInTwips(int columnIndex, PositioningOptions options)
		{
			Utilities.VerifyColumnIndex(this.Worksheet, columnIndex, "columnIndex");
			return this.GetCellBoundsInTwipsInternal((short)columnIndex, options);
		}

		internal Rectangle GetCellBoundsInTwipsInternal(short columnIndex, PositioningOptions options)
		{
			// MD 3/27/12 - 12.1 - Table Support
			//return WorksheetShape.GetBoundsInTwips(
			//    this, columnIndex, Utilities.PointFEmpty,
			//    this, columnIndex, new PointF(100, 100), options);
			return WorksheetShape.GetBoundsInTwips(this.Worksheet,
				this.Index, columnIndex, Utilities.PointFEmpty,
				this.Index, columnIndex, new PointF(100, 100), options);
		}

		#endregion GetCellBoundsInTwips

		// MD 4/12/11 - TFS67084
		#region GetCellComment

		/// <summary>
		/// Gets or sets the comment applied to the cell at the specified column index.
		/// </summary>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="columnIndex"/> is less than zero or greater than or equal to the number of columns in the worksheet.
		/// </exception>
		/// <returns>The comment applied to the cell.</returns>
		/// <seealso cref="WorksheetCell.Comment"/>
		public WorksheetCellComment GetCellComment(int columnIndex)
		{
			Utilities.VerifyColumnIndex(this.Worksheet, columnIndex, "columnIndex");
			return this.GetCellCommentInternal((short)columnIndex);
		}

		internal WorksheetCellComment GetCellCommentInternal(short columnIndex)
		{
			if (this.Worksheet.HasCellOwnedComments == false)
				return null;

			// MD 3/27/12 - 12.1 - Table Support
			//WorksheetCellAddress cellAddress = new WorksheetCellAddress(this, (short)columnIndex);
			WorksheetCellAddress cellAddress = new WorksheetCellAddress(this.Index, (short)columnIndex);

			WorksheetCellComment comment;
			if (this.Worksheet.CellOwnedComments.TryGetValue(cellAddress, out comment))
				return comment;

			return null;
		}

		#endregion // GetCellComment

		// MD 12/19/11 - 12.1 - Table Support
		#region GetCellDisplayText

		/// <summary>
		/// Gets the display text in the cell at the specified index.
		/// </summary>
		/// <param name="columnIndex">The 0-based index of the cell within the <see cref="WorksheetRow"/>.</param>
		/// <remarks>
		/// <p class="body">
		/// The display text is based on the value of the cell and the format string applied to the cell.
		/// </p>
		/// </remarks>
		/// <seealso cref="GetCellValue(int)"/>
		/// <seealso cref="IWorksheetCellFormat.FormatString"/>

		[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelTables)]

		public string GetCellText(int columnIndex)
		{
			return this.GetCellText(columnIndex, TextFormatMode.AsDisplayed);
		}

		/// <summary>
		/// Gets the text in the cell at the specified index.
		/// </summary>
		/// <param name="columnIndex">The 0-based index of the cell within the <see cref="WorksheetRow"/>.</param>
		/// <param name="textFormatMode">The format mode to use when getting the cell text.</param>
		/// <remarks>
		/// <p class="body">
		/// The text is based on the value of the cell and the format string applied to the cell.
		/// </p>
		/// </remarks>
		/// <exception cref="InvalidEnumArgumentException">
		/// <paramref name="textFormatMode"/> is not defined in the <see cref="TextFormatMode"/> enumeration.
		/// </exception>
		/// <seealso cref="GetCellValue(int)"/>
		/// <seealso cref="IWorksheetCellFormat.FormatString"/>

		[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelTables)]

		public string GetCellText(int columnIndex, TextFormatMode textFormatMode)
		{
			Utilities.VerifyColumnIndex(this.Worksheet, columnIndex, "columnIndex");
			return this.GetCellTextInternal((short)columnIndex, textFormatMode);
		}

		internal string GetCellTextInternal(short columnIndex, TextFormatMode textFormatMode)
		{
			GetCellTextParameters parameters = new GetCellTextParameters(columnIndex);
			parameters.TextFormatMode = textFormatMode;
			parameters.PreventTextFormattingTypes = PreventTextFormattingTypes.None;
			return this.GetCellTextInternal(parameters);
		}

		internal string GetCellTextInternal(GetCellTextParameters parameters)
		{
			WorksheetCellBlock cellBlock;
			this.TryGetCellBlock(parameters.ColumnIndex, out cellBlock);
			return this.GetCellTextInternal(cellBlock, parameters);
		}

		internal string GetCellTextInternal(WorksheetCellBlock cellBlock, GetCellTextParameters parameters)
		{
			double? numericValue;
			ValueFormatter.SectionType formattedAs;
			return this.GetCellTextInternal(cellBlock, parameters, out numericValue, out formattedAs);
		}

		internal string GetCellTextInternal(GetCellTextParameters parameters, out double? numericValue, out ValueFormatter.SectionType formattedAs)
		{
			WorksheetCellBlock cellBlock;
			this.TryGetCellBlock(parameters.ColumnIndex, out cellBlock);
			return this.GetCellTextInternal(cellBlock, parameters, out numericValue, out formattedAs);
		}

		internal string GetCellTextInternal(WorksheetCellBlock cellBlock, GetCellTextParameters parameters, out double? numericValue, out ValueFormatter.SectionType formattedAs)
		{
			Utilities.VerifyEnumValue(parameters.TextFormatMode);

			if (cellBlock == null)
			{
				numericValue = null;
				formattedAs = ValueFormatter.SectionType.Text;
				return string.Empty;
			}

			if (parameters.UseCalculatedValues.HasValue == false)
				parameters.UseCalculatedValues = (this.Worksheet.DisplayOptions.ShowFormulasInCells == false);

			return cellBlock.GetCellTextInternal(this, parameters, out numericValue, out formattedAs);
		}

		#endregion // GetCellDisplayText

		// MD 4/12/11 - TFS67084
		#region GetCellFormat

		/// <summary>
		/// Gets the cell formatting for the cell at the specified column index.
		/// </summary>
		/// <param name="columnIndex">The 0-based index of the cell within the <see cref="WorksheetRow"/>.</param>
		/// <remarks>
		/// <p class="body">
		/// Use this method to set cell formatting specific to the cell. If you will be applying the format to numerous cells, 
		/// see the <see cref="Workbook.CreateNewWorksheetCellFormat"/> method for performance considerations.
		/// </p>
		/// </remarks>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="columnIndex"/> is less than zero or greater than or equal to the number of columns in the worksheet.
		/// </exception>
		/// <returns>The cell formatting for the cell at the specified column index.</returns>
		/// <seealso cref="GetResolvedCellFormat"/>
		/// <seealso cref="WorksheetCell.CellFormat"/>
		public IWorksheetCellFormat GetCellFormat(int columnIndex)
		{
			Utilities.VerifyColumnIndex(this.Worksheet, columnIndex, "columnIndex");
			return this.GetCellFormatInternal((short)columnIndex);
		}

		#endregion  // GetCellFormat

		// MD 4/12/11 - TFS67084
		#region GetCellFormula

		/// <summary>
		/// Gets the formula which has been applied to the cell at the specified column index.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// If a formula has been applied to the cell, getting the value with the <see cref="GetCellValue(int)"/> method will return the 
		/// calculated value of the formula.
		/// </p>
		/// </remarks>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="columnIndex"/> is less than zero or greater than or equal to the number of columns in the worksheet.
		/// </exception>
		/// <returns>The formula which has been applied to the cell or null if no formula has been applied.</returns>
		/// <seealso cref="Excel.Formula.ApplyTo(WorksheetCell)"/>
		/// <seealso cref="Excel.Formula.ApplyTo(WorksheetRegion)"/>
		/// <seealso cref="Excel.Formula.ApplyTo(WorksheetRegion[])"/>
		/// <seealso cref="ApplyCellFormula"/>
		/// <seealso cref="WorksheetRegion.ApplyFormula"/>
		/// <seealso cref="WorksheetRegion.ApplyArrayFormula"/>
		/// <seealso cref="WorksheetCell.Formula"/>
		public Formula GetCellFormula(int columnIndex)
		{
			Utilities.VerifyColumnIndex(this.Worksheet, columnIndex, "columnIndex");
			return this.GetCellFormulaInternal((short)columnIndex);
		}

		internal Formula GetCellFormulaInternal(short columnIndex)
		{
			return this.GetCellValueRaw(columnIndex) as Formula;
		} 

		#endregion  // GetCellFormula

		// MD 4/12/11 - TFS67084
		#region GetCellValue

		/// <summary>
		/// Gets the value of the cell at the specified column index.
		/// </summary>
		/// <param name="columnIndex">The 0-based index of the cell within the <see cref="WorksheetRow"/>.</param>
		/// <remarks>
		/// <p class="body">
		/// If this cell belongs to a merged cell region and it is the top-left cell of the region, getting and setting the value 
		/// will get and set the value of the associated merged cell region. Getting the value of other cells in a merged cell region
		/// will always return null. Setting the value of other cells in a merged cell region will have no effect.
		/// </p>
		/// <p class="body">
		/// If a formula has been applied to the cell or a data table is associated with the cell, getting the Value will return the 
		/// calculated value of the cell.
		/// </p>
		/// <p class="body">
		/// The types supported for the value are:
		/// <BR/>
		/// <ul>
		/// <li class="taskitem"><span class="taskitemtext">System.Byte</span></li>
		/// <li class="taskitem"><span class="taskitemtext">System.SByte</span></li>
		/// <li class="taskitem"><span class="taskitemtext">System.Int16</span></li>
		/// <li class="taskitem"><span class="taskitemtext">System.Int64</span></li>
		/// <li class="taskitem"><span class="taskitemtext">System.UInt16</span></li>
		/// <li class="taskitem"><span class="taskitemtext">System.UInt64</span></li>
		/// <li class="taskitem"><span class="taskitemtext">System.UInt32</span></li>
		/// <li class="taskitem"><span class="taskitemtext">System.Int32</span></li>
		/// <li class="taskitem"><span class="taskitemtext">System.Single</span></li>
		/// <li class="taskitem"><span class="taskitemtext">System.Double</span></li>
		/// <li class="taskitem"><span class="taskitemtext">System.Boolean</span></li>
		/// <li class="taskitem"><span class="taskitemtext">System.Char</span></li>
		/// <li class="taskitem"><span class="taskitemtext">System.Enum</span></li>
		/// <li class="taskitem"><span class="taskitemtext">System.Decimal</span></li>
		/// <li class="taskitem"><span class="taskitemtext">System.DateTime</span></li>
		/// <li class="taskitem"><span class="taskitemtext">System.String</span></li>
		/// <li class="taskitem"><span class="taskitemtext">System.Text.StringBuilder</span></li>
		/// <li class="taskitem"><span class="taskitemtext">System.DBNull</span></li>
		/// <li class="taskitem"><span class="taskitemtext"><see cref="ErrorValue"/></span></li>
		/// <li class="taskitem"><span class="taskitemtext"><see cref="FormattedString"/></span></li>
		/// </ul>
		/// </p>
		/// </remarks>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="columnIndex"/> is less than zero or greater than or equal to the number of columns in the worksheet.
		/// </exception>
		/// <value>The value of the cell.</value>
		/// <seealso cref="SetCellValue"/>
		/// <seealso cref="GetCellAssociatedMergedCellsRegion"/>
		/// <seealso cref="WorksheetCell.IsCellTypeSupported"/>
		/// <seealso cref="WorksheetMergedCellsRegion.Value"/>
		/// <seealso cref="GetCellFormula"/>
		/// <seealso cref="GetCellAssociatedDataTable"/>
		/// <seealso cref="WorksheetCell.Value"/>
		public object GetCellValue(int columnIndex)
		{
			Utilities.VerifyColumnIndex(this.Worksheet, columnIndex, "columnIndex");

			// MD 2/27/12 - 12.1 - Table Support
			// Moved code to the new GetCellValueInternal helper method.
			return this.GetCellValueInternal((short)columnIndex);
		}

		// MD 2/27/12 - 12.1 - Table Support
		internal object GetCellValueInternal(short columnIndex)
		{
			return this.GetCellValueHelper(columnIndex, false);
		}

		#endregion  // GetCellValue

		// MD 4/12/11 - TFS67084
		#region GetResolvedCellFormat

		/// <summary>
		/// Gets the resolved cell formatting for the cell at the specified column index.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// If any cell format properties are the default values on the cell, the values from the owning row's cell format will be used.
		/// If those are default, then the values from the owning column's cell format will be used. Otherwise, the workbook default values
		/// will be used.
		/// </p>
		/// </remarks>
		/// <param name="columnIndex">The 0-based index of the cell within the <see cref="WorksheetRow"/>.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="columnIndex"/> is less than zero or greater than or equal to the number of columns in the worksheet.
		/// </exception>
		/// <returns>A format object describing the actual formatting that will be used when displayed the cell in Microsoft Excel.</returns>
		/// <seealso cref="GetCellFormat"/>
		/// <seealso cref="RowColumnBase.CellFormat"/>
		/// <seealso cref="WorksheetCell.GetResolvedCellFormat"/>
		public IWorksheetCellFormat GetResolvedCellFormat(int columnIndex)
		{
			Utilities.VerifyColumnIndex(this.Worksheet, columnIndex, "columnIndex");
			return this.GetResolvedCellFormatInternal((short)columnIndex);
		}

		internal IWorksheetCellFormat GetResolvedCellFormatInternal(short columnIndex)
		{
			// MD 1/8/12 - 12.1 - Cell Format Updates
			// We don't need to do any resolution when getting the resolved format. Return an instance which will do the resolution when asked.
			#region Old Code

			//Worksheet worksheet = this.Worksheet;
			//Workbook workbook = worksheet.Workbook;
			//WorksheetCellFormatData emptyFormatData = (WorksheetCellFormatData)workbook.CellFormats.EmptyElement.Clone();
			//emptyFormatData.IsStyle = false;

			//WorksheetCellFormatData rowFormatData = this.HasCellFormat
			//    ? this.CellFormatInternal.Element
			//    : emptyFormatData;

			//WorksheetCellFormatData cellFormatData;
			//if (this.HasCellFormatForCellResolved(columnIndex, out cellFormatData) == false)
			//    cellFormatData = emptyFormatData;

			//WorksheetCellFormatData resolvedData = this.GetResolvedCellFormatHelper(columnIndex, worksheet, cellFormatData, rowFormatData, emptyFormatData);

			//// We also need to resolve the font data.
			//resolvedData = resolvedData.ResolvedCellFormatData();
			//resolvedData.FontInternal.SetFontFormatting(resolvedData.FontInternal.Element.ResolvedFontData());
			//return resolvedData;

			#endregion // Old Code
			return new WorksheetCellFormatDataResolved(this.GetCellFormatInternal(columnIndex));
		}

		// MD 1/8/12 - 12.1 - Cell Format Updates
		// This is no longer needed due to the changes in GetResolvedCellFormat(short)
		#region Removed

		//internal WorksheetCellFormatData GetResolvedCellFormatHelper(
		//    short columnIndex,
		//    Worksheet worksheet,
		//    WorksheetCellFormatData cellFormatData,
		//    WorksheetCellFormatData rowFormatData,
		//    WorksheetCellFormatData emptyFormatData)
		//{
		//    WorksheetColumn column = worksheet.HasColumns
		//        ? worksheet.Columns.GetIfCreated(columnIndex)
		//        : null;

		//    return WorksheetCellBlock.GetResolvedCellFormatHelper(worksheet, column, cellFormatData, rowFormatData, emptyFormatData);
		//}

		#endregion // Removed

		#endregion // GetResolvedCellFormat

		// MD 4/12/11 - TFS67084
		#region SetCellComment

		/// <summary>
		/// Sets the comment applied to the cell at the specified column index.
		/// </summary>
		/// <param name="columnIndex">The 0-based index of the cell within the <see cref="WorksheetRow"/>.</param>
		/// <param name="comment">The comment to apply to the cell.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="columnIndex"/> is less than zero or greater than or equal to the number of columns in the worksheet.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// The value applied only has only one anchor cell set. It should have both or neither anchor cells set.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// The value has its <see cref="WorksheetShape.TopLeftCornerCell"/> and <see cref="WorksheetShape.BottomRightCornerCell"/> 
		/// anchors set but they are from different worksheets.
		/// </exception>
		/// <seealso cref="WorksheetCell.Comment"/>
		public void SetCellComment(int columnIndex, WorksheetCellComment comment)
		{
			Utilities.VerifyColumnIndex(this.Worksheet, columnIndex, "columnIndex");
			this.SetCellCommentInternal((short)columnIndex, comment);
		}

		internal void SetCellCommentInternal(short columnIndex, WorksheetCellComment comment)
		{
			WorksheetCellComment oldComment = this.GetCellCommentInternal(columnIndex);
			if (oldComment != comment)
			{
				Worksheet worksheet = this.Worksheet;

				// MD 9/2/08 - Cell Comments
				// Validate the comment before applying it to the cell.
				if (comment != null)
				{
					// Initialize default anchor cells if they have not already been set.
					if (comment.TopLeftCornerCell == null && comment.BottomRightCornerCell == null)
						this.InitializeDefaultCommentBounds(columnIndex, comment);

					// Make sure the anchor cells have been set before the comment is added to a collection.
					if (comment.TopLeftCornerCell == null || comment.BottomRightCornerCell == null)
						throw new InvalidOperationException(SR.GetString("LE_InvalidOperationException_AnchorCommentBeforeApplyingToCell"));

					comment.OnSitedOnWorksheet(worksheet);
				}

				// Remove the old comment from the cell.
				if (oldComment != null)
					oldComment.Cell = null;

				// MD 3/27/12 - 12.1 - Table Support
				//WorksheetCellAddress cellAddress = new WorksheetCellAddress(this, columnIndex);
				WorksheetCellAddress cellAddress = new WorksheetCellAddress(this.Index, columnIndex);

				if (comment == null)
				{
					if (worksheet.HasCellOwnedComments)
						worksheet.CellOwnedComments.Remove(cellAddress);
				}
				else
				{
					worksheet.CellOwnedComments[cellAddress] = comment;
				}

				if (comment != null)
				{
					comment.Cell = this.Cells[columnIndex];

					// Initialize the author of the comment.
					if (comment.Author == null)
					{
						Workbook workbook = worksheet.Workbook;

						if (workbook != null && workbook.DocumentProperties.Author != null)
							comment.Author = workbook.DocumentProperties.Author;
						else
							comment.Author = string.Empty;
					}
				}
			}
		}

		#endregion // SetCellComment

		// MD 4/12/11 - TFS67084
		#region SetCellValue

		/// <summary>
		/// Sets the value of a cell at the specified column index.
		/// </summary>
		/// <param name="columnIndex">The 0-based index of the cell within the <see cref="WorksheetRow"/>.</param>
		/// <param name="value">The value to assign to the cell.</param>
		/// <remarks>
		/// <p class="body">
		/// If this cell belongs to a merged cell region and it is the top-left cell of the region, getting and setting the value 
		/// will get and set the value of the associated merged cell region. Getting the value of other cells in a merged cell region
		/// will always return null. Setting the value of other cells in a merged cell region will have no effect.
		/// </p>
		/// <p class="body">
		/// If a formula has been applied to the cell or a data table is associated with the cell, getting the Value will return the 
		/// calculated value of the cell.
		/// </p>
		/// <p class="body">
		/// The types supported for the value are:
		/// <BR/>
		/// <ul>
		/// <li class="taskitem"><span class="taskitemtext">System.Byte</span></li>
		/// <li class="taskitem"><span class="taskitemtext">System.SByte</span></li>
		/// <li class="taskitem"><span class="taskitemtext">System.Int16</span></li>
		/// <li class="taskitem"><span class="taskitemtext">System.Int64</span></li>
		/// <li class="taskitem"><span class="taskitemtext">System.UInt16</span></li>
		/// <li class="taskitem"><span class="taskitemtext">System.UInt64</span></li>
		/// <li class="taskitem"><span class="taskitemtext">System.UInt32</span></li>
		/// <li class="taskitem"><span class="taskitemtext">System.Int32</span></li>
		/// <li class="taskitem"><span class="taskitemtext">System.Single</span></li>
		/// <li class="taskitem"><span class="taskitemtext">System.Double</span></li>
		/// <li class="taskitem"><span class="taskitemtext">System.Boolean</span></li>
		/// <li class="taskitem"><span class="taskitemtext">System.Char</span></li>
		/// <li class="taskitem"><span class="taskitemtext">System.Enum</span></li>
		/// <li class="taskitem"><span class="taskitemtext">System.Decimal</span></li>
		/// <li class="taskitem"><span class="taskitemtext">System.DateTime</span></li>
		/// <li class="taskitem"><span class="taskitemtext">System.String</span></li>
		/// <li class="taskitem"><span class="taskitemtext">System.Text.StringBuilder</span></li>
		/// <li class="taskitem"><span class="taskitemtext">System.DBNull</span></li>
		/// <li class="taskitem"><span class="taskitemtext"><see cref="ErrorValue"/></span></li>
		/// <li class="taskitem"><span class="taskitemtext"><see cref="FormattedString"/></span></li>
		/// </ul>
		/// </p>
		/// </remarks>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="columnIndex"/> is less than zero or greater than or equal to the number of columns in the worksheet.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// The assigned value's type is not supported and can't be exported to Excel.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// The value assigned is a <see cref="Formula"/>. Instead, <see cref="Formula.ApplyTo(WorksheetCell)"/> 
		/// should be called on the Formula, passing in the cell.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// The value assigned is a <see cref="WorksheetDataTable"/>. Instead, the <see cref="WorksheetDataTable.CellsInTable"/>
		/// should be set to a region containing the cell.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// The value assigned is a FormattedString which is the value another cell or merged cell region.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// The value is assigned and this cell is part of an <see cref="ArrayFormula"/> or WorksheetDataTable.
		/// </exception>
		/// <value>The value of the cell.</value>
		/// <seealso cref="GetCellValue(int)"/>
		/// <seealso cref="GetCellAssociatedMergedCellsRegion(int)"/>
		/// <seealso cref="WorksheetCell.IsCellTypeSupported"/>
		/// <seealso cref="WorksheetMergedCellsRegion.Value"/>
		/// <seealso cref="GetCellFormula"/>
		/// <seealso cref="GetCellAssociatedDataTable"/>
		/// <seealso cref="WorksheetCell.Value"/>
		public void SetCellValue(int columnIndex, object value)
		{
			Utilities.VerifyColumnIndex(this.Worksheet, columnIndex, "columnIndex");
			WorksheetCellBlock.VerifyDirectSetValue(value);
			this.SetCellValueRaw((short)columnIndex, value);
		}

		#endregion  // SetCellValue

		#endregion  // Public Methods

		#region Internal Methods

		// MD 1/18/11 - TFS62762
		#region CalculateHeightWithWrappedText

		private int CalculateHeightWithWrappedText()
		{
			// MD 4/12/11
			// Found while fixing TFS67084
			// Cache the workbook.
			//WorkbookFontData defaultFontData = this.Worksheet.Workbook.Fonts.DefaultElement;
			//int defaultFontHeight = this.Worksheet.Workbook.DefaultFontHeight;
			Workbook workbook = this.Worksheet.Workbook;
			WorkbookFontData defaultFontData = workbook.Fonts.DefaultElement;

			// MD 1/8/12 - 12.1 - Cell Format Updates
			// The default font height is exposed off the normal style.
			//int defaultFontHeight = workbook.DefaultFontHeight;
			WorksheetCellFormatData normalFormat = workbook.Styles.NormalStyle.StyleFormatInternal;
			int defaultFontHeight = normalFormat.FontHeightResolved;

			ExcelDefaultableBoolean rowWrapText = ExcelDefaultableBoolean.Default;
			WorkbookFontData rowFontData = defaultFontData;

			if (this.HasCellFormat)
			{
				rowWrapText = this.CellFormatInternal.WrapText;
				rowFontData = this.CellFormatInternal.Element.FontInternal.Element;
			}

			int maxHeight = this.Worksheet.DefaultRowHeight;

			// MD 3/26/12 - 12.1 - Table Support
			// When measuring row height and auto-sizing column widths, they seem to think there is extra width associated 
			// with the cell text, so we will remove some width when measuring below.
			
			int columnWidthCorrection = (int)Math.Floor(this.Worksheet.ZeroCharacterWidth * 0.35);







			using (Bitmap b = new Bitmap(1, 1))
			using (Graphics grfx = Graphics.FromImage(b))

			{
				// MD 4/12/11 - TFS67084
				// Moved away from using WorksheetCell objects.
				//foreach (WorksheetCell cell in this.Cells)
				//{
				foreach (KeyValuePair<short, WorksheetCellBlock> pair in this.GetCellBlocksWithValues())
				{
					// MD 3/26/12 - 12.1 - Table Support
					// Cleaned up this code to use some new code that was written for 12.1
					#region Old Code

					//                    short columnIndex = pair.Key;
					//                    WorksheetCellBlock cellBlock = pair.Value;

					//                    // MD 4/12/11 - TFS67084
					//                    // Moved away from using WorksheetCell objects.
					//                    //object cellValue = cell.Value;
					//                    //
					//                    //if (WorksheetCell.IsCellValueSavedAsString(cellValue) == false)
					//                    //    continue;
					//                    object cellValue;
					//                    if (cellBlock.IsCellValueSavedAsString(this, columnIndex, out cellValue) == false)
					//                        continue;

					//                    // MD 3/15/12 - TFS104581
					//                    //ExcelDefaultableBoolean columnWrapText = ExcelDefaultableBoolean.Default;
					//                    //// MD 4/12/11 - TFS67084
					//                    //// Moved away from using WorksheetCell objects.
					//                    ////WorksheetColumn cellsParentColumn = this.Worksheet.Columns.GetIfCreated(cell.ColumnIndex);
					//                    //WorksheetColumn cellsParentColumn = this.Worksheet.Columns.GetIfCreated(columnIndex);
					//                    //if (cellsParentColumn != null && cellsParentColumn.HasCellFormat)
					//                    //    columnWrapText = cellsParentColumn.CellFormatInternal.WrapText;
					//                    WorksheetColumnBlock cellsParentColumn = this.Worksheet.GetColumnBlock(columnIndex);
					//                    ExcelDefaultableBoolean columnWrapText = cellsParentColumn.CellFormat.WrapText;

					//                    ExcelDefaultableBoolean cellWrapText = ExcelDefaultableBoolean.Default;

					//                    // MD 4/12/11 - TFS67084
					//                    // Moved away from using WorksheetCell objects.
					//                    //if (cell.HasCellFormat)
					//                    //    cellWrapText = cell.CellFormatInternal.WrapText;
					//                    // MD 4/18/11 - TFS62026
					//                    // Since we are not setting any properties, we just need the element, not the proxy, so get that instead.
					//                    //WorksheetCellFormatProxy cellFormatProxy = null;
					//                    //if (this.HasCellFormatForCellResolved(columnIndex, out cellFormatProxy))
					//                    //    cellWrapText = cellFormatProxy.WrapText;
					//                    WorksheetCellFormatData cellFormatData = null;
					//                    // MD 2/25/12 - 12.1 - Table Support
					//                    //if (this.HasCellFormatForCellResolved(columnIndex, out cellFormatData))
					//                    if (this.TryGetCellFormat(columnIndex, out cellFormatData))
					//                        cellWrapText = cellFormatData.WrapText;

					//                    bool resolvedWrapText = false;
					//                    if (cellWrapText != ExcelDefaultableBoolean.Default)
					//                        resolvedWrapText = (cellWrapText == ExcelDefaultableBoolean.True);
					//                    else if (rowWrapText != ExcelDefaultableBoolean.Default)
					//                        resolvedWrapText = (rowWrapText == ExcelDefaultableBoolean.True);
					//                    else if (columnWrapText != ExcelDefaultableBoolean.Default)
					//                        resolvedWrapText = (columnWrapText == ExcelDefaultableBoolean.True);

					//                    if (resolvedWrapText == false)
					//                        continue;

					//                    // TODO: We have to do something different for formatted strings
					//                    string cellText = cellValue.ToString();
					//                    int columnWidth = (int)this.Worksheet.GetColumnWidthInPixels(cellsParentColumn, true);

					//                    // MD 3/15/12 - TFS104581
					//                    //WorkbookFontData columnFontData = defaultFontData;
					//                    //if (cellsParentColumn != null && cellsParentColumn.HasCellFormat)
					//                    //    columnFontData = cellsParentColumn.CellFormatInternal.Element.FontInternal.Element;
					//                    WorkbookFontData columnFontData = cellsParentColumn.CellFormat.FontInternal.Element;

					//                    WorkbookFontData cellFontData = defaultFontData;

					//                    // MD 4/12/11 - TFS67084
					//                    // Moved away from using WorksheetCell objects.
					//                    //if (cell.HasCellFormat)
					//                    //    cellFontData = cell.CellFormatInternal.Element.FontInternal.Element;
					//                    // MD 4/18/11 - TFS62026
					//                    // Since we are not setting any properties, we are just using the element.
					//                    //if (cellFormatProxy != null)
					//                    //    cellFontData = cellFormatProxy.Element.FontInternal.Element;
					//                    if (cellFormatData != null)
					//                        cellFontData = cellFormatData.FontInternal.Element;

					//                    // TODO_12_1: Can this be cleaned up now?
					//                    WorkbookFontData resolvedFontData = WorkbookFontData.Combine(
					//                        cellFontData,
					//                        WorkbookFontData.Combine(rowFontData, columnFontData));

					//                    if (resolvedFontData.Height < 0)
					//                        resolvedFontData.Height = defaultFontHeight;

					//                    // MD 1/8/12 - 12.1 - Cell Format Updates
					//                    // The default font name is exposed off the normal style.
					//                    //if (resolvedFontData.Name == null)
					//                    //    resolvedFontData.Name = Workbook.DefaultFontName;
					//                    if (resolvedFontData.Name == null)
					//                        resolvedFontData.Name = workbook.Styles.NormalStyle.StyleFormatInternal.FontNameResolved;

					//                    // TODO: What should be done with the subscript/superscript style
					//                    // Measure the text and see if it fits
					//                    double neededHeightInPixels;
					//#if SILVERLIGHT
					//                    // MD 6/6/11 - TFS78116
					//                    // Moved this code to the WrappedTextHeightResolver class so it can be called on the UI thread.
					//                    #region Moved

					//                    //textBlock.FontFamily = new System.Windows.Media.FontFamily(resolvedFontData.Name);
					//                    //textBlock.FontSize = (resolvedFontData.Height / Worksheet.TwipsPerPixelAt96DPI);
					//                    //
					//                    //if (resolvedFontData.Bold == ExcelDefaultableBoolean.True)
					//                    //    textBlock.FontWeight = FontWeights.Bold;
					//                    //else
					//                    //    textBlock.FontWeight = FontWeights.Normal;
					//                    //
					//                    //if (resolvedFontData.Italic == ExcelDefaultableBoolean.True)
					//                    //    textBlock.FontStyle = FontStyles.Italic;
					//                    //else
					//                    //    textBlock.FontStyle = FontStyles.Normal;
					//                    //
					//                    //if (resolvedFontData.UnderlineStyle == FontUnderlineStyle.None)
					//                    //    textBlock.TextDecorations = null;
					//                    //else
					//                    //    textBlock.TextDecorations = TextDecorations.Underline;
					//                    //
					//                    //textBlock.Text = cellText;
					//                    //textBlock.TextWrapping = System.Windows.TextWrapping.Wrap;
					//                    //textBlock.Width = columnWidth;
					//                    //
					//                    //neededHeightInPixels = textBlock.ActualHeight;

					//                    #endregion  // Moved
					//                    try
					//                    {
					//                        WrappedTextHeightResolver resolver = new WrappedTextHeightResolver(resolvedFontData, cellText, columnWidth);
					//                        Dispatcher dispatcher = Deployment.Current.Dispatcher;
					//                        DispatcherSynchronizationContext context = new DispatcherSynchronizationContext(dispatcher);

					//                        context.Send(new SendOrPostCallback(resolver.Execute), null);
					//                        neededHeightInPixels = resolver.NeededHeightInPixels;
					//                    }
					//                    catch (Exception e)
					//                    {
					//                        Utilities.DebugFail(e.ToString());
					//                        neededHeightInPixels = 20;
					//                    }
					//#else
					//                    // MD 2/14/12 - 12.1 - Table Support
					//                    // Moved this code to the CreateFont helper method.
					//                    #region Moved

					//                    //FontFamily family = new FontFamily(resolvedFontData.Name);
					//                    //
					//                    //FontStyle style = FontStyle.Regular;
					//                    //if (resolvedFontData.Bold == ExcelDefaultableBoolean.True && family.IsStyleAvailable(FontStyle.Bold))
					//                    //    style |= FontStyle.Bold;
					//                    //
					//                    //if (resolvedFontData.Italic == ExcelDefaultableBoolean.True && family.IsStyleAvailable(FontStyle.Italic))
					//                    //    style |= FontStyle.Italic;
					//                    //
					//                    //if (resolvedFontData.UnderlineStyle != FontUnderlineStyle.None && family.IsStyleAvailable(FontStyle.Underline))
					//                    //    style |= FontStyle.Underline;
					//                    //
					//                    //if (resolvedFontData.Strikeout == ExcelDefaultableBoolean.True && family.IsStyleAvailable(FontStyle.Strikeout))
					//                    //    style |= FontStyle.Strikeout;
					//                    //
					//                    //using (Font font = new Font(resolvedFontData.Name, resolvedFontData.Height / 20f, style))
					//                    //{
					//                    //    neededHeightInPixels = WorksheetRow.MeasureTextHeightInPixels(grfx, cellText, font, columnWidth);
					//                    //}

					//                    #endregion // Moved
					//                    using (Font font = resolvedFontData.CreateFont())
					//                    {
					//                        neededHeightInPixels = WorksheetRow.MeasureTextHeightInPixels(grfx, cellText, font, columnWidth);
					//                    }
					//#endif

					//                    // MD 2/10/12 - TFS97827
					//                    // Consolidated a lot of the unit conversion code so we don't duplicate code.
					//                    //int neededHeightInTwips = (int)Math.Ceiling(neededHeightInPixels * Worksheet.TwipsPerPixelAt96DPI);
					//                    int neededHeightInTwips = (int)Math.Ceiling(Worksheet.ConvertPixelsToTwips(neededHeightInPixels));

					//                    maxHeight = Math.Max(maxHeight, neededHeightInTwips);

					#endregion // Old Code
					short columnIndex = pair.Key;
					WorksheetCellBlock cellBlock = pair.Value;

					object cellValue;
					if (cellBlock.IsCellValueSavedAsString(this, columnIndex, out cellValue) == false)
						continue;

					WorksheetCellFormatData cellFormat = this.Worksheet.GetCellFormatElementReadOnly(this, columnIndex);
					if (cellFormat.WrapTextResolved != ExcelDefaultableBoolean.True)
						continue;

					
					GetCellTextParameters parameters = new GetCellTextParameters(columnIndex);
					parameters.TextFormatMode = TextFormatMode.AsDisplayed;
					parameters.PreventTextFormattingTypes = PreventTextFormattingTypes.None;
					string cellText = this.GetCellTextInternal(cellBlock, parameters);

					WorksheetColumnBlock columnBlock = this.Worksheet.GetColumnBlock(pair.Key);
					int columnWidth = (int)this.Worksheet.GetColumnWidthInPixels(columnBlock, true) - this.Worksheet.GetColumnWidthPadding();
					columnWidth -= columnWidthCorrection;

					
					// Measure the text and see if it fits
					double neededHeightInPixels;


#region Infragistics Source Cleanup (Region)











#endregion // Infragistics Source Cleanup (Region)

					using (Font font = WorkbookFontData.CreateFont(cellFormat))
					{
						neededHeightInPixels = WorksheetRow.MeasureTextHeightInPixels(grfx, cellText, font, columnWidth);
					}


					int neededHeightInTwips = (int)Math.Ceiling(Worksheet.ConvertPixelsToTwips(neededHeightInPixels));
					maxHeight = Math.Max(maxHeight, neededHeightInTwips);
				}
			}

			return maxHeight;
		}

		#endregion // CalculateHeightWithWrappedText

		// MD 3/5/12 - 12.1 - Table Support
		#region ClearCellFormat

		internal void ClearCellFormat(short columnIndex)
		{
			short blockIndex = (short)(columnIndex / WorksheetRow.CellBlockSize);
			int indexInBlock = columnIndex % WorksheetRow.CellBlockSize;

			WorksheetCellFormatData[] cellFormatBlock;
			if (this.CellFormatsForCells.TryGetValue(blockIndex, out cellFormatBlock) == false)
				return;

			WorksheetCellFormatData data = cellFormatBlock[indexInBlock];
			if (data == null)
				return;

			GenericCacheElementEx.ReleaseEx(data);
			cellFormatBlock[indexInBlock] = null;
		}

		#endregion // ClearCellFormat

		// MD 4/12/11 - TFS67084
		#region DirtyCellCalcReference

		internal void DirtyCellCalcReference(short columnIndex)
		{
			short blockIndex = (short)(columnIndex / WorksheetRow.CellBlockSize);

			CellCalcReference calcReference = null;

			CellCalcReference[] cellCalcReferenceBlock = null;
			if (this.HasCellCalcReferences &&
				this.CellCalcReferences.TryGetValue(blockIndex, out cellCalcReferenceBlock))
			{
				short indexInBlock = (short)(columnIndex % WorksheetRow.CellBlockSize);
				calcReference = cellCalcReferenceBlock[indexInBlock];

				if (calcReference != null)
					calcReference.ClearCachedStaticValue();
			}

			Workbook workbook = this.Worksheet.Workbook;
			if (workbook == null)
				return;

			// If the calc reference is not referenced by any pre-existing formulas, we don't need to root the reference.
			// It can be discarded right now and it will be recreated when a formula is added which references this cell.
			// So create a temporary calc reference if there is not one already on the cell. If that is the case and 
			// formulas are actually dirtied by NotifyValueChanged call, store the temporary calc reference on the cell.
			CellCalcReference tempCalcReference = calcReference;
			if (tempCalcReference == null)
				tempCalcReference = new CellCalcReference(this, columnIndex);

			if (workbook.NotifyValueChanged(tempCalcReference) &&
				calcReference == null)
			{
				if (cellCalcReferenceBlock == null)
				{
					cellCalcReferenceBlock = new CellCalcReference[WorksheetRow.CellBlockSize];
					this.CellCalcReferences.Add(blockIndex, cellCalcReferenceBlock);
				}

				short indexInBlock = (short)(columnIndex % WorksheetRow.CellBlockSize);
				cellCalcReferenceBlock[indexInBlock] = tempCalcReference;
			}
		}

		#endregion  // DirtyCellCalcReference

		// MD 1/18/11 - TFS62762
		#region DoesAffectCachedHeightWithWrappedText

		// MD 4/18/11 - TFS62026
		// Added a way to check this for multiple values at once.
		internal static bool DoesAffectCachedHeightWithWrappedText(IList<CellFormatValue> values)
		{
			for (int i = 0; i < values.Count; i++)
			{
				if (WorksheetRow.DoesAffectCachedHeightWithWrappedText(values[i]))
					return true;
			}

			return false;
		}

		// MD 4/18/11 - TFS62026
		// Made private
		//internal static bool DoesAffectCachedHeightWithWrappedText(CellFormatValue value)
		private static bool DoesAffectCachedHeightWithWrappedText(CellFormatValue value)
		{
			switch (value)
			{
				case CellFormatValue.FontBold:
				case CellFormatValue.FontHeight:
				case CellFormatValue.FontItalic:
				case CellFormatValue.FontName:
				case CellFormatValue.FontStrikeout:
				case CellFormatValue.FontSuperscriptSubscriptStyle:
				case CellFormatValue.FontUnderlineStyle:
				case CellFormatValue.WrapText:
					return true;
			}

			return false;
		} 

		#endregion // DoesAffectCachedHeightWithWrappedText

		// MD 4/12/11 - TFS67084
		#region FindFormula

		internal Formula FindFormula(short columnIndex)
		{
			Formula formula;
			this.FindFormulaHelper(columnIndex, out formula);
			return formula;
		} 

		#endregion  // FindFormula

		// MD 4/12/11 - TFS67084
		#region FindFormulaIndex

		internal int FindFormulaIndex(short columnIndex)
		{
			Formula formula;
			return this.FindFormulaHelper(columnIndex, out formula);
		} 

		#endregion  // FindFormulaIndex

		// MD 2/29/12 - 12.1 - Table Support
		#region GetAssociatedColumnAreaFormat

		internal WorksheetCellFormatProxy GetAssociatedColumnAreaFormat(short columnIndex)
		{
			WorksheetTable table = this.GetCellAssociatedTableInternal(columnIndex);
			if (table == null)
				return null;

			return this.GetAssociatedColumnAreaFormat(table, columnIndex);
		}

		internal WorksheetCellFormatProxy GetAssociatedColumnAreaFormat(WorksheetTable table, short columnIndex)
		{
			WorksheetTableArea tableArea = table.GetTableAreaOfRow(this);

			int tableColumnIndex = columnIndex - table.WholeTableAddress.FirstColumnIndex;
			if (0 <= tableColumnIndex && tableColumnIndex < table.Columns.Count)
			{
				WorksheetTableColumn associatedTableColumn = table.Columns[tableColumnIndex];

				WorksheetTableColumnArea tableColumnArea = associatedTableColumn.GetColumnAreaOfRow(this);
				return associatedTableColumn.AreaFormats.GetFormatProxy(null, tableColumnArea, false);
			}
			else
			{
				Utilities.DebugFail("This is unexpected.");
			}

			return null;
		}

		#endregion // GetAssociatedColumnAreaFormat

		// MD 4/12/11 - TFS67084
		#region GetCellBlock

		internal WorksheetCellBlock GetCellBlock(short columnIndex)
		{
			short blockIndex = (short)(columnIndex / WorksheetRow.CellBlockSize);

			// MD 1/31/12 - TFS100573
			//WorksheetCellBlock cellCacheBlock;
			//if (this.CellBlocks.TryGetValue(blockIndex, out cellCacheBlock) == false)
			//{
			//    cellCacheBlock = new WorksheetCellBlock();
			//    this.CellBlocks.Add(blockIndex, cellCacheBlock);
			//}
			//
			//return cellCacheBlock;
			int index = this.BinarySearchForBlock(blockIndex);

			if (index < 0)
			{
				WorksheetCellBlock cellBlock = new WorksheetCellBlockHalf(blockIndex);
				this.InsertCellBlock(~index, cellBlock);
				return cellBlock;
			}

			return this.cellBlocks[index];
		} 

		#endregion  // GetCellBlock

		// MD 4/12/11 - TFS67084
		#region GetCellBlocksWithValue

		internal IEnumerable<KeyValuePair<short, WorksheetCellBlock>> GetCellBlocksWithValues()
		{
			// MD 1/31/12 - TFS100573
			//if (this.HasCellBlocks)
			if (this.cellBlocks != null)
			{
				// MD 1/31/12 - TFS100573
				//foreach (KeyValuePair<short, WorksheetCellBlock> pair in this.CellBlocks)
				//{
				//    int startColumnIndex = pair.Key * WorksheetRow.CellBlockSize;
				//    WorksheetCellBlock cellBlock = pair.Value;
				// MD 1/31/12 - TFS100573
				//foreach (WorksheetCellBlock cellBlock in this.CellBlocks)
				//{
				for (int j = 0; j < this.cellBlocks.Length; j++)
				{
					WorksheetCellBlock cellBlock = this.cellBlocks[j];
					if (cellBlock == null)
						yield break;

					int startColumnIndex = cellBlock.BlockIndex * WorksheetRow.CellBlockSize;

					for (short i = 0; i < WorksheetRow.CellBlockSize; i++)
					{
						short columnIndex = (short)(startColumnIndex + i);

						if (cellBlock.DoesCellHaveValue(columnIndex))
							yield return new KeyValuePair<short, WorksheetCellBlock>(columnIndex, cellBlock);
					}
				}
			}
		} 

		#endregion  // GetCellBlocksWithValue

		// MD 4/12/11 - TFS67084
		#region GetCellCalcReference

		internal CellCalcReference GetCellCalcReference(short columnIndex)
		{
			short blockIndex = (short)(columnIndex / WorksheetRow.CellBlockSize);
			short indexInBlock = (short)(columnIndex % WorksheetRow.CellBlockSize);

			CellCalcReference[] cellCalcReferenceBlock;
			if (this.CellCalcReferences.TryGetValue(blockIndex, out cellCalcReferenceBlock) == false)
			{
				cellCalcReferenceBlock = new CellCalcReference[WorksheetRow.CellBlockSize];
				this.CellCalcReferences.Add(blockIndex, cellCalcReferenceBlock);
			}

			CellCalcReference cellCalcReference = cellCalcReferenceBlock[indexInBlock];

			if (cellCalcReference == null)
			{
				cellCalcReference = new CellCalcReference(this, columnIndex);
				cellCalcReferenceBlock[indexInBlock] = cellCalcReference;
			}

			return cellCalcReference;
		}

		#endregion // GetCellCalcReference

		// MD 3/5/12 - 12.1 - Table Support
		#region GetCellFormatBlock

		internal WorksheetCellFormatData[] GetCellFormatBlock(short columnIndex)
		{
			short blockIndex = (short)(columnIndex / WorksheetRow.CellBlockSize);

			WorksheetCellFormatData[] cellFormatBlock;
			if (this.CellFormatsForCells.TryGetValue(blockIndex, out cellFormatBlock) == false)
			{
				cellFormatBlock = new WorksheetCellFormatData[WorksheetRow.CellBlockSize];
				this.CellFormatsForCells.Add(blockIndex, cellFormatBlock);
			}

			return cellFormatBlock;
		}

		#endregion // GetCellFormatBlock

		// MD 3/1/12 - 12.1 - Table Support
		#region GetCellFormatForBorderSynchronization

		internal WorksheetCellFormatProxy GetCellFormatForBorderSynchronization(short columnIndex, CellFormatValue value)
		{
			WorksheetCellFormatProxy cellFormat;
			if (this.TryGetCellFormat(columnIndex, out cellFormat))
				return cellFormat;

			// If column format is valid and has the property set, force the cell format to get created so we can overwrite 
			// its values with the current value being set.
			WorksheetCellFormatProxy associatedTableColumnAreaFormat = this.GetAssociatedColumnAreaFormat(columnIndex);
			if (associatedTableColumnAreaFormat != null && associatedTableColumnAreaFormat.IsValueDefault(value) == false)
				return this.GetCellFormatInternal(columnIndex);

			return null;
		}

		#endregion // GetCellFormatForBorderSynchronization

		// MD 4/12/11 - TFS67084
		#region GetCellFormatInternal

		internal WorksheetCellFormatProxy GetCellFormatInternal(short columnIndex)
		{
			// MD 3/5/12 - 12.1 - Table Support
			// Moved this code to a helper method.
			#region Moved

			//short blockIndex = (short)(columnIndex / WorksheetRow.CellBlockSize);
			//int indexInBlock = columnIndex % WorksheetRow.CellBlockSize;

			//WorksheetCellFormatData[] cellFormatBlock;
			//if (this.CellFormatsForCells.TryGetValue(blockIndex, out cellFormatBlock) == false)
			//{
			//    cellFormatBlock = new WorksheetCellFormatData[WorksheetRow.CellBlockSize];
			//    this.CellFormatsForCells.Add(blockIndex, cellFormatBlock);
			//}

			#endregion // Moved
			WorksheetCellFormatData[] cellFormatBlock = this.GetCellFormatBlock(columnIndex);

			// MD 3/5/12 - 12.1 - Table Support
			// Moved the rest of this code to the new overload.
			return this.GetCellFormatInternal(cellFormatBlock, columnIndex);
		}

		// MD 3/5/12 - 12.1 - Table Support
		internal WorksheetCellFormatProxy GetCellFormatInternal(WorksheetCellFormatData[] cellFormatBlock, short columnIndex)
		{
			// MD 3/5/12 - 12.1 - Table Support
			int indexInBlock = columnIndex % WorksheetRow.CellBlockSize;

			WorksheetCellFormatData data = cellFormatBlock[indexInBlock];
			WorksheetCellOwnedFormatProxy cellFormat;

			if (data == null)
			{
				Workbook workbook = this.Worksheet.Workbook;
				WorksheetCellFormatCollection cellFormats = workbook == null ? null : workbook.CellFormats;

				if (cellFormats == null)
				{
					data = new WorksheetCellFormatData(null, WorksheetCellFormatType.CellFormat);

					// MD 1/2/12 - 12.1 - Cell Format Updates
					// This is not needed.
					//data.IsStyle = false;
				}
				else
				{
					// MD 1/1/12 - 12.1 - Cell Format Updates
					//data = (WorksheetCellFormatData)workbook.CellFormats.DefaultCellElement.Clone();
					data = workbook.CellFormats.DefaultElement.CloneInternal();
				}

				this.InitializeDefaultCellProxyData(columnIndex, data);

				cellFormat = new WorksheetCellOwnedFormatProxy(data, cellFormats, this, columnIndex, cellFormatBlock);
				cellFormatBlock[indexInBlock] = cellFormat.ElementInternal;

				// MD 4/19/11 - TFS73111
				// When the cell format is initialize, the count of cells with data may increase by one, so dirty the count.
				if (this.cells != null)
					this.cells.DirtyCellCount();
			}
			else
			{
				cellFormat = this.GetCellFormatProxyFromData(columnIndex, data, cellFormatBlock, false);
			}

			return cellFormat;
		} 

		#endregion  // GetCellFormatInternal

		// MD 4/12/11 - TFS67084
		#region GetCellFormatsOnCells

		internal IEnumerable<CellFormatContext> GetCellFormatsOnCells()
		{
			if (this.HasCellFormatsForCells)
			{
				foreach (KeyValuePair<short, WorksheetCellFormatData[]> pair in this.CellFormatsForCells)
				{
					int startColumnIndex = pair.Key * WorksheetRow.CellBlockSize;
					WorksheetCellFormatData[] cellFormatBlock = pair.Value;

					for (short i = 0; i < WorksheetRow.CellBlockSize; i++)
					{
						WorksheetCellFormatData data = cellFormatBlock[i];

						if (data == null)
							continue;

						yield return new CellFormatContext((short)(startColumnIndex + i), data, cellFormatBlock);
					}
				}
			}
		}

		#endregion  // GetCellFormatsOnCells

		// MD 4/12/11 - TFS67084
		#region GetCellAddressString

		internal string GetCellAddressString(int columnIndex, CellReferenceMode cellReferenceMode, bool includeWorksheetName)
		{
			return this.GetCellAddressString(columnIndex, cellReferenceMode, includeWorksheetName, false, false);
		}

		internal string GetCellAddressString(int columnIndex, CellReferenceMode cellReferenceMode, bool includeWorksheetName, bool useRelativeColumn, bool useRelativeRow)
		{
            // MRS 9/20/2011 - TFS85272
            // Moved into a static helper method
            //
            //return (includeWorksheetName ? Utilities.CreateReferenceString(null, this.Worksheet.Name) : string.Empty) +
            //    CellAddress.GetCellReferenceString(
            //        this.Index, columnIndex,
            //        useRelativeRow, useRelativeColumn,
            //        this.Worksheet.CurrentFormat, this, (short)columnIndex, false, cellReferenceMode);
            //
            return WorksheetCell.GetCellAddressString(
                this, //worksheetRow, 
                columnIndex, 
                cellReferenceMode, 
                includeWorksheetName, 
                useRelativeColumn, 
                useRelativeRow);
		}

        

		#endregion GetCellAddressString

		// MD 4/18/11 - TFS62026
		#region GetCellFormatProxyFromData

		internal WorksheetCellOwnedFormatProxy GetCellFormatProxyFromData(short columnIndex, WorksheetCellFormatData cellFormatData, WorksheetCellFormatData[] cellFormatBlock, bool isUsedInternally)
		{
			WorksheetCellFormatCollection cellFormats = null;
			Workbook workbook = this.Worksheet.Workbook;
			if (workbook != null)
				cellFormats = workbook.CellFormats;

			WorksheetCellOwnedFormatProxy proxy = new WorksheetCellOwnedFormatProxy(
				null,
				cellFormats,
				this,
				columnIndex,
				cellFormatBlock);
			proxy.ElementInternal = cellFormatData;
			return proxy;
		}

		#endregion  // GetCellFormatProxyFromData

		// MD 4/12/11 - TFS67084
		#region GetCellsWithData

		internal IEnumerable<CellDataContext> GetCellsWithData()
		{
			// MD 12/1/11 - TFS96113
			// Moved all code to the new overload.
			return this.GetCellsWithData(0, Int32.MaxValue);
		}

		// MD 12/1/11 - TFS96113
		// Added a way to enumerate only cell in a certain range.
		internal IEnumerable<CellDataContext> GetCellsWithData(int startIndex, int endIndex)
		{
			// MD 1/31/12 - TFS100573
			//IEnumerator<KeyValuePair<short, WorksheetCellBlock>> cacheBlocks = this.HasCellBlocks ? this.CellBlocks.GetEnumerator() : null;
			WorksheetCellBlock[] cacheBlocks = this.cellBlocks;
			int index = -1;

			IEnumerator<KeyValuePair<short, WorksheetCellFormatData[]>> cellFormatBlocks = null;
			WorksheetCellFormatCollection cellFormats = null;

			// Only iterate cells with cell formats when we are attached to a workbook.
			Workbook workbook = this.Worksheet.Workbook;
			if (workbook != null)
			{
				cellFormatBlocks = this.HasCellFormatsForCells ? this.CellFormatsForCells.GetEnumerator() : null;
				cellFormats = workbook.CellFormats;
			}

			// MD 1/9/12 - 12.1 - Cell Format Updates
			// Cache the row format.
			WorksheetCellFormatData rowFormat = cellFormats.DefaultElement;
			if (this.HasCellFormat)
				rowFormat = this.CellFormatInternal.Element;

			// MD 1/31/12 - TFS100573
			//KeyValuePair<short, WorksheetCellBlock>? nextCellBlockPair = null;
			WorksheetCellBlock nextCellBlock = null;

			KeyValuePair<short, WorksheetCellFormatData[]>? nextCellFormatBlockPair = null;
			while (cacheBlocks != null || cellFormatBlocks != null)
			{
				// MD 1/31/12 - TFS100573
				//if (nextCellBlockPair == null && cacheBlocks != null)
				if (nextCellBlock == null && cacheBlocks != null)
				{
					// MD 1/31/12 - TFS100573
					//if (cacheBlocks.MoveNext())
					//{
					//    nextCellBlockPair = cacheBlocks.Current;
					//}
					//else
					//{
					//    cacheBlocks.Dispose();
					//    cacheBlocks = null;
					//}
					if (++index < this.cellBlocks.Length)
						nextCellBlock = this.cellBlocks[index];

					if (nextCellBlock == null)
						cacheBlocks = null;
				}

				if (nextCellFormatBlockPair == null && cellFormatBlocks != null)
				{
					if (cellFormatBlocks.MoveNext())
					{
						nextCellFormatBlockPair = cellFormatBlocks.Current;
					}
					else
					{
						cellFormatBlocks.Dispose();
						cellFormatBlocks = null;
					}
				}

				short currentBlockIndex = Int16.MaxValue;
				// MD 1/31/12 - TFS100573
				//if (nextCellBlockPair.HasValue)
				//    currentBlockIndex = Math.Min(currentBlockIndex, nextCellBlockPair.Value.Key);
				if (nextCellBlock != null)
					currentBlockIndex = Math.Min(currentBlockIndex, nextCellBlock.BlockIndex);

				if (nextCellFormatBlockPair.HasValue)
					currentBlockIndex = Math.Min(currentBlockIndex, nextCellFormatBlockPair.Value.Key);

				WorksheetCellBlock currentCellBlock = null;

				// MD 1/31/12 - TFS100573
				//if (nextCellBlockPair.HasValue && nextCellBlockPair.Value.Key == currentBlockIndex)
				//{
				//    currentCellBlock = nextCellBlockPair.Value.Value;
				//    nextCellBlockPair = null;
				//}
				if (nextCellBlock != null && nextCellBlock.BlockIndex == currentBlockIndex)
				{
					currentCellBlock = nextCellBlock;
					nextCellBlock = null;
				}

				WorksheetCellFormatData[] currentCellFormatBlock = null;
				if (nextCellFormatBlockPair.HasValue && nextCellFormatBlockPair.Value.Key == currentBlockIndex)
				{
					currentCellFormatBlock = nextCellFormatBlockPair.Value.Value;
					nextCellFormatBlockPair = null;
				}

				int startColumnIndex = currentBlockIndex * WorksheetRow.CellBlockSize;

				// MD 12/1/11 - TFS96113
				// Skip over blocks which are outside the range we are looking for.
				if ((startColumnIndex + WorksheetRow.CellBlockSize) <= startIndex && endIndex < startColumnIndex)
					continue;

				for (short i = 0; i < WorksheetRow.CellBlockSize; i++)
				{
					short columnIndex = (short)(startColumnIndex + i);

					// MD 12/1/11 - TFS96113
					// Make sure the column index is within the range we are looking for.
					if (columnIndex < startIndex || endIndex < columnIndex)
						continue;

					// MD 1/9/12 - 12.1 - Cell Format Updates
					// Split this up into two values so we can trakc whether it has a value and a format separately.
					//bool shouldReturnCell = false;
					bool hasFormat = false;
					bool hasValue = false;

					// MD 3/1/12 - 12.1 - Table Support
					//if (currentCellBlock != null && currentCellBlock.DoesCellHaveValue(columnIndex))
					//{
					//    // MD 1/9/12 - 12.1 - Cell Format Updates
					//    //shouldReturnCell = true;
					//    hasValue = true;
					//}
					bool hasStatusBits = false;
					if (currentCellBlock != null)
					{
						hasValue = currentCellBlock.DoesCellHaveValue(columnIndex);
						hasStatusBits = currentCellBlock.GetIsInTableHeaderOrTotalRow(columnIndex);
					}

					WorksheetCellFormatData cellFormatData = null;
					if (currentCellFormatBlock != null)
					{
						cellFormatData = currentCellFormatBlock[i];

						// MD 1/1/12 - 12.1 - Cell Format Updates
						// For cells, we should really be checking to see if they are equal to the row format, not the default format.
						// Because if they are equal to the row format and there is no value, we should not be writing out the cells.
						//if (cellFormatData != null && cellFormatData.HasSameData(cellFormats.DefaultElement) == false)
						if (cellFormatData != null && cellFormatData.EqualsInternal(rowFormat) == false)
						{
							// MD 1/9/12 - 12.1 - Cell Format Updates
							//shouldReturnCell = true;
							hasFormat = true;
						}
					}

					// MD 6/2/11 - TFS73945
					// If the cell is part of a merged region that will be written out, the cell is considered to have data because
					// it will be written out in the saved file.
					//if (shouldReturnCell == false)
					if (hasFormat == false)
					{
						WorksheetMergedCellsRegion mergedRegion = this.GetCellAssociatedMergedCellsRegionInternal(columnIndex);
						if (mergedRegion != null && mergedRegion.HasCellFormat)
						{
							// MD 1/9/12 - 12.1 - Cell Format Updates
							//if (mergedRegion.CellFormatInternal.HasDefaultValue == false)
							//    shouldReturnCell = true;
							if (mergedRegion.CellFormatInternal.Element.EqualsInternal(cellFormats.DefaultElement) == false)
								hasFormat = true;
						}
					}

					// MD 1/9/12 - 12.1 - Cell Format Updates
					//if (shouldReturnCell)
					//    yield return new CellDataContext(columnIndex, currentCellBlock, cellFormatData);
					// MD 3/1/12 - 12.1 - Table Support
					//if (hasFormat || hasValue)
					if (hasFormat || hasValue || hasStatusBits)
					{
						// MD 2/23/12 - 12.1 - Table Support
						// The called may have replaced the cell block
						//yield return new CellDataContext(columnIndex, currentCellBlock, cellFormatData, hasFormat, hasValue);
						CellDataContext cellDataContext = new CellDataContext(columnIndex, currentCellBlock, cellFormatData, hasFormat, hasValue);
						yield return cellDataContext;

						currentCellBlock = cellDataContext.CellBlock;
						if (cellDataContext.CellFormatDataWasCleared && currentCellFormatBlock != null)
							currentCellFormatBlock[i] = null;
					}
				}
			}
		}

		#endregion  // GetCellsWithData

		// MD 2/26/12 - 12.1 - Table Support
		#region GetCellValue

		internal static object GetCellValue(WorksheetRow row, short columnIndex)
		{
			if (row == null)
				return null;

			return row.GetCellValueInternal(columnIndex);
		}

		#endregion // GetCellValue

		// MD 4/12/11 - TFS67084
		#region GetCellValueHelper

		private object GetCellValueHelper(short columnIndex, bool getValueRaw)
		{
			WorksheetCellBlock cellBlock;
			if (this.TryGetCellBlock(columnIndex, out cellBlock) == false)
				return null;

			if (getValueRaw)
				return cellBlock.GetCellValueRaw(this, (short)columnIndex);

			return cellBlock.GetCellValue(this, (short)columnIndex);
		}

		#endregion  // GetCellValueHelper

		// MD 4/12/11 - TFS67084
		#region GetCellValueRaw

		internal object GetCellValueRaw(short columnIndex)
		{
			return this.GetCellValueHelper(columnIndex, true);
		}

		#endregion  // GetCellValueRaw

		// MD 4/12/11 - TFS67084
		#region GetColumnIndexesWithCalcReference

		// MD 7/19/12 - TFS116808 (Table resizing)
		// Re-wrote this method to allow enumerating forwards or backwards.
		#region Old Code

		//internal IEnumerable<short> GetColumnIndexesWithCalcReference()
		//{
		//    if (this.HasCellCalcReferences)
		//    {
		//        foreach (KeyValuePair<short, CellCalcReference[]> pair in this.CellCalcReferences)
		//        {
		//            int startColumnIndex = pair.Key * WorksheetRow.CellBlockSize;
		//            CellCalcReference[] cellCalcReferenceBlock = pair.Value;

		//            for (short i = 0; i < WorksheetRow.CellBlockSize; i++)
		//            {
		//                CellCalcReference cellCalcReference = cellCalcReferenceBlock[i];
		//                if (cellCalcReference == null)
		//                    continue;

		//                yield return (short)(startColumnIndex + i);
		//            }
		//        }
		//    }
		//} 

		#endregion // Old Code
		internal IEnumerable<short> GetColumnIndexesWithCalcReference(bool enumerateForwards)
		{
			if (this.HasCellCalcReferences)
			{
				SortedList<short, CellCalcReference[]> cellCalcReferences = this.CellCalcReferences;

				if (enumerateForwards)
				{
					foreach (KeyValuePair<short, CellCalcReference[]> pair in cellCalcReferences)
					{
						int startColumnIndex = pair.Key * WorksheetRow.CellBlockSize;
						CellCalcReference[] cellCalcReferenceBlock = pair.Value;

						for (short i = 0; i < WorksheetRow.CellBlockSize; i++)
						{
							CellCalcReference cellCalcReference = cellCalcReferenceBlock[i];
							if (cellCalcReference == null)
								continue;

							yield return (short)(startColumnIndex + i);
						}
					}
				}
				else
				{
					for (int i = cellCalcReferences.Count - 1; i >= 0; i--)
					{
						int startColumnIndex = cellCalcReferences.Keys[i] * WorksheetRow.CellBlockSize;
						CellCalcReference[] cellCalcReferenceBlock = cellCalcReferences.Values[i];

						for (short j = WorksheetRow.CellBlockSize - 1; j >= 0; j--)
						{
							CellCalcReference cellCalcReference = cellCalcReferenceBlock[j];
							if (cellCalcReference == null)
								continue;

							yield return (short)(startColumnIndex + j);
						}
					}
				}
			}
		} 

		#endregion  // GetColumnIndexesWithCalcReference

		// MD 4/12/11 - TFS67084
		#region GetDataTableDependantCells

		internal List<WorksheetCellAddress> GetDataTableDependantCells(short columnIndex)
		{
			// MD 3/27/12 - 12.1 - Table Support
			//WorksheetCellAddress cellAddress = new WorksheetCellAddress(this, columnIndex);
			WorksheetCellAddress cellAddress = new WorksheetCellAddress(this.Index, columnIndex);

			List<WorksheetCellAddress> dataTableDependantCells;
			if (this.Worksheet.CellOwnedDataTableDependantCells.TryGetValue(cellAddress, out dataTableDependantCells) == false)
			{
				dataTableDependantCells = new List<WorksheetCellAddress>();
				this.Worksheet.CellOwnedDataTableDependantCells.Add(cellAddress, dataTableDependantCells);
			}

			return dataTableDependantCells;
		}

		#endregion  // GetDataTableDependantCells

		#region GetResolvedHeight

		internal int GetResolvedHeight(bool ignoreHidden)
		{
			if (ignoreHidden == false && this.Hidden)
				return 0;

			if (this.Height < 0)
			{
				// MD 2/27/12
				// Found while implementing 12.1 - Table Support
				// The heights are now cached on the rows themselves.
				//int cachedHeightWithWrappedText;
				//if (this.Worksheet.CachedRowHeightsWithWrappedText.TryGetValue(this, out cachedHeightWithWrappedText) == false)
				//{
				//    cachedHeightWithWrappedText = this.CalculateHeightWithWrappedText();
				//    this.Worksheet.CachedRowHeightsWithWrappedText.Add(this, cachedHeightWithWrappedText);
				//}
				//
				//return cachedHeightWithWrappedText;
				if (this.resolvedRowHeight < 0)
					this.resolvedRowHeight = (short)this.CalculateHeightWithWrappedText();

				return this.resolvedRowHeight;
			}

			return this.Height;
		}

		#endregion // GetResolvedHeight

		// MD 2/25/12 - 12.1 - Table Support
		// This is no longer needed since we force all cells in a merged region to have their formats created.
		#region Removed

		//// MD 4/12/11 - TFS67084
		//#region HasCellFormatForCellResolved

		//internal bool HasCellFormatForCellResolved(short columnIndex, out WorksheetCellFormatData cellFormatData)
		//{
		//    if (this.TryGetCellFormat(columnIndex, out cellFormatData))
		//        return true;

		//    WorksheetMergedCellsRegion associatedMergedRegion = this.GetCellAssociatedMergedCellsRegionInternal(columnIndex);

		//    if (associatedMergedRegion == null)
		//        return false;

		//    if (associatedMergedRegion.HasCellFormat)
		//    {
		//        cellFormatData = this.GetCellFormatInternal(columnIndex).Element;
		//        return true;
		//    }

		//    return false;
		//}

		//internal bool HasCellFormatForCellResolved(short columnIndex, out WorksheetCellFormatProxy cellFormatProxy)
		//{
		//    if (this.TryGetCellFormat(columnIndex, out cellFormatProxy))
		//        return true;

		//    WorksheetMergedCellsRegion associatedMergedRegion = this.GetCellAssociatedMergedCellsRegionInternal(columnIndex);

		//    if (associatedMergedRegion == null)
		//        return false;

		//    if (associatedMergedRegion.HasCellFormat)
		//    {
		//        cellFormatProxy = this.GetCellFormatInternal(columnIndex);
		//        return true;
		//    }

		//    return false;
		//}

		//#endregion  // HasCellFormatForCellResolved

		#endregion // Removed

		#region InitSerializationCache



#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

		// MD 7/26/10 - TFS34398
		// We should return the serialization cache.
		//internal void InitSerializationCache( 
		internal WorksheetRowSerializationCache InitSerializationCache( 
			WorkbookSerializationManager serializationManager, 
			WorksheetRow previousRow,
			// MD 1/9/12 - 12.1 - Cell Format Updates
			// We no longer need to pass alogn the empty element. It is the DefaultElement on the cell formats collection.
			//// MD 2/15/11 - TFS66333
			//// We are using the EmptyElement for the default data element. The DefaultElement will be populated with data if 
			//// the workbook was loaded from a file or stream. Also, renamed the parameter for clarity.
			////WorksheetCellFormatData defaultFormatData,
			//WorksheetCellFormatData emptyFormatData,
			bool defaultRowHidden,
			ref bool hasShapes )								// MD 7/20/2007 - BR25039
		{
			// Reset the flag indicating whether the row should be serialized
			// MD 7/26/10 - TFS34398
			// This is no longer store on the row.
			//this.hasData = false;
			WorksheetRowSerializationCache serializationCache = new WorksheetRowSerializationCache();

			#region Determine if a collapse indicator is next to the row

			// MD 3/15/11 - TFS21053
			// The row with the collapsed indicator may be before or after an outlining group. 
			// If the ShowExpansionIndicatorBelowGroupedRowsResolved value is False, the group 
			// is after the row with the indicator, so check the values of the next row instead 
			// of the previous one.
			//
			//// Initialize the default hidden state and outline level of the previous row
			//int previousRowOutlineLevel = 0;
			//bool previousRowHidden = defaultRowHidden;
			//// If the previous row specified is actually the immediate row before this row,
			//// take the hidden state and outline level from that row
			//if ( previousRow != null && previousRow.Index + 1 == this.Index )
			//{
			//    previousRowOutlineLevel = previousRow.OutlineLevel;
			//    previousRowHidden = previousRow.Hidden;
			//}
			// Initialize the default hidden state and outline level of the previous row
			int groupOutlineLevel = 0;
			bool groupHidden = defaultRowHidden;

			if (this.Worksheet.DisplayOptions.ShowExpansionIndicatorBelowGroupedRowsResolved)
			{
				// If the previous row specified is actually the immediate row before this row,
				// take the hidden state and outline level from that row
				if (previousRow != null && previousRow.Index + 1 == this.Index)
				{
					groupOutlineLevel = previousRow.OutlineLevel;
					groupHidden = previousRow.Hidden;
				}
			}
			else
			{
				WorksheetRow nextRow = this.Worksheet.Rows.GetIfCreated(this.Index + 1);
				if (nextRow != null)
				{
					groupOutlineLevel = nextRow.OutlineLevel;
					groupHidden = nextRow.Hidden;
				}
			}

			// This row will only display a collapse indicator if the previous row is hidden and the previous row has 
			// a higher outline level
			// MD 7/26/10 - TFS34398
			// This is no longer store on the row.
			//this.hasCollapseIndicator =
			serializationCache.hasCollapseIndicator =
				groupHidden && this.OutlineLevel < groupOutlineLevel;

			#endregion Determine if a collapse indicator is next to the row

			#region Add the row's cell format to the serialization manager

			// MD 2/15/11 - TFS66333
			//WorksheetCellFormatData rowFormatData = defaultFormatData;
			// MD 1/9/12 - 12.1 - Cell Format Updates
			//WorksheetCellFormatData rowFormatData = emptyFormatData;
			WorksheetCellFormatData rowFormatData;

			if (this.HasCellFormat)
			{
				rowFormatData = this.CellFormatInternal.Element;

				// MD 1/10/12 - 12.1 - Cell Format Updates
				// The columns format must be in the manager now, because it is in the cell formats collection, so we don't have to do anything here.
				//serializationManager.AddFormat( rowFormatData );
			}
			// MD 1/9/12 - 12.1 - Cell Format Updates
			else
			{
				rowFormatData = serializationManager.Workbook.CellFormats.DefaultElement;
			}

			#endregion Add the row's cell format to the serialization manager

			#region Determine the first and last cell's with data

			// Initialize the fields which determine the the range of defined cells in the row
			// MD 7/26/10 - TFS34398
			// These is no longer store on the row.
			//this.firstCellInUndefinedTail = 0;
			//this.firstCell = 0;

			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
			//if ( this.cells != null )
			// MD 1/31/12 - TFS100573
			//if(this.HasCellBlocks || this.HasCellFormatsForCells)
			if (this.cellBlocks != null || this.HasCellFormatsForCells)
			{
				// MD 4/12/11 - TFS67084
				// Moved away from using WorksheetCell objects.
				#region Old Code

				//foreach ( WorksheetCell cell in this.cells )
				//{
				//    // Init the serialize cache for each cell so its HasData property returns a valid value
				//    // MD 7/20/2007 - BR25039
				//    // A new parameter was added to InitSerializationCache
				//    //cell.InitSerializationCache( serializationManager, rowFormatData, defaultFormatData );
				//    // MD 2/15/11 - TFS66333
				//    //cell.InitSerializationCache( serializationManager, rowFormatData, defaultFormatData, ref hasShapes );
				//    cell.InitSerializationCache(serializationManager, rowFormatData, emptyFormatData, ref hasShapes);
				//
				//    if ( cell.HasData )
				//    {
				//        // MD 7/26/10 - TFS34398
				//        // These is no longer store on the row.
				//        //// If the cell has data, this row has data
				//        //this.hasData = true;
				//        //
				//        //// If this if the first cell with data, initialize the firstCell field (if any previous cell 
				//        //// had data, the firstCellInUndefinedTail would be one greater than its column index, which 
				//        //// would make it greater than zero)
				//        //if ( this.firstCellInUndefinedTail == 0 )
				//        //    this.firstCell = cell.ColumnIndex;
				//        //
				//        //// The first cell in the undefined tail of the row is after the current cell
				//        //this.firstCellInUndefinedTail = cell.ColumnIndex + 1;
				//        // If the cell has data, this row has data
				//        serializationCache.hasData = true;
				//
				//        // If this if the first cell with data, initialize the firstCell field (if any previous cell 
				//        // had data, the firstCellInUndefinedTail would be one greater than its column index, which 
				//        // would make it greater than zero)
				//        if (serializationCache.firstCellInUndefinedTail == 0)
				//            serializationCache.firstCell = (short)cell.ColumnIndex;
				//
				//        // The first cell in the undefined tail of the row is after the current cell
				//        serializationCache.firstCellInUndefinedTail = (short)(cell.ColumnIndex + 1);
				//    }
				//}

				#endregion // Old Code
				// MD 4/18/11 - TFS62026
				List<short> cellFormatIndexValues = new List<short>();

				foreach (CellDataContext cellDataContext in this.GetCellsWithData())
				{
					// Init the serialize cache for each cell so its HasData property returns a valid value
					// MD 1/10/12 - 12.1 - Cell Format Updates
					// We don't need to pass as much stuff here anymore.
					//short indexInFormatCollection;
					//this.InitSerializationCacheForCell(cellDataContext, 
					//    serializationManager,
					//    rowFormatData,
					//    emptyFormatData, 
					//    out indexInFormatCollection);
					this.InitSerializationCacheForCell(cellDataContext, serializationManager);

					// If the cell has data, this row has data
					serializationCache.hasData = true;

					// If this if the first cell with data, initialize the firstCell field (if any previous cell 
					// had data, the firstCellInUndefinedTail would be one greater than its column index, which 
					// would make it greater than zero)
					if (serializationCache.firstCellInUndefinedTail == 0)
						serializationCache.firstCell = cellDataContext.ColumnIndex;

					// The first cell in the undefined tail of the row is after the current cell
					serializationCache.firstCellInUndefinedTail = (short)(cellDataContext.ColumnIndex + 1);

					// MD 1/10/12 - 12.1 - Cell Format Updates
					// We no longer need to cache the indexes.
					//// MD 4/18/11 - TFS62026
					//// We will now store a list of cell format indexes for cells on the row cache so we don't have 
					//// to store a big dictionary on the serialization manager.
					//cellFormatIndexValues.Add(indexInFormatCollection);
				}

				// MD 1/10/12 - 12.1 - Cell Format Updates
				// We no longer need to cache the indexes.
				//// MD 4/18/11 - TFS62026
				//// Store the list of cell format indexes on the row cache.
				//serializationCache.cellFormatIndexValues = cellFormatIndexValues.ToArray();
			}

			#endregion Determine the first and last cell's with data

			// If the row still doesn't have data, cache the value of the HasDataIgnoreHidden property
			// MD 7/26/10 - TFS34398
			// This is no longer stored on the row. Also, the HasDataIgnoreHidden property can't get to the hasCollapseIndicator
			// value, so check it here.
			//if ( this.hasData == false )
			//    this.hasData = this.HasDataIgnoreHidden;
			if (serializationCache.hasData == false)
				serializationCache.hasData = serializationCache.hasCollapseIndicator || this.HasDataIgnoreHidden;

			// MD 7/26/10 - TFS34398
			// MD 4/14/11
			// Found while fixing TFS62026
			// We should only store the row caches for rows which will be written out.
			//serializationManager.RowSerializationCaches[this] = serializationCache;
			if (serializationCache.hasData)
				serializationManager.RowSerializationCaches[this] = serializationCache;

			return serializationCache;
		}

		#endregion InitSerializationCache

		// MD 1/18/11 - TFS62762

		#region MeasureTextHeightInPixels

		private static double MeasureTextHeightInPixels(Graphics grfx, string text, Font font, int widthLimit)
		{
			// MD 3/26/12 - 12.1 - Table Support
			// Rewrote this code to more accurately match Excel.
			#region Old Code

			//Size proposedSize = new Size(widthLimit, Int32.MaxValue);
			//Size size = TextRenderer.MeasureText(grfx, text, font, proposedSize, TextFormatFlags.Top);
			//if (size.Width < widthLimit)
			//    return size.Height;

			////~ We shouldn't use GDI+ measuring here, but it does what we need it to do, which is
			////~ to wrap words when they exceed the bounds of the layout area.
			//SizeF gdiPlusSize = grfx.MeasureString(text, font, widthLimit);
			//return gdiPlusSize.Height;

			#endregion // Old Code
			Size proposedSize = new Size(widthLimit, Int32.MaxValue);
			Size size = TextRenderer.MeasureText(grfx, text, font, proposedSize, TextFormatFlags.NoPadding | TextFormatFlags.WordBreak);

			Size singleLineSize = TextRenderer.MeasureText(grfx, "0", font, Size.Empty, TextFormatFlags.NoPadding | TextFormatFlags.SingleLine);

			Debug.Assert(size.Height % singleLineSize.Height == 0, "The height should be a multiple of the single line height.");
			int numberOfLines = size.Height / singleLineSize.Height;

			double defaultRowHeightForFont = Worksheet.GetDefaultRowHeight(font.Name, (int)(font.SizeInPoints * 20)) / Worksheet.TwipsPerPixelAt96DPI;
			return numberOfLines * defaultRowHeightForFont;
		}

		#endregion // MeasureTextHeightInPixels


		// MD 4/12/11 - TFS67084
		#region OnCellOwnedFormatValueChanged

		internal void OnCellOwnedFormatValueChanged(WorksheetCellOwnedFormatProxy sender, IList<CellFormatValue> values, CellFormatValueChangedOptions options)
		{
			// MD 4/19/11 - TFS73111
			// If the format values are changing to or from default values, the cells count could change by one, so dirty the count.
			if (this.cells != null)
				this.cells.DirtyCellCount();

			// MD 1/18/11 - TFS62762
			// If one of the format values affecting the height of cells with wrapped text has changed, 
			// we should reset the calculated height of the row, because it might change.
			if (WorksheetRow.DoesAffectCachedHeightWithWrappedText(values))
				this.ResetCachedHeightWithWrappedText();

			WorksheetMergedCellsRegion associatedMergedCellRegion = null;

			if ((options & CellFormatValueChangedOptions.PreventCellToMergedRegionSyncronization) == 0)
				associatedMergedCellRegion = this.GetCellAssociatedMergedCellsRegionInternal(sender.ColumnIndex);

			// MD 5/2/11 - TFS74130
			// This is a mistake with the refactoring for TFS67084.
			// The borderCellFormat value was being used for four different sides, not just one.
			// Use a dictionary to cache the formats for each side.
			//WorksheetCellFormatProxy borderCellFormat = null;
			//bool isBorderCellFormatValid = false;
			Dictionary<CellFormatValue, WorksheetCellFormatProxy> borderCellFormats = new Dictionary<CellFormatValue, WorksheetCellFormatProxy>(8);

			// MD 3/10/12 - 12.1 - Table Support
			WorksheetCellBlock cellBlock;
			this.TryGetCellBlock(sender.ColumnIndex, out cellBlock);

			for (int i = 0; i < values.Count; i++)
			{
				CellFormatValue value = values[i];

				switch (value)
				{
					case CellFormatValue.Alignment:

					// MD 10/26/11 - TFS91546
					case CellFormatValue.DiagonalBorderColorInfo:
					case CellFormatValue.DiagonalBorders:
					case CellFormatValue.DiagonalBorderStyle:

					case CellFormatValue.Fill:

					// MD 1/29/12 - 12.1 - Cell Format Updates
					// We have a separate case for the font properties now.
					//// MD 10/13/10 - TFS43003
					//case CellFormatValue.FontBold:
					//case CellFormatValue.FontColorInfo:
					//case CellFormatValue.FontHeight:
					//case CellFormatValue.FontItalic:
					//case CellFormatValue.FontName:
					//case CellFormatValue.FontStrikeout:
					//case CellFormatValue.FontSuperscriptSubscriptStyle:
					//case CellFormatValue.FontUnderlineStyle:

					case CellFormatValue.FormatString:
					case CellFormatValue.Indent:
					case CellFormatValue.Locked:
					case CellFormatValue.Rotation:
					case CellFormatValue.ShrinkToFit:

					// MD 2/27/12 - 12.1 - Table Support
					//case CellFormatValue.IsStyle:

					case CellFormatValue.VerticalAlignment:
					case CellFormatValue.WrapText:

					// MD 1/1/12 - 12.1 - Cell Format Updates
					case CellFormatValue.FormatOptions:

						// For non-border properties, push the values onto the associated merged region's format, if this cell is merged.
						if (associatedMergedCellRegion != null)
						{
							// MD 6/2/11
							// Found while fixing TFS73945.
							// I think I added the PreventMergedRegionToCellSyncronization flag here as a performance fix so we wouldn't try
							// to set the new value on the cell which already has the value. However, the merged region needs to also sync
							// the value to the other cells in the region, so we cannot pass PreventMergedRegionToCellSyncronization here.
							//Utilities.CopyCellFormatValue(sender,
							//    associatedMergedCellRegion.CellFormatInternal,
							//    value,
							//    true,
							//    CellFormatValueChangedOptions.PreventMergedRegionToCellSyncronization);
							Utilities.CopyCellFormatValue(sender,
								associatedMergedCellRegion.CellFormatInternal,
								value);
						}

						break;

					// MD 1/29/12 - 12.1 - Cell Format Updates
					case CellFormatValue.Style:
						{
							if (associatedMergedCellRegion != null)
								Utilities.CopyCellFormatValue(sender, associatedMergedCellRegion.CellFormatInternal, value);

							WorkbookStyle style = sender.Style;
							if (style != null &&
								Utilities.TestFlag(style.StyleFormatInternal.FormatOptions, WorksheetCellFormatOptions.ApplyFontFormatting))
							{
								// MD 3/10/12 - 12.1 - Table Support
								//WorksheetCellBlock cellBlock;
								//if (this.TryGetCellBlock(sender.ColumnIndex, out cellBlock))
								if (cellBlock != null)
								{
									FormattedStringElement formattedString = cellBlock.GetCellValueIfFormattedString(this, sender.ColumnIndex) as FormattedStringElement;
									if (formattedString != null)
										formattedString.ClearFormatting();
								}
							}
						}
						break;

					// MD 1/29/12 - 12.1 - Cell Format Updates
					case CellFormatValue.FontBold:
					case CellFormatValue.FontColorInfo:
					case CellFormatValue.FontHeight:
					case CellFormatValue.FontItalic:
					case CellFormatValue.FontName:
					case CellFormatValue.FontStrikeout:
					case CellFormatValue.FontSuperscriptSubscriptStyle:
					case CellFormatValue.FontUnderlineStyle:
						{
							if (associatedMergedCellRegion != null)
								Utilities.CopyCellFormatValue(sender, associatedMergedCellRegion.CellFormatInternal, value);

							// MD 3/10/12 - 12.1 - Table Support
							//WorksheetCellBlock cellBlock;
							//if (this.TryGetCellBlock(sender.ColumnIndex, out cellBlock))
							if (cellBlock != null)
							{
								FormattedStringElement formattedString = cellBlock.GetCellValueIfFormattedString(this, sender.ColumnIndex) as FormattedStringElement;
								if (formattedString != null && formattedString.HasFormatting)
								{
									Workbook workbook = this.Worksheet.Workbook;

									foreach (FormattedStringRun run in formattedString.FormattingRuns)
									{
										if (run.HasFont == false)
											continue;

										IWorkbookFont font = run.GetFont(workbook);
										switch (value)
										{
											case CellFormatValue.FontBold:
												font.Bold = sender.Font.Bold;
												break;

											case CellFormatValue.FontColorInfo:
												font.ColorInfo = sender.Font.ColorInfo;
												break;

											case CellFormatValue.FontHeight:
												font.Height = sender.Font.Height;
												break;

											case CellFormatValue.FontItalic:
												font.Italic = sender.Font.Italic;
												break;

											case CellFormatValue.FontName:
												font.Name = sender.Font.Name;
												break;

											case CellFormatValue.FontStrikeout:
												font.Strikeout = sender.Font.Strikeout;
												break;

											case CellFormatValue.FontSuperscriptSubscriptStyle:
												font.SuperscriptSubscriptStyle = sender.Font.SuperscriptSubscriptStyle;
												break;

											case CellFormatValue.FontUnderlineStyle:
												font.UnderlineStyle = sender.Font.UnderlineStyle;
												break;

											default:
												Utilities.DebugFail("Unknown format value: " + value);
												break;
										}
									}
								}
							}
						}
						break;

					case CellFormatValue.BottomBorderColorInfo:
					case CellFormatValue.BottomBorderStyle:
					case CellFormatValue.LeftBorderColorInfo:
					case CellFormatValue.LeftBorderStyle:
					case CellFormatValue.RightBorderColorInfo:
					case CellFormatValue.RightBorderStyle:
					case CellFormatValue.TopBorderColorInfo:
					case CellFormatValue.TopBorderStyle:
						{
							// If the cell's border value has changed and it belongs to a merged cell region, it might change the merged cell format properties.
							if (associatedMergedCellRegion != null)
								associatedMergedCellRegion.OnCellBorderValueChanged(this, sender.ColumnIndex, value);

							// MD 10/21/10 - TFS34398
							// If we should prevent syncing the borders of adjacent cells, skip the next block of code.
							if ((options & CellFormatValueChangedOptions.PreventAdjacentBorderSyncronization) != 0)
								break;

							// MD 3/22/12 - TFS104630
							// This is no longer true. Now we are resetting overlapping borders, but only if they don't match.
							//// MD 9/28/11 - TFS88683
							//// We now always sync overlapping border properties, not just when they are set to non-default values.
							////// If the value was changed to a default value, we don't need to reset the bordering cell's value.
							////if (sender.IsValueDefault(value))
							////    break;
							if (sender.IsValueDefault(value))
								break;

							// MD 5/2/11 - TFS74130
							// This is a mistake with the refactoring for TFS67084.
							// The borderCellFormat value was being used for four different sides, not just one.
							// Use a dictionary to cache the formats for each side.
							//// If the cell's border was changed and it shares a border with a merged cell region, it might change the merged cell format properties.
							//if (isBorderCellFormatValid == false)
							//{
							//    borderCellFormat = this.GetAdjacentBorderCellFormat(sender.ColumnIndex, value, this.Worksheet.CurrentFormat);
							//    isBorderCellFormatValid = true;
							//}
							WorksheetCellFormatProxy borderCellFormat;
							if (borderCellFormats.TryGetValue(value, out borderCellFormat) == false)
							{
								borderCellFormat = this.GetAdjacentBorderCellFormat(sender.ColumnIndex, value, this.Worksheet.CurrentFormat);
								borderCellFormats[value] = borderCellFormat;
								borderCellFormats[Utilities.GetAssociatedBorderValue(value)] = borderCellFormat;
							}

							// If there is no border cell format, we don't need to reset the bordering cell's value.
							if (borderCellFormat == null)
								break;

							// MD 3/22/12 - TFS104630
							// We are now going back to resetting overlapping borders instead of syncing them, because that it what 
							// Microsoft seems to do internally based on looking at what is saved in their file formats. But we may also
							// need to take the associated border value from the overlapping cell format. This is all done in a helper
							// method now.
							//CellFormatValue oppositeBorderValue = Utilities.GetOppositeBorderValue(value);
							//
							//// MD 9/28/11 - TFS88683
							//// Instead of resetting overlapping borders, we will now keep them synced.
							////// Or if the value was set to the same as the corresponding border property on the bordering cell, we don't need to reset the bordering cell's value.
							////if (Object.Equals(sender.GetValue(value), borderCellFormat.GetValue(oppositeBorderValue)))
							////{
							////    break;
							////}
							////
							////// Reset the property value on the bordering cell.
							////borderCellFormat.ResetValue(oppositeBorderValue, CellFormatValueChangedOptions.PreventAdjacentBorderSyncronization);
							//borderCellFormat.SetValue(oppositeBorderValue, sender.GetValue(value), true, CellFormatValueChangedOptions.PreventAdjacentBorderSyncronization);
							Utilities.SynchronizeOverlappingBorderProperties(sender, borderCellFormat, value, sender.GetValue(value), options);
						}
						break;

					default:
						Utilities.DebugFail("Unknown format value: " + value);
						break;
				}
			}

			// MD 3/10/12 - 12.1 - Table Support
			// If the cell is a table header or total cell, sync the cell format with the associated column area format.
			if (cellBlock != null && cellBlock.GetIsInTableHeaderOrTotalRow(sender.ColumnIndex))
			{
				WorksheetTable table = this.GetCellAssociatedTableInternal(sender.ColumnIndex);
				if (table != null)
				{
					int tableColumnIndex = sender.ColumnIndex - table.WholeTableAddress.FirstColumnIndex;
					if (tableColumnIndex < 0 || table.Columns.Count <= tableColumnIndex)
					{
						Utilities.DebugFail("This is unexpected.");
					}
					else
					{
						List<CellFormatValue> valuesToSyncWithTableColumnArea = new List<CellFormatValue>(values);

						Workbook workbook = this.Worksheet.Workbook;
						WorksheetTableColumn column = table.Columns[tableColumnIndex];
						WorksheetCell headerCell = column.HeaderCell;
						if (headerCell != null && headerCell.Row == this && headerCell.ColumnIndex == sender.ColumnIndex)
						{
							valuesToSyncWithTableColumnArea.Remove(CellFormatValue.TopBorderColorInfo);
							valuesToSyncWithTableColumnArea.Remove(CellFormatValue.TopBorderStyle);
							Utilities.CopyCellFormatValues(sender,
								column.AreaFormats.GetFormatProxy(workbook, WorksheetTableColumnArea.HeaderCell),
								valuesToSyncWithTableColumnArea);
						}
						else
						{
							WorksheetCell totalCell = column.TotalCell;
							if (totalCell != null && totalCell.Row == this && totalCell.ColumnIndex == sender.ColumnIndex)
							{
								valuesToSyncWithTableColumnArea.Remove(CellFormatValue.BottomBorderColorInfo);
								valuesToSyncWithTableColumnArea.Remove(CellFormatValue.BottomBorderStyle);
								Utilities.CopyCellFormatValues(sender,
									column.AreaFormats.GetFormatProxy(workbook, WorksheetTableColumnArea.TotalCell),
									valuesToSyncWithTableColumnArea);
							}
							else
							{
								Utilities.DebugFail("This is unexpected.");
							}
						}
					}
				}
			}
		}

		#endregion  // OnCellOwnedFormatValueChanged

		// MD 7/2/09 - TFS18634
		#region OnCurrentFormatChanged

		// MD 11/24/10 - TFS34598
		// This is now a virtual defined on the base.
		//internal void OnCurrentFormatChanged()
		internal override void OnCurrentFormatChanged()
		{
			// MD 11/24/10 - TFS34598
			// Call the base implementation.
			base.OnCurrentFormatChanged();

			this.Cells.OnCurrentFormatChanged();
		} 

		#endregion OnCurrentFormatChanged

		// MD 1/31/12 - TFS100573
		#region ReplaceCellBlock

		internal void ReplaceCellBlock(short columnIndex, WorksheetCellBlock replacementBlock)
		{
			short blockIndex = (short)(columnIndex / WorksheetRow.CellBlockSize);
			int index = this.BinarySearchForBlock(blockIndex);
			if (index < 0)
			{
				Utilities.DebugFail("This shouldn't have happened.");
				this.InsertCellBlock(~index, replacementBlock);
			}
			else
			{
				this.cellBlocks[index] = replacementBlock;
			}
		}

		#endregion // ReplaceCellBlock

		// MD 4/12/11 - TFS67084
		#region RemoveFormula

		internal void RemoveFormula(short columnIndex)
		{
			Formula formula;
			int index = this.FindFormulaHelper(columnIndex, out formula);

			if (0 <= index)
				this.CellOwnedFormulas.RemoveAt(index);
		}

		#endregion  // RemoveFormula

		// MD 1/18/11 - TFS62762
		#region ResetCachedHeightWithWrappedText

		internal void ResetCachedHeightWithWrappedText()
		{
			// MD 2/27/12
			// Found while implementing 12.1 - Table Support
			// The heights are now cached on the rows themselves.
			//// MD 2/3/12 - TFS100573
			//// This is a slight performance improvement so we don't have to create/search the dictionary when it doesn't exist.
			//if (this.Worksheet.HasCachedRowHeightsWithWrappedText == false)
			//    return;
			//
			//int cachedHeightWithWrappedText;
			//if (this.Worksheet.CachedRowHeightsWithWrappedText.TryGetValue(this, out cachedHeightWithWrappedText) == false)
			//    return;
			if (this.resolvedRowHeight < 0)
				return;

			int cachedHeightWithWrappedText = this.resolvedRowHeight;

			// If the height of the row was cached and is now changing, we may need to adjust the anchor cell positions
			// of shapes intersecting this row, so call the appropriate methods to do so.
			this.Worksheet.OnBeforeWorksheetElementResize(this);

			// Also, remove the cache height, it will be re-cached in OnAfterWorksheetElementResized if any shapes intersect 
			// this row.
			// MD 2/27/12
			// Found while implementing 12.1 - Table Support
			// The heights are now cached on the rows themselves.
			//this.Worksheet.CachedRowHeightsWithWrappedText.Remove(this);
			this.resolvedRowHeight = -1;

			this.Worksheet.Rows.ResetHeightCache(this.Index, false);
			this.Worksheet.OnAfterWorksheetElementResized(this, cachedHeightWithWrappedText, this.Hidden);
		} 

		#endregion // ResetCachedHeightWithWrappedText

		// MD 4/12/11 - TFS67084
		#region SetAssociatedMergedCellsRegion

		internal void SetAssociatedMergedCellsRegion(short columnIndex, WorksheetMergedCellsRegion region)
		{
			// MD 3/27/12 - 12.1 - Table Support
			//WorksheetCellAddress address = new WorksheetCellAddress(this, columnIndex);
			WorksheetCellAddress address = new WorksheetCellAddress(this.Index, columnIndex);

			if (region == null)
			{
				if (this.Worksheet.HasCellOwnedAssociatedMergedCellsRegions)
					this.Worksheet.CellOwnedAssociatedMergedCellsRegions.Remove(address);
			}
			else
			{
				this.Worksheet.CellOwnedAssociatedMergedCellsRegions[address] = region;
			}
		}

		#endregion  // SetAssociatedMergedCellsRegion

		// MD 4/12/11 - TFS67084
		#region SetCellFormatWhileLoading

		internal void SetCellFormatWhileLoading(short columnIndex, WorksheetCellFormatData data)
		{
			short blockIndex = (short)(columnIndex / WorksheetRow.CellBlockSize);
			int indexInBlock = columnIndex % WorksheetRow.CellBlockSize;

			WorksheetCellFormatData[] cellFormatBlock;
			if (this.CellFormatsForCells.TryGetValue(blockIndex, out cellFormatBlock) == false)
			{
				cellFormatBlock = new WorksheetCellFormatData[WorksheetRow.CellBlockSize];
				this.CellFormatsForCells.Add(blockIndex, cellFormatBlock);
			}

			WorksheetCellFormatData cellFormatData = cellFormatBlock[indexInBlock];
			if (cellFormatData != null)
			{
				this.GetCellFormatProxyFromData(columnIndex, cellFormatData, cellFormatBlock, true).SetFormatting(data);
				return;
			}

			this.CacheCellFormat(columnIndex, cellFormatBlock, indexInBlock, data);
		} 

		#endregion  // SetCellFormatWhileLoading

		// MD 4/12/11 - TFS67084
		#region SetCellValueRaw

		internal void SetCellValueRaw(short columnIndex, object valueRaw)
		{
			this.SetCellValueRaw(columnIndex, valueRaw, true);
		}

		internal void SetCellValueRaw(short columnIndex, object valueRaw, bool checkForBlockingValues)
		{
			short blockIndex = (short)(columnIndex / WorksheetRow.CellBlockSize);

			WorksheetCellBlock cellBlock;
			if (valueRaw == null)
			{
				// If the cell cache isn't created yet, the value is implicitly null, so we don't have to do anything.
				this.TryGetCellBlock(columnIndex, out cellBlock);
			}
			else
			{
				cellBlock = this.GetCellBlock(columnIndex);
			}

			if (cellBlock != null)
			{
				// MD 1/31/12 - TFS100573
				// This method now has an out parameter which returns the replacement block. We don't have to do anything with it because it
				// will replace itself on the row's CellBlocks collection when a repalcement is needed.
				//cellBlock.SetCellValueInternal(this, (short)columnIndex, valueInternal, checkForBlockingValues);
				WorksheetCellBlock replacementBlock;
				cellBlock.SetCellValueRaw(this, (short)columnIndex, valueRaw, checkForBlockingValues, out replacementBlock);
			}
			else
			{
				this.SetFormulaOnCalcReference(columnIndex, valueRaw, true);
				this.DirtyCellCalcReference(columnIndex);
			}
		}

		#endregion  // SetCellValueRaw

		// MD 4/12/11 - TFS67084
		#region SetFormulaOnCalcReference

		internal void SetFormulaOnCalcReference(short columnIndex, object valueRaw, bool canClearPreviouslyCalculatedValue)
		{
			Formula formula = null;

			if (valueRaw != null)
			{
				formula = valueRaw as Formula;

				// If the formula is null, but the data table is not, get the formula which would need to be solved for the interior cell's value 
				// to be calculated correctly and use that formula instead.
				if (formula == null)
				{
					WorksheetDataTable dataTable = valueRaw as WorksheetDataTable;

					if (dataTable != null)
						formula = dataTable.GetInteriorCellFormula(this, columnIndex);
				}
			}

			CellCalcReference cellCalcReference;
			if (formula == null)
			{
				if (this.TryGetCellCalcReference(columnIndex, out cellCalcReference))
					cellCalcReference.SetAndCompileFormula(null, canClearPreviouslyCalculatedValue);

				return;
			}

			cellCalcReference = this.GetCellCalcReference(columnIndex);
			cellCalcReference.SetAndCompileFormula(formula, canClearPreviouslyCalculatedValue);

			// If any interior cells in a data table were calculated with a derived version of this cell's formula or value, and the 
			// value has changed, all dependant cells should be dirtied for recalculation.
			if (this.Worksheet.HasCellOwnedDataTableDependantCells)
			{
				// MD 3/27/12 - 12.1 - Table Support
				//WorksheetCellAddress cellAddress = new WorksheetCellAddress(this, columnIndex);
				WorksheetCellAddress cellAddress = new WorksheetCellAddress(this.Index, columnIndex);

				List<WorksheetCellAddress> dataTableDependantCells;
				if (this.Worksheet.CellOwnedDataTableDependantCells.TryGetValue(cellAddress, out dataTableDependantCells))
				{
					// If the formula has been cleared, the interior cells will not depend on this cell anymore. Clear the dependant cell 
					// collection. If a new formula has replaced the old one, the interior cells will end up repopulating the collection 
					// anyway.
					this.Worksheet.CellOwnedDataTableDependantCells.Remove(cellAddress);

					// Let all dependant cells recreate their derived formulas, but don't make them clear their cached values, just in case 
					// the calculation mode is manual for data tables, they should still return their old values.
					foreach (WorksheetCellAddress dependantCell in dataTableDependantCells)
					{
						// MD 3/27/12 - 12.1 - Table Support
						//object dependantCellValueRaw = dependantCell.Row.GetCellValueRaw(dependantCell.ColumnIndex);
						//dependantCell.Row.SetFormulaOnCalcReference(dependantCell.ColumnIndex, dependantCellValueRaw, false);
						WorksheetRow row = this.Worksheet.Rows[dependantCell.RowIndex];
						object dependantCellValueRaw = row.GetCellValueRaw(dependantCell.ColumnIndex);
						row.SetFormulaOnCalcReference(dependantCell.ColumnIndex, dependantCellValueRaw, false);
					}
				}
			}
		}

		#endregion  // SetFormulaOnCalcReference

		// MD 2/16/12 - 12.1 - Table Support
		#region SetIsInTableHeaderOrTotalRow

		internal void SetIsInTableHeaderOrTotalRow(short columnIndex, bool isInTableHeaderOrTotalRow)
		{
			short blockIndex = (short)(columnIndex / WorksheetRow.CellBlockSize);

			WorksheetCellBlock cellBlock;
			if (isInTableHeaderOrTotalRow == false)
			{
				// If the cell cache isn't created yet, the value is implicitly false, so we don't have to do anything.
				this.TryGetCellBlock(columnIndex, out cellBlock);
			}
			else
			{
				cellBlock = this.GetCellBlock(columnIndex);
			}

			if (cellBlock != null)
			{
				WorksheetCellBlock replacementBlock;
				cellBlock.SetIsInTableHeaderOrTotalRow(this, (short)columnIndex, isInTableHeaderOrTotalRow, out replacementBlock);
			}
		}

		#endregion // SetIsInTableHeaderOrTotalRow

		// MD 4/12/11 - TFS67084
		#region SetMergedCellOnCell

		internal void SetMergedCellOnCell(short columnIndex, WorksheetMergedCellsRegion region)
		{
			if (region != null)
			{
				// Only preserve the value and comment of the top-left cell, clear them on the other cells.
				if (region.FirstRow != this.Index || region.FirstColumnInternal != columnIndex)
				{
					this.SetCellCommentInternal(columnIndex, null);
					this.SetCellValueRaw(columnIndex, null);
				}
			}
			else
			{
				Debug.Assert(this.GetCellAssociatedMergedCellsRegionInternal(columnIndex) != null);
			}

			this.SetAssociatedMergedCellsRegion(columnIndex, region);
		} 

		#endregion  // SetMergedCellOnCell

		// MD 4/12/11 - TFS67084
		#region TryGetCellBlock

		internal bool TryGetCellBlock(short columnIndex, out WorksheetCellBlock cellBlock)
		{
			// MD 1/31/12 - TFS100573
			//if (columnIndex < 0 || this.HasCellBlocks == false)
			if (columnIndex < 0 || this.cellBlocks == null)
			{
				cellBlock = null;
				return false;
			}

			short blockIndex = (short)(columnIndex / WorksheetRow.CellBlockSize);

			// MD 1/31/12 - TFS100573
			//return this.CellBlocks.TryGetValue(blockIndex, out cellBlock);
			int index = this.BinarySearchForBlock(blockIndex);
			if (index < 0)
			{
				cellBlock = null;
				return false;
			}

			// MD 1/31/12 - TFS100573
			//cellBlock = this.CellBlocks[index];
			cellBlock = this.cellBlocks[index];
			return true;
		} 

		#endregion  // TryGetCellBlock

		// MD 4/12/11 - TFS67084
		#region TryGetCellCalcReference

		internal bool TryGetCellCalcReference(short columnIndex, out CellCalcReference cellCalcReference)
		{
			return this.TryGetCellCalcReference(columnIndex, false, out cellCalcReference);
		}

		internal bool TryGetCellCalcReference(short columnIndex, bool createReferenceIfCellHasValue, out CellCalcReference cellCalcReference)
		{
			if (createReferenceIfCellHasValue == false && this.HasCellCalcReferences == false)
			{
				cellCalcReference = null;
				return false;
			}

			short blockIndex = (short)(columnIndex / WorksheetRow.CellBlockSize);

			CellCalcReference[] cellCalcReferenceBlock;
			if (this.CellCalcReferences.TryGetValue(blockIndex, out cellCalcReferenceBlock) == false)
			{
				if (createReferenceIfCellHasValue == false ||
					this.DoesCellHaveValue(columnIndex) == false)
				{
					cellCalcReference = null;
					return false;
				}

				cellCalcReferenceBlock = new CellCalcReference[WorksheetRow.CellBlockSize];
				this.CellCalcReferences.Add(blockIndex, cellCalcReferenceBlock);
			}

			short indexInBlock = (short)(columnIndex % WorksheetRow.CellBlockSize);

			cellCalcReference = cellCalcReferenceBlock[indexInBlock];

			if (cellCalcReference == null &&
				createReferenceIfCellHasValue &&
				this.DoesCellHaveValue(columnIndex))
			{
				cellCalcReference = new CellCalcReference(this, columnIndex);
				cellCalcReferenceBlock[indexInBlock] = cellCalcReference;
			}

			return cellCalcReference != null;
		}

		#endregion // TryGetCalcReference

		// MD 4/12/11 - TFS67084
		#region TryGetCellFormat

		internal bool TryGetCellFormat(short columnIndex, out WorksheetCellFormatProxy cellFormatProxy)
		{
			WorksheetCellFormatData cellFormatData;
			WorksheetCellFormatData[] cellFormatBlock;
			if (this.TryGetCellFormat(columnIndex, out cellFormatData, out cellFormatBlock))
			{
				cellFormatProxy = this.GetCellFormatProxyFromData(columnIndex, cellFormatData, cellFormatBlock, true);
				return true;
			}

			cellFormatProxy = null;
			return false;
		}

		internal bool TryGetCellFormat(short columnIndex, out WorksheetCellFormatData cellFormatData)
		{
			WorksheetCellFormatData[] cellFormatBlock;
			return this.TryGetCellFormat(columnIndex, out cellFormatData, out cellFormatBlock);
		}

		private bool TryGetCellFormat(short columnIndex, out WorksheetCellFormatData cellFormatData, out WorksheetCellFormatData[] cellFormatBlock)
		{
			if (columnIndex < 0 || this.Worksheet.Workbook == null)
			{
				cellFormatData = null;
				cellFormatBlock = null;
				return false;
			}

			short blockIndex = (short)(columnIndex / WorksheetRow.CellBlockSize);

			if (this.CellFormatsForCells.TryGetValue(blockIndex, out cellFormatBlock) == false)
			{
				cellFormatData = null;
				return false;
			}

			short indexInBlock = (short)(columnIndex % WorksheetRow.CellBlockSize);
			cellFormatData = cellFormatBlock[indexInBlock];
			return cellFormatData != null;
		}

		#endregion  // TryGetCellFormat

		// MD 4/18/11 - TFS62026
		#region UnrootAllCellFormatsForCells

		// MD 12/21/11 - 12.1 - Table Support
		//internal void UnrootAllCellFormatsForCells(WorksheetCellFormatCollection cellFormatCollection)
		internal void UnrootAllCellFormatsForCells()
		{
			if (this.HasCellFormatsForCells == false)
				return;

			foreach (KeyValuePair<short, WorksheetCellFormatData[]> pair in this.CellFormatsForCells)
			{
				int startColumnIndex = pair.Key * WorksheetRow.CellBlockSize;
				WorksheetCellFormatData[] cellFormatBlock = pair.Value;

				for (short i = 0; i < WorksheetRow.CellBlockSize; i++)
				{
					WorksheetCellFormatData cellFormatData = cellFormatBlock[i];

					if (cellFormatData == null)
						continue;

					// MD 12/21/11 - 12.1 - Table Support
					//GenericCacheElement.Release(cellFormatData, cellFormatCollection);
					GenericCacheElementEx.ReleaseEx(cellFormatData);

					if (cellFormatData.ReferenceCount > 0)
						cellFormatBlock[i] = cellFormatData.CloneInternal();
				}
			}
		} 

		#endregion  // UnrootAllCellFormatsForCells

		// MD 4/12/11 - TFS67084
		#region UpdateFormattedStringElementOnCell

		// MD 5/31/11 - TFS75574
		// We need more information than just the key from the element, so just pass the element directly.
		//internal void UpdateFormattedStringKeyOnCell(short columnIndex, uint key)
		internal void UpdateFormattedStringElementOnCell(short columnIndex, StringElement element)
		{
			WorksheetCellBlock cellBlock;
			if (this.TryGetCellBlock(columnIndex, out cellBlock) == false)
				return;

			// MD 5/31/11 - TFS75574
			// We need more information than just the key from the element, so just pass the element directly.
			//cellBlock.SetFormattedStringKey(columnIndex, key);
			cellBlock.UpdateFormattedStringElement(columnIndex, element);
		}

		#endregion  // UpdateFormattedStringElementOnCell

		#endregion // Internal Methods

		#region Private Methods

		// MD 1/31/12 - TFS100573
		#region BinarySearchForBlock

		private int BinarySearchForBlock(short blockIndex)
		{
			if (this.cellBlocks == null)
				return -1;

			int start = 0;
			int end = this.cellBlocks.Length - 1;

			while (start <= end)
			{
				int mid = start + ((end - start) / 2);

				WorksheetCellBlock block = this.cellBlocks[mid];

				if (block == null)
				{
					end = mid - 1;
					continue;
				}

				int compareResult = block.BlockIndex - blockIndex;
				if (compareResult == 0)
					return mid;

				if (compareResult < 0)
					start = mid + 1;
				else
					end = mid - 1;
			}

			return ~start;
		}

		#endregion // BinarySearchForBlock

		// MD 4/12/11 - TFS67084
		#region CacheCellFormat

		private void CacheCellFormat(short columnIndex,
			WorksheetCellFormatData[] cellFormatBlock, int indexInBlock,
			WorksheetCellFormatData initialData)
		{
			Workbook workbook = this.Worksheet.Workbook;
			Debug.Assert(workbook != null, "The workbook should not be null here.");
			WorksheetCellFormatCollection cellFormatCollection = workbook != null ? workbook.CellFormats : null;

			if (initialData == null)
			{
				// MD 1/1/12 - 12.1 - Cell Format Updates
				// The default element is now a cell format and not a style format.
				//initialData = (WorksheetCellFormatData)cellFormatCollection.DefaultCellElement.Clone();
				initialData = cellFormatCollection.DefaultElement.CloneInternal();
			}

			// MD 2/2/12 - TFS100573
			//cellFormatBlock[columnIndex % WorksheetRow.CellBlockSize] = GenericCacheElement.FindExistingOrAddToCache(initialData, cellFormatCollection);
			cellFormatBlock[columnIndex % WorksheetRow.CellBlockSize] = GenericCacheElementEx.FindExistingOrAddToCacheEx(initialData, cellFormatCollection);
		}

		#endregion  // CacheCellFormat

		// MD 4/12/11 - TFS67084
		#region DoesCellHaveValue

		private bool DoesCellHaveValue(short columnIndex)
		{
			WorksheetCellBlock cellBlock;
			if (this.TryGetCellBlock(columnIndex, out cellBlock) && cellBlock.DoesCellHaveValue(columnIndex))
				return true;

			return false;
		} 

		#endregion  // DoesCellHaveValue

		// MD 4/12/11 - TFS67084
		#region FindFormulaHelper

		private int FindFormulaHelper(short columnIndex, out Formula formula)
		{
			if (this.cellOwnedFormulas == null)
			{
				formula = null;
				return ~0;
			}

			int start = 0;
			int end = this.cellOwnedFormulas.Count - 1;

			while (start <= end)
			{
				int mid = start + ((end - start) / 2);
				formula = this.cellOwnedFormulas[mid];

				int compareResult = formula.OwningCellColumnIndex - columnIndex;
				if (compareResult == 0)
					return mid;

				if (compareResult < 0)
					start = mid + 1;
				else
					end = mid - 1;
			}

			formula = null;
			return ~start;
		} 

		#endregion  // FindFormulaHelper

		// MD 4/12/11 - TFS67084
		#region GetAdjacentBorderCellFormat

		private WorksheetCellFormatProxy GetAdjacentBorderCellFormat(short columnIndex, CellFormatValue value, WorkbookFormat currentFormat)
		{
			WorksheetCellFormatData cellFormatData;
			WorksheetCellFormatProxy cellFormatProxy;
			WorksheetMergedCellsRegion associatedMergedCellsRegion;
			this.GetAdjacentBorderCellFormatHelper(columnIndex, value, currentFormat, false, true, out cellFormatData, out cellFormatProxy, out associatedMergedCellsRegion);

			return cellFormatProxy;
		}

		private WorksheetCellFormatData GetAdjacentBorderCellFormat(short columnIndex, CellFormatValue value, WorkbookFormat currentFormat,
			out WorksheetMergedCellsRegion associatedMergedCellsRegion)
		{
			WorksheetCellFormatData cellFormatData;
			WorksheetCellFormatProxy cellFormatProxy;
			this.GetAdjacentBorderCellFormatHelper(columnIndex, value, currentFormat, true, false, out cellFormatData, out cellFormatProxy, out associatedMergedCellsRegion);

			return cellFormatData;
		}

		private void GetAdjacentBorderCellFormatHelper(short columnIndex, CellFormatValue value, WorkbookFormat currentFormat, bool getMergedRegion, bool getProxy,
			out WorksheetCellFormatData cellFormatData,
			out WorksheetCellFormatProxy cellFormatProxy,
			out WorksheetMergedCellsRegion associatedMergedCellsRegion)
		{
			cellFormatData = null;
			cellFormatProxy = null;
			associatedMergedCellsRegion = null;

			WorksheetRow borderCellRow = null;
			short borderCellColumnIndex = 0;

			switch (value)
			{
				case CellFormatValue.BottomBorderColorInfo:
				case CellFormatValue.BottomBorderStyle:
					if ((this.Index + 1) < Workbook.GetMaxRowCount(currentFormat))
					{
						borderCellRow = this.Worksheet.Rows.GetIfCreated(this.Index + 1);
						borderCellColumnIndex = columnIndex;
					}
					break;

				case CellFormatValue.LeftBorderColorInfo:
				case CellFormatValue.LeftBorderStyle:
					if (columnIndex > 0)
					{
						borderCellRow = this;
						borderCellColumnIndex = (short)(columnIndex - 1);
					}
					break;

				case CellFormatValue.RightBorderColorInfo:
				case CellFormatValue.RightBorderStyle:
					if ((columnIndex + 1) < Workbook.GetMaxColumnCount(currentFormat))
					{
						borderCellRow = this;
						borderCellColumnIndex = (short)(columnIndex + 1);
					}
					break;

				case CellFormatValue.TopBorderColorInfo:
				case CellFormatValue.TopBorderStyle:
					if (this.Index > 0)
					{
						borderCellRow = this.Worksheet.Rows.GetIfCreated(this.Index - 1);
						borderCellColumnIndex = columnIndex;
					}
					break;
			}

			if (borderCellRow != null)
			{
				if (getMergedRegion)
					associatedMergedCellsRegion = this.GetCellAssociatedMergedCellsRegionInternal(columnIndex);

				if (getProxy)
					borderCellRow.TryGetCellFormat(borderCellColumnIndex, out cellFormatProxy);
				else
					borderCellRow.TryGetCellFormat(borderCellColumnIndex, out cellFormatData);
			}
		}

		#endregion // GetAdjacentBorderCellFormat

		// MD 3/1/12 - 12.1 - Table Support
		#region GetCellFormatsForRowFormatValueSynchronization

		private IEnumerable<CellFormatContext> GetCellFormatsForRowFormatValueSynchronization(CellFormatValue value)
		{
			
			SortedList<short, WorksheetTable> tablesIntersectingWithRow = new SortedList<short, WorksheetTable>();
			for (int i = 0; i < this.Worksheet.Tables.Count; i++)
			{
				WorksheetTable table = this.Worksheet.Tables[i];
				WorksheetRegionAddress tableAddress = table.WholeTableAddress;
				if (tableAddress.FirstRowIndex <= this.Index && this.Index <= tableAddress.LastRowIndex)
					tablesIntersectingWithRow.Add(tableAddress.FirstColumnIndex, table);
			}

			short lastIteratedIndex = -1;
			foreach (KeyValuePair<short, WorksheetCellFormatData[]> pair in this.CellFormatsForCells)
			{
				int startColumnIndex = pair.Key * WorksheetRow.CellBlockSize;
				WorksheetCellFormatData[] cellFormatBlock = pair.Value;

				for (short indexInBlock = 0; indexInBlock < WorksheetRow.CellBlockSize; indexInBlock++)
				{
					WorksheetCellFormatData data = cellFormatBlock[indexInBlock];
					if (data == null)
						continue;

					short currentColumnIndex = (short)(startColumnIndex + indexInBlock);
					foreach (CellFormatContext cellFormatContext in
						this.GetCellFormatsForRowFormatValueSynchronizationHelper(tablesIntersectingWithRow, value, currentColumnIndex, lastIteratedIndex))
					{
						yield return cellFormatContext;
					}

					yield return new CellFormatContext(currentColumnIndex, data, cellFormatBlock);

					while (tablesIntersectingWithRow.Count != 0 && tablesIntersectingWithRow.Keys[0] < currentColumnIndex)
						tablesIntersectingWithRow.RemoveAt(0);

					lastIteratedIndex = currentColumnIndex;
				}
			}

			foreach (CellFormatContext cellFormatContext in
				this.GetCellFormatsForRowFormatValueSynchronizationHelper(tablesIntersectingWithRow, value, this.Worksheet.Columns.MaxCount, lastIteratedIndex))
			{
				yield return cellFormatContext;
			}
		}

		private IEnumerable<CellFormatContext> GetCellFormatsForRowFormatValueSynchronizationHelper(
			SortedList<short, WorksheetTable> tables,
			CellFormatValue value,
			int currentColumnIndex,
			short lastIteratedIndex)
		{
			if ((lastIteratedIndex + 1) != currentColumnIndex)
			{
				for (int i = 0; i < tables.Count; i++)
				{
					if (currentColumnIndex < tables.Keys[i])
						break;

					WorksheetTable table = tables.Values[i];
					WorksheetRegionAddress tableAddress = table.WholeTableAddress;

					short startColumnIndex = Math.Max((short)(lastIteratedIndex + 1), tableAddress.FirstColumnIndex);
					short endColumnIndex = Math.Min((short)(currentColumnIndex - 1), tableAddress.LastColumnIndex);

					for (short columnIndex = startColumnIndex; columnIndex <= endColumnIndex; columnIndex++)
					{
						// If column format is valid and has the property set, force the cell format to get created so we can overwrite 
						// its values with the current value being set.
						WorksheetCellFormatProxy associatedTableColumnAreaFormat = this.GetAssociatedColumnAreaFormat(table, columnIndex);
						if (associatedTableColumnAreaFormat != null && associatedTableColumnAreaFormat.IsValueDefault(value) == false)
							yield return new CellFormatContext(columnIndex, null, null);
					}
				}
			}
		}

		#endregion // GetCellFormatsForRowFormatValueSynchronization

		// MD 4/12/11 - TFS67084
		#region InitializeDefaultCellProxyData






		private void InitializeDefaultCellProxyData(short columnIndex, WorksheetCellFormatData cellFormat)
		{
			// MD 3/22/12 - TFS104630
			// Rewrote this code now that we are using new rules regarding border synchronization to more closely 
			// match those of Excel. Also, we no longer need to sync with the merged region because the cell formats
			// in a merged region are forced to be created when they are merged.
			#region Old Code

			//Worksheet worksheet = this.Worksheet;

			//// MD 3/15/12 - TFS104581
			////WorksheetColumn column = worksheet.Columns.GetIfCreated(columnIndex);
			//WorksheetColumnBlock column = worksheet.GetColumnBlock(columnIndex);

			//int rowIndex = this.Index;
			//WorkbookFormat currentFormat = worksheet.CurrentFormat;

			//WorksheetMergedCellsRegion bottomBorderMergedRegion;
			//WorksheetCellFormatData bottomBorderCellFormat = this.GetAdjacentBorderCellFormat(columnIndex, CellFormatValue.BottomBorderColorInfo, currentFormat, out bottomBorderMergedRegion);

			//WorksheetMergedCellsRegion leftBorderMergedRegion;
			//WorksheetCellFormatData leftBorderCellFormat = this.GetAdjacentBorderCellFormat(columnIndex, CellFormatValue.LeftBorderColorInfo, currentFormat, out leftBorderMergedRegion);

			//WorksheetMergedCellsRegion rightBorderMergedRegion;
			//WorksheetCellFormatData rightBorderCellFormat = this.GetAdjacentBorderCellFormat(columnIndex, CellFormatValue.RightBorderColorInfo, currentFormat, out rightBorderMergedRegion);

			//WorksheetMergedCellsRegion topBorderMergedRegion;
			//WorksheetCellFormatData topBorderCellFormat = this.GetAdjacentBorderCellFormat(columnIndex, CellFormatValue.TopBorderColorInfo, currentFormat, out topBorderMergedRegion);

			//// Cache the merged region of the cell.
			//WorksheetMergedCellsRegion associatedMergedCellsRegion = this.GetCellAssociatedMergedCellsRegionInternal(columnIndex);

			//// MD 2/29/12 - 12.1 - Table Support
			//WorksheetCellFormatProxy associatedTableAreaFormat;
			//WorksheetCellFormatProxy associatedTableColumnAreaFormat;
			//this.GetAssociatedTableAndColumnAreaFormats(columnIndex, 
			//    out associatedTableAreaFormat, 
			//    out associatedTableColumnAreaFormat);

			//// MD 10/26/11 - TFS91546
			//// A better way to do this is just to use Enum.GetValues
			////CellFormatValue[] valuesToSync = new CellFormatValue[]{
			////        CellFormatValue.Alignment,
			////        CellFormatValue.BottomBorderColor,
			////        CellFormatValue.BottomBorderStyle,
			////        CellFormatValue.FillPattern,
			////        CellFormatValue.FillPatternBackgroundColor,
			////        CellFormatValue.FillPatternForegroundColor,
			////        CellFormatValue.FontBold,
			////        CellFormatValue.FontColor,
			////        CellFormatValue.FontHeight,
			////        CellFormatValue.FontItalic,
			////        CellFormatValue.FontName,
			////        CellFormatValue.FontStrikeout,
			////        CellFormatValue.FontSuperscriptSubscriptStyle,
			////        CellFormatValue.FontUnderlineStyle,
			////        CellFormatValue.FormatString,
			////        CellFormatValue.Indent,
			////        CellFormatValue.LeftBorderColor,
			////        CellFormatValue.LeftBorderStyle,
			////        CellFormatValue.Locked,
			////        CellFormatValue.RightBorderColor,
			////        CellFormatValue.RightBorderStyle,
			////        CellFormatValue.Rotation,
			////        CellFormatValue.ShrinkToFit,
			////        CellFormatValue.Style,
			////        CellFormatValue.TopBorderColor,
			////        CellFormatValue.TopBorderStyle,
			////        CellFormatValue.VerticalAlignment,
			////        CellFormatValue.WrapText,
			////    };
			//// MD 3/1/12 - 12.1 - Table Support
			////CellFormatValue[] valuesToSync = (CellFormatValue[])CalcManagerUtilities.EnumGetValues(typeof(CellFormatValue));
			//IList<CellFormatValue> valuesToSync = WorksheetCellFormatData.AllCellFormatValues;

			//// MD 9/28/11 - TFS88683
			//// We may need to initialize the border properties from the adjacent rows or columns.
			//int maxRowIndex = Workbook.GetMaxRowCount(currentFormat) - 1;
			//int maxColumnIndex = Workbook.GetMaxColumnCount(currentFormat) - 1;
			//WorksheetRow rowBelow = rowIndex < maxRowIndex ? worksheet.Rows.GetIfCreated(rowIndex + 1) : null;
			//WorksheetRow rowAbove = 0 < rowIndex ? worksheet.Rows.GetIfCreated(rowIndex - 1) : null;

			//// MD 3/15/12 - TFS104581
			////WorksheetColumn columnToRight = columnIndex < maxColumnIndex ? worksheet.Columns.GetIfCreated(columnIndex + 1) : null;
			////WorksheetColumn columnToLeft = 0 < columnIndex ? worksheet.Columns.GetIfCreated(columnIndex - 1) : null;
			//WorksheetColumnBlock columnToRight = columnIndex < maxColumnIndex ? worksheet.GetColumnBlock((short)(columnIndex + 1)) : null;
			//WorksheetColumnBlock columnToLeft = 0 < columnIndex ? worksheet.GetColumnBlock((short)(columnIndex - 1)) : null;

			//WorksheetCellFormatData currentRowFormat = null;
			//if (this.HasCellFormat)
			//    currentRowFormat = this.CellFormatInternal.Element;

			//WorksheetCellFormatData rowBelowFormat = null;
			//if (rowBelow != null && rowBelow.HasCellFormat)
			//    rowBelowFormat = rowBelow.CellFormatInternal.Element;

			//WorksheetCellFormatData rowAboveFormat = null;
			//if (rowAbove != null && rowAbove.HasCellFormat)
			//    rowAboveFormat = rowAbove.CellFormatInternal.Element;

			//WorksheetCellFormatData columnToRightFormat = null;
			//if (columnToRight != null)
			//    columnToRightFormat = columnToRight.CellFormat;

			//WorksheetCellFormatData columnToLeftFormat = null;
			//if (columnToLeft != null)
			//    columnToLeftFormat = columnToLeft.CellFormat;

			//foreach (CellFormatValue valueToSync in valuesToSync)
			//{
			//    WorksheetMergedCellsRegion borderMergedRegion = null;
			//    WorksheetCellFormatData borderCellFormat = null;

			//    // MD 3/15/12 - TFS104581
			//    //RowColumnBase owner = null;
			//    //// MD 9/28/11 - TFS88683
			//    //RowColumnBase adjacentOwner = null;
			//    WorksheetCellFormatData ownerFormat = null;
			//    WorksheetCellFormatData adjacentOwnerFormat = null;

			//    switch (valueToSync)
			//    {
			//        // MD 1/8/12 - 12.1 - Cell Format Updates
			//        // We should not sync this properties.
			//        case CellFormatValue.FormatOptions:
			//            continue;

			//        case CellFormatValue.BottomBorderColorInfo:
			//        case CellFormatValue.BottomBorderStyle:
			//            borderMergedRegion = bottomBorderMergedRegion;
			//            borderCellFormat = bottomBorderCellFormat;

			//            // MD 3/15/12 - TFS104581
			//            //owner = this;
			//            //// MD 9/28/11 - TFS88683
			//            //adjacentOwner = rowBelow;
			//            ownerFormat = currentRowFormat;
			//            adjacentOwnerFormat = rowBelowFormat;

			//            break;

			//        case CellFormatValue.TopBorderColorInfo:
			//        case CellFormatValue.TopBorderStyle:
			//            borderMergedRegion = topBorderMergedRegion;
			//            borderCellFormat = topBorderCellFormat;

			//            // MD 3/15/12 - TFS104581
			//            //owner = this;
			//            //// MD 9/28/11 - TFS88683
			//            //adjacentOwner = rowAbove;
			//            ownerFormat = currentRowFormat;
			//            adjacentOwnerFormat = rowAboveFormat;

			//            break;

			//        case CellFormatValue.LeftBorderColorInfo:
			//        case CellFormatValue.LeftBorderStyle:
			//            borderMergedRegion = leftBorderMergedRegion;
			//            borderCellFormat = leftBorderCellFormat;

			//            // MD 3/15/12 - TFS104581
			//            //owner = column;
			//            //// MD 9/28/11 - TFS88683
			//            //adjacentOwner = columnToLeft;
			//            ownerFormat = column.CellFormat;
			//            adjacentOwnerFormat = columnToLeftFormat;

			//            break;

			//        case CellFormatValue.RightBorderColorInfo:
			//        case CellFormatValue.RightBorderStyle:
			//            borderMergedRegion = rightBorderMergedRegion;
			//            borderCellFormat = rightBorderCellFormat;

			//            // MD 3/15/12 - TFS104581
			//            //owner = column;
			//            //// MD 9/28/11 - TFS88683
			//            //adjacentOwner = columnToRight;
			//            ownerFormat = column.CellFormat;
			//            adjacentOwnerFormat = columnToRightFormat;

			//            break;

			//        default:
			//            // MD 10/24/11 - TFS91505
			//            // If the merged region doesn't have the value set, get it from the row and then column instead because the cells
			//            // should now always have their fully resolved format stored on them.
			//            //if (associatedMergedCellsRegion != null && associatedMergedCellsRegion.HasCellFormat)
			//            //{
			//            //    cellFormat.SetValue(valueToSync, associatedMergedCellsRegion.CellFormatInternal.GetValue(valueToSync));
			//            //}
			//            // MD 1/8/12 - 12.1 - Cell Format Updates
			//            // This is rewritten based on the fact that cells in merged regions only pick up format properties from the parent
			//            // row/column when the merged region is one cell tall/wide.
			//            #region Old Code

			//            //if (associatedMergedCellsRegion != null && 
			//            //    associatedMergedCellsRegion.HasCellFormat && 
			//            //    associatedMergedCellsRegion.CellFormatInternal.IsValueDefault(valueToSync) == false)
			//            //{
			//            //    Utilities.CopyCellFormatValue(associatedMergedCellsRegion.CellFormatInternal, cellFormat, valueToSync);
			//            //}
			//            //else if (this.HasCellFormat && 
			//            //    this.CellFormatInternal.IsValueDefault(valueToSync) == false)
			//            //{
			//            //    Utilities.CopyCellFormatValue(this.CellFormatInternal, cellFormat, valueToSync);
			//            //}
			//            //else if (column != null &&
			//            //    column.HasCellFormat &&
			//            //    column.CellFormatInternal.IsValueDefault(valueToSync) == false)
			//            //{
			//            //    Utilities.CopyCellFormatValue(column.CellFormatInternal, cellFormat, valueToSync);
			//            //}

			//            #endregion // Old Code
			//            if (associatedMergedCellsRegion != null)
			//            {
			//                if (WorksheetRow.ResolveCellFormatPropertyFromOwner(cellFormat, associatedMergedCellsRegion, valueToSync))
			//                    continue;

			//                // The cells in a merged region only pick up props from the parent row when the merged cell height is 1.
			//                if (associatedMergedCellsRegion.Height == 1 &&
			//                    WorksheetRow.ResolveCellFormatPropertyFromOwner(cellFormat, this, valueToSync))
			//                    continue;

			//                // The cells in a merged region only pick up props from the parent column when the merged cell width is 1.
			//                if (associatedMergedCellsRegion.Width == 1 &&
			//                    column != null &&
			//                    WorksheetRow.ResolveCellFormatPropertyFromOwner(cellFormat, column.CellFormat, valueToSync))
			//                    continue;
			//            }
			//            else
			//            {
			//                // MD 3/1/12 - 12.1 - Table Support
			//                // The table and column area formats have higher priority, so sync with them first.
			//                if (associatedTableColumnAreaFormat != null)
			//                {
			//                    if (WorksheetRow.ResolveCellFormatPropertyFromOwner(cellFormat, associatedTableColumnAreaFormat.Element, valueToSync))
			//                        continue;
			//                }

			//                if (associatedTableAreaFormat != null)
			//                {
			//                    if (WorksheetRow.ResolveCellFormatPropertyFromOwner(cellFormat, associatedTableAreaFormat.Element, valueToSync))
			//                        continue;
			//                }


			//                if (WorksheetRow.ResolveCellFormatPropertyFromOwner(cellFormat, this, valueToSync))
			//                    continue;

			//                if (column != null && WorksheetRow.ResolveCellFormatPropertyFromOwner(cellFormat, column.CellFormat, valueToSync))
			//                    continue;
			//            }
			//            continue;
			//    }

			//    // MD 3/15/12 - TFS104581
			//    //// Copy the border property from the owning row or column.
			//    //if (owner != null && owner.HasCellFormat)
			//    //    Utilities.CopyCellFormatValue(owner.CellFormatInternal, cellFormat, valueToSync);
			//    //// MD 9/28/11 - TFS88683
			//    //// If we couldn't get the border properties from the owning row or column, try to get them from the adjacent row or column.
			//    //else if (adjacentOwner != null && adjacentOwner.HasCellFormat)
			//    //    cellFormat.SetValue(valueToSync, adjacentOwner.CellFormatInternal.GetValue(Utilities.GetOppositeBorderValue(valueToSync)));
			//    // Copy the border property from the owning row or column.
			//    if (ownerFormat != null)
			//        Utilities.CopyCellFormatValue(ownerFormat, cellFormat, valueToSync);
			//    else if (adjacentOwnerFormat != null)
			//        cellFormat.SetValue(valueToSync, adjacentOwnerFormat.GetValue(Utilities.GetOppositeBorderValue(valueToSync)));

			//    // If the border cell is part of a merged region, we will sync with the merged region's format instead.
			//    if (borderMergedRegion != null)
			//    {
			//        // However, only sync if the current cell is not within the same merged region as the border cell and the border cell's
			//        // merged region has a cell format.
			//        if (associatedMergedCellsRegion != borderMergedRegion && borderMergedRegion.HasCellFormat)
			//        {
			//            cellFormat.SetValue(valueToSync, borderMergedRegion.CellFormatInternal.GetValue(Utilities.GetOppositeBorderValue(valueToSync)));
			//        }
			//    }
			//    else if (borderCellFormat != null)
			//    {
			//        cellFormat.SetValue(valueToSync, borderCellFormat.GetValue(Utilities.GetOppositeBorderValue(valueToSync)));
			//    }
			//}

			#endregion // Old Code
			Worksheet worksheet = this.Worksheet;
			WorksheetColumnBlock columnBlock = worksheet.GetColumnBlock(columnIndex);
			WorksheetCellFormatProxy associatedTableColumnAreaFormat = this.GetAssociatedColumnAreaFormat(columnIndex);
			foreach (CellFormatValue valueToSync in WorksheetCellFormatData.AllCellFormatValues)
			{
				// We should not sync this property
				if (valueToSync == CellFormatValue.FormatOptions)
					continue;

				if (associatedTableColumnAreaFormat != null &&
					WorksheetRow.ResolveCellFormatPropertyFromOwner(cellFormat, associatedTableColumnAreaFormat.Element, valueToSync))
					continue;

				if (Utilities.IsVerticalBorderValue(valueToSync) == false &&
					WorksheetRow.ResolveCellFormatPropertyFromOwner(cellFormat, this, valueToSync))
					continue;

				if (Utilities.IsHorizontalBorderValue(valueToSync) == false &&
					WorksheetRow.ResolveCellFormatPropertyFromOwner(cellFormat, columnBlock.CellFormat, valueToSync))
					continue;
			}
		}

		#endregion // InitializeDefaultCellProxyData

		// MD 4/12/11 - TFS67084
		#region InitializeDefaultCommentBounds

		private void InitializeDefaultCommentBounds(short columnIndex, WorksheetCellComment comment)
		{
			const int DefaultTopPaddingAboveCellTop = 150;
			const int DefaultBottomPaddingBelowCellTopIfCommentLow = 120;
			const int DefaultWidth = 2160;
			const int DefaultHeight = 1185;
			const int HorizontalPadding = 240;
			const int EdgePadding = 30;

			// MD 7/26/10 - TFS34398
			// Now that the row is stored on the cell, the Worksheet getter is now a bit slower, so cache it.
			//WorkbookFormat currentFormat = this.Worksheet.CurrentFormat;
			Worksheet worksheet = this.Worksheet;
			WorkbookFormat currentFormat = worksheet.CurrentFormat;

			int maxRowCount = Workbook.GetMaxRowCount(currentFormat);
			int maxColumnCount = Workbook.GetMaxColumnCount(currentFormat);

			// Determine the bounds of this cell.
			Rectangle currentCellTwipsRect = this.GetCellBoundsInTwipsInternal(columnIndex);

			// Determine the default left and top edges of the comment shape, assuming we have enough room to display
			int commentLeftTwips = (int)currentCellTwipsRect.Right + HorizontalPadding;
			int commentTopTwips = Math.Max(EdgePadding, (int)currentCellTwipsRect.Top - DefaultTopPaddingAboveCellTop);

			// Find the right edge of the worksheet
			Rectangle rightMostCellBounds = worksheet.Rows[0].GetCellBoundsInTwipsInternal((short)(maxColumnCount - 1));
			int worksheetRightMostEdge = (int)rightMostCellBounds.Right;

			// If the comment will go past the right edge of the worksheet, move it to the other side of the cell.
			if (commentLeftTwips + DefaultWidth + EdgePadding > worksheetRightMostEdge)
				commentLeftTwips = (int)currentCellTwipsRect.Left - HorizontalPadding - DefaultWidth;

			// Find the bottom edge of the worksheet
			Rectangle bottomMostCellBounds = worksheet.Rows[maxRowCount - 1].GetCellBoundsInTwipsInternal(0);
			int worksheetBottomMostEdge = (int)bottomMostCellBounds.Bottom;

			// If the comment will go past the bottom edge of the worksheet, shift it up slightly. Prefer to have the bottom edge of the 
			// comment 120 twips below the top edge of the cell's row, but if that is also below the worksheet, leave the bottom 30 twips 
			// off the bottom edge.
			if (commentTopTwips + DefaultHeight + EdgePadding > worksheetBottomMostEdge)
			{
				int bottomTwips = (int)Math.Min(worksheetBottomMostEdge - EdgePadding, currentCellTwipsRect.Top + DefaultBottomPaddingBelowCellTopIfCommentLow);
				commentTopTwips = bottomTwips - DefaultHeight;
			}

			Rectangle commentTwips = new Rectangle(
				commentLeftTwips,
				commentTopTwips,
				DefaultWidth,
				DefaultHeight);

			// Set the new bounds of the comment.
			// MD 7/26/10 - TFS34398
			// Use the cached worksheet.
			//comment.SetBoundsInTwips( this.worksheet, commentTwips );
			comment.SetBoundsInTwips(worksheet, commentTwips);
		}

		#endregion InitializeDefaultCommentBounds

		// MD 4/12/11 - TFS67084
		#region InitSerializationCacheForCell



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		// MD 1/10/12 - 12.1 - Cell Format Updates
		// We don't need to pass as much stuff here anymore.
		//private void InitSerializationCacheForCell(
		//    CellDataContext cellDataContext,
		//    WorkbookSerializationManager serializationManager,
		//    WorksheetCellFormatData rowFormatData,
		//    WorksheetCellFormatData emptyFormatData,
		//    out short indexInFormatCollection)
		private void InitSerializationCacheForCell(
			CellDataContext cellDataContext,
			WorkbookSerializationManager serializationManager)
		{
			// MD 7/26/10 - TFS34398
			// Now that the row is stored on the cell, the Worksheet getter is now a bit slower, so cache it.
			Worksheet worksheet = this.Worksheet;
			Workbook workbook = worksheet.Workbook;

			// MD 5/10/12 - TFS111420
			// We can no longer initialize the cell formats at this point because the formats collection is locked.
			// Instead, we will initialize the cell formats when the cell's value is set.
			#region Removed

			//#region Resolve the cell format

			//// MD 1/10/12 - 12.1 - Cell Format Updates
			//// We don't need to do a lot of the resolution stuff anymore. All we need to do is force the cell's format to get created if it is part of a merged region.
			//#region Old Code

			////if (cellDataContext.CellFormatData == null)
			////{
			////    cellDataContext.CellFormatData = this.GetResolvedCellFormatHelper(
			////        cellDataContext.ColumnIndex,
			////        worksheet,
			////        emptyFormatData,
			////        rowFormatData,
			////        emptyFormatData);
			////}
			////
			////WorksheetCellFormatData resolvedFormat;
			////if (serializationManager.ResolvedCellFormatsByFormat.TryGetValue(cellDataContext.CellFormatData, out resolvedFormat) == false)
			////{
			////    resolvedFormat = cellDataContext.CellFormatData.ResolvedCellFormatData();
			////    serializationManager.ResolvedCellFormatsByFormat[cellDataContext.CellFormatData] = resolvedFormat;
			////
			////    if (serializationManager.AddResolvedFormat(resolvedFormat, true) == false)
			////        resolvedFormat = serializationManager.Formats[resolvedFormat.IndexInFormatCollection];
			////}
			////
			////if (Utilities.Is2003Format(workbook.CurrentFormat))
			////{
			////    Debug.Assert(resolvedFormat.IndexInFormatCollection <= short.MaxValue, "We need more than 2 bytes for the indexInFormatCollection");
			////    indexInFormatCollection = (short)resolvedFormat.IndexInFormatCollection;
			////}
			////else
			////{
			////    Debug.Assert(resolvedFormat.IndexInXfsCollection <= short.MaxValue, "We need more than 2 bytes for the indexInFormatCollection");
			////    indexInFormatCollection = (short)resolvedFormat.IndexInXfsCollection;
			////}

			//#endregion // Old Code
			//if (cellDataContext.CellFormatData == null)
			//{
			//    bool initializeFormat = false;

			//    // MD 3/15/12 - TFS104581
			//    //WorksheetColumn column = this.Worksheet.Columns.GetIfCreated(cellDataContext.ColumnIndex);
			//    //// MD 3/2/12 - 12.1 - Table Support
			//    ////if (column != null && column.HasCellFormat && column.CellFormatInternal.HasDefaultValue == false)
			//    //if (column != null && column.HasCellFormat && column.CellFormatInternal.IsEmpty == false)
			//    WorksheetColumnBlock columnBlock = this.Worksheet.GetColumnBlock(cellDataContext.ColumnIndex);
			//    if (columnBlock.CellFormat.IsEmpty == false)
			//        initializeFormat = true;
			//    else if (this.GetCellAssociatedMergedCellsRegionInternal(cellDataContext.ColumnIndex) != null)
			//        initializeFormat = true;

			//    if (initializeFormat)
			//    {
			//        // TODO_Perf_12_1: This might be unnecessarily creating a proxy which is then discarded (but is needed to get the element in the cell formats collection and match it with duplicates). See if there is a better way to do it.
			//        cellDataContext.CellFormatData = this.GetCellFormatInternal(cellDataContext.ColumnIndex).Element;
			//    }
			//}

			//#endregion Resolve the cell format

			#endregion // Removed

			#region Store string cell values in the shared string table

			// We can't store the StringBuilders in the shared string table because they are mutable and we can't get notified when they are changed,
			// so we need to add the manually here.
			// MD 1/10/12 - 12.1
			// Found while doing Cell Format Updates
			// We should also check that a value actually exists before doing this.
			//if (cellDataContext.CellBlock != null)
			if (cellDataContext.HasValue && cellDataContext.CellBlock != null)
			{
				StringBuilder stringBuilder = cellDataContext.CellBlock.GetCellValueIfStringBuilder(this, cellDataContext.ColumnIndex);

				if (stringBuilder != null)
				{
					if (serializationManager.AdditionalStringsInStringTable.ContainsKey(stringBuilder) == false)
					{
						// MD 2/2/12 - TFS100573
						// The string element no longer has a reference to the Workbook.
						//StringElement element = new StringElement(workbook, stringBuilder.ToString());
						StringElement element = new StringElement(stringBuilder.ToString());

						StringElement foundElement = workbook.SharedStringTable.Find(element);

						int index = 0;
						if (foundElement != null)
						{
							// MD 2/1/12 - TFS100573
							// The index is no longer stored on the element.
							//index = foundElement.IndexInStringTable;
							index = workbook.SharedStringTable.FindStringIndex(foundElement);
						}
						else
						{
							// MD 2/1/12 - TFS100573
							// The index is no longer stored on the element and the manager's SharedStringTable only contains 
							// StringBuilder strings when saving now.
							//index = serializationManager.SharedStringTable.Count;
							//element.IndexInStringTable = index;
							index = serializationManager.SharedStringCountDuringSave;

							// MD 5/10/12 - TFS111420
							// We can no longer initialize the cell formats at this point because the formats collection is locked.
							// Instead, we will initialize the cell formats when the cell's value is set.
							#region Removed

							//// MD 1/18/12 - 12.1 - Cell Format Updates
							////element.InitSerializationCache(serializationManager);
							//if (cellDataContext.CellFormatData == null)
							//{
							//    // TODO_Perf_12_1: This might be unnecessarily creating a proxy which is then discarded (but is needed to get the element in the cell formats collection and match it with duplicates). See if there is a better way to do it.
							//    cellDataContext.CellFormatData = this.GetCellFormatInternal(cellDataContext.ColumnIndex).Element;
							//}

							#endregion // Removed

							element.InitSerializationCache(serializationManager, cellDataContext.CellFormatData);

							serializationManager.SharedStringTable.Add(element);
						}

						serializationManager.AdditionalStringsInStringTable[stringBuilder] = index;
					}

					// Keep a count of the total number of strings used in the document
					serializationManager.TotalStringsUsedInDocument++;
				}
			}

			#endregion Store string cell values in the shared string table
		}

		#endregion InitSerializationCacheForCell

		// MD 1/31/12 - TFS100573
		#region InsertCellBlock

		private void InsertCellBlock(int index, WorksheetCellBlock cellBlock)
		{
			if (this.cellBlocks == null)
			{
				Debug.Assert(index == 0, "This is unexpected.");
				this.cellBlocks = new WorksheetCellBlock[] { cellBlock };
				return;
			}

			if (this.cellBlocks[this.cellBlocks.Length - 1] == null)
			{
				Array.Copy(this.cellBlocks, index, this.cellBlocks, index + 1, this.cellBlocks.Length - index - 1);
				this.cellBlocks[index] = cellBlock;
				return;
			}

			WorksheetCellBlock[] temp = new WorksheetCellBlock[this.cellBlocks.Length * 2];

			Array.Copy(this.cellBlocks, temp, index);
			temp[index] = cellBlock;
			Array.Copy(this.cellBlocks, index, temp, index + 1, this.cellBlocks.Length - index);
			this.cellBlocks = temp;
		}

		#endregion // InsertCellBlock

		// MD 1/8/12 - 12.1 - Cell Format Updates
		#region ResolveCellFormatPropertyFromOwner

		private static bool ResolveCellFormatPropertyFromOwner(
			WorksheetCellFormatData cellFormat,
			ICellFormatOwner owner,
			CellFormatValue valueToSync)
		{
			if (owner.HasCellFormat == false)
				return false;

			return WorksheetRow.ResolveCellFormatPropertyFromOwner(cellFormat, owner.CellFormatInternal.Element, valueToSync);
		}

		private static bool ResolveCellFormatPropertyFromOwner(
			WorksheetCellFormatData cellFormat,
			WorksheetCellFormatData ownerCellFormat,
			CellFormatValue valueToSync)
		{
			if (WorksheetCellFormatData.IsValueDefault(valueToSync, ownerCellFormat.GetValue(valueToSync)))
				return false;

			Utilities.CopyCellFormatValue(ownerCellFormat, cellFormat, valueToSync);
			return true;
		}

		#endregion // ResolveCellFormatPropertyFromOwner

		// MD 4/12/11 - TFS67084
		#region VerifyFormatLimitsOnCell

		// MD 3/2/12 - 12.1 - Table Support
		#region Old Code

		//private void VerifyFormatLimitsOnCell(short columnIndex, FormatLimitErrors limitErrors, WorkbookFormat testFormat)
		//{
		//    WorksheetCellFormatProxy cellFormat;
		//    this.TryGetCellFormat(columnIndex, out cellFormat);

		//    // MD 1/19/12 - 12.1 - Cell Format Updates
		//    //if (cellFormat != null)
		//    //    cellFormat.Element.VerifyFormatLimits(limitErrors, testFormat);

		//    Formula formula = this.GetCellFormulaInternal(columnIndex);

		//    if (formula != null)
		//    {
		//        CellReferenceMode cellReferenceMode = this.Worksheet.CellReferenceMode;
		//        formula.VerifyFormatLimits(limitErrors, testFormat, cellReferenceMode, true);
		//    }

		//    int maxColumnIndex = Workbook.GetMaxColumnCount(testFormat) - 1;

		//    if (maxColumnIndex < columnIndex)
		//        limitErrors.AddError(String.Format(SR.GetString("LE_FormatLimitError_MaxColumnIndex"), columnIndex, maxColumnIndex));
		//}

		#endregion // Old Code
		private void VerifyFormatLimitsOnCell(Workbook workbook, CellDataContext cellDataContext, FormatLimitErrors limitErrors, WorkbookFormat testFormat)
		{
			int maxColumnIndex = Workbook.GetMaxColumnCount(testFormat) - 1;
			if (maxColumnIndex < cellDataContext.ColumnIndex)
				limitErrors.AddError(String.Format(SR.GetString("LE_FormatLimitError_MaxColumnIndex"), cellDataContext.ColumnIndex, maxColumnIndex));

			if (cellDataContext.HasValue && cellDataContext.CellBlock != null)
			{
				Formula formula = cellDataContext.CellBlock.GetCellValueRaw(this, cellDataContext.ColumnIndex) as Formula;
				if (formula != null)
				{
					CellReferenceMode cellReferenceMode = this.Worksheet.CellReferenceMode;
					formula.VerifyFormatLimits(workbook, limitErrors, testFormat, cellReferenceMode, true);
				}
			}
		}

		#endregion VerifyFormatLimitsOnCell

		#endregion  // Private Methods

		#endregion Methods

		#region Properties

		#region Public Properties

		#region Cells

		/// <summary>
		/// Gets the collection of cells in the row.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The collection of cells is a fixed length collection, with the maximum number of cells in the collection being 
		/// <see cref="Workbook.MaxExcelColumnCount"/> or <see cref="Workbook.MaxExcel2007ColumnCount"/>,
		/// depending on the <see cref="Workbook.CurrentFormat">Workbook.CurrentFormat</see>.  Internally, the cells 
		/// are only created and added to the collection when they are requested.
		/// </p>
		/// <p class="note">
		/// <b>Note:</b> Iterating the collection will not create all cells. It will only iterate the cells which have already 
		/// been used.  To create and iterate all cells in the worksheet use a For loop, iterating from 0 to one less than 
		/// MaxExcelColumnCount, and pass in each index to the collection's indexer.
		/// </p>
		/// </remarks>
		/// <value>The collection of cells in the row.</value>
		public WorksheetCellCollection Cells
		{
			get
			{
				if ( this.cells == null )
					this.cells = new WorksheetCellCollection( this );

				return this.cells;
			}
		}

		// MD 4/12/11 - TFS67084
		// Moved away from using WorksheetCell objects.
		//internal bool HasCells
		//{
		//    get 
		//    {
		//        return 
		//            this.cells != null && 
		//            this.cells.Count > 0; 
		//    }
		//}

		#endregion Cells

		#region Height

		/// <summary>
		/// Gets or sets the height of the row in twips (1/20th of a point).
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// If the height of the row is less than zero, the <see cref="Worksheet.DefaultRowHeight"/> of the
		/// owning worksheet will be used as the row's height.
		/// </p>
		/// <p class="body">
		/// The value assigned must be between -1 and 8192. Invalid values will be automatically adjusted to valid values.
		/// </p>
		/// </remarks>
		/// <value>The height of the row in twips (1/20th of a point).</value>
		public int Height
		{
			get { return this.height; }
			set 
			{
				if ( this.height != value )
				{
					// MD 7/3/07 - BR24403
					// This exception breaks backwards compatibility. Now the invalid value is "fixed". Comments have been updated.
					//if ( value < 0 || 8192 < value )
					//    throw new ArgumentOutOfRangeException( "value", value, SR.GetString( "LE_ArgumentOutOfRangeException_RowHeight" ) );
					// MD 3/5/10 - TFS26342
					// We have to allow the user to set the default value back on the property.
					//if ( value < 0 )
					//    value = 0;
					if (value < -1)
						value = -1;

					if ( 8192 < value )
						value = 8192;

					// MD 3/5/10 - TFS26342
					// Notify the worksheet before the resize occurs and cache the old extent of this row.
					// MD 7/23/10 - TFS35969
					// The OnBeforeWorksheetElementResize method now takes the element being resized.
					//this.Worksheet.OnBeforeWorksheetElementResize();
					this.Worksheet.OnBeforeWorksheetElementResize(this);

					int oldExtentInTwipsHiddenIgnored = this.GetExtentInTwips(true);

					// MD 3/16/09 - TFS14252
					// Only 2 bytes are needed to describe the height. To cut down on the memory fottprint, 
					// this has been changed to a short.
					//this.height = value;
					this.height = (short)value;

					// MD 7/23/10 - TFS35969
					// When the row height changes, reset the height cache on it's node.
					this.Worksheet.Rows.ResetHeightCache(this.Index, false);

					// MD 3/5/10 - TFS26342
					// Notify the worksheet after the resize occurs.
					this.Worksheet.OnAfterWorksheetElementResized(this, oldExtentInTwipsHiddenIgnored, this.Hidden);
				}
			}
		}

		#endregion Height

		#endregion Public Properties

		#region Internal Properties

		#region Removed

		//#region FirstCell

		//internal int FirstCell
		//{
		//    get { return this.firstCell; }
		//}

		//#endregion FirstCell

		//#region FirstCellInUndefinedTail

		//internal int FirstCellInUndefinedTail
		//{
		//    get { return this.firstCellInUndefinedTail; }
		//}

		//#endregion FirstCellInUndefinedTail

		//#region HasCollapseIndicator

		//internal bool HasCollapseIndicator
		//{
		//    get { return this.hasCollapseIndicator; }
		//}

		//#endregion HasCollapseIndicator

		//#region HasData

		//internal bool HasData
		//{
		//    get { return this.hasData; }
		//    set { this.hasData = value; }
		//}

		//#endregion HasData 

		#endregion  // Removed

		// MD 1/31/12 - TFS100573
		// This is no longer needed.
		#region Removed

		//// MD 4/12/11 - TFS67084
		//#region CellBlocks

		//internal SortedList<short, WorksheetCellBlock> CellBlocks
		//{
		//    get
		//    {
		//        if (this.cellBlocks == null)
		//            this.cellBlocks = new SortedList<short, WorksheetCellBlock>();

		//        return this.cellBlocks;
		//    }
		//}

		//internal bool HasCellBlocks
		//{
		//    get { return this.cellBlocks != null && this.cellBlocks.Count > 0; }
		//} 

		//#endregion  // CellBlocks

		#endregion // Removed

		// MD 4/12/11 - TFS67084
		#region CellCalcReferences

		private SortedList<short, CellCalcReference[]> CellCalcReferences
		{
			get
			{
				if (this.cellCalcReferences == null)
					this.cellCalcReferences = new SortedList<short, CellCalcReference[]>();

				return this.cellCalcReferences;
			}
		}

		internal bool HasCellCalcReferences
		{
			get { return this.cellCalcReferences != null && this.cellCalcReferences.Count > 0; }
		} 

		#endregion  // CellCalcReferences

		// MD 4/12/11 - TFS67084
		#region CellFormatsForCells

		internal SortedList<short, WorksheetCellFormatData[]> CellFormatsForCells
		{
			get
			{
				if (this.cellFormatsForCells == null)
					this.cellFormatsForCells = new SortedList<short, WorksheetCellFormatData[]>();

				return this.cellFormatsForCells;
			}
		}

		internal bool HasCellFormatsForCells
		{
			get { return this.cellFormatsForCells != null && this.cellFormatsForCells.Count > 0; }
		}

		#endregion  // CellFormatsForCells

		// MD 4/12/11 - TFS67084
		#region CellOwnedFormulas

		internal List<Formula> CellOwnedFormulas
		{
			get
			{
				if (this.cellOwnedFormulas == null)
					this.cellOwnedFormulas = new List<Formula>();

				return this.cellOwnedFormulas;
			}
		} 

		#endregion  // CellOwnedFormulas

		#region Removed

		// MD 4/12/11 - TFS67084
		#region SharedStringTable

		internal GenericCachedCollection<StringElement> SharedStringTable
		{
			get
			{
				Workbook workbook = this.Worksheet.Workbook;

				if (workbook == null)
					return null;

				return workbook.SharedStringTable;
			}
		}

		#endregion  // SharedStringTable

		#endregion // Removed

		#endregion Internal Properties

		#endregion Properties


		// MD 6/6/11 - TFS78116
		// Moved this code from the WorksheetRow.CalculateHeightWithWrappedText method so it can be called on the UI thread.


#region Infragistics Source Cleanup (Region)















































































#endregion // Infragistics Source Cleanup (Region)

	}

	internal class WorksheetRowSerializationCache
	{
		internal bool hasCollapseIndicator;
		internal bool hasData;
		internal short firstCell;
		internal short firstCellInUndefinedTail;

		// MD 1/10/12 - 12.1 - Cell Format Updates
		// These are no longer needed because we are not cached format indexes.
		//// MD 4/18/11 - TFS62026
		//internal short nextCellFormatIndex;
		//internal short[] cellFormatIndexValues;
	}

	// MD 4/18/11 - TFS62026
	#region CellDataContext class

	internal class CellDataContext
	{
		private WorksheetCellBlock cellBlock;
		private WorksheetCellFormatData cellFormatData;

		// MD 2/23/12 - 12.1 - Table Support
		private bool cellFormatDataWasCleared;

		private short columnIndex;

		// MD 1/9/12 - 12.1 - Cell Format Updates
		private bool hasFormat;
		private bool hasValue;

		// MD 1/9/12 - 12.1 - Cell Format Updates
		//public CellDataContext(short columnIndex, WorksheetCellBlock cellBlock, WorksheetCellFormatData cellFormatData)
		public CellDataContext(short columnIndex, WorksheetCellBlock cellBlock, WorksheetCellFormatData cellFormatData, bool hasFormat, bool hasValue)
		{
			this.columnIndex = columnIndex;
			this.cellBlock = cellBlock;
			this.cellFormatData = cellFormatData;

			// MD 1/9/12 - 12.1 - Cell Format Updates
			this.hasFormat = hasFormat;
			this.hasValue = hasValue;
		}

		public WorksheetCellBlock CellBlock
		{
			get { return this.cellBlock; }
			// MD 2/23/12 - 12.1 - Table Support
			set { this.cellBlock = value; }
		}

		public WorksheetCellFormatData CellFormatData
		{
			get { return this.cellFormatData; }
			// MD 2/23/12 - 12.1 - Table Support
			//set { this.cellFormatData = value; }
			set
			{
				if (value == null)
				{
					Debug.Assert(this.cellFormatData != null, "This is unexpected.");
					this.cellFormatDataWasCleared = true;
				}
				else
				{
					Debug.Assert(this.cellFormatData == null, "This is unexpected.");
				}

				this.cellFormatData = value;
			}
		}

		// MD 2/23/12 - 12.1 - Table Support
		public bool CellFormatDataWasCleared
		{
			get { return this.cellFormatDataWasCleared; }
		}

		public short ColumnIndex
		{
			get { return this.columnIndex; }
		}

		// MD 1/9/12 - 12.1 - Cell Format Updates
		public bool HasFormat
		{
			get { return this.hasFormat; }
		}

		// MD 1/9/12 - 12.1 - Cell Format Updates
		public bool HasValue
		{
			get { return this.hasValue; }
		}
	}

	#endregion // CellDataContext class

	// MD 4/12/11 - TFS67084
	#region CellFormatContext class

	internal class CellFormatContext
	{
		private WorksheetCellFormatData[] cellFormatBlock;
		private WorksheetCellFormatData cellFormatData;
		private short columnIndex;

		public CellFormatContext(short columnIndex, WorksheetCellFormatData cellFormatData, WorksheetCellFormatData[] cellFormatBlock)
		{
			this.columnIndex = columnIndex;
			this.cellFormatData = cellFormatData;
			this.cellFormatBlock = cellFormatBlock;
		}

		public WorksheetCellFormatProxy GetProxy(WorksheetRow row)
		{
			// MD 3/1/12 - 12.1 - Table Support
			if (this.cellFormatData == null || this.cellFormatBlock == null)
				return row.GetCellFormatInternal(this.columnIndex);

			return row.GetCellFormatProxyFromData(this.columnIndex, this.cellFormatData, this.cellFormatBlock, true);
		}

		// MD 1/8/12 - 12.1 - Cell Format Updates
		public short ColumnIndex
		{
			get { return this.columnIndex; }
		}
	}

	#endregion // CellFormatContext class

	// MD 3/26/12 - 12.1 - Table Support
	#region GetCellTextParameters

	internal class GetCellTextParameters
	{
		public GetCellTextParameters(short columnIndex)
		{
			this.ColumnIndex = columnIndex;
		}

		public readonly short ColumnIndex;
		public PreventTextFormattingTypes PreventTextFormattingTypes;
		public TextFormatMode TextFormatMode;
		public bool? UseCalculatedValues;
	}

	#endregion // GetCellTextParameters
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