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
using System.Collections;

namespace Infragistics.Controls.Schedules.Primitives
{
	/// <summary>
	/// Represents the area of a <see cref="XamMonthView"/> that contains the weeks for a specific <see cref="CalendarGroupBase"/>
	/// </summary>
	[TemplatePart(Name = PartDayOfWeekHeadersPanel, Type = typeof(ScheduleItemsPanel))]
	public class MonthViewCalendarGroupTimeslotArea : CalendarGroupTimeslotArea
	{
		#region Member Variables

		private const string PartDayOfWeekHeadersPanel = "DayOfWeekHeadersPanel";

		private ScheduleItemsPanel _headersPanel;
		private IList _dayOfWeekHeaders;

		#endregion // Member Variables

		#region Constructor
		static MonthViewCalendarGroupTimeslotArea()
		{

			MonthViewCalendarGroupTimeslotArea.DefaultStyleKeyProperty.OverrideMetadata(typeof(MonthViewCalendarGroupTimeslotArea), new FrameworkPropertyMetadata(typeof(MonthViewCalendarGroupTimeslotArea)));

		}

		/// <summary>
		/// Initializes a new <see cref="MonthViewCalendarGroupTimeslotArea"/>
		/// </summary>
		public MonthViewCalendarGroupTimeslotArea()
		{



		}
		#endregion //Constructor

		#region Base class overrides
		
		#region GetBorderThickness

		internal override Thickness GetBorderThickness(double borderSize)
		{
			return new Thickness(borderSize);
		}

		#endregion //GetBorderThickness

		#region OnApplyTemplate
		/// <summary>
		/// Invoked when the template for the control has been applied.
		/// </summary>
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			ScheduleItemsPanel oldPanel = _headersPanel;
			_headersPanel = this.GetTemplateChild(PartDayOfWeekHeadersPanel) as ScheduleItemsPanel;

			if (oldPanel != _headersPanel)
			{
				if (null != oldPanel)
				{
					oldPanel.Items = null;
				}

				if (_headersPanel != null)
				{
					var ctrl = ScheduleControlBase.GetControl(this);

					if (null != ctrl)
						_headersPanel.Orientation = ctrl.CalendarGroupOrientation;

					_headersPanel.Items = this.DayOfWeekHeaders;
				}
			}
		}
		#endregion //OnApplyTemplate

		#region SetProviderBrushes

		internal override void SetProviderBrushes()
		{
			if (!this.IsBrushVersionBindingInitialized)
				return;

			var ctrl = ScheduleControlBase.GetControl(this);

			if (ctrl == null)
				return;

			CalendarBrushProvider brushProvider = ScheduleUtilities.GetCalendarGroupBrushProvider(this, ctrl);

			if (brushProvider == null)
				return;
			
			base.SetProviderBrushes();

			#region Set background

			if (this.HasCalendarAreaInControl)
			{
				Brush br = brushProvider.GetBrush(CalendarBrushId.MonthViewBackground);

				if (br != null)
					this.ComputedBackground = br;
			}
			else
				this.ClearValue(ComputedBackgroundPropertyKey);

			#endregion //Set background

		}

		#endregion //SetProviderBrushes

		#endregion // Base class overrides

		#region Properties

		#region WeekHeaderWidth

		internal static readonly DependencyPropertyKey WeekHeaderWidthPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("WeekHeaderWidth",
			typeof(double), typeof(MonthViewCalendarGroupTimeslotArea), double.NaN, null);

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
				return (double)this.GetValue(MonthViewCalendarGroupTimeslotArea.WeekHeaderWidthProperty);
			}
			internal set
			{
				this.SetValue(MonthViewCalendarGroupTimeslotArea.WeekHeaderWidthPropertyKey, value);
			}
		}

		#endregion //WeekHeaderWidth

		#region Internal Properties

		#region DayOfWeekHeaders

		/// <summary>
		/// Returns or sets the recyclable items that represent that days of week headers
		/// </summary>
		internal IList DayOfWeekHeaders
		{
			get
			{
				return _dayOfWeekHeaders;
			}
			set
			{
				if (value != _dayOfWeekHeaders)
				{
					_dayOfWeekHeaders = value;

					if (_headersPanel != null)
						_headersPanel.Items = value;
				}
			}
		}
		#endregion //DayOfWeekHeaders

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