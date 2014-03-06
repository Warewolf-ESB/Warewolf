using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Infragistics.Documents.Excel.CalcEngine;
using System.Collections.ObjectModel;

namespace Infragistics.Documents.Excel.FormulaUtilities.CalcEngine
{
	internal sealed class RegionGroupCalcReference : ExcelRefBase
	{
		#region Member Variables

		private string elementName;

		// MD 12/1/11
		// Found while fixing TFS96113
		// This should be immutable because we should never modify it once it is created.
		//private List<WorksheetRegion> regionsSortedHorizontally;
		private readonly ReadOnlyCollection<WorksheetRegion> regionsSortedHorizontally;

		private IExcelCalcReferenceCollection referencesCollection;

		#endregion Member Variables

		#region Constructor

		// MD 12/1/11
		// Found while fixing TFS96113
		// The regionsSortedHorizontally collection is now immutable, so it needs to be passed in.
		//private RegionGroupCalcReference()
		//{
		//    this.regionsSortedHorizontally = new List<WorksheetRegion>();
		//}
		private RegionGroupCalcReference(List<WorksheetRegion> regionsSortedHorizontally)
		{
			this.regionsSortedHorizontally = regionsSortedHorizontally.AsReadOnly();
		}

		// MD 4/12/11 - TFS67084
		// Moved away from using WorksheetCell objects.
		//public RegionGroupCalcReference( WorksheetCell cell )
		//    : this( cell.CachedRegion ) { }
		public RegionGroupCalcReference(WorksheetRow row, short columnIndex)
			: this(row.Worksheet.GetCachedRegion(row.Index, columnIndex, row.Index, columnIndex)) { }

		// MD 12/1/11
		// Found while fixing TFS96113
		// The regionsSortedHorizontally collection is now immutable.
		//public RegionGroupCalcReference( WorksheetRegion region )
		//    : this()
		//{
		//    this.regionsSortedHorizontally.Add( region );
		//} 
		public RegionGroupCalcReference(WorksheetRegion region)
		{
			List<WorksheetRegion> regionsSortedHorizontally = new List<WorksheetRegion>();
			regionsSortedHorizontally.Add(region);
			this.regionsSortedHorizontally = regionsSortedHorizontally.AsReadOnly();
		}

		#endregion Constructor

		#region Base Class Overrides

		// MD 4/12/11 - TFS67084
		// Moved away from using WorksheetCell objects.
		//#region Cell

		//public override WorksheetCell Cell
		//{
		//    get
		//    {
		//        if ( this.regionsSortedHorizontally.Count == 0 )
		//            return null;

		//        return this.regionsSortedHorizontally[ 0 ].TopLeftCell;
		//    }
		//} 

		//#endregion Cell

		// MD 4/12/11 - TFS67084
		#region ColumnIndex

		public override short ColumnIndex
		{
			get
			{
				if (this.regionsSortedHorizontally.Count == 0)
					return -1;

				return this.regionsSortedHorizontally[0].FirstColumnInternal;
			}
		}

		#endregion  // ColumnIndex

		#region ContainsReference

		public override bool ContainsReference( IExcelCalcReference inReference )
		{
			// MD 2/24/12 - 12.1 - Table Support
			//IExcelCalcReference resolvedReference = UltraCalcEngine.GetResolvedReference( inReference );
			//
			//if ( resolvedReference is UCReference )
			//    return false;
			//
			//WorksheetRegion[] regionGroup = CalcUtilities.GetRegionGroup( resolvedReference.Context );
			IList<WorksheetRegion> regionGroup = CalcUtilities.GetRegionGroup(inReference);
			if (regionGroup == null)
				return false;

			foreach ( WorksheetRegion otherRegion in regionGroup )
			{
				foreach ( WorksheetRegion region in this.regionsSortedHorizontally )
				{
					if ( region.IntersectsWith( otherRegion ) )
						return true;
				}
			}

			return false;
		}

		#endregion ContainsReference

		#region Context

		public override object Context
		{
			get { return this.regionsSortedHorizontally; }
		}

		#endregion Context

		#region ElementName

