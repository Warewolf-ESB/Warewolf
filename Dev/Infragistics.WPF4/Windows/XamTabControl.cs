using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.ComponentModel;
using Infragistics.Windows.Helpers;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Data;
using Infragistics.Windows.Internal;
using System.Diagnostics;
using System.Windows.Media;
using Infragistics.Windows.Controls.Events;
using System.Windows.Documents;
using Infragistics.Windows.Themes;
using Infragistics.Windows.Commands;
using Infragistics.Windows.Automation.Peers;
using System.Windows.Automation.Peers;

namespace Infragistics.Windows.Controls
{
	/// <summary>
	/// A custom tab control with multiple layout styles and tab item scrolling functionality.
	/// </summary>
	[TemplatePart(Name = "PART_TabItemScrollViewer", Type=typeof(ScrollViewer))]
	[TemplatePart(Name = "PART_Popup", Type = typeof(Popup))]
	[TemplatePart(Name = "PART_SelectedContentHost", Type = typeof(ContentPresenter))]
	[TemplatePart(Name = "PART_SelectedContentHostMinimized", Type = typeof(ContentPresenter))]
	[TemplatePart(Name = "PART_HeaderArea", Type = typeof(FrameworkElement))] // AS 11/27/07 BR28724
		// AS 11/7/07 BR21903
		// AS 11/7/07 BR21903
    public class XamTabControl : TabControl, ICommandHost
	{
		#region Member Variables

		private object _previouslySelectedItem = null;
		private ScrollViewer _tabItemScrollViewer;
		private Popup _popup;
		private int		_lastSingleLeftMouseDownTime;
		private Point	_lastSingleLeftMouseDownPoint;

		// AS 11/7/07 BR21903
		private Infragistics.Windows.Licensing.UltraLicense _license;

		// AS 11/12/07 BR28406
		private ContentPresenter _cpMaximized;
		private ContentPresenter _cpMinimized;

		// AS 11/27/07 BR28724
		private PopupPlacementAdorner _popupPlacementAdorner;
		private static readonly object NotInitialized = new object();
		private object _oldPlacementTarget = NotInitialized;

		// AS 7/1/08 BR33755/BR33686
		private int _suppressSelectionChangeCount = 0;
		private bool _initializingSelection;

        // JJD 9/25/08 - added
        private bool _eatNextMouseLeftButtonUp;

		// AS 7/13/09 TFS18399
		private bool _isRefreshSelectedTabPeerPending = false;

		#endregion //Member Variables

		#region Constructor
		static XamTabControl()
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(XamTabControl), new FrameworkPropertyMetadata(typeof(XamTabControl)));
            
            // JJD 8/1/08
            // register the groupings that should be applied when the theme property is changed
            ThemeManager.RegisterGroupings(typeof(XamTabControl), new string[] { PrimitivesGeneric.Location.Grouping });

			TabControl.TabStripPlacementProperty.OverrideMetadata(typeof(XamTabControl), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnTabStripPlacementChanged)));

			FrameworkElementFactory fef = new FrameworkElementFactory(typeof(TabItemPanel));
			RelativeSource relativeSourceTab = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(XamTabControl), 1);
			fef.SetBinding(XamTabControl.InterTabSpacingProperty, Utilities.CreateBindingObject(XamTabControl.InterTabSpacingProperty, BindingMode.OneWay, relativeSourceTab));
			fef.SetBinding(XamTabControl.TabLayoutStyleProperty, Utilities.CreateBindingObject(XamTabControl.TabLayoutStyleProperty, BindingMode.OneWay, relativeSourceTab));
			fef.SetBinding(XamTabControl.MaximumSizeToFitAdjustmentProperty, Utilities.CreateBindingObject(XamTabControl.MaximumSizeToFitAdjustmentProperty, BindingMode.OneWay, relativeSourceTab));
			fef.SetBinding(XamTabControl.MinimumTabExtentProperty, Utilities.CreateBindingObject(XamTabControl.MinimumTabExtentProperty, BindingMode.OneWay, relativeSourceTab));
			fef.SetBinding(XamTabControl.TabStripPlacementProperty, Utilities.CreateBindingObject(XamTabControl.TabStripPlacementProperty, BindingMode.OneWay, relativeSourceTab));
			fef.SetValue(Panel.IsItemsHostProperty, KnownBoxes.TrueBox);

            fef.SetBinding(XamTabControl.InterRowSpacingProperty, Utilities.CreateBindingObject(XamTabControl.InterRowSpacingProperty, BindingMode.OneWay, relativeSourceTab));
            fef.SetBinding(XamTabControl.MaximumTabRowsProperty, Utilities.CreateBindingObject(XamTabControl.MaximumTabRowsProperty, BindingMode.OneWay, relativeSourceTab));

			ItemsPanelTemplate template = new ItemsPanelTemplate(fef);
			template.Seal();

			ItemsControl.ItemsPanelProperty.OverrideMetadata(typeof(XamTabControl), new FrameworkPropertyMetadata(template));
			
			// AS 10/12/07
			// The previous behavior would allow an unminimize when it was collapsed but if it was dropped down then you
			// could not unminimize it. This was because we were storing the left button down time when we were dropped down.
			//
			//EventManager.RegisterClassHandler(typeof(Popup), Mouse.PreviewMouseDownEvent, new MouseButtonEventHandler(OnPopupPreviewMouseDown_ClassHandler));

			EventManager.RegisterClassHandler(typeof(Popup), Mouse.PreviewMouseUpEvent, new MouseButtonEventHandler(OnPopupPreviewMouseUp_ClassHandler));
		}

		/// <summary>
		/// Initializes a new <see cref="XamTabControl"/>
		/// </summary>
		public XamTabControl()
		{
			// AS 11/7/07 BR21903
			try
			{
				// We need to pass our type into the method since we do not want to pass in 
				// the derived type.
				this._license = LicenseManager.Validate(typeof(XamTabControl), this) as Infragistics.Windows.Licensing.UltraLicense;
			}
			catch (System.IO.FileNotFoundException) { }

			this.VerifyScrollBarVisibility();

			// AS 11/27/07 BR28724
			// We need to expose a custompopuplacement callback. The delegate has to be an instance method because it utilizes
			// some information calculated when its dropped down.
			//
			this.SetValue(XamTabControl.PreferredDropDownPlacementCallbackPropertyKey, new CustomPopupPlacementCallback(this.GetPreferredPopupPlacement));
		}
		#endregion //Constructor

        #region Base class overrides

        #region OnApplyTemplate
        /// <summary>
		/// Invoked when the template is applied to the control.
		/// </summary>
		public override void OnApplyTemplate()
		{
			// AS 11/12/07 BR28406
			// This is the heart of the problem. The old content presenter must have been around 
			// long enough to cause problems with the inherited properties. Presumably because the 
			// new elements were reparented into the old content presenter.
			//
			if (this._cpMinimized != null)
				this._cpMinimized.ClearValue(ContentPresenter.ContentProperty);

			if (this._cpMaximized != null)
				this._cpMaximized.ClearValue(ContentPresenter.ContentProperty);

			base.OnApplyTemplate();

			// AS 11/12/07 BR28406
			this._cpMinimized = this.GetTemplateChild("PART_SelectedContentHostMinimized") as ContentPresenter;
			this._cpMaximized = this.GetTemplateChild("PART_SelectedContentHost") as ContentPresenter;

			this._tabItemScrollViewer = GetTemplateChild("PART_TabItemScrollViewer") as ScrollViewer;

			// AS 11/27/07 BR28724
			if (null != this._popup)
				this._popup.Closed -= new EventHandler(this.OnPopupClosed);

			this._popup = GetTemplateChild("PART_Popup") as Popup;

			// AS 11/27/07 BR28724
			// We need to hook the closed to restore the popups original settings.
			//
			if (null != this._popup)
				this._popup.Closed += new EventHandler(this.OnPopupClosed);

			if (null != this._tabItemScrollViewer)
			{
				this.SetBinding(XamTabControl.ComputedTabItemHorizontalScrollBarVisibilityProperty, Utilities.CreateBindingObject(ScrollViewer.ComputedHorizontalScrollBarVisibilityProperty, BindingMode.OneWay, this._tabItemScrollViewer));
				this.SetBinding(XamTabControl.ComputedTabItemVerticalScrollBarVisibilityProperty, Utilities.CreateBindingObject(ScrollViewer.ComputedVerticalScrollBarVisibilityProperty, BindingMode.OneWay, this._tabItemScrollViewer));
			}
			else
			{
				BindingOperations.ClearBinding(this, XamTabControl.ComputedTabItemHorizontalScrollBarVisibilityProperty);
				BindingOperations.ClearBinding(this, XamTabControl.ComputedTabItemVerticalScrollBarVisibilityProperty);
			}

			// AS 6/29/12 TFS114953
			// Expose the tab header height so the ribbon can bring the glass into the area.
			//
			var header = this.GetTemplateChild( "PART_HeaderArea" ) as FrameworkElement;

			if ( null != header )
				this.SetBinding( TabHeaderHeightProperty, new Binding( "ActualHeight" ) { Source = header } );
			else
				BindingOperations.ClearBinding( this, TabHeaderHeightProperty );

			// AS 11/26/07 BR28697
			// Moved the selected content if the ribbon is minimized.
			//
			if (this.IsMinimized)
				this.VerifyMinimizedContentBinding();
		}

		#endregion //OnApplyTemplate

        #region OnCreateAutomationPeer

        // JJD 8/22/08 - added automation support for XamTabControl's ExpandCollapse pattern
        /// <summary>
        /// Called to create an automation peer.
        /// </summary>
        /// <returns></returns>
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new XamTabControlAutomationPeer(this);
        }

        #endregion //OnCreateAutomationPeer	
    
		#region OnInitialized
		/// <summary>
		/// Invoked after the element has been initialized.
		/// </summary>
		/// <param name="e">Provides information for the event.</param>
		protected override void OnInitialized(EventArgs e)
		{
			Debug.Assert(this.ItemContainerGenerator != null);

			// AS 7/1/08 BR33755/BR33686
			if (null != this.ItemContainerGenerator)
				this.ItemContainerGenerator.StatusChanged += new EventHandler(OnGeneratorStatusChanged);

			base.OnInitialized(e);
		}
		#endregion //OnInitialized

        // JJD 8/5/08 - added
        #region OnKeyDown

        /// <summary>
        /// Helper method for processing a keydown.
        /// </summary>
        /// <param name="e">Provides information about the keyboard event</param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            // Pass this key along to our commands class which will check to see if a command
            // needs to be executed.  If so, the commands class will execute the command
            // return true.
            if (e.Handled == false &&
                TabControlCommands.Instance.ProcessKeyboardInput(e, this) == true)
                e.Handled = true;
        }

        #endregion //OnKeyDown	

		#region OnMouseDoubleClick
		/// <summary>
		/// Invoked when the mouse has been double clicked within the tab control.
		/// </summary>
		/// <param name="e">Provides information about the event arguments</param>
		protected override void OnMouseDoubleClick(System.Windows.Input.MouseButtonEventArgs e)
		{
			base.OnMouseDoubleClick(e);

			
#region Infragistics Source Cleanup (Region)












#endregion // Infragistics Source Cleanup (Region)

		}
		#endregion //OnMouseDoubleClick

		#region GetContainerForItemOverride
		/// <summary>
		/// Returns an instance of element used to display an item within the tab control.
		/// </summary>
		/// <returns>An instance of the <see cref="TabItemEx"/> class</returns>
		protected override DependencyObject GetContainerForItemOverride()
		{
			// AS 10/18/07
			// Added a derived tab item type for the xamtabcontrol and moved some members
			// from the ribbontabitem.
			//
			return new TabItemEx();
		} 
		#endregion //GetContainerForItemOverride

		#region GetTabItem
		
#region Infragistics Source Cleanup (Region)




















