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
using System.Windows.Threading;
using System.Threading;
using Infragistics.Collections;
using Infragistics.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using Infragistics.Controls.Editors.Primitives;

namespace Infragistics.Controls.Editors
{
	#region XamMaskedInput Class

	/// <summary>
	/// Value editor for displaying and editing data based on a mask.
	/// </summary>
	/// <remarks>
	/// <p class="body">
	/// <b>XamMaskedInput</b> is an editor control that lets you display and edit values based on a mask. The mask is 
	/// specified via its <see cref="XamMaskedInput.Mask"/> property. If a mask is not explicitly set then a default mask 
	/// is used based on the <see cref="ValueInput.ValueType"/> property. The default mask is determined using the masks 
	/// that are registered for specific types using <see cref="RegisterDefaultMaskForType(Type,string)"/>.</p>
	/// <seealso cref="ValueInput"/>
	/// <seealso cref="TextInputBase"/>
	/// </remarks>
	[System.Windows.Markup.ContentProperty( "Value" )]
	// SSP 3/23/09 IME
	// Added PART_InputTextBox TemplatePart.
	// 
	[TemplatePart( Name = "PART_InputTextBox", Type = typeof( MaskedInputTextBox ) )]
	// SSP 10/14/09 - NAS10.1 Spin Buttons
	// 
	[TemplatePart( Name = "PART_SpinButtons", Type = typeof( FrameworkElement ) )]
	[StyleTypedProperty( Property = "SpinButtonStyle", StyleTargetType = typeof( RepeatButton ) )]

	
	

	public class XamMaskedInput : TextInputBase, ICommandTarget
	{
		#region static constants

		internal const char DEFAULT_PROMPT_CHAR = '_';
		internal const char DEFAULT_PAD_CHAR = ' ';

		// SSP 12/18/02 UWE342
		// Added a way for the user to specify a mask in which we will translate certain
		// characters to underlying locale characters.
		//
		internal const string LOCALIZED_ESCAPE_SEQUENCE = "{LOC}";

		#endregion //static constants

		#region Variables


		private UltraLicense _license;


		private EditInfo _editInfo;
		private MaskInfo _maskInfo;

		// Control that was last hooked into the GotFocus and LostFocus events
		//
		//private Control			lastHookedControl = null;
		private SectionsCollection _displayChars_Sections;
		private DisplayCharsCollection _displayChars;

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

		private static Dictionary<Type, string> g_defaultMaskTable = new Dictionary<Type, string>( );
		private static bool g_defaultMaskTableInitialized = false;

		// SSP 11/18/04 BR00499
		// Added AllowShiftingAcrossSections property.
		//
		private bool _allowShiftingAcrossSections = true;

		// SSP 10/8/09 - NAS10.1 Spin Buttons
		// 
		internal SpinInfo _cachedSpinInfo;

		// SSP 8/30/11 TFS76307
		// 
		private bool _cachedTrimFractionalZeros;

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
		public event EventHandler<InvalidCharEventArgs> InvalidChar;

		#endregion

		#region Constructors

		static XamMaskedInput( )
		{

			//This OverrideMetadata call tells the system that this element wants to provide a style that is different than its base class.
			//This style is defined in themes\generic.xaml
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata( typeof( XamMaskedInput ), DependencyPropertyUtilities.CreateMetadata( typeof( XamMaskedInput ) ) );

			// SSP 6/6/07 BR23366
			// We need this in order to make the Tab and Shift+Tab navigation work properly.
			// This is similar to what inbox ComboBox does.
			// 
			KeyboardNavigation.TabNavigationProperty.OverrideMetadata( typeof( XamMaskedInput ), DependencyPropertyUtilities.CreateMetadata( KeyboardNavigationMode.Local ) );
			KeyboardNavigation.ControlTabNavigationProperty.OverrideMetadata( typeof( XamMaskedInput ), DependencyPropertyUtilities.CreateMetadata( KeyboardNavigationMode.None ) );
			KeyboardNavigation.DirectionalNavigationProperty.OverrideMetadata( typeof( XamMaskedInput ), DependencyPropertyUtilities.CreateMetadata( KeyboardNavigationMode.None ) );

			Style style = new Style( );
			style.Seal( );
			FocusVisualStyleProperty.OverrideMetadata( typeof( XamMaskedInput ), new FrameworkPropertyMetadata( style ) );

		}

		/// <summary>
		/// Initializes a new <see cref="XamMaskedInput"/>
		/// </summary>
		public XamMaskedInput( )
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
			// AS 11/7/07 BR21903
			// Always do the license checks.
			//
			//if (DesignerProperties.GetIsInDesignMode(this))
			{
				try
				{
					// We need to pass our type into the method since we do not want to pass in 
					// the derived type.
					this._license = LicenseManager.Validate(typeof(XamMaskedInput), this) as UltraLicense;
				}
				catch ( System.IO.FileNotFoundException ) { }
			}


			
			
			
			
			
			this.InitializeCachedPropertyValues( );

			_maskInfo = new MaskInfo( this );
			_editInfo = new EditInfo( _maskInfo );

			// Set the Sections property to the mask info's sections. MaskInfo only sets the masked editor's
			// sections if it's the same as the editor's MaskInfo. This happens in the constructor of the
			// MaskInfo and therefore _maskInfo is not set yet and therefore the MaskInfo's check to see if
			// it's the mask info of the editor fails.
			// 
			this.InternalSetSections( _maskInfo.Sections );






			// SSP 6/22/12 TFS115350
			// 
			CommandSourceManager.RegisterCommandTarget( this );
		}

		#endregion // Constructors

		#region Embedding code

		#region Public Properties

		#endregion //Public Properties

		#region Public Methods

		#region CanEditType

		/// <summary>
		/// Overridden. Determines if the editor natively supports editing values of specified type.
		/// </summary>
		/// <param name="type">The data type to check.</param>
		/// <returns>Returns True if the editor natively supports editing values of specified type, False otherwise.</returns>
		/// <remarks>
		/// <p class="body">
		/// XamMaskedInput's implementation returns True for the string, sbyte, byte, 
		/// short, ushort, int, uint, long, ulong, float, double, decimal and DateTime data types.
		/// </p>
		/// <p class="body">
		/// For these data types the editor will calculate a default mask. For any other
		/// data type, you should specify a mask that makes sense for the data type other wise
		/// a default mask will be used. You can change/register default masks for these and other
		/// data types using the <see cref="XamMaskedInput.RegisterDefaultMaskForType"/> method.
		/// </p>
		/// <p class="body">
		/// See ValueInput's <see cref="ValueInput.CanEditType"/> for more information.
		/// </p>
		/// </remarks>
		/// <seealso cref="ValueType"/>
		/// <seealso cref="Mask"/>
		/// <seealso cref="ValueInput.CanEditType(Type)"/>
		public override bool CanEditType( System.Type type )
		{
			return XamMaskedInput.SupportsDataType( type );
		}

		#endregion //CanEditType

		#region CanRenderType

		/// <summary>
		/// Overridden. Determines if the editor natively supports displaying of values of specified type.
		/// </summary>
		/// <param name="type">The data type to check.</param>
		/// <returns>Returns True if the editor natively supports displaying values of specified type, False otherwise.</returns>
		/// <remarks>
		/// <p class="body">
		/// See ValueInput's <see cref="ValueInput.CanRenderType"/> for more information.
		/// </p>
		/// </remarks>
		/// <seealso cref="CanEditType(Type)"/>
		/// <seealso cref="ValueInput.CanRenderType(Type)"/>
		public override bool CanRenderType( System.Type type )
		{
			return this.CanEditType( type );
		}

		#endregion //CanRenderType

		#endregion //Public Methods

		#region Protected Methods

		#region OnCreateAutomationPeer
		/// <summary>
		/// Returns an automation peer that exposes the <see cref="XamMaskedInput"/> to UI Automation.
		/// </summary>
		/// <returns>A <see cref="Infragistics.AutomationPeers.XamMaskedInputAutomationPeer"/></returns>
		protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer( )
		{
			return new Infragistics.AutomationPeers.XamMaskedInputAutomationPeer(this);
		}
		#endregion //OnCreateAutomationPeer

		#region DoInitialization

		/// <summary>
		/// Overridden. Called from OnInitialized to provide the derived classes an opportunity to 
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

		#region GetFocusSite

		/// <summary>
		/// Gets the focus site of the editor.
		/// </summary>
		/// <returns>FrameworkElement in the template of the editor that is to receive focus.</returns>
		protected override FrameworkElement GetFocusSite( )
		{
			return Utils.GetDescendantFromName( this, "PART_InputTextBox" );
		}

		#endregion // GetFocusSite

		#region OnCoerceText

