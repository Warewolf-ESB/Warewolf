using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;

namespace Infragistics.Controls.Schedules.Primitives
{
	/// <summary>
	/// Custom element displayed over a timeslot and is used to create a new activity.
	/// </summary>
	public class ClickToAddActivityElement : ResourceCalendarElementBase
	{
		#region Member Variables

		private bool _isAllDayActivity;
		private ActivityTypes _activityTypes;

		#endregion // Member Variables

		#region Constructor
		static ClickToAddActivityElement()
		{

			ClickToAddActivityElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(ClickToAddActivityElement), new FrameworkPropertyMetadata(typeof(ClickToAddActivityElement)));
			UIElement.FocusableProperty.OverrideMetadata(typeof(ClickToAddActivityElement), new FrameworkPropertyMetadata(KnownBoxes.FalseBox)); // AS 12/16/10 TFS61923

		}

		/// <summary>
		/// Initializes a new <see cref="ClickToAddActivityElement"/>
		/// </summary>
		public ClickToAddActivityElement()
		{



		}
		#endregion //Constructor

		#region Properties

		#region ActivityTypes

		private static readonly DependencyPropertyKey ActivityTypesPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("ActivityTypes",
			typeof(ActivityTypes), typeof(ClickToAddActivityElement),
			ActivityTypes.None,
			new PropertyChangedCallback(OnActivityTypesChanged)
			);

		/// <summary>
		/// Identifies the read-only <see cref="ActivityTypes"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ActivityTypesProperty = ActivityTypesPropertyKey.DependencyProperty;

		private static void OnActivityTypesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ClickToAddActivityElement instance = (ClickToAddActivityElement)d;
			instance._activityTypes = (ActivityTypes)e.NewValue;
			instance.SetProviderBrushes();
		}

		/// <summary>
		/// Returns the type of activity that may be created when the Click To Add element is clicked.
		/// </summary>
		/// <seealso cref="ActivityTypesProperty"/>
		public ActivityTypes ActivityTypes
		{
			get
			{
				return _activityTypes;
			}
			internal set
			{
				this.SetValue(ClickToAddActivityElement.ActivityTypesPropertyKey, value);
			}
		}

		#endregion //ActivityTypes

		#region ComputedForeground

		private static readonly DependencyPropertyKey ComputedForegroundPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("ComputedForeground",
			typeof(Brush), typeof(ClickToAddActivityElement), null, null);

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
				return (Brush)this.GetValue(ClickToAddActivityElement.ComputedForegroundProperty);
			}
			internal set
			{
				this.SetValue(ClickToAddActivityElement.ComputedForegroundPropertyKey, value);
			}
		}

		#endregion //ComputedForeground

		#region IsAllDayActivity
		internal bool IsAllDayActivity
		{
			get { return _isAllDayActivity; }
			set { _isAllDayActivity = value; }
		} 
		#endregion // IsAllDayActivity

		#region IsSingleLineDisplay

		private static readonly DependencyPropertyKey IsSingleLineDisplayPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("IsSingleLineDisplay",
			typeof(bool), typeof(ClickToAddActivityElement), KnownBoxes.TrueBox, OnIsSingleLineDisplayChanged);

		/// <summary>
		/// Identifies the read-only <see cref="IsSingleLineDisplay"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsSingleLineDisplayProperty = IsSingleLineDisplayPropertyKey.DependencyProperty;

		private static void OnIsSingleLineDisplayChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ClickToAddActivityElement instance = (ClickToAddActivityElement)d;

			instance.SetProviderBrushes();
		}

		/// <summary>
		/// Returns true if this element should display its contents in a single line (read-only)
		/// </summary>
		/// <seealso cref="IsSingleLineDisplayProperty"/>
		public bool IsSingleLineDisplay
		{
			get
			{
				return (bool)this.GetValue(ClickToAddActivityElement.IsSingleLineDisplayProperty);
			}
			internal set
			{
				this.SetValue(ClickToAddActivityElement.IsSingleLineDisplayPropertyKey, value);
			}
		}

		#endregion //IsSingleLineDisplay

		#region Prompt

		private static readonly DependencyPropertyKey PromptPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("Prompt",
			typeof(string), typeof(ClickToAddActivityElement), null, null);

		/// <summary>
		/// Identifies the read-only <see cref="Prompt"/> dependency property
		/// </summary>
		public static readonly DependencyProperty PromptProperty = PromptPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the string to be displayed in the element.
		/// </summary>
		/// <seealso cref="PromptProperty"/>
		public string Prompt
		{
			get
			{
				return (string)this.GetValue(ClickToAddActivityElement.PromptProperty);
			}
			internal set
			{
				this.SetValue(ClickToAddActivityElement.PromptPropertyKey, value);
			}
		}

		#endregion //Prompt

		#endregion // Properties

		#region Methods

		#region SetProviderBrushes

		internal override void SetProviderBrushes()
		{
			ResourceCalendar calendar = this.DataContext as ResourceCalendar;

			if (calendar == null)
				return;

			CalendarBrushProvider brushProvider = calendar != null ? calendar.BrushProvider : null;

			if (brushProvider == null)
				return;

			this.SetValue(CalendarBrushProvider.BrushProviderProperty, brushProvider);

			// AS 3/1/11 NA 2011.1 ActivityTypeChooser
			//this.ComputedBackground = brushProvider.GetBrush(CalendarBrushId.AppointmentBackground);
			//this.ComputedBorderBrush = brushProvider.GetBrush(CalendarBrushId.AppointmentBorder);
			//this.ComputedForeground = brushProvider.GetBrush(CalendarBrushId.AppointmentForegroundOverlayed);
			CalendarBrushId backgroundId, borderId, foregroundId;

			if (this.ActivityTypes == Schedules.ActivityTypes.Journal)
			{
				backgroundId = CalendarBrushId.JournalBackground;
				borderId = CalendarBrushId.JournalBorder;
				foregroundId = CalendarBrushId.JournalForegroundOverlayed;
			}
			else if (this.ActivityTypes == Schedules.ActivityTypes.Task)
			{
				backgroundId = CalendarBrushId.TaskBackground;
				borderId = CalendarBrushId.TaskBorder;
				foregroundId = CalendarBrushId.TaskForegroundOverlayed;
			}
			else
			{
				backgroundId = CalendarBrushId.AppointmentBackground;
				borderId = CalendarBrushId.AppointmentBorder;
				foregroundId = CalendarBrushId.AppointmentForegroundOverlayed;
			}

			this.ComputedBackground = brushProvider.GetBrush(backgroundId);
			this.ComputedBorderBrush = brushProvider.GetBrush(borderId);
			this.ComputedForeground = brushProvider.GetBrush(foregroundId);
		}

		#endregion //SetProviderBrushes

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