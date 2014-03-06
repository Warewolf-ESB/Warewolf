using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Collections;
using System.Diagnostics;
using System.ComponentModel;

// SSP 4/14/04 
// Created SparseArray class for use with UltraDataSource as well as UltraGrid. UltraDataSource
// makes use of this data structure to maintain its rows collection. UltraGrid uses it to maintain
// its rows collection. This structure also has the functionality for managing scroll and visible
// indeces and counts which the UltraGrid makes use of.
// Internally sparse array uses a balanced binary tree to maintain the necessary information.
//



using Infragistics.Services;

namespace Infragistics.Collections.Services



{

	#region ISparseArrayItem Interface

	/// <summary>
	/// This interface can be implemented on items to be contained in the sparse 
	/// array to get a fast IndexOf operation.
	/// </summary>
	public interface ISparseArrayItem
	{
		/// <summary>
		/// Gets the owner data of the item.
		/// </summary>
		/// <param name="context">The sparse array context.</param>
		/// <remarks>
		/// <p class="body"><b>GetOwnerData</b> and <see cref="SetOwnerData"/> methods are used by the <see cref="SparseArray"/> implementation to maintain a transparent piece of data. It uses this data to provide efficient IndexOf operation.</p>
		/// </remarks>
		/// <returns></returns>
		object GetOwnerData( SparseArray context );


		/// <summary>
		/// Sets the owner data of the item.
		/// </summary>
		/// <param name="ownerData"></param>
		/// <param name="context">The sparse array context.</param>
		/// <remarks>
		/// <p class="body"><see cref="GetOwnerData"/> and <b>SetOwnerData</b> methods are used by the <see cref="SparseArray"/> implementation to maintain a transparent piece of data. It uses this data to provide efficient IndexOf operation.</p>
		/// </remarks>
		void SetOwnerData( object ownerData, SparseArray context );
	}

	#endregion // ISparseArrayItem Interface

	#region ISparseArrayMultiItem

	/// <summary>
	/// ISparseArrayMultiItem interface.
	/// </summary>
	public interface ISparseArrayMultiItem : ISparseArrayItem
	{
		/// <summary>
		/// Gets the scroll count of this item. If the scroll count is 0 then item 
		/// is considered hidden otherwise it's considered visible. Scroll count must 
		/// be a non-negative number.
		/// </summary>
		int ScrollCount
		{
			get;
		}

		/// <summary>
		/// Returns a descendant item at the specified index. The returned item 
		/// doesn't necessarily have to be an immediate child.
		/// </summary>
		/// <param name="scrollIndex"></param>
		/// <returns></returns>
		object GetItemAtScrollIndex( int scrollIndex );
	}

	#endregion // ISparseArrayMultiItem

	#region ICreateItemCallback

	/// <summary>
	/// ICreateItemCallback interface.
	/// </summary>
	public interface ICreateItemCallback
	{
		/// <summary>
		/// Returns a new item to be assigned to a location in the array. CreateItem should not 
		/// set the new item at the location in the array. Array and relativeIndex are provided 
		/// for information only. Also the implementation should not modify array in any way 
		/// otherwise results will be undefined.
		/// </summary>
		object CreateItem( SparseArray array, int relativeIndex );
	}

	#endregion // ICreateItemCallback

	#region SparseArray Class

	/// <summary>
	/// SparseArray class.
	/// </summary>
	public class SparseArray : System.Collections.IList
	{
		#region Class Definitions

		#region Node Class Definition

		private class Node
		{
			internal Node parent, left, right;
			internal int leftCount, rightCount, count;
			internal object[] arr;

			internal Node( )
			{
			}

			internal Node( Node parent )
			{
				this.parent = parent;
			}

			internal int TotalCount
			{
				get
				{
					return this.leftCount + this.rightCount + this.count;
				}
			}

			internal object[] ItemsArray
			{
				get
				{
					return null != this.arr ? this.arr : this.arr = new object[ this.count ];
				}
			}

			
#region Infragistics Source Cleanup (Region)



























#endregion // Infragistics Source Cleanup (Region)


			public override string ToString( )
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder( );
				sb.Append( this.GetType( ).Name ).Append( " " ).Append( this.GetHashCode( ) );
				sb.Append( " " ).Append( null != parent 
					? ( this == parent.left 
					? "lc" : ( this == parent.right ? "rc" : "invalid" ) ) : "no parent" );

				return sb.ToString( );
			}

			
#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)
			  
		}

		private class NodeExtended : Node
		{
			internal int visibleCount;
			internal int leftVisibleCount;
			internal int rightVisibleCount;

			internal int scrollCount;
			internal int leftScrollCount;
			internal int rightScrollCount;

			internal bool isNodeDirty, isLeftDirty, isRightDirty;

			internal NodeExtended( ) : base( )
			{
			}

			internal NodeExtended( NodeExtended parent ) : base( parent )
			{
			}

			internal int TotalVisibleCount
			{
				get
				{
					return this.leftVisibleCount + this.visibleCount + this.rightVisibleCount;
				}
			}

			internal int TotalScrollCount
			{
				get
				{
					return this.leftScrollCount + this.scrollCount + this.rightScrollCount;
				}
			}

			// SSP 2/28/10 - Optimizations
			// 
			internal virtual int[] ScrollCounts
			{
				get
				{
					return null;
				}
				set
				{
				}
			}
		}

		// SSP 2/28/10 - Optimizations
		// Added NodeExtendedWithScrollCountCache.
		// 
		private class NodeExtendedWithScrollCountCache : NodeExtended
		{
			internal static int[] g_dirtyFlag = new int[0];
			private int[] _scrollCounts = g_dirtyFlag;

			internal NodeExtendedWithScrollCountCache( ) : base( )
			{
			}

			internal NodeExtendedWithScrollCountCache( NodeExtended parent )
				: base( parent )
			{
			}

			internal override int[] ScrollCounts
			{
				get
				{
					return _scrollCounts;
				}
				set
				{
					_scrollCounts = value;
				}
			}
		}

		#endregion // Node Class Definition

		#region Enumerator Class Definition

		private class Enumerator : System.Collections.IEnumerator
		{
			private SparseArray sarr;
			private int nodeIndex = -1;
			private Node currNode;

			internal Enumerator( SparseArray sarr )
			{
				this.sarr = sarr;
				((System.Collections.IEnumerator)this).Reset( );
			}

			private void EnsureNotExhausted( )
			{
				if ( null == this.currNode )
					throw new InvalidOperationException( SparseArray.GetString( "LE_InvalidOperationException_1" ) );
			}

			void System.Collections.IEnumerator.Reset( )
			{
				this.currNode = SparseArray.GetMinimum(sarr.tree);
				this.nodeIndex = -this.currNode.leftCount - 1;
			}

			bool System.Collections.IEnumerator.MoveNext( )
			{
				this.EnsureNotExhausted( );

				nodeIndex++;

				if ( nodeIndex < currNode.count )
					return true;

				if ( nodeIndex < currNode.count + currNode.rightCount && null == currNode.right )
					return true;

				currNode = SparseArray.GetSuccessor( currNode );
				if ( null != currNode )
					nodeIndex = null == currNode.left ? -currNode.leftCount : 0;

				return null != currNode;
			}

			object System.Collections.IEnumerator.Current
			{
				get
				{
					this.EnsureNotExhausted( );

					return nodeIndex >= 0 && nodeIndex < currNode.count && null != currNode.arr
						? currNode.arr[ nodeIndex ] : null;
				}
			}
		}

		#endregion // Enumerator Class Definition

		#region CreateItemEnumerator Class Definition

		private class CreateItemEnumerator : System.Collections.IEnumerator
		{
			private SparseArray sarr = null;
			int index = -1;
			private ICreateItemCallback createItemCallback = null;

			internal CreateItemEnumerator( SparseArray sarr, ICreateItemCallback createItemCallback )
			{
				this.sarr = sarr;
				this.createItemCallback = createItemCallback;
				((System.Collections.IEnumerator)this).Reset( );
			}

			private void EnsureNotExhausted( )
			{
				if ( this.index >= this.sarr.Count )
					throw new InvalidOperationException( SparseArray.GetString( "LE_InvalidOperationException_1" ) );
			}

			void System.Collections.IEnumerator.Reset( )
			{
				this.index = -1;
			}

			bool System.Collections.IEnumerator.MoveNext( )
			{
				this.EnsureNotExhausted( );
				this.index++;
				return this.index < this.sarr.Count;
			}

			object System.Collections.IEnumerator.Current
			{
				get
				{
					this.EnsureNotExhausted( );
					return this.sarr.GetItem( this.index, this.createItemCallback );
				}
			}
		}

		#endregion // CreateItemEnumerator Class Definition

		#region NonNullEnumerator Class Definition

		private class NonNullEnumerator : System.Collections.IEnumerator
		{
			private SparseArray sarr;
			private int nodeIndex = -1;
			private Node currNode;

			internal NonNullEnumerator( SparseArray sarr )
			{
				this.sarr = sarr;
				((System.Collections.IEnumerator)this).Reset( );
			}

			private void EnsureNotExhausted( )
			{
				if ( null == this.currNode )
					throw new InvalidOperationException( SparseArray.GetString( "LE_InvalidOperationException_1" ) );
			}

			void System.Collections.IEnumerator.Reset( )
			{
				this.currNode = SparseArray.GetMinimum(sarr.tree);
				this.nodeIndex = -1;
			}

			bool System.Collections.IEnumerator.MoveNext( )
			{
				this.EnsureNotExhausted( );

				nodeIndex++;

				while ( null != currNode && 
					( nodeIndex >= currNode.count 
					|| null == currNode.arr || null == currNode.arr[ nodeIndex ] ) )
				{
					nodeIndex++;

					if ( nodeIndex >= currNode.count )
					{
						currNode = SparseArray.GetSuccessor( currNode );
						nodeIndex = 0;
					}
				}

				return null != currNode;
			}

			object System.Collections.IEnumerator.Current
			{
				get
				{
					this.EnsureNotExhausted( );

					return null != currNode.arr ? currNode.arr[ nodeIndex ] : null;
				}
			}
		}

		#endregion // NonNullEnumerator Class Definition

		#region NonNullEnumerable Class Definition

		private class NonNullEnumerable : System.Collections.IEnumerable
		{
			private SparseArray sarr = null;

			internal NonNullEnumerable( SparseArray sarr )
			{
				this.sarr = sarr;
			}

			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator( )
			{
				return new SparseArray.NonNullEnumerator( this.sarr );
			}
		}

		#endregion // NonNullEnumerable Class Definition

		#region NodeEnumerator Class Definition

		
#region Infragistics Source Cleanup (Region)








































#endregion // Infragistics Source Cleanup (Region)

		#endregion // NodeEnumerator Class Definition

		#region VisibleItemsEnumerator Class Definition

		// SSP 2/21/08
		// Added VisibleItemsEnumerator for enumerating visible items.
		// 
		/// <summary>
		/// Enumerates visible items in the sparse array. If createItemCallback is specified,
		/// new items are created to fill null slots otherwise null slots are ignored even
		/// though they are considered to be visible.
		/// </summary>
		private class VisibleItemsEnumerator : IEnumerator
		{
			internal class Enumerable : IEnumerable
			{
				private SparseArray _sarr;
				private ICreateItemCallback _createItemCallback;

				internal Enumerable( SparseArray sarr, ICreateItemCallback createItemCallback )
				{
					_sarr = sarr;
					_createItemCallback = createItemCallback;
				}

				public IEnumerator GetEnumerator( )
				{
					return new VisibleItemsEnumerator( _sarr, _createItemCallback );
				}
			}

			private SparseArray _sarr;
			private object _current;
			private int _visibleIndex;
			private ICreateItemCallback _createItemCallback;

			internal VisibleItemsEnumerator( SparseArray sarr, ICreateItemCallback createItemCallback )
			{
				Debug.Assert( sarr.manageScrollCounts );
				if ( !sarr.manageScrollCounts )
					throw new InvalidOperationException( "Sparse array doesn't manage visible counts!" );

				_sarr = sarr;
				_createItemCallback = createItemCallback;
				this.Reset( );
			}

			public void Reset( )
			{
				_current = null;
				_visibleIndex = -1;
			}

			public bool MoveNext( )
			{
				while ( ++_visibleIndex < _sarr.GetVisibleCount( ) )
				{
					_current = _sarr.GetItemAtVisibleIndex( _visibleIndex, _createItemCallback );
					if ( null != _current )
						return true;
				}

				return false;
			}

			public object Current
			{
				get
				{
					return _current;
				}
			}

		}

		#endregion // VisibleItemsEnumerator Class Definition

		#endregion // Class Definitions

		#region Member Variables

		private const int   DEFAULT_FACTOR = 40;
		private const float DEFAULT_GROWTH_FACTOR = 0.25f;

		private const int	NULL_ITEM_VISIBLE_COUNT = 1;
		private const int	NULL_ITEM_SCROLL_COUNT  = 1;

		private Node	tree		= null;
		private int		factor;//		= 0;
		private int		growBy;//		= 0;	
		private bool    scrollCountInfoDirtyOnAllNodes;// = false;
		private bool	useOwnerData;//		 = false;
		private bool	manageScrollCounts;//   = false;		

		// SSP 4/9/07 BR20818 - Optimizations
		// 
		private bool onScrollCountChangedCalledBefore;

		// SSP 5/18/09 TFS16576
		// Added a flag to keep track of whether EnsureScrollCountCalculated gets called
		// recursively. This will let us debug.assert to let the caller know that it's
		// getting called recursively.
		// 
		private bool _inEnsureScrollCountCalculated;

		// SSP 2/28/10 - Optimizations
		// 
		private bool _cacheScrollCounts;

        #endregion // Member Variables

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="SparseArray"/> class with the default factor
		/// </summary>
		public SparseArray( ) : this( SparseArray.DEFAULT_FACTOR )
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SparseArray"/> class
		/// </summary>
		/// <param name="useOwnerData">If true then either all items should implement ISparseArrayItem interface or the derived class should override <see cref="SparseArray.GetOwnerData"/> and <see cref="SetOwnerData"/> methods to maintain owner data for items.</param>
		public SparseArray( bool useOwnerData ) : this( SparseArray.DEFAULT_FACTOR, SparseArray.DEFAULT_GROWTH_FACTOR, useOwnerData )
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SparseArray"/> class .
		/// </summary>
		/// <param name="factor"></param>
		public SparseArray( int factor ) : this( factor, SparseArray.DEFAULT_GROWTH_FACTOR )
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SparseArray"/> class.
		/// </summary>
		/// <param name="factor"></param>
		/// <param name="growthFactor">Must be between 0f and 1f exclusive.</param>
		public SparseArray( int factor, float growthFactor ) : this( factor, growthFactor, false )
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SparseArray"/> class.
		/// </summary>
		/// <param name="factor"></param>
		/// <param name="growthFactor">Must be between 0f and 1f exclusive.</param>
		/// <param name="useOwnerData">If true then all the items must implement ISparseArrayItem interface.</param>
		public SparseArray( int factor, float growthFactor, bool useOwnerData )
			: this( factor, growthFactor, useOwnerData, false )
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SparseArray"/> class.
		/// </summary>
		/// <param name="manageScrollCounts"></param>
		/// <param name="factor"></param>
		/// <param name="growthFactor">Must be between 0f and 1f exclusive.</param>
        // SSP 5/18/10 TFS32148
        // Made public from protected.
        // 
		//protected SparseArray( bool manageScrollCounts, int factor, float growthFactor )
        public SparseArray( bool manageScrollCounts, int factor, float growthFactor )
			: this( factor, growthFactor, true, manageScrollCounts )
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="factor"></param>
		/// <param name="growthFactor"></param>
		/// <param name="useOwnerData"></param>
		/// <param name="manageScrollCounts"></param>
		// SSP 1/5/09 - NAS9.1 Record Filtering
		// Changed accessor from private to protected.
		// 
		//private SparseArray( int factor, float growthFactor, bool useOwnerData, bool manageScrollCounts )
		protected SparseArray( int factor, float growthFactor, bool useOwnerData, bool manageScrollCounts )
		{
			if ( factor < 2 )
				throw new ArgumentOutOfRangeException( "factor", GetString( "LE_ArgumentOutOfRangeException_1" ) );

			if ( growthFactor <= 0.0f || growthFactor > 1.0f )
				throw new ArgumentOutOfRangeException( "growthFactor", GetString("LE_ArgumentOutOfRangeException_2") );
				
			this.factor = factor;
			this.growBy = Math.Max(1, (int)(factor * growthFactor));

			// Make sure to set the manageScrollCounts before calling the CreateNode.
			// Also owner data has to be supported for managing scroll count.
			//
			this.useOwnerData = useOwnerData || manageScrollCounts;
			this.manageScrollCounts = manageScrollCounts;

			// SSP 2/25/2011 - Optimizations
			// 
			_cacheScrollCounts = this.manageScrollCounts;

			this.tree = this.CreateNode( );
		}

