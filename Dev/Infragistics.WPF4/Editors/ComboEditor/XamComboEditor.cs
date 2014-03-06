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
using Infragistics.Windows.Licensing;
using Infragistics.Windows.Helpers;
using System.Windows.Automation.Peers;
using Infragistics.Windows.Automation.Peers.Editors;
using Infragistics.Windows.Commands;
using Infragistics.Windows.Controls.Events;
using Infragistics.Windows.Internal;
using System.Windows.Threading;

namespace Infragistics.Windows.Editors
{

	/// <summary>
	/// An editor that displays a list of items in a drop down from which the user can select an entry.
	/// The editor also supports entering any arbitrary value in the edit portion.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// <b>XamComboEditor</b> is a value editor that displays a list of items in a drop down from which the 
	/// user can select an entry. The editor also supports entering any arbitrary value in the edit portion.
	/// </para>
	/// <para class="body">
	/// The way you populate items in the drop-down of this editor is by creating a <see cref="ComboBoxItemsProvider"/>
	/// object and setting the editor's <see cref="XamComboEditor.ItemsProvider"/> property. ComboBoxItemsProvider
	/// exposes ItemsSource and Items properties just like an ItemsControl for specifying items either by binding
	/// to an external collection of items using ItemsSource property or directly populating the Items property.
	/// </para>
	/// <para class="body">
	/// The default <b>ValueType</b> for this editor is string. However, you can set the data type to a different type.
	/// </para>
	/// </remarks>
	
	
	//[ToolboxItem(true)]
	//[System.Drawing.ToolboxBitmap( typeof( XamComboEditor ), AssemblyVersion.ToolBoxBitmapFolder + "ComboEditor.bmp" )]
	//[Description( "Value Editor that displays a list of items in drop-down list from which to select one." )]
	[System.Windows.Markup.ContentProperty( "Value" )]
    // AS 9/5/08
    // These should have been added to document the style properties we have exposed.
    //
    [StyleTypedProperty(Property = "ComboBoxStyle", StyleTargetType = typeof(ComboBox))]
    [StyleTypedProperty(Property = "DropDownButtonStyle", StyleTargetType = typeof(System.Windows.Controls.Primitives.ToggleButton))]
    // AS 9/19/08
    [TemplatePart(Name = "PART_DropDownButton", Type = typeof(System.Windows.Controls.Primitives.ToggleButton))]
	// SSP 3/10/09 Display Value Task
	// Added DisplayValue and DisplayValueSource properties on XamComboEditor.
	// Since now we need to use ContentPresenter, we decided to rename the template
	// part to PART_Content.
	// 
    //[TemplatePart(Name = "PART_TextBlock", Type = typeof(TextBlock))]

    // JJD 4/15/10 - NA2010 Vol 2 - Added support for VisualStateManager

    [TemplateVisualState(Name = VisualStateUtilities.StateFocusedDropDown, GroupName = VisualStateUtilities.GroupFocus)]

    [TemplateVisualState(Name = VisualStateUtilities.StateEditable, GroupName = VisualStateUtilities.GroupEdit)]
    [TemplateVisualState(Name = VisualStateUtilities.StateUneditable, GroupName = VisualStateUtilities.GroupEdit)]

    [TemplatePart(Name = "PART_Content", Type = typeof(FrameworkElement))]
    public class XamComboEditor : TextEditorBase, ISupportsSelectableText, ICommandHost
	{
		#region Nested Data Structures

		#region ComboBox_ItemsSourceWrapper Class

		// SSP 1/20/09 - NAS9.1 Record Filtering
		// ComboBox will null out its SeletedValue/Text and set SelectedIndex to -1 if a reset
		// is raised from the data source its bound to, even if there's a new item the data source
		// that matches its text. We have to prevent the combo editor from responding to it
		// and also reset back the combo box's SelectedValue/Text/SelectedIndex properties back
		// to what they should be based on the combo editor's value.
		// 
		internal class ComboBox_ItemsSourceWrapper : CollectionViewProxy
			// AS 8/27/09 TFS21536
			// In the 3.0 version of the WPF framework when the ItemsSource of an ItemsControl
			// is set it uses the CollectionViewSource.GetDefaultCollectionView to obtain the 
			// underlying collection view it will maintain for the items. This delegates off to 
			// the DataBindEngine class to get a ViewRecord. In 3.0, this wouldn't check the 
			// collection to see if it implemented ICollectionView. Instead it would see that it 
			// implements IList and then ultimately create a ListCollectionView around our 
			// collection. This class seems to have a bug (even in the current impl) whereby 
			// when its Refresh method is invoked it ALWAYS does a lock on the SyncRoot of the 
			// underlying collection without first checking if its IsSynchronized was true. 
			// Well when our SyncRoot is requested we delegate off to the SyncRoot of the 
			// underlying CollectionView which in the case of the xamComboEditor's usage is 
			// the ItemCollection of the ComboBox used by the ComboBoxItemsProvider. Well that 
			// class will throw a NotSupportedException when the ItemsSource is bound. In 3.5, 
			// this doesn't happen because in the GetViewRecord they first check to see if the 
			// collection implements ICollectionView and if so create a wrapper around that.
			// Rather than try to catch the exception, we're going to implement ICollectionViewFactory.
			// In 3.0, this is checked before it checks if the collection is an IList and we can 
			// just return this collectionview. In 3.5 and later, it wouldn't even get to that 
			// logic because it would have exited when it found that the collection itself is 
			// an ICollectionView.
			// 
			, ICollectionViewFactory
		{
			private XamComboEditor _comboEditor;

			internal ComboBox_ItemsSourceWrapper( XamComboEditor comboEditor, CollectionView itemsSource )
				: base( itemsSource )
			{
				Utils.ValidateNull( comboEditor );
				_comboEditor = comboEditor;
			}

			protected override void OnSourceCollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
			{
				bool origInInitializeComboBoxValue = _comboEditor._isInitializingComboBox;
				_comboEditor._isInitializingComboBox = true;

				try
				{
					base.OnSourceCollectionChanged( sender, e );
				}
				finally
				{
					_comboEditor._isInitializingComboBox = false;
				}
			}

			// AS 8/27/09 TFS21536
			#region ICollectionViewFactory Members

			ICollectionView ICollectionViewFactory.CreateView()
			{
				return this;
			}

			#endregion
		}

		#endregion // ComboBox_ItemsSourceWrapper Class

		#region ValueChangeSource Enum

		private enum ValueChangeSource
		{
			Value,
			Text,
			SelectedIndex,
			SelectedItem,
			ComboBoxText,
			ComboBoxSelectedIndex,
		}

		#endregion // ValueChangeSource Enum

		#endregion // Nested Data Structures

		#region Member Variables

		private UltraLicense _license;
		private ComboBox _lastComboBox;
		private TextBox _lastComboBoxTextBox;
		private bool _inOnComboBoxSelectionChanged;
		private int _cachedSelectionStart;
		private int _cachedSelectionLength;
		private bool _isInitializingComboBox;

		// SSP 1/10/08 BR29141
		// Added ItemsSource property.
		// 
		private ComboBoxItemsProvider _internalItemsProvider;

		private bool _inOnValueChanged_SyncProperties;

        // AS 9/10/08 NA 2008 Vol 2
        private bool _isClosePending;

		// SSP 3/10/09 Display Value Task
		// In order to workaround an issue in the ComboBox where it erronously uses a string template
		// for dependency objects even if there's data template defined for it, we have to explicitly
		// set the ItemTemplate property.
		// 
		private bool _itemTemplateSetOnComboBox;

		// SSP 4/18/11 - Coerce SelectedItem Fix
		// 
		private bool _selectedItemCoerced;

		// SSP 1/16/12 TFS59404
		// 
		private bool _selectedIndexCoerced;

		// JM 06-23-11 TFS67853
		private static DependencyProperty s_IsTextSearchCaseSensitiveProperty;

		// SSP 11/9/11 TFS92225
		// 
		private bool _bypassNextComboTextChanged;

        #endregion Member Variables

		#region Constructors

		static XamComboEditor( )
		{
			// SSP 10/22/09 TFS23844
			// Default value type for the combo editor should be object since the values come from the list and
			// it could be any type.
			// 
			ValueTypeProperty.OverrideMetadata( typeof( XamComboEditor ), new FrameworkPropertyMetadata( typeof( object ) ) );

			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata( typeof( XamComboEditor ), new FrameworkPropertyMetadata( typeof( XamComboEditor ) ) );

			// We need to explicitly set TextAlignment on the TextBox when HorizontalContentAlignment
			// is set to a non-default value.
			Control.HorizontalContentAlignmentProperty.OverrideMetadata( typeof( XamComboEditor ),
				new FrameworkPropertyMetadata( new PropertyChangedCallback( OnHorizontalContentAlignmentChanged ) ) );

			// SSP 6/6/07 BR23366
			// We need this in order to make the Tab and Shift+Tab navigation work properly.
			// This is similar to what inbox ComboBox does.
			// 
			KeyboardNavigation.TabNavigationProperty.OverrideMetadata( typeof( XamComboEditor ), new FrameworkPropertyMetadata( KeyboardNavigationMode.Local ) );
			KeyboardNavigation.ControlTabNavigationProperty.OverrideMetadata( typeof( XamComboEditor ), new FrameworkPropertyMetadata( KeyboardNavigationMode.None ) );
			KeyboardNavigation.DirectionalNavigationProperty.OverrideMetadata( typeof( XamComboEditor ), new FrameworkPropertyMetadata( KeyboardNavigationMode.None ) );

			
			
			VerticalContentAlignmentProperty.OverrideMetadata( typeof( XamComboEditor ), new FrameworkPropertyMetadata( VerticalAlignment.Center ) );

			// JM 06-23-11 TFS67853
			// Cache the DependencyProperty for the IsTextSearchCaseSensitiveProperty since
			// this property were only introduced in 3.5 we can't access these properties 
			// directly. 
			DependencyPropertyDescriptor pd = DependencyPropertyDescriptor.FromName("IsTextSearchCaseSensitive", typeof(ComboBox), typeof(ComboBox));
			if (pd != null)
				s_IsTextSearchCaseSensitiveProperty = pd.DependencyProperty;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="XamComboEditor"/> class
		/// </summary>
		public XamComboEditor( )
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
					this._license = LicenseManager.Validate( typeof( XamComboEditor ), this ) as UltraLicense;
				}
				catch ( System.IO.FileNotFoundException ) { }
			}

