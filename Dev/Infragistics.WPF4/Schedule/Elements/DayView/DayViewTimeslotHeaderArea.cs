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
	/// Custom <see cref="TimeslotHeaderArea"/> used within a <see cref="XamDayView"/>
	/// </summary>
	public class DayViewTimeslotHeaderArea : TimeslotHeaderArea
	{
		#region Constructor
		static DayViewTimeslotHeaderArea()
		{

			DayViewTimeslotHeaderArea.DefaultStyleKeyProperty.OverrideMetadata(typeof(DayViewTimeslotHeaderArea), new FrameworkPropertyMetadata(typeof(DayViewTimeslotHeaderArea)));

		}

		/// <summary>
		/// Initializes a new <see cref="DayViewTimeslotHeaderArea"/>
		/// </summary>
		public DayViewTimeslotHeaderArea()
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

			this.VerifyState();
		}

		#endregion //OnApplyTemplate

		#region OnCalendarHeaderAreaVisibilityChanged
		internal override void OnCalendarHeaderAreaVisibilityChanged()
		{
			this.VerifyState();
		}
		#endregion // OnCalendarHeaderAreaVisibilityChanged

		#endregion //Base class overrides

		#region Properties

		#region Public Properties

		#region ComputedMargin

		private static readonly DependencyPropertyKey ComputedMarginPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("ComputedMargin",
			typeof(Thickness), typeof(DayViewTimeslotHeaderArea), new Thickness(0, 0, 0, 1), null);

		/// <summary>
		/// Identifies the read-only <see cref="ComputedMargin"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ComputedMarginProperty = ComputedMarginPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the margin to use for the Panel based on the element's state and the associated <see cref="XamScheduleDataManager"/>'s <see cref="XamScheduleDataManager.ColorScheme"/>
		/// </summary>
		/// <seealso cref="ComputedMarginProperty"/>
		public Thickness ComputedMargin
		{
			get
			{
				return (Thickness)this.GetValue(DayViewTimeslotHeaderArea.ComputedMarginProperty);
			}
			internal set
			{
				this.SetValue(DayViewTimeslotHeaderArea.ComputedMarginPropertyKey, value);
			}
		}

		#endregion //ComputedMargin

		#endregion //Public Properties	
    
		#endregion //Properties	
 
		#region Methods

		#region Private Methods

		#region VerifyState

		private void VerifyState()
		{
			var control = ScheduleControlBase.GetControl(this);

			if (control == null || control.CalendarHeaderAreaVisibilityResolved == Visibility.Collapsed)
			{
				// Note: the default value is Thickness(0, 0, 0, 1);
				this.ClearValue(ComputedMarginPropertyKey);
			}
			else
				this.ComputedMargin = new Thickness(0, 0, 0, 4);
		}

		#endregion //VerifyState

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