		#endregion // Constructor

		#region Public Properties

		#region Count

		/// <summary>
		/// Retruns the number of items contained in the collection.
		/// </summary>
		public int Count
		{
			get
			{
				return this.tree.TotalCount;
			}
		}

		#endregion // Count

		#region Indexer

		/// <summary>
		/// Indexer.
		/// </summary>
		public object this[ int index ]
		{
			get
			{
				return this.GetItem( index, null );
			}
			set
			{
				// SSP 8/26/11 TFS84827
				// Pass false for the new performingAdd parameter. Also the method name was changed to include
				// postfix of Helper.
				// 
				//this.SetItem( index, value );
				this.SetItemHelper( index, value, false );
			}
		}

		#endregion // Indexer

		#region IsSynchronized

		/// <summary>
		/// Indicates whether this collection is synchronized. Always returns false.
		/// </summary>
		public bool IsSynchronized
		{
			get
			{
				return false;
			}
		}

		#endregion // IsSynchronized

		#region SyncRoot

		/// <summary>
		/// Returns an object that can be used to synchronize thread access.
		/// </summary>
		public object SyncRoot
		{
			get
			{
				return this;
			}
		}

		#endregion // SyncRoot

		#region NonNullItems

		/// <summary>
		/// Returns a new instance of enumerable that can be used to enumerate through only the non-null items in the collection.
		/// </summary>
		public System.Collections.IEnumerable NonNullItems
		{
			get
			{
				return new NonNullEnumerable( this );
			}
		}

		#endregion // NonNullItems

		#endregion // Public Properties

		#region Public Methods

		#region Expand

		/// <summary>
		/// Expands the array to the new count. New count must be greater than or equal to the 
		/// current count. This has the same logical effect as adding new count - current count 
		/// number of null items at the end of the array.
		/// </summary>
		/// <param name="newCount"></param>
		public void Expand( int newCount )
		{
			if ( newCount < this.Count )
				throw new ArgumentOutOfRangeException( "newCount", GetString("LE_ArgumentOutOfRangeException_3") );
			
			int delta = newCount - this.Count;
			Node n = this.tree;

			while ( null != n.right )
			{
				n.rightCount += delta;
				n = n.right;
			}

			// n.count can be less than factor if and only if n is the only node in the tree.
			//
			bool dirtyNode = false;
			if ( n.count < this.factor )
			{
				int d = Math.Min( delta, this.factor - n.count );
				this.Lend( null, n, d, false );
				delta -= d;

				// Since we changed the count of the node, we need to set the isNodeDirty flag
				// of the node below when we call DirtyScrollCount on the node.
				//
				dirtyNode = true;
			}

			// SSP 1/31/05
			// Expand should only expand the array to the right.
			//
			
#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)


			n.rightCount += delta;

			// Dirty the scroll and visible counts on the ancestors, including n itself.
			//
			// SSP 4/9/07 BR20818 - Optimizations
			// Pass in true for the new overallScrollCountChanged parameter.
			// 
			//this.DirtyScrollCount( n, dirtyNode );
			this.DirtyScrollCount( n, dirtyNode, true );

			this.MaintainAncestors( n );
		}

		#endregion // Expand

		#region Add

		/// <summary>
		/// Adds the speicifed item at the end of the collection.
		/// </summary>
		/// <param name="item"></param>
		/// <returns>The index at which the item was added.</returns>
		public int Add( object item )
		{
			Node n = this.tree;
			while ( null != n.right )
			{
				n.rightCount++;
				n = n.right;
			}

			if ( n.rightCount <= 0 )
			{
				this.Array_Insert( n, n.count, item );

				// Dirty the scroll and visible counts on n and also update the necessary flags
				// on the ancestors of n.
				//
				// SSP 4/9/07 BR20818 - Optimizations
				// 
				//this.DirtyScrollCount( n );
				this.DirtyScrollCount_OverallScrollCountChanged( n );

				// Call FixupNode to maintain the ancestor chain.
				//
				this.FixupNode( n );
			}
			else
			{
				// SSP 11/9/06
				// Since the rightCount of the maximum node of the tree was not incremented in
				// the while loop above on (in the beginning of the method), increment it here.
				// 
				n.rightCount++;

				// Since node containing the last index (this.Count - 1) doesn't exist,
				// call SetItem to create it and set the item at the index. Also don't
				// create it if item is null.
				//
				// SSP 8/26/11 TFS84827
				// 
				// ------------------------------------------------------------------------
				//if ( null != item )
				//	this.SetItem( this.Count - 1, item );
				if ( null != item )
					this.SetItemHelper( this.Count - 1, item, true );
				else
					this.DirtyScrollCount( n, false, true );
				// ------------------------------------------------------------------------
			}

			// Return the location at which the item was added. IList.Add requires the
			// method to return the index at which item was added.
			//
			return this.Count - 1;
		}

		#endregion // Add

		#region Insert

		/// <summary>
		/// Inserts the specified item at the specified location in the array.
		/// </summary>
		/// <param name="index"></param>
		/// <param name="item"></param>
		public void Insert( int index, object item )
		{
			if ( index >= 0 && index < this.Count )
			{
				int origIndex = index;

				// If the item being inserted is null and no node has been created that contains
				// the index at which the item is being inserted, then don't create a node since
				// by default all items are considered null.
				// 
				Node n = this.GetNodeContainingIndex( ref index, null != item );

				if ( null != n )
				{
					this.Array_Insert( n, index, item );
					this.ApplyDeltaToAncestors( n, 1 );
					this.FixupNode( n );
				}
				else
				{
					// If we didn't force creation of the node because item was null, then we
					// still need to bump the counts on nodes on the path down to the 
					// non-existant node contianing index.
					//
					this.ApplyDeltaToDescendantPath( origIndex, 1 );
				}
			}
			else if ( this.Count == index )
			{
				// If adding at the end use the Add method. The reason for doing this is that
				// GetNodeContainingIndex only works with valid indeces.
				//
				this.Add( item );
			}
			else
				throw new ArgumentOutOfRangeException( "index" );
		}

		#endregion // Insert

		#region AddRange

		/// <summary>
		/// Adds items in the specified collection to the end of the collection.
		/// </summary>
		/// <param name="items"></param>
		public void AddRange( ICollection items )
		{
			this.InsertRange( this.Count, items );
		}

		#endregion // AddRange

		#region InsertRange

		/// <summary>
		/// Inserts items in the specified collection to the collection at the specified index.
		/// </summary>
		/// <param name="index"></param>
		/// <param name="items"></param>
		public void InsertRange( int index, ICollection items )
		{
			if ( index < 0 )
				this.ValidateIndex( index );
			
			// Note: index can be count.
			//
			if ( index > this.Count )
				this.ValidateIndex( index );

			int insertCount = items.Count;
			if ( insertCount > 0 )
			{
				this.Expand( this.Count + insertCount );

				// To insert items, expand the array by the number of items we are going to be
				// inserting and then move items starting at index to right by insert count
				// amount so we can set the items to be inserted to the space that becomes
				// available as a result.
				//
				for ( int i = this.Count - 1 - insertCount; i >= index; i-- )
					this[ i + insertCount ] = this[ i ];

				//foreach ( object item in items )
				//	this[ index++ ] = item;

				// Following code is the optimized version of the above commented out two lines.
				//
				IEnumerator e = items.GetEnumerator( );
				e.Reset( );
				bool hasMoreItems = true;
				while ( hasMoreItems && e.MoveNext( ) )
				{
					object item = e.Current;
					int i = index;
					Node n = this.GetNodeContainingIndex( ref i, null != item );
					index++;
					if ( null != n )
					{
						object[] arr = n.ItemsArray;
						arr[i++] = item;
						this.SetOwnerDataHelper( item, n );

						while ( i < n.count && ( hasMoreItems = e.MoveNext( ) ) )
						{
							item = e.Current;
							this.SetOwnerDataHelper( item, n );
							arr[i++] = item;
							index++;
						}

						// SSP 4/9/07 BR20818 - Optimizations
						// 
						//this.DirtyScrollCount( n );
						this.DirtyScrollCount_OverallScrollCountChanged( n );
					}
				}
			}
		}

		#endregion // InsertRange

		#region RemoveAt

		/// <summary>
		/// Removes an item at the specified index.
		/// </summary>
		/// <param name="index"></param>
		public void RemoveAt( int index )
		{
			this.ValidateIndex( index );
			int origIndex = index;

			Node n = this.GetNodeContainingIndex( ref index, false );
			if ( null != n )
			{
				this.Array_Remove( n, index );
				this.ApplyDeltaToAncestors( n, -1 );
				this.FixupNode( n );
			}
			else
			{
				// If node containing the index doesn't exist, we still need to decrement
				// the counts on nodes on the path down to the node containing index.
				//
				this.ApplyDeltaToDescendantPath( origIndex, -1 );
			}
		}

		#endregion // RemoveAt

        #region Move

        // SSP 5/18/10 TFS32148
        // 

        /// <summary>
        /// Moves the specified item to a new location.
        /// </summary>
        /// <param name="item">Item to move.</param>
        /// <param name="newIndex">New location of the item.</param>
        public void Move( object item, int newIndex )
        {
            int index = this.IndexOf( item );
            if ( index < 0 )
                throw new ArgumentException( );

            this.MoveHelper( index, newIndex, item, true );
        }

        /// <summary>
        /// Moves the item at the itemIndex to newIndex.
        /// </summary>
        /// <param name="itemIndex">Item at this index will be moved.</param>
        /// <param name="newIndex">Item will be moved to this index.</param>
        public void Move( int itemIndex, int newIndex )
        {
            this.MoveHelper( itemIndex, newIndex, null, false );
        }

        private void MoveHelper( int itemIndex, int newIndex, object item, bool itemIsSpecified )
        {
            this.ValidateIndex( itemIndex );
            this.ValidateIndex( newIndex );

            if ( !itemIsSpecified )
                item = this.GetItem( itemIndex );

            // We need to prevent OnScrollCountChanged method from being called as part of
            // RemoveAt and Insert calls below because the overall scroll count of the sparse
            // array doesn't change. This is for performance reasons. Also RemoveAt and Insert
            // calls should not cause any external processing because they just manipulate the
            // internal data structure without calling anything external and therefore should 
            // not cause the onScrollCountChangedCalledBefore to be set to true for any valid 
            // reason, and therefore we should be safe setting the 
            // onScrollCountChangedCalledBefore to true and false as a way of preventing
            // OnScrollCountChanged method from being called.
            // 
            bool origOnScrollCountChangedCalledBefore = this.onScrollCountChangedCalledBefore;
            this.onScrollCountChangedCalledBefore = true;
            try
            {
                this.RemoveAt( itemIndex );
                this.Insert( newIndex, item );
            }
            finally
            {
                this.onScrollCountChangedCalledBefore = origOnScrollCountChangedCalledBefore;
            }
        }

        #endregion // Move

		#region IndexOf

		/// <summary>
		/// Retruns the index of the specified item.
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public int IndexOf( object item )
		{
			if ( this.useOwnerData )
			{
                Node n = this.GetOwnerDataHelper( item ) as Node;
				if ( null != n )
				{
					// Get the start index of the node n. Then add to it the index of the item
					// within the node's array. Note: If the node is not the subtree, then 
					// GetStartIndex will return -1. This probably means that the item is removed.
					//
					int nodeIndex  = this.GetNodeStartIndex( n );
					if ( nodeIndex >= 0 )
					{
						if ( null != n.arr )
						{
							
							
							
							
							
							
							
							
							int indexInNodeArr = Array.IndexOf( n.arr, item, 0, n.count );

							if ( indexInNodeArr >= 0 )
								return nodeIndex + indexInNodeArr;
						}
					}
				}
                
                return -1;
			}
            else
            {			
			    // Perform a linear search on the tree to get the index.
			    //
			    return this.IndexOf( this.tree, item );
            }
		}

		#endregion // IndexOf

		#region RemoveRange

		/// <summary>
		/// Removes count number of items starting at the specified index in the array.
		/// </summary>
		/// <param name="index"></param>
		/// <param name="count"></param>
		public void RemoveRange( int index, int count )
		{
			if ( count <= 0 )
			{
				if ( 0 == count )
					return;

				throw new ArgumentOutOfRangeException( "count", GetString( "LE_ArgumentOutOfRangeException_4" ) );
			}

			int startIndex = index;
			int endIndex = startIndex + count - 1;

			this.ValidateIndex( startIndex );
			this.ValidateIndex( endIndex );

			this.RemoveRangeHelper( startIndex, endIndex );
		}

		#endregion // RemoveRange
		
		#region Remove

		/// <summary>
		/// Removes the specified item. If the item doesn't exist in the collection, it does nothing.
		/// </summary>
		/// <param name="value"></param>
		public void Remove( object value )
		{
			int index = this.IndexOf( value );
			if ( index >= 0 )
				this.RemoveAt( index );
		}

		#endregion // Remove

		#region Contains

		/// <summary>
		/// Returns true if the specified item is contained in the collection.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public bool Contains( object value )
		{
			return this.IndexOf( value ) >= 0;
		}

		#endregion // Contains

		#region Clear

		/// <summary>
		/// Removes all items from the array.
		/// </summary>
		public void Clear( )
		{
			this.Clear( true );
		}

		// SSP 7/23/10 TFS36141
		// Added an overload of Clear that takes in raiseOnScrollCountChanged parameter.
		// 
		private void Clear( bool raiseOnScrollCountChanged )
		{
			this.ClearOwnerDataTreeWide( this.tree );
			this.tree = this.CreateNode( );

			if ( raiseOnScrollCountChanged )
				this.OnScrollCountChanged( );
		}

		#endregion // Clear

		#region CopyTo

		/// <summary>
		/// Copies all the elements from this collection to the specified array starting at index.
		/// </summary>
		/// <param name="array"></param>
		/// <param name="index"></param>
		public void CopyTo( System.Array array, int index )
		{
			// SSP 12/5/05
			// Added an overload of CopyTo that takes in the callback. The following code was
			// moved into the new overload.
			// 
			
#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

			this.CopyTo( array, index, null );
		}

		// SSP 12/5/05
		// Added an overload of CopyTo that takes in the callback.
		// 
		/// <summary>
		/// Copies all the elements from this collection to the specified array starting at index.
		/// </summary>
		/// <param name="array"></param>
		/// <param name="index"></param>
		/// <param name="callback"></param>
		public void CopyTo( System.Array array, int index, ICreateItemCallback callback )
		{
			if ( array.Length - index < this.Count )
				throw new ArgumentException( GetString( "LE_ArgumentException_3" ), "array" );
		
			if ( null == callback )
			{
				this.CopyToHelper( this.tree, array, ref index );
			}
			else
			{
				IEnumerator e = this.GetEnumerator( callback );
				while ( e.MoveNext( ) )
					array.SetValue( e.Current, index++ );
			}
		}

