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
using Infragistics.Shared;
using Infragistics.Windows;
using Infragistics.Windows.Resizing;
using Infragistics.Windows.Controls;
using Infragistics.Windows.Themes;
using Infragistics.Windows.Editors.Events;
using Infragistics.Windows.Helpers;
using System.Windows.Automation.Peers;
using Infragistics.Windows.Automation.Peers.Editors;
using System.Windows.Markup;
using System.Globalization;
using Infragistics.Windows.Internal;

namespace Infragistics.Windows.Editors
{
	#region ValueEditor Class



#region Infragistics Source Cleanup (Region)

















#endregion // Infragistics Source Cleanup (Region)

    /// <summary>
	/// An abstract base class that provides functionality for displaying or editing values. 
	/// </summary>
	/// <remarks>
	/// <p class="body"> <see cref="ValueEditor"/> 
	/// is an abstract base class from which all other editors are derived.
	/// ValueEditor implements a significant portion of functionality that is
	/// common to all the value editors.
	/// </p>
	/// <p class="body">Here's a listing of key features
	/// that make ValueEditor and derived classes.</p>
	/// <UL>
	/// <LI>
	/// <see cref="ValueEditor.ValueType"/> -- The ValueType property specifies the type of values that are being
	/// displayed/edited by the editor. The ValueEditor ensures that the user input
	/// is of this type. The editor will automatically parse the input and convert
	/// it to an object of this type. If the user input is not in the correct
	/// format, the value editor will reject the input and take appropriate action
	/// based on the <see cref="ValueEditor.InvalidValueBehavior"/> 
	/// property setting.
	/// </LI>
	/// <LI>
	/// <see cref="ValueEditor.Value"/> -- The Value property gets or sets the value being displayed/edited by the
	/// editor. The Value property is an object type property. This is because the
	/// Value property will return objects of type specified by the ValueType
	/// property. For example, if you set the ValueType of a xamTextEditor to
	/// DateTime, the Value property will return DateTime objects. The editor will
	/// automatically convert the user input into DateTime objects as mentioned
	/// above. <see cref="ValueEditor.Text"/> property returns string representation of the value.
	/// </LI>
	/// <LI>
	/// <see cref="ValueEditor.ValueConstraint"/> -- The ValueConstraint property allows you to specify constraints for the
	/// values that the user can enter in the value editor. Among the constraints
	/// you can specify include minimum value, maximum value, regular expression,
	/// nullable, maximum length, etc. The value editor will validate the user
	/// input against these constraints and if the input doesn't satisfy the
	/// constraints, the editor will consider the entered value invalid. The
	/// IsValueValid property of the ValueEditor indicates whether or not the
	/// current input is valid or not. If the input is invalid, the editor will
	/// take actions when the user attempts to leave the editor based on the
	/// InvalidValueBehavior property setting.
	/// </LI>
	/// <LI>
	/// <see cref="ValueEditor.Format"/> -- The Format property lets you specify formatting to use when the editor is not
	/// in edit mode. For example, you can set Format to "c" on a xamTextEditor
	/// whose ValueType is set to decimal. The xamTextEditor control will format
	/// the values using currency formatting ("c" format). When the editor enters
	/// edit mode, it will remove this formatting for a more natural editing
	/// experience.
	/// </LI>
	/// <LI>
	/// <STRONG>Custom Converters</STRONG> -- The ValueEditor and
	/// TextEditorBase classes (from which all the text-based editors are derived
	/// from) expose properties of type IValueConverter for specifying custom logic
	/// for converting between text and value. ValueEditor exposes <see cref="ValueEditor.ValueToTextConverter"/>,
	/// and <see cref="TextEditorBase"/> exposes <see cref="TextEditorBase.ValueToDisplayTextConverter"/> 
	/// properties, respectively. ValueToTextConverter is used for converting
	/// between the Value and Text properties. ValueToTextConverter is used in both
	/// directions -- for converting value to text, and from text (user input) into
	/// the value type. On the other hand, the ValueToDisplayTextConverter property
	/// is used only for converting value to display text.<BR/><BR/>The key
	/// distinction between ValueToTextConverter and ValueToDisplayTextConverter is
	/// that ValueToDisplayTextConverter is used in display mode and
	/// ValueToTextConverter is used in edit mode. As a result,
	/// ValueToTextConverter is used for not only converting value to text, but
	/// also for converting back the user input in the form of text to the value
	/// type. For example, if a xamTextEditor's ValueType is set to DateTime, the
	/// ValueToTextConverter property will be used for converting DateTime objects
	/// to text to display when in edit mode and also for converting back the user
	/// input into DateTime objects. ValueToDisplayTextConverter will be used for
	/// converting DateTime objects to text for displaying while the editor is not
	/// in edit mode. The reason for using two different converters -- one for edit
	/// mode and one for display mode -- is that it lets you display values with
	/// different formatting and edit values with different formatting. For
	/// example, in the case of DateTime, you may want to display dates as long
	/// dates (e.g., January 01, 2007). And when the end user enters edit mode
	/// to modify the value, you may want to display the same value as 01/01/07 for
	/// easier editing. After the user modifies the date in short date format and
	/// leaves the editor, the new date will automatically be displayed in the long
	/// date format using the ValueToDisplayTextConverter property.<note>
	/// <STRONG>Note:</STRONG> Typically, there is no need to
	/// specify these converters because the default conversion logic provided
	/// by the editors is sufficient. You may, however, want to specify these if
	/// you want to implement custom conversion logic. Essentially, this facility
	/// lets you do some of the same things that the editor data filter
	/// functionality lets you do in Windows Forms embeddable editor architecture.
	/// For example, you can specify ValueToTextConverter that converts '2k', '2m',
	/// etc... to 1000, 1000000 respectively. Or '+2m', '-1d' to a date that is two
	/// months from now and yesterday, respectively.
	/// </note>
	/// </LI>
	/// <LI><see cref="ValueEditor.InvalidValueBehavior"/> -- The InvalidValueBehavior
	/// property specifies which action to take when the user attempts to
	/// leave the editor after entering an invalid value. The editor will consider
	/// user input to be invalid when the input cannot be properly parsed into the
	/// editor's ValueType or it does not satisfy the value constraints specified
	/// by the ValueConstraint. The default value is DisplayErrorMessage, which
	/// will display a message to the user indicating that the value entered is
	/// invalid.
	/// </LI>
	/// <LI><STRONG>Default Editors Repository</STRONG> -- ValueEditor
	/// maintains a mapping of editors to use by default for each known data type.
	/// This mapping is used by the controls such as xamDataGrid and
	/// xamDataPresenter to select a default editor for each field based on the
	/// field's data type. For example, for Boolean fields, xamCheckEditor is
	/// used. You can override which editor to use by default for a specific data
	/// type using the ValueEditor's RegisterDefaultEditorForType static method.
	/// You can even register a custom ValueEditor for a data type.
	/// </LI>
	/// <LI><STRONG>Default Masks Repository</STRONG> -- xamMaskedEditor maintains
	/// a mapping of masks to use by default for each known data type. For example,
	/// for long data type, the default mask that gets used has 19 digits. You can
	/// specify default mask for a data type using the xamMaskedEditor's
	/// <see cref="XamMaskedEditor.RegisterDefaultMaskForType"/> 
	/// static method. <note><STRONG>Note</STRONG> The default masks
	/// repository is used by xamMaskedEditor and all the other mask-based editors
	/// as well (e.g., xamDateTimeEditor
	/// and xamNumericEditor).</note>
	/// </LI>
	/// </UL>
	/// </remarks>


    // JJD 4/15/10 - NA2010 Vol 2 - Added support for VisualStateManager
    [TemplateVisualState(Name = VisualStateUtilities.StateNormal,              GroupName = VisualStateUtilities.GroupCommon)]
    [TemplateVisualState(Name = VisualStateUtilities.StateReadOnly,            GroupName = VisualStateUtilities.GroupCommon)]
    [TemplateVisualState(Name = VisualStateUtilities.StateMouseOver,           GroupName = VisualStateUtilities.GroupCommon)]
    [TemplateVisualState(Name = VisualStateUtilities.StateDisabled,            GroupName = VisualStateUtilities.GroupCommon)]

    [TemplateVisualState(Name = VisualStateUtilities.StateEmbedded,            GroupName = VisualStateUtilities.GroupEmbedded)]
    [TemplateVisualState(Name = VisualStateUtilities.StateNotEmbedded,         GroupName = VisualStateUtilities.GroupEmbedded)]

    [TemplateVisualState(Name = VisualStateUtilities.StateFocused,             GroupName = VisualStateUtilities.GroupFocus)]
    [TemplateVisualState(Name = VisualStateUtilities.StateUnfocused,           GroupName = VisualStateUtilities.GroupFocus)]

    [TemplateVisualState(Name = VisualStateUtilities.StateInvalidFocusedEx,    GroupName = VisualStateUtilities.GroupValidationEx)]
    [TemplateVisualState(Name = VisualStateUtilities.StateInvalidUnfocusedEx,  GroupName = VisualStateUtilities.GroupValidationEx)]
    [TemplateVisualState(Name = VisualStateUtilities.StateValidEx,             GroupName = VisualStateUtilities.GroupValidationEx)]

	[TemplatePart(Name = "PART_FocusSite", Type = typeof(DependencyObject))]
	//[Description("Base class from which other value editors are derived from.")]
	public abstract class ValueEditor : Control
		// SSP 3/24/10 TFS27839
		// Implemented IWeakEventListener.
		// 
		, IWeakEventListener
	{
		#region Private Members

		private ValuePresenter _host;
		private object _hostContext;
		private DependencyObject _focusSite;
		private bool _initialized;
		private bool _isInEditMode;
		internal bool _isEnteringEditMode;
		private int _syncingValueProperties;
		internal bool _isInEndEditMode;
		
		// JM 08-25-11 TFS84624 - Removed.
		//private bool _isChangingTemplate;
		
		// SSP 12/31/07 BR27393
		// While we are synchronizing value properties, we need to delay raising changed notifications
		// until after all the value properties have been synchronized. Otherwise when a property
		// changed notification is raised, some of the other value properties may not have been
		// updated yet.
		// 
		private List<RoutedEventArgs> _pendingPropertyChangedEvents;

		
		
		private bool _processingPendingPropertyChangedEvents;

		// SSP 6/2/09 TFS17233
		// Added caching mechanism for this resolved property because it has to take into account
		// the language property and resolve a culture from it which can be inefficient, especially
		// when it gets called a lot, for example during sorting or grouping in the data presenter.
		// 
		private IFormatProvider _cachedFormatProviderResolved;


        // JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
        private bool _hasVisualStateGroups;


		// JJD 08/08/12 - TFS118455
		// Added anti re-entrancy flag when displaying error msg dialog
		private bool _isDisplayingErrorMsgDialog;

		// SSP 1/16/12 TFS59404
		// 
		private bool _isInitializing;

		#endregion //Private Members

		#region Private Static Members

		private static Dictionary<Type, Type> g_defaultEditors;

		// MD 8/12/10 - TFS26592
		// Moved the caching logic to the Utilities class in the Windows assembly.
		//// JJD 4/10/08
		//// Added map to cache cultures for each language
		//[ThreadStatic()]
		//private static Dictionary<XmlLanguage, CultureInfo> g_LanguageCultureMap;

		
		
		private bool _inSyncValueProperties;

		#endregion //Private Static Members

		#region Constructor

		static ValueEditor()
		{
			// AS 5/9/08
			// register the groupings that should be applied when the theme property is changed
			ThemeManager.RegisterGroupings(typeof(ValueEditor), new string[] { PrimitivesGeneric.Location.Grouping, EditorsGeneric.Location.Grouping });

			// register the default editors here

			RegisterDefaultEditorForType( typeof( string ), typeof( XamTextEditor ), false );

			RegisterDefaultEditorForType( typeof( bool ), typeof( XamCheckEditor ), false );


			RegisterDefaultEditorForType( typeof( DateTime ), typeof( XamDateTimeEditor ), false );
			RegisterDefaultEditorForType( typeof( decimal ), typeof( XamCurrencyEditor ), false );
			RegisterDefaultEditorForType( typeof( double ), typeof( XamNumericEditor ), false );
			RegisterDefaultEditorForType( typeof( float ), typeof( XamNumericEditor ), false );
			RegisterDefaultEditorForType( typeof( byte ), typeof( XamNumericEditor ), false );
			RegisterDefaultEditorForType( typeof( sbyte ), typeof( XamNumericEditor ), false );
			RegisterDefaultEditorForType( typeof( short ), typeof( XamNumericEditor ), false );
			RegisterDefaultEditorForType( typeof( ushort ), typeof( XamNumericEditor ), false );
			RegisterDefaultEditorForType( typeof( int ), typeof( XamNumericEditor ), false );
			RegisterDefaultEditorForType( typeof( uint ), typeof( XamNumericEditor ), false );
			RegisterDefaultEditorForType( typeof( long ), typeof( XamNumericEditor ), false );
			RegisterDefaultEditorForType( typeof( ulong ), typeof( XamNumericEditor ), false );


			TemplateProperty.OverrideMetadata(typeof(ValueEditor), new FrameworkPropertyMetadata(null, new CoerceValueCallback(OnCoerceTemplate)));

			// This will manage FocusWithinManager.IsFocusWithin property for this type.
			// 
			FocusWithinManager.RegisterType( typeof( ValueEditor ) );


            // JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
            UIElement.IsEnabledProperty.OverrideMetadata(typeof(ValueEditor), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnVisualStatePropertyChanged)));

        }

		/// <summary>
		/// Initializes a new <see cref="ValueEditor"/>
		/// </summary>
		protected ValueEditor()
		{
			
			
			
			
			
			this.InitializeCachedPropertyValues( );
		}

		#endregion //Constructor	
    
		#region Base class overrides

			#region HitTestCore

		// Overrode HitTestCore to make sure the value editor gets mouse messages regardless of whether
		// its background is transparent or not.
		// 
		/// <summary>
		/// Overridden.
		/// </summary>
		/// <param name="hitTestParameters"></param>
		/// <returns></returns>
		/// <remarks>
		/// <p class="body">
		/// This method is overridden on this class to make sure the value editor gets mouse messages
		/// regardless of whether its background is transparent or not.
		/// </p>
		/// </remarks>
		protected override HitTestResult HitTestCore( PointHitTestParameters hitTestParameters )
		{
			Rect rect = new Rect( new Point( ), this.RenderSize );
			if ( rect.Contains( hitTestParameters.HitPoint ) )
				return new PointHitTestResult( this, hitTestParameters.HitPoint );

			return base.HitTestCore( hitTestParameters );
		}

			#endregion // HitTestCore

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
			this.VerifyFocusSite();
