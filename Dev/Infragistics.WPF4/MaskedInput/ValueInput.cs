using System;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Controls;
using Infragistics;
using Infragistics.Collections;
using Infragistics.Controls;
using System.Globalization;
using System.Windows.Automation.Peers;
using Infragistics.Controls.Editors.Primitives;
using Infragistics.AutomationPeers;


using Infragistics.Windows.Helpers;


namespace Infragistics.Controls.Editors
{
	#region ValueInput Class

	/// <summary>
	/// An abstract base class that provides functionality for displaying or edit values. 
	/// </summary>
	/// <remarks>
	/// <p class="body">The <b>ValueInput</b> exposes <see cref="Value"/> and <see cref="Text"/> properties that return 
	/// the current value and the text representation of that value respectively. The <see cref="ValueToTextConverter"/> is 
	/// used to convert between the <b>Value</b> and <b>Text</b>.</p>
	/// <p class="body">The <b>ValueInput</b> supports being put into edit mode. When in edit mode, the <see cref="Value"/> of the control 
	/// can be changed by the end user. The <see cref="ValueInput.ValueConstraint"/> can be set to a <see cref="Infragistics.Controls.Editors.ValueConstraint"/> 
	/// instance to provide constraints such as minimum and maximum values that can be used to limit what is considered a valid value for the 
	/// control. The ValueConstraint's <see cref="Infragistics.Controls.Editors.ValueConstraint.Nullable"/> property can be used to specify if the control will accept a null entry. The <see cref="InvalidValueBehavior"/> 
	/// property determines how the control should behave when it is exiting edit mode and the current value is not valid.</p>
	/// </remarks>
    // JJD 4/15/10 - NA2010 Vol 2 - Added support for VisualStateManager
    [TemplateVisualState(Name = VisualStateUtilities.StateNormal,              GroupName = VisualStateUtilities.GroupCommon)]
    [TemplateVisualState(Name = VisualStateUtilities.StateReadOnly,            GroupName = VisualStateUtilities.GroupCommon)]
    [TemplateVisualState(Name = VisualStateUtilities.StateMouseOver,           GroupName = VisualStateUtilities.GroupCommon)]
    [TemplateVisualState(Name = VisualStateUtilities.StateDisabled,            GroupName = VisualStateUtilities.GroupCommon)]

    [TemplateVisualState(Name = VisualStateUtilities.StateFocused,             GroupName = VisualStateUtilities.GroupFocus)]
    [TemplateVisualState(Name = VisualStateUtilities.StateUnfocused,           GroupName = VisualStateUtilities.GroupFocus)]

    [TemplateVisualState(Name = VisualStateUtilities.StateInvalidFocusedEx,    GroupName = VisualStateUtilities.GroupValidationEx)]
    [TemplateVisualState(Name = VisualStateUtilities.StateInvalidUnfocusedEx,  GroupName = VisualStateUtilities.GroupValidationEx)]
    [TemplateVisualState(Name = VisualStateUtilities.StateValidEx,             GroupName = VisualStateUtilities.GroupValidationEx)]
	//[Description("Base class from which other value editors are derived from.")]
	public abstract class ValueInput : Control, IPropertyChangeListener



	{
		#region Private Members
		
		private DependencyObject _focusSite;
		private bool _initialized;
		private int _syncingValueProperties;
		internal bool _isInEndEditMode;
		
		// SSP 12/31/07 BR27393
		// While we are synchronizing value properties, we need to delay raising changed notifications
		// until after all the value properties have been synchronized. Otherwise when a property
		// changed notification is raised, some of the other value properties may not have been
		// updated yet.
		// 
		private List<IRaiseEventDefinition> _pendingPropertyChangedEvents;

		
		
		private bool _processingPendingPropertyChangedEvents;

		// SSP 6/2/09 TFS17233
		// Added caching mechanism for this resolved property because it has to take into account
		// the language property and resolve a culture from it which can be inefficient, especially
		// when it gets called a lot, for example during sorting or grouping in the data presenter.
		// 
		private IFormatProvider _cachedFormatProviderResolved;

		private Type _cachedValueTypeResolved;


        private bool _hasVisualStateGroups;

		// JJD 07/17/12 - TFS90155 
		// added flag so we know that we used SetCurrentValue to disable the tab stop while we are
		// in edit mode
		private bool _wasTabStopDisabled;


		
		private LostFocusTracker _lostFocusTracker;

		private List<string> _considerInEditModeAttributes;
		private bool _cachedIsInEditMode;
		private bool _isInValidateInputHelper;

		#endregion //Private Members

		#region Private Static Members

		private bool _inSyncValueProperties;

		#endregion //Private Static Members

		#region Constructor

		static ValueInput()
		{

            // JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
            UIElement.IsEnabledProperty.OverrideMetadata(typeof(ValueInput), DependencyPropertyUtilities.CreateMetadata(new PropertyChangedCallback(OnVisualStatePropertyChanged)));

		}

		/// <summary>
		/// Initializes a new <see cref="ValueInput"/>
		/// </summary>
		protected ValueInput()
		{
			this.ValueTypeResolved = this.DefaultValueType;








			
			
			
			
			
			this.InitializeCachedPropertyValues( );
		}

		#endregion //Constructor	
    
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



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)


			this.VerifyFocusSite();


            // JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
            this._hasVisualStateGroups = VisualStateUtilities.GetHasVisualStateGroups(this);


			this.UpdateVisualStates(false);
        }

			#endregion //OnApplyTemplate	

			#region OnCreateAutomationPeer
		/// <summary>
		/// Returns an automation peer that exposes the <see cref="ValueInput"/> to UI Automation.
		/// </summary>
		/// <returns>A <see cref="ValueInputAutomationPeer"/></returns>
		protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer()
		{
			return new ValueInputAutomationPeer(this);
		}
			#endregion //OnCreateAutomationPeer

			#region OnInitialized


		/// <summary>
		/// Overridden.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnInitialized( EventArgs e )



		{

			base.OnInitialized( e );


			if ( !_initialized )
			{
				_initialized = true;

				this.DoInitialization( );
			}
		}

			#endregion // OnInitialized

			#region OnGotFocus

		/// <summary>
		/// Overridden. Called when the control receives focus.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnGotFocus( RoutedEventArgs e )
		{
			base.OnGotFocus( e );

			this.OnIsFocusWithinChangedHelper( true );
		} 

			#endregion // OnGotFocus

			#region OnIsKeyboardFocusWithinChanged


		/// <summary>
		/// Called when the keyboard focus shifts into or out of the visual tree of this element.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnIsKeyboardFocusWithinChanged( DependencyPropertyChangedEventArgs e )
		{
			base.OnIsKeyboardFocusWithinChanged( e );

			this.OnIsFocusWithinChangedHelper( (bool)e.NewValue );
		} 


			#endregion // OnIsKeyboardFocusWithinChanged

		    #region OnMouseEnter
		/// <summary>
		/// Invoked when the mouse is moved within the bounds of the element.
		/// </summary>
		/// <param name="e">Provides information about the mouse position.</param>
		protected override void OnMouseEnter(MouseEventArgs e)
		{
			base.OnMouseEnter(e);





            // JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
            this.UpdateVisualStates();
        }
		#endregion //OnMouseEnter

			#region OnMouseLeave
		/// <summary>
		/// Invoked when the mouse is moved outside the bounds of the element.
		/// </summary>
		/// <param name="e">Provides information about the mouse position.</param>
		protected override void OnMouseLeave(MouseEventArgs e)
		{
			base.OnMouseLeave(e);





            // JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
            this.UpdateVisualStates();
        }
		    #endregion //OnMouseLeave

			#region OnMouseLeftButtonDown

		/// <summary>
		/// Called when the left mouse button is pressed.
		/// </summary>
		/// <param name="e">The event arguments</param>
		/// <remarks>
		/// <p class="body">
		/// ValueInput overrides this method to give focus to the editor and enter edit mode
		/// if necessary in response to left mouse button down.
		/// </p>
		/// </remarks>
		protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			base.OnMouseLeftButtonDown( e );

			// SSP 5/8/08 BR32695
			// Moved the code into the new StartEditModeOnMouseDownHelper so we can call 
			// it from another place.
			// 
			if ( !e.Handled )
			{
				bool setFocusToFocusSite = true;

				// SSP 10/24/11 TFS92050
				// This is to fix an issue in windows phone where the text box ends up selecting
				// all of its text if we call focus when the mouse is clicked into the first time.
				// 


#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)


				if ( setFocusToFocusSite )
					this.SetFocusToFocusSite( );
			}
		}

			#endregion //OnMouseLeftButtonDown	

			#region OnPropertyChanged


		/// <summary>
		/// Called when a property value has changed
		/// </summary>
		/// <param name="e"></param>
		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnPropertyChanged(e);

			if (e.Property == IsMouseOverProperty)
				this.ProcessPropertyChanged(e);
		}


			#endregion //OnPropertyChanged	
    
		#endregion //Base class overrides

		#region Events

		#region ValidationError

		/// <summary>
		/// Occurs when the user attempts to leave the editor with an invalid value.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// The editor validates the user input when the user attempts to leave the control
		/// after modifying the value of the editor. EditModeValidationError event is raised 
		/// if the modified value is invalid. The editor considers a value invalid if the entered
		/// text can not be parsed into <see cref="ValueType"/> or it does not satisfy all the
		/// constraints specified by the <see cref="ValueConstraint"/>. 
		/// </para>
		/// <para class="body">
		/// See <see cref="InvalidValueBehavior"/> and <see cref="IsValueValid"/> for more information.
		/// </para>
		/// </remarks>
		/// <seealso cref="ValueConstraint"/>
		/// <seealso cref="IsValueValid"/>
		/// <seealso cref="InvalidValueBehavior"/>
		/// <seealso cref="EditModeValidationErrorEventArgs"/>
		public event EventHandler<EditModeValidationErrorEventArgs> ValidationError;

		/// <summary>
		/// Raises <see cref="ValidationError"/> event.
		/// </summary>
		/// <param name="args">Event arguments.</param>
		protected void RaiseValidationError( EditModeValidationErrorEventArgs args )
		{
			if ( null != this.ValidationError )
				this.ValidationError( this, args );
		}

		#endregion // ValidationError

		#endregion //Events

		#region Properties

		#region Private/Internal Properties

				#region AlwaysValidateResolved

		// SSP 2/6/09 TFS10586
		// Added AlwaysValidate property.
		// 
		/// <summary>
		/// Read-only. Gets the resolved value of the <see cref="AlwaysValidate"/> property.
		/// </summary>
		[ Browsable( false ), EditorBrowsable( EditorBrowsableState.Advanced ) ]
		public bool AlwaysValidateResolved
		{
			get
			{
				bool? val = this.AlwaysValidate;
				
				if ( val.HasValue )
					return val.Value;

				return false;
			}
		}

				#endregion // AlwaysValidateResolved

				#region CultureInfoResolved

		// SSP 7/9/08 BR34636
		// 
		/// <summary>
		/// Returns the culture info resolved from format provider setting and then from the language setting.
		/// </summary>
		internal CultureInfo CultureInfoResolved
		{
			get
			{
				CultureInfo ret = this.FormatProviderResolved as CultureInfo;
				if ( null != ret )
					return ret;

				return Utils.GetCultureInfo( this );
			}
		}

				#endregion // CultureInfoResolved

				#region FormatProviderResolved

		// SSP 9/12/07
		// Make use of Language.
		//
        // JJD 1/9/09 - made public
        /// <summary>
        /// Returns the resolved format provider.
        /// </summary>
        [EditorBrowsable( EditorBrowsableState.Never )]
        [Browsable(false)]
		//internal IFormatProvider FormatProviderResolved
		public IFormatProvider FormatProviderResolved
		{
			get
			{
				// SSP 6/2/09 TFS17233
				// Moved existing code into the new InternalFormatProviderResolved and added 
				// caching mechanism to cache the format provider resolved.
				// 
				if ( null == _cachedFormatProviderResolved )
					_cachedFormatProviderResolved = this.InternalFormatProviderResolved;

				return _cachedFormatProviderResolved;
			}
		}

		
		
		
		private IFormatProvider InternalFormatProviderResolved
		{
			get
			{
				IFormatProvider provider = this.FormatProvider;
				if ( null != provider )
					return provider;

				return Utils.GetCultureInfo( this );
			}
		}

				#endregion // FormatProviderResolved

				#region InvalidValueBehaviorResolved

		internal InvalidValueBehavior InvalidValueBehaviorResolved
		{
			get
			{
				
#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

				InvalidValueBehavior value = this.InvalidValueBehavior;


				if (value == InvalidValueBehavior.Default)
				{
					if (value == InvalidValueBehavior.Default)
						value = InvalidValueBehavior.DisplayErrorMessage;
				}

				return value;
			}
		}

				#endregion // InvalidValueBehaviorResolved

				#region InternalIsInitialized

		// SSP 8/19/08
		// 
		/// <summary>
		/// Returns the value of _initialized flag.
		/// </summary>
		internal bool InternalIsInitialized
		{
			get
			{
				return _initialized;
			}
		}
			
				#endregion // InternalIsInitialized

                // AS 10/3/08 TFS8634
                #region IsEditingAllowed
        internal bool IsEditingAllowed
        {
			get { return true; }
        }
                #endregion //IsEditingAllowed

				#region SyncingValueProperties

		internal bool SyncingValueProperties
		{
			get
			{
				return _syncingValueProperties > 0;
			}
		}

				#endregion // SyncingValueProperties

				#region ValueToTextConverterResolved

		// SSP 2/4/09 - NAS9.1 Record Filtering
		// Made ValueToDisplayTextConverterResolved public so the data presenter can convert 
		// cell values to text for displaying in filter drop-down.
		// 
		/// <summary>
		/// Resolved converter used for converting editor value to editor text.
		/// </summary>
		/// <seealso cref="ValueInput.ValueToTextConverter"/>
		/// <seealso cref="TextInputBase.ValueToDisplayTextConverter"/>
		[Browsable( false ), EditorBrowsable( EditorBrowsableState.Advanced )]
		public IValueConverter ValueToTextConverterResolved
		{
			get
			{
				IValueConverter converter = this.ValueToTextConverter;
				if ( null == converter )
					converter = this.DefaultValueToTextConverter;

				return converter;
			}
		}

				#endregion // ValueToTextConverterResolved

			#endregion // Private/Internal Properties

		#region Public Properties
				
				#region AlwaysValidate

		// SSP 2/6/09 TFS10586
		// Added AlwaysValidate property.
		// 

		/// <summary>
		/// Identifies the <see cref="AlwaysValidate"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty AlwaysValidateProperty = DependencyPropertyUtilities.Register(
  			"AlwaysValidate",
			typeof( bool? ),
			typeof( ValueInput ),
			DependencyPropertyUtilities.CreateMetadata( null, new PropertyChangedCallback( OnAlwaysValidateChanged ) ) 
		);

		/// <summary>
		/// Specifies whether to validate the editor's value even if the user doesn't modify the value. Default value
		/// is <b>False</b> where the editor only validates the value if the user modifies it.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// By default the editor validates the value only if the user has modified it, even if the editor's value is
		/// invalid. If the user doesn't modify the value and tries the leave the editor with invalid value that the
		/// editor was initialized with, no validation takes place. You can change this behavior by setting 
		/// <b>AlwaysValidate</b> to true, where the editor will
		/// take validation action based on the <see cref="InvalidValueBehavior"/> property setting as long as the 
		/// editor's value is invalid, regardless of whether the the user has modified it or not.
		/// </para>
		/// <para class="body">
		/// As an example, let's say ValueConstraint on the editor had MinLength constraint set to 5. The editor is
		/// initialized with string value "a", which doesn't meet the MinLength of 5 constraint. The editor's 
		/// IsValueValid would be <i>false</i>, since the current value doesn't meet the value constraint. If the 
		/// user tries to leave the editor without modifying the value, by default the editor will not take any 
		/// validation action and prompt the user of invalid value. However if you set <i>AlwaysValidate</i> to 
		/// <i>true</i>, the editor will take validation action. Note that if the user modifies the value, then
		/// the editor will validate regardless of this property setting.
		/// </para>
		/// </remarks>
		/// <seealso cref="ValueConstraint"/>
		/// <seealso cref="IsValueValid"/>
		/// <seealso cref="InvalidValueBehavior"/>
		/// <seealso cref="ValidationError"/>
		//[Description( "Specifies whether to validate the editor's value even if the user doesn't modify the value." )]
		//[Category( "Behavior" )]
		[Bindable( true )]





		public bool? AlwaysValidate
		{
			get
			{
				return (bool?)this.GetValue( AlwaysValidateProperty );
			}
			set
			{
				this.SetValue( AlwaysValidateProperty, value );
			}
		}

		private static void OnAlwaysValidateChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			ValueInput input = (ValueInput)dependencyObject;
			input.ProcessPropertyChanged( e );
		}

		/// <summary>
		/// Returns true if the AlwaysValidate property is set to a non-default value.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeAlwaysValidate( )
		{
			return DependencyPropertyUtilities.ShouldSerialize( this, AlwaysValidateProperty );
		}

		/// <summary>
		/// Resets the AlwaysValidate property to its default state.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetAlwaysValidate( )
		{
			this.ClearValue( AlwaysValidateProperty );
		}

				#endregion // AlwaysValidate

				#region IsKeyboardFocusWithin



