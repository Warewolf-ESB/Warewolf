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
using Infragistics.Controls.Layouts;


using Infragistics.Windows.Licensing;


using System.Windows.Automation.Peers;
using Infragistics.AutomationPeers;
using System.Windows.Threading;
using System.Threading;
using Infragistics.Collections;
using Infragistics.Controls;
using Infragistics.Controls.Primitives;
using Infragistics.Controls.Layouts.Primitives;
using System.Windows.Media.Animation;
using System.Windows.Data;

namespace Infragistics.Controls.Layouts
{

	/// <summary>
	/// A control that arranges and displays its child elements as tiles, with native support for scrolling and virtualizing those items.
	/// </summary>

	
	

	[ContentProperty("Items")]
	[StyleTypedProperty(Property="ItemContainerStyle", StyleTargetType=typeof(XamTile))]
	[TemplatePart(Name = TilePanel, Type = typeof(TileAreaPanel))]
	public class XamTileManager : Control, ISupportPropertyChangeNotifications, ILogicalTreeNode, IResizeHostMulti, IProvideCustomObjectPersistence

		// JJD 02/22/12 - TFS100150 - Added touch support for scrolling
		, ISupportScrollHelper

	{
		#region Member Variables

		const string TilePanel = "TilePanel";

		private UltraLicense _license;

		private ObservableItemCollection _items;
		private ObservableCollectionExtended<object> _maximizedItems;
		private ReadOnlyObservableCollection<object> _maximizedItemsReadOnly;
		private WeakList<object>					_maximizedItemsSnapshot; 

		private TileAreaPanel _panel;
		private TileLayoutManager _layoutManager;
		internal ResizeController _resizeController;

		//private XamTileManager.TileMgr _manager = new XamTileManager.TileMgr();
		//private TileAreaSplitter _splitter;
		private Dictionary<string, object> _itemSerializationMap;


		// JJD 02/22/12 - TFS100150 - Added touch support for scrolling
		internal TouchScrollHelper _scrollHelper;
		private double? _cachedHorizValue;
		private double? _cachedVertValue;





		private double _minimizedAreaExplicitExtentX;
		private double _minimizedAreaExplicitExtentY;
		private double _interTileAreaSpacingResolved;
		private double _interTileSpacingX = (double)DependencyPropertyUtilities.GetDefaultValue(typeof(XamTileManager), InterTileSpacingXProperty);
		private double _interTileSpacingXMaximizedResolved;
		private double _interTileSpacingXMinimizedResolved;
		private double _interTileSpacingY = (double)DependencyPropertyUtilities.GetDefaultValue(typeof(XamTileManager), InterTileSpacingYProperty);
		private double _interTileSpacingYMaximizedResolved;
		private double _interTileSpacingYMinimizedResolved;
		private XamTile _currentMinimizedExpandedTile;

		private string _headerPath;
		private string _serializationIdPath;
		private bool _isLoadingLayout;
		private bool _isAnimationInProgress;
		private bool _isInitialized;
		internal XamTile _settingStateOfTile;
		private int _layoutVersion;
		
		private TileAreaSplitter		_splitter;
		private double					_splitterMinExtent;

		private List<object>			_extraLogicalChildren = new List<object>();

		
		private NormalModeSettings		_normalModeSettings;
		private MaximizedModeSettings	_maximizedModeSettings;

		private NormalModeSettings		_normalModeSettingsSafe = new NormalModeSettings();
		private MaximizedModeSettings	_maximizedModeSettingsSafe = new MaximizedModeSettings();
		
		private PropertyChangeListenerList _propChangeListeners = new PropertyChangeListenerList( );
		
		// JJD 02/24/12 - TFS101202 - added
		private bool _panelInvalidationPending;

		// JJD 11/1/11 - TFS88171
		// Added object to cache states during a swap operation
		internal SwapInfo _swapinfo;

		[ThreadStatic()]
		private static Dictionary<DependencyProperty, Binding> _CachedBindings;

		#endregion //Member Variables

		#region Constructor

		static XamTileManager()
		{

			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(XamTileManager), new FrameworkPropertyMetadata(typeof(XamTileManager)));


		}

