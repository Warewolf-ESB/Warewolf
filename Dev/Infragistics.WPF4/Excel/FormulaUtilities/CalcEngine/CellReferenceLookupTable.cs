using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Diagnostics;
using Infragistics.Documents.Excel.CalcEngine;
using Infragistics.Documents.Excel.FormulaUtilities.CalcEngine;





namespace Infragistics.Documents.Excel
{
	// MD 1/12/12 - TFS99279





	internal class CellReferenceLookupTable
	{
		#region Member Variables

		private Dictionary<Worksheet, WorksheetCellReferences> _cellReferencesByWorksheet;

		#endregion // Member Variables

		#region Constructor

		public CellReferenceLookupTable()
		{
			_cellReferencesByWorksheet = new Dictionary<Worksheet, WorksheetCellReferences>();
		}

		#endregion // Constructor

		#region Methods

		#region Add

		public void Add(CellCalcReference cellCalcReference)
		{
			cellCalcReference.IsInRecalcChain = true;

			Worksheet worksheet = cellCalcReference.Worksheet;

			WorksheetCellReferences cellReferences;
			if (_cellReferencesByWorksheet.TryGetValue(worksheet, out cellReferences) == false)
			{
				cellReferences = new WorksheetCellReferences();
				_cellReferencesByWorksheet.Add(worksheet, cellReferences);
			}

			cellReferences.Add(cellCalcReference);
		}

		#endregion  // Add

		#region Contains

		public bool Contains(ExcelRefBase reference)
		{
			Worksheet worksheet = reference.Worksheet;
			if (worksheet == null)
				return false;

			WorksheetCellReferences cellReferences;
			if (_cellReferencesByWorksheet.TryGetValue(worksheet, out cellReferences))
				return cellReferences.Contains(reference);

			return false;
		}

		#endregion  // Contains

		#region Remove

		public void Remove(CellCalcReference cellCalcReference)
		{
			cellCalcReference.IsInRecalcChain = false;

			Worksheet worksheet = cellCalcReference.Worksheet;

			WorksheetCellReferences cellReferences;
			if (_cellReferencesByWorksheet.TryGetValue(worksheet, out cellReferences))
			{
				cellReferences.Remove(cellCalcReference);
				if (cellReferences.IsEmpty)
					_cellReferencesByWorksheet.Remove(worksheet);
			}
		}

		#endregion  // Remove

		#endregion  // Methods

		#region Properties

		#region IsEmpty

		public bool IsEmpty
		{
			get { return _cellReferencesByWorksheet.Count == 0; }
		}

		#endregion  // IsEmpty

		#endregion  // Properties


		#region WorksheetCellReferences class

		private class WorksheetCellReferences
		{
			#region Member Variables

			private SortedList<int, List<short>> _rowBuckets;

			#endregion  // Member Variables

			#region Constructor

			public WorksheetCellReferences()
			{
				_rowBuckets = new SortedList<int, List<short>>();
			}

			#endregion  // Constructor

			#region Methods

			#region Add

			public void Add(CellCalcReference cellCalcReference)
			{
				short columnIndex = cellCalcReference.ColumnIndex;
				int rowIndex = cellCalcReference.Row.Index;

				List<short> rowBucket;
				if (_rowBuckets.TryGetValue(rowIndex, out rowBucket) == false)
				{
					rowBucket = new List<short>();
					_rowBuckets.Add(rowIndex, rowBucket);
				}

				int index = rowBucket.BinarySearch(columnIndex);
				if (index < 0)
					rowBucket.Insert(~index, columnIndex);
			}

			#endregion  // Add

			#region Contains

			public bool Contains(ExcelRefBase reference)
			{
				CellCalcReference cellReference = reference as CellCalcReference;
				if (cellReference != null)
					return cellReference.IsInRecalcChain;

				// MD 2/24/12 - 12.1 - Table Support
				//RegionCalcReference regionReference = reference as RegionCalcReference;
				RegionCalcReferenceBase regionReference = reference as RegionCalcReferenceBase;
				if (regionReference != null)
					return this.Contains(regionReference);

				RegionGroupCalcReference regionGroupReference = reference as RegionGroupCalcReference;
				if (regionGroupReference != null)
				{
					for (int i = 0; i < regionGroupReference.Regions.Count; i++)
					{
						if (this.Contains((ExcelRefBase)regionGroupReference.Regions[i].CalcReference))
							return true;
					}

					return false;
				}

				return false;
			}

			// MD 2/24/12 - 12.1 - Table Support
			//public bool Contains(RegionCalcReference regionReferences)
			public bool Contains(RegionCalcReferenceBase regionReferences)
			{
				WorksheetRegion region = regionReferences.Region;

				// MD 2/24/12 - 12.1 - Table Support
				if (region == null)
					return false;

				short firstColumnIndex = region.FirstColumnInternal;
				int firstRowIndex = region.FirstRow;
				short lastColumnIndex = region.LastColumnInternal;
				int lastRowIndex = region.LastRow;

				int rowBucketIndex = Utilities.BinarySearch(_rowBuckets.Keys, firstRowIndex, _rowBuckets.Comparer);
				if (rowBucketIndex < 0)
					rowBucketIndex = ~rowBucketIndex;

				for (;
					rowBucketIndex < _rowBuckets.Count && lastRowIndex < _rowBuckets.Keys[rowBucketIndex];
					rowBucketIndex++)
				{
					List<short> rowBucket = _rowBuckets.Values[rowBucketIndex];

					int columnBucketIndex = rowBucket.BinarySearch(firstColumnIndex);
					if (0 <= columnBucketIndex)
						return true;

					columnBucketIndex = ~columnBucketIndex;
					if (columnBucketIndex < rowBucket.Count && lastColumnIndex < rowBucket[columnBucketIndex])
						return true;
				}

				return false;
			}

			#endregion  // Contains

			#region Remove

			public void Remove(CellCalcReference cellCalcReference)
			{
				int rowIndex = cellCalcReference.Row.Index;

				List<short> rowBucket;
				if (_rowBuckets.TryGetValue(rowIndex, out rowBucket) == false)
					return;

				int index = rowBucket.BinarySearch(cellCalcReference.ColumnIndex);
				if (0 <= index)
				{
					rowBucket.RemoveAt(index);

					if (rowBucket.Count == 0)
						_rowBuckets.Remove(rowIndex);
				}
			}

			#endregion  // Remove

			#endregion  // Methods

			#region Properties

			#region IsEmpty

			public bool IsEmpty
			{
				get { return _rowBuckets.Count == 0; }
			}

			#endregion  // IsEmpty

			#endregion  // Properties
		}

		#endregion  // WorksheetCellReferences class
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