#region Infragistics Source Cleanup (Region)



























#endregion // Infragistics Source Cleanup (Region)


				#endregion // IsKeyboardFocusWithin

				#region IsInEditMode

		private static readonly DependencyPropertyKey IsInEditModePropertyKey = DependencyPropertyUtilities.RegisterReadOnly(
			"IsInEditMode",
			typeof( bool ),
			typeof( ValueInput ),
			KnownBoxes.FalseBox,
			new PropertyChangedCallback( OnIsInEditModeChanged )
		);

		/// <summary>
		/// Identifies the read-only <see cref="IsInEditMode"/> dependency property
		/// </summary>
		private static readonly DependencyProperty IsInEditModeProperty = IsInEditModePropertyKey.DependencyProperty;

		private static void OnIsInEditModeChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			ValueInput editor = (ValueInput)d;
			editor._cachedIsInEditMode = (bool)e.NewValue;

			editor.OnIsInEditModeChanged( editor._cachedIsInEditMode );
		}

		internal void VerifyIsInEditMode( )
		{
			bool focused = null != _considerInEditModeAttributes && _considerInEditModeAttributes.Count > 0;

			if ( _cachedIsInEditMode != focused )
				this.SetValue( IsInEditModePropertyKey, focused );
		}

		/// <summary>
		/// Returns
		/// </summary>
		/// <seealso cref="IsInEditModeProperty"/>
		internal bool IsInEditMode
		{
			get
			{
				return _cachedIsInEditMode;
			}
		}

				#endregion // IsInEditMode

				#region IsMouseOver



#region Infragistics Source Cleanup (Region)

































#endregion // Infragistics Source Cleanup (Region)


				#endregion // IsMouseOver

				#region Value

		/// <summary>
		/// Identifies the <see cref="Value"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty ValueProperty = DependencyPropertyUtilities.Register(
			"Value",
			typeof( object ),
			typeof( ValueInput ),
			DependencyPropertyUtilities.CreateMetadata( null,
					
					new PropertyChangedCallback( OnValueChanged ),
					DependencyPropertyUtilities.MetadataOptionFlags.Journal | DependencyPropertyUtilities.MetadataOptionFlags.BindsTwoWayByDefault
				)
			);

		/// <summary>
		/// Gets or sets the value of the editor.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// The <see cref="ValueType"/> property specifies the type of values returned and expected 
		/// by the <b>Value</b> property. For example, if you set the <b>ValueType</b> to 
		/// <i>Decimal</i> then the <b>Value</b> property will return decimal values. The user input will
		/// be converted to Decimal before returning from Value property. The <see cref="Text"/> 
		/// property on the other hand always returns the text representation of the value.
		/// </para>
		/// <para class="note">
		/// <b>Note:</b> Setting the <b>Value</b> property will also update the <see cref="Text"/> property.
		/// </para>
		/// <para class="note">
		/// <b>Note:</b> As the user enters/modifies the contents of the <see cref="ValueInput"/>, the 
		/// <see cref="Text"/> and <b>Value</b> properties will be synchronously updated to reflect the current 
		/// contents. If the user input can not be parsed into the associated <see cref="ValueType"/>, the <b>Text</b> 
		/// property will be updated however the <b>Value</b> property will retain last parsed value. In such 
		/// a case, the <see cref="IsValueValid"/> property will return <b>false</b> indicating that the user input 
		/// is invalid.</para>
		/// <seealso cref="ValueType"/>
		/// <seealso cref="Text"/>
		/// <seealso cref="ValueConstraint"/>
		/// <seealso cref="IsValueValid"/>
		/// <seealso cref="HasValueChanged"/>
		/// <seealso cref="OriginalValue"/>
		/// </remarks>
		[Bindable( true )]
		public object Value
		{
			get
			{
				return (object)this.GetValue( ValueProperty );
			}
			set
			{
				this.SetValue( ValueProperty, value );
			}
		}

		private static void OnValueChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			ValueInput valueInput = (ValueInput)dependencyObject;

			// Don't syncrhonize between properties during the process of initialization 
			// since other properties like ValueType may or may not have been set yet.
			// 
			if ( ! valueInput._initialized )
				return;

			
			
			
			
			if ( Utils.Equals( e.OldValue, e.NewValue ) )
				return;

			
			
			
			valueInput.BeginSyncValueProperties( );
			try
			{
				// Call SyncValueProperties to synchronize the Text property with the new value.
				// 
				valueInput.SyncValueProperties( e.Property, e.NewValue );

				// Call virtual OnValueChanged method to let the derived editors know of change
				// in the Value property.
				// 
				valueInput.OnValueChanged( e.OldValue, e.NewValue );
			}
			finally
			{
				valueInput.EndSyncValueProperties( );
			}
		}

		/// <summary>
		/// Occurs when Value property changes or the user modifies the contents of the editor.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// ValueChanged event is raised when the <see cref="Value"/> property is changed or
		/// the user modifies the contents of the editor. When the user modifies the contents of 
		/// the editor, the Value property is updated to reflect the new content. However the
		/// editor may fail to update the Value property if the new contents of the editor can
		/// not be prased into the <see cref="ValueType"/>. For example if the ValueType is
		/// set to a numeric type like Double and the user enters a non-numeric value then the
		/// entered value can not be parsed into a Double. Therefore the Value property will not
		/// be updated. However the ValueChanged event will still be raised.
		/// </para>
		/// <seealso cref="IsValueValid"/>
		/// <seealso cref="Value"/>
		/// <seealso cref="Text"/>
		/// </remarks>
		public event EventHandler<EventArgs> ValueChanged;

		/// <summary>
		/// Called when <b>Value</b> property changes or the contents of the editor changes.
		/// </summary>
		/// <seealso cref="ValueInput.ValueChanged"/>
		protected virtual void OnValueChanged( object previousValue, object currentValue )
		{
			this.RaiseValuePropertyChangedEvent( new RaiseEventDefinition<EventArgs>( this, this.ValueChanged, new EventArgs( ) ) );
		}

		/// <summary>
		/// Called from the <see cref="Value"/> property's CoerceValue handler. The default 
		/// implementation performs type conversions therefore you should call the base implementation
		/// to ensure proper type conversions are performed.
		/// </summary>
		/// <returns></returns>
		internal void CoerceValueHelper( )
		{
			// If the value being set is not of the ValueType, then coerce the value to that type.
			// 
			object val = this.Value;
			if ( null != val && DBNull.Value != val && !this.ValueTypeResolved.IsInstanceOfType( val ) )
			{
				// SSP 9/12/07 - XamComboEditor
				// Combo editor's default text converter maps values to display text. Here we simply want
				// to convert the value to the ValueType.
				// 
				//val = this.DefaultValueToTextConverter.Convert( val, this.ValueType, this, null );
				this.Value = CoreUtilities.ConvertDataValue( val, this.ValueTypeResolved, this.FormatProviderResolved, this.Format );
			}
		}

				#endregion // Value

				#region Text

		/// <summary>
		/// Identifies the <see cref="Text"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty TextProperty = DependencyPropertyUtilities.Register(
			"Text",
			typeof( string ),
			typeof( ValueInput ),
			DependencyPropertyUtilities.CreateMetadata( string.Empty,
				new PropertyChangedCallback( OnTextChanged ),
				DependencyPropertyUtilities.MetadataOptionFlags.BindsTwoWayByDefault 
			)
		);

		/// <summary>
		/// Gets or sets the value of the editor as text.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// Setting the <b>Text</b> property will also update the <b>Value</b> property. If the
		/// new text can not be parsed into the value type (<see cref="ValueType"/>) then the
		/// <b>Value</b> property will not be updated.
		/// </para>
		/// <para class="body">
		/// <b>Note:</b> As the user enters/modifies contents, the Text and Value properties will
		/// be synchronously updated to reflect the current contents. If the user input can not be 
		/// parsed into the value type, the Text property will be updated however the Value property 
		/// will retain last parsed value. However in such a case <see cref="IsValueValid"/>
		/// property will return False indicating that the user input is invalid.
		/// </para>
		/// <seealso cref="Value"/>
		/// <seealso cref="ValueType"/>
		/// <seealso cref="ValueConstraint"/>
		/// <seealso cref="IsValueValid"/>
		/// <seealso cref="HasValueChanged"/>
		/// <seealso cref="OriginalValue"/>
		/// </remarks>
		[Bindable( true )]
		[Browsable( false )]
		public string Text
		{
			get
			{
				return (string)this.GetValue( TextProperty );
			}
			set
			{
				this.SetValue( TextProperty, value );
			}
		}

		private static void OnTextChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			ValueInput valueInput = (ValueInput)dependencyObject;

			// Don't syncrhonize between properties during the process of initialization 
			// since other properties like ValueType may or may not have been set yet.
			// 
			if ( ! valueInput._initialized )
				return;

			valueInput.SyncValueProperties( e.Property, e.NewValue );

			valueInput.OnTextChanged( (string)e.OldValue, (string)e.NewValue );
		}

		/// <summary>
		/// Occurs when property 'Text' changes
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// TextChanged event is raised when the <see cref="Text"/> property is changed or
		/// the user modifies the contents of the editor. When the user modifies the contents of 
		/// the editor, the Text property is updated to reflect the new content.
		/// </para>
		/// <seealso cref="Text"/>
		/// <seealso cref="Value"/>
		/// <seealso cref="ValueChanged"/>
		/// </remarks>
		public event EventHandler<EventArgs> TextChanged;


		/// <summary>
		/// Called when property 'Text' changes
		/// </summary>
		protected virtual void OnTextChanged( string previousValue, string currentValue )
		{
			this.RaiseValuePropertyChangedEvent( new RaiseEventDefinition<EventArgs>( this, this.TextChanged, new EventArgs( ) ) );
		}
		
		private static object OnCoerceText( DependencyObject dependencyObject, object valueAsObject )
		{
			ValueInput valueInput = (ValueInput)dependencyObject;
			string text = (string)valueAsObject;

			// Don't coerce during the process of initialization since ValueType may or 
			// may not have been set yet.
			// 
			if ( ! valueInput._initialized )
				return text;

			return valueInput.OnCoerceText( text );
		}

		/// <summary>
		/// Called from the <see cref="Text"/> property's CoerceValue handler. The default 
		/// implementation does nothing.
		/// </summary>
		/// <param name="text">The text to coerce</param>
		/// <returns>Coerced value</returns>
		/// <remarks>
		/// <para class="body">
		/// The default implementation simply returns the value of <paramref name="text"/> parameter.
		/// The derived editors can override this method to provide editor specific coersion logic.
		/// </para>
		/// </remarks>
		protected virtual string OnCoerceText( string text )
		{
			return text;
		}

				#endregion // Text
		
				#region ValueType

		/// <summary>
		/// Identifies the <see cref="ValueType"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty ValueTypeProperty = DependencyPropertyUtilities.Register(
			"ValueType",
			typeof( Type ),
			typeof( ValueInput ),
			DependencyPropertyUtilities.CreateMetadata( null, new PropertyChangedCallback( OnValueTypeChanged ) ) 
		);

		/// <summary>
		/// Gets or sets the type of values that this editor manipulates.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>ValueType</b> specifies the type of values that this editor manipulates. The 
		/// Value property will return objects of this type. Also the user input will be
		/// validated accordingly as well. That is when the user enters some text, the text
		/// will be parsed into an object of this type. If parsing fails then the input
		/// is considered invalid. If parsing succeeds, the <see cref="Value"/> property is
		/// updated with the parsed value. 
		/// </para>
		/// <para class="body">
		/// You can use the <see cref="IsValueValid"/> property to find out if the current 
		/// input is valid. For the input to be considered valid, the editor must be able to
		/// parse the input text into an object of <i>ValueType</i> and it must satisfy
		/// any constraints specified via this <see cref="ValueInput.ValueConstraint"/> property.
		/// </para>
		/// <seealso cref="ValueInput.Value"/>
		/// <seealso cref="ValueInput.Text"/>
		/// <seealso cref="ValueInput.ValueConstraint"/>
		/// <seealso cref="ValueInput.IsValueValid"/>
		/// <seealso cref="ValueInput.HasValueChanged"/>
		/// <seealso cref="ValueInput.ValueToTextConverter"/>
		/// <seealso cref="TextInputBase.ValueToDisplayTextConverter"/>
		/// <seealso cref="ValueTypeResolved"/>
		/// </remarks>
		//[Description( "Gets/sets the type of values the editor manipulates" )]
		//[Category( "Data" )]
		[Browsable(false)]
		public Type ValueType
		{
			get
			{
				return (Type)this.GetValue( ValueTypeProperty );
			}
			set
			{
				this.SetValue( ValueTypeProperty, value );
			}
		}

		private static void OnValueTypeChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			ValueInput editor = (ValueInput)dependencyObject;

			editor.ResolveValueType();

			editor.ProcessPropertyChanged( e );
		}

		/// <summary>
		/// Returns true if the ValueType property is set to a non-default value.
		/// </summary>
		public bool ShouldSerializeValueType( )
		{
			return DependencyPropertyUtilities.ShouldSerialize( this, ValueTypeProperty );
		}

		/// <summary>
		/// Resets the ValueType property to its default value.
		/// </summary>
		public void ResetValueType( )
		{
			this.ClearValue( ValueTypeProperty );
		}

				#endregion // ValueType

				#region ValueTypeName

		/// <summary>
		/// Identifies the <see cref="ValueTypeName"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ValueTypeNameProperty = DependencyPropertyUtilities.Register("ValueTypeName",
			typeof(string), typeof(ValueInput),
			DependencyPropertyUtilities.CreateMetadata(null, new PropertyChangedCallback(OnValueTypeNameChanged))
			);

		private static void OnValueTypeNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ValueInput instance = (ValueInput)d;

			instance.ResolveValueType();

			instance.ProcessPropertyChanged( e );
		}

		/// <summary>
		/// Gets or sets the name of the type of values that this editor manipulates.
		/// </summary>
		/// <remarks>
		/// <para class="body">This property is exposed so that the type can be specified in xaml such that the same string will result in the appropriate type being resolved in <see cref="ValueTypeResolved"/> in both SL and WPF. 
		/// In addition to being able to resolve fully qualified type names this property supports short names for the common types, e.g. 'DateTime', 'Int32', as well as their corresponding nullable types, e.g. 'DateTime?', 'Int32?', 'Double?' etc.</para>
		/// <para class="note"><b>Note:</b> this property is ignored if <see cref="ValueType"/> is specified</para>
		/// </remarks>
		/// <seealso cref="ValueTypeNameProperty"/>
		/// <seealso cref="ValueType"/>
		/// <seealso cref="ValueTypeResolved"/>
		public string ValueTypeName
		{
			get
			{
				return (string)this.GetValue(ValueInput.ValueTypeNameProperty);
			}
			set
			{
				this.SetValue(ValueInput.ValueTypeNameProperty, value);
			}
		}

				#endregion //ValueTypeName

				#region ValueTypeResolved

		private static readonly DependencyPropertyKey ValueTypeResolvedPropertyKey = DependencyPropertyUtilities.RegisterReadOnly(
			"ValueTypeResolved",
			typeof( Type ),
			typeof( ValueInput ),
			null,
			new PropertyChangedCallback( OnValueTypeResolvedChanged )
		);

		/// <summary>
		/// Identifies the read-only <see cref="ValueTypeResolved"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ValueTypeResolvedProperty = ValueTypeResolvedPropertyKey.DependencyProperty;

		private static void OnValueTypeResolvedChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			ValueInput editor = (ValueInput)d;

			editor._cachedValueTypeResolved = e.NewValue as Type;
			editor.OnValueTypeResolvedChanged( editor._cachedValueTypeResolved );

			editor.ProcessPropertyChanged( e );
		}

		/// <summary>
		/// Called when the value of <b>ValueType</b> property changes.
		/// </summary>
		/// <param name="newType"></param>
		protected virtual void OnValueTypeResolvedChanged( Type newType )
		{
			// SSP 10/5/11 TFS90162
			// If ValueType is changed, make sure the Value property's value is of that type.
			// 
			if ( this.InternalIsInitialized )
			{
				object value = this.Value;
				if ( null != value )
					this.CoerceValuePropertyHelper( ValueProperty );
			}
		}

		/// <summary>
		/// Gets the resolved value type.
		/// </summary>
		/// <remarks>
		/// <para class="note"><b>Note:</b> the resolution order is as follows. if the <see cref="ValueType"/> property is set it takes precedence. Otherwise, if the <see cref="ValueTypeName"/> is specified an attempt is made to resolve the type. Finally the default type for the editor is used.</para>
		/// </remarks>
		/// <seealso cref="ValueType"/>
		/// <seealso cref="ValueTypeName"/>
		/// <seealso cref="ValueTypeResolvedProperty"/>
		public Type ValueTypeResolved
		{
			get
			{
				// Optimization - use the locally cached property 
				return this._cachedValueTypeResolved;
			}
			internal set
			{
				this.SetValue( ValueTypeResolvedPropertyKey, value );
			}
		}

				#endregion // ValueTypeResolved

				#region ValueConstraint

		/// <summary>
		/// Identifies the <see cref="ValueConstraint"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty ValueConstraintProperty = DependencyPropertyUtilities.Register(
			"ValueConstraint",
			typeof( ValueConstraint ),
			typeof( ValueInput ),
			DependencyPropertyUtilities.CreateMetadata( null, new PropertyChangedCallback( OnValueConstraintChanged ) ) 
		);

		// JJD 4/27/07
		// Optimization - cache the property locally
		private ValueConstraint _cachedValueConstraint;

		
		
		
		
		
		
		
		