		public override string ElementName
		{
			get 
			{
				if ( this.elementName == null && this.regionsSortedHorizontally.Count > 0 )
				{
					StringBuilder elementNameBuilder = new StringBuilder();

					foreach ( WorksheetRegion region in this.regionsSortedHorizontally )
					{
						elementNameBuilder.Append( region.ToString( CellReferenceMode.A1, true ) );
						elementNameBuilder.Append( "," );
					}

					if ( elementNameBuilder.Length >= 1 )
						elementNameBuilder.Remove( elementNameBuilder.Length - 1, 1 );

					this.elementName = elementNameBuilder.ToString();
				}

				return this.elementName; 
			}
		}

		#endregion ElementName

		// MD 12/1/11 - TFS96113
		// This is no longer needed now that we use a derived ArrayProxy for regions references.
		#region Removed

		//#region EnsureArrayValuesCreated

		//public override void EnsureArrayValuesCreated()
		//{
		//    if ( this.regionsSortedHorizontally.Count == 0 )
		//        return;

		//    ExcelCalcValue value = this.ValueInternal;
		//    ExcelCalcValue[][ , ] arrayGroup = (ExcelCalcValue[][ , ])value.Value;

		//    if ( arrayGroup[ 0 ] != null )
		//        return;

		//    ExcelCalcValue[][ , ] references = new ExcelCalcValue[ this.regionsSortedHorizontally.Count ][ , ];

		//    for ( int i = 0; i < this.regionsSortedHorizontally.Count; i++ )
		//        references[ i ] = CalcUtilities.GetReferenceValuesForRegion( this.regionsSortedHorizontally[ i ] );

		//    this.ValueInternal = new ExcelCalcValue( references );
		//}

		//#endregion EnsureArrayValuesCreated

		#endregion // Removed

		// MD 2/24/12 - 12.1 - Table Support
		#region GetRegionGroup

		public override IList<WorksheetRegion> GetRegionGroup()
		{
			return this.regionsSortedHorizontally;
		}

		#endregion // GetRegionGroup

		#region IsEnumerable

		public override bool IsEnumerable
		{
			get
			{
				return true;
			}
		}

		#endregion IsEnumerable

		#region IsSubsetReference

		public override bool IsSubsetReference( IExcelCalcReference inReference )
		{
			
#region Infragistics Source Cleanup (Region)


























#endregion // Infragistics Source Cleanup (Region)

			Utilities.DebugFail( "This seems to only be called on formula owners and an instance of RegionCalcReference cannot own a formula." );
			return false;
		} 

		#endregion IsSubsetReference

		#region References

		public override IExcelCalcReferenceCollection References
		{
			get
			{
				if ( this.referencesCollection == null )
					this.referencesCollection = new GeneralReferencesCollection( this );

				return this.referencesCollection;
			}
		}

		#endregion References

		#region ResolveReferenceForArrayFormula

		protected override ExcelRefBase ResolveReferenceForArrayFormula( CellCalcReference formulaOwner, out ExcelCalcErrorValue errorValue )
		{
			if ( this.regionsSortedHorizontally.Count != 1 )
			{
				errorValue = new ExcelCalcErrorValue( ExcelCalcErrorCode.Value );
				return null;
			}

			return CalcUtilities.GetSingleReference( formulaOwner, this.regionsSortedHorizontally[ 0 ], out errorValue );
		}

		#endregion ResolveReferenceForArrayFormula

		// MD 4/12/11 - TFS67084
		#region Row

		public override WorksheetRow Row
		{
			get
			{
				if (this.regionsSortedHorizontally.Count == 0)
					return null;

				WorksheetRegion region = this.regionsSortedHorizontally[0];
				return region.TopRow;
			}
		}

		#endregion  // Row

		#region ValueInternal

		protected override ExcelCalcValue ValueInternal
		{
			get
			{
				ExcelCalcValue value = base.ValueInternal;

				if ( value == null )
				{
					// MD 12/1/11 - TFS96113
					// Instead of storing arrays for rectangular regions of values we now use an ArrayProxy.
					//value = new ExcelCalcValue( new ExcelCalcValue[ this.regionsSortedHorizontally.Count ][ , ] );
					ArrayProxy[] arrayProxyGroup = new ArrayProxy[this.regionsSortedHorizontally.Count];
					for (int i = 0; i < this.regionsSortedHorizontally.Count; i++)
						arrayProxyGroup[i] = new RegionCalcReference.RegionArrayProxy(this.regionsSortedHorizontally[i]);

					value = new ExcelCalcValue(arrayProxyGroup);

					this.ValueInternal = value;
				}

				return value;
			}
		} 