		/// <summary>
		/// Initializes a new <see cref="XamTileManager"/>
		/// </summary>
		public XamTileManager()
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
				this._license = LicenseManager.Validate(typeof(XamTileManager), this) as UltraLicense;
			}
			catch (System.IO.FileNotFoundException) { }
			

			// JJD 02/22/12 - TFS100150 - Added touch support for scrolling
			this.SetCurrentValue(IsManipulationEnabledProperty, KnownBoxes.TrueBox);




			_items = new ObservableItemCollection(this);
			_items.CollectionChanged += new NotifyCollectionChangedEventHandler(OnItemsChanged);

			_layoutManager = new TileLayoutManager(this);

			_resizeController = new ResizeController(this);

			this._maximizedItems = new ObservableCollectionExtended<object>();
			this._maximizedItemsReadOnly = new ReadOnlyObservableCollection<object>(this._maximizedItems);
			
			this.InitializeSpacingOnResizeController();


			// JJD 02/22/12 - TFS100150 - Added touch support for scrolling
			// Initialize new TouchScrollHelper instance
			_scrollHelper = new TouchScrollHelper(this, this);
			_scrollHelper.AnimateToIntegralItemOnEndPan = true;


		}

		#endregion //Constructor
		
		#region Base class overrides

		#region LogicalChildren


		/// <summary>
		/// Gets an enumerator that can iterate the logical children of this element.
		/// </summary>
		protected override IEnumerator LogicalChildren
		{
			get
			{
				// JJD 02/28/12 - The items that were added are already in the _extraLogicalChildren collection so we
				// shouldn't return them twice
				//if (_items.ItemsSource == null )
				//    return new MultiSourceEnumerator(this._extraLogicalChildren.GetEnumerator(), _items.GetEnumerator() );
				//else
				return this._extraLogicalChildren.GetEnumerator();
			}
		}


		#endregion //LogicalChildren	

		#region OnCreateAutomationPeer

		/// <summary>
		/// Returns an automation peer that exposes the <see cref="XamTileManager"/> to UI Automation.
		/// </summary>
		/// <returns>A <see cref="XamTileManagerAutomationPeer"/></returns>
		protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer()
		{
			return new XamTileManagerAutomationPeer(this);
		}

		#endregion //OnCreateAutomationPeer	
    
		#region OnApplyTemplate

		/// <summary>
		/// Invoked when the template has been applied to the element.
		/// </summary>
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();







			TileAreaPanel panel = this.GetTemplateChild(TilePanel) as TileAreaPanel;

			if (panel != this._panel)
			{
				
				// Keep track of whether we need to initialize a tile area splitter
				bool initalizeSplitter = false;

				if (_panel != null)
				{
					if (_splitter != null)
					{
						_panel.RemoveExtraChild(_splitter);

						
						// Clear the related cached members and set the flag to initialize a new splitter
						_splitterMinExtent = 0;
						_splitter = null;
						initalizeSplitter = true;
					}

					_panel.Detach();

				}

				_panel = panel;

				if (_panel != null)
				{
					_panel.Attach(this);

					
					// If the initalizeSplitter flag was set above then call InitializeSplitter
					
					
					if (initalizeSplitter)
						this.InitializeSplitter();

				}

				_layoutManager.InitializePanel(_panel);
			}
		}

		#endregion //OnApplyTemplate	

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

		}
		#endregion //OnInitialized

		#region OnLostMouseCapture

		/// <summary>
		/// Called when mouse capture is lost
		/// </summary>
		/// <param name="e"></param>
		protected override void OnLostMouseCapture(MouseEventArgs e)
		{
			base.OnLostMouseCapture(e);

			if (_panel != null)
				_panel.ProcessLostMouseCapture(e);
		}

		#endregion //OnLostMouseCapture

		#region OnMouseLeftButtonUp

		/// <summary>
		/// Called when the left button is released.
		/// </summary>
		/// <param name="e">arguments</param>
		protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
		{
			if (this._panel == null || !_panel.ProcessMouseLeftButtonUp(e))
				base.OnMouseLeftButtonUp(e);
		}

		#endregion //OnMouseLeftButtonUp

		#region OnMouseMove

		/// <summary>
		/// Called when the mouse is moved.
		/// </summary>
		/// <param name="e">arguments</param>
		protected override void OnMouseMove(MouseEventArgs e)
		{
			if (this._panel == null || !_panel.ProcessMouseMove(e))
				base.OnMouseMove(e);
		}

		#endregion //OnMouseMove	

		#endregion //Base class overrides

		#region Events

		#region AnimationEnded

		/// <summary>
		/// Occurs when tile animations end
		/// </summary>
		/// <seealso cref="AnimationEnded"/>
		protected virtual void OnAnimationEnded()
		{
			if ( this.AnimationEnded != null )
				this.AnimationEnded(this, new EventArgs());
		}

		internal void RaiseAnimationEnded()
		{
			this.OnAnimationEnded();
		}

		/// <summary>
		/// Occurs when tile animations end
		/// </summary>
		/// <seealso cref="OnAnimationEnded"/>
		public event EventHandler AnimationEnded;

		#endregion //AnimationEnded

		#region AnimationStarted


		/// <summary>
		/// Occurs when tile animations start
		/// </summary>
		/// <seealso cref="AnimationStarted"/>
		protected virtual void OnAnimationStarted()
		{
			if (this.AnimationStarted != null)
				this.AnimationStarted(this, new EventArgs());
		}

		internal void RaiseAnimationStarted()
		{
			this.OnAnimationStarted();
		}

		/// <summary>
		/// Occurs when tile animations start
		/// </summary>
		/// <seealso cref="OnAnimationStarted"/>
		public event EventHandler AnimationStarted;

		#endregion //AnimationStarted

		#region LoadingItemMapping

		/// <summary>
		/// Occurs during a call to <b>Infrageistics.Persistence.PersistenceManager.Load()</b>.
		/// </summary>
		/// <seealso cref="LoadingItemMapping"/>
		/// <seealso cref="LoadingItemMappingEventArgs"/>
		protected virtual void OnLoadingItemMapping(LoadingItemMappingEventArgs args)
		{
			if (this.LoadingItemMapping != null)
				this.LoadingItemMapping(this, args);
		}

		internal void RaiseLoadingItemMapping(LoadingItemMappingEventArgs args)
		{
			this.OnLoadingItemMapping(args);
		}

		/// <summary>
		/// Occurs during a call to <b>Infrageistics.Persistence.PersistenceManager.Load()</b>.
		/// </summary>
		/// <seealso cref="OnLoadingItemMapping"/>
		/// <seealso cref="LoadingItemMappingEventArgs"/>
		public event EventHandler<LoadingItemMappingEventArgs> LoadingItemMapping;

		#endregion //LoadingItemMapping

		#region SavingItemMapping

		/// <summary>
		/// Occurs during a call to <b>Infrageistics.Persistence.PersistenceManager.Save()</b>.
		/// </summary>
		/// <seealso cref="SavingItemMapping"/>
		/// <seealso cref="SavingItemMappingEventArgs"/>
		protected virtual void OnSavingItemMapping(SavingItemMappingEventArgs args)
		{
			if (this.SavingItemMapping != null)
				this.SavingItemMapping(this, args);
		}

		internal void RaiseSavingItemMapping(SavingItemMappingEventArgs args)
		{
			this.OnSavingItemMapping(args);
		}

		/// <summary>
		/// Occurs during a call to <b>Infrageistics.Persistence.PersistenceManager.Save()</b>.
		/// </summary>
		/// <seealso cref="OnSavingItemMapping"/>
		/// <seealso cref="SavingItemMappingEventArgs"/>
		public event EventHandler<SavingItemMappingEventArgs> SavingItemMapping;

		#endregion //SavingItemMapping

		#region TileClosed
		/// <summary>
		/// Occurs after a <see cref="XamTile"/> has been closed.
		/// </summary>
		/// <seealso cref="TileClosed"/>
		/// <seealso cref="TileClosedEventArgs"/>
		protected virtual void OnTileClosed(TileClosedEventArgs args)
		{
			if (this.TileClosed != null)
				this.TileClosed(this, args);
		}

		/// <summary>
		/// Occurs after a <see cref="XamTile"/> has been closed.
		/// </summary>
		/// <seealso cref="OnTileClosed"/>
		/// <seealso cref="TileClosedEventArgs"/>
		public event EventHandler<TileClosedEventArgs> TileClosed;

		#endregion //TileClosed

		#region TileClosing

		/// <summary>
		/// Occurs when a <see cref="XamTile"/> is about to close.
		/// </summary>
		/// <seealso cref="TileClosing"/>
		/// <seealso cref="TileClosingEventArgs"/>
		protected virtual void OnTileClosing(TileClosingEventArgs args)
		{
			if (this.TileClosing != null)
				this.TileClosing(this, args);
		}

		/// <summary>
		/// Occurs when a <see cref="XamTile"/> is about to close.
		/// </summary>
		/// <seealso cref="OnTileClosing"/>
		/// <seealso cref="TileClosingEventArgs"/>
		public event EventHandler<TileClosingEventArgs> TileClosing;

		#endregion //TileClosing

		#region TileDragging

		/// <summary>
		/// Occurs when a <see cref="XamTile"/> is about to be dragged.
		/// </summary>
		/// <seealso cref="TileDragging"/>
		/// <seealso cref="TileDraggingEventArgs"/>
		protected virtual void OnTileDragging(TileDraggingEventArgs args)
		{
			if (this.TileDragging != null)
				this.TileDragging(this, args);
		}

		internal void RaiseTileDragging(TileDraggingEventArgs args)
		{
			this.OnTileDragging(args);
		}

		/// <summary>
		/// Occurs when a <see cref="XamTile"/> is about to be dragged.
		/// </summary>
		/// <seealso cref="OnTileDragging"/>
		/// <seealso cref="TileDraggingEventArgs"/>
		public event EventHandler<TileDraggingEventArgs> TileDragging;

		#endregion //TileDragging

		#region TileStateChanged

		/// <summary>
		/// Occurs after the state of a <see cref="XamTile"/> has changed.
		/// </summary>
		/// <seealso cref="TileStateChanged"/>
		/// <seealso cref="TileStateChangedEventArgs"/>
		internal protected virtual void OnTileStateChanged(TileStateChangedEventArgs args)
		{
			if (this.TileStateChanged != null)
				this.TileStateChanged(this, args);
		}

		/// <summary>
		/// Occurs after the state of a <see cref="XamTile"/> has changed.
		/// </summary>
		/// <seealso cref="OnTileStateChanged"/>
		/// <seealso cref="TileStateChangedEventArgs"/>
		public event EventHandler<TileStateChangedEventArgs> TileStateChanged;

		#endregion //TileStateChanged

		#region TileStateChanging

		/// <summary>
		/// Occurs when the state of a <see cref="XamTile"/> is about to change.
		/// </summary>
		/// <seealso cref="TileStateChanging"/>
		/// <seealso cref="TileStateChangingEventArgs"/>
		protected virtual void OnTileStateChanging(TileStateChangingEventArgs args)
		{
			if (this.TileStateChanging != null)
				this.TileStateChanging(this, args);
		}

		/// <summary>
		/// Occurs when the state of a <see cref="XamTile"/> is about to change.
		/// </summary>
		/// <seealso cref="OnTileStateChanging"/>
		/// <seealso cref="TileStateChangingEventArgs"/>
		public event EventHandler<TileStateChangingEventArgs> TileStateChanging;

		#endregion //TileStateChanging

		#region TileSwapping

		/// <summary>
		/// Occurs when a <see cref="XamTile"/> is dragged over another tile that is a potential swap target.
		/// </summary>
		/// <seealso cref="TileSwapping"/>
		/// <seealso cref="TileSwappingEventArgs"/>
		protected virtual void OnTileSwapping(TileSwappingEventArgs args)
		{
			if (this.TileSwapping != null)
				this.TileSwapping(this, args);
		}

		internal void RaiseTileSwapping(TileSwappingEventArgs args)
		{
			this.OnTileSwapping(args);
		}

		/// <summary>
		/// Occurs when a <see cref="XamTile"/> is dragged over another tile that is a potential swap target.
		/// </summary>
		/// <seealso cref="OnTileSwapping"/>
		/// <seealso cref="TileSwappingEventArgs"/>
		public event EventHandler<TileSwappingEventArgs> TileSwapping;

		#endregion //TileSwapping

		#region TileSwapped

		/// <summary>
		/// Occurs when a <see cref="XamTile"/> is dropped over another tile and swaps places with it.
		/// </summary>
		/// <seealso cref="TileSwapped"/>
		/// <seealso cref="TileSwappedEventArgs"/>
		protected virtual void OnTileSwapped(TileSwappedEventArgs args)
		{
			if (this.TileSwapped != null)
				this.TileSwapped(this, args);
		}

		/// <summary>
		/// Occurs when a <see cref="XamTile"/> is dropped over another tile and swaps places with it.
		/// </summary>
		/// <seealso cref="OnTileSwapped"/>
		/// <seealso cref="TileSwappedEventArgs"/>
		public event EventHandler<TileSwappedEventArgs> TileSwapped;

		#endregion //TileSwapped

		#endregion //Events	

		#region Properties

		#region Public Attached Properties

		#region Column

		/// <summary>
		/// Identifies the Column attached dependency property
		/// </summary>
		/// <seealso cref="GetColumn"/>
		/// <seealso cref="SetColumn"/>
		public static readonly DependencyProperty ColumnProperty = DependencyPropertyUtilities.RegisterAttached("Column",
			typeof(int), typeof(XamTileManager),
			DependencyPropertyUtilities.CreateMetadata((int)0, new PropertyChangedCallback(OnRowColumnChanged))
			);

		private static void OnRowColumnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ValidateColumnRow(e.NewValue);
			OnConstraintPropertyChanged(d, e);
		}

		/// <summary>
		/// Gets the value of the Column attached property of the specified element, -1 indicates that the tile will be positioned relative to the previous tile in the panel. The default value is 0.
		/// </summary>
		/// <param name="elem">This element's Column value will be returned.</param>
		/// <returns>The value of the Column attached property. The default value is 0.</returns>
		/// <remarks>
		/// <para class="note"><b>Note:</b> this property is ignored unless <see cref="TileLayoutOrder"/> is 'UseExplicitRowColumnOnTile'.</para>
		/// </remarks>
		public static int GetColumn(DependencyObject elem)
		{
			return (int)elem.GetValue(ColumnProperty);
		}

		/// <summary>
		/// Sets the value of the Column attached property of the specified element, -1 indicates that the tile will be positioned relative to the previous tile in the panel. The default value is 0.
		/// </summary>
		/// <param name="elem">This element's Column value will be set.</param>
		/// <param name="value">Value to set. This can be -1 which will position the element relative to previous element in the panel.</param>
		/// <remarks>
		/// <para class="note"><b>Note:</b> this property is ignored unless <see cref="TileLayoutOrder"/> is 'UseExplicitRowColumnOnTile'.</para>
		/// </remarks>
		public static void SetColumn(DependencyObject elem, int value)
		{
			elem.SetValue(ColumnProperty, value);
		}

		#endregion // Column

		#region ColumnSpan

		/// <summary>
		/// Identifies the ColumnSpan attached dependency property
		/// </summary>
		/// <seealso cref="GetColumnSpan"/>
		/// <seealso cref="SetColumnSpan"/>
		public static readonly DependencyProperty ColumnSpanProperty = DependencyPropertyUtilities.RegisterAttached("ColumnSpan",
			typeof(int), typeof(XamTileManager),
			DependencyPropertyUtilities.CreateMetadata((int)1, new PropertyChangedCallback(OnRowColumnSpanChanged))
			);

		private static void OnRowColumnSpanChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ValidateColumnRowSpan(e.NewValue);
			OnConstraintPropertyChanged(d, e);
		}

		/// <summary>
		/// Gets the value of the ColumnSpan attached property of the specified element, 0 indicates
		/// that the element will occupy the remainder of the space in its logical column. The default is 1. 
		/// </summary>
		/// <param name="elem">This element's ColumnSpan value will be returned.</param>
		/// <returns>The value of the ColumnSpan attached property. The default value is 1.</returns>
		/// <remarks>
		/// <para class="note"><b>Note:</b> this property is ignored unless <see cref="TileLayoutOrder"/> is 'UseExplicitRowColumnOnTile'.</para>
		/// </remarks>
		public static int GetColumnSpan(DependencyObject elem)
		{
			return (int)elem.GetValue(ColumnSpanProperty);
		}

		/// <summary>
		/// Sets the value of the ColumnSpan attached property of the specified element, 0 indicates
		/// that the element will occupy the remainder of the space in its logical column. The default is 1. 
		/// </summary>
		/// <param name="elem">This element's ColumnSpan value will be set.</param>
		/// <param name="value">Value to set. This can be 0 to indicate that the element should occupy the remainder of the logical column.</param>
		/// <remarks>
		/// <para class="note"><b>Note:</b> this property is ignored unless <see cref="TileLayoutOrder"/> is 'UseExplicitRowColumnOnTile'.</para>
		/// </remarks>
		public static void SetColumnSpan(DependencyObject elem, int value)
		{
			elem.SetValue(ColumnSpanProperty, value);
		}

		#endregion // ColumnSpan

		#region ColumnWeight

		/// <summary>
		/// Identifies the ColumnWeight attached dependency property
		/// </summary>
		/// <seealso cref="GetColumnWeight"/>
		/// <seealso cref="SetColumnWeight"/>
		public static readonly DependencyProperty ColumnWeightProperty = DependencyPropertyUtilities.RegisterAttached("ColumnWeight",
			typeof(float), typeof(XamTileManager),
			DependencyPropertyUtilities.CreateMetadata(0f, new PropertyChangedCallback(OnRowColumnWeightChanged))
			);

		private static void OnRowColumnWeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ValidateColumnRowWeight(e.NewValue);
			OnConstraintPropertyChanged(d, e);

		}

		/// <summary>
		/// Gets the value of the ColumnWeight attached property of the specified element. ColumnWeight specifies
		/// how any extra width will be distributed among elements.
		/// </summary>
		/// <returns>The value of the ColumnWeight attached property. The default value is 0.</returns>
		/// <seealso cref="ColumnWeightProperty"/>
		/// <seealso cref="SetColumnWeight"/>



		public static float GetColumnWeight(DependencyObject d)
		{
			return (float)d.GetValue(XamTileManager.ColumnWeightProperty);
		}

		/// <summary>
		/// Sets the value of the ColumnWeight attached property of the specified element. ColumnWeight specifies
		/// how any extra width will be distributed among elements.
		/// </summary>
		/// <returns>The value of the ColumnWeight attached property.</returns>
		/// <seealso cref="ColumnWeightProperty"/>
		/// <seealso cref="GetColumnWeight"/>
		public static void SetColumnWeight(DependencyObject d, float value)
		{
			d.SetValue(XamTileManager.ColumnWeightProperty, value);
		}

		#endregion //ColumnWeight

		#region Constraints

		/// <summary>
		/// Identifies the Constraints attached dependency property
		/// </summary>
		/// <seealso cref="GetConstraints"/>
		/// <seealso cref="SetConstraints"/>
		public static readonly DependencyProperty ConstraintsProperty = DependencyPropertyUtilities.RegisterAttached("Constraints",
			typeof(TileConstraints), typeof(XamTileManager),
			DependencyPropertyUtilities.CreateMetadata(null, new PropertyChangedCallback(OnConstraintPropertyChanged))
			);

		/// <summary>
		/// Gets the value of the 'Constraints' attached property which contains size constraints for tiles when their <see cref="XamTile.State"/> is 'Normal'.
		/// </summary>
		/// <seealso cref="ConstraintsProperty"/>
		/// <seealso cref="SetConstraints"/>
		public static TileConstraints GetConstraints(DependencyObject d)
		{
			return (TileConstraints)d.GetValue(XamTileManager.ConstraintsProperty);
		}

		/// <summary>
		/// Sets the value of the 'Constraints' attached property which contains size constraints for tiles when their <see cref="XamTile.State"/> is 'Normal'.
		/// </summary>
		/// <seealso cref="ConstraintsProperty"/>
		/// <seealso cref="GetConstraints"/>
		public static void SetConstraints(DependencyObject d, TileConstraints value)
		{
			d.SetValue(XamTileManager.ConstraintsProperty, value);
		}

		#endregion //Constraints

		#region ConstraintsMaximized

		/// <summary>
		/// Identifies the ConstraintsMaximized attached dependency property
		/// </summary>
		/// <seealso cref="GetConstraintsMaximized"/>
		/// <seealso cref="SetConstraintsMaximized"/>
		public static readonly DependencyProperty ConstraintsMaximizedProperty = DependencyPropertyUtilities.RegisterAttached("ConstraintsMaximized",
			typeof(TileConstraints), typeof(XamTileManager),
			DependencyPropertyUtilities.CreateMetadata(null, new PropertyChangedCallback(OnConstraintPropertyChanged))
			);

		/// <summary>
		/// Gets the value of the 'ConstraintsMaximized' attached property which contains size constraints for tiles when their <see cref="XamTile.State"/> is 'Maximized'.
		/// </summary>
		/// <seealso cref="ConstraintsMaximizedProperty"/>
		/// <seealso cref="SetConstraintsMaximized"/>
		public static TileConstraints GetConstraintsMaximized(DependencyObject d)
		{
			return (TileConstraints)d.GetValue(XamTileManager.ConstraintsMaximizedProperty);
		}

		/// <summary>
		/// Sets the value of the 'ConstraintsMaximized' attached property which contains size constraints for tiles when their <see cref="XamTile.State"/> is 'Maximized'.
		/// </summary>
		/// <seealso cref="ConstraintsMaximizedProperty"/>
		/// <seealso cref="GetConstraintsMaximized"/>
		public static void SetConstraintsMaximized(DependencyObject d, TileConstraints value)
		{
			d.SetValue(XamTileManager.ConstraintsMaximizedProperty, value);
		}

		#endregion //ConstraintsMaximized

		#region ConstraintsMinimized

		/// <summary>
		/// Identifies the ConstraintsMinimized attached dependency property
		/// </summary>
		/// <seealso cref="GetConstraintsMinimized"/>
		/// <seealso cref="SetConstraintsMinimized"/>
		public static readonly DependencyProperty ConstraintsMinimizedProperty = DependencyPropertyUtilities.RegisterAttached("ConstraintsMinimized",
			typeof(TileConstraints), typeof(XamTileManager),
			DependencyPropertyUtilities.CreateMetadata(null, new PropertyChangedCallback(OnConstraintPropertyChanged))
			);

		/// <summary>
		/// Gets the value of the 'ConstraintsMinimized' attached property which contains size constraints for tiles when their <see cref="XamTile.State"/> is 'Minimized'.
		/// </summary>
		/// <seealso cref="ConstraintsMinimizedProperty"/>
		/// <seealso cref="SetConstraintsMinimized"/>
		public static TileConstraints GetConstraintsMinimized(DependencyObject d)
		{
			return (TileConstraints)d.GetValue(XamTileManager.ConstraintsMinimizedProperty);
		}

		/// <summary>
		/// Sets the value of the 'ConstraintsMinimized' attached property which contains size constraints for tiles when their <see cref="XamTile.State"/> is 'Minimized'.
		/// </summary>
		/// <seealso cref="ConstraintsMinimizedProperty"/>
		/// <seealso cref="GetConstraintsMinimized"/>
		public static void SetConstraintsMinimized(DependencyObject d, TileConstraints value)
		{
			d.SetValue(XamTileManager.ConstraintsMinimizedProperty, value);
		}

		#endregion //ConstraintsMinimized

		#region ConstraintsMinimizedExpanded

		/// <summary>
		/// Identifies the ConstraintsMinimizedExpanded attached dependency property
		/// </summary>
		/// <seealso cref="GetConstraintsMinimizedExpanded"/>
		/// <seealso cref="SetConstraintsMinimizedExpanded"/>
		public static readonly DependencyProperty ConstraintsMinimizedExpandedProperty = DependencyPropertyUtilities.RegisterAttached("ConstraintsMinimizedExpanded",
			typeof(TileConstraints), typeof(XamTileManager),
			DependencyPropertyUtilities.CreateMetadata(null, new PropertyChangedCallback(OnConstraintPropertyChanged))
			);

		/// <summary>
		/// Gets the value of the 'ConstraintsMinimizedExpanded' attached property which contains size constraints for tiles when their <see cref="XamTile.State"/> is 'MinimizedExpanded'.
		/// </summary>
		/// <seealso cref="ConstraintsMinimizedExpandedProperty"/>
		/// <seealso cref="SetConstraintsMinimizedExpanded"/>
		public static TileConstraints GetConstraintsMinimizedExpanded(DependencyObject d)
		{
			return (TileConstraints)d.GetValue(XamTileManager.ConstraintsMinimizedExpandedProperty);
		}

		/// <summary>
		/// Sets the value of the 'ConstraintsMinimizedExpanded' attached property which contains size constraints for tiles when their <see cref="XamTile.State"/> is 'MinimizedExpanded'.
		/// </summary>
		/// <seealso cref="ConstraintsMinimizedExpandedProperty"/>
		/// <seealso cref="GetConstraintsMinimizedExpanded"/>
		public static void SetConstraintsMinimizedExpanded(DependencyObject d, TileConstraints value)
		{
			d.SetValue(XamTileManager.ConstraintsMinimizedExpandedProperty, value);
		}

		#endregion //ConstraintsMinimizedExpanded

		#region IsDragging

		internal static readonly DependencyPropertyKey IsDraggingPropertyKey = DependencyPropertyUtilities.RegisterAttachedReadOnly("IsDragging",
			typeof(bool), typeof(XamTileManager), KnownBoxes.FalseBox, null);


		/// <summary>
		/// Identifies the IsDragging" attached read-only dependency property
		/// </summary>
		/// <seealso cref="GetIsDragging"/>
		public static readonly DependencyProperty IsDraggingProperty =
			IsDraggingPropertyKey.DependencyProperty;


		/// <summary>
		/// Returns whether this tile is currently being dragged (read-only)
		/// </summary>
		/// <seealso cref="IsDraggingProperty"/>
		public static bool GetIsDragging(DependencyObject d)
		{
			return (bool)d.GetValue(XamTileManager.IsDraggingProperty);
		}

		#endregion //IsDragging

		#region IsSwapTarget

		internal static readonly DependencyPropertyKey IsSwapTargetPropertyKey = DependencyPropertyUtilities.RegisterAttachedReadOnly("IsSwapTarget",
			typeof(bool), typeof(XamTileManager), KnownBoxes.FalseBox, null);

		/// <summary>
		/// Identifies the read-only IsSwapTarget attached dependency property
		/// </summary>
		/// <seealso cref="GetIsSwapTarget"/>
		public static readonly DependencyProperty IsSwapTargetProperty = IsSwapTargetPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns whether another tile is being dragged over this tile and if released will swap positions with this tile (read-only)
		/// </summary>
		/// <seealso cref="IsSwapTargetProperty"/>
		public static bool GetIsSwapTarget(DependencyObject d)
		{
			return (bool)d.GetValue(XamTileManager.IsSwapTargetProperty);
		}

		#endregion //IsSwapTarget

		#region Row

		/// <summary>
		/// Identifies the Row attached dependency property
		/// </summary>
		/// <seealso cref="GetRow"/>
		/// <seealso cref="SetRow"/>
		public static readonly DependencyProperty RowProperty = DependencyPropertyUtilities.RegisterAttached("Row",
			typeof(int), typeof(XamTileManager),
			DependencyPropertyUtilities.CreateMetadata((int)0, new PropertyChangedCallback(OnRowColumnChanged))
			);

		/// <summary>
		/// Gets the value of the Row attached property of the specified element, -1 indicates that the tile will be positioned relative to the previous tile in the panel. The default value is 0. 
		/// </summary>
		/// <param name="elem">This element's Row value will be returned.</param>
		/// <returns>The value of the Row attached property. The default value is 0.</returns>
		/// <remarks>
		/// <para class="note"><b>Note:</b> this property is ignored unless <see cref="TileLayoutOrder"/> is 'UseExplicitRowColumnOnTile'.</para>
		/// </remarks>
		public static int GetRow(DependencyObject elem)
		{
			return (int)elem.GetValue(RowProperty);
		}

		/// <summary>
		/// Sets the value of the Row attached property of the specified element, -1 indicates that the tile will be positioned relative to the previous tile in the panel. The default value is 0.
		/// </summary>
		/// <param name="elem">This element's Row value will be set.</param>
		/// <param name="value">Value to set. This can be -1 which will position the element relative to previous element in the panel.</param>
		/// <remarks>
		/// <para class="note"><b>Note:</b> this property is ignored unless <see cref="TileLayoutOrder"/> is 'UseExplicitRowColumnOnTile'.</para>
		/// </remarks>
		public static void SetRow(DependencyObject elem, int value)
		{
			elem.SetValue(RowProperty, value);
		}

		#endregion // Row

		#region RowSpan

		/// <summary>
		/// Identifies the RowSpan attached dependency property
		/// </summary>
		/// <seealso cref="GetRowSpan"/>
		/// <seealso cref="SetRowSpan"/>
		public static readonly DependencyProperty RowSpanProperty = DependencyPropertyUtilities.RegisterAttached("RowSpan",
			typeof(int), typeof(XamTileManager),
			DependencyPropertyUtilities.CreateMetadata((int)1, new PropertyChangedCallback(OnRowColumnSpanChanged))
			);

		/// <summary>
		/// Gets the value of the RowSpan attached property of the specified element, 0 indicates
		/// that the element will occupy the remainder of the space in its logical column. The default is 1. 
		/// </summary>
		/// <param name="elem">This element's RowSpan value will be returned.</param>
		/// <returns>The value of the RowSpan attached property. The default value is 1.</returns>
		/// <remarks>
		/// <para class="note"><b>Note:</b> this property is ignored unless <see cref="TileLayoutOrder"/> is 'UseExplicitRowColumnOnTile'.</para>
		/// </remarks>
		public static int GetRowSpan(DependencyObject elem)
		{
			return (int)elem.GetValue(RowSpanProperty);
		}

		/// <summary>
		/// Sets the value of the RowSpan attached property of the specified element, 0 indicates
		/// that the element will occupy the remainder of the space in its logical column. The default is 1. 
		/// </summary>
		/// <param name="elem">This element's RowSpan value will be set.</param>
		/// <param name="value">Value to set. This can be 0 to indicate that the element should occupy the remainder of the logical row.</param>
		/// <remarks>
		/// <para class="note"><b>Note:</b> this property is ignored unless <see cref="TileLayoutOrder"/> is 'UseExplicitRowColumnOnTile'.</para>
		/// </remarks>
		public static void SetRowSpan(DependencyObject elem, int value)
		{
			elem.SetValue(RowSpanProperty, value);
		}

		#endregion // RowSpan

		#region RowWeight

		/// <summary>
		/// Identifies the RowWeight attached dependency property
		/// </summary>
		/// <seealso cref="GetRowWeight"/>
		/// <seealso cref="SetRowWeight"/>
		public static readonly DependencyProperty RowWeightProperty = DependencyPropertyUtilities.RegisterAttached("RowWeight",
			typeof(float), typeof(XamTileManager),
			DependencyPropertyUtilities.CreateMetadata(0f, new PropertyChangedCallback(OnRowColumnWeightChanged))
			);

		/// <summary>
		/// Gets the value of the RowWeight attached property of the specified element. RowWeight specifies
		/// how any extra height will be distributed among elements.
		/// </summary>
		/// <returns>The value of the RowWeight attached property. The default value is 0.</returns>
		/// <seealso cref="RowWeightProperty"/>
		/// <seealso cref="SetRowWeight"/>



		public static float GetRowWeight(DependencyObject d)
		{
			return (float)d.GetValue(XamTileManager.RowWeightProperty);
		}

		/// <summary>
		/// Sets the value of the RowWeight attached property of the specified element. RowWeight specifies
		/// how any extra height will be distributed among elements.
		/// </summary>
		/// <returns>The value of the RowWeight attached property.</returns>
		/// <seealso cref="RowWeightProperty"/>
		/// <seealso cref="GetRowWeight"/>
		public static void SetRowWeight(DependencyObject d, float value)
		{
			d.SetValue(XamTileManager.RowWeightProperty, value);
		}

		#endregion //RowWeight

		#region SerializationId

		/// <summary>
		/// Identifies the SerializationId attached dependency property
		/// </summary>
		/// <seealso cref="GetSerializationId"/>
		/// <seealso cref="SetSerializationId"/>
		public static readonly DependencyProperty SerializationIdProperty = DependencyPropertyUtilities.RegisterAttached("SerializationId",
			typeof(string), typeof(XamTileManager),
			DependencyPropertyUtilities.CreateMetadata(null)
			);

		/// <summary>
		/// Gets the value of the 'SerializationId' attached property
		/// </summary>
		/// <seealso cref="SerializationIdProperty"/>
		/// <seealso cref="SetSerializationId"/>
		public static string GetSerializationId(DependencyObject d)
		{
			return (string)d.GetValue(XamTileManager.SerializationIdProperty);
		}

		/// <summary>
		/// Sets the value of the 'SerializationId' attached property
		/// </summary>
		/// <seealso cref="SerializationIdProperty"/>
		/// <seealso cref="GetSerializationId"/>
		public static void SetSerializationId(DependencyObject d, string value)
		{
			d.SetValue(XamTileManager.SerializationIdProperty, value);
		}

		#endregion //SerializationId

		#endregion //Public Attached Properties

		#region Public Properties

		#region HeaderPath

		/// <summary>
		/// Identifies the <see cref="HeaderPath"/> dependency property
		/// </summary>
		public static readonly DependencyProperty HeaderPathProperty = DependencyPropertyUtilities.Register("HeaderPath",
			typeof(string), typeof(XamTileManager),
			DependencyPropertyUtilities.CreateMetadata(null, new PropertyChangedCallback(OnHeaderPathChanged))
			);

		private static void OnHeaderPathChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			XamTileManager control = target as XamTileManager;

			control._headerPath = (string)e.NewValue;
		}

		/// <summary>
		/// Gets/sets a path to a value on the source object that will be used to initialize the Header of each <see cref="XamTile"/>. 
		/// </summary>
		/// <remarks>
		/// <para class="note"><b>Note:</b> this setting is ignored for <see cref="XamTile"/>'s that are direct children of the XamTileManager. This setting only has meaning for <see cref="XamTile"/>s that get generated as item containers.</para>
		/// </remarks>
		/// <seealso cref="HeaderPathProperty"/>
		//[Description("Gets/sets a path to a value on the source object that will be used to initialize the Header of each Tile.")]
		//[Category("TilesControl Properties")]
		public string HeaderPath
		{
			get
			{
				return this._headerPath;
			}
			set
			{
				this.SetValue(XamTileManager.HeaderPathProperty, value);
			}
		}

		#endregion //HeaderPath

		#region InterTileAreaSpacingResolved

		private static readonly DependencyPropertyKey InterTileAreaSpacingResolvedPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("InterTileAreaSpacingResolved",
			typeof(double), typeof(XamTileManager),
			0d,
			new PropertyChangedCallback(OnInterTileAreaSpacingResolvedChanged)
			);

		/// <summary>
		/// Identifies the read-only <see cref="InterTileAreaSpacingResolved"/> dependency property
		/// </summary>
		public static readonly DependencyProperty InterTileAreaSpacingResolvedProperty = InterTileAreaSpacingResolvedPropertyKey.DependencyProperty;

		private static void OnInterTileAreaSpacingResolvedChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			XamTileManager control = target as XamTileManager;

			control._interTileAreaSpacingResolved = (double)e.NewValue;
		}

		/// <summary>
		/// Returns the resolved value that will be used for spacing between the maximized tile area and the minimized tile area when is maximized mode.
		/// </summary>
		/// <remarks>
		/// <para class="note"><b>Note:</b> if the <see cref="Infragistics.Controls.Layouts.MaximizedModeSettings.InterTileAreaSpacing"/> property is not set then the <see cref="InterTileSpacingXMaximizedResolved"/> or <see cref="InterTileSpacingYMaximizedResolved"/> setting will be used. If this also is not set then the <see cref="InterTileSpacingX"/> or <see cref="InterTileSpacingY"/> value will be used based on the <see cref="MaximizedTileLocation"/>.</para>
		/// </remarks>
		/// <seealso cref="MaximizedModeSettings"/>
		/// <seealso cref="Infragistics.Controls.Layouts.MaximizedModeSettings.InterTileAreaSpacing"/>
		/// <seealso cref="XamTileManager.IsInMaximizedMode"/>
		/// <seealso cref="XamTileManager.MaximizedItems"/>

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]

		[ReadOnly(true)]
		public double InterTileAreaSpacingResolved { get { return this._interTileAreaSpacingResolved; } }

		#endregion //InterTileAreaSpacingResolved

		#region InterTileSpacingX

		/// <summary>
		/// Identifies the <see cref="InterTileSpacingX"/> dependency property
		/// </summary>
		public static readonly DependencyProperty InterTileSpacingXProperty = DependencyPropertyUtilities.Register("InterTileSpacingX",
			typeof(double), typeof(XamTileManager),
			DependencyPropertyUtilities.CreateMetadata(2.0d, new PropertyChangedCallback(OnInterTileSpacingXChanged))
			);

		private static void OnInterTileSpacingXChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			ValidateInterTileSpacing(e.NewValue);

			XamTileManager instance = target as XamTileManager;

			instance._interTileSpacingX = (double)e.NewValue;

			instance.InitializeSpacingOnResizeController();
			
			instance.CalculateResolvedSpacing();

			if (instance._panel != null)
			{
				instance._panel.InvalidateMeasure();
				instance._panel.InvalidateArrange();
			}

		}

		/// <summary>
		/// Gets/sets the amount of spacing between tiles horizontally.
		/// </summary>
		/// <remarks>
		/// <para class="note"><b>Note:</b> the default value for this property is 2.</para>
		/// </remarks>
		/// <seealso cref="InterTileSpacingXProperty"/>
		//[Description("Gets/sets the amount of spacing between tiles horizontally.")]
		//[Category("TilesControl Properties")]
		public double InterTileSpacingX
		{
			get
			{
				return this._interTileSpacingX;
			}
			set
			{
				this.SetValue(XamTileManager.InterTileSpacingXProperty, value);
			}
		}

		#endregion //InterTileSpacingX

		#region InterTileSpacingXMaximizedResolved

		private static readonly DependencyPropertyKey InterTileSpacingXMaximizedResolvedPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("InterTileSpacingXMaximizedResolved",
			typeof(double), typeof(XamTileManager),
			0d,
			new PropertyChangedCallback(OnInterTileSpacingXMaximizedResolvedChanged)
			);

		/// <summary>
		/// Identifies the read-only <see cref="InterTileSpacingXMaximizedResolved"/> dependency property
		/// </summary>
		public static readonly DependencyProperty InterTileSpacingXMaximizedResolvedProperty = InterTileSpacingXMaximizedResolvedPropertyKey.DependencyProperty;

		private static void OnInterTileSpacingXMaximizedResolvedChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			XamTileManager control = target as XamTileManager;

			control._interTileSpacingXMaximizedResolved = (double)e.NewValue;
		}

		/// <summary>
		/// Returns the resolved value that will be used for spacing between the maximized tiles horizontally.
		/// </summary>
		/// <remarks>
		/// <para class="note"><b>Note:</b> if the <see cref="Infragistics.Controls.Layouts.MaximizedModeSettings.InterTileSpacingXMaximized"/> property is not set then the <see cref="InterTileSpacingX"/> value will be used.</para>
		/// </remarks>
		/// <seealso cref="InterTileSpacingX"/>
		/// <seealso cref="Infragistics.Controls.Layouts.MaximizedModeSettings.InterTileSpacingXMaximized"/>
		/// <seealso cref="XamTileManager.IsInMaximizedMode"/>
		/// <seealso cref="XamTileManager.MaximizedItems"/>

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]

		[ReadOnly(true)]
		public double InterTileSpacingXMaximizedResolved { get { return this._interTileSpacingXMaximizedResolved; } }

		#endregion //InterTileSpacingXMaximizedResolved

		#region InterTileSpacingXMinimizedResolved

		private static readonly DependencyPropertyKey InterTileSpacingXMinimizedResolvedPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("InterTileSpacingXMinimizedResolved",
			typeof(double), typeof(XamTileManager),
			0d,
			new PropertyChangedCallback(OnInterTileSpacingXMinimizedResolvedChanged)
			);

		/// <summary>
		/// Identifies the read-only <see cref="InterTileSpacingXMinimizedResolved"/> dependency property
		/// </summary>
		public static readonly DependencyProperty InterTileSpacingXMinimizedResolvedProperty = InterTileSpacingXMinimizedResolvedPropertyKey.DependencyProperty;

		private static void OnInterTileSpacingXMinimizedResolvedChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			XamTileManager control = target as XamTileManager;

			control._interTileSpacingXMinimizedResolved = (double)e.NewValue;
		}

		/// <summary>
		/// Returns the resolved value that will be used for spacing between the minimized tiles horizontally.
		/// </summary>
		/// <remarks>
		/// <para class="note"><b>Note:</b> if the <see cref="Infragistics.Controls.Layouts.MaximizedModeSettings.InterTileSpacingXMinimized"/> property is not set then the <see cref="InterTileSpacingX"/> value will be used.</para>
		/// </remarks>
		/// <seealso cref="InterTileSpacingX"/>
		/// <seealso cref="Infragistics.Controls.Layouts.MaximizedModeSettings.InterTileSpacingXMaximized"/>
		/// <seealso cref="XamTileManager.IsInMaximizedMode"/>
		/// <seealso cref="XamTileManager.MaximizedItems"/>

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]

		[ReadOnly(true)]
		public double InterTileSpacingXMinimizedResolved { get { return this._interTileSpacingXMinimizedResolved; } }

		#endregion //InterTileSpacingXMinimizedResolved

		#region InterTileSpacingY

		/// <summary>
		/// Identifies the <see cref="InterTileSpacingY"/> dependency property
		/// </summary>
		public static readonly DependencyProperty InterTileSpacingYProperty = DependencyPropertyUtilities.Register("InterTileSpacingY",
			typeof(double), typeof(XamTileManager),
			DependencyPropertyUtilities.CreateMetadata(2.0d, new PropertyChangedCallback(OnInterTileSpacingYChanged))
			);

		private static void OnInterTileSpacingYChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			ValidateInterTileSpacing(e.NewValue);

			XamTileManager instance = target as XamTileManager;

			instance._interTileSpacingY = (double)e.NewValue;

			instance.InitializeSpacingOnResizeController();

			instance.CalculateResolvedSpacing();

			if (instance._panel != null)
			{
				instance._panel.InvalidateMeasure();
				instance._panel.InvalidateArrange();
			}
		}

		/// <summary>
		/// Gets/sets the amount of spacing between tiles vertically.
		/// </summary>
		/// <remarks>
		/// <para class="note"><b>Note:</b> the default value for this property is 2.</para>
		/// </remarks>
		/// <seealso cref="InterTileSpacingYProperty"/>
		//[Description("Gets/sets the amount of spacing between tiles vertically.")]
		//[Category("TilesControl Properties")]
		public double InterTileSpacingY
		{
			get
			{
				return this._interTileSpacingY;
			}
			set
			{
				this.SetValue(XamTileManager.InterTileSpacingYProperty, value);
			}
		}

		#endregion //InterTileSpacingY

		#region InterTileSpacingYMaximizedResolved

		private static readonly DependencyPropertyKey InterTileSpacingYMaximizedResolvedPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("InterTileSpacingYMaximizedResolved",
			typeof(double), typeof(XamTileManager),
			0d,
			new PropertyChangedCallback(OnInterTileSpacingYMaximizedResolvedChanged)
			);

		/// <summary>
		/// Identifies the read-only <see cref="InterTileSpacingYMaximizedResolved"/> dependency property
		/// </summary>
		public static readonly DependencyProperty InterTileSpacingYMaximizedResolvedProperty = InterTileSpacingYMaximizedResolvedPropertyKey.DependencyProperty;

		private static void OnInterTileSpacingYMaximizedResolvedChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			XamTileManager control = target as XamTileManager;

			control._interTileSpacingYMaximizedResolved = (double)e.NewValue;
		}

		/// <summary>
		/// Returns the resolved value that will be used for spacing between the maximized tiles vertically.
		/// </summary>
		/// <remarks>
		/// <para class="note"><b>Note:</b> if the <see cref="Infragistics.Controls.Layouts.MaximizedModeSettings.InterTileSpacingYMaximized"/> property is not set then the <see cref="InterTileSpacingY"/> value will be used.</para>
		/// </remarks>
		/// <seealso cref="InterTileSpacingY"/>
		/// <seealso cref="Infragistics.Controls.Layouts.MaximizedModeSettings.InterTileSpacingYMaximized"/>
		/// <seealso cref="XamTileManager.IsInMaximizedMode"/>
		/// <seealso cref="XamTileManager.MaximizedItems"/>

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]

		[ReadOnly(true)]
		public double InterTileSpacingYMaximizedResolved { get { return this._interTileSpacingYMaximizedResolved; } }

		#endregion //InterTileSpacingYMaximizedResolved

		#region InterTileSpacingYMinimizedResolved

		private static readonly DependencyPropertyKey InterTileSpacingYMinimizedResolvedPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("InterTileSpacingYMinimizedResolved",
			typeof(double), typeof(XamTileManager),
			0d,
			new PropertyChangedCallback(OnInterTileSpacingYMinimizedResolvedChanged)
			);

		/// <summary>
		/// Identifies the read-only <see cref="InterTileSpacingYMinimizedResolved"/> dependency property
		/// </summary>
		public static readonly DependencyProperty InterTileSpacingYMinimizedResolvedProperty = InterTileSpacingYMinimizedResolvedPropertyKey.DependencyProperty;

		private static void OnInterTileSpacingYMinimizedResolvedChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			XamTileManager control = target as XamTileManager;

			control._interTileSpacingYMinimizedResolved = (double)e.NewValue;
		}

		/// <summary>
		/// Returns the resolved value that will be used for spacing between the minimized tiles vertically.
		/// </summary>
		/// <remarks>
		/// <para class="note"><b>Note:</b> if the <see cref="Infragistics.Controls.Layouts.MaximizedModeSettings.InterTileSpacingYMinimized"/> property is not set then the <see cref="InterTileSpacingY"/> value will be used.</para>
		/// </remarks>
		/// <seealso cref="InterTileSpacingY"/>
		/// <seealso cref="Infragistics.Controls.Layouts.MaximizedModeSettings.InterTileSpacingYMaximized"/>
		/// <seealso cref="XamTileManager.IsInMaximizedMode"/>
		/// <seealso cref="XamTileManager.MaximizedItems"/>

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]

		[ReadOnly(true)]
		public double InterTileSpacingYMinimizedResolved { get { return this._interTileSpacingYMinimizedResolved; } }

		#endregion //InterTileSpacingYMinimizedResolved

		#region IsAnimationInProgress

		private static readonly DependencyPropertyKey IsAnimationInProgressPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("IsAnimationInProgress",
			typeof(bool), typeof(XamTileManager),
			KnownBoxes.FalseBox,
			new PropertyChangedCallback(OnIsAnimationInProgressChanged)
			);

		/// <summary>
		/// Identifies the read-only <see cref="IsAnimationInProgress"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsAnimationInProgressProperty = IsAnimationInProgressPropertyKey.DependencyProperty;

		private static void OnIsAnimationInProgressChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			XamTileManager instance = target as XamTileManager;

			if (instance != null)
			{
				instance._isAnimationInProgress = (bool)e.NewValue;

				if (instance._isAnimationInProgress)
					instance.RaiseAnimationStarted();
				else
					instance.RaiseAnimationEnded();
			}
		}

		internal void SyncIsAnimationInProgress()
		{
			bool animationsInProgress = _panel != null && _panel.IsAnimationInProgress;

			if (animationsInProgress != _isAnimationInProgress)
				this.SetValue(IsAnimationInProgressPropertyKey, KnownBoxes.FromValue(animationsInProgress));

		}

		/// <summary>
		/// Returns true if tile animations are in progress (read-only)
		/// </summary>
		/// <seealso cref="IsAnimationInProgressProperty"/>

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]

		[ReadOnly(true)]
		public bool IsAnimationInProgress
		{
			get
			{
				return _isAnimationInProgress;
			}
		}

		#endregion //IsAnimationInProgress

		#region IsInMaximizedMode

		private static readonly DependencyPropertyKey IsInMaximizedModePropertyKey = DependencyPropertyUtilities.RegisterReadOnly("IsInMaximizedMode",
			typeof(bool), typeof(XamTileManager),
			KnownBoxes.FalseBox,
			new PropertyChangedCallback(OnIsInMaximizedModeChanged)
			);

		/// <summary>
		/// Identifies the read-only <see cref="IsInMaximizedMode"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsInMaximizedModeProperty = IsInMaximizedModePropertyKey.DependencyProperty;

		private static void OnIsInMaximizedModeChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			XamTileManager tm = target as XamTileManager;

			if (tm._panel != null)
			{
				tm._panel.InvalidateMeasure();
				tm._panel.InvalidateArrange();
			}

			tm._splitterMinExtent = 0;
			tm.InitializeSplitter();
			
			AutomationPeer peer = FrameworkElementAutomationPeer.FromElement(tm);

			if (peer != null)
				AutomationPeerHelper.InvalidateChildren(peer);

		}

		/// <summary>
		/// Returns true if there is at least one <see cref="XamTile"/> whose <see cref="XamTile.State"/> is 'Maximized'. (read-only)
		/// </summary>
		/// <seealso cref="IsInMaximizedModeProperty"/>

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]

		[ReadOnly(true)]
		public bool IsInMaximizedMode
		{
			get
			{
				return (bool)this.GetValue(XamTileManager.IsInMaximizedModeProperty);
			}
		}

		#endregion //IsInMaximizedMode

		#region ItemContainerStyle

		/// <summary>
		/// Identifies the <see cref="ItemContainerStyle"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ItemContainerStyleProperty = DependencyPropertyUtilities.Register("ItemContainerStyle",
			typeof(Style), typeof(XamTileManager),
			null, null
			);

		/// <summary>
		/// Returns or sets the style to use for <see cref="XamTile"/> containers that are generated for each item.
		/// </summary>
		/// <seealso cref="ItemContainerStyleProperty"/>
		public Style ItemContainerStyle
		{
			get
			{
				return (Style)this.GetValue(XamTileManager.ItemContainerStyleProperty);
			}
			set
			{
				this.SetValue(XamTileManager.ItemContainerStyleProperty, value);
			}
		}

		#endregion //ItemContainerStyle

		#region ItemHeaderTemplate

		/// <summary>
		/// Identifies the <see cref="ItemHeaderTemplate"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ItemHeaderTemplateProperty = DependencyPropertyUtilities.Register("ItemHeaderTemplate",
			typeof(DataTemplate), typeof(XamTileManager),
			DependencyPropertyUtilities.CreateMetadata(null)
			);

		/// <summary>
		/// Gets/sets the HeaderTemplate that will be set on an item's Tile.
		/// </summary>
		/// <seealso cref="ItemHeaderTemplateProperty"/>
		//[Description("Gets/sets the HeaderTemplate that will be set on an item's Tile.")]
		//[Category("TilesControl Properties")]
		public DataTemplate ItemHeaderTemplate
		{
			get
			{
				return (DataTemplate)this.GetValue(XamTileManager.ItemHeaderTemplateProperty);
			}
			set
			{
				this.SetValue(XamTileManager.ItemHeaderTemplateProperty, value);
			}
		}

		#endregion //ItemHeaderTemplate

		#region Items

		/// <summary>
		/// Returns the collection of items
		/// </summary>
		/// <seealso cref="ItemsSource"/>
		public IList Items
		{
			get { return _items; }
		}

		#endregion //Items	
    
		#region ItemsSource

		/// <summary>
		/// Identifies the <see cref="ItemsSource"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ItemsSourceProperty = DependencyPropertyUtilities.Register("ItemsSource",
			typeof(IEnumerable), typeof(XamTileManager),
			DependencyPropertyUtilities.CreateMetadata(null, new PropertyChangedCallback(OnItemsSourceChanged))
			);

		private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			XamTileManager instance = (XamTileManager)d;

			instance._items.ItemsSource = e.NewValue as IEnumerable;
		}

		/// <summary>
		/// Returns or sets an enumerable used to populate the <see cref="Items"/> collection
		/// </summary>
		/// <seealso cref="ItemsSourceProperty"/>
		public IEnumerable ItemsSource
		{
			get
			{
				return (IEnumerable)this.GetValue(XamTileManager.ItemsSourceProperty);
			}
			set
			{
				this.SetValue(XamTileManager.ItemsSourceProperty, value);
			}
		}

		#endregion //ItemsSource

		#region ItemTemplate

		/// <summary>
		/// Identifies the <see cref="ItemTemplate"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ItemTemplateProperty = DependencyPropertyUtilities.Register("ItemTemplate",
			typeof(DataTemplate), typeof(XamTileManager),
			null, null
			);

		/// <summary>
		/// Returns or sets the data template to use for each item in the <see cref="Items"/> collection.
		/// </summary>
		/// <seealso cref="ItemTemplateProperty"/>
		public DataTemplate ItemTemplate
		{
			get
			{
				return (DataTemplate)this.GetValue(XamTileManager.ItemTemplateProperty);
			}
			set
			{
				this.SetValue(XamTileManager.ItemTemplateProperty, value);
			}
		}

		#endregion //ItemTemplate

		#region ItemTemplateMaximized

		/// <summary>
		/// Identifies the <see cref="ItemTemplateMaximized"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ItemTemplateMaximizedProperty = DependencyPropertyUtilities.Register("ItemTemplateMaximized",
			typeof(DataTemplate), typeof(XamTileManager),
			DependencyPropertyUtilities.CreateMetadata(null)
			);

		/// <summary>
		/// Gets/sets the ItemTemplate that will be used when a Tile's State is 'Maximized'.
		/// </summary>
		/// <seealso cref="ItemTemplateMaximizedProperty"/>
		//[Description("Gets/sets the ItemTemplate that will be used when a Tile's State is 'Maximized'.")]
		//[Category("TilesControl Properties")]
		public DataTemplate ItemTemplateMaximized
		{
			get
			{
				return (DataTemplate)this.GetValue(XamTileManager.ItemTemplateMaximizedProperty);
			}
			set
			{
				this.SetValue(XamTileManager.ItemTemplateMaximizedProperty, value);
			}
		}

		#endregion //ItemTemplateMaximized

		#region ItemTemplateMinimized

		/// <summary>
		/// Identifies the <see cref="ItemTemplateMinimized"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ItemTemplateMinimizedProperty = DependencyPropertyUtilities.Register("ItemTemplateMinimized",
			typeof(DataTemplate), typeof(XamTileManager),
			DependencyPropertyUtilities.CreateMetadata(null)
			);

		/// <summary>
		/// Gets/sets the ItemTemplate that will be used when a Tile's State is 'Minimized'.
		/// </summary>
		/// <seealso cref="ItemTemplateMinimizedProperty"/>
		//[Description("Gets/sets the ItemTemplate that will be used when a Tile's State is 'Minimized'.")]
		//[Category("TilesControl Properties")]
		public DataTemplate ItemTemplateMinimized
		{
			get
			{
				return (DataTemplate)this.GetValue(XamTileManager.ItemTemplateMinimizedProperty);
			}
			set
			{
				this.SetValue(XamTileManager.ItemTemplateMinimizedProperty, value);
			}
		}

		#endregion //ItemTemplateMinimized

		#region ItemTemplateMinimizedExpanded

		/// <summary>
		/// Identifies the <see cref="ItemTemplateMinimizedExpanded"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ItemTemplateMinimizedExpandedProperty = DependencyPropertyUtilities.Register("ItemTemplateMinimizedExpanded",
			typeof(DataTemplate), typeof(XamTileManager),
			DependencyPropertyUtilities.CreateMetadata(null)
			);


		/// <summary>
		/// Gets/sets the ItemTemplate that will be used when a Tile's State is 'MinimizedExpanded'.
		/// </summary>
		/// <seealso cref="ItemTemplateMinimizedExpandedProperty"/>
		//[Description("Gets/sets the ItemTemplate that will be used when a Tile's State is 'MinimizedExpanded'.")]
		//[Category("TilesControl Properties")]
		public DataTemplate ItemTemplateMinimizedExpanded
		{
			get
			{
				return (DataTemplate)this.GetValue(XamTileManager.ItemTemplateMinimizedExpandedProperty);
			}
			set
			{
				this.SetValue(XamTileManager.ItemTemplateMinimizedExpandedProperty, value);
			}
		}

		#endregion //ItemTemplateMinimizedExpanded

		// JJD 03/14/12 - TFS100150 - Added touch support
		#region IsTouchSupportEnabled


		/// <summary>
		/// Identifies the <see cref="IsTouchSupportEnabled"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsTouchSupportEnabledProperty = DependencyPropertyUtilities.Register("IsTouchSupportEnabled",
			typeof(bool), typeof(XamTileManager),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.TrueBox, new PropertyChangedCallback(OnIsTouchSupportEnabledChanged))
			);

		private static void OnIsTouchSupportEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			XamTileManager instance = (XamTileManager)d;

			instance._scrollHelper.IsEnabled = (bool)e.NewValue;


			instance.SetCurrentValue(FrameworkElement.IsManipulationEnabledProperty, KnownBoxes.FromValue((bool)e.NewValue));

		}

		/// <summary>
		/// Returns or sets whether touch support is enabled for this control
		/// </summary>
		/// <seealso cref="IsTouchSupportEnabledProperty"/>
		public bool IsTouchSupportEnabled
		{
			get
			{
				return (bool)this.GetValue(XamTileManager.IsTouchSupportEnabledProperty);
			}
			set
			{
				this.SetValue(XamTileManager.IsTouchSupportEnabledProperty, value);
			}
		}

		#endregion //IsTouchSupportEnabled

		#region MaximizedModeSettings

		/// <summary>
		/// Identifies the <see cref="MaximizedModeSettings"/> dependency property
		/// </summary>
		public static readonly DependencyProperty MaximizedModeSettingsProperty = DependencyPropertyUtilities.Register("MaximizedModeSettings",
			typeof(MaximizedModeSettings), typeof(XamTileManager),
			DependencyPropertyUtilities.CreateMetadata(null, new PropertyChangedCallback(OnMaximizedModeSettingsChanged))
			);

		private static void OnMaximizedModeSettingsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			XamTileManager instance = (XamTileManager)d;

			MaximizedModeSettings oldMaximizedModeSettings = instance._maximizedModeSettings;
			MaximizedModeSettings newMaximizedModeSettings = e.NewValue as MaximizedModeSettings;

			if (oldMaximizedModeSettings != null)
			{
				// unwire the old settings property changed handler
				oldMaximizedModeSettings.PropertyChanged -= new PropertyChangedEventHandler(instance.OnMaximizedModeSettingsPropertyChanged);

				instance.RemoveLogicalChild(oldMaximizedModeSettings);
				instance._extraLogicalChildren.Remove(oldMaximizedModeSettings);

			}

			instance._maximizedModeSettings = newMaximizedModeSettings;

			if (instance._maximizedModeSettings != null)
			{
				instance._maximizedModeSettingsSafe = instance._maximizedModeSettings;

				// wire the new settings property changed handler
				instance._maximizedModeSettings.PropertyChanged += new PropertyChangedEventHandler(instance.OnMaximizedModeSettingsPropertyChanged);


				instance.AddLogicalChild(instance._maximizedModeSettings);
				instance._extraLogicalChildren.Add(instance._maximizedModeSettings);

			}
			else
				instance._maximizedModeSettingsSafe = new MaximizedModeSettings();


			if (instance._panel != null && instance.IsInMaximizedMode)
			{
				instance.InvalidateArrange();
				instance.InvalidateMeasure();
			}

		}

		private void OnMaximizedModeSettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (this._panel == null || this.IsInMaximizedMode == false)
				return;

			bool affectsSplitterLocation = false;

			switch (e.PropertyName)
			{
				case "AllowTileSizing":
				case "AllowTileDragging":
				case "RepositionAnimation":
				case "ResizeAnimation":
					break;

				case "InterTileSpacingXMaximized":
				case "InterTileSpacingXMinimized":
				case "InterTileSpacingYMaximized":
				case "InterTileSpacingYMinimized":
					this.CalculateResolvedSpacing();
					this._panel.InvalidateMeasure();
					break;

				case "InterTileAreaSpacing":
				case "MaximizedTileLocation":
				case "ShowTileAreaSplitter":
					this.CalculateResolvedSpacing();
					this.InitializeSplitter();
					this._panel.InvalidateMeasure();
					affectsSplitterLocation = true;
					break;

				case "MinimizedTileExpandButtonVisibility":
				case "MinimizedTileExpansionMode":
					this.BumpLayoutVersion();
					this._panel.InvalidateMeasure();
					break;

				case "MaximizedTileLayoutOrder":
				case "ShowAllMinimizedTiles":
				default:
					this._panel.InvalidateMeasure();
					break;
			}

			if (affectsSplitterLocation)
				this._panel.InvalidateArrange();
		}

		/// <summary>
		/// Gets/sets the settings that are used to layout Tiles when in maximized mode
		/// </summary>
		/// <seealso cref="IsInMaximizedMode"/>
		/// <seealso cref="MaximizedModeSettingsProperty"/>
		//[Description("Gets/sets the settings that are used to layout Tiles when in maximized mode")]
		//[Category("TilesControl Properties")]
		public MaximizedModeSettings MaximizedModeSettings
		{
			get
			{
				return (MaximizedModeSettings)this.GetValue(XamTileManager.MaximizedModeSettingsProperty);
			}
			set
			{
				this.SetValue(XamTileManager.MaximizedModeSettingsProperty, value);
			}
		}

		#endregion //MaximizedModeSettings

		#region MaximizedItems

		/// <summary>
		/// Returns a read-only collection of the items that are maximized.
		/// </summary>

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]

		[ReadOnly(true)]
		[Bindable(true)]
		public ReadOnlyObservableCollection<object> MaximizedItems
		{
			get { return this._maximizedItemsReadOnly; }
		}

		#endregion //MaximizedItems

		#region MaximizedTileLimit

		/// <summary>
		/// Identifies the <see cref="MaximizedTileLimit"/> dependency property
		/// </summary>
		public static readonly DependencyProperty MaximizedTileLimitProperty = DependencyPropertyUtilities.Register("MaximizedTileLimit",
			typeof(int), typeof(XamTileManager),
			DependencyPropertyUtilities.CreateMetadata((int)1, new PropertyChangedCallback(OnMaximizedTileLimitChanged))
			);

		private static void OnMaximizedTileLimitChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			XamTileManager instance = (XamTileManager)d;

			if (instance.IsInitializedInternal)
			    instance.ProcessNewMaximizedTileLimit((int)e.NewValue);

		}

		/// <summary>
		/// Gets/sets the limit on the number of 'Maximized' tiles that will be allowed.
		/// </summary>
		/// <seealso cref="MaximizedTileLimitProperty"/>
		//[Description("Gets/sets the limit on the number of 'Maximized' tiles that will be allowed.")]
		//[Category("TilesControl Properties")]
		public int MaximizedTileLimit
		{
			get
			{
				return (int)this.GetValue(MaximizedTileLimitProperty);
			}
			set
			{
				this.SetValue(MaximizedTileLimitProperty, value);
			}
		}

		#endregion //MaximizedTileLimit



