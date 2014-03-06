using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Infragistics.Documents.Excel
{
	// MD 2/29/12 - 12.1 - Table Support
	internal struct CellShiftOperation
	{
		#region Member Variables

		private readonly WorksheetRegionAddress _regionAddressBeforeShift;
		private readonly int _shiftAmount;
		private readonly CellShiftType _shiftType;
		private readonly Worksheet _worksheet;

		#endregion // Member Variables

		#region Constructor

		public CellShiftOperation(Worksheet worksheet, CellShiftType shiftType,
			int firstRowIndex, int lastRowIndex, short firstColumnIndex, short lastColumnIndex, int shiftAmount)
		{
			_shiftType = shiftType;
			_regionAddressBeforeShift = new WorksheetRegionAddress(firstRowIndex, lastRowIndex, firstColumnIndex, lastColumnIndex);
			_shiftAmount = shiftAmount;
			_worksheet = worksheet;

			Debug.Assert(_shiftAmount != 0, "The shift amount should not be 0.");
			Debug.Assert(
				_shiftType != CellShiftType.VerticalRotate ||
				shiftAmount <= _regionAddressBeforeShift.Height,
				"We should not be rotating more than the amount of rows in the region.");
		}

		#endregion // Constructor

		#region Methods

		#region GetAddressBeforeShift

		public WorksheetCellAddress GetAddressBeforeShift(WorksheetCellAddress addressAfterShift)
		{
			if (this.IsVertical == false)
			{
				Utilities.DebugFail("Implement this.");
				return addressAfterShift;
			}

			switch (_shiftType)
			{
				case CellShiftType.VerticalRotate:
					if (_regionAddressBeforeShift.Contains(addressAfterShift))
					{
						if (_shiftAmount <= 0)
						{
							// We have rotated up...
							int highestWrappedRowIndex = _regionAddressBeforeShift.LastRowIndex + _shiftAmount;
							if (highestWrappedRowIndex <= addressAfterShift.RowIndex)
							{
								// The row was wrapped
								int offsetFromHighestRow = addressAfterShift.RowIndex - highestWrappedRowIndex;
								return new WorksheetCellAddress(_regionAddressBeforeShift.FirstRowIndex + offsetFromHighestRow, addressAfterShift.ColumnIndex);
							}
						}
						else
						{
							// We have rotated down...
							int lowestWrappedRowIndex = _regionAddressBeforeShift.FirstRowIndex + _shiftAmount;
							if (addressAfterShift.RowIndex <= lowestWrappedRowIndex)
							{
								// The row was wrapped
								int offsetFromLowestRow = lowestWrappedRowIndex - addressAfterShift.RowIndex;
								return new WorksheetCellAddress(_regionAddressBeforeShift.LastRowIndex - offsetFromLowestRow, addressAfterShift.ColumnIndex);
							}
						}

						// The row was shifted
						return new WorksheetCellAddress(addressAfterShift.RowIndex + _shiftAmount, addressAfterShift.ColumnIndex);
					}
					break;

				case CellShiftType.VerticalShift:
					{
						if (this.RegionAddressAfterShift.Contains(addressAfterShift))
							return new WorksheetCellAddress(addressAfterShift.RowIndex - _shiftAmount, addressAfterShift.ColumnIndex);
					}
					break;

				default:
					Utilities.DebugFail("Unknown CellShiftType: " + _shiftType);
					break;
			}

			return addressAfterShift;
		}

		#endregion // GetAddressBeforeShift

		#region MoveRowOfWorksheetCellData

		private void MoveRowOfWorksheetCellData(Worksheet worksheet,
			WorksheetRow sourceRow,
			WorksheetRow previouslyIteratedRow,
			int destinationRowIndex)
		{
			this.MoveRowOfWorksheetCellDataHelper(worksheet, sourceRow, previouslyIteratedRow, destinationRowIndex, null, null, false);
		}

		private void MoveRowOfWorksheetCellData(Worksheet worksheet,
			Dictionary<short, ExcelTuple<object, bool, IWorksheetCellFormat>> sourceCache,
			int destinationRowIndex)
		{
			this.MoveRowOfWorksheetCellDataHelper(worksheet, null, null, destinationRowIndex, sourceCache, null, false);
		}

		private void MoveRowOfWorksheetCellData(Worksheet worksheet,
			WorksheetRow sourceRow,
			Dictionary<short, ExcelTuple<object, bool, IWorksheetCellFormat>> destinationCache,
			bool destinationRowHasFormat)
		{
			this.MoveRowOfWorksheetCellDataHelper(worksheet, sourceRow, null, -1, null, destinationCache, destinationRowHasFormat);
		}

		private void MoveRowOfWorksheetCellDataHelper(Worksheet worksheet,
			WorksheetRow sourceRow,
			WorksheetRow previouslyIteratedRow,
			int destinationRowIndex,
			Dictionary<short, ExcelTuple<object, bool, IWorksheetCellFormat>> sourceCache,
			Dictionary<short, ExcelTuple<object, bool, IWorksheetCellFormat>> destinationCache,
			bool destinationRowHasFormatDefault)
		{
			Debug.Assert(sourceRow == null ^ sourceCache == null, "One of the sources should be valid.");
			Debug.Assert(destinationRowIndex == -1 ^ destinationCache == null, "One of the destinations should be valid.");

			bool sourceRowHasFormat = sourceRow == null ? false : sourceRow.HasCellFormat;
			bool destinationRowHasFormat = destinationRowHasFormatDefault;

			// Reuse the row from the last iteration if possible
			WorksheetRow destinationRow = null;
			if (0 <= destinationRowIndex)
			{
				if (previouslyIteratedRow != null &&
					previouslyIteratedRow.Index == destinationRowIndex)
				{
					destinationRow = previouslyIteratedRow;
					destinationRowHasFormat = destinationRow.HasCellFormat;
				}
				else
				{
					destinationRow = worksheet.Rows[destinationRowIndex];
					destinationRowHasFormat = destinationRow.HasCellFormat;
				}
			}

			if (sourceRow != null)
			{
				foreach (CellDataContext cellContext in sourceRow.GetCellsWithData(
					_regionAddressBeforeShift.FirstColumnIndex,
					_regionAddressBeforeShift.LastColumnIndex))
				{
					if (cellContext.CellBlock != null)
					{
						this.MoveWorksheetCellValue(cellContext.ColumnIndex,
							sourceRow, cellContext.CellBlock,
							destinationRow,
							sourceCache,
							destinationCache);
					}

					// If either row has a format, we will copy all cell formats below, so we can skip it here.
					if (sourceRowHasFormat == false && destinationRowHasFormat == false && cellContext.HasFormat)
						this.MoveWorksheetCellFormat(cellContext.ColumnIndex, sourceRow, destinationRow, sourceCache, destinationCache);
				}
			}
			else if (sourceCache != null)
			{
				foreach (short columnIndex in sourceCache.Keys)
				{
					this.MoveWorksheetCellValue(columnIndex,
						sourceRow, null,
						destinationRow,
						sourceCache,
						destinationCache);
				}
			}

			// If either row has a format, we need to iterate all cells, not just those with data and existing formats.
			if (sourceRowHasFormat || destinationRowHasFormat)
			{
				for (short columnIndex = _regionAddressBeforeShift.FirstColumnIndex;
					columnIndex <= _regionAddressBeforeShift.LastColumnIndex;
					columnIndex++)
				{
					this.MoveWorksheetCellFormat(columnIndex, sourceRow, destinationRow, sourceCache, destinationCache);
				}
			}
		}

		#endregion // MoveRowOfWorksheetCellData

		#region MoveWorksheetCellFormat

		private void MoveWorksheetCellFormat(short columnIndex,
			WorksheetRow sourceRow,
			WorksheetRow destinationRow,
			Dictionary<short, ExcelTuple<object, bool, IWorksheetCellFormat>> sourceCache,
			Dictionary<short, ExcelTuple<object, bool, IWorksheetCellFormat>> destinationCache)
		{
			IWorksheetCellFormat sourceFormat;
			if (sourceRow != null)
			{
				sourceFormat = sourceRow.GetCellFormatInternal(columnIndex);
			}
			else if (sourceCache != null)
			{
				ExcelTuple<object, bool, IWorksheetCellFormat> cellDataCache;
				if (sourceCache.TryGetValue(columnIndex, out cellDataCache) == false)
					return;

				sourceFormat = cellDataCache.Item3;
			}
			else
			{
				Utilities.DebugFail("This is unexpected.");
				return;
			}

			WorksheetCellFormatProxy sourceFormatProxy = sourceFormat as WorksheetCellFormatProxy;

			if (destinationRow != null)
			{
				destinationRow.GetCellFormatInternal(columnIndex).SetFormatting(sourceFormat);
			}
			else if (destinationCache != null)
			{
				IWorksheetCellFormat format = sourceFormatProxy != null
					? sourceFormatProxy.Element.CloneInternal()
					: sourceFormat;

				ExcelTuple<object, bool, IWorksheetCellFormat> newTuple;
				ExcelTuple<object, bool, IWorksheetCellFormat> existingTuple;
				if (destinationCache.TryGetValue(columnIndex, out existingTuple))
					newTuple = new ExcelTuple<object, bool, IWorksheetCellFormat>(existingTuple.Item1, existingTuple.Item2, format);
				else
					newTuple = new ExcelTuple<object, bool, IWorksheetCellFormat>(null, false, format);

				destinationCache[columnIndex] = newTuple;
			}
			else
			{
				Utilities.DebugFail("This is unexpected.");
				return;
			}

			if (sourceFormatProxy != null)
				sourceFormatProxy.Reset();
		}

		#endregion // MoveWorksheetCellFormat

		#region MoveWorksheetCellValue

		private void MoveWorksheetCellValue(short columnIndex,
			WorksheetRow sourceRow,
			WorksheetCellBlock sourceCellBlock,
			WorksheetRow destinationRow,
			Dictionary<short, ExcelTuple<object, bool, IWorksheetCellFormat>> sourceCache,
			Dictionary<short, ExcelTuple<object, bool, IWorksheetCellFormat>> destinationCache)
		{
			object valueRaw;
			bool isInTableHeaderOrTotalRow;
			if (sourceRow != null)
			{
				if (sourceCellBlock == null)
				{
					Utilities.DebugFail("This is unexpected.");
					return;
				}

				WorksheetCellBlock.DataType dataType;
				WorksheetCellBlock.CellValue cellValueStruct;
				valueRaw = sourceCellBlock.GetCellValueRaw(sourceRow, columnIndex, out dataType, out cellValueStruct);
				isInTableHeaderOrTotalRow = sourceCellBlock.GetIsInTableHeaderOrTotalRow(columnIndex);

				// Clear the cell first so an owned value with not throw an exception for having two owners.
				WorksheetCellBlock replacementSourceBlock;
				sourceCellBlock.SetIsInTableHeaderOrTotalRow(sourceRow, columnIndex, false, false, out replacementSourceBlock);
				Debug.Assert(replacementSourceBlock == null, "The cell block should not have been replaced when clearing the flag.");
				sourceCellBlock.SetCellValueRawDirect(sourceRow, columnIndex, null, valueRaw, dataType, cellValueStruct, true, out replacementSourceBlock);
				Debug.Assert(replacementSourceBlock == null, "The cell block should not have been replaced when clearing the value.");
			}
			else if (sourceCache != null)
			{
				ExcelTuple<object, bool, IWorksheetCellFormat> cellDataCache;
				if (sourceCache.TryGetValue(columnIndex, out cellDataCache) == false)
					return;

				valueRaw = cellDataCache.Item1;
				isInTableHeaderOrTotalRow = cellDataCache.Item2;
			}
			else
			{
				Utilities.DebugFail("This is unexpected.");
				return;
			}

			if (destinationRow != null)
			{
				WorksheetCellBlock destinationBlock = destinationRow.GetCellBlock(columnIndex);

				Debug.Assert(
					destinationBlock.GetCellValueRaw(destinationRow, columnIndex) == null,
					"We should not be replacing data. It should have already been moved.");

				WorksheetCellBlock replacementDestinationBlock;
				destinationBlock.SetCellValueRawDirect(destinationRow, columnIndex,
					valueRaw, null, WorksheetCellBlock.DataType.Null, new WorksheetCellBlock.CellValue(), true,
					out replacementDestinationBlock);

				if (replacementDestinationBlock != null)
					destinationBlock = replacementDestinationBlock;

				destinationBlock.SetIsInTableHeaderOrTotalRow(destinationRow, columnIndex, isInTableHeaderOrTotalRow, false, out replacementDestinationBlock);
			}
			else if (destinationCache != null)
			{
				Debug.Assert(destinationCache.ContainsKey(columnIndex) == false, "This is unexpected.");
				destinationCache[columnIndex] = new ExcelTuple<object, bool, IWorksheetCellFormat>(valueRaw, isInTableHeaderOrTotalRow, null);
			}
			else
			{
				Utilities.DebugFail("This is unexpected.");
				return;
			}
		}

		#endregion // MoveWorksheetCellValue

		#region ShiftCellAddress

		public ShiftAddressResult ShiftCellAddress(ref WorksheetCellAddress cellAddress)
		{
			int rowIndex = cellAddress.RowIndex;
			ShiftAddressResult result = this.ShiftCellAddress(ref rowIndex, cellAddress.ColumnIndex);

			if (result.DidShift)
				cellAddress.RowIndex = rowIndex;

			return result;
		}

		public ShiftAddressResult ShiftCellAddress(ref int cellRowIndex, short cellColumnIndex)
		{
			if (this.IsVertical == false)
			{
				Utilities.DebugFail("Implement this.");
				return ShiftAddressResult.NoShiftResult;
			}

			switch (_shiftType)
			{
				case CellShiftType.VerticalRotate:
					if (_regionAddressBeforeShift.Contains(cellRowIndex, cellColumnIndex))
					{
						if (_shiftAmount <= 0)
						{
							// We are rotating up...
							int indexOfLowestRowToWrap = _regionAddressBeforeShift.FirstRowIndex - _shiftAmount - 1;
							if (cellRowIndex <= indexOfLowestRowToWrap)
							{
								// The row should be wrapped
								int offsetFromLowestRow = indexOfLowestRowToWrap - cellRowIndex;
								cellRowIndex = _regionAddressBeforeShift.LastRowIndex - offsetFromLowestRow;
								return new ShiftAddressResult(true);
							}
						}
						else
						{
							// We are rotating down...
							int highestRowIndexToWrap = _regionAddressBeforeShift.LastRowIndex - _shiftAmount + 1;
							if (highestRowIndexToWrap <= cellRowIndex)
							{
								// The row should be wrapped
								int offsetFromLowestRow = highestRowIndexToWrap - cellRowIndex;
								cellRowIndex = _regionAddressBeforeShift.FirstRowIndex + offsetFromLowestRow;
								return new ShiftAddressResult(true);
							}
						}

						// The row was shifted down
						cellRowIndex += _shiftAmount;
						return new ShiftAddressResult(true);
					}
					break;

				case CellShiftType.VerticalShift:
					if (_regionAddressBeforeShift.FirstColumnIndex <= cellColumnIndex &&
						cellColumnIndex <= _regionAddressBeforeShift.LastColumnIndex)
					{
						CellShiftDeleteReason deleteReason = CellShiftDeleteReason.NotDeleted;
						bool didShift = false;

						if (_regionAddressBeforeShift.FirstRowIndex <= cellRowIndex &&
							cellRowIndex <= _regionAddressBeforeShift.LastRowIndex)
						{
							// If the cell is in the shifted area, shift it as well.
							cellRowIndex += _shiftAmount;
							didShift = true;

							if (cellRowIndex < 0)
								deleteReason = CellShiftDeleteReason.ShiftedOffWorksheetTop;
							else if (_worksheet.Rows.MaxCount <= cellRowIndex)
								deleteReason = CellShiftDeleteReason.ShiftedOffWorksheetBottom;
						}
						else
						{
							if (_shiftAmount <= 0)
							{
								if (_regionAddressBeforeShift.FirstRowIndex + _shiftAmount <= cellRowIndex &&
									cellRowIndex < _regionAddressBeforeShift.FirstRowIndex)
								{
									didShift = true;

									// Otherwise, if this is a shift up and the cell is in the area where the shifted area is 
									// moving into, it is now a #REF! error.
									if (cellRowIndex + _shiftAmount < 0)
										deleteReason = CellShiftDeleteReason.ShiftedOffWorksheetTop;
									else
										deleteReason = CellShiftDeleteReason.ShiftUpCoveredAddress;
								}
							}
							else
							{
								if (_regionAddressBeforeShift.LastRowIndex < cellRowIndex &&
									cellRowIndex <= _regionAddressBeforeShift.LastRowIndex + _shiftAmount)
								{
									didShift = true;

									// Otherwise, if this is a shift down and the cell is in the area where the shifted area is 
									// moving into, it is now a #REF! error.
									if (_worksheet.Rows.MaxCount <= cellRowIndex + _shiftAmount)
										deleteReason = CellShiftDeleteReason.ShiftedOffWorksheetBottom;
									else
										deleteReason = CellShiftDeleteReason.ShiftDownCoveredAddress;
								}
							}
						}

						return new ShiftAddressResult(didShift, deleteReason);
					}
					break;

				default:
					Utilities.DebugFail("Unknown CellShiftType: " + _shiftType);
					break;
			}

			return ShiftAddressResult.NoShiftResult;
		}

		#endregion // ShiftCellAddress

		#region ShiftRegionAddress

		public ShiftAddressResult ShiftRegionAddress(ref WorksheetRegionAddress regionAddress, bool leaveAttachedToBottomOfWorksheet)
		{
			List<WorksheetRegionAddress> splitRegionAddresses;
			ShiftAddressResult result = this.ShiftRegionAddress(ref regionAddress,
				leaveAttachedToBottomOfWorksheet, false,
				out splitRegionAddresses);

			Debug.Assert(
				splitRegionAddresses == null || splitRegionAddresses.Count == 0,
				"There should be no split regions here because we disallowed them.");

			return result;
		}

		public ShiftAddressResult ShiftRegionAddress(ref WorksheetRegionAddress regionAddress,
			bool leaveAttachedToBottomOfWorksheet, bool allowSplitShift,
			out List<WorksheetRegionAddress> splitRegionAddresses)
		{
			splitRegionAddresses = null;
			if (this.IsVertical == false)
			{
				Utilities.DebugFail("Implement this.");
				return ShiftAddressResult.NoShiftResult;
			}

			switch (_shiftType)
			{
				case CellShiftType.VerticalRotate:
					if (_regionAddressBeforeShift.IntersectsWith(regionAddress) == false)
						return ShiftAddressResult.NoShiftResult;

					break;

				case CellShiftType.VerticalShift:
					if (_shiftAmount <= 0)
					{
						if (_regionAddressBeforeShift.LastRowIndex < regionAddress.FirstRowIndex)
							return ShiftAddressResult.NoShiftResult;
					}
					else
					{
						if (regionAddress.LastRowIndex < _regionAddressBeforeShift.FirstRowIndex)
							return ShiftAddressResult.NoShiftResult;
					}
					break;

				default:
					Utilities.DebugFail("Unknown CellShiftType: " + _shiftType);
					break;
			}

			if (_regionAddressBeforeShift.FirstColumnIndex <= regionAddress.FirstColumnIndex &&
				regionAddress.LastColumnIndex <= _regionAddressBeforeShift.LastColumnIndex)
			{
				switch (_shiftType)
				{
					case CellShiftType.VerticalRotate:
						{
							int firstRowIndex = regionAddress.FirstRowIndex;
							int lastRowIndex = regionAddress.LastRowIndex;
							short firstColumnIndex = regionAddress.FirstColumnIndex;
							short lastColumnIndex = regionAddress.LastColumnIndex;

							ShiftAddressResult topCellShiftResult = this.ShiftCellAddress(ref firstRowIndex, firstColumnIndex);
							Debug.Assert(topCellShiftResult.IsDeleted == false, "Nothing should be deleted in this operation.");

							ShiftAddressResult bottomCellShiftResult = this.ShiftCellAddress(ref lastRowIndex, lastColumnIndex);
							Debug.Assert(bottomCellShiftResult.IsDeleted == false, "Nothing should be deleted in this operation.");

							if (topCellShiftResult.DidShift || bottomCellShiftResult.DidShift)
							{
								if (topCellShiftResult.DidShift && bottomCellShiftResult.DidShift)
								{
									// If the corner cells swapped order...
									if (lastRowIndex < firstRowIndex)
									{
										if (allowSplitShift)
										{
											if (splitRegionAddresses == null)
												splitRegionAddresses = new List<WorksheetRegionAddress>();

											WorksheetRegionAddress splitRegion = _shiftAmount <= 0
												? new WorksheetRegionAddress(firstRowIndex, _regionAddressBeforeShift.LastRowIndex, firstColumnIndex, lastColumnIndex)
												: new WorksheetRegionAddress(_regionAddressBeforeShift.FirstRowIndex, lastRowIndex, firstColumnIndex, lastColumnIndex);

											splitRegionAddresses.Add(splitRegion);
										}

										// If we are rotating up, throw away the portion of the region in the rows wrapped to the bottom.
										// Otherwise, throw away the portion of the region in the rows wrapped to the top.
										if (_shiftAmount <= 0)
											firstRowIndex = _regionAddressBeforeShift.FirstRowIndex;
										else
											lastRowIndex = _regionAddressBeforeShift.LastRowIndex;
									}
								}
								else if (allowSplitShift)
								{
									// If one corner cell was in the rotated region and the other was not and we are allowed to split
									// the region, split off the portion outside the rotated region and call back into this method with
									// the portion that is only in the rotated region. If the splits are not allowed, we don't need to 
									// do anything because the new region will be from the rotated corner cell to the corner cell outside
									// the rotated region.

									WorksheetRegionAddress unshiftedRegion;
									WorksheetRegionAddress shiftedRegion;
									if (topCellShiftResult.DidShift == false)
									{
										unshiftedRegion = new WorksheetRegionAddress(
											firstRowIndex, _regionAddressBeforeShift.FirstRowIndex,
											firstColumnIndex, lastColumnIndex);

										shiftedRegion = new WorksheetRegionAddress(
											_regionAddressBeforeShift.FirstRowIndex, lastRowIndex,
											firstColumnIndex, lastColumnIndex);
									}
									else
									{
										unshiftedRegion = new WorksheetRegionAddress(
											_regionAddressBeforeShift.LastRowIndex, lastRowIndex,
											firstColumnIndex, lastColumnIndex);

										shiftedRegion = new WorksheetRegionAddress(
											firstRowIndex, _regionAddressBeforeShift.LastRowIndex,
											firstColumnIndex, lastColumnIndex);
									}

									Debug.Assert(_regionAddressBeforeShift.Contains(shiftedRegion),
										"The region we are going to pass to the recursive call should be fully contained in the rotated region.");

									ShiftAddressResult shiftedRegionResult = this.ShiftRegionAddress(ref shiftedRegion,
										leaveAttachedToBottomOfWorksheet, allowSplitShift,
										out splitRegionAddresses);
									Debug.Assert(shiftedRegionResult.DidShift, "There should have been a valid shift here.");

									regionAddress = unshiftedRegion;
									if (shiftedRegionResult.IsDeleted == false)
									{
										if (splitRegionAddresses == null)
											splitRegionAddresses = new List<WorksheetRegionAddress>();

										splitRegionAddresses.Add(shiftedRegion);
									}

									return new ShiftAddressResult(true);
								}

								regionAddress = new WorksheetRegionAddress(firstRowIndex, lastRowIndex, firstColumnIndex, lastColumnIndex);
								return new ShiftAddressResult(true);
							}
						}
						break;

					case CellShiftType.VerticalShift:
						{
							CellShiftDeleteReason deleteReason = CellShiftDeleteReason.NotDeleted;
							bool isShifted = false;
							if (_regionAddressBeforeShift.FirstRowIndex <= regionAddress.FirstRowIndex && regionAddress.FirstRowIndex <= _regionAddressBeforeShift.LastRowIndex)
							{
								// If the top of the region is in the shifted area, shift it as well.
								isShifted = true;
								regionAddress.FirstRowIndex += _shiftAmount;

								if (regionAddress.FirstRowIndex < 0)
									deleteReason = CellShiftDeleteReason.ShiftedOffWorksheetTop;
								else if (_worksheet.Rows.MaxCount <= regionAddress.FirstRowIndex)
									deleteReason = CellShiftDeleteReason.ShiftedOffWorksheetBottom;
							}
							else if (_shiftAmount > 0 &&
								_regionAddressBeforeShift.LastRowIndex < regionAddress.FirstRowIndex &&
								regionAddress.FirstRowIndex <= _regionAddressBeforeShift.LastRowIndex + _shiftAmount)
							{
								// Otherwise, if this is a shift down and the top of the region is in the area where the 
								// shifted area is moving into, move the top of the region to just below the shifted region.
								isShifted = true;
								regionAddress.FirstRowIndex = _regionAddressBeforeShift.LastRowIndex + _shiftAmount + 1;
							}

							// Regions touching the bottom of the worksheet should remain on the bottom when 
							// leaveAttachedToBottomOfWorksheet is True.
							bool preventLastRowShiftUp = leaveAttachedToBottomOfWorksheet &&
								_shiftAmount < 0 &&
								regionAddress.LastRowIndex == _worksheet.Rows.MaxCount - 1;

							if (preventLastRowShiftUp == false)
							{
								if (_regionAddressBeforeShift.FirstRowIndex <= regionAddress.LastRowIndex && regionAddress.LastRowIndex <= _regionAddressBeforeShift.LastRowIndex)
								{
									// If the bottom of the region is in the shifted area, shift it as well.
									isShifted = true;
									regionAddress.LastRowIndex += _shiftAmount;
								}
								else if (_shiftAmount < 0 &&
									_regionAddressBeforeShift.FirstRowIndex + _shiftAmount <= regionAddress.LastRowIndex && regionAddress.LastRowIndex < _regionAddressBeforeShift.FirstRowIndex)
								{
									// Otherwise, if this is a shift up and the bottom of the region is in the area where the 
									// shifted area is moving into, move the bottom of the region to just above the shifted region.
									isShifted = true;
									regionAddress.LastRowIndex = _regionAddressBeforeShift.FirstRowIndex + _shiftAmount - 1;
								}
							}

							// If the region overwritten by the shift, it is now a #REF! error.
							if (regionAddress.LastRowIndex < regionAddress.FirstRowIndex)
							{
								if (_shiftAmount <= 0)
								{
									if (regionAddress.LastRowIndex < 0)
										deleteReason = CellShiftDeleteReason.ShiftedOffWorksheetTop;
									else
										deleteReason = CellShiftDeleteReason.ShiftUpCoveredAddress;
								}
								else
								{
									if (_worksheet.Rows.MaxCount <= regionAddress.FirstRowIndex)
										deleteReason = CellShiftDeleteReason.ShiftedOffWorksheetBottom;
									else
										deleteReason = CellShiftDeleteReason.ShiftDownCoveredAddress;
								}
							}

							return new ShiftAddressResult(isShifted, deleteReason);
						}

					default:
						Utilities.DebugFail("Unknown CellShiftType: " + _shiftType);
						break;
				}
			}
			else if (allowSplitShift &&
				_regionAddressBeforeShift.FirstColumnIndex <= regionAddress.LastColumnIndex && regionAddress.FirstColumnIndex <= _regionAddressBeforeShift.LastColumnIndex)
			{
				WorksheetRegionAddress unshiftedRegion;
				WorksheetRegionAddress shiftedRegion;
				if (regionAddress.FirstColumnIndex < _regionAddressBeforeShift.FirstColumnIndex)
				{
					unshiftedRegion = new WorksheetRegionAddress(
						regionAddress.FirstRowIndex, regionAddress.LastRowIndex,
						regionAddress.FirstColumnIndex, (short)(_regionAddressBeforeShift.FirstColumnIndex - 1));

					shiftedRegion = new WorksheetRegionAddress(
						regionAddress.FirstRowIndex, regionAddress.LastRowIndex,
						_regionAddressBeforeShift.FirstColumnIndex, regionAddress.LastColumnIndex);
				}
				else
				{
					unshiftedRegion = new WorksheetRegionAddress(
						regionAddress.FirstRowIndex, regionAddress.LastRowIndex,
						(short)(_regionAddressBeforeShift.LastColumnIndex + 1), regionAddress.LastColumnIndex);

					shiftedRegion = new WorksheetRegionAddress(
						regionAddress.FirstRowIndex, regionAddress.LastRowIndex,
						regionAddress.FirstColumnIndex, _regionAddressBeforeShift.LastColumnIndex);
				}

				ShiftAddressResult shiftedRegionResult = this.ShiftRegionAddress(ref shiftedRegion,
					leaveAttachedToBottomOfWorksheet, allowSplitShift,
					out splitRegionAddresses);
				Debug.Assert(shiftedRegionResult.DidShift, "There should have been a valid shift here.");

				regionAddress = unshiftedRegion;
				if (shiftedRegionResult.IsDeleted == false)
				{
					if (splitRegionAddresses == null)
						splitRegionAddresses = new List<WorksheetRegionAddress>();

					splitRegionAddresses.Add(shiftedRegion);
				}

				return new ShiftAddressResult(true);
			}

			return ShiftAddressResult.NoShiftResult;
		}

		#endregion // ShiftRegionAddress

		#region ShiftWorksheetCellData

		public void ShiftWorksheetCellData(Worksheet worksheet)
		{
			switch (_shiftType)
			{
				case CellShiftType.VerticalRotate:
					{
						Dictionary<int, Dictionary<short, ExcelTuple<object, bool, IWorksheetCellFormat>>> wrapRowDataCache =
							new Dictionary<int, Dictionary<short, ExcelTuple<object, bool, IWorksheetCellFormat>>>();

						int firstShiftIndex;
						int lastShiftIndex;
						int firstWrapIndex;
						int lastWrapIndex;
						bool enumerateForwards;
						if (_shiftAmount <= 0)
						{
							firstShiftIndex = _regionAddressBeforeShift.FirstRowIndex - _shiftAmount;
							lastShiftIndex = _regionAddressBeforeShift.LastRowIndex;
							firstWrapIndex = _regionAddressBeforeShift.FirstRowIndex;
							lastWrapIndex = firstShiftIndex - 1;
							enumerateForwards = true;
						}
						else
						{
							firstShiftIndex = _regionAddressBeforeShift.FirstRowIndex;
							lastShiftIndex = _regionAddressBeforeShift.LastRowIndex - _shiftAmount;
							firstWrapIndex = lastShiftIndex + 1;
							lastWrapIndex = _regionAddressBeforeShift.LastRowIndex;
							enumerateForwards = true;
						}

						foreach (WorksheetRow wrapRow in worksheet.Rows.GetItemsInRange(firstWrapIndex, lastWrapIndex))
						{
							int destinationRowIndex = wrapRow.Index;
							this.ShiftCellAddress(ref destinationRowIndex, _regionAddressBeforeShift.FirstColumnIndex);
							WorksheetRow destinationRow = worksheet.Rows.GetIfCreated(destinationRowIndex);
							bool destinationRowHasFormat = destinationRow != null && destinationRow.HasCellFormat;

							Dictionary<short, ExcelTuple<object, bool, IWorksheetCellFormat>> wrapRowData =
								new Dictionary<short, ExcelTuple<object, bool, IWorksheetCellFormat>>();

							this.MoveRowOfWorksheetCellData(worksheet, wrapRow, wrapRowData, destinationRowHasFormat);
							Debug.Assert(wrapRowDataCache.ContainsKey(wrapRow.Index) == false, "This is unexpected");
							wrapRowDataCache[wrapRow.Index] = wrapRowData;
						}

						WorksheetRow previouslyIteratedRow = null;
						foreach (WorksheetRow shiftRow in worksheet.Rows.GetItemsInRange(firstShiftIndex, lastShiftIndex, enumerateForwards))
						{
							this.MoveRowOfWorksheetCellData(worksheet, shiftRow, previouslyIteratedRow, shiftRow.Index + _shiftAmount);
							previouslyIteratedRow = shiftRow;
						}

						foreach (KeyValuePair<int, Dictionary<short, ExcelTuple<object, bool, IWorksheetCellFormat>>> wrapRowDataPair in wrapRowDataCache)
						{
							int rowIndex = wrapRowDataPair.Key;
							Dictionary<short, ExcelTuple<object, bool, IWorksheetCellFormat>> wrapRowData = wrapRowDataPair.Value;

							// Translate the source row index to the destination row index
							this.ShiftCellAddress(ref rowIndex, _regionAddressBeforeShift.FirstColumnIndex);

							// Copy the data back to the wrap area
							this.MoveRowOfWorksheetCellData(worksheet, wrapRowData, rowIndex);
						}
					}
					break;

				case CellShiftType.VerticalShift:
					{
						// If we are shifting up, we want to iterate the rows forwards.
						// If we are shifting down, we want to iterate the rows backwards.
						bool enumerateForwards =
							this.RegionAddressAfterShift.FirstRowIndex <
							this.RegionAddressBeforeShift.FirstRowIndex;

						WorksheetRow previouslyIteratedRow = null;
						foreach (WorksheetRow sourceRow in worksheet.Rows.GetItemsInRange(
							_regionAddressBeforeShift.FirstRowIndex,
							_regionAddressBeforeShift.LastRowIndex,
							enumerateForwards))
						{
							this.MoveRowOfWorksheetCellData(worksheet, sourceRow, previouslyIteratedRow, sourceRow.Index + _shiftAmount);
							previouslyIteratedRow = sourceRow;
						}
					}
					break;

				default:
					Utilities.DebugFail("Unknown CellShiftType: " + _shiftType);
					break;
			}
		}

		#endregion // ShiftWorksheetCellData

		#region VerifyShiftForNonSplittableItem

		public bool VerifyShiftForNonSplittableItem<T>(List<T> itemsWithinShiftRegion,
			T item,
			WorksheetRegionAddress itemRegionAddress)
		{
			switch (_shiftType)
			{
				case CellShiftType.VerticalRotate:
				case CellShiftType.VerticalShift:
					{
						WorksheetRegionAddress beforeShiftRegion = this.RegionAddressBeforeShift;
						WorksheetRegionAddress afterShiftRegion = this.RegionAddressAfterShift;

						// If the item is above the shift region, it will not be affected 
						if (itemRegionAddress.LastRowIndex < beforeShiftRegion.FirstRowIndex &&
							itemRegionAddress.LastRowIndex < afterShiftRegion.FirstRowIndex)
							return true;

						// If the item is below the shift region, it will not be affected 
						if (beforeShiftRegion.LastRowIndex < itemRegionAddress.FirstRowIndex &&
							afterShiftRegion.LastRowIndex < itemRegionAddress.FirstRowIndex)
							return true;

						if (beforeShiftRegion.Contains(itemRegionAddress))
						{
							if (_shiftType == CellShiftType.VerticalRotate)
							{
								int wrapSplitIndex = (_shiftAmount <= 0)
									? beforeShiftRegion.FirstRowIndex - _shiftAmount
									: beforeShiftRegion.LastRowIndex - _shiftAmount;

								// If part of the item will wrap around the region but not all of it, the item will be split and 
								// that is not allowed.
								bool isTopAboveWrapSplit = itemRegionAddress.FirstRowIndex < wrapSplitIndex;
								bool isBottomAboveWrapSplit = itemRegionAddress.LastRowIndex < wrapSplitIndex;
								if (isTopAboveWrapSplit != isBottomAboveWrapSplit)
									return false;
							}

							if (itemsWithinShiftRegion != null)
								itemsWithinShiftRegion.Add(item);

							return true;
						}

						if (beforeShiftRegion.IntersectsWith(itemRegionAddress) ||
							afterShiftRegion.IntersectsWith(itemRegionAddress))
							return false;

						return true;
					}

				default:
					Utilities.DebugFail("Unknown CellShiftType: " + _shiftType);
					return true;
			}
		}

		#endregion // VerifyShiftForNonSplittableItem

		#endregion // Methods

		#region Properties

		#region DeletedRegionAddress

		public WorksheetRegionAddress? DeletedRegionAddress
		{
			get
			{
				switch (_shiftType)
				{
					case CellShiftType.VerticalRotate:
						return null;

					case CellShiftType.VerticalShift:
						if (_shiftAmount <= 0)
						{
							return new WorksheetRegionAddress(
								_regionAddressBeforeShift.FirstRowIndex + _shiftAmount, _regionAddressBeforeShift.FirstRowIndex - 1,
								_regionAddressBeforeShift.FirstColumnIndex, _regionAddressBeforeShift.LastColumnIndex);
						}

						return new WorksheetRegionAddress(
							_regionAddressBeforeShift.LastRowIndex + 1, _regionAddressBeforeShift.LastRowIndex + _shiftAmount,
							_regionAddressBeforeShift.FirstColumnIndex, _regionAddressBeforeShift.LastColumnIndex);

					default:
						Utilities.DebugFail("Unknown CellShiftType: " + _shiftType);
						return null;
				}
			}
		}

		#endregion // DeletedRegionAddress

		#region InsertedRegionAddress

		public WorksheetRegionAddress? InsertedRegionAddress
		{
			get
			{
				switch (_shiftType)
				{
					case CellShiftType.VerticalRotate:
						return null;

					case CellShiftType.VerticalShift:
						if (_shiftAmount <= 0)
						{
							return new WorksheetRegionAddress(
								_regionAddressBeforeShift.LastRowIndex + _shiftAmount + 1, _regionAddressBeforeShift.LastRowIndex,
								_regionAddressBeforeShift.FirstColumnIndex, _regionAddressBeforeShift.LastColumnIndex);
						}

						return new WorksheetRegionAddress(
							_regionAddressBeforeShift.FirstRowIndex, _regionAddressBeforeShift.FirstRowIndex + _shiftAmount - 1,
							_regionAddressBeforeShift.FirstColumnIndex, _regionAddressBeforeShift.LastColumnIndex);

					default:
						Utilities.DebugFail("Unknown CellShiftType: " + _shiftType);
						return null;
				}
			}
		}

		#endregion // InsertedRegionAddress

		#region IsVertical

		public bool IsVertical
		{
			get
			{
				switch (_shiftType)
				{
					case CellShiftType.VerticalRotate:
					case CellShiftType.VerticalShift:
						return true;

					default:
						Utilities.DebugFail("Unknown CellShiftType: " + _shiftType);
						return false;
				}
			}
		}

		#endregion // IsVertical

		#region RegionAddressAfterShift

		public WorksheetRegionAddress RegionAddressAfterShift
		{
			get
			{
				switch (_shiftType)
				{
					case CellShiftType.VerticalRotate:
						return _regionAddressBeforeShift;

					case CellShiftType.VerticalShift:
						return new WorksheetRegionAddress(
							_regionAddressBeforeShift.FirstRowIndex + _shiftAmount, _regionAddressBeforeShift.LastRowIndex + _shiftAmount,
							_regionAddressBeforeShift.FirstColumnIndex, _regionAddressBeforeShift.LastColumnIndex);

					default:
						Utilities.DebugFail("Unknown CellShiftType: " + _shiftType);
						return _regionAddressBeforeShift;
				}
			}
		}

		#endregion // RegionAddressAfterShift

		#region RegionAddressBeforeShift

		public WorksheetRegionAddress RegionAddressBeforeShift
		{
			get { return _regionAddressBeforeShift; }
		}

		#endregion // RegionAddressBeforeShift

		#region Worksheet

		public Worksheet Worksheet
		{
			get { return _worksheet; }
		}

		#endregion // Worksheet

		#endregion // Properties
	}

	// MD 7/19/12 - TFS116808 (Table resizing)
	#region ShiftAddressResult class

	internal class ShiftAddressResult
	{
		public static readonly ShiftAddressResult NoShiftResult = new ShiftAddressResult(false);

		private CellShiftDeleteReason _deleteReason;
		private bool _didShift;

		public ShiftAddressResult(bool didShift)
			: this(didShift, CellShiftDeleteReason.NotDeleted) { }

		public ShiftAddressResult(bool didShift, CellShiftDeleteReason deleteReason)
		{
			_didShift = didShift;
			_deleteReason = deleteReason;
		}

		public CellShiftDeleteReason DeleteReason
		{
			get { return _deleteReason; }
		}

		public bool DidShift
		{
			get { return _didShift; }
		}

		public bool IsDeleted
		{
			get { return _deleteReason != CellShiftDeleteReason.NotDeleted; }
		}
	}

	#endregion // ShiftAddressResult class
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