		#endregion // CopyTo

		#region ToArray

		/// <summary>
		/// Returns a new array containing all the elements of this collection.
		/// </summary>
		/// <returns></returns>
		public object[] ToArray( )
		{
			return (object[])this.ToArray( typeof( object ) );
		}

		/// <summary>
		/// Returns a new array of specified type containing all the elements of this collection.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public System.Array ToArray( Type type )
		{
			Array array = Array.CreateInstance( type, this.Count );
			this.CopyTo( array, 0 );
			return array;
		}

		#endregion // ToArray

		#region GetItem

		/// <summary>
		/// Gets the item at the specified index. If the item at the specified index is null 
		/// and createItemCallback parameter is non-null, it will call CreateItem on the 
		/// callback to create it and set it on the array at the specified index.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public object GetItem( int index )
		{
			return this.GetItem( index, null );
		}

		/// <summary>
		/// Gets the item at the specified index. If the item at the specified index is null 
		/// and createItemCallback parameter is non-null, it will call CreateItem on the 
		/// callback to create it and set it on the array at the specified index.
		/// </summary>
		/// <param name="index"></param>
		/// <param name="createItemCallback"></param>
		/// <returns></returns>
		public object GetItem( int index, ICreateItemCallback createItemCallback )
		{
			this.ValidateIndex( index );

			int i = index;
			Node n = this.GetNodeContainingIndex( ref i, null != createItemCallback );

			if ( null != n )
			{
				// If createItemCallback is provided, then create the item if it's null.
				//
				if ( null != createItemCallback && ( null == n.arr || null == n.arr[ i ] ) )
					
					
					
					
					
					
					
					
					return this.CreateItemHelper( createItemCallback, n, i, index );

				return null != n.arr ? n.arr[ i ] : null;
			}

			return null;
		}

		#endregion // GetItem

		#region Reverse

		/// <summary>
		/// Reverses the order of items in the collection.
		/// </summary>
		public void Reverse( )
		{
			this.Reverse( 0, this.Count );
		}

		/// <summary>
		/// Reverses the order of items in the specified range.
		/// </summary>
		/// <param name="index"></param>
		/// <param name="count"></param>
		public void Reverse( int index, int count )
		{
			// JAS 4/29/05 BR03522
			if( count < 1 )
				return;

			int startIndex = index;
			int endIndex = index + count - 1;

			this.ValidateIndex( startIndex );
			this.ValidateIndex( endIndex );

			while ( startIndex < endIndex )
			{
				object tmp = this[ startIndex ];
				this[ startIndex ] = this[ endIndex ];
				this[ endIndex ] = tmp;
				startIndex++;
				endIndex--;
			}
		}

		#endregion // Reverse

		#region Sort

		/// <summary>
		/// Sorts the array.
		/// </summary>
		public void Sort( )
		{
			this.Sort( null );
		}

		/// <summary>
		/// Sorts the array.
		/// </summary>
		/// <param name="comparer"></param>
		public void Sort( IComparer comparer )
		{
			object[] arr = (object[])this.ToArray( typeof( object ) );

			// SSP 5/24/07
			// Use the merge sort. Also if comparer is null then use a default one.
			// 
			//Array.Sort( arr, comparer );
			CoreUtilities.SortMerge( arr, null != comparer ? comparer : Comparer<object>.Default );

			// SSP 7/23/10 TFS36141
			// Don't raise multiple scroll count change notifications. When Clear method
			// call raises it, the sparse array is empty and if the owner accesses the sparse
			// array from the notification then it can have incorrect results.
			// Added the new ReplaceAllItems method. Use that instead.
			// 
			
			
			this.ReplaceAllItems( arr );
		}

		// SSP 9/20/07
		// Added generic version of Sort.
		// 
		/// <summary>
		/// Sorts the array using generic array to achieve slightly better performance.
		/// </summary>
		/// <typeparam name="T">The array is assumed to contain objects of type T.</typeparam>
		/// <param name="comparer">Comparer to compare the elements of the array with.</param>
		public void SortGeneric<T>( System.Collections.Generic.IComparer<T> comparer )
		{
			T[] arr = (T[])this.ToArray( typeof( T ) );

			CoreUtilities.SortMergeGeneric<T>( arr, comparer );

			// SSP 7/23/10 TFS36141
			// Don't raise multiple scroll count change notifications. When Clear method
			// call raises it, the sparse array is empty and if the owner accesses the sparse
			// array from the notification then it can have incorrect results.
			// Added the new ReplaceAllItems method. Use that instead.
			// 
			
			
			this.ReplaceAllItems( arr );
		}

		#endregion // Sort

		#region Compact

		
		
		
 
		/// <summary>
		/// Removes data structures allocated for empty slots in the sparse array.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// Over time setting non-null slots to null can result in sparse array holding onto
		/// data structures that are not necessary since all the associated slots are null
		/// values. Calling this method will cause the sparse array to release those data 
		/// structures.
		/// </para>
		/// </remarks>
		public void Compact( )
		{
			bool treeModified = this.CompactHelper( this.tree );

			if ( treeModified && this.manageScrollCounts )
			{
				this.scrollCountInfoDirtyOnAllNodes = true;
				this.EnsureScrollCountCalculated( );
			}
		}

		private bool CompactHelper( Node n )
		{
			bool treeModified = false;

			if ( null == n )
				return treeModified;

			// Compact the subtrees first.
			// 
			treeModified = this.CompactHelper( n.left ) || treeModified;
			treeModified = this.CompactHelper( n.right ) || treeModified;

			// Then work on the node itself.
			// 
			if ( this.Array_AreAllItemsNull( n ) )
				treeModified = this.RemoveEmptyNode( n ) || treeModified;

			return treeModified;
		}

		private bool RemoveEmptyNode( Node n )
		{
			bool isRootNode = n == this.tree;

			// Rotate in such a way that will result in a better tree balance.
			// 
			bool rotateLeft;
			if ( null != n.left && null != n.right )
				rotateLeft = n.leftCount < n.rightCount;
			else if ( null != n.left )
				// If there is no RIGHT node then we can only rotate RIGHT.
				// 
				rotateLeft = false;
			else if ( null != n.right )
				// If there is no LEFT node then we can only rotate LEFT.
				// 
				rotateLeft = true;
			else
			{
				// If the node is a leaf node, simply release the reference to it from
				// the parent node.

				if ( null != n.parent )
				{
					if ( n == n.parent.left )
						n.parent.left = null;
					else if ( n == n.parent.right )
						n.parent.right = null;
					else
						Debug.Assert( false );

					return true;
				}

				return false;
			}

			Node newParent = rotateLeft ? this.RotateLeft( n ) : this.RotateRight( n );

			// If we rotated root node then it would have changed the root node of the tree
			// to a different instance of node. In which case make sure the tree member 
			// variable references the correct root node of the tree.
			// 
			if ( isRootNode )
				this.tree = newParent;

			// Call recursively until n is a leaf node at which point it will get removed.
			// 
			this.RemoveEmptyNode( n );

			return true;
		}

		#endregion // Compact

		#endregion // Public Methods

		#region Protected Methods

		#region GetOwnerData

		/// <summary>
		/// Returns the owner data of the item. Derived class can override <b>GetOwnerData</b> and <b>SetOwnerData</b> to support owner data without having to implement ISparseArrayItem interface on the items.
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		protected virtual object GetOwnerData( object item )
		{
			ISparseArrayItem sparseArrayItem = item as ISparseArrayItem;
			return null != sparseArrayItem ? sparseArrayItem.GetOwnerData( this ) : null;
		}

		#endregion // GetOwnerData

		#region SetOwnerData

		/// <summary>
		/// Sets the owner data on item. Derived class can override <b>GetOwnerData</b> and <b>SetOwnerData</b> to support owner data without having to implement ISparseArrayItem interface on the items.
		/// </summary>
		/// <param name="item"></param>
		/// <param name="ownerData"></param>
		protected virtual void SetOwnerData( object item, object ownerData )
		{
			// Set the owner data to the node containing the item.
			//
			Debug.Assert( item is ISparseArrayItem );
			ISparseArrayItem sparseArrayItem = item as ISparseArrayItem;
			if ( null != sparseArrayItem )
				sparseArrayItem.SetOwnerData( ownerData, this );
		}

		#endregion // SetOwnerData

		#region GetVisibleCount

		/// <summary>
		/// Returns the visible count.
		/// </summary>
		/// <returns></returns>
        // JJD 05/06/10 - TFS27757 - made public
        //protected int GetVisibleCount( )
		public int GetVisibleCount( )
		{
			this.EnsureScrollCountCalculated( );
			return ((NodeExtended)this.tree).TotalVisibleCount;
		}

		#endregion // GetVisibleCount

		#region GetScrollCount

		/// <summary>
		/// Returns the scroll count.
		/// </summary>
		/// <returns></returns>
        // JJD 05/06/10 - TFS27757 - made public
        //protected int GetScrollCount()
        public int GetScrollCount()
		{
			this.EnsureScrollCountCalculated( );
			return ((NodeExtended)this.tree).TotalScrollCount;
		}

		#endregion // GetScrollCount

		#region GetVisibleIndexOf

		/// <summary>
		/// Returns the visible index associted with the specified item. Item must be contained 
		/// within the array. If the item is hidden, that is it's ScrollCount is 0, the this
		/// method returns -1.
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
        // JJD 05/06/10 - TFS27757 - made public
        //protected int GetVisibleIndexOf(ISparseArrayMultiItem item)
        public int GetVisibleIndexOf(ISparseArrayMultiItem item)
		{
			return this.GetVisibleIndexOf( item, false );
		}

		#region CountDescretePoints

		// SSP 2/28/10 - Optimizations
		// 
		private static int CountDescretePoints( int[] arr, int startIndex, int endIndex )
		{
			int c = 0;

			int prevPoint = 0;

			for ( int i = startIndex; i <= endIndex; i++ )
			{
				int point = arr[i];
				if ( prevPoint != point )
					c++;

				prevPoint = point;
			}

			return c;
		}

		#endregion // CountDescretePoints

		/// <summary>
		/// Returns the visible index associted with the specified item. Item must be contained 
		/// within the array.
		/// </summary>
		/// <param name="item"></param>
		/// <param name="ignoreItemHiddenState">If false then returns -1 if the item is hidden. 
		/// If true, returns the visible index of item as it would have been if it were visible.
		/// This parameter doesn't have any effect on the returned value if the item is not hidden.
		/// </param>
		/// <returns></returns>
		// SSP 1/10/09 NAS9.1 Record Filtering
		// Made protected from private.
		// 
		//private int GetVisibleIndexOf( ISparseArrayMultiItem item, bool ignoreItemHiddenState )
		// JJD 05/06/10 - TFS27757 - made public
		//protected int GetVisibleIndexOf( ISparseArrayMultiItem item, bool ignoreItemHiddenState )
		public int GetVisibleIndexOf( ISparseArrayMultiItem item, bool ignoreItemHiddenState )
		{
			// SSP 6/1/04
			// Throw InvalidOperationException if not managing visible/scroll counts. Also 
			// make sure scroll counts are recalculated if scroll count info is marked dirty.
			//
			// ------------------------------------------------------------------------------
			if ( !this.manageScrollCounts )
				throw new InvalidOperationException( );

			this.EnsureScrollCountCalculated( );
			// ------------------------------------------------------------------------------

			// If the item is hidden, return -1.
			//
			if ( !ignoreItemHiddenState && item.ScrollCount <= 0 )
				return -1;

			NodeExtended n = this.GetOwnerDataHelper( item ) as NodeExtended;

			if ( null != n )
			{
				int nodeVisibleIndex = this.GetNodeStartVisibleIndex( n );
				if ( nodeVisibleIndex >= 0 )
				{
					int visibleIndexInNodeArr = 0;
					if ( null != n.arr )
					{
						bool itemFound = false;

						// SSP 2/28/10 - Optimizations
						// Added the if block and enclosed the existing code into the else block.
						// 
						int[] cachedScrollCounts;
						if ( _cacheScrollCounts && NodeExtendedWithScrollCountCache.g_dirtyFlag != ( cachedScrollCounts = n.ScrollCounts ) )
						{
							int itemIndex = Array.IndexOf<object>( n.arr, item );
							itemFound = itemIndex >= 0;
							if ( itemFound )
								visibleIndexInNodeArr = null != cachedScrollCounts 
									? CountDescretePoints( cachedScrollCounts, 0, itemIndex - 1 ) 
									: itemIndex;
						}
						else
						{
							for ( int i = 0; !itemFound && i < n.count; i++ )
							{
								ISparseArrayMultiItem tmpItem = (ISparseArrayMultiItem)n.arr[i];
								if ( tmpItem != item )
									visibleIndexInNodeArr += SparseArray.GetItemVisibleCount( tmpItem );
								else
									itemFound = true;
							}
						}

						if ( itemFound )
							return nodeVisibleIndex + visibleIndexInNodeArr;
					}
				}
			}

			return -1;
		}

        // SSP 5/18/10 TFS32148
        // Added an overload of GetVisibleIndexOf to be able to calculate visible index of an item
        // that hasn't been allocated yet.
        // 
        /// <summary>
        /// Gets the visible index of the item at the specified index. If the item hasn't been allocated
        /// yet then it assumes it's visible.
        /// </summary>
        /// <param name="itemIndex">Index of the item in the sparse array.</param>
        /// <param name="ignoreItemHiddenState">If false and the item is hidden, returns -1. Otherwise returns
        /// the visible index as if it were visible.</param>
        /// <returns>Visible index.</returns>
        /// <remarks>
        /// <para class="body">
        /// This method is typically used when the item at a specific index hasn't been allocated yet
        /// and one needs to calculate its visible index.
        /// </para>
        /// </remarks>
        public int GetVisibleIndexOf( int itemIndex, bool ignoreItemHiddenState )
        {
            if ( !this.manageScrollCounts )
                throw new InvalidOperationException( );

            this.EnsureScrollCountCalculated( );

            int indexInNode = itemIndex;
            NodeExtended n = this.GetNodeContainingIndex( ref indexInNode, true ) as NodeExtended;
            if ( null != n && indexInNode >= 0 )
            {
                int visibleIndex = this.GetNodeStartVisibleIndex( n );
				if ( visibleIndex >= 0 )
				{
					if ( null == n.arr )
						return visibleIndex + indexInNode;

					// SSP 2/28/10 - Optimizations
					// Added the if block and enclosed the existing code into the else block.
					// 
					int[] cachedScrollCounts;
					if ( _cacheScrollCounts && NodeExtendedWithScrollCountCache.g_dirtyFlag != ( cachedScrollCounts = n.ScrollCounts ) )
					{
						int nodeVisibleIndex = null != cachedScrollCounts
							? CountDescretePoints( cachedScrollCounts, 0, indexInNode - 1 )
							: indexInNode;

						int itemVisibleCount = null == cachedScrollCounts ? 1 
							: cachedScrollCounts[indexInNode] - ( indexInNode > 0 ? cachedScrollCounts[indexInNode - 1] : 0 );

						if ( !ignoreItemHiddenState && itemVisibleCount <= 0 )
							return -1;

						return visibleIndex + nodeVisibleIndex;
					}
					else
					{
						for ( int i = 0; i < n.count; i++ )
						{
							ISparseArrayMultiItem item = (ISparseArrayMultiItem)n.arr[i];
							int itemVisibleCount = SparseArray.GetItemVisibleCount( item );

							if ( indexInNode == i )
							{
								if ( !ignoreItemHiddenState && itemVisibleCount <= 0 )
									return -1;

								return visibleIndex;
							}

							visibleIndex += itemVisibleCount;
						}
					}
				}
            }

            return -1;
        }

