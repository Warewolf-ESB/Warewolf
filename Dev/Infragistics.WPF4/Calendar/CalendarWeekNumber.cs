using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.ComponentModel;
using Infragistics.Controls.Primitives;
using System.Windows.Media;

namespace Infragistics.Controls.Editors.Primitives
{
	/// <summary>
	/// Represents a specific week number header in the <see cref="CalendarItemGroup"/>
	/// </summary>
    //[System.ComponentModel.ToolboxItem(false)]
	[TemplateVisualState(Name = VisualStateUtilities.StateSelected,				GroupName = VisualStateUtilities.GroupSelection)]
	[TemplateVisualState(Name = VisualStateUtilities.StateUnselected,			GroupName = VisualStateUtilities.GroupSelection)]
	// JJD 4/11/12 - Added SelectedUnfocused state
	[TemplateVisualState(Name = VisualStateUtilities.StateSelectedUnfocused,	GroupName = VisualStateUtilities.GroupSelection)]

	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!

	public class CalendarWeekNumber : Control, ISelectableElement, ISelectableItem
	{
		private CalendarBase _calendar;
		private CalendarItemArea _itemArea;
		private int _week;
		private DateRange _dateRange;

		private bool _hasVisualStateGroups;

		private bool _isInitialized;

		static CalendarWeekNumber()
		{

			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(CalendarWeekNumber), new FrameworkPropertyMetadata(typeof(CalendarWeekNumber)));
			//UIElement.FocusableProperty.OverrideMetadata(typeof(CalendarWeekNumber), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));
			//Control.HorizontalContentAlignmentProperty.OverrideMetadata(typeof(CalendarWeekNumber), new FrameworkPropertyMetadata(KnownBoxes.HorizontalAlignmentCenterBox));
			//Control.VerticalContentAlignmentProperty.OverrideMetadata(typeof(CalendarWeekNumber), new FrameworkPropertyMetadata(KnownBoxes.VerticalAlignmentCenterBox));

        }

		/// <summary>
		/// Initializes a new <see cref="CalendarWeekNumber"/>
		/// </summary>
		/// <remarks>
		/// <p class="note"><b>Note:</b> This constructor is only used for styling purposes. At runtime, the controls are automatically generated.</p>
		/// </remarks>
		public CalendarWeekNumber()
		{



		}

		#region Base class overrides

		#region OnApplyTemplate

		/// <summary>
		/// Called when the template is applied.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// OnApplyTemplate is a .NET framework method exposed by the FrameworkElement. This class overrides
		/// it to get the focus site from the control template whenever template gets applied to the control.
		/// </p>
		/// </remarks>
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();


			this._hasVisualStateGroups = VisualStateUtilities.GetHasVisualStateGroups(this);


			this.UpdateVisualStates(false);

