using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.ComponentModel;
using System.Globalization;




using Infragistics.Shared;


namespace Infragistics.Documents.Excel
{
	// MD 5/13/11 - Data Validations / Page Breaks
	/// <summary>
	/// A collection of cells or regions which are all on the same <see cref="Worksheet"/>.
	/// </summary>



	[InfragisticsFeature(Version = FeatureInfo.Version_11_2, FeatureName = FeatureInfo.FeatureName_ExcelAdditions)] 
	public

		 class WorksheetReferenceCollection : ICollection<WorksheetCell>, ICollection<WorksheetRegion>
	{
		#region Member Variables

		private int? cachedCellsCount;
		private string cachedA1StringValue;
		private string cachedR1C1StringValue;

		// MD 1/23/12 - TFS99849
		// Renamed for clarity.
		//private List<WorksheetRegion> regions;
		// MD 3/13/12 - 12.1 - Table Support
		//private List<WorksheetRegion> regionsSortedHorizontally;
		private List<WorksheetRegionAddress> regionsSortedHorizontally;

		// MD 1/23/12 - TFS99849
		// Added a region lookup table for fast searching through the contained regions.
		// MD 3/13/12 - 12.1 - Table Support
		//private RegionLookupTable<WorksheetRegion> regionLookupTable;
		private RegionLookupTable<WorksheetRegionAddress> regionLookupTable;

		private Worksheet worksheet;

		#endregion  // Member Variables

		#region Constructors

		/// <summary>
		/// Creates a new <see cref="WorksheetReferenceCollection"/> instance.
		/// </summary>
		/// <param name="worksheet">The worksheet to which the references in the collection will belong.</param>
		/// <exception cref="ArgumentNullException">
		/// Occurs when <paramref name="worksheet"/> is null.
		/// </exception>
		public WorksheetReferenceCollection(Worksheet worksheet)
		{
			if (worksheet == null)
				throw new ArgumentNullException("worksheet");

			this.Initialize(worksheet);
		}

		/// <summary>
		/// Creates a new <see cref="WorksheetReferenceCollection"/> instance.
		/// </summary>
		/// <param name="worksheet">The worksheet to which the references in the collection will belong.</param>
		/// <param name="references">The space delimited list of references to add to the collection.</param>
		/// <exception cref="ArgumentNullException">
		/// Occurs when <paramref name="worksheet"/> is null.
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// Occurs when the <paramref name="references"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the list of reference is not well formed.
		/// </exception>
		public WorksheetReferenceCollection(Worksheet worksheet, string references)
			:this(worksheet)
		{
			this.Add(references);
		}

		/// <summary>
		/// Creates a new <see cref="WorksheetReferenceCollection"/> instance and initializes it with a cell.
		/// </summary>
		/// <param name="cell">The cell with which the collection should be initialized.</param>
		/// <exception cref="ArgumentNullException">
		/// Occurs when <paramref name="cell"/> is null.
		/// </exception>
		public WorksheetReferenceCollection(WorksheetCell cell)
		{
			if (cell == null)
				throw new ArgumentNullException("cell");

			// MD 2/29/12 - 12.1 - Table Support
			// The worksheet can now be null.
			if (cell.Worksheet == null)
				throw new ArgumentException(SR.GetString("LE_ArgumentException_CellShiftedOffWorksheet"), "cell");

			this.Initialize(cell.Worksheet);

			// MD 1/23/12 - TFS99849
			// Use the AddDirect method because the region needs to be added to both collections.
			//this.regionsSortedHorizontally.Add(cell.GetCachedRegion());
			// MD 3/13/12 - 12.1 - Table Support
			//this.AddDirect(cell.GetCachedRegion());
			this.AddDirect(cell.RegionAddress);
		}

		/// <summary>
		/// Creates a new <see cref="WorksheetReferenceCollection"/> instance and initializes it with a cell.
		/// </summary>
		/// <param name="region">The cell with which the collection should be initialized.</param>
		/// <exception cref="ArgumentNullException">
		/// Occurs when <paramref name="region"/> is null.
		/// </exception>
		public WorksheetReferenceCollection(WorksheetRegion region)
		{
			if (region == null)
				throw new ArgumentNullException("region");

			// MD 2/29/12 - 12.1 - Table Support
			// The worksheet can now be null.
			if (region.Worksheet == null)
				throw new ArgumentException(SR.GetString("LE_ArgumentException_RegionShiftedOffWorksheet"), "region");

			this.Initialize(region.Worksheet);

			// MD 1/23/12 - TFS99849
			// Use the AddDirect method because the region needs to be added to both collections.
			//this.regionsSortedHorizontally.Add(region);
			// MD 3/13/12 - 12.1 - Table Support
			//this.AddDirect(region);
			this.AddDirect(region.Address);
		}

		#endregion  // Constructors

		#region Base Class Overrides

		#region ToString

		/// <summary>
		/// Returns the string value represent the cell and region addresses in the collection.
		/// </summary>
		public override string ToString()
		{
			return this.ToString(this.Worksheet.CellReferenceMode);
		}

		#endregion  // ToString

		#endregion  // Base Class Overrides

		#region Interfaces

		#region ICollection<WorksheetCell> Members

		void ICollection<WorksheetCell>.CopyTo(WorksheetCell[] array, int arrayIndex)
		{
			int index = arrayIndex;
			foreach (WorksheetCell cell in (ICollection<WorksheetCell>)this)
				array[index++] = cell;
		}

		int ICollection<WorksheetCell>.Count
		{
			get { return this.CellsCount; }
		}

		bool ICollection<WorksheetCell>.IsReadOnly
		{
			get { return false; }
		}

		#endregion

		#region IEnumerable<WorksheetCell> Members

		IEnumerator<WorksheetCell> IEnumerable<WorksheetCell>.GetEnumerator()
		{
			// MD 3/13/12 - 12.1 - Table Support
			//foreach (WorksheetRegion region in this.regionsSortedHorizontally)
			//{
			//    foreach (WorksheetCell cell in region)
			//        yield return cell;
			//}
			foreach (WorksheetRegionAddress regionAddress in this.regionsSortedHorizontally)
			{
				for (int rowIndex = regionAddress.FirstRowIndex; rowIndex <= regionAddress.LastRowIndex; rowIndex++)
				{
					WorksheetRow row = this.worksheet.Rows[rowIndex];
					for (int columnIndex = regionAddress.FirstColumnIndex; columnIndex <= regionAddress.LastColumnIndex; columnIndex++)
						yield return row.Cells[columnIndex];
				}
			}
		}

		#endregion

		#region IEnumerable Members

		/// <summary>
		/// Gets an enumerator which returns all <see cref="WorksheetRegion"/> and <see cref="WorksheetCell"/> instances in the collection of references.
		/// </summary>
		public System.Collections.IEnumerator GetEnumerator()
		{
			//foreach (WorksheetRegion region in this.regionsSortedHorizontally)
			//{
			//    if (region.IsSingleCell)
			//        yield return region.TopLeftCell;
			//    else
			//        yield return region;
			//}
			foreach (WorksheetRegionAddress regionAddress in this.regionsSortedHorizontally)
			{
				if (regionAddress.IsSingleCell)
					yield return this.worksheet.Rows[regionAddress.FirstRowIndex].Cells[regionAddress.FirstColumnIndex];
				else
					yield return this.worksheet.GetCachedRegion(regionAddress);
			}
		}

		#endregion

		#region ICollection<WorksheetRegion> Members

		void ICollection<WorksheetRegion>.CopyTo(WorksheetRegion[] array, int arrayIndex)
		{
			int index = arrayIndex;
			foreach (WorksheetRegion region in (ICollection<WorksheetRegion>)this)
				array[index++] = region;
		}

		int ICollection<WorksheetRegion>.Count
		{
			get { return this.regionsSortedHorizontally.Count; }
		}

		bool ICollection<WorksheetRegion>.IsReadOnly
		{
			get { return false; }
		}

		#endregion

		#region IEnumerable<WorksheetRegion> Members

