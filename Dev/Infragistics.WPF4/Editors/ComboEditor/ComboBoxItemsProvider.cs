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
using System.Windows.Controls.Primitives;
using System.Xml;
using System.Reflection;

namespace Infragistics.Windows.Editors
{
	#region ComboBoxItemsProvider Class

	/// <summary>
	/// Used for specifying items on a <see cref="XamComboEditor"/>.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// <see cref="XamComboEditor"/> value editor is not an ItemsControl. The way you specify
	/// the items to display it its drop-down is by setting its <see cref="XamComboEditor.ItemsProvider"/>
	/// property.
	/// </para>
	/// <seealso cref="XamComboEditor"/>
	/// </remarks>
	[ContentProperty( "Items" ), DefaultProperty( "Items" )]
	public class ComboBoxItemsProvider : DependencyObject
	{
		#region Nested Data Structures

		#region ComboEditorDefaultConverter Class

		internal class ComboEditorDefaultConverter : ValueEditorDefaultConverter
		{
			private static ComboEditorDefaultConverter _converter;

			// SSP 4/28/11 TFS65229
			// 
			private static ComboEditorDefaultConverter _displayTextConverter;

			protected ComboEditorDefaultConverter( bool isDisplayTextConverter )
				: base( isDisplayTextConverter )
			{
			}

			public static ComboEditorDefaultConverter Converter
			{
				get
				{
					if ( null == _converter )
						_converter = new ComboEditorDefaultConverter( false );

					return _converter;
				}
			}

			// SSP 4/28/11 TFS65229
			// 
			public static ComboEditorDefaultConverter DisplayTextConverter
			{
				get
				{
					if ( null == _displayTextConverter )
						_displayTextConverter = new ComboEditorDefaultConverter( true );

					return _displayTextConverter;
				}
			}

			protected override object ConvertHelper( bool convertingBack, object value, Type targetType,
				ValueEditor valueEditor, IFormatProvider formatProvider, string format )
			{
				XamComboEditor editor = valueEditor as XamComboEditor;
				ComboBoxItemsProvider itemsProvider = null != editor ? editor.ItemsProvider : null;

				if ( null != itemsProvider )
				{
					// SSP 2/20/09 TFS13641
					// Added support for duplicate data values and/or display texts - basically multiple items with 
					// the same data value and/or display text. Added preferredIndex parameter.
					// 
					int preferredIndex = editor.SelectedIndex;

					if ( convertingBack )
					{
						// Converting back from display text to value.

						string displayText = value as string;
						if ( null == displayText )
							displayText = null != value ? value.ToString( ) : string.Empty;

						// SSP 10/24/08 TFS9442
						// 
						//return itemsProvider.GetDataValueFromDisplayText( displayText, false );
						object dataVal;
						// SSP 2/20/09 TFS13641
						// 
						//if ( itemsProvider.GetDataValueFromDisplayText( displayText, false, out dataVal ) )
						if ( itemsProvider.GetDataValueFromDisplayText( displayText, false, preferredIndex, out dataVal ) )
						{
							// SSP 5/11/11 TFS36390
							// 
							if ( null != dataVal && !targetType.IsInstanceOfType( dataVal ) )
								dataVal = Utilities.ConvertDataValue( dataVal, targetType, formatProvider, format );

							return dataVal;
						}
					}
					else
					{
						// SSP 2/20/09 TFS13641
						// 
						//return itemsProvider.GetDisplayTextFromDataValue( value );
						// SSP 5/4/11 TFS65229
						// Take into account NullText.
						// 
						// --------------------------------------------------------------------------------
						//return itemsProvider.GetDisplayTextFromDataValue( value, preferredIndex );
						string tmp = itemsProvider.GetDisplayTextFromDataValue( value, preferredIndex );
						if ( _isDisplayTextConverter && string.IsNullOrEmpty( tmp ) && ( null == value || DBNull.Value == value ) )
							tmp = editor.NullText;

						return tmp;
						// --------------------------------------------------------------------------------
					}
				}

				return base.ConvertHelper( convertingBack, value, targetType, valueEditor, formatProvider, format );
			}
		}

		#endregion // ComboEditorDefaultConverter Class

		#region ItemHolder Class

		private class ItemHolder
		{
			internal object _item;
			internal object _dataValue;
			internal string _displayText;
			internal int _itemIndex;

			internal ItemHolder( object item, int itemIndex, object dataValue, string displayText )
			{
				_item = item;
				_dataValue = dataValue;
				_displayText = displayText;
				_itemIndex = itemIndex;
			}

			internal void HookUnhookDataItem( CachedInfo cacheInfo, bool hook )
			{
				INotifyPropertyChanged nc = _item as INotifyPropertyChanged;
				if ( null != nc )
				{
					if ( hook )
						nc.PropertyChanged += cacheInfo._itemPropChangeHandler;
					else
						nc.PropertyChanged -= cacheInfo._itemPropChangeHandler;
				}
			}
		}

		#endregion // ItemHolder Class

		#region MemberPathEvaluator Class

		// SSP 8/7/09 TFS19798
		// 
		private class MemberPathEvaluator
		{
			#region Member Vars

			private string _memberPath;
			private bool _isMemberPathEmpty;
			private Type _componentType;
			private PropertyDescriptor _pd;

			#endregion // Member Vars

			#region Constructor

			public MemberPathEvaluator( string memberPath )
			{
				_memberPath = memberPath;
				_isMemberPathEmpty = string.IsNullOrEmpty( memberPath );
			}

			#endregion // Constructor

			#region GetMemberValue