		#endregion ValueInternal

		#endregion Base Class Overrides

		#region Methods

		#region Static Methods

		#region FromReference

		// MD 2/27/12 - TFS102520
		//public static RegionGroupCalcReference FromReference( IExcelCalcReference reference, IExcelCalcFormula formulaBeingSolved )
		public static RegionGroupCalcReference FromReference(IExcelCalcReference reference)
		{
			reference = ExcelCalcEngine.GetResolvedReference( reference );

			RegionGroupCalcReference groupReference = reference as RegionGroupCalcReference;

			if ( groupReference != null )
				return groupReference;

			CellCalcReference cellReference = reference as CellCalcReference;

			if ( cellReference != null )
			{
				// MD 4/12/11 - TFS67084
				// Use short instead of int so we don't have to cast.
				//return new RegionGroupCalcReference( cellReference.Cell );
				return new RegionGroupCalcReference(cellReference.Row, cellReference.ColumnIndex);
			}

			// MD 2/24/12 - 12.1 - Table Support
			//RegionCalcReference regionReference = reference as RegionCalcReference;
			//
			//if ( regionReference != null )
			//    return new RegionGroupCalcReference( regionReference.Region );
			RegionCalcReferenceBase regionReference = reference as RegionCalcReferenceBase;

			if (regionReference != null)
			{
				WorksheetRegion region = regionReference.Region;
				if (region != null)
					return new RegionGroupCalcReference(region);
			}

			return null;
		}

		#endregion FromReference

		#region Intersect

		public static IExcelCalcReference Intersect( RegionGroupCalcReference ref1, RegionGroupCalcReference ref2 )
		{
			Worksheet worksheet = ref1.Worksheet;

			if ( worksheet == null || worksheet != ref2.Worksheet )
			{
				Utilities.DebugFail( "Regions combined from different worksheets!" );
				return ref1;
			}

			// MD 12/1/11
			// Found while fixing TFS96113
			// The regionsSortedHorizontally collection is now immutable so we need to construct it first.
			//RegionGroupCalcReference result = new RegionGroupCalcReference();
			List<WorksheetRegion> newRegions = new List<WorksheetRegion>();
			
			foreach ( WorksheetRegion ref1Region in ref1.regionsSortedHorizontally )
			{
				// MD 2/29/12 - 12.1 - Table Support
				// The worksheet can now be null.
				if (ref1Region.Worksheet == null)
					return ExcelReferenceError.Instance;

				foreach ( WorksheetRegion ref2Region in ref2.regionsSortedHorizontally )
				{
					// MD 2/29/12 - 12.1 - Table Support
					// The worksheet can now be null.
					if (ref2Region.Worksheet == null)
						return ExcelReferenceError.Instance;

					// If the first and last row of the first region are both above the first row of the second region, they don't intersect.
					if ( ref1Region.FirstRow < ref2Region.FirstRow &&
						ref1Region.LastRow < ref2Region.FirstRow )
					{
						continue;
					}

					// If the first and last row of the first region are both below the last row of the second region, they don't intersect.
					if ( ref2Region.LastRow < ref1Region.FirstRow &&
						ref2Region.LastRow < ref1Region.LastRow )
					{
						continue;
					}

					// If the first and last column of the first region are both to the left of the first column of the second region, they don't intersect.
					if ( ref1Region.FirstColumn < ref2Region.FirstColumn &&
						ref1Region.LastColumn < ref2Region.FirstColumn )
					{
						continue;
					}

					// If the first and last column of the first region are both to the right of the last column of the second region, they don't intersect.
					if ( ref2Region.LastColumn < ref1Region.FirstColumn &&
						ref2Region.LastColumn < ref1Region.LastColumn )
					{
						continue;
					}

					// MD 12/1/11
					// Found while fixing TFS96113
					// The regionsSortedHorizontally collection is now immutable so we need to construct it first.
                    //result.regionsSortedHorizontally.Add(ref1Region.Worksheet.GetCachedRegion( 
					newRegions.Add(ref1Region.Worksheet.GetCachedRegion( 
						Math.Max( ref1Region.FirstRow, ref2Region.FirstRow ),
						Math.Max( ref1Region.FirstColumn, ref2Region.FirstColumn ),
						Math.Min( ref1Region.LastRow, ref2Region.LastRow ),
						Math.Min( ref1Region.LastColumn, ref2Region.LastColumn ) ) );
				}
			}

			// MD 12/1/11
			// Found while fixing TFS96113
			// The regionsSortedHorizontally collection is now immutable so we need to construct it first.
			// Sort the regions added to the collection
			//result.regionsSortedHorizontally.Sort( WorksheetRegion.HorizontalSorter.Instance );
			//
			//return result.GetResolvedReference();
			newRegions.Sort(WorksheetRegion.HorizontalSorter.Instance);
			return new RegionGroupCalcReference(newRegions).GetResolvedReference();
		}