		IEnumerator<WorksheetRegion> IEnumerable<WorksheetRegion>.GetEnumerator()
		{
			// MD 3/13/12 - 12.1 - Table Support
			//return this.regionsSortedHorizontally.GetEnumerator();
			foreach (WorksheetRegionAddress regionAddress in this.regionsSortedHorizontally)
				yield return this.worksheet.GetCachedRegion(regionAddress);
		}

		#endregion

		#endregion  // Interfaces

		#region Methods

		#region Public Methods

		#region Add( string )

		/// <summary>
		/// Adds a list of references to the collection of references.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The references must be separated by one or more space (' ') characters.
		/// </p>
		/// <p class="body">
		/// The references in the list cannot contain the worksheet name. They are all assumed to be from the worksheet of this collection.
		/// </p>
		/// <p class="body">
		/// If all references are already contained in the collection this call will have no effect on the collection.
		/// </p>
		/// </remarks>
		/// <param name="references">The space delimited list of references to add to the collection.</param>
		/// <exception cref="ArgumentNullException">
		/// Occurs when the <paramref name="references"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the list of reference is not well formed.
		/// </exception>
		public void Add(string references)
		{
			this.Add(references, this.worksheet.CellReferenceMode);
		}

		#endregion  // Add( string )

		#region Add( string, CellReferenceMode )

		/// <summary>
		/// Adds a list of references to the collection of references.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The references must be separated by one or more space (' ') characters.
		/// </p>
		/// <p class="body">
		/// The references in the list cannot contain the worksheet name. They are all assumed to be from the worksheet of this collection.
		/// </p>
		/// <p class="body">
		/// If all references are already contained in the collection this call will have no effect on the collection.
		/// </p>
		/// </remarks>
		/// <param name="references">The space delimited list of references to add to the collection.</param>
		/// <param name="cellReferenceMode">The cell reference mode with which to parse the <paramref name="references"/>.</param>
		/// <exception cref="ArgumentNullException">
		/// Occurs when the <paramref name="references"/> is null.
		/// </exception>
		/// <exception cref="InvalidEnumArgumentException">
		/// Occurs when <paramref name="cellReferenceMode"/> is not defined in the <see cref="CellReferenceMode"/> enumeration.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the list of reference is not well formed.
		/// </exception>
		public void Add(string references, CellReferenceMode cellReferenceMode)
		{
			if (references == null)
				throw new ArgumentNullException("references");

			if (Enum.IsDefined(typeof(CellReferenceMode), cellReferenceMode) == false)
				throw new InvalidEnumArgumentException("cellReferenceMode", (int)cellReferenceMode, typeof(CellReferenceMode));

			// MD 4/9/12 - TFS101506
			//List<WorksheetRegion> regionsToAdd = this.GetRegions(references, cellReferenceMode);
			List<WorksheetRegion> regionsToAdd = this.GetRegions(references, cellReferenceMode, this.worksheet.Culture);

			if (regionsToAdd.Count == 0)
				return;

			for (int i = 0; i < regionsToAdd.Count; i++)
			{
				// MD 3/13/12 - 12.1 - Table Support
				//this.AddRegionHelper(regionsToAdd[i]);
				this.AddRegionHelper(regionsToAdd[i].Address);
			}

			this.Condense();
		} 

		#endregion  // Add( string, CellReferenceMode )

		#region Add( WorksheetCell )

		/// <summary>
		/// Adds a cell to the collection of references.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The cell added to the collection must be from the same <see cref="Worksheet"/> as the collection.
		/// </p>
		/// <p class="body">
		/// If the cell is already contained in the collection, or there is a region in the collection which contains the cell, this call
		/// will have no effect on the collection.
		/// </p>
		/// </remarks>
		/// <param name="cell">The cell to add to the collection.</param>
		/// <exception cref="ArgumentNullException">
		/// Occurs when the <paramref name="cell"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the <paramref name="cell"/> is from a Worksheet other than the references collection.
		/// </exception>
		public void Add(WorksheetCell cell)
		{
			if (cell == null)
				throw new ArgumentNullException("cell");

			// MD 2/29/12 - 12.1 - Table Support
			// The worksheet can now be null.
			if (cell.Worksheet == null)
				throw new ArgumentException(SR.GetString("LE_ArgumentException_CellShiftedOffWorksheet"), "cell");

			if (cell.Worksheet != this.worksheet)
				throw new ArgumentException(SR.GetString("LE_ArgumentException_CellMustBeOnSameWorksheetAsReferencesCollection"), "cell");

			this.Add(cell.GetCachedRegion());
		}

		#endregion  // Add( WorksheetCell )

		#region Add( WorksheetRegion )

		/// <summary>
		/// Adds a region to the collection of references.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The region added to the collection must be from the same <see cref="Worksheet"/> as the collection.
		/// </p>
		/// <p class="body">
		/// If the region is already contained in the collection, or there is a region in the collection which fully contains the specified region, 
		/// this call will have no effect on the collection.
		/// </p>
		/// </remarks>
		/// <param name="region">The region to add to the collection.</param>
		/// <exception cref="ArgumentNullException">
		/// Occurs when the <paramref name="region"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the <paramref name="region"/> is from a Worksheet other than the references collection.
		/// </exception>
		public void Add(WorksheetRegion region)
		{
			if (region == null)
				throw new ArgumentNullException("region");

			// MD 2/29/12 - 12.1 - Table Support
			// The worksheet can now be null.
			if (region.Worksheet == null)
				throw new ArgumentException(SR.GetString("LE_ArgumentException_RegionShiftedOffWorksheet"), "region");

			if (region.Worksheet != this.worksheet)
				throw new ArgumentException(SR.GetString("LE_ArgumentException_RegionMustBeOnSameWorksheetAsReferencesCollection"), "region");

			// MD 3/13/12 - 12.1 - Table Support
			//if (this.AddRegionHelper(region))
			if (this.AddRegionHelper(region.Address))
				this.Condense();
		}

		#endregion  // Add( WorksheetRegion )

		#region Clear

		/// <summary>
		/// Clears all references from the collection.
		/// </summary>
		public void Clear()
		{
			// MD 1/23/12 - TFS99849
			this.regionLookupTable.Clear();

			this.regionsSortedHorizontally.Clear();
			this.ResetCache();
		}

		#endregion  // Clear

		#region Contains( WorksheetCell )

		/// <summary>
		/// Determines whether the collection contains the specified cell.
		/// </summary>
		/// <param name="cell">The cell to search for in the collection.</param>
		/// <exception cref="ArgumentNullException">
		/// Occurs when the <paramref name="cell"/> is null.
		/// </exception>
		/// <returns>
		/// True if the cell is contained in the collection or a region which contains the cell is contained in the collection; False otherwise.
		/// </returns>
		public bool Contains(WorksheetCell cell)
		{
			if (cell == null)
				throw new ArgumentNullException("cell");

			// MD 2/29/12 - 12.1 - Table Support
			// The worksheet can now be null.
			if (cell.Worksheet == null)
				throw new ArgumentException(SR.GetString("LE_ArgumentException_CellShiftedOffWorksheet"), "cell");

			// MD 3/13/12 - 12.1 - Table Support
			//return this.Contains(cell.GetCachedRegion());
			if (cell.Worksheet != this.Worksheet)
				return false;

			return this.Contains(cell.RegionAddress);
		}

		#endregion  // Contains( WorksheetCell )

		#region Contains( WorksheetRegion )

		/// <summary>
		/// Determines whether the collection contains the specified region.
		/// </summary>
		/// <param name="region">The region to search for in the collection.</param>
		/// <exception cref="ArgumentNullException">
		/// Occurs when the <paramref name="region"/> is null.
		/// </exception>
		/// <returns>
		/// True if the region is contained in the collection or a region which fully contains the specified region is contained in the collection; 
		/// False otherwise.
		/// </returns>
		public bool Contains(WorksheetRegion region)
		{
			if (region == null)
				throw new ArgumentNullException("region");

			// MD 2/29/12 - 12.1 - Table Support
			// The worksheet can now be null.
			if (region.Worksheet == null)
				throw new ArgumentException(SR.GetString("LE_ArgumentException_RegionShiftedOffWorksheet"), "region");

			if (region.Worksheet != this.Worksheet)
				return false;

			// MD 3/13/12 - 12.1 - Table Support
			// Moved all code to the new overload.
			return this.Contains(region.Address);
		}

