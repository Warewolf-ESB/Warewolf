using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Infragistics.AutomationPeers;

namespace Infragistics.Controls.Menus
{

    /// <summary>
    /// Displays a menu in a WPF application.
    /// </summary>
    /// <remarks>
    /// <p class="body">The <b>XamMenu</b> control is used to display a menu in a WPF application.
    /// The <b>XamMenu</b> control supports the following features:
    ///     <ul>
    ///         <li>Data binding that allows the control's menu items to be bound to hierarchal data sources.</li>
    ///         <li>Programmatic access to the <b>XamMenu</b> object model to dynamically create menus, populate menu items, set properties, and so on.</li>
    ///         <li>Customizable appearance through styles, and user-defined templates.</li>
    ///     </ul>
    /// </p>
    /// <p class="body"><b>XamMenu</b> is a ItemsControl. Its content properties are Items and ItemsSource.
    /// You can read more information about ItemsControl in WPF help dcumentation.</p>
    /// <p class="body">The <b>XamMenu</b> control presents a list of items that specify commands or options
    /// for a WPF application. Typically, clicking an item on a menu opens a submenu or causes an
    /// application to carry out a command. </p>
    /// <p class="body">An item in a menu can be anything that can be added to an ItemCollection. 
    /// The <see cref="XamMenuItem"/> is the most common type of item in a XamMenu. A <see cref="XamMenuItem"/> can contain 
    /// child items. The child items will appear in a submenu when the user chooses a parent <see cref="XamMenuItem"/>.</p>
    /// <p class="body">You can change the menu orientation by <see cref="MenuOrientation"/> property to either horizontal or vertical.
    /// Also you can change how the menu opens its submenus by <see cref="ExpandOnHover"/> property.</p>
    /// </remarks>


#region Infragistics Source Cleanup (Region)





















#endregion // Infragistics Source Cleanup (Region)

    [StyleTypedProperty(Property = "ItemContainerStyle", StyleTargetType = typeof(XamMenuItem))]

    
    

    public class XamMenu : XamMenuBase
    {
        

        private Window _window;        
        private bool _windowEventsHooked;


        #region Member variables
        private DispatcherTimer _webBehaviorTimer;
        private bool _isMenuOrientationNotSet;
        #endregion

        #region Constructors


        /// <summary>
        /// Static constructor of the <see cref="XamMenu"/> class. 
        /// </summary>
        static XamMenu()
        {
            Style style = new Style();
            style.Seal();
            Control.FocusVisualStyleProperty.OverrideMetadata(typeof(XamMenu), new FrameworkPropertyMetadata(style));
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="XamMenu"/> class.
        /// </summary>
        public XamMenu()
        {
            DefaultStyleKey = typeof(XamMenu);

            _webBehaviorTimer = new DispatcherTimer();
            _webBehaviorTimer.Tick += (sender, e) =>
            {
                CloseCurrentOpen();
                ReturnFocus();
                this.ClearPreviouslyFocusedControl();
            };

            Infragistics.Windows.Utilities.ValidateLicense(typeof(XamMenu), this);
            this.Focusable = false;
            this.IsTabStop = false;

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

            return base.ArrangeOverride(finalSize);
        }
        #endregion

        #region OnBeforeUnload

        /// <summary>
        /// Called before unload the menu from the browser.
        /// </summary>
        protected override void OnBeforeUnload()
        {
            if (this.CurrentOpen != null)
            {
                this.CurrentOpen.UnloadSubmenu();
                this.CurrentOpen = null;
            }
        }
        #endregion //OnBeforeUnload

        #region OnCreateAutomationPeer
        /// <summary>
        /// When implemented in a derived class, returns class-specific <see cref="T:System.Windows.Automation.Peers.AutomationPeer"/> implementations for the automation infrastructure.
        /// </summary>
        /// <returns>
        /// The class-specific <see cref="T:System.Windows.Automation.Peers.AutomationPeer"/> subclass to return.
        /// </returns>
        protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer()
        {
            return new XamMenuAutomationPeer(this);
        }
        #endregion //OnCreateAutomationPeer

        #region OnCurrentOpenItemChanged
        /// <summary>
        /// Called when the current open menu item has changed.
        /// </summary>
        /// <param name="oldItem">the old item</param>
        /// <param name="newItem">the new item</param>
        protected internal override void OnCurrentOpenItemChanged(XamMenuItem oldItem, XamMenuItem newItem)
        {
            // Fix if the menu root is any other popup. For example it can be our Window
            if (oldItem == null && newItem != null)
            {
                UIElement menuRoot = GetRoot(newItem) as UIElement;
                if (menuRoot != null && menuRoot != PlatformProxy.GetRootVisual(this))
                {
                    menuRoot.AddHandler(UIElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(this.RootVisual_MouseLeftButtonDown), true);
                }

                this.EnableCloseUpTriggerTrackers();

            }
        }
        #endregion

        #region OnLostFocus

        /// <summary>
        /// Called before the <see cref="System.Windows.UIElement.LostFocus"/> event occurs.
        /// </summary>
        /// <param name="e">The data for the event.</param>
        protected override void OnLostFocus(RoutedEventArgs e)
        {
            object newFocus = PlatformProxy.GetFocusedElement(this);
            if (IsObjectFromMenu(this, newFocus))
                return;

            this._previouslyFocusedControl = null;
        }

        #endregion

        #region OnApplyTemplate

        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal processes call System.Windows.FrameworkElement.ApplyTemplate().
        /// </summary>
        public override void OnApplyTemplate()
        {

            AccessKeyManager.RemoveAccessKeyPressedHandler(this, this.HandleAccessKeyPressed);

            base.OnApplyTemplate();


            AccessKeyManager.AddAccessKeyPressedHandler(this, this.HandleAccessKeyPressed);


            this._isMenuOrientationNotSet = true;
        }

        #endregion // OnApplyTemplate

        #endregion

        #region Properties

        #region Public properties

        #region public Orientation MenuOrientation

        /// <summary>
        /// Identifies the <see cref="MenuOrientation"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty MenuOrientationProperty =
            DependencyProperty.Register("MenuOrientation", typeof(Orientation), typeof(XamMenu),
            new PropertyMetadata(Orientation.Horizontal, new PropertyChangedCallback(OnMenuOrientationPropertyChanged)));

        /// <summary>
        /// Determinates how the menu is orientated. It can be positioned either horizontal or vertical.
        /// </summary>
        /// <remarks>
        /// The menu relies on the SatckPanel to arranges its items. If you change the ItemsPanelTemplate to
        /// different from StackPanel the property will have no mean in menu. 
        /// </remarks>
        public Orientation MenuOrientation
        {
            get { return (Orientation)this.GetValue(MenuOrientationProperty); }
            set { this.SetValue(MenuOrientationProperty, value); }
        }

        private static void OnMenuOrientationPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            XamMenu menu = d as XamMenu;
            if (menu != null)
            {
                StackPanel hostPanel = menu.HeaderedItemContainerGenerator.ItemsHost as StackPanel;
                if (hostPanel != null)
                {
                    hostPanel.Orientation = menu.MenuOrientation;
                }
                else
                {
                    // TFS13619 workaround the first set when the panel is not visible
                    menu._isMenuOrientationNotSet = true;
                }
            }
        }
        #endregion // MenuOrientation

