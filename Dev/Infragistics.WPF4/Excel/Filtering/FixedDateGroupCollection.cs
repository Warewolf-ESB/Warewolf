using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using System.Security;
using System.ComponentModel;
using System.Collections;


using Infragistics.Shared;


namespace Infragistics.Documents.Excel.Filtering
{
	// MD 12/13/11 - 12.1 - Table Support



	/// <summary>
	/// A collection of fixed date groups.
	/// </summary>
	/// <seealso cref="FixedValuesFilter.DateGroups"/>
	[DebuggerDisplay("FixedDateGroupCollection: Count - {Count}")]
	[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelTables)]
	public

 class FixedDateGroupCollection : IList<FixedDateGroup>
	{
		#region Member Variables

		private FixedValuesFilter _owner;
		private List<FixedDateGroup> _fixedDateGroups = new List<FixedDateGroup>();

		#endregion // Member Variables

		#region Constructor

		internal FixedDateGroupCollection(FixedValuesFilter owner)
		{
			_owner = owner;
		}

		#endregion // Constructor

		#region Interfaces

		#region ICollection<FixedDateGroup> Members

		void ICollection<FixedDateGroup>.CopyTo(FixedDateGroup[] array, int arrayIndex)
		{
			_fixedDateGroups.CopyTo(array, arrayIndex);
		}

		bool ICollection<FixedDateGroup>.IsReadOnly
		{
			get { return false; }
		}

		#endregion

		#region IEnumerable<FixedDateGroup> Members

		IEnumerator<FixedDateGroup> IEnumerable<FixedDateGroup>.GetEnumerator()
		{
			return _fixedDateGroups.GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _fixedDateGroups.GetEnumerator();
		}

		#endregion

		#endregion // Interfaces

		#region Methods

		#region Public Methods

		#region Add

		/// <summary>
		/// Adds a fixed date group to the collection.
		/// </summary>
		/// <param name="item">The fixed date group to add to the collection.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="item"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="item"/> is already in the collection.
		/// </exception>
		public void Add(FixedDateGroup item)
		{
			this.VerifyItem(item);
			_fixedDateGroups.Add(item);
			_owner.OnModified();
		}

		#endregion // Add

		#region Clear

		/// <summary>
		/// Clears the collection.
		/// </summary>
		/// <exception cref="InvalidOperationException">
		/// The collection is owned by a <see cref="FixedValuesFilter"/> which only allowed the values in the collection.
		/// Clearing the collection would prevent the filter from including any values, which is not allowed for a FixedValuesFilter.
		/// </exception>
		public void Clear()
		{
			if (_owner.GetAllowedItemCount() == _fixedDateGroups.Count)
				throw new InvalidOperationException(SR.GetString("LE_InvalidOperationException_FixedValuesFilterMustAcceptAValue"));

			_fixedDateGroups.Clear();
			_owner.OnModified();
		}

		#endregion // Clear

		#region Contains

		/// <summary>
		/// Determines whether the specified fixed date group is in the collection.
		/// </summary>
		/// <param name="item">The fixed date group to find in the collection.</param>
		/// <returns>True if the item is in the collection; False otherwise.</returns>
		public bool Contains(FixedDateGroup item)
		{
			return this.IndexOf(item) >= 0;
		}

		#endregion // Contains

		#region IndexOf

		/// <summary>
		/// Gets the index of the specified fixed date group in the collection.
		/// </summary>
		/// <param name="item">The fixed date group to find in the collection.</param>
		/// <returns>
		/// The 0-based index of the specified fixed date group in the collection or -1 if the item is not in the collection.
		/// </returns>
		public int IndexOf(FixedDateGroup item)
		{
			for (int i = 0; i < _fixedDateGroups.Count; i++)
			{
				if (Object.Equals(item, _fixedDateGroups[i]))
					return i;
			}

			return -1;
		}

		#endregion // IndexOf

		#region Insert

		/// <summary>
		/// Inserts a fixed date group into the collection.
		/// </summary>
		/// <param name="index">The 0-based index where the value should be inserted.</param>
		/// <param name="item">The fixed date group to insert into the collection.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="item"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="item"/> is already in the collection.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="index"/> is less than 0 or greater than <see cref="Count"/>.
		/// </exception>
		public void Insert(int index, FixedDateGroup item)
		{
			this.VerifyItem(item);
			_fixedDateGroups.Insert(index, item);
			_owner.OnModified();
		}

		#endregion // Insert

		#region Remove

		/// <summary>
		/// Removes the fixed date group from the collection.
		/// </summary>
		/// <param name="item">The fixed date group to remove from the collection.</param>
		/// <returns>True if the value was found and removed; False otherwise.</returns>
		public bool Remove(FixedDateGroup item)
		{
			int index = this.IndexOf(item);
			if (index < 0)
				return false;

			this.RemoveAt(index);
			return true;
		}

		#endregion // Remove

		#region RemoveAt

		/// <summary>
		/// Removes the fixed date group at the specified index.
		/// </summary>
		/// <param name="index">The 0-based index of the value to remove.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="index"/> is less than 0 or greater than or equal to <see cref="Count"/>.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// This operation removes the last item in the collection and it is owned by a <see cref="FixedValuesFilter"/> which only 
		/// allowed the values in the collection. Clearing the collection would prevent the filter from including any values, which 
		/// is not allowed for a FixedValuesFilter.
		/// </exception>
		public void RemoveAt(int index)
		{
			if (_owner.GetAllowedItemCount() == 1)
				throw new InvalidOperationException(SR.GetString("LE_InvalidOperationException_FixedValuesFilterMustAcceptAValue"));

			_fixedDateGroups.RemoveAt(index);
			_owner.OnModified();
		}

		#endregion // RemoveAt

		#endregion // Public Methods

		#region Private Methods

		#region VerifyItem

		private void VerifyItem(FixedDateGroup item)
		{
			if (item == null)
				throw new ArgumentNullException("item");

			if (this.Contains(item))
				throw new ArgumentException(SR.GetString("LE_ArgumentException_DuplicateFixedDateGroup"), "item");
		}

		#endregion // Private Methods

		#endregion // MyRegion

		#endregion // Methods

		#region Properties

		#region Public Properties

		#region Count

		/// <summary>
		/// Gets the number of fixed date groups in the collection.
		/// </summary>
		public int Count
		{
			get { return _fixedDateGroups.Count; }
		}

		#endregion // Count

		#region Indexer[int]

		/// <summary>
		/// Gets or sets the fixed date group at the specified index.
		/// </summary>
		/// <param name="index">The 0-based index of the value to get or set.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="index"/> is less than 0 or greater than or equal to <see cref="Count"/>.
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// The value assigned is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// The value assigned is already in the collection.
		/// </exception>
		public FixedDateGroup this[int index]
		{
			get { return _fixedDateGroups[index]; }
			set
			{
				if (value == null)
					throw new ArgumentNullException("item");

				int existingIndex = this.IndexOf(value);
				if (existingIndex < 0 || existingIndex == index)
				{
					_fixedDateGroups[index] = value;
					return;
				}

				throw new ArgumentException(SR.GetString("LE_ArgumentException_DuplicateFixedDateGroup"), "item");
			}
		}

		#endregion // Indexer[int]

		#endregion // Public Properties

		#endregion // Properties
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