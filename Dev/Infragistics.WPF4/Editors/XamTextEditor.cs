using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Controls;
using Infragistics.Shared;
using Infragistics.Windows;
using Infragistics.Windows.Resizing;
using Infragistics.Windows.Controls;
using Infragistics.Windows.Licensing;
using Infragistics.Windows.Helpers;
using System.Windows.Documents;

namespace Infragistics.Windows.Editors
{

	/// <summary>
	/// An editor for editing values as strings.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// <b>XamTextEditor</b> is a value editor that can be used for editing various data types as strings.
	/// You can specify the data type being edited by setting the <see cref="ValueEditor.ValueType"/> property.
	/// </para>
	/// <para class="body">
	/// The default <b>ValueType</b> for this editor is string. However, you can set the data type to a different type such 
	/// as <see cref="DateTime"/> in which case the editor will ensure that the value entered by the user is a valid date.
	/// It will parse the input into object of type specified by the <b>ValueType</b> property. If parsing fails,
	/// the input will be considered invalid and appropriate action will be taken by the editor. The action
	/// taken is based on the <see cref="ValueEditor.InvalidValueBehavior"/> property setting. You can use the 
	/// <see cref="ValueEditor.ValueToTextConverter"/> and <see cref="TextEditorBase.ValueToDisplayTextConverter"/> 
	/// properties to specify custom conversion logic for converting between the text and the value type.
	/// </para>
	/// </remarks>
	
	
	//[ToolboxItem(true)]
	//[System.Drawing.ToolboxBitmap(typeof(XamTextEditor), AssemblyVersion.ToolBoxBitmapFolder + "TextEditor.bmp")]
	//[Description( "Value Editor for editing values in the form of text." )]
	[System.Windows.Markup.ContentProperty( "Value" )]
	public class XamTextEditor : TextEditorBase, ISupportsSelectableText
	{
		#region Member Variables

		private UltraLicense _license;
		private TextBox _lastTextBox;
		private bool _inOnTextBoxSelectionChanged; // = false;
		private int _cachedSelectionStart; // = 0;
		private int _cachedSelectionLength; // = 0;

		#endregion Member Variables

		#region Constructors

		static XamTextEditor()
		{
			//ContentControl.ContentProperty.OverrideMetadata(typeof(XamTextEditor), new FrameworkPropertyMetadata(string.Empty));
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(XamTextEditor), new FrameworkPropertyMetadata(typeof(XamTextEditor)));

			// We need to explicitly set TextAlignment on the TextBox when HorizontalContentAlignment
			// is set to a non-default value.
			Control.HorizontalContentAlignmentProperty.OverrideMetadata( typeof( XamTextEditor ), 
				new FrameworkPropertyMetadata( new PropertyChangedCallback( OnHorizontalContentAlignmentChanged ) ) );

            // JJD 10/31/08 - TFS6094/BR33963
            // Override the metadata of the TextAlignment attached property so we can refresh 
            // over TextAlignmentResolved
            Block.TextAlignmentProperty.OverrideMetadata(typeof(XamTextEditor),
				new FrameworkPropertyMetadata( new PropertyChangedCallback( OnTextAlignmentChanged ) ) );

