using System;
using System.Collections;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;

namespace Infragistics.Controls.Charts.Util
{
    /// <summary>
    /// Used to get a sorted view of an unsorted list, based on the sorted indices for the list.
    /// </summary>
    /// <typeparam name="T">The item type</typeparam>
    public class SortedListView<T>
        : IList<T>
    {
        private IList<int> _sortedIndices = null;
        private IList<T> _source = null;

        /// <summary>
        /// Constructs an instance of a SortedListView
        /// </summary>
        /// <param name="source">The unsorted collection.</param>
        /// <param name="sortedIndices">The indexes in order if the list were sorted.</param>
        public SortedListView(IList<T> source, IList<int> sortedIndices)
        {
            _sortedIndices = sortedIndices;
            _source = source;
        }

        /// <summary>
        /// Adds and item to the collection.
        /// </summary>
        /// <param name="value">The item to add.</param>
        /// <returns></returns>
        /// <remarks>Not supported because view is read only.</remarks>



        public int Add(object value)

        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Clears the collection.
        /// </summary>
        /// <remarks>Not supported because view is read only.</remarks>
        public void Clear()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Determines if the collection contains the provided item.
        /// </summary>
        /// <param name="value">The item to check for.</param>
        /// <returns>true, if the list contains the item.</returns>
        public bool Contains(T value)
        {
            return _source.Contains(value);
        }

        /// <summary>
        /// Returns the index of the provided value in the collection.
        /// </summary>
        /// <param name="value">The value to find.</param>
        /// <returns>The index of the value.</returns>
        public int IndexOf(T value)
        {
            return _sortedIndices.IndexOf(_source.IndexOf(value));
        }

        /// <summary>
        /// Inserts the item at the specified index.
        /// </summary>
        /// <param name="index">The index to insert the item at.</param>
        /// <param name="value">The item to insert.</param>
        /// <remarks>Not supported because the view is read only.</remarks>
        public void Insert(int index, T value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns if the view has a fixed size. 
        /// </summary>
        public bool IsFixedSize
        {
            get { return true; }
        }

        /// <summary>
        /// Returns if the view is read only.
        /// </summary>
        public bool IsReadOnly
        {
            get { return true; }
        }

        /// <summary>
        /// Removes the specified value from the collection.
        /// </summary>
        /// <param name="value">The value to remove</param>
        /// <remarks>Not supported because the view is read only.</remarks>
        public void Remove(T value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Removes the item at the specified index.
        /// </summary>
        /// <param name="index">The index of the item to remove.</param>
        /// <remarks>Not supported because the view is read only.</remarks>
        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets or sets the item at the specified index.
        /// </summary>
        /// <param name="index">The index to get or set from.</param>
        /// <returns>The item requested</returns>
        /// <remarks>Only the getter is supported as the view is read only.</remarks>
        public T this[int index]
        {
            get
            {
                return _source[_sortedIndices[index]];
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Returns the count of the items in the view.
        /// </summary>
        public int Count
        {
            get { return _source.Count; }
        }

        /// <summary>
        /// Returns whether the collection is synchronized.
        /// </summary>
        public bool IsSynchronized
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Returns the synchronization root to use to synchronize the collection.
        /// </summary>
        public object SyncRoot
        {
            get { throw new NotImplementedException(); }
        }

        //private IEnumerable Enumerate()
        //{
        //    for (int i = 0; i < Count; i++)
        //    {
        //        yield return this[i];
        //    }
        //}

        /// <summary>
        /// Gets the enumerator for iterating through the collection.
        /// </summary>
        /// <returns>The enumerator requested.</returns>
        public IEnumerator GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return this[i];
            }
        }

        /// <summary>
        /// Adds an item to the collection.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <remarks>Not supported because the view is read only.</remarks>
        void ICollection<T>.Add(T item)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Removes an item from the collection.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        /// <remarks>Not supported because the view is read only.</remarks>
        bool ICollection<T>.Remove(T item)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the enumerator for iterating through the collection.
        /// </summary>
        /// <returns>The enumerator requested.</returns>
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return this[i];
            }
        }        

        /// <summary>
        /// Copies the collection to an array.
        /// </summary>
        /// <param name="array">The array to copy to.</param>
        /// <param name="arrayIndex">The starting index in the array.</param>
        /// <remarks>Not supported.</remarks>
        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
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