//			this.VerifyTemplateState();

            // JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
            this._hasVisualStateGroups = VisualStateUtilities.GetHasVisualStateGroups(this);

            this.UpdateVisualStates(false);

        }

			#endregion //OnApplyTemplate	

			#region OnCreateAutomationPeer
		/// <summary>
		/// Returns an automation peer that exposes the <see cref="ValueEditor"/> to UI Automation.
		/// </summary>
		/// <returns>A <see cref="Infragistics.Windows.Automation.Peers.Editors.ValueEditorAutomationPeer"/></returns>
		protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer()
		{
			return new Infragistics.Windows.Automation.Peers.Editors.ValueEditorAutomationPeer(this);
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

				
				
				
				
				if ( null == _host && this.IsAlwaysInEditMode )
					this.StartEditMode( false );
			}
		}

			#endregion // OnInitialized

			#region OnIsKeyboardFocusWithinChanged

		/// <summary>
		/// Called when the value of IsKeyboardFocusWithin property changes.
		/// </summary>
		/// <param name="e"></param>
		/// <remarks>
		/// <p class="body">
		/// This method is overridden to ensure that when the value editor receives keyboard focus, its focus site
		/// (the element designated as PART_FocusSite in the control template) is given the keyboard focus. For example,
		/// when XamTextEditor receives keyboard focus, it gives keyboard focus to the TextBox control that it uses
		/// for editing. This way all the keyboard messages get delivered to the TextBox control.
		/// </p>
		/// </remarks>
		protected override void OnIsKeyboardFocusWithinChanged( DependencyPropertyChangedEventArgs e )
		{
			base.OnIsKeyboardFocusWithinChanged( e );

			bool focused = (bool)e.NewValue;

			if ( focused )
				this.SetFocusToFocusSite( );

            // JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
            this.UpdateVisualStates();

        }

			#endregion // OnIsKeyboardFocusWithinChanged

			#region OnKeyDown

		/// <summary>
		/// Called when a key is pressed.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// ValueEditor overrides OnKeyDown to handle certain key presses when it's not in embedded mode.
		/// <p class="body">
		/// <b>Note:</b> This is not a comprehensive list of key commands that are handled by the editor.
		/// Derived value may provide logic for other key strokes using .NET Commands infrastructure or
		/// indirectly by embedding another control inside the editor that processes keyboard messages.
		/// </p>
		/// <ul>
		/// <li><b>Escape:</b> Exits edit mode.</li>
		/// <li><b>Enter:</b> Enters edit mode.</li>
		/// <li><b>F2:</b> Toggles edit mode.</li>
		/// </ul>
		/// </p>
		/// </remarks>
		protected override void OnKeyDown(KeyEventArgs e)
		{
			// Call the base class implementation.
			// 
			base.OnKeyDown( e );

			// If the base class handled the key then return.
			// 
			if ( e.Handled )
				return;

			// If the host is interested in the key then return.
			// 
			if ( null != _host && _host.OnValueEditorKeyDown( e ) )
				return;

			// Call the virtual ProcessKeyDown method which the derived classes can override
			// to perform their own key action mappings.
			// 
			this.ProcessKeyDown( e );

			if ( e.Handled )
				return;

			// Process certain keys for the standalone editors.
			// 
			if (this._host == null)
			{
				switch (e.Key)
				{
					case Key.Escape:
						if (this.IsInEditMode)
						{
							if ( this.IsAlwaysInEditMode == false )
							{
								// SSP 9/27/07
								// This is to support the editors inside ribbon menu. Escape exits the edit mode
								// and changes the template to render template which causes the element with focus
								// like the textbox inside the editor to get disconnected from visual hierarchy.
								// This causes the framework to give focus to the root visual which would be the 
								// window which in turn will cause the menu to close up. The ideal thing would be
								// to leave the focus on the editor and simply exit edit mode. This will also 
								// result in better keyboard navigation behavior where you can hit Escape from an
								// editor and continue with tab navigation from that editor.
								// 
								this.Focus( );

								this.EndEditMode( false, false );
							}
							else
								this.RevertValueBackToOriginalValue( );

							e.Handled = true;
						}
						break;

					case Key.Enter:
						if (this.IsAlwaysInEditMode == false)
						{
							if ( this.IsInEditMode )
							{
								// SSP 9/27/07
								// Changed related to above. See there for more info.
								// 
								this.Focus( );

								this.EndEditMode( true, false );
								e.Handled = true;
							}
						}
						break;
					
					case Key.F2:
						if (this.IsAlwaysInEditMode == false)
						{
							if ( this.IsInEditMode )
							{
								// SSP 9/27/07
								// Changed related to above. See there for more info.
								// 
								this.Focus( );

								this.EndEditMode( true, false );
							}
							else
								this.StartEditMode( );

							e.Handled = true;
						}
						break;
				}
			}
		}

			#endregion //OnKeyDown	
    
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
		/// ValueEditor overrides this method to give focus to the editor and enter edit mode
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
			if ( ! e.Handled )
				e.Handled = this.StartEditModeOnMouseDownHelper( );
		}

			#endregion //OnMouseLeftButtonDown	

			#region OnPreviewLostKeyboardFocus

		
		
		
		/// <summary>
		/// This method is called when the editor looses focus.
		/// </summary>
		/// <param name="e">Associated event args.</param>
		protected override void OnPreviewLostKeyboardFocus( KeyboardFocusChangedEventArgs e )
		{
			if ( this.IsInEditMode )
			{
				// Only cancel shifting focus when the focus is being shifted within the same
				// focus scope. We do want to allow focus to go to different focus scope (like
				// another window, message box etc...).
				// 
				// NOTE: NewFocus of null indicates the focus shifting out of application. In
				// which case we should not perform validation.
				//
				Debug.Assert( ( null != e.OldFocus ) == ( e.OldFocus is DependencyObject ), "e.OldFocus is not a DependencyObject !" );
				Debug.Assert( ( null != e.NewFocus ) == ( e.NewFocus is DependencyObject ), "e.NewFocus is not a DependencyObject !" );
				DependencyObject oldFocus = e.OldFocus as DependencyObject;
				DependencyObject newFocus = e.NewFocus as DependencyObject;

				if ( null != newFocus && FocusManager.GetFocusScope( newFocus ) == FocusManager.GetFocusScope( this )
					// This is to account for keyboard focus shifting to a descendant element (like focus site)
					// in which case we should not execute validation logic.
					&& !FocusWithinManager.IsAncestorOf( this, newFocus ) )
				{
					EditModeValidationErrorEventArgs eventArgs;
					bool stayInEditMode;
					this.ValidateInputHelper( true, false, out eventArgs, out stayInEditMode );

					// If InvalidValueBehavior setting indicates that we should stay in edit mode then
					// set Handled to true to retain focus.
					// 
					if ( null != eventArgs && stayInEditMode )
						e.Handled = true;
				}
			}

			base.OnPreviewLostKeyboardFocus( e );
		}

			#endregion // OnPreviewLostKeyboardFocus

			#region OnPropertyChanged

		/// <summary>
		/// Called when a property value has changed
		/// </summary>
		/// <param name="e">The event arguments</param>
		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			DependencyProperty dp = e.Property;

			// JM 08-25-11 TFS84624 - Moved this code here from the now defunct SetTemplate method.
			// 
			// AS 3/6/09 TFS15045
			// If keyboard focus is within our template then its going to 
			// be orphaned when we release the edit template so we need to 
			// 
			// AS 3/10/09 TFS15045
			// We should not take focus if we don't have logical focus. The containing 
			// window may be closing. When that happens they clear the logical focus. 
			// As a result we exit edit mode. While exiting edit mode we restore the 
			// render template but since keyboard focus was within we were trying to 
			// give keyboard focus to the editor. This caused it to get logical focus 
			// again which caused it to try and get into edit mode while it was exiting
			// edit mode. If logical focus is elsewhere then we will not interfere with 
			// the keyboard focus.
			//
			if (dp == TemplateProperty)
			{
				if (this.IsKeyboardFocusWithin && this.IsKeyboardFocused == false && this.IsFocusWithin)
					this.Focus();
			}

			base.OnPropertyChanged(e);

			if (dp == StyleProperty)
			{
				this.VerifyTemplateState();
			}
			else if (dp == TemplateProperty)
			{
				// clear the old focus site
				if (this._focusSite != null)
				{
					this._focusSite = null;
					this.OnFocusSiteChanged();
				}

				this.VerifyTemplateState();
				this.VerifyFocusSite();
			}
			else if (dp == IsInEditModeProperty)
			{
				this.VerifyTemplateState();
			}
			else if (dp == ValueProperty)
			{
				// if we aren't in edit mode sync up the OriginalValue property
                // AS 10/16/08 TFS9214
                // We do want to update the original value for an IsAlwaysInEditMode
                // editor when it doesn't have logical focus. Also changed to use the 
                // virtual helper method for consistency.
                //
				//if (this.IsInEditMode == false)
				//	this.SetValue(OriginalValuePropertyKey, this.Value);
                if (this.IsInEditMode == false || (this.IsAlwaysInEditMode && this.IsFocusWithin == false))
                    this.InitializeOriginalValueFromValue();
			}
			else if (dp == IsAlwaysInEditModeProperty)
			{
				// SSP 7/7/08 BR34163
				// 
				// ----------------------------------------------------------------
				
#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)

				// Don't take any action if we haven't been initialized yet. We enter edit mode
				// based on IsAlwaysInEditMode setting in OnInitialized method.
				// 
				if ( null == _host && this.IsInitialized )
				{
					if ( this.IsAlwaysInEditMode )
					{
						// If IsAlwaysInEditMode is being set to true then enter edit mode if not
						// already.
						// 
						if ( ! this.IsInEditMode )
							this.StartEditMode( false );
					}
					else
					{
						// If IsAlwaysInEditMode is being set to false and the editor doesn't have
						// focus then exit the edit mode.
						// 
						if ( this.IsInEditMode && ! this.IsFocusWithin )
							this.EndEditMode( true, false );
					}
				}
				// ----------------------------------------------------------------
			}
			else if (dp == EditTemplateProperty)
			{
				this.VerifyTemplateState();
			}
			else if ( dp == FocusWithinManager.IsFocusWithinProperty )
			{
				this.OnIsFocusWithinChanged( (bool)e.NewValue );
			}
			else if (dp == IsReadOnlyProperty)
			{
				// raise a property changed for the automation peer
				ValueEditorAutomationPeer peer = UIElementAutomationPeer.FromElement(this) as ValueEditorAutomationPeer;

				if (null != peer)
					peer.RaiseReadOnlyPropertyChangedEvent((bool)e.OldValue, (bool)e.NewValue);
			}
			else if (dp == TextProperty)
			{
				// raise a property changed for the automation peer
				ValueEditorAutomationPeer peer = UIElementAutomationPeer.FromElement(this) as ValueEditorAutomationPeer;

				if (null != peer)
					peer.RaiseValuePropertyChangedEvent((string)e.OldValue, (string)e.NewValue);
			}
            else if (dp == LanguageProperty 
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
				this.SyncTextWithValue( );
            }
		}

			#endregion //OnPropertyChanged	

		#endregion //Base class overrides

		#region Events

			#region EditModeEnded

		/// <summary>
		/// Event ID for the <see cref="EditModeEnded"/> routed event
		/// </summary>
		public static readonly RoutedEvent EditModeEndedEvent =
			EventManager.RegisterRoutedEvent("EditModeEnded", RoutingStrategy.Bubble, typeof(EventHandler<EditModeEndedEventArgs>), typeof(ValueEditor));

		/// <summary>
		/// This method is called when the control has just exited edit mode. This method raises 
		/// <see cref="ValueEditor.EditModeEnded"/> event.
		/// </summary>
		/// <remarks>
		/// <seealso cref="ValueEditor.EditModeEnded"/>
		/// </remarks>
		protected virtual void OnEditModeEnded(EditModeEndedEventArgs args)
		{
			// MD 7/16/10 - TFS26592
			// Call off to the new helper method to raise the event. If the event was suppressed, manually call the 
			// event handler on the ValuePresenter.
			//this.RaiseEvent(args);
			if (this.RaiseEventHelper(args) == false)
				ValuePresenter.ClassHandler_EditModeEnded(this, args);
		}

		internal void RaiseEditModeEnded(EditModeEndedEventArgs args)
		{
			args.RoutedEvent = ValueEditor.EditModeEndedEvent;
			args.Source = this;
			this.OnEditModeEnded(args);
		}

		/// <summary>
		/// Occurs when the control has just exited edit mode
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// See <see cref="ValueEditor.StartEditMode()"/> and <see cref="ValueEditor.EndEditMode"/>
		/// methods for more information.
		/// </p>
		/// <seealso cref="IsAlwaysInEditMode"/>
		/// <seealso cref="OnEditModeEnded"/>
		/// <seealso cref="StartEditMode()"/>
		/// <seealso cref="EndEditMode"/>
		/// <seealso cref="EditModeEnding"/>
		/// <seealso cref="EditModeEnding"/>
		/// <seealso cref="EditModeEnded"/>
		/// <seealso cref="EditModeEndedEvent"/>
		/// <seealso cref="EditModeEndedEventArgs"/>
		/// </remarks>
		//[Description("Occurs when the control has just exited edit mode")]
		//[Category("Behavior")]
		public event EventHandler<EditModeEndedEventArgs> EditModeEnded
		{
			add
			{
				base.AddHandler(ValueEditor.EditModeEndedEvent, value);
			}
			remove
			{
				base.RemoveHandler(ValueEditor.EditModeEndedEvent, value);
			}
		}

			#endregion //EditModeEnded

			#region EditModeEnding

		/// <summary>
		/// Event ID for the <see cref="EditModeEnding"/> routed event
		/// </summary>
		/// <seealso cref="StartEditMode()"/>
		/// <seealso cref="EndEditMode"/>
		/// <seealso cref="EditModeEnding"/>
		/// <seealso cref="EditModeEnded"/>
		/// <seealso cref="EditModeStarting"/>
		/// <seealso cref="EditModeStarted"/>
		/// <seealso cref="OnEditModeEnding"/>
		/// <seealso cref="EditModeEndingEventArgs"/>
		public static readonly RoutedEvent EditModeEndingEvent =
			EventManager.RegisterRoutedEvent("EditModeEnding", RoutingStrategy.Bubble, typeof(EventHandler<EditModeEndingEventArgs>), typeof(ValueEditor));

		/// <summary>
		/// This method is called when the control is about to exit edit mode. This method raises 
		/// <see cref="ValueEditor.EditModeEnding"/> event.
		/// </summary>
		/// <remarks>
		/// <seealso cref="ValueEditor.EditModeEnding"/>
		/// </remarks>
		protected virtual void OnEditModeEnding(EditModeEndingEventArgs args)
		{
			// MD 7/16/10 - TFS26592
			// Call off to the new helper method to raise the event. If the event was suppressed, manually call the 
			// event handler on the ValuePresenter.
			//this.RaiseEvent(args);
			if (this.RaiseEventHelper(args) == false)
				ValuePresenter.ClassHandler_EditModeEnding(this, args);
		}

		internal void RaiseEditModeEnding(EditModeEndingEventArgs args)
		{
			args.RoutedEvent = ValueEditor.EditModeEndingEvent;
			args.Source = this;
			this.OnEditModeEnding(args);
		}

		/// <summary>
		/// Occurs when the control is about to exit edit mode
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// See <see cref="ValueEditor.StartEditMode()"/> and <see cref="ValueEditor.EndEditMode"/>
		/// methods for more information.
		/// </p>
		/// <seealso cref="IsAlwaysInEditMode"/>
		/// <seealso cref="OnEditModeEnding"/>
		/// <seealso cref="StartEditMode()"/>
		/// <seealso cref="EndEditMode"/>
		/// <seealso cref="EditModeEnded"/>
		/// <seealso cref="EditModeEnding"/>
		/// <seealso cref="EditModeEnded"/>
		/// <seealso cref="EditModeEndingEvent"/>
		/// <seealso cref="EditModeEndingEventArgs"/>
		/// </remarks>
		//[Description("Occurs when the control is about to exit edit mode")]
		//[Category("Behavior")]
		public event EventHandler<EditModeEndingEventArgs> EditModeEnding
		{
			add
			{
				base.AddHandler(ValueEditor.EditModeEndingEvent, value);
			}
			remove
			{
				base.RemoveHandler(ValueEditor.EditModeEndingEvent, value);
			}
		}

			#endregion //EditModeEnding

			#region EditModeStarted

		/// <summary>
		/// Event ID for the <see cref="EditModeStarted"/> routed event
		/// </summary>
		/// <seealso cref="IsAlwaysInEditMode"/>
		/// <seealso cref="StartEditMode()"/>
		/// <seealso cref="EndEditMode"/>
		/// <seealso cref="EditModeStarted"/>
		/// <seealso cref="EditModeStarting"/>
		/// <seealso cref="EditModeEnding"/>
		/// <seealso cref="EditModeEnded"/>
		/// <seealso cref="OnEditModeStarted"/>
		/// <seealso cref="EditModeStartedEventArgs"/>
		public static readonly RoutedEvent EditModeStartedEvent =
			EventManager.RegisterRoutedEvent("EditModeStarted", RoutingStrategy.Bubble, typeof(EventHandler<EditModeStartedEventArgs>), typeof(ValueEditor));

		/// <summary>
		/// This method is called when the control has just entered edit mode. This method raises 
		/// <see cref="ValueEditor.EditModeStarted"/> event.
		/// </summary>
		/// <remarks>
		/// <seealso cref="ValueEditor.EditModeStarted"/>
		/// </remarks>
		protected virtual void OnEditModeStarted(EditModeStartedEventArgs args)
		{
			// MD 7/16/10 - TFS26592
			// Call off to the new helper method to raise the event. If the event was suppressed, manually call the 
			// event handler on the ValuePresenter.
			//this.RaiseEvent(args);
			if (this.RaiseEventHelper(args) == false)
				ValuePresenter.ClassHandler_EditModeStarted(this, args);
		}

		internal void RaiseEditModeStarted(EditModeStartedEventArgs args)
		{
			args.RoutedEvent = ValueEditor.EditModeStartedEvent;
			args.Source = this;
			this.OnEditModeStarted(args);
		}

		/// <summary>
		/// Occurs when the control has just entered edit mode
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// See <see cref="ValueEditor.StartEditMode()"/> and <see cref="ValueEditor.EndEditMode"/>
		/// methods for more information.
		/// </p>
		/// <seealso cref="IsAlwaysInEditMode"/>
		/// <seealso cref="OnEditModeStarted"/>
		/// <seealso cref="StartEditMode()"/>
		/// <seealso cref="EndEditMode"/>
		/// <seealso cref="EditModeStarting"/>
		/// <seealso cref="EditModeEnding"/>
		/// <seealso cref="EditModeEnded"/>
		/// <seealso cref="EditModeStartedEvent"/>
		/// <seealso cref="EditModeStartedEventArgs"/>
		/// </remarks>
		//[Description("Occurs when the control has just entered edit mode")]
		//[Category("Behavior")]
		public event EventHandler<EditModeStartedEventArgs> EditModeStarted
		{
			add
			{
				base.AddHandler(ValueEditor.EditModeStartedEvent, value);
			}
			remove
			{
				base.RemoveHandler(ValueEditor.EditModeStartedEvent, value);
			}
		}

			#endregion //EditModeStarted

			#region EditModeStarting

		/// <summary>
		/// Event ID for the <see cref="EditModeStarting"/> routed event
		/// </summary>
		/// <seealso cref="IsAlwaysInEditMode"/>
		/// <seealso cref="StartEditMode()"/>
		/// <seealso cref="EndEditMode"/>
		/// <seealso cref="EditModeStarting"/>
		/// <seealso cref="EditModeStarted"/>
		/// <seealso cref="EditModeEnding"/>
		/// <seealso cref="EditModeEnded"/>
		/// <seealso cref="OnEditModeStarting"/>
		/// <seealso cref="EditModeStartingEventArgs"/>
		public static readonly RoutedEvent EditModeStartingEvent =
			EventManager.RegisterRoutedEvent("EditModeStarting", RoutingStrategy.Bubble, typeof(EventHandler<EditModeStartingEventArgs>), typeof(ValueEditor));

		/// <summary>
		/// This method is called when the control is about to enter edit mode. This method raises 
		/// <see cref="ValueEditor.EditModeStarting"/> event.
		/// </summary>
		/// <remarks>
		/// <seealso cref="ValueEditor.EditModeStarting"/>
		/// </remarks>
		protected virtual void OnEditModeStarting(EditModeStartingEventArgs args)
		{
			// MD 7/16/10 - TFS26592
			// Call off to the new helper method to raise the event. If the event was suppressed, manually call the 
			// event handler on the ValuePresenter.
			//this.RaiseEvent(args);
			if (this.RaiseEventHelper(args) == false)
				ValuePresenter.ClassHandler_EditModeStarting(this, args);
		}

		internal void RaiseEditModeStarting(EditModeStartingEventArgs args)
		{
			args.RoutedEvent = ValueEditor.EditModeStartingEvent;
			args.Source = this;
			this.OnEditModeStarting(args);
		}

		/// <summary>
		/// Occurs when the control is about to enter edit mode
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// See <see cref="ValueEditor.StartEditMode()"/> and <see cref="ValueEditor.EndEditMode"/>
		/// methods for more information.
		/// </p>
		/// <seealso cref="IsAlwaysInEditMode"/>
		/// <seealso cref="OnEditModeStarting"/>
		/// <seealso cref="StartEditMode()"/>
		/// <seealso cref="EndEditMode"/>
		/// <seealso cref="EditModeStarted"/>
		/// <seealso cref="EditModeEnding"/>
		/// <seealso cref="EditModeEnded"/>
		/// <seealso cref="EditModeStartingEvent"/>
		/// <seealso cref="EditModeStartingEventArgs"/>
		/// </remarks>
		//[Description("Occurs when the control is about to enter edit mode")]
		//[Category("Behavior")]
		public event EventHandler<EditModeStartingEventArgs> EditModeStarting
		{
			add
			{
				base.AddHandler(ValueEditor.EditModeStartingEvent, value);
			}
			remove
			{
				base.RemoveHandler(ValueEditor.EditModeStartingEvent, value);
			}
		}

			#endregion //EditModeStarting

			#region EditModeValidationError

		/// <summary>
		/// Event ID for the <see cref="EditModeValidationError"/> routed event
		/// </summary>
		/// <seealso cref="StartEditMode()"/>
		/// <seealso cref="EndEditMode"/>
		/// <seealso cref="EditModeValidationError"/>
		/// <seealso cref="EditModeStarting"/>
		/// <seealso cref="EditModeEnding"/>
		/// <seealso cref="EditModeEnded"/>
		/// <seealso cref="OnEditModeValidationError"/>
		/// <seealso cref="EditModeValidationErrorEventArgs"/>
		public static readonly RoutedEvent EditModeValidationErrorEvent =
			EventManager.RegisterRoutedEvent( "EditModeValidationError", RoutingStrategy.Bubble, typeof( EventHandler<EditModeValidationErrorEventArgs> ), typeof( ValueEditor ) );

		/// <summary>
		/// Occurs when there is an input validation error
		/// </summary>
		/// <seealso cref="StartEditMode()"/>
		/// <seealso cref="EndEditMode"/>
		/// <seealso cref="EditModeValidationError"/>
		/// <seealso cref="EditModeStarting"/>
		/// <seealso cref="EditModeEnding"/>
		/// <seealso cref="EditModeEnded"/>
		/// <seealso cref="EditModeValidationErrorEvent"/>
		/// <seealso cref="EditModeValidationErrorEventArgs"/>
		protected virtual void OnEditModeValidationError( EditModeValidationErrorEventArgs args )
		{
			// MD 7/16/10 - TFS26592
			// Call off to the new helper method to raise the event. If the event was suppressed, manually call the 
			// event handler on the ValuePresenter.
			//this.RaiseEvent(args);
			if (this.RaiseEventHelper(args) == false)
				ValuePresenter.ClassHandler_EditModeValidationError(this, args);
		}

		internal void RaiseEditModeValidationError( EditModeValidationErrorEventArgs args )
		{
			args.RoutedEvent = ValueEditor.EditModeValidationErrorEvent;
			args.Source = this;
			this.OnEditModeValidationError( args );
		}

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
		/// <seealso cref="OnEditModeValidationError"/>
		/// <seealso cref="StartEditMode()"/>
		/// <seealso cref="EndEditMode"/>
		/// <seealso cref="EditModeStarting"/>
		/// <seealso cref="EditModeEnding"/>
		/// <seealso cref="EditModeEnded"/>
		/// <seealso cref="EditModeValidationErrorEvent"/>
		/// <seealso cref="EditModeValidationErrorEventArgs"/>
		//[Description( "Occurs when the user attempts to leave the editor with an invalid value." )]
		//[Category( "Behavior" )]
		public event EventHandler<EditModeValidationErrorEventArgs> EditModeValidationError
		{
			add
			{
				base.AddHandler( ValueEditor.EditModeValidationErrorEvent, value );
			}
			remove
			{
				base.RemoveHandler( ValueEditor.EditModeValidationErrorEvent, value );
			}
		}

			#endregion //EditModeValidationError

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

				return null != _host && _host.AlwaysValidate;
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

				return this.LanguageCultureInfo;
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
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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

				// SSP 7/9/08 BR34636
				// Moved code into new LanguageCultureInfo property.
				// 
				return this.LanguageCultureInfo;
			}
		}

				#endregion // FormatProviderResolved

				#region InSyncTextWithValue

		// SSP 5/31/11 TFS57173
		// 
		internal static readonly DependencyProperty InSyncTextWithValueProperty = DependencyProperty.Register(
			"InSyncTextWithValue",
			typeof( string ),
			typeof( ValueEditor ),
			new FrameworkPropertyMetadata( null )
		);

				#endregion // InSyncTextWithValue

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
					if (this._host != null)
						value = this._host.DefaultInvalidValueBehavior;

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

				#region InternalIsInitializing

		// SSP 1/16/12 TFS59404
		// 
		/// <summary>
		/// Returns true if we are in DoInitialization method.
		/// </summary>
		internal bool InternalIsInitializing
		{
			get
			{
				return _isInitializing;
			}
		} 

				#endregion // InternalIsInitializing

                // AS 10/3/08 TFS8634
                #region IsEditingAllowed
        internal bool IsEditingAllowed
        {
            get { return this.Host == null || this.Host.IsEditingAllowed; }
        }
                #endregion //IsEditingAllowed

				#region LanguageCultureInfo

		// SSP 7/9/08 BR34636
		// Added LanguageCultureInfo. Code in there was moved from the FormatProviderResolved method.
		// 
		/// <summary>
		/// Returns the CultureInfo associated with the Language property setting.
		/// </summary>
		internal CultureInfo LanguageCultureInfo
		{
			get
			{
				// MD 8/12/10 - TFS26592
				// Moved this code to a static method so it could be used in other places.
				#region Moved
		
				//XmlLanguage xmlLanguage = this.Language;
				//if ( null != xmlLanguage )
				//{
				//    CultureInfo cultureInfo = null;
				//    bool wasLanguageMapped = false;
				//
				//    // JJD 4/10/08
				//    // Added map to cache cultureinfo for each language
				//    if ( g_LanguageCultureMap == null )
				//        g_LanguageCultureMap = new Dictionary<XmlLanguage, CultureInfo>( );
				//    else
				//    {
				//        if ( g_LanguageCultureMap.TryGetValue( xmlLanguage, out cultureInfo ) )
				//            wasLanguageMapped = true;
				//    }
				//
				//    if ( !wasLanguageMapped )
				//    {
				//        // JJD 4/15/08
				//        // Call the new GetNonNeutralCulture method instead which will return a 
				//        // culture that is non-neutral and can be used in formatting operations
				//        // without throwing an exception.
				//        //cultureInfo = xmlLanguage.GetEquivalentCulture();
				//        cultureInfo = Utilities.GetNonNeutralCulture( this );
				//
				//        // map the lamguage regardless of whether GetEquivalentCulture succeeded
				//        g_LanguageCultureMap.Add( xmlLanguage, cultureInfo );
				//    }
				//
				//    if ( cultureInfo != null )
				//        return cultureInfo;
				//}
				//
				//// use the current thread's ui culture as a fallback
				//// SSP 7/9/08 BR34636
				//// Use the CurrentCulture instead of CurrentUICulture. That's what we've been using
				//// in the past.
				//// 
				////return Thread.CurrentThread.CurrentUICulture;
				//return System.Globalization.CultureInfo.CurrentCulture;

				#endregion // Moved
				return Utilities.GetLanguageCultureInfo(this);
			}
		}

				#endregion // LanguageCultureInfo

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
		/// <seealso cref="ValueEditor.ValueToTextConverter"/>
		/// <seealso cref="TextEditorBase.ValueToDisplayTextConverter"/>
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
		public static readonly DependencyProperty AlwaysValidateProperty = DependencyProperty.Register(
  			"AlwaysValidate",
			typeof( bool? ),
			typeof( ValueEditor ),
			new FrameworkPropertyMetadata( null, new PropertyChangedCallback( OnAlwaysValidateChanged ) ) 
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
		/// <seealso cref="EditModeValidationError"/>
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
			bool? newVal = (bool?)e.NewValue;
		}

		/// <summary>
		/// Returns true if the AlwaysValidate property is set to a non-default value.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeAlwaysValidate( )
		{
			return Utilities.ShouldSerialize( AlwaysValidateProperty, this );
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

				#region Value

		/// <summary>
		/// Identifies the <see cref="Value"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
			"Value",
			typeof( object ),
			typeof( ValueEditor ),
			new FrameworkPropertyMetadata( null,
				
				FrameworkPropertyMetadataOptions.Journal | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
				new PropertyChangedCallback( OnValueChanged ),
				new CoerceValueCallback( OnCoerceValue ) )
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
		/// <b>Note:</b> As the user enters/modifies the contents of the <see cref="ValueEditor"/>, the 
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
		/// <seealso cref="ValueChanged"/>
		/// <seealso cref="TextChanged"/>
		/// </remarks>
		//[Description( "Gets/sets the value of the editor" )]
		//[Category( "Data" )]
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
			ValueEditor valueEditor = (ValueEditor)dependencyObject;

			// Don't syncrhonize between properties during the process of initialization 
			// since other properties like ValueType may or may not have been set yet.
			// 
			if ( ! valueEditor._initialized )
				return;

			
			
			
			
			if ( Utils.Equals( e.OldValue, e.NewValue ) )
				return;

			
			
			
			valueEditor.BeginSyncValueProperties( );
			try
			{
				// Call SyncValueProperties to synchronize the Text property with the new value.
				// 
				valueEditor.SyncValueProperties( e.Property, e.NewValue );

				// Call virtual OnValueChanged method to let the derived editors know of change
				// in the Value property.
				// 
				valueEditor.OnValueChanged( e.OldValue, e.NewValue );
			}
			finally
			{
				valueEditor.EndSyncValueProperties( );
			}
		}

		/// <summary>
		/// Event ID for the 'ValueChanged' routed event
		/// </summary>
		public static readonly RoutedEvent ValueChangedEvent =
				EventManager.RegisterRoutedEvent( "ValueChanged", RoutingStrategy.Bubble, typeof( RoutedPropertyChangedEventHandler<object> ), typeof( ValueEditor ) );

		/// <summary>
		/// Called when <b>Value</b> property changes or the contents of the editor changes.
		/// </summary>
		/// <seealso cref="ValueEditor.ValueChanged"/>
		protected virtual void OnValueChanged( object previousValue, object currentValue )
		{
			if (this._host != null && !this._isEnteringEditMode)
			{
				// make sure the host knows that the value has been changed

				this._host.OnValueChanged();
			}

			RoutedPropertyChangedEventArgs<object> newEvent = new RoutedPropertyChangedEventArgs<object>( previousValue, currentValue );
			newEvent.RoutedEvent = ValueEditor.ValueChangedEvent;
			newEvent.Source = this;

			
			
			
			this.RaiseValuePropertyChangedEvent( newEvent );
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
		/// not be parsed into the <see cref="ValueType"/>. For example if the ValueType is
		/// set to a numeric type like Double and the user enters a non-numeric value then the
		/// entered value can not be parsed into a Double. Therefore the Value property will not
		/// be updated. However the ValueChanged event will still be raised.
		/// </para>
		/// <seealso cref="IsValueValid"/>
		/// <seealso cref="Value"/>
		/// <seealso cref="Text"/>
		/// </remarks>
		//[Description( "Occurs when property 'Value' changes" )]
		//[Category( "Behavior" )]
		public event RoutedPropertyChangedEventHandler<object> ValueChanged
		{
			add
			{
				base.AddHandler( ValueEditor.ValueChangedEvent, value );
			}
			remove
			{
				base.RemoveHandler( ValueEditor.ValueChangedEvent, value );
			}
		}

		private static object OnCoerceValue( DependencyObject dependencyObject, object value )
		{
			ValueEditor valueEditor = (ValueEditor)dependencyObject;

			// Don't coerce during the process of initialization since ValueType may or 
			// may not have been set yet.
			// 
			if ( ! valueEditor._initialized )
				return value;

			return valueEditor.OnCoerceValue( value );
		}

		/// <summary>
		/// Called from the <see cref="Value"/> property's CoerceValue handler. The default 
		/// implementation performs type conversions therefore you should call the base implementation
		/// to ensure proper type conversions are performed.
		/// </summary>
		/// <param name="val"></param>
		/// <returns></returns>
		protected virtual object OnCoerceValue( object val )
		{
			// If the value being set is not of the ValueType, then coerce the value to that type.
			// 
			if ( null != val && DBNull.Value != val && !this.ValueType.IsInstanceOfType( val ) )
			{
				// SSP 9/12/07 - XamComboEditor
				// Combo editor's default text converter maps values to display text. Here we simply want
				// to convert the value to the ValueType.
				// 
				//val = this.DefaultValueToTextConverter.Convert( val, this.ValueType, this, null );
				val = Utilities.ConvertDataValue( val, this.ValueType, this.FormatProviderResolved, this.Format );
			}

			return val;
		}

				#endregion // Value

				#region Text

		/// <summary>
		/// Identifies the <see cref="Text"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
			"Text",
			typeof( string ),
			typeof( ValueEditor ),
			new FrameworkPropertyMetadata( string.Empty,
				
				FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
				new PropertyChangedCallback( OnTextChanged ),
				new CoerceValueCallback( OnCoerceText ) )
			);

		/// <summary>
		/// Gets or sets the value of the editor as text.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// Setting the <b>Text</b> property will also update the <b>Value</b> property. If the
		/// new text can not be parsed into the value type (<see cref="ValueType"/>) then the
		/// <b>Value</b> property will not be updated. However note that the <see cref="ValueChanged"/>
		/// event will still be raised.
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
		/// <seealso cref="ValueChanged"/>
		/// <seealso cref="TextChanged"/>
		/// </remarks>
		//[Description( "Gets/sets the value of the editor as text" )]
		//[Category( "Data" )]
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

		// SSP 5/10/12 TFS100047
		// 
		internal string Text_CurrentValue
		{
			get
			{
				return this.Text;
			}
			set
			{
				DependencyPropertyUtilities.SetCurrentValue(this, TextProperty, value);
			}
		}

		private static void OnTextChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			ValueEditor valueEditor = (ValueEditor)dependencyObject;

			// Don't syncrhonize between properties during the process of initialization 
			// since other properties like ValueType may or may not have been set yet.
			// 
			if ( ! valueEditor._initialized )
				return;

			string newValue = (string)e.NewValue;

			// SSP 5/31/11 TFS57173
			// If we are setting the Text in the SyncTextWithValueHelper method then don't recursively
			// synchronize the value to that text since the text is based on the current value.
			// Enclosed the existing code in the if block.
			// 
			string textBeingSynced = (string)valueEditor.GetValue( InSyncTextWithValueProperty );
			if ( null == textBeingSynced || newValue != textBeingSynced )
				valueEditor.SyncValueProperties( e.Property, newValue );

			valueEditor.OnTextChanged( (string)e.OldValue, newValue );
		}

		/// <summary>
		/// Event ID for the 'TextChanged' routed event
		/// </summary>
		public static readonly RoutedEvent TextChangedEvent =
				EventManager.RegisterRoutedEvent( "TextChanged", RoutingStrategy.Bubble, typeof( RoutedPropertyChangedEventHandler<string> ), typeof( ValueEditor ) );


		/// <summary>
		/// Called when property 'Text' changes
		/// </summary>
		protected virtual void OnTextChanged( string previousValue, string currentValue )
		{
			RoutedPropertyChangedEventArgs<string> newEvent = new RoutedPropertyChangedEventArgs<string>( previousValue, currentValue );
			newEvent.RoutedEvent = ValueEditor.TextChangedEvent;
			newEvent.Source = this;

			
			
			
			this.RaiseValuePropertyChangedEvent( newEvent );
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
		//[Description( "Occurs when property 'Text' changes" )]
		//[Category( "Behavior" )]
		public event RoutedPropertyChangedEventHandler<string> TextChanged
		{
			add
			{
				base.AddHandler( ValueEditor.TextChangedEvent, value );
			}
			remove
			{
				base.RemoveHandler( ValueEditor.TextChangedEvent, value );
			}
		}
		
		private static object OnCoerceText( DependencyObject dependencyObject, object valueAsObject )
		{
			ValueEditor valueEditor = (ValueEditor)dependencyObject;
			string text = (string)valueAsObject;

			// Don't coerce during the process of initialization since ValueType may or 
			// may not have been set yet.
			// 
			if ( ! valueEditor._initialized )
				return text;

			return valueEditor.OnCoerceText( text );
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
		public static readonly DependencyProperty ValueTypeProperty = DependencyProperty.Register(
			"ValueType",
			typeof( Type ),
			typeof( ValueEditor ),
			new FrameworkPropertyMetadata( typeof( string ), FrameworkPropertyMetadataOptions.None,
				new PropertyChangedCallback( OnValueTypeChanged )
				
				
				
				
				
				
				
				
			) );

		
		
		
		
		
		
		
		
		//private Type _cachedValueType = typeof(string);
		private Type _cachedValueType;

		
		
		
		
		
		
		
		
#region Infragistics Source Cleanup (Region)











#endregion // Infragistics Source Cleanup (Region)


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
		/// any constraints specified via this <see cref="ValueEditor.ValueConstraint"/> property.
		/// </para>
		/// <seealso cref="ValueEditor.Value"/>
		/// <seealso cref="ValueEditor.Text"/>
		/// <seealso cref="ValueEditor.ValueConstraint"/>
		/// <seealso cref="ValueEditor.IsValueValid"/>
		/// <seealso cref="ValueEditor.HasValueChanged"/>
		/// <seealso cref="ValueEditor.ValueToTextConverter"/>
		/// <seealso cref="TextEditorBase.ValueToDisplayTextConverter"/>
		/// </remarks>
		//[Description( "Gets/sets the type of values the editor manipulates" )]
		//[Category( "Data" )]
		public Type ValueType
		{
			get
			{
				// JJD 4/27/07
				// Optimization - use the locally cached property 
				//return (Type)this.GetValue( ValueTypeProperty );
				return this._cachedValueType;
			}
			set
			{
				this.SetValue( ValueTypeProperty, value );
			}
		}

		private static void OnValueTypeChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			ValueEditor editor = (ValueEditor)dependencyObject;
			Type newType = (Type)e.NewValue;

			
			
			
			
			
			
			
			editor._cachedValueType = newType;

			editor.OnValueTypeChanged( newType );
		}

		/// <summary>
		/// Called when the value of <b>ValueType</b> property changes.
		/// </summary>
		/// <param name="newType"></param>
		protected virtual void OnValueTypeChanged( Type newType )
		{
			// SSP 10/5/11 TFS90162
			// If ValueType is changed, make sure the Value property's value is of that type.
			// 
			if ( this.InternalIsInitialized )
			{
				object value = this.Value;
				if ( null != value )
					this.CoerceValue( ValueProperty );
			}
		}

		/// <summary>
		/// Returns true if the ValueType property is set to a non-default value.
		/// </summary>
		public bool ShouldSerializeValueType( )
		{
			return Utilities.ShouldSerialize( ValueTypeProperty, this );
		}

		/// <summary>
		/// Resets the ValueType property to its default value.
		/// </summary>
		public void ResetValueType( )
		{
			this.ClearValue( ValueTypeProperty );
		}

				#endregion // ValueType

				#region ValueConstraint

		/// <summary>
		/// Identifies the <see cref="ValueConstraint"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty ValueConstraintProperty = DependencyProperty.Register(
			"ValueConstraint",
			typeof( ValueConstraint ),
			typeof( ValueEditor ),
			new FrameworkPropertyMetadata( null, FrameworkPropertyMetadataOptions.None,
				
				
				
				
				
				
				
				
#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

				new PropertyChangedCallback( OnValueConstraintChanged )
			) 
		);

		// JJD 4/27/07
		// Optimization - cache the property locally
		private ValueConstraint _cachedValueConstraint;

		
		
		
		
		
		
		
		