#endregion // Infragistics Source Cleanup (Region)

		// AS 2/3/10 TFS26270
		// Take the MouseEventArgs instead so we can do additional hittesting.
		//
		//private TabItem GetTabItem(Point point)
		private TabItem GetTabItem(MouseEventArgs mouseArgs)
		{
			// AS 2/3/10 TFS26270
			Point point = mouseArgs.GetPosition(this);

			DependencyObject elementUnderMouse = this.InputHitTest(point) as DependencyObject;

			if (elementUnderMouse == null)
				return null;

            // AS 2/5/09 TFS13045
			//return Utilities.GetAncestorFromType(elementUnderMouse, typeof(TabItem), true, this) as TabItem;
			TabItem tab = Utilities.GetAncestorFromType(elementUnderMouse, typeof(TabItem), true, this) as TabItem;

            if (null != tab)
            {
                int index = this.ItemContainerGenerator.IndexFromContainer(tab);

                if (index < 0)
                    tab = null;
            }

			// AS 2/3/10 TFS26270
			if (null != tab)
			{
				DependencyObject source = mouseArgs.OriginalSource as DependencyObject;

				while (source is ContentElement)
					source = LogicalTreeHelper.GetParent(source);

				UIElement sourceElement = source as UIElement;

				// if the mouse is within the target element then we should not do anything
				// unless it is an ancestor of the tab we're going to include
				if (null != sourceElement &&
					sourceElement.InputHitTest(mouseArgs.GetPosition(sourceElement)) != null &&
					tab.IsAncestorOf(sourceElement) == false)
				{
					return null;
				}
			}

            return tab;
		}
		#endregion //GetTabItem

		#region OnPreviewMouseLeftButtonDown
		/// <summary>
		/// Invoked when the mouse is being pressed down on an element within the control.
		/// </summary>
		/// <param name="e">Provides information about the mouse event.</param>
		protected override void OnPreviewMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
		{
			#region Refactored
			
#region Infragistics Source Cleanup (Region)











































#endregion // Infragistics Source Cleanup (Region)

			#endregion //Refactored

			Point point = e.GetPosition(this);
			// AS 2/3/10 TFS26270
			//TabItem tab = this.GetTabItem(point);
			TabItem tab = this.GetTabItem(e);

            // JJD 9/25/08 
            // clear the eat next mouse up flag
            this._eatNextMouseLeftButtonUp = false;
            
            // JJD 8/8/08
            // If the tab is focusable then we don't want to eat the message 
            bool letMouseMsgProcess = (tab != null && tab.Focusable);

			if (null != tab)
			{
                // JJD 2/19/09 - TFS8466
                // See if we are over a button inside the tab.
                DependencyObject elementUnderMouse = this.InputHitTest(point) as DependencyObject;

                if (elementUnderMouse != null)
                {
                    ButtonBase btn = elementUnderMouse as ButtonBase;

                    if (btn == null)
                         btn = Utilities.GetAncestorFromType(elementUnderMouse, typeof(ButtonBase), true, tab) as ButtonBase;

                    // JJD 2/19/09 - TFS8466
                    // if we are over a button and it is enabled then don't process the mouse down
                    if (btn != null && btn.IsEnabled)
                    {
                        base.OnPreviewMouseLeftButtonDown(e);
                        return;
                    }
                }

				bool processDoubleClick = false;

				// AS 11/9/07 BR28367
				// Cache whether the tab was selected before doing anything.
				//
				bool isTabSelected = tab != null && tab.IsSelected;

				// get the tab item under the mouse
				int doubleClickTime = NativeWindowMethods.DoubleClickTime;
				Size doubleClickSize = NativeWindowMethods.DoubleClickSize;
				double deltaX = Math.Abs(point.X - this._lastSingleLeftMouseDownPoint.X);
				double deltaY = Math.Abs(point.Y - this._lastSingleLeftMouseDownPoint.Y);

				if (e.Timestamp - this._lastSingleLeftMouseDownTime < doubleClickTime &&
					deltaX <= doubleClickSize.Width &&
					deltaY <= doubleClickSize.Height)
				{
					processDoubleClick = true;
				}
				else
				{
					// do not consider the first mouse click if we are not minimized and the
					// mouse is pressed down on another tab item
					if (isTabSelected || this.IsMinimized)
					{
						// on a single click we save the time and point and
						// then toggle the dropped down state
						this._lastSingleLeftMouseDownTime = e.Timestamp;
						this._lastSingleLeftMouseDownPoint = point;
					}

					// AS 11/9/07 BR28367
					// If it needs to dropdown, the act of setting the selected tab will handle that. Otherwise
					// it will dropdown with the previously selected tab and then the selected tab changes.
					//
					//this.IsDropDownOpen = false == this.IsDropDownOpen;

                    // JJD 8/8/08
                    // If the tab is focusable and not yet focused then we don't want to eat the message 
                    if (!letMouseMsgProcess)
                        e.Handled = true;

					// AS 11/9/07 BR28367
					// If we're open and you click on the selected tab or an empty area, then close the dropdown.
					//
					if (this.IsDropDownOpen && (isTabSelected || tab == null))
					{
						this.IsDropDownOpen = false;
						return;
					}
				}

				// if the tab is not focusable, it will not try to select itself
				// when the mouse button is pressed down on it. i would have handled
				// this in the OnMouseLeftButtonDown but the TabItem is still marking
				// the event as handled
				// AS 11/13/07 BR28417
				// If we mark it handled and do not select it here, it won't get selected.
				//
				//if (tab.Focusable == false)
				if (true)
				{
					// AS 11/9/07 BR28367
					// If the tab was selected then don't reselect it since we might have closed
					// it above.
					// 
                    if (isTabSelected == false)
                    {
                        // JJD 9/25/08
                        // make sure we eat this message if we are minimized as well
                        // as eating the next mouse up. 
                        // Otherwise the popup's mouse handling logic will close
                        // the popup on the next mouse up if the tab is moved as a result
                        // of its being selected below and now the mouse is no longer over 
                        // the tab, e.g. in multi row scenarios
                        if (this.IsMinimized)
                        {
                            letMouseMsgProcess = false;
                            this._eatNextMouseLeftButtonUp = true;
                        }

						// AS 5/10/12 TFS111333
						//tab.IsSelected = true;
						DependencyPropertyUtilities.SetCurrentValue(tab, TabItem.IsSelectedProperty, KnownBoxes.TrueBox);
					}

					// AS 11/5/07
					// The tab needs to be brought into view.
					//
					tab.BringIntoView();

                    // JJD 8/8/08
                    // If the tab is focusable and not yet focused then we don't want to eat the message 
                    if (!letMouseMsgProcess)
                        e.Handled = true;
				}

				if (processDoubleClick && this.AllowMinimize)
				{
					// if double clicking on our tab item then toggle the minimized state
					this.IsMinimized = !this.IsMinimized;
					e.Handled = true;
				}

                // JJD 8/8/08
                // Call the base if we aren't eating the message
                if (!e.Handled && letMouseMsgProcess)
                    base.OnPreviewMouseLeftButtonDown(e);
            }
		}
		#endregion //OnPreviewMouseLeftButtonDown

        // JJD 9/25/08 - added
        #region OnPreviewMouseLeftButtonUp

        /// <summary>
        /// Invoked when the mouse is release on an element within the control.
        /// </summary>
        /// <param name="e">Provides information about the mouse event.</param>
        protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            // Eat the message if the flag is set
            if (this._eatNextMouseLeftButtonUp)
            {
                this._eatNextMouseLeftButtonUp = false;
                e.Handled = true;
                return;
            }

            base.OnPreviewMouseLeftButtonUp(e);
        }

        #endregion //OnPreviewMouseLeftButtonUp	

		#region OnPropertyChanged
		/// <summary>
		/// Invoked when a property for the element has been changed.
		/// </summary>
		/// <param name="e">Provides information about the property change</param>
		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			// AS 11/14/07 BR28406
			// We cannot bind to the property so update it as needed. Note, we need to do it before
			// calling the base. Also, using a PropertyChangedCallback in overriden metadata is too
			// late as well.
			//
			if (e.Property == Control.TemplateProperty)
			{
				if (this._cpMinimized != null)
					this._cpMinimized.ClearValue(ContentControl.ContentProperty);

				if (this._cpMaximized != null)
					this._cpMaximized.ClearValue(ContentControl.ContentProperty);
			}

			base.OnPropertyChanged(e);
		} 
		#endregion //OnPropertyChanged

		#region OnSelectionChanged
		/// <summary>
		/// Invoked when the selected tab has been changed.
		/// </summary>
		/// <param name="e">Provides information about the selection change.</param>
		protected override void OnSelectionChanged(SelectionChangedEventArgs e)
		{
			// AS 7/1/08 BR33755/BR33686
			if (this._initializingSelection)
				return;

			// when a tab is selected while minimized, we should drop down the popup
			if (e.AddedItems.Count == 1 && this.IsMinimized)
			{
				// AS 7/1/08 BR33755/BR33686
				if (this._suppressSelectionChangeCount > 0)
				{
					this._initializingSelection = true;

					try
					{
						this.SelectedIndex = -1;
					}
					finally
					{
						this._initializingSelection = false;
					}

					return;
				}

				// AS 11/27/07 BR28724
				bool wasDropDownOpen = this.IsDropDownOpen;

				this.IsDropDownOpen = true;

				// AS 11/27/07 BR28724
				// If the user is selecting a different tab while we are minimized then we want to
				// recalculate the positioning info.
				//
				if (wasDropDownOpen)
					this.InitializeDropdownLocationInfo();
			}

			// AS 7/1/08 BR33755/BR33686
			Debug.Assert(this._suppressSelectionChangeCount == 0);

            // JJD 10/21/08
            // Bring the new selected tab into view
            if (this.IsInitialized &&
                e.AddedItems.Count == 1)
            {
                TabItem selectedTab = this.GetSelectedTabItem();

                if (selectedTab != null)
                    selectedTab.BringIntoView();
            }

			base.OnSelectionChanged(e);

			// AS 7/13/09 TFS18399
			this.RefreshSelectedTabPeerChildren();
		}
		#endregion //OnSelectionChanged

		#endregion //Base class overrides

		#region Properties

		#region Internal

		#region HasVerticalTabs
		internal bool HasVerticalTabs
		{
			get { return this.TabStripPlacement == Dock.Left || this.TabStripPlacement == Dock.Right; }
		}
		#endregion //HasVerticalTabs

		#region IsMultiRow

		private static readonly DependencyPropertyKey IsMultiRowPropertyKey =
			DependencyProperty.RegisterReadOnly("IsMultiRow",
			typeof(bool), typeof(XamTabControl), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnIsMultiRowChanged)));

		private static void OnIsMultiRowChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			XamTabControl tab = d as XamTabControl;

			if (null != tab)
				tab.VerifyScrollBarVisibility();
		}

		/// <summary>
		/// Identifies the <see cref="IsMultiRow"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsMultiRowProperty =
			IsMultiRowPropertyKey.DependencyProperty;

		/// <summary>
		/// Indicates if the <see cref="TabLayoutStyle"/> is one that would arrange the tab items in multiple rows.
		/// </summary>
		/// <seealso cref="IsMultiRowProperty"/>
		//[Description("Indicates if the 'TabLayoutStyle' is one that would arrange the tab items in multiple rows.")]
		//[Category("Behavior")]
		[Bindable(true)]
		[ReadOnly(true)]
		public bool IsMultiRow 
		{
			get
			{
				return (bool)this.GetValue(XamTabControl.IsMultiRowProperty);
			}
		}

		#endregion //IsMultiRow

		#region ComputedTabItemHorizontalScrollBarVisibility

		private static readonly DependencyProperty ComputedTabItemHorizontalScrollBarVisibilityProperty = DependencyProperty.Register("ComputedTabItemHorizontalScrollBarVisibility",
			typeof(Visibility), typeof(XamTabControl), new FrameworkPropertyMetadata(Visibility.Collapsed, new PropertyChangedCallback(OnComputedScrollBarVisibilityChanged)));

		#endregion //TabItemHorizontalScrollBarVisibility

		#region ComputedTabItemVerticalScrollBarVisibility

		private static readonly DependencyProperty ComputedTabItemVerticalScrollBarVisibilityProperty = DependencyProperty.Register("ComputedTabItemVerticalScrollBarVisibility",
			typeof(Visibility), typeof(XamTabControl), new FrameworkPropertyMetadata(Visibility.Collapsed, new PropertyChangedCallback(OnComputedScrollBarVisibilityChanged)));

		#endregion //TabItemVerticalScrollBarVisibility

		// AS 2/1/10 TFS26795
		#region HeaderArea
		internal FrameworkElement HeaderArea
		{
			get { return this.GetTemplateChild("PART_HeaderArea") as FrameworkElement; }
		} 
		#endregion //HeaderArea

        // JJD 8/4/08 Added
        #region InternalVersion

        private static readonly DependencyPropertyKey InternalVersionPropertyKey =
            DependencyProperty.RegisterReadOnly("InternalVersion",
            typeof(int), typeof(XamTabControl), new FrameworkPropertyMetadata(0));

        internal static readonly DependencyProperty InternalVersionProperty =
            InternalVersionPropertyKey.DependencyProperty;

         internal int InternalVersion
        {
            get
            {
                return (int)this.GetValue(XamTabControl.InternalVersionProperty);
            }
        }

        #endregion //InternalVersion

		// AS 7/13/09 TFS18399
		#region MinimizedContentPresenter
		internal ContentPresenter MinimizedContentPresenter
		{
			get { return _cpMinimized; }
		}
		#endregion //MinimizedContentPresenter

		#endregion //Internal

		#region Public

		#region AllowMinimize

		// JM 10-07-08 - Change the default to False.
		/// <summary>
		/// Identifies the <see cref="AllowMinimize"/> dependency property
		/// </summary>
		//public static readonly DependencyProperty AllowMinimizeProperty = DependencyProperty.Register("AllowMinimize",
		//    typeof(bool), typeof(XamTabControl), new FrameworkPropertyMetadata(KnownBoxes.TrueBox));
		//public static readonly DependencyProperty AllowMinimizeProperty = DependencyProperty.Register("AllowMinimize",
		//    typeof(bool), typeof(XamTabControl), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));
		public static readonly DependencyProperty AllowMinimizeProperty = DependencyProperty.Register("AllowMinimize",
			typeof(bool), typeof(XamTabControl), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnAllowMinimizeChanged)));

        // JJD 8/22/08 - added automation support for XamTabControl's ExpandCollapse pattern
        private static void OnAllowMinimizeChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            XamTabControl tc = target as XamTabControl;

            if ( tc != null )
                tc.RaiseAutomationExpandCollapseStateChanged();
        }

		/// <summary>
		/// Determines if the control can be minimized via the user interface - e.g. by double clicking on a tab item.
		/// </summary>
		/// <seealso cref="AllowMinimizeProperty"/>
		//[Description("Determines if the control can be minimized via the user interface - e.g. by double clicking on a tab item.")]
		//[Category("Behavior")]
		[Bindable(true)]
		public bool AllowMinimize
		{
			get
			{
				return (bool)this.GetValue(XamTabControl.AllowMinimizeProperty);
			}
			set
			{
				this.SetValue(XamTabControl.AllowMinimizeProperty, value);
			}
		}

		#endregion //AllowMinimize

        // JJD 8/4/08 XamTabControl
        #region AllowTabClosing

        /// <summary>
        /// Identifies the <see cref="AllowTabClosing"/> dependency property
        /// </summary>
        public static readonly DependencyProperty AllowTabClosingProperty = DependencyProperty.Register("AllowTabClosing",
            typeof(bool?), typeof(XamTabControl), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnAllowTabClosingChanged)));

        private static void OnAllowTabClosingChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            XamTabControl tc = target as XamTabControl;

            if (tc != null)
                tc.BumpInternalVersion();
        }

        /// <summary>
        /// Returns or sets a nullable boolean indicating if the containing tab items may be closed by default.
        /// </summary>
        /// <seealso cref="AllowTabClosingProperty"/>
        /// <seealso cref="TabItemEx.AllowClosing"/>
        //[Description("Returns or sets a nullable boolean indicating if the containing tab items may be closed by default.")]
        //[Category("Behavior")]
        public bool? AllowTabClosing
        {
            get
            {
                return (bool?)this.GetValue(XamTabControl.AllowTabClosingProperty);
            }
            set
            {
                this.SetValue(XamTabControl.AllowTabClosingProperty, value);
            }
        }

        #endregion //AllowTabClosing

        // JJD 8/4/08 XamTabControl
        #region DropDownAnimation

        /// <summary>
        /// Identifies the <see cref="DropDownAnimation"/> dependency property
        /// </summary>
        public static readonly DependencyProperty DropDownAnimationProperty = DependencyProperty.Register("DropDownAnimation",
            typeof(PopupAnimation), typeof(XamTabControl), new FrameworkPropertyMetadata(PopupAnimation.None));
        
        /// <summary>
        /// Returns or sets the animation used when IsMinimized is true and the control is showing/hiding the popup containing the selected content.
        /// </summary>
        /// <seealso cref="DropDownAnimationProperty"/>
        //[Description("Returns or sets the animation used when IsMinimized is true and the control is showing/hiding the popup containing the selected content.")]
        //[Category("Behavior")]
        public PopupAnimation DropDownAnimation
        {
            get
            {
                return (PopupAnimation)this.GetValue(XamTabControl.DropDownAnimationProperty);
            }
            set
            {
                this.SetValue(XamTabControl.DropDownAnimationProperty, value);
            }
        }

        #endregion //DropDownAnimation

		#region InterRowSpacing

		internal const double DefaultInterRowSpacing = 0d;

		/// <summary>
		/// Identifies the <see cref="InterRowSpacing"/> dependency property
		/// </summary>
		public static readonly DependencyProperty InterRowSpacingProperty = DependencyProperty.Register("InterRowSpacing",
			typeof(double), typeof(XamTabControl), new FrameworkPropertyMetadata(DefaultInterRowSpacing));

		/// <summary>
		/// The amount of space between rows of tab items.
		/// </summary>
		/// <seealso cref="InterRowSpacingProperty"/>
		//[Description("The amount of space between rows of tab items.")]
		//[Category("Layout")]
		[Bindable(true)]
		public double InterRowSpacing
		{
			get
			{
				return (int)this.GetValue(XamTabControl.InterRowSpacingProperty);
			}
			set
			{
				this.SetValue(XamTabControl.InterRowSpacingProperty, value);
			}
		}

		#endregion //InterRowSpacing

		#region InterTabSpacing

		internal const double DefaultInterTabSpacing = 0d;

		/// <summary>
		/// Identifies the <see cref="InterTabSpacing"/> dependency property
		/// </summary>
		// AS 10/16/07
		// Changed to internal. Perhaps we should just expose IsFirstTabInRow, IsLastTabInRow later and let the item change it margins based on that.
		//
		//public static readonly DependencyProperty InterTabSpacingProperty = DependencyProperty.Register("InterTabSpacing",
		public static readonly DependencyProperty InterTabSpacingProperty = DependencyProperty.Register("InterTabSpacing",
			typeof(double), typeof(XamTabControl), new FrameworkPropertyMetadata(DefaultInterTabSpacing));

		/// <summary>
		/// The amount of space between tab items.
		/// </summary>
		/// <seealso cref="InterTabSpacingProperty"/>
		//[Description("The amount of space between tab items.")]
		//[Category("Layout")]
		[Bindable(true)]
		// AS 10/16/07
		// Changed to internal. Perhaps we should just expose IsFirstTabInRow, IsLastTabInRow later and let the item change it margins based on that.
		//
		//public double InterTabSpacing
		public double InterTabSpacing
		{
			get
			{
				return (int)this.GetValue(XamTabControl.InterTabSpacingProperty);
			}
			set
			{
				this.SetValue(XamTabControl.InterTabSpacingProperty, value);
			}
		}

		#endregion //InterTabSpacing

		#region IsDropDownOpen

		/// <summary>
		/// Identifies the <see cref="IsDropDownOpen"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsDropDownOpenProperty = DependencyProperty.Register("IsDropDownOpen",
			typeof(bool), typeof(XamTabControl), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnIsDropDownOpenChanged), new CoerceValueCallback(CoerceIsDropDownOpen)));

		private static void OnIsDropDownOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			XamTabControl tab = d as XamTabControl;

			if (tab != null)
			{
				bool isDroppedDown = (bool)e.NewValue;

				if (isDroppedDown)
				{
					if (tab.IsMinimized == false)
					{
						// if the tab is not minimzed and we're being dropped down then 
						// change the isminimized state to true since it can only be dropped
						// down while minimized
						tab.IsMinimized = true;
					}
					else
					{
						tab.ResetSelectedItem();
					}
				}
				else if (tab.IsMinimized)
				{
					// when collapsing, we need to clear the selected tab item
					// and cache the previously selected tab item
					tab.ResetSelectedItem();
				}

				// AS 10/18/07
				// We need to make sure that the SelectedContent has been updated. Unfortunately the only way to 
				// do this is to call a method that will call the UpdateSelectedContent method of the tabcontrol
				// since that is protected. The least intrusive way is to call OnApplyTemplate since they 
				// only do that within that method.
				//
				tab.VerifySelectedContent();

				// AS 11/27/07 BR28724
				// We need to have custom logic for positioning the dropdown. We want it to be as wide as the header 
				// area when possible. Also, we need to special handle the position when the popup would span 
				// monitors.
				//
				if (isDroppedDown)
					tab.InitializeDropdownLocationInfo();

				// AS 7/13/09 TFS18399
				tab.RefreshSelectedTabPeerChildren();

				if (isDroppedDown)
					tab.RaiseDropDownOpened(new RoutedEventArgs());
				else
					tab.RaiseDropDownClosed(new RoutedEventArgs());

			}
		}

		private static object CoerceIsDropDownOpen(DependencyObject d, object value)
		{
			// we cannot dropdown when the control is not minimized
			XamTabControl tab = d as XamTabControl;

			if (tab != null &&
				KnownBoxes.TrueBox.Equals(value) &&
				tab.IsMinimized == false)
			{
				return KnownBoxes.FalseBox;
			}

			if (true == (bool)value)
			{
				TabControlDropDownOpeningEventArgs args = new TabControlDropDownOpeningEventArgs();
				tab.RaiseDropDownOpening(args);
			}

			return value;
		}

		/// <summary>
		/// Indicates if the minimized tab control is currently dropped down.
		/// </summary>
		/// <seealso cref="IsDropDownOpenProperty"/>
		//[Description("Indicates if the minimized tab control is currently dropped down.")]
		//[Category("Behavior")]
		[Bindable(true)]
		public bool IsDropDownOpen
		{
			get
			{
				return (bool)this.GetValue(XamTabControl.IsDropDownOpenProperty);
			}
			set
			{
				this.SetValue(XamTabControl.IsDropDownOpenProperty, value);
			}
		}

		#endregion //IsDropDownOpen

		#region IsMinimized

		/// <summary>
		/// Identifies the <see cref="IsMinimized"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsMinimizedProperty = DependencyProperty.Register("IsMinimized",
			typeof(bool), typeof(XamTabControl), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnIsMinimizedChanged)));

		/// <summary>
		/// Determines if the contents of the selected tab are displayed in a popup instead of within the tab control itself.
		/// </summary>
		/// <seealso cref="IsMinimizedProperty"/>
		//[Description("Determines if the contents of the selected tab are displayed in a popup instead of within the tab control itself.")]
		//[Category("Behavior")]
		[Bindable(true)]
		public bool IsMinimized
		{
			get
			{
				return (bool)this.GetValue(XamTabControl.IsMinimizedProperty);
			}
			set
			{
				this.SetValue(XamTabControl.IsMinimizedProperty, value);
			}
		}

		private static void OnIsMinimizedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			XamTabControl tab = d as XamTabControl;

			if (null != tab)
			{
				bool isMinimized = (bool)e.NewValue;

				// when it is restored, we should make sure the dropdown is closed
				if (isMinimized == false)
				{
					if (tab.IsDropDownOpen)
						tab.IsDropDownOpen = false;
				}

				// AS 10/18/07
				// Instead of forcing the user to handle this in the template, we'll do it in the code.
				//
				// AS 11/12/07 BR28406
				//ContentPresenter cpMinimized = tab.GetTemplateChild("PART_SelectedContentHostMinimized") as ContentPresenter;
				//ContentPresenter cpMaximized = tab.GetTemplateChild("PART_SelectedContentHost") as ContentPresenter;
				
#region Infragistics Source Cleanup (Region)









#endregion // Infragistics Source Cleanup (Region)

				tab.VerifyMinimizedContentBinding();

				tab.ResetSelectedItem();

                // JJD 8/22/08 - added automation support for XamTabControl's ExpandCollapse pattern
                tab.RaiseAutomationExpandCollapseStateChanged();

			}
		}

		#endregion //IsMinimized

		#region IsTabItemPanelScrolling

		private static readonly DependencyPropertyKey IsTabItemPanelScrollingPropertyKey =
			DependencyProperty.RegisterReadOnly("IsTabItemPanelScrolling",
			typeof(bool), typeof(XamTabControl), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		/// <summary>
		/// Identifies the <see cref="IsTabItemPanelScrolling"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsTabItemPanelScrollingProperty =
			IsTabItemPanelScrollingPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns a boolean indicating whether the scroll viewer containing the tab item panel is currently set up so that scrolling elements are visible.
		/// </summary>
		/// <seealso cref="IsTabItemPanelScrollingProperty"/>
		//[Description("Returns a boolean indicating whether the scroll viewer containing the tab item panel is currently set up so that scrolling elements are visible.")]
		//[Category("Behavior")]
		[Bindable(true)]
		[ReadOnly(true)]
		public bool IsTabItemPanelScrolling
		{
			get
			{
				return (bool)this.GetValue(XamTabControl.IsTabItemPanelScrollingProperty);
			}
		}

		#endregion //IsTabItemPanelScrolling

		#region MaximumSizeToFitAdjustment

		internal const double DefaultMaximumSizeToFit = 0d;

		/// <summary>
		/// Identifies the <see cref="MaximumSizeToFitAdjustment"/> dependency property
		/// </summary>
		public static readonly DependencyProperty MaximumSizeToFitAdjustmentProperty = DependencyProperty.Register("MaximumSizeToFitAdjustment",
			typeof(double), typeof(XamTabControl), new FrameworkPropertyMetadata(DefaultMaximumSizeToFit), new ValidateValueCallback(OnValidateAtLeastZero));

		private static bool OnValidateAtLeastZero(object newValue)
		{
			return newValue is double && ((double)newValue) >= 0;
		}

		/// <summary>
		/// The maximum amount of additional size to add to an element when the TabLayoutStyle is 'SingleRowSizeToFit' or 'MultiRowSizeToFit'.
		/// </summary>
		/// <seealso cref="MaximumSizeToFitAdjustmentProperty"/>
		/// <seealso cref="TabLayoutStyle"/>
		//[Description("The maximum amount of additional size to add to an element when the TabLayoutStyle is SingleRowSizeToFit.")]
		//[Category("Behavior")]
		[Bindable(true)]
		public double MaximumSizeToFitAdjustment
		{
			get
			{
				return (double)this.GetValue(XamTabControl.MaximumSizeToFitAdjustmentProperty);
			}
			set
			{
				this.SetValue(XamTabControl.MaximumSizeToFitAdjustmentProperty, value);
			}
		}

		#endregion //MaximumSizeToFitAdjustment

        #region MaximumTabRows

        /// <summary>
        /// Identifies the <see cref="MaximumTabRows"/> dependency property
        /// </summary>
        /// <seealso cref="MaximumTabRows"/>
        public static readonly DependencyProperty MaximumTabRowsProperty = DependencyProperty.Register("MaximumTabRows",
            typeof(int), typeof(XamTabControl), new FrameworkPropertyMetadata(3), new ValidateValueCallback(ValidateMaximumTabRows));

        private static bool ValidateMaximumTabRows(object value)
        {
            if (!(value is int) ||
                (int)value < 1)
                return false;

            return true;
        }

        /// <summary>
        /// Determines the maximum number of tab rows that will be displayed before a vertical scrollbar will appear.
        /// </summary>
        /// <remarks>
        /// <para class="body">The property can only be set to a positive integer. It defaults to 3.</para>
        /// <para class="note"><b>Note:</b> This property is ignored if the <see cref="XamTabControl.TabLayoutStyle"/> property is not set to one of the 'Multi...' layouts.</para>
        /// </remarks>
        /// <seealso cref="MaximumTabRowsProperty"/>
        /// <seealso cref="XamTabControl.TabLayoutStyle"/>
        //[Description("Determines the maximum number of tab rows that will be displayed before a vertical scrollbar will appear")]
        //[Category("Layout")]
        [Bindable(true)]
        public int MaximumTabRows
        {
            get
            {
                return (int)this.GetValue(XamTabControl.MaximumTabRowsProperty);
            }
            set
            {
                this.SetValue(XamTabControl.MaximumTabRowsProperty, value);
            }
        }

        #endregion //MaximumTabRows

		#region MinimumTabExtent

		internal const double DefaultMinimumTabExtent = 18d;

		/// <summary>
		/// Identifies the <see cref="MinimumTabExtent"/> dependency property
		/// </summary>
		public static readonly DependencyProperty MinimumTabExtentProperty = DependencyProperty.Register("MinimumTabExtent",
			typeof(double), typeof(XamTabControl), new FrameworkPropertyMetadata(DefaultMinimumTabExtent), new ValidateValueCallback(OnValidateAtLeastZero));

		/// <summary>
		/// The minimum extent for a tab item. That is, the minimum physical width when the TabStripPlacement is Top or Bottom and the minimum physical height when the TabStripPlacement is Left or Right.
		/// </summary>
		/// <seealso cref="MinimumTabExtentProperty"/>
		//[Description("The minimum extent for a tab item. That is, the minimum physical width when the TabStripPlacement is Top or Bottom and the minimum physical height when the TabStripPlacement is Left or Right.")]
		//[Category("Appearance")]
		[Bindable(true)]
		public double MinimumTabExtent
		{
			get
			{
				return (double)this.GetValue(XamTabControl.MinimumTabExtentProperty);
			}
			set
			{
				this.SetValue(XamTabControl.MinimumTabExtentProperty, value);
			}
		}

		#endregion //MinimumTabExtent

		#region PostTabItemContent

		/// <summary>
		/// Identifies the <see cref="PostTabItemContent"/> dependency property
		/// </summary>
		public static readonly DependencyProperty PostTabItemContentProperty = DependencyProperty.Register("PostTabItemContent",
			typeof(object), typeof(XamTabControl), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// The content that should be displayed in the tab item header area to the right/bottom of the tab items.
		/// </summary>
		/// <seealso cref="PostTabItemContentProperty"/>
		//[Description("The content that should be displayed in the tab item header area to the left/top of the tab items.")]
		//[Category("Behavior")]
		[Bindable(true)]
		public object PostTabItemContent
		{
			get
			{
				return (object)this.GetValue(XamTabControl.PostTabItemContentProperty);
			}
			set
			{
				this.SetValue(XamTabControl.PostTabItemContentProperty, value);
			}
		}

		#endregion //PostTabItemContent

		#region PostTabItemContentTemplate

		/// <summary>
		/// Identifies the <see cref="PostTabItemContentTemplate"/> dependency property
		/// </summary>
		public static readonly DependencyProperty PostTabItemContentTemplateProperty = DependencyProperty.Register("PostTabItemContentTemplate",
			typeof(DataTemplate), typeof(XamTabControl), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// The template used to display the content of the <see cref="PostTabItemContent"/>
		/// </summary>
		/// <seealso cref="PostTabItemContentTemplateProperty"/>
		/// <seealso cref="PostTabItemContentTemplateSelector"/>
		/// <seealso cref="PostTabItemContent"/>
		//[Description("The template used to display the content of the 'PostTabItemContent'")]
		//[Category("Behavior")]
		[Bindable(true)]
		public DataTemplate PostTabItemContentTemplate
		{
			get
			{
				return (DataTemplate)this.GetValue(XamTabControl.PostTabItemContentTemplateProperty);
			}
			set
			{
				this.SetValue(XamTabControl.PostTabItemContentTemplateProperty, value);
			}
		}

		#endregion //PostTabItemContentTemplate

		#region PostTabItemContentTemplateSelector

		/// <summary>
		/// Identifies the <see cref="PostTabItemContentTemplateSelector"/> dependency property
		/// </summary>
		public static readonly DependencyProperty PostTabItemContentTemplateSelectorProperty = DependencyProperty.Register("PostTabItemContentTemplateSelector",
			typeof(DataTemplateSelector), typeof(XamTabControl), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// A DataTemplateSelector that can be used to provide custom logic for choosing the template for the <see cref="PostTabItemContent"/>.
		/// </summary>
		/// <seealso cref="PostTabItemContentTemplateSelectorProperty"/>
		/// <seealso cref="PostTabItemContentTemplate"/>
		/// <seealso cref="PostTabItemContent"/>
		//[Description("A DataTemplateSelector that can be used to provide custom logic for choosing the template for the 'PostTabItemContent'.")]
		//[Category("Behavior")]
		[Bindable(true)]
		public DataTemplateSelector PostTabItemContentTemplateSelector
		{
			get
			{
				return (DataTemplateSelector)this.GetValue(XamTabControl.PostTabItemContentTemplateSelectorProperty);
			}
			set
			{
				this.SetValue(XamTabControl.PostTabItemContentTemplateSelectorProperty, value);
			}
		}

		#endregion //PostTabItemContentTemplateSelector

		// AS 11/27/07 BR28724
		#region PreferredDropDownExtent

		private static readonly DependencyPropertyKey PreferredDropDownExtentPropertyKey =
			DependencyProperty.RegisterReadOnly("PreferredDropDownExtent",
			typeof(double), typeof(XamTabControl), new FrameworkPropertyMetadata(double.NaN));

		/// <summary>
		/// Identifies the <see cref="PreferredDropDownExtent"/> dependency property
		/// </summary>
		public static readonly DependencyProperty PreferredDropDownExtentProperty =
			PreferredDropDownExtentPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the preferred extent (width when tabs are on top/bottom and height when tabs are on the left/right) for the dropdown of the tab control.
		/// </summary>
		/// <seealso cref="PreferredDropDownExtentProperty"/>
		[Browsable(false)]
		[Bindable(true)]
		public double PreferredDropDownExtent
		{
			get
			{
				return (double)this.GetValue(XamTabControl.PreferredDropDownExtentProperty);
			}
		}

		#endregion //PreferredDropDownExtent

		// AS 11/27/07 BR28724
		#region PreferredDropDownPlacementCallback

		private static readonly DependencyPropertyKey PreferredDropDownPlacementCallbackPropertyKey = DependencyProperty.RegisterReadOnly("PreferredDropDownPlacementCallback",
			typeof(CustomPopupPlacementCallback), typeof(XamTabControl), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Identifies the <see cref="PreferredDropDownPlacementCallback"/> dependency property
		/// </summary>
		public static readonly DependencyProperty PreferredDropDownPlacementCallbackProperty = PreferredDropDownPlacementCallbackPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns a delegate handler that positions the dropdown of the control.
		/// </summary>
		/// <seealso cref="PreferredDropDownPlacementCallbackProperty"/>
		[Browsable(false)]
		[Bindable(true)]
		public CustomPopupPlacementCallback PreferredDropDownPlacementCallback
		{
			get
			{
				return (CustomPopupPlacementCallback)this.GetValue(XamTabControl.PreferredDropDownPlacementCallbackProperty);
			}
		}

		#endregion //PreferredDropDownPlacementCallback

		#region PreTabItemContent

		/// <summary>
		/// Identifies the <see cref="PreTabItemContent"/> dependency property
		/// </summary>
		public static readonly DependencyProperty PreTabItemContentProperty = DependencyProperty.Register("PreTabItemContent",
			typeof(object), typeof(XamTabControl), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// The content that should be displayed in the tab item header area to the left/top of the tab items.
		/// </summary>
		/// <seealso cref="PreTabItemContentProperty"/>
		//[Description("The content that should be displayed in the tab item header area to the left/top of the tab items.")]
		//[Category("Behavior")]
		[Bindable(true)]
		public object PreTabItemContent
		{
			get
			{
				return (object)this.GetValue(XamTabControl.PreTabItemContentProperty);
			}
			set
			{
				this.SetValue(XamTabControl.PreTabItemContentProperty, value);
			}
		}

		#endregion //PreTabItemContent

		#region PreTabItemContentTemplate

		/// <summary>
		/// Identifies the <see cref="PreTabItemContentTemplate"/> dependency property
		/// </summary>
		public static readonly DependencyProperty PreTabItemContentTemplateProperty = DependencyProperty.Register("PreTabItemContentTemplate",
			typeof(DataTemplate), typeof(XamTabControl), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// The template used to display the content of the <see cref="PreTabItemContent"/>
		/// </summary>
		/// <seealso cref="PreTabItemContentTemplateProperty"/>
		/// <seealso cref="PreTabItemContentTemplateSelector"/>
		/// <seealso cref="PreTabItemContent"/>
		//[Description("The template used to display the content of the 'PreTabItemContent'")]
		//[Category("Behavior")]
		[Bindable(true)]
		public DataTemplate PreTabItemContentTemplate
		{
			get
			{
				return (DataTemplate)this.GetValue(XamTabControl.PreTabItemContentTemplateProperty);
			}
			set
			{
				this.SetValue(XamTabControl.PreTabItemContentTemplateProperty, value);
			}
		}

		#endregion //PreTabItemContentTemplate

		#region PreTabItemContentTemplateSelector

		/// <summary>
		/// Identifies the <see cref="PreTabItemContentTemplateSelector"/> dependency property
		/// </summary>
		public static readonly DependencyProperty PreTabItemContentTemplateSelectorProperty = DependencyProperty.Register("PreTabItemContentTemplateSelector",
			typeof(DataTemplateSelector), typeof(XamTabControl), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// A DataTemplateSelector that can be used to provide custom logic for choosing the template for the <see cref="PreTabItemContent"/>.
		/// </summary>
		/// <seealso cref="PreTabItemContentTemplateSelectorProperty"/>
		/// <seealso cref="PreTabItemContentTemplate"/>
		/// <seealso cref="PreTabItemContent"/>
		//[Description("A DataTemplateSelector that can be used to provide custom logic for choosing the template for the 'PreTabItemContent'.")]
		//[Category("Behavior")]
		[Bindable(true)]
		public DataTemplateSelector PreTabItemContentTemplateSelector
		{
			get
			{
				return (DataTemplateSelector)this.GetValue(XamTabControl.PreTabItemContentTemplateSelectorProperty);
			}
			set
			{
				this.SetValue(XamTabControl.PreTabItemContentTemplateSelectorProperty, value);
			}
		}

		#endregion //PreTabItemContentTemplateSelector

        // JJD 8/4/08 XamTabControl
        #region ShowTabHeaderCloseButton

        /// <summary>
        /// Identifies the <see cref="ShowTabHeaderCloseButton"/> dependency property
        /// </summary>
        public static readonly DependencyProperty ShowTabHeaderCloseButtonProperty = DependencyProperty.Register("ShowTabHeaderCloseButton",
            typeof(bool), typeof(XamTabControl), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnShowTabHeaderCloseButtonChanged)));

        private static void OnShowTabHeaderCloseButtonChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            XamTabControl tc = target as XamTabControl;

            if (tc != null)
            {
                tc.BumpInternalVersion();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        /// <summary>
        /// Returns or sets a boolean indicating whether a close button should be displayed within the tab item area of the control
        /// </summary>
        /// <seealso cref="ShowTabHeaderCloseButtonProperty"/>
        /// <seealso cref="AllowTabClosing"/>
        /// <seealso cref="TabItemCloseButtonVisibility"/>
        /// <seealso cref="TabItemEx.AllowClosing"/>
        /// <seealso cref="TabItemEx.CloseButtonVisibility"/>
        //[Description("Returns or sets a boolean indicating whether a close button should be displayed within the tab item area of the control")]
        //[Category("Behavior")]
        public bool ShowTabHeaderCloseButton
        {
            get
            {
                return (bool)this.GetValue(XamTabControl.ShowTabHeaderCloseButtonProperty);
            }
            set
            {
                this.SetValue(XamTabControl.ShowTabHeaderCloseButtonProperty, value);
            }
        }

        #endregion //ShowTabHeaderCloseButton

		// AS 6/29/12 TFS114953
		#region TabHeaderHeight

		/// <summary>
		/// Identifies the <see cref="TabHeaderHeight"/> dependency property
		/// </summary>
		public static readonly DependencyProperty TabHeaderHeightProperty = DependencyPropertyUtilities.Register( "TabHeaderHeight",
			typeof( double ), typeof( XamTabControl ),
			DependencyPropertyUtilities.CreateMetadata( 0d )
			);

		/// <summary>
		/// Returns the actual height of the tab header area.
		/// </summary>
		/// <seealso cref="TabHeaderHeightProperty"/>
		public double TabHeaderHeight
		{
			get
			{
				return (double)this.GetValue( XamTabControl.TabHeaderHeightProperty );
			}
			private set
			{
				this.SetValue( XamTabControl.TabHeaderHeightProperty, value );
			}
		}

		#endregion //TabHeaderHeight

        // JJD 8/4/08 XamTabControl
        #region TabItemCloseButtonVisibility

        /// <summary>
        /// Identifies the <see cref="TabItemCloseButtonVisibility"/> dependency property
        /// </summary>
        public static readonly DependencyProperty TabItemCloseButtonVisibilityProperty = DependencyProperty.Register("TabItemCloseButtonVisibility",
            typeof(TabItemCloseButtonVisibility), typeof(XamTabControl), new FrameworkPropertyMetadata(TabItemCloseButtonVisibility.Hidden, new PropertyChangedCallback(OnTabItemCloseButtonVisibilityChanged)));

        private static void OnTabItemCloseButtonVisibilityChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            XamTabControl tc = target as XamTabControl;

            if (tc != null)
                tc.BumpInternalVersion();
        }

        /// <summary>
        /// Returns or sets a the default value for the TabItemEx.CloseButtonVisibility determining when a tab item should display the close button.
        /// </summary>
        /// <seealso cref="TabItemCloseButtonVisibilityProperty"/>
        //[Description("Returns or sets a the default value for the TabItemEx.CloseButtonVisibility determining when a tab item should display the close button.")]
        //[Category("Behavior")]
        public TabItemCloseButtonVisibility TabItemCloseButtonVisibility
        {
            get
            {
                return (TabItemCloseButtonVisibility)this.GetValue(XamTabControl.TabItemCloseButtonVisibilityProperty);
            }
            set
            {
                this.SetValue(XamTabControl.TabItemCloseButtonVisibilityProperty, value);
            }
        }

        #endregion //TabItemCloseButtonVisibility
	
		#region TabItemContentHeight

		/// <summary>
		/// Identifies the <see cref="TabItemContentHeight"/> dependency property
		/// </summary>
		public static readonly DependencyProperty TabItemContentHeightProperty = DependencyProperty.Register("TabItemContentHeight",
			typeof(double), typeof(XamTabControl), new FrameworkPropertyMetadata(double.NaN));

		/// <summary>
		/// Returns or sets a value that controls the height of the content presenter used to display the 'Content' of the selected tab item.
		/// </summary>
		/// <seealso cref="TabItemContentHeightProperty"/>
		//[Description("Returns or sets a value that controls the height of the content presenter used to display the 'Content' of the selected tab item.")]
		//[Category("Layout")]
		[Bindable(true)]
		public double TabItemContentHeight
		{
			get
			{
				return (double)this.GetValue(XamTabControl.TabItemContentHeightProperty);
			}
			set
			{
				this.SetValue(XamTabControl.TabItemContentHeightProperty, value);
			}
		}

		#endregion //TabItemContentHeight

		#region TabItemHorizontalScrollBarVisibility

		private static readonly DependencyPropertyKey TabItemHorizontalScrollBarVisibilityPropertyKey =
			DependencyProperty.RegisterReadOnly("TabItemHorizontalScrollBarVisibility",
			typeof(ScrollBarVisibility), typeof(XamTabControl), new FrameworkPropertyMetadata(ScrollBarVisibility.Auto));

		/// <summary>
		/// Identifies the <see cref="TabItemHorizontalScrollBarVisibility"/> dependency property
		/// </summary>
		public static readonly DependencyProperty TabItemHorizontalScrollBarVisibilityProperty =
			TabItemHorizontalScrollBarVisibilityPropertyKey.DependencyProperty;

		/// <summary>
		/// Indicates the preferred visibility of the horizontal scroll elements for scrolling the tab items.
		/// </summary>
		/// <seealso cref="TabItemHorizontalScrollBarVisibilityProperty"/>
		[ReadOnly(true)]
		public ScrollBarVisibility TabItemHorizontalScrollBarVisibility
		{
			get
			{
				return (ScrollBarVisibility)this.GetValue(XamTabControl.TabItemHorizontalScrollBarVisibilityProperty);
			}
		}

		#endregion //TabItemHorizontalScrollBarVisibility

		#region TabItemVerticalScrollBarVisibility

		private static readonly DependencyPropertyKey TabItemVerticalScrollBarVisibilityPropertyKey =
			DependencyProperty.RegisterReadOnly("TabItemVerticalScrollBarVisibility",
			typeof(ScrollBarVisibility), typeof(XamTabControl), new FrameworkPropertyMetadata(ScrollBarVisibility.Auto));

		/// <summary>
		/// Identifies the <see cref="TabItemVerticalScrollBarVisibility"/> dependency property
		/// </summary>
		public static readonly DependencyProperty TabItemVerticalScrollBarVisibilityProperty =
			TabItemVerticalScrollBarVisibilityPropertyKey.DependencyProperty;

		/// <summary>
		/// Indicates the preferred visibility of the vertical scroll elements for scrolling the tab items.
		/// </summary>
		/// <seealso cref="TabItemVerticalScrollBarVisibilityProperty"/>
		[ReadOnly(true)]
		public ScrollBarVisibility TabItemVerticalScrollBarVisibility
		{
			get
			{
				return (ScrollBarVisibility)this.GetValue(XamTabControl.TabItemVerticalScrollBarVisibilityProperty);
			}
		}

		#endregion //TabItemVerticalScrollBarVisibility

		#region TabLayoutStyle

		/// <summary>
		/// Identifies the <see cref="TabLayoutStyle"/> dependency property
		/// </summary>
		public static readonly DependencyProperty TabLayoutStyleProperty = DependencyProperty.Register("TabLayoutStyle",
			typeof(TabLayoutStyle), typeof(XamTabControl), new FrameworkPropertyMetadata(TabLayoutStyle.SingleRowAutoSize, new PropertyChangedCallback(OnTabLayoutStyleChanged)));

		private static void OnTabLayoutStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			XamTabControl tab = d as XamTabControl;

			if (null != tab)
				tab.SetValue(IsMultiRowPropertyKey, TabItemPanel.IsMultiRowStyle(tab.TabLayoutStyle));
		}

		/// <summary>
		/// Determines how the tab items will be arranged.
		/// </summary>
		/// <seealso cref="TabLayoutStyleProperty"/>
		//[Description("Determines how the tab items will be arranged.")]
		//[Category("Behavior")]
		[Bindable(true)]
		public TabLayoutStyle TabLayoutStyle
		{
			get
			{
				return (TabLayoutStyle)this.GetValue(XamTabControl.TabLayoutStyleProperty);
			}
			set
			{
				this.SetValue(XamTabControl.TabLayoutStyleProperty, value);
			}
		}

		#endregion //TabLayoutStyle

		#region TabPriority

		/// <summary>
		/// Identifies the TabPriority attached dependency property
		/// </summary>
		/// <seealso cref="GetTabPriority"/>
		/// <seealso cref="SetTabPriority"/>
		public static readonly DependencyProperty TabPriorityProperty = DependencyProperty.RegisterAttached("TabPriority",
			typeof(int), typeof(XamTabControl), new FrameworkPropertyMetadata(0));

		/// <summary>
		/// Gets the value of the 'TabPriority' attached property
		/// </summary>
		/// <remarks>
		/// <p class="body">The priority is used to determine the order in which tabs are resized. For example, then when 
		/// the <see cref="TabLayoutStyle"/> is set to <b>SingleRowSizeToFit</b>, tabs with a higher priority will be 
		/// increased up to the <see cref="MaximumSizeToFitAdjustment"/> before other tabs with a lower priority</p>
		/// </remarks>
		/// <seealso cref="TabPriorityProperty"/>
		/// <seealso cref="SetTabPriority"/>
		[AttachedPropertyBrowsableForChildrenAttribute()]
		public static int GetTabPriority(DependencyObject d)
		{
			return (int)d.GetValue(XamTabControl.TabPriorityProperty);
		}

		/// <summary>
		/// Sets the value of the 'TabPriority' attached property
		/// </summary>
		/// <seealso cref="TabPriorityProperty"/>
		/// <seealso cref="GetTabPriority"/>
		public static void SetTabPriority(DependencyObject d, int value)
		{
			d.SetValue(XamTabControl.TabPriorityProperty, value);
		}

		#endregion //TabPriority

        #region Theme

        /// <summary>
        /// Identifies the 'Theme' dependency property
        /// </summary>
        public static readonly DependencyProperty ThemeProperty = ThemeManager.ThemeProperty.AddOwner(typeof(XamTabControl), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnThemeChanged)));

        /// <summary>
        /// Event ID for the 'ThemeChanged' routed event
        /// </summary>
        public static readonly RoutedEvent ThemeChangedEvent = ThemeManager.ThemeChangedEvent.AddOwner(typeof(XamTabControl));

        private static void OnThemeChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            XamTabControl control = target as XamTabControl;

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
        //[Category("Appearance")]
        [Bindable(true)]
        [DefaultValue((string)null)]
        [TypeConverter(typeof(Infragistics.Windows.Themes.Internal.PrimitivesThemeTypeConverter))]
        public string Theme
        {
            get
            {
                return (string)this.GetValue(XamTabControl.ThemeProperty);
            }
            set
            {
                this.SetValue(XamTabControl.ThemeProperty, value);
            }
        }

        /// <summary>
        /// Called when property 'Theme' changes
        /// </summary>
        protected virtual void OnThemeChanged(string previousValue, string currentValue)
        {
            RoutedPropertyChangedEventArgs<string> newEvent = new RoutedPropertyChangedEventArgs<string>(previousValue, currentValue);
            newEvent.RoutedEvent = XamTabControl.ThemeChangedEvent;
            newEvent.Source = this;
            RaiseEvent(newEvent);
        }

        /// <summary>
        /// Invoked when the 'Theme' property has been changed.
        /// </summary>
        //[Description("Invoked when the 'Theme' property has been changed.")]
        public event RoutedPropertyChangedEventHandler<string> ThemeChanged
        {
            add
            {
                base.AddHandler(XamTabControl.ThemeChangedEvent, value);
            }
            remove
            {
                base.RemoveHandler(XamTabControl.ThemeChangedEvent, value);
            }
        }

        #endregion //Theme

		#endregion //Public

		#endregion //Properties

		#region Methods

        #region Public Methods

        #region ExecuteCommand

        /// <summary>
        /// Executes the specified RoutedCommand.
        /// </summary>
        /// <param name="command">The RoutedCommand to execute.</param>
        /// <returns>True if command was executed, false if canceled.</returns>
        /// <seealso cref="ExecutingCommand"/>
        /// <seealso cref="ExecutedCommand"/>
        /// <seealso cref="TabControlCommands"/>
        public bool ExecuteCommand(RoutedCommand command)
        {
            return this.ExecuteCommandImpl(new ExecuteCommandInfo(command));
        }

        #endregion //ExecuteCommand

        #endregion //Public Methods

		#region Private

        // JJD 8/4/08 added
        #region BumpInternalVersion

        private void BumpInternalVersion()
        {
            this.SetValue(InternalVersionPropertyKey, this.InternalVersion + 1);
        }

        #endregion //BumpInternalVersion	

        // JJD 8/5/08 added
        #region CloseTab

        internal bool CloseTab(TabItemEx tab)
        {
            // try to close the tab
            if (tab.Close() == false)
                return false;

            // Make sure that the selected tab is still visible
            this.EnsureSelectedTabIsVisible();

            return true;
        }

        #endregion //CloseTab	
    
        // JJD 8/5/08 added
        #region CloseAllTabs

        internal bool CloseAllTabs(TabItem excludeTab)
        {
            if (this.HasItems)
            {
                // Loop over the tab items looking for TabItemExs and close all but the excludeTab
                for (int i = 0, count = this.Items.Count; i < count; i++)
                {
                    TabItemEx tab = this.GetTabItemAtIndex(i, false, true) as TabItemEx;

                    if (null != tab && tab != excludeTab)
                        tab.Close();
                }
            }

            // Make sure that the selected tab is still visible
            this.EnsureSelectedTabIsVisible();

            return true;
        }

        #endregion //CloseAllTabs	
    
        // JJD 8/5/08 added
        #region EnsureSelectedTabIsVisible

        private void EnsureSelectedTabIsVisible()
        {
            TabItem tab = this.GetSelectedTabItem();

            if (tab == null || tab.Visibility == Visibility.Visible)
                return;

            // First try to get the next selectable tab (visible and enabled)
            TabItem tabToSelect = this.GetNextTabItem(tab, true, true);

            // If that failed try to get the previous selectable tab (visible and enabled)
            if (tabToSelect == null)
                tabToSelect = this.GetPreviousTabItem(tab, true, true);

            // If that failed try to get the next visible tab 
            if (tabToSelect == null)
                tabToSelect = this.GetNextTabItem(tab, false, true);

            // If that failed try to get the previous visible tab 
            if (tabToSelect == null)
                tabToSelect = this.GetPreviousTabItem(tab, false, true);

            if (tabToSelect == null)
                this.SelectedIndex = -1;
            else
                this.SelectedIndex = this.ItemContainerGenerator.IndexFromContainer(tabToSelect);

        }

        #endregion //EnsureSelectedTabIsVisible	

        // JJD 8/5/08 added
        #region ExecuteCommandImpl

        private bool ExecuteCommandImpl(ExecuteCommandInfo commandInfo)
        {
            RoutedCommand command = commandInfo.RoutedCommand;

            // Make sure we have a command to execute.
            Utilities.ThrowIfNull(command, "command");

            // Make sure the minimal control state exists to execute the command.
            if (TabControlCommands.IsMinimumStatePresentForCommand(this as ICommandHost, command) == false)
                return false;

            // make sure the command can be executed
            if (false == ((ICommandHost)this).CanExecute(commandInfo))
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

            // =========================================================================================
            // Determine which of our supported commands should be executed and do the associated action.
            bool handled = false;

            if (command == TabItemExCommands.Close ||
                command == TabControlCommands.CloseSelected)
            {
                TabItemEx tab = this.GetSelectedTabItem() as TabItemEx;

                if (tab != null)
                    handled = this.CloseTab(tab);
            }
            else
            if (command == TabItemExCommands.CloseAllButThis)
            {
                TabItem tab = this.GetSelectedTabItem() as TabItem;

                if (tab != null)
                    handled = this.CloseAllTabs(tab);
            }
            else
            if (command == TabControlCommands.CloseAll)
                handled = this.CloseAllTabs(null);
            else
            if (command == TabControlCommands.SelectFirstTab)
            {
                TabItem tab = this.GetFirstTabItemToSelect();

                if (tab != null)
                {
					// AS 5/10/12 TFS111333
					//tab.IsSelected = true;
					DependencyPropertyUtilities.SetCurrentValue(tab, TabItem.IsSelectedProperty, KnownBoxes.TrueBox);
					tab.BringIntoView();
                }

                handled = tab != null && tab.IsSelected;
            }
            else
            if (command == TabControlCommands.SelectLastTab)
            {
                TabItem tab = this.GetLastTabItem(true, true);

                if (tab == null)
                    tab = this.GetLastTabItem(false, true);

                if (tab != null)
                {
					// AS 5/10/12 TFS111333
					//tab.IsSelected = true;
					DependencyPropertyUtilities.SetCurrentValue(tab, TabItem.IsSelectedProperty, KnownBoxes.TrueBox);
					tab.BringIntoView();
                }

                handled = tab != null && tab.IsSelected;
            }

            if (command == TabControlCommands.SelectNextTab)
            {
                TabItem currentlySelectedTab = this.GetSelectedTabItem();

                if (currentlySelectedTab != null)
                {
                    TabItem tab = this.GetNextTabItem(currentlySelectedTab, true, true);

                    if (tab == null)
                        tab = this.GetNextTabItem(currentlySelectedTab, false, true);

                    if (tab != null)
                    {
						// AS 5/10/12 TFS111333
						//tab.IsSelected = true;
						DependencyPropertyUtilities.SetCurrentValue(tab, TabItem.IsSelectedProperty, KnownBoxes.TrueBox);
						tab.BringIntoView();
                    }

                    handled = tab != null && tab.IsSelected;
                }
            }
            else
            if (command == TabControlCommands.SelectPreviousTab)
            {
                TabItem currentlySelectedTab = this.GetSelectedTabItem();

                if (currentlySelectedTab != null)
                {
                    TabItem tab = this.GetPreviousTabItem(currentlySelectedTab, true, true);

                    if (tab == null)
                        tab = this.GetPreviousTabItem(currentlySelectedTab, false, true);

                    if (tab != null)
                    {
						// AS 5/10/12 TFS111333
						//tab.IsSelected = true;
						DependencyPropertyUtilities.SetCurrentValue(tab, TabItem.IsSelectedProperty, KnownBoxes.TrueBox);
						tab.BringIntoView();
                    }

                    handled = tab != null && tab.IsSelected;
                }
            }
            else
            if (command == TabControlCommands.Expand)
            {
                this.IsMinimized = false;

                handled = this.IsMinimized == false;
            }
            else
            if (command == TabControlCommands.Minimize)
            {
                if (this.AllowMinimize)
                {
                    this.IsMinimized = true;

                    handled = this.IsMinimized == true;
                }
            }
            else
            if (command == TabControlCommands.ToggleMinimized)
            {
                if (this.IsMinimized)
                {
                    this.IsMinimized = false;

                    handled = this.IsMinimized == false;
                }
                else
                {
                    if (!this.AllowMinimize)
                        return false;

                    this.IsMinimized = true;

                    handled = this.IsMinimized == true;
                }
            }

            // =========================================================================================

            //PostExecute:
            // If the command was executed, fire the 'after executed' event.
            if (handled == true)
                this.RaiseExecutedCommand(new ExecutedCommandEventArgs(command));


            return handled;
        }

        #endregion //ExecuteCommandImpl
    
        // JJD 8/5/08 added
        #region GetFirstTabItem

        private TabItem GetFirstTabItem(bool excludeDisabled, bool excludeCollapsed)
        {
            if (this.HasItems)
            {
                for (int i = 0, count = this.Items.Count; i < count; i++)
                {
                    TabItem tab = this.GetTabItemAtIndex(i, excludeDisabled, excludeCollapsed);

                    if (null != tab)
                        return tab;
                }
            }

            return null;
        }

        #endregion //GetFirstTabItem

        // JJD 9/11/08 - added
        #region GetFirstTabItemToSelect

        private TabItem GetFirstTabItemToSelect()
        {
            // try to get the first visible and enabled tab
            TabItem tab = this.GetFirstTabItem(true, true);

            if (tab != null)
                return tab;

            // settle for the 1st visible tab
            return this.GetFirstTabItem(false, true);
        }

        #endregion //GetFirstTabItemToSelect	
    
        // JJD 8/5/08 added
        #region GetLastTabItem

        private TabItem GetLastTabItem(bool excludeDisabled, bool excludeCollapsed)
        {
            if (this.HasItems)
            {
                for (int i = this.Items.Count - 1; i >= 0; i--)
                {
                    TabItem tab = this.GetTabItemAtIndex(i, excludeDisabled, excludeCollapsed);

                    if (null != tab)
                        return tab;
                }
            }

            return null;
        }

        #endregion //GetLastTabItem	

        // JJD 8/5/08 added
        #region GetNextTabItem

        private TabItem GetNextTabItem(TabItem startTab, bool excludeDisabled, bool excludeCollapsed)
        {
            if (this.HasItems)
            {
                int startIndex = this.ItemContainerGenerator.IndexFromContainer(startTab);

                for (int i = startIndex + 1, count = this.Items.Count; i < count; i++)
                {
                    TabItem tab = this.GetTabItemAtIndex(i, excludeDisabled, excludeCollapsed);

                    if (null != tab)
                        return tab;
                }
            }

            return null;
        }

        #endregion //GetNextTabItem	

        // JJD 8/5/08 added
        #region GetPreviousTabItem

        private TabItem GetPreviousTabItem(TabItem startTab, bool excludeDisabled, bool excludeCollapsed)
        {
            if (this.HasItems)
            {
                int startIndex = this.ItemContainerGenerator.IndexFromContainer(startTab);

                if (startIndex < 0)
                    startIndex = this.Items.Count;

                for (int i = startIndex - 1; i >= 0; i--)
                {
                    TabItem tab = this.GetTabItemAtIndex(i, excludeDisabled, excludeCollapsed);

                    if (null != tab)
                        return tab;
                }
            }

            return null;
        }

        #endregion //GetPreviousTabItem	
    
		// JM 10-13-08 TFS8792
		#region GetFirstVisibleTabItem

		private object GetFirstVisibleTabItem()
		{
			for (int i = 0; i < this.Items.Count; i++)
			{
				FrameworkElement itemContainer = this.ItemContainerGenerator.ContainerFromIndex(i) as FrameworkElement;
				if (itemContainer != null && itemContainer.Visibility == Visibility.Visible  &&  itemContainer.IsEnabled == true)
					return itemContainer;
			}

			return null;
		}

		#endregion //GetFirstVisibleTabItem

		// AS 11/27/07 BR28724
		#region GetPreferredPopupPlacement
		private CustomPopupPlacement[] GetPreferredPopupPlacement(Size popupSize, Size targetSize, Point offset)
		{
			CustomPopupPlacement[] placements = new CustomPopupPlacement[2];
			PopupPrimaryAxis axis;
			Point primaryPoint, alternatePoint;

			// AS 12/7/07 RightToLeft
			// Account for the horizontal flow direction when on top/bottom
			//
			double horizontalOffsetForTopBottom = this.FlowDirection == FlowDirection.LeftToRight ? 0 : -popupSize.Width;

			switch (this.TabStripPlacement)
			{
				case Dock.Left:
					primaryPoint = new Point(targetSize.Width, 0);
					alternatePoint = new Point(-popupSize.Width, 0);
					axis = PopupPrimaryAxis.Horizontal;
					break;
				case Dock.Right:
					alternatePoint = new Point(targetSize.Width, 0);
					primaryPoint = new Point(-popupSize.Width, 0);
					axis = PopupPrimaryAxis.Horizontal;
					break;
				default:
				case Dock.Top:
					primaryPoint = new Point(horizontalOffsetForTopBottom, targetSize.Height);
					alternatePoint = new Point(horizontalOffsetForTopBottom, -popupSize.Height);
					axis = PopupPrimaryAxis.Vertical;
					break;
				case Dock.Bottom:
					alternatePoint = new Point(horizontalOffsetForTopBottom, targetSize.Height);
					primaryPoint = new Point(horizontalOffsetForTopBottom, -popupSize.Height);
					axis = PopupPrimaryAxis.Vertical;
					break;
			}

			placements[0] = new CustomPopupPlacement(primaryPoint, axis);
			placements[1] = new CustomPopupPlacement(alternatePoint, axis);

			return placements;
		}
		#endregion //GetPreferredPopupPlacement

        // JJD 8/5/08 added
        #region GetSelectedTabItem

        private TabItem GetSelectedTabItem()
        {
            int index = this.SelectedIndex;

            if (index >= 0)
                return this.ItemContainerGenerator.ContainerFromIndex(index) as TabItem;

            return null;

        }

        #endregion //GetSelectedTabItem	

        // JJD 8/5/08 added
        #region GetTabItemAtIndex

        private TabItem GetTabItemAtIndex(int index, bool nullIfDisabled, bool nullIfCollapsed)
        {
            TabItem tab = this.ItemContainerGenerator.ContainerFromIndex(index) as TabItem;

            if (null != tab)
            {
                if (nullIfCollapsed == false || tab.Visibility != Visibility.Collapsed)
                {
                    if (nullIfDisabled == false || tab.IsEnabled)
                        return tab;
                }
            }

            return null;
        }

        #endregion //GetTabItemAtIndex	
        
		// AS 11/27/07 BR28724
		#region InitializeDropdownLocationInfo
		private void InitializeDropdownLocationInfo()
		{
			// get the tab item for the selected item
			object selectedItem = this.SelectedItem;
			TabItem selectedTabItem = selectedItem == null ? null : this.ItemContainerGenerator.ContainerFromItem(selectedItem) as TabItem;

			// get the element that determines the extent of the tab area and therefore the popup
			FrameworkElement headerArea = this.GetTemplateChild("PART_HeaderArea") as FrameworkElement ?? this;

			bool isOpenWithMouse = Mouse.LeftButton == MouseButtonState.Pressed && this.IsMouseOver;

			// the mouse could be down on something else that caused the dropdown. we really only
			// care about the mouse position so we can know whether to consider the mouse point
			// which itself only matters if a this item spans multiple monitors
			if (isOpenWithMouse && selectedTabItem is TabItemEx)
			{
				HitTestResult hr = VisualTreeHelper.HitTest(selectedTabItem, Mouse.GetPosition(selectedTabItem));

				if (null == hr || hr.VisualHit == null)
					isOpenWithMouse = false;
			}
			else
			{
				// if there is no container for the tab then it can't be opened with the mouse
				isOpenWithMouse = false;
			}

			#region Calculate Relative Point

			Point relativePoint;
			Size headerAreaSize = new Size(headerArea.ActualWidth, headerArea.ActualHeight);

			// get the rect of the header area in screen coordinates
			Point headerAreaScreenPoint = Utilities.PointToScreenSafe(headerArea, new Point());
			// AS 12/7/07 RightToLeft
			//Rect headerAreaScreenRect = new Rect(headerAreaScreenPoint, headerAreaSize);
			Point headerAreaScreenPointBottomRight = Utilities.PointToScreenSafe(headerArea, new Point(headerAreaSize.Width, headerAreaSize.Height));
			Rect headerAreaScreenRect = Utilities.RectFromPoints(headerAreaScreenPoint, headerAreaScreenPointBottomRight);

			// if it was opened with the mouse then use that point regardless of how
			// much of the this exists within that screen
			if (isOpenWithMouse)
				relativePoint = Mouse.GetPosition(headerArea);
			else
			{
				// otherwise get the screen for the corners of the header area
				Rect workRectTopLeft = Utilities.GetWorkArea(headerArea, new Point(), null);
				Rect workRectBottomRight = Utilities.GetWorkArea(headerArea, new Point(headerArea.ActualWidth, headerArea.ActualHeight), null);

				// get the portion of the rect in that screen
				workRectTopLeft.Intersect(headerAreaScreenRect);
				workRectBottomRight.Intersect(headerAreaScreenRect);

				// get the total area within each screen
				double topLeftArea = workRectTopLeft.Width * workRectTopLeft.Height;
				double bottomRightArea = workRectBottomRight.Width * workRectBottomRight.Height;

				// use the appropriate point based on which contains more of the this item
				relativePoint = topLeftArea >= bottomRightArea ? workRectTopLeft.Location : workRectBottomRight.Location;

				// get it in relative coordinates
				relativePoint = Utilities.PointFromScreenSafe(headerArea, relativePoint);
			}

			#endregion //Calculate Relative Point

			// now we know what screen point to use, find the screen rect
			Rect workRect = Utilities.GetWorkArea(headerArea, relativePoint, null);

			// find the portion of the header in that screen
			Rect availableScreenRect = Rect.Intersect(headerAreaScreenRect, workRect);

			// AS 6/27/11 TFS78202
			// The availableScreenRect is in screen coordinates but the preferredDropDownExtent is 
			// used to control the width of the Popup and therefore needs to be in logical units 
			// - not device units so we'll do this below where we can use the relative units.
			//
			//// now we know how wide the popup should be
			//double preferredDropDownExtent = this.TabStripPlacement == Dock.Left || this.TabStripPlacement == Dock.Right
			//    ? availableScreenRect.Height
			//    : availableScreenRect.Width;

			// convert this to coordinates relative to the header area
			Point availableClientPt = Utilities.PointFromScreenSafe(headerArea, availableScreenRect.Location);
			// AS 12/7/07 RightToLeft
			//Rect availableClientRect = new Rect(availableClientPt, availableScreenRect.Size);
			Point availableClientPtBottomRight = Utilities.PointFromScreenSafe(headerArea, new Point(availableScreenRect.Right, availableScreenRect.Bottom));
			Rect availableClientRect = Utilities.RectFromPoints(availableClientPt, availableClientPtBottomRight);

			// AS 6/27/11 TFS78202
			// now we know how wide the popup should be
			double preferredDropDownExtent = this.TabStripPlacement == Dock.Left || this.TabStripPlacement == Dock.Right
				? availableClientRect.Height
				: availableClientRect.Width;

			// update the cached values
			this.SetValue(XamTabControl.PreferredDropDownExtentPropertyKey, preferredDropDownExtent);

			// if the popup is using our custom placement callback then we need to replace the target element
			// with our element. otherwise the popup class will position the popup with respect to whichever
			// monitor contains more of the placement element. our position calculation is based on which 
			// monitor the mouse was in if it was opened with the mouse otherwise its the larger of the 
			// two sides of the header area
			if (null != this._popup && this._popup.CustomPopupPlacementCallback == this.PreferredDropDownPlacementCallback && this._popup.Placement == PlacementMode.Custom)
			{
				if (this._popupPlacementAdorner == null)
					this._popupPlacementAdorner = new PopupPlacementAdorner(this);

				if (this._popupPlacementAdorner.Parent is AdornerLayer == false)
				{
					AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(this);
					// JM 10-13-08 TFS8859 - check for null.
					if (null != adornerLayer)
						adornerLayer.Add(this._popupPlacementAdorner);
				}
				else
				{
					// AS 12/7/07 RightToLeft
					// The flow direction may have changed so be sure to rearrange the adorners just in case
					((AdornerLayer)this._popupPlacementAdorner.Parent).InvalidateArrange();
				}

				// position the adorner where the portion of the header area where we want the popup to be displayed
				this._popupPlacementAdorner.Rect = headerArea.TransformToVisual(this).TransformBounds(availableClientRect);

				// cache the old placement target so we can restore it when the popup closes
				if (this._popup.PlacementTarget is PopupPlacementAdorner == false)
				{
					this._oldPlacementTarget = this._popup.ReadLocalValue(Popup.PlacementTargetProperty);
					this._popup.PlacementTarget = this._popupPlacementAdorner;
				}
			}
		}
		#endregion //InitializeDropdownLocationInfo

		// AS 7/1/08 BR33755/BR33686
		#region OnAfterContainersGenerated
		private void OnAfterContainersGenerated(object context)
		{
			this._suppressSelectionChangeCount--;
		} 
		#endregion //OnAfterContainersGenerated

		#region OnComputedScrollBarVisibilityChanged
		private static void OnComputedScrollBarVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			XamTabControl tab = d as XamTabControl;

			if (null != tab)
			{
				Visibility horzVis = (Visibility)tab.GetValue(ComputedTabItemHorizontalScrollBarVisibilityProperty);
				Visibility vertVis = (Visibility)tab.GetValue(ComputedTabItemVerticalScrollBarVisibilityProperty);

				bool areScrollElementsVis = horzVis == Visibility.Visible || vertVis == Visibility.Visible;

				tab.SetValue(IsTabItemPanelScrollingPropertyKey, KnownBoxes.FromValue(areScrollElementsVis));
			}
		} 
		#endregion //OnComputedScrollBarVisibilityChanged

		#region OnGeneratorStatusChanged
		private void OnGeneratorStatusChanged(object sender, EventArgs e)
		{
			// AS 7/1/08 BR33755/BR33686
			if (this.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
			{
                if (this.IsMinimized)
                {
                    // AS 7/9/08 BR33755/BR33686
                    // This was test code that was accidentally checked in.
                    //
                    //return;

                    this._suppressSelectionChangeCount++;

                    this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Send,
                        new System.Threading.ContextCallback(OnAfterContainersGenerated), null);
                }
                else if (this.HasItems && this.SelectedIndex < 0)
                {
                    // AS 7/10/08 BR34741
                    // In the tab control's handling of the GeneratorStatusChanged, it will
                    // select the first tab regardless of the visibility so we have to workaround
                    // this by selecting another tab before they handle the event.
                    //

                    // JJD 9/11/08
                    // Use GetFirstTabItemToSelect method instead which will skip over disabled tabs
                    #region Old code

                    //for (int i = 0, count = this.Items.Count; i < count; i++)
                    //{
                    //    TabItem tab = this.ItemContainerGenerator.ContainerFromIndex(i) as TabItem;

                    //    if (null != tab && tab.Visibility != Visibility.Collapsed)
                    //    {
                    //        this.SelectedIndex = i;
                    //        break;
                    //    }
                    //}

                    #endregion //Old code
                    TabItem tabToSelect = this.GetFirstTabItemToSelect();

					if (tabToSelect != null)
					{
						// AS 5/10/12 TFS111333
						//tabToSelect.IsSelected = true;
						DependencyPropertyUtilities.SetCurrentValue(tabToSelect, TabItem.IsSelectedProperty, KnownBoxes.TrueBox);
					}
                }
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

		// AS 11/27/07 BR28724
		#region OnPopupClosed
		private void OnPopupClosed(object sender, EventArgs e)
		{
			Popup popup = sender as Popup;

			if (this._oldPlacementTarget != NotInitialized)
			{
				// if the value was not set...
				if (this._oldPlacementTarget == DependencyProperty.UnsetValue)
					popup.ClearValue(Popup.PlacementTargetProperty);
				else if (this._oldPlacementTarget is BindingBase)
					popup.SetBinding(Popup.PlacementTargetProperty, (BindingBase)this._oldPlacementTarget);
				else
					popup.SetValue(Popup.PlacementTargetProperty, this._oldPlacementTarget);
			}

			this._oldPlacementTarget = NotInitialized;
		}
		#endregion //OnPopupClosed

		#region OnPopupPreviewMouseDown_ClassHandler
		
#region Infragistics Source Cleanup (Region)




































































#endregion // Infragistics Source Cleanup (Region)

		#endregion //OnPopupPreviewMouseDown_ClassHandler	

		#region OnPopupPreviewMouseUp_ClassHandler

		private static void OnPopupPreviewMouseUp_ClassHandler(object sender, MouseButtonEventArgs e)
		{
			if (e.ChangedButton != MouseButton.Left)
				return;

            Popup popup = sender as Popup;

            if ( popup == null )
                return;

            // JJD 8/29/08
            // Get the templatedpanet from the sender to ensure we don't accidently eat the mouseup for any
            // other descendant popup
			//XamTabControl tabControl = Utilities.GetAncestorFromType(sender as DependencyObject, typeof(XamTabControl), true) as XamTabControl;
			XamTabControl tabControl = popup.TemplatedParent as XamTabControl;

			if (tabControl == null)
				return;

			if (!tabControl.IsMinimized ||
				!tabControl.IsDropDownOpen)
				return;

			Point point = e.GetPosition(tabControl);

			DependencyObject elementUnderMouse = tabControl.InputHitTest(point) as DependencyObject;

			if (elementUnderMouse == null)
				return;

			TabItem tabItem = Utilities.GetAncestorFromType(elementUnderMouse, typeof(TabItem), true, tabControl) as TabItem;

			if (tabItem == null)
				return;

            // JJD 8/29/08
            // The contains method returns false unless the TabItem was added directly to the tabcontrol.
            // Use IndexFromContainer instead.
            //if (!tabControl.Items.Contains(tabItem))
            int index = tabControl.ItemContainerGenerator.IndexFromContainer(tabItem);
            if (index < 0)
                return;
 
			// if the tab control is minimized then the popup will automatically close up when it gets
			// the mouse up so we need to eat the mouse up when you release the mouse over the selected item

            // JJD 8/29/08
            // Use SelectedIndex instead since the tabitem will never be the SelectedItem unless the TabItem
            // was added directly to the tabcontrol
			//if (tabItem == tabControl.SelectedItem)
			if (index == tabControl.SelectedIndex)
				e.Handled = true;

		}

		#endregion //OnPopupPreviewMouseUp_ClassHandler	
    
		#region OnTabStripPlacementChanged
		private static void OnTabStripPlacementChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			XamTabControl tab = d as XamTabControl;

			if (null != tab)
				tab.VerifyScrollBarVisibility();
		}
		#endregion //OnTabStripPlacementChanged

        // JJD 8/22/08 - added automation support for XamTabControl's ExpandCollapse pattern
        #region RaiseAutomationExpandCollapseStateChanged

        private void RaiseAutomationExpandCollapseStateChanged()
        {
            // AS 6/8/07 UI Automation
            XamTabControlAutomationPeer peer = UIElementAutomationPeer.FromElement(this) as XamTabControlAutomationPeer;

            if (null != peer)
                peer.RaiseExpandCollapseStateChanged();
        }

        #endregion //RaiseAutomationExpandCollapseStateChanged	

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
    
		#region ResetSelectedItem
		private void ResetSelectedItem()
		{
			if (this.IsMinimized && this.IsDropDownOpen == false && this.SelectedItem != null)
			{
				// store the previously selected item in case it
				// is unminimized without selecting a tab
				this._previouslySelectedItem = this.SelectedItem;

				TabItem tab = this.ItemContainerGenerator.ContainerFromItem(this._previouslySelectedItem) as TabItem;

				// clear the selected item
				this.ClearValue(Selector.SelectedItemProperty);

				if (tab != null)
				{
					// if focus is on the tab item then it will not select itself
					// when it is clicked. also, when the window gets focus the next
					// time, the tab item will reselect it self when it gets the 
					// keyboard focus. to prevent this, we need to shift focus to
					// another element.
					if (tab.IsKeyboardFocusWithin)
					{
						// AS 10/18/07
						// If the tab is focusable then focus the tab item itself.
						//
						if (tab.Focusable)
							tab.Focus();
						else
							this.Focus();
					}
					else
					{
						DependencyObject focusScope = FocusManager.GetFocusScope(tab);

						if (null != focusScope && focusScope == FocusManager.GetFocusScope(this))
						{
							// AS 10/19/10 TFS42668
							//FocusManager.SetFocusedElement(focusScrope, this);
							Utilities.SetFocusedElement(focusScope, this);
						}
					}
				}
			}
			else if (this.SelectedItem == null && (this.IsMinimized == false || this.IsDropDownOpen == true))
			{
				// JM 10-13-08 TFS8792
				if (this._previouslySelectedItem == null && this.Items.Count > 0)
					this._previouslySelectedItem = this.GetFirstVisibleTabItem();

				if (this.ItemContainerGenerator.ContainerFromItem(this._previouslySelectedItem) != null)
				{
					this.SelectedItem = this._previouslySelectedItem;
				}
				

				this._previouslySelectedItem = null;
			}
		}
		#endregion //ResetSelectedItem

        #region UpdateThemeResources

        private void UpdateThemeResources()
        {
            string[] groupings = new string[] { PrimitivesGeneric.Location.Grouping };

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
				this.UpdateLayout();
			}
        }

        #endregion //UpdateThemeResources	

		#region VerifyScrollBarVisibility
		private void VerifyScrollBarVisibility()
		{
			object vertVis, horzVis;

			if (this.IsMultiRow != this.HasVerticalTabs)
			{
 				vertVis = KnownBoxes.ScrollBarVisibilityAutoBox;
				horzVis = KnownBoxes.ScrollBarVisibilityDisabledBox;
			}
			else
			{
				vertVis = KnownBoxes.ScrollBarVisibilityDisabledBox;
				horzVis = KnownBoxes.ScrollBarVisibilityAutoBox;
			}

			this.SetValue(TabItemHorizontalScrollBarVisibilityPropertyKey, horzVis);
			this.SetValue(TabItemVerticalScrollBarVisibilityPropertyKey, vertVis);
		}
		#endregion //VerifyScrollBarVisibility

		// AS 11/26/07 BR28697
		#region VerifyMinimizedContentBinding
		private void VerifyMinimizedContentBinding()
		{
			bool isMinimized = this.IsMinimized;
			ContentPresenter cpMinimized = this._cpMinimized;
			ContentPresenter cpMaximized = this._cpMaximized;

			if (null != cpMinimized && null != cpMaximized)
			{
				ContentPresenter cpSource = isMinimized ? cpMaximized : cpMinimized;
				ContentPresenter cpDest = isMinimized ? cpMinimized : cpMaximized;

				cpSource.Content = null;
				cpDest.SetBinding(ContentPresenter.ContentProperty, Utilities.CreateBindingObject(TabControl.SelectedContentProperty, BindingMode.OneWay, this));
			}
		}
		#endregion //VerifyMinimizedContentBinding

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

		#endregion //Private

		#endregion //Methods

		#region Events

		#region DropDownClosed

		/// <summary>
		/// Event ID for the <see cref="DropDownClosed"/> routed event
		/// </summary>
		/// <seealso cref="DropDownClosed"/>
		/// <seealso cref="IsDropDownOpen"/>
		/// <seealso cref="OnDropDownClosed"/>
		public static readonly RoutedEvent DropDownClosedEvent =
			EventManager.RegisterRoutedEvent("DropDownClosed", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(XamTabControl));

		/// <summary>
		/// Occurs after the dropdown of the minimized <see cref="XamTabControl"/> has been closed
		/// </summary>
		/// <seealso cref="DropDownClosed"/>
		/// <seealso cref="IsDropDownOpen"/>
		/// <seealso cref="DropDownClosedEvent"/>
		protected virtual void OnDropDownClosed(RoutedEventArgs args)
		{
			this.RaiseEvent(args);
		}

		internal void RaiseDropDownClosed(RoutedEventArgs args)
		{
			args.RoutedEvent = XamTabControl.DropDownClosedEvent;
			args.Source = this;
			this.OnDropDownClosed(args);
		}

		/// <summary>
		/// Occurs after the tool has been closed
		/// </summary>
		/// <seealso cref="OnDropDownClosed"/>
		/// <seealso cref="DropDownClosedEvent"/>
		//[Description("Occurs after the dropdown of the XamTabControl has been closed")]
		//[Category("Behavior")]
		public event RoutedEventHandler DropDownClosed
		{
			add
			{
				base.AddHandler(XamTabControl.DropDownClosedEvent, value);
			}
			remove
			{
				base.RemoveHandler(XamTabControl.DropDownClosedEvent, value);
			}
		}

		#endregion //DropDownClosed

		#region DropDownOpened

		/// <summary>
		/// Event ID for the <see cref="DropDownOpened"/> routed event
		/// </summary>
		/// <seealso cref="DropDownOpened"/>
		/// <seealso cref="IsDropDownOpen"/>
		/// <seealso cref="OnDropDownOpened"/>
		public static readonly RoutedEvent DropDownOpenedEvent =
			EventManager.RegisterRoutedEvent("DropDownOpened", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(XamTabControl));

		/// <summary>
		/// Occurs after the tool has been opened
		/// </summary>
		/// <seealso cref="DropDownOpened"/>
		/// <seealso cref="IsDropDownOpen"/>
		/// <seealso cref="DropDownOpenedEvent"/>
		protected virtual void OnDropDownOpened(RoutedEventArgs args)
		{
			this.RaiseEvent(args);
		}

		internal void RaiseDropDownOpened(RoutedEventArgs args)
		{
			args.RoutedEvent = XamTabControl.DropDownOpenedEvent;
			args.Source = this;
			this.OnDropDownOpened(args);
		}

		/// <summary>
		/// Occurs after the dropdown of the minimized <see cref="XamTabControl"/> has been opened
		/// </summary>
		/// <seealso cref="OnDropDownOpened"/>
		/// <seealso cref="DropDownOpenedEvent"/>
		//[Description("Occurs after the dropdown of the minimized XamTabControl has been opened")]
		//[Category("Behavior")]
		public event RoutedEventHandler DropDownOpened
		{
			add
			{
				base.AddHandler(XamTabControl.DropDownOpenedEvent, value);
			}
			remove
			{
				base.RemoveHandler(XamTabControl.DropDownOpenedEvent, value);
			}
		}

		#endregion //DropDownOpened

		#region DropDownOpening

		/// <summary>
		/// Event ID for the <see cref="DropDownOpening"/> routed event
		/// </summary>
		/// <seealso cref="DropDownOpening"/>
		/// <seealso cref="IsDropDownOpen"/>
		/// <seealso cref="OnDropDownOpening"/>
		public static readonly RoutedEvent DropDownOpeningEvent =
			EventManager.RegisterRoutedEvent("DropDownOpening", RoutingStrategy.Bubble, typeof(EventHandler<TabControlDropDownOpeningEventArgs>), typeof(XamTabControl));

		/// <summary>
		/// Occurs before the tool has been opened
		/// </summary>
		/// <seealso cref="DropDownOpening"/>
		/// <seealso cref="IsDropDownOpen"/>
		/// <seealso cref="DropDownOpeningEvent"/>
		protected virtual void OnDropDownOpening(TabControlDropDownOpeningEventArgs args)
		{
			this.RaiseEvent(args);
		}

		private void RaiseDropDownOpening(TabControlDropDownOpeningEventArgs args)
		{
			args.RoutedEvent = XamTabControl.DropDownOpeningEvent;
			args.Source = this;

			this.OnDropDownOpening(args);
		}

		/// <summary>
		/// Occurs before the dropdown of the <see cref="XamTabControl"/> has been opened
		/// </summary>
		/// <seealso cref="OnDropDownOpening"/>
		/// <seealso cref="DropDownOpeningEvent"/>
		//[Description("Occurs before the dropdown of the XamTabControl has been opened")]
		//[Category("Behavior")]
		public event EventHandler<TabControlDropDownOpeningEventArgs> DropDownOpening
		{
			add
			{
				base.AddHandler(XamTabControl.DropDownOpeningEvent, value);
			}
			remove
			{
				base.RemoveHandler(XamTabControl.DropDownOpeningEvent, value);
			}
		}

		#endregion //DropDownOpening

        #region ExecutingCommand

        /// <summary>
        /// Event ID for the <see cref="ExecutingCommand"/> routed event
        /// </summary>
        /// <seealso cref="ExecutingCommand"/>
        /// <seealso cref="OnExecutingCommand"/>
        /// <seealso cref="ExecutingCommandEventArgs"/>
        public static readonly RoutedEvent ExecutingCommandEvent =
            EventManager.RegisterRoutedEvent("ExecutingCommand", RoutingStrategy.Bubble, typeof(EventHandler<ExecutingCommandEventArgs>), typeof(XamTabControl));

        /// <summary>
        /// Occurs before a command is executed.
        /// </summary>
        /// <remarks><para class="body">This event is cancellable.</para></remarks>
        /// <seealso cref="ExecuteCommand(RoutedCommand)"/>
        /// <seealso cref="ExecutedCommand"/>
        /// <seealso cref="ExecutingCommand"/>
        /// <seealso cref="ExecutingCommandEvent"/>
        /// <seealso cref="ExecutingCommandEventArgs"/>
        protected virtual void OnExecutingCommand(ExecutingCommandEventArgs args)
        {
            this.RaiseEvent(args);
        }

        internal bool RaiseExecutingCommand(ExecutingCommandEventArgs args)
        {
            args.RoutedEvent = XamTabControl.ExecutingCommandEvent;
            args.Source = this;
            this.OnExecutingCommand(args);

            return args.Cancel == false;
        }

        /// <summary>
        /// Occurs before a command is executed
        /// </summary>
        /// <remarks><para class="body">This event is cancellable.</para></remarks>
        /// <seealso cref="ExecuteCommand(RoutedCommand)"/>
        /// <seealso cref="OnExecutingCommand"/>
        /// <seealso cref="ExecutingCommandEvent"/>
        /// <seealso cref="ExecutingCommandEventArgs"/>
        //[Description("Occurs before a command is performed")]
        //[Category("DockManager Events")] // Action
        public event EventHandler<ExecutingCommandEventArgs> ExecutingCommand
        {
            add
            {
                base.AddHandler(XamTabControl.ExecutingCommandEvent, value);
            }
            remove
            {
                base.RemoveHandler(XamTabControl.ExecutingCommandEvent, value);
            }
        }

        #endregion //ExecutingCommand

        #region ExecutedCommand

        /// <summary>
        /// Event ID for the <see cref="ExecutedCommand"/> routed event
        /// </summary>
        /// <seealso cref="ExecuteCommand(RoutedCommand)"/>
        /// <seealso cref="ExecutedCommand"/>
        /// <seealso cref="OnExecutedCommand"/>
        /// <seealso cref="ExecutedCommandEventArgs"/>
        public static readonly RoutedEvent ExecutedCommandEvent =
            EventManager.RegisterRoutedEvent("ExecutedCommand", RoutingStrategy.Bubble, typeof(EventHandler<ExecutedCommandEventArgs>), typeof(XamTabControl));

        /// <summary>
        /// Occurs after a command is executed
        /// </summary>
        /// <seealso cref="ExecuteCommand(RoutedCommand)"/>
        /// <seealso cref="ExecutingCommand"/>
        /// <seealso cref="ExecutedCommand"/>
        /// <seealso cref="ExecutedCommandEvent"/>
        /// <seealso cref="ExecutedCommandEventArgs"/>
        protected virtual void OnExecutedCommand(ExecutedCommandEventArgs args)
        {
            this.RaiseEvent(args);
        }

        internal void RaiseExecutedCommand(ExecutedCommandEventArgs args)
        {
            args.RoutedEvent = XamTabControl.ExecutedCommandEvent;
            args.Source = this;
            this.OnExecutedCommand(args);
        }

        /// <summary>
        /// Occurs after a command is executed
        /// </summary>
        /// <seealso cref="ExecuteCommand(RoutedCommand)"/>
        /// <seealso cref="OnExecutedCommand"/>
        /// <seealso cref="ExecutingCommand"/>
        /// <seealso cref="ExecutedCommandEvent"/>
        /// <seealso cref="ExecutedCommandEventArgs"/>
        //[Description("Occurs after a command is performed")]
        //[Category("DockManager Events")] // Action
        public event EventHandler<ExecutedCommandEventArgs> ExecutedCommand
        {
            add
            {
                base.AddHandler(XamTabControl.ExecutedCommandEvent, value);
            }
            remove
            {
                base.RemoveHandler(XamTabControl.ExecutedCommandEvent, value);
            }
        }

        #endregion //ExecutedCommand

		#endregion //Events
		
		#region ResourceKeys

			#region CloseButtonStyleKey

			/// <summary>
			/// The key used to identify the style used for the close button.
			/// </summary>
			public static readonly ResourceKey CloseButtonStyleKey = new StaticPropertyResourceKey(typeof(XamTabControl), "CloseButtonStyleKey");

			#endregion //CloseButtonStyleKey

			#region MultiRowTabItemPagerStyleKey

			/// <summary>
			/// The key used to identify the style used for the XamPager that contains the tab items when the TabLayoutStyle is set to one of the 'MultiRow...' settings.
			/// </summary>
            /// <seealso cref="XamPager"/>
            /// <seealso cref="XamTabControl.TabLayoutStyle"/>
			public static readonly ResourceKey MultiRowTabItemPagerStyleKey = new StaticPropertyResourceKey(typeof(XamTabControl), "MultiRowTabItemPagerStyleKey");

			#endregion //MultiRowTabItemPagerStyleKey

			#region ScrollDownButtonStyleKey

			/// <summary>
            /// The key used to identify the style used for the for the XamPager's scroll down button.
			/// </summary>
            /// <seealso cref="XamPager"/>
            /// <seealso cref="XamPager.ScrollDownButtonStyle"/>
			public static readonly ResourceKey ScrollDownButtonStyleKey = new StaticPropertyResourceKey(typeof(XamTabControl), "ScrollDownButtonStyleKey");

			#endregion //ScrollDownButtonStyleKey

			#region ScrollLeftButtonStyleKey

			/// <summary>
            /// The key used to identify the style used for the for the XamPager's scroll left button.
			/// </summary>
            /// <seealso cref="XamPager"/>
            /// <seealso cref="XamPager.ScrollLeftButtonStyle"/>
			public static readonly ResourceKey ScrollLeftButtonStyleKey = new StaticPropertyResourceKey(typeof(XamTabControl), "ScrollLeftButtonStyleKey");

			#endregion //ScrollLeftButtonStyleKey

			#region ScrollRightButtonStyleKey

			/// <summary>
            /// The key used to identify the style used for the for the XamPager's scroll right button.
			/// </summary>
            /// <seealso cref="XamPager"/>
            /// <seealso cref="XamPager.ScrollRightButtonStyle"/>
			public static readonly ResourceKey ScrollRightButtonStyleKey = new StaticPropertyResourceKey(typeof(XamTabControl), "ScrollRightButtonStyleKey");

			#endregion //ScrollRightButtonStyleKey

			#region ScrollUpButtonStyleKey

			/// <summary>
            /// The key used to identify the style used for the for the XamPager's scroll up button.
			/// </summary>
            /// <seealso cref="XamPager"/>
            /// <seealso cref="XamPager.ScrollUpButtonStyle"/>
			public static readonly ResourceKey ScrollUpButtonStyleKey = new StaticPropertyResourceKey(typeof(XamTabControl), "ScrollUpButtonStyleKey");

			#endregion //ScrollUpButtonStyleKey

			#region SingleRowTabItemPagerStyleKey

			/// <summary>
			/// The key used to identify the style used for the XamPager that contains the tab items when the TabLayoutStyle is set to one of the 'SingleRow...' settings.
			/// </summary>
            /// <seealso cref="XamPager"/>
            /// <seealso cref="XamTabControl.TabLayoutStyle"/>
			public static readonly ResourceKey SingleRowTabItemPagerStyleKey = new StaticPropertyResourceKey(typeof(XamTabControl), "SingleRowTabItemPagerStyleKey");

			#endregion //SingleRowTabItemPagerStyleKey

		#endregion ResourceKeys

			// AS 11/27/07 BR28724
		#region PopupPlacementAdorner 





        // JJD 8/20/08 - BR35341
        // Added AdornerEx abstract base class to handle adornerlayer re-creations based on template changes from higher level elements 
        //private class PopupPlacementAdorner : Adorner
		private class PopupPlacementAdorner : AdornerEx
		{
			#region Member Variables

			private Rect _rect = new Rect();

			#endregion //Member Variables

			#region Constructor
			internal PopupPlacementAdorner(XamTabControl tabControl)
				: base(tabControl)
			{
				// the element is not meant to be visible - just serve as a placeholder
				this.Visibility = Visibility.Hidden;
			}
			#endregion //Constructor

			#region Properties
			internal Rect Rect
			{
				get { return this._rect; }
				set
				{
					if (value != this._rect)
					{
						Debug.Assert(value.IsEmpty == false);

						if (value.IsEmpty)
							value = new Rect();

						this._rect = value;
						this.InvalidateMeasure();
					}
				}
			}
			#endregion //Properties

			#region Base class overrides
			protected override Size MeasureOverride(Size constraint)
			{
				return this._rect.Size;
			}

			public override GeneralTransform GetDesiredTransform(GeneralTransform transform)
			{
				GeneralTransformGroup group = new GeneralTransformGroup();
				group.Children.Add(transform);
				group.Children.Add(new TranslateTransform(this._rect.X, this._rect.Y));
				return group;
			}
			#endregion //Base class overrides
		} 
		#endregion //PopupPlacementAdorner 

	    // JJD 8/5/08 - added
        #region ICommandHost Members

        bool ICommandHost.Execute(ExecuteCommandInfo commandInfo)
        {
            RoutedCommand command = commandInfo.RoutedCommand;
            return null != command && this.ExecuteCommandImpl(commandInfo);
        }

		// SSP 3/18/10 TFS29783 - Optimizations
		// Changed CurrentState property to a method.
		// 
        //long ICommandHost.CurrentState
		long ICommandHost.GetCurrentState( long statesToQuery )
		{
			//get 
			//{
			TabControlStates state = 0;

			if ( this.AllowMinimize )
				state |= TabControlStates.AllowMinimized;

			if ( this.IsMinimized )
				state |= TabControlStates.Minimized;

			TabItem selectedTab = this.GetSelectedTabItem( );

			if ( selectedTab != null )
			{
				state |= TabControlStates.HasSelectedTab;

				if ( selectedTab is TabItemEx &&
					( (TabItemEx)selectedTab ).AllowClosingResolved )
					state |= TabControlStates.SelectedTabAllowsClosing;

			}

			TabItem tiFirst = this.GetFirstTabItem( true, true );

			if ( tiFirst != null )
			{
				state |= TabControlStates.HasSelectableTab;

				if ( selectedTab == tiFirst )
					state |= TabControlStates.FirstTabSelected;

				TabItem tiLast = this.GetLastTabItem( true, true );

				if ( selectedTab == tiLast )
					state |= TabControlStates.LastTabSelected;
			}

			return (long)state;
			//}
		}

        bool ICommandHost.CanExecute(ExecuteCommandInfo commandInfo)
        {
            RoutedCommand command = commandInfo.RoutedCommand;

            if (null == command)
                return false;

            if (command == TabControlCommands.ToggleMinimized)
                return this.AllowMinimize;

            if (command == TabControlCommands.Expand)
                return this.IsMinimized;

            if (command == TabControlCommands.Minimize)
                return this.AllowMinimize && !this.IsMinimized;

            if (command == TabItemExCommands.Close ||
                command == TabControlCommands.CloseSelected)
            {
                TabItemEx tab = this.GetSelectedTabItem() as TabItemEx;

                return tab != null && tab.AllowClosingResolved;
            }

            TabItem firstVisibleTabItem = this.GetFirstTabItem(false, true);

            if ( command == TabControlCommands.CloseAll )
                return firstVisibleTabItem != null;

            if (firstVisibleTabItem != null)
            {
                TabItem firstTab = this.GetFirstTabItem(true, true);

                if (firstTab != null)
                {
                    TabItem selectedTab = this.GetSelectedTabItem();

                    if (command == TabItemExCommands.CloseAllButThis)
                        return selectedTab != null;

                    if ( command == TabControlCommands.SelectFirstTab )
                        return firstTab != selectedTab;
                
                    TabItem lastTab = this.GetLastTabItem(true, true);

                    if ( command == TabControlCommands.SelectLastTab )
                        return lastTab != selectedTab;

                    if (selectedTab != null)
                    {
                        if (command == TabControlCommands.SelectNextTab)
                            return lastTab != selectedTab;

                        if (command == TabControlCommands.SelectPreviousTab)
                            return firstTab != selectedTab;
                    }
                }
            }
            return false;
        }

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