		// MD 3/13/12 - 12.1 - Table Support
		private bool Contains(WorksheetRegionAddress region)
		{
			// MD 1/23/12
			// Found while fixing TFS99849
			// This can be made more efficient with the region lookup table.
			#region Old Code

			//for (int i = 0; i < this.regionsSortedHorizontally.Count; i++)
			//{
			//    WorksheetRegion existingRegion = this.regionsSortedHorizontally[i];
			//    if (existingRegion.Contains(region))
			//        return true;

			//    if (existingRegion.IntersectsWith(region) == false)
			//        continue;

			//    List<WorksheetRegion> regionsNotInSubtractedArea;
			//    WorksheetReferenceCollection.SubtractRegion(region, existingRegion, out regionsNotInSubtractedArea);

			//    if (regionsNotInSubtractedArea.Count == 0)
			//    {
			//        Utilities.DebugFail("The WorksheetRegion.Contains call should have returned True above.");
			//        return true;
			//    }

			//    for (int j = 0; j < regionsNotInSubtractedArea.Count; j++)
			//    {
			//        if (this.Contains(regionsNotInSubtractedArea[j]) == false)
			//            return false;
			//    }

			//    return true;
			//}

			#endregion // Old Code
			// MD 3/13/12 - 12.1 - Table Support
			//List<WorksheetRegion> regionsContaining = this.regionLookupTable.GetItemsContainingRange(region);
			List<WorksheetRegionAddress> regionsContaining = this.regionLookupTable.GetItemsContainingRange(region);

			if (regionsContaining != null && regionsContaining.Count != 0)
				return true;

			// MD 3/13/12 - 12.1 - Table Support
			//List<WorksheetRegion> regionsIntersectingWith = this.regionLookupTable.GetItemsIntersectingWithRange(region);
			List<WorksheetRegionAddress> regionsIntersectingWith = this.regionLookupTable.GetItemsIntersectingWithRange(region);

			if (regionsIntersectingWith != null && regionsIntersectingWith.Count != 0)
			{
				// MD 3/13/12 - 12.1 - Table Support
				//List<WorksheetRegion> regionsNotInSubtractedArea;
				List<WorksheetRegionAddress> regionsNotInSubtractedArea;

				WorksheetReferenceCollection.SubtractRegion(region, regionsIntersectingWith[0], out regionsNotInSubtractedArea);

				for (int i = 0; i < regionsNotInSubtractedArea.Count; i++)
				{
					if (this.Contains(regionsNotInSubtractedArea[i]) == false)
						return false;
				}

				return true;
			}

			return false;
		}

		#endregion  // Contains( WorksheetRegion )

		#region IntersectsWith

		// MD 3/13/12 - 12.1 - Table Support
		//internal bool IntersectsWith(WorksheetRegion region)
		internal bool IntersectsWith(WorksheetRegionAddress region)
		{
			// MD 1/23/12
			// Found while fixing TFS99849
			// This can be made more efficient with the region lookup table.
			//for (int i = 0; i < this.regionsSortedHorizontally.Count; i++)
			//{
			//    if (region.IntersectsWith(this.regionsSortedHorizontally[i]))
			//        return true;
			//}
			//
			//return false;
			// MD 3/13/12 - 12.1 - Table Support
			//if (region.Worksheet != this.worksheet)
			//    return false;

			// MD 3/13/12 - 12.1 - Table Support
			//List<WorksheetRegion> intersectingRegions = this.regionLookupTable.GetItemsIntersectingWithRange(region);
			List<WorksheetRegionAddress> intersectingRegions = this.regionLookupTable.GetItemsIntersectingWithRange(region);

			return intersectingRegions != null && intersectingRegions.Count != 0;
		}

		#endregion  // IntersectsWith

		#region Remove( string )

		/// <summary>
		/// Removes a list of references from the collection of references.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The references must be separated by one or more space (' ') characters.
		/// </p>
		/// <p class="body">
		/// The references in the list cannot contain the worksheet name. They are all assumed to be from the worksheet of this collection.
		/// </p>
		/// <p class="body">
		/// If the references are not contained in the collection, this call will have no effect on the collection.
		/// </p>
		/// </remarks>
		/// <param name="references">The space delimited list of references to remove from the collection.</param>
		/// <exception cref="ArgumentNullException">
		/// Occurs when the <paramref name="references"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the list of reference is not well formed.
		/// </exception>
		/// <returns>True if any cells in the references were found and removed. False otherwise.</returns>
		public bool Remove(string references)
		{
			return Remove(references, this.worksheet.CellReferenceMode);
		}

		#endregion  // Remove( string )

		#region Remove( string, CellReferenceMode )

		/// <summary>
		/// Removes a list of references from the collection of references.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The references must be separated by one or more space (' ') characters.
		/// </p>
		/// <p class="body">
		/// The references in the list cannot contain the worksheet name. They are all assumed to be from the worksheet of this collection.
		/// </p>
		/// <p class="body">
		/// If the references are not contained in the collection, this call will have no effect on the collection.
		/// </p>
		/// </remarks>
		/// <param name="references">The space delimited list of references to remove from the collection.</param>
		/// <param name="cellReferenceMode">The cell reference mode with which to parse the <paramref name="references"/>.</param>
		/// <exception cref="ArgumentNullException">
		/// Occurs when the <paramref name="references"/> is null.
		/// </exception>
		/// <exception cref="InvalidEnumArgumentException">
		/// Occurs when <paramref name="cellReferenceMode"/> is not defined in the <see cref="CellReferenceMode"/> enumeration.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the list of reference is not well formed.
		/// </exception>
		/// <returns>True if any cells in the references were found and removed. False otherwise.</returns>
		public bool Remove(string references, CellReferenceMode cellReferenceMode)
		{
			if (references == null)
				throw new ArgumentNullException("references");

			if (Enum.IsDefined(typeof(CellReferenceMode), cellReferenceMode) == false)
				throw new InvalidEnumArgumentException("cellReferenceMode", (int)cellReferenceMode, typeof(CellReferenceMode));

			// MD 4/9/12 - TFS101506
			//List<WorksheetRegion> regionsToRemove = this.GetRegions(references, cellReferenceMode);
			List<WorksheetRegion> regionsToRemove = this.GetRegions(references, cellReferenceMode, this.worksheet.Culture);

			if (regionsToRemove.Count == 0)
				return false;

			bool cellsRemoved = false;
			for (int i = 0; i < regionsToRemove.Count; i++)
			{
				// MD 1/23/12 - TFS99849
				// RemoveRegionHelper is now an instance method and renamed to SubtractRegionHelper.
				//cellsRemoved |= WorksheetReferenceCollection.RemoveRegionHelper(this.regionsSortedHorizontally, regionsToRemove[i]);
				// MD 3/13/12 - 12.1 - Table Support
				//cellsRemoved |= this.SubtractRegionHelper(regionsToRemove[i]);
				cellsRemoved |= this.SubtractRegionHelper(regionsToRemove[i].Address);
			}

			if (cellsRemoved == false)
				return false;

			this.Condense();
			return true;
		}

		#endregion  // Remove( string, CellReferenceMode )

		#region Remove( WorksheetCell )