			// SSP 6/6/07 BR23366
			// We need this in order to make the Tab and Shift+Tab navigation work properly.
			// This is similar to what inbox ComboBox does.
			// 
			KeyboardNavigation.TabNavigationProperty.OverrideMetadata( typeof( XamTextEditor ), new FrameworkPropertyMetadata( KeyboardNavigationMode.Local ) );
			KeyboardNavigation.ControlTabNavigationProperty.OverrideMetadata( typeof( XamTextEditor ), new FrameworkPropertyMetadata( KeyboardNavigationMode.None ) );
			KeyboardNavigation.DirectionalNavigationProperty.OverrideMetadata( typeof( XamTextEditor ), new FrameworkPropertyMetadata( KeyboardNavigationMode.None ) );

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="XamTextEditor"/> class
		/// </summary>
		public XamTextEditor()
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
					this._license = LicenseManager.Validate(typeof(XamTextEditor), this) as UltraLicense;
				}
				catch (System.IO.FileNotFoundException) { }
			}

			
			
			
			
			
			this.InitializeCachedPropertyValues( );
		}

		#endregion //Constructors	
    
		#region Base class overrides

			#region CanEditType

		/// <summary>
		/// Determines if the editor natively supports editing values of specified type.
		/// </summary>
		/// <param name="type">The data type to check.</param>
		/// <returns>Returns True if the editor natively supports editing values of specified type, False otherwise.</returns>
		/// <remarks>
		/// <p class="body">
		/// XamTextEditor's implementation returns True for only the string type since that's
		/// the only data type it natively renders and edits.
		/// </p>
		/// <p class="body">
		/// See ValueEditor's <see cref="ValueEditor.CanEditType"/> for more information.
		/// </p>
		/// </remarks>
		public override bool CanEditType(Type type)
		{
			return type == typeof(string);
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
		/// XamTextEditor's implementation returns True for only the string type since that's
		/// the only data type it natively renders and edits.
		/// </p>
		/// <p class="body">
		/// See ValueEditor's <see cref="ValueEditor.CanRenderType"/> for more information.
		/// </p>
		/// </remarks>
		public override bool CanRenderType(Type type)
		{
			return type == typeof(string);
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
		/// XamTextEditor's implementation returns True for horizontal dimension since the value of the editor 
		/// affects the width of the control. It returns True or False for the vertical dimension based
		/// on whether <see cref="XamTextEditor.TextWrapping"/> property is set to True or False respectively.
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

			if ( TextWrapping.NoWrap == this.TextWrapping )
				return false;
			else
				return true;
		}

			#endregion // IsExtentBasedOnValue

			#region ValidateFocusSite

		/// <summary>
		/// Validates the focus site. Returns true if the focus site is acceptable.
		/// </summary>
		/// <param name="focusSite">The focus site to validate.</param>
		/// <param name="errorMessage">If the foucs site is invalid then this out parameter will be assigned relevant error message.</param>
		/// <returns>True if the focus site is valid, False otherwise.</returns>
		/// <remarks>
		/// <para class="body">
		/// XamTextEditor's implementation of ValidateFocusSite makes sure that the focus site is a TextBox.
		/// </para>
		/// <para class="body">
		/// See ValueEditor's <see cref="ValueEditor.ValidateFocusSite"/> for more information.
		/// </para>
		/// <seealso cref="ValueEditor.FocusSite"/>
		/// </remarks>
		protected override bool ValidateFocusSite( object focusSite, out Exception errorMessage )
		{
			if ( ! base.ValidateFocusSite( focusSite, out errorMessage ) )
				return false;

			if ( ! ( focusSite is TextBox ) )
			{
				errorMessage = new NotSupportedException( ValueEditor.GetString( "LE_NotSupportedException_5", focusSite.GetType( ).Name ) );
				return false;
			}

			return true;
		}

			#endregion // ValidateFocusSite

			#region OnCreateAutomationPeer
		/// <summary>
		/// Returns an automation peer that exposes the <see cref="XamTextEditor"/> to UI Automation.
		/// </summary>
		/// <returns>A <see cref="Infragistics.Windows.Automation.Peers.Editors.XamTextEditorAutomationPeer"/></returns>
		protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer()
		{
			return new Infragistics.Windows.Automation.Peers.Editors.XamTextEditorAutomationPeer(this);
		}
			#endregion //OnCreateAutomationPeer

			#region OnFocusSiteChanged

		/// <summary>
		/// Called when the focus site changes.
		/// </summary>
		/// <seealso cref="ValueEditor.FocusSite"/>
		/// <seealso cref="ValueEditor.ValidateFocusSite"/>
		protected override void OnFocusSiteChanged( )
		{
			base.OnFocusSiteChanged( );

			if ( null != _lastTextBox )
			{
				_lastTextBox.SelectionChanged -= new RoutedEventHandler( this.OnTextBoxSelectionChanged );

				// SSP 5/10/12 TFS100047
				// Took out the binding in the template that synchronized the XamTextEditor's Text to the textbox's Text property
				// so now we have to manually synchronize them. We did this to make use of the SetCurrentValue to set the text property.
				// 
				_lastTextBox.TextChanged -= new TextChangedEventHandler( this.OnTextBoxTextChanged );

				_lastTextBox = null;
			}

			_lastTextBox = this.FocusSite as TextBox;
			if ( null != _lastTextBox )
			{
				_lastTextBox.SelectionStart = _cachedSelectionStart;
				_lastTextBox.SelectionLength = _cachedSelectionLength;

				_lastTextBox.SelectionChanged += new RoutedEventHandler( this.OnTextBoxSelectionChanged );

				// SSP 5/10/12 TFS100047
				// Took out the binding in the template that synchronized the XamTextEditor's Text to the textbox's Text property
				// so now we have to manually synchronize them. We did this to make use of the SetCurrentValue to set the text property.
				// 
				_lastTextBox.TextChanged += new TextChangedEventHandler( this.OnTextBoxTextChanged );
				this.SyncTextBoxText( );
			}
		}

			#endregion //OnFocusSiteChanged	

			#region OnTextChanged

		// SSP 5/10/12 TFS100047
		// Took out the binding in the template that synchronized the XamTextEditor's Text to the textbox's Text property
		// so now we have to manually synchronize them. We did this to make use of the SetCurrentValue to set the text property.
		// 
		/// <summary>
		/// Overridden.
		/// </summary>
		/// <param name="previousValue"></param>
		/// <param name="currentValue"></param>
		protected override void OnTextChanged( string previousValue, string currentValue )
		{
			base.OnTextChanged( previousValue, currentValue );

			this.SyncTextBoxText( );
		}

			#endregion // OnTextChanged

		#endregion //Base class overrides

		#region Properties

			#region Private/Internal Properties

				#region TextBox

		internal TextBox TextBox
		{
			get
			{
				return (TextBox)this.FocusSite;
			}
		}

				#endregion // TextBox

			#endregion // Private/Internal Properties

			#region Public Properties

				// JJD 11/29/10 - TFS58984 - Added
				#region AcceptsReturn

		/// <summary>
		/// Identifies the <see cref="AcceptsReturn"/> dependency property
		/// </summary>
		public static readonly DependencyProperty AcceptsReturnProperty = KeyboardNavigation.AcceptsReturnProperty.AddOwner(typeof(XamTextEditor));

		/// <summary>
		/// Gets/sets whether a return key will be accepted as part of the value.
		/// </summary>
		/// <seealso cref="AcceptsReturnProperty"/>
		//[Description("Gets/sets whether a return key will be accepted as part of the value.")]
		//[Category("Behavior")]
		public bool AcceptsReturn
		{
			get
			{
				return (bool)this.GetValue(XamTextEditor.AcceptsReturnProperty);
			}
			set
			{
				this.SetValue(XamTextEditor.AcceptsReturnProperty, value);
			}
		}

				#endregion //AcceptsReturn

				// JJD 11/29/10 - TFS58984 - Added
				#region AcceptsTab

		/// <summary>
		/// Identifies the <see cref="AcceptsTab"/> dependency property
		/// </summary>
		public static readonly DependencyProperty AcceptsTabProperty = DependencyProperty.Register("AcceptsTab",
			typeof(bool), typeof(XamTextEditor), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		/// <summary>
		/// Gets/sets whether a tab key will be accepted as part of the value.
		/// </summary>
		/// <seealso cref="AcceptsTabProperty"/>
		//[Description("Gets/sets whether a tab key will be accepted as part of the value.")]
		//[Category("Behavior")]
		public bool AcceptsTab
		{
			get
			{
				return (bool)this.GetValue(XamTextEditor.AcceptsTabProperty);
			}
			set
			{
				this.SetValue(XamTextEditor.AcceptsTabProperty, value);
			}
		}

				#endregion //AcceptsTab

				#region HorizontalScrollBarVisibility

		/// <summary>
		/// Identifies the <see cref="HorizontalScrollBarVisibility"/> dependency property
		/// </summary>
		public static readonly DependencyProperty HorizontalScrollBarVisibilityProperty = DependencyProperty.Register("HorizontalScrollBarVisibility",
			typeof(ScrollBarVisibility), typeof(XamTextEditor), new FrameworkPropertyMetadata(KnownBoxes.ScrollBarVisibilityHiddenBox));

		/// <summary>
		/// Determines if a horizontal scrollbar is visible.
		/// </summary>
		/// <seealso cref="HorizontalScrollBarVisibilityProperty"/>
		//[Description("Determines if a horizontal scrollbar is visible.")]
		//[Category("Behavior")]
		[Bindable(true)]
		public ScrollBarVisibility HorizontalScrollBarVisibility
		{
			get
			{
				return (ScrollBarVisibility)this.GetValue(XamTextEditor.HorizontalScrollBarVisibilityProperty);
			}
			set
			{
				this.SetValue(XamTextEditor.HorizontalScrollBarVisibilityProperty, value);
			}
		}

				#endregion //HorizontalScrollBarVisibility

				#region TextWrapping

		/// <summary>
		/// Identifies the <see cref="TextWrapping"/> dependency property
		/// </summary>
		public static readonly DependencyProperty TextWrappingProperty = DependencyProperty.Register("TextWrapping",
			typeof(TextWrapping), typeof(XamTextEditor), 
			new FrameworkPropertyMetadata(TextWrapping.NoWrap,
				
				
				
				
				
				
				
				
				new PropertyChangedCallback( OnTextWrappingChanged )
			)
		);

		
		
		
		
		
		
		//private TextWrapping _cachedTextWrapping = TextWrapping.NoWrap;
		private TextWrapping _cachedTextWrapping;

		
		
		
		
		
		
		
		
#region Infragistics Source Cleanup (Region)











#endregion // Infragistics Source Cleanup (Region)


		
		
		
		
		
		private static void OnTextWrappingChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			XamTextEditor editor = (XamTextEditor)dependencyObject;
			TextWrapping newVal = (TextWrapping)e.NewValue;

			editor._cachedTextWrapping = newVal;
		}

		/// <summary>
		/// Gets/sets whether the text will wrap if there is not enough space for it to fit on one line.
		/// </summary>
		/// <seealso cref="TextWrappingProperty"/>
		//[Description("Gets/sets whether the text will wrap if there is not enough space for it to fit on one line.")]
		//[Category("Behavior")]
		[Bindable(true)]
		public TextWrapping TextWrapping
		{
			get
			{
				// JJD 4/27/07
				// Optimization - use the locally cached property 
				//return (TextWrapping)this.GetValue(XamTextEditor.TextWrappingProperty);
				return this._cachedTextWrapping;
			}
			set
			{
				this.SetValue(XamTextEditor.TextWrappingProperty, value);
			}
		}

				#endregion //TextWrapping

				#region TextAlignment

		/// <summary>
		/// Identifies the <see cref="TextAlignment"/> dependency property
		/// </summary>
        // JJD 10/31/08 - TFS6094/BR33963
        // Instead of registering a new propewrty just addowner the Block's TextAlignment
        // attached/inherits property so we pick up settings from above.
        // Note: we will get notified of the change because we are overriding the metadata
        // in our static ctor.
        //public static readonly DependencyProperty TextAlignmentProperty = DependencyProperty.Register(
        //    "TextAlignment",
        //    typeof( TextAlignment ),
        //    typeof( XamTextEditor ),
        //    new FrameworkPropertyMetadata( TextAlignment.Left, FrameworkPropertyMetadataOptions.None,
        //        new PropertyChangedCallback( OnTextAlignmentChanged ),
        //        null )
        //    );
		public static readonly DependencyProperty TextAlignmentProperty = Block.TextAlignmentProperty.AddOwner(typeof(XamTextEditor));

		/// <summary>
		/// Specifies the text horizontal alignment. Default is to base the text alignment on
		/// the <b>HorizontalContentAlignment</b> property settings.
		/// </summary>
		//[Description( "Gets/sets the text alignment." )]
		//[Category( "Appearance" )]
		[Bindable( true )]
		public TextAlignment TextAlignment
		{
			get
			{
				return (TextAlignment)this.GetValue( TextAlignmentProperty );
			}
			set
			{
				this.SetValue( TextAlignmentProperty, value );
			}
		}

		private void UpdateTextAlignmentResolved( )
		{
			object textAlignment = null;

			object val = this.ReadLocalValue( TextAlignmentProperty );
			if ( DependencyProperty.UnsetValue != val )
				textAlignment = val;

			if ( null == textAlignment )
			{
				val = this.ReadLocalValue( HorizontalContentAlignmentProperty );
				if ( DependencyProperty.UnsetValue != val )
					textAlignment = Utils.ToTextAlignment( (HorizontalAlignment)val );
			}

			if ( null == textAlignment )
				textAlignment = this.GetValue( TextAlignmentProperty );

			Debug.Assert( null != textAlignment );
			if ( null != textAlignment )
				this.SetValue( TextAlignmentResolvedPropertyKey, textAlignment );
			else
				this.ClearValue( TextAlignmentResolvedPropertyKey );
		}

		private static void OnHorizontalContentAlignmentChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			XamTextEditor editor = (XamTextEditor)dependencyObject;
			editor.UpdateTextAlignmentResolved( );
		}

		private static void OnTextAlignmentChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			XamTextEditor editor = (XamTextEditor)dependencyObject;
			editor.UpdateTextAlignmentResolved( );
		}

		/// <summary>
		/// Returns true if the TextAlignment property is set to a non-default value.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public bool ShouldSerializeTextAlignment( )
		{
			return Utilities.ShouldSerialize( TextAlignmentProperty, this );
		}

		/// <summary>
		/// Resets the TextAlignment property to its default state.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public void ResetTextAlignment( )
		{
			this.ClearValue( TextAlignmentProperty );
		}

				#endregion // TextAlignment

				#region TextAlignmentResolved

		private static readonly DependencyPropertyKey TextAlignmentResolvedPropertyKey = DependencyProperty.RegisterReadOnly(
			"TextAlignmentResolved",
			typeof( TextAlignment ),
			typeof( XamTextEditor ),
			new FrameworkPropertyMetadata( TextAlignment.Left, FrameworkPropertyMetadataOptions.None,
				null )
			);

		/// <summary>
		/// Identifies the Read-Only <see cref="TextAlignmentResolved"/> dependency property
		/// </summary>
		public static readonly DependencyProperty TextAlignmentResolvedProperty = TextAlignmentResolvedPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the resolved text alignment.
		/// </summary>
		//[Description( "Returns the resolved text alignment." )]
		//[Category( "Appearance" )]
		[Bindable( true )]
		[Browsable( false )]
		[ReadOnly( true )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public TextAlignment TextAlignmentResolved
		{
			get
			{
				return (TextAlignment)this.GetValue( TextAlignmentResolvedProperty );
			}
		}

				#endregion // TextAlignmentResolved

				#region VerticalScrollBarVisibility

		/// <summary>
		/// Identifies the <see cref="VerticalScrollBarVisibility"/> dependency property
		/// </summary>
		public static readonly DependencyProperty VerticalScrollBarVisibilityProperty = DependencyProperty.Register("VerticalScrollBarVisibility",
			typeof(ScrollBarVisibility), typeof(XamTextEditor), new FrameworkPropertyMetadata(KnownBoxes.ScrollBarVisibilityHiddenBox));

		/// <summary>
		/// Determines if a vertical scrollbar is visible.
		/// </summary>
		/// <seealso cref="VerticalScrollBarVisibilityProperty"/>
		//[Description("Determines if a vertical scrollbar is visible.")]
		//[Category("Behavior")]
		[Bindable(true)]
		public ScrollBarVisibility VerticalScrollBarVisibility
		{
			get
			{
				return (ScrollBarVisibility)this.GetValue(XamTextEditor.VerticalScrollBarVisibilityProperty);
			}
			set
			{
				this.SetValue(XamTextEditor.VerticalScrollBarVisibilityProperty, value);
			}
		}

				#endregion //VerticalScrollBarVisibility

				#region SelectionStart

		/// <summary>
		/// Gets or sets the location of the selected text. If no text is selected, this property returns 
		/// the location of the caret.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>SelectionStart</b> returns the starting location of the current text selection. If nothing
		/// is selected, it returns the location of the caret. You can set this property to change the 
		/// selected text. Note that setting this property does not modify the contents of the control.
		/// </para>
		/// </remarks>
		//[Description( "Indicates the location of the selected text." )]
		//[Category( "Behavior" )]
		[Browsable( false )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public int SelectionStart
		{
			get
			{
				if ( null != this.TextBox )
					_cachedSelectionStart = this.TextBox.SelectionStart;

				return _cachedSelectionStart;
			}
			set
			{
				_cachedSelectionStart = value;

				if ( null != this.TextBox )
					this.TextBox.SelectionStart = _cachedSelectionStart;
			}
		}

				#endregion // SelectionStart

				#region SelectionLength

		/// <summary>
		/// Gets or sets the length of the selected text. If nothing is selected then returns 0.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>SelectionLength</b> returns the length of the currently selected text. If nothing
		/// is currently selected then returns 0.
		/// </para>
		/// <para class="body">
		/// Setting this property will modify the text that's selected. It can be used to increase
		/// or decrease the amount of text that's currently selected. Setting it to 0 deselects
		/// the selected text. Note that setting this property does not modify the contents of
		/// the control.
		/// </para>
		/// </remarks>
		//[Description( "Gets/sets the length of the text selection." )]
		//[Category( "Behavior" )]
		[Browsable( false )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public int SelectionLength
		{
			get
			{
				if ( null != this.TextBox )
					_cachedSelectionLength = this.TextBox.SelectionLength;

				return _cachedSelectionLength;
			}
			set
			{
				_cachedSelectionLength = value;

				if ( null != this.TextBox )
					this.TextBox.SelectionLength = _cachedSelectionLength;
			}
		}

				#endregion // SelectionLength

				#region TextLength

		/// <summary>
		/// Indicates the length of the current text in the editor.
		/// </summary>
		//[Description( "Indicates the length of the current text in the editor." )]
		//[Category( "Data" )]
		[ReadOnly( true )]
		[Browsable( false )]
		public int TextLength
		{
			get
			{
				string text = null != this.TextBox ? this.TextBox.Text : this.Text;
				// AS 6/4/07
				//return text.Length;
				return text != null ? text.Length : 0;
			}
		}

				#endregion // TextLength

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
		//[Description( "Gets or sets the selected text." )]
		//[Category( "Data" )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public string SelectedText
		{
			get
			{
				return null != this.TextBox ? this.TextBox.SelectedText : string.Empty;
			}
			set
			{
				if ( null != this.TextBox )
					this.TextBox.SelectedText = value;
			}
		}

				#endregion // SelectedText

			#endregion //Public Properties

		#endregion //Properties

		#region Methods

			#region Private/Internal Methods

				#region InitializeCachedPropertyValues

		
		
		
		
		
		/// <summary>
		/// Initializes the variables used to cache the dependency property values by
		/// getting the dependency property metadata for this object and getting DefaultValue
		/// of that metadata for the respective property.
		/// </summary>
		private void InitializeCachedPropertyValues( )
		{
			_cachedTextWrapping = (TextWrapping)TextWrappingProperty.GetMetadata( this ).DefaultValue;
		}

				#endregion // InitializeCachedPropertyValues

				#region OnTextBoxSelectionChanged

		internal void OnTextBoxSelectionChanged( object sender, RoutedEventArgs e )
		{
			bool origInOnTextBoxSelectionChanged = _inOnTextBoxSelectionChanged;
			_inOnTextBoxSelectionChanged = true;
			try
			{
				TextBox textBox = this.TextBox;
				Debug.Assert( null != textBox );

				if ( null != textBox )
				{
					_cachedSelectionStart = textBox.SelectionStart;
					_cachedSelectionLength = textBox.SelectionLength;
				}
			}
			finally
			{
				_inOnTextBoxSelectionChanged = origInOnTextBoxSelectionChanged;
			}
		}

				#endregion // OnTextBoxSelectionChanged

				#region OnTextBoxTextChanged

		// SSP 5/10/12 TFS100047
		// Took out the binding in the template that synchronized the XamTextEditor's Text to the textbox's Text property
		// so now we have to manually synchronize them. We did this to make use of the SetCurrentValue to set the text property.
		// 
		private void OnTextBoxTextChanged( object sender, TextChangedEventArgs e )
		{
			if ( ! this.SyncingValueProperties )
			{
				TextBox textBox = _lastTextBox;
				Debug.Assert( null != textBox );
				if ( null != textBox )
					this.Text_CurrentValue = textBox.Text;
			}
		} 

				#endregion // OnTextBoxTextChanged

				#region SyncTextBoxText

		// SSP 5/10/12 TFS100047
		// Took out the binding in the template that synchronized the XamTextEditor's Text to the textbox's Text property
		// so now we have to manually synchronize them. We did this to make use of the SetCurrentValue to set the text property.
		// 
		private void SyncTextBoxText( )
		{
			if ( null != _lastTextBox )
				_lastTextBox.Text = this.Text;
		}

				#endregion // SyncTextBoxText

			#endregion // Private/Internal Methods

			#region Public Methods

		#region SelectAll

		/// <summary>
		/// Selects all text in the editor.
		/// </summary>
		public void SelectAll( )
		{
			this.SelectionStart = 0;
			this.SelectionLength = this.TextLength;
		}

				#endregion // SelectAll

			#endregion // Public Methods

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