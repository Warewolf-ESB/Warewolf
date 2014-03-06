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
using System.ComponentModel;
using Infragistics.Controls.Schedules;
using Infragistics.AutomationPeers;

namespace Infragistics.Controls.Schedules.Primitives
{
	/// <summary>
	/// Represents the header for a given day of the week within a specific <see cref="CalendarGroupBase"/> of a <see cref="XamMonthView"/>
	/// </summary>
	public class MonthViewDayOfWeekHeader : ResourceCalendarElementBase
	{
		#region Member Variables

		private bool _isVerified; 

		#endregion // Member Variables

		#region Constructor
		static MonthViewDayOfWeekHeader()
		{

			MonthViewDayOfWeekHeader.DefaultStyleKeyProperty.OverrideMetadata(typeof(MonthViewDayOfWeekHeader), new FrameworkPropertyMetadata(typeof(MonthViewDayOfWeekHeader)));

		}

		/// <summary>
		/// Initializes a new <see cref="MonthViewDayOfWeekHeader"/>
		/// </summary>
		public MonthViewDayOfWeekHeader()
		{



		}
		#endregion //Constructor

		#region Base class overrides

		#region MeasureOverride

		/// <summary>
		/// Invoked to measure the element and its children.
		/// </summary>
		/// <param name="availableSize">The size that reflects the available size that this element can give to its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> that represents the desired size of the element.</returns>
		protected override Size MeasureOverride(Size availableSize)
		{
			this.VerifyDisplayNames();

			return base.MeasureOverride(availableSize);
		}
		#endregion //Base class overrides

		#region OnCreateAutomationPeer
		/// <summary>
		/// Returns an automation peer that exposes the <see cref="MonthViewDayOfWeekHeader"/> to UI Automation.
		/// </summary>
		/// <returns>A <see cref="MonthViewDayOfWeekHeaderAutomationPeer"/></returns>
		protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer()
		{
			return new MonthViewDayOfWeekHeaderAutomationPeer(this);
		}
		#endregion // OnCreateAutomationPeer

		#region SetProviderBrushes
		internal override void SetProviderBrushes()
		{
			if (!this.IsBrushVersionBindingInitialized)
				return;

			ResourceCalendar rc = this.DataContext as ResourceCalendar;

			if (rc == null )
				return;

			CalendarBrushProvider brushProvider = rc.BrushProvider;

			if (brushProvider == null)
				return;

			bool isLastItem = ScheduleItemsPanel.GetIsLastItem(this);

			if (isLastItem)
				this.ComputedBorderThickness = new Thickness();
			else
				this.ClearValue(ComputedBorderThicknessPropertyKey);

			#region Set background

			Brush br = brushProvider.GetBrush(CalendarBrushId.MonthViewDayOfWeekHeaderBackground);

			if (br != null)
				this.ComputedBackground = br;

			#endregion //Set background

			#region Set border brush

			br = brushProvider.GetBrush(CalendarBrushId.MonthViewDayOfWeekHeaderBorder);

			if (br != null)
				this.ComputedBorderBrush = br;

			#endregion //Set border brush

			#region Set Foreground

			br = brushProvider.GetBrush(CalendarBrushId.MonthViewDayOfWeekHeaderForeground);

			if (br != null)
				this.ComputedForeground = br;

			#endregion //Set Foreground

		} 
		#endregion // SetProviderBrushes

		#endregion //MeasureOverride

		#region Properties

		#region AbbreviatedDayName

		private static readonly DependencyPropertyKey AbbreviatedDayNamePropertyKey = DependencyPropertyUtilities.RegisterReadOnly("AbbreviatedDayName",
			typeof(string), typeof(MonthViewDayOfWeekHeader), "Sun", null);

		/// <summary>
		/// Identifies the read-only <see cref="AbbreviatedDayName"/> dependency property
		/// </summary>
		public static readonly DependencyProperty AbbreviatedDayNameProperty = AbbreviatedDayNamePropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the abbreviated version of the localized string representation of the <see cref="DayOfWeek"/>
		/// </summary>
		/// <seealso cref="AbbreviatedDayNameProperty"/>
		public string AbbreviatedDayName
		{
			get
			{
				return (string)this.GetValue(MonthViewDayOfWeekHeader.AbbreviatedDayNameProperty);
			}
			internal set
			{
				this.SetValue(MonthViewDayOfWeekHeader.AbbreviatedDayNamePropertyKey, value);
			}
		}

		#endregion //AbbreviatedDayName

		#region ComputedBorderThickness

