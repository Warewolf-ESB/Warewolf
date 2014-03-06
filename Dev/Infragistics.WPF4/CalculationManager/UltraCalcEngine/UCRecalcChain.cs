using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;


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
using Infragistics.Collections;



namespace Infragistics.Calculations.Engine





{



#region Infragistics Source Cleanup (Region)
























#endregion // Infragistics Source Cleanup (Region)

	internal class UCRecalcChain : IUltraCalcReferenceCollection
	{
		#region HSparseArray Class
		






		private class HSparseArray
        
                    : SparseArray
        




		{
            private Dictionary<object, object> table = null;

			internal HSparseArray( ) : base( 20, 1.0f, true )
			{
                this.table = new Dictionary<object, object>();
			}

			protected override object GetOwnerData( object item )
			{
                if (this.table.ContainsKey(item))
                    return table[item];
                else
                    return null;
			}

			protected override void SetOwnerData( object item, object ownerData )
			{
				if ( null != ownerData )
					this.table[ item ] = ownerData;
				else if ( this.table.ContainsKey( item ) )
					this.table.Remove( item );
			}

			internal new IUltraCalcReference this[ int index ]
			{
				get
				{
					return (IUltraCalcReference)this.GetItem( index );
				}
			}
		}

		#endregion // HSparseArray Class

		#region Private Vars
		
		// MD 8/27/08 - Code Analysis - Performance
		//private UltraCalcEngine calcEngine = null;
		//private HSparseArray recalcChain = null;
		//private int dirtyChainStartIndex = 0;
		//private int recalcChainVersion = 0;
		private UltraCalcEngine calcEngine; 
		private HSparseArray recalcChain;
		private int dirtyChainStartIndex;
		private int recalcChainVersion;

		#endregion // Private Vars

		#region Constructor
		





		internal UCRecalcChain(UltraCalcEngine calcEngine)
		{
			this.calcEngine = calcEngine; 
			this.recalcChain = new HSparseArray( );
		}

		#endregion // Constructor

		#region CalcEngine






		internal UltraCalcEngine CalcEngine
		{
			get
			{
				return this.calcEngine;
			}
		}

		#endregion // CalcEngine 

		#region DirtyChainStartIndex
		


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		internal int DirtyChainStartIndex
		{
			get
			{
				return this.dirtyChainStartIndex;
			}
		}

		#endregion // DirtyChainStartIndex

		#region DirtyChainItemCount






		internal int DirtyChainItemCount
		{
			get
			{
				return this.Count - this.DirtyChainStartIndex;
			}
		}

		#endregion // DirtyChainItemCount 

		#region HasDirtyItems
		





		internal bool HasDirtyItems
		{
			get
			{
				return this.dirtyChainStartIndex < this.Count;
			}
		}

		#endregion // HasDirtyItems

		#region ResetDirtyChainStartIndex
		





		internal void ResetDirtyChainStartIndex( )
		{
			this.dirtyChainStartIndex = this.Count;
		}

		#endregion // ResetDirtyChainStartIndex

		#region RecalcChainVersion







		internal int RecalcChainVersion
		{
			get
			{
				return this.recalcChainVersion;
			}
		}

		#endregion // RecalcChainVersion 

		#region BumpRecalcChainVersion
		






		private void BumpRecalcChainVersion( )
		{
			this.recalcChainVersion++;
		}

		#endregion // BumpRecalcChainVersion

		#region Indexer
		





		internal IUltraCalcReference this[ int index ]
		{
			get 
			{ 
				return this.recalcChain[ index ]; 
			}
		}

		#endregion // Indexer

		#region Add
		






		internal bool Add( IUltraCalcReference reference )
		{
			if ( ! ( reference is UltraCalcReferenceError ) && ! this.Contains( reference ) )
			{
				Debug.Assert( ! ( reference is UCReference ), "An UCReference should not be added to the recalc chain." );
				this.recalcChain.Add( reference );

				this.BumpRecalcChainVersion( );
				return true;
			}

			return false;
		}

		#endregion // Add

		#region IndexOf
		


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		internal int IndexOf( IUltraCalcReference reference )
		{
			return this.recalcChain.IndexOf( reference );
		}

		#endregion // IndexOf

		#region Contains



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		internal bool Contains( IUltraCalcReference reference )
		{
			return this.IndexOf(reference) >= 0;
		}

		#endregion // Contains

		#region IsRecalcListed

		internal bool IsRecalcListed( IUltraCalcReference reference )
		{
			return this.Contains( reference );
		}

		#endregion // IsRecalcListed

