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


namespace Infragistics.Windows.Editors
{
	#region XamMaskedEditor Class

	/// <summary>
	/// Value editor for displaying and editing data based on a mask.
	/// </summary>
	/// <remarks>
	/// <p class="body">
	/// <b>XamMaskedEditor</b> is an editor control that lets you display and edit values based on a mask. The mask is 
	/// specified via its <see cref="XamMaskedEditor.Mask"/> property. If a mask is not explicitly set then a default mask 
	/// is used based on the <see cref="ValueEditor.ValueType"/> property. The default mask is determined using the masks 
	/// that are registered for specific types using <see cref="RegisterDefaultMaskForType(Type,string)"/>.</p>
	/// <p class="body">
	/// There are certain type specific editors that are available that derive from XamMaskedEditor. They include 
	/// <see cref="XamCurrencyEditor"/>, <see cref="XamNumericEditor"/> and <see cref="XamDateTimeEditor"/>. Although
	/// the XamMaskedEditor can be used to edit all these types, the derived editors default certain settings and
	/// provide more specific object model for editing the associated data types.
	/// </p>
	/// <seealso cref="ValueEditor"/>
	/// <seealso cref="TextEditorBase"/>
	/// <seealso cref="XamCurrencyEditor"/>
	/// <seealso cref="XamNumericEditor"/>
	/// <seealso cref="XamDateTimeEditor"/>
	/// </remarks>
	
	
	[System.Windows.Markup.ContentProperty( "Value" )]
	// SSP 3/23/09 IME
	// Added PART_InputTextBox TemplatePart.
	// 
	[TemplatePart( Name = "PART_InputTextBox", Type = typeof( TextBox ) )]
	// SSP 10/14/09 - NAS10.1 Spin Buttons
	// 
	[TemplatePart( Name = "PART_SpinButtons", Type = typeof( FrameworkElement ) )]
	[StyleTypedProperty( Property = "SpinButtonStyle", StyleTargetType = typeof( RepeatButton ) )]
	public class XamMaskedEditor : TextEditorBase, ICommandHost, ISupportsSelectableText
	{
		#region static constants

		internal const char					DEFAULT_PROMPT_CHAR		= '_';
		internal const char					DEFAULT_PAD_CHAR		= ' ';

		// SSP 12/18/02 UWE342
		// Added a way for the user to specify a mask in which we will translate certain
		// characters to underlying locale characters.
		//
		internal const string				LOCALIZED_ESCAPE_SEQUENCE = "{LOC}";
		
		#endregion //static constants

		#region Variables

		private UltraLicense _license;

		private EditInfo _editInfo;
		private MaskInfo _maskInfo;

		// Control that was last hooked into the GotFocus and LostFocus events
		//
		//private Control			lastHookedControl = null;
		private SectionsCollection		_displayChars_Sections;
		private DisplayCharsCollection	_displayChars;

		private bool _displayNullTextWhenNotFocused; // = false;

		// AS 11/12/03 optimization
		// Try to avoid rebuilding the default date mask
		// if the short date pattern and date separator 
		// have not changed.
		//
		private static string g_lastShortDatePattern;
		private static char g_lastDateSeparator; // = '\0';
		private static string g_lastDefaultDateMask;
		// SSP 2/6/09 TFS13259
		// We also need to check if the last format provider was the same
		// as well as the new usePostfixSeparatorsFromLongDatePattern parameter.
		// 
		private static IFormatProvider g_lastDateFormatProvider;
		private static bool g_lastUsePostfixSeparatorsFromLongDatePattern;

		private static string g_lastShortTimePattern;
		private static char g_lastTimeSeparator; // = '\0';
		private static string g_lastDefaultTimeMask;

		private static Hashtable g_defaultMaskTable = new Hashtable( );
		private static bool g_defaultMaskTableInitialized = false;

		// SSP 11/18/04 BR00499
		// Added AllowShiftingAcrossSections property.
		//
		private bool _allowShiftingAcrossSections = true;

		// SSP 7/19/07 BR22754
		// We need to appear as if we have keyboard focus when we are displaying context menu.
		// 
		internal bool _isDisplayingContextMenu = false;

		// SSP 10/17/07 UWG2076 BR27228
		// Added transitingIntoEditModeInMouseDown. This will prevent the caret from being drawn
		// and also prevent the IsBeingEditedAndFocused property to return false. The reason
		// for doing this is that when the masked editor goes into edit mode with a display mode
		// of something other than IncludeBoth, there is a big potential for characters shifting
		// around as prompt characters show up. At this point the mouse click hasn't finished
		// processing, especially the act of selecting the character where the caret should be
		// positioned. As a result of characters shifting, the caret may end up being positioned
		// on the wrong character. This flag will prevent this from happening. If this causes
		// problems, it should be safe to take it out but just make sure that when clicking in
		// a grid cell and going into edit mode selects the right character in above described
		// situation.
		//
		internal bool _transitingIntoEditModeInMouseDown = false;
		internal Point _transitingIntoEditModeInMouseDownPoint = new Point( );

		// SSP 10/1/07 - XamRibbon
		// XamRibbon needs to keep the capture on the editor even after the mouse is released.
		// Therefore we need to keep a flag to indicate when we are in drag selection mode
		// and not just rely on the editor having mouse capture.
		// 
		private bool _isDragSelecting = false;

		// SSP 10/8/09 - NAS10.1 Spin Buttons
		// 
		internal SpinInfo _cachedSpinInfo;

		// MD 4/25/11 - TFS73181
		private bool _skipNextMaskValidation;

		// SSP 7/22/11 TFS80613
		// 
		private bool _resetIsTextBoxEntryEnabledWhenEditModeEnds;

		// SSP 8/30/11 TFS76307
		// 
		private bool _cachedTrimFractionalZeros;


		// JJD 03/20/12 - TFS105113 - Added touch support for 12.1
		internal static bool _IsTouchEnabled;


		#endregion //Variables

		#region Events

		/// <summary>
		/// Occurs when the user types a character that fails mask validation.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// The InvalidChar event is fired when user types a character that does not match the mask associated with the current input position.
		/// </para>
		/// <seealso cref="InvalidCharEventArgs"/>
		/// </remarks>
		public event EventHandler<InvalidCharEventArgs>				InvalidChar;

		#endregion
	
		#region Constructors

		static XamMaskedEditor( )
		{
			//This OverrideMetadata call tells the system that this element wants to provide a style that is different than its base class.
			//This style is defined in themes\generic.xaml
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata( typeof( XamMaskedEditor ), new FrameworkPropertyMetadata( typeof( XamMaskedEditor ) ) );

            // SSP 3/15/07 BR21086
            // Do this through the style via a trigger so when the masked editor is not in edit mode, it draws 
            // the focus rect. For example, in DataPresenter when the cell exits edit mode via Escape key, the
            // editor retains focus and is not in edit mode. In such a case, it should draw focus rect.
            // 
            // SSP 3/5/07 BR20704
			// Masked editor should not draw any focus rect when it's focused.
			//FrameworkElement.FocusVisualStyleProperty.OverrideMetadata( typeof( XamMaskedEditor ), new FrameworkPropertyMetadata( new Style( ) ) );

			// SSP 6/6/07 BR23366
			// We need this in order to make the Tab and Shift+Tab navigation work properly.
			// This is similar to what inbox ComboBox does.
			// 
			KeyboardNavigation.TabNavigationProperty.OverrideMetadata( typeof( XamMaskedEditor ), new FrameworkPropertyMetadata( KeyboardNavigationMode.Local ) );
			KeyboardNavigation.ControlTabNavigationProperty.OverrideMetadata( typeof( XamMaskedEditor ), new FrameworkPropertyMetadata( KeyboardNavigationMode.None ) );
			KeyboardNavigation.DirectionalNavigationProperty.OverrideMetadata( typeof( XamMaskedEditor ), new FrameworkPropertyMetadata( KeyboardNavigationMode.None ) );
		}

		/// <summary>
		/// Initializes a new <see cref="XamMaskedEditor"/>
		/// </summary>
		public XamMaskedEditor( )
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
			//if (DesignerProperties.GetIsInDesignMode(this))
			{
				try
				{
					// We need to pass our type into the method since we do not want to pass in 
					// the derived type.
					this._license = LicenseManager.Validate(typeof(XamMaskedEditor), this) as UltraLicense;
				}
				catch (System.IO.FileNotFoundException) { }
			}

			// JJD 4/25/07
			// Optimization - don't create a spearate context menu for every editor
			// Instead override OnContextMenuOpening
			//this.ContextMenu = this.CreateContextMenu( );

			
			
			
			
			
			this.InitializeCachedPropertyValues( );
		}

		#endregion // Constructors

		#region Embedding code

		#region Public Properties
		
		#endregion //Public Properties
		
		#region Public Methods

		#region CanEditType

		/// <summary>
		/// Determines if the editor natively supports editing values of specified type.
		/// </summary>
		/// <param name="type">The data type to check.</param>
		/// <returns>Returns True if the editor natively supports editing values of specified type, False otherwise.</returns>
		/// <remarks>
		/// <p class="body">
		/// XamMaskedEditor's implementation returns True for the string, sbyte, byte, 
		/// short, ushort, int, uint, long, ulong, float, double, decimal and DateTime data types.
		/// </p>
		/// <p class="body">
		/// For these data types the editor will calculate a default mask. For any other
		/// data type, you should specify a mask that makes sense for the data type other wise
		/// a default mask will be used. You can change/register default masks for these and other
		/// data types using the <see cref="XamMaskedEditor.RegisterDefaultMaskForType"/> method.
		/// </p>
		/// <p class="body">
		/// See ValueEditor's <see cref="ValueEditor.CanEditType"/> for more information.
		/// </p>
		/// </remarks>
		/// <seealso cref="ValueType"/>
		/// <seealso cref="Mask"/>
		/// <seealso cref="ValueEditor.CanEditType(Type)"/>
		public override bool CanEditType( System.Type type )
		{
			return XamMaskedEditor.SupportsDataType( type );
		}

		#endregion //CanEditType

		#region CanRenderType

		/// <summary>
		/// Determines if the editor natively supports displaying of values of specified type.
		/// </summary>
		/// <param name="type">The data type to check.</param>
		/// <returns>Returns True if the editor natively supports displaying values of specified type, False otherwise.</returns>
		/// <remarks>
		/// <p class="body">
		/// See ValueEditor's <see cref="ValueEditor.CanRenderType"/> for more information.
		/// </p>
		/// </remarks>
		/// <seealso cref="CanEditType(Type)"/>
		/// <seealso cref="ValueEditor.CanRenderType(Type)"/>
		public override bool CanRenderType(System.Type type)
		{		
			return this.CanEditType( type );
		}

		#endregion //CanRenderType	

		#region IsExtentBasedOnValue

		/// <summary>
		/// Indicates whether the desired width or the height of the editor is based on the value.
		/// </summary>
		/// <param name="orientation">Orientation of the extent being evaluated. Horizontal indicates the width and vertical indicates the height.</param>
		/// <returns>True if extent is based on the value.</returns>
		/// <remarks>
		/// <para class="body">
		/// XamMaskedEditor's implementation returns True for horizontal dimension and False for the vertical
		/// dimension.
		/// </para>
		/// <para class="body">
		/// See ValueEditor's <see cref="ValueEditor.IsExtentBasedOnValue"/> for more information.
		/// </para>
		/// </remarks>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public override bool IsExtentBasedOnValue( Orientation orientation )
		{
			if ( Orientation.Horizontal == orientation )
				return true;

			return false;
		}

		#endregion // IsExtentBasedOnValue
	
		#endregion //Public Methods

		#region Protected Methods

		#region OnCreateAutomationPeer
		/// <summary>
		/// Returns an automation peer that exposes the <see cref="XamMaskedEditor"/> to UI Automation.
		/// </summary>
		/// <returns>A <see cref="Infragistics.Windows.Automation.Peers.Editors.XamMaskedEditorAutomationPeer"/></returns>
		protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer()
		{
			return new Infragistics.Windows.Automation.Peers.Editors.XamMaskedEditorAutomationPeer(this);
		}
		#endregion //OnCreateAutomationPeer

		#region DoInitialization

		/// <summary>
		/// Called from OnInitialized to provide the derived classes an opportunity to 
		/// perform appropriate initialization tasks. OnInitialized implementation enters
		/// the editor into edit mode at the end if AlwaysInEditMode is true. This method 
		/// is called before that.
		/// </summary>
		protected override void DoInitialization( )
		{
			base.DoInitialization( );

			// For efficiency reasons we skip applying mask to the text during the 
			// process of initialization since we don't know if a mask is going to 
			// be set during initialization. Therefore apply the mask when the 
			// initialization process finishes.
			// 
			MaskInfo maskInfo = this.MaskInfo;
			maskInfo.RecreateSections( true );
			maskInfo.InternalRefreshValue( this.Value );
			this.ApplyMaskToText( );
		}

		#endregion // DoInitialization

		#region OnCoerceText

		/// <summary>
		/// Called from the <see cref="ValueEditor.Text"/> property's CoerceValue handler.
		/// </summary>
		/// <param name="text">The text to coerce</param>
		/// <returns>Coerced value</returns>
		/// <remarks>
		/// <para class="body">
		/// XamMaskedEditor's implementation applies mask to the <paramref name="text"/>.
		/// </para>
		/// </remarks>
		protected override string OnCoerceText( string text )
		{
			text = base.OnCoerceText( text );

			// Apply the mask to the set text. Only do so if the Text is not being set by us
			// to an already mask-coerced value.
			// 
			if ( ! this.SyncingValueProperties 
				&& ( null == this._editInfo || ! this._editInfo._inOnTextChanged ) )
			{
				if ( null != text && text.Length > 0 )
				{
					MaskInfo maskInfo = MaskInfo.CreateMaskInfoForConverter( this );
					Debug.Assert( null != maskInfo );
					if ( null != maskInfo )
					{
						try
						{
							XamMaskedEditor.SetText( maskInfo.Sections, text, maskInfo );
							text = XamMaskedEditor.GetText( maskInfo.Sections, maskInfo.DataMode, maskInfo );
						}
						finally
						{
							// SSP 5/19/08 BR33116
							// Release the mask info to have its sections collection release reference to the 
							// editor to prevent memory leak since the sections are statically cached.
							// 
							MaskInfo.ReleaseMaskInfoForConverter( maskInfo );
						}
					}
				}
			}
			
			return text;
		}

		#endregion // OnCoerceText

		#region OnPropertyChanged

		// SSP 9/26/07 BR26060 BR26063
		// Overrode OnPropertyChanged.
		// 
		/// <summary>
		/// Called when a property value has changed
		/// </summary>
		/// <param name="e">The event arguments</param>
		protected override void OnPropertyChanged( DependencyPropertyChangedEventArgs e )
		{
			base.OnPropertyChanged( e );

			DependencyProperty prop = e.Property;

			if ( ValueEditor.FormatProviderProperty  == prop
				// SSP 7/9/08 BR34636
				// 
				|| LanguageProperty == prop 
				)
			{
				if ( null != _maskInfo )
				{
					// SSP 7/9/08 BR34636
					// 
					//_maskInfo.FormatProvider = e.NewValue as IFormatProvider;
					_maskInfo.InitializeFormatProvider( );

					this.ReparseMask( );
				}
			}
			else if ( ValueEditor.FormatProperty == prop )
			{
				if ( null != _maskInfo )
					_maskInfo.Format = e.NewValue as string;
			}
			// SSP 10/10/07 
			// Added the following else-if block.
			// 
			else if ( ValueEditor.IsInEditModeProperty == prop )
			{
				this.ProcessIsBeingEditedAndFocusedChanged( );
			}

			// SSP 10/5/09 - NAS10.1 Spin Buttons
			// 
			if ( UIElement.IsMouseOverProperty == prop
				|| XamDateTimeEditor.SpinButtonDisplayModeProperty == prop
				|| ValueEditor.IsInEditModeProperty == prop
				|| ValueEditor.IsReadOnlyProperty == prop )
			{
				this.UpdateSpinButtonVisibility( );
			}
		}

		#endregion //OnPropertyChanged	

		#region ValidateCurrentValue

		/// <summary>
		/// Validates the current value of the editor. This method is called by the editor to perform
		/// editor specific validation of the current value.
		/// </summary>
		/// <returns>True if the value is valid, False otherwise.</returns>
		/// <remarks>
		/// See ValueEditor's <see cref="ValueEditor.ValidateCurrentValue(out Exception)"/> for more information.
		/// <seealso cref="ValueEditor.IsValueValid"/>
		/// <seealso cref="ValueEditor.InvalidValueBehavior"/>
		/// </remarks>
		protected override bool ValidateCurrentValue( out Exception error )
		{
			if ( !base.ValidateCurrentValue( out error ) )
				return false;

			// MD 4/25/11 - TFS73181
			// If we should skip the next mask validation, reset the flag here and just return true.
			if (this._skipNextMaskValidation)
			{
				error = null;
				this._skipNextMaskValidation = false;
				return true;
			}

			MaskInfo maskInfo = this.MaskInfo;
			string errorMsg;
			if ( null != maskInfo && null != maskInfo.Sections
				&& ! this.IsInputValid( maskInfo.Sections, out errorMsg ) )
			{
				error = new Exception( errorMsg );
				return false;
			}

			return true;
		}

		#endregion // ValidateCurrentValue

		#endregion //Protected Methods

		#endregion // End of embedding code

		#region Base Overrides

		#region ValidateValue

		// SSP 5/5/09 - Clipboard Support
		// Added ValidateValue to be used by the clipboard functionality in the data presenter
		// for ensuring text being pasted is not invalid.
		// 
		/// <summary>
		/// Returns true if the specified value would be considered to be valid by 
		/// the editor based on implicit as well as explicit value constraints enforced by the editor. 
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
		public override bool ValidateValue( object value, out Exception error )
		{
			if ( !base.ValidateValue( value, out error ) )
				return false;

			MaskInfo maskInfo = MaskInfo.CreateMaskInfoForConverter( this );

			try
			{
				string errorMsg;
				if ( null != maskInfo && null != maskInfo.Sections )
				{
					if ( ! maskInfo.InternalRefreshValue( value, out error ) )
						return false;

					if ( !this.IsInputValid( maskInfo.Sections, out errorMsg ) )
					{
						error = new Exception( errorMsg );
						return false;
					}
				}
			}
			finally
			{
				MaskInfo.ReleaseMaskInfoForConverter( maskInfo );
			}

			return true;
		}

		#endregion // ValidateValue

		#region OnFocusSiteChanged

		
		
		
		/// <summary>
		/// Called when the focus site changes.
		/// </summary>
		protected override void OnFocusSiteChanged( )
		{
			base.OnFocusSiteChanged( );

			EditInfo editInfo = this.EditInfo;
			if ( null != editInfo )
				editInfo.InitializeIMETextBox( this.FocusSite );
		}

		#endregion // OnFocusSiteChanged

		// JJD 03/20/12 - TFS105113 - Added touch support for 12.1
		#region OnTouchEnter


		/// <summary>
		/// Called when the user is about to touch down on a touch enabled syatem
		/// </summary>
		/// <param name="e"></param>
		protected override void OnTouchEnter(TouchEventArgs e)
		{
			base.OnTouchEnter(e);

			if (_IsTouchEnabled == false)
			{
				// set a static flag so we know we are on a touch enabled system
				_IsTouchEnabled = true;

				EditInfo editInfo = this.EditInfo;
				if (null != editInfo)
					editInfo.InitializeIsTextBoxEntryEnabled();
			}
		}


		#endregion //OnTouchEnter	
    
		#region OnValueConstraintChanged

		
		
		
		/// <summary>
		/// This method is called whenever the ValueConstraint or one of its properties changes.
		/// </summary>
		/// <param name="valueConstraintPropertyName">Null if the ValueConstraint itself changed or 
		/// the name of the property that changed.</param>
		internal override void OnValueConstraintChanged( string valueConstraintPropertyName )
		{
			if ( null != _maskInfo )
				_maskInfo.InitializedCachedMinMaxValues( );

			base.OnValueConstraintChanged( valueConstraintPropertyName );
		}

		#endregion // OnValueConstraintChanged

		#endregion // Base Overrides

		#region Private/Internal methods

		#region InitializeCachedPropertyValues

		
		
		
		
		
		/// <summary>
		/// Initializes the variables used to cache the dependency property values by
		/// getting the dependency property metadata for this object and getting DefaultValue
		/// of that metadata for the respective property.
		/// </summary>
		private void InitializeCachedPropertyValues( )
		{
			_cachedCaretVisible = (bool)CaretVisibleProperty.GetMetadata( this ).DefaultValue;
			_cachedClipMode = (MaskMode)ClipModeProperty.GetMetadata( this ).DefaultValue;
			_cachedDataMode = (MaskMode)DataModeProperty.GetMetadata( this ).DefaultValue;
			_cachedDisplayMode = (MaskMode)DisplayModeProperty.GetMetadata( this ).DefaultValue;
			_cachedMask = (string)MaskProperty.GetMetadata( this ).DefaultValue;
			_cachedPadChar = (char)PadCharProperty.GetMetadata( this ).DefaultValue;
			_cachedPromptChar = (char)PromptCharProperty.GetMetadata( this ).DefaultValue;
			// SSP 8/30/11 TFS76307
			// 
			_cachedTrimFractionalZeros = (bool)TrimFractionalZerosProperty.GetMetadata( this ).DefaultValue;
		}

		#endregion // InitializeCachedPropertyValues

		#region ResolveMask



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		internal bool ResolveMask( 
			Type dataType,
			IFormatProvider formatProvider,
			out string mask )
		{
			// MD 4/25/11 - TFS73181
			// Moved all code to the new overload.
			MaskDefaultType maskDefaultType;
			return this.ResolveMask(dataType, formatProvider, out mask, out maskDefaultType);
		}

		// MD 4/25/11 - TFS73181
		// Added an overload which has an out paramter for the MaskDefaultType
		internal bool ResolveMask(
			Type dataType,
			IFormatProvider formatProvider,
			out string mask, out MaskDefaultType maskDefaultType)
		{
			mask = null;

			// MD 4/25/11 - TFS73181
			// Intialize the out parameter.
			maskDefaultType = MaskDefaultType.None;

			// If Mask property is explicitly set to a value then use that mask.
			// 
			string tmpMask = this.Mask;
			if ( null != tmpMask && tmpMask.Length > 0 )
			{
				mask = tmpMask;
				return true;
			}

			// Use the GetDefaultMask method to allow other editors that derive from this editor
			// to be able to specify a default mask that will be used when none is explicitly set.
			//
			string defaultMask = this.GetDefaultMask( dataType, formatProvider );
			if ( null != defaultMask && defaultMask.Length > 0 )
			{
				mask = defaultMask;
				return true;
			}

			if ( null == dataType )
				return false;

			// MD 4/25/11 - TFS73181
			// Call off to the new overload which has an out MaskDefaultType parameter.
			//mask = GetDefaultMaskForType( dataType );
			mask = GetDefaultMaskForType(dataType, out maskDefaultType);

			// If the dataType was supported and mask was returned, then
			// return true.
			//
			return null != mask && mask.Length > 0;
		}

		#endregion //ResolveMask

		#region GetNonstandardForeignDateMaskAndPostfixSymbols



#region Infragistics Source Cleanup (Region)










#endregion // Infragistics Source Cleanup (Region)

		internal static string GetNonstandardForeignDateMaskAndPostfixSymbols( IFormatProvider formatProv, ref Hashtable foreignDateSymbols )
		{
			foreignDateSymbols = null;
			string  mask       = null;

			DateTimeFormatInfo dateTimeFormatInfo = XamMaskedEditor.GetDateTimeFormatInfo( formatProv );
			if( dateTimeFormatInfo == null )
				return mask;

			string longDatePattern = dateTimeFormatInfo.LongDatePattern;
			if( longDatePattern == null )
				return mask;

			int mIndexFirst = longDatePattern.IndexOfAny( new char[] { 'm', 'M' } );
			int dIndexFirst = longDatePattern.IndexOfAny( new char[] { 'd', 'D' } );
			int yIndexFirst = longDatePattern.IndexOfAny( new char[] { 'y', 'Y' } );

			int mIndexLast = longDatePattern.LastIndexOfAny( new char[] { 'm', 'M' } );
			int dIndexLast = longDatePattern.LastIndexOfAny( new char[] { 'd', 'D' } );
			int yIndexLast = longDatePattern.LastIndexOfAny( new char[] { 'y', 'Y' } );

			int mDiff = mIndexLast - mIndexFirst;
			int dDiff = dIndexLast - dIndexFirst;

			if( mDiff < 2 && dDiff < 2 )
			{
				// Get the postfix symbols used by the current culture.
				char mSymbol = (char)0;
				if( mIndexLast + 2 < longDatePattern.Length )
					mSymbol = longDatePattern[ mIndexLast + 2 ];

				char dSymbol = (char)0;
				if( dIndexLast + 2 < longDatePattern.Length )
					dSymbol = longDatePattern[ dIndexLast + 2 ];

				char ySymbol = (char)0;
				if( yIndexLast + 2 < longDatePattern.Length )
					ySymbol = longDatePattern[ yIndexLast + 2 ];

				if( mSymbol != (char)0 &&  dSymbol != (char)0 &&  ySymbol != (char)0 &&
					mIndexFirst != -1  &&  dIndexFirst != -1  &&  yIndexFirst != -1   )
				{
					// Fill the output parameter with the different symbols that will be used when making the mask.
					foreignDateSymbols = new Hashtable( 3 );
					foreignDateSymbols.Add( typeof( DaySection ),   dSymbol );
					foreignDateSymbols.Add( typeof( MonthSection ), mSymbol );
					foreignDateSymbols.Add( typeof( YearSection ),  ySymbol );

					int yCount     = yIndexLast - yIndexFirst + 1;
					string yString = yCount > 2 ? "yyyy" : "yy";
					string mString = "mm";
					string dString = "dd";					

					// Construct the mask.
					if ( mIndexFirst <= dIndexFirst && mIndexFirst <= yIndexFirst )
					{
						if ( dIndexFirst < yIndexFirst )
							mask = String.Format( "{0}{1}{2}{3}{4}{5}", mString, mSymbol, dString, dSymbol, yString, ySymbol );
						else 
							mask = String.Format( "{0}{1}{2}{3}{4}{5}", mString, mSymbol, yString, ySymbol, dString, dSymbol );
					}
					else if ( dIndexFirst <= mIndexFirst && dIndexFirst <= yIndexFirst )
					{
						if ( mIndexFirst < yIndexFirst )
							mask = String.Format( "{0}{1}{2}{3}{4}{5}", dString, dSymbol, mString, mSymbol, yString, ySymbol );
						else 
							mask = String.Format( "{0}{1}{2}{3}{4}{5}", dString, dSymbol, yString, ySymbol, mString, mSymbol );
					}
					else if ( yIndexFirst <= mIndexFirst && yIndexFirst <= dIndexFirst )
					{
						if ( mIndexFirst < dIndexFirst )
							mask = String.Format( "{0}{1}{2}{3}{4}{5}", yString, ySymbol, mString, mSymbol, dString, dSymbol );
						else
							mask = String.Format( "{0}{1}{2}{3}{4}{5}", yString, ySymbol, dString, dSymbol, mString, mSymbol );
					}
				}
			}

			return mask;
		}

		#endregion // GetNonstandardForeignDateMaskAndPostfixSymbols
		
		#region IsSectionNumeric



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		internal static bool IsSectionNumeric( SectionBase section )
		{
			if ( section is NumberSection )
				return true;

			bool signEncountered = false;
			bool digitEncountered = false;

			for ( int i = 0; i < section.DisplayChars.Count; i++ )
			{
				DisplayCharBase dc = section.DisplayChars[i];

				if ( dc.Section.PlusSignChar == dc.Char ||
					dc.Section.MinusSignChar == dc.Char )
				{
					// If we previously had a sign, then retrun false as a number
					// can't have two signs.
					//
					if ( signEncountered || digitEncountered )
						return false;

					signEncountered = true;
					continue;
				}

				if ( !( dc is DigitChar ) && 
					!( !dc.IsEditable && char.IsDigit( dc.Char ) ) )
					return false;

				if ( !dc.IsEmpty )
					digitEncountered = true;
			}

			return true;
		}

		#endregion //IsSectionNumeric
		
		#region ConvertToFourYear



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

        // AS 8/25/08 Support Calendar
        //internal static int ConvertToFourYear( int twoYear )
		internal static int ConvertToFourYear( int twoYear, System.Globalization.Calendar calendar )
		{
            if (twoYear < 100)
                // AS 8/25/08 Support Calendar
                //return System.Globalization.CultureInfo.CurrentCulture.Calendar.ToFourDigitYear(twoYear);
                return calendar.ToFourDigitYear(twoYear);

			return twoYear;
		}

		#endregion // ConvertToFourYear

		#region GetNumberSectionValueHelper

		// SSP 9/22/11 TFS82033
		// 
		/// <summary>
		/// Gets the value of the number section of the specified type. If the section is not found returns -2 and
		/// if the section is empty returns -1.
		/// </summary>
		/// <param name="sections"></param>
		/// <param name="sectionType"></param>
		/// <returns></returns>
		private static int GetNumberSectionValueHelper( SectionsCollection sections, Type sectionType )
		{
			NumberSection numberSection = (NumberSection)XamMaskedEditor.GetSection( sections, sectionType );
			return null == numberSection ? -2
				: ( numberSection.IsEmpty ? -1 : numberSection.ToInt( ) );
		} 

		#endregion // GetNumberSectionValueHelper

		#region GetDateValue

		// SSP 9/22/11 TFS82033
		// 
		/// <summary>
		/// Gets the date value if there are year, month and day sections and they are all filled.
		/// </summary>
		/// <param name="sections"></param>
		/// <returns></returns>
		internal static DateTime? GetDateValue( SectionsCollection sections )
		{
			int year = GetNumberSectionValueHelper( sections, typeof( YearSection ) );
			int month = GetNumberSectionValueHelper( sections, typeof( MonthSection ) );
			int day = GetNumberSectionValueHelper( sections, typeof( DaySection ) );

			if ( year > 0 && month > 0 && day > 0 )
			{
				DateTime defaultDate = DateTime.Now;

				try
				{
					return new DateTime( year, month, day );
				}
				catch
				{
				}
			}

			return null;
		} 

		#endregion // GetDateValue

