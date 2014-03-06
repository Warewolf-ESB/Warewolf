using System;
using System.Collections;
using System.Diagnostics;
using System.Collections.Generic;
using Infragistics.Documents.Excel.FormulaUtilities.CalcEngine;


using Infragistics.Shared;


namespace Infragistics.Documents.Excel.CalcEngine
{



#region Infragistics Source Cleanup (Region)













#endregion // Infragistics Source Cleanup (Region)

	internal class UltraCalcAncestorMap : ICollection
	{
		#region Member Variables

		private List<AncestorMapEntry> ancestorList;

		private Dictionary<IExcelCalcReference, AncestorMapEntry> ancestorDictionary =
			new Dictionary<IExcelCalcReference, AncestorMapEntry>();

		private Dictionary<CellCalcReference, AncestorMapEntry> cellPredecessorEntries =
			new Dictionary<CellCalcReference, AncestorMapEntry>();

		private Dictionary<Worksheet, RegionLookupTable<RegionCalcReferenceBase>> regionPredecessorEntries = new Dictionary<Worksheet, RegionLookupTable<RegionCalcReferenceBase>>();
		private List<AncestorMapEntry> nonCellPredecessorEntries = new List<AncestorMapEntry>();

		#endregion //Member Variables

		#region Constructor

		internal UltraCalcAncestorMap()
		{
			ancestorList = new List<AncestorMapEntry>();
		}

		#endregion //Constructor

		#region Implementation of IUltraCalcAncestorMap

		/// <summary>
		/// Add a formula reference the given predecessor reference
		/// </summary>
		/// <param name="predecessor"><b>UCReference</b> predecessor reference</param>
		/// <param name="ancestor"><b>IUltraCalcReference</b> of formula that contains the given predecessor</param>
		public void AddAncestor(UCReference predecessor, IExcelCalcReference ancestor)
		{
			try
			{
				Debug.Assert(!(ancestor is FormulaReference), "Unexpected ancestor type");

				IExcelCalcReference ancestorRef = new FormulaReference(ancestor);

				// MD 8/21/08 - Excel formula solving - Performance
				// This is a better way to search for the ancestor in Excel
				IExcelCalcReference resolvedPrecessor = ExcelCalcEngine.GetResolvedReference(predecessor);

				AncestorMapEntry existingEntry;
				if (this.ancestorDictionary.TryGetValue(resolvedPrecessor, out existingEntry))
					existingEntry.Ancestors.Add(ancestorRef);
				else
				{
					// Create a new Ancestor entry for this Predecessor/Ancestor
					AncestorMapEntry entry = new AncestorMapEntry(predecessor);
					entry.Ancestors.Add(ancestorRef);

					// MD 8/21/08 - Excel formula solving - Performance
					this.ancestorDictionary.Add(resolvedPrecessor, entry);

					CellCalcReference cellPrecessor = resolvedPrecessor as CellCalcReference;

					if (cellPrecessor != null)
						this.cellPredecessorEntries.Add(cellPrecessor, entry);
					else
					{
						// MD 11/1/10 - TFS56976
						// Instead of keeping regions in the linear nonCellPredecessorEntries collection, we will now use a special lookup table for them.
						//this.nonCellPredecessorEntries.Add( entry );
						// MD 2/24/12 - 12.1 - Table Support
						//RegionCalcReference regionPrecessor = resolvedPrecessor as RegionCalcReference;
						RegionCalcReferenceBase regionPrecessor = resolvedPrecessor as RegionCalcReferenceBase;
						if (regionPrecessor != null)
						{
							// MD 1/23/12 - TFS99849
							// Added a region parameter to the Add method because Add now takes a generic item.
							//this.regionPredecessorEntries.Add(regionPrecessor);
							// MD 2/24/12 - 12.1 - Table Support
							//this.regionPredecessorEntries.Add(regionPrecessor.Region, regionPrecessor);
							WorksheetRegion region = regionPrecessor.Region;
							if (region != null)
							{
								// MD 3/22/12 - TFS105885
								// We now keep a separate RegionLookupTable for each worksheet. 
								//this.regionPredecessorEntries.Add(region.Address, regionPrecessor);
								Worksheet worksheet = regionPrecessor.Worksheet;
								RegionLookupTable<RegionCalcReferenceBase> regionLookupTable;
								if (this.regionPredecessorEntries.TryGetValue(worksheet, out regionLookupTable) == false)
								{
									regionLookupTable = new RegionLookupTable<RegionCalcReferenceBase>(worksheet);
									this.regionPredecessorEntries.Add(worksheet, regionLookupTable);
								}

								regionLookupTable.Add(region, regionPrecessor);
							}
						}
						else
						{
							this.nonCellPredecessorEntries.Add(entry);
						}
					}
					ancestorList.Add(entry);
				}
			}
			catch (Exception e)
			{
				Debug.Assert(false, "UltraCalcAncestorMap.AddAncestor: [0]", e.ToString());
				throw new UltraCalcException(SR.GetString("Error_Internal", new object[] { "UltraCalcAncestorMap.AddAncestor" }), e);
			}
		}