		/// <summary>
		/// Removes a cell from the collection of references.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// If the cell is not contained in the collection, this call will have no effect on the collection.
		/// </p>
		/// </remarks>
		/// <param name="cell">The cell to remove from the collection.</param>
		/// <exception cref="ArgumentNullException">
		/// Occurs when the <paramref name="cell"/> is null.
		/// </exception>
		/// <returns>True if the cell was found and removed. False otherwise.</returns>
		public bool Remove(WorksheetCell cell)
		{
			if (cell == null)
				throw new ArgumentNullException("cell");

			// MD 2/29/12 - 12.1 - Table Support
			// The worksheet can now be null.
			if (cell.Worksheet == null)
				throw new ArgumentException(SR.GetString("LE_ArgumentException_CellShiftedOffWorksheet"), "cell");

			if (cell.Worksheet != this.worksheet)
				return false;

			// MD 1/23/12
			// Found while fixing TFS99849
			// This can be made more efficient with the region lookup table.
			//for (int i = this.regionsSortedHorizontally.Count - 1; i >= 0; i--)
			//{
			//    WorksheetRegion region = this.regionsSortedHorizontally[i];
			//    if (region.Contains(cell) == false)
			//        continue;
			//
			//    // MD 1/23/12 - TFS99849
			//    // Use the RemoveAtDirect method because the region needs to be removed from both collections.
			//    //this.regionsSortedHorizontally.RemoveAt(i);
			//    this.RemoveAtDirect(region, i);

			// MD 3/13/12 - 12.1 - Table Support
			//List<WorksheetRegion> regionsContainingCell = this.regionLookupTable.GetItemsContainingCell(cell.RowIndex, cell.ColumnIndexInternal);
			List<WorksheetRegionAddress> regionsContainingCell = this.regionLookupTable.GetItemsContainingCell(cell.RowIndex, cell.ColumnIndexInternal);

			if(regionsContainingCell != null && regionsContainingCell.Count != 0)
			{
				Debug.Assert(regionsContainingCell.Count == 1, "There should only be one region containing the cell.");

				// MD 3/13/12 - 12.1 - Table Support
				//WorksheetRegion region = regionsContainingCell[0];
				WorksheetRegionAddress region = regionsContainingCell[0];

				this.RemoveDirect(region);

				int cellRowIndex = cell.Row.Index;

				// MD 3/13/12 - 12.1 - Table Support
				//WorksheetRegion cellRegion = new WorksheetRegion(cell.Worksheet, cellRowIndex, cell.ColumnIndex, cellRowIndex, cell.ColumnIndex);
				short cellColumnIndex = cell.ColumnIndexInternal;
				WorksheetRegionAddress cellRegion = new WorksheetRegionAddress(cellRowIndex, cellRowIndex, cellColumnIndex, cellColumnIndex);

				// MD 3/13/12 - 12.1 - Table Support
				//List<WorksheetRegion> regionsNotInSubtractedArea;
				List<WorksheetRegionAddress> regionsNotInSubtractedArea;

				WorksheetReferenceCollection.SubtractRegion(region, cellRegion, out regionsNotInSubtractedArea);

				if (regionsNotInSubtractedArea.Count > 0)
				{
					// MD 1/23/12 - TFS99849
					// Use the AddRangeDirect method because the regions needs to be removed from both collections.
					//this.regionsSortedHorizontally.AddRange(regionsNotInSubtractedArea);
					this.AddRangeDirect(regionsNotInSubtractedArea);

					this.regionsSortedHorizontally.Sort(WorksheetRegion.HorizontalSorter.Instance);
				}

				this.Condense();
				return true;
			}

			return false;
		}

		#endregion  // Remove( WorksheetCell )

		#region Remove( WorksheetRegion )

		/// <summary>
		/// Removes a region from the collection of references.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// If the region is not contained in the collection, this call will have no effect on the collection.
		/// </p>
		/// </remarks>
		/// <param name="region">The region to remove from the collection.</param>
		/// <exception cref="ArgumentNullException">
		/// Occurs when the <paramref name="region"/> is null.
		/// </exception>
		/// <returns>True if any cells in the region were found and removed. False otherwise.</returns>
		public bool Remove(WorksheetRegion region)
		{
			if (region == null)
				throw new ArgumentNullException("region");

			// MD 2/29/12 - 12.1 - Table Support
			// The worksheet can now be null.
			if (region.Worksheet == null)
				throw new ArgumentException(SR.GetString("LE_ArgumentException_RegionShiftedOffWorksheet"), "region");

			if (region.Worksheet != this.worksheet)
				return false;

			// MD 3/13/12 - 12.1 - Table Support
			return this.Remove(region.Address);
		}

		// MD 3/13/12 - 12.1 - Table Support
		// Moved all code to the new overload.
		internal bool Remove(WorksheetRegionAddress region)
		{
			// MD 1/23/12 - TFS99849
			// RemoveRegionHelper is now an instance method and renamed to SubtractRegionHelper.
			//if (WorksheetReferenceCollection.RemoveRegionHelper(this.regionsSortedHorizontally, region))
			if (this.SubtractRegionHelper(region))
			{
				this.Condense();
				return true;
			}

			return false;
		}

		#endregion  // Remove( WorksheetRegion )

		#region ToString

		/// <summary>
		/// Returns the string value represent the cell and region addresses in the collection.
		/// </summary>
		/// <param name="cellReferenceMode">The cell reference mode with which to get the region strings.</param>
		/// <exception cref="InvalidEnumArgumentException">
		/// Occurs when <paramref name="cellReferenceMode"/> is not defined in the <see cref="CellReferenceMode"/> enumeration.
		/// </exception>
		public string ToString(CellReferenceMode cellReferenceMode)
		{
			if (Enum.IsDefined(typeof(CellReferenceMode), cellReferenceMode) == false)
				throw new InvalidEnumArgumentException("cellReferenceMode", (int)cellReferenceMode, typeof(CellReferenceMode));

			if (cellReferenceMode == CellReferenceMode.A1)
				return this.ToStringHelper(cellReferenceMode, ref this.cachedA1StringValue);

			return this.ToStringHelper(cellReferenceMode, ref this.cachedR1C1StringValue);
		}

		#endregion  // ToString

		#endregion  // Public Methods

		#region Internal Methods

		#region GetBoundingCells

		// MD 1/23/12
		// Found while fixing TFS99849
		// We only need two corner cells.
		//internal void GetBoundingCells(out WorksheetCell leftMostCell,
		//    out WorksheetCell topMostCell,
		//    out WorksheetCell rightMostCell,
		//    out WorksheetCell bottomMostCell)
		internal void GetBoundingCells(out WorksheetCell topLeftCell, out WorksheetCell bottomRightCell)
		{
			// MD 1/23/12
			// Found while fixing TFS99849
			// This can be made more efficient with the region lookup table.
			#region Old Code

			//leftMostCell = null;
			//topMostCell = null;
			//rightMostCell = null;
			//bottomMostCell = null;

			//if (this.regionsSortedHorizontally.Count == 0)
			//    return;

			//WorksheetRegion firstRegion = this.regionsSortedHorizontally[0];
			//leftMostCell = topMostCell = firstRegion.TopLeftCell;
			//bottomMostCell = rightMostCell = this.worksheet.Rows[firstRegion.LastRowInternal].Cells[firstRegion.LastColumnInternal];

			//for (int i = 1; i < this.regionsSortedHorizontally.Count; i++)
			//{
			//    WorksheetRegion region = this.regionsSortedHorizontally[i];

			//    if (region.FirstColumnInternal < leftMostCell.ColumnIndexInternal)
			//        leftMostCell = region.TopLeftCell;

			//    if (region.FirstRow < topMostCell.RowIndex)
			//        topMostCell = region.TopLeftCell;

			//    if (rightMostCell.ColumnIndexInternal < region.LastColumnInternal)
			//        rightMostCell = this.worksheet.Rows[region.LastRowInternal].Cells[region.LastColumnInternal];

			//    if (bottomMostCell.RowIndex < region.LastRowInternal)
			//        bottomMostCell = this.worksheet.Rows[region.LastRowInternal].Cells[region.LastColumnInternal];
			//}

			#endregion // Old Code
			int firstRowIndex;
			int firstColumnIndex;
			int lastRowIndex;
			int lastColumnIndex;
			this.regionLookupTable.GetBoundingIndexes(out firstRowIndex, out firstColumnIndex, out lastRowIndex, out lastColumnIndex);

			if (firstRowIndex == -1 || firstColumnIndex == -1 || lastRowIndex == -1 || lastColumnIndex == -1)
			{
				topLeftCell = bottomRightCell = null;
			}
			else
			{
				topLeftCell = this.worksheet.Rows[firstRowIndex].Cells[firstColumnIndex];
				bottomRightCell = this.worksheet.Rows[lastRowIndex].Cells[lastColumnIndex];
			}
		}

