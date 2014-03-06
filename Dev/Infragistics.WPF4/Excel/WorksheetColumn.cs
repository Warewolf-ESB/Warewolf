using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.ComponentModel;
using Infragistics.Documents.Excel.Serialization;




using Infragistics.Shared;


namespace Infragistics.Documents.Excel
{
	/// <summary>
	/// Represents a column in a Microsoft Excel worksheet.
	/// </summary>
	[DebuggerDisplay( "Column: Index - {Index}" )]



	public

		 class WorksheetColumn : RowColumnBase
	{
		#region Member Variables

		// MD 3/15/12 - TFS104581
		// Moved these members to the WorksheetColumnBlock
		#region Moved

		//private int width = -1;

		//#region Serialization Cache

		//// This is only valid when the column's worksheet is about to be saved
		//private bool hasCollapseIndicator;
		//private bool hasData;

		//#endregion Serialization Cache

		#endregion // Moved

		// MD 3/15/12 - TFS104581
		// Moved the index from the base class so we could store it as a short on columns.
		private readonly short index;

		#endregion Member Variables

		#region Constructor

		// MD 3/15/12 - TFS104581
		//internal WorksheetColumn( Worksheet worksheet, int index )
		//    : base( worksheet, index ) {}
		internal WorksheetColumn(Worksheet worksheet, short index)
			: base(worksheet)
		{
			this.index = index;
		}

		#endregion Constructor

		#region Base Class Overrides

		// MD 3/15/12 - TFS104581
		#region CreateCellFormatProxy

		internal override WorksheetCellFormatProxy CreateCellFormatProxy(GenericCachedCollectionEx<WorksheetCellFormatData> cellFormatCollection)
		{
			return new WorksheetColumnOwnedFormatProxy(cellFormatCollection, this);
		}

		#endregion // CreateCellFormatProxy

		// MD 3/22/12 - TFS104630
		#region GetAdjacentFormatForBorderResolution

		internal override WorksheetCellFormatData GetAdjacentFormatForBorderResolution(WorksheetCellFormatProxy sender, CellFormatValue borderValue)
		{
			switch (borderValue)
			{
				case CellFormatValue.TopBorderColorInfo:
				case CellFormatValue.TopBorderStyle:
				case CellFormatValue.BottomBorderColorInfo:
				case CellFormatValue.BottomBorderStyle:
					return null;

				case CellFormatValue.LeftBorderColorInfo:
				case CellFormatValue.LeftBorderStyle:
					if (this.Index == 0)
						return null;

					return this.Worksheet.GetColumnBlock((short)(this.IndexInternal - 1)).CellFormat;

				case CellFormatValue.RightBorderColorInfo:
				case CellFormatValue.RightBorderStyle:
					if (this.Index == this.Worksheet.Columns.MaxCount - 1)
						return null;

					return this.Worksheet.GetColumnBlock((short)(this.IndexInternal + 1)).CellFormat;

				default:
					Utilities.DebugFail("Unknown edge border value.");
					return null;
			}
		}

		#endregion // GetAdjacentFormatForBorderResolution

		// MD 3/5/10 - TFS26342
		#region GetExtentInTwips



#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

		internal override int GetExtentInTwips(bool ignoreHidden)
		{
			return this.Worksheet.GetColumnWidthInTwips(this.Index, ignoreHidden);
		}

		#endregion // GetExtentInTwips

		// MD 3/15/12 - TFS104581
		// Moved this to the WorksheetColumnBlock.
		#region Moved

		//        #region HasDataIgnoreHidden

		//#if DEBUG
		//        /// <summary>
		//        /// Gets the value indicating whether this row or column should be saved 
		//        /// (if it has data) without considering the hidden state of the row or column.
		//        /// </summary>  
		//#endif
		//        internal override bool HasDataIgnoreHidden
		//        {
		//            get
		//            {
		//                if ( base.HasDataIgnoreHidden )
		//                    return true;

		//                //if ( this.width >= 0 )
		//                if (this.Width >= 0)
		//                    return true;

		//                if ( this.hasCollapseIndicator )
		//                    return true;

		//                return false;
		//            }
		//        }

		//        #endregion HasDataIgnoreHidden

		#endregion // Moved

		// MD 3/15/12 - TFS104581
		#region HiddenInternal

		internal override bool HiddenInternal
		{
			get { return this.Worksheet.GetColumnBlock(this.index).Hidden; }
			set
			{
				if (this.Hidden == value)
					return;

				this.OnBeforeColumnChange();
				this.Worksheet.GetColumnBlock(this.index).Hidden = value;
				this.OnAfterColumnChange();
			}
		}

		#endregion // HiddenInternal

		// MD 3/15/12 - TFS104581
		#region Index

		/// <summary>
		/// Gets the 0-based index of the column in the worksheet.
		/// </summary>
		/// <value>The 0-based index of the column in the worksheet.</value>
		public override int Index
		{
			get { return this.index; }
		}

		#endregion // Index

		// MD 5/12/10 - TFS26732
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
			// MD 1/18/11 - TFS62762
			// If one of the format values affecting the height of cells with wrapped text has changed, 
			// we should reset the calculated height of the rows which intersect with this column, because 
			// the heights might change.
			// MD 4/18/11 - TFS62026
			// The proxy will now pass along all values that changed in one operation as opposed to one at a time.
			//if (WorksheetRow.DoesAffectCachedHeightWithWrappedText(value))
			if (WorksheetRow.DoesAffectCachedHeightWithWrappedText(values))
			{
				foreach (WorksheetRow row in this.Worksheet.Rows)
				{
					// MD 4/12/11 - TFS67084
					// Moved away from using WorksheetCell objects.
					//if (row.HasCells == false)
					//    continue;
					//
					// The height of the cell can only change if there is a value in it, so if the value is null, skip it.
					//if (row.Cells.GetIfCreated(this.Index) == null)
					//    continue;
					if (row.GetCellValueRaw((short)this.Index) == null)
						continue;

					// If there is a cell at the intersection of the row and this column, it's height could 
					// change based on the new format value, so clear the row's cached height.
					row.ResetCachedHeightWithWrappedText();
				}
			}