		#endregion // GetVisibleIndexOf

		#region GetScrollIndexOf

		/// <summary>
		/// Gets the scroll index of the specified item.
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
        // JJD 05/06/10 - TFS27757 - made public
        //protected int GetScrollIndexOf(ISparseArrayMultiItem item)
        public int GetScrollIndexOf(ISparseArrayMultiItem item)
		{
			return this.GetScrollIndexOf( item, false );
		}

		// SSP 7/15/05 - BR05159 BR0508 - UltraWinGrid Fixed Add Row
		// Added an overload of GetScrollIndexOf that takes in the ignoreItemHiddenState parameter.
		// This change is to fix an issue with FixedAddRowOnTop AllowAddNew style where if all the
		// rows were visible in the grid (no scrollbar) and the fixed add-row on top is activated,
		// the first scrolling row would get scrolled out of view on top. Essentially this happened
		// because the RowsCollection.GetScrollIndex method assumes that the returned scroll index 
		// would be -1 if the item is hidden. This is what the GetVisibleIndexOf does. Therefore
		// made the GetScrollIndexOf do that as well. Also added this overload with 
		// ignoreItemHiddenState parameter in case there is a need to get the scroll index without
		// regard to the hidden state of the item.
		// 
		/// <summary>
		/// Gets the scroll index of the specified item.
		/// </summary>
		/// <param name="item"></param>
		/// <param name="ignoreItemHiddenState">If false then returns -1 if the item is hidden. 
		/// If true, returns the visible index of item as it would have been if it were visible.
		/// This parameter doesn't have any effect on the returned value if the item is not hidden.
		/// </param>
		/// <returns></returns>
        // JJD 05/06/10 - TFS27757 - made public
        //protected int GetScrollIndexOf(ISparseArrayMultiItem item, bool ignoreItemHiddenState)
        public int GetScrollIndexOf(ISparseArrayMultiItem item, bool ignoreItemHiddenState)
		{
			// SSP 6/1/04
			// Throw InvalidOperationException if not managing visible/scroll counts. Also 
			// make sure scroll counts are recalculated if scroll count info is marked dirty.
			//
			// ------------------------------------------------------------------------------
			if ( ! this.manageScrollCounts )
				throw new InvalidOperationException( );

			this.EnsureScrollCountCalculated( );
			// ------------------------------------------------------------------------------

			// SSP 7/15/05 - BR05159 BR0508 - UltraWinGrid Fixed Add Row
			// Added an overload of GetScrollIndexOf that takes in the ignoreItemHiddenState parameter.
			// 
			// If the item is hidden, return -1.
			//
			if ( ! ignoreItemHiddenState && item.ScrollCount <= 0 )
				return -1;

			NodeExtended n = this.GetOwnerDataHelper( item ) as NodeExtended;
			
			if ( null != n )
			{
				int nodeScrollIndex = this.GetNodeStartScrollIndex( n );
				if ( nodeScrollIndex >= 0 )
				{
					int scrollIndexInNodeArr = 0;
					if ( null != n.arr )
					{
						bool itemFound = false;

						// SSP 2/28/10 - Optimizations
						// Added the if block and enclosed the existing code into the else block.
						// 
						int[] cachedScrollCounts;
						if ( _cacheScrollCounts && NodeExtendedWithScrollCountCache.g_dirtyFlag != ( cachedScrollCounts = n.ScrollCounts ) )
						{
							int itemIndex = Array.IndexOf<object>( n.arr, item );
							itemFound = itemIndex >= 0;

							scrollIndexInNodeArr = itemIndex > 0 
								? ( null != cachedScrollCounts ? cachedScrollCounts[itemIndex - 1] : itemIndex )
								: 0;							
						}
						else
						{
							for ( int i = 0; !itemFound && i < n.count; i++ )
							{
								ISparseArrayMultiItem tmpItem = (ISparseArrayMultiItem)n.arr[i];
								if ( tmpItem != item )
									scrollIndexInNodeArr += SparseArray.GetItemScrollCount( tmpItem );
								else
									itemFound = true;
							}
						}

						if ( itemFound )
							return nodeScrollIndex + scrollIndexInNodeArr;
					}
				}
			}

			return -1;
		}

		#endregion // GetScrollIndexOf

		#region GetItemAtVisibleIndexOffset

		/// <summary>
		/// Returns offset'th visible item from startItem. StartItem can be hidden in which case
		/// it will start from the next visible item. Offset can be 0. If the resulting visible
		/// index is out of bounds, returns null.
		/// </summary>
		/// <param name="startItem"></param>
		/// <param name="offset"></param>
		/// <param name="createItemCallback"></param>
		/// <returns></returns>
        protected object GetItemAtVisibleIndexOffset( ISparseArrayMultiItem startItem, int offset, ICreateItemCallback createItemCallback )
		{
			int visibleIndex = this.GetVisibleIndexOf( startItem, true );

			// If the item is not in the collection, then return null.
			//
			if ( visibleIndex < 0 )
				return null;

			return this.GetItemAtVisibleIndex( visibleIndex + offset, createItemCallback );
		}

		#endregion // GetItemAtVisibleIndexOffset

		#region GetItemAtVisibleIndex

		/// <summary>
		/// Returns the item at the specified visible index. This method returns null if the visible index is out of bounds.
		/// </summary>
		/// <param name="visibleIndex">Visible index at which to get the item.</param>
		/// <param name="createItemCallback">Optional call back to create the item at the specified visible index if none exists at that visible index.</param>
		/// <returns></returns>
        protected object GetItemAtVisibleIndex( int visibleIndex, ICreateItemCallback createItemCallback )
		{
			if ( ! this.manageScrollCounts )
				throw new InvalidOperationException( );

			// If the visible index is out of bounds, return null rather than throwing 
			// the exception.
			//
			if ( visibleIndex < 0 || visibleIndex >= this.GetVisibleCount( ) )
				return null;				

			// Slots in the array can be empty. Following block may create new items. As it does
			// that, the visible and scroll counts of the nodes and the tree could change. Remember
			// null items are considered to be visible and have scroll count of 1. When an item
			// is created, it could say that it's hidden or that it's scroll count is more than 1
			// etc. In that case the overall visible count and scroll count of the tree could change.
			// In such a situation if the visible count gets smaller so that visibleIndex is beyond
			// the visible count, then return null. This outermost while loop is for this reason.
			//
			int origVisibleIndex = visibleIndex;
			int loopCounter = 0;
			while ( origVisibleIndex < this.GetVisibleCount( ) )
			{
				loopCounter++;
				visibleIndex = origVisibleIndex;
				NodeExtended n = (NodeExtended)this.tree;
				bool left = false;
				NodeExtended p = null;
				while ( null != n )
				{
					p = n;
					if ( visibleIndex < n.leftVisibleCount )
					{
						left = true;
						n = (NodeExtended)n.left;
					}
					else
					{
						visibleIndex -= n.leftVisibleCount;

						if ( visibleIndex < n.visibleCount )
							break;

						visibleIndex -= n.visibleCount;
						n = (NodeExtended)n.right;
						left = false;
					}
				}


				if ( null == n )
				{
					// If node containing specified visible index doesn't exist, then create it if
					// we are told to do so (if createItemCallback is passed in).
					//
					if ( null != createItemCallback )
					{
						// The idea behind this code is to create a node that we think will most likely
						// contain the visible index and re-execute the while loop. Next time around in
						// the while loop if we guessed correctly then we will have the node. If we 
						// didn't guess correctly then we will get here and create another node that we
						// think will most likely contain the visible index. This process will repeat
						// until either we have created all nodes in which case we will eventually
						// find the correct node or visibleIndex < this.GetVisibleCount( ) condition
						// of the outer loop will be false in which case we will exit the loop and 
						// method will return.
						// 
						int parentRegularIndex = this.GetNodeStartIndex( p );
						if ( left )
						{
							parentRegularIndex -= ( p.leftVisibleCount - visibleIndex ) / NULL_ITEM_VISIBLE_COUNT;
							parentRegularIndex = Math.Max( 0, parentRegularIndex );
						}
						else
						{
							parentRegularIndex += p.count;
							parentRegularIndex += visibleIndex / NULL_ITEM_VISIBLE_COUNT;
							parentRegularIndex = Math.Min( this.Count - 1, parentRegularIndex );
						}
				
						this.GetNodeContainingIndex( ref parentRegularIndex, true );
						
						// We should eventually stop coming here and exit the loop once all the nodes are
						// created. However in case there is an infinite loop return null.
						//
						Debug.Assert( loopCounter <= this.Count );
						if ( loopCounter > this.Count )
							return null;

						// Creating a node may have moved around nodes in the tree so reget the node
						// at the specified index starting from the top.
						// Tail-recurse.
						//
						continue;
					}
					else
					{
						// If node doesn't exist at the specified visible index and createItemCallback
						// is null, then return null.
						//
						return null;
					}
				}

				// If the item at the visible index is null and createItemCallback is also
				// null (which means that we aren't creating a new item), then return null.
				//
				if ( null == n.arr && null == createItemCallback )
					return null;

				// SSP 2/28/10 - Optimizations
				// 
				int prevScrollCount = 0;
				bool useScrollCountCache = false;
				int[] cachedScrollCounts = null;
				if ( _cacheScrollCounts )
				{
					cachedScrollCounts = n.ScrollCounts;
					useScrollCountCache = NodeExtendedWithScrollCountCache.g_dirtyFlag != cachedScrollCounts;
					Debug.Assert( useScrollCountCache );
				}

				bool anItemCreated = false;

				for ( int i = 0; i < n.count; i++ )
				{
					ISparseArrayMultiItem item = (ISparseArrayMultiItem)n.ItemsArray[i];
					int itemVisibleCount;

					// SSP 2/28/10 - Optimizations
					// Added the if block and enclosed the existing code in the else block.
					// 					
					if ( useScrollCountCache )
					{
						int tmp = null != cachedScrollCounts ? cachedScrollCounts[i] : prevScrollCount + NULL_ITEM_SCROLL_COUNT;
						itemVisibleCount = tmp - prevScrollCount > 0 ? 1 : 0;
						prevScrollCount = tmp;
					}
					else
						itemVisibleCount = SparseArray.GetItemVisibleCount( item );

					if ( visibleIndex < itemVisibleCount )
					{
						// If item is at the specified visible index is null then create a new item
						// if we are told to do so (if createItemCallback is passed in).
						//
						if ( null == item && null != createItemCallback )
						{
							int overallRegularIndex = this.GetNodeStartIndex( n ) + i;
							item = (ISparseArrayMultiItem)this.CreateItemHelper( createItemCallback, n, i, overallRegularIndex );
							
							// If an item is created, it's visibleCount and scrollCount could be
							// different (for example they could be 0) from the nullItemVisibleCount
							// and nullItemScrollCount. So execute the code in this for block again
							// for that item to recheck the visible count.
							//
							if ( null != item )
							{
								// SSP 2/28/10 - Optimizations
								// 
								useScrollCountCache = false;

								anItemCreated = true;
								i--;
								continue;
							}
						}

						return item;
					}
					
					visibleIndex -= itemVisibleCount;
				}

				if ( ! anItemCreated )
				{
					// If the implementor of ISparseArrayMultiItem doesn't notify of the change in the
					// scroll count then we should throw an assert.
					//
					
					
					
					Debug.WriteLine( "Some item in the sparse array changed it's scroll count without notifying the sparse array of the change !" );

					return null;
				}

			} // Outermost loop

			return null;
		}

		#endregion // GetItemAtVisibleIndex

		#region GetItemAtScrollIndex

		/// <summary>
		/// Returns the item at the specified scroll index. This method returns null if the scroll index is out of bounds.
		/// </summary>
		/// <param name="scrollIndex">Scroll index at which to get the item.</param>
		/// <param name="createItemCallback">Optional call back to create the item at the specified scroll index if none exists at that scroll index.</param>
		/// <returns></returns>
		protected object GetItemAtScrollIndex( int scrollIndex, ICreateItemCallback createItemCallback )
		{
			int offsetIntoItem;
			return this.GetItemAtScrollIndexHelper( scrollIndex, createItemCallback, true, out offsetIntoItem );
		}

		// SSP 7/30/09 - NAS9.2 Enhanced grid view
		// Added GetItemContainingScrollIndex method. Moved existing code from GetItemAtScrollIndex into the new
		// method GetItemAtScrollIndexHelper so the new method can use the same logic.
		// 
		/// <summary>
		/// Returns the item at the specified scroll index. This method returns null if the scroll index is out of bounds.
		/// </summary>
		/// <param name="scrollIndex">Scroll index at which to get the item. It will be modified to be the offset into the item's descendants.</param>
		/// <param name="createItemCallback">Optional call back to create the item at the specified scroll index if none exists at that scroll index.</param>
		/// <returns></returns>
		protected object GetItemContainingScrollIndex( ref int scrollIndex, ICreateItemCallback createItemCallback )
		{
			int offsetIntoItem;
			object retVal = this.GetItemAtScrollIndexHelper( scrollIndex, createItemCallback, false, out offsetIntoItem );
			if ( offsetIntoItem >= 0 )
				scrollIndex = offsetIntoItem;

			return retVal;
		}

		/// <summary>
		/// Returns the item at the specified scroll index. This method returns null if the scroll index is out of bounds.
		/// </summary>
		/// <param name="scrollIndex">Visible index at which to get the item.</param>
		/// <param name="createItemCallback">Optional call back to create the item at the specified scroll index if none exists at that scroll index.</param>
		/// <param name="traverseHierarchy">If false then the method return the item in this sparse array that contains the 
		/// specified scroll index. If true then it may traverse down the sparse array hierarchy to get the item at the
		/// specified scroll index from a sparse array that's at a lower level.</param>
		/// <param name="offsetIntoItem">If traverseHierarchy is false then this will be set to the scroll index relative 
		/// to the returned item.</param>
		/// <returns></returns>
		private object GetItemAtScrollIndexHelper( int scrollIndex, ICreateItemCallback createItemCallback, bool traverseHierarchy, out int offsetIntoItem )
		{
			offsetIntoItem = -1;

			if ( ! this.manageScrollCounts )
				throw new InvalidOperationException( );

			if ( scrollIndex < 0 || scrollIndex >= this.GetScrollCount( ) )
				return null;