		/// <summary>
		/// Return the position of the given predecessor in the collection
		/// </summary>
		/// <param name="predecessor"></param>
		/// <returns></returns>
		private int Find(IExcelCalcReference predecessor)
		{
			// MD 8/21/08 - Excel formula solving - Performance
			// This is a better way to search for the ancestor in Excel
			AncestorMapEntry existingEntry;
			if (this.ancestorDictionary.TryGetValue(ExcelCalcEngine.GetResolvedReference(predecessor), out existingEntry) == false)
				return -1;

			for (int i = 0; i < ancestorList.Count; i++)
			{
				if (ancestorList[i] == existingEntry)
					return i;
			}

			Utilities.DebugFail("The entry should have been in the list. The dictionary is out of sync.");
			return -1;
		}


		/// <summary>
		/// Remove the given formula ancestor from the given  predecessor's list of formulas.
		/// </summary>
		/// <param name="formulaPredecessor"><b>IUltraCalcReference</b> that's referenced by given ancestor's formula</param>
		/// <param name="ancestor">Formula reference to remove for given predecessor's list of formulas</param>
		public void DeleteAncestor(IExcelCalcReference formulaPredecessor, IExcelCalcReference ancestor)
		{
			try
			{
				// Find the predecessor entry in the map
				int posPredecessor = Find(formulaPredecessor);

				// Is it in the map?
				if (posPredecessor != -1)
				{
					int posAncestor;
					// Find the ancestor is in predecessor's ancestor list
					if ((posAncestor = ancestorList[posPredecessor].Ancestors.IndexOf(ancestor)) != -1)
						DeleteAncestorAt(posPredecessor, posAncestor, formulaPredecessor == ancestorList[posPredecessor].Predecessor);
				}
			}
			catch (Exception e)
			{
				Debug.Assert(false, "UltraCalcAncestorMap.DeleteAncestor: [0]", e.ToString());
				throw new UltraCalcException(SR.GetString("Error_Internal", new object[] { "UltraCalcAncestorMap.DeleteAncestor" }), e);
			}
		}