			public bool GetMemberValue( object dataItem, out object val )
			{
				val = null;

				if ( _isMemberPathEmpty )
				{
					val = dataItem;
					return true;
				}

				if ( null != dataItem )
				{
					if ( null == _componentType || !_componentType.IsInstanceOfType( dataItem ) )
					{
						_componentType = dataItem.GetType( );
						try
						{
							PropertyDescriptorCollection properties = TypeDescriptor.GetProperties( _componentType );
							_pd = null != properties ? properties.Find( _memberPath, false ) : null;
						}
						catch
						{
							_pd = null;
						}
					}

					if ( null != _pd )
					{
						try
						{
							val = _pd.GetValue( dataItem );
							return true;
						}
						catch
						{
							// If getting the value fails then we should null out _pd so we don't make this
							// attempt for every item and potentially cause exception in each case.
							// 
							_pd = null;
						}
					}
				}

				return false;
			}

			#endregion // GetMemberValue

			#region GetMemberValueAsString

			public bool GetMemberValueAsString( object dataItem, out string val )
			{
				val = null;

				object tmp;
				bool ret = this.GetMemberValue( dataItem, out tmp );
				if ( ret )
					val = null == tmp || tmp is string ? (string)tmp : tmp.ToString( );

				return ret;
			}

			#endregion // GetMemberValueAsString
		}

		#endregion // MemberPathEvaluator Class

		#endregion // Nested Data Structures

		#region Variables

		private ComboBox _comboBox;
		private CachedInfo _cachedInfo;
		private IComparer _cachedValueComparer;
		
		
		private IComparer _cachedDisplayTextComparer;

		// SSP 8/7/09 TFS19798 - Optimizations
		// 
		private IItemMemberProvider _cachedItemMemberProvider;

		#endregion // Variables

		#region Constructor

		/// <summary>
		/// Constructor. Initializes a new instance of <see cref="ComboBoxItemsProvider"/> class.
		/// </summary>
		public ComboBoxItemsProvider( )
		{
			_comboBox = new ComboBox( );
		}

		#endregion // Constructor

		#region Properties

		#region Private/Internal Properties

		#region ComboBox

		private ComboBox ComboBox
		{
			get
			{
				return _comboBox;
			}
		}

		#endregion // ComboBox

		#region Version

		// SSP 1/19/09 - NAS9.1 Record Filtering
		// Added code to handle case where the items of the ComboBoxItemsProvider are 
		// changed - either its ItemsSource is changed or the items source list is changed.
		// This is more likely to happen with the record filtering functionality where
		// the drop-down list is lazily populated.
		// 
		/// <summary>
		/// Identifies the <see cref="Version"/> dependency property.
		/// </summary>
		internal static readonly DependencyProperty VersionProperty = DependencyProperty.Register(
  			"Version",
			typeof( int ),
			typeof( ComboBoxItemsProvider ),
			new FrameworkPropertyMetadata( 0 ) 
		);

		#endregion // Version

		#endregion // Private/Internal Properties

		#region Public Properties

		#region DisplayMemberPath

		/// <summary>
		/// Identifies the <see cref="DisplayMemberPath"/> dependency property
		/// </summary>
		public static readonly DependencyProperty DisplayMemberPathProperty = DependencyProperty.Register(
			"DisplayMemberPath",
			typeof( string ),
			typeof( ComboBoxItemsProvider ),
			new FrameworkPropertyMetadata( null, FrameworkPropertyMetadataOptions.None,
				new PropertyChangedCallback( OnDisplayMemberPathChanged ) )
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
			string newVal = (string)e.NewValue;
			ComboBoxItemsProvider itemsProvider = (ComboBoxItemsProvider)dependencyObject;
			itemsProvider._comboBox.DisplayMemberPath = newVal;

			
			
			
			itemsProvider.DirtyCachedItems( );
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

		#region DisplayTextComparer

		/// <summary>
		/// Identifies the <see cref="DisplayTextComparer"/> dependency property
		/// </summary>
		public static readonly DependencyProperty DisplayTextComparerProperty = DependencyProperty.Register(
			"DisplayTextComparer",
			typeof( IComparer ),
			typeof( ComboBoxItemsProvider ),
			new FrameworkPropertyMetadata( null, FrameworkPropertyMetadataOptions.None,
				new PropertyChangedCallback( OnDisplayTextComparerChanged ),
				null )
			);

		/// <summary>
		/// Specifies a custom comparer to use for comparing display text associated with each combo-box item.
		/// </summary>
		//[Description( "Specifies a custom comparer to use for comparing display text of each item." )]
		//[Category( "Behavior" )]
		[Bindable( true )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public IComparer DisplayTextComparer
		{
			get
			{
				
				
				//return (IComparer)this.GetValue( DisplayTextComparerProperty );
				return _cachedDisplayTextComparer;
			}
			set
			{
				this.SetValue( DisplayTextComparerProperty, value );
			}
		}

		private static void OnDisplayTextComparerChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			ComboBoxItemsProvider ip = (ComboBoxItemsProvider)dependencyObject;

			
			
			ip._cachedDisplayTextComparer = (IComparer)e.NewValue;

			ip.DirtyCachedItems( );
		}

		#endregion // DisplayTextComparer

		#region ItemMemberProvider

		// SSP 8/7/09 TFS19798 - Optimizations
		// Added ItemMemberProvider property.
		// 

		/// <summary>
		/// Identifies the <see cref="ItemMemberProvider"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty ItemMemberProviderProperty = DependencyProperty.Register(
			"ItemMemberProvider",
			typeof( IItemMemberProvider ),
			typeof( ComboBoxItemsProvider ),
			new FrameworkPropertyMetadata( null, FrameworkPropertyMetadataOptions.None,
				new PropertyChangedCallback( OnItemMemberProviderChanged ) )
		);

