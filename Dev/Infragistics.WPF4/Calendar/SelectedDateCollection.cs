using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using Infragistics.Controls.Editors;
using Infragistics.Collections;
using System.Globalization;

namespace Infragistics.Controls.Editors.Primitives
{
    /// <summary>
    /// A custom collection of <see cref="DateTime"/> instances used to represent the selected dates within the <see cref="CalendarBase"/>
    /// </summary>
    internal class SelectedDateCollection : DateCollection
    {
        #region Member Variables

        private CalendarBase _owner;
 
        #endregion //Member Variables

        #region Constructor
        internal SelectedDateCollection(CalendarBase owner)
        {
            CalendarUtilities.ValidateNull("owner", owner);

            this._owner = owner;
        } 
        #endregion //Constructor

        #region Base class overrides

		#region GetAllowedDates
		/// <summary>
		/// Returns an array of the dates that are allowed to be added for the specified range.
		/// </summary>
		/// <param name="dateRange">An object representing the range of dates to add</param>
		/// <returns>An array of the dates that are allowed to be added.</returns>
		protected override DateTime[] GetAllowedDates(DateRange dateRange)
		{
			return this._owner.GetSelectableDates(new DateRange(dateRange.Start.Date, dateRange.End.Date));

		}
		#endregion //GetAllowedDates

		#region OnDatesChanged
		/// <summary>
		/// Invoked when the selection has changed indicating the added and removed dates.
		/// </summary>
		/// <param name="added">List of dates added</param>
		/// <param name="removed">List of dates that were removed from the collection</param>
		protected override void OnDatesChanged(IList<DateTime> added, IList<DateTime> removed)
		{
			base.OnDatesChanged(added, removed);

            this.RaiseSelectionChanged(added, removed);
		}
		#endregion //OnDatesChanged

        #region OnCollectionChanged
        /// <summary>
        /// Invoked when the collection has been changed.
        /// </summary>
        /// <param name="e">Provides information about the change</param>
        protected override void OnCollectionChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
			int count = this.Items.Count;

            // keep the selected date in sync
			// only update the selectdate if this is the public SelectedDates collection, i.e. not the clone
			// used during a selection drag operation
			if (this._owner.SelectedDates == this)
			{
				
				this._owner.SelectedDate = count == 0 ? null : (DateTime?)this.Items[0];
			}

            base.OnCollectionChanged(e);

			// JJD 5/9/11 - TFS74024
			// Re-factored - Moved logic into VerifyWeekSelection
			this.VerifyWeekSelection();

			// JJD 5/9/11 - TFS74024
			// Call VerifySelectedDayStatesAsync to make sure the
			// selected days states are in sync
			if ( _owner.SelectedDates == this )
				this._owner.VerifySelectedDayStatesAsync();
        }

        #endregion //OnCollectionChanged

		#region VerifyNewDateCount
		/// <summary>
		/// Used to determine if the number of items that will be in the collection is allowed.
		/// </summary>
		/// <param name="newItemCount">The new item count. This is passed by reference and when <paramref name="validate"/> is false, this should be updated to reflect the constrained value if it exceeds the maximum</param>
		/// <param name="validate">True if an exception should be thrown if the new count exceeds the allowed</param>
		protected override void VerifyNewDateCount(ref int newItemCount, bool validate)
		{
			int maxAllowed = this._owner.MaxSelectedDatesResolved;

			if (newItemCount > maxAllowed)
			{
				if (validate)
					throw new InvalidOperationException(CalendarUtilities.GetString("LE_MaxSelectedDatesExceeded", this.Count + 1, this._owner.MaxSelectedDatesResolved));
				else
					newItemCount = maxAllowed;
			}
		}
		#endregion //VerifyNewDateCount

        #endregion //Base class overrides

        #region Methods

        #region Internal

        #region ContainsSelection
        internal bool ContainsSelection(DateTime start, DateTime end)
        {
			return this.IntersectsWith(new DateRange(start, end));
		}
        #endregion //ContainsSelection

        #region EnsureWithinMinMax
        internal void EnsureWithinMinMax()
        {
            if (this.Count > 0)
            {
                DateTime minDate = this._owner.MinDateResolved;
                DateTime maxDate = this._owner.MaxDateResolved;

				this.AllowedRange = new DateRange(minDate, maxDate);
            }
        } 
        #endregion //EnsureWithinMinMax

        #region IsSelected
        internal bool IsSelected(DateTime date)
        {
            return base.ContainsDate(date);
        }
        #endregion //IsSelected

        #region IsSelected (range)
        internal bool IsSelected(DateRange range)
        {
			range.Normalize();
			range.RemoveTime();

			DateTime dt = range.Start;
			DateTime dtEnd = range.End;
			
			if (!ContainsDate(dtEnd))
				return false;

			// JJD 3/8/11 - TFS66513
			CalendarManager cm = _owner.CalendarManager;

			while (dt < dtEnd)
			{
				if (!ContainsDate(dt))
					return false;

				// JJD 3/8/11 - TFS66513
				// Use the calendar manager for adding days because it deals gracefully with
				// min and ma dates without blowing up
				//dt = dt.AddDays(1);
				dt = cm.AddDays(dt, 1);
			}

			return true;
        }
        #endregion //IsSelected

        #region RaiseSelectionChanged
        private void RaiseSelectionChanged(IList<DateTime> selected, IList<DateTime> unselected)
        {
            // AS 10/13/08 TFS8974
            this._owner.OnSelectedStateChanged(selected, unselected);

            this._owner.RaiseSelectedDatesChanged(unselected, selected, false);
        } 
        #endregion //RaiseSelectionChanged

		// JJD 5/9/11 - TFS74024 - Re-factored
		#region VerifyWeekSelection

		internal void VerifyWeekSelection()
		{
			int count = this.Items.Count;

			// if week selection is supported and there are at least 7 days selected
			// then synchronize the IsSelected property of each CalendarWeekNumber in the selected range
			if (count > 6 && _owner.SupportsWeekSelectionMode && _owner.SelectedDatesInternal == this)
			{
				CalendarWeekRule? rule = _owner.WeekRuleInternal;
				DayOfWeek? dow = _owner.FirstDayOfWeekInternal;

				// create a temp list of the selected dates and sort it
				List<DateTime> tempList = new List<DateTime>(this.Items);
				tempList.Sort();
				int year;
				int lastWkNum = -1;

				// walk over the selected dates looking for breaks in week number
				for (int i = 0; i < count; i++)
				{
					DateTime dt = tempList[i];
					int wkNum = this._owner.CalendarManager.GetWeekNumberForDate(dt, rule, dow, out year);

					// when the week number changes get the associated CalendarWeekNumber and
					// have it verify its selected status
					if (wkNum != lastWkNum)
					{
						lastWkNum = wkNum;
						CalendarWeekNumber wkNumElement = this._owner.GetWeekNumber(dt);

						if (wkNumElement != null)
							wkNumElement.VerifyIsSelected();
					}
				}
			}
		}

		#endregion //VerifyWeekSelection	
    
        #endregion //Internal

        #region Private

        #endregion //Private

        #region Public

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