			// MD 4/18/11 - TFS62026
			// Refactored this code to account for multiple values being passed in as opposed to one.
			#region Refactored

			//// If the value was change to the default, we don't have to push anything down to the cells.
			//if (this.CellFormatInternal.IsValueDefault(value))
			//    return;
			//
			//// MD 4/12/11 - TFS67084
			//// Cache the casted columnIndex
			//short columnIndex = (short)this.Index;
			//
			//switch (value)
			//{
			//    case CellFormatValue.LeftBorderColor:
			//    case CellFormatValue.LeftBorderStyle:
			//        foreach (WorksheetRow row in this.Worksheet.Rows)
			//        {
			//            // MD 4/12/11 - TFS67084
			//            // Moved away from using WorksheetCell objects.
			//            //WorksheetCell cell = row.Cells.GetIfCreated(this.Index);
			//            //
			//            //// Copy the left and right border properties to the cells.
			//            //if (cell != null && cell.HasCellFormat)
			//            //    Utilities.CopyCellFormatValue(this.CellFormatInternal, cell.CellFormatInternal, value);
			//            // Copy the left and right border properties to the cells.
			//            WorksheetCellFormatProxy cellFormat;
			//            if (row.HasCellFormatForCellResolved(columnIndex, out cellFormat))
			//                Utilities.CopyCellFormatValue(this.CellFormatInternal, cellFormat, value);
			//
			//            // MD 10/21/10 - TFS34398
			//            // If we should prevent syncing the borders of adjacent cells, skip the next block of code.
			//            if ((options & CellFormatValueChangedOptions.PreventAdjacentBorderSyncronization) != 0)
			//                break;
			//
			//            // Reset properties for the border of adjacent cells which have the border common with this column.
			//            // MD 4/12/11 - TFS67084
			//            // Moved away from using WorksheetCell objects.
			//            //if (this.Index > 0)
			//            //{
			//            //    cell = row.Cells.GetIfCreated(this.Index - 1);
			//            //
			//            //    if (cell != null && cell.HasCellFormat)
			//            //        cell.CellFormatInternal.ResetValue(Utilities.GetOppositeBorderValue(value));
			//            //}
			//            if (columnIndex > 0)
			//            {
			//                short previousColumnIndex = (short)(columnIndex - 1);
			//                if (row.HasCellFormatForCellResolved(previousColumnIndex, out cellFormat))
			//                    cellFormat.ResetValue(Utilities.GetOppositeBorderValue(value));
			//            }
			//        }
			//        break;
			//
			//    case CellFormatValue.RightBorderColor:
			//    case CellFormatValue.RightBorderStyle:
			//        int maxColumnCount = Workbook.GetMaxColumnCount(this.Worksheet.CurrentFormat);
			//
			//        foreach (WorksheetRow row in this.Worksheet.Rows)
			//        {
			//            // MD 4/12/11 - TFS67084
			//            // Moved away from using WorksheetCell objects.
			//            //WorksheetCell cell = row.Cells.GetIfCreated(this.Index);
			//            //
			//            //// Copy the left and right border properties to the cells.
			//            //if (cell != null && cell.HasCellFormat)
			//            //    Utilities.CopyCellFormatValue(this.CellFormatInternal, cell.CellFormatInternal, value);
			//            // Copy the left and right border properties to the cells.
			//            WorksheetCellFormatProxy cellFormat;
			//            if (row.HasCellFormatForCellResolved(columnIndex, out cellFormat))
			//                Utilities.CopyCellFormatValue(this.CellFormatInternal, cellFormat, value);
			//
			//            // MD 10/21/10 - TFS34398
			//            // If we should prevent syncing the borders of adjacent cells, skip the next block of code.
			//            if ((options & CellFormatValueChangedOptions.PreventAdjacentBorderSyncronization) != 0)
			//                break;
			//
			//            // MD 4/12/11 - TFS67084
			//            // Moved away from using WorksheetCell objects.
			//            //// Reset properties for the border of adjacent cells which have the border common with this column.
			//            //if (this.Index < maxColumnCount)
			//            //{
			//            //    cell = row.Cells.GetIfCreated(this.Index + 1);
			//            //
			//            //    if (cell != null && cell.HasCellFormat)
			//            //        cell.CellFormatInternal.ResetValue(Utilities.GetOppositeBorderValue(value));
			//            //}
			//            if (columnIndex > 0)
			//            {
			//                short nextColumnIndex = (short)(columnIndex + 1);
			//                if (row.HasCellFormatForCellResolved(nextColumnIndex, out cellFormat))
			//                    cellFormat.ResetValue(Utilities.GetOppositeBorderValue(value));
			//
			//            }
			//        }
			//        break;
			//
			//    // Top and bottom borders are not used for columns, so ignore them.
			//    case CellFormatValue.BottomBorderColor:
			//    case CellFormatValue.BottomBorderStyle:
			//    case CellFormatValue.TopBorderColor:
			//    case CellFormatValue.TopBorderStyle:
			//        break;
			//
			//    default:
			//        // Copy all other values to the cells.
			//        foreach (WorksheetRow row in this.Worksheet.Rows)
			//        {
			//            // MD 4/12/11 - TFS67084
			//            // Moved away from using WorksheetCell objects.
			//            //WorksheetCell cell = row.Cells.GetIfCreated(this.Index);
			//            //
			//            //if (cell != null && cell.HasCellFormat)
			//            //    Utilities.CopyCellFormatValue(this.CellFormatInternal, cell.CellFormatInternal, value);
			//            WorksheetCellFormatProxy cellFormat;
			//            if (row.HasCellFormatForCellResolved(columnIndex, out cellFormat))
			//                Utilities.CopyCellFormatValue(this.CellFormatInternal, cellFormat, value);
			//        }
			//        break;
			//
			//} 

