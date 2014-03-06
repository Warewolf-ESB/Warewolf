using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;
using System.Windows.Markup;
using System.Diagnostics;
using System.Windows.Threading;
using System.ComponentModel;
using System.Windows.Documents;
using System.Windows.Shapes;

using Infragistics.Windows.Selection;
using Infragistics.Windows.Licensing;
using Infragistics.Windows.Tiles.Events;
using Infragistics.Windows.Helpers;
using Infragistics.Windows.Commands;
using Infragistics.Windows.Virtualization;
using System.Xml;
using Infragistics.Controls.Layouts.Primitives;
using System.IO;
using System.Reflection;
using Infragistics.Windows.Themes;
using Infragistics.Windows.Automation.Peers.Tiles;
using Infragistics.Windows.Resizing;
using System.Windows.Interop;
using Infragistics.Collections;

namespace Infragistics.Windows.Tiles
{
    /// <summary>
    /// A <see cref="System.Windows.Controls.ItemsControl"/> derived element that arranges and displays its child elements as tiles, with native support for scrolling and virtualizing those items.
    /// </summary>
    
    
    [StyleTypedProperty(Property = "EmptyTilePlaceholderStyle", StyleTargetType = typeof(Tile))]	
    //[ToolboxItem(true)]
    //[System.Drawing.ToolboxBitmap(typeof(TilesPanel), AssemblyVersion.ToolBoxBitmapFolder + "TilesPanel.bmp")]
    //[Description("An ItemControl control derived element that arranges and displays its child elements arranged as tiles, with native support for scrolling and virtualizing those items.")]
    [InfragisticsFeature(Version = FeatureInfo.Version_10_1, FeatureName = FeatureInfo.FeatureName_TilesPanel)]
    public class XamTilesControl : RecyclingItemsControl, IResizeHostMulti
    {
        #region Private Members

        private ObservableCollectionExtended<object> _maximizedItems;
        private ReadOnlyObservableCollection<object> _maximizedItemsReadOnly;
        private UltraLicense _license;
        private TilesPanel _panel;
        private ResizeController _resizeController;

        private TilesPanel.TileMgr  _manager = new TilesPanel.TileMgr();
        private TileAreaSplitter    _splitter;
        private Dictionary<string, object> _itemSerializationMap;

        private double _minimizedAreaExplicitExtentX;
        private double _minimizedAreaExplicitExtentY;
        private double _interTileAreaSpacingResolved;
        private double _interTileSpacingX = (double)InterTileSpacingXProperty.DefaultMetadata.DefaultValue;
        private double _interTileSpacingXMaximizedResolved;
        private double _interTileSpacingXMinimizedResolved;
        private double _interTileSpacingY = (double)InterTileSpacingYProperty.DefaultMetadata.DefaultValue;
        private double _interTileSpacingYMaximizedResolved;
        private double _interTileSpacingYMinimizedResolved;

        private string _headerPath;
        private string _serializationIdPath;
        private bool   _isLoadingLayout;
		
		// JJD 4/19/11 - TFS73129  added
        private bool   _isAnimationInProgress;
		
		private double _splitterMinExtent;
        private List<FrameworkElement> _extraVisualChildren = new List<FrameworkElement>();
		
		// JJD 5/9/11 - TFS74206 - added 
		[ThreadStatic()]
		private static Dictionary<DependencyProperty, Binding> _CachedBindings;

        internal static DependencyProperty s_ItemStringFormatProperty;
        internal static DependencyProperty s_ContentStringFormatProperty;
        internal static DependencyProperty s_HeaderStringFormatProperty;
        internal static PropertyInfo s_BindingStringFormatInfo;

        #endregion //Private Members

        #region Constructor

        static XamTilesControl()
        {
            // register the groupings that should be applied when the theme property is changed
            ThemeManager.RegisterGroupings(typeof(XamTilesControl), new string[] { TilesGeneric.Location.Grouping });

            EventManager.RegisterClassHandler(typeof(XamTilesControl), XamTilesControl.TileClosedEvent, new EventHandler<TileClosedEventArgs>(TileClosedHandler));
            EventManager.RegisterClassHandler(typeof(XamTilesControl), XamTilesControl.TileClosingEvent, new EventHandler<TileClosingEventArgs>( TileClosingHandler) );
            EventManager.RegisterClassHandler(typeof(XamTilesControl), XamTilesControl.TileDraggingEvent, new EventHandler<TileDraggingEventArgs>( TileDraggingHandler) );
            EventManager.RegisterClassHandler(typeof(XamTilesControl), XamTilesControl.TileStateChangedEvent, new EventHandler<TileStateChangedEventArgs>( TileStateChangedHandler) );
            EventManager.RegisterClassHandler(typeof(XamTilesControl), XamTilesControl.TileStateChangingEvent, new EventHandler<TileStateChangingEventArgs>( TileStateChangingHandler) );
            EventManager.RegisterClassHandler(typeof(XamTilesControl), XamTilesControl.TileSwappingEvent, new EventHandler<TileSwappingEventArgs>( TileSwappingHandler) );
            EventManager.RegisterClassHandler(typeof(XamTilesControl), XamTilesControl.TileSwappedEvent, new EventHandler<TileSwappedEventArgs>( TileSwappedHandler) );

            ItemsPanelTemplate template = new ItemsPanelTemplate(new FrameworkElementFactory(typeof(TilesPanel)));
			template.Seal();

            ItemsPanelProperty.OverrideMetadata(typeof(XamTilesControl), new FrameworkPropertyMetadata(template));

            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(XamTilesControl), new FrameworkPropertyMetadata(typeof(XamTilesControl)));
 
            TilesPanel.IsInMaximizedModeProperty.OverrideMetadata(typeof(XamTilesControl),
                                new FrameworkPropertyMetadata(new PropertyChangedCallback(OnIsInMaximizedModeChanged))
                                , TilesPanel.IsInMaximizedModePropertyKey);

            TilesPanel.InterTileAreaSpacingResolvedProperty.OverrideMetadata(typeof(XamTilesControl),
                                new FrameworkPropertyMetadata(new PropertyChangedCallback(OnInterTileAreaSpacingResolvedChanged))
                                , TilesPanel.InterTileAreaSpacingResolvedPropertyKey);

            TilesPanel.InterTileSpacingXMaximizedResolvedProperty.OverrideMetadata(typeof(XamTilesControl),
                                new FrameworkPropertyMetadata(new PropertyChangedCallback(OnInterTileSpacingXMaximizedResolvedChanged))
                                , TilesPanel.InterTileSpacingXMaximizedResolvedPropertyKey);

            TilesPanel.InterTileSpacingXMinimizedResolvedProperty.OverrideMetadata(typeof(XamTilesControl),
                                new FrameworkPropertyMetadata(new PropertyChangedCallback(OnInterTileSpacingXMinimizedResolvedChanged))
                                , TilesPanel.InterTileSpacingXMinimizedResolvedPropertyKey);

            TilesPanel.InterTileSpacingYMaximizedResolvedProperty.OverrideMetadata(typeof(XamTilesControl),
                                new FrameworkPropertyMetadata(new PropertyChangedCallback(OnInterTileSpacingYMaximizedResolvedChanged))
                                , TilesPanel.InterTileSpacingYMaximizedResolvedPropertyKey);

            TilesPanel.InterTileSpacingYMinimizedResolvedProperty.OverrideMetadata(typeof(XamTilesControl),
                                new FrameworkPropertyMetadata(new PropertyChangedCallback(OnInterTileSpacingYMinimizedResolvedChanged))
                                , TilesPanel.InterTileSpacingYMinimizedResolvedPropertyKey);


            // Cache the DependencyPropertys for ItemStringFormat and ContentStringFormat since
            // these properties were only introduced in 3.5 we can't access these properties 
            // directly. Otherwise we will blow up when running on an earlier versions of the framework
            DependencyPropertyDescriptor pd = DependencyPropertyDescriptor.FromName("ItemStringFormat", typeof(ItemsControl), typeof(XamTilesControl));

            if (pd != null)
            {
                s_ItemStringFormatProperty = pd.DependencyProperty;

                pd = DependencyPropertyDescriptor.FromName("ContentStringFormat", typeof(ContentControl), typeof(Tile));

                if (pd != null)
                    s_ContentStringFormatProperty = pd.DependencyProperty;

                pd = DependencyPropertyDescriptor.FromName("HeaderStringFormat", typeof(HeaderedContentControl), typeof(Tile));

                if (pd != null)
                    s_HeaderStringFormatProperty = pd.DependencyProperty;

                s_BindingStringFormatInfo = typeof(BindingBase).GetProperty("StringFormat");
            }
        }

        /// <summary>
        /// Instantiates a new instance of a XamTilesControl.
        /// </summary>
        public XamTilesControl()
        {
            try
            {
                // We need to pass our type into the method since we do not want to pass in 
                // the derived type.
                this._license = LicenseManager.Validate(typeof(XamTilesControl), this) as UltraLicense;
            }
            catch (System.IO.FileNotFoundException) { }

            this._maximizedItems            = new ObservableCollectionExtended<object>();
            this._maximizedItemsReadOnly    = new ReadOnlyObservableCollection<object>(this._maximizedItems);

            // Create a ResizeController to wire the mouse events for resizing tiles 
            this._resizeController          = new ResizeController(this);

			// JJD 4/28/11 - TFS73523 - added
			this.InitializeSpacingOnResizeController();

		}

        #endregion //Constructor

        #region Base Class Overrides

			#region Properties

				#region HandlesScrolling

		/// <summary>
		/// Returns a value that indicates whether the control handles scrolling.
		/// </summary>
		/// <returns>True if the control has support for scrolling, otherwise false.</returns>
		protected override bool HandlesScrolling
		{
			get { return true; }
		}

				#endregion //HandlesScrolling

                #region LogicalChildren

        /// <summary>
        /// Gets an enumerator for the control's logical child elements.
        /// </summary>
        /// <value>An enumerator.</value>
        protected override IEnumerator LogicalChildren
        {
            get
            {
                if (this._splitter == null)
                    return base.LogicalChildren;

                return new MultiSourceEnumerator(base.LogicalChildren, new SingleItemEnumerator(this._splitter));
            }
        }

                #endregion //LogicalChildren	
    
                #region OnCreateAutomationPeer

        /// <summary>
        /// Returns an automation peer that exposes the <see cref="XamTilesControl"/> to UI Automation.
        /// </summary>
        /// <returns>A <see cref="Infragistics.Windows.Automation.Peers.Tiles.XamTilesControlAutomationPeer"/></returns>
        protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer()
        {
            return new XamTilesControlAutomationPeer(this); ;
        }

                #endregion //OnCreateAutomationPeer	
    
			#endregion //Properties
        
            #region Methods

                #region ArrangeOverride

		/// <summary>
		/// Positions child elements and determines a size for this element.
		/// </summary>
		/// <param name="finalSize">The size available to this element for arranging its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> used by this element to arrange its children.</returns>
		protected override Size ArrangeOverride(Size finalSize)
        {
            IResizeHost resizeHost = this as IResizeHost;

            if (resizeHost != null &&
                 resizeHost.Controller.ResizerBar != null)
                resizeHost.Controller.PositionResizerBar();

            this.InitializeSplitter();

            if (this._splitter != null && this._panel != null)
            {
                Rect splitterRect = this._panel.GetSplitterRect();

                if (splitterRect.IsEmpty)
                    this._splitter.Visibility = Visibility.Collapsed;
                else
                {
                    Point pt = this.TranslatePoint(splitterRect.Location, this._panel);

                    // offset the rect into our coordinates
                    splitterRect.Offset(splitterRect.X - pt.X, splitterRect.Y - pt.Y);

                    this._splitter.Visibility = Visibility.Visible;

                    //this._splitter.Measure(splitterRect.Size);

                    double measureExtent;
                    double desiredExtent;

                    if (this._splitter.Orientation == Orientation.Vertical)
                    {
                        measureExtent = splitterRect.Width;
                        desiredExtent = this._splitter.DesiredSize.Width;
                        splitterRect.Width = desiredExtent;

                        
                        // Make sure we don't try to arrange with an infinite height
                        if (double.IsPositiveInfinity(splitterRect.Height))
                            splitterRect.Height = finalSize.Height;
                    }
                    else
                    {
                        measureExtent = splitterRect.Height;
                        desiredExtent = this._splitter.DesiredSize.Height;
                        splitterRect.Height = desiredExtent;

                        
                        // Make sure we don't try to arrange with an infinite width
                        if (double.IsPositiveInfinity(splitterRect.Width))
                            splitterRect.Height = finalSize.Width;
                    }

                    bool isVertical = this._splitter.Orientation == Orientation.Vertical;

                    double delta = measureExtent - desiredExtent;

                    // if the splitter resists the size change then try to center it
                    if ( delta > 0 && !TileUtilities.AreClose(delta, 0))
                    {
                        if (isVertical)
                            splitterRect.Offset(delta / 2, 0);
                        else
                            splitterRect.Offset(0, delta / 2);
                    }

                    this._splitter.Arrange(splitterRect);
                }
            }

            return base.ArrangeOverride(finalSize);
        }

                #endregion //ArrangeOverride	

                #region ClearContainerForItemOverride

        /// <summary>
        /// Called to clear the effects of the PrepareContainerForItemOverride method. 
        /// </summary>
        /// <param name="element">The container being cleared.</param>
        /// <param name="item">The item contained by the container being cleared.</param>
        protected override void ClearContainerForItemOverride(DependencyObject element, object item)
        {
            base.ClearContainerForItemOverride(element, item);

            Tile tile = element as Tile;

            if (tile != null && tile != item)
            {
                string headerPath = this.HeaderPath;

                if (headerPath != null && headerPath.Length > 0)
                    BindingOperations.ClearBinding(element, Tile.HeaderProperty);

                string serializationIdPath = this.SerializationIdPath;

                if (serializationIdPath != null && serializationIdPath.Length > 0)
                    BindingOperations.ClearBinding(element, Tile.SerializationIdProperty);

                DependencyObject content = tile.Content as DependencyObject;

                if (content != null)
                    tile.ClearValue(Tile.ContentProperty);
            }


        }

