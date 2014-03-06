using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.ComponentModel;

namespace Infragistics.Controls.Schedules.Primitives
{
	/// <summary>
	/// Custom element that represents the location of the current time in a <see cref="TimeslotHeaderArea"/> when the current date is within the <see cref="ScheduleControlBase.VisibleDates"/> of the owning control.
	/// </summary>
	[DesignTimeVisible(false)]
	public class CurrentTimeIndicator : Control, ICalendarBrushClient
	{
		#region Constructor
		static CurrentTimeIndicator()
		{

			CurrentTimeIndicator.DefaultStyleKeyProperty.OverrideMetadata(typeof(CurrentTimeIndicator), new FrameworkPropertyMetadata(typeof(CurrentTimeIndicator)));
			UIElement.FocusableProperty.OverrideMetadata(typeof(CurrentTimeIndicator), new FrameworkPropertyMetadata(KnownBoxes.FalseBox)); // AS 12/16/10 TFS61923

		}

		/// <summary>
		/// Initializes a new <see cref="CurrentTimeIndicator"/>
		/// </summary>
		public CurrentTimeIndicator()
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
			UIElement parent = VisualTreeHelper.GetParent(this) as UIElement;

			ScheduleControlBase control = parent != null ? ScheduleUtilities.GetControl(parent) : null;

			if (control != null)
				control.BindToBrushVersion(this);

			base.OnApplyTemplate();

			this.SetProviderBrushes();
		}
		#endregion //OnApplyTemplate

		#endregion // Base class overrides

		#region Properties

		#region ComputedBackground

		private static readonly DependencyPropertyKey ComputedBackgroundPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("ComputedBackground",
			typeof(Brush), typeof(CurrentTimeIndicator), null, null);

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
				return (Brush)this.GetValue(CurrentTimeIndicator.ComputedBackgroundProperty);
			}
			internal set
			{
				this.SetValue(CurrentTimeIndicator.ComputedBackgroundPropertyKey, value);
			}
		}

		#endregion //ComputedBackground

		#region ComputedBorderBrush

		private static readonly DependencyPropertyKey ComputedBorderBrushPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("ComputedBorderBrush",
			typeof(Brush), typeof(CurrentTimeIndicator), null, null);

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
				return (Brush)this.GetValue(CurrentTimeIndicator.ComputedBorderBrushProperty);
			}
			internal set
			{
				this.SetValue(CurrentTimeIndicator.ComputedBorderBrushPropertyKey, value);
			}
		}

		#endregion //ComputedBorderBrush

		#region Orientation

		/// <summary>
		/// Identifies the <see cref="Orientation"/> dependency property
		/// </summary>
		public static readonly DependencyProperty OrientationProperty = DependencyPropertyUtilities.Register("Orientation",
			typeof(Orientation), typeof(CurrentTimeIndicator),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.OrientationHorizontalBox)
			);

		/// <summary>
		/// Returns or sets the orientation of the indicator.
		/// </summary>
		/// <seealso cref="OrientationProperty"/>
		public Orientation Orientation
		{
			get
			{
				return (Orientation)this.GetValue(CurrentTimeIndicator.OrientationProperty);
			}
			set
			{
				this.SetValue(CurrentTimeIndicator.OrientationProperty, value);
			}
		}

		#endregion //Orientation

		#endregion // Properties

		#region Methods

		#region SetProviderBrushes

		private void SetProviderBrushes()
		{
			XamScheduleDataManager dm = ScheduleUtilities.GetDataManager(this);

			if (dm == null)
				return;

			CalendarBrushProvider brushProvider = dm.ColorSchemeResolved.DefaultBrushProvider;

			if ( brushProvider == null )
				return;

			Brush br = brushProvider.GetBrush(CalendarBrushId.CurrentTimeIndicatorBackground);

			if (br != null)
				this.ComputedBackground = br;

			br = brushProvider.GetBrush(CalendarBrushId.CurrentTimeIndicatorBorder);
			if (br != null)
				this.ComputedBorderBrush = br;

		}

		#endregion //SetProviderBrushes

		#endregion //Methods	
    
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