		#endregion  // GetBoundingCells

		#region OnCellsShifted

		internal void OnCellsShifted(CellShiftOperation shiftOperation, out bool isDeleted)
		{
			List<WorksheetRegionAddress> newRegionsToAdd = new List<WorksheetRegionAddress>();
			for (int i = this.regionsSortedHorizontally.Count - 1; i >= 0; i--)
			{
				WorksheetRegionAddress regionAddress = this.regionsSortedHorizontally[i];
				
				List<WorksheetRegionAddress> splitRegionAddresses;
				ShiftAddressResult result = shiftOperation.ShiftRegionAddress(ref regionAddress, true, true, out splitRegionAddresses);

				if (result.DidShift)
				{
					this.RemoveAtDirect(this.regionsSortedHorizontally[i], i);

					if (result.IsDeleted)
						continue;

					if (regionAddress.IsValid)
						newRegionsToAdd.Add(regionAddress);

					if (splitRegionAddresses != null)
					{
						for (int j = 0; j < splitRegionAddresses.Count; j++)
						{
							WorksheetRegionAddress splitRegionAddress = splitRegionAddresses[j];
							if (splitRegionAddress.IsValid)
								newRegionsToAdd.Add(splitRegionAddress);
						}
					}
				}
			}

			if (newRegionsToAdd.Count != 0)
			{
				this.AddRangeDirect(newRegionsToAdd);
				this.Condense();
			}

			isDeleted = this.regionsSortedHorizontally.Count == 0;
		}

		#endregion // OnCellsShifted

		#endregion  // Internal Methods

		#region Private Methods

		#region Add

		internal void Add(WorksheetReferenceCollection references)
		{
			bool cellsAdded = false;
			for (int i = 0; i < references.regionsSortedHorizontally.Count; i++)
				cellsAdded |= this.AddRegionHelper(references.regionsSortedHorizontally[i]);

			if (cellsAdded)
				this.Condense();
		}

		#endregion  // Add

		// MD 1/23/12 - TFS99849
		#region AddDirect

		// MD 3/13/12 - 12.1 - Table Support
		//private void AddDirect(WorksheetRegion region)
		private void AddDirect(WorksheetRegionAddress region)
		{
			this.regionsSortedHorizontally.Add(region);

			// MD 3/22/12 - TFS105885
			//this.regionLookupTable.Add(region, region);
			this.regionLookupTable.Add(region, region);
		}

		#endregion // AddRangeDirect

		// MD 1/23/12 - TFS99849
		#region AddRangeDirect

		// MD 3/13/12 - 12.1 - Table Support
		//private void AddRangeDirect(IEnumerable<WorksheetRegion> regions)
		private void AddRangeDirect(IEnumerable<WorksheetRegionAddress> regions)
		{
			// MD 3/13/12 - 12.1 - Table Support
			//foreach (WorksheetRegion region in regions)
			foreach (WorksheetRegionAddress region in regions)
				this.AddDirect(region);
		}

		#endregion // AddRangeDirect

		#region AddRegionHelper

		// MD 3/13/12 - 12.1 - Table Support
		//private bool AddRegionHelper(WorksheetRegion region)
		private bool AddRegionHelper(WorksheetRegionAddress region)
		{
			// If any regions completely contain the new region, it is already in the collection, so don't do anything.
			// MD 1/23/12 - TFS99849
			// This is inefficient. It can be done faster with a region lookup table.
			//for (int i = 0; i < this.regionsSortedHorizontally.Count; i++)
			//{
			//    if (this.regionsSortedHorizontally[i].Contains(region))
			//        return false;
			//}
			// MD 3/13/12 - 12.1 - Table Support
			//List<WorksheetRegion> regionsFullyContainingRange = this.regionLookupTable.GetItemsContainingRange(region);
			List<WorksheetRegionAddress> regionsFullyContainingRange = this.regionLookupTable.GetItemsContainingRange(region);
			if (regionsFullyContainingRange != null && regionsFullyContainingRange.Count != 0)
			{
				Debug.Assert(regionsFullyContainingRange.Count <= 1, "There cannot be more than one region fully enclosing the region.");
				return false;
			}

			// Remove the region being added from the current collection so it doesn't overlap anything when we add it in.
			// MD 1/23/12 - TFS99849
			// RemoveRegionHelper is now an instance method and renamed to SubtractRegionHelper.
			//WorksheetReferenceCollection.RemoveRegionHelper(this.regions, region);
			this.SubtractRegionHelper(region);

			// MD 1/23/12 - TFS99849
			// Use a helper method to do this so we can do it in other places as well.
			//int index = this.regionsSortedHorizontally.BinarySearch(region, WorksheetRegion.HorizontalSorter.Instance);
			//if (index >= 0)
			//{
			//    Utilities.DebugFail("The region is already in the collection.");
			//    return false;
			//}
			//
			//this.regionsSortedHorizontally.Insert(~index, region);
			this.InsertSorted(region);

			return true;
		}

		#endregion  // AddRegionHelper

		#region Condense

		private void Condense()
		{
			if (this.regionsSortedHorizontally.Count > 1)
			{
				// MD 1/23/12 - TFS99849
				// CondenseHelper is now an instance method.
				//WorksheetReferenceCollection.CondenseHelper(this.worksheet, this.regionsSortedHorizontally, true);
				this.CondenseHelper(this.worksheet, true);
			}

			this.ResetCache();
		}

		// MD 1/23/12 - TFS99849
		// Made this an instance method.
		//private static void CondenseHelper(Worksheet worksheet, List<WorksheetRegion> regions, bool expandRightFirst)
		private void CondenseHelper(Worksheet worksheet, bool expandRightFirst)
		{
			

			// The value is ignore here in this dictionary.
			// MD 3/13/12 - 12.1 - Table Support
			//Dictionary<WorksheetRegion, bool> expandedRegions = new Dictionary<WorksheetRegion, bool>();
			Dictionary<WorksheetRegionAddress, bool> expandedRegions = new Dictionary<WorksheetRegionAddress, bool>();

			while (true)
			{
				bool wereRegionsModified = false;

				for (int i = 0; i < this.regionsSortedHorizontally.Count; i++)
				{
					// MD 3/13/12 - 12.1 - Table Support
					//WorksheetRegion region = this.regionsSortedHorizontally[i];
					WorksheetRegionAddress region = this.regionsSortedHorizontally[i];

					// To prevent getting into an infinite loop, don't let a region expand twice.
					if (expandedRegions.ContainsKey(region))
						continue;

					// MD 3/13/12 - 12.1 - Table Support
					//WorksheetRegion combinedRegion = new WorksheetRegion(region.Worksheet, region.FirstRow, region.FirstColumn, region.LastRow, region.LastColumn, false);
					WorksheetRegionAddress combinedRegion = new WorksheetRegionAddress(region.FirstRowIndex, region.LastRowIndex, region.FirstColumnIndex, region.LastColumnIndex);

					//WorksheetReferenceCollection.Expand(this.regionsSortedHorizontally, combinedRegion, i, expandRightFirst);
					// MD 3/13/12 - 12.1 - Table Support
					//this.Expand(combinedRegion, i, expandRightFirst);
					this.Expand(ref combinedRegion, i, expandRightFirst);

					if (combinedRegion.Equals(region))
						continue;

					expandedRegions[region] = false;

					// MD 3/13/12 - 12.1 - Table Support
					//// Add the region to the worksheet cache.
					//WorksheetRegion foundRegion;
					//worksheet.AddCachedRegion(combinedRegion, out foundRegion);
					//if (foundRegion != null)
					//    combinedRegion = foundRegion;

					// Remove all intersecting regions.
					// MD 1/23/12 - TFS99849
					// RemoveRegionHelper is now an instance method and renamed to SubtractRegionHelper.
					//WorksheetReferenceCollection.RemoveRegionHelper(this.regionsSortedHorizontally, combinedRegion);
					this.SubtractRegionHelper(combinedRegion);

					// Add the combined region and resort the list.
					// MD 1/23/12 - TFS99849
					// Since RemoveRegionHelper sorts the regions already, all we need to do is insert at the right positions so we don't need to sort again.
					//this.regionsSortedHorizontally.Add(combinedRegion);
					//this.regionsSortedHorizontally.Sort(WorksheetRegion.HorizontalSorter.Instance);
					this.InsertSorted(combinedRegion);

					wereRegionsModified = true;
					break;
				}

				if (wereRegionsModified == false)
					break;
			}
		}