			// Slots in the array can be empty. Following block may create new items. As it does
			// that, the visible and scroll counts of the nodes and the tree could change. Remember
			// null items are considered to be visible and have scroll count of 1. When an item
			// is created, it could say that it's hidden or that it's scroll count is more than 1
			// etc. In that case the overall visible count and scroll count of the tree could change.
			// In such a situation if the visible count gets smaller so that visibleIndex is beyond
			// the visible count, then return null. This outermost while loop is for this reason.
			//
			int origScrollIndex = scrollIndex;
			int loopCounter = 0;
			while ( origScrollIndex < this.GetScrollCount( ) )
			{
				loopCounter++;
				scrollIndex = origScrollIndex;
				NodeExtended n = (NodeExtended)this.tree;
				bool left = false;
				NodeExtended p = null;
				while ( null != n )
				{
					p = n;
					if ( scrollIndex < n.leftScrollCount )
					{
						left = true;
						n = (NodeExtended)n.left;
					}
					else
					{
						scrollIndex -= n.leftScrollCount;

						if ( scrollIndex < n.scrollCount )
							break;

						scrollIndex -= n.scrollCount;
						n = (NodeExtended)n.right;
						left = false;
					}
				}


				if ( null == n )
				{
					// If node containing specified visible index doesn't exist, then create it if
					// we are told to do so (if createItemCallback is passed in).
					//
					if ( null != createItemCallback )
					{
						// The idea behind this code is to create a node that we think will most likely
						// contain the scroll index and re-execute the while loop. Next time around in
						// the while loop if we guessed correctly then we will have the node. If we 
						// didn't guess correctly then we will get here and create another node that we
						// think will most likely contain the scroll index. This process will repeat
						// until either we have created all nodes in which case we will eventually
						// find the correct node or scrollIndex < this.GetScrollCount( ) condition
						// of the outer loop will be false in which case we will exit the loop and 
						// method will return.
						// 
						int parentRegularIndex = this.GetNodeStartIndex( p );
						if ( left )
						{
							parentRegularIndex -= ( p.leftScrollCount - scrollIndex ) / NULL_ITEM_SCROLL_COUNT;
							parentRegularIndex = Math.Max( 0, parentRegularIndex );
						}
						else
						{
							parentRegularIndex += p.count;
							parentRegularIndex += scrollIndex / NULL_ITEM_SCROLL_COUNT;
							parentRegularIndex = Math.Min( this.Count - 1, parentRegularIndex );
						}
				
						this.GetNodeContainingIndex( ref parentRegularIndex, true );

						// We should eventually stop coming here and exit the loop once all the nodes are
						// created. However in case there is an infinite loop return null.
						//
						Debug.Assert( loopCounter <= this.Count );
						if ( loopCounter > this.Count )
							return null;

						// Creating a node may have moved around nodes in the tree so reget the node
						// at the specified index starting from the top.
						// Tail-recurse.
						//
						continue;
					}
					else
					{
						// If node doesn't exist at the specified visible index and createItemCallback
						// is null, then return null.
						//
						return null;
					}
				}

				// If the item at the visible index is null and createItemCallback is also
				// null (which means that we aren't creating a new item), then return null.
				//
				if ( null == n.arr && null == createItemCallback )
					return null;

				// SSP 2/28/10 - Optimizations
				// 
				int prevScrollCount = 0;
				bool useScrollCountCache = false;
				int[] cachedScrollCounts = null;
				if ( _cacheScrollCounts )
				{
					cachedScrollCounts = n.ScrollCounts;
					useScrollCountCache = NodeExtendedWithScrollCountCache.g_dirtyFlag != cachedScrollCounts;
					Debug.Assert( useScrollCountCache );
				}

				bool anItemCreated = false;

				for ( int i = 0; i < n.count; i++ )
				{
					ISparseArrayMultiItem item = (ISparseArrayMultiItem)n.ItemsArray[i];
					int itemScrollCount;

					// SSP 2/28/10 - Optimizations
					// Added the if block and enclosed the existing code in the else block.
					// 
					if ( useScrollCountCache )
					{
						int tmp = null != cachedScrollCounts ? cachedScrollCounts[i] : prevScrollCount + NULL_ITEM_SCROLL_COUNT;
						itemScrollCount = tmp - prevScrollCount;
						prevScrollCount = tmp;
					}
					else
						itemScrollCount = SparseArray.GetItemScrollCount( item );					

					if ( scrollIndex < itemScrollCount )
					{
						// If item is at the specified visible index is null then create a new item
						// if we are told to do so (if createItemCallback is passed in).
						//
						if ( null == item && null != createItemCallback )
						{
							int overallRegularIndex = this.GetNodeStartIndex( n ) + i;
							item = (ISparseArrayMultiItem)this.CreateItemHelper( createItemCallback, n, i, overallRegularIndex );
							
							// If an item is created, it's visibleCount and scrollCount could be
							// different (for example they could be 0) from the nullItemVisibleCount
							// and nullItemScrollCount. So execute the code in this for block again
							// for that item to recheck the visible count.
							//
							if ( null != item )
							{
								// SSP 2/28/10 - Optimizations
								// 
								useScrollCountCache = false;

								anItemCreated = true;
								i--;
								continue;
							}
						}

						// SSP 9/11/06 BR15732
						// Account for item not being created above (if for example createItemCallback is null).
						// 
						//return item.GetItemAtScrollIndex( scrollIndex );
						// SSP 7/30/09 - NAS9.2 Enhanced grid view
						// Don't traverse down the hierarchy if the new traverseHierarchy parameter is false.
						// 
						//return null != item ? item.GetItemAtScrollIndex( scrollIndex ) : null;
						offsetIntoItem = scrollIndex;
						return traverseHierarchy && null != item ? item.GetItemAtScrollIndex( scrollIndex ) : item;
					}
					
					scrollIndex -= itemScrollCount;
				}

				if ( ! anItemCreated )
				{
					// If the implementor of ISparseArrayMultiItem doesn't notify of the change in the
					// scroll count then we should throw an assert.
					//
					
					
					
					Debug.WriteLine( "Some item in the sparse array changed its scroll count without notifying the sparse array of the change !" );

					return null;
				}

			} // Outermost loop

			return null;
		}

		#endregion // GetItemAtScrollIndex

		#region NotifyScrollCountChanged
		
		/// <summary>
		/// Whenever a scroll count of an item contained within this collection is changed, 
		/// the collection must be notified of the change. If not notified of such a change,
		/// behavior of various scroll and visible index related methods will be undefined.
		/// </summary>
		/// <param name="item">Item whose scroll count changed.</param>
        // SSP 5/18/10 TFS32148
        // Made public.
        // 
		//protected void NotifyItemScrollCountChanged( ISparseArrayMultiItem item )
        public void NotifyItemScrollCountChanged( ISparseArrayMultiItem item )
		{
			// SSP 4/22/05 - Optimization
			// Just a minor optimization. If it causes problems take it out. Call to 
			// DirtyScrollCount doesn't do anything if scrollCountInfoDirtyOnAllNodes 
			// is true.
			// 
			//if ( null != item )
			if ( null != item && ! this.scrollCountInfoDirtyOnAllNodes )
			{
				NodeExtended n = this.GetOwnerDataHelper( item ) as NodeExtended;

				//Debug.Assert( null != n, "Invalid owner data. Item may not exist in the array." );

				if ( null != n )
					// SSP 4/9/07 BR20818 - Optimizations
					// 
					//this.DirtyScrollCount( n );
					this.DirtyScrollCount_OverallScrollCountChanged( n );
			}
		}

		#endregion // NotifyScrollCountChanged

		#region DirtyScrollCountInfo

		/// <summary>
		/// Dirties scroll count info for the whole collection so next time it will be re-calculated.
		/// </summary>
		protected void DirtyScrollCountInfo( )
		{
			if ( ! this.scrollCountInfoDirtyOnAllNodes )
			{
				this.scrollCountInfoDirtyOnAllNodes = true;
				this.OnScrollCountChanged( );
			}
		}

		#endregion // DirtyScrollCountInfo

		#region OnScrollCountChanged

		/// <summary>
		/// Called by the sparse array whenever visible count or scroll count changes or is dirtied. Default implementation does nothing. This may get called multiple times for the same change as well as may get called even when the count doesn't change but is simply dirtied.
		/// </summary>
		protected virtual void OnScrollCountChanged( )
		{
		}

		#endregion // OnScrollCountChanged

		#region OnItemCreated

		// SSP 4/17/07
		// 
		/// <summary>
		/// Called when the sparese array creates an item by calling CreateItem
		/// on the ICreateItemCallback instance that gets passed into the method
		/// that initiates the creatio of the item (like GetItem method for example).
		/// </summary>
		/// <param name="item"></param>
		/// <param name="index"></param>
		protected virtual void OnItemCreated( object item, int index )
		{
		}

		#endregion // OnItemCreated

		#region OnDeserializationComplete

		// SSP 11/28/05
		// Added OnDeserializationComplete method. The UltraDataSource utilizes this.
		// 
		/// <summary>
		/// Sets the owner data on all the items contained in the tree. Typically you do not 
		/// need to call this as the sprase array automatically manages the owner data as you
		/// add and remove items. This is useful if you deserialize the sparse array and
		/// need to make sure that the owner data are set properly on the contained items.
		/// </summary>
		[ EditorBrowsable( EditorBrowsableState.Never ) ]
		public void OnDeserializationComplete( )
		{
			if ( this.useOwnerData && null != this.tree )
				this.SetOwnerDataTreeWide( this.tree );
		}

		#endregion // OnDeserializationComplete

		#region GetVisibleItems

		// SSP 2/21/08
		// Added VisibleItemsEnumerator for enumerating visible items.
		// 
		/// <summary>
		/// Returns all the visible items. New items will be created to fill null slots via
		/// createItemCallback parameter if it's non-null.
		/// </summary>
		/// <param name="createItemCallback">Used to create items to fill null slots. If null, null items will be skipped.</param>
		/// <returns>Visible items in this sparse array.</returns>
		protected IEnumerable GetVisibleItems( ICreateItemCallback createItemCallback )
		{
			return new VisibleItemsEnumerator.Enumerable( this, createItemCallback );
		}

		#endregion // GetVisibleItems

		#endregion // Protected Methods

		#region Protected Properties

		#region InCalculatingScrollCount

		// SSP 5/18/09 TFS16576
		// Added a flag to keep track of whether EnsureScrollCountCalculated gets called
		// recursively. This will let us debug.assert to let the caller know that it's
		// getting called recursively.
		// 
		/// <summary>
		/// Indicates whether the sparse array is currently in the process of calculating its scroll count.
		/// </summary>
		protected bool InCalculatingScrollCount
		{
			get
			{
				return _inEnsureScrollCountCalculated;
			}
		}

		#endregion // InCalculatingScrollCount

		#endregion // Protected Properties

		#region Private Properties/Methods

		#region GetString
		internal static string GetString(string name)
		{
			return GetString(name, null);
		}

		internal static string GetString(string name, params object[] args)
		{
#pragma warning disable 436
			return SR.GetString(name, args);
#pragma warning restore 436
		}
		#endregion //GetString

		#region MaxFactor

		private int MaxFactor
		{
			get
			{
				return 2 * this.factor - 1;
			}
		}

		#endregion // MaxFactor

		#region ValidateIndex







		private void ValidateIndex( int index )
		{
			if ( index < 0 || index >= this.Count )
				throw new IndexOutOfRangeException( );
		}

		#endregion // ValidateIndex

		#region SetItemHelper



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		// SSP 8/26/11 TFS84827
		// 
		//private void SetItem( int index, object item )
		private void SetItemHelper( int index, object item, bool performingAdd )
		{
			this.ValidateIndex( index );

			Node n = this.GetNodeContainingIndex( ref index, null != item );
			if ( null != n )
			{
				// SSP 4/30/07 - WPF Optimizations
				// 
				// --------------------------------------------
				//n.ItemsArray[index] = item;
				object[] itemsArray = n.ItemsArray;

				bool dirtyScrollCount = false;
				if ( this.manageScrollCounts )
				{
					dirtyScrollCount = true;

					// If the old item was null then it was assumed to occupy 1 scroll count. If the
					// new item also has 1 scroll count then don't dirty the scroll count for efficiency
					// reasons. If this causes a problem, take out the fix and reconsider optimization.
					// 
                    object oldItem = itemsArray[index];
					// SSP 8/26/11 TFS84827
					// 
					// ------------------------------------------------------------------------------------
					//if ( null == oldItem && 1 == GetItemScrollCount( (ISparseArrayMultiItem)item ) )
					//	dirtyScrollCount = false;
					if ( null == oldItem )
					{
						int itemScrollCount = GetItemScrollCount( (ISparseArrayMultiItem)item );

						// This method gets called from two places. From indexer setter and from Add 
						// implementation. When called from indexer setter, we don't need to dirty
						// the scroll count if the old item was null, and thus its could having been
						// considered as 1, and the scroll count of the new item is 1.
						// 
						// When performing add operation, we need to dirty the scroll count unless 
						// the scroll count of the item being added is 0.
						// 
						if ( itemScrollCount == ( performingAdd ? 0 : 1 ) )
							dirtyScrollCount = false;
					}
					// ------------------------------------------------------------------------------------
				}

				itemsArray[ index ] = item;
				// --------------------------------------------

				// Set the owner data.
				//
				this.SetOwnerDataHelper( item, n );

				// SSP 4/30/07 - WPF Optimizations
				// Related to the change above. Enclosed the code in the if block.
				// 
				if ( dirtyScrollCount )
				{
					// Dirty the scroll and visible counts on n and also update the necessary flags
					// on the ancestors of n.
					//
					// SSP 4/9/07 BR20818 - Optimizations
					// 
					//this.DirtyScrollCount( n );
					this.DirtyScrollCount_OverallScrollCountChanged( n );
				}
			}
		}

		#endregion // SetItemHelper

		#region ApplyDeltaToAncestors



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

		private void ApplyDeltaToAncestors( Node n, int delta )
		{
			Node origNode = n;

			while ( null != n.parent )
			{
				if ( n == n.parent.left )
					n.parent.leftCount += delta;
				else
					n.parent.rightCount += delta;

				n = n.parent;
			}

			// SSP 6/24/10 - TFS32949
			// Moved this from above before the while loop to here. We should fixup
			// the sparse array before raising any external notifications like 
			// OnScrollCountChanged.
			// 
			// If we are managing the scroll counts, dirty the scroll and visible counts 
			// on n and also update the necessary flags on the ancestors of n.
			// NOTE: Callers are expecting this method to dirty the scroll count on the
			// node since this method typically gets called when the count of the node 
			// changes.
			//
			// SSP 4/9/07 BR20818 - Optimizations
			// 
			//this.DirtyScrollCount( n );
			this.DirtyScrollCount_OverallScrollCountChanged( origNode );
		}

		#endregion // ApplyDeltaToAncestors

		#region ApplyDeltaToDescendantPath



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		private void ApplyDeltaToDescendantPath( int index, int delta )
		{
			Node n = this.tree;				
			Node p = null;
			while ( null != n )
			{
				p = n;
				if ( index < n.leftCount )
				{
					n.leftCount += delta;
					n = n.left;
				}
				else
				{
					index -= n.leftCount;

					if ( index < n.count )
						break;

					index -= n.count;
					n.rightCount += delta;
					n = n.right;
				}
			}

			// If we are managing the scroll counts, dirty the scroll and visible counts 
			// on n and also update the necessary flags on the ancestors of p.
			//
			//this.DirtyScrollCount( p );
			// SSP 4/9/07 BR20818 - Optimizations
			// Pass in true for the new overallScrollCountChanged parameter.
			// 
			//this.DirtyScrollCount( p, false );
			this.DirtyScrollCount( p, false, true );
		}

		#endregion // ApplyDeltaToDescendantPath

		#region RemoveRangeHelper

		
		
		private void GetNodeContainingIndeces( ref int xIndex, ref int yIndex, out Node xNode, out Node yNode )
		{
			int xi = xIndex;
			int yi = xIndex;

			xNode = this.GetNodeContainingIndex( ref xIndex, true );
			yNode = this.GetNodeContainingIndex( ref yIndex, false );
			if ( null == yNode )
			{
				xIndex = xi;
				yIndex = yi;

				yNode = this.GetNodeContainingIndex( ref yIndex, true );
				xNode = this.GetNodeContainingIndex( ref xIndex, true );
			}
		}



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		private void RemoveRangeHelper( int startIndex, int endIndex )
		{
			
			
			while ( startIndex <= endIndex )
			{
				int si = startIndex;
				int ei = endIndex;

				
				
				
				//Node startNode = this.GetNodeContainingIndex( ref si, true );
				//Node endNode   = this.GetNodeContainingIndex( ref ei, true );
				Node startNode, endNode;
				this.GetNodeContainingIndeces( ref si, ref ei, out startNode, out endNode );
				

				if ( startNode == endNode )
				{
					// If both the startIndex and endIndex fall in the same node, then remove
					// necessary items from the node and return.
					//
					int delta = 1 + ei - si;
					this.Array_RemoveRange( startNode, si, delta, true );
					this.ApplyDeltaToAncestors( startNode, -delta );
					this.FixupNode( startNode );
					endIndex -= delta;
				}
				else
				{
					// Otherwise see if either the start node or the end node fall completely 
					// within the startIndex and endIndex range. If they do, then we can simply
					// delete the whole node.
					//
					if ( 0 == si || endNode.count - 1 == ei )
					{
						Node n = 0 == si ? startNode : endNode;
						int delta = n.count;
						this.Array_RemoveRange( n, 0, delta, true );
						this.ApplyDeltaToAncestors( n, -delta );
						this.FixupNode( n );
						endIndex -= delta;
					}
					else
					{
						// If the startNode and the endNode do not completely fall in the start
						// index and end index range, then recursively call RemoveRangeHelper
						// with the endIndex such that the endNode will completely fall within
						// the startIndex and endIndex range in the next call.
						//
						this.RemoveRangeHelper( startIndex, endIndex - ( ei + 1 ) );

						// Since we deleted endIndex - (ei + 1) - startIndex + 1 number
						// of items in the above RemoveRangeHelper call, decrement
						// the endIndex by that amount. Start index doesn't change,
						// only the end index changes.
						//
						endIndex = startIndex + ei; 
					}
				}
			}
		}

