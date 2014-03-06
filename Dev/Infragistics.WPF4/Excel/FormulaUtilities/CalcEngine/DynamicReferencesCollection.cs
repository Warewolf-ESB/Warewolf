using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Infragistics.Documents.Excel.CalcEngine;
using Infragistics.Documents.Excel.FormulaUtilities.Tokens;

namespace Infragistics.Documents.Excel.FormulaUtilities.CalcEngine
{
	// MD 1/25/12 - TFS100119
	internal class DynamicReferencesCollection : IExcelCalcReferenceCollection
	{
		#region Member Variables

		private List<IExcelCalcReference> _allReferences;
		private Dictionary<CellCalcReference, bool> _cellReferences;
		private List<RegionGroupCalcReference> _regionGroupReferences;

		// MD 2/24/12 - 12.1 - Table Support
		// Changed the generic type because this is also used for table and table column references now.
		//private RegionLookupTable<RegionCalcReference> _regionReferences;
		// MD 3/22/12 - TFS105885
		// The region lookup take was keeping cells from all worksheets, which is incorrect. We should have one region lookup table per worksheet.
		//private RegionLookupTable<RegionCalcReferenceBase> _regionReferences;
		private Dictionary<Worksheet, RegionLookupTable<RegionCalcReferenceBase>> _regionReferences;

		#endregion // Member Variables

		#region Constructor

		public DynamicReferencesCollection()
		{
			_allReferences = new List<IExcelCalcReference>();
			_cellReferences = new Dictionary<CellCalcReference, bool>();
			_regionGroupReferences = new List<RegionGroupCalcReference>();

			// MD 2/24/12 - 12.1 - Table Support
			//_regionReferences = new RegionLookupTable<RegionCalcReference>();
			// MD 3/22/12 - TFS105885
			//_regionReferences = new RegionLookupTable<RegionCalcReferenceBase>();
			_regionReferences = new Dictionary<Worksheet, RegionLookupTable<RegionCalcReferenceBase>>();
		}

		#endregion // Constructor

		#region Interfaces

		#region IEnumerable Members

		public System.Collections.IEnumerator GetEnumerator()
		{
			return _allReferences.GetEnumerator();
		}

		#endregion

		#endregion // Interfaces

		#region Methods

		#region Add

		public void Add(IExcelCalcReference reference)
		{
			_allReferences.Add(reference);

			CellCalcReference cellCalcReference = reference as CellCalcReference;
			if (cellCalcReference != null)
			{
				_cellReferences[cellCalcReference] = true;
				return;
			}

			// MD 2/24/12 - 12.1 - Table Support
			//RegionCalcReference regionCalcReference = reference as RegionCalcReference;
			//if (regionCalcReference != null)
			//{
			//    _regionReferences.Add(regionCalcReference.Region, regionCalcReference);
			//    return;
			//}
			RegionCalcReferenceBase regionCalcReference = reference as RegionCalcReferenceBase;
			if (regionCalcReference != null)
			{
				WorksheetRegion region = regionCalcReference.Region;
				if (region != null)
				{
					// MD 3/22/12 - TFS105885
					// We now keep a separate RegionLookupTable for each worksheet. 
					//_regionReferences.Add(region, regionCalcReference);
					Worksheet worksheet = regionCalcReference.Worksheet;
					RegionLookupTable<RegionCalcReferenceBase> regionLookupTable;
					if (_regionReferences.TryGetValue(worksheet, out regionLookupTable) == false)
					{
						regionLookupTable = new RegionLookupTable<RegionCalcReferenceBase>(worksheet);
						_regionReferences.Add(worksheet, regionLookupTable);
					}
					regionLookupTable.Add(region, regionCalcReference);
				}
				return;
			}

			RegionGroupCalcReference regionCalcGroupReference = reference as RegionGroupCalcReference;
			if (regionCalcGroupReference != null)
			{
				_regionGroupReferences.Add(regionCalcGroupReference);
				return;
			}

			// MD 2/24/12 - 12.1 - Table Support
			TableCalcReferenceBase tableCalcReferenceBase = reference as TableCalcReferenceBase;
			if (tableCalcReferenceBase != null)
			{
				WorksheetRegion region = tableCalcReferenceBase.Region;
				if (region != null)
				{
					// MD 3/22/12 - TFS105885
					// We now keep a separate RegionLookupTable for each worksheet. 
					//_regionReferences.Add(region, tableCalcReferenceBase);
					Worksheet worksheet = tableCalcReferenceBase.Worksheet;
					RegionLookupTable<RegionCalcReferenceBase> regionLookupTable;
					if (_regionReferences.TryGetValue(worksheet, out regionLookupTable) == false)
					{
						regionLookupTable = new RegionLookupTable<RegionCalcReferenceBase>(worksheet);
						_regionReferences.Add(worksheet, regionLookupTable);
					}
					regionLookupTable.Add(region, tableCalcReferenceBase);
				}

				return;
			}

			Utilities.DebugFail("Unknown reference type.");
		}

		#endregion // Add

		#region Contains

