using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Media;
using System.Windows.Data;

namespace Infragistics.Controls.Schedules.Primitives
{
	/// <summary>
	/// Represents a specific <see cref="Timeslot"/>
	/// </summary>
	[TemplateVisualState(Name = VisualStateUtilities.StateWorkingHour, GroupName = VisualStateUtilities.GroupWorkingHour)]
	[TemplateVisualState(Name = VisualStateUtilities.StateNonWorkingHour, GroupName = VisualStateUtilities.GroupWorkingHour)]
	[TemplateVisualState(Name = VisualStateUtilities.StateSelected, GroupName = VisualStateUtilities.GroupSelection)]
	[TemplateVisualState(Name = VisualStateUtilities.StateUnselected, GroupName = VisualStateUtilities.GroupSelection)]
	[DesignTimeVisible(false)]
	public class TimeslotPresenter : TimeslotPresenterBase
	{
		#region Constructor
		static TimeslotPresenter()
		{

			TimeslotPresenter.DefaultStyleKeyProperty.OverrideMetadata(typeof(TimeslotPresenter), new FrameworkPropertyMetadata(typeof(TimeslotPresenter)));

		}

		/// <summary>
		/// Initializes a new <see cref="TimeslotPresenter"/>
		/// </summary>
		public TimeslotPresenter()
		{



		} 
		#endregion //Constructor

		#region Base class overrides

		#region ChangeVisualState
		internal override void ChangeVisualState(bool useTransitions)
		{
			base.ChangeVisualState(useTransitions);

			Timeslot ts = this.Timeslot as Timeslot;

			if (null != ts && ts.IsSelected)
				VisualStateUtilities.GoToState(this, useTransitions, VisualStateUtilities.StateSelected, VisualStateUtilities.StateUnselected);
			else
				this.GoToState(VisualStateUtilities.StateUnselected, useTransitions);

			if (null != ts && ts.IsWorkingHour)
				VisualStateUtilities.GoToState(this, useTransitions, VisualStateUtilities.StateWorkingHour, VisualStateUtilities.StateNonWorkingHour);
			else
				this.GoToState(VisualStateUtilities.StateNonWorkingHour, useTransitions);
		}

		#endregion //ChangeVisualState

		#region SetProviderBrushes

		internal override void SetProviderBrushes()
		{
			if (!this.IsBrushVersionBindingInitialized)
				return;

			Timeslot ts = this.Timeslot as Timeslot;
			CalendarBrushProvider brushProvider = ScheduleUtilities.GetCalendarGroupBrushProvider(this);

			bool isFirstInDay = this.IsFirstInDay && !TimeslotPanel.GetIsFirstItem(this);
			bool isLastInDay = this.IsLastInDay && !TimeslotPanel.GetIsLastItem(this);

			#region Set background brush

			Brush br = null;

			if (brushProvider != null)
			{
				CalendarBrushId brushId;

				if (ts.IsSelected)
					brushId = CalendarBrushId.SelectedTimeslotBackground;
				else
					if (ts.IsWorkingHour)
						brushId = CalendarBrushId.WorkingHourTimeslotBackground;
					else
						brushId = CalendarBrushId.NonWorkingHourTimeslotBackground;

				br = brushProvider.GetBrush(brushId);
			}

			if (br != null)
				this.ComputedBackground = br;

			#endregion //Set background brush	
	
			#region Set border brush

			br = null;

			if (brushProvider != null)
			{
				CalendarBrushId brushId;

				if ( isLastInDay )
					brushId = CalendarBrushId.TimeslotDayBorder;
				else
				if (this.IsLastInMajor)
					brushId = CalendarBrushId.TimeslotMajorBorder;
				else
				{
					if (ts.IsWorkingHour)
						brushId = CalendarBrushId.WorkingHourTimeslotMinorBorder;
					else
						brushId = CalendarBrushId.NonWorkingHourTimeslotMinorBorder;
				}

				br = brushProvider.GetBrush(brushId);
			}

			if (br != null)
				this.ComputedBorderBrush = br;

			#endregion //Set border brush

			#region Set Day border brush

			if (brushProvider != null)
			{
				Thickness thickness = new Thickness();
				bool isVisible = false;
				bool isVertical = this.Orientation == System.Windows.Controls.Orientation.Vertical;

				if (isFirstInDay)
				{
					if (isLastInDay)
					{
						isVisible = true;
						thickness = isVertical ? new Thickness(0, 1, 0, 1) : new Thickness(1, 0, 1, 0);
					}
					else
					{
						isVisible = true;
						thickness = isVertical ? new Thickness(0, 1, 0, 0) : new Thickness(1, 0, 0, 0);
					}
				}
				else
				{
					if (isLastInDay)
					{
						isVisible = true;
						thickness = isVertical ? new Thickness(0, 0, 0, 1) : new Thickness(0, 0, 1, 0);
					}
				}

				if (isVisible)
				{
					this.SetValue(DayBorderBrushProperty, brushProvider.GetBrush(CalendarBrushId.TimeslotDayBorder));
					this.SetValue(DayBorderThicknessProperty, thickness);
					this.SetValue(DayBorderVisibilityProperty, KnownBoxes.VisibilityVisibleBox);
				}
				else
				{
					this.ClearValue(DayBorderBrushProperty);
					this.ClearValue(DayBorderThicknessProperty);
					this.ClearValue(DayBorderVisibilityProperty);
				}
			}

			#endregion //Set Day border brush
		}

