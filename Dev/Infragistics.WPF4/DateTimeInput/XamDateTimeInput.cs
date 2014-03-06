using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using System.Windows;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Collections.Specialized;
using System.Collections;
using System.Windows.Media;
using System.Windows.Markup;
using System.Windows.Controls;
using Infragistics.Controls.Editors;


using Infragistics.Windows.Licensing;


using System.Windows.Automation.Peers;
using Infragistics.AutomationPeers;
using System.Windows.Threading;
using System.Threading;
using Infragistics.Collections;
using Infragistics.Controls;
using Infragistics.Controls.Primitives;
using System.Windows.Controls.Primitives;
using Infragistics.Controls.Editors.Primitives;


namespace Infragistics.Controls.Editors
{


	/// <summary>
	/// Allows editing of date and/or time based on a mask.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// <b>XamDateTimeInput</b> can be used to edit a date and/or time. Based on the value of the
	/// <see cref="XamMaskedInput.Mask"/> property, it can edit date, time or both date and time.
	/// The following are some example masks:<br/>
	/// <ul>
	/// <li><b>{date}</b> - Date mask based on <i>short</i> date pattern setting of the system.</li>
	/// <li><b>{time}</b> - Time mask based on <i>short</i> time pattern setting of the system. Short time pattern typically does not include seconds portion.</li>
	/// <li><b>{longtime} - Time mask based on <i>long</i> time pattern setting of the system. Long time pattern typically includes seconds portion.</b></li>
	/// 
	/// <li><b>{date}</b> - Creates a date only mask based on the short date pattern.</li>
	/// <li><b>{time}</b> - Creates a time only mask based on the short time pattern.</li>
	/// <li><b>{longtime}</b> - Creates a time only mask based on the long time pattern, which typically includes seconds.</li>
	/// <li><b>{date} {time}</b> - Creates a date and time mask based on the short date and short time patterns.</li>
	/// <li><b>mm/dd/yyyy</b> - Creates a date only mask. Note: This mask specifies the exact order of day, month and year explicitly. The underlying
	/// culture settings will not be used to determine the order of day, month and year section.</li>
	/// </ul>
	/// <br/>
	/// See help for <see cref="XamMaskedInput.Mask"/> property for a listing of all the supported masks,
	/// including ones that are relevant for date and time.
	/// </para>
	/// <para class="body">
	/// By default the current culture settings will be used to determine the format of date and time.
	/// However you can override that by setting the <see cref="ValueInput.FormatProvider"/> and 
	/// <see cref="ValueInput.Format"/> properties. If <b>FormatProvider</b> is set then the mask and the 
	/// formatting will be based on the settings provided by <b>FormatProvider</b>. Otherwise the formatting will 
	/// be based on the current culture. Note: the <b>Format</b> property only controls what gets displayed when
	/// the control is not in edit mode. See <see cref="ValueInput.Format"/> for more information.
	/// </para>
	/// <seealso cref="XamMaskedInput.Mask"/>
	/// <seealso cref="ValueInput.Value"/>
	/// <seealso cref="ValueInput.ValueType"/>
	/// <seealso cref="ValueInput.FormatProvider"/>
	/// <seealso cref="ValueInput.Format"/>
	/// </remarks>
	[StyleTypedProperty(Property = "DropDownButtonStyle", StyleTargetType = typeof(ToggleButton))]
	[TemplatePart(Name = PopupPart, Type = typeof(Popup))]
	[TemplatePart(Name = DropDownButtonPart, Type = typeof(ToggleButton))]
	[TemplatePart(Name = CalendarPart, Type = typeof(XamCalendar))]
	[TemplateVisualState(Name = VisualStateUtilities.StateFocusedDropDown, GroupName = VisualStateUtilities.GroupFocus)]

	
	

	public class XamDateTimeInput : XamMaskedInput
	{
		#region Member Variables

		private const string PopupPart = "PART_Popup";
		private const string DropDownButtonPart = "PART_DropDownButton";
		private const string CalendarPart = "PART_Calendar";

		private UltraLicense _license;


		private ControlDropDownManager _dropDownManager;
		private bool _isInitialized;
		private bool _isClosePending;
		private bool _hasTimeSectionsOnly;
		private Popup _popup;
		private ToggleButton _dropDownButton;
		private XamCalendar _calendar;

		#endregion //Member Variables

		#region Constructor

		static XamDateTimeInput()
		{

			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(XamDateTimeInput), new FrameworkPropertyMetadata(typeof(XamDateTimeInput)));


		}

		/// <summary>
		/// Initializes a new <see cref="XamDateTimeInput"/>
		/// </summary>
		public XamDateTimeInput()
		{


#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

			// verify and cache the license
			//
			// Wrapped in a try/catch for a FileNotFoundException.
			// When the assembly is loaded dynamically, VS seems 
			// to be trying to reload a copy of Shared even though 
			// one is in memory. This generates a FileNotFoundException
			// when the dll is not in the gac and not in the AppBase
			// for the AppDomain.
			//
			try
			{
				// We need to pass our type into the method since we do not want to pass in 
				// the derived type.
				this._license = LicenseManager.Validate(typeof(XamDateTimeInput), this) as UltraLicense;
			}
			catch (System.IO.FileNotFoundException) { }

		}

		#endregion //Constructor

		#region Base class overrides

		#region ArrangeOverride



#region Infragistics Source Cleanup (Region)











#endregion // Infragistics Source Cleanup (Region)


		#endregion //ArrangeOverride	
    
		#region DefaultValueType

		/// <summary>
		/// Returns the default value type of the editor. When the <see cref="ValueInput.ValueType"/> property is not set, this is
		/// the type that the <see cref="ValueInput.ValueTypeResolved"/> will return.
		/// </summary>
		protected override System.Type DefaultValueType
		{
			get
			{
				return typeof(DateTime);
			}
		}

		#endregion // DefaultValueType 

		#region HasDropDown
		internal override bool HasDropDown
		{
			get { return this.AllowDropDownResolved; }
		}
		#endregion //HasDropDown

		#region HasOpenDropDown
		internal override bool HasOpenDropDown
		{
			get
			{
				return this.IsDropDownOpen;
			}
		}
		#endregion //HasOpenDropDown

		#region InitializeValueProperties