			#endregion  // Refactored

			// MD 4/12/11 - TFS67084
			// Cache the casted columnIndex
			short columnIndex = (short)this.Index;

			for (int i = 0; i < values.Count; i++)
			{
				CellFormatValue value = values[i];

				// If the value was changed to the default, we don't have to push anything down to the cells.
				if (this.CellFormatInternal.IsValueDefault(value))
					continue;

				// MD 9/28/11 - TFS88683
				// Cache this because we need it in almost all cases.
				object formatValue = sender.GetValue(value);

				switch (value)
				{
					// MD 3/1/12 - 12.1 - Table Support
					#region Refactored

					//case CellFormatValue.LeftBorderColorInfo:
					//case CellFormatValue.LeftBorderStyle:
					//    {
					//    // MD 9/28/11 - TFS88683
					//    // Cache this so we don't need to get it multiple times below.
					//    CellFormatValue oppositeValue = Utilities.GetOppositeBorderValue(value);

					//    // MD 9/28/11 - TFS88683
					//    // We need to sync up overlapping borders on neighboring columns.
					//    if ((options & CellFormatValueChangedOptions.PreventAdjacentBorderSyncronization) == 0 && 
					//        0 < this.Index)
					//    {
					//        WorksheetColumn neighboringColumn = this.Worksheet.Columns.GetIfCreated(this.Index - 1);
					//        if (neighboringColumn != null && neighboringColumn.HasCellFormat)
					//            neighboringColumn.CellFormatInternal.SetValue(oppositeValue, formatValue);
					//    }

					//    foreach (WorksheetRow row in this.Worksheet.Rows)
					//    {
					//        // MD 1/8/12 - 12.1 - Cell Format Updates
					//        // Changes to the column format should only be pushed to the merged cells if they are one column wide.
					//        WorksheetMergedCellsRegion associatedMergedCellRegion = row.GetCellAssociatedMergedCellsRegion(this.Index);
					//        if (associatedMergedCellRegion != null && associatedMergedCellRegion.Width != 1)
					//            continue;

					//        // MD 3/1/12 - 12.1 - Table Support
					//        #region Refactored

					//        //WorksheetCellFormatProxy cellFormat;
					//        //// MD 2/25/12 - 12.1 - Table Support
					//        ////if (row.HasCellFormatForCellResolved(columnIndex, out cellFormat))
					//        //if (row.TryGetCellFormat(columnIndex, out cellFormat))
					//        //{
					//        //    // MD 9/28/11 - TFS88683
					//        //    // Use the cached format value so we don't need to get it in the CopyCellFormatValue method.
					//        //    // Also, this will sync the borders on the adjacent cells, so we can continue here because the code below would be redundant.
					//        //    //Utilities.CopyCellFormatValue(this.CellFormatInternal, cellFormat, value);
					//        //    cellFormat.SetValue(value, formatValue, true, options);
					//        //
					//        //    // We can skip the code below for setting the adjacent border because setting the border of this 
					//        //    // cell will automatically sync the adjacent border.
					//        //    continue;
					//        //}

					//        #endregion // Refactored
					//        // Copy the left and right border properties to the cells.
					//        WorksheetCellFormatProxy cellFormat = WorksheetColumn.GetCellFormatForVeritcalBorderSynchronization(row, columnIndex, value);
					//        if (cellFormat != null)
					//        {
					//            cellFormat.SetValue(value, formatValue, true, options);

					//            // We can skip the code below for setting the adjacent border because setting the border of this 
					//            // cell will automatically sync the adjacent border.
					//            continue;
					//        }		

					//        // If we should prevent syncing the borders of adjacent cells, skip the next block of code.
					//        if ((options & CellFormatValueChangedOptions.PreventAdjacentBorderSyncronization) != 0)
					//            break;

					//        // Reset properties for the border of adjacent cells which have the border common with this column.
					//        if (columnIndex > 0)
					//        {
					//            // MD 3/1/12 - 12.1 - Table Support
					//            #region Old Code

					//            //short previousColumnIndex = (short)(columnIndex - 1);
					//            //// MD 2/25/12 - 12.1 - Table Support
					//            ////if (row.HasCellFormatForCellResolved(previousColumnIndex, out cellFormat))
					//            //if (row.TryGetCellFormat(previousColumnIndex, out cellFormat))
					//            //{
					//            //    // MD 9/28/11 - TFS88683
					//            //    // Instead of resetting overlapping borders, we will now keep them synced.
					//            //    //cellFormat.ResetValue(Utilities.GetOppositeBorderValue(value));
					//            //    cellFormat.SetValue(oppositeValue, formatValue);
					//            //}

					//            #endregion // Old Code
					//            short previousColumnIndex = (short)(columnIndex - 1);
					//            cellFormat = WorksheetColumn.GetCellFormatForVeritcalBorderSynchronization(row, previousColumnIndex, oppositeValue);
					//            if (cellFormat != null)
					//                cellFormat.SetValue(oppositeValue, formatValue);
					//        }
					//    }
					//    }
					//    break;

					//case CellFormatValue.RightBorderColorInfo:
					//case CellFormatValue.RightBorderStyle:
					//    {
					//    int maxColumnCount = Workbook.GetMaxColumnCount(this.Worksheet.CurrentFormat);

					//    // MD 9/28/11 - TFS88683
					//    // Cache this so we don't need to get it multiple times below.
					//    CellFormatValue oppositeValue = Utilities.GetOppositeBorderValue(value);

					//    // MD 9/28/11 - TFS88683
					//    // We need to sync up overlapping borders on neighboring columns.
					//    if ((options & CellFormatValueChangedOptions.PreventAdjacentBorderSyncronization) == 0 &&
					//        this.Index < (maxColumnCount - 1))
					//    {
					//        WorksheetColumn neighboringColumn = this.Worksheet.Columns.GetIfCreated(this.Index + 1);
					//        if (neighboringColumn != null && neighboringColumn.HasCellFormat)
					//            neighboringColumn.CellFormatInternal.SetValue(oppositeValue, formatValue);
					//    }

					//    foreach (WorksheetRow row in this.Worksheet.Rows)
					//    {
					//        // MD 1/8/12 - 12.1 - Cell Format Updates
					//        // Changes to the column format should only be pushed to the merged cells if they are one column wide.
					//        WorksheetMergedCellsRegion associatedMergedCellRegion = row.GetCellAssociatedMergedCellsRegion(this.Index);
					//        if (associatedMergedCellRegion != null && associatedMergedCellRegion.Width != 1)
					//            continue;

					//        // MD 3/1/12 - 12.1 - Table Support
					//        #region Refactored

					//        //// Copy the left and right border properties to the cells.
					//        //WorksheetCellFormatProxy cellFormat;
					//        //// MD 2/25/12 - 12.1 - Table Support
					//        ////if (row.HasCellFormatForCellResolved(columnIndex, out cellFormat))
					//        //if (row.TryGetCellFormat(columnIndex, out cellFormat))
					//        //{
					//        //    // MD 9/28/11 - TFS88683
					//        //    // Use the cached format value so we don't need to get it in the CopyCellFormatValue method.
					//        //    // Also, this will sync the borders on the adjacent cells, so we can continue here because the code below would be redundant.
					//        //    //Utilities.CopyCellFormatValue(this.CellFormatInternal, cellFormat, value);
					//        //    cellFormat.SetValue(value, formatValue);

					//        //    // We can skip the code below for setting the adjacent border because setting the border of this 
					//        //    // cell will automatically sync the adjacent border.
					//        //    continue;
					//        //}

					//        #endregion // Refactored
					//        // Copy the left and right border properties to the cells.
					//        WorksheetCellFormatProxy cellFormat = WorksheetColumn.GetCellFormatForVeritcalBorderSynchronization(row, columnIndex, value);
					//        if (cellFormat != null)
					//        {
					//            cellFormat.SetValue(value, formatValue, true, options);

					//            // We can skip the code below for setting the adjacent border because setting the border of this 
					//            // cell will automatically sync the adjacent border.
					//            continue;
					//        }

					//        // If we should prevent syncing the borders of adjacent cells, skip the next block of code.
					//        if ((options & CellFormatValueChangedOptions.PreventAdjacentBorderSyncronization) != 0)
					//            break;

					//        // Reset properties for the border of adjacent cells which have the border common with this column.
					//        if (columnIndex > 0)
					//        {
					//            // MD 3/1/12 - 12.1 - Table Support
					//            #region Old Code

					//            //short nextColumnIndex = (short)(columnIndex + 1);
					//            //// MD 2/25/12 - 12.1 - Table Support
					//            ////if (row.HasCellFormatForCellResolved(nextColumnIndex, out cellFormat))
					//            //if (row.TryGetCellFormat(nextColumnIndex, out cellFormat))
					//            //{
					//            //    // MD 9/28/11 - TFS88683
					//            //    // Instead of resetting overlapping borders, we will now keep them synced.
					//            //    //cellFormat.ResetValue(Utilities.GetOppositeBorderValue(value));
					//            //    cellFormat.SetValue(oppositeValue, formatValue);
					//            //}

					//            #endregion // Old Code
					//            short nextColumnIndex = (short)(columnIndex + 1);
					//            cellFormat = WorksheetColumn.GetCellFormatForVeritcalBorderSynchronization(row, nextColumnIndex, oppositeValue);
					//            if (cellFormat != null)
					//                cellFormat.SetValue(oppositeValue, formatValue);
					//        }
					//    }
					//    }
					//    break;

					#endregion // Refactored
					case CellFormatValue.LeftBorderColorInfo:
					case CellFormatValue.LeftBorderStyle:
					case CellFormatValue.RightBorderColorInfo:
					case CellFormatValue.RightBorderStyle:
						{
							// Cache this so we don't need to get it multiple times below.
							CellFormatValue oppositeValue = Utilities.GetOppositeBorderValue(value);

							short? adjacentColumnIndex = null;
							if (value == CellFormatValue.LeftBorderColorInfo || value == CellFormatValue.LeftBorderStyle)
							{
								if (0 < columnIndex)
									adjacentColumnIndex = (short)(columnIndex - 1);
							}
							else
							{
								int maxColumnIndex = this.Worksheet.Columns.MaxCount - 1;
								if (columnIndex < maxColumnIndex)
									adjacentColumnIndex = (short)(columnIndex + 1);
							}

							// We need to sync up overlapping borders on neighboring columns.
							if (adjacentColumnIndex.HasValue &&
								(options & CellFormatValueChangedOptions.PreventAdjacentBorderSyncronization) == 0)
							{
								// MD 3/15/12 - TFS104581
								//WorksheetColumn neighboringColumn = this.Worksheet.Columns.GetIfCreated(adjacentColumnIndex.Value);
								//if (neighboringColumn != null && neighboringColumn.HasCellFormat)
								//    neighboringColumn.CellFormatInternal.SetValue(oppositeValue, formatValue);
								WorksheetColumn neighboringColumn = this.Worksheet.Columns[adjacentColumnIndex.Value];

								// MD 3/22/12 - TFS104630
								// We are now going back to resetting overlapping borders instead of syncing them, because that it what 
								// Microsoft seems to do internally based on looking at what is saved in their file formats. But we may also
								// need to take the associated border value from the overlapping cell format. This is all done in a helper
								// method now.
								//neighboringColumn.CellFormatInternal.SetValue(oppositeValue, formatValue, true, CellFormatValueChangedOptions.PreventAdjacentBorderSyncronization);
								Utilities.SynchronizeOverlappingBorderProperties(sender, neighboringColumn.CellFormatInternal, value, formatValue, options);
							}

							foreach (WorksheetRow row in this.Worksheet.Rows)
							{
								// Changes to the column format should only be pushed to the merged cells if they are one column wide.
								WorksheetMergedCellsRegion associatedMergedCellRegion = row.GetCellAssociatedMergedCellsRegion(this.Index);
								if (associatedMergedCellRegion != null && associatedMergedCellRegion.Width != 1)
									continue;

								// Copy the left and right border properties to the cells.
								WorksheetCellFormatProxy cellFormat = row.GetCellFormatForBorderSynchronization(columnIndex, value);
								if (cellFormat != null)
								{
									cellFormat.SetValue(value, formatValue, true, options);

									// We can skip the code below for setting the adjacent border because setting the border of this 
									// cell will automatically sync the adjacent border.
									continue;
								}

								// If we should prevent syncing the borders of adjacent cells, skip the next block of code.
								if ((options & CellFormatValueChangedOptions.PreventAdjacentBorderSyncronization) != 0)
									break;

								// Reset properties for the border of adjacent cells which have the border common with this column.
								if (adjacentColumnIndex.HasValue)
								{
									cellFormat = row.GetCellFormatForBorderSynchronization(adjacentColumnIndex.Value, oppositeValue);

									// MD 3/22/12 - TFS104630
									// We are now going back to resetting overlapping borders instead of syncing them, but we also need to reset
									// the associated property. And we should only reset them when the existing property doesn't match.
									//if (cellFormat != null)
									//    cellFormat.SetValue(oppositeValue, formatValue);
									if (cellFormat != null && cellFormat.GetValue(oppositeValue) != formatValue)
									{
										cellFormat.ResetValue(oppositeValue, CellFormatValueChangedOptions.PreventAdjacentBorderSyncronization);
										cellFormat.ResetValue(Utilities.GetAssociatedBorderValue(oppositeValue), CellFormatValueChangedOptions.PreventAdjacentBorderSyncronization);
									}
								}
							}
						}
						break;

					// Top and bottom borders are not used for columns, so ignore them.
					case CellFormatValue.BottomBorderColorInfo:
					case CellFormatValue.BottomBorderStyle:
					case CellFormatValue.TopBorderColorInfo:
					case CellFormatValue.TopBorderStyle:
						break;

					// MD 2/9/12 - TFS89375
					// Since a column's font affects a cell in all rows, a change in the font might change the default row height of the worksheet.
					case CellFormatValue.FontHeight:
					case CellFormatValue.FontName:
						this.Worksheet.ResetDefaultRowHeightResolved();
						goto default;

					default:
						// MD 10/26/11 - TFS91546
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

						// Copy all other values to the cells.
						foreach (WorksheetRow row in this.Worksheet.Rows)
						{
							// MD 3/1/12 - 12.1 - Table Support
							#region Refactored

							//WorksheetCellFormatProxy cellFormat;
							//// MD 10/24/11 - TFS91505
							//// We should also copy the value to the cell if the row has the value set because the row has a higher 
							//// precedence for the format values and we need to supersede the row's value here.
							////if (row.HasCellFormatForCellResolved(columnIndex, out cellFormat))
							//// MD 2/25/12 - 12.1 - Table Support
							////if (row.HasCellFormatForCellResolved(columnIndex, out cellFormat) ||
							//if (row.TryGetCellFormat(columnIndex, out cellFormat) ||
							//    (row.HasCellFormat && row.CellFormatInternal.IsValueDefault(value) == false))
							//{
							//    // MD 1/8/12 - 12.1 - Cell Format Updates
							//    // Changes to the column format should only be pushed to the merged cells if they are one column wide.
							//    WorksheetMergedCellsRegion associatedMergedCellRegion = row.GetCellAssociatedMergedCellsRegion(this.Index);
							//    if (associatedMergedCellRegion != null && associatedMergedCellRegion.Width != 1)
							//        continue;

							//    // MD 10/24/11 - TFS91505
							//    // The cellFormat may be null in here now, so make sure we resolve it.
							//    if (cellFormat == null)
							//        cellFormat = row.GetCellFormatInternal(columnIndex);

							//    // MD 9/28/11 - TFS88683
							//    // Use the cached format value so we don't need to get it in the CopyCellFormatValue method.
							//    //Utilities.CopyCellFormatValue(this.CellFormatInternal, cellFormat, value);
							//    // MD 10/26/11 - TFS91546
							//    // We have to do some special merging logic for the diagonals.
							//    //cellFormat.SetValue(value, formatValue);
							//    object resolvedFormatValue = formatValue;
							//    if (value == CellFormatValue.DiagonalBorders)
							//    {
							//        DiagonalBorders resolvedDiagonalBorders = newDiagonalBorders | cellFormat.DiagonalBorders;

							//        // MD 12/22/11 - 12.1 - Table Support
							//        // There is now a bit for when any value is set, so we can't remove all bits from each diagonal value.
							//        //if (removeDiagonalDown)
							//        //    resolvedDiagonalBorders &= ~DiagonalBorders.DiagonalDown;
							//        //
							//        //if (removeDiagonalUp)
							//        //    resolvedDiagonalBorders &= ~DiagonalBorders.DiagonalUp;
							//        if (removeDiagonalDown)
							//            Utilities.RemoveDiagonalDownBit(ref resolvedDiagonalBorders);

							//        if (removeDiagonalUp)
							//            Utilities.RemoveDiagonalUpBit(ref resolvedDiagonalBorders);

							//        resolvedFormatValue = resolvedDiagonalBorders;
							//    }
							//    cellFormat.SetValue(value, resolvedFormatValue);
							//}

							#endregion // Refactored
							WorksheetCellFormatProxy cellFormat = WorksheetColumn.GetCellFormatForGeneralSynchronization(row, columnIndex, value);
							if (cellFormat == null)
								continue;

							// Changes to the column format should only be pushed to the merged cells if they are one column wide.
							WorksheetMergedCellsRegion associatedMergedCellRegion = row.GetCellAssociatedMergedCellsRegion(this.Index);
							if (associatedMergedCellRegion != null && associatedMergedCellRegion.Width != 1)
								continue;

							object resolvedFormatValue = formatValue;
							if (value == CellFormatValue.DiagonalBorders)
							{
								DiagonalBorders resolvedDiagonalBorders = newDiagonalBorders | cellFormat.DiagonalBorders;

								if (removeDiagonalDown)
									Utilities.RemoveDiagonalDownBit(ref resolvedDiagonalBorders);

								if (removeDiagonalUp)
									Utilities.RemoveDiagonalUpBit(ref resolvedDiagonalBorders);

								resolvedFormatValue = resolvedDiagonalBorders;
							}

							cellFormat.SetValue(value, resolvedFormatValue);
						}
						break;
				}
			}
		}