		#region GetDateTimeValue



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal static DateTime GetDateTimeValue( SectionsCollection sections )
		{
			DateTime date;

			int defDay, defMonth, defYear, defHour, defMinute, defSecond;

            // AS 8/25/08 Support Calendar
            System.Globalization.Calendar cal = sections.Calendar;

			DateTime currDate = DateTime.Now;

			defDay   = 1;
			defMonth = 1;
            // AS 8/25/08 Support Calendar
            //defYear = currDate.Year;
            defYear = cal.GetYear(currDate);
			defHour  = 0;
			defMinute = 0;
			defSecond = 0;
			
			// SSP 2/25/04 UWG2962 UWE856
			// Retain the date portion when editing time and the same goes for time portion as 
			// well.
			//
			// ----------------------------------------------------------------------------------
			bool useCurrDateDefaults = true;
			MaskInfo maskInfo = sections.MaskInfo;
			if ( null != maskInfo && null != maskInfo.MaskEditor )
			{
				object originalValue = maskInfo.MaskEditor.OriginalValue;
				if ( null != originalValue )
					originalValue = Utilities.ConvertDataValue( originalValue, typeof( DateTime ), maskInfo.FormatProvider, maskInfo.Format );

				if ( originalValue is DateTime )
				{
					DateTime origDate = (DateTime)originalValue;
                    // AS 8/25/08 Support Calendar
                    //defDay   = origDate.Day;
                    //defMonth = origDate.Month;
                    //defYear  = origDate.Year;
                    //defHour  = origDate.Hour;
                    //defMinute = origDate.Minute;
                    //defSecond = origDate.Second;

                    // make sure its within a valid range for the calendar
                    if (origDate >= cal.MinSupportedDateTime && origDate <= cal.MaxSupportedDateTime)
                    {
                        defDay = cal.GetDayOfMonth(origDate);
                        defMonth = cal.GetMonth(origDate);
                        defYear = cal.GetYear(origDate);
                        defHour = cal.GetHour(origDate);
                        defMinute = cal.GetMinute(origDate);
                        defSecond = cal.GetSecond(origDate);
                        useCurrDateDefaults = false;
                    }
				}
			}
			// ----------------------------------------------------------------------------------		

			MonthSection monthSection = (MonthSection)XamMaskedEditor.GetSection( sections, typeof( MonthSection ) );
			DaySection daySection = (DaySection)XamMaskedEditor.GetSection( sections, typeof( DaySection ) );
			YearSection yearSection = (YearSection)XamMaskedEditor.GetSection( sections, typeof( YearSection ) );
			HourSection hourSection = (HourSection)XamMaskedEditor.GetSection( sections, typeof( HourSection ) );
			MinuteSection minuteSection = (MinuteSection)XamMaskedEditor.GetSection( sections, typeof( MinuteSection ) );
			SecondSection secondSection = (SecondSection)XamMaskedEditor.GetSection( sections, typeof( SecondSection ) );

			// SSP 2/25/04 UWG2962 UWE856
			// Related to the change above. 
			// Enclosed the existing code into the if block.
			//
			if ( useCurrDateDefaults )
			{
                // AS 8/25/08 Support Calendar
                //defDay   = null != hourSection ? currDate.Day : 1;
                //defMonth = null != hourSection ? currDate.Month : 1;
                defDay = null != hourSection ? cal.GetDayOfMonth(currDate) : 1;
                defMonth = null != hourSection ? cal.GetMonth(currDate) : 1;
            }

			// SSP 1/17/02
			// Added code for AM-PM section
			//
			AMPMSection ampmSection = (AMPMSection)XamMaskedEditor.GetSection( sections, typeof( AMPMSection ) );

			// SSP 1/11/02
			// If none of the sections exist, then return null
			// 
			if ( null == monthSection && null == daySection &&
				null == yearSection && null == hourSection &&
				null == minuteSection && null == secondSection )
			{
				// SSP 6/27/11 TFS77173
				// If a textual mask is used, for example to allow for +1m type of input, then parse using
				// DateTime.Parse instead of throwing an exception.
				// 
				// ------------------------------------------------------------------------------------------
				string text = AreAllDisplayCharsEmpty( sections ) ? null : GetText( sections, maskInfo.DataMode, maskInfo );
				if ( !string.IsNullOrEmpty( text ) )
				{
					IValueConverter valueToTextConverter = maskInfo.MaskEditor.ValueToTextConverter;
					object convertedVal = null != valueToTextConverter
						? valueToTextConverter.ConvertBack( text, typeof( DateTime ), maskInfo.MaskEditor, maskInfo.FormatProvider as CultureInfo )
						: CoreUtilities.ConvertDataValue( text, typeof( DateTime ), maskInfo.FormatProvider, maskInfo.Format );

					if ( convertedVal is DateTime )
						return (DateTime)convertedVal;
				}
				// ------------------------------------------------------------------------------------------

				throw new ArgumentException( XamMaskedEditor.GetString( "LE_ArgumentException_4" ) );
			}

			// SSP 1/17/02
			// Added code form AM-PM section
			//
			int hour = null != hourSection ? hourSection.ToInt( ) : defHour;
			if ( null != hourSection && null != ampmSection )
			{
				// If the am-pm section has pm in it add 12 to the hour.
				// Do so only if the hour is less that 12.
				//
				if ( hour < 12 )
				{							
					if ( null != ampmSection.PMValue && 
						// JJD 12/10/02 - FxCop
						// Pass the culture info in explicitly
						//0 == string.Compare( ampmSection.PMValue, ampmSection.GetText( MaskMode.IncludeLiterals ), true ) )
						0 == string.Compare( ampmSection.PMValue, ampmSection.GetText( MaskMode.IncludeLiterals ), true, System.Globalization.CultureInfo.CurrentCulture ) )
						hour += 12;
				}
					// SSP 11/26/02 UWM156
					// If the hour is 12 and it's AM, then make it 0 before passing it in the 
					// DateTime constructor.
					//
				else if ( 12 == hour )
				{
					if ( null != ampmSection.AMValue && 
						// JJD 12/10/02 - FxCop
						// Pass the culture info in explicitly
						//0 == string.Compare( ampmSection.AMValue, ampmSection.GetText( MaskMode.IncludeLiterals ), true ) )
						0 == string.Compare( ampmSection.AMValue, ampmSection.GetText( MaskMode.IncludeLiterals ), true, System.Globalization.CultureInfo.CurrentCulture ) )
						hour -= 12;
				}
			}


			// SSP - 10/23/01
			// Changed the behaviour so that now we allow the user
			// to specify only one year, month or day section in the mask
			// for a date. So as long as one of them is there.
			//

			
#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)


			// SSP 1/17/02
			// If the year section is a 2 digit year section, the pad left the
			// number by current century. (If the year section has 02, pad left with
			// 20 so that the year is 2002. )
			//			
			// SSP 8/7/02
			// Use a ToFourDigitYear method off the calendar to convert from two digit 
			// year to four digit year. To do that call GetYear method off the year
			// section.
			//
			
#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

			int year = null != yearSection ? yearSection.GetYear( ) : defYear;

			// SSP 10/31/05 BR07389
			// Make sure we don't end up using an invalid default day for the month.
			// 
			// ----------------------------------------------------------------------
			if ( null == daySection && null != monthSection )
			{
				int month = monthSection.ToInt( );
                // AS 8/25/08 Support Calendar
                //int maxDays = DateTime.DaysInMonth( year, month );
                int maxDays = cal.GetDaysInMonth(year, month);
                if (defDay > maxDays)
					defDay = maxDays;
			}
			// ----------------------------------------------------------------------

            // AS 8/25/08 Support Calendar
            //date = new DateTime( 				
            date = cal.ToDateTime(
                year,
                null != monthSection ? monthSection.ToInt() : defMonth,
                null != daySection ? daySection.ToInt() : defDay,
                hour,
                null != minuteSection ? minuteSection.ToInt() : defMinute,
                null != secondSection ? secondSection.ToInt() : defSecond,
                0);

			return date;
		}
		
		#endregion //GetDateTimeValue

		#region GetText



#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

		internal static string GetText( 
			SectionsCollection sections, 
			MaskMode maskMode,
			char promptChar,
			char padChar )
		{
			StringBuilder sb = new StringBuilder( 1 + XamMaskedEditor.GetTotalNumberOfDisplayChars( sections ) );

			for ( int i = 0, sectionsCount = sections.Count; i < sectionsCount; i++ )
			{
				DisplayCharsCollection displayChars = sections[i].DisplayChars;

				int count = displayChars.Count;
				for ( int j = 0; j < count; j++ )
				{
					DisplayCharBase dc = displayChars[j];

					char c = dc.GetChar( maskMode, promptChar, padChar );
					
					if ( 0 != (int)c )
						sb.Append( c );
					
					// SSP 10/16/02 UWE273
					// Take care of commas in number sections.
					//
					if ( dc is DigitChar && 
						((DigitChar)dc).ShouldIncludeComma( maskMode ) && 0 != dc.Section.CommaChar )
						sb.Append( dc.Section.CommaChar );					
				}
			}

			return sb.ToString( );
		}

		internal static bool AreAllDisplayCharsEmpty( SectionsCollection sections )
		{
			foreach ( DisplayCharBase dc in sections.AllDisplayCharacters )
			{
				if ( dc.IsEditable && !dc.IsEmpty )
					return false;
			}

			return true;
		}



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		internal static string GetText( 
			SectionsCollection sections, 
			MaskMode maskMode,
			MaskInfo maskInfo )
		{
			return XamMaskedEditor.GetText( sections, maskMode, 
				null != maskInfo ? maskInfo.PromptChar : DEFAULT_PROMPT_CHAR,
				null != maskInfo ? maskInfo.PadChar    : DEFAULT_PAD_CHAR );
		}



#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

		internal static string GetText(
			SectionsCollection sections,
			MaskMode maskMode,
			MaskInfo maskInfo,
			string nullText )
		{
			if ( AreAllDisplayCharsEmpty( sections ) )
				return nullText;

			return XamMaskedEditor.GetText( sections, maskMode,
				null != maskInfo ? maskInfo.PromptChar : DEFAULT_PROMPT_CHAR,
				null != maskInfo ? maskInfo.PadChar : DEFAULT_PAD_CHAR );
		}

		#endregion //GetText

        #region SetText

        /// <summary>
		/// Sets the text to sections collection.
		/// </summary>
		/// <param name="sections">Sections to be updated</param>
		/// <param name="text">Text used to update the sections</param>
		/// <param name="promptCharacter">Character interpretted as the prompt character</param>
		/// <param name="padCharacter">Character interpretted as the pad character</param>
		/// <returns>Number of characters from text that matched.</returns>
		internal static int SetText( 
			SectionsCollection sections, 
			string text,
			char promptCharacter,
			char padCharacter )
		{
            // AS 10/17/08 TFS8886
            return SetText(sections, text, promptCharacter, padCharacter, true);
        }

        // AS 10/17/08 TFS8886
        // Added overload because we may have multiple numeric sections that 
        // are not part of a single numeric value.
        //
		/// <summary>
		/// Sets the text to sections collection.
		/// </summary>
		/// <param name="sections">Sections to be updated</param>
		/// <param name="text">Text used to update the sections</param>
		/// <param name="promptCharacter">Character interpretted as the prompt character</param>
		/// <param name="padCharacter">Character interpretted as the pad character</param>
        /// <param name="skipDigitSeparator">True if the value may be numeric in which case digit separators should be ignored; otherwise false.</param>
		/// <returns>Number of characters from text that matched.</returns>
		internal static int SetText( 
			SectionsCollection sections, 
			string text,
			char promptCharacter,
			char padCharacter,
            bool skipDigitSeparator)
		{
			if ( null == text )
				text = "";

			// First erase all the characters.
			//
			XamMaskedEditor.EraseAllChars( sections );

			int textIndex = 0;
			
			DisplayCharBase dc = GetDisplayCharAtPosition( sections, 0 );

			// SSP 1/16/12 TFS98252
			// 
			MaskInfo maskInfo = sections.MaskInfo;
			MaskMode maskMode = null != maskInfo ? maskInfo.DataMode : MaskMode.IncludeLiterals;

			while ( null != dc && textIndex < text.Length )
			{
				char c = text[ textIndex ];

				if ( dc.MatchChar( c ) )
				{
					if ( dc.IsEditable )
						dc.Char = c;
					textIndex++;
					dc = dc.NextDisplayChar;
				}
				else if ( promptCharacter == c || padCharacter == c )
				{
					// SSP 1/16/12 TFS98252
					// If literals aren't included in the data and we encounter a prompt or pad character in place
					// of a literal then don't skip the unmatched prompt or pad character in the text as it will
					// match the next editable character, either as a value or as a placeholder.
					// 
					// --------------------------------------------------------------------------------------------
					//textIndex++;
					bool moveToNextChar = true;

					if ( !dc.IsEditable && ( MaskMode.Raw == maskMode || MaskMode.IncludePromptChars == maskMode ) )
					{
						moveToNextChar = false;
					}

					if ( moveToNextChar )
						textIndex++;
					// --------------------------------------------------------------------------------------------

					dc = dc.NextDisplayChar;
				}
				// Skip commas in number sections. Although number sections display commas,
				// digit characters do not take anything but digits and +/- sign.
				//
				else if ( dc is DigitChar && dc.Section is NumberSection
					&& skipDigitSeparator // AS 10/17/08 TFS8886
					&& dc.Section.CommaChar == c )
				{
					textIndex++;
				}
				else
				{
					SectionBase nextSection = dc.Section.NextSection;

					if ( null != nextSection )
						dc = nextSection.FirstDisplayChar;
					else
						break;
				}
			}

			for ( int i = 0; i < sections.Count; i++ )
			{
				EditSectionBase editSection = sections[i] as EditSectionBase;

				if ( null != editSection )				
					editSection.ValidateSection( );
			}

			// Return the number of characters matched.
			//
			return textIndex;
		}



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal static int SetText( 
			SectionsCollection sections, 
			string text,
			MaskInfo maskInfo )
		{
            return XamMaskedEditor.SetText(sections, text, maskInfo.PromptChar, maskInfo.PadChar, 
                 maskInfo.SkipDigitSeparator);
		}
		

		#endregion //SetText

		#region EraseAllChars







		internal static void EraseAllChars( SectionsCollection sections )
		{
			if ( null != sections )
			{
				for ( int i = 0, sectionsCount = sections.Count; i < sectionsCount; i++ )
				{
					DisplayCharsCollection displayChars = sections[i].DisplayChars;

					for ( int j = 0, displayCharsCount = displayChars.Count; j < displayCharsCount; j++ )
					{
						DisplayCharBase dc = displayChars[j];

						if ( null != dc && dc.IsEditable )
							dc.Char = (char)0;
					}
				}
			}
		}

		#endregion //EraseAllChars

		#region GetDisplayCharAtPosition



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		internal static DisplayCharBase GetDisplayCharAtPosition( SectionsCollection sections, int position )
		{
			// If sections is null or if there are no sections, 
			// or if position is less than 0, then return null.
			//
			if ( null == sections || position < 0 || sections.Count <= 0 )
				return null;

			int sectionsCount = sections.Count;

			int c = 0;
			for ( int i = 0; i < sectionsCount; i++ )
			{
				SectionBase section = sections[ i ];
				
				int displayCharsCount = section.DisplayChars.Count;

				if ( position < c + displayCharsCount )
					return section.DisplayChars[ position - c ];

				c += displayCharsCount;
			}

			// if we have an invalid position then we automatically return null here
			//
			return null;
		}

		#endregion //GetDisplayCharAtPosition

		#region GetTotalNumberOfDisplayChars







		internal static int GetTotalNumberOfDisplayChars( SectionsCollection sections )
		{
			// If sections is null or if there are no sections, 
			// or if position is less than 0, then return null.
			//
			if ( null == sections || sections.Count <= 0 )
				return 0;

			int sectionsCount = sections.Count;

			int c = 0;
			for ( int i = 0; i < sectionsCount; i++ )
			{
				SectionBase section = sections[ i ];

				c += section.DisplayChars.Count;
			}

			return c;
		}

		#endregion // GetTotalNumberOfDisplayChars

		#region GetWholeNumberValue



#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

		private static object GetWholeNumberValue( 
			SectionsCollection sections,
			System.Type wholeNumberType,
			MaskInfo maskInfo )
		{
			string str = XamMaskedEditor.GetText( sections, MaskMode.Raw, maskInfo );

			if ( null == str || 0 == str.Length )
				throw new InvalidOperationException( XamMaskedEditor.GetString( "LE_InvalidOperationException_9" ) );

			// SSP 3/2/07
			// 
			//return Convert.ChangeType( str, wholeNumberType, maskInfo.FormatProvider );
			decimal val = decimal.Parse( str, maskInfo.FormatProvider );
			return Convert.ChangeType( val, wholeNumberType, maskInfo.FormatProvider );
		}

		#endregion //GetWholeNumberValue

		#region GetDoubleValue

		private static double GetDoubleValue( 
			SectionsCollection sections,
			MaskInfo maskInfo )
		{
			string integerPart = string.Empty;
			string fractionPart = string.Empty;
			bool pastDecimalSeperator = false;
			
			for ( int i = 0; i <  sections.Count; i++ )
			{
				// The section has to be either numeric, or a comma or a decimal

				SectionBase section = sections[i];

				if ( XamMaskedEditor.IsSectionNumeric( section ) )
				{
					if (!pastDecimalSeperator )
					{
						integerPart += section.GetText( MaskMode.Raw );
					}
					else
					{
						fractionPart = section.GetText( MaskMode.Raw );
					}
				}
					// SSP 1/2/04 UWM185
					// Commented out the original code and added new code below. Relaxed the 
					// conditions a bit for what kind of characters can appear in the mask
					// (to allow for % and other symbols).
					//
					// ------------------------------------------------------------------------------
					
#region Infragistics Source Cleanup (Region)


















#endregion // Infragistics Source Cleanup (Region)

				else if ( section is LiteralSection )
				{
					if ( section.DecimalSeperatorChar == section.DisplayChars[0].Char )
					{
						if ( ! pastDecimalSeperator )
						{
							pastDecimalSeperator = true;
						}
						else
						{
							// If the decimal separator is encountered twice, then something's wrong. 
							// Probably the user did not specify a valid format. However in any case
							// we should use parse below.
							//
							integerPart = null;
							fractionPart = null;
							break;
						}
					}
				}
					// If some other kind of non-literal section that's not a number section is
					// encountered, then we should use the parese method instead.
					//
				else
				{					
					// If the section is not a comma or a decimal then
					// it's not a valid number. So try the last resort below
					// 
					integerPart = null;
					fractionPart = null;
					break;
				}
				// ------------------------------------------------------------------------------					// SSP 1/2/04 UWM185
			}

			if ( null != fractionPart && null != integerPart )
			{
				try
				{
					if ( fractionPart.Length > 0 )
					{
						// SSP 2/21/03
						// Use the underlying cutlure decimal character rather than ".". 
						//
						//return Double.Parse( integerPart + "." + fractionPart );
						IFormatProvider formatProvider =	maskInfo != null ?
							maskInfo.FormatProvider :
							null;

						//	BF 2.26.03	(Related to UWE437)
						//	Use the overload of the Parse method that takes a format provider,
						//	using the owner provided IFormatProvider.
						//
						//return Double.Parse( integerPart + maskInfo.DecimalSeperatorChar + fractionPart );
						return Double.Parse( integerPart + maskInfo.DecimalSeperatorChar + fractionPart, formatProvider );
					}
					else
						return Double.Parse( integerPart );
				}
				catch ( Exception )
				{
				}
			}

			string str = XamMaskedEditor.GetText( sections, maskInfo.DataMode, maskInfo );

			return Double.Parse( str, maskInfo.FormatProvider );
		}

		#endregion //GetDoubleValue

		#region GetFloatValue

		private static float GetFloatValue( 
			SectionsCollection sections,
			MaskInfo maskInfo )
		{
			double d = (double)XamMaskedEditor.GetDoubleValue( sections, maskInfo );

			return (float)d;
		}

		#endregion //GetFloatValue

		#region GetCurrencyValue

		private static decimal GetCurrencyValue( 
			SectionsCollection sections,
			MaskInfo maskInfo )
		{
			string integerPart = string.Empty;
			string fractionPart = string.Empty;
			bool pastDecimalSeperator = false;
			
			for ( int i = 0; i <  sections.Count; i++ )
			{
				// The section has to be either numeric, or a comma or a decimal

				SectionBase section = sections[i];

				if ( XamMaskedEditor.IsSectionNumeric( section ) )
				{
					if (!pastDecimalSeperator )
					{
						integerPart += section.GetText( MaskMode.Raw );
					}
					else
					{
						fractionPart = section.GetText( MaskMode.Raw );
					}
				}
					// SSP 1/2/04 UWM185
					// Commented out the original code and added new code below. Relaxed the 
					// conditions a bit for what kind of characters can appear in the mask
					// (to allow for % and other symbols).
					//
					// ------------------------------------------------------------------------------
					
#region Infragistics Source Cleanup (Region)



















#endregion // Infragistics Source Cleanup (Region)

				else if ( section is LiteralSection )
				{
					if ( section.DecimalSeperatorChar == section.DisplayChars[0].Char )
					{
						if ( ! pastDecimalSeperator )
						{
							pastDecimalSeperator = true;
						}
						else
						{
							// If the decimal separator is encountered twice, then something's wrong. 
							// Probably the user did not specify a valid format. However in any case
							// we should use parse below.
							//
							integerPart = null;
							fractionPart = null;
							break;
						}
					}
				}
					// If some other kind of non-literal section that's not a number section is
					// encountered, then we should use the parese method instead.
					//
				else
				{					
					// If the section is not a comma or a decimal then
					// it's not a valid number. So try the last resort below
					// 
					integerPart = null;
					fractionPart = null;
					break;
				}
				// ------------------------------------------------------------------------------
			}

			if ( null != fractionPart && null != integerPart )
			{
				try
				{
					if ( fractionPart.Length > 0 )
					{
						//	BF 2.13.03	UWE437
						//
						//	We can't hardcode the '.' character here, because
						//	not all cultures use that as the decimal separator.
						//	In fact, some cultures use the '.' character as the
						//	digit grouping symbol, which means that the Parse method
						//	will not throw an exception, and the integral portion of
						//	the value will be returned. Use the culture-sensitive
						//	DecimalSeperatorChar property here instead.
						//	
						//return decimal.Parse( integerPart + "." + fractionPart );
						string decimalChar =	maskInfo != null ? 
							maskInfo.DecimalSeperatorChar.ToString() :
							System.Globalization.CultureInfo.CurrentCulture.NumberFormat.CurrencyDecimalSeparator.ToString();

						IFormatProvider formatProvider =	maskInfo != null ?
							maskInfo.FormatProvider :
							null;

						//	BF 2.26.03	(Related to UWE437)
						//	Use the overload of the Parse method that takes a format provider,
						//	using the owner provided IFormatProvider.
						//
						//return decimal.Parse( integerPart + decimalChar + fractionPart );
						// SSP 9/30/03 UWE503
						//
						//return decimal.Parse( integerPart + decimalChar + fractionPart, formatProvider );
						return decimal.Parse( integerPart + decimalChar + fractionPart, NumberStyles.Currency, formatProvider );
					}
					else
						return decimal.Parse( integerPart );
				}
				catch ( Exception )
				{
				}
			}

			string str = XamMaskedEditor.GetText( sections, maskInfo.DataMode, maskInfo );

			return decimal.Parse( str, maskInfo.FormatProvider );
		}

		#endregion //GetCurrencyValue

		#region GetDataValue



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		internal static object GetDataValue( MaskInfo maskInfo )
		{
			Type dataType = maskInfo.DataType;
			SectionsCollection sections = maskInfo.Sections;

			// Clone so we don't affect the sections being used for editing. Otherwise
			// the display will change if ValidateAllSections modifies the sections.
			// 
			sections = sections.Clone( maskInfo, true );
			ValidateAllSections( sections, true );

			try
			{
				if ( typeof( DateTime ) == dataType )
				{
					return XamMaskedEditor.GetDateTimeValue( sections );
				}
				else if ( typeof( double ) == dataType )
				{
					return XamMaskedEditor.GetDoubleValue( sections, maskInfo );
				}
				else if ( typeof( float ) == dataType )
				{
					return XamMaskedEditor.GetFloatValue( sections, maskInfo );
				}
				else if ( typeof( byte ) == dataType || typeof( sbyte ) == dataType ||
					typeof( short ) == dataType || typeof( ushort ) == dataType || typeof( Int16 )  == dataType ||
					typeof( int ) == dataType || typeof( uint ) == dataType || typeof( Int32 ) == dataType ||
					typeof( long ) == dataType || typeof( ulong ) == dataType || typeof( Int64 )  == dataType )
				{
					return XamMaskedEditor.GetWholeNumberValue( sections, dataType, maskInfo );
				}
				else if ( typeof( decimal ) == dataType )
				{
					return XamMaskedEditor.GetCurrencyValue( sections, maskInfo );
				}
				else if ( typeof( string ) == dataType )
				{
					return XamMaskedEditor.GetText( sections, maskInfo.DataMode, maskInfo );
				}
				else
				{
                    // AS 10/17/08 TFS8886
                    // Try to use the typeconverter to create an instance of
                    // the object from a string.
                    //
                    TypeConverter tc = TypeDescriptor.GetConverter(dataType);

                    if (tc.CanConvertFrom(typeof(string)))
                    {
                        string text = XamMaskedEditor.GetText(maskInfo.Sections, maskInfo.DataMode, maskInfo);
                        return tc.ConvertFromString(null, maskInfo.CultureInfo, text);
                    }

					Debug.Assert( !XamMaskedEditor.SupportsDataType( dataType ), "A suported data type but can't recognize it." );
					throw new ArgumentException( XamMaskedEditor.GetString( "LE_NotSupportedException_2", dataType.Name ), "dataType" );
				}
			}
			catch ( Exception e )
			{

				throw new ArgumentException( XamMaskedEditor.GetString( "LE_ArgumentException_5" ), e );
			}
		}
		
		#endregion //GetDataValue
			
		#region GetSection

		internal static SectionBase GetSection( SectionsCollection sections, Type sectionType )
		{
			int temp = 0;
			return XamMaskedEditor.GetSection( sections, sectionType, ref temp );
		}

		internal static SectionBase GetSection( SectionsCollection sections, Type sectionType, ref int index )
		{
			if ( null == sections )
				return null;

			for ( int i = index, count = sections.Count; i < count; i++ )
			{
				// SSP 4/9/05 BR03077
				// Also find the matching derived classes.
				//
				//if ( sections[ i ].GetType() == sectionType )
				Type type = sections[ i ].GetType();
				
				// JJD 6/14/07
				// Use IsAssignableFrom instead of IsSubclassOf since that is 10x more efficient
				//if (type == sectionType || type.IsSubclassOf(sectionType))
				if (sectionType.IsAssignableFrom(type))
				{
					index = 1 + 1;
					return sections[i];
				}
			}
			return null;
		}

		#endregion //GetSection

        #region HasNonDateTimeEditSections

        // SSP 5/14/10 TFS32082
        // 
        internal static bool HasNonDateTimeEditSections( SectionsCollection sections )
        {
            for ( int i = 0, count = sections.Count; i < count; i++ )
            {
                EditSectionBase editSection = sections[i] as EditSectionBase;
                if ( null != editSection && !IsDateTimeSection( editSection ) )
                    return true;
            }

            return false;
        }

        #endregion // HasNonDateTimeEditSections

        #region IsDateTimeSection

        // SSP 5/14/10 TFS32082
        // 
        /// <summary>
        /// Returns true if the specified edit section is year, month, day, hour, minute, second or am/pm section.
        /// </summary>
        /// <param name="editSection"></param>
        /// <returns></returns>
        internal static bool IsDateTimeSection( EditSectionBase editSection )
        {
            return editSection is YearSection
                || editSection is MonthSection
                || editSection is DaySection
                || editSection is HourSection
                || editSection is MinuteSection
                || editSection is SecondSection
                || editSection is AMPMSection;
        }

        #endregion // IsDateTimeSection

		#region PadFractionPartHelper

		// SSP 8/30/11 TFS76307
		// Refactored. Moved existing code from SetCurrencyValue.
		// 
		private void PadFractionPartHelper( SectionsCollection sections )
		{
			FractionPart fractionPart = (FractionPart)XamMaskedEditor.GetSection( sections, typeof( FractionPart ) );
			if ( null != fractionPart )
			{
				// SSP 8/30/11 TFS76307
				// Added TrimFractionalZeros property. Also note that for FractionPartContinuous we always need to
				// pad because the input in FractionPartContinuous is from right to left where the input is
				// padded to left with 0's.
				// 
				//fractionPart.PadWithZero( );
				if ( fractionPart is FractionPartContinuous || ! this.TrimFractionalZeros )
					fractionPart.PadWithZero( );
				// SSP 9/14/11 TFS76307
				// 
				else if ( this.TrimFractionalZeros )
					fractionPart.TrimInsignificantZeros( );
			}
		} 

		#endregion // PadFractionPartHelper

		#region SetDateTimeValue



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

		internal static void SetDateTimeValue( 
			SectionsCollection sections, 
			DateTime date,
			MaskInfo maskInfo,
			// SSP 3/6/09 TFS15024
			// Added retainOriginalTime.
			// 
			bool retainOriginalTime
			)
		{
			HourSection	   hourSection   = null;
			MinuteSection  minuteSection = null;
			SecondSection  secondSection = null;
			MonthSection   monthSection  = null;
			DaySection	   daySection    = null;
			YearSection    yearSection   = null;
			AMPMSection    ampmSection	 = null;

            // AS 8/25/08 Support Calendar
            Debug.Assert(null != maskInfo);
            System.Globalization.Calendar calendar = sections.Calendar;

            monthSection = (MonthSection)XamMaskedEditor.GetSection(sections, typeof(MonthSection));
			daySection = (DaySection)XamMaskedEditor.GetSection( sections, typeof( DaySection ) );
			yearSection = (YearSection)XamMaskedEditor.GetSection( sections, typeof( YearSection ) );
			hourSection = (HourSection)XamMaskedEditor.GetSection( sections, typeof( HourSection ) );
			minuteSection = (MinuteSection)XamMaskedEditor.GetSection( sections, typeof( MinuteSection ) );
			secondSection = (SecondSection)XamMaskedEditor.GetSection( sections, typeof( SecondSection ) );
			ampmSection = (AMPMSection)XamMaskedEditor.GetSection( sections, typeof( AMPMSection ) );

			//	BF 6.13.03	UWE610
			//
			//	In the UWE610 scenario, this whole block was being skipped
			//	because one or more of these sections were null. I relaxed
			//	this conditional to allow us to get in here if any section
			//	is non-null, so that we populate as much of the mask as we
			//	can given however many sections exist.
			



			if ( yearSection != null	||	monthSection != null	||
				daySection != null		||	hourSection != null		||
				minuteSection != null	||	secondSection != null )
			{

				if ( null != yearSection )
                    // AS 8/25/08 Support Calendar
                    //yearSection.SetText( date.Year.ToString( ).PadLeft( yearSection.NumberOfDigits, '0' ) );
					yearSection.SetText( calendar.GetYear(date).ToString( ).PadLeft( yearSection.NumberOfDigits, '0' ) );

				if ( null != monthSection )
                    // AS 8/25/08 Support Calendar
                    //monthSection.SetText( date.Month.ToString( ).PadLeft( monthSection.NumberOfDigits, '0' ) );
					monthSection.SetText( calendar.GetMonth(date).ToString( ).PadLeft( monthSection.NumberOfDigits, '0' ) );

				if ( null != daySection )
                    // AS 8/25/08 Support Calendar
                    //daySection.SetText( date.Day.ToString( ).PadLeft( daySection.NumberOfDigits, '0' ) );
					daySection.SetText( calendar.GetDayOfMonth(date).ToString( ).PadLeft( daySection.NumberOfDigits, '0' ) );

				// SSP 3/6/09 TFS15024
				// Added retainOriginalTime. Enclosed the existing code into the if block.
				// 
				if ( !retainOriginalTime )
				{
					if ( null != ampmSection )
						ampmSection.SetText( date.Hour >= 12 ? ampmSection.PMValue : ampmSection.AMValue );

					if ( null != hourSection )
					{
						// If there is an AM-PM section, then convert the 24-hour hour into
						// 12-hour hour.
						// 
						if ( null != ampmSection )
						{
							int hour = date.Hour;

							if ( 0 == hour )
								hour = 12;
							if ( hour > 12 )
								hour -= 12;

							hourSection.SetText( hour.ToString( ) );
						}
						else
						{
							// If there is no AM-PM section, then keep the hour based 
							// on 24-hour time format.
							//
							hourSection.SetText( date.Hour.ToString( ) );
						}
					}

					if ( null != minuteSection )
						minuteSection.SetText( date.Minute.ToString( ) );

					if ( null != secondSection )
						secondSection.SetText( date.Second.ToString( ) );
				}
			}
			else 
			{
				// SSP 6/27/11 TFS77173
				// 
				// --------------------------------------------------------------
				if ( retainOriginalTime )
				{
					try
					{
						DateTime currVal = GetDateTimeValue( sections );
						date = date.Date.Add( currVal.TimeOfDay );
					}
					catch
					{
					}
				}
				// --------------------------------------------------------------

				XamMaskedEditor.SetText( sections, date.ToString( maskInfo.Format, maskInfo.FormatProvider ), maskInfo );
			}
		}
		
		#endregion //SetDateTimeValue

		#region SetCurrencyValue

