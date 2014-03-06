using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;

namespace Infragistics.Controls.Interactions.Primitives
{
	/// <summary>
	/// A collection that can be filtered based on any predicate.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class FilteredCollection<T> : IList<T>, INotifyCollectionChanged
	{
		#region Member Variables

		private List<T> _allItems;
		private List<T> _filteredItems;

		#endregion  // Member Variables

		#region Constructor

		/// <summary>
		/// Creates a new <see cref="FilteredCollection&lt;T&gt;"/> instance.
		/// </summary>
		/// <param name="allItems"></param>
		public FilteredCollection(List<T> allItems)
		{
			_allItems = allItems;
			this.ApplyFilter(null);
		}

		#endregion  // Constructor

		#region Interfaces

		#region IList<T> Members

		/// <summary>
		/// Returns the index of the item in the filtered collection.
		/// </summary>
		/// <param name="item">The item of which to get the index.</param>
		/// <returns>The 0-based index of the item in the filtered collection, or -1 if it doesn't exist.</returns>
		public int IndexOf(T item)
		{
			return _filteredItems.IndexOf(item);
		}

		void IList<T>.Insert(int index, T item)
		{
			CoreUtilities.RaiseReadOnlyCollectionException();
		}

		void IList<T>.RemoveAt(int index)
		{
			CoreUtilities.RaiseReadOnlyCollectionException();
		}

		/// <summary>
		/// Gets the item at the specified index in the filtered collection.
		/// </summary>
		/// <param name="index">The 0-based index.</param>
		/// <returns>The item at the specified index in the filtered collection.</returns>
		public T this[int index]
		{
			get { return _filteredItems[index]; }
		}

		T IList<T>.this[int index]
		{
			get { return this[index]; }
			set { CoreUtilities.RaiseReadOnlyCollectionException(); }
		}

		#endregion

		#region ICollection<T> Members

		void ICollection<T>.Add(T item)
		{
			CoreUtilities.RaiseReadOnlyCollectionException();
		}

		void ICollection<T>.Clear()
		{
			CoreUtilities.RaiseReadOnlyCollectionException();
		}

		/// <summary>
		/// Returns whether the filtered collection contains the specified item.
		/// </summary>
		/// <param name="item">The item to search for in the filtered collection.</param>
		/// <returns>True if the filtered collection contains the item; False otherwise.</returns>
		public bool Contains(T item)
		{
			return _filteredItems.Contains(item);
		}

		void ICollection<T>.CopyTo(T[] array, int arrayIndex)
		{
			_filteredItems.CopyTo(array, arrayIndex);
		}

		/// <summary>
		/// Gets the number of items in the filtered collection.
		/// </summary>
		public int Count
		{
			get { return _filteredItems.Count; }
		}

		bool ICollection<T>.IsReadOnly
		{
			get { return true; }
		}

		bool ICollection<T>.Remove(T item)
		{
			CoreUtilities.RaiseReadOnlyCollectionException();
			return false;
		}

		#endregion

		#region IEnumerable<T> Members

		/// <summary>
		/// Returns an enumerator that iterates through the filtered collection.
		/// </summary>
		/// <returns></returns>
		public IEnumerator<T> GetEnumerator()
		{
			return _filteredItems.GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return _filteredItems.GetEnumerator();
		}

		#endregion

		#region INotifyCollectionChanged Members

		/// <summary>
		/// Occurs when the collection's filter is changed.
		/// </summary>
		public event NotifyCollectionChangedEventHandler CollectionChanged;

		#endregion

		#endregion  // Interfaces

		#region Methods

		#region ApplyFilter

		/// <summary>
		/// Filters the items with the specified predicate, or null to remove the filter.
		/// </summary>
		/// <param name="filterPredicate">The predicate by which to filter the list of items.</param>
		public void ApplyFilter(Func<T, bool> filterPredicate)
		{
			if (filterPredicate == null)
				_filteredItems = _allItems;
			else
				_filteredItems = _allItems.Where(filterPredicate).ToList();

			this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		#endregion  // ApplyFilter

		#region OnCollectionChanged

		private void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			var handler = this.CollectionChanged;
			if (handler != null)
				handler(this, e);
		}

		#endregion  // OnCollectionChanged

		#endregion  // Methods

		#region Properties

		#region AllItems

		internal List<T> AllItems
		{
			get { return _allItems; }
		}

		#endregion  // AllItems

		#endregion  // Properties
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