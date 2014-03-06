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
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Infragistics.Collections;

namespace Infragistics.Controls.Schedules.Primitives
{
	/// <summary>
	/// An object that exposes tooltip information for a day inside a <see cref="XamDateNavigator"/>
	/// </summary>
	public class DayToolTipInfo : DependencyObject
	{
		#region Private Members

		private ReadOnlyObservableCollection<DayActivityToolTipInfo> _activitiesReadOnly;

		#endregion //Private members	

		#region Constructor

		internal DayToolTipInfo(DateTime date, List<DayActivityToolTipInfo> activities, XamScheduleDataManager dm)
		{
			this.Date = date;

			activities.Sort(new ActivitySortComparer(dm));

			TimeZoneInfoProvider tzProvider = dm.TimeZoneInfoProviderResolved;

			TimeZoneToken token = tzProvider != null ? tzProvider.LocalToken : null;

			if (token != null)
			{
				// loop over the sorted activities and set the AlignEndTimeProperty
				foreach (DayActivityToolTipInfo actInfo in activities)
				{
					ActivityBase activity = actInfo.Activity;
					DateTime start = activity.GetStartLocal(token);

					// if the start times can't be aligned then the end time can't be
					if ( start.Date != date )
						actInfo.SetValue(DayActivityToolTipInfo.AlignEndTimePropertyKey, KnownBoxes.FalseBox);
				}
			}

			_activitiesReadOnly = new ReadOnlyObservableCollection<DayActivityToolTipInfo>(new ObservableCollectionExtended<DayActivityToolTipInfo>(activities));
		}

		#endregion //Constructor	
 
		#region Properties

		#region ActivityToolTipInfos

		/// <summary>
		/// Returns a read only collection of <see cref="DayActivityToolTipInfo"/> objects
		/// </summary>
		public ReadOnlyObservableCollection<DayActivityToolTipInfo> ActivityToolTipInfos
		{
			get
			{
				return _activitiesReadOnly;
			}
		}

		#endregion //ActivityToolTipInfos

		#region Date

		private static readonly DependencyPropertyKey DatePropertyKey = DependencyPropertyUtilities.RegisterReadOnly("Date",
			typeof(DateTime), typeof(DayToolTipInfo), DateTime.Now.Date, null);

		/// <summary>
		/// Identifies the read-only <see cref="Date"/> dependency property
		/// </summary>
		public static readonly DependencyProperty DateProperty = DatePropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the date (read-only)
		/// </summary>
		/// <seealso cref="DateProperty"/>
		public DateTime Date
		{
			get
			{
				return (DateTime)this.GetValue(DayToolTipInfo.DateProperty);
			}
			internal set
			{
				this.SetValue(DayToolTipInfo.DatePropertyKey, value);
			}
		}

		#endregion //Date
    
   		#endregion //Properties	

		#region ActivitySortComparer class
		private class ActivitySortComparer : IComparer<DayActivityToolTipInfo>
		{
			#region Member Variables

			private XamScheduleDataManager _dm;
			private TimeZoneToken _token;

			#endregion // Member Variables

			#region Constructor
			internal ActivitySortComparer(XamScheduleDataManager dm)
			{
				_dm = dm;

				_token = dm.TimeZoneInfoProviderResolved.LocalToken;
			}
			#endregion // Constructor

			#region IComparer<DayActivityToolTipInfo> Members

			int IComparer<DayActivityToolTipInfo>.Compare(DayActivityToolTipInfo tti1, DayActivityToolTipInfo tti2)
			{
				if (tti1 == tti2)
					return 0;

				if (tti1 == null)
					return -1;
				else if (tti2 == null)
					return 1;

				ActivityBase a1 = tti1.Activity;
				ActivityBase a2 = tti2.Activity;

				if (a1 == a2)
					return 0;

				if (a1 == null)
					return -1;
				else if (a2 == null)
					return 1;

				int compare = 0;

				// starting date (earliest first)
				var a1Range = ScheduleUtilities.ConvertFromUtc(_token, a1);
				var a2Range = ScheduleUtilities.ConvertFromUtc(_token, a2);
				compare = a1Range.Start.CompareTo(a2Range.Start);

				if (compare == 0)
				{
					// duration 
					compare = a1Range.End.CompareTo(a2Range.End);

					if (compare == 0)
					{
						// within the same calendar outlook sorts by subject
						compare = string.Compare(a1.Subject, a2.Subject, StringComparison.CurrentCulture);

						if (compare == 0)
						{
							// if the start and end date are the same we need to fall back to checking something. we don't 
							// have an idea of the actual "creation" date or anything similar to compare. using the original 
							// position in the query result isn't ideal either as that is the result of a query and therefore
							// 2 successive executions of the same query could result in different ordered return values.
							// so we'll fall back to doing an ordinal (i.e. byte) comparison of the id's so at least the 
							// position of 2 activities with the same start and end are consistent.
							compare = string.CompareOrdinal(a1.Id, a2.Id);
						}
					}
				}

				return compare;
			}

			#endregion //IComparer<ActivityBase> Members
		}
		#endregion // ActivitySortComparer class
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