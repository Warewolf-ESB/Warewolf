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
using Infragistics.Windows.Controls.Events;
using Infragistics.Windows.Helpers;
using Infragistics.Windows.Commands;
using Infragistics.Windows.Virtualization;
using System.Xml;
using Infragistics.Collections;

namespace Infragistics.Windows.Controls
{
	#region XamCarouselPanel Class

	/// <summary>
	/// A <see cref="System.Windows.Controls.Panel"/> derived element that arranges and displays its child elements along a user-defined path, with native support for scrolling and virtualizing those items.
	/// </summary>
	/// <remarks>
    /// <p class="body">The XamCarouselPanel provides advanced layout capabilties that let you arrange elements along a path of your choosing and apply various
    /// parent effects to the items based on their position in the XamCarouselPanel's display area.</p>
    /// <p class="note"><b>Note: </b>Since the XamCarouselPanel is also used by the <see cref="XamCarouselListBox"/> as its ItemsPanel, all the functionality described here for the XamCarouselPanel also
    /// applies to the <see cref="XamCarouselListBox"/>.  While the XamCarouselPanel used by <see cref="XamCarouselListBox"/> is not directly exposed via a property, the <see cref="XamCarouselListBox"/>
    /// exposes a <see cref="XamCarouselListBox.ViewSettings"/> property of type <see cref="CarouselViewSettings"/> that lets you control the corresponding settings on the embedded XamCarouselPanel.</p>
    /// <p class="body">The XamCarouselPanel.ViewSettings property exposes a <see cref="CarouselViewSettings"/> object that lets you tweak numerous settings that control all aspects of 
    /// layout and presentation, including:
    ///		<ul>
    ///			<li>the number of items to display per page (<see cref="CarouselViewSettings.ItemsPerPage"/>)</li>
    ///			<li>the path geometry used to create the path along which items in the list are arranged (<see cref="CarouselViewSettings.ItemPath"/>)</li>
    ///			<li>the various parent effects that can be applied to items as they are displayed at different points along the path (<see cref="CarouselViewSettings.OpacityEffectStops"/>),
    /// <see cref="CarouselViewSettings.ScalingEffectStops"/>, <see cref="CarouselViewSettings.SkewAngleXEffectStops"/>,
    /// <see cref="CarouselViewSettings.SkewAngleYEffectStops"/>, <see cref="CarouselViewSettings.ZOrderEffectStops"/>)</li>
    ///		</ul>
    /// </p>
	/// <p class="body">Refer to the <a href="xamCarousel_Terms_Architecture.html">Carousel Architecture Overview</a> topic in the Developer's Guide for an explanation of how Carousel presentation works.</p>
	/// </remarks>
	/// <seealso cref="XamCarouselPanel.ViewSettings"/>
	/// <seealso cref="CarouselViewSettings"/>
	/// <seealso cref="System.Windows.Controls.Panel"/>
	
	
	//[ToolboxItem(true)]
	// AS 5/13/10
	//[System.Drawing.ToolboxBitmap(typeof(XamCarouselPanel), AssemblyVersion.ToolBoxBitmapFolder + "CarouselPanel.bmp")]
	//[Description("A Panel derived element that arranges and displays its child elements along a user-defined path, with native support for scrolling and virtualizing those items.")]
	public class XamCarouselPanel : RecyclingItemsPanel,
									IScrollInfo,
									IWeakEventListener,
									ICommandHost

	{
		#region Member Variables

		private ScrollData									_scrollingData;

		private UltraLicense								_license;
		private long										_previousTick = long.MinValue;
		private double										_smoothedFrameRate = XamCarouselPanel.REFERENCE_FRAME_RATE;

		private Size										_previousArrangeSize = Size.Empty;
		private Size										_previousMeasureSize = Size.Empty;

		private bool										_shouldGenerateElements = true;
		private ScrollActionInfo							_scrollActionInfo = ScrollActionInfo.Empty;
		private double										_scrollOffsetForInitialArrangeAnimation = 0;

		private CarouselPanelAdorner						_carouselPanelAdorner;
		private Size										_wrapperSize = (Size)CarouselViewSettings.ItemSizeProperty.DefaultMetadata.DefaultValue;
		private bool										_wrapperSizeDirty = true;

		private bool										_initialArrangeAnimationDone;
		private CarouselPanelNavigator						_carouselPanelNavigator;

		private ScalingEffectStopCollection					_defaultScalingEffectStops;
		private ZOrderEffectStopCollection					_defaultZOrderEffectStops;
		private SkewAngleXEffectStopCollection				_defaultSkewAngleXEffectStops;
		private SkewAngleYEffectStopCollection				_defaultSkewAngleYEffectStops;
		private OpacityEffectStopCollection					_defaultOpacityEffectStops;

		private Pen											_cachedItemPathRenderPen;
		private Brush										_cachedItemPathRenderBrush;

		private double[]									_visiblePositionPercentValues;
		private bool										_visiblePositionPercentValuesDirty = true;
		private double										_distanceBetweenVisiblePositions;
		private double										_firstVisiblePositionPercentLocation;
		private double										_lastVisiblePositionPercentLocation = 1;

		private ICarouselPanelSelectionHost					_selectionHost;

		private PathGeometry								_itemPathGeometry;

		private CarouselViewSettings						_viewSettings;

        // JJD 9/08/09
        // Added proxy class to prevent rooting of the XamCarouselPanel if we don't
        // unwire the Rendering event. This was caught by unit tests.
        private RenderingEventProxy                         _renderingEventProxy;

		// JM 01-10-08 BR29568
		//private Hashtable									_pathFractionPointsCache;
		private Dictionary<double, PathFractionPoint>		_pathFractionPointsCache;

        // JJD 1/2/07 
        // Added ViewSettingsInternal so we could create a default ViewSettings for use at design time
        // to allow the VS2008 designers to work
        private CarouselViewSettings                        _designTimeDefaultViewSettings;

		// JM 01-05-12 TFS96730
		private bool										_wasLoaded;

		#endregion //Member Variables

		#region Constructor

		/// <summary>
		/// Creates an instance of a XamCarouselPanel.
		/// </summary>
		public XamCarouselPanel()
		{
			this.ClipToBounds	= true;
			this.Focusable		= true;

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
					this._license = LicenseManager.Validate(typeof(XamCarouselPanel), this) as UltraLicense;
				}
				catch (System.IO.FileNotFoundException) { }
			}


