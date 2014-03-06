using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;




using Infragistics.Shared;


namespace Infragistics.Documents.Excel
{
	/// <summary>
	/// Abstract base class for collections of the main worksheet elements (rows, columns, and cells).
	/// </summary>
	/// <remarks>
	/// <p class="body">
	/// Because of the large number of elements on a worksheet, this collection internally creates elements
	/// when they are requested. Iterating this collection will only iterate the elements which have already
	/// been created.
	/// </p>
	/// </remarks>
	/// <typeparam name="T">The type of item contained in the collection.</typeparam>



	public

		 abstract class WorksheetItemCollection<T> : 
		ICollection<T>,
		ILoadOnDemandTreeOwner<T>
	{
		#region Member Variables

		private LoadOnDemandTree<T> items;

		// MD 7/2/09 - TFS18634
		// Added the member variable back in so we can cache the value and not call the virtual 
		// property all the time.
		// MD 6/31/08 - Excel 2007 Format
		// The maximum is now dynamic
		//private int maxCount;
		private int maxCount;

		private Worksheet worksheet;

        // MBS 7/24/08 - Excel 2007 Format
        private int lastLoadedIndex = -1;

		#endregion Member Variables

		#region Constructor

		// MD 6/31/08 - Excel 2007 Format
		//internal WorksheetItemCollection( Worksheet worksheet, int maxCount )
		internal WorksheetItemCollection( Worksheet worksheet )
		{
			// MD 7/23/10 - TFS35969
			// We now create this from a virtual method which is called below.
			//this.items = new LoadOnDemandTree<T>( this );

			// MD 6/31/08 - Excel 2007 Format
			//this.maxCount = maxCount;

			this.worksheet = worksheet;

			// MD 7/2/09 - TFS18634
			// Cache the max count
			this.maxCount = this.MaxCount;

			// MD 7/23/10 - TFS35969
			
			
			this.items = this.CreateLoadOnDemandTree();
		}

		#endregion Constructor

		#region Interfaces

		#region ICollection<T> Members

		void ICollection<T>.Add( T item )
		{
			throw new InvalidOperationException( SR.GetString( "LE_InvalidOperationException_CantModifyCollection" ) );
		}

		void ICollection<T>.Clear()
		{
			throw new InvalidOperationException( SR.GetString( "LE_InvalidOperationException_CantModifyCollection" ) );
		}

		bool ICollection<T>.Contains( T item )
		{
			// MD 4/12/11 - TFS67084
			// The items collection can now be null.
			if (this.items == null)
				return false;

			return ( (ICollection<T>)this.items ).Contains( item );
		}

		void ICollection<T>.CopyTo( T[] array, int arrayIndex )
		{
			// MD 4/12/11 - TFS67084
			// The items collection can now be null.
			if (this.items == null)
				return;

			( (ICollection<T>)this.items ).CopyTo( array, arrayIndex );
		}

		int ICollection<T>.Count
		{
			get { return this.Count; }
		}

		bool ICollection<T>.IsReadOnly
		{
			get { return true; }
		}

		bool ICollection<T>.Remove( T item )
		{
			throw new InvalidOperationException( SR.GetString( "LE_InvalidOperationException_CantModifyCollection" ) );
		}

		#endregion

		#region IEnumerable<T> Members

		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			// MD 4/12/11 - TFS67084
			// Moved all code to a virtual helper method.
			//return ( (IEnumerable<T>)this.items ).GetEnumerator();
			return this.GetEnumeratorHelper();
		}

