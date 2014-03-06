using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Infragistics.Controls.Schedules.Primitives
{
	/// <summary>
	/// Represents a single logical day in a <see cref="XamMonthView"/>
	/// </summary>
	public class MonthViewDay : DayPresenterBase
	{
		#region Constructor
		static MonthViewDay()
		{

			MonthViewDay.DefaultStyleKeyProperty.OverrideMetadata(typeof(MonthViewDay), new FrameworkPropertyMetadata(typeof(MonthViewDay)));

		}

		/// <summary>
		/// Initializes a new <see cref="MonthViewDay"/>
		/// </summary>
		public MonthViewDay()
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
					return CalendarBrushId.SelectedDayBackgroundMonthView;

				if (_isAlternate)
					return CalendarBrushId.AlternateMonthDayBackground;

				return CalendarBrushId.DayBackground;
			}
		} 
		#endregion // BackgroundBrushId

		#region BorderBrushId
		internal override CalendarBrushId BorderBrushId
		{
			get
			{
				if (this.IsToday)
					return CalendarBrushId.CurrentDayBorderMonthCalendar;

				return CalendarBrushId.DayBorder;
			}
		} 
		#endregion // BorderBrushId

		#endregion // Base class overrides

		#region Properties

		#region Public Properties

		#region IsAlternate

		private bool _isAlternate;

		private static readonly DependencyPropertyKey IsAlternatePropertyKey = DependencyPropertyUtilities.RegisterReadOnly("IsAlternate",
			typeof(bool), typeof(MonthViewDay),
			KnownBoxes.FalseBox,
			new PropertyChangedCallback(OnIsAlternateChanged)
			);

		/// <summary>
		/// Identifies the read-only <see cref="IsAlternate"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsAlternateProperty = IsAlternatePropertyKey.DependencyProperty;

		private static void OnIsAlternateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			MonthViewDay instance = (MonthViewDay)d;

			bool IsAlternate = true.Equals(e.NewValue);

			instance._isAlternate = IsAlternate;
			instance.UpdateVisualState();
		}

		/// <summary>
		/// Returns a boolean indicating if the elements represents a day in an alternate month
		/// </summary>
		/// <seealso cref="IsAlternateProperty"/>
		public bool IsAlternate
		{
			get
			{
				return _isAlternate;
			}
			internal set
			{
				this.SetValue(MonthViewDay.IsAlternatePropertyKey, KnownBoxes.FromValue(value));
			}
		}

		#endregion //IsAlternate

		#endregion // Public Properties

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