			// AS 1/9/08 BR29510
			// We need this property so that we can set it on the ComboBox within the editor
			// so it has a backward pointer to this control. However, we do not need to set
			// it on the combo editor itself - only on the ComboBox. Setting it on the control
			// is causing Blend to get into a recursive situation which results in a stack
			// overflow.
			//
			//this.SetValue( XamComboEditor.ComboEditorProperty, this );

			
			
			
			
			
			this.InitializeCachedPropertyValues( );
		}

		#endregion //Constructors

		#region Events

		#region ExecutingCommand

		/// <summary>
		/// Event ID for the <see cref="ExecutingCommand"/> routed event
		/// </summary>
		/// <seealso cref="ExecutingCommand"/>
		/// <seealso cref="OnExecutingCommand"/>
		/// <seealso cref="ExecutingCommandEventArgs"/>
		public static readonly RoutedEvent ExecutingCommandEvent =
			EventManager.RegisterRoutedEvent( "ExecutingCommand", RoutingStrategy.Bubble, typeof( EventHandler<ExecutingCommandEventArgs> ), typeof( XamComboEditor ) );

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
			args.RoutedEvent = XamComboEditor.ExecutingCommandEvent;
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
				base.AddHandler( XamComboEditor.ExecutingCommandEvent, value );
			}
			remove
			{
				base.RemoveHandler( XamComboEditor.ExecutingCommandEvent, value );
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
			EventManager.RegisterRoutedEvent( "ExecutedCommand", RoutingStrategy.Bubble, typeof( EventHandler<ExecutedCommandEventArgs> ), typeof( XamComboEditor ) );

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
			args.RoutedEvent = XamComboEditor.ExecutedCommandEvent;
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
				base.AddHandler( XamComboEditor.ExecutedCommandEvent, value );
			}
			remove
			{
				base.RemoveHandler( XamComboEditor.ExecutedCommandEvent, value );
			}
		}

		#endregion //ExecutedCommand

		#region DropDownOpened

		/// <summary>
		/// Event ID for the <see cref="DropDownOpened"/> routed event
		/// </summary>
		/// <seealso cref="OnDropDownOpened"/>
		/// <seealso cref="DropDownClosed"/>
		public static readonly RoutedEvent DropDownOpenedEvent =
			EventManager.RegisterRoutedEvent( "DropDownOpened", RoutingStrategy.Bubble, typeof( EventHandler<RoutedEventArgs> ), typeof( XamComboEditor ) );

		/// <summary>
		/// This method is called when the drop-down list is opened. It raises <see cref="DropDownOpened"/> event.
		/// </summary>
		/// <seealso cref="DropDownOpened"/>
		/// <seealso cref="DropDownClosed"/>
		protected virtual void OnDropDownOpened( RoutedEventArgs args )
		{
			// MD 7/16/10 - TFS26592
			// Call off to the new helper method to raise the event. 
			//this.RaiseEvent( args );
			this.RaiseEventHelper(args);
		}

		internal void RaiseDropDownOpened( RoutedEventArgs args )
		{
			args.RoutedEvent = XamComboEditor.DropDownOpenedEvent;
			args.Source = this;
			this.OnDropDownOpened( args );
		}

		/// <summary>
		/// Occurs when the drop-down list is opened.
		/// </summary>
		/// <seealso cref="OnDropDownOpened"/>
		/// <seealso cref="DropDownClosed"/>
		//[Description( "Occurs before a command is performed" )]
		//[Category( "Behavior" )]
		public event EventHandler<RoutedEventArgs> DropDownOpened
		{
			add
			{
				base.AddHandler( XamComboEditor.DropDownOpenedEvent, value );
			}
			remove
			{
				base.RemoveHandler( XamComboEditor.DropDownOpenedEvent, value );
			}
		}

		#endregion //DropDownOpened

		#region DropDownClosed

		/// <summary>
		/// Event ID for the <see cref="DropDownClosed"/> routed event
		/// </summary>
		/// <seealso cref="OnDropDownClosed"/>
		/// <seealso cref="DropDownClosed"/>
		public static readonly RoutedEvent DropDownClosedEvent =
			EventManager.RegisterRoutedEvent( "DropDownClosed", RoutingStrategy.Bubble, typeof( EventHandler<RoutedEventArgs> ), typeof( XamComboEditor ) );

		/// <summary>
		/// This method is called when the drop-down list is closed. It raises <see cref="DropDownClosed"/> event.
		/// </summary>
		/// <seealso cref="DropDownClosed"/>
		/// <seealso cref="DropDownClosed"/>
		protected virtual void OnDropDownClosed( RoutedEventArgs args )
		{
			// MD 7/16/10 - TFS26592
			// Call off to the new helper method to raise the event. 
			//this.RaiseEvent( args );
			this.RaiseEventHelper(args);
		}

		internal void RaiseDropDownClosed( RoutedEventArgs args )
		{
			args.RoutedEvent = XamComboEditor.DropDownClosedEvent;
			args.Source = this;
			this.OnDropDownClosed( args );
		}

		/// <summary>
		/// Occurs when the drop-down list is closed.
		/// </summary>
		/// <seealso cref="OnDropDownClosed"/>
		/// <seealso cref="DropDownClosed"/>
		//[Description( "Occurs when the drop-down list is closed" )]
		//[Category( "Behavior" )]
		public event EventHandler<RoutedEventArgs> DropDownClosed
		{
			add
			{
				base.AddHandler( XamComboEditor.DropDownClosedEvent, value );
			}
			remove
			{
				base.RemoveHandler( XamComboEditor.DropDownClosedEvent, value );
			}
		}

		#endregion //DropDownClosed

		#endregion // Events

		#region Base class overrides

		#region Properties

		#region DefaultValueToDisplayTextConverter

		/// <summary>
		/// Returns the default converter used for converting between the value and the text.
		/// </summary>
		protected override IValueConverter DefaultValueToDisplayTextConverter
		{
			get
			{
				// SSP 4/28/11 TFS65229
				// Use the new DisplayTextConverter.
				// 
				//return ComboBoxItemsProvider.ComboEditorDefaultConverter.Converter;
				return ComboBoxItemsProvider.ComboEditorDefaultConverter.DisplayTextConverter;
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
				return ComboBoxItemsProvider.ComboEditorDefaultConverter.Converter;
			}
		}

		#endregion // DefaultValueToTextConverter

        // AS 9/5/08 NA 2008 Vol 2
        #region HasDropDown
        internal override bool HasDropDown
        {
            get { return true; }
        }
        #endregion //HasDropDown

        // AS 9/5/08 NA 2008 Vol 2
        #region HasOpenDropDown
        internal override bool HasOpenDropDown
        {
            get
            {
                return this.IsDropDownOpen;
            }
        }
        #endregion //HasOpenDropDown

        

        #endregion // Properties

		#region Methods

		#region CanEditType

		/// <summary>
		/// Determines if the editor natively supports editing values of specified type.
		/// </summary>
		/// <param name="type">The data type to check.</param>
		/// <returns>Returns True if the editor natively supports editing values of specified type, False otherwise.</returns>
		/// <remarks>
		/// <p class="body">
		/// XamComboEditor's implementation returns True for only the string type since that's
		/// the only data type it natively renders and edits.
		/// </p>
		/// <p class="body">
		/// See ValueEditor's <see cref="ValueEditor.CanEditType"/> for more information.
		/// </p>
		/// </remarks>
		public override bool CanEditType( Type type )
		{
			return type == typeof( string );
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
		/// XamComboEditor's implementation returns True for only the string type since that's
		/// the only data type it natively renders and edits.
		/// </p>
		/// <p class="body">
		/// See ValueEditor's <see cref="ValueEditor.CanRenderType"/> for more information.
		/// </p>
		/// </remarks>
		public override bool CanRenderType( Type type )
		{
			return type == typeof( string );
		}

		#endregion //CanRenderType

		#region ConvertTextToValueHelper

		// SSP 8/20/08 BR35749
		// Overrode ConvertTextToValueHelper method.
		// 
		internal override bool ConvertTextToValueHelper( string text, out object value, out Exception error )
		{
			if ( !base.ConvertTextToValueHelper( text, out value, out error ) )
			{
				// If there's an entry in the items provider with 'null' as the data value then
				// the base class will consider it invalid. However since it's valid because 
				// there's an entry in items provider, return true here.
				// 
				if ( null == value )
				{
					ComboBoxItemsProvider ip = this.ItemsProvider;
					if ( null != ip )
					{
						object listItem;
						// SSP 2/20/09 TFS13641
						// Added support for duplicate data values and/or display texts - basically multiple 
						// items with the same data value and/or display text. Added preferredIndex parameter.
						// 
						// --------------------------------------------------------------------------------------
						//int index = ip.FindListItemFromDisplayText( text, false, 0, out listItem );
						int preferredIndex = this.SelectedIndex;
						int index = ip.FindListItemFromDisplayText( text, false, 0, preferredIndex, out listItem );
						// --------------------------------------------------------------------------------------

						if ( index >= 0 )
							return true;
					}
				}

				return false;
			}

			return true;
		}

		#endregion // ConvertTextToValueHelper

		#region InitializeValueProperties

		// SSP 1/3/07 BR27394
		// 
		/// <summary>
		/// Initializes the value properties. This synchronizes all the value properties if one of
		/// them is set in xaml since we delay syncrhonization until after initialization in case
		/// other settings in xaml effect how they are synchronized.
		/// </summary>
		internal override void InitializeValueProperties( )
		{
			// SSP 1/7/08 BR29457
			// 
			//if ( DependencyProperty.UnsetValue != this.ReadLocalValue( SelectedIndexProperty ) )
			
			
			
			if ( Utils.IsValuePropertySet( SelectedIndexProperty, this ) && this.SelectedIndex >= 0 )
			{
				this.CoerceValue( SelectedIndexProperty );

				
				
				
				
				
				this.SyncValueProperties( SelectedIndexProperty, this.SelectedIndex );
			}
			// SSP 9/11/09 TFS18713
			// 
			else if ( Utils.IsValuePropertySet( SelectedItemProperty, this ) && null != this.SelectedItem )
			{
				this.SyncValueProperties( SelectedItemProperty, this.SelectedItem );
			}
			else
			{
				base.InitializeValueProperties( );

				
				
				
				
				
			}
		}

		#endregion // InitializeValueProperties

		#region IsExtentBasedOnValue

		/// <summary>
		/// Indicates whether the desired width or the height of the editor is based on the value.
		/// </summary>
		/// <param name="orientation">Orientation of the extent being evaluated. Horizontal indicates the width and vertical indicates the height.</param>
		/// <returns>True if extent is based on the value.</returns>
		/// <remarks>
		/// <para class="body">
		/// XamComboEditor's implementation returns True for horizontal dimension since the value of the editor 
		/// affects the width of the control. It returns False for the vertical dimension.
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

		#region OnCoerceText

		// SSP 9/25/09 TFS19412
		// 
		/// <summary>
		/// Called from the <see cref="ValueEditor.Text"/> property's CoerceValue handler.
		/// </summary>
		/// <param name="text">The text to coerce</param>
		/// <returns>Coerced value</returns>
		protected override string OnCoerceText( string text )
		{
			string ret = base.OnCoerceText( text );

			this.OnCoerceValueProperty( TextProperty, text );

			return ret;
		}

		#endregion // OnCoerceText

		#region OnCoerceValue

		// SSP 9/25/09 TFS19412
		// 
		/// <summary>
		/// Called from the <see cref="ValueEditor.Value"/> property's CoerceValue handler.
		/// </summary>
		/// <param name="val"></param>
		/// <returns></returns>
		protected override object OnCoerceValue( object val )
		{
			object ret = base.OnCoerceValue( val );

			this.OnCoerceValueProperty( ValueProperty, val );

			return ret;
		}

		#endregion // OnCoerceValue

        #region OnCreateAutomationPeer

        /// <summary>
		/// Returns an automation peer that exposes the <see cref="XamComboEditor"/> to UI Automation.
		/// </summary>
		/// <returns>A <see cref="Infragistics.Windows.Automation.Peers.Editors.XamComboEditorAutomationPeer"/></returns>
		protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer( )
		{
			return new Infragistics.Windows.Automation.Peers.Editors.XamComboEditorAutomationPeer( this );
		}

		#endregion //OnCreateAutomationPeer

		#region OnEditModeStarted

		/// <summary>
		/// This method is called when the control has just entered edit mode. This method raises 
		/// <see cref="ValueEditor.EditModeStarted"/> event.
		/// </summary>
		/// <remarks>
		/// <seealso cref="ValueEditor.EditModeStarted"/>
		/// </remarks>
		protected override void OnEditModeStarted( EditModeStartedEventArgs args )
		{
			this.InitializeComboBoxValue( false );

			base.OnEditModeStarted( args );
		}

		#endregion // OnEditModeStarted

		#region OnFocusSiteChanged

		/// <summary>
		/// Called when the focus site changes.
		/// </summary>
		/// <seealso cref="ValueEditor.FocusSite"/>
		/// <seealso cref="ValueEditor.ValidateFocusSite"/>
		protected override void OnFocusSiteChanged( )
		{
			base.OnFocusSiteChanged( );

			ComboBox oldComboBox = _lastComboBox;

			_lastComboBox = this.FocusSite as ComboBox;

			this.OnComboBoxChanged( oldComboBox );
		}

		#endregion //OnFocusSiteChanged

		#region OnMouseLeftButtonDown

        
#region Infragistics Source Cleanup (Region)




















#endregion // Infragistics Source Cleanup (Region)

		#endregion // OnMouseLeftButtonDown

		#region OnPreviewKeyDown

		// SSP 11/16/10 TFS33587
		// Overrode OnPreviewKeyDown so when the editor is read-only, we handle certain keys to
		// prevent the combobox from changing its values. The issue is that the combobox doesn't 
		// have any way to make it read-only.
		// 
		/// <summary>
		/// Called to preview key-down.
		/// </summary>
		/// <param name="e">Event args.</param>
		protected override void OnPreviewKeyDown( KeyEventArgs e )
		{
			if ( this.IsInEditMode && this.IsReadOnly )
			{
				switch ( e.Key )
				{
					case Key.Down:
					case Key.Up:
					case Key.Left:
					case Key.Right:
					case Key.PageDown:
					case Key.PageUp:
					case Key.F2:
					case Key.Oem5:
					case Key.End:
					case Key.Home:
						e.Handled = true;
						break;
				}
			}

			base.OnPreviewKeyDown( e );
		}

		#endregion // OnPreviewKeyDown

		#region OnPreviewMouseLeftButtonDown

        
#region Infragistics Source Cleanup (Region)




















#endregion // Infragistics Source Cleanup (Region)

        #endregion // OnPreviewMouseLeftButtonDown

        #region OnPropertyChanged

        /// <summary>
		/// Called when a dependency property changes.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnPropertyChanged( DependencyPropertyChangedEventArgs e )
		{
			base.OnPropertyChanged( e );

			DependencyProperty prop = e.Property;

			if ( UIElement.IsMouseOverProperty == prop
				|| XamComboEditor.DropDownButtonDisplayModeProperty == prop
				|| ValueEditor.IsInEditModeProperty == prop )
			{
				this.UpdateDropDownButtonVisibility( );
			}
			else if ( FrameworkElement.ActualWidthProperty == prop
				|| XamComboEditor.MinDropDownWidthProperty == prop 
				|| XamComboEditor.EditPortionWidthProperty == prop )
			{
				this.UpdateMinDropDownWidthResolved( );
			}
			// SSP 3/10/09 Display Value Task
			// Added DisplayValue and DisplayValueSource properties on XamComboEditor.
			// 
			else if ( DisplayTextProperty == prop )
			{
				this.InitializeDisplayValue( );
			}
		}

		#endregion // OnPropertyChanged

        #region SetVisualState


        // JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
        /// <summary>
        /// Called to set the VisualStates of the editor
        /// </summary>
        /// <param name="useTransitions">Determines whether transitions should be used.</param>
        protected override void SetVisualState(bool useTransitions)
        {
            base.SetVisualState(useTransitions);

            // Set Edit states
            if (this.IsEditable && this.IsEditingAllowed)
                VisualStateManager.GoToState(this, VisualStateUtilities.StateEditable, useTransitions);
            else
                VisualStateManager.GoToState(this, VisualStateUtilities.StateUneditable, useTransitions);
        }


        #endregion //SetVisualState	

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
			error = null;

			ValueChangeSource source;

			if ( ValueEditor.ValueProperty == prop )
			{
				source = ValueChangeSource.Value;
			}
			else if ( ValueEditor.TextProperty == prop )
			{
				source = ValueChangeSource.Text;
			}
			else if ( XamComboEditor.SelectedItemProperty == prop )
			{
				source = ValueChangeSource.SelectedItem;
			}
			else if ( XamComboEditor.SelectedIndexProperty== prop )
			{
				source = ValueChangeSource.SelectedIndex;
			}
			else
			{
				Debug.Assert( false, "Unknown property." );
				return base.SyncValuePropertiesOverride( prop, newValue, out error );
			}

			// SSP 6/28/11 TFS63736
			// Pass true for the new calledFromSyncValuePropertiesOverride parameter.
			// 
			//this.OnValueChanged_SyncProperties( source, newValue );
			this.OnValueChanged_SyncProperties( source, newValue, true );

			return true;
		}

		#endregion // SyncValuePropertiesOverride

        // AS 9/10/08 NA 2008 Vol 2
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

        #region ShouldToggleDropDown
        internal override bool ShouldToggleDropDown(MouseButtonEventArgs e)
        {
            bool toggleDropDown = false;

            // AS 9/3/08 NA 2008 Vol 2
            // This portion of the MouseLeftButtonDown_ToggleDropDownHelper does not
            // make sense in the base class.
            //
            // When in drop down list mode (IsEditable is false), clicking anywhere in the editor
            // should drop down the drop-down.
            // 
            if (!this.IsEditable)
            {
                const string TEXT_BLOCK_NAME = "TextBlock";
                // AS 9/19/08
                // Made this a part but continued to look for this name for backward compatibility.
                //
                //FrameworkElement editPortion = this.GetTemplateChild(TEXT_BLOCK_NAME) as FrameworkElement;
                const string TEXT_BLOCK_PARTNAME = "PART_TextBlock";
				// SSP 3/10/09 Display Value Task
				// Added DisplayValue and DisplayValueSource properties on XamComboEditor. Use those
				// instead of the DisplayText. This also means that we can't use TextBlock since 
				// DisplayValue is an object type and can contain anything. Changed to use 
				// ContentPresenter instead of TextBlock.
				// 
                //FrameworkElement editPortion = (this.GetTemplateChild(TEXT_BLOCK_PARTNAME) ?? this.GetTemplateChild(TEXT_BLOCK_NAME)) as FrameworkElement;
				const string CONTENT_PARTNAME = "PART_Content";
				FrameworkElement editPortion = ( this.GetTemplateChild( CONTENT_PARTNAME )
					?? this.GetTemplateChild(TEXT_BLOCK_PARTNAME) ?? this.GetTemplateChild(TEXT_BLOCK_NAME) ) as FrameworkElement;

                if (null != editPortion)
                    editPortion = Utilities.GetParent(editPortion) as FrameworkElement;
                if (null != editPortion && Utils.IsMouseOverElement(editPortion, e))
                {
                    toggleDropDown = true;
                }
                else
                {
                    ComboBox comboBox = this.ComboBox;
                    if (null != comboBox)
                    {
                        if (comboBox.ActualWidth <= 0 || comboBox.ActualHeight <= 0)
                            this.UpdateLayout();

                        if (Utils.IsMouseOverElement(comboBox, e))
                            toggleDropDown = true;
                    }
                }
            }

            if (false == toggleDropDown)
                toggleDropDown = base.ShouldToggleDropDown(e);

            return toggleDropDown;
        }
        #endregion //ShouldToggleDropDown

		#region ValidateCurrentValue

		// SSP 6/28/11 TFS63736
		// Overwrote ValidateCurrentValue method.
		// 
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
			if ( ! base.ValidateCurrentValue( out error ) )
				return false;

			if ( this.LimitToList )
			{
				if ( this.SelectedIndex < 0 && ( ! Utils.IsValueEmpty( this.Value ) || ! Utils.IsValueEmpty( this.Text ) ) )
				{
					error = new Exception( "Value must be one of the items in the drop-down." );
					return false;
				}
			}

			return true;
		}

		#endregion // ValidateCurrentValue

		#region ValidateFocusSite

		/// <summary>
		/// Validates the focus site. Returns true if the focus site is acceptable.
		/// </summary>
		/// <param name="focusSite">The focus site to validate.</param>
		/// <param name="errorMessage">If the foucs site is invalid then this out parameter will be assigned relevant error message.</param>
		/// <returns>True if the focus site is valid, False otherwise.</returns>
		/// <remarks>
		/// <para class="body">
		/// XamComboEditor's implementation of ValidateFocusSite makes sure that the focus site is a TextBox.
		/// </para>
		/// <para class="body">
		/// See ValueEditor's <see cref="ValueEditor.ValidateFocusSite"/> for more information.
		/// </para>
		/// <seealso cref="ValueEditor.FocusSite"/>
		/// </remarks>
		protected override bool ValidateFocusSite( object focusSite, out Exception errorMessage )
		{
			if ( !base.ValidateFocusSite( focusSite, out errorMessage ) )
				return false;

			if ( !( focusSite is ComboBox ) )
			{
				errorMessage = new NotSupportedException( ValueEditor.GetString( "LE_NotSupportedException_5", focusSite.GetType( ).Name ) );
				return false;
			}

			return true;
		}

		#endregion // ValidateFocusSite

		#endregion // Methods

		#endregion //Base class overrides

		#region Properties

		#region Private/Internal Properties

		#region EditPortionWidth

		/// <summary>
		/// Identifies the <see cref="EditPortionWidth"/> dependency property. This property indicates
		/// the width of the edit part of the editor, especially relevant with ComboEditorTool where
		/// there is a label, icon etc...
		/// </summary>
		private static readonly DependencyProperty EditPortionWidthProperty = DependencyProperty.Register(
			"EditPortionWidth",
			typeof( double ),
			typeof( XamComboEditor ),
			new FrameworkPropertyMetadata( Double.NaN )
			);

		private double EditPortionWidth
		{
			get
			{
				return (double)this.GetValue( EditPortionWidthProperty );
			}
			set
			{
				this.SetValue( EditPortionWidthProperty, value );
			}
		}

		#endregion // EditPortionWidth

		#region TextBox

		internal TextBox TextBox
		{
			get
			{
				return _lastComboBoxTextBox;
			}
		}

		#endregion // TextBox

		#endregion // Private/Internal Properties

		#region Public Properties

		#region ComboBox

		/// <summary>
		/// Returns the underlying ComboBox used to perform the editing. Only valid when the editor is in edit mode.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// This property returns the underlying ComboBox used to perform the editing. This property is
		/// only valid during edit mode. When not in edit mode, this property returns null. You
		/// can find out if the editor is in edit mode using <see cref="ValueEditor.IsInEditMode"/> property.
		/// </para>
		/// <para class="body">
		/// <b>NOTE:</b> You should not use ComboBox property to set the drop-down items. Instead set the 
		/// XamComboEditor's <see cref="XamComboEditor.ItemsProvider"/> or <see cref="XamComboEditor.ItemsSource"/>
		/// property.
		/// </para>
		/// </remarks>
		[Browsable( false )]
		public ComboBox ComboBox
		{
			get
			{
				return _lastComboBox;
			}
		}

		#endregion // ComboBox

		#region ComboBoxStyle

		/// <summary>
		/// Identifies the <see cref="ComboBoxStyle"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ComboBoxStyleProperty = DependencyProperty.Register(
			"ComboBoxStyle",
			typeof( Style ),
			typeof( XamComboEditor ),
			new FrameworkPropertyMetadata( null, FrameworkPropertyMetadataOptions.None,
				new PropertyChangedCallback( OnComboBoxStyleChanged ),
				null )
			);

		/// <summary>
		/// Used for setting the Style of the underlying ComboBox used for editing. Default value is null.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// Default value of this property is null. You can use this property to specify a Style object to use for the
		/// underlying ComboBox used for editing. The value specified on this property will be set on the ComboBox'
		/// Style property.
		/// </para>
		/// <seealso cref="DropDownButtonDisplayMode"/>
		/// </remarks>
		//[Description( "Used for setting the Style of the underlying ComboBox" )]
		//[Category( "Behavior" )]
		[Bindable( true )]
		public Style ComboBoxStyle
		{
			get
			{
				return (Style)this.GetValue( ComboBoxStyleProperty );
			}
			set
			{
				this.SetValue( ComboBoxStyleProperty, value );
			}
		}

		private static void OnComboBoxStyleChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			Style newVal = (Style)e.NewValue;
		}

		/// <summary>
		/// Returns true if the ComboBoxStyle property is set to a non-default value.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public bool ShouldSerializeComboBoxStyle( )
		{
			return Utilities.ShouldSerialize( ComboBoxStyleProperty, this );
		}

		/// <summary>
		/// Resets the ComboBoxStyle property to its default state.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public void ResetComboBoxStyle( )
		{
			this.ClearValue( ComboBoxStyleProperty );
		}

		#endregion // ComboBoxStyle

		#region ComboBoxStyleKey

		/// <summary>
		/// Returns the key of the default style of the ComboBox that the XamComboEditor uses.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// You can create a Style object and add it to the Resources collection of an ancestor
		/// element of the XamComboEditor with this key to override the style of the ComboBox used
		/// by the XamComboEditor, without completely resplacing it.
		/// </para>
		/// <seealso cref="ComboEditorComboBoxStyleKey"/>
		/// </remarks>
		protected virtual ResourceKey ComboBoxStyleKey
		{
			get
			{
				return ComboEditorComboBoxStyleKey;
			}
		}

		#endregion // ComboBoxStyleKey

		#region ComboEditor

		/// <summary>
		/// For internal use only. This property is used from ComboBox template as a convenient
		/// way of getting the XamComboEditor associated with it.
		/// </summary>
		[Browsable( false ), EditorBrowsable( EditorBrowsableState.Never )]
		public static readonly DependencyProperty ComboEditorProperty = DependencyProperty.Register(
			"ComboEditor",
			typeof( XamComboEditor ),
			typeof( XamComboEditor ),
			new FrameworkPropertyMetadata( null, FrameworkPropertyMetadataOptions.None,
				null )
			);

		/// <summary>
		/// For internal use only. See <see cref="ComboEditorProperty"/> for more info.
		/// </summary>
		/// <param name="element">The target element.</param>
		/// <returns>The value of the property.</returns>
		/// <remarks>
		/// <seealso cref="ComboEditorProperty"/>
		/// </remarks>
		[EditorBrowsable( EditorBrowsableState.Never ), Browsable( false )]
		public static XamComboEditor GetComboEditor( DependencyObject element )
		{
			return (XamComboEditor)element.GetValue( ComboEditorProperty );
		}

		/// <summary>
		/// For internal use only. See <see cref="ComboEditorProperty"/> for more info.
		/// </summary>
		/// <param name="element">The target element.</param>
		/// <param name="value">The value to set on the property.</param>
		/// <returns>The value of the property.</returns>
		/// <remarks>
		/// <seealso cref="ComboEditorProperty"/>
		/// </remarks>
		[EditorBrowsable( EditorBrowsableState.Never ), Browsable( false )]
		public static void SetComboEditor( DependencyObject element, XamComboEditor value )
		{
			element.SetValue( ComboEditorProperty, value );
		}

		#endregion // ComboEditor

		#region ComboEditorComboBoxStyleKey

		/// <summary>
		/// The key used to identify the combo box style used for the ComboBox inside the XamComboEditor
		/// </summary>
		public static readonly ResourceKey ComboEditorComboBoxStyleKey = new StaticPropertyResourceKey( typeof( XamComboEditor ), "ComboEditorComboBoxStyleKey" );

		#endregion // ComboEditorComboBoxStyleKey

		#region DisplayValue

		// SSP 3/10/09 Display Value Task
		// Added DisplayValue and DisplayValueSource properties on XamComboEditor.
		// 

		/// <summary>
		/// Identifies the property key for read-only <see cref="DisplayValue"/> dependency property.
		/// </summary>
		private static readonly DependencyPropertyKey DisplayValuePropertyKey = DependencyProperty.RegisterReadOnly(
			"DisplayValue",
			typeof( object ),
			typeof( XamComboEditor ),
			new FrameworkPropertyMetadata( null, FrameworkPropertyMetadataOptions.None )
		);

		/// <summary>
		/// Identifies the read-only <see cref="DisplayValue"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty DisplayValueProperty = DisplayValuePropertyKey.DependencyProperty;

		/// <summary>
		/// Indicates the value that's displayed in the edit portion.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>DisplayValue</b> indicates the value that's displayed in the edit portion of the editor.
		/// It's value depends on the value of <see cref="DisplayValueSource"/> property. If <i>DisplayValueSource</i>
		/// is set to <i>DisplayText</i> then the property returns the value of <see cref="TextEditorBase.DisplayText"/> 
		/// property. If the <i>DisplayValueSource</i> is set to <i>Value</i> then the property returns the value of
		/// the <see cref="ValueEditor.Value"/> property. If it's set to <i>SelectedItem</i> then this property returns
		/// the value of the <see cref="XamComboEditor.SelectedItem"/> property.
		/// </para>
		/// </remarks>
		/// <seealso cref="DisplayValueSource"/>
		/// <seealso cref="ValueEditor.Value"/>
		/// <seealso cref="XamComboEditor.SelectedItem"/>
		/// <seealso cref="XamComboEditor.SelectedIndex"/>
		/// <seealso cref="TextEditorBase.DisplayText"/>
		//[Description( "Value displayed in the edit portion." )]
		//[Category( "Data" )]
		[Bindable( true )]
		[Browsable( false )]
		[ReadOnly( true )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public object DisplayValue
		{
			get
			{
				return (object)this.GetValue( DisplayValueProperty );
			}
		}

		#endregion // DisplayValue

		#region DisplayValueSource

		// SSP 3/10/09 Display Value Task
		// Added DisplayValue and DisplayValueSource properties on XamComboEditor.
		// 

		/// <summary>
		/// Identifies the <see cref="DisplayValueSource"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty DisplayValueSourceProperty = DependencyProperty.Register(
			"DisplayValueSource",
			typeof( DisplayValueSource ),
			typeof( XamComboEditor ),
			new FrameworkPropertyMetadata( DisplayValueSource.DisplayText, FrameworkPropertyMetadataOptions.None,
				new PropertyChangedCallback( OnDisplayValueSourceChanged ) )
		);

		/// <summary>
		/// Specifies whether the editor's <see cref="TextEditorBase.DisplayText"/> or 
		/// <see cref="ValueEditor.Value"/> is displayed in the edit portion. Default is <b>DisplayText</b>.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>DisplayValueSource</b> property controls the <see cref="DisplayValue"/> property's
		/// value. <i>DisplayValue</i> in turn is used to display the contents of the editor
		/// in the edit portion. Essentially <i>DisplayValueSource</i> specifies whether to 
		/// display the display text or the value in the edit portion of the XamComboEditor.
		/// Default is <b>DisplayText</b>.
		/// </para>
		/// </remarks>
		/// <seealso cref="DisplayValue"/>
		/// <seealso cref="TextEditorBase.DisplayText"/>
		/// <seealso cref="ValueEditor.Value"/>
		//[Description( "Specifies what to display in the edit portion of the editor." )]
		//[Category( "Data" )]
		[Bindable( true )]
		public DisplayValueSource DisplayValueSource
		{
			get
			{
				return (DisplayValueSource)this.GetValue( DisplayValueSourceProperty );
			}
			set
			{
				this.SetValue( DisplayValueSourceProperty, value );
			}
		}

		private static void OnDisplayValueSourceChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			XamComboEditor editor = (XamComboEditor)dependencyObject;

			editor.InitializeComboBoxItemTemplateHelper( );

			editor.InitializeDisplayValue( );
		}

		private void InitializeDisplayValue( )
		{
			DisplayValueSource source = this.DisplayValueSource;

			// SSP 3/12/10 TFS27090
			// Added SelectedItem to the DisplayValueSource enum. Added the if block and
			// enclosed the existing code into the else block.
			// 
			if ( DisplayValueSource.SelectedItem == source 
				|| this.SelectedItem is ComboBoxDataItem && string.IsNullOrEmpty( this.DisplayMemberPath ) 
				)
				this.SetValue( DisplayValuePropertyKey, this.SelectedItem );
			else if ( DisplayValueSource.Value == source )
				this.SetValue( DisplayValuePropertyKey, this.Value );
			else
				this.SetValue( DisplayValuePropertyKey, this.DisplayText );
		}

		/// <summary>
		/// Returns true if the DisplayValueSource property is set to a non-default value.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public bool ShouldSerializeDisplayValueSource( )
		{
			return Utilities.ShouldSerialize( DisplayValueSourceProperty, this );
		}

		/// <summary>
		/// Resets the DisplayValueSource property to its default state.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public void ResetDisplayValueSource( )
		{
			this.ClearValue( DisplayValueSourceProperty );
		}

		#endregion // DisplayValueSource

		#region DisplayValueTemplateKey

		// SSP 3/10/09 Display Value Task
		// 
		/// <summary>
		/// The key used to identify the template used to display editor values when the <see cref="DisplayValueSource"/> is set to <i>Value</i>.
		/// </summary>
		public static readonly ResourceKey DisplayValueTemplateKey = new StaticPropertyResourceKey( typeof( XamComboEditor ), "DisplayValueTemplateKey" );

		#endregion // DisplayValueTemplateKey

		#region DropDownButtonDisplayMode

		/// <summary>
		/// Identifies the <see cref="DropDownButtonDisplayMode"/> dependency property
		/// </summary>
		public static readonly DependencyProperty DropDownButtonDisplayModeProperty = DependencyProperty.Register(
			"DropDownButtonDisplayMode",
			typeof( DropDownButtonDisplayMode ),
			typeof( XamComboEditor ),
			new FrameworkPropertyMetadata( DropDownButtonDisplayMode.MouseOver, FrameworkPropertyMetadataOptions.None,
				null )
			);

		/// <summary>
		/// Specifies when to display the drop down button. Default is <b>MouseOver</b>.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>DropDownButtonDisplayMode</b> specifies when to display the drop down button.
		/// The default value of the property is <b>MouseOver</b>. However note that styles 
		/// of some of the themes may explicitly set this property to some other value to
		/// ensure consistency with the default operating system behavior. For example, 
		/// the default XamComboEditor style in "Aero" theme used in Windows Vista sets this 
		/// property to <b>Always</b>.
		/// </para>
		/// <para class="body">
		/// <b>Note</b> that the drop down button will always be displayed when the editor
		/// is in edit mode.
		/// </para>
		/// <para class="body">
		/// <b>NOTE:</b> If the <see cref="ValueEditor.IsAlwaysInEditMode"/> property is set to True, the
		/// drop down button will always be displayed and in effect this property will ignored.
		/// </para>
		/// </remarks>
		//[Description( "Specifies when to display the drop down button" )]
		//[Category( "Behavior" )]
		[Bindable( true )]
		public DropDownButtonDisplayMode DropDownButtonDisplayMode
		{
			get
			{
				return (DropDownButtonDisplayMode)this.GetValue( DropDownButtonDisplayModeProperty );
			}
			set
			{
				this.SetValue( DropDownButtonDisplayModeProperty, value );
			}
		}

		/// <summary>
		/// Returns true if the DropDownButtonDisplayMode property is set to a non-default value.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public bool ShouldSerializeDropDownButtonDisplayMode( )
		{
			return Utilities.ShouldSerialize( DropDownButtonDisplayModeProperty, this );
		}

		/// <summary>
		/// Resets the DropDownButtonDisplayMode property to its default state.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public void ResetDropDownButtonDisplayMode( )
		{
			this.ClearValue( DropDownButtonDisplayModeProperty );
		}

		#endregion // DropDownButtonDisplayMode

		#region DropDownButtonStyle

		/// <summary>
		/// Identifies the <see cref="DropDownButtonStyle"/> dependency property
		/// </summary>
		public static readonly DependencyProperty DropDownButtonStyleProperty = DependencyProperty.Register(
			"DropDownButtonStyle",
			typeof( Style ),
			typeof( XamComboEditor ),
			new FrameworkPropertyMetadata( null, FrameworkPropertyMetadataOptions.None,
				null )
			);

		/// <summary>
		/// Used for setting the Style of the drop-down button. Default value is null.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// Default value of this property is null. You can use this property to specify a Style object to use for the
		/// drop-down button displayed inside the editor.
		/// </para>
		/// <seealso cref="ComboBoxStyle"/>
		/// </remarks>
		//[Description( "Used for setting the Style of the drop-down button" )]
		//[Category( "Behavior" )]
		[Bindable( true )]
		public Style DropDownButtonStyle
		{
			get
			{
				return (Style)this.GetValue( DropDownButtonStyleProperty );
			}
			set
			{
				this.SetValue( DropDownButtonStyleProperty, value );
			}
		}

		/// <summary>
		/// Returns true if the DropDownButtonStyle property is set to a non-default value.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public bool ShouldSerializeDropDownButtonStyle( )
		{
			return Utilities.ShouldSerialize( DropDownButtonStyleProperty, this );
		}

		/// <summary>
		/// Resets the DropDownButtonStyle property to its default state.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public void ResetDropDownButtonStyle( )
		{
			this.ClearValue( DropDownButtonStyleProperty );
		}

		#endregion // DropDownButtonStyle

		#region DropDownButtonVisibility

        // AS 9/3/08 NA 2008 Vol 2
        // I moved the UpdateDropDownVisibility method to the TextEditorBase to
        // avoid duplicating code so I need to be able to access this DPK.
        //
		//private static readonly DependencyPropertyKey DropDownButtonVisibilityPropertyKey = DependencyProperty.RegisterReadOnly(
		internal static readonly DependencyPropertyKey DropDownButtonVisibilityPropertyKey = DependencyProperty.RegisterReadOnly(
			"DropDownButtonVisibility",
			typeof( Visibility ),
			typeof( XamComboEditor ),
			new FrameworkPropertyMetadata( Visibility.Collapsed, FrameworkPropertyMetadataOptions.None,
				null )
			);

		/// <summary>
		/// Identifies the Read-Only <see cref="DropDownButtonVisibility"/> dependency property
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
		//[Description( "Indicates whether the drop down button is currently visible or not." )]
		//[Category( "Appearance" )]
		[Bindable( true )]
		[Browsable( false )]
		[ReadOnly( true )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public Visibility DropDownButtonVisibility
		{
			get
			{
				return (Visibility)this.GetValue( DropDownButtonVisibilityProperty );
			}
		}

		#endregion // DropDownButtonVisibility

		#region DropDownResizeMode

		/// <summary>
		/// Identifies the <see cref="DropDownResizeMode"/> dependency property
		/// </summary>
		public static readonly DependencyProperty DropDownResizeModeProperty = DependencyProperty.Register(
			"DropDownResizeMode",
			typeof( PopupResizeMode ),
			typeof( XamComboEditor ),
			new FrameworkPropertyMetadata( PopupResizeMode.Both,
				FrameworkPropertyMetadataOptions.None,
				null )
			);

		/// <summary>
		/// Specifies whether to allow the user to resize the drop down and if so how.
		/// Default value of this property is <b>Both</b> which will allow the user to
		/// resize the drop-down list in both directions - horizontally as well vertically.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// This editor has the functionality to let the user resize the drop-down list
		/// while it's dropped down. By default value of this property is <b>Both</b> which 
		/// will let the user resize in both directions, horizontally as well as vertically. You
		/// can set this property to <b>None</b> to disable resizing.
		/// </para>
		/// <para class="body">
		/// A resize element is displayed at the bottom or bottom-right corner of the drop-down
		/// depending on whether this property is set to <b>Vertical</b> or <b>Both</b>, respectively.
		/// </para>
		/// </remarks>
		//[Description( "Specifies whether to allow the user to resize the drop down and if so how" )]
		//[Category( "Behavior" )]
		[Bindable( true )]
		public PopupResizeMode DropDownResizeMode
		{
			get
			{
				return (PopupResizeMode)this.GetValue( DropDownResizeModeProperty );
			}
			set
			{
				this.SetValue( DropDownResizeModeProperty, value );
			}
		}

		/// <summary>
		/// Returns true if the DropDownResizeMode property is set to a non-default value.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public bool ShouldSerializeDropDownResizeMode( )
		{
			return Utilities.ShouldSerialize( DropDownResizeModeProperty, this );
		}

		/// <summary>
		/// Resets the DropDownResizeMode property to its default state.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public void ResetDropDownResizeMode( )
		{
			this.ClearValue( DropDownResizeModeProperty );
		}

		#endregion // DropDownResizeMode

		#region IsDropDownOpen

		/// <summary>
		/// Identifies the <see cref="IsDropDownOpen"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsDropDownOpenProperty = DependencyProperty.Register(
			"IsDropDownOpen",
			typeof( bool ),
			typeof( XamComboEditor ),
			new FrameworkPropertyMetadata( false,
				
				FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                // AS 9/10/08 NA 2008 Vol 2
				//null, 
                new PropertyChangedCallback(OnIsDropDownOpenChanged),
                new CoerceValueCallback( OnCoerceIsDropDownOpen ) )
			);

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
		[Bindable( true )]
		[Browsable( false )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
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

		/// <summary>
		/// When IsDroppedDown is set, we need to make sure that the editor is in edit mode and if not
		/// enter it into edit mode. That's what this coerce handler does.
		/// </summary>
		/// <param name="dependencyObject"></param>
		/// <param name="valueAsObject"></param>
		/// <returns></returns>
		private static object OnCoerceIsDropDownOpen( DependencyObject dependencyObject, object valueAsObject )
		{
			bool val = (bool)valueAsObject;
			XamComboEditor editor = (XamComboEditor)dependencyObject;

			if ( val )
			{
				// SSP 11/9/10 TFS33587
				// If the editor is read-only, disallow the drop-down from being displayed.
				// 
				if ( editor.IsReadOnly )
					return false;

				editor.StartEditMode( );
				if ( ! editor.IsInEditMode )
					return false;	
			}

			return val;
		}

        private static void OnIsDropDownOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            XamComboEditor editor = (XamComboEditor)d;
            bool isOpen = true.Equals(e.NewValue);

            // AS 9/10/08 NA 2008 Vol 2
            if (isOpen)
            {
                // if we're opening and we have a pending close then fire the close now
                editor.RaiseDropDownClosedIfPending();
            }
            else
            {
                // otherwise track that we are waiting for a close
                editor._isClosePending = true;
            }
            
            // TK Raise property change event
            bool newValue = (bool)e.NewValue;
            bool oldValue = !newValue;
            editor.RaiseAutomationExpandCollapseStateChanged(oldValue, newValue);

            // AS 2/5/09 TFS13569
            editor.CoerceValue(ToolTipService.IsEnabledProperty);
        }

		#endregion // IsDropDownOpen

		#region IsEditable

		/// <summary>
		/// Identifies the <see cref="IsEditable"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsEditableProperty = DependencyProperty.Register(
			"IsEditable",
			typeof( bool ),
			typeof( XamComboEditor ),
			new FrameworkPropertyMetadata( false, FrameworkPropertyMetadataOptions.None,

            // JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
                 new PropertyChangedCallback(OnVisualStatePropertyChanged) )




			);

		/// <summary>
		/// Gets or sets a value that enables or disables editing of text in the text box portion of the editor.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// If <b>IsEditable</b> is set to <b>False</b>, the textbox portion of the editor is not editable. It
		/// will not let you enter any text. In this mode the only way to modify the value of editor is to select an
		/// item from the drop down. This differs from <seealso cref="ValueEditor.ReadOnly"/> property in that
		/// the <b>ReadOnly</b> doesn't let you modify the value of the editor at all.
		/// </para>
		/// <seealso cref="ValueEditor.ReadOnly"/>
		/// </remarks>
		//[Description( "Specifies whether the combo portion is editable textbox." )]
		//[Category( "Behavior" )]
		[Bindable( true )]
		public bool IsEditable
		{
			get
			{
				return (bool)this.GetValue( IsEditableProperty );
			}
			set
			{
				this.SetValue( IsEditableProperty, value );
			}
		}

		/// <summary>
		/// Returns true if the IsEditable property is set to a non-default value.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public bool ShouldSerializeIsEditable( )
		{
			return Utilities.ShouldSerialize( IsEditableProperty, this );
		}

		/// <summary>
		/// Resets the IsEditable property to its default state.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public void ResetIsEditable( )
		{
			this.ClearValue( IsEditableProperty );
		}

		#endregion // IsEditable

		#region ItemsProvider

		/// <summary>
		/// Identifies the <see cref="ItemsProvider"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ItemsProviderProperty = DependencyProperty.Register(
			"ItemsProvider",
			typeof( ComboBoxItemsProvider ),
			typeof( XamComboEditor ),
			new FrameworkPropertyMetadata( null, FrameworkPropertyMetadataOptions.None,
				new PropertyChangedCallback( OnItemsProviderChanged ),
				new CoerceValueCallback( OnItemsProviderCoerced ) )
			);

		/// <summary>
		/// Used for specifying the items to be displayed in the drop down. Default value of this property is null.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>ItemsProvider</b> property is used for specifying the items to be displayed in the drop down. You can
		/// set this property to a new or an existing instance of <see cref="ComboBoxItemsProvider"/>. Populate
		/// the ComboBoxItemsProvider instace with the items to be displayed. A ComboBoxItemsProvider instance can
		/// be assigned to multiple XamComboEditor instances for efficiency purposes if the drop down items are the same
		/// across those XamComboEditors. ComboBoxItemsProvider object is the one that manages the items and binding 
		/// to the items source.
		/// </para>
		/// <see cref="ComboBoxItemsProvider"/>
		/// </remarks>
		//[Description( "Used for specifying the items to be displayed in the drop down." )]
		//[Category( "Data" )]
		[Bindable( true )]
		public ComboBoxItemsProvider ItemsProvider
		{
			get
			{
				return (ComboBoxItemsProvider)this.GetValue( ItemsProviderProperty );
			}
			set
			{
				this.SetValue( ItemsProviderProperty, value );
			}
		}

		private static void OnItemsProviderChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			XamComboEditor editor = (XamComboEditor)dependencyObject;
			ComboBoxItemsProvider newVal = (ComboBoxItemsProvider)e.NewValue;

			// SSP 1/10/08 BR29141
			// Added ItemsSource property.
			//
			editor.VerifyInternalItemsProvider( false, newVal  );

			
			
			
			
			editor.InitializeComboBoxItemsSource( editor.ComboBox );
			




			

			// SSP 3/5/08 BR31088
			// When the items provider changes, we need to sync the value and display text.
			// 
			// SSP 1/19/09 - NAS9.1 Record Filtering
			// Moved existing code into new OnItemsProviderItemsChanged method.
			// 
			// ------------------------------------------------------------------------
			// SSP 9/28/09 TFS19412
			// 
			// ----------------------------------------------------------
			//editor.OnItemsProviderItemsChanged( );
			Dispatcher dispatcher = editor.Dispatcher;
			// JJD 6/29/11 - TFS79601 
			// Only call BeginInvoke if asynchronous operation are supported
			//if ( null != dispatcher )
			if ( null != dispatcher && editor.SupportsAsyncOperations)
				dispatcher.BeginInvoke( DispatcherPriority.DataBind, new Utils.MethodInvoker( editor.OnItemsProviderItemsChanged ) );
			else
				editor.OnItemsProviderItemsChanged( );
			// ----------------------------------------------------------

			if ( null != newVal )
				editor._itemsProviderVersionTracker = new PropertyValueTracker( newVal,
					ComboBoxItemsProvider.VersionProperty, editor.OnItemsProviderItemsChanged,
					// SSP 1/16/12 TFS87805
					// Pass in true for async. Now we hook into each data item's property changed as well
					// which means that if multiple items are being modified at the same time, we should wait
					// till they are all modified before responding for performance reasons.
					// 
					true );
			else
				editor._itemsProviderVersionTracker = null;
			
