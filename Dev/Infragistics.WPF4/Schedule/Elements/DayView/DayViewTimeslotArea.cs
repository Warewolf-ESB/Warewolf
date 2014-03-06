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
	/// Represents the timeslots for a specific logical day in the <see cref="XamDayView"/>
	/// </summary>
	[TemplateVisualState(Name = VisualStateUtilities.StateRegularDay,	GroupName = VisualStateUtilities.GroupDay)]
	[TemplateVisualState(Name = VisualStateUtilities.StateToday,		GroupName = VisualStateUtilities.GroupDay)]
	public class DayViewTimeslotArea : TimeslotArea
	{
		#region Constructor
		static DayViewTimeslotArea()
		{

			DayViewTimeslotArea.DefaultStyleKeyProperty.OverrideMetadata(typeof(DayViewTimeslotArea), new FrameworkPropertyMetadata(typeof(DayViewTimeslotArea)));

		}

		/// <summary>
		/// Initializes a new <see cref="DayViewTimeslotArea"/>
		/// </summary>
		public DayViewTimeslotArea()
		{



		}
		#endregion //Constructor

		#region Base class overrides

		#region BrushIds

		internal override CalendarBrushId BorderBrushId
		{
			get
			{
				if (this.IsToday)
					return CalendarBrushId.CurrentDayBorder;

				return CalendarBrushId.DayBorder;
			}
		}

		#endregion //BrushIds

		#region ChangeVisualState
		internal override void ChangeVisualState(bool useTransitions)
		{
			if (this.IsToday)
				VisualStateUtilities.GoToState(this, useTransitions, VisualStateUtilities.StateToday, VisualStateUtilities.StateRegularDay);
			else
				this.GoToState(VisualStateUtilities.StateRegularDay, useTransitions);

			base.ChangeVisualState(useTransitions);
		}

		#endregion //ChangeVisualState

		#region IsTodayInternal
		internal override bool IsTodayInternal
		{
			get
			{
				return this.IsToday;
			}
			set
			{
				this.IsToday = value;
			}
		} 
		#endregion // IsTodayInternal

		#endregion // Base class overrides

		#region Properties

		#region IsToday

		private static readonly DependencyPropertyKey IsTodayPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("IsToday",
			typeof(bool), typeof(DayViewTimeslotArea),
			KnownBoxes.FalseBox,
			new PropertyChangedCallback(OnIsTodayChanged)
			);

		/// <summary>
		/// Identifies the read-only <see cref="IsToday"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsTodayProperty = IsTodayPropertyKey.DependencyProperty;

		private static void OnIsTodayChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			DayViewTimeslotArea instance = (DayViewTimeslotArea)d;

			// bring the today item to the front so its border shows
			if (true.Equals(e.NewValue))
				Canvas.SetZIndex(instance, 1);
			else
				instance.ClearValue(Canvas.ZIndexProperty);

			instance.UpdateVisualState();
		}

		/// <summary>
		/// Returns a boolean indicating if the element represents the current logical date.
		/// </summary>
		/// <seealso cref="IsTodayProperty"/>
		public bool IsToday
		{
			get
			{
				return (bool)this.GetValue(DayViewTimeslotArea.IsTodayProperty);
			}
			internal set
			{
				this.SetValue(DayViewTimeslotArea.IsTodayPropertyKey, KnownBoxes.FromValue(value));
			}
		}

		#endregion //IsToday

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