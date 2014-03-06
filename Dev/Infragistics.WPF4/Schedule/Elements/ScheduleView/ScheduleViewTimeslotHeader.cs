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
using Infragistics.Controls.Editors;

namespace Infragistics.Controls.Schedules.Primitives
{
	/// <summary>
	/// A custom <see cref="TimeslotHeader"/> that represents the label for a timeslot in a <see cref="XamScheduleView"/>
	/// </summary>
	public class ScheduleViewTimeslotHeader : TimeslotHeader
	{
		#region Constructor
		static ScheduleViewTimeslotHeader()
		{

			ScheduleViewTimeslotHeader.DefaultStyleKeyProperty.OverrideMetadata(typeof(ScheduleViewTimeslotHeader), new FrameworkPropertyMetadata(typeof(ScheduleViewTimeslotHeader)));

		}

		/// <summary>
		/// Initializes a new <see cref="ScheduleViewTimeslotHeader"/>
		/// </summary>
		public ScheduleViewTimeslotHeader()
		{



		}
		#endregion //Constructor

		#region Base class overrides
		
		#region Kind

		internal override TimeRangeKind Kind { get { return TimeRangeKind.TimeHeaderWithDayContext; } }

		#endregion //Kind	
		
		#region ForegroundBrushId

		internal override CalendarBrushId ForegroundBrushId { get { return CalendarBrushId.TimeslotHeaderForegroundScheduleView; } }

		#endregion //ForegroundBrushId	

		#region SetProviderBrushes

		internal override void SetProviderBrushes()
		{
			base.SetProviderBrushes();

			bool isFirstItem = TimeslotPanel.GetIsFirstItem(this);

			if (isFirstItem == false && this.IsFirstInDay)
				this.LeadingTickmarkKind = TimeslotTickmarkKind.Day;
			else
				if (this.IsFirstInMajor)
					this.LeadingTickmarkKind = TimeslotTickmarkKind.Major;
				else
					this.ClearValue(LeadingTickmarkKindPropertyKey);

			if (isFirstItem || this.IsFirstInDay )
			{
				this.LeadingTickmarkVisibility = Visibility.Visible;
			}
			else
			{
				this.ClearValue(LeadingTickmarkVisibilityPropertyKey);
			}

			if ( this.IsLastInDay )
			{
				this.TrailingTickmarkKind = TimeslotTickmarkKind.Day;
			}
			else
			if (this.IsLastInMajor)
			{
				this.TrailingTickmarkKind = TimeslotTickmarkKind.Major;
			}
			else
			{
				this.ClearValue(TrailingTickmarkKindPropertyKey);
			}
		}

		#endregion //SetProviderBrushes

		#region TickmarkBrushId

		internal override CalendarBrushId TickmarkBrushId { get { return CalendarBrushId.TimeslotHeaderTickmarkScheduleView; } }

		#endregion //TickmarkBrushId

		#endregion // Base class overrides
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