		#endregion //SetProviderBrushes	

		#endregion // Base class overrides

		#region Properties

		#region DayBorderBrush

		/// <summary>
		/// Identifies the <see cref="DayBorderBrush"/> dependency property
		/// </summary>
		public static readonly DependencyProperty DayBorderBrushProperty = DependencyPropertyUtilities.Register(
	  "DayBorderBrush", typeof(Brush), typeof(TimeslotPresenter),
			DependencyPropertyUtilities.CreateMetadata(null)
			);

		/// <summary>
		/// Determines the brush that will be used to draw borders on timeslots whose start and/or end times border days
		/// </summary>
		/// <seealso cref="TimeslotPresenterBase.IsFirstInDay"/>
		/// <seealso cref="TimeslotPresenterBase.IsLastInDay"/>
		/// <seealso cref="DayBorderThickness"/>
		/// <seealso cref="DayBorderBrushProperty"/>
		public Brush DayBorderBrush
		{
			get
			{
				return (Brush)this.GetValue(TimeslotPresenter.DayBorderBrushProperty);
			}
			set
			{
				this.SetValue(TimeslotPresenter.DayBorderBrushProperty, value);
			}
		}

		#endregion //DayBorderBrush

		#region DayBorderThickness

		/// <summary>
		/// Identifies the <see cref="DayBorderThickness"/> dependency property
		/// </summary>
		public static readonly DependencyProperty DayBorderThicknessProperty = DependencyPropertyUtilities.Register(
	  "DayBorderThickness", typeof(Thickness), typeof(TimeslotPresenter),
			DependencyPropertyUtilities.CreateMetadata(new Thickness())
			);

		/// <summary>
		/// Determines which border to draw around timeslots whose start and/or end times border days
		/// </summary>
		/// <seealso cref="TimeslotPresenterBase.IsFirstInDay"/>
		/// <seealso cref="TimeslotPresenterBase.IsLastInDay"/>
		/// <seealso cref="DayBorderBrush"/>
		/// <seealso cref="DayBorderThicknessProperty"/>
		public Thickness DayBorderThickness
		{
			get
			{
				return (Thickness)this.GetValue(TimeslotPresenter.DayBorderThicknessProperty);
			}
			set
			{
				this.SetValue(TimeslotPresenter.DayBorderThicknessProperty, value);
			}
		}

		#endregion //DayBorderThickness

		#region DayBorderVisibility

		/// <summary>
		/// Identifies the <see cref="DayBorderVisibility"/> dependency property
		/// </summary>
		public static readonly DependencyProperty DayBorderVisibilityProperty = DependencyPropertyUtilities.Register(
	  "DayBorderVisibility", typeof(Visibility), typeof(TimeslotPresenter),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.VisibilityCollapsedBox)
			);

		/// <summary>
		/// Determines the visibility of the day borders
		/// </summary>
		/// <seealso cref="TimeslotPresenterBase.IsFirstInDay"/>
		/// <seealso cref="TimeslotPresenterBase.IsLastInDay"/>
		/// <seealso cref="DayBorderThickness"/>
		/// <seealso cref="DayBorderBrushProperty"/>
		public Visibility DayBorderVisibility
		{
			get
			{
				return (Visibility)this.GetValue(TimeslotPresenter.DayBorderVisibilityProperty);
			}
			set
			{
				this.SetValue(TimeslotPresenter.DayBorderVisibilityProperty, value);
			}
		}

		#endregion //DayBorderVisibility

		#region IsSelected

		private static readonly DependencyPropertyKey IsSelectedPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("IsSelected",
			typeof(bool), typeof(TimeslotPresenter),
			KnownBoxes.FalseBox, 
			new PropertyChangedCallback(OnVisualStatePropertyChanged)
			);

		/// <summary>
		/// Identifies the <see cref="IsSelected"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsSelectedProperty = IsSelectedPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns a boolean indicating if the object is currently selected.
		/// </summary>
		/// <seealso cref="IsSelectedProperty"/>
		public bool IsSelected
		{
			get
			{
				return (bool)this.GetValue(TimeslotPresenter.IsSelectedProperty);
			}
			internal set
			{
				this.SetValue(TimeslotPresenter.IsSelectedPropertyKey, KnownBoxes.FromValue(value));
			}
		}

		#endregion //IsSelected

		#region IsWorkingHour

		private static readonly DependencyPropertyKey IsWorkingHourPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("IsWorkingHour",
			typeof(bool), typeof(TimeslotPresenter),
			KnownBoxes.FalseBox, 
			new PropertyChangedCallback(OnVisualStatePropertyChanged)
			);

		/// <summary>
		/// Identifies the <see cref="IsWorkingHour"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsWorkingHourProperty = IsWorkingHourPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns a boolean indicating if the timeslot represents a working hour for the current associated <see cref="ResourceCalendar"/>
		/// </summary>
		/// <seealso cref="IsWorkingHourProperty"/>
		public bool IsWorkingHour
		{
			get
			{
				return (bool)this.GetValue(TimeslotPresenter.IsWorkingHourProperty);
			}
			internal set
			{
				this.SetValue(TimeslotPresenter.IsWorkingHourPropertyKey, KnownBoxes.FromValue(value));
			}
		}

		#endregion //IsWorkingHour

		#endregion // Properties

		#region Methods
	
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