		#endregion // OnCellFormatValueChanged

		// MD 3/15/12 - TFS104581
		// This is no longer needed.
		#region Removed

		//// MD 7/23/10 - TFS35969
		//#region OnHiddenChanged

		//internal override void OnHiddenChanged()
		//{
		//    base.OnHiddenChanged();
		//    this.Worksheet.Columns.ResetColumnWidthCache(this.Index, true);
		//} 

		//#endregion // OnHiddenChanged

		#endregion // Removed

		// MD 3/15/12 - TFS104581
		#region OutlineLevelInternal

		internal override byte OutlineLevelInternal
		{
			get { return this.Worksheet.GetColumnBlock(this.index).OutlineLevel; }
			set
			{
				if (this.OutlineLevel == value)
					return;

				this.OnBeforeColumnChange();
				this.Worksheet.GetColumnBlock(this.index).OutlineLevel = value;
				this.OnAfterColumnChange();
			}
		}

		#endregion // OutlineLevelInternal

		// MD 3/15/12 - TFS104581
		// Moved this to the WorksheetColumnBlock.
		#region Moved

		//// MD 7/2/08 - Excel 2007 Format
		//#region VerifyFormatLimits

		//internal override void VerifyFormatLimits( FormatLimitErrors limitErrors, WorkbookFormat testFormat )
		//{
		//    base.VerifyFormatLimits( limitErrors, testFormat );

