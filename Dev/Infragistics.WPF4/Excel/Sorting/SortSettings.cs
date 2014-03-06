using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using System.Security;
using System.ComponentModel;
using System.Collections;


using Infragistics.Shared;


namespace Infragistics.Documents.Excel.Sorting
{
	// MD 12/14/11 - 12.1 - Table Support



	/// <summary>
	/// Represents the settings which apply to sorting a region of values.
	/// </summary>
	/// <typeparam name="T">
	/// A type which logically contains data and can have sort condition applied to that data.
	/// </typeparam>
	/// <seealso cref="WorksheetTable.SortSettings"/>
	[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelTables)]
	public

 class SortSettings<T>
		 where T : ISortable
	{
		#region Member Variables

		private bool _caseSensitive;
		private ISortSettingsOwner _owner;
		private SortConditionCollection<T> _sortConditions;
		private CultureInfo _sortCulture;
		private SortMethod _sortMethod;

		#endregion // Member Variables

		#region Constructor

		internal SortSettings(ISortSettingsOwner owner)
		{
			_owner = owner;
		}

		#endregion // Constructor

		#region Methods

		#region OnSortSettingsModified

		internal void OnSortSettingsModified()
		{
			_owner.OnSortSettingsModified();
		}

		#endregion // OnSortSettingsModified

		#region ReapplySortConditionsToRows

		internal void ReapplySortConditionsToRows()
		{
			WorksheetRegion sortRegion = _owner.SortRegion;
			if (sortRegion == null)
				return;

			Worksheet worksheet = sortRegion.Worksheet;
			if (worksheet == null)
				return;

			// If there is only one row, there is nothing to sort.
			int sortRowCount = sortRegion.Height;
			if (sortRowCount == 1)
				return;

			// Sort conditions shouldn't be reapplied when loading.
			Workbook workbook = worksheet.Workbook;
			if (workbook != null && workbook.IsLoading)
				return;

			if (this.SortConditions.Count == 0)
				return;

			List<int> visibleRowIndexes = new List<int>(sortRowCount);
			List<VisibleRowIndexBlock> visibleRowIndexBlocks = new List<VisibleRowIndexBlock>();

			WorksheetRegionAddress sortRegionAddress = sortRegion.Address;

			VisibleRowIndexBlock currentVisibleRowBlock = null;
			for (int rowIndex = sortRegionAddress.FirstRowIndex; rowIndex <= sortRegionAddress.LastRowIndex; rowIndex++)
			{
				if (worksheet.IsRowHidden(rowIndex))
				{
					if (currentVisibleRowBlock != null)
					{
						visibleRowIndexBlocks.Add(currentVisibleRowBlock);
						currentVisibleRowBlock = null;
					}
					continue;
				}

				if (currentVisibleRowBlock == null)
					currentVisibleRowBlock = new VisibleRowIndexBlock(rowIndex);
				else
					currentVisibleRowBlock.Size++;

				visibleRowIndexes.Add(rowIndex);
			}

			if (currentVisibleRowBlock != null)
				visibleRowIndexBlocks.Add(currentVisibleRowBlock);

			RowIndexComparer comparer = new RowIndexComparer(this, worksheet, sortRegionAddress.FirstColumnIndex);
			Utilities.SortMergeGeneric(visibleRowIndexes, comparer);

			int nextVisibleIndex = 0;
			for (int i = 0; i < visibleRowIndexBlocks.Count; i++)
			{
				VisibleRowIndexBlock block = visibleRowIndexBlocks[i];
				for (int rowIndex = block.StartIndex; rowIndex <= block.EndIndex; rowIndex++)
				{
					int currentVisibleIndex = nextVisibleIndex++;
					int expectedRowIndex = visibleRowIndexes[currentVisibleIndex];

					if (rowIndex == expectedRowIndex)
						continue;

					int swapVisibleIndex = visibleRowIndexes.IndexOf(rowIndex, nextVisibleIndex);
					if (swapVisibleIndex < 0)
					{
						Utilities.DebugFail("This is unexpected.");
						continue;
					}

					int swapRowIndex = visibleRowIndexes[swapVisibleIndex];

					visibleRowIndexes[currentVisibleIndex] = swapRowIndex;
					visibleRowIndexes[swapVisibleIndex] = expectedRowIndex;

					WorksheetRow row = worksheet.Rows[expectedRowIndex];
					WorksheetRow swapRow = worksheet.Rows[swapRowIndex];
					SortSettings<T>.SwapRows(row, swapRow, sortRegionAddress.FirstColumnIndex, sortRegionAddress.LastColumnIndex);
				}
			}
		}

		#endregion // ReapplySortConditionsToRows

		#region SwapCellFormats

		private static void SwapCellFormats(WorksheetRow firstRow, WorksheetRow secondRow, short columnIndex)
		{
			WorksheetCellFormatProxy firstProxy = firstRow.GetCellFormatInternal(columnIndex);
			WorksheetCellFormatProxy secondProxy = secondRow.GetCellFormatInternal(columnIndex);
			WorksheetCellFormatData firstFormatClone = firstProxy.Element.CloneInternal();
			firstProxy.SetFormatting(secondProxy);
			secondProxy.SetFormatting(firstFormatClone);
		}

		#endregion // SwapCellFormats

		#region SwapCells

		private static void SwapCells(WorksheetRow firstRow, WorksheetRow secondRow, CellDataContext firstCellContext)
		{
			Worksheet worksheet = firstRow.Worksheet;
			short columnIndex = firstCellContext.ColumnIndex;

			WorksheetCellBlock secondCellBlock = secondRow.GetCellBlock(columnIndex);
			if (firstCellContext.HasValue || secondCellBlock.DoesCellHaveValue(columnIndex))
			{
				WorksheetCellBlock firstCellBlock = firstCellContext.CellBlock ?? firstRow.GetCellBlock(columnIndex);

				object valueRaw = firstCellBlock.GetCellValueRaw(firstRow, columnIndex);
				object otherValueRaw = secondCellBlock.GetCellValueRaw(secondRow, columnIndex);

				WorksheetCellBlock replacementBlock;
				firstCellBlock.SetCellValueRaw(firstRow, columnIndex, null, out replacementBlock);
				secondCellBlock.SetCellValueRaw(secondRow, columnIndex, null, out replacementBlock);

				firstCellBlock.SetCellValueRaw(firstRow, columnIndex, otherValueRaw, out replacementBlock);
				secondCellBlock.SetCellValueRaw(secondRow, columnIndex, valueRaw, out replacementBlock);

				Formula formula = valueRaw as Formula;
				ArrayFormula arrayFormula = formula as ArrayFormula;
				if (formula != null)
				{
					CellShiftOperation shiftOperation = new CellShiftOperation(worksheet, CellShiftType.VerticalShift,
						firstRow.Index, firstRow.Index, columnIndex, columnIndex,
						secondRow.Index - firstRow.Index);

					// MD 7/19/12
					// Found while fixing TFS116808 (Table resizing)
					// We need to update the array formula range here.
					if (arrayFormula != null)
					{
						Debug.Assert(arrayFormula.CellRange.IsSingleCell, "This is unexpected");
						ShiftAddressResult result = arrayFormula.CellRange.ShiftRegion(shiftOperation, false);
						Debug.Assert(result.DidShift && result.IsDeleted == false, "This is unexpected");
					}

					formula.OnCellsShifted(worksheet,
						shiftOperation,
						ReferenceShiftType.MaintainRelativeReferenceOffset);
				}

				Formula otherFormula = otherValueRaw as Formula;
				ArrayFormula otherArrayFormula = otherFormula as ArrayFormula;
				if (otherFormula != null)
				{
					CellShiftOperation shiftOperation = new CellShiftOperation(worksheet, CellShiftType.VerticalShift,
						secondRow.Index, secondRow.Index, columnIndex, columnIndex,
						firstRow.Index - secondRow.Index);

					// MD 7/19/12
					// Found while fixing TFS116808 (Table resizing)
					// We need to update the array formula range here.
					if (otherArrayFormula != null)
					{
						Debug.Assert(otherArrayFormula.CellRange.IsSingleCell, "This is unexpected");
						ShiftAddressResult result = otherArrayFormula.CellRange.ShiftRegion(shiftOperation, false);
						Debug.Assert(result.DidShift && result.IsDeleted == false, "This is unexpected");
					}

					otherFormula.OnCellsShifted(worksheet,
						shiftOperation,
						ReferenceShiftType.MaintainRelativeReferenceOffset);
				}
			}

			SortSettings<T>.SwapCellFormats(firstRow, secondRow, columnIndex);
		}

		#endregion // SwapCells

		#region SwapRows

		private static void SwapRows(WorksheetRow firstRow, WorksheetRow secondRow, short firstColumnIndex, short lastColumnIndex)
		{
			Worksheet worksheet = firstRow.Worksheet;

			int totalColumnCount = lastColumnIndex - firstColumnIndex + 1;
			BitArray rowSwapFlags = new BitArray(totalColumnCount);

			bool allCellsIterated = true;
			int expectedColumnIndex = firstColumnIndex;
			foreach (CellDataContext cellContext in firstRow.GetCellsWithData(firstColumnIndex, lastColumnIndex))
			{
				if (expectedColumnIndex != cellContext.ColumnIndex)
					allCellsIterated = false;

				rowSwapFlags[cellContext.ColumnIndex - firstColumnIndex] = true;
				SortSettings<T>.SwapCells(firstRow, secondRow, cellContext);

				expectedColumnIndex = cellContext.ColumnIndex + 1;
			}

			if (expectedColumnIndex != lastColumnIndex + 1)
				allCellsIterated = false;

			if (allCellsIterated == false)
			{
				foreach (CellDataContext cellContext in secondRow.GetCellsWithData(firstColumnIndex, lastColumnIndex))
				{
					int relativeColumnIndex = cellContext.ColumnIndex - firstColumnIndex;
					if (rowSwapFlags[relativeColumnIndex])
						continue;

					rowSwapFlags[relativeColumnIndex] = true;
					SortSettings<T>.SwapCells(secondRow, firstRow, cellContext);
				}

				if (firstRow.HasCellFormat || secondRow.HasCellFormat)
				{
					for (short i = 0; i < totalColumnCount; i++)
					{
						if (rowSwapFlags[i])
							continue;

						SortSettings<T>.SwapCellFormats(secondRow, firstRow, i);
					}
				}
			}
		}

		#endregion // SwapRows

		#endregion // Methods

		#region Properties

		#region Public Properties

		#region CaseSensitive

		/// <summary>
		/// Gets or sets the value which indicates whether strings should be compared case-sensitively when they are sorted.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// This is only applicable to sort conditions which sort strings.
		/// </p>
		/// </remarks>
		/// <value>True to sort strings case-sensitively; False to ignore case.</value>
		public bool CaseSensitive
		{
			get { return _caseSensitive; }
			set
			{
				if (this.CaseSensitive == value)
					return;

				_caseSensitive = value;
				this.OnSortSettingsModified();
			}
		}

		#endregion // CaseSensitive

		#region SortConditions

		/// <summary>
		/// Gets the collection of sort conditions to use when sorting the region of data.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// If these settings are used in a <see cref="WorksheetTable"/>, each sort condition in the collection applies to a 
		/// <see cref="WorksheetTableColumn"/>.
		/// </p>
		/// <p class="body">
		/// This collection is ordered based on precedence. The first sort condition in the collection has the highest sort precedence.
		/// </p>
		/// </remarks>
		/// <seealso cref="WorksheetTableColumn.SortCondition"/>
		public SortConditionCollection<T> SortConditions
		{
			get
			{
				if (_sortConditions == null)
					_sortConditions = new SortConditionCollection<T>(this);

				return _sortConditions;
			}
		}

		#endregion // SortConditions

		#endregion // Public Properties

		#region Internal Properties

		#region HasAlternateSortMethod

		internal bool HasAlternateSortMethod
		{
			get
			{
				// MD 4/9/12 - TFS101506
				//if (CultureInfo.CurrentCulture.Name == "zh-TW")
				if (_owner.Culture.Name == "zh-TW")
					return this.SortMethod == SortMethod.PinYin;

				return this.SortMethod == SortMethod.Stroke;
			}
		}

		#endregion // HasAlternateSortMethod

		#region IsDirty

		internal bool IsDirty
		{
			get
			{
				return
					_caseSensitive ||
					_sortMethod != SortMethod.Default ||
					(_sortConditions != null && _sortConditions.Count != 0);
			}
		}

		#endregion // IsDirty

		#region SortCulture

		internal CultureInfo SortCulture
		{
			get
			{
				if (_sortCulture == null)
				{
					// MD 4/9/12 - TFS101506
					//_sortCulture = CultureInfo.CurrentCulture;
					_sortCulture = _owner.Culture;


					// http://msdn.microsoft.com/en-us/library/a7zyyk0c.aspx
					if (this.SortMethod == Sorting.SortMethod.Stroke)
					{
						switch (_sortCulture.LCID)
						{
							case 0x0804: // zh-CN
							case 0x0C04: // zh-HK
							case 0x1004: // zh-SG
							case 0x1404: // zh-MO
								_sortCulture = new CultureInfo(0x20000 | _sortCulture.LCID);
								break;
						}
					}
					else if (_sortCulture.LCID == 0x0404) // zh-TW
					{
						_sortCulture = new CultureInfo(0x30404);
					}

				}

				return _sortCulture;
			}
		}

		#endregion // SortCulture

		#region SortMethod



#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

		internal SortMethod SortMethod
		{
			get { return _sortMethod; }
			set
			{
				if (this.SortMethod == value)
					return;

				_sortMethod = value;

				_sortCulture = null;
				this.OnSortSettingsModified();
			}
		}

		#endregion // SortMethod

		#endregion // Internal Properties

		#endregion // Properties


		#region RowIndexComparer class

		private class RowIndexComparer : IComparer<int>
		{
			private short _columnIndexOffset;
			private SortSettings<T> _owner;
			private Worksheet _worksheet;

			public RowIndexComparer(SortSettings<T> owner, Worksheet worksheet, short columnIndexOffset)
			{
				_columnIndexOffset = columnIndexOffset;
				_owner = owner;
				_worksheet = worksheet;
			}

			int IComparer<int>.Compare(int x, int y)
			{
				for (int i = 0; i < _owner.SortConditions.Count; i++)
				{
					KeyValuePair<T, SortCondition> sortConditionPair = _owner.SortConditions[i];
					int result = sortConditionPair.Value.CompareCells(_owner, _worksheet, x, y, (short)(_columnIndexOffset + sortConditionPair.Key.Index));
					if (result != 0)
						return result;
				}

				return 0;
			}
		}

		#endregion // RowIndexComparer class

		#region VisibleRowIndexBlock class

		private class VisibleRowIndexBlock
		{
			public int Size;
			public readonly int StartIndex;

			public VisibleRowIndexBlock(int startIndex)
			{
				this.StartIndex = startIndex;
				this.Size = 1;
			}

			public int EndIndex
			{
				get { return this.StartIndex + this.Size - 1; }
			}
		}

		#endregion // VisibleRowIndexBlock class
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