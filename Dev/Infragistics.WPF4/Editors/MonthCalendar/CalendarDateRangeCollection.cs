using System;
using System.Collections.Generic;
using System.Text;
using Infragistics.Windows.Helpers;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using Infragistics.Collections;

namespace Infragistics.Windows.Editors
{
    /// <summary>
    /// A collection of <see cref="CalendarDateRange"/> instances.
    /// </summary>
    public sealed class CalendarDateRangeCollection : ObservableCollectionExtended<CalendarDateRange>
    {
        #region Member Variables

        private XamMonthCalendar _owner;
        private CalendarDateRange[] _combinedRanges;

        #endregion //Member Variables

        #region Constructor
        internal CalendarDateRangeCollection(XamMonthCalendar owner)
        {
            Utils.ValidateNull("owner", owner);
            this._owner = owner;
        } 
        #endregion //Constructor

        #region Base class overrides

        /// <summary>
        /// Invoked when the collection has been changed.
        /// </summary>
        /// <param name="e">Provides information about the change</param>
        protected override void OnCollectionChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this._combinedRanges = null;

            base.OnCollectionChanged(e);
        }

        /// <summary>
        /// Invoked when a property of the collection has been changed.
        /// </summary>
        /// <param name="e">Provides information about the change</param>
        protected override void OnPropertyChanged(System.ComponentModel.PropertyChangedEventArgs e)
        {
            this._combinedRanges = null;

            base.OnPropertyChanged(e);
        }

        #endregion //Base class overrides

        #region Methods

        #region Private

        #region GetNextAvailableDate
        private DateTime? GetNextAvailableDate(ref DateTime start, ref DateTime end, int index)
        {
            int disabledDays = (int)this._owner.DisabledDaysOfWeek;
            Debug.Assert(disabledDays != XamMonthCalendar.AllDays);

            DateTime dateToCheck = start;
            Calendar cal = this._owner.CalendarManager.Calendar;

            // ok so we definitely have a gap. let's find the first available date
            //
            for (int i = Math.Max(0, index - 1); i < this._combinedRanges.Length; i++)
            {
                CalendarDateRange item = this._combinedRanges[i];

                // the first range could be before our range
                if (item.End < dateToCheck)
                    continue;

                // if there's a gap check for a non-disabled date
                while (dateToCheck < item.Start)
                {
                    if (0 == (disabledDays & 1 << (int)cal.GetDayOfWeek(dateToCheck)))
                        return dateToCheck;

                    // if we've hit the end of our range then exit
                    if (dateToCheck == end)
                        return null;

                    // move to the next day
                    dateToCheck = dateToCheck.AddTicks(TimeSpan.TicksPerDay);
                }

                // if this range covers the rest of our search then exit
                if (end <= item.End)
                    return null;

                // otherwise go to the day after the range
                dateToCheck = item.End.AddTicks(TimeSpan.TicksPerDay);
            }

            // if we got here then we ran out of combined ranges
            // so we just need to check the days of the week
            for (int i = 0; i < 7; i++)
            {
                if (0 == (disabledDays & 1 << (int)cal.GetDayOfWeek(dateToCheck)))
                    return dateToCheck;

                if (dateToCheck == end)
                    break;

                dateToCheck = dateToCheck.AddTicks(TimeSpan.TicksPerDay);
            }

            return null;
        }
        #endregion //GetNextAvailableDate

        #region GetPreviousAvailableDate
        private DateTime? GetPreviousAvailableDate(ref DateTime start, ref DateTime end, int index)
        {
            int disabledDays = (int)this._owner.DisabledDaysOfWeek;
            Debug.Assert(disabledDays != XamMonthCalendar.AllDays);

            DateTime dateToCheck = end;
            Calendar cal = this._owner.CalendarManager.Calendar;

            int startingIndex = Array.BinarySearch(this._combinedRanges, new CalendarDateRange(end, end));

            if (startingIndex < 0)
                startingIndex = ~startingIndex;

            // ok so we definitely have a gap. let's find the first available date
            //
            for (int i = this._combinedRanges.Length - 1, endIndex = Math.Max(0, index - 1); i >= endIndex; i--)
            {
                CalendarDateRange item = this._combinedRanges[i];

                // skip any range that ends before our range
                if (item.End < start || item.Start > end)
                    continue;

                // if we hit the beginning of the range without an open item
                // then return null
                if (item.Start <= start)
                    return null;

                // if the range ends before the one being checked then 
                // change our date to the previous day and keep checking
                // to see what the available range is
                if (item.Start <= dateToCheck && item.End >= dateToCheck)
                {
                    dateToCheck = item.Start.AddTicks(-TimeSpan.TicksPerDay);
                    continue;
                }

                // if there's a gap check for a non-disabled date
                while (dateToCheck > item.End)
                {
                    if (0 == (disabledDays & 1 << (int)cal.GetDayOfWeek(dateToCheck)))
                        return dateToCheck;

                    // don't go before the start date
                    if (dateToCheck == start)
                        return null;

                    // move to the next day
                    dateToCheck = dateToCheck.AddTicks(-TimeSpan.TicksPerDay);
                }

                // if this range covers the rest of our search then exit
                if (end <= item.End)
                    return null;

                // otherwise go to the day after the range
                dateToCheck = item.End.AddTicks(TimeSpan.TicksPerDay);
            }

            // if we got here then we ran out of combined ranges
            // so we just need to check the days of the week
            for (int i = 0; i < 7; i++)
            {
                if (0 == (disabledDays & 1 << (int)cal.GetDayOfWeek(dateToCheck)))
                    return dateToCheck;

                if (dateToCheck == start)
                    break;

                dateToCheck = dateToCheck.AddTicks(-TimeSpan.TicksPerDay);
            }

            return null;
        }
        #endregion //GetPreviousAvailableDate