		/// <summary>
		/// Specifies an interface that the ComboBoxItemsProvider will use to get value and display text
		/// for each data item in its items source.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>Note</b> that you don't normally have to provide this interface. The ComboBoxItemsProvider
		/// has default logic to get the value and display text for each item based on the <see cref="ValuePath"/>
		/// and <see cref="DisplayMemberPath"/> properties. However you can provide this interface if you
		/// have custom logic for generating value and display texts or to improve performance when you
		/// have a large number of data items in the items source, since the default logic involves using
		/// reflection to evaluate value and display member paths. Note that if you use <see cref="ComboBoxDataItem"/>
		/// objects as data items then this property will be ignored since such an interface is not 
		/// necessary in that case.
		/// </para>
		/// <seealso cref="ComboBoxDataItem"/>
		/// <seealso cref="ValuePath"/>
		/// <seealso cref="DisplayMemberPath"/>
		/// </remarks>
		[Bindable( true )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public IItemMemberProvider ItemMemberProvider
		{
			get
			{
				return (IItemMemberProvider)this.GetValue( ItemMemberProviderProperty );
			}
			set
			{
				this.SetValue( ItemMemberProviderProperty, value );
			}
		}

		private static void OnItemMemberProviderChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			ComboBoxItemsProvider ip = (ComboBoxItemsProvider)dependencyObject;
			ip._cachedItemMemberProvider = (IItemMemberProvider)e.NewValue;

			ip.DirtyCachedItems( );
		}

		#endregion // ItemMemberProvider

		#region Items

		private ItemCollection _cachedItems;

		/// <summary>
		/// Returns the items to be displayed in the drop-down portion of this XamComboEditor.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// See <see cref="ItemsSource"/> for more information on how to specify items for the
		/// XamComboEditor's drop-down.
		/// </para>
		/// <seealso cref="ItemsSource"/>
		/// <seealso cref="XamComboEditor"/>
		/// </remarks>
		//[Description( "Specifies the items to display in the drop-down list of the associated XamComboEditor" )]
		//[Category( "Data" )]
		[Bindable( true ), DesignerSerializationVisibility( DesignerSerializationVisibility.Content )]
		public ItemCollection Items
		{
			get
			{
				ItemCollection items = _comboBox.Items;

				if ( _cachedItems != items )
				{
					if ( null != _cachedItems )
						( (ICollectionView)_cachedItems ).CollectionChanged -= new NotifyCollectionChangedEventHandler( OnItems_CollectionChanged );

					_cachedItems = items;

					if ( null != _cachedItems )
						( (ICollectionView)_cachedItems ).CollectionChanged += new NotifyCollectionChangedEventHandler( OnItems_CollectionChanged );
				}

				return items;
			}
		}

		private void OnItems_CollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
		{
			this.DirtyCachedItems( );
		}

		// SSP 7/17/09 TFS18423
		// If the items source is a binding list that's sending out ItemChanged notifications,
		// make use those notifications to refresh cached data. ICollectionView's CollectionChanged
		// will not get raised in the cases where simply a property on the item has changed.
		// 
		private void OnItemsSource_ListChanged( object sender, ListChangedEventArgs e )
		{
			switch ( e.ListChangedType )
			{
				case ListChangedType.ItemChanged:
					{
						// If some property other than the DisplayMemberPath
						// and ValuePath has changed its value then ignore it.
						// 
						PropertyDescriptor pd = e.PropertyDescriptor;
						if ( null != pd && pd.Name != this.DisplayMemberPath
							&& pd.Name != this.ValuePath )
							return;
					}
					break;
			}

			this.DirtyCachedItems( );
		}

		#endregion // Items

		#region ItemsSource

		/// <summary>
		/// Identifies the <see cref="ItemsSource"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
			"ItemsSource",
			typeof( IEnumerable ),
			typeof( ComboBoxItemsProvider ),
			new FrameworkPropertyMetadata( null, FrameworkPropertyMetadataOptions.None,
				new PropertyChangedCallback( OnItemsSourceChanged ) )
			);

		/// <summary>
		/// Specifies the collection from which to populate the drop-down list of the associated XamComboEditor.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// There are two ways you can specify the items to display in the drop-down of the XamComboEditor.
		/// You can set the ItemsSource property to a collection of items to display in the drop-down or
		/// you can add items to the collection returned <see cref="ComboBoxItemsProvider.Items"/> property.
		/// Note that you can not do both at the same time. When you specify the <b>ItemsSource</b> property,
		/// the <b>Items</b> property will return a collection that's read-only. It will reflect the same
		/// items as the ItemsSource however it will be read-only. Any changes made to the ItemsSource will
		/// also be reflected by the Items property.
		/// </para>
		/// </remarks>
		//[Description( "Specifies the collection from which to populate the drop down of this XamComboEditor" )]
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
			IEnumerable newVal = (IEnumerable)e.NewValue;
			ComboBoxItemsProvider comboBoxItemsProvider = (ComboBoxItemsProvider)dependencyObject;

			// SSP 7/17/09 TFS18423
			// If the items source is a binding list that's sending out ItemChanged notifications,
			// make use those notifications to refresh cached data. ICollectionView's CollectionChanged
			// will not get raised in the cases where simply a property on the item has changed.
			// 
			// ----------------------------------------------------------------------------------------
			IEnumerable oldVal = (IEnumerable)e.OldValue;
			IBindingList oldBindingList = oldVal as IBindingList;
			if ( null != oldBindingList )
				oldBindingList.ListChanged -= new ListChangedEventHandler( comboBoxItemsProvider.OnItemsSource_ListChanged );

			IBindingList newBindingList = newVal as IBindingList;
			if ( null != newBindingList )
				newBindingList.ListChanged += new ListChangedEventHandler( comboBoxItemsProvider.OnItemsSource_ListChanged );
			// ----------------------------------------------------------------------------------------

