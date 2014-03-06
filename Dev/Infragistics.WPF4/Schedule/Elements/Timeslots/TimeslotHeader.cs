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
using System.Windows.Data;
using Infragistics.Controls.Editors;

namespace Infragistics.Controls.Schedules.Primitives
{
	/// <summary>
	/// Represents the header for a given time range.
	/// </summary>
	public abstract class TimeslotHeader : TimeslotPresenterBase
	{
		#region Member Variables

		private bool _isBrushVersionBindingInitialized;
		private TimeslotHeaderTimePresenter _timePresenter;

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="TimeslotHeader"/>
		/// </summary>
		protected TimeslotHeader()
		{
		}
		#endregion //Constructor

		#region Base class overrides

		#region Kind

		internal override TimeRangeKind Kind { get { return TimeRangeKind.TimeHeader; } }

		#endregion //Kind	

		#region LocalRange
		internal override DateRange LocalRange
		{
			get
			{
				DateRange range = base.LocalRange;

				var adapter = this.Timeslot as TimeslotHeaderAdapter;
				var token = adapter == null ? null : adapter.TimeZoneToken;

				if ( null != token && null != token.Provider.LocalToken )
					range = new DateRange(token.ConvertToLocal(range.Start), token.ConvertToLocal(range.End));
				
				return range;
			}
		} 
		#endregion // LocalRange

		#region OnIsFirstInMajorChanged

		internal override void OnIsFirstInMajorChanged(bool oldValue, bool newValue)
		{
			if (this._timePresenter != null)
				this._timePresenter.InvalidateMeasure();
		}

		#endregion //OnIsFirstInMajorChanged	
    
		#region OnTimeSlotChanged

		internal override void OnTimeSlotChanged(TimeslotBase newValue, TimeslotBase oldValue)
		{
			base.OnTimeSlotChanged(newValue, oldValue);

			this._isBrushVersionBindingInitialized = false;

			if (newValue == null)
				this.ClearValue(ScheduleControlBase.BrushVersionProperty);
			else
			{
				this.VerifyBrushVersionBinding();

				if (this._timePresenter != null)
					this._timePresenter.InvalidateMeasure();
			}

		}

		#endregion //OnTimeSlotChanged	

		#region SetProviderBrushes

		internal override void SetProviderBrushes()
		{
			if (!this._isBrushVersionBindingInitialized)
				return;

			ScheduleControlBase control = this.ScheduleControl;

			if (control == null)
				return;

			CalendarBrushProvider brushProvider = control.DefaultBrushProvider;

			if (brushProvider == null)
				return;

			// AS 7/21/10 TFS36040
			// We don't want a backcolor on this element since the current time indicator 
			// is below this element. I was going to let this be null but I think we will
			// want the element to be hittested and so we'll need a background color even 
			// if its transparent.
			//
			//Brush br = brushProvider.GetBrush(this.BackgroundBrushId);
			Brush br = CalendarBrushProvider.TransparentBrush;

			if (br != null)
				this.ComputedBackground = br;

			br = brushProvider.GetBrush(this.ForegroundBrushId);

			if (br != null)
				this.ComputedForeground = br;

			br = brushProvider.GetBrush(this.TickmarkBrushId);

			if (br != null)
				this.TickmarkBrush = br;
		}

		#endregion //SetProviderBrushes

		#region VerifyBrushVersionBinding
		internal override void VerifyBrushVersionBinding()
		{
			if (!this._isBrushVersionBindingInitialized && this.Timeslot != null)
			{
				ScheduleControlBase control = this.ScheduleControl;
				if (control != null)
				{
					control.BindToBrushVersion(this);

					this._isBrushVersionBindingInitialized = true;
				}
			}
		}
		#endregion // VerifyBrushVersionBinding

		#endregion // Base class overrides

		#region Properties

		#region BackgroundBrushId

		// AS 7/21/10 TFS36040
		// We don't want to set a background since the time indicator is behind this element.
		//
		//internal virtual CalendarBrushId BackgroundBrushId { get { return CalendarBrushId.WorkingHourTimeslotBackground; } }

