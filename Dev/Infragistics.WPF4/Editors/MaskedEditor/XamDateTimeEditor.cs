using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Diagnostics;
using System.ComponentModel;
using System.Globalization;
using Infragistics.Shared;
using Infragistics.Windows;
using Infragistics.Windows.Resizing;
using Infragistics.Windows.Controls;
using Infragistics.Windows.Licensing;
using Infragistics.Windows.Commands;
using Infragistics.Windows.Editors.Events;
using Infragistics.Windows.Controls.Events;
using Infragistics.Windows.Helpers;
using Infragistics.Windows.Automation.Peers.Editors;
using System.Windows.Automation.Peers;
using System.Windows.Threading;
using Infragistics.Windows.Internal;


namespace Infragistics.Windows.Editors
{
    
#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

    /// <summary>
	/// Allows editing of date and/or time based on a mask.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// <b>XamDateTimeEditor</b> can be used to edit a date and/or time. Based on the value of the
	/// <see cref="XamMaskedEditor.Mask"/> property, it can edit date, time or both date and time.
	/// The following are some example masks:<br/>
	/// <ul>
	/// <li><b>{date}</b> - Date mask based on <i>short</i> date pattern setting of the system.</li>
	/// <li><b>{time}</b> - Time mask based on <i>short</i> time pattern setting of the system. Short time pattern typically does not include seconds portion.</li>
	/// <li><b>{longtime} - Time mask based on <i>long</i> time pattern setting of the system. Long time pattern typically includes seconds portion.</b></li>
	/// 
	/// <li><b>{date}</b> - Creates a date only mask based on the short date pattern.</li>
	/// <li><b>{time}</b> - Creates a time only mask based on the short time pattern.</li>
	/// <li><b>{longtime}</b> - Creates atime only mask based on the long time pattern, which typically includes seconds.</li>
	/// <li><b>{date} {time}</b> - Creates a date and time mask based on the short date and short time patterns.</li>
	/// <li><b>mm/dd/yyyy</b> - Creates a date only mask. Note: This mask specifies the exact order of day, month and year explicitly. The underlying
	/// culture settings will not be used to determine the order of day, month and year section.</li>
	/// </ul>
	/// <br/>
	/// See help for <see cref="XamMaskedEditor.Mask"/> property for a listing of all the supported masks,
	/// including ones that are relevant for date and time.
	/// </para>
	/// <para class="body">
	/// By default the current culture settings will be used to determine the format of date and time.
	/// However you can override that by setting the <see cref="ValueEditor.FormatProvider"/> and 
	/// <see cref="ValueEditor.Format"/> properties. If <b>FormatProvider</b> is set then the mask and the 
	/// formatting will be based on the settings provided by <b>FormatProvider</b>. Otherwise the formatting will 
	/// be based on the current culture. Note: the <b>Format</b> property only controls what gets displayed when
	/// the control is not in edit mode. See <see cref="ValueEditor.Format"/> for more information.
	/// </para>
	/// <seealso cref="XamMaskedEditor.Mask"/>
	/// <seealso cref="ValueEditor.Value"/>
	/// <seealso cref="ValueEditor.ValueType"/>
	/// <seealso cref="ValueEditor.FormatProvider"/>
	/// <seealso cref="ValueEditor.Format"/>
	/// </remarks>
	
	
	//[Description( "Masked editor for editing date and/or time values." )]
    [StyleTypedProperty(Property = "DropDownButtonStyle", StyleTargetType = typeof(ToggleButton))]
    [TemplatePart(Name="PART_Popup", Type=typeof(Popup))]
    [TemplatePart(Name = "PART_DropDownButton", Type = typeof(ToggleButton))]
    [TemplatePart(Name = "PART_Calendar", Type = typeof(XamMonthCalendar))]

    // JJD 4/15/10 - NA2010 Vol 2 - Added support for VisualStateManager
    [TemplateVisualState(Name = VisualStateUtilities.StateFocusedDropDown, GroupName = VisualStateUtilities.GroupFocus)]

    public class XamDateTimeEditor : XamMaskedEditor
	{
		#region static constants

		#endregion //static constants

		#region Variables

		private UltraLicense _license;

        // AS 9/3/08 NA 2008 Vol 2
        private Popup _popup;
		
		
		
        
        private bool _isClosePending;