#region Infragistics Source Cleanup (Region)











#endregion // Infragistics Source Cleanup (Region)


		
		
		
		
		
		private static void OnValueConstraintChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			ValueInput editor = (ValueInput)dependencyObject;
			ValueConstraint newVal = (ValueConstraint)e.NewValue;

			
			
			
			editor._cachedValueConstraint = newVal;
			PropertyChangeListenerList.ManageListenerHelper( ref editor._cachedValueConstraint, newVal, editor, true );

			editor.ProcessPropertyChanged( e );

			// SSP 5/11/12 TFS111240
			// 
			editor.OnValueConstraintChanged( null );
		}

		// SSP 3/24/10 TFS27839
		// 
		//private void OnValueConstraint_SubPropChanged( object sender, PropertyChangedEventArgs e )
		//{
		//    this.OnValueConstraintChanged( e.PropertyName );
		//}

		
		
		
		/// <summary>
		/// This method is called whenever the ValueConstraint or one of its properties changes.
		/// </summary>
		/// <param name="valueConstraintPropertyName">Null if the ValueConstraint itself changed or 
		/// the name of the property that changed.</param>
		internal virtual void OnValueConstraintChanged( string valueConstraintPropertyName )
		{
			this.RevalidateCurrentValue( );
		}

		/// <summary>
		/// Gets or sets the constraints for editor input. Default value is <b>null</b>.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>Note</b> that the default value of this property is <b>null</b>. You must set
		/// the property to an instance of <see cref="ValueConstraint"/> object.
		/// </para>
		/// <para class="body">
		/// <b>ValueConstraint</b> is used to limit what the user can input into the editor.
		/// More accurately, the editor will consider user input invalid if it doesn't
		/// satisfy one or more criteria specified by the <i>ValueConstraint</i>. The 
		/// <see cref="IsValueValid"/> property can be used to find out if editor considers
		/// current value valid.
		/// </para>
		/// <para class="body">
		/// When an invalid value is entered into the editor, there are behavioral implications
		/// based on various settings. For example, by default the editor doesn't exit edit mode
		/// when the current input is invalid. The action taken by the editor can be controlled
		/// using the <see cref="InvalidValueBehavior"/> property.
		/// </para>
		/// <seealso cref="ValueType"/>
		/// <seealso cref="Value"/>
		/// <seealso cref="IsValueValid"/>
		/// <seealso cref="InvalidValueBehavior"/>
		/// <seealso cref="HasValueChanged"/>
		/// </remarks>
		//[Description( "Gets/sets the editor input constraints" )]
		//[Category( "Data" )]
		public ValueConstraint ValueConstraint
		{
			get
			{
				// JJD 4/27/07
				// Optimization - use the locally cached property 
				//return (ValueConstraint)this.GetValue( ValueConstraintProperty );
				return this._cachedValueConstraint;
			}
			set
			{
				this.SetValue( ValueConstraintProperty, value );
			}
		}

		/// <summary>
		/// Returns true if the ValueConstraint property is set to a non-default value.
		/// </summary>
		public bool ShouldSerializeValueConstraint( )
		{
			return null != this.ValueConstraint;
		}

		/// <summary>
		/// Resets the ValueConstraint property to its default value of <b>null</b>.
		/// </summary>
		public void ResetValueConstraint( )
		{
			this.ClearValue(ValueConstraintProperty);
		}

				#endregion // ValueConstraint

				#region InvalidValueBehavior


		/// <summary>
		/// Identifies the <see cref="InvalidValueBehavior"/> dependency property
		/// </summary>
		public static readonly DependencyProperty InvalidValueBehaviorProperty = DependencyPropertyUtilities.Register(
			"InvalidValueBehavior",
			typeof( InvalidValueBehavior ),
			typeof( ValueInput ),
			DependencyPropertyUtilities.CreateMetadata( InvalidValueBehavior.Default,
				
				
				
				
				
				
				
				
				new PropertyChangedCallback( OnInvalidValueBehaviorChanged )
			) );


        
		
		
		
		
		
		
		private InvalidValueBehavior _cachedInvalidValueBehavior;

		
		
		
		
		
		
		
		
#region Infragistics Source Cleanup (Region)











