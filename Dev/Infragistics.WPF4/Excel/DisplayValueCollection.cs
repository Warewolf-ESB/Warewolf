using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using System.Security;
using System.ComponentModel;
using System.Collections;
using Infragistics.Documents.Excel.Filtering;


using Infragistics.Shared;


namespace Infragistics.Documents.Excel
{
	// MD 12/13/11 - 12.1 - Table Support



	/// <summary>
	/// A collection of display text values.
	/// </summary>
	/// <seealso cref="WorksheetCell.GetText()"/>
	/// <seealso cref="WorksheetRow.GetCellText(int)"/>
	/// <seealso cref="Filtering.FixedValuesFilter.DisplayValues"/>
	[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelTables)]
	public

 class DisplayValueCollection : IList<string>
	{
		#region Member Variables

		private FixedValuesFilter _owner;
		private List<string> _displayValues = new List<string>();

		#endregion // Member Variables

		#region Constructor

		internal DisplayValueCollection(FixedValuesFilter owner) 
		{
			_owner = owner;
		}

		#endregion // Constructor

		#region Interfaces

		#region ICollection<string> Members

		void ICollection<string>.CopyTo(string[] array, int arrayIndex)
		{
			_displayValues.CopyTo(array, arrayIndex);
		}

		bool ICollection<string>.IsReadOnly
		{
			get { return false; }
		}

		#endregion

		#region IEnumerable<string> Members

		IEnumerator<string> IEnumerable<string>.GetEnumerator()
		{
			return _displayValues.GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _displayValues.GetEnumerator();
		}

		#endregion

		#endregion // Interfaces

		#region Methods

		#region Public Methods

		#region Add

		/// <summary>
		/// Adds a display text value to the collection.
		/// </summary>
		/// <param name="item">The display text value to add to the collection.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="item"/> is null or empty.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="item"/> matches another value in the collection. Values are compared case-insensitively.
		/// </exception>
		public void Add(string item)
		{
			this.VerifyItem(item);
			_displayValues.Add(item);
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
			if (_owner.GetAllowedItemCount() == _displayValues.Count)
				throw new InvalidOperationException(SR.GetString("LE_InvalidOperationException_FixedValuesFilterMustAcceptAValue"));

			_displayValues.Clear();
			_owner.OnModified();
		}

		#endregion // Clear

		#region Contains

		/// <summary>
		/// Determines whether the specified value is in the collection.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// Display text values are compared case-insensitively.
		/// </p>
		/// </remarks>
		/// <param name="item">The display text value to find in the collection.</param>
		/// <returns></returns>
		public bool Contains(string item)
		{
			return this.IndexOf(item) >= 0;
		}

		#endregion // Contains

		#region IndexOf

		/// <summary>
		/// Gets the index of the specified display text value in the collection.
		/// </summary>
		/// <param name="item">The display text value to find in the collection.</param>
		/// <returns>
		/// The 0-based index of the specified display text value in the collection or -1 if the item is not in the collection.
		/// </returns>
		public int IndexOf(string item)
		{
			// MD 4/9/12 - TFS101506
			CultureInfo culture = _owner.Culture;

			for (int i = 0; i < _displayValues.Count; i++)
			{
				// MD 4/9/12 - TFS101506
				//if (String.Equals(item, _displayValues[i], StringComparison.CurrentCultureIgnoreCase))
				if (String.Compare(item, _displayValues[i], culture, CompareOptions.IgnoreCase) == 0)
					return i;
			}

			return -1;
		}

		#endregion // IndexOf

		#region Insert

		/// <summary>
		/// Inserts a display text value into the collection.
		/// </summary>
		/// <param name="index">The 0-based index where the value should be inserted.</param>
		/// <param name="item">The display text value to insert into the collection.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="item"/> is null or empty.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="item"/> matches another value in the collection. Values are compared case-insensitively.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="index"/> is less than 0 or greater than <see cref="Count"/>.
		/// </exception>
		public void Insert(int index, string item)
		{
			this.VerifyItem(item);
			_displayValues.Insert(index, item);
			_owner.OnModified();
		}

		#endregion // Insert

		#region Remove

		/// <summary>
		/// Removes a display text value from the collection.
		/// </summary>
		/// <param name="item">The display text value to remove from the collection.</param>
		/// <remarks>
		/// <p class="body">
		/// Display text values are compared case-insensitively.
		/// </p>
		/// </remarks>
		/// <returns>True if the value was found and removed; False otherwise.</returns>
		public bool Remove(string item)
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
		/// Removes the display text value at the specified index.
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

			_displayValues.RemoveAt(index);
			_owner.OnModified();
		}

		#endregion // RemoveAt

		#endregion // Public Methods

		#region Private Methods

		#region VerifyItem

		private void VerifyItem(string item)
		{
			if (String.IsNullOrEmpty(item))
				throw new ArgumentNullException("item");

			if (this.Contains(item))
				throw new ArgumentException(SR.GetString("LE_ArgumentException_DuplicateDisplayValue"), "item");
		}

		#endregion // VerifyItem

		#endregion // Private Methods

		#endregion // Methods

		#region Properties

		#region Public Properties

		#region Count

		/// <summary>
		/// Gets the number of display text values in the collection.
		/// </summary>
		public int Count
		{
			get { return _displayValues.Count; }
		}

		#endregion // Count

		#region Indexer[int]

		/// <summary>
		/// Gets or sets the display text value at the specified index.
		/// </summary>
		/// <param name="index">The 0-based index of the value to get or set.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="index"/> is less than 0 or greater than or equal to <see cref="Count"/>.
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// The value assigned is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// The value assigned matches another value in the collection. Values are compared case-insensitively.
		/// </exception>
		public string this[int index]
		{
			get { return _displayValues[index]; }
			set
			{
				if (value == null)
					throw new ArgumentNullException("item");

				int existingIndex = this.IndexOf(value);
				if (existingIndex < 0 || existingIndex == index)
				{
					_displayValues[index] = value;
					_owner.OnModified();
					return;
				}

				throw new ArgumentException(SR.GetString("LE_ArgumentException_DuplicateDisplayValue"), "item");
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