			comboBoxItemsProvider._comboBox.ItemsSource = newVal;

			comboBoxItemsProvider.DirtyCachedItems( );

			//comboBoxItemsProvider.SetValue( ItemsPropertyKey, comboBoxItemsProvider._comboBox.Items );
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

		#region ValueComparer

		/// <summary>
		/// Identifies the <see cref="ValueComparer"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ValueComparerProperty = DependencyProperty.Register(
			"ValueComparer",
			typeof( IComparer ),
			typeof( ComboBoxItemsProvider ),
			new FrameworkPropertyMetadata( null, FrameworkPropertyMetadataOptions.None,
				new PropertyChangedCallback( OnValueComparerChanged ),
				null )
			);

		/// <summary>
		/// Specifies a custom comparer to use for comparing values associated with each combo-box item.
		/// </summary>
		//[Description( "Specifies a custom comparer to use for comparing values." )]
		//[Category( "Behavior" )]
		[Bindable( true )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public IComparer ValueComparer
		{
			get
			{
				return _cachedValueComparer;
			}
			set
			{
				this.SetValue( ValueComparerProperty, value );
			}
		}

		private static void OnValueComparerChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			ComboBoxItemsProvider ip = (ComboBoxItemsProvider)dependencyObject;
			ip._cachedValueComparer = (IComparer)e.NewValue;

			
			
			
			ip.DirtyCachedItems( );
		}

		#endregion // ValueComparer

		#region ValuePath

		/// <summary>
		/// Identifies the <see cref="ValuePath"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ValuePathProperty = DependencyProperty.Register(
			"ValuePath",
			typeof( string ),
			typeof( ComboBoxItemsProvider ),
			new FrameworkPropertyMetadata( null, FrameworkPropertyMetadataOptions.None,
				new PropertyChangedCallback( OnValuePathChanged ) )
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
			string newVal = (string)e.NewValue;
			ComboBoxItemsProvider itemsProvider = (ComboBoxItemsProvider)dependencyObject;
			itemsProvider._comboBox.SelectedValuePath = newVal;

			
			
			
			itemsProvider.DirtyCachedItems( );
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

		#endregion // Public Properties

		#endregion // Properties

		#region Methods

		#region Private/Internal Methods

		#region DirtyCachedItems

		/// <summary>
		/// Dirties the cached items and sort indexs.
		/// </summary>
		private void DirtyCachedItems( )
		{
			if ( null != _cachedInfo )
			{
				// SSP 1/16/12 TFS87805
				// 
				_cachedInfo.Dispose( );

				_cachedInfo = null;
			}

			// SSP 1/19/09 - NAS9.1 Record Filtering
			// 
			this.SetValue( VersionProperty, 1 + (int)this.GetValue( VersionProperty ) );
		}

		#endregion // DirtyCachedItems

		#region FindItemHolderFromDataValueHelper

		
		
		
		
		/// <summary>
		/// Finds ItemHolder for the specified data value.
		/// </summary>
		/// <param name="dataValue">Data value for which to get the item holder.</param>
		/// <param name="preferredIndex">This only matters when there are multiple items with the same data values. 
		/// In that case if the item at this index matches the specified data value then the associated item holder will
		/// be returned. Otherwise the first matching item holder will be returned.
		/// </param>
		/// <param name="itemHolder">This out parameter will be set to the found item holder.</param>
		/// <returns></returns>
		private bool FindItemHolderFromDataValueHelper( object dataValue, int preferredIndex, out ItemHolder itemHolder )
		{
			itemHolder = null;

			if ( !this.VerifyCachedItems( ) )
				return false;

			
			
			
			
			
			
			
			CachedInfo cachedInfo = _cachedInfo;
			bool doBinarySearch = cachedInfo._canUseBinarySearch;
			if ( doBinarySearch && null != cachedInfo._compareType 
				&& ! cachedInfo._compareDataValuesAsText && null != dataValue && DBNull.Value != dataValue )
			{
				object val = IndexComparer.ConvertDataValue( cachedInfo, dataValue, cachedInfo._compareType );
				if ( null != val )
					dataValue = val;
				else
					doBinarySearch = false;
			}
			

			ItemHolder[] items = cachedInfo._items;

			
			
			
			
			if ( preferredIndex >= 0 && preferredIndex < items.Length )
			{
				ItemHolder item = items[preferredIndex];
				if ( IndexComparer.DataValueEquals( cachedInfo, dataValue, item._dataValue ) )
				{
					itemHolder = item;
					return true;
				}
			}

			// SSP 8/20/08 BR35749
			// Enclosed the existing code in the if block and added the else block.
			// 
			
			
			
			if ( doBinarySearch )
			{
				int[] index = cachedInfo._dataValueIndex;

				int si = 0, ei = items.Length - 1;
				int mi;

				while ( si <= ei )
				{
					mi = ( si + ei ) / 2;

					ItemHolder item = items[index[mi]];

					
					
					
					
					int r = IndexComparer.CompareHelper( cachedInfo, dataValue, item._dataValue, cachedInfo._compareDataValuesAsText );

					if ( 0 == r )
					{
						itemHolder = item;
						break;
					}

					if ( r < 0 )
						ei = mi - 1;
					else
						si = mi + 1;
				}
			}
			else
			{
				for ( int i = 0; i < items.Length; i++ )
				{
					ItemHolder item = items[i];

					if ( IndexComparer.DataValueEquals( cachedInfo, dataValue, item._dataValue ) )
					{
						itemHolder = item;
						break;
					}
				}
			}

			return true;
		}

		#endregion // FindItemHolderFromDataValueHelper

		#region FindItemHolderFromDisplayTextHelper

		
		
		
		
		private bool FindItemHolderFromDisplayTextHelper( string displayText, bool partial, int startIndex, int preferredIndex, out ItemHolder itemHolder )
		{
			itemHolder = null;

			if ( !this.VerifyCachedItems( ) )
				return false;

			ItemHolder[] items = _cachedInfo._items;

			
			
			
			
			if ( preferredIndex >= startIndex && preferredIndex < items.Length )
			{
				ItemHolder item = items[ preferredIndex ];
				if ( IndexComparer.DisplayTextEquals( this, displayText, item._displayText, partial ) )
				{
					itemHolder = item;
					return true;
				}
			}

			// SSP 8/20/08 BR35749
			// Enclosed the existing code in the if block and added the else block.
			// 
			if ( _cachedInfo._canUseBinarySearch )
			{
				int[] index = _cachedInfo._displayTextIndex;

				int si = startIndex, ei = items.Length - 1;
				int mi = ( si + ei ) / 2;

				while ( si <= ei )
				{
					mi = ( si + ei ) / 2;

					ItemHolder item = items[index[mi]];

					int r = IndexComparer.CompareDisplayText( this, displayText, item._displayText );

					if ( 0 == r )
					{
						itemHolder = item;
						break;
					}

					if ( r < 0 )
						ei = mi - 1;
					else
						si = mi + 1;
				}

				if ( null == itemHolder && partial )
				{
					while ( mi < items.Length )
					{
						ItemHolder item = items[index[mi]];
						if ( IndexComparer.DisplayTextEquals( this, displayText, item._displayText, true ) )
						{
							itemHolder = item;
							break;
						}

						mi++;
					}
				}
			}
			else
			{
				// SSP 8/20/08 BR35749
				// 
				for ( int i = startIndex; i < items.Length; i++ )
				{
					ItemHolder item = items[i];

					if ( IndexComparer.DisplayTextEquals( this, displayText, item._displayText, partial ) )
					{
						itemHolder = item;
						break;
					}
				}
			}

			return true;
		}

		#endregion // FindItemHolderFromDisplayTextHelper

		#region FindListItemFromDisplayText

		
		
		
		
		internal int FindListItemFromDisplayTextHelper( string displayText, bool partial, int startIndex, int preferredIndex, out object foundListItem )
		{
			ItemCollection items = _comboBox.Items;

			
			
			
			
			if ( preferredIndex >= startIndex && preferredIndex < items.Count )
			{
				object item = items[preferredIndex];
				string iiDisplayText = this.GetDisplayTextHelper( item, preferredIndex );
				if ( IndexComparer.DisplayTextEquals( this, displayText, iiDisplayText, partial ) )
				{
					foundListItem = item;
					return preferredIndex;
				}
			}

			for ( int i = startIndex; i < items.Count; i++ )
			{
				object item = items[i];
				string iiDisplayText = this.GetDisplayTextHelper( item, i );

				if ( IndexComparer.DisplayTextEquals( this, displayText, iiDisplayText, partial ) )
				{
					foundListItem = item;
					return i;
				}
			}

			foundListItem = null;
			return -1;
		}

		
		
		
		
		internal int FindListItemFromDisplayText( string displayText, bool partial, int startIndex, int preferredIndex, out object foundListItem )
		{
			ItemHolder itemHolder;
			if ( this.FindItemHolderFromDisplayTextHelper( displayText, partial, startIndex, preferredIndex, out itemHolder ) )
			{
				if ( null != itemHolder )
				{
					foundListItem = itemHolder._item;
					return itemHolder._itemIndex;
				}

				foundListItem = null;
				return -1;
			}
			else
			{
				return this.FindListItemFromDisplayTextHelper( displayText, partial, startIndex, preferredIndex, out foundListItem );
			}
		}

		#endregion // FindListItemFromDisplayText

		#region FindListItemFromDataValue

		
		
		
		
		internal int FindListItemFromDataValueHelper( object dataValue, int preferredIndex, out object foundListItem )
		{
			
			
			
			
			if ( preferredIndex >= 0 && preferredIndex < _comboBox.Items.Count )
			{
				_comboBox.SelectedIndex = preferredIndex;
				if ( _comboBox.SelectedIndex == preferredIndex 
					&& Utils.AreEqual( dataValue, _comboBox.SelectedValue ) )
				{
					foundListItem = _comboBox.Items[ preferredIndex ];
					return preferredIndex;
				}
			}

			_comboBox.SelectedValue = dataValue;

			int selectedIndex = _comboBox.SelectedIndex;
			if ( selectedIndex >= 0 )
			{
				foundListItem = _comboBox.Items[selectedIndex];
				return selectedIndex;
			}

			foundListItem = null;
			return -1;
		}

		
		
		
		
		internal int FindListItemFromDataValue( object dataValue, int preferredIndex, out object foundListItem )
		{
			ItemHolder itemHolder;
			if ( this.FindItemHolderFromDataValueHelper( dataValue, preferredIndex, out itemHolder ) )
			{
				if ( null != itemHolder )
				{
					foundListItem = itemHolder._item;
					return itemHolder._itemIndex;
				}

				foundListItem = null;
				return -1;
			}
			else
			{
				return this.FindListItemFromDataValueHelper( dataValue, preferredIndex, out foundListItem );
			}
		}

		#endregion // FindListItemFromDataValue

		#region GetDataValue

		private object GetDataValueHelper( object item, int itemIndexIfKnown )
		{
			ComboBoxDataItem vlItem = item as ComboBoxDataItem;
			if ( null != vlItem )
				return vlItem.Value;

			// SSP 8/7/09 TFS19798 - Optimizations
			// Added ItemMemberProvider property.
			// 
			if ( null != _cachedItemMemberProvider )
				return _cachedItemMemberProvider.GetValue( item );

			if ( itemIndexIfKnown >= 0 )
				_comboBox.SelectedIndex = itemIndexIfKnown;
			else
				_comboBox.SelectedItem = item;

			return _comboBox.SelectedValue;
		}

		// SSP 2/20/09 TFS13641
		// Added support for duplicate data values and/or display texts - basically multiple 
		// items with the same data value and/or display text. Added itemIndexIfKnown parameter.
		// 
		internal object GetDataValue( object listObject, int itemIndexIfKnown )
		{
			if ( this.VerifyCachedItems( ) )
			{
				ItemHolder item;
				if ( _cachedInfo._itemsTable.TryGetValue( Utils.DictionaryKey( listObject ), out item ) )
					return item._dataValue;
			}

			return this.GetDataValueHelper( listObject, itemIndexIfKnown );
		}

		#endregion // GetDataValue

		#region GetDataValueFromDisplayText

		
		
		
		
		
		