		// SSP 1/16/12 TFS97623
		// 
		/// <summary>
		/// Overridden. Initializes the value properties. This synchronizes all the value properties if one of
		/// them is set in xaml since we delay syncrhonization until after initialization in case
		/// other settings in xaml effect how they are synchronized.
		/// </summary>
		internal override void InitializeValueProperties( )
		{
			if ( Utils.IsValuePropertySet( DateValueProperty, this )
				&& !Utils.IsValuePropertySet( ValueProperty, this ) )
			{
				this.SyncValueProperties( DateValueProperty, this.DateValue );
			}
			else
				base.InitializeValueProperties( );
		}

		#endregion // InitializeValueProperties

		#region OnApplyTemplate
		/// <summary>
		/// Invoked when the template has been applied to the editor.
		/// </summary>
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			if (_dropDownManager != null)
			{
				_dropDownManager.Dispose();
				_dropDownManager = null;
			}

			if (_calendar != null)
			{
				_calendar.SelectionCommitted -= new EventHandler(this.OnSelectionCommitted);
			}

			this._popup				= this.GetTemplateChild(PopupPart) as Popup;
			this._dropDownButton	= this.GetTemplateChild(DropDownButtonPart) as ToggleButton;
			this._calendar			= this.GetTemplateChild(CalendarPart) as XamCalendar;

			if (_calendar != null)
			{
				_calendar.SelectionCommitted += new EventHandler(this.OnSelectionCommitted);
			}

			if (_dropDownButton != null)
			{

				ControlDropDownManager.InitializeIfNotDefault(_dropDownButton, WidthProperty, SystemParameters.VerticalScrollBarWidth);



			}


			Microsoft.Windows.Themes.SystemDropShadowChrome chrome = null;

			if (null != this._popup)
			{
				_popup.SetBinding(Popup.IsOpenProperty, Infragistics.Windows.Utilities.CreateBindingObject(IsDropDownOpenProperty, System.Windows.Data.BindingMode.TwoWay, this));

				chrome = new Microsoft.Windows.Themes.SystemDropShadowChrome();

				if (_popup.HasDropShadow)
				{
					chrome.Margin = new Thickness(0, 0, 5, 5);
					chrome.Color = Color.FromArgb(113, 0, 0, 0);
				}
				else
				{
					chrome.Color = Colors.Transparent;
				}
			}


			if (null != this._popup)
			{
				this._dropDownManager = new ControlDropDownManager(this, _popup, _dropDownButton, new Action(this.OnPopopOpened), new Action(this.OnPopupClosed)

						, chrome

					);
			}






		}

		#endregion //OnApplyTemplate

		#region OnGotFocus

		/// <summary>
		/// Called when the control receives focus
		/// </summary>
		/// <param name="e"></param>
		protected override void OnGotFocus(RoutedEventArgs e)
		{
			base.OnGotFocus(e);

			if (this.IsDropDownOpen && _calendar != null)
				_calendar.Focus();

			this.UpdateDropDownButtonVisibility();
		}

		#endregion //OnGotFocus	
    
		#region OnInitialized

		/// <summary>
		/// Overridden. Raises the <see cref="FrameworkElement.Initialized"/> event. This method is invoked when the <see cref="FrameworkElement.IsInitialized"/> is set to true.
		/// </summary>
		/// <param name="e">Event arguments</param>
		protected override void OnInitialized(EventArgs e)



		{

			base.OnInitialized(e);

			if (_isInitialized)
				return;

			_isInitialized = true;

			// make sure we can show the calendar dropdown
			this.ResolveAllowDropDown();
			this.UpdateDropDownButtonVisibility(this.AllowDropDownResolved);
			this.UpdateComputedDates();

		}
		#endregion //OnInitialized

		#region OnIsKeyboardFocusWithinChanged


		/// <summary>
		/// Called when the keyboard focus state within the visual tree of this element has changed 
		/// </summary>
		/// <param name="e"></param>
		protected override void OnIsKeyboardFocusWithinChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnIsKeyboardFocusWithinChanged(e);

			if (this.IsDropDownOpen && _calendar != null)
				_calendar.Focus();

			this.UpdateDropDownButtonVisibility();
		}

		#endregion //OnIsKeyboardFocusWithinChanged	
    
		#region OnKeyDown


		/// <summary>
		/// Called when a key is pressed
		/// </summary>
		/// <param name="e"></param>
		protected override void OnKeyDown(KeyEventArgs e)
		{
			switch (e.Key)
			{
				case Key.Enter:
				case Key.Escape:
					{
						if (this.IsDropDownOpen)
						{
							if (e.Key == Key.Enter || e.Key == Key.Escape)
							{
								this.ToggleDropDown();
								e.Handled = true;
							}
						}
					}
					break;

				case Key.F4:
					if (Keyboard.Modifiers == 0)
					{
						this.ToggleDropDown();
						e.Handled = true;
					}
					break;

			}

			base.OnKeyDown(e);
		}

		#endregion //OnKeyDown	

		#region OnLostFocus



