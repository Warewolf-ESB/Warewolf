using System;
using System.Collections;
using System.Diagnostics;
using System.Collections.Generic;



using UltraCalcFunction = Infragistics.Calculations.Engine.CalculationFunction;
using UltraCalcNumberStack = Infragistics.Calculations.Engine.CalculationNumberStack;
using UltraCalcValue = Infragistics.Calculations.Engine.CalculationValue;
using IUltraCalcReference = Infragistics.Calculations.Engine.ICalculationReference;
using IUltraCalcFormula = Infragistics.Calculations.Engine.ICalculationFormula;
using UltraCalcErrorValue = Infragistics.Calculations.Engine.CalculationErrorValue;
using IUltraCalcReferenceCollection = Infragistics.Calculations.Engine.ICalculationReferenceCollection;
using UltraCalcReferenceError = Infragistics.Calculations.Engine.CalculationReferenceError;
using UltraCalcException = Infragistics.Calculations.Engine.CalculationException;
using UltraCalcErrorException = Infragistics.Calculations.Engine.CalcErrorException;
using UltraCalcNumberException = Infragistics.Calculations.Engine.CalcNumberException;
using UltraCalcValueException = Infragistics.Calculations.Engine.CalculationValueException;





using Infragistics.Shared;



namespace Infragistics.Calculations.Engine





{



#region Infragistics Source Cleanup (Region)













#endregion // Infragistics Source Cleanup (Region)

	internal class UltraCalcAncestorMap : ICollection
	{
		// MD 8/22/08 - Excel formula solving - Performance
		// The ancestor list will now be a generic list.
		#region Not Used

		//#region AncestorList Class
		//
		//internal class AncestorList : ArrayList
		//{
		//    internal AncestorList( ) : base( )
		//    {
		//    }
		//
		//    internal new AncestorMapEntry this[ int index ]
		//    {
		//        get
		//        {
		//            return (AncestorMapEntry)base[ index ];
		//        }
		//    }
		//}
		//
		//#endregion // AncestorList Class 

		#endregion Not Used

		#region Member Variables

		// Storage for the ancestor collection
		// MD 8/22/08 - Excel formula solving - Performance
		// The ancestor list will now be a generic list.
		//private AncestorList ancestorList;
		private List<AncestorMapEntry> ancestorList;

		// SSP 10/7/04
		// Backward pointer to the associated calc engine.
		//
		private UltraCalcEngine calcEngine = null; 

		#endregion //Member Variables

		#region Constructor





		internal UltraCalcAncestorMap(UltraCalcEngine calcEngine)
		{
			if ( null == calcEngine )
				throw new ArgumentNullException();

			this.calcEngine = calcEngine; 

			// MD 8/22/08 - Excel formula solving - Performance
			//ancestorList = new AncestorList();
			ancestorList = new List<AncestorMapEntry>();
		}
		#endregion //Constructor

		#region Dump
		[Conditional("DEBUG")]
		public void Dump()
		{
			Debug.WriteLine("+===================================================================================================================+");
			Debug.WriteLine( string.Format( "Ancestor Map: {0} entries", ancestorList.Count ) );
			Debug.WriteLine("+-------------------------------------------------------------------------------------------------------------------+");
			foreach (AncestorMapEntry entry in ancestorList) 
				entry.Dump();
			Debug.WriteLine("+===================================================================================================================+");
		}
		#endregion //Dump

		#region Implementation of IUltraCalcAncestorMap

		public UltraCalcReferenceCollection DisposeReferences()
		{
			// Allocate a collection to hold disconnected references
			UltraCalcReferenceCollection disconnectedReferences = new UltraCalcReferenceCollection();

			// Remove disposed ancestors, and disconnect any disposed predecessors and save them in the collection of disconnected references
			int index = 0;
			while ( index < ancestorList.Count )
			{
				// Remove any disposed ancestor references
				// SSP 7/7/05
				// We also need to disconnect all the tokens of the formulas as well. Before this
				// we only disconnected the tokens referenced by predecessor entries in the
				// ancestor map.
				// 
				//ancestorList[index].DisposeReferences( );
				ancestorList[ index ].DisposeReferences( disconnectedReferences );

				// Remove the entry if there aren't any more ancestors
				if ( 0 == ancestorList[ index ].Ancestors.Count )
				{
					ancestorList.RemoveAt( index );

					// SSP 10/7/04
					// Added ancestor map version number.
					//
					this.calcEngine.BumpAncestorMapVersion();
				}
				// SSP 7/7/05
				// Related to the change above with the same tag.
				// NOTE: This following else block is not necessary since we make sure that the 
				// predecessor entry always points to a reference that's a token of one of the
				// ancestor. When an ancestor is deleted the token the predecessor entry is pointing
				// to gets replaced with a token from an existing ancestor. I'm leaving it there
				// anyways since it won't cause any problems and in case somehow a predecessor 
				// entry happens to point to a non-existant ancestor token.
				// 
				else
				{
					// Disconnect the reference if its disposed
					if ( ancestorList[ index ].Predecessor.IsDisposedReference )
					{
						bool disconnected = ancestorList[ index ].Predecessor.Disconnect();

						// If a reference got disconnected then bump ancestor map version.
						//
						if ( disconnected )
							this.calcEngine.BumpAncestorMapVersion();

						disconnectedReferences.Add( ancestorList[ index ].Predecessor );
					}
					index++;
				}
			}

			// SSP 7/7/05
			// Related to the change above with the same tag.
			// 
			if ( disconnectedReferences.Count > 0 )
				this.calcEngine.BumpAncestorMapVersion();

			return disconnectedReferences;
		} 