		internal bool Contains(IExcelCalcReference reference, bool checkEquality)
		{
			CellCalcReference cellCalcReference = reference as CellCalcReference;

			if (cellCalcReference != null)
			{
				if (_cellReferences.ContainsKey(cellCalcReference))
					return true;

				if (checkEquality == false)
				{
					// MD 3/22/12 - TFS105885
					// We now keep a separate RegionLookupTable for each worksheet. 
					//// MD 2/24/12 - 12.1 - Table Support
					////List<RegionCalcReference> existingReferences = _regionReferences.GetItemsContainingCell(
					//List<RegionCalcReferenceBase> existingReferences = _regionReferences.GetItemsContainingCell(
					//    cellCalcReference.Row.Index, cellCalcReference.ColumnIndex);
					//
					//if (existingReferences != null && existingReferences.Count != 0)
					//    return true;
					RegionLookupTable<RegionCalcReferenceBase> regionLookupTable;
					if (_regionReferences.TryGetValue(cellCalcReference.Worksheet, out regionLookupTable))
					{
						List<RegionCalcReferenceBase> existingReferences = regionLookupTable.GetItemsContainingCell(cellCalcReference.Row.Index, cellCalcReference.ColumnIndex);
						if (existingReferences != null && existingReferences.Count != 0)
							return true;
					}

					for (int i = 0; i < _regionGroupReferences.Count; i++)
						_regionGroupReferences[i].ContainsReference(cellCalcReference);
				}

				return false;
			}

			// MD 2/24/12 - 12.1 - Table Support
			//RegionCalcReference regionCalcReference = reference as RegionCalcReference;
			RegionCalcReferenceBase regionCalcReference = reference as RegionCalcReferenceBase;

			if (regionCalcReference != null)
			{
				// MD 2/24/12 - 12.1 - Table Support
				WorksheetRegion region = regionCalcReference.Region;
				if (region == null || region.Worksheet == null)
					return false;

				if (region.IsSingleCell)
				{
					if (region.TopRow.TryGetCellCalcReference(region.FirstColumnInternal, out cellCalcReference))
					{
						if (_cellReferences.ContainsKey(cellCalcReference))
							return true;
					}
				}

				// MD 3/22/12 - TFS105885
				// We now keep a separate RegionLookupTable for each worksheet. 
				//// MD 2/24/12 - 12.1 - Table Support
				////List<RegionCalcReference> existingReferences = _regionAndTableReferences.GetItemsContainingRange(region);
				//List<RegionCalcReferenceBase> existingReferences = _regionReferences.GetItemsContainingRange(region.Address);
				//if (existingReferences != null && existingReferences.Count != 0)
				//{
				//    if (checkEquality)
				//        return existingReferences.Contains(regionCalcReference);
				//    else
				//        return true;
				//}
				RegionLookupTable<RegionCalcReferenceBase> regionLookupTable;
				if (_regionReferences.TryGetValue(region.Worksheet, out regionLookupTable))
				{
					List<RegionCalcReferenceBase> existingReferences = regionLookupTable.GetItemsContainingRange(region.Address);

					if (existingReferences != null && existingReferences.Count != 0)
					{
						if (checkEquality)
							return existingReferences.Contains(regionCalcReference);
						else
							return true;
					}
				}

				if (checkEquality == false)
				{
					for (int i = 0; i < _regionGroupReferences.Count; i++)
						_regionGroupReferences[i].ContainsReference(regionCalcReference);
				}

				return false;
			}

			if (checkEquality)
			{
				for (int i = 0; i < _allReferences.Count; i++)
				{
					if (_allReferences[i].Equals(reference))
						return true;
				}
			}
			else
			{
				for (int i = 0; i < _allReferences.Count; i++)
				{
					if (_allReferences[i].ContainsReference(reference))
						return true;
				}
			}

			return false;
		}

		#endregion // Contains

		#region Remove

		internal void Remove(IExcelCalcReference reference)
		{
			_allReferences.Remove(reference);

			CellCalcReference cellCalcReference = reference as CellCalcReference;
			if (cellCalcReference != null)
			{
				_cellReferences.Remove(cellCalcReference);
				return;
			}

			// MD 2/24/12 - 12.1 - Table Support
			//RegionCalcReference regionCalcReference = reference as RegionCalcReference;
			RegionCalcReferenceBase regionCalcReference = reference as RegionCalcReferenceBase;
			if (regionCalcReference != null)
			{
				// MD 2/24/12 - 12.1 - Table Support
				//_regionReferences.Remove(regionCalcReference.Region, regionCalcReference);
				WorksheetRegion region = regionCalcReference.Region;
				if (region != null)
				{
					// MD 3/22/12 - TFS105885
					// We now keep a separate RegionLookupTable for each worksheet. 
					//_regionReferences.Remove(region.Address, regionCalcReference);
					RegionLookupTable<RegionCalcReferenceBase> regionLookupTable;
					if (_regionReferences.TryGetValue(regionCalcReference.Worksheet, out regionLookupTable))
					{
						regionLookupTable.Remove(region, regionCalcReference);

						if (regionLookupTable.IsEmpty)
							_regionReferences.Remove(regionCalcReference.Worksheet);
					}
				}
				return;
			}

			RegionGroupCalcReference regionCalcGroupReference = reference as RegionGroupCalcReference;
			if (regionCalcGroupReference != null)
			{
				_regionGroupReferences.Remove(regionCalcGroupReference);
				return;
			}

			Utilities.DebugFail("Unknown reference type.");
		}

		#endregion // Remove

		#endregion // Methods
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