		//    int maxColumnIndex = Workbook.GetMaxColumnCount( testFormat ) - 1;

		//    if ( maxColumnIndex < this.Index )
		//        limitErrors.AddError(String.Format(SR.GetString("LE_FormatLimitError_MaxColumnIndex"), this.Index, maxColumnIndex));
		//} 

		//#endregion VerifyFormatLimits

		#endregion // Moved

		#endregion Base Class Overrides

		#region Methods

		#region Public Methods

		// MD 2/10/12 - TFS97827
		#region GetWidth

		/// <summary>
		/// Gets the column width in the specified units, or NaN if the column has the default width.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// If <paramref name="units"/> is Character256th, the value returned will be the same as the value of the <see cref="Width"/> 
		/// property, with one exception: if the column has the default width, this method will return NaN and Width will return -1.
		/// </p>
		/// </remarks>
		/// <param name="units">The units in which the width should be returned.</param>
		/// <exception cref="InvalidEnumArgumentException">
		/// <paramref name="units"/> is not defined in the <see cref="WorksheetColumnWidthUnit"/> enumeration.
		/// </exception>
		/// <returns>The column width in the specified units, or NaN if the column has the default width.</returns>
		/// <seealso cref="Width"/>
		/// <seealso cref="SetWidth"/>
		/// <seealso cref="Worksheet.GetDefaultColumnWidth"/>
		public double GetWidth(WorksheetColumnWidthUnit units)
		{
			return this.Worksheet.ConvertFromCharacter256thsInt(this.Width, units);
		}

