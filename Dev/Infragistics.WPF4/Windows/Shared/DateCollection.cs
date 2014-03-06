using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Collections.Specialized;
using System.Diagnostics;

namespace Infragistics.Collections
{
	/// <summary>
	/// A custom collection of unique <see cref="DateTime"/> instances that do not contain time information.
	/// </summary>
	public class DateCollection : ObservableCollection<DateTime>
		, ICollection<DateTime>
	{
		#region Member Variables

		private bool _isSortDirty;
		private DateTime[] _sortedDates;

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="DateCollection"/>
		/// </summary>
		public DateCollection()
		{
		}
		#endregion //Constructor

		#region Base class overrides

		#region ClearItems
		/// <summary>
		/// Removes all the items from the collection.
		/// </summary>
		protected override void ClearItems()
		{
			DateTime[] oldDates = new DateTime[this.Count];
			this.CopyTo(oldDates, 0);
			this.InvalidateSortedDates();

			base.ClearItems();

			if (oldDates.Length > 0)
				this.OnDatesChanged(new DateTime[0], oldDates);
		}
		#endregion //ClearItems

		#region InsertItem
		/// <summary>
		/// Inserts a new item at the specified index in the collection.
		/// </summary>
		/// <param name="index">The index at which to insert the <paramref name="item"/></param>
		/// <param name="item">The date to insert in the collection</param>
		protected override void InsertItem(int index, DateTime item)
		{
			// AS 2/19/09 TFS10861
			// Strip the time portion.
			//
			item = item.Date;

			this.VerifyCanAdd(item);

			int newCount = this.Count + 1;
			this.VerifyNewDateCount(ref newCount, true);

			this.InvalidateSortedDates();

			base.InsertItem(index, item);

			this.OnDatesChanged(new DateTime[] { item }, new DateTime[0]);
		}
		#endregion //InsertItem

		#region RemoveItem
		/// <summary>
		/// Removes an item at the specified index.
		/// </summary>
		/// <param name="index">The index of the item in the collection to be removed.</param>
		protected override void RemoveItem(int index)
		{
			DateTime item = this[index];
			this.InvalidateSortedDates();

			base.RemoveItem(index);

			this.OnDatesChanged(new DateTime[0], new DateTime[] { item });
		}
		#endregion //RemoveItem

		#region SetItem
		/// <summary>
		/// Replaces an item at the specified index in the collection 
		/// </summary>
		/// <param name="index">Index of the item to replace</param>
		/// <param name="item">The item to insert into the collection.</param>
		protected override void SetItem(int index, DateTime item)
		{
			// AS 2/19/09 TFS10861
			// Strip the time portion.
			//
			item = item.Date;

			if (this[index] == item)
				return;

			this.VerifyCanAdd(item);
			this.InvalidateSortedDates();

			DateTime oldItem = this[index];

			base.SetItem(index, item);

			this.OnDatesChanged(new DateTime[] { item }, new DateTime[] { oldItem });
		}
		#endregion //SetItem

		#endregion //Base class overrides

		#region Properties

		#region Protected

		#region AllowedRange
		private DateRange _allowedRange = DateRange.Infinite;

		/// <summary>
		/// Returns a range that represents the dates that may be added.
		/// </summary>
		internal protected DateRange AllowedRange
		{
			get { return _allowedRange; }
			set
			{
				value.Normalize();
				value.RemoveTime();

				if (value != _allowedRange)
				{
					_allowedRange = value;
					this.EnsureWithinMinMax();
				}
			}
		}
		#endregion //AllowedRange

		#endregion //Protected

		#endregion //Properties

		#region Methods

		#region Internal

		#region BinarySearch
		internal int BinarySearch(DateTime date)
		{
			this.VerifySortedDates();

			return Array.BinarySearch(_sortedDates, date.Date);
		} 
		#endregion // BinarySearch

		#region ContainsDate
		internal bool ContainsDate(DateTime date)
		{
			return this.BinarySearch(date) >= 0;
		}
		#endregion //ContainsDate

		#region Reinitialize
		internal void Reinitialize(IList<DateTime> newSelection)
		{
			List<DateTime> datesSelected = new List<DateTime>();
			List<DateTime> datesUnselected = new List<DateTime>();

			// if we are reinitializing from the sort array then 
			// we don't need to calculate a delta or dirty the sort
			bool isSortArray = newSelection == _sortedDates;

			if (!isSortArray)
			{
				this.CalculateDelta(newSelection, datesSelected, datesUnselected);

				this.InvalidateSortedDates();
			}

			// update the selection
			this.Items.Clear();

			foreach ( DateTime date in newSelection )
			{
				Debug.Assert(date.TimeOfDay == TimeSpan.Zero, "Date passed to Reinitialize has time portion");
				this.Items.Add(date);
			}

			// AS 10/13/08 TFS8974
			// All the other places that call RaiseSelectionChanged are not calling
			// OnSelectedStateChanged. In theory we could update just those places
			// but we'll be inconsistent because in some cases the items we'll send
			// property changed & collection changed before notifying the owner
			// and sometimes it will be afterwards. To be consistent, I'm moving this
			// call into the RaiseSelectionChanged method so they both always happen
			// after the Property/Collection change notifications.
			//
			//this._owner.OnSelectedStateChanged(datesSelected, datesUnselected);

			// JJD 4/5/11 - TFS66907
			// Moved from below so the collection is in a stable state before any
			// notifications are raised.
			if (datesSelected.Count > 0 || datesUnselected.Count > 0)
				this.OnDatesChanged(datesSelected, datesUnselected);

			// AS 10/13/08 TFS8974
			// I noticed I'm not sending a Count changed. I would conditionally do this
			// but I see that ObservableCollection doesn't make any checks in its Clear
			// call.
			//
			this.OnPropertyChanged(new PropertyChangedEventArgs("Count"));

			this.OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
			this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

			//if (datesSelected.Count > 0 || datesUnselected.Count > 0)
			//    this.OnDatesChanged(datesSelected, datesUnselected);
		}
		#endregion //Reinitialize

		#endregion //Internal

		#region Private

		#region CalculateDelta

		private void CalculateDelta(IList<DateTime> newSelection,
			List<DateTime> datesSelected,
			List<DateTime> datesUnselected)
		{
			if (newSelection.Count == 0)
				datesUnselected.AddRange(this);
			else if (this.Count == 0)
				datesSelected.AddRange(newSelection);
			else
			{
				// neither are 0 so find the diff

				// assume all dates unselected
				datesUnselected.AddRange(this);

				// then see what is in the new either 
				// adding them to the selected list or 
				// removing them from the unselected list
				// AS 2/19/09 TFS10861
				// Strip the time portion.
				//
				//foreach (DateTime date in newSelection)
				//{
				for (int i = 0, count = newSelection.Count; i < count; i++)
				{
					DateTime date = newSelection[i].Date;

					if (this.ContainsDate(date))
						datesUnselected.Remove(date);
					else
						datesSelected.Add(date);
				}
			}
		}
		#endregion //CalculateDelta

		#region EnsureWithinMinMax
		private void EnsureWithinMinMax()
		{
			if (this.Count > 0)
			{
				this.VerifySortedDates();
				DateRange allowedRange = this.AllowedRange;
				allowedRange.Normalize();
				allowedRange.RemoveTime();
				DateTime minDate = allowedRange.Start;
				DateTime maxDate = allowedRange.End;

				int startIndex = Array.BinarySearch(this._sortedDates, minDate);
				int endIndex = Array.BinarySearch(this._sortedDates, maxDate);

				if (startIndex < 0)
					startIndex = ~startIndex;

				if (endIndex < 0)
				{
					endIndex = ~endIndex;
					endIndex--; 
				}

				int count = endIndex - startIndex + 1;

				// if we need to remove dates..
				if (count < this.Count)
				{
					List<DateTime> newDates = new List<DateTime>(Math.Max(count, 0));

					if (count > 0)
					{
						DateRange validRange = new DateRange(minDate, maxDate);

						// keep the original order so we need to iterate the collection
						for (int i = 0, len = this.Items.Count; i < len; i++)
						{
							DateTime date = this.Items[i];

							if (validRange.Contains(date))
								newDates.Add(date);
						}
					}

					this.Reinitialize(newDates.ToArray());
				}
			}
		}
		#endregion //EnsureWithinMinMax

		#region InvalidateSortedDates
		private void InvalidateSortedDates()
		{
			_isSortDirty = true;
			this._sortedDates = null;
		}
		#endregion //InvalidateSortedDates

		#region SyncSortedDates
		private void SyncSortedDates()
		{
			DateTime[] items = new DateTime[this.Count];
			this.CopyTo(items, 0);
			Array.Sort(items, Comparer<DateTime>.Default);
			this._sortedDates = items;
		}
		#endregion //SyncSortedDates

		#region VerifyCanAdd
		private void VerifyCanAdd(DateTime date)
		{
			this.VerifySortedDates();

#pragma warning disable 436

			if (Array.BinarySearch(this._sortedDates, date) >= 0)
			{
				throw new InvalidOperationException(SR.GetString("LE_DateAlreadySelected", date)); 
			}

			if (!_allowedRange.Contains(date))
				throw new ArgumentOutOfRangeException(string.Format(SR.GetString("LE_DateOutOfRange"), date, _allowedRange.Start, _allowedRange.End)); 

#pragma warning restore 436
		}
		#endregion //VerifyCanAdd

		#region VerifySortedDates
		private void VerifySortedDates()
		{
			if (null == this._sortedDates)
				this.SyncSortedDates();
		}
		#endregion //VerifySortedDates

		#endregion //Private

		#region Public

		#region AddRange
		/// <summary>
		/// Adds a specified range of dates to the collection.
		/// </summary>
		/// <param name="start">Start of the range</param>
		/// <param name="end">End of the range</param>
		/// <param name="allowPartialSelection">True if as many dates up to the allowed maximum dates should be selected. If false and the range would cause the selection to exceed the maximum an exception will be thrown.</param>
		/// <exception cref="ArgumentOutOfRangeException">The specified range would result in more dates being selected than allowed by the maximum # of dates.</exception>
		public void AddRange(DateTime start, DateTime end, bool allowPartialSelection)
		{
			this.AddRange(new DateRange(start, end), allowPartialSelection);
		}

		/// <summary>
		/// Adds a specified range of dates to the collection.
		/// </summary>
		/// <param name="range">The range of dates to add to the collection</param>
		/// <param name="allowPartialSelection">True if as many dates up to the maximum allowable dates should be selected. If false and the range would cause the selection to exceed the maximum an exception will be thrown.</param>
		/// <exception cref="ArgumentOutOfRangeException">The specified range would result in more dates being selected than allowed by the maximum allowable dates.</exception>
		public void AddRange(DateRange range, bool allowPartialSelection)
		{
			range.RemoveTime();
			range.Normalize();

			if (!allowPartialSelection && !_allowedRange.Contains(range))
				throw new ArgumentOutOfRangeException(); 

			// AS 2/19/09 TFS10861
			// Strip the time portion.
			//
			//DateTime[] dates = this._owner.GetSelectableDates(new DateRange(start, end));
			DateTime[] dates = this.GetAllowedDates(range);

			if (null != dates && dates.Length > 0)
			{
				int datesCount = dates.Length;

				// make sure it doesn't contain a date that is already selected
				for (int i = 0; i < dates.Length; i++)
					this.VerifyCanAdd(dates[i]);

				int newCount = this.Count + datesCount;
				int allowedNewCount = newCount;

				this.VerifyNewDateCount(ref allowedNewCount, !allowPartialSelection);

				// if we're not allowed to add them all
				if (allowedNewCount < newCount)
				{
					datesCount = allowedNewCount - this.Count;

					// if we're already at the max then bail out
					if (datesCount <= 0)
						return;

					// we should be trimming this because we will be passing this into the OnDatesChanged
					DateTime[] tempDates = new DateTime[datesCount];
					Array.Copy(dates, tempDates, datesCount);
					dates = tempDates;
				}

				for (int i = 0; i < datesCount; i++)
					this.Items.Add(dates[i]);

				this.InvalidateSortedDates();

				// AS 10/13/08 TFS8974
				// I noticed I'm not sending a Count changed.
				//
				this.OnPropertyChanged(new PropertyChangedEventArgs("Count"));

				this.OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
				this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

				this.OnDatesChanged(dates, new DateTime[0]);
			}
		}
		#endregion //AddRange

		#region Contains
		/// <summary>
		/// Returns a boolean indicating if the collection contains a given date.
		/// </summary>
		/// <param name="date">The date to evaluate</param>
		/// <returns>A boolean indicating if the collection contains the specified date.</returns>
		public new bool Contains(DateTime date)
		{
			return this.ContainsDate(date);
		}
		#endregion //Contains

		#region IntersectsWith
		/// <summary>
		/// Returns true if there are any dates in the collection that intersect with the specified range.
		/// </summary>
		/// <param name="range">The range to evaluate</param>
		/// <returns>True if the collection contains any date within the specified range</returns>
		public bool IntersectsWith(DateRange range)
		{
			range.RemoveTime();
			range.Normalize();

			DateTime start = range.Start;
			DateTime end = range.End;

			if (start == end)
				return ContainsDate(start);

			this.VerifySortedDates();

			Debug.Assert(start <= end);

			int startIndex = Array.BinarySearch(this._sortedDates, start);

			if (startIndex >= 0)
				return true;
			else
				startIndex = ~startIndex;

			int endIndex = Array.BinarySearch(this._sortedDates, end);

			if (endIndex >= 0)
				return true;
			else
				endIndex = ~endIndex;

			for (int i = startIndex, lastIndex = Math.Min(endIndex, this._sortedDates.Length - 1); i <= lastIndex; i++)
			{
				DateTime date = this._sortedDates[i];

				if (date >= start && date <= end)
					return true;
			}

			return false;
		}
		#endregion //IntersectsWith

		#region Sort
		/// <summary>
		/// Sorts the contents of the collection in ascending order.
		/// </summary>
		public void Sort()
		{
			if (this.Count > 0 && _isSortDirty)
			{
				this.VerifySortedDates();
				_isSortDirty = false;

				Debug.Assert(this.Count == this._sortedDates.Length);

				this.Reinitialize(this._sortedDates);
			}
		}
		#endregion //Sort

		#endregion //Public

		#region Protected

		#region GetAllowedDates
		/// <summary>
		/// Returns an array of the dates that are allowed to be added for the specified range.
		/// </summary>
		/// <param name="dateRange">An object representing the range of dates to add</param>
		/// <returns>An array of the dates that are allowed to be added.</returns>
		protected virtual DateTime[] GetAllowedDates(DateRange dateRange)
		{
			List<DateTime> dates = new List<DateTime>();

			dateRange.Normalize();
			dateRange.RemoveTime();

			// use the intersection with the allowed range
			if (!dateRange.Intersect(_allowedRange))
				return new DateTime[0];

			DateTime date = dateRange.Start.Date;
			DateTime endDate = dateRange.End.Date;

			while (date <= endDate)
			{
				dates.Add(date);
				date = date.AddDays(1d);
			}

			return dates.ToArray();
		}
		#endregion //GetAllowedDates

		#region OnDatesChanged
		/// <summary>
		/// Invoked when the selection has changed indicating the added and removed dates.
		/// </summary>
		/// <param name="added">List of dates added</param>
		/// <param name="removed">List of dates that were removed from the collection</param>
		protected virtual void OnDatesChanged(IList<DateTime> added, IList<DateTime> removed)
		{
		}
		#endregion //OnDatesChanged

		#region VerifyNewDateCount
		/// <summary>
		/// Used to determine if the number of items that will be in the collection is allowed.
		/// </summary>
		/// <param name="newItemCount">The new item count. This is passed by reference and when <paramref name="validate"/> is false, this should be updated to reflect the constrained value if it exceeds the maximum</param>
		/// <param name="validate">True if an exception should be thrown if the new count exceeds the allowed</param>
		protected virtual void VerifyNewDateCount(ref int newItemCount, bool validate)
		{
		}
		#endregion //VerifyNewDateCount

		#endregion //Protected

		#endregion //Methods

		#region ICollection<DateTime>
		bool ICollection<DateTime>.Contains(DateTime date)
		{
			return this.ContainsDate(date);
		}
		#endregion //ICollection<DateTime>
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