                #endregion //ClearContainerForItemOverride	
    
				#region GetContainerForItemOverride

		/// <summary>
		/// Provides a container object for a items in the list.
		/// </summary>
		/// <returns>An object that that can be used as a container for items in the list.</returns>
		protected override System.Windows.DependencyObject GetContainerForItemOverride()
		{
            return new Tile();
		}

				#endregion //GetContainerForItemOverride

                #region GetVisualChild

        /// <summary>
        /// Gets the parent child at a specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the specific child parent.</param>
        /// <returns>The parent child at the specified index.</returns>
        protected override Visual GetVisualChild(int index)
        {
            int baseCount = base.VisualChildrenCount;

            IResizeHost resizeHost = this as IResizeHost;

            if (index >= baseCount)
            {
                int extraChildIndex = index - baseCount;

                if (extraChildIndex < this._extraVisualChildren.Count)
                    return this._extraVisualChildren[extraChildIndex];
            }

            return base.GetVisualChild(index);
        }

                #endregion //GetVisualChild	

                #region IsContainerCompatibleWithItem

        /// <summary>
        /// Determines if a container can be reused for a specific item.
        /// </summary>
        /// <param name="container">The container to be reused.</param>
        /// <param name="item">The potential new item.</param>
        /// <returns>True if the container can be reused for the item</returns>
        /// <remarks>
        /// <para class="body">When looking for a suitable container for an item the generator will search its cache and call this method to see 
        /// if one of its cached containers is compatible with the item. If this method returns true then the container is assigned to the item and 
        /// the <see cref="ReuseContainerForNewItem"/> method is called.
        /// </para>
        /// <para class="note"><b>Note:</b> the default implementation always returns true.</para>
        /// </remarks>
        internal protected override bool IsContainerCompatibleWithItem(DependencyObject container, object item)
        {
            Tile tile = container as Tile;

            if (tile != null)
            {
                switch (tile.State)
                {
                    case TileState.Maximized:
                    case TileState.MinimizedExpanded:
                        return false;
                }
            }

            return base.IsContainerCompatibleWithItem(container, item);
        }

                #endregion //IsContainerCompatibleWithItem	
    
                #region IsItemItsOwnContainerOverride

