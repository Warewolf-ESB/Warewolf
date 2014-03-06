using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.ComponentModel;
using System.Windows.Media;
using Infragistics.Collections;

namespace Infragistics.Controls.Editors.Primitives
{
	/// <summary>
	/// Represents a specific <see cref="DayOfWeek"/> in the header area of the <see cref="CalendarItemGroup"/>
	/// </summary>
    //[System.ComponentModel.ToolboxItem(false)]

	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!

	public class CalendarDayOfWeek : Control
	{
		#region Constructor

		static CalendarDayOfWeek()
		{

			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(CalendarDayOfWeek), new FrameworkPropertyMetadata(typeof(CalendarDayOfWeek)));
			//UIElement.FocusableProperty.OverrideMetadata(typeof(CalendarDayOfWeek), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));
			//Control.HorizontalContentAlignmentProperty.OverrideMetadata(typeof(CalendarDayOfWeek), new FrameworkPropertyMetadata(KnownBoxes.HorizontalAlignmentCenterBox));
			//Control.VerticalContentAlignmentProperty.OverrideMetadata(typeof(CalendarDayOfWeek), new FrameworkPropertyMetadata(KnownBoxes.VerticalAlignmentCenterBox));

		}

		internal CalendarDayOfWeek()
		{




		}

		#endregion //Constructor	

		#region Base class overrides

		#region OnMouseLeftButtonDown

		/// <summary>
		/// Called when the left mouse button is pressed
		/// </summary>
		/// <param name="e">The event arguments</param>
		protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
		{
			base.OnMouseLeftButtonDown(e);

			DependencyObject parent = VisualTreeHelper.GetParent(this);

			CalendarBase cal = parent != null ? CalendarBase.GetCalendar(parent) : null;

			if (cal != null && cal.SupportsWeekSelectionMode)
			{
				CalendarItemGroup group = PresentationUtilities.GetVisualAncestor<CalendarItemGroup>(this, null);

				if (group != null && group.FirstDateOfGroup.HasValue && group.LastDateOfGroup.HasValue)
				{
					int offset;

					// JJD 3/8/11 - TFS66513
					// Get the calendar manager once
					CalendarManager cm = cal.CalendarManager;

					// select all the weeks of the month
					DateTime dtFirst = cm.GetFirstDayOfWeekForDate(group.FirstDateOfGroup.Value, null, out offset);
					//DateTime dtLast = cm.GetFirstDayOfWeekForDate(group.LastDateOfGroup.Value, null, out offset).AddDays(6);
					DateTime dtLast = cm.AddDays(cm.GetFirstDayOfWeekForDate(group.LastDateOfGroup.Value, null, out offset), 6);

					TimeSpan ts = dtLast.Subtract(dtFirst);

					List<DateTime> newSelection = new List<DateTime>(ts.Days + 1);

					DateTime dt = dtFirst;

					while (dtLast >= dt)
					{
						newSelection.Add(dt);
						// JJD 3/8/11 - TFS66513
						// Use the calendar manager for adding days because it deals gracefully with
						// min and ma dates without blowing up
						//dt = dt.AddDays(1);
						dt = cm.AddDays(dt, 1);
					}

					DateCollection selectedDates = cal.SelectedDates;

					if (selectedDates.Count != newSelection.Count ||
						 selectedDates[0] != newSelection[0] ||
						 selectedDates[selectedDates.Count - 1] != newSelection[newSelection.Count - 1])
						selectedDates.Reinitialize(newSelection);
				}
			}
		}

		#endregion //OnMouseLeftButtonDown

		#endregion //Base class overrides	
    
		#region Properties

		#region Public Properties

		#region DayOfWeek

		private static readonly DependencyPropertyKey DayOfWeekPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("DayOfWeek",
			typeof(DayOfWeek), typeof(CalendarDayOfWeek), DayOfWeek.Sunday, null);

		/// <summary>
		/// Identifies the read-only <see cref="DayOfWeek"/> dependency property
		/// </summary>
		public static readonly DependencyProperty DayOfWeekProperty = DayOfWeekPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns an enumeration for this day of week (read-only).
		/// </summary>
		/// <seealso cref="DayOfWeekProperty"/>
		[Bindable(true)]
		[ReadOnly(true)]
		public DayOfWeek DayOfWeek
		{
			get
			{
				return (DayOfWeek)this.GetValue(CalendarDayOfWeek.DayOfWeekProperty);
			}
			internal set
			{
				this.SetValue(CalendarDayOfWeek.DayOfWeekPropertyKey, value);
			}
		}

		#endregion //DayOfWeek

		#region DisplayText

		private static readonly DependencyPropertyKey DisplayTextPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("DisplayText",
			typeof(string), typeof(CalendarDayOfWeek), string.Empty, OnDisplayTextChanged);

		private static void OnDisplayTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			CalendarDayOfWeek instance = d as CalendarDayOfWeek;

			instance.InvalidateMeasure();
		}
		/// <summary>
		/// Identifies the read-only <see cref="DisplayText"/> dependency property
		/// </summary>
		public static readonly DependencyProperty DisplayTextProperty = DisplayTextPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the string to display for this day of week (read-only)
		/// </summary>
		/// <seealso cref="DisplayTextProperty"/>
		[Bindable(true)]
		[ReadOnly(true)]
		public string DisplayText
		{
			get
			{
				return (string)this.GetValue(CalendarDayOfWeek.DisplayTextProperty);
			}
			internal set
			{
				this.SetValue(CalendarDayOfWeek.DisplayTextPropertyKey, value);
			}
		}

		#endregion //DisplayText

		#endregion //Public Properties	
    
		#endregion //Properties	
    
		#region Methods

		#region Internal

		#region Initialize

		internal void Initialize(DayOfWeek dow, CalendarBase cal)
		{
			this.DayOfWeek = dow;
			this.DisplayText = cal.GetDayOfWeekHeader(this.DayOfWeek);

			// JJD 11/9/11 - TFS80707
			// Set the attached calendar property
			CalendarBase.SetCalendar(this, cal);
		}

		#endregion //Initialize

		#endregion //Internal	
    
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