using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;




using System.Drawing;


namespace Infragistics.Documents.Excel
{
	/// <summary>
	/// Represents a merged region of cells, or cells which share a value and format and appear as one cell when 
	/// viewed in Microsoft Excel.
	/// </summary>
	/// <remarks>
	/// <p class="body">
	/// Merged cell regions cannot overlap (a cell can only belong to one merged cell region). In addition, 
	/// <see cref="ArrayFormula"/> and <see cref="WorksheetDataTable"/> instances cannot be applied to merged cell
	/// regions.
	/// </p>
	/// </remarks>



	public

		 class WorksheetMergedCellsRegion : WorksheetRegion,

		// MD 4/12/11 - TFS67084
		// Moved away from using WorksheetCell objects.
		//IWorksheetCell,
		ICellFormatOwner,		// MD 1/8/12 - 12.1 - Cell Format Updates
		IWorksheetCellFormatProxyOwner	// MD 5/12/10 - TFS26732
	{
		#region Member Variables

		private WorksheetCellFormatProxy cellFormat;
		private bool isOnWorksheet = true;

		// MD 4/18/11 - TFS62026
		// This is no longer needed.
		//// MD 5/12/10 - TFS26732
		//private bool suspendSyncronizationsToAndFromCellFormats;

		// MD 8/20/08 - Excel formula solving
		// Refactoring - The merged region no longer stores the value, it is now stored on the top-left cell of the region.
		//private object value;

		#endregion Member Variables

		#region Constructor

		internal WorksheetMergedCellsRegion(Worksheet worksheet, int firstRow, int firstColumn, int lastRow, int lastColumn)
			// MD 8/21/08 - Excel formula solving
			// These should not get into the cached regions
			//: base( worksheet, firstRow, firstColumn, lastRow, lastColumn ) 
			: base(worksheet, firstRow, firstColumn, lastRow, lastColumn, false)
		{
			// MD 2/17/12 - TFS101826
			// Rewrote this code to act more like Excel does. Also, this was written before the border cells were kept in sync always,
			// so it was a little out of date.
			#region Old Code

			//// MD 8/20/08 - Excel formula solving
			//// Refactoring - The merged region no longer stores the value, it is now stored on the top-left cell of the region.
			//object value = null;

			//// MD 5/12/10 - TFS26732
			//// Determine the borders which should be applied to this merged cell after we copy over the cell format from the top left cell.
			//Dictionary<CellFormatValue, object> borders = this.GetMergedCellBorderValues(null);

			//// MD 10/26/11 - TFS91546
			//// We need to make sure all cells in the merged region have the same diagonal border properties. If so, they can be put in 
			//// the merged region.
			//bool bordersCanBySynced = true;
			//CellBorderLineStyle? diagonalBorderStyle = null;
			//DiagonalBorders? diagonalBorders = null;

			//// MD 1/16/12 - 12.1 - Cell Format Updates
			////Color? diagonalBorderColor = null;
			//WorkbookColorInfo diagonalBorderColorInfo = null;

			//// MD 1/8/12 - 12.1 - Cell Format Updates
			//// Cache the normal format so we can use it below.
			//WorksheetCellFormatData normalFormat = null;
			//if (worksheet.Workbook != null)
			//    normalFormat = worksheet.Workbook.Styles.NormalStyle.StyleFormatInternal;

			//// MD 1/8/12 - 12.1 - Table Support
			//// We need to get the top-left cell format before setting the associated merged cell on it because otherwise, it might
			//// not initialize it's default data correctly.
			//WorksheetRow topRow = this.TopRow;
			//WorksheetCellFormatProxy topLeftCellFormat = topRow.GetCellFormatInternal(this.FirstColumnInternal);

			//// Initialize this region's cell format and value and let all cells in the region
			//// know this is their merged cell region now
			//for ( int rowIndex = firstRow; rowIndex <= lastRow; rowIndex++ )
			//{
			//    WorksheetRow row = worksheet.Rows[ rowIndex ];

			//    // MD 4/12/11 - TFS67084
			//    // Use short instead of int so we don't have to cast.
			//    //for ( int columnIndex = firstColumn; columnIndex <= lastColumn; columnIndex++ )
			//    for (short columnIndex = (short)firstColumn; columnIndex <= lastColumn; columnIndex++)
			//    {
			//        // MD 4/12/11 - TFS67084
			//        // Moved away from using WorksheetCell objects.
			//        //WorksheetCell cell = row.Cells[ columnIndex ];

			//        // MD 5/12/10 - TFS26732
			//        // This was incorrect logic. We shouldn't have been copying over properties from the first cell with a format. We should
			//        // have alwasy been copying over proeprties from the top-left cell. This is done after the loop.
			//        //if ( cell.HasCellFormat && cell.CellFormatInternal.HasDefaultValue == false )
			//        //{
			//        //    if ( this.HasCellFormat == false || this.CellFormatInternal.HasDefaultValue )
			//        //        this.CellFormatInternal.SetToElement( cell.CellFormatInternal.Element );
			//        //}

			//        // MD 8/20/08 - Excel formula solving
			//        // Refactoring - The merged region no longer stores the value, it is now stored on the top-left cell of the region.
			//        //if ( cell.Value != null && this.Value == null )
			//        // MD 10/20/10 - TFS36617
			//        // Based on the fix for TFS17197 below, we should be getting the ValueInternal rather than the Value. Also, we shouldn't
			//        // be getting it twice, so this has been refactored so we only get the value once.
			//        #region Refactored

			//        //if ( cell.Value != null && value == null )
			//        //{
			//        //    // MD  10/19/07 - BR27421
			//        //    // If the value is a formula, it can't be set directly, because we are trying to give it a second owner, 
			//        //    // which is not allowed. We need to cache the formula, clear the value (and the formula's owner), then
			//        //    // set the value on the region using InternalSetValue because setting the Value directly is not allowed
			//        //    // for formulas.
			//        //    //this.Value = cell.Value;
			//        //    //object value = cell.Value;
			//        //    //cell.Value = null;
			//        //    // MD 8/20/08 - Excel formula solving
			//        //    // Refactoring - The merged region no longer stores the value, it is now stored on the top-left cell of the region.
			//        //    //this.InternalSetValue( value, true );
			//        //    // MD 5/4/09 - TFS17197
			//        //    // Due to formula solving, the Value of the cell may return the calculated value, but we want the actual 
			//        //    // value (the formula if it is present) when we set it on the top-left cell.
			//        //    //value = cell.Value;
			//        //    value = cell.ValueInternal;
			//        //
			//        //    // MD 9/2/08 - Cell Comments
			//        //    // This will now be cleared in the SetMergedCell method called below.
			//        //    //cell.Value = null;
			//        //} 

			//        #endregion // Refactored
			//        if (value == null)
			//        {
			//            // MD 4/12/11 - TFS67084
			//            // Moved away from using WorksheetCell objects.
			//            //value = cell.ValueInternal;
			//            value = row.GetCellValueInternal(columnIndex);
			//        }

			//        // MD 4/12/11 - TFS67084
			//        // Moved away from using WorksheetCell objects.
			//        //cell.SetMergedCell( this );
			//        row.SetMergedCellOnCell(columnIndex, this);

			//        // MD 5/12/10 - TFS26732
			//        // Clear out the interior border values of the cells in the merged region.
			//        // MD 4/12/11 - TFS67084
			//        // Moved away from using WorksheetCell objects.
			//        //if (cell.HasCellFormat)
			//        //{
			//        //    if(rowIndex != firstRow)
			//        //    {
			//        //        cell.CellFormatInternal.ResetValue(CellFormatValue.TopBorderColor);
			//        //        cell.CellFormatInternal.ResetValue(CellFormatValue.TopBorderStyle);
			//        //    }
			//        //
			//        //    if (rowIndex != lastRow)
			//        //    {
			//        //        cell.CellFormatInternal.ResetValue(CellFormatValue.BottomBorderColor);
			//        //        cell.CellFormatInternal.ResetValue(CellFormatValue.BottomBorderStyle);
			//        //    }
			//        //
			//        //    if (columnIndex != firstColumn)
			//        //    {
			//        //        cell.CellFormatInternal.ResetValue(CellFormatValue.LeftBorderColor);
			//        //        cell.CellFormatInternal.ResetValue(CellFormatValue.LeftBorderStyle);
			//        //    }
			//        //
			//        //    if (columnIndex != lastColumn)
			//        //    {
			//        //        cell.CellFormatInternal.ResetValue(CellFormatValue.RightBorderColor);
			//        //        cell.CellFormatInternal.ResetValue(CellFormatValue.RightBorderStyle);
			//        //    }
			//        //}
			//        WorksheetCellFormatProxy cellFormat;
			//        if (row.HasCellFormatForCellResolved(columnIndex, out cellFormat))
			//        {
			//            if (rowIndex != firstRow)
			//            {
			//                cellFormat.ResetValue(CellFormatValue.TopBorderColorInfo);
			//                cellFormat.ResetValue(CellFormatValue.TopBorderStyle);
			//            }

			//            if (rowIndex != lastRow)
			//            {
			//                cellFormat.ResetValue(CellFormatValue.BottomBorderColorInfo);
			//                cellFormat.ResetValue(CellFormatValue.BottomBorderStyle);
			//            }

			//            if (columnIndex != firstColumn)
			//            {
			//                cellFormat.ResetValue(CellFormatValue.LeftBorderColorInfo);
			//                cellFormat.ResetValue(CellFormatValue.LeftBorderStyle);
			//            }

			//            if (columnIndex != lastColumn)
			//            {
			//                cellFormat.ResetValue(CellFormatValue.RightBorderColorInfo);
			//                cellFormat.ResetValue(CellFormatValue.RightBorderStyle);
			//            }
			//        }

			//        // MD 10/26/11 - TFS91546
			//        // We need to make sure all cells in the merged region have the same diagonal border properties. If so, they can be put in 
			//        // the merged region.
			//        #region Check the diagonal border properties

			//        if (bordersCanBySynced)
			//        {
			//            CellBorderLineStyle resolvedDiagonalBorderStyle;
			//            DiagonalBorders resolvedDiagonalBorders;

			//            // MD 1/16/12 - 12.1 - Cell Format Updates
			//            //Color resolvedDiagonalBorderColor;
			//            WorkbookColorInfo resolvedDiagonalBorderColorInfo;
			//            if (cellFormat != null)
			//            {
			//                // MD 1/8/12 - 12.1 - Cell Format Updates
			//                // Use the resolved values.
			//                //resolvedDiagonalBorderStyle = cellFormat.DiagonalBorderStyle;
			//                //resolvedDiagonalBorders = cellFormat.DiagonalBorders;
			//                //resolvedDiagonalBorderColor = cellFormat.DiagonalBorderColor;
			//                resolvedDiagonalBorderStyle = cellFormat.Element.DiagonalBorderStyleResolved;
			//                resolvedDiagonalBorders = cellFormat.Element.DiagonalBordersResolved;
			//                resolvedDiagonalBorderColorInfo = cellFormat.Element.DiagonalBorderColorInfoResolved;
			//            }
			//            else if (row.HasCellFormat)
			//            {
			//                // MD 1/8/12 - 12.1 - Cell Format Updates
			//                // Use the resolved values.
			//                //resolvedDiagonalBorderStyle = row.CellFormat.DiagonalBorderStyle;
			//                //resolvedDiagonalBorders = row.CellFormat.DiagonalBorders;
			//                //resolvedDiagonalBorderColor = row.CellFormat.DiagonalBorderColor;
			//                resolvedDiagonalBorderStyle = row.CellFormatInternal.Element.DiagonalBorderStyleResolved;
			//                resolvedDiagonalBorders = row.CellFormatInternal.Element.DiagonalBordersResolved;
			//                resolvedDiagonalBorderColorInfo = row.CellFormatInternal.Element.DiagonalBorderColorInfoResolved;
			//            }
			//            else
			//            {
			//                WorksheetColumn column = worksheet.Columns.GetIfCreated(columnIndex);
			//                if (column != null && column.HasCellFormat)
			//                {
			//                    // MD 1/8/12 - 12.1 - Cell Format Updates
			//                    // Use the resolved values.
			//                    //resolvedDiagonalBorderStyle = column.CellFormat.DiagonalBorderStyle;
			//                    //resolvedDiagonalBorders = column.CellFormat.DiagonalBorders;
			//                    //resolvedDiagonalBorderColor = column.CellFormat.DiagonalBorderColor;
			//                    resolvedDiagonalBorderStyle = column.CellFormatInternal.Element.DiagonalBorderStyleResolved;
			//                    resolvedDiagonalBorders = column.CellFormatInternal.Element.DiagonalBordersResolved;
			//                    resolvedDiagonalBorderColorInfo = column.CellFormatInternal.Element.DiagonalBorderColorInfoResolved;
			//                }
			//                // MD 1/8/12 - 12.1 - Cell Format Updates
			//                // Use the normal format before using ultimate defaults.
			//                else if (normalFormat != null)
			//                {
			//                    resolvedDiagonalBorderStyle = normalFormat.DiagonalBorderStyleResolved;
			//                    resolvedDiagonalBorders = normalFormat.DiagonalBordersResolved;
			//                    resolvedDiagonalBorderColorInfo = normalFormat.DiagonalBorderColorInfoResolved;
			//                }
			//                else
			//                {
			//                    resolvedDiagonalBorderStyle = CellBorderLineStyle.Default;

			//                    // MD 12/22/11 - 12.1 - Table Support
			//                    // The DiagonalBorders enumeration now has a default value.
			//                    //resolvedDiagonalBorders = DiagonalBorders.None;
			//                    resolvedDiagonalBorders = DiagonalBorders.Default;

			//                    // MD 1/16/12 - 12.1 - Cell Format Updates
			//                    //resolvedDiagonalBorderColor = Utilities.ColorEmpty;
			//                    resolvedDiagonalBorderColorInfo = WorkbookColorInfo.Automatic;

			//                    bordersCanBySynced = false;
			//                }
			//            }

			//            if (bordersCanBySynced)
			//            {
			//                if (diagonalBorderStyle.HasValue)
			//                {
			//                    if (diagonalBorderStyle.Value != resolvedDiagonalBorderStyle ||
			//                        diagonalBorders.Value != resolvedDiagonalBorders ||
			//                        // MD 1/16/12 - 12.1 - Cell Format Updates
			//                        //diagonalBorderColor.Value != resolvedDiagonalBorderColor)
			//                        diagonalBorderColorInfo != resolvedDiagonalBorderColorInfo)
			//                    {
			//                        bordersCanBySynced = false;
			//                    }
			//                }
			//                else
			//                {
			//                    diagonalBorderStyle = resolvedDiagonalBorderStyle;
			//                    diagonalBorders = resolvedDiagonalBorders;

			//                    // MD 1/16/12 - 12.1 - Cell Format Updates
			//                    //diagonalBorderColor = resolvedDiagonalBorderColor;
			//                    diagonalBorderColorInfo = resolvedDiagonalBorderColorInfo;
			//                }
			//            }
			//        }

			//        #endregion  // Check the diagonal border properties
			//    }
			//}

			//// MD 5/12/10 - TFS26732
			//// Copy the format settings from the top left cell.
			//// MD 4/12/11 - TFS67084
			//// Moved away from using WorksheetCell objects.
			////this.CellFormatInternal.SetFormatting(this.TopLeftCell.CellFormatInternal);
			//// MD 1/8/12 - 12.1 - Table Support
			//// We are not resolving this above.
			////WorksheetRow topRow = this.TopRow;
			////this.CellFormatInternal.SetFormatting(topRow.GetCellFormatInternal(this.FirstColumnInternal));
			//this.CellFormatInternal.SetFormatting(topLeftCellFormat);

			//// MD 5/12/10 - TFS26732
			//// Overwrite the border settings with the resolved values.
			//foreach (KeyValuePair<CellFormatValue, object> border in borders)
			//    this.CellFormatInternal.SetValue(border.Key, border.Value);

			//// MD 10/26/11 - TFS91546
			//// If all cells didn't have the same diagonal border properties, clear the diagonal borders.
			//if (bordersCanBySynced == false)
			//{
			//    this.CellFormatInternal.ResetValue(CellFormatValue.DiagonalBorderColorInfo);
			//    this.CellFormatInternal.ResetValue(CellFormatValue.DiagonalBorders);
			//    this.CellFormatInternal.ResetValue(CellFormatValue.DiagonalBorderStyle);
			//}

			//// MD 8/20/08 - Excel formula solving
			//// Refactoring - The merged region no longer stores the value, it is now stored on the top-left cell of the region.
			////this.TopLeftCell.Value = value;
			//// MD 5/4/09 - TFS17197
			//// Due to formula solving, the Value of the cell may return the calculated value, but we want the actual 
			//// value (the formula if it is present) when we set it on the top-left cell. Also, we should use InternalSetValue
			//// because the Value setter will throw an exception if a Formula is passed.
			//// MD 4/12/11 - TFS67084
			//// Moved away from using WorksheetCell objects.
			////if ( value != this.TopLeftCell.ValueInternal )
			////    this.TopLeftCell.InternalSetValue( value );
			//WorksheetCellBlock cellBlock;
			//if (topRow.TryGetCellBlock(this.FirstColumnInternal, out cellBlock) &&
			//    value != cellBlock.GetCellValueInternal(topRow, this.FirstColumnInternal))
			//{
			//    // MD 1/31/12 - TFS100573
			//    // The cell block may get replaced by this operation. If it does, replace our reference to it (even though we don't 
			//    // currently do anything with the cell block after this, it will prevent bugs from being introduced if code is added
			//    // later which does use it).
			//    //cellBlock.SetCellValueInternal(topRow, this.FirstColumnInternal, value);
			//    WorksheetCellBlock replacementBlock;
			//    cellBlock.SetCellValueInternal(topRow, this.FirstColumnInternal, value, out replacementBlock);
			//    cellBlock = replacementBlock ?? cellBlock;
			//}

			#endregion // Old Code
			object value = null;

			WorkbookColorInfo commonBottomBorderColorInfo = null;
			CellBorderLineStyle commonBottomBorderStyle = CellBorderLineStyle.Default;
			WorkbookColorInfo commonDiagonalBorderColorInfo = null;
			DiagonalBorders commonDiagonalBorders = DiagonalBorders.Default;
			CellBorderLineStyle commonDiagonalBorderStyle = CellBorderLineStyle.Default;
			WorkbookColorInfo commonLeftBorderColorInfo = null;
			CellBorderLineStyle commonLeftBorderStyle = CellBorderLineStyle.Default;
			WorkbookColorInfo commonRightBorderColorInfo = null;
			CellBorderLineStyle commonRightBorderStyle = CellBorderLineStyle.Default;
			WorkbookColorInfo commonTopBorderColorInfo = null;
			CellBorderLineStyle commonTopBorderStyle = CellBorderLineStyle.Default;

			// We need to get the top-left cell format before setting the associated merged cell on it because otherwise, it might
			// not initialize it's default data correctly.
			WorksheetRow topRow = this.TopRow;
			WorksheetCellFormatData mergedCellFormat = topRow.GetCellFormatInternal(this.FirstColumnInternal).Element.CloneInternal();

			// Initialize this region's cell format and value and let all cells in the region
			for (int rowIndex = firstRow; rowIndex <= lastRow; rowIndex++)
			{
				WorksheetRow row = worksheet.Rows[rowIndex];
				for (short columnIndex = (short)firstColumn; columnIndex <= lastColumn; columnIndex++)
				{
					WorksheetCellFormatProxy cellFormat = row.GetCellFormatInternal(columnIndex);
					WorksheetCellFormatDataResolved cellFormatResolved = new WorksheetCellFormatDataResolved(cellFormat);

					if (value == null)
					{
						// Take the value and cell format of the first cell with a non-null value.
						value = row.GetCellValueRaw(columnIndex);
						if (value != null)
							mergedCellFormat = cellFormat.Element.CloneInternal();
					}

					if (rowIndex == firstRow)
					{
						WorksheetMergedCellsRegion.CombineBorderValues(
							cellFormatResolved.TopBorderColorInfo, cellFormatResolved.TopBorderStyle,
							ref commonTopBorderColorInfo, ref commonTopBorderStyle);
					}

					if (rowIndex == lastRow)
					{
						WorksheetMergedCellsRegion.CombineBorderValues(
							cellFormatResolved.BottomBorderColorInfo, cellFormatResolved.BottomBorderStyle,
							ref commonBottomBorderColorInfo, ref commonBottomBorderStyle);
					}

					if (columnIndex == firstColumn)
					{
						WorksheetMergedCellsRegion.CombineBorderValues(
							cellFormatResolved.LeftBorderColorInfo, cellFormatResolved.LeftBorderStyle,
							ref commonLeftBorderColorInfo, ref commonLeftBorderStyle);
					}

					if (columnIndex == lastColumn)
					{
						WorksheetMergedCellsRegion.CombineBorderValues(
							cellFormatResolved.RightBorderColorInfo, cellFormatResolved.RightBorderStyle,
							ref commonRightBorderColorInfo, ref commonRightBorderStyle);
					}

					WorksheetMergedCellsRegion.CombineBorderValues(
						cellFormatResolved.DiagonalBorderColorInfo, cellFormatResolved.DiagonalBorders, cellFormatResolved.DiagonalBorderStyle,
						ref commonDiagonalBorderColorInfo, ref commonDiagonalBorders, ref commonDiagonalBorderStyle);
				}
			}

			// If we are loading the merged cell, we shouldn't be affecting any of the cells within the merged region because
			// theoretically, they are already in the proper state.
			Workbook workbook = worksheet.Workbook;
			bool shouldResetCellAppearances = workbook == null || workbook.IsLoading == false;

			// Let all cells in the region know this is their merged cell region now
			for (int rowIndex = firstRow; rowIndex <= lastRow; rowIndex++)
			{
				WorksheetRow row = worksheet.Rows[rowIndex];
				for (short columnIndex = (short)firstColumn; columnIndex <= lastColumn; columnIndex++)
				{
					row.SetMergedCellOnCell(columnIndex, this);

					if (shouldResetCellAppearances == false)
						continue;

					WorksheetCellFormatProxy cellFormat = row.GetCellFormatInternal(columnIndex);
					cellFormat.Style = mergedCellFormat.Style;
					cellFormat.FormatOptions = WorksheetCellFormatOptions.None;
				}
			}

			WorksheetCellFormatProxy mergedFormat = this.CellFormatInternal;
			mergedFormat.SetFormatting(mergedCellFormat);

			// Note: When we are checking the resolved value below, use the element's resolved properties instead of using GetResolvedCellFormat 
			// because we don't want to use the logic which checks the cells within the merged regions and those bordering the merged region. 
			// We just want to consider the format and parent style.

			// The the merged cell provides its own border values and they are not the correct values, clear the ApplyBorderFormatting
			// style option to see if the owning style has the correct border information.
			bool shouldCheckBorderValues = true;
			if (Utilities.TestFlag(mergedFormat.FormatOptions, WorksheetCellFormatOptions.ApplyBorderFormatting))
			{
				if (mergedFormat.Element.BottomBorderColorInfoResolved != commonBottomBorderColorInfo ||
					mergedFormat.Element.BottomBorderStyleResolved != commonBottomBorderStyle ||
					mergedFormat.Element.DiagonalBorderColorInfoResolved != commonDiagonalBorderColorInfo ||
					mergedFormat.Element.DiagonalBordersResolved != commonDiagonalBorders ||
					mergedFormat.Element.DiagonalBorderStyleResolved != commonDiagonalBorderStyle ||
					mergedFormat.Element.LeftBorderColorInfoResolved != commonLeftBorderColorInfo ||
					mergedFormat.Element.LeftBorderStyleResolved != commonLeftBorderStyle ||
					mergedFormat.Element.RightBorderColorInfoResolved != commonRightBorderColorInfo ||
					mergedFormat.Element.RightBorderStyleResolved != commonRightBorderStyle ||
					mergedFormat.Element.TopBorderColorInfoResolved != commonTopBorderColorInfo ||
					mergedFormat.Element.TopBorderStyleResolved != commonTopBorderStyle)
				{
					mergedFormat.FormatOptions &= ~WorksheetCellFormatOptions.ApplyBorderFormatting;
				}
				else
				{
					// If all the properties match already, we don't need to check them again below.
					shouldCheckBorderValues = false;
				}
			}

			if (shouldCheckBorderValues)
			{
				if (mergedFormat.Element.BottomBorderColorInfoResolved != commonBottomBorderColorInfo)
					mergedFormat.BottomBorderColorInfo = commonBottomBorderColorInfo;

				if (mergedFormat.Element.BottomBorderStyleResolved != commonBottomBorderStyle)
					mergedFormat.BottomBorderStyle = commonBottomBorderStyle;

				if (mergedFormat.Element.DiagonalBorderColorInfoResolved != commonDiagonalBorderColorInfo)
					mergedFormat.DiagonalBorderColorInfo = commonDiagonalBorderColorInfo;

				if (mergedFormat.Element.DiagonalBordersResolved != commonDiagonalBorders)
					mergedFormat.DiagonalBorders = commonDiagonalBorders;

				if (mergedFormat.Element.DiagonalBorderStyleResolved != commonDiagonalBorderStyle)
					mergedFormat.DiagonalBorderStyle = commonDiagonalBorderStyle;

				if (mergedFormat.Element.LeftBorderColorInfoResolved != commonLeftBorderColorInfo)
					mergedFormat.LeftBorderColorInfo = commonLeftBorderColorInfo;

				if (mergedFormat.Element.LeftBorderStyleResolved != commonLeftBorderStyle)
					mergedFormat.LeftBorderStyle = commonLeftBorderStyle;

				if (mergedFormat.Element.RightBorderColorInfoResolved != commonRightBorderColorInfo)
					mergedFormat.RightBorderColorInfo = commonRightBorderColorInfo;

				if (mergedFormat.Element.RightBorderStyleResolved != commonRightBorderStyle)
					mergedFormat.RightBorderStyle = commonRightBorderStyle;

				if (mergedFormat.Element.TopBorderColorInfoResolved != commonTopBorderColorInfo)
					mergedFormat.TopBorderColorInfo = commonTopBorderColorInfo;

				if (mergedFormat.Element.TopBorderStyleResolved != commonTopBorderStyle)
					mergedFormat.TopBorderStyle = commonTopBorderStyle;
			}

			topRow.SetCellValueRaw(this.FirstColumnInternal, value);
		}

		#endregion Constructor	

		#region Interfaces

		// MD 1/8/12 - 12.1 - Cell Format Updates
		#region ICellFormatOwner Members

		WorksheetCellFormatProxy ICellFormatOwner.CellFormatInternal
		{
			get { return this.CellFormatInternal; }
		}

		bool ICellFormatOwner.HasCellFormat
		{
			get { return this.HasCellFormat; }
		}

		#endregion

		// MD 4/12/11 - TFS67084
		// This is no longer needed.
		//// MD 9/2/08 - Cell Comments
		//#region IFormattedStringOwner Members

		//void IFormattedStringOwner.OnUnformattedStringChanged() { }

		//#endregion

		// MD 4/12/11 - TFS67084
		// Moved away from using WorksheetCell objects.
		#region Removed

		//#region IWorksheetCell Members

		//// MD 7/14/08 - Excel formula solving
		//void IWorksheetCell.DirtyReference()
		//{
		//    // MD 4/12/11 - TFS67084
		//    // Moved away from using WorksheetCell objects.
		//    //IWorksheetCell topLeftCell = this.TopLeftCell;
		//    //topLeftCell.DirtyReference();
		//    this.Worksheet.Rows[this.FirstRow].DirtyReference(this.FirstColumnInternal);
		//}

		//void IWorksheetCell.InternalSetValue( object value )
		//{
		//    // MD 8/20/08 - Excel formula solving
		//    // Refactoring - The merged region no longer stores the value, it is now stored on the top-left cell of the region.
		//    //this.InternalSetValue( value, true );
		//    // MD 9/23/09 - TFS19150
		//    // Added a parameter to the InternalSetValue method.
		//    //this.TopLeftCell.InternalSetValue( value, true );
		//    // MD 4/12/11 - TFS67084
		//    // Moved away from using WorksheetCell objects.
		//    //this.TopLeftCell.InternalSetValue( value, null, true );
		//    this.Worksheet.Rows[this.FirstRow].SetCellValueInternal(this.FirstColumnInternal, value);
		//}

		//// MD 7/14/08 - Excel formula solving
		//void IWorksheetCell.SetFormulaOnCalcReference( bool canClearPreviouslyCalculatedValue )
		//{
		//    // MD 4/12/11 - TFS67084
		//    // Moved away from using WorksheetCell objects.
		//    //IWorksheetCell topLeftCell = this.TopLeftCell;
		//    //topLeftCell.SetFormulaOnCalcReference( canClearPreviouslyCalculatedValue );
		//    this.Worksheet.Rows[this.FirstRow].SetFormulaOnCalcReference(this.FirstColumnInternal, canClearPreviouslyCalculatedValue);
		//}

		//int IWorksheetCell.ColumnIndex
		//{
		//    get { return this.FirstColumn; }
		//}

		//bool IWorksheetCell.HasCellFormat
		//{
		//    get { return this.HasCellFormat; }
		//}

		//bool IWorksheetCell.IsOnWorksheet
		//{
		//    get { return this.isOnWorksheet; }
		//}

		//// MD 7/14/08 - Excel formula solving
		//IWorksheetRegionBlockingValue IWorksheetCell.RegionBlockingValue
		//{
		//    // MD 8/20/08 - Excel formula solving
		//    // Refactoring - The merged region no longer stores the value, it is now stored on the top-left cell of the region.
		//    //get { return this.value as IWorksheetRegionBlockingValue; }
		//    // MD 4/12/11 - TFS67084
		//    // Moved away from using WorksheetCell objects.
		//    //get { return this.TopLeftCell.ValueInternal as IWorksheetRegionBlockingValue; }
		//    get { return this.Worksheet.Rows[this.FirstRow].GetCellValueInternal(this.FirstColumnInternal) as IWorksheetRegionBlockingValue; }
		//}

		//int IWorksheetCell.RowIndex
		//{
		//    get { return this.FirstRow; }
		//}

		//#endregion 

		#endregion  // Removed

		// MD 5/12/10 - TFS26732
		#region IWorksheetCellFormatProxyOwner Members

		// MD 3/22/12 - TFS104630
		WorksheetCellFormatData IWorksheetCellFormatProxyOwner.GetAdjacentFormatForBorderResolution(WorksheetCellFormatProxy sender, CellFormatValue borderValue)
		{
			Utilities.DebugFail("This shouldn't be used.");
			return null;
		}

		// MD 10/21/10 - TFS34398
		// We need to pass along options to the handlers of the cell format value change.
		//void IWorksheetCellFormatProxyOwner.OnCellFormatValueChanged(CellFormatValue value)
		// MD 4/12/11 - TFS67084
		// We need to pass along the sender now because some object own multiple cell formats.
		//void IWorksheetCellFormatProxyOwner.OnCellFormatValueChanged(CellFormatValue value, CellFormatValueChangedOptions options)
		// MD 4/18/11 - TFS62026
		// The proxy will now pass along all values that changed in one operation as opposed to one at a time.
		//void IWorksheetCellFormatProxyOwner.OnCellFormatValueChanged(WorksheetCellFormatProxy sender, CellFormatValue value, CellFormatValueChangedOptions options)
		void IWorksheetCellFormatProxyOwner.OnCellFormatValueChanged(WorksheetCellFormatProxy sender, IList<CellFormatValue> values, CellFormatValueChangedOptions options)
		{
			// MD 2/29/12 - 12.1 - Table Support
			// The worksheet can now be null.
			if (this.Worksheet == null)
			{
				Utilities.DebugFail("This is unexpected");
				return;
			}

			// MD 4/18/11 - TFS62026
			// Refactored this code to deal with multiple values.
			#region Refactored

			//if (this.suspendSyncronizationsToAndFromCellFormats)
			//    return;

			//bool oldSuspendSyncronizationsToAndFromCellFormats = this.suspendSyncronizationsToAndFromCellFormats;
			//this.suspendSyncronizationsToAndFromCellFormats = true;
			//try
			//{


			//    // When the borders are set on the merged cell, push the values to the equivalent borders of the individual cells in the merged cell.
			//    switch (value)
			//    {
			//        case CellFormatValue.BottomBorderColor:
			//        case CellFormatValue.BottomBorderStyle:
			//            {
			//                WorksheetRow row = this.Worksheet.Rows[this.LastRow];

			//                // MD 4/12/11 - TFS67084
			//                // Moved away from using WorksheetCell objects.
			//                //for (int i = this.FirstColumn; i <= this.LastColumn; i++)
			//                //    Utilities.CopyCellFormatValue(this.CellFormatInternal, row.Cells[i].CellFormatInternal, value);
			//                for (short i = this.FirstColumnInternal; i <= this.LastColumnInternal; i++)
			//                    Utilities.CopyCellFormatValue(this.CellFormatInternal, row.GetCellFormatInternal(i), value);
			//            }
			//            break;

			//        case CellFormatValue.LeftBorderColor:
			//        case CellFormatValue.LeftBorderStyle:
			//            {
			//                for (int i = this.FirstRow; i <= this.LastRow; i++)
			//                {
			//                    // MD 4/12/11 - TFS67084
			//                    // Moved away from using WorksheetCell objects.
			//                    //Utilities.CopyCellFormatValue(this.CellFormatInternal, this.Worksheet.Rows[i].Cells[this.FirstColumn].CellFormatInternal, value);
			//                    Utilities.CopyCellFormatValue(this.CellFormatInternal, this.Worksheet.Rows[i].GetCellFormatInternal(this.FirstColumnInternal), value);
			//                }
			//            }
			//            break;

			//        case CellFormatValue.RightBorderColor:
			//        case CellFormatValue.RightBorderStyle:
			//            {
			//                for (int i = this.FirstRow; i <= this.LastRow; i++)
			//                {
			//                    // MD 4/12/11 - TFS67084
			//                    // Moved away from using WorksheetCell objects.
			//                    //Utilities.CopyCellFormatValue(this.CellFormatInternal, this.Worksheet.Rows[i].Cells[this.LastColumn].CellFormatInternal, value);
			//                    Utilities.CopyCellFormatValue(this.CellFormatInternal, this.Worksheet.Rows[i].GetCellFormatInternal(this.LastColumnInternal), value);
			//                }
			//            }
			//            break;

			//        case CellFormatValue.TopBorderColor:
			//        case CellFormatValue.TopBorderStyle:
			//            {
			//                WorksheetRow row = this.TopRow;

			//                // MD 4/12/11 - TFS67084
			//                // Moved away from using WorksheetCell objects.
			//                //for (int i = this.FirstColumn; i <= this.LastColumn; i++)
			//                //    Utilities.CopyCellFormatValue(this.CellFormatInternal, row.Cells[i].CellFormatInternal, value);
			//                for (short i = this.FirstColumnInternal; i <= this.LastColumnInternal; i++)
			//                    Utilities.CopyCellFormatValue(this.CellFormatInternal, row.GetCellFormatInternal(i), value);
			//            }
			//            break;

			//        default:
			//            // MD 4/12/11 - TFS67084
			//            // Moved away from using WorksheetCell objects.
			//            //foreach (WorksheetCell cell in this)
			//            //    Utilities.CopyCellFormatValue(this.CellFormatInternal, cell.CellFormatInternal, value);
			//            for (int rowIndex = this.FirstRow; rowIndex <= this.LastRow; rowIndex++)
			//            {
			//                WorksheetRow row = this.Worksheet.Rows[rowIndex];

			//                for (short columnIndex = this.FirstColumnInternal; columnIndex <= this.LastColumnInternal; columnIndex++)
			//                {
			//                    WorksheetCellFormatProxy proxy;
			//                    if (row.TryGetCellFormat(columnIndex, out proxy))
			//                        Utilities.CopyCellFormatValue(this.CellFormatInternal, proxy, value);
			//                }
			//            }
			//            break;
			//    }
			//}
			//finally
			//{
			//    this.suspendSyncronizationsToAndFromCellFormats = oldSuspendSyncronizationsToAndFromCellFormats;
			//} 

			#endregion  // Refactored
			if ((options & CellFormatValueChangedOptions.PreventMergedRegionToCellSyncronization) != 0)
				return;

			// MD 3/22/12 - TFS104630
			// If we are loading the merged cell, we shouldn't be affecting any of the cells within the merged region because
			// theoretically, they are already in the proper state.
			Workbook workbook = this.Worksheet.Workbook;
			if (workbook != null && workbook.IsLoading)
				return;

			for (int valueIndex = 0; valueIndex < values.Count; valueIndex++)
			{
				CellFormatValue value = values[valueIndex];

				// When the borders are set on the merged cell, push the values to the equivalent borders of the individual cells in the merged cell.
				switch (value)
				{
					case CellFormatValue.BottomBorderColorInfo:
					case CellFormatValue.BottomBorderStyle:
						{
							WorksheetRow row = this.Worksheet.Rows[this.LastRow];

							for (short i = this.FirstColumnInternal; i <= this.LastColumnInternal; i++)
								Utilities.CopyCellFormatValue(this.CellFormatInternal, row.GetCellFormatInternal(i), value, true, CellFormatValueChangedOptions.PreventCellToMergedRegionSyncronization);
						}
						break;

					case CellFormatValue.LeftBorderColorInfo:
					case CellFormatValue.LeftBorderStyle:
						{
							for (int i = this.FirstRow; i <= this.LastRow; i++)
							{
								Utilities.CopyCellFormatValue(this.CellFormatInternal, this.Worksheet.Rows[i].GetCellFormatInternal(this.FirstColumnInternal), value, true, CellFormatValueChangedOptions.PreventCellToMergedRegionSyncronization);
							}
						}
						break;

					case CellFormatValue.RightBorderColorInfo:
					case CellFormatValue.RightBorderStyle:
						{
							for (int i = this.FirstRow; i <= this.LastRow; i++)
							{
								Utilities.CopyCellFormatValue(this.CellFormatInternal, this.Worksheet.Rows[i].GetCellFormatInternal(this.LastColumnInternal), value, true, CellFormatValueChangedOptions.PreventCellToMergedRegionSyncronization);
							}
						}
						break;

					case CellFormatValue.TopBorderColorInfo:
					case CellFormatValue.TopBorderStyle:
						{
							WorksheetRow row = this.TopRow;

							for (short i = this.FirstColumnInternal; i <= this.LastColumnInternal; i++)
								Utilities.CopyCellFormatValue(this.CellFormatInternal, row.GetCellFormatInternal(i), value, true, CellFormatValueChangedOptions.PreventCellToMergedRegionSyncronization);
						}
						break;

					default:
						 for (int rowIndex = this.FirstRow; rowIndex <= this.LastRow; rowIndex++)
						{
							WorksheetRow row = this.Worksheet.Rows[rowIndex];

							for (short columnIndex = this.FirstColumnInternal; columnIndex <= this.LastColumnInternal; columnIndex++)
							{
								WorksheetCellFormatProxy proxy;
								if (row.TryGetCellFormat(columnIndex, out proxy))
									Utilities.CopyCellFormatValue(this.CellFormatInternal, proxy, value, true, CellFormatValueChangedOptions.PreventCellToMergedRegionSyncronization);
							}
						}
						break;
				}
			}
		}

		// MD 2/29/12 - 12.1 - Table Support
		void IWorksheetCellFormatProxyOwner.OnCellFormatValueChanging(WorksheetCellFormatProxy sender, IList<CellFormatValue> values) { }

		// MD 2/29/12 - 12.1 - Table Support
		void IWorksheetCellFormatProxyOwner.VerifyFormatOptions(WorksheetCellFormatProxy sender, WorksheetCellFormatOptions formatOptions) { }

		// MD 2/29/12 - 12.1 - Table Support
		// This is no longer needed.
		//// MD 11/1/11 - TFS94534
		//bool IWorksheetCellFormatProxyOwner.CanOwnStyleFormat
		//{
		//    get { return false; }
		//}

		// MD 1/17/12 - 12.1 - Cell Format Updates
		// This is no longer needed.
		//// MD 7/26/10 - TFS34398
		//Workbook IWorksheetCellFormatProxyOwner.Workbook
		//{
		//    get { return this.Worksheet.Workbook; }
		//}

		#endregion 

		#endregion Interfaces

		#region Base Class Overrides

		#region ApplyFormulaToRegion

		// MD 4/12/11 - TFS67084
		// Moved away from using WorksheetCell objects.
		//internal override void ApplyFormulaToRegion( Formula formula, ref IWorksheetCell firstAppliedToCell )
		//{
		//    if ( firstAppliedToCell == null )
		//        firstAppliedToCell = this;
		//
		//    formula.ApplyTo( firstAppliedToCell, this );
		//}
		internal override void ApplyFormulaToRegion(Formula formula, ref WorksheetRow firstAppliedToRow, ref short firstAppliedToColumnIndex)
		{
			WorksheetRow firstRow = this.TopRow;

			// MD 2/29/12 - 12.1 - Table Support
			// The TopRow can now be null.
			if (firstRow == null)
			{
				Utilities.DebugFail("This is unexpected");
				return;
			}

			if (firstAppliedToRow == null)
			{
				firstAppliedToRow = firstRow;
				firstAppliedToColumnIndex = this.FirstColumnInternal;
			}

			formula.ApplyTo(firstAppliedToRow, firstAppliedToColumnIndex, firstRow, this.FirstColumnInternal);
		}

		#endregion ApplyFormulaToRegion

		#region ShiftVertically

		internal override ShiftAddressResult ShiftRegion(CellShiftOperation shiftOperation, bool leaveAttachedToBottomOfWorksheet)
		{
			WorksheetRegionAddress originalAddres = this.Address;

			ShiftAddressResult result = base.ShiftRegion(shiftOperation, leaveAttachedToBottomOfWorksheet);
			if (result.DidShift && result.IsDeleted == false)
			{
				Debug.Assert(this.Worksheet != null, "The Worksheet should not be null here.");
				if (this.Worksheet != null)
				{
					for (int rowIndex = originalAddres.FirstRowIndex;
						rowIndex <= originalAddres.LastRowIndex;
						rowIndex++)
					{
						WorksheetRow oldRow = this.Worksheet.Rows[rowIndex];
						for (short columnIndex = originalAddres.FirstColumnIndex;
							columnIndex <= originalAddres.LastColumnIndex;
							columnIndex++)
						{
							oldRow.SetAssociatedMergedCellsRegion(columnIndex, null);
						}
					}

					WorksheetRegionAddress newAddress = this.Address;
					for (int rowIndex = newAddress.FirstRowIndex;
						rowIndex <= newAddress.LastRowIndex;
						rowIndex++)
					{
						WorksheetRow newRow = this.Worksheet.Rows[rowIndex];
						for (short columnIndex = newAddress.FirstColumnIndex;
							columnIndex <= newAddress.LastColumnIndex;
							columnIndex++)
						{
							newRow.SetAssociatedMergedCellsRegion(columnIndex, this);
						}
					}
				}
			}

			return result;
		}

		#endregion // ShiftVertically

		#endregion Base Class Overrides

		#region Methods

		// MD 2/17/12 - TFS101826
		#region CombineBorderValues

		private static void CombineBorderValues(WorkbookColorInfo borderColorInfo, CellBorderLineStyle borderStyle,
			ref WorkbookColorInfo commonBorderColorInfo,
			ref CellBorderLineStyle commonBorderStyle)
		{
			DiagonalBorders commonDiagonalBorders = DiagonalBorders.Default;
			WorksheetMergedCellsRegion.CombineBorderValues(borderColorInfo, commonDiagonalBorders, borderStyle,
				ref commonBorderColorInfo, ref commonDiagonalBorders, ref commonBorderStyle);
		}

		private static void CombineBorderValues(WorkbookColorInfo borderColorInfo, DiagonalBorders diagonalBorders, CellBorderLineStyle borderStyle,
			ref WorkbookColorInfo commonBorderColorInfo,
			ref DiagonalBorders commonDiagonalBorders,
			ref CellBorderLineStyle commonBorderStyle)
		{
			if (commonBorderColorInfo == null)
			{
				commonBorderColorInfo = borderColorInfo;
				commonDiagonalBorders = diagonalBorders;
				commonBorderStyle = borderStyle;
			}
			else if (
				commonBorderColorInfo != borderColorInfo ||
				commonDiagonalBorders != diagonalBorders ||
				commonBorderStyle != borderStyle)
			{
				commonBorderColorInfo = WorkbookColorInfo.Automatic;
				commonDiagonalBorders = DiagonalBorders.None;
				commonBorderStyle = CellBorderLineStyle.None;
			}
		}

		#endregion // CombineBorderValues

		// MD 5/12/10 - TFS26732
		#region GetMergedCellBorderValues



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		// MD 3/22/12 - TFS104630
		// Added a resolveExternalBorders parameter and made this internal.
		//private Dictionary<CellFormatValue, object> GetMergedCellBorderValues(CellFormatValue? value)
		internal Dictionary<CellFormatValue, object> GetMergedCellBorderValues(CellFormatValue? value, bool resolveExternalBorders)
		{
			// MD 3/26/12 - 12.1 - Table Support
			// Moved all code to the static helper method.
			return WorksheetMergedCellsRegion.GetEdgeBorderValues(this.Worksheet, this.Address, value, resolveExternalBorders);
		}

		// MD 3/26/12 - 12.1 - Table Support
		// Added a static helper method so this could be used by the WorksheetTable.
		internal static Dictionary<CellFormatValue, object> GetEdgeBorderValues(Worksheet worksheet, WorksheetRegionAddress regionAddress,
			CellFormatValue? value, bool resolveExternalBorders)
		{
			Dictionary<CellFormatValue, object> borders = new Dictionary<CellFormatValue, object>();

			// MD 2/29/12 - 12.1 - Table Support
			// The worksheet can now be null.
			if (worksheet == null)
			{
				Utilities.DebugFail("This is unexpected");
				return borders;
			}

			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
			//WorksheetCell topLeftCell = this.TopLeftCell;

			WorkbookFormat currentFormat = worksheet.CurrentFormat;

			// MD 4/12/11 - TFS67084
			WorksheetRow topRow = worksheet.Rows[regionAddress.FirstRowIndex];
			WorksheetRow bottomRow = worksheet.Rows[regionAddress.LastRowIndex];
			short leftColumnIndex = regionAddress.FirstColumnIndex;
			short rightColumnIndex = regionAddress.LastColumnIndex;

			if (value == null ||
				value == CellFormatValue.TopBorderColorInfo || value == CellFormatValue.TopBorderStyle)
			{
				// MD 4/12/11 - TFS67084
				// Moved away from using WorksheetCell objects.
				//WorksheetRow topRow = this.Worksheet.Rows[this.FirstRow];
				//WorksheetRow topBorderingRow = this.FirstRow > 0 ? this.Worksheet.Rows.GetIfCreated(this.FirstRow - 1) : null;
				//
				//if (value == null || value == CellFormatValue.TopBorderColor)
				//    this.ResolveHorizontalBorderValue(borders, topRow, topBorderingRow, topLeftCell, CellFormatValue.TopBorderColor);
				//
				//if (value == null || value == CellFormatValue.TopBorderStyle)
				//    this.ResolveHorizontalBorderValue(borders, topRow, topBorderingRow, topLeftCell, CellFormatValue.TopBorderStyle);
				// MD 3/22/12 - TFS104630
				// Check the new resolveExternalBorders parameter before getting the external row.
				//WorksheetRow topBorderingRow = this.FirstRow > 0 ? this.Worksheet.Rows.GetIfCreated(this.FirstRow - 1) : null;
				WorksheetRow topBorderingRow = (resolveExternalBorders && regionAddress.FirstRowIndex > 0)
					? worksheet.Rows.GetIfCreated(regionAddress.FirstRowIndex - 1)
					: null;

				if (value == null || value == CellFormatValue.TopBorderColorInfo)
					WorksheetMergedCellsRegion.ResolveHorizontalBorderValue(worksheet, regionAddress, borders, topRow, topBorderingRow, topRow, leftColumnIndex, CellFormatValue.TopBorderColorInfo);

				if (value == null || value == CellFormatValue.TopBorderStyle)
					WorksheetMergedCellsRegion.ResolveHorizontalBorderValue(worksheet, regionAddress, borders, topRow, topBorderingRow, topRow, leftColumnIndex, CellFormatValue.TopBorderStyle);
			}

			if (value == null ||
				value == CellFormatValue.BottomBorderColorInfo || value == CellFormatValue.BottomBorderStyle)
			{
				// MD 4/12/11 - TFS67084
				// Moved away from using WorksheetCell objects.
				//WorksheetCell bottomLeftCell = this.Worksheet.Rows[this.LastRow].Cells[this.FirstColumn];
				//WorksheetRow bottomRow = this.Worksheet.Rows[this.LastRow];
				//WorksheetRow bottomRowBorderingRow = (this.LastRow + 1) < Workbook.GetMaxRowCount(currentFormat) ? this.Worksheet.Rows.GetIfCreated(this.LastRow + 1) : null;
				//
				//if (value == null || value == CellFormatValue.BottomBorderColor)
				//    this.ResolveHorizontalBorderValue(borders, bottomRow, bottomRowBorderingRow, bottomLeftCell, CellFormatValue.BottomBorderColor);
				//
				//if (value == null || value == CellFormatValue.BottomBorderStyle)
				//    this.ResolveHorizontalBorderValue(borders, bottomRow, bottomRowBorderingRow, bottomLeftCell, CellFormatValue.BottomBorderStyle);
				// MD 3/22/12 - TFS104630
				// Check the new resolveExternalBorders parameter before getting the external row.
				//WorksheetRow bottomRowBorderingRow = (this.LastRow + 1) < Workbook.GetMaxRowCount(currentFormat) ? this.Worksheet.Rows.GetIfCreated(this.LastRow + 1) : null;
				WorksheetRow bottomRowBorderingRow = (resolveExternalBorders && (regionAddress.LastRowIndex + 1) < Workbook.GetMaxRowCount(currentFormat))
					? worksheet.Rows.GetIfCreated(regionAddress.LastRowIndex + 1)
					: null;

				if (value == null || value == CellFormatValue.BottomBorderColorInfo)
					WorksheetMergedCellsRegion.ResolveHorizontalBorderValue(worksheet, regionAddress, borders, bottomRow, bottomRowBorderingRow, bottomRow, leftColumnIndex, CellFormatValue.BottomBorderColorInfo);

				if (value == null || value == CellFormatValue.BottomBorderStyle)
					WorksheetMergedCellsRegion.ResolveHorizontalBorderValue(worksheet, regionAddress, borders, bottomRow, bottomRowBorderingRow, bottomRow, leftColumnIndex, CellFormatValue.BottomBorderStyle);
			}

			if (value == null ||
				value == CellFormatValue.LeftBorderColorInfo || value == CellFormatValue.LeftBorderStyle ||
				value == CellFormatValue.RightBorderColorInfo || value == CellFormatValue.RightBorderStyle)
			{
				// MD 4/12/11 - TFS67084
				// Moved away from using WorksheetCell objects.
				//WorksheetCell topRightCell = this.Worksheet.Rows[this.FirstRow].Cells[this.LastColumn];
				//int leftBorderingColumnIndex = this.FirstColumn > 0 ? this.FirstColumn - 1 : -1;
				//int rightBorderingColumnIndex = (this.LastColumn + 1) < Workbook.GetMaxColumnCount(currentFormat) ? this.LastColumn + 1 : -1;
				//
				//if (value == null || value == CellFormatValue.LeftBorderColor)
				//    this.ResolveVerticalBorderValue(borders, this.FirstColumn, leftBorderingColumnIndex, topLeftCell, CellFormatValue.LeftBorderColor);
				//
				//if (value == null || value == CellFormatValue.LeftBorderStyle)
				//    this.ResolveVerticalBorderValue(borders, this.FirstColumn, leftBorderingColumnIndex, topLeftCell, CellFormatValue.LeftBorderStyle);
				//
				//if (value == null || value == CellFormatValue.RightBorderColor)
				//    this.ResolveVerticalBorderValue(borders, this.LastColumn, rightBorderingColumnIndex, topRightCell, CellFormatValue.RightBorderColor);
				//
				//if (value == null || value == CellFormatValue.RightBorderStyle)
				//    this.ResolveVerticalBorderValue(borders, this.LastColumn, rightBorderingColumnIndex, topRightCell, CellFormatValue.RightBorderStyle);
				// MD 3/22/12 - TFS104630
				// Check the new resolveExternalBorders parameter before getting the external column index.
				//short leftBorderingColumnIndex = (short)(this.FirstColumn > 0 ? this.FirstColumn - 1 : -1);
				//short rightBorderingColumnIndex = (short)((this.LastColumn + 1) < Workbook.GetMaxColumnCount(currentFormat) ? this.LastColumn + 1 : -1);
				short leftBorderingColumnIndex = (short)((resolveExternalBorders && regionAddress.FirstColumnIndex > 0) ? regionAddress.FirstColumnIndex - 1 : -1);
				short rightBorderingColumnIndex = (short)((resolveExternalBorders && (regionAddress.LastColumnIndex + 1) < Workbook.GetMaxColumnCount(currentFormat)) ? regionAddress.LastColumnIndex + 1 : -1);

				if (value == null || value == CellFormatValue.LeftBorderColorInfo)
					WorksheetMergedCellsRegion.ResolveVerticalBorderValue(worksheet, regionAddress, borders, regionAddress.FirstColumnIndex, leftBorderingColumnIndex, topRow, leftColumnIndex, CellFormatValue.LeftBorderColorInfo);

				if (value == null || value == CellFormatValue.LeftBorderStyle)
					WorksheetMergedCellsRegion.ResolveVerticalBorderValue(worksheet, regionAddress, borders, regionAddress.FirstColumnIndex, leftBorderingColumnIndex, topRow, leftColumnIndex, CellFormatValue.LeftBorderStyle);

				if (value == null || value == CellFormatValue.RightBorderColorInfo)
					WorksheetMergedCellsRegion.ResolveVerticalBorderValue(worksheet, regionAddress, borders, regionAddress.LastColumnIndex, rightBorderingColumnIndex, topRow, rightColumnIndex, CellFormatValue.RightBorderColorInfo);

				if (value == null || value == CellFormatValue.RightBorderStyle)
					WorksheetMergedCellsRegion.ResolveVerticalBorderValue(worksheet, regionAddress, borders, regionAddress.LastColumnIndex, rightBorderingColumnIndex, topRow, rightColumnIndex, CellFormatValue.RightBorderStyle);
			}

			return borders;
		}

		#endregion // GetMergedCellBorderValues

		#region GetResolvedCellFormat

		/// <summary>
		/// Gets the resolved cell formatting for this merged cell region.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// If any cell format properties are the default values on the merged cell region, the values from the owning row's cell format will be used.
		/// If those are default, then the values from the owning column's cell format will be used. Otherwise, the workbook default values
		/// will be used.
		/// </p>
		/// </remarks>
		/// <returns>A format object describing the actual formatting that will be used when displayed this cell in Microsoft Excel.</returns>
		/// <seealso cref="CellFormat"/>
		/// <seealso cref="RowColumnBase.CellFormat"/>
		public IWorksheetCellFormat GetResolvedCellFormat()
		{
			// MD 1/8/12 - 12.1 - Cell Format Updates
			// We don't need to do any resolution when getting the resolved format. Return an instance which will do the resolution when asked.
			#region Old Code

			//Worksheet worksheet = this.Worksheet;
			//Workbook workbook = worksheet.Workbook;
			//WorksheetCellFormatData emptyFormatData = (WorksheetCellFormatData)workbook.CellFormats.EmptyElement.Clone();
			//emptyFormatData.IsStyle = false;

			//WorksheetRow row = worksheet.HasRows
			//    ? worksheet.Rows.GetIfCreated(this.FirstRow)
			//    : null;

			//WorksheetColumn column = worksheet.HasColumns
			//    ? worksheet.Columns.GetIfCreated(this.FirstColumn)
			//    : null;

			//WorksheetCellFormatData cellFormatData = this.HasCellFormat
			//    ? this.CellFormatInternal.Element
			//    : emptyFormatData;

			//WorksheetCellFormatData rowFormatData = row != null && row.HasCellFormat
			//    ? row.CellFormatInternal.Element
			//    : emptyFormatData;

			//// MD 4/12/11 - TFS67084
			//// Moved away from using WorksheetCell objects.
			////WorksheetCellFormatData resolvedData = WorksheetCell.GetResolvedCellFormatHelper(
			//WorksheetCellFormatData resolvedData = WorksheetCellBlock.GetResolvedCellFormatHelper(
			//    // MD 4/18/11 - TFS62026
			//    // We don't need to pass in the workbook anymore.
			//    //workbook, worksheet, column, cellFormatData, rowFormatData, emptyFormatData);
			//    worksheet, column, cellFormatData, rowFormatData, emptyFormatData);

			//resolvedData = resolvedData.ResolvedCellFormatData();
			//resolvedData.FontInternal.SetFontFormatting(resolvedData.FontInternal.Element.ResolvedFontData());
			//return resolvedData;

			#endregion // Old Code
			return new WorksheetMergedCellFormatDataResolved(this);
		}

		#endregion // GetResolvedCellFormat

		// MD 8/20/08 - Excel formula solving
		// Refactoring - The merged region no longer stores the value, it is now stored on the top-left cell of the region.
		#region Not Used

		//#region InternalSetValue
		//
		//internal void InternalSetValue( object value, bool checkForBlockingValues )
		//{
		//    WorksheetCell.ApplyNewValue( this, ref this.value, value, checkForBlockingValues );
		//}
		//
		//#endregion InternalSetValue 

		#endregion Not Used

		// MD 5/12/10 - TFS26732
		#region OnCellBorderValueChanged



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		// MD 4/12/11 - TFS67084
		// Moved away from using WorksheetCell objects.
		//internal void OnCellBorderValueChanged(WorksheetCell cell, CellFormatValue value)
		internal void OnCellBorderValueChanged(WorksheetRow row, short columnIndex, CellFormatValue value)
		{
			// MD 4/18/11 - TFS62026
			#region Refactored

			//if (this.suspendSyncronizationsToAndFromCellFormats)
			//    return;

			//bool oldSuspendSyncronizationsToAndFromCellFormats = this.suspendSyncronizationsToAndFromCellFormats;
			//this.suspendSyncronizationsToAndFromCellFormats = true;
			//try
			//{
			//    // If the border value which changed was not on the merged cell's exterior, we don't need to do anything.
			//    switch (value)
			//    {
			//        case CellFormatValue.BottomBorderColor:
			//        case CellFormatValue.BottomBorderStyle:
			//            // MD 4/12/11 - TFS67084
			//            // Moved away from using WorksheetCell objects.
			//            //if (cell.RowIndex != this.LastRow)
			//            if (row.Index != this.LastRow)
			//                return;
			//            break;

			//        case CellFormatValue.LeftBorderColor:
			//        case CellFormatValue.LeftBorderStyle:
			//            // MD 4/12/11 - TFS67084
			//            // Moved away from using WorksheetCell objects.
			//            //if (cell.ColumnIndex != this.FirstColumn)
			//            if (columnIndex != this.FirstColumn)
			//                return;
			//            break;

			//        case CellFormatValue.RightBorderColor:
			//        case CellFormatValue.RightBorderStyle:
			//            // MD 4/12/11 - TFS67084
			//            // Moved away from using WorksheetCell objects.
			//            //if (cell.ColumnIndex != this.LastColumn)
			//            if (columnIndex != this.LastColumn)
			//                return;
			//            break;

			//        case CellFormatValue.TopBorderColor:
			//        case CellFormatValue.TopBorderStyle:
			//            // MD 4/12/11 - TFS67084
			//            // Moved away from using WorksheetCell objects.
			//            //if (cell.RowIndex != this.FirstRow)
			//            if (row.Index != this.FirstRow)
			//                return;
			//            break;
			//    }

			//    // Get the new border value which should be applied on the merged cell and apply it.
			//    Dictionary<CellFormatValue, object> borders = this.GetMergedCellBorderValues(value);
			//    this.CellFormatInternal.SetValue(value, borders[value]);
			//}
			//finally
			//{
			//    this.suspendSyncronizationsToAndFromCellFormats = oldSuspendSyncronizationsToAndFromCellFormats;

			//} 

			#endregion  // Refactored

			// If the border value which changed was not on the merged cell's exterior, we don't need to do anything.
			switch (value)
			{
				case CellFormatValue.BottomBorderColorInfo:
				case CellFormatValue.BottomBorderStyle:
					if (row.Index != this.LastRow)
						return;
					break;

				case CellFormatValue.LeftBorderColorInfo:
				case CellFormatValue.LeftBorderStyle:
					if (columnIndex != this.FirstColumn)
						return;
					break;

				case CellFormatValue.RightBorderColorInfo:
				case CellFormatValue.RightBorderStyle:
					if (columnIndex != this.LastColumn)
						return;
					break;

				case CellFormatValue.TopBorderColorInfo:
				case CellFormatValue.TopBorderStyle:
					if (row.Index != this.FirstRow)
						return;
					break;
			}

			// Get the new border value which should be applied on the merged cell and apply it.
			// MD 3/22/12 - TFS104630
			// Pass False for the new parameter so we don't consider external cells. When syncing border values, we should only sync 
			// with the internal cells.
			//Dictionary<CellFormatValue, object> borders = this.GetMergedCellBorderValues(value);
			Dictionary<CellFormatValue, object> borders = this.GetMergedCellBorderValues(value, false);

			this.CellFormatInternal.SetValue(value, borders[value], true, CellFormatValueChangedOptions.PreventMergedRegionToCellSyncronization);
		}

		#endregion // OnCellBorderValueChanged

		#region OnRemovedFromCollection






		internal void OnRemovedFromCollection()
		{
			// MD 2/29/12 - 12.1 - Table Support
			// The worksheet can now be null.
			if (this.Worksheet == null)
			{
				Utilities.DebugFail("This is unexpected");
				return;
			}

			for ( int rowIndex = this.FirstRow; rowIndex <= this.LastRow; rowIndex++ )
			{
				WorksheetRow row = this.Worksheet.Rows[ rowIndex ];

				// MD 4/12/11 - TFS67084
				// Use short instead of int so we don't have to cast.
				//for ( int columnIndex = this.FirstColumn; columnIndex <= this.LastColumn; columnIndex++ )
				for (short columnIndex = this.FirstColumnInternal; columnIndex <= this.LastColumnInternal; columnIndex++)
				{
					// MD 4/12/11 - TFS67084
					// Moved away from using WorksheetCell objects.
					//WorksheetCell cell = row.Cells[ columnIndex ];
					//
					//Debug.Assert( cell.AssociatedMergedCellsRegion == this );
					//
					// Clear the merged region on the cell
					//cell.SetMergedCell( null );
					Debug.Assert(row.GetCellAssociatedMergedCellsRegionInternal(columnIndex) == this);
					row.SetMergedCellOnCell(columnIndex, null);

					// MD 8/20/08 - Excel formula solving
					// Refactoring - The merged region no longer stores the value, it is now stored on the top-left cell of the region.
					//// If the cell is the top-left cell, set the merged region's value on the cell
					//if ( rowIndex == this.FirstRow && columnIndex == this.FirstColumn )
					//{
					//    // MD  10/19/07 - BR27421
					//    // If the value is a formula, it can't be set directly, because we are trying to give it a second owner, 
					//    // which is not allowed. We need to cache the formula, clear the value (and the formula's owner), then
					//    // set the value on the cell using InternalSetValue because setting the Value directly is not allowed
					//    // for formulas.
					//    //cell.Value = this.value;
					//    object value = this.Value;
					//    this.Value = null;
					//    cell.InternalSetValue( value );
					//}
				}
			}

			this.isOnWorksheet = false;

			// The cell format is not rooted anymore
			if ( this.cellFormat != null )
				this.cellFormat.OnUnrooted();
		}

		#endregion OnRemovedFromCollection

		// MD 5/12/10 - TFS26732
		#region ResolveHorizontalBorderValue



#region Infragistics Source Cleanup (Region)










#endregion // Infragistics Source Cleanup (Region)

		// MD 4/12/11 - TFS67084
		// Moved away from using WorksheetCell objects.
		//private void ResolveHorizontalBorderValue(Dictionary<CellFormatValue, object> borders, WorksheetRow interiorRow, WorksheetRow exteriorBorderingRow, WorksheetCell leftCornerInteriorCell, CellFormatValue value)
		// MD 3/26/12 - 12.1 - Table Support
		//private void ResolveHorizontalBorderValue(Dictionary<CellFormatValue, object> borders, WorksheetRow interiorRow, WorksheetRow exteriorBorderingRow, WorksheetRow leftCornerInteriorCellRow, short leftCornerInteriorCellColumnIndex, CellFormatValue value)
		private static void ResolveHorizontalBorderValue(Worksheet worksheet, WorksheetRegionAddress regionAddress,
			Dictionary<CellFormatValue, object> borders, WorksheetRow interiorRow, WorksheetRow exteriorBorderingRow, WorksheetRow leftCornerInteriorCellRow, short leftCornerInteriorCellColumnIndex, CellFormatValue value)
		{
			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
			//WorksheetCell exteriorAdjacentCell = exteriorBorderingRow != null ? exteriorBorderingRow.Cells.GetIfCreated(this.FirstColumn) : null;
			//object leftCornerCellBorderValue = Utilities.ResolveCellBorderValue(leftCornerInteriorCell, exteriorAdjacentCell, value);
			// MD 3/22/12 - TFS104630
			// The worksheet needs to be passed in now.
			//object leftCornerCellBorderValue = Utilities.ResolveCellBorderValue(leftCornerInteriorCellRow, leftCornerInteriorCellColumnIndex, exteriorBorderingRow, this.FirstColumnInternal, value);
			object leftCornerCellBorderValue = Utilities.ResolveCellBorderValue(worksheet, leftCornerInteriorCellRow, leftCornerInteriorCellColumnIndex, exteriorBorderingRow, regionAddress.FirstColumnIndex, value);

			object interiorRowBorderValue = Utilities.ResolveCellOwnerBorderValue(interiorRow, exteriorBorderingRow, value);

			object requiredValueForAllBorders = leftCornerCellBorderValue;

			// MD 1/8/12 - 12.1 - Cell Format Updates
			// Moved IsValueDefault to the WorksheetCellFormatData.
			//if (WorksheetCellFormatProxy.IsValueDefault(value, requiredValueForAllBorders))
			if (WorksheetCellFormatData.IsValueDefault(value, requiredValueForAllBorders))
				requiredValueForAllBorders = interiorRowBorderValue;

			// MD 1/8/12 - 12.1 - Cell Format Updates
			// Moved IsValueDefault to the WorksheetCellFormatData.
			//if (WorksheetCellFormatProxy.IsValueDefault(value, requiredValueForAllBorders) == false)
			if (WorksheetCellFormatData.IsValueDefault(value, requiredValueForAllBorders) == false)
			{
				// MD 4/12/11 - TFS67084
				// Use short instead of int so we don't have to cast.
				//for (int i = this.FirstColumn + 1; i <= this.LastColumn; i++)
				for (short i = (short)(regionAddress.FirstColumnIndex + 1); i <= regionAddress.LastColumnIndex; i++)
				{
					// MD 4/12/11 - TFS67084
					// Moved away from using WorksheetCell objects.
					//WorksheetCell interiorCell = interiorRow.Cells[i];
					//WorksheetCell exteriorCell = exteriorBorderingRow != null ? exteriorBorderingRow.Cells.GetIfCreated(i) : null;
					//
					//object cellBorderValue = Utilities.ResolveCellBorderValue(interiorCell, exteriorCell, value);
					// MD 3/22/12 - TFS104630
					// The worksheet needs to be passed in now.
					//object cellBorderValue = Utilities.ResolveCellBorderValue(interiorRow, i, exteriorBorderingRow, i, value);
					object cellBorderValue = Utilities.ResolveCellBorderValue(worksheet, interiorRow, i, exteriorBorderingRow, i, value);

					// MD 1/8/12 - 12.1 - Cell Format Updates
					// Moved IsValueDefault to the WorksheetCellFormatData.
					//if (WorksheetCellFormatProxy.IsValueDefault(value, cellBorderValue))
					if (WorksheetCellFormatData.IsValueDefault(value, cellBorderValue))
						cellBorderValue = interiorRowBorderValue;

					if (Object.Equals(cellBorderValue, requiredValueForAllBorders) == false)
					{
						// MD 1/8/12 - 12.1 - Cell Format Updates
						// Moved GetDefaultValue to the WorksheetCellFormatData.
						//requiredValueForAllBorders = WorksheetCellFormatProxy.GetDefaultValue(value);
						requiredValueForAllBorders = WorksheetCellFormatData.GetDefaultValue(value);

						// MD 12/31/11 - 12.1 - Table Support
						// This is no longer true. The Style property has a null default value.
						//// MD 10/26/11 - TFS91546
						//// GetDefaultValue may return null for properties which don't have a default value (one that will resolve from owners).
						//if (requiredValueForAllBorders == null)
						//{
						//    Utilities.DebugFail("The default value is null here.");
						//    return;
						//}

						break;
					}
				}
			}

			borders.Add(value, requiredValueForAllBorders);
		} 

		#endregion // ResolveHorizontalBorderValue

		// MD 5/12/10 - TFS26732
		#region ResolveVerticalBorderValue



#region Infragistics Source Cleanup (Region)










#endregion // Infragistics Source Cleanup (Region)

		// MD 4/12/11 - TFS67084
		// Moved away from using WorksheetCell objects.
		//private void ResolveVerticalBorderValue(Dictionary<CellFormatValue, object> borders, int interiorColumnIndex, int exteriorBorderingColumnIndex, WorksheetCell topCornerInteriorCell, CellFormatValue value)
		// MD 3/26/12 - 12.1 - Table Support
		//private void ResolveVerticalBorderValue(Dictionary<CellFormatValue, object> borders, short interiorColumnIndex, short exteriorBorderingColumnIndex, WorksheetRow topCornerInteriorCellRow, short topCornerInteriorCellColumnIndex, CellFormatValue value)
		private static void ResolveVerticalBorderValue(Worksheet worksheet, WorksheetRegionAddress regionAddress,
			Dictionary<CellFormatValue, object> borders, short interiorColumnIndex, short exteriorBorderingColumnIndex, WorksheetRow topCornerInteriorCellRow, short topCornerInteriorCellColumnIndex, CellFormatValue value)
		{
			// MD 2/29/12 - 12.1 - Table Support
			// The worksheet can now be null.
			if (worksheet == null)
			{
				Utilities.DebugFail("This is unexpected");
				return;
			}

			// MD 3/15/12 - TFS104581
			//WorksheetColumn interiorColumn = this.Worksheet.Columns.GetIfCreated(interiorColumnIndex);
			//WorksheetColumn exteriorBorderingColumn = exteriorBorderingColumnIndex >= 0 ? this.Worksheet.Columns.GetIfCreated(exteriorBorderingColumnIndex) : null;
			int maxColumnCount = worksheet.Columns.MaxCount;
			WorksheetColumnBlock interiorColumn = null;
			if (0 <= interiorColumnIndex && interiorColumnIndex < maxColumnCount)
				interiorColumn = worksheet.GetColumnBlock(interiorColumnIndex);
			WorksheetColumnBlock exteriorBorderingColumn = null;
			if (0 <= exteriorBorderingColumnIndex && exteriorBorderingColumnIndex < maxColumnCount)
				exteriorBorderingColumn = worksheet.GetColumnBlock(exteriorBorderingColumnIndex);

			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
			//WorksheetCell exteriorAdjacentCell = exteriorBorderingColumnIndex >= 0 ? this.Worksheet.Rows[this.FirstRow].Cells.GetIfCreated(exteriorBorderingColumnIndex) : null;
			//object topCornerCellBorderValue = Utilities.ResolveCellBorderValue(topCornerInteriorCell, exteriorAdjacentCell, value);
			WorksheetRow exteriorAdjacentCellRow = worksheet.Rows[regionAddress.FirstRowIndex];
			// MD 3/22/12 - TFS104630
			// The worksheet needs to be passed in now.
			//object topCornerCellBorderValue = Utilities.ResolveCellBorderValue(topCornerInteriorCellRow, topCornerInteriorCellColumnIndex, exteriorAdjacentCellRow, exteriorBorderingColumnIndex, value);
			object topCornerCellBorderValue = Utilities.ResolveCellBorderValue(worksheet, topCornerInteriorCellRow, topCornerInteriorCellColumnIndex, exteriorAdjacentCellRow, exteriorBorderingColumnIndex, value);

			// MD 3/15/12 - TFS104581
			//object interiorColumnBorderValue = Utilities.ResolveCellOwnerBorderValue(interiorColumn, exteriorBorderingColumn, value);
			object interiorColumnBorderValue;
			if (interiorColumn != null)
				interiorColumnBorderValue = interiorColumn.CellFormat.GetValue(value);
			else if (exteriorBorderingColumn != null)
				interiorColumnBorderValue = exteriorBorderingColumn.CellFormat.GetValue(Utilities.GetOppositeBorderValue(value));
			else
				interiorColumnBorderValue = WorksheetCellFormatData.GetDefaultValue(value);

			object requiredValueForAllBorders = topCornerCellBorderValue;

			// MD 1/8/12 - 12.1 - Cell Format Updates
			// Moved IsValueDefault to the WorksheetCellFormatData.
			//if (WorksheetCellFormatProxy.IsValueDefault(value, requiredValueForAllBorders))
			if (WorksheetCellFormatData.IsValueDefault(value, requiredValueForAllBorders))
				requiredValueForAllBorders = interiorColumnBorderValue;

			// MD 1/8/12 - 12.1 - Cell Format Updates
			// Moved IsValueDefault to the WorksheetCellFormatData.
			//if (WorksheetCellFormatProxy.IsValueDefault(value, requiredValueForAllBorders) == false)
			if (WorksheetCellFormatData.IsValueDefault(value, requiredValueForAllBorders) == false)
			{
				for (int i = regionAddress.FirstRowIndex + 1; i <= regionAddress.LastRowIndex; i++)
				{
					WorksheetRow row = worksheet.Rows[i];

					// MD 4/12/11 - TFS67084
					// Moved away from using WorksheetCell objects.
					//WorksheetCell interiorCell = row.Cells[interiorColumnIndex];
					//WorksheetCell exteriorCell = exteriorBorderingColumnIndex < 0 ? null : row.Cells[exteriorBorderingColumnIndex];
					//
					//object cellBorderValue = Utilities.ResolveCellBorderValue(interiorCell, exteriorCell, value);
					// MD 3/22/12 - TFS104630
					// The worksheet needs to be passed in now.
					//object cellBorderValue = Utilities.ResolveCellBorderValue(row, interiorColumnIndex, row, exteriorBorderingColumnIndex, value);
					object cellBorderValue = Utilities.ResolveCellBorderValue(worksheet, row, interiorColumnIndex, row, exteriorBorderingColumnIndex, value);

					// MD 1/8/12 - 12.1 - Cell Format Updates
					// Moved IsValueDefault to the WorksheetCellFormatData.
					//if (WorksheetCellFormatProxy.IsValueDefault(value, cellBorderValue))
					if (WorksheetCellFormatData.IsValueDefault(value, cellBorderValue))
						cellBorderValue = interiorColumnBorderValue;

					if (Object.Equals(cellBorderValue, requiredValueForAllBorders) == false)
					{
						// MD 1/8/12 - 12.1 - Cell Format Updates
						// Moved GetDefaultValue to the WorksheetCellFormatData.
						//requiredValueForAllBorders = WorksheetCellFormatProxy.GetDefaultValue(value);
						requiredValueForAllBorders = WorksheetCellFormatData.GetDefaultValue(value);

						// MD 12/31/11 - 12.1 - Table Support
						// This is no longer true. The Style property has a null default value.
						//// MD 10/26/11 - TFS91546
						//// GetDefaultValue may return null for properties which don't have a default value (one that will resolve from owners).
						//if (requiredValueForAllBorders == null)
						//{
						//    Utilities.DebugFail("The default value is null here.");
						//    return;
						//}

						break;
					}
				}
			}

			borders.Add(value, requiredValueForAllBorders);
		} 

		#endregion // ResolveVerticalBorderValue

		#endregion Methods

		#region Properties

		#region CellFormat

		/// <summary>
		/// Gets the cell formatting for the merged cell region.
		/// </summary>
		/// <value>The cell formatting for the merged cell region.</value>
		public IWorksheetCellFormat CellFormat
		{
			get { return this.CellFormatInternal; }
		}

		internal bool HasCellFormat
		{
			get { return this.cellFormat != null; }
		}

		#endregion CellFormat

		#region CellFormatInternal

		internal WorksheetCellFormatProxy CellFormatInternal
		{
			get 
			{
				if ( this.cellFormat == null )
				{
					// MD 2/29/12 - 12.1 - Table Support
					// The worksheet can now be null.
					//Workbook workbook = this.Worksheet.Workbook;
					Workbook workbook = null;
					Debug.Assert(this.Worksheet != null, "This is unexpected");
					if (this.Worksheet != null)
						workbook = this.Worksheet.Workbook;

					// MD 2/2/12 - TFS100573
					//GenericCachedCollection<WorksheetCellFormatData> cellFormatCollection = this.isOnWorksheet ? workbook.CellFormats : null;
					GenericCachedCollectionEx<WorksheetCellFormatData> cellFormatCollection = this.isOnWorksheet ? workbook.CellFormats : null;

					// MD 5/12/10 - TFS26732
					// Added an owner parameter to the constructor.
					//this.cellFormat = new WorksheetCellFormatProxy( cellFormatCollection, workbook );
					// MD 4/18/11 - TFS62026
					// The workbook no longer needs to be specified.
					//this.cellFormat = new WorksheetCellFormatProxy(cellFormatCollection, workbook, this);
					this.cellFormat = new WorksheetCellFormatProxy(cellFormatCollection, this);

					// MD 2/27/12 - 12.1 - Table Support
					// This is no longer needed.
					//this.cellFormat.IsStyle = false;
				}

				return this.cellFormat; 
			}
		}

		#endregion CellFormatInternal

		// MD 9/2/08 - Cell Comments
		#region Comment

		/// <summary>
		/// Gets or sets the comment for the merged cells region.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The comment of the merged region can also be accessed from the top-left cell of the merged region of cells.
		/// </p>
		/// </remarks>
		/// <value>The comment for the merged cells region.</value>
		public WorksheetCellComment Comment
		{
			get
			{
				if ( this.isOnWorksheet == false )
					return null;

				// MD 2/29/12 - 12.1 - Table Support
				// The TopRow can now be null.
				if (this.TopRow == null)
				{
					Utilities.DebugFail("This is unexpected");
					return null;
				}

				// MD 4/12/11 - TFS67084
				// Moved away from using WorksheetCell objects.
				//return this.TopLeftCell.Comment;
				return this.TopRow.GetCellCommentInternal(this.FirstColumnInternal);
			}
			set
			{
				if ( this.isOnWorksheet == false )
					return;

				// MD 2/29/12 - 12.1 - Table Support
				// The TopRow can now be null.
				if (this.TopRow == null)
				{
					Utilities.DebugFail("This is unexpected");
					return;
				}

				// MD 4/12/11 - TFS67084
				// Moved away from using WorksheetCell objects.
				//this.TopLeftCell.Comment = value;
				this.TopRow.SetCellCommentInternal(this.FirstColumnInternal, value);
			}
		} 

		#endregion Comment

		// MD 7/14/08 - Excel formula solving
		#region Formula

		/// <summary>
		/// Gets the formula which has been applied to the merged region.
		/// </summary>
		/// <value>The formula which has been applied to the merged region or null if no formula has been applied.</value>
        /// <see cref="Excel.Formula.ApplyTo(WorksheetCell)"/>
        /// <see cref="Excel.Formula.ApplyTo(WorksheetRegion)"/>
        /// <see cref="Excel.Formula.ApplyTo(WorksheetRegion[])"/>
		/// <see cref="WorksheetRegion.ApplyFormula"/>
		public Formula Formula
		{
			// MD 8/20/08 - Excel formula solving
			// Refactoring - The merged region no longer stores the value, it is now stored on the top-left cell of the region.
			//get { return this.value as Formula; }
			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
			//get { return this.TopLeftCell.Formula; }
			get 
			{
				// MD 2/29/12 - 12.1 - Table Support
				// The TopRow can now be null.
				if (this.TopRow == null)
				{
					Utilities.DebugFail("This is unexpected");
					return null;
				}

				return this.TopRow.GetCellValueRaw(this.FirstColumnInternal) as Formula; 
			}
		}

		#endregion Formula

		#region Value

		/// <summary>
		/// Gets or sets the value of the merged cell region.
		/// </summary>
		/// <remarks>
		/// <p class="body">The types supported for the value are:
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
		/// <exception cref="System.NotSupportedException">
		/// The assigned value's type is not supported and can't be exported to Excel.
		/// </exception>
		/// <exception cref="InvalidOperationException">
        /// The value assigned is a <see cref="Formula"/>. Instead, <see cref="Excel.Formula.ApplyTo(WorksheetCell)"/> 
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
		/// <value>The value of the merged cell region.</value>
		/// <seealso cref="WorksheetCell.Value"/>
		/// <seealso cref="WorksheetCell.IsCellTypeSupported"/>
		public object Value
		{
			// MD 7/14/08 - Excel formula solving
			//get { return this.value; }
			get 
			{
				if ( this.isOnWorksheet == false )
					return null;

				// MD 2/29/12 - 12.1 - Table Support
				// The TopRow can now be null.
				if (this.TopRow == null)
				{
					Utilities.DebugFail("This is unexpected");
					return null;
				}

				// MD 8/20/08 - Excel formula solving
				// Refactoring - The merged region no longer stores the value, it is now stored on the top-left cell of the region.
				//// This is a breaking change: The Value getter will now return the calculated formula value.
				//if ( this.Formula != null )
				//{
				//    WorksheetCell topLeftCell = this.TopLeftCell;
				//
				//    topLeftCell.CalcReference.EnsureCalculated();
				//    return topLeftCell.CalculatedValue;
				//}
				//
				//return this.value; 
				// MD 4/12/11 - TFS67084
				// Moved away from using WorksheetCell objects.
				//return this.TopLeftCell.Value;
				return this.TopRow.GetCellValueInternal(this.FirstColumnInternal);
			}
			set
			{
				if ( this.isOnWorksheet == false )
					return;

				// MD 2/29/12 - 12.1 - Table Support
				// The TopRow can now be null.
				if (this.TopRow == null)
				{
					Utilities.DebugFail("This is unexpected");
					return;
				}

				// MD 8/20/08 - Excel formula solving
				// Refactoring - The merged region no longer stores the value, it is now stored on the top-left cell of the region.
				//if ( this.value != value )
				//{
				//    WorksheetCell.VerifyDirectSetValue( value );
				//    this.InternalSetValue( value, true );
				//}
				// MD 4/12/11 - TFS67084
				// Moved away from using WorksheetCell objects.
				//this.TopLeftCell.Value = value;
				this.TopRow.SetCellValue(this.FirstColumnInternal, value);
			}
		}

		#endregion Value

		// MD 8/20/08 - Excel formula solving
		// Refactoring - The merged region no longer stores the value, it is now stored on the top-left cell of the region.
		#region Not Used

		//        // MD 7/21/08 - Excel formula solving
		//        #region ValueInternal
		//
		//#if DEBUG
		//        /// <summary>
		//        /// Gets the raw value on the merged cell region. This is basically the same as the Value property, except it will not return a calculated 
		//        /// formula value. INstead it will return the Formula instance on the merged region.
		//        /// </summary> 
		//#endif
		//        internal object ValueInternal
		//        {
		//            get { return this.value; }
		//        } 
		//
		//        #endregion ValueInternal 

		#endregion Not Used

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