#region Infragistics Source Cleanup (Region)
















#endregion // Infragistics Source Cleanup (Region)


		#region NormalModeSettings

		/// <summary>
		/// Identifies the <see cref="NormalModeSettings"/> dependency property
		/// </summary>
		public static readonly DependencyProperty NormalModeSettingsProperty = DependencyPropertyUtilities.Register("NormalModeSettings",
			typeof(NormalModeSettings), typeof(XamTileManager),
			DependencyPropertyUtilities.CreateMetadata(null, new PropertyChangedCallback(OnNormalModeSettingsChanged))
			);

		private static void OnNormalModeSettingsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			XamTileManager instance = (XamTileManager)d;
			NormalModeSettings oldNormalModeSettings = instance._normalModeSettings; ;
			NormalModeSettings newNormalModeSettings = e.NewValue as NormalModeSettings;

			if (oldNormalModeSettings != null)
			{
				// unwire the old settings property changed handler
				oldNormalModeSettings.PropertyChanged -= new PropertyChangedEventHandler(instance.OnNormalModeSettingsPropertyChanged);

				instance.RemoveLogicalChild(oldNormalModeSettings);
				instance._extraLogicalChildren.Remove(oldNormalModeSettings);

			}

			instance._normalModeSettings = newNormalModeSettings;

			if (instance._normalModeSettings != null)
			{
				instance._normalModeSettingsSafe = instance._normalModeSettings;

				// wire the new settings property changed handler
				instance._normalModeSettings.PropertyChanged += new PropertyChangedEventHandler(instance.OnNormalModeSettingsPropertyChanged);


				instance.AddLogicalChild(instance._normalModeSettings);
				instance._extraLogicalChildren.Add(instance._normalModeSettings);

			}
			else
				instance._normalModeSettingsSafe = new NormalModeSettings();

			if (instance._panel != null && !instance.IsInMaximizedMode)
			{
				instance.InvalidateArrange();
				instance.InvalidateMeasure();
			}
		}

		private void OnNormalModeSettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (_panel == null || this.IsInMaximizedMode == true)
				return;

			switch (e.PropertyName)
			{
				case "AllowTileSizing":
				case "AllowTileDragging":
				case "RepositionAnimation":
				case "ResizeAnimation":
					break;


				case "EmptyTileAreaBackground":
					this._panel.InvalidateVisual();
					break;


				case "ExplicitLayoutTileSizeBehavior":
				case "HorizontalTileAreaAlignment":
				case "MaxColumns":
				case "MaxRows":
				case "MinColumns":
				case "MinRows":
				case "MinHeight":
				case "MaxHeight":
				case "TileLayoutOrder":
				case "ShowAllTiles":
				case "VerticalTileAreaAlignment":
				default:
					this._panel.InvalidateMeasure();
					break;
			}
		}

		/// <summary>
		/// Gets/sets the settings that are used to layout Tiles when not in maximized mode
		/// </summary>
		/// <seealso cref="IsInMaximizedMode"/>
		/// <seealso cref="NormalModeSettingsProperty"/>
		//[Description("Gets/sets the settings that are used to layout Tiles when not in maximized mode")]
		//[Category("TilesControl Properties")]
		public NormalModeSettings NormalModeSettings
		{
			get
			{
				return (NormalModeSettings)this.GetValue(XamTileManager.NormalModeSettingsProperty);
			}
			set
			{
				this.SetValue(XamTileManager.NormalModeSettingsProperty, value);
			}
		}

		#endregion //NormalModeSettings

		#region SerializationIdPath

		/// <summary>
		/// Identifies the <see cref="SerializationIdPath"/> dependency property
		/// </summary>
		public static readonly DependencyProperty SerializationIdPathProperty = DependencyPropertyUtilities.Register("SerializationIdPath",
			typeof(string), typeof(XamTileManager),
			DependencyPropertyUtilities.CreateMetadata(null, new PropertyChangedCallback(OnSerializationIdPathChanged))
			);

		private static void OnSerializationIdPathChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			XamTileManager control = target as XamTileManager;

			control._serializationIdPath = (string)e.NewValue;
		}

		/// <summary>
		/// Gets/sets a path to a value on the source object that will be used to initialize the <see cref="XamTileManager.SerializationIdProperty"/> of each <see cref="XamTile"/>. 
		/// </summary>
		/// <remarks>
		/// <para class="note"><b>Note:</b> this setting is ignored for <see cref="XamTile"/>'s that are direct children of the XamTileManager. This setting only has meaning for <see cref="XamTile"/>s that get generated as item containers.</para>
		/// </remarks>
		/// <seealso cref="SerializationIdPathProperty"/>
		//[Description("Gets/sets a path to a value on the source object that will be used to initialize the SerializationId of each Tile.")]
		//[Category("TilesControl Properties")]
		public string SerializationIdPath
		{
			get
			{
				return this._serializationIdPath;
			}
			set
			{
				this.SetValue(XamTileManager.SerializationIdPathProperty, value);
			}
		}

		#endregion //SerializationIdPath

		#region TileAreaPadding

		/// <summary>
		/// Identifies the <see cref="TileAreaPadding"/> dependency property
		/// </summary>
		public static readonly DependencyProperty TileAreaPaddingProperty = DependencyPropertyUtilities.Register("TileAreaPadding",
			typeof(Thickness), typeof(XamTileManager),
			DependencyPropertyUtilities.CreateMetadata(new Thickness(), new PropertyChangedCallback(OnTileAreaPaddingChanged))
			);

		private static void OnTileAreaPaddingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			XamTileManager instance = (XamTileManager)d;

			if (instance._panel != null)
			{
				instance._panel.InvalidateMeasure();
				instance._panel.InvalidateArrange();
			}
		}

		/// <summary>
		/// Get/sets that amount of space between the XamTileManager and the area where the tiles are arranged.
		/// </summary>
		/// <seealso cref="TileAreaPaddingProperty"/>
		//[Description("Get/sets that amount of space between the XamTileManager and the area where the tiles are arranged.")]
		//[Category("TilesControl Properties")]
		public Thickness TileAreaPadding
		{
			get
			{
				return (Thickness)this.GetValue(XamTileManager.TileAreaPaddingProperty);
			}
			set
			{
				this.SetValue(XamTileManager.TileAreaPaddingProperty, value);
			}
		}

		#endregion //TileAreaPadding

		#region TileCloseAction

		/// <summary>
		/// Identifies the <see cref="TileCloseAction"/> dependency property
		/// </summary>
		public static readonly DependencyProperty TileCloseActionProperty = DependencyPropertyUtilities.Register("TileCloseAction",
			typeof(TileCloseAction), typeof(XamTileManager),
			DependencyPropertyUtilities.CreateMetadata(TileCloseAction.Default, new PropertyChangedCallback(OnTileCloseActionChanged))
			);

		private static void OnTileCloseActionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			CoreUtilities.ValidateEnum(typeof(TileCloseAction), e.NewValue);

			XamTileManager instance = (XamTileManager)d;

			instance.BumpLayoutVersion();

		}

		/// <summary>
		/// Gets/sets whether Tiles can be closed by the user.
		/// </summary>
		/// <remarks>
		/// <para class="note"><b>Note</b>: if TileCloseAction is set to 'DoNothing' (its default value) then, by default, <see cref="XamTile"/>s can't be closed. 
		/// However, this behavior can be overridden for individual tiles by setting their <see cref="XamTile.CloseAction"/> property.</para>
		/// </remarks>
		/// <seealso cref="TileCloseActionProperty"/>
		/// <seealso cref="XamTile.CloseAction"/>
		/// <seealso cref="XamTile.CloseButtonVisibility"/>
		/// <seealso cref="XamTile.CloseButtonVisibilityResolved"/>
		//[Description("Gets/sets what happens when Tiles are closed.")]
		//[Category("TilesControl Properties")]
		public TileCloseAction TileCloseAction
		{
			get
			{
				return (TileCloseAction)this.GetValue(TileCloseActionProperty);
			}
			set
			{
				this.SetValue(TileCloseActionProperty, value);
			}
		}

		#endregion //TileCloseAction

		#endregion //Public Properties	
    
		#region Internal Properties

		#region CanDragTiles

		internal bool CanDragTiles
		{
			get
			{
				if (this.IsInMaximizedMode)
					return this.MaximizedModeSettingsSafe.AllowTileDragging != AllowTileDragging.No;
				else
					return this.NormalModeSettingsSafe.AllowTileDragging != AllowTileDragging.No;
			}
		}

		#endregion //CanDragTiles

		#region CurrentMinimizedExpandedTile

		internal XamTile CurrentMinimizedExpandedTile { get { return this._currentMinimizedExpandedTile; } }

		#endregion //CurrentMinimizedExpandedTile

		#region IsExpandedWhenMinimizedDefault

		internal bool IsExpandedWhenMinimizedDefault
		{
			get
			{
				return this.MaximizedModeSettingsSafe.MinimizedTileExpansionMode == MinimizedTileExpansionMode.AllowMultipleExpandAllInitially;
			}
		}

		#endregion //IsExpandedWhenMinimizedDefault	

		#region IsInitializedInternal

		internal bool IsInitializedInternal { get { return _isInitialized; } }

		#endregion //IsInitializedInternal	
    
		#region IsLoadingLayout

		internal bool IsLoadingLayout
		{
			get { return this._isLoadingLayout; }
			set
			{
				if (value != this._isLoadingLayout)
				{
					this._isLoadingLayout = value;

					if (this._isLoadingLayout == false)
					{
						this._itemSerializationMap = null;
					}
				}
			}
		}
		#endregion //IsLoadingLayout

		#region LayoutManager

		internal TileLayoutManager LayoutManager { get { return _layoutManager; } }

		#endregion //LayoutManager	
    
		#region LayoutVersion

		internal int LayoutVersion
		{
			get { return _layoutVersion; }
		}

		#endregion //LayoutVersion	

		#region MaximizedItemsInternal

		internal ObservableCollectionExtended<object> MaximizedItemsInternal { get { return this._maximizedItems; } }

		#endregion //MaximizedItemsInternal	

		#region MaximizedModeSettingsSafe

		internal MaximizedModeSettings MaximizedModeSettingsSafe
		{
			get
			{
				return _maximizedModeSettingsSafe;
			}
		}

		#endregion //MaximizedModeSettingsSafe

		#region MinimizedAreaExplicitExtentX

		internal double MinimizedAreaExplicitExtentX
		{
			get
			{
				return this._minimizedAreaExplicitExtentX;
			}
			set
			{
				if (value != this._minimizedAreaExplicitExtentX)
				{
					this._minimizedAreaExplicitExtentX = value;

					if (this._panel != null)
						this._panel.InvalidateMeasure();
				}
			}
		}

		#endregion //MinimizedAreaExplicitExtentY

		#region MinimizedAreaExplicitExtentY

		internal double MinimizedAreaExplicitExtentY
		{
			get
			{
				return this._minimizedAreaExplicitExtentY;
			}
			set
			{
				if (value != this._minimizedAreaExplicitExtentY)
				{
					this._minimizedAreaExplicitExtentY = value;

					if (this._panel != null)
						this._panel.InvalidateMeasure();
				}
			}
		}

		#endregion //MinimizedAreaExplicitExtentY	

		#region NormalModeSettingsSafe

		internal NormalModeSettings NormalModeSettingsSafe
		{
			get
			{
				return _normalModeSettingsSafe;
			}
		}

		#endregion //NormalModeSettingsSafe	

		#region Panel

		internal TileAreaPanel Panel { get { return _panel; } }

		#endregion //Panel	
    
		#region PropChangeListeners

		/// <summary>
		/// Gets collection of property change listeners.
		/// </summary>
		internal PropertyChangeListenerList PropChangeListeners
		{
			get
			{
				return _propChangeListeners;
			}
		}

		#endregion // PropChangeListeners

		#region ShouldAnimate

		internal bool ShouldAnimate
		{
			get
			{
				if (DesignerProperties.GetIsInDesignMode(this))
					return false;

				if (this.IsInMaximizedMode)
					return this.MaximizedModeSettingsSafe.ShouldAnimate;
				else
					return this.NormalModeSettingsSafe.ShouldAnimate;
			}
		}

		#endregion //ShouldAnimate	

		#region Splitter

		internal TileAreaSplitter Splitter { get { return _splitter; } }

		#endregion //Splitter	
    
		#region SplitterMinExtent

		internal double SplitterMinExtent
		{
			get
			{
				this.InitializeSplitter();

				// JJD 1/6/12 - TFS98941
				// if we have a splitter and the min extent has not been initialized 
				// then call InitializeSplitterMinExtent
				if (_splitterMinExtent == 0 &&
					_splitter != null)
				{
					this.InitializeSplitterMinExtent();
				}

				return this._splitterMinExtent;
			}
		}

		#endregion //SplitterMinExtent	

		#endregion //Internal Properties	

		#region IsStyleAlreadySet

		// used to know if we have already set the item container style on a XamTile
		private static readonly DependencyProperty IsStyleAlreadySetProperty = DependencyPropertyUtilities.RegisterAttached("IsStyleAlreadySet",
			typeof(bool), typeof(XamTileManager),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.FalseBox)
			);

		#endregion //IsStyleAlreadySet
        
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

		#region GetInViewRect

		/// <summary>
		/// Returns the area of the specified tile that is in view or will be in view when the current animations end.
		/// </summary>
		/// <param name="tile">The tile in question.</param>
		/// <returns>A rect (in coordinates relative to the tile) that represents the area of the tile that will be in view once all animations have ended. If no part of the tile will be in view then an empty rect is returned.</returns>
		public Rect GetInViewRect(XamTile tile)
		{
			return this._layoutManager.GetInViewRect(tile);
		}

		#endregion //GetInViewRect	

		#region GetItemInfo

		/// <summary>
		/// Gets the associated info for an item.
		/// </summary>
		/// <param name="item">The item in question.</param>
		/// <returns>The associated ItemInfo object that can be used to get/or set certain status information or null if the item doesn't exist in the Items collection.</returns>
		public ItemTileInfo GetItemInfo(object item)
		{
			if (item == null)
				return null;

			// First call the GetItemInfo with false.
			// This will use a hash to find an existing item which is most
			// likely much faster than the IndexOf call below
			ItemTileInfo info = _layoutManager.GetItemInfo(item, false, -1);

			if (info != null)
				return info;

			int index = this.Items.IndexOf(item);

			if (index < 0)
				return null;

			return this._layoutManager.GetItemInfo(item, true, index);
		}

		#endregion //GetItemInfo

		#region ScrollIntoView

		/// <summary>
		/// Scrolls the item into view
		/// </summary>
		/// <param name="item">The item to scroll</param>
		public void ScrollIntoView(object item)
		{

			if (this.IsLoaded && this._panel != null)
				this.ProcessScrollIntoView(item);
			else
				this.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action<object>(this.ProcessScrollIntoView), item);






		}

		#endregion //ScrollIntoView	

		#region TileFromItem

		/// <summary>
		/// Gets the associated tile for an item.
		/// </summary>
		/// <param name="item">The item in question.</param>
		/// <returns>The associated Tile or null if the Tile hasn't been generated.</returns>
		public XamTile TileFromItem(object item)
		{
			XamTile tile = item as XamTile;

			if (tile != null)
				return tile;

			TileLayoutManager.LayoutItem li = _layoutManager.GetLayoutItemFromMap(item);
			if (li == null)
				return null;

			return li.Container as XamTile;
		}

		#endregion //TileFromItem

		#endregion //Public Methods	
    
		#region Protected Methods

		#region ClearContainerForItemOverride

		/// <summary>
		/// Called to clear a tile that wrapped an item
		/// </summary>
		/// <param name="tile">The <see cref="XamTile"/> that will contained the item</param>
		/// <param name="item">The contained item.</param>
		internal protected virtual void ClearContainerForItemOverride(XamTile tile, object item)
		{
			if (tile != null && tile != item)
			{
				tile.UnwireOwner();

				string headerPath = this.HeaderPath;

				if (headerPath != null && headerPath.Length > 0)
					tile.ClearValue(XamTile.HeaderProperty);

				string serializationIdPath = this.SerializationIdPath;

				if (serializationIdPath != null && serializationIdPath.Length > 0)
					tile.ClearValue( XamTileManager.SerializationIdProperty);

				object content = tile.Content;

				if (content == item || content is DependencyObject)
					tile.ClearValue(XamTile.ContentProperty);

				tile.ClearValue(DataContextProperty);

				tile.ClearValue(XamTileManager.ColumnProperty);
				tile.ClearValue(XamTileManager.ColumnSpanProperty);
				tile.ClearValue(XamTileManager.ColumnWeightProperty);
				tile.ClearValue(XamTileManager.RowProperty);
				tile.ClearValue(XamTileManager.RowSpanProperty);
				tile.ClearValue(XamTileManager.RowWeightProperty);
				
				tile.ClearValue(XamTileManager.ConstraintsProperty);
				tile.ClearValue(XamTileManager.ConstraintsMaximizedProperty);
				tile.ClearValue(XamTileManager.ConstraintsMinimizedExpandedProperty);
				tile.ClearValue(XamTileManager.ConstraintsMinimizedProperty);

				XamTileAutomationPeer tilePeer = FrameworkElementAutomationPeer.FromElement(tile) as XamTileAutomationPeer;

				if (tilePeer != null)
				{
					TileItemAutomationPeer itemPeer = tilePeer.EventsSource as TileItemAutomationPeer;

					if (itemPeer != null)
					{
						itemPeer.InvalidatePeer();
						AutomationPeerHelper.InvalidateChildren(itemPeer);
						itemPeer.RaiseAutomationEvent(AutomationEvents.StructureChanged);
					}

					tilePeer.InvalidatePeer();
					AutomationPeerHelper.InvalidateChildren(tilePeer);
					tilePeer.RaiseAutomationEvent(AutomationEvents.StructureChanged);

					tilePeer.EventsSource = null;
				}

			}
		}

		#endregion //ClearContainerForItemOverride

		#region GetContainerForItemOverride

		/// <summary>
		/// Called to create a tile to wrap an item
		/// </summary>
		/// <returns>A new <see cref="XamTile"/> instance.</returns>
		internal protected virtual XamTile GetContainerForItemOverride()
		{
			return new XamTile();
		}

		#endregion //GetContainerForItemOverride	

		#region IsItemItsOwnContainerOverride

		/// <summary>
		/// Determines if the item is its own tile
		/// </summary>
		/// <param name="item">The item to check.</param>
		/// <returns>true if the item is a <see cref="XamTile"/>, otherwise false.</returns>
		internal protected virtual bool IsItemItsOwnContainerOverride(object item)
		{
			return item is XamTile;
		}

		#endregion //IsItemItsOwnContainerOverride

		#region OnItemsChanged

		/// <summary>
		/// Called when the items collection has changed
		/// </summary>
		/// <param name="e">The event arguments that contain information about what has changed.</param>
		protected virtual void OnItemsChanged(NotifyCollectionChangedEventArgs e)
		{
		}

		#endregion //OnItemsChanged

		#region PrepareContainerForItemOverride

		/// <summary>
		/// Called to prepare a tile to wrap an item
		/// </summary>
		/// <param name="container">The <see cref="XamTile"/> that will contain the item</param>
		/// <param name="item">The item to be contained.</param>
		internal protected virtual void PrepareContainerForItemOverride(XamTile container, object item)
		{
			this.InitializeTileContainer(item, container);
		}

		#endregion //PrepareContainerForItemOverride

		#endregion //Protected Methods

		#region Internal Methods

		#region BumpLayoutVersion

		internal void BumpLayoutVersion()
		{
			_layoutVersion++;
			_propChangeListeners.OnPropertyValueChanged(this, "LayoutVersion", null);

			if (_panel != null)
			{
				_panel.InvalidateMeasure();
				_panel.InvalidateArrange();
			}
		}

		#endregion //BumpLayoutVersion	

		#region ChangeTileState

		internal bool ChangeTileState(XamTile tile, TileState newState)
		{
			return this.ChangeTileState(tile, newState, tile.State);
		}

		internal bool ChangeTileState(XamTile tile, TileState newState, TileState oldState)
		{
			// JJD 10/11/11 - TFS91173, TFS90845 and TFS90952
			// Keep track of the initial tile whose state id being changed. If this
			// method is re-entered for the same tile then return.
			if (_settingStateOfTile == null)
				this._settingStateOfTile = tile;
			else
				if (_settingStateOfTile == tile)
					return false;

			try
			{
				return this.ChangeTileStateHelper(tile, newState, oldState);
			}
			finally
			{
				// JJD 10/11/11 - TFS91173, TFS90845 and TFS90952
				// reset from the stack variable cached above if the tile
				// matches it since this routine can be re-entered
				// if e.g. the MaximizedTileLimit was exceeded and another tile was un-maximized which would
				// cause this method to be entered for that tile.
				if (this._settingStateOfTile == tile)
				{
					_settingStateOfTile = null;
				}
			}
		}

		private bool ChangeTileStateHelper(XamTile tile, TileState newState, TileState oldState)
		{
			object item = this.ItemFromTile(tile);

			// JJD 10/11/11 - TFS91173, TFS90845 and TFS90952
			// Only raise the changing event for the original tile whose state we were changing.
			// This avoids raising the event other tiles since this routine can be re-entered
			// if e.g. the MaximizedTileLimit was exceeded and another tile was un-maximized which would
			// cause this method to be entered for that tile.
			if (_settingStateOfTile == tile)
			{
				TileStateChangingEventArgs args = new TileStateChangingEventArgs(tile, item, newState);

				// raise the before event
				this.OnTileStateChanging(args);

				if (args.Cancel == true)
					return false;

				newState = args.NewState;
			}

			if (newState == oldState)
				return false;

			bool bumpLayoutVersion = false;

			// If we are loading a layout then bypass maintaining _maximizedItems
			if (!this.IsLoadingLayout)
			{
				if (oldState == TileState.Maximized)
				{
					int index = this.GetIndexInMaximizedCollection(tile);

					
					
					
					
					

					if (index >= 0)
					{
						this._maximizedItems.RemoveAt(index);
					}

					tile.IsMaximized = false;

					// Always bump the layout version even if we didn't remove it from the collection
					bumpLayoutVersion = true;
				}
				else if (newState == TileState.Maximized)
				{
					tile.IsMaximized = true;

					int index = this.GetIndexInMaximizedCollection(tile);

					
					
					

					if (index < 0)
					{
						
						int limit = this.MaximizedTileLimit;

						this._maximizedItems.BeginUpdate();

						while (this._maximizedItems.Count > 0 &&
								this._maximizedItems.Count > limit - 1)
						{
							
							
							// Remove the last item until we get under the limit
							this.RemoveMaximizedItemAtIndex(this._maximizedItems.Count - 1);
						}

						this._maximizedItems.Add(item);

						this._maximizedItems.EndUpdate();

						
						
						
					}

					
					// Always bump the layout version even if we didn't add it to the collection
					bumpLayoutVersion = true;
				}
			}

			// MD 5/14/10 - TFS32080
			// Moved below. See comments on moved line.
			//tile.State = newState;

			
			
			
			// JJD 10/11/11 - [TFS91626]
			// Only call VerifyIsInMaximizedModeProperty for the original tile whose state we were changing.
			// This avoids a situation where replacing a maximized tile when the limit is one 
			// would remove the existing maximized tile and therefore temporarily be in a transient
			// state where IsInMaximized mode would verify to false erroneously.
			if (_settingStateOfTile == tile)
				this.VerifyIsInMaximizedModeProperty();

			// If the state is being set to one of the Minimized... states then
			// make sure the IsExpandedWhenMinimizedResolved is synched up
			switch (newState)
			{
				case TileState.MinimizedExpanded:
					if ( tile.IsExpandedWhenMinimizedResolved == false)
						tile.IsExpandedWhenMinimized = true;
					break;
				case TileState.Minimized:
					if ( tile.IsExpandedWhenMinimizedResolved == true)
						tile.IsExpandedWhenMinimized = false;
					break;
			}

			
#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)


			
			if (newState == TileState.MinimizedExpanded && this.IsInMaximizedMode)
			{
				if (tile != this._currentMinimizedExpandedTile)
				{
					// if we only alLow one expanded guy then minimize the existing expanded tile
					if (this.MaximizedModeSettingsSafe.MinimizedTileExpansionMode == MinimizedTileExpansionMode.AllowOne)
					{
						if (this._currentMinimizedExpandedTile != null)
						{
							if (this._currentMinimizedExpandedTile.State == TileState.MinimizedExpanded)
							{
								this._currentMinimizedExpandedTile.IsExpandedWhenMinimized = null;
								this._currentMinimizedExpandedTile.ResolveState();
							}
							else
							{
								item = this.ItemFromTile(this._currentMinimizedExpandedTile);

								ItemTileInfo info = this._layoutManager.GetItemInfo(item);
								if (info != null)
									info.IsExpandedWhenMinimized = null;
							}
						}
					}

					this._currentMinimizedExpandedTile = tile;
				}
			}

			// MD 5/14/10 - TFS32080
			// We need to set the State after verifying the IsInMaximizedModeProperty. This is because the State value is coerced based on the 
			// IsInMaximizedModeProperty and so it needs to be updated correctly first.
			tile.ResolveState();

			// JJD 10/11/11 - TFS91173
			// Only bump the layout if we are at the end of processing the original tile whose state we were changing.
			// This avoids triggering synchronous state changes of other tiles since this routine can be re-entered
			// if e.g. the MaximizedTileLimit was exceeded above and another tile was un-maximized which would
			// cause this method to be entered for that tile.
			if (bumpLayoutVersion && _settingStateOfTile == tile)
				this.BumpLayoutVersion();

			//Note: We don't have to raise the TileStateChanged event here since that will get 
			//      raised inside the Tile.OnStateChanged method
			return true;
		}

		#endregion //ChangeTileState

		#region CloseTile

		internal bool CloseTile(XamTile tile)
		{
			TileCloseAction action = tile.CloseActionResolved;

			if (action == TileCloseAction.DoNothing)
				return false;

			object item = this.ItemFromTile(tile);

			TileClosingEventArgs args = new TileClosingEventArgs(tile, item);

			// raise the before event
			this.OnTileClosing(args);

			if (args.Cancel == true)
				return false;

			TileState oldState = tile.State;
			tile.IsClosed = true;

			if (tile.State == TileState.Maximized)
			{
				int index = this.GetIndexInMaximizedCollection(tile);

				//Debug.Assert(index >= 0, "Index not found in MaximizedItems collection");

				if (index >= 0)
				{
					this._maximizedItems.RemoveAt(index);

					
					
					this.VerifyIsInMaximizedModeProperty();

					this.BumpLayoutVersion();
				}
			}

			// try to remove the item if TileCloseAction is set to RemoveItem
			if (action == TileCloseAction.RemoveItem)
				((ObservableItemCollection)(this.Items)).TryRemove(item);

			// raise the after event
			this.OnTileClosed(new TileClosedEventArgs(tile, item));

			return true;
		}

		#endregion //CloseTile	
    
		#region ExecuteCommandImpl

		internal void ExecuteTileCommandImpl(XamTile tile, TileCommandType command, object parameter)
		{
			TileState oldState = tile.State;

			switch (command)
			{
				case TileCommandType.Close:
					this.CloseTile(tile);
					break;

				case TileCommandType.ToggleMaximized:
					{
						TileState newState = TileState.Normal;
						if (oldState == TileState.Maximized)
						{
							if (this._maximizedItems.Count == 1 &&
								this._maximizedItems[0] == this.ItemFromTile(tile))
								newState = TileState.Normal;
							else if (this._maximizedItems.Count == 0)
								newState = TileState.Normal;
							else if (tile.IsExpandedWhenMinimizedResolved)
								newState = TileState.MinimizedExpanded;
							else
								newState = TileState.Minimized;
						}
						else
						{
							newState = TileState.Maximized;
						}

						this.ChangeTileState(tile, newState);

						break;

					}

				case TileCommandType.ToggleMinimizedExpansion:
					{
						switch (oldState)
						{
							case TileState.Minimized:
								this.ChangeTileState(tile, TileState.MinimizedExpanded);
								break;

							case TileState.MinimizedExpanded:
								this.ChangeTileState(tile, TileState.Minimized);
								break;

							default:
								if (tile.IsExpandedWhenMinimized.HasValue)
									tile.IsExpandedWhenMinimized = null;
								else
									tile.IsExpandedWhenMinimized = !TileUtilities.GetIsExpandedWhenMinimizedHelper(this, tile, null, tile.IsExpandedWhenMinimized);
								break;

						}

						break;
					}
			}
		}

		#endregion //ExecuteCommandImpl

		// JJD 02/22/12 - TFS100150 - Added touch support for scrolling
		#region GetFirstItemHeight

		internal double GetFirstItemHeight()
		{
			XamTile tile = GetFirstScrollableTileInView();

			if (tile != null)
			{
				double height = tile.ActualHeight;

				if (height == 0d)
					height = tile.DesiredSize.Height;

				return height;
			}

			return 50;
		}

		#endregion //GetFirstItemHeight	
    
		// JJD 02/22/12 - TFS100150 - Added touch support for scrolling
		#region GetFirstItemWidth

		internal double GetFirstItemWidth()
		{
			XamTile tile = GetFirstScrollableTileInView();

			if (tile != null)
			{
				double width = tile.ActualWidth;

				if (width == 0d)
					width = tile.DesiredSize.Width;
				return width;
			}

			return 50;
		}

		#endregion //GetFirstItemWidth	
    
		#region GetItemInfoFromContainer

		internal ItemTileInfo GetItemInfoFromContainer(XamTile tile)
		{
			if (tile == null)
				return null;

			object item = this.ItemFromTile(tile);

			if ( item != null )
				return this.GetItemInfo(item);

			object dataContext = tile.DataContext;

			// If the DatContext wasn't set then assume the tile is the item
			return this.GetItemInfo(dataContext != null ? dataContext : tile);
		}

		#endregion //GetItemInfoFromContainer	
    
		#region GetItemFromSerializationId

		internal object GetItemFromSerializationId(string serializationId)
		{
			if (string.IsNullOrEmpty(serializationId))
				return null;

			if (this._itemSerializationMap == null)
			{
				this._itemSerializationMap = new Dictionary<string, object>();

				foreach (object item in this.Items)
				{
					XamTile tile = TileFromItem(item);

					string serId = GetSerializationIdFromItem(tile, item);
					if (!string.IsNullOrEmpty(serId))
						this._itemSerializationMap.Add(serId, item);
				}
			}

			object rtnValue;

			this._itemSerializationMap.TryGetValue(serializationId, out rtnValue);

			return rtnValue;
		}

		#endregion //GetItemFromSerializationId	

		#region GetRepositionAnimation

		internal Timeline GetRepositionAnimation()
		{
			if (this.IsInMaximizedMode)
				return this.MaximizedModeSettingsSafe.RepositionAnimation;
			else
				return this.NormalModeSettingsSafe.RepositionAnimation;
		}

		#endregion //GetRepositionAnimation

		#region GetResizeAnimation

		internal Timeline GetResizeAnimation()
		{
			if (this.IsInMaximizedMode)
				return this.MaximizedModeSettingsSafe.ResizeAnimation;
			else
				return this.NormalModeSettingsSafe.ResizeAnimation;
		}

		#endregion //GetResizeAnimation	

		#region GetSerializationIdFromItem

		internal string GetSerializationIdFromItem(XamTile tile, object item)
		{
			string serializationId = null;

			if (tile != null)
			{
				serializationId = tile.GetValue(SerializationIdProperty) as string;

				if (string.IsNullOrEmpty(serializationId))
					serializationId = tile.Name;
			}
			else
			{
				string path = this.SerializationIdPath;
				if (!string.IsNullOrEmpty(path))
				{

					// create a temp object to bind to

					DependencyObject dpo = new DependencyObject();




					TileUtilities.BindPathProperty(this, item, dpo, XamTileManager.SerializationIdPathProperty,
															SerializationIdProperty);

					serializationId = dpo.GetValue(SerializationIdProperty) as string;
				}
				else
				{
					DependencyObject dpo = item as DependencyObject;

					if (dpo != null)
					{
						serializationId = dpo.GetValue(SerializationIdProperty) as string;
						if (string.IsNullOrEmpty(serializationId))
						{
							FrameworkElement fe = dpo as FrameworkElement;

							if (fe != null)
								serializationId = fe.Name;
						}
					}
				}
			}

			return serializationId;
		}

		#endregion //GetSerializationIdFromItem

		// JJD 03/27/12 - TFS106851 - added
		#region GetSynchonizeDimensions

		internal void GetSynchonizeDimensions(out bool synchronizeWidth, out bool synchronizeHeight)
		{
			synchronizeWidth = false;
			synchronizeHeight = false;

			if (this.IsInMaximizedMode)
				return;

			NormalModeSettings settings = this.NormalModeSettingsSafe;

			if (settings.AllowTileSizing != AllowTileSizing.Synchronized)
				return;

			switch (settings.TileLayoutOrder)
			{
				case TileLayoutOrder.Horizontal:
				case TileLayoutOrder.Vertical:
					synchronizeWidth = true;
					synchronizeHeight = true;
					break;
				case TileLayoutOrder.HorizontalVariable:
					synchronizeHeight = true;
					break;
				case TileLayoutOrder.VerticalVariable:
					synchronizeWidth = true;
					break;
			}
		}

		#endregion //GetSynchonizeDimensions	
    
		#region InitializeTileStyle

		// applies the ItemContainerStyle to tiles
		internal void InitializeTileStyle(XamTile tile)
		{
			if (tile != null)
			{
				bool isStyleAlereadySet = (bool)tile.GetValue(IsStyleAlreadySetProperty);

				if (true == isStyleAlereadySet || DependencyProperty.UnsetValue == tile.ReadLocalValue(FrameworkElement.StyleProperty))
				{
					Style style = this.ItemContainerStyle;

					if (style != null)
					{
						// make sure the style's type is appropriate
						if (style.TargetType != null &&
							 !style.TargetType.IsInstanceOfType(tile))
							throw new InvalidOperationException(TileUtilities.GetString("LE_InvalidStyleTargetType", style.TargetType, tile.GetType()));

						tile.SetValue(FrameworkElement.StyleProperty, style);
					}
					else
						tile.ClearValue(FrameworkElement.StyleProperty);

					tile.SetValue(IsStyleAlreadySetProperty, KnownBoxes.FromValue(style != null));
				}
			}
		}

		#endregion //InitializeTileStyle	
    
		#region ItemFromTile

		internal object ItemFromTile(XamTile tile)
		{
			object item = this._layoutManager.GetItemFromContainer(tile);

			if (item == null)
			{
				if (this.Items.Contains(tile))
					item = tile;
			}

			return item;
		}

		#endregion //ItemFromTile	

		#region OnContainersSwapped



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		internal void OnContainersSwapped(FrameworkElement dragItemContainer, ItemTileInfo dragItemInfo, FrameworkElement swapTargetContainer, ItemTileInfo swapTargetItemInfo)
		{
			ItemTileInfo sourceInfo = dragItemInfo;
			ItemTileInfo targetInfo = swapTargetItemInfo;

			Debug.Assert(sourceInfo != null);
			Debug.Assert(targetInfo != null);

			if (sourceInfo == null ||
				 targetInfo == null)
				return;

			// if the IsMaximized states are different we need to do an atomic swap of 
			// the 
			if (sourceInfo.IsMaximized != targetInfo.IsMaximized)
			{
				XamTile maximizedTile;
				XamTile minimizedTile;
				ItemTileInfo maximizedInfo;
				ItemTileInfo minimzedInfo;

				if (sourceInfo.IsMaximized)
				{
					maximizedTile = dragItemContainer as XamTile;
					minimizedTile = swapTargetContainer as XamTile;
					maximizedInfo = sourceInfo;
					minimzedInfo = targetInfo;
				}
				else
				{
					maximizedTile = swapTargetContainer as XamTile;
					minimizedTile = dragItemContainer as XamTile;
					maximizedInfo = targetInfo;
					minimzedInfo = sourceInfo;
				}

				if (maximizedTile == null ||
					 minimizedTile == null)
					return;

				int maximizedIndex = this._maximizedItems.IndexOf(maximizedInfo.Item);

				Debug.Assert(maximizedIndex >= 0, "Maximized item must be in the collection");

				if (maximizedIndex < 0)
					return;

				this._settingStateOfTile = maximizedTile;

				try
				{
					this._maximizedItems.BeginUpdate();
					this._maximizedItems[maximizedIndex] = minimzedInfo.Item;

					maximizedTile.IsMaximized = false;
					minimizedTile.IsMaximized = true;

					// JJD 11/1/11 - TFS88171 
					// Only swap the isexpandedWhenMinimized setting if _swapIsExpandedWhenMinimized is true
					if (_swapinfo._swapIsExpandedWhenMinimized)
					{
						// JJD 11/1/11 - TFS88171
 						// Swap both tile settings
						bool? temp = minimizedTile.IsExpandedWhenMinimized;
						minimizedTile.IsExpandedWhenMinimized = maximizedTile.IsExpandedWhenMinimized;
						maximizedTile.IsExpandedWhenMinimized = temp;
					}

					maximizedTile.ResolveState();
					minimizedTile.ResolveState();

					maximizedInfo.SyncFromTileState(maximizedTile);
					minimzedInfo.SyncFromTileState(minimizedTile);
				}
				finally
				{
					this._settingStateOfTile = null;
					this._maximizedItems.EndUpdate();
				}
			}
			else
			{
				// JJD 11/1/11 - TFS88171 
				// If _swapIsExpandedWhenMinimized was explicitly set in the event args then swap the isexpandedWhenMinimized settings
				if (_swapinfo._swapIsExpandedWhenMinimized)
				{
					sourceInfo.IsExpandedWhenMinimized = _swapinfo._targetIsExpandedWhenMinimized;
					targetInfo.IsExpandedWhenMinimized = _swapinfo._sourceIsExpandedWhenMinimized;
				}
			}

			// JJD 11/1/11 - TFS88171 
			// clear the cached swap info
			_swapinfo = null;

			TileSwappedEventArgs args = new TileSwappedEventArgs(dragItemContainer as XamTile, dragItemInfo.Item, swapTargetContainer as XamTile, swapTargetItemInfo.Item);

			// raise the TileSwapped event
			this.OnTileSwapped(args);
		}

		#endregion //OnContainersSwapped
		
		#region OnItemIsClosedChanged

		internal void OnItemIsClosedChanged(object item, bool isClosed, bool isMaximized)
		{
			bool alreadyInList = this._maximizedItems.Contains(item);

			if (alreadyInList && isClosed)
			{
				// when the item is closed we need to remove it from the _maximizedItems collection
				this._maximizedItems.Remove(item);

				if (this._maximizedItems.Count == 0)
				{
					this.VerifyIsInMaximizedModeProperty();
					this.BumpLayoutVersion();
				}
			}
			else
				if (isMaximized == true &&
					alreadyInList == false &&
					isClosed == false)
				{
					
					// When the item is un-closed and it is maximized it needs to
					// added back into the _maximizedItems collection
					this._maximizedItems.Add(item);

					if (this._maximizedItems.Count == 1)
					{
						this.VerifyIsInMaximizedModeProperty();
						this.BumpLayoutVersion();
					}
				}

			// JJD 4/22/11 - TFS58637
			// When an item is being unclosed and it isn'r maximized there is
			// no guarantee that it will be included in the next arrange pass
			// so we need to arrange it out of view
			if (isClosed == false && isMaximized == false)
			{
				XamTile tile = this.TileFromItem(item);

				if (tile != null)
				{
					if (tile.Visibility != Visibility.Collapsed &&
						tile.DesiredSize.Width > 0 &&
						tile.DesiredSize.Height > 0)
					{
						tile.Arrange(new Rect(new Point(-100000, -100000), tile.DesiredSize));
					}
				}
			}
		}

		#endregion //OnItemIsClosedChanged

		#region OnTileIsClosedChanged

		internal void OnTileIsClosedChanged(XamTile tile)
		{
			object item = ItemFromTile(tile);
			if (item == null)
				return;

			if (tile.State == TileState.Maximized)
			{
				if (tile.IsClosed)
				{
					// when we collapse the tile we take a snapshot of the
					// current max items list using a WeakList. This is 
					// in an effort to maintain the items relative positioning
					// when it gets un-closed later
					if (tile.CloseActionResolved == TileCloseAction.CollapseTile)
					{
						if (this._maximizedItemsSnapshot == null)
						{
							this._maximizedItemsSnapshot = new WeakList<object>();
							this._maximizedItemsSnapshot.AddRange(_maximizedItems);
						}
						else
						{
							// if the item isn't already in the snapshot list then append it
							if (!this._maximizedItemsSnapshot.Contains(item))
								this._maximizedItemsSnapshot.Add(item);
						}
					}
				}
				else
				{
					if (!this._maximizedItems.Contains(item))
					{
						int insertAt = -1;

						// try to determine where we should re-sinsert this item using the 
						// snapshooted list
						if (this._maximizedItemsSnapshot != null)
						{
							this._maximizedItemsSnapshot.Compact();

							// get the index of the item in the snapshot we took when the item was collapsed
							int oldIndex = this._maximizedItemsSnapshot.IndexOf(item);

							if (oldIndex >= 0)
							{
								// walk over the snapshoted items backwards from the previous item
								// to see if any prior item is still in the current maximized list
								for (int i = oldIndex - 1; i >= 0; i--)
								{
									insertAt = _maximizedItems.IndexOf(_maximizedItemsSnapshot[i]);

									if (insertAt >= 0)
									{
										// since this previous item is still in the maximized list
										// we want to re-insert the item at the slot immediately after it
										insertAt++;
										break;
									}
								}

								if (insertAt < 0)
								{
									// walk over the snapshoted items forwards from the next item
									// to see if any trailing item is still in the current maximized list
									for (int i = oldIndex + 1; i < _maximizedItemsSnapshot.Count; i++)
									{
										insertAt = _maximizedItems.IndexOf(_maximizedItemsSnapshot[i]);

										if (insertAt >= 0)
											break;
									}
								}
							}
						}
						int limit = this.MaximizedTileLimit;

						// make sure we insert the item at an index that won't cause it to be beyond the
						// max tile limit and therefore be truncated  by the call to VerifyMaximizedTileLimit
						// below
						insertAt = Math.Min(insertAt, limit - 1);

						if (insertAt >= 0 && insertAt < _maximizedItems.Count)
							this._maximizedItems.Insert(insertAt, item);
						else
							this._maximizedItems.Add(item);

						// walk over the snapshotted list to make sure it is still needed
						if (this._maximizedItemsSnapshot != null)
						{
							bool allItemsFound = true;

							foreach (object obj in _maximizedItemsSnapshot)
							{
								if (!_maximizedItems.Contains(obj))
								{
									allItemsFound = false;
									break;
								}
							}

							// if all items were found in the current maximized items list
							// we can null out the snapshot
							if (allItemsFound)
								_maximizedItemsSnapshot = null;
						}

						this.VerifyMaximizedTileLimit(limit);
						this.VerifyIsInMaximizedModeProperty();
						this.BumpLayoutVersion();
					}
				}
			}

			
			// If the tile is being un-closed then invalidate the panel so the tile
			// can get measured and arranged
			if (tile.IsClosed == false)
				this.InvalidateMeasure();
		}

		#endregion //OnTileIsClosedChanged	

		#region OnItemsInViewChanged






		internal void OnItemsInViewChanged()
		{
			this.SyncIsAnimationInProgress();
		}

		#endregion //OnItemsInViewChanged	

		// JJD 03/06/12 - TFS100150 - Added touch support for scrolling
		#region StartDragHelper

		internal bool StartDragHelper(XamTile tile, TileHeaderPresenter header, Point mouseDownLocation, Point point)
		{
			if (_panel == null)
				return false;



#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)


			return _panel.StartDrag(tile, point);
		}

		#endregion //StartDragHelper	
    
		#region SyncItemInfo

		internal void SyncItemInfo(XamTile tile, bool calledFromInitializePanel)
		{
			object item = this.ItemFromTile(tile);
			if (item == null)
			    return;

			ItemTileInfo info = _layoutManager.GetItemInfo(item);

			Debug.Assert(info != null, "ItemInfo not found");

			if (info != null)
			{
			    info.SyncFromTileState(tile);

			    // If the tile is being initialized and
			    // it is maximized then make sure we update the maximized items
			    // collection
			    if (calledFromInitializePanel &&
			        tile.State == TileState.Maximized &&
			       
			        !this._maximizedItems.Contains(item))
			    {
			        this._maximizedItems.Add(item);
			        this.VerifyIsInMaximizedModeProperty();
			        this.BumpLayoutVersion();
			    }
			}

		}

		#endregion //SyncItemInfo

		#region SyncTileFromItemInfo

		internal void SyncTileFromItemInfo(XamTile tile)
		{
			object item = this.ItemFromTile(tile);

			Debug.Assert(item != null, "unknown tile");
			if (item == null)
				return;

			ItemTileInfo info = this._layoutManager.GetItemInfo(item);

			Debug.Assert(info != null, "ItemInfo not found");

			if (info != null)
				tile.SynchStateFromInfo(info);

		}

		#endregion //SyncTileFromItemInfo
		
		#region VerifyIsInMaximizedModeProperty

		internal void VerifyIsInMaximizedModeProperty()
		{
			if (this._maximizedItems.Count == 0)
				this.SetValue(IsInMaximizedModePropertyKey, KnownBoxes.FalseBox);
			else
				this.SetValue(IsInMaximizedModePropertyKey, KnownBoxes.TrueBox);
		}

		#endregion //VerifyIsInMaximizedModeProperty	

		#region VerifyMaximizedItems



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal void VerifyMaximizedItems() 
		{
			int count = this._maximizedItems.Count;

			if (count < 0)
				return;

			bool wereItemsRemoved = false;

			try
			{
				for (int i = count - 1; i >= 0; i--)
				{
					object item = this._maximizedItems[i];

					ItemTileInfo info = this._layoutManager.GetItemInfo(item, false, -1);

					Debug.Assert(info != null, "There should be an info object for all maximized items");

					// if the index is no longer valid then remove the item
					if (info == null ||
						info.Index < 0)
					{
						if (wereItemsRemoved == false)
						{
							wereItemsRemoved = true;
							this._maximizedItems.BeginUpdate();
						}
						this._maximizedItems.RemoveAt(i);
					}
				}
			}
			finally
			{
				if (wereItemsRemoved)
				{
					this._maximizedItems.EndUpdate();

					bool isInMax = this.IsInMaximizedMode;

					
					
					
					
					this.VerifyIsInMaximizedModeProperty();

					
					// if IsInMaximizedMode changed then bump the layout versions
					if (isInMax != this.IsInMaximizedMode)
						this.BumpLayoutVersion();
				}
			}
		}

		#endregion //VerifyMaximizedItems	

		#endregion //Internal Methods	
    
		#region Private Methods

		#region BindPathProperty

		private bool BindPathProperty(object item, XamTile tile, DependencyProperty dpPath, DependencyProperty dpTarget)
		{
			return TileUtilities.BindPathProperty(this, item, tile, dpPath, dpTarget);
		}

		#endregion //BindPathProperty	
		
		#region BindTilePropertyIfSpecified

		private static void BindTilePropertyIfSpecified(DependencyObject item, XamTile tile, DependencyProperty dp)
		{
			if (item == null)
				return;

			object value = item.ReadLocalValue(dp);

			if (DependencyProperty.UnsetValue == value)
				return;

			Binding binding = null;

			if (_CachedBindings == null)
				_CachedBindings = new Dictionary<DependencyProperty, Binding>();
			else
				_CachedBindings.TryGetValue(dp, out binding);

			if (binding == null)
			{
				binding = PresentationUtilities.CreateBinding(
					new BindingPart
					{

						PathParameter = dp



					}
				);
				binding.Mode = BindingMode.OneWay;
				_CachedBindings[dp] = binding;
			}

			tile.SetBinding(dp, binding);
		}

		#endregion //BindTilePropertyIfSpecified	

		#region CalculateResolvedSpacing

		private void CalculateResolvedSpacing()
		{
			MaximizedModeSettings maxModeSettings = this.MaximizedModeSettingsSafe;


			// set _interTileSpacingXMaximizedResolved (default to _interTileSpacingResolved;
			double value = maxModeSettings.InterTileSpacingXMaximized;
			if (double.IsNaN(value))
				value = this.InterTileSpacingX;

			this.SetValue(XamTileManager.InterTileSpacingXMaximizedResolvedPropertyKey, value);



			// set _interTileSpacingXMinimizedResolved (default to _interTileSpacingResolved;
			value = maxModeSettings.InterTileSpacingXMinimized;
			if (double.IsNaN(value))
				value = this.InterTileSpacingX;

			this.SetValue(XamTileManager.InterTileSpacingXMinimizedResolvedPropertyKey, value);


			// set _interTileSpacingYMaximizedResolved (default to _interTileSpacingResolved;
			value = maxModeSettings.InterTileSpacingYMaximized;
			if (double.IsNaN(value))
				value = this.InterTileSpacingY;

			this.SetValue(XamTileManager.InterTileSpacingYMaximizedResolvedPropertyKey, value);



			// set _interTileSpacingYMinimizedResolved (default to _interTileSpacingResolved;
			value = maxModeSettings.InterTileSpacingYMinimized;
			if (double.IsNaN(value))
				value = this.InterTileSpacingY;

			this.SetValue(XamTileManager.InterTileSpacingYMinimizedResolvedPropertyKey, value);


			// set _interTileAreaSpacingResolved (default to _interTileSpacingMaximizedResolved;
			value = maxModeSettings.InterTileAreaSpacing;
			if (double.IsNaN(value))
			{
                switch (maxModeSettings.MaximizedTileLocationResolved)
				{
					case MaximizedTileLocation.Left:
					case MaximizedTileLocation.Right:
						value = this.InterTileSpacingXMaximizedResolved;
						break;
					default:
						value = this.InterTileSpacingYMaximizedResolved;
						break;
				}
			}

			this.SetValue(XamTileManager.InterTileAreaSpacingResolvedPropertyKey, value);

		}

		#endregion //CalculateResolvedSpacing	

		#region GetIndexInMaximizedCollection

		private int GetIndexInMaximizedCollection(XamTile tile)
		{
			if (this._maximizedItems.Count == 0)
				return -1;

			object item = this.ItemFromTile(tile);

			return this._maximizedItems.IndexOf(item);
		}

		#endregion //GetIndexInMaximizedCollection	

		// JJD 02/22/12 - TFS100150 - Added touch support for scrolling
		#region GetFirstScrollableTileInView

		private XamTile GetFirstScrollableTileInView()
		{
			if (_layoutManager == null)
				return null;

			int scrollPos = this._layoutManager.ScrollPosition;

			if (scrollPos < 0)
				return null;

			ItemTileInfo info = _layoutManager.GetItemAtScrollIndex(scrollPos);

			if (info == null)
				return null;

			return this.TileFromItem(info.Item);

		}

		#endregion //GetFirstScrollableTileInView	

		// JJD 02/22/12 - TFS100150 - Added touch support for scrolling
		#region GetScrollValueHelper

		private static double GetScrollValueHelper(double actualValue, ref double? cachedValue)
		{
			if (cachedValue.HasValue)
			{
				
					return cachedValue.Value;

				
			}

			return actualValue;
		}

		#endregion //GetScrollValueHelper	
        
        #region InitializeRowColumnAttribute

        private static void InitializeRowColumnAttribute(DependencyProperty dp, XamTile tile, DependencyObject item, object value)
        {
            Debug.Assert(value != null, "Value should not be null");
            
            if (value == null)
                return;

            if (tile != null)
                tile.SetValue(dp, value);
			else if (item != null)
                item.SetValue(dp, value);
        }

        #endregion //InitializeRowColumnAttribute

		#region InitializeSpacingOnResizeController

		private void InitializeSpacingOnResizeController()
		{
			this._resizeController.InterItemSpacingX = this.InterTileSpacingX;
			this._resizeController.InterItemSpacingY = this.InterTileSpacingY;
		}

		#endregion //InitializeSpacingOnResizeController	

		#region InitializeSplitter

		private void InitializeSplitter()
		{
			if (this._panel == null || !this._isInitialized)
				return;
			
			// if we are in the middle of measuring the panel 
			// delay initializing the splitter until after it completes
			if (_panel.IsInMeasure)
			{



				this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(this.InitializeSplitter));

				return;
			}

			MaximizedModeSettings settings = this._panel.MaximizedModeSettingsSafe;

			if (this.IsInMaximizedMode && settings.ShowTileAreaSplitter)
			{
				if (this._splitter == null)
				{
					this._splitter = new TileAreaSplitter(this);
					this._panel.AddExtraChild(this._splitter);
				}

				Orientation orientation;
                switch (settings.MaximizedTileLocationResolved)
				{
					case MaximizedTileLocation.Bottom:
					case MaximizedTileLocation.Top:
						orientation = Orientation.Horizontal;
						break;
					default:
						orientation = Orientation.Vertical;
						break;
				}

				TileAreaSplitter.SetOrientation(_splitter, orientation);

				// JJD 1/6/12 - TFS98941
				// Moved logic into InitializeSplitterMinExtent
				this.InitializeSplitterMinExtent();

				if (_panel != null)
					_panel.InvalidateArrange();
			}
			else
			{
				if (this._splitter != null)
				{
					TileAreaSplitter splitter = this._splitter;
					this._splitter = null;
					this._splitterMinExtent = 0;

					if (_panel != null)
						_panel.RemoveExtraChild(splitter);
				}

			}
		}

		#endregion //InitializeSplitter	

		// JJD 1/6/12 - TFS98941 - added
		#region InitializeSplitterMinExtent

		private void InitializeSplitterMinExtent()
		{
			if (_splitter == null)
				return;

			if (this._splitterMinExtent < 1)
			{
				this._splitter.InvalidateMeasure();

				this._splitter.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

				Size desiredSize = this._splitter.DesiredSize;

				Orientation orientation = TileAreaSplitter.GetOrientation(_splitter);

				if (orientation == Orientation.Vertical)
					this._splitterMinExtent = this._splitter.DesiredSize.Width;
				else
					this._splitterMinExtent = this._splitter.DesiredSize.Height;
			}
		}

		#endregion //InitializeSplitterMinExtent	
            
		#region InitializeTileContainer

		private void InitializeTileContainer(object item, XamTile tile)
		{
			ItemTileInfo info = this.GetItemInfo(item);

			if (tile != item)
			{
				tile.WireOwner();

				tile.Content = item;
				tile.DataContext = item;

				Style style = this.ItemContainerStyle;

				if (style != null)
					tile.Style = style;

				DataTemplate template = this.ItemTemplate;

				if (template != null)
					tile.ContentTemplate = template;

				template = this.ItemTemplateMaximized;

				if (template != null)
					tile.ContentTemplateMaximized = template;

				template = this.ItemTemplateMinimized;

				if (template != null)
					tile.ContentTemplateMinimized = template;

				template = this.ItemTemplateMinimizedExpanded;

				if (template != null)
					tile.ContentTemplateMinimizedExpanded = template;

				template = this.ItemHeaderTemplate;

				if (template != null)
					tile.HeaderTemplate = template;






				// bind the Header property
				this.BindPathProperty(item,
										tile,
										XamTileManager.HeaderPathProperty,
										XamTile.HeaderProperty);

				// bind the SerializationId property
				bool isSerializationIdBound = this.BindPathProperty(item,
																	tile,
																	XamTileManager.SerializationIdPathProperty,
																	XamTileManager.SerializationIdProperty);


				DependencyObject d = item as DependencyObject;

				// initialize any attached properties
				BindTilePropertyIfSpecified(d, tile, XamTileManager.ColumnProperty);
				BindTilePropertyIfSpecified(d, tile, XamTileManager.ColumnSpanProperty);
				BindTilePropertyIfSpecified(d, tile, XamTileManager.ColumnWeightProperty);
				BindTilePropertyIfSpecified(d, tile, XamTileManager.ConstraintsProperty);
				BindTilePropertyIfSpecified(d, tile, XamTileManager.ConstraintsMaximizedProperty);
				BindTilePropertyIfSpecified(d, tile, XamTileManager.ConstraintsMinimizedProperty);
				BindTilePropertyIfSpecified(d, tile, XamTileManager.ConstraintsMinimizedExpandedProperty);
				BindTilePropertyIfSpecified(d, tile, XamTileManager.RowProperty);
				BindTilePropertyIfSpecified(d, tile, XamTileManager.RowSpanProperty);
				BindTilePropertyIfSpecified(d, tile, XamTileManager.RowWeightProperty);

				if (!isSerializationIdBound)
					BindTilePropertyIfSpecified(d, tile, XamTileManager.SerializationIdProperty);

				if (info != null)
				{
					// see if there was a peer create for this element
					AutomationPeer tmPeer = FrameworkElementAutomationPeer.FromElement(this);

					if (tmPeer != null)
					{
						XamTileAutomationPeer tilePeer = FrameworkElementAutomationPeer.CreatePeerForElement(tile) as XamTileAutomationPeer;

						if (tilePeer != null)
						{
							TileItemAutomationPeer itemPeer = info.GetAutomationPeer();
							TileItemAutomationPeer oldItemPeer = tilePeer.EventsSource as TileItemAutomationPeer;

							if (oldItemPeer == null || itemPeer != oldItemPeer)
							{
								if (oldItemPeer != null)
								{
									oldItemPeer.InvalidatePeer();
									oldItemPeer.RaiseAutomationEvent(AutomationEvents.StructureChanged);
									tilePeer.InvalidatePeer();
									AutomationPeerHelper.InvalidateChildren(tilePeer);
									tilePeer.RaiseAutomationEvent(AutomationEvents.StructureChanged);
								}

								itemPeer.InvalidatePeer();
								AutomationPeerHelper.InvalidateChildren(itemPeer);
								itemPeer.RaiseAutomationEvent(AutomationEvents.StructureChanged);

								tilePeer.EventsSource = itemPeer;
							}
						}
					}
				}
			}

			if (info != null)
				tile.SynchStateFromInfo(info);
		}

		#endregion //InitializeTileContainer	
		
		// JJD 02/24/12 - TFS101202 - added
		#region InvalidatePanelAsync

		private void InvalidatePanelAsync()
		{
			_panelInvalidationPending = false;
			if (_panel != null)
			{
				// first make sure the last panel measure was processed
				_panel.UpdateLayout();
				_panel.InvalidateMeasure();
			}
		}

		#endregion //InvalidatePanelAsync	
    
		#region Load

		private void Load(object data)
		{
			CoreUtilities.ValidateNotNull(data, "data");

			XamTileManagerPersistenceInfo tmpi = data as XamTileManagerPersistenceInfo;

			if ( tmpi == null )
				throw new InvalidOperationException(TileUtilities.GetString("LE_LoadDataInvalid"));

			if ( this.IsLoadingLayout )
				throw new InvalidOperationException(TileUtilities.GetString("LE_LoadLayoutInProgress"));

			// JJD 12/2/11 - TFS97014
			// load splitter position info 
			this.MinimizedAreaExplicitExtentX = tmpi.MinimizedAreaExtentX;
			this.MinimizedAreaExplicitExtentY = tmpi.MinimizedAreaExtentY;

			ObservableCollectionExtended<object> maximizedItems = this.MaximizedItemsInternal;

			List<ItemTileInfo> existingMaximizedItems = new List<ItemTileInfo>();

			foreach (object item in maximizedItems)
			{
				ItemTileInfo info = this.GetItemInfo(item);

				
				// Make sure an ItemInfo object was returned
				if (info != null)
					existingMaximizedItems.Add(info);
			}

			List<ItemTileInfo> newMaximizedItems = new List<ItemTileInfo>();

			// call BeginUpdate so we don't respond to notifications during the load process
			//            maximizedItems.BeginUpdate();

			this.IsLoadingLayout = true;

			// JJD 06/04/12 - TFS112448
			// As we de-serailaize each item keep track of the last one that was expanded but not maximized.
			ItemTileInfo lastExpandedItemInfo = null;

			try
			{
				Size? synchronizedItemSize = null;
				if (tmpi.SynchoronizedTileWidth > 0 ||
					tmpi.SynchoronizedTileHeight > 0)
					synchronizedItemSize = new Size(tmpi.SynchoronizedTileWidth, tmpi.SynchoronizedTileHeight);

				this._layoutManager.SynchronizedItemSize = synchronizedItemSize;

				#region Items

				foreach (TileItemPersistenceInfo itemPersistInfo in tmpi.Items)
				{
					// prepare the item
					string serializationId = itemPersistInfo.SerializationId;

					LoadingItemMappingEventArgs args = new LoadingItemMappingEventArgs(this, serializationId);

					this.RaiseLoadingItemMapping(args);
					object item = args.Item;

					if (item == null)
						continue;

					ItemTileInfo info = this.GetItemInfo(item);

					if (info == null)
						continue;

					info.IsClosed = itemPersistInfo.IsClosed;
					info.IsExpandedWhenMinimized = itemPersistInfo.IsExpandedWhenMinimized;

					
					
					
					
					bool wasSerializedAsMaximized = itemPersistInfo.IsMaximized;

					
					
					
					if (wasSerializedAsMaximized)
					{
						// add it to the new list of maximized items
						newMaximizedItems.Add(info);

						
#region Infragistics Source Cleanup (Region)










#endregion // Infragistics Source Cleanup (Region)


						// if it is in the existing list then remove it
						int oldIndex = existingMaximizedItems.IndexOf(info);

						if (oldIndex >= 0)
							existingMaximizedItems.RemoveAt(oldIndex);
					}
					else
					{
						bool? isExpandedWhenMinimized = itemPersistInfo.IsExpandedWhenMinimized;

						// JJD 06/04/12 - TFS112448
						// As we de-serailaize each item keep track of the last one that was expanded but not maximized.
						if (isExpandedWhenMinimized.HasValue && true == isExpandedWhenMinimized.Value)
							lastExpandedItemInfo = info;
					}

					if (tmpi.IsExplicitLayout)
					{
						XamTile tile = this.TileFromItem(item);
						DependencyObject dpo = item as DependencyObject;

						InitializeRowColumnAttribute(XamTileManager.ColumnProperty, tile, dpo, itemPersistInfo.Column);
						InitializeRowColumnAttribute(XamTileManager.ColumnSpanProperty, tile, dpo, itemPersistInfo.ColumnSpan);
						InitializeRowColumnAttribute(XamTileManager.ColumnWeightProperty, tile, dpo, itemPersistInfo.ColumnWeight);
						InitializeRowColumnAttribute(XamTileManager.RowProperty, tile, dpo, itemPersistInfo.Row);
						InitializeRowColumnAttribute(XamTileManager.RowSpanProperty, tile, dpo, itemPersistInfo.RowSpan);
						InitializeRowColumnAttribute(XamTileManager.RowWeightProperty, tile, dpo, itemPersistInfo.RowWeight);
					}

					info.SerializationInfo = itemPersistInfo;

					double preferredWidthOverride = itemPersistInfo.PreferredWidthOverride;
					double preferredHeightOverride = itemPersistInfo.PreferredHeightOverride;

					Size? sizeOverride = null;
					if (preferredWidthOverride > 0 ||
							preferredHeightOverride > 0)
						sizeOverride = new Size(preferredWidthOverride, preferredHeightOverride);

					info.SizeOverride = sizeOverride;
				}
				#endregion //Items

				// un-maximize any existing maximized guys that are left
				foreach (ItemTileInfo info in existingMaximizedItems)
				{
					info.IsMaximized = false;

					XamTile tile = this.TileFromItem(info.Item);

					if (tile != null)
						tile.SynchStateFromInfo(info);

					this.BumpLayoutVersion();
				}

				maximizedItems.BeginUpdate();

				// clear the maximized items collection
				maximizedItems.Clear();

				// sort the new maximized items into the proper order
				if (newMaximizedItems.Count > 1)
					newMaximizedItems.Sort(new MaximizedItemInfoComparer());

				
				// Keep a count so we don't exceed the MaximizedTileLimit
				int limit = this.MaximizedTileLimit;
				int countOfNewMaximizedItems = 0;

				// add in the new maximized items which will ensure that
				// they are in the proper order
				foreach (ItemTileInfo info in newMaximizedItems)
				{
					// JJD 4/22/11 - TFS58637
					// If the item is closed then just set the IsMaximized flag and return
					if (info.IsClosed)
					{
						info.IsMaximized = true;
						continue;
					}

					
					// See if we have reached the MaximizedTileLimit
					if (countOfNewMaximizedItems == limit)
					{
						// If the item was maximized then set its IsMaxmized
						// property to false and fall thru to sync up the 
						// tiles state below.
						// Otherwise, just continue
						if (info.IsMaximized)
							info.IsMaximized = false;
						else
							continue;
					}
					else
					{
						// add the item to the collection
						maximizedItems.Add(info.Item);

						
						// set its IsMaxmized state to true
						info.SetIsMaximizedInternal( true, false);

						
						// Bump the count 
						countOfNewMaximizedItems++;
					}

					
					// Synchronize the Tile's state from the info object
					XamTile tile = this.TileFromItem(info.Item);

					if (tile != null)
						tile.SynchStateFromInfo(info);
				}

			}
			finally
			{
				this.IsLoadingLayout = false;

				// call EndUpdate on the maximized items collection so a notification is sent out
				maximizedItems.EndUpdate();

				this._layoutManager.SortItems(new ItemInfoComparer());

				this.SetValue(IsInMaximizedModePropertyKey, KnownBoxes.FromValue(maximizedItems.Count > 0));

				// JJD 06/04/12 - TFS112448
				// If we are maximized and we are only allowing one minimized expanded tile
				// then set its state here
				if (lastExpandedItemInfo != null &&
					this.IsInMaximizedMode && 
					this.MaximizedModeSettingsSafe.MinimizedTileExpansionMode == MinimizedTileExpansionMode.AllowOne)
				{
					XamTile tile = this.TileFromItem(lastExpandedItemInfo.Item);

					if (tile == null)
					{
						// bring the tile for this item into view
						this._layoutManager.BringIndexIntoView(lastExpandedItemInfo.LogicalIndex);

						// re-try to get the tile
						tile = this.TileFromItem(lastExpandedItemInfo.Item);

						Debug.Assert(tile != null, "We should have a tile at this point");
					}

					if (tile != null)
					{
						if (tile.State == TileState.Minimized)
							this.ChangeTileState(tile, TileState.MinimizedExpanded);
					}
					else
					{
						// since we don't have a tile that is expanded we should at least collapse the
						// one that is currently expanded
						if (_currentMinimizedExpandedTile != null)
							this.ChangeTileState(_currentMinimizedExpandedTile, TileState.Minimized);

					}
				}

				
				// Bump the version to sync up all tile states
				this.BumpLayoutVersion();

				this.InvalidateMeasure();

			}
		}

		#endregion //Load	
    		
		#region OnConstraintPropertyChanged

		private static void OnConstraintPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{

			TileAreaPanel panel = null != d ? VisualTreeHelper.GetParent(d) as TileAreaPanel : null;
			if (null != panel)
			{
				panel.InvalidateMeasure();

				// JJD 02/24/12 - TFS101202
				// When there is only one tile in view in normal mode and the contraints are changed
				// there is a timing issue in certain cases and the new constraints don't get applied
				// until the next panel measure. To avoid this we need to re-invaidate the panel's measure
				// asynchronously
				XamTileManager tm = panel.Manager;
				if (tm != null &&
					 false == tm._panelInvalidationPending &&
					 false == tm.IsInMaximizedMode &&
					 tm._layoutManager.ScrollPosition == 0 &&
					 tm._layoutManager.GetItemsInViewCount() == 1 &&
					 tm._layoutManager.ScrollMaxOffset.X == 0 &&
					 tm._layoutManager.ScrollMaxOffset.Y == 0)
				{
					tm._panelInvalidationPending = true;
					tm.Dispatcher.BeginInvoke(new Action(tm.InvalidatePanelAsync));
				}
			}
		}

		#endregion // OnConstraintPropertyChanged

		#region OnItemsChanged

		private void OnItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			// JJD 1/9/12 - TFS98431 (similar to fix for TFS36079 in XamTilesControl)
			// On a Reset notification if we are bound (i.e. the ItemSource property is set) 
			// we need to refreash the DataContext, Content and
			// ContentTemplateSelector properties on each container (tile)
			// The reason for this is that with certain DataSources, e.g. DataView,
			// it is possible that the cell values have changed without individual row
			// notifications being sent out. In this case bindings would not update automatically
			// without the DataContext, Content and ContentTemplateSelectorproperties being refreshed.

			if (e.Action == NotifyCollectionChangedAction.Reset &&
				this.ItemsSource is System.Data.DataView &&
				this._panel != null)
			{
				UIElementCollection children = this._panel.Children;

				for (int i = 0; i < children.Count; i++)
				{
					XamTile tile = children[i] as XamTile;

					if (tile != null)
					{
						object dc = tile.DataContext;
						DataTemplateSelector selector = tile.ContentTemplateSelector;
						object content = tile.Content;

						if (dc != null)
							tile.DataContext = null;

						if (content != null)
							tile.Content = null;

						if (selector != null)
							tile.ContentTemplateSelector = null;

						// if the content is not null and content is the same as the data context
						// then call InitializeTileContainer which will reinitialized all the bindings
						// inclusing the HeaderPath
						if ( content != null && content != tile && content == dc )
							this.InitializeTileContainer(content, tile);
						else
							if (dc != null)
								tile.DataContext = dc;

						//reset the selector if specified
						if (selector != null)
							tile.ContentTemplateSelector = selector;

						if ( content != null && (content == tile || content != dc ))
						    tile.Content = content;
					}
				}
			}


			this.OnItemsChanged(e);
		}

		#endregion //OnItemsChanged	
        
		#region PerformResize

		private void PerformResize(FrameworkElement resizableItem, double deltaX, double deltaY)
		{
			XamTile tile = resizableItem as XamTile;

			if (tile == null)
				return;

			if (this.IsInMaximizedMode == false &&
				 this._panel != null)
			{
				NormalModeSettings settings = this._panel.NormalModeSettingsSafe;
				if (settings.AllowTileSizing == AllowTileSizing.Synchronized)
				{
					bool synchronizeWidth;
					bool synchronizeHeight;

					// JJD 03/27/12 - TFS106851 - Refactored
					// Moved logic into GetSynchonizeDimensions so we can also can it from the automation peer 
					// resize logic
					this.GetSynchonizeDimensions(out synchronizeWidth, out synchronizeHeight);

					// if we area synchronizing either the width or height then
					// set the SynchronizedItemSize on the manager
					
					
					
					
					if ((synchronizeWidth == true && deltaX != 0) ||
						(synchronizeHeight == true && deltaY != 0))
					{
						Size size = new Size(Math.Max(resizableItem.ActualWidth + deltaX, 0), Math.Max(resizableItem.ActualHeight + deltaY, 0));

						this._layoutManager.SynchronizedItemSize = size;
					}

					// if we are synchronizing both dimensions then we can bail out
					// since we don't need to explicitly set a size on the item below
					
					
					
					
					
					if ((synchronizeWidth == true || deltaX == 0) &&
						(synchronizeHeight == true || deltaY == 0))
						return;
				}
			}

			ItemTileInfo info = this.GetItemInfoFromContainer(tile);

			if (info != null)
			{
				info.OnResize(deltaX, deltaY);
			}

		}

		#endregion //PerformResize	

		#region ProcessNewMaximizedTileLimit

		private void ProcessNewMaximizedTileLimit(int limit)
		{
			// JJD 4/19/11 - TFS58732
			// Moved logic to VerifyMaximizedTileLimit
			this.VerifyMaximizedTileLimit(limit);

			
			
			
			this.VerifyIsInMaximizedModeProperty();
			this.BumpLayoutVersion();
		}

		#endregion //ProcessNewMaximizedTileLimit	
    
		#region ProcessScrollIntoView

		private void ProcessScrollIntoView(object item)
		{
			ItemTileInfo info = this.GetItemInfo(item);

			if (info != null)
				info.BringIntoView();
		}

		#endregion //ProcessScrollIntoView	

		#region RemoveMaximizedItemAtIndex

		private void RemoveMaximizedItemAtIndex(int index)
		{
			object item = this._maximizedItems[index];

			XamTile tileToRemove = this.TileFromItem(item);

			// JJD 10/8/10 - TFS37313
			// See if the item is a tile
			if (tileToRemove == null)
				tileToRemove = item as XamTile;

			Debug.Assert(tileToRemove != null, "We should have a tile here");
			this._maximizedItems.RemoveAt(index);

			if (tileToRemove != null)
			{
				tileToRemove.IsMaximized = false;
				tileToRemove.ResolveState();
			}
			else
			{
				ItemTileInfo info = this.GetItemInfo(item);

				if (info != null)
					info.IsMaximized = false;
			}
		}

		#endregion //RemoveMaximizedItemAtIndex	

		#region Save

		private XamTileManagerPersistenceInfo Save()
		{
			XamTileManagerPersistenceInfo tmpi = new XamTileManagerPersistenceInfo();

			NormalModeSettings settings = this.NormalModeSettings;

			tmpi.IsExplicitLayout = settings != null && settings.TileLayoutOrder == TileLayoutOrder.UseExplicitRowColumnOnTile;

			tmpi.MinimizedAreaExtentX = this.MinimizedAreaExplicitExtentX;
			tmpi.MinimizedAreaExtentY = this.MinimizedAreaExplicitExtentY;

			Size? synchronizedItemSize = _layoutManager.SynchronizedItemSize;

			if (synchronizedItemSize.HasValue)
			{
				tmpi.SynchoronizedTileHeight = synchronizedItemSize.Value.Height;
				tmpi.SynchoronizedTileWidth = synchronizedItemSize.Value.Width;
			}

			tmpi.Items = new List<TileItemPersistenceInfo>();

			foreach (object item in this.Items)
			{
				ItemTileInfo info = this.GetItemInfo(item);

				Debug.Assert(info != null, "ItemInfo not found for item");
				if (info == null)
					continue;

				XamTile tile = this.TileFromItem(item);

				// skip tiles that shouldn't be serialized
				if (tile != null && tile.SaveInLayout == false)
					continue;

				SavingItemMappingEventArgs args = new SavingItemMappingEventArgs(tile, this, item);

				this.RaiseSavingItemMapping(args);

				string serializationId = args.SerializationId;

				if (string.IsNullOrEmpty(serializationId))
					continue;

				TileItemPersistenceInfo itemPersistInfo = new TileItemPersistenceInfo();

				tmpi.Items.Add(itemPersistInfo);

				itemPersistInfo.IsClosed = info.IsClosed;
				itemPersistInfo.IsExpandedWhenMinimized = info.IsExpandedWhenMinimized;
				itemPersistInfo.IsMaximized = info.IsMaximized;
				itemPersistInfo.SerializationId = serializationId;

				// If the item is closed it won't be in the MaximizedItems collection
				if (info.IsMaximized && !info.IsClosed)
				{
					itemPersistInfo.IsMaximized = true;

					itemPersistInfo.MaximizedIndex = this.MaximizedItemsInternal.IndexOf(item);

					Debug.Assert(itemPersistInfo.MaximizedIndex >= 0, "item not found in maximized items collection");
				}
				else
				{
					itemPersistInfo.MaximizedIndex = -1;
				}

				itemPersistInfo.LogicalIndex = info.LogicalIndex;

				if (tmpi.IsExplicitLayout)
				{
					DependencyObject dpo = tile != null ? tile : item as DependencyObject;

					itemPersistInfo.Column = XamTileManager.GetColumn(dpo);
					itemPersistInfo.ColumnSpan = XamTileManager.GetColumnSpan(dpo);
					itemPersistInfo.ColumnWeight = XamTileManager.GetColumnWeight(dpo);
					itemPersistInfo.Row = XamTileManager.GetRow(dpo);
					itemPersistInfo.RowSpan = XamTileManager.GetRowSpan(dpo);
					itemPersistInfo.RowWeight = XamTileManager.GetRowWeight(dpo);
				}

				Size? sizeOverride = info.SizeOverride;

				if (sizeOverride.HasValue)
				{
					itemPersistInfo.PreferredHeightOverride = sizeOverride.Value.Height;
					itemPersistInfo.PreferredWidthOverride = sizeOverride.Value.Width;
				}

			}

			return tmpi;
		}

		#endregion //Save	
    
		#region ValidateColumnRow

		private static void ValidateColumnRow(object objVal)
		{
			int val = (int)objVal;
			if ( val < 0 && val != GridBagConstraintConstants.Relative)
				throw new ArgumentOutOfRangeException(TileUtilities.GetString("LE_MustBeZeroPositiveOrRelative"));
		}

		#endregion // ValidateColumnRow

		#region ValidateColumnRowSpan

		private static void ValidateColumnRowSpan(object objVal)
		{
			int val = (int)objVal;
			if ( val < 1 && val != GridBagConstraintConstants.Remainder)
				throw new ArgumentOutOfRangeException(TileUtilities.GetString("LE_CantBeNegative"));
		}

		#endregion // ValidateColumnRowSpan

		#region ValidateColumnRowWeight

		private static void ValidateColumnRowWeight(object objVal)
		{
			float val = (float)objVal;

			if (float.IsNaN(val) || float.IsInfinity(val) || val < 0)
				throw new ArgumentOutOfRangeException(TileUtilities.GetString("LE_CantBeNegativeOrNanOrInfinity"));
		}

		#endregion // ValidateColumnRowWeight

		#region ValidateInterTileSpacing

		private static void ValidateInterTileSpacing(object objVal)
		{
			double val = (double)objVal;

			if (double.IsNaN(val) || double.IsInfinity(val) || val < 0)
				throw new ArgumentOutOfRangeException(TileUtilities.GetString("LE_CantBeNegativeOrNanOrInfinity"));

		}

		#endregion //ValidateInterTileSpacing	
    
		#region ValidateMaximizedTileLimit

		internal static void ValidateMaximizedTileLimit(object objVal)
		{
			int val = (int)objVal;
			if ( val < 0 )
				throw new ArgumentOutOfRangeException(TileUtilities.GetString("LE_CantBeNegative"));
		}

		#endregion // ValidateMaximizedTileLimit
		
		#region VerifyMaximizedTileLimit

		private void VerifyMaximizedTileLimit(int limit)
		{
			if (limit < this.MaximizedItems.Count)
			{
				this._maximizedItems.BeginUpdate();

				while (this._maximizedItems.Count > 0 &&
						this._maximizedItems.Count > limit)
				{
					
					
					// Remove the first item until we get under the limit
					this.RemoveMaximizedItemAtIndex(0);
				}

				this._maximizedItems.EndUpdate();
			}
		}

		#endregion //VerifyMaximizedTileLimit	

		#endregion //Private Methods	
    
		#endregion //Methods	
        
		#region ITypedSupportPropertyChangeNotifications<object,string> Members

		void ITypedSupportPropertyChangeNotifications<object, string>.AddListener(ITypedPropertyChangeListener<object, string> listener, bool useWeakReference)
		{
			this._propChangeListeners.Add(listener, useWeakReference);
		}

		void ITypedSupportPropertyChangeNotifications<object, string>.RemoveListener(ITypedPropertyChangeListener<object, string> listener)
		{
			this._propChangeListeners.Remove(listener);
		}

		#endregion

		#region ILogicalTreeNode Members


		void ILogicalTreeNode.AddLogicalChild(object child)
		{
			_extraLogicalChildren.Add(child);

			base.AddLogicalChild(child);
		}

		void ILogicalTreeNode.RemoveLogicalChild(object child)
		{
			_extraLogicalChildren.Remove(child);

			base.RemoveLogicalChild(child);
		}


		#endregion

		#region IResizeHostMulti Members

		bool IResizeHostMulti.CanResizeInBothDimensions(FrameworkElement resizableItem)
		{
			return ((IResizeHost)this).CanResize(resizableItem, true) &&
					((IResizeHost)this).CanResize(resizableItem, false);
		}

		Cursor IResizeHostMulti.GetMultiResizeCursor(FrameworkElement resizableItem, Cursor cursor)
		{
			return cursor;
		}

		void IResizeHostMulti.ResizeBothDimensions(FrameworkElement resizableItem, double deltaX, double deltaY)
		{
			this.PerformResize(resizableItem, deltaX, deltaY);
		}

		#endregion

		#region IResizeHost Members

		ResizeController IResizeHost.Controller
		{
			get { return _resizeController; }
		}

		FrameworkElement IResizeHost.RootElement
		{
			get { return this; }
		}

		void IResizeHost.AddResizerBar(FrameworkElement resizerBar)
		{
			if ( this._panel != null )
				this._panel.AddExtraChild(resizerBar);
		}

		// JJD 03/08/12 TFS100150 - Added touch support
		#region CanCaptureMouse

		/// <summary>
		/// Determines if the control is in a state to allow mouse capture.
		/// </summary>
		/// <returns>True if the mouse can be captured at this time</returns>
		bool IResizeHost.CanCaptureMouse(MouseButtonEventArgs e, FrameworkElement itemToResize, bool? resizeInXAxis)
		{


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

			return true;
		}

		#endregion //CanCaptureMouse	
 
		bool IResizeHost.CanResize(FrameworkElement resizableItem, bool resizeInXAxis)
		{
			// Only allow resizing if we are not in the middle of a drag operation
			if (this._panel == null || _panel.IsDragging)
				return false;

			if (this.IsInMaximizedMode)
				return false;

			XamTile tile = resizableItem as XamTile;

			if (tile == null)
				return false;

			if (this._panel.NormalModeSettingsSafe.AllowTileSizing == AllowTileSizing.No)
				return false;

			return true;
		}

		FrameworkElement IResizeHost.GetResizeAreaForItem(FrameworkElement resizableItem)
		{
			// Only return an area if we are not in the middle of a drag operation
			if (_panel != null && false == _panel.IsDragging)
				return _panel;

			return this;
		}

		void IResizeHost.RemoveResizerBar(FrameworkElement resizerBar)
		{
			if (this._panel != null)
				this._panel.RemoveExtraChild(resizerBar);
		}

		void IResizeHost.Resize(FrameworkElement resizableItem, bool resizeInXAxis, double delta)
		{
			if (resizeInXAxis)
				this.PerformResize(resizableItem, delta, 0);
			else
				this.PerformResize(resizableItem, 0, delta);
		}

		Cursor IResizeHost.GetResizeCursor(FrameworkElement resizableItem, bool resizeInXAxis, Cursor cursor)
		{
			return cursor;
		}

		void IResizeHost.InitializeResizeConstraints(FrameworkElement resizeArea, FrameworkElement resizableItem, ResizeConstraints constraints)
		{
			if (this._panel == null)
			{
				constraints.Cancel = true;
				return;
			}

			XamTile tile = resizableItem as XamTile;

			if ( tile == null )
			{
				constraints.Cancel = true;
				return;
			}

			ItemTileInfo info = this.GetItemInfoFromContainer(tile);

			if (info == null)
			{
				constraints.Cancel = true;
				return;
			}

			double maxDeltaLeft, maxDeltaRight, maxDeltaTop, maxDeltaBottom;

			info.GetResizeRange(out maxDeltaLeft, out maxDeltaRight,
								out maxDeltaTop, out maxDeltaBottom);

			double actualExtent;
			double maxDeltaNegative;
			double maxDeltaPositive;

			if (constraints.ResizeInXAxis)
			{
				actualExtent = resizableItem.ActualWidth;
				maxDeltaNegative = maxDeltaLeft;
				maxDeltaPositive = maxDeltaRight;
			}
			else
			{
				actualExtent = resizableItem.ActualHeight;
				maxDeltaNegative = maxDeltaTop;
				maxDeltaPositive = maxDeltaBottom;
			}

			// JJD 03/21/12 - TFS100151 
			#region Get InterTileSpacing

			double interTileSpacing = 0;

			// JJD 03/21/12 - TFS100151
			// Since the maxDelta... values returned above take into account inter-item spacing
			// we need to adjust for that when we caclculate the MinExtent below.
			// If we are in maximized mode then use the appropriate maxmimized or minimized
			// inter-tile spacing values
			if (this.IsInMaximizedMode)
			{
				if (tile.IsMaximized)
				{
					if (constraints.ResizeInXAxis)
						interTileSpacing = this.InterTileSpacingXMaximizedResolved;
					else
						interTileSpacing = this.InterTileSpacingYMaximizedResolved;
				}
				else
				{
					switch (this.MaximizedModeSettingsSafe.MaximizedTileLocation)
					{
						case MaximizedTileLocation.Bottom:
						case MaximizedTileLocation.Top:
							if (constraints.ResizeInXAxis)
								interTileSpacing = this.InterTileSpacingXMinimizedResolved;
							break;
						case MaximizedTileLocation.Left:
						case MaximizedTileLocation.Right:
							if (!constraints.ResizeInXAxis)
								interTileSpacing = this.InterTileSpacingYMinimizedResolved;
							break;
					}
				}
			}
			else
			{
				// JJD 03/21/12 - TFS100151
				// Sine we aren't in mamimized mode use the normal settings
				if (constraints.ResizeInXAxis)
					interTileSpacing = this.InterTileSpacingX;
				else
					interTileSpacing = this.InterTileSpacingY;
			}

			// JJD 03/21/12 - TFS100151
			// use the greater of the interTileSpacing and the ResizerBarWidth for
			// the calculation of MinExtent below
			interTileSpacing = Math.Max(this._resizeController.ResizerBarWidth, interTileSpacing);

			#endregion //Get InterTileSpacing	
    
			// JJD 03/21/12 - TFS100151
 			// adjust the MinExtent by 1/2 of the intertilespacing in the apprpriate dimension
			//constraints.MinExtent = Math.Max(actualExtent - maxDeltaNegative, constraints.MinExtent);
			constraints.MinExtent = Math.Max(actualExtent - (maxDeltaNegative + (interTileSpacing / 2)), constraints.MinExtent);
			constraints.MaxExtent = actualExtent + maxDeltaPositive;

			constraints.ResizeWhileDragging = false;

			// if we are resizing in the Y dimension make sure we leave enough
			// height to display the header
			if (tile != null && constraints.ResizeInXAxis == false)
			{
				TileHeaderPresenter thp = tile.HeaderPresenter;

				if (thp != null)
				{
					double headerMinExtent = thp.ActualHeight;

					GeneralTransform xform = thp.TransformToVisual(tile);

					double topOffset = xform.Transform(new Point(0, 0)).Y;
					double bottomOffset = tile.ActualHeight - xform.Transform(new Point(0, headerMinExtent)).Y;

					double extraExtent = Math.Max(Math.Min(topOffset, bottomOffset), 0);

					constraints.MinExtent = Math.Max(headerMinExtent + (2 * extraExtent), constraints.MinExtent);
				}
			}
		}

		#endregion

		#region IProvideCustomObjectPersistence Members

		void IProvideCustomObjectPersistence.LoadObject(object data)
		{
			this.Load(data);
		}

		object IProvideCustomObjectPersistence.SaveObject()
		{
			return this.Save();
		}

		#endregion

		// JJD 02/22/12 - TFS100150 - Added touch support for scrolling
		#region ISupportScrollHelper Members



		#region Properties

		#region HorizontalMax

		double ISupportScrollHelper.HorizontalMax
		{
			get
			{
				if (_panel != null)
				{
					TileAreaPanel.ScrollData sd = _panel.ScrollDataInfo;
					if (sd._canHorizontallyScroll)
						return Math.Max( sd._extent.Width - sd._viewport.Width, 0);
				}
				return 0;
			}
		}

		#endregion //HorizontalMax

		#region HorizontalScrollType

		ScrollType ISupportScrollHelper.HorizontalScrollType
		{
			get { return ScrollType.Item; }
		}

		#endregion //HorizontalScrollType

		#region HorizontalValue

		double ISupportScrollHelper.HorizontalValue
		{
			get
			{
				if (_panel != null)
					return GetScrollValueHelper(_panel.ScrollDataInfo._offset.X, ref _cachedHorizValue);

				return 0;
			}
			set
			{
				_cachedHorizValue = value;

				if (_panel != null)
				{
					_panel._bypassAnimations = true;
					_panel.SetHorizontalOffset(value, true);
				}
			}
		}

		#endregion //HorizontalValue

		#region VerticalMax

		double ISupportScrollHelper.VerticalMax
		{
			get
			{
				if (_panel != null)
				{
					TileAreaPanel.ScrollData sd = _panel.ScrollDataInfo;
					if (sd._canVerticallyScroll)
						return Math.Max(sd._extent.Height - sd._viewport.Height, 0);
				}
				return 0;
			}
		}

		#endregion //VerticalMax	
    
		#region VerticalScrollType

		ScrollType ISupportScrollHelper.VerticalScrollType
		{
			get { return ScrollType.Item; }
		}

		#endregion //VerticalScrollType	
    
		#region VerticalValue

		double ISupportScrollHelper.VerticalValue
		{
			get
			{
				if (_panel != null)
					return GetScrollValueHelper(_panel.ScrollDataInfo._offset.Y, ref _cachedVertValue);
				return 0;
			}
			set
			{
				this._cachedVertValue = value;
				if (_panel != null)
				{
					_panel._bypassAnimations = true;
					_panel.SetVerticalOffset(value, true);
				}
			}
		}

		#endregion //VerticalValue	
    
		#endregion //Properties	

		#region Methods

		#region GetFirstItemHeight

		double ISupportScrollHelper.GetFirstItemHeight()
		{
			return this.GetFirstItemHeight();
		}

		#endregion //GetFirstItemHeight

		#region GetFirstItemWidth

		double ISupportScrollHelper.GetFirstItemWidth()
		{
			return this.GetFirstItemWidth();
		}

		#endregion //GetFirstItemWidth

		#region GetScrollModeFromPoint

		TouchScrollMode ISupportScrollHelper.GetScrollModeFromPoint(Point point, UIElement elementDirectlyOver)
		{
			if (!this.IsTouchSupportEnabled)
				return TouchScrollMode.None;

			bool canScrollVertically = ((ISupportScrollHelper)this).VerticalMax > 0 && _layoutManager.ScrollTilesVertically;
			bool canScrollHorizonatally = ((ISupportScrollHelper)this).HorizontalMax > 0 && _layoutManager.ScrollTilesHorizontally;

			if (false == canScrollVertically &&
				 false == canScrollHorizonatally)
				return TouchScrollMode.None;

			if (this.IsInMaximizedMode)
			{
				Rect minTileArea = _layoutManager.GetMaximizedModeTileArea(TileLayoutManager.TileArea.MinimizedTiles, true);

				if (!minTileArea.Contains(point))
					return TouchScrollMode.None;
			}

			TileAreaPanel panel = PresentationUtilities.GetVisualAncestor<TileAreaPanel>(elementDirectlyOver, null, this);

			if (panel == null)
				return TouchScrollMode.None;

			if (canScrollHorizonatally)
				return canScrollVertically ? TouchScrollMode.Both : TouchScrollMode.Horizontal;

			return TouchScrollMode.Vertical;
		}

		#endregion //GetScrollModeFromPoint

		#region InvalidateScrollLayout

		void ISupportScrollHelper.InvalidateScrollLayout()
		{
			if (_panel != null)
			{
				_panel._bypassAnimations = true;
				_panel.InvalidateMeasure();
			}
		}

		#endregion //InvalidateScrollLayout	
    
		void ISupportScrollHelper.OnStateChanged(TouchState newState, TouchState oldState)
		{


#region Infragistics Source Cleanup (Region)












#endregion // Infragistics Source Cleanup (Region)

		}
    
		#region OnPanComplete

		void ISupportScrollHelper.OnPanComplete()
		{
			_cachedHorizValue = null;
			_cachedVertValue = null;

			if (_panel != null)
				_panel.OnPanComplete();
		}

		#endregion //OnPanComplete	
    
		#endregion //Methods	

		#endregion

        #region ItemInfoComparer class

        private class ItemInfoComparer : IComparer<ItemTileInfo>
        {
            #region IComparer<ItemInfoBase> Members

            public int Compare(ItemTileInfo x, ItemTileInfo y)
            {
                if (x == null)
                    return -1;

                if (y == null)
                    return 1;

                ItemTileInfo infoX = x as ItemTileInfo;
                ItemTileInfo infoY = y as ItemTileInfo;

                int logicalIndexX = -1;
                int logicalIndexY = -1;
                if (infoX != null && infoX.SerializationInfo != null)
                    logicalIndexX = infoX.SerializationInfo.LogicalIndex;
                else
                    logicalIndexX = x.LogicalIndex;

                if (infoY != null && infoY.SerializationInfo != null)
                    logicalIndexY = infoY.SerializationInfo.LogicalIndex;
                else
                    logicalIndexY = y.LogicalIndex;

                if (logicalIndexX < logicalIndexY)
                    return -1;

                if (logicalIndexX > logicalIndexY)
                    return 1;

                if (x.Index < y.Index)
                    return -1;

                if (x.Index > y.Index)
                    return 1;

                return 0;
            }

            #endregion
        }

        #endregion //ItemInfoComparer class	
    
        #region MaximizedItemInfoComparer class

        private class MaximizedItemInfoComparer : IComparer<ItemTileInfo>
        {
            #region IComparer<ItemInfo> Members

            public int Compare(ItemTileInfo x, ItemTileInfo y)
            {
                if (x == null)
                    return -1;

                if (y == null)
                    return 1;

                int indexX = -1;
                int indexY = -1;
                if (x.SerializationInfo != null)
                    indexX = x.SerializationInfo.MaximizedIndex;
                if (y.SerializationInfo != null)
                    indexY = y.SerializationInfo.MaximizedIndex;

                if (indexX < indexY)
                    return -1;

                if (indexX > indexY)
                    return 1;

                return 0;
            }

            #endregion
        }

        #endregion //MaximizedItemInfoComparer class	
		
		// JJD 11/1/11 - TFS88171
		// Added object to cache states during a swap operation
		#region SwapInfo nested private class

		internal class SwapInfo
		{
			internal bool _swapIsExpandedWhenMinimized;
			internal bool? _sourceIsExpandedWhenMinimized;
			internal bool? _targetIsExpandedWhenMinimized;
		}

		#endregion //SwapInfo nested private class

		// JJD 03/06/12 - TFS100150 - Added touch support for scrolling


#region Infragistics Source Cleanup (Region)






















































#endregion // Infragistics Source Cleanup (Region)

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