#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)


		#endregion //OnLostFocus	
    
		#region OnMouseEnter

		/// <summary>
		/// Called when the mouse enters the element
		/// </summary>
		/// <param name="e"></param>
		protected override void OnMouseEnter(MouseEventArgs e)
		{
			base.OnMouseEnter(e);

			this.UpdateDropDownButtonVisibility();
		}

		#endregion //OnMouseEnter	
    
		#region OnMouseLeave

		/// <summary>
		/// Called when the mouse leaves the element
		/// </summary>
		/// <param name="e"></param>
		protected override void OnMouseLeave(MouseEventArgs e)
		{
			base.OnMouseLeave(e);

			this.UpdateDropDownButtonVisibility();
		}

		#endregion //OnMouseLeave	
    
		#region OnIsReadOnlyChanged
		/// <summary>
		/// Called when the <see cref="Infragistics.Controls.Editors.ValueInput.IsReadOnly"/>  property has been changed
		/// </summary>
		protected override void OnIsReadOnlyChanged()
		{
			this.ResolveAllowDropDown();
		}
		#endregion //OnIsReadOnlyChanged

		#region ProcessPropertyChanged

		internal override void ProcessPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			base.ProcessPropertyChanged(e);

			DependencyProperty prop = e.Property;

			if (IsMouseOverProperty == prop
				|| DropDownButtonDisplayModeProperty == prop
				|| IsKeyboardFocusWithinProperty == prop)
			{
				this.UpdateDropDownButtonVisibility(this.AllowDropDownResolved);
			}
		}

		#endregion // ProcessPropertyChanged

		#region OnSectionsChanged
		internal override void OnSectionsChanged()
		{
			base.OnSectionsChanged();

			// AS 6/30/10 TFS32850
			// Since we will be allowing the dropdown when there are no date sections
			// we don't want to use Centuries in that case so if there are no date 
			// sections computedMode will be null and we will assume days.
			//
			//CalendarMode computedMode = CalendarZoomMode.Centuries;
			CalendarZoomMode? computedMode = null;

			// AS 9/24/08
			// Must reset this state.
			//
			
			
			
			

			// AS 6/30/10 TFS32850
			bool? hasTimeSectionsOnly = null;

			if (this.Sections != null)
			{
				foreach (SectionBase section in this.Sections)
				{
					if (section is DaySection)
					{
						
						
						
						

						computedMode = CalendarZoomMode.Days;

						// SSP 7/13/10 TFS32569
						// We can't break out without setting the _hasTimeSectionsOnly flag to false further below.
						// 
						//break;
					}
					else if (section is MonthSection)
					{
						
						
						
						

						// AS 6/30/10 TFS32850
						//if (computedMode > CalendarZoomMode.Months)
						if (computedMode == null || computedMode > CalendarZoomMode.Months)
							computedMode = CalendarZoomMode.Months;
					}
					else if (section is YearSection)
					{
						
						
						
						

						// AS 6/30/10 TFS32850
						//if (computedMode > CalendarZoomMode.Years)
						if (computedMode == null || computedMode > CalendarZoomMode.Years)
							computedMode = CalendarZoomMode.Years;
					}

					// AS 6/30/10 TFS32850
					if (section is HourSection ||
						section is MinuteSection ||
						section is AMPMSection ||
						section is SecondSection)
					{
						if (hasTimeSectionsOnly == null)
							hasTimeSectionsOnly = true;
					}
					else if (section is EditSectionBase)
						hasTimeSectionsOnly = false;
				}

				// AS 6/30/10 TFS32850
				_hasTimeSectionsOnly = hasTimeSectionsOnly == true;

				// if there were no date sections then assume days
				if (computedMode == null)
					computedMode = CalendarZoomMode.Days;
			}
			else
			{
				computedMode = CalendarZoomMode.Days;

				// AS 6/30/10 TFS32850
				_hasTimeSectionsOnly = false;
			}

			this.SetValue(ComputedMinCalendarModePropertyKey, computedMode);

			// update the button visibility if needed
			this.ResolveAllowDropDown();
			this.UpdateDropDownButtonVisibility(this.AllowDropDownResolved);
		}
		#endregion //OnSectionsChanged

		#region OnValueConstraintChanged
		internal override void OnValueConstraintChanged(string valueConstraintPropertyName)
		{
			base.OnValueConstraintChanged(valueConstraintPropertyName);

			this.UpdateComputedDates();
		}
		#endregion //OnValueConstraintChanged

		#region RaiseDropDownClosedIfPending
		internal override void RaiseDropDownClosedIfPending()
		{
			if (this._isClosePending)
			{
				this._isClosePending = false;
				this.RaiseDropDownClosed(new RoutedEventArgs());
			}
		}
		#endregion //RaiseDropDownClosedIfPending

		#region SyncValuePropertiesOverride

		// SSP 3/6/09 TFS15024
		// Added DateValue property. We need to synchronize it with other value properties.
		// 
		/// <summary>
		/// Overridden. Called to synchronize value and text properties. Derived classes can override this
		/// method if they have their own value properties (like the XamCheckEditor which has
		/// IsChecked property) because the Value, Text and all other editor specific value
		/// related properties need to be kept in sync. Default implementation synchronizes
		/// Value and Text properties.
		/// </summary>
		/// <param name="prop">Property that changed.</param>
		/// <param name="newValue">New value of the property.</param>
		/// <param name="error">Set this to the any error message if synchronization fails (for example
		/// because of type conversion or some other problem with synchronizing the new value).</param>
		/// <returns>Value indicating whether the new value should be considered valid. If false is
		/// returned, IsValueValid property will be set to false.</returns>
		internal override bool SyncValuePropertiesOverride(DependencyProperty prop, object newValue, out Exception error)
		{
			if (DateValueProperty == prop)
			{
				DateTime? newDate = (DateTime?)newValue;

				MaskInfo maskInfo = this.MaskInfo;
				EditInfo editInfo = this.EditInfo;
				SectionsCollection sections = maskInfo.Sections;
				if (null != editInfo)
				{
					// If in edit mode then set the year/month/day portions in the mask to reflect the
					// date being set. Leave the time portions the way they are.
					// 

					// Get the current text which we use to see if the value has changed so we can raise
					// proper notifications.
					// 
					string oldText = XamMaskedInput.GetText(sections, InputMaskMode.IncludeBoth, maskInfo);

					if (newDate.HasValue)
					{
						// Update the date sections.
						// 
						XamMaskedInput.SetDateTimeValue(sections, newDate.Value, editInfo.MaskInfo, true);
					}
					else
					{
						// If DataValue is being set to null, clear year, month and day sections.
						// 
						YearSection yearSection = (YearSection)XamMaskedInput.GetSection(sections, typeof(YearSection));
						MonthSection monthSection = (MonthSection)XamMaskedInput.GetSection(sections, typeof(MonthSection));
						DaySection daySection = (DaySection)XamMaskedInput.GetSection(sections, typeof(DaySection));

						yearSection.EraseAllChars();
						monthSection.EraseAllChars();
						daySection.EraseAllChars();
					}

					// If the text has changed, raise proper notifications and sync other value properties.
					// 
					string newText = XamMaskedInput.GetText( sections, InputMaskMode.IncludeBoth, maskInfo );
					if (oldText != newText)
					{
						editInfo.OnTextChanged(true);
						try
						{
							this.Value = editInfo.Value;
						}
						catch (Exception e)
						{
							this.Value = null;
							error = e;
							return false;
						}
						finally
						{
							this.SyncDisplayText();
						}

						error = null;
						return true;
					}
				}
				else
				{
					// If not in edit mode, update the Value property to reflect the set DateValue.
					// 
					object newDateTime = null;

					if (newDate.HasValue)
					{
						// Get the current date-time value of the editor.
						// 
						object value = this.Value;
						if (!CoreUtilities.IsValueEmpty(value) && !(value is DateTime))
							value = CoreUtilities.ConvertDataValue(value, typeof(DateTime), this.FormatProviderResolved, this.Format);

						if (value is DateTime)
						{
							// Create new date time based on the date portion of the set DateValue and
							// the time portion from the current Value.
							// 
							DateTime date = (DateTime)value;
							newDateTime = sections.Calendar.ToDateTime(
								newDate.Value.Year, newDate.Value.Month, newDate.Value.Day,
								date.Hour, date.Minute, date.Second, date.Millisecond);
						}
						else
						{
							// If there's no current value, then the set DateValue becomes the new Value.
							// 
							newDateTime = newDate;
						}
					}

					this.SetValue(ValueProperty, newDateTime);
					return base.SyncValuePropertiesOverride(ValueProperty, this.Value, out error);
				}

				error = null;
				return true;
			}
			else
			{
				// If some other value property changes, update the DateValue property to reflect
				// the new value of the editor.
				// 

				bool retVal = base.SyncValuePropertiesOverride(prop, newValue, out error);

				SectionsCollection sections = this.Sections;

				// SSP 9/22/11 TFS82033
				// Added the if block and enclosed the existing code into the else block. When the 
				// value is partially input, the Value property will retain the last value that 
				// was fully input. If the user selects that date from the calendar, it will be 
				// a NOOP since the DateValue property will reflect that previous fully input date,
				// even though the input currently is partial. Therefore set the DateValue to null
				// if the current input is partial.
				//
				object newVal;
				if ( !this.IsValueValid )
				{
					// If month, day, year sections are filled but time sections are not, try to
					// use the filled portions otherwise use null, which is what the GetDateValue
					// will do.
					// 
					newVal = GetDateValue( sections );
				}
				else
				{
					// SSP 2/3/10 TFS24992
					// If the ValueType is something other than DateTime, like string, then we should
					// get the date value from the sections. Otherwise if the mask data mode happens to
					// be raw, then there won't be any date separators in the value and we won't be
					// able to prase the value into a date time below.
					// 
					// ----------------------------------------------------------------------------------
					//object newVal = Utilities.ConvertDataValue( this.Value, typeof( DateTime ), this.FormatProviderResolved, this.Format );

					object currValue = this.Value;
					newVal = currValue is DateTime ? currValue : null;

					string tmp;
					if ( null == newVal && !CoreUtilities.IsValueEmpty( currValue )
						&& null != sections && IsMaskValidForDataType( typeof( DateTime ), sections )
						&& this.IsInputValid( sections, out tmp ) )
					{
						try
						{
							newVal = XamMaskedInput.GetDateTimeValue( this.Sections );
						}
						catch
						{
						}
					}

					if ( null == newVal )
						newVal = CoreUtilities.ConvertDataValue( currValue, typeof( DateTime ), this.FormatProviderResolved, this.Format );
					// ----------------------------------------------------------------------------------
				}

				this.SetValue(DateValueProperty, DBNull.Value != newVal ? newVal : null);

				return retVal;
			}
		}

		#endregion // SyncValuePropertiesOverride

		#region ToggleDropDown

		internal override void ToggleDropDown()
		{
			this.IsDropDownOpen = !this.IsDropDownOpen;
		}

		#endregion // ToggleDropDown

		#endregion //Base class overrides

		#region Properties

		#region AllowDropDown

		/// <summary>
		/// Identifies the <see cref="AllowDropDown"/> dependency property
		/// </summary>
		public static readonly DependencyProperty AllowDropDownProperty = DependencyPropertyUtilities.Register("AllowDropDown",
			typeof(bool?), typeof(XamDateTimeInput),
			DependencyPropertyUtilities.CreateMetadata(null, new PropertyChangedCallback(OnAllowDropDownChanged))
			);

		private static void OnAllowDropDownChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			XamDateTimeInput instance = (XamDateTimeInput)d;

			instance.ResolveAllowDropDown();
		}

		private void ResolveAllowDropDown()
		{
			bool? newVal = this.AllowDropDown;

			if (_hasTimeSectionsOnly)
				newVal = false;

			if (false == this.IsEditingAllowed)
				newVal = false;

			if (newVal.HasValue == false)
				newVal = true;

			this.AllowDropDownResolved = newVal.Value;
		}

		/// <summary>
		/// Returns or sets a value that indicates whether the dropdown should be used to select a date.
		/// </summary>
		/// <remarks>
		/// <p class="body">By default, the XamDateTimeInput will show a dropdown as long as it it can enter edit mode 
		/// and has a date mask - i.e. it will not show if the <see cref="XamMaskedInput.Mask"/> is set to a time-only 
		/// input mask. The AllowDropDown property can be used to prevent the dropdown from being available even when 
		/// the editor is used to edit a date. When set to false, the editor will not attempt to show the dropdown 
		/// calendar when using the mouse or keyboard.</p>
		/// </remarks>
		/// <seealso cref="AllowDropDownProperty"/>





		public bool? AllowDropDown
		{
			get
			{
				return (bool?)this.GetValue(XamDateTimeInput.AllowDropDownProperty);
			}
			set
			{
				this.SetValue(XamDateTimeInput.AllowDropDownProperty, value);
			}
		}

		#endregion //AllowDropDown

		#region AllowDropDownResolved

		private static readonly DependencyPropertyKey AllowDropDownResolvedPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("AllowDropDownResolved",
			typeof(bool), typeof(XamDateTimeInput),
			KnownBoxes.TrueBox,
			new PropertyChangedCallback(OnAllowDropDownResolvedChanged)
			);

		/// <summary>
		/// Identifies the read-only <see cref="AllowDropDownResolved"/> dependency property
		/// </summary>
		public static readonly DependencyProperty AllowDropDownResolvedProperty = AllowDropDownResolvedPropertyKey.DependencyProperty;

		private static void OnAllowDropDownResolvedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			XamDateTimeInput instance = (XamDateTimeInput)d;

			instance.UpdateDropDownButtonVisibility(instance.AllowDropDownResolved);

			instance.RaisePeerExpandCollapseChange();

		}

		/// <summary>
		/// Returns the resolved value that indicates whether the dropdown should be used to select a date (read-only).
		/// </summary>
		/// <remarks>
		/// <p class="body">By default, the XamDateTimeInput will show a dropdown as long as it it can enter edit mode 
		/// and has a date mask - i.e. it will not show if the <see cref="XamMaskedInput.Mask"/> is set to a time-only 
		/// input mask. The AllowDropDown property can be used to prevent the dropdown from being available even when 
		/// the editor is used to edit a date. When set to false, the editor will not attempt to show the dropdown 
		/// calendar when using the mouse or keyboard.</p>
		/// </remarks>
		/// <seealso cref="AllowDropDownProperty"/>
		/// <seealso cref="AllowDropDownResolvedProperty"/>
		[Bindable(true)]
		[ReadOnly(true)]

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]

		public bool AllowDropDownResolved
		{
			get
			{
				return (bool)this.GetValue(XamDateTimeInput.AllowDropDownResolvedProperty);
			}
			internal set
			{
				this.SetValue(XamDateTimeInput.AllowDropDownResolvedPropertyKey, value);
			}
		}

		#endregion //AllowDropDownResolved

		#region ComputedMaxDate

		private static readonly DependencyPropertyKey ComputedMaxDatePropertyKey = DependencyPropertyUtilities.RegisterReadOnly("ComputedMaxDate",
			typeof(DateTime), typeof(XamDateTimeInput), DateTime.MaxValue, null);

		/// <summary>
		/// Identifies the read-only <see cref="ComputedMaxDate"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ComputedMaxDateProperty = ComputedMaxDatePropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the calculated maximum date value for the control
		/// </summary>
		/// <remarks>
		/// <p class="body">The ComputedMaxDate and <see cref="ComputedMinDate"/> are read-only properties that 
		/// expose resolved DateTime values from the minimum and maximum properties of the <see cref="ValueInput.ValueConstraint"/> 
		/// and is meant to be used from within the template of the editor. The default template contains a <see cref="XamCalendar"/> 
		/// that uses these properties to control its <see cref="XamCalendar.MinDate"/> and <see cref="XamCalendar.MaxDate"/>.</p>
		/// </remarks>
		/// <seealso cref="ComputedMaxDateProperty"/>
		/// <seealso cref="ComputedMinDate"/>
		//[Description("Returns the calculated maximum date value for the control")]
		//[Category("Data")]
		[Bindable(true)]
		[ReadOnly(true)]
		public DateTime ComputedMaxDate
		{
			get
			{
				return (DateTime)this.GetValue(XamDateTimeInput.ComputedMaxDateProperty);
			}
			internal set
			{
				this.SetValue(XamDateTimeInput.ComputedMaxDatePropertyKey, value);
			}
		}

		#endregion //ComputedMaxDate

		#region ComputedMinCalendarMode

		private static readonly DependencyPropertyKey ComputedMinCalendarModePropertyKey = DependencyPropertyUtilities.RegisterReadOnly("ComputedMinCalendarMode",
			typeof(CalendarZoomMode), typeof(XamDateTimeInput), CalendarZoomMode.Days, null);

		/// <summary>
		/// Identifies the read-only <see cref="ComputedMinCalendarMode"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ComputedMinCalendarModeProperty = ComputedMinCalendarModePropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the preferred MinCalendarMode for the XamCalendar within the dropdown of the control.
		/// </summary>
		/// <remarks>
		/// <p class="body">The ComputedMinCalendarMode is a read only property that returns a <see cref="CalendarMode"/> 
		/// that identifies the smallest date unit that should be available within the <see cref="XamCalendar"/> 
		/// used in the dropdown of the control. For example, when the <see cref="XamMaskedInput.Mask"/> is set 
		/// to a mask that includes days, this property will return <b>Days</b> but if the smallest date unit 
		/// in the mask is months (e.g. mm/yyyy), then this property will return <b>Months</b> to indicate the 
		/// dropdown should only allow the user to see months and not days. This property is used within the default 
		/// template to control the <see cref="XamCalendar.MinCalendarMode"/> of the contained 
		/// <see cref="XamCalendar"/>.</p>
		/// </remarks>
		/// <seealso cref="ComputedMinCalendarModeProperty"/>
		//[Description("Returns the preferred MinCalendarMode for the XamCalendar within the dropdown of the control.")]
		//[Category("Data")]
		[Bindable(true)]
		[ReadOnly(true)]
		public CalendarZoomMode ComputedMinCalendarMode
		{
			get
			{
				return (CalendarZoomMode)this.GetValue(XamDateTimeInput.ComputedMinCalendarModeProperty);
			}
			internal set
			{
				this.SetValue(XamDateTimeInput.ComputedMinCalendarModePropertyKey, value);
			}
		}

		#endregion //ComputedMinCalendarMode

		#region ComputedMinDate

		private static readonly DependencyPropertyKey ComputedMinDatePropertyKey = DependencyPropertyUtilities.RegisterReadOnly("ComputedMinDate",
			typeof(DateTime), typeof(XamDateTimeInput), DateTime.MinValue, null);

		/// <summary>
		/// Identifies the read-only <see cref="ComputedMinDate"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ComputedMinDateProperty = ComputedMinDatePropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the calculated minimum date value for the control
		/// </summary>
		/// <p class="body">The ComputedMinDate and <see cref="ComputedMaxDate"/> are read-only properties that 
		/// expose resolved DateTime values from the minimum and maximum properties of the <see cref="ValueInput.ValueConstraint"/> 
		/// and is meant to be used from within the template of the editor. The default template contains a <see cref="XamCalendar"/> 
		/// that uses these properties to control its <see cref="XamCalendar.MinDate"/> and <see cref="XamCalendar.MaxDate"/>.</p>
		/// <seealso cref="ComputedMinDateProperty"/>
		/// <seealso cref="ComputedMaxDate"/>
		//[Description("Returns the calculated minimum date value for the control")]
		//[Category("Data")]
		[Bindable(true)]
		[ReadOnly(true)]
		public DateTime ComputedMinDate
		{
			get
			{
				return (DateTime)this.GetValue(XamDateTimeInput.ComputedMinDateProperty);
			}
			internal set
			{
				this.SetValue(XamDateTimeInput.ComputedMinDatePropertyKey, value);
			}
		}

		#endregion //ComputedMinDate

		#region DateValue

		// Added DateValue property which is meant to be used in the template to bind to the 
		// SelectedDate property of the XamCalendar. It can't directly bind to Value 
		// property because we need to maintain the time portion in the masked editor when
		// a date is selected from the calendar.
		// 

		/// <summary>
		/// Identifies the <see cref="DateValue"/> dependency property
		/// </summary>
		public static readonly DependencyProperty DateValueProperty = DependencyPropertyUtilities.Register("DateValue",
			typeof(DateTime?), typeof(XamDateTimeInput),
			DependencyPropertyUtilities.CreateMetadata(null, new PropertyChangedCallback(OnDateValueChanged))
			);

		private static void OnDateValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			XamDateTimeInput instance = (XamDateTimeInput)d;
			instance.SyncValueProperties(DateValueProperty, e.NewValue);
		}

		/// <summary>
		/// Gets or sets the date portion of the editor's value.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>DateValue</b> property returns the date portion of the editor's value. If the 
		/// editor's current value is null or has invalid date in it, this property returns
		/// null.
		/// </para>
		/// <para class="body">
		/// Setting this property updates only the date portion of the editor's value. If
		/// the current value is null and the editor is in edit mode, only the date portion 
		/// of the editor's content is be updated, leaving the time portion empty. If the
		/// editor is not edit mode and the the current value is null, then setting DateValue
		/// property also sets the Value property to the same DateTime instance.
		/// </para>
		/// </remarks>
		/// <seealso cref="ValueInput.Value"/>
		//[Description( "The date portion of the editor's value." )]
		//[Category( "Data" )]
		[Bindable(true)]

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]





		[EditorBrowsable(EditorBrowsableState.Never)]
		[Browsable(false)]
		public DateTime? DateValue
		{
			get
			{
				return (DateTime?)this.GetValue(DateValueProperty);
			}
			set
			{
				this.SetValue(DateValueProperty, value);
			}
		}

		#endregion // DateValue

		#region DropDownButtonDisplayMode

		/// <summary>
		/// Identifies the <see cref="DropDownButtonDisplayMode"/> dependency property
		/// </summary>
		public static readonly DependencyProperty DropDownButtonDisplayModeProperty = DependencyPropertyUtilities.Register("DropDownButtonDisplayMode",
			typeof(DropDownButtonDisplayMode), typeof(XamDateTimeInput),
			DropDownButtonDisplayMode.MouseOver,
			new PropertyChangedCallback(OnDropDownButtonDisplayModeChanged)
			);

		private static void OnDropDownButtonDisplayModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			XamDateTimeInput instance = d as XamDateTimeInput;

			instance.UpdateDropDownButtonVisibility();
		}

		/// <summary>
		/// Specifies when to display the drop down button. Default is <b>MouseOver</b>.
		/// </summary>
		/// <remarks>
		/// <p class="body">The <b>DropDownButtonDisplayMode</b> determines when the drop down button should be displayed.</p>
		/// <p class="note"><b>Note</b> that the drop down button will always be displayed when the editor 
		/// is in edit mode.</p>
		/// </remarks>
		//[Description("Specifies when to display the drop down button")]
		//[Category("Behavior")]
		[Bindable(true)]
		public DropDownButtonDisplayMode DropDownButtonDisplayMode
		{
			get
			{
				return (DropDownButtonDisplayMode)this.GetValue(DropDownButtonDisplayModeProperty);
			}
			set
			{
				this.SetValue(DropDownButtonDisplayModeProperty, value);
			}
		}

		#endregion // DropDownButtonDisplayMode

		#region DropDownButtonStyle

		/// <summary>
		/// Identifies the <see cref="DropDownButtonStyle"/> dependency property
		/// </summary>
		public static readonly DependencyProperty DropDownButtonStyleProperty = DependencyPropertyUtilities.Register("DropDownButtonStyle",
			typeof(Style), typeof(XamDateTimeInput),
			DependencyPropertyUtilities.CreateMetadata(null)
			);

		/// <summary>
		/// Used for setting the Style of the drop-down button. Default value is null.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// Default value of this property is null. You can use this property to specify a Style object to use for the
		/// drop-down button displayed inside the editor.
		/// </para>
		/// </remarks>
		//[Description("Used for setting the Style of the drop-down button")]
		//[Category("Appearance")]
		[Bindable(true)]
		public Style DropDownButtonStyle
		{
			get
			{
				return (Style)this.GetValue(DropDownButtonStyleProperty);
			}
			set
			{
				this.SetValue(DropDownButtonStyleProperty, value);
			}
		}
		#endregion // DropDownButtonStyle

		#region DropDownButtonVisibilityProperty

		private static readonly DependencyPropertyKey DropDownButtonVisibilityPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("DropDownButtonVisibility",
			typeof(Visibility), typeof(XamDateTimeInput), KnownBoxes.VisibilityVisibleBox, null);

		/// <summary>
		/// Identifies the read-only <see cref="DropDownButtonVisibilityProperty"/> dependency property
		/// </summary>
		public static readonly DependencyProperty DropDownButtonVisibilityProperty = DropDownButtonVisibilityPropertyKey.DependencyProperty;

		/// <summary>
		/// Indicates whether the drop down button is currently visible or not.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// This property can be used to find out if the drop down button is visible or not.
		/// </para>
		/// <seealso cref="DropDownButtonDisplayMode"/>
		/// </remarks>
		//[Description("Indicates whether the drop down button is currently visible or not.")]
		//[Category("Appearance")]
		[Bindable(true)]
		[Browsable(false)]
		[ReadOnly(true)]

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]

		public Visibility DropDownButtonVisibility
		{
			get
			{
				return (Visibility)this.GetValue(DropDownButtonVisibilityProperty);
			}
			internal set
			{
				this.SetValue(XamDateTimeInput.DropDownButtonVisibilityPropertyKey, value);
			}
		}

		#endregion // DropDownButtonVisibility

		#region IsDropDownOpen

		/// <summary>
		/// Identifies the <see cref="IsDropDownOpen"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsDropDownOpenProperty = DependencyPropertyUtilities.Register(
			"IsDropDownOpen",
			typeof(bool),
			typeof(XamDateTimeInput),

			new FrameworkPropertyMetadata(KnownBoxes.FalseBox,
				FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
				new PropertyChangedCallback(OnIsDropDownOpenChanged),
				new CoerceValueCallback(OnCoerceIsDropDownOpen))






		);

		private static void OnIsDropDownOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			XamDateTimeInput editor = (XamDateTimeInput)d;
			bool isOpen = (bool)e.NewValue;

			editor.RaisePeerExpandCollapseChange();







			if (isOpen)
			{
				editor.RaiseDropDownClosedIfPending();

				editor.RaiseDropDownOpened(new RoutedEventArgs());
			}
			else
			{
				// put focus back into the focus site (if it has focus)

				if (editor.IsKeyboardFocusWithin)

					editor.SetFocusToFocusSite();

				editor._isClosePending = true;

				// only raise the closed event if we don't have a popup. if we do
				// then we will wait for its closed event
				if (editor._popup == null)
					editor.RaiseDropDownClosedIfPending();
			}

			editor.UpdateDropDownButtonVisibility();

			
			
			
		}

		/// <summary>
		/// When IsDroppedDown is set, we need to make sure that the editor is in edit mode and if not
		/// enter it into edit mode. That's what this coerce handler does.
		/// </summary>
		/// <param name="dependencyObject"></param>
		/// <param name="valueAsObject"></param>
		/// <returns></returns>
		private static object OnCoerceIsDropDownOpen(DependencyObject dependencyObject, object valueAsObject)
		{
			bool val = (bool)valueAsObject;
			XamDateTimeInput editor = (XamDateTimeInput)dependencyObject;

			if (val)
			{

				if (false == editor.IsLoaded)
				{
					editor.RegisterToOpenOnLoad();
					return KnownBoxes.FalseBox;
				}





				// Do not allow the dropdown when AllowDropDown is false.
				//
				if (false == editor.AllowDropDown)
					return KnownBoxes.FalseBox;

				// If the editor is read-only, disallow the drop-down from being displayed.
				// 
				if (editor.IsReadOnly)
					return false;
			}

			return val;
		}

		private void RegisterToOpenOnLoad()
		{
			RoutedEventHandler handler = new RoutedEventHandler(this.OpenOnLoad);
			this.Loaded -= handler;
			this.Loaded += handler;
		}

		private void OpenOnLoad(object sender, RoutedEventArgs e)
		{
			this.Loaded -= new RoutedEventHandler(this.OpenOnLoad);


			this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Input, 
				new Action(delegate()



				{
					this.VerifyIsDropDownOpen();
				}));
		}

		private void VerifyIsDropDownOpen()
		{

			this.CoerceValue(XamDateTimeInput.IsDropDownOpenProperty);


		}

		/// <summary>
		/// Specifies whether the drop down is currently open.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>IsDropDownOpen</b> returns a value indicating whether the drop down is currently open.
		/// You can set this property to open or close drop down as well. If you set this property
		/// to true and the editor is not in edit mode, it will be put in edit mode.
		/// </para>
		/// <para class="body">
		/// You can also hook into <see cref="DropDownOpened"/> and <see cref="DropDownClosed"/> events
		/// to be notified when the drop-down is opened and closed.
		/// </para>
		/// <seealso cref="DropDownOpened"/> <seealso cref="DropDownClosed"/>
		/// </remarks>
		//[Description( "Specifies whether the drop-down is dropped down" )]
		//[Category( "Behavior" )]
		[Bindable(true)]
		[Browsable(false)]

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]

		public bool IsDropDownOpen
		{
			get
			{
				return (bool)this.GetValue(IsDropDownOpenProperty);
			}
			set
			{
				this.SetValue(IsDropDownOpenProperty, value);
			}
		}
		#endregion // IsDropDownOpen

		#endregion //Properties

		#region Methods

		#region EnsureFocusInCalendar


		private void EnsureFocusInCalendar()
		{
			if (_calendar == null || this.IsDropDownOpen == false || _calendar.IsFocused == true)
				return;

			this._calendar.Focus();

			// if focus didn't take then try again asynchronously
			if (_calendar.IsFocused == false && _calendar.IsEnabled)
				this.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(this.EnsureFocusInCalendar));
			else
			{
				// JJD 10/12/11 - TFS90901
				// Make sure the state of the calendar in the popup is synchronized with
				// the settings on the XamDataTimeInput before the popup is shown
				this.InitializeCalendarStateInPopup();
			}
		}



		#endregion //EnsureFocusInCalendar	
    
		#region GetDate
		private DateTime? GetDate(object value, TimeSpan adjustment)
		{
			DateTime? dateValue = null;

			if (null != value)
			{
				MaskInfo maskInfo = this.MaskInfo;
				dateValue = (DateTime?)CoreUtilities.ConvertDataValue(value, typeof(DateTime), maskInfo.FormatProvider, maskInfo.Format);

				// if we have a date then strip the time portion and adjust the date
				if (null != dateValue)
				{
					// strip the time portion
					dateValue = dateValue.Value.Date;

					try
					{
						dateValue = dateValue.Value.Add(adjustment);
					}
					catch (ArgumentException)
					{
					}
				}
			}

			return dateValue;
		}
		#endregion //GetDate

		#region InitializeCalendarStateInPopup

		// JJD 10/12/11 - TFS90901 - added helper method
		private void InitializeCalendarStateInPopup()
		{
			// JJD 10/07/11 - TFS90901
			// Make sure that the calendar in the drop down starts out
			// with the current mode initialized the same as the MinCalendarMode
			_calendar.CurrentMode = _calendar.MinCalendarMode;

			// Make sure the selected date is in view
			if (_calendar.SelectedDate.HasValue)
			{
				// JJD 07/24/12 - TFS113395
				// Temporarily disable animations so the calendar doesn't
				// animate its dates during the call to BringDateIntoView
				// below.
				_calendar.DisableAnimations = true;

				try
				{
					_calendar.BringDateIntoView(_calendar.SelectedDate.Value);
				}
				finally
				{
					// JJD 07/24/12 - TFS113395
					// Re-enable animations after we have brought the
					// selected date into view
					_calendar.DisableAnimations = false;
				}
			}
		}

		#endregion //InitializeCalendarStateInPopup	
    
		#region OnPopupOpened

		private void OnPopopOpened()
		{


#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)

			this.EnsureFocusInCalendar();



		}

		#endregion //OnPopupOpened

		#region OnPopupClosed


		private void OnPopupClosed()
		{




			this.RaiseDropDownClosedIfPending();
		}

		#endregion //OnPopupClosed

		#region OnSelectionCommitted
		private void OnSelectionCommitted(object sender, EventArgs e)
		{
			if (sender == this._calendar)
			{
				if (this.IsDropDownOpen)
					this.ToggleDropDown();
			}
		}
		#endregion //OnSelectionCommitted

		#region RaisePeerExpandCollapseChange
		private void RaisePeerExpandCollapseChange()
		{
			XamMaskedInputAutomationPeer peer = FrameworkElementAutomationPeer.FromElement(this) as XamMaskedInputAutomationPeer;

			if (null != peer)
				peer.RaiseExpandCollapseChanged();
		}
		#endregion //RaisePeerExpandCollapseChange

		#region UpdateComputedDates
		private void UpdateComputedDates()
		{
			if (this._isInitialized == false)
				return;

			ValueConstraint constraint = this.ValueConstraint;

			object maxDate = DependencyProperty.UnsetValue;
			object minDate = DependencyProperty.UnsetValue;

			if (null != constraint)
			{
				MaskInfo maskInfo = this.MaskInfo;
				DateTime? maxEx = GetDate(constraint.MaxExclusive, new TimeSpan(-TimeSpan.TicksPerDay));
				DateTime? max = GetDate(constraint.MaxInclusive, TimeSpan.Zero);
				DateTime? minEx = GetDate(constraint.MinExclusive, new TimeSpan(TimeSpan.TicksPerDay));
				DateTime? min = GetDate(constraint.MinInclusive, TimeSpan.Zero);

				if (null != maxEx)
				{
					if (null != max && max < maxEx)
						maxDate = max;
					else
						maxDate = maxEx;
				}
				else if (null != max)
					maxDate = max;

				if (null != minEx)
				{
					if (null != min && min > minEx)
						minDate = min;
					else
						minDate = minEx;
				}
				else if (null != min)
					minDate = min;
			}

			if (maxDate == DependencyProperty.UnsetValue)
				this.ClearValue(ComputedMaxDatePropertyKey);
			else
				this.SetValue(ComputedMaxDatePropertyKey, maxDate);

			if (minDate == DependencyProperty.UnsetValue)
				this.ClearValue(ComputedMinDatePropertyKey);
			else
				this.SetValue(ComputedMinDatePropertyKey, minDate);
		}
		#endregion //UpdateComputedDates

		#region UpdateDropDownButtonVisibility

		/// <summary>
		/// Updates the DropDownButtonVisibility property according to the current state of the editor
		/// and DropDownButtonDisplayMode property setting.
		/// </summary>
		internal void UpdateDropDownButtonVisibility()
		{
			this.UpdateDropDownButtonVisibility(this.AllowDropDownResolved);
		}

		// Added hideButton param since the XamDateTimeEditor may need
		// to force the button to not show.
		//
		internal void UpdateDropDownButtonVisibility(bool allowButton)
		{
			bool isVisible = false;

			if (allowButton)
			{
				DropDownButtonDisplayMode displayMode = this.DropDownButtonDisplayMode;

				switch (displayMode)
				{
					case DropDownButtonDisplayMode.Always:
						isVisible = true;
						break;
					case DropDownButtonDisplayMode.Focused:
						isVisible = this.IsKeyboardFocusWithin || this.IsDropDownOpen;
						break;
						




					case DropDownButtonDisplayMode.MouseOver:
					default:
						isVisible = this.IsKeyboardFocusWithin || this.IsMouseOver || this.IsDropDownOpen;
						break;
				}
			}

			this.DropDownButtonVisibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
		}

		#endregion //UpdateDropDownButtonVisibility	
    
		#endregion //Methods

		#region Events

		#region DropDownOpened

		/// <summary>
		/// This method is called when the drop-down list is opened. It raises <see cref="DropDownOpened"/> event.
		/// </summary>
		/// <seealso cref="DropDownOpened"/>
		/// <seealso cref="DropDownClosed"/>
		protected virtual void OnDropDownOpened(EventArgs args)
		{
			if (this.DropDownOpened != null)
				this.DropDownOpened(this, args);
		}

		internal void RaiseDropDownOpened(EventArgs args)
		{
			// JJD 08/01/12 - TFS118198 
			// Notify the base class that the dropdown is now open
			this.OnIsDropDownOpenChanged(true);

			// SSP 9/20/11 TFS87057
			// Tell the editor to behave if it's still in edit mode since the focus will go to the drop-down
			// but the editor should logically be considered to be in edit mode.
			// 
			this.ConsiderIsInEditMode( "IsDropDownOpen", true );

			this.OnDropDownOpened(args);
		}

		/// <summary>
		/// Occurs when the drop-down calendar is opened.
		/// </summary>
		/// <seealso cref="OnDropDownOpened"/>
		/// <seealso cref="IsDropDownOpen"/>
		/// <seealso cref="DropDownClosed"/>
		public event EventHandler<EventArgs> DropDownOpened;

		#endregion //DropDownOpened

		#region DropDownClosed

		/// <summary>
		/// This method is called when the drop-down list is closed. It raises <see cref="DropDownClosed"/> event.
		/// </summary>
		/// <seealso cref="DropDownClosed"/>
		/// <seealso cref="DropDownClosed"/>
		protected virtual void OnDropDownClosed(EventArgs args)
		{
			if (this.DropDownClosed != null)
				this.DropDownClosed(this, args);
		}

		internal void RaiseDropDownClosed(RoutedEventArgs args)
		{
			// SSP 9/20/11 TFS87057
			// Tell the editor to behave if it's still in edit mode since the focus will go to the drop-down
			// but the editor should logically be considered to be in edit mode.
			// 
			// ------------------------------------------------------------------------------------------------


#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

			// JJD 08/01/12 - TFS118198 
			// Notify the base class that the dropdown is now closed
			this.OnIsDropDownOpenChanged(false);

			this.ConsiderIsInEditMode( "IsDropDownOpen", false );
			// ------------------------------------------------------------------------------------------------

			this.OnDropDownClosed(args);
		}

		/// <summary>
		/// Occurs when the drop-down calendar is closed.
		/// </summary>
		/// <seealso cref="OnDropDownClosed"/>
		/// <seealso cref="IsDropDownOpen"/>
		/// <seealso cref="DropDownOpened"/>
		//[Description("Occurs when the drop-down list is closed")]
		//[Category("Behavior")]
		public event EventHandler<EventArgs> DropDownClosed;

		#endregion //DropDownClosed

		#endregion //Events
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