		#endregion  // Condense

		#region Expand

		// MD 1/23/12 - TFS99849
		// Made this an instance method.
		//private static void Expand(List<WorksheetRegion> regions, WorksheetRegion combinedRegion, int originalRegionIndex, bool expandRightFirst)
		// MD 3/13/12 - 12.1 - Table Support
		//private void Expand(WorksheetRegion combinedRegion, int originalRegionIndex, bool expandRightFirst)
		private void Expand(ref WorksheetRegionAddress combinedRegion, int originalRegionIndex, bool expandRightFirst)
		{
			bool hitHoleOnRight = false;
			bool hitHoleOnBottom = false;
			while (hitHoleOnRight == false || hitHoleOnBottom == false)
			{
				int neighborRowIndex = combinedRegion.LastRowIndex + 1;
				int neighborColumnIndex = combinedRegion.LastColumnIndex + 1;

				List<int> neededColumnIndexValues = new List<int>(combinedRegion.Width);
				if (hitHoleOnBottom == false)
				{
					for (int i = combinedRegion.FirstColumnIndex; i <= combinedRegion.LastColumnIndex; i++)
						neededColumnIndexValues.Add(i);
				}

				List<int> neededRowIndexValues = new List<int>(combinedRegion.Height);
				if (hitHoleOnRight == false)
				{
					for (int i = combinedRegion.FirstRowIndex; i <= combinedRegion.LastRowIndex; i++)
						neededRowIndexValues.Add(i);
				}

				// MD 1/23/12
				// Found while fixing TFS99849
				// This can be made more efficient with the region lookup table.
				#region Old Code

				//bool foundBottomRightCornerNeighbor = false;
				//for (int i = 0; i < this.regionsSortedHorizontally.Count; i++)
				//{
				//    if (i == originalRegionIndex)
				//        continue;

				//    WorksheetRegion neighborRegion = this.regionsSortedHorizontally[i];

				//    // If the next region does neighbor the first region, remove it's columns from the neededColumnIndexValues collection.
				//    if (hitHoleOnBottom == false &&
				//        neededColumnIndexValues.Count > 0 &&
				//        neighborRegion.FirstRow <= neighborRowIndex && neighborRowIndex <= neighborRegion.LastRow &&
				//        neighborRegion.LastColumn >= neededColumnIndexValues[0] &&
				//        neighborRegion.FirstColumn <= neededColumnIndexValues[neededColumnIndexValues.Count - 1])
				//    {
				//        int firstIndex = neededColumnIndexValues.BinarySearch(neighborRegion.FirstColumn);
				//        if (firstIndex < 0)
				//            firstIndex = ~firstIndex;

				//        int lastIndex = neededColumnIndexValues.BinarySearch(neighborRegion.LastColumn);
				//        if (lastIndex < 0)
				//            lastIndex = ~lastIndex - 1;

				//        neededColumnIndexValues.RemoveRange(firstIndex, (lastIndex - firstIndex) + 1);
				//    }

				//    // If the next region does neighbor the first region, remove it's rows from the neededRowIndexValues collection.
				//    if (hitHoleOnRight == false &&
				//        neededRowIndexValues.Count > 0 &&
				//        neighborRegion.FirstColumn <= neighborColumnIndex && neighborColumnIndex <= neighborRegion.LastColumn &&
				//        neighborRegion.LastRow >= neededRowIndexValues[0] &&
				//        neighborRegion.FirstRow <= neededRowIndexValues[neededRowIndexValues.Count - 1])
				//    {
				//        int firstIndex = neededRowIndexValues.BinarySearch(neighborRegion.FirstRow);
				//        if (firstIndex < 0)
				//            firstIndex = ~firstIndex;

				//        int lastIndex = neededRowIndexValues.BinarySearch(neighborRegion.LastRow);
				//        if (lastIndex < 0)
				//            lastIndex = ~lastIndex - 1;

				//        neededRowIndexValues.RemoveRange(firstIndex, (lastIndex - firstIndex) + 1);
				//    }

				//    if (foundBottomRightCornerNeighbor == false)
				//    {
				//        foundBottomRightCornerNeighbor = neighborRegion.Contains(combinedRegion.LastRow + 1, combinedRegion.LastColumn + 1);
				//    }

				//    if (neededRowIndexValues.Count == 0 && neededColumnIndexValues.Count == 0)
				//    {
				//        if (hitHoleOnBottom || hitHoleOnRight || foundBottomRightCornerNeighbor)
				//            break;
				//    }
				//}

				#endregion // Old Code
				// MD 3/13/12 - 12.1 - Table Support
				//List<WorksheetRegion> regionsOnRightBorder = this.regionLookupTable.GetItemsIntersectingWithRange(
				List<WorksheetRegionAddress> regionsOnRightBorder = this.regionLookupTable.GetItemsIntersectingWithRange(
					combinedRegion.FirstRowIndex, 
					combinedRegion.LastColumnIndex + 1, 
					combinedRegion.LastRowIndex, 
					combinedRegion.LastColumnIndex + 1);
				// MD 3/13/12 - 12.1 - Table Support
				//List<WorksheetRegion> regionsOnBottomBorder = this.regionLookupTable.GetItemsIntersectingWithRange(
				List<WorksheetRegionAddress> regionsOnBottomBorder = this.regionLookupTable.GetItemsIntersectingWithRange(
					combinedRegion.LastRowIndex + 1,
					combinedRegion.FirstColumnIndex,
					combinedRegion.LastRowIndex + 1,
					combinedRegion.LastColumnIndex);

				if (regionsOnRightBorder != null)
				{
					for (int i = 0; i < regionsOnRightBorder.Count; i++)
					{
						// MD 3/13/12 - 12.1 - Table Support
						//WorksheetRegion regionOnRightBorder = regionsOnRightBorder[i];
						WorksheetRegionAddress regionOnRightBorder = regionsOnRightBorder[i];

						int index = neededRowIndexValues.BinarySearch(regionOnRightBorder.FirstRowIndex);
						if (0 <= index)
						{
							int height = regionOnRightBorder.Height;
							for (int j = 0; j < height && index < neededRowIndexValues.Count; j++)
								neededRowIndexValues.RemoveAt(index);
						}
					}
				}

				if (regionsOnBottomBorder != null)
				{
					for (int i = 0; i < regionsOnBottomBorder.Count; i++)
					{
						// MD 3/13/12 - 12.1 - Table Support
						//WorksheetRegion regionOnBottomBorder = regionsOnBottomBorder[i];
						WorksheetRegionAddress regionOnBottomBorder = regionsOnBottomBorder[i];

						int index = neededColumnIndexValues.BinarySearch(regionOnBottomBorder.FirstColumnIndex);
						if (0 <= index)
						{
							int width = regionOnBottomBorder.Width;
							for (int j = 0; j < width && index < neededColumnIndexValues.Count; j++)
								neededColumnIndexValues.RemoveAt(index);
						}
					}
				}

				hitHoleOnBottom |= (neededColumnIndexValues.Count > 0);
				hitHoleOnRight |= (neededRowIndexValues.Count > 0);

				if (hitHoleOnBottom == false && hitHoleOnRight == false)
				{
					// MD 1/23/12 - TFS99849
					//if (foundBottomRightCornerNeighbor)
					// MD 3/13/12 - 12.1 - Table Support
					//List<WorksheetRegion> bottomRightCornerRegions = this.regionLookupTable.GetItemsContainingCell(combinedRegion.LastRow + 1, combinedRegion.LastColumnInternal + 1);
					List<WorksheetRegionAddress> bottomRightCornerRegions = this.regionLookupTable.GetItemsContainingCell(combinedRegion.LastRowIndex + 1, combinedRegion.LastColumnIndex + 1);

					if (bottomRightCornerRegions != null && bottomRightCornerRegions.Count != 0)
					{
						combinedRegion.LastRowIndex++;
						combinedRegion.LastColumnIndex++;
					}
					else if (expandRightFirst)
					{
						combinedRegion.LastColumnIndex++;
					}
					else
					{
						combinedRegion.LastRowIndex++;
					}
				}
				else if (hitHoleOnRight == false)
				{
					combinedRegion.LastColumnIndex++;
				}
				else if (hitHoleOnBottom == false)
				{
					combinedRegion.LastRowIndex++;
				}
			}
		} 