		/// <summary>
		/// Remove the given formula ancestor from the given  predecessor's list of formulas.
		/// </summary>
		/// <param name="posPredecessor">Position of the predecessor in the ancestor list</param>
		/// <param name="posAncestor">Position of the ancestor in the predecessor's Ancestors list</param>
		/// <param name="replacePredecessorInMapEntry">Indicates whether to update the predecessor reference in the ancestor map</param>
		public void DeleteAncestorAt(int posPredecessor, int posAncestor, bool replacePredecessorInMapEntry)
		{
			try
			{
				Debug.Assert(posPredecessor != -1);
				Debug.Assert(posAncestor != -1);

				// Remove the ancestor from the predecessor's ancestor list
				AncestorMapEntry entry = ancestorList[posPredecessor];
				entry.Ancestors.RemoveAt(posAncestor);

				// If there aren't any more ancestors for this predecessor, remove the predecessor entry
				if (entry.Ancestors.Count == 0)
				{
					// MD 8/21/08 - Excel formula solving - Performance
					this.RemoveEntry(entry);

					ancestorList.RemoveAt(posPredecessor);
				}
				else
				{
					// If the ancestor we just deleted had a predecessor that was being used as the key to the ancestorMapEntry, replace it
					// with an instance of predecessor from another ancestor
					if (replacePredecessorInMapEntry)
					{
						IExcelCalcReference ancestor = entry.Ancestors[0];
						foreach (UCReference predecessor in ancestor.Formula.References)
						{
							// SSP 9/7/04
							// Perform a case insensitive comparison since absolute names are case insensitive.
							//
							//if (String.Compare(entry.Predecessor.AbsoluteName, predecessor.AbsoluteName) == 0)
							// SSP 7/7/05
							// I noticed this while debugging another issue. This is not right. We should
							// be returning only when the predecessor is replaced.
							//
							// ------------------------------------------------------------------------------
							




							if (ExcelCalcEngine.CompareReferences(entry.Predecessor, predecessor))
							{
								entry.Predecessor = predecessor;
								return;
							}
							// ------------------------------------------------------------------------------
						}
					}
				}
			}
			catch (Exception e)
			{
				Debug.Assert(false, "UltraCalcAncestorMap.DeleteAncestorAt: [0]", e.ToString());
				throw new UltraCalcException(SR.GetString("Error_Internal", new object[] { "UltraCalcAncestorMap.DeleteAncestorAt" }), e);
			}
		}

		/// <summary>
		/// Return the collection of ancestors of the given predecessor
		/// </summary>
		/// <param name="predecessor">Reference whose collection of formulas to return</param>
		/// <returns>Collection of references whose formulas reference the given predecessor</returns>
		public UltraCalcReferenceCollection Ancestors(IExcelCalcReference predecessor)
		{
			UltraCalcReferenceCollection outCol = null;

			// MD 1/12/12 - TFS99279
			// Copied this code from the UltraCalcEngine.DirtyParents so we can use those performance enhancements in all places
			// where we need to iterate all ancestors for a given predecessor.

			// MD 8/22/08 - Excel formula solving - Performance
			// Instead of always searching through each entry, always search through the non-cell predecessor 
			// entries and if the predecessor is a cell reference, just dirty that one cell reference.
			IExcelCalcReference resolvedPredecessor = ExcelCalcEngine.GetResolvedReference(predecessor);
			CellCalcReference cellReference = resolvedPredecessor as CellCalcReference;

			if (cellReference != null)
			{
				UltraCalcAncestorMap.AncestorMapEntry entry;
				if (this.CellPredecessorEntries.TryGetValue(cellReference, out entry))
				{
					if (outCol == null)
						outCol = new UltraCalcReferenceCollection(entry.Ancestors);
					else
						outCol.Merge(entry.Ancestors);
				}

				// MD 11/1/10 - TFS56976
				// Search through the new collection of region predecessors for regions containing the cell.
				// MD 4/12/11 - TFS67084
				// Moved away from using WorksheetCell objects.
				//List<RegionCalcReference> regionCalcReferences = this.ancestorMap.RegionPredecessorEntries.GetRegionsContainingCell(cellReference.Cell);
				// MD 1/23/12 - TFS99849
				// The GetItemsContainingCell doesn't need a row instance, it just needs the row index, so the parameter type has been changed.
				//List<RegionCalcReference> regionCalcReferences = this.RegionPredecessorEntries.GetRegionsContainingCell(cellReference.Row, cellReference.ColumnIndex);
				// MD 2/24/12 - 12.1 - Table Support
				//List<RegionCalcReference> regionCalcReferences = this.RegionPredecessorEntries.GetItemsContainingCell(cellReference.Row.Index, cellReference.ColumnIndex);
				// MD 3/22/12 - TFS105885
				// We now keep a separate RegionLookupTable for each worksheet.
				//List<RegionCalcReferenceBase> regionCalcReferences = this.RegionPredecessorEntries.GetItemsContainingCell(cellReference.Row.Index, cellReference.ColumnIndex);
				List<RegionCalcReferenceBase> regionCalcReferences = null;
				RegionLookupTable<RegionCalcReferenceBase> regionLookupTable;
				if (this.RegionPredecessorEntries.TryGetValue(cellReference.Worksheet, out regionLookupTable))
					regionCalcReferences = regionLookupTable.GetItemsContainingCell(cellReference.Row.Index, cellReference.ColumnIndex);

				if (regionCalcReferences != null)
				{
					// Dirty the parents of each region reference that contains the cell.
					// MD 2/24/12 - 12.1 - Table Support
					//foreach (RegionCalcReference regionCalcReference in regionCalcReferences)
					foreach (RegionCalcReferenceBase regionCalcReference in regionCalcReferences)
					{
						if (this.AncestorDictionary.TryGetValue(regionCalcReference, out entry) == false)
						{
							Utilities.DebugFail("Could not find the RegionCalcReference in the AncestorDictionary.");
							continue;
						}

						if (outCol == null)
							outCol = new UltraCalcReferenceCollection(entry.Ancestors);
						else
							outCol.Merge(entry.Ancestors);
					}
				}
			}

			// MD 11/1/10 - TFS56976
			// Now that region predecessors are stored in a different collection, we should verify the assumption that cell references are the only
			// things that are dirtied that could intersect with the region.
			Debug.Assert(
				cellReference != null || resolvedPredecessor is NamedCalcReference || resolvedPredecessor.Value.IsError,
				"Didn't expect somethig other than a cell, named reference or error to be dirtied: " + resolvedPredecessor.GetType().Name);


			// MD 11/1/10 - TFS56976
			// Wrapped this foreach loop in an if statement. There's no point to creating and disposing an enumerator when there are no items.
			if (this.NonCellPredecessorEntries.Count > 0)
			{
				foreach (UltraCalcAncestorMap.AncestorMapEntry entry in this.NonCellPredecessorEntries)
				{
					if (entry.Predecessor.ContainsReference(resolvedPredecessor))
					{
						if (outCol == null)
							outCol = new UltraCalcReferenceCollection(entry.Ancestors);
						else
							outCol.Merge(entry.Ancestors);
					}
				}
			}

			if (outCol == null)
				return new UltraCalcReferenceCollection();
			else
				return outCol;
		}