		private static void SetCurrencyValue( 
			SectionsCollection sections,
			decimal val,
			MaskInfo maskInfo )
		{
			// SSP 1/14/03 UWE410
			// If we have number and fraction sections, then set their texts directly rather
			// than converting the whole decimal to the string and applying the whole mask.
			// The reason for doing this is when they specify a format string of "C", the
			// resulting converted string contains commas (for example "$1,000.00"), which
			// can't be assigned to the digit characters that the number section is composed
			// so it doesn't set the rest of the characters.
			//
			// -----------------------------------------------------------------------------
			NumberSection numberSection = (NumberSection)XamMaskedEditor.GetSection( sections, typeof( NumberSection ) );
			FractionPart fractionSection = (FractionPart)XamMaskedEditor.GetSection( sections, typeof( FractionPart ) );

			// SSP 6/20/03 UWE606
			// A common situation is where the mask is something like "nnnn" where they want 
			// to enter the integer portion only (like in currencies that are rounded to dollars).
			// This fix was actually prompted by the situtaion where the mask was ".nnnn" where
			// they were entering percentages. However the same could apply to currencies and
			// integer portions.
			//
			//if ( null != numberSection && null != fractionSection )
			if ( null != numberSection || null != fractionSection )
			{
				// Erase everything first.
				//
				XamMaskedEditor.EraseAllChars( sections );

				// SSP 2/10/04 UWE764
				// Round the number to the number of places in the fraction part. For example we
				// want 0.99999999989 to become 1.0 when we only have 2 decimal places in the
				// fraction part.
				// 
				val = Math.Round( val, null != fractionSection ? fractionSection.DisplayChars.Count : 0 );

				// SSP 2/29/12 TFS92791
				// If the mask is not specified and auto-generated, expand it if we encounter a value that's
				// larger than our auto-generated mask.
				// 
				XamMaskedEditor editor = maskInfo.MaskEditor;
				bool isAutoGeneratedMask = null != editor && string.IsNullOrEmpty( editor.Mask );

				// SSP 3/20/03 UWG2084
				//
				//decimal ip = decimal.Floor( val );
				//decimal fp = val - ip;
				decimal ip = val;
				decimal fp = 0.0m;
				if ( val >= 0.0m )
				{
					ip = decimal.Floor( val );
					fp = val - ip;

					// SSP 6/20/03 UWE606
					// Related to change above. Check for null since we changed the condition for the
					// above for the if statement.
					// 
					if ( null != numberSection )
					{
						// SSP 2/29/12 TFS92791
						// 
						//numberSection.SetText( ip.ToString( ) );
						numberSection.SetText( ip.ToString( ), isAutoGeneratedMask );
					}

					// SSP 6/20/03 UWE606
					// Related to change above. Check for null since we changed the condition for the
					// above for the if statement.
					// 
					if ( null != fractionSection )
						fractionSection.SetFractionValue( (double)fp );
				}
				else
				{
					ip = decimal.Floor( val );

					if ( ip != val )
					{
						ip++;
						fp = - val + ip;
					}

					// SSP 6/20/03 UWE606
					// Related to change above. Check for null since we changed the condition for the
					// above for the if statement.
					// 
					if ( null != numberSection )
					{
						// SSP 2/29/12 TFS92791
						// 
						//numberSection.SetText( 0.0m == ip ? "-0" : ip.ToString( ) );
						numberSection.SetText( 0.0m == ip ? "-0" : ip.ToString( ), isAutoGeneratedMask );
					}

					// SSP 6/20/03 UWE606
					// Related to change above. Check for null since we changed the condition for the
					// above for the if statement.
					// 
					if ( null != fractionSection )
						fractionSection.SetFractionValue( (double)fp );
				}
			
				return;
			}
			// -----------------------------------------------------------------------------

			// SSP 3/5/03 UWE488
			// Remove the leading 0. The reason for doing this is that if they have a mask that begins
			// with ".", then ToString returns a string that starts with "0" like "0.923" and the first character in
			// that string doesn't match the "." display character and the SetText fails. Besides 
			// removing leading 0's doesn't change the value and shouldn't do any harm.
			//
			//XamMaskedEditor.SetText( sections, val.ToString( maskInfo.Format, maskInfo.FormatProvider ), maskInfo );
			string text = val.ToString( maskInfo.Format, maskInfo.FormatProvider );

			if ( text.Length > 2 && text.StartsWith( "0." )  )
				text = text.Remove( 0, 1 );

			XamMaskedEditor.SetText( sections, text, maskInfo );

			// SSP 7/22/02
			// Fill remaining empty characters in the fraction porition of the currency mask
			// with zeros.
			//
			// SSP 8/30/11 TFS76307
			// Refactored. Moved the code into PadFractionPartHelper.
			// 
			// ------------------------------------------------------------------------------------
			maskInfo.MaskEditor.PadFractionPartHelper( sections );
			//FractionPart fractionPart = (FractionPart)XamMaskedEditor.GetSection( sections, typeof( FractionPart ) );
			//if ( null != fractionPart )
			//    fractionPart.PadWithZero( );
			// ------------------------------------------------------------------------------------
		}

		#endregion //SetCurrencyValue

		#region SetDoubleValue

		private static void SetDoubleValue( 
			SectionsCollection sections,
			double val,
			MaskInfo maskInfo )
		{
			// SSP 1/14/03 UWE410
			// If we have number and fraction sections, then set their texts directly rather
			// than converting the whole decimal to the string and applying the whole mask.
			// The reason for doing this is when they specify a format string of "C", the
			// resulting converted string contains commas (for example "$1,000.00"), which
			// can't be assigned to the digit characters that the number section is composed
			// so it doesn't set the rest of the characters.
			//
			// -----------------------------------------------------------------------------
			NumberSection numberSection = (NumberSection)XamMaskedEditor.GetSection( sections, typeof( NumberSection ) );
			FractionPart fractionSection = (FractionPart)XamMaskedEditor.GetSection( sections, typeof( FractionPart ) );

			// SSP 6/20/03 UWE606
			// A common situation is where the mask is something like ".nnnn" where they want 
			// to enter the fractions only (like in percentages).
			//
			//if ( null != numberSection && null != fractionSection )
			if ( null != numberSection || null != fractionSection )
			{
				// Erase everything first.
				//
				XamMaskedEditor.EraseAllChars( sections );

				// SSP 3/16/09 TFS6232
				// Handle infinity and NaN values.
				// 
				if ( Utils.IsInfinityOrNaN( val ) )
				{
					// Infinity and NaN can't be represented in the mask (as it only accepts digits).
					// Therefore simply return after erasing all the display chars.
					// 
					return;
				}

				// SSP 2/10/04 UWE764
				// Round the number to the number of places in the fraction part. For example we
				// want 0.99999999989 to become 1.0 when we only have 2 decimal places in the
				// fraction part.
				// 
				// MRS 9/1/04 - UWE1041
				// Round can't handle more than 15 digits.  
				// val = Math.Round( val, null != fractionSection ? fractionSection.DisplayChars.Count : 0 );
				val = Math.Round( val, null != fractionSection ? Math.Min(fractionSection.DisplayChars.Count,15) : 0 );

				// SSP 2/29/12 TFS92791
				// If the mask is not specified and auto-generated, expand it if we encounter a value that's
				// larger than our auto-generated mask.
				// 
				XamMaskedEditor editor = maskInfo.MaskEditor;
				bool isAutoGeneratedMask = null != editor && string.IsNullOrEmpty( editor.Mask );

				// SSP 3/20/03 UWG2084
				//
				//double ip = Math.Floor( val );
				//double fp = val - ip;
				double ip = val;
				double fp = 0.0;
				if ( val >= 0.0 )
				{
					ip = Math.Floor( val );
					fp = val - ip;

					// SSP 6/20/03 UWE606
					// Related to change above. Check for null since we changed the condition for the
					// above for the if statement.
					// 
					if ( null != numberSection )
                    {
                        // MRS 1/23/06 - BR09080
                        // To ToString method of a double returns
                        // scientific notation for anything over 
                        // 15 digits by default. We want the raw
                        // number here. 
                        //
                        //numberSection.SetText( ip.ToString( ) );
						// SSP 2/29/12 TFS92791
						// 
                        //numberSection.SetText(ip.ToString("G17"));
						// SSP 5/10/12 TFS110448
						// If the magnitude of the number is greater than 17 decimals then G17 will result in
						// scientific notation which is not suitable for SetText call.
						// 
						//numberSection.SetText( ip.ToString( "G17" ), isAutoGeneratedMask );
						numberSection.SetText( ip.ToString( "G28" ), isAutoGeneratedMask );
                    }

					// SSP 6/20/03 UWE606
					// Related to change above. Check for null since we changed the condition for the
					// above for the if statement.
					// 
					if ( null != fractionSection )
						fractionSection.SetFractionValue( fp );
				}
				else
				{
					ip = Math.Floor( val );
					
					if ( ip != val )
					{
						ip++;
						fp = - val + ip;
					}

					// SSP 6/20/03 UWE606
					// Related to change above. Check for null since we changed the condition for the
					// above for the if statement.
					// 
					if ( null != numberSection )
					{
						// SSP 2/29/12 TFS92791
						// 
						//numberSection.SetText( 0.0 == ip ? "-0" : ip.ToString( ) );
						numberSection.SetText( 0.0 == ip ? "-0" : ip.ToString( ), isAutoGeneratedMask );
					}

					// SSP 6/20/03 UWE606
					// Related to change above. Check for null since we changed the condition for the
					// above for the if statement.
					// 
					if ( null != fractionSection )
						fractionSection.SetFractionValue( fp );
				}

				// SSP 6/20/03
				// This was accidently left over here. Above if-else statement already does this.
				//
				



				
				return;
			}
			// -----------------------------------------------------------------------------

			// SSP 3/5/03 UWE488
			// Remove the leading 0. The reason for doing this is that if they have a mask that begins
			// with ".", then ToString returns a string that starts with "0" like "0.923" and the first character in
			// that string doesn't match the "." display character and the SetText fails. Besides 
			// removing leading 0's doesn't change the value and shouldn't do any harm.
			//
			//XamMaskedEditor.SetText( sections, val.ToString( maskInfo.Format, maskInfo.FormatProvider ), maskInfo );
			string text = val.ToString( maskInfo.Format, maskInfo.FormatProvider );

			if ( text.Length > 2 && text.StartsWith( "0." )  )
				text = text.Remove( 0, 1 );

			XamMaskedEditor.SetText( sections, text, maskInfo );

			// SSP 7/22/02
			// Fill remaining empty characters in the fraction porition of the currency mask
			// with zeros.
			//
			// SSP 8/30/11 TFS76307
			// Refactored. Moved the code into PadFractionPartHelper.
			// 
			// ------------------------------------------------------------------------------------
			maskInfo.MaskEditor.PadFractionPartHelper( sections );
			//FractionPart fractionPart = (FractionPart)XamMaskedEditor.GetSection( sections, typeof( FractionPart ) );
			//if ( null != fractionPart )
			//    fractionPart.PadWithZero( );
			// ------------------------------------------------------------------------------------
		}

		#endregion //SetDoubleValue

		#region SetFloatValue

		private static void SetFloatValue( 
			SectionsCollection sections,
			float val,
			MaskInfo maskInfo )
		{
			XamMaskedEditor.SetDoubleValue( sections, (double)val, maskInfo );
		}

		#endregion //SetFloatValue

		#region SetDataValue



#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

		// SSP 5/19/09 - Clipboard Support
		// Changed the return type from void to bool.
		// 
		internal static bool SetDataValue( 
			SectionsCollection sections, 
			System.Type dataType,
			object val,
			MaskInfo maskInfo )
		{
			// MD 4/22/11 - TFS73181
			// Moved most of the code to the new overload.
			try
			{
				dataType = Utilities.GetUnderlyingType(dataType);

				return XamMaskedEditor.SetDataValueHelper(sections, dataType, val, maskInfo);
			}
			finally
			{
				// Pad the fraction portion of a number section with 0's so 1.__ becomes 1.00 etc.
				//
				XamMaskedEditor.ValidateAllSections(sections);
			}
		}

		internal static bool SetDataValueHelper(
			SectionsCollection sections,
			System.Type dataType,
			object val,
			MaskInfo maskInfo)
		{
			// If val is null or DBNull, then delete all the text 
			// and return 
			//
			if ( null == val || val is System.DBNull )
			{
				XamMaskedEditor.SetText( sections, "", maskInfo );
				
				
				
				return true;
			}

			if ( !XamMaskedEditor.SupportsDataType( dataType ) )
			{

				throw new ArgumentException( XamMaskedEditor.GetString( "LE_NotSupportedException_2", dataType.Name ), "dataType" );
			}

            // Check for Nullable types
			// MD 4/22/11 - TFS73181
			// We don't need to do this because we already resolved the underlying type above.
            //dataType = Utilities.GetUnderlyingType(dataType);

			if ( val.GetType( ) != dataType )
			{
				// AS 7/20/11 TFS81229
				// In the OnCoerceValue of the editor, we use the ConvertDataValue to change the data type 
				// so to be consistent we need to do that here as well. This also allows us to consider the 
				// format.
				//
				//val = Convert.ChangeType( val, dataType, maskInfo.FormatProvider );
				val = Utilities.ConvertDataValue( val, dataType, maskInfo.FormatProvider, maskInfo.Format );
			}

			
			
			
			bool retVal = true;

			if ( typeof( DateTime ) == dataType )
			{
				// SSP 3/6/09 TFS15024
				// Pass along the new retainOriginalTime parameter.
				// 
				//XamMaskedEditor.SetDateTimeValue( sections, (DateTime)val, maskInfo );
				XamMaskedEditor.SetDateTimeValue( sections, (DateTime)val, maskInfo, false );
			}
			else if ( typeof( decimal ) == dataType )
			{
				XamMaskedEditor.SetCurrencyValue( sections, (decimal)val, maskInfo );
			}
			else if ( typeof( double ) == dataType )
			{
				XamMaskedEditor.SetDoubleValue( sections, (double)val, maskInfo );
			}
			else if ( typeof( float ) == dataType )
			{
				XamMaskedEditor.SetFloatValue( sections, (float)val, maskInfo );
			}
			else if ( typeof( string ) == dataType )
			{
				
				
				
				//XamMaskedEditor.SetText( sections, (string)val, maskInfo );
				string text = (string)val;
				int processedChars = XamMaskedEditor.SetText( sections, text, maskInfo );
				if ( null != text && processedChars < text.Length )
					return false;
				
			}
			else if ( typeof( byte ) == dataType || typeof( sbyte ) == dataType ||
				typeof( short ) == dataType || typeof( ushort ) == dataType || typeof( Int16 )  == dataType ||
				typeof( int ) == dataType || typeof( uint ) == dataType || typeof( Int32 ) == dataType ||
				typeof( long ) == dataType || typeof( ulong ) == dataType || typeof( Int64 )  == dataType )
			{
				
				
				
				
				string text = val.ToString( );
				int processedChars = XamMaskedEditor.SetText( sections, text, maskInfo );
				if ( null != text && processedChars < text.Length )
					retVal = false;
				
			}
			else
			{
                // AS 10/17/08 TFS8886
                // Try to update the sections using the typeconverter associated
                // with the type. I had to add an overload of the SetText method
                // since the mask may be something like nn,nn where the n's are not
                // part of a single numeric value and therefore the , should not 
                // be ignored.
                //
                TypeConverter tc = TypeDescriptor.GetConverter(dataType);

                if (null != tc && tc.CanConvertTo(typeof(string)))
                {
                    string strValue = tc.ConvertToString(null, maskInfo.CultureInfo, val);
					int processedChars = XamMaskedEditor.SetText( sections, strValue, maskInfo.PromptChar, maskInfo.PadChar, false );
					
					
					if ( null != strValue && processedChars < strValue.Length )
						retVal = false;
                }
                else
                {
                    Debug.Assert(!XamMaskedEditor.SupportsDataType(dataType), "A suported data type but can't recognize it.");

                    throw new ArgumentException(XamMaskedEditor.GetString("LE_NotSupportedException_2", dataType.Name), "dataType");
                }
			}

			// MD 4/22/11 - TFS73181
			// We don't need to do this here anymore. Now we will do this only in SetDataValue.
			//// Pad the fraction portion of a number section with 0's so 1.__ becomes 1.00 etc.
			////
			//XamMaskedEditor.ValidateAllSections( sections );			

			return retVal;
		}

		#endregion //SetDataValue

		#region ValidateAllSections



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		internal static bool ValidateAllSections( SectionsCollection sections )
		{
			return ValidateAllSections( sections, false );
		}







		internal bool IsInputValid( SectionsCollection sections, out string maskValidationErrorMessage )
		{
			maskValidationErrorMessage = null;

			// If all display characters are empty and the editor is nullable then return true.
			// 
			if ( this.IsNullable && AreAllDisplayCharsEmpty( sections ) )
				return true;

			// This is for AutoFillDate logic. We need to clone the sections because as part of
			// the validation logic we need to modify its contents. We don't want to change what's
			// currently displayed. Therefore clone what's being validated.
			// 
			sections = sections.Clone( sections.MaskInfo, true );
			ValidateAllSections( sections, true );

			for ( int i = 0; i < sections.Count; i++ )
			{
				EditSectionBase editSection = sections[i] as EditSectionBase;

				// An empty number section should be considered a valid number section
				// with a value of 0.
				//
				if ( null != editSection &&
					typeof( NumberSection ) == editSection.GetType( ) )
				{
					NumberSection numberSection = (NumberSection)editSection;

					// MD 4/22/11 - TFS73181
					// Moved from below so we don't have to get it multiple times.
					string str = numberSection.GetText(MaskMode.Raw);

					// SSP 9/1/09 TFS18219
					// Validate the value against the range specified in "{number:min-max}" mask.
					// 
					string tmpErrorMessage;

					// MD 4/22/11 - TFS73181
					// Since we already have the raw text, pass it along so ValidateAgainstMinMaxHelper doesn't have to re-get it.
					//if ( ! numberSection.ValidateAgainstMinMaxHelper( out tmpErrorMessage ) )
					if (!numberSection.ValidateAgainstMinMaxHelper(str, out tmpErrorMessage))
					{
						maskValidationErrorMessage = tmpErrorMessage;
						return false;
					}

					// MD 4/22/11 - TFS73181
					// Moved above below so we don't have to get it multiple times.
					//string str = numberSection.GetText( MaskMode.Raw );

					if ( null == str || str.Length == 0 )
						continue;
				}
                
				// literals are skipped

				if ( null != editSection && !editSection.ValidateSection( false ) )
				{
					maskValidationErrorMessage = XamMaskedEditor.GetString("MaskValidationErrorInputDoesNotMatchMask");
					return false;
				}
			}

			MonthSection	monthSection	= XamMaskedEditor.GetSection( sections, typeof( MonthSection ) ) as MonthSection;			
			DaySection		daySection		= XamMaskedEditor.GetSection( sections, typeof( DaySection ) ) as DaySection;
			YearSection		yearSection		= XamMaskedEditor.GetSection( sections, typeof( YearSection ) ) as YearSection;

			if ( null != monthSection && null != daySection )
			{
				int month = monthSection.ToInt( );
				int day = daySection.ToInt( );  
              	
				// If february and day is over 29, it's an invalid date
				if ( 2 == month && day > 29 )
				{
					maskValidationErrorMessage = XamMaskedEditor.GetString( "MaskValidationErrorInvalidDate" );
					return false;
				}

				int daysInMonth = 30 + ( month + ( month >= 8 ? 1 : 0 ) ) % 2;

				if ( day > daysInMonth )
				{
					maskValidationErrorMessage = XamMaskedEditor.GetString( "MaskValidationErrorInvalidDayOfMonth" );
					return false;
				}

				// if an year is given, then check for February's leap
				// year
				if ( null != yearSection )
				{
					// MD 7/11/06 - BR14269 
					// A 2-digit year of 00 was causing a bug here, 
					// get the year value, not the int value
					//int year = yearSection.ToInt( );
					int year = yearSection.GetYear();

                    // AS 10/8/08 Support Calendar
                    //if ( day > DateTime.DaysInMonth( year, month ) )
                    System.Globalization.Calendar cal = sections.Calendar;
					if ( day > cal.GetDaysInMonth( year, month ) )
					{
						maskValidationErrorMessage = XamMaskedEditor.GetString( "MaskValidationErrorInvalidDayOfMonth" );
						return false;
					}
				}
			}

			// Clear the MaskValidationErrorMessage if validation succeeded.
			// 
			maskValidationErrorMessage = null;

			return true;		
		}



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		internal static bool ValidateAllSections( SectionsCollection sections, bool loosingFocus )
		{
			bool b = true;

			for ( int i = 0; i < sections.Count; i++ )
			{
				EditSectionBase editSection = sections[i] as EditSectionBase;

				if ( null != editSection )
					b = b && editSection.ValidateSection( );

				// SSP 5/8/02
				// Pad the fraction section with 0s. so if we have 10.__ it will become
				// 10.00 and 10.5_ will become 10.50
				//
				if ( editSection is FractionPart )
				{
					NumberSection numberSection = editSection.PreviousEditSection as NumberSection;

					bool flag = false;

					if ( null != numberSection )
					{
						string str = numberSection.GetText( MaskMode.Raw );

						if ( null != str && str.Length > 0 )
							flag = true;
					}

					if ( ! flag )
					{
						
						//string str = ((FractionPart)editSection).GetText( MaskMode.Raw );
						string str = editSection.GetText(MaskMode.Raw);

						if ( null != str && str.Length > 0 )
							flag = true;
					}

					if ( flag )
					{
						// SSP 8/30/11 TFS76307
						// Refactored. Moved the code into PadFractionPartHelper.
						// 
						//( (FractionPart)editSection ).PadWithZero( );
						if ( null != sections.MaskedEditor )
							sections.MaskedEditor.PadFractionPartHelper( sections );
					}
				}
			}

			// Apply AutoFillDate logic.
			// 
			// --------------------------------------------------------------------------------
			if ( loosingFocus )
			{
				AutoFillDate autoFillType = sections.MaskedEditor.AutoFillDate;
				DateTime autoFillValue = DateTime.Now;

                // AS 8/25/08 Support Calendar
                System.Globalization.Calendar calendar = sections.Calendar;

				MonthSection monthSection = (MonthSection)XamMaskedEditor.GetSection( sections, typeof( MonthSection ) );
				DaySection daySection = (DaySection)XamMaskedEditor.GetSection( sections, typeof( DaySection ) );
				YearSection yearSection = (YearSection)XamMaskedEditor.GetSection( sections, typeof( YearSection ) );

				if ( 
					// If month is not filled
					null != monthSection && monthSection.IsEmpty 
					// And day is filled
					&& null != daySection && ! daySection.IsEmpty 
					&& AutoFillDate.MonthAndYear == autoFillType )
				{
					// Auto-fill the month.
                    // AS 8/25/08 Support Calendar
                    //monthSection.SetText( autoFillValue.Month.ToString( ) );
					monthSection.SetText( calendar.GetMonth(autoFillValue).ToString( ) );
				}

				if ( 
					// If the year is not filled
					null != yearSection && yearSection.IsEmpty
					// And month is filled
					&& null != monthSection && ! monthSection.IsEmpty
					// And day is non-existant or is filled
					&& ( null == daySection || ! daySection.IsEmpty ) 
					&& ( AutoFillDate.Year == autoFillType || AutoFillDate.MonthAndYear == autoFillType ) )
				{
					// Auto-fill the year.
                    // AS 8/25/08 Support Calendar
                    //yearSection.SetText( autoFillValue.Year.ToString( ) );
					yearSection.SetText( calendar.GetYear(autoFillValue).ToString( ) );
				}
			}
			// --------------------------------------------------------------------------------

			// When loosing focus, set the value of AM/PM section depending on the value of the 
			// hour section.
			// 
			// ------------------------------------------------------------------------------------
			if ( b && loosingFocus && null != sections.MaskInfo && null != sections.MaskedEditor )
			{
				// SSP 9/15/11 TFS87816
				// Do this regardless of HasFormat or  DisplayFormattedTextWhenNotFocused.
				// 
				//if ( sections.MaskedEditor.DisplayFormattedTextWhenNotFocused && sections.MaskInfo.HasFormat )
				//{
					try
					{
						AMPMSection ampmSection = (AMPMSection)XamMaskedEditor.GetSection( sections, typeof( AMPMSection ) );
						HourSection hourSection = (HourSection)XamMaskedEditor.GetSection( sections, typeof( HourSection ) );
						if ( null != ampmSection && null != hourSection && ampmSection.IsEmpty && ! hourSection.IsEmpty )
						{
							int hour = hourSection.ToInt( );
							if ( hour >= 12 )
								ampmSection.SetText( ampmSection.PMValue );
							else 
								ampmSection.SetText( ampmSection.AMValue );
						}
					}
					catch 
					{
					}
				//}
			}
			// ------------------------------------------------------------------------------------

			return b;
		}

		#endregion // ValidateAllSections

		#region DeduceEditAsType



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		internal static System.Type DeduceEditAsType( SectionsCollection sections )
		{
            bool hasDateComponents, hasTimeComponents;
            return DeduceEditAsType( sections, out hasDateComponents, out hasTimeComponents );
        }

        // SSP 5/14/10 TFS32082
        // Added an overload that takes in hasDateComponents and hasTimeComponents parameters.
        // 


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

        internal static System.Type DeduceEditAsType( SectionsCollection sections, out bool hasDateComponents, out bool hasTimeComponents )
        {
			System.Type deducedEditAsType = null;
            hasDateComponents = hasTimeComponents = false;

			try
			{
				MonthSection monthSection = (MonthSection)XamMaskedEditor.GetSection( sections, typeof( MonthSection ) );
				DaySection   daySection   = (DaySection)XamMaskedEditor.GetSection( sections, typeof( DaySection ) );
				YearSection  yearSection  = (YearSection)XamMaskedEditor.GetSection( sections, typeof( YearSection ) );
				HourSection	   hourSection   = (HourSection)XamMaskedEditor.GetSection( sections, typeof( HourSection ) );
				MinuteSection  minuteSection = (MinuteSection)XamMaskedEditor.GetSection( sections, typeof( MinuteSection ) );
				SecondSection  secondSection = (SecondSection)XamMaskedEditor.GetSection( sections, typeof( SecondSection ) );

				hasDateComponents = 
					( null != monthSection && null != daySection ) 
					|| ( null != monthSection && null != yearSection );
				hasTimeComponents = 
					( null != hourSection && null != minuteSection );
						
				if ( hasDateComponents || hasTimeComponents )
				{
					deducedEditAsType = typeof( DateTime );
				}

				if ( null == deducedEditAsType )
				{
					bool hasIntegerPart = false;
					bool hasFractionPart = false;
					bool pastDecimalSeperator = false;
					for ( int i = 0, count = sections.Count; i <  count; i++ )
					{
						// SSP 1/25/02 UWG985
						// Changed the call from IsSectionNumeric to NumberSection
						//
						if ( sections[i] is NumberSection || ( pastDecimalSeperator && sections[i] is FractionPart ) )
						{
							if (!pastDecimalSeperator )
							{
								// SSP 6/14/02 UWG1179								
								// If we already encountered a number section before and
								// we encounter a second one, then this is probably not
								// a number mask.
								//
								if ( hasIntegerPart )
								{
									hasIntegerPart = false;
									hasFractionPart = false;
									break;
								}

								hasIntegerPart = true;
							}
							else
							{
								// SSP 6/14/02 UWG1179								
								// If we already encountered a number section before and
								// we encounter a second one, then this is probably not
								// a number mask.
								//
								if ( hasFractionPart )
								{
									hasIntegerPart = false;
									hasFractionPart = false;
									break;
								}

								hasFractionPart = true;
							}
						}
							// SSP 1/24/02 UWG985
							// 
							



						else if ( sections[i] is LiteralSection && 
							1 == sections[i].DisplayChars.Count )
						{
							if (  sections[i].DecimalSeperatorChar == sections[i].DisplayChars[0].Char )
							{								
								// SSP 1/24/02 UWG985
								// If we enocunter a second decimal seperator, then it's not
								// a number portion either.
								//
								if ( pastDecimalSeperator )
								{
									hasIntegerPart = false;
									hasFractionPart = false;
									break;
								}

								pastDecimalSeperator = true;
							}
							else if ( sections[i].CommaChar != sections[i].DisplayChars[0].Char )
							{
								// SSP 1/24/02 UWG985
								// If we enocunter something other than a comma or a decimal seperator
								// after the integer portion, then this is not a number mask.
								//
								if ( hasIntegerPart )
								{
									hasIntegerPart = false;
									hasFractionPart = false;
									break;
								}
							}							
						}
						else
						{
							// SSP 1/24/02 UWG985
							// We encountered a section that is more than one character wide
							// after having an integer section.
							//
							if ( hasIntegerPart || pastDecimalSeperator )
							{
								hasIntegerPart = false;
								hasFractionPart = false;
								break;
							}
								// SSP 6/14/02 UWG1179
								// If it's not an integer, decimal or comma, then
								// this is not a number seciton mask.
								//
							else if ( !( sections[i] is LiteralSection ) )
							{
								hasIntegerPart = false;
								hasFractionPart = false;
								break;
							}
						}
					}

					if ( hasIntegerPart && hasFractionPart )
						deducedEditAsType = typeof( decimal );
					else if ( hasIntegerPart )
						deducedEditAsType = typeof( int );
				}
				
				if ( null == deducedEditAsType )
				{
					// Remember we can always work with strings.
					//
					if ( sections.Count > 0 )
						deducedEditAsType = typeof( string );
				}
			}
			catch ( Exception )
			{
			}

			return deducedEditAsType;
		}

		#endregion //DeduceEditAsType

		#region OnIsKeyboardFocusWithinChanged

		/// <summary>
		/// Overridden.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnIsKeyboardFocusWithinChanged( DependencyPropertyChangedEventArgs e )
		{
			base.OnIsKeyboardFocusWithinChanged( e );

			bool focused = (bool)e.NewValue;

			// SSP 7/19/07 BR22754
			// Moved code from here into ProcessIsBeingEditedAndFocusedChanged.
			// 
			this.ProcessIsBeingEditedAndFocusedChanged( focused );
		}

        // AS 9/5/08 NA 2008 Vol 2
        //private void ProcessIsBeingEditedAndFocusedChanged( )
		internal void ProcessIsBeingEditedAndFocusedChanged( )
		{
			MaskInfo maskInfo = this.MaskInfo;
			bool isBeingEditedAndFocused = null != maskInfo && maskInfo.IsBeingEditedAndFocused;
			this.ProcessIsBeingEditedAndFocusedChanged( isBeingEditedAndFocused );
		}

		// SSP 7/19/07 BR22754
		// Added ProcessIsBeingEditedAndFocusedChanged helper method. Code in there was moved from
		// OnIsKeyboardFocusWithinChanged.
		// 
		private void ProcessIsBeingEditedAndFocusedChanged( bool focused )
		{
			if ( this.IsInEditMode && null != this.EditInfo )
			{
				// SSP 3/23/09 IME
				// 
				this.EditInfo.HookUnhookInputMethodHelper( focused );

				SectionsCollection sections = this.MaskInfo.Sections;
				if ( null != sections )
				{
					foreach ( SectionBase section in sections )
					{
						foreach ( DisplayCharBase dc in section.DisplayChars )
						{
							dc.NotifyPropertyChangedEvent( DisplayCharBase.PROPERTY_DRAWSTRING );
							dc.NotifyPropertyChangedEvent( DisplayCharBase.PROPERTY_VISIBILITY );
							dc.NotifyPropertyChangedEvent( DisplayCharBase.PROPERTY_DRAWSELECTED );
						}
					}

					// Update DrawAsSelected on display character elements.
					// 
					this.SyncDrawAsSelectedOnDisplayPresenters( );
				}

                // AS 9/5/08 NA 2008 Vol 2
                // We want to hide the caret when the dropdown is open.
                //
                if (focused && this.HasOpenDropDown)
                    focused = false;

				if ( focused )
				{
					// SSP 9/8/09 TFS21334
					// Use the new ShowHideCaretHelper method which checks for additional condition
					// of having selected text to not show the caret to be consistent with the WPF
					// TextBox, which doesn't show the caret when there's some text selected.
					// 
					//this.ShowCaretElement( );
					this.ShowHideCaretHelper( );

					// When the editor receives focus, make sure the display character is scrolled
					// into view.
					// Also only do this if the control is getting the focus other than through the 
					// masked edit element's mouse down. Because the mouse down handler will do
					// the scrolling itself. We don't want to do it here prematurely.
					//
					if ( this.IsInEditMode && null != this.EditInfo )
						this.EditInfo.ScrollDisplayCharIntoView( this.EditInfo.CaretPosition );
				}
				else
				{
					if ( null != this.EditInfo )
					{
						// Validate the sections like we do in Leave. This will fill the rest of
						// a 4 digit year section (if only two digits were entered in it).
						//
						// Added new overload of ValidateAllSections that takes in loosingFocus
						// parameter. Pass that as true here.
						// 
						this.EditInfo.ValidateAllSections( true );

						this.EditInfo.EnsureMaskEditAreaFilled( );
					}

					// SSP 9/8/09 TFS21334
					// Use the new ShowHideCaretHelper method which checks for additional condition
					// of having selected text to not show the caret to be consistent with the WPF
					// TextBox, which doesn't show the caret when there's some text selected.
					// 
					//this.HideCaretElement( );
					this.ShowHideCaretHelper( );
				}
			}
			else
			{
				// SSP 9/8/09 TFS21334
				// 
				this.ShowHideCaretHelper( );
			}
		}

		#endregion // OnIsKeyboardFocusWithinChanged

		#region OnInvalidOperation






		internal void OnInvalidOperation( InvalidOperationEventArgs e )
		{
		}

		#endregion //OnInvalidOperation

		#region OnInvalidChar






		internal void OnInvalidChar( InvalidCharEventArgs e )
		{
			// if nobody is listening then just return
			//
			if ( null == this.InvalidChar )
				return;
            
			try
			{
				// fire the event
				//
				this.InvalidChar( this, e );
			}
			finally
			{
			}
		}

		#endregion //OnInvalidChar

		#region EnsureInEditMode






		internal void EnsureInEditMode( )
		{
			// Throw an exception if not in edit mode.
			//
			if ( !this.IsInEditMode )
				throw new InvalidOperationException( XamMaskedEditor.GetString( "LE_InvalidOperationException_10" ) );
		}
		
