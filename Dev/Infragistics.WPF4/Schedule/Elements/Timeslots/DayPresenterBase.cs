using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using Infragistics.Controls.Editors;

namespace Infragistics.Controls.Schedules.Primitives
{
	/// <summary>
	/// Represents a single logical day in a <see cref="XamMonthView"/>
	/// </summary>
	[TemplateVisualState(Name = VisualStateUtilities.StateRegularDay,	GroupName = VisualStateUtilities.GroupDay)]
	[TemplateVisualState(Name = VisualStateUtilities.StateToday,		GroupName = VisualStateUtilities.GroupDay)]
	[TemplateVisualState(Name = VisualStateUtilities.StateSelected,		GroupName = VisualStateUtilities.GroupSelection)]
	[TemplateVisualState(Name = VisualStateUtilities.StateUnselected,	GroupName = VisualStateUtilities.GroupSelection)]
	public abstract class DayPresenterBase : TimeRangePresenterBase
	{
		#region Member Variables

		#endregion // Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="DayPresenterBase"/>
		/// </summary>
		protected DayPresenterBase()
		{
		}
		#endregion //Constructor

		#region Base class overrides

		#region ChangeVisualState
		internal override void ChangeVisualState(bool useTransitions)
		{
			var ts = this.Timeslot as DayPresenterBaseAdapter;

			if (null != ts && ts.IsSelected)
				VisualStateUtilities.GoToState(this, useTransitions, VisualStateUtilities.StateSelected, VisualStateUtilities.StateUnselected);
			else
				this.GoToState(VisualStateUtilities.StateUnselected, useTransitions);

			if (this.IsToday)
				VisualStateUtilities.GoToState(this, useTransitions, VisualStateUtilities.StateToday, VisualStateUtilities.StateRegularDay);
			else
				this.GoToState(VisualStateUtilities.StateRegularDay, useTransitions);

			base.ChangeVisualState(useTransitions);
		}
		#endregion //ChangeVisualState

		#region Kind

		internal override TimeRangeKind Kind { get { return TimeRangeKind.Day; } }

		#endregion //Kind	

		#region SetProviderBrushes
		internal override void SetProviderBrushes()
		{
			ScheduleControlBase ctrl = ScheduleUtilities.GetControl(this);
			CalendarBrushProvider brushProvider = ScheduleUtilities.GetCalendarGroupBrushProvider(this, ctrl);

			if (brushProvider == null)
				return;

			this.ComputedBackground = brushProvider.GetBrush(this.BackgroundBrushId);
			this.ComputedBorderBrush = brushProvider.GetBrush(this.BorderBrushId);
		}

		#endregion // SetProviderBrushes

		#endregion // Base class overrides

		#region Properties

		#region Public Properties

		#region IsSelected

		private bool _isSelected;

		private static readonly DependencyPropertyKey IsSelectedPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("IsSelected",
			typeof(bool), typeof(DayPresenterBase),
			KnownBoxes.FalseBox,
			new PropertyChangedCallback(OnIsSelectedChanged)
			);

		/// <summary>
		/// Identifies the <see cref="IsSelected"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsSelectedProperty = IsSelectedPropertyKey.DependencyProperty;

		private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			DayPresenterBase item = (DayPresenterBase)d;
			item._isSelected = (bool)e.NewValue;
			item.UpdateVisualState();
		}

		/// <summary>
		/// Returns a boolean indicating if the object is currently selected.
		/// </summary>
		/// <seealso cref="IsSelectedProperty"/>
		public bool IsSelected
		{
			get
			{
				return _isSelected;
			}
			internal set
			{
				this.SetValue(DayPresenterBase.IsSelectedPropertyKey, KnownBoxes.FromValue(value));
			}
		}

		#endregion //IsSelected

		#region IsToday

		private bool _isToday;

		private static readonly DependencyPropertyKey IsTodayPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("IsToday",
			typeof(bool), typeof(DayPresenterBase),
			KnownBoxes.FalseBox,
			new PropertyChangedCallback(OnIsTodayChanged)
			);

		/// <summary>
		/// Identifies the read-only <see cref="IsToday"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsTodayProperty = IsTodayPropertyKey.DependencyProperty;

		private static void OnIsTodayChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			DayPresenterBase instance = (DayPresenterBase)d;

			bool isToday = true.Equals(e.NewValue);

			instance._isToday = isToday;

			// bring the today item to the front so its border shows
			if (isToday)
				Canvas.SetZIndex(instance, 1);
			else
				instance.ClearValue(Canvas.ZIndexProperty);

			instance.UpdateVisualState();
		}

		/// <summary>
		/// Returns a boolean indicating if the element represents the current logical day.
		/// </summary>
		/// <seealso cref="IsTodayProperty"/>
		public bool IsToday
		{
			get
			{
				return _isToday;
			}
			internal set
			{
				this.SetValue(DayPresenterBase.IsTodayPropertyKey, KnownBoxes.FromValue(value));
			}
		}

		#endregion //IsToday

		#endregion // Public Properties

		#region Internal Properties
		internal abstract CalendarBrushId BackgroundBrushId { get; }

		internal abstract CalendarBrushId BorderBrushId { get; }
		#endregion // Internal Properties 

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