		#endregion // RemoveRangeHelper

		#region CanLend

		
#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)


		#endregion // CanLend

		#region CanBorrow

		private bool CanBorrow( Node n, int itemCount )
		{
			return n.count - itemCount >= this.factor;
		}

		#endregion // CanBorrow

		#region Expand_Collapse



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		private object[] Expand_Collapse( Node n, int capacity )
		{
			object[] arr = n.arr;
			int currLen = null != arr ? arr.Length : 0;
			if ( capacity <= currLen )
			{
				if ( currLen - capacity < this.growBy )
					return arr;
			}

			int newCapacity = ( capacity / this.growBy ) * this.growBy;
			if ( newCapacity < capacity )
				newCapacity += this.growBy;
			
			// Since the node isn't expected to grow beyond maxFactor + 1, make sure
			// we don't allocate more space than ever will be needed.
			//
			if ( newCapacity > this.MaxFactor + 1 )
				newCapacity = Math.Max( capacity, this.MaxFactor + 1 );
				
			object[] oldArr = arr;
			n.arr = new object[ newCapacity ];
			return oldArr;
		}

		#endregion // Expand_Collapse

		#region GetOwnerDataHelper
		


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		private object GetOwnerDataHelper( object item )
		{
			return this.useOwnerData && null != item ? this.GetOwnerData( item ) : null;
		}

		#endregion // GetOwnerDataHelper

		#region SetOwnerData
		


#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		private void SetOwnerDataHelper( Node n, int startIndex, int count )
		{
			this.SetOwnerDataHelper( n.arr, n, startIndex, count );
		}



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		private void SetOwnerDataHelper( object[] arr, object ownerData, int startIndex, int count )
		{
			if ( this.useOwnerData && null != arr )
			{
				count += startIndex;
				for ( int i = startIndex; i < count; i++ )
					this.SetOwnerDataHelper( arr[ i ], ownerData );
			}
		}



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		private void SetOwnerDataHelper( object item, object ownerData )
		{
			if ( this.useOwnerData && null != item )
				this.SetOwnerData( item, ownerData );
		}

		#endregion // SetOwnerData

		#region ClearOwnerData

        
#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)




#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		private void ClearOwnerData( Node n, int startIndex, int count )
		{
			if ( null != n )
				this.ClearOwnerData( n.arr, startIndex, count );
		}



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		private void ClearOwnerData( object[] arr, int startIndex, int count )
		{
			this.SetOwnerDataHelper( arr, null, startIndex, count );
		}

		#endregion // ClearOwnerData

		#region Array_Insert

		private void Array_Insert( Node n, int index, object item )
		{
			if ( null != n.arr || null != item )
			{
				object[] oldArr = this.Expand_Collapse( n, 1 + n.count );

				if ( null != oldArr )
				{
					if ( oldArr != n.arr && index > 0 )
						Array.Copy( oldArr, 0, n.arr, 0, index );

					
					
					
					
					if ( index < n.count )
						Array.Copy( oldArr, index, n.arr, index + 1, n.count - index );
				}

				n.arr[ index ] = item;

				// Set the owner data to the node containing the item.
				//
				this.SetOwnerDataHelper( item, n );
			}

			n.count++;
		}

		#endregion // Array_Insert

		#region Array_Remove

		private void Array_Remove( Node n, int index )
		{
			this.Array_RemoveRange( n, index, 1, true );

			
#region Infragistics Source Cleanup (Region)
















#endregion // Infragistics Source Cleanup (Region)

		}

		#endregion // Array_Remove

		#region Array_RemoveRange

		private void Array_RemoveRange( Node n, int index, int count, bool clearOwnerData )
		{
			Debug.Assert( index < n.count && index + count <= n.count );

			if ( null != n.arr )
			{
				if ( count < n.count )
				{
					object[] oldArr = this.Expand_Collapse( n, n.count - count );

					if ( oldArr != n.arr && index > 0 )
						Array.Copy( oldArr, 0, n.arr, 0, index );

					// Reset the owner data to null.
					//
					if ( clearOwnerData )
						this.ClearOwnerData( oldArr, index, count );

					if ( index + count < n.count )
						Array.Copy( oldArr, index + count, n.arr, index, n.count - ( index + count ) );

					if ( oldArr == n.arr )
						Array.Clear( oldArr, n.count - count, count );
				}
				else
				{
					if ( clearOwnerData )
						this.ClearOwnerData( n, 0, n.count );
					n.arr = null;
				}
			}

			n.count -= count;
		}

		#endregion // Array_RemoveRange

		#region Array_InsertRange

		private void Array_InsertRange( Node dest, int destIndex, Node src, int srcIndex, int count )
		{
			if ( null != dest.arr || null != src && null != src.arr )
			{
				object[] oldArr = this.Expand_Collapse( dest, count + dest.count );

				if ( oldArr != dest.arr && destIndex > 0 && null != oldArr )
					Array.Copy( oldArr, 0, dest.arr, 0, destIndex );

				if ( destIndex < dest.count && null != oldArr )
					Array.Copy( oldArr, destIndex, dest.arr, destIndex + count, dest.count - destIndex );

				if ( null != src && null != src.arr )
				{
					Array.Copy( src.arr, srcIndex, dest.arr, destIndex, count );
					this.SetOwnerDataHelper( dest, destIndex, count );
				}
				else if ( oldArr == dest.arr )
				{
					Array.Clear( oldArr, destIndex, count );
				}			
			}

			dest.count += count;
		}

		#endregion // Array_InsertRange

		#region Array_AllItemsNull

		
		
		private bool Array_AreAllItemsNull( Node n )
		{
			object[] arr = n.arr;
			if ( null != arr )
			{
				for ( int i = 0; i < arr.Length; i++ )
				{
					if ( null != arr[i] )
						return false;
				}
			}

			return true;
		}

		#endregion // Array_AllItemsNull

		#region Lend

		private void Lend( Node nodeFrom, Node nodeTo, int itemsToLend, bool toLeft )
		{
			if ( 0 == nodeTo.count && null != nodeFrom && itemsToLend == nodeFrom.count )
			{
				// As an optimization, if we can simply swap the arrays and the counts on 
				// the two nodes then do that instead of copying around things.
				//
				nodeTo.arr = nodeFrom.arr;
				nodeFrom.arr = null;
				nodeTo.count = nodeFrom.count;
				nodeFrom.count = 0;
				this.SetOwnerDataHelper( nodeTo, 0, nodeTo.count );
			}
			else
			{
				if ( toLeft )
				{
					this.Array_InsertRange( nodeTo, 0, nodeFrom, null != nodeFrom ? nodeFrom.count - itemsToLend : 0, itemsToLend );
					if ( null != nodeFrom )
						this.Array_RemoveRange( nodeFrom, nodeFrom.count - itemsToLend, itemsToLend, false );
				}
				else
				{
					this.Array_InsertRange( nodeTo, nodeTo.count, nodeFrom, 0, itemsToLend );
					if ( null != nodeFrom )
						this.Array_RemoveRange( nodeFrom, 0, itemsToLend, false );
				}
			}
		}

		#endregion // Lend

		#region GetMaximum



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		private static Node GetMaximum(Node n)
		{
			if ( null != n )
			{
				while ( null != n.right )
					n = n.right;
			}

			return n;
		}

		#endregion // GetMaximum

		#region GetMinimum



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		private static Node GetMinimum(Node n)
		{
			if ( null != n )
			{
				while ( null != n.left )
					n = n.left;
			}

			return n;
		}

		#endregion // GetMinimum

		#region GetPredecessor



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		private static Node GetPredecessor( Node n )
		{
			if ( null != n.left )
				return SparseArray.GetMaximum( n.left );
			
			while ( null != n.parent )
			{
				if ( n == n.parent.right )
					return n.parent;

				n = n.parent;
			}

			return null;
		}

		#endregion // GetPredecessor

		#region GetSuccessor
		


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		private static Node GetSuccessor( Node n )
		{
			if ( null != n.right )
				return SparseArray.GetMinimum(n.right);
			
			while ( null != n.parent )
			{
				if ( n == n.parent.left )
					return n.parent;

				n = n.parent;
			}

			return null;
		}

		#endregion // GetSuccessor

		#region FixupNode



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		private void FixupNode( Node n )
		{
			// A node must have at least this.factor and at most 2*this.factor - 1 number of
			// items. When an item is removed, a node's count may tip below the factor. In
			// that case we need to borrow from a successor or a predecessor or merge with
			// a successor or a predecessor.
			//
			while ( null != n && n.count < this.factor )
			{
				Node maintainNode = null;
				int delta = this.factor - n.count;
				int origDelta = delta;
				int lendCount;

				// If the top-most node is the only node, then we can't do anything.
				//
				if ( null == n.parent && 0 == n.leftCount && 0 == n.rightCount )
					break;

				// If the parent node is successor or predecessor of n, then borrow or merge 
				// with the parent node.
				//
				if ( null != n.parent && ( n == n.parent.left && 0 == n.rightCount || n == n.parent.right && 0 == n.leftCount ) )
				{
					if ( this.CanBorrow( n.parent, delta ) )
					{
						this.Lend( n.parent, n, delta, n != n.parent.left );
						if ( n == n.parent.left )
							n.parent.leftCount += delta;
						else
							n.parent.rightCount += delta;

						// Dirty the visible and scroll counts on the n. Note we are dirtying the
						// n.parent at the end of the while loop. We just need to dirty the n right
						// here.
						//
						this.DirtyScrollCount( n );
					}
					else
					{
						// Otherwise merge with the parent.
						//
						lendCount = n.count;
						if ( lendCount > 0 )
							this.Lend( n, n.parent, lendCount, n.parent.left == n );

						if ( n == n.parent.left )
						{
							n.parent.leftCount -= lendCount;

							// n can only have left child since it's right count was 0 to begin with.
							// as required by the outer if condition.
							//
							Debug.Assert( null == n.right );
							n.parent.left = n.left;
							if ( null != n.left )
								n.left.parent = n.parent;
						}
						else
						{
							n.parent.rightCount -= lendCount;

							// n can only have right child since it's left count was 0 to begin with
							// as required by the outer if condition.
							//
							Debug.Assert( null == n.left );
							n.parent.right = n.right;
							if ( null != n.right )
								n.right.parent = n.parent;
						}
					}

					maintainNode = n.parent;

					// Set n to null since it may have gotten deleted in the else block above.
					// 
					n = null;
				}
				else
				{
					bool removeFromLeft = n.leftCount > n.rightCount;
					
					if ( removeFromLeft )
					{
						if ( null == n.left )
						{
							// If n has non-existant left (left count > 0 ), then borrow from that non-existant left.
							//
							Debug.Assert( n.leftCount > 0 );
							lendCount = Math.Min( n.leftCount, delta );
							this.Lend( null, n, lendCount, true );
							delta -= lendCount;
							n.leftCount -= lendCount;
							maintainNode = n;								
						}
						else
						{
							Node predecessor = SparseArray.GetMaximum(n.left);
						
							// If the predecessor is not really the predecessor because it has a non-existant 
							// right node, then borrow from the non-existant actual predecessor.
							//
							if ( predecessor.rightCount > 0 )
							{
								lendCount = Math.Min( predecessor.rightCount, delta );
								this.Lend( null, n, lendCount, true );
								predecessor.rightCount -= lendCount;
								delta -= lendCount;
							}

							if ( delta > 0 )
							{
								// If delta is still greater than 0, then the predecessor is really the
								// predecessor. It's right count was exhausted in the preceding if block.
								// So borrow from the predecessor or merge with it.
								//
								lendCount = this.CanBorrow( predecessor, delta ) ? delta : predecessor.count;
								this.Lend( predecessor, n, lendCount, true );
								delta -= lendCount;
							}

							maintainNode = predecessor;

							// If we merged the predecessor, then remove the predecessor node.
							//
							if ( 0 == predecessor.count )
							{
								Debug.Assert( predecessor.parent.left == predecessor || predecessor.parent.right == predecessor );
								Debug.Assert( null == predecessor.right );

								if ( predecessor == predecessor.parent.left )
									predecessor.parent.left = predecessor.left;
								else
									predecessor.parent.right = predecessor.left;

								if ( null != predecessor.left )
									predecessor.left.parent = predecessor.parent;

								maintainNode = predecessor.parent;
							}

							// Update the counts up the chain.
							//
							lendCount = origDelta - delta;
							n.leftCount -= lendCount;

							while ( predecessor.parent != n )
							{
								predecessor = predecessor.parent;
								predecessor.rightCount -= lendCount;
							}
						}
					}
					else
					{
						if ( null == n.right )
						{
							// If n has non-existant right (right count > 0 ), then borrow from that non-existant right.
							//
							Debug.Assert( n.rightCount > 0 );
							lendCount = Math.Min( n.rightCount, delta );

							// SSP 7/30/04 WDS44
							// We should be lending lendCount and not delta.
							//
							//this.Lend( null, n, delta, false );
							this.Lend( null, n, lendCount, false );

							delta -= lendCount;
							n.rightCount -= lendCount;
							maintainNode = n;
						}
						else
						{
							Node successor = SparseArray.GetMinimum(n.right);

							// If the successor is not really the successor because it has a non-existant 
							// left node, then borrow from the non-existant actual successor.
							//
							if ( successor.leftCount > 0 )
							{
								lendCount = Math.Min( successor.leftCount, delta );
								this.Lend( null, n, lendCount, false );
								successor.leftCount -= lendCount;
								delta -= lendCount;
							}
						
							if ( delta > 0 )
							{
								// If delta is still greater than 0, then the successor is really the
								// successor. It's left count was exhausted in the preceding if block.
								// So borrow from the successor or merge with it.
								//
								lendCount = this.CanBorrow( successor, delta ) ? delta : successor.count;
								this.Lend( successor, n, lendCount, false );
								delta -= lendCount;								
							}

							maintainNode = successor;

							// If we merged the successor, then remove the successor node.
							//
							if ( 0 == successor.count )
							{
								Debug.Assert( successor.parent.left == successor || successor.parent.right == successor );
								Debug.Assert( null == successor.left );
								if ( successor == successor.parent.left )
									successor.parent.left = successor.right;
								else
									successor.parent.right = successor.right;

								if ( null != successor.right )
									successor.right.parent = successor.parent;

								maintainNode = successor.parent;
							}

							// Update the counts up the chain.
							//
							lendCount = origDelta - delta;
							n.rightCount -= lendCount;

							while ( successor.parent != n )
							{
								successor = successor.parent;
								successor.leftCount -= lendCount;
							}
						}
					}
				}

				if ( null != maintainNode )
				{
					// Dirty the visible and scroll counts on the maintainNode and isLeftDirty or 
					// isRightDirty flags on its ancestors. Also dirty the scroll counts on the n.
					// 
					this.DirtyScrollCount( maintainNode );

					if ( null != n && maintainNode != n )
						// The reason why we are not updating the ancestor flags is that maintainNode
						// is a descendant of the n so we don't need set the flags on the ancestors 
						// here as they are already been done by above call to DirtyScrollCount with
						// maintainNode as the argument.
						//
						this.DirtyScrollCount( n );

					this.MaintainAncestors( maintainNode );
				}
			}


			// Likewise when an item is inserted, node's count may go above the 2*this.factor - 1
			// in which case we need to split the node.
			//
			while ( null != n && n.count > this.MaxFactor )
			{
				Node newNode = this.CreateNode( );
				bool insertToLeft = n.leftCount < n.rightCount;

				// Note the ! (not logical operator) in front of insertToLeft in the argument
				// list of the Lend call below. That was meant to be there.
				//
				this.Lend( n, newNode, this.factor, ! insertToLeft );
				
				if ( insertToLeft )
				{
					n.leftCount += newNode.count;
					if ( null == n.left )
					{
						n.left = newNode;
						newNode.parent = n;
					}
					else
					{
						Node predecessor = n.left;
						predecessor.rightCount += newNode.count;
						while ( null != predecessor.right )
						{
							predecessor = predecessor.right;
							predecessor.rightCount += newNode.count;
						}

						predecessor.right = newNode;
						newNode.parent = predecessor;
					}

					if ( newNode == newNode.parent.left )
						newNode.leftCount = newNode.parent.leftCount - newNode.count;
					else
						newNode.leftCount = newNode.parent.rightCount - newNode.count;
				}
				else
				{
					n.rightCount += newNode.count;
					if ( null == n.right )
					{
						n.right = newNode;
						newNode.parent = n;
					}
					else
					{
						Node successor = n.right;
						successor.leftCount += newNode.count;
						while ( null != successor.left )
						{
							successor = successor.left;
							successor.leftCount += newNode.count;
						}

						successor.left = newNode;
						newNode.parent = successor;
					}

					if ( newNode == newNode.parent.right )
						newNode.rightCount = newNode.parent.rightCount - newNode.count;
					else
						newNode.rightCount = newNode.parent.leftCount - newNode.count;
				}

				// Dirty the visible and scroll counts related flags on nodes.
				//
				this.DirtyScrollCount( n );
				this.DirtyScrollCount( newNode );

				if ( null != newNode.parent )
					this.MaintainAncestors( newNode.parent );
				else
					Debug.WriteLine( "test" );
			}
		}