		#endregion

		#region Implementation of ICollection
		/// <summary>
		/// Copys the collection to an array.
		/// </summary>
		/// <param name="array">Array used for the desitnation of the copy.</param>
		/// <param name="index">The zero-based index in array at which copying begins.</param>
		public void CopyTo(System.Array array, int index)
		{
			// MD 8/22/08 - Excel formula solving - Performance
			// The ancestor list will now be a generic list.
			//ancestorList.CopyTo(array, index);		
			ancestorList.CopyTo((AncestorMapEntry[])array, index);
		}

		/// <summary>
		/// Denotes whether the collection is thread-safe.
		/// </summary>
		public bool IsSynchronized
		{
			get
			{
				// MD 8/22/08 - Excel formula solving - Performance
				// The ancestor list will now be a generic list.
				//return ancestorList.IsSynchronized;
				return false;
			}
		}

		/// <summary>
		/// Returns a number of elements in the collection.
		/// </summary>
		public int Count
		{
			get
			{
				return ancestorList.Count;
			}
		}

		/// <summary>
		/// Returns an object that can be used to synchronize access to the collection.
		/// </summary>
		public object SyncRoot
		{
			get
			{
				// MD 8/22/08 - Excel formula solving - Performance
				// The ancestor list will now be a generic list.
				//return ancestorList.SyncRoot;
				return null;
			}
		}
		#endregion

		#region Implementation of IEnumerable
		/// <summary>
		/// Returns the collection enumerator.
		/// </summary>
		/// <returns>Collection enumerator.</returns>
		public System.Collections.IEnumerator GetEnumerator()
		{
			return ancestorList.GetEnumerator();
		}
		#endregion

		#region AncestorMapEntry





		internal class AncestorMapEntry
		{
			public AncestorMapEntry(UCReference predecessor)
			{
				formulaPredecessor = predecessor;
				formulaAncestors = new UltraCalcReferenceCollection();
			}

