using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Collections;
using System.Diagnostics;
using System.Windows.Data;
using System.Linq;


#pragma warning disable 1574
using Infragistics.Services;
using Infragistics.Collections.Services;

namespace Infragistics.Controls.Schedules.Services





{
    #region WorkingHoursCollection Class

    /// <summary>
    /// Collection used for specifying working hours.
    /// </summary>
    /// <remarks>
    /// <para class="body">
    /// <b>WorkingHoursCollection</b> class is a collection of <see cref="TimeRange"/> instances. It's used for specifying working hours.
    /// </para>
    /// </remarks>
    /// <seealso cref="ScheduleSettings.WorkingHours"/>
    /// <seealso cref="DaySettings.WorkingHours"/>
    /// <seealso cref="ScheduleSettings.DaysOfWeek"/>
    /// <seealso cref="Resource.DaysOfWeek"/>
    public class WorkingHoursCollection : ObservableCollectionExtended<TimeRange>
	{
		#region Member Vars

        private static WorkingHoursCollection g_defaultWorkingHours;
        private bool _isReadOnly;

		#endregion // Member Vars

		#region Constructor

        /// <summary>
        /// Initializes a new instance of <see cref="WorkingHoursCollection"/> object.
        /// </summary>
        public WorkingHoursCollection( )
		{
		}

        /// <summary>
        /// Initializes a new instance of <see cref="WorkingHoursCollection"/> object.
        /// </summary>
        /// <param name="item">This item will be added to the collection.</param>
        public WorkingHoursCollection( TimeRange item )
        {
            this.Add( item );
        }

        /// <summary>
        /// Initializes a new instance of <see cref="WorkingHoursCollection"/> object.
        /// </summary>
        /// <param name="workHours">These items will be added to the collection.</param>
        public WorkingHoursCollection( IEnumerable<TimeRange> workHours )
        {
            if ( null != workHours )
            {
                foreach ( TimeRange ii in workHours )
                    this.Add( ii );
            }
        }

		#endregion // Constructor

        #region Base Overrides

        #region OnItemAdding

        /// <summary>
        /// Overridden. Validates the specified time range instance to ensure that it's start time and end time are valid.
        /// </summary>
        /// <param name="item">TimeRange instance to validate.</param>
        protected override void OnItemAdding( TimeRange item )
        {
            base.OnItemAdding( item );

            if ( _isReadOnly )
				CoreUtilities.RaiseReadOnlyCollectionException();

            if ( item.End - item.Start <= TimeSpan.Zero )
                new ArgumentOutOfRangeException( "TimeRange", "The end time must be after the start time." );

            this.ValidateState( item );
        } 

        #endregion // OnItemAdding

        #endregion // Base Overrides

        #region Properties

        #region Internal Properties

        #region DefaultWorkingHours

        /// <summary>
        /// Returns default working hours from 9:00 AM to 5:00 PM.
        /// </summary>
        internal static WorkingHoursCollection DefaultWorkingHours
        {
            get
            {
                if ( null == g_defaultWorkingHours )
                {
                    WorkingHoursCollection workingHours = new WorkingHoursCollection( );
                    workingHours.Add( new TimeSpan( 9, 0, 0 ), new TimeSpan( 17, 0, 0 ) );
                    workingHours._isReadOnly = true;

                    g_defaultWorkingHours = workingHours;
                }

                return g_defaultWorkingHours;
            }
        }

        #endregion // DefaultWorkingHours

        #endregion // Internal Properties

        #endregion // Properties

        #region Methods

        #region Public Methods

        #region Add

        /// <summary>
        /// Adds a new TimeRange instance based on the specified values.
        /// </summary>
        /// <param name="start">Start time of the time range.</param>
        /// <param name="end">End time of the time range.</param>
        public void Add( TimeSpan start, TimeSpan end )
        {
            this.Add( new TimeRange( start, end ) );
        }

        #endregion // Add 

        #endregion // Public Methods
        
        #region Private Methods
        
        #region ValidateState

        /// <summary>
        /// Ensures that the working hours don't span over more than 24 hours.
        /// </summary>
        private void ValidateState( TimeRange itemBeingAdded )
        {
            if ( this.Count > 0 )
            {
				ScheduleUtilities.AggregateEnumerable<TimeRange> items = new ScheduleUtilities.AggregateEnumerable<TimeRange>(
                    this, new TimeRange[] { itemBeingAdded } );

                TimeSpan min = items.Min( ii => ii.Start );
                TimeSpan max = items.Max( ii => ii.End );

                if ( min < TimeSpan.Zero || max > TimeSpan.FromDays( 1 ) )
					throw new ArgumentOutOfRangeException("TimeRange", ScheduleUtilities.GetString("LE_WorkingHoursSpanTooBige")); // "Working hours must be within the span of 24 hours."
            }
        } 

        #endregion // ValidateState

        #endregion // Private Methods

        #endregion // Methods
    }

    #endregion // WorkingHoursCollection Class
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