			// JM 05-06-08 - BR32342
			this.Unloaded += new RoutedEventHandler(OnUnloaded);
		}

		#endregion //Constructor

		#region Constants

		private const int								SCROLL_SMALL_CHANGE = 1;

		private const double							REFERENCE_FRAME_RATE = 30;
		private const double							FRAME_RATE_SMOOTHING_FACTOR = .05;
		private const double							MAX_VELOCITY = .09; //0.05;
		private const double							ATTRACTION = 7;//6;//5;
		private const double							DAMPENING = 0;
		private const double							MIN_DISTANCE_THRESHHOLD = .0004; //0.0001;

		#endregion //Constants

		#region Base Class Overrides

			#region ArrangeOverride

		/// <summary>
		/// Positions child elements and determines a size for this element.
		/// </summary>
		/// <param name="finalSize">The size available to this element for arranging its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> used by this element to arrange its children.</returns>
		protected override Size ArrangeOverride(Size finalSize)
		{
			this.DoArrangeProcessing(finalSize, true);

			return finalSize;
		}

			#endregion //ArrangeOverride	

			#region CreateUIElementCollection

		/// <summary>
		/// Returns a UIElementCollection instance for storing child elements.
		/// </summary>
		/// <param name="logicalParent">The logical parent element of the collection to be created.</param>
		/// <returns>An ordered collection of elements that have the specified logical parent.</returns>
		protected override UIElementCollection CreateUIElementCollection(FrameworkElement logicalParent)
		{
			// JM 11-28-07 BR28764
			//return new CarouselPanelUIElementCollection(this, this, logicalParent);
			CarouselPanelUIElementCollection coll	= new CarouselPanelUIElementCollection(this, this, logicalParent);
			coll.CollectionChanged					+= new EventHandler<EventArgs>(OnChildrenCollectionChanged);
			return coll;
		}

			#endregion //CreateUIElementCollection	

			#region GetCanCleanupItem

		/// <summary>
		/// Derived classes must return whether the item at the specified index can be cleaned up.
		/// </summary>
		/// <param name="itemIndex">The index of the item to be cleaned up.</param>
		/// <returns>True if the item at the specified index can be cleaned up, false if it cannot.</returns>
		protected override bool GetCanCleanupItem(int itemIndex)
		{
			return this.GetIsItemIndexVisible(itemIndex, this.FirstVisibleItemIndex) == false;
		}

			#endregion //GetCanCleanupItem

			#region IsOkToCleanupUnusedGeneratedElements

		/// <summary>
		/// Called when elements are about to be cleaned up.  Return true to allow cleanup, false to prevent cleanup.
		/// </summary>
		protected override bool IsOkToCleanupUnusedGeneratedElements
		{
			get	{ return this.ScrollInProgress == false; }
		}

			#endregion IsOkToCleanupUnusedGeneratedElements

            // AS 3/11/09 TFS11010
            // We were adding the ViewSettings & CarouselPanelNavigator as logical children but 
            // but including it in the LogicalChildren.
            //
            #region LogicalChildren
        /// <summary>
        /// Gets an enumerator that can iterate the logical child elements of this element.
        /// </summary>
        protected override IEnumerator LogicalChildren
        {
            get
            {
                return new MultiSourceEnumerator(
                    new SingleItemEnumerator(null != this._viewSettings && LogicalTreeHelper.GetParent(this._viewSettings) == this ? this._viewSettings : null),
                    new SingleItemEnumerator(this._carouselPanelNavigator),
                    base.LogicalChildren);
            }
        } 
            #endregion //LogicalChildren

			#region MaximumUnusedGeneratedItemsToKeep

		/// <summary>
		/// Returns the maximum number of unused generated items that should be kept around at any given time.
		/// </summary>
		protected override int MaximumUnusedGeneratedItemsToKeep
		{
			get 
			{
				return Math.Min(this.ItemsPerPageResolved * 5, 100);
			}
		}

			#endregion //MaximumUnusedGeneratedItemsToKeep
    
			#region MeasureOverride

		/// <summary>
		/// Invoked to measure the element and its children.
		/// </summary>
		/// <param name="availableSize">The size that reflects the available size that this element can give to its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> that represents the desired size of the element.</returns>
		protected override Size MeasureOverride(Size availableSize)
		{
			this.PathFractionPointsCache.Clear();

			CarouselViewSettings viewSettings = this.ViewSettingsInternal;

			if (availableSize.Width == double.PositiveInfinity || availableSize.Height == double.PositiveInfinity)
			{
				double width	= (availableSize.Width == double.PositiveInfinity) ? viewSettings.WidthInInfiniteContainers : availableSize.Width;
				double height	= (availableSize.Height == double.PositiveInfinity) ? viewSettings.HeightInInfiniteContainers : availableSize.Height;
				availableSize	= new Size(width, height);
			}


			// Create and initialize a CarouselPanelAdorner if we haven't already done so.
			if (this._carouselPanelAdorner == null)
				this.InitializeAdorner();

	
			// [BR17712] 3-1-07 JM - No longer need to do this.
			// Skip our Measure processing if we are in the middle of a scroll.  For some reason measure is getting called
			// during the scroll - I'm not sure what's triggering it.  I don't think we need to do our Measure processing in this
			// case.  If we notice layout problems during animated scrolling, we may have to comment out the following 2 lines.
			//if (this.ScrollInProgress)
			//    return availableSize;


			// Recalculate the VisiblePositionPercentLocations if necessary (this should only be necessary here on 
			// the first measure).
			if (this._visiblePositionPercentValuesDirty)
				this.RecalculateVisiblePositionPercentValues();


			// Update our ScrollData (doing this here in MeasureOverride is really just to initialize the values in ScrollData so we
			// have them in time for the first ArrangeOverride)
			int itemsPerPage = this.ItemsPerPageResolved;

			// [JM 05-17-07]
			//this.UpdateScrollData(new Size((double)base.TotalItemCount, (double)base.TotalItemCount),
			//                      new Size((double)itemsPerPage, (double)itemsPerPage),
			//                      this.ScrollingData._offset);
			this.UpdateScrollData(new Size((double)this.TotalItemCount, (double)this.TotalItemCount),
								  new Size((double)itemsPerPage, (double)itemsPerPage),
								  this.ScrollingData._offset);


			// Save our measure size
			this._previousMeasureSize = availableSize;


			// Scale up/down the size of the Path based on the size of our client area and the size of the PathGeometry
			this.ScaleItemPath(availableSize);


			return availableSize;
		}

			#endregion //MeasureOverride	

			#region OnMouseWheel

		/// <summary>
		/// Called when the MouseWheel is moved.
		/// </summary>
		/// <param name="e">An instance of MouseWheelEventArgs that provides information about the MouseWheel.</param>
		protected override void OnMouseWheel(MouseWheelEventArgs e)
		{
			if (e.Delta > 0)
			{
				this.MouseWheelLeft();
				e.Handled = true;
			}
			else
			if (e.Delta < 0)
			{
				this.MouseWheelRight();
				e.Handled = true;
			}

			base.OnMouseWheel(e);
		}

			#endregion //OnMouseWheel	
    
			#region OnInitialized

		/// <summary>
		/// Raises the System.Windows.FrameworkElement.Initialized event.
		/// </summary>
		/// <param name="e">An instance of EventArgs that provides information about the event.</param>
		protected override void OnInitialized(EventArgs e)
		{
			base.OnInitialized(e);

			// AS 6/29/07 BR24566
			// When the panel is first measured, it ends up measuring with its
			// own view settings instead of that of the containing listbox. 
			// To avoid that we could either change the itemspaneltemplate of 
			// the carousellistbox to try and do it or we could have the listbox
			// do it. I talked to Joe M about it and we decided to go this route.
			// If that gets reconsidered, we'll need to set a binding between the 
			// ViewSettings. The only way I found to do that and have it work was to 
			// use a relative source of templatedparent and a path of 
			// "(FrameworkElement.TemplatedParent).ViewSettings"
			//
			if (this._viewSettings == null && this.IsInCarouselListBox)
			{
				XamCarouselListBox listBox = Utilities.GetAncestorFromType(this, typeof(XamCarouselListBox), true) as XamCarouselListBox;

				if (listBox != null)
					this.SetBinding(XamCarouselPanel.ViewSettingsProperty, Utilities.CreateBindingObject(XamCarouselListBox.ViewSettingsProperty, BindingMode.OneWay, listBox));
			}

			// AS 6/29/07
			// I don't think we should really be converting what could have been 
			// a templated value to a local value unless we need to create the view
			// settings.
			//
            // JJD 1/2/07 
            // Check for design mode first
            // If we are in design mode don't sync ViewSettings
            //if  (this._viewSettings == null)
			if  (this._viewSettings == null &&
                 DesignerProperties.GetIsInDesignMode(this) == false)
			{
				// Set the ViewSettings dependency property using the CLR property which will do a lazy create of the object.
				this.SetValue(XamCarouselPanel.ViewSettingsProperty, this.ViewSettings);
			}

			this.ViewSettingsInternal.OnControlInitialized();
		}

			#endregion //OnInitialized	
    
			#region OnItemsChanged

		/// <summary>
		/// Called when the contents of the associated list changes.
		/// </summary>
		/// <param name="sender">The sender of the event.</param>
		/// <param name="args">An instance of ItemsChangedEventArgs that contains information about the items that were changed.</param>
		protected override void OnItemsChanged(object sender, ItemsChangedEventArgs args)
		{
			base.OnItemsChanged(sender, args);

			switch (args.Action)
			{
				case NotifyCollectionChangedAction.Remove:
					// JJD 4/17/07
					// Call the new RemoveRangeOfChldren helper method
					//if (args.Position.Index > -1)
					//this.RemoveInternalChildRange(args.Position.Index, 1);
					this.RemoveRangeOfChldren(args.Position, args.ItemUICount);
					break;

				case NotifyCollectionChangedAction.Replace:
					// JJD 4/17/07
					// On a replace get rid of the old items
					this.RemoveRangeOfChldren(args.Position, args.ItemUICount);
					break;

				case NotifyCollectionChangedAction.Move:
					// JJD 4/17/07
					// On a move get rid of the old items (using the OldPosition)
					this.RemoveRangeOfChldren(args.OldPosition, args.ItemUICount);
					break;
			}

			switch (args.Action)
			{
				case NotifyCollectionChangedAction.Add:
				case NotifyCollectionChangedAction.Move:
				case NotifyCollectionChangedAction.Remove:
				case NotifyCollectionChangedAction.Replace:
				case NotifyCollectionChangedAction.Reset:
					// Update our ScrollData
					int itemsPerPage = this.ItemsPerPageResolved;

					// [JM 05-17-07]
					//this.UpdateScrollData(new Size((double)base.TotalItemCount, (double)base.TotalItemCount),
					//                      new Size((double)itemsPerPage, (double)itemsPerPage),
					//                      new Vector(this.ScrollOffsetForInitialArrangeAnimation, this.ScrollOffsetForInitialArrangeAnimation));
					// JM BR25655 09-26-07 - Only change the vertical offset when the items are being reset.
					//this.UpdateScrollData(new Size((double)this.TotalItemCount, (double)this.TotalItemCount),
					//                      new Size((double)itemsPerPage, (double)itemsPerPage),
					//                      new Vector(this.ScrollOffsetForInitialArrangeAnimation, this.ScrollOffsetForInitialArrangeAnimation));
					if (args.Action == NotifyCollectionChangedAction.Reset)
						this.UpdateScrollData(new Size((double)this.TotalItemCount, (double)this.TotalItemCount),
											  new Size((double)itemsPerPage, (double)itemsPerPage),
											  new Vector(this.ScrollOffsetForInitialArrangeAnimation, this.ScrollOffsetForInitialArrangeAnimation));
					else
						this.UpdateScrollData(new Size((double)this.TotalItemCount, (double)this.TotalItemCount),
											  new Size((double)itemsPerPage, (double)itemsPerPage),
											  new Vector(this.VerticalOffset, this.VerticalOffset));


					// Force new items to get generated as a result of the change.
					this.ShouldGenerateElements = true;


					// Force the initial arrange to animate.
					// JM BR25655 09-26-07 - Only do the initial arrange animation when the items are being reset.
					//this._initialArrangeAnimationDone = false;
					if (args.Action == NotifyCollectionChangedAction.Reset)
						this._initialArrangeAnimationDone = false;
					// JM 06-10-08 - TFS Work Item #4472 - Also do initial arrange animation on Add, Move, Remove and Replace if requested
					else
					if (this.ViewSettingsInternal.ShouldAnimateItemsOnListChange == true)
						this._initialArrangeAnimationDone = false;


					this.InvalidateArrange();
					break;
			}
		}

			#endregion //OnItemsChanged

			#region OnKeyDown

		/// <summary>
		/// Called when the element has input focus and a key is pressed.
		/// </summary>
		/// <param name="e">An instance of KeyEventArgs that contains information about the Key that was pressed.</param>
		protected override void OnKeyDown(KeyEventArgs e)
		{
			bool	handled						= false;
			Key		key							= e.Key;
			bool	itemsHostSupportsSelection	= this.ItemsHostSupportsSelection;

			switch (key)
			{
				case Key.Up:
				case Key.Left:
					if (this.IsItemsHost && itemsHostSupportsSelection)
					{
						if (this.SelectionHost.SelectedItemIndex < 0)
							this.SelectionHost.SelectedItemIndex = 0;
						else
							this.SelectionHost.SelectedItemIndex = Math.Max(0, this.SelectionHost.SelectedItemIndex - 1);

						this.EnsureItemIsVisible(this.SelectionHost.SelectedItemIndex);
					}
					else
						this.LineUp();

					handled = true;
					break;

				case Key.Down:
				case Key.Right:
					if (this.IsItemsHost && itemsHostSupportsSelection)
					{
						if (this.SelectionHost.SelectedItemIndex < 0)
							this.SelectionHost.SelectedItemIndex = 0;
						else
							this.SelectionHost.SelectedItemIndex = Math.Min(this.TotalItemCount - 1, this.SelectionHost.SelectedItemIndex + 1);

						this.EnsureItemIsVisible(this.SelectionHost.SelectedItemIndex);
					}
					else
						this.LineDown();

					handled = true;
					break;

				case Key.PageUp:
					if (this.IsItemsHost && itemsHostSupportsSelection)
					{
						if (this.GetIsItemIndexVisible(this.SelectionHost.SelectedItemIndex, this.FirstVisibleItemIndex) &&
							this.SelectionHost.SelectedItemIndex != this.FirstVisibleItemIndex)
							this.SelectionHost.SelectedItemIndex = this.FirstVisibleItemIndex;
						else
						{
							this.SelectionHost.SelectedItemIndex = Math.Max(0, this.SelectionHost.SelectedItemIndex - this.ItemsPerPageResolved + 1);
							this.EnsureItemIsVisible(this.SelectionHost.SelectedItemIndex);
						}

					}
					else
						this.PageUp();

					handled = true;
					break;

				case Key.PageDown:
					if (this.IsItemsHost && itemsHostSupportsSelection)
					{
						if (this.GetIsItemIndexVisible(this.SelectionHost.SelectedItemIndex, this.FirstVisibleItemIndex) &&
							this.SelectionHost.SelectedItemIndex != this.LastVisibleItemIndex)
							this.SelectionHost.SelectedItemIndex = this.LastVisibleItemIndex;
						else
						{
							this.SelectionHost.SelectedItemIndex = Math.Min(this.TotalItemCount - 1, this.SelectionHost.SelectedItemIndex + this.ItemsPerPageResolved - 1);
							this.SetVerticalOffset(this.FirstVisibleItemIndexFromLastVisibleItemIndex(this.SelectionHost.SelectedItemIndex));
						}

					}
					else
						this.PageDown();

					handled = true;
					break;
			}

			if (handled)
				e.Handled = true;
			else
				base.OnKeyDown(e);
		}

			#endregion //OnKeyDown	
    
			#region OnMouseDown

		/// <summary>
		/// Called when the mouse is pressed within the element.
		/// </summary>
		/// <param name="e">An instance of MouseButtonEventArgs that contains information about the Mouse.</param>
		protected override void OnMouseDown(MouseButtonEventArgs e)
		{
			base.OnMouseDown(e);

			if (this.IsFocused == false)
				this.Focus();
		}

			#endregion //OnMouseDown	
    
			#region OnNewElementRealized

		/// <summary>
		/// Called after a newly realized element is generated, added to the children collection and 'prepared'.
		/// </summary>
		/// <param name="element">The newly realized element</param>
		/// <param name="index">The position at which the element was added to the children collection</param>
		protected override void OnNewElementRealized(UIElement element, int index)
		{

		}

			#endregion //OnNewElementRealized

			#region OnPropertyChanged

		/// <summary>
		/// Called when a property changes.
		/// </summary>
		/// <param name="e">An instance of DependencyPropertyChangedEventArgs that contains information about the property that changed.</param>
		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnPropertyChanged(e);

			// AS 1/24/08
			// Do a reference comparison instead of a string comparison
			//
			//switch (e.Property.Name)
			if (e.Property == ViewSettingsProperty)
			{
				//case "ViewSettings":
					// Remove the old property changed event listener.
					if (this._viewSettings != null)
					{
						// AS 6/4/09 TFS18192
						////PropertyChangedEventManager.RemoveListener(this._viewSettings, this, string.Empty);
						//this._viewSettings.PropertyChanged -= new PropertyChangedEventHandler(OnViewSettingsPropertyChanged);
						PropertyChangedEventManager.RemoveListener(this._viewSettings, this, string.Empty);

						// Remove the view settings object as our logical child.
						DependencyObject logicalParent = LogicalTreeHelper.GetParent(this._viewSettings);
						if (logicalParent == this)
							this.RemoveLogicalChild(this._viewSettings);
					}


					// Update our member variable.
					this._viewSettings = e.NewValue as CarouselViewSettings;


					// JM 4-5-07
					//if (this.IsInitialized == true)
					if (this.IsInitialized == true && this._viewSettings != null)
						this._viewSettings.OnControlInitialized();


					// Rebind the CarouselPanelNavigator's visibility to the IsNavigatorVisible 
					// property on the new View Settings
					this.BindNavigatorVisibility();


					// Add a new property changed event listener.
					if (this._viewSettings != null)
					{
						// [JM 04-23-07]
						// Update our internal CarouselViewSettings version number.
						this.BumpCarouselViewSettingsVersion();


						// AS 6/4/09 TFS18192
						////PropertyChangedEventManager.AddListener(this._viewSettings, this, string.Empty);
						//this._viewSettings.PropertyChanged += new PropertyChangedEventHandler(OnViewSettingsPropertyChanged);
						PropertyChangedEventManager.AddListener(this._viewSettings, this, string.Empty);


						// Add the view settings object as our logical child.
						//
						// Make sure the new view settings object does not already have a logical parent.
						DependencyObject logicalParent = LogicalTreeHelper.GetParent(this._viewSettings);
                        
                        
                        
                        
                        //if (logicalParent != null && logicalParent != this)
                        //{
                        //    this.RemoveLogicalChild(this._viewSettings);
                        //    logicalParent = LogicalTreeHelper.GetParent(this._viewSettings);
                        //}

						if (logicalParent == null)
							this.AddLogicalChild(this._viewSettings);


						// Tell the CarouselViewSettings object to raise prop change notifications for 
						// any props that contain non-default values.
						this._viewSettings.EnumeratePropertiesWithNonDefaultValues(this.OnViewSettingsPropertyChanged);
					}

					// AS 6/29/07
					// Since some settings could have been set on the old view settings but not
					// on the new settings, we should probably consider those as property changes.
					//
					CarouselViewSettings oldViewSettings = e.OldValue as CarouselViewSettings;

					if (null != oldViewSettings)
					{
						// Tell the CarouselViewSettings object to raise prop change notifications for 
						// any props that contain non-default values.
						oldViewSettings.EnumeratePropertiesWithNonDefaultValues(this.OnViewSettingsPropertyChanged);
					}


					//break;
			}
		}
			#endregion //OnPropertyChanged	

			#region OnRender

		/// <summary>
		/// Called when the element should render its contents.
		/// </summary>
		/// <param name="drawingContext">An initialized DrawingContext that should be used for all drawing within this method.</param>
		protected override void OnRender(DrawingContext drawingContext)
		{
			base.OnRender(drawingContext);

			if (this._cachedItemPathRenderPen != null || this._cachedItemPathRenderBrush != null)
				drawingContext.DrawGeometry(this._cachedItemPathRenderBrush, this._cachedItemPathRenderPen, this.ItemPathGeometry);
			else
				drawingContext.DrawRectangle(Brushes.Transparent, null, new Rect(new Point(0, 0), this.RenderSize));
		}

			#endregion //OnRender	
    
			#region TotalVisibleGeneratedItems

		/// <summary>
		/// Derived classes must return the number of visible generated items.
		/// </summary>
		protected override int TotalVisibleGeneratedItems
		{
			get 
			{
				if (base.IsItemsHost == false)
					return 0;

				return Math.Min(this.TotalItemCount - this.FirstVisibleItemIndex, ((int)this.ScrollingData._offset.Y >= 0 ? this.ItemsPerPageResolved : this.ItemsPerPageResolved + (int)this.ScrollingData._offset.Y));
			}
		}

			#endregion //TotalVisibleGeneratedItems
    
		#endregion //Base Class Overrides

		#region Implemented Interfaces

			#region IScrollInfo Members

				#region ExtentHeight, ExtentHeight

		/// <summary>
		/// Returns the overall logical height of the scrollable area. (read only)
		/// </summary>
		public double ExtentHeight
		{
			get
			{
				return this.ScrollingData._extent.Height;
			}
		}

		/// <summary>
		/// Returns the overall logical width of the scrollable area. (read only)
		/// </summary>
		public double ExtentWidth
		{
			get
			{
				return this.ScrollingData._extent.Width;
			}
		}

				#endregion //ExtentWidth, ExtentHeight

				#region HorizontalOffset, VerticalOffset

		/// <summary>
		/// Returns the logical horizontal offset of the scrollable area. (read only)
		/// </summary>
		public double HorizontalOffset
		{
			get
			{
				// Since we are only supporting scrolling in one direction always return vertical offsets.
				return this.ScrollingData._offset.Y;
			}
		}

		/// <summary>
		/// Returns the logical vertical offset of the scrollable area. (read only)
		/// </summary>
		public double VerticalOffset
		{
			get
			{
				return this.ScrollingData._offset.Y;
			}
		}

				#endregion //HorizontalOffset, VerticalOffset

				#region SetHorizontalOffset, SetVerticalOffset

		/// <summary>
		/// Sets the horizontal scroll offset.
		/// </summary>
		/// <param name="offset">The new horizontal scroll offset.</param>
		public void SetHorizontalOffset(double offset)
		{
			// JM 10-16-09 TFS23915 - Moved logic to new internal method passing null for new second parameter.
			//// Since we are only supporting scrolling in one direction always set the vertical offset.
			//this.SetVerticalOffset(offset);
			this.SetHorizontalOffset(offset, null);
		}

		// JM 10-16-09 TFS23915 - Added
		internal void SetHorizontalOffset(double offset, bool? scrollingHigherHint)
		{
			// Since we are only supporting scrolling in one direction always set the vertical offset.
			this.SetVerticalOffset(offset, scrollingHigherHint);
		}

		/// <summary>
		/// Sets the vertical scroll offset.
		/// </summary>
		/// <param name="offset">The new vertical scroll offset.</param>
		public void SetVerticalOffset(double offset)
		{
			// JM 10-16-09 TFS23915 - Moved logic to new internal method passing null for new second parameter.
			this.SetVerticalOffset(offset, null);
		}

		// JM 10-16-09 TFS23915 - Added
		internal void SetVerticalOffset(double offset, bool? scrollingHigherHint)
		{
Debug.WriteLine("In SetVerticalOffset.  CurrentOffset: " + this.ScrollingData._offset.Y.ToString() + ", RequestedOffset: " + offset.ToString());

			// JM 10-16-09 TFS23915 - Use the new scrollingHigherHint parameter if available
			//bool					scrollingHigher		= offset > this.ScrollingData._offset.Y;
			bool					scrollingHigher		= scrollingHigherHint.HasValue ? scrollingHigherHint.Value : (offset > this.ScrollingData._offset.Y);
			double					newOffsetNormalized	= 0;
			CarouselViewSettings	viewSettings		= this.ViewSettingsInternal;
			int						itemsPerPage		= this.ItemsPerPageResolved;


			if (viewSettings.IsListContinuous == false || (viewSettings.IsListContinuous == true && (this.TotalItemCount < itemsPerPage)))
				newOffsetNormalized = Math.Min(Math.Max(this.LowestPossibleScrollOffset, offset), this.HighestPossibleScrollOffset);
			else
			{
				int highestOffset = this.TotalItemCount - 1;
				if (offset > highestOffset)
					newOffsetNormalized = offset - highestOffset - 1;
				else
				if (offset < 0)
					newOffsetNormalized = highestOffset - Math.Abs(offset) + 1;
				else
					newOffsetNormalized = offset;
			}


			// JM 9-11-08 [BR30890 TFS5877] - If the list is Continuous and the new offset is currently visible, always consider 
			// scrollingHigher to be true except when the number of items in the list is <= the number of ItemsPerPage.  This will allow
			// us to properly handle the situation where the old offset and the new offset are both visible and span the beginning/end of the list.
			int newOffsetVisiblePosition;
			bool newOffsetCurrentlyVisible = this.GetIsItemIndexVisible((int)newOffsetNormalized, (int)this.ScrollingData._offset.Y, out newOffsetVisiblePosition);
			// JM 10-16-09 TFS23915 - Only reset the scrollingHigher variable if we did not get a scrollingHigherHint passed to this method.
			//if (newOffsetCurrentlyVisible && viewSettings.IsListContinuous == true && this.TotalItemCount > itemsPerPage)
			if (scrollingHigherHint.HasValue == false && newOffsetCurrentlyVisible && viewSettings.IsListContinuous == true && this.TotalItemCount > itemsPerPage)
				scrollingHigher = true;

			double offsetDelta			= newOffsetNormalized - this.ScrollingData._offset.Y;
			if (offsetDelta == 0)
				return;


			// If there is an existing scroll in progress, set the target location pct for all children equal to the current location pct
			//before starting the new scroll.
			// [JM 05-17-07] Change looping logic to use GetChildElement instead of iterating through the child elements collection.
			//UIElementCollection children		= this.Children;

			// JJD 6/6/07
			// Use Children collection to only access active children
			//int childrenCount = this.VisualChildrenCount; //children.Count;
			// AS 7/9/07
			//IList	children = this.Children;
			IList	children = this.ChildElements;
			int		childrenCount	= children.Count;
			if (this.ScrollInProgress)
			{
				this.StopAnimatedScrollProcessing();
				for (int i = 0; i < childrenCount; i++)
				{
					//(children[i] as CarouselPanelItem).PathTargetPercent = (children[i] as CarouselPanelItem).PathLocationPercent;
					// JJD 6/6/07
					// Use Children collection to only access active children
					//(this.GetChildElement(i) as CarouselPanelItem).PathTargetPercent = (this.GetChildElement(i) as CarouselPanelItem).PathLocationPercent;
					CarouselPanelItem child = children[i] as CarouselPanelItem;
					
					// JM 05-07-08 - BR30992 - Check for null Child.
					if (child == null)
						continue;

					child.PathTargetPercent = child.PathLocationPercent;
				}
			}


			// Start animated Scrolling.
			this.StartAnimatedScrollProcessing(new ScrollActionInfo(this.ScrollingData._offset.Y, newOffsetNormalized, scrollingHigher, this));


			// Update the ScrollData.
			double oldOffset = this.ScrollingData._offset.Y;
			this.UpdateScrollData(this.ScrollingData._extent,
								  this.ScrollingData._viewport,
								  new Vector(newOffsetNormalized, newOffsetNormalized));


			// If we are an itemsHost, generate child elements that are revealed by the scroll.
			if (this.IsItemsHost)
			{
//Debug.WriteLine("    In SetVerticalOffset Generating startItemIndex1:" + newOffsetNormalized.ToString() + " Length1: " + itemsPerPage.ToString() + " startItemIndex2: " + oldOffset.ToString() + " Length: " + itemsPerPage.ToString());
				this.GenerateElementsHelper((int)newOffsetNormalized, itemsPerPage, (int)oldOffset, itemsPerPage);
			}


			// Set:
			//		the target location for each item
			//		the current location pct for all new children so they 'scroll in' from the correct end
			// [JM 05-17-07] Change looping logic to use GetChildElement instead of iterating through the child elements collection.
			
			// JJD 6/6/07
			// Use Children collection to only access active children
			//childrenCount = this.VisualChildrenCount; //children.Count;
			childrenCount = children.Count;

			bool isListContinuous	= viewSettings.IsListContinuous;
			for (int i = 0; i < childrenCount; i++)
			{
				CarouselPanelItem	child					= children[i] as CarouselPanelItem;
				int					childItemIndex			= this.GetItemIndexFromChildIndex(i);
				int					visiblePositionBeforeScroll;
				int					visiblePositionAfterScroll;
				bool				itemVisibleBeforeScroll	= this.GetIsItemIndexVisible(childItemIndex, (int)oldOffset, out visiblePositionBeforeScroll);
				bool				itemVisibleAfterScroll	= this.GetIsItemIndexVisible(childItemIndex, (int)newOffsetNormalized, out visiblePositionAfterScroll);


				// Check for null here - this can happen if the user chooses 'Edit Layout of Items' on the CarouselListBox
				// in Blend. [BR19326]
				//Debug.Assert(child != null, "Child is null in SetVerticalOffset!");
				if (child == null)
					continue;

				// [JM 05-18-07] Reset the child's velocity.
				child.Velocity = 0;


				// Set the child's TARGET location as a percentage along the item path based on the new offset.
				child.PathTargetPercent = this.CalculateChildTargetPct(i, childItemIndex, (int)newOffsetNormalized, itemVisibleBeforeScroll, itemVisibleAfterScroll);


				// [JM 05-22-07] When we generated records above in preparation for the scroll, the recycling generator may have assigned different
				// elements to data records that were previously in view.  To ensure that the PathLocation property exposed on the element accurately
				// reflects the record's physical position, re-initialize the PathLocationPercent property now.  Note that we use
				// the old offset to do this.
				if (itemVisibleBeforeScroll == true)
					child.PathLocationPercent = this.CalculateChildTargetPct(i, childItemIndex, (int)oldOffset, true, true);


				// Make sure new items scroll in from the correct end.
				if (this._scrollActionInfo.ScrollingHigher == true)
				{
					if (itemVisibleAfterScroll == true && itemVisibleBeforeScroll == false)
					{
						if (child.IsPathLocationUnset  ||  isListContinuous == false)
							child.PathLocationPercent = 1;
						else
						if (isListContinuous && child.PathLocationPercent != 1)
							child.PathLocationPercent = 1;
					}
					else
					if (itemVisibleAfterScroll == true && itemVisibleBeforeScroll == true)
					{
						if ((visiblePositionAfterScroll > visiblePositionBeforeScroll)  &&
							 isListContinuous											&& 
							 child.PathLocationPercent != 1)
							child.PathLocationPercent = 1;
					}
				}
				else
				{
//if (child.PathLocationPercent == 1 && itemVisibleBeforeScroll == true)
//	Debug.WriteLine("Got One!");

					if (itemVisibleAfterScroll == true && itemVisibleBeforeScroll == false)
					{
						if (child.IsPathLocationUnset)
							child.PathLocationPercent = 0;
						else
						// [JM 05-29-07]
						//if (isListContinuous && child.PathLocationPercent != 0)
						if (child.PathLocationPercent != 0)
							child.PathLocationPercent = 0;
					}
					else
					if (itemVisibleAfterScroll == true && itemVisibleBeforeScroll == true)
					{
						if ((visiblePositionAfterScroll < visiblePositionBeforeScroll)	&&
							 isListContinuous											&&
							 child.PathLocationPercent != 0)
							child.PathLocationPercent = 0;
					}
				}

//XmlElement xml = ((FrameworkElement)child).DataContext as XmlElement;
//string name = "";
//if (xml != null)
//{
//    name = xml.Attributes["FirstName"].InnerText;
//}
//Debug.WriteLine("    In SetVerticalOffset Child #" + i.ToString() + "  Name: " + name + " pathLocationPct: " + ((CarouselPanelItem)child).PathLocationPercent.ToString() + ", VisibleBefore: " + itemVisibleBeforeScroll.ToString() + " - " + visiblePositionBeforeScroll.ToString() + ", VisibleAfter: " + itemVisibleAfterScroll.ToString() + " - " + visiblePositionAfterScroll.ToString() + ", Visibility = " + ((FrameworkElement)child).Visibility.ToString());
			}
		}

				#endregion //SetHorizontalOffset, SetVerticalOffset

				#region LineDown, LineUp, LineLeft, LineRight

		/// <summary>
		/// Scrolls down 1 line.
		/// </summary>
		public void LineDown()
		{
			// JM 10-16-09 TFS23915
			//this.SetVerticalOffset(this.VerticalOffset + SCROLL_SMALL_CHANGE);
			this.SetVerticalOffset(this.VerticalOffset + SCROLL_SMALL_CHANGE, true);
		}

		/// <summary>
		/// Scrolls left 1 line.
		/// </summary>
		public void LineLeft()
		{
			// JM 10-16-09 TFS23915
			//this.SetHorizontalOffset(this.HorizontalOffset - SCROLL_SMALL_CHANGE);
			this.SetHorizontalOffset(this.HorizontalOffset - SCROLL_SMALL_CHANGE, false);
		}

		/// <summary>
		/// Scrolls right 1 line.
		/// </summary>
		public void LineRight()
		{
			// JM 10-16-09 TFS23915
			//this.SetHorizontalOffset(this.HorizontalOffset + SCROLL_SMALL_CHANGE);
			this.SetHorizontalOffset(this.HorizontalOffset + SCROLL_SMALL_CHANGE, true);
		}

		/// <summary>
		/// Scrolls up 1 line.
		/// </summary>
		public void LineUp()
		{
			// JM 10-16-09 TFS23915
			//this.SetVerticalOffset(this.VerticalOffset - SCROLL_SMALL_CHANGE);
			this.SetVerticalOffset(this.VerticalOffset - SCROLL_SMALL_CHANGE, false);
		}

				#endregion //LineDown, LineUp, LineLeft, LineRight

				#region MouseWheelDown, MouseWheelUp, MouseWheelLeft, MouseWheelRight

		/// <summary>
		/// Performs scrolling action in response to a MouseWheelDown.
		/// </summary>
		public void MouseWheelDown()
		{
			// JM 10-16-09 TFS23915
			//this.SetVerticalOffset(this.VerticalOffset + SCROLL_SMALL_CHANGE);
			this.SetVerticalOffset(this.VerticalOffset + SCROLL_SMALL_CHANGE, true);
		}

		/// <summary>
		/// Performs scrolling action in response to a MouseWheelLeft.
		/// </summary>
		public void MouseWheelLeft()
		{
			// JM 10-16-09 TFS23915
			//this.SetVerticalOffset(this.VerticalOffset - SCROLL_SMALL_CHANGE);
			this.SetVerticalOffset(this.VerticalOffset - SCROLL_SMALL_CHANGE, false);
		}

		/// <summary>
		/// Performs scrolling action in response to a MouseWheelRight.
		/// </summary>
		public void MouseWheelRight()
		{
			// JM 10-16-09 TFS23915
			//this.SetVerticalOffset(this.VerticalOffset + SCROLL_SMALL_CHANGE);
			this.SetVerticalOffset(this.VerticalOffset + SCROLL_SMALL_CHANGE, true);
		}

		/// <summary>
		/// Performs scrolling action in response to a MouseWheelUp.
		/// </summary>
		public void MouseWheelUp()
		{
			// JM 10-16-09 TFS23915
			//this.SetVerticalOffset(this.VerticalOffset - SCROLL_SMALL_CHANGE);
			this.SetVerticalOffset(this.VerticalOffset - SCROLL_SMALL_CHANGE, false);
		}

				#endregion //MouseWheelDown, MouseWheelUp, MouseWheelLeft, MouseWheelRight

				#region PageUp, PageDown, PageLeft, PageRight

		/// <summary>
		/// Scrolls down 1 page.
		/// </summary>
		public void PageDown()
		{
			// JM 10-16-09 TFS23915
			//this.SetVerticalOffset(this.VerticalOffset + this.ScrollLargeChange);
			this.SetVerticalOffset(this.VerticalOffset + this.ScrollLargeChange, true);
		}

		/// <summary>
		/// Scrolls left 1 page.
		/// </summary>
		public void PageLeft()
		{
			// JM 10-16-09 TFS23915
			//this.SetHorizontalOffset(this.HorizontalOffset - this.ScrollLargeChange);
			this.SetHorizontalOffset(this.HorizontalOffset - this.ScrollLargeChange, false);
		}

		/// <summary>
		/// Scrolls right 1 page.
		/// </summary>
		public void PageRight()
		{
			// JM 10-16-09 TFS23915
			//this.SetHorizontalOffset(this.HorizontalOffset + this.ScrollLargeChange);
			this.SetHorizontalOffset(this.HorizontalOffset + this.ScrollLargeChange, true);
		}

		/// <summary>
		/// Scrolls up 1 page.
		/// </summary>
		public void PageUp()
		{
			// JM 10-16-09 TFS23915
			//this.SetVerticalOffset(this.VerticalOffset - this.ScrollLargeChange);
			this.SetVerticalOffset(this.VerticalOffset - this.ScrollLargeChange, false);
		}

				#endregion //PageUp, PageDown, PageLeft, PageRight

				#region ScrollOwner

		/// <summary>
		/// Returns/sets the scroll owner.
		/// </summary>
		public ScrollViewer ScrollOwner
		{
			get
			{
				return this.ScrollingData._scrollOwner;
			}
			set
			{
				if (value != this.ScrollingData._scrollOwner)
				{
					this.ScrollingData.Reset();
					this.ScrollingData._scrollOwner = value;
				}
			}
		}

				#endregion //ScrollOwner

				#region ViewportHeight, ViewportWidth

		/// <summary>
		/// Returns the height of the Viewport. (read only)
		/// </summary>
		public double ViewportHeight
		{
			get
			{
				return this.ScrollingData._viewport.Height;
			}
		}

		/// <summary>
		/// Returns the width of the Viewport. (read only)
		/// </summary>
		public double ViewportWidth
		{
			get
			{
				return this.ScrollingData._viewport.Width;
			}
		}

				#endregion //ViewportHeight, ViewportWidth

				#region MakeVisible

		/// <summary>
		/// Ensures that the supplied parent is visible.
		/// </summary>
		/// <param name="visual">The element to make visible.</param>
		/// <param name="rectangle">The rectangle within the parent to make visible.</param>
		/// <returns>The rectangle that was actually made visible.</returns>
		/// <remarks>
		/// <p class="note"><b>Note: </b>When this method is called the specified parent is assumed to be a CarouselPanelItem.
		/// If it is, the entire item is scrolled into view if necessary and the rectangle paramater is ignored.</p>
		/// </remarks>
		public Rect MakeVisible(Visual visual, Rect rectangle)
		{
			CarouselPanelItem carouselPanelItem = visual as CarouselPanelItem;
			if (carouselPanelItem != null)
			{
				// [JM 05-17-07] Change looping logic to use GetChildElement instead of iterating through the child elements collection.
				int childrenCount = this.VisualChildrenCount; //base.Children.Count;
				for(int i = 0; i < childrenCount; i++)
				{
					//UIElement child = base.Children[i];
					// JJD 6/6/07
					//UIElement child = this.GetChildElement(i) as UIElement;
					UIElement child = this.GetVisualChild(i) as UIElement;
					if (child == carouselPanelItem)
					{
						this.EnsureItemIsVisible(base.GetItemIndexFromChildIndex(i));
						break;
					}
				}
			}

			return rectangle;
		}

				#endregion //MakeVisible

				#region CanVerticallyScroll, CanHorizontallyScroll

		/// <summary>
		/// Returns/sets whether vertical scrolling can be performed.
		/// </summary>
		// AS 3/22/07 BR21223
		//public bool CanVerticallyScroll
		bool IScrollInfo.CanVerticallyScroll
		{
			get
			{

				return this.ScrollingData._canVerticallyScroll;
			}
			set
			{
				if (this.ScrollingData._canVerticallyScroll != value)
				{
					this.ScrollingData._canVerticallyScroll = value;
					this.InvalidateMeasure();
				}
			}
		}

		/// <summary>
		/// Returns/sets whether horizontal scrolling can be performed.
		/// </summary>
		// AS 3/22/07 BR21223
		//public bool CanHorizontallyScroll
		bool IScrollInfo.CanHorizontallyScroll
		{
			get
			{

				return this.ScrollingData._canHorizontallyScroll;
			}
			set
			{
				if (this.ScrollingData._canHorizontallyScroll != value)
				{
					this.ScrollingData._canHorizontallyScroll = value;
					this.InvalidateMeasure();
				}
			}
		}

				#endregion //CanVerticallyScroll, CanHorizontallyScroll

			#endregion //IScrollInfo Members

		#endregion //Implemented Interfaces

		#region Properties

			#region Public Properties
	
				#region CanNavigateToPreviousItem

		private static readonly DependencyPropertyKey CanNavigateToPreviousItemPropertyKey =
			DependencyProperty.RegisterReadOnly("CanNavigateToPreviousItem",
			typeof(bool), typeof(XamCarouselPanel), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		/// <summary>
		/// Identifies the <see cref="CanNavigateToPreviousItem"/> dependency property
		/// </summary>
		public static readonly DependencyProperty CanNavigateToPreviousItemProperty =
			CanNavigateToPreviousItemPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns true if navigation to a previous item is possible. (read only)
		/// </summary>
		/// <remarks>
		/// <p class="body">To perform navigation in the <see cref="XamCarouselPanel"/> use one of the <see cref="XamCarouselPanelCommands"/></p>
		/// </remarks>
		/// <seealso cref="CanNavigateToPreviousItemProperty"/>
		/// <seealso cref="CanNavigateToPreviousPage"/>
		/// <seealso cref="CanNavigateToNextItem"/>
		/// <seealso cref="CanNavigateToNextPage"/>
		/// <seealso cref="XamCarouselPanel"/>
		/// <seealso cref="XamCarouselPanelCommands"/>
		//[Description("Returns true if navigation to a previous item is possible. (read only)")]
		//[Category("Behavior")]
		[Bindable(true)]
		[ReadOnly(true)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool CanNavigateToPreviousItem
		{
			get
			{
				return (bool)this.GetValue(XamCarouselPanel.CanNavigateToPreviousItemProperty);
			}
		}

				#endregion //CanNavigateToPreviousItem

				#region CanNavigateToPreviousPage

		private static readonly DependencyPropertyKey CanNavigateToPreviousPagePropertyKey =
			DependencyProperty.RegisterReadOnly("CanNavigateToPreviousPage",
			typeof(bool), typeof(XamCarouselPanel), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		/// <summary>
		/// Identifies the <see cref="CanNavigateToPreviousPage"/> dependency property
		/// </summary>
		public static readonly DependencyProperty CanNavigateToPreviousPageProperty =
			CanNavigateToPreviousPagePropertyKey.DependencyProperty;

		/// <summary>
		/// Returns true if navigation to a previous page of items is possible. (read only)
		/// </summary>
		/// <remarks>
		/// <p class="body">To perform navigation in the <see cref="XamCarouselPanel"/> use one of the <see cref="XamCarouselPanelCommands"/></p>
		/// </remarks>
		/// <seealso cref="CanNavigateToPreviousPageProperty"/>
		/// <seealso cref="CanNavigateToPreviousItem"/>
		/// <seealso cref="CanNavigateToNextItem"/>
		/// <seealso cref="CanNavigateToNextPage"/>
		/// <seealso cref="XamCarouselPanel"/>
		/// <seealso cref="XamCarouselPanelCommands"/>
		//[Description("Returns true if navigation to a previous page of items is possible. (read only)")]
		//[Category("Behavior")]
		[Bindable(true)]
		[ReadOnly(true)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool CanNavigateToPreviousPage
		{
			get
			{
				return (bool)this.GetValue(XamCarouselPanel.CanNavigateToPreviousPageProperty);
			}
		}

				#endregion //CanNavigateToPreviousPage

				#region CanNavigateToNextItem

		private static readonly DependencyPropertyKey CanNavigateToNextItemPropertyKey =
			DependencyProperty.RegisterReadOnly("CanNavigateToNextItem",
			typeof(bool), typeof(XamCarouselPanel), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		/// <summary>
		/// Identifies the <see cref="CanNavigateToNextItem"/> dependency property
		/// </summary>
		public static readonly DependencyProperty CanNavigateToNextItemProperty =
			CanNavigateToNextItemPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns true if navigation to a following item is possible. (read only)
		/// </summary>
		/// <remarks>
		/// <p class="body">To perform navigation in the <see cref="XamCarouselPanel"/> use one of the <see cref="XamCarouselPanelCommands"/></p>
		/// </remarks>
		/// <seealso cref="CanNavigateToNextItemProperty"/>
		/// <seealso cref="CanNavigateToNextPage"/>
		/// <seealso cref="CanNavigateToPreviousItem"/>
		/// <seealso cref="CanNavigateToPreviousPage"/>
		/// <seealso cref="XamCarouselPanel"/>
		/// <seealso cref="XamCarouselPanelCommands"/>
		//[Description("Returns true if navigation to a following item is possible. (read only)")]
		//[Category("Behavior")]
		[Bindable(true)]
		[ReadOnly(true)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool CanNavigateToNextItem
		{
			get
			{
				return (bool)this.GetValue(XamCarouselPanel.CanNavigateToNextItemProperty);
			}
		}

				#endregion //CanNavigateToNextItem

				#region CanNavigateToNextPage

		private static readonly DependencyPropertyKey CanNavigateToNextPagePropertyKey =
			DependencyProperty.RegisterReadOnly("CanNavigateToNextPage",
			typeof(bool), typeof(XamCarouselPanel), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		/// <summary>
		/// Identifies the <see cref="CanNavigateToNextPage"/> dependency property
		/// </summary>
		public static readonly DependencyProperty CanNavigateToNextPageProperty =
			CanNavigateToNextPagePropertyKey.DependencyProperty;

		/// <summary>
		/// Returns true if navigation to a following page of items is possible. (read only)
		/// </summary>
		/// <remarks>
		/// <p class="body">To perform navigation in the <see cref="XamCarouselPanel"/> use one of the <see cref="XamCarouselPanelCommands"/></p>
		/// </remarks>
		/// <seealso cref="CanNavigateToNextPageProperty"/>
		/// <seealso cref="CanNavigateToNextItem"/>
		/// <seealso cref="CanNavigateToPreviousItem"/>
		/// <seealso cref="CanNavigateToPreviousPage"/>
		/// <seealso cref="XamCarouselPanel"/>
		/// <seealso cref="XamCarouselPanelCommands"/>
		//[Description("Returns true if navigation to a following page of items is possible. (read only)")]
		//[Category("Behavior")]
		[Bindable(true)]
		[ReadOnly(true)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool CanNavigateToNextPage
		{
			get
			{
				return (bool)this.GetValue(XamCarouselPanel.CanNavigateToNextPageProperty);
			}
		}

				#endregion //CanNavigateToNextPage

				#region FirstVisibleItemIndex

		private static readonly DependencyPropertyKey FirstVisibleItemIndexPropertyKey =
			DependencyProperty.RegisterReadOnly("FirstVisibleItemIndex",
			typeof(int), typeof(XamCarouselPanel), new FrameworkPropertyMetadata(0));

		/// <summary>
		/// Identifies the <see cref="FirstVisibleItemIndex"/> dependency property
		/// </summary>
		public static readonly DependencyProperty FirstVisibleItemIndexProperty =
			FirstVisibleItemIndexPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns a number that represents the visible item with the lowest index. (read only)
		/// </summary>
		/// <seealso cref="FirstVisibleItemIndexProperty"/>
		//[Description("Returns a number that represents the visible item with the lowest index. (read only)")]
		//[Category("Data")]
		[Bindable(true)]
		[ReadOnly(true)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int FirstVisibleItemIndex
		{
			get
			{
				return (int)this.GetValue(XamCarouselPanel.FirstVisibleItemIndexProperty);
			}
		}

				#endregion //FirstVisibleItemIndex

				#region FocalPoint Properties

		//                #region FocalPointItemPosition

//        /// <summary>
//        /// Identifies the <see cref="FocalPointItemPosition"/> dependency property
//        /// </summary>
//        public static readonly DependencyProperty FocalPointItemPositionProperty = DependencyProperty.Register("FocalPointItemPosition",
//            typeof(int), typeof(XamCarouselPanel), new FrameworkPropertyMetadata((int)0, null, new CoerceValueCallback(OnCoerceFocalPointItemPosition)));

//        private static object OnCoerceFocalPointItemPosition(DependencyObject d, object value)
//        {
//            if ((int)value < 0)
//                return 0;

//            return value;
//        }

//        /// <summary>
//        /// Returns/sets the zero based position within the list of visible items that should be used as the focal point.
//        /// </summary>
//        /// <remarks>
//        /// The default is 0.
//        /// </remarks>
//        /// <seealso cref="FocalPointItemPositionProperty"/>
//        /// <seealso cref="FocalPointItemPositionResolved"/>
//        /// <seealso cref="ItemClickAction"/>
//        /// <seealso cref="SyncActiveItemAndFocalPoint"/>
//        [Description("Returns/sets the zero based position within the list of visible items that should be used as the focal point.")]
////		[Category("Behavior")]
//        [Bindable(true)]
//        public int FocalPointItemPosition
//        {
//            get
//            {
//                return (int)this.GetValue(XamCarouselPanel.FocalPointItemPositionProperty);
//            }
//            set
//            {
//                this.SetValue(XamCarouselPanel.FocalPointItemPositionProperty, value);
//            }
//        }

//                #endregion //FocalPointItemPosition

//                #region FocalPointItemPositionResolved

//        /// <summary>
//        /// Returns the value of the <see cref="FocalPointItemPosition"/> property making sure that it is within range based on the <see cref="ItemsPerPage"/> property.
//        /// </summary>
//        /// <remarks>
//        /// If the value of the <see cref="FocalPointItemPosition"/> property is greater than <see cref="ItemsPerPage"/> - 1 then the value returned form this property will be <see cref="ItemsPerPage"/> - 1.
//        /// </remarks>
//        /// <seealso cref="ItemClickAction"/>
//        /// <seealso cref="FocalPointItemPosition"/>
//        /// <seealso cref="SyncActiveItemAndFocalPoint"/>
//        [Description("If the value of the FocalPointItemPosition property is greater than ItemsPerPage - 1 then the value returned form this property will be ItemsPerPage - 1.")]
//        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
//        public int FocalPointItemPositionResolved
//        {
//            get
//            {
//                return Math.Min(Math.Max(0, this.FocalPointItemPosition), this.ItemsPerPage - 1);
//            }
//        }

		//                #endregion //FocalPointItemPositionResolved

		#endregion FocalPoint Properties

				#region ItemsPerPageResolved

		/// <summary>
		/// Returns the lesser of the total number of items in the list and the <see cref="CarouselViewSettings.ItemsPerPage"/>.  This is the number
		/// used by the <see cref="XamCarouselPanel"/> to determine the actual number of items per page. (read only)
		/// </summary>
		/// <remarks>
		/// <p class="body">If the total number of items in the list is less than <see cref="CarouselViewSettings.ItemsPerPage"/>, the <see cref="XamCarouselPanel"/> will spread out those
		/// items evenly along the <see cref="CarouselViewSettings.ItemPath"/> so they take up all the space between the prefix and suffix areas.  For example, if the number of items per page is set to 10 and the total
		/// number of items in the list is 20, this property will return 10 and the 10 displayed items will be arranged along the <see cref="CarouselViewSettings.ItemPath"/> between the prefix and suffix areas using even spacing.
		/// If the number of items in the list is 5, this property will return 5 and those 5 items will be evenly arranged along the <see cref="CarouselViewSettings.ItemPath"/> between the prefix and
		/// suffix areas, but with increased spacing so the entire areas is used.  This prevents items from being 'bunched up' at the beginning of the path.</p>
		/// </remarks>
		/// <seealso cref="XamCarouselPanel"/>
		/// <seealso cref="CarouselViewSettings.ItemsPerPage"/>
		/// <seealso cref="CarouselViewSettings.ItemPath"/>
		[Bindable(true)]
		[ReadOnly(true)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int ItemsPerPageResolved
		{
			get
			{
				if (this.ViewSettingsInternal != null)
					return Math.Max(1, Math.Min(this.TotalItemCount, this.ViewSettingsInternal.ItemsPerPage));

				return Math.Max(1, Math.Min(this.TotalItemCount, CarouselViewSettings.DEFAULT_ITEMS_PER_PAGE));
			}
		}

				#endregion //ItemsPerPageResolved	
    
				#region OpacityEffectStopsResolved

		/// <summary>
		/// Return the OpacityEffectStops collection (if one has been defined and populated), otherwise returns an internally generated default collection.  
		/// The default collection contains 2 stops with offset,value of 0,1 and 1,1
		/// </summary>
		/// <seealso cref="OpacityEffectStop"/>
		/// <seealso cref="CarouselViewSettings.OpacityEffectStops"/>
		//[Description("Return the OpacityEffectStops collection (if one has been defined and populated), otherwise returns an internally generated default collection.  The default collection contains 2 stops with offset & value of 0,1 and 1,1")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public OpacityEffectStopCollection OpacityEffectStopsResolved
		{
			get
			{
				if (this.ViewSettingsInternal.OpacityEffectStops == null || this.ViewSettingsInternal.OpacityEffectStops.Count < 1)
					return this.DefaultOpacityEffectStops;
				else
					return this.ViewSettingsInternal.OpacityEffectStops;
			}
		}

				#endregion //OpacityEffectStopsResolved

				#region OpacityEffectStopDirectionResolved

		/// <summary>
		/// Returns the value of the <see cref="CarouselViewSettings.OpacityEffectStopDirection"/> property if set to something other than 'Default' otherwise returns a default value of <see cref="EffectStopDirection"/>.UseItemPath.
		/// </summary>
		/// <seealso cref="CarouselViewSettings.OpacityEffectStopDirection"/>
		/// <seealso cref="EffectStopDirection"/>
		//[Description("Returns the value of the OpacityEffectStopDirection property if set to something other than 'Default' otherwise returns a default value of 'UseItemPath'.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public EffectStopDirection OpacityEffectStopDirectionResolved
		{
			get
			{
				if (this.ViewSettingsInternal.OpacityEffectStopDirection == EffectStopDirection.Default)
					return EffectStopDirection.UseItemPath;

				return this.ViewSettingsInternal.OpacityEffectStopDirection;
			}

		}

				#endregion //OpacityEffectStopDirectionResolved	
    
				#region ReflectionVisibility

		private static readonly DependencyPropertyKey ReflectionVisibilityPropertyKey =
			DependencyProperty.RegisterReadOnly("ReflectionVisibility",
			typeof(Visibility), typeof(XamCarouselPanel), new FrameworkPropertyMetadata(KnownBoxes.VisibilityVisibleBox));

		/// <summary>
		/// Identifies the <see cref="ReflectionVisibility"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ReflectionVisibilityProperty =
			ReflectionVisibilityPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the Visibility of the reflection displayed for each item in the list.
		/// </summary>
		/// <remarks>
		/// This is a read-only property that can be used to determine the visibility of elements used to render a reflection in an item's style.
		/// This property returns Visibility.Visible if the <see cref="CarouselViewSettings.ReserveSpaceForReflections"/> property is set to true, other wise it returns Visibility.Hidden.
		/// </remarks>
		/// <seealso cref="ReflectionVisibilityProperty"/>
		/// <seealso cref="CarouselViewSettings.ReserveSpaceForReflections"/>
		//[Description("Returns the Visibility of the reflection displayed for each item in the list.")]
//		[Category("Appearance")]
		[Bindable(true)]
		[ReadOnly(true)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Visibility ReflectionVisibility
		{
			get
			{
				return (Visibility)this.GetValue(XamCarouselPanel.ReflectionVisibilityProperty);
			}
		}

				#endregion //ReflectionVisibility
    
				#region ScalingEffectStopsResolved

		/// <summary>
		/// Return the ScalingEffectStops collection (if one has been defined and populated), otherwise returns an internally generated default collection.  
		/// The default collection contains 2 stops with offset,value of 0,1 and 1,1
		/// </summary>
		/// <seealso cref="ScalingEffectStop"/>
		/// <seealso cref="CarouselViewSettings.ScalingEffectStops"/>
		//[Description("Return the ScalingEffectStops collection (if one has been defined and populated), otherwise returns an internally generated default collection.  The default collection contains 2 stops with offset & value of 0,1 and 1,1")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public ScalingEffectStopCollection ScalingEffectStopsResolved
		{
			get
			{
				if (this.ViewSettingsInternal.ScalingEffectStops == null || this.ViewSettingsInternal.ScalingEffectStops.Count < 1)
					return this.DefaultScalingEffectStops;
				else
					return this.ViewSettingsInternal.ScalingEffectStops;
			}
		}

				#endregion //ScalingEffectStopsResolved	

				#region ScalingEffectStopDirectionResolved

		/// <summary>
		/// Returns the value of the <see cref="CarouselViewSettings.ScalingEffectStopDirection"/> property if set to something other than 'Default' otherwise returns a default value of <see cref="EffectStopDirection"/>.UseItemPath.
		/// </summary>
		/// <seealso cref="CarouselViewSettings.ScalingEffectStopDirection"/>
		/// <seealso cref="EffectStopDirection"/>
		//[Description("Returns the value of the ScalingEffectStopDirection property if set to something other than 'Default' otherwise returns a default value of 'UseItemPath'.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public EffectStopDirection ScalingEffectStopDirectionResolved
		{
			get
			{
				if (this.ViewSettingsInternal.ScalingEffectStopDirection == EffectStopDirection.Default)
					return EffectStopDirection.UseItemPath;

				return this.ViewSettingsInternal.ScalingEffectStopDirection;
			}

		}

				#endregion //ScalingEffectStopDirectionResolved	

				#region SelectionHost

		/// <summary>
		/// Returns/sets a reference to an <see cref="ICarouselPanelSelectionHost"/> interface implemented by the control that is hosting this <see cref="XamCarouselPanel"/>.
		/// </summary>
		/// <remarks>
		/// Note: This property should be set by a control that hosts the <see cref="XamCarouselPanel"/> if it supports selection.
		/// </remarks>
		/// <seealso cref="XamCarouselPanel"/>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public ICarouselPanelSelectionHost SelectionHost
		{
			get { return this._selectionHost; }
			set { this._selectionHost = value; }
		}

				#endregion //SelectionHost

				#region SkewAngleXEffectStopsResolved

		/// <summary>
		/// Return the SkewAngleXEffectStops collection (if one has been defined and populated), otherwise returns an internally generated default collection.  
		/// The default collection contains 2 stops with offset,value of 0,0 and 1,0
		/// </summary>
		/// <seealso cref="SkewAngleXEffectStop"/>
		/// <seealso cref="CarouselViewSettings.SkewAngleXEffectStops"/>
		//[Description("Return the SkewAngleXEffectStops collection (if one has been defined and populated), otherwise returns an internally generated default collection.  The default collection contains 2 stops with offset & value of 0,0 and 1,0")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public SkewAngleXEffectStopCollection SkewAngleXEffectStopsResolved
		{
			get
			{
				if (this.ViewSettingsInternal.SkewAngleXEffectStops == null || this.ViewSettingsInternal.SkewAngleXEffectStops.Count < 1)
					return this.DefaultSkewAngleXEffectStops;
				else
					return this.ViewSettingsInternal.SkewAngleXEffectStops;
			}
		}

				#endregion //SkewAngleXEffectStopsResolved

				#region SkewAngleXEffectStopDirectionResolved

		/// <summary>
		/// Returns the value of the <see cref="CarouselViewSettings.SkewAngleXEffectStopDirection"/> property if set to something other than 'Default' otherwise returns a default value of <see cref="EffectStopDirection"/>.UseItemPath.
		/// </summary>
		/// <seealso cref="CarouselViewSettings.SkewAngleXEffectStopDirection"/>
		/// <seealso cref="EffectStopDirection"/>
		//[Description("Returns the value of the SkewAngleXEffectStopDirection property if set to something other than 'Default' otherwise returns a default value of 'UseItemPath'.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public EffectStopDirection SkewAngleXEffectStopDirectionResolved
		{
			get
			{
				if (this.ViewSettingsInternal.SkewAngleXEffectStopDirection == EffectStopDirection.Default)
					return EffectStopDirection.UseItemPath;

				return this.ViewSettingsInternal.SkewAngleXEffectStopDirection;
			}

		}

				#endregion //SkewAngleXEffectStopDirectionResolved	

				#region SkewAngleYEffectStopsResolved

		/// <summary>
		/// Return the SkewAngleYEffectStops collection (if one has been defined and populated), otherwise returns an internally generated default collection.  
		/// The default collection contains 2 stops with offset,value of 0,0 and 1,0
		/// </summary>
		/// <seealso cref="SkewAngleYEffectStop"/>
		/// <seealso cref="CarouselViewSettings.SkewAngleYEffectStops"/>
		//[Description("Return the SkewAngleYEffectStops collection (if one has been defined and populated), otherwise returns an internally generated default collection.  The default collection contains 2 stops with offset & value of 0,0 and 1,0")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public SkewAngleYEffectStopCollection SkewAngleYEffectStopsResolved
		{
			get
			{
				if (this.ViewSettingsInternal.SkewAngleYEffectStops == null || this.ViewSettingsInternal.SkewAngleYEffectStops.Count < 1)
					return this.DefaultSkewAngleYEffectStops;
				else
					return this.ViewSettingsInternal.SkewAngleYEffectStops;
			}
		}

				#endregion //SkewAngleYEffectStopsResolved

				#region SkewAngleYEffectStopDirectionResolved

		/// <summary>
		/// Returns the value of the <see cref="CarouselViewSettings.SkewAngleYEffectStopDirection"/> property if set to something other than 'Default' otherwise returns a default value of <see cref="EffectStopDirection"/>.UseItemPath.
		/// </summary>
		/// <seealso cref="CarouselViewSettings.SkewAngleYEffectStopDirection"/>
		/// <seealso cref="EffectStopDirection"/>
		//[Description("Returns the value of the SkewAngleYEffectStopDirection property if set to something other than 'Default' otherwise returns a default value of 'UseItemPath'.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public EffectStopDirection SkewAngleYEffectStopDirectionResolved
		{
			get
			{
				if (this.ViewSettingsInternal.SkewAngleYEffectStopDirection == EffectStopDirection.Default)
					return EffectStopDirection.UseItemPath;

				return this.ViewSettingsInternal.SkewAngleYEffectStopDirection;
			}

		}

				#endregion //SkewAngleYEffectStopDirectionResolved	

		#region FocalPoint Properties

//                #region SyncActiveItemAndFocalPoint

//        /// <summary>
//        /// Identifies the <see cref="SyncActiveItemAndFocalPoint"/> dependency property
//        /// </summary>
//        public static readonly DependencyProperty SyncActiveItemAndFocalPointProperty = DependencyProperty.Register("SyncActiveItemAndFocalPoint",
//            typeof(bool), typeof(XamCarouselPanel), new FrameworkPropertyMetadata(false)); 

//        /// <summary>
//        /// Returns/sets whether the item considered to be the 'Active' item is always kept in the <see cref="FocalPointItemPosition"/>.
//        /// </summary>
//        /// <remarks>
//        /// The default is false.
//        /// If this property is set to true and the a new item is made active either programatically or via an item click (see <see cref="ItemClickAction"/>) the item will be automatically scrolled into the <see cref="FocalPointItemPosition"/>.
//        /// Conversely, if the control is scrolled, the item that resides in the <see cref="FocalPointItemPosition"/> at the conclusion of the scroll will be made the active item.
//        /// </remarks>
//        /// <seealso cref="SyncActiveItemAndFocalPointProperty"/>
//        /// <seealso cref="ItemClickAction"/>
//        /// <seealso cref="FocalPointItemPosition"/>
//        /// <seealso cref="FocalPointItemPositionResolved"/>
//        [Description("Returns/sets whether the item considered to be the 'Active' item is always kept in the FocalPointItemPosition.")]
////		[Category("Behavior")]
//        [Bindable(true)]
//        public bool SyncActiveItemAndFocalPoint
//        {
//            get
//            {
//                return (bool)this.GetValue(XamCarouselPanel.SyncActiveItemAndFocalPointProperty);
//            }
//            set
//            {
//                this.SetValue(XamCarouselPanel.SyncActiveItemAndFocalPointProperty, value);
//            }
//        }

//                #endregion //SyncActiveItemAndFocalPoint

		#endregion FocalPoint Properties

				#region ViewSettings

		/// <summary>
		/// Identifies the <see cref="ViewSettings"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ViewSettingsProperty = DependencyProperty.Register("ViewSettings",
			typeof(CarouselViewSettings), typeof(XamCarouselPanel), new FrameworkPropertyMetadata(null, new CoerceValueCallback(OnCoerceViewSettings)));

		private static object OnCoerceViewSettings(DependencyObject d, object value)
		{
            if (value == null)
            {
                // JJD 1/2/07 
                // Check for design mode first
                // Only create a ViewSettings if we are in run mode 
                if (DesignerProperties.GetIsInDesignMode(d) == false)
                    return new CarouselViewSettings();
            }

			return value;
		}

		/// <summary>
		/// Returns/set the <see cref="CarouselViewSettings"/> object for this <see cref="XamCarouselPanel"/>.
		/// </summary>
		/// <seealso cref="ViewSettingsProperty"/>
		//[Description("Returns/set the CarouselViewSettings object for this XamCarouselPanel.")]
		//[Category("Appearance")]
		[Bindable(true)]
		public CarouselViewSettings ViewSettings
		{
			get
			{
                // JJD 1/2/07 
                // Check for design mode first
                // If we are in design mode don't create an instance of the ViewSettings object
//				if (this._viewSettings == null)
				if (this._viewSettings == null &&
                    DesignerProperties.GetIsInDesignMode(this) == false)
				{
					this._viewSettings = new CarouselViewSettings();
					// AS 6/4/09 TFS18192
					// It seems that we used to use a weakeventlistener at one point but then 
					// changed it. It could be that this was changed accidentally. 
					//
					//// JM 4-5-07
					////PropertyChangedEventManager.AddListener(this._viewSettings, this, string.Empty);
					//this._viewSettings.PropertyChanged += new PropertyChangedEventHandler(OnViewSettingsPropertyChanged);
					PropertyChangedEventManager.AddListener(this._viewSettings, this, string.Empty);
                }

				return this._viewSettings;
			}
			set
			{
				this.SetValue(XamCarouselPanel.ViewSettingsProperty, value);
			}
		}

		/// <summary>
		/// Determines if the <see cref="ViewSettings"/> property needs to be serialized.
		/// </summary>
		/// <returns>True if the property should be serialized</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeViewSettings()
		{
			return this.ViewSettings != (CarouselViewSettings)XamCarouselPanel.ViewSettingsProperty.DefaultMetadata.DefaultValue;
		}

		/// <summary>
		/// Resets the <see cref="ViewSettings"/> property to its default value.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetViewSettings()
		{
			this.ViewSettings = (CarouselViewSettings)XamCarouselPanel.ViewSettingsProperty.DefaultMetadata.DefaultValue;
		}

				#endregion //ViewSettings

				#region ZOrderEffectStopsResolved

		/// <summary>
		/// Return the ZOrderEffectStops collection (if one has been defined and populated), otherwise returns an internally generated default collection.  
		/// The default collection contains 2 stops with offset,value of 0,0 and 1,1
		/// </summary>
		/// <seealso cref="CarouselViewSettings.ZOrderEffectStops"/>
		/// <seealso cref="ZOrderEffectStop"/>
		/// <seealso cref="CarouselViewSettings.UseZOrder"/>
		//[Description("Return the ZOrderEffectStops collection (if one has been defined and populated), otherwise returns an internally generated default collection.  The default collection contains 2 stops with offset & value of 0,0 and 1,1")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public ZOrderEffectStopCollection ZOrderEffectStopsResolved
		{
			get
			{
				if (this.ViewSettingsInternal.UseZOrder			== false	||
					this.ViewSettingsInternal.ZOrderEffectStops == null		||
					this.ViewSettingsInternal.ZOrderEffectStops.Count < 1)
					return this.DefaultZOrderEffectStops;
				else
					return this.ViewSettingsInternal.ZOrderEffectStops;
			}
		}

				#endregion //ZOrderEffectStopsResolved

				#region ZOrderEffectStopDirectionResolved

		/// <summary>
		/// Returns the value of the <see cref="CarouselViewSettings.ZOrderEffectStopDirection"/> property if set to something other than 'Default' otherwise returns a default value of <see cref="EffectStopDirection"/>.UseItemPath.
		/// </summary>
		/// <seealso cref="CarouselViewSettings.ZOrderEffectStopDirection"/>
		//[Description("Returns the value of the ZOrderEffectStopDirection property if set to something other than 'Default' otherwise returns a default value of 'UseItemPath'.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public EffectStopDirection ZOrderEffectStopDirectionResolved
		{
			get
			{
				if (this.ViewSettingsInternal.UseZOrder					== false  ||
					this.ViewSettingsInternal.ZOrderEffectStopDirection == EffectStopDirection.Default)
					return EffectStopDirection.UseItemPath;

				return this.ViewSettingsInternal.ZOrderEffectStopDirection;
			}

		}

				#endregion //ZOrderEffectStopDirectionResolved	

			#endregion //Public Properties

			#region Internal Properties

				#region FirstVisibleChildIndex






		internal int FirstVisibleChildIndex
		{
			get { return base.GetChildIndexFromItemIndex(this.FirstVisibleItemIndex); }
		}

				#endregion //FirstVisibleChildIndex

				#region IsInCarouselListBox

		/// <summary>
		/// Identifies the <see cref="IsInCarouselListBox"/> dependency property
		/// </summary>
		internal static readonly DependencyProperty IsInCarouselListBoxProperty = DependencyProperty.Register("IsInCarouselListBox",
			typeof(bool), typeof(XamCarouselPanel), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		internal bool IsInCarouselListBox
		{
			get
			{
				return (bool)this.GetValue(XamCarouselPanel.IsInCarouselListBoxProperty);
			}
			set
			{
				this.SetValue(XamCarouselPanel.IsInCarouselListBoxProperty, value);
			}
		}

				#endregion //IsInCarouselListBox

				// [JM 05-21-07] Removed (use GetIsListWrapping instead)
				#region IsListWrapping

		//internal bool IsListWrapping
		//{
		//    get { return this.GetIsListWrapping(this.FirstVisibleItemIndex); }
		//}

				#endregion //IsListWrapping	

				#region ScrollingData

		internal ScrollData ScrollingData
		{
			get
			{
				if (this._scrollingData == null)
					this._scrollingData = new ScrollData();

				return this._scrollingData;
			}
		}

				#endregion //#region ScrollingData

				#region ScrollLargeChange

		internal int ScrollLargeChange
		{
			get
			{
				return this.ItemsPerPageResolved;
			}
		}

				#endregion //ScrollLargeChange	

                #region ViewSettingsInternal

        // JJD 1/2/07 
        // Added ViewSettingsInternal so we could create a default ViewSettings for use at design time
        // to allow the VS2008 designers to work
        internal CarouselViewSettings ViewSettingsInternal
        {
            get
            {
                CarouselViewSettings vs = this.ViewSettings;

                // JJD 1/2/07 
                // The public ViewSettings property will now return null at design time
                // to allow the VS2008 designers to work
                if (vs == null)
                {
                    if (this._designTimeDefaultViewSettings == null)
                        this._designTimeDefaultViewSettings = new CarouselViewSettings();

                    return this._designTimeDefaultViewSettings;
                }

                return vs;
            }
        }

                #endregion //ViewSettingsInternal	
        
			#endregion //Internal Properties

			#region Protected Properties
    
				#region ItemsHostSupportsSelection

		/// <summary>
		/// Returns true if the ItemsHost supports selection.
		/// </summary>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		protected bool ItemsHostSupportsSelection
		{
			get { return this.SelectionHost != null; }
		}

				#endregion //ItemsHostSupportsSelection	
  
				#region CarouselPanelAdorner

		/// <summary>
		/// Returns a reference to the <see cref="CarouselPanelAdorner"/> used by the <see cref="XamCarouselPanel"/>
		/// </summary>
		/// <seealso cref="XamCarouselPanel"/>
		/// <seealso cref="CarouselPanelAdorner"/>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		internal protected CarouselPanelAdorner CarouselPanelAdorner
		{
			get { return this._carouselPanelAdorner; }
		}

				#endregion //CarouselPanelAdorner	
    
				#region CurrentState
		/// <summary>
        /// Returns a set of flags (<see cref="XamCarouselPanelStates"/>) that indicate the current state of the panel.
		/// </summary>
        /// <seealso cref="XamCarouselPanelStates"/>
		protected virtual long CurrentState
		{
			get
			{
				long state = 0;

				if (this.CanNavigateToNextItem)
					state |= (long)XamCarouselPanelStates.CanNavigateToNextItem;

				if (this.CanNavigateToPreviousItem)
					state |= (long)XamCarouselPanelStates.CanNavigateToPreviousItem;

				if (this.CanNavigateToNextPage)
					state |= (long)XamCarouselPanelStates.CanNavigateToNextPage;

				if (this.CanNavigateToPreviousPage)
					state |= (long)XamCarouselPanelStates.CanNavigateToPreviousPage;

				return state;
			}
		} 
				#endregion //CurrentState

				#region ScrollOffsetForInitialArrangeAnimation

		/// <summary>
		/// Final scroll offset to animate towards when doing an initial arrange animation.
		/// </summary>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		protected double ScrollOffsetForInitialArrangeAnimation
		{
			get { return this._scrollOffsetForInitialArrangeAnimation; }
			set { this._scrollOffsetForInitialArrangeAnimation = value; }
		}

				#endregion //ScrollOffsetForInitialArrangeAnimation	
    
				#region ScrollInProgress

		/// <summary>
		/// Returns true if a scroll is in progress.
		/// </summary>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		protected bool ScrollInProgress
		{
			get { return this._scrollActionInfo.IsEmpty != true; }
		}

				#endregion //ScrollInProgress

				#region ShouldGenerateElements

		/// <summary>
		/// Returns/sets a value that determines whether elements should be generated for the currently visible items.
		/// </summary>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		protected bool ShouldGenerateElements
		{
			get { return this._shouldGenerateElements; }
			set { this._shouldGenerateElements = value; }
		}

				#endregion //ShouldGenerateElements	

				#region TotalItemCount

		/// <summary>
		/// Returns the total number of items in the associated list if the panel is an Items Host, otherwise returns the total number of children.
		/// </summary>
		protected int TotalItemCount
		{
			get { return (base.IsItemsHost ? this.ItemsControl.Items.Count : base.InternalChildren.Count); }
		}

				#endregion //TotalItemCount
    
			#endregion //Protected Properties

			#region Private Properties

				#region ArcPath

		private static PathGeometry ArcPath
		{
			get
			{
				PathGeometry	pg		= new PathGeometry();
				Rect			rect	= new Rect(0, 0, 1, 1);
				PathFigure		pf		= new PathFigure();

				pf.StartPoint = rect.Location;
				pf.Segments.Add(new ArcSegment(rect.TopRight, new Size(rect.Width / 2, rect.Height), 180, false, SweepDirection.Counterclockwise, false));
				pg.Figures.Add(pf);

				return pg;
			}
		}

				#endregion //ArcPath
    
				#region CarouselPanelNavigator

		private CarouselPanelNavigator CarouselPanelNavigator
		{
			get
			{
				if (this._carouselPanelNavigator == null)
				{
					this._carouselPanelNavigator = new CarouselPanelNavigator(this);

                    if (LogicalTreeHelper.GetParent(this._carouselPanelNavigator) == null)
                        this.AddLogicalChild(this._carouselPanelNavigator);

					this.BindNavigatorVisibility();
				}

				return this._carouselPanelNavigator;
			}
		}

				#endregion //CarouselPanelNavigator	

				#region DefaultScalingEffectStops

		private ScalingEffectStopCollection DefaultScalingEffectStops
		{
			get
			{
				if (this._defaultScalingEffectStops == null)
				{
					this._defaultScalingEffectStops = new ScalingEffectStopCollection();

					this._defaultScalingEffectStops.Add(new ScalingEffectStop(0.0, 1.0));
					this._defaultScalingEffectStops.Add(new ScalingEffectStop(1.0, 1.0));
				}

				return this._defaultScalingEffectStops;
			}
		}

				#endregion //DefaultScalingEffectStops	

				#region DefaultSkewAngleXEffectStops

		private SkewAngleXEffectStopCollection DefaultSkewAngleXEffectStops
		{
			get
			{
				if (this._defaultSkewAngleXEffectStops == null)
				{
					this._defaultSkewAngleXEffectStops = new SkewAngleXEffectStopCollection();

					this._defaultSkewAngleXEffectStops.Add(new SkewAngleXEffectStop(0.0, 0.0));
					this._defaultSkewAngleXEffectStops.Add(new SkewAngleXEffectStop(1.0, 0.0));
				}

				return this._defaultSkewAngleXEffectStops;
			}
		}

				#endregion //DefaultSkewAngleXEffectStops	

				#region DefaultSkewAngleYEffectStops

		private SkewAngleYEffectStopCollection DefaultSkewAngleYEffectStops
		{
			get
			{
				if (this._defaultSkewAngleYEffectStops == null)
				{
					this._defaultSkewAngleYEffectStops = new SkewAngleYEffectStopCollection();

					this._defaultSkewAngleYEffectStops.Add(new SkewAngleYEffectStop(0.0, 0.0));
					this._defaultSkewAngleYEffectStops.Add(new SkewAngleYEffectStop(1.0, 0.0));
				}

				return this._defaultSkewAngleYEffectStops;
			}
		}

				#endregion //DefaultSkewAngleYEffectStops	

				#region DefaultOpacityEffectStops

		private OpacityEffectStopCollection DefaultOpacityEffectStops
		{
			get
			{
				if (this._defaultOpacityEffectStops == null)
				{
					this._defaultOpacityEffectStops = new OpacityEffectStopCollection();

					this._defaultOpacityEffectStops.Add(new OpacityEffectStop(0.0, 1.0));
					this._defaultOpacityEffectStops.Add(new OpacityEffectStop(1.0, 1.0));
				}

				return this._defaultOpacityEffectStops;
			}
		}

				#endregion //DefaultOpacityEffectStops	

				#region DefaultZOrderEffectStops

		private ZOrderEffectStopCollection DefaultZOrderEffectStops
		{
			get
			{
				if (this._defaultZOrderEffectStops == null)
				{
					this._defaultZOrderEffectStops = new ZOrderEffectStopCollection();

					this._defaultZOrderEffectStops.Add(new ZOrderEffectStop(0.0, 0.0));
					this._defaultZOrderEffectStops.Add(new ZOrderEffectStop(1.0, 1.0));
				}

				return this._defaultZOrderEffectStops;
			}
		}

				#endregion //DefaultZOrderEffectStops	
    
				#region DiagonalPath

		private static PathGeometry DiagonalPath
		{
			get
			{
				PathGeometry	pg		= new PathGeometry();
				Rect			rect	= new Rect(0, 0, 1, 1);

				Point		startPoint	= new Point(0, 0);
				Point		endPoint	= new Point(1, 1);
				PathFigure	pf			= new PathFigure();

				pf.StartPoint			= startPoint;
				pf.Segments.Add(new LineSegment(endPoint, false));
				pg.Figures.Add(pf);

				return pg;
			}
		}

				#endregion //DiagonalPath
    
				#region EllipsePath

		private static PathGeometry EllipsePath
		{
			get
			{
				// Use an ArcSegment instead of an EllipseGeometry so that we can specify the start point.
				PathGeometry	pg		= new PathGeometry();
				Rect			rect	= new Rect(0, 0, 1, 1);
				PathFigure		pf		= new PathFigure();

				pf.StartPoint = new Point(.5, .0);
				pf.Segments.Add(new ArcSegment(new Point(.50001, .0), new Size(rect.Width / 2, rect.Height / 2), 360, true, SweepDirection.Counterclockwise, true));
				pg.Figures.Add(pf);

				return pg;
			}
		}

				#endregion //EllipsePath

				#region HighestPossibleScrollOffset

		private double HighestPossibleScrollOffset
		{
			get { return this.ScrollingData._extent.Height - 1; }
		}

				#endregion //HighestPossibleScrollOffset	
    
				#region IsEntireListVisible

		private bool IsEntireListVisible
		{
			get { return this.TotalItemCount <= this.ItemsPerPageResolved; }
		}

				#endregion //IsEntireListVisible	

				#region ItemPathGeometry

		private PathGeometry ItemPathGeometry
		{
			get
			{
				if (this._itemPathGeometry == null)
				{
					Shape itemPath	= this.ViewSettingsInternal.ItemPath;

					// If the ItemPath is a Path shape, use its DataProperty to get the geometry.
					if (itemPath is Path)
					{
						Path path = itemPath as Path;
						if (path.Data			!= null && 
							path.Data.IsEmpty() == false)
							this._itemPathGeometry = PathGeometry.CreateFromGeometry(path.Data);
					}


					// If we were not able to get a geometry above and the ItemPath contains a RenderedGeometry
					// get the geometry from there.
					if (this._itemPathGeometry		== null	&&
						itemPath					!= null	&&
						itemPath.RenderedGeometry	!= null	&& 
						itemPath.RenderedGeometry.IsEmpty() == false)
						this._itemPathGeometry = PathGeometry.CreateFromGeometry(itemPath.RenderedGeometry);


					// If we still don't have a geometry, use a default.
					if (this._itemPathGeometry == null)
						this._itemPathGeometry = XamCarouselPanel.EllipsePath.Clone();
				}

				return this._itemPathGeometry;
			}
		}

				#endregion //ItemPathGeometry	
    
				#region LastVisibleItemIndex

		private int LastVisibleItemIndex
		{
			get { return this.GetLastVisibleItemIndex(this.FirstVisibleItemIndex); }
		}

				#endregion //LastVisibleItemIndex	
    
				#region LowestPossibleScrollOffset

		private double LowestPossibleScrollOffset
		{
			get { return 0 - (this.ScrollingData._viewport.Height - 1); }
		}

				#endregion //LowestPossibleScrollOffset	
    
				#region PathFractionPointsCache

		// JM 01-10-08 BR29568
		//private Hashtable PathFractionPointsCache
		private Dictionary<double, PathFractionPoint> PathFractionPointsCache
		{
			get
			{
				if (this._pathFractionPointsCache == null)
					// JM 01-10-08 BR29568
					//this._pathFractionPointsCache = new Hashtable(10);
					this._pathFractionPointsCache = new Dictionary<double, PathFractionPoint>(10);

				return this._pathFractionPointsCache;
			}
		}

				#endregion //PathFractionPointsCache

				#region RectPath

		private static PathGeometry RectPath
		{
			get
			{
				PathGeometry	pg		= new PathGeometry();
				Rect			rect	= new Rect(0, 0, 1, 1);

				pg.AddGeometry(new RectangleGeometry(rect));

				return pg;
			}
		}

				#endregion //RectPath

				// [JM 04-23-07]
				#region ViewSettingsVersion

		internal static readonly DependencyProperty ViewSettingsVersionProperty = DependencyProperty.Register("ViewSettingsVersion",
			typeof(int), typeof(XamCarouselPanel), new FrameworkPropertyMetadata(0));

				#endregion //ViewSettingsVersion

				#region VisiblePositionPercentLocations

		private double[] VisiblePositionPercentLocations
		{
			get
			{
				if (this._visiblePositionPercentValues.Length != this.ItemsPerPageResolved)
					this._visiblePositionPercentValuesDirty = true;

				this.RecalculateVisiblePositionPercentValues();

				return this._visiblePositionPercentValues;
			}
		}

				#endregion //VisiblePositionPercentLocations	
    
				#region WrapperSize

		private Size WrapperSize
		{
			get
			{
				if (this._wrapperSizeDirty)
				{
					CarouselViewSettings viewSettings = this.ViewSettingsInternal;

					this._wrapperSize		= new Size(viewSettings.ItemSize.Width, viewSettings.ReserveSpaceForReflections ? viewSettings.ItemSize.Height * 2 : viewSettings.ItemSize.Height);
					this._wrapperSizeDirty	= false;
				}

				return this._wrapperSize;
			}
		}

				#endregion //WrapperSize

			#endregion //Private Properties
    
		#endregion //Properties

		#region Methods

			#region Public Methods

				#region EnsureItemIsVisible

		/// <summary>
		/// Ensures that the item represented by the specified index is visible.  If it is the item is
		/// scrolled into view.
		/// </summary>
		/// <param name="itemIndex"></param>
		/// <returns>True if a scroll was required false if a scroll was not required.</returns>
		public bool EnsureItemIsVisible(int itemIndex)
		{
			if (itemIndex < 0 || itemIndex > (this.TotalItemCount - 1))
				throw new ArgumentException(SR.GetString("LE_ArgumentException_10"));

			if (this.GetIsItemIndexVisible(itemIndex, this.FirstVisibleItemIndex))
				return false;

			if (itemIndex < this.FirstVisibleItemIndex)
			{
				this.SetVerticalOffset(itemIndex);
				return true;
			}

			this.SetVerticalOffset(this.FirstVisibleItemIndexFromLastVisibleItemIndex(itemIndex));
			return true;
		}

				#endregion //EnsureItemIsVisible	
 
				#region ExecuteCommand
		/// <summary>
		/// Executes the RoutedCommand represented by the specified CommandWrapper.
		/// </summary>
		/// <param name="commandWrapper">The CommandWrapper that contains the RoutedCommand to execute</param>
		/// <returns>True if command was executed, false if canceled.</returns>
		/// <seealso cref="XamCarouselPanelCommands"/>
		public bool ExecuteCommand(CommandWrapper commandWrapper)
		{
			if (commandWrapper == null)
				throw new ArgumentNullException("commandWrapper");

			return this.ExecuteCommand(commandWrapper.Command);
		}

		/// <summary>
		/// Executes the specified RoutedCommand.
		/// </summary>
		/// <param name="command">The RoutedCommand to execute.</param>
		/// <returns>True if command was executed, false if canceled.</returns>
		/// <seealso cref="XamCarouselPanelCommands"/>
		public bool ExecuteCommand(RoutedCommand command)
		{
			if (false == XamCarouselPanelCommands.IsMinimumStatePresentForCommand(this, command))
				return false;

			// Fire the 'before executed' cancelable event.
			ExecutingCommandEventArgs beforeArgs = new ExecutingCommandEventArgs(command);
			bool proceed = this.RaiseExecutingCommand(beforeArgs);

			if (proceed == false)
            {
                // JJD 06/02/10 - TFS33112
                // Return the inverse of ContinueKeyRouting so that the developer can prevent
                // the original key message from bubbling
                //return false;
                return !beforeArgs.ContinueKeyRouting;
            }

			bool handled = false;

			if (command == XamCarouselPanelCommands.NavigateToNextItem)
			{
				this.LineRight();
				handled = true;
			}
			else if (command == XamCarouselPanelCommands.NavigateToPreviousItem)
			{
				this.LineLeft();
				handled = true;
			}
			else if (command == XamCarouselPanelCommands.NavigateToNextPage)
			{
				this.PageDown();
				handled = true;
			}
			else if (command == XamCarouselPanelCommands.NavigateToPreviousPage)
			{
				this.PageUp();
				handled = true;
			}

			// If the command was executed, fire the 'after executed' event.
			if (handled == true)
				this.RaiseExecutedCommand(new ExecutedCommandEventArgs(command));

			return handled;
		} 
				#endregion //ExecuteCommand

			#endregion //Public Methods

			#region Private Methods

				#region ApplyEffectsToItem

		private void ApplyEffectsToItem(CarouselPanelItem item, double currentPct, Point itemTangentPoint)
		{
			if (item == null)
				return;


			// Scale the item and adjust it's position so it is centered on the ItemPath
			Matrix					m				= Matrix.Identity;
			double					scalingFactor	= 1;
			Rect					sceneRect		= new Rect(new Point(0, 0), this._previousArrangeSize);
			CarouselViewSettings	viewSettings	= this.ViewSettingsInternal;
			Point					itemLocation	= item.CurrentLocation;


			// Apply scaling effects if specified.
			if (viewSettings.UseScaling)
			{
				EffectStopDirection scalingEffectStopDirectionResolved = this.ScalingEffectStopDirectionResolved;
				if (scalingEffectStopDirectionResolved == EffectStopDirection.Vertical)
					scalingFactor = this.ScalingEffectStopsResolved.GetStopValueFromRange(sceneRect.Top, sceneRect.Bottom, itemLocation.Y);
				else
				if (scalingEffectStopDirectionResolved == EffectStopDirection.Horizontal)
					scalingFactor = this.ScalingEffectStopsResolved.GetStopValueFromRange(sceneRect.Left, sceneRect.Right, itemLocation.X);
				else
					scalingFactor = this.ScalingEffectStopsResolved.GetStopValueFromOffset(currentPct);

				m.M11 = m.M22	= scalingFactor;
			}


			// Apply Opacity effects if specified.
			double elementOpacity = 1;
			if (viewSettings.UseOpacity)
			{
				EffectStopDirection opacityEffectStopDirectionResolved = this.OpacityEffectStopDirectionResolved;
				if (opacityEffectStopDirectionResolved == EffectStopDirection.Vertical)
					elementOpacity = this.OpacityEffectStopsResolved.GetStopValueFromRange(sceneRect.Top, sceneRect.Bottom, itemLocation.Y);
				else
				if (opacityEffectStopDirectionResolved == EffectStopDirection.Horizontal)
					elementOpacity = this.OpacityEffectStopsResolved.GetStopValueFromRange(sceneRect.Left, sceneRect.Right, itemLocation.X);
				else
					elementOpacity = this.OpacityEffectStopsResolved.GetStopValueFromOffset(currentPct);
			}


			// [JM BR21683 4-20-06] Move this code below after we set the matrix's offset 
//			// Rotate the item with the Path tangent if necessary.
//			if (viewSettings.RotateItemsWithPathTangent)
				//m.Rotate(XamCarouselPanel.CalculateRotationFromTangentPoint(itemTangentPoint));


			// Compute the item's fade factor.  Move the element offscene if its transition factor is 0.
			double	transitionFactor = 1;
			bool	inPathPrefixArea = false;
			bool	inPathSuffixArea = false;

			if (inPathPrefixArea = this.InPathPrefixArea(currentPct, viewSettings))
				transitionFactor = this.GetPrefixAreaPenetrationPercent(currentPct, viewSettings);
			else if (inPathSuffixArea = this.InPathSuffixArea(currentPct, viewSettings))
				transitionFactor = this.GetSuffixAreaPenetrationPercent(currentPct, viewSettings);

			if (transitionFactor == 0)
			{
				m.OffsetX = -10000;
				m.OffsetY = -10000;
			}
			else
			{
				m.OffsetX = itemLocation.X;
				m.OffsetY = itemLocation.Y;
			}


			// Offset the item by half its width and height so that it is centered on the path.
			Size itemSize	= viewSettings.ItemSize;
			m.OffsetX		= m.OffsetX - ((itemSize.Width * scalingFactor) / 2);
			m.OffsetY		= m.OffsetY - ((itemSize.Height * scalingFactor) / 2);


			// [JM BR21683 4-20-06] Call 'RotateAt' instead of 'Rotate' so that the item is rotated about its center.
			// Rotate the item with the Path tangent if necessary.
			if (viewSettings.RotateItemsWithPathTangent)
				//m.Rotate(XamCarouselPanel.CalculateRotationFromTangentPoint(itemTangentPoint));
				m.RotateAt(XamCarouselPanel.CalculateRotationFromTangentPoint(itemTangentPoint), itemLocation.X, itemLocation.Y);


			// Adjust Opacity
			if ((viewSettings.ItemTransitionStyle & PathItemTransitionStyle.AdjustOpacity) != 0  && (inPathPrefixArea || inPathSuffixArea))
			{
			    if (item.Opacity != (elementOpacity * transitionFactor))
			        item.Opacity = elementOpacity * transitionFactor;
			}
			else
			if (item.Opacity != elementOpacity)
				item.Opacity = elementOpacity;



			// Adjust Size
			if ((viewSettings.ItemTransitionStyle & PathItemTransitionStyle.AdjustSize) != 0 && (inPathPrefixArea || inPathSuffixArea))
				m.M11 = m.M22 = m.M11 * transitionFactor;


			// Calculate skewing effect values.
			double skewAngleX = 0;
			double skewAngleY = 0;
			if (viewSettings.UseSkewing)
			{
				if (this.SkewAngleXEffectStopDirectionResolved == EffectStopDirection.Vertical)
					skewAngleX = this.SkewAngleXEffectStopsResolved.GetStopValueFromRange(sceneRect.Top, sceneRect.Bottom, itemLocation.Y);
				else
				if (this.SkewAngleXEffectStopDirectionResolved == EffectStopDirection.Horizontal)
					skewAngleX = this.SkewAngleXEffectStopsResolved.GetStopValueFromRange(sceneRect.Left, sceneRect.Right, itemLocation.X);
				else
					skewAngleX = this.SkewAngleXEffectStopsResolved.GetStopValueFromOffset(currentPct);

				if (this.SkewAngleYEffectStopDirectionResolved == EffectStopDirection.Vertical)
					skewAngleY = this.SkewAngleYEffectStopsResolved.GetStopValueFromRange(sceneRect.Top, sceneRect.Bottom, itemLocation.Y);
				else
				if (this.SkewAngleYEffectStopDirectionResolved == EffectStopDirection.Horizontal)
					skewAngleY = this.SkewAngleYEffectStopsResolved.GetStopValueFromRange(sceneRect.Left, sceneRect.Right, itemLocation.X);
				else
					skewAngleY = this.SkewAngleYEffectStopsResolved.GetStopValueFromOffset(currentPct);

			}

			// Commented this out - can't seem to get skewing working properly using the MatrixTransform.
			// The items are not being skewed around the center point of the item when using the
			// MatrixTransform (which results in incorrect positioning of the items but are when using the
			// SkewTransform.  So we'll use the separate SkewTransform for now - hopefully we can get it working
			// at some point using the MatrixTransform so we are applying just a single transform.
			//
			//double centerX = (this.ItemSize.Width * transitionFactor) / 2;
			//double centerY = (this.ItemSize.Height * transitionFactor) / 2;
			//m.Translate(-centerX, -centerY);
			//m.Skew(skewAngleX, skewAngleY);
			//m.Translate(centerX, centerY);


			// Update the transforms on the item.
			item.UpdateTransforms(m, skewAngleX, skewAngleY, itemSize.Width / 2, itemSize.Height / 2);
		}

				#endregion //ApplyEffectsToItem

				#region BindNavigatorVisibility

		private void BindNavigatorVisibility()
		{
			if (this._carouselPanelNavigator != null)
			{
				Binding binding = new Binding();
				binding.Mode = BindingMode.OneWay;
				binding.Source = this.ViewSettingsInternal;
				binding.Path = new PropertyPath(CarouselViewSettings.IsNavigatorVisibleProperty);
				binding.Converter = new BooleanToVisibilityConverter();

				this._carouselPanelNavigator.SetBinding(CarouselPanelNavigator.VisibilityProperty, binding);
			}
		}

				#endregion //BindNavigatorVisibility	

				// [JM 04-23-07]
				#region BumpCarouselViewSettingsVersion

		private void BumpCarouselViewSettingsVersion()
		{
			this.SetValue(XamCarouselPanel.ViewSettingsVersionProperty, (int)this.GetValue(XamCarouselPanel.ViewSettingsVersionProperty) + 1);
		}

				#endregion //BumpCarouselViewSettingsVersion

				#region CalculateChildTargetPct

		private double CalculateChildTargetPct(int childIndex, int childsItemIndex, int scrollOffset, bool itemVisibleBeforeScroll, bool itemVisibleAfterScroll)
		{
			// Determine the child's target percent based on the child's Item Index and the current scroll offset.
			// A child that falls before the first visible item will be assigned a percent of zero and a child that
			// falls after the last visible item will be assigned a percent of 100 (i.e., 1.0).  Visible children will
			// be assigned a percentage between the prefix percent and the suffix percent.  The prefix and suffix areas
			// are used to fade items in and out.
			double					pct				= 0;
			CarouselViewSettings	viewSettings	= this.ViewSettingsInternal;


			if (this.GetShouldPositionItemAtBeginningOfPath(childsItemIndex, scrollOffset, itemVisibleBeforeScroll, itemVisibleAfterScroll) == true)
				pct = 0;
			else 
			if (this.GetShouldPositionItemAtEndOfPath(childsItemIndex, scrollOffset, itemVisibleBeforeScroll, itemVisibleAfterScroll) == true)
				pct = 1;
			else
			{
				// Get the visible position of the child based on its item index.  -1 indicates that the item is not visible
				int visiblePosition = this.GetVisiblePositionFromItemIndex(childsItemIndex, scrollOffset);

				if (visiblePosition < 0)
				{
					// [JM 05-17-07] Use GetChildElement instead of accessing the child elements collection directly.
					//CarouselPanelItem child = this.Children[childIndex] as CarouselPanelItem;
					// JJD 6/6/07
					// Use Children collection to only access active children
					//CarouselPanelItem child = this.GetChildElement(childIndex) as CarouselPanelItem;
					// AS 7/9/07
					//CarouselPanelItem child = this.Children[childIndex] as CarouselPanelItem;
					CarouselPanelItem child = this.ChildElements[childIndex] as CarouselPanelItem;

					// JM 05-07-08 - BR30992 - Check for null Child.
					if (child == null)
						pct = 1;
					else if (child.PathLocationPercent == 0)
						pct = 0;
					else if (child.PathLocationPercent == 1)
						pct = 1;
					else if (child.PathLocationPercent < (1 - child.PathLocationPercent))
						pct = 0;
					else
						pct = 1;
				}
				else if (visiblePosition == 0)
					pct = viewSettings.ItemPathPrefixPercent;
				else if (visiblePosition == (this.ItemsPerPageResolved - 1))
					pct = 1 - viewSettings.ItemPathSuffixPercent;
				else
					pct = this.VisiblePositionPercentLocations[visiblePosition];
			}

			return pct;
		}

				#endregion //CalculateChildTargetPct

				#region CalculateRotationFromTangentPoint

		private static double CalculateRotationFromTangentPoint(Point tangentPoint)
		{
			double angle = Math.Acos(tangentPoint.X) * 57.295779513082323;

			if (tangentPoint.Y < 0)
				angle = 360 - angle;

			return angle;
		}

				#endregion //CalculateRotationFromTangentPoint
    
				#region CompositionTarget_Rendering

		private void CompositionTarget_Rendering(object sender, EventArgs e)
		{
			if (this.IsInitialized				== false		||
				this._previousMeasureSize		== Size.Empty	||
			   (this._previousMeasureSize.Width == 0 && this._previousMeasureSize.Height == 0))
				return;

			//int		tier						= System.Windows.Media.RenderCapability.Tier; 

			// Compute the 'smoothed' frame rate.  (don't allow the smoothed framerate to fluctuate more than FRAME_RATE_SMOOTHING_FACTOR percent) 
			long	nowTick						= DateTime.Now.Ticks;
			long	ticksSinceLastFrameRender	= nowTick - this._previousTick;
			double	secondsSinceLastFrameRender	= XamCarouselPanel.SecondsFromTicks(Math.Min(400000, ticksSinceLastFrameRender));
			this._previousTick					= nowTick;

			double latestFrameRate				= 1 / secondsSinceLastFrameRender;
			if (latestFrameRate > this._smoothedFrameRate)
			{
				if ((latestFrameRate - this._smoothedFrameRate) / this._smoothedFrameRate > FRAME_RATE_SMOOTHING_FACTOR)
					this._smoothedFrameRate *= (1 + FRAME_RATE_SMOOTHING_FACTOR);
				else
					this._smoothedFrameRate = latestFrameRate;
			}
			else
			{
				if ((this._smoothedFrameRate - latestFrameRate) / this._smoothedFrameRate > FRAME_RATE_SMOOTHING_FACTOR)
					this._smoothedFrameRate *= (1 - FRAME_RATE_SMOOTHING_FACTOR);
				else
					this._smoothedFrameRate = latestFrameRate;
			}



			// Setup some variables
			bool	elementPositionChanged		= false;
			bool	scrollInProgress			= this.ScrollInProgress;
			bool	scrollingHigher				= this._scrollActionInfo.ScrollingHigher;


			// Cache some values.
			// [JM 05-17-07] Change looping logic to use GetChildElement instead of iterating through the child elements collection.
			//UIElementCollection		children			= this.Children;
			// JJD 6/6/07
			// Use Children collection to only access active children
			//int childrenCount = this.VisualChildrenCount; // children.Count;
			// AS 7/9/07
			//IList		children			= this.Children;
			IList		children			= this.ChildElements;
			int			childrenCount		= children.Count;
			CarouselViewSettings	viewSettings		= this.ViewSettingsInternal;
			double					adjustedAttraction	= XamCarouselPanel.ATTRACTION * (XamCarouselPanel.REFERENCE_FRAME_RATE / this._smoothedFrameRate);

//Debug.WriteLine("FrameRate: " + this._smoothedFrameRate.ToString() + ", AttractionFactor: " + adjustedAttraction.ToString());

			for (int i = 0; i < childrenCount; i++)
			{
				//if (XamCarouselPanel.UpdateElementPosition(children[i] as CarouselPanelItem, secondsSinceLastFrameRender, XamCarouselPanel.DAMPENING, adjustedAttraction, scrollInProgress, scrollingHigher))
				// JJD 6/6/07
				// Use Children collection to only access active children
				//if (XamCarouselPanel.UpdateElementPosition(this.GetChildElement(i) as CarouselPanelItem, secondsSinceLastFrameRender, XamCarouselPanel.DAMPENING, adjustedAttraction, scrollInProgress, scrollingHigher))

				// JM 05-07-08 - BR30992 - Make sure child is a CarouselPanelItem.
				if (children[i] is CarouselPanelItem == false)
					continue;

				if (XamCarouselPanel.UpdateElementPosition(children[i] as CarouselPanelItem, secondsSinceLastFrameRender, XamCarouselPanel.DAMPENING, adjustedAttraction, scrollInProgress, scrollingHigher))
					elementPositionChanged = true;

//XmlElement xml = ((FrameworkElement)this.GetChildElement(i)).DataContext as XmlElement;
//string name = "";
//if (xml != null)
//{
//    name = xml.Attributes["FirstName"].InnerText;
//}
//Debug.WriteLine("Child #" + i.ToString() + "  Name: " + name + " currentPct: " + ((CarouselPanelItem)this.GetChildElement(i)).PathLocationPercent.ToString() + ", targetPct: " + ((CarouselPanelItem)this.GetChildElement(i)).PathTargetPercent.ToString() + ", velocity: " + ((CarouselPanelItem)this.GetChildElement(i)).Velocity.ToString() + ", Visibility = " + ((FrameworkElement)this.GetChildElement(i)).Visibility.ToString());
			}


			// If no element positions were changed during this frame render, stop the active scroll if any.
			if (elementPositionChanged == false && this.ScrollInProgress)
				this.StopAnimatedScrollProcessing();
			else
			if (elementPositionChanged == true)
			{
				this.DoArrangeProcessing(this._previousArrangeSize, false);
			}
		}

				#endregion //CompositionTarget_Rendering

				#region DoArrangeProcessing

		private void DoArrangeProcessing(Size finalSize, bool forceArrangeOfAllItems)
		{
			this._previousArrangeSize				= finalSize;
			Size					wrapperSize		= this.WrapperSize;
			CarouselViewSettings	viewSettings	= this.ViewSettingsInternal;


			// Generate Elements if we are an ItemsHost and elements need to be generated.
			int totalItemsGenerated = 0;

			int totalItemCount		= this.TotalItemCount;
			int itemsPerPage		= this.ItemsPerPageResolved;
			if (this.IsItemsHost && this.ShouldGenerateElements)
			{
				totalItemsGenerated = this.GenerateElementsHelper((int)this.ScrollingData._offset.Y, itemsPerPage, null, 0);
				if (totalItemsGenerated > 0 && totalItemCount > 0)
					this.ShouldGenerateElements = false;
			}


			// JM BR25655 09-26-07
			if (this.ScrollInProgress == false)
				this.RecalculateCurrentItemLocations();


			// Arrange our child elements.
			// [JM 05-17-07] Change looping logic to use GetChildElement instead of iterating through the child elements collection.
			bool				shouldAnimateInitialArrange = (viewSettings.ShouldScrollItemsIntoInitialPosition == false) ? false : ((this.IsItemsHost == true && totalItemsGenerated > 0) || this.IsItemsHost == false);
			//UIElementCollection children					= this.Children;
			// JJD 6/6/07
			// Use Children collection to only access active children
			//int childrenCount = this.VisualChildrenCount; //children.Count;
			// AS 7/9/07
			//IList				children					= this.Children;
			IList				children					= this.ChildElements;
			int					childrenCount				= children.Count;

			// [JM 06-07-07]
			if (childrenCount < 1)
				return;

			int					itemIndex					= base.GetItemIndexFromChildIndex(0);

			// JM 11-18-11 TFS93047 - If we are an ItemsHost make sure that any VisualChildren which are not in our ChildElements collection
			// (i.e., they were not generated above) have their opacity set to zero so they do not appear.
			if (this.IsItemsHost)
			{
				int visualChildrenCount = VisualTreeHelper.GetChildrenCount(this);
				for (int i = 0; i < visualChildrenCount; i++)
				{
					UIElement uie = VisualTreeHelper.GetChild(this, i) as UIElement;
					if (uie != null && false == children.Contains(uie))
						uie.Opacity = 0;
				}
			}

			for (int i = 0; i < childrenCount; i++, itemIndex++)
			{
				//CarouselPanelItem child = children[i] as CarouselPanelItem;
				// JJD 6/6/07
				// Use Children collection to only access active children
				//CarouselPanelItem child = this.GetChildElement(i) as CarouselPanelItem;
				CarouselPanelItem child = children[i] as CarouselPanelItem;
				if (child == null)
				{
					// JM 05-07-08 - BR30992
					//continue;
					throw new NotSupportedException(SR.GetString("LE_NotSupportedException_12"));
				}

				// Get the child's CURRENT location as a percentage along the item path.
				if (shouldAnimateInitialArrange == true && this._initialArrangeAnimationDone == false)
					child.PathLocationPercent = 0;
				else
				if (child.IsPathLocationUnset)
				{
					int visibleIndex = i - this.FirstVisibleChildIndex;
					if (visibleIndex >= 0 && visibleIndex < itemsPerPage)
						child.PathLocationPercent = this.VisiblePositionPercentLocations[visibleIndex];
					else
						child.PathLocationPercent = 0;
				}

				double currentLocationPercent	= child.PathLocationPercent;

				// Get the point that corresponds to the current location percent.  Check our cache first
				// to try and avoid calling GetPointAtFractionLength which is somewhat costly.
				Point currentLocationPoint	= new Point(0, 0);
				Point tangent				= new Point(0, 0);
				try
				{
					PathFractionPoint pfp;

					// JM 01-10-08 - Optimization to reduce the size of the cache.
					//if (this.PathFractionPointsCache.ContainsKey(currentLocationPercent))
					double clp = Math.Round(currentLocationPercent, 4);	// only use 4 digits of precision in the percent to reduce the total possible number of values and keep the size of the cache down.
					if (this.PathFractionPointsCache.ContainsKey(clp))
					{
						// JM 01-10-08 - Optimization to reduce the size of the cache.
						//pfp						= (PathFractionPoint)this.PathFractionPointsCache[currentLocationPercent];
						pfp						= (PathFractionPoint)this.PathFractionPointsCache[clp];
						currentLocationPoint	= pfp.Point;
						tangent					= pfp.TangentPoint;
					}
					else
					{
						// JM 01-10-08 - Optimization to reduce the size of the cache.
						//this.ItemPathGeometry.GetPointAtFractionLength(currentLocationPercent, out currentLocationPoint, out tangent);
						this.ItemPathGeometry.GetPointAtFractionLength(clp, out currentLocationPoint, out tangent);

						pfp = new PathFractionPoint(currentLocationPoint, tangent);
						// JM 01-10-08 - Optimization to reduce the size of the cache.
						//this.PathFractionPointsCache.Add(currentLocationPercent, pfp);
						this.PathFractionPointsCache.Add(clp, pfp);
					}
				}
				catch (Exception)
				{ currentLocationPoint = child.CurrentLocation; }


				// [JM BR24885 07-18-07] Don't rely on the ARrange alone to size the item and its descendants.  Explicitly set the width/height
				// and re-measure if necessary.
				child.Width		= wrapperSize.Width;
				child.Height	= wrapperSize.Height;
				if (child.IsMeasureValid == false)
					child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));


				if (child.NeedsArrange || child.IsArrangeValid == false)
				{
					child.Arrange(new Rect(new Point(0, 0), wrapperSize));
					child.NeedsArrange = false;
				}

				XamCarouselPanel.UpdateFirstAndLastItemPropertiesOnChild(child, itemIndex, totalItemCount);


				child.CurrentLocation = currentLocationPoint;


				// Apply effects (if any).
				this.ApplyEffectsToItem(child, currentLocationPercent, tangent);
			}


			// If necessary, setup for the initial scroll animation by invoking the animation asynchronously.
			if (shouldAnimateInitialArrange			== true		&&
				this._initialArrangeAnimationDone	== false	&&
				childrenCount > 0)
			{
				// Set the scrolling offset equal to the number of elements we have.  We will reset the scrolling offset in 
				// PerformInitialArrangeAnimation to trigger the scroll
				// [JM 06-14-07]
				//double offset				= this.ScrollOffsetForInitialArrangeAnimation + (double)childrenCount;
				// [JM 08-13-07 BR25648 ]
				//double offset				= this.ScrollOffsetForInitialArrangeAnimation + Math.Min((double)childrenCount - 1, itemsPerPage - 1);
				double offset				= this.ScrollOffsetForInitialArrangeAnimation + Math.Max(Math.Min((double)childrenCount - 1, itemsPerPage - 1), 1);
				this.ScrollingData._offset = new Vector(offset, offset);


				// Reset our scrolling offset asynchronously to trigger the initial arrange animation.
				this.Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new MethodDelegate(PerformInitialArrangeAnimation));
			}

			this.SetZOrder();
		}

				#endregion //DoArrangeProcessing	
    
				#region FirstVisibleItemIndexFromLastVisibleItemIndex

		private int FirstVisibleItemIndexFromLastVisibleItemIndex(int lastVisibleItemIndex)
		{
			CarouselViewSettings viewSettings = this.ViewSettingsInternal;

			int firstVisibleItemIndexCandidate = lastVisibleItemIndex - this.ItemsPerPageResolved + 1;

			if (viewSettings.IsListContinuous)
			{
				if (firstVisibleItemIndexCandidate >= 0)
					return firstVisibleItemIndexCandidate;

				int diff = Math.Abs(firstVisibleItemIndexCandidate);

				return this.TotalItemCount - diff;
			}
			else
				return Math.Max(0, firstVisibleItemIndexCandidate);
		}

				#endregion //FirstVisibleItemIndexFromLastVisibleItemIndex	

				// [JM 05-17-07] Copied from old IGVirtualizingPanel class.
				#region GenerateElements



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		private int GenerateElements(int firstItemIndexToGenerate, int totalElementsToGenerate)
		{
			Debug.Assert(this.IsItemsHost == true, "GenerateElements called but IsItemsHost = false!");
			if (this.IsItemsHost == false)
				return 0;

			// Must access the internal children collection before using the generator (this is probably a bug in the framework).
			UIElementCollection		children					= base.InternalChildren;

			int						currentChildIndex			= 0;
			int						currentItemIndex			= firstItemIndexToGenerate;
			int						totalElementsGenerated		= 0;
			GeneratorPosition		generatorStartPosition		= this.GetGeneratorPositionFromItemIndex(firstItemIndexToGenerate, out currentChildIndex);

			// [JM 05-17-07]
			//IItemContainerGenerator generator					= this.IItemContainerGenerator;
			IItemContainerGenerator generator					= this.ActiveItemContainerGenerator;

			using (IDisposable disposable1 = generator.StartAt(generatorStartPosition, GeneratorDirection.Forward, true))
			{
				int totalItems = this.TotalItemCount;

				while (currentItemIndex < totalItems && totalElementsGenerated < totalElementsToGenerate)
				{
					if (currentItemIndex >= 0)
					{
						bool isNewlyRealized;
						UIElement generatedElement = generator.GenerateNext(out isNewlyRealized) as UIElement;
						if (generatedElement == null)
							break;

						totalElementsGenerated++;


						// If the generated item is 'newly realized', add it to our children collection
						// and 'prepare' it.
						if (isNewlyRealized)
						{
							int index = currentChildIndex;

							if (currentChildIndex >= base.InternalChildren.Count)
							{
								this.AddInternalChild(generatedElement);

								// [JM 05-17-07]
								//index = base.Children.Count - 1;
								index = this.VisualChildrenCount - 1;
							}
							else
								this.InsertInternalChild(currentChildIndex, generatedElement);

							generator.PrepareItemContainer(generatedElement);

							this.OnNewElementRealized(generatedElement, index);
						}
					}


					// Bump some counters.
					currentChildIndex++;
					currentItemIndex++;
				}
			}

			return totalElementsGenerated;
		}

				#endregion //GenerateElements
    
				#region GenerateElementsHelper



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

		private int GenerateElementsHelper(int firstItemIndex1, int totalElements1, Nullable<int> firstItemIndex2, int totalElements2)
		{
			ScrollDirection scrollDirection = ScrollDirection.Increment;
			if (this.ScrollInProgress)
				scrollDirection = this._scrollActionInfo.ScrollingHigher ? ScrollDirection.Increment : ScrollDirection.Decrement;


			// Start the generation.
			this.BeginGeneration(scrollDirection);


			// Generate the first set of elements.
//Debug.WriteLine("Generating group #1: first item index = " + firstItemIndex1.ToString() + ", total to generate = " + totalElements1.ToString() );
			int totalElementsGenerated = this.GenerateElementsHelperPrivate(firstItemIndex1, totalElements1);


			// Generate the second set of elements if necessary.
			if (firstItemIndex2.HasValue)
			{
//Debug.WriteLine("Generating group #2: first item index = " + firstItemIndex2.Value.ToString() + ", total to generate = " + totalElements2.ToString());
				totalElementsGenerated += this.GenerateElementsHelperPrivate(firstItemIndex2.Value, totalElements2);
			}


			// End the generation.
			this.EndGeneration();

			return totalElementsGenerated;
		}



#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

		private int GenerateElementsHelperPrivate(int firstItemIndexToGenerate, int totalElementsToGenerate)
		{
			// [JM 05-17-07]
			//if (this.IsListWrapping == false)
			//	return base.GenerateElements(firstItemIndexToGenerate, totalElementsToGenerate);
			if (this.GetIsListWrapping(firstItemIndexToGenerate) == false)				
				return this.GenerateElements(firstItemIndexToGenerate, totalElementsToGenerate);


			// Generate the first bunch of elements.
			int totalGenerated			= 0;
			int totalItemsInFirstBunch	= this.TotalItemCount - firstItemIndexToGenerate;

			// [JM 05-17-07]
			//totalGenerated				= base.GenerateElements(firstItemIndexToGenerate, totalItemsInFirstBunch);
			totalGenerated					= this.GenerateElements(firstItemIndexToGenerate, totalItemsInFirstBunch);


			// Generate the second bunch of elements.
			if (totalGenerated < totalElementsToGenerate)
				// [JM 05-17-07]
				//totalGenerated += base.GenerateElements(0, this.ItemsPerPageResolved - totalItemsInFirstBunch);
				totalGenerated += this.GenerateElements(0, this.ItemsPerPageResolved - totalItemsInFirstBunch);


			return totalGenerated;
		}

				#endregion //GenerateElementsHelper	

				// [JM 05-17-07] Copied from old IGVirtualizingPanel class.
				#region GetGeneratorPositionFromItemIndex



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		private GeneratorPosition GetGeneratorPositionFromItemIndex(int itemIndex, out int childIndex)
		{
			Debug.Assert(this.IsItemsHost == true, "GetGeneratorPositionFromItemIndex called but IsItemsHost == false");
			if (this.IsItemsHost != true)
			{
				childIndex = 0;
				return new GeneratorPosition(-1, itemIndex + 1);
			}

			// Must access the internal children collection before using the generator (this is probably a bug in the framework).
			UIElementCollection		children			= base.InternalChildren;

			// [JM 05-17-07]
			//IItemContainerGenerator generator = this.IItemContainerGenerator;
			IItemContainerGenerator generator = this.ActiveItemContainerGenerator;

			GeneratorPosition		generatorPosition = (generator != null) ? generator.GeneratorPositionFromIndex(itemIndex) :
																				new GeneratorPosition(-1, itemIndex + 1);

			childIndex = (generatorPosition.Offset == 0) ? generatorPosition.Index :
														   generatorPosition.Index + 1;

			return generatorPosition;
		}

				#endregion //GetGeneratorPositionFromItemIndex
    
				#region GetIsItemIndexInLastVisiblePosition

		private bool GetIsItemIndexInLastVisiblePosition(int itemIndex, double scrollOffset)
		{
			int visiblePosition = (int)scrollOffset < 0 ? itemIndex + Math.Abs((int)scrollOffset) : itemIndex - (int)scrollOffset;

			return visiblePosition == this.ItemsPerPageResolved - 1;
		}

				#endregion //GetIsItemIndexInLastVisiblePosition	
    
				#region GetIsListWrapping

		private bool GetIsListWrapping(int scrollOffset)
		{
			CarouselViewSettings viewSettings = this.ViewSettingsInternal;

			if (viewSettings.IsListContinuous == false)
				return false;

			int itemsPerPage	= this.ItemsPerPageResolved;

			// [JM 06-11-07]
			//if (this.TotalItemCount == itemsPerPage && scrollOffset != 0)
			//	  return true;
			if (this.TotalItemCount == itemsPerPage)
			{
				if (scrollOffset != 0 && scrollOffset % itemsPerPage != 0)
					return true;

				return false;
			}

			if (this.TotalItemCount < itemsPerPage)
				return false;

			// At this point we have a continuous list with more than 1 page of items
			return (this.TotalItemCount - scrollOffset) < itemsPerPage;
		}

				#endregion //GetIsListWrapping	
    
				#region GetLastVisibleItemIndex

		private int GetLastVisibleItemIndex(int scrollOffset)
		{
			return this.LastVisibleItemIndexFromFirstVisibleItemIndex(scrollOffset);
		}

				#endregion //GetLastVisibleItemIndex	

				#region GetPrefixAreaPenetrationPercent



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		private double GetPrefixAreaPenetrationPercent(double pathPositionPercentage, CarouselViewSettings viewSettings)
		{
			if (viewSettings.ItemPathPrefixPercent == 0)
				return 0;

			return pathPositionPercentage / viewSettings.ItemPathPrefixPercent;
		}

				#endregion //GetPrefixAreaPenetrationPercent	

				#region GetShouldPositionItemAtEndOfPath

		private bool GetShouldPositionItemAtEndOfPath(int itemIndex, int scrollOffset, bool itemVisibleBeforeScroll, bool itemVisibleAfterScroll)
		{
			CarouselViewSettings viewSettings = this.ViewSettingsInternal;

			if (viewSettings.IsListContinuous == false)
				return itemIndex >= (scrollOffset + this.ItemsPerPageResolved);

			if (this.GetIsItemIndexVisible(itemIndex, scrollOffset) == true)
				return false;

			if (this.ScrollInProgress)
				return this._scrollActionInfo.ScrollingHigher	== false	&&
						itemVisibleBeforeScroll					== true		&&
						itemVisibleAfterScroll					== false;
			else
				return itemIndex > this.GetLastVisibleItemIndex(scrollOffset);
		}

				#endregion //GetShouldPositionItemAtEndOfPath

				#region GetShouldPositionItemAtBeginningOfPath

		private bool GetShouldPositionItemAtBeginningOfPath(int itemIndex, int scrollOffset, bool itemVisibleBeforeScroll, bool itemVisibleAfterScroll)
		{
			if (this.ViewSettingsInternal.IsListContinuous == false)
				return itemIndex < scrollOffset;

			if (this.GetIsItemIndexVisible(itemIndex, scrollOffset) == true)
				return false;

			if (this.ScrollInProgress)
				return	this._scrollActionInfo.ScrollingHigher == true  &&
						itemVisibleBeforeScroll == true					&& 
						itemVisibleAfterScroll == false;
			else
				return itemIndex < scrollOffset;
		}

				#endregion //GetShouldPositionItemAtBeginningOfPath	

				#region GetSuffixAreaPenetrationPercent



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		private double GetSuffixAreaPenetrationPercent(double pathPositionPercentage, CarouselViewSettings viewSettings)
		{
			if (viewSettings.ItemPathSuffixPercent == 0)
				return 0;

			return (1 - pathPositionPercentage) / viewSettings.ItemPathSuffixPercent;
		}

				#endregion //GetSuffixAreaPenetrationPercent	
  
				#region GetVisiblePositionFromItemIndex



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		private int GetVisiblePositionFromItemIndex(int itemIndex, int scrollOffset)
		{
			CarouselViewSettings	viewSettings = this.ViewSettingsInternal;
			int						itemsPerPage = this.ItemsPerPageResolved;

			if (this.GetIsListWrapping(scrollOffset) == false)
			{
				int vp = itemIndex - scrollOffset;
				if (vp < 0 || vp >= itemsPerPage)
					return -1;
			}


			int totalItemsShowingAtEndOfList = this.TotalItemCount - scrollOffset;

			if (itemIndex >= scrollOffset &&
				itemIndex < scrollOffset + totalItemsShowingAtEndOfList)
				return itemIndex - scrollOffset;

			int visiblePositionCandidate = totalItemsShowingAtEndOfList + this.GetChildIndexFromItemIndex(itemIndex);
			if (visiblePositionCandidate >= itemsPerPage)
				return -1;

			return visiblePositionCandidate;
		}

				#endregion //GetVisiblePositionFromItemIndex

				#region InitializeAdorner

		private void InitializeAdorner()
		{
			if (this._carouselPanelAdorner != null)
				return;

			this._carouselPanelAdorner = new CarouselPanelAdorner(this);


			// Bind the adorner's Visibility property to our IsVisible property. [BR19499]
			Binding binding		= new Binding();
			binding.Mode		= BindingMode.OneWay;
			binding.Source		= this;
			binding.Path		= new PropertyPath("IsVisible");
			binding.Converter	= new BooleanToVisibilityConverter();
			this._carouselPanelAdorner.SetBinding(CarouselPanelAdorner.VisibilityProperty, binding);


			// Bind the adorner's IsEnabled property to our IsEnabled property. [BR20069]
			binding				= new Binding();
			binding.Mode		= BindingMode.OneWay;
			binding.Source		= this;
			binding.Path		= new PropertyPath("IsEnabled");
			this._carouselPanelAdorner.SetBinding(CarouselPanelAdorner.IsEnabledProperty, binding);


			// Bind the adorner's Opacity property to our Opacity property. [BR20070]
			binding				= new Binding();
			binding.Mode		= BindingMode.OneWay;
			binding.Source		= this;
			binding.Path		= new PropertyPath("Opacity");
			this._carouselPanelAdorner.SetBinding(CarouselPanelAdorner.OpacityProperty, binding);


			this.AddAdornerChildElement(this.CarouselPanelNavigator);

			AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(this);
			if (adornerLayer != null)
				adornerLayer.Add(this._carouselPanelAdorner);

            // AS 3/11/09 TFS11010
            // This isn't needed. In the lazy property creation we add it to the logical tree.
            //
            //// Add the Navigator as our logical child.
            //DependencyObject logicalParent = LogicalTreeHelper.GetParent(this.CarouselPanelNavigator);
            //if (logicalParent == null)
            //    this.AddLogicalChild(this.CarouselPanelNavigator);
            Debug.Assert(LogicalTreeHelper.GetParent(this.CarouselPanelNavigator) == this);
		}

				#endregion //InitializeAdorner	
    
				#region InPathPrefixArea

		private bool InPathPrefixArea(double pathPositionPercentage, CarouselViewSettings viewSettings)
		{
			return pathPositionPercentage < viewSettings.ItemPathPrefixPercent;
		}

				#endregion //InPathPrefixArea	
    
				#region InPathSuffixArea

		private bool InPathSuffixArea(double pathPositionPercentage, CarouselViewSettings viewSettings)
		{
			return pathPositionPercentage > (1 - viewSettings.ItemPathSuffixPercent);
		}

				#endregion //InPathSuffixArea	
    
				#region LastVisibleItemIndexFromFirstVisibleItemIndex

		private int LastVisibleItemIndexFromFirstVisibleItemIndex(int scrollOffset)
		{
			CarouselViewSettings viewSettings = this.ViewSettingsInternal;

			int lastVisibleItemIndexCandidate = scrollOffset + this.ItemsPerPageResolved - 1;

			if (viewSettings.IsListContinuous)
			{
				if (lastVisibleItemIndexCandidate < this.TotalItemCount)
					return lastVisibleItemIndexCandidate;

				return lastVisibleItemIndexCandidate - this.TotalItemCount - 1;
			}
			else
				return Math.Min(this.TotalItemCount, lastVisibleItemIndexCandidate);
		}

				#endregion //LastVisibleItemIndexFromFirstVisibleItemIndex	

				// JM 11-28-07 BR28764
				#region OnChildrenCollectionChanged

		private void OnChildrenCollectionChanged(object sender, EventArgs e)
		{
			if (this.ScrollInProgress)
				this.StopAnimatedScrollProcessing();
		}

				#endregion //OnChildrenCollectionChanged	

				// JM 05-06-08 - BR32342
				#region OnUnloaded

		void OnUnloaded(object sender, RoutedEventArgs e)
		{
			this.Unloaded	-= new RoutedEventHandler(OnUnloaded);
			this.Loaded		+= new RoutedEventHandler(OnLoaded);

			// JM 01-05-12 TFS96730
			this._wasLoaded = true;
			this.RemoveAdornerChildElement(this.CarouselPanelNavigator);
			AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(this);
			if (adornerLayer != null && this._carouselPanelAdorner != null)
				adornerLayer.Remove(this._carouselPanelAdorner);
		}

				#endregion //OnUnloaded	
    
				// JM 05-06-08 - BR32342
				#region OnLoaded

		void OnLoaded(object sender, RoutedEventArgs e)
		{
			this.Loaded		-= new RoutedEventHandler(OnLoaded);
			this.Unloaded	+= new RoutedEventHandler(OnUnloaded);

			this.ShouldGenerateElements = true;
			this.InvalidateMeasure();
			this.UpdateLayout();

			// JM 01-05-12 TFS96730
			if (this._wasLoaded)
			{
				this._wasLoaded = false;

				AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(this);
				if (adornerLayer != null)
				{
					Adorner [] adorners = adornerLayer.GetAdorners(this);
					if (adorners == null || (adorners.GetLength(0) > 0 && adorners[0] != this._carouselPanelAdorner))
						adornerLayer.Add(this._carouselPanelAdorner);
				}

				this.AddAdornerChildElement(this.CarouselPanelNavigator);
			}
		}

				#endregion //OnLoaded	
            
				#region OnOpacityEffectStopsCollectionChanged

		void OnOpacityEffectStopsCollectionChanged(object sender, PropertyChangedEventArgs e)
		{
			this.InvalidateArrange();
		}

				#endregion //OnOpacityEffectStopsCollectionChanged

				#region OnScalingEffectStopsCollectionChanged

		void OnScalingEffectStopsCollectionChanged(object sender, PropertyChangedEventArgs e)
		{
			this.InvalidateArrange();
		}

				#endregion //OnScalingEffectStopsCollectionChanged

				#region OnSkewAngleXEffectStopsCollectionChanged

		void OnSkewAngleXEffectStopsCollectionChanged(object sender, PropertyChangedEventArgs e)
		{
			this.InvalidateArrange();
		}

				#endregion //OnSkewAngleXEffectStopsCollectionChanged

				#region OnSkewAngleYEffectStopsCollectionChanged

		void OnSkewAngleYEffectStopsCollectionChanged(object sender, PropertyChangedEventArgs e)
		{
			this.InvalidateArrange();
		}

				#endregion //OnSkewAngleYEffectStopsCollectionChanged

				#region OnViewSettingsPropertyChanged

		void OnViewSettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			// [JM 04-23-07]
			// Update our internal CarouselViewSettings version number.
			this.BumpCarouselViewSettingsVersion();


			switch (e.PropertyName)
			{
				case "ShouldScrollItemsIntoInitialPosition":
					this._scrollOffsetForInitialArrangeAnimation = 0;
					break;

				case "IsListContinuous":
					this.UpdateCanNavigateStatus();
					this.InvalidateArrange();
					break;

				case "ItemSize":
					this._wrapperSizeDirty = true;
					this.InvalidateArrange();
					break;

				case "ItemPathPrefixPercent":
				case "ItemPathSuffixPercent":
					this.RecalculateCurrentItemLocations();
					this.InvalidateArrange();
					break;

				case "AutoScaleItemContentsToFit":
				case "ItemHorizontalScrollBarVisibility":
				case "ItemVerticalScrollBarVisibility":
					this.InvalidateArrange();
					break;

				case "UseZOrder":
				case "ZOrderEffectStopDirection":
				case "ZOrderEffectStops":
				case "OpacityEffectStops":
				case "OpacityEffectStopDirection":
				case "ScalingEffectStops":
				case "ScalingEffectStopDirection":
				case "SkewAngleXEffectStops":
				case "SkewAngleXEffectStopDirection":
				case "SkewAngleYEffectStops":
				case "SkewAngleYEffectStopDirection":
				case "UseOpacity":
				case "UseScaling":
				case "UseSkewing":
					this.InvalidateArrange();
					break;

				case "ItemPath":
					// Force the _itemPathGeometry to get recreated.
					this._itemPathGeometry = null;

					this.InvalidateVisual();
					this.InvalidateMeasure();
					this.UpdateLayout();
					break;

				case "ItemPathRenderBrush":
					this._cachedItemPathRenderBrush = this.ViewSettingsInternal.ItemPathRenderBrush;
					this.InvalidateVisual();
					break;

				case "ItemPathRenderPen":
					this._cachedItemPathRenderPen = this.ViewSettingsInternal.ItemPathRenderPen;
					this.InvalidateVisual();
					break;

				case "ItemPathAutoPad":
				case "ItemPathHorizontalAlignment":
				case "ItemPathPadding":
				case "ItemPathStretch":
				case "ItemPathVerticalAlignment":
				case "RotateItemsWithPathTangent":
					this.InvalidateMeasure();
					break;

				case "ReserveSpaceForReflections":
					this._wrapperSizeDirty					= true;
					this._visiblePositionPercentValuesDirty	= true;

					this.SetValue(ReflectionVisibilityPropertyKey, 
							this.ViewSettingsInternal.ReserveSpaceForReflections == true ? Visibility.Visible 
																				 : Visibility.Collapsed); 

					this.InvalidateMeasure();
					break;

				case "ItemsPerPage":
					// JM 05-09-12 TFS108386
					if (this.ScrollInProgress)
						this.StopAnimatedScrollProcessing();

					// Force new items to get generated if necessary as a result of the change.
					this.ShouldGenerateElements = true;


					// Update our ScrollData
					int itemsPerPage = this.ItemsPerPageResolved;
					this.UpdateScrollData(this.ScrollingData._extent,
										  new Size((double)itemsPerPage, (double)itemsPerPage),
										  this.ScrollingData._offset);

					this.RecalculateCurrentItemLocations();

					this.InvalidateMeasure();
					break;
					
				// [BR21028 21064 JM 3-13-07]
				case "CarouselPanelNavigatorStyle":
					this.CarouselPanelNavigator.ClearValue(CarouselPanelNavigator.StyleProperty);
					if (this.ViewSettingsInternal.CarouselPanelNavigatorStyle != null)
						this.CarouselPanelNavigator.Style = this.ViewSettingsInternal.CarouselPanelNavigatorStyle;

					break;

				default:
					break;
			}
		}

				#endregion //OnViewSettingsPropertyChanged	
        
				#region OnZOrderEffectStopsCollectionChanged

		void OnZOrderEffectStopsCollectionChanged(object sender, PropertyChangedEventArgs e)
		{
			this.SetZOrder();
		}

				#endregion //OnZOrderEffectStopsCollectionChanged

				#region PerformInitialArrangeAnimation

		delegate void MethodDelegate();
		private void PerformInitialArrangeAnimation()
		{
			if (this._initialArrangeAnimationDone == true)
				return;

			this._initialArrangeAnimationDone = true;

			this.SetVerticalOffset(this.ScrollOffsetForInitialArrangeAnimation);

			this._scrollOffsetForInitialArrangeAnimation = 0;
		}

				#endregion //PerformInitialArrangeAnimation	
   
				#region RecalculateCurrentItemLocations

		private void RecalculateCurrentItemLocations()
		{
			this._visiblePositionPercentValuesDirty	= true;
			this.RecalculateVisiblePositionPercentValues();

			// [JM 05-17-07] Change looping logic to use GetChildElement instead of iterating through the child elements collection.
			//UIElementCollection children		= this.Children;
			// JJD 6/6/07
			// Use Children collection to only access active children
			//int					childrenCount	= this.VisualChildrenCount; //children.Count;
			// AS 7/9/07
			//IList				children		= this.Children;
			IList				children		= this.ChildElements;
			int					childrenCount	= children.Count;
			int					currentOffset	= (int)this.ScrollingData._offset.Y;

			for (int i = 0; i < childrenCount; i++)
			{
				//CarouselPanelItem	child			= children[i] as CarouselPanelItem;
				// JJD 6/6/07
				// Use Children collection to only access active children
				//CarouselPanelItem	child			= this.GetChildElement(i) as CarouselPanelItem;
				CarouselPanelItem	child			= children[i] as CarouselPanelItem;

				// JM 05-07-08 - BR30992 - Check for null Child.
				if (child == null)
					continue;

				int					childItemIndex	= this.GetItemIndexFromChildIndex(i);

				child.PathLocationPercent			= this.CalculateChildTargetPct(i, childItemIndex, currentOffset, true, true);

				// JM BR25655 09-26-07
				child.NeedsArrange					= true;
			}
		}

				#endregion //RecalculateCurrentItemLocations	
    
				#region RecalculateVisiblePositionPercentValues

		private void RecalculateVisiblePositionPercentValues()
		{
			if (this._visiblePositionPercentValuesDirty)
			{
				CarouselViewSettings	viewSettings		= this.ViewSettingsInternal;
				int						itemsPerPage		= this.ItemsPerPageResolved;

				this._visiblePositionPercentValues			= new double[itemsPerPage];

				double firstPositionPercent					= viewSettings.ItemPathPrefixPercent;
				double lastPositionPercent					= 1 - viewSettings.ItemPathSuffixPercent;
				double totalPercentAvailableForVisibleItems = lastPositionPercent - firstPositionPercent;

				if (itemsPerPage - 1 < 0)
					this._distanceBetweenVisiblePositions	= totalPercentAvailableForVisibleItems;
				else
					this._distanceBetweenVisiblePositions	= totalPercentAvailableForVisibleItems / (itemsPerPage - 1);

				for (int i = 0; i < itemsPerPage; i++)
				{
					this._visiblePositionPercentValues[i]	= firstPositionPercent + (i * this._distanceBetweenVisiblePositions);
				}

				this._firstVisiblePositionPercentLocation	= this._visiblePositionPercentValues[0];
				this._lastVisiblePositionPercentLocation	= this._visiblePositionPercentValues[itemsPerPage - 1];

				this._visiblePositionPercentValuesDirty		= false;
			}
		}

				#endregion //RecalculateVisiblePositionPercentValues	

				#region RemoveRangeOfChldren

		// JJD 4/17/07
		// Added routine for removing a range of child elements
		private void RemoveRangeOfChldren(GeneratorPosition position, int count)
		{
			// [JM 05-17-07]
			if (base.ItemContainerGenerationModeResolved == ItemContainerGenerationMode.Recycle  ||
				base.Children == null)
				return;

			if (count < 1)
				return;

			int index = position.Index;

			if (position.Offset > 0)
				index += position.Offset;

			if (index < 0)
				return;

			// [JM 05-17-07]
			//int childCount = this.TotalGeneratedChildrenCount;
			// JJD 6/6/07
			// Use Children collection to only access active children
			//int childCount = this.CountOfActiveContainers;
			// AS 7/9/07
			//int childCount = this.Children.Count;
			int childCount = this.ChildElements.Count;
			if (index < childCount)
				base.RemoveInternalChildRange(index, Math.Min(count, childCount - index));
		}

				#endregion //RemoveRangeOfChldren	
    
				#region ScaleItemPath

		private void ScaleItemPath(Size availableSize)
		{
			CarouselViewSettings viewSettings = this.ViewSettingsInternal;

			// Reset the previous transform (if any) on the geometry before we look at the size of the geometry.
			this.ItemPathGeometry.Transform	= null;


			Rect		pathGeometryBounds			= new Rect(this.ItemPathGeometry.Bounds.Left,
															   this.ItemPathGeometry.Bounds.Top,
															   this.ItemPathGeometry.Bounds.Width	== 0 ? 1 : this.ItemPathGeometry.Bounds.Width,
															   this.ItemPathGeometry.Bounds.Height	== 0 ? 1 : this.ItemPathGeometry.Bounds.Height);

			Thickness	itemPathPadding				= viewSettings.ItemPathPadding;
			if (viewSettings.ItemPathAutoPad)
			{
				// [JM BR25571 08-09-07]
				//Size wrapperSize					= this.WrapperSize;
				Size wrapperSize					= viewSettings.ItemSize;
				itemPathPadding						= new Thickness(itemPathPadding.Left + (wrapperSize.Width / 2),
																	itemPathPadding.Top + (wrapperSize.Height / 2),
																	itemPathPadding.Right + (wrapperSize.Width / 2),
																	itemPathPadding.Bottom + (wrapperSize.Height / 2));
			}

			Rect		carouselPanelInnerBounds	= new Rect(0 + itemPathPadding.Left,
															   0 + itemPathPadding.Top,
															   Math.Max(availableSize.Width - (itemPathPadding.Left + itemPathPadding.Right), 0),
															   Math.Max(availableSize.Height - (itemPathPadding.Top + itemPathPadding.Bottom), 0));


			double scaleX = carouselPanelInnerBounds.Width / pathGeometryBounds.Width;
			double scaleY = carouselPanelInnerBounds.Height / pathGeometryBounds.Height;
			
			switch (viewSettings.ItemPathStretch)
			{
				case Stretch.None:
					scaleX = scaleY = 1;
					break;
				case Stretch.Fill:
					// No need to adjust our scaling factors since we're scaling in both dimensions.
					break;
				case Stretch.Uniform:
					scaleX = scaleY = Math.Min(scaleX, scaleY);

					break;
				case Stretch.UniformToFill:
					scaleX = scaleY = Math.Max(scaleX, scaleY);

					break;
			}


			// Establish the offset to use for positioning the path.
			double pathOffsetX			= -(scaleX * pathGeometryBounds.Left);
			double pathOffsetY			= -(scaleY * pathGeometryBounds.Top);


			// Adjust the offset for the path based on the ItemPathHorizontalAlignment and ItemPathVerticalAlignment properties.
			double scaledPathGeometryWidth	= this.ItemPathGeometry.Bounds.Width	== 0 ? 1 : pathGeometryBounds.Width * scaleX;
			double scaledPathGeometryHeight = this.ItemPathGeometry.Bounds.Height	== 0 ? 1 : pathGeometryBounds.Height * scaleY;

			switch (viewSettings.ItemPathHorizontalAlignment)
			{
				case HorizontalAlignment.Center:
				case HorizontalAlignment.Stretch:
					pathOffsetX += itemPathPadding.Left + ((carouselPanelInnerBounds.Width - scaledPathGeometryWidth) / 2);
					break;
				case HorizontalAlignment.Left:
					pathOffsetX += itemPathPadding.Left;
					break;
				case HorizontalAlignment.Right:
					pathOffsetX += itemPathPadding.Left + (carouselPanelInnerBounds.Width - scaledPathGeometryWidth);
					break;
			}

			switch (viewSettings.ItemPathVerticalAlignment)
			{
				case VerticalAlignment.Center:
				case VerticalAlignment.Stretch:
					pathOffsetY += itemPathPadding.Top + ((carouselPanelInnerBounds.Height - scaledPathGeometryHeight) / 2);
					break;
				case VerticalAlignment.Top:
					pathOffsetY += itemPathPadding.Top;
					break;
				case VerticalAlignment.Bottom:
					pathOffsetY += itemPathPadding.Top + (carouselPanelInnerBounds.Height - scaledPathGeometryHeight);
					break;
			}


			// Create and apply a Matrix transform to the PathGeometry based on the calculated scaling and offset values.
			this.ItemPathGeometry.Transform = new MatrixTransform(new Matrix(scaleX,
																			 0, 
																			 0,
																			 scaleY, 
																			 pathOffsetX, 
																			 pathOffsetY));

			//Debug.WriteLine("ScaleX/Y: " + scaleX.ToString() + "/" + scaleY.ToString() + "OffsetX/Y: " + pathOffsetX.ToString() + "/" + pathOffsetY.ToString() + ", PPInnerBounds: " + carouselPanelInnerBounds.ToString() + ", scaled PG W/H: " + scaledPathGeometryWidth.ToString() + "/" + scaledPathGeometryHeight.ToString());
		}

				#endregion //ScaleItemPath	
    
				#region SecondsFromTicks

		private static double SecondsFromTicks(long ticks)
		{
			double seconds = ticks / (double)10000000; //1 tick = 100-nanoseconds, so 10,000,000
			return seconds;
		}

				#endregion //SecondsFromTicks

				#region SetZOrder

		private void SetZOrder()
		{
			// [JM 05-17-07] Change looping logic to use GetChildElement instead of iterating through the child elements collection.
			//UIElementCollection		children		= this.Children;
			// JJD 6/6/07
			// Use Children collection to only access active children
			//int						childrenCount	= this.VisualChildrenCount; //children.Count;
			// AS 7/9/07
			//IList					children		= this.Children;
			IList					children		= this.ChildElements;
			int						childrenCount	= children.Count;
			CarouselViewSettings	viewSettings	= this.ViewSettingsInternal;

			// If ZOrder effects are not being used, reset the zIndex of each item if necessary so that
			// each item has a zIndex greater than the previous logical item.
			if (viewSettings.UseZOrder == false)
			{
				// [JM 04-17-07 BR22035]
				// Set the zIndex of each item taking into account the setting of IsListContinuous.  If IsListContinuous is
				// true, wrap around to the first item after the last item is processed to ebsure that zIndex is applied correctly
				// across the first/last item boundary.
				int firstVisibleChildIndex	= Math.Max(0, this.GetChildIndexFromItemIndex((int)this.ScrollingData._offset.Y));
				int totalChildrenProcessed	= 0;
				int zIndexOfPreviousItem	= -1;
				for (int i = firstVisibleChildIndex; totalChildrenProcessed < childrenCount && i < childrenCount; i++)
				{
					//if ((int)(children[i].GetValue(Panel.ZIndexProperty)) <= zIndexOfPreviousItem)
					//	children[i].SetValue(Panel.ZIndexProperty, zIndexOfPreviousItem + 1);
					// JJD 6/6/07
					// Use Children collection to only access active children
					//if ((int)(this.GetChildElement(i).GetValue(Panel.ZIndexProperty)) <= zIndexOfPreviousItem)
					//    this.GetChildElement(i).SetValue(Panel.ZIndexProperty, zIndexOfPreviousItem + 1);

					////zIndexOfPreviousItem = (int)(children[i].GetValue(Panel.ZIndexProperty));
					//zIndexOfPreviousItem = (int)(this.GetChildElement(i).GetValue(Panel.ZIndexProperty));
					DependencyObject child = children[i] as DependencyObject;

					int zIndex = (int)(child.GetValue(Panel.ZIndexProperty));

					if (zIndex <= zIndexOfPreviousItem)
					{
						zIndex = zIndexOfPreviousItem + 1;
						child.SetValue(Panel.ZIndexProperty, zIndex);
					}

					zIndexOfPreviousItem = zIndex;

					totalChildrenProcessed++;

					if (totalChildrenProcessed < childrenCount)
					{
						// Wrap around to the first child if IsListContinuous is true and we just processsed the last child in the list.
						if (viewSettings.IsListContinuous == true && i == (childrenCount - 1))
							i = -1;
					}
					
				}

				return;
			}


			//List<int>	childrenInZOrder	= new List<int>(children.Count);
			List<int>	childrenInZOrder	= new List<int>(childrenCount);
			Size		sceneSize;

			if (this._previousArrangeSize != Size.Empty)
				sceneSize = this._previousArrangeSize;
			else
			if (this._previousMeasureSize != Size.Empty)
				sceneSize = this._previousMeasureSize;
			else
				return;


			Rect						sceneRect					= new Rect(0, 0, sceneSize.Width, sceneSize.Height);
			int							childrenInZOrderCount;
			EffectStopDirection			effectStopDirectionResolved	= this.ZOrderEffectStopDirectionResolved;
			ZOrderEffectStopCollection	zorderEffectStopsResolved	= this.ZOrderEffectStopsResolved;

			// [JM 05-17-07] Change looping logic to use GetChildElement instead of iterating through the child elements collection.
			for (int i = 0; i < childrenCount; i++)
			{
				double				effectStopValue;
				bool				inserted	= false;
				//CarouselPanelItem	child		= children[i] as CarouselPanelItem;
				// JJD 6/6/07
				// Use Children collection to only access active children
				//CarouselPanelItem child = this.GetChildElement(i) as CarouselPanelItem;
				CarouselPanelItem	child		= children[i] as CarouselPanelItem;

				// JM 05-07-08 - BR30992 - Check for null Child.
				if (child == null)
					continue;

				if (child.Opacity < .2)
					continue;

				// Calculate the effectStopValue for the current child
				if (effectStopDirectionResolved == EffectStopDirection.Vertical)
					effectStopValue = zorderEffectStopsResolved.GetStopValueFromRange(sceneRect.Top,
																					  sceneRect.Bottom,
																					  (int)child.CurrentLocation.Y);
				else
				if (effectStopDirectionResolved == EffectStopDirection.Horizontal)
					effectStopValue = zorderEffectStopsResolved.GetStopValueFromRange(sceneRect.Left,
																					  sceneRect.Right,
																					  (int)child.CurrentLocation.X);
				else
					effectStopValue = zorderEffectStopsResolved.GetStopValueFromOffset(child.PathLocationPercent);


				// Save the effectStopValue in the CarouselPanelItem so we can compare against it when processing
				// subsequent children.
				child.CurrentZindexEffectStopValue = effectStopValue;


				// If there no entries in the childrenInZOrder list, just add the current child to the list.
				if (childrenInZOrder.Count == 0)
				{
					childrenInZOrder.Add(i);
					continue;
				}


				childrenInZOrderCount = childrenInZOrder.Count;
				for (int j = 0; j < childrenInZOrderCount; j++)
				{
					//if (effectStopValue <= ((CarouselPanelItem)children[childrenInZOrder[j]]).CurrentZindexEffectStopValue)
					// JJD 6/6/07
					// Use Children collection to only access active children
					//if (effectStopValue <= ((CarouselPanelItem)this.GetChildElement(childrenInZOrder[j])).CurrentZindexEffectStopValue)
					if (effectStopValue <= ((CarouselPanelItem)children[childrenInZOrder[j]]).CurrentZindexEffectStopValue)
					{
						childrenInZOrder.Insert(j, i);
						inserted = true;
						break;
					}
				}

				if (!inserted)
					childrenInZOrder.Add(i);
			}


			// Update the zorder of each element whose zorder has changed.
			childrenInZOrderCount = childrenInZOrder.Count;
			for (int i = 0; i < childrenInZOrderCount; i++)
			{
				//if ((int)(children[childrenInZOrder[i]].GetValue(Panel.ZIndexProperty)) != i)
				//	children[childrenInZOrder[i]].SetValue(Panel.ZIndexProperty, i);
				// JJD 6/6/07
				// Use Children collection to only access active children
				//if ((int)(this.GetChildElement(childrenInZOrder[i]).GetValue(Panel.ZIndexProperty)) != i)
				//    this.GetChildElement(childrenInZOrder[i]).SetValue(Panel.ZIndexProperty, i);
				DependencyObject child = children[childrenInZOrder[i]] as DependencyObject;
				if ((int)(child.GetValue(Panel.ZIndexProperty)) != i)
					child.SetValue(Panel.ZIndexProperty, i);
			}
		}

				#endregion //SetZOrder

				#region StartAnimatedScrollProcessing

		private void StartAnimatedScrollProcessing(ScrollActionInfo scrollActionInfo)
		{
			this._scrollActionInfo = scrollActionInfo;

            // JJD 9/08/09
            // Added proxy class to prevent rooting of the XamCarouselPanel if we don't
            // unwire the Rendering event. This was caught by unit tests.
			//CompositionTarget.Rendering += new EventHandler(CompositionTarget_Rendering);
            if ( this._renderingEventProxy == null )
                this._renderingEventProxy = new RenderingEventProxy(this);

            this._renderingEventProxy.WireRenderingEvent();
		}

				#endregion //StartAnimatedScrollProcessing	
    
				#region StopAnimatedScrollProcessing

		private void StopAnimatedScrollProcessing()
		{
			this._scrollActionInfo			= ScrollActionInfo.Empty;

            // JJD 9/08/09
            // Added proxy class to prevent rooting of the XamCarouselPanel if we don't
            // unwire the Rendering event. This was caught by unit tests.
            //CompositionTarget.Rendering		-= new EventHandler(CompositionTarget_Rendering);
            if (this._renderingEventProxy != null)
                this._renderingEventProxy.UnwireRenderingEvent();

			this.OnAnimatedScrollComplete((int)this.ScrollingData._offset.Y);

			base.TriggerCleanupOfUnusedGeneratedItems(false);
		}

				#endregion //StopAnimatedScrollProcessing	
        
				#region UpdateElementPosition

		private static bool UpdateElementPosition(CarouselPanelItem element, double secondsSinceLastFrameRender, double dampening, double attractionFactor, bool scrollInProgress, bool scrollingHigher)
		{
			Debug.Assert(element != null, "Element == null in UpdateElementPosition!");
			if (element == null)
				return false;

			double target	= element.PathTargetPercent;
			double current	= element.PathLocationPercent;
			
			if (current == target)
				return false;

			double	velocity			= element.Velocity;
			double	distanceToTarget	= 0;

			if (scrollInProgress)
			{
				if (scrollingHigher)
				{
					if (current > target)
						distanceToTarget = target - current;
					else
						distanceToTarget = 0;
				}
				else
				{
					if (target > current) 
						distanceToTarget = target - current;
					else
						distanceToTarget = 0;
				}
			}
			else
				distanceToTarget = target - current;

			if (Math.Abs(distanceToTarget) > MIN_DISTANCE_THRESHHOLD)// || Math.Abs(velocity) > MIN_DISTANCE_THRESHHOLD)
			{
				velocity *= dampening;
				velocity += distanceToTarget;

				double delta = velocity * secondsSinceLastFrameRender * attractionFactor;

				//velocity shouldn't be greater than...maxVelocity?
				delta *= (delta > MAX_VELOCITY) ? (MAX_VELOCITY / delta) : 1;

				current += delta;
				current = Math.Max(Math.Min(current, 1), 0);

				element.PathLocationPercent = current;
				element.Velocity			= velocity;

				return true;
			}
			else if (current != target)
			{
				current						= target;
				element.PathLocationPercent = current;
				return true;
			}
			else
				return false;
		}

				#endregion //UpdateElementPosition

				#region UpdateFirstAndLastItemPropertiesOnChild

		private static void UpdateFirstAndLastItemPropertiesOnChild(CarouselPanelItem child, int childsItemIndex, int totalItemCount)
		{
			Debug.Assert(child != null, "Child is null in UpdateFirstAndLastItemPropertiesOnChild!");
			if (child == null)
				return;


			
#region Infragistics Source Cleanup (Region)














#endregion // Infragistics Source Cleanup (Region)

			// Update firstitem/lastitem attached properties on the child.
			child.SetValue(CarouselPanelItem.IsFirstItemPropertyKey, childsItemIndex == 0);
			// AS 5/23/07 BR23156
			//child.SetValue(CarouselPanelItem.IsFirstItemPropertyKey, childsItemIndex == totalItemCount - 1);
			child.SetValue(CarouselPanelItem.IsLastItemPropertyKey, childsItemIndex == totalItemCount - 1);
		}

				#endregion //UpdateFirstAndLastItemPropertiesOnChild	

				#region UpdateScrollData







		private void UpdateScrollData(Size newExtent, Size newViewport, Vector newOffset)
		{
			if (this.ScrollingData._extent		!= newExtent	||
				this.ScrollingData._viewport	!= newViewport	||
				this.ScrollingData._offset		!= newOffset)
			{
				// Update scrolling data since something has changed.
				this.ScrollingData._extent		= newExtent;
				this.ScrollingData._viewport	= newViewport;
				this.ScrollingData._offset		= newOffset;

				this.SetValue(XamCarouselPanel.FirstVisibleItemIndexPropertyKey, (int)Math.Max(newOffset.Y, 0));


				// Update CanNavigatexxx based on the new offset.
				this.UpdateCanNavigateStatus();
			}
		}

				#endregion //UpdateScrollData

				#region UpdateCanNavigateStatus

		private void UpdateCanNavigateStatus()
		{
			CarouselViewSettings	viewSettings		= this.ViewSettingsInternal;
			bool					previousItemsExist	= (this.ScrollingData._offset.Y > this.LowestPossibleScrollOffset)	|| viewSettings.IsListContinuous;
			bool					followingItemsExist = (this.ScrollingData._offset.Y < this.HighestPossibleScrollOffset)	|| viewSettings.IsListContinuous;

			if (viewSettings.IsListContinuous == true)
			{
				// JM 10-07-08 TFS8498 - We only need to check the TotalItemCount - the ItemsPerPageResolved should not be considered.
				//						 This was suppressing scrolling when TotalItemCount was greater than 1 but the ItemsPerPageResolved was 1.
				// JM 12-21-07 BR29267 - Make sure we are displaying more than 1 item per page before enabling the nav buttons.
				//if (this.ItemsPerPageResolved > 1 && this.TotalItemCount > 1)
				if (this.TotalItemCount > 1)
				{
					// Update previousitem/nextitem status.
					this.SetValue(XamCarouselPanel.CanNavigateToPreviousItemPropertyKey, KnownBoxes.TrueBox);
					this.SetValue(XamCarouselPanel.CanNavigateToNextItemPropertyKey, KnownBoxes.TrueBox);

					// Update previouspage/nextpage status.
					int itemsPerPage = this.ItemsPerPageResolved;

					// JM 09-08-09 TFS21261 - Paging forward or back is a no-op when IsListContinuous = true and the ItemCount
					// equals the ItemsPerPage, because the display would end up showing the same items after the paging operation
					//this.SetValue(XamCarouselPanel.CanNavigateToPreviousPagePropertyKey, KnownBoxes.TrueBox);
					//this.SetValue(XamCarouselPanel.CanNavigateToNextPagePropertyKey, KnownBoxes.TrueBox);
					int itemCount = this.TotalItemCount;
					this.SetValue(XamCarouselPanel.CanNavigateToPreviousPagePropertyKey, itemsPerPage != itemCount);
					this.SetValue(XamCarouselPanel.CanNavigateToNextPagePropertyKey, itemsPerPage != itemCount);

					return;
				}
				else
				{
					// Update previousitem/nextitem status.
					this.SetValue(XamCarouselPanel.CanNavigateToPreviousItemPropertyKey, KnownBoxes.FalseBox);
					this.SetValue(XamCarouselPanel.CanNavigateToNextItemPropertyKey, KnownBoxes.FalseBox);

					// Update previouspage/nextpage status.
					this.SetValue(XamCarouselPanel.CanNavigateToPreviousPagePropertyKey, KnownBoxes.FalseBox);
					this.SetValue(XamCarouselPanel.CanNavigateToNextPagePropertyKey, KnownBoxes.FalseBox);


					return;
				}
			}


			// Update previousitem/nextitem status.
			if (previousItemsExist == true && this.CanNavigateToPreviousItem == false)
				this.SetValue(XamCarouselPanel.CanNavigateToPreviousItemPropertyKey, KnownBoxes.TrueBox);
			else
			if (previousItemsExist == false && this.CanNavigateToPreviousItem == true)
				this.SetValue(XamCarouselPanel.CanNavigateToPreviousItemPropertyKey, KnownBoxes.FalseBox);

			if (followingItemsExist == true && this.CanNavigateToNextItem == false)
				this.SetValue(XamCarouselPanel.CanNavigateToNextItemPropertyKey, KnownBoxes.TrueBox);
			else
			if (followingItemsExist == false && this.CanNavigateToNextItem == true)
				this.SetValue(XamCarouselPanel.CanNavigateToNextItemPropertyKey, KnownBoxes.FalseBox);


			// Update previouspage/nextpage status.
			if (previousItemsExist == true && this.CanNavigateToPreviousPage == false)
				this.SetValue(XamCarouselPanel.CanNavigateToPreviousPagePropertyKey, KnownBoxes.TrueBox);
			else
			if (previousItemsExist == false && this.CanNavigateToPreviousPage == true)
				this.SetValue(XamCarouselPanel.CanNavigateToPreviousPagePropertyKey, KnownBoxes.FalseBox);

			if (followingItemsExist == true && this.CanNavigateToNextPage == false)
				this.SetValue(XamCarouselPanel.CanNavigateToNextPagePropertyKey, KnownBoxes.TrueBox);
			else
			if (followingItemsExist == false && this.CanNavigateToNextPage == true)
				this.SetValue(XamCarouselPanel.CanNavigateToNextPagePropertyKey, KnownBoxes.FalseBox);
		}

				#endregion //UpdateCanNavigateStatus	
 
			#endregion //Private Methods

			#region Protected Methods

				#region AddAdornerChildElement

		/// <summary>
		/// Adds the specified child element to the <see cref="XamCarouselPanel"/>'s <see cref="CarouselPanelAdorner"/>
		/// </summary>
		/// <seealso cref="XamCarouselPanel"/>
		/// <seealso cref="CarouselPanelAdorner"/>
		/// <param name="childElement"></param>
		protected void AddAdornerChildElement(UIElement childElement)
		{
			if (this.CarouselPanelAdorner != null)
				this.CarouselPanelAdorner.AddChildElement(childElement);
		}

				#endregion //AddAdornerChildElement	
    
				#region GetIsItemIndexVisible

		/// <summary>
		/// Returns true if the item represented by the specified item index is visible given the specified scroll offset.
		/// </summary>
		/// <param name="itemIndex"></param>
		/// <param name="scrollOffset"></param>
		/// <returns></returns>
		internal protected bool GetIsItemIndexVisible(int itemIndex, int scrollOffset)
		{
			int visiblePosition = 0;
			return this.GetIsItemIndexVisible(itemIndex, scrollOffset, out visiblePosition);
		}

		private bool GetIsItemIndexVisible(int itemIndex, int scrollOffset, out int visiblePosition)
		{
			visiblePosition = this.GetVisiblePositionFromItemIndex(itemIndex, scrollOffset);

			return visiblePosition != -1;
		}

				#endregion //GetIsItemIndexVisible	

				// JM 01-05-12 TFS96730 Added.
				#region RemoveAdornerChildElement

		/// <summary>
		/// Removes the specified child element from the <see cref="XamCarouselPanel"/>'s <see cref="CarouselPanelAdorner"/>
		/// </summary>
		/// <seealso cref="XamCarouselPanel"/>
		/// <seealso cref="CarouselPanelAdorner"/>
		/// <param name="childElement"></param>
		protected void RemoveAdornerChildElement(UIElement childElement)
		{
			if (this.CarouselPanelAdorner != null)
				this.CarouselPanelAdorner.RemoveChildElement(childElement);
		}

				#endregion //RemoveAdornerChildElement

			#endregion //Protected Methods

			#region Protected Virtual Methods

				#region CanExecuteCommand
		// AS 2/5/08 ExecuteCommandInfo
		///// <param name="command">The command whose CanExecute state is being queried</param>
		///// <param name="commandParameter">Data used by the command. If the command does not require data to be passed, this object can be set to null.</param>
		//protected virtual bool CanExecuteCommand(RoutedCommand command, object commandParameter)
		/// <summary>
		/// Returns whether the specified command is allowed by the panel.
		/// </summary>
		/// <param name="commandInfo">Provides information about the command whose CanExecute state is being queried</param>
		protected virtual bool CanExecuteCommand(ExecuteCommandInfo commandInfo)
		{
			RoutedCommand command = commandInfo.RoutedCommand;
			return command != null && command.OwnerType == typeof(XamCarouselPanelCommands);
		} 
				#endregion //CanExecuteCommand

				#region OnAnimatedScrollComplete

		/// <summary>
		/// Called when the CarouselPanel completes an animated scroll operation.
		/// </summary>
		/// <param name="newScrollPosition">The new scroll position within the list of items being displayed by the CarouselPanel</param>
		protected virtual void OnAnimatedScrollComplete(int newScrollPosition)
		{
		}

				#endregion //OnAnimatedScrollComplete	

				#region OnCommand
		// AS 2/5/08 ExecuteCommandInfo
		//protected virtual void ExecuteCommand(ExecutedRoutedEventArgs args)
		///// <param name="args">Event arguments that indicating the command to be processed, the source of the event, etc.</param>
		/// <summary>
		/// Invoked when the panel should process a routed command.
		/// </summary>
		/// <param name="commandInfo">Contains information about the command being executed and the command parameter.</param>
		protected virtual bool ExecuteCommand(ExecuteCommandInfo commandInfo)
		{
			// AS 2/5/08 ExecuteCommandInfo
			//RoutedCommand command = args.Command as RoutedCommand;
			//if (command != null)
			//	args.Handled = this.ExecuteCommand(command);
			RoutedCommand command = commandInfo.RoutedCommand;
			return null != command && this.ExecuteCommand(command);
		} 
				#endregion //OnCommand

			#endregion //Protected Virtual Methods

			#region Static Methods

				#region FindTransform

		internal static Transform FindTransform(Transform renderTransform, Type transformTypeToFind)
		{
			if (renderTransform == null)
				return null;

			
			Type renderTransformType = renderTransform.GetType();
			if (renderTransformType == transformTypeToFind)
				return renderTransform;


			if (renderTransformType == typeof(TransformGroup))
			{
				TransformGroup		tg				= renderTransform as TransformGroup;
				TransformCollection tc				= tg.Children;
				int					childrenCount	= tc.Count;

				for (int i = 0; i < childrenCount; i++)
				{
					Transform t = tc[i];
					if (t.GetType() == transformTypeToFind)
						return t;
				}
			}

			return null;
		}

				#endregion //FindTransform

				#region ReplaceTransform

		internal static bool ReplaceTransform(UIElement element, Transform newTransform)
		{
			if (element.RenderTransform == null)
			{
				element.RenderTransform = newTransform;
				return true;
			}

			Type renderTransformType	= element.RenderTransform.GetType();
			Type newTransformType		= newTransform.GetType();
			if (renderTransformType == newTransformType)
			{
				element.RenderTransform = newTransform;
				return true;
			}


			if (renderTransformType == typeof(TransformGroup))
			{
				TransformGroup		tg				= element.RenderTransform as TransformGroup;
				TransformCollection	tc				= tg.Children;
				int					childrenCount	= tc.Count;

				for(int i = 0; i < childrenCount; i++)
				{
					if (tc[i].GetType() == newTransformType)
					{
						tc[i] = newTransform;

						return true;
					}
				}

				tc.Add(newTransform);

				return true;
			}

			return false;
		}

				#endregion //ReplaceTransform

			#endregion //Static Methods

			#region Internal Methods

				#region RemoveLogicalChildProxy







		internal void RemoveLogicalChildProxy(object child)
		{
			this.RemoveLogicalChild(child);
		}

				#endregion //RemoveLogicalChildProxy	

			#endregion //Internal Methods

		#endregion //Methods

		#region Events

		#region ExecutingCommand

		/// <summary>
		/// Event ID for the <see cref="ExecutingCommand"/> routed event
		/// </summary>
		/// <seealso cref="ExecutingCommand"/>
		/// <seealso cref="OnExecutingCommand"/>
		/// <seealso cref="ExecutingCommandEventArgs"/>
		public static readonly RoutedEvent ExecutingCommandEvent =
			EventManager.RegisterRoutedEvent("ExecutingCommand", RoutingStrategy.Bubble, typeof(EventHandler<ExecutingCommandEventArgs>), typeof(XamCarouselPanel));

		/// <summary>
		/// Occurs before a command is performed
		/// </summary>
		/// <seealso cref="ExecutingCommand"/>
		/// <seealso cref="ExecutingCommandEvent"/>
		/// <seealso cref="ExecutingCommandEventArgs"/>
		protected virtual void OnExecutingCommand(ExecutingCommandEventArgs args)
		{
			this.RaiseEvent(args);
		}

		internal bool RaiseExecutingCommand(ExecutingCommandEventArgs args)
		{
			args.RoutedEvent = ExecutingCommandEvent;
			args.Source = this;
			this.OnExecutingCommand(args);

			return args.Cancel == false;
		}

		/// <summary>
		/// Occurs before a command is performed
		/// </summary>
		/// <seealso cref="OnExecutingCommand"/>
		/// <seealso cref="ExecutingCommandEvent"/>
		/// <seealso cref="ExecutingCommandEventArgs"/>
		//[Description("Occurs before a command is performed")]
		//[Category("Behavior")]
		public event EventHandler<ExecutingCommandEventArgs> ExecutingCommand
		{
			add
			{
				base.AddHandler(ExecutingCommandEvent, value);
			}
			remove
			{
				base.RemoveHandler(ExecutingCommandEvent, value);
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
			EventManager.RegisterRoutedEvent("ExecutedCommand", RoutingStrategy.Bubble, typeof(EventHandler<ExecutedCommandEventArgs>), typeof(XamCarouselPanel));

		/// <summary>
		/// Occurs after a command is performed
		/// </summary>
		/// <seealso cref="ExecutedCommand"/>
		/// <seealso cref="ExecutedCommandEvent"/>
		/// <seealso cref="ExecutedCommandEventArgs"/>
		protected virtual void OnExecutedCommand(ExecutedCommandEventArgs args)
		{
			this.RaiseEvent(args);
		}

		internal void RaiseExecutedCommand(ExecutedCommandEventArgs args)
		{
			args.RoutedEvent = ExecutedCommandEvent;
			args.Source = this;
			this.OnExecutedCommand(args);
		}

		/// <summary>
		/// Occurs after a command is performed
		/// </summary>
		/// <seealso cref="OnExecutedCommand"/>
		/// <seealso cref="ExecutedCommandEvent"/>
		/// <seealso cref="ExecutedCommandEventArgs"/>
		//[Description("Occurs after a command is performed")]
		//[Category("Behavior")]
		public event EventHandler<ExecutedCommandEventArgs> ExecutedCommand
		{
			add
			{
				base.AddHandler(ExecutedCommandEvent, value);
			}
			remove
			{
				base.RemoveHandler(ExecutedCommandEvent, value);
			}
		}

		#endregion //ExecutedCommand

		#endregion //Events

		#region Nested Classes

            // JJD 9/08/09
            // Added proxy class to prevent rooting of the XamCarouselPanel if we don't
            // unwire the Rendering event. This was caught by unit tests.
            #region RenderingEventProxy

        private class RenderingEventProxy
        {
            private WeakReference _panelRef;
            private bool _isWired;

            internal RenderingEventProxy(XamCarouselPanel panel)
            {
                this._panelRef = new WeakReference(panel);
                this._isWired = false;
            }

            private void OnCompositionTarget_Rendering(object sender, EventArgs e)
            {
                XamCarouselPanel panel = Utilities.GetWeakReferenceTargetSafe(this._panelRef) as XamCarouselPanel;

                if (panel != null)
                    panel.CompositionTarget_Rendering(sender, e);
                else
                    this.UnwireRenderingEvent();
            }

            internal void UnwireRenderingEvent()
            {
                if (this._isWired)
                    CompositionTarget.Rendering -= new EventHandler(OnCompositionTarget_Rendering);

                this._isWired = false;
            }

            internal void WireRenderingEvent()
            {
                if (this._isWired)
                    return;

                this._isWired = true;

                CompositionTarget.Rendering += new EventHandler(OnCompositionTarget_Rendering);
            }
        }

            #endregion //RenderingEventProxy	
    
			#region ScrollData Internal Class

		internal class ScrollData
		{
			#region Member Variables

			internal ScrollViewer						_scrollOwner;
			internal Size								_extent = new Size();
			internal Size								_viewport = new Size();
			internal Vector								_offset = new Vector();
			internal bool								_canHorizontallyScroll;
			internal bool								_canVerticallyScroll;

			#endregion //Member Variables

			#region Methods

				#region Reset

			internal void Reset()
			{
				this._offset		= new Vector();
				this._extent		= new Size();
				this._viewport		= new Size();
			}

				#endregion //Reset

			#endregion //Methods
		}

			#endregion //ScrollData Internal Class

		#endregion //Nested Classes

		#region Nested Structs

			#region PathFractionPoint

		internal struct PathFractionPoint
		{
			internal Point					Point;
			internal Point					TangentPoint;

			internal PathFractionPoint(Point point, Point tangentPoint)
			{
				this.Point			= point;
				this.TangentPoint	= tangentPoint;
			}
		}

			#endregion //PathFractionPoint

			#region ScrollActionInfo Struct

		internal struct ScrollActionInfo
		{
			#region Member Variables

			internal double						OffsetBeforeScroll;
			internal double						OffsetAfterScroll;
			internal static ScrollActionInfo	Empty = new ScrollActionInfo(double.NegativeInfinity, double.NegativeInfinity, false, null);
			internal bool						ScrollingHigher;
			internal XamCarouselPanel			CarouselPanel;

			#endregion //Member Variables	

			#region Constructor

			internal ScrollActionInfo(double offsetBeforeScroll, double offsetAfterScroll, bool scrollingHigher, XamCarouselPanel carouselPanel)
			{
				this.OffsetAfterScroll	= offsetAfterScroll;
				this.OffsetBeforeScroll = offsetBeforeScroll;
				this.ScrollingHigher	= scrollingHigher;
				this.CarouselPanel		= carouselPanel;
			}

			#endregion //Constructor	
    
			#region Properties

				#region IsEmpty

			internal bool IsEmpty
			{
				get
				{
					return (this.OffsetBeforeScroll == double.NegativeInfinity) &&
						   (this.OffsetAfterScroll == double.NegativeInfinity);
				}
			}

				#endregion //IsEmpty	
    
				#region ScrollAmount

			internal int ScrollAmount
			{
				get
				{
					if (this.CarouselPanel.ViewSettingsInternal.IsListContinuous)
					{
						if (this.ScrollingHigher)
						{
							if (this.OffsetBeforeScroll > this.OffsetAfterScroll)
								return (this.CarouselPanel.TotalItemCount - (int)this.OffsetBeforeScroll) + ((int)this.OffsetAfterScroll + 1);
						}
						else
						{
							if (this.OffsetBeforeScroll < this.OffsetAfterScroll)
								return -((int)this.OffsetBeforeScroll + (this.CarouselPanel.TotalItemCount - (int)this.OffsetAfterScroll));
						}
					}

					return (int)(this.OffsetAfterScroll - this.OffsetBeforeScroll);
				}
			}

				#endregion //ScrollAmount	
    
				#region ScrollAmountAbsolute

			internal int ScrollAmountAbsolute
			{
				get
				{
					return Math.Abs(this.ScrollAmount);
				}
			}

				#endregion //ScrollAmountAbsolute	
    
			#endregion //Properties	
    
			#region Methods

				#region Equals

			
#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

				#endregion //Equals

			#endregion //Methods
		}

			#endregion //ScrollActionInfo Struct	
    
		#endregion //Nested Structs

		#region IWeakEventListener Members

		bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
		{
			if (managerType == typeof(PropertyChangedEventManager))
			{
				PropertyChangedEventArgs args = e as PropertyChangedEventArgs;

				if (args != null)
				{
					if (sender is CarouselViewSettings)
					{
						if (sender == this._viewSettings)
						{
							this.OnViewSettingsPropertyChanged(sender, args);
							return true;
						}

						Debug.Fail("Invalid sender object in ReceiveWeakEvent for CarouselViewPanel, arg type: " + e != null ? e.ToString() : "null");
					}

					Debug.Fail("Invalid sender type in ReceiveWeakEvent for CarouselViewPanel, arg type: " + e != null ? e.ToString() : "null");
				}

				Debug.Fail("Invalid args in ReceiveWeakEvent for CarouseViewPanel, arg type: " + e != null ? e.ToString() : "null");
			}

			Debug.Fail("Invalid managerType in ReceiveWeakEvent for CarouseViewPanel, type: " + managerType != null ? managerType.ToString() : "null");

			return false;
		}

		#endregion

		#region ICommandHost Members

		// AS 2/5/08 ExecuteCommandInfo
		//void ICommandHost.Execute(ExecutedRoutedEventArgs args)
		bool ICommandHost.Execute(ExecuteCommandInfo commandInfo)
		{
			// AS 2/5/08 ExecuteCommandInfo
			//return this.ExecuteCommand(args);
			RoutedCommand command = commandInfo.RoutedCommand;
			return command != null && this.ExecuteCommand(command);
		}

		// SSP 3/18/10 TFS29783 - Optimizations
		// Changed CurrentState property to a method.
		// 
		long ICommandHost.GetCurrentState( long statesToQuery )
		{
			return this.CurrentState;
		}
		//long ICommandHost.CurrentState
		//{
		//    get { return this.CurrentState; }
		//}

		// AS 2/5/08 ExecuteCommandInfo
		//bool ICommandHost.CanExecute(RoutedCommand command, object commandParameter)
		bool ICommandHost.CanExecute(ExecuteCommandInfo commandInfo)
		{
			// AS 2/5/08
			//return this.CanExecuteCommand(command, commandParameter);
			return this.CanExecuteCommand(commandInfo);
		}

		#endregion // ICommandHost
	}

	#endregion //XamCarouselPanel Class

	#region XamCarouselPanelCommands
	/// <summary>
	/// Provides a list of RoutedCommands supported by the <see cref="XamCarouselPanel"/>
	/// </summary>
    /// <remarks>
    /// <p class="body">The commands supported by the <see cref="XamCarouselPanel"/> are:
    ///		<ul>
    ///			<li><see cref="XamCarouselPanelCommands.NavigateToNextItem"/> - Navigates to the next item in the panel.</li>
    ///			<li><see cref="XamCarouselPanelCommands.NavigateToNextPage"/> - Navigates to the next page of items in the panel.</li>
    ///			<li><see cref="XamCarouselPanelCommands.NavigateToPreviousItem"/> - Navigates to the previous item in the panel.</li>
    ///			<li><see cref="XamCarouselPanelCommands.NavigateToPreviousPage"/> - Navigates to the previous page of items in the panel.</li>
    ///		</ul>
    /// </p>
    /// </remarks>
    public class XamCarouselPanelCommands : Commands<XamCarouselPanel>
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
		/// Navigates to the next item in the panel.
		/// </summary>
		public static readonly RoutedCommand NavigateToNextItem = new IGRoutedCommand("NavigateToNextItem",
			typeof(XamCarouselPanelCommands),
			0,
			(long)XamCarouselPanelStates.CanNavigateToNextItem);

		/// <summary>
		/// Navigates to the previous item in the panel.
		/// </summary>
		public static readonly RoutedCommand NavigateToPreviousItem = new IGRoutedCommand("NavigateToPreviousItem",
			typeof(XamCarouselPanelCommands),
			0,
			(long)XamCarouselPanelStates.CanNavigateToPreviousItem);

		/// <summary>
		/// Navigates to the next page of items in the panel.
		/// </summary>
		public static readonly RoutedCommand NavigateToNextPage = new IGRoutedCommand("NavigateToNextPage",
			typeof(XamCarouselPanelCommands),
			0,
			(long)XamCarouselPanelStates.CanNavigateToNextPage);

		/// <summary>
		/// Navigates to the previous page of items in the panel.
		/// </summary>
		public static readonly RoutedCommand NavigateToPreviousPage = new IGRoutedCommand("NavigateToPreviousPage",
			typeof(XamCarouselPanelCommands),
			0,
			(long)XamCarouselPanelStates.CanNavigateToPreviousPage);

		#endregion //Command Definitions

		// ====================================================================================================================================

		// ====================================================================================================================================
		// ADD COMMANDWRAPPERS HERE FOR EACH COMMAND DEFINED ABOVE.
		// ------------------------------------------------------------------------------------------------------------------------------------
		//
		#region CommandWrapper Definitions

		private static CommandWrapper[] GetCommandWrappers()
		{
			return new CommandWrapper[] {
				new CommandWrapper(	(IGRoutedCommand)NavigateToNextItem, null ),
				new CommandWrapper(	(IGRoutedCommand)NavigateToNextPage, null ),
				new CommandWrapper(	(IGRoutedCommand)NavigateToPreviousItem, null ),
				new CommandWrapper(	(IGRoutedCommand)NavigateToPreviousPage, null ),
			};
		}
		#endregion //CommandWrapper Definitions

		static XamCarouselPanelCommands()
		{
			// Call the Initialize method of our base class Commands<T> to register bindings for the commands represented
			// by our CommandWrappers.
			Commands<XamCarouselPanel>.Initialize(XamCarouselPanelCommands.GetCommandWrappers());
		}

		/// <summary>
		/// This method is provided as a convenience for initializing the statics in this class which kicks off
		/// the process of setting up and registering the commands.
		/// </summary>
		public static void LoadCommands()
		{
		}
	} 
	#endregion //XamCarouselPanelCommands

	#region Interface ICarouselPanelSelectionHost

	/// <summary>
	/// An interface Implemented by controls that host the XamCarouselPanel and support selection.
	/// </summary>
    /// <remarks>
    /// <p class="note"><b>Note: </b>This interface is for Infragistics internal use only.</p>
    /// </remarks>
    public interface ICarouselPanelSelectionHost
	{
		/// <summary>
		/// Returns/sets the index of the currently selected item.  If more than 1 item is selected this property returns/sets the first selected item.
		/// </summary>
		int		SelectedItemIndex { get; set; }
	}

	#endregion Interface ICarouselPanelSelectionHost

	#region Interface IActivationHost (commented out for now)

	///// <summary>
	///// Implemented by controls that host the XamCarouselPanel and that support activation/selection.
	///// </summary>
	//public interface IActivationHost
	//{
	//    /// <summary>
	//    /// Returns/sets the index of the current active item.  If the host does not support activation, then this property
	//    /// will return/set the index of the current selected item.
	//    /// </summary>
	//    int		ActiveItemIndex { get; set; }

	//    event	ActiveItemChangedEventHandler ActiveItemChanged;
	//}

	//#region ActiveItemChangedEventArgs class

	///// <summary>
	///// Event arguments for routed event <see cref="Infragistics.Windows.Controls.ActiveItemChanged"/>
	///// </summary>
	///// <seealso cref="Infragistics.Windows.Controls.IActivationHost"/>
	///// <seealso cref="Infragistics.Windows.Controls.XamCarouselPanel"/>
	///// <seealso cref="ActiveItemChangedEventHandler"/>
	//public class ActiveItemChangedEventArgs : RoutedEventArgs
	//{
	//    private int		_activeitemIndex;

	//    internal ActiveItemChangedEventArgs(int activeItemIndex)
	//    {
	//        this._activeitemIndex = activeItemIndex;
	//    }

	//    /// <summary>
	//    /// Returns the index of the item that has just been made active.
	//    /// </summary>
	//    public int ActiveItemIndex { get { return this._activeitemIndex; } }
	//}

	//#endregion //ActiveItemChangedEventArgs class

	//#endregion Interface IActivationHost

	//#region ActiveItemChanged Event

	//    /// <summary>
	//    /// Event ID for the <see cref="ActiveItemChanged"/> routed event
	//    /// </summary>
	//    /// <seealso cref="ActiveItemChangedEventArgs"/>
	//    /// <seealso cref="ActiveItemChangedEventHandler"/>
	//    public static readonly RoutedEvent ActiveItemChangedEvent =
	//        EventManager.RegisterRoutedEvent("ActiveItemChanged", RoutingStrategy.Bubble, typeof(ActiveItemChangedEventHandler), typeof(XamCarouselPanel));

	#endregion 
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