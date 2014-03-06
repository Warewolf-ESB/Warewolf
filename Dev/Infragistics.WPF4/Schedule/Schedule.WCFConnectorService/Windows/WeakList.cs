using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;


using Infragistics.Services;

namespace Infragistics.Collections.Services



{
    // AS 10/1/08 TFS5939/BR32114
    // Refactored this class into a generic and left a non-generic impl for backward compatibility.
    //
    /// <summary>
    /// A strongly typed list class that manages items using WeakReferences so the items in this list
    /// can be garbage collected. Items collected by garbage collector will be replaced
    /// by null. The <see cref="WeakList&lt;T&gt;.Compact"/> method can be used to remove entries
    /// from the list that have been garbage collected.
    /// </summary>
    public class WeakList<T> : IList<T>, IList
        where T : class
    {
        #region Nested Classes/Structures

        #region Enumerator Class

        private class Enumerator : IEnumerator<T>
        {
            #region Private Vars

            private WeakList<T> _list;
            private T _current;
            private int _index;

            #endregion // Private Vars

            #region Constructor

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="list">List to enumerate.</param>
            public Enumerator(WeakList<T> list)
            {
                _list = list;
                this.Reset();
            }

            #endregion // Constructor

            #region Current

            /// <summary>
            /// Returns the current item.
            /// </summary>
            public T Current
            {
                get
                {
                    return _current;
                }
            }

            #endregion // Current

            #region MoveNext

            /// <summary>
            /// Moves to next item.
            /// </summary>
            /// <returns>Returns false if the enumerator is exhausted.</returns>
            public bool MoveNext()
            {
                _current = null;

                while (++_index < _list.Count)
                {
                    _current = _list[_index];
                    if (null != _current)
                        break;
                }

                return null != _current;
            }

            #endregion // MoveNext

            #region Reset

            public void Reset()
            {
                _index = -1;
                _current = null;
            }

            #endregion // Reset

            #region IEnumerator.Current
            object IEnumerator.Current
            {
                get { return this.Current; }
            } 
            #endregion //IEnumerator.Current

            #region Dispose
            void IDisposable.Dispose()
            {
            } 
            #endregion //Dispose
        }

        #endregion // Enumerator Class

        #endregion // Nested Classes/Structures

        #region Private Vars

        // AS 9/30/08 Optimization
        //
        //private ArrayList _list = new ArrayList( );
        private List<WeakReference> _list = new List<WeakReference>();

        #endregion // Private Vars

        #region Properties

        #region Public Properties

        #region Count

        /// <summary>
        /// Returns the number of items in the list.
        /// </summary>
        public int Count
        {
            get
            {
                return _list.Count;
            }
        }

        #endregion // Count

        #region Indexer[ int ]

        /// <summary>
        /// Gets or sets the item at the specified location.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public T this[int index]
        {
            get
            {
                return this.GetItemHelper(index);
            }
            set
            {
                _list[index] = this.CreateWeakReference(value);
            }
        }

        #endregion // Indexer[ int ]

        #region SyncRoot

        /// <summary>
        /// Returns the object with which to syncrhonize.
        /// </summary>
        public object SyncRoot
        {
            // AS 9/30/08 Optimization
            //get { return _list.SyncRoot; }
            get { return ((ICollection)_list).SyncRoot; }
        }

        #endregion // SyncRoot

        #endregion // Public Properties

        #endregion // Properties

        #region Methods

        #region Public Methods

        #region Add

        /// <summary>
        /// Adds the specified item.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public int Add(T item)
        {
            //return _list.Add( this.CreateWeakReference( item ) );
            _list.Add(this.CreateWeakReference(item));
            return _list.Count - 1;
        }

        #endregion // Add

		#region AddRange

		// SSP 2/1/10
		// 
		/// <summary>
		/// Adds specified items to the list.
		/// </summary>
		/// <param name="items">Items to add to the list.</param>
		public void AddRange( IEnumerable<T> items )
		{
			foreach ( T ii in items )
			{
				this.Add( ii );
			}
		}

		#endregion // AddRange

		#region Clear

		/// <summary>
        /// Clears the list.
        /// </summary>
        public void Clear()
        {
            _list.Clear();
        }

        #endregion // Clear

        #region Compact

        /// <summary>
        /// Removes entries from the list that are no longer alive. Note that entries can
        /// get garbase collected during the process of compacting and therefore it's not
        /// guarrenteed that all the items will remain alive after this method returns.
        /// </summary>
        public void Compact()
        {
            for (int i = 0, count = _list.Count; i < count; i++)
                this.GetItemHelper(i);

            CoreUtilities.RemoveAll(_list, null);
        }

        #endregion // Compact

        #region Contains

        /// <summary>
        /// Returns true if the specified item is contained within this list.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool Contains(T value)
        {
            return this.IndexOf(value) >= 0;
        }

        #endregion // Contains

        #region CopyTo

        /// <summary>
        /// Copies items from the list to the specified array starting at index location in the specified array.
        /// </summary>
        /// <param name="array">The array to which to copy items.</param>
        /// <param name="index">The location in the array at which to start copying.</param>
        public void CopyTo(T[] array, int index)
        {
            ((ICollection)this).CopyTo(array, index);
        }

        #endregion // CopyTo

        #region GetEnumerator
        /// <summary>
        /// Returns an enumerator for iterating the live items.
        /// </summary>
        /// <returns>An enumerator for the <see cref="WeakList&lt;T&gt;"/></returns>
        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(this);
        } 
        #endregion //GetEnumerator

        #region IndexOf