		#endregion  //EnsureInEditMode

		#region VerifyElementsPositioned

		internal void VerifyElementsPositioned( )
		{
			this.UpdateLayout( );
		}

		#endregion // VerifyElementsPositioned

		#region DefaultCultureInfo

		// JJD 4/27/07
		// Optimization - cache the property locally
		[ThreadStatic()]
		private static System.Globalization.CultureInfo s_defaultCultureInfo;

		internal static System.Globalization.CultureInfo DefaultCultureInfo
		{
			get
			{
				// JJD 4/27/07
				// Optimization - cache the property locally
				// Note: the cached value get cleared in the MeasureOverride
				//return System.Globalization.CultureInfo.CurrentCulture;
				if ( s_defaultCultureInfo == null )
					s_defaultCultureInfo = System.Globalization.CultureInfo.CurrentCulture;

				return s_defaultCultureInfo;
			}
		}

		#endregion // DefaultCultureInfo

		#region GetNumberFormatInfo



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal static NumberFormatInfo GetNumberFormatInfo( IFormatProvider formatProvider )
		{
			NumberFormatInfo nfi = formatProvider as NumberFormatInfo;
			
			if ( null == nfi && null != formatProvider )
				nfi = (NumberFormatInfo)formatProvider.GetFormat( typeof( NumberFormatInfo ) );

            // JJD 6/6/08 - Optimization
            // This showed up as a minor hit during profiling. There is no need to 
            // do a get of XamMaskedEditor.DefaultCultureInfo twice
            //if ( null == nfi && null != XamMaskedEditor.DefaultCultureInfo )
            //    nfi = XamMaskedEditor.DefaultCultureInfo.NumberFormat;
            if (null == nfi)
            {
                CultureInfo ci = XamMaskedEditor.DefaultCultureInfo;

                if (ci != null)
                    nfi = ci.NumberFormat;
            }

			return nfi;
		}

		#endregion // GetNumberFormatInfo

		#region GetDateTimeFormatInfo



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal static DateTimeFormatInfo GetDateTimeFormatInfo( IFormatProvider formatProvider )
		{
			DateTimeFormatInfo dfi = formatProvider as DateTimeFormatInfo;

			if ( null == dfi && null != formatProvider )
				dfi = (DateTimeFormatInfo)formatProvider.GetFormat( typeof( DateTimeFormatInfo ) );

			if ( null == dfi && null != XamMaskedEditor.DefaultCultureInfo )
				dfi = XamMaskedEditor.DefaultCultureInfo.DateTimeFormat;

			return dfi;
		}

		#endregion // GetNumberFormatInfo

		#region NotifyPropertyOnDisplayCharacters

		internal void NotifyDisplayCharDrawStringsChanged( )
		{
			SectionsCollection sections = this.Sections;
			XamMaskedEditor.NotifyPropertyOnDisplayCharacters( sections, DisplayCharBase.PROPERTY_DRAWSTRING );
		}

		internal static void NotifyPropertyOnDisplayCharacters( SectionsCollection sections, string propertyName )
		{
			if ( null != sections )
			{
				foreach ( DisplayCharBase dc in sections.AllDisplayCharacters )
					dc.NotifyPropertyChangedEvent( propertyName );
			}
		}

		internal static void NotifyPropertyOnDisplayCharacters( SectionBase section, string propertyName )
		{
			if ( null != section )
			{
				DisplayCharsCollection displayChars = section.DisplayChars;
				if ( null != displayChars )
				{
					for ( int i = 0, count = displayChars.Count; i < count; i++ )
					{
						DisplayCharBase dc = displayChars[i];
						dc.NotifyPropertyChangedEvent( propertyName );
					}
				}
			}
		}

		#endregion // NotifyPropertyOnDisplayCharacters

        // AS 8/25/08 Support Calendar
        #region GetCultureCalendar



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

        internal static System.Globalization.Calendar GetCultureCalendar(IFormatProvider formatProvider)
        {
            DateTimeFormatInfo dateTimeFormatInfo = XamMaskedEditor.GetDateTimeFormatInfo(formatProvider);

            Debug.Assert(null != dateTimeFormatInfo);

            return dateTimeFormatInfo.Calendar;
        }

        #endregion // GetCultureCalendar

        #region GetCultureChar

        internal static char GetCultureChar( char c, IFormatProvider formatProvider )
		{
			return XamMaskedEditor.GetCultureChar( c, formatProvider, false );
		}

		internal static char GetCultureChar( char c, MaskInfo maskInfo )
		{
			bool useCurrencySymbols = null != maskInfo && typeof( decimal ) == maskInfo.DataType;

			return XamMaskedEditor.GetCultureChar( c, null != maskInfo ? maskInfo.FormatProvider : null, useCurrencySymbols );
		}

		internal static char GetCultureChar( char c, IFormatProvider formatProvider, bool useCurrencySymbols )
		{
			NumberFormatInfo numberFormatInfo = XamMaskedEditor.GetNumberFormatInfo( formatProvider );
			DateTimeFormatInfo dateTimeFormatInfo = XamMaskedEditor.GetDateTimeFormatInfo( formatProvider );

			string retVal = null;

			switch ( c )
			{
				// Decimal separater.
				case '.' :
					if ( !useCurrencySymbols )
						retVal = numberFormatInfo.NumberDecimalSeparator;
					else
						retVal = numberFormatInfo.CurrencyDecimalSeparator;
					break;

				// Thousands separator.
				case ',' :
					if ( !useCurrencySymbols )
						retVal = numberFormatInfo.NumberGroupSeparator;
					else
						retVal = numberFormatInfo.CurrencyGroupSeparator;
					break;

				// Time separator.
				case ':' :
					retVal = dateTimeFormatInfo.TimeSeparator;
					break;

				// Date separator.
				case '/' :
					retVal = dateTimeFormatInfo.DateSeparator;
					break;

					// Positive symbol.
				case '+' :
					retVal = numberFormatInfo.PositiveSign;
					break;

					// Negative symbol.
				case '-' :
					retVal = numberFormatInfo.NegativeSign;
					break;

				// Currency symbol.
				case '$':
					retVal = numberFormatInfo.CurrencySymbol;
					break;
			}

			return null != retVal && retVal.Length > 0 ? retVal[0] : c;
		}

		// AS 9/13/04 UWM200
		// The localized string could be empty so I added a helper routine to 
		// handle retreiving a character.
		//
		private static char GetChar( string value, int position )
		{
			if ( value == null || value.Length <= position )
				return '\0';

			return value[position];
		}

		#endregion //GetCultureChar

		#region GetNumberSectionCount







		internal static int GetNumberSectionCount( SectionsCollection sections )
		{
			int numberSectionCount = 0;
			foreach ( SectionBase section in sections )
			{
				if ( XamMaskedEditor.IsSectionNumeric( section ) )
					numberSectionCount++;
			}

			return numberSectionCount;
		}

		#endregion // GetNumberSectionCount

		#region GetCultureAMPMDesignator



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal static string GetCultureAMPMDesignator( bool am, IFormatProvider formatProvider )
		{
			DateTimeFormatInfo dateTimeFormatInfo = XamMaskedEditor.GetDateTimeFormatInfo( formatProvider );

			return am ? dateTimeFormatInfo.AMDesignator : dateTimeFormatInfo.PMDesignator;
		}

		#endregion // GetCultureAMPMDesignator

		#region IllegalOperation

		internal void IllegalOperation( string message )
		{
			InvalidOperationEventArgs e = new InvalidOperationEventArgs( message );

			this.OnInvalidOperation( e );

			if ( e.Beep )
			{
				this.Beep( );
			}
		}

		#endregion //IllegalOperation

		#region UpdateSpinButtonVisibility

		// SSP 10/5/09 - NAS10.1 Spin Buttons
		// 
		private void UpdateSpinButtonVisibility( )
		{
			bool isVisible = false;

			if ( !this.IsReadOnly )
			{
				SpinButtonDisplayMode spinButtonDisplayMode = this.SpinButtonDisplayMode;
				switch ( spinButtonDisplayMode )
				{
					case SpinButtonDisplayMode.Always:
						isVisible = true;
						break;
					case SpinButtonDisplayMode.Focused:
						isVisible = this.IsInEditMode || this.IsFocusWithin;
						break;
					case SpinButtonDisplayMode.OnlyInEditMode:
						isVisible = this.IsInEditMode;
						break;
					case SpinButtonDisplayMode.MouseOver:
						isVisible = this.IsInEditMode || this.IsMouseOver;
						break;
					case SpinButtonDisplayMode.Never:
						break;
					default:
						Debug.Assert( false, "Unknown SpinButtonDisplayMode value." );
						break;
				}
			}

			this.SetValue( SpinButtonVisibilityResolvedPropertyKey,
				isVisible ? KnownBoxes.VisibilityVisibleBox : KnownBoxes.VisibilityCollapsedBox );
		}

		#endregion // UpdateSpinButtonVisibility

		// MD 4/25/11 - TFS73181
		#region TryFastSyncOnValueChanged

		private bool TryFastSyncOnValueChanged(object newValue, out Exception error)
		{
			error = null;

			MaskInfo maskInfo = this.MaskInfo;

			// Try to determine the Type for which the default mask was created. If a default mask wasn't created based on type, 
			// the GetDefaultTypeForSyncTextFromValue method will return null.
			Type defaultTypeForSyncTextWithValue = maskInfo.GetDefaultTypeForSyncTextFromValue(newValue);
			if (defaultTypeForSyncTextWithValue == null)
				return false;

			this.BeginSyncValueProperties();

			try
			{
				// Determine whether the ValueToTextConverterResolved is the default converter.
				bool hasDefaultValueToTextConverter = this.ValueToTextConverterResolved is MaskedEditorDefaultConverter;

				// If we have the default converter, skip the conversion for now because we know we will be able to convert the
				// value to text. We can get the text after updating the text in the sections. Otherwise, force the conversion.
				string text = null;
				if ( hasDefaultValueToTextConverter ||
					this.ConvertValueToText( newValue, out text, out error ) )
				{
					// Update the sections on the mask info.
					XamMaskedEditor.SetDataValueHelper( maskInfo.Sections, defaultTypeForSyncTextWithValue, newValue, maskInfo );

					// If we had the default value ot text converter and skipped the conversion above, do it now, but not
					// by calling the ConvertValueToText method, which will cause a cloned MaskInfo to get initialized.
					// Instead, get the text from this MaskInfo directly.
					if ( hasDefaultValueToTextConverter )
						text = XamMaskedEditor.GetText( maskInfo.Sections, maskInfo.DataMode, maskInfo );

					// Set the Text property.
					this.SetValue( ValueEditor.TextProperty, text );

					// Sync the display text as well. We can speed things up a bit here if we have a default value to display text 
					// converter. If that is the case, don't call SyncDisplayText, which will call ConvertValueToDisplayText, which
					// will cause a cloned MaskInfo to get initialized. // Instead, get the display text from this MaskInfo directly.
					if (
						// SSP 6/1/11 TFS77397
						// If there's a Format then fallback to the old logic which will convert the value using the format
						// and will not make clones of the sections. Added the check for Format being null.
						// 
						string.IsNullOrEmpty( maskInfo.Format )
						&& this.ValueToDisplayTextConverterResolved is MaskedEditorDefaultConverter )
					{
						string displayText = XamMaskedEditor.GetText( maskInfo.Sections, maskInfo.DisplayMode, maskInfo );
						this.SetValue( TextEditorBase.DisplayTextPropertyKey, text );
					}
					else
					{
						this.SyncDisplayText( );
					}

					// Since the default mask was created based on type, all values of the type will be valid, so there is no need for
					// normal logic to validate against the mask. So set the falg which will skip the next mask validation.
					_skipNextMaskValidation = true;

					return true;
				}
				else
				{
					// If the value can't be converted to text then consider it an invalid value.
					this.SetIsValueValid( false, error );
					return false;
				}
			}
			// SSP 5/10/12 TFS110448
			// Added the catch block. SetDataValueHelper call above can throw an exception if numeric value is too large.
			// 
			catch
			{
				return false;
			}
			finally
			{
				this.EndSyncValueProperties();
			}
		} 

		#endregion // TryFastSyncOnValueChanged

		#endregion //Private/Internal methods

		#region Public properties/methods

		#region TrimFractionalZeros

		// SSP 8/30/11 TFS76307
		// 

		/// <summary>
		/// Identifies the <see cref="TrimFractionalZeros"/> dependency property
		/// </summary>
		public static readonly DependencyProperty TrimFractionalZerosProperty = DependencyProperty.Register(
			"TrimFractionalZeros",
			typeof( bool ),
			typeof( XamMaskedEditor ),
			new FrameworkPropertyMetadata( KnownBoxes.FalseBox, new PropertyChangedCallback( OnTrimFractionalZerosChanged ) )
		);

		private static void OnTrimFractionalZerosChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			XamMaskedEditor editor = (XamMaskedEditor)d;
			editor._cachedTrimFractionalZeros = (bool)e.NewValue;
		}

		/// <summary>
		/// Specifies whether to trim insignificant zero's in fraction part of numeric masks. Default value is <b>False</b>.
		/// </summary>
		/// <remarks>
		/// <b>TrimFractionalZeros</b> property specifies whether to trim insignificant zero's in fraction
		/// part of numeric masks. By defualt fraction part is padded with zero's to the right. If this 
		/// property is set to <i>true</i>, this padding of zero's will not occur and furthermore any
		/// insignificant zero's will be removed.
		/// </remarks>
		/// <seealso cref="TrimFractionalZerosProperty"/>
		public bool TrimFractionalZeros
		{
			get
			{
				return _cachedTrimFractionalZeros;
			}
			set
			{
				this.SetValue( TrimFractionalZerosProperty, value );
			}
		}

		#endregion // TrimFractionalZeros

		#region SpinButtonDisplayMode

		// SSP 10/5/09 - NAS10.1 Spin Buttons
		// 

		/// <summary>
		/// Identifies the <see cref="SpinButtonDisplayMode"/> dependency property.
		/// </summary>
		[InfragisticsFeature( FeatureName = FeatureInfo.FeatureName_SpinButtons, Version = FeatureInfo.Version_10_1 )]
		public static readonly DependencyProperty SpinButtonDisplayModeProperty = DependencyProperty.Register(
			"SpinButtonDisplayMode",
			typeof( SpinButtonDisplayMode ),
			typeof( XamMaskedEditor ),
			new FrameworkPropertyMetadata( SpinButtonDisplayMode.Never )
		);

		/// <summary>
		/// Specifies if and when to display spin buttons which are used to increment or decrement the editor value.
		/// Default value is <b>Never</b>.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>SpinButtonDisplayMode</b> specifies if and when to display the spin buttons in the control. Spin buttons 
		/// allow the user to increment and decrement the current value in the editor. By default the value of the current
		/// section (section where the caret is) is incremented or decremented. If you specify the <see cref="SpinIncrement"/>
		/// property then the whole value of the editor will be incremented or decremented by that value depending upon
		/// whether the up or down spin button is pressed, respectively.
		/// </para>
		/// </remarks>
		/// <seealso cref="SpinIncrement"/>
		/// <seealso cref="SpinWrap"/>
		//[Description( "Specifies if and when to display spin buttons." )]
		//[Category( "Behavior" )]
		[Bindable( true )]
		[InfragisticsFeature( FeatureName = FeatureInfo.FeatureName_SpinButtons, Version = FeatureInfo.Version_10_1 )]
		public SpinButtonDisplayMode SpinButtonDisplayMode
		{
			get
			{
				return (SpinButtonDisplayMode)this.GetValue( SpinButtonDisplayModeProperty );
			}
			set
			{
				this.SetValue( SpinButtonDisplayModeProperty, value );
			}
		}

		#endregion // SpinButtonDisplayMode

		#region SpinButtonStyle

		/// <summary>
		/// Identifies the <see cref="SpinButtonStyle"/> dependency property.
		/// </summary>
		[InfragisticsFeature( FeatureName = FeatureInfo.FeatureName_SpinButtons, Version = FeatureInfo.Version_10_1 )]
		public static readonly DependencyProperty SpinButtonStyleProperty = DependencyProperty.Register(
			"SpinButtonStyle",
			typeof( Style ),
			typeof( XamMaskedEditor ),
			new FrameworkPropertyMetadata( null, FrameworkPropertyMetadataOptions.None )
		);

		/// <summary>
		/// Used for setting the Style of the spin buttons which are instances of RepeatButton class. Default value is null.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// Default value of this property is null. You can use this property to specify a Style object to use for the
		/// spin buttons, which are RepeatButton instances, displayed inside the editor.
		/// </para>
		/// </remarks>
		/// <seealso cref="SpinButtonDisplayMode"/>
		/// <seealso cref="SpinIncrement"/>
		//[Description( "Used for setting the Style of the spin buttons" )]
		//[Category( "Behavior" )]
		[Bindable( true )]
		[InfragisticsFeature( FeatureName = FeatureInfo.FeatureName_SpinButtons, Version = FeatureInfo.Version_10_1 )]
		public Style SpinButtonStyle
		{
			get
			{
				return (Style)this.GetValue( SpinButtonStyleProperty );
			}
			set
			{
				this.SetValue( SpinButtonStyleProperty, value );
			}
		}

		#endregion // SpinButtonStyle

		#region SpinButtonVisibilityResolved

		// SSP 10/5/09 - NAS10.1 Spin Buttons
		// 

		/// <summary>
		/// Identifies the property key for read-only <see cref="SpinButtonVisibilityResolved"/> dependency property.
		/// </summary>
		private static readonly DependencyPropertyKey SpinButtonVisibilityResolvedPropertyKey = DependencyProperty.RegisterReadOnly(
			"SpinButtonVisibilityResolved",
			typeof( Visibility ),
			typeof( XamMaskedEditor ),
			new FrameworkPropertyMetadata( Visibility.Collapsed )
		);

		/// <summary>
		/// Identifies the read-only <see cref="SpinButtonVisibilityResolved"/> dependency property.
		/// </summary>
		[InfragisticsFeature( FeatureName = FeatureInfo.FeatureName_SpinButtons, Version = FeatureInfo.Version_10_1 )]
		public static readonly DependencyProperty SpinButtonVisibilityResolvedProperty = SpinButtonVisibilityResolvedPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the value indicating whether the spin buttons should be displayed.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>SpinButtonVisibilityResolved</b> property returns the resolved value indicating 
		/// the visibility of the spin buttons in the control. This property is used by the
		/// control template to control the visibility of the spin buttons.
		/// </para>
		/// <para class="body">
		/// Set the <see cref="SpinButtonDisplayMode"/> property to control if and when the
		/// spin buttons are displayed.
		/// </para>
		/// </remarks>
		/// <seealso cref="SpinButtonDisplayMode"/>
		//[Description( "Indicates the visibility of the spin buttons." )]
		//[Category( "Behavior" )]
		[Bindable( true )]
		[Browsable( false )]
		[ReadOnly( true )]
		[EditorBrowsable( EditorBrowsableState.Advanced )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		[InfragisticsFeature( FeatureName = FeatureInfo.FeatureName_SpinButtons, Version = FeatureInfo.Version_10_1 )]
		public Visibility SpinButtonVisibilityResolved
		{
			get
			{
				return (Visibility)this.GetValue( SpinButtonVisibilityResolvedProperty );
			}
		}

		#endregion // SpinButtonVisibilityResolved

		#region SpinIncrement

		// SSP 10/2/09 - NAS10.1 Spin Buttons
		// Added SpinIncrement property.
		// 

		/// <summary>
		/// Identifies the <see cref="SpinIncrement"/> dependency property.
		/// </summary>
		[InfragisticsFeature( FeatureName = FeatureInfo.FeatureName_SpinButtons, Version = FeatureInfo.Version_10_1 )]
		public static readonly DependencyProperty SpinIncrementProperty = DependencyProperty.Register(
			"SpinIncrement",
			typeof( object ),
			typeof( XamMaskedEditor ),
			new FrameworkPropertyMetadata( null, FrameworkPropertyMetadataOptions.None,
				new PropertyChangedCallback( OnSpinIncrementChanged ) )
		);

		/// <summary>
		/// Specifies the amount by which to increase or decrease the value of the editor when 
		/// up or down spin button is clicked.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>SpinIncrement</b> property specifies the amount by which the value of the editor
		/// will be increased or decreased when up or down spin button is clicked, respectively.
		/// </para>
		/// <para class="body">
		/// The amount one specifies depends on the type of values that the editor edits. 
		/// When the editor is used for numeric values then the spin increment amount can be 
		/// specified as a numeric value in the form of any numeric type (for example 5, 10.5 etc...), 
		/// as long as the type can be converted to match the editor's <see cref="ValueType"/>.
		/// A special string vlaue of "log" is supported for numeric types where the editor's
		/// value is incremented in an accelerated fashion when the mouse button is held pressed 
		/// over the spin button for a certain amount of time.
		/// </para>
		/// <para class="body">
		/// When date and time values are being edited, you can specify the amount as a TimeSpan
		/// instance or as one of the following tokens.
		/// <list type="bullet">
		/// 
		/// <item>
		/// <term>"1d"</term>
		/// <description>
		/// Date will be incremented or decrement by 1 day. You can specify
		/// a different integer value for the number of days, for example "10d".
		/// </description>
		/// </item>
		/// 
		/// <item>
		/// <term>"1m"</term>
		/// <description>
		/// Depending on the mask type, date or time will be incremented or decrement 
		/// by 1 month or 1 minute. If the mask is a date mask, month will be affected.
		/// If the mask is a time mask, minute will be affect. You can specify
		/// a different integer value for the number of months or minutes, for example 
		/// "2m".
		/// </description>
		/// </item>
		/// 
		/// <item>
		/// <term>"1y"</term>
		/// <description>
		/// Date will be incremented or decrement by 1 year. You can specify
		/// a different integer value for the number of years, for example "2y".
		/// </description>
		/// </item>
		/// 
		/// <item>
		/// <term>"1h"</term>
		/// <description>
		/// Time will be incremented or decrement by 1 hour. You can specify
		/// a different integer value for the number of hours, for example "2h".
		/// </description>
		/// </item>
		/// 
		/// <item>
		/// <term>"1s"</term>
		/// <description>
		/// Time will be incremented or decrement by 1 second. You can specify
		/// a different integer value for the number of hours, for example "2s".
		/// </description>
		/// </item>
		/// 
		/// </list>
		/// </para>
		/// </remarks>
		/// <seealso cref="SpinWrap"/>
		//[Description( "The amount by which to increase or decrease the value of the editor via spin buttons." )]
		//[Category( "Data" )]
		[Bindable( true )]
		[InfragisticsFeature( FeatureName = FeatureInfo.FeatureName_SpinButtons, Version = FeatureInfo.Version_10_1 )]
		public object SpinIncrement
		{
			get
			{
				return (object)this.GetValue( SpinIncrementProperty );
			}
			set
			{
				this.SetValue( SpinIncrementProperty, value );
			}
		}

		/// <summary>
		/// Returns true if the SpinIncrement has been set to a valid value.
		/// </summary>
		internal bool HasSpinIncrement
		{
			get
			{
				return null != this.SpinIncrement;
			}
		}

		private static void OnSpinIncrementChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			XamMaskedEditor maskedEditor = (XamMaskedEditor)dependencyObject;
			object newVal = (object)e.NewValue;
			
			maskedEditor._cachedSpinInfo = null != newVal ? SpinInfo.Parse( maskedEditor, newVal ) : null;
		}

		#endregion // SpinIncrement

		#region SpinWrap

		/// <summary>
		/// Identifies the <see cref="SpinWrap"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty SpinWrapProperty = DependencyProperty.Register(
			"SpinWrap",
			typeof( bool ),
			typeof( XamMaskedEditor ),
			new FrameworkPropertyMetadata( KnownBoxes.FalseBox )
			);

		/// <summary>
		/// Returns or sets a value indicating whether the control's spin buttons should wrap its value. Default value is <b>False</b>.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// If True the spin button will wrap the value incremented/decremented based on its Min/Max value.
		/// When incrementing the value and the value is already at its maximum, the value will wrap to 
		/// minimum value. The same applies when decrementing the value.
		/// </p>
		/// <p class="body">
		/// To actually specify the minimum and maximum value, use the <see cref="ValueEditor.ValueConstraint"/>
		/// property.
		/// </p>
		/// </remarks>
		public bool SpinWrap
		{
			get
			{
				return (bool)this.GetValue( SpinWrapProperty );
			}
			set
			{
				this.SetValue( SpinWrapProperty, value );
			}
		}

		#endregion // SpinWrap

		#region Beep

		/// <summary>
		/// Calls the MessageBeep api
		/// </summary>
		internal void Beep( )
		{
			try
			{
				System.Media.SystemSounds.Beep.Play();
			}
			catch
			{
				Debug.Assert(false);
			}
		}

		#endregion //Beep
	
		#region Sections

		// SSP 8/7/07 
		// Made sections read-only.
		//
		