			this._isInitialized = true;
		}

		#endregion //OnApplyTemplate

		#endregion //Base class overrides	
    
		#region Properties

		#region Public Properties

		#region ComputedBackground

		private static readonly DependencyPropertyKey ComputedBackgroundPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("ComputedBackground",
			typeof(Brush), typeof(CalendarWeekNumber), new SolidColorBrush(Colors.Transparent), null);

		/// <summary>
		/// Identifies the read-only <see cref="ComputedBackground"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ComputedBackgroundProperty = ComputedBackgroundPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the brush to use for the background based on the element's state.
		/// </summary>
		/// <seealso cref="ComputedBackgroundProperty"/>
		public Brush ComputedBackground
		{
			get
			{
				return (Brush)this.GetValue(CalendarWeekNumber.ComputedBackgroundProperty);
			}
			internal set
			{
				this.SetValue(CalendarWeekNumber.ComputedBackgroundPropertyKey, value);
			}
		}

		#endregion //ComputedBackground

		#region IsSelected

		private static readonly DependencyPropertyKey IsSelectedPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("IsSelected",
			typeof(bool), typeof(CalendarWeekNumber),
			KnownBoxes.FalseBox,
			new PropertyChangedCallback(OnIsSelectedChanged)
			);

		/// <summary>
		/// Identifies the read-only <see cref="IsSelected"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsSelectedProperty = IsSelectedPropertyKey.DependencyProperty;

		private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			CalendarWeekNumber instance = (CalendarWeekNumber)d;

			instance.UpdateVisualStates();

			if ( true == (bool)e.NewValue )
				instance._calendar.SelectedDatesInternalChanged += new EventHandler<SelectedDatesChangedEventArgs>(instance.OnCalendar_SelectedDatesChanged);
			else
				instance._calendar.SelectedDatesInternalChanged -= new EventHandler<SelectedDatesChangedEventArgs>(instance.OnCalendar_SelectedDatesChanged);

		}

		/// <summary>
		/// Returns whether all the days in the week are selected (read-only)
		/// </summary>
		/// <seealso cref="IsSelectedProperty"/>
		public bool IsSelected
		{
			get
			{
				return (bool)this.GetValue(CalendarWeekNumber.IsSelectedProperty);
			}
			internal set
			{
				this.SetValue(CalendarWeekNumber.IsSelectedPropertyKey, value);
			}
		}

		#endregion //IsSelected

		#endregion //Public Properties	

		#region Internal Properties

		#region Range

		internal DateRange Range { get { return _dateRange; } }

		#endregion //Range	
     
		#region Week

		internal int Week
		{
			get
			{
				return _week;
			}
		}

		#endregion //Week

		#endregion //Internal Properties	
    
		#endregion //Properties	

		#region Methods

		#region Initialize

		internal void Initialize(CalendarBase cal, CalendarItemArea itemArea, int weekNumber, Visibility vis, DateTime startDay, DateTime endDay)
		{
			_calendar = cal;
			_itemArea = itemArea;
			_week = weekNumber;
			_dateRange = new DateRange(startDay, endDay);

			if (vis == System.Windows.Visibility.Visible)
				this.DataContext = weekNumber;
			else
				this.DataContext = string.Empty;

			this.VerifyIsSelected();

			// JJD 11/9/11 - TFS80707
			// Set the attached calendar property
			CalendarBase.SetCalendar(this, cal);
		}

		#endregion //Initialize

		#region OnCalendar_SelectedDatesChanged

		private void OnCalendar_SelectedDatesChanged(object sender, SelectedDatesChangedEventArgs e)
		{
			// JJD 11/15/11 - TFS79820
			// Call VerifyIsSelected synchronously so we can avoid any possible delay in
			// updating the visual states. 
			this.VerifyIsSelected();

			// JJD 11/15/11 - TFS79820
			// Also do a delayed verification in case the selected dates change as a result of the
			// of the SelectedDatesChanged event being raised, e.g. if XamOutlookCalendarView is
			// in week selection mode and it changes the seelcted dates based on that.
			this.Dispatcher.BeginInvoke(new CalendarUtilities.MethodInvoker(this.VerifyIsSelected));
		}

		#endregion //OnCalendar_SelectedDatesChanged	
    
		#region SetProviderBrushes

		internal void SetProviderBrushes()
		{
			if (this._calendar == null)
				return;

			CalendarResourceProvider rp = _calendar.ResourceProviderResolved;

			if (rp == null)
				return;

			bool isSelected = this.IsSelected;
			bool isSelectionActive = _calendar.IsSelectionActive;

			CalendarResourceId idBackground;

			if (isSelected == true)
			{
				if (isSelectionActive == true)
				{
					idBackground = CalendarResourceId.SelectedFocusedItemBackgroundBrush;
				}
				else
				{
					idBackground = CalendarResourceId.SelectedItemBackgroundBrush;
				}

				this.ComputedBackground = rp[idBackground] as Brush;
			}
			else
				this.ClearValue(ComputedBackgroundPropertyKey);

		}

		#endregion //SetProviderBrushes

		#region VerifyIsSelected

		internal void VerifyIsSelected()
		{
			SelectedDateCollection selectedDates = _calendar.SelectedDatesInternal as SelectedDateCollection;

			if (_calendar.SupportsWeekSelectionMode && selectedDates.IsSelected(_dateRange))
				this.SetValue(IsSelectedPropertyKey, KnownBoxes.TrueBox);
			else
				this.ClearValue(IsSelectedPropertyKey);
		}

		#endregion //VerifyIsSelected	
    
		#region VisualState... Methods

		/// <summary>
		/// Called to set the VisualStates of the editor
		/// </summary>
		/// <param name="useTransitions">Determines whether transitions should be used.</param>
		protected virtual void SetVisualState(bool useTransitions)
		{
			if (_calendar == null || _calendar.SupportsWeekSelectionMode == false)
				return;

			// Set Selection states
			// JJD 4/11/12 - Added SelectedUnfocused state
			//if (_calendar.IsSelectionActive && this.IsSelected)
			if (this.IsSelected)
			{
				if (_calendar.IsSelectionActive)
					VisualStateManager.GoToState(this, VisualStateUtilities.StateSelected, useTransitions);
				else
					VisualStateManager.GoToState(this, VisualStateUtilities.StateSelectedUnfocused, useTransitions);
			}
			else
				VisualStateManager.GoToState(this, VisualStateUtilities.StateUnselected, useTransitions);

			this.SetProviderBrushes();
		}

		// JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
		/// <summary>
		/// Called to set the visual states of the control
		/// </summary>
		protected void UpdateVisualStates()
		{
			this.UpdateVisualStates(true);
		}

		/// <summary>
		/// Called to set the visual states of the control
		/// </summary>
		/// <param name="useTransitions">Determines whether transitions should be used.</param>
		protected void UpdateVisualStates(bool useTransitions)
		{

			if (false == this._hasVisualStateGroups)
			{
				this.SetProviderBrushes();
				return;
			}

			if (!this._isInitialized)
				useTransitions = false;

			this.SetVisualState(useTransitions);
		}

		#endregion //VisualState... Methods	

		#endregion //Methods	
            
		#region ISelectableElement Members

		ISelectableItem ISelectableElement.SelectableItem
		{
			get { return this; }
		}

		#endregion

		#region ISelectableItem Members

		bool ISelectableItem.IsSelectable
		{
			get { return _calendar.SupportsWeekSelectionMode; }
		}

		bool ISelectableItem.IsDraggable
		{
			get { return false; }
		}

		bool ISelectableItem.IsTabStop
		{
			get { return false; }
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