		#endregion // GetWidth

		// MD 2/10/12 - TFS97827
		#region SetWidth

		/// <summary>
		/// Sets the column width in the specified units.
		/// </summary>
		/// <param name="value">The width to set on the column, expressed in the specified <paramref name="units"/>.</param>
		/// <param name="units">The units in which the <paramref name="value"/> is expressed.</param>
		/// <remarks>
		/// <p class="body">
		/// Setting a value of NaN will reset the column width so that the column uses the default column width of the worksheet.
		/// </p>
		/// </remarks>
		/// <exception cref="ArgumentException">
		/// <paramref name="value"/> is infinity.
		/// </exception>
		/// <exception cref="InvalidEnumArgumentException">
		/// <paramref name="units"/> is not defined in the <see cref="WorksheetColumnWidthUnit"/> enumeration.
		/// </exception>
		/// <seealso cref="Width"/>
		/// <seealso cref="GetWidth"/>
		/// <seealso cref="Worksheet.SetDefaultColumnWidth(double,WorksheetColumnWidthUnit)"/>
		public void SetWidth(double value, WorksheetColumnWidthUnit units)
		{
			this.Width = this.Worksheet.ConvertToCharacter256thsInt(value, units);
		}

		#endregion // SetWidth

		#endregion // Public Methods