		// AS 6/30/10 TFS32850
		private bool _hasTimeSectionsOnly;

		#endregion //Variables

		#region Constructors

		static XamDateTimeEditor( )
		{
			// Default value type for this editor should be DateTime.
			ValueTypeProperty.OverrideMetadata( typeof( XamDateTimeEditor ), new FrameworkPropertyMetadata( typeof( DateTime ) ) );

			//This OverrideMetadata call tells the system that this element wants to provide a style that is different than its base class.
			//This style is defined in themes\generic.xaml
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata( typeof( XamDateTimeEditor ), new FrameworkPropertyMetadata( typeof( XamDateTimeEditor ) ) );

			// AS 3/24/10 TFS28062
			// Changed name to SelectionCommitted instead of SelectionMouseUp
			//
			// AS 9/8/08 NA 2008 Vol 2
            EventManager.RegisterClassHandler(typeof(XamDateTimeEditor), XamMonthCalendar.SelectionCommittedEvent, new RoutedEventHandler(OnSelectionCommitted));

            // AS 10/6/08
            ValueEditor.IsReadOnlyProperty.OverrideMetadata(typeof(XamDateTimeEditor), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnIsReadOnlyChanged)));
		}

		/// <summary>
		/// Initializes a new <see cref="XamDateTimeEditor"/>
		/// </summary>
		public XamDateTimeEditor( )
		{
			// verify and cache the license
			//
			// Wrapped in a try/catch for a FileNotFoundException.
			// When the assembly is loaded dynamically, VS seems 
			// to be trying to reload a copy of Shared even though 
			// one is in memory. This generates a FileNotFoundException
			// when the dll is not in the gac and not in the AppBase
			// for the AppDomain.
			//
			// AS 11/7/07 BR21903
			// Always do the license checks.
			//
			//if ( DesignerProperties.GetIsInDesignMode( this ) )
			{
				try
				{
					// We need to pass our type into the method since we do not want to pass in 
					// the derived type.
					this._license = LicenseManager.Validate( typeof( XamDateTimeEditor ), this ) as UltraLicense;
				}
				catch ( System.IO.FileNotFoundException ) { }
			}
		}

		#endregion // Constructors

        #region Base class overrides

        #region HasDropDown
        internal override bool HasDropDown
        {
            get { return this.AllowDropDown; }
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
				&& ! Utils.IsValuePropertySet( ValueProperty, this ) )
			{
				this.CoerceValue( DateValueProperty );
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

            Debug.Assert(this.IsInEditMode == false || null != this.GetTemplateChild("PART_Calendar"));

            if (null != this._popup)
            {
                this._popup.Opened -= new EventHandler(OnPopupOpened);
                this._popup.Closed -= new EventHandler(OnPopupClosed);
            }

            this._popup = this.GetTemplateChild("PART_Popup") as Popup;

            if (null != this._popup)
            {
                this._popup.Opened += new EventHandler(OnPopupOpened);
                this._popup.Closed += new EventHandler(OnPopupClosed);
            }
        }
        #endregion //OnApplyTemplate

        #region OnInitialized
        /// <summary>
        /// Overriden. Raises the <see cref="FrameworkElement.Initialized"/> event. This method is invoked when the <see cref="FrameworkElement.IsInitialized"/> is set to true.
        /// </summary>
        /// <param name="e">Event arguments</param>
        protected override void OnInitialized(EventArgs e)
        {
            // make sure we can show the calendar dropdown
            this.CoerceValue(AllowDropDownProperty);
            this.UpdateDropDownButtonVisibility(this.AllowDropDown);
            this.UpdateComputedDates();

            base.OnInitialized(e);
        } 
        #endregion //OnInitialized

        #region OnPropertyChanged
        /// <summary>
        /// Called when a dependency property changes.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            DependencyProperty prop = e.Property;

            if (UIElement.IsMouseOverProperty == prop
                || XamDateTimeEditor.DropDownButtonDisplayModeProperty == prop
                || ValueEditor.IsInEditModeProperty == prop)
            {
                this.UpdateDropDownButtonVisibility(this.AllowDropDown);
            }
        }
        #endregion //OnPropertyChanged

        #region OnSectionsChanged
        internal override void OnSectionsChanged()
        {
            base.OnSectionsChanged();

			// AS 6/30/10 TFS32850
			// Since we will be allowing the dropdown when there are no date sections
			// we don't want to use Centuries in that case so if there are no date 
			// sections computedMode will be null and we will assume days.
			//
			//CalendarMode computedMode = CalendarMode.Centuries;
			CalendarMode? computedMode = null;

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
						
						
						
						

						computedMode = CalendarMode.Days;

						// SSP 7/13/10 TFS32569
						// We can't break out without setting the _hasTimeSectionsOnly flag to false further below.
						// 
						//break;
					}
					else if (section is MonthSection)
					{
						
						
						
						

						// AS 6/30/10 TFS32850
						//if (computedMode > CalendarMode.Months)
						if (computedMode == null || computedMode > CalendarMode.Months)
							computedMode = CalendarMode.Months;
					}
					else if (section is YearSection)
					{
						
						
						
						

						// AS 6/30/10 TFS32850
						//if (computedMode > CalendarMode.Years)
						if (computedMode == null || computedMode > CalendarMode.Years)
							computedMode = CalendarMode.Years;
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
					computedMode = CalendarMode.Days;
			}
			else
			{
				computedMode = CalendarMode.Days;

				// AS 6/30/10 TFS32850
				_hasTimeSectionsOnly = false;
			}

            this.SetValue(ComputedMinCalendarModePropertyKey, computedMode);

            // update the button visibility if needed
            this.CoerceValue(AllowDropDownProperty);
            this.UpdateDropDownButtonVisibility(this.AllowDropDown);
        } 
        #endregion //OnSectionsChanged

        #region OnValueConstraintChanged
		internal override void OnValueConstraintChanged( string valueConstraintPropertyName )
        {
            base.OnValueConstraintChanged( valueConstraintPropertyName );

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
		/// because of type conversion or some other problem with syncrhonizing the new value).</param>
		/// <returns>Value indicating whether the new value should be considered valid. If false is
		/// returned, IsValueValid property will be set to false.</returns>
		internal override bool SyncValuePropertiesOverride( DependencyProperty prop, object newValue, out Exception error )
		{
			if ( DateValueProperty == prop )
			{
				DateTime? newDate = (DateTime?)newValue;

				MaskInfo maskInfo = this.MaskInfo;
				EditInfo editInfo = this.EditInfo;
				SectionsCollection sections = maskInfo.Sections;
				if ( null != editInfo )
				{
					// If in edit mode then set the year/month/day portions in the mask to reflect the
					// date being set. Leave the time poritions the way they are.
					// 

					// Get the current text which we use to see if the value has changed so we can raise
					// proper notifications.
					// 
					string oldText = XamMaskedEditor.GetText( sections, MaskMode.IncludeBoth, maskInfo );

					if ( newDate.HasValue )
					{
						// Update the date sections.
						// 
						XamMaskedEditor.SetDateTimeValue( sections, newDate.Value, editInfo.MaskInfo, true );
					}
					else
					{
						// If DataValue is being set to null, clear year, month and day sections.
						// 
						YearSection yearSection = (YearSection)XamMaskedEditor.GetSection( sections, typeof( YearSection ) );
						MonthSection monthSection = (MonthSection)XamMaskedEditor.GetSection( sections, typeof( MonthSection ) );
						DaySection daySection = (DaySection)XamMaskedEditor.GetSection( sections, typeof( DaySection ) );

						yearSection.EraseAllChars( );
						monthSection.EraseAllChars( );
						daySection.EraseAllChars( );
					}

					// If the text has changed, raise proper notifications and sync other value properties.
					// 
					string newText = XamMaskedEditor.GetText( sections, MaskMode.IncludeBoth, maskInfo );
					if ( oldText != newText )
					{
						editInfo.OnTextChanged( true );
						try
						{
							this.Value = editInfo.Value;
						}
						catch ( Exception e )
						{
							this.Value = null;
							error = e;
							return false;
						}
						finally
						{
							this.SyncDisplayText( );
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

					if ( newDate.HasValue )
					{
						// Get the current date-time value of the editor.
						// 
						object value = this.Value;
						if ( !Utils.IsValueEmpty( value ) && !( value is DateTime ) )
							value = Utilities.ConvertDataValue( value, typeof( DateTime ), this.FormatProviderResolved, this.Format );

						if ( value is DateTime )
						{
							// Create new date time based on the date portion of the set DateValue and
							// the time portion from the current Value.
							// 
							DateTime date = (DateTime)value;
							newDateTime = sections.Calendar.ToDateTime(
								newDate.Value.Year, newDate.Value.Month, newDate.Value.Day,
								date.Hour, date.Minute, date.Second, date.Millisecond );
						}
						else
						{
							// If there's no current value, then the set DateValue becomes the new Value.
							// 
							newDateTime = newDate;
						}
					}

					this.SetValue( ValueProperty, newDateTime );
					return base.SyncValuePropertiesOverride( ValueProperty, this.Value, out error );
				}

				error = null;
				return true;
			}
			else
			{
				// If some other value property changes, update the DateValue property to reflect
				// the new value of the editor.
				// 

				bool retVal = base.SyncValuePropertiesOverride( prop, newValue, out error );

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
					if ( null == newVal && !Utils.IsValueEmpty( currValue )
						&& null != sections && IsMaskValidForDataType( typeof( DateTime ), sections )
						&& this.IsInputValid( sections, out tmp ) )
					{
						try
						{
							newVal = XamMaskedEditor.GetDateTimeValue( this.Sections );
						}
						catch
						{
						}
					}

					if ( null == newVal )
						newVal = Utilities.ConvertDataValue( currValue, typeof( DateTime ), this.FormatProviderResolved, this.Format );
					// ----------------------------------------------------------------------------------
				}

				this.SetValue( DateValueProperty, DBNull.Value != newVal ? newVal : null );

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
        public static readonly DependencyProperty AllowDropDownProperty = DependencyProperty.Register("AllowDropDown",
            typeof(bool), typeof(XamDateTimeEditor), new FrameworkPropertyMetadata(KnownBoxes.TrueBox, new PropertyChangedCallback(OnAllowDropDownChanged), new CoerceValueCallback(CoerceAllowDropDown)));

        private static object CoerceAllowDropDown(DependencyObject d, object newValue)
        {
            XamDateTimeEditor editor = (XamDateTimeEditor)d;

			// AS 6/30/10 TFS32850
			//if (editor._hasDateSections == false)
			if (editor._hasTimeSectionsOnly)
				return KnownBoxes.FalseBox;

            // AS 10/3/08 TFS8634
            if (false == editor.IsEditingAllowed)
                return KnownBoxes.FalseBox;

            // AS 10/6/08
            // In order to maintain backward compatibility such that you cannot 
            // change the value of the editor when isreadonly is true we will 
            // not allow dropdown in that situation. In theory we should do that 
            // for the xamcomboeditor but currently its consistent with the intrinsic
            // combobox and its already in release allowing you to change the value
            // via selection when isreadonly is true.
            //
			// SSP 11/9/10 TFS33587
			// To be consistent with the combo editor, we do want to show the drop-down
			// however disable it. Commented out the following existing code.
			// 
            //if (editor.IsReadOnly)
            //    return KnownBoxes.FalseBox;

            return newValue;
        }

        private static void OnAllowDropDownChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            XamDateTimeEditor editor = (XamDateTimeEditor)d;

            editor.UpdateDropDownButtonVisibility(editor.AllowDropDown);

            editor.RaisePeerExpandCollapseChange();
        }

        /// <summary>
        /// Returns or sets a value that indicates whether the dropdown should be used to select a date.
        /// </summary>
        /// <remarks>
        /// <p class="body">By default, the XamDateTimeEditor will show a dropdown as long as it it can enter edit mode 
        /// and has a date mask - i.e. it will not show if the <see cref="XamMaskedEditor.Mask"/> is set to a time-only 
        /// input mask. The AllowDropDown property can be used to prevent the dropdown from being available even when 
        /// the editor is used to edit a date. When set to false, the editor will not attempt to show the dropdown 
        /// calendar when using the mouse or keyboard.</p>
        /// </remarks>
        /// <seealso cref="AllowDropDownProperty"/>
        //[Description("Returns or sets a value that indicates whether the dropdown should be used to select a date.")]
        //[Category("Behavior")]
        [Bindable(true)]
        public bool AllowDropDown
        {
            get
            {
                return (bool)this.GetValue(XamDateTimeEditor.AllowDropDownProperty);
            }
            set
            {
                this.SetValue(XamDateTimeEditor.AllowDropDownProperty, value);
            }
        }

        #endregion //AllowDropDown

        #region ComputedMaxDate

        private static readonly DependencyPropertyKey ComputedMaxDatePropertyKey =
            DependencyProperty.RegisterReadOnly("ComputedMaxDate",
            typeof(DateTime), typeof(XamDateTimeEditor), new FrameworkPropertyMetadata(DateTime.MaxValue));

        /// <summary>
        /// Identifies the <see cref="ComputedMaxDate"/> dependency property
        /// </summary>
        public static readonly DependencyProperty ComputedMaxDateProperty =
            ComputedMaxDatePropertyKey.DependencyProperty;

        /// <summary>
        /// Returns the calculated maximum date value for the control
        /// </summary>
        /// <remarks>
        /// <p class="body">The ComputedMaxDate and <see cref="ComputedMinDate"/> are read-only properties that 
        /// expose resolved DateTime values from the minimum and maximum properties of the <see cref="ValueEditor.ValueConstraint"/> 
        /// and is meant to be used from within the template of the editor. The default template contains a <see cref="XamMonthCalendar"/> 
        /// that uses these properties to control its <see cref="XamMonthCalendar.MinDate"/> and <see cref="XamMonthCalendar.MaxDate"/>.</p>
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
                return (DateTime)this.GetValue(XamDateTimeEditor.ComputedMaxDateProperty);
            }
        }

        #endregion //ComputedMaxDate

        #region ComputedMinCalendarMode

        private static readonly DependencyPropertyKey ComputedMinCalendarModePropertyKey =
            DependencyProperty.RegisterReadOnly("ComputedMinCalendarMode",
            typeof(CalendarMode), typeof(XamDateTimeEditor), new FrameworkPropertyMetadata(CalendarMode.Days));

        /// <summary>
        /// Identifies the <see cref="ComputedMinCalendarMode"/> dependency property
        /// </summary>
        public static readonly DependencyProperty ComputedMinCalendarModeProperty =
            ComputedMinCalendarModePropertyKey.DependencyProperty;

        /// <summary>
        /// Returns the preferred MinCalendarMode for the XamMonthCalendar within the dropdown of the control.
        /// </summary>
        /// <remarks>
        /// <p class="body">The ComputedMinCalendarMode is a read only property that returns a <see cref="CalendarMode"/> 
        /// that identifies the smallest date unit that should be available within the <see cref="XamMonthCalendar"/> 
        /// used in the dropdown of the control. For example, when the <see cref="XamMaskedEditor.Mask"/> is set 
        /// to a mask that includes days, this property will return <b>Days</b> but if the smallest date unit 
        /// in the mask is months (e.g. mm/yyyy), then this property will return <b>Months</b> to indicate the 
        /// dropdown should only allow the user to see months and not days. This property is used within the default 
        /// template to control the <see cref="XamMonthCalendar.MinCalendarMode"/> of the contained 
        /// <see cref="XamMonthCalendar"/>.</p>
        /// </remarks>
        /// <seealso cref="ComputedMinCalendarModeProperty"/>
        //[Description("Returns the preferred MinCalendarMode for the XamMonthCalendar within the dropdown of the control.")]
        //[Category("Data")]
        [Bindable(true)]
        [ReadOnly(true)]
        public CalendarMode ComputedMinCalendarMode
        {
            get
            {
                return (CalendarMode)this.GetValue(XamDateTimeEditor.ComputedMinCalendarModeProperty);
            }
        }

        #endregion //ComputedMinCalendarMode

        #region ComputedMinDate

        private static readonly DependencyPropertyKey ComputedMinDatePropertyKey =
            DependencyProperty.RegisterReadOnly("ComputedMinDate",
            typeof(DateTime), typeof(XamDateTimeEditor), new FrameworkPropertyMetadata(DateTime.MinValue));

        /// <summary>
        /// Identifies the <see cref="ComputedMinDate"/> dependency property
        /// </summary>
        public static readonly DependencyProperty ComputedMinDateProperty =
            ComputedMinDatePropertyKey.DependencyProperty;

        /// <summary>
        /// Returns the calculated minimum date value for the control
        /// </summary>
        /// <p class="body">The ComputedMinDate and <see cref="ComputedMaxDate"/> are read-only properties that 
        /// expose resolved DateTime values from the minimum and maximum properties of the <see cref="ValueEditor.ValueConstraint"/> 
        /// and is meant to be used from within the template of the editor. The default template contains a <see cref="XamMonthCalendar"/> 
        /// that uses these properties to control its <see cref="XamMonthCalendar.MinDate"/> and <see cref="XamMonthCalendar.MaxDate"/>.</p>
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
                return (DateTime)this.GetValue(XamDateTimeEditor.ComputedMinDateProperty);
            }
        }

        #endregion //ComputedMinDate

		#region DateValue

		// SSP 3/6/09 TFS15024
		// Added DateValue property which is meant to be used in the template to bind to the 
		// SelectedDate property of the XamMonthCalendar. It can't directly bind to Value 
		// property because we need to maintain the time portion in the masked editor when
		// a date is selected from the calendar.
		// 

		/// <summary>
		/// Identifies the <see cref="DateValue"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty DateValueProperty = DependencyProperty.Register(
			"DateValue",
			typeof( DateTime? ),
			typeof( XamDateTimeEditor ),
			new FrameworkPropertyMetadata( null, FrameworkPropertyMetadataOptions.None,
				new PropertyChangedCallback( OnDateValueChanged ) )
		);

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
		/// <seealso cref="ValueEditor.Value"/>
		//[Description( "The date portion of the editor's value." )]
		//[Category( "Data" )]
		[Bindable( true )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		[EditorBrowsable( EditorBrowsableState.Never )]
		[Browsable( false )]
		public DateTime? DateValue
		{
			get
			{
				return (DateTime?)this.GetValue( DateValueProperty );
			}
			set
			{
				this.SetValue( DateValueProperty, value );
			}
		}

		private static void OnDateValueChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			XamDateTimeEditor editor = (XamDateTimeEditor)dependencyObject;
			editor.SyncValueProperties( DateValueProperty, e.NewValue );
		}

		#endregion // DateValue

        #region DropDownButtonDisplayMode

        /// <summary>
        /// Identifies the <see cref="DropDownButtonDisplayMode"/> dependency property
        /// </summary>
        public static readonly DependencyProperty DropDownButtonDisplayModeProperty = XamComboEditor.DropDownButtonDisplayModeProperty.AddOwner(typeof(XamDateTimeEditor));

        /// <summary>
        /// Specifies when to display the drop down button. Default is <b>MouseOver</b>.
        /// </summary>
        /// <remarks>
        /// <p class="body">The <b>DropDownButtonDisplayMode</b> determines when the drop down button should be displayed.</p>
        /// <p class="note"><b>Note</b> that the drop down button will always be displayed when the editor 
        /// is in edit mode. Therefore if the <see cref="ValueEditor.IsAlwaysInEditMode"/> is true, this property 
        /// will be ignored and the button will always be displayed.</p>
		/// <para class="body">
		/// Note that the default value of the property is <b>MouseOver</b> however styles 
		/// of some of the themes may explicitly set this property to some other value to
		/// ensure consistency with the default operating system behavior. For example, 
		/// the default XamComboEditor style in "Aero" theme used in Windows Vista sets this 
		/// property to <b>Always</b>.
		/// </para>
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
        public static readonly DependencyProperty DropDownButtonStyleProperty = XamComboEditor.DropDownButtonStyleProperty.AddOwner(typeof(XamDateTimeEditor));

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

        #region DropDownButtonVisibility

        /// <summary>
        /// Identifies the Read-Only <see cref="DropDownButtonVisibility"/> dependency property
        /// </summary>
        public static readonly DependencyProperty DropDownButtonVisibilityProperty = XamComboEditor.DropDownButtonVisibilityProperty.AddOwner(typeof(XamDateTimeEditor));

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
        }

        #endregion // DropDownButtonVisibility

		#region IsDropDownOpen

		/// <summary>
		/// Identifies the <see cref="IsDropDownOpen"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsDropDownOpenProperty = XamComboEditor.IsDropDownOpenProperty.AddOwner(typeof(XamDateTimeEditor),
            new FrameworkPropertyMetadata(KnownBoxes.FalseBox,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                new PropertyChangedCallback(OnIsDropDownOpenChanged), 
                new CoerceValueCallback(OnCoerceIsDropDownOpen)));

        private static void OnIsDropDownOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            XamDateTimeEditor editor = (XamDateTimeEditor)d;
            bool isOpen = (bool)e.NewValue;

            editor.RaisePeerExpandCollapseChange();

            editor.ProcessIsBeingEditedAndFocusedChanged();

            if (isOpen)
            {
                editor.RaiseDropDownClosedIfPending();

                editor.RaiseDropDownOpened(new RoutedEventArgs());
            }
            else
            {
                // put focus back into the focus site (if it has focus)
                if (editor.IsFocusWithin)
                    editor.SetFocusToFocusSite();

                editor._isClosePending = true;

                // only raise the closed event if we don't have a popup. if we do
                // then we will wait for its closed event
                if (editor._popup == null)
                    editor.RaiseDropDownClosedIfPending();
            }

            // AS 2/5/09 TFS13569
            editor.CoerceValue(ToolTipService.IsEnabledProperty);
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
            XamDateTimeEditor editor = (XamDateTimeEditor)dependencyObject;

            if (val)
            {
                if (false == editor.IsLoaded)
                {
                    editor.RegisterToOpenOnLoad();
                    return KnownBoxes.FalseBox;
                }

                // AS 10/17/08
                // Do not allow the dropdown when AllowDropDown is false.
                //
                if (false == editor.AllowDropDown)
                    return KnownBoxes.FalseBox;

				// SSP 11/9/10 TFS33587
				// If the editor is read-only, disallow the drop-down from being displayed.
				// 
				if ( editor.IsReadOnly )
					return false;

                editor.StartEditMode();
                if (!editor.IsInEditMode)
                    return KnownBoxes.FalseBox;
            }

            return val;
        }

        private void RegisterToOpenOnLoad()
        {
            Debug.Assert(this.IsLoaded == false);
            RoutedEventHandler handler = new RoutedEventHandler(this.OpenOnLoad);
            this.Loaded -= handler;
            this.Loaded += handler;
        }

        private void OpenOnLoad(object sender, RoutedEventArgs e)
        {
            this.Loaded -= new RoutedEventHandler(this.OpenOnLoad);

            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Input, 
                new DispatcherOperationCallback(delegate(object param){
                    this.CoerceValue(XamDateTimeEditor.IsDropDownOpenProperty);
                    return null;
                }), null);
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
				return (bool)this.GetValue( IsDropDownOpenProperty );
			}
			set
			{
				this.SetValue( IsDropDownOpenProperty, value );
			}
		}
		#endregion // IsDropDownOpen

		#endregion //Properties

		#region Methods

		#region GetDate
		private DateTime? GetDate(object value, TimeSpan adjustment)
        {
            DateTime? dateValue = null;

            if (null != value)
            {
                MaskInfo maskInfo = this.MaskInfo;
                dateValue = (DateTime?)Utilities.ConvertDataValue(value, typeof(DateTime), maskInfo.FormatProvider, maskInfo.Format);

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

        // AS 10/6/08
        #region OnIsReadOnlyChanged
        private static void OnIsReadOnlyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.CoerceValue(AllowDropDownProperty);
        } 
        #endregion //OnIsReadOnlyChanged

        #region OnPopupOpened
        private void OnPopupOpened(object sender, EventArgs e)
        {
            Popup p = (Popup)sender;

            if (null != p.Child)
                p.Child.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
        }
        #endregion //OnPopupOpened

        #region OnPopupClosed
        private void OnPopupClosed(object sender, EventArgs e)
        {
            this.RaiseDropDownClosedIfPending();
        } 
        #endregion //OnPopupClosed

		// AS 3/24/10 TFS28062
		// Changed name to SelectionCommitted instead of SelectionMouseUp
		//
		#region OnSelectionCommitted
        private static void OnSelectionCommitted(object sender, RoutedEventArgs e)
        {
            XamDateTimeEditor editor = sender as XamDateTimeEditor;
            XamMonthCalendar mc = e.OriginalSource as XamMonthCalendar;

            if (null != mc && mc.TemplatedParent == editor && editor.IsDropDownOpen)
                editor.ToggleDropDown();
        }
        #endregion //OnSelectionCommitted

        #region RaisePeerExpandCollapseChange
        private void RaisePeerExpandCollapseChange()
        {
            XamMaskedEditorAutomationPeer peer = UIElementAutomationPeer.FromElement(this) as XamMaskedEditorAutomationPeer;

            if (null != peer)
                peer.RaiseExpandCollapseChanged();
        }
        #endregion //RaisePeerExpandCollapseChange

        #region UpdateComputedDates
        private void UpdateComputedDates()
        {
            if (this.IsInitialized == false)
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

            this.SetValue(ComputedMaxDatePropertyKey, maxDate);
            this.SetValue(ComputedMinDatePropertyKey, minDate);
        }
        #endregion //UpdateComputedDates

        #endregion //Methods

        #region Events

        #region DropDownOpened

        /// <summary>
        /// Event ID for the <see cref="DropDownOpened"/> routed event
        /// </summary>
        /// <seealso cref="OnDropDownOpened"/>
        /// <seealso cref="DropDownClosed"/>
        public static readonly RoutedEvent DropDownOpenedEvent = XamComboEditor.DropDownOpenedEvent.AddOwner(typeof(XamDateTimeEditor));

        /// <summary>
        /// This method is called when the drop-down list is opened. It raises <see cref="DropDownOpened"/> event.
        /// </summary>
        /// <seealso cref="DropDownOpened"/>
        /// <seealso cref="DropDownClosed"/>
        protected virtual void OnDropDownOpened(RoutedEventArgs args)
        {
			// MD 7/16/10 - TFS26592
			// Call off to the new helper method to raise the event. 
            //this.RaiseEvent(args);
			this.RaiseEventHelper(args);
        }

        internal void RaiseDropDownOpened(RoutedEventArgs args)
        {
            args.RoutedEvent = XamDateTimeEditor.DropDownOpenedEvent;
            args.Source = this;
            this.OnDropDownOpened(args);
        }

        /// <summary>
        /// Occurs when the drop-down calendar is opened.
        /// </summary>
        /// <seealso cref="OnDropDownOpened"/>
        /// <seealso cref="IsDropDownOpen"/>
        /// <seealso cref="DropDownClosed"/>
        //[Description("Occurs when the dropdown has been opened.")]
        //[Category("Behavior")]
        public event EventHandler<RoutedEventArgs> DropDownOpened
        {
            add
            {
                base.AddHandler(XamDateTimeEditor.DropDownOpenedEvent, value);
            }
            remove
            {
                base.RemoveHandler(XamDateTimeEditor.DropDownOpenedEvent, value);
            }
        }

        #endregion //DropDownOpened

        #region DropDownClosed

        /// <summary>
        /// Event ID for the <see cref="DropDownClosed"/> routed event
        /// </summary>
        /// <seealso cref="OnDropDownClosed"/>
        /// <seealso cref="DropDownClosed"/>
        public static readonly RoutedEvent DropDownClosedEvent = XamComboEditor.DropDownClosedEvent.AddOwner(typeof(XamDateTimeEditor));

        /// <summary>
        /// This method is called when the drop-down list is closed. It raises <see cref="DropDownClosed"/> event.
        /// </summary>
        /// <seealso cref="DropDownClosed"/>
        /// <seealso cref="DropDownClosed"/>
        protected virtual void OnDropDownClosed(RoutedEventArgs args)
        {
			// MD 7/16/10 - TFS26592
			// Call off to the new helper method to raise the event. 
            //this.RaiseEvent(args);
			this.RaiseEventHelper(args);
        }

        internal void RaiseDropDownClosed(RoutedEventArgs args)
        {
            args.RoutedEvent = XamDateTimeEditor.DropDownClosedEvent;
            args.Source = this;
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
        public event EventHandler<RoutedEventArgs> DropDownClosed
        {
            add
            {
                base.AddHandler(XamDateTimeEditor.DropDownClosedEvent, value);
            }
            remove
            {
                base.RemoveHandler(XamDateTimeEditor.DropDownClosedEvent, value);
            }
        }

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