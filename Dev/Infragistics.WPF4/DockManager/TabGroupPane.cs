using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using Infragistics.Windows.Helpers;
using System.Windows.Data;
using Infragistics.Windows.Controls;
using Infragistics.Windows.Themes;
using System.ComponentModel;
using System.Collections;
using System.Windows.Input;
using System.Diagnostics;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Collections.Specialized;
using Infragistics.Shared;

using System.Windows.Automation.Peers;
using Infragistics.Windows.Automation.Peers.DockManager;
using System.Windows.Threading;
using System.Threading;
using Infragistics.Windows.DockManager.Events;

namespace Infragistics.Windows.DockManager
{
	/// <summary>
	/// A custom group pane used in a <see cref="XamDockManager"/> that can display one or more <see cref="ContentPane"/> instances as tab items.
	/// </summary>
	[TemplatePart(Name="PART_FilesMenuItem", Type=typeof(MenuItem))]
	[TemplatePart(Name="PART_HeaderArea", Type=typeof(FrameworkElement))]
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class TabGroupPane : TabControl
		, IContentPaneContainer
		, IPaneContainer
	{
		#region Member Variables

		private MenuItem _filesMenuItem;
		private MenuItem _hiddenMenuItem;
		private bool _initializingSelection;
		private bool _isMovingItem;
		private int _navigationModeVersion;
		private IList _itemsSnapshot;
		private FrameworkElement _headerArea;

		// AS 7/13/09 TFS18399
		private bool _isRefreshSelectedTabPeerPending = false;

		// AS 7/5/11 TFS80723
		private int _previousSelectedIndex;

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="TabGroupPane"/>
		/// </summary>
		public TabGroupPane()
		{
		}

		static TabGroupPane()
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(TabGroupPane), new FrameworkPropertyMetadata(typeof(TabGroupPane)));
			Control.HorizontalAlignmentProperty.OverrideMetadata(typeof(TabGroupPane), new FrameworkPropertyMetadata(KnownBoxes.HorizontalAlignmentStretchBox));
			Control.VerticalAlignmentProperty.OverrideMetadata(typeof(TabGroupPane), new FrameworkPropertyMetadata(KnownBoxes.VerticalAlignmentStretchBox));
			FrameworkElement.FocusableProperty.OverrideMetadata(typeof(TabGroupPane), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

			#region Default ItemsPanel

			FrameworkElementFactory fef = new FrameworkElementFactory(typeof(TabItemPanel));
			RelativeSource relativeSourceTab = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(TabGroupPane), 1);
			fef.SetBinding(TabControl.TabStripPlacementProperty, Utilities.CreateBindingObject(TabControl.TabStripPlacementProperty, BindingMode.OneWay, relativeSourceTab));
			fef.SetBinding(TabItemPanel.TabLayoutStyleProperty, Utilities.CreateBindingObject(TabItemPanel.TabLayoutStyleProperty, BindingMode.OneWay, relativeSourceTab));
			fef.SetBinding(TabItemPanel.MinimumTabExtentProperty, Utilities.CreateBindingObject(TabItemPanel.MinimumTabExtentProperty, BindingMode.OneWay, relativeSourceTab));
			fef.SetValue(Panel.IsItemsHostProperty, KnownBoxes.TrueBox);
			ItemsPanelTemplate template = new ItemsPanelTemplate(fef);
			template.Seal();

			ItemsControl.ItemsPanelProperty.OverrideMetadata(typeof(TabGroupPane), new FrameworkPropertyMetadata(template));

			#endregion //Default ItemsPanel

