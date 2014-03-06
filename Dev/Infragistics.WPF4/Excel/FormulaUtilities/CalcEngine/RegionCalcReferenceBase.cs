using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Diagnostics;
using Infragistics.Documents.Excel.CalcEngine;

namespace Infragistics.Documents.Excel.FormulaUtilities.CalcEngine
{
	// MD 2/24/12 - 12.1 - Table Support
	internal abstract class RegionCalcReferenceBase : ExcelRefBase
	{
		#region Member Variables

		private IExcelCalcReferenceCollection referencesCollection;

		#endregion // Member Variables

		#region Constructor

		public RegionCalcReferenceBase() { }

		#endregion Constructor

		#region Base Class Overrides

		#region CanOwnFormula

		public sealed override bool CanOwnFormula
		{
			get { return false; }
		}

		#endregion CanOwnFormula

		#region ColumnIndex

		public sealed override short ColumnIndex
		{
			get
			{
				WorksheetRegion region = this.Region;
				if (region == null)
					return -1;

				return region.FirstColumnInternal;
			}
		}

		#endregion  // ColumnIndex

		#region ContainsReference

		public sealed override bool ContainsReference(IExcelCalcReference inReference)
		{
			WorksheetRegion region = this.Region;
			if (region == null)
				return false;

			return CalcUtilities.ContainsReferenceHelper(region, inReference);
		}

		#endregion ContainsReference

		#region Context

		public sealed override object Context
		{
			get { return this.Region; }
		}

		#endregion Context

		#region Equals

		public override abstract bool Equals(object obj);

		#endregion Equals

		#region GetHashCode

		public override abstract int GetHashCode();

		#endregion GetHashCode

		#region GetRegionGroup

		public sealed override IList<WorksheetRegion> GetRegionGroup()
		{
			WorksheetRegion region = this.Region;
			if (region == null)
				return new WorksheetRegion[0];

			return new WorksheetRegion[] { region };
		}

		#endregion // GetRegionGroup

		#region IsEnumerable

		public sealed override bool IsEnumerable
		{
			get { return true; }
		}

		#endregion IsEnumerable

		#region IsSubsetReference

		public sealed override bool IsSubsetReference(IExcelCalcReference inReference)
		{
			Utilities.DebugFail("This seems to only be called on formula owners and an instance of TableCalcReferenceBase cannot own a formula.");
			return false;
		}

		#endregion IsSubsetReference

		#region References

		public override IExcelCalcReferenceCollection References
		{
			get
			{
				if (this.referencesCollection == null)
					this.referencesCollection = new RegionReferencesCollection(this);

				return this.referencesCollection;
			}
		}

		#endregion References

		#region ResolveReferenceForArrayFormula

		protected sealed override ExcelRefBase ResolveReferenceForArrayFormula(CellCalcReference formulaOwner, out ExcelCalcErrorValue errorValue)
		{
			WorksheetRegion region = this.Region;
			if (region == null)
			{
				errorValue = null;
				return ExcelReferenceError.Instance;
			}

			return CalcUtilities.GetSingleReference(formulaOwner, region, out errorValue);
		}

		#endregion ResolveReferenceForArrayFormula

		#region Row

		public sealed override WorksheetRow Row
		{
			get
			{
				WorksheetRegion region = this.Region;
				if (region == null)
					return null;

				return region.TopRow;
			}
		}

		#endregion  // Row

		#endregion Base Class Overrides

		#region Properties

		public abstract WorksheetRegion Region { get; }

		#endregion Properties


		#region RegionReferencesCollection class