        #region InitializeCombinedRanges
        private void InitializeCombinedRanges()
        {
            // copy over the ranges
            CalendarDateRange[] sortedRanges = new CalendarDateRange[this.Count];
            this.CopyTo(sortedRanges, 0);

            // ensure the item dates are normalized (i.e. start before end)
			for (int i = 0; i < sortedRanges.Length; i++)
			{
				sortedRanges[i].Normalize();

				// AS 1/8/10
				// Explicitly remove the time - this used to happen within the normalize.
				//
				sortedRanges[i].RemoveTime();
			}

            // then sort them
            Array.Sort(sortedRanges);

            int destIndex = -1;

            // now combine them so we don't have dates defined multiple times
            // e.g. if we have a range 8/1/08-8/20/08 and 7/20/08-8/5/08 then
            // we should only have 1 range from 7/20/08-8/20/08
            for (int sourceIndex = 0; sourceIndex < sortedRanges.Length; sourceIndex++)
            {
                CalendarDateRange source = sortedRanges[sourceIndex];

                // if the one we're processing overlaps with the next
                if (destIndex >= 0 &&
                    source.Start.Subtract(sortedRanges[destIndex].End).Ticks <= TimeSpan.TicksPerDay)
                {
                    // just update the end date if its not contained within the current range
                    if (source.End > sortedRanges[destIndex].End)
                        sortedRanges[destIndex].End = source.End;
                }
                else
                {
                    // otherwise create a new slot and use the source
                    destIndex++;
                    sortedRanges[destIndex] = source;
                }
            }

            // trim off the excess
            Array.Resize(ref sortedRanges, destIndex + 1);

            this._combinedRanges = sortedRanges;

        }
        #endregion //InitializeCombinedRanges

        #region VerifyCombinedRanges
        private void VerifyCombinedRanges()
        {
            if (this._combinedRanges == null)
                this.InitializeCombinedRanges();
        }
        #endregion //VerifyCombinedRanges

        #endregion //Private

        #region Internal

        #region AddAvailableDates
        internal void AddAvailableDates(CalendarDateRange range, List<DateTime> dates)
        {
            int disabledDays = (int)this._owner.DisabledDaysOfWeek;
            Debug.Assert(disabledDays != XamMonthCalendar.AllDays);

            CalendarManager calendarManager = this._owner.CalendarManager;
            Calendar cal = calendarManager.Calendar;
            CalendarMode minMode = this._owner.MinCalendarMode;
            bool offsetWithAdd = minMode != CalendarMode.Days;

            // make sure the flattened list of dates is up to date
            this.VerifyCombinedRanges();

            int index = Array.BinarySearch(this._combinedRanges, new CalendarDateRange(range.Start, range.End));

            if (index < 0)
                index = ~index;

            DateTime date = range.Start;
            DateTime end = range.End;

            for (int i = Math.Max(0, index - 1); i < this._combinedRanges.Length; i++)
            {
                CalendarDateRange item = this._combinedRanges[i];

                // if the range ended before the date we're up to bail out
                if (item.End < date)
                    continue;

                // process all dates up to the range start
                while (date < item.Start)
                {
                    if (0 == (disabledDays & 1 << (int)cal.GetDayOfWeek(date)))
                        dates.Add(date);

                    // only add 1 date per item
                    if (offsetWithAdd)
                        date = calendarManager.AddItemOffset(calendarManager.GetItemStartDate(date, minMode), 1, minMode);

                    if (date >= end)
                        return;

                    date = date.AddTicks(TimeSpan.TicksPerDay);
                }

                if (end <= item.End)
                    return;

                // start with the day after this range
                date = item.End.AddTicks(TimeSpan.TicksPerDay);
            }

            while (date <= end)
            {
                if (0 == (disabledDays & 1 << (int)cal.GetDayOfWeek(date)))
                    dates.Add(date);

                // only add 1 date per item
                if (offsetWithAdd)
                    date = calendarManager.AddItemOffset(calendarManager.GetItemStartDate(date, minMode), 1, minMode);

                if (date >= end)
                    return;

                date = date.AddTicks(TimeSpan.TicksPerDay);
            }
        }
        #endregion //AddAvailableDates

        #region GetAvailableDate
        internal DateTime? GetAvailableDate(DateTime start, DateTime end, bool next)
        {
            this.VerifyCombinedRanges();

            // find out where the item is
            CalendarDateRange range = new CalendarDateRange(start, end);

            // find out where it is or would be
            int index = Array.BinarySearch(this._combinedRanges, range);

            if (index < 0)
                index = ~index;

            // the index could be "after" the item to compare since the combined
            // range could have started before the specified range but end after
            // the specified range started
            if (index > 0 && this._combinedRanges[index - 1].Contains(range))
                return null;

            // the index provided could be "before" the item to compare since 
            // they could have the same start dates but the one in the combined
            // list has a later end date
            if (index < this._combinedRanges.Length && this._combinedRanges[index].Contains(range))
                return null;

            if (next)
                return GetNextAvailableDate(ref start, ref end, index);
            else
                return GetPreviousAvailableDate(ref start, ref end, index);
        }
        #endregion //GetAvailableDate

        #endregion //Internal

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