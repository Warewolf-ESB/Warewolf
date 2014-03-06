using System;
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
using System.Collections.ObjectModel;

namespace Infragistics.Controls.Charts.Util
{
    /// <summary>
    /// Creates a wrapper around a collection that asserts that access to the items is readonly
    /// and that Double.NaN and Double.(Positive/Negative)Infinity values are coerced to safe
    /// values for calculation and rendering.
    /// </summary>
    public class SafeReadOnlyDoubleCollection
        : IList<double>
    {
        private ReadOnlyCollection<double> _target;

        internal double MakeSafe(double value)
        {
            if (double.IsInfinity(value) ||
                double.IsNaN(value))
            {
                return 0.0;
            }

            return value;
        }

        /// <summary>
        /// Constructs a new SafeReadOnlyDoubleCollection
        /// </summary>
        /// <param name="target">The target collection to wrap.</param>
        public SafeReadOnlyDoubleCollection(IList<double> target)
        {
            this._target = new ReadOnlyCollection<double>(target);
        }

        #region IList<double> Members

        /// <summary>
        /// Returns the index of the given item.
        /// </summary>
        /// <param name="item">The item to search for.</param>
        /// <returns>The index of the item.</returns>
        public int IndexOf(double item)
        {
            return _target.IndexOf(item);
        }

        /// <summary>
        /// Inserts the given item as the specified index.
        /// </summary>
        /// <param name="index">The index at which to insert the item.</param>
        /// <param name="item">The item to insert.</param>
        /// <remarks>
        /// Will fail, as this collection is read-only. 
        /// Implemented only to satisfy the IList interface.
        /// </remarks>
        public void Insert(int index, double item)
        {
            (_target as IList<double>).Insert(index, item);
        }

        /// <summary>
        /// Removes the item at the specified index.
        /// </summary>
        /// <param name="index">The index from which to remove the item.</param>
        /// <remarks>
        /// Will fail, as this collection is read-only. 
        /// Implemented only to satisfy the IList interface.
        /// </remarks>
        public void RemoveAt(int index)
        {
            (_target as IList<double>).RemoveAt(index);
        }

        /// <summary>
        /// Gets or sets the item at the specified index.
        /// </summary>
        /// <param name="index">The index at which to get or set the item.</param>
        /// <returns>The item requested.</returns>
        /// <remarks>
        /// The set will fail, as this collection is read-only. 
        /// Implemented only to satisfy the IList interface.
        /// </remarks>
        public double this[int index]
        {
            get
            {
                return MakeSafe(_target[index]);
            }
            set
            {
                (_target as IList<double>)[index] = value;
            }
        }

        #endregion

        #region ICollection<double> Members

        /// <summary>
        /// Adds a new item to the collection.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <remarks>
        /// Will fail, as this collection is read-only. 
        /// Implemented only to satisfy the ICollection interface.
        /// </remarks>
        public void Add(double item)
        {
            (_target as IList<double>).Add(item);
        }

        /// <summary>
        /// Clears the items from the collection.
        /// </summary>
        /// <remarks>
        /// Will fail, as this collection is read-only. 
        /// Implemented only to satisfy the ICollection interface.
        /// </remarks>
        public void Clear()
        {
            (_target as IList<double>).Clear();
        }

        /// <summary>
        /// Determines whether the collection contains the provided item.
        /// </summary>
        /// <param name="item">The item to check for.</param>
        /// <returns>True if the item is found in the collection.</returns>
        public bool Contains(double item)
        {
            return _target.Contains(item);
        }

        /// <summary>
        /// Copies the collection to the provided array, starting at the specified index.
        /// </summary>
        /// <param name="array">The array to copy to.</param>
        /// <param name="arrayIndex">The starting index.</param>
        public void CopyTo(double[] array, int arrayIndex)
        {
            for (int i = arrayIndex; i < array.Length; i++)
            {
                array[i] = this[i];
            }
        }

        /// <summary>
        /// Gets the number of items in the collection.
        /// </summary>
        public int Count
        {
            get { return _target.Count; }
        }

        /// <summary>
        /// Gets whether the collection is readonly.
        /// </summary>
        public bool IsReadOnly
        {
            get { return (_target as IList<double>).IsReadOnly; }
        }

        /// <summary>
        /// Removes the provided item from the collection.
        /// </summary>
        /// <param name="item">The item to remove from the collection.</param>
        /// <returns>True if the item was removed from the collection.</returns>
        /// <remarks>
        /// Will fail, as this collection is read-only. 
        /// Implemented only to satisfy the ICollection interface.
        /// </remarks>
        public bool Remove(double item)
        {
            return (_target as IList<double>).Remove(item);
        }

        #endregion

        #region IEnumerable<double> Members

        /// <summary>
        /// Gets an enumerator for the collection.
        /// </summary>
        /// <returns>The enumerator instance.</returns>
        public IEnumerator<double> GetEnumerator()
        {
            return new SafeEnumerable(_target).GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// Gets an enumerator for the collection.
        /// </summary>
        /// <returns>The enumerator instance.</returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion
    }

    /// <summary>
    /// Creates a wrapper around a collection that asserts that access to the items is readonly
    /// and that Double.NaN and Double.(Positive/Negative)Infinity values are coerced to safe
    /// values for calculation and rendering. In addition, it will emulate that the items are sorted
    /// based on a sorted index map provided upon creation.
    /// </summary>
    public class SafeSortedReadOnlyDoubleCollection
        : IList<double>
    {
        private SafeReadOnlyDoubleCollection _target;
        private IList<int> _sortedIndices;

        internal double MakeSafe(double value)
        {
            if (double.IsInfinity(value) ||
                double.IsNaN(value))
            {
                return 0.0;
            }

            return value;
        }

        /// <summary>
        /// Constructs a new SafeSortedReadOnlyDoubleCollection instance.
        /// </summary>
        /// <param name="target">The target collection to wrap.</param>
        /// <param name="sortedIndices">The unsorted indexes of the target collection if it were sorted.</param>
        public SafeSortedReadOnlyDoubleCollection(IList<double> target, IList<int> sortedIndices)
        {
            this._target = new SafeReadOnlyDoubleCollection(target);
            _sortedIndices = sortedIndices;
        }

        #region IList<double> Members

        /// <summary>
        /// Returns the index of the given item.
        /// </summary>
        /// <param name="item">The item to search for.</param>
        /// <returns>The index of the item.</returns>
        public int IndexOf(double item)
        {
            int innerIndex = _target.IndexOf(item);
            return _sortedIndices.IndexOf(innerIndex);
        }

        /// <summary>
        /// Inserts the given item as the specified index.
        /// </summary>
        /// <param name="index">The index at which to insert the item.</param>
        /// <param name="item">The item to insert.</param>
        /// <remarks>
        /// Will fail, as this collection is read-only. 
        /// Implemented only to satisfy the IList interface.
        /// </remarks>
        public void Insert(int index, double item)
        {
            //don't care that the index is wrong, this will get thrown as not supported
            //by inner.
            (_target as IList<double>).Insert(index, item);
        }

        /// <summary>
        /// Removes the item at the specified index.
        /// </summary>
        /// <param name="index">The index from which to remove the item.</param>
        /// <remarks>
        /// Will fail, as this collection is read-only. 
        /// Implemented only to satisfy the IList interface.
        /// </remarks>
        public void RemoveAt(int index)
        {
            //don't care that the index is wrong, this will get thrown as not supported
            //by inner
            (_target as IList<double>).RemoveAt(index);
        }

        /// <summary>
        /// Gets or sets the item at the specified index.
        /// </summary>
        /// <param name="index">The index at which to get or set the item.</param>
        /// <returns>The item requested.</returns>
        /// <remarks>
        /// The set will fail, as this collection is read-only. 
        /// Implemented only to satisfy the IList interface.
        /// </remarks>
        public double this[int index]
        {
            get
            {
                int innerIndex = _sortedIndices[index];
                return MakeSafe(_target[innerIndex]);
            }
            set
            {
                //don't care that the index is wrong, this will get thrown as not supported
                //by inner
                (_target as IList<double>)[index] = value;
            }
        }

        #endregion

        #region ICollection<double> Members

        /// <summary>
        /// Adds a new item to the collection.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <remarks>
        /// Will fail, as this collection is read-only. 
        /// Implemented only to satisfy the ICollection interface.
        /// </remarks>
        public void Add(double item)
        {
            //don't care that the index is wrong, this will get thrown as not supported
            //by inner
            (_target as IList<double>).Add(item);
        }

        /// <summary>
        /// Clears the items from the collection.
        /// </summary>
        /// <remarks>
        /// Will fail, as this collection is read-only. 
        /// Implemented only to satisfy the ICollection interface.
        /// </remarks>
        public void Clear()
        {
            //don't care that the index is wrong, this will get thrown as not supported
            //by inner
            (_target as IList<double>).Clear();
        }

        /// <summary>
        /// Determines whether the collection contains the provided item.
        /// </summary>
        /// <param name="item">The item to check for.</param>
        /// <returns>True if the item is found in the collection.</returns>
        public bool Contains(double item)
        {
            return _target.Contains(item);
        }

        /// <summary>
        /// Copies the collection to the provided array, starting at the specified index.
        /// </summary>
        /// <param name="array">The array to copy to.</param>
        /// <param name="arrayIndex">The starting index.</param>
        public void CopyTo(double[] array, int arrayIndex)
        {
            for (int i = arrayIndex; i < array.Length; i++)
            {
                array[i] = this[i];
            }
        }

        /// <summary>
        /// Gets the number of items in the collection.
        /// </summary>
        public int Count
        {
            get { return _target.Count; }
        }

        /// <summary>
        /// Gets whether the collection is readonly.
        /// </summary>
        public bool IsReadOnly
        {
            get { return (_target as IList<double>).IsReadOnly; }
        }

        /// <summary>
        /// Removes the provided item from the collection.
        /// </summary>
        /// <param name="item">The item to remove from the collection.</param>
        /// <returns>True if the item was removed from the collection.</returns>
        /// <remarks>
        /// Will fail, as this collection is read-only. 
        /// Implemented only to satisfy the ICollection interface.
        /// </remarks>
        public bool Remove(double item)
        {
            return (_target as IList<double>).Remove(item);
        }

        #endregion

        #region IEnumerable<double> Members

        /// <summary>
        /// Gets an enumerator for the collection.
        /// </summary>
        /// <returns>The enumerator instance.</returns>
        public IEnumerator<double> GetEnumerator()
        {
            for (int i = 0; i < _target.Count; i++)
            {
                yield return this[i];
            }
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// Gets an enumerator for the collection.
        /// </summary>
        /// <returns>The enumerator instance.</returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion
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