using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Infragistics.Controls.Editors;

namespace Infragistics.Controls.Schedules.Primitives
{
	/// <summary>
	/// Represents a single date within the <see cref="XamDayView"/> area where the activities which are 24 hours or longer are displayed.
	/// </summary>
	public class MultiDayActivityArea : DayPresenterBase
	{
		#region Constructor
		static MultiDayActivityArea()
		{

			MultiDayActivityArea.DefaultStyleKeyProperty.OverrideMetadata(typeof(MultiDayActivityArea), new FrameworkPropertyMetadata(typeof(MultiDayActivityArea)));

		}

		/// <summary>
		/// Initializes a new <see cref="MultiDayActivityArea"/>
		/// </summary>
		public MultiDayActivityArea()
		{



		}
		#endregion //Constructor

		#region Base class overrides

		#region BackgroundBrushId

		internal override CalendarBrushId BackgroundBrushId
		{
			get
			{
				if (this.IsSelected)
					return CalendarBrushId.SelectedMultiDayActivityAreaBackground;

				return CalendarBrushId.MultiDayActivityAreaBackground;
			}
		}

		#endregion //BackgroundBrushId	

		#region BorderBrushId
		internal override CalendarBrushId BorderBrushId
		{
			get
			{
				if (this.IsToday)
					return CalendarBrushId.CurrentDayBorder;

				return CalendarBrushId.DayBorder;
			}
		} 
		#endregion // BorderBrushId 
    
		#region Kind

		internal override TimeRangeKind Kind { get { return TimeRangeKind.DayViewMultiDayArea; } }

		#endregion //Kind	

		#endregion // Base class overrides

		#region Properties

		#region ComputedBorderThickness

		private static readonly DependencyPropertyKey ComputedBorderThicknessPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("ComputedBorderThickness",
			typeof(Thickness), typeof(MultiDayActivityArea), new Thickness(1, 0, 1, 0), null);

		/// <summary>
		/// Identifies the read-only <see cref="ComputedBorderThickness"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ComputedBorderThicknessProperty = ComputedBorderThicknessPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the border thickness to use for the BorderBrush based on the element's state and the associated <see cref="XamScheduleDataManager"/>'s <see cref="XamScheduleDataManager.ColorScheme"/>
		/// </summary>
		/// <seealso cref="ComputedBorderThicknessProperty"/>
		public Thickness ComputedBorderThickness
		{
			get
			{
				return (Thickness)this.GetValue(MultiDayActivityArea.ComputedBorderThicknessProperty);
			}
			internal set
			{
				this.SetValue(MultiDayActivityArea.ComputedBorderThicknessPropertyKey, value);
			}
		}

		#endregion //ComputedBorderThickness

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