		private static readonly DependencyPropertyKey ComputedBorderThicknessPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("ComputedBorderThickness",
			typeof(Thickness), typeof(MonthViewDayOfWeekHeader), new Thickness(0,0,1,0), null);

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
				return (Thickness)this.GetValue(MonthViewDayOfWeekHeader.ComputedBorderThicknessProperty);
			}
			internal set
			{
				this.SetValue(MonthViewDayOfWeekHeader.ComputedBorderThicknessPropertyKey, value);
			}
		}

		#endregion //ComputedBorderThickness

		#region ComputedForeground

		private static readonly DependencyPropertyKey ComputedForegroundPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("ComputedForeground",
			typeof(Brush), typeof(MonthViewDayOfWeekHeader), null, null);

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
				return (Brush)this.GetValue(MonthViewDayOfWeekHeader.ComputedForegroundProperty);
			}
			internal set
			{
				this.SetValue(MonthViewDayOfWeekHeader.ComputedForegroundPropertyKey, value);
			}
		}

		#endregion //ComputedForeground

		#region DayOfWeek

		private static readonly DependencyPropertyKey DayOfWeekPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("DayOfWeek",
			typeof(DayOfWeek), typeof(MonthViewDayOfWeekHeader),
			DayOfWeek.Sunday,
			new PropertyChangedCallback(OnDayOfWeekChanged)
			);

		/// <summary>
		/// Identifies the read-only <see cref="DayOfWeek"/> dependency property
		/// </summary>
		public static readonly DependencyProperty DayOfWeekProperty = DayOfWeekPropertyKey.DependencyProperty;

		private static void OnDayOfWeekChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			MonthViewDayOfWeekHeader instance = (MonthViewDayOfWeekHeader)d;
			instance._isVerified = false;
			instance.InvalidateMeasure();
		}

		/// <summary>
		/// Returns an enumeration value indicating which day of the week the element represents.
		/// </summary>
		/// <seealso cref="DayOfWeekProperty"/>
		public DayOfWeek DayOfWeek
		{
			get
			{
				return (DayOfWeek)this.GetValue(MonthViewDayOfWeekHeader.DayOfWeekProperty);
			}
			internal set
			{
				this.SetValue(MonthViewDayOfWeekHeader.DayOfWeekPropertyKey, value);
			}
		}

		#endregion //DayOfWeek

		#region DayName

		private static readonly DependencyPropertyKey DayNamePropertyKey = DependencyPropertyUtilities.RegisterReadOnly("DayName",
			typeof(string), typeof(MonthViewDayOfWeekHeader), null, null);

		/// <summary>
		/// Identifies the read-only <see cref="DayName"/> dependency property
		/// </summary>
		public static readonly DependencyProperty DayNameProperty = DayNamePropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the localized string representation of the <see cref="DayOfWeek"/>
		/// </summary>
		/// <seealso cref="DayNameProperty"/>
		public string DayName
		{
			get
			{
				return (string)this.GetValue(MonthViewDayOfWeekHeader.DayNameProperty);
			}
			internal set
			{
				this.SetValue(MonthViewDayOfWeekHeader.DayNamePropertyKey, value);
			}
		}

		#endregion //DayName

		#region ShortestDayName

		private static readonly DependencyPropertyKey ShortestDayNamePropertyKey = DependencyPropertyUtilities.RegisterReadOnly("ShortestDayName",
			typeof(string), typeof(MonthViewDayOfWeekHeader), "Sun", null);

		/// <summary>
		/// Identifies the read-only <see cref="ShortestDayName"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ShortestDayNameProperty = ShortestDayNamePropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the shortest version of the localized string representation of the <see cref="DayOfWeek"/>
		/// </summary>
		/// <seealso cref="ShortestDayNameProperty"/>
		public string ShortestDayName
		{
			get
			{
				return (string)this.GetValue(MonthViewDayOfWeekHeader.ShortestDayNameProperty);
			}
			internal set
			{
				this.SetValue(MonthViewDayOfWeekHeader.ShortestDayNamePropertyKey, value);
			}
		}

		#endregion //ShortestDayName

		#endregion // Properties

		#region Methods

		#region VerifyDisplayNames
		private void VerifyDisplayNames()
		{
			if (_isVerified)
				return;

			_isVerified = true;

			var ctrl = ScheduleUtilities.GetControl(this);
			var dateProvider = ScheduleUtilities.GetDateInfoProvider(ctrl);
			var dow = this.DayOfWeek;

			this.DayName = dateProvider.FormatDayOfWeek(DateTimeFormatType.DayOfWeek, dow);
			this.AbbreviatedDayName = dateProvider.FormatDayOfWeek(DateTimeFormatType.ShortDayOfWeek, dow);
			this.ShortestDayName = dateProvider.FormatDayOfWeek(DateTimeFormatType.ShortestDayOfWeek, dow);
		}
		#endregion // VerifyDisplayNames

		#endregion // Methods
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