		/// <summary>
		/// Add a formula reference the given predecessor reference
		/// </summary>
		/// <param name="predecessor"><b>UCReference</b> predecessor reference</param>
		/// <param name="ancestor"><b>IUltraCalcReference</b> of formula that contains the given predecessor</param>
		public void AddAncestor( UCReference predecessor, IUltraCalcReference ancestor )
		{
			try 
			{
				Debug.Assert( !(ancestor is FormulaReference), "Unexpected ancestor type" );

				IUltraCalcReference ancestorRef = new FormulaReference(ancestor);

				// Check to see if predecessor is already in map
				int pos = IndexOf(predecessor);

				// Its in the map
				if (pos != -1) 
				{
					// Is the Ancestor is in Predecessor's Ancestor list 
					if ( -1 == ancestorList[pos].Ancestors.IndexOf( ancestorRef ) ) 
					{
						// Add the ancestor to the list
						ancestorList[pos].Ancestors.Add( ancestorRef );
					}
				}
				else
				{
					// Create a new Ancestor entry for this Predecessor/Ancestor
					AncestorMapEntry entry = new AncestorMapEntry( predecessor );
					entry.Ancestors.Add(ancestorRef);

					ancestorList.Add(entry);
				}
			}
			catch (Exception e) 
			{
				Debug.Assert(false, "UltraCalcAncestorMap.AddAncestor: [0]", e.ToString());
				throw new UltraCalcException( SR.GetString("Error_Internal",new object[] {"UltraCalcAncestorMap.AddAncestor"}),e );			
			}
			return;
		}

		/// <summary>
		/// Return the position of the given predecessor in the collection
		/// </summary>
		/// <param name="predecessor"></param>
		/// <returns></returns>
		private int Find(IUltraCalcReference predecessor)
		{
			for (int i = 0; i< ancestorList.Count; i++)
			{
				// SSP 9/7/04
				// Perform a case insensitive comparison since absolute names are case insensitive.
				//
				//if (String.Compare(((AncestorMapEntry)ancestorList[i]).Predecessor.AbsoluteName, predecessor.AbsoluteName) == 0)
				if ( UltraCalcEngine.CompareReferences( ancestorList[i].Predecessor, predecessor ) )
					return i;
			}
			return -1;
		}



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		internal FormulaReference FindAncestor( IUltraCalcReference reference )
		{
			foreach ( UltraCalcAncestorMap.AncestorMapEntry entry in this )
			{
				foreach ( FormulaReference ancestor in entry.Ancestors )
				{
					if ( UltraCalcEngine.CompareReferences( reference, ancestor ) )
						return ancestor;
				}
			}

			return null;
		} 