		#endregion //BackgroundBrushId	

		#region ComputedForeground

		private static readonly DependencyPropertyKey ComputedForegroundPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("ComputedForeground",
			typeof(Brush), typeof(TimeslotHeader), null, null);

		/// <summary>
		/// Identifies the read-only <see cref="ComputedForeground"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ComputedForegroundProperty = ComputedForegroundPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the brush to use for the Foreground based on the element's state and the associated <see cref="XamScheduleDataManager"/>'s <see cref="XamScheduleDataManager.ColorScheme"/>
		/// </summary>
		/// <seealso cref="ComputedForegroundProperty"/>
		public Brush ComputedForeground
		{
			get
			{
				return (Brush)this.GetValue(TimeslotHeader.ComputedForegroundProperty);
			}
			internal set
			{
				this.SetValue(TimeslotHeader.ComputedForegroundPropertyKey, value);
			}
		}

		#endregion //ComputedForeground

		#region ScheduleControl
		internal ScheduleControlBase ScheduleControl
		{
			get { return ScheduleUtilities.GetControl(this); }
		}
		#endregion // ScheduleControl

		#region ForegroundBrushId

		internal virtual CalendarBrushId ForegroundBrushId { get { return CalendarBrushId.TimeslotHeaderForegroundDayView; } }

		#endregion //ForegroundBrushId	

		#region LeadingTickmarkKind

		internal static readonly DependencyPropertyKey LeadingTickmarkKindPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("LeadingTickmarkKind",
			typeof(TimeslotTickmarkKind), typeof(TimeslotHeader), TimeslotTickmarkKind.Minor, null);

		/// <summary>
		/// Identifies the read-only <see cref="LeadingTickmarkKind"/> dependency property
		/// </summary>
		public static readonly DependencyProperty LeadingTickmarkKindProperty = LeadingTickmarkKindPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the kind of tickmark at the beginning of the timeslot (read-only)
		/// </summary>
		/// <seealso cref="LeadingTickmarkKindProperty"/>
		public TimeslotTickmarkKind LeadingTickmarkKind
		{
			get
			{
				return (TimeslotTickmarkKind)this.GetValue(TimeslotHeader.LeadingTickmarkKindProperty);
			}
			internal set
			{
				this.SetValue(TimeslotHeader.LeadingTickmarkKindPropertyKey, value);
			}
		}

		#endregion //LeadingTickmarkKind

		#region LeadingTickmarkVisibility

		internal static readonly DependencyPropertyKey LeadingTickmarkVisibilityPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("LeadingTickmarkVisibility",
			typeof(Visibility), typeof(TimeslotHeader), KnownBoxes.VisibilityCollapsedBox, null);

		/// <summary>
		/// Identifies the read-only <see cref="LeadingTickmarkVisibility"/> dependency property
		/// </summary>
		public static readonly DependencyProperty LeadingTickmarkVisibilityProperty = LeadingTickmarkVisibilityPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the visibility of the leading tickmark (read-only).
		/// </summary>
		/// <seealso cref="LeadingTickmarkVisibilityProperty"/>
		public Visibility LeadingTickmarkVisibility
		{
			get
			{
				return (Visibility)this.GetValue(TimeslotHeader.LeadingTickmarkVisibilityProperty);
			}
			internal set
			{
				this.SetValue(TimeslotHeader.LeadingTickmarkVisibilityPropertyKey, value);
			}
		}

		#endregion //LeadingTickmarkVisibility

		#region ShowAMPMDesignator

		internal static readonly DependencyPropertyKey ShowAMPMDesignatorPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("ShowAMPMDesignator",
			typeof(bool), typeof(TimeslotHeader),
			KnownBoxes.FalseBox,
			new PropertyChangedCallback(OnShowAMPMDesignatorChanged)
			);

		/// <summary>
		/// Identifies the read-only <see cref="ShowAMPMDesignator"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ShowAMPMDesignatorProperty = ShowAMPMDesignatorPropertyKey.DependencyProperty;

		private static void OnShowAMPMDesignatorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			TimeslotHeader instance = (TimeslotHeader)d;