		#endregion Intersect

		#region Range

		public static IExcelCalcReference Range( RegionGroupCalcReference ref1, RegionGroupCalcReference ref2 )
		{
			Worksheet worksheet = ref1.Worksheet;

			if ( worksheet == null || worksheet != ref2.Worksheet )
			{
				Utilities.DebugFail( "Regions combined from different worksheets!" );
				return ref1;
			}

			// MD 2/29/12 - 12.1 - Table Support
			// The worksheet can now be null.
			for (int i = 0; i < ref1.regionsSortedHorizontally.Count; i++)
			{
				if (ref1.regionsSortedHorizontally[i].Worksheet == null)
					return ExcelReferenceError.Instance;
			}

			for (int i = 0; i < ref2.regionsSortedHorizontally.Count; i++)
			{
				if (ref2.regionsSortedHorizontally[i].Worksheet == null)
					return ExcelReferenceError.Instance;
			}

			List<WorksheetRegion> ref1RegionsSortedVertically = new List<WorksheetRegion>( ref1.regionsSortedHorizontally );
			ref1RegionsSortedVertically.Sort( WorksheetRegion.VerticalSorter.Instance );

			List<WorksheetRegion> ref2RegionsSortedVertically = new List<WorksheetRegion>( ref2.regionsSortedHorizontally );
			ref2RegionsSortedVertically.Sort( WorksheetRegion.VerticalSorter.Instance );

			// MD 12/1/11
			// Found while fixing TFS96113
			// The regionsSortedHorizontally collection is now immutable so we need to construct it first.
			//RegionGroupCalcReference result = new RegionGroupCalcReference();
			List<WorksheetRegion> newRegions = new List<WorksheetRegion>();

			int firstRow1 = ref1RegionsSortedVertically[ 0 ].FirstRow;
			int firstRow2 = ref2RegionsSortedVertically[ 0 ].FirstRow;

			int firstColumn1 = ref1.regionsSortedHorizontally[ 0 ].FirstColumn;
			int firstColumn2 = ref2.regionsSortedHorizontally[ 0 ].FirstColumn;

			int lastRow1 = ref1RegionsSortedVertically[ ref1RegionsSortedVertically.Count - 1 ].LastRow;
			int lastRow2 = ref2RegionsSortedVertically[ ref2RegionsSortedVertically.Count - 1 ].LastRow;

			int lastColumn1 = ref1.regionsSortedHorizontally[ ref1.regionsSortedHorizontally.Count - 1 ].LastColumn;
			int lastColumn2 = ref2.regionsSortedHorizontally[ ref2.regionsSortedHorizontally.Count - 1 ].LastColumn;

            WorksheetRegion resultRegion = ref1.regionsSortedHorizontally[0].Worksheet.GetCachedRegion( 
				Math.Min( firstRow1, firstRow2 ),
				Math.Min( firstColumn1, firstColumn2 ),
				Math.Max( lastRow1, lastRow2 ),
				Math.Max( lastColumn1, lastColumn2 ) );

			// MD 12/1/11
			// Found while fixing TFS96113
			// The regionsSortedHorizontally collection is now immutable so we need to construct it first.
			//result.regionsSortedHorizontally.Add( resultRegion );
			//
			//return result.GetResolvedReference();
			newRegions.Add(resultRegion);
			return new RegionGroupCalcReference(newRegions).GetResolvedReference();
		}