		/// <summary>
		/// Remove the given formula ancestor from the given  predecessor's list of formulas.
		/// </summary>
		/// <param name="formulaPredecessor"><b>IUltraCalcReference</b> that's referenced by given ancestor's formula</param>
		/// <param name="ancestor">Formula refernce to remove for given predecessor's list of formulas</param>
		public void DeleteAncestor(IUltraCalcReference formulaPredecessor, IUltraCalcReference ancestor)
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
					if ( (posAncestor = ancestorList[posPredecessor].Ancestors.IndexOf( ancestor ) ) != -1) 
						DeleteAncestorAt(posPredecessor, posAncestor, formulaPredecessor == ancestorList[posPredecessor].Predecessor );
				}
			}
			catch (Exception e) 
			{
				Debug.Assert(false, "UltraCalcAncestorMap.DeleteAncestor: [0]", e.ToString());
				throw new UltraCalcException( SR.GetString("Error_Internal",new object[] {"UltraCalcAncestorMap.DeleteAncestor"}),e );			
			}
			return;
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
					ancestorList.RemoveAt(posPredecessor);
				}
				else 
				{
					// If the ancestor we just deleted had a predecessor that was being used as the key to the ancestorMapEntry, replace it
					// with an instance of predecessor from another ancestor
					if (replacePredecessorInMapEntry)
					{	
						IUltraCalcReference ancestor = entry.Ancestors[0];
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
							




							if ( UltraCalcEngine.CompareReferences( entry.Predecessor, predecessor ) )
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
				throw new UltraCalcException( SR.GetString("Error_Internal",new object[] {"UltraCalcAncestorMap.DeleteAncestorAt"}),e );			
			}
			return;
		}

		/// <summary>
		/// Return the collection of ancestors of the given predecessor
		/// </summary>
		/// <param name="predecessor">Reference whose collection of formulas to return</param>
		/// <returns>Collection of references whose formulas reference the given predecessor</returns>
		public UltraCalcReferenceCollection Ancestors(IUltraCalcReference predecessor)
		{
			UltraCalcReferenceCollection outCol = null;

			for (int i = 0; i < ancestorList.Count; i++)
			{
				if (ancestorList[i].Predecessor.ContainsReference(predecessor))
				{
					if (outCol == null)
					{
						outCol = new UltraCalcReferenceCollection(ancestorList[i].Ancestors);
					}
					else
					{
						outCol.Merge(ancestorList[i].Ancestors);
					}
				}
			}

			if (outCol == null)
				return new UltraCalcReferenceCollection();
			else
				return outCol;
		}

		/// <summary>
		/// Return the index of the given predecessor
		/// </summary>
		/// <param name="predecessor">Reference whose index is to be returned</param>
		/// <returns>Index of given predecessor, or -1 if its not found</returns>
		public int IndexOf( IUltraCalcReference predecessor )
		{
			try
			{
				for ( int i = 0; i < ancestorList.Count; i++ )
					// Is the Ancestor is in Predecessor's Ancestor list
					// SSP 9/7/04
					// Perform a case insensitive comparison since absolute names are case insensitive.
					//
					//if (((AncestorMapEntry)ancestorList[i]).Predecessor.AbsoluteName == predecessor.AbsoluteName)
					if ( UltraCalcEngine.CompareReferences( ancestorList[ i ].Predecessor, predecessor ) )
						return i;
			}
			catch ( Exception e )
			{
				Debug.Assert( false, "UltraCalcAncestorMap.IndexOf: [0]", e.ToString() );
				throw new UltraCalcException( SR.GetString( "Error_Internal", new object[] { "UltraCalcAncestorMap.IndexOf" } ), e );
			}
			return -1;
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
			ancestorList.CopyTo( (AncestorMapEntry[])array, index );		
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
		/// <summary>
		/// Provides methods and properties to manage a list of formulas that reference a given reference
		/// </summary>

        internal



            class AncestorMapEntry
		{
			/// <summary>
			/// Default constructor
			/// </summary>
			public AncestorMapEntry()
			{
				formulaAncestors = new UltraCalcReferenceCollection();
			}

			/// <summary>
			/// Predecessor constructor
			/// </summary>
			/// <param name="predecessor">predecessor for this instance</param>
			public AncestorMapEntry(UCReference predecessor)
			{
				formulaPredecessor = predecessor;
				formulaAncestors = new UltraCalcReferenceCollection();
			}

			// SSP 7/7/05
			// We also need to disconnect all the tokens of the formulas as well. Before this
			// we only disconnected the tokens referenced by predecessor entries in the
			// ancestor map.
			// 
			//public void DisposeReferences()
			public void DisposeReferences( UltraCalcReferenceCollection addDisconnectedReferencesTo )
			{
				int index = 0;
				while ( index < formulaAncestors.Count )
					if ( formulaAncestors[ index ].IsDisposedReference )
						formulaAncestors.RemoveAt( index );
					else
					{
						// SSP 7/7/05
						// We also need to disconnect all the tokens of the formulas as well. Before this
						// we only disconnected the tokens referenced by predecessor entries in the
						// ancestor map.
						// 
						// ------------------------------------------------------------------------------
						IUltraCalcFormula formula = formulaAncestors[ index ].Formula;
						if ( null != formula )
						{
							foreach ( UCReference token in formula.References )
							{
								if ( token.IsDisposedReference && token.Disconnect() )
									addDisconnectedReferencesTo.Add( token );
							}
						}
						// ------------------------------------------------------------------------------

						index++;
					}
			} 

			public void Dump()
			{
				Debug.WriteLine("{0}  ==>", formulaPredecessor.AbsoluteName);
				foreach (IUltraCalcReference reference in formulaAncestors) 
					Debug.WriteLine("\t {0}", reference.AbsoluteName);
				Debug.WriteLine(" ");
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
				// SSP 10/7/04
				//
				//return (formulaPredecessor.Equals(((AncestorMapEntry)obj).Predecessor));
				return obj is AncestorMapEntry && UltraCalcEngine.CompareReferences( this.formulaPredecessor, ((AncestorMapEntry)obj).Predecessor );
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
	}

	/// <summary>
	/// IUltraCalcReference implementation for caching the Formula of the underlying reference.
	/// </summary>
	internal class FormulaReference : IUltraCalcReference
	{
		#region Member Variables

		private IUltraCalcReference		reference;
		private IUltraCalcFormula		formula;

		#endregion //Member Variables

		#region Constructor
		internal FormulaReference(IUltraCalcReference reference)
		{
			this.reference = reference;
			this.formula = reference.Formula;
		}
		#endregion //Constructor

		#region IUltraCalcReference

		public bool IsSiblingReference(IUltraCalcReference reference)
		{
			return this.reference.IsSiblingReference(reference);
		}

		public void MarkRelativeIndices( IUltraCalcReference inReference )
		{
			this.reference.MarkRelativeIndices( inReference );
		} 

		public IUltraCalcReference ResolveReference(IUltraCalcReference reference, ResolveReferenceType referenceType)
		{
			return this.reference.ResolveReference(reference,referenceType);
		}

		public bool ContainsReference(IUltraCalcReference inReference)
		{
			return this.reference.ContainsReference(inReference);
		}

		public IUltraCalcReferenceCollection References
		{
			get
			{
				return this.reference.References;
			}
		}

		public bool IsSubsetReference(IUltraCalcReference inReference)
		{
			return this.reference.IsSubsetReference(inReference);
		}

		public IUltraCalcReference CreateRange(string fromReference, string toReference)
		{
			return this.reference.CreateRange(fromReference,toReference);
		}

		public IUltraCalcReference[] GetChildReferences( ChildReferenceType referenceType )
		{
			return this.reference.GetChildReferences( referenceType );
		} 

		public IUltraCalcReference CreateReference(string referenceString)
		{
			return this.reference.CreateReference(referenceString);
		}

		public bool IsDisposedReference
		{
			get
			{
				return this.reference.IsDisposedReference;
			}
		}

		public bool HasRelativeIndex
		{
			get
			{
				return this.reference.HasRelativeIndex;
			}
		} 

		public object Context
		{
			get
			{
				return this.reference.Context;
			}
		}

		public bool HasAbsoluteIndex
		{
			get
			{
				return this.reference.HasAbsoluteIndex;
			}
		} 

		public UltraCalcValue Value
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

		public bool HasScopeAll
		{
			get
			{
				return this.reference.HasScopeAll;
			}
		}

		public bool RecalcDeferred
		{
			get
			{
				return this.reference.RecalcDeferred;
			}
			set
			{
				this.reference.RecalcDeferred = value;
			}
		} 

		public bool IsDataReference
		{
			get
			{
				return this.reference.IsDataReference;
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

		public bool RecalcVisible
		{
			get
			{
				return this.reference.RecalcVisible;
			}
			set
			{
				this.reference.RecalcVisible = value;
			}
		} 

		public bool IsEnumerable
		{
			get
			{
				return this.reference.IsEnumerable;
			}
		}

		public IUltraCalcReference Parent
		{
			get
			{
				return this.reference.Parent;
			}
		} 

		public IUltraCalcFormula Formula
		{
			get
			{
                CalcManagerUtilities.DebugWriteLineIf(this.formula != this.reference.Formula, "Formulas are different!");

				return this.formula;
			}
		}
		#endregion
		
		#region UnderlyingReference
		public IUltraCalcReference UnderlyingReference
		{
			get { return this.reference; }
		}
		#endregion //UnderlyingReference

		// SSP 10/12/04
		// Overrode GetHashCode and Equals methods.
		//
		#region GetHashCode

		public override int GetHashCode( )
		{
			return this.reference.GetHashCode( );
		}

		#endregion // GetHashCode

		#region Equals

		public override bool Equals( object obj )
		{
			return UltraCalcEngine.CompareReferences( this.reference, obj as IUltraCalcReference );
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