		#region IsRecalcDirty

		internal bool IsRecalcDirty( IUltraCalcReference reference )
		{
			return this.IndexOf( reference ) >= this.dirtyChainStartIndex;
		}

		#endregion // IsRecalcDirty

		#region RemoveAt







		internal void RemoveAt( int index )
		{
			Debug.Assert( index != -1 && index < this.recalcChain.Count );

			this.recalcChain.RemoveAt( index );

			if ( index < this.dirtyChainStartIndex )
				this.dirtyChainStartIndex--;

			this.BumpRecalcChainVersion( );
		}

		#endregion // RemoveAt

		#region RemoveRange



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		internal void RemoveRange( int index, int count )
		{
			for ( int i = 0; i < count; i++ )
				this.RemoveAt( index );
		}

		#endregion // RemoveRange 

		#region Remove







		internal void Remove( IUltraCalcReference reference )
		{
			int index = this.IndexOf( reference );
			if ( index >= 0 )
				this.RemoveAt( index );
		}

		#endregion // Remove

		#region RemoveDisposedReferences






		internal void RemoveDisposedReferences()
		{
			for ( int i = this.Count - 1; i >= 0; i-- )
			{
				if ( this[ i ].IsDisposedReference )
					this.RemoveAt( i );
			}
		}

		#endregion // RemoveDisposedReferences 

		#region Sort

		// SSP 9/27/04 - Circular Relative Index Support
		// Added Sort method.
		//






		internal void Sort( IComparer comparer )
		{
			this.recalcChain.Sort( comparer );
		}

		#endregion // Sort

		#region ContainsSubsetOf



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		internal bool ContainsSubsetOf( IUltraCalcReference reference )
		{
			foreach ( IUltraCalcReference r in this.recalcChain )
			{
				if ( reference.IsSubsetReference( r ) )
					return true;
			}

			return false;
		}

		#endregion // ContainsSubsetOf 

		#region ContainsSupersetOf
		


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		internal bool ContainsSupersetOf( IUltraCalcReference reference )
		{
			foreach ( IUltraCalcReference r in this.recalcChain )
			{
				if ( r.IsSubsetReference( reference ) )
					return true;
			}

			return false;
		}

		#endregion // ContainsSupersetOf

		#region RemoveSubsetReferences
		


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		internal void RemoveSubsetReferences( IUltraCalcReference reference )
		{
			for ( int i = this.Count - 1; i >= 0; i-- )
			{
				if ( reference.IsSubsetReference( this[i] ) )
					this.RemoveAt( i );
			}
		}

		#endregion // RemoveSubsetReferences

		#region Dump
		[Conditional("DEBUG")]
		public void Dump()
		{
			Debug.WriteLine("+===================================================================================================================+");
			Debug.WriteLine( string.Format( "Recalc Chain: {0} entries, Dirty Chain End={1}", recalcChain.Count, this.DirtyChainStartIndex ) );
			Debug.WriteLine("+-------------------------------------------------------------------------------------------------------------------+");
			foreach (IUltraCalcReference reference in recalcChain) 
				Debug.WriteLine( string.Format( "{0} = {1} | {2} {3}", reference.AbsoluteName, reference.Formula.FormulaString, this.IsRecalcListed(reference) ? "Listed" : " ", this.IsRecalcDirty(reference) ? "Dirty" : " " ) );
			Debug.WriteLine("+===================================================================================================================+");
		}
		#endregion //Dump

		#region Implementation of ICollection

		/// <summary>
		/// Copys the collection to an array.
		/// </summary>
		/// <param name="array">Array used for the desitnation of the copy.</param>
		/// <param name="index">The zero-based index in array at which copying begins.</param>
		public void CopyTo(System.Array array, int index)
		{
			recalcChain.CopyTo(array,index);
		}

		/// <summary>
		/// Denotes whether the collection is thread-safe.
		/// </summary>
		public bool IsSynchronized
		{
			get
			{
			    return recalcChain.IsSynchronized;
			}
		}

		/// <summary>
		/// Returns a number of elements in the collection.
		/// </summary>
		public int Count
		{
			get
			{
				return recalcChain.Count;
			}
		}

		/// <summary>
		/// Returns an object that can be used to synchronize access to the collection.
		/// </summary>
		public object SyncRoot
		{
			get
			{
				return recalcChain.SyncRoot;
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
			return recalcChain.GetEnumerator();
		}

		#endregion
	}

	
#region Infragistics Source Cleanup (Region)




















































































































































































































































































































































































































#endregion // Infragistics Source Cleanup (Region)


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