			XamDockManager.PaneLocationPropertyKey.OverrideMetadata(typeof(TabGroupPane), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnPaneLocationChanged)));

			// AS 7/5/11 TFS80723
			Selector.SelectedIndexProperty.OverrideMetadata(typeof(TabGroupPane), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnSelectedIndexChanged)));

			// we need to populate/change the files menu when opened...
			EventManager.RegisterClassHandler(typeof(TabGroupPane), MenuItem.SubmenuOpenedEvent, new RoutedEventHandler(OnSubmenuOpened));

			// we need to handle selecting the tab, etc. when a child requests to be brought into view
			EventManager.RegisterClassHandler(typeof(TabGroupPane), FrameworkElement.RequestBringIntoViewEvent, new RequestBringIntoViewEventHandler(OnRequestBringIntoView));

			// we need to change the visible to collapsed if all children are collapsed
			UIElement.VisibilityProperty.OverrideMetadata(typeof(TabGroupPane), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnVisibilityChanged), new CoerceValueCallback(CoerceVisibility)));
			EventManager.RegisterClassHandler(typeof(TabGroupPane), DockManagerUtilities.VisibilityChangedEvent, new RoutedEventHandler(OnChildVisibilityChanged));

			// AS 1/12/11 TFS62589
			TabGroupPane.ItemsSourceProperty.OverrideMetadata(typeof(TabGroupPane), new FrameworkPropertyMetadata(null, new CoerceValueCallback(CoerceItemsSource)));

			// AS 6/6/08 
			// With the change for BR33654, if a pane in the tab group has its AllowClose set to false
			// the Commands<T> handling of the command will not mark the CanExecute as handled and so
			// it will bubble up the chain to the dockmanager. The dm then delegates that command to the 
			// the active pane. Since the commandmanager adjusted the original source to be the element
			// in the root focusscope before bubbling up in the root focus scope, the dm has no idea where
			// the command came from - a child element or an element in an outside focus scope so it has
			// to assume that it should delegate. I started to go the route of changing the commands<t> to 
			// let the ICommandHost provide a way of indicating if the command should be marked handled
			// even if it cannot be executed but in this case the close command has a minimum state of 
			// requiring allowclose so it doesn't even get to asking the commandhost. For now, we have 
			// implemented a separate command for the tabgrouppane so it can deal with closing the 
			// selected item. It in turn will use the close command of the contentpane so the cp will 
			// still raise its executing/executed command events.
			//
			CommandManager.RegisterClassCommandBinding(typeof(TabGroupPane), new CommandBinding(TabGroupPane.CloseSelectedItemCommand, new ExecutedRoutedEventHandler(OnExecuteCommand), new CanExecuteRoutedEventHandler(OnCanExecuteCommand)));
		}
		#endregion //Constructor

		#region Base class overrides

		#region ArrangeOverride
		/// <summary>
		/// Positions child elements and determines a size for this element.
		/// </summary>
		/// <param name="arrangeBounds">The size available to this element for arranging its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> used by this element to arrange its children.</returns>
		protected override Size ArrangeOverride(Size arrangeBounds)
		{
			DockManagerUtilities.InitializePaneFloatingSize(this, this.Items, arrangeBounds);

			return base.ArrangeOverride(arrangeBounds);
		}
		#endregion //ArrangeOverride

        #region OnCreateAutomationPeer

        /// <summary>
        /// Returns <see cref="TabGroupPane"/> Automation Peer Class <see cref="TabGroupPaneAutomationPeer"/>
        /// </summary>
        /// <returns>AutomationPeer</returns>
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new TabGroupPaneAutomationPeer(this);
        }

        #endregion //OnCreateAutomationPeer

		#region ClearContainerForItemOverride
		/// <summary>
		/// Called to clear the effects of the PrepareContainerForItemOverride method. 
		/// </summary>
		/// <param name="element">The container being cleared.</param>
		/// <param name="item">The item contained by the container being cleared.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Portability", "CA1903:UseOnlyApiFromTargetedFramework", MessageId = "System.Windows.Controls.Primitives.Selector.#ClearContainerForItemOverride(System.Windows.DependencyObject,System.Object)")]
        protected override void ClearContainerForItemOverride(DependencyObject element, object item)
		{
			PaneTabItem tabItem = element as PaneTabItem;

			BindingOperations.ClearBinding(tabItem, PaneTabItem.HeaderProperty);
			BindingOperations.ClearBinding(tabItem, PaneTabItem.HeaderTemplateProperty);
			BindingOperations.ClearBinding(tabItem, PaneTabItem.HeaderTemplateSelectorProperty);

			base.ClearContainerForItemOverride(element, item);
		}
		#endregion //ClearContainerForItemOverride

		#region GetContainerForItemOverride
		/// <summary>
		/// Returns an instance of element used to display an item within the tab control.
		/// </summary>
		/// <returns>An instance of the <see cref="PaneTabItem"/> class</returns>
		protected override DependencyObject GetContainerForItemOverride()
		{
			return new PaneTabItem();
		}
		#endregion //GetContainerForItemOverride

		#region IsItemItsOwnContainerOverride
		/// <summary>
		/// Determines if the specified item is (or is eligible to be) its own container. 
		/// </summary>
		/// <param name="item">The item to evaluate</param>
		/// <returns>True if the specified item is its own container.</returns>
		protected override bool IsItemItsOwnContainerOverride(object item)
		{
			return item is PaneTabItem;
		}
		#endregion //IsItemItsOwnContainerOverride

		#region OnApplyTemplate
		/// <summary>
		/// Invoked when the template for the control has been changed.
		/// </summary>
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			if (null != this._filesMenuItem && null != this._hiddenMenuItem)
				this._filesMenuItem.Items.Remove(this._hiddenMenuItem);

			this._filesMenuItem = this.GetTemplateChild("PART_FilesMenuItem") as MenuItem;

			// put a default hidden menu item in the template so the menu item is considered
			// a header item and not a leaf item - otherwise we won't get the submenuopened event
			if (null != this._filesMenuItem)
				this._filesMenuItem.Items.Add(this.HiddenMenuItem);

			this._headerArea = this.GetTemplateChild("PART_HeaderArea") as FrameworkElement;
		}
		#endregion //OnApplyTemplate

		#region OnKeyDown
		/// <summary>
		/// Invoked when a key is pressed while focus is within the control.
		/// </summary>
		/// <param name="e">Provides data for the event</param>
		protected override void OnKeyDown(KeyEventArgs e)
		{
			// alt-ctrl-down arrow will show the files menu item
			if (e.Key == Key.Down && Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Alt))
			{
				if (this._filesMenuItem != null)
				{
					this._filesMenuItem.IsSubmenuOpen = true;
					this._filesMenuItem.Focus();

					e.Handled = true;
				}
			}

			// note: i was going to handle the ctrl-pageup/down to navigate between the items when 
			// not in a document tab group to mimic vs but scrollviewer intercepts that keystroke
			// and since scrollviewers are likely to be in the element tree between us and the focused
			// item, it would rarely work and might come off as inconsistent behavior of ours so
			// we've decided not to try and handle this yet.

			// the base class processes keystrokes like ctrl-tab, home, end
			// which we do not want handled.
			return;
		} 
		#endregion //OnKeyDown

		#region OnInitialized
		/// <summary>
		/// Invoked after the element has been initialized.
		/// </summary>
		/// <param name="e">Provides information for the event.</param>
		protected override void OnInitialized(EventArgs e)
		{
			Debug.Assert(this.ItemContainerGenerator != null);

			// AS 5/20/08 BR33082
			if (null != this.ItemContainerGenerator)
				this.ItemContainerGenerator.StatusChanged += new EventHandler(OnGeneratorStatusChanged);

			base.OnInitialized(e);

			// hide the element if it doesn't have any visible children
			this.CoerceValue(FrameworkElement.VisibilityProperty);

			// make sure a containing toolwindow knows it has new children
			DockManagerUtilities.VerifyOwningToolWindow(this);

			// make sure the tab item is hidden/shown as needed
			this.VerifyTabAreaVisibility();
		}
		#endregion //OnInitialized

		#region OnItemsChanged
		/// <summary>
		/// Invoked when the <see cref="ItemsControl.Items"/> collection has been changed.
		/// </summary>
		protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
		{
			// AS 7/16/09 TFS18852
			// We were previously validating the item type in the PrepareContainerForItemOverride
			// but other methods earlier expect certain item types so now we'll explicitly raise 
			// this.
			//
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Replace:
				case NotifyCollectionChangedAction.Add:
					foreach (object item in e.NewItems)
						this.VerifyItem(item);
					break;
				case NotifyCollectionChangedAction.Reset:
					this.VerifyAllItems();
					break;
			}

			// AS 5/4/10 TFS31380
			this.FixupFilesMenu(e);

			// if we had an items snapshot then release it since the collection was changed
			// unless it was just changed by us promoting an item to the front which would
			// happen during navigation
			if (this._isMovingItem == false)
				this.ClearItemsSnapshot();

			// make sure all items have their current container set to this element
			DockManagerUtilities.InitializeCurrentContainer(this.Items, this, e);

			// hide the element if it doesn't have any visible children
			this.CoerceValue(FrameworkElement.VisibilityProperty);

			// let the tool window know in case it needs to show/hide its caption
			DockManagerUtilities.VerifyOwningToolWindow(this);

			// update the tab item area visibility as needed
			this.VerifyTabAreaVisibility();

			// AS 5/20/08 BR33082
			this.VerifySelectedItem();

			base.OnItemsChanged(e);

			// AS 5/1/08 BR32272
			// When the GeneratorStatusChanged event from the itemcontainergenerator is
			// caught, the TabControl calls its UpdateSelectedContent method. This method
			// uses a helper method named GetSelectedTabItem. This method assumes that the
			// SelectedItem and SelectedIndex are in sync. They are getting this call 
			// during the Insert call above so if the item we inserted is before the
			// selected item then that assumption is wrong because the Selector has not
			// updated the SelectedIndex yet. The GetSelectedTabItem will work if the selected
			// item is a tab item but if its not, then it uses the SelectedIndex to get the
			// container from the generator (probably for perf reasons) instead of getting
			// the container from the item (although it should have validated the item).
			// Anyway, the tab item it returns is used by the UpdateSelectedContent to 
			// update the SelectedContent property.
			//
			this.VerifySelectedContent();
		}
		#endregion //OnItemsChanged

		#region OnSelectionChanged
		/// <summary>
		/// Invoked when the selection has been changed.
		/// </summary>
		/// <param name="e">Provides information about the selection change</param>
		protected override void OnSelectionChanged(SelectionChangedEventArgs e)
		{
			if (this._initializingSelection)
				return;

			
#region Infragistics Source Cleanup (Region)













#endregion // Infragistics Source Cleanup (Region)

			// AS 5/8/12 TFS108266
			// Moved to a helper method.
			//
			//if (this.CanRepositionSelectedItem())
			//{
			//    try
			//    {
			//        this._initializingSelection = true;
			//
			//        this.MoveItemToFront(this.SelectedIndex);
			//    }
			//    finally
			//    {
			//        this._initializingSelection = false;
			//    }
			//
			//    // now we can reselect the first item
			//    this.SelectedIndex = 0;
			//    return;
			//}
			if (this.EnsureSelectedDocumentIsInViewImpl(false))
				return;

			DockManagerUtilities.VerifyOwningToolWindow(this);

			// let the content get updated
			base.OnSelectionChanged(e);

			// AS 7/14/09 TFS18400
			// Note, we have to do this after calling the base OnSelectionChanged because
			// we need the tab control to update its selected content.
			//
			this.VerifyActivePanes();

			// AS 7/13/09 TFS18399
			this.RefreshSelectedTabPeerChildren();
		} 
		#endregion //OnSelectionChanged

		#region OnVisualParentChanged
		/// <summary>
		/// Invoked when the visual parent of the element has been changed.
		/// </summary>
		/// <param name="oldParent">The previous visual parent or null if the element was not contained within another element.</param>
		protected override void OnVisualParentChanged(DependencyObject oldParent)
		{
			// AS 9/8/09 TFS19079
			// We have to wait to verify the items until after the default initializer has 
			// a chance to clear out the default tab items that the designtime adds to this 
			// class because it is a derived TabControl.
			//
			this.VerifyAllItems();

			base.OnVisualParentChanged(oldParent);
		} 
		#endregion //OnVisualParentChanged

		#region PrepareContainerForItemOverride
		/// <summary>
		/// Prepares the specified container element to display the specified item. 
		/// </summary>
		/// <param name="element">The container element to prepare.</param>
		/// <param name="item">The item contained by the specified container element.</param>
		protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
		{
			// AS 7/16/09 TFS18852
			this.VerifyItem(item);

			if (item is ContentPanePlaceholder == false)
			{
				// AS 7/16/09 TFS18852
				//if (item is ContentPane == false)
				//	throw new InvalidOperationException(SR.GetString("LE_InvalidTabGroupPaneChild"));

				PaneTabItem tabItem = element as PaneTabItem;
				ContentPane pane = item as ContentPane;

				tabItem.SetBinding(PaneTabItem.HeaderProperty, Utilities.CreateBindingObject(ContentPane.TabHeaderProperty, System.Windows.Data.BindingMode.OneWay, pane));
				tabItem.SetBinding(PaneTabItem.HeaderTemplateProperty, Utilities.CreateBindingObject(ContentPane.TabHeaderTemplateProperty, System.Windows.Data.BindingMode.OneWay, pane));
				tabItem.SetBinding(PaneTabItem.HeaderTemplateSelectorProperty, Utilities.CreateBindingObject(ContentPane.TabHeaderTemplateSelectorProperty, System.Windows.Data.BindingMode.OneWay, pane));
			}

			BindingOperations.SetBinding(element, UIElement.VisibilityProperty, Utilities.CreateBindingObject(UIElement.VisibilityProperty, System.Windows.Data.BindingMode.OneWay, item));

			base.PrepareContainerForItemOverride(element, item);
		}
		#endregion //PrepareContainerForItemOverride

		#endregion //Base class overrides

		#region Resource Keys

		#region DockableTabGroupTemplateKey

		/// <summary>
		/// The key used to identify the <see cref="ControlTemplate"/> for the <see cref="TabGroupPane"/> when it is not contained within the <see cref="DocumentContentHost"/> area.
		/// </summary>
		/// <seealso cref="DocumentTabGroupTemplateKey"/>
		/// <seealso cref="PaneTabItem.DockableTabItemTemplateKey"/>
		public static readonly ResourceKey DockableTabGroupTemplateKey = new StaticPropertyResourceKey(typeof(TabGroupPane), "DockableTabGroupTemplateKey");

		#endregion //DockableTabGroupTemplateKey

		#region DocumentTabGroupTemplateKey

		/// <summary>
		/// The key used to identify the <see cref="ControlTemplate"/> for the <see cref="TabGroupPane"/> when hosted within a <see cref="DocumentContentHost"/>
		/// </summary>
		/// <seealso cref="DockableTabGroupTemplateKey"/>
		/// <seealso cref="PaneTabItem.DocumentTabItemTemplateKey"/>
		public static readonly ResourceKey DocumentTabGroupTemplateKey = new StaticPropertyResourceKey(typeof(TabGroupPane), "DocumentTabGroupTemplateKey");

		#endregion //DocumentTabGroupTemplateKey

		#region DocumentCloseButtonStyleKey

		/// <summary>
		/// The key used to identify the <see cref="Style"/> for the close button when the <see cref="TabGroupPane"/> is within the <see cref="DocumentContentHost"/>.
		/// </summary>
		public static readonly ResourceKey DocumentCloseButtonStyleKey = new StaticPropertyResourceKey(typeof(TabGroupPane), "DocumentCloseButtonStyleKey");

		#endregion //DocumentCloseButtonStyleKey

		#region DocumentFilesMenuItemStyleKey

		/// <summary>
		/// The key used to identify the <see cref="Style"/> for the files <see cref="MenuItem"/> when the <see cref="TabGroupPane"/> is within the <see cref="DocumentContentHost"/>.
		/// </summary>
		public static readonly ResourceKey DocumentFilesMenuItemStyleKey = new StaticPropertyResourceKey(typeof(TabGroupPane), "DocumentFilesMenuItemStyleKey");

		#endregion //DocumentFilesMenuItemStyleKey

		#region DocumentPaneNavigatorButtonStyleKey

		/// <summary>
		/// The key used to identify the <see cref="Style"/> for the show pane navigator button when the <see cref="TabGroupPane"/> is within the <see cref="DocumentContentHost"/>.
		/// </summary>
		public static readonly ResourceKey DocumentPaneNavigatorButtonStyleKey = new StaticPropertyResourceKey(typeof(TabGroupPane), "DocumentPaneNavigatorButtonStyleKey");

		#endregion //DocumentPaneNavigatorButtonStyleKey

		#endregion //Resource Keys

		#region Properties

		#region Public Properties

		#region CloseSelectedItemCommand
		/// <summary>
		/// A command used to close the selected <see cref="ContentPane"/> in a <see cref="TabGroupPane"/>
		/// </summary>
		public static readonly RoutedCommand CloseSelectedItemCommand = new RoutedCommand("CloseSelectedItem", typeof(TabGroupPane));

		#endregion //CloseSelectedItemCommand

		#region IsTabItemAreaVisible

		private static readonly DependencyPropertyKey IsTabItemAreaVisiblePropertyKey =
			DependencyProperty.RegisterReadOnly("IsTabItemAreaVisible",
			typeof(bool), typeof(TabGroupPane), new FrameworkPropertyMetadata(KnownBoxes.TrueBox));

		/// <summary>
		/// Identifies the <see cref="IsTabItemAreaVisible"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsTabItemAreaVisibleProperty =
			IsTabItemAreaVisiblePropertyKey.DependencyProperty;

		/// <summary>
		/// Returns a boolean indicating if the tab item area should be displayed.
		/// </summary>
		/// <seealso cref="IsTabItemAreaVisibleProperty"/>
		//[Description("Returns a boolean indicating if the tab item area should be displayed.")]
		//[Category("DockManager Properties")] // Behavior
		[Bindable(true)]
		[ReadOnly(true)]
		public bool IsTabItemAreaVisible
		{
			get
			{
				return (bool)this.GetValue(TabGroupPane.IsTabItemAreaVisibleProperty);
			}
		}

		#endregion //IsTabItemAreaVisible

		#region ItemsSource
		/// <summary>
		/// The ItemsSource is not supported for the <see cref="TabGroupPane"/>
		/// </summary>
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new IEnumerable ItemsSource
		{
			get { return ((ItemsControl)this).ItemsSource; }
			set { ((ItemsControl)this).ItemsSource = value; }
		} 
		#endregion //ItemsSource

		#endregion //Public Properties

		#region Internal Properties

		// AS 7/14/09 TFS18400
		#region ContainsActivePane
		internal bool ContainsActivePane
		{
			get
			{
				XamDockManager dm = XamDockManager.GetDockManager(this);

				if (dm == null)
					return false;

				return this.ContainsPane(dm.ActivePaneManager.GetLastActivePane());
			}
		} 
		#endregion //ContainsActivePane

		// AS 7/14/09 TFS18400
		#region ContainsActiveDocument
		internal bool ContainsActiveDocument
		{
			get
			{
				if (XamDockManager.GetPaneLocation(this) != PaneLocation.Document)
					return false;

				XamDockManager dm = XamDockManager.GetDockManager(this);

				if (dm == null)
					return false;

				return this.ContainsPane(dm.ActivePaneManager.GetActiveDocument());
			}
		} 
		#endregion //ContainsActiveDocument

		#region HeaderArea
		internal FrameworkElement HeaderArea
		{
			get { return this._headerArea; }
		} 
		#endregion //HeaderArea

		#region IsMovingItem
		internal bool IsMovingItem
		{
			get { return this._isMovingItem; }
		}
		#endregion //IsMovingItem

		#endregion //Internal Properties

		#region Private Properties

		// AS 9/8/09 TFS19079
		// Do not verify the items until the initializer has had a chance to run. Otherwise 
		// we will throw an exception because the default tabs that the designer adds will 
		// still be in the items collection.
		//
		#region CanVerifyItems
		private bool CanVerifyItems
		{
			get { return !DesignerProperties.GetIsInDesignMode(this) || VisualTreeHelper.GetParent(this) != null; }
		} 
		#endregion //CanVerifyItems

		#region CurrentItemsCollection
		/// <summary>
		/// Returns either the items snapshot or the current items collection. This should only be used by something that doesn't 
		/// need to modify the collection.
		/// </summary>
		private IList CurrentItemsCollection
		{
			get
			{
				this.VerifyItemsSnapshot();

				return this._itemsSnapshot ?? (IList)this.Items;
			}
		}
		#endregion // CurrentItemsCollection

		#region HiddenMenuItem
		private MenuItem HiddenMenuItem
		{
			get
			{
				if (null == this._hiddenMenuItem)
				{
					this._hiddenMenuItem = new MenuItem();
					this._hiddenMenuItem.Visibility = Visibility.Collapsed;
				}

				return this._hiddenMenuItem;
			}
		}
		#endregion //HiddenMenuItem

		#endregion //Private Properties

		#endregion //Properties

		#region Methods

		#region Private Methods

		// AS 12/9/09 TFS25268
		// Most of this was moved from the OnSelectionChanged criteria.
		//
		#region CanRepositionSelectedItem
		private bool CanRepositionSelectedItem(bool checkClip )
		{
			if (!this.IsInitialized)
				return false;

			if (this.SelectedIndex < 0)
				return false;

			if (XamDockManager.GetPaneLocation(this) != PaneLocation.Document)
				return false;

			TabItem selectedTab = this.GetSelectedTabItem();

			if (selectedTab != null)
			{
				// AS 5/8/12 TFS108266
				// The selected item may be allocated but completely clipped 
				// in which case we conditionally want to consider that state.
				//
				//return false;
				bool isInView = true;
				if (checkClip)
				{
					var clip = VisualTreeHelper.GetClip(selectedTab);

					if (clip == Geometry.Empty)
						isInView = false;
					else if (clip is RectangleGeometry)
					{
						var rect = ((RectangleGeometry)clip).Rect;

						if (rect.Height <= 0 || rect.Width <= 0)
							isInView = false;
					}
				}

				if (isInView)
					return false;
			}

			XamDockManager dm = XamDockManager.GetDockManager(this);

			if (dm == null)
				return false;

			// AS 12/9/09 TFS25268
			// If we're repositioning items during a drag then we 
			// don't want to interfere with the processing.
			//
			switch (dm.DragManager.DragState)
			{
				case Dragging.DragState.Dragging:
				case Dragging.DragState.ProcessingDrop:
					return false;
			}

			// AS 12/9/09 TFS25268
			// The following would be inconsistent depending on where an item 
			// was inserted. Essentially what we originally wanted to check was 
			// if any containers were generated. Note the status of 
			// ContainersGenerated cannot be relied upon because that is never 
			// changed once set.
			//
			//// make sure there are some items generated
			//this.ItemContainerGenerator.ContainerFromIndex(0) != null)
			bool hasContainers = false;

			for (int i = 0; i < this.Items.Count; i++)
			{
				// AS 5/8/12 TFS108266
				// This isn't required for this fix but we shouldn't do this 
				// for anything except our document tab panel.
				//
				//if (this.ItemContainerGenerator.ContainerFromIndex(i) != null)
				//	hasContainers = true;
				var container = this.ItemContainerGenerator.ContainerFromIndex(i);

				if (container != null)
				{
					// only do this if its our document panel
					if (!(VisualTreeHelper.GetParent(container) is DocumentTabPanel))
						return false;

					hasContainers = true;
					break;
				}
			}

			if (!hasContainers)
				return false;

			return true;
		}
		#endregion //CanRepositionSelectedItem

		#region ClearItemsSnapshot
		private void ClearItemsSnapshot()
		{
			Debug.WriteLineIf(this._itemsSnapshot != null, "Clearing ItemsSnapshot:" + this.GetHashCode());
			this._itemsSnapshot = null;
		}
		#endregion // ClearItemsSnapshot

		// AS 10/28/10 TFS42895
		// There is a bug in the Selector's SelectedValue coersion that it will check its HasItems and 
		// then if true will access the 0th entry. However HasItems can be out of sync as it won't be 
		// updated until the collection change notification. So if the IsEmpty (which is what the 
		// HasItems is based upon) is out of sync with the HasItems then check the selection again.
		// In doing this I added an overload of VerifySelectedItem that takes a param so we know whether 
		// its being called async to ensure we don't get into a situation where we endless call this 
		// not that these properties should ever get permanently out of sync.
		//
		#region ClearSelectionProperties
		private void ClearSelectionProperties()
		{
			if (this.Items.IsEmpty == this.HasItems)
			{
				this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new SendOrPostCallback(VerifySelectedItem), this);
			}
			else
			{
				this.SelectedItem = null;
				this.SelectedValue = null;
			}
		} 
		#endregion //ClearSelectionProperties

		#region CoerceItemsSource
		private static object CoerceItemsSource(DependencyObject d, object newValue)
		{
			// AS 1/12/11 TFS62589
			// In Blend when you edit a TabGroupPane Style or Template, Blend is setting 
			// the ItemsSource to a string representing the class name ("TabGroupPane" in 
			// this case) and then to a list of strings if the template has an ItemsPresenter.
			// Since the TabGroupPane is a specialized TabControl that can only display 
			// ContentPane instances this results in an exception. We'll suppress that at 
			// design time and show some dummy contentpane instances. Note I'm not localizing 
			// the header because I'm specifically using the ContentPane's class name so 
			// people will understand what is in there.
			//
			if (newValue != null && DesignerProperties.GetIsInDesignMode(d))
			{
				if (newValue is string || newValue is IList<string>)
				{
					List<ContentPane> panes = new List<ContentPane>();
					panes.Add(new ContentPane { Header = "ContentPane" });
					panes.Add(new ContentPane { Header = "ContentPane" });
					return panes;
				}
			}

			return newValue;
		}
		#endregion //CoerceItemsSource

		#region CoerceVisibility
		private static object CoerceVisibility(DependencyObject d, object newValue)
		{
			return DockManagerUtilities.ProcessCoerceVisibility(d, newValue);
		} 
		#endregion //CoerceVisibility

		// AS 7/14/09 TFS18400
		#region ContainsPane
		private bool ContainsPane(ContentPane contentPane)
		{
			if (null == contentPane)
				return false;

			TabGroupPane tgp = LogicalTreeHelper.GetParent(contentPane) as TabGroupPane;

			if (tgp != this)
			{
				Debug.Assert(this.Items.IndexOf(contentPane) < 0);
				return false;
			}

			return this.Items.IndexOf(contentPane) >= 0;
		}
		#endregion //ContainsPane

		// AS 5/8/12 TFS108266
		// Moved the implementation here from the OnSelectionChanged
		//
		#region EnsureSelectedDocumentIsInView
		internal void EnsureSelectedDocumentIsInView()
		{
			this.EnsureSelectedDocumentIsInViewImpl(true);
		}

		private bool EnsureSelectedDocumentIsInViewImpl(bool checkClip)
		{
			if (this.CanRepositionSelectedItem(checkClip))
			{
				try
				{
					this._initializingSelection = true;

					this.MoveItemToFront(this.SelectedIndex);
				}
				finally
				{
					this._initializingSelection = false;
				}

				// now we can reselect the first item
				this.SelectedIndex = 0;
				return true;
			}

			return false;
		}
		#endregion //EnsureSelectedDocumentIsInView

		// AS 5/4/10 TFS31380
		#region FixupFilesMenu
		private void FixupFilesMenu(NotifyCollectionChangedEventArgs e)
		{
			if (_filesMenuItem != null && _filesMenuItem.IsSubmenuOpen == false)
			{
				// the Items of this menu item are dynamically populated when the list 
				// is dropped down. the menu items we create need a strong reference 
				// to the pane they are associated with. so if the items collection 
				// is modified such that an item is removed after the list has been
				// shown, a menu item will exist within this collection that has a 
				// strong reference to the pane and therefore the pane will be rooted.
				// to avoid this we'll just clear the items if the tabgroup's items 
				// change while the menu is not open.
				//
				switch (e.Action)
				{
					case NotifyCollectionChangedAction.Replace:
					case NotifyCollectionChangedAction.Remove:
					case NotifyCollectionChangedAction.Reset:
						// if items are added/removed while the menu is not opened just clear
						// the items. it gets populated when its opened
						_filesMenuItem.Items.Clear();

						// AS 5/11/10 TFS32083
						// We need to add in one item so its role remains TopLevelHeader and 
						// will therefore open its submenu when clicked.
						//
						_filesMenuItem.Items.Add(this.HiddenMenuItem);
						break;
				}
			}
		}
		#endregion //FixupFilesMenu 

		// AS 7/5/11 TFS80723
		#region GetSelectableTabIndex
		private int GetSelectableTabIndex(int startingIndex)
		{
			int itemCount = this.Items.Count;

			// AS 7/6/11 TFS80804
			startingIndex = Math.Max(Math.Min(startingIndex, itemCount - 1), 0);

			for (int i = startingIndex, count = this.Items.Count; i < count; i++)
			{
				FrameworkElement element = this.Items[i] as FrameworkElement;

				Debug.Assert(null != element);

				if (element != null && element.Visibility == Visibility.Visible)
				{
					return i;
				}
			}

			for (int i = startingIndex - 1; i >= 0; i--)
			{
				FrameworkElement element = this.Items[i] as FrameworkElement;

				Debug.Assert(null != element);

				if (element != null && element.Visibility == Visibility.Visible)
				{
					return i;
				}
			}

			return -1;
		}
		#endregion //GetSelectableTabIndex

		#region GetSelectedTabItem
		private TabItem GetSelectedTabItem()
		{
			TabItem tabItem = this.SelectedItem as TabItem;

			if (null == tabItem && this.SelectedIndex >= 0)
				tabItem = this.ItemContainerGenerator.ContainerFromIndex(this.SelectedIndex) as TabItem;

			return tabItem;
		} 
		#endregion //GetSelectedTabItem

		// AS 7/5/11 TFS80723
		#region InitializeSelectedIndex
		private bool InitializeSelectedIndex()
		{
			var dm = XamDockManager.GetDockManager(this);

			if (dm == null || dm.IsLoadingLayout)
				return false;

			int index = -1;
			DateTime lastActivatedDate = DateTime.MinValue;

			// give preference to the pane that was activated most recently...
			for (int i = 0, count = this.Items.Count; i < count; i++)
			{
				ContentPane tempPane = this.Items[i] as ContentPane;

				// ignore collapsed panes
				if (tempPane == null || tempPane.Visibility == Visibility.Collapsed)
					continue;

				DateTime tempActivatedDate = tempPane.LastActivatedTime;

				// if we've found one and it was activated more recently than 
				// this one then skip it
				if (index >= 0 && lastActivatedDate > tempActivatedDate)
					continue;

				index = i;
				lastActivatedDate = tempActivatedDate;
			}

			// if we didn't find one or none have been activated...
			if (index < 0 || lastActivatedDate == DateTime.MinValue)
			{
				// prefer the current selected index
				int startingIndex = this.SelectedIndex;

				// if we don't have one then use the last index we did have
				if (startingIndex < 0)
					startingIndex = _previousSelectedIndex;

				index = this.GetSelectableTabIndex(startingIndex);

				if (index < 0)
					return false;
			}

			this.SelectedIndex = index;
			return true;
		}
		#endregion //InitializeSelectedIndex

		#region MoveItemToFront
		private void MoveItemToFront(int index)
		{
			// AS 7/1/08 BR33677
			Debug.Assert(XamDockManager.GetPaneLocation(this) == PaneLocation.Document);

			Debug.WriteLine("Old Index=" + index.ToString(), "TabGroupPane.MoveItemToFront");

			bool wasMovingItem = this._isMovingItem;

			this._isMovingItem = true;

			Debug.Assert(index >= 0 && index < this.Items.Count);

			this.VerifyItemsSnapshot();

			try
			{
				object selectedItem = this.Items[index];

				this.Items.RemoveAt(index);
				this.Items.Insert(0, selectedItem);
			}
			finally
			{
				this._isMovingItem = wasMovingItem;
			}

			this.UpdateLayout();
		}

		#endregion //MoveItemToFront

		#region OnChildVisibilityChanged
		private static void OnChildVisibilityChanged(object sender, RoutedEventArgs e)
		{
			TabGroupPane group = sender as TabGroupPane;

			if (sender != e.OriginalSource)
			{
				ContentPane pane = e.OriginalSource as ContentPane;

				if (null != pane)
				{
					if (pane == group.SelectedItem)
					{
						
#region Infragistics Source Cleanup (Region)













#endregion // Infragistics Source Cleanup (Region)

						int count = group.Items.Count;
						DateTime latestActivated = DateTime.MinValue;
						int indexToSelect = -1;

						for (int i = 0; i < count; i++)
						{
							ContentPane child = group.Items[i] as ContentPane;

							if (null != child && child.Visibility == Visibility.Visible && child.IsEnabled)
							{
								if (child.LastActivatedTime > latestActivated)
								{
									latestActivated = child.LastActivatedTime;
									indexToSelect = i;
								}
							}
						}

						if (indexToSelect < 0 && count > 0)
						{
							int startingIndex = Math.Max(0, Math.Min(group.SelectedIndex, count - 1));

							for (int i = startingIndex, end = (startingIndex + count - 1); i < end; i++)
							{
								int index = i % count;
								ContentPane child = group.Items[index] as ContentPane;

								if (null != child && child.Visibility == Visibility.Visible && child.IsEnabled)
								{
									indexToSelect = index;
									break;
								}
							}
						}

						if (indexToSelect >= 0)
							group.SelectedIndex = indexToSelect;
					}
				}

				group.VerifyTabAreaVisibility();

				group.CoerceValue(UIElement.VisibilityProperty);
				e.Handled = true;
			}

			DockManagerUtilities.VerifyOwningToolWindow(group);

		}
		#endregion // OnChildVisibilityChanged

		#region OnCanExecuteCommand
		private static void OnCanExecuteCommand(object sender, CanExecuteRoutedEventArgs e)
		{
			TabGroupPane group = (TabGroupPane)sender;

			if (e.Command == CloseSelectedItemCommand)
			{
				ContentPane cp = group.SelectedItem as ContentPane;
				e.CanExecute = cp != null && cp.AllowClose;
				e.Handled = true;
			}
		}
		#endregion //OnCanExecuteCommand

		#region OnExecuteCommand
		private static void OnExecuteCommand(object sender, ExecutedRoutedEventArgs e)
		{
			TabGroupPane group = (TabGroupPane)sender;

			if (e.Command == CloseSelectedItemCommand)
			{
				ContentPane cp = group.SelectedItem as ContentPane;

				if (null != cp)
				{
					e.Handled = cp.ExecuteCommand(ContentPaneCommands.Close);
				}
			}
		}
		#endregion //OnExecuteCommand

		#region OnGeneratorStatusChanged
		private void OnGeneratorStatusChanged(object sender, EventArgs e)
		{
			// AS 5/20/08 BR33082
			// The tabcontrol just chooses the first item when there is no selection and 
			// the generator status switches to ContainersGenerated. We'll catch it before
			// them and choose a tab item that is actually visible. :-)
			// 
			if (this.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
			{
				this.VerifySelectedItem();
			}
		} 
		#endregion //OnGeneratorStatusChanged

		// AS 7/13/09 TFS18399
		#region OnLayoutUpdated
		private void OnLayoutUpdated(object sender, EventArgs e)
		{
			this.LayoutUpdated -= new EventHandler(OnLayoutUpdated);

			if (_isRefreshSelectedTabPeerPending)
			{
				_isRefreshSelectedTabPeerPending = false;

				TabItem ti = this.GetSelectedTabItem();

				if (null != ti)
				{
					AutomationPeer tabPeer = UIElementAutomationPeer.FromElement(ti);

					Debug.Assert(null == tabPeer || tabPeer is TabItemWrapperAutomationPeer);

					// the peer for the tabitem would be a TabItemWrapperAutomationPeer. however, 
					// the peer that the tabcontrol's automation peer returns for its children 
					// is a TabItemAutomationPeer (one for each item in the items collection). the 
					// selected tabitemautomationpeer includes the children of the selectedcontentpresenter
					// as its automation peer children so we need to reset its children
					if (null != tabPeer && null != tabPeer.EventsSource)
					{
						tabPeer.EventsSource.ResetChildrenCache();
					}
				}
			}
		}
		#endregion //OnLayoutUpdated

		#region OnPaneLocationChanged
		private static void OnPaneLocationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((TabGroupPane)d).VerifyTabAreaVisibility();
		} 
		#endregion //OnPaneLocationChanged

		#region OnRequestBringIntoView
		private static void OnRequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
		{
			TabGroupPane tabGroup = sender as TabGroupPane;

			tabGroup.OnRequestBringIntoView(e);
		}

		private void OnRequestBringIntoView(RequestBringIntoViewEventArgs e)
		{
            
#region Infragistics Source Cleanup (Region)










#endregion // Infragistics Source Cleanup (Region)

            DependencyObject item = DockManagerUtilities.GetTabControlChild(e.TargetObject, this);

			if (item != null && item is TabItem == false)
			{
				int index = this.Items.IndexOf(item);

				if (index >= 0)
				{
					// AS 5/15/08 BR32762
					// Do not try to select the tab of a non-visible item.
					//
					FrameworkElement element = item as FrameworkElement;
					Debug.Assert(null == element || element.Visibility == Visibility.Visible, "We should not be trying to make a hidden tab the selected item");

					if (element != null && element.Visibility != Visibility.Visible)
						return;

					TabItem tabItem = this.ItemContainerGenerator.ContainerFromIndex(index) as TabItem;

					// AS 7/7/09 TFS18565
					// Do not try to force items to be generated if the containers for the group have not yet been created.
					// 
					//if (null == tabItem && XamDockManager.GetPaneLocation(this) == PaneLocation.Document)
					if (null == tabItem &&
						this.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated &&
						XamDockManager.GetPaneLocation(this) == PaneLocation.Document)
					{
						// the layout may be dirty so process that first
						this.UpdateLayout();

						tabItem = this.ItemContainerGenerator.ContainerFromIndex(index) as TabItem;

						if (null == tabItem)
						{
							// if the element isn't created then we need to move
							// it in the collection to ensure its in view
							this.MoveItemToFront(index);

							// update its index since it was moved
							index = 0;

							e.Handled = true;
						}
					}

					// then select the tab so its content is in view
					this.SelectedIndex = index;
				}
			}
		}
		#endregion //OnRequestBringIntoView

		// AS 7/5/11 TFS80723
		#region OnSelectedIndexChanged
		private static void OnSelectedIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var tabGroup = d as TabGroupPane;
			int newIndex = (int)e.NewValue;

			if (newIndex >= 0)
				tabGroup._previousSelectedIndex = newIndex;
		} 
		#endregion //OnSelectedIndexChanged

		#region OnSubmenuOpened
		private static void OnSubmenuOpened(object sender, RoutedEventArgs e)
		{
			TabGroupPane groupPane = sender as TabGroupPane;

			if (null != groupPane &&
				groupPane._filesMenuItem == e.OriginalSource)
			{
				ItemCollection items = groupPane._filesMenuItem.Items;
				items.Clear();
				items.Add(groupPane.HiddenMenuItem);

				object[] panes = new object[groupPane.Items.Count];
				groupPane.Items.CopyTo(panes, 0);

				Utilities.SortMergeGeneric<object>(panes, TabHeaderComparer.Instance);

				// create a menu item for each visible pane
				foreach (object item in panes)
				{
					ContentPane pane = item as ContentPane;

					if (pane != null && pane.Visibility != Visibility.Collapsed)
					{
						MenuItem mi = new MenuItem();
						mi.Tag = pane;
						mi.Header = pane.TabHeader;
						mi.HeaderTemplate = pane.TabHeaderTemplate;
						mi.HeaderTemplateSelector = pane.TabHeaderTemplateSelector;

						mi.Command = ContentPaneCommands.ActivatePane;
						mi.CommandTarget = pane;
						// AS 3/25/08
						// There is a bug/behavior in the wpf framework whereby when the commandmanager
						// is walking up the element tree, if the canexecute/execute is invoked for an
						// element that is a focus scope, it will handle the event and raise it for the 
						// focused element of that focus scope (if it has one). This is presumably to
						// handle the case whereby a button/menuitem within a menu/toolbar is associated
						// with a command. The (can)execute is bubbled up to the toolbar/menu and then 
						// they shift it to the focused element within the window - e.g. a richtextbox
						// that has focus in the window. We need the contentpane to be a focusscope but
						// we need the original source so we will use the CommandParameter in addition to
						// the CommandTarget.
						//
						mi.CommandParameter = pane;
						mi.SetValue(DefaultStyleKeyProperty, XamDockManager.MenuItemStyleKey);

						if (pane.HasImage)
						{
							Image image = new AutoDisabledImage();
							image.Source = pane.Image;
							mi.Icon = image;
						}

						items.Add(mi);
					}
				}

				// AS 9/29/09 NA 2010.1 - FilesMenuOpening
				// Raise an event to allow the developer to add/remove items.
				//
				groupPane.RaiseFilesMenuOpening(new FilesMenuOpeningEventArgs(items));
			}
		}
		#endregion //OnSubmenuOpened

		#region OnVisibilityChanged
		private static void OnVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			TabGroupPane group = (TabGroupPane)d;
			DockManagerUtilities.RaiseVisibilityChanged(d, e);
			group.VerifyTabAreaVisibility();

			// the item that was made visible may not be the selected item
			// so verify the selected item
			group.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Infragistics.Windows.DockManager.DockManagerUtilities.MethodInvoker(group.VerifySelectedItem));
		}
		#endregion //OnVisibilityChanged

		// AS 7/13/09 TFS18399
		#region RefreshSelectedTabPeerChildren
		private void RefreshSelectedTabPeerChildren()
		{
			if (_isRefreshSelectedTabPeerPending)
				return;

			_isRefreshSelectedTabPeerPending = true;

			// since the TabItemAutomationPeer needs to get to the children of the 
			// PART_SelectedContentPresenter to include in its children, we need to 
			// wait until the layout is updated to update the automation peer
			this.LayoutUpdated -= new EventHandler(OnLayoutUpdated);
			this.LayoutUpdated += new EventHandler(OnLayoutUpdated);
		}
		#endregion //RefreshSelectedTabPeerChildren

		#region SelectTabItem
		// AS 7/5/11 TFS80723
		//private bool SelectTabItem(int startingIndex)
		//{
		//    for (int i = startingIndex, count = this.Items.Count; i < count; i++)
		//    {
		//        FrameworkElement element = this.Items[i] as FrameworkElement;
		//	
		//        Debug.Assert(null != element);
		//	
		//        if (element != null && element.Visibility == Visibility.Visible)
		//        {
		//            this.SelectedIndex = i;
		//            return true;
		//        }
		//    }
		//	
		//    return false;
		//}
		#endregion //SelectTabItem

		// AS 7/14/09 TFS18400
		#region VerifyActivePanes
		private void VerifyActivePanes()
		{
			ContentPane cp = this.SelectedItem as ContentPane;

			if (null == cp || cp.IsActivating || cp.IsKeyboardFocusWithinEx)
				return;

			XamDockManager dm = XamDockManager.GetDockManager(this);

			// if the dockmanager is closing panes then it will fix up the 
			// active pane/document when it is done
			if (dm != null && dm.IsClosingPanes)
				return;

			// if we contain the active pane then just change the active pane to the newly selected pane
			// AS 1/12/11 TFS61435
			// We don't want to activate a pane if we don't have an active pane currently.
			//
			//if (this.ContainsActivePane)
			if (dm == null)
				return;

			bool containsActivePane = this.ContainsActivePane;

			if (containsActivePane && dm.ActivePane != null)
				cp.ActivateInternal(true, true);
			else if (this.ContainsActiveDocument)
			{
				dm.ActivePaneManager.SetActiveDocument(cp);
			}
			else if (containsActivePane)
			{
				// AS 1/12/11 TFS61435
				// However we need to move it up in the list or else we could activate 
				// the previous selected pane in the group when the dm regains focus.
				//
				dm.ActivePaneManager.MoveToFront(cp);
			}
		}
		#endregion //VerifyActivePanes 

		// AS 7/16/09 TFS18852
		#region VerifyAllItems
		private void VerifyAllItems()
		{
			// AS 9/8/09 TFS19079
			if (!this.CanVerifyItems)
				return;

			foreach (object item in this.Items)
				this.VerifyItem(item);
		} 
		#endregion //VerifyAllItems

		// AS 7/16/09 TFS18852
		#region VerifyItem
		private void VerifyItem(object item)
		{
			if (item is ContentPanePlaceholder == false)
			{
				if (item is ContentPane == false)
				{
					// AS 9/8/09 TFS19079
					if (!this.CanVerifyItems)
						return;

					throw new InvalidOperationException(XamDockManager.GetString("LE_InvalidTabGroupPaneChild"));
				}
			}
		}
		#endregion //VerifyItem

		#region VerifyItemsSnapshot
		/// <summary>
		/// Helper method for building a cached list of the items in the current order.
		/// </summary>
		private void VerifyItemsSnapshot()
		{
			XamDockManager dockManager = XamDockManager.GetDockManager(this);

			Debug.Assert(null != dockManager || this._itemsSnapshot == null, "We can't get to the dockmanager so we may be incorrectly releasing the snapshot.");

			if (null != dockManager
				&& dockManager.ActivePaneManager.IsInNavigationMode)
			{
				int currentNavVersion = dockManager.ActivePaneManager.NavigationModeVersion;

				if (currentNavVersion != this._navigationModeVersion || this._itemsSnapshot == null)
				{
					Debug.WriteLine("Creating snapshot:" + currentNavVersion);

					this._navigationModeVersion = currentNavVersion;

					// before we reorder the items while in navigation mode, we want to 
					// cache the list of tabs in their current order since as tabs are navigated
					// we will be (as we are about to do) changing the order of the collection
					// so we will never be able to navigate to all of the items
					object[] snapshot = new object[this.Items.Count];
					this.Items.CopyTo(snapshot, 0);
					this._itemsSnapshot = snapshot;
				}
			}
			else
			{
				this.ClearItemsSnapshot();
			}
		}
		#endregion // VerifyItemsSnapshot

		#region VerifyNotUsingSnapshot
		[Conditional("DEBUG")]
		private void VerifyNotUsingSnapshot()
		{
			Debug.Assert(this._itemsSnapshot == null);
		} 
		#endregion // VerifyNotUsingSnapshot

		#region VerifySelectedItem
		private void VerifySelectedItem()
		{
			// AS 10/28/10 TFS42895 - See ClearSelectionProperties
			this.VerifySelectedItem(null);
		}

		// AS 10/28/10 TFS42895 - See ClearSelectionProperties
		// Added overload so we know whether this is being called async or not. Only when 
		// we are not called async will we call ClearSelectionProperties to avoid any 
		// possible endless cycle.
		//
		private void VerifySelectedItem(object param)
		{
			// AS 5/14/08 BR32587
			// We should not adjust the selected index until the containers are generated
			// or else we can overwrite a selected index that may have been set (e.g. 
			// during the load layout).
			//
			if (this.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
				return;

			int selectedIndex = this.SelectedIndex;

			TabItem selectedTab = selectedIndex >= 0 ? this.ItemContainerGenerator.ContainerFromIndex(selectedIndex) as TabItem : null;

			if (selectedTab == null)
			{
				// AS 5/20/08 BR32587
				// Since we can reuse elements, the status may be ContainersGenerated even though
				// there are no containers. We really only need to look for a tab item when we
				// are in the documents area and even then not when we are loading from a layout.
				//
				// AS 7/7/09 TFS18565
				// If an item is added/removed we should not automatically change the selected item 
				// even if the group is within the document content host.
				//
				//if (XamDockManager.GetPaneLocation(this) != PaneLocation.Document && selectedIndex >= 0 && selectedIndex < this.Items.Count)
				if (selectedIndex >= 0 && selectedIndex < this.Items.Count)
				{
					FrameworkElement item = this.Items[selectedIndex] as FrameworkElement;

					if (null != item && item.Visibility == Visibility.Visible)
						return;
				}

				// find the first visible tab item to select
				// AS 10/13/10 TFS42895
				// In CLR4, MS switched to using the new SetCurrentValue instead of setting a local value. 
				// Well the underlying/previous value is still strongly referenced by the WPF framework 
				// so that object can be rooted. To get around this (since we don't know that this is 
				// being done) is to always explicitly clear (which will set a local value for) the 
				// selecteditem and selectedvalue.
				//
				//this.SelectTabItem(0);
				// AS 7/5/11 TFS80723
				//if (!this.SelectTabItem(0))
				if (!this.InitializeSelectedIndex())
				{
					// AS 10/28/10 TFS42895 - See ClearSelectionProperties
					//this.SelectedItem = null;
					//this.SelectedValue = null;
					if (param == null)
						this.ClearSelectionProperties();
				}
			}
			else if (selectedTab.Visibility == Visibility.Collapsed)
			{
				// find the next/previous visible tab item to select
				// AS 7/5/11 TFS80723
				//if (false == this.SelectTabItem(selectedIndex + 1))
				if (false == this.InitializeSelectedIndex())
				{
					// AS 10/13/10 TFS42895 - See above
					//this.SelectTabItem(0);
					// AS 7/5/11 TFS80723
					//if (!this.SelectTabItem(0))
					{
						// AS 10/28/10 TFS42895 - See ClearSelectionProperties
						//this.SelectedItem = null;
						//this.SelectedValue = null;
						if (param == null)
							this.ClearSelectionProperties();
					}
				}
			}
		} 
		#endregion //VerifySelectedItem

		#region VerifySelectedContent
		private void VerifySelectedContent()
		{
			// AS 10/18/07
			// We need to make sure that the SelectedContent has been updated. Unfortunately the only way to 
			// do this is to call a method that will call the UpdateSelectedContent method of the tabcontrol
			// since that is protected. The least intrusive way is to call OnApplyTemplate since they 
			// only do that within that method.
			//
			object selectedObject = this.SelectedItem;

			TabItem tabItem = selectedObject as TabItem;

			if (tabItem == null && selectedObject != null)
				tabItem = this.ItemContainerGenerator.ContainerFromItem(selectedObject) as TabItem;

			if (null != tabItem && this.SelectedContent != tabItem.Content)
				base.OnApplyTemplate();
		}
		#endregion //VerifySelectedContent

		#region VerifyTabAreaVisibility
		private void VerifyTabAreaVisibility()
		{
			int visibleItemCount = 0;

			for (int i = 0, count = this.Items.Count; i < count; i++)
			{
				UIElement child = this.Items[i] as UIElement;

				// AS 9/8/09 TFS19079
				//if (child.Visibility == Visibility.Visible)
				if (null != child && child.Visibility == Visibility.Visible)
				{
					visibleItemCount++;

					if (visibleItemCount > 1)
						break;
				}
			}


			bool hideTabItemArea = false;

			// update a new dep property that we bind to in the template
			if (visibleItemCount == 1)
			{
				switch (XamDockManager.GetPaneLocation(this))
				{
					// AS 5/14/08
					// Noticed this while working on BR32568. Since you can have groups
					// in a floating only window, we should also allow hiding the tab item
					// area there when there is only 1 pane. This indirectly gets around
					// the bug too but the null reference was a missing null check in
					// PaneToolWindow.
					//
					case PaneLocation.FloatingOnly:

					case PaneLocation.DockedBottom:
					case PaneLocation.DockedLeft:
					case PaneLocation.DockedRight:
					case PaneLocation.DockedTop:
					case PaneLocation.Floating:
						hideTabItemArea = true;
						break;
				}
			}

			this.SetValue(IsTabItemAreaVisiblePropertyKey, hideTabItemArea ? KnownBoxes.FalseBox : DependencyProperty.UnsetValue);
		} 
		#endregion // VerifyTabAreaVisibility

		#endregion //Private Methods

		#endregion //Methods

		#region Events

		// AS 9/29/09 NA 2010.1 - FilesMenuOpening
		#region FilesMenuOpening

		/// <summary>
		/// Event ID for the <see cref="FilesMenuOpening"/> routed event
		/// </summary>
		/// <seealso cref="FilesMenuOpening"/>
		/// <seealso cref="OnFilesMenuOpening"/>
		/// <seealso cref="FilesMenuOpeningEventArgs"/>
		[InfragisticsFeature(Version = FeatureInfo.Version_10_1, FeatureName = FeatureInfo.FeatureName_MiscDockManagerFeatures_10_1)]
		public static readonly RoutedEvent FilesMenuOpeningEvent =
			EventManager.RegisterRoutedEvent("FilesMenuOpening", RoutingStrategy.Bubble, typeof(EventHandler<FilesMenuOpeningEventArgs>), typeof(TabGroupPane));

		/// <summary>
		/// Occurs when the files menu for the <see cref="TabGroupPane"/> is about to be displayed
		/// </summary>
		/// <seealso cref="FilesMenuOpening"/>
		/// <seealso cref="FilesMenuOpeningEvent"/>
		/// <seealso cref="FilesMenuOpeningEventArgs"/>
		[InfragisticsFeature(Version = FeatureInfo.Version_10_1, FeatureName = FeatureInfo.FeatureName_MiscDockManagerFeatures_10_1)]
		protected virtual void OnFilesMenuOpening(FilesMenuOpeningEventArgs args)
		{
			this.RaiseEvent(args);
		}

		internal void RaiseFilesMenuOpening(FilesMenuOpeningEventArgs args)
		{
			args.RoutedEvent = TabGroupPane.FilesMenuOpeningEvent;
			args.Source = this;
			this.OnFilesMenuOpening(args);
		}

		/// <summary>
		/// Occurs when the files menu for the <see cref="TabGroupPane"/> is about to be displayed
		/// </summary>
		/// <seealso cref="OnFilesMenuOpening"/>
		/// <seealso cref="FilesMenuOpeningEvent"/>
		/// <seealso cref="FilesMenuOpeningEventArgs"/>
		//[Description("Occurs when the files menu for the 'TabGroupPane' is about to be displayed")]
		//[Category("DockManager Events")] // Behavior
		[InfragisticsFeature(Version = FeatureInfo.Version_10_1, FeatureName = FeatureInfo.FeatureName_MiscDockManagerFeatures_10_1)]
		public event EventHandler<FilesMenuOpeningEventArgs> FilesMenuOpening
		{
			add
			{
				base.AddHandler(TabGroupPane.FilesMenuOpeningEvent, value);
			}
			remove
			{
				base.RemoveHandler(TabGroupPane.FilesMenuOpeningEvent, value);
			}
		}

		#endregion //FilesMenuOpening

		#endregion //Events

		#region TabHeaderComparer
		private class TabHeaderComparer : IComparer<object>
		{
			#region Member Variables

			internal static readonly IComparer<object> Instance = new TabHeaderComparer();

			#endregion //Member Variables

			#region Constructor
			private TabHeaderComparer()
			{
			}
			#endregion //Constructor

			#region IComparer<object> Members

			int IComparer<object>.Compare(object x, object y)
			{
				ContentPane paneX = x as ContentPane;
				ContentPane paneY = y as ContentPane;

				if (null == paneX && null == paneY)
					return 0;
				else if (null == paneX)
					return -1;
				else if (null == paneY)
					return 1;

				object headerX = paneX.TabHeader;
				object headerY = paneY.TabHeader;

				if (headerX == null && headerY == null)
					return 0;
				else if (headerX == null)
					return -1;
				else if (headerY == null)
					return 1;

				if (headerX.GetType() == headerY.GetType())
				{
					IComparable comparableX = headerX as IComparable;

					if (null != comparableX)
						comparableX.CompareTo(headerY);
				}

				string headerStringX = headerX.ToString();
				string headerStringY = headerY.ToString();

				return string.Compare(headerStringX, headerStringY, StringComparison.Ordinal);
			}

			#endregion
		} 
		#endregion //TabHeaderComparer

		#region IContentPaneContainer Members

		FrameworkElement IContentPaneContainer.ContainerElement
		{
			get { return this; }
		}

		PaneLocation IContentPaneContainer.PaneLocation
		{
			get { return XamDockManager.GetPaneLocation(this); }
		}

		void IContentPaneContainer.RemoveContentPane(ContentPane pane, bool replaceWithPlaceholder)
		{
			int index = DockManagerUtilities.IndexOf(this.Items, pane, true);

			Debug.Assert(index >= 0, "The pane does not exist within this pane!");

			if (index >= 0)
			{
				object element = this.Items[index];

				// if we already have a placeholder for the element...
				if (element is ContentPanePlaceholder && replaceWithPlaceholder)
				{
					Debug.Fail("Why are we getting here when we already have a placeholder?");
					return;
				}

				// AS 4/28/11 TFS73532
				// When we call Items.RemoveAt the base ItemsControl impl will remove the item 
				// from the logical tree but if it is the selected item then it will subsequently 
				// remove it from the visual tree. However when the item is removed from the visual 
				// tree after it is removed from the logical tree (i.e. when it has no logical parent) 
				// the wpf framework does another traversal of the element tree. So if the item 
				// being removed is the selected item we will just clear the selection so it is 
				// removed from the visual tree first.
				//
				if (index == this.SelectedIndex)
					this.SelectedIndex = -1;

				this.Items.RemoveAt(index);

				if (replaceWithPlaceholder)
				{
					Debug.Assert(DockManagerUtilities.NeedsPlaceholder(XamDockManager.GetPaneLocation(this), PaneLocation.Unknown));

					ContentPanePlaceholder placeholder = new ContentPanePlaceholder();
					placeholder.Initialize(pane);
					this.Items.Insert(index, placeholder);

					// cache the current relative size on the placeholder so we can maintain the old
					// relative size when we restore the pane
					if (pane.ReadLocalValue(SplitPane.RelativeSizeProperty) != DependencyProperty.UnsetValue)
						placeholder.SetValue(SplitPane.RelativeSizeProperty, pane.ReadLocalValue(SplitPane.RelativeSizeProperty));

					pane.PlacementInfo.StorePlaceholder(placeholder);
				}
				else if (element is ContentPanePlaceholder)
				{
					// if we're removing the placeholder then remove it from the placement info
					pane.PlacementInfo.RemovePlaceholder((ContentPanePlaceholder)element);
				}


				// AS 4/8/08 BR31848
				// I actually noticed this as well but was holding off on dealing with it. Basically
				// the tab control doesn't care about the visibility of an item with regards to whether
				// it should be selected. The placeholder was taking the place of the pane and if that 
				// pane was the selected item then the placeholder was remaining as the selected item
				// but since it is collapsed, you didn't see anything.
				//
				this.VerifySelectedItem();

				this.VerifySelectedContent();
			}
		}

		void IContentPaneContainer.InsertContentPane(int? newIndex, ContentPane pane)
		{
			// find the placeholder
			int placeholderIndex = DockManagerUtilities.IndexOf(this.Items, pane, true);

			bool wasSelected = false;

			if (placeholderIndex >= 0)
			{
				Debug.Assert(this.Items[placeholderIndex] is ContentPanePlaceholder);

				if (this.Items[placeholderIndex] is ContentPane)
					return;

				wasSelected = placeholderIndex == this.SelectedIndex;

				ContentPanePlaceholder placeholder = this.Items[placeholderIndex] as ContentPanePlaceholder;

				// remove the placeholder
				this.Items.RemoveAt(placeholderIndex);

				// update the placement info
				pane.PlacementInfo.RemovePlaceholder(placeholder);

				// restore the relative size for the pane when it was in this pane
				pane.SetValue(SplitPane.RelativeSizeProperty, placeholder.ReadLocalValue(SplitPane.RelativeSizeProperty));
			}
			else // add it to the end
				placeholderIndex = this.Items.Count;

			if (newIndex != null)
				placeholderIndex = Math.Max(0, Math.Min(this.Items.Count, newIndex.Value));

			this.Items.Insert(placeholderIndex, pane);

			if (wasSelected)
				this.SelectedIndex = placeholderIndex;
		}

		IList<ContentPane> IContentPaneContainer.GetVisiblePanes()
		{
			this.VerifyNotUsingSnapshot();

			return DockManagerUtilities.CreateVisiblePaneList(this.Items);
		}

		IList<ContentPane> IContentPaneContainer.GetAllPanesForPaneAction(ContentPane pane)
		{
			if (XamDockManager.GetPaneLocation(this) == PaneLocation.Document)
				return new ContentPane[] { pane };

			Debug.Assert(pane != null && this.Items.IndexOf(pane) >= 0);
			return ((IContentPaneContainer)this).GetVisiblePanes();
		}
		#endregion //IContentPaneContainer

		#region IPaneContainer Members

		IList IPaneContainer.Panes
		{
			get 
			{ 
				return this.CurrentItemsCollection; 
			}
		}

		bool IPaneContainer.RemovePane(object pane)
		{
			int index = this.Items.IndexOf(pane);

			if (index >= 0)
			{
				this.Items.RemoveAt(index);
			}

			return index >= 0;
		}

		bool IPaneContainer.CanBeRemoved
		{
			// AS 5/17/08 BR32346
			//get { return this.Items.Count == 0; }
			get { return this.Items.Count == 0 && DockManagerUtilities.ShouldPreventPaneRemoval(this) == false; }
		}

		#endregion //IPaneContainer
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