			/// <summary>
			/// Get/Set the predecessor for this entry
			/// </summary>
			public UCReference Predecessor
			{
				get { return formulaPredecessor; }
				set { formulaPredecessor = value; }
			}

			/// <summary>
			/// Get/Set the ancestor collection for this entry
			/// </summary>
			public UltraCalcReferenceCollection Ancestors
			{
				get { return formulaAncestors; }
			}

			/// <summary>
			/// Equality method that returns whether an object is equal to this one
			/// </summary>
			/// <param name="obj">Object to compare to this entry</param>
			/// <returns>True if object is equal to this instance, else false</returns>
			public override bool Equals(object obj)
			{
				return obj is AncestorMapEntry && ExcelCalcEngine.CompareReferences(this.formulaPredecessor, ((AncestorMapEntry)obj).Predecessor);
			}

			/// <summary>
			/// Return hashcode for this object
			/// </summary>
			/// <returns>Integer hash code for this object</returns>
			public override int GetHashCode()
			{
				return formulaPredecessor.GetHashCode();
			}

			/// <summary>
			/// Storage for predecessor
			/// </summary>
			private UCReference formulaPredecessor;

			/// <summary>
			/// Storage for ancestor collection
			/// </summary>
			private UltraCalcReferenceCollection formulaAncestors;
		}
		#endregion //AncestorMapEntry

		// MD 11/1/10 - TFS56976
		// Exposed this so it could be used from the calc engine.
		#region AncestorDictionary

		public Dictionary<IExcelCalcReference, AncestorMapEntry> AncestorDictionary
		{
			get { return this.ancestorDictionary; }
		}

		#endregion // AncestorDictionary

		// MD 8/22/08 - Excel formula solving - Performance
		#region CellPredecessorEntries






		public Dictionary<CellCalcReference, AncestorMapEntry> CellPredecessorEntries
		{
			get { return this.cellPredecessorEntries; }
		}

		#endregion CellPredecessorEntries

		#region NonCellPredecessorEntries






		public List<AncestorMapEntry> NonCellPredecessorEntries
		{
			get { return this.nonCellPredecessorEntries; }
		}

		#endregion NonCellPredecessorEntries

		#region OnPredecessorRenamed

		public void OnPredecessorRenamed(UCReference predecessor)
		{
			AncestorMapEntry entry;
			this.ancestorDictionary.TryGetValue(predecessor, out entry);

			predecessor.RecacheName();

			if (entry != null)
				this.ancestorDictionary.Add(predecessor, entry);
		}

		#endregion // OnPredecessorRenamed

		// MD 11/1/10 - TFS56976
		// Instead of keeping regions in the linear nonCellPredecessorEntries collection, we will now use a special lookup table for them.
		#region RegionPredecessorEntries

		// MD 1/23/12 - TFS99849
		// Made the RegionLookupTable a generic collection instead of a collection of RegionCalcReferences so it could be used in other places.
		//public RegionLookupTable RegionPredecessorEntries
		// MD 2/24/12 - 12.1 - Table Support
		//public RegionLookupTable<RegionCalcReference> RegionPredecessorEntries
		// MD 3/22/12 - TFS105885
		// We now keep a separate RegionLookupTable for each worksheet.
		//public RegionLookupTable<RegionCalcReferenceBase> RegionPredecessorEntries
		public Dictionary<Worksheet, RegionLookupTable<RegionCalcReferenceBase>> RegionPredecessorEntries
		{
			get { return this.regionPredecessorEntries; }
		}

		#endregion // RegionPredecessorEntries

		#region RemoveEntry

