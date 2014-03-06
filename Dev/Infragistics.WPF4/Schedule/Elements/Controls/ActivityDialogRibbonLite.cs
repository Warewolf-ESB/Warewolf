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
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Controls.Primitives;
using Infragistics.Controls.Menus;
using System.Windows.Data;

namespace Infragistics.Controls.Schedules.Primitives
{
	/// <summary>
	/// Ribbon-like control used in the 'lightweight' Activity dialogs.  For internal use only.
	/// </summary>
	[TemplatePart(Name = PartOptionsGroup, Type = typeof(Grid))]
	[TemplatePart(Name = PartRecurrenceButton, Type = typeof(Button))]
	[TemplatePart(Name = PartTimeZonesButton, Type = typeof(ToggleButton))]
	[TemplatePart(Name = PartReminderPickerLabel, Type = typeof(TextBlock))]
	[TemplatePart(Name = PartReminderPickerImage, Type = typeof(Image))]
	[TemplatePart(Name = PartCategorizeButton, Type = typeof(ToggleButton))]
	[TemplatePart(Name = PartTagsGroup, Type = typeof(Grid))]



	[TemplatePart(Name = PartReminderPicker, Type = typeof(ComboBox))]

	[DesignTimeVisible(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class ActivityDialogRibbonLite : Control
	{
		#region Member Variables

		// Template part names
		private const string PartOptionsGroup			= "OptionsGroup";
		private const string PartReminderPicker			= "ReminderPicker";
		private const string PartRecurrenceButton		= "RecurrenceButton";
		private const string PartTimeZonesButton		= "TimeZonesButton";
		private const string PartReminderPickerLabel	= "ReminderPickerLabel";
		private const string PartReminderPickerImage	= "ReminderPickerImage";
		private const string PartCategorizeButton		= "CategorizeButton";
		private const string PartTagsGroup				= "TagsGroup";

		private CalendarColorScheme				_colorScheme;
		private bool							_initialized;
		private ComboBoxProxy					_reminderPickerProxy;
		private ActivityDialogCore				_adc;
		private Dictionary<string, string>		_localizedStrings;
		private Button							_recurrenceButton;
		private ToggleButton					_timeZonesButton;
		private TextBlock						_reminderPickerLabel;
		private Image							_reminderPickerImage;
		private bool							_isRecurrenceToolRequired;
		private bool							_isReminderToolRequired;
		private bool							_isTimeZoneToolRequired;
		private ToggleButton					_categorizeButton;
		private XamContextMenu					_categorizeContextMenu;

		#endregion //Member Variables

		#region Constructors
		static ActivityDialogRibbonLite()
		{

			ActivityDialogRibbonLite.DefaultStyleKeyProperty.OverrideMetadata(typeof(ActivityDialogRibbonLite), new FrameworkPropertyMetadata(typeof(ActivityDialogRibbonLite)));

		}

		/// <summary>
		/// Creates an instance of the ActivityDialogRibbonLite.  For internal use only.
		/// </summary>
		/// <param name="colorScheme"></param>
		/// <param name="isRecurrenceToolRequired"></param>
		/// <param name="isReminderToolRequired"></param>
		/// <param name="isTimeZoneToolRequired"></param>
		public ActivityDialogRibbonLite(CalendarColorScheme colorScheme, bool isRecurrenceToolRequired, bool isReminderToolRequired, bool isTimeZoneToolRequired)
		{



			CoreUtilities.ValidateNotNull(colorScheme, "colorScheme");

			this._colorScheme				= colorScheme;
			this._isRecurrenceToolRequired	= isRecurrenceToolRequired;
			this._isReminderToolRequired	= isReminderToolRequired;
			this._isTimeZoneToolRequired	= isTimeZoneToolRequired;
		}
		#endregion //Constructors

		#region Base Class Overrides

		#region OnApplyTemplate
		/// <summary>
		/// Invoked when the template for the control has been applied.
		/// </summary>
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			// Initialize.
			this.Dispatcher.BeginInvoke(new ScheduleUtilities.MethodInvoker(this.Initialize));
		}
		#endregion //OnApplyTemplate

		#endregion //Base Class Overrides

		#region Properties

		#region Public Properties

		#region ColorScheme
		/// <summary>
		/// Returns the <see cref="CalendarColorScheme"/> associated with the control.
		/// </summary>
		public CalendarColorScheme ColorScheme
		{
			get { return this._colorScheme; }
		}
		#endregion //ColorScheme

		#region LocalizedStrings
		/// <summary>
		/// Returns a dictionary of localized strings for use by the controls in the template.
		/// </summary>
		public Dictionary<string, string> LocalizedStrings
		{
			get
			{
				if (this._localizedStrings == null)
				{
					this._localizedStrings = new Dictionary<string, string>(10);

					this._localizedStrings.Add("DLG_Appointment_Core_RibbonGroup_Actions", ScheduleUtilities.GetString("DLG_Appointment_Core_RibbonGroup_Actions"));
					this._localizedStrings.Add("DLG_Appointment_Core_ButtonTool_SaveClose", ScheduleUtilities.GetString("DLG_Appointment_Core_ButtonTool_SaveClose"));
					this._localizedStrings.Add("DLG_Appointment_Core_ButtonTool_Delete", ScheduleUtilities.GetString("DLG_Appointment_Core_ButtonTool_Delete"));
					this._localizedStrings.Add("DLG_Appointment_Core_RibbonGroup_Options", ScheduleUtilities.GetString("DLG_Appointment_Core_RibbonGroup_Options"));
					this._localizedStrings.Add("DLG_Appointment_Core_ComboTool_Reminder", ScheduleUtilities.GetString("DLG_Appointment_Core_ComboTool_Reminder"));
					this._localizedStrings.Add("DLG_Appointment_Core_ButtonTool_TimeZones", ScheduleUtilities.GetString("DLG_Appointment_Core_ButtonTool_TimeZones"));
					this._localizedStrings.Add("DLG_Appointment_Core_ButtonTool_Recurrence", ScheduleUtilities.GetString("DLG_Appointment_Core_ButtonTool_Recurrence"));
					this._localizedStrings.Add("DLG_Appointment_Core_RibbonGroup_Tags", ScheduleUtilities.GetString("DLG_Appointment_Core_RibbonGroup_Tags"));
					this._localizedStrings.Add("DLG_Appointment_Core_ButtonTool_Categorize", ScheduleUtilities.GetString("DLG_Appointment_Core_ButtonTool_Categorize"));
				}

				return this._localizedStrings;
			}
		}
		#endregion //LocalizedStrings

		#endregion //Public Properties

		#endregion //Properties

		#region Methods

		#region Initialize
		private void Initialize()
		{
			if (this._initialized == false)
			{
				// Find the parts we need.
				//
				// Reminder combo
				FrameworkElement fe = this.GetTemplateChild(PartReminderPicker) as FrameworkElement;
				if (fe != null)
				{
					this._reminderPickerProxy					= new ComboBoxProxy(fe);
					this._reminderPickerProxy.ItemsSource		= AppointmentDialogUtilities.GetReminderListItems();
					this._reminderPickerProxy.LostFocus			+= new ComboBoxProxy.ProxyLostFocusEventHandler(reminderPicker_LostFocus);
					this._reminderPickerProxy.SelectionChanged	+= new ComboBoxProxy.ProxySelectionChangedEventHandler(reminderPicker_SelectionChanged);
					this._reminderPickerProxy.KeyUp				+= new KeyEventHandler(reminderPicker_KeyUp);

					this._reminderPickerProxy.ItemsSource		= AppointmentDialogUtilities.GetReminderListItems();
				}

				// Reminder label 
				this._reminderPickerLabel	= this.GetTemplateChild(PartReminderPickerLabel) as TextBlock;

				// Reminder image
				this._reminderPickerImage	= this.GetTemplateChild(PartReminderPickerImage) as Image;

				// Recurrence Button
				this._recurrenceButton		= this.GetTemplateChild(PartRecurrenceButton) as Button;

				// TimeZones Button
				this._timeZonesButton		= this.GetTemplateChild(PartTimeZonesButton) as ToggleButton;

				// Categorize Button
				this._categorizeButton		= this.GetTemplateChild(PartCategorizeButton) as ToggleButton;

				// Find the ActivityDialogCore that we are within.
				this._adc					= PresentationUtilities.GetVisualAncestor<ActivityDialogCore>(this, null);

				// JM 05-08-12 TFS104446
				bool isActivityLocked		= this._adc.IsActivityModifiable == false;

				bool reminderToolCollapsed = false, recurrenceToolCollapsed = false, timeZoneToolCollapsed = false;

				// Initialize the Reminder combo or hide it if Reminders are not allowed for the activity.
				if (this._reminderPickerProxy != null)
				{
					if (true == this._isReminderToolRequired &&
						true == this._adc.DataManager.IsActivityReminderAllowed(this._adc.Activity))
					{
						TimeSpan? ts = null;
						if (true == this._adc.Activity.ReminderEnabled)
							ts = this._adc.Activity.ReminderInterval;

						this._reminderPickerProxy.Text = DurationListItem.DurationStringFromTimeSpan(ts);

						// JM 05-08-12 TFS104446
						if (isActivityLocked)
							this._reminderPickerProxy.ComboBoxControl.IsEnabled = false;
					}
					else
					{
						reminderToolCollapsed									= true;

						this._reminderPickerProxy.ComboBoxControl.Visibility	= Visibility.Collapsed;

						if (null != this._reminderPickerLabel)
							this._reminderPickerLabel.Visibility				= Visibility.Collapsed;

						if (null != this._reminderPickerImage)
							this._reminderPickerImage.Visibility				= Visibility.Collapsed;
					}
				}

				// Hide the Recurrence button if Recurrences are not allowed for the activity.
				// JM 11-2-10 TFS58955 - Also hide the Recurrence button if we are editing an occurrence.
				//if (this._recurrenceButton != null && false == this._adc.DataManager.IsRecurringActivityAllowed(this._adc.Appointment))
				if (this._recurrenceButton != null)
				{
					if (false	== this._isRecurrenceToolRequired	||
						true	== this._adc.IsOccurrence			||
						false	== this._adc.DataManager.IsRecurringActivityAllowed(this._adc.Activity))
					{
						recurrenceToolCollapsed				= true;
						this._recurrenceButton.Visibility	= Visibility.Collapsed;
					}
				}
				
				// Hide the TimeZones buttons if we are editing an Occurrence
				if (null != this._timeZonesButton && (true	== this._adc.IsOccurrence ||
													  false	== this._isTimeZoneToolRequired))
				{
					timeZoneToolCollapsed				= true;
					this._timeZonesButton.Visibility	= Visibility.Collapsed;
				}

				// Collapse the OptionsGroup if the Recurrence, Reminder and TimeZone buttons are all collapsed.
				if (recurrenceToolCollapsed && reminderToolCollapsed && timeZoneToolCollapsed)
				{
					Grid optionsGroup = this.GetTemplateChild(PartOptionsGroup) as Grid;
					if (optionsGroup != null)
						optionsGroup.Visibility = Visibility.Collapsed;
				}

				// Hide the Categorize Button if we are editing an Occurrence, otherwise setup a context
				// menu for it and bind IsOpen of the menu to IsChecked of the button
				if (null != this._categorizeButton)
				{
					if (true == this._adc.IsOccurrence || this._adc.ShouldHideCategoriesButton)
					{
						this._categorizeButton.Visibility = Visibility.Collapsed;

						Grid tagsGroup = this.GetTemplateChild(PartTagsGroup) as Grid;
						if (tagsGroup != null)
							tagsGroup.Visibility = Visibility.Collapsed;
					}
					else
					{
						// Setup a Context Menu for the Categorize Button.
						ActivityCategoryHelper ach			= this._adc.ActivityCategoryHelper;
						this._categorizeContextMenu			= ach.GetContextMenu(this._categorizeButton, Infragistics.Controls.Menus.PlacementMode.AlignedBelow, 0d, 0d, null);

						Binding binding						= new Binding("IsChecked");
						binding.Source						= this._categorizeButton;
						binding.Mode						= BindingMode.TwoWay;
						this._categorizeContextMenu.SetBinding(XamContextMenu.IsOpenProperty, binding);

						// JM 9-29-11 TFS85588
						this._adc.Closing += new EventHandler(OnActivityDialogClosing);

						// JM 05-08-12 TFS104446
						if (isActivityLocked)
							this._categorizeButton.IsEnabled = false;
					}
				}

				this._initialized = true;
			}
		}
		#endregion //Initialize

		#region ParseReminderTextHelper
		private void ParseReminderTextHelper(bool forceUpdate)
		{
			DependencyObject	o;
			DependencyProperty	p;
			string				t;






			o = this._reminderPickerProxy.ComboBoxControl as ComboBox;
			p = ComboBox.TextProperty;
			t = ((ComboBox)this._reminderPickerProxy.ComboBoxControl).Text;


			AppointmentDialogUtilities.ParseReminderText(o, p, t, forceUpdate);
		}
		#endregion //ParseReminderTextHelper

		#endregion //Methods

		#region Event Handlers

		// JM 9-29-11 TFS85588 Added.
		#region OnActivityDialogClosing
		void OnActivityDialogClosing(object sender, EventArgs e)
		{
			if (this._categorizeContextMenu != null)
				this._categorizeContextMenu.IsOpen = false;
		}
		#endregion //OnActivityDialogClosing

		#region reminderPicker_KeyUp
		private void reminderPicker_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
				this.ParseReminderTextHelper(false);
			else
			{
				if (this._adc != null)
					this._adc.UpdateDirtyStatus(true);
			}
		}
		#endregion //reminderPicker_KeyUp

		#region reminderPicker_LostFocus
		private void reminderPicker_LostFocus(object sender, EventArgs e)
		{
			this.ParseReminderTextHelper(false);
		}
		#endregion //reminderPicker_LostFocus

		#region reminderPicker_SelectionChanged
		private void reminderPicker_SelectionChanged(object sender, EventArgs e)
		{
			if (true == this._initialized)
				this.ParseReminderTextHelper(true);
		}
		#endregion //reminderPicker_SelectionChanged

		#endregion //EventHandlers
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