#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)


		private static readonly DependencyPropertyKey SectionsPropertyKey = DependencyProperty.RegisterReadOnly(
					"Sections",
					typeof( SectionsCollection ),
					typeof( XamMaskedEditor ),
					new FrameworkPropertyMetadata( null, FrameworkPropertyMetadataOptions.None,
                        // AS 9/3/08 NA 2008 Vol 2
						//null )
						new PropertyChangedCallback(OnSectionsChanged) )
				);

        // AS 9/3/08 NA 2008 Vol 2
        private static void OnSectionsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((XamMaskedEditor)d).OnSectionsChanged();
        }

        // AS 9/3/08 NA 2008 Vol 2
        internal virtual void OnSectionsChanged()
        {
        }

		/// <summary>
		/// Identifies the Read-Only <see cref="Sections"/> dependency property
		/// </summary>
		public static readonly DependencyProperty SectionsProperty = SectionsPropertyKey.DependencyProperty;

		/// <summary>
		/// A collection of the sections used in the control. Returns
		/// a valid collection only if the mask has been parsed yet.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// When mask is parsed the result is a collection of <see cref="SectionBase"/> derived
		/// objects. This property returns that collection. Each SectionBase object has 
		/// a collection of its display characters return via its <see cref="SectionBase.DisplayChars"/>
		/// property. XamMaskedEditor also exposes a collection that contains aggregate display characters 
		/// of all sections via its <see cref="XamMaskedEditor.DisplayChars"/> property.
		/// </para>
		/// <para class="body">
		/// This property is useful for example if you want to query and find out the structure 
		/// of the parsed mask or to query and/or manipulate the current user input on a per
		/// section or per display character basis.
		/// </para>
		/// <seealso cref="XamMaskedEditor.DisplayChars"/>
		/// </remarks>
		[ ReadOnly( true ), DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden ) ]
		public SectionsCollection Sections
		{
			get
			{
				return (SectionsCollection)this.GetValue( SectionsProperty );
			}
			// SSP 8/7/07 
			// Made sections read-only.
			// 
			
#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		}

		// SSP 8/7/07 
		// Made sections read-only.
		// 
		internal void InternalSetSections( SectionsCollection sections )
		{
			this.SetValue( SectionsPropertyKey, sections );
		}

		#endregion // Sections

		#region DisplayChars
		
		/// <summary>
		/// A collection of the display characters used in the control. Returns
		/// a valid collection only if the mask has been parsed yet.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// Returns a collection of display characters. When mask is parsed, result is
		/// a collection of sections where each section corresponds with a part of the
		/// mask. Each section in turn has a collection of <see cref="DisplayCharBase"/>
		/// derived objects each of which correspond to a placeholder character in the 
		/// part of the mask associated with the section. DisplayChars returns the 
		/// aggregate display character instances from all sections.
		/// </para>
		/// <para class="body">
		/// See <see cref="Sections"/> for more information on potential usage of this
		/// and Sections property.
		/// </para>
		/// <seealso cref="Sections"/>
		/// </remarks>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
		[ReadOnly( true ), Bindable( false )] 
		public DisplayCharsCollection DisplayChars
		{
			get
			{
                // SSP 5/6/10 TFS31789
                // There's no reason to have this restriction. We can return display chars even when 
                // not in edit mode.
                // 
                // ----------------------------------------------------------------------------------
				//if ( null == this.EditInfo )
				//	return null;

				//SectionsCollection sections = this.EditInfo.Sections;
                SectionsCollection sections = this.Sections;
                // ----------------------------------------------------------------------------------

				if ( this._displayChars_Sections != sections )
				{
					if ( null == this._displayChars )
						this._displayChars = new DisplayCharsCollection( );

					this._displayChars.Clear( );
					
					if ( null != sections )
					{
						for ( int i = 0; i < sections.Count; i++ )
						{
							SectionBase section = sections[i];

							for ( int j = 0; j < section.DisplayChars.Count; j++ )
								this._displayChars.Add( section.DisplayChars[j] );
						}
					}
				
					this._displayChars_Sections = sections;
				}

				return this._displayChars;
			}
		}
		
		#endregion //DisplayChars

		#region DesignMode

		internal bool DesignMode
		{
			get
			{
				return false;
			}
		}

		#endregion // DesignMode

		#region RegisterDefaultMaskForType

		/// <summary>
		/// Changes the default mask used by the masked editor for the specified data type.
		/// </summary>
		/// <param name="dataType"></param>
		/// <param name="mask"></param>
		/// <remarks>
		/// <para class="body">
		/// If a mask is not explicitly specified on a masked editor or derived editor, 
		/// a default is calculated based on the data type. You can override these 
		/// default calculated masks using this method.
		/// </para>
		/// <seealso cref="GetDefaultMaskForType(Type)"/>
		/// </remarks>
		public static void RegisterDefaultMaskForType( Type dataType, string mask )
		{
			lock ( g_defaultMaskTable )
			{
				if ( null != mask && mask.Length > 0 )
				{
					g_defaultMaskTable[dataType] = mask;
					g_defaultMaskTableInitialized = true;
				}
				else
				{
					if ( g_defaultMaskTable.ContainsKey( dataType ) )
					{
						g_defaultMaskTable.Remove( dataType );
						if ( 0 == g_defaultMaskTable.Count )
							g_defaultMaskTableInitialized = false;
					}
				}
			}
		}

		#endregion // RegisterDefaultMaskForType

		#region ResetDefaultMaskForType

		/// <summary>
		/// Resets the default mask for the specified data type to the default value.
		/// </summary>
		/// <param name="dataType"></param>
		/// <remarks>
		/// <para class="body">
		/// This method is used to revert any default masks that were registered using 
		/// <see cref="RegisterDefaultMaskForType"/> method. The mask will be reverted back
		/// to the default mask that's calculated by the masked editor.
		/// </para>
		/// </remarks>
		public static void ResetDefaultMaskForType( Type dataType )
		{
			RegisterDefaultMaskForType( dataType, null );
		}

		#endregion // ResetDefaultMaskForType

		#region GetDefaultMaskForType

		/// <summary>
		/// Returns the default mask that will be used for the specified data type by the masked editor
		/// and derived editors for the specified data type.
		/// </summary>
		/// <param name="dataType"></param>
		/// <returns></returns>
		/// <remarks>
		/// <para class="body">
		/// You can change the default masks using the <see cref="RegisterDefaultMaskForType"/> static method.
		/// </para>
		/// </remarks>
		public static string GetDefaultMaskForType( Type dataType )
		{
			// MD 4/25/11 - TFS73181
			// Moved all code to the new overload.
			MaskDefaultType maskDefaultType;
			return XamMaskedEditor.GetDefaultMaskForType(dataType, out maskDefaultType);
		}

		// MD 4/25/11 - TFS73181
		// Added a new overload which has an out parameter for the MaskDefaultType.
		private static string GetDefaultMaskForType(Type dataType, out MaskDefaultType maskDefaultType)
		{
			// MD 4/25/11 - TFS73181
			// Initialize the MaskDefaultType out parameter.
			maskDefaultType = MaskDefaultType.None;

			if ( g_defaultMaskTableInitialized )
			{
				lock( g_defaultMaskTable )
				{
					string mask = g_defaultMaskTable[dataType] as string;
					if ( null != mask && mask.Length > 0 )
						return mask;
				}
			}

			// MD 4/25/11 - TFS73181
			//return GetDefaultMaskForTypeHelper( dataType );
			return GetDefaultMaskForTypeHelper(dataType, out maskDefaultType);
		}

		#endregion // GetDefaultMaskForType		

		#region GetDefaultMaskForTypeHelper

		// MD 4/25/11 - TFS73181
		//private static string GetDefaultMaskForTypeHelper( Type type )
		private static string GetDefaultMaskForTypeHelper(Type type, out MaskDefaultType maskDefaultType)
		{
			// MD 4/25/11 - TFS73181
			// Initialize the MaskDefaultType out parameter.
			maskDefaultType = MaskDefaultType.None;

			type = Utilities.GetUnderlyingType( type );

			string mask = null;

			if ( typeof( DateTime ) == type )
			{
				mask = "{date}";

				// MD 4/25/11 - TFS73181
				maskDefaultType = MaskDefaultType.DateTime;
			}
			else if ( typeof( byte ) == type )
			{
				mask = "{number:0-255}";

				// MD 4/25/11 - TFS73181
				maskDefaultType = MaskDefaultType.Byte;
			}
			else if ( typeof( sbyte ) == type )
			{
				mask = "{number:-128-127}";

				// MD 4/25/11 - TFS73181
				maskDefaultType = MaskDefaultType.SByte;
			}
			else if ( typeof( short ) == type )
			{
				mask = "{number:-32768-32767}";

				// MD 4/25/11 - TFS73181
				maskDefaultType = MaskDefaultType.Int16;
			}
			else if ( typeof( ushort ) == type )
			{
				mask = "{number:0-65535}";

				// MD 4/25/11 - TFS73181
				maskDefaultType = MaskDefaultType.UInt16;
			}
			else if ( typeof( int ) == type )
			{
				mask = "{number:-2147483648-2147483647}";

				// MD 4/25/11 - TFS73181
				maskDefaultType = MaskDefaultType.Int32;
			}
			else if ( typeof( uint ) == type )
			{
				mask = "{number:0-4294967295}";

				// MD 4/25/11 - TFS73181
				maskDefaultType = MaskDefaultType.UInt32;
			}
			else if ( typeof( long ) == type )
			{
				
				
				
				
				
				mask = "{number:-9223372036854775808-9223372036854775807}";

				// MD 4/25/11 - TFS73181
				maskDefaultType = MaskDefaultType.Int64;
			}
			else if ( typeof( ulong ) == type )
			{
				mask = "{number:0-18446744073709551615}";

				// MD 4/25/11 - TFS73181
				maskDefaultType = MaskDefaultType.UInt64;
			}
			else if ( typeof( decimal ) == type )
			{
				mask = "{currency}";

				// MD 4/25/11 - TFS73181
				maskDefaultType = MaskDefaultType.Currency; 
			}
			else if ( typeof( double ) == type || typeof( float ) == type )
			{
				mask = "{double}";

				// MD 4/25/11 - TFS73181
				maskDefaultType = (typeof(double) == type) ? MaskDefaultType.Double : MaskDefaultType.Float;
			}
			else if ( typeof( string ) == type )
			{
				mask = new String( '&', 64 );
			}

			return mask;
		}

		#endregion // GetDefaultMaskForTypeHelper

		#region SupportsDataType

		/// <summary>
		/// Returns true if the data type is supported by the XamMaskedEditor, false otherwise.
		/// </summary>
		/// <param name="dataType"><see cref="Type"/></param>
		/// <returns><b>True</b> if type is supported, <b>False</b> otherwise.</returns>
		public static bool SupportsDataType( System.Type dataType )
		{
            // AS 10/17/08 TFS8886
            bool usesConverter;
            return SupportsDataType(dataType, out usesConverter);
        }

        // AS 10/17/08 TFS8886
        // Added an overload so we can determine if we're using the type converter.
        //
        internal static bool SupportsDataType(System.Type dataType, out bool usesConverter)
        {
            // AS 10/17/08 TFS8886
            usesConverter = false;

			System.Type type = dataType;

			if ( null == type )
				return false;

            // Check for Nullable types
			type = Utilities.GetUnderlyingType( type );

			if ( typeof( DateTime ) == type )
			{
			}
			else if ( 
				typeof( short )    == type ||
				typeof( int )      == type ||
				typeof( long )     == type ||
				typeof( ushort )   == type ||
				typeof( uint )     == type ||
				typeof( ulong )    == type ||				
				typeof( Int16 )    == type || 
				typeof( Int32 )    == type || 
				typeof( Int64 )    == type ||
				typeof( byte )	   == type ||
				typeof( sbyte )	   == type )
			{
			}
			else if ( typeof( decimal ) == type )
			{
			}
			else if ( typeof( string ) == type )
			{
			}
				// SSP 2/21/02 
				// Added typeof( float ) check
				//
			else if ( typeof( double ) == type || typeof( float ) == type )
			{
			}
			else 
			{
                // AS 10/17/08 TFS8886
                // If the type converter associated with the type supports converting
                // to and from strings then we will assume that we can support it. Note
                // the developer will have to ensure that the mask conforms with how 
                // the type expects to be parsed.
                //
                TypeConverter tc = TypeDescriptor.GetConverter(dataType);

                if (tc.CanConvertFrom(typeof(string)) && tc.CanConvertTo(typeof(string)))
                {
                    // AS 10/17/08 TFS8886
                    usesConverter = true;
                    return true;
                }

				return false;
			}
			
			return true;
		}

		#endregion //SupportsDataType

		#region CalcDefaultTimeMask

		/// <summary>
		/// Calculates the default mask for time based on the specified format provider.
		/// </summary>
		/// <param name="formatProvider">The format provider to use to get necessary format 
		/// information to derive the mask from.
		/// Can be a CultureInfo or a DateTimeFormatInfo instance.
		/// </param>
		/// <returns>Mask as a string.</returns>
		/// <remarks>
		/// <para class="body">
		/// Typically there is no need for you to directly call this method. If the ValueType
		/// is set to DateTime the XamMaskedEditor and derived editors will call this method 
		/// to calculate the mask if none has been explicitly set. Also if you want to explicitly
		/// set the mask to make use of culture sensitive date and time mask then use the following
		/// mask tokens when setting the <see cref="XamMaskedEditor.Mask"/> property:
		/// <ul>
		/// <li><b>{date}</b> - Date mask based on <i>short</i> date pattern setting of the system.</li>
		/// <li><b>{time}</b> - Time mask based on <i>short</i> time pattern setting of the system. Short time pattern typically does not include seconds portion.</li>
		/// <li><b>{longtime} - Time mask based on <i>long</i> time pattern setting of the system. Long time pattern typically includes seconds portion.</b></li>
		/// </ul>
		/// </para>
		/// <seealso cref="XamMaskedEditor.Mask"/>
		/// </remarks>
		public static string CalcDefaultTimeMask( IFormatProvider formatProvider )
		{
			// SSP 12/18/02 UWE342
			//
			
#region Infragistics Source Cleanup (Region)









#endregion // Infragistics Source Cleanup (Region)

			DateTimeFormatInfo dateTimeFormatInfo = XamMaskedEditor.GetDateTimeFormatInfo( formatProvider );

			bool hasSeconds = false;
			bool hasAMPM    = false;

			string shortDatePattern = dateTimeFormatInfo.ShortTimePattern;

			// AS 11/12/03 optimization
			// Get the character now so we can see if we can
			// just use the last mask we calculated.
			//
			char timeSeparatorChar = XamMaskedEditor.GetCultureChar( ':', formatProvider );

			// AS 11/12/03 optimization
			// If everything is the same, use the same mask
			// as the last time.
			//
			if (timeSeparatorChar == g_lastTimeSeparator &&
				shortDatePattern == g_lastShortTimePattern)
				return g_lastDefaultTimeMask;

			if ( null != shortDatePattern && shortDatePattern.Length > 0 )
			{
				hasSeconds = shortDatePattern.IndexOfAny( new char[] { 's', 'S' } ) >= 0;
				hasAMPM = shortDatePattern.IndexOfAny( new char[] { 't', 'T' } ) >= 0;
			}

			string mask = hasSeconds ? "hh:mm:ss" : "hh:mm";
			if ( hasAMPM )
				mask = mask + " tt";

			// AS 11/12/03 optimization
			// I changed this to get the character above since
			// we need that to know if the mask will be the same.
			//
			//return mask.Replace( ':', XamMaskedEditor.GetCultureChar( ':', formatProvider ) );
			mask = mask.Replace( ':', timeSeparatorChar );

			// AS 11/12/03 optimization
			// Store the info so we don't have to build
			// the mask next time if its the same
			// short datetime pattern and date separator.
			//
			g_lastDefaultTimeMask = mask;
			g_lastShortTimePattern = shortDatePattern;
			g_lastTimeSeparator = timeSeparatorChar;

			return mask;

		}

		/// <summary>
		/// Calculates the default mask for long time based on the specified format provider.
		/// </summary>
		/// <param name="formatProvider">The format provider to use to get necessary format 
		/// information to derive the mask from.
		/// Can be a CultureInfo or a DateTimeFormatInfo instance.
		/// </param>
		/// <returns>Mask as a string.</returns>
		/// <remarks>
		/// <para class="body">
		/// Typically there is no need for you to directly call this method. If the ValueType
		/// is set to DateTime the XamMaskedEditor and derived editors will call this method 
		/// to calculate the mask if none has been explicitly set. Also if you want to explicitly
		/// set the mask to make use of culture sensitive date and time mask then use the following
		/// mask tokens when setting the <see cref="XamMaskedEditor.Mask"/> property:
		/// <ul>
		/// <li><b>{date}</b> - Date mask based on <i>short</i> date pattern setting of the system.</li>
		/// <li><b>{time}</b> - Time mask based on <i>short</i> time pattern setting of the system. Short time pattern typically does not include seconds portion.</li>
		/// <li><b>{longtime} - Time mask based on <i>long</i> time pattern setting of the system. Long time pattern typically includes seconds portion.</b></li>
		/// </ul>
		/// </para>
		/// <seealso cref="XamMaskedEditor.Mask"/>
		/// </remarks>
		public static string CalcDefaultLongTimeMask( IFormatProvider formatProvider )
		{
			// SSP 12/18/02 UWE342
			//
			
#region Infragistics Source Cleanup (Region)









#endregion // Infragistics Source Cleanup (Region)

			DateTimeFormatInfo dateTimeFormatInfo = XamMaskedEditor.GetDateTimeFormatInfo( formatProvider );

			bool hasSeconds = false;
			bool hasAMPM    = false;

			string longTimePattern = dateTimeFormatInfo.LongTimePattern;

			// AS 11/12/03 optimization
			// Get the character now so we can see if we can
			// just use the last mask we calculated.
			//
			char timeSeparatorChar = XamMaskedEditor.GetCultureChar( ':', formatProvider );

			if ( null != longTimePattern && longTimePattern.Length > 0 )
			{
				hasSeconds = longTimePattern.IndexOfAny( new char[] { 's', 'S' } ) >= 0;
				hasAMPM = longTimePattern.IndexOfAny( new char[] { 't', 'T' } ) >= 0;
			}

			string mask = hasSeconds ? "hh:mm:ss" : "hh:mm";
			if ( hasAMPM )
				mask = mask + " tt";

			// AS 11/12/03 optimization
			// I changed this to get the character above since
			// we need that to know if the mask will be the same.
			//
			//return mask.Replace( ':', XamMaskedEditor.GetCultureChar( ':', formatProvider ) );
			mask = mask.Replace( ':', timeSeparatorChar );

			return mask;
		}
		
		#endregion //CalcDefaultTimeMask

		#region CalcDefaultDateMask

		/// <summary>
		/// Calculates the default mask for date based on the specified format provider.
		/// </summary>
		/// <param name="formatProvider">The format provider to use to get necessary format 
		/// information to derive the mask from.
		/// Can be a CultureInfo or a DateTimeFormatInfo instance.
		/// </param>
		/// <returns>Mask as a string.</returns>
		/// <remarks>
		/// <para class="body">
		/// Typically there is no need for you to directly call this method. If the ValueType
		/// is set to DateTime the XamMaskedEditor and derived editors will call this method 
		/// to calculate the mask if none has been explicitly set. Also if you want to explicitly
		/// set the mask to make use of culture sensitive date and time mask then use the following
		/// mask tokens when setting the <see cref="XamMaskedEditor.Mask"/> property:
		/// <ul>
		/// <li><b>{date}</b> - Date mask based on <i>short</i> date pattern setting of the system.</li>
		/// <li><b>{time}</b> - Time mask based on <i>short</i> time pattern setting of the system. Short time pattern typically does not include seconds portion.</li>
		/// <li><b>{longtime} - Time mask based on <i>long</i> time pattern setting of the system. Long time pattern typically includes seconds portion.</b></li>
		/// </ul>
		/// </para>
		/// <seealso cref="XamMaskedEditor.Mask"/>
		/// </remarks>
		public static string CalcDefaultDateMask( IFormatProvider formatProvider )
		{
			return CalcDefaultDateMask( formatProvider, true );
		}

		// SSP 2/6/09 TFS13259
		// Added an overload that takes in usePostfixSeparatorsFromLongDatePattern parameter.
		// 
		/// <summary>
		/// Calculates the default mask for date based on the specified format provider.
		/// </summary>
		/// <param name="formatProvider">The format provider to use to get necessary format 
		/// information to derive the mask from.
		/// Can be a CultureInfo or a DateTimeFormatInfo instance.
		/// </param>
		/// <param name="usePostfixSeparatorsFromLongDatePattern">
		/// This parameter indicates whether to use culture specific date separators that
		/// are composed of postfix symbols from the long date pattern of the culture's 
		/// date-time format information. 
		/// </param>
		/// <returns>Mask as a string.</returns>
		/// <remarks>
		/// <para class="body">
		/// Typically there is no need for you to directly call this method. If the ValueType
		/// is set to DateTime the XamMaskedEditor and derived editors will call this method 
		/// to calculate the mask if none has been explicitly set. Also if you want to explicitly
		/// set the mask to make use of culture sensitive date and time mask then use the following
		/// mask tokens when setting the <see cref="XamMaskedEditor.Mask"/> property:
		/// <ul>
		/// <li><b>{date}</b> - Date mask based on <i>short</i> date pattern setting of the system.</li>
		/// <li><b>{time}</b> - Time mask based on <i>short</i> time pattern setting of the system. Short time pattern typically does not include seconds portion.</li>
		/// <li><b>{longtime} - Time mask based on <i>long</i> time pattern setting of the system. Long time pattern typically includes seconds portion.</b></li>
		/// </ul>
		/// </para>
		/// <seealso cref="XamMaskedEditor.Mask"/>
		/// </remarks>
		public static string CalcDefaultDateMask( IFormatProvider formatProvider, bool usePostfixSeparatorsFromLongDatePattern )
		{
			// SSP 12/18/02 UWE342
			// Use the new helper method to get the date time format info object.
			//
			
#region Infragistics Source Cleanup (Region)











#endregion // Infragistics Source Cleanup (Region)

			DateTimeFormatInfo dateTimeFormatInfo = XamMaskedEditor.GetDateTimeFormatInfo( formatProvider );

			string shortDatePattern =  dateTimeFormatInfo.ShortDatePattern;

			string mask = null;

			// AS 11/12/03 optimization
			// Get the character now so we can see if we can
			// just use the last mask we calculated.
			//
			char dateSeparatorChar = XamMaskedEditor.GetCultureChar( '/', formatProvider );

			// AS 11/12/03 optimization
			// If everything is the same, use the same mask
			// as the last time.
			//
			if (dateSeparatorChar == g_lastDateSeparator &&
				shortDatePattern == g_lastShortDatePattern
				// SSP 2/6/09 TFS13259
				// We also need to check if the last format provider was the same
				// as well as the new usePostfixSeparatorsFromLongDatePattern parameter.
				// 
				&& formatProvider == g_lastDateFormatProvider
				&& usePostfixSeparatorsFromLongDatePattern == g_lastUsePostfixSeparatorsFromLongDatePattern
				)
				return g_lastDefaultDateMask;

			if ( null != shortDatePattern && shortDatePattern.Length > 0 )
			{
				// We are going to use the culture info to only determine the 
				// order of day, month and year in the date pattern. We are going
				// to that by just looking at which order 'm', 'd' and 'y's occur
				// in the pattern string.
				//
				int mIndex = shortDatePattern.IndexOfAny( new char[] { 'm', 'M' } );
				int dIndex = shortDatePattern.IndexOfAny( new char[] { 'd', 'D' } );
				int yIndex = shortDatePattern.IndexOfAny( new char[] { 'y', 'Y' } );

				int yCount = shortDatePattern.LastIndexOfAny( new char[] { 'y', 'Y' } ) - yIndex;

				string yearStr = yCount > 2 ? "yyyy" : "yy";

				// JAS 12/15/04 Japanese DateTime Separators Implementation
				// SSP 2/6/09 TFS13259
				// Added usePostfixSeparatorsFromLongDatePattern parameter. Don't use nonstandard
				// foreign date mask if usePostfixSeparatorsFromLongDatePattern is false.
				// Enclosed the existing code in the if block.
				// 
				if ( usePostfixSeparatorsFromLongDatePattern )
				{
					Hashtable notUsed = null;
					mask = XamMaskedEditor.GetNonstandardForeignDateMaskAndPostfixSymbols( dateTimeFormatInfo, ref notUsed );
				}

				bool doNormalProcessing = (mask == null || mask.Length == 0);

				if( doNormalProcessing )
				{				
					if ( mIndex <= dIndex && mIndex <= yIndex )
					{
						if ( dIndex < yIndex )
							mask = "mm/dd/" + yearStr;
						else 
							mask = "mm/" + yearStr + "/dd";
					}
					else if ( dIndex <= mIndex && dIndex <= yIndex )
					{
						if ( mIndex < yIndex )
							mask = "dd/mm/" + yearStr;
						else 
							mask = "dd/" + yearStr + "/mm";
					}
						// yIndex is minimum now
						//
					else if ( yIndex <= mIndex && yIndex <= dIndex )
					{
						if ( mIndex < dIndex )
							mask = yearStr + "/mm/dd";
						else
							// SSP 5/22/06 BR12669
							// This was a typo.
							// 
							//mask = yearStr + "/dd/yy";
							mask = yearStr + "/dd/mm";
					}
				}
			}

			if ( null == mask )
				mask = "mm/dd/yyyy";

			// AS 11/12/03 optimization
			// I changed this to get the character above since
			// we need that to know if the mask will be the same.
			//
			//return mask.Replace( '/', XamMaskedEditor.GetCultureChar( '/', formatProvider ) );
			mask = mask.Replace( '/', dateSeparatorChar );

			// AS 11/12/03 optimization
			// Store the info so we don't have to build
			// the mask next time if its the same
			// short datetime pattern and date separator.
			//
			g_lastDefaultDateMask = mask;
			g_lastShortDatePattern = shortDatePattern;
			g_lastDateSeparator = dateSeparatorChar;

			// SSP 2/6/09 TFS13259
			// We also need to check if the last format provider was the same
			// as well as the new usePostfixSeparatorsFromLongDatePattern parameter.
			// 
			g_lastDateFormatProvider = formatProvider;
			g_lastUsePostfixSeparatorsFromLongDatePattern = usePostfixSeparatorsFromLongDatePattern;

			return mask;
		}

		#endregion //CalcDefaultDateMask

		#region CalcDefaultCurrencyMask

		/// <summary>
		/// Calculates the default mask for currency based on the cultureInfo.
		/// </summary>
		/// <param name="formatProvider">Format provider to use to construct a default mask with. Can be a CultureInfo or a NumberFormatInfo instance.</param>
		/// <param name="integerDigits">Number of digits in integer section. -1 means use a default. Can be 0 in which case there won't be an integer portion.</param>
		/// <param name="fractionDigits">Number of digits in fraction section. -1 means use one specified by culture info. Can be 0 in which case there won't be fraction section.</param>
		/// <param name="allowNegatives">If '-' or '+' then negative numbers are allowed. '-' specifies that the minus sign should be displayed only when the number is negative. '+' specifies that the plus or minus sign will always be displayed depending on whther the number is negative or positive. If this parameter is any other character then it's ignored.</param>
		/// <param name="includeCurrencySymbol">Specifies whether the mask should include the currency symbol.</param>
		/// <returns>Mask as a string.</returns>
		/// <remarks>
		/// <para class="body">
		/// Typically there is no need for you to directly call this method. If the ValueType
		/// is set to Decimal the XamMaskedEditor and derived editors will call this method 
		/// to calculate the mask if none has been explicitly set. Also if you want to explicitly
		/// set the mask to make use of culture sensitive currency mask then use one of the
		/// currency tokens as documented in the table listing all the mask tokens in the
		/// help for <see cref="XamMaskedEditor.Mask"/> property.
		/// </para>
		/// <seealso cref="XamMaskedEditor.Mask"/>
		/// </remarks>
		public static string CalcDefaultCurrencyMask( IFormatProvider formatProvider,
			int integerDigits, int fractionDigits, char allowNegatives, bool includeCurrencySymbol )
		{
			// SSP 12/18/02 UWE342
			// Use the new helper method to get the date time format info object.
			//
			
#region Infragistics Source Cleanup (Region)









#endregion // Infragistics Source Cleanup (Region)

			NumberFormatInfo numberFormatInfo = XamMaskedEditor.GetNumberFormatInfo( formatProvider );

			// SSP 4/8/05 BR03077
			// Added a static overload of CalcDefaultCurrencyMask. Also added 
			// integerDigits, fractionDigits and allowNegativeCharacter parametrs.
			//
			//int numberOfDigitsToDisplay = 9;

			StringBuilder maskSB = new StringBuilder( 15 );

			//	BF 10.9.03	UWE728
			//	We need to look at the culture's CurrencyPositivePattern to
			//	determine whether the currency symbol is the first thing or
			//	the last thing in the mask. This property also tells us
			//	whether there is a space between the number and the symbol.
			
#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

			int currencyPositivePattern = numberFormatInfo.CurrencyPositivePattern;
			bool includeSpaceWithCurrencySymbol = (currencyPositivePattern > 1) ? true : false;
			bool currencySymbolAtBeginning = (currencyPositivePattern % 2 == 0) ? true : false;

			// SSP 8/27/06 - NAS 6.3
			// Added an overload that takes in includeCurrencySymbol parameter.
			// 
			//if ( currencySymbolAtBeginning )
			if ( currencySymbolAtBeginning && includeCurrencySymbol )
			{
                // MBS 9/26/06 BR15826
                //maskSB.Append( numberFormatInfo.CurrencySymbol );
                string literalCurrencySymbol;
                MaskParser.EscapeLiteralsInString(numberFormatInfo.CurrencySymbol, out literalCurrencySymbol);
                maskSB.Append(literalCurrencySymbol);

				if ( includeSpaceWithCurrencySymbol )
					maskSB.Append( ' ' );
			}


			int currencyGroupSizesIndex = 0;
			int lastCommaPosition = 0;

			// Get the group sizes array from the number format which has the
			// values indicating the difference in digits between successive
			// commas.
			//
			int[] commPosDeltaArr = numberFormatInfo.CurrencyGroupSizes;

			// Create a temp string builder for the digits
			//
			StringBuilder sb = new StringBuilder( 15 );
			// SSP 4/8/05 BR03077
			// Replaced numberOfDigitsToDisplay with integerDigits.
			//
			if ( integerDigits < 0 )
				integerDigits = 9;
			//for ( int i = 0; i < numberOfDigitsToDisplay; i++ )
			for ( int i = 0; i < integerDigits; i++ )
			{
				// Find out if we need to put a comma at ith position in the whole
				// number part of the currency (left to right)
				int commaPositionDelta = 0;
				if ( currencyGroupSizesIndex < commPosDeltaArr.Length )
					commaPositionDelta = commPosDeltaArr[ currencyGroupSizesIndex ];
				else 
					commaPositionDelta = commPosDeltaArr[ commPosDeltaArr.Length - 1 ];

				// Insert a comma if it's time to do so.
				//
				if ( i == lastCommaPosition + commaPositionDelta )
				{
					lastCommaPosition = i;
					sb.Append( ',' );
						
					// Once a comma is insrted in the mask, get the next comma delta
					// from the group sizes array
					//
					currencyGroupSizesIndex++;
				}			
	
				
				sb.Append( 'n' );
			}

			// SSP 4/8/05 BR03077
			// Added a static overload of CalcDefaultCurrencyMask. Also added 
			// integerDigits, fractionDigits and allowNegativeCharacter parametrs.
			//
			if ( '-' == allowNegatives || '+' == allowNegatives )
				maskSB.Append( allowNegatives );

			// Now we have to reverse the sb (digits) because the commas were added
			// from right to left above
			//
			for ( int j = sb.Length - 1; j >= 0; j-- )
				maskSB.Append( sb[j] );
			
			// Now append the digit seperator.
			// NOTE: the reason why we use '.' and ',' and not the settings off the
			// CultureInfo because the mask parser expects '.' and ',' and it
			// will map these characters to appropriate in the cutlure so we don't
			// need to do it here.
			//
			// SSP 4/8/05 BR03077
			// Added a static overload of CalcDefaultCurrencyMask. Also added 
			// integerDigits, fractionDigits and allowNegativeCharacter parametrs.
			//
			// ------------------------------------------------------------------------
			if ( fractionDigits < 0 )
				fractionDigits = numberFormatInfo.CurrencyDecimalDigits;

			if ( fractionDigits > 0 )
			{
				maskSB.Append( '.' );

				// Now create the 
				maskSB.Append( new String( 'n', fractionDigits ) );
			}
			
#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

			// ------------------------------------------------------------------------

			//	BF 10.9.03	UWE728 (see above)
			// SSP 8/27/06 - NAS 6.3
			// Added an overload that takes in includeCurrencySymbol parameter.
			// 
			//if ( ! currencySymbolAtBeginning )
			if ( ! currencySymbolAtBeginning && includeCurrencySymbol )
			{
				if ( includeSpaceWithCurrencySymbol )
					maskSB.Append( ' ' );

                // MBS 9/26/06 BR15826
                //maskSB.Append( numberFormatInfo.CurrencySymbol );
                string literalCurrencySymbol;
                MaskParser.EscapeLiteralsInString(numberFormatInfo.CurrencySymbol, out literalCurrencySymbol);
                maskSB.Append(literalCurrencySymbol);
			}

			return maskSB.ToString( );
		}

		#endregion //CalcDefaultCurrencyMask

		#region CalcDefaultDoubleMask

		/// <summary>
		/// Calculates the default mask for double mask based on the CultureInfo
		/// </summary>
		/// <param name="formatProvider">Format provider to use to construct a default mask with. Can be a CultureInfo or a NumberFormatInfo instance.</param>
		/// <param name="integerDigits">Number of digits in integer section. -1 means use a default. Can be 0 in which case there won't be an integer portion.</param>
		/// <param name="fractionDigits">Number of digits in fraction section. -1 means use one specified by culture info. Can be 0 in which case there won't be fraction section.</param>
		/// <param name="allowNegatives">If '-' or '+' then negative numbers are allowed. '-' specifies that the minus sign should be displayed only when the number is negative. '+' specifies that the plus or minus sign will always be displayed depending on whther the number is negative or positive. If this parameter is any other character then it's ignored.</param>
		/// <returns>Mask as a string.</returns>
		/// <remarks>
		/// <para class="body">
		/// Typically there is no need for you to directly call this method. If the ValueType
		/// is set to Double or Float, the XamMaskedEditor and derived editors will call this method 
		/// to calculate the mask if none has been explicitly set. Also if you want to explicitly
		/// set the mask to make use of culture sensitive double/float mask then use one of the
		/// double tokens as documented in the table listing all the mask tokens in the
		/// help for <see cref="XamMaskedEditor.Mask"/> property.
		/// </para>
		/// <seealso cref="XamMaskedEditor.Mask"/>
		/// </remarks>
		public static string CalcDefaultDoubleMask( IFormatProvider formatProvider,
			int integerDigits, int fractionDigits, char allowNegatives )
		{
			// SSP 12/18/02 UWE342
			// Use the new helper method to get the nbumber format info object.
			//
			
#region Infragistics Source Cleanup (Region)









#endregion // Infragistics Source Cleanup (Region)

			NumberFormatInfo numberFormatInfo = XamMaskedEditor.GetNumberFormatInfo( formatProvider );

			// SSP 4/8/05 BR03077
			// Replaced numberOfDigitsToDisplay with integerDigits.
			//
			//int numberOfDigitsToDisplay = 9;

			StringBuilder maskSB = new StringBuilder( 15 );

			int numberGroupSizesIndex = 0;
			int lastCommaPosition = 0;

			// Get the group sizes array from the number format which has the
			// values indicating the difference in digits between successive
			// commas.
			//
			int[] commPosDeltaArr = numberFormatInfo.NumberGroupSizes;

			// Create a temp string builder for the digits
			//
			StringBuilder sb = new StringBuilder( 15 );
			// SSP 4/8/05 BR03077
			// Replaced numberOfDigitsToDisplay with integerDigits.
			//
			if ( integerDigits < 0 )
				integerDigits = 9;
			//for ( int i = 0; i < numberOfDigitsToDisplay; i++ )
			for ( int i = 0; i < integerDigits; i++ )
			{
				// Find out if we need to put a comma at ith position in the whole
				// number part of the currency (left to right)
				int commaPositionDelta = 0;
				if ( numberGroupSizesIndex < commPosDeltaArr.Length )
					commaPositionDelta = commPosDeltaArr[ numberGroupSizesIndex ];
				else 
					commaPositionDelta = commPosDeltaArr[ commPosDeltaArr.Length - 1 ];

				// Insert a comma if it's time to do so.
				//
				if ( i == lastCommaPosition + commaPositionDelta )
				{
					lastCommaPosition = i;
					sb.Append( ',' );
						
					// Once a comma is insrted in the mask, get the next comma delta
					// from the group sizes array
					//
					numberGroupSizesIndex++;
				}
				
				sb.Append( 'n' );
			}

			// SSP 4/8/05 BR03077
			// Added a static overload of CalcDefaultCurrencyMask. Also added 
			// integerDigits, fractionDigits and allowNegativeCharacter parametrs.
			//
			if ( '-' == allowNegatives || '+' == allowNegatives )
				maskSB.Append( allowNegatives );

			// Now we have to reverse the sb (digits) because the commas were added
			// from right to left above
			//
			for ( int j = sb.Length - 1; j >= 0; j-- )
				maskSB.Append( sb[j] );
			
			// Now append the digital seperator.
			// NOTE: the reason why we use '.' and ',' and not the settings off the
			// CultureInfo because the mask parser expects '.' and ',' and it
			// will map these characters to appropriate in the cutlure so we don't
			// need to do it here.
			//
			// SSP 4/8/05 BR03077
			// Added a static overload of CalcDefaultCurrencyMask. Also added 
			// integerDigits, fractionDigits and allowNegativeCharacter parametrs.
			//
			// ------------------------------------------------------------------------
			if ( fractionDigits < 0 )
				fractionDigits = numberFormatInfo.NumberDecimalDigits;
			if ( fractionDigits > 0 )
			{
				maskSB.Append( '.' );

				// Now create the 
				maskSB.Append( new String( 'n', fractionDigits ) );
			}
			
#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

			// ------------------------------------------------------------------------

			return maskSB.ToString( );
		}

		#endregion //CalcDefaultDoubleMask

		#region IsMaskValidForDataType

		/// <summary>
		/// Returns true if mask is valid for the type.
		/// </summary>
		/// <param name="dataType"></param>
		/// <param name="mask"></param>
		/// <param name="formatProvider"></param>
		/// <returns></returns>
		internal static bool IsMaskValidForDataType( System.Type dataType, string mask, IFormatProvider formatProvider )
		{
			if ( null == mask )
				return false;

			MaskInfo maskInfo = new MaskInfo( );

			// SSP 7/9/08 BR34636
			// 
			//maskInfo.FormatProvider = formatProvider;
			maskInfo.InitializeFormatProvider( formatProvider, null );

			SectionsCollection sections;
			MaskParser.Parse( mask, out sections, maskInfo.FormatProvider );
			sections.Initialize( maskInfo );

			// SSP 7/9/08 BR34636
			// Moved the code into another overload.
			// 
			return IsMaskValidForDataType( dataType, sections );
		}

		/// <summary>
		/// Returns true if mask is valid for the type.
		/// </summary>
		/// <param name="dataType"></param>
		/// <param name="parsedSections"></param>
		/// <returns></returns>
		internal static bool IsMaskValidForDataType( System.Type dataType, SectionsCollection parsedSections )
		{
			SectionsCollection sections = parsedSections;

			System.Type type = Utilities.GetUnderlyingType( dataType );

			if ( typeof( DateTime ) == type )
			{
				int monthSections = 0;
				int daySections = 0;
				int yearSections = 0;
				int minuteSections = 0;
				int hourSections = 0;
				int secondSections = 0;
				
				// SSP 12/19/02
				//
				int amPmSection = 0;

				for ( int i = 0; i < sections.Count; i++ )
				{
					SectionBase section = sections[i];

					if ( section is DaySection )
					{
						daySections++;
						continue;
					}

					if ( section is YearSection )
					{
						yearSections++;
						continue;
					}

					if ( section is MonthSection )
					{
						monthSections++;
						continue;
					}

					if ( section is MinuteSection )
					{
						minuteSections++;
						continue;
					}

					if ( section is HourSection )
					{
						hourSections++;
						continue;
					}

					if ( section is SecondSection )
					{
						secondSections++;
						continue;
					}

					// SSP 12/19/02
					//
					if ( section is AMPMSection )
					{
						amPmSection++;
						continue;
					}

					if ( section is LiteralSection )
						continue;

					// If we get here, then this section is none
					// of the above sections and thus not a valid date
					//
					return false;
				}
					
				bool b = 						
					monthSections  <= 1 &&		// No repeating sections and
					daySections    <= 1 &&		// at least one of them
					yearSections   <= 1 &&
					hourSections   <= 1 &&
					minuteSections <= 1 &&
					secondSections <= 1 &&
					// SSP 12/19/02
					//
					amPmSection    <= 1;

				b = b &&
					( 1 == monthSections ||
					1 == daySections   || 
					1 == yearSections  ||
					1 == hourSections  );						 

				return b;
			}
			else if ( typeof( byte ) == type || typeof( sbyte ) == type 
				|| typeof( short ) == type || typeof( ushort ) == type || typeof( Int16 )  == type 
				|| typeof( int ) == type || typeof( uint ) == type || typeof( Int32 ) == type || 
				typeof( long ) == type || typeof( ulong ) == type || typeof( Int64 )  == type )
			{
				int numericSections = 0;
					
				for ( int i = 0; i < sections.Count; i++ )
				{
					if ( XamMaskedEditor.IsSectionNumeric( sections[i] ) )
					{
						numericSections++;
					}

					if ( numericSections >= 2 )
						break;
				}

				// One and only one numeric section is required for integer
				//
				return 1 == numericSections;
			}
			else if ( typeof( double ) == type || typeof( float ) == type 
				|| typeof( decimal ) == type )
			{
				SectionBase integerPart = null;
				SectionBase fractionPart = null;
				bool pastDecimalSeperator = false;
				for ( int i = 0; i <  sections.Count; i++ )
				{
					if ( XamMaskedEditor.IsSectionNumeric( sections[i] ) )
					{
						if ( !pastDecimalSeperator )
						{
							integerPart = sections[i];
						}
						else if ( null == fractionPart )
						{
							fractionPart = sections[i];								
						}
						else
						{
							return false;
						}
					}		
						// skip the commas
					else if ( sections[i] is LiteralSection &&
						1 == sections[i].DisplayChars.Count &&
						sections[i].CommaChar == sections[i].DisplayChars[0].Char )
					{
						if ( !pastDecimalSeperator )
						{
							integerPart = null;
							continue;
						}
						else // can't have commas in fraction portion of double
						{
							return false;
						}
					}
					else if ( sections[i] is LiteralSection && 
						1 == sections[i].DisplayChars.Count &&
						sections[i].DecimalSeperatorChar == sections[i].DisplayChars[0].Char )
						pastDecimalSeperator = true;							
					else if ( ( null != integerPart || null != fractionPart || pastDecimalSeperator )
						// SSP 2/09/04 UWE792
						// Allow for literal sections to follow a number mask like in "nnn.nn %".
						//
						&& ! ( sections[i] is LiteralSection ) )
						return false;
				}

				return null != integerPart || null != fractionPart;
			}
			else if ( typeof( string ) == type )
			{
				return true;
			}

			return false;
		}

		#endregion //IsMaskValidForDataType

		#region GetText

		/// <summary>
		/// Returns the current text of the editor based on the specified mask mode.
		/// </summary>
		/// <param name="maskMode">The mode that determines how literals and prompt characters are accounted for.</param>
		/// <returns>The current text of the editor with the specified mask mode applied to it.</returns>
		/// <remarks>
		/// <para class="body">
		/// <see cref="ValueEditor.Text"/> property can also be used to retrieve the current text of the editor.
		/// The Text property will return a value with the mask mode specified by the <see cref="DataMode"/>
		/// property applied to the returned value. This method allows you to use any mode without having to
		/// set the DataMode property.
		/// </para>
		/// <para class="body">
		/// Any of <see cref="ValueEditor.Value"/>, <see cref="ValueEditor.Text"/> and <see cref="TextEditorBase.DisplayText"/> 
		/// properties can also be used to retrieve the current value of the editor.
		/// </para>
		/// <seealso cref="ValueEditor.Value"/>
		/// <seealso cref="ValueEditor.Text"/>
		/// <seealso cref="TextEditorBase.DisplayText"/>
		/// <seealso cref="XamMaskedEditor.DataMode"/>
		/// <seealso cref="XamMaskedEditor.DisplayMode"/>
		/// <seealso cref="XamMaskedEditor.Mask"/>
		/// </remarks>
		public string GetText( MaskMode maskMode )
		{
			
			
			
			

			EditInfo editInfo = this.EditInfo;
			if ( null != editInfo )
				return editInfo.GetText( maskMode );

			MaskInfo maskInfo = this.MaskInfo;
			return XamMaskedEditor.GetText( maskInfo.Sections, maskMode, maskInfo );
			
		}

		#endregion //GetText

		#region MaskInfo

		internal MaskInfo MaskInfo
		{
			get
			{
				if ( null == _maskInfo )
				{
					_maskInfo = new MaskInfo( this );
					// SSP 8/7/07 
					// Made sections read-only.
					//
					//this.Sections = _maskInfo.Sections;
					this.InternalSetSections( _maskInfo.Sections );
				}

				return _maskInfo;
			}
		}

		#endregion // MaskInfo

		#region MaskInfoIfAllocated

		internal MaskInfo MaskInfoIfAllocated
		{
			get
			{
				return _maskInfo;
			}
		}

		#endregion // MaskInfoIfAllocated

		#region EditInfo

		internal EditInfo EditInfo
		{
			get
			{
				return this._editInfo;
			}
		}

		#endregion // EditInfo

		#region IsTextBoxEntryEnabled

		// SSP 3/23/09 IME
		// Added IsTextBoxEntryEnabled property.
		// 

		/// <summary>
		/// Identifies the property key for read-only <see cref="IsTextBoxEntryEnabled"/> dependency property.
		/// </summary>
		private static readonly DependencyPropertyKey IsTextBoxEntryEnabledPropertyKey = DependencyProperty.RegisterReadOnly(
			"IsTextBoxEntryEnabled",
			typeof( bool ),
			typeof( XamMaskedEditor ),
			new FrameworkPropertyMetadata( KnownBoxes.FalseBox, FrameworkPropertyMetadataOptions.None )
		);

		/// <summary>
		/// Identifies the read-only <see cref="IsTextBoxEntryEnabled"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty IsTextBoxEntryEnabledProperty = IsTextBoxEntryEnabledPropertyKey.DependencyProperty;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[Bindable( true )]
		[Browsable( false )]
		[ReadOnly( true )]
		[EditorBrowsable( EditorBrowsableState.Never )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public bool IsTextBoxEntryEnabled
		{
			get
			{
				return (bool)this.GetValue( IsTextBoxEntryEnabledProperty );
			}
		}

		internal void InternalSetIsTextBoxEntryEnabled( bool value )
		{
			if ( this.IsTextBoxEntryEnabled != value )
			{
				// SSP 7/22/11 TFS80613
				// Once ime textbox is enabled don't hide it while in edit mode. If an IME
				// composition is in progress and the ime textbox is hidden, it causes a 
				// framework exception.
				// 
				// ----------------------------------------------------------------------------
				_resetIsTextBoxEntryEnabledWhenEditModeEnds = false;
				if ( !value && this.IsInEditMode )
				{
					_resetIsTextBoxEntryEnabledWhenEditModeEnds = true;
					return;
				}
				// ----------------------------------------------------------------------------

				// Since we are switching visibility of SectionsList and ImeTextBox, we need to
				// make sure we give focus to the new visible element. However we should do so
				// only if the editor had focus to begin with. Also since the element is going
				// to be collapsed, we want to first move the focus to the editor before 
				// collapsing the element, which will prevent the framework from shifting focus
				// somewhere else.
				// 
				bool isKeyboardFocusedWithin = this.IsKeyboardFocusWithin;
				if ( isKeyboardFocusedWithin )
					this.Focus( );

				// Set the property.
				// 
				this.SetValue( IsTextBoxEntryEnabledPropertyKey, value );

				// SSP 9/8/09 TFS21334
				// Use the new ShowHideCaretHelper method which checks for additional conditions,
				// including the IsTextBoxEntryEnabled.
				// 
				// ------------------------------------------------------------------------------
				this.ShowHideCaretHelper( );
				
#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

				// ------------------------------------------------------------------------------

				// If we had and still have keyboard focus, make sure the section list or the
				// textbox receives focus.
				// 
				isKeyboardFocusedWithin = isKeyboardFocusedWithin && this.IsKeyboardFocusWithin;
				if ( this.IsInEditMode && isKeyboardFocusedWithin && !_isInEndEditMode )
				{
					this.Dispatcher.BeginInvoke( System.Windows.Threading.DispatcherPriority.Input,
							new Utils.MethodInvoker( this.DelayedSetFocusToFocusSiteHelper ) );

					// SSP 9/8/09 TFS21334
					// Use the new ShowHideCaretHelper method which checks for additional condition
					// of having selected text to not show the caret to be consistent with the WPF
					// TextBox, which doesn't show the caret when there's some text selected.
					// 
					// ----------------------------------------------------------------------------
					this.ShowHideCaretHelper( );
					
#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

					// ----------------------------------------------------------------------------
				}
			}
		}

		private void DelayedSetFocusToFocusSiteHelper( )
		{
			if ( this.IsInEditMode && !_isInEndEditMode )
			{
				this.Dispatcher.BeginInvoke( System.Windows.Threading.DispatcherPriority.Input,
					new Utils.MethodInvoker( this.SetFocusToFocusSite ) );
			}
		}

		#endregion // IsTextBoxEntryEnabled

		/// <summary>
		/// Occurs when the control is about to enter edit mode.
		/// </summary>
		/// <seealso cref="ValueEditor.EditModeStarting"/>
		protected override void OnEditModeStarting( EditModeStartingEventArgs args )
		{
			base.OnEditModeStarting( args );

			if ( ! args.Cancel )
			{
				this._editInfo = new EditInfo( this.MaskInfo );
				this.MaskInfo.InternalRefreshValue( this.Value );
			}
		}

		/// <summary>
		/// Occurs when the control has just entered edit mode.
		/// </summary>
		/// <seealso cref="ValueEditor.EditModeStarted"/>
		protected override void OnEditModeStarted( EditModeStartedEventArgs args )
		{
			// SSP 10/17/07 UWG2076 BR27228
			// Moved this here from OnMouseLeftButtonDown. Before this move, we used to position the
			// caret after raising EditModeStarted. So if the user set the caret position in the
			// event handler of that event then we ended up overwriting that. Therefore do it before
			// we raise the EditModeStarted which will give the user a chance to set the selection
			// start and length to any desired values.
			// 
			// --------------------------------------------------------------------------------------
			EditInfo editInfo = this.EditInfo;
			if ( null != editInfo && _transitingIntoEditModeInMouseDown )
			{
				object val = this.Value;
				bool isValueEmpty = null == val || DBNull.Value == val || val is string && 0 == ( (string)val ).Length;
				bool isDisplayTextFormatted = ! string.IsNullOrEmpty( this.Format );

				if ( isDisplayTextFormatted || isValueEmpty )
				{
					editInfo.SetCaretPivot( 0 );
				}
				else
				{
					this.VerifyElementsPositioned( );

					editInfo.MaskedEditUIElementClicked( _transitingIntoEditModeInMouseDownPoint );

					this.CaptureMouse( );

					// SSP 10/1/07 - XamRibbon
					// XamRibbon needs to keep the capture on the editor even after the mouse is released.
					// Therefore we need to keep a flag to indicate when we are in drag selection mode
					// and not just rely on the editor having mouse capture.
					// 
					if ( this.IsMouseCaptured )
						_isDragSelecting = true;
				}

				_transitingIntoEditModeInMouseDown = false;
				this.ProcessIsBeingEditedAndFocusedChanged( );
			}
			// --------------------------------------------------------------------------------------

			base.OnEditModeStarted( args );

			if ( null != this._editInfo )
			{
				this._editInfo.SyncCaretAndPivotsWithMaskedEditor( );
				this.ProcessSelectionChanged( );
			}

			if ( null != this.MaskInfo && this.MaskInfo.IsBeingEditedAndFocused )
			{
				// SSP 9/8/09 TFS21334
				// Use the new ShowHideCaretHelper method which checks for additional condition
				// of having selected text to not show the caret to be consistent with the WPF
				// TextBox, which doesn't show the caret when there's some text selected.
				// 
				//this.ShowCaretElement( );
				this.ShowHideCaretHelper( );

				// If the mask is right-to-left then scroll to the right.
				// 
				if ( null != this._editInfo )
					this._editInfo.ScrollCaretIntoView( );
			}
		}

		/// <summary>
		/// Occurs when the control has just exited edit mode
		/// </summary>
		/// <seealso cref="ValueEditor.EditModeEnded"/>
		protected override void OnEditModeEnded( EditModeEndedEventArgs args )
		{
			base.OnEditModeEnded( args );

			// SSP 9/8/09 TFS21334
			// Use the new ShowHideCaretHelper method which checks for additional conditions.
			// 
			//this.HideCaretElement( );
			this.ShowHideCaretHelper( );

            if (this._editInfo != null)
            {
                this._editInfo.InitializeIMETextBox(null);

				// SSP 7/22/11 TFS80613
				// 
				if ( _resetIsTextBoxEntryEnabledWhenEditModeEnds )
					this.InternalSetIsTextBoxEntryEnabled( false );

                this._editInfo = null;
            }
		}

		#region InsertMode

		/// <summary>
		/// Identifies the <see cref="InsertMode"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty InsertModeProperty = DependencyProperty.Register(
			"InsertMode",
			typeof( bool ),
			typeof( XamMaskedEditor ),
			
			
			
			
			new FrameworkPropertyMetadata( KnownBoxes.TrueBox )
			);

		/// <summary>
		/// Returns or sets the editing mode (insert or overstrike).
		/// </summary>
		/// <remarks>
		/// <p class="body">When this property is set to True, characters typed will be inserted at the current caret position and any following characters will be shifted. When set to False, typing at an insertion point that contains an existing character will replace that character. The value of this property also affects how characters are deleted using either The Delete key or the Backspace key. When in insert mode, characters after the character being deleted will be shifted by one to the left within the section.</p>
		/// <seealso cref="AllowShiftingAcrossSections"/>
		/// <seealso cref="SelectAllBehavior"/>
		/// <seealso cref="TabNavigation"/>
		/// <seealso cref="AutoFillDate"/>
		/// </remarks>
		//[Description( "Specifies whether the edit mode is in 'insert' or 'overstrike'." )]
		//[Category( "Behavior" )]
		[Bindable( true )]
		[Browsable( false ), DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public bool InsertMode
		{
			get
			{
				return (bool)this.GetValue( InsertModeProperty );
			}
			set
			{
				this.SetValue( InsertModeProperty, value );
			}
		}

		#endregion // InsertMode

		#region Mask

		/// <summary>
		/// Identifies the <see cref="Mask"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty MaskProperty = DependencyProperty.Register(
			"Mask",
			typeof( string ),
			typeof( XamMaskedEditor ),
			new FrameworkPropertyMetadata( null, FrameworkPropertyMetadataOptions.None,
				new PropertyChangedCallback( OnMaskChanged )
				
				
				
				
				
				
				
				
			) );

		// JJD 4/27/07
		// Optimization - cache the property locally
		private string _cachedMask;

		
		
		
		
		
		
		
		
#region Infragistics Source Cleanup (Region)











#endregion // Infragistics Source Cleanup (Region)


		/// <summary>
		/// Returns or sets the mask associated with the masked edit control.
		/// </summary>
		/// <remarks>
		/// <p class="body">When a mask is defined, placeholders are defined by the 
		/// <see cref="PromptChar"/> property. When inputting data, the user can only 
		/// replace a placeholder with a character that is of the same type as the one 
		/// specified in the mask. If the user enters an invalid character, the control 
		/// rejects the character and generates the <see cref="InvalidChar"/> event. The 
		/// control can distinguish between numeric and alphabetic characters for 
		/// validation, as well as validate for valid content for certain types like, 
		/// date or time or numeric types such as the correct 
		/// month or time of day etc...</p>
		/// <p class="body">For a complete list of the various mask tokens as well as 
		/// examples please refer to <a href="xamInputs_Masks.html">Masks</a> topic.</p>
		/// <p class="note"><b>Note:</b> When specifying the mask from within XAML and using 
		/// one of the special tokens that are enclosed within {}, you must preceed the 
		/// mask with {} - e.g. {}{date}.</p>
		/// </remarks>
		//[Description( "Gets/sets the mask." )]
		//[Category( "Behavior" )]
		[RefreshPropertiesAttribute( RefreshProperties.All )]
		public string Mask
		{
			get
			{
				// JJD 4/27/07
				// Optimization - use the locally cached property 
				//return (string)this.GetValue( MaskProperty );
				return this._cachedMask;
			}
			set
			{
				this.SetValue( MaskProperty, value );
			}
		}

		private static void OnMaskChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			XamMaskedEditor editor = (XamMaskedEditor)dependencyObject;

			
			
			
			
			
			editor._cachedMask = (string)e.NewValue;

			// Don't take any actions if we are in the process of initializing. At the
			// end of the initialization process we'll take the necessary actions.
			// 
			if ( editor.IsInitialized )
				editor.ReparseMask( );
		}

		private void ReparseMask( )
		{
			if ( null != _maskInfo )
				_maskInfo.RecreateSections( false );

			// SSP 9/3/09 TFS18219
			// This issue was discovered while debugging TFS18219. Basically we need
			// to revalidate the value agains the new mask since the mask is part of
			// the value validation.
			
			
			//this.ApplyMaskToText( );
			this.SyncValueProperties( ValueProperty, this.Value );
		}

		// SSP 7/9/08 BR34636
		// 
		/// <summary>
		/// Called when the ValueType property changes.
		/// </summary>
		/// <param name="newType">New value of the property.</param>
		protected override void OnValueTypeChanged( Type newType )
		{
			base.OnValueTypeChanged( newType );

			if ( null != _maskInfo )
			{
				_maskInfo.DataType = this.ValueType;

				// Don't take any actions if we are in the process of initializing. At the
				// end of the initialization process we'll take the necessary actions.
				// 
				if ( this.IsInitialized )
					this.ReparseMask( );
			}
		}

		private void ApplyMaskToText( )
		{
			
			
			
			
			this.SyncTextWithValue( );
			




			

			
			
			
			
			
			this.SyncDisplayText( );
		}

		#endregion // Mask

		#region ApplyMask

		// SSP 10/12/07
		// Added ApplyMask method.
		// 
		/// <summary>
		/// Applies the specified mask to the specified text and returns the result.
		/// </summary>
		/// <param name="text">Text to apply the mask to.</param>
		/// <param name="mask">The mask to apply.</param>
		/// <param name="formatProvider">Provides culture specific symbols in the mask. This may not 
		/// be applicable to all masks - only the masks that have characters (like currency symbol) 
		/// that need to be represented by culture specific version of the symbol.</param>
		/// <param name="promptCharacter">The prompt character - only applicable with certain mask modes.</param>
		/// <param name="padCharacter">The pad character - only applicable with certain mask modes.</param>
		/// <param name="maskMode">Specifies the mask mode.</param>
		/// <returns>The result of applying the specified mask to the specified text.</returns>
		public static string ApplyMask( string text, string mask, 
			IFormatProvider formatProvider, char promptCharacter, char padCharacter, MaskMode maskMode )
		{
			ParsedMask parsedMask = new ParsedMask( mask, formatProvider, promptCharacter, padCharacter );
			return parsedMask.ApplyMask( text, maskMode );
		}

		#endregion // ApplyMask

		#region SyncValuePropertiesOverride

		
		
		

		