		#endregion // FixupNode

		#region RotateLeft
		


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		private Node RotateLeft( Node p )
		{
			// We can only rotate left if p has a right node.
			//
			Node newParent = p.right;
			if ( null != newParent )
			{
				if ( p == this.tree )
					this.tree = newParent;
				else if ( p == p.parent.left )
					p.parent.left = newParent;
				else if ( p == p.parent.right )
					p.parent.right = newParent;
				else
					Debug.Assert( false );

				newParent.parent = p.parent;
				p.parent = newParent;
				p.right = newParent.left;
				newParent.left = p;
				if ( null != p.right )
					p.right.parent = p;
				p.rightCount = newParent.leftCount;
				newParent.leftCount = p.TotalCount;
			}

			return newParent;
		}

		#endregion // RotateLeft

		#region RotateRight



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		private Node  RotateRight( Node p )
		{
			// We can only rotate right if p has a left node.
			//
			Node newParent = p.left;
			if ( null != newParent )
			{
				if ( p == this.tree )
					this.tree = newParent;
				else if ( p == p.parent.left )
					p.parent.left = newParent;
				else if ( p == p.parent.right )
					p.parent.right = newParent;
				else
					Debug.Assert( false );

				newParent.parent = p.parent;
				p.parent = newParent;
				p.left = newParent.right;							
				newParent.right = p;
				if ( null != p.left )
					p.left.parent = p;
				p.leftCount = newParent.rightCount;
				newParent.rightCount = p.TotalCount;
			}

			return newParent;
		}

		#endregion // RotateRight

		#region Maintain



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		private void Maintain( Node p )
		{
			while ( null != p )
			{
				// Code in the block rotates p to left or right if the resulting subtree will be
				// more balanced.
				//

				Node newParent = null;
				int newLeftCount, newRightCount;

				if ( p.rightCount > p.leftCount && null != p.right )
				{
					newLeftCount = p.leftCount + p.count + p.right.leftCount;
					newRightCount = p.right.rightCount;

					// If the rotation results in a better balance then simply rotate.
					//
					if ( Math.Abs( newRightCount - newLeftCount ) < p.rightCount - p.leftCount )
					{
						newParent = this.RotateLeft( p );
					}
					else
					{
						// Otherwise ensure that the difference between the tree heights of the left
						// and the right subtrees is not greater than 1.
						//
						if ( ( this.factor + p.rightCount ) >= ( this.factor + p.leftCount ) * 4 )
						{
							Node tmp = p.right;
							if ( null != this.RotateRight( tmp ) )
							{
								//this.DirtyAncestorLeftRightScrollCountFlags( tmp );
								// SSP 4/9/07 BR20818 - Optimizations
								// 
								//this.DirtyScrollCount( tmp, false );
								this.DirtyScrollCount( tmp, false, false );

								newParent = this.RotateLeft( p );
							}
						}
					}
				}
				
				if ( p.leftCount > p.rightCount && null == newParent && null != p.left )
				{
					newLeftCount = p.left.leftCount;
					newRightCount = p.count + p.rightCount + p.left.rightCount;

					// If the rotation results in a better balance then simply rotate.
					//
					if ( Math.Abs( newLeftCount - newRightCount ) < p.leftCount - p.rightCount )
					{
						newParent = this.RotateRight( p );
					}
					else
					{
						// Otherwise ensure that the difference between the tree heights of the left
						// and the right subtrees is not greater than 1.
						//
						if ( ( this.factor + p.leftCount ) >= ( this.factor + p.rightCount ) * 4 )
						{
							Node tmp = p.left;
							if ( null != this.RotateLeft( tmp ) )
							{
								//this.DirtyAncestorLeftRightScrollCountFlags( tmp );
								// SSP 4/9/07 BR20818 - Optimizations
								// 
								//this.DirtyScrollCount( tmp, false );
								this.DirtyScrollCount( tmp, false, false );

								newParent = this.RotateRight( p );
							}
						}
					}
				}

				// If we did move any nodes around, then p's left count and right count 
				// could have changed. Same goes for the newParent.
				//
				if ( null != newParent )
				{
					// If we performed a rotation, dirty the scroll counts flags on the ancestors of p.
					//
					//this.DirtyAncestorLeftRightScrollCountFlags( p );
					// SSP 4/9/07 BR20818 - Optimizations
					// 
					//this.DirtyScrollCount( p, false );
					this.DirtyScrollCount( p, false, false );

					this.Maintain( p );
				}

				p = newParent;
			}
		}

		#endregion // Maintain

		#region MaintainAncestors







		private void MaintainAncestors( Node n )
		{
			while ( null != n )
			{
				this.Maintain( n );
				n = n.parent;
			}
		}

		#endregion // MaintainAncestors

		#region GetNodeContainingIndex
	


#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		private Node GetNodeContainingIndex( ref int indexVal, bool forceCreation )
		{
			Node p = null;
			Node n = this.tree;
			int index = indexVal;
			bool left = false;
			
			while ( null != n )
			{
				p = n;
				if ( index < n.leftCount )
				{
					n = n.left;
					left = true;
				}
				else
				{
					index -= n.leftCount;

					if ( index < n.count )
						break;

					index -= n.count;
					n = n.right;
					left = false;
				}
			}

			// If a node containing the specified index has not been created yet and forceCreation
			// is true, then create it.
			//
			if ( null == n && forceCreation )
			{
				bool dirtyParentScrollCount = true;

				// Left indicates whether index falls in the left subtree or the right subtree of
				// the parent node p.
				//
				if ( left )
				{
					if ( p.leftCount < this.factor )
					{
						if ( this.CanBorrow( p, this.factor - p.leftCount ) )
						{
							// If the left count of the parent is not enough to create the left child, then
							// we have to see if we can borrow enough from the parent to create the left child.
							//
							p.left = n = this.CreateNode( p );
							n.count = p.leftCount;
							this.Lend( p, n, this.factor - p.leftCount, false );
							p.leftCount = n.count;
						}
						else
						{
							// If we weren't able to borrow enough, we should be able to lend the left
							// count items to the parent.
							// 
							this.Lend( null, p, p.leftCount, true );
							p.leftCount = 0;
							n = p;
						}
					}
					else
					{
						// Create the left child that contains the index.
						//
						p.left = n = this.CreateNode( p );
						n.leftCount = this.factor * ( index / this.factor );
						n.count = this.factor;
						if ( n.leftCount + n.count > p.leftCount )
							n.leftCount = p.leftCount - this.factor;

						n.rightCount = p.leftCount - ( n.leftCount + n.count );
						index -= n.leftCount;

						// Since we did not add or remove items from the parent node,
						// we do not need to dirty the parent's scroll count.
						//
						dirtyParentScrollCount = false;
					}
				}
				else
				{
					if ( p.rightCount < this.factor )
					{
						int delta = this.factor - p.rightCount;
						if ( this.CanBorrow( p, delta ) )
						{
							// If the right count of the parent is not enough to create the right child, then
							// we have to see if we can borrow enough from the parent to create the right child.
							//
							p.right = n = this.CreateNode( p );
							n.count = p.rightCount;
							this.Lend( p, n, delta, true );
							p.rightCount = n.count;
							index += delta;
						}
						else
						{
							// If we weren't able to borrow enough, we should be able to lend the right
							// count items to the parent.
							// 
							index += p.count;
							this.Lend( null, p, p.rightCount, false );
							p.rightCount = 0;
							n = p;
						}
					}
					else
					{
						// Create the right child that contains the index.
						//
						p.right = n = this.CreateNode( p );
						n.leftCount = this.factor * ( index / this.factor );
						n.count = this.factor;
						if ( n.leftCount + n.count > p.rightCount )
							n.leftCount = p.rightCount - this.factor;

						n.rightCount = p.rightCount - ( n.leftCount + n.count );
						index -= n.leftCount;

						// Since we did not add or remove items from the parent node,
						// we do not need to dirty the parent's scroll count.
						//
						dirtyParentScrollCount = false;
					}
				}

				// Dirty the scroll and visible counts of the new node that was created 
				// as well as the isLeftDirty and isRightDirty flags of ancestor nodes
				// as well.
				//
				// SSP 6/9/05 - Optimizations
				// 
				// ----------------------------------------------------------------------
				//this.DirtyScrollCount( n );
				if ( this.manageScrollCounts )
				{
					NodeExtended ne = (NodeExtended)n;
					if ( ! dirtyParentScrollCount )
					{
						ne.leftScrollCount  = ne.leftVisibleCount  = n.leftCount;
						ne.scrollCount		= ne.visibleCount	   = n.count;
						ne.rightScrollCount = ne.rightVisibleCount = n.rightCount;

						// SSP 2/28/10 - Optimizations
						// 
						ne.ScrollCounts = null;
					}
					else
						this.DirtyScrollCount( n );
				}
				// ----------------------------------------------------------------------

				// If a new node was created then balance the parent of the new node.
				//
				if ( n != p )
				{
					// Dirty the scroll and visible counts of p since p's count changed
					// as indicated by dirtyParentScrollCount flag.
					//
					if ( dirtyParentScrollCount )
						this.DirtyScrollCount( p );

					this.Maintain( p );
				}
			}

			indexVal   = index;
			return n;
		}

		#endregion // GetNodeContainingIndex

		#region GetNodeStartIndex
		


#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		private int GetNodeStartIndex( Node n )
		{
			int index = n.leftCount;

			while ( null != n.parent )
			{
				if ( n == n.parent.right )
					index += n.parent.leftCount + n.parent.count;
					// If parent's left or right do not refer to n then that means that
					// n has been been deleted from the tree in which case return - 1.
					// 
				else if ( n != n.parent.left )
					return -1;

				n = n.parent;
			}

			// If the root node is not the same as the tree, then that means that the 
			// node has been deleted from the tree in which case return -1.
			//
			if ( n != this.tree )
				index = -1;

			return index;
		}

		#endregion // GetNodeStartIndex

		#region IndexOf



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		private int IndexOf( Node n, object item )
		{
			int i;

			if ( null != n.left )
			{
				i = this.IndexOf( n.left, item );
				if ( i >= 0 )
					return i;
			}
			
			
			
			else if ( null == item && n.leftCount > 0 )
			{
				return 0;
			}

			if ( null != n.arr )
			{
				
				
				
				
				
				
				
				
				i = Array.IndexOf( n.arr, item, 0, n.count );

				if ( i >= 0 )
					return i + n.leftCount;
			}
			
			
			
			else if ( null == item && n.count > 0 )
			{
				return n.leftCount;
			}

			if ( null != n.right )
			{
				i = this.IndexOf( n.right, item );
				if ( i >= 0 )
					return i + n.leftCount + n.count;
			}
			
			
			
			else if ( null == item && n.rightCount > 0 )
			{
				return n.leftCount + n.count;
			}

			return -1;
		}

		#endregion // IndexOf

		#region ClearOwnerDataTreeWide
		






		private void ClearOwnerDataTreeWide( Node n )
		{
			if ( null != n.left )
				this.ClearOwnerDataTreeWide( n.left );

			this.ClearOwnerData( n, 0, n.count );

			if ( null != n.right )
				this.ClearOwnerDataTreeWide( n.right );
		}

		#endregion // ClearOwnerDataTreeWide

		#region SetOwnerDataTreeWide



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		private void SetOwnerDataTreeWide( Node n )
		{
			if ( null != n.left )
				this.SetOwnerDataTreeWide( n.left );

			this.SetOwnerDataHelper( n, 0, n.count );

			if ( null != n.right )
				this.SetOwnerDataTreeWide( n.right );
		}

		#endregion // SetOwnerDataTreeWide

		#region CopyToHelper
		


#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		private void CopyToHelper( Node n, Array array, ref int index )
		{
			// Copy from the left subtree recursively.
			//
			if ( null != n.left )
			{
				this.CopyToHelper( n.left, array, ref index );
			}
			else if ( n.leftCount > 0 )
			{
				Array.Clear( array, index, n.leftCount );
				index += n.leftCount;
			}

			// Copy from the node.
			//
			if ( null != n.arr )
				Array.Copy( n.arr, 0, array, index, n.count );
			else
				Array.Clear( array, index, n.count );

			index += n.count;

			// Copy from the right subtree recursively.
			//
			if ( null != n.right )
			{
				this.CopyToHelper( n.right, array, ref index );
			}
			else if ( n.rightCount > 0 )
			{
				Array.Clear( array, index, n.rightCount );
				index += n.rightCount;
			}
		}

		#endregion // CopyToHelper

		#region GetCommonAncestor

		
#region Infragistics Source Cleanup (Region)





































#endregion // Infragistics Source Cleanup (Region)


		#endregion // GetCommonAncestor

		#region CreateNode
		






		private Node CreateNode( )
		{
			return this.CreateNode( null );
		}







		private Node CreateNode( Node parent )
		{
			if ( ! this.manageScrollCounts )
				return new Node( parent );
			// SSP 2/28/10 - Optimizations
			// Added the following else-if block.
			// 
			else if ( _cacheScrollCounts )
				return new NodeExtendedWithScrollCountCache( (NodeExtendedWithScrollCountCache)parent );
			else
				return new NodeExtended( (NodeExtended)parent );
		}

		#endregion // CreateNode