#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

			// ------------------------------------------------------------------------
		}

		// SSP 1/19/09 - NAS9.1 Record Filtering
		// Added code to handle case where the items of the ComboBoxItemsProvider are 
		// changed - either its ItemsSource is changed or the items source list is changed.
		// This is more likely to happen with the record filtering functionality where
		// the drop-down list is lazily populated.
		//
		private PropertyValueTracker _itemsProviderVersionTracker;
		private void OnItemsProviderItemsChanged( )
		{
			// SSP 3/5/08 BR31088
			// When the items provider changes, we need to sync the value and display text.
			// 
			if ( !Utils.IsValueEmpty( this.Value ) )
				// SSP 6/28/11 TFS63736
				// 
				//this.OnValueChanged_SyncProperties( ValueChangeSource.Value, this.Value );
				this.SyncValueProperties( ValueProperty, this.Value );
			else if ( !Utils.IsValueEmpty( this.Text ) )
				// SSP 6/28/11 TFS63736
				// 
				//this.OnValueChanged_SyncProperties( ValueChangeSource.Text, this.Text );
				this.SyncValueProperties( TextProperty, this.Text );
			// SSP 9/25/09 TFS19412
			// 
			// SSP 1/16/12 TFS59404
			// 
			// ------------------------------------------------------
			//else
			//	this.CoerceValue( SelectedItemProperty );
			else if ( _selectedItemCoerced )
				this.CoerceValue( SelectedItemProperty );
			else if ( _selectedIndexCoerced )
				this.CoerceValue( SelectedIndexProperty );
			// ------------------------------------------------------
		}

		// SSP 1/10/08 BR29141
		// Added ItemsSource property.
		// 
		private static object OnItemsProviderCoerced( DependencyObject dependencyObject, object valueAsObject )
		{
			XamComboEditor editor = (XamComboEditor)dependencyObject;

			editor.VerifyInternalItemsProvider( true, valueAsObject as ComboBoxItemsProvider  );
			if ( null != editor._internalItemsProvider )
				return editor._internalItemsProvider;

			return valueAsObject;
		}

		/// <summary>
		/// Returns true if the ItemsProvider property is set to a non-default value.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public bool ShouldSerializeItemsProvider( )
		{
			return Utilities.ShouldSerialize( ItemsProviderProperty, this )
				// SSP 1/10/08 BR29141
				// Don't serialize the items provider if its our internal items provider.
				// 
				&& _internalItemsProvider != this.ItemsProvider;
		}

		/// <summary>
		/// Resets the ItemsProvider property to its default state.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public void ResetItemsProvider( )
		{
			this.ClearValue( ItemsProviderProperty );
		}

		#endregion // ItemsProvider

		#region ItemsSource

		// SSP 1/10/08 BR29141
		// Added ItemsSource, ValuePath and DisplayMemberPath properties.
		// 

		/// <summary>
		/// Identifies the <see cref="ItemsSource"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ItemsSourceProperty =
			ComboBoxItemsProvider.ItemsSourceProperty.AddOwner( typeof( XamComboEditor ),
				new FrameworkPropertyMetadata(new PropertyChangedCallback( OnItemsSourceChanged ) )
				);

		/// <summary>
		/// Specifies the collection from which to populate the drop-down of this XamComboEditor.
		/// This property providers an alternate way of specifying <see cref="ItemsProvider"/>.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// There are two ways you can specify the items to display in the drop-down of the XamComboEditor.
		/// By assigning <see cref="ItemsProvider"/> property to an instance of <see cref="ComboBoxItemsProvider"/>
		/// or by assigning <b>ItemsSource</b> property. 
		/// </para>
		/// <para class="body">
		/// Using <i>ItemsProvider</i> is more efficient when you
		/// need to bind the same items source to multiple XamComboEditors, in which case you would assign
		/// the same <i>ComboBoxItemsProvider</i> instance to multiple XamComboEditors. For example, if you 
		/// have a two or more XamComboEditors on the form displaying the same drop-down list of items. Or 
		/// inside a DataGrid field where cells are using XamComboEditors that all bind to the same items source.
		/// It is more efficient because a single ComboBoxItemsProvider manages binding to underlying data source 
		/// items collection and all the associated XamComboEditors make use of that single instance.
		/// </para>
		/// <para class="body">
		/// If however situation doesn't call for multiple instances of the editor using the same data source,
		/// <i>ItemsSource</i> property offers a more convenient way of specifying the drop-down list items 
		/// where you don't have to create an instance of 
		/// ComboBoxItemsProvider. Although creating ComboBoxItemsProvider is a very simple procedure, in certain
		/// situations using <i>ItemsSource</i> can be more convenient, especially in XAML where binding is
		/// involved. As a note, when you set <i>ItemsSource</i> property, internally the XamComboEditor
		/// simply creates a ComboBoxItemsProvider that wraps the specified items source and sets the 
		/// <i>ItemsProvider</i> to it.
		/// </para>
		/// <para class="body">
		/// <b>NOTE:</b> <b>ItemsSource</b> and <b>ItemsProvider</b> are exclusive. Use one or the other.
		/// Setting <i>ItemsProvider</i> and then setting <i>ItemsSource</i> will ignore the <i>ItemsSource</i>.
		/// </para>
		/// </remarks>
		//[Description( "Specifies the collection from which to populate the drop-down list" )]
		//[Category( "Data" )]
		[Bindable( true )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public IEnumerable ItemsSource
		{
			get
			{
				return (IEnumerable)this.GetValue( ItemsSourceProperty );
			}
			set
			{
				this.SetValue( ItemsSourceProperty, value );
			}
		}

		private static void OnItemsSourceChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			XamComboEditor editor = (XamComboEditor)dependencyObject;
			editor.VerifyInternalItemsProvider( false, editor.ItemsProvider  );
		}

		// AS 5/5/09 TFS17347
		// Attempting to access the value of a DP from within the Coerce while the 
		// property is bound can result in an exception in the earlier versions of 
		// the WPF framework. Even if this didn't result in an exception it could 
		// result in the wrong behavior because in the coerce the property may not 
		// have been set yet.
		//
		//private void VerifyInternalItemsProvider( bool fromItemsProviderCoerced )
		private void VerifyInternalItemsProvider( bool fromItemsProviderCoerced, ComboBoxItemsProvider currentItemsProvider )
		{
			IEnumerable itemsSource = this.ItemsSource;

			// AS 5/5/09 TFS17347
			// Added outer if block. If this is called from the coerce and the 
			// ItemsProvider is being provided then we do want to release any
			// items provider we have created.
			//
			if (!fromItemsProviderCoerced || currentItemsProvider == null)
			{
				if (null != _internalItemsProvider && null != itemsSource && _internalItemsProvider.ItemsSource == itemsSource)
					return;
			}

			// AS 5/5/09 TFS17347
			//bool itemsProviderExplicitlySet = null != this.ItemsProvider
			//	&& this.ItemsProvider != _internalItemsProvider;
			bool itemsProviderExplicitlySet = null != currentItemsProvider
				&& currentItemsProvider != _internalItemsProvider;

			if ( !itemsProviderExplicitlySet )
			{
				if ( null != itemsSource )
				{
					ComboBoxItemsProvider itemsProvider = new ComboBoxItemsProvider( );
					itemsProvider.ValuePath = this.ValuePath;
					itemsProvider.DisplayMemberPath = this.DisplayMemberPath;
					itemsProvider.ItemsSource = itemsSource;
					_internalItemsProvider = itemsProvider;
					if ( ! fromItemsProviderCoerced )
						this.CoerceValue( ItemsProviderProperty );
				}
				else
				{
					_internalItemsProvider = null;
					if ( !fromItemsProviderCoerced )
						this.ClearValue( ItemsProviderProperty );
				}
			}
			else
			{
				_internalItemsProvider = null;
			}
		}

		/// <summary>
		/// Returns true if the ItemsSource property is set to a non-default value.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public bool ShouldSerializeItemsSource( )
		{
			return Utilities.ShouldSerialize( ItemsSourceProperty, this );
		}

		/// <summary>
		/// Resets the ItemsSource property to its default state.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public void ResetItemsSource( )
		{
			this.ClearValue( ItemsSourceProperty );
		}

		#endregion // ItemsSource

		#region DisplayMemberPath

		// SSP 1/10/08 BR29141
		// Added ItemsSource, ValuePath and DisplayMemberPath properties.
		// 

		/// <summary>
		/// Identifies the <see cref="DisplayMemberPath"/> dependency property
		/// </summary>
		public static readonly DependencyProperty DisplayMemberPathProperty =
			ComboBoxItemsProvider.DisplayMemberPathProperty.AddOwner( typeof( XamComboEditor ),
					new FrameworkPropertyMetadata( new PropertyChangedCallback( OnDisplayMemberPathChanged ) )
				);

		/// <summary>
		/// Specifies the path into the selected item from which to get the text.
		/// The Text and DisplayText properties will return values from this path.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// DisplayMemberPath specifies the list object property to use to get the text for the editor.
		/// When an item is selected from the drop-down, the <see cref="XamComboEditor.SelectedItem"/> property
		/// returns the item itself. The <see cref="ValueEditor.Text"/> and <see cref="TextEditorBase.DisplayText"/> 
		/// properties return the value of the property specified by <b>DisplayMemberPath</b> from that selected 
		/// item.
		/// </para>
		/// <para class="body">
		/// <b>NOTE:</b> If you set the <see cref="ItemsProvider"/> property to an instance of 
		/// <see cref="ComboBoxItemsProvider"/> object then specify the <i>ValuePath</i> and
		/// <i>DisplayMemberPath</i> properies on that object. In that case these properties 
		/// on the editor will be ignored as <i>ItemsProvider</i> supercedes these properties.
		/// </para>
		/// </remarks>
		//[Description( "Path into the selected item from which to get the text" )]
		//[Category( "Data" )]
		[Bindable( true )]
		public string DisplayMemberPath
		{
			get
			{
				return (string)this.GetValue( DisplayMemberPathProperty );
			}
			set
			{
				this.SetValue( DisplayMemberPathProperty, value );
			}
		}


		private static void OnDisplayMemberPathChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			XamComboEditor editor = (XamComboEditor)dependencyObject;
			string newVal = (string)e.NewValue;
			if ( null != editor._internalItemsProvider )
				editor._internalItemsProvider.DisplayMemberPath = newVal;
		}

		/// <summary>
		/// Returns true if the DisplayMemberPath property is set to a non-default value.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public bool ShouldSerializeDisplayMemberPath( )
		{
			return Utilities.ShouldSerialize( DisplayMemberPathProperty, this );
		}

		/// <summary>
		/// Resets the DisplayMemberPath property to its default state.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public void ResetDisplayMemberPath( )
		{
			this.ClearValue( DisplayMemberPathProperty );
		}

		#endregion // DisplayMemberPath

		#region ValuePath

		// SSP 1/10/08 BR29141
		// Added ItemsSource, ValuePath and DisplayMemberPath properties.
		// 

		/// <summary>
		/// Identifies the <see cref="ValuePath"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ValuePathProperty =
			ComboBoxItemsProvider.ValuePathProperty.AddOwner( typeof( XamComboEditor ),
				new FrameworkPropertyMetadata( new PropertyChangedCallback( OnValuePathChanged ) )
				);

		/// <summary>
		/// Specifies the path into the selected item from which to get the data value.
		/// The Value property will return values from this path.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// ValuePath specifies the list object property to use to get the value for the editor.
		/// When an item is selected from the drop-down, the <see cref="XamComboEditor.SelectedItem"/> property
		/// returns the item itself. The <see cref="ValueEditor.Value"/> property returns the
		/// value of the property specified by <b>ValuePath</b> from that selected item.
		/// </para>
		/// <para class="body">
		/// <b>NOTE:</b> If you set the <see cref="ItemsProvider"/> property to an instance of 
		/// <see cref="ComboBoxItemsProvider"/> object then specify the <i>ValuePath</i> and
		/// <i>DisplayMemberPath</i> properies on that object. In that case these properties 
		/// on the editor will be ignored as <i>ItemsProvider</i> supercedes these properties.
		/// </para>
		/// </remarks>
		//[Description( "Path into the selected item from which to get the data value" )]
		//[Category( "Data" )]
		[Bindable( true )]
		public string ValuePath
		{
			get
			{
				return (string)this.GetValue( ValuePathProperty );
			}
			set
			{
				this.SetValue( ValuePathProperty, value );
			}
		}

		private static void OnValuePathChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			XamComboEditor editor = (XamComboEditor)dependencyObject;
			string newVal = (string)e.NewValue;
			if ( null != editor._internalItemsProvider )
				editor._internalItemsProvider.ValuePath = newVal;
		}

		/// <summary>
		/// Returns true if the ValuePath property is set to a non-default value.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public bool ShouldSerializeValuePath( )
		{
			return Utilities.ShouldSerialize( ValuePathProperty, this );
		}

		/// <summary>
		/// Resets the ValuePath property to its default state.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public void ResetValuePath( )
		{
			this.ClearValue( ValuePathProperty );
		}

		#endregion // ValuePath

		#region LimitToList

		// SSP 6/28/11 TFS63736
		// 
		/// <summary>
		/// Identifies the read-only <see cref="LimitToList"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty LimitToListProperty = DependencyProperty.Register(
			"LimitToList",
			typeof( bool ),
			typeof( XamComboEditor ),
			new FrameworkPropertyMetadata( KnownBoxes.FalseBox, new PropertyChangedCallback( OnLimitToListChanged ) )
		);
		
		private static void OnLimitToListChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			XamComboEditor editor = (XamComboEditor)d;
			editor.ValidateCurrentValue( );
		}

		/// <summary>
		/// Specifies whether to restrict the input to items in the items source. Default value is <b>False</b>.
		/// </summary>
		/// <seealso cref="LimitToListProperty"/>
		/// <remarks>
		/// <para class="body">
		/// <b>LimitToList</b> is used to restrict the user input to drop-down items. If the user enters text that doesn't
		/// have a matching item in the underlying items collection, the <see cref="ValueEditor.IsValueValid"/> property
		/// will be set to <b>False</b>. When the user attempts the leave the editor with such a value, 
		/// <see cref="ValueEditor.EditModeValidationError"/> event is raised and by default an error message is 
		/// displayed to the user. This behavior can be controlled by the <see cref="InvalidValueBehavior"/> property.
		/// </para>
		/// <para class="body">
		/// <b>Note</b> that the matching item is found by searching drop-down items for an item that has the same
		/// display text (based on the <see cref="DisplayMemberPath"/>) as what the user has entered in the editor.
		/// If a matching is item is found, the <see cref="ValueEditor.Value"/>, <see cref="ValueEditor.Text"/>, <see cref="SelectedIndex"/>
		/// and <see cref="SelectedItem"/> will be updated to reflect the matching item. If no matching item is found,
		/// the <b>Value</b> property will retain its previous value. The <b>Text</b> property will reflect the entered 
		/// text, <b>SelectedItem</b> will be set to <i>null</i> and <b>SelectedIndex</b> will be set to <i>-1</i>.
		/// </para>
		/// </remarks>
		/// <seealso cref="InvalidValueBehavior"/>
		/// <seealso cref="DisplayMemberPath"/>
		/// <seealso cref="ValuePath"/>
		/// <seealso cref="ValueEditor.ValueConstraint"/>
		public bool LimitToList
		{
			get
			{
				return (bool)this.GetValue( LimitToListProperty );
			}
			set
			{
				this.SetValue( LimitToListProperty, value );
			}
		}

		#endregion // LimitToList

		#region MaxDropDownHeight

		/// <summary>
		/// Identifies the <see cref="MaxDropDownHeight"/> dependency property
		/// </summary>
		public static readonly DependencyProperty MaxDropDownHeightProperty = DependencyProperty.Register(
			"MaxDropDownHeight",
			typeof( double ),
			typeof( XamComboEditor ),
			new FrameworkPropertyMetadata( 
				
				
				
				
				ComboBox.MaxDropDownHeightProperty.DefaultMetadata.DefaultValue,
				FrameworkPropertyMetadataOptions.None,
				null ),
			new ValidateValueCallback( ValidateMaxDropDownHeight )
			);

		/// <summary>
		/// Specifies the maximum drop-down height.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// You can use this property to constraint the height of the drop-down list. Particularly
		/// useful to constraint the user from resizing the height when drop-down resizing 
		/// functionality is enabled (see <see cref="DropDownResizeMode"/>).
		/// </para>
		/// <seealso cref="DropDownResizeMode"/>
		/// <seealso cref="MinDropDownWidth"/>
		/// <seealso cref="MaxDropDownWidth"/>
		/// </remarks>
		//[Description( "Maximum drop-down list height" )]
		//[Category( "Behavior" )]
		[Bindable( true )]
		public double MaxDropDownHeight
		{
			get
			{
				return (double)this.GetValue( MaxDropDownHeightProperty );
			}
			set
			{
				this.SetValue( MaxDropDownHeightProperty, value );
			}
		}

		private static bool ValidateMaxDropDownHeight( object objVal )
		{
			double val = (double)objVal;
			if ( val < 0 )
				return false;

			return true;
		}

		/// <summary>
		/// Returns true if the MaxDropDownHeight property is set to a non-default value.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public bool ShouldSerializeMaxDropDownHeight( )
		{
			return Utilities.ShouldSerialize( MaxDropDownHeightProperty, this );
		}

		/// <summary>
		/// Resets the MaxDropDownHeight property to its default state.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public void ResetMaxDropDownHeight( )
		{
			this.ClearValue( MaxDropDownHeightProperty );
		}

		#endregion // MaxDropDownHeight

		#region MaxDropDownWidth

		/// <summary>
		/// Identifies the <see cref="MaxDropDownWidth"/> dependency property
		/// </summary>
		public static readonly DependencyProperty MaxDropDownWidthProperty = DependencyProperty.Register(
			"MaxDropDownWidth",
			typeof( double ),
			typeof( XamComboEditor ),
			new FrameworkPropertyMetadata( 
				
				
				
				Double.PositiveInfinity, 
				FrameworkPropertyMetadataOptions.None,
				null ),
			new ValidateValueCallback( ValidateMaxDropDownWidth )
			);

		/// <summary>
		/// Specifies the maximum drop-down width.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// You can use this property to constraint the width of the drop-down list. Particularly
		/// useful to constraint the user from resizing the width when drop-down resizing 
		/// functionality is enabled (see <see cref="DropDownResizeMode"/>).
		/// </para>
		/// <seealso cref="DropDownResizeMode"/>
		/// <seealso cref="MinDropDownWidth"/>
		/// <seealso cref="MaxDropDownHeight"/>
		/// </remarks>
		//[Description( "Maximum drop-down list width" )]
		//[Category( "Behavior" )]
		[Bindable( true )]
		public double MaxDropDownWidth
		{
			get
			{
				return (double)this.GetValue( MaxDropDownWidthProperty );
			}
			set
			{
				this.SetValue( MaxDropDownWidthProperty, value );
			}
		}

		private static bool ValidateMaxDropDownWidth( object objVal )
		{
			double val = (double)objVal;

			if ( val < 0 )
				return false;

			return true;
		}

		/// <summary>
		/// Returns true if the MaxDropDownWidth property is set to a non-default value.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public bool ShouldSerializeMaxDropDownWidth( )
		{
			return Utilities.ShouldSerialize( MaxDropDownWidthProperty, this );
		}

		/// <summary>
		/// Resets the MaxDropDownWidth property to its default state.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public void ResetMaxDropDownWidth( )
		{
			this.ClearValue( MaxDropDownWidthProperty );
		}

		#endregion // MaxDropDownWidth

		#region MinDropDownWidth

		/// <summary>
		/// Identifies the <see cref="MinDropDownWidth"/> dependency property
		/// </summary>
		public static readonly DependencyProperty MinDropDownWidthProperty = DependencyProperty.Register(
			"MinDropDownWidth",
			typeof( double ),
			typeof( XamComboEditor ),
			new FrameworkPropertyMetadata( (double)0, FrameworkPropertyMetadataOptions.None,
				null ),
			new ValidateValueCallback( ValidateMinDropDownWidth )
			);

		/// <summary>
		/// Specifies the minimum drop-down width.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// You can use this property to constraint the width of the drop-down list. Particularly
		/// useful to constraint the user from resizing the width when drop-down resizing 
		/// functionality is enabled (see <see cref="DropDownResizeMode"/>).
		/// </para>
		/// <seealso cref="DropDownResizeMode"/>
		/// <seealso cref="MaxDropDownHeight"/>
		/// <seealso cref="MaxDropDownWidth"/>
		/// </remarks>
		//[Description( "Minimum drop-down list width" )]
		//[Category( "Behavior" )]
		[Bindable( true )]
		public double MinDropDownWidth
		{
			get
			{
				return (double)this.GetValue( MinDropDownWidthProperty );
			}
			set
			{
				this.SetValue( MinDropDownWidthProperty, value );
			}
		}

		private static bool ValidateMinDropDownWidth( object objVal )
		{
			double val = (double)objVal;
			return true;
		}

		/// <summary>
		/// Returns true if the MinDropDownWidth property is set to a non-default value.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public bool ShouldSerializeMinDropDownWidth( )
		{
			return Utilities.ShouldSerialize( MinDropDownWidthProperty, this );
		}

		/// <summary>
		/// Resets the MinDropDownWidth property to its default state.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public void ResetMinDropDownWidth( )
		{
			this.ClearValue( MinDropDownWidthProperty );
		}

		#endregion // MinDropDownWidth

		#region MinDropDownWidthResolved

		private static readonly DependencyPropertyKey MinDropDownWidthResolvedPropertyKey = DependencyProperty.RegisterReadOnly(
			"MinDropDownWidthResolved",
			typeof( double ),
			typeof( XamComboEditor ),
			new FrameworkPropertyMetadata( (double)0, FrameworkPropertyMetadataOptions.None,
				null )
			);

		/// <summary>
		/// Identifies the Read-Only <see cref="MinDropDownWidthResolved"/> dependency property
		/// </summary>
		public static readonly DependencyProperty MinDropDownWidthResolvedProperty = MinDropDownWidthResolvedPropertyKey.DependencyProperty;

		/// <summary>
		/// Resolved minimum drop-down width.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// The drop-down width is constrainted by default to the width of the edit portion. You also can
		/// explicitly set the <see cref="MinDropDownWidth"/>.
		/// </para>
		/// <seealso cref="DropDownResizeMode"/>
		/// <seealso cref="MinDropDownWidth"/>
		/// </remarks>
		[Bindable( true )]
		[Browsable( false )]
		[ReadOnly( true )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public double MinDropDownWidthResolved
		{
			get
			{
				return (double)this.GetValue( MinDropDownWidthResolvedProperty );
			}
		}

		internal void UpdateMinDropDownWidthResolved( )
		{
			double resolvedValue = this.MinDropDownWidth;

			if ( 0 == resolvedValue )
			{
				double editPortionWidth = this.EditPortionWidth;
				if ( editPortionWidth > 0 )
				{
					resolvedValue = editPortionWidth;

					ComboBox cb = this.ComboBox;
					if ( null != cb )
					{
						Thickness margin = cb.Margin;
						if ( null != margin )
							resolvedValue += margin.Left + margin.Right + 7; // 7 is for the shadow, borders, etc...
					}
				}
			}

			this.SetValue( XamComboEditor.MinDropDownWidthResolvedPropertyKey, resolvedValue );
		}

		#endregion // MinDropDownWidthResolved

		// JJD 07/06/10 - TFS32174
		#region PreDropDownAreaTemplate

		/// <summary>
		/// Identifies the <see cref="PreDropDownAreaTemplate"/> dependency property
		/// </summary>
		public static readonly DependencyProperty PreDropDownAreaTemplateProperty = DependencyProperty.Register("PreDropDownAreaTemplate",
			typeof(DataTemplate), typeof(XamComboEditor), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Determines what, if anything, will be displayed on top of the drop down items when the drop down is open.
		/// </summary>
		/// <seealso cref="PreDropDownAreaTemplateProperty"/>
		//[Description("Determines what, if anything, will be displayed on top of the drop down items when the drop down is open.")]
		//[Category("Behavior")]
		public DataTemplate PreDropDownAreaTemplate
		{
			get
			{
				return (DataTemplate)this.GetValue(XamComboEditor.PreDropDownAreaTemplateProperty);
			}
			set
			{
				this.SetValue(XamComboEditor.PreDropDownAreaTemplateProperty, value);
			}
		}

		#endregion //PreDropDownAreaTemplate

		// JJD 07/06/10 - TFS32174
		#region PostDropDownAreaTemplate

		/// <summary>
		/// Identifies the <see cref="PostDropDownAreaTemplate"/> dependency property
		/// </summary>
		public static readonly DependencyProperty PostDropDownAreaTemplateProperty = DependencyProperty.Register("PostDropDownAreaTemplate",
			typeof(DataTemplate), typeof(XamComboEditor), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Determines what, if anything, will be displayed below the drop down items when the drop down is open.
		/// </summary>
		/// <seealso cref="PostDropDownAreaTemplateProperty"/>
		//[Description("Determines what, if anything, will be displayed on top of the drop down items when the drop down is open.")]
		//[Category("Behavior")]
		public DataTemplate PostDropDownAreaTemplate
		{
			get
			{
				return (DataTemplate)this.GetValue(XamComboEditor.PostDropDownAreaTemplateProperty);
			}
			set
			{
				this.SetValue(XamComboEditor.PostDropDownAreaTemplateProperty, value);
			}
		}

		#endregion //PostDropDownAreaTemplate
	
		#region SelectedIndex

		/// <summary>
		/// Identifies the <see cref="SelectedIndex"/> dependency property
		/// </summary>
		public static readonly DependencyProperty SelectedIndexProperty = DependencyProperty.Register(
			"SelectedIndex",
			typeof( int ),
			typeof( XamComboEditor ),
			new FrameworkPropertyMetadata( -1,
				
				FrameworkPropertyMetadataOptions.Journal | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
				new PropertyChangedCallback( OnSelectedIndexChanged ),
				new CoerceValueCallback( OnCoerceSelectedIndex ) ) 
			);

		/// <summary>
		/// Gets or sets the selected index. Only valid when in edit mode.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>SelectedIndex</b> returns the index of the currently selected item. If no item
		/// is selected then returns -1. Likewise you can set this property to -1 to clear selected item.
		/// You can set SelectedIndex to a different value to change
		/// the item that's currently selected. This is always synchronized with the 
		/// <see cref="SelectedItem"/> property. Also note that the <see cref="ValueEditor.Value"/> property
		/// will reflect the updated selected item.
		/// </para>
		/// <seealso cref="SelectedItem"/> <seealso cref="ValueEditor.Value"/> <seealso cref="ValueEditor.Text"/>
		/// </remarks>
		//[Description( "Gets or sets the selected item index." )]
		//[Category( "Data" )]
		[Bindable( true )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public int SelectedIndex
		{
			get
			{
				return (int)this.GetValue( SelectedIndexProperty );
			}
			set
			{
				this.SetValue( SelectedIndexProperty, value );
			}
		}

		private static void OnSelectedIndexChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			XamComboEditor editor = (XamComboEditor)dependencyObject;
			int newVal = (int)e.NewValue;

			
			
			
			
			
			editor.SyncValueProperties( SelectedIndexProperty, newVal );
		}

		private static object OnCoerceSelectedIndex( DependencyObject dependencyObject, object valueAsObject )
		{
			XamComboEditor editor = (XamComboEditor)dependencyObject;
			int val = (int)valueAsObject;

			// SSP 1/16/12 TFS59404
			// 
			bool selectedIndexCoerced = false;

			// SSP 8/19/08
			// Check for IsInitialized. Otherwise don't validate. Enclosed the existing code into the
			// if block.
			// 
			if ( editor.IsInitialized )
			{
				ComboBoxItemsProvider itemsProvider = editor.ItemsProvider;
				int itemCount = null != itemsProvider ? itemsProvider.Items.Count : 0;

				if ( val < 0 || val >= itemCount )
				{
					// SSP 1/16/12 TFS59404
					// 
					selectedIndexCoerced = val >= 0;

					val = -1;
				}
			}

			// SSP 1/16/12 TFS59404
			// 
			editor._selectedIndexCoerced = selectedIndexCoerced;

			// SSP 9/25/09 TFS19412
			// 
			editor.OnCoerceValueProperty( SelectedIndexProperty, val );

			return val;
		}

		#endregion // SelectedIndex

		#region SelectedItem

		/// <summary>
		/// Identifies the <see cref="SelectedItem"/> dependency property
		/// </summary>
		public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
			"SelectedItem",
			typeof( object ),
			typeof( XamComboEditor ),
			new FrameworkPropertyMetadata( null,
				
				FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
				new PropertyChangedCallback( OnSelectedItemChanged ),
				new CoerceValueCallback( OnCoerceSelectedItem ) )
			);

		/// <summary>
		/// Gets or sets the selected item. Only valid when in edit mode.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>SelectedItem</b> returns the selected item. You can set this property to change the
		/// selected item. <see cref="SelectedIndex"/> along with the <see cref="ValueEditor.Value"/> and <see cref="ValueEditor.Text"/>
		/// properties will all be updated to reflect the selected item.
		/// </para>
		/// <seealso cref="SelectedIndex"/> <seealso cref="ValueEditor.Value"/> <see cref="ValueEditor.Text"/>
		/// </remarks>
		//[Description( "The item that's currently selected" )]
		//[Category( "Data" )]
		[Bindable( true )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public object SelectedItem
		{
			get
			{
				return (object)this.GetValue( SelectedItemProperty );
			}
			set
			{
				this.SetValue( SelectedItemProperty, value );
			}
		}

		private static void OnSelectedItemChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			XamComboEditor editor = (XamComboEditor)dependencyObject;
			object newVal = e.NewValue;

			
			
			
			editor.BeginSyncValueProperties( );
			try
			{
				
				
				
				
				
				editor.SyncValueProperties( SelectedItemProperty, newVal );
				
				editor.OnSelectedItemChanged( e.OldValue, newVal );
			}
			finally
			{
				editor.EndSyncValueProperties( );
			}
		}

		private static object OnCoerceSelectedItem( DependencyObject dependencyObject, object valueAsObject )
		{
			object val = (object)valueAsObject;
			XamComboEditor editor = (XamComboEditor)dependencyObject;

			// SSP 9/25/09 TFS19412
			// 
			// ------------------------------------------------------------------
			bool selectedItemCoerced = false;

			if ( null != val )
			{
				ComboBoxItemsProvider ip = editor.ItemsProvider;
				if ( null == ip || ip.GetItemIndex( val ) < 0 )
				{
					val = DependencyProperty.UnsetValue;
					selectedItemCoerced = true;
				}
			}

			// SSP 4/18/11 - Coerce SelectedItem Fix
			// Set the _selectedItemCoerced to keep track of if we have coerced the 
			// selected item to a different value.
			// 
			editor._selectedItemCoerced = selectedItemCoerced;

			editor.OnCoerceValueProperty( SelectedItemProperty, val );
			// ------------------------------------------------------------------

			return val;
		}

		/// <summary>
		/// Event ID for the 'SelectedItemChanged' routed event
		/// </summary>
		public static readonly RoutedEvent SelectedItemChangedEvent =
				EventManager.RegisterRoutedEvent( "SelectedItemChanged", RoutingStrategy.Bubble, typeof( RoutedPropertyChangedEventHandler<object> ), typeof( XamComboEditor ) );

		/// <summary>
		/// Called when <b>SelectedItem</b> property changes.
		/// </summary>
		/// <seealso cref="ValueEditor.ValueChanged"/>
		protected virtual void OnSelectedItemChanged( object previousValue, object currentValue )
		{
			RoutedPropertyChangedEventArgs<object> newEvent = new RoutedPropertyChangedEventArgs<object>( previousValue, currentValue );
			newEvent.RoutedEvent = SelectedItemChangedEvent;
			newEvent.Source = this;

			
			
			
			this.RaiseValuePropertyChangedEvent( newEvent );
		}

		/// <summary>
		/// Occurs when SelectedItem property changes or the user modifies the contents of the editor.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// SelectedItemChanged event is raised when the <see cref="SelectedItem"/> property's value
		/// changes. You can also use the <see cref="ValueEditor.ValueChanged"/> event to find out
		/// whenever the value of the editor changes.
		/// </para>
		/// <seealso cref="ValueEditor.ValueChanged"/>
		/// </remarks>
		//[Description( "Occurs when property 'SelectedItem' changes" )]
		//[Category( "Behavior" )]
		public event RoutedPropertyChangedEventHandler<object> SelectedItemChanged
		{
			add
			{
				base.AddHandler( XamComboEditor.SelectedItemChangedEvent, value );
			}
			remove
			{
				base.RemoveHandler( XamComboEditor.SelectedItemChangedEvent, value );
			}
		}

		#endregion // SelectedItem

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

		#region TextAlignmentResolved

		private static readonly DependencyPropertyKey TextAlignmentResolvedPropertyKey = DependencyProperty.RegisterReadOnly(
			"TextAlignmentResolved",
			typeof( TextAlignment ),
			typeof( XamComboEditor ),
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

		private void UpdateTextAlignmentResolved( )
		{
			object val = this.ReadLocalValue( HorizontalContentAlignmentProperty );
			if ( DependencyProperty.UnsetValue != val )
			{
				TextAlignment textAlignment = Utils.ToTextAlignment( (HorizontalAlignment)val );
				this.SetValue( TextAlignmentResolvedPropertyKey, textAlignment );
			}
			else
			{
				this.ClearValue( TextAlignmentResolvedPropertyKey );
			}
		}

		private static void OnHorizontalContentAlignmentChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			XamComboEditor editor = (XamComboEditor)dependencyObject;
			editor.UpdateTextAlignmentResolved( );
		}

		#endregion // TextAlignmentResolved

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

		#endregion //Public Properties

		#endregion //Properties

		#region Methods

		#region Private/Internal Methods

		#region ClearBypassNextComboTextChanged

		// SSP 11/9/11 TFS92225
		// 
		private void ClearBypassNextComboTextChanged( )
		{
			_bypassNextComboTextChanged = false;
		}

		#endregion // ClearBypassNextComboTextChanged

		#region InitializeCachedPropertyValues

		
		
		
		
		
		/// <summary>
		/// Initializes the variables used to cache the dependency property values by
		/// getting the dependency property metadata for this object and getting DefaultValue
		/// of that metadata for the respective property.
		/// </summary>
		private void InitializeCachedPropertyValues( )
		{
			// This is necessary for ComboEditorTool or any derived class that overrides the
			// default value of the DropDownButtonDisplayMode property.
			// 
			this.UpdateDropDownButtonVisibility( );
		}

		#endregion // InitializeCachedPropertyValues

		#region InitializeComboBoxItemsSource

		// SSP 1/20/09 - NAS9.1 Record Filtering
		// 
		private void InitializeComboBoxItemsSource( ComboBox comboBox )
		{
			ComboBoxItemsProvider ip = this.ItemsProvider;
			ItemCollection itemsSource = null != ip ? ip.Items : null;

			if ( null != comboBox && comboBox.ItemsSource != itemsSource )
			{
				// SSP 1/20/09 - NAS9.1 Record Filtering
				// See comments on ComboBox_ItemsSourceWrapper class definition.
				// 
				//comboBox.ItemsSource = itemsSource;
				// SSP 9/25/09 TFS19412
				// When setting ItemsSource to null on the combo-box, don't respond to its
				// SelectionChanged and null out Value/Text properties of the editor.
				// --------------------------------------------------------------------------
				//comboBox.ItemsSource = null != itemsSource ? new ComboBox_ItemsSourceWrapper( this, itemsSource ) : null;
				bool oldIsInitializingComboBox = _isInitializingComboBox;
				_isInitializingComboBox = true;
				try
				{
					comboBox.ItemsSource = null != itemsSource ? new ComboBox_ItemsSourceWrapper( this, itemsSource ) : null;
				}
				finally
				{
					_isInitializingComboBox = oldIsInitializingComboBox;
				}
				// --------------------------------------------------------------------------
			}
		}

		#endregion // InitializeComboBoxItemsSource

		#region InitializeComboBoxItemTemplateHelper

		// SSP 3/10/09 Display Value Task
		// 
		private void InitializeComboBoxItemTemplateHelper( )
		{
			ComboBox comboBox = this.ComboBox;
			if ( !_itemTemplateSetOnComboBox && null != comboBox
				&& DisplayValueSource.Value == this.DisplayValueSource
				&& null == comboBox.ItemTemplate )
			{
				comboBox.SetResourceReference( ComboBox.ItemTemplateProperty, XamComboEditor.DisplayValueTemplateKey );
				_itemTemplateSetOnComboBox = true;
			}
		}

		#endregion // InitializeComboBoxItemTemplateHelper

		#region MouseLeftButtonDown_ToggleDropDownHelper
		
#region Infragistics Source Cleanup (Region)









































































#endregion // Infragistics Source Cleanup (Region)

		#endregion // MouseLeftButtonDown_ToggleDropDownHelper

		#region OnCoerceValueProperty

		// SSP 9/25/09 TFS19412
		// 
		private void OnCoerceValueProperty( DependencyProperty property, object value )
		{
			// Since we coerce the SelectedItem to return null when the item doesn't
			// exist in the items source and then re-coerce when the items source is
			// assigned (in case the new items source contains the item), we need to
			// make sure that we don't end up coercing the item back to what it was
			// set to if Value, Text or SelectedIndex properties were set after 
			// SelectedItem was set.
			// 
			// SSP 4/18/11 - Coerce SelectedItem Fix
			// Check the new _selectedItemCoerced flag and also reset it to false.
			// 
			//if ( SelectedItemProperty != property )
			if ( _selectedItemCoerced && SelectedItemProperty != property )
			{
				_selectedItemCoerced = false;

				this.SelectedItem = this.SelectedItem;
			}

			// SSP 1/16/12 TFS59404
			// 
			if ( _selectedIndexCoerced && SelectedIndexProperty != property )
			{
				_selectedIndexCoerced = false;

				this.SelectedIndex = this.SelectedIndex;
			}
		}

		#endregion // OnCoerceValueProperty

		#region OnComboBox_SelectionChanged

		private void OnComboBox_SelectionChanged( object sender, SelectionChangedEventArgs e )
		{
			ComboBox comboBox = this.ComboBox;
			Debug.Assert( null != comboBox );
			if ( null != comboBox && this.IsInEditMode )
			{
				// SSP 11/16/10 TFS33587
				// If the editor is read-only, don't synchronize to combobox's value. Instead force the combobox's
				// value to be editor's value. Note that we prevent the keyboard messages from reaching the combobox
				// and also disable the drop-down to make sure the user cannot modify the combobox's value when the
				// editor is read-only so this is a fail-safe measure.
				// 
				// ------------------------------------------------------------------------------------------------------
				//this.OnValueChanged_SyncProperties( ValueChangeSource.ComboBoxSelectedIndex, comboBox.SelectedIndex );
				if ( ! this.IsReadOnly )
				{
					// SSP 8/9/12 TFS111388
					// This is to work-around the ComboBox's auto-complete functionality causing an issue where
					// it auto-selects a matching item when a character is deleted from the current text where
					// the new text happens to partially match an item. The problem is that XamComboEditor 
					// would and should return -1 for the SelectedIndex in that case since the text that's 
					// currently in the edit portion does not match any item fully. However the ComboBox
					// itself has its SelectedIndex set to the item that partially matches the current text.
					// In such a case, we should ignore the combobox's selection changed event and rely on the 
					// text to synchronize our value properties. NOTE that this change did not change the behavior 
					// in the sense that the end result is the same now as was before. The value properties of
					// the XamComboEditor and text that was visually displayed in the edit portion and the 
					// selected item in the drop-down are the same now as they were before the fix. The only 
					// difference is that before the fix, there was an extra unneccessary value changed event
					// that was raised, as described in the bug's description. This fix resolves that.
					// 
					// --------------------------------------------------------------------------------------------
					bool ignoreNotification = false;
					int index = comboBox.SelectedIndex;
					if ( null != _lastComboBoxTextBox && index >= 0 )
					{
						ComboBoxItemsProvider ip = this.ItemsProvider;
						if ( null != ip )
						{
							string text = ip.GetDisplayText( comboBox.SelectedItem, index );
							if ( text != _lastComboBoxTextBox.Text )
								ignoreNotification = true;
						}
					}
					// --------------------------------------------------------------------------------------------

					// SSP 8/9/12 TFS111388
					// Related to above change. Enclosed the existing code in the if block.
					// 
					if ( ! ignoreNotification )
					{
						// SSP 6/28/11 TFS63736
						// Pass false for the new calledFromSyncValuePropertiesOverride parameter.
						// 
						//this.OnValueChanged_SyncProperties( ValueChangeSource.ComboBoxSelectedIndex, comboBox.SelectedIndex );
						this.OnValueChanged_SyncProperties( ValueChangeSource.ComboBoxSelectedIndex, comboBox.SelectedIndex, false );

						// SSP 11/9/11 TFS92225
						// 
						if ( !_bypassNextComboTextChanged )
						{
							_bypassNextComboTextChanged = true;
							this.Dispatcher.BeginInvoke( new Utils.MethodInvoker( this.ClearBypassNextComboTextChanged ) );
						}
					}
				}
				else
				{
					comboBox.SelectedIndex = this.SelectedIndex;
				}
				// ------------------------------------------------------------------------------------------------------
			}
		}

		#endregion // OnComboBox_SelectionChanged

		#region OnComboBox_DropDownOpened

		private void OnComboBox_DropDownOpened( object sender, EventArgs e )
		{
			this.RaiseDropDownOpened( new RoutedEventArgs( DropDownOpenedEvent, this ) );
		}

		#endregion // OnComboBox_DropDownOpened

		#region OnComboBox_DropDownClosed

		private void OnComboBox_DropDownClosed( object sender, EventArgs e )
		{
            // AS 9/10/08 NA 2008 Vol 2
            //this.RaiseDropDownClosed( new RoutedEventArgs( DropDownClosedEvent, this ) );
            this.RaiseDropDownClosedIfPending();
		}

		#endregion // OnComboBox_DropDownClosed

		#region OnComboBoxChanged

		private void OnComboBoxChanged( ComboBox oldComboBox )
		{
			ComboBox newComboBox = this.ComboBox;

			if ( null != oldComboBox )
			{
				// Clear the EditPortionWidthProperty binding from the old combo box.
				// 
				BindingOperations.ClearBinding( this, EditPortionWidthProperty );

				oldComboBox.SelectionChanged -= new SelectionChangedEventHandler( OnComboBox_SelectionChanged );
				oldComboBox.DropDownOpened -= new EventHandler( OnComboBox_DropDownOpened );
				oldComboBox.DropDownClosed -= new EventHandler( OnComboBox_DropDownClosed );

				// SSP 3/10/09 Display Value Task
				// In order to workaround an issue in the ComboBox where it erronously uses a string template
				// for dependency objects even if there's data template defined for it, we have to explicitly
				// set the ItemTemplate property.
				// 
				if ( _itemTemplateSetOnComboBox )
				{
					_itemTemplateSetOnComboBox = false;
					oldComboBox.ClearValue( ComboBox.ItemTemplateProperty );
				}
			}

			if ( null != _lastComboBoxTextBox )
			{
				_lastComboBoxTextBox.SelectionChanged -= new RoutedEventHandler( this.OnTextBox_SelectionChanged );
				_lastComboBoxTextBox.TextChanged -= new TextChangedEventHandler( this.OnTextBox_TextChanged );
				_lastComboBoxTextBox = null;
			}

			if ( null != newComboBox )
			{
				newComboBox.SetValue( DefaultStyleKeyProperty, this.ComboBoxStyleKey );

				// Ensure combo box has applied its template so FindName works below.
				// 
				newComboBox.ApplyTemplate( );

				
				
				
				
				this.InitializeComboBoxItemsSource( newComboBox );
				




				

				newComboBox.SelectionChanged += new SelectionChangedEventHandler( OnComboBox_SelectionChanged );
				newComboBox.DropDownOpened += new EventHandler( OnComboBox_DropDownOpened );
				newComboBox.DropDownClosed += new EventHandler( OnComboBox_DropDownClosed );

				// PART_EditableTextBox comes from the ComboBox's template.
				// 
				//_lastComboBoxTextBox = this.GetTemplateChild( "PART_EditableTextBox" ) as TextBox;
				_lastComboBoxTextBox = Utilities.GetDescendantFromName( this, "PART_EditableTextBox" ) as TextBox;
				if ( null != _lastComboBoxTextBox )
				{
					_lastComboBoxTextBox.SelectionChanged += new RoutedEventHandler( this.OnTextBox_SelectionChanged );
					_lastComboBoxTextBox.TextChanged += new TextChangedEventHandler( this.OnTextBox_TextChanged );
				}

				// Bind the EditPortionWidthProperty to the combo box' ActualWidth so we can keep the
				// MinDropDownWidthResolved in sync with the edit area width.
				// 
				Binding binding = Utilities.CreateBindingObject( ComboBox.ActualWidthProperty, BindingMode.OneWay, newComboBox );
				this.SetBinding( EditPortionWidthProperty, binding );

				// SSP 3/10/09 Display Value Task
				// 
				this.InitializeComboBoxItemTemplateHelper( );
			}

			this.InitializeComboBoxValue( true );
		}

		#endregion // OnComboBoxChanged

		#region OnTextBox_SelectionChanged

		private void OnTextBox_SelectionChanged( object sender, RoutedEventArgs e )
		{
			bool orig_inOnComboBoxSelectionChanged = _inOnComboBoxSelectionChanged;
			_inOnComboBoxSelectionChanged = true;
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
				_inOnComboBoxSelectionChanged = orig_inOnComboBoxSelectionChanged;
			}
		}

		#endregion // OnTextBox_SelectionChanged

		#region OnTextBox_TextChanged

		private void OnTextBox_TextChanged( object sender, TextChangedEventArgs e )
		{
			Debug.Assert( null != _lastComboBoxTextBox );
			if ( null != _lastComboBoxTextBox && this.IsInEditMode )
			{
				// SSP 11/16/10 TFS33587
				// If the editor is read-only, don't synchronize to combobox's value. Instead force the combobox's
				// value to be editor's value. Note that we prevent the keyboard messages from reaching the combobox
				// and also disable the drop-down to make sure the user cannot modify the combobox's value when the
				// editor is read-only so this is a fail-safe measure.
				// 
				// ------------------------------------------------------------------------------------------------------
				//this.OnValueChanged_SyncProperties( ValueChangeSource.ComboBoxText, _lastComboBoxTextBox.Text );
				if ( !this.IsReadOnly
					// SSP 11/9/11 TFS92225
					// 
					&& ( ! _bypassNextComboTextChanged || this.Text != _lastComboBoxTextBox.Text )
					)
				{
					// SSP 6/28/11 TFS63736
					// Pass false for the new calledFromSyncValuePropertiesOverride parameter.
					// 
					//this.OnValueChanged_SyncProperties( ValueChangeSource.ComboBoxText, _lastComboBoxTextBox.Text );
					this.OnValueChanged_SyncProperties( ValueChangeSource.ComboBoxText, _lastComboBoxTextBox.Text, false );
				}
				// ------------------------------------------------------------------------------------------------------
			}
		}

		#endregion // OnTextBox_TextChanged

		#region OnValueChanged_SyncProperties

		// SSP 6/28/11 TFS63736
		// 
		//private void OnValueChanged_SyncPropertiesHelper( ValueChangeSource source, object value )
		private bool? OnValueChanged_SyncPropertiesHelper( ValueChangeSource source, object value )
		{
			ComboBoxItemsProvider ip = this.ItemsProvider;

			object selectedItem = null;
			int selectedIndex = -1;
			string displayText = string.Empty;
			object dataValue = null;
			ComboBox comboBox = this.ComboBox;
			bool isSourceComboBox = false;

			// SSP 6/28/11 TFS63736
			// 
			bool? isValueValid = null;

			// SSP 2/20/09 TFS13641
			// Added support for duplicate data values and/or display texts - basically multiple 
			// items with the same data value and/or display text. Added preferredIndex parameter.
			// 
			int preferredIndex = this.SelectedIndex;

			switch ( source )
			{
				case ValueChangeSource.Value:
					{
						dataValue = value;
						if ( null != ip )
						{
							// SSP 2/20/09 TFS13641
							// Added support for duplicate data values and/or display texts - basically multiple 
							// items with the same data value and/or display text. Added preferredIndex parameter.
							// 
							// ----------------------------------------------------------------------------------
							//selectedIndex = ip.FindListItemFromDataValue( dataValue, out selectedItem );
							//displayText = ip.GetDisplayTextFromDataValue( value );
							selectedIndex = ip.FindListItemFromDataValue( dataValue, preferredIndex, out selectedItem );
							if ( selectedIndex >= 0 )
								displayText = ip.GetDisplayText( selectedItem, selectedIndex );
							else
								displayText = ip.GetDisplayTextFromDataValue( value, preferredIndex );
							// ----------------------------------------------------------------------------------
						}
						else
						{
							Exception error;
							// SSP 5/3/11 TFS65229
							// 
							//this.ConvertValueToDisplayText( value, out displayText, out error );
							this.ConvertValueToText( value, out displayText, out error );
						}

						break;
					}
				case ValueChangeSource.SelectedItem:
					{
						if ( null != ip )
						{
							selectedIndex = ip.GetItemIndex( value );

							// SSP 9/25/09 TFS19412
							// Enclosed the existing code into the if block. Only set the selectedItem
							// if it exists in the item source.
							// 
							if ( selectedIndex >= 0 )
							{
								selectedItem = value;

								
								
								
								
								
								dataValue = ip.GetDataValue( selectedItem, selectedIndex );
								displayText = ip.GetDisplayText( selectedItem, selectedIndex );
							}
						}

						break;
					}
				case ValueChangeSource.SelectedIndex:
					{
						if ( null != ip )
						{
							selectedIndex = (int)value;
							if ( selectedIndex >= 0 )
							{
								selectedItem = ip.GetItemAtIndex( selectedIndex );
								
								
								
								
								
								dataValue = ip.GetDataValue( selectedItem, selectedIndex );
								displayText = ip.GetDisplayText( selectedItem, selectedIndex );
							}
						}

						break;
					}
				case ValueChangeSource.ComboBoxSelectedIndex:
					{
						isSourceComboBox = true;

						Debug.Assert( null != comboBox && null != ip );
						if ( null != comboBox && null != ip )
						{
							selectedIndex = comboBox.SelectedIndex;
							if ( selectedIndex >= 0 )
							{
								selectedItem = ip.GetItemAtIndex( selectedIndex );
								
								
								
								
								
								dataValue = ip.GetDataValue( selectedItem, selectedIndex );
								displayText = ip.GetDisplayText( selectedItem, selectedIndex );
							}
							// SSP 5/11/12 TFS111388
							// When the text in the combobox is changed to a value that does not 
							// have a matching item in the drop-down then we'll get here with 
							// SelectedIndex of -1. However that doesn't mean we should null out
							// the Value and Text etc... which only happens temporarily because
							// we do get combobox's TextChanged event which we process and 
							// re-initialize the Value and the Text correctly. However this 
							// temporary nulling out of Value and Text is not right.
							// 
							else
							{
								if ( null != _lastComboBoxTextBox && !string.IsNullOrEmpty( _lastComboBoxTextBox.Text ) )
									return this.OnValueChanged_SyncPropertiesHelper( ValueChangeSource.ComboBoxText, _lastComboBoxTextBox.Text );
							}
						}

						break;
					}
				case ValueChangeSource.ComboBoxText:
					// SSP 2/20/09 TFS14010
					// Apparently the ComboBox' SelectedIndex retains its old value 
					// if the new text partially matches the entry at the index.
					// For example, with "abc" entry at index 2, deleting 'c' will
					// still keep the selected index value of 2. This is incorrect.
					// If the new text "ab" doesn't have corresponding entry in the
					// list then the selected index should go to -1.
					// Commented out the following code. Fall through to the 
					// ValueChangeSource.Text case below.
					// 
					
#region Infragistics Source Cleanup (Region)

























#endregion // Infragistics Source Cleanup (Region)

				case ValueChangeSource.Text:
					{
						displayText = null != value ? value.ToString( ) : string.Empty;
						// SSP 2/20/09 TFS13641
						// Added support for duplicate data values and/or display texts - basically multiple 
						// items with the same data value and/or display text. Added preferredIndex parameter.
						// 
						//selectedIndex = null == ip ? -1 : ip.FindListItemFromDisplayText( displayText, false, 0, out selectedItem );
						selectedIndex = null == ip ? -1 : ip.FindListItemFromDisplayText( displayText, false, 0, preferredIndex, out selectedItem );

						if ( selectedIndex >= 0 )
						{
							selectedItem = ip.GetItemAtIndex( selectedIndex );

							// JM 06-23-11 TFS67853.  Since we are always doing case-insensitive searches, do an additional compare to make sure that
							// the selected item exactly matches the display text if the combo box in our template has its IsTextSearchCaseSensitive
							// property set to true.  Make sure the property exists on the ComboBox since the IsTextSearchCaseSensitive 
							// was not added to the ItemsControl until CLR4
							if (null != s_IsTextSearchCaseSensitiveProperty &&
								null != this._lastComboBox					&& 
								(bool)(this._lastComboBox.GetValue(s_IsTextSearchCaseSensitiveProperty)) == true)
							{
								string selectedItemDisplayText = ip.GetDisplayText(selectedItem, selectedIndex);

								if (string.Compare(displayText, selectedItemDisplayText, false) == 0)
								{
									dataValue		= ip.GetDataValue(selectedItem, selectedIndex);
									displayText		= selectedItemDisplayText;
								}
								else
									selectedIndex	= -1;
							}
							else
							{
								
								
								
								
								
								dataValue = ip.GetDataValue(selectedItem, selectedIndex);
								displayText = ip.GetDisplayText(selectedItem, selectedIndex);
							}
						}
						else
						{
							// SSP 6/28/11 TFS63736
							// Added the if block and enclosed the existing code in the else block.
							// 
							if ( this.LimitToList )
							{
								isValueValid = false;
							}
							else
							{
								
								
								
								
								// SSP 1/13/12 TFS99243
								// We should always use null otherwise binding will not work as it cannot convert
								// DBNull to a DateTime? value of null for example.
								// 
								
								dataValue = string.IsNullOrEmpty( displayText ) ? null : value;

								value = dataValue;

								// Since there's no corresponding entry for the new text, the value 
								// should be set to that value also. However if that value does
								// have a corresponding entry, then all the properties need to be
								// updated according to that matching entry. Example: if "1" was
								// mapped to "a" and Text was set to "1" then there would be no
								// matching entry (there's no entry with "1" as the text although 
								// there's one with "1" as the value however that means no matching
								// entry for "1" text). This will cause us to set both the Value and 
								// Text to "1". However since Value is "1" now, the Text should 
								// really be "a" since value "1" maps to "a" in list of items.
								// This call is for this situation.
								// 

								// SSP 6/28/11 TFS63736
								// Changed the return type of the method from void to bool?.
								// 
								//this.OnValueChanged_SyncPropertiesHelper( ValueChangeSource.Value, dataValue );
								//return;
								return this.OnValueChanged_SyncPropertiesHelper( ValueChangeSource.Value, dataValue );
							}
						}

						break;
					}
				default:
					Debug.Assert( false, "Unknown value for source" );
					break;
			}

			// SSP 10/27/11 TFS90711
			// Added Sync_SetValueHelper which has logic to not set the property that cause this
			// synchronization in the first place so that any underlying binding doesn't get 
			// re-evaluated again.
			// 
			// --------------------------------------------------------------------------------------
			this.Sync_SetValueHelper( SelectedIndexProperty, selectedIndex, source );
			this.Sync_SetValueHelper( SelectedItemProperty, selectedItem, source );

			// SSP 6/28/11 TFS63736
			// If the value is invalid then leave the Value property to its original value.
			// 
			if ( isValueValid ?? true )
				this.Sync_SetValueHelper( ValueProperty, dataValue, source );

			this.Sync_SetValueHelper( TextProperty, displayText, source );
			
#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)

			// --------------------------------------------------------------------------------------

			// SSP 1/13/10 TFS25609
			// If a custom ValueToDisplayTextConverter is set, use that for display text.
			// 
			// --------------------------------------------------------------------------------------------
			//this.SetValue( DisplayTextPropertyKey, displayText );
			string resolvedDisplayText = displayText;

			IValueConverter customDisplayTextConverter = this.ValueToDisplayTextConverter;
			if ( null != customDisplayTextConverter
				// SSP 5/4/11 TFS65229
				// Take into account NullText.
				// 
				|| string.IsNullOrEmpty( resolvedDisplayText ) && ( null == value || DBNull.Value == value ) )
			{
				string customDisplayText;
				Exception error;
				if ( base.ConvertValueToDisplayText( dataValue, out customDisplayText, out error ) )
					resolvedDisplayText = customDisplayText;
			}
			
			this.SetValue( DisplayTextPropertyKey, resolvedDisplayText );
			// --------------------------------------------------------------------------------------------

			// SSP 3/10/09 Display Value Task
			// Added DisplayValue and DisplayValueSource properties on XamComboEditor.
			// 
			this.InitializeDisplayValue( );

			if ( !isSourceComboBox && null != comboBox )
			{
				if ( selectedIndex >= 0 )
				{
					comboBox.SelectedIndex = selectedIndex;
				}
				else
				{
					comboBox.Text = displayText;

					// SSP 10/13/08 BR35126
					// Apparently setting Text on the combo box to empty string is not enough if we
					// get in here recursively. We also have to set the SelectedIndex to -1.
					// 
					// SSP 8/10/12 TFS111388
					// 
					// ----------------------------------------------------------------------------------------
					//if ( string.IsNullOrEmpty( displayText ) )
					//	comboBox.SelectedIndex = -1;

					if ( comboBox.SelectedIndex >= 0 )
					{
						int selStart = this.SelectionStart;
						int selLength = this.SelectionLength;

						// We need to prevent the combo box from selecting a matching item when its Text is set below.
						// 
						using ( new TempValueReplacement( comboBox, ComboBox.IsTextSearchEnabledProperty, KnownBoxes.FalseBox ) )
						{
							// Setting selected index to -1 will cause the combo box to null out the 
							// Text so we need to restore it. Also we need to restore the selection 
							// as well.
							// 
							comboBox.SelectedIndex = -1;
							comboBox.Text = displayText;

							if ( ! string.IsNullOrEmpty( displayText ) )
							{
								this.SelectionStart = Math.Min( displayText.Length, selStart );
								this.SelectionLength = selStart + Math.Min( displayText.Length, selLength - selStart );
							}
						}
					}
					// ----------------------------------------------------------------------------------------
				}
			}
			
			// SSP 6/28/11 TFS63736
			// 
			return isValueValid;
		}

		// SSP 6/28/11 TFS63736
		// Also added calledFromSyncValuePropertiesOverride parameter.
		// 
		//private void OnValueChanged_SyncProperties( ValueChangeSource source, object value )
		private void OnValueChanged_SyncProperties( ValueChangeSource source, object value, bool calledFromSyncValuePropertiesOverride )
		{
			if ( _inOnValueChanged_SyncProperties || _isInitializingComboBox
				// SSP 8/19/08
				// Check for IsInitialized. Otherwise don't sync properties yet.
				// 
				|| !this.InternalIsInitialized )
				return;

			_inOnValueChanged_SyncProperties = true;

			
			
			this.BeginSyncValueProperties( );

			try
			{
				// SSP 6/28/11 TFS63736
				// Changed the return type from void to bool?. Also added calledFromSyncValuePropertiesOverride parameter.
				// 
				// --------------------------------------------------------------------------------------
				//this.OnValueChanged_SyncPropertiesHelper( source, value );
				bool? isValueValid = this.OnValueChanged_SyncPropertiesHelper( source, value );

				if ( !calledFromSyncValuePropertiesOverride && isValueValid.HasValue && ! isValueValid.Value )
					this.ValidateCurrentValue( );
				// --------------------------------------------------------------------------------------
			}
			finally
			{
				_inOnValueChanged_SyncProperties = false;

				
				
				this.EndSyncValueProperties( );
			}
		}

		#endregion // OnValueChanged_SyncProperties

		#region Sync_SetValueHelper

		// SSP 10/27/11 TFS90711
		// 
		private void Sync_SetValueHelper( DependencyProperty prop, object value, ValueChangeSource changeSource )
		{
			DependencyProperty changeSourceProp = null;

			switch ( changeSource )
			{
				case ValueChangeSource.Value:
					changeSourceProp = ValueProperty;
					break;
				case ValueChangeSource.Text:
					changeSourceProp = TextProperty;
					break;
				case ValueChangeSource.SelectedItem:
					changeSourceProp = SelectedItemProperty;
					break;
				case ValueChangeSource.SelectedIndex:
					changeSourceProp = SelectedIndexProperty;
					break;
			}

			// Don't set the property to the same value if we are responding to the change of that property.
			// This causes underlying binding if any to get re-evaluated.
			// 
			// SSP 1/16/12 TFS59404
			// 
			//if ( null != prop && prop == changeSourceProp )
			if ( null != prop && ( prop == changeSourceProp || this.InternalIsInitializing ) )
			{
				object currVal = this.GetValue( prop );
				// SSP 1/16/12 TFS59404
				// If it's primitive type then do value comparison.
				// 
				//if ( value == currVal )
				if ( null != value && value.GetType( ).IsPrimitive ? Utils.AreEqual( value, currVal ) : value == currVal )
					return;
			}

			this.SetValue( prop, value );
		}

		#endregion // Sync_SetValueHelper

		#region ToggleDropDown

		/// <summary>
		/// Toggles the drop down state, entering edit mode if necessary.
		/// </summary>
        // AS 9/3/08 NA 2008 Vol 2
        // Changed to internal virtual on TextEditorBase
        //
		//private void ToggleDropDown( )
		internal override void ToggleDropDown( )
		{
			this.IsDropDownOpen = !this.IsDropDownOpen;
		}

		#endregion // ToggleDropDown

        // TK 18/09/08 - added automation support for XamComboEditor's ExpandCollapse pattern
        #region RaiseAutomationExpandCollapseStateChanged

        private void RaiseAutomationExpandCollapseStateChanged(bool oldValue, bool newValue)
        {
            XamComboEditorAutomationPeer peer = UIElementAutomationPeer.FromElement(this) as XamComboEditorAutomationPeer;

            if (null != peer)
                peer.RaiseExpandCollapseStateChanged(oldValue, newValue);
        }

        #endregion //RaiseAutomationExpandCollapseStateChanged	

		#region UpdateComboBox

		private void InitializeComboBoxValue( bool onlyUpdateWhenInEditMode )
		{
			bool origInUpdateComboBox = _isInitializingComboBox;
			_isInitializingComboBox = true;

			try
			{
				if ( !onlyUpdateWhenInEditMode || this.IsInEditMode )
				{
					ComboBox comboBox = this.ComboBox;
					if ( null != comboBox )
					{
						if ( this.SelectedIndex >= 0 )
						{
							comboBox.SelectedIndex = this.SelectedIndex;
						}
						else
						{
							comboBox.SelectedIndex = -1;
							comboBox.Text = this.Text;
						}
					}
				}
			}
			finally
			{
				_isInitializingComboBox = origInUpdateComboBox;
			}
		}

		#endregion // UpdateComboBox

		#region UpdateDropDownButtonVisibility

        
#region Infragistics Source Cleanup (Region)































#endregion // Infragistics Source Cleanup (Region)

        #endregion // UpdateDropDownButtonVisibility

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

		#region ICommandHost Members

		#region ICommandHost.CurrentState

		// SSP 3/18/10 TFS29783 - Optimizations
		// Changed CurrentState property to a method.
		// 
		Int64 ICommandHost.GetCurrentState( Int64 statesToQuery )
		{
			ComboEditorStates state = 0;

			if ( this.IsInEditMode )
				state |= ComboEditorStates.IsInEditMode;

			if ( this.IsDropDownOpen )
				state |= ComboEditorStates.IsDropDownOpen;

			return (long)state & statesToQuery;
		}

		#endregion //ICommandHost.CurrentState

		#region ICommandHost.CanExecute

		// AS 2/5/08 ExecuteCommandInfo
		//bool ICommandHost.CanExecute( RoutedCommand command, object commandParameter )
		bool ICommandHost.CanExecute(ExecuteCommandInfo commandInfo)
		{
			RoutedCommand command = commandInfo.RoutedCommand;

			// SSP 6/7/07 BR22768
			// If the command is NotACommand then always return true as it's
			// also allowed.
			// 
			if ( ComboEditorCommands.NotACommand == command )
				return true;

			return command != null && command.OwnerType == typeof( ComboEditorCommands );
		}

		#endregion //ICommandHost.CanExecute

		#region ICommandHost.Execute

		// AS 2/5/08 ExecuteCommandInfo
		//void ICommandHost.Execute(ExecutedRoutedEventArgs args)
		bool ICommandHost.Execute(ExecuteCommandInfo commandInfo)
		{
			// AS 2/5/08 ExecuteCommandInfo
			//RoutedCommand command = args.Command as RoutedCommand;
			//if ( command != null )
			//	args.Handled = this.ExecuteCommandImpl( command, args.Parameter );
			RoutedCommand command = commandInfo.RoutedCommand;
			return command != null && this.ExecuteCommandImpl( command, commandInfo.Parameter );
		}

		private bool ExecuteCommandImpl( RoutedCommand command, object commandParameter )
		{
			// Make sure we have a command to execute.
			if ( command == null )
				throw new ArgumentNullException( "command" );


			// Make sure the minimal control state exists to execute the command.
			if ( ComboEditorCommands.IsMinimumStatePresentForCommand( this as ICommandHost, command ) == false )
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

			if ( ComboEditorCommands.NotACommand == command )
			{
				handled = true;
			}
			else if ( ComboEditorCommands.ToggleDropDown == command )
			{
				this.ToggleDropDown( );

				handled = true;
			}

			// If the command was executed, fire the 'after executed' event.
			if ( handled == true )
				this.RaiseExecutedCommand( new ExecutedCommandEventArgs( command ) );

			return handled;
		}

		#endregion //ICommandHost.Execute

		#endregion //ICommandHost Members
	}


	#region ComboEditorStates

	/// <summary>
	/// Represents the different states of the combo editor control.  Used to evaluate whether a specific command can be executed.
	/// </summary>
	[Flags]
	public enum ComboEditorStates : long
	{
		/// <summary>
		/// Indicates whether the editor is in edit mode
		/// </summary>
		IsInEditMode = 0x00000001,

		/// <summary>
		/// Indicates whether the drop down is currenty dropped down
		/// </summary>
		IsDropDownOpen = 0x00000002,
	};

	#endregion // ComboEditorStates

	#region ComboEditorCommands Class

	/// <summary>
	/// Provides the list of RoutedCommands supported by the XamComboEditor. 
	/// </summary>
	public class ComboEditorCommands : Commands<XamComboEditor>
	{
		// ====================================================================================================================================
		// ADD NEW COMMANDS HERE with the minimum required control state (also add a CommandWrapper for each command to the CommandWrappers array
		// below which will let you specify the triggering KeyGestures and required/disallowed states)
		//
		// Note that while individual commands in this static list are defined as type RoutedCommand or RoutedUICommand,
		// we actually create IGRoutedCommands or IGRoutedUICommands (both derived from RoutedCommand) so we can specify
		// and store the minimum control state needed to execute the command.
		// ------------------------------------------------------------------------------------------------------------------------------------
		//

		#region Command Definitions

		/// <summary>
		/// Represents a command which is always ignored.
		/// </summary>
		public static readonly RoutedCommand NotACommand = ApplicationCommands.NotACommand;
		//  . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 

		/// <summary>
		/// Command for toggling the drop down state of the ComboEditor. If the editor is not in edit mode,
		/// this command will put the editor in edit mode.
		/// </summary>
		public static readonly RoutedCommand ToggleDropDown = new IGRoutedCommand( "ToggleDropDown",
																					  typeof( ComboEditorCommands ),
																					  (Int64)0,
																					  (Int64)0 );
		//  . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . 
		
		#endregion //Command Definitions

		// ====================================================================================================================================


		// ====================================================================================================================================
		// ADD COMMANDWRAPPERS HERE FOR EACH COMMAND DEFINED ABOVE.
		// ------------------------------------------------------------------------------------------------------------------------------------
		//
		/// <summary>
		/// The list of CommandWrappers for each supported command.
		/// </summary>
		/// <seealso cref="Infragistics.Windows.Commands.CommandWrapper"/>

		#region CommandWrapper Definitions

		private static CommandWrapper[] GetCommandWrappers( )
		{
			return new CommandWrapper[] {
				//					RoutedCommand					StateDisallowed					StateRequired								InputGestures
				//					=============					===============					=============								=============
				// move the caret to right by one ( Right )
				new CommandWrapper(	
					ToggleDropDown,	// Action
					(Int64)0,	// Disallowed state		
					(Int64)0,	// Required state
					new InputGesture[] { },
					ModifierKeys.None )
				// ------------------------------------------------------------------------}
			};
		}
		#endregion //CommandWrapper Definitions

		// ====================================================================================================================================


		static ComboEditorCommands( )
		{
			// Call the Initialize method of our base class Commands<T> to register bindings for the commands represented
			// by our CommandWrappers.
			Commands<XamComboEditor>.Initialize( ComboEditorCommands.GetCommandWrappers( ) );
		}


		/// <summary>
		/// This method is provided as a convenience for initializing the statics in this class which kicks off
		/// the process of setting up and registering the commands.
		/// </summary>
		public static void LoadCommands( )
		{
		}

		private static ComboEditorCommands g_instance;
		internal static ComboEditorCommands Instance
		{
			get
			{
				if ( g_instance == null )
					g_instance = new ComboEditorCommands( );

				return g_instance;
			}
		}
	}

	#endregion // ComboEditorCommands Class


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