#region Infragistics Source Cleanup (Region)











#endregion // Infragistics Source Cleanup (Region)


		
		
		
		
		
		private static void OnValueConstraintChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			ValueEditor editor = (ValueEditor)dependencyObject;
			ValueConstraint newVal = (ValueConstraint)e.NewValue;

			
			
			
			
			editor.InternalSetCachedValueConstraintHelper( newVal );
		}

		
		
		
		private void InternalSetCachedValueConstraintHelper( ValueConstraint newVal )
		{
			ValueConstraint oldValueConstraint = _cachedValueConstraint;

			if ( null != oldValueConstraint )
			{
				// SSP 3/24/10 TFS27839
				// 
				//oldValueConstraint.PropertyChanged -= new PropertyChangedEventHandler( this.OnValueConstraint_SubPropChanged );
				PropertyChangedEventManager.RemoveListener( oldValueConstraint, this, string.Empty );
			}

			_cachedValueConstraint = newVal;

			if ( null != newVal )
			{
				// SSP 3/24/10 TFS27839
				// 
				//newVal.PropertyChanged += new PropertyChangedEventHandler( this.OnValueConstraint_SubPropChanged );
				PropertyChangedEventManager.AddListener( newVal, this, string.Empty );
			}

			if ( _initialized )
				this.OnValueConstraintChanged( null );
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
		public static readonly DependencyProperty InvalidValueBehaviorProperty = DependencyProperty.Register(
			"InvalidValueBehavior",
			typeof( InvalidValueBehavior ),
			typeof( ValueEditor ),
			new FrameworkPropertyMetadata( InvalidValueBehavior.Default,
				
				
				
				
				
				
				
				
				new PropertyChangedCallback( OnInvalidValueBehaviorChanged )
			) );

		
		
		
		
		
		
		
		private InvalidValueBehavior _cachedInvalidValueBehavior;

		
		
		
		
		
		
		
		
#region Infragistics Source Cleanup (Region)











#endregion // Infragistics Source Cleanup (Region)


		
		
		
		
		
		private static void OnInvalidValueBehaviorChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			ValueEditor editor = (ValueEditor)dependencyObject;
			InvalidValueBehavior newVal = (InvalidValueBehavior)e.NewValue;

			editor._cachedInvalidValueBehavior = newVal;
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
		/// <see cref="ValueEditor.ValueType"/> property, then the value is considered invalid. For
		/// example, if the ValueType is set to <i>Int32</i> or any other numeric type and the user 
		/// enteres a non-numeric text then the text can not be parsed into the value type. As a result
		/// the editor will consider the input invalid.
		/// </para>
		/// <para class="body">
		/// Another way the value can be considered invalid is if the entered value can not satisfy
		/// constraints specified by <see cref="ValueEditor.ValueConstraint"/> object. For example, if
		/// <see cref="Infragistics.Windows.Editors.ValueConstraint.MinInclusive"/> is specified as 10 and the value entered is 8
		/// then the value does not satisfy the constraints and thus will be considred invalid.
		/// </para>
		/// <seealso cref="ValueEditor.ValueType"/>
		/// <seealso cref="ValueEditor.ValueConstraint"/>
		/// <seealso cref="Infragistics.Windows.Editors.ValueConstraint.Nullable"/>
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
			return Utilities.ShouldSerialize( InvalidValueBehaviorProperty, this );
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
		public static readonly DependencyProperty ValueToTextConverterProperty = DependencyProperty.Register(
  			"ValueToTextConverter",
			typeof( IValueConverter ),
			typeof( ValueEditor ),
			new FrameworkPropertyMetadata( null, FrameworkPropertyMetadataOptions.None ) 
			);

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
		/// <seealso cref="TextEditorBase.ValueToDisplayTextConverter"/>
		/// <seealso cref="ValueEditor.FormatProvider"/>
		/// <seealso cref="ValueEditor.Format"/>
		/// <seealso cref="ValueEditor.ValueType"/>
		/// <seealso cref="ValueEditor.Value"/>
		/// <seealso cref="ValueEditor.Text"/>
		/// <seealso cref="TextEditorBase.DisplayText"/>
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

		private static readonly DependencyPropertyKey IsValueValidPropertyKey = DependencyProperty.RegisterReadOnly(
  			"IsValueValid",
			typeof( bool ),
			typeof( ValueEditor ),
			new FrameworkPropertyMetadata( KnownBoxes.TrueBox, FrameworkPropertyMetadataOptions.None

            // JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
                , new PropertyChangedCallback(OnVisualStatePropertyChanged)

            ) 
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

			if ( null != _host )
				_host.OnValueValidated( );
		}

				#endregion // IsValueValid

				// JM 08-25-11 TFS84624 - Removed this property - there is no need for us to cache the RenderTemplate since we have 
				// reworked the logic to always Coerce the Template property rather than Set it directly which preserves the 
				// underlying value (i.e., the 'render' template) of the Template property.  We then return either this.EditTemplate
				// or 'value' inside the Coerce of the Template property depending on whether we are in edit mode or not.
				#region CachedRenderTemplate

			//    private static readonly DependencyProperty CachedRenderTemplateProperty = DependencyProperty.Register("CachedRenderTemplate", 
			//typeof(ControlTemplate), typeof(ValueEditor));

				#endregion //CachedRenderTemplate

				#region EditTemplate

		/// <summary>
		/// Identifies the 'EditTemplate' dependency property
		/// </summary>
		public static readonly DependencyProperty EditTemplateProperty = DependencyProperty.Register("EditTemplate",
				typeof(ControlTemplate), typeof(ValueEditor), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnEditTemplateChanged)));

		/// <summary>
		/// Event ID for the 'EditTemplateChanged' routed event
		/// </summary>
		public static readonly RoutedEvent EditTemplateChangedEvent =
				EventManager.RegisterRoutedEvent("EditTemplateChanged", RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<ControlTemplate>), typeof(ValueEditor));

		private static void OnEditTemplateChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			ValueEditor control = target as ValueEditor;

			if (control != null)
			{
				ControlTemplate newValue = (ControlTemplate)e.NewValue;
				if (newValue != control._cachedEditTemplate)
				{
					ControlTemplate oldValue = control._cachedEditTemplate;
					control._cachedEditTemplate = newValue;

					// JM 08-25-11 TFS84624 - Coerce the Template property so that this change to the EditTemplate gets
					// applied if we are in edit mode.
					control.CoerceValue(TemplateProperty);

					control.OnEditTemplateChanged(oldValue, newValue);
				}
			}
		}

		private ControlTemplate _cachedEditTemplate;

		/// <summary>
		/// Template used while the editor is in edit mode.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// ValueEditor exposes two ControlTemplate properties: <b>Template</b> property which it inherits from the base
		/// Control class and the <b>EditTemplate</b> property. Value of Template property is used when the editor is 
		/// in display mode where as the value of EditTemplate property is used when the editor is in edit mode.
		/// If EditTemplate is not specified then the editor will use the value of Template property for both edit and
		/// display mode. This means that the Template property is required however EditTemplate could be left off to 
		/// null. However note that if you do so, the control template specified by the Template property must support
		/// editing as well.
		/// </p>
		/// <p class="body">
		/// As an example, <see cref="XamTextEditor"/>'s Template property uses <b>TextBlock</b> to display the contents.
		/// Its EditTemplate uses a <b>TextBox</b> to display and edit contents since EditTemplate has to support editing
		/// as well. TextBlock is more efficient than TextBox and therefore this kind of configuration can result in
		/// greater rendering speeds, esp. when a lot of editor instances are to be rendered like in a XamDataGrid where
		/// each cell has an instance of ValueEditor. In a control like XamDataGrid, only one cell can be in edit mode at a
		/// time and therefore only one ValueEditor instance in the XamDataGrid will use EditTemplate at a time while the 
		/// rest will use the more efficient control template provided by the Template property.
		/// </p>
		/// <p class="body">
		/// <b>Note:</b> The editors provide default values for Template and EditTemplate properties. You don't need
		/// to typically set these properties unless you want to change the default control templates used by an editor.
		/// </p>
		/// <para class="body">
		/// <b>Note:</b> When the editor enters edit mode the Template property will be set to the value of
		/// EditTemplate to apply the edit template. However since the editor needs to switch back to the origianl value 
		/// of the Template property when the editor exits edit mode, the editor stores the original value
		/// of Template property so it can revert back to it when the editor exits the edit mode.
		/// </para>
		/// </remarks>
		//[Description("Template used while the editor is in edit mode")]
		//[Category("Behavior")]
		[Bindable(true)]
		public ControlTemplate EditTemplate
		{
			get
			{
				return this._cachedEditTemplate;
			}
			set
			{
				this.SetValue(ValueEditor.EditTemplateProperty, value);
			}
		}

		/// <summary>
		/// Called when property 'EditTemplate' changes
		/// </summary>
		/// <remarks>
		/// <seealso cref="EditTemplate"/>
		/// <seealso cref="EditTemplateChanged"/>
		/// </remarks>
		protected virtual void OnEditTemplateChanged(ControlTemplate previousValue, ControlTemplate currentValue)
		{
			RoutedPropertyChangedEventArgs<ControlTemplate> newEvent = new RoutedPropertyChangedEventArgs<ControlTemplate>(previousValue, currentValue);
			newEvent.RoutedEvent = ValueEditor.EditTemplateChangedEvent;
			newEvent.Source = this;

			// MD 7/16/10 - TFS26592
			// Call off to the new helper method to raise the event. 
			//RaiseEvent(newEvent);
			RaiseEventHelper(newEvent);
		}

		/// <summary>
		/// Occurs when property 'EditTemplate' changes
		/// </summary>
		/// <remarks>
		/// <seealso cref="EditTemplate"/>
		/// </remarks>
		//[Description("Occurs when property 'EditTemplate' changes")]
		//[Category("Behavior")]
		public event RoutedPropertyChangedEventHandler<ControlTemplate> EditTemplateChanged
		{
			add
			{
				base.AddHandler(ValueEditor.EditTemplateChangedEvent, value);
			}
			remove
			{
				base.RemoveHandler(ValueEditor.EditTemplateChangedEvent, value);
			}
		}

				#endregion //EditTemplate

				#region HasValueChanged

		private static readonly DependencyPropertyKey HasValueChangedPropertyKey = DependencyProperty.RegisterReadOnly(
			"HasValueChanged",
			typeof( bool ),
			typeof( ValueEditor ),
			new FrameworkPropertyMetadata( KnownBoxes.FalseBox, FrameworkPropertyMetadataOptions.None )
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
		/// <seealso cref="IsInEditMode"/>
		/// <seealso cref="EndEditMode"/>
		/// <seealso cref="EditModeEnding"/>
		/// <seealso cref="EditModeEnded"/>
		/// <seealso cref="OriginalValue"/>
		/// <seealso cref="Value"/>
		[Browsable( false )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		[Bindable( false )]
		[ReadOnly( true )]
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
		private static readonly DependencyPropertyKey InvalidValueErrorInfoPropertyKey = DependencyProperty.RegisterReadOnly(
			"InvalidValueErrorInfo",
			typeof( ValidationErrorInfo ),
			typeof( ValueEditor ),
			new FrameworkPropertyMetadata( null, FrameworkPropertyMetadataOptions.None )
		);

		/// <summary>
		/// Identifies the read-only <see cref="InvalidValueErrorInfo"/> dependency property.
		/// </summary>
		[InfragisticsFeature( FeatureName = FeatureInfo.FeatureName_IDataErrorInfo, Version = FeatureInfo.Version_9_2 )]
		public static readonly DependencyProperty InvalidValueErrorInfoProperty = InvalidValueErrorInfoPropertyKey.DependencyProperty;

		/// <summary>
		/// If the editor's value is invalid, returns error information regarding why it's invalid.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// When the editor's value is invalid, <see cref="ValueEditor.IsValueValid"/> property returns
		/// false. To get the error information regarding why the value is invalid, use the 
		/// <see cref="ValueEditor.InvalidValueErrorInfo"/> property which returns an instance 
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
		[InfragisticsFeature( FeatureName = FeatureInfo.FeatureName_IDataErrorInfo, Version = FeatureInfo.Version_9_2 )]
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
		/// <seealso cref="TextEditorBase.NullText"/>
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

				#region IsAlwaysInEditMode

		/// <summary>
		/// Identifies the <see cref="IsAlwaysInEditMode"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsAlwaysInEditModeProperty = DependencyProperty.Register("IsAlwaysInEditMode",
			typeof(bool), typeof(ValueEditor), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		/// <summary>
		/// Gets/sets whether this editor is always in edit mode but is ignored when the editor is embedded inside another control.
		/// Default value is <b>False</b>.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// This property defaults to <b>False</b>. When set to False, the editor will exit edit mode 
		/// when the control looses focus and re-enter edit mode when it receives focus. As a result
		/// it will also raise the <see cref="EditModeStarting"/>, <see cref="EditModeStarted"/>, 
		/// <see cref="EditModeEnding"/> and <see cref="EditModeEnded"/> events.
		/// </para>
		/// <para class="body">
		/// If you set this property to <b>True</b>, the editor will always remain in edit mode.
		/// However note that this property is ignored when the editor is embedded inside another 
		/// control (i.e. is hosted inside a <see cref="ValuePresenter"/>). When it's embedded,
		/// it will exit edit mode as directed by the hosting control.
		/// </para>
		/// </remarks>
		/// <seealso cref="IsAlwaysInEditModeProperty"/>
		/// <seealso cref="IsInEditModeProperty"/>
		/// <seealso cref="IsInEditMode"/>
		//[Description("Gets/sets whther this editor is always in edit mode")]
		//[Category("Behavior")]
		[Bindable(true)]
		public bool IsAlwaysInEditMode
		{
			get
			{
				return (bool)this.GetValue(ValueEditor.IsAlwaysInEditModeProperty);
			}
			set
			{
				this.SetValue(ValueEditor.IsAlwaysInEditModeProperty, value);
			}
		}

				#endregion //IsAlwaysInEditMode

				#region IsEmbedded

		private static readonly DependencyPropertyKey IsEmbeddedPropertyKey =
			DependencyProperty.RegisterReadOnly("IsEmbedded",
			typeof(bool), typeof(ValueEditor), new FrameworkPropertyMetadata(KnownBoxes.FalseBox

                // JJD 4/12/10 - NA2010 Vol 2 - Added support for VisualStateManager
                , new PropertyChangedCallback(OnVisualStatePropertyChanged)

            )
            );

		/// <summary>
		/// Identifies the <see cref="IsEmbedded"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsEmbeddedProperty =
			IsEmbeddedPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns true if the editor is embedded in another control, e.g. a DataPresenter cell.
		/// </summary>
		/// <seealso cref="IsEmbeddedProperty"/>
		//[Description("Returns true if the editor is embedded in another control,e.g. a DataPresenter cell.")]
		//[Category("Behavior")]
		[Bindable(true)]
		[ReadOnly(true)]
		public bool IsEmbedded
		{
			get
			{
				return (bool)this.GetValue(ValueEditor.IsEmbeddedProperty);
			}
		}

				#endregion //IsEmbedded

				#region IsFocusWithin

		// SSP 2/3/10 TFS24689
		// Added owner to allow for being able to use IsFocusWithin in XAML.
		// 
		/// <summary>
		/// Identifies the <see cref="IsFocusWithin"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty IsFocusWithinProperty =
			FocusWithinManager.IsFocusWithinProperty.AddOwner( typeof( ValueEditor ) );

		/// <summary>
		/// Returns true if the focus is within this element.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// This property returns true if this element or a descendant element has focus.
		/// Note that this does not check for keyboard focus. It checks for logical focus
		/// as managed by the <see cref="FocusManager"/> class.
		/// </para>
		/// </remarks>
		[Browsable( false )]
		[ReadOnly( true )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public bool IsFocusWithin
		{
			get
			{
				return (bool)this.GetValue( FocusWithinManager.IsFocusWithinProperty );
			}
		}

				#endregion // IsFocusWithin

				#region IsInEditMode

		/// <summary>
		/// Identifies the <see cref="IsInEditMode"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsInEditModeProperty = DependencyProperty.Register("IsInEditMode",
			typeof(bool), typeof(ValueEditor), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, null, new CoerceValueCallback(CoerceIsInEditMode)));

		private static object CoerceIsInEditMode(DependencyObject target, object value)
		{
			ValueEditor editor = target as ValueEditor;

			Debug.Assert(editor != null);

			if (editor != null && value is bool)
			{
				// compare the new value against the cached member value
				if ( editor._isInEditMode != (bool)value )
				{
					// call either StartEditMode or EndEditMode
					if ((bool)value == true)
						editor.StartEditMode();
					else
						editor.EndEditMode(true, false);
				}

				// return the cached value in case the operation was cancelled
				return editor._isInEditMode;
			}

			return KnownBoxes.FalseBox;
		}

		/// <summary>
		/// Gets/sets whether this editor is in edit mode.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// Note that setting this property will cause the editor to enter or exit edit mode
		/// depending on the set value.
		/// </p>
		/// </remarks>
		/// <seealso cref="IsInEditModeProperty"/>
		/// <seealso cref="StartEditMode()"/>
		/// <seealso cref="EndEditMode"/>
		//[Description("Gets/sets whether this element is being edited")]
		//[Category("Behavior")]
		[Bindable(true)]
		public bool IsInEditMode
		{
			get
			{
				return (bool)this.GetValue(ValueEditor.IsInEditModeProperty);
			}
			set
			{
				this.SetValue(ValueEditor.IsInEditModeProperty, value);
			}
		}

				#endregion //IsInEditMode

				#region IsReadOnly

		
		
		

		/// <summary>
		/// Identifies the <see cref="IsReadOnly"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register(
			"IsReadOnly",
			typeof( bool ),
			typeof( ValueEditor ),
			new FrameworkPropertyMetadata( KnownBoxes.FalseBox, new PropertyChangedCallback( OnIsReadOnlyChanged ) )
			);

		private static void OnIsReadOnlyChanged( DependencyObject target, DependencyPropertyChangedEventArgs e )
		{
			ValueEditor editor = (ValueEditor)target;
#pragma warning disable 618
			// SSP 6/6/08 BR32918
			// Only set the value if it's not the same. Otherwise any one-way binding with the 
			// property will not work.
			// 
			if ( ! object.Equals( editor.GetValue( ValueEditor.ReadOnlyProperty ), e.NewValue ) )
				editor.SetValue( ValueEditor.ReadOnlyProperty, e.NewValue );


            // JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
            editor.UpdateVisualStates();


#pragma warning restore 618
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
			return Utilities.ShouldSerialize( IsReadOnlyProperty, this );
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
		protected static readonly DependencyPropertyKey OriginalValuePropertyKey =
			DependencyProperty.RegisterReadOnly("OriginalValue",
			typeof(object), typeof(ValueEditor), new FrameworkPropertyMetadata());

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
		/// Note that <b>OriginalValue</b> property is only valid during edit mode.
		/// This property is reset when the editor re-enters edit mode. If 
		/// <see cref="IsAlwaysInEditMode"/> is set to True, the OriginalValue
		/// is also reset when the editor receives focus.
		/// Essentially the <b>OriginalValue</b> property keeps track of the 
		/// original value when the user enters the editor. This is used to revert 
		/// back to the original value if the user decides to cancel the edit 
		/// operation.
		/// </para>
		/// </remarks>
		/// <seealso cref="OriginalValueProperty"/>
		//[Description("Gets the original value while in edit mode (read-only)")]
		//[Category("Data")]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		[Bindable(true)]
		[ReadOnly(true)]
		public object OriginalValue
		{
			get
			{
				return (object)this.GetValue(ValueEditor.OriginalValueProperty);
			}
		}

				#endregion //OriginalValue

				#region ReadOnly

		/// <summary>
		/// Identifies the <see cref="ReadOnly"/> dependency property.
		/// </summary>
		[Obsolete( "This property has been obsoleted. Use 'IsReadOnly' property instead.", false )]
		[EditorBrowsable( EditorBrowsableState.Never ), Browsable( false )]
		public static readonly DependencyProperty ReadOnlyProperty = DependencyProperty.Register(
			"ReadOnly",
			typeof( bool ),
			typeof( ValueEditor ),
			new FrameworkPropertyMetadata( KnownBoxes.FalseBox, new PropertyChangedCallback( OnReadOnlyChanged ) )
			);

		private static void OnReadOnlyChanged( DependencyObject target, DependencyPropertyChangedEventArgs e )
		{
			ValueEditor editor = (ValueEditor)target;

			// SSP 6/6/08 BR32918
			// Only set the value if it's not the same. Otherwise any one-way binding with the 
			// property will not work.
			// 
			if ( !object.Equals( editor.GetValue( ValueEditor.IsReadOnlyProperty ), e.NewValue ) )
				editor.SetValue( ValueEditor.IsReadOnlyProperty, e.NewValue );
		}

		/// <summary>
		/// Specifies whether the user is allowed to modify the value in the editor. Default value is <b>false</b>.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// If <b>ReadOnly</b> is set to <b>True</b> the user will not be allowed to modify the
		/// value of the editor. However note that you will still be able to modify the value of 
		/// the editor in code via for example its <see cref="Value"/> property.
		/// The default value of this property is <b>False</b>.
		/// </para>
		/// </remarks>
		//[Description( "Specifies whether the user is allowed to modify the value" )]
		//[Category( "Behavior" )]
		
		
		
		[Obsolete( "This property has been obsoleted. Use 'IsReadOnly' property instead.", false )]
		[EditorBrowsable( EditorBrowsableState.Never ), Browsable( false )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
#pragma warning disable 0618
        public bool ReadOnly
		{
			get
			{
				return (bool)this.GetValue( ReadOnlyProperty );
			}
			set
			{
				this.SetValue( ReadOnlyProperty, value );
			}
		}

		/// <summary>
		/// Returns true if the ReadOnly property is set to a non-default value.
		/// </summary>
		[Obsolete( "This method has been obsoleted. Use 'ShouldSerializeIsReadOnly' method instead.", false )]
		[EditorBrowsable( EditorBrowsableState.Never ), Browsable( false )]
		public bool ShouldSerializeReadOnly( )
		{
			
			
			
			
			return false;
		}

		/// <summary>
		/// Resets the ReadOnly property to its default value of <b>false</b>.
		/// </summary>
		[Obsolete( "This method has been obsoleted. Use 'ResetIsReadOnly' method instead.", false )]
		[EditorBrowsable( EditorBrowsableState.Never ), Browsable( false )]
		public void ResetReadOnly( )
		{

			this.ClearValue( ReadOnlyProperty );
		}
#pragma warning restore 0618

				#endregion // ReadOnly

				#region Theme

		#region Old Version
		
#region Infragistics Source Cleanup (Region)








































#endregion // Infragistics Source Cleanup (Region)

		#endregion //Old Version

		/// <summary>
		/// Identifies the <see cref="Theme"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ThemeProperty = ThemeManager.ThemeProperty.AddOwner(typeof(ValueEditor), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnThemeChangedCallback)));

		private static void OnThemeChangedCallback(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			ValueEditor editor = target as ValueEditor;

			editor.OnThemeChanged((string)(e.OldValue), (string)(e.NewValue));
		}

		/// <summary>
		/// Called when the <see cref="Theme"/> property has changed
		/// </summary>
		/// <param name="newTheme"></param>
		/// <param name="oldTheme"></param>
		protected virtual void OnThemeChanged(string newTheme, string oldTheme)
		{
		}

		/// <summary>
		/// Gets/sets the theme of the control
		/// </summary>
		/// <seealso cref="ThemeProperty"/>
		//[Description("Gets/sets the theme of the control")]
		//[Category("Appearance")]
		[Bindable(true)]
		[TypeConverter(typeof(Infragistics.Windows.Themes.Internal.EditorsThemeTypeConverter))]
		public string Theme
		{
			get
			{
				return (string)this.GetValue(ValueEditor.ThemeProperty);
			}
			set
			{
				this.SetValue(ValueEditor.ThemeProperty, value);
			}
		}

				#endregion //Theme

				#region FormatProvider

		/// <summary>
		/// Identifies the <see cref="FormatProvider"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty FormatProviderProperty = DependencyProperty.Register(
			"FormatProvider",
			typeof( IFormatProvider ),
			typeof( ValueEditor ),
			new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None,
				
				
				
				
				
				
				
				
				
				
				
				new PropertyChangedCallback( OnFormatProviderChanged )
				)
			);

		// JJD 4/27/07
		// Optimization - cache the property locally
		private IFormatProvider _cachedFormatProvider;

		
		
		
		
		
		
		
		
