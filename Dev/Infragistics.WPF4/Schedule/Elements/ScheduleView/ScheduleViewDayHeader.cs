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
using System.ComponentModel;

namespace Infragistics.Controls.Schedules.Primitives
{
	/// <summary>
	/// Custom element that displays the name of the associated logical day below the timeslot headers.
	/// </summary>
	/// <seealso cref="ScheduleViewTimeslotHeaderArea"/>
	[DesignTimeVisible(false)]
	public class ScheduleViewDayHeader : Control, ICalendarBrushClient
	{
		#region Constructor
		static ScheduleViewDayHeader()
		{

			ScheduleViewDayHeader.DefaultStyleKeyProperty.OverrideMetadata(typeof(ScheduleViewDayHeader), new FrameworkPropertyMetadata(typeof(ScheduleViewDayHeader)));
			UIElement.FocusableProperty.OverrideMetadata(typeof(ScheduleViewDayHeader), new FrameworkPropertyMetadata(KnownBoxes.FalseBox)); // AS 12/16/10 TFS61923

		}

		/// <summary>
		/// Initializes a new <see cref="ScheduleViewDayHeader"/>
		/// </summary>
		public ScheduleViewDayHeader()
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
			ScheduleControlBase control = this.Control;

			if (control != null)
				control.BindToBrushVersion(this);

			base.OnApplyTemplate();

			this.SetProviderBrushes();
		}
		#endregion //OnApplyTemplate

		#region OnCreateAutomationPeer
		/// <summary>
		/// Returns an automation peer that exposes the <see cref="ScheduleViewDayHeader"/> to UI Automation.
		/// </summary>
		/// <returns>A <see cref="ScheduleViewDayHeaderAutomationPeer"/></returns>
		protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer()
		{
			return new ScheduleViewDayHeaderAutomationPeer(this);
		} 
		#endregion // OnCreateAutomationPeer
		
		#endregion // Base class overrides

		#region Properties

		#region ComputedBackground

		private static readonly DependencyPropertyKey ComputedBackgroundPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("ComputedBackground",
			typeof(Brush), typeof(ScheduleViewDayHeader), null, null);

		/// <summary>
		/// Identifies the read-only <see cref="ComputedBackground"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ComputedBackgroundProperty = ComputedBackgroundPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the brush to use for the Background based on the element's state and the associated <see cref="XamScheduleDataManager"/>'s <see cref="XamScheduleDataManager.ColorScheme"/>
		/// </summary>
		/// <seealso cref="ComputedBackgroundProperty"/>
		public Brush ComputedBackground
		{
			get
			{
				return (Brush)this.GetValue(ScheduleViewDayHeader.ComputedBackgroundProperty);
			}
			internal set
			{
				this.SetValue(ScheduleViewDayHeader.ComputedBackgroundPropertyKey, value);
			}
		}

		#endregion //ComputedBackground

		#region ComputedBorderBrush

		private static readonly DependencyPropertyKey ComputedBorderBrushPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("ComputedBorderBrush",
			typeof(Brush), typeof(ScheduleViewDayHeader), null, null);

		/// <summary>
		/// Identifies the read-only <see cref="ComputedBorderBrush"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ComputedBorderBrushProperty = ComputedBorderBrushPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the brush to use for the BorderBrush based on the element's state and the associated <see cref="XamScheduleDataManager"/>'s <see cref="XamScheduleDataManager.ColorScheme"/>
		/// </summary>
		/// <seealso cref="ComputedBorderBrushProperty"/>
		public Brush ComputedBorderBrush
		{
			get
			{
				return (Brush)this.GetValue(ScheduleViewDayHeader.ComputedBorderBrushProperty);
			}
			internal set
			{
				this.SetValue(ScheduleViewDayHeader.ComputedBorderBrushPropertyKey, value);
			}
		}

		#endregion //ComputedBorderBrush

		#region ComputedForeground

		private static readonly DependencyPropertyKey ComputedForegroundPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("ComputedForeground",
			typeof(Brush), typeof(ScheduleViewDayHeader), null, null);

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
				return (Brush)this.GetValue(ScheduleViewDayHeader.ComputedForegroundProperty);
			}
			internal set
			{
				this.SetValue(ScheduleViewDayHeader.ComputedForegroundPropertyKey, value);
			}
		}

		#endregion //ComputedForeground

		#region Control

		private ScheduleControlBase Control
		{
			get
			{
				UIElement parent = VisualTreeHelper.GetParent(this) as UIElement;

				return parent != null ? ScheduleUtilities.GetControl(parent) : null;
			}
		}

		#endregion //Control	
    
		#region Date

		private static readonly DependencyPropertyKey DatePropertyKey = DependencyPropertyUtilities.RegisterReadOnly("Date",
			typeof(DateTime), typeof(ScheduleViewDayHeader), DateTime.Today, null);

		/// <summary>
		/// Identifies the read-only <see cref="Date"/> dependency property
		/// </summary>
		public static readonly DependencyProperty DateProperty = DatePropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the logical day that the element represents
		/// </summary>
		/// <seealso cref="DateProperty"/>
		public DateTime Date
		{
			get
			{
				return (DateTime)this.GetValue(ScheduleViewDayHeader.DateProperty);
			}
			internal set
			{
				this.SetValue(ScheduleViewDayHeader.DatePropertyKey, value);
			}
		}

		#endregion //Date

		#endregion // Properties

		#region Methods

		#region SetProviderBrushes

		private void SetProviderBrushes()
		{
			ScheduleControlBase control = this.Control;

			CalendarBrushProvider brushProvider = control != null ? control.DefaultBrushProvider : null;

			if (brushProvider == null)
				return;

			Brush br = brushProvider.GetBrush(CalendarBrushId.WorkingHourTimeslotBackground);

			if (br != null)
				this.ComputedBackground = br;

			br = brushProvider.GetBrush(CalendarBrushId.TimeslotHeaderTickmarkScheduleView);

			if (br != null)
				this.ComputedBorderBrush = br;

			br = brushProvider.GetBrush(CalendarBrushId.TimeslotHeaderForegroundScheduleView);
			if (br != null)
				this.ComputedForeground = br;

		}

		#endregion //SetProviderBrushes

		#endregion // Methods

		#region ICalendarBrushClient Members

		void ICalendarBrushClient.OnBrushVersionChanged()
		{
			this.SetProviderBrushes();
		}

		#endregion
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