#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)

		
		
		
		
		internal bool GetDataValueFromDisplayText( string displayText, bool partialMatch, int preferredIndex, out object dataValue )
		{
			object listItem;
			int index = this.FindListItemFromDisplayText( displayText, partialMatch, 0, preferredIndex, out listItem );

			if ( index >= 0 )
			{
				
				
				dataValue = this.GetDataValue( listItem, index );
				return true;
			}

			dataValue = null;
			return false;
		}
		

		#endregion // GetDataValueFromDisplayText

		#region GetDisplayText

		private string GetDisplayTextHelper( object item, int itemIndexIfKnown  )
		{
			ComboBoxDataItem vlItem = item as ComboBoxDataItem;
			if ( null != vlItem )
				return vlItem.DisplayText;

			// SSP 8/7/09 TFS19798 - Optimizations
			// Added ItemMemberProvider property.
			// 
			if ( null != _cachedItemMemberProvider )
				return _cachedItemMemberProvider.GetDisplayText( item );

			if ( itemIndexIfKnown >= 0 )
				_comboBox.SelectedIndex = itemIndexIfKnown;
			else
				_comboBox.SelectedItem = item;

			return _comboBox.Text;
		}

		// SSP 2/20/09 TFS13641
		// Added support for duplicate data values and/or display texts - basically multiple 
		// items with the same data value and/or display text. Added itemIndexIfKnown parameter.
		// 
		internal string GetDisplayText( object listObject, int itemIndexIfKnown )
		{
			if ( this.VerifyCachedItems( ) )
			{
				ItemHolder item;
				if ( _cachedInfo._itemsTable.TryGetValue( Utils.DictionaryKey( listObject ), out item ) )
					return item._displayText;
			}

			return this.GetDisplayTextHelper( listObject, itemIndexIfKnown );
		}

		#endregion // GetDisplayText

		#region GetDisplayTextFromDataValue

		
		
		
		
		internal string GetDisplayTextFromDataValue( object dataValue, int preferredIndex )
		{
			object listItem;
			int index = this.FindListItemFromDataValue( dataValue, preferredIndex, out listItem );
			if ( index >= 0 )
				
				
				return this.GetDisplayText( listItem, index );

			return null != dataValue ? dataValue.ToString( ) : string.Empty;
		}

		#endregion // GetDisplayTextFromDataValue

		#region GetItemAtIndex

		/// <summary>
		/// Returns the item the specified index.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		internal object GetItemAtIndex( int index )
		{
			if ( this.VerifyCachedItems( ) )
			{
				if ( index >= 0 && index < _cachedInfo._items.Length )
				{
					ItemHolder itemHolder = _cachedInfo._items[index];
					return itemHolder._item;
				}
				else
					return null;
			}

			if ( index >= 0 && index < _comboBox.Items.Count )
			{
				_comboBox.SelectedIndex = index;
				if ( _comboBox.SelectedIndex == index )
					return _comboBox.SelectedItem;
			}

			return null;
		}

		#endregion // GetItemAtIndex

		#region GetItemIndex

		/// <summary>
		/// Returns the selected index value associated with the specified listObject.
		/// </summary>
		/// <param name="listObject"></param>
		/// <returns></returns>
		internal int GetItemIndex( object listObject )
		{
			if ( this.VerifyCachedItems( ) )
			{
				ItemHolder itemHolder;
				_cachedInfo._itemsTable.TryGetValue( Utils.DictionaryKey( listObject ), out itemHolder );
				return null != itemHolder ? itemHolder._itemIndex : -1;
			}

			_comboBox.SelectedItem = listObject;
			return _comboBox.SelectedIndex;
		}

		#endregion // GetItemIndex

		#region VerifyCachedItems

		private bool VerifyCachedItems( )
		{
			if ( null != _cachedInfo )
				// SSP 8/20/08 BR35749
				// 
				//return _cachedInfo.canUseBinarySearch;
				return null != _cachedInfo._itemsTable;

			_cachedInfo = new CachedInfo( this );
			ItemCollection itemsColl = _comboBox.Items;
			
			
			
			
			List<ItemHolder> itemsList = new List<ItemHolder>( itemsColl.Count );

			// SSP 8/20/08 BR35749
			// 
			bool canUseBinarySearch = true;

			// SSP 8/7/09 TFS19798 - Optimizations
			// 
			MemberPathEvaluator valuePathEvaluator = null != _cachedItemMemberProvider ? null : new MemberPathEvaluator( this.ValuePath );
			MemberPathEvaluator displayPathEvaluator = null != _cachedItemMemberProvider ? null : new MemberPathEvaluator( this.DisplayMemberPath );

			Type lastDataValueType = null;
			bool dataValueTypesDifferent = false;
			for ( int i = 0, count = itemsColl.Count; i < count; i++ )
			{
				object item = itemsColl[i];

				if ( item is XmlNode )
				{
					// SSP 8/20/08 BR35749
					// 
					//return false;
					canUseBinarySearch = false;
					
					// SSP 8/7/09 TFS19798 - Optimizations
					// 
					valuePathEvaluator = displayPathEvaluator = null;
				}

				// SSP 8/7/09 TFS19798 - Optimizations
				// 
				// ----------------------------------------------------------------------n
				object dataValue;
				string displayText;

				if ( item is ComboBoxDataItem )
					valuePathEvaluator = displayPathEvaluator = null;

				if ( null == valuePathEvaluator || ! valuePathEvaluator.GetMemberValue( item, out dataValue ) )
					dataValue = this.GetDataValueHelper( item, i );

				if ( null == displayPathEvaluator || ! displayPathEvaluator.GetMemberValueAsString( item, out displayText ) )
					displayText = this.GetDisplayTextHelper( item, i );

				//object dataValue = this.GetDataValueHelper( item, i );
				//string displayText = this.GetDisplayTextHelper( item, i );
				// ----------------------------------------------------------------------

				ItemHolder itemHolder = new ItemHolder( item, i, dataValue, displayText );
				itemsList.Add( itemHolder );

				if ( null != dataValue && DBNull.Value != dataValue )
				{
					
					// If data values are not IComparable then we can not sort and use the
					// binary search. We do however allow for null and DBNull as special cases.
					// 
					if ( !( dataValue is IComparable ) )
					{
						// SSP 8/20/08 BR35749
						// 
						//return false;
						canUseBinarySearch = false;
					}

					Type dataValueType = dataValue.GetType( );
					if ( lastDataValueType != dataValueType && null != lastDataValueType )
						dataValueTypesDifferent = true;

					lastDataValueType = dataValueType;
				}
			}

			ItemHolder[] items = itemsList.ToArray( );
			_cachedInfo._items = items;
			_cachedInfo._compareDataValuesAsText = dataValueTypesDifferent;
			
			
			_cachedInfo._compareType = dataValueTypesDifferent ? typeof( string ) : lastDataValueType;

			int[] dataValueIndex = null, displayTextIndex = null;

			// SSP 8/20/08 BR35749
			// Enclosed the existing code in the if block.
			// 
			if ( canUseBinarySearch )
			{
				dataValueIndex = new int[items.Length];
				displayTextIndex = new int[items.Length];

				for ( int i = 0; i < dataValueIndex.Length; i++ )
				{
					dataValueIndex[i] = i;
					displayTextIndex[i] = i;
				}

				try
				{
					Utilities.SortMergeGeneric<int>( dataValueIndex, new IndexComparer( _cachedInfo, false ) );
					Utilities.SortMergeGeneric<int>( displayTextIndex, new IndexComparer( _cachedInfo, true ) );
				}
				catch
				{
					// SSP 8/20/08 BR35749
					// 
					//return false;
					canUseBinarySearch = false;
				}
			}

			Dictionary<object, ItemHolder> table = new Dictionary<object, ItemHolder>( items.Length );
			for ( int i = 0; i < items.Length; i++ )
			{
				ItemHolder ii = items[i];
				table[ Utils.DictionaryKey( ii._item ) ] = ii;
			}
			
			_cachedInfo._dataValueIndex = dataValueIndex;
			_cachedInfo._displayTextIndex = displayTextIndex;
			_cachedInfo._itemsTable = table;
			// SSP 8/20/08 BR35749
			// 
			//_cachedInfo.canUseBinarySearch = true;
			_cachedInfo._canUseBinarySearch = canUseBinarySearch;

			// SSP 1/16/12 TFS87805
			// 
			_cachedInfo.HookUnhookDataItems( true );

			return true;
		}

		private class CachedInfo
		{
			internal ComboBoxItemsProvider _itemsProvider;
			internal ItemHolder[] _items;
			internal Dictionary<object, ItemHolder> _itemsTable;
			internal int[] _displayTextIndex;
			internal int[] _dataValueIndex;
			internal bool _compareDataValuesAsText;
			internal bool _canUseBinarySearch;
			
			
			internal Type _compareType;

			// SSP 1/16/12 TFS87805
			// 
			internal readonly PropertyChangedEventHandler _itemPropChangeHandler;

			internal CachedInfo( ComboBoxItemsProvider itemsProvider )
			{
				_itemsProvider = itemsProvider;

				// SSP 1/16/12 TFS87805
				// 
				_itemPropChangeHandler = new PropertyChangedEventHandler( this.OnDataItemPropertyChanged );
			}

			// SSP 1/16/12 TFS87805
			// 
			private void OnDataItemPropertyChanged( object sender, PropertyChangedEventArgs args )
			{
				if ( this == _itemsProvider._cachedInfo )
					_itemsProvider.DirtyCachedItems( );
			}

			// SSP 1/16/12 TFS87805
			// 
			internal void Dispose( )
			{
				this.HookUnhookDataItems( false );
			}

			// SSP 1/16/12 TFS87805
			// 
			internal void HookUnhookDataItems( bool hook )
			{
				if ( null != _items )
				{
					foreach ( ItemHolder ii in _items )
						ii.HookUnhookDataItem( this, hook );
				}
			}
		}

		private class IndexComparer : IComparer, IComparer<int>
		{
			#region Vars

			private CachedInfo _cachedInfo;
			private bool _compareByDisplayText;
			private ItemHolder[] _items;

			#endregion // Vars

			#region Constructor

			/// <summary>
			/// Constructor
			/// </summary>
			/// <param name="cachedInfo"></param>
			/// <param name="compareByDisplayText"></param>
			public IndexComparer( CachedInfo cachedInfo, bool compareByDisplayText )
			{
				_cachedInfo = cachedInfo;
				_items = cachedInfo._items;
				_compareByDisplayText = compareByDisplayText;
			}

			#endregion // Constructor

			#region Compare

			public int Compare( object xObj, object yObj )
			{
				int x = (int)xObj;
				int y = (int)yObj;

				return this.Compare( x, y );
			}

			public int Compare( int x, int y )
			{
				ItemHolder xx = _items[x];
				ItemHolder yy = _items[y];

				if ( _compareByDisplayText )
					return CompareDisplayText( _cachedInfo._itemsProvider, xx._displayText, yy._displayText );
				else
					return CompareHelper( _cachedInfo, xx._dataValue, yy._dataValue, _cachedInfo._compareDataValuesAsText );
			}

			#endregion // Compare

			#region CompareDataValue

			internal static int CompareHelper( CachedInfo cacheInfo, object x, object y, bool compareAsText )
			{
				
				
				
				
				IComparer dataValueComparer = cacheInfo._itemsProvider.ValueComparer;
				if ( null != dataValueComparer )
					return dataValueComparer.Compare( x, y );				

				if ( null == x || DBNull.Value == x )
				{
					if ( null == y || DBNull.Value == y )
						return 0;
					else
						return -1;
				}

				if ( null == y || DBNull.Value == y )
					return 1;

				if ( compareAsText )
				{
					string xStr = x.ToString( );
					string yStr = y.ToString( );

					return string.CompareOrdinal( xStr, yStr );
				}

				return Comparer<object>.Default.Compare( x, y );
			}

			
			
			
			
#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)


			#endregion // CompareDataValue

			#region CompareDisplayText

			public static int CompareDisplayText( ComboBoxItemsProvider itemsProvider, string x, string y )
			{
				IComparer displayTextComparer = itemsProvider.DisplayTextComparer;
				if ( null != displayTextComparer )
					return displayTextComparer.Compare( x, y );

				return string.Compare( x, y, true, System.Globalization.CultureInfo.CurrentCulture );
			}

			#endregion // CompareDisplayText

			#region ConvertDataValue

			/// <summary>
			/// Conversion routine for use by index comparer.
			/// </summary>
			/// <param name="cacheInfo"></param>
			/// <param name="value"></param>
			/// <param name="targetType"></param>
			/// <returns></returns>
			public static object ConvertDataValue( CachedInfo cacheInfo, object value, Type targetType )
			{
				// Make sure string conversion logic is in sync with other places.
				// 
				if ( targetType == typeof( string ) && null != value )
					return value.ToString( );

				return Utilities.ConvertDataValue( value, targetType, null, null );
			}

			#endregion // ConvertDataValue

			#region DataValueEquals

			// SSP 8/20/08 BR35749
			// 
			public static bool DataValueEquals( CachedInfo cacheInfo, object x, object y )
			{
				IComparer dataValueComparer = cacheInfo._itemsProvider.ValueComparer;
				if ( null != dataValueComparer )
					return 0 == dataValueComparer.Compare( x, y );

				
				
				
				
				

				// If data types are the same then use object.Equals.
				// 
				
				
				
				
				
				if ( null != x && null != y && x.GetType( ) == y.GetType( ) )
					return object.Equals( x, y );

				// Otherwise compare as text.
				// 
				return 0 == CompareHelper( cacheInfo, x, y, true );
				
			}

			#endregion // DataValueEquals

			#region DisplayTextEquals

			public static bool DisplayTextEquals( ComboBoxItemsProvider itemsProvider, string x, string y )
			{
				return 0 == CompareDisplayText( itemsProvider, x, y );
			}

			/// <summary>
			/// If partial is true then checks if the overallText starts with searchText.
			/// </summary>
			/// <param name="itemsProvider"></param>
			/// <param name="searchText"></param>
			/// <param name="overallText"></param>
			/// <param name="partial"></param>
			/// <returns></returns>
			public static bool DisplayTextEquals( ComboBoxItemsProvider itemsProvider, string searchText, string overallText, bool partial )
			{
				if ( partial )
				{
					if ( searchText.Length <= overallText.Length )
						overallText = overallText.Substring( 0, searchText.Length );
					else // If search text is longer than the overallText, then return false.
						return false;

					
					
					
					
					
				}

				return IndexComparer.DisplayTextEquals( itemsProvider, searchText, overallText );
			}

			#endregion // DisplayTextEquals
		}

		#endregion // VerifyCachedItems

		#endregion // Private/Internal Methods

		#endregion // Methods
	}

	#endregion // ComboBoxItemsProvider Class

	#region IItemMemberProvider Interface

	// SSP 8/7/09 TFS19798 - Optimizations
	// 
	/// <summary>
	/// Used by the ComboBoxItemsProvider's <see cref="ComboBoxItemsProvider.ItemMemberProvider"/> property.
	/// Used for providing value and display text associated with each item in the ComboBoxItemsProvider's
	/// items source.
	/// </summary>
	public interface IItemMemberProvider
	{
		/// <summary>
		/// Gets the value for the specified data item.
		/// </summary>
		/// <param name="dataItem">Gets the value for this data item.</param>
		/// <returns>Value associated with the specified data item.</returns>
		object GetValue( object dataItem );

		/// <summary>
		/// Gets the display text for the specified data item.
		/// </summary>
		/// <param name="dataItem">Gets the display text for this data item.</param>
		/// <returns>Display text associated with the specified data item.</returns>
		string GetDisplayText( object dataItem );
	}

	#endregion // IItemMemberProvider Interface

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