			if (null != instance._timePresenter)
				instance._timePresenter.InvalidateMeasure();
		}

		/// <summary>
		/// Returns a boolean indicating if the element represents the first visible time slot for the AM/PM hours.
		/// </summary>
		/// <seealso cref="ShowAMPMDesignatorProperty"/>
		public bool ShowAMPMDesignator
		{
			get
			{
				return (bool)this.GetValue(TimeslotHeader.ShowAMPMDesignatorProperty);
			}
		}

		#endregion //ShowAMPMDesignator

		#region TickmarkBrush

		private static readonly DependencyPropertyKey TickmarkBrushPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("TickmarkBrush",
			typeof(Brush), typeof(TimeslotHeader), null, null);

		/// <summary>
		/// Identifies the read-only <see cref="TickmarkBrush"/> dependency property
		/// </summary>
		public static readonly DependencyProperty TickmarkBrushProperty = TickmarkBrushPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the brush to use for the background of the tickmarks
		/// </summary>
		/// <seealso cref="TickmarkBrushProperty"/>
		public Brush TickmarkBrush
		{
			get
			{
				return (Brush)this.GetValue(TimeslotHeader.TickmarkBrushProperty);
			}
			internal set
			{
				this.SetValue(TimeslotHeader.TickmarkBrushPropertyKey, value);
			}
		}

		#endregion //TickmarkBrush

		#region TickmarkBrushId

		internal virtual CalendarBrushId TickmarkBrushId { get { return CalendarBrushId.TimeslotHeaderTickmarkDayView; } }

		#endregion //TickmarkBrushId

		#region TrailingTickmarkKind

		internal static readonly DependencyPropertyKey TrailingTickmarkKindPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("TrailingTickmarkKind",
			typeof(TimeslotTickmarkKind), typeof(TimeslotHeader), TimeslotTickmarkKind.Minor, null);

		/// <summary>
		/// Identifies the read-only <see cref="TrailingTickmarkKind"/> dependency property
		/// </summary>
		public static readonly DependencyProperty TrailingTickmarkKindProperty = TrailingTickmarkKindPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the kind of tickmark at the end of the timeslot (read-only)
		/// </summary>
		/// <seealso cref="TrailingTickmarkKindProperty"/>
		public TimeslotTickmarkKind TrailingTickmarkKind
		{
			get
			{
				return (TimeslotTickmarkKind)this.GetValue(TimeslotHeader.TrailingTickmarkKindProperty);
			}
			internal set
			{
				this.SetValue(TimeslotHeader.TrailingTickmarkKindPropertyKey, value);
			}
		}

		#endregion //TrailingTickmarkKind

		#region TrailingTickmarkVisibility

		internal static readonly DependencyPropertyKey TrailingTickmarkVisibilityPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("TrailingTickmarkVisibility",
			typeof(Visibility), typeof(TimeslotHeader), KnownBoxes.VisibilityVisibleBox, null);

		/// <summary>
		/// Identifies the read-only <see cref="TrailingTickmarkVisibility"/> dependency property
		/// </summary>
		public static readonly DependencyProperty TrailingTickmarkVisibilityProperty = TrailingTickmarkVisibilityPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the visibility of the trailing tickmark (read-only).
		/// </summary>
		/// <seealso cref="TrailingTickmarkVisibilityProperty"/>
		public Visibility TrailingTickmarkVisibility
		{
			get
			{
				return (Visibility)this.GetValue(TimeslotHeader.TrailingTickmarkVisibilityProperty);
			}
			internal set
			{
				this.SetValue(TimeslotHeader.TrailingTickmarkVisibilityPropertyKey, value);
			}
		}

		#endregion //TrailingTickmarkVisibility
	
		#endregion // Properties

		#region Methods

		#region Internal Methods

		#region InitializeTimePresenter

		internal void InitializeTimePresenter(TimeslotHeaderTimePresenter timePresenter)
		{
			this._timePresenter = timePresenter;
		}

		#endregion //InitializeTimePresenter

		#endregion //Internal Methods	
    
		#endregion // Methods
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