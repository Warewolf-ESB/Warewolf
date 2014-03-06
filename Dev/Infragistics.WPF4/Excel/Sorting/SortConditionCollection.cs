using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using System.Security;
using System.ComponentModel;
using System.Collections;




using Infragistics.Shared;


namespace Infragistics.Documents.Excel.Sorting
{
	// MD 12/14/11 - 12.1 - Table Support



	/// <summary>
	/// An ordered collection of sort conditions which are applied to a sort-able regions in a worksheet.
	/// </summary>
	/// <remarks>
	/// <p class="body">
	/// The sort conditions in the collection are applied in order to the data.
	/// </p>
	/// <p class="note">
	/// <B>Note:</B> The collection can hold a maximum of 64 sort condition.
	/// </p>
	/// </remarks>
	/// <typeparam name="T">
	/// A type which logically contains data and can have sort condition applied to that data.
	/// </typeparam>
	/// <seealso cref="SortSettings&lt;T&gt;.SortConditions"/>
	[DebuggerDisplay("SortConditionCollection: Count - {Count}")]
	[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelTables)]
	public

 class SortConditionCollection<T> :
		IDictionary<T, SortCondition>, IList<KeyValuePair<T, SortCondition>>
		where T : ISortable
	{
		#region Constants

		private const int MaxCount = 64;

		#endregion // Constants

		#region Member Variables

		private KeysCollection _keys;
		private SortSettings<T> _owner;
		private List<KeyValuePair<T, SortCondition>> _sortConditons = new List<KeyValuePair<T, SortCondition>>();
		private ValuesCollection _values;

		#endregion // Member Variables

		#region Constructor

		internal SortConditionCollection(SortSettings<T> owner)
		{
			_owner = owner;
		}

		#endregion // Constructor

		#region Interfaces

		#region IDictionary<T,SortCondition> Members

		bool IDictionary<T, SortCondition>.ContainsKey(T key)
		{
			return this.Contains(key);
		}

		ICollection<T> IDictionary<T, SortCondition>.Keys
		{
			get
			{
				if (_keys == null)
					_keys = new KeysCollection(this);

				return _keys;
			}
		}

		bool IDictionary<T, SortCondition>.TryGetValue(T key, out SortCondition value)
		{
			value = this[key];
			return value != null;
		}

		ICollection<SortCondition> IDictionary<T, SortCondition>.Values
		{
			get
			{
				if (_values == null)
					_values = new ValuesCollection(this);

				return _values;
			}
		}

		#endregion

		#region IList<KeyValuePair<T,SortCondition>> Members

		int IList<KeyValuePair<T, SortCondition>>.IndexOf(KeyValuePair<T, SortCondition> item)
		{
			int index = this.IndexOf(item.Key);
			if (index < 0)
				return index;

			if (_sortConditons[index].Value != item.Value)
				return -1;

			return index;
		}

		void IList<KeyValuePair<T, SortCondition>>.Insert(int index, KeyValuePair<T, SortCondition> item)
		{
			this.Insert(index, item.Key, item.Value);
		}

		#endregion

		#region ICollection<KeyValuePair<T,SortCondition>> Members

		void ICollection<KeyValuePair<T, SortCondition>>.Add(KeyValuePair<T, SortCondition> item)
		{
			this.Add(item.Key, item.Value);
		}

		bool ICollection<KeyValuePair<T, SortCondition>>.Contains(KeyValuePair<T, SortCondition> item)
		{
			int index = this.IndexOf(item.Value);
			if (index < 0)
				return false;

			return Object.Equals(this[index].Key, item.Key);
		}

		void ICollection<KeyValuePair<T, SortCondition>>.CopyTo(KeyValuePair<T, SortCondition>[] array, int arrayIndex)
		{
			_sortConditons.CopyTo(array, arrayIndex);
		}

		bool ICollection<KeyValuePair<T, SortCondition>>.IsReadOnly
		{
			get { return false; }
		}

		bool ICollection<KeyValuePair<T, SortCondition>>.Remove(KeyValuePair<T, SortCondition> item)
		{
			int index = this.IndexOf(item.Value);
			if (index < 0)
				return false;

			if (Object.Equals(this[index].Key, item.Key) == false)
				return false;

			this.RemoveAt(index);
			return true;
		}

		#endregion

		#region IEnumerable<KeyValuePair<T,SortCondition>> Members

		IEnumerator<KeyValuePair<T, SortCondition>> IEnumerable<KeyValuePair<T, SortCondition>>.GetEnumerator()
		{
			return _sortConditons.GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _sortConditons.GetEnumerator();
		}

		#endregion

		#endregion // Interfaces

		#region Methods

		#region Public Methods

		#region Add

		/// <summary>
		/// Adds a sort condition to the collection.
		/// </summary>
		/// <param name="sortableItem">The sort-able item over which the sort condition will be applied.</param>
		/// <param name="sortCondition">The sort condition to apply to the sort-able item.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="sortableItem"/> is null.
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="sortCondition"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="sortableItem"/> is already has a sort condition applied to it in the collection.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// There are already 64 sort conditions in the collection.
		/// </exception>
		public void Add(T sortableItem, SortCondition sortCondition)
		{
			this.ValidateNewPair(sortableItem, sortCondition);

			_sortConditons.Add(new KeyValuePair<T, SortCondition>(sortableItem, sortCondition));
			this.OnSortConditionsModified();
		}

		#endregion // Add

		#region Clear

		/// <summary>
		/// Clears the collection.
		/// </summary>
		public void Clear()
		{
			if (_sortConditons.Count == 0)
				return;

			_sortConditons.Clear();
			this.OnSortConditionsModified();
		}

		#endregion // Clear

		#region Contains

		/// <summary>
		/// Determines whether the specified sort-able item is in the collection.
		/// </summary>
		/// <param name="sortableItem">The sort-able item to find in the collection.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="sortableItem"/> is null.
		/// </exception>
		/// <returns>True if the sort-able item is in the collection; False otherwise.</returns>
		public bool Contains(T sortableItem)
		{
			return this.IndexOf(sortableItem) >= 0;
		}

		/// <summary>
		/// Determines whether the specified sort condition is in the collection.
		/// </summary>
		/// <param name="sortCondition">The sort condition to find in the collection.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="sortCondition"/> is null.
		/// </exception>
		/// <returns>True if the sort condition is in the collection; False otherwise.</returns>
		public bool Contains(SortCondition sortCondition)
		{
			return this.IndexOf(sortCondition) >= 0;
		}

		#endregion // Contains

		#region IndexOf

		/// <summary>
		/// Gets the index of the specified sort-able item in the collection.
		/// </summary>
		/// <param name="sortableItem">The sort-able item to find in the collection.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="sortableItem"/> is null.
		/// </exception>
		/// <returns>
		/// The 0-based index of the specified sort-able item in the collection or -1 if the item is not in the collection.
		/// </returns>
		public int IndexOf(T sortableItem)
		{
			if (sortableItem == null)
				throw new ArgumentNullException("sortableItem");

			for (int i = 0; i < _sortConditons.Count; i++)
			{
				KeyValuePair<T, SortCondition> pair = _sortConditons[i];
				if (Object.ReferenceEquals(pair.Key, sortableItem))
					return i;
			}

			return -1;
		}

		/// <summary>
		/// Gets the index of the specified sort condition in the collection.
		/// </summary>
		/// <param name="sortCondition">The sort condition to find in the collection.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="sortCondition"/> is null.
		/// </exception>
		/// <returns>
		/// The 0-based index of the specified sort condition in the collection or -1 if the item is not in the collection.
		/// </returns>
		public int IndexOf(SortCondition sortCondition)
		{
			if (sortCondition == null)
				throw new ArgumentNullException("sortCondition");

			for (int i = 0; i < _sortConditons.Count; i++)
			{
				KeyValuePair<T, SortCondition> pair = _sortConditons[i];
				if (pair.Value == sortCondition)
					return i;
			}

			return -1;
		}

		#endregion // IndexOf

		#region Insert

		/// <summary>
		/// Inserts a sort condition into the collection.
		/// </summary>
		/// <param name="index">The 0-based index where the sort condition should be inserted.</param>
		/// <param name="sortableItem">The sort-able item over which the sort condition will be applied.</param>
		/// <param name="sortCondition">The sort condition to apply to the sort-able item.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="index"/> is less than 0 or greater than <see cref="Count"/>.
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="sortableItem"/> is null.
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="sortCondition"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="sortableItem"/> is already has a sort condition applied to it in the collection.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// There are already 64 sort conditions in the collection.
		/// </exception>
		public void Insert(int index, T sortableItem, SortCondition sortCondition)
		{
			this.ValidateNewPair(sortableItem, sortCondition);

			_sortConditons.Insert(index, new KeyValuePair<T, SortCondition>(sortableItem, sortCondition));
			this.OnSortConditionsModified();
		}

		#endregion // Insert

		#region Remove

		/// <summary>
		/// Removes a sort-able item from the collection.
		/// </summary>
		/// <param name="sortableItem">The sort-able item to remove from the collection.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="sortableItem"/> is null.
		/// </exception>
		/// <returns>True if the sort-able item was found and removed; False otherwise.</returns>
		public bool Remove(T sortableItem)
		{
			if (sortableItem == null)
				throw new ArgumentNullException("sortableItem");

			int index = this.IndexOf(sortableItem);
			if (index < 0)
				return false;

			this.RemoveAt(index);
			return true;
		}

		/// <summary>
		/// Removes a sort condition from the collection.
		/// </summary>
		/// <param name="sortCondition">The sort condition to remove from the collection.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="sortCondition"/> is null.
		/// </exception>
		/// <returns>True if the sort condition was found and removed; False otherwise.</returns>
		public bool Remove(SortCondition sortCondition)
		{
			if (sortCondition == null)
				throw new ArgumentNullException("sortCondition");

			int index = this.IndexOf(sortCondition);
			if (index < 0)
				return false;

			this.RemoveAt(index);
			return true;
		}

		#endregion // Remove

		#region RemoveAt

		/// <summary>
		/// Removes the sort condition at the specified index.
		/// </summary>
		/// <param name="index">The 0-based index of the sort condition to remove.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="index"/> is less than 0 or greater than or equal to <see cref="Count"/>.
		/// </exception>
		public void RemoveAt(int index)
		{
			if (index < 0 || _sortConditons.Count <= index)
				throw new ArgumentOutOfRangeException("index", index, SR.GetString("LE_ArgumentOutOfRangeException_InvalidRemoveAtIndex_SortConditions"));

			_sortConditons.RemoveAt(index);
			this.OnSortConditionsModified();
		}

		#endregion // RemoveAt

		#endregion // Public Methods

		#region Private Methods

		#region OnSortConditionsModified

		private void OnSortConditionsModified()
		{
			_owner.OnSortSettingsModified();
		}

		#endregion // OnSortConditionsModified

		#region ValidateNewPair

		private void ValidateNewPair(T sortableItem, SortCondition sortCondition)
		{
			if (sortableItem == null)
				throw new ArgumentNullException("sortableItem");

			if (sortCondition == null)
				throw new ArgumentNullException("sortCondition");

			if (this.Contains(sortableItem))
				throw new ArgumentException(SR.GetString("LE_ArgumentOutOfRangeException_DuplicateItemSorted"), "sortableItem");

			if (this.Count == SortConditionCollection<T>.MaxCount)
				throw new InvalidOperationException(SR.GetString("LE_ArgumentOutOfRangeException_MaxSortConditions", SortConditionCollection<T>.MaxCount));
		}

		#endregion // ValidateNewPair

		#endregion // Private Methods

		#endregion // Methods

		#region Properties

		#region Public Properties

		#region Count

		/// <summary>
		/// Gets the number of sort conditions in the collection.
		/// </summary>
		public int Count
		{
			get { return _sortConditons.Count; }
		}

		#endregion // Count

		#region Indexer[int]

		/// <summary>
		/// Gets or sets the pair of item and sort condition at the specified index.
		/// </summary>
		/// <param name="index">The index at which to get the pair of item and sort condition.</param>
		/// <exception cref="ArgumentNullException">
		/// The Key or Value of the assigned value is null.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// The Key of the assigned value already in the collection at a different index.
		/// </exception>
		public KeyValuePair<T, SortCondition> this[int index]
		{
			get
			{
				return _sortConditons[index];
			}
			set
			{
				if (value.Key == null || value.Value == null)
					throw new ArgumentNullException("value");

				int existingIndex = this.IndexOf(value.Key);
				if (existingIndex == index && _sortConditons[existingIndex].Value == value.Value)
					return;

				if (existingIndex < 0 || existingIndex == index)
				{
					_sortConditons[index] = value;
					this.OnSortConditionsModified();
				}

				throw new ArgumentException(SR.GetString("LE_ArgumentOutOfRangeException_DuplicateItemSorted"), "value");
			}
		}

		#endregion // Indexer[T]

		#region Indexer[T]

		/// <summary>
		/// Gets or sets the sort condition for the specified sort-able item.
		/// </summary>
		/// <param name="sortableItem">The sort-able item for which to get or set the sort condition.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="sortableItem"/> is null.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// The sort-able item is not already in the collection and there are already 64 sort conditions in the collection.
		/// </exception>
		public SortCondition this[T sortableItem]
		{
			get
			{
				if (sortableItem == null)
					throw new ArgumentNullException("sortableItem");

				int index = this.IndexOf(sortableItem);
				if (index < 0)
					return null;

				return _sortConditons[index].Value;
			}
			set
			{
				if (sortableItem == null)
					throw new ArgumentNullException("sortableItem");

				int index = this.IndexOf(sortableItem);
				if (index < 0)
				{
					if (value != null)
						this.Add(sortableItem, value);

					return;
				}

				if (value == null)
				{
					this.RemoveAt(index);
					return;
				}

				_sortConditons[index] = new KeyValuePair<T, SortCondition>(sortableItem, value);
				this.OnSortConditionsModified();
			}
		}

		#endregion // Indexer[T]

		#endregion // Public Properties

		#endregion // Properties


		#region KeysCollection class

		private class KeysCollection : ICollection<T>
		{
			private SortConditionCollection<T> _owner;

			public KeysCollection(SortConditionCollection<T> owner)
			{
				_owner = owner;
			}

			private void ThrowOnModified()
			{
				throw new InvalidOperationException(SR.GetString("LE_InvalidOperationException_CannotModifyKeysCollection"));
			}

			#region ICollection<T> Members

			void ICollection<T>.Add(T item)
			{
				this.ThrowOnModified();
			}

			void ICollection<T>.Clear()
			{
				this.ThrowOnModified();
			}

			bool ICollection<T>.Contains(T item)
			{
				return _owner.Contains(item);
			}

			void ICollection<T>.CopyTo(T[] array, int arrayIndex)
			{
				for (int i = 0; i < _owner.Count; i++)
					array[arrayIndex + i] = _owner._sortConditons[i].Key;
			}

			int ICollection<T>.Count
			{
				get { return _owner.Count; }
			}

			bool ICollection<T>.IsReadOnly
			{
				get { return true; }
			}

			bool ICollection<T>.Remove(T item)
			{
				this.ThrowOnModified();
				return false;
			}

			#endregion

			#region IEnumerable<T> Members

			public IEnumerator<T> GetEnumerator()
			{
				foreach (KeyValuePair<T, SortCondition> pair in _owner)
					yield return pair.Key;
			}

			#endregion

			#region IEnumerable Members

			IEnumerator IEnumerable.GetEnumerator()
			{
				return this.GetEnumerator();
			}

			#endregion
		}

		#endregion // KeysCollection class

		#region ValuesCollection class

		private class ValuesCollection : ICollection<SortCondition>
		{
			private SortConditionCollection<T> _owner;

			public ValuesCollection(SortConditionCollection<T> owner)
			{
				_owner = owner;
			}

			private void ThrowOnModified()
			{
				throw new InvalidOperationException(SR.GetString("LE_InvalidOperationException_CannotModifyValuesCollection"));
			}

			#region ICollection<SortCondition> Members

			void ICollection<SortCondition>.Add(SortCondition item)
			{
				this.ThrowOnModified();
			}

			void ICollection<SortCondition>.Clear()
			{
				this.ThrowOnModified();
			}

			bool ICollection<SortCondition>.Contains(SortCondition item)
			{
				return _owner.Contains(item);
			}

			void ICollection<SortCondition>.CopyTo(SortCondition[] array, int arrayIndex)
			{
				for (int i = 0; i < _owner.Count; i++)
					array[arrayIndex + i] = _owner._sortConditons[i].Value;
			}

			int ICollection<SortCondition>.Count
			{
				get { return _owner.Count; }
			}

			bool ICollection<SortCondition>.IsReadOnly
			{
				get { return true; }
			}

			bool ICollection<SortCondition>.Remove(SortCondition item)
			{
				this.ThrowOnModified();
				return false;
			}

			#endregion

			#region IEnumerable<SortCondition> Members

			public IEnumerator<SortCondition> GetEnumerator()
			{
				foreach (KeyValuePair<T, SortCondition> pair in _owner)
					yield return pair.Value;
			}

			#endregion

			#region IEnumerable Members

			IEnumerator IEnumerable.GetEnumerator()
			{
				return this.GetEnumerator();
			}

			#endregion
		}

		#endregion // ValuesCollection class
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