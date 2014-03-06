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
	/// An object that exposes tooltip information for an activity inside a <see cref="XamDateNavigator"/>
	/// </summary>
	public class DayActivityToolTipInfo : DependencyObject
	{
		#region Private Members

		private XamScheduleDataManager _dm;

		#endregion //Private members

		#region Constructor

		/// <summary>
		/// Initializes a new <see cref="DayActivityToolTipInfo"/>
		/// </summary>
		internal DayActivityToolTipInfo(DateTime date, ActivityBase activity, XamScheduleDataManager dm, CalendarBrushProvider provider)
		{
			this._dm = dm;

			this.Date = date;
			this.Activity = activity;

			if (dm != null)
			{
				if (ScheduleUtilities.GetIsReminderActive(activity, dm))
					this.SetValue(ReminderVisibilityPropertyKey, KnownBoxes.VisibilityVisibleBox);
				
				IEnumerable<ActivityCategory> categories =  dm.ResolveActivityCategories(activity);

				Color? firstCategoryColor = null;

				// Get the first category that has a color
				if (categories != null)
				{
					this.SetValue(CategoriesPropertyKey, categories);

					IEnumerator<ActivityCategory> enumerator = categories.GetEnumerator();

					while (enumerator.MoveNext())
					{
						firstCategoryColor = enumerator.Current.Color;
						if (firstCategoryColor != null)
							break;
					}
				}

				if (firstCategoryColor != null)
				{
					this.CategoryBackground = ScheduleUtilities.GetActivityCategoryBrush(firstCategoryColor.Value, ActivityCategoryBrushId.Background);
					this.CategoryForeground = ScheduleUtilities.GetActivityCategoryBrush(firstCategoryColor.Value, ActivityCategoryBrushId.Foreground);
				}
				else
				{
					if (provider != null)
						this.CategoryForeground = provider.GetBrush(CalendarBrushId.ToolTipForeground);
				}
			}
		}

		#endregion //Constructor

		#region Properties
		
		#region Public Properties

		#region Activity

		private static readonly DependencyPropertyKey ActivityPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("Activity",
			typeof(ActivityBase), typeof(DayActivityToolTipInfo), null, null);

		/// <summary>
		/// Identifies the read-only <see cref="Activity"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ActivityProperty = ActivityPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the associated activity object. (read-only)
		/// </summary>
		/// <seealso cref="ActivityProperty"/>
		public ActivityBase Activity
		{
			get
			{
				return (ActivityBase)this.GetValue(DayActivityToolTipInfo.ActivityProperty);
			}
			internal set
			{
				this.SetValue(DayActivityToolTipInfo.ActivityPropertyKey, value);
			}
		}

		#endregion //Activity

		#region AlignEndTime

		internal static readonly DependencyPropertyKey AlignEndTimePropertyKey = DependencyPropertyUtilities.RegisterReadOnly("AlignEndTime",
			typeof(bool), typeof(DayActivityToolTipInfo), KnownBoxes.TrueBox, null);

		/// <summary>
		/// Identifies the read-only <see cref="AlignEndTime"/> dependency property
		/// </summary>
		public static readonly DependencyProperty AlignEndTimeProperty = AlignEndTimePropertyKey.DependencyProperty;

		/// <summary>
		/// Returns whether the end time should be aligned (read-only)
		/// </summary>
		/// <seealso cref="AlignEndTimeProperty"/>
		public bool AlignEndTime
		{
			get
			{
				return (bool)this.GetValue(DayActivityToolTipInfo.AlignEndTimeProperty);
			}
			internal set
			{
				this.SetValue(DayActivityToolTipInfo.AlignEndTimePropertyKey, value);
			}
		}

		#endregion //AlignEndTime

		#region Categories

		private static readonly DependencyPropertyKey CategoriesPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("Categories",
			typeof(IEnumerable<ActivityCategory>), typeof(DayActivityToolTipInfo),
			null,
			null
			);

		/// <summary>
		/// Identifies the read-only <see cref="Categories"/> dependency property
		/// </summary>

		[InfragisticsFeature(FeatureName = "ActivityCategories", Version = "11.1")]

		public static readonly DependencyProperty CategoriesProperty = CategoriesPropertyKey.DependencyProperty;


		/// <summary>
		/// Returns the categories associated with the <see cref="Activity"/> represented by the element.
		/// </summary>
		/// <seealso cref="CategoriesProperty"/>

		[InfragisticsFeature(FeatureName = "ActivityCategories", Version = "11.1")]

		public IEnumerable<ActivityCategory> Categories
		{
			get
			{
				return (IEnumerable<ActivityCategory>)this.GetValue(DayActivityToolTipInfo.CategoriesProperty);
			}
			private set
			{
				this.SetValue(DayActivityToolTipInfo.CategoriesPropertyKey, value);
			}
		}

		#endregion //Categories

		#region CategoryBackground

		private static readonly DependencyPropertyKey CategoryBackgroundPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("CategoryBackground",
			typeof(Brush), typeof(DayActivityToolTipInfo), ScheduleUtilities.GetBrush(Colors.Transparent), null);

		/// <summary>
		/// Identifies the read-only <see cref="CategoryBackground"/> dependency property
		/// </summary>
		public static readonly DependencyProperty CategoryBackgroundProperty = CategoryBackgroundPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the brush to use for the background of the category indicator/>
		/// </summary>
		/// <seealso cref="CategoryBackgroundProperty"/>
		public Brush CategoryBackground
		{
			get
			{
				return (Brush)this.GetValue(DayActivityToolTipInfo.CategoryBackgroundProperty);
			}
			internal set
			{
				this.SetValue(DayActivityToolTipInfo.CategoryBackgroundPropertyKey, value);
			}
		}

		#endregion //CategoryBackground

		#region CategoryForeground

		private static readonly DependencyPropertyKey CategoryForegroundPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("CategoryForeground",
			typeof(Brush), typeof(DayActivityToolTipInfo), ScheduleUtilities.GetBrush(Colors.Black), null);

		/// <summary>
		/// Identifies the read-only <see cref="CategoryForeground"/> dependency property
		/// </summary>
		public static readonly DependencyProperty CategoryForegroundProperty = CategoryForegroundPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the brush to use for the Foreground of the category/>
		/// </summary>
		/// <seealso cref="CategoryForegroundProperty"/>
		public Brush CategoryForeground
		{
			get
			{
				return (Brush)this.GetValue(DayActivityToolTipInfo.CategoryForegroundProperty);
			}
			internal set
			{
				this.SetValue(DayActivityToolTipInfo.CategoryForegroundPropertyKey, value);
			}
		}

		#endregion //CategoryForeground

		#region Date

		private static readonly DependencyPropertyKey DatePropertyKey = DependencyPropertyUtilities.RegisterReadOnly("Date",
			typeof(DateTime), typeof(DayActivityToolTipInfo), DateTime.Now.Date, null);

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
				return (DateTime)this.GetValue(DayActivityToolTipInfo.DateProperty);
			}
			internal set
			{
				this.SetValue(DayActivityToolTipInfo.DatePropertyKey, value);
			}
		}

		#endregion //Date

		#region ReminderVisibility

		private static readonly DependencyPropertyKey ReminderVisibilityPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("ReminderVisibility",
			typeof(Visibility), typeof(DayActivityToolTipInfo), KnownBoxes.VisibilityCollapsedBox, null);

		/// <summary>
		/// Identifies the read-only <see cref="ReminderVisibility"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ReminderVisibilityProperty = ReminderVisibilityPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the visibility of the reminder indicator (read-only)
		/// </summary>
		/// <seealso cref="ReminderVisibilityProperty"/>
		public Visibility ReminderVisibility
		{
			get
			{
				return (Visibility)this.GetValue(DayActivityToolTipInfo.ReminderVisibilityProperty);
			}
			internal set
			{
				this.SetValue(DayActivityToolTipInfo.ReminderVisibilityPropertyKey, value);
			}
		}

		#endregion //ReminderVisibility

		#endregion //Public Properties	
		
		#region Internal Properties

		#region DataManager

		internal XamScheduleDataManager DataManager { get { return _dm; } }

		#endregion //DataManager

		#endregion //Internal Properties	
    		
		#endregion //Properties
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