		#endregion  // Expand

		#region GetRegions

		// MD 4/9/12 - TFS101506
		//private List<WorksheetRegion> GetRegions(string references, CellReferenceMode cellReferenceMode)
		private List<WorksheetRegion> GetRegions(string references, CellReferenceMode cellReferenceMode, CultureInfo culture)
		{
			WorkbookFormat format = this.Worksheet.CurrentFormat;

			string[] referencesSplit = references.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			List<WorksheetRegion> regions = new List<WorksheetRegion>(referencesSplit.Length);

			for (int i = 0; i < referencesSplit.Length; i++)
			{
				string reference = referencesSplit[i].Trim();

				// MD 3/13/12 - 12.1 - Table Support
				#region Old Code

				//int firstRowIndex;
				//short firstColumnIndex;
				//int lastRowIndex;
				//short lastColumnIndex;

				//Utilities.ParseRegionAddress(reference, format, cellReferenceMode, null, -1,
				//    out firstRowIndex, out firstColumnIndex, out lastRowIndex, out lastColumnIndex);

				//if (0 <= lastRowIndex)
				//{
				//    regions.Add(this.worksheet.GetCachedRegion(firstRowIndex, firstColumnIndex, lastRowIndex, lastColumnIndex));
				//    continue;
				//}

				//if (0 <= firstRowIndex)
				//{
				//    regions.Add(this.worksheet.GetCachedRegion(firstRowIndex, firstColumnIndex, firstRowIndex, firstColumnIndex));
				//    continue;
				//}

				//throw new ArgumentException(SR.GetString("LE_ArgumentException_InvalidReferencesString"), "references");

				#endregion // Old Code
				// MD 8/6/12 - TFS118386
				// Use the passed in CellReferenceMode.
				//WorksheetRegion region = this.worksheet.GetRegion(reference);
				WorksheetRegion region = this.worksheet.GetRegion(reference, cellReferenceMode);

				if (region == null)
					throw new ArgumentException(SR.GetString("LE_ArgumentException_InvalidReferencesString"), "references");

				regions.Add(region);
			}

			return regions;
		}

		#endregion  // GetRegions

		#region Initialize

		private void Initialize(Worksheet worksheet)
		{
			this.worksheet = worksheet;

			// MD 3/13/12 - 12.1 - Table Support
			//this.regionsSortedHorizontally = new List<WorksheetRegion>();
			//
			//// MD 1/23/12 - TFS99849
			//this.regionLookupTable = new RegionLookupTable<WorksheetRegion>();
			this.regionsSortedHorizontally = new List<WorksheetRegionAddress>();

			// MD 3/22/12 - TFS105885
			//this.regionLookupTable = new RegionLookupTable<WorksheetRegionAddress>();
			this.regionLookupTable = new RegionLookupTable<WorksheetRegionAddress>(this.worksheet);
		}

		#endregion  // Initialize

		// MD 1/23/12 - TFS99849
		#region InsertDirect

		// MD 3/13/12 - 12.1 - Table Support
		//private void InsertDirect(int index, WorksheetRegion region)
		private void InsertDirect(int index, WorksheetRegionAddress region)
		{
			this.regionsSortedHorizontally.Insert(index, region);

			// MD 3/22/12 - TFS105885
			//this.regionLookupTable.Add(region, region);
			this.regionLookupTable.Add(region, region);
		}

		#endregion // InsertDirect

		// MD 1/23/12 - TFS99849
		#region InsertSorted

		// MD 3/13/12 - 12.1 - Table Support
		//private void InsertSorted(WorksheetRegion region)
		private void InsertSorted(WorksheetRegionAddress region)
		{
			int index = this.regionsSortedHorizontally.BinarySearch(region, WorksheetRegion.HorizontalSorter.Instance);
			if (index < 0)
				this.InsertDirect(~index, region);
			else
				Utilities.DebugFail("The region is already in the collection.");
		}

		#endregion // InsertSorted

		// MD 1/23/12 - TFS99849
		#region RemoveAtDirect

		// MD 3/13/12 - 12.1 - Table Support
		//private void RemoveAtDirect(WorksheetRegion region, int index)
		private void RemoveAtDirect(WorksheetRegionAddress region, int index)
		{
			this.regionsSortedHorizontally.RemoveAt(index);

			// MD 3/22/12 - TFS105885
			//this.regionLookupTable.Remove(region, region);
			this.regionLookupTable.Remove(region, region);
		}

		#endregion // RemoveAtDirect

		// MD 1/23/12 - TFS99849
		#region RemoveDirect

		// MD 3/13/12 - 12.1 - Table Support
		//private void RemoveDirect(WorksheetRegion region)
		private void RemoveDirect(WorksheetRegionAddress region)
		{
			int index = this.regionsSortedHorizontally.BinarySearch(region, WorksheetRegion.HorizontalSorter.Instance);
			if (index < 0)
			{
				Utilities.DebugFail("The region is not in the collection.");
				return;
			}

			this.RemoveAtDirect(region, index);
		}

		#endregion // RemoveDirect

		#region SubtractRegionHelper

		// MD 1/23/12 - TFS99849
		// Made this an instance method and renamed it for clarity
		//private static bool RemoveRegionHelper(List<WorksheetRegion> regions, WorksheetRegion region)
		// MD 3/13/12 - 12.1 - Table Support
		//private bool SubtractRegionHelper(WorksheetRegion region)
		private bool SubtractRegionHelper(WorksheetRegionAddress region)
		{
			// MD 1/23/12 - TFS99849
			// Rewrote this method to be more efficient with the region lookup table.
			#region Old Code

			//bool returnValue = false;
			//bool regionsAdded = false;

			//for (int i = this.regionsSortedHorizontally.Count - 1; i >= 0; i--)
			//{
			//    WorksheetRegion existingRegion = this.regionsSortedHorizontally[i];
			//    if (region.IntersectsWith(existingRegion) == false)
			//        continue;

			//    //this.regionsSortedHorizontally.RemoveAt(i);
			//    this.RemoveAtDirect(existingRegion, i);

			//    List<WorksheetRegion> regionsNotInSubtractedArea;
			//    WorksheetReferenceCollection.SubtractRegion(existingRegion, region, out regionsNotInSubtractedArea);

			//    //this.regionsSortedHorizontally.AddRange(regionsNotInSubtractedArea);
			//    this.AddRangeDirect(regionsNotInSubtractedArea);

			//    regionsAdded |= regionsNotInSubtractedArea.Count > 0;

			//    returnValue = true;
			//}

			//if (regionsAdded)
			//    this.regionsSortedHorizontally.Sort(WorksheetRegion.HorizontalSorter.Instance);

			//return returnValue;

			#endregion // Old Code
			// MD 3/13/12 - 12.1 - Table Support
			//List<WorksheetRegion> intersectingRegions = this.regionLookupTable.GetItemsIntersectingWithRange(region);
			List<WorksheetRegionAddress> intersectingRegions = this.regionLookupTable.GetItemsIntersectingWithRange(region);

			if (intersectingRegions == null || intersectingRegions.Count == 0)
				return false;

			bool regionsAdded = false;
			for (int i = intersectingRegions.Count - 1; i >= 0; i--)
			{
				// MD 3/13/12 - 12.1 - Table Support
				//WorksheetRegion existingRegion = intersectingRegions[i];
				WorksheetRegionAddress existingRegion = intersectingRegions[i];

				this.RemoveDirect(existingRegion);

				// MD 3/13/12 - 12.1 - Table Support
				//List<WorksheetRegion> regionsNotInSubtractedArea;
				List<WorksheetRegionAddress> regionsNotInSubtractedArea;

				WorksheetReferenceCollection.SubtractRegion(existingRegion, region, out regionsNotInSubtractedArea);

				this.AddRangeDirect(regionsNotInSubtractedArea);
				regionsAdded |= regionsNotInSubtractedArea.Count > 0;
			}

			if (regionsAdded)
				this.regionsSortedHorizontally.Sort(WorksheetRegion.HorizontalSorter.Instance);

			return true;
		}