		#region GetNodeStartVisibleIndex
		


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		private int GetNodeStartVisibleIndex( NodeExtended n )
		{
			int visibleIndex = n.leftVisibleCount;

			while ( null != n.parent )
			{
				NodeExtended parent = (NodeExtended)n.parent;
				if ( n == parent.right )
					visibleIndex += parent.leftVisibleCount + parent.visibleCount;
					// If parent's left or right do not refer to n then that means that
					// n has been been deleted from the tree in which case return - 1.
					// 
				else if ( n != parent.left )
					return -1;

				n = parent;
			}

			// If the root node is not the same as the tree, then that means that the 
			// node has been delted from the tree in which case return -1.
			//
			if ( n != this.tree )
				visibleIndex = -1;

			return visibleIndex;
		}

		#endregion // GetNodeStartVisibleIndex

		#region GetNodeStartScrollIndex
		


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		private int GetNodeStartScrollIndex( NodeExtended n )
		{
			int scrollIndex = n.leftScrollCount;

			while ( null != n.parent )
			{
				NodeExtended parent = (NodeExtended)n.parent;
				if ( n == parent.right )
					scrollIndex += parent.leftScrollCount + parent.scrollCount;
					// If parent's left or right do not refer to n then that means that
					// n has been been deleted from the tree in which case return - 1.
					// 
				else if ( n != parent.left )
					return -1;

				n = parent;
			}

			// If the root node is not the same as the tree, then that means that the 
			// node has been delted from the tree in which case return -1.
			//
			if ( n != this.tree )
				scrollIndex = -1;

			return scrollIndex;
		}

		#endregion // GetNodeStartScrollIndex

		#region CreateItemHelper

		
#region Infragistics Source Cleanup (Region)



















#endregion // Infragistics Source Cleanup (Region)

		


#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

		private object CreateItemHelper( ICreateItemCallback createItemCallback, Node node, int nodeIndex, int overallIndex )
		{
			object oldItem = node.ItemsArray[ nodeIndex ];
			Debug.Assert( null == oldItem, "Item alreay non-null and CreateItemHelper called !" );

			object item = createItemCallback.CreateItem( this, overallIndex );
			node.ItemsArray[ nodeIndex ] = item;
			this.SetOwnerDataHelper( item, node );

			if ( null != item )
			{
				if ( this.manageScrollCounts )
				{
					// Update the visible and scroll count on the node and the ancestors.
					//
					ISparseArrayMultiItem multiItem = (ISparseArrayMultiItem)item;
					ISparseArrayMultiItem oldMultiItem = (ISparseArrayMultiItem)oldItem;

					int visibleDelta = SparseArray.GetItemVisibleCount( multiItem )
						- SparseArray.GetItemVisibleCount( oldMultiItem );

					int scrollDelta = SparseArray.GetItemScrollCount( multiItem )
						- SparseArray.GetItemScrollCount( oldMultiItem );

					this.ApplyScrollDelta( (NodeExtended)node, visibleDelta, scrollDelta );
				}

				// SSP 4/17/07
				// Added OnItemCreated virtual method.
				// 
				this.OnItemCreated( item, overallIndex );
			}

			return item;
		}

		#endregion // CreateItemHelper

		#region ApplyScrollDelta
		


#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		private void ApplyScrollDelta( NodeExtended node, int visibleDelta, int scrollDelta )
		{
			if ( 0 != visibleDelta || 0 != scrollDelta )
			{
				NodeExtended n = (NodeExtended)node;
				n.visibleCount += visibleDelta;
				n.scrollCount  += scrollDelta;

				// SSP 2/28/10 - Optimizations
				// 
				n.ScrollCounts = NodeExtendedWithScrollCountCache.g_dirtyFlag;

				while ( null != n.parent )
				{
					NodeExtended parent = (NodeExtended)n.parent;
					if ( n == parent.left )
					{
						parent.leftVisibleCount += visibleDelta;
						parent.leftScrollCount  += scrollDelta;
					}
					else
					{
						Debug.Assert( n == n.parent.right );

						parent.rightVisibleCount += visibleDelta;
						parent.rightScrollCount  += scrollDelta;
					}

					n = parent;
				}

				// Call the virtual OnScrollCountChanged method to notify derived classes.
				//
				this.OnScrollCountChanged( );
			}
		}

		#endregion // ApplyScrollDelta

		#region GetItemVisibleCount
		


#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

		private static int GetItemVisibleCount(ISparseArrayMultiItem item)
		{
			return null != item ? ( item.ScrollCount > 0 ? 1 : 0 ) : NULL_ITEM_VISIBLE_COUNT;
		}

		#endregion // GetItemVisibleCount

		#region GetItemScrollCount
		


#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		private static int GetItemScrollCount(ISparseArrayMultiItem item)
		{
			return null != item ? item.ScrollCount : NULL_ITEM_SCROLL_COUNT;
		}

		#endregion // GetItemScrollCount

		#region EnsureScrollCountCalculated







		private void EnsureScrollCountCalculated( )
		{
			// SSP 5/18/09 TFS16576
			// Added _inEnsureScrollCountCalculated flag and the code to set and reset it.
			// 
			bool origInEnsureScrollCountCalculated = _inEnsureScrollCountCalculated;
			if ( origInEnsureScrollCountCalculated )
				Debug.WriteLine( "EnsureScrollCountCalculated should not be called recursively. Sparse array is in the middle of calculating its scroll counts and thus calling EnsureScrollCountCalculated is not valid." );

			_inEnsureScrollCountCalculated = true;
			try
			{
				this.EnsureScrollCountCalculatedHelper( (NodeExtended)this.tree );
				this.scrollCountInfoDirtyOnAllNodes = false;

				// SSP 8/25/09 TFS18934
				// Moved setting the onScrollCountChangedCalledBefore to false here from 
				// above before the call to EnsureScrollCountCalculatedHelper. The reason
				// for this is that there's a possibility of an item raising scroll count
				// change notification while we are calculating the scroll counts in
				// the EnsureScrollCountCalculatedHelper, which will cause us to mark 
				// onScrollCountChangedCalledBefore as true in DirtyScrollCount method
				// and that will result in subsequent dirty item notifications after 
				// we finish EnsureScrollCountCalculated call to not raise any 
				// OnScrollCountChanged notification. So essentially this change prevents
				// OnScrollCountChanged call while we are calculating the scroll count and
				// allows for subsequent OnScrollCountChanged notifications. This is a 
				// better behavior because listeners, like the data presenter, skip 
				// processing OnScrollCountChanged while InCalculatingScrollCount flag is
				// true, and that causes a problem because subsequent notifications are 
				// never raised because onScrollCountChangedCalledBefore flag is left
				// to true.
				// 
				// SSP 4/9/07 BR20818 - Optimizations
				// 
				this.onScrollCountChangedCalledBefore = false;
			}
			finally
			{
				// SSP 5/18/09 TFS16576
				// 
				_inEnsureScrollCountCalculated = origInEnsureScrollCountCalculated;
			}
		}
		


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		private void EnsureScrollCountCalculatedHelper( NodeExtended n )
		{
			if ( n.isLeftDirty || this.scrollCountInfoDirtyOnAllNodes )
			{
				NodeExtended leftNode = (NodeExtended)n.left;
				if ( null != leftNode )
				{
					this.EnsureScrollCountCalculatedHelper( leftNode );
					n.leftVisibleCount = leftNode.TotalVisibleCount;
					n.leftScrollCount  = leftNode.TotalScrollCount;
				}
				else
				{
					n.leftVisibleCount = n.leftCount * NULL_ITEM_VISIBLE_COUNT;
					n.leftScrollCount  = n.leftCount * NULL_ITEM_SCROLL_COUNT;
				}

				// SSP 5/28/04
				//
				//n.isLeftDirty = false;
			}

			if ( n.isNodeDirty || this.scrollCountInfoDirtyOnAllNodes )
			{
				if ( null == n.arr )
				{
					n.visibleCount = n.count * NULL_ITEM_VISIBLE_COUNT;
					n.scrollCount  = n.count * NULL_ITEM_SCROLL_COUNT;

					// SSP 2/28/10 - Optimizations
					// 
					n.ScrollCounts = null;
				}
				else
				{
					// SSP 10/18/04
					// The original code is commented out below. This was done to handle some
					// recursive situations better.
					//
					// --------------------------------------------------------------------------
					int visibleCount = 0, scrollCount = 0;

					// SSP 2/28/10 - Optimizations
					// 
					int[] scrollCountCache = null;
					if ( _cacheScrollCounts )
					{
						scrollCountCache = n.ScrollCounts;
						if ( null == scrollCountCache || scrollCountCache.Length != n.count )
							scrollCountCache = new int[n.count];

						n.ScrollCounts = NodeExtendedWithScrollCountCache.g_dirtyFlag;
					}

					for ( int i = 0; i < n.count; i++ )
					{
						ISparseArrayMultiItem mi = (ISparseArrayMultiItem)n.arr[i];
						if ( null != mi )
						{
							int count = mi.ScrollCount;
							if ( count < 0 )
								throw new InvalidOperationException( GetString( "LE_InvalidOperationException_2" ) );

							// If the scroll count is greater than 0, that means the item is visible.
							//
							visibleCount += count > 0 ? 1 : 0;
							scrollCount  += count;
						}
						else
						{
							visibleCount += NULL_ITEM_VISIBLE_COUNT;
							scrollCount  += NULL_ITEM_SCROLL_COUNT;
						}

						if ( null != scrollCountCache )
							scrollCountCache[i] = scrollCount;
					}

					n.visibleCount = visibleCount;
					n.scrollCount = scrollCount;

					// SSP 2/28/10 - Optimizations
					// 
					n.ScrollCounts = scrollCountCache;

					
#region Infragistics Source Cleanup (Region)




















#endregion // Infragistics Source Cleanup (Region)

					// --------------------------------------------------------------------------
				}

				// SSP 5/28/04
				//
				//n.isNodeDirty = false;
			}

			if ( n.isRightDirty || this.scrollCountInfoDirtyOnAllNodes )
			{
				NodeExtended rightNode = (NodeExtended)n.right;
				if ( null != rightNode )
				{
					this.EnsureScrollCountCalculatedHelper( rightNode );
					n.rightVisibleCount = rightNode.TotalVisibleCount;
					n.rightScrollCount  = rightNode.TotalScrollCount;
				}
				else
				{
					n.rightVisibleCount = n.rightCount * NULL_ITEM_VISIBLE_COUNT;
					n.rightScrollCount  = n.rightCount * NULL_ITEM_SCROLL_COUNT;
				}

				// SSP 5/28/04
				//
				//n.isRightDirty = false;
			}

			// SSP 5/28/04
			// Commented out lines that reset these flags in above if blocks and 
			// added code to reset them here.
			//
			n.isLeftDirty = n.isNodeDirty = n.isRightDirty = false;
		}

		#endregion // EnsureScrollCountCalculated

		#region DirtyScrollCount



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		private void DirtyScrollCount( Node node )
		{
			// SSP 4/9/07 BR20818 - Optimizations
			// Pass in false by default for overallScrollCountChanged. All the places calling this
			// method were modified to ensure they are calling the correct value for this new 
			// parameter is passed in.
			// 
			//this.DirtyScrollCount( node, true );
			this.DirtyScrollCount( node, true, false );
		}

		// SSP 4/9/07 BR20818 - Optimizations
		// Added DirtyScrollCount_OverallScrollCountChanged.
		// 


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		private void DirtyScrollCount_OverallScrollCountChanged( Node node )
		{
			this.DirtyScrollCount( node, true, true );
		}



#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

		// SSP 4/9/07 BR20818 - Optimizations
		// 
		//private void DirtyScrollCount( Node node, bool setIsNodeDirtyFlag )
		private void DirtyScrollCount( Node node, bool setIsNodeDirtyFlag, bool overallScrollCountChanged )
		{
			// If scrollCountInfoDirtyOnAllNodes flag is true, then it means we are going to
			// recalculate the scroll count on all items. In which case we don't need to dirty
			// any flags.
			//
			if ( this.manageScrollCounts && ! this.scrollCountInfoDirtyOnAllNodes && null != node )
			{
				NodeExtended n = (NodeExtended)node;
				// SSP 4/9/07 BR20818 - Optimizations
				// 
				//bool onScrollCountChangedCalledBefore = n.isLeftDirty || n.isRightDirty || n.isNodeDirty;

				n.isLeftDirty = n.isRightDirty = true;
				if ( setIsNodeDirtyFlag )
				{
					n.isNodeDirty = true;

					// SSP 2/28/10 - Optimizations
					// 
					n.ScrollCounts = NodeExtendedWithScrollCountCache.g_dirtyFlag;
				}

				while ( null != n.parent )
				{
					NodeExtended parent = (NodeExtended)n.parent;
					if ( n == parent.left )
					{
						if ( ! parent.isLeftDirty )
							parent.isLeftDirty = true;
						else
							break;
					}
					else
					{
						if ( ! parent.isRightDirty )
							parent.isRightDirty = true;
						else
							break;
					}

					n = parent;
				}

				// Call OnScrollCountChanged to give derived class a chance to take appropriate
				// action, like for example notify the parent scroll count manager of the change.
				// Also only call OnScrollCountChanged if we already haven't called before. If 
				// one of isLeftDirty, isNodeDirty or isRightDirty flags were true, then that 
				// means that OnScrollCountChanged was already called before.
				//
				// SSP 4/9/07 BR20818 - Optimizations
				// 
				// --------------------------------------------------------------------------
				if ( overallScrollCountChanged && !this.onScrollCountChangedCalledBefore )
				{
					this.onScrollCountChangedCalledBefore = true;
					this.OnScrollCountChanged( );
				}
				




				// --------------------------------------------------------------------------
			}
		}

		#endregion // DirtyScrollCount

		#region ReplaceAllItems

		// SSP 7/23/10 TFS36141
		// Added ReplaceAllItems method.
		// 
		private void ReplaceAllItems( ICollection newItems )
		{
			bool origOnScrollCountChangedCalledBefore = this.onScrollCountChangedCalledBefore;
			this.onScrollCountChangedCalledBefore = true;

			try
			{
				this.Clear( false );
				this.AddRange( newItems );
			}
			finally
			{
				this.onScrollCountChangedCalledBefore = origOnScrollCountChangedCalledBefore;
			}

			this.DirtyScrollCountInfo( );
		}

		#endregion // ReplaceAllItems

		#region IsNodeInTree

		
#region Infragistics Source Cleanup (Region)










#endregion // Infragistics Source Cleanup (Region)


		#endregion // IsNodeInTree

		#region IsTreeValid



#region Infragistics Source Cleanup (Region)








































































#endregion // Infragistics Source Cleanup (Region)


		#endregion // IsTreeValid

		#endregion // Private Properties/Methods

		#region Implementation of IList

		bool IList.IsReadOnly
		{
			get
			{
				return false;
			}
		}

		bool IList.IsFixedSize
		{
			get
			{
				return false;
			}
		}

		#endregion // Implementation of IList

		#region Implementation of IEnumerable

		/// <summary>
		/// Returns an instance of IEnumerator that can be used to enumerate through all the 
		/// elements of this collection.
		/// </summary>
		/// <returns></returns>
		public System.Collections.IEnumerator GetEnumerator( )
		{
			return new SparseArray.Enumerator( this );
		}

		/// <summary>
		/// Returns an instance of IEnumerator that can be used to enumerate through all the 
		/// elements of this collection. New elements will be created to fill null slots via
		/// createItemCallback parameter if it's non-null.
		/// </summary>
		/// <param name="createItemCallback"></param>
		/// <returns></returns>
		public System.Collections.IEnumerator GetEnumerator( ICreateItemCallback createItemCallback )
		{
			return new SparseArray.CreateItemEnumerator( this, createItemCallback );
		}

		#endregion // Implementation of IEnumerable
	}

	#endregion // SparseArray Class

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