		#region Internal Methods

		// MD 3/15/12 - TFS104581
		// Moved this to the WorksheetColumnBlock.
		#region Moved

		//        #region InitSerializationCache

		//#if DEBUG
		//        /// <summary>
		//        /// Initialize the serialization cache member variables which store information need when saving the workbook.
		//        /// </summary>
		//        /// <param name="serializationManager">The manager which will save the workbook.</param>
		//        /// <param name="previousColumn">
		//        /// The previous collumn in the parent columns collection (because they are lazily created, it may not be the column immediately 
		//        /// before this column in the worksheet).
		//        /// </param>
		//#endif
		//        internal void InitSerializationCache( WorkbookSerializationManager serializationManager, WorksheetColumn previousColumn )
		//        {
		//            // MD 1/28/09 - TFS12400
		//            // We want to always add the cell format to the manager if the column will be written out, so this has to be moved to after
		//            // we determine if the column has data and therefore if it will be written out.
		//            //// Add the column's cell format to the serialization manager
		//            //if ( this.HasCellFormat )
		//            //    serializationManager.AddFormat( this.CellFormatInternal.Element );

		//            #region Determine if a collapse indicator is above the column

		//            // MD 3/15/11 - TFS21053 
		//            // The column with the collapsed indicator may be before or after an outlining group. 
		//            // If the ShowExpansionIndicatorToRightOfGroupedColumnsResolved value is False, the group 
		//            // is after the column with the indicator, so check the values of the next column instead 
		//            // of the previous one.
		//            //
		//            //// Initialize the default hidden state and outline level of the previous column
		//            //int previousColumnOutlineLevel = 0;
		//            //bool previousColumnHidden = false;
		//            //
		//            //// If the previous column specified is actually the immediate column before this column,
		//            //// take the hidden state and outline level from that column
		//            //if ( previousColumn != null && previousColumn.Index + 1 == this.Index )
		//            //{
		//            //    previousColumnOutlineLevel = previousColumn.OutlineLevel;
		//            //    previousColumnHidden = previousColumn.Hidden;
		//            //}
		//            // Initialize the default hidden state and outline level of the previous column
		//            int groupOutlineLevel = 0;
		//            bool groupHidden = false;

		//            if (this.Worksheet.DisplayOptions.ShowExpansionIndicatorToRightOfGroupedColumnsResolved)
		//            {
		//                // If the previous column specified is actually the immediate column before this column,
		//                // take the hidden state and outline level from that column
		//                if (previousColumn != null && previousColumn.Index + 1 == this.Index)
		//                {
		//                    groupOutlineLevel = previousColumn.OutlineLevel;
		//                    groupHidden = previousColumn.Hidden;
		//                }
		//            }
		//            else
		//            {
		//                WorksheetColumn nextColumn = this.Worksheet.Columns.GetIfCreated(this.Index + 1);
		//                if (nextColumn != null)
		//                {
		//                    groupOutlineLevel = nextColumn.OutlineLevel;
		//                    groupHidden = nextColumn.Hidden;
		//                }
		//            }

		//            // This column will only display a collapse indicator if the previous column is hidden and the previous column has 
		//            // a higher outline level
		//            //
		//            // MBS 7/24/08 - Excel 2007 Format
		//            //if ( previousColumnHidden && this.OutlineLevel < previousColumnOutlineLevel )
		//            //    this.hasCollapseIndicator = true;
		//            this.hasCollapseIndicator = groupHidden && this.OutlineLevel < groupOutlineLevel;

		//            #endregion Determine if a collapse indicator is above the column

		//            // Determine and store the value indicating whether the column should be saved
		//            this.hasData = this.Hidden || this.HasDataIgnoreHidden;

		//            // MD 1/10/12 - 12.1 - Cell Format Updates
		//            // The columns format must be in the manager now, because it is in the cell formats collection, so we don't have to do anything here.
		//            //// MD 1/28/09 - TFS12400
		//            //// Moved from above. We always want to add the format to the manager if the column has non-default data, even if the format 
		//            //// has all default data. This is because when a COLINFO record is written out, it should have a cell format index, not a style 
		//            //// format index. This is not enforced by Excel, but a 3rd party xls consumer may enforce this.
		//            //if ( this.HasData )
		//            //{
		//            //    serializationManager.AddFormat( this.CellFormatInternal.Element );
		//            //}
		//        }

		//        #endregion InitSerializationCache