		#endregion  // SubtractRegionHelper

		#region ResetCache

		private void ResetCache()
		{
			this.cachedCellsCount = null;
			this.cachedA1StringValue = null;
			this.cachedR1C1StringValue = null;
		}

		#endregion  // ResetCache

		#region SubtractRegion

		// MD 3/13/12 - 12.1 - Table Support
		//private static void SubtractRegion(WorksheetRegion sourceRegion, WorksheetRegion regionToSubtract,
		//    out List<WorksheetRegion> regionsNotInSubtractedArea)
		private static void SubtractRegion(WorksheetRegionAddress sourceRegion, WorksheetRegionAddress regionToSubtract,
			out List<WorksheetRegionAddress> regionsNotInSubtractedArea)
		{
			#region Old Code

			//regionsNotInSubtractedArea = new List<WorksheetRegion>();

			//// Top region
			//if (sourceRegion.FirstRow < regionToSubtract.FirstRow)
			//{
			//    regionsNotInSubtractedArea.Add(new WorksheetRegion(sourceRegion.Worksheet,
			//        sourceRegion.FirstRow, sourceRegion.FirstColumn,
			//        regionToSubtract.FirstRow - 1, sourceRegion.LastColumn));
			//}

			//// Bottom region
			//if (regionToSubtract.LastRow < sourceRegion.LastRow)
			//{
			//    regionsNotInSubtractedArea.Add(new WorksheetRegion(sourceRegion.Worksheet,
			//        regionToSubtract.LastRow + 1, sourceRegion.FirstColumn,
			//        sourceRegion.LastRow, sourceRegion.LastColumn));
			//}

			//int sideChunkFirstRowIndex = Math.Max(regionToSubtract.FirstRow, sourceRegion.FirstRow);
			//int sideChunkLastRowIndex = Math.Min(regionToSubtract.LastRow, sourceRegion.LastRow);

			//// Left region
			//if (sourceRegion.FirstColumn < regionToSubtract.FirstColumn)
			//{
			//    regionsNotInSubtractedArea.Add(new WorksheetRegion(sourceRegion.Worksheet,
			//        sideChunkFirstRowIndex, sourceRegion.FirstColumn,
			//        sideChunkLastRowIndex, regionToSubtract.FirstColumn - 1));
			//}

			//// Right region
			//if (regionToSubtract.LastColumn < sourceRegion.LastColumn)
			//{
			//    regionsNotInSubtractedArea.Add(new WorksheetRegion(sourceRegion.Worksheet,
			//        sideChunkFirstRowIndex, regionToSubtract.LastColumn + 1,
			//        sideChunkLastRowIndex, sourceRegion.LastColumn));
			//}

			#endregion // Old Code
			regionsNotInSubtractedArea = new List<WorksheetRegionAddress>();

			// Top region
			if (sourceRegion.FirstRowIndex < regionToSubtract.FirstRowIndex)
			{
				regionsNotInSubtractedArea.Add(new WorksheetRegionAddress(
					sourceRegion.FirstRowIndex, regionToSubtract.FirstRowIndex - 1,
					sourceRegion.FirstColumnIndex, sourceRegion.LastColumnIndex));
			}

			// Bottom region
			if (regionToSubtract.LastRowIndex < sourceRegion.LastRowIndex)
			{
				regionsNotInSubtractedArea.Add(new WorksheetRegionAddress(
					regionToSubtract.LastRowIndex + 1, sourceRegion.LastRowIndex,
					sourceRegion.FirstColumnIndex, sourceRegion.LastColumnIndex));
			}

			int sideChunkFirstRowIndex = Math.Max(regionToSubtract.FirstRowIndex, sourceRegion.FirstRowIndex);
			int sideChunkLastRowIndex = Math.Min(regionToSubtract.LastRowIndex, sourceRegion.LastRowIndex);

			// Left region
			if (sourceRegion.FirstColumnIndex < regionToSubtract.FirstColumnIndex)
			{
				regionsNotInSubtractedArea.Add(new WorksheetRegionAddress(
					sideChunkFirstRowIndex, sideChunkLastRowIndex,
					sourceRegion.FirstColumnIndex, (short)(regionToSubtract.FirstColumnIndex - 1)));
			}

			// Right region
			if (regionToSubtract.LastColumnIndex < sourceRegion.LastColumnIndex)
			{
				regionsNotInSubtractedArea.Add(new WorksheetRegionAddress(
					sideChunkFirstRowIndex, sideChunkLastRowIndex,
					(short)(regionToSubtract.LastColumnIndex + 1), sourceRegion.LastColumnIndex));
			}
		}

		#endregion  // SubtractRegion

		#region ToStringHelper

		private string ToStringHelper(CellReferenceMode cellReferenceMode, ref string cachedString)
		{
			if (cachedString != null)
				return cachedString;

			bool useRelativeReferences = (cellReferenceMode == CellReferenceMode.A1);

			StringBuilder sb = new StringBuilder();

			foreach (object item in (System.Collections.IEnumerable)this)
			{
				if (sb.Length != 0)
					sb.Append(" ");

				WorksheetCell cell = item as WorksheetCell;

				if (cell != null)
				{
					sb.Append(cell.ToString(cellReferenceMode, false, useRelativeReferences, useRelativeReferences));
					continue;
				}

				WorksheetRegion region = (WorksheetRegion)item;
				sb.Append(region.ToString(cellReferenceMode, false, useRelativeReferences, useRelativeReferences));
			}

			cachedString = sb.ToString();
			return cachedString;
		}

		#endregion  // ToStringHelper

		#endregion  // Private Methods

		#endregion  // Methods

		#region Properties

		#region Public Properties

		#region CellsCount

		/// <summary>
		/// Gets the number of cells contains in all references in this collection.
		/// </summary>
		public int CellsCount
		{
			get
			{
				if (this.cachedCellsCount.HasValue == false)
				{
					int cellCount = 0;
					for (int i = 0; i < this.regionsSortedHorizontally.Count; i++)
					{
						// MD 3/13/12 - 12.1 - Table Support
						//WorksheetRegion region = this.regionsSortedHorizontally[i];
						WorksheetRegionAddress region = this.regionsSortedHorizontally[i];

						cellCount += region.Height * region.Width;
					}

					this.cachedCellsCount = cellCount;
				}

				return this.cachedCellsCount.Value;
			}
		}

		#endregion  // CellsCount

		#region Worksheet

		/// <summary>
		/// Gets the worksheet for which this collection contains references.
		/// </summary>
		public Worksheet Worksheet
		{
			get { return this.worksheet; }
		}

		#endregion  // Worksheet

		#endregion  // Public Properties

		#region Internal Properties

		#region Regions

		// MD 3/13/12 - 12.1 - Table Support
		//internal List<WorksheetRegion> Regions
		internal List<WorksheetRegionAddress> Regions
		{
			get { return this.regionsSortedHorizontally; }
		}

		#endregion  // Regions

		#region TopLeftCell

		internal WorksheetCell TopLeftCell
		{
			get
			{
				if (this.regionsSortedHorizontally.Count == 0)
					return null;

				WorksheetRegionAddress address = this.regionsSortedHorizontally[0];
				return this.worksheet.Rows[address.FirstRowIndex].Cells[address.FirstColumnIndex];
			}
		}

		#endregion // TopLeftCell

		#endregion  // Internal Properties

		#endregion  // Properties
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