        /// <summary>
        /// Returns the index of the specified item. If the item doesn't exist then returns -1.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public int IndexOf(T value)
        {
            for (int i = 0, count = _list.Count; i < count; i++)
            {
                if (value == this.GetItemHelper(i))
                    return i;
            }

            return -1;
        }

        #endregion // IndexOf

        #region Insert

        /// <summary>
        /// Inserts the specified item at the specified location in the list.
        /// </summary>
        /// <param name="index">The location at which to insert the item.</param>
        /// <param name="item">The item to insert.</param>
        public void Insert(int index, T item)
        {
            _list.Insert(index, this.CreateWeakReference(item));
        }

        #endregion // Insert

        #region Remove

        /// <summary>
        /// Removes the first occurrence of the specified item from the list. If the item doesn't 
        /// exist in the list then this method does nothing.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        public bool Remove(T item)
        {
            int index = this.IndexOf(item);
            if (index >= 0)
            {
                this.RemoveAt(index);
                return true;
            }

            return false;
        }

        #endregion // Remove

        #region RemoveAt

        /// <summary>
        /// Removes item at specified index.
        /// </summary>
        /// <param name="index">Index of the item to remove.</param>
        public void RemoveAt(int index)
        {
            _list.RemoveAt(index);
        }

        #endregion // RemoveAt

		#region RemoveRange
		/// <summary>
		/// Removes a contiguous block of items from the collection.
		/// </summary>
		/// <param name="index">The zero-based starting index of the range of elements to remove.</param>
		/// <param name="count">The number of elements to remove</param>
		public void RemoveRange(int index, int count)
		{
			_list.RemoveRange(index, count);
		} 
		#endregion // RemoveRange

        #endregion // Public Methods

        #region Private/Internal Methods

        #region CreateWeakReference

        private WeakReference CreateWeakReference(object item)
        {
            return new WeakReference(item);
        }

        #endregion // CreateWeakReference

        #region GetItemHelper

        private T GetItemHelper(int index)
        {
            WeakReference ww = _list[index];
            T target = null;
            if (null != ww)
            {
                target = (T)CoreUtilities.GetWeakReferenceTargetSafe(ww);
                if (null == target)
                    _list[index] = null;
            }

            return target;
        }

        #endregion // GetItemHelper

        #endregion // Private/Internal Methods

        #endregion // Methods

        #region Explicit IList Implementations

        #region IEnumerable.GetEnumerator

        /// <summary>
        /// Gets the enumerator for enumerating this list.
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }

        #endregion // IEnumerable.GetEnumerator

        #region IList.this[int]
        object IList.this[int index]
        {
            get { return this[index]; }
            set { this[index] = (T)value; }
        } 
        #endregion //IList.this[int]

        #region ICollection<T>.Add
        void ICollection<T>.Add(T item)
        {
            this.Add(item);
        } 
        #endregion //ICollection<T>.Add

        #region ICollection<T>.IsReadOnly

        /// <summary>
        /// Returns true if the list is read-only.
        /// </summary>
        bool ICollection<T>.IsReadOnly
        {
            get { return false; }
        }

        #endregion // ICollection<T>.IsReadOnly

        #region IList.Add
        int IList.Add(object item)
        {
            return this.Add((T)item);
        } 
        #endregion //IList.Add

        #region IList.Contains
        bool IList.Contains(object item)
        {
            return item is T && this.Contains((T)item);
        }
        #endregion //IList.Contains

        #region IList.Contains
        void ICollection.CopyTo(Array array, int index)
        {
            for (int i = 0, count = _list.Count; i < count; i++)
            {
                array.SetValue(this.GetItemHelper(i), index++);
            }
        }
        #endregion //IList.Contains

        #region IList.IndexOf
        int IList.IndexOf(object item)
        {
            return item is T ? this.IndexOf((T)item) : -1;
        }
        #endregion //IList.IndexOf

        #region IList.Insert
        void IList.Insert(int index, object item)
        {
            this.Insert(index, (T)item);
        }
        #endregion //IList.Insert

        #region IList.IsFixedSize

        /// <summary>
        /// Returns true if the list is fixed size.
        /// </summary>
        bool IList.IsFixedSize
        {
            get { return false; }
        }

        #endregion // IList.IsFixedSize

        #region IList.IsReadOnly

        /// <summary>
        /// Returns true if the list is read-only.
        /// </summary>
        bool IList.IsReadOnly
        {
            get { return false; }
        }

        #endregion // IList.IsReadOnly

        #region IList.Remove
        void IList.Remove(object item)
        {
            if (item is T)
                this.Remove((T)item);
        }
        #endregion //IList.Remove

        #region ICollection.IsSynchronized

        /// <summary>
        /// Returns true if this list is syncrhonized.
        /// </summary>
        bool ICollection.IsSynchronized
        {
            // AS 9/30/08 Optimization
            //get { return _list.IsSynchronized; }
            get { return ((ICollection)_list).IsSynchronized; }
        }

        #endregion // ICollection.IsSynchronized

        #endregion // Explicit IList Implementations
    }

	

	/// <summary>
	/// List class that manages items using WeakReferences so the items in this list
	/// can be garbage collected. Items collected by garbage collector will be replaced
    /// by null. The <see cref="WeakList&lt;T&gt;.Compact"/> method can be used to remove entries
	/// from the list that have been garbage collected.
	/// </summary>
	public class WeakList : WeakList<object>
	{
        /// <summary>
        /// Initializes a new <see cref="WeakList"/>
        /// </summary>
        public WeakList()
        {
        }
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