#endregion // Infragistics Source Cleanup (Region)


		
		
		
		
		
		private static void OnInvalidValueBehaviorChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			ValueInput editor = (ValueInput)dependencyObject;
			InvalidValueBehavior newVal = (InvalidValueBehavior)e.NewValue;

			editor._cachedInvalidValueBehavior = newVal;

			editor.ProcessPropertyChanged( e );
		}

		/// <summary>
		/// Specifies what action to take when the user attempts to leave the editor with an invalid value.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>InvalidValueBehavior</b> specifies what action to take when the user tries to leave
		/// the editor after entering an invalid value.
		/// </para>
		/// <para class="body">
		/// There are various ways a value in the editor can be considered invalid by the editor.
		/// If the entered text can not be parsed into an object of type specified by 
		/// <see cref="ValueInput.ValueType"/> property, then the value is considered invalid. For
		/// example, if the ValueType is set to <i>Int32</i> or any other numeric type and the user 
		/// enteres a non-numeric text then the text can not be parsed into the value type. As a result
		/// the editor will consider the input invalid.
		/// </para>
		/// <para class="body">
		/// Another way the value can be considered invalid is if the entered value can not satisfy
		/// constraints specified by <see cref="ValueInput.ValueConstraint"/> object. For example, if
		/// <see cref="Infragistics.Controls.Editors.ValueConstraint.MinInclusive"/> is specified as 10 and the value entered is 8
		/// then the value does not satisfy the constraints and thus will be considred invalid.
		/// </para>
		/// <seealso cref="ValueInput.ValueType"/>
		/// <seealso cref="ValueInput.ValueConstraint"/>
		/// <seealso cref="Infragistics.Controls.Editors.ValueConstraint.Nullable"/>
		/// </remarks>
		//[Description( "Determines what action to take when attempting to leave the editor with an invalid value." )]
		//[Category( "Behavior" )]
		[Bindable( true )]
		public InvalidValueBehavior InvalidValueBehavior
		{
			get
			{
				// JJD 4/27/07
				// Optimization - use the locally cached property 
				//return (InvalidValueBehavior)this.GetValue(InvalidValueBehaviorProperty);
				return this._cachedInvalidValueBehavior;
			}
			set
			{
				this.SetValue( InvalidValueBehaviorProperty, value );
			}
		}

		/// <summary>
		/// Returns true if the InvalidValueBehavior property is set to a non-default value.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public bool ShouldSerializeInvalidValueBehavior( )
		{
			return DependencyPropertyUtilities.ShouldSerialize( this, InvalidValueBehaviorProperty );
		}

		/// <summary>
		/// Resets the InvalidValueBehavior property to its default state.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public void ResetInvalidValueBehavior( )
		{
			this.ClearValue( InvalidValueBehaviorProperty );
		}

				#endregion // InvalidValueBehavior

				#region ValueToTextConverter

		/// <summary>
		/// Identifies the <see cref="ValueToTextConverter"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty ValueToTextConverterProperty = DependencyPropertyUtilities.Register(
  			"ValueToTextConverter",
			typeof( IValueConverter ),
			typeof( ValueInput ),
			null,
			ValueToTextConverterChanged
		);

		private static void ValueToTextConverterChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			ValueInput input = (ValueInput)d;
			input.ProcessPropertyChanged( e );
		}

		/// <summary>
		/// Specifies the converter used for converting between text and value.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// The conversions between the 'string' and the <b>ValueType</b> by default are done using 
		/// built in conversion logic. You can override the conversion logic by setting the
		/// <b>ValueToDisplayTextConverter</b> and <see cref="ValueToTextConverter"/>. 
		/// <b>ValueToTextConverter</b> is used when the editor is in edit mode where as 
		/// <b>ValueToDisplayTextConverter</b> is used when the editor is not in edit mode.
		/// </para>
		/// <para class="body">
		/// Note: An editor can edit values of types other than 'string'. For example, a <i>XamTextEditor</i> 
		/// can edit values of types <i>DateTime</i>. You can specify the type of values being edited 
		/// by the editor using the <see cref="ValueType"/> property.
		/// </para>
		/// <para class="body">
		/// Although the built-in default conversion logic should be sufficient for
		/// most situations, you may want make use of this functionality to provide
		/// custom logic for converting user input into value type object. Examples
		/// where this would be needed are if you are editing custom objects where the
		/// built-in conversion logic would not know how to convert text into custom
		/// object type. Or you want to support entering certain symbols in the text
		/// that signify certain aspect of the value - for example you want 'k' in '2k'
		/// to be interpreted as 1000 magnitude, or +1d to be interpreted as tomorrow's
		/// date when editing DateTime.
		/// </para>
		/// <seealso cref="TextInputBase.ValueToDisplayTextConverter"/>
		/// <seealso cref="ValueInput.FormatProvider"/>
		/// <seealso cref="ValueInput.Format"/>
		/// <seealso cref="ValueInput.ValueType"/>
		/// <seealso cref="ValueInput.Value"/>
		/// <seealso cref="ValueInput.Text"/>
		/// <seealso cref="TextInputBase.DisplayText"/>
		/// </remarks>
		//[Description( "Specifies the converter for converting between text and value" )]
		//[Category( "Data" )]
		public IValueConverter ValueToTextConverter
		{
			get
			{
				return (IValueConverter)this.GetValue( ValueToTextConverterProperty );
			}
			set
			{
				this.SetValue( ValueToTextConverterProperty, value );
			}
		}

				#endregion // ValueToTextConverter

				#region IsValueValid

		private static readonly DependencyPropertyKey IsValueValidPropertyKey = DependencyPropertyUtilities.RegisterReadOnly(
  			"IsValueValid",
			typeof( bool ),
			typeof( ValueInput ),
			KnownBoxes.TrueBox,
            new PropertyChangedCallback( OnVisualStatePropertyChanged )
		);

		/// <summary>
		/// Identifies the Read-Only <see cref="IsValueValid"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty IsValueValidProperty = IsValueValidPropertyKey.DependencyProperty;

		/// <summary>
		/// Specifies whether the current value of the editor is valid.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// This property can be used to find out if the current value of the editor is valid.
		/// Value is considered valid if it can be coerced into a <see cref="ValueType"/> object
		/// and it satisfies constraints specified by <see cref="ValueConstraint"/> object.
		/// </para>
		/// <para class="body">
		/// When the user input can not be parsed into an object of type <i>ValueType</i>, the
		/// <see cref="Value"/> property will return the last valid value. However the <see cref="Text"/>
		/// property will return the user input.
		/// </para>
		/// <seealso cref="Value"/>
		/// <seealso cref="Text"/>
		/// <seealso cref="ValueType"/>
		/// <seealso cref="ValueConstraint"/>
		/// </remarks>
		//[Description( "Specifies whether current value of the editor is valid" )]
		//[Category( "Behavior" )]
		public bool IsValueValid
		{
			get
			{
				return (bool)this.GetValue( IsValueValidProperty );
			}
		}

		// SSP 4/17/09 NAS9.2 IDataErrorInfo Support
		// 
		internal void SetIsValueValid( bool isValueValid, Exception error )
		{
			Debug.Assert( isValueValid || null != error );

			this.SetIsValueValid( isValueValid,
				null != error ? new ValidationErrorInfo( error ) : null );
		}

		// SSP 4/17/09 NAS9.2 IDataErrorInfo Support
		// 
		internal void SetIsValueValid( bool isValueValid, ValidationErrorInfo invalidValueErrorInfo )
		{
			this.SetValue( IsValueValidPropertyKey, KnownBoxes.FromValue( isValueValid ) );
			this.SetValue( InvalidValueErrorInfoPropertyKey, invalidValueErrorInfo );
		}

				#endregion // IsValueValid

				#region HasValueChanged

		private static readonly DependencyPropertyKey HasValueChangedPropertyKey = DependencyPropertyUtilities.RegisterReadOnly(
			"HasValueChanged",
			typeof( bool ),
			typeof( ValueInput ),
			KnownBoxes.FalseBox,
			null
		);

		/// <summary>
		/// Identifies the Read-Only <see cref="HasValueChanged"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty HasValueChangedProperty = HasValueChangedPropertyKey.DependencyProperty;

		/// <summary>
		/// Determines if the content has changed while in edit mode (read-only).
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// Note that this property is reset to False when the editor leaves edit mode.
		/// It's meant to be valid while the editor is in edit mode. When not in edit mode,
		/// this property will return False.
		/// </para>
		/// </remarks>
		/// <seealso cref="ValueChanged"/>
		/// <seealso cref="OriginalValue"/>
		/// <seealso cref="Value"/>
		[Browsable( false )]
		[Bindable( false )]
		[ReadOnly( true )]

		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]

		public bool HasValueChanged
		{
			get
			{
				return (bool)this.GetValue( HasValueChangedProperty );
			}
		}

				#endregion //HasValueChanged	

				#region InvalidValueErrorInfo

		// SSP 4/17/09 NAS9.2 IDataErrorInfo Support
		// 
		/// <summary>
		/// Identifies the property key for read-only <see cref="InvalidValueErrorInfo"/> dependency property.
		/// </summary>
		private static readonly DependencyPropertyKey InvalidValueErrorInfoPropertyKey = DependencyPropertyUtilities.RegisterReadOnly(
			"InvalidValueErrorInfo",
			typeof( ValidationErrorInfo ),
			typeof( ValueInput ),
			null,
			null
		);

		/// <summary>
		/// Identifies the read-only <see cref="InvalidValueErrorInfo"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty InvalidValueErrorInfoProperty = InvalidValueErrorInfoPropertyKey.DependencyProperty;

		/// <summary>
		/// If the editor's value is invalid, returns error information regarding why it's invalid.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// When the editor's value is invalid, <see cref="ValueInput.IsValueValid"/> property returns
		/// false. To get the error information regarding why the value is invalid, use the 
		/// <see cref="ValueInput.InvalidValueErrorInfo"/> property which returns an instance 
		/// <see cref="ValidationErrorInfo"/> class.
		/// </para>
		/// </remarks>
		/// <seealso cref="IsValueValid"/>
		/// <seealso cref="ValidationErrorInfo"/>
		//[Description( "Error information regarding why the editor's value is invalid." )]
		//[Category( "Data" )]
		[Bindable( true )]
		[Browsable( false )]
		[ReadOnly( true )]

		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]

		public ValidationErrorInfo InvalidValueErrorInfo
		{
			get
			{
				return (ValidationErrorInfo)this.GetValue( InvalidValueErrorInfoProperty );
			}
		}

				#endregion // InvalidValueErrorInfo

				#region IsNullable

		/// <summary>
		/// Indicates whether the user can delete all the contents of the value editor. Default is <b>true</b>.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>IsNullable</b> indicates to the editor that null editor value (empty contents)
		/// is considered a valid input. Invalid inputs are handled by <see cref="InvalidValueBehavior"/>
		/// property settings.
		/// </para>
		/// <seealso cref="TextInputBase.NullText"/>
		/// </remarks>
		//[Description( "Specifies whether null values are allowed." )]
		//[Category( "Data" )]
		internal bool IsNullable
		{
			get
			{
				ValueConstraint vc = this.ValueConstraint;
				if ( null != vc && vc.Nullable.HasValue )
					return vc.Nullable.Value;

				return true;
			}
		}

				#endregion // IsNullable

				#region IsReadOnly

		
		
		

		/// <summary>
		/// Identifies the <see cref="IsReadOnly"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty IsReadOnlyProperty = DependencyPropertyUtilities.Register(
			"IsReadOnly",
			typeof( bool ),
			typeof( ValueInput ),
			DependencyPropertyUtilities.CreateMetadata( KnownBoxes.FalseBox, new PropertyChangedCallback( OnIsReadOnlyChanged ) )
		);


		private static void OnIsReadOnlyChanged( DependencyObject target, DependencyPropertyChangedEventArgs e )
		{
			ValueInput editor = (ValueInput)target;

            // JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
            editor.UpdateVisualStates();

			editor.OnIsReadOnlyChanged();

			editor.ProcessPropertyChanged( e );
		}

		/// <summary>
		/// Called when the <see cref="IsReadOnly"/> property has been changed
		/// </summary>
		protected virtual void OnIsReadOnlyChanged()
		{
		}

		/// <summary>
		/// Specifies whether the user is allowed to modify the value in the editor. Default value is <b>false</b>.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// If <b>IsReadOnly</b> is set to <b>True</b> the user will not be allowed to modify the
		/// value of the editor. However note that you will still be able to modify the value of 
		/// the editor in code via for example its <see cref="Value"/> property.
		/// The default value of this property is <b>False</b>.
		/// </para>
		/// </remarks>
		//[Description( "Specifies whether the user is allowed to modify the value" )]
		//[Category( "Behavior" )]
		public bool IsReadOnly
		{
			get
			{
				return (bool)this.GetValue( IsReadOnlyProperty );
			}
			set
			{
				this.SetValue( IsReadOnlyProperty, value );
			}
		}

		/// <summary>
		/// Returns true if the IsReadOnly property is set to a non-default value.
		/// </summary>
		public bool ShouldSerializeIsReadOnly( )
		{
			return DependencyPropertyUtilities.ShouldSerialize( this, IsReadOnlyProperty );
		}

		/// <summary>
		/// Resets the IsReadOnly property to its default value of <b>false</b>.
		/// </summary>
		public void ResetIsReadOnly( )
		{
			this.ClearValue( IsReadOnlyProperty );
		}

				#endregion // IsReadOnly

				#region OriginalValue

		/// <summary>
		/// Identifies the key for the <see cref="OriginalValue"/> dependency property
		/// </summary>
		private static readonly DependencyPropertyKey OriginalValuePropertyKey = DependencyPropertyUtilities.RegisterReadOnly(
			"OriginalValue", typeof(object), typeof(ValueInput), null, null
		);

		/// <summary>
		/// Identifies the <see cref="OriginalValue"/> dependency property
		/// </summary>
		public static readonly DependencyProperty OriginalValueProperty =
			OriginalValuePropertyKey.DependencyProperty;

		/// <summary>
		/// Gets the original value while in edit mode (read-only)
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// The <b>OriginalValue</b> property keeps track of the original value when the user 
		/// enters the editor. This is used to revert back to the original value if the user 
		/// decides to cancel the edit operation.
		/// </para>
		/// </remarks>
		/// <seealso cref="OriginalValueProperty"/>
		[Browsable(false)]
		[Bindable(true)]
		[ReadOnly(true)]
		public object OriginalValue
		{
			get
			{
				return (object)this.GetValue(ValueInput.OriginalValueProperty);
			}
		}

				#endregion //OriginalValue

				#region FormatProvider

		/// <summary>
		/// Identifies the <see cref="FormatProvider"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty FormatProviderProperty = DependencyPropertyUtilities.Register(
			"FormatProvider",
			typeof( IFormatProvider ),
			typeof( ValueInput ),
			DependencyPropertyUtilities.CreateMetadata(null, 
				
				
				
				
				
				
				
				
				
				
				
				new PropertyChangedCallback( OnFormatProviderChanged )
				)
			);

		// JJD 4/27/07
		// Optimization - cache the property locally
		private IFormatProvider _cachedFormatProvider;

		
		
		
		
		
		private static void OnFormatProviderChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			ValueInput editor = (ValueInput)dependencyObject;
			IFormatProvider newVal = (IFormatProvider)e.NewValue;

			editor._cachedFormatProvider = newVal;

			// SSP 6/2/09 TFS17233
			// Added caching for FormatProviderResolved property.
			// 
			editor._cachedFormatProviderResolved = null;

			editor.ProcessPropertyChanged( e );
		}

		/// <summary>
		/// Specifies format provider used for converting between value and text.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>FormatProvider</b> is used to convert between text and the value. The editor
		/// will use this along with <see cref="Format"/> property setting to convert the value 
		/// to text for display purposes. Note that when editing, only the <i>FormatProvider</i> will 
		/// be used and the <i>Format</i> property will be ignored. This is to facilitate easier
		/// editing of values without the clutter of formatting symbols.
		/// </para>
		/// <para class="body">
		/// The default behavior can be changed by providing custom conversion logic using 
		/// <see cref="ValueInput.ValueToTextConverter"/> and <see cref="TextInputBase.ValueToDisplayTextConverter"/>
		/// properties.
		/// </para>
		/// <para class="body">
		/// <b>Note:</b> <b>FormatProvider</b> property is of type <b>IFormatProvider</b> interface. IFormatProvider 
		/// is implemented by <b>CultureInfo</b> object therefore this property can be set to an instance of 
		/// <b>CultureInfo</b>. You can also use <b>DateTimeFormatInfo</b> or <b>NumberFormatInfo</b> as these 
		/// implement the interface as well.
		/// </para>
		/// <seealso cref="ValueInput.ValueToTextConverter"/> 
		/// <seealso cref="TextInputBase.ValueToDisplayTextConverter"/>
		/// <seealso cref="ValueInput.Format"/>
		/// </remarks>
		//[Description( "Specifies format provider used for converting between value and text" )]
		//[Category( "Behavior" )]
		public IFormatProvider FormatProvider
		{
			get
			{
				// JJD 4/27/07
				// Optimization - use the locally cached property 
				//return (IFormatProvider)this.GetValue( FormatProviderProperty );
				return this._cachedFormatProvider;
			}
			set
			{
				this.SetValue( FormatProviderProperty, value );
			}
		}

		/// <summary>
		/// Returns true if the FormatProvider property is set to a non-default value.
		/// </summary>
		public bool ShouldSerializeFormatProvider( )
		{
			return DependencyPropertyUtilities.ShouldSerialize( this, FormatProviderProperty );
		}

		/// <summary>
		/// Resets the FormatProvider property to its default value of <b>null</b>.
		/// </summary>
		public void ResetFormatProvider( )
		{
			this.ClearValue(FormatProviderProperty);
		}

				#endregion // FormatProvider

				#region Format

		/// <summary>
		/// Identifies the <see cref="Format"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty FormatProperty = DependencyPropertyUtilities.Register(
			"Format",
			typeof( string ),
			typeof( ValueInput ),
			DependencyPropertyUtilities.CreateMetadata( null, new PropertyChangedCallback( OnFormatChanged ) )
		);

		// JJD 4/27/07
		// Optimization - cache the property locally
		private string _cachedFormat;

		
		
		
		
		
		private static void OnFormatChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			ValueInput editor = (ValueInput)dependencyObject;
			string newVal = (string)e.NewValue;

			editor._cachedFormat = newVal;

			editor.ProcessPropertyChanged( e );
		}

		/// <summary>
		/// Specifies format used for converting between value and text.
		/// </summary>
		/// <remarks>
		/// See <see cref="FormatProvider"/> property for more information.
		/// </remarks>
		//[Description( "Specifies format used for converting between value and text" )]
		//[Category( "Behavior" )]
		public string Format
		{
			get
			{
				// JJD 4/27/07
				// Optimization - use the locally cached property 
				//return (string)this.GetValue( FormatProperty );
				return this._cachedFormat;
			}
			set
			{
				this.SetValue( FormatProperty, value );
			}
		}

		/// <summary>
		/// Returns true if the Format property is set to a non-default value.
		/// </summary>
		public bool ShouldSerializeFormat( )
		{
			string format = this.Format;
			return null != format && format.Length > 0;
		}

		/// <summary>
		/// Resets the Format property to its default value of <b>null</b>.
		/// </summary>
		public void ResetFormat( )
		{
			this.ClearValue(FormatProperty);
		}

				#endregion // Format

		#endregion //Public Properties

		#region Protected Properties

				#region DefaultValueToTextConverter

		/// <summary>
		/// Returns the default converter used for converting between the value and the text.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// DefaultValueToTextConverter returns a value converter that provides the default
		/// logic for converting between value and text. Derived editor classes can override
		/// this property to return editor specific conversion logic. If you want to provide
		/// custom conversion logic, use the <see cref="ValueInput.ValueToTextConverter"/>
		/// and <see cref="TextInputBase.ValueToDisplayTextConverter"/> properties.
		/// </para>
		/// </remarks>
		protected virtual IValueConverter DefaultValueToTextConverter
		{
			get
			{
				return ValueInputDefaultConverter.ValueToTextConverter;
			}
		}

				#endregion // DefaultValueToTextConverter

				#region DefaultValueType

		/// <summary>
		/// Returns the default value type of the editor. When the <see cref="ValueType"/> property is not set, this is
		/// the type that the <see cref="ValueTypeResolved"/> will return.
		/// </summary>
		protected virtual Type DefaultValueType
		{
			get
			{
				return typeof( string );
			}
		} 

				#endregion // DefaultValueType

				#region FocusSite

		/// <summary>
		/// Returns the element that is to revieve keyboard focus when the editor is focused.
		/// </summary>
		/// <seealso cref="OnFocusSiteChanged"/>
		protected DependencyObject FocusSite { get { return this._focusSite; } }

				#endregion //FocusSite

                // JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
                // Moved HasDropDown and HasOpenDropDown properties up from TextInputBase class 
                // AS 9/10/08 NA 2008 Vol 2
                #region HasDropDown
                internal virtual bool HasDropDown
                {
                    get { return false; }
                }
                #endregion //HasDropDown

                // JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
                // Moved HasDropDown and HasOpenDropDown properties up from TextInputBase class 
                // AS 9/10/08 NA 2008 Vol 2
                #region HasOpenDropDown
                internal virtual bool HasOpenDropDown
                {
                    get
                    {
                        return false;
                    }
                }
                #endregion //HasOpenDropDown

		#endregion //Protected Properties

		#endregion //Properties

		#region Methods

				#region Static

				#region RegisterResources

        /// <summary>
        /// Adds an additonal Resx file in which the control will pull its resources from.
        /// </summary>
        /// <param name="name">The name of the embedded resx file that contains the resources to be used.</param>
        /// <param name="assembly">The assembly in which the resx file is embedded.</param>
        /// <remarks>Don't include the extension of the file, but prefix it with the default Namespace of the assembly.</remarks>
        public static void RegisterResources(string name, System.Reflection.Assembly assembly)
        {
#pragma warning disable 436
            SR.AddResource(name, assembly);
#pragma warning restore 436
        }

				#endregion // RegisterResources

				#region UnregisterResources

        /// <summary>
        /// Removes a previously registered resx file.
        /// </summary>
        /// <param name="name">The name of the embedded resx file that was used for registration.</param>
        /// <remarks>
        /// Note: this won't have any effect on controls that are already in view and are already displaying strings.
        /// It will only affect any new controls created.
        /// </remarks>
        public static void UnregisterResources(string name)
        {
#pragma warning disable 436
            SR.RemoveResource(name);
#pragma warning restore 436
        }

				#endregion // UnregisterResources

				#endregion // Static

				#region Public Methods

				#region CanEditType

				/// <summary>
		/// Determines if the editor natively supports editing values of specified type.
		/// </summary>
		/// <param name="type">The data type to check.</param>
		/// <returns>Returns True if the editor natively supports editing values of specified type, False otherwise.</returns>
		/// <remarks>
		/// <p class="body">
		/// CanEditType method indicates if the editor natively supports editing values
		/// of specified type. Typically there is no need to call this method. It's used
		/// by the value editor infrastructure.
		/// </p>
		/// <p class="body">
		/// <b>Note:</b> CanEditType does not indicate what types can be set on the 
		/// <see cref="ValueInput.ValueType"/> property. ValueType property can be set to
		/// any type as long as there is conversion logic for converting between the native
		/// data type of the editor and that type. For example, <see cref="XamMaskedInput"/>
		/// natively supports editing string type only. However its ValueType can be set to
		/// Double or DateTime or any type as long as the editor can convert between string
		/// and that data type. ValueType can even be set to a custom type. You can provide
		/// custom conversion logic using <see cref="ValueInput.ValueToTextConverter"/>
		/// and <see cref="TextInputBase.ValueToDisplayTextConverter"/> properties.
		/// </p>
		/// </remarks>
		public abstract bool CanEditType(Type type);

				#endregion //CanEditType

				#region CanRenderType

		/// <summary>
		/// Determines if the editor natively supports displaying of values of specified type.
		/// </summary>
		/// <param name="type">The data type to check.</param>
		/// <returns>Returns True if the editor natively supports displaying values of specified type, False otherwise.</returns>
		/// <remarks>
		/// <p class="body">
		/// CanRenderType method indicates if the editor natively supports displaying values
		/// of specified type. Typically there is no need to call this method. It's used
		/// by the value editor infrastructure.
		/// </p>
		/// <p class="body">
		/// See <see cref="CanEditType"/> for more information.
		/// </p>
		/// </remarks>
		public abstract bool CanRenderType(Type type);

				#endregion //CanRenderType
    
				#region IsExtentBasedOnValue

		/// <summary>
		/// Indicates whether the desired width or the height of the editor is based on the value.
		/// </summary>
		/// <param name="orientation">Orientation of the extent being evaluated. Horizontal indicates the width and vertical indicates the height.</param>
		/// <returns>True if the extent is based on the value.</returns>
		/// <remarks>
		/// <p class="body">
		/// This method is used by the Infragistics internal infrastructure to determine if an editor's
		/// desired size changes based on the value of the editor. For example, XamCheckEditor returns
		/// False for both orientation since its width and height both are not related to its value.
		/// Whether the value of a XamCheckEditor is True or Flase, it will be the same width and height.
		/// XamTextEditor on the other hand returns True for the horizontal orientation since since 
		/// the desired size depends on the current value in the editor. It returns True or False
		/// for the vertical dimension based on whether wrapping is turned on or off respectively.
		/// </p>
		/// </remarks>
		[ EditorBrowsable( EditorBrowsableState.Advanced ) ]
		public virtual bool IsExtentBasedOnValue( Orientation orientation )
		{
			return true;
		}

				#endregion // IsExtentBasedOnValue

				#region ValidateCurrentValue

		/// <summary>
		/// Validates the current value of the editor and initializes the <see cref="IsValueValid"/> 
		/// property based on the results of the value validation.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// ValueInput automatically validates the value whenever it changes and therefore typically 
		/// it should not be necessary to call this method. However there may be times when you may
		/// want to force the value to be re-validated, for instance when the external validation
		/// criteria changes. This is especially useful when you are overriding the 
		/// <see cref="ValidateCurrentValue(out Exception)"/> virtual method to provide custom logic
		/// for the value validation. The ValueInput will update the <see cref="IsValueValid"/> 
		/// property to reflect whether the value is valid or not.
		/// </para>
		/// <seealso cref="IsValueValid"/>
		/// </remarks>
		public void ValidateCurrentValue( )
		{
			this.RevalidateCurrentValue( );
		}

				#endregion // ValidateCurrentValue

				#region ValidateValue

		// SSP 5/5/09 - Clipboard Support
		// Added ValidateValue to be used by the clipboard functionality in the data presenter
		// for ensuring text being pasted is not invalid.
		// 
		/// <summary>
		/// Returns true if the specified value would be considered to be valid by the editor
		/// based on implicit as well as explicit value constraints enforced by the editor. 
		/// </summary>
		/// <param name="value">Value to check if it's valid.</param>
		/// <param name="error">If the value is invalid, this is set to an exception containing appropriate error message.</param>
		/// <returns>True if the specified value is valid, False otherwise.</returns>
		/// <remarks>
		/// <para class="body">
		/// <b>ValidateValue</b> method is used to determine if a value is valid or not based
		/// on the implicit as well as explicit value constraints enforced by the editor.
		/// Implicit value constraints are <see cref="ValueInput.ValueType"/>,
		/// <see cref="XamMaskedInput.Mask"/> etc... and explicit constraints are specified
		/// via <see cref="ValueInput.ValueConstraint"/> property.
		/// </para>
		/// <seealso cref="ValueInput.IsValueValid"/>
		/// <seealso cref="ValueInput.InvalidValueBehavior"/>
		/// </remarks>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public virtual bool ValidateValue( object value, out Exception error )
		{
			return this.IsValueValidHelper( value, out error );
		}

				#endregion // ValidateValue

			#endregion //Public Methods

			#region Protected Methods

				#region DoInitialization

		/// <summary>
		/// Called from OnInitialized to provide the derived classes an opportunity to 
		/// perform appropriate initialization tasks. OnInitialized implementation enters
		/// the editor into edit mode at the end if AlwaysInEditMode is true. This method 
		/// is called before that.
		/// </summary>
		protected virtual void DoInitialization( )
		{
			// SSP 1/3/07 BR27394
			// Moved the existing code into the new InitializeValueProperties method.
			// 
			this.InitializeValueProperties( );
		}

				#endregion // DoInitialization

				#region GetFocusSite

		/// <summary>
		/// Gets the focus site of the editor.
		/// </summary>
		/// <returns>FrameworkElement in the template of the editor that is to receive focus.</returns>
		protected abstract FrameworkElement GetFocusSite( ); 

				#endregion // GetFocusSite

				#region InitializeValueProperties

		// SSP 1/3/07 BR27394
		// 
		/// <summary>
		/// Initializes the value properties. This synchronizes all the value properties if one of
		/// them is set in xaml since we delay syncrhonization until after initialization in case
		/// other settings in xaml effect how they are synchronized.
		/// </summary>
		internal virtual void InitializeValueProperties( )
		{
			// We are deferring synchronization of Value and Text properties until the control
			// is initialized. This is because during initialization not all properties (like
			// ValueType or Mask) may have been set when Value or Text gets set.
			// 
			// SSP 1/7/08 BR29457
			// 
			//if ( DependencyProperty.UnsetValue != this.ReadLocalValue( ValueProperty ) )
			if ( Utils.IsValuePropertySet( ValueProperty, this ) )
			{
				this.CoerceValuePropertyHelper( ValueProperty );
				this.SyncValueProperties( ValueProperty, this.Value );
			}
			// SSP 1/7/08 BR29457
			// 
			//else if ( DependencyProperty.UnsetValue != this.ReadLocalValue( TextProperty ) )
			else if ( Utils.IsValuePropertySet( TextProperty, this ) )
			{
				this.CoerceValuePropertyHelper( TextProperty );
				this.SyncValueProperties( TextProperty, this.Text );
			}
				// SSP 6/15/09 TFS17552
				// Added the else block. Even if no value is set, we still need to validate the 
				// current value, even if it's null, and initialize IsValueValid property based
				// on it. Added the else block.
				// 
			else
			{
				this.RevalidateCurrentValue( );
			}
		}

				#endregion // InitializeValueProperties

				#region InitializeOriginalValueFromValue

		/// <summary>
		/// Called when starting edit mode to copy the Content propety value into the <see cref="OriginalValue"/> property.
		/// </summary>
		/// <remarks>The default implementation just sets the <see cref="OriginalValue"/> without any coersions.</remarks>
		// SSP 5/9/12 TFS108278
		// Made public.
		// 
		//protected virtual void InitializeOriginalValueFromValue()
		public virtual void InitializeOriginalValueFromValue( )
		{
			this.SetValue(OriginalValuePropertyKey, this.Value);
		}

				#endregion //InitializeOriginalValueFromValue	
    
				#region OnFocusSiteChanged

		/// <summary>
		/// Called when the focus site changes.
		/// </summary>
		/// <seealso cref="ValueInput.FocusSite"/>
		/// <seealso cref="ValueInput.ValidateFocusSite"/>
		protected virtual void OnFocusSiteChanged()
		{
		}

				#endregion //OnFocusSiteChanged	

				// JJD 08/01/12 - TFS118198 - added
				#region OnIsDropDownOpenChanged

		/// <summary>
		/// Called when the dropdown is opened or closed
		/// </summary>
		internal virtual void OnIsDropDownOpenChanged(bool isDropDownOpen)
		{

			// JJD 08/01/12 - TFS118198 
			// If the IsTabStop property was set to false while we are in edit mode
			// we want to reset it back to true before focus is shifted to an
			// element in the dropdown (e.g. XamCalendar).
			// When the dropdown closes up we want to set it back to false
			if (_wasTabStopDisabled)
				this.SetCurrentValue(IsTabStopProperty, KnownBoxes.FromValue(isDropDownOpen));


		}

				#endregion // OnIsDropDownOpenChanged

				#region OnIsInEditModeChanged

		/// <summary>
		/// Called when the focus is moved into or out of this editor.
		/// </summary>
		internal virtual void OnIsInEditModeChanged( bool isInEditMode )
		{
			if (!isInEditMode)
			{

				// JJD 07/17/12 - TFS90155 
				// If we used SetCurrentValue to set the IsTabStop to false below
				// then we need to set it back to true
				if (_wasTabStopDisabled)
				{
					// reset the flag
					_wasTabStopDisabled = false;

					// JJD 07/17/12 - TFS90155 
					// First verify that the local value is still false.
					// If it is thn set it to true.
					object localValue = this.ReadLocalValue(IsTabStopProperty);
					if (localValue is bool && false == (bool)localValue)
						this.SetCurrentValue(IsTabStopProperty, KnownBoxes.TrueBox);
				}


				// If IsAlwaysInEditMode is true then EndEditMode logic won't be executed and therefore
				// we have to validate the input here in OnLostFocus.
				// 
				EditModeValidationErrorEventArgs eventArgs;
				bool stayInEditMode;
				this.ValidateInputHelper(true, out eventArgs, out stayInEditMode);
			}

			if ( isInEditMode && this.IsKeyboardFocusWithin && ( null == _lostFocusTracker || _lostFocusTracker.ElementWithFocus != _focusSite ) )
				this.SetFocusToFocusSite( );

			// JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
			this.UpdateVisualStates( );

			// SSP 9/2/11 TFS84350
			// When we get focus, initialize the OriginalValue from Value.
			// 
			if ( isInEditMode && this.IsValueValid )
				this.InitializeOriginalValueFromValue( );

			// JJD 07/17/12 - TFS90155 
			// In WPF we need to set IsTabStop to false (if it is true) while we are in edit mode.
			// Otherwise, a shift-tab will de-focus the textbox inside our template and
			// focus this control (which will just re-focus the textbox).
			if (isInEditMode && this.IsTabStop)
			{
				this.SetCurrentValue(IsTabStopProperty, KnownBoxes.FalseBox);

				_wasTabStopDisabled = true;
			}

		}

				#endregion // OnIsInEditModeChanged

				#region ProcessKeyDown

		/// <summary>
		/// Processes the key down event. Default implementation does nothing.
		/// This class overrides OnKeyDown and performs some default processing and
		/// then calls this method if further key down processing is to be done.
		/// Derived classes are intended to override this method instead of OnKeyDown.
		/// </summary>
		/// <param name="e">Key event args.</param>
		internal protected virtual void ProcessKeyDown( KeyEventArgs e )
		{
			switch (e.Key)
			{
				// SSP 5/8/12 TFS108278
				// After discussing this with Andrew we decided to remove this functionality for the time
				// being because handling the escape key will interfere with the escape key handling by the
				// XamGrid if the editor is embedded inside it. Furthermore, this bug where the editor is
				// used standalone requires that the escape key be not handled there as well (maybe the
				// customer wants to handle the Escape key or have the framework process it which normally
				// means processing the cancel button if there's any).
				// 
				//case Key.Escape:
				//    this.RevertValueBackToOriginalValue();
				//    e.Handled = true;
				//    break;
			}
		}

				#endregion // ProcessKeyDown
    
				#region RevertValueBackToOriginalValue

		/// <summary>
		/// Called when ending edit mode and not accepting the changes.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// The default implementation just sets the Value to <see cref="OriginalValue"/> without any coersions.
		/// </para>
		/// <para class="body">
		/// See <see cref="OriginalValue"/> for more information on how OriginalValue gets useed.
		/// </para>
		/// <seealso cref="OriginalValue"/>
		/// </remarks>
		// SSP 5/9/12 TFS108278
		// Made public.
		// 
		//protected virtual void RevertValueBackToOriginalValue()
		public virtual void RevertValueBackToOriginalValue( )
		{
			this.Value = this.OriginalValue;

			
			// We need to explicitly make sure that the Text property is synced with the Value
			// in case the Value and OriginalValue are the same but the Text is not reflective
			// of the Value (because the Text was modified to a value that we could not parse
			// to the value type).
			// 
			this.SyncValueProperties( ValueProperty, this.Value );
		}

				#endregion //RevertValueBackToOriginalValue

				#region ValidateFocusSite

		/// <summary>
		/// Validates the focus site. Returns true if the focus site is acceptable.
		/// </summary>
		/// <param name="focusSite">The focus site to validate.</param>
		/// <param name="errorMessage">If the foucs site is invalid then this out parameter will be assigned relevant error message.</param>
		/// <returns>True if the focus site is valid, False otherwise.</returns>
		/// <remarks>
		/// <para class="body">
		/// ValidateFocusSite method is called to ensure the element named PART_FocusSite in the control
		/// template is a valid focus site for the value editor. The default implementation ensures
		/// that the focus site is either a FrameworkElement or FrameworkContentElement. Derived
		/// value editors can override this method to further constraint what the focus site can be.
		/// </para>
		/// <seealso cref="FocusSite"/>
		/// </remarks>
		protected virtual bool ValidateFocusSite( object focusSite, out Exception errorMessage )
		{
			if ( !( focusSite is Control ) )
			{
				errorMessage = new NotSupportedException( "Focus site must be a Control." );
				return false;
			}

			errorMessage = null;
			return true;
		}

				#endregion // ValidateFocusSite

				#region ValidateCurrentValue

		/// <summary>
		/// Validates the current value of the editor. This method is called by the editor to perform
		/// editor specific validation of the current value.
		/// </summary>
		/// <returns>True if the value is valid, False otherwise.</returns>
		/// <remarks>
		/// <para class="body">
		/// <b>ValidateCurrentValue</b> method is called by the editor to determine if the current
		/// value of the editor is valid or not. It uses this method to update the value of 
		/// <see cref="IsValueValid"/> property. The derived editors can override this method to 
		/// perform editor specific validations.
		/// </para>
		/// <para class="body">
		/// If the value entered by the user is invalid then the editor will take action based
		/// on the setting of <see cref="InvalidValueBehavior"/> property when the user attempts
		/// to leave the editor.
		/// </para>
		/// <seealso cref="IsValueValid"/>
		/// <seealso cref="InvalidValueBehavior"/>
		/// </remarks>
		protected virtual bool ValidateCurrentValue( out Exception error )
		{
			return this.IsValueValidHelper( this.Value, out error );
		}

				#endregion // ValidateCurrentValue

                #region VisualState... Methods

        // JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
        /// <summary>
        /// Called to set the VisualStates of the editor
        /// </summary>
        /// <param name="useTransitions">Determines whether transitions should be used.</param>
        protected virtual void SetVisualState(bool useTransitions)
        {

            // Set Common states
            if ( this.IsEnabled == false )
                VisualStateManager.GoToState(this, VisualStateUtilities.StateDisabled, useTransitions);
            else if (this.IsReadOnly || !this.IsEditingAllowed)
            {
                if (this.IsMouseOver)
                    VisualStateUtilities.GoToState(this, useTransitions, VisualStateUtilities.StateReadOnly, VisualStateUtilities.StateMouseOver, VisualStateUtilities.StateNormal);
                else
                    VisualStateUtilities.GoToState(this, useTransitions, VisualStateUtilities.StateReadOnly, VisualStateUtilities.StateNormal);
            }
			else if ( this.IsMouseOver )
                VisualStateUtilities.GoToState(this, useTransitions, VisualStateUtilities.StateMouseOver, VisualStateUtilities.StateNormal);
            else
                VisualStateManager.GoToState(this, VisualStateUtilities.StateNormal, useTransitions);

            bool isFocused = this.IsKeyboardFocusWithin;

            // Set Focus states
            if (isFocused)
            {
                if (this.HasOpenDropDown)
                    VisualStateUtilities.GoToState(this, useTransitions, VisualStateUtilities.StateFocusedDropDown, VisualStateUtilities.StateFocused);
                else
                    VisualStateManager.GoToState(this, VisualStateUtilities.StateFocused, useTransitions);
            }
            else
                VisualStateManager.GoToState(this, VisualStateUtilities.StateUnfocused, useTransitions);

            // Set Validation states
            if ( this.IsValueValid )
                VisualStateManager.GoToState(this, VisualStateUtilities.StateValidEx, useTransitions);
            else if ( isFocused )
                VisualStateManager.GoToState(this, VisualStateUtilities.StateInvalidFocusedEx, useTransitions);
            else
                VisualStateManager.GoToState(this, VisualStateUtilities.StateInvalidUnfocusedEx, useTransitions);

        }

        // JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
        internal static void OnVisualStatePropertyChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            ValueInput editor = target as ValueInput;

            if ( editor != null )
                editor.UpdateVisualStates();
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
                return;



            if (!this.IsLoaded)
                useTransitions = false;


            this.SetVisualState(useTransitions);
        }

                #endregion //VisualState... Methods	

			#endregion //Protected Methods

			#region Internal Methods

				#region BeginSyncValueProperties

		
		
		/// <summary>
		/// Begins synchronization of value properties (Value, Text, DisplayText, SelectedItem etc...).
		/// This method suspends raising of any value property changed notifications since they all need
		/// to be raised after all the value properties are synced. You need to use RaiseValuePropertyChangedEvent
		/// method to raise value property change notification that need to be delayed in this manner.
		/// </summary>
		internal void BeginSyncValueProperties( )
		{
			if ( null == _pendingPropertyChangedEvents )
				_pendingPropertyChangedEvents = new List<IRaiseEventDefinition>( );

			_syncingValueProperties++;
		}

				#endregion // BeginSyncValueProperties

				#region ConsiderIsInEditMode

		/// <summary>
		/// When a context menu is opened or a drop-down is opened that takes the focus away from the editor,
		/// the editor should still be considered logically to be in edit mode or focused. This method is
		/// used to keep track of any such states that cause the editor to be considered in edit mode regardless
		/// of the focus status.
		/// </summary>
		/// <param name="stateName"></param>
		/// <param name="state"></param>
		internal void ConsiderIsInEditMode( string stateName, bool state )
		{
			if ( state )
			{
				if ( null == _considerInEditModeAttributes )
					_considerInEditModeAttributes = new List<string>( );

				if ( ! _considerInEditModeAttributes.Contains( stateName ) )
					_considerInEditModeAttributes.Add( stateName );
			}
			else
			{
				if ( null != _considerInEditModeAttributes )
				{
					_considerInEditModeAttributes.Remove( stateName );

					if ( 0 == _considerInEditModeAttributes.Count )
						_considerInEditModeAttributes = null;
				}
			}

			this.VerifyIsInEditMode( );
		} 

				#endregion // ConsiderIsInEditMode

				#region EndSyncValueProperties

		
		
		/// <summary>
		/// Ends synchronization of value properties and raises any pending value property change 
		/// notifications. See <see cref="BeginSyncValueProperties"/> for more info.
		/// </summary>
		internal void EndSyncValueProperties( )
		{
			_syncingValueProperties--;

			if ( !this.SyncingValueProperties )
			{
				Debug.Assert( null != _pendingPropertyChangedEvents );
				// SSP 10/12/08 TFS8836
				// There's a possibility that the _pendingPropertyChangedEvents could
				// get modified while raising one of its events. Added anti-recursion
				// flag.
				// 
				//if ( null != _pendingPropertyChangedEvents )
				if ( null != _pendingPropertyChangedEvents && !_processingPendingPropertyChangedEvents )
				{
					
					
					_processingPendingPropertyChangedEvents = true;

					try
					{
						// SSP 10/12/08 TFS8836
						// There's a possibility that the _pendingPropertyChangedEvents could
						// get modified while raising one of its events.
						// ------------------------------------------------------------------
						//foreach ( RoutedEventArgs ee in _pendingPropertyChangedEvents )
						//	this.RaiseEvent( ee );
						var list = _pendingPropertyChangedEvents;
						for ( int i = 0; i < list.Count; i++ )
						{
							this.RaiseEventHelper( list[i] );
						}
						// ------------------------------------------------------------------
					}
					finally
					{
						_pendingPropertyChangedEvents.Clear( );
						_pendingPropertyChangedEvents = null;

						
						
						_processingPendingPropertyChangedEvents = false;
					}
				}
			}
		}

				#endregion // EndSyncValueProperties

				#region GetString
		internal static string GetString(string name)
		{
			return GetString(name, null);
		}

		internal static string GetString(string name, params object[] args)
		{
#pragma warning disable 436
			return SR.GetString(name, args);
#pragma warning restore 436
		}
				#endregion // GetString

				#region HasValueChangedInternal

		// SSP 2/10/09 TFS12242
		// Don't validate the value and prompt the user if the user types back in the same value as
		// the original value. Added HasValueChangedInternal method that checks if the current value
		// is the same as the original value.
		// 
		/// <summary>
		/// Gets a value indicating if the value of the editor has changed since entering edit mode.
		/// </summary>
		/// <param name="compareWithOriginalValue">If true then checks if the original value and the
		/// current value are the same and if so returns false.</param>
		/// <returns>True if the value has changed, false otherwise.</returns>
		internal bool HasValueChangedInternal( bool compareWithOriginalValue )
		{
			if ( !this.HasValueChanged )
				return false;

			if ( compareWithOriginalValue )
			{
				object origValue = this.OriginalValue;
				object newValue = this.Value;

				bool origValueEmpty = Utils.IsValueEmpty( origValue );
				bool newValueEmpty = Utils.IsValueEmpty( newValue );

				// If the original value is null and the new value is null or the values are
				// the same then return false.
				// 
				if ( origValueEmpty && newValueEmpty || Utils.AreEqual( origValue, newValue ) )
				{
					// SSP 3/13/09 TFS12242
					// In addition to checking the value and original value, also check 
					// the texts because if the Text cannot be converted to value, then 
					// the Value property doesn't truly depict the current input and 
					// therefore cannot be relied upon to determine if the current value
					// is the same as the original value.
					// 
					// --------------------------------------------------------------------
					//return false;
					Exception error;
					string origValueText;
					this.ConvertValueToText( origValue, out origValueText, out error );

					if ( origValueText == this.Text )
						return false;
					// --------------------------------------------------------------------
				}
			}

			return true;
		}

				#endregion // HasValueChangedInternal

				#region OnProcessPropertyChangedCallback

		internal static void OnProcessPropertyChangedCallback( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			ValueInput editor = (ValueInput)d;
			editor.ProcessPropertyChanged( e );
		} 

				#endregion // OnProcessPropertyChangedCallback

				#region RaiseEventHelper

		// MD 7/16/10 - TFS26592






		internal bool RaiseEventHelper( IRaiseEventDefinition args )
		{
			args.Raise( );
			return true;
		} 

				#endregion // RaiseEventHelper

				#region RaiseValuePropertyChangedEvent

		
		
		/// <summary>
		/// Raises the specified value property change notification. If value property synchronization
		/// is in progress, delays raising of the event until the syncrhonization is complete.
		/// </summary>
		/// <param name="e"></param>
		internal void RaiseValuePropertyChangedEvent( IRaiseEventDefinition e )
		{
			if ( this.SyncingValueProperties )
			{
				Debug.Assert( null != _pendingPropertyChangedEvents );
				if ( null != _pendingPropertyChangedEvents )
				{
					_pendingPropertyChangedEvents.Add( e );
					return;
				}
			}

			// MD 7/16/10 - TFS26592
			// Call off to the new helper method to raise the event. 
			//this.RaiseEvent( e );
			this.RaiseEventHelper(e);
		}

				#endregion // RaiseValuePropertyChangedEvent

				#region SetFocusToFocusSite

		internal void SetFocusToFocusSite()
		{
			// if a focus stite has been specified then set focus to it now
			if (this._focusSite != null)
			{
				Control fe = this._focusSite as Control;




				if ( fe != null )
				{
					// SSP 3/23/09 IME
					// 
					//fe.Focus( );
					fe.MoveFocus( new TraversalRequest( FocusNavigationDirection.First ) );
				}
				else
				{
					FrameworkContentElement fce = this._focusSite as FrameworkContentElement;
					if ( fce != null )
					{
						// SSP 3/23/09 IME
						// 
						//fce.Focus( );
						fce.MoveFocus( new TraversalRequest( FocusNavigationDirection.First ) );
					}
				}

			}
		}

				#endregion //SetFocusToFocusSite	

				#region ValidateInputHelper

		internal void ValidateInputHelper( bool fromLostFocus, out EditModeValidationErrorEventArgs eventArgs, out bool stayInEditMode )
		{
			eventArgs = null;
			stayInEditMode = false;

			if ( !_isInValidateInputHelper 
 				&& ! this.IsValueValid
				// SSP 2/6/09 TFS10586
				// Added AlwaysValidate property. If it's set to true then validate regardless of
				// whether the user has modified the value.
				// 
				// SSP 2/10/09 TFS12242
				// Don't validate the value and prompt the user if the user types back in the same value as
				// the original value. Added HasValueChangedInternal method that checks if the current value
				// is the same as the original value.
				// 
				//&& ( this.HasValueChanged || this.AlwaysValidateResolved ) )
				&& ( this.HasValueChangedInternal( true ) || this.AlwaysValidateResolved ) )
			{
				_isInValidateInputHelper = true;
				try
				{
					// SSP 5/5/09 - IDataErrorInfo Support
					// Use the error information contained in the new InvalidValueErrorInfo property.
					// 
					// ----------------------------------------------------------------------------------
					ValidationErrorInfo errorInfo = this.InvalidValueErrorInfo;
					Debug.Assert( null != errorInfo );
					if ( null == errorInfo )
						errorInfo = new ValidationErrorInfo( new Exception( ValueInput.GetString( "LE_Exception_1" ) ) );

					eventArgs = new EditModeValidationErrorEventArgs( this, errorInfo.Exception, errorInfo.ErrorMessage );
					
#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

					// ----------------------------------------------------------------------------------

					this.RaiseValidationError( eventArgs );

					// SSP 1/20/10 TFS30656
					// Enclosed the existing code in the if block. If the value is changed to a valid value from
					// within the EditModeValidationError event, then don't show the message box or stay in edit 
					// mode since the value of the editor is valid.
					// 
					if ( !this.IsValueValid )
					{
						switch ( eventArgs.InvalidValueBehavior )
						{
							case InvalidValueBehavior.Default:
                            case InvalidValueBehavior.DisplayErrorMessage:

								// AS 7/19/07 BR25005
								// See CanShowErrorMessage for details.
								//
								//if (eventArgs.ErrorMessage != null && eventArgs.ErrorMessage.Length > 0)
								if ( !string.IsNullOrEmpty( eventArgs.ErrorMessage ) &&
									this.CanShowErrorMessage( ) )
								{
									Utils.ShowMessageBox( this,
										eventArgs.ErrorMessage,
										ValueInput.GetString( "LMSG_ValueConstraint_InvalidValue" ),
										MessageBoxButton.OK

										, MessageBoxImage.Stop

									);
								}

								stayInEditMode = true;
								break;
							case InvalidValueBehavior.RetainValue:
								stayInEditMode = true;
								break;
							case InvalidValueBehavior.RevertValue:
								this.RevertValueBackToOriginalValue( );
								break;
							default:
								Debug.Assert( false, "Unknown member." );
								break;
						}
					}
				}
				finally
				{
					_isInValidateInputHelper = false;
				}
			}
		}

				#endregion // ValidateInputHelper

			#endregion //Internal Methods

			#region Private Methods

				// AS 7/19/07 BR25005
				// Now that we are exiting edit mode when the owner (DataPresenter in 
				// this case) loses focus, we need to try and suppress showing a message
				// box when the window is being closed. Otherwise the modal messagebox
				// is shown when the window is no longer visible & painting so you see
				// a black box where the window was.
				//
		#region CanShowErrorMessage
		internal bool CanShowErrorMessage()
		{
			UIElement root = PresentationUtilities.GetRootVisual( this ) as UIElement;

			if ( null == root )
				return true;


			return root.IsVisible;



		} 
		#endregion //CanShowErrorMessage

		#region CoerceValuePropertyHelper

		private void CoerceValuePropertyHelper( DependencyProperty property )
		{
			if ( ValueProperty == property )
			{
				this.CoerceValueHelper( );
			}
		} 

		#endregion // CoerceValuePropertyHelper

		#region InitializeCachedPropertyValues

		
		
		
		
		
		/// <summary>
		/// Initializes the variables used to cache the dependency property values by
		/// getting the dependency property metadata for this object and getting DefaultValue
		/// of that metadata for the respective property.
		/// </summary>
		private void InitializeCachedPropertyValues( )
		{
			_cachedFormat = (string)DependencyPropertyUtilities.GetDefaultValue( this, FormatProperty );
			_cachedFormatProvider = (IFormatProvider)DependencyPropertyUtilities.GetDefaultValue( this, FormatProviderProperty );
			_cachedInvalidValueBehavior = (InvalidValueBehavior)DependencyPropertyUtilities.GetDefaultValue( this, InvalidValueBehaviorProperty );

			_cachedValueConstraint = (ValueConstraint)DependencyPropertyUtilities.GetDefaultValue( this, ValueConstraintProperty );
		}

		#endregion // InitializeCachedPropertyValues

		#region IsValueValidHelper

		private bool IsValueValidHelper( object value, out Exception error )
		{
			string errorMessage = null;
			error = null;
			ValueConstraint vc = this.ValueConstraint;
			if ( null != vc
				&& !vc.Validate( value, this.ValueTypeResolved, ValueConstraintFlags.All,
									this.FormatProviderResolved, this.Format, ref errorMessage ) )
			{
				if ( !String.IsNullOrEmpty( errorMessage ) )
					error = new Exception( errorMessage );

				return false;
			}

			if ( null == value || DBNull.Value == value )
			{
				if ( !this.IsNullable )
					return false;
			}
			else
			{
				if ( !this.ValueTypeResolved.IsInstanceOfType( value ) )
					return false;
			}

			return true;
		}

		#endregion // IsValueValidHelper

		#region ConvertTextToValue

		/// <summary>
		/// Converts the specified text to the value type using the <see cref="ValueToTextConverterResolved"/>.
		/// This method is typically called to convert the text modified by the user back to the ValueType.
		/// </summary>
		/// <param name="text">The text to convert.</param>
		/// <param name="value">This out parameter will be set to the converted value.</param>
		/// <param name="error">If conversion fails, error is set to a value that indicates the error.</param>
		/// <returns>True if conversion succeeds, False otherwise.</returns>
		/// <remarks>
		/// <p class="body">
		/// ConvertTextToValue is used to convert text into an object of type specified by 
		/// <see cref="ValueInput.ValueType"/> property. This method is typically used to 
		/// convert the user input in the form of text to the value that gets returned
		/// from the <see cref="ValueInput.Value"/> property. Value property returns objects 
		/// of type specified by ValueType property.
		/// </p>
		/// <p class="body">
		/// For example, if the ValueType property of a <see cref="XamMaskedInput"/> is set to DateTime type, 
		/// and the user types in "1/1/07", this method will get called to convert that text value
		/// into a DateTime object.
		/// </p>
		/// <p class="body">
		/// <b>Note:</b> Typically there is no need for you to call this method directly as this method is 
		/// automatically called by the ValueInput itself to perform the necessary conversions between text and value.
		/// </p>
		/// <p class="body">
		/// <b>Note:</b> If you want to override the default conversion logic for converting between text and value,
		/// set the <see cref="ValueInput.ValueToTextConverter"/> and <see cref="TextInputBase.ValueToDisplayTextConverter"/>
		/// properties.
		/// </p>
		/// </remarks>
		[EditorBrowsable( EditorBrowsableState.Advanced )]
		public bool ConvertTextToValue( string text, out object value, out Exception error )
		{
			return this.ConvertTextToValueHelper( text, out value, out error );
		}

		// SSP 8/20/08 BR35749
		// Added ConvertTextToValueHelper method. Code in there is moved from the ConvertTextToValue method
		// above.
		// 
		internal virtual bool ConvertTextToValueHelper( string text, out object value, out Exception error )
		{
			error = null;
			value = null;

			try
			{
				value = this.ValueToTextConverterResolved.ConvertBack( text, this.ValueTypeResolved,
					this, this.FormatProviderResolved as System.Globalization.CultureInfo );

				if ( null == value )
				{
					if ( null == text || text.Length == 0 )
					{
						
						
						
						
						value = null;
					}
					else
					{
						// SSP 4/21/09 NAS9.2 IDataErrorInfo Support
						// Set error to an appropriate error message.
						// 
						error = Utils.GetTextToValueConversionError( this.ValueTypeResolved, text );

						return false;
					}
				}
			}
			catch ( Exception e )
			{
				error = e;
				return false;
			}

			return true;
		}

		#endregion // ConvertTextToValue

		#region ConvertValueToText

		/// <summary>
		/// Converts the specified value to text using the <see cref="ValueInput.ValueToTextConverterResolved"/>.
		/// This method is used to display the value of the editor when the editor is in edit mode.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <param name="text">This out parameter will be set to the converted text.</param>
		/// <param name="error">If conversion fails, error is set to a value that indicates the error.</param>
		/// <returns>True if conversion succeeds, False otherwise.</returns>
		/// <remarks>
		/// <p class="body">
		/// ConvertValueToText is used to convert value to text. This method is typically used to 
		/// convert the value of the editor (as specified by the <see cref="ValueInput.Value"/> property)
		/// into text that the user can edit when the editor enters edit mode. When not in edit mode,
		/// <see cref="TextInputBase.ConvertValueToDisplayText"/> method is used to convert value
		/// to text that gets displayed in the editor. The <see cref="ValueInput.Text"/> property's return value
		/// corresponds to the text that this method converts where as the <see cref="TextInputBase.DisplayText"/> 
		/// property's return value corresponds to the text that ConvertValueToDisplayText method converts.
		/// </p>
		/// <p class="body">
		/// <b>Note</b> that DisplayText and ConvertValueToDisplayText methods are defined on <see cref="TextInputBase"/>
		/// class. This is because display text conversions are only applicable for text based editors, all of which
		/// derive from TextInputBase.
		/// </p>
		/// <p class="body">
		/// As an example, the ValueType property of a <see cref="XamMaskedInput"/> is set to DateTime type, 
		/// and the <see cref="ValueInput.Value"/> property is set to a "01/01/2007" DateTime instance.
		/// This method gets called to convert that DateTime value to a string when the user enters
		/// edit mode. When the editor is not in edit mode, <see cref="TextInputBase.ConvertValueToDisplayText"/>
		/// is used. The difference between this method and ConvertValueToDisplayText is that the
		/// ConvertValueToDisplayText will take into account <see cref="ValueInput.FormatProvider"/>
		/// and <see cref="ValueInput.Format"/> property settings where as ConvertValueToText will not.
		/// </p>
		/// <p class="body">
		/// <b>Note:</b> Typically there is no need for you to call this method directly as this method is 
		/// automatically called by the ValueInput itself to perform the necessary conversions between value 
		/// and text.
		/// </p>
		/// <p class="body">
		/// <b>Note:</b> If you want to override the default conversion logic for converting between value and text,
		/// set the <see cref="ValueInput.ValueToTextConverter"/> and <see cref="TextInputBase.ValueToDisplayTextConverter"/>
		/// properties.
		/// </p>
		/// </remarks>
		[EditorBrowsable( EditorBrowsableState.Advanced )]
		public bool ConvertValueToText( object value, out string text, out Exception error )
		{
			error = null;
			text = null;

			try
			{
				text = (string)this.ValueToTextConverterResolved.Convert( value, typeof( string ),
					this, this.FormatProviderResolved as System.Globalization.CultureInfo );
			}
			catch ( Exception e )
			{
				error = e;
				return false;
			}

			return true;
		}

		#endregion // ConvertValueToText

		#region OnIsFocusWithinChangedHelper

		internal void OnIsFocusWithinChangedHelper( bool gotFocus )
		{
			bool focusedStateChanged = false;
			if ( gotFocus )
			{
				if ( null == _lostFocusTracker )
				{
					var lostFocusTracker = new LostFocusTracker( this, OnLostFocusTrackerCallback );
					if ( lostFocusTracker.IsActive )
					{
						_lostFocusTracker = lostFocusTracker;
						focusedStateChanged = true;
					}
				}
			}
			else
			{
				if ( null != _lostFocusTracker )
				{
					_lostFocusTracker.Deactivate( false );
					_lostFocusTracker = null;
					focusedStateChanged = true;
				}
			}

			if ( focusedStateChanged )
			{







				this.ConsiderIsInEditMode( "IsKeyboardFocusWithin", gotFocus );
			}
		}

		#endregion // OnIsFocusWithinChangedHelper

		#region OnLostFocusTrackerCallback

		private void OnLostFocusTrackerCallback( )
		{
			this.OnIsFocusWithinChangedHelper( false );
		}

		#endregion // OnLostFocusTrackerCallback

		#region ProcessPropertyChanged

		/// <summary>
		/// Called when a property value has changed
		/// </summary>
		/// <param name="e">The event arguments</param>
		internal virtual void ProcessPropertyChanged( DependencyPropertyChangedEventArgs e )
		{
			DependencyProperty dp = e.Property;

			if ( dp == ValueProperty )
			{
				// if we aren't in edit mode sync up the OriginalValue property
				// AS 10/16/08 TFS9214
				// We do want to update the original value for an IsAlwaysInEditMode
				// editor when it doesn't have logical focus. Also changed to use the 
				// virtual helper method for consistency.
				//
				//if (this.IsInEditMode == false)
				//	this.SetValue(OriginalValuePropertyKey, this.Value);
				if ( this.IsInEditMode == false )
					this.InitializeOriginalValueFromValue( );
			}
			else if ( dp == IsReadOnlyProperty )
			{
				// raise a property changed for the automation peer
				ValueInputAutomationPeer peer = FrameworkElementAutomationPeer.FromElement( this ) as ValueInputAutomationPeer;

				if ( null != peer )
					peer.RaiseReadOnlyPropertyChangedEvent( (bool)e.OldValue, (bool)e.NewValue );
			}
			else if ( dp == TextProperty )
			{
				// raise a property changed for the automation peer
				ValueInputAutomationPeer peer = FrameworkElementAutomationPeer.FromElement( this ) as ValueInputAutomationPeer;

				if ( null != peer )
					peer.RaiseValuePropertyChangedEvent( (string)e.OldValue, (string)e.NewValue );
			}
			else if ( dp == LanguageProperty
				// SSP 2/6/09 TFS10470
				// 
				|| FormatProperty == dp || FormatProviderProperty == dp
				)
			{
				// SSP 6/2/09 TFS17233
				// Added caching for FormatProviderResolved property.
				// 
				_cachedFormatProviderResolved = null;

				// SSP 2/6/09 TFS10470
				// When Format or FormatProvider changes, re-format the display text based on the new values.
				// 
				this.SyncValueProperties( ValueProperty, this.Value );
			}
		}

		#endregion // ProcessPropertyChanged

		#region ResolveValueType

		private void ResolveValueType()
		{
			this.ValueTypeResolved = TypeResolverUtilities.ResolveType(this, this.ValueType, this.ValueTypeName, this.DefaultValueType, "UnknownTypeName");
		}

		#endregion //ResolveValueType	
    		
		#region RevalidateCurrentValue

		
		
		/// <summary>
		/// Validates the current value and updates the IsValueValid property accordingly. However
		/// note that this doesn't raise any events or display error messages. It simply checks if the
		/// current value is valid and based on that updates the IsValueValid property.
		/// </summary>
		private void RevalidateCurrentValue( )
		{
			Exception error;

			// JJD 1/9/09 - NA 2009 vol 1 - Record filtering 
			// Call new ValidateCurrentValueHelper methid that will also
			// let the host performa any additional validation logic.
			//bool isValueValid = this.ValidateCurrentValue( out error );
			bool isValueValid = this.ValidateCurrentValueHelper( out error );

			// SSP 4/17/09 NAS9.2 IDataErrorInfo Support
			// Use the new SetIsValueValid method instead.
			// 
			//this.SetValue( IsValueValidPropertyKey, isValueValid );
			this.SetIsValueValid( isValueValid, error );
		}

		#endregion // RevalidateCurrentValue

		#region SyncValueProperties

		internal void SyncValueProperties( DependencyProperty prop, object newValue )
		{
			Exception error = null;

			// Do not recursively sync properties. Also do not sync properties while initializing.
			// 
			
			
			
			
			
			
			if ( _initialized && ! _inSyncValueProperties )
			{
				
				
				
				this.BeginSyncValueProperties( );

				
				
				_inSyncValueProperties = true;

				try
				{
					
					
					
					
					bool isValueValid = this.SyncValuePropertiesOverride( prop, newValue, out error );
					
#region Infragistics Source Cleanup (Region)



















#endregion // Infragistics Source Cleanup (Region)

					

					if ( isValueValid )
					{
						//Exception tmp;

                        // JJD 1/9/09 - NA 2009 vol 1 - Record filtering 
                        // Call new ValidateCurrentValueHelper methid that will also
                        // let the host performa any additional validation logic.
                        //if (!this.ValidateCurrentValue(out tmp))
						// SSP 4/17/09 NAS9.2 IDataErrorInfo Support
						// Use the error member variable instead of tmp so we can use 
						// it further down.
						// 
                        //if (!this.ValidateCurrentValueHelper(out tmp))
						if ( !this.ValidateCurrentValueHelper( out error ) )
							isValueValid = false;
					}

					// Update the IsValueValid property.
					// 
					// SSP 4/17/09 NAS9.2 IDataErrorInfo Support
					// Use the new SetIsValueValid method instead.
					// 
					//this.SetValue(IsValueValidPropertyKey, KnownBoxes.FromValue(isValueValid));
					this.SetIsValueValid( isValueValid, error );
				}
				finally
				{
					// SSP 1/19/10 TFS30067
					// Moved this line from below. We need to reset the _inSyncValueProperties flag before
					// we call EndSyncValueProperties which raises value changed events. If a value changed
					// handler sets a value property of the editor then we will skip synchronizing it with
					// other value properties because of the _inSyncValueProperties flag. Therefore we need
					// to reset before raising value changed events.
					// 
					_inSyncValueProperties = false;

					
					
					
					this.EndSyncValueProperties( );

					
					
					// SSP 1/19/10 TFS30067
					// Moved this line above before the EndSyncValueProperties call.
					// 
					//_inSyncValueProperties = false;
				}
			}

			if ( this.IsInEditMode )
				this.SetValue( HasValueChangedPropertyKey, KnownBoxes.TrueBox );
		}

		#endregion // SyncValueProperties

		#region SyncValuePropertiesOverride

		
		
		

		/// <summary>
		/// Called to synchronize value and text properties. Derived classes can override this
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
		internal virtual bool SyncValuePropertiesOverride( DependencyProperty prop, object newValue, out Exception error )
		{
			if ( ValueInput.ValueProperty == prop )
			{
				object value = newValue;
				string text;
				this.ConvertValueToText( value, out text, out error );
				this.Text = text;
			}
			else if ( ValueInput.TextProperty == prop )
			{
				string text = (string)newValue;
				object value;
				this.ConvertTextToValueHelper( text, out value, out error );
				this.Value = value;
			}
			else
			{
				Debug.Assert( false, "This should only be called for Text and Value properties. Derived classes need to handle their own properties." );
				error = new InvalidOperationException( "This should only be called for Text and Value properties. Derived classes need to handle their own properties." );
				return false;
			}

			return null == error;
		}

		#endregion // SyncValuePropertiesOverride

		// JJD 1/9/09 - NA 2009 vol 1 - Record filtering - added
        #region ValidateCurrentValueHelper

        private bool ValidateCurrentValueHelper(out Exception error)
        {
            bool isValueValid = this.ValidateCurrentValue(out error);

            return isValueValid;
        }

        #endregion //ValidateCurrentValueHelper	
    
		#region VerifyFocusSite

		private void VerifyFocusSite( )
		{
			DependencyObject focusSite = this.GetFocusSite( );

			if ( focusSite == null )
				return;

			Exception error;
			if ( !this.ValidateFocusSite( focusSite, out error ) )
				throw error;

			if ( focusSite != this._focusSite )
			{
				this._focusSite = focusSite;
				this.OnFocusSiteChanged( );
			}
		}

		#endregion //VerifyFocusSite	

		#endregion //Private Methods

		#endregion //Methods

		#region IPropertyChangeListener Interface Implementation

		void ITypedPropertyChangeListener<object, string>.OnPropertyValueChanged( object dataItem, string property, object extraInfo )
		{
			if ( _cachedValueConstraint == dataItem )
			{
				this.OnValueConstraintChanged( property );
			}
		} 

		#endregion // IPropertyChangeListener Interface Implementation

		#region ISupportInitialize Members


