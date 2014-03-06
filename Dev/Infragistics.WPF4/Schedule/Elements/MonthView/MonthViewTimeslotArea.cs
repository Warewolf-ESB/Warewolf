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

namespace Infragistics.Controls.Schedules.Primitives
{
	/// <summary>
	/// Represents the timeslots for a specific logical day in the <see cref="XamMonthView"/>
	/// </summary>
	[TemplatePart(Name = PartDayHeaderPanel, Type = typeof(ScheduleStackPanel))]
	public class MonthViewTimeslotArea : TimeslotArea
	{
		#region Member Variables

		private const string PartDayHeaderPanel = "DayHeaderPanel";
		
		private ScheduleStackPanel _dayHeaderPanel;

		#endregion // Member Variables

		#region Constructor
		static MonthViewTimeslotArea()
		{

			MonthViewTimeslotArea.DefaultStyleKeyProperty.OverrideMetadata(typeof(MonthViewTimeslotArea), new FrameworkPropertyMetadata(typeof(MonthViewTimeslotArea)));

		}

		/// <summary>
		/// Initializes a new <see cref="MonthViewTimeslotArea"/>
		/// </summary>
		public MonthViewTimeslotArea()
		{



		}
		#endregion //Constructor

		#region Base class overrides

		#region OnApplyTemplate
		/// <summary>
		/// Invoked when the template for the control has been applied.
		/// </summary>
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			if (_dayHeaderPanel != null)
			{
				_dayHeaderPanel.ClearValue(ScheduleControlBase.ControlProperty);
				_dayHeaderPanel.Items = null;
			}

			_dayHeaderPanel = this.GetTemplateChild(PartDayHeaderPanel) as ScheduleStackPanel;

			if (null != _dayHeaderPanel)
			{
				var areaAdapter = this.AreaAdapter as MonthViewTimeslotAreaAdapter;

				_dayHeaderPanel.Orientation = Orientation.Horizontal;
				ScheduleControlBase.SetControl(_dayHeaderPanel, ScheduleControlBase.GetControl(this));

				if (null != areaAdapter)
					_dayHeaderPanel.Items = areaAdapter.DayHeaders;
			}
		}
		#endregion //OnApplyTemplate

		#region OnAreaAdapterChanged
		internal override void OnAreaAdapterChanged()
		{
			base.OnAreaAdapterChanged();

			if (null != _dayHeaderPanel)
			{
				var areaAdapter = this.AreaAdapter as MonthViewTimeslotAreaAdapter;
				_dayHeaderPanel.Items = areaAdapter != null ? areaAdapter.DayHeaders : null;
			}
		}
		#endregion // OnAreaAdapterChanged

		#region SetProviderBrushes
		internal override void SetProviderBrushes()
		{
			if (!this.IsBrushVersionBindingInitialized)
				return;

			ScheduleControlBase ctrl = ScheduleUtilities.GetControl(this);
			CalendarBrushProvider brushProvider = ScheduleUtilities.GetCalendarGroupBrushProvider(this, ctrl);

			if ( brushProvider != null )
				this.ComputedBorderBrush = brushProvider.GetBrush(this.BorderBrushId);
		} 
		#endregion // SetProviderBrushes

		#endregion //Base class overrides

		#region Properties

		#region Start

		private static readonly DependencyPropertyKey StartPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("Start",
			typeof(DateTime), typeof(MonthViewTimeslotArea), DateTime.MinValue, null);

		/// <summary>
		/// Identifies the read-only <see cref="Start"/> dependency property
		/// </summary>
		public static readonly DependencyProperty StartProperty = StartPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the logical day for the first day in the week.
		/// </summary>
		/// <seealso cref="StartProperty"/>
		public DateTime Start
		{
			get
			{
				return (DateTime)this.GetValue(MonthViewTimeslotArea.StartProperty);
			}
			internal set
			{
				this.SetValue(MonthViewTimeslotArea.StartPropertyKey, value);
			}
		}

		#endregion //Start

		#region End

		private static readonly DependencyPropertyKey EndPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("End",
			typeof(DateTime), typeof(MonthViewTimeslotArea), DateTime.MaxValue, null);

		/// <summary>
		/// Identifies the read-only <see cref="End"/> dependency property
		/// </summary>
		public static readonly DependencyProperty EndProperty = EndPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the logical date for the last day of the week
		/// </summary>
		/// <seealso cref="EndProperty"/>
		public DateTime End
		{
			get
			{
				return (DateTime)this.GetValue(MonthViewTimeslotArea.EndProperty);
			}
			internal set
			{
				this.SetValue(MonthViewTimeslotArea.EndPropertyKey, value);
			}
		}

		#endregion //End

		#region WeekNumber

		private static readonly DependencyPropertyKey WeekNumberPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("WeekNumber",
			typeof(int), typeof(MonthViewTimeslotArea), 0, null);

		/// <summary>
		/// Identifies the read-only <see cref="WeekNumber"/> dependency property
		/// </summary>
		public static readonly DependencyProperty WeekNumberProperty = WeekNumberPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the week # that the element represents.
		/// </summary>
		/// <seealso cref="WeekNumberProperty"/>
		public int WeekNumber
		{
			get
			{
				return (int)this.GetValue(MonthViewTimeslotArea.WeekNumberProperty);
			}
			internal set
			{
				this.SetValue(MonthViewTimeslotArea.WeekNumberPropertyKey, value);
			}
		}

		#endregion //WeekNumber

		#region WeekHeaderWidth

		internal static readonly DependencyPropertyKey WeekHeaderWidthPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("WeekHeaderWidth",
			typeof(double), typeof(MonthViewTimeslotArea), double.NaN, null);

		/// <summary>
		/// Identifies the read-only <see cref="WeekHeaderWidth"/> dependency property
		/// </summary>
		public static readonly DependencyProperty WeekHeaderWidthProperty = WeekHeaderWidthPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns or sets the preferred width for the <see cref="MonthViewWeekHeader"/>
		/// </summary>
		/// <seealso cref="WeekHeaderWidthProperty"/>
		public double WeekHeaderWidth
		{
			get
			{
				return (double)this.GetValue(MonthViewTimeslotArea.WeekHeaderWidthProperty);
			}
			internal set
			{
				this.SetValue(MonthViewTimeslotArea.WeekHeaderWidthPropertyKey, value);
			}
		}

		#endregion //WeekHeaderWidth

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