#region Infragistics Source Cleanup (Region)

















#endregion // Infragistics Source Cleanup (Region)


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
			// MD 4/25/11 - TFS73181
			// Added a performance improvement here when the Value property was set and a default mask was created based on the data
			// type of the editor.
			// -------------------------------------------------------------------
			bool retVal;
			if (prop == ValueEditor.ValueProperty && 
				this.TryFastSyncOnValueChanged(newValue, out error))
			{
				retVal = true;
			}
			else
			{
			// ----------------------------- TFS73181 --------------------------

			//bool retVal = base.SyncValuePropertiesOverride( prop, newValue, out error );
			retVal = base.SyncValuePropertiesOverride(prop, newValue, out error);

			// Syncrhonize display characters to reflect the new value. However only do
			// so if this OnValueChanged is not a result of edit info's OnTextChanged
			// in response to user changing the value of a display char.
			// 
			if ( null == _editInfo || !_editInfo._inOnTextChanged )
			{
				// SSP 5/19/09 - Clipboard Support
				// Also return false if InternalRefreshValue fails.
				// 
				// --------------------------------------------------------------------------
				//this.MaskInfo.InternalRefreshValue( this.Value )
				Exception tmpError;
				if ( !this.MaskInfo.InternalRefreshValue( this.Value, out tmpError ) )
				{
					if ( null == error )
						error = tmpError;

					retVal = false;
				}
				// --------------------------------------------------------------------------
			}

			// MD 4/25/11 - TFS73181
			}

			// SSP 3/23/09 IME
			// 
			if ( null != _editInfo )
				_editInfo.SyncIMETextBox( );

			return retVal;
		}

		#endregion // SyncValuePropertiesOverride		

		#region DataMode

		/// <summary>
		/// Identifies the <see cref="DataMode"/> dependency property
		/// </summary>
		public static readonly DependencyProperty DataModeProperty = DependencyProperty.Register(
			"DataMode",
			typeof( MaskMode ),
			typeof( XamMaskedEditor ),
			new FrameworkPropertyMetadata( MaskMode.IncludeLiterals, FrameworkPropertyMetadataOptions.None,
				new PropertyChangedCallback( OnDataModeChanged )
				
				
				
				
				
				
				
				
			) );

		
		
		
		
		
		
		//private MaskMode _cachedDataMode = MaskMode.IncludeLiterals;
		private MaskMode _cachedDataMode;

		
		
		
		
		
		
		
		
#region Infragistics Source Cleanup (Region)











#endregion // Infragistics Source Cleanup (Region)


		/// <summary>
		/// Returns or sets a value that determines how the control's contents will be stored by 
		/// the data source when data masking is enabled.
		/// </summary>
		/// <remarks>
		/// <p class="body">This property is used to determine how mask literals and prompt characters are handled when the control's contents are passed to the data source (or are retrieved using the <see cref="ValueEditor.Text"/> property.) Based on the setting of this property, the text of the control will contain no prompt characters or literals (just the raw data), the data and just the literals, the data and just the prompt characters, or all the text including both prompt characters and literals. The formatted spacing of partially masked values can be preserved by indicating to include literals with padding, which includes data and literal characters, but replaces prompt characters with pad characters (usually spaces).</p>
		/// <p class="body">Generally, simply the raw data is committed to the data source and data masking is used to format the data when it is displayed. In some cases, however, it may be appropriate in your application to store mask literals as well as data.</p>
		/// <seealso cref="DisplayMode"/> <seealso cref="ClipMode"/>
		/// </remarks>
		//[Description( "Specifies the mask mode that will be applied to data returned by the Text and Value properties." )]
		//[Category( "Behavior" )]
		[Bindable( true )]
		public MaskMode DataMode
		{
			get
			{
				// JJD 4/27/07
				// Optimization - use the locally cached property 
				//return (MaskMode)this.GetValue( DataModeProperty );
				return this._cachedDataMode;
			}
			set
			{
				this.SetValue( DataModeProperty, value );
			}
		}

		private static void OnDataModeChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			XamMaskedEditor editor = (XamMaskedEditor)dependencyObject;
			MaskMode newVal = (MaskMode)e.NewValue;

			
			
			
			
			
			editor._cachedDataMode = newVal;

			if ( null != editor._maskInfo )
				editor._maskInfo.DataMode = newVal;

			// SSP 6/13/07 BR21830 BR21831
			// Make sure the Value, Text and DisplayText properties reflect the new DataMode 
			// setting.
			// 
			// SSP 1/3/07 BR28372 
			// Added check for editor.IsInitialized.
			// 
			if ( editor.IsInitialized )
				editor.ApplyMaskToText( );
		}

		/// <summary>
		/// Returns true if the DataMode property is set to a non-default value.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public bool ShouldSerializeDataMode( )
		{
			return Utilities.ShouldSerialize( DataModeProperty, this );
		}

		/// <summary>
		/// Resets the DataMode property to its default state.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public void ResetDataMode( )
		{
			this.ClearValue( DataModeProperty );
		}

		#endregion // DataMode

		#region ClipMode

		/// <summary>
		/// Identifies the <see cref="ClipMode"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ClipModeProperty = DependencyProperty.Register(
			"ClipMode",
			typeof( MaskMode ),
			typeof( XamMaskedEditor ),
			new FrameworkPropertyMetadata( MaskMode.IncludeLiterals, FrameworkPropertyMetadataOptions.None,
				new PropertyChangedCallback( OnClipModeChanged )
				
				
				
				
				
				
				
				//, new CoerceValueCallback(CoerceClipMode) 
			) );

		
		
		
		
		
		
		
		private MaskMode _cachedClipMode;

		
		
		
		
		
		
		
		
#region Infragistics Source Cleanup (Region)











#endregion // Infragistics Source Cleanup (Region)


		/// <summary>
		/// Returns or sets a value that determines how the control's contents will be copied to the clipboard when data masking is in enabled.
		/// </summary>
		/// <remarks>
		/// <p class="body">This property is used to determine how mask literals and prompt characters are handled when the control's contents are copied to the clipboard. Based on the setting of this property, the text of the control will contain no prompt characters or literals (just the raw data), the data and just the literals, the data and just the prompt characters, or all the text including both prompt characters and literals. The formatted spacing of partially masked values can be preserved by indicating to include literals with padding, which includes data and literal characters, but replaces prompt characters with pad characters (usually spaces).</p>
		/// <seealso cref="DataMode"/> <seealso cref="DisplayMode"/>
		/// </remarks>
		//[Description( "Specifies the mask mode that will be applied when copying text." )]
		//[Category( "Behavior" )]
		[Bindable( true )]
		public MaskMode ClipMode
		{
			get
			{
				// JJD 4/27/07
				// Optimization - use the locally cached property 
				//return (MaskMode)this.GetValue( ClipModeProperty );
				return this._cachedClipMode;
			}
			set
			{
				this.SetValue( ClipModeProperty, value );
			}
		}

		private static void OnClipModeChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			XamMaskedEditor editor = (XamMaskedEditor)dependencyObject;
			MaskMode newVal = (MaskMode)e.NewValue;

			
			
			
			
			
			editor._cachedClipMode = newVal;

			if ( null != editor._maskInfo )
				editor._maskInfo.ClipMode = newVal;
		}

		/// <summary>
		/// Returns true if the ClipMode property is set to a non-default value.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public bool ShouldSerializeClipMode( )
		{
			return Utilities.ShouldSerialize( ClipModeProperty, this );
		}

		/// <summary>
		/// Resets the ClipMode property to its default state.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public void ResetClipMode( )
		{
			this.ClearValue( ClipModeProperty );
		}

		#endregion // ClipMode

		#region DisplayMode

		/// <summary>
		/// Identifies the <see cref="DisplayMode"/> dependency property
		/// </summary>
		public static readonly DependencyProperty DisplayModeProperty = DependencyProperty.Register(
			"DisplayMode",
			typeof( MaskMode ),
			typeof( XamMaskedEditor ),
			new FrameworkPropertyMetadata( MaskMode.IncludeLiterals, FrameworkPropertyMetadataOptions.None,
				new PropertyChangedCallback( OnDisplayModeChanged )
				
				
				
				
				
				
				
				//, new CoerceValueCallback(CoerceDisplayMode) 
			) );

		
		
		
		
		
		
		
		private MaskMode _cachedDisplayMode;

		
		
		
		
		
		
		
		
#region Infragistics Source Cleanup (Region)











#endregion // Infragistics Source Cleanup (Region)


		/// <summary>
		/// Returns or sets a value that determines how the control's contents will be displayed when the control is not in edit mode and data masking is enabled.
		/// </summary>
		/// <remarks>
		/// <p class="body">This property is used to determine how mask literals and prompt characters are displayed when the control is not in edit mode. Based on the setting of this property, the text of the control will contain no prompt characters or literals (just the raw data), the data and just the literals, the data and just the prompt characters, or all the text including both prompt characters and literals. The formatted spacing of partially masked values can be preserved by indicating to include literals with padding, which includes data and literal characters, but replaces prompt characters with pad characters (usually spaces).</p>
		/// <p class="body">Generally, prompt characters disappear when a cell is no longer in edit mode, as a visual cue to the user. In some cases, however, it may be appropriate in your application to display mask literals as well as data when a cell is no longer in edit mode.</p>
		/// <seealso cref="DataMode"/> <seealso cref="ClipMode"/>
		/// </remarks>
		//[Description( "Specifies the mask mode that will be applied while not in edit mode." )]
		//[Category( "Behavior" )]
		[Bindable( true )]
		public MaskMode DisplayMode
		{
			get
			{
				// JJD 4/27/07
				// Optimization - use the locally cached property 
				//return (MaskMode)this.GetValue( DisplayModeProperty );
				return this._cachedDisplayMode;
			}
			set
			{
				this.SetValue( DisplayModeProperty, value );
			}
		}

		private static void OnDisplayModeChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			XamMaskedEditor editor = (XamMaskedEditor)dependencyObject;
			MaskMode newVal = (MaskMode)e.NewValue;

			
			
			
			
			
			editor._cachedDisplayMode = newVal;

			if ( null != editor._maskInfo )
				editor._maskInfo.DisplayMode = newVal;
		}

		/// <summary>
		/// Returns true if the DisplayMode property is set to a non-default value.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public bool ShouldSerializeDisplayMode( )
		{
			return Utilities.ShouldSerialize( DisplayModeProperty, this );
		}

		/// <summary>
		/// Resets the DisplayMode property to its default state.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public void ResetDisplayMode( )
		{
			this.ClearValue( DisplayModeProperty );
		}

		#endregion // DisplayMode

		#region PadChar

		/// <summary>
		/// Identifies the <see cref="PadChar"/> dependency property
		/// </summary>
		public static readonly DependencyProperty PadCharProperty = DependencyProperty.Register(
			"PadChar",
			typeof( char ),
			typeof( XamMaskedEditor ),
			new FrameworkPropertyMetadata( DEFAULT_PAD_CHAR, FrameworkPropertyMetadataOptions.None,
				new PropertyChangedCallback( OnPadCharChanged )
				
				
				
				
				
				
				
				//, new CoerceValueCallback(CoercePadChar) 
			),
			new ValidateValueCallback( ValidatePadChar )
			);

		
		
		
		
		
		
		
		private char _cachedPadChar;

		
		
		
		
		
		
		
		
#region Infragistics Source Cleanup (Region)











#endregion // Infragistics Source Cleanup (Region)


		/// <summary>
		/// Returns or sets the character that will be used as the pad character. Default is space character (' ').
		/// </summary>
		/// <remarks>
		/// <p class="body">The pad character is the character that is used to replace the prompt characters when getting the data from the XamMaskedEditor control with DataMode of IncludeLiteralsWithPadding.</p>
		/// <para class="body">
		/// For example, if the data in the editor is as follows:<br/>
		/// 111-2_-____<br/>
		/// and DataMode is set to IncludeLiteralsWithPadding then the returned value will be "111-2 -    ".
		/// Prompt characters will be replaced by the pad character.
		/// </para>
		/// <seealso cref="PromptChar"/>
		/// </remarks>
		//[Description( "Character used for padding the input when a placeholder character is empty. Default is space character." )]
		//[Category( "Behavior" )]
		[Bindable( true )]
		public char PadChar
		{
			get
			{
				// JJD 4/27/07
				// Optimization - use the locally cached property 
				//return (char)this.GetValue( PadCharProperty );
				return this._cachedPadChar;
			}
			set
			{
				this.SetValue( PadCharProperty, value );
			}
		}

		private static bool ValidatePadChar( object objVal )
		{
			return XamMaskedEditor.IsValidPromptChar( (char)objVal );
		}

		private static void OnPadCharChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			XamMaskedEditor editor = (XamMaskedEditor)dependencyObject;

			
			
			
			
			
			char newVal = (char)e.NewValue;
			editor._cachedPadChar = newVal;

			editor.NotifyDisplayCharDrawStringsChanged( );
		}

		/// <summary>
		/// Returns true if the PadChar property is set to a non-default value.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public bool ShouldSerializePadChar( )
		{
			return Utilities.ShouldSerialize( PadCharProperty, this );
		}

		/// <summary>
		/// Resets the PadChar property to its default state.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public void ResetPadChar( )
		{
			this.ClearValue( PadCharProperty );
		}

		#endregion // PadChar

		#region PromptChar

		/// <summary>
		/// Identifies the <see cref="PromptChar"/> dependency property
		/// </summary>
		public static readonly DependencyProperty PromptCharProperty = DependencyProperty.Register(
			"PromptChar",
			typeof( char ),
			typeof( XamMaskedEditor ),
			new FrameworkPropertyMetadata( DEFAULT_PROMPT_CHAR, FrameworkPropertyMetadataOptions.None,
				new PropertyChangedCallback( OnPromptCharChanged )
				
				
				
				
				
				
				
				
			),
			new ValidateValueCallback( ValidatePromptChar )
			);

		
		
		
		
		
		
		//private char _cachedPromptChar = DEFAULT_PROMPT_CHAR;
		private char _cachedPromptChar;

		
		
		
		
		
		
		
		
