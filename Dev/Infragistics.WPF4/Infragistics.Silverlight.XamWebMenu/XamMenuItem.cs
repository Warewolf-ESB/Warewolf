using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using Infragistics.AutomationPeers;
using Infragistics.Controls.Menus.Primitives;


using System.Collections.Generic;


namespace Infragistics.Controls.Menus
{
    /// <summary>
    /// Represents a selectable item inside a <see cref="XamMenu"/> control.
    /// </summary>
    /// <remarks>
    /// <p class="body">A <b>XamMenuItem</b> can have submenus. The submenu of the <b>XamMenuItem</b> is made up of the objects within
    /// the ItemCollection of a <b>XamMenuItem</b>. It is common for a <b>XamMenuItem</b> to contain other <b>XamMenuItem</b> objects to create 
    /// nested submenus.</p>
    /// </remarks>
    [TemplatePart(Name = XamMenuItem.Popup, Type = typeof(Popup))]
    [TemplatePart(Name = XamMenuItem.ScrollViewer, Type = typeof(ScrollViewer))]
    [TemplateVisualState(GroupName = XamMenuItem.CommonStatesGroupName, Name = XamMenuItem.Normal)]
    [TemplateVisualState(GroupName = XamMenuItem.CommonStatesGroupName, Name = XamMenuItem.MouseOver)]
    [TemplateVisualState(GroupName = XamMenuItem.CommonStatesGroupName, Name = XamMenuItem.Highlighted)]
    [TemplateVisualState(GroupName = XamMenuItem.CommonStatesGroupName, Name = XamMenuItem.Disabled)]
    [TemplateVisualState(GroupName = XamMenuItem.SubmenuStateGroupName, Name = XamMenuItem.SubmenuOpen)]
    [TemplateVisualState(GroupName = XamMenuItem.SubmenuStateGroupName, Name = XamMenuItem.SubmenuOpenImmediately)]
    [TemplateVisualState(GroupName = XamMenuItem.SubmenuStateGroupName, Name = XamMenuItem.SubmenuClose)]
    [TemplateVisualState(GroupName = XamMenuItem.SubmenuStateGroupName, Name = XamMenuItem.SubmenuCloseImmediately)]
    [TemplateVisualState(GroupName = XamMenuItem.MenuItemRoleGroupName, Name = XamMenuItem.SubmenuHeader)]
    [TemplateVisualState(GroupName = XamMenuItem.MenuItemRoleGroupName, Name = XamMenuItem.SubmenuItem)]
    [TemplateVisualState(GroupName = XamMenuItem.MenuItemRoleGroupName, Name = XamMenuItem.TopLevelHeader)]
    [TemplateVisualState(GroupName = XamMenuItem.MenuItemRoleGroupName, Name = XamMenuItem.TopLevelHeaderWithIcon)]
    [TemplateVisualState(GroupName = XamMenuItem.MenuItemRoleGroupName, Name = XamMenuItem.TopLevelItem)]
    [TemplateVisualState(GroupName = XamMenuItem.MenuItemRoleGroupName, Name = XamMenuItem.TopLevelItemWithIcon)]
    public class XamMenuItem : XamHeaderedItemsControl, INotifyPropertyChanged
    {
        #region Constants
        private const string CommonStatesGroupName = "CommonStates";
        private const string MouseOver = "MouseOver";
        private const string Normal = "Normal";
        private const string Highlighted = "Highlighted";
        private const string Disabled = "Disabled";

        private const string SubmenuStateGroupName = "SubmenuState";
        private const string SubmenuOpen = "SubmenuOpen";
        private const string SubmenuOpenImmediately = "SubmenuOpenImmediately";
        private const string SubmenuClose = "SubmenuClose";
        private const string SubmenuCloseImmediately = "SubmenuCloseImmediately";

        private const string MenuItemRoleGroupName = "MenuItemRole";
        private const string TopLevelHeader = "TopLevelHeader";
        private const string TopLevelHeaderWithIcon = "TopLevelHeaderWithIcon";
        private const string SubmenuHeader = "SubmenuHeader";
        private const string SubmenuItem = "SubmenuItem";
        private const string TopLevelItem = "TopLevelItem";
        private const string TopLevelItemWithIcon = "TopLevelItemWithIcon";

        private const string Popup = "Popup";
        private const string ScrollViewer = "ScrollViewer";
        #endregion

        #region Members

        private ItemsControl _parentItemsControl;
        private Popup _dropDownPopup;
        private ScrollViewer _scrollViewer;
        private bool _isImmediately = true;
        private bool _isMenuOrientationNotSet;
        private const double MAX_POPUPHEIGHT_FROM_ROOTHEIGHT = 0.97;
        private bool _hasMouseCapture;
        private ContentControl _headerIconPresenter;
        private bool _closeStoreboardInProcess, _closeImmediatelyStoreboardInProcess;

        private bool _isDesignMode = false;


        private VisualState _subMenuClosedVisualState, _subMenuClosedImmediatelyVisualState;

        #endregion // Members

        #region Constructors


        /// <summary>
        /// Static constructor of the <see cref="XamMenuItem"/> class. 
        /// </summary>
        static XamMenuItem()
        {
            Style style = new Style();
            style.Seal();
            Control.FocusVisualStyleProperty.OverrideMetadata(typeof(XamMenuItem), new FrameworkPropertyMetadata(style));
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="XamMenuItem"/> class.
        /// </summary>
        public XamMenuItem()
        {

            Infragistics.Windows.Utilities.ValidateLicense(typeof(XamMenuItem), this);

            this._isDesignMode = DesignerProperties.GetIsInDesignMode(this);

            DefaultStyleKey = typeof(XamMenuItem);
            this.IsEnabledChanged += ControlIsEnabledChanged;
            this._navigator = new MenuNavigation();

            this.Unloaded += new RoutedEventHandler(XamMenuItem_Unloaded);
        }

        #endregion

        #region Base class override

        #region ArrangeOverride
        /// <summary>
        /// Arranges and sizes the auto complete control and its contents.
        /// </summary>
        /// <param name="finalSize">The provided arrangement bounds object.</param>
        /// <returns>Returns the arrangement bounds, unchanged.</returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
            if (this._isMenuOrientationNotSet)
            {
                StackPanel hostPanel = this.HeaderedItemContainerGenerator.ItemsHost as StackPanel;
                if (hostPanel != null && hostPanel.Orientation != this.MenuOrientation)
                {
                    hostPanel.Orientation = this.MenuOrientation;
                    // TFS13619 - the first set. When the panel is not visible we cant set it from property changed handler
                    this._isMenuOrientationNotSet = false;
                }
            }
            Size r = base.ArrangeOverride(finalSize);
            EnsureSubmenuPosition();

            SetDropdownPopupPlacement();

            return r;
        }
        #endregion

        #region OnApplyTemplate
        /// <summary>
        /// Invoked when the template for the element has been applied.
        /// </summary>
        public override void OnApplyTemplate()
        {

            AccessKeyManager.RemoveAccessKeyPressedHandler(this, this.HandleAccessKeyPressed);

            _scrollViewer = GetTemplateChild(ScrollViewer) as ScrollViewer;

            if (_dropDownPopup != null && _dropDownPopup.Child != null)
            {
                _dropDownPopup.Child.MouseEnter -= new MouseEventHandler(Items_MouseEnter);
                _dropDownPopup.Child.MouseLeave -= new MouseEventHandler(Items_MouseLeave);
                (_dropDownPopup.Child as FrameworkElement).SizeChanged -= new SizeChangedEventHandler(Popup_SizeChanged);
            }

            base.OnApplyTemplate();

            if (this._headerIconPresenter != null)
                this._headerIconPresenter.Content = null;

            this._headerIconPresenter = base.GetTemplateChild("HeaderIconPresenter") as ContentControl;

            if (this._headerIconPresenter != null)
                this._headerIconPresenter.Content = this.Icon;

            this._subMenuClosedVisualState = GetTemplateChild(SubmenuClose) as VisualState;
            this._subMenuClosedImmediatelyVisualState = GetTemplateChild(SubmenuCloseImmediately) as VisualState;



            _dropDownPopup = GetTemplateChild(Popup) as Popup;

            if (_dropDownPopup != null && _dropDownPopup.Child != null)
            {


                
                _dropDownPopup.PlacementTarget = this;
                _dropDownPopup.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                this.SetDropdownPopupPlacement();

                
                _dropDownPopup.AllowsTransparency = true;

            }


            EnsureMenuItemRole();
            ChangeVisualState(false);
            VisualStateManager.GoToState(this, SubmenuCloseImmediately, false);


            AccessKeyManager.AddAccessKeyPressedHandler(this, this.HandleAccessKeyPressed);

        }

        #endregion

        #region OnCreateAutomationPeer
        /// <summary>
        /// When implemented in a derived class, returns class-specific <see cref="T:System.Windows.Automation.Peers.AutomationPeer"/> implementations for the automation infrastructure.
        /// </summary>
        /// <returns>
        /// The class-specific <see cref="T:System.Windows.Automation.Peers.AutomationPeer"/> subclass to return.
        /// </returns>
        protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer()
        {
            return new XamMenuItemAutomationPeer(this);
        }
        #endregion //OnCreateAutomationPeer

        #region ItemsControl override
        #region GetContainerForItemOverride
        /// <summary>
        /// Creates a new XamMenuItem to use to display the object.
        /// </summary>
        /// <returns>A new XamMenuItem.</returns>
        protected override DependencyObject GetContainerForItemOverride()
        {
            XamMenuItem item = null;
            if (this.DefaultItemsContainer != null)
            {
                item = this.DefaultItemsContainer.LoadContent() as XamMenuItem;
            }

            if (item == null)
                return new XamMenuItem();
            else
                return item;
        }
        #endregion

        #region IsItemItsOwnContainerOverride
        /// <summary>
        /// Overrides the framework invocation testing if an item is already a container.
        /// </summary>
        /// <param name="item">The item to test.</param>
        /// <returns>Returns true if its already a <see cref="XamMenuItem"/> or <see cref="XamMenuSeparator"/></returns>
        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return (item is XamMenuItem) || (item is XamMenuSeparator);
        }
        #endregion

        #region PrepareContainerForItemOverride
        /// <summary>
        /// Prepares the specified element to display the specified item.
        /// </summary>
        /// <param name="element">
        /// Element used to display the specified item.
        /// </param>
        /// <param name="item">Specified item.</param>
        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            if (element is XamMenuSeparator)
            {
                return;
            }

            XamMenuItem node = element as XamMenuItem;
            if (node != null)
            {
                // Associate the Parent ItemsControl
                node.ParentItemsControl = this;
            }