        /// <summary>
        /// Determines if the specified item is (or is eligible to be) its own container. 
        /// </summary>
        /// <param name="item">The item to evaluate</param>
        /// <returns>True if the specified item is its own container.</returns>
        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return (item is Tile);
        }

                #endregion //IsItemItsOwnContainerOverride
        
                // JJD 05/07/10 - TFS31643 - added
                #region IsStillValid

        /// <summary>
        /// Deterimines whether the container and item are still valid
        /// </summary>
        /// <param name="container">The container associated with the item.</param>
        /// <param name="item">Its associated item.</param>
        /// <returns>True if still valid. The default is null.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        internal protected override bool? IsStillValid(DependencyObject container, object item) 
        {
            // JJD 05/07/10 - TFS31643
            // Call the GetItemInfo with false. This will use a hash to find an 
            // existing item which is much more efficient than calling IndexOf
            // on the Items collection
            return item != null && 
                this._manager != null &&
                this._manager.GetItemInfo(item, false, -1) != null;
         }

                #endregion //IsStillValid
    

                #region OnApplyTemplate

        /// <summary>
        /// Invoked when the template has been applied to the element.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }

                #endregion //OnApplyTemplate	

                #region OnItemsChanged

        /// <summary>
        /// Overridden. Invoked when the contents of the items collection has changed.
        /// </summary>
        /// <param name="e">Event arguments indicating the change that occurred.</param>
        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);

            // if we are bound to a list of items then we don't need to sync them up
            if (this.ItemsSource != null)
                return;

            // JJD 2/06/10 - TFS27262 
            // If Tiles are added directly to the Items collection we need to make sure
            // that any with a State of Maximized end up in the MaximizedItems collection
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Reset:
                    {
                        ItemCollection items = this.Items;

                        int count = items.Count;

                        for (int i = 0; i < count; i++)
                        {
                            Tile tile = items[i] as Tile;

                            if (tile != null)
                                tile.InitializeTilesControl(this);
                        }
                        break;
                    }
                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Replace:
                    {
                        // JJD 2/06/10 - TFS27262 
                        IList newItems = e.NewItems;

                        int count = newItems.Count;

                        for (int i = 0; i < count; i++)
                        {
                            Tile tile = newItems[i] as Tile;

                            if (tile != null)
                                tile.InitializeTilesControl(this);
                        }
                        break;
                    }
            }
        }

                #endregion //OnItemsChanged	

                // JJD 5/3/10 - TFS31411 - added
                #region OnNavigationServiceInitialized

        /// <summary>
        /// Called when the NavigationService has been set to a non-null value
        /// </summary>
        protected override void OnNavigationServiceInitialized()
        {
            // do not call the base implementation since that will remove all
            // existing generated items and will re-animate any new ones 
            // into place
            if (this._panel != null)
                this._panel.InvalidateMeasure();
        }

                #endregion //OnNavigationServiceInitialized	
    
                #region PrepareContainerForItemOverride

        /// <summary>
        /// Prepares the specified container element to display the specified item. 
        /// </summary>
        /// <param name="element">The container element to prepare.</param>
        /// <param name="item">The item contained by the specified container element.</param>
        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
		{
            Tile tile = element as Tile;

            if (tile != null)
            {
                InitializeTileContainer(item, tile);
            }

			base.PrepareContainerForItemOverride(element, item);
		}

				#endregion //PrepareContainerForItemOverride
    		
                #region ReuseContainerForNewItem

		/// <summary>
		/// Called when a container is being reused, i.e. recycled, or a different item.
		/// </summary>
		/// <param name="container">The container being reused/recycled.</param>
		/// <param name="item">The new item.</param>
		/// <remarks>
		/// <para class="note"><b>Note:</b> if the container had previously been deactivated then the original setting for the Visibility property, prior to its deactivation (refer to <see cref="RecyclingItemsControl.DeactivateContainer"/>), will be restored before a this method is called.</para>
		/// </remarks>
		internal protected override void ReuseContainerForNewItem(DependencyObject container, object item)
		{
            Tile tile = container as Tile;

            if (tile != null)
            {
                InitializeTileContainer(item, tile);
            }

            base.ReuseContainerForNewItem(container, item);
		}

				#endregion //ReuseContainerForNewItem	
        
                #region VisualChildrenCount

        /// <summary>
        /// Returns the total number of children (read-only)
        /// </summary>
        protected override int VisualChildrenCount
        {
            get
            {
                return base.VisualChildrenCount + this._extraVisualChildren.Count;
            }
        }

                #endregion //VisualChildrenCount	

            #endregion //Methods

        #endregion //Base Class Overrides

        #region Events

            #region Class event handlers

        static void TileClosedHandler(object sender, TileClosedEventArgs e)
        {
            XamTilesControl control = sender as XamTilesControl;

            if (control != null && !(e.OriginalSource is XamTilesControl))
            {
                TileClosedEventArgs newArgs = new TileClosedEventArgs(e.Tile, e.Item);
                newArgs.RoutedEvent = TilesPanel.TileClosedEvent;
                newArgs.Source = control;
                newArgs.Handled = e.Handled;
                control.OnTileClosed(newArgs);
                e.Handled = true;
            }
        }

        static void TileClosingHandler(object sender, TileClosingEventArgs e)
        {
            XamTilesControl control = sender as XamTilesControl;

            if (control != null && !(e.OriginalSource is XamTilesControl))
            {
                TileClosingEventArgs newArgs = new TileClosingEventArgs(e.Tile, e.Item);
                newArgs.RoutedEvent = TilesPanel.TileClosingEvent;
                newArgs.Source = control;
                newArgs.Cancel = e.Cancel;
                newArgs.Handled = e.Handled;
                control.OnTileClosing(newArgs);
                e.Cancel = newArgs.Cancel;
                e.Handled = true;
            }
        }

        static void TileDraggingHandler(object sender, TileDraggingEventArgs e)
        {
            XamTilesControl control = sender as XamTilesControl;

            if (control != null && !(e.OriginalSource is XamTilesControl))
            {
                TileDraggingEventArgs newArgs = new TileDraggingEventArgs(e.Tile, e.Item);
                newArgs.RoutedEvent = TilesPanel.TileDraggingEvent;
                newArgs.Source = control;
                newArgs.Cancel = e.Cancel;
                newArgs.Handled = e.Handled;
                control.OnTileDragging(newArgs);
                e.Cancel = newArgs.Cancel;
                e.Handled = true;
            }
        }

        static void TileStateChangedHandler(object sender, TileStateChangedEventArgs e)
        {
            XamTilesControl control = sender as XamTilesControl;

            if (control != null && !(e.OriginalSource is XamTilesControl))
            {
                TileStateChangedEventArgs newArgs = new TileStateChangedEventArgs(e.Tile, e.Item, e.NewState, e.OldState);
                newArgs.RoutedEvent = TilesPanel.TileStateChangedEvent;
                newArgs.Source = control;
                newArgs.Handled = e.Handled;
                control.OnTileStateChanged(newArgs);
                e.Handled = true;
            }
        }

        static void TileStateChangingHandler(object sender, TileStateChangingEventArgs e)
        {
            XamTilesControl control = sender as XamTilesControl;

            if (control != null && !(e.OriginalSource is XamTilesControl))
            {
                TileStateChangingEventArgs newArgs = new TileStateChangingEventArgs(e.Tile, e.Item, e.NewState);
                newArgs.RoutedEvent = TilesPanel.TileStateChangingEvent;
                newArgs.Source = control;
                newArgs.Cancel = e.Cancel;
                newArgs.Handled = e.Handled;
                control.OnTileStateChanging(newArgs);
                e.Cancel = newArgs.Cancel;
                e.Handled = true;
            }
        }

        static void TileSwappingHandler(object sender, TileSwappingEventArgs e)
        {
            XamTilesControl control = sender as XamTilesControl;

            if (control != null && !(e.OriginalSource is XamTilesControl))
            {
                TileSwappingEventArgs newArgs = new TileSwappingEventArgs(e.Tile, e.Item, e.TargetTile, e.TargetItem);
				// JJD 11/1/11 - TFS88171 
				// copy over the SwapIsExpandedWhenMinimized parameter
				newArgs.SwapIsExpandedWhenMinimized = e.SwapIsExpandedWhenMinimized;

                newArgs.RoutedEvent = TilesPanel.TileSwappingEvent;
                newArgs.Source = control;
                newArgs.Cancel = e.Cancel;
                newArgs.Handled = e.Handled;
                control.OnTileSwapping(newArgs);
				
				// JJD 11/1/11 - TFS88171 
				// copy back the SwapIsExpandedWhenMinimized parameter
				e.SwapIsExpandedWhenMinimized = newArgs.SwapIsExpandedWhenMinimized;
                e.Cancel = newArgs.Cancel;
                e.Handled = true;
            }
        }

        static void TileSwappedHandler(object sender, TileSwappedEventArgs e)
        {
            XamTilesControl control = sender as XamTilesControl;

            if (control != null && !(e.OriginalSource is XamTilesControl))
            {
                TileSwappedEventArgs newArgs = new TileSwappedEventArgs(e.Tile, e.Item, e.TargetTile, e.TargetItem);
                newArgs.RoutedEvent = TilesPanel.TileSwappedEvent;
                newArgs.Source = control;
                newArgs.Handled = e.Handled;
                control.OnTileSwapped(newArgs);
                e.Handled = true;
            }
        }

            #endregion //Class event handlers	

			// JJD 4/19/11 - TFS73129  added
			#region AnimationEnded

		/// <summary>
		/// Event ID for the <see cref="AnimationEnded"/> routed event
		/// </summary>
		/// <seealso cref="AnimationEnded"/>
		/// <seealso cref="OnAnimationEnded"/>
		public static readonly RoutedEvent AnimationEndedEvent =
			EventManager.RegisterRoutedEvent("AnimationEnded", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(XamTilesControl));

		/// <summary>
		/// Occurs when tile animations end
		/// </summary>
		/// <seealso cref="AnimationEnded"/>
		/// <seealso cref="AnimationEndedEvent"/>
		protected virtual void OnAnimationEnded(RoutedEventArgs args)
		{
			this.RaiseEvent(args);
		}

		internal void RaiseAnimationEnded()
		{
			RoutedEventArgs args = new RoutedEventArgs();
			args.RoutedEvent = XamTilesControl.AnimationEndedEvent;
			args.Source = this;
			this.OnAnimationEnded(args);
		}

		/// <summary>
		/// Occurs when tile animations end
		/// </summary>
		/// <seealso cref="OnAnimationEnded"/>
		/// <seealso cref="AnimationEndedEvent"/>
		//[Description("Occurs when tile animations end")]
		//[Category("TilesControl Events")] 
		public event RoutedEventHandler AnimationEnded
		{
			add
			{
				base.AddHandler(XamTilesControl.AnimationEndedEvent, value);
			}
			remove
			{
				base.RemoveHandler(XamTilesControl.AnimationEndedEvent, value);
			}
		}

			#endregion //AnimationEnded

			// JJD 4/19/11 - TFS73129  added
			#region AnimationStarted

		/// <summary>
		/// Event ID for the <see cref="AnimationStarted"/> routed event
		/// </summary>
		/// <seealso cref="AnimationStarted"/>
		/// <seealso cref="OnAnimationStarted"/>
		public static readonly RoutedEvent AnimationStartedEvent =
			EventManager.RegisterRoutedEvent("AnimationStarted", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(XamTilesControl));

		/// <summary>
		/// Occurs when tile animations start
		/// </summary>
		/// <seealso cref="AnimationStarted"/>
		/// <seealso cref="AnimationStartedEvent"/>
		protected virtual void OnAnimationStarted(RoutedEventArgs args)
		{
			this.RaiseEvent(args);
		}

		internal void RaiseAnimationStarted()
		{
			RoutedEventArgs args = new RoutedEventArgs();
			args.RoutedEvent = XamTilesControl.AnimationStartedEvent;
			args.Source = this;
			this.OnAnimationStarted(args);
		}

		/// <summary>
		/// Occurs when tile animations start
		/// </summary>
		/// <seealso cref="OnAnimationStarted"/>
		/// <seealso cref="AnimationStartedEvent"/>
		//[Description("Occurs when tile animations start")]
		//[Category("TilesControl Events")] 
		public event RoutedEventHandler AnimationStarted
		{
			add
			{
				base.AddHandler(XamTilesControl.AnimationStartedEvent, value);
			}
			remove
			{
				base.RemoveHandler(XamTilesControl.AnimationStartedEvent, value);
			}
		}

			#endregion //AnimationStarted

            #region LoadingItemMapping

        /// <summary>
        /// Event ID for the <see cref="LoadingItemMapping"/> routed event
        /// </summary>
        /// <seealso cref="LoadingItemMapping"/>
        /// <seealso cref="OnLoadingItemMapping"/>
        /// <seealso cref="LoadingItemMappingEventArgs"/>
        public static readonly RoutedEvent LoadingItemMappingEvent =
            EventManager.RegisterRoutedEvent("LoadingItemMapping", RoutingStrategy.Direct, typeof(EventHandler<LoadingItemMappingEventArgs>), typeof(XamTilesControl));

        /// <summary>
        /// Occurs during a call to <see cref="LoadLayout(Stream)"/>.
        /// </summary>
        /// <seealso cref="LoadingItemMapping"/>
        /// <seealso cref="LoadingItemMappingEvent"/>
        /// <seealso cref="LoadingItemMappingEventArgs"/>
        protected virtual void OnLoadingItemMapping(LoadingItemMappingEventArgs args)
        {
            this.RaiseEvent(args);
        }

        internal void RaiseLoadingItemMapping(LoadingItemMappingEventArgs args)
        {
            args.RoutedEvent = XamTilesControl.LoadingItemMappingEvent;
            args.Source = this;
            this.OnLoadingItemMapping(args);
        }

        /// <summary>
        /// Occurs during a call to <see cref="LoadLayout(Stream)"/>.
        /// </summary>
        /// <seealso cref="OnLoadingItemMapping"/>
        /// <seealso cref="LoadingItemMappingEvent"/>
        /// <seealso cref="LoadingItemMappingEventArgs"/>
        //[Description("Occurs during a call to 'LoadLayout'.")]
        //[Category("TilesControl Events")] 
        public event EventHandler<LoadingItemMappingEventArgs> LoadingItemMapping
        {
            add
            {
                base.AddHandler(XamTilesControl.LoadingItemMappingEvent, value);
            }
            remove
            {
                base.RemoveHandler(XamTilesControl.LoadingItemMappingEvent, value);
            }
        }

            #endregion //LoadingItemMapping

            #region SavingItemMapping

        /// <summary>
        /// Event ID for the <see cref="SavingItemMapping"/> routed event
        /// </summary>
        /// <seealso cref="SavingItemMapping"/>
        /// <seealso cref="OnSavingItemMapping"/>
        /// <seealso cref="SavingItemMappingEventArgs"/>
        public static readonly RoutedEvent SavingItemMappingEvent =
            EventManager.RegisterRoutedEvent("SavingItemMapping", RoutingStrategy.Direct, typeof(EventHandler<SavingItemMappingEventArgs>), typeof(XamTilesControl));

        /// <summary>
        /// Occurs during a call to <see cref="SaveLayout(Stream)"/>.
        /// </summary>
        /// <seealso cref="SavingItemMapping"/>
        /// <seealso cref="SavingItemMappingEvent"/>
        /// <seealso cref="SavingItemMappingEventArgs"/>
        protected virtual void OnSavingItemMapping(SavingItemMappingEventArgs args)
        {
            this.RaiseEvent(args);
        }

        internal void RaiseSavingItemMapping(SavingItemMappingEventArgs args)
        {
            args.RoutedEvent = XamTilesControl.SavingItemMappingEvent;
            args.Source = this;
            this.OnSavingItemMapping(args);
        }

        /// <summary>
        /// Occurs during a call to <see cref="SaveLayout(Stream)"/>.
        /// </summary>
        /// <seealso cref="OnSavingItemMapping"/>
        /// <seealso cref="SavingItemMappingEvent"/>
        /// <seealso cref="SavingItemMappingEventArgs"/>
        //[Description("Occurs during a call to 'SaveLayout'.")]
        //[Category("TilesControl Events")] 
        public event EventHandler<SavingItemMappingEventArgs> SavingItemMapping
        {
            add
            {
                base.AddHandler(XamTilesControl.SavingItemMappingEvent, value);
            }
            remove
            {
                base.RemoveHandler(XamTilesControl.SavingItemMappingEvent, value);
            }
        }

            #endregion //SavingItemMapping
 
            #region TileClosed

        /// <summary>
        /// Event ID for the <see cref="TileClosed"/> routed event
        /// </summary>
        /// <seealso cref="TileClosed"/>
        /// <seealso cref="OnTileClosed"/>
        /// <seealso cref="TileClosedEventArgs"/>
        public static readonly RoutedEvent TileClosedEvent = TilesPanel.TileClosedEvent.AddOwner(typeof(XamTilesControl));

        /// <summary>
        /// Occurs after a <see cref="Tile"/> has been closed.
        /// </summary>
        /// <seealso cref="TileClosed"/>
        /// <seealso cref="TileClosedEvent"/>
        /// <seealso cref="TileClosedEventArgs"/>
        protected virtual void OnTileClosed(TileClosedEventArgs args)
        {
            this.RaiseEvent(args);
        }

        /// <summary>
        /// Occurs after a <see cref="Tile"/> has been closed.
        /// </summary>
        /// <seealso cref="OnTileClosed"/>
        /// <seealso cref="TileClosedEvent"/>
        /// <seealso cref="TileClosedEventArgs"/>
        //[Description("Occurs after the Tile has been closed.")]
        //[Category("TilesControl Events")]  
        public event EventHandler<TileClosedEventArgs> TileClosed
        {
            add
            {
                base.AddHandler(XamTilesControl.TileClosedEvent, value);
            }
            remove
            {
                base.RemoveHandler(XamTilesControl.TileClosedEvent, value);
            }
        }

            #endregion //TileClosed

            #region TileClosing

        /// <summary>
        /// Event ID for the <see cref="TileClosing"/> routed event
        /// </summary>
        /// <seealso cref="TileClosing"/>
        /// <seealso cref="OnTileClosing"/>
        /// <seealso cref="TileClosingEventArgs"/>
        public static readonly RoutedEvent TileClosingEvent = TilesPanel.TileClosingEvent.AddOwner(typeof(XamTilesControl));

        /// <summary>
        /// Occurs when a <see cref="Tile"/> is about to close.
        /// </summary>
        /// <seealso cref="TileClosing"/>
        /// <seealso cref="TileClosingEvent"/>
        /// <seealso cref="TileClosingEventArgs"/>
        protected virtual void OnTileClosing(TileClosingEventArgs args)
        {
            this.RaiseEvent(args);
        }

        /// <summary>
        /// Occurs when a <see cref="Tile"/> is about to close.
        /// </summary>
        /// <seealso cref="OnTileClosing"/>
        /// <seealso cref="TileClosingEvent"/>
        /// <seealso cref="TileClosingEventArgs"/>
        //[Description("Occurs when a Tile is about to close.")]
        //[Category("TilesControl Events")]  
        public event EventHandler<TileClosingEventArgs> TileClosing
        {
            add
            {
                base.AddHandler(XamTilesControl.TileClosingEvent, value);
            }
            remove
            {
                base.RemoveHandler(XamTilesControl.TileClosingEvent, value);
            }
        }

            #endregion //TileClosing

            #region TileDragging

        /// <summary>
        /// Event ID for the <see cref="TileDragging"/> routed event
        /// </summary>
        /// <seealso cref="TileDragging"/>
        /// <seealso cref="OnTileDragging"/>
        /// <seealso cref="TileDraggingEventArgs"/>
        public static readonly RoutedEvent TileDraggingEvent = TilesPanel.TileDraggingEvent.AddOwner(typeof(XamTilesControl));

        /// <summary>
        /// Occurs when a <see cref="Tile"/> is about to be dragged.
        /// </summary>
        /// <seealso cref="TileDragging"/>
        /// <seealso cref="TileDraggingEvent"/>
        /// <seealso cref="TileDraggingEventArgs"/>
        protected virtual void OnTileDragging(TileDraggingEventArgs args)
        {
            this.RaiseEvent(args);
        }

        /// <summary>
        /// Occurs when a <see cref="Tile"/> is about to be dragged.
        /// </summary>
        /// <seealso cref="OnTileDragging"/>
        /// <seealso cref="TileDraggingEvent"/>
        /// <seealso cref="TileDraggingEventArgs"/>
        //[Description("Occurs when a Tile is about to be dragged.")]
        //[Category("TilesControl Events")]  
        public event EventHandler<TileDraggingEventArgs> TileDragging
        {
            add
            {
                base.AddHandler(XamTilesControl.TileDraggingEvent, value);
            }
            remove
            {
                base.RemoveHandler(XamTilesControl.TileDraggingEvent, value);
            }
        }

            #endregion //TileDragging

            #region TileStateChanged

        /// <summary>
        /// Event ID for the <see cref="TileStateChanged"/> routed event
        /// </summary>
        /// <seealso cref="TileStateChanged"/>
        /// <seealso cref="OnTileStateChanged"/>
        /// <seealso cref="TileStateChangedEventArgs"/>
        public static readonly RoutedEvent TileStateChangedEvent = TilesPanel.TileStateChangedEvent.AddOwner(typeof(XamTilesControl));

        /// <summary>
        /// Occurs after the state of a <see cref="Tile"/> has changed.
        /// </summary>
        /// <seealso cref="TileStateChanged"/>
        /// <seealso cref="TileStateChangedEvent"/>
        /// <seealso cref="TileStateChangedEventArgs"/>
        protected virtual void OnTileStateChanged(TileStateChangedEventArgs args)
        {
            this.RaiseEvent(args);
        }

        /// <summary>
        /// Occurs after the state of a <see cref="Tile"/> has changed.
        /// </summary>
        /// <seealso cref="TileStateChangedEvent"/>
        /// <seealso cref="OnTileStateChanged"/>
        /// <seealso cref="TileStateChangedEventArgs"/>
        //[Description("Occurs after the state of a Tile has changed.")]
        //[Category("TilesControl Events")]  
        public event EventHandler<TileStateChangedEventArgs> TileStateChanged
        {
            add
            {
                base.AddHandler(XamTilesControl.TileStateChangedEvent, value);
            }
            remove
            {
                base.RemoveHandler(XamTilesControl.TileStateChangedEvent, value);
            }
        }

            #endregion //TileStateChanged

            #region TileStateChanging

        /// <summary>
        /// Event ID for the <see cref="TileStateChanging"/> routed event
        /// </summary>
        /// <seealso cref="TileStateChanging"/>
        /// <seealso cref="OnTileStateChanging"/>
        /// <seealso cref="TileStateChangingEventArgs"/>
        public static readonly RoutedEvent TileStateChangingEvent = TilesPanel.TileStateChangingEvent.AddOwner(typeof(XamTilesControl));

        /// <summary>
        /// Occurs when the state of a <see cref="Tile"/> is about to change.
        /// </summary>
        /// <seealso cref="TileStateChanging"/>
        /// <seealso cref="TileStateChangingEvent"/>
        /// <seealso cref="TileStateChangingEventArgs"/>
        protected virtual void OnTileStateChanging(TileStateChangingEventArgs args)
        {
            this.RaiseEvent(args);
        }

        /// <summary>
        /// Occurs when the state of a <see cref="Tile"/> is about to change.
        /// </summary>
        /// <seealso cref="TileStateChangingEvent"/>
        /// <seealso cref="OnTileStateChanging"/>
        /// <seealso cref="TileStateChangingEventArgs"/>
        //[Description("Occurs when the state of a Tile is about to change.")]
        //[Category("TilesControl Events")]  
        public event EventHandler<TileStateChangingEventArgs> TileStateChanging
        {
            add
            {
                base.AddHandler(XamTilesControl.TileStateChangingEvent, value);
            }
            remove
            {
                base.RemoveHandler(XamTilesControl.TileStateChangingEvent, value);
            }
        }

            #endregion //TileStateChanging

            #region TileSwapping

        /// <summary>
        /// Event ID for the <see cref="TileSwapping"/> routed event
        /// </summary>
        /// <seealso cref="TileSwapping"/>
        /// <seealso cref="OnTileSwapping"/>
        /// <seealso cref="TileSwappingEventArgs"/>
        public static readonly RoutedEvent TileSwappingEvent = TilesPanel.TileSwappingEvent.AddOwner(typeof(XamTilesControl));

        /// <summary>
        /// Occurs when a <see cref="Tile"/> is dragged over another tile that is a potential swap target.
        /// </summary>
        /// <seealso cref="TileSwapping"/>
        /// <seealso cref="TileSwappingEvent"/>
        /// <seealso cref="TileSwappingEventArgs"/>
        protected virtual void OnTileSwapping(TileSwappingEventArgs args)
        {
            this.RaiseEvent(args);
        }

        /// <summary>
        /// Occurs when a <see cref="Tile"/> is dragged over another tile that is a potential swap target.
        /// </summary>
        /// <seealso cref="OnTileSwapping"/>
        /// <seealso cref="TileSwappingEvent"/>
        /// <seealso cref="TileSwappingEventArgs"/>
        //[Description("Occurs when a Tile is dragged over another tile that is a potential swap target.")]
        //[Category("TilesControl Events")]  
        public event EventHandler<TileSwappingEventArgs> TileSwapping
        {
            add
            {
                base.AddHandler(XamTilesControl.TileSwappingEvent, value);
            }
            remove
            {
                base.RemoveHandler(XamTilesControl.TileSwappingEvent, value);
            }
        }

            #endregion //TileSwapping

            #region TileSwapped

        /// <summary>
        /// Event ID for the <see cref="TileSwapped"/> routed event
        /// </summary>
        /// <seealso cref="TileSwapped"/>
        /// <seealso cref="OnTileSwapped"/>
        /// <seealso cref="TileSwappedEventArgs"/>
        public static readonly RoutedEvent TileSwappedEvent = TilesPanel.TileSwappedEvent.AddOwner(typeof(XamTilesControl));

        /// <summary>
        /// Occurs when a <see cref="Tile"/> is dropped over another tile and swaps places with it.
        /// </summary>
        /// <seealso cref="TileSwapped"/>
        /// <seealso cref="TileSwappedEvent"/>
        /// <seealso cref="TileSwappedEventArgs"/>
        protected virtual void OnTileSwapped(TileSwappedEventArgs args)
        {
            this.RaiseEvent(args);
        }

        /// <summary>
        /// Occurs when a <see cref="Tile"/> is dropped over another tile and swaps places with it.
        /// </summary>
        /// <seealso cref="OnTileSwapped"/>
        /// <seealso cref="TileSwappedEvent"/>
        /// <seealso cref="TileSwappedEventArgs"/>
        //[Description("Occurs when a Tile is dropped over another tile and swaps places with it.")]
        //[Category("TilesControl Events")]  
        public event EventHandler<TileSwappedEventArgs> TileSwapped
        {
            add
            {
                base.AddHandler(XamTilesControl.TileSwappedEvent, value);
            }
            remove
            {
                base.RemoveHandler(XamTilesControl.TileSwappedEvent, value);
            }
        }

            #endregion //TileSwapped

        #endregion //Events	
    
        #region Properties
        
            #region Public Attached Properties

		        #region Column

		/// <summary>
		/// Identifies the Column attached dependency property.
		/// </summary>
		public static readonly DependencyProperty ColumnProperty = TilesPanel.ColumnProperty.AddOwner(typeof(XamTilesControl));

        /// <summary>
        /// Gets the value of the Column attached property of the specified element, -1 indicates that the tile will be positioned relative to the previous tile in the panel. The default value is 0.
        /// </summary>
        /// <param name="elem">This element's Column value will be returned.</param>
        /// <returns>The value of the Column attached property. The default value is 0.</returns>
        /// <remarks>
        /// <para class="note"><b>Note:</b> this property is ignored unless <see cref="XamTilesControl"/>.<see cref="XamTilesControl.NormalModeSettings"/>.<see cref="Infragistics.Windows.Tiles.NormalModeSettings.TileLayoutOrder"/> is set to 'UseExplicitRowColumnOnTile'.</para>
        /// </remarks>
        [AttachedPropertyBrowsableForChildren()]
        public static int GetColumn(DependencyObject elem)
		{
			return (int)elem.GetValue( ColumnProperty );
		}

        /// <summary>
        /// Sets the value of the Column attached property of the specified element, -1 indicates that the tile will be positioned relative to the previous tile in the panel. The default value is 0.
        /// </summary>
        /// <param name="elem">This element's Column value will be set.</param>
		/// <param name="value">Value to set. This can be -1 which will position the element relative to previous element in the panel.</param>
        /// <remarks>
        /// <para class="note"><b>Note:</b> this property is ignored unless <see cref="XamTilesControl"/>.<see cref="XamTilesControl.NormalModeSettings"/>.<see cref="Infragistics.Windows.Tiles.NormalModeSettings.TileLayoutOrder"/> is set to 'UseExplicitRowColumnOnTile'.</para>
        /// </remarks>
        public static void SetColumn(DependencyObject elem, int value)
		{
			elem.SetValue( ColumnProperty, value );
		}

		        #endregion // Column

		        #region ColumnSpan

		/// <summary>
		/// Identifies the ColumnSpan attached dependency property.
		/// </summary>
		public static readonly DependencyProperty ColumnSpanProperty = TilesPanel.ColumnSpanProperty.AddOwner(typeof(XamTilesControl));

        /// <summary>
        /// Gets the value of the ColumnSpan attached property of the specified element, 0 indicates
        /// that the element will occupy the remainder of the space in its logical column. The default is 1. 
        /// </summary>
        /// <param name="elem">This element's ColumnSpan value will be returned.</param>
        /// <returns>The value of the ColumnSpan attached property. The default value is 1.</returns>
        /// <remarks>
        /// <para class="note"><b>Note:</b> this property is ignored unless <see cref="XamTilesControl"/>.<see cref="XamTilesControl.NormalModeSettings"/>.<see cref="Infragistics.Windows.Tiles.NormalModeSettings.TileLayoutOrder"/> is set to 'UseExplicitRowColumnOnTile'.</para>
        /// </remarks>
        [AttachedPropertyBrowsableForChildren()]
        public static int GetColumnSpan(DependencyObject elem)
		{
			return (int)elem.GetValue( ColumnSpanProperty );
		}

        /// <summary>
        /// Sets the value of the ColumnSpan attached property of the specified element, 0 indicates
        /// that the element will occupy the remainder of the space in its logical column. The default is 1. 
        /// </summary>
        /// <param name="elem">This element's ColumnSpan value will be set.</param>
		/// <param name="value">Value to set. This can be 0 to indicate that the element should occupy the remainder of the logical column.</param>
        /// <remarks>
        /// <para class="note"><b>Note:</b> this property is ignored unless <see cref="XamTilesControl"/>.<see cref="XamTilesControl.NormalModeSettings"/>.<see cref="Infragistics.Windows.Tiles.NormalModeSettings.TileLayoutOrder"/> is set to 'UseExplicitRowColumnOnTile'.</para>
        /// </remarks>
        public static void SetColumnSpan(DependencyObject elem, int value)
		{
			elem.SetValue( ColumnSpanProperty, value );
		}

		        #endregion // ColumnSpan

                #region ColumnWeight

        /// <summary>
        /// Identifies the ColumnWeight attached dependency property
        /// </summary>
        /// <seealso cref="GetColumnWeight"/>
        /// <seealso cref="SetColumnWeight"/>
        public static readonly DependencyProperty ColumnWeightProperty = TilesPanel.ColumnWeightProperty.AddOwner(typeof(XamTilesControl));
        /// <summary>
        /// Gets the value of the ColumnWeight attached property of the specified element. ColumnWeight specifies
        /// how any extra width will be distributed among elements.
        /// </summary>
         /// <returns>The value of the ColumnWeight attached property.</returns>
        /// <seealso cref="ColumnWeightProperty"/>
        /// <seealso cref="SetColumnWeight"/>
        [AttachedPropertyBrowsableForChildren()]
        public static float GetColumnWeight(DependencyObject d)
        {
            return (float)d.GetValue(TilesPanel.ColumnWeightProperty);
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
            d.SetValue(TilesPanel.ColumnWeightProperty, value);
        }

                #endregion //ColumnWeight

                #region Constraints

        /// <summary>
        /// Identifies the Constraints attached dependency property
        /// </summary>
        /// <seealso cref="GetConstraints"/>
        /// <seealso cref="SetConstraints"/>
        public static readonly DependencyProperty ConstraintsProperty = TilesPanel.ConstraintsProperty.AddOwner(typeof(XamTilesControl));

        /// <summary>
        /// Gets the value of the 'Constraints' attached property which contains size constraints for tiles when their <see cref="Tile.State"/> is 'Normal'.
        /// </summary>
        /// <seealso cref="ConstraintsProperty"/>
        /// <seealso cref="SetConstraints"/>
        [AttachedPropertyBrowsableForChildren()]
        public static TileConstraints GetConstraints(DependencyObject d)
        {
            return (TileConstraints)d.GetValue(TilesPanel.ConstraintsProperty);
        }

        /// <summary>
        /// Sets the value of the 'Constraints' attached property which contains size constraints for tiles when their <see cref="Tile.State"/> is 'Normal'.
        /// </summary>
        /// <seealso cref="ConstraintsProperty"/>
        /// <seealso cref="GetConstraints"/>
        public static void SetConstraints(DependencyObject d, TileConstraints value)
        {
            d.SetValue(TilesPanel.ConstraintsProperty, value);
        }

                #endregion //Constraints

                #region ConstraintsMaximized

        /// <summary>
        /// Identifies the ConstraintsMaximized attached dependency property
        /// </summary>
        /// <seealso cref="GetConstraintsMaximized"/>
        /// <seealso cref="SetConstraintsMaximized"/>
        public static readonly DependencyProperty ConstraintsMaximizedProperty = TilesPanel.ConstraintsMaximizedProperty.AddOwner(typeof(XamTilesControl));

        /// <summary>
        /// Gets the value of the 'ConstraintsMaximized' attached property which contains size constraints for tiles when their <see cref="Tile.State"/> is 'Maximized'.
        /// </summary>
        /// <seealso cref="ConstraintsMaximizedProperty"/>
        /// <seealso cref="SetConstraintsMaximized"/>
        [AttachedPropertyBrowsableForChildren()]
        public static TileConstraints GetConstraintsMaximized(DependencyObject d)
        {
            return (TileConstraints)d.GetValue(TilesPanel.ConstraintsMaximizedProperty);
        }

        /// <summary>
        /// Sets the value of the 'ConstraintsMaximized' attached property which contains size constraints for tiles when their <see cref="Tile.State"/> is 'Maximized'.
        /// </summary>
        /// <seealso cref="ConstraintsMaximizedProperty"/>
        /// <seealso cref="GetConstraintsMaximized"/>
        public static void SetConstraintsMaximized(DependencyObject d, TileConstraints value)
        {
            d.SetValue(TilesPanel.ConstraintsMaximizedProperty, value);
        }

                #endregion //ConstraintsMaximized

                #region ConstraintsMinimized

        /// <summary>
        /// Identifies the ConstraintsMinimized attached dependency property
        /// </summary>
        /// <seealso cref="GetConstraintsMinimized"/>
        /// <seealso cref="SetConstraintsMinimized"/>
        public static readonly DependencyProperty ConstraintsMinimizedProperty = TilesPanel.ConstraintsMinimizedProperty.AddOwner(typeof(XamTilesControl));

        /// <summary>
        /// Gets the value of the 'ConstraintsMinimized' attached property which contains size constraints for tiles when their <see cref="Tile.State"/> is 'Minimized'.
        /// </summary>
        /// <seealso cref="ConstraintsMinimizedProperty"/>
        /// <seealso cref="SetConstraintsMinimized"/>
        [AttachedPropertyBrowsableForChildren()]
        public static TileConstraints GetConstraintsMinimized(DependencyObject d)
        {
            return (TileConstraints)d.GetValue(TilesPanel.ConstraintsMinimizedProperty);
        }

        /// <summary>
        /// Sets the value of the 'ConstraintsMinimized' attached property which contains size constraints for tiles when their <see cref="Tile.State"/> is 'Minimized'.
        /// </summary>
        /// <seealso cref="ConstraintsMinimizedProperty"/>
        /// <seealso cref="GetConstraintsMinimized"/>
        public static void SetConstraintsMinimized(DependencyObject d, TileConstraints value)
        {
            d.SetValue(TilesPanel.ConstraintsMinimizedProperty, value);
        }

                #endregion //ConstraintsMinimized

                #region ConstraintsMinimizedExpanded

        /// <summary>
        /// Identifies the ConstraintsMinimizedExpanded attached dependency property
        /// </summary>
        /// <seealso cref="GetConstraintsMinimizedExpanded"/>
        /// <seealso cref="SetConstraintsMinimizedExpanded"/>
        public static readonly DependencyProperty ConstraintsMinimizedExpandedProperty = TilesPanel.ConstraintsMinimizedExpandedProperty.AddOwner(typeof(XamTilesControl));

        /// <summary>
        /// Gets the value of the 'ConstraintsMinimizedExpanded' attached property which contains size constraints for tiles when their <see cref="Tile.State"/> is 'MinimizedExpanded'.
        /// </summary>
        /// <seealso cref="ConstraintsMinimizedExpandedProperty"/>
        /// <seealso cref="SetConstraintsMinimizedExpanded"/>
        [AttachedPropertyBrowsableForChildren()]
        public static TileConstraints GetConstraintsMinimizedExpanded(DependencyObject d)
        {
            return (TileConstraints)d.GetValue(TilesPanel.ConstraintsMinimizedExpandedProperty);
        }

        /// <summary>
        /// Sets the value of the 'ConstraintsMinimizedExpanded' attached property which contains size constraints for tiles when their <see cref="Tile.State"/> is 'MinimizedExpanded'.
        /// </summary>
        /// <seealso cref="ConstraintsMinimizedExpandedProperty"/>
        /// <seealso cref="GetConstraintsMinimizedExpanded"/>
        public static void SetConstraintsMinimizedExpanded(DependencyObject d, TileConstraints value)
        {
            d.SetValue(TilesPanel.ConstraintsMinimizedExpandedProperty, value);
        }

                #endregion //ConstraintsMinimizedExpanded

		        #region Row

		/// <summary>
		/// Identifies the Row attached dependency property.
		/// </summary>
        public static readonly DependencyProperty RowProperty = TilesPanel.RowProperty.AddOwner(typeof(XamTilesControl));

        /// <summary>
        /// Gets the value of the Row attached property of the specified element, -1 indicates that the tile will be positioned relative to the previous tile in the panel. The default value is 0. 
        /// </summary>
        /// <param name="elem">This element's Row value will be returned.</param>
        /// <returns>The value of the Row attached property. The default value is 0.</returns>
        /// <remarks>
        /// <para class="note"><b>Note:</b> this property is ignored unless <see cref="XamTilesControl"/>.<see cref="XamTilesControl.NormalModeSettings"/>.<see cref="Infragistics.Windows.Tiles.NormalModeSettings.TileLayoutOrder"/> is set to 'UseExplicitRowColumnOnTile'.</para>
        /// </remarks>
        [AttachedPropertyBrowsableForChildren()]
		public static int GetRow( DependencyObject elem )
		{
			return (int)elem.GetValue( RowProperty );
		}

        /// <summary>
        /// Sets the value of the Row attached property of the specified element, -1 indicates that the tile will be positioned relative to the previous tile in the panel. The default value is 0.
        /// </summary>
        /// <param name="elem">This element's Row value will be set.</param>
		/// <param name="value">Value to set. This can be -1 which will position the element relative to previous element in the panel.</param>
        /// <remarks>
        /// <para class="note"><b>Note:</b> this property is ignored unless <see cref="XamTilesControl"/>.<see cref="XamTilesControl.NormalModeSettings"/>.<see cref="Infragistics.Windows.Tiles.NormalModeSettings.TileLayoutOrder"/> is set to 'UseExplicitRowColumnOnTile'.</para>
        /// </remarks>
        public static void SetRow(DependencyObject elem, int value)
		{
			elem.SetValue( RowProperty, value );
		}

		        #endregion // Row

		        #region RowSpan

		/// <summary>
		/// Identifies the RowSpan attached dependency property.
		/// </summary>
        public static readonly DependencyProperty RowSpanProperty = TilesPanel.RowSpanProperty.AddOwner(typeof(XamTilesControl));

        /// <summary>
        /// Gets the value of the RowSpan attached property of the specified element, 0 indicates
        /// that the element will occupy the remainder of the space in its logical column. The default is 1. 
        /// </summary>
        /// <param name="elem">This element's RowSpan value will be returned.</param>
        /// <returns>The value of the RowSpan attached property. The default value is 1.</returns>
        /// <remarks>
        /// <para class="note"><b>Note:</b> this property is ignored unless <see cref="XamTilesControl"/>.<see cref="XamTilesControl.NormalModeSettings"/>.<see cref="Infragistics.Windows.Tiles.NormalModeSettings.TileLayoutOrder"/> is set to 'UseExplicitRowColumnOnTile'.</para>
        /// </remarks>
        [AttachedPropertyBrowsableForChildren()]
        public static int GetRowSpan(DependencyObject elem)
		{
			return (int)elem.GetValue( RowSpanProperty );
		}

		/// <summary>
		/// Sets the value of the RowSpan attached property of the specified element. Default value is 0, which indicates
		/// that the element will occupy the remainder of the space in its logical row.
		/// </summary>
		/// <param name="elem">This element's RowSpan value will be set.</param>
		/// <param name="value">Value to set. This can be 0 to indicate that the element should occupy the remainder of the logical row.</param>
        /// <remarks>
        /// <para class="note"><b>Note:</b> this property is ignored unless <see cref="XamTilesControl"/>.<see cref="XamTilesControl.NormalModeSettings"/>.<see cref="Infragistics.Windows.Tiles.NormalModeSettings.TileLayoutOrder"/> is set to 'UseExplicitRowColumnOnTile'.</para>
        /// </remarks>
        public static void SetRowSpan(DependencyObject elem, int value)
		{
			elem.SetValue( RowSpanProperty, value );
		}

		        #endregion // RowSpan

                #region RowWeight

        /// <summary>
        /// Identifies the RowWeight attached dependency property
        /// </summary>
        /// <seealso cref="GetRowWeight"/>
        /// <seealso cref="SetRowWeight"/>
        public static readonly DependencyProperty RowWeightProperty = TilesPanel.RowWeightProperty.AddOwner(typeof(XamTilesControl));

        /// <summary>
        /// Gets the value of the RowWeight attached property of the specified element. RowWeight specifies
        /// how any extra height will be distributed among elements.
        /// </summary>
        /// <returns>The value of the RowWeight attached property.</returns>
        /// <seealso cref="RowWeightProperty"/>
        /// <seealso cref="SetRowWeight"/>
        [AttachedPropertyBrowsableForChildren()]
        public static float GetRowWeight(DependencyObject d)
        {
            return (float)d.GetValue(TilesPanel.RowWeightProperty);
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
            d.SetValue(TilesPanel.RowWeightProperty, value);
        }

                #endregion //RowWeight

                #region SerializationId

        /// <summary>
        /// Identifies the SerializationId attached dependency property
        /// </summary>
        /// <seealso cref="GetSerializationId"/>
        /// <seealso cref="SetSerializationId"/>
        public static readonly DependencyProperty SerializationIdProperty = TilesPanel.SerializationIdProperty.AddOwner(typeof(XamTilesControl));

        /// <summary>
        /// Gets the value of the 'SerializationId' attached property
        /// </summary>
        /// <seealso cref="SerializationIdProperty"/>
        /// <seealso cref="SetSerializationId"/>
        public static string GetSerializationId(DependencyObject d)
        {
            return (string)d.GetValue(TilesPanel.SerializationIdProperty);
        }

        /// <summary>
        /// Sets the value of the 'SerializationId' attached property
        /// </summary>
        /// <seealso cref="SerializationIdProperty"/>
        /// <seealso cref="GetSerializationId"/>
        public static void SetSerializationId(DependencyObject d, string value)
        {
            d.SetValue(TilesPanel.SerializationIdProperty, value);
        }

                #endregion //SerializationId

            #endregion //Public Attached Properties

            #region Public Properties

        