		#endregion // Moved

		// MD 3/15/12 - TFS104581
		#region OnAfterColumnChange

		internal WorksheetColumnBlock OnAfterColumnChange()
		{
			return this.Worksheet.TryMergeColumnBlock(this.index);
		}

		#endregion // OnAfterColumnChange

		// MD 3/15/12 - TFS104581
		#region OnBeforeColumnChange

		internal WorksheetColumnBlock OnBeforeColumnChange()
		{
			return this.Worksheet.SplitColumnBlock(this.index, this.index);
		}

		#endregion // OnBeforeColumnChange

		#endregion // Internal Methods

		#region Private Methods

		// MD 3/1/12 - 12.1 - Table Support
		#region GetCellFormatForGeneralSynchronization

		private static WorksheetCellFormatProxy GetCellFormatForGeneralSynchronization(WorksheetRow row, short columnIndex, CellFormatValue value)
		{
			WorksheetCellFormatProxy cellFormat;
			if (row.TryGetCellFormat(columnIndex, out cellFormat))
				return cellFormat;

			// We should copy the value to the cell if the row has the value set because the row has a higher precedence for the 
			// format values and we need to supersede the row's value.
			if (row.HasCellFormat && row.CellFormatInternal.IsValueDefault(value) == false)
				return row.GetCellFormatInternal(columnIndex);

			// If column format is valid and has the property set, force the cell format to get created so we can overwrite 
			// its values with the current value being set.
			WorksheetCellFormatProxy associatedTableColumnAreaFormat = row.GetAssociatedColumnAreaFormat(columnIndex);
			if (associatedTableColumnAreaFormat != null && associatedTableColumnAreaFormat.IsValueDefault(value) == false)
				return row.GetCellFormatInternal(columnIndex);

			return null;
		}

		#endregion // GetCellFormatForGeneralSynchronization

		#endregion // Private Methods

		#endregion Methods

		#region Properties

		#region Public Properties

		#region Width

		/// <summary>
		/// Gets or sets the column width including padding, in 256ths of the '0' digit character width in the workbook's default font.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// If the width of the column is less than zero, the <see cref="Worksheet.DefaultColumnWidth"/> of the
		/// owning worksheet will be used as the column's width.
		/// </p>
		/// <p class="body">
		/// The value assigned must be between -1 and 65535. Invalid values will be automatically adjusted to valid values.
		/// </p>
		/// <p class="body">
		/// Setting or getting this property is equivalent to calling <see cref="SetWidth"/> or <see cref="GetWidth"/> using the 
		/// <see cref="WorksheetColumnWidthUnit"/> value of Character256th.
		/// </p>
		/// </remarks>
		/// <value>
		/// The column width including padding, in 256ths of the '0' digit character width in the workbook's default font.
		/// </value>
		/// <seealso cref="GetWidth"/>
		/// <seealso cref="SetWidth"/>
		/// <seealso cref="Workbook.CharacterWidth256thsToPixels"/>
		/// <seealso cref="Workbook.PixelsToCharacterWidth256ths"/>
		public int Width
		{
			// MD 3/15/12 - TFS104581
			//get { return this.width; }
			get { return this.Worksheet.GetColumnBlock(this.index).Width; }
			set
			{
				// MD 3/15/12 - TFS104581
				//if ( this.width != value )
				if (this.Width != value)
				{
					// MD 7/3/07 - BR24403
					// This exception breaks backwards compatibility. Now the invalid value is "fixed". Comments have been updated.
					//if ( value < 0 || 65535 < value )
					//    throw new ArgumentOutOfRangeException( "value", value, SR.GetString( "LE_ArgumentOutOfRangeException_ColumnWidth" ) );
					// MD 3/5/10 - TFS26342
					// We have to allow the user to set the default value back on the property.
					//if ( value < 0 )
					//    value = 0;
					if (value < -1)
						value = -1;

					if ( 65535 < value )
						value = 65535;

					// MD 3/5/10 - TFS26342
					// Notify the worksheet before the resize occurs and cache the old extent of this column.
					// MD 7/23/10 - TFS35969
					// The OnBeforeWorksheetElementResize method now takes the element being resized.
					//this.Worksheet.OnBeforeWorksheetElementResize();
					this.Worksheet.OnBeforeWorksheetElementResize(this);

					int oldExtentInTwipsHiddenIgnored = this.GetExtentInTwips(true);

					// MD 3/15/12 - TFS104581
					//this.width = value;
					this.OnBeforeColumnChange();
					this.Worksheet.GetColumnBlock(this.index).Width = value;
					this.OnAfterColumnChange();

					// MD 3/15/12 - TFS104581
					// We no longer cache the widths on the balance tree nodes.
					//// MD 7/23/10 - TFS35969
					//// If the width of the column changes, reset it's nodes height cache.
					//this.Worksheet.Columns.ResetColumnWidthCache(this.Index, false);

					// MD 3/5/10 - TFS26342
					// Notify the worksheet after the resize occurs.
					this.Worksheet.OnAfterWorksheetElementResized(this, oldExtentInTwipsHiddenIgnored, this.Hidden);
				}
			}
		}

		#endregion Width

		#endregion Public Properties

		#region Internal Properties

		// MD 3/15/12 - TFS104581
		// Moved this to the WorksheetColumnBlock.
		#region Moved

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
		//}

		//#endregion HasData

		#endregion // Moved

		// MD 3/15/12 - TFS104581
		#region IndexInternal

		internal short IndexInternal
		{
			get { return this.index; }
		}

		#endregion // IndexInternal

		#endregion Internal Properties

		#endregion Properties
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