            base.PrepareContainerForItemOverride(element, item);
        }
        #endregion

        #region ClearContainerForItemOverride
        /// <summary>
        /// Undoes the effects of PrepareContainerForItemOverride.
        /// </summary>
        /// <param name="element">The container element.</param>
        /// <param name="item">The contained item.</param>
        protected override void ClearContainerForItemOverride(DependencyObject element, object item)
        {
            if (element is XamMenuSeparator)
            {
                return;
            }

            XamMenuItem node = element as XamMenuItem;
            if (node != null)
            {
                node.ParentItemsControl = null;
            }

            base.ClearContainerForItemOverride(element, item);
        }
        #endregion

        #region OnItemsChanged
        /// <summary>
        /// Overrides the framework invocation when an item is added or removed for the objects in the tree.
        /// </summary>
        /// <param name="e">Data on what items changed.</param>
        protected override void OnItemsChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add ||
                e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove ||
                e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
            {
                EnsureMenuItemRole();
            }

            this.SetValue(XamMenuItem.HasChildrenProperty, this.Items.Count == 0 ? false : true);
        }

        #endregion

        #endregion

        #region Event handlers

        #region OnMouseEnter
        /// <summary>
        /// Called before the <see cref="UIElement.MouseEnter"/> event occurs.
        /// </summary>
        /// <param name="e">The data for the event</param>
        protected override void OnMouseEnter(MouseEventArgs e)
        {
            
            if (this.ParentXamMenu == null)
                return;

            if (this.IsInContextMenu == false)
            {
                XamMenu menu = this.ParentXamMenu as XamMenu;
                if (menu.ExpandOnHover && this.Role == MenuItemRole.TopLevelHeader)
                {
                    menu.StopTimer();
                }
            }

            base.OnMouseEnter(e);
            if (this.IsHighlighted == false)
            {
                this.IgnorePropertyChange = true;
                this.IsHighlighted = true;
            }

            IsMouseOver = true;
        }
        #endregion

        #region OnMouseLeave
        /// <summary>
        /// Called before the <see cref="UIElement.MouseLeave"/> event occurs.
        /// </summary>
        /// <param name="e">The data for the event</param>
        protected override void OnMouseLeave(MouseEventArgs e)
        {
            
            if (this.ParentXamMenu == null)
                return;

            
#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

            if (this.ParentXamMenuItem != null && this.ParentXamMenuItem.IgnoreMouseUp)
                this.ParentXamMenuItem.IgnoreMouseUp = false;


            if (this.IsInContextMenu == false)
            {
                XamMenu menu = this.ParentXamMenu as XamMenu;
                if (menu.ExpandOnHover && this.Role == MenuItemRole.TopLevelHeader)
                {
                    menu.StartTimer();
                }
            }

            if (this.IsHighlighted)
            {
                this.IgnorePropertyChange = true;
                this.IsHighlighted = false;
            }

            IsMouseOver = false;
        }
        #endregion

        #region OnMouseLeftButtonDown
        /// <summary>
        /// Overrides the framework invocation when a user clicks the left mouse button.
        /// </summary>
        /// <param name="e">Data about the mousedown event.</param>
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);






            if (this.ParentXamMenu == null)
            {
                return;
            }

            if (this.IsMouseOverSubling)
            {
                
                return;
            }

            if (this._scrollViewer.ComputedVerticalScrollBarVisibility == Visibility.Visible ||
                this._scrollViewer.ComputedHorizontalScrollBarVisibility == Visibility.Visible)
            {
                Rect mouse = new Rect(e.GetPosition(this), new Size(0, 0));
                Rect r = new Rect(this.RenderSize);
                if (!r.IntersectsWith(mouse))
                {
                    return;
                }
            }


            if (!e.Handled && IsEnabled)
            {
                if (this.Role == MenuItemRole.TopLevelItem)
                {
                    if (!this.IsSelected)
                    {
                        this.IsSelected = true;
                    }
                }
                else
                {
                    this.FocusOrSelect();
                }
            }

            if (this.Role == MenuItemRole.TopLevelHeader)
            {
                if (this.IsSubmenuOpen && ((XamMenu)this.ParentXamMenu).ExpandOnHover == false)
                {
                    this.ManipulateSubmenu(false, false);
                    this.ParentXamMenu.CloseCurrentOpen();
                    this.ParentXamMenu.ReturnFocus();
                }
                else
                {
                    this.ManipulateSubmenu(true, false);
                }
            }
            else if (this.Role == MenuItemRole.SubmenuHeader)
            {
                XamMenuItem currentOpenItem = this.SublingOpenItem;
                if (currentOpenItem != null)
                {
                    currentOpenItem.ManipulateSubmenu(false, true);
                    currentOpenItem = null;
                }

                this.ManipulateSubmenu(true, true);
            }

            



        }
        #endregion

        #region OnMouseLeftButtonUp
        /// <summary>
        /// Overrides the framework invocation when a user releases the left mouse button.
        /// </summary>
        /// <param name="e">Data about the left mouse button up event.</param>
        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);


            // If we're WPF, then clicking on a disabled item, doesn't raise any events
            // So, we need to walk up the visual tree, so that we don't invoke anything on the parent item
            // The check we make here is looking of popups, if we hit a popup without hitting ourselves
            // then it means we're dealing with a disabled menu item.
            DependencyObject elem = e.OriginalSource as DependencyObject;
            while (elem != null)
            {
                if (elem == this)
                    break;
                else if (elem is Popup)
                    return;
                elem = XamMenu.GetParent(elem);
            }

            this.HandleItemSelection();
        }
        #endregion

        #region OnLostMouseCapture

        /// <summary>
        /// Called before the <see cref="E:System.Windows.UIElement.LostMouseCapture"/> event occurs to provide handling for the event in a derived class without attaching a delegate.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Input.MouseEventArgs"/> that contains the event data.</param>
        protected override void OnLostMouseCapture(MouseEventArgs e)
        {
            
            if (this._hasMouseCapture)
            {
                this.ReleaseMouseCapture();
                this._hasMouseCapture = false;
            }

            base.OnLostMouseCapture(e);

            if (this.IsEnabled == false || this.Visibility == Visibility.Collapsed)
            {
                if (this.IsHighlighted)
                {
                    this.IgnorePropertyChange = true;
                    this.IsHighlighted = false;
                }

                IsMouseOver = false;
            }
        }

        #endregion //OnLostMouseCapture


        #region OnAccessKey

        /// <summary>
        /// Provides class handling for when an access key that is meaningful for this element is invoked.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnAccessKey(AccessKeyEventArgs e)
        {
            base.OnAccessKey(e);
            if (this.HasChildren)
            {
                OpenSubmenu();
                this.IsSelected = true;
                this.IsHighlighted = true;
                XamMenuItem item = FindSiblingItem(true, true);
                MoveToItem(item);
            }
            else
            {
                this.IsSelected = true;
                this.IsHighlighted = true;
                this.HandleItemSelection();
            }
        }

        #endregion // OnAccessKey



        #region OnKeyDown
        /// <summary>
        /// Overrides the framework invocation when a user clicks a key.
        /// </summary>
        /// <param name="e">Data about the keydown event.</param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (this.ParentXamMenu == null)
                return;

            Orientation menuOrientation = Orientation.Vertical;

            if (this.IsInContextMenu == false)
            {
                menuOrientation = (this.ParentXamMenu as XamMenu).MenuOrientation;
            }

            // TFS 12975. If element within the item's got the focus we dont need to handle the event.
            DependencyObject focused = PlatformProxy.GetFocusedElement(this) as DependencyObject;
            if (object.ReferenceEquals(focused, this) == false)
            {
                return;
            }

            if (this.IsEnabled)
            {
                if (e.Handled)
                {
                    return;
                }
                e.Handled = true;


                Key currentKey = e.Key;

                if (this.ParentXamMenu.FlowDirection == System.Windows.FlowDirection.RightToLeft)
                {
                    if (e.Key == Key.Left)
                        currentKey = Key.Right;
                    else if (e.Key == Key.Right)
                        currentKey = Key.Left;
                }

                switch (currentKey)
                {
                    case Key.Left:
                        if ((this.Role == MenuItemRole.TopLevelHeader && menuOrientation == Orientation.Vertical)
                        || (this.ParentXamMenuItem != null && this.ParentXamMenuItem.MenuOrientation == Orientation.Horizontal))
                            HandleUpDownKey(false);
                        else
                            HandleLeftKey();
                        break;

                    case Key.Right:
                        if ((this.Role == MenuItemRole.TopLevelHeader && menuOrientation == Orientation.Vertical)
                        || (this.ParentXamMenuItem != null && this.ParentXamMenuItem.MenuOrientation == Orientation.Horizontal))
                            HandleUpDownKey(true);
                        else
                            HandleRightKey();
                        break;

                    case Key.Up:
                        if ((this.Role == MenuItemRole.TopLevelHeader && menuOrientation == Orientation.Vertical)
                         || (this.ParentXamMenuItem != null && this.ParentXamMenuItem.MenuOrientation == Orientation.Horizontal))
                            HandleLeftKey();
                        else
                            HandleUpDownKey(false);
                        break;

                    case Key.Down:
                        if ((this.Role == MenuItemRole.TopLevelHeader && menuOrientation == Orientation.Vertical)
                        || (this.ParentXamMenuItem != null && this.ParentXamMenuItem.MenuOrientation == Orientation.Horizontal))
                            HandleRightKey();
                        else
                            HandleUpDownKey(true);
                        break;

                    case Key.Escape:
                        {
                            if (this.ParentXamMenuItem == this.ParentXamMenu.CurrentOpen)
                            {
                                this.ParentXamMenu.CloseCurrentOpen();
                                this.ParentXamMenu.ReturnFocus();
                            }
                            else
                            {
                                this.HandleLeftKey();
                            }

                            break;
                        }
                    case Key.Space:
                        if (this.IsCheckable)
                        {
                            this.IsChecked = !this.IsChecked;
                            if (this.StaysOpenOnClick == false && this.Role != MenuItemRole.SubmenuHeader)
                            {
                                XamMenuBase parent = this.ParentXamMenu;
                                this.RaiseClickEvent();
                                if (parent != null)
                                {
                                    parent.CloseCurrentOpen();
                                    parent.ReturnFocus();
                                }
                            }
                        }
                        break;

                    case Key.Enter:
                        if (this.Role == MenuItemRole.TopLevelHeader ||
                        this.Role == MenuItemRole.SubmenuHeader)
                        {
                            if (this.IsSubmenuOpen)
                            {
                                // When enter is clicked, instead of just closing the submenu, close the current open
                                // so that if the expanOnHover is false, that the menu doesn't open when you hover over it.
                                //this.CloseSubmenu();
                                this.ParentXamMenu.CloseCurrentOpen();
                                this.ParentXamMenu.ReturnFocus();
                            }
                            else
                            {
                                
                                this.ManipulateSubmenu(true, true);
                                XamMenuItem childItem = FindSiblingItem(true, true);
                                if (childItem != null)
                                {
                                    this.IsHighlighted = true;
                                    this.MoveToItem(childItem);
                                }
                            }
                        }
                        else
                        {
                            if (this.StaysOpenOnClick == false && this.Role != MenuItemRole.SubmenuHeader)
                            {
                                this.ParentXamMenu.CloseCurrentOpen();
                                this.ParentXamMenu.ReturnFocus();
                            }
                        }

                        this.RaiseClickEvent();
                        break;

                    default:

                        e.Handled = false; 
                        break;
                }
            }
        }
        #endregion

        #endregion

        #region OnLostFocus

        /// <summary>
        /// Called before the <see cref="E:System.Windows.UIElement.LostFocus"/> event occurs.
        /// </summary>
        /// <param name="e">The data for the event.</param>
        protected override void OnLostFocus(RoutedEventArgs e)
        {
            if (this.ParentXamMenu != null)
            {
                object newFocus = PlatformProxy.GetFocusedElement(this.ParentXamMenu);

                if (!XamMenu.IsObjectFromMenu(this, newFocus))
                {
                    
                    this.IsSelected = false;
                    this.IsHighlighted = false;
                }

                if (XamMenu.IsObjectFromMenu(this.ParentXamMenu, newFocus))
                    return;

                this.ParentXamMenu._previouslyFocusedControl = null;

                // If the item looses focus to something outside of it, make it not selected
                // Related to Bug: 58300
                this.IsSelected = false;
            }
        }

        #endregion

        #region OnGotFocus

        /// <summary>
        /// Invoked whenever an unhandled <see cref="E:System.Windows.UIElement.GotFocus"/> event reaches this element in its route.
        /// </summary>
        /// <param name="e">The <see cref="T:System.Windows.RoutedEventArgs"/> that contains the event data.</param>
        protected override void OnGotFocus(RoutedEventArgs e)
        {
            // No need to highlight when we're in a ContextMenu
            if (this.IsInContextMenu != true)
            {
                
                bool highlight = this.ParentXamMenu == null ? true : this.ParentXamMenu._previouslyFocusedControl != this;

                
                this.IsHighlighted = highlight;
                this.IsSelected = highlight;
            }

            base.OnGotFocus(e);
        }

        #endregion //OnGotFocus

        #endregion

        #region Properties

        #region Public properties

        #region IsKeyboardNavigatable

        /// <summary>
        /// Identifies the <see cref="IsKeyboardNavigable"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty IsKeyboardNavigableProperty = DependencyProperty.Register("IsKeyboardNavigable", typeof(bool), typeof(XamMenuItem), new PropertyMetadata(true));

        /// <summary>
        /// Gets / sets if the end user will be able to reach this <see cref="XamMenuItem"/> via the keyboard.
        /// </summary>
        public bool IsKeyboardNavigable
        {
            get { return (bool)this.GetValue(IsKeyboardNavigableProperty); }
            set { this.SetValue(IsKeyboardNavigableProperty, value); }
        }

        #endregion // IsKeyboardNavigatable 
          

        #region CheckBoxVisibilityResolved

        /// <summary>
        /// Identifies the <see cref="CheckBoxVisibilityResolved"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty CheckBoxVisibilityResolvedProperty =
            DependencyProperty.Register("CheckBoxVisibilityResolved", typeof(Visibility), typeof(XamMenuItem),
            new PropertyMetadata(Visibility.Collapsed));

        /// <summary>
        /// Determinates if the checkbox is visible. 
        /// </summary>
        public Visibility CheckBoxVisibilityResolved
        {
            get { return (Visibility)this.GetValue(CheckBoxVisibilityResolvedProperty); }
            //set { this.SetValue(CheckBoxVisibilityResolvedProperty, value); }
        }

        #endregion // CheckBoxVisibilityResolved

        #region Icon

        /// <summary>
        /// Identifies the <see cref="Icon"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(object), typeof(XamMenuItem),
            new PropertyMetadata(new PropertyChangedCallback(IconChanged)));

        /// <summary>
        /// Gets or sets the data for image that is displayed next to the text in a menu item.
        /// </summary>
        public object Icon
        {
            get { return (object)this.GetValue(IconProperty); }
            set { this.SetValue(IconProperty, value); }
        }

        private static void IconChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamMenuItem item = obj as XamMenuItem;
            if (e.NewValue != null && item.IsCheckable == false)
            {
                item.SetValue(XamMenuItem.IconVisibilityResolvedProperty, Visibility.Visible);
                item.SetValue(XamMenuItem.CheckBoxVisibilityResolvedProperty, Visibility.Collapsed);                
            }

            if (item._headerIconPresenter != null)
                item._headerIconPresenter.Content = item.Icon;
        }

        #endregion // Icon

        #region IconVisibilityResolved

        /// <summary>
        /// Identifies the <see cref="IconVisibilityResolved"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty IconVisibilityResolvedProperty =
            DependencyProperty.Register("IconVisibilityResolved", typeof(Visibility), typeof(XamMenuItem),
            new PropertyMetadata(Visibility.Collapsed));

        /// <summary>
        /// Determinates if the icon is visible. 
        /// </summary>
        public Visibility IconVisibilityResolved
        {
            get { return (Visibility)this.GetValue(IconVisibilityResolvedProperty); }
        }

        #endregion // IconVisibilityResolved

        #region InputGestureText

        /// <summary>
        /// Identifies the <see cref="InputGestureText"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty InputGestureTextProperty = DependencyProperty.Register("InputGestureText", typeof(string), typeof(XamMenuItem), null);

        /// <summary>
        /// Sets the text describing an input gesture that will call the command tied to the specified item. 
        /// </summary>
        public string InputGestureText
        {
            get { return (string)this.GetValue(InputGestureTextProperty); }
            set { this.SetValue(InputGestureTextProperty, value); }
        }

        #endregion // InputGestureText

        #region IsCheckable

        /// <summary>
        /// Identifies the <see cref="IsCheckable"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty IsCheckableProperty =
            DependencyProperty.Register("IsCheckable", typeof(bool), typeof(XamMenuItem),
            new PropertyMetadata(false, new PropertyChangedCallback(IsCheckableChanged)));
        /// <summary>
        /// Gets/sets a value that indicates whether a XamMenuItem can be checked.
        /// </summary>
        public bool IsCheckable
        {
            get { return (bool)this.GetValue(IsCheckableProperty); }
            set { this.SetValue(IsCheckableProperty, value); }
        }

        private static void IsCheckableChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamMenuItem item = obj as XamMenuItem;
            bool newValue = (bool)e.NewValue;
            if (newValue)
            {
                item.SetValue(XamMenuItem.CheckBoxVisibilityResolvedProperty, Visibility.Visible);
                item.SetValue(XamMenuItem.IconVisibilityResolvedProperty, Visibility.Collapsed);
            }
            else
            {
                item.SetValue(XamMenuItem.CheckBoxVisibilityResolvedProperty, Visibility.Collapsed);
                if (item.Icon != null)
                {
                    item.SetValue(XamMenuItem.IconVisibilityResolvedProperty, Visibility.Visible);
                }
            }
        }

        #endregion // IsCheckable

        #region IsChecked

        /// <summary>
        /// Identifies the <see cref="IsChecked"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register("IsChecked", typeof(bool), typeof(XamMenuItem),
            new PropertyMetadata(false, new PropertyChangedCallback(IsCheckedChanged)));

        /// <summary>
        /// Gets a value that indicates whether a XamMenuItem is checked.
        /// </summary>
        public bool IsChecked
        {
            get { return (bool)this.GetValue(IsCheckedProperty); }
            set { this.SetValue(IsCheckedProperty, value); }
        }

        private static void IsCheckedChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamMenuItem item = obj as XamMenuItem;

            bool newValue = (bool)e.NewValue;

            if (newValue)
                item.RaiseCheckedEvent();
            else
                item.RaiseUncheckedEvent();
        }

        #endregion // IsChecked

        #region IsHighlighted

        /// <summary>
        /// Identifies the <see cref="IsHighlighted"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty IsHighlightedProperty =
            DependencyProperty.Register("IsHighlighted", typeof(Boolean), typeof(XamMenuItem),
            new PropertyMetadata(false, new PropertyChangedCallback(IsHighlightedChanged)));

        /// <summary>
        /// Gets a value that indicates whether a XamMenuItem is highlighted.
        /// </summary>
        public Boolean IsHighlighted
        {
            get { return (Boolean)this.GetValue(IsHighlightedProperty); }
            internal set { this.SetValue(IsHighlightedProperty, value); }
        }

        private static void IsHighlightedChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamMenuItem item = (XamMenuItem)obj;

            if (item.IgnorePropertyChange)
            {
                item.IgnorePropertyChange = false;
                return;
            }

            if ((bool)e.NewValue)
            {
                if (item.SublingOpenItem != null)
                    item.SublingOpenItem.ManipulateSubmenu(false, false);
            }
            item.ChangeVisualState(false);
        }

        #endregion // IsHighlighted

        #region IsSubmenuOpen

        /// <summary>
        /// Identifies the <see cref="IsSubmenuOpen"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty IsSubmenuOpenProperty =
            DependencyProperty.Register("IsSubmenuOpen", typeof(Boolean), typeof(XamMenuItem),
            new PropertyMetadata(false, new PropertyChangedCallback(IsSubmenuOpenChanged)));

        /// <summary>
        /// Gets or sets a value that indicates whether the submenu of the XamMenuItem is open.
        /// </summary>
        public Boolean IsSubmenuOpen
        {
            get { return (Boolean)this.GetValue(IsSubmenuOpenProperty); }
            set { this.SetValue(IsSubmenuOpenProperty, value); }
        }

        private static void IsSubmenuOpenChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamMenuItem item = (XamMenuItem)obj;

            if (item.IgnorePropertyChange)
            {
                item.IgnorePropertyChange = false;
                return;
            }

            bool newValue = (bool)e.NewValue;
            bool oldValue = (bool)e.OldValue;
            if (item.Role == MenuItemRole.TopLevelItem || item.Role == MenuItemRole.SubmenuItem)
            {
                item.IgnorePropertyChange = true;
                item.IsSubmenuOpen = oldValue;
                return;
            }

            if (newValue)
            {
                item.OpenSubmenu();
                item.IsHighlighted = true;
                item.RaiseSubmenuOpenedEvent();
            }
            else
            {
                item.CloseSubmenu();
                if (item._dropDownPopup != null)
                    item._dropDownPopup.IsOpen = false;

                item.RaiseSubmenuClosedEvent();

                // TFS-29741 only the next line
                item.ParentXamMenu.ReturnFocus();

                // TFS-22277 && 23296
                if (item.Role == MenuItemRole.TopLevelHeader)
                {
                    item.ParentXamMenu.CloseCurrentOpen();
                    item.ParentXamMenu.ClearPreviouslyFocusedControl();
                }
            }
        }

        #endregion // IsSubmenuOpen

        #region MenuOrientation

        /// <summary>
        /// Identifies the <see cref="MenuOrientation"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty MenuOrientationProperty = DependencyProperty.Register("MenuOrientation", typeof(Orientation), typeof(XamMenuItem), new PropertyMetadata(Orientation.Vertical, new PropertyChangedCallback(MenuOrientationChanged)));

        /// <summary>
        /// Determinates how menu is orientated. It can be positioned either horizontal or vertical.
        /// </summary>
        /// <remarks>
        /// <p class="body">For arranging the items, menu uses the panel supplied by ItemsPanel property. 
        /// The default one is a StackPanel and it allows the items to be arranged vertical or horizontal.
        /// If you set another type panel, for example a Grid, then the MenuOrientation property will not
        /// be honored automatically. The Grid will take responsibility for arranging the items according 
        /// its layout logic. </p>
        /// </remarks>
        public Orientation MenuOrientation
        {
            get { return (Orientation)this.GetValue(MenuOrientationProperty); }
            set { this.SetValue(MenuOrientationProperty, value); }
        }

        private static void MenuOrientationChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {

            XamMenuItem menuItem = obj as XamMenuItem;
            if (menuItem != null)
            {
                StackPanel hostPanel = menuItem.HeaderedItemContainerGenerator.ItemsHost as StackPanel;
                if (hostPanel != null)
                {
                    hostPanel.Orientation = menuItem.MenuOrientation;
                }
                else
                {
                    // TFS13619 workaround the first set when the panel is not visible
                    menuItem._isMenuOrientationNotSet = true;
                }

                menuItem.SetDropdownPopupPlacement();

            }
        }
        #endregion // MenuOrientation

        #region HasChildren

        /// <summary>
        /// Identifies the <see cref="HasChildren"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty HasChildrenProperty =
            DependencyProperty.Register("HasChildren", typeof(bool), typeof(XamMenuItem), new PropertyMetadata(false));

        /// <summary>
        /// Indicates if this XamMenuItem has any child XamMenuItem (read-only) 
        /// </summary>
        public bool HasChildren
        {
            get { return (bool)this.GetValue(HasChildrenProperty); }
            //set { this.SetValue(HasChildrenProperty, value); }
        }

        #endregion // HasChildren

        #region ParentXamMenuItem

        /// <summary>
        /// Gets a reference to the parent XamMenuItem of this XamMenuItem.
        /// </summary>
        public XamMenuItem ParentXamMenuItem
        {
            get { return ParentItemsControl as XamMenuItem; }
        }

        #endregion

        #region SubMenuPreferredLocation

        /// <summary>
        /// Identifies the <see cref="SubmenuPreferredLocation"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty SubmenuPreferredLocationProperty =
            DependencyProperty.Register("SubmenuPreferredLocation", typeof(MenuItemPosition), typeof(XamMenuItem), new PropertyMetadata(new PropertyChangedCallback(SubmenuPreferredLocationChanged)));

        /// <summary>
        /// Determinates the user preferred position where the popup will be opened for this menu item. 
        /// </summary>
        public MenuItemPosition SubmenuPreferredLocation
        {
            get { return (MenuItemPosition)this.GetValue(SubmenuPreferredLocationProperty); }
            set { this.SetValue(SubmenuPreferredLocationProperty, value); }
        }

        private static void SubmenuPreferredLocationChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {

            XamMenuItem item = (XamMenuItem)obj;
            if (item != null)
            {
                item.SetDropdownPopupPlacement();
            }

        }
        #endregion // SubMenuPreferredLocation

        #region StaysOpenOnClick

        /// <summary>
        /// Identifies the <see cref="StaysOpenOnClick "/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty StaysOpenOnClickProperty =
            DependencyProperty.Register("StaysOpenOnClick", typeof(bool), typeof(XamMenuItem), new PropertyMetadata(false));

        /// <summary>
        /// Gets or sets a value that indicates that the  submenu in which
        /// this MenuItem is located should not close when this item is clicked.
        /// </summary>
        public bool StaysOpenOnClick
        {
            get { return (bool)this.GetValue(StaysOpenOnClickProperty); }
            set { this.SetValue(StaysOpenOnClickProperty, value); }
        }

        #endregion // StaysOpenOnClick

        #endregion

        #region Protected

        #region Role

        /// <summary>
        /// Identifies the <see cref="Role"/> dependency property. 
        /// </summary>
        protected static readonly DependencyProperty RoleProperty =
            DependencyProperty.Register("Role", typeof(MenuItemRole), typeof(XamMenuItem),
            new PropertyMetadata(MenuItemRole.SubmenuItem, new PropertyChangedCallback(RoleChanged)));

        /// <summary>
        /// Gets a value that indicates the role of a XamMenuItem in the menu.
        /// </summary>
        protected MenuItemRole Role
        {
            get { return (MenuItemRole)this.GetValue(RoleProperty); }
            private set { this.SetValue(RoleProperty, value); }
        }

        private static void RoleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {

            XamMenuItem item = (XamMenuItem)obj;
            if (item != null)
            {
                item.SetDropdownPopupPlacement();
            }

        }
        #endregion // Role

        #endregion

        #region Internal properties

        #region IsInContextMenu
        /// <summary>
        /// Gets a value to the parent XamMenu of the XamMenuItem.
        /// </summary>
        internal bool? IsInContextMenu
        {
            get
            {
                if (this.ParentXamMenu is XamContextMenu)
                    return true;
                else if (this.ParentXamMenu is XamMenu)
                    return false;
                else
                    return null;
            }
        }
        #endregion

        #region IsSelected

        /// <summary>
        /// Identifies the <see cref="IsSelected"/> dependency property. 
        /// </summary>
        internal static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register("IsSelected", typeof(Boolean), typeof(XamMenuItem), new PropertyMetadata(new PropertyChangedCallback(IsSelectedChanged)));

        internal Boolean IsSelected
        {
            get { return (Boolean)this.GetValue(IsSelectedProperty); }
            set { this.SetValue(IsSelectedProperty, value); }
        }

        private static void IsSelectedChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamMenuItem item = (XamMenuItem)obj;
            if (item.ParentXamMenu == null)
                return;

            bool newValue = (bool)e.NewValue;
            if (newValue)
            {
                item.ParentXamMenu.CurrentSelected = item;
                item.IsHighlighted = false;
                if (item.ChildHighlightedItem != null)
                    item.ChildHighlightedItem.ManipulateSubmenu(false, false);
            }
            else
            {
                item.ChangeVisualState(false);
            }
        }

        #endregion // IsSelected

        #region IsMouseOver

        /// <summary>
        /// Identifies the <see cref="IsMouseOver"/> dependency property. 
        /// </summary>
        internal static readonly DependencyProperty IsMouseOverProperty = DependencyProperty.Register("IsMouseOver", typeof(Boolean), typeof(XamMenuItem), new PropertyMetadata(new PropertyChangedCallback(IsMouseOverChanged)));

        internal Boolean IsMouseOver
        {
            get { return (Boolean)this.GetValue(IsMouseOverProperty); }
            set { this.SetValue(IsMouseOverProperty, value); }
        }

        private static void IsMouseOverChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamMenuItem item = (XamMenuItem)obj;
            if (item.ParentXamMenu == null)
                return;

            bool newValue = (bool)e.NewValue;
            bool expanOnHover = false;

            // manipulate items in context menu
            if (item.IsInContextMenu == true)
            {
                if (newValue)
                {
                    if (item.IsInContextMenu == true)
                    {
                        if (item.SublingOpenItem != null)
                        {
                            item.SublingOpenItem.ManipulateSubmenu(false, true);
                        }

                        if (item.Role == MenuItemRole.SubmenuHeader)
                        {
                            item.IsHighlighted = false;
                            item.ManipulateSubmenu(true, true);
                        }
                    }

                    item.FocusOrSelect();
                }
                else
                {
                    item.IsSelected = false;
                    if (item.Role == MenuItemRole.SubmenuHeader)
                    {
                        item.ManipulateSubmenu(false, true);
                    }
                }
            }
            else
            {
                expanOnHover = ((XamMenu)item.ParentXamMenu).ExpandOnHover;

                if (newValue)
                {
                    if (item.ParentXamMenu.CurrentOpen != null || expanOnHover)
                    {
                        item.FocusOrSelect();
                    }
                }
                else
                {
                    item.IsSelected = false;
                }

                switch (item.Role)
                {
                    case MenuItemRole.TopLevelHeader:
                        ManupulateMousOverTopLavelHeader(newValue, item, expanOnHover);
                        break;

                    case MenuItemRole.TopLevelItem:

                        if (item.ParentXamMenu.CurrentOpen != null && newValue)
                        {
                            item.ParentXamMenu.CurrentOpen = null;
                        }

                        break;

                    case MenuItemRole.SubmenuHeader:
                        if (newValue)
                        {
                            // mouse enter
                            item.IsHighlighted = false;
                            item.ManipulateSubmenu(true, false);
                        }
                        else
                        {
                            // mouse leave
                            item.ManipulateSubmenu(false, false);
                        }

                        break;
                }
            }

            item.ChangeVisualState(false);
        }

        #endregion // IsMouseOver

        #region IsMouseOverSubling

        /// <summary>
        /// Identifies the <see cref="IsMouseOverSubling"/> dependency property. 
        /// </summary>
        internal static readonly DependencyProperty IsMouseOverSublingProperty = DependencyProperty.Register("IsMouseOverSubling", typeof(Boolean), typeof(XamMenuItem), new PropertyMetadata(new PropertyChangedCallback(IsMouseOverSublingChanged)));

        internal Boolean IsMouseOverSubling
        {
            get { return (Boolean)this.GetValue(IsMouseOverSublingProperty); }
            set { this.SetValue(IsMouseOverSublingProperty, value); }
        }

        private static void IsMouseOverSublingChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamMenuItem item = (XamMenuItem)obj;
            if ((bool)e.NewValue)
            {
                item.IsHighlighted = true;
                item.ManipulateSubmenu(true, false);
            }
        }

        #endregion // IsMouseOverSubling

        #region ParentItemsControl
        /// <summary>
        /// Gets or sets a reference to the parent ItemsControl of a XamMenuItem.
        /// </summary>
        internal ItemsControl ParentItemsControl
        {
            get { return _parentItemsControl; }
            set 
            {
                if (_parentItemsControl != value)
                {
                    _parentItemsControl = value;
                    this.OnPropertyChanged("ParentItemsControl");
                    this.OnPropertyChanged("ParentXamMenu");
                    this.OnPropertyChanged("ParentXamMenuItem");
                }               
            }
        }
        #endregion

        #region ParentXamMenu
        /// <summary>
        /// Gets a reference to the parent XamMenu of the XamMenuItem.
        /// </summary>
        internal protected XamMenuBase ParentXamMenu
        {
            get
            {
                XamMenuItem current = this;
                while (current != null)
                {
                    XamMenuBase menu = current.ParentItemsControl as XamMenuBase;
                    if (menu != null)
                    {
                        return menu;
                    }

                    current = current.ParentXamMenuItem;
                }

                return null;
            }
        }
        #endregion

        #region SublingOpenItem
        internal XamMenuItem SublingOpenItem
        {
            get
            {
                // Look for the next item in the siblings of this item
                ItemsControl parent = ParentItemsControl;
                ItemContainerGenerator generator = XamMenu.GetGenerator(parent);
                if (parent == null || generator == null)
                {
                    return null;
                }

                XamMenuItem childItem;
                for (int ind = 0; ind < parent.Items.Count; ind++)
                {
                    childItem = generator.ContainerFromIndex(ind) as XamMenuItem;
                    if (childItem != null && childItem.IsSubmenuOpen && childItem != this)
                        return childItem;
                }
                return null;
            }
        }
        #endregion

        #region ChildHighlightedItem
        internal XamMenuItem ChildHighlightedItem
        {
            get
            {
                XamMenuItem childItem;
                for (int ind = 0; ind < this.Items.Count; ind++)
                {
                    childItem = ItemContainerGenerator.ContainerFromIndex(ind) as XamMenuItem;
                    if (childItem != null && childItem.IsHighlighted)
                        return childItem;
                }
                return null;
            }
        }
        #endregion

        #region IgnoreMouseUp

        internal bool IgnoreMouseUp
        {
            get;
            set;
        }

        #endregion // IgnoreMouseUp

        #endregion

        #region Private

        #region IgnorePropertyChange

        /// <summary>
        /// Gets or sets a value indicating whether to ignore calling a pending 
        /// change handlers. 
        /// </summary>
        private bool IgnorePropertyChange { get; set; }

        #endregion // IgnorePropertyChange

        #endregion // Private

        #endregion

        #region Methods

        #region Internal

        internal void AutomationMenuItemClick()
        {
            this.OnClick(EventArgs.Empty);
        }

        #region CloseSubmenu
        internal void UnloadSubmenu()
        {
            

            if (this.Role == MenuItemRole.SubmenuHeader || this.Role == MenuItemRole.TopLevelHeader)
            {
                if (this.ChildHighlightedItem != null)
                {
                    this.ChildHighlightedItem.UnloadSubmenu();
                }
            }

            if (this._dropDownPopup != null)
            {
                this._dropDownPopup.IsOpen = false;
            }
        }

        internal void CloseSubmenu()
        {


            

            if (this.Role == MenuItemRole.SubmenuHeader || this.Role == MenuItemRole.TopLevelHeader)
            {
                if (ChildHighlightedItem != null)
                {
                    this.ChildHighlightedItem.ManipulateSubmenu(false, this._isImmediately);
                }
            }

            this.IsHighlighted = false;
            this.IsMouseOverSubling = false;
            if (this.Role == MenuItemRole.TopLevelHeader || this._isImmediately)
            {
                this.GoToSubMenuClosedImmediatelyState();
            }
            else if (this.Role == MenuItemRole.SubmenuHeader)
            {
                this.GoToSubMenuClosedState();
            }
        }
        #endregion

        #region ManipulateSubmenu
        /// <summary>
        /// Helper method that set IsImmediately before to call open or close for submenu.
        /// </summary>
        /// <param name="isOpen">if true the submenu is open; false close the submenu</param>
        /// <param name="isImmediately">if true the action on submenu is execute imeddiately;
        /// false the action on submenu is execute with animation</param>
        internal void ManipulateSubmenu(bool isOpen, bool isImmediately)
        {
            this._isImmediately = isImmediately;

            if (isOpen)
            {
                OpenSubmenu();
            }
            else
            {
                CloseSubmenu();
            }

            // reset the flag
            this._isImmediately = true;

        }
        #endregion

        #region OpenSubmenu
        internal void OpenSubmenu()
        {

            if (_dropDownPopup != null)
            {
                if ( _dropDownPopup.Child != null)
                {
                    // Since there is a chance we could get this called, when we haven't actually removed the events yet
                    // lets make sure we don't attach more than one event handler.
                    _dropDownPopup.Child.MouseEnter -= Items_MouseEnter;
                    _dropDownPopup.Child.MouseLeave -= Items_MouseLeave;
                    (_dropDownPopup.Child as FrameworkElement).SizeChanged -= Popup_SizeChanged;

                    _dropDownPopup.Child.MouseEnter += new MouseEventHandler(Items_MouseEnter);
                    _dropDownPopup.Child.MouseLeave += new MouseEventHandler(Items_MouseLeave);
                    (_dropDownPopup.Child as FrameworkElement).SizeChanged += new SizeChangedEventHandler(Popup_SizeChanged);
                }

                //TFS 12976 prevent opening the dropdown if the parent hasnt opened yet.
                if (this.Role == MenuItemRole.SubmenuHeader && this.ParentXamMenuItem != null && this.ParentXamMenuItem.IsSubmenuOpen != true)
                    return;

                // This was bug fixing from posted by forum
                FrameworkElement popupChild = _dropDownPopup.Child as FrameworkElement;
                if (popupChild != null)
                {
                    popupChild.MaxHeight = double.PositiveInfinity;
                    popupChild.MinHeight = 0;
                    //popupChild.MaxWidth = double.PositiveInfinity;
                    //popupChild.MinWidth = 0;
                }


                _dropDownPopup.IsOpen = true;

                if (this.Role == MenuItemRole.TopLevelHeader || this._isImmediately)
                {
                    VisualStateManager.GoToState(this, SubmenuOpenImmediately, false);
                }
                else if (this.Role == MenuItemRole.SubmenuHeader)
                {
                    VisualStateManager.GoToState(this, SubmenuOpen, false);
                }


                // notify parent menu that the open menu has changed
                if (this.Role == MenuItemRole.TopLevelHeader && this.ParentXamMenu != null)
                {
                    this.ParentXamMenu.CurrentOpen = this;
                }

                // raise open event
                if (this.IsSubmenuOpen == false)
                {

                    
                    if (this._isMenuOrientationNotSet)
                    {
                        StackPanel hostPanel = this.HeaderedItemContainerGenerator.ItemsHost as StackPanel;
                        if (hostPanel != null)
                        {
                            hostPanel.Orientation = this.MenuOrientation;
                            this._isMenuOrientationNotSet = false;
                        }
                    }

                    this.IgnorePropertyChange = true;
                    this.IsSubmenuOpen = true;
                    this.RaiseSubmenuOpenedEvent();
                }

                _dropDownPopup.UpdateLayout();
            }
        }
        #endregion

        #region FindSiblingItem



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

        internal XamMenuItem FindSiblingItem(bool isForward, bool recurse)
        {
            // Look for the next item in the children of this item (if allowed)
            if (recurse)
            {
                int startInd = 0;
                if (isForward == false)
                {
                    startInd = Items.Count - 1;
                }
                XamMenuItem childItem = ItemContainerGenerator.ContainerFromIndex(startInd) as XamMenuItem;
                if (childItem != null)
                {
                    return childItem.IsEnabled ?
                        childItem :
                        childItem.FindSiblingItem(isForward, false);
                }
            }

            // Look for the next item in the siblings of this item
            ItemsControl parent = ParentItemsControl;
            ItemContainerGenerator generator = XamMenu.GetGenerator(parent);
            if (parent == null && generator == null)
            {
                return null;
            }

            // Get the index of this item relative to its siblings
            XamMenuItem item = null;
            int index = generator.IndexFromContainer(this);
            int count = parent.Items.Count;

            bool isLoopedAllItems = false;
            if (isForward)
            {
                // reset the index if we are on bottom
                if (index == count - 1)
                {
                    index = -1;
                    isLoopedAllItems = true;
                }

                // Check for any siblings below this item
                while (index++ < count)
                {

                    item = generator.ContainerFromIndex(index) as XamMenuItem;
                    if (item != null && item.IsEnabled && item.IsKeyboardNavigable)
                    {
                        return item;
                    }
                    else if (isLoopedAllItems == false && index == count - 1)
                    {
                        index = -1;
                        isLoopedAllItems = true;
                    }
                }
            }
            else
            {
                // reset the index if we are on top
                if (index == 0)
                {
                    isLoopedAllItems = true;
                    index = count;
                }

                // Check for any siblings below this item
                while (index-- > 0)
                {

                    item = generator.ContainerFromIndex(index) as XamMenuItem;
                    if (item != null && item.IsEnabled && item.IsKeyboardNavigable)
                    {
                        return item;
                    }
                    else if (isLoopedAllItems == false && index == 0)
                    {
                        index = count;
                        isLoopedAllItems = true;
                    }
                }
            }
            // If nothing else was found, try to find the next sibling below
            // the parent of this item
            //XamMenuItem parentItem = ParentXamMenuItem;
            //if (parentItem != null)
            //{
            //    return parentItem.FindNextSublingItem();
            //}


            return null;
        }

        #endregion //FindSiblingItem

        #endregion

        #region Protected

        #region OnChecked
        /// <summary>
        /// Raises a Checked event when the <see cref="IsChecked"/> property changes from
        /// false to true.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        protected virtual void OnChecked(EventArgs e)
        {
            if (Checked != null)
                Checked(this, e);
        }

        private void RaiseCheckedEvent()
        {
            EventArgs args = new EventArgs();
            OnChecked(args);
        }
        #endregion

        #region OnUnchecked
        /// <summary>
        /// Raises an Unchecked event when the <see cref="IsChecked"/> property changes from
        /// true to false.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        protected virtual void OnUnchecked(EventArgs e)
        {
            if (Unchecked != null)
                Unchecked(this, e);
        }

        private void RaiseUncheckedEvent()
        {
            EventArgs args = new EventArgs();
            OnUnchecked(args);
        }
        #endregion

        #region OnClick

        /// <summary>
        /// Raises a Click event when the user click with left mouse button.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        protected virtual void OnClick(EventArgs e)
        {
            if (Click != null)
                Click(this, e);
        }

        private void RaiseClickEvent()
        {
            XamMenuBase parentBase = this.ParentXamMenu;
            

            if (this.ParentXamMenu != null && this.StaysOpenOnClick == false &&
               (this.Role == MenuItemRole.SubmenuItem || this.Role == MenuItemRole.TopLevelItem))
            {
                parentBase.CloseCurrentOpen();
                
                parentBase.ReturnFocus();
            }

            EventArgs args = new EventArgs();
            this.OnClick(args);

            if (parentBase != null)
            {
                parentBase.RaiseItemClickedEvent(this);
            }

            #region Navigation
            if (this.IsInContextMenu == false)
            {
                if (this.NavigationOnClick)
                {
                    if (this.NavigationElement == null && this.ParentXamMenu != null)
                    {
                        this._navigator.NavigationElement = (this.ParentXamMenu as XamMenu).NavigationElement;
                    }

                    this._navigator.NavigateTo();
                }
            }

            #endregion
        }
        #endregion

        #region OnSubmenuClosed
        /// <summary>
        /// Raises an SubmenuClosed event when the submenu is closed.
        /// </summary>
        /// <param name="e">The event data.</param>
        protected virtual void OnSubmenuClosed(EventArgs e)
        {
            if (SubmenuClosed != null)
                SubmenuClosed(this, e);
        }

        private void RaiseSubmenuClosedEvent()
        {
            EventArgs args = new EventArgs();
            OnSubmenuClosed(args);
        }
        #endregion

        #region OnSubmenuOpened
        /// <summary>
        /// Raises an SubmenuOpened event when the submenu is opened.
        /// </summary>
        /// <param name="e">The event data.</param>
        protected virtual void OnSubmenuOpened(EventArgs e)
        {
            if (SubmenuOpened != null)
                SubmenuOpened(this, e);
        }

        private void RaiseSubmenuOpenedEvent()
        {
            EventArgs args = new EventArgs();
            OnSubmenuOpened(args);
        }
        #endregion

        #endregion

        #region Private

        #region GoToSubMenuClosedState
        private void GoToSubMenuClosedState()
        {
            if (this._subMenuClosedVisualState != null && this._subMenuClosedVisualState.Storyboard != null)
            {
                _closeStoreboardInProcess = true;
                this._subMenuClosedVisualState.Storyboard.Completed += new EventHandler(Storyboard_Completed);

                if (!VisualStateManager.GoToState(this, SubmenuClose, false))
                {
                    this._subMenuClosedVisualState.Storyboard.Completed -= Storyboard_Completed;
                }
            }
            else
            {
                DetachPopupHandlers();
            }

        }
        #endregion // GoToSubMenuClosedState

        #region GoToSubMenuClosedImmediatelyState
        private void GoToSubMenuClosedImmediatelyState()
        {
            if (this._subMenuClosedImmediatelyVisualState != null && this._subMenuClosedImmediatelyVisualState.Storyboard != null)
            {
                _closeImmediatelyStoreboardInProcess = true;
                this._subMenuClosedImmediatelyVisualState.Storyboard.Completed += new EventHandler(Storyboard_Completed);

                if (!VisualStateManager.GoToState(this, SubmenuCloseImmediately, false))
                {
                    this._subMenuClosedImmediatelyVisualState.Storyboard.Completed -= Storyboard_Completed;
                }
            }
            else
            {
                DetachPopupHandlers();
            }
        }
        #endregion // GoToSubMenuClosedImmediatelyState

        private static void ManupulateMousOverTopLavelHeader(bool isMouseOver, XamMenuItem item, bool expanOnHover)
        {
            if (isMouseOver)
            {
                if ((item.ParentXamMenu.CurrentOpen == null && expanOnHover) || item.ParentXamMenu._previouslyFocusedControl != null)
                {
                    item.ManipulateSubmenu(true, true);
                    item.ParentXamMenu.CurrentOpen = item;
                }
                else if (item.ParentXamMenu.CurrentOpen != null)
                {
                    item.ParentXamMenu.CurrentOpen = item;
                }
                item.IsHighlighted = false;
            }
            else if (item.IsSubmenuOpen)
            {
                item.IsHighlighted = true;
            }
        }

        private void DetachPopupHandlers()
        {
            
#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)





                if (this._dropDownPopup != null && _dropDownPopup.Child != null)
                {
                    _dropDownPopup.Child.MouseEnter -= Items_MouseEnter;
                    _dropDownPopup.Child.MouseLeave -= Items_MouseLeave;
                    (_dropDownPopup.Child as FrameworkElement).SizeChanged -= Popup_SizeChanged;
                }



        }

        private void Storyboard_Completed(object sender, EventArgs e)
        {
            DetachPopupHandlers();

            if (this.IsSubmenuOpen)
            {
                //Debug.WriteLine("Storyboard_Completed: {0}", this.Header); 
                this._dropDownPopup.IsOpen = false;
                this.IgnorePropertyChange = true;
                this.IsSubmenuOpen = false;

                // TFS-29887 only the next line
                this.IsHighlighted = false;
                this.ChangeVisualState(false);
                this.RaiseSubmenuClosedEvent();
            }

            if (this._subMenuClosedImmediatelyVisualState != null && this._subMenuClosedImmediatelyVisualState.Storyboard != null)
            {
                this._subMenuClosedImmediatelyVisualState.Storyboard.Completed -= Storyboard_Completed;
            }

            if (this._subMenuClosedVisualState != null && this._subMenuClosedVisualState.Storyboard != null)
            {
                this._subMenuClosedVisualState.Storyboard.Completed -= Storyboard_Completed;
            }
        }

        private void Popup_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            EnsureSubmenuPosition();
        }

        /// <summary>
        /// Handle the change of the IsEnabled property.
        /// </summary>
        /// <param name="sender">The source object.</param>
        /// <param name="e">The event data.</param>
        private void ControlIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ChangeVisualState(false);
        }

        private void FocusOrSelect()
        {
            if (this.ParentXamMenu._previouslyFocusedControl == null)
            {
                Control focusedElem = PlatformProxy.GetFocusedElement(this) as Control;

                // From what i gather, the _previousFocusedControl should never be a xamMenuItem
                // Otherwise, it could receieve focus after another menu item is clicked. 
                XamMenuItem focusItem = focusedElem as XamMenuItem;
                if (focusItem == null)
                    this.ParentXamMenu._previouslyFocusedControl = focusedElem;
            }





            this.Dispatcher.BeginInvoke(new Action( ()=>{

                base.Focus();



            }));


            if (!this.IsSelected)
            {
                this.IsSelected = true;
            }
        }

        private void Items_MouseEnter(object sender, MouseEventArgs e)
        {
            if (this.ParentXamMenu == null)
                return;

            if (this.IsInContextMenu == false)
            {
                XamMenu menu = this.ParentXamMenu as XamMenu;

                if (menu.ExpandOnHover)
                {
                    menu.StopTimer();
                }
            }

            if (this.ChildHighlightedItem != null)
            {
                this.ChildHighlightedItem.ManipulateSubmenu(false, true);
            }

            if (_closeStoreboardInProcess)
            {
                _closeStoreboardInProcess = false;
                this._subMenuClosedVisualState.Storyboard.Completed -= Storyboard_Completed;
            }

            if (_closeImmediatelyStoreboardInProcess)
            {
                _closeImmediatelyStoreboardInProcess = false;
                this._subMenuClosedImmediatelyVisualState.Storyboard.Completed -= Storyboard_Completed;
            }

            this.IsMouseOverSubling = true;
        }

        private void Items_MouseLeave(object sender, MouseEventArgs e)
        {
            if (this.ParentXamMenu == null)
                return;

            if (this.IsInContextMenu == false)
            {
                XamMenu menu = this.ParentXamMenu as XamMenu;
                if (menu.ExpandOnHover)
                {
                    menu.StartTimer();
                }
            }

            this.IsMouseOverSubling = false;
        }

        private void EnsureMenuItemRole()
        {
            // When the menu items are in context menu they can be only SubmenuHeader or SubmenuItem
            // So it could happen to cast the ParentXamMenu to XamMenu wothout check for type
            MenuItemRole topLevelHeader;
            if (Items.Count != 0)
            {
                if (this.ParentXamMenuItem == null && this.IsInContextMenu == false)
                {
                    topLevelHeader = MenuItemRole.TopLevelHeader;
                }
                else
                {
                    topLevelHeader = MenuItemRole.SubmenuHeader;
                }
            }
            else if (this.ParentXamMenuItem == null && this.IsInContextMenu == false)
            {
                topLevelHeader = MenuItemRole.TopLevelItem;
            }
            else
            {
                topLevelHeader = MenuItemRole.SubmenuItem;
            }

            SetValue(RoleProperty, topLevelHeader);
            this.UpdateRoleState();
        }

        /// <summary>
        /// Based on the Role of the <see cref="XamMenuItem"/> sets up the visual states of the control.
        /// </summary>
        protected virtual void UpdateRoleState()
        {

            if (this._isDesignMode)
                return;

            switch (this.Role)
            {
                case MenuItemRole.TopLevelHeader:
                    if (this.CheckBoxVisibilityResolved == Visibility.Visible || this.IconVisibilityResolved == Visibility.Visible)
                    {
                        VisualStateManager.GoToState(this, TopLevelHeaderWithIcon, false);
                    }
                    else
                    {
                        VisualStateManager.GoToState(this, TopLevelHeader, false);
                    }

                    break;

                case MenuItemRole.TopLevelItem:
                    if (this.CheckBoxVisibilityResolved == Visibility.Visible || this.IconVisibilityResolved == Visibility.Visible)
                    {
                        VisualStateManager.GoToState(this, TopLevelItemWithIcon, false);
                    }
                    else
                    {
                        VisualStateManager.GoToState(this, TopLevelItem, false);
                    }

                    break;

                case MenuItemRole.SubmenuHeader:
                    VisualStateManager.GoToState(this, SubmenuHeader, false);
                    break;

                case MenuItemRole.SubmenuItem:
                    VisualStateManager.GoToState(this, SubmenuItem, false);
                    break;
            }
        }

        /// <summary>
        /// Sets the VisualStates on the control to represent the current settings.
        /// </summary>
        /// <param name="useTransitions"></param>
        protected internal virtual void ChangeVisualState(bool useTransitions)
        {
            

            if (IsEnabled == false)
            {
                VisualStateManager.GoToState(this, Disabled, useTransitions);
            }
            else if (this.IsSelected)
            {
                VisualStateManager.GoToState(this, MouseOver, useTransitions);
            }
            else if (IsHighlighted)
            {
                VisualStateManager.GoToState(this, Highlighted, useTransitions);
            }
            else if (IsMouseOver)
            {
                VisualStateManager.GoToState(this, MouseOver, useTransitions);
            }
            else
            {
                VisualStateManager.GoToState(this, Normal, useTransitions);
            }
        }

        private void EnsureSubmenuPosition()
        {
            if (_dropDownPopup == null)
            {
                return;
            }

            FrameworkElement popupChild = _dropDownPopup.Child as FrameworkElement;
            if (popupChild == null)
            {
                return;
            }




            


            

            Size hostContentSize = new Size(Int16.MaxValue, Int16.MaxValue);
            this._dropDownPopup.HorizontalOffset = 0;
            this._dropDownPopup.VerticalOffset = 0;

            return;

            double rootWidth = hostContentSize.Width;
            double rootHeight = hostContentSize.Height;

            double popupContentWidth = popupChild.RenderSize.Width;
            double popupContentHeight = popupChild.RenderSize.Height;

            if (rootHeight == 0 || rootWidth == 0 || popupContentWidth == 0 || popupContentHeight == 0)
            {
                return;
            }

            Point menuItemOffset = new Point(0, 0);






            double rootOffsetX = menuItemOffset.X;
            double rootOffsetY = menuItemOffset.Y;

            double myControlHeight = this.ActualHeight;
            double myControlWidth = this.ActualWidth;
            double popupMaxHeight = (rootHeight - myControlHeight) * MAX_POPUPHEIGHT_FROM_ROOTHEIGHT;

            popupContentWidth = Math.Min(popupContentWidth, rootWidth);
            double popupContentWidthMin = Math.Min(myControlWidth, popupContentWidth);
            popupContentHeight = Math.Min(popupContentHeight, popupMaxHeight);

            bool below = true;
            double popupX = 0;
            double popupY = 0;

            if (this.SubmenuPreferredLocation == MenuItemPosition.Auto)
            {
                switch (this.Role)
                {
                    case MenuItemRole.TopLevelHeader:
                        if (this.ParentXamMenu != null && ((XamMenu)this.ParentXamMenu).MenuOrientation == Orientation.Vertical)
                        {
                            PlaceSubmenuOnPrefferedPosition(out popupX, out popupY, out below, rootOffsetX, rootOffsetY,
                                                            rootWidth, rootHeight, popupContentWidth, popupContentHeight,
                                                            myControlWidth, myControlHeight,
                                                            MenuItemPosition.Right);
                        }
                        else
                        {
                            PlaceSubmenuOnPrefferedPosition(out popupX, out popupY, out below, rootOffsetX, rootOffsetY, rootWidth, rootHeight, popupContentWidth, popupContentHeight, myControlWidth, myControlHeight,
                                MenuItemPosition.Bottom);
                        }
                        break;

                    case MenuItemRole.SubmenuHeader:
                        if (this.ParentXamMenuItem != null && this.ParentXamMenuItem.MenuOrientation == Orientation.Horizontal)
                        {
                            popupX = DefineHorizontalOffset(rootOffsetX, rootWidth, popupContentWidth);
                            popupY = rootOffsetY + myControlHeight;
                            popupY = DefineTopOrBottom(popupY, ref below, rootOffsetY, rootHeight, popupContentHeight, myControlHeight);
                        }
                        else
                        {
                            // try to put on right side
                            popupX = rootOffsetX + myControlWidth;
                            double posX = DefineHorizontalOffset(popupX, rootWidth, popupContentWidth);
                            if (popupX != posX)
                            {
                                // try to put on left side
                                popupX = rootOffsetX - popupContentWidth;
                                if (popupX < 0)
                                {
                                    PlaceSubmenuOnPrefferedPosition(out popupX, out popupY, out below, rootOffsetX, rootOffsetY, rootWidth, rootHeight, popupContentWidth, popupContentHeight, myControlWidth, myControlHeight,
                                        MenuItemPosition.Right);
                                }
                                else
                                {
                                    popupY = DefineVerticalAlignment(ref below, rootOffsetY, rootHeight, popupContentHeight, myControlHeight);
                                }
                            }
                            else
                            {
                                popupY = DefineVerticalAlignment(ref below, rootOffsetY, rootHeight, popupContentHeight, myControlHeight);
                            }
                        }
                        break;
                }
            }
            else
            {
                PlaceSubmenuOnPrefferedPosition(out popupX, out popupY, out below, rootOffsetX, rootOffsetY, rootWidth, rootHeight, popupContentWidth, popupContentHeight, myControlWidth, myControlHeight, this.SubmenuPreferredLocation);
            }

            // Now that we have positioned the popup we may need to truncate its size.
            popupMaxHeight = below ? Math.Min(rootHeight - popupY, popupMaxHeight) : Math.Min(rootOffsetY, popupMaxHeight);

            this._dropDownPopup.HorizontalOffset = popupX - rootOffsetX;
            this._dropDownPopup.VerticalOffset = popupY - rootOffsetY;

            popupChild.MinWidth = popupContentWidthMin;
            popupChild.MaxWidth = rootWidth;
            popupChild.MinHeight = 0;
            popupChild.MaxHeight = Math.Max(0, popupMaxHeight);

            popupChild.HorizontalAlignment = HorizontalAlignment.Left;
            popupChild.VerticalAlignment = VerticalAlignment.Top;


            if (_scrollViewer != null)
            {


#region Infragistics Source Cleanup (Region)



















#endregion // Infragistics Source Cleanup (Region)

            }
        }

        private static void PlaceSubmenuOnPrefferedPosition(
                                out double popupX,
                                out double popupY,
                                out bool below,
                                double rootOffsetX,
                                double rootOffsetY,
                                double rootWidth,
                                double rootHeight,
                                double popupContentWidth,
                                double popupContentHeight,
                                double myControlWidth,
                                double myControlHeight,
                                MenuItemPosition prefPosition)
        {

            below = true;
            popupX = 0;
            popupY = 0;
            switch (prefPosition)
            {
                case MenuItemPosition.Bottom:
                    popupX = DefineHorizontalOffset(rootOffsetX, rootWidth, popupContentWidth);
                    popupY = rootOffsetY + myControlHeight;
                    popupY = DefineTopOrBottom(popupY, ref below, rootOffsetY, rootHeight, popupContentHeight, myControlHeight);
                    break;

                case MenuItemPosition.Top:
                    popupX = DefineHorizontalOffset(rootOffsetX, rootWidth, popupContentWidth);
                    // by this way say we want top alignment
                    popupY = rootHeight + 1;
                    popupY = DefineTopOrBottom(popupY, ref below, rootOffsetY, rootHeight, popupContentHeight, myControlHeight);
                    break;

                case MenuItemPosition.Left:
                    popupX = rootOffsetX - popupContentWidth;
                    if (popupX < 0)
                    {
                        popupX = 0;
                        popupY = rootOffsetY + myControlHeight;
                        popupY = DefineTopOrBottom(popupY, ref below, rootOffsetY, rootHeight, popupContentHeight, myControlHeight);
                    }
                    else
                    {
                        popupY = DefineVerticalOffset(rootOffsetY, rootHeight, popupContentHeight);
                    }

                    break;

                case MenuItemPosition.Right:
                    popupX = rootOffsetX + myControlWidth;
                    double posX = DefineHorizontalOffset(popupX, rootWidth, popupContentWidth);
                    if (popupX != posX)
                    {
                        // Shift it to the left until it does fit.
                        popupX = posX;
                        popupY = rootOffsetY + myControlHeight;
                        popupY = DefineTopOrBottom(popupY, ref below, rootOffsetY, rootHeight, popupContentHeight, myControlHeight);
                    }
                    else
                    {
                        popupY = DefineVerticalOffset(rootOffsetY, rootHeight, popupContentHeight);
                    }
                    break;
            }
        }

        private static double DefineTopOrBottom(double popupY, ref bool below, double rootOffsetY, double rootHeight, double popupContentHeight, double myControlHeight)
        {
            double posY = popupY;
            if (rootHeight < posY + popupContentHeight)
            {
                below = false;
                // It doesn't fit below the parent, try putting it above.
                posY = rootOffsetY - popupContentHeight;
                if (posY <= 0)
                {
                    // doesn't really fit below either.  Now we just pick top 
                    // or bottom based on wich area is bigger.
                    if (rootOffsetY < (rootHeight - myControlHeight) / 2)
                    {
                        below = true;
                        posY = rootOffsetY + myControlHeight;
                    }
                    else
                    {
                        below = false;
                        posY = rootOffsetY - popupContentHeight;
                    }
                }
            }
            return posY;
        }
        private static double DefineVerticalOffset(double rootOffsetY, double rootHeight, double popupContentHeight)
        {
            // Align submenu with top egde of the control
            double popupY = rootOffsetY;
            if (rootHeight < popupY + popupContentHeight)
            {
                // Shift it to the top until it does fit.
                popupY = rootHeight - popupContentHeight;
                popupY = Math.Max(0, popupY);
            }
            return popupY;

        }
        private static double DefineVerticalAlignment(ref bool below, double rootOffsetY, double rootHeight, double popupContentHeight, double myControlHeight)
        {
            // We prefer to put the popup below the menuitem if it will fit.
            //double popupY = rootOffsetY;
            double posY = rootOffsetY;
            if (rootHeight < posY + popupContentHeight)
            {
                below = false;
                // It doesn't fit below the parent, try putting it above.
                posY = (rootOffsetY + myControlHeight) - popupContentHeight;
                if (posY < 0)
                {
                    // doesn't really fit below either.  Now we just pick top 
                    // or bottom based on wich area is bigger.
                    if (rootOffsetY < (rootHeight - myControlHeight) / 2)
                    {
                        below = true;
                        posY = rootOffsetY + myControlHeight;
                    }
                    else
                    {
                        below = false;
                        posY = rootOffsetY - popupContentHeight;
                    }
                }
            }
            return posY;
        }
        private static double DefineHorizontalOffset(double refferPoint, double rootWidth, double popupContentWidth)
        {
            // Align submenu with left egde of the control
            double popupX = refferPoint;// rootOffsetX;
            if (rootWidth < popupX + popupContentWidth)
            {
                // Shift it to the left until it does fit.
                popupX = rootWidth - popupContentWidth;
                popupX = Math.Max(0, popupX);
            }
            return popupX;
        }
        private static double DefineLeftOrRight(double popupX, double rootOffsetX, double rootWidth, double popupContentWidth)
        {
            bool isOverlay;
            return DefineLeftOrRight(popupX, rootOffsetX, rootWidth, popupContentWidth, out isOverlay);
        }
        private static double DefineLeftOrRight(double popupX, double rootOffsetX, double rootWidth, double popupContentWidth, out bool isOverlay)
        {
            isOverlay = false;
            // We prefer to align the submenu on left
            double posX = popupX;
            if (rootWidth < posX + popupContentWidth)
            {
                // Align the submenu on right
                posX = rootOffsetX - popupContentWidth;
                if (posX < 0)
                {
                    isOverlay = true;
                    posX = 0;
                }

                //posX = Math.Max(0, posX);
            }
            return posX;
        }


        private void SetDropdownPopupPlacement()
        {
            if (this._dropDownPopup == null)
            {
                return;
            }

            MenuItemPosition position = this.SubmenuPreferredLocation;

            if (position == MenuItemPosition.Auto)
            {
                XamMenu parentMenu = this.ParentXamMenu as XamMenu;

                if (parentMenu == null)
                {
                    
                    position = MenuItemPosition.Right;
                }
                else if (parentMenu.MenuOrientation == Orientation.Vertical)
                {
                    
                    position = this.Role == MenuItemRole.TopLevelHeader ? MenuItemPosition.Right : MenuItemPosition.Left;
                }
                else
                {
                    
                    position = this.Role == MenuItemRole.TopLevelHeader ? MenuItemPosition.Bottom : MenuItemPosition.Right;
                }
            }

            System.Windows.Controls.Primitives.PlacementMode dropdownPopupPlacement;

            switch (position)
            {
                case MenuItemPosition.Top:
                    dropdownPopupPlacement = System.Windows.Controls.Primitives.PlacementMode.Top;
                    break;
                case MenuItemPosition.Bottom:
                    dropdownPopupPlacement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                    break;
                case MenuItemPosition.Left:
                    dropdownPopupPlacement = System.Windows.Controls.Primitives.PlacementMode.Left;
                    break;
                case MenuItemPosition.Right:
                    dropdownPopupPlacement = System.Windows.Controls.Primitives.PlacementMode.Right;
                    break;
                default:
                    dropdownPopupPlacement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                    break;
            }

            this._dropDownPopup.Placement = dropdownPopupPlacement;
        }


        #region Key navigation
        private void HandleUpDownKey(bool isDown)
        {
            if (!this.IsSelected && isDown)
            {
                this.IsSelected = true;
                this.ChangeVisualState(false);
                if (!this.IsSubmenuOpen)
                {
                    
                    return;
                }
            }

            XamMenuItem nextItem;
            if (this.Role == MenuItemRole.TopLevelHeader)
            {
                if (this.IsSubmenuOpen == false)
                    this.ManipulateSubmenu(true, true);

                nextItem = FindSiblingItem(isDown, true);

                this.IsHighlighted = true;
            }
            else
            {
                nextItem = FindSiblingItem(isDown, false);
            }

            MoveToItem(nextItem);
        }

        private void HandleLeftKey()
        {
            XamMenuItem nextItem = null;
            switch (this.Role)
            {
                case MenuItemRole.SubmenuHeader:
                case MenuItemRole.SubmenuItem:

                    nextItem = this.ParentXamMenuItem;
                    if (nextItem == null)
                        return; // this happens in a ContextMenu and ParentXamMenuItem is null
                    nextItem.ManipulateSubmenu(false, true);

                    if (nextItem.Role == MenuItemRole.TopLevelHeader)
                    {
                        nextItem = this.ParentXamMenuItem.FindSiblingItem(false, false);
                        if (nextItem != null)
                        {
                            if (nextItem.Role == MenuItemRole.TopLevelHeader)
                                nextItem.ManipulateSubmenu(true, true);

                            this.ParentXamMenuItem.IsHighlighted = false;
                            this.ParentXamMenuItem.ChangeVisualState(false);
                        }
                    }
                    break;
                case MenuItemRole.TopLevelHeader:
                case MenuItemRole.TopLevelItem:

                    nextItem = FindSiblingItem(false, false);
                    if (nextItem != null)
                    {
                        if (nextItem.Role == MenuItemRole.TopLevelHeader)
                            nextItem.ManipulateSubmenu(true, true);
                        else
                            this.ManipulateSubmenu(false, true);
                    }
                    break;
            }
            MoveToItem(nextItem);
        }

        private void HandleRightKey()
        {
            XamMenuItem childItem = null;
            switch (this.Role)
            {
                case MenuItemRole.SubmenuHeader:
                    this.ManipulateSubmenu(true, true);
                    childItem = FindSiblingItem(true, true);
                    if (childItem != null)
                    {
                        this.IsHighlighted = true;
                    }
                    break;

                case MenuItemRole.TopLevelHeader:
                case MenuItemRole.TopLevelItem:

                    childItem = FindSiblingItem(true, false);
                    if (childItem != null)
                    {
                        if (childItem.Role == MenuItemRole.TopLevelHeader)
                            childItem.ManipulateSubmenu(true, true);
                        else
                            this.ManipulateSubmenu(false, true);
                    }
                    break;

                case MenuItemRole.SubmenuItem:

                    if (ParentXamMenu.CurrentOpen == null)
                        return; // this happens in a ContextMenu and ParentXamMenuItem is null

                    childItem = ParentXamMenu.CurrentOpen.FindSiblingItem(true, false);
                    if (childItem != null)
                    {
                        if (childItem.Role == MenuItemRole.TopLevelHeader)
                            childItem.ManipulateSubmenu(true, true);
                        else if (childItem.Role == MenuItemRole.TopLevelItem)
                        {
                            this.ParentXamMenu.CloseCurrentOpen();
                        }
                    }
                    break;
            }

            MoveToItem(childItem);
        }

        private void MoveToItem(XamMenuItem menuItem)
        {
            if (menuItem != null)
            {
                menuItem.FocusOrSelect();
                menuItem.ChangeVisualState(false);
                this.ChangeVisualState(false);
                if (this.ParentXamMenuItem != null)
                {
                    this.ParentXamMenuItem.HeaderedItemContainerGenerator.ScrollIntoView(menuItem);
                }
            }
        }

        #endregion // Key navigation

        private void HandleItemSelection()
        {

            // So basically, in WPF, the MouseLeftButtonUp event will actually bubble up. 
            // We don't want to mark it handled though, b/c that would stop the MouseLeftButtonUp event from being raised. 
            // So, instead, if we have a ParentMenuItem, we look at it, and tell it to Ignore doing anything on the MouseUp
            // By doing this, we make it act like it was originally intendend, like in SL.
            if (this.ParentXamMenuItem != null)
                this.ParentXamMenuItem.IgnoreMouseUp = true;

            if (this.IgnoreMouseUp)
            {
                this.IgnoreMouseUp = false;
                return;
            }

            if (this.ParentXamMenu == null)
            {
                return;
            }

            if (this.IsCheckable)
            {
                this.IsChecked = !this.IsChecked;
            }

            this.RaiseClickEvent();

            if (this.Role == MenuItemRole.SubmenuItem)
            {
                if (this.ParentXamMenu != null && !this.StaysOpenOnClick)
                {
                    this.ParentXamMenu.ReturnFocus();
                    this.IsMouseOver = false;
                }
                else
                {
                    this.Focus();
                }
            }

            if (this.Role == MenuItemRole.TopLevelItem)
            {
                if (this.ParentXamMenu != null)
                {
                    this.ParentXamMenu.ReturnFocus();
                }
            }

            if (this._hasMouseCapture && !this.StaysOpenOnClick)
            {
                this.ReleaseMouseCapture();
                this._hasMouseCapture = false;
            }
        }

        #region HandleAccessKeyPressed
        private void HandleAccessKeyPressed(object sender, AccessKeyPressedEventArgs e)
        {
            if (e.Scope == null && e.Target == null)
            {
                if (string.IsNullOrEmpty(e.Key))
                    return;

                e.Target = this;
                e.Handled = true;
            }
        }
        #endregion // HandleAccessKeyPressed


        #endregion // Private

        #endregion // Methods

        #region Events

        /// <summary>
        /// Occurs when a <see cref="XamMenuItem"/> is clicked.
        /// </summary>
        public event EventHandler Click;

        /// <summary>
        /// Occurs when a <see cref="XamMenuItem"/> is checked.
        /// </summary>
        public event EventHandler Checked;

        /// <summary>
        /// Occurs when a <see cref="XamMenuItem"/> is unchecked.
        /// </summary>
        public event EventHandler Unchecked;

        /// <summary>
        /// Occurs when a submenu of <see cref="XamMenuItem"/> is closed.
        /// </summary>
        public event EventHandler SubmenuClosed;

        /// <summary>
        /// Occurs when a submenu of <see cref="XamMenuItem"/> is opened.
        /// </summary>
        public event EventHandler SubmenuOpened;

        #endregion

        #region Navigation
        INavigation _navigator;

        #region NavigationElement

        /// <summary>
        /// Identifies the <see cref="NavigationElement"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty NavigationElementProperty =
            DependencyProperty.Register("NavigationElement", typeof(Control), typeof(XamMenuItem), new PropertyMetadata(new PropertyChangedCallback(NavigationElementChanged)));

        /// <summary>
        /// Gets or sets a control that will be used for navigation.
        /// </summary>
        /// <remarks>      
        /// <p class="note">The only elements you can set here are Frame and Page. Only those controls support navigation.</p>
        /// <p class="body">When this property is not set, then <see cref="XamMenu.NavigationElement"/> will be used
        /// to deal with navigation.</p>
        /// </remarks>
        public Control NavigationElement
        {
            get { return (Control)this.GetValue(NavigationElementProperty); }
            set { this.SetValue(NavigationElementProperty, value); }
        }

        private static void NavigationElementChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamMenuItem item = obj as XamMenuItem;
            item._navigator.NavigationElement = e.NewValue as Control;
        }

        #endregion // NavigationElement

        #region NavigationUri

        /// <summary>
        /// Identifies the <see cref="NavigationUri"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty NavigationUriProperty =
            DependencyProperty.Register("NavigationUri", typeof(Uri), typeof(XamMenuItem),
            new PropertyMetadata(new PropertyChangedCallback(NavigationUriChanged)));

        /// <summary>
        /// The URI of the content to navigate to.
        /// </summary>
        [TypeConverter(typeof(UriTypeConverter))]
        public Uri NavigationUri
        {
            get { return (Uri)this.GetValue(NavigationUriProperty); }
            set { this.SetValue(NavigationUriProperty, value); }
        }

        private static void NavigationUriChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamMenuItem item = obj as XamMenuItem;
            item._navigator.NavigationUri = e.NewValue as Uri;
        }

        #endregion // NavigationUri

        #region NavigationParameter

        /// <summary>
        /// Identifies the <see cref="NavigationParameter"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty NavigationParameterProperty =
            DependencyProperty.Register("NavigationParameter", typeof(string), typeof(XamMenuItem),
            new PropertyMetadata(new PropertyChangedCallback(NavigationParameterChanged)));

        /// <summary>
        /// Gets or sets parameters that are passed to NavigationService.Navigate method.
        /// </summary>
        /// <remarks>
        /// <p class="body">This is a string that contains parameters which can be reach after navigation complete by 
        /// NavigationContext.QueryString property. The format must be as follow: /param1/param2…/paramN. Each parameter must be separated by ‘/’.</p>
        /// <para class="note">If you pass only one parameter you don't need to write '/' sing.</para>
        /// </remarks>
        public string NavigationParameter
        {
            get { return (string)this.GetValue(NavigationParameterProperty); }
            set { this.SetValue(NavigationParameterProperty, value); }
        }

        private static void NavigationParameterChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamMenuItem item = obj as XamMenuItem;
            item._navigator.NavigationParameter = e.NewValue != null ? e.NewValue.ToString() : "";
        }

        #endregion // NavigationParameter

        #region NavigationOnClick

        /// <summary>
        /// Identifies the <see cref="NavigationOnClick"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty NavigationOnClickProperty =
            DependencyProperty.Register("NavigationOnClick", typeof(bool), typeof(XamMenuItem), new PropertyMetadata(false));

        /// <summary>
        /// Gets or sets whether the <see cref="XamMenuItem"/> will navigate to the content specified in the 
        /// <see cref="NavigationUri"/> property when user clicks on it.
        /// </summary>
        public bool NavigationOnClick
        {
            get { return (bool)this.GetValue(NavigationOnClickProperty); }
            set { this.SetValue(NavigationOnClickProperty, value); }
        }

        #endregion // NavigationOnClick

        #endregion

        #region EventHandlers

        void XamMenuItem_Unloaded(object sender, RoutedEventArgs e)
        {
            // When the menu's unloaded, be sure to make sure the Storyboard completed is called. 
            // otherwise if a menu was in the middle of closing when you dragged it, it might not have closed properly
            // and won't reopen.
            this.Storyboard_Completed(this, EventArgs.Empty);
        }

        #endregion // EventHandlers

        #region INotifyPropertyChanged Members
        /// <summary>
        /// Event raised when a property on this object changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the PropertyChanged event.
        /// </summary>
        /// <param name="name"></param>
        protected virtual void OnPropertyChanged(string name)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(name));
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