		#endregion

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ( (IEnumerable<T>)this ).GetEnumerator();
		}

		#endregion

		#region ILoadOnDemandTreeOwner<T> Members

		T ILoadOnDemandTreeOwner<T>.CreateValue( int index )
		{
			return this.CreateValue( index );
		}

		#endregion

		#endregion Interfaces

		#region Methods

		#region Abstract Methods

		internal abstract T CreateValue( int index );
		internal abstract void ThrowIndexOutOfRangeException( int index );

		// MD 7/2/08 - Excel 2007 Format
		internal abstract void VerifyFormatLimits( FormatLimitErrors limitErrors, WorkbookFormat testFormat );

		#endregion Abstract Methods

		#region Internal Methods

		#region CreateLoadOnDemandTree

		internal virtual LoadOnDemandTree<T> CreateLoadOnDemandTree()
		{
			return new LoadOnDemandTree<T>(this);
		}

		#endregion // CreateLoadOnDemandTree

		#region GetAllItemsEnumerator







		internal IEnumerable<T> GetAllItemsEnumerator()
		{
			// MD 4/12/11 - TFS67084
			// The items collection can now be null.
			if (this.items == null)
			{
				Utilities.DebugFail("This collection should not be iterated.");
				yield break;
			}

			// MD 7/2/09 - TFS18634
			// Use the member variable instead of the virtual property.
			//// MD 6/31/08 - Excel 2007 Format
			////for ( int i = 0; i < this.maxCount; i++ )
			//int maxCount = this.MaxCount;
			//
			//for ( int i = 0; i < maxCount; i++ )
			for ( int i = 0; i < this.maxCount; i++ )
				yield return this.items[ i ];
		}

		#endregion GetAllItemsEnumerator

		// MD 4/12/11 - TFS67084
		#region GetEnumeratorHelper

		internal virtual IEnumerator<T> GetEnumeratorHelper()
		{
			if (this.items == null)
				return this.GetAllItemsEnumerator().GetEnumerator();

			return ((IEnumerable<T>)this.items).GetEnumerator();
		}

		// MD 2/16/12 - 12.1 - Table Support
		// Renamed this for clarity and added a new overload.
		//// MD 12/1/11 - TFS96113
		//internal virtual IEnumerable<T> GetEnumeratorHelper(int startIndex, int endIndex)
		//{
		//    if (this.items == null)
		//        return this.GetAllItemsEnumerator();
		//
		//    return this.items.GetEnumeratorHelper(startIndex, endIndex);
		//}
		internal IEnumerable<T> GetItemsInRange(int startIndex, int endIndex)
		{
			return this.GetItemsInRange(startIndex, endIndex, true);
		}

		internal virtual IEnumerable<T> GetItemsInRange(int startIndex, int endIndex, bool enumerateForwards)
		{
			if (this.items == null)
				return this.GetAllItemsEnumerator();

			return this.items.GetEnumeratorHelper(startIndex, endIndex, enumerateForwards);
		}

		#endregion  // GetEnumeratorHelper

		// MD 3/15/12 - TFS104581
		// Moved this to the WorksheetRowCollection because that is the only collection where it should be used now 
		// and we don't want to inadvertently use it for columns or cells in the future.
		#region Moved

		//        #region GetIfCreated

		//#if DEBUG
		//        /// <summary>
		//        /// Requests an item at the specified index, but doesn't create the item if it doesn't exist.
		//        /// </summary>  
		//#endif
		//        internal T GetIfCreated( int index )
		//        {
		//            // MD 4/12/11 - TFS67084
		//            // The items collection can now be null.
		//            if (this.items == null)
		//            {
		//                Utilities.DebugFail("GetIfCreated should not be called on this collection.");
		//                return default(T);
		//            }

		//            T item;
		//            if ( this.items.TryGetValue( index, out item ) )
		//                return item;

		//            return default( T );
		//        }

		//        #endregion GetIfCreated

		#endregion // Moved

		// MD 4/18/11 - TFS62026
		// This is no longer used.
		#region Removed

		// MD 10/20/10 - TFS36617
		//        #region GetItem

		//#if DEBUG
		//        /// <summary>
		//        /// Requests an item at the specified index and indicates whether it existed previously or whether is was 
		//        /// created by the call.
		//        /// </summary>  
		//#endif
		//        internal T GetItem(int index, out bool wasCreated)
		//        {
		//            // MD 4/12/11 - TFS67084
		//            // The items collection can now be null.
		//            if (this.items == null)
		//            {
		//                wasCreated = true;
		//                return this.CreateValue(index);
		//            }

		//            LoadOnDemandTree<T>.FindState state;
		//            T item = this.items.GetValue(index, true, out state);
		//            wasCreated = (state == LoadOnDemandTree<T>.FindState.ValueInserted);
		//            return item;
		//        } 

		//        #endregion // GetItem 

		#endregion // Removed

		#region InternalIndexer

		internal T InternalIndexer( int index )
		{
			if ( index < 0 )
				throw new ArgumentOutOfRangeException( "index", index, SR.GetString( "LE_ArgumentOutOfRangeException_IndexNegative" ) );

			// MD 7/2/09 - TFS18634
			// Use the member variable instead of the virtual property.
			//// MD 6/31/08 - Excel 2007 Format
			////if ( index >= this.maxCount )
			//if ( index >= this.MaxCount )
			if ( index >= this.maxCount )
				this.ThrowIndexOutOfRangeException( index );

			return this.items[ index ];
		}

		#endregion InternalIndexer

		// MD 7/2/09 - TFS18634
		#region OnCurrentFormatChanged

		internal virtual void OnCurrentFormatChanged()
		{
			// Re-cache the max count for the collection when the format has changed.
			this.maxCount = this.MaxCount;
		}

		#endregion OnCurrentFormatChanged

        #endregion Internal Methods

        #endregion Methods

        #region Properties

        #region Abstract Properties

        // MD 6/31/08 - Excel 2007 Format
		/// <summary>
		/// Gets the maximum number of items allowed in this collection.
		/// </summary> 
		// MD 2/1/11 - Page Break support
		//protected abstract int MaxCount { get; }
		protected internal abstract int MaxCount { get; }

		#endregion Abstract Properties

		#region Internal Properties

		#region Count

		// MD 4/19/11 - TFS73111
		// Made virtual
		//internal int Count
		internal virtual int Count
		{
			// MD 4/12/11 - TFS67084
			// The items collection can now be null.
			//get { return this.items.Count; }
			get 
			{
				if (this.items == null)
					return 0;

				return this.items.Count; 
			}
		}

		#endregion Count

		// MD 3/15/12 - TFS104581
		// Exposed this internally so the WorksheetRowCollection.GetIfCreated method could use it.
		#region ItemsInternal

		internal LoadOnDemandTree<T> ItemsInternal
		{
			get { return this.items; }
		}

		#endregion // ItemsInternal

        // 7/24/08 - Excel 2007 Format
        #region LastLoadedIndex

        internal int LastLoadedIndex
        {
            get { return this.lastLoadedIndex; }
            set { this.lastLoadedIndex = value; }
        }
        #endregion //LastLoadedIndex

        #region Worksheet

        internal Worksheet Worksheet
		{
			get { return this.worksheet; }
		}

		#endregion Worksheet

		#endregion Internal Properties

		#endregion Properties
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