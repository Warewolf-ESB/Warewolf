using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Diagnostics;
using Infragistics.Documents.Excel.CalcEngine;

namespace Infragistics.Documents.Excel.FormulaUtilities.CalcEngine
{
	// MD 2/24/12 - 12.1 - Table Support
	internal abstract class RegionArrayProxyBase : ArrayProxy
	{
		#region Member Variables

		private ExcelCalcValue[,] array;

		#endregion // Member Variables

		#region Constructor

		public RegionArrayProxyBase()
		{
		}

		#endregion // Constructor

		#region Base Class Overrides

		#region ContainsReferences

		internal override bool ContainsReferences
		{
			get { return true; }
		}

		#endregion // ContainsReferences

		#region GetIteratorHelper

		internal override IEnumerable<KeyValuePair<int, ExcelCalcValue>> GetIteratorHelper(int dimension, int index)
		{
			WorksheetRegion region = this.Region;

			// MD 2/24/12 - 12.1 - Table Support
			if (region == null)
				yield break;

			// MD 2/29/12 - 12.1 - Table Support
			// The worksheet can now be null.
			if (region.Worksheet == null)
			{
				yield return new KeyValuePair<int, ExcelCalcValue>(
					0,
					new ExcelCalcValue(ErrorValue.InvalidCellReference.ToCalcErrorValue()));
				yield break;
			}

			short firstColumnIndex = region.FirstColumnInternal;
			int firstRowIndex = region.FirstRow;
			short lastColumnIndex = region.LastColumnInternal;
			int lastRowIndex = region.LastRow;

			switch (dimension)
			{
				case 0:
					{
						// MD 1/3/12 - TFS98766
						// I confused the differences between the absolute and relative indexes here. I have fixed the errors and also 
						// renamed the variables for clarity. 
						//short columnIndex = (short)index;
						//using (IEnumerator<WorksheetRow> rows = this.region.Worksheet.Rows.GetEnumeratorHelper(firstRowIndex, lastRowIndex))
						//{
						//    while (rows.MoveNext())
						//    {
						//        WorksheetRow row = rows.Current;
						//
						//        CellCalcReference cellCalcReference;
						//        if (row.TryGetCellCalcReference(columnIndex, out cellCalcReference))
						//            yield return new KeyValuePair<int, ExcelCalcValue>(row.Index, new ExcelCalcValue(cellCalcReference));
						//    }
						//}
						// 1/11/12 - TFS99216
						// If the cell is referenced as part of a region, it may not have a CellCalcReference, but we should return it if it 
						// has a value. That logic is now in the ShouldIterateCell method.
						//short columnIndexAbsolute = (short)(firstColumnIndex + index);
						//using (IEnumerator<WorksheetRow> rows = this.region.Worksheet.Rows.GetEnumeratorHelper(firstRowIndex, lastRowIndex))
						//{
						//    while (rows.MoveNext())
						//    {
						//        WorksheetRow row = rows.Current;
						//
						//        CellCalcReference cellCalcReference;
						//        if (row.TryGetCellCalcReference(columnIndexAbsolute, out cellCalcReference))
						//            yield return new KeyValuePair<int, ExcelCalcValue>(
						//                row.Index - firstRowIndex,
						//                new ExcelCalcValue(cellCalcReference));
						//    }
						//}
						short columnIndexAbsolute = (short)(firstColumnIndex + index);
						foreach (WorksheetRow row in region.Worksheet.Rows.GetItemsInRange(firstRowIndex, lastRowIndex))
						{
							CellCalcReference cellCalcReference;
							if (RegionArrayProxyBase.ShouldIterateCell(row, columnIndexAbsolute, out  cellCalcReference))
							{
								yield return new KeyValuePair<int, ExcelCalcValue>(
								   row.Index - firstRowIndex,
								   new ExcelCalcValue(cellCalcReference));
							}
						}
					}
					break;

				case 1:
					{
						// MD 1/3/12 - TFS98766
						// I confused the differences between the absolute and relative indexes here. I have fixed the errors and also 
						// renamed the variables for clarity. 
						//WorksheetRow row = this.region.Worksheet.Rows.GetIfCreated(index);
						//for (short i = firstColumnIndex; i <= lastColumnIndex; i++)
						//{
						//    CellCalcReference cellCalcReference;
						//    if (row.TryGetCellCalcReference(i, out cellCalcReference))
						//        yield return new KeyValuePair<int, ExcelCalcValue>(i, new ExcelCalcValue(cellCalcReference));
						//}
						int rowIndexAbsolute = firstRowIndex + index;
						WorksheetRow row = region.Worksheet.Rows.GetIfCreated(rowIndexAbsolute);

						// MD 1/12/12
						// Found while fixing TFS99279
						// If the row is null, we have nothing to iterate.
						if (row == null)
							yield break;

						for (short columnIndexAbsolute = firstColumnIndex; columnIndexAbsolute <= lastColumnIndex; columnIndexAbsolute++)
						{
							CellCalcReference cellCalcReference;

							// 1/11/12 - TFS99216
							// If the cell is referenced as part of a region, it may not have a CellCalcReference, but we should return it if it 
							// has a value. That logic is now in the ShouldIterateCell method.
							//if (row.TryGetCellCalcReference(columnIndexAbsolute, out cellCalcReference))
							//    yield return new KeyValuePair<int, ExcelCalcValue>(
							//        columnIndexAbsolute - firstColumnIndex,
							//        new ExcelCalcValue(cellCalcReference));
							if (RegionArrayProxyBase.ShouldIterateCell(row, columnIndexAbsolute, out  cellCalcReference))
							{
								yield return new KeyValuePair<int, ExcelCalcValue>(
								   columnIndexAbsolute - firstColumnIndex,
								   new ExcelCalcValue(cellCalcReference));
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
			WorksheetRegion region = this.Region;
			if (region == null)
				return 0;

			switch (dimension)
			{
				case 0:
					return region.Width;

				case 1:
					return region.Height;

				default:
					this.ThrowOutOfBoundsException();
					return -1;
			}
		}

		#endregion // GetLength

		#region GetValue

		internal override ExcelCalcValue GetValue(int x, int y)
		{
			WorksheetRegion region = this.Region;
			if (region == null || region.Worksheet == null)
				return new ExcelCalcValue(new ExcelCalcErrorValue(ExcelCalcErrorCode.Reference));

			if (x < 0 || region.Width <= x)
				this.ThrowOutOfBoundsException();

			if (y < 0 || region.Height <= y)
				this.ThrowOutOfBoundsException();

			short columnIndex = (short)(region.FirstColumnInternal + x);
			int rowIndex = region.FirstRow + y;
			return new ExcelCalcValue(region.Worksheet.Rows[rowIndex].GetCellCalcReference(columnIndex));
		}

		#endregion // GetValue

		// MD 7/5/12 - TFS112278
		#region IterateValues

		internal override void IterateValues(bool shouldIterateBlanks, ArrayProxy.ValueCallback valueCallback)
		{
			if (shouldIterateBlanks)
			{
				base.IterateValues(shouldIterateBlanks, valueCallback);
				return;
			}

			WorksheetRegion region = this.Region;

			int firstRowIndex = region.FirstRow;
			int firstColumnIndex = region.FirstColumn;
			foreach (WorksheetRow row in region.Worksheet.Rows.GetItemsInRange(firstRowIndex, region.LastRow))
			{
				int relativeRowIndex = row.Index - firstRowIndex;
				foreach (CellDataContext cellContext in row.GetCellsWithData(region.FirstColumn, region.LastColumn))
				{
					CellCalcReference cellCalcReference = row.GetCellCalcReference(cellContext.ColumnIndex);
					valueCallback(new ExcelCalcValue(cellCalcReference), relativeRowIndex, cellContext.ColumnIndex - firstColumnIndex);
				}
			}
		}

		#endregion // IterateValues

		#region ToArray

		internal override ExcelCalcValue[,] ToArray()
		{
			if (this.array == null)
				this.array = CalcUtilities.GetReferenceValuesForRegion(this.Region);

			return this.array;
		}

		#endregion // ToArray

		#endregion // Base Class Overrides

		#region Methods

		#region ShouldIterateCell

		// 1/11/12 - TFS99216
		private static bool ShouldIterateCell(WorksheetRow row, short columnIndex, out CellCalcReference cellCalcReference)
		{
			WorksheetCellBlock cellBlock;
			bool forceGetCalcReference = row.TryGetCellBlock(columnIndex, out cellBlock) &&
				cellBlock.DoesCellHaveData(row, columnIndex);

			if (forceGetCalcReference)
			{
				cellCalcReference = row.GetCellCalcReference(columnIndex);
				return true;
			}

			return row.TryGetCellCalcReference(columnIndex, out cellCalcReference);
		}

		#endregion // ShouldIterateCell

		#endregion // Methods

		#region Properties

		protected abstract WorksheetRegion Region { get; }

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