#region Infragistics Source Cleanup (Region)















































#endregion // Infragistics Source Cleanup (Region)

                #region HeaderPath

        /// <summary>
        /// Identifies the <see cref="HeaderPath"/> dependency property
        /// </summary>
        public static readonly DependencyProperty HeaderPathProperty = DependencyProperty.Register("HeaderPath",
            typeof(string), typeof(XamTilesControl), new FrameworkPropertyMetadata(null));

        private static void OnHeaderPathChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            XamTilesControl control = target as XamTilesControl;

            control._headerPath = (string)e.NewValue;
        }

        /// <summary>
        /// Gets/sets a path to a value on the source object that will be used to initialize the Header of each <see cref="Tile"/>. 
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> this setting is ignored for <see cref="Tile"/>'s that are direct children of the XamTilesControl. This setting only has meaning for <see cref="Tile"/>s that get generated as item containers.</para>
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
                this.SetValue(XamTilesControl.HeaderPathProperty, value);
            }
        }

                #endregion //HeaderPath

                #region InterTileAreaSpacingResolved

        /// <summary>
        /// Identifies the <see cref="InterTileAreaSpacingResolved"/> dependency property
        /// </summary>
        public static readonly DependencyProperty InterTileAreaSpacingResolvedProperty 
            = TilesPanel.InterTileAreaSpacingResolvedProperty.AddOwner(typeof(XamTilesControl));

        private static void OnInterTileAreaSpacingResolvedChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            XamTilesControl control = target as XamTilesControl;

            control._interTileAreaSpacingResolved = (double)e.NewValue;
        }

        /// <summary>
        /// Returns the resolved value that will be used for spacing between the maximized tile area and the minimized tile area when is maximized mode.
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> if the <see cref="Infragistics.Windows.Tiles.MaximizedModeSettings.InterTileAreaSpacing"/> property is not set then the <see cref="InterTileSpacingXMaximizedResolved"/> or <see cref="InterTileSpacingYMaximizedResolved"/> setting will be used. If this also is not set then the <see cref="InterTileSpacingX"/> or <see cref="InterTileSpacingY"/> value will be used based on the <see cref="MaximizedTileLocation"/>.</para>
        /// </remarks>
        /// <seealso cref="MaximizedModeSettings"/>
        /// <seealso cref="Infragistics.Windows.Tiles.MaximizedModeSettings.InterTileAreaSpacing"/>
        /// <seealso cref="XamTilesControl.IsInMaximizedMode"/>
        /// <seealso cref="XamTilesControl.MaximizedItems"/>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [ReadOnly(true)]
        public double InterTileAreaSpacingResolved { get { return this._interTileAreaSpacingResolved; } }

                #endregion //InterTileAreaSpacingResolved	

                #region InterTileSpacingX

        /// <summary>
        /// Identifies the <see cref="InterTileSpacingX"/> dependency property
        /// </summary>
        public static readonly DependencyProperty InterTileSpacingXProperty
            = TilesPanel.InterTileSpacingXProperty.AddOwner(typeof(XamTilesControl),
                                new FrameworkPropertyMetadata(new PropertyChangedCallback(OnInterTileSpacingXChanged)));

        private static void OnInterTileSpacingXChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            XamTilesControl control = target as XamTilesControl;

            control._interTileSpacingX = (double)e.NewValue;

			// JJD 4/28/11 - TFS73523 - added 
			control.InitializeSpacingOnResizeController();

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
                this.SetValue(XamTilesControl.InterTileSpacingXProperty, value);
            }
        }

                #endregion //InterTileSpacingX	

                #region InterTileSpacingXMaximizedResolved

        /// <summary>
        /// Identifies the <see cref="InterTileSpacingXMaximizedResolved"/> dependency property
        /// </summary>
        public static readonly DependencyProperty InterTileSpacingXMaximizedResolvedProperty 
            = TilesPanel.InterTileSpacingXMaximizedResolvedProperty.AddOwner(typeof(XamTilesControl));

        private static void OnInterTileSpacingXMaximizedResolvedChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            XamTilesControl control = target as XamTilesControl;

            control._interTileSpacingXMaximizedResolved = (double)e.NewValue;
		}

        /// <summary>
        /// Returns the resolved value that will be used for spacing between the maximized tiles horizontally.
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> if the <see cref="Infragistics.Windows.Tiles.MaximizedModeSettings.InterTileSpacingXMaximized"/> property is not set then the <see cref="InterTileSpacingX"/> value will be used.</para>
        /// </remarks>
        /// <seealso cref="InterTileSpacingX"/>
        /// <seealso cref="Infragistics.Windows.Tiles.MaximizedModeSettings.InterTileSpacingXMaximized"/>
        /// <seealso cref="XamTilesControl.IsInMaximizedMode"/>
        /// <seealso cref="XamTilesControl.MaximizedItems"/>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [ReadOnly(true)]
        public double InterTileSpacingXMaximizedResolved { get { return this._interTileSpacingXMaximizedResolved; } }

                #endregion //InterTileSpacingXMaximizedResolved	

                #region InterTileSpacingXMinimizedResolved

        /// <summary>
        /// Identifies the <see cref="InterTileSpacingXMinimizedResolved"/> dependency property
        /// </summary>
        public static readonly DependencyProperty InterTileSpacingXMinimizedResolvedProperty
            = TilesPanel.InterTileSpacingXMinimizedResolvedProperty.AddOwner(typeof(XamTilesControl));

        private static void OnInterTileSpacingXMinimizedResolvedChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            XamTilesControl control = target as XamTilesControl;

            control._interTileSpacingXMinimizedResolved = (double)e.NewValue;
		}

        /// <summary>
        /// Returns the resolved value that will be used for spacing between the minimized tiles horizontally.
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> if the <see cref="Infragistics.Windows.Tiles.MaximizedModeSettings.InterTileSpacingXMinimized"/> property is not set then the <see cref="InterTileSpacingX"/> value will be used.</para>
        /// </remarks>
        /// <seealso cref="InterTileSpacingX"/>
        /// <seealso cref="Infragistics.Windows.Tiles.MaximizedModeSettings.InterTileSpacingXMaximized"/>
        /// <seealso cref="XamTilesControl.IsInMaximizedMode"/>
        /// <seealso cref="XamTilesControl.MaximizedItems"/>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [ReadOnly(true)]
        public double InterTileSpacingXMinimizedResolved { get { return this._interTileSpacingXMinimizedResolved; } }

                #endregion //InterTileSpacingXMinimizedResolved

                #region InterTileSpacingY

        /// <summary>
        /// Identifies the <see cref="InterTileSpacingY"/> dependency property
        /// </summary>
        public static readonly DependencyProperty InterTileSpacingYProperty
            = TilesPanel.InterTileSpacingYProperty.AddOwner(typeof(XamTilesControl),
                                new FrameworkPropertyMetadata(new PropertyChangedCallback(OnInterTileSpacingYChanged)));

        private static void OnInterTileSpacingYChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            XamTilesControl control = target as XamTilesControl;

            control._interTileSpacingY = (double)e.NewValue;

			// JJD 4/28/11 - TFS73523 - added 
			control.InitializeSpacingOnResizeController();
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
                this.SetValue(XamTilesControl.InterTileSpacingYProperty, value);
            }
        }

                #endregion //InterTileSpacingY	

                #region InterTileSpacingYMaximizedResolved

        /// <summary>
        /// Identifies the <see cref="InterTileSpacingYMaximizedResolved"/> dependency property
        /// </summary>
        public static readonly DependencyProperty InterTileSpacingYMaximizedResolvedProperty
            = TilesPanel.InterTileSpacingYMaximizedResolvedProperty.AddOwner(typeof(XamTilesControl));

        private static void OnInterTileSpacingYMaximizedResolvedChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            XamTilesControl control = target as XamTilesControl;

            control._interTileSpacingYMaximizedResolved = (double)e.NewValue;
		}

        /// <summary>
        /// Returns the resolved value that will be used for spacing between the maximized tiles vertically.
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> if the <see cref="Infragistics.Windows.Tiles.MaximizedModeSettings.InterTileSpacingYMaximized"/> property is not set then the <see cref="InterTileSpacingY"/> value will be used.</para>
        /// </remarks>
        /// <seealso cref="InterTileSpacingY"/>
        /// <seealso cref="Infragistics.Windows.Tiles.MaximizedModeSettings.InterTileSpacingYMaximized"/>
        /// <seealso cref="XamTilesControl.IsInMaximizedMode"/>
        /// <seealso cref="XamTilesControl.MaximizedItems"/>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [ReadOnly(true)]
         public double InterTileSpacingYMaximizedResolved { get { return this._interTileSpacingYMaximizedResolved; } }

                #endregion //InterTileSpacingYMaximizedResolved	

                #region InterTileSpacingYMinimizedResolved

        /// <summary>
        /// Identifies the <see cref="InterTileSpacingYMinimizedResolved"/> dependency property
        /// </summary>
        public static readonly DependencyProperty InterTileSpacingYMinimizedResolvedProperty
            = TilesPanel.InterTileSpacingYMinimizedResolvedProperty.AddOwner(typeof(XamTilesControl));

        private static void OnInterTileSpacingYMinimizedResolvedChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            XamTilesControl control = target as XamTilesControl;

            control._interTileSpacingYMinimizedResolved = (double)e.NewValue;
		}

        /// <summary>
        /// Returns the resolved value that will be used for spacing between the minimized tiles vertically.
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> if the <see cref="Infragistics.Windows.Tiles.MaximizedModeSettings.InterTileSpacingYMinimized"/> property is not set then the <see cref="InterTileSpacingY"/> value will be used.</para>
        /// </remarks>
        /// <seealso cref="InterTileSpacingY"/>
        /// <seealso cref="Infragistics.Windows.Tiles.MaximizedModeSettings.InterTileSpacingYMaximized"/>
        /// <seealso cref="XamTilesControl.IsInMaximizedMode"/>
        /// <seealso cref="XamTilesControl.MaximizedItems"/>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [ReadOnly(true)]
        public double InterTileSpacingYMinimizedResolved { get { return this._interTileSpacingYMinimizedResolved; } }

                #endregion //InterTileSpacingYMinimizedResolved

				// JJD 4/19/11 - TFS73129  added
				#region IsAnimationInProgress

		private static readonly DependencyPropertyKey IsAnimationInProgressPropertyKey =
			DependencyProperty.RegisterReadOnly("IsAnimationInProgress",
			typeof(bool), typeof(XamTilesControl), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, OnIsAnimationInProgressChanged));

		private static void OnIsAnimationInProgressChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			XamTilesControl instance = target as XamTilesControl;

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

			if ( animationsInProgress != _isAnimationInProgress )
				this.SetValue(IsAnimationInProgressPropertyKey, KnownBoxes.FromValue(animationsInProgress));

		}

		/// <summary>
		/// Identifies the <see cref="IsAnimationInProgress"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsAnimationInProgressProperty =
			IsAnimationInProgressPropertyKey.DependencyProperty;

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

        /// <summary>
        /// Identifies the <see cref="IsInMaximizedMode"/> dependency property
        /// </summary>
        public static readonly DependencyProperty IsInMaximizedModeProperty = TilesPanel.IsInMaximizedModeProperty.AddOwner(typeof(XamTilesControl));

        private static void OnIsInMaximizedModeChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            XamTilesControl tc = target as XamTilesControl;

            tc._splitterMinExtent = 0;
            tc.InitializeSplitter();

        }

        /// <summary>
        /// Returns true if there is at least one <see cref="Tile"/> whose <see cref="Tile.State"/> is 'Maximized'. (read-only)
        /// </summary>
        /// <seealso cref="IsInMaximizedModeProperty"/>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [ReadOnly(true)]
        public bool IsInMaximizedMode
        {
            get
            {
                return (bool)this.GetValue(TilesPanel.IsInMaximizedModeProperty);
            }
        }

                #endregion //IsInMaximizedMode

                #region ItemHeaderTemplate

        /// <summary>
        /// Identifies the <see cref="ItemHeaderTemplate"/> dependency property
        /// </summary>
        public static readonly DependencyProperty ItemHeaderTemplateProperty = DependencyProperty.Register("ItemHeaderTemplate",
            typeof(DataTemplate), typeof(XamTilesControl), new FrameworkPropertyMetadata(null));

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
                return (DataTemplate)this.GetValue(XamTilesControl.ItemHeaderTemplateProperty);
            }
            set
            {
                this.SetValue(XamTilesControl.ItemHeaderTemplateProperty, value);
            }
        }

                #endregion //ItemHeaderTemplate

                #region ItemHeaderTemplateSelector

        /// <summary>
        /// Identifies the <see cref="ItemHeaderTemplateSelector"/> dependency property
        /// </summary>
        public static readonly DependencyProperty ItemHeaderTemplateSelectorProperty = DependencyProperty.Register("ItemHeaderTemplateSelector",
            typeof(DataTemplateSelector), typeof(XamTilesControl), new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Gets/sets the HeaderTemplateSelector that will be set on an item's Tile to allow the application writer to supply logic in choosing a headet template for each item's tile.
        /// </summary>
        /// <seealso cref="ItemHeaderTemplateSelectorProperty"/>
        //[Description("Gets/sets the HeaderTemplateSelector that will be set on an item's Tile.")]
        //[Category("TilesControl Properties")]
        public DataTemplateSelector ItemHeaderTemplateSelector
        {
            get
            {
                return (DataTemplateSelector)this.GetValue(XamTilesControl.ItemHeaderTemplateSelectorProperty);
            }
            set
            {
                this.SetValue(XamTilesControl.ItemHeaderTemplateSelectorProperty, value);
            }
        }

                #endregion //ItemHeaderTemplateSelector

                #region ItemTemplateMaximized

        /// <summary>
        /// Identifies the <see cref="ItemTemplateMaximized"/> dependency property
        /// </summary>
        public static readonly DependencyProperty ItemTemplateMaximizedProperty = DependencyProperty.Register("ItemTemplateMaximized",
            typeof(DataTemplate), typeof(XamTilesControl), new FrameworkPropertyMetadata(null));

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
                return (DataTemplate)this.GetValue(XamTilesControl.ItemTemplateMaximizedProperty);
            }
            set
            {
                this.SetValue(XamTilesControl.ItemTemplateMaximizedProperty, value);
            }
        }

                #endregion //ItemTemplateMaximized

                #region ItemTemplateMinimized

        /// <summary>
        /// Identifies the <see cref="ItemTemplateMinimized"/> dependency property
        /// </summary>
        public static readonly DependencyProperty ItemTemplateMinimizedProperty = DependencyProperty.Register("ItemTemplateMinimized",
            typeof(DataTemplate), typeof(XamTilesControl), new FrameworkPropertyMetadata(null));

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
                return (DataTemplate)this.GetValue(XamTilesControl.ItemTemplateMinimizedProperty);
            }
            set
            {
                this.SetValue(XamTilesControl.ItemTemplateMinimizedProperty, value);
            }
        }

                #endregion //ItemTemplateMinimized

                #region ItemTemplateMinimizedExpanded

        /// <summary>
        /// Identifies the <see cref="ItemTemplateMinimizedExpanded"/> dependency property
        /// </summary>
        public static readonly DependencyProperty ItemTemplateMinimizedExpandedProperty = DependencyProperty.Register("ItemTemplateMinimizedExpanded",
            typeof(DataTemplate), typeof(XamTilesControl), new FrameworkPropertyMetadata(null));

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
                return (DataTemplate)this.GetValue(XamTilesControl.ItemTemplateMinimizedExpandedProperty);
            }
            set
            {
                this.SetValue(XamTilesControl.ItemTemplateMinimizedExpandedProperty, value);
            }
        }

                #endregion //ItemTemplateMinimizedExpanded

                #region MaximizedModeSettings

        /// <summary>
        /// Identifies the <see cref="MaximizedModeSettings"/> dependency property
        /// </summary>
        public static readonly DependencyProperty MaximizedModeSettingsProperty = TilesPanel.MaximizedModeSettingsProperty.AddOwner(typeof(XamTilesControl));

        /// <summary>
        /// Gets/sets the settings that are used to layout Tiles when in maximized mode
        /// </summary>
        /// <seealso cref="IsInMaximizedMode"/>
        /// <seealso cref="MaximizedModeSettingsProperty"/>
        //[Description("Gets/sets the settings that are used to layout Tiles when in maximized mode")]
        //[Category("TilesControl Properties")]
        [RefreshProperties(RefreshProperties.Repaint)]
        public MaximizedModeSettings MaximizedModeSettings
        {
            get
            {
                return (MaximizedModeSettings)this.GetValue(XamTilesControl.MaximizedModeSettingsProperty);
            }
            set
            {
                this.SetValue(XamTilesControl.MaximizedModeSettingsProperty, value);
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
        public static readonly DependencyProperty MaximizedTileLimitProperty = TilesPanel.MaximizedTileLimitProperty.AddOwner( typeof(XamTilesControl));

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

                #region NormalModeSettings

        /// <summary>
        /// Identifies the <see cref="NormalModeSettings"/> dependency property
        /// </summary>
        public static readonly DependencyProperty NormalModeSettingsProperty = TilesPanel.NormalModeSettingsProperty.AddOwner(typeof(XamTilesControl));

        /// <summary>
        /// Gets/sets the settings that are used to layout Tiles when not in maximized mode
        /// </summary>
        /// <seealso cref="IsInMaximizedMode"/>
        /// <seealso cref="NormalModeSettingsProperty"/>
        //[Description("Gets/sets the settings that are used to layout Tiles when not in maximized mode")]
        //[Category("TilesControl Properties")]
        [RefreshProperties(RefreshProperties.Repaint)]
        public NormalModeSettings NormalModeSettings
        {
            get
            {
                return (NormalModeSettings)this.GetValue(XamTilesControl.NormalModeSettingsProperty);
            }
            set
            {
                this.SetValue(XamTilesControl.NormalModeSettingsProperty, value);
            }
        }

                #endregion //NormalModeSettings

                #region SerializationIdPath

        /// <summary>
        /// Identifies the <see cref="SerializationIdPath"/> dependency property
        /// </summary>
        public static readonly DependencyProperty SerializationIdPathProperty = DependencyProperty.Register("SerializationIdPath",
            typeof(string), typeof(XamTilesControl), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnSerializationIdPathChanged)));

        private static void OnSerializationIdPathChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            XamTilesControl control = target as XamTilesControl;

            control._serializationIdPath = (string)e.NewValue;
        }

        /// <summary>
        /// Gets/sets a path to a value on the source object that will be used to initialize the <see cref="Tile.SerializationId"/> of each <see cref="Tile"/>. 
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> this setting is ignored for <see cref="Tile"/>'s that are direct children of the XamTilesControl. This setting only has meaning for <see cref="Tile"/>s that get generated as item containers.</para>
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
                this.SetValue(XamTilesControl.SerializationIdPathProperty, value);
            }
        }

                #endregion //SerializationIdPath

                #region Theme

        /// <summary>
        /// Identifies the 'Theme' dependency property
        /// </summary>
        public static readonly DependencyProperty ThemeProperty = ThemeManager.ThemeProperty.AddOwner(typeof(XamTilesControl), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnThemeChanged)));

        /// <summary>
        /// Event ID for the 'ThemeChanged' routed event
        /// </summary>
        public static readonly RoutedEvent ThemeChangedEvent = ThemeManager.ThemeChangedEvent.AddOwner(typeof(XamTilesControl));

        private static void OnThemeChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            XamTilesControl control = target as XamTilesControl;

            control.UpdateThemeResources();
            control.OnThemeChanged((string)(e.OldValue), (string)(e.NewValue));
        }

        /// <summary>
        /// Gets/sets the default look for the control.
        /// </summary>
        /// <remarks>
        /// <para class="body">If left set to null then the default 'Generic' theme will be used. This property can 
        /// be set to the name of any registered theme (see <see cref="Infragistics.Windows.Themes.ThemeManager.Register(string, string, ResourceDictionary)"/> and <see cref="Infragistics.Windows.Themes.ThemeManager.GetThemes()"/> methods).</para>
        /// <para></para>
        /// <para class="body">The following themes are pre-registered by this assembly but additional themes can be registered as well.
        /// <ul>
        /// <li>"Aero" - a theme that is compatible with Vista's 'Aero' theme.</li>
        /// <li>"Generic" - the default theme.</li>
        /// <li>"LunaNormal" - a theme that is compatible with XP's 'blue' theme.</li>
        /// <li>"LunaOlive" - a theme that is compatible with XP's 'olive' theme.</li>
        /// <li>"LunaSilver" - a theme that is compatible with XP's 'silver' theme.</li>
        /// <li>"Office2k7Black" - a theme that is compatible with MS Office 2007's 'Black' theme.</li>
        /// <li>"Office2k7Blue" - a theme that is compatible with MS Office 2007's 'Blue' theme.</li>
        /// <li>"Office2k7Silver" - a theme that is compatible with MS Office 2007's 'Silver' theme.</li>
        /// <li>"Onyx" - a theme that features black and orange highlights.</li>
        /// <li>"Royale" - a theme that features subtle blue highlights.</li>
        /// <li>"RoyaleStrong" - a theme that features strong blue highlights.</li>
        /// </ul>
        /// </para>
        /// </remarks>
        /// <seealso cref="Infragistics.Windows.Themes.ThemeManager"/>
        /// <seealso cref="ThemeProperty"/>
        //[Description("Gets/sets the default look for the control.")]
        //[Category("TilesControl Properties")]
        [Bindable(true)]
        [DefaultValue((string)null)]
        [TypeConverter(typeof(Infragistics.Windows.Themes.Internal.TilesThemeTypeConverter))]
        public string Theme
        {
            get
            {
                return (string)this.GetValue(XamTilesControl.ThemeProperty);
            }
            set
            {
                this.SetValue(XamTilesControl.ThemeProperty, value);
            }
        }

        /// <summary>
        /// Called when property 'Theme' changes
        /// </summary>
        protected virtual void OnThemeChanged(string previousValue, string currentValue)
        {
            RoutedPropertyChangedEventArgs<string> newEvent = new RoutedPropertyChangedEventArgs<string>(previousValue, currentValue);
            newEvent.RoutedEvent = XamTilesControl.ThemeChangedEvent;
            newEvent.Source = this;
            RaiseEvent(newEvent);
        }

        /// <summary>
        /// Invoked when the 'Theme' property has been changed.
        /// </summary>
        //[Category("TilesControl Events")]
        //[Description("Invoked when the 'Theme' property has been changed.")]
        public event RoutedPropertyChangedEventHandler<string> ThemeChanged
        {
            add
            {
                base.AddHandler(XamTilesControl.ThemeChangedEvent, value);
            }
            remove
            {
                base.RemoveHandler(XamTilesControl.ThemeChangedEvent, value);
            }
        }

                #endregion //Theme

                #region TileAreaPadding

        /// <summary>
        /// Identifies the <see cref="TileAreaPadding"/> dependency property
        /// </summary>
        public static readonly DependencyProperty TileAreaPaddingProperty = TilesPanel.TileAreaPaddingProperty.AddOwner( typeof(XamTilesControl) );

        /// <summary>
        /// Get/sets that amount of space between the TilesPanel and the area where the tiles are arranged.
        /// </summary>
        /// <seealso cref="TileAreaPaddingProperty"/>
        //[Description("Get/sets that amount of space between the TilesPanel and the area where the tiles are arranged.")]
        //[Category("TilesControl Properties")]
        public Thickness TileAreaPadding
        {
            get
            {
                return (Thickness)this.GetValue(TilesPanel.TileAreaPaddingProperty);
            }
            set
            {
                this.SetValue(TilesPanel.TileAreaPaddingProperty, value);
            }
        }

                #endregion //TileAreaPadding

                #region TileCloseAction

        /// <summary>
        /// Identifies the <see cref="TileCloseAction"/> dependency property
        /// </summary>
        public static readonly DependencyProperty TileCloseActionProperty = TilesPanel.TileCloseActionProperty.AddOwner(typeof(XamTilesControl));

        /// <summary>
        /// Gets/sets whether Tiles can be closed by the user.
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note</b>: if TileCloseAction is set to 'DoNothing' (its default value) then, by default, <see cref="Tile"/>s can't be closed. 
        /// However, this behavior can be overidden for individual tiles by setting their <see cref="Tile.CloseAction"/> property.</para>
        /// </remarks>
        /// <seealso cref="TileCloseActionProperty"/>
        /// <seealso cref="Tile.CloseAction"/>
        /// <seealso cref="Tile.CloseButtonVisibility"/>
        /// <seealso cref="Tile.CloseButtonVisibilityResolved"/>
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

                #region ToolTipStyleKey

        /// <summary>
        /// The key used to identify the <see cref="Style"/> for <see cref="ToolTip"/> instances used in the XamTilesControl.
        /// </summary>
        public static readonly ResourceKey ToolTipStyleKey = new StaticPropertyResourceKey(typeof(XamTilesControl), "ToolTipStyleKey");

                #endregion //ToolTipStyleKey

            #endregion //Public Properties

            #region Internal Properties

                #region HasHwndHost

        internal bool HasHwndHost
        {
            get
            {
                if (this._panel != null)
                {
                    int count = VisualTreeHelper.GetChildrenCount(this._panel);

                    for (int i = 0; i < count; i++)
                    {
                        Tile tile = VisualTreeHelper.GetChild(this._panel, i) as Tile;

                        if (tile != null && tile.HasHwndHost)
                            return true;
                    }
                }

                return false;
            }
        }
                #endregion //HasHwndHost

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

                #region Manager

        internal TilesPanel.TileMgr Manager
        {
            get
            {
                return this._manager;
            }
        }

                #endregion //Manager	

                #region MaximizedItemsInternal

        internal ObservableCollectionExtended<object> MaximizedItemsInternal { get { return this._maximizedItems; } }

                #endregion //MaximizedItemsInternal	

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

                    if ( this._panel != null )
                        this._panel.InvalidateMeasure();
                }
            }
        }

                #endregion //MinimizedAreaExplicitExtentY	
    
                #region Panel

        internal TilesPanel Panel
        {
            get
            {
                if (this._panel == null)
                    this.FindPanel();

                return this._panel;
            }
        }

                #endregion //Panel

                #region ShowSplitterInPopup
        internal bool ShowSplitterInPopup
        {
            get
            {
                // if we have any hwndhosts then we need to show the flyout
                // in a toolwindow so that it is displayed above the hwnds
                // of those hwndhost controls
                if (this.HasHwndHost)
                {
                    if (BrowserInteropHelper.IsBrowserHosted)
                        return TileUtilities.HasUnmanagedCodeRights;

                    return true;
                }

                return false;
            }
        }
                #endregion //ShowSplitterInPopup
        
                #region ShowToolWindowsInPopup
        internal bool ShowToolWindowsInPopup
        {
            get
            {
                // if we have hwnd hosts within the dm then we may want to 
                // use a popup since that is a separate hwnd. otherwise the 
                // floating windows could be hosted within the adorner layer
                // which means they will be behind any hwndhosts
                if (this.HasHwndHost)
                {
                    // if we're hosted in a browser then we only want to 
                    // do this if we have unmanaged code rights. basically the 
                    // popup class does not manage its own z-order so the popup
                    // would fall behind hwndhosts which means its no better than
                    // putting it in the adorner layer. and having it in a popup 
                    // when its in the browser has the downside that the popup 
                    // doesn't support transparency when its a child window - which 
                    // it is when in a browser. when we have unmanaged code rights 
                    // though we can use apis to bring the popup to the front ourselves 
                    // so we can use it
                    if (BrowserInteropHelper.IsBrowserHosted)
                        return TileUtilities.HasUnmanagedCodeRights;
                }

                return false;
            }
        }
        #endregion //ShowToolWindowsInPopup

                #region SplitterMinExtent

        internal double SplitterMinExtent
        {
            get
            {
                //if (this._splitterMinExtent < 1)
                    this.InitializeSplitter();

                return this._splitterMinExtent;
            }
        }

                #endregion //SplitterMinExtent	
    
    
            #endregion //Internal Properties	
        
        #endregion //Properties

        #region Methods

            #region Public Methods

                #region GetItemInfo

        /// <summary>
        /// Gets the associated info for an item.
        /// </summary>
        /// <param name="item">The item in question.</param>
        /// <returns>The associated ItemInfo object that can be used to get/or set certain status information or null if the item doesn't exist in the Items collection.</returns>
        public ItemInfo GetItemInfo(object item)
        {
            if (item == null)
                return null;
            
            // JJD 05/07/10 - TFS31643
            // First call the GetItemInfo with false.
            // This will use a hash to find an existing item which is most
            // likely much faster than the IndexOf call below
            ItemInfo info = (ItemInfo)this._manager.GetItemInfo(item, false, -1);

            if (info != null)
                return info;

            int index = this.Items.IndexOf(item);

            if (index < 0)
                return null;

            return (ItemInfo)this._manager.GetItemInfo(item, true, index);
        }

                #endregion //GetItemInfo	
    
                #region LoadLayout

        /// <summary>
        /// Loads a layout that was saved with the <see cref="SaveLayout(Stream)"/> method.
        /// </summary>
        /// <param name="stream">The stream containing the saved layout.</param>
        public void LoadLayout(Stream stream)
        {
            LayoutManager.LoadLayout(this, stream);
        }

        /// <summary>
        /// Loads a layout saved with the <see cref="SaveLayout()"/> method.
        /// </summary>
        /// <param name="layout">The string containing the saved layout</param>
        public void LoadLayout(string layout)
        {
            LayoutManager.LoadLayout(this, layout);
        }
                #endregion //LoadLayout

                #region SaveLayout

        /// <summary>
        /// Saves information about the current arrangement of the panes to the specified stream.
        /// </summary>
        /// <exception cref="InvalidOperationException">The 'Name' property of a <see cref="Tile"/> whose <see cref="Tile.SaveInLayout"/> is true has not been set or is not unique with respect to the other content panes being saved.</exception>
        /// <param name="stream">The stream to which the layout should be saved.</param>
        public void SaveLayout(Stream stream)
        {
            LayoutManager.SaveLayout(this, stream);
        }

        /// <summary>
        /// Saves information about the current arrangement of the panes and returns that as a string.
        /// </summary>
        /// <exception cref="InvalidOperationException">The 'Name' property of a <see cref="Tile"/> whose <see cref="Tile.SaveInLayout"/> is true has not been set or is not unique with respect to the other content panes being saved.</exception>
        ///	<returns>A string that contains the layout information.</returns>
        public string SaveLayout()
        {
            return LayoutManager.SaveLayout(this);
        }
                #endregion //SaveLayout

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
                this.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new DispatcherOperationCallback(this.ProcessScrollIntoView), item);
        }

                #endregion //ScrollIntoView	
    
                #region TileFromItem

        /// <summary>
        /// Gets the associated tile for an item.
        /// </summary>
        /// <param name="item">The item in question.</param>
        /// <returns>The associated Tile or null if the Tile hasn't been generated.</returns>
        public Tile TileFromItem(object item)
        {
            Tile tile = item as Tile;

            if (tile != null)
                return tile;

            TilesPanel panel = this.Panel;

            if (panel == null)
                return null;

            return panel.TileFromItem(item);
        }

                #endregion //TileFromItem

            #endregion //Public Methods	

            #region Internal Methods

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
                    Tile tile = TileFromItem(item);

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

                #region GetItemInfoFromContainer

        internal ItemInfo GetItemInfoFromContainer(FrameworkElement container)
        {
            if ( container == null )
                return null;

            if (this._panel != null)
                return this.GetItemInfo(this._panel.ItemFromTile(container as Tile));

            object dataContext = container.DataContext;

            // If the DatContext wasn't set then assume the containe is the item
            return this.GetItemInfo(dataContext != null ? dataContext : container);
        }

                #endregion //GetItemInfoFromContainer	
    
                #region GetSerializationIdFromItem

        internal string GetSerializationIdFromItem(Tile tile, object item)
        {
            string serializationId = null;

            if (tile != null)
            {
                serializationId = tile.GetValue(Tile.SerializationIdProperty) as string;

                if (string.IsNullOrEmpty(serializationId))
                    serializationId = tile.Name;
            }
            else
            {
                string path = this.SerializationIdPath;
                if (!string.IsNullOrEmpty(path))
                {
                    string stringFormat = null;

                    if (XamTilesControl.s_ItemStringFormatProperty != null)
                        stringFormat = this.GetValue(XamTilesControl.s_ItemStringFormatProperty) as string;

                    // create a temp object to bind to
                    DependencyObject dpo = new DependencyObject();

                    TileUtilities.BindPathProperty(this, item, dpo, XamTilesControl.SerializationIdPathProperty,
                                                            Tile.SerializationIdProperty,
                                                            stringFormat);

                    serializationId = dpo.GetValue(Tile.SerializationIdProperty) as string;
                }
                else
                {
                    DependencyObject dpo = item as DependencyObject;

                    if (dpo != null)
                    {
                        serializationId = dpo.GetValue(Tile.SerializationIdProperty) as string;
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

                #region InitializeTilesPanel

        internal void InitializeTilesPanel(TilesPanel panel)
        {
            if (panel != this._panel)
            {
                if (this._panel != null)
                {
                    // unwire the CollectionChanged event
                    ((INotifyCollectionChanged)(this._panel.MaximizedItems)).CollectionChanged -= new NotifyCollectionChangedEventHandler(OnMaximizedItemsCollectionChanged);

                    // de-couple the old panel
                    this._panel.InitializeTilesControl(null);
                }

                this._panel = panel;

                if (this._panel != null)
                {
                    // wire the CollectionChanged event
                    ((INotifyCollectionChanged)(this._panel.MaximizedItems)).CollectionChanged += new NotifyCollectionChangedEventHandler(OnMaximizedItemsCollectionChanged);

                    // initialize the new panel 
                    this._panel.InitializeTilesControl(this);
                }

                this.SynchronizeMaximizedItemsCollection();
            }
        }

                #endregion //InitializeTilesPanel

            #endregion //Internal Methods	
        
            #region Private Methods

                #region BindPathProperty

        private bool BindPathProperty(object item, Tile tile, DependencyProperty dpPath, DependencyProperty dpTarget, string stringFormat)
        {
            return TileUtilities.BindPathProperty(this, item, tile, dpPath, dpTarget, stringFormat);
        }

                #endregion //BindPathProperty	
    
				// JJD 5/9/11 - TFS74206 - added 
                #region BindTilePropertyIfSpecified

        private static void BindTilePropertyIfSpecified(DependencyObject item, Tile tile, DependencyProperty dp)
        {
			if (item == null)
				return;

			ValueSource source = DependencyPropertyHelper.GetValueSource(item, dp);

			if (source.BaseValueSource == BaseValueSource.Default)
				return;

			Binding binding = null;

			if (_CachedBindings == null)
				_CachedBindings = new Dictionary<DependencyProperty, Binding>();
			else
				_CachedBindings.TryGetValue(dp, out binding);

			if (binding == null)
			{
				binding = new Binding();
				binding.Path = new PropertyPath(dp);
				binding.Mode = BindingMode.OneWay;
				_CachedBindings[dp] = binding;
			}

			tile.SetBinding(dp, binding);
        }

                #endregion //BindTilePropertyIfSpecified	
    
                #region FindPanel

        private void FindPanel()
        {
			Utilities.DependencyObjectSearchCallback<TilesPanel> callback = new Utilities.DependencyObjectSearchCallback<TilesPanel>(delegate(TilesPanel tilesPanel)
			{
				return this == ItemsControl.GetItemsOwner(tilesPanel);
			});

            // Look for our Panel and cache a reference to it.
            this.InitializeTilesPanel( Utilities.GetDescendantFromType<TilesPanel>(this, true, callback) );
        }

                #endregion //FindPanel

				#region InitializeSpacingOnResizeController

		// JJD 4/28/11 - TFS73523 - added 
		private void InitializeSpacingOnResizeController()
		{
			this._resizeController.InterItemSpacingX = this.InterTileSpacingX;
			this._resizeController.InterItemSpacingY = this.InterTileSpacingY;
		}

				#endregion //InitializeSpacingOnResizeController	
    
                #region InitializeSplitter

        private void InitializeSplitter()
        {
            if ( this._panel == null || !this.IsInitialized)
                return;

            MaximizedModeSettings settings = this._panel.MaximizedModeSettingsSafe;

            if (this.IsInMaximizedMode && settings.ShowTileAreaSplitter)
            {
                if (this._splitter == null)
                {
                    this._splitter = new TileAreaSplitter(this);
                    this.AddVisualChild(this._splitter);
                    this.AddLogicalChild(this._splitter);
                    this._extraVisualChildren.Add(this._splitter);
                }

                Orientation orientation;
                switch (settings.MaximizedTileLocation)
                {
                    case MaximizedTileLocation.Bottom:
                    case MaximizedTileLocation.Top:
                        orientation = Orientation.Horizontal;
                        break;
                    default:
                        orientation = Orientation.Vertical;
                        break;
                }

                this._splitter.Orientation = orientation;

                if (this._splitterMinExtent < 1)
                {
                    this._splitter.InvalidateMeasure();

                    this._splitter.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                    Size desiredSize = this._splitter.DesiredSize;

                    if (orientation == Orientation.Vertical)
                        this._splitterMinExtent = this._splitter.DesiredSize.Width;
                    else
                        this._splitterMinExtent = this._splitter.DesiredSize.Height;
                }

                this.InvalidateArrange();
            }
            else
            {
                if (this._splitter != null)
                {
                    TileAreaSplitter splitter = this._splitter;
                    this._splitter = null;
                    this.RemoveVisualChild(splitter);
                    this.RemoveLogicalChild(splitter);
                    this._extraVisualChildren.Remove(splitter);
                    this._splitterMinExtent = 0;
                }

            }
        }

                #endregion //InitializeSplitter	
    
                #region InitializeTileContainer

        private void InitializeTileContainer(object item, Tile tile)
        {
            if (tile != item)
            {
                DataTemplate template = this.ItemTemplate;

                if (template != null)
                    tile.ContentTemplate = template;
                
                DataTemplateSelector templateSelector = this.ItemTemplateSelector;

                if (templateSelector != null)
                    tile.ContentTemplateSelector = templateSelector;
                
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

                templateSelector = this.ItemHeaderTemplateSelector;

                if (templateSelector != null)
                    tile.HeaderTemplateSelector = templateSelector;

                string stringFormat = null;

                // Use the cached DependencyPropertys for ItemStringFormat and ContentStringFormat since
                // these properties were only introduced in 3.5 we can't access these properties 
                // directly. Otherwise we will blow up when running on an earlier versions of the framework
                if (s_ItemStringFormatProperty != null &&
                    s_ContentStringFormatProperty != null &&
                    s_HeaderStringFormatProperty != null)
                {
                    stringFormat = this.GetValue(s_ItemStringFormatProperty) as string;

                    if (stringFormat != null)
                    {
                        tile.SetValue(s_ContentStringFormatProperty, stringFormat);
                        tile.SetValue(s_HeaderStringFormatProperty, stringFormat);
                    }
                }

                // bind the Header property
                this.BindPathProperty(item,
                                        tile,
                                        XamTilesControl.HeaderPathProperty,
                                        Tile.HeaderProperty,
                                        stringFormat);

                // bind the SerializationId property
                bool isSerializationIdBound = this.BindPathProperty(item,
                                                                    tile,
                                                                    XamTilesControl.SerializationIdPathProperty,
                                                                    Tile.SerializationIdProperty,
                                                                    stringFormat);


                DependencyObject d = item as DependencyObject;

				// JJD 5/9/11 - TFS74206 
				// Call BindTilePropertyIfSpecified instead
				//// initialize any attached properties
				//InitialzeTilePropertyIfNonDefault(d, tile, Tile.ColumnProperty);
				//InitialzeTilePropertyIfNonDefault(d, tile, Tile.ColumnSpanProperty);
				//InitialzeTilePropertyIfNonDefault(d, tile, Tile.ColumnWeightProperty);
				//InitialzeTilePropertyIfNonDefault(d, tile, Tile.ConstraintsProperty);
				//InitialzeTilePropertyIfNonDefault(d, tile, Tile.ConstraintsMaximizedProperty);
				//InitialzeTilePropertyIfNonDefault(d, tile, Tile.ConstraintsMinimizedProperty);
				//InitialzeTilePropertyIfNonDefault(d, tile, Tile.ConstraintsMinimizedExpandedProperty);
				//InitialzeTilePropertyIfNonDefault(d, tile, Tile.RowProperty);
				//InitialzeTilePropertyIfNonDefault(d, tile, Tile.RowSpanProperty);
				//InitialzeTilePropertyIfNonDefault(d, tile, Tile.RowWeightProperty);

				//if (!isSerializationIdBound)
				//    InitialzeTilePropertyIfNonDefault(d, tile, Tile.SerializationIdProperty);
                BindTilePropertyIfSpecified(d, tile, Tile.ColumnProperty);
				BindTilePropertyIfSpecified(d, tile, Tile.ColumnSpanProperty);
				BindTilePropertyIfSpecified(d, tile, Tile.ColumnWeightProperty);
				BindTilePropertyIfSpecified(d, tile, Tile.ConstraintsProperty);
				BindTilePropertyIfSpecified(d, tile, Tile.ConstraintsMaximizedProperty);
				BindTilePropertyIfSpecified(d, tile, Tile.ConstraintsMinimizedProperty);
				BindTilePropertyIfSpecified(d, tile, Tile.ConstraintsMinimizedExpandedProperty);
				BindTilePropertyIfSpecified(d, tile, Tile.RowProperty);
				BindTilePropertyIfSpecified(d, tile, Tile.RowSpanProperty);
				BindTilePropertyIfSpecified(d, tile, Tile.RowWeightProperty);

                if (!isSerializationIdBound)
					BindTilePropertyIfSpecified(d, tile, Tile.SerializationIdProperty);
            }

            ItemInfo info = this.GetItemInfo(item);

            if (info != null)
                tile.SynchStateFromInfo(info);
        }

                #endregion //InitializeTileContainer	
    
                #region InitialzeTilePropertyIfNonDefault

		// JJD 5/9/11 - TFS74206 - no longer used
		//private static void InitialzeTilePropertyIfNonDefault(DependencyObject item, Tile tile, DependencyProperty dp)
		//{
		//    object value = item != null ? item.GetValue(dp) : null;

		//    if (value != null && !Object.Equals(value, dp.DefaultMetadata.DefaultValue))
		//        tile.SetValue(dp, value);
		//    else
		//    {
		//        // if we aren't settting the property then we should clear the setting from
		//        // the last time we used this container
		//        ValueSource source = DependencyPropertyHelper.GetValueSource(tile, dp);
		//        if (source.BaseValueSource == BaseValueSource.Local)
		//            tile.ClearValue(dp);
		//    }
        //}

                #endregion //InitialzeTilePropertyIfNonDefault	
    
                #region OnMaximizedItemsCollectionChanged

        private void OnMaximizedItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.SynchronizeMaximizedItemsCollection();
        }

                #endregion //OnMaximizedItemsCollectionChanged	
        
                #region PerformResize
    
        private void PerformResize(FrameworkElement resizableItem, double deltaX, double deltaY)
        {
            Tile tile = resizableItem as Tile;

            if (tile == null)
                return;

            if (this.IsInMaximizedMode == false &&
                 this._panel != null)
            {
                NormalModeSettings settings = this._panel.NormalModeSettingsSafe;
                if (settings.AllowTileSizing == AllowTileSizing.Synchronized)
                {
                    bool synchronizeWidth = false;
                    bool synchronizeHeight = false;

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

                    // if we ara sychronizing either the width ot height then
                    // set the SynchronizedItemSize on the manager
                    
                    
                    
                    
                    if ((synchronizeWidth == true  && deltaX != 0 ) ||
                        (synchronizeHeight == true && deltaY != 0 ) )
                    {
                        Size size = new Size(Math.Max(resizableItem.ActualWidth + deltaX, 0), Math.Max(resizableItem.ActualHeight + deltaY, 0));

                        this._panel.Manager.SynchronizedItemSize = size;
                    }

                    // if we are synchronizing both dimensions then we can bail out
                    // since we don't need to explicitly set a size on the item below
                    
                    
                    
                    
                    
                    if ((synchronizeWidth == true  || deltaX == 0 ) &&
                        (synchronizeHeight == true || deltaY == 0 ))
                        return;
                }
            }

            ItemInfo info = this.GetItemInfo(this._panel.ItemFromTile(tile));

            if (info != null)
            {
                info.OnResizeInternal(deltaX, deltaY);
            }

        }

   	            #endregion //PerformResize	

                #region ProcessScrollIntoView

        
        private object ProcessScrollIntoView(object item)
        {
            ItemInfo info = this.GetItemInfo(item);

            if (info != null)
                info.BringIntoView();

            return item;
        }

                #endregion //ProcessScrollIntoView	
    
                #region SynchronizeMaximizedItemsCollection

        private void SynchronizeMaximizedItemsCollection()
        {
            int newCount = this._panel != null ? this._panel.MaximizedItems.Count : 0;

            // if the counts match then verify that each item matches
            if (newCount == this._maximizedItems.Count)
            {
                bool collectionsMatchExactly = true;

                for (int i = 0; i < newCount; i++)
                {
                    if (this._maximizedItems[i] != this._panel.MaximizedItems[i])
                    {
                        collectionsMatchExactly = false;
                        break;
                    }
                }

                // if everything matched up we can return withour doing anything
                if (collectionsMatchExactly)
                    return;
            }

            // call BeginUpdate to prevent individual notifications from being generated
            this._maximizedItems.BeginUpdate();

            // first clear the collection
            this._maximizedItems.Clear();

            // then add all items from the panel's collection
            if (this._panel != null && this._panel.MaximizedItems.Count > 0)
                this._maximizedItems.AddRange(this._panel.MaximizedItems);

            // calling EndUpdate will generate a Reset notification.
            this._maximizedItems.EndUpdate();

            if (this._maximizedItems.Count == 0)
                this.SetValue(TilesPanel.IsInMaximizedModePropertyKey, KnownBoxes.FalseBox);
            else
                this.SetValue(TilesPanel.IsInMaximizedModePropertyKey, KnownBoxes.TrueBox);

        }

                #endregion //SynchronizeMaximizedItemsCollection	

                #region UpdateThemeResources

        private void UpdateThemeResources()
        {
            string[] groupings = new string[] { TilesGeneric.Location.Grouping };

            ThemeManager.OnThemeChanged(this, this.Theme, groupings);

            // AS 9/4/09 TFS21087
            if (this.IsInitialized)
            {
                // JJD 2/26/07
                // we need to call UpdateLayout after we change the merged dictionaries.
                // Otherwise, the styles from the new merged dictionary are not picked
                // up right away. It seems the framework must be caching some information
                // that doesn't get refreshed until the next layout update
                this.InvalidateMeasure();

                if (this.Panel != null)
                    this.Panel.InvalidateMeasure();

                this.UpdateLayout();
            }
        }

            #endregion //UpdateThemeResources	
     
            #endregion //Private Methods	
    
        #endregion //Methods	
    
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


        #region IResizeHost Members

        ResizeController IResizeHost.Controller
        {
            get
            {
                return this._resizeController;
            }
        }

        FrameworkElement IResizeHost.RootElement
        {
            get { return this; }
        }

        void IResizeHost.AddResizerBar(FrameworkElement resizerBar)
        {
            if (!this._extraVisualChildren.Contains(resizerBar))
            {
                this._extraVisualChildren.Add(resizerBar);
                this.AddVisualChild(resizerBar);

                Debug.Assert(this._extraVisualChildren.Count < 3, "There can only be up to 2 resizer bars");
            }
        }

        bool IResizeHost.CanResize(FrameworkElement resizableItem, bool resizeInXAxis)
        {

            if (this._panel == null)
                return false;

            if (this.IsInMaximizedMode)
                return false;

            Tile tile = resizableItem as Tile;

            if (tile == null)
                return false;

            if (this._panel.NormalModeSettingsSafe.AllowTileSizing == AllowTileSizing.No)
                return false;

            return true;
        }

        FrameworkElement IResizeHost.GetResizeAreaForItem(FrameworkElement resizableItem)
        {
			// JJD 07/12/12 - TFS112221
			// Return the panel unless we are in the middle of a drag operation
			if (_panel != null && false == _panel._IsDragging)
				return _panel;

			return this;
        }

        void IResizeHost.RemoveResizerBar(FrameworkElement resizerBar)
        {
            if (this._extraVisualChildren.Contains(resizerBar))
            {
                this._extraVisualChildren.Remove(resizerBar);
                this.RemoveVisualChild(resizerBar);
            }
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

            ItemInfo info = this.GetItemInfoFromContainer(resizableItem);

            if ( info == null )
            {
                constraints.Cancel = true;
                return;
            }

            double maxDeltaLeft, maxDeltaRight, maxDeltaTop, maxDeltaBottom;

            info.GetResizeRangeInternal(out maxDeltaLeft, out maxDeltaRight,
                                                out maxDeltaTop, out maxDeltaBottom);

            double actualExtent;
            double maxDeltaNegative;
            double maxDeltaPositive;

            Tile tile = resizableItem as Tile;

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
 
            constraints.MinExtent = Math.Max( actualExtent - maxDeltaNegative , constraints.MinExtent);
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

                    double topOffset    = thp.TranslatePoint(new Point(0, 0), tile).Y;
                    double bottomOffset = tile.ActualHeight - thp.TranslatePoint(new Point(0, headerMinExtent), tile).Y;

                    double extraExtent = Math.Max(Math.Min(topOffset, bottomOffset), 0);

                    constraints.MinExtent = Math.Max(headerMinExtent + (2 * extraExtent), constraints.MinExtent);
                }
            }
        }

        #endregion

        #endregion
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