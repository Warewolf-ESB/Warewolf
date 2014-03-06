using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using Infragistics.Windows.Helpers;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using Infragistics.Windows.Editors.Events;
using Infragistics.Shared;

namespace Infragistics.Windows.Editors
{
    /// <summary>
    /// A custom collection of <see cref="DateTime"/> instances used to represent the selected dates within the <see cref="XamMonthCalendar"/>
    /// </summary>
    public class SelectedDateCollection : ObservableCollection<DateTime>
    {
        #region Member Variables

        private XamMonthCalendar _owner;
        private DateTime[] _sortedDates;

        #endregion //Member Variables

        #region Constructor
        internal SelectedDateCollection(XamMonthCalendar owner)
        {
            Utils.ValidateNull("owner", owner);

            this._owner = owner;
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
                this.RaiseSelectionChanged(new DateTime[0], oldDates);
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

            if (this.Count + 1 > this._owner.MaxSelectedDatesResolved)
                throw new InvalidOperationException(Utils.GetString("LE_MaxSelectedDatesExceeded", this.Count + 1, this._owner.MaxSelectedDatesResolved));

            this.InvalidateSortedDates();

            base.InsertItem(index, item);

            this.RaiseSelectionChanged(new DateTime[] { item }, new DateTime[0]);
        } 
        #endregion //InsertItem

        #region OnCollectionChanged
        /// <summary>
        /// Invoked when the collection has been changed.
        /// </summary>
        /// <param name="e">Provides information about the change</param>
        protected override void OnCollectionChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // clear the cached sorted list
            this.InvalidateSortedDates();

            // keep the selected date in sync
            this._owner.SelectedDate = this.Items.Count == 0 ? null : (DateTime?)this.Items[0];

            base.OnCollectionChanged(e);
        } 
        #endregion //OnCollectionChanged

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

            this.RaiseSelectionChanged(new DateTime[0], new DateTime[] { item });
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

            this.VerifyCanAdd(item);
            this.InvalidateSortedDates();

            DateTime oldItem = this[index];

            base.SetItem(index, item);

            this.RaiseSelectionChanged(new DateTime[] { item }, new DateTime[] { oldItem });
        } 
        #endregion //SetItem

        #endregion //Base class overrides

        #region Methods

        #region Internal

        #region ContainsSelection
        internal bool ContainsSelection(DateTime start, DateTime end)
        {
            if (start == end)
                return IsSelected(start);

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
        #endregion //ContainsSelection

        #region EnsureWithinMinMax
        internal void EnsureWithinMinMax()
        {
            if (this.Count > 0)
            {
                this.VerifySortedDates();
                DateTime minDate = this._owner.MinDate;
                DateTime maxDate = this._owner.MaxDate;

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
                        CalendarDateRange validRange = new CalendarDateRange(minDate, maxDate);

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

        #region IsSelected
        internal bool IsSelected(DateTime date)
        {
            this.VerifySortedDates();

            return Array.BinarySearch(this._sortedDates, date) >= 0;
        }
        #endregion //IsSelected

        #region RaiseSelectionChanged
        private void RaiseSelectionChanged(IList<DateTime> selected, IList<DateTime> unselected)
        {
            // AS 10/13/08 TFS8974
            this._owner.OnSelectedStateChanged(selected, unselected);

            this._owner.RaiseSelectedDatesChanged(new SelectedDatesChangedEventArgs(unselected, selected));
        } 
        #endregion //RaiseSelectionChanged

        #region Reinitialize
        internal void Reinitialize(IList<DateTime> newSelection)
        {
            List<DateTime> datesSelected = new List<DateTime>();
            List<DateTime> datesUnselected = new List<DateTime>();

            this.CalculateDelta(newSelection, datesSelected, datesUnselected);

            this.InvalidateSortedDates();

            // update the selection
            this.Items.Clear();

            foreach (DateTime date in newSelection)
                this.Items.Add(date);

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

            // AS 10/13/08 TFS8974
            // I noticed I'm not sending a Count changed. I would conditionally do this
            // but I see that ObservableCollection doesn't make any checks in its Clear
            // call.
            //
            this.OnPropertyChanged(new PropertyChangedEventArgs("Count"));

            this.OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

            if (datesSelected.Count > 0 || datesUnselected.Count > 0)
                this.RaiseSelectionChanged(datesSelected, datesUnselected);
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

                    if (this.IsSelected(date))
                        datesUnselected.Remove(date);
                    else
                        datesSelected.Add(date);
                }
            }
        }
        #endregion //CalculateDelta

        #region InvalidateSortedDates
        private void InvalidateSortedDates()
        {
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

            if (Array.BinarySearch(this._sortedDates, date) > 0)
                throw new InvalidOperationException(Utils.GetString("LE_DateAlreadySelected", date));
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
        /// <param name="allowPartialSelection">True if as many dates up to the <see cref="XamMonthCalendar.MaxSelectedDates"/> should be selected. If false and the range would cause the selection to exceed the maximum an exception will be thrown.</param>
        /// <exception cref="ArgumentOutOfRangeException">The specified range would result in more dates being selected than allowed by the <see cref="XamMonthCalendar.MaxSelectedDates"/>.</exception>
        public void AddRange(DateTime start, DateTime end, bool allowPartialSelection)
        {
            // AS 2/19/09 TFS10861
            // Strip the time portion.
            //
            //DateTime[] dates = this._owner.GetSelectableDates(new CalendarDateRange(start, end));
            DateTime[] dates = this._owner.GetSelectableDates(new CalendarDateRange(start.Date, end.Date));

            if (null != dates && dates.Length > 0)
            {
                int datesCount = dates.Length;

                // make sure it doesn't contain a date that is already selected
                for (int i = 0; i < dates.Length; i++)
                    this.VerifyCanAdd(dates[i]);

                int max = this._owner.MaxSelectedDatesResolved;

                if (this.Count + datesCount > max)
                {
                    if (allowPartialSelection == false)
                        throw new InvalidOperationException(Utils.GetString("LE_MaxSelectedDatesExceeded", this.Count + datesCount, max));

                    datesCount = max - this.Count;
                }

                for(int i = 0; i < datesCount; i++)
                    this.Items.Add(dates[i]);

                this.InvalidateSortedDates();

                // AS 10/13/08 TFS8974
                // I noticed I'm not sending a Count changed.
                //
                this.OnPropertyChanged(new PropertyChangedEventArgs("Count"));

                this.OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
                this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

                this.RaiseSelectionChanged(dates, new DateTime[0]);
            }
        }
        #endregion //AddRange

        #region Sort
        /// <summary>
        /// Sorts the contents of the collection in ascending order.
        /// </summary>
        public void Sort()
        {
            if (this.Count > 0)
            {
                this.VerifySortedDates();

                Debug.Assert(this.Count == this._sortedDates.Length);

                this.Reinitialize(this._sortedDates);
            }
        }
        #endregion //Sort

        #endregion //Public

        #endregion //Methods
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