		private void RemoveEntry(AncestorMapEntry entry)
		{
			IExcelCalcReference resolvedPrecessor = ExcelCalcEngine.GetResolvedReference(entry.Predecessor);
			this.ancestorDictionary.Remove(resolvedPrecessor);

			CellCalcReference cellPrecessor = resolvedPrecessor as CellCalcReference;

			if (cellPrecessor != null)
				this.cellPredecessorEntries.Remove(cellPrecessor);
			else
			{
				// MD 11/1/10 - TFS56976
				// Instead of keeping regions in the linear nonCellPredecessorEntries collection, we will now use a special lookup table for them.
				//this.nonCellPredecessorEntries.Remove( entry );
				// MD 2/24/12 - 12.1 - Table Support
				//RegionCalcReference regionPrecessor = resolvedPrecessor as RegionCalcReference;
				RegionCalcReferenceBase regionPrecessor = resolvedPrecessor as RegionCalcReferenceBase;
				if (regionPrecessor != null)
				{
					// MD 1/23/12 - TFS99849
					// Added a region parameter to the Remove method because Remove now takes a generic item.
					//this.regionPredecessorEntries.Remove(regionPrecessor);
					// MD 2/24/12 - 12.1 - Table Support
					//this.regionPredecessorEntries.Remove(regionPrecessor.Region, regionPrecessor);
					WorksheetRegion region = regionPrecessor.Region;
					if (region != null)
					{
						// MD 3/22/12 - TFS105885
						// We now keep a separate RegionLookupTable for each worksheet.
						//this.regionPredecessorEntries.Remove(region.Address, regionPrecessor);
						RegionLookupTable<RegionCalcReferenceBase> regionLookupTable;
						if (this.regionPredecessorEntries.TryGetValue(regionPrecessor.Worksheet, out regionLookupTable))
						{
							regionLookupTable.Remove(region, regionPrecessor);

							if (regionLookupTable.IsEmpty)
								this.regionPredecessorEntries.Remove(regionPrecessor.Worksheet);
						}
					}
				}
				else
				{
					this.nonCellPredecessorEntries.Remove(entry);
				}
			}
		}

		#endregion RemoveEntry
	}

	/// <summary>
	/// IUltraCalcReference implementation for caching the Formula of the underlying reference.
	/// </summary>
	internal class FormulaReference : IExcelCalcReference
	{
		#region Member Variables

		private IExcelCalcReference reference;
		private IExcelCalcFormula formula;

		#endregion //Member Variables

		#region Constructor
		internal FormulaReference(IExcelCalcReference reference)
		{
			this.reference = reference;
			this.formula = reference.Formula;
		}
		#endregion //Constructor

		#region IUltraCalcReference

		public bool ContainsReference(IExcelCalcReference inReference)
		{
			return this.reference.ContainsReference(inReference);
		}

		public IExcelCalcReferenceCollection References
		{
			get
			{
				return this.reference.References;
			}
		}

		public bool IsSubsetReference(IExcelCalcReference inReference)
		{
			return this.reference.IsSubsetReference(inReference);
		}

		public IExcelCalcReference CreateReference(string referenceString)
		{
			return this.reference.CreateReference(referenceString);
		}

		public object Context
		{
			get
			{
				return this.reference.Context;
			}
		}

		public ExcelCalcValue Value
		{
			get
			{
				return this.reference.Value;
			}
			set
			{
				this.reference.Value = value;
			}
		}

		public string AbsoluteName
		{
			get
			{
				return this.reference.AbsoluteName;
			}
		}

		public string NormalizedAbsoluteName
		{
			get
			{
				return this.reference.NormalizedAbsoluteName;
			}
		}

		public string ElementName
		{
			get
			{
				return this.reference.ElementName;
			}
		}

		public bool IsEnumerable
		{
			get
			{
				return this.reference.IsEnumerable;
			}
		}

		public IExcelCalcFormula Formula
		{
			get
			{
				CalcManagerUtilities.DebugWriteLineIf(this.formula != this.reference.Formula, "Formulas are different!");

				return this.formula;
			}
		}
		#endregion

		#region UnderlyingReference
		public IExcelCalcReference UnderlyingReference
		{
			get { return this.reference; }
		}
		#endregion //UnderlyingReference

		// SSP 10/12/04
		// Overrode GetHashCode and Equals methods.
		//
		#region GetHashCode

		public override int GetHashCode()
		{
			return this.reference.GetHashCode();
		}

		#endregion // GetHashCode

		#region Equals

		public override bool Equals(object obj)
		{
			return ExcelCalcEngine.CompareReferences(this.reference, obj as IExcelCalcReference);
		}

		#endregion // Equals
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