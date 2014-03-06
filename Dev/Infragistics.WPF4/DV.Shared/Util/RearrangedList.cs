using System.Collections.Generic;
using System;

namespace Infragistics
{
    /// <summary>
    /// List class for known lists which are reordered.
    /// </summary>
    /// <typeparam name="T">The Type of items in the list.</typeparam>
    public class RearrangedList<T>
        : IList<T>
    {
        private IList<T> _inner;
        private IList<int> _indexes;
        /// <summary>
        /// RearrangedList constructor.
        /// </summary>
        /// <param name="inner">The original list.</param>
        /// <param name="indexes">A list of indices, representing the order of items in the RearrangedList.</param>
        public RearrangedList(IList<T> inner, IList<int> indexes)
        {
            _inner = inner;
            _indexes = indexes;
        }
        /// <summary>
        /// Gets the index of the given item.
        /// </summary>
        /// <param name="item">The item under observation.</param>
        /// <returns>The index of the given item.</returns>
        public int IndexOf(T item)
        {
            var innerIndex = _inner.IndexOf(item);
            if (innerIndex == -1)
            {
                return -1;
            }
            return _indexes.IndexOf(innerIndex);
        }
        /// <summary>
        /// Inserts an item into the collection at the specified index.
        /// </summary>
        /// <param name="index">The index at which to insert the item.</param>
        /// <param name="item">The item to insert.</param>        
        public void Insert(int index, T item)
        {
            throw new System.NotImplementedException();
        }
        /// <summary>
        /// Removes the item at the specified index.
        /// </summary>
        /// <param name="index">The index of the item to remove.</param>
        public void RemoveAt(int index)
        {
            throw new System.NotImplementedException();
        }
        /// <summary>
        /// RearrangedList indexer.
        /// </summary>
        /// <param name="index">The index of the item to get or set.</param>
        /// <returns>The item at the specified index.</returns>
        public T this[int index]
        {
            get
            {
                return _inner[_indexes[index]];
            }
            set
            {
                throw new System.NotImplementedException();
            }
        }
        /// <summary>
        /// Adds an item to the collection.
        /// </summary>
        /// <param name="item">The item to add.</param>
        public void Add(T item)
        {
            throw new System.NotImplementedException();
        }
        /// <summary>
        /// Clears the collection.
        /// </summary>
        public void Clear()
        {
            _indexes.Clear();
        }
        /// <summary>
        /// Checks whether or not the item is present in the collection.
        /// </summary>
        /// <param name="item">The item under observation.</param>
        /// <returns>True if the item is present in the collection, otherwise False.</returns>
        public bool Contains(T item)
        {
            return _inner.Contains(item);
        }
        /// <summary>
        /// Copies the items in the collection to the array, starting at the specified index.
        /// </summary>
        /// <param name="array">The array to add collection items to.</param>
        /// <param name="arrayIndex">The index at which to start the copy operation.</param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new System.NotImplementedException();
        }
        /// <summary>
        /// The total number of items in the collection.
        /// </summary>
        public int Count
        {
            get { return _indexes.Count; }
        }
        /// <summary>
        /// Boolean indicating whether or not the collection is read-only.
        /// </summary>
        public bool IsReadOnly
        {
            get { return true; }
        }
        /// <summary>
        /// Removes the specified item from the collection.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        /// <returns>True if the item was found and removed, otherwise False.</returns>
        public bool Remove(T item)
        {
            throw new System.NotImplementedException();
        }
        /// <summary>
        /// Gets the enumerator for iterating through all items in the collection.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<T> GetEnumerator()
        {
            foreach (var ind in _indexes)
            {
                yield return _inner[ind];
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach (var ind in _indexes)
            {
                yield return _inner[ind];
            }
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