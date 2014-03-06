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
using Infragistics.AutomationPeers;

namespace Infragistics.Controls.Schedules.Primitives
{
	/// <summary>
	/// Represents the header for a week in a <see cref="XamMonthView"/>
	/// </summary>
	public class MonthViewWeekHeader : ResourceCalendarElementBase
	{
		#region Constructor
		static MonthViewWeekHeader()
		{

			MonthViewWeekHeader.DefaultStyleKeyProperty.OverrideMetadata(typeof(MonthViewWeekHeader), new FrameworkPropertyMetadata(typeof(MonthViewWeekHeader)));

		}

		/// <summary>
		/// Initializes a new <see cref="MonthViewWeekHeader"/>
		/// </summary>
		public MonthViewWeekHeader()
		{



		}
		#endregion //Constructor

		#region Base class overrides

		#region OnApplyTemplate
		/// <summary>
		/// Invoked when the templated has been applied to the control
		/// </summary>
		public override void OnApplyTemplate()
		{
			var ctrl = ScheduleUtilities.GetControlFromElementTree(this) as XamMonthView;

			if (null != ctrl)
			{
				ctrl.AddWeekHeader(this);
				this.ShowWeekNumbers = ctrl.ShowWeekNumbers;
			}

			base.OnApplyTemplate();
		} 
		#endregion // OnApplyTemplate

		#region OnCreateAutomationPeer
		/// <summary>
		/// Returns an automation peer that exposes the <see cref="MonthViewWeekHeader"/> to UI Automation.
		/// </summary>
		/// <returns>A <see cref="MonthViewWeekHeaderAutomationPeer"/></returns>
		protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer()
		{
			return new MonthViewWeekHeaderAutomationPeer(this);
		}
		#endregion // OnCreateAutomationPeer

		#region OnMouseLeftButtonDown
		/// <summary>
		/// Invoked when the left mouse button is pressed down on the element.
		/// </summary>
		/// <param name="e">Provides information about the mouse operation</param>
		protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
		{
			// AS 3/7/12 TFS102945
			//ClickHelper.OnMouseLeftButtonDown(this, e, this.RaiseClick, true);
			ScheduleUtilities.OnTouchAwareClickHelperDown(this, e, this.RaiseClick, true);

			base.OnMouseLeftButtonDown(e);
		}
		#endregion // OnMouseLeftButtonDown

		#endregion // Base class overrides

		#region Properties

		#region ComputedForeground

		private static readonly DependencyPropertyKey ComputedForegroundPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("ComputedForeground",
			typeof(Brush), typeof(MonthViewWeekHeader), null, null);

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
				return (Brush)this.GetValue(MonthViewWeekHeader.ComputedForegroundProperty);
			}
			internal set
			{
				this.SetValue(MonthViewWeekHeader.ComputedForegroundPropertyKey, value);
			}
		}

		#endregion //ComputedForeground

		#region End

		/// <summary>
		/// Identifies the <see cref="End"/> dependency property
		/// </summary>
		public static readonly DependencyProperty EndProperty = DependencyPropertyUtilities.Register("End",
			typeof(DateTime), typeof(MonthViewWeekHeader),
			DependencyPropertyUtilities.CreateMetadata(DateTime.Today)
			);

		/// <summary>
		/// Returns or sets the date that represents the last day of the week.
		/// </summary>
		/// <seealso cref="EndProperty"/>
		public DateTime End
		{
			get
			{
				return (DateTime)this.GetValue(MonthViewWeekHeader.EndProperty);
			}
			set
			{
				this.SetValue(MonthViewWeekHeader.EndProperty, value);
			}
		}

		#endregion //End

		#region ShowWeekNumbers

		private static readonly DependencyPropertyKey ShowWeekNumbersPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("ShowWeekNumbers",
			typeof(bool), typeof(MonthViewWeekHeader), KnownBoxes.FalseBox, null);

		/// <summary>
		/// Identifies the read-only <see cref="ShowWeekNumbers"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ShowWeekNumbersProperty = ShowWeekNumbersPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns a boolean indicating whether week numbers should be displayed.
		/// </summary>
		/// <seealso cref="ShowWeekNumbersProperty"/>
		public bool ShowWeekNumbers
		{
			get
			{
				return (bool)this.GetValue(MonthViewWeekHeader.ShowWeekNumbersProperty);
			}
			internal set
			{
				this.SetValue(MonthViewWeekHeader.ShowWeekNumbersPropertyKey, KnownBoxes.FromValue(value));
			}
		}

		#endregion //ShowWeekNumbers

		#region Start

		/// <summary>
		/// Identifies the <see cref="Start"/> dependency property
		/// </summary>
		public static readonly DependencyProperty StartProperty = DependencyPropertyUtilities.Register("Start",
			typeof(DateTime), typeof(MonthViewWeekHeader),
			DependencyPropertyUtilities.CreateMetadata(DateTime.Today)
			);

		/// <summary>
		/// Returns or sets the date that represents the start of the week.
		/// </summary>
		/// <seealso cref="StartProperty"/>
		public DateTime Start
		{
			get
			{
				return (DateTime)this.GetValue(MonthViewWeekHeader.StartProperty);
			}
			set
			{
				this.SetValue(MonthViewWeekHeader.StartProperty, value);
			}
		}

		#endregion //Start

		#region WeekNumber

		/// <summary>
		/// Identifies the <see cref="WeekNumber"/> dependency property
		/// </summary>
		public static readonly DependencyProperty WeekNumberProperty = DependencyPropertyUtilities.Register("WeekNumber",
			typeof(int), typeof(MonthViewWeekHeader),
			DependencyPropertyUtilities.CreateMetadata(0)
			);

		/// <summary>
		/// Returns or sets the week number of the week that the element represents
		/// </summary>
		/// <seealso cref="WeekNumberProperty"/>
		public int WeekNumber
		{
			get
			{
				return (int)this.GetValue(MonthViewWeekHeader.WeekNumberProperty);
			}
			set
			{
				this.SetValue(MonthViewWeekHeader.WeekNumberProperty, value);
			}
		}

		#endregion //WeekNumber
		
		#endregion // Properties

		#region Methods

		#region Private Methods

		#region RaiseClick
		private void RaiseClick()
		{
			var ctrl = ScheduleUtilities.GetControlFromElementTree(this) as XamMonthView;

			if (null != ctrl)
				ctrl.OnWeekHeaderClick(this.ResourceCalendar, this.Start);
		}
		#endregion // RaiseClick

		#region SetProviderBrushes

		internal override void SetProviderBrushes()
		{
			if (!this.IsBrushVersionBindingInitialized)
				return;

			var owningControl = ScheduleUtilities.GetControl(this);
			CalendarBrushProvider brushProvider = ScheduleUtilities.GetCalendarGroupBrushProvider(this, owningControl);

			if (brushProvider == null)
				return;

			this.ComputedBackground = brushProvider.GetBrush(CalendarBrushId.WeekHeaderBackground);
			this.ComputedBorderBrush = brushProvider.GetBrush(CalendarBrushId.WeekHeaderBorder);
			this.ComputedForeground = brushProvider.GetBrush(CalendarBrushId.WeekHeaderForeground);
		}

		#endregion //SetProviderBrushes

		#endregion //Private Methods

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