#region Infragistics Source Cleanup (Region)











#endregion // Infragistics Source Cleanup (Region)


		
		
		
		
		
		private static void OnFormatProviderChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			ValueEditor editor = (ValueEditor)dependencyObject;
			IFormatProvider newVal = (IFormatProvider)e.NewValue;

			editor._cachedFormatProvider = newVal;

			// SSP 6/2/09 TFS17233
			// Added caching for FormatProviderResolved property.
			// 
			editor._cachedFormatProviderResolved = null;
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
		/// <see cref="ValueEditor.ValueToTextConverter"/> and <see cref="TextEditorBase.ValueToDisplayTextConverter"/>
		/// properties.
		/// </para>
		/// <para class="body">
		/// <b>Note:</b> <b>FormatProvider</b> property is of type <b>IFormatProvider</b> interface. IFormatProvider 
		/// is implemented by <b>CultureInfo</b> object therefore this property can be set to an instance of 
		/// <b>CultureInfo</b>. You can also use <b>DateTimeFormatInfo</b> or <b>NumberFormatInfo</b> as these 
		/// implement the interface as well.
		/// </para>
		/// <seealso cref="ValueEditor.ValueToTextConverter"/> 
		/// <seealso cref="TextEditorBase.ValueToDisplayTextConverter"/>
		/// <seealso cref="ValueEditor.Format"/>
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
			return Utilities.ShouldSerialize( FormatProviderProperty, this );
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
		public static readonly DependencyProperty FormatProperty = DependencyProperty.Register(
			"Format",
			typeof( string ),
			typeof( ValueEditor ),
			new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None,
				
				
				
				
				
				
				
				
#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

				new PropertyChangedCallback( OnFormatChanged )
				)
			);

		// JJD 4/27/07
		// Optimization - cache the property locally
		private string _cachedFormat;

		
		
		
		
		
		
		
		