		/// <summary>
		/// Overridden. Called from the <see cref="ValueInput.Text"/> property's CoerceValue handler.
		/// </summary>
		/// <param name="text">The text to coerce</param>
		/// <returns>Coerced value</returns>
		/// <remarks>
		/// <para class="body">
		/// XamMaskedInput's implementation applies mask to the <paramref name="text"/>.
		/// </para>
		/// </remarks>
		protected override string OnCoerceText( string text )
		{
			text = base.OnCoerceText( text );

			// Apply the mask to the set text. Only do so if the Text is not being set by us
			// to an already mask-coerced value.
			// 
			if ( !this.SyncingValueProperties
				&& ( null == this._editInfo || !this._editInfo._inOnTextChanged ) )
			{
				if ( null != text && text.Length > 0 )
				{
					MaskInfo maskInfo = MaskInfo.CreateMaskInfoForConverter( this );
					Debug.Assert( null != maskInfo );
					if ( null != maskInfo )
					{
						try
						{
							XamMaskedInput.SetText( maskInfo.Sections, text, maskInfo );
							text = XamMaskedInput.GetText( maskInfo.Sections, maskInfo.DataMode, maskInfo );
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

		#region ProcessPropertyChanged

		internal override void ProcessPropertyChanged( DependencyPropertyChangedEventArgs e )
		{
			base.ProcessPropertyChanged( e );

			DependencyProperty prop = e.Property;

			if ( ValueInput.FormatProviderProperty == prop
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
			else if ( ValueInput.FormatProperty == prop )
			{
				if ( null != _maskInfo )
					_maskInfo.Format = e.NewValue as string;
			}

			// SSP 10/5/09 - NAS10.1 Spin Buttons
			// 
			if ( ValueInput.IsMouseOverProperty == prop
				|| XamMaskedInput.SpinButtonDisplayModeProperty == prop
				|| ValueInput.IsReadOnlyProperty == prop )
			{
				this.UpdateSpinButtonVisibility( );
			}
		}

		#endregion // ProcessPropertyChanged

		#region ValidateCurrentValue

		/// <summary>
		/// Validates the current value of the editor. This method is called by the editor to perform
		/// editor specific validation of the current value.
		/// </summary>
		/// <returns>True if the value is valid, False otherwise.</returns>
		/// <remarks>
		/// See ValueInput's <see cref="ValueInput.ValidateCurrentValue(out Exception)"/> for more information.
		/// <seealso cref="ValueInput.IsValueValid"/>
		/// <seealso cref="ValueInput.InvalidValueBehavior"/>
		/// </remarks>
		protected override bool ValidateCurrentValue( out Exception error )
		{
			if ( !base.ValidateCurrentValue( out error ) )
				return false;

			MaskInfo maskInfo = this.MaskInfo;
			string errorMsg;
			if ( null != maskInfo && null != maskInfo.Sections
				&& !this.IsInputValid( maskInfo.Sections, out errorMsg ) )
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

		#region DefaultValueType

		/// <summary>
		/// Returns the default value type of the editor. When the <see cref="ValueType"/> property is not set, this is
		/// the type that the <see cref="ValueInput.ValueTypeResolved"/> will return.
		/// </summary>
		protected override Type DefaultValueType
		{
			get
			{
				return typeof( string );
			}
		}

		#endregion // DefaultValueType

		#region OnFocusSiteChanged

		
		
		
		/// <summary>
		/// Overridden. Called when the focus site changes.
		/// </summary>
		protected override void OnFocusSiteChanged( )
		{
			base.OnFocusSiteChanged( );

			EditInfo editInfo = this.EditInfo;
			if ( null != editInfo )
				editInfo.InitializeIMETextBox( this.FocusSite );
		}

		#endregion // OnFocusSiteChanged

		#region OnIsInEditModeChanged

		internal override void OnIsInEditModeChanged( bool isInEditMode )
		{
			base.OnIsInEditModeChanged( isInEditMode );

			if ( null != _editInfo && this.InternalIsInitialized )
			{
				// SSP 9/14/11 TFS76307
				// 
				if ( !isInEditMode )
					_editInfo.ValidateAllSections( true );

				_editInfo.SyncIMETextBox( true );

				this.UpdateSpinButtonVisibility( );
			}
		}

		#endregion // OnIsInEditModeChanged

		#region OnValueConstraintChanged

		
		
		
		/// <summary>
		/// This method is called whenever the ValueConstraint or one of its properties changes.
		/// </summary>
		/// <param name="valueConstraintPropertyName">Null if the ValueConstraint itself changed or 
		/// the name of the property that changed.</param>
		internal override void OnValueConstraintChanged( string valueConstraintPropertyName )
		{
			if ( null != _maskInfo )
			{
				_maskInfo.InitializedCachedMinMaxValues( );

				// SSP 6/29/12 TFS115883
				// Change in value constraints can potentially affect the current state.
				// 
				if ( null != _editInfo )
					_editInfo.NotifyStateChanged( );
			}

			base.OnValueConstraintChanged( valueConstraintPropertyName );
		}

		#endregion // OnValueConstraintChanged

		#region ProcessKeyDown

		/// <summary>
		/// Overridden. Processes the key down event. It executes associated commands if any.
		/// </summary>
		/// <param name="args">Key event args.</param>
		internal protected override void ProcessKeyDown( KeyEventArgs args )
		{
			bool handled = false;
			Key key = args.Key;
			ModifierKeys modifierKeys = Keyboard.Modifiers;

			var matchingCommands = this.GetMatchingCommands( key, modifierKeys );
			if ( null != matchingCommands )
			{
				foreach ( var ii in matchingCommands )
				{
					this.ExecuteCommand( ii, null, this );
					handled = true;
				}
			}

			if ( handled )
				args.Handled = true;
			else
				base.ProcessKeyDown( args );
		}

		#endregion // ProcessKeyDown

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
		/// Implicit value constraints are <see cref="ValueInput.ValueType"/>,
		/// <see cref="XamMaskedInput.Mask"/> etc... and explicit constraints are specified
		/// via <see cref="ValueInput.ValueConstraint"/> property.
		/// </para>
		/// <seealso cref="ValueInput.IsValueValid"/>
		/// <seealso cref="ValueInput.InvalidValueBehavior"/>
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
					if ( !maskInfo.InternalRefreshValue( value, out error ) )
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
			_cachedClipMode = (InputMaskMode)DependencyPropertyUtilities.GetDefaultValue( this, ClipModeProperty );
			_cachedDataMode = (InputMaskMode)DependencyPropertyUtilities.GetDefaultValue( this, DataModeProperty );
			_cachedDisplayMode = (InputMaskMode)DependencyPropertyUtilities.GetDefaultValue( this, DisplayModeProperty );
			_cachedMask = (string)DependencyPropertyUtilities.GetDefaultValue( this, MaskProperty );
			_cachedPadChar = (char)DependencyPropertyUtilities.GetDefaultValue( this, PadCharProperty );
			_cachedPromptChar = (char)DependencyPropertyUtilities.GetDefaultValue( this, PromptCharProperty );

			// SSP 8/30/11 TFS76307
			// 
			_cachedTrimFractionalZeros = (bool)DependencyPropertyUtilities.GetDefaultValue( this, TrimFractionalZerosProperty );
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
			mask = null;

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

			mask = GetDefaultMaskForType( dataType );

			// If the dataType was supported and mask was returned, then
			// return true.
			//
			return null != mask && mask.Length > 0;
		}

		#endregion //ResolveMask

		#region GetNonstandardForeignDateMaskAndPostfixSymbols



#region Infragistics Source Cleanup (Region)










#endregion // Infragistics Source Cleanup (Region)

		internal static string GetNonstandardForeignDateMaskAndPostfixSymbols( IFormatProvider formatProv, ref Dictionary<Type, char> foreignDateSymbols )
		{
			foreignDateSymbols = null;
			string mask = null;

			DateTimeFormatInfo dateTimeFormatInfo = XamMaskedInput.GetDateTimeFormatInfo( formatProv );
			if ( dateTimeFormatInfo == null )
				return mask;

			string longDatePattern = dateTimeFormatInfo.LongDatePattern;
			if ( longDatePattern == null )
				return mask;

			int mIndexFirst = longDatePattern.IndexOfAny( new char[] { 'm', 'M' } );
			int dIndexFirst = longDatePattern.IndexOfAny( new char[] { 'd', 'D' } );
			int yIndexFirst = longDatePattern.IndexOfAny( new char[] { 'y', 'Y' } );

			int mIndexLast = longDatePattern.LastIndexOfAny( new char[] { 'm', 'M' } );
			int dIndexLast = longDatePattern.LastIndexOfAny( new char[] { 'd', 'D' } );
			int yIndexLast = longDatePattern.LastIndexOfAny( new char[] { 'y', 'Y' } );

			int mDiff = mIndexLast - mIndexFirst;
			int dDiff = dIndexLast - dIndexFirst;

			if ( mDiff < 2 && dDiff < 2 )
			{
				// Get the postfix symbols used by the current culture.
				char mSymbol = (char)0;
				if ( mIndexLast + 2 < longDatePattern.Length )
					mSymbol = longDatePattern[mIndexLast + 2];

				char dSymbol = (char)0;
				if ( dIndexLast + 2 < longDatePattern.Length )
					dSymbol = longDatePattern[dIndexLast + 2];

				char ySymbol = (char)0;
				if ( yIndexLast + 2 < longDatePattern.Length )
					ySymbol = longDatePattern[yIndexLast + 2];

				if ( mSymbol != (char)0 && dSymbol != (char)0 && ySymbol != (char)0 &&
					mIndexFirst != -1 && dIndexFirst != -1 && yIndexFirst != -1 )
				{
					// Fill the output parameter with the different symbols that will be used when making the mask.
					foreignDateSymbols = new Dictionary<Type, char>( 3 );
					foreignDateSymbols.Add( typeof( DaySection ), dSymbol );
					foreignDateSymbols.Add( typeof( MonthSection ), mSymbol );
					foreignDateSymbols.Add( typeof( YearSection ), ySymbol );

					int yCount = yIndexLast - yIndexFirst + 1;
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
			if ( twoYear < 100 )
				// AS 8/25/08 Support Calendar
				//return System.Globalization.CultureInfo.CurrentCulture.Calendar.ToFourDigitYear(twoYear);
				return calendar.ToFourDigitYear( twoYear );

			return twoYear;
		}

		#endregion // ConvertToFourYear

		#region GetMatchingCommands

		internal IEnumerable<MaskedInputCommandId> GetMatchingCommands( Key key, ModifierKeys modifierKeys )
		{
			return this.Commands.GetMatchingCommands( key, modifierKeys, this.EditInfo.GetCurrentStateLong );
		}

		#endregion // GetMatchingCommands

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
			NumberSection numberSection = (NumberSection)XamMaskedInput.GetSection( sections, sectionType );
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

			defDay = 1;
			defMonth = 1;
			// AS 8/25/08 Support Calendar
			//defYear = currDate.Year;
			defYear = cal.GetYear( currDate );
			defHour = 0;
			defMinute = 0;
			defSecond = 0;

			// SSP 2/25/04 UWG2962 UWE856
			// Retain the date portion when editing time and the same goes for time portion as 
			// well.
			//
			// ----------------------------------------------------------------------------------
			bool useCurrDateDefaults = true;
			MaskInfo maskInfo = sections.MaskInfo;
			if ( null != maskInfo && null != maskInfo.MaskedInput )
			{
				object originalValue = maskInfo.MaskedInput.OriginalValue;
				if ( null != originalValue )
					originalValue = CoreUtilities.ConvertDataValue( originalValue, typeof( DateTime ), maskInfo.FormatProvider, maskInfo.Format );

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
					if ( origDate >= cal.MinSupportedDateTime && origDate <= cal.MaxSupportedDateTime )
					{
						defDay = cal.GetDayOfMonth( origDate );
						defMonth = cal.GetMonth( origDate );
						defYear = cal.GetYear( origDate );
						defHour = cal.GetHour( origDate );
						defMinute = cal.GetMinute( origDate );
						defSecond = cal.GetSecond( origDate );
						useCurrDateDefaults = false;
					}
				}
			}
			// ----------------------------------------------------------------------------------		

			MonthSection monthSection = (MonthSection)XamMaskedInput.GetSection( sections, typeof( MonthSection ) );
			DaySection daySection = (DaySection)XamMaskedInput.GetSection( sections, typeof( DaySection ) );
			YearSection yearSection = (YearSection)XamMaskedInput.GetSection( sections, typeof( YearSection ) );
			HourSection hourSection = (HourSection)XamMaskedInput.GetSection( sections, typeof( HourSection ) );
			MinuteSection minuteSection = (MinuteSection)XamMaskedInput.GetSection( sections, typeof( MinuteSection ) );
			SecondSection secondSection = (SecondSection)XamMaskedInput.GetSection( sections, typeof( SecondSection ) );

			// SSP 2/25/04 UWG2962 UWE856
			// Related to the change above. 
			// Enclosed the existing code into the if block.
			//
			if ( useCurrDateDefaults )
			{
				// AS 8/25/08 Support Calendar
				//defDay   = null != hourSection ? currDate.Day : 1;
				//defMonth = null != hourSection ? currDate.Month : 1;
				defDay = null != hourSection ? cal.GetDayOfMonth( currDate ) : 1;
				defMonth = null != hourSection ? cal.GetMonth( currDate ) : 1;
			}

			// SSP 1/17/02
			// Added code for AM-PM section
			//
			AMPMSection ampmSection = (AMPMSection)XamMaskedInput.GetSection( sections, typeof( AMPMSection ) );

			// SSP 1/11/02
			// If none of the sections exist, then return null
			// 
			if ( null == monthSection && null == daySection &&
				null == yearSection && null == hourSection &&
				null == minuteSection && null == secondSection )
				throw new ArgumentException( XamMaskedInput.GetString( "LE_ArgumentException_4" ) );

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
						0 == Utils.CompareStrings( ampmSection.PMValue, ampmSection.GetText( InputMaskMode.IncludeLiterals ), true ) )
						hour += 12;
				}
				// SSP 11/26/02 UWM156
				// If the hour is 12 and it's AM, then make it 0 before passing it in the 
				// DateTime constructor.
				//
				else if ( 12 == hour )
				{
					if ( null != ampmSection.AMValue &&
						0 == Utils.CompareStrings( ampmSection.AMValue, ampmSection.GetText( InputMaskMode.IncludeLiterals ), true ) )
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
				int maxDays = cal.GetDaysInMonth( year, month );
				if ( defDay > maxDays )
					defDay = maxDays;
			}
			// ----------------------------------------------------------------------

			// AS 8/25/08 Support Calendar
			//date = new DateTime( 				
			date = cal.ToDateTime(
				year,
				null != monthSection ? monthSection.ToInt( ) : defMonth,
				null != daySection ? daySection.ToInt( ) : defDay,
				hour,
				null != minuteSection ? minuteSection.ToInt( ) : defMinute,
				null != secondSection ? secondSection.ToInt( ) : defSecond,
				0 );

			return date;
		}

		#endregion //GetDateTimeValue

		#region GetText



#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

		internal static string GetText(
			SectionsCollection sections,
			InputMaskMode maskMode,
			char promptChar,
			char padChar )
		{
			StringBuilder sb = new StringBuilder( 1 + XamMaskedInput.GetTotalNumberOfDisplayChars( sections ) );

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
						( (DigitChar)dc ).ShouldIncludeComma( maskMode ) && 0 != dc.Section.CommaChar )
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
			InputMaskMode maskMode,
			MaskInfo maskInfo )
		{
			return XamMaskedInput.GetText( sections, maskMode,
				null != maskInfo ? maskInfo.PromptChar : DEFAULT_PROMPT_CHAR,
				null != maskInfo ? maskInfo.PadChar : DEFAULT_PAD_CHAR );
		}



#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

		internal static string GetText(
			SectionsCollection sections,
			InputMaskMode maskMode,
			MaskInfo maskInfo,
			string nullText )
		{
			if ( AreAllDisplayCharsEmpty( sections ) )
				return nullText;

			return XamMaskedInput.GetText( sections, maskMode,
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
			return SetText( sections, text, promptCharacter, padCharacter, true );
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
			bool skipDigitSeparator )
		{
			if ( null == text )
				text = "";

			// First erase all the characters.
			//
			XamMaskedInput.EraseAllChars( sections );

			int textIndex = 0;

			// SSP 1/16/12 TFS98252
			// 
			MaskInfo maskInfo = sections.MaskInfo;
			InputMaskMode maskMode = null != maskInfo ? maskInfo.DataMode : InputMaskMode.IncludeLiterals;

			DisplayCharBase dc = GetDisplayCharAtPosition( sections, 0 );

			while ( null != dc && textIndex < text.Length )
			{
				char c = text[textIndex];

				if ( dc.MatchChar( c ) 
					// SSP 10/12/11 TFS89579
					// Since the end user cannot distinquish between an explicitly typed in prompt character 
					// and an empty character, we should treat prompt characters in values as characters that
					// are not set.
					// 
					&& promptCharacter != c
					)
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

					if ( !dc.IsEditable && ( InputMaskMode.Raw == maskMode || InputMaskMode.IncludePromptChars == maskMode ) )
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
			return XamMaskedInput.SetText( sections, text, maskInfo.PromptChar, maskInfo.PadChar,
				 maskInfo.SkipDigitSeparator );
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
			if ( null == sections || position < 0 )
				return null;

			int sectionsCount = sections.Count;

			int c = 0;
			for ( int i = 0; i < sectionsCount; i++ )
			{
				SectionBase section = sections[i];

				int displayCharsCount = section.DisplayChars.Count;

				if ( position < c + displayCharsCount )
					return section.DisplayChars[position - c];

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
			if ( null == sections )
				return 0;

			int sectionsCount = sections.Count;

			int c = 0;
			for ( int i = 0; i < sectionsCount; i++ )
			{
				SectionBase section = sections[i];

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
			string str = XamMaskedInput.GetText( sections, InputMaskMode.Raw, maskInfo );

			if ( null == str || 0 == str.Length )
				throw new InvalidOperationException( XamMaskedInput.GetString( "LE_InvalidOperationException_9" ) );

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

			for ( int i = 0; i < sections.Count; i++ )
			{
				// The section has to be either numeric, or a comma or a decimal

				SectionBase section = sections[i];

				if ( XamMaskedInput.IsSectionNumeric( section ) )
				{
					if ( !pastDecimalSeperator )
					{
						integerPart += section.GetText( InputMaskMode.Raw );
					}
					else
					{
						fractionPart = section.GetText( InputMaskMode.Raw );
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
						if ( !pastDecimalSeperator )
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
						IFormatProvider formatProvider = maskInfo != null ?
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

			string str = XamMaskedInput.GetText( sections, maskInfo.DataMode, maskInfo );

			return Double.Parse( str, maskInfo.FormatProvider );
		}

		#endregion //GetDoubleValue

		#region GetFloatValue

		private static float GetFloatValue(
			SectionsCollection sections,
			MaskInfo maskInfo )
		{
			double d = (double)XamMaskedInput.GetDoubleValue( sections, maskInfo );

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

			for ( int i = 0; i < sections.Count; i++ )
			{
				// The section has to be either numeric, or a comma or a decimal

				SectionBase section = sections[i];

				if ( XamMaskedInput.IsSectionNumeric( section ) )
				{
					if ( !pastDecimalSeperator )
					{
						integerPart += section.GetText( InputMaskMode.Raw );
					}
					else
					{
						fractionPart = section.GetText( InputMaskMode.Raw );
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
						if ( !pastDecimalSeperator )
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
						string decimalChar = maskInfo != null ?
							maskInfo.DecimalSeperatorChar.ToString( ) :
							System.Globalization.CultureInfo.CurrentCulture.NumberFormat.CurrencyDecimalSeparator.ToString( );

						IFormatProvider formatProvider = maskInfo != null ?
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

			string str = XamMaskedInput.GetText( sections, maskInfo.DataMode, maskInfo );

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
					return XamMaskedInput.GetDateTimeValue( sections );
				}
				else if ( typeof( double ) == dataType )
				{
					return XamMaskedInput.GetDoubleValue( sections, maskInfo );
				}
				else if ( typeof( float ) == dataType )
				{
					return XamMaskedInput.GetFloatValue( sections, maskInfo );
				}
				else if ( typeof( byte ) == dataType || typeof( sbyte ) == dataType ||
					typeof( short ) == dataType || typeof( ushort ) == dataType || typeof( Int16 ) == dataType ||
					typeof( int ) == dataType || typeof( uint ) == dataType || typeof( Int32 ) == dataType ||
					typeof( long ) == dataType || typeof( ulong ) == dataType || typeof( Int64 ) == dataType )
				{
					return XamMaskedInput.GetWholeNumberValue( sections, dataType, maskInfo );
				}
				else if ( typeof( decimal ) == dataType )
				{
					return XamMaskedInput.GetCurrencyValue( sections, maskInfo );
				}
				else if ( typeof( string ) == dataType )
				{
					return XamMaskedInput.GetText( sections, maskInfo.DataMode, maskInfo );
				}
				else
				{

					// AS 10/17/08 TFS8886
					// Try to use the typeconverter to create an instance of
					// the object from a string.
					//
					TypeConverter tc = TypeDescriptor.GetConverter( dataType );

					if ( tc.CanConvertFrom( typeof( string ) ) )
					{
						string text = XamMaskedInput.GetText( maskInfo.Sections, maskInfo.DataMode, maskInfo );
						return tc.ConvertFromString( null, maskInfo.CultureInfo, text );
					}


					Debug.Assert( !XamMaskedInput.SupportsDataType( dataType ), "A suported data type but can't recognize it." );
					throw new ArgumentException( XamMaskedInput.GetString( "LE_NotSupportedException_2", dataType.Name ), "dataType" );
				}
			}
			catch ( Exception e )
			{
				throw new ArgumentException( XamMaskedInput.GetString( "LE_ArgumentException_5" ), e );
			}
		}

		#endregion //GetDataValue

		#region GetSection

		internal static SectionBase GetSection( SectionsCollection sections, Type sectionType )
		{
			int temp = 0;
			return XamMaskedInput.GetSection( sections, sectionType, ref temp );
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
				Type type = sections[i].GetType( );

				// JJD 6/14/07
				// Use IsAssignableFrom instead of IsSubclassOf since that is 10x more efficient
				//if (type == sectionType || type.IsSubclassOf(sectionType))
				if ( sectionType.IsAssignableFrom( type ) )
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
			FractionPart fractionPart = (FractionPart)XamMaskedInput.GetSection( sections, typeof( FractionPart ) );
			if ( null != fractionPart )
			{
				// SSP 8/30/11 TFS76307
				// Added TrimFractionalZeros property. Also note that for FractionPartContinuous we always need to
				// pad because the input in FractionPartContinuous is from right to left where the input is
				// padded to left with 0's.
				// 
				//fractionPart.PadWithZero( );
				if ( fractionPart is FractionPartContinuous || !this.TrimFractionalZeros )
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
			HourSection hourSection = null;
			MinuteSection minuteSection = null;
			SecondSection secondSection = null;
			MonthSection monthSection = null;
			DaySection daySection = null;
			YearSection yearSection = null;
			AMPMSection ampmSection = null;

			// AS 8/25/08 Support Calendar
			Debug.Assert( null != maskInfo );
			System.Globalization.Calendar calendar = sections.Calendar;

			monthSection = (MonthSection)XamMaskedInput.GetSection( sections, typeof( MonthSection ) );
			daySection = (DaySection)XamMaskedInput.GetSection( sections, typeof( DaySection ) );
			yearSection = (YearSection)XamMaskedInput.GetSection( sections, typeof( YearSection ) );
			hourSection = (HourSection)XamMaskedInput.GetSection( sections, typeof( HourSection ) );
			minuteSection = (MinuteSection)XamMaskedInput.GetSection( sections, typeof( MinuteSection ) );
			secondSection = (SecondSection)XamMaskedInput.GetSection( sections, typeof( SecondSection ) );
			ampmSection = (AMPMSection)XamMaskedInput.GetSection( sections, typeof( AMPMSection ) );

			//	BF 6.13.03	UWE610
			//
			//	In the UWE610 scenario, this whole block was being skipped
			//	because one or more of these sections were null. I relaxed
			//	this conditional to allow us to get in here if any section
			//	is non-null, so that we populate as much of the mask as we
			//	can given however many sections exist.
			



			if ( yearSection != null || monthSection != null ||
				daySection != null || hourSection != null ||
				minuteSection != null || secondSection != null )
			{

				if ( null != yearSection )
					// AS 8/25/08 Support Calendar
					//yearSection.SetText( date.Year.ToString( ).PadLeft( yearSection.NumberOfDigits, '0' ) );
					yearSection.SetText( calendar.GetYear( date ).ToString( ).PadLeft( yearSection.NumberOfDigits, '0' ) );

				if ( null != monthSection )
					// AS 8/25/08 Support Calendar
					//monthSection.SetText( date.Month.ToString( ).PadLeft( monthSection.NumberOfDigits, '0' ) );
					monthSection.SetText( calendar.GetMonth( date ).ToString( ).PadLeft( monthSection.NumberOfDigits, '0' ) );

				if ( null != daySection )
					// AS 8/25/08 Support Calendar
					//daySection.SetText( date.Day.ToString( ).PadLeft( daySection.NumberOfDigits, '0' ) );
					daySection.SetText( calendar.GetDayOfMonth( date ).ToString( ).PadLeft( daySection.NumberOfDigits, '0' ) );

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
				XamMaskedInput.SetText( sections, date.ToString( maskInfo.Format, maskInfo.FormatProvider ), maskInfo );
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
			NumberSection numberSection = (NumberSection)XamMaskedInput.GetSection( sections, typeof( NumberSection ) );
			FractionPart fractionSection = (FractionPart)XamMaskedInput.GetSection( sections, typeof( FractionPart ) );

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
				XamMaskedInput.EraseAllChars( sections );

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
				XamMaskedInput editor = maskInfo.MaskedInput;
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
						fp = -val + ip;
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
			//XamMaskedInput.SetText( sections, val.ToString( maskInfo.Format, maskInfo.FormatProvider ), maskInfo );
			string text = val.ToString( maskInfo.Format, maskInfo.FormatProvider );

			if ( text.Length > 2 && text.StartsWith( "0." ) )
				text = text.Remove( 0, 1 );

			XamMaskedInput.SetText( sections, text, maskInfo );

			// SSP 7/22/02
			// Fill remaining empty characters in the fraction porition of the currency mask
			// with zeros.
			//
			// SSP 8/30/11 TFS76307
			// Refactored. Moved the code into PadFractionPartHelper.
			// 
			// ------------------------------------------------------------------------------------
			maskInfo.MaskedInput.PadFractionPartHelper( sections );
			//FractionPart fractionPart = (FractionPart)XamMaskedInput.GetSection( sections, typeof( FractionPart ) );
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
			NumberSection numberSection = (NumberSection)XamMaskedInput.GetSection( sections, typeof( NumberSection ) );
			FractionPart fractionSection = (FractionPart)XamMaskedInput.GetSection( sections, typeof( FractionPart ) );

			// SSP 6/20/03 UWE606
			// A common situation is where the mask is something like ".nnnn" where they want 
			// to enter the fractions only (like in percentages).
			//
			//if ( null != numberSection && null != fractionSection )
			if ( null != numberSection || null != fractionSection )
			{
				// Erase everything first.
				//
				XamMaskedInput.EraseAllChars( sections );

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
				val = Math.Round( val, null != fractionSection ? Math.Min( fractionSection.DisplayChars.Count, 15 ) : 0 );

				// SSP 2/29/12 TFS92791
				// If the mask is not specified and auto-generated, expand it if we encounter a value that's
				// larger than our auto-generated mask.
				// 
				XamMaskedInput editor = maskInfo.MaskedInput;
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
						fp = -val + ip;
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
			//XamMaskedInput.SetText( sections, val.ToString( maskInfo.Format, maskInfo.FormatProvider ), maskInfo );
			string text = val.ToString( maskInfo.Format, maskInfo.FormatProvider );

			if ( text.Length > 2 && text.StartsWith( "0." ) )
				text = text.Remove( 0, 1 );

			XamMaskedInput.SetText( sections, text, maskInfo );

			// SSP 7/22/02
			// Fill remaining empty characters in the fraction porition of the currency mask
			// with zeros.
			//
			// SSP 8/30/11 TFS76307
			// Refactored. Moved the code into PadFractionPartHelper.
			// 
			// ------------------------------------------------------------------------------------
			maskInfo.MaskedInput.PadFractionPartHelper( sections );
			//FractionPart fractionPart = (FractionPart)XamMaskedInput.GetSection( sections, typeof( FractionPart ) );
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
			XamMaskedInput.SetDoubleValue( sections, (double)val, maskInfo );
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
			dataType = CoreUtilities.GetUnderlyingType( dataType );

			// If val is null or DBNull, then delete all the text 
			// and return 
			//
			if ( null == val || val is System.DBNull )
			{
				XamMaskedInput.SetText( sections, "", maskInfo );
				
				
				
				return true;
			}

			if ( !XamMaskedInput.SupportsDataType( dataType ) )
			{

				throw new ArgumentException( XamMaskedInput.GetString( "LE_NotSupportedException_2", dataType.Name ), "dataType" );
			}

			// Check for Nullable types
			dataType = CoreUtilities.GetUnderlyingType( dataType );

			if ( val.GetType( ) != dataType )
			{
				// AS 7/20/11 TFS81229
				// In the OnCoerceValue of the editor, we use the ConvertDataValue to change the data type 
				// so to be consistent we need to do that here as well. This also allows us to consider the 
				// format.
				//
				//val = Convert.ChangeType( val, dataType, maskInfo.FormatProvider );
				val = CoreUtilities.ConvertDataValue( val, dataType, maskInfo.FormatProvider, maskInfo.Format );
			}

			
			
			
			bool retVal = true;

			if ( typeof( DateTime ) == dataType )
			{
				// SSP 3/6/09 TFS15024
				// Pass along the new retainOriginalTime parameter.
				// 
				//XamMaskedInput.SetDateTimeValue( sections, (DateTime)val, maskInfo );
				XamMaskedInput.SetDateTimeValue( sections, (DateTime)val, maskInfo, false );
			}
			else if ( typeof( decimal ) == dataType )
			{
				XamMaskedInput.SetCurrencyValue( sections, (decimal)val, maskInfo );
			}
			else if ( typeof( double ) == dataType )
			{
				XamMaskedInput.SetDoubleValue( sections, (double)val, maskInfo );
			}
			else if ( typeof( float ) == dataType )
			{
				XamMaskedInput.SetFloatValue( sections, (float)val, maskInfo );
			}
			else if ( typeof( string ) == dataType )
			{
				
				
				
				//XamMaskedInput.SetText( sections, (string)val, maskInfo );
				string text = (string)val;
				int processedChars = XamMaskedInput.SetText( sections, text, maskInfo );
				if ( null != text && processedChars < text.Length )
					return false;
				
			}
			else if ( typeof( byte ) == dataType || typeof( sbyte ) == dataType ||
				typeof( short ) == dataType || typeof( ushort ) == dataType || typeof( Int16 ) == dataType ||
				typeof( int ) == dataType || typeof( uint ) == dataType || typeof( Int32 ) == dataType ||
				typeof( long ) == dataType || typeof( ulong ) == dataType || typeof( Int64 ) == dataType )
			{
				
				
				
				
				string text = val.ToString( );
				int processedChars = XamMaskedInput.SetText( sections, text, maskInfo );
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
				TypeConverter tc = null;

				tc = TypeDescriptor.GetConverter( dataType );


				if ( null != tc && tc.CanConvertTo( typeof( string ) ) )
				{

					string strValue = tc.ConvertToString( null, maskInfo.CultureInfo, val );



					int processedChars = XamMaskedInput.SetText( sections, strValue, maskInfo.PromptChar, maskInfo.PadChar, false );
					
					
					if ( null != strValue && processedChars < strValue.Length )
						retVal = false;
				}
				else
				{
					Debug.Assert( !XamMaskedInput.SupportsDataType( dataType ), "A suported data type but can't recognize it." );

					throw new ArgumentException( XamMaskedInput.GetString( "LE_NotSupportedException_2", dataType.Name ), "dataType" );
				}
			}

			// Pad the fraction portion of a number section with 0's so 1.__ becomes 1.00 etc.
			//
			XamMaskedInput.ValidateAllSections( sections );

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

					// SSP 9/1/09 TFS18219
					// Validate the value against the range specified in "{number:min-max}" mask.
					// 
					string tmpErrorMessage;
					if ( !numberSection.ValidateAgainstMinMaxHelper( out tmpErrorMessage ) )
					{
						maskValidationErrorMessage = tmpErrorMessage;
						return false;
					}

					string str = numberSection.GetText( InputMaskMode.Raw );
					if ( null == str || str.Length == 0 )
						continue;
				}

				// literals are skipped

				if ( null != editSection && !editSection.ValidateSection( false ) )
				{
					maskValidationErrorMessage = XamMaskedInput.GetString( "MaskValidationErrorInputDoesNotMatchMask" );
					return false;
				}
			}

			MonthSection monthSection = XamMaskedInput.GetSection( sections, typeof( MonthSection ) ) as MonthSection;
			DaySection daySection = XamMaskedInput.GetSection( sections, typeof( DaySection ) ) as DaySection;
			YearSection yearSection = XamMaskedInput.GetSection( sections, typeof( YearSection ) ) as YearSection;

			if ( null != monthSection && null != daySection )
			{
				int month = monthSection.ToInt( );
				int day = daySection.ToInt( );

				// If february and day is over 29, it's an invalid date
				if ( 2 == month && day > 29 )
				{
					maskValidationErrorMessage = XamMaskedInput.GetString( "MaskValidationErrorInvalidDate" );
					return false;
				}

				int daysInMonth = 30 + ( month + ( month >= 8 ? 1 : 0 ) ) % 2;

				if ( day > daysInMonth )
				{
					maskValidationErrorMessage = XamMaskedInput.GetString( "MaskValidationErrorInvalidDayOfMonth" );
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
					int year = yearSection.GetYear( );

					// AS 10/8/08 Support Calendar
					//if ( day > DateTime.DaysInMonth( year, month ) )
					System.Globalization.Calendar cal = sections.Calendar;
					if ( day > cal.GetDaysInMonth( year, month ) )
					{
						maskValidationErrorMessage = XamMaskedInput.GetString( "MaskValidationErrorInvalidDayOfMonth" );
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
			bool areAllSectionsValid = true;
			XamMaskedInput maskedInput = sections.MaskedInput;

			for ( int i = 0; i < sections.Count; i++ )
			{
				EditSectionBase editSection = sections[i] as EditSectionBase;

				if ( null != editSection )
					areAllSectionsValid = areAllSectionsValid && editSection.ValidateSection( );

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
						string str = numberSection.GetText( InputMaskMode.Raw );

						if ( null != str && str.Length > 0 )
							flag = true;
					}

					if ( !flag )
					{
						
						//string str = ((FractionPart)editSection).GetText( InputMaskMode.Raw );
						string str = editSection.GetText( InputMaskMode.Raw );

						if ( null != str && str.Length > 0 )
							flag = true;
					}

					if ( flag )
					{
						// SSP 8/30/11 TFS76307
						// Refactored. Moved the code into PadFractionPartHelper.
						// 
						//( (FractionPart)editSection ).PadWithZero( );
						if ( null != maskedInput )
							maskedInput.PadFractionPartHelper( sections );
					}
				}
			}

			// Apply AutoFillDate logic.
			// 
			// --------------------------------------------------------------------------------
			if ( loosingFocus )
			{
				AutoFillDate autoFillType = maskedInput.AutoFillDate;
				DateTime autoFillValue = DateTime.Now;

				// AS 8/25/08 Support Calendar
				System.Globalization.Calendar calendar = sections.Calendar;

				MonthSection monthSection = (MonthSection)XamMaskedInput.GetSection( sections, typeof( MonthSection ) );
				DaySection daySection = (DaySection)XamMaskedInput.GetSection( sections, typeof( DaySection ) );
				YearSection yearSection = (YearSection)XamMaskedInput.GetSection( sections, typeof( YearSection ) );

				if (
					// If month is not filled
					null != monthSection && monthSection.IsEmpty
					// And day is filled
					&& null != daySection && !daySection.IsEmpty
					&& AutoFillDate.MonthAndYear == autoFillType )
				{
					// Auto-fill the month.
					// AS 8/25/08 Support Calendar
					//monthSection.SetText( autoFillValue.Month.ToString( ) );
					monthSection.SetText( calendar.GetMonth( autoFillValue ).ToString( ) );
				}

				if (
					// If the year is not filled
					null != yearSection && yearSection.IsEmpty
					// And month is filled
					&& null != monthSection && !monthSection.IsEmpty
					// And day is non-existant or is filled
					&& ( null == daySection || !daySection.IsEmpty )
					&& ( AutoFillDate.Year == autoFillType || AutoFillDate.MonthAndYear == autoFillType ) )
				{
					// Auto-fill the year.
					// AS 8/25/08 Support Calendar
					//yearSection.SetText( autoFillValue.Year.ToString( ) );
					yearSection.SetText( calendar.GetYear( autoFillValue ).ToString( ) );
				}


				// SSP 9/21/11 TFS86681
				// If the number section is empty but some value was entered in fraction section then set
				// the number section to 0.
				// 
				NumberSection numberSection = (NumberSection)XamMaskedInput.GetSection( sections, typeof( NumberSection ) );
				if ( null != numberSection )
				{
					if ( numberSection.IsEmpty 
						// SSP 3/15/12 TFS98213
						// We also need to do this when the number section has only '-' or '+' and no digits.
						// 
						|| numberSection.HasOnlySignSymbol && numberSection.DisplayChars.Count >= 2
						)
					{
						// SSP 3/15/12 TFS98213
						// 
						
						
						
						if ( numberSection.IsFractionPartNonEmpty )
							numberSection.InsertCharAt( numberSection.DisplayChars.Count - 1, '0' );
					}
				}
			}
			// --------------------------------------------------------------------------------

			// When loosing focus, set the value of AM/PM section depending on the value of the 
			// hour section.
			// 
			// ------------------------------------------------------------------------------------
			if ( areAllSectionsValid && loosingFocus && null != sections.MaskInfo && null != maskedInput )
			{
				// SSP 9/15/11 TFS87816
				// Do this regardless of HasFormat or  DisplayFormattedTextWhenNotFocused.
				// 
				//if ( maskedInput.DisplayFormattedTextWhenNotFocused && sections.MaskInfo.HasFormat )
				{
					try
					{
						AMPMSection ampmSection = (AMPMSection)XamMaskedInput.GetSection( sections, typeof( AMPMSection ) );
						HourSection hourSection = (HourSection)XamMaskedInput.GetSection( sections, typeof( HourSection ) );
						if ( null != ampmSection && null != hourSection && ampmSection.IsEmpty && !hourSection.IsEmpty )
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
				}
			}
			// ------------------------------------------------------------------------------------

			return areAllSectionsValid;
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
				MonthSection monthSection = (MonthSection)XamMaskedInput.GetSection( sections, typeof( MonthSection ) );
				DaySection daySection = (DaySection)XamMaskedInput.GetSection( sections, typeof( DaySection ) );
				YearSection yearSection = (YearSection)XamMaskedInput.GetSection( sections, typeof( YearSection ) );
				HourSection hourSection = (HourSection)XamMaskedInput.GetSection( sections, typeof( HourSection ) );
				MinuteSection minuteSection = (MinuteSection)XamMaskedInput.GetSection( sections, typeof( MinuteSection ) );
				SecondSection secondSection = (SecondSection)XamMaskedInput.GetSection( sections, typeof( SecondSection ) );

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
					for ( int i = 0, count = sections.Count; i < count; i++ )
					{
						// SSP 1/25/02 UWG985
						// Changed the call from IsSectionNumeric to NumberSection
						//
						if ( sections[i] is NumberSection || ( pastDecimalSeperator && sections[i] is FractionPart ) )
						{
							if ( !pastDecimalSeperator )
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
							if ( sections[i].DecimalSeperatorChar == sections[i].DisplayChars[0].Char )
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

		#region VerifyElementsPositioned

		internal void VerifyElementsPositioned( )
		{
			this.UpdateLayout( );
		}

		#endregion // VerifyElementsPositioned

		#region DefaultCultureInfo

		// JJD 4/27/07
		// Optimization - cache the property locally
		[ThreadStatic( )]
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
			// do a get of XamMaskedInput.DefaultCultureInfo twice
			//if ( null == nfi && null != XamMaskedInput.DefaultCultureInfo )
			//    nfi = XamMaskedInput.DefaultCultureInfo.NumberFormat;
			if ( null == nfi )
			{
				CultureInfo ci = XamMaskedInput.DefaultCultureInfo;

				if ( ci != null )
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

			if ( null == dfi && null != XamMaskedInput.DefaultCultureInfo )
				dfi = XamMaskedInput.DefaultCultureInfo.DateTimeFormat;

			return dfi;
		}

		#endregion // GetNumberFormatInfo

		#region NotifyPropertyOnDisplayCharacters

		internal void NotifyDisplayCharDrawStringsChanged( )
		{
			SectionsCollection sections = this.Sections;
			XamMaskedInput.NotifyPropertyOnDisplayCharacters( sections, DisplayCharBase.PROPERTY_DRAWSTRING );
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

		internal static System.Globalization.Calendar GetCultureCalendar( IFormatProvider formatProvider )
		{
			DateTimeFormatInfo dateTimeFormatInfo = XamMaskedInput.GetDateTimeFormatInfo( formatProvider );

			Debug.Assert( null != dateTimeFormatInfo );

			return dateTimeFormatInfo.Calendar;
		}

		#endregion // GetCultureCalendar

		#region GetCultureChar

		internal static char GetCultureChar( char c, IFormatProvider formatProvider )
		{
			return XamMaskedInput.GetCultureChar( c, formatProvider, false );
		}

		internal static char GetCultureChar( char c, MaskInfo maskInfo )
		{
			bool useCurrencySymbols = null != maskInfo && typeof( decimal ) == maskInfo.DataType;

			return XamMaskedInput.GetCultureChar( c, null != maskInfo ? maskInfo.FormatProvider : null, useCurrencySymbols );
		}

		internal static char GetCultureChar( char c, IFormatProvider formatProvider, bool useCurrencySymbols )
		{
			NumberFormatInfo numberFormatInfo = XamMaskedInput.GetNumberFormatInfo( formatProvider );
			DateTimeFormatInfo dateTimeFormatInfo = XamMaskedInput.GetDateTimeFormatInfo( formatProvider );

			string retVal = null;

			switch ( c )
			{
				// Decimal separater.
				case '.':
					if ( !useCurrencySymbols )
						retVal = numberFormatInfo.NumberDecimalSeparator;
					else
						retVal = numberFormatInfo.CurrencyDecimalSeparator;
					break;

				// Thousands separator.
				case ',':
					if ( !useCurrencySymbols )
						retVal = numberFormatInfo.NumberGroupSeparator;
					else
						retVal = numberFormatInfo.CurrencyGroupSeparator;
					break;

				// Time separator.
				case ':':
					retVal = DateTime.Now.ToString( "%:", dateTimeFormatInfo );
					break;

				// Date separator.
				case '/':
					retVal = DateTime.Now.ToString( "%/", dateTimeFormatInfo );
					break;

				// Positive symbol.
				case '+':
					retVal = numberFormatInfo.PositiveSign;
					break;

				// Negative symbol.
				case '-':
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
				if ( XamMaskedInput.IsSectionNumeric( section ) )
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
			DateTimeFormatInfo dateTimeFormatInfo = XamMaskedInput.GetDateTimeFormatInfo( formatProvider );

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
						isVisible = this.IsInEditMode;
						break;
					case SpinButtonDisplayMode.MouseOver:
						isVisible = this.IsMouseOver || this.IsInEditMode;
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

		// AS 11/16/11 TFS90933
		#region OnMouseLeftButtonUpHandled


#region Infragistics Source Cleanup (Region)













































#endregion // Infragistics Source Cleanup (Region)

		#endregion //OnMouseLeftButtonUpHandled

		#endregion //Private/Internal methods

		#region Public properties/methods

		#region TrimFractionalZeros

		// SSP 8/30/11 TFS76307
		// 

		/// <summary>
		/// Identifies the <see cref="TrimFractionalZeros"/> dependency property
		/// </summary>
		public static readonly DependencyProperty TrimFractionalZerosProperty = DependencyPropertyUtilities.Register(
			"TrimFractionalZeros",
			typeof( bool ),
			typeof( XamMaskedInput ),
			DependencyPropertyUtilities.CreateMetadata( KnownBoxes.FalseBox, new PropertyChangedCallback( OnTrimFractionalZerosChanged ) )
		);

		private static void OnTrimFractionalZerosChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			XamMaskedInput editor = (XamMaskedInput)d;
			editor._cachedTrimFractionalZeros = (bool)e.NewValue;

			editor.ProcessPropertyChanged( e );
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

		/// <summary>
		/// Identifies the <see cref="SpinButtonDisplayMode"/> dependency property
		/// </summary>
		public static readonly DependencyProperty SpinButtonDisplayModeProperty = DependencyPropertyUtilities.Register("SpinButtonDisplayMode",
			typeof(SpinButtonDisplayMode), typeof(XamMaskedInput),
			SpinButtonDisplayMode.Never,
			new PropertyChangedCallback(OnProcessPropertyChangedCallback)
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
		public static readonly DependencyProperty SpinButtonStyleProperty = DependencyPropertyUtilities.Register(
			"SpinButtonStyle",
			typeof( Style ),
			typeof( XamMaskedInput ),
			null,
			new PropertyChangedCallback(OnProcessPropertyChangedCallback)
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
		private static readonly DependencyPropertyKey SpinButtonVisibilityResolvedPropertyKey = DependencyPropertyUtilities.RegisterReadOnly(
			"SpinButtonVisibilityResolved",
			typeof( Visibility ),
			typeof( XamMaskedInput ),
			Visibility.Collapsed,
			OnSpinButtonVisibilityResolvedChanged
		);

		private static void OnSpinButtonVisibilityResolvedChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			XamMaskedInput input = (XamMaskedInput)d;
			input.ProcessPropertyChanged( e );
		}

		/// <summary>
		/// Identifies the read-only <see cref="SpinButtonVisibilityResolved"/> dependency property.
		/// </summary>
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
		public static readonly DependencyProperty SpinIncrementProperty = DependencyPropertyUtilities.Register(
			"SpinIncrement",
			typeof( object ),
			typeof( XamMaskedInput ),
			null, 
			new PropertyChangedCallback( OnSpinIncrementChanged ) 
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
			XamMaskedInput maskedInput = (XamMaskedInput)dependencyObject;
			object newVal = (object)e.NewValue;

			maskedInput._cachedSpinInfo = null != newVal ? SpinInfo.Parse( maskedInput, newVal ) : null;

			maskedInput.ProcessPropertyChanged(e);
		}

		#endregion // SpinIncrement

		#region SpinWrap

		/// <summary>
		/// Identifies the <see cref="SpinWrap"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty SpinWrapProperty = DependencyPropertyUtilities.Register(
			"SpinWrap",
			typeof( bool ),
			typeof( XamMaskedInput ),
			KnownBoxes.FalseBox,
			new PropertyChangedCallback(OnProcessPropertyChangedCallback)
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
		/// To actually specify the minimum and maximum value, use the <see cref="ValueInput.ValueConstraint"/>
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

				System.Media.SystemSounds.Beep.Play( );

			}
			catch
			{
				Debug.Assert( false );
			}
		}

		#endregion //Beep

		#region Sections

		private static readonly DependencyPropertyKey SectionsPropertyKey = DependencyPropertyUtilities.RegisterReadOnly(
					"Sections",
					typeof( SectionsCollection ),
					typeof( XamMaskedInput ),
					null,
					new PropertyChangedCallback( OnSectionsChanged )
				);

		// AS 9/3/08 NA 2008 Vol 2
		private static void OnSectionsChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			( (XamMaskedInput)d ).OnSectionsChanged( );
		}

		// AS 9/3/08 NA 2008 Vol 2
		internal virtual void OnSectionsChanged( )
		{
			// AS 8/2/11
			// Let the textbox know when the sections have changed so it can initialize the InputScope as needed.
			//
			if (_editInfo != null && _editInfo._imeTextBox != null)
				_editInfo._imeTextBox.OnSectionsChanged();
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
		/// property. XamMaskedInput also exposes a collection that contains aggregate display characters 
		/// of all sections via its <see cref="XamMaskedInput.DisplayChars"/> property.
		/// </para>
		/// <para class="body">
		/// This property is useful for example if you want to query and find out the structure 
		/// of the parsed mask or to query and/or manipulate the current user input on a per
		/// section or per display character basis.
		/// </para>
		/// <seealso cref="XamMaskedInput.DisplayChars"/>
		/// </remarks>
		[ReadOnly( true )]

		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]

		public SectionsCollection Sections
		{
			get
			{
				return (SectionsCollection)this.GetValue( SectionsProperty );
			}
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
		[Browsable( false )]
		[ReadOnly( true ), Bindable( false )]

		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]

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
		/// Changes the default mask used by the MaskedInput for the specified data type.
		/// </summary>
		/// <param name="dataType">Data type for which to register the mask.</param>
		/// <param name="mask">The default mask that will be used for the specified data type.</param>
		/// <remarks>
		/// <para class="body">
		/// If a mask is not explicitly specified on a MaskedInput or derived editor, 
		/// a default is calculated based on the data type. You can override these 
		/// default calculated masks using this method.
		/// </para>
		/// <seealso cref="GetDefaultMaskForType"/>
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
		/// <param name="dataType">Data type for which to reset the mask to the default mask.</param>
		/// <remarks>
		/// <para class="body">
		/// This method is used to revert any default masks that were registered using 
		/// <see cref="RegisterDefaultMaskForType"/> method. The mask will be reverted back
		/// to the default mask that's calculated by the MaskedInput.
		/// </para>
		/// </remarks>
		public static void ResetDefaultMaskForType( Type dataType )
		{
			RegisterDefaultMaskForType( dataType, null );
		}

		#endregion // ResetDefaultMaskForType

		#region GetDefaultMaskForType

		/// <summary>
		/// Returns the default mask that will be used for the specified data type by the MaskedInput
		/// and derived editors for the specified data type.
		/// </summary>
		/// <param name="dataType">Gets the default mask that will be used for the specified data type, if a default exists for this type.</param>
		/// <returns></returns>
		/// <remarks>
		/// <para class="body">
		/// You can change the default masks using the <see cref="RegisterDefaultMaskForType"/> static method.
		/// </para>
		/// </remarks>
		public static string GetDefaultMaskForType( Type dataType )
		{
			if ( g_defaultMaskTableInitialized )
			{
				lock ( g_defaultMaskTable )
				{
					string mask = g_defaultMaskTable[dataType] as string;
					if ( null != mask && mask.Length > 0 )
						return mask;
				}
			}

			return GetDefaultMaskForTypeHelper( dataType );
		}

		#endregion // GetDefaultMaskForType

		#region GetDefaultMaskForTypeHelper

		private static string GetDefaultMaskForTypeHelper( Type type )
		{
			type = CoreUtilities.GetUnderlyingType( type );

			string mask = null;

			if ( typeof( DateTime ) == type )
			{
				mask = "{date}";
			}
			else if ( typeof( byte ) == type )
			{
				mask = "{number:0-255}";
			}
			else if ( typeof( sbyte ) == type )
			{
				mask = "{number:-128-127}";
			}
			else if ( typeof( short ) == type )
			{
				mask = "{number:-32768-32767}";
			}
			else if ( typeof( ushort ) == type )
			{
				mask = "{number:0-65535}";
			}
			else if ( typeof( int ) == type )
			{
				mask = "{number:-2147483648-2147483647}";
			}
			else if ( typeof( uint ) == type )
			{
				mask = "{number:0-4294967295}";
			}
			else if ( typeof( long ) == type )
			{
				
				
				
				
				
				mask = "{number:-9223372036854775808-9223372036854775807}";
			}
			else if ( typeof( ulong ) == type )
			{
				mask = "{number:0-18446744073709551615}";
			}
			else if ( typeof( decimal ) == type )
			{
				mask = "{currency}";
			}
			else if ( typeof( double ) == type || typeof( float ) == type )
			{
				mask = "{double}";
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
		/// Returns true if the data type is supported by the XamMaskedInput, false otherwise.
		/// </summary>
		/// <param name="dataType"><see cref="Type"/></param>
		/// <returns><b>True</b> if type is supported, <b>False</b> otherwise.</returns>
		public static bool SupportsDataType( System.Type dataType )
		{
			// AS 10/17/08 TFS8886
			bool usesConverter;
			return SupportsDataType( dataType, out usesConverter );
		}

		// AS 10/17/08 TFS8886
		// Added an overload so we can determine if we're using the type converter.
		//
		internal static bool SupportsDataType( System.Type dataType, out bool usesConverter )
		{
			// AS 10/17/08 TFS8886
			usesConverter = false;

			System.Type type = dataType;

			if ( null == type )
				return false;

			// Check for Nullable types
			type = CoreUtilities.GetUnderlyingType( type );

			if ( typeof( DateTime ) == type )
			{
			}
			else if (
				typeof( short ) == type ||
				typeof( int ) == type ||
				typeof( long ) == type ||
				typeof( ushort ) == type ||
				typeof( uint ) == type ||
				typeof( ulong ) == type ||
				typeof( Int16 ) == type ||
				typeof( Int32 ) == type ||
				typeof( Int64 ) == type ||
				typeof( byte ) == type ||
				typeof( sbyte ) == type )
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
				TypeConverter tc = TypeDescriptor.GetConverter( dataType );

				if ( tc.CanConvertFrom( typeof( string ) ) && tc.CanConvertTo( typeof( string ) ) )
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
		/// is set to DateTime the XamMaskedInput and derived editors will call this method 
		/// to calculate the mask if none has been explicitly set. Also if you want to explicitly
		/// set the mask to make use of culture sensitive date and time mask then use the following
		/// mask tokens when setting the <see cref="XamMaskedInput.Mask"/> property:
		/// <ul>
		/// <li><b>{date}</b> - Date mask based on <i>short</i> date pattern setting of the system.</li>
		/// <li><b>{time}</b> - Time mask based on <i>short</i> time pattern setting of the system. Short time pattern typically does not include seconds portion.</li>
		/// <li><b>{longtime} - Time mask based on <i>long</i> time pattern setting of the system. Long time pattern typically includes seconds portion.</b></li>
		/// </ul>
		/// </para>
		/// <seealso cref="XamMaskedInput.Mask"/>
		/// </remarks>
		public static string CalcDefaultTimeMask( IFormatProvider formatProvider )
		{
			// SSP 12/18/02 UWE342
			//
			
#region Infragistics Source Cleanup (Region)









#endregion // Infragistics Source Cleanup (Region)

			DateTimeFormatInfo dateTimeFormatInfo = XamMaskedInput.GetDateTimeFormatInfo( formatProvider );

			bool hasSeconds = false;
			bool hasAMPM = false;

			string shortDatePattern = dateTimeFormatInfo.ShortTimePattern;

			// AS 11/12/03 optimization
			// Get the character now so we can see if we can
			// just use the last mask we calculated.
			//
			char timeSeparatorChar = XamMaskedInput.GetCultureChar( ':', formatProvider );

			// AS 11/12/03 optimization
			// If everything is the same, use the same mask
			// as the last time.
			//
			if ( timeSeparatorChar == g_lastTimeSeparator &&
				shortDatePattern == g_lastShortTimePattern )
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
			//return mask.Replace( ':', XamMaskedInput.GetCultureChar( ':', formatProvider ) );
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
		/// is set to DateTime the XamMaskedInput and derived editors will call this method 
		/// to calculate the mask if none has been explicitly set. Also if you want to explicitly
		/// set the mask to make use of culture sensitive date and time mask then use the following
		/// mask tokens when setting the <see cref="XamMaskedInput.Mask"/> property:
		/// <ul>
		/// <li><b>{date}</b> - Date mask based on <i>short</i> date pattern setting of the system.</li>
		/// <li><b>{time}</b> - Time mask based on <i>short</i> time pattern setting of the system. Short time pattern typically does not include seconds portion.</li>
		/// <li><b>{longtime} - Time mask based on <i>long</i> time pattern setting of the system. Long time pattern typically includes seconds portion.</b></li>
		/// </ul>
		/// </para>
		/// <seealso cref="XamMaskedInput.Mask"/>
		/// </remarks>
		public static string CalcDefaultLongTimeMask( IFormatProvider formatProvider )
		{
			// SSP 12/18/02 UWE342
			//
			
#region Infragistics Source Cleanup (Region)









#endregion // Infragistics Source Cleanup (Region)

			DateTimeFormatInfo dateTimeFormatInfo = XamMaskedInput.GetDateTimeFormatInfo( formatProvider );

			bool hasSeconds = false;
			bool hasAMPM = false;

			string longTimePattern = dateTimeFormatInfo.LongTimePattern;

			// AS 11/12/03 optimization
			// Get the character now so we can see if we can
			// just use the last mask we calculated.
			//
			char timeSeparatorChar = XamMaskedInput.GetCultureChar( ':', formatProvider );

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
			//return mask.Replace( ':', XamMaskedInput.GetCultureChar( ':', formatProvider ) );
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
		/// is set to DateTime the XamMaskedInput and derived editors will call this method 
		/// to calculate the mask if none has been explicitly set. Also if you want to explicitly
		/// set the mask to make use of culture sensitive date and time mask then use the following
		/// mask tokens when setting the <see cref="XamMaskedInput.Mask"/> property:
		/// <ul>
		/// <li><b>{date}</b> - Date mask based on <i>short</i> date pattern setting of the system.</li>
		/// <li><b>{time}</b> - Time mask based on <i>short</i> time pattern setting of the system. Short time pattern typically does not include seconds portion.</li>
		/// <li><b>{longtime} - Time mask based on <i>long</i> time pattern setting of the system. Long time pattern typically includes seconds portion.</b></li>
		/// </ul>
		/// </para>
		/// <seealso cref="XamMaskedInput.Mask"/>
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
		/// is set to DateTime the XamMaskedInput and derived editors will call this method 
		/// to calculate the mask if none has been explicitly set. Also if you want to explicitly
		/// set the mask to make use of culture sensitive date and time mask then use the following
		/// mask tokens when setting the <see cref="XamMaskedInput.Mask"/> property:
		/// <ul>
		/// <li><b>{date}</b> - Date mask based on <i>short</i> date pattern setting of the system.</li>
		/// <li><b>{time}</b> - Time mask based on <i>short</i> time pattern setting of the system. Short time pattern typically does not include seconds portion.</li>
		/// <li><b>{longtime} - Time mask based on <i>long</i> time pattern setting of the system. Long time pattern typically includes seconds portion.</b></li>
		/// </ul>
		/// </para>
		/// <seealso cref="XamMaskedInput.Mask"/>
		/// </remarks>
		public static string CalcDefaultDateMask( IFormatProvider formatProvider, bool usePostfixSeparatorsFromLongDatePattern )
		{
			// SSP 12/18/02 UWE342
			// Use the new helper method to get the date time format info object.
			//
			
#region Infragistics Source Cleanup (Region)











#endregion // Infragistics Source Cleanup (Region)

			DateTimeFormatInfo dateTimeFormatInfo = XamMaskedInput.GetDateTimeFormatInfo( formatProvider );

			string shortDatePattern = dateTimeFormatInfo.ShortDatePattern;

			string mask = null;

			// AS 11/12/03 optimization
			// Get the character now so we can see if we can
			// just use the last mask we calculated.
			//
			char dateSeparatorChar = XamMaskedInput.GetCultureChar( '/', formatProvider );

			// AS 11/12/03 optimization
			// If everything is the same, use the same mask
			// as the last time.
			//
			if ( dateSeparatorChar == g_lastDateSeparator &&
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
					Dictionary<Type, char> notUsed = null;
					mask = XamMaskedInput.GetNonstandardForeignDateMaskAndPostfixSymbols( dateTimeFormatInfo, ref notUsed );
				}

				bool doNormalProcessing = ( mask == null || mask.Length == 0 );

				if ( doNormalProcessing )
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
			//return mask.Replace( '/', XamMaskedInput.GetCultureChar( '/', formatProvider ) );
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
		/// is set to Decimal the XamMaskedInput and derived editors will call this method 
		/// to calculate the mask if none has been explicitly set. Also if you want to explicitly
		/// set the mask to make use of culture sensitive currency mask then use one of the
		/// currency tokens as documented in the table listing all the mask tokens in the
		/// help for <see cref="XamMaskedInput.Mask"/> property.
		/// </para>
		/// <seealso cref="XamMaskedInput.Mask"/>
		/// </remarks>
		public static string CalcDefaultCurrencyMask( IFormatProvider formatProvider,
			int integerDigits, int fractionDigits, char allowNegatives, bool includeCurrencySymbol )
		{
			// SSP 12/18/02 UWE342
			// Use the new helper method to get the date time format info object.
			//
			
#region Infragistics Source Cleanup (Region)









#endregion // Infragistics Source Cleanup (Region)

			NumberFormatInfo numberFormatInfo = XamMaskedInput.GetNumberFormatInfo( formatProvider );

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
			bool includeSpaceWithCurrencySymbol = ( currencyPositivePattern > 1 ) ? true : false;
			bool currencySymbolAtBeginning = ( currencyPositivePattern % 2 == 0 ) ? true : false;

			// SSP 8/27/06 - NAS 6.3
			// Added an overload that takes in includeCurrencySymbol parameter.
			// 
			//if ( currencySymbolAtBeginning )
			if ( currencySymbolAtBeginning && includeCurrencySymbol )
			{
				// MBS 9/26/06 BR15826
				//maskSB.Append( numberFormatInfo.CurrencySymbol );
				string literalCurrencySymbol;
				MaskParser.EscapeLiteralsInString( numberFormatInfo.CurrencySymbol, out literalCurrencySymbol );
				maskSB.Append( literalCurrencySymbol );

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
					commaPositionDelta = commPosDeltaArr[currencyGroupSizesIndex];
				else
					commaPositionDelta = commPosDeltaArr[commPosDeltaArr.Length - 1];

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
			if ( !currencySymbolAtBeginning && includeCurrencySymbol )
			{
				if ( includeSpaceWithCurrencySymbol )
					maskSB.Append( ' ' );

				// MBS 9/26/06 BR15826
				//maskSB.Append( numberFormatInfo.CurrencySymbol );
				string literalCurrencySymbol;
				MaskParser.EscapeLiteralsInString( numberFormatInfo.CurrencySymbol, out literalCurrencySymbol );
				maskSB.Append( literalCurrencySymbol );
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
		/// is set to Double or Float, the XamMaskedInput and derived editors will call this method 
		/// to calculate the mask if none has been explicitly set. Also if you want to explicitly
		/// set the mask to make use of culture sensitive double/float mask then use one of the
		/// double tokens as documented in the table listing all the mask tokens in the
		/// help for <see cref="XamMaskedInput.Mask"/> property.
		/// </para>
		/// <seealso cref="XamMaskedInput.Mask"/>
		/// </remarks>
		public static string CalcDefaultDoubleMask( IFormatProvider formatProvider,
			int integerDigits, int fractionDigits, char allowNegatives )
		{
			// SSP 12/18/02 UWE342
			// Use the new helper method to get the nbumber format info object.
			//
			
#region Infragistics Source Cleanup (Region)









#endregion // Infragistics Source Cleanup (Region)

			NumberFormatInfo numberFormatInfo = XamMaskedInput.GetNumberFormatInfo( formatProvider );

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
					commaPositionDelta = commPosDeltaArr[numberGroupSizesIndex];
				else
					commaPositionDelta = commPosDeltaArr[commPosDeltaArr.Length - 1];

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

			System.Type type = CoreUtilities.GetUnderlyingType( dataType );

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

				for ( int i = 0, count = sections.Count; i < count; i++ )
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
					monthSections <= 1 &&		// No repeating sections and
					daySections <= 1 &&		// at least one of them
					yearSections <= 1 &&
					hourSections <= 1 &&
					minuteSections <= 1 &&
					secondSections <= 1 &&
					// SSP 12/19/02
					//
					amPmSection <= 1;

				b = b &&
					( 1 == monthSections ||
					1 == daySections ||
					1 == yearSections ||
					1 == hourSections );

				return b;
			}
			else if ( typeof( byte ) == type || typeof( sbyte ) == type
				|| typeof( short ) == type || typeof( ushort ) == type || typeof( Int16 ) == type
				|| typeof( int ) == type || typeof( uint ) == type || typeof( Int32 ) == type ||
				typeof( long ) == type || typeof( ulong ) == type || typeof( Int64 ) == type )
			{
				int numericSections = 0;

				for ( int i = 0, count = sections.Count; i < count; i++ )
				{
					if ( XamMaskedInput.IsSectionNumeric( sections[i] ) )
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
				for ( int i = 0, count = sections.Count; i < count; i++ )
				{
					SectionBase section = sections[i];
					if ( XamMaskedInput.IsSectionNumeric( section ) )
					{
						if ( !pastDecimalSeperator )
						{
							integerPart = section;
						}
						else if ( null == fractionPart )
						{
							fractionPart = section;
						}
						else
						{
							return false;
						}
					}
					// skip the commas
					else if ( section is LiteralSection &&
						1 == section.DisplayChars.Count &&
						section.CommaChar == section.DisplayChars[0].Char )
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
					else if ( section is LiteralSection &&
						1 == section.DisplayChars.Count &&
						section.DecimalSeperatorChar == section.DisplayChars[0].Char )
						pastDecimalSeperator = true;
					else if ( ( null != integerPart || null != fractionPart || pastDecimalSeperator )
						// SSP 2/09/04 UWE792
						// Allow for literal sections to follow a number mask like in "nnn.nn %".
						//
						&& !( section is LiteralSection ) )
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
		/// <see cref="ValueInput.Text"/> property can also be used to retrieve the current text of the editor.
		/// The Text property will return a value with the mask mode specified by the <see cref="DataMode"/>
		/// property applied to the returned value. This method allows you to use any mode without having to
		/// set the DataMode property.
		/// </para>
		/// <para class="body">
		/// Any of <see cref="ValueInput.Value"/>, <see cref="ValueInput.Text"/> and <see cref="TextInputBase.DisplayText"/> 
		/// properties can also be used to retrieve the current value of the editor.
		/// </para>
		/// <seealso cref="ValueInput.Value"/>
		/// <seealso cref="ValueInput.Text"/>
		/// <seealso cref="TextInputBase.DisplayText"/>
		/// <seealso cref="XamMaskedInput.DataMode"/>
		/// <seealso cref="XamMaskedInput.DisplayMode"/>
		/// <seealso cref="XamMaskedInput.Mask"/>
		/// </remarks>
		public string GetText( InputMaskMode maskMode )
		{
			
			
			
			

			EditInfo editInfo = this.EditInfo;
			if ( null != editInfo )
				return editInfo.GetText( maskMode );

			MaskInfo maskInfo = this.MaskInfo;
			return XamMaskedInput.GetText( maskInfo.Sections, maskMode, maskInfo );
			
		}

		#endregion //GetText

		#region MaskInfo

		internal MaskInfo MaskInfo
		{
			get
			{
				return _maskInfo;
			}
		}

		#endregion // MaskInfo

		#region EditInfo

		internal EditInfo EditInfo
		{
			get
			{
				return this._editInfo;
			}
		}

		#endregion // EditInfo

		#region InsertMode

		/// <summary>
		/// Identifies the <see cref="InsertMode"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty InsertModeProperty = DependencyPropertyUtilities.Register(
			"InsertMode",
			typeof( bool ),
			typeof( XamMaskedInput ),
			KnownBoxes.TrueBox,
			new PropertyChangedCallback(OnProcessPropertyChangedCallback)
			);

		/// <summary>
		/// Returns or sets the editing mode (insert or overstrike).
		/// </summary>
		/// <remarks>
		/// <p class="body">When this property is set to True, characters typed will be inserted at the current caret position and any following characters will be shifted. When set to False, typing at an insertion point that contains an existing character will replace that character. The value of this property also affects how characters are deleted using either The Delete key or the Backspace key. When in insert mode, characters after the character being deleted will be shifted by one to the left within the section.</p>
		/// <seealso cref="AllowShiftingAcrossSections"/>
		/// <seealso cref="SelectAllBehavior"/>
		/// <seealso cref="SectionTabNavigation"/>
		/// <seealso cref="AutoFillDate"/>
		/// </remarks>
		//[Description( "Specifies whether the edit mode is in 'insert' or 'overstrike'." )]
		//[Category( "Behavior" )]
		[Bindable( true )]
		[Browsable( false )]
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
		public static readonly DependencyProperty MaskProperty = DependencyPropertyUtilities.Register(
			"Mask",
			typeof( string ),
			typeof( XamMaskedInput ),
			null,
			new PropertyChangedCallback( OnMaskChanged )
			);

		// JJD 4/27/07
		// Optimization - cache the property locally
		private string _cachedMask;

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
			XamMaskedInput editor = (XamMaskedInput)dependencyObject;

			
			
			
			
			
			editor._cachedMask = (string)e.NewValue;

			// Don't take any actions if we are in the process of initializing. At the
			// end of the initialization process we'll take the necessary actions.
			// 
			if ( editor.InternalIsInitialized )
				editor.ReparseMask( );

			editor.ProcessPropertyChanged(e);
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
		/// Overridden. Called when the ValueTypeResolved property changes.
		/// </summary>
		/// <param name="newType">New value of the property.</param>
		protected override void OnValueTypeResolvedChanged( Type newType )
		{
			base.OnValueTypeResolvedChanged( newType );

			if ( null != _maskInfo )
			{
				_maskInfo.DataType = this.ValueTypeResolved;

				// Don't take any actions if we are in the process of initializing. At the
				// end of the initialization process we'll take the necessary actions.
				// 
				if ( this.InternalIsInitialized )
					this.ReparseMask( );
			}
		}

		private void ApplyMaskToText( )
		{
			this.SyncValueProperties( ValueProperty, this.Value );

			
			
			
			
			
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
			IFormatProvider formatProvider, char promptCharacter, char padCharacter, InputMaskMode maskMode )
		{
			ParsedMask parsedMask = new ParsedMask( mask, formatProvider, promptCharacter, padCharacter );
			return parsedMask.ApplyMask( text, maskMode );
		}

		#endregion // ApplyMask

		#region SyncValuePropertiesOverride

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
			bool retVal = base.SyncValuePropertiesOverride( prop, newValue, out error );

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
		public static readonly DependencyProperty DataModeProperty = DependencyPropertyUtilities.Register(
			"DataMode",
			typeof( InputMaskMode ),
			typeof( XamMaskedInput ),
			InputMaskMode.IncludeLiterals, 
			new PropertyChangedCallback( OnDataModeChanged )
		);

		
		
		
		
		
		
		//private InputMaskMode _cachedDataMode = InputMaskMode.IncludeLiterals;
		private InputMaskMode _cachedDataMode;

		/// <summary>
		/// Returns or sets a value that determines how the control's contents will be stored by 
		/// the data source when data masking is enabled.
		/// </summary>
		/// <remarks>
		/// <p class="body">This property is used to determine how mask literals and prompt characters are handled when the control's contents are passed to the data source (or are retrieved using the <see cref="ValueInput.Text"/> property.) Based on the setting of this property, the text of the control will contain no prompt characters or literals (just the raw data), the data and just the literals, the data and just the prompt characters, or all the text including both prompt characters and literals. The formatted spacing of partially masked values can be preserved by indicating to include literals with padding, which includes data and literal characters, but replaces prompt characters with pad characters (usually spaces).</p>
		/// <p class="body">Generally, simply the raw data is committed to the data source and data masking is used to format the data when it is displayed. In some cases, however, it may be appropriate in your application to store mask literals as well as data.</p>
		/// <seealso cref="DisplayMode"/> <seealso cref="ClipMode"/>
		/// </remarks>
		//[Description( "Specifies the mask mode that will be applied to data returned by the Text and Value properties." )]
		//[Category( "Behavior" )]
		[Bindable( true )]
		public InputMaskMode DataMode
		{
			get
			{
				// JJD 4/27/07
				// Optimization - use the locally cached property 
				//return (InputMaskMode)this.GetValue( DataModeProperty );
				return this._cachedDataMode;
			}
			set
			{
				this.SetValue( DataModeProperty, value );
			}
		}

		private static void OnDataModeChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			XamMaskedInput editor = (XamMaskedInput)dependencyObject;
			InputMaskMode newVal = (InputMaskMode)e.NewValue;

			
			
			
			
			
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
			if ( editor.InternalIsInitialized )
				editor.ApplyMaskToText( );

			editor.ProcessPropertyChanged(e);
		}

		/// <summary>
		/// Returns true if the DataMode property is set to a non-default value.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public bool ShouldSerializeDataMode( )
		{
			return DependencyPropertyUtilities.ShouldSerialize( this, DataModeProperty );
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
		public static readonly DependencyProperty ClipModeProperty = DependencyPropertyUtilities.Register(
			"ClipMode",
			typeof( InputMaskMode ),
			typeof( XamMaskedInput ),
			InputMaskMode.IncludeLiterals, 
			new PropertyChangedCallback( OnClipModeChanged )
		);

		
		
		
		
		
		
		
		private InputMaskMode _cachedClipMode;

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
		public InputMaskMode ClipMode
		{
			get
			{
				// JJD 4/27/07
				// Optimization - use the locally cached property 
				//return (InputMaskMode)this.GetValue( ClipModeProperty );
				return this._cachedClipMode;
			}
			set
			{
				this.SetValue( ClipModeProperty, value );
			}
		}

		private static void OnClipModeChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			XamMaskedInput editor = (XamMaskedInput)dependencyObject;
			InputMaskMode newVal = (InputMaskMode)e.NewValue;

			
			
			
			
			
			editor._cachedClipMode = newVal;

			if ( null != editor._maskInfo )
				editor._maskInfo.ClipMode = newVal;

			editor.ProcessPropertyChanged(e);
		}

		/// <summary>
		/// Returns true if the ClipMode property is set to a non-default value.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public bool ShouldSerializeClipMode( )
		{
			return DependencyPropertyUtilities.ShouldSerialize( this, ClipModeProperty );
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
		public static readonly DependencyProperty DisplayModeProperty = DependencyPropertyUtilities.Register(
			"DisplayMode",
			typeof( InputMaskMode ),
			typeof( XamMaskedInput ),
			InputMaskMode.IncludeLiterals, 
			new PropertyChangedCallback( OnDisplayModeChanged )
		);

		
		
		
		
		
		
		
		private InputMaskMode _cachedDisplayMode;

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
		public InputMaskMode DisplayMode
		{
			get
			{
				// JJD 4/27/07
				// Optimization - use the locally cached property 
				//return (InputMaskMode)this.GetValue( DisplayModeProperty );
				return this._cachedDisplayMode;
			}
			set
			{
				this.SetValue( DisplayModeProperty, value );
			}
		}

		private static void OnDisplayModeChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			XamMaskedInput editor = (XamMaskedInput)dependencyObject;
			InputMaskMode newVal = (InputMaskMode)e.NewValue;

			
			
			
			
			
			editor._cachedDisplayMode = newVal;

			if ( null != editor._maskInfo )
				editor._maskInfo.DisplayMode = newVal;

			editor.ProcessPropertyChanged(e);
		}

		/// <summary>
		/// Returns true if the DisplayMode property is set to a non-default value.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public bool ShouldSerializeDisplayMode( )
		{
			return DependencyPropertyUtilities.ShouldSerialize( this, DisplayModeProperty );
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
		public static readonly DependencyProperty PadCharProperty = DependencyPropertyUtilities.Register(
			"PadChar",
			typeof( char ),
			typeof( XamMaskedInput ),
			DEFAULT_PAD_CHAR, 
			new PropertyChangedCallback( OnPadCharChanged )
		);

		
		
		
		
		
		
		
		private char _cachedPadChar;

		/// <summary>
		/// Returns or sets the character that will be used as the pad character. Default is space character (' ').
		/// </summary>
		/// <remarks>
		/// <p class="body">The pad character is the character that is used to replace the prompt characters when getting the data from the XamMaskedInput control with DataMode of IncludeLiteralsWithPadding.</p>
		/// <para class="body">
		/// For example, if the data in the editor is as follows:<br/>
		/// 111-2_-____<br/>
		/// and DataMode is set to IncludeLiteralsWithPadding then the returned value will be "111-2 -    ".
		/// Prompt characters will be replaced by the pad character.
		/// </para>
		/// <seealso cref="PromptChar"/>
		/// </remarks>
		[Bindable( true )]
		[TypeConverter( typeof( MaskCharConverter ) )]
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
			return XamMaskedInput.IsValidPromptChar( (char)objVal );
		}

		private static void OnPadCharChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			XamMaskedInput editor = (XamMaskedInput)dependencyObject;

			
			
			
			
			
			char newVal = (char)e.NewValue;
			editor._cachedPadChar = newVal;

			ValidatePadChar( newVal );

			editor.NotifyDisplayCharDrawStringsChanged( );

			editor.ProcessPropertyChanged(e);
		}

		/// <summary>
		/// Returns true if the PadChar property is set to a non-default value.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public bool ShouldSerializePadChar( )
		{
			return DependencyPropertyUtilities.ShouldSerialize( this, PadCharProperty );
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
		public static readonly DependencyProperty PromptCharProperty = DependencyPropertyUtilities.Register(
			"PromptChar",
			typeof( char ),
			typeof( XamMaskedInput ),
			DEFAULT_PROMPT_CHAR, 
			new PropertyChangedCallback( OnPromptCharChanged )
		);

		
		
		
		
		
		
		//private char _cachedPromptChar = DEFAULT_PROMPT_CHAR;
		private char _cachedPromptChar;

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
		[TypeConverter( typeof( MaskCharConverter ) )]
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
			return XamMaskedInput.IsValidPromptChar( (char)objVal );
		}

		private static void OnPromptCharChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			XamMaskedInput editor = (XamMaskedInput)dependencyObject;

			
			
			
			
			
			
			
			char newVal = (char)e.NewValue;
			editor._cachedPromptChar = newVal;

			ValidatePromptChar( newVal );

			editor.NotifyDisplayCharDrawStringsChanged( );

			editor.ProcessPropertyChanged(e);
		}

		/// <summary>
		/// Returns true if the PromptChar property is set to a non-default value.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public bool ShouldSerializePromptChar( )
		{
			return DependencyPropertyUtilities.ShouldSerialize( this, PromptCharProperty );
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
			// SSP 5/13/10 TFS 31103
			// Pass true for the new fireTextChanged parameter, which will raise value/text changed events.
			// 
			//return this.EditInfo.Delete( );
			return this.EditInfo.Delete( false, true );
		}

		#endregion // Delete

		#region Copy

		/// <summary>
		/// Performs a Copy edit operation on the currently selected text, placing it on the clipboard.
		/// </summary>
		public void Copy( )
		{
			this.EditInfo.Copy( );
		}

		#endregion //Copy

		#region Cut

		/// <summary>
		/// Performs a Cut edit operation on the currently selected text, removing it from the control and placing it on the clipboard.
		/// </summary>
		public void Cut( )
		{
			this.EditInfo.Cut( );
		}

		#endregion //Cut

		#region Paste

		/// <summary>
		/// Performs a Paste edit operation.
		/// </summary>
		public void Paste( )
		{
			this.EditInfo.Paste( );
		}

		#endregion // Paste

		#region ToggleInsertMode

		/// <summary>
		/// Toggles between insert and overstrike mode.
		/// </summary>
		public void ToggleInsertMode( )
		{
			this.EditInfo.ToggleInsertMode( );
		}

		#endregion // ToggleInsertMode

		#region SectionTabNavigation

		/// <summary>
		/// Identifies the <see cref="SectionTabNavigation"/> dependency property
		/// </summary>
		public static readonly DependencyProperty SectionTabNavigationProperty = DependencyPropertyUtilities.Register(
			"SectionTabNavigation",
			typeof( MaskedEditTabNavigation ),
			typeof( XamMaskedInput ),
			MaskedEditTabNavigation.NextControl,
			new PropertyChangedCallback(OnProcessPropertyChangedCallback)
		);

		/// <summary>
		/// Specifies whether to tab between sections when Tab and Shift+Tab keys are pressed.
		/// The default value is <b>NextControl</b>.
		/// </summary>
		/// <seealso cref="AllowShiftingAcrossSections"/>
		/// <seealso cref="InsertMode"/>
		/// <seealso cref="SelectAllBehavior"/>
		/// <seealso cref="AutoFillDate"/>
		public MaskedEditTabNavigation SectionTabNavigation
		{
			get
			{
				return (MaskedEditTabNavigation)this.GetValue( SectionTabNavigationProperty );
			}
			set
			{
				this.SetValue( SectionTabNavigationProperty, value );
			}
		}

		#endregion // SectionTabNavigation

		#region CreateContextMenu



		private void SetCommand( MenuItem item, MaskedInputCommandId command )
		{
			MaskedInputCommandSource commandSource = new MaskedInputCommandSource( )
			{
				CommandId = command,
				EventName = "Click",
				Target = this
			};

			Commanding.SetCommand( item, commandSource );
		}

		internal ContextMenu CreateContextMenu( )
		{
			ContextMenu menu = new ContextMenu( );
			MenuItem item;

			IInputElement commandTarget = this;

			item = new MenuItem( );
			menu.Items.Add( item );
			this.SetCommand( item, MaskedInputCommandId.Undo );
			item.CommandTarget = commandTarget;
			item.Header = ApplicationCommands.Undo.Name;

			menu.Items.Add( new Separator( ) );

			item = new MenuItem( );
			menu.Items.Add( item );
			this.SetCommand( item, MaskedInputCommandId.Cut );
			item.CommandTarget = commandTarget;
			item.Header = ApplicationCommands.Cut.Name;

			item = new MenuItem( );
			menu.Items.Add( item );
			this.SetCommand( item, MaskedInputCommandId.Copy );
			item.CommandTarget = commandTarget;
			item.Header = ApplicationCommands.Copy.Name;

			item = new MenuItem( );
			menu.Items.Add( item );
			this.SetCommand( item, MaskedInputCommandId.Paste );
			item.CommandTarget = commandTarget;
			item.Header = ApplicationCommands.Paste.Name;

			item = new MenuItem( );
			menu.Items.Add( item );
			this.SetCommand( item, MaskedInputCommandId.Delete );
			item.CommandTarget = commandTarget;
			item.Header = ApplicationCommands.Delete.Name;

			menu.Items.Add( new Separator( ) );

			item = new MenuItem( );
			menu.Items.Add( item );
			this.SetCommand( item, MaskedInputCommandId.SelectAll );
			item.CommandTarget = commandTarget;
			item.Header = ApplicationCommands.SelectAll.Name;

			return menu;
		}



		#endregion // CreateContextMenu

		#region SectionTabNavigationResolved

		internal MaskedEditTabNavigation SectionTabNavigationResolved
		{
			get
			{
				MaskedEditTabNavigation ret = this.SectionTabNavigation;

				if ( MaskedEditTabNavigation.NextSection == ret && null != _editInfo
					&& _editInfo.LastEditSection is FractionPartContinuous )
					ret = MaskedEditTabNavigation.NextControl;

				return ret;
			}
		}

		#endregion // SectionTabNavigationResolved

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
		/// <seealso cref="SectionTabNavigation"/>
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
		/// Indicates if the specified character is valid for use as a prompt character for the <see cref="XamMaskedInput"/>.
		/// </summary>
		/// <param name="promptCharacter">Character to evaluate</param>
		/// <returns>False if the character is a tab, new line or carriage return. Otherwise true is returned.</returns>
		private static bool IsValidPromptChar( char promptCharacter )
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
		public static readonly DependencyProperty SelectAllBehaviorProperty = DependencyPropertyUtilities.Register(
			"SelectAllBehavior",
			typeof( MaskSelectAllBehavior ),
			typeof( XamMaskedInput ),
			MaskSelectAllBehavior.SelectAllCharacters,
			new PropertyChangedCallback(OnProcessPropertyChangedCallback)
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
		/// <seealso cref="SectionTabNavigation"/>
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
			return DependencyPropertyUtilities.ShouldSerialize( this, SelectAllBehaviorProperty );
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
		public static readonly DependencyProperty AutoFillDateProperty = DependencyPropertyUtilities.Register(
			"AutoFillDate",
			typeof( AutoFillDate ),
			typeof( XamMaskedInput ),
			AutoFillDate.None,
			new PropertyChangedCallback(OnProcessPropertyChangedCallback)
		);

		/// <summary>
		/// Specifies whether to auto-fill empty date components when the user attempts to leave the editor.
		/// The default is <b>None</b>.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// If the user types in an incomplete date then the editor will consider the input invalid
		/// and take appropriate actions based on the <see cref="ValueInput.InvalidValueBehavior"/>
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
		/// <seealso cref="SectionTabNavigation"/>
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
			return DependencyPropertyUtilities.ShouldSerialize( this, AutoFillDateProperty );
		}

		/// <summary>
		/// Resets the AutoFillDate property to its default state.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public void ResetAutoFillDate( )
		{
			this.ClearValue( AutoFillDateProperty );
		}

		#endregion // AutoFillDate

		#region DefaultValueToDisplayTextConverter

		/// <summary>
		/// Overridden. Returns the default converter used for converting between the value and the text.
		/// </summary>
		protected override IValueConverter DefaultValueToDisplayTextConverter
		{
			get
			{
				return MaskedInputDefaultConverter.ValueToDisplayTextConverter;
			}
		}

		#endregion // DefaultValueToDisplayTextConverter

		#region DefaultValueToTextConverter

		/// <summary>
		/// Overridden. Returns the default converter used for converting between the value and the text.
		/// </summary>
		protected override IValueConverter DefaultValueToTextConverter
		{
			get
			{
				return MaskedInputDefaultConverter.ValueToTextConverter;
			}
		}

		#endregion // DefaultValueToTextConverter

		#region ExecuteCommand

		/// <summary>
		/// Executes the command associated with the specified <see cref="MaskedInputCommandId"/> value.
		/// </summary>
		/// <param name="command">The Command to execute.</param>
		/// <param name="commandParameter">An optional parameter.</param>
		/// <param name="sourceElement">The source of the command</param>
		/// <returns>True if command was executed, false if canceled.</returns>
		/// <seealso cref="MaskedInputCommandId"/>
		public bool ExecuteCommand( MaskedInputCommandId command, object commandParameter, FrameworkElement sourceElement )
		{
			var editInfo = this.EditInfo;

			// Setup some info needed by more than 1 command.
			bool shiftKeyDown = ( Keyboard.Modifiers & ModifierKeys.Shift ) != 0;
			bool ctlKeyDown = ( Keyboard.Modifiers & ModifierKeys.Control ) != 0;

			if ( !this.CanExecuteCommand( command, commandParameter ) )
				return false;

			// Determine which of our supported commands should be executed and do the associated action.
			bool handled = false;

			if ( null != editInfo && editInfo.ExecuteCommandImpl( command, commandParameter, shiftKeyDown, ctlKeyDown, sourceElement ) )
				handled = true;

			return handled;
		}

		#endregion // ExecuteCommand

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


		// JJD 4/25/07
		// Optimization - don't create a spearate context menu for every editor
		// Instead override OnContextMenuOpening
		/// <summary>
		/// Called when the ContextMenu is about to open
		/// </summary>
		/// <param name="e">The event arguments</param>
		protected override void OnContextMenuOpening( ContextMenuEventArgs e )
		{
			ContextMenu menu = this.ContextMenu;

			if ( this.ReadLocalValue( ContextMenuProperty ) != null )
			{
				if ( menu != null )
				{
					menu.Closed += new RoutedEventHandler( OnContextMenuClosed );
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
						if ( !editAreaRect.Contains( mouseLoc ) )
							return;
					}
					// ------------------------------------------------------------------------------------

					menu = this.CreateContextMenu( );
					menu.Closed += new RoutedEventHandler( OnContextMenuClosed );
					menu.PlacementTarget = this;
					menu.Placement = PlacementMode.RelativePoint;

					Point pt = new Point( );

					if ( e.OriginalSource is UIElement )
						pt = this.TranslatePoint( pt, e.OriginalSource as UIElement );

					menu.HorizontalOffset = e.CursorLeft - pt.X;
					menu.VerticalOffset = e.CursorTop - pt.Y;

					// SSP 7/19/07 BR22754
					// We need to appear as if we have keyboard focus when we are displaying context menu.
					// 
					this.ConsiderIsInEditMode( "IsDisplayingContextMenu", true );

					// This is to enable/disable menu items based on whether the associated commands can
					// be executed based on the current state of the control.
					// 
					CommandSourceManager.NotifyCanExecuteChanged( typeof( MaskedInputCommand ) );

					menu.IsOpen = true;
					e.Handled = true;

					if ( ! menu.IsOpen )
						this.ConsiderIsInEditMode( "IsDisplayingContextMenu", false );
				}
			}
		}

		private void OnContextMenuClosed( object sender, RoutedEventArgs e )
		{
			ContextMenu menu = sender as ContextMenu;

			// SSP 7/19/07 BR22754
			// We need to appear as if we have keyboard focus when we are displaying context menu.
			// 
			this.ConsiderIsInEditMode( "IsDisplayingContextMenu", false );

			if ( menu != null )
				menu.Closed -= new RoutedEventHandler( OnContextMenuClosed );
		}


		#region ICommandTarget Interface Implementation

		private MaskedInputCommandsHelper _commands;
		private MaskedInputCommandsHelper Commands
		{
			get
			{
				if ( null == _commands )
					_commands = new MaskedInputCommandsHelper( );

				return _commands;
			}
		}

		object ICommandTarget.GetParameter( CommandSource source )
		{
			return source is MaskedInputCommandSource ? this : null;
		}

		bool ICommandTarget.SupportsCommand( ICommand command )
		{
			return command is MaskedInputCommand;
		}

		internal bool CanExecuteCommand( MaskedInputCommandId command, object commandParameter )
		{
			return this.Commands.DoesMinimumStateMatch( command, this.EditInfo.GetCurrentStateLong ) ?? true;
		}

		#endregion // ICommandTarget Interface Implementation
	}

	#endregion // XamMaskedInput Class

	#region MaskedInputDefaultConverter Class

	internal class MaskedInputDefaultConverter : ValueInputDefaultConverter
	{
		private static MaskedInputDefaultConverter _valueToDisplayTextConverter;
		private static MaskedInputDefaultConverter _valueToTextConverter;

		protected MaskedInputDefaultConverter( bool isDisplayTextConverter )
			: base( isDisplayTextConverter )
		{
		}

		public new static MaskedInputDefaultConverter ValueToTextConverter
		{
			get
			{
				if ( null == _valueToTextConverter )
					_valueToTextConverter = new MaskedInputDefaultConverter( false );

				return _valueToTextConverter;
			}
		}

		public new static MaskedInputDefaultConverter ValueToDisplayTextConverter
		{
			get
			{
				if ( null == _valueToDisplayTextConverter )
					_valueToDisplayTextConverter = new MaskedInputDefaultConverter( true );

				return _valueToDisplayTextConverter;
			}
		}

		#region ConvertToTextBasedOnMask

		
		
		
		
		internal string ConvertToTextBasedOnMask( XamMaskedInput editor, object value, bool useSectionsDirectly )
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
					Type maskType = XamMaskedInput.DeduceEditAsType( sections, out hasDateSections, out hasTimeSections );
					bool hasNonDateTimeEditSections = XamMaskedInput.HasNonDateTimeEditSections( sections );

					if ( value is DateTime && typeof( DateTime ) == maskType && !hasNonDateTimeEditSections )
					{
						XamMaskedInput.SetDateTimeValue( sections, (DateTime)value, maskInfo, false );
						valueApplied = true;
					}
				}

				if ( !valueApplied )
					maskInfo.InternalRefreshValue( value );

				bool contentsEmpty = XamMaskedInput.AreAllDisplayCharsEmpty( sections );
				if ( contentsEmpty )
					return _isDisplayTextConverter ? editor.NullText : string.Empty;
				else
					return XamMaskedInput.GetText( sections, _isDisplayTextConverter ? maskInfo.DisplayMode : maskInfo.DataMode, maskInfo );
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
			ValueInput valueInput, IFormatProvider formatProvider, string format )
		{
			// SSP 8/20/08 BR35749
			// Moved handling of null into new virtual ConvertHelper virtual method so
			// derived editors have control over it. Let the base implementation handle
			// null values as it was doing before this change. Enclosed the existing code
			// into the if block.
			// 
			if ( null != value && DBNull.Value != value )
			{
				XamMaskedInput editor = valueInput as XamMaskedInput;
				Debug.Assert( null != editor );
				if ( null != editor )
				{
					if ( convertingBack )
					{
						// Converting back from display text to value.

						if ( editor.IsInEditMode && object.Equals( value, editor.Text ) && null != editor.EditInfo
							// SSP 10/24/08 TFS9442
							// Only get the value from sections if this call resulted from change the sections.
							// Otherwise this call could have resulted from set of Text property directly.
							// 
							&& editor.EditInfo._inOnTextChanged )
						{
							if ( XamMaskedInput.AreAllDisplayCharsEmpty( editor.EditInfo.Sections ) )
								
								
								
								
								return null;

							return editor.EditInfo.Value;
						}
						else
						{
							// AS 5/1/09 NA 2009 Vol 2 - Clipboard Support
							if ( _isDisplayTextConverter && object.Equals( value, editor.NullText ) )
								
								
								
								
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
									//XamMaskedInput.SetText( maskInfo.Sections, null != value ? value.ToString( ) : string.Empty, maskInfo );
									string valueAsString = null != value ? value.ToString( ) : string.Empty;
									int machedChars = XamMaskedInput.SetText( maskInfo.Sections, valueAsString, maskInfo );
									if ( machedChars < valueAsString.Length )
										return null;
									// ----------------------------------------------------------------------------

									if ( XamMaskedInput.AreAllDisplayCharsEmpty( maskInfo.Sections ) )
										
										
										
										
										return null;

									return XamMaskedInput.GetDataValue( maskInfo );
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

			return base.ConvertHelper( convertingBack, value, targetType, valueInput, formatProvider, format );
		}
	}

	#endregion // MaskedInputDefaultConverter Class
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