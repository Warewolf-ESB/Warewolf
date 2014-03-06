using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;









namespace Infragistics.Windows.Helpers

{

	#region HashSet Class

	
	
	
	/// <summary>
	/// Data structure for use as a set.
	/// </summary>
	public class HashSet : ICollection
	{
		#region Private Vars

        
        
		

		private Hashtable _table;

		#endregion // Private Vars

		#region Constructor

		/// <summary>
		/// Constructor.
		/// </summary>
		public HashSet( )
		{
			this._table = new Hashtable( );
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		public HashSet( int initialCapacity )
		{
			this._table = new Hashtable( initialCapacity );
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		public HashSet( int initialCapacity, float loadFactor )
		{
			this._table = new Hashtable( initialCapacity, loadFactor );
		}

		// SSP 11/7/08  TFS9486
		// 
		/// <summary>
		/// Constructor.
		/// </summary>
		public HashSet( int initialCapacity, float loadFactor, IEqualityComparer equalityComparer )
		{
			this._table = new Hashtable( initialCapacity, loadFactor, equalityComparer );
		}

		#endregion // Constructor

		#region Methods

		#region Public Methods

		#region Add

		/// <summary>
		/// Adds the item to the set. If the item already exists in the set, does nothing.
		/// </summary>
		/// <param name="item"></param>
		public void Add( object item )
		{
            
            // Initialize the value to the item so we can return it from GetEquivalentItem
            
            this._table[item] = item;
		}

		#endregion // Add

		#region AddItems

		/// <summary>
		/// Adds items from the specified set to this set.
		/// </summary>
		/// <param name="items"></param>
		public void AddItems( IEnumerable items )
		{
			foreach ( object ii in items )
				this.Add( ii );
		}

		/// <summary>
		/// Adds items from the specified set to this set.
		/// </summary>
		/// <param name="source"></param>
		public void AddItems( HashSet source )
		{
			foreach ( object item in source )
				this.Add( item );
		}

		#endregion // AddItems

		#region AreEqual

		/// <summary>
		/// Returns true if both sets are equal. They have the same items.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public static bool AreEqual( HashSet x, HashSet y )
		{
			if ( x.Count != y.Count )
				return false;

			return x.IsSubsetOf( y );
		}

		#endregion // AreEqual

		#region Clear

		/// <summary>
		/// Clears the set.
		/// </summary>
		public void Clear( )
		{
			this._table.Clear( );
		}

		#endregion // Clear

		#region Contains
		internal bool Contains(object item)
		{
			return this.Exists(item);
		} 
		#endregion // Contains

		#region CopyTo

		/// <summary>
		/// Copies all the elements of this set to the spcified array starting at the specified index in the array.
		/// </summary>
		/// <param name="array"></param>
		/// <param name="arrayIndex"></param>
		public void CopyTo( System.Array array, int arrayIndex )
		{
			this._table.Keys.CopyTo( array, arrayIndex );
		}

		#endregion // CopyTo

		#region DoesIntersect

		/// <summary>
		/// Returns true of the specified set and this set intersect.
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		public bool DoesIntersect( HashSet s )
		{
			if ( this.Count > s.Count )
				return s.DoesIntersect( this );

			foreach ( object item in this )
			{
				if ( s.Exists( item ) )
					return true;
			}

			return false;
		}

		#endregion // DoesIntersect

		#region Exists

		/// <summary>
		/// Returns true if the specified item exists in this set.
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public bool Exists( object item )
		{
			return this._table.ContainsKey( item );
		}

		#endregion // Exists

		#region GetEnumerator

		/// <summary>
		/// Returns a new enumerator that enumerates all the elements of this set.
		/// </summary>
		/// <returns></returns>
		public IEnumerator GetEnumerator( )
		{
			return this._table.Keys.GetEnumerator( );
		}

		#endregion // GetEnumerator

        // JJD 5/13/09 - NA 2009 vol 2 - added
        #region GetEquivalentItem

        /// <summary>
        /// Returns the item in the hash that has the equivalent key to the padded in item
        /// </summary>
        /// <param name="item">The item in question</param>
        /// <returns>The item from the hash with the equivalent key</returns>
        public object GetEquivalentItem(object item)
        {
            return this._table[item];
        }

        #endregion //GetEquivalentItem	
    
		#region GetIntersection

		/// <summary>
		/// Calculates the intersection of the specified sets.
		/// </summary>
		/// <param name="set1"></param>
		/// <param name="set2"></param>
		/// <returns></returns>
		public static HashSet GetIntersection( HashSet set1, HashSet set2 )
		{
			HashSet result = new HashSet( );

			if ( set1.Count > set2.Count )
				return GetIntersection( set2, set1 );

			foreach ( object item in set1 )
			{
				if ( set2.Exists( item ) )
					result.Add( item );
			}

			return result;
		}

		#endregion // GetIntersection

		#region GetUnion

		/// <summary>
		/// Calculates the union of the specified sets.
		/// </summary>
		/// <param name="set1"></param>
		/// <param name="set2"></param>
		/// <returns></returns>
		public static HashSet GetUnion( HashSet set1, HashSet set2 )
		{
			HashSet result = new HashSet( );
			result.AddItems( set1 );
			result.AddItems( set2 );

			return result;
		}

		#endregion // GetUnion

		#region IsSubsetOf

		/// <summary>
		/// Returns true if this set is a subset of the specified set.
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		public bool IsSubsetOf( HashSet s )
		{
			if ( this.Count > s.Count )
				return false;

			foreach ( object item in this )
			{
				if ( !s.Exists( item ) )
					return false;
			}

			return true;
		}

		#endregion // IsSubsetOf

		#region Remove

		/// <summary>
		/// Removes the specified item from the set. If the item doesn't exist in the set
		/// does nothing.
		/// </summary>
		/// <param name="item"></param>
		public void Remove( object item )
		{
			this._table.Remove( item );
		}

		#endregion // Remove

		#region ToArray

		/// <summary>
		/// Returns an array containing all the elements of this set.
		/// </summary>
		/// <returns></returns>
		public T[] ToArray<T>( )
		{
			T[] arr = new T[this.Count];
			int i = 0;

			foreach ( T item in this )
				arr[i++] = item;

			return arr;
		}

		#endregion // ToArray

		#endregion // Public Methods

		#endregion // Methods

		#region Properties

		#region Public Properties

		#region Count

		/// <summary>
		/// Returns the number of items contained in the set.
		/// </summary>
		public int Count
		{
			get
			{
				return this._table.Count;
			}
		}

		#endregion // Count

		#region IsEmpty

		/// <summary>
		/// Returns true if the set is empty, that is it has no elements.
		/// </summary>
		public bool IsEmpty
		{
			get
			{
				return 0 == this.Count;
			}
		}

		#endregion // IsEmpty

		#region IsSynchronized

		/// <summary>
		/// Indicates whether this data structure is synchronized.
		/// </summary>
		public bool IsSynchronized
		{
			get
			{
				return this._table.IsSynchronized;
			}
		}

		#endregion // IsSynchronized

		#region SyncRoot

		/// <summary>
		/// Returns the object that can be used to synchronize the access to this data structure.
		/// </summary>
		public object SyncRoot
		{
			get
			{
				return this._table.SyncRoot;
			}
		}

		#endregion // SyncRoot

		#endregion // Public Properties

		#endregion // Properties
	}

	#endregion // HashSet Class

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