#region Infragistics Source Cleanup (Region)











#endregion // Infragistics Source Cleanup (Region)


		/// <summary>
		/// Returns or sets the prompt character. The default prompt character is the underscore (_).
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// Prompt character is the character that's displayed in place of any blank display characters.
		/// Each display character is a place holder in the mask where the user enters the characters
		/// as required by the mask. The default prompt character is underscore (_).
		/// </para>
		/// <seealso cref="PadChar"/>
		/// </remarks>
		//[Description( "Character used for prompts. Default is '_' character." )]
		//[Category( "Display" )]
		[Bindable( true )]
		public char PromptChar
		{
			get
			{
				// JJD 4/27/07
				// Optimization - use the locally cached property 
				//return (char)this.GetValue( PromptCharProperty );
				return this._cachedPromptChar;
			}
			set
			{
				this.SetValue( PromptCharProperty, value );
			}
		}

		private static bool ValidatePromptChar( object objVal )
		{
			return XamMaskedEditor.IsValidPromptChar( (char)objVal );
		}

		private static void OnPromptCharChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			XamMaskedEditor editor = (XamMaskedEditor)dependencyObject;

			
			
			
			
			
			
			
			char newVal = (char)e.NewValue;
			editor._cachedPromptChar = newVal;

			editor.NotifyDisplayCharDrawStringsChanged( );
		}

		/// <summary>
		/// Returns true if the PromptChar property is set to a non-default value.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public bool ShouldSerializePromptChar( )
		{
			return Utilities.ShouldSerialize( PromptCharProperty, this );
		}

		/// <summary>
		/// Resets the PromptChar property to its default state.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public void ResetPromptChar( )
		{
			this.ClearValue( PromptCharProperty );
		}

		#endregion // PromptChar

		#region SelectAll

		/// <summary>
		/// Selects all the text in the control.
		/// </summary>
		public void SelectAll( )
		{
			this.EnsureInEditMode( );

			this.EditInfo.SelectAll( );
		}

		#endregion //SelectAll

		#region Delete

		/// <summary>
		/// Deletes currently selected text if possible and shifts characters accordingly.
		/// </summary>
		/// <remarks>
		/// <p class="body">When you invoke this method, the control tries to delete the  currently selected text and shift characters. If nothing is selected, it tries to delete the character at the current input position.</p> 
		/// <p class="body">This method returns True if the operation was successful. If the operation fails because characters after the selection could not be shifted, the method returns False.</p>
		/// </remarks>
		/// <returns></returns>
		public bool Delete( )
		{
			this.EnsureInEditMode( );

            // SSP 5/13/10 TFS 31103
            // Pass true for the new fireTextChanged parameter, which will raise value/text changed events.
            // 
			//return this.EditInfo.Delete( );
            return this.EditInfo.Delete( false, true );
		}
		
		#endregion //Delete

		#region Copy
		
		/// <summary>
		/// Performs a Copy edit operation on the currently selected text, placing it on the clipboard.
		/// </summary>
		public void Copy( )
		{
			this.EnsureInEditMode( );
			this.EditInfo.Copy( );
		}

		#endregion //Copy

		#region Cut

		/// <summary>
		/// Performs a Cut edit operation on the currently selected text, removing it from the control and placing it on the clipboard.
		/// </summary>
		public void Cut( )
		{
			this.EnsureInEditMode( );

			this.EditInfo.Cut( );
		}

		#endregion //Cut

		#region Paste

		/// <summary>
		/// Performs a Paste edit operation.
		/// </summary>
		public void Paste( )
		{
			this.EnsureInEditMode( );
			this.EditInfo.Paste( );
		}
		
		#endregion //Paste
		
		#region ToggleInsertMode

		/// <summary>
		/// Toggles between insert and overstrike mode.
		/// </summary>
		public void ToggleInsertMode( )
		{
			this.EnsureInEditMode( );

			this.EditInfo.ToggleInsertMode( );
		}

		#endregion //ToggleInsertMode

		#region DisplayNullTextWhenNotFocused

		/// <summary>
		/// Indicates whether the mask editor displays NullText if the value entered 
		/// is null and the control doesn't have focus.
		/// </summary>
		[ EditorBrowsable( EditorBrowsableState.Advanced ), Browsable( false ), 
		DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden) ]
		internal bool DisplayNullTextWhenNotFocused
		{
			get
			{
				return this._displayNullTextWhenNotFocused;
			}
			set
			{
				this._displayNullTextWhenNotFocused = value;
			}
		}

		#endregion // DisplayNullTextWhenNotFocused

		#region TabNavigation

		/// <summary>
		/// Identifies the <see cref="TabNavigation"/> dependency property
		/// </summary>
		public static readonly DependencyProperty TabNavigationProperty = DependencyProperty.Register(
			"TabNavigation",
			typeof(MaskedEditTabNavigation),
			typeof(XamMaskedEditor),
			new FrameworkPropertyMetadata(MaskedEditTabNavigation.NextControl, FrameworkPropertyMetadataOptions.None,
				null)
			);

		/// <summary>
		/// Specifies whether to tab between sections when Tab and Shift+Tab keys are pressed.
		/// The default value is <b>NextControl</b>.
		/// </summary>
		/// <remarks>
		/// <seealso cref="AllowShiftingAcrossSections"/>
		/// <seealso cref="InsertMode"/>
		/// <seealso cref="SelectAllBehavior"/>
		/// <seealso cref="AutoFillDate"/>
		/// </remarks>
		//[Description("Specifies whether to tab between sections.")]
		//[Category("Behavior")]
		public MaskedEditTabNavigation TabNavigation
		{
			get
			{
				return (MaskedEditTabNavigation)this.GetValue(TabNavigationProperty);
			}
			set
			{
				this.SetValue(TabNavigationProperty, value);
			}
		}

		/// <summary>
		/// Returns true if the TabNavigation property is set to a non-default value.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeTabNavigation()
		{
			return Utilities.ShouldSerialize(TabNavigationProperty, this);
		}

		/// <summary>
		/// Resets the TabNavigation property to its default state.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetTabNavigation()
		{
			this.ClearValue(TabNavigationProperty);
		}

		#endregion // TabNavigation

		#region CreateContextMenu

		internal ContextMenu CreateContextMenu( )
		{
			ContextMenu menu = new ContextMenu( );
			// AS 2/25/11 TFS67071
			// We call this method when the editor goes into edit mode to 
			// initialize the imetexteditor (as well as when OnContextMenuOpening 
			// is invoked). Since the menu items would have commands associated 
			// with them, every mouse/key down will cause the (Preview)CanExecute 
			// routed events (since wpf explicitly calls InvalidRequerySuggested on 
			// mouse/key down). So I moved the impl into a static method that a 
			// helper class will use to lazily populate the commands only while 
			// the menu is open.
			//
			//MenuItem item;
			//
			//IInputElement commandTarget = this;
			ContextMenuHelper helper = new ContextMenuHelper(this, menu);
			return menu;
		}

		// AS 2/25/11 TFS67071
		private static void InitializeContextMenu(XamMaskedEditor commandTarget, ContextMenu menu)
		{
			MenuItem item = new MenuItem();
			menu.Items.Add(item);
			item.Command = MaskedEditorCommands.Undo;
			item.CommandTarget = commandTarget;

			menu.Items.Add( new Separator( ) );

			item = new MenuItem( );
			menu.Items.Add( item );
			item.Command = MaskedEditorCommands.Cut;
			item.CommandTarget = commandTarget;

			item = new MenuItem( );
			menu.Items.Add( item );
			item.Command = MaskedEditorCommands.Copy;
			item.CommandTarget = commandTarget;

			item = new MenuItem( );
			menu.Items.Add( item );
			item.Command = MaskedEditorCommands.Paste;
			item.CommandTarget = commandTarget;

			item = new MenuItem( );
			menu.Items.Add( item );
			item.Command = MaskedEditorCommands.Delete;
			item.CommandTarget = commandTarget;

			menu.Items.Add( new Separator( ) );

			item = new MenuItem( );
			menu.Items.Add( item );
			item.Command = MaskedEditorCommands.SelectAll;
			item.CommandTarget = commandTarget;

			// AS 2/25/11 TFS67071
			//return menu;
		}

		#endregion // CreateContextMenu

		#region TabNavigationResolved

		internal MaskedEditTabNavigation TabNavigationResolved
		{
			get
			{
				MaskedEditTabNavigation ret = this.TabNavigation;

				if ( MaskedEditTabNavigation.NextSection == ret && null != this.EditInfo 
					&& this.EditInfo.LastEditSection is FractionPartContinuous )
					ret = MaskedEditTabNavigation.NextControl;

				return ret;
			}
		}

		#endregion // TabNavigationResolved

		#region AllowShiftingAcrossSections

		
		
		
		/// <summary>
		/// Specifies whether to shift characters across section boundaries when deleting characters.
		/// Default value is <b>True</b>.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// This property controls what happens to the characters in the following sections when one or 
		/// more characters are deleted in the current section. For example, in a simple mask like 
		/// "###-###" where there are two input sections separated by a '-' character. Each input section
		/// comprises of three digit placeholders. Let's say the whole mask is currently filled with data. 
		/// When you delete a character in the first section, this property controls whether the characters
		/// from the next input section flow into this section or not.
		/// </para>
		/// <para class="body">
		/// Continuing from above example, let's say the editor has the following data:<br/>
		/// "123-456"<br/>
		/// If the caret is located before the character '2' and you hit Delete key, the 2 will be deleted 
		/// and '3' will be shifted left to occupy the position '2' had occupied. This will happen regardless
		/// of the AllowShiftingAcrossSections setting. However what happens after this depends on the value
		/// of this property.
		/// <b>If AllowShiftingAcrossSections is set to False</b>, the position '3' originally occupied will
		/// become empty and the resulting display characters will be as the following:<br/>
		/// "13_-456"<br/>
		/// <b>If AllowShiftingAcrossSections is set to True</b>, characters from the next input section will
		/// 'flow' into current input section to fill in the position vacated by '3' when it got shifted left
		/// to occpy the position of '2'. Here is how the resulting display characters will look like:<br/>
		/// "134-56_"<br/>
		/// </para>
		/// <para class="body">
		/// What value you would use for this property depends on the kind of mask that you have. The usability
		/// is greatly affected by the value of this property depending on the mask being used. For example, in
		/// a Date mask where each section of the date (month, day, year) is a logically distinct value, you 
		/// would not want to shift values across sections. As a matter of fact, for certain built in masks
		/// like date, time etc... shifting across sections can not be enabled even by setting this property to
		/// True since that is something that would not be desirable under any circumstance for those masks. However 
		/// for custom masks, you may want to set this property to a value based on whether it makes sense for that 
		/// particular mask to shift characters across sections.
		/// </para>
		/// <seealso cref="InsertMode"/>
		/// <seealso cref="SelectAllBehavior"/>
		/// <seealso cref="TabNavigation"/>
		/// <seealso cref="AutoFillDate"/>
		/// </remarks>
		public bool AllowShiftingAcrossSections
		{
			get
			{
				return this._allowShiftingAcrossSections;
			}
			set
			{
				if ( this._allowShiftingAcrossSections != value )
				{
					this._allowShiftingAcrossSections = value;
				}
			}
		}

		#endregion // AllowShiftingAcrossSections

		#region IsValidPromptChar

		/// <summary>
		/// Indicates if the specified character is valid for use as a prompt character for the <see cref="XamMaskedEditor"/>.
		/// </summary>
		/// <param name="promptCharacter">Character to evaluate</param>
		/// <returns>False if the character is a tab, new line or carriage return. Otherwise true is returned.</returns>
		private static bool IsValidPromptChar(char promptCharacter)
		{
			return ( "\x08\x09\x13".IndexOf( promptCharacter ) < 0 );
		}

		#endregion //IsValidPromptChar

		#region SelectionStart

		/// <summary>
		/// Indicates the start location of the selected text. If no text is selected, this property indicates
		/// the location of the caret.
		/// </summary>
		//[Description( "Indicates the location of the selected text." )]
		//[Category( "Data" )]
		[Browsable( false )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public int SelectionStart
		{
			get
			{
				return null != this.EditInfo ? this.EditInfo.SelectionStart : 0;
			}
			set
			{
				if ( null != this.EditInfo )
					this.EditInfo.SelectionStart = value;
			}
		}

		#endregion // SelectionStart

		#region SelectionLength

		/// <summary>
		/// Gets/sets the length of the selected text. If nothing is selected then returns 0.
		/// </summary>
		//[Description( "The length of the selected text." )]
		//[Category( "Data" )]
		[Browsable( false )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public int SelectionLength
		{
			get
			{
				return null != this.EditInfo ? this.EditInfo.SelectionLength : 0;
			}
			set
			{
				if ( null != this.EditInfo )
					this.EditInfo.SelectionLength = value;
			}
		}

		#endregion // SelectionLength

		#region SelectedText

		/// <summary>
		/// Gets or sets the selected text.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>SelectedText</b> property returns the currently selected text if any. If nothing is selected
		/// then returns empty string.
		/// </para>
		/// <para class="body">
		/// Setting this property replaces the current selected text with the set value. If nothing
		/// is selected and the property is set, the set value is inserted at the location of the caret.
		/// Note that setting this property will modify the contents of the control.
		/// </para>
		/// </remarks>
		//[Description( "The selected text." )]
		//[Category( "Data" )]
		[Browsable( false )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public string SelectedText
		{
			get
			{
				return null != this.EditInfo ? this.EditInfo.SelectedText : null;
			}
			set
			{
				if ( null != this.EditInfo )
					this.EditInfo.SelectedText = value;
			}
		}

		#endregion // SelectedText

		#region TextLength

		/// <summary>
		/// Indicates the total length of the text in the control when in edit mode.
		/// </summary>
		//[Description( "The total length of the text in the control when in edit mode." )]
		//[Category( "Data" )]
		[Browsable( false )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public int TextLength
		{
			get
			{
				return null != this.EditInfo ? this.EditInfo.GetTotalNumberOfDisplayChars( ) : 0;
			}
		}

		#endregion // TextLength

		#region CaretPosition

		/// <summary>
		/// Identifies the <see cref="CaretPosition"/> dependency property
		/// </summary>
		internal static readonly DependencyProperty CaretPositionProperty = DependencyProperty.Register(
			"CaretPosition",
			typeof( int ),
			typeof( XamMaskedEditor ),
			new FrameworkPropertyMetadata( 0, FrameworkPropertyMetadataOptions.AffectsArrange,
				new PropertyChangedCallback( OnCaretPositionChanged ),
				new CoerceValueCallback( OnCoerceCaretPosition ) )
			);

		/// <summary>
		/// Gets or sets the current caret position. Only valid when the editor is in edit mode.
		/// </summary>
		//[Description( "Specifies the current caret position" )]
		//[Category( "Behavior" )]
		[Bindable( true )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		internal int CaretPosition
		{
			get
			{
				return (int)this.GetValue( CaretPositionProperty );
			}
			set
			{
				this.SetValue( CaretPositionProperty, value );
			}
		}

		private static void OnCaretPositionChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			XamMaskedEditor maskedEditor = (XamMaskedEditor)dependencyObject;
			int newVal = (int)e.NewValue;

			if ( null != maskedEditor && null != maskedEditor.EditInfo )
			{
				maskedEditor.EditInfo.CaretPosition = newVal;
				maskedEditor.ProcessSelectionChanged( );
			}
		}

		internal void ProcessSelectionChanged( )
		{
			EditInfo editInfo = this.EditInfo;
			if ( null != editInfo )
			{
				XamMaskedEditor.NotifyPropertyOnDisplayCharacters( this.EditInfo.Sections, DisplayCharBase.PROPERTY_DRAWSELECTED );
				this.SyncDrawAsSelectedOnDisplayPresenters( );

				// SSP 3/23/09 IME
				// 
				editInfo.SyncIMETextBox( );

				// SSP 9/8/09 TFS21334
				// To be consistent with the WPF TextBox which doesn't show the caret when
				// there's some text selected, we should show or hide the caret accordingly 
				// when the selection is changed.
				// 
				this.ShowHideCaretHelper( );
			}
		}

		private static object OnCoerceCaretPosition( DependencyObject dependencyObject, object valueAsObject )
		{
			XamMaskedEditor maskedEditor = (XamMaskedEditor)dependencyObject;
			int val = (int)valueAsObject;

			if ( null != maskedEditor.EditInfo )
				val = maskedEditor.EditInfo.CoerceCaretPosition( val );
			else
				val = 0;

			return val;
		}

		#endregion // CaretPosition

		#region PivotPosition

		/// <summary>
		/// Identifies the <see cref="PivotPosition"/> dependency property
		/// </summary>
		internal static readonly DependencyProperty PivotPositionProperty = DependencyProperty.Register(
			"PivotPosition",
			typeof( int ),
			typeof( XamMaskedEditor ),
			new FrameworkPropertyMetadata( 0, FrameworkPropertyMetadataOptions.AffectsArrange,
				new PropertyChangedCallback( OnPivotPositionChanged ),
				new CoerceValueCallback( OnCoercePivotPosition ) )
			);

		/// <summary>
		/// Gets or sets the pivot position. Pivot position and caret positions indicate which
		/// characters are selected in the control.
		/// </summary>
		//[Description( "Specifies the pivot position" )]
		//[Category( "Behavior" )]
		[Bindable( true )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		internal int PivotPosition
		{
			get
			{
				return (int)this.GetValue( PivotPositionProperty );
			}
			set
			{
				this.SetValue( PivotPositionProperty, value );
			}
		}

		private static void OnPivotPositionChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			XamMaskedEditor maskedEditor = (XamMaskedEditor)dependencyObject;
			int newVal = (int)e.NewValue;

			if ( null != maskedEditor && null != maskedEditor.EditInfo )
			{
				maskedEditor.EditInfo.PivotPosition = newVal;
				maskedEditor.ProcessSelectionChanged( );
			}
		}

		private static object OnCoercePivotPosition( DependencyObject dependencyObject, object valueAsObject )
		{
			XamMaskedEditor maskedEditor = (XamMaskedEditor)dependencyObject;
			int val = (int)valueAsObject;

			if ( maskedEditor.IsInEditMode )
				val = maskedEditor.EditInfo.CoercePivotPosition( val );
			else
				val = 0;

			return val;
		}

		#endregion // PivotPosition

		#region ExecutingCommand

		/// <summary>
		/// Event ID for the <see cref="ExecutingCommand"/> routed event
		/// </summary>
		/// <seealso cref="ExecutingCommand"/>
		/// <seealso cref="OnExecutingCommand"/>
		/// <seealso cref="ExecutingCommandEventArgs"/>
		public static readonly RoutedEvent ExecutingCommandEvent =
			EventManager.RegisterRoutedEvent( "ExecutingCommand", RoutingStrategy.Bubble, typeof( EventHandler<ExecutingCommandEventArgs> ), typeof( XamMaskedEditor ) );

		/// <summary>
		/// Occurs before a command is performed
		/// </summary>
		/// <seealso cref="ExecutingCommand"/>
		/// <seealso cref="ExecutingCommandEvent"/>
		/// <seealso cref="ExecutingCommandEventArgs"/>
		protected virtual void OnExecutingCommand( ExecutingCommandEventArgs args )
		{
			// MD 7/16/10 - TFS26592
			// Call off to the new helper method to raise the event. 
			//this.RaiseEvent( args );
			this.RaiseEventHelper(args);
		}

		internal bool RaiseExecutingCommand( ExecutingCommandEventArgs args )
		{
			args.RoutedEvent = XamMaskedEditor.ExecutingCommandEvent;
			args.Source = this;
			this.OnExecutingCommand( args );

			return args.Cancel == false;
		}

		/// <summary>
		/// Occurs before a command is performed
		/// </summary>
		/// <seealso cref="OnExecutingCommand"/>
		/// <seealso cref="ExecutingCommandEvent"/>
		/// <seealso cref="ExecutingCommandEventArgs"/>
		//[Description( "Occurs before a command is performed" )]
		//[Category( "Behavior" )]
		public event EventHandler<ExecutingCommandEventArgs> ExecutingCommand
		{
			add
			{
				base.AddHandler( XamMaskedEditor.ExecutingCommandEvent, value );
			}
			remove
			{
				base.RemoveHandler( XamMaskedEditor.ExecutingCommandEvent, value );
			}
		}

		#endregion //ExecutingCommand

		#region ExecutedCommand

		/// <summary>
		/// Event ID for the <see cref="ExecutedCommand"/> routed event
		/// </summary>
		/// <seealso cref="ExecutedCommand"/>
		/// <seealso cref="OnExecutedCommand"/>
		/// <seealso cref="ExecutedCommandEventArgs"/>
		public static readonly RoutedEvent ExecutedCommandEvent =
			EventManager.RegisterRoutedEvent( "ExecutedCommand", RoutingStrategy.Bubble, typeof( EventHandler<ExecutedCommandEventArgs> ), typeof( XamMaskedEditor ) );

		/// <summary>
		/// Occurs after a command is performed
		/// </summary>
		/// <seealso cref="ExecutedCommand"/>
		/// <seealso cref="ExecutedCommandEvent"/>
		/// <seealso cref="ExecutedCommandEventArgs"/>
		protected virtual void OnExecutedCommand( ExecutedCommandEventArgs args )
		{
			// MD 7/16/10 - TFS26592
			// Call off to the new helper method to raise the event. 
			//this.RaiseEvent( args );
			this.RaiseEventHelper(args);
		}

		internal void RaiseExecutedCommand( ExecutedCommandEventArgs args )
		{
			args.RoutedEvent = XamMaskedEditor.ExecutedCommandEvent;
			args.Source = this;
			this.OnExecutedCommand( args );
		}

		/// <summary>
		/// Occurs after a command is performed
		/// </summary>
		/// <seealso cref="OnExecutedCommand"/>
		/// <seealso cref="ExecutedCommandEvent"/>
		/// <seealso cref="ExecutedCommandEventArgs"/>
		//[Description( "Occurs after a command is performed" )]
		//[Category( "Behavior" )]
		public event EventHandler<ExecutedCommandEventArgs> ExecutedCommand
		{
			add
			{
				base.AddHandler( XamMaskedEditor.ExecutedCommandEvent, value );
			}
			remove
			{
				base.RemoveHandler( XamMaskedEditor.ExecutedCommandEvent, value );
			}
		}

		#endregion //ExecutedCommand

		#region ExecuteCommand

		/// <summary>
		/// Executes the RoutedCommand represented by the specified CommandWrapper.
		/// </summary>
		/// <param name="commandWrapper">The CommandWrapper that contains the RoutedCommand to execute</param>
		/// <returns>True if command was executed, false if canceled.</returns>
		/// <seealso cref="MaskedEditorCommands"/>
		public bool ExecuteCommand( CommandWrapper commandWrapper )
		{
			if ( commandWrapper == null )
				throw new ArgumentNullException( "commandWrapper" );


			return this.ExecuteCommandImpl( commandWrapper.Command, null );
		}

		/// <summary>
		/// Executes the specified RoutedCommand.
		/// </summary>
		/// <param name="command">The RoutedCommand to execute.</param>
		/// <returns>True if command was executed, false if canceled.</returns>
		/// <seealso cref="MaskedEditorCommands"/>
		public bool ExecuteCommand( RoutedCommand command )
		{
			return this.ExecuteCommandImpl( command, null );
		}

		private bool ExecuteCommandImpl( RoutedCommand command, object commandParameter )
		{
			// Make sure we have a command to execute.
			if ( command == null )
				throw new ArgumentNullException( "command" );


			// Make sure the minimal control state exists to execute the command.
			if ( MaskedEditorCommands.IsMinimumStatePresentForCommand( this as ICommandHost, command ) == false )
				return false;

			// Fire the 'before executed' cancelable event.
			ExecutingCommandEventArgs beforeArgs = new ExecutingCommandEventArgs( command );
			bool proceed = this.RaiseExecutingCommand( beforeArgs );

			if ( proceed == false )
            {
                // JJD 06/02/10 - TFS33112
                // Return the inverse of ContinueKeyRouting so that the developer can prevent
                // the original key message from bubbling
                //return false;
                return !beforeArgs.ContinueKeyRouting;
            }

			// Setup some info needed by more than 1 command.
			bool shiftKeyDown = ( Keyboard.Modifiers & ModifierKeys.Shift ) != 0;
			bool ctlKeyDown = ( Keyboard.Modifiers & ModifierKeys.Control ) != 0;
			bool tabKeyDown = Keyboard.IsKeyDown( Key.Tab );

			// Determine which of our supported commands should be executed and do the associated action.
			bool handled = false;


			if ( this.IsInEditMode && null != this.EditInfo
				// SSP 10/5/09 - NAS10.1 Spin Buttons
				// Added an commandParameter parameter to the ExecuteCommandImpl method.
				// 
				//&& this.EditInfo.ExecuteCommandImpl( command, shiftKeyDown, ctlKeyDown ) 
				&& this.EditInfo.ExecuteCommandImpl( command, commandParameter, shiftKeyDown, ctlKeyDown ) 
				)
				handled = true;

			// If the command was executed, fire the 'after executed' event.
			if ( handled == true )
				this.RaiseExecutedCommand( new ExecutedCommandEventArgs( command ) );

			return handled;
		}

		#endregion //ExecuteCommandImpl

		#endregion //Public properties/methods

		#region GetDefaultMask

		/// <summary>
		/// Gets the default mask for the editor. When the owner doesn't provide any
		/// mask, value of this property will be used as the default mask. Default value is
		/// null. You should only override this property if your editor only supports a specific 
		/// type. For example, DateTimeEditor only supports Date. Editing numbers, or strings 
		/// do not make sense for a date time editor. So for such editors, override and return
		/// a default mask for that editor.
		/// </summary>
		internal protected virtual string GetDefaultMask( Type dataType, IFormatProvider formatProvider )
		{
			return null;
		}

		#endregion GetDefaultMask

		#region SelectAllBehavior

		/// <summary>
		/// Identifies the <see cref="SelectAllBehavior"/> dependency property
		/// </summary>
		public static readonly DependencyProperty SelectAllBehaviorProperty = DependencyProperty.Register(
			"SelectAllBehavior",
			typeof( MaskSelectAllBehavior ),
			typeof( XamMaskedEditor ),
			new FrameworkPropertyMetadata( MaskSelectAllBehavior.SelectAllCharacters, FrameworkPropertyMetadataOptions.None,
				null )
			);

		/// <summary>
		/// Specifies whether to select only the entered characters or all the characters (including prompt
		/// characters) when the editor performs the operation of select all text. The default value of
		/// the property is <b>SelectAllCharacters</b>.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// When this property is set to <b>SelectEnteredCharacters</b>, the select-all-text operation will
		/// select text starting from the first entered character to the last entered character, including
		/// adjacent literals.
		/// </p>
		/// </remarks>
		/// <seealso cref="AllowShiftingAcrossSections"/>
		/// <seealso cref="InsertMode"/>
		/// <seealso cref="TabNavigation"/>
		/// <seealso cref="AutoFillDate"/>
		//[Description( "Specifies whether to select only the entered characters or all the characters." )]
		//[Category( "Behavior" )]
		public MaskSelectAllBehavior SelectAllBehavior
		{
			get
			{
				return (MaskSelectAllBehavior)this.GetValue( SelectAllBehaviorProperty );
			}
			set
			{
				this.SetValue( SelectAllBehaviorProperty, value );
			}
		}

		/// <summary>
		/// Returns true if the SelectAllBehavior property is set to a non-default value.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public bool ShouldSerializeSelectAllBehavior( )
		{
			return Utilities.ShouldSerialize( SelectAllBehaviorProperty, this );
		}

		/// <summary>
		/// Resets the SelectAllBehavior property to its default state.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public void ResetSelectAllBehavior( )
		{
			this.ClearValue( SelectAllBehaviorProperty );
		}

		#endregion // SelectAllBehavior

		#region AutoFillDate

		// SSP 10/20/09 TFS23496
		// Converted AutoFillDate from a CLR property to dependency property.
		// 

		/// <summary>
		/// Identifies the <see cref="AutoFillDate"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty AutoFillDateProperty = DependencyProperty.Register(
			"AutoFillDate",
			typeof( AutoFillDate ),
			typeof( XamMaskedEditor ),
			new FrameworkPropertyMetadata( AutoFillDate.None )
		);

		/// <summary>
		/// Specifies whether to auto-fill empty date components when the user attempts to leave the editor.
		/// The default is <b>None</b>.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// If the user types in an incomplete date then the editor will consider the input invalid
		/// and take appropriate actions based on the <see cref="ValueEditor.InvalidValueBehavior"/>
		/// property setting. <b>AutoFillDate</b> lets you specify that such partial date inputs be
		/// auto-filled using the current date.
		/// </para>
		/// <para class="body">
		/// If you set the AutoFillDate to <b>Year</b> then the user will be required to enter both 
		/// the month and the day. The year if left blank will be filled in with the current year.
		/// If AutoFillDate is set to <b>MonthAndYear</b> then the user will be required to enter
		/// the day. The month and year if left blank will be filled in with the current month and
		/// the current year respectively.
		/// </para>
		/// <seealso cref="AllowShiftingAcrossSections"/>
		/// <seealso cref="InsertMode"/>
		/// <seealso cref="SelectAllBehavior"/>
		/// <seealso cref="TabNavigation"/>
		/// </remarks>
		//[Description( "Specifies whether to auto-fill empty date components when the user leaves the editor." )]
		//[Category( "Behavior" )]
		[Bindable( true )]
		public AutoFillDate AutoFillDate
		{
			get
			{
				return (AutoFillDate)this.GetValue( AutoFillDateProperty );
			}
			set
			{
				this.SetValue( AutoFillDateProperty, value );
			}
		}

		/// <summary>
		/// Returns true if the AutoFillDate property is set to a non-default value.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public bool ShouldSerializeAutoFillDate( )
		{
			return Utilities.ShouldSerialize( AutoFillDateProperty, this );
		}

		/// <summary>
		/// Resets the AutoFillDate property to its default state.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public void ResetAutoFillDate( )
		{
			this.ClearValue( AutoFillDateProperty );
		}

		
#region Infragistics Source Cleanup (Region)














#endregion // Infragistics Source Cleanup (Region)


		#endregion // AutoFillDate

		#region DefaultValueToDisplayTextConverter

		/// <summary>
		/// Returns the default converter used for converting between the value and the text.
		/// </summary>
		protected override IValueConverter DefaultValueToDisplayTextConverter
		{
			get
			{
				return MaskedEditorDefaultConverter.ValueToDisplayTextConverter;
			}
		}

		#endregion // DefaultValueToDisplayTextConverter

		#region DefaultValueToTextConverter

		/// <summary>
		/// Returns the default converter used for converting between the value and the text.
		/// </summary>
		protected override IValueConverter DefaultValueToTextConverter
		{
			get
			{
				return MaskedEditorDefaultConverter.ValueToTextConverter;
			}
		}

		#endregion // DefaultValueToTextConverter

		#region ProcessKeyDown

		/// <summary>
		/// Processes the key down event args. Default implementation does nothing.
		/// This class overrides OnKeyDown and performs some default processing and
		/// then calls this method if further key down processing is to be done.
		/// Derived classes are intended to override this method instead of OnKeyDown.
		/// </summary>
		/// <param name="e"></param>
		internal protected override void ProcessKeyDown( KeyEventArgs e )
		{
			// First call the base implementation.
			base.ProcessKeyDown( e );
			if ( e.Handled )
				return;

			// Pass this key along to our commands class which will check to see if a command
			// needs to be executed.  If so, the commands class will execute the command and
			// return true.
			if ( this.Commands != null && this.Commands.ProcessKeyboardInput( e, this as ICommandHost ) )
				e.Handled = true;
		}

		#endregion // ProcessKeyDown

		#region OnTextInput

		/// <summary>
		/// Overridden.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnTextInput( TextCompositionEventArgs e )
		{
			base.OnTextInput( e );

			if ( !e.Handled && this.IsInEditMode && null != this.EditInfo && null != e.Text && e.Text.Length > 0 )
				
				
				
				
				
				
				
				this.EditInfo.ProcessTextInput( e, true );
		}

		#endregion // OnTextInput

		#region Commands

		/// <summary>
		/// Gets the supported commands (read-only) 
		/// </summary>
		/// <value>A static instance of the <see cref="MaskedEditorCommands"/> class.</value>
		/// <remarks>
		/// <p class="body">This class exposes properties that return all of the commands that the control understands. 
		/// </p>
		/// </remarks>
		protected CommandsBase Commands
		{
			get { return MaskedEditorCommands.Instance; }
		}

		#endregion //Commands	

		#region ICommandHost Members

			#region ICommandHost.CurrentState

		// SSP 3/18/10 TFS29783 - Optimizations
		// Changed CurrentState property to a method.
		// 
		Int64 ICommandHost.GetCurrentState( Int64 statesToQuery )
		{
			// SSP 8/16/10 TFS27897 - Optimizations
			// Added logic to calculate only the states that are queried for.
			// 
			//if ( null != this.EditInfo )
			//	return (long)this.EditInfo.CurrentState & statesToQuery;
			EditInfo editInfo = this.EditInfo;
			if ( null != editInfo )
				return (long)editInfo.GetCurrentState( (MaskedEditorStates)statesToQuery );

			return 0;
		}

			#endregion //ICommandHost.CurrentState

			#region ICommandHost.CanExecute

		// AS 2/5/08 ExecuteCommandInfo
		//bool ICommandHost.CanExecute(RoutedCommand command, object commandParameter)
		bool ICommandHost.CanExecute(ExecuteCommandInfo commandInfo)
		{
			RoutedCommand command = commandInfo.RoutedCommand;

			// SSP 10/5/09 - NAS10.1 Spin Buttons
			// 
			object commandParameter = commandInfo.Parameter;

			if (this.IsInEditMode && null != this.EditInfo)
			{
				EditInfo editInfo = this.EditInfo;
				MaskedEditorStates state = editInfo.CurrentState;

				// SSP 11/5/10 TFS30131
				// If the control is read-only then return false to indicate the command is not allowed.
				// This is mainly to disable the context menu entries for cut, paste etc...
				// 
				bool isReadOnly = this.IsReadOnly;
				if ( isReadOnly )
				{
					if ( MaskedEditorCommands.Delete == command
						|| MaskedEditorCommands.Cut == command
						|| MaskedEditorCommands.Paste == command
						|| MaskedEditorCommands.Undo == command
						|| MaskedEditorCommands.Redo == command )
						return false;
				}

				if (MaskedEditorCommands.Delete == command)
				{
					// In order to delete, the caret must not be after the last character
					// or something has to be selected.
					// 
					if (0 == (MaskedEditorStates.AfterLastCharacter & state)
						|| 0 != (MaskedEditorStates.Selected & state))
						return true;
				}
				else if (MaskedEditorCommands.Copy == command)
				{
					if (0 != (MaskedEditorStates.Selected & state))
						return true;
				}
				else if (MaskedEditorCommands.Cut == command)
				{
					if (0 != (MaskedEditorStates.Selected & state))
						return true;
				}
				else if (MaskedEditorCommands.Paste == command)
				{
					return true;
				}
				else if (MaskedEditorCommands.SelectAll == command)
				{
					return true;
				}
				else if (MaskedEditorCommands.Undo == command)
				{
					if (0 != (MaskedEditorStates.CanUndo & state))
						return true;
				}
				else if (MaskedEditorCommands.Redo == command)
				{
					if (0 != (MaskedEditorStates.CanRedo & state))
						return true;
				}
				// SSP 10/5/09 - NAS10.1 Spin Buttons
				// Added amount parameter to support SpinUp and SpinDown commands taking a parameter that 
				// indicates the amount by which to spin.
				// 
				else if ( MaskedEditorCommands.SpinDown == command || MaskedEditorCommands.SpinUp == command )
				{
					if ( editInfo.CanSpin( MaskedEditorCommands.SpinUp == command, commandParameter ) )
						return true;
				}
			}
			else // JM 12-02-08 TFS11061
				commandInfo.ContinueRouting = true;

			// SSP 6/7/07 BR22768
			// If the command is NotACommand then always return true as it's
			// also allowed.
			// 
			if ( MaskedEditorCommands.NotACommand == command )
				return true;

			return command != null && command.OwnerType == typeof(MaskedEditorCommands);
		}

			#endregion //ICommandHost.CanExecute

			#region ICommandHost.Execute

		// AS 2/5/08 ExecuteCommandInfo
		//void ICommandHost.Execute( ExecutedRoutedEventArgs args )
		bool ICommandHost.Execute( ExecuteCommandInfo commandInfo )
		{
			// AS 2/5/08 ExecuteCommandInfo
			//RoutedCommand command = args.Command as RoutedCommand;
			//if ( command != null )
			//    args.Handled = this.ExecuteCommandImpl( command, args.Parameter );
			RoutedCommand command = commandInfo.RoutedCommand;
			return null != command && this.ExecuteCommandImpl(command, commandInfo.Parameter);
		}

			#endregion //ICommandHost.Execute

		#endregion //ICommandHost Members

		#region Caret Logic

		/// <summary>
		/// Identifies the <see cref="CaretVisible"/> dependency property
		/// </summary>
		internal static readonly DependencyProperty CaretVisibleProperty = DependencyProperty.Register(
			"CaretVisible",
			typeof( bool ),
			typeof( XamMaskedEditor ),
			new FrameworkPropertyMetadata( KnownBoxes.FalseBox, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure,
					
					
					
					
					
					
					
					
					new PropertyChangedCallback( OnCaretVisibleChanged )
					)
			);

		// JJD 4/27/07
		// Optimization - cache the property locally
		private bool _cachedCaretVisible;

		
		
		
		
		
		
		
		
#region Infragistics Source Cleanup (Region)











#endregion // Infragistics Source Cleanup (Region)


		private static void OnCaretVisibleChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			XamMaskedEditor editor = (XamMaskedEditor)dependencyObject;

			
			
			
			
			
			bool newVal = (bool)e.NewValue;
			editor._cachedCaretVisible = newVal;

			editor.NotifyDisplayCharDrawStringsChanged( );
		}

		/// <summary>
		/// Flag that indicates whether the caret should be shown.
		/// </summary>
		internal bool CaretVisible
		{
			get
			{
				// JJD 4/27/07
				// Optimization - use the locally cached property 
				//return (bool)this.GetValue( CaretVisibleProperty );
				return this._cachedCaretVisible;
			}
			set
			{
				this.SetValue( CaretVisibleProperty, KnownBoxes.FromValue(value) );
			}
		}

		private void ShowCaretElement( )
		{
			if ( ! this.CaretVisible )
			{
				// SSP 3/23/09 IME
				// If we are using text box for editing then don't display the caret.
				// Enclosed the existing code into the if block.
				// 
				if ( !this.IsTextBoxEntryEnabled )
				{
					this.CaretVisible = true;
					this.AddVisualChild( this.CaretElement );
				}
			}
		}

		private void HideCaretElement( )
		{
			if ( this.CaretVisible )
			{
				this.RemoveVisualChild( this.CaretElement );
				this.CaretVisible = false;
			}
		}

		// SSP 9/8/09 TFS21334
		// Added ShowHideCaretHelper method.
		// 
		internal void ShowHideCaretHelper( )
		{
			MaskInfo maskInfo = this.MaskInfo;
			EditInfo editInfo = this.EditInfo;

			bool shouldCaretBeVisible = null != maskInfo && null != editInfo 
				&& maskInfo.IsBeingEditedAndFocused
				&& ! this.IsTextBoxEntryEnabled
				&& 0 == editInfo.SelectionLength;

			if ( shouldCaretBeVisible )
				this.ShowCaretElement( );
			else
				this.HideCaretElement( );
		}

		internal void ResetCaretBlinking( )
		{
			if ( null != this._caretElement )
				this._caretElement.ResetCaretBlinking( );
		}

		/// <summary>
		/// Returns the total numder of visual children (read-only).
		/// </summary>
		protected override int VisualChildrenCount
		{
			get
			{
				int r = base.VisualChildrenCount;

				if ( this.CaretVisible )
					r++;

				return r;
			}
		}

		private CaretElement _caretElement;

		internal CaretElement CaretElement
		{
			get
			{
				if ( null == _caretElement )
					_caretElement = new CaretElement( );

				return _caretElement;
			}
		}

		/// <summary>
		/// Gets the visual child at a specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the specific child visual.</param>
		/// <returns>The visual child at the specified index.</returns>
		protected override Visual GetVisualChild(int index)
		{
			if ( this.CaretVisible && 1 + index == this.VisualChildrenCount )
				return this.CaretElement;

			return base.GetVisualChild( index );
		}

		/// <summary>
		/// Invoked to measure the element and its children.
		/// </summary>
		/// <param name="availableSize">The size that reflects the available size that this element can give to its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> that represents the desired size of the element.</returns>
		protected override Size MeasureOverride(Size availableSize)
		{
			// JJD 4/27/07
			// Optimization - clear the cached s_defaultCultureInfo ;
			s_defaultCultureInfo = null;

			
			
			
			if ( !this.IsInEditMode )
			{
				if ( DependencyProperty.UnsetValue == this.ReadLocalValue( MinHeightProperty )
					&& DependencyProperty.UnsetValue == this.ReadLocalValue( HeightProperty ) )
				{
					// MD 8/12/10 - TFS26592
					// By default, the SimpleTextBlock will now be used in the XamMaskedEditor, so try to set the MinHeight on that first. 
					// If it is not there, try to set it on the TextBlock instead.
					//TextBlock textBlock = (TextBlock)Utilities.GetDescendantFromType( this, typeof( TextBlock ), true );
					//if ( null != textBlock )
					//    textBlock.MinHeight = Utils.GetLineHeight( this );
					SimpleTextBlock simpleTextBlock = (SimpleTextBlock)Utilities.GetDescendantFromType(this, typeof(SimpleTextBlock), true);
					if (null != simpleTextBlock)
					{
						simpleTextBlock.MinHeight = Utils.GetLineHeight(this);
					}
					else
					{
						TextBlock textBlock = (TextBlock)Utilities.GetDescendantFromType(this, typeof(TextBlock), true);
						if (null != textBlock)
							textBlock.MinHeight = Utils.GetLineHeight(this);
					}
				}
			}

			Size retSize = base.MeasureOverride( availableSize );

			if ( this.CaretVisible )
			{
				DisplayCharBase dc = this.EditInfo.GetDisplayCharAtPosition( this.CaretPosition );
				if ( null == dc )
					dc = this.EditInfo.LastDisplayChar;

				DisplayCharacterPresenter dcPresenter = this.GetDisplayCharacterPresenter( dc );

				if ( null != dcPresenter )
				{
					Size size = dcPresenter.DesiredSize;

					
					
					
					
					
					
					size.Width = double.PositiveInfinity;

					this.CaretElement.Measure( size );
				}
			}

			return retSize;
		}

		/// <summary>
		/// Positions child elements and determines a size for this element.
		/// </summary>
		/// <param name="finalSize">The size available to this element for arranging its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> used by this element to arrange its children.</returns>
		protected override Size ArrangeOverride(Size finalSize)
		{
			Size retSize = base.ArrangeOverride( finalSize );

			if ( this.CaretVisible )
			{
				bool before = true;
				DisplayCharBase dc = this.EditInfo.GetDisplayCharAtPosition( this.CaretPosition );
				if ( null == dc )
				{
					dc = this.EditInfo.LastDisplayChar;
					before = false;
				}

				DisplayCharacterPresenter dcPresenter = this.GetDisplayCharacterPresenter( dc );

				if ( null != dcPresenter )
				{
					Point point = new Point( 0, 0 );
					if ( !before )
						point.X += dcPresenter.ActualWidth;

					point = dcPresenter.TranslatePoint( point, this );

					Size size = this.CaretElement.DesiredSize;
					size.Height = dcPresenter.ActualHeight;

					Rect rect = new Rect( point, size );

                    // SSP 5/13/10 TFS30701
                    // Previously we were setting the ClipToBounds to true in the template of the XamMaskedEditor.
                    // We took that out to make the UIElement.Effect work.
                    // 
                    double maxHeight = finalSize.Height - rect.Top;
                    if ( rect.Height > maxHeight )
                        rect.Height = Math.Max( 0, maxHeight );

					this.CaretElement.Arrange( rect );
				}
			}

			return retSize;
		}

		internal Rect GetDisplayCharLocation( DisplayCharBase dc )
		{
			DisplayCharacterPresenter dcPresenter = this.GetDisplayCharacterPresenter( dc );

			if ( null != dcPresenter )
			{
				Point point = dcPresenter.TranslatePoint( new Point( 0, 0 ), this );
				Size size = new Size( dcPresenter.ActualWidth, dcPresenter.ActualHeight );

				return new Rect( point, size );
			}

			return Rect.Empty;
		}

		internal DisplayCharacterPresenter GetDisplayCharacterPresenter( DisplayCharBase dc )
		{
			Utilities.DependencyObjectSearchCallback<DisplayCharacterPresenter> searchCallback = 
				new Utilities.DependencyObjectSearchCallback<DisplayCharacterPresenter>(
					delegate( DisplayCharacterPresenter presenter )
					{
						return dc == presenter.DisplayCharacter;
					} );

			DisplayCharacterPresenter dcPresenter = Utilities.GetDescendantFromType( this, true, searchCallback );

			return dcPresenter;
		}

		internal IList GetDisplayCharacterPresenters( )
		{
			ArrayList list = new ArrayList( );

			if ( this.IsInEditMode && null != this.MaskInfo )
			{
				foreach ( DisplayCharBase dc in this.MaskInfo.Sections.AllDisplayCharacters )
				{
					DisplayCharacterPresenter dcPresenter = this.GetDisplayCharacterPresenter( dc );
					if ( null != dcPresenter )
						list.Add( dcPresenter );
				}
			}

			return list;
		}

		#region IsMouseOverTemplateChild

		// SSP 10/14/09 - NAS10.1 Spin Buttons
		// 
		internal bool IsMouseOverTemplateChild( string templateChildName, MouseEventArgs mouseArgs )
		{
			DependencyObject tmp;
			return this.IsMouseOverTemplateChild( templateChildName, mouseArgs, out tmp );
		}

		internal bool IsMouseOverTemplateChild( string templateChildName, MouseEventArgs mouseArgs, out DependencyObject templateChildElem )
		{
			templateChildElem = this.GetTemplateChild( templateChildName );
			if ( null != templateChildElem && Utils.IsMouseOverElement( templateChildElem, mouseArgs ) )
				return true;

			return false;
		}

		#endregion // IsMouseOverTemplateChild

		internal void SyncDrawAsSelectedOnDisplayPresenters( )
		{
			if ( this.IsInEditMode && null != this.EditInfo )
			{
				foreach ( DisplayCharacterPresenter dcPresenter in this.GetDisplayCharacterPresenters( ) )
					dcPresenter.SyncDrawAsSelected( );
			}
		}

		// JJD 4/25/07
		// Optimization - don't create a spearate context menu for every editor
		// Instead override OnContextMenuOpening
		/// <summary>
		/// Called when the ContextMenu is about to open
		/// </summary>
		/// <param name="e">The event arguments</param>
		protected override void OnContextMenuOpening(ContextMenuEventArgs e)
		{
			if (this.IsInEditMode == false)
				return;

			ContextMenu menu = this.ContextMenu;

			if (this.ReadLocalValue(ContextMenuProperty) != null)
			{
				if (menu != null)
				{
					menu.Closed += new RoutedEventHandler(OnContextMenuClosed);
				}
				else
				{
					// SSP 10/16/07 BR27125
					// When the mouse is right-clicked on the label portion of the editor in the ribbon,
					// do not show the context menu. In that case the ribbon should show its own context
					// menu for adding/removing from qat.
					// 
					// ------------------------------------------------------------------------------------
					UIElement originalSourceAsElem = e.OriginalSource as UIElement;
					FrameworkElement editArea = this.FocusSite as FrameworkElement;
					if ( null != originalSourceAsElem && null != editArea )
					{
						Rect editAreaRect = new Rect( 0, 0, editArea.ActualWidth, editArea.ActualHeight );
						Point mouseLoc = originalSourceAsElem.TranslatePoint( new Point( e.CursorLeft, e.CursorTop ), editArea );
						if ( ! editAreaRect.Contains( mouseLoc ) )
							return;
					}
					// ------------------------------------------------------------------------------------

					menu = this.CreateContextMenu();
					menu.Closed += new RoutedEventHandler(OnContextMenuClosed);
					menu.PlacementTarget = this;
					menu.Placement = PlacementMode.RelativePoint;

					Point pt = new Point();

					if ( e.OriginalSource is UIElement )
						pt = this.TranslatePoint( pt, e.OriginalSource as UIElement );

					menu.HorizontalOffset = e.CursorLeft - pt.X;
					menu.VerticalOffset = e.CursorTop - pt.Y;
					menu.IsOpen = true;
					e.Handled = true;

					// SSP 7/19/07 BR22754
					// We need to appear as if we have keyboard focus when we are displaying context menu.
					// 
					_isDisplayingContextMenu = menu.IsOpen;
					this.ProcessIsBeingEditedAndFocusedChanged( );
				}
			}
		}

		private void OnContextMenuClosed(object sender, RoutedEventArgs e)
		{
			ContextMenu menu = sender as ContextMenu;

			// SSP 7/19/07 BR22754
			// We need to appear as if we have keyboard focus when we are displaying context menu.
			// 
			_isDisplayingContextMenu = false;
			this.ProcessIsBeingEditedAndFocusedChanged( );

			if ( menu != null )
				menu.Closed -= new RoutedEventHandler(OnContextMenuClosed);
		}

		/// <summary>
		/// Overridden.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnMouseDoubleClick( MouseButtonEventArgs e )
		{
			base.OnMouseDoubleClick( e );

            // AS 9/5/08 NA 2008 Vol 2
            // The mouse could be over the dropdown.
            //
            //if ( null != this.EditInfo )
			if ( null != this.EditInfo && Utils.IsMouseOverElement(this, e)
				// SSP 10/2/09 - NAS10.1 Spin Buttons
				// Also don't select when the mouse is double clicked over a button in the editor.
				// 
				&& ! Utils.IsMouseOverButton( this, e )
				)
			{
				this.EditInfo.SelectAll( );

				e.Handled = true;
			}
		}

		/// <summary>
		/// Called when the left mouse button is pressed
		/// </summary>
		/// <param name="e">The event arguments</param>
		protected override void OnMouseLeftButtonDown( MouseButtonEventArgs e )
		{
			// SSP 10/17/07 UWG2076 BR27228
			// When going into edit mode via mouse click, position the caret at the character that's 
			// clicked upon. See the comment over where the _transitingIntoEditModeInMouseDown member
			// var is declared for more info. Also we cannot do this when there's format since when
			// the text is formatted, we don't know which character in the formatted text maps to 
			// which display character. In that case, simply position the caret at the beginning of
			// the content.
			// 
			// ----------------------------------------------------------------------------------------
			// SSP 10/14/09 - NAS10.1 Spin Buttons
			// 
			bool wasOverSpinButton = false;

			bool prevIsInEditMode = this.IsInEditMode;
			if ( !prevIsInEditMode )
			{
				_transitingIntoEditModeInMouseDown = true;
				_transitingIntoEditModeInMouseDownPoint = e.GetPosition( this );

				// SSP 10/14/09 - NAS10.1 Spin Buttons
				// 
				wasOverSpinButton = this.IsMouseOverTemplateChild( "PART_SpinButtons", e );
			}

			try
			{
				base.OnMouseLeftButtonDown( e );

				// If we were previously in edit mode and we are still in edit mode then capture the mouse
				// for drag selection. If we weren't in edit mode previously and the base.OnMouseLeftButtonDown 
				// caused us to go into edit mode then the OnEditModeStarted method will be the one that will
				// position the caret at appropriate location and capture the mouse if necessary. Therefore
				// in that case we shouldn't do it here. That's the reason for checking prevIsInEditMode.
				// 
				// SSP 10/9/08 BR33762
				// If a cell in the grid is clicked initially and the editor enters edit mode via that click
				// then we shouldn't set the caret at the click location. By default we select all text. This
				// also allows you to set SelectionStart and SelectionLength in EditModeStarted event handler.
				// 
                // AS 9/5/08 NA 2008 Vol 2
                //if ( prevIsInEditMode && this.IsInEditMode )
				//if ( prevIsInEditMode && this.IsInEditMode && Utils.IsMouseOverElement(this, e))
				if ( prevIsInEditMode && this.IsInEditMode && Utils.IsMouseOverElement(this, e)
					&& ( null == this.Host || this.Host._previewMouseLeftButtonDown_EnterEditMode_TimeStamp != e.Timestamp ) )
				{
					EditInfo editInfo = this.EditInfo;
					if ( null != editInfo )
					{
						editInfo.MaskedEditUIElementClicked( e.GetPosition( this ) );

						this.CaptureMouse( );

						// SSP 10/1/07 - XamRibbon
						// XamRibbon needs to keep the capture on the editor even after the mouse is released.
						// Therefore we need to keep a flag to indicate when we are in drag selection mode
						// and not just rely on the editor having mouse capture.
						// 
						if ( this.IsMouseCaptured )
							_isDragSelecting = true;
					}
				}

				// SSP 10/14/09 - NAS10.1 Spin Buttons
				// 
				// ------------------------------------------------------------------------
				DependencyObject spinButtonsElem;
				if ( wasOverSpinButton && this.IsMouseOverTemplateChild( "PART_SpinButtons", e, out spinButtonsElem ) )
				{
					ButtonBase button = Utils.HitTest( this, e.GetPosition( this ), typeof( ButtonBase ), true ) as ButtonBase;
					if ( null != button )
						Utils.SendLeftMouseButtonDownHelper( button, e );
				}
				// ------------------------------------------------------------------------
			}
			finally
			{
				if ( _transitingIntoEditModeInMouseDown )
				{
					_transitingIntoEditModeInMouseDown = false;
					this.ProcessIsBeingEditedAndFocusedChanged( );
				}
			}

			
#region Infragistics Source Cleanup (Region)





















#endregion // Infragistics Source Cleanup (Region)

			// ----------------------------------------------------------------------------------------
		}

		/// <summary>
		/// Overridden.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnMouseUp( MouseButtonEventArgs e )
		{
			base.OnMouseUp( e );

			// Release the mouse capture.
			// 
			if ( null != e.Device && null != e.Device.Target )
			{
				// SSP 10/1/07 - XamRibbon
				// XamRibbon needs to keep the capture on the editor even after the mouse is released.
				// Therefore we need to keep a flag to indicate when we are in drag selection mode
				// and not just rely on the editor having mouse capture.
				// 
				_isDragSelecting = false;

				e.Device.Target.ReleaseMouseCapture( );
			}
		}

		/// <summary>
		/// Overridden.
		/// </summary>
		/// <param name="e">Event args that contains more information about the event.</param>
		protected override void OnLostMouseCapture( MouseEventArgs e )
		{
			// SSP 10/1/07 - XamRibbon
			// XamRibbon needs to keep the capture on the editor even after the mouse is released.
			// Therefore we need to keep a flag to indicate when we are in drag selection mode
			// and not just rely on the editor having mouse capture.
			// 
			_isDragSelecting = false;

			base.OnLostMouseCapture( e );
		}

		/// <summary>
		/// Overridden.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnMouseLeftButtonUp( MouseButtonEventArgs e )
		{
			base.OnMouseLeftButtonUp( e );

            if ( null != this.EditInfo )
			{
				this.EditInfo.MaskedEditUIElementMouseUp( e.GetPosition( this ) );

				e.Handled = true;
			}
		}

		/// <summary>
		/// Overridden.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnMouseMove( MouseEventArgs e )
		{
			base.OnMouseMove( e );

			// SSP 10/1/07 - XamRibbon
			// XamRibbon needs to keep the capture on the editor even after the mouse is released.
			// Therefore we need to keep a flag to indicate when we are in drag selection mode
			// and not just rely on the editor having mouse capture.
			// Added _isDragSelecting condition.
			// 
			
			if ( _isDragSelecting && null != this.EditInfo && null != e.Device 
				&& null != e.Device.Target && e.Device.Target.IsMouseCaptured )
			{
				this.EditInfo.MouseDragged( e.GetPosition( this ) );

				e.Handled = true;
			}
		}

		#endregion // Caret Logic

		// AS 2/25/11 TFS67071
		#region ContextMenuHelper
		private class ContextMenuHelper
		{
			#region Member Variables

			private XamMaskedEditor _editor;
			private ContextMenu _menu;

			#endregion //Member Variables

			#region Constructor
			internal ContextMenuHelper(XamMaskedEditor editor, ContextMenu menu)
			{
				_menu = menu;
				_editor = editor;

				menu.Opened += new RoutedEventHandler(OnMenuOpened);
				menu.Closed += new RoutedEventHandler(OnMenuClosed);
			}
			#endregion //Constructor

			#region Methods
			private void OnMenuClosed(object sender, RoutedEventArgs e)
			{
				_menu.Items.Clear();
			}

			private void OnMenuOpened(object sender, RoutedEventArgs e)
			{
				_menu.Items.Clear();
				XamMaskedEditor.InitializeContextMenu(_editor, _menu);
			}
			#endregion //Methods
		}
		#endregion //ContextMenuHelper
	}

	#endregion // XamMaskedEditor Class

	#region MaskedEditorDefaultConverter Class

	// MD 4/25/11 - TFS73181
	// Made this class sealed because it doesn't have any derived classes and there is code in the TryFastSyncOnValueChanged
	// method which does some "is" checks against this type and performs the conversion logic in a faster way if they pass.
	// If derived classes are needed, it is safe to remove the sealed keyword here, but update the logic in 
	// TryFastSyncOnValueChanged accordingly to make sure it still works correctly (such as by doing a GetType() check instead 
	// of an "is" check).
	//internal class MaskedEditorDefaultConverter : ValueEditorDefaultConverter
	internal sealed class MaskedEditorDefaultConverter : ValueEditorDefaultConverter
	{
		private static MaskedEditorDefaultConverter _valueToDisplayTextConverter;
		private static MaskedEditorDefaultConverter _valueToTextConverter;

		// MD 4/25/11 - TFS73181
		// Now that the class is sealed, the constructor can be private instead of protected.
		//protected MaskedEditorDefaultConverter( bool isDisplayTextConverter ) : base( isDisplayTextConverter )
		private MaskedEditorDefaultConverter( bool isDisplayTextConverter ) : base( isDisplayTextConverter )
		{
		}

		public new static MaskedEditorDefaultConverter ValueToTextConverter
		{
			get
			{
				if ( null == _valueToTextConverter )
					_valueToTextConverter = new MaskedEditorDefaultConverter( false );

				return _valueToTextConverter;
			}
		}

		public new static MaskedEditorDefaultConverter ValueToDisplayTextConverter
		{
			get
			{
				if ( null == _valueToDisplayTextConverter )
					_valueToDisplayTextConverter = new MaskedEditorDefaultConverter( true );

				return _valueToDisplayTextConverter;
			}
		}

        #region ConvertToTextBasedOnMask

        
        
        
        
        internal string ConvertToTextBasedOnMask( XamMaskedEditor editor, object value, bool useSectionsDirectly )
        {
            MaskInfo maskInfo = MaskInfo.CreateMaskInfoForConverter( editor );
            SectionsCollection sections = maskInfo.Sections;

            Debug.Assert( null != sections );
            if ( null == sections )
                return null;

            try
            {
                bool valueApplied = false;
                if ( useSectionsDirectly )
                {
                    bool hasDateSections, hasTimeSections;
                    Type maskType = XamMaskedEditor.DeduceEditAsType( sections, out hasDateSections, out hasTimeSections );
                    bool hasNonDateTimeEditSections = XamMaskedEditor.HasNonDateTimeEditSections( sections );

                    if ( value is DateTime && typeof( DateTime ) == maskType && !hasNonDateTimeEditSections )
                    {
                        XamMaskedEditor.SetDateTimeValue( sections, (DateTime)value, maskInfo, false );
                        valueApplied = true;
                    }
                }

                if ( ! valueApplied )
                    maskInfo.InternalRefreshValue( value );

                bool contentsEmpty = XamMaskedEditor.AreAllDisplayCharsEmpty( sections );
                if ( contentsEmpty )
                    return _isDisplayTextConverter ? editor.NullText : string.Empty;
                else
                    return XamMaskedEditor.GetText( sections, _isDisplayTextConverter ? maskInfo.DisplayMode : maskInfo.DataMode, maskInfo );
            }
            finally
            {
                // SSP 5/19/08 BR33116
                // Release the mask info to have its sections collection release reference to the 
                // editor to prevent memory leak since the sections are statically cached.
                // 
                MaskInfo.ReleaseMaskInfoForConverter( maskInfo );
            }
        }

        #endregion // ConvertToTextBasedOnMask

		protected override object ConvertHelper( bool convertingBack, object value, Type targetType, 
			ValueEditor valueEditor, IFormatProvider formatProvider, string format )
		{
			// SSP 8/20/08 BR35749
			// Moved handling of null into new virtual ConvertHelper virtual method so
			// derived editors have control over it. Let the base implementation handle
			// null values as it was doing before this change. Enclosed the existing code
			// into the if block.
			// 
			if ( null != value && DBNull.Value != value )
			{
				XamMaskedEditor editor = valueEditor as XamMaskedEditor;
				Debug.Assert( null != editor );
				if ( null != editor )
				{
					if ( convertingBack )
					{
						// Converting back from display text to value.

						if ( editor.IsInEditMode && !editor._isEnteringEditMode && object.Equals( value, editor.Text ) && null != editor.EditInfo
							// SSP 10/24/08 TFS9442
							// Only get the value from sections if this call resulted from change the sections.
							// Otherwise this call could have resulted from set of Text property directly.
							// 
							&& editor.EditInfo._inOnTextChanged )
						{
							if ( XamMaskedEditor.AreAllDisplayCharsEmpty( editor.EditInfo.Sections ) )
								// SSP 1/13/12 TFS99243
								// We should always use null otherwise binding will not work as it cannot convert
								// DBNull to a DateTime? value of null for example.
								// 
								//return DBNull.Value;
								return null;

							return editor.EditInfo.Value;
						}
						else
						{
							// AS 5/1/09 NA 2009 Vol 2 - Clipboard Support
							if (_isDisplayTextConverter && object.Equals(value, editor.NullText))
								// SSP 1/13/12 TFS99243
								// We should always use null otherwise binding will not work as it cannot convert
								// DBNull to a DateTime? value of null for example.
								// 
								//return DBNull.Value;
								return null;

							// AS 8/25/09 TFS21354
							// Since we don't use the mask when converting to display text if there is a format
							// we should not try to use the mask to parse the value if we have a format and this 
							// is a displaytext to value conversion.
							//
							if ( null == format || !_isDisplayTextConverter )
							{
								MaskInfo maskInfo = MaskInfo.CreateMaskInfoForConverter( editor );
								try
								{
									// SSP 5/19/09 - Clipboard Support
									// If the value doesn't match the mask, then we need to return null to 
									// indicate failure.
									// 
									// ----------------------------------------------------------------------------
									//XamMaskedEditor.SetText( maskInfo.Sections, null != value ? value.ToString( ) : string.Empty, maskInfo );
									string valueAsString = null != value ? value.ToString( ) : string.Empty;
									int machedChars = XamMaskedEditor.SetText( maskInfo.Sections, valueAsString, maskInfo );
									if ( machedChars < valueAsString.Length )
										return null;
									// ----------------------------------------------------------------------------

									if ( XamMaskedEditor.AreAllDisplayCharsEmpty( maskInfo.Sections ) )
										// SSP 1/13/12 TFS99243
										// We should always use null otherwise binding will not work as it cannot convert
										// DBNull to a DateTime? value of null for example.
										// 
										//return DBNull.Value;
										return null;

									return XamMaskedEditor.GetDataValue( maskInfo );
								}
								finally
								{
									// SSP 5/19/08 BR33116
									// Release the mask info to have its sections collection release reference to the 
									// editor to prevent memory leak since the sections are statically cached.
									// 
									MaskInfo.ReleaseMaskInfoForConverter( maskInfo );
								}
							}
						}
					}
					else if ( typeof( string ) == targetType )
					{
						// If format is specified then don't use masking.
						// 
						if ( null == format || !_isDisplayTextConverter )
						{
                            
                            
                            
                            return this.ConvertToTextBasedOnMask( editor, value, false );
						}
					}
				}
			}

			return base.ConvertHelper( convertingBack, value, targetType, valueEditor, formatProvider, format );
		}
	}

	#endregion // MaskedEditorDefaultConverter Class
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