#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

		#endregion // ISupportInitialize Members
	}

	#endregion // ValueInput Class

	#region ValueInputDefaultConverter Class

	internal class ValueInputDefaultConverter : IValueConverter
	{
		private static ValueInputDefaultConverter _valueToDisplayTextConverter;
		private static ValueInputDefaultConverter _valueToTextConverter;

		protected bool _isDisplayTextConverter;

		protected ValueInputDefaultConverter( bool isDisplayTextConverter )
		{
			_isDisplayTextConverter = isDisplayTextConverter;
		}

		public static ValueInputDefaultConverter ValueToTextConverter
		{
			get
			{
				if ( null == _valueToTextConverter )
					_valueToTextConverter = new ValueInputDefaultConverter( false );

				return _valueToTextConverter;
			}
		}

		public static ValueInputDefaultConverter ValueToDisplayTextConverter
		{
			get
			{
				if ( null == _valueToDisplayTextConverter )
					_valueToDisplayTextConverter = new ValueInputDefaultConverter( true );

				return _valueToDisplayTextConverter;
			}
		}

		private object ConvertHelper( bool convertingBack, object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
		{
			ValueInput editor = parameter as ValueInput;

			IFormatProvider formatProvider;
			string format = null;

			Debug.Assert( null != editor );
			if ( null != editor )
			{
				formatProvider = editor.FormatProviderResolved;

				if ( _isDisplayTextConverter )
					format = editor.Format;
			}
			else
			{
				formatProvider = culture;
			}

			// SSP 8/20/08 BR35749
			// Moved handling of null into new virtual ConvertHelper virtual method so
			// derived editors have control over it.
			// 
			
#region Infragistics Source Cleanup (Region)
























#endregion // Infragistics Source Cleanup (Region)


			if ( null != editor )
			{
				return this.ConvertHelper( convertingBack, value, targetType, editor, formatProvider, format );
			}
			else
			{
				// SSP 8/20/08 BR35749
				// Moved handling of null into virtual ConvertHelper so the derived editors 
				// have control over it. However that means we need to handle it here in the
				// case where editor is null and we don't call the virtual ConvertHelper 
				// method.
				// 
				// If the Value is null or DBNull, then return empty string.
				// 
				// SSP 10/28/09 BR35477
				// When converting back, convert string.Empty to null as well.
				// 
				// ------------------------------------------------------------------------------
				object convertedValue;
				if ( this.HandleNullValueHelper( convertingBack, value, targetType, editor, out convertedValue ) )
					return convertedValue;
				



				// ------------------------------------------------------------------------------

				return CoreUtilities.ConvertDataValue( value, targetType, formatProvider, format );
			}
		}

		// SSP 8/20/08 BR35749
		// Added HandleNullHelper helper method.
		// 
		// SSP 10/28/09 BR35477
		// When converting back, convert string.Empty to null.
		// 
		// --------------------------------------------------------------------------------------------------------------
		/// <summary>
		/// Handles null and empty string values.
		/// </summary>
		/// <param name="convertingBack">Whether converting back.</param>
		/// <param name="value">Value to convert.</param>
		/// <param name="targetType">Type to convert the value to.</param>
		/// <param name="editor">Optional - can be null.</param>
		/// <param name="convertedValue">This will be assigned the converted value.</param>
		/// <returns>Returns true if the value was handled.</returns>
		private bool HandleNullValueHelper( bool convertingBack, object value, Type targetType, ValueInput editor, out object convertedValue )
		{
			bool isNull = null == value || DBNull.Value == value;

			if ( convertingBack )
			{
				// AS 5/1/09 NA 2009 Vol 2 - Clipboard Support
				// We need to handle the display to text converter converting back from the null text.
				//
				if (_isDisplayTextConverter)
				{
					TextInputBase textInput = editor as TextInputBase;
					if (null != textInput && object.Equals(value, textInput.NullText))
					{
						
						
						
						
						convertedValue = null;

						return true;
					}
				}

				bool isEmptyString = value is string && 0 == ( (string)value ).Length;

				// When converting text back into value, convert empty string to null. That is when
				// the user deletes all the contents of the editor, the value should be returned 
				// as null.
				// 
				if ( isEmptyString )
				{
					
					
					
					
					convertedValue = null;

					return true;
				}
			}
			else // If converting from value to text, then null/dbnull should be converted to NullText.
			{
				if ( isNull )
				{
					string nullText = string.Empty;

					// For DisplayText conversions use NullText setting.
					// 
					if ( _isDisplayTextConverter )
					{
						TextInputBase textInput = editor as TextInputBase;
						if ( null != textInput )
							nullText = textInput.NullText;
					}

					Debug.Assert( typeof( string ) == targetType, "This conversion phase is only for converting to text." );

					convertedValue = typeof( string ) == targetType ? nullText : null;
					return true;
				}
			}

			
			// If the value is null or DBNull then return the value as it is.
			// 
			if ( isNull )
			{
				convertedValue = value;
				return true;
			}

			convertedValue = null;
			return false;
		}
		
#region Infragistics Source Cleanup (Region)






















#endregion // Infragistics Source Cleanup (Region)

		// --------------------------------------------------------------------------------------------------------------

		protected virtual object ConvertHelper( bool convertingBack, object value, Type targetType, 
			ValueInput editor, IFormatProvider formatProvider, string format )
		{
			// SSP 8/20/08 BR35749
			// Moved handling of null to here from the non-virtual ConvertHelper 
			// to this method so derived editors have control over it.
			// 
			// If the Value is null or DBNull, then return empty string.
			// 
			// SSP 10/28/09 BR35477
			// When converting back, convert string.Empty to null as well.
			// 
			// ------------------------------------------------------------------------------
			object convertedValue;
			if ( this.HandleNullValueHelper( convertingBack, value, targetType, editor, out convertedValue ) )
				return convertedValue;
			



			// ------------------------------------------------------------------------------

			return CoreUtilities.ConvertDataValue( value, targetType, formatProvider, format );
		}

		object IValueConverter.Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
		{
			return this.ConvertHelper( false, value, targetType, parameter, culture );
		}

		object IValueConverter.ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
		{
			return this.ConvertHelper( true, value, targetType, parameter, culture );
		}
	}

	#endregion // ValueInputDefaultConverter Class

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