		#endregion Range

		#region Union

		public static IExcelCalcReference Union( RegionGroupCalcReference ref1, RegionGroupCalcReference ref2 )
		{
			Worksheet worksheet = ref1.Worksheet;

			if ( worksheet == null || worksheet != ref2.Worksheet )
			{
				Utilities.DebugFail( "Regions combined from different worksheets!" );
				return ref1;
			}

			// MD 12/1/11
			// Found while fixing TFS96113
			// The regionsSortedHorizontally collection is now immutable so we need to construct it first.
			//RegionGroupCalcReference result = new RegionGroupCalcReference();
			//
			//result.regionsSortedHorizontally.AddRange( ref1.regionsSortedHorizontally );
			//result.regionsSortedHorizontally.AddRange( ref2.regionsSortedHorizontally );
			//result.regionsSortedHorizontally.Sort( WorksheetRegion.HorizontalSorter.Instance );
			//
			//return result.GetResolvedReference();
			List<WorksheetRegion> newRegions = new List<WorksheetRegion>();

			newRegions.AddRange(ref1.regionsSortedHorizontally);
			newRegions.AddRange(ref2.regionsSortedHorizontally);
			newRegions.Sort(WorksheetRegion.HorizontalSorter.Instance);

			return new RegionGroupCalcReference(newRegions).GetResolvedReference();
		}

		#endregion Union 

		#endregion Static Methods

		#region GetResolvedReference

		private IExcelCalcReference GetResolvedReference()
		{
			if ( this.regionsSortedHorizontally.Count == 0 )
				return null;

			if ( this.regionsSortedHorizontally.Count != 1 )
				return this;

			return this.regionsSortedHorizontally[ 0 ].CalcReference;
		} 

		#endregion GetResolvedReference

		#endregion Methods

		#region Properties

		#region Regions

		// MD 12/1/11
		// Found while fixing TFS96113
		// The regionsSortedHorizontally collection is now immutable.
		//public List<WorksheetRegion> Regions
		public ReadOnlyCollection<WorksheetRegion> Regions
		{
			get { return this.regionsSortedHorizontally; }
		} 

		#endregion Regions

		#endregion Properties		


		#region GeneralReferencesCollection class

		private class GeneralReferencesCollection : 
			IEnumerable<IExcelCalcReference>, 
			IExcelCalcReferenceCollection
		{
			#region Member Variables

			private RegionGroupCalcReference reference;

			#endregion Member Variables

			#region Constructor

			public GeneralReferencesCollection( RegionGroupCalcReference reference )
			{
				this.reference = reference;
			}

			#endregion Constructor

			#region IEnumerable<IExcelCalcReference> Members

			public IEnumerator<IExcelCalcReference> GetEnumerator()
			{
				// MD 2/29/12 - 12.1 - Table Support
				// The worksheet can now be null.
				for (int i = 0; i < this.reference.regionsSortedHorizontally.Count; i++)
				{
					if (this.reference.regionsSortedHorizontally[i].Worksheet == null)
					{
						yield return ExcelReferenceError.Instance;
						yield break;
					}
				}

				ExcelCalcNumberStack numberStack = this.reference.NumberStack;
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


				for ( int i = 0; i < this.reference.regionsSortedHorizontally.Count; i++ )
				{
					WorksheetRegion region = this.reference.regionsSortedHorizontally[ i ];

					// MD 4/12/11 - TFS67084
					// Moved away from using WorksheetCell objects.
					//foreach ( WorksheetCell cell in region.GetCreatedCells() )
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
					//    cellReference.ExpectedParameterClass = this.reference.ExpectedParameterClass;
					//    cellReference.NumberStack = this.reference.NumberStack;
					//
					//    yield return cellReference;
					//}
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

							cellReference.ExpectedParameterClass = this.reference.ExpectedParameterClass;
							cellReference.NumberStack = this.reference.NumberStack;

							yield return cellReference;
						}
					}
				}
			}

			#endregion

			#region IEnumerable Members

			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
			{
				return ( (IEnumerable<IExcelCalcReference>)this ).GetEnumerator();
			}

			#endregion
		}

		#endregion GeneralReferencesCollection class
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