        #region public bool ExpandOnHover

        /// <summary>
        /// Identifies the <see cref="ExpandOnHover"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty ExpandOnHoverProperty =
            DependencyProperty.Register("ExpandOnHover", typeof(bool), typeof(XamMenu), new PropertyMetadata(false));

        /// <summary>
        /// Determinates if the submenu of the root item will be opened when the 
        /// mouse is over the item or user must click on the root item to open the submenu.
        /// </summary>
        public bool ExpandOnHover
        {
            get { return (bool)this.GetValue(ExpandOnHoverProperty); }
            set { this.SetValue(ExpandOnHoverProperty, value); }
        }

        #endregion // ExpandOnHover

        #region  public Control NavigationElement

        /// <summary>
        /// Identifies the <see cref="NavigationElement"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty NavigationElementProperty =
            DependencyProperty.Register("NavigationElement", typeof(Control), typeof(XamMenu), null);

        /// <summary>
        /// Gets or sets a control that will be used for navigation.
        /// </summary>
        /// <remarks>      
        /// <p class="note">The only elements you can set here are Frame and Page. Only those controls support navigation.</p>
        /// <p class="body">When you set this property, all <see cref="XamMenuItem"/>s in the menu will use 
        /// it to deal with navigation. Except when you set explicitly <see cref="XamMenuItem.NavigationElement"/>.</p>
        /// </remarks>
        public Control NavigationElement
        {
            get { return (Control)this.GetValue(NavigationElementProperty); }
            set { this.SetValue(NavigationElementProperty, value); }
        }

        #endregion // NavigationElement

        #endregion

        #endregion

        #region Methods

        #region Internal methods

        internal override void CloseCurrentOpen()
        {
            base.CloseCurrentOpen();
            if (this._webBehaviorTimer != null)
            {
                this._webBehaviorTimer.Stop();
            }

            UIElement menuRoot = GetRoot(this) as UIElement;
            if (menuRoot != null && menuRoot != PlatformProxy.GetRootVisual(this))
            {
                menuRoot.RemoveHandler(UIElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(this.RootVisual_MouseLeftButtonDown));

                this.DisableCloseUpTriggerTrackers();

            }
        }

        internal void StartTimer()
        {
            this._webBehaviorTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            this._webBehaviorTimer.Start();
        }

        internal void StopTimer()
        {
            this._webBehaviorTimer.Stop();
        }

        #endregion

        #region Private methods


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



        #endregion

        #endregion


        private void EnableCloseUpTriggerTrackers()
        {
            if (!_windowEventsHooked)
            {
                this._window = Window.GetWindow(this);
                if (null != this._window)
                {
                    //this._window.LocationChanged += new EventHandler(OnWindowStateChanged);
                    this._window.Deactivated += new EventHandler(OnWindowStateChanged);
                    //this._window.SizeChanged += new SizeChangedEventHandler(OnWindowSizeChanged);

                    //this._window.AddHandler(UIElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(this.RootVisual_MouseLeftButtonDown), true);
                    this._windowEventsHooked = true;
                }
            }
        }

        void OnWindowStateChanged(object sender, EventArgs e)
        {
            if (this.CurrentOpen != null)
                this.CloseCurrentOpen();
        }

        #region DisableCloseUpTriggerTrackers

        private void DisableCloseUpTriggerTrackers()
        {            
            if (true == this._windowEventsHooked && this._window != null)
            {
                if (null != this._window)
                {
                    //this._window.LocationChanged -= new EventHandler(OnWindowStateChanged);
                    this._window.Deactivated -= new EventHandler(OnWindowStateChanged);
                    //this._window.SizeChanged -= new SizeChangedEventHandler(OnWindowSizeChanged);
                    //this._window.RemoveHandler(UIElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(this.RootVisual_MouseLeftButtonDown));
                    this._windowEventsHooked = false;
                }
            }
        }

        #endregion //DisableCloseUpTriggerTrackers

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