#region Infragistics Source Cleanup (Region)











#endregion // Infragistics Source Cleanup (Region)


		
		
		
		
		
		private static void OnFormatChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			ValueEditor editor = (ValueEditor)dependencyObject;
			string newVal = (string)e.NewValue;

			editor._cachedFormat = newVal;
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
		/// custom conversion logic, use the <see cref="ValueEditor.ValueToTextConverter"/>
		/// and <see cref="TextEditorBase.ValueToDisplayTextConverter"/> properties.
		/// </para>
		/// </remarks>
		protected virtual IValueConverter DefaultValueToTextConverter
		{
			get
			{
				return ValueEditorDefaultConverter.ValueToTextConverter;
			}
		}

				#endregion // DefaultValueToTextConverter

				#region FocusSite

		/// <summary>
		/// Returns the element in the visual tree that was named 'PART_FocusSite'. 
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// This will be a class derived from FrameworkElement or FrameworkContentElement or null.
		/// </para>
		/// <para class="body">
		/// FocusSite indicates the element that will receive keyboard focus whenever the editor
		/// is in edit mode. This way the keyboard messages get delivered to this element. 
		/// For example, <see cref="XamTextEditor"/> defines a TextBox as PART_FocusSite in its
		/// edit control template. Whenever XamTextEditor is in edit mode, it gives focus to
		/// the TextBox so all the keyboard messages get delivered to the TextBox for processing.
		/// </para>
		/// <para class="body">
		/// Different editors will define different elements as their FocusSite. If you define
		/// a custom edit template for an editor, make sure the element that will perform the 
		/// editing in response to keyboard messages is named PART_FocusSite so the editor
		/// knows which element to give keyboard focus when it receives the focus.
		/// </para>
		/// </remarks>
		/// <seealso cref="OnFocusSiteChanged"/>
		/// <seealso cref="EditTemplate"/>
		/// <seealso cref="IsFocusWithin"/>
		protected DependencyObject FocusSite { get { return this._focusSite; } }

				#endregion //FocusSite

                // JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
                // Moved HasDropDown and HasOpenDropDown properties up from TextEditorBase class 
                // AS 9/10/08 NA 2008 Vol 2
                #region HasDropDown
                internal virtual bool HasDropDown
                {
                    get { return false; }
                }
                #endregion //HasDropDown

                // JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
                // Moved HasDropDown and HasOpenDropDown properties up from TextEditorBase class 
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

				#region Host

		/// <summary>
		/// Returns the associated <see cref="ValuePresenter"/> if any.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// If this editor is embedded inside another control like XamDataGrid's cell for example,
		/// the <b>Host</b> property will return the <see cref="ValuePresenter"/> element that's 
		/// hosting the editor. If this editor is being used as a standalone control then this 
		/// property will return null.
		/// </para>
		/// </remarks>
		/// <seealso cref="IsEmbedded"/>
		/// <seealso cref="ValuePresenter"/>
		public ValuePresenter Host { get { return this._host; } }

				#endregion //Host
		
				// JJD 6/29/11 - TFS79601 - added
				#region SupportsAsyncOperations

		/// <summary>
		/// Determines if asynchronous operations are supported (read-only)
		/// </summary>
		/// <value>True if asynchronous operations are supported, otherwise false.</value>
		/// <remarks>
		/// <para class="body">This property returns false during certain operations that are synchronous in nature, e.g. during a report or export operation.</para>
		/// </remarks>
		protected bool SupportsAsyncOperations 
		{ 
			get 
			{
				if (_host != null)
					return _host.SupportsAsyncOperations;

				return !Reporting.ReportSection.GetIsInReport(this); 
			} 
		}

				#endregion //SupportsAsyncOperations	
    
			#endregion //Protected Properties	
        
		#endregion //Properties
		
		#region Methods

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
		/// <see cref="ValueEditor.ValueType"/> property. ValueType property can be set to
		/// any type as long as there is conversion logic for converting between the native
		/// data type of the editor and that type. For example, <see cref="XamTextEditor"/>
		/// natively supports editing string type only. However its ValueType can be set to
		/// Double or DateTime or any type as long as the editor can convert between string
		/// and that data type. ValueType can even be set to a custom type. You can provide
		/// custom conversion logic using <see cref="ValueEditor.ValueToTextConverter"/>
		/// and <see cref="TextEditorBase.ValueToDisplayTextConverter"/> properties.
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
    
				#region EndEditMode

		/// <summary>
		/// Exits edit mode.
		/// </summary>
		/// <param name="acceptChanges">If True will accept any changes that were made while in edit mode.</param>
		/// <param name="force">If True will prevent the cancellation of the action.</param>
		/// <remarks>
		/// <p class="body">
		/// EndEditMode exits edit mode, validating the user input in the process if 
		/// <paramref name="acceptChanges"/> is True. If acceptChanges is False then
		/// the user input is discarded and the <see cref="ValueEditor.Value"/> is restored
		/// back to the original value.
		/// </p>
		/// <p class="body">
		/// As part of the process of exitting edit mode, <see cref="ValueEditor.EditModeEnding"/> and
		/// <see cref="ValueEditor.EditModeEnded"/> events are raised. You can cancel 
		/// <see cref="ValueEditor.EditModeEnding"/> event to cancel exitting edit mode.
		/// </p>
		/// <p class="body">
		/// <b>Note:</b> Typically there is no need to call this method directly. The editor will automatically
		/// enter and exit edit mode as necessary. For example, when using the editor embedded inside a field of 
		/// a XamDataGrid, the editor is put into edit mode automatically when the cell is clicked and exits edit 
		/// mode when the focus leaves the cell. When using the editor as a stand-alone control, the editor
		/// will automatically enter edit mode when the editor receives focus (for example via a mouse click
		/// or when tabbing into it) and exit edit mode when the editor looses focus. This default behavior
		/// can be controlled using the <see cref="ValueEditor.IsAlwaysInEditMode"/> property.
		/// </p>
		/// <seealso cref="StartEditMode()"/>
		/// <seealso cref="ValueEditor.IsAlwaysInEditMode"/>
		/// </remarks>
		public void EndEditMode(bool acceptChanges, bool force)
		{
			// honor the always in edit mode property if editor is not hosted
			if (this._host == null &&
				 this.IsAlwaysInEditMode == true)
				return;

			if (!this._isInEditMode)
				return;

			if ( _isInEndEditMode )
				return;

			_isInEndEditMode = true;
			try
			{
				// raise the EditModeEnding event
				EditModeEndingEventArgs args = new EditModeEndingEventArgs( acceptChanges, force );
				this.RaiseEditModeEnding( args );

				// SSP 2/10/09 TFS12242
				// Don't validate the value and prompt the user if the user types back in the same value as
				// the original value. Added HasValueChangedInternal method that checks if the current value
				// is the same as the original value.
				// 
				//bool hasValueChanged = this.HasValueChanged;
				bool hasValueChanged = this.HasValueChangedInternal( true );

				// Raise EditModeValidationError if the value in the editor is invalid.
				// 
				if ( !args.Cancel && args.AcceptChanges && !this.IsValueValid
					// SSP 2/6/09 TFS10586
					// Added AlwaysValidate property. If it's set to true then validate regardless of
					// whether the user has modified the value.
					// 
					&& ( hasValueChanged || this.AlwaysValidateResolved )
					)
				{
					EditModeValidationErrorEventArgs validationErrorArgs;
					bool stayInEditMode;
					this.ValidateInputHelper( false, args.Force, out validationErrorArgs, out stayInEditMode );

					// If the validation error handler indicated that we stay in edit mode then cancel
					// exitting edit mode.
					// 
					if ( stayInEditMode && !args.Force )
						args.Cancel = true;
					// If the value is invalid then don't accept the changes.
					// 
					else if ( !this.IsValueValid )
						args.AcceptChanges = false;
				}

				if ( ! args.Cancel && hasValueChanged )
				{
					if ( args.AcceptChanges == false )
					{
						// call the virtual method to revert back to the OriginalValue
						this.RevertValueBackToOriginalValue( );
					}
					else
					{
						// Have the host commit the new edited value. If the commit fails (for example
						// if the data source doesn't accept the value) then stay in edit mode if 
						// requested by the host.
						// 
						if ( null != _host )
						{
							bool stayInEditMode;
							bool commitSucceeded = _host.CommitEditValue( this.Value, out stayInEditMode );
							if ( stayInEditMode && ! args.Force )
								args.Cancel = true;
						}
					}
				}

				if ( args.Cancel && !args.Force )
				{
					// refocus the focus site
					// SSP 3/1/07 BR20788
					// Only focus the focus site if the editor contains the focus otherwise we could
					// get into a recursive situation of loosing and getting focus.
					// 
					if ( this.IsFocusWithin )
						this.SetFocusToFocusSite( );

					return;
				}
				
				// set the cached member 1st so that we don't trigger a recursive call to this 
				// method in the property change of IsInEditMode
				this._isInEditMode = false;

				// now set the property
				this.IsInEditMode = false;

				// Reset the Text property according to non-edit-mode settings (like formatting)
				this.SyncTextWithValue( );

				// raise the EditModeEnded event
				this.RaiseEditModeEnded( new EditModeEndedEventArgs( args.AcceptChanges ) );

				// reset the HasValueChanged to false
				this.SetValue( HasValueChangedPropertyKey, KnownBoxes.FalseBox );
			}
			finally
			{
				_isInEndEditMode = false;
			}
		}

				#endregion //EndEditMode

				#region GetDefaultEditorForType (static)

		/// <summary>
		/// Gets the default editor for a data type.
		/// </summary>
		/// <param name="dataType">The type of the data to be edited.</param>
		/// <returns>A type that derives from ValueEditor or null if no editor has been registered for the data type.</returns>
		/// <remarks>
		/// <para class="body">
		/// GetDefaultEditorForType returns the editor that will be used by default for the specified
		/// data type. ValueEditor maintains a static table that associates a data type with the default
		/// editor to use to edit that data type. This mapping is used by the controls that embedd editors in them
		/// like the XamDataGrid. XamDataGrid will use this table to determine which editor to use for each of its
		/// fields based on the field's data type. For example, for a field of Double type, the XamDataGrid
		/// will query this method to determine the editor to use for the field if none has been explicitly
		/// specified.
		/// </para>
		/// <para class="body">
		/// The data type to editor mapping is many to one mapping. That is multiple data types can be registered
		/// to make use of a single editor. However a single data type can not be mapped to multiple editors.
		/// </para>
		/// <para class="body">
		/// You can change the mappings using ValueEditor's <see cref="RegisterDefaultEditorForType"/> static
		/// method.
		/// </para>
		/// <seealso cref="RegisterDefaultEditorForType"/>
		/// </remarks>
		public static Type GetDefaultEditorForType(Type dataType)
		{
			if (dataType == null)
				throw new ArgumentNullException("dataType");

			Monitor.Enter(typeof(ValueEditor));

			try
			{
				if (g_defaultEditors == null)
					return null;

				if (g_defaultEditors.ContainsKey(dataType))
					return g_defaultEditors[dataType];

				Type nullableUnderlyingType = Nullable.GetUnderlyingType( dataType );
				if ( null != nullableUnderlyingType 
					&& dataType != nullableUnderlyingType
					&& g_defaultEditors.ContainsKey( nullableUnderlyingType ) )
					return g_defaultEditors[nullableUnderlyingType];

                // JJD 2/7/08 - BR30444
                // Don't use an editor for ImageSource 
                if (typeof(ImageSource).IsAssignableFrom(dataType))
                    return null;

				// If the data type can be converted to and from string then return XamTextEditor
				// as the default editor.
				TypeConverter typeConverter = TypeDescriptor.GetConverter( dataType );
				if ( null != typeConverter
					&& typeConverter.CanConvertFrom( typeof( string ) )
					&& typeConverter.CanConvertTo( typeof( string ) ) )
					return typeof( XamTextEditor );

				return null;
			}
			finally
			{
				Monitor.Exit(typeof(ValueEditor));
			}
		}

				#endregion //GetDefaultEditorForType	

				#region InitializeHostInfo

		/// <summary>
		/// Initializes the editor with the associated host information when it is 
		/// embedded in another control.
		/// </summary>
		/// <param name="host">The ValuePresenter that will be hosting this editor.</param>
		/// <param name="hostContext">Some data that has meaning to the host which is passed back to the host when calling methods on the ValuePresenter class.</param>
		public void InitializeHostInfo(ValuePresenter host, object hostContext)
		{
			this._host = host;
			this._hostContext = hostContext;

			// if we are hosted then we can't always be in edit mode and we need to default to not being in edit mode
			if (this._host != null)
			{
				
				
				
				
				
				
				
				




			}

			this.SetValue(IsEmbeddedPropertyKey, KnownBoxes.FromValue(this._host != null));
		}

				#endregion //InitializeHostInfo

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

				#region RegisterDefaultEditorForType (static)

		/// <summary>
		/// Registers a default editor for a data type.
		/// </summary>
		/// <param name="dataType">The type of the data to be edited.</param>
		/// <param name="editorType">The type of the editor to be used - must derive from <see cref="ValueEditor"/>.</param>
		/// <param name="overlayExisting">If true will overlay any previously registered editor for the data type.</param>
		/// <remarks>
		/// <para class="body">
		/// Use RegisterDefaultEditorForType method to change or register default editor that will get used
		/// for a data type. The <paramref name="editorType"/> parameter specifies the type of the editor.
		/// Controls like XamDataGrid making use of this default editor mapping infrastructure will create 
		/// instances of this editor type to edit fields of <paramref name="dataType"/>.
		/// The editorType must be a type that derives from <see cref="ValueEditor"/> class. 
		/// </para>
		/// <para class="body">
		/// You can register custom data types and custom editors as well. You can register a custom data 
		/// type to make use of an existing editor or you can change the editor of an already registered
		/// data type to make use of a custom editor or both.
		/// </para>
		/// <para class="body">
		/// See <see cref="GetDefaultEditorForType"/> for further information.
		/// </para>
		/// <para class="body">
		/// <b>Note:</b> You can register a different editor for a primitive type and Nullable version of
		/// the primitive type. If no editor is explicitly registered for the Nullable version of the type
		/// then the editor for the type will be used.
		/// </para>
		/// <seealso cref="GetDefaultEditorForType"/>
		/// </remarks>
		public static void RegisterDefaultEditorForType(Type dataType, Type editorType, bool overlayExisting)
		{
			if (dataType == null)
				throw new ArgumentNullException("dataType");
			
			if (editorType == null)
				throw new ArgumentNullException("editorType");

			if (typeof(ValueEditor).IsAssignableFrom(dataType))
				throw new ArgumentException( ValueEditor.GetString( "LE_ArgumentException_6" ), "dataType" );

			if( !typeof(ValueEditor).IsAssignableFrom(editorType ))
				throw new ArgumentException( ValueEditor.GetString( "LE_ArgumentException_7" ), "editorType" );

			Monitor.Enter(typeof(ValueEditor));

			try
			{
				if (g_defaultEditors == null)
					g_defaultEditors = new Dictionary<Type, Type>();

				// see if an editor is already registered for this data type
				if (g_defaultEditors.ContainsKey(dataType))
				{
					// see if it should be overlaid
					if (overlayExisting == true)
						g_defaultEditors[dataType] = editorType;
					
					return;
				}

				g_defaultEditors.Add(dataType, editorType);
			}
			finally
			{
				Monitor.Exit(typeof(ValueEditor));
			}
		}

				#endregion //RegisterDefaultEditorForType	
    
				#region StartEditMode

		/// <summary>
		/// Enters edit mode.
		/// </summary>
		/// <returns>Returns True if successful, False otherwise.</returns>
		/// <remarks>
		/// <p class="body">
		/// StartEditMode enters the editor into edit mode.
		/// </p>
		/// <p class="body">
		/// As part of the process of entering edit mode, <see cref="ValueEditor.EditModeStarting"/> and
		/// <see cref="ValueEditor.EditModeStarted"/> events are raised. You can cancel 
		/// <see cref="ValueEditor.EditModeStarting"/> event to cancel entering edit mode.
		/// </p>
		/// <p class="body">
		/// <b>Note:</b> Typically there is no need to call this method directly. The editor will automatically
		/// enter and exit edit mode as necessary. For example, when using the editor embedded inside a field of 
		/// a XamDataGrid, the editor is put into edit mode automatically when the cell is clicked and exits edit 
		/// mode when the focus leaves the cell. When using the editor as a stand-alone control, the editor
		/// will automatically enter edit mode when the editor receives focus (for example via a mouse click
		/// or when tabbing into it) and exit edit mode when the editor looses focus. This default behavior
		/// can be controlled using the <see cref="ValueEditor.IsAlwaysInEditMode"/> property.
		/// </p>
		/// <seealso cref="StartEditMode()"/>
		/// <seealso cref="ValueEditor.IsAlwaysInEditMode"/>
		/// </remarks>
		public bool StartEditMode( )
		{
			return this.StartEditMode( true );
		}

		/// <summary>
		/// Enters edit mode
		/// </summary>
		/// <returns>True if the operation was successful</returns>
		private bool StartEditMode( bool takeFocus )
		{
			//// honor the always in edit mode property if editor is not hosted
			//if (this._host == null &&
			//     this.IsAlwaysInEditMode == true)
			//    this._isInEditMode = true;

			if (this._isInEditMode == true)
				return true;

            // AS 10/3/08 TFS8634
            // We had a slight hole here - the CVP may not want to allow
            // editing but the editor's StartEditMode didn't check that.
            // Since the editor needs to know this state now I moved the check
            // here.
            //
            if (this.IsEditingAllowed == false)
                return false;

            // raise the EditModeStarting event
			EditModeStartingEventArgs args = new EditModeStartingEventArgs();
			this.RaiseEditModeStarting(args);

			if (args.Cancel)
				return false;

			_isEnteringEditMode = true;
			try
			{
				// call the virtual method to initialize the OriginalValue property
				this.InitializeOriginalValueFromValue( );

				// set the cached member 1st so that we don't trigger a recursive call to this 
				// method in the property change of IsInEditMode
				this._isInEditMode = true;

				// now set the property
				this.IsInEditMode = true;

				// apply the edit mode text converter
				this.SyncTextWithValue( );

				// SSP 2/6/09 TFS10586
				// Added AlwaysValidate property.
				// 
				if ( this.AlwaysValidateResolved )
					this.RevalidateCurrentValue( );

				// call measure tomake sure the edit template is hydrated
				this.Measure( new Size( double.PositiveInfinity, double.PositiveInfinity ) );

				// if a focus stite has been specified then set focus to it now
				if ( takeFocus )
					this.SetFocusToFocusSite( );

				// SSP 10/9/08 BR33762
				// Allow for setting selection start/length in the EditModeStarted event.
				// Moved this here from ValuePresenter.OnPreviewMouseLeftButtonDown.
				// 
				// If entering edit mode, select all text.
				// 
				if ( null != _host && this is ISupportsSelectableText && _isInEditMode )
					( (ISupportsSelectableText)this ).SelectAll( );
			}
			finally
			{
				_isEnteringEditMode = false;
			}

			// SSP 10/22/08 BR35625
			// If we somehow exited edit mode during the process of entering edit mode above,
			// then don't raise EditModeStarted event. Therefore check to make sure that
			// we are still in edit mode before raising the event.
			// 
			if ( _isInEditMode )
			{
				// raise the EditModeStarted event
				this.RaiseEditModeStarted( new EditModeStartedEventArgs( ) );
			}

			return true;
		}

				#endregion //StartEditMode

				#region ValidateCurrentValue

		/// <summary>
		/// Validates the current value of the editor and initializes the <see cref="IsValueValid"/> 
		/// property based on the results of the value validation.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// ValueEditor automatically validates the value whenever it changes and therefore typically 
		/// it should not be necessary to call this method. However there may be times when you may
		/// want to force the value to be re-validated, for instance when the external validation
		/// criteria changes. This is especially useful when you are overriding the 
		/// <see cref="ValidateCurrentValue(out Exception)"/> virtual method to provide custom logic
		/// for the value validation. The ValueEditor will update the <see cref="IsValueValid"/> 
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
		/// Implicit value constraints are <see cref="ValueEditor.ValueType"/>,
		/// <see cref="XamMaskedEditor.Mask"/> etc... and explicit constraints are specified
		/// via <see cref="ValueEditor.ValueConstraint"/> property.
		/// </para>
		/// <seealso cref="ValueEditor.IsValueValid"/>
		/// <seealso cref="ValueEditor.InvalidValueBehavior"/>
		/// </remarks>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public virtual bool ValidateValue( object value, out Exception error )
		{
			return this.IsValueValidHelper( value, out error );
		}

				#endregion // ValidateValue

			#endregion //Public Methods

			#region Protected Methods

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
		/// <see cref="ValueEditor.ValueType"/> property. This method is typically used to 
		/// convert the user input in the form of text to the value that gets returned
		/// from the <see cref="ValueEditor.Value"/> property. Value property returns objects 
		/// of type specified by ValueType property.
		/// </p>
		/// <p class="body">
		/// For example, if the ValueType property of a <see cref="XamTextEditor"/> is set to DateTime type, 
		/// and the user types in "1/1/07", this method will get called to convert that text value
		/// into a DateTime object.
		/// </p>
		/// <p class="body">
		/// <b>Note:</b> Typically there is no need for you to call this method directly as this method is 
		/// automatically called by the ValueEditor itself to perform the necessary conversions between text and value.
		/// </p>
		/// <p class="body">
		/// <b>Note:</b> If you want to override the default conversion logic for converting between text and value,
		/// set the <see cref="ValueEditor.ValueToTextConverter"/> and <see cref="TextEditorBase.ValueToDisplayTextConverter"/>
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
				value = this.ValueToTextConverterResolved.ConvertBack( text, this.ValueType,
					this, this.FormatProviderResolved as System.Globalization.CultureInfo );

				if ( null == value )
				{
					if ( null == text || text.Length == 0 )
					{
						// SSP 1/13/12 TFS99243
						// We should always use null otherwise binding will not work as it cannot convert
						// DBNull to a DateTime? value of null for example.
						// 
						//value = DBNull.Value;
						value = null;
					}
					else
					{
						// SSP 4/21/09 NAS9.2 IDataErrorInfo Support
						// Set error to an appropriate error message.
						// 
						error = Utils.GetTextToValueConversionError( this.ValueType, text );

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
		/// Converts the specified value to text using the <see cref="ValueEditor.ValueToTextConverterResolved"/>.
		/// This method is used to display the value of the editor when the editor is in edit mode.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <param name="text">This out parameter will be set to the converted text.</param>
		/// <param name="error">If conversion fails, error is set to a value that indicates the error.</param>
		/// <returns>True if conversion succeeds, False otherwise.</returns>
		/// <remarks>
		/// <p class="body">
		/// ConvertValueToText is used to convert value to text. This method is typically used to 
		/// convert the value of the editor (as specified by the <see cref="ValueEditor.Value"/> property)
		/// into text that the user can edit when the editor enters edit mode. When not in edit mode,
		/// <see cref="TextEditorBase.ConvertValueToDisplayText"/> method is used to convert value
		/// to text that gets displayed in the editor. The <see cref="ValueEditor.Text"/> property's return value
		/// corresponds to the text that this method converts where as the <see cref="TextEditorBase.DisplayText"/> 
		/// property's return value corresponds to the text that ConvertValueToDisplayText method converts.
		/// </p>
		/// <p class="body">
		/// <b>Note</b> that DisplayText and ConvertValueToDisplayText methods are defined on <see cref="TextEditorBase"/>
		/// class. This is because display text conversions are only applicable for text based editors, all of which
		/// derive from TextEditorBase.
		/// </p>
		/// <p class="body">
		/// As an example, the ValueType property of a <see cref="XamTextEditor"/> is set to DateTime type, 
		/// and the <see cref="ValueEditor.Value"/> property is set to a "01/01/2007" DateTime instance.
		/// This method gets called to convert that DateTime value to a string when the user enters
		/// edit mode. When the editor is not in edit mode, <see cref="TextEditorBase.ConvertValueToDisplayText"/>
		/// is used. The difference between this method and ConvertValueToDisplayText is that the
		/// ConvertValueToDisplayText will take into account <see cref="ValueEditor.FormatProvider"/>
		/// and <see cref="ValueEditor.Format"/> property settings where as ConvertValueToText will not.
		/// </p>
		/// <p class="body">
		/// <b>Note:</b> Typically there is no need for you to call this method directly as this method is 
		/// automatically called by the ValueEditor itself to perform the necessary conversions between value 
		/// and text.
		/// </p>
		/// <p class="body">
		/// <b>Note:</b> If you want to override the default conversion logic for converting between value and text,
		/// set the <see cref="ValueEditor.ValueToTextConverter"/> and <see cref="TextEditorBase.ValueToDisplayTextConverter"/>
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

				#region DoInitialization

		/// <summary>
		/// Called from OnInitialized to provide the derived classes an opportunity to 
		/// perform appropriate initialization tasks. OnInitialized implementation enters
		/// the editor into edit mode at the end if AlwaysInEditMode is true. This method 
		/// is called before that.
		/// </summary>
		protected virtual void DoInitialization( )
		{
			// SSP 1/16/12 TFS59404
			// 
			_isInitializing = true;

			try
			{
				// SSP 1/3/07 BR27394
				// Moved the existing code into the new InitializeValueProperties method.
				// 
				this.InitializeValueProperties( );
			}
			finally
			{
				_isInitializing = false;
			}
		}

				#endregion // DoInitialization

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
				this.CoerceValue( ValueProperty );
				this.SyncValueProperties( ValueProperty, this.Value );
			}
			// SSP 1/7/08 BR29457
			// 
			//else if ( DependencyProperty.UnsetValue != this.ReadLocalValue( TextProperty ) )
			else if ( Utils.IsValuePropertySet( TextProperty, this ) )
			{
				this.CoerceValue( TextProperty );
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
		protected virtual void InitializeOriginalValueFromValue()
		{
			this.SetValue(OriginalValuePropertyKey, this.Value);
		}

				#endregion //InitializeOriginalValueFromValue	
    
				#region OnFocusSiteChanged

		/// <summary>
		/// Called when the focus site changes.
		/// </summary>
		/// <seealso cref="ValueEditor.FocusSite"/>
		/// <seealso cref="ValueEditor.ValidateFocusSite"/>
		protected virtual void OnFocusSiteChanged()
		{
		}

				#endregion //OnFocusSiteChanged	

				#region OnIsFocusWithinChanged

		/// <summary>
		/// Called when property <see cref="IsFocusWithin"/> changes.
		/// </summary>
		protected virtual void OnIsFocusWithinChanged( bool gotFocus )
		{
			if ( ! gotFocus )
			{
				// If IsAlwaysInEditMode is true then EndEditMode logic won't be executed and therefore
				// we have to validate the input here in OnLostFocus.
				// 
				if ( null == _host && this.IsAlwaysInEditMode && this.IsInEditMode )
				{
					EditModeValidationErrorEventArgs eventArgs;
					bool stayInEditMode;
					this.ValidateInputHelper( true, false, out eventArgs, out stayInEditMode );
				}

				// End edit mode if IsAlwaysInEditMode is false.
				// 
				if ( null == _host && this.IsInEditMode )
				{
					if ( !this.IsAlwaysInEditMode )
					{
						this.EndEditMode( true, false );
					}
                    
#region Infragistics Source Cleanup (Region)














#endregion // Infragistics Source Cleanup (Region)

				}
			}
			else // got focus
			{
				// Enter edit mode if not already in edit mode.
				// 
				if ( null == _host && !this.IsInEditMode && !_isEnteringEditMode )
				{
					this.StartEditMode( );
				}
                else if (null == _host && this.IsInEditMode && this.IsAlwaysInEditMode)
                {
                    // AS 10/16/08 TFS9214
                    // When an editor whose IsAlwaysInEditMode is false enters edit mode,
                    // it updates its originalvalue from the value. Similarly when an editor
                    // whose IsAlwaysInEditMode is true gets logical focus, it should update
                    // its original value from its value.
                    //
                    this.InitializeOriginalValueFromValue();
                }
			}
		}

				#endregion // OnIsFocusWithinChanged

				#region ProcessKeyDown

		/// <summary>
		/// Processes the key down event. Default implementation does nothing.
		/// This class overrides OnKeyDown and performs some default processing and
		/// then calls this method if further key down processing is to be done.
		/// Derived classes are intended to override this method instead of OnKeyDown.
		/// </summary>
		/// <param name="e"></param>
		internal protected virtual void ProcessKeyDown( KeyEventArgs e )
		{
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
		protected virtual void RevertValueBackToOriginalValue()
		{
			this.Value = this.OriginalValue;

			
			// We need to explicitly make sure that the Text property is synced with the Value
			// in case the Value and OriginalValue are the same but the Text is not reflective
			// of the Value (because the Text was modified to a value that we could not parse
			// to the value type).
			// 
			this.SyncTextWithValue( );
		}

				#endregion //RevertValueBackToOriginalValue

				#region VerifyTemplateState

		/// <summary>
		/// Makes sure that either the <see cref="EditTemplate"/> or the <b>Template</b> is being used based on 
		/// whether the editor is in edit mode.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// VerifyTemplateState method makes sure the correct control template is being used based on the
		/// value of <see cref="IsInEditMode"/> property. When in edit mode, the control template specified
		/// by the <see cref="EditTemplate"/> property is used. When not in edit mode, the control template
		/// specified by the <b>Template</b> property is used. If EditTemplate is not set then no
		/// action will be taken and the template specified by the Template property will be used.
		/// </para>
		/// <para class="body">
		/// See <see cref="EditTemplate"/> for more information.
		/// </para>
		/// <see cref="EditTemplate"/>
		/// </remarks>
		protected void VerifyTemplateState()
		{
			// JM 08-25-11 TFS84624 - No need to do this dance -  we just coerce the Template property.
			//
			//if (this.IsInEditMode)
			//{
			//    ControlTemplate editTemplate = this.EditTemplate;

			//    if (editTemplate != null &&
			//        editTemplate != this.Template)
			//    {
			//        ////// store the previous non-edit mode template if we're just entering edit mode
			//        ////if (this.ReadLocalValue(CachedRenderTemplateProperty) == DependencyProperty.UnsetValue)
			//        ////    this.SetValue(CachedRenderTemplateProperty, this.Template);

			//        /* AS 3/6/09 TFS15045
			//         * Changed to a helper method that we can use to set the
			//         * _isChangingTemplate flag and more importantly ensure we 
			//         * do not orphan the focused element.
			//         * 
			//        bool wasChangingTemplate = this._isChangingTemplate;
			//        try
			//        {
			//            this._isChangingTemplate = true;
			//            this.Template = editTemplate;
			//        }
			//        finally
			//        {
			//            this._isChangingTemplate = wasChangingTemplate;
			//        }
			//        */

			//        this.SetTemplate(editTemplate);
			//    }
			//}
			//else
			//{
			//    // if we're coming out of edit mode there will still be a cached
			//    // copy of the template we had before entering edit mode...
			//    if (this.ReadLocalValue(CachedRenderTemplateProperty) != DependencyProperty.UnsetValue)
			//    {
			//        ControlTemplate renderTemplate = (ControlTemplate)this.GetValue(CachedRenderTemplateProperty);

			//        // clear the reference to the template before changing the template
			//        this.ClearValue(CachedRenderTemplateProperty);

			//        // change the template
			//        // AS 3/6/09 TFS15045
			//        // Changed to a helper method that we can use to set the
			//        // _isChangingTemplate flag and more importantly ensure we 
			//        // do not orphan the focused element.
			//        //
			//        //this.Template = renderTemplate;
			//        this.SetTemplate(renderTemplate);
			//    }
			//}

			this.CoerceValue(TemplateProperty);
		}
				#endregion //VerifyTemplateState

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
			errorMessage = null;

			FrameworkElement fe = focusSite as FrameworkElement;
			FrameworkContentElement fce = focusSite as FrameworkContentElement;

			if ( fe == null && fce == null )
			{
				errorMessage = new NotSupportedException( ValueEditor.GetString( "LE_NotSupportedException_3", focusSite.GetType( ).Name ) );
				return false;
			}

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
            else if (this.IsMouseOver)
                VisualStateUtilities.GoToState(this, useTransitions, VisualStateUtilities.StateMouseOver, VisualStateUtilities.StateNormal);
            else
                VisualStateManager.GoToState(this, VisualStateUtilities.StateNormal, useTransitions);
            
            if (this.IsEmbedded )
                VisualStateManager.GoToState(this, VisualStateUtilities.StateEmbedded, useTransitions);
            else
                VisualStateManager.GoToState(this, VisualStateUtilities.StateNotEmbedded, useTransitions);

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
            ValueEditor editor = target as ValueEditor;

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
				_pendingPropertyChangedEvents = new List<RoutedEventArgs>( );

			_syncingValueProperties++;
		}

				#endregion // BeginSyncValueProperties

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
						List<RoutedEventArgs> list = _pendingPropertyChangedEvents;
						for ( int i = 0; i < list.Count; i++ )
						{
							// MD 7/16/10 - TFS26592
							// Call off to the new helper method to raise the event. 
							//this.RaiseEvent( list[i] );
							this.RaiseEventHelper(list[i]);
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

			if ( compareWithOriginalValue
				// When IsAlwaysInEditMode is true, there's no concept of comitting edit value. Since
				// we don't know whether the value is comitted, we shouldn't base whether to validate 
				// the value or not on what value was the last time the editor got focus (when
				// IsAlwaysInEditMode is true the original value is initialized to the editor's value 
				// when the editor gets focus).
				// 
				&& ( null != _host || ! this.IsAlwaysInEditMode ) )
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

				#region RaiseEventHelper

		// MD 7/16/10 - TFS26592






		internal bool RaiseEventHelper(RoutedEventArgs args)
		{
			if (_host != null && _host.ShouldSuppressEvent(args))
			{
				if (args.RoutedEvent == ValueEditor.ValueChangedEvent)
					this._host.OnEditorValueChanged(args);

				return false;
			}

			this.RaiseEvent(args);
			return true;
		} 

				#endregion // RaiseEventHelper

				#region RaiseValuePropertyChangedEvent

		
		
		/// <summary>
		/// Raises the specified value property change notification. If value property synchronization
		/// is in progress, delays raising of the event until the syncrhonization is complete.
		/// </summary>
		/// <param name="e"></param>
		internal void RaiseValuePropertyChangedEvent( RoutedEventArgs e )
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
				FrameworkElement fe = this._focusSite as FrameworkElement;

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

				#region StartEditModeOnMouseDownHelper

		// SSP 5/8/08 BR32695
		// Added StartEditModeOnMouseDownHelper method. Code in there is moved from 
		// OnMouseLeftButtonDown method.
		// 
		internal bool StartEditModeOnMouseDownHelper( )
		{
			// SSP 8/31/09 TFS20880
			// Check for _host == null is moved below inside the if block because
			// we need to focus the editor if the editor is already in edit mode 
			// even if there's a _host.
			// 
			//if ( this._host == null && this.Focusable && this.IsEnabled )
			if ( this.Focusable && this.IsEnabled )
			{
				
				
				
				
				bool ret = null == _host;

				
				
				
				
				if ( ( null == _host || this.IsInEditMode ) && !this.IsKeyboardFocused && !this.IsKeyboardFocusWithin )
					// give the control focus if it doesn't already have it
					this.Focus( );

				//if (!this.IsAlwaysInEditMode && !this.IsInEditMode )
				
				
				
				
				
				
				
				
				
				if ( null == _host && this.IsFocusWithin && !this.IsInEditMode )
				{
					// enter edit mode
					this.StartEditMode( );

					
					ret = true;
				}

				
				
				return ret;
			}

			return false;
		}

				#endregion // StartEditModeOnMouseDownHelper

				#region ValidateInputHelper

		internal void ValidateInputHelper( bool fromLostFocus, bool forceExitEditMode, 
			out EditModeValidationErrorEventArgs eventArgs, out bool stayInEditMode )
		{
			eventArgs = null;

			// JJD 08/08/12 - TFS118455
			// check the anti re-entrancy flag 
			if (_isDisplayingErrorMsgDialog)
			{
				// stay in edit mode
				stayInEditMode = true;

				// asynchronously re-focus the editor
				this.Dispatcher.BeginInvoke( new Utils.MethodInvoker(this.ReFocus));

				return;
			}

			stayInEditMode = false;

			if ( ! this.IsValueValid
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
				// SSP 5/5/09 - IDataErrorInfo Support
				// Use the error information contained in the new InvalidValueErrorInfo property.
				// 
				// ----------------------------------------------------------------------------------
				ValidationErrorInfo errorInfo = this.InvalidValueErrorInfo;
				Debug.Assert( null != errorInfo );
				if ( null == errorInfo )
					errorInfo = new ValidationErrorInfo( new Exception( ValueEditor.GetString( "LE_Exception_1" ) ) );

				eventArgs = new EditModeValidationErrorEventArgs( this, forceExitEditMode, errorInfo.Exception, errorInfo.ErrorMessage );
				
#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

				// ----------------------------------------------------------------------------------

				this.RaiseEditModeValidationError( eventArgs );

				// SSP 1/20/10 TFS30656
				// Enclosed the existing code in the if block. If the value is changed to a valid value from
				// within the EditModeValidationError event, then don't show the message box or stay in edit 
				// mode since the value of the editor is valid.
				// 
				if ( ! this.IsValueValid )
				{
					switch ( eventArgs.InvalidValueBehavior )
					{
						case InvalidValueBehavior.Default:
						case InvalidValueBehavior.DisplayErrorMessage:

						// AS 7/19/07 BR25005
						// See CanShowErrorMessage for details.
						//
						//if (eventArgs.ErrorMessage != null && eventArgs.ErrorMessage.Length > 0)
						if (eventArgs.ErrorMessage != null && eventArgs.ErrorMessage.Length > 0 && 
							this.CanShowErrorMessage())
						{
							// JJD 08/08/12 - TFS118455
							// set the anti re-entrancy flag 
							_isDisplayingErrorMsgDialog = true;

							try
							{
								// AS 10/23/08 TFS9546
								//MessageBox.Show(Window.GetWindow(this),
								Utilities.ShowMessageBox(this,
									eventArgs.ErrorMessage,
									// SSP 9/2/08 BR35906
									// Localize the string.
									// 
									//"Invalid Value",
									ValueEditor.GetString("LMSG_ValueConstraint_InvalidValue"),
									MessageBoxButton.OK,
									MessageBoxImage.Stop);
							}
							finally
							{
								// JJD 08/08/12 - TFS118455
								// reset the anti re-entrancy flag 
								_isDisplayingErrorMsgDialog = false;
							}
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
			DependencyObject rootWindow = this;

			while (rootWindow != null)
			{
				if (rootWindow is Page || rootWindow is Window)
					break;

				rootWindow = VisualTreeHelper.GetParent(rootWindow);
			}

			return rootWindow == null || ((UIElement)rootWindow).IsVisible;
		} 
				#endregion //CanShowErrorMessage

				#region InitializeCachedPropertyValues

		
		
		
		
		
		/// <summary>
		/// Initializes the variables used to cache the dependency property values by
		/// getting the dependency property metadata for this object and getting DefaultValue
		/// of that metadata for the respective property.
		/// </summary>
		private void InitializeCachedPropertyValues( )
		{
			_cachedEditTemplate = (ControlTemplate)EditTemplateProperty.GetMetadata( this ).DefaultValue;
			_cachedFormat = (string)FormatProperty.GetMetadata( this ).DefaultValue;
			_cachedFormatProvider = (IFormatProvider)FormatProviderProperty.GetMetadata( this ).DefaultValue;
			_cachedInvalidValueBehavior = (InvalidValueBehavior)InvalidValueBehaviorProperty.GetMetadata( this ).DefaultValue;

			
			
			
			//_cachedValueConstraint = (ValueConstraint)ValueConstraintProperty.GetMetadata( this ).DefaultValue;
			this.InternalSetCachedValueConstraintHelper( (ValueConstraint)ValueConstraintProperty.GetMetadata( this ).DefaultValue );
			
			_cachedValueType = (Type)ValueTypeProperty.GetMetadata( this ).DefaultValue;
		}

				#endregion // InitializeCachedPropertyValues

				#region IsValueValidHelper

		private bool IsValueValidHelper( object value, out Exception error )
		{
			string errorMessage = null;
			error = null;
			ValueConstraint vc = this.ValueConstraint;
			if ( null != vc
				&& !vc.Validate( value, this.ValueType, ValueConstraintFlags.All,
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
				if ( ! this.ValueType.IsInstanceOfType( value ) )
					return false;
			}

			return true;
		}

				#endregion // IsValueValidHelper

				#region SyncTextWithValue

		internal bool SyncTextWithValue( )
		{
			Exception error;
			// SSP 3/26/10 TFS26082
			// Added syncDisplayText parameter. Pass true for it because most of the time
			// we want to also synchronize display text as well.
			// 
			//return this.SyncTextWithValue( this.Value, out error );
			return this.SyncTextWithValueHelper( this.Value, out error, true );
		}

		// SSP 3/26/10 TFS26082
		// 
		//private bool SyncTextWithValue( object newValue, out Exception error )
		private bool SyncTextWithValueHelper( object newValue, out Exception error, bool syncDisplayText )
		{
			
			
			
			this.BeginSyncValueProperties( );

			try
			{
				string text;
				if ( this.ConvertValueToText( newValue, out text, out error ) )
				{
					// SSP 5/31/11 TFS57173
					// 
					// ------------------------------------------------------------
					//this.SetValue( ValueEditor.TextProperty, text );
					this.SetValue( InSyncTextWithValueProperty, text );
					try
					{
						// SSP 5/10/12 TFS100047
						// 
						//this.SetValue( ValueEditor.TextProperty, text );
						this.Text_CurrentValue = text;
					}
					finally
					{
						this.ClearValue( InSyncTextWithValueProperty );
					}
					// ------------------------------------------------------------

					// SSP 3/26/10 TFS26082
					// 
					if ( syncDisplayText )
					{
						TextEditorBase textEditor = this as TextEditorBase;
						if ( null != textEditor && syncDisplayText )
							textEditor.SyncDisplayText( );
					}

					return true;
				}
				else
				{
					// If the value can't be converted to text then consider it an invalid value.
					// 
					// SSP 4/17/09 NAS9.2 IDataErrorInfo Support
					// Use the new SetIsValueValid method instead.
					// 
					//this.SetValue( IsValueValidPropertyKey, KnownBoxes.FalseBox );
					this.SetIsValueValid( false, error );

					return false;
				}
			}
			finally
			{
				
				
				
				this.EndSyncValueProperties( );
			}
		}

				#endregion // SyncTextWithValue

				#region SyncValueWithText

		private bool SyncValueWithText( object newValue, out Exception error )
		{
			
			
			
			this.BeginSyncValueProperties( );

			try
			{
				object value;
				if ( this.ConvertTextToValue( (string)newValue, out value, out error ) )
				{
					this.SetValue( ValueEditor.ValueProperty, value );
					return true;
				}
				else
				{
					// If the text cannot be converted to value type then set IsValueValid to false.
					// 
					// SSP 4/17/09 NAS9.2 IDataErrorInfo Support
					// Use the new SetIsValueValid method instead.
					// 
					//this.SetValue( IsValueValidPropertyKey, KnownBoxes.FalseBox );					
					this.SetIsValueValid( false, error );

					// It was decided that we should raise the ValueChanged event when the Text
					// changes even though the value of the Value property hasn't changed.
					// 
					this.OnValueChanged( this.Value, this.Value );

					return false;
				}
			}
			finally
			{
				
				
				
				this.EndSyncValueProperties( );
			}
		}

				#endregion // SyncValueWithText

				#region OnCoerceTemplate
		private static object OnCoerceTemplate(DependencyObject target, object value)
		{
			// let the base coerce get a first crack at it if there is one
			if (TemplateProperty.DefaultMetadata.CoerceValueCallback != null)
				value = TemplateProperty.DefaultMetadata.CoerceValueCallback(target, value);

			ValueEditor control = target as ValueEditor;

			// JM 08-25-11 TFS84624 - We have removed the CachedRenderTemplate property and instead are relying on the
			// underlying (non-coerced) value of the Template to hold the render template.  Note that as part of this change,
			// we no longer set the Template property directly - we always coerce it.   
			//
			//// if we're in edit mode and the template is being changed externally...
			//if (null != control && control.IsInEditMode && control._isChangingTemplate == false)
			//{
			//    //~ SSP 7/16/08
			//    //~ At design-time for some reasons the property gets set null. Commented out the assert.
			//    //~ 
			//    //~Debug.Assert(value != null, "The template that we will revert to when not in edit mode will be null!");

			//    //~ SSP 3/7/07 
			//    //~ We shouldn't disallow them to set the template even if it's not going to be used.
			//    //~ 
			//    /*
			//    // since the template is only going to be used when we come out of edit mode, we're
			//    // going to throw an exception if the editor doesn't come out of edit mode
			//    if ( control.IsAlwaysInEditMode )
			//        throw new InvalidOperationException( "The Template property should not be set for a 'ValueEditor' whose 'IsAlwaysInEditMode' is true. Set the 'EditTemplate' instead." );
			//    */

			//    // otherwise cache this as the new template
			//    control.SetValue(CachedRenderTemplateProperty, value);

			//    // and pass the current template along so we stay in edit mode
			//    value = control.Template;
			//}

			// If we are in edit mode return the EditTemplate otherwise just return 'value' (i.e., the render template)
			if (null != control && control.IsInEditMode && control.EditTemplate != null)
				return control.EditTemplate;

			return value;
		} 
				#endregion //OnCoerceTemplate

				// JJD 08/08/12 - TFS118455 - added
				#region ReFocus

		private void ReFocus()
		{
			this.Focus();
		}

				#endregion //ReFocus	
    
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

				// JM 08-25-11 TFS84624 - No longer need this method since we are now simply coercing the Template property  
				// in the VerifyTemplateState method.  Note: I moved the 2 lines of focus-related logic from this method to the
				// OnPropertyChanged override (specifically in the TemplateProperty switch case)
				//
				// AS 3/6/09 TFS15045
                #region SetTemplate
		//private void SetTemplate(ControlTemplate template)
		//{
		//    bool wasChangingTemplate = this._isChangingTemplate;
		//    try
		//    {
		//        this._isChangingTemplate = true;

		//        // AS 3/6/09 TFS15045
		//        // If keyboard focus is within our template then its going to 
		//        // be orphaned when we release the edit template so we need to 
		//        // 
		//        // AS 3/10/09 TFS15045
		//        // We should not take focus if we don't have logical focus. The containing 
		//        // window may be closing. When that happens they clear the logical focus. 
		//        // As a result we exit edit mode. While exiting edit mode we restore the 
		//        // render template but since keyboard focus was within we were trying to 
		//        // give keyboard focus to the editor. This caused it to get logical focus 
		//        // again which caused it to try and get into edit mode while it was exiting
		//        // edit mode. If logical focus is elsewhere then we will not interfere with 
		//        // the keyboard focus.
		//        //
		//        //if (this.IsKeyboardFocusWithin && this.IsKeyboardFocused == false)
		//        if (this.IsKeyboardFocusWithin && this.IsKeyboardFocused == false && this.IsFocusWithin)
		//            this.Focus();

		//        this.Template = template;
		//    }
		//    finally
		//    {
		//        this._isChangingTemplate = wasChangingTemplate;
		//    }
		//} 
                #endregion //SetTemplate

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

			if ( this.IsInEditMode && ! _isEnteringEditMode )
				this.SetValue( HasValueChangedPropertyKey, KnownBoxes.TrueBox );
		}

				#endregion // SyncValueProperties

				#region SyncValuePropertiesOverride

		
		
		

		
#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)


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
			if ( ValueEditor.ValueProperty == prop )
			{
				// SSP 3/26/10 TFS26082
				// Added syncDisplayText parameter. Pass false for it.
				// 
				//return this.SyncTextWithValue( newValue, out error );
				return this.SyncTextWithValueHelper( newValue, out error, false );
			}
			else if ( ValueEditor.TextProperty == prop )
			{
				return this.SyncValueWithText( newValue, out error );
			}
			else
			{
				Debug.Assert( false, "This should only be called for Text and Value properties. Derived classes need to handle their own properties." );
				error = new InvalidOperationException( "This should only be called for Text and Value properties. Derived classes need to handle their own properties." );
				return false;
			}
		}
				#endregion // SyncValuePropertiesOverride

				// JJD 1/9/09 - NA 2009 vol 1 - Record filtering - added
                #region ValidateCurrentValueHelper

        private bool ValidateCurrentValueHelper(out Exception error)
        {
            bool isValueValid = this.ValidateCurrentValue(out error);

            // if the value is valid from the editor's perspective call
            // the host's PerformAdditionalValidation method to let it participate
            // in the validation
            if (isValueValid && this._host != null)
                return this._host.IsCurrentValueValid(out error);

            return isValueValid;

        }

                #endregion //ValidateCurrentValueHelper	
    
				#region VerifyFocusSite

		private void VerifyFocusSite( )
		{
			DependencyObject focusSite = base.GetTemplateChild( "PART_FocusSite" );

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

		#region IWeakEventListener Members

		// SSP 3/24/10 TFS27839
		// 
		bool IWeakEventListener.ReceiveWeakEvent( Type managerType, object sender, EventArgs e )
		{
			if ( typeof( PropertyChangedEventManager ) == managerType )
			{
				PropertyChangedEventArgs args = e as PropertyChangedEventArgs;
				Debug.Assert( null != args );

				if ( null != args )
				{
					if ( sender == _cachedValueConstraint )
					{
						this.OnValueConstraintChanged( args.PropertyName );
						return true;
					}
				}
			}

			Debug.Assert( false );

			return false;
		}

		#endregion // IWeakEventListener Members
	}

	#endregion // ValueEditor Class

	#region ValueEditorDefaultConverter Class

	internal class ValueEditorDefaultConverter : IValueConverter
	{
		private static ValueEditorDefaultConverter _valueToDisplayTextConverter;
		private static ValueEditorDefaultConverter _valueToTextConverter;

		protected bool _isDisplayTextConverter;

		protected ValueEditorDefaultConverter( bool isDisplayTextConverter )
		{
			_isDisplayTextConverter = isDisplayTextConverter;
		}

		public static ValueEditorDefaultConverter ValueToTextConverter
		{
			get
			{
				if ( null == _valueToTextConverter )
					_valueToTextConverter = new ValueEditorDefaultConverter( false );

				return _valueToTextConverter;
			}
		}

		public static ValueEditorDefaultConverter ValueToDisplayTextConverter
		{
			get
			{
				if ( null == _valueToDisplayTextConverter )
					_valueToDisplayTextConverter = new ValueEditorDefaultConverter( true );

				return _valueToDisplayTextConverter;
			}
		}

		private object ConvertHelper( bool convertingBack, object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
		{
			ValueEditor editor = parameter as ValueEditor;

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

				return Utilities.ConvertDataValue( value, targetType, formatProvider, format );
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
		private bool HandleNullValueHelper( bool convertingBack, object value, Type targetType, ValueEditor editor, out object convertedValue )
		{
			bool isNull = null == value || DBNull.Value == value;

			if ( convertingBack )
			{
				// AS 5/1/09 NA 2009 Vol 2 - Clipboard Support
				// We need to handle the display to text converter converting back from the null text.
				//
				if (_isDisplayTextConverter)
				{
					TextEditorBase textEditor = editor as TextEditorBase;
					if (null != textEditor && object.Equals(value, textEditor.NullText))
					{
						// SSP 1/13/12 TFS99243
						// We should always use null otherwise binding will not work as it cannot convert
						// DBNull to a DateTime? value of null for example.
						// 
						//convertedValue = DBNull.Value;
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
					// SSP 1/13/12 TFS99243
					// We should always use null otherwise binding will not work as it cannot convert
					// DBNull to a DateTime? value of null for example.
					// 
					//convertedValue = DBNull.Value;
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
						TextEditorBase textEditor = editor as TextEditorBase;
						if ( null != textEditor )
							nullText = textEditor.NullText;
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
			ValueEditor editor, IFormatProvider formatProvider, string format )
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

			return Utilities.ConvertDataValue( value, targetType, formatProvider, format );
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

	#endregion // ValueEditorDefaultConverter Class
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