		private class RegionReferencesCollection
			: IExcelCalcReferenceCollection
		{
			#region Member Variables

			private RegionCalcReferenceBase regionReference;

			#endregion Member Variables

			#region Constructor

			internal RegionReferencesCollection(RegionCalcReferenceBase regionReference)
			{
				this.regionReference = regionReference;
			}

			#endregion Constructor

			#region Interfaces

			#region IEnumerable Members

			IEnumerator IEnumerable.GetEnumerator()
			{
				ExcelCalcNumberStack numberStack = this.regionReference.NumberStack;
				IExcelCalcReference formulaOwner = numberStack == null ? null : numberStack.FormulaOwner;

				// MD 4/12/11 - TFS67084
				// Moved away from using WorksheetCell objects.
				//WorksheetCell targetCell = formulaOwner == null ? null : formulaOwner.Context as WorksheetCell;
				//WorksheetDataTable dataTable = targetCell == null ? null : targetCell.AssociatedDataTable;
				WorksheetRow targetCellRow = null;
				WorksheetDataTable dataTable = null;
				short targetCellColumnIndex = -1;
				if (formulaOwner != null)
				{
					CellCalcReference targetCellReference = formulaOwner.Context as CellCalcReference;
					targetCellRow = targetCellReference.Row;
					targetCellColumnIndex = targetCellReference.ColumnIndex;

					dataTable = targetCellRow.GetCellValueRaw(targetCellColumnIndex) as WorksheetDataTable;
				}

				// MD 4/12/11 - TFS67084
				// Moved away from using WorksheetCell objects.
				//foreach ( WorksheetCell cell in this.Region.GetCreatedCells() )
				//{
				//    CellCalcReference cellReference = cell.CalcReference;
				//
				//    // Reset the AddDynamicReferences flag on the cell reference just in case it was set before.
				//    cellReference.AddDynamicReferences = false;
				//
				//    // If we are iterating a group of references and one of them is a row or column input cell for a data table,
				//    // the cell returned should actually be the corresponding external cell in tne data table for the cell whose 
				//    // value is being calculated. If this replacement occurs, the returned cell reference is automatically a
				//    // dynamic reference.
				//    if ( dataTable != null && CalcUtilities.PerformDataTableReferenceReplacement( ref cellReference, targetCell, dataTable ) )
				//        cellReference.AddDynamicReferences = true;
				//
				//    cellReference.ExpectedParameterClass = this.regionReference.ExpectedParameterClass;
				//    cellReference.NumberStack = this.regionReference.NumberStack;
				//
				//    yield return cellReference;
				//}
				WorksheetRegion region = this.Region;

				// MD 2/24/12 - 12.1 - Table Support
				if (region == null || region.Worksheet == null)
				{
					yield return ExcelReferenceError.Instance;
					yield break;
				}

				// MD 1/14/12 - TFS99375
				// There is now a much faster way to iterate the created rows in a range.
				//for (int rowIndex = region.FirstRow; rowIndex <= region.LastRow; rowIndex++)
				//{
				//    WorksheetRow row = region.Worksheet.Rows.GetIfCreated(rowIndex);
				//    if (row == null)
				//        continue;
				foreach (WorksheetRow row in region.Worksheet.Rows.GetItemsInRange(region.FirstRow, region.LastRow))
				{
					for (short columnIndex = region.FirstColumnInternal; columnIndex <= region.LastColumnInternal; columnIndex++)
					{
						CellCalcReference cellReference;
						if (row.TryGetCellCalcReference(columnIndex, true, out cellReference) == false)
							continue;

						// Reset the AddDynamicReferences flag on the cell reference just in case it was set before.
						cellReference.AddDynamicReferences = false;

						// If we are iterating a group of references and one of them is a row or column input cell for a data table,
						// the cell returned should actually be the corresponding external cell in the data table for the cell whose 
						// value is being calculated. If this replacement occurs, the returned cell reference is automatically a
						// dynamic reference.
						if (dataTable != null && CalcUtilities.PerformDataTableReferenceReplacement(ref cellReference, targetCellRow, targetCellColumnIndex, dataTable))
							cellReference.AddDynamicReferences = true;

						cellReference.ExpectedParameterClass = this.regionReference.ExpectedParameterClass;
						cellReference.NumberStack = this.regionReference.NumberStack;

						yield return cellReference;
					}
				}
			}

			#endregion

			#endregion Interfaces

			#region Internal Properties

			#region Region

			internal WorksheetRegion Region
			{
				get
				{
					return this.regionReference.Region;
				}
			}

			#endregion Region

			#endregion Internal Properties
		}

		#endregion RegionReferencesCollection class
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