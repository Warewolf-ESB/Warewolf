using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Infragistics.AutomationPeers;
using Infragistics.Controls.Menus.Primitives;

namespace Infragistics.Controls.Menus
{

    /// <summary>
    /// Represents a pop-up menu that enables a control to expose functionality that is specific to the context of the control.
    /// </summary>
    /// <remarks>
    /// A context menu (also called contextual, shortcut, and popup or pop-up menu) is a menu in a graphical user interface (GUI) that appears upon user interaction, such as a right mouse click. A context menu offers a limited set of choices that are available in the current state, or context, of the operating system or application.
    /// </remarks>


#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)


    
    

    public class XamContextMenu : XamMenuBase, ICommandTarget
    {
        #region Constants

        private const PlacementMode PlacementModeDefaultValue = PlacementMode.MouseClick;

        #endregion //Constants

        #region Members

        private Popup _contextMenuPopup; // the popup used to show the context menu
        private Point _contextMenuLocation; // see ContextMenuLocation property
        private Point _mouseClickLocation; // see MouseClickLocation property
        private bool _skipIsOpenChange; // indicates that this is an internal change
        private WeakReference _targetElement; // a target element for an attached XamContextMenu
        private bool _mustSetPosition; // inidcates that the position must be changed in size changed event handler
        private Control _focusedControl; // focused control
        private ScaleTransform _zoomTransform; // used to display the context menu at correct position
        private bool _preventPlacementChangeEvent; // used to prevent the Placement property from changing the position of the menu when the value is altered.


        private bool _skipPopupEvents; // indicates that an internal change of Popup.IsOpen occurs
        private bool _popupInitiatedClose;


        #endregion //Members

        #region Constructor


        /// <summary>
        /// Static constructor of the <see cref="XamContextMenu"/> class. 
        /// </summary>
        static XamContextMenu()
        {
            Style style = new Style();
            style.Seal();
            Control.FocusVisualStyleProperty.OverrideMetadata(typeof(XamContextMenu), new FrameworkPropertyMetadata(style));
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="XamContextMenu"/> class. 
        /// </summary>
        public XamContextMenu()
        {

            this.DefaultStyleKey = typeof (XamContextMenu);
            // there is a problem with popoup when zoom. we must scale the popup to ensure correct size and position
            _zoomTransform = new ScaleTransform() { ScaleX = 1, ScaleY = 1, CenterX = 0, CenterY = 0 };
            this._contextMenuPopup = new Popup() { RenderTransform = _zoomTransform };


            this._contextMenuPopup.StaysOpen = false;


            this._contextMenuPopup.Opened += this.Popup_Opened;
            this._contextMenuPopup.Closed += this.Popup_Closed;
            this.SizeChanged += this.XamContextMenu_SizeChanged;

            CommandSourceManager.RegisterCommandTarget(this);
            CommandSourceManager.NotifyCanExecuteChanged(typeof(OpenCommand));
            CommandSourceManager.NotifyCanExecuteChanged(typeof(CloseCommand));


            Infragistics.Windows.Utilities.ValidateLicense(typeof(XamContextMenu), this);

        }

        #endregion //Constructor

        #region Overrides

        #region OnCreateAutomationPeer
        /// <summary>
        /// When implemented in a derived class, returns class-specific <see cref="T:System.Windows.Automation.Peers.AutomationPeer"/> implementations for the automation infrastructure.
        /// </summary>
        /// <returns>
        /// The class-specific <see cref="T:System.Windows.Automation.Peers.AutomationPeer"/> subclass to return.
        /// </returns>
        protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer()
        {
            return new XamContextMenuAutomationPeer(this);
        }
        #endregion //OnCreateAutomationPeer

        #region OnItemClicked

        /// <summary>
        /// Raises a <see cref="XamMenuBase.ItemClicked"/> event when the user click with left mouse button on any menu item.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        /// <remarks>The <see cref="XamContextMenu"/>ContextMenu closes always when the clicked <see cref="XamMenuItem"/> has no subitems</remarks>
        protected override void OnItemClicked(ItemClickedEventArgs e)
        {
            if (!(e.Item.IsSubmenuOpen || e.Item.StaysOpenOnClick))
            {
                this.IsOpen = false;
            }

            base.OnItemClicked(e);
        }

        #endregion //OnItemClicked

        #region OnLostFocus

        /// <summary>
        /// Called before the <see cref="System.Windows.UIElement.LostFocus"/> event occurs.
        /// </summary>
        /// <param name="e">The data for the event.</param>
        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);

            if (this.IsOpen)
            {
                FrameworkElement focused = PlatformProxy.GetFocusedElement(this) as FrameworkElement;
                if (!this.IsChild(focused))
                {
                    this.IsOpen = false;
                }
            }
        }

        #endregion //OnLostFocus

        #region CloseCurrentOpen







        internal override void CloseCurrentOpen()
        {
            base.CloseCurrentOpen();
            this.IsOpen = false;

            if (this.IsOpen)
            {
                // cannot close the context menu; user cancels closing
                this._focusedControl = this._previouslyFocusedControl;
                this._previouslyFocusedControl = null; // do not return the focus
            }
        }

        #endregion //CloseCurrentOpen

        #region OnBeforeUnload

        /// <summary>
        /// Called before unload the menu from the browser.
        /// </summary>
        protected override void OnBeforeUnload()
        {
            foreach (var item in Items)
            {
                XamMenuItem menuItem = this.ItemContainerGenerator.ContainerFromItem(item) as XamMenuItem;
                if (menuItem != null)
                {
                    menuItem.UnloadSubmenu();
                }
            }
        }

        #endregion //OnBeforeUnload

        #region OnKeyDown

        /// <summary>
        /// Called before the <see cref="E:System.Windows.UIElement.KeyDown"/> event occurs.
        /// </summary>
        /// <param name="e">The data for the event.</param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.CloseCurrentOpen();
                this.ClearPreviouslyFocusedControl();
            }

            base.OnKeyDown(e);
        }

        #endregion //OnKeyDown

        #region PropertiesToIgnore

        /// <summary>
        /// Gets a List of properties that shouldn't be saved when the PersistenceManager goes to save them.
        /// </summary>
        protected override List<string> PropertiesToIgnore
        {
            get
            {
                List<string> pti = base.PropertiesToIgnore;

                pti.Add("PlacementTarget");

                return pti;
            }
        }
        #endregion // PropertiesToIgnore

        #endregion //Overrides

        #region Properties

        #region Public Properties

        #region IsOpen

        /// <summary>
        /// Identifies the <see cref="IsOpen"/> dependency property
        /// </summary>
        public static readonly DependencyProperty IsOpenProperty = DependencyProperty.Register("IsOpen", typeof(bool), typeof(XamContextMenu), new PropertyMetadata(new PropertyChangedCallback(OnIsOpenChanged)));

        private static void OnIsOpenChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            XamContextMenu contextMenu = target as XamContextMenu;

            if (contextMenu != null)
            {
                if (!contextMenu.ResolveIsOpenChange(e))
                {
                    return;
                }

                if ((bool)e.NewValue)
                {
                    contextMenu.OnContextMenuOpened();
                }
                else
                {
                    contextMenu.OnContextMenuClosed();
                }
            }

            CommandSourceManager.NotifyCanExecuteChanged(typeof(OpenCommand));
            CommandSourceManager.NotifyCanExecuteChanged(typeof(CloseCommand));
        }

        /// <summary>
        /// Gets/set a value that indicates whether the <see cref="XamContextMenu"/> is visible. 
        /// </summary>
        /// <seealso cref="IsOpenProperty"/>
        /// <seealso cref="Opening"/>
        /// <seealso cref="Opened"/>
        /// <seealso cref="Closing"/>
        /// <seealso cref="Closed"/>
        public bool IsOpen
        {
            get { return (bool)this.GetValue(IsOpenProperty); }
            set { this.SetValue(IsOpenProperty, value); }
        }

        #endregion //IsOpen

        #region Placement

        /// <summary>
        /// Identifies the <see cref="Placement "/> dependency property
        /// </summary>
        public static readonly DependencyProperty PlacementProperty =
            DependencyProperty.Register("Placement", typeof(PlacementMode), typeof(XamContextMenu),
                                        new PropertyMetadata(PlacementModeDefaultValue, OnPlacementChanged));

        private static void OnPlacementChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            XamContextMenu contextMenu = target as XamContextMenu;

            if (!contextMenu._preventPlacementChangeEvent)
            {
                if (contextMenu != null && contextMenu.IsOpen)
                    contextMenu.SetPosition();
            }
        }

        /// <summary>
        /// Gets/sets the mode that controls how <see cref="XamContextMenu"/> appears on the screen.
        /// </summary>
        /// <seealso cref="PlacementProperty"/>
        public PlacementMode Placement
        {
            get { return (PlacementMode)this.GetValue(PlacementProperty); }
            set { this.SetValue(PlacementProperty, value); }
        }

        #endregion //Placement

        #region PlacementTarget

        /// <summary>
        /// Identifies the <see cref="PlacementTarget"/> dependency property
        /// </summary>
        public static readonly DependencyProperty PlacementTargetProperty =
            DependencyProperty.Register("PlacementTarget", typeof(UIElement), typeof(XamContextMenu),
                                        new PropertyMetadata(new PropertyChangedCallback(OnPlacementTargetChanged)));

        private static void OnPlacementTargetChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            XamContextMenu contextMenu = target as XamContextMenu;

            if (contextMenu != null && contextMenu.IsOpen)
            {
                contextMenu.SetPosition();
            }
        }

        /// <summary>
        /// Gets/sets the target element relative to which of <see cref="XamContextMenu"/> is positioned when it opens.
        /// </summary>
        /// <seealso cref="PlacementTargetProperty"/>
        public UIElement PlacementTarget
        {
            get { return (UIElement)this.GetValue(PlacementTargetProperty); }

            set { this.SetValue(PlacementTargetProperty, value); }
        }

        #endregion //PlacementTarget

        #region PlacementTargetResolved

        /// <summary>
        /// Gets the effective value of <see cref="PlacementTarget"/> property.
        /// </summary>
        /// <remarks>The <see cref="PlacementTarget"/>  proeprty </remarks>
        public UIElement PlacementTargetResolved
        {
            get
            {
                UIElement target = this.PlacementTarget ?? this.GetBindingValue(PlacementTargetProperty) as UIElement;

                return target ?? this.ParentElement;
            }
        }

        #endregion //PlacementTargetResolved

        #region PlacementRectangle

        /// <summary>
        /// Identifies the <see cref="PlacementRectangle"/> dependency property
        /// </summary>
        public static readonly DependencyProperty PlacementRectangleProperty =
            DependencyProperty.Register("PlacementRectangle",
                                        typeof(Rect), typeof(XamContextMenu),
                                        new PropertyMetadata(Rect.Empty,
                                                             new PropertyChangedCallback(OnPlacementRectangleChanged)));

        private static void OnPlacementRectangleChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            XamContextMenu contextMenu = target as XamContextMenu;

            if (contextMenu != null && contextMenu.IsOpen)
            {
                contextMenu.SetPosition();
            }
        }

        /// <summary>
        /// Gets/sets the rectangle which is relative to the <see cref="PlacementTarget"/> used to calculate the position of <see cref="XamContextMenu"/>.
        /// </summary>
        /// <seealso cref="PlacementRectangleProperty"/>
        public Rect PlacementRectangle
        {
            get { return (Rect)this.GetValue(PlacementRectangleProperty); }

            set { this.SetValue(PlacementRectangleProperty, value); }
        }

        #endregion //PlacementRectangle

        #region HorizontalOffset

        /// <summary>
        /// Identifies the <see cref="HorizontalOffset"/> dependency property
        /// </summary>
        public static readonly DependencyProperty HorizontalOffsetProperty =
            DependencyProperty.Register("HorizontalOffset", typeof(double), typeof(XamContextMenu),
                                        new PropertyMetadata(0.0, new PropertyChangedCallback(OnHorizontalOffsetChanged)));

        private static void OnHorizontalOffsetChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            XamContextMenu contextMenu = target as XamContextMenu;

            if (contextMenu != null)
            {
                contextMenu.SetPosition();
            }
        }

        /// <summary>
        /// Gets/set the horizontal distance between the target origin and the <see cref="XamContextMenu"/> alignment point.
        /// </summary>
        /// <seealso cref="HorizontalOffsetProperty"/>
        public double HorizontalOffset
        {
            get { return (double)this.GetValue(HorizontalOffsetProperty); }

            set { this.SetValue(HorizontalOffsetProperty, value); }
        }

        #endregion //HorizontalOffset

        #region VerticalOffset

        /// <summary>
        /// Identifies the <see cref="VerticalOffset"/> dependency property
        /// </summary>
        public static readonly DependencyProperty VerticalOffsetProperty =
            DependencyProperty.Register("VerticalOffset", typeof(double), typeof(XamContextMenu),
            new PropertyMetadata(0.0, new PropertyChangedCallback(OnVerticalOffsetChanged)));

        private static void OnVerticalOffsetChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            XamContextMenu contextMenu = target as XamContextMenu;

            if (contextMenu != null)
            {
                contextMenu.SetPosition();
            }
        }

        /// <summary>
        /// Gets/sets the vertical distance between the target origin and the <see cref="XamContextMenu"/> alignment point.
        /// </summary>
        /// <seealso cref="VerticalOffsetProperty"/>
        public double VerticalOffset
        {
            get { return (double)this.GetValue(VerticalOffsetProperty); }
            set { this.SetValue(VerticalOffsetProperty, value); }
        }

        #endregion //VerticalOffset

        #region MouseClickLocation

        /// <summary>
        /// Gets the mouse position when the mouse click is used to open the <see cref="XamContextMenu"/>, otherwise this value is invalid.
        /// </summary>
        /// <seealso cref="OpenMode"/>
        public Point MouseClickLocation
        {
            get { return this._mouseClickLocation; }
            internal set { this._mouseClickLocation = value; }
        }

        #endregion //MouseClickLocation

        #region ContextMenuLocation

        /// <summary>
        /// Gets the location of the <see cref="XamContextMenu"/>.
        /// </summary>
        public Point ContextMenuLocation
        {
            get
            {

                

                if (this.IsOpen && this.ParentElement != null && this._contextMenuPopup.IsOpen)
                {
                    
                    FrameworkElement root = PlatformProxy.GetRootVisual(this.ParentElement) as FrameworkElement;
                    if (root == null)
                    {
                        root = PlatformProxy.GetRootParent(this.ParentElement) as FrameworkElement;
                    }

                    if (root != null)
                    {
                        Popup popup = root as Popup;
                        if (popup != null)
                        {
                            

                            root = popup.Child as FrameworkElement;
                        }

                        if ((root != null) && !(root is Popup))
                        {
                            Point location, p = new Point(0, 0);
                            location = this.PointToScreen(p);
                            p = root.PointToScreen(p);

                            location.X -= p.X;
                            location.Y -= p.Y;

                            return location;
                        }
                    }
                }

                return this._contextMenuLocation;
            }

            internal set
            {
                this._contextMenuLocation = value;

				// AS 6/13/12 TFS107208
				// The coordinates have to be relative to the parent of the popup if it has one.
				//
				var parent = this.PopupParent;

				if (null != parent)
				{
					var root = PlatformProxy.GetRootVisual(parent);
					value = root.TransformToVisual(parent).Transform(value);
				}

                this._contextMenuPopup.HorizontalOffset = value.X;
                this._contextMenuPopup.VerticalOffset = value.Y;
            }
        }

        #endregion //ContextMenuLocation

        #region ShouldRightClickBeHandled

        /// <summary>
        /// Identifies the <see cref="ShouldRightClickBeHandled"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty ShouldRightClickBeHandledProperty = DependencyProperty.Register("ShouldRightClickBeHandled", typeof(bool), typeof(XamContextMenu), new PropertyMetadata(true, new PropertyChangedCallback(ShouldRightClickBeHandledChanged)));

        /// <summary>
        /// Gets/Sets whether the RightClick should be handled when displaying the ContextMenu
        /// </summary>
        public bool ShouldRightClickBeHandled
        {
            get { return (bool)this.GetValue(ShouldRightClickBeHandledProperty); }
            set { this.SetValue(ShouldRightClickBeHandledProperty, value); }
        }

        private static void ShouldRightClickBeHandledChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {

        }

        #endregion // ShouldRightClickBeHandled 

		// AS 6/13/12 TFS107208
		#region PopupParent

		/// <summary>
		/// Identifies the <see cref="PopupParent"/> dependency property
		/// </summary>
		public static readonly DependencyProperty PopupParentProperty =
			DependencyProperty.Register("PopupParent", typeof(Panel), typeof(XamContextMenu),
										new PropertyMetadata(new PropertyChangedCallback(OnPopupParentChanged)));

		private static void OnPopupParentChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			XamContextMenu contextMenu = target as XamContextMenu;

			var oldParent = e.OldValue as Panel;
			var newParent = e.NewValue as Panel;

			if (null != oldParent)
				oldParent.Children.Remove(contextMenu._contextMenuPopup);

			if (null != newParent)
				newParent.Children.Add(contextMenu._contextMenuPopup);
		}

		/// <summary>
		/// Gets/sets the Panel to contain the Popup which contains the <see cref="XamContextMenu"/>.
		/// </summary>
		/// <seealso cref="PopupParentProperty"/>
		public Panel PopupParent
		{
			get { return (Panel)this.GetValue(PopupParentProperty); }
			set { this.SetValue(PopupParentProperty, value); }
		}

		#endregion //PopupParent

		#endregion //Public Properties

        #region Internal Properites

        #region ParentElement








        internal UIElement ParentElement
        {
            get
            {
                if (this._targetElement != null && this._targetElement.IsAlive)
                {
                    return this._targetElement.Target as UIElement;
                }

                return null;
            }

            set { this._targetElement = new WeakReference(value); }
        }

        #endregion //ParentElement

        #region IsCancelled

        internal bool IsCancelled { get; set; }

        #endregion //IsCancelled

        #endregion //Internal Properites

        #region Private Properties

        private static DependencyProperty BindingHelperProperty = DependencyProperty.RegisterAttached("BindingHelper",
                                                                                                      typeof(object),
                                                                                                      typeof(
                                                                                                          XamContextMenu
                                                                                                          ),
                                                                                                      new PropertyMetadata
                                                                                                          (null));

        #endregion //Private Properties

        #endregion //Properties

        #region Methods

        #region Protected Methods

        #region OnOpening

        /// <summary>
        /// Raises the <see cref="Opening"/> event.
        /// </summary>
        /// <seealso cref="Opening"/>
        /// <seealso cref="OpeningEventArgs"/>
        /// <seealso cref="XamContextMenu"/>
        protected virtual void OnOpening(OpeningEventArgs args)
        {
            if (this.Opening != null)
            {
                this.Opening(this, args);
            }
        }

        #endregion //OnOpening

        #region OnClosing

        /// <summary>
        /// Raises the <see cref="Closing"/> event.
        /// </summary>
        /// <seealso cref="Closing"/>
        /// <seealso cref="CancellableEventArgs"/>
        /// <seealso cref="XamContextMenu"/>
        protected virtual void OnClosing(CancellableEventArgs args)
        {
            if (this.Closing != null)
            {
                this.Closing(this, args);
            }
        }

        #endregion //OnClosing

        #region OnClosed

        /// <summary>
        /// Raises the <see cref="Closed"/> event.
        /// </summary>
        /// <seealso cref="Closed"/>
        /// <seealso cref="EventArgs"/>
        /// <seealso cref="XamContextMenu"/>
        protected virtual void OnClosed(EventArgs args)
        {
            if (this.Closed != null)
            {
                this.Closed(this, args);
            }
        }

        #endregion //OnClosed

        #region OnOpened

        /// <summary>
        /// Raises the <see cref="Opened"/> event.
        /// </summary>
        /// <seealso cref="Opened"/>
        /// <seealso cref="OpenedEventArgs"/>
        /// <seealso cref="XamContextMenu"/>
        protected virtual void OnOpened(OpenedEventArgs args)
        {
            if (this.Opened != null)
            {
                this.Opened(this, args);
            }
        }

        #endregion //OnOpened

        #endregion //Protected Methods

        #region Internal Methods

        #region GetClickedElements



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)


        internal List<T> GetClickedElements<T>() where T : UIElement
        {
            List<T> elements = new List<T>();
            T parentElement = this.ParentElement as T;

            Point point;





            


            UIElement rootVisual = PlatformProxy.GetRootVisual(this.ParentElement);
            point = rootVisual.TranslatePoint(this.MouseClickLocation, this.ParentElement);


            IEnumerable<UIElement> uiElements = PlatformProxy.GetElementsFromPoint(point, this.ParentElement);

            if (uiElements == null)
            {
                return null;
            }

            if (!uiElements.GetEnumerator().MoveNext())
            {


#region Infragistics Source Cleanup (Region)













#endregion // Infragistics Source Cleanup (Region)

            }
            else
            {

                foreach (UIElement uiElement in uiElements)
                {
                    T element = uiElement as T;
                    if (element != null)
                    {

                        elements.Insert(0, element);



                    }
                }
            }

            


            //foreach (UIElement uiElement in uiElements)
            //{
            //    if (uiElement != this.ParentElement)
            //    {
            //        DependencyObject parent = GetParent(uiElement);
            //        while (parent != null && parent != this.ParentElement)
            //        {
            //            T element = parent as T;
            //            if (element != null && element.IsMouseOver)
            //            {
            //                elements.Add(element);
            //            }

            //            parent = GetParent(parent);
            //        }
            //    }
            //}
            return elements;

            if (parentElement != null && !elements.Contains(parentElement))
            {
                elements.Insert(0, parentElement);
            }

            return elements;
        }

        #endregion //GetClickedElements

        #region OpenInternal

        internal void OpenInternal()
        {
            if (this.Placement == PlacementMode.MouseClick)
            {
                bool wasMouseClick = this.Placement == PlacementMode.MouseClick;

                this._preventPlacementChangeEvent = true;

                PlacementMode old = this.Placement;

                if (wasMouseClick)
                    this.Placement = PlacementMode.Center;

                this.IsOpen = true;

                if (wasMouseClick)
                    this.Placement = old;

                this._preventPlacementChangeEvent = false;
            }
            else
            {
                this.IsOpen = true;
            }
        }

        #endregion // OpenInternal

        #endregion //Internal Methods

        #region Private Methods

        #region Open-Close Context Menu

        private bool ResolveIsOpenChange(DependencyPropertyChangedEventArgs e)
        {
            if (this._skipIsOpenChange)
            {
                this._skipIsOpenChange = false;
                return false; // this is an internal change
            }

            OpeningEventArgs args = new OpeningEventArgs(this, this.MouseClickLocation);
            // the context menu must be attached to some element
            if (this.ParentElement != null)
            {
                if ((bool)e.NewValue)
                {
                    this.OnOpening(args);
                }
                else
                {
                    this.OnClosing(args);


                    
#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

                    if (this._popupInitiatedClose)
                    {
                        args.Cancel = false;
                    }

                }
            }
            else
            {
                args.Cancel = this.IsOpen;
            }

            this.IsCancelled = args.Cancel;

            if (args.Cancel)
            {
                this._skipIsOpenChange = true;
                this.SetValue(IsOpenProperty, e.OldValue);
                return false;
            }

            if (this.GetValue(IsOpenProperty) != e.NewValue)
            {
                this._skipIsOpenChange = true;
                this.SetValue(IsOpenProperty, e.NewValue);
            }

            return true;
        }

        private void OnContextMenuOpened()
        {
            this._previouslyFocusedControl = PlatformProxy.GetFocusedElement(this) as Control;
            //this.Visibility = Visibility.Visible;
            this.IsEnabled = true;

            if (!this._contextMenuPopup.IsOpen)
            {






                this.AddContextMenuToPopup();


                // we need to know the size of the context menu to set position and align
                this.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                this.SetPopupPosition();

                this._contextMenuPopup.IsOpen = true;
                ContextMenuService.OnContextMenuOpened(this);


                Mouse.Capture(this, CaptureMode.SubTree);

            }

            // this code clears menu items visual states
            foreach (var item in Items)
            {
                XamMenuItem menuItem = this.ItemContainerGenerator.ContainerFromItem(item) as XamMenuItem;
                if (menuItem != null)
                {
                    menuItem.IsSelected = false;
                    menuItem.IsMouseOver = false;
                    menuItem.IsHighlighted = false;
                    menuItem.ChangeVisualState(false);
                }
            }



#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

        }


        




        private void AddContextMenuToPopup()
        {
            this.RemoveContextMenuFromPopup();

            Grid grid = new Grid();
            this._contextMenuPopup.Child = grid;
            grid.Children.Add(this);
            grid.UpdateLayout();
        }

        private void RemoveContextMenuFromPopup()
        {
            Grid grid = this._contextMenuPopup.Child as Grid;

            if (grid != null)
            {
                grid.Children.Clear();
                this._contextMenuPopup.Child = null;
            }
        }



        private void OnContextMenuClosed()
        {
            bool isPopupInitiatedClose = false;


            isPopupInitiatedClose = this._popupInitiatedClose;


            if (this._contextMenuPopup.IsOpen || isPopupInitiatedClose)
            {


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)


                if (this.IsChild(PlatformProxy.GetFocusedElement(this) as FrameworkElement))
                {
                    if (this._previouslyFocusedControl == null)
                    {
                        this._previouslyFocusedControl = this._focusedControl;
                        this._focusedControl = null;
                    }

                    this.ReturnFocus();
                }

                this._previouslyFocusedControl = null;

                //this.Visibility = Visibility.Collapsed;
                foreach (var item in Items)
                {
                    XamMenuItem menuItem = this.ItemContainerGenerator.ContainerFromItem(item) as XamMenuItem;
                    if (menuItem != null)
                    {
                        if (menuItem.IsSubmenuOpen)
                        {
                            menuItem.CloseSubmenu();
                        }
                    }
                }

                this.RemoveContextMenuFromPopup();
                Mouse.Capture(null);

                this._contextMenuPopup.Child = null;
                this._contextMenuPopup.IsOpen = false;
                ContextMenuService.OnContextMenuClosed(this);
            }

            this.OnClosed(EventArgs.Empty);
        }

        private bool IsChild(FrameworkElement element)
        {
            return IsObjectFromMenu(this, element);
        }

        private bool IsChildOfParentElement(DependencyObject element)
        {
            while (element != null)
            {
                if (this.ParentElement == element)
                {
                    return true;
                }

                element = GetParent(element);
            }

            return false;
        }

        #endregion //Open-Close Context Menu

        #region Popup

        private double GetActualWidth()
        {
            return this.ActualWidth > 0 ? this.ActualWidth : this.DesiredSize.Width;
        }

        private double GetActualHeight()
        {
            return this.ActualHeight > 0 ? this.ActualHeight : this.DesiredSize.Height;
        }

        private void SetPopupPosition()
        {
            double zoomFactor = PlatformProxy.GetZoomFactor();
            Size hostContentSize = PlatformProxy.ResolveContainerSize();
            double width = hostContentSize.Width;
            double height = hostContentSize.Height;

            bool mustRepairZoom = zoomFactor > 0 && !PlatformProxy.IsVersionSupported("4.0");
            if (mustRepairZoom)
            {
                width /= zoomFactor;
                height /= zoomFactor;
            }

            FrameworkElement root = PlatformProxy.GetRootVisual(this) as FrameworkElement;
            if (root == null)
            {
                root = PlatformProxy.GetRootVisual(this.ParentElement as DependencyObject) as FrameworkElement;
            }
            Rect targetRect = this.GetTargetRect();
            Point p = this.AlignToRect(targetRect, root);



#region Infragistics Source Cleanup (Region)






























































#endregion // Infragistics Source Cleanup (Region)

            FrameworkElement parent = this.ParentElement as FrameworkElement;
            if (parent != null && this.Placement == PlacementMode.MouseClick &&
                FrameworkElement.GetFlowDirection(parent) == System.Windows.FlowDirection.RightToLeft)
            {
                
                p.X = parent.ActualWidth - p.X;
            }

            
            this.ContextMenuLocation = p;

            this._skipPopupEvents = this._contextMenuPopup.IsOpen && !this.IsMouseCaptured;
            if (this._skipPopupEvents)
            {
                

                this._contextMenuPopup.IsOpen = false;
                this._contextMenuPopup.IsOpen = true;
                Mouse.Capture(this, CaptureMode.SubTree);
                this._skipPopupEvents = false;
            }


            this._mustSetPosition = false;

            if (mustRepairZoom)
            {
                _zoomTransform.ScaleX = _zoomTransform.ScaleY = zoomFactor;
            }
        }

        #endregion //Popup

        #region Positioning

        private void SetPosition()
        {
            if (this.IsOpen)
            {
                this.SetPopupPosition();
            }
        }

        private Point AlignToRect(Rect rect, FrameworkElement root)
        {
            double x = this.HorizontalOffset, y = this.VerticalOffset; // offset from the position  
            double menuWidth = this.GetActualWidth();
            double menuHeight = this.GetActualHeight();

            if (!rect.IsEmpty)
            {
                switch (this.Placement)
                {
                    case PlacementMode.AlignedBelow:
                        if ((root != null && root.FlowDirection == FlowDirection.LeftToRight) && this.FlowDirection == System.Windows.FlowDirection.RightToLeft)
                        {
                            x = rect.Right;
                        }
                        else if (this.FlowDirection == System.Windows.FlowDirection.LeftToRight || (root != null && root.FlowDirection == FlowDirection.LeftToRight))
                        {
                            x += rect.Left;
                        }
                        else if (this.FlowDirection == System.Windows.FlowDirection.RightToLeft && (root != null && root.FlowDirection == FlowDirection.RightToLeft))
                        {
                            x = rect.Right;
                        }
                        else if (root != null)
                        {
                            x += root.ActualWidth - rect.Left;
                        }
                        else
                        {
                            x += -rect.Left;
                        }
                        y += rect.Bottom; // the context menu top is on the bottom of the target rectangle
                        break;
                    case PlacementMode.Center:
                        if ((root != null && root.FlowDirection == FlowDirection.LeftToRight) && this.FlowDirection == System.Windows.FlowDirection.RightToLeft)
                        {
                            x += (rect.Right + rect.Left + menuWidth) / 2;
                        }
                        else if (this.FlowDirection == System.Windows.FlowDirection.LeftToRight || (root != null && root.FlowDirection == FlowDirection.LeftToRight))
                        {
                            x += (rect.Left + rect.Right - menuWidth) / 2;
                        }
                        else if (this.FlowDirection == System.Windows.FlowDirection.RightToLeft && (root != null && root.FlowDirection == FlowDirection.RightToLeft))
                        {
                            x += (rect.Left + rect.Right + menuWidth) / 2;
                        }
                        else
                        {
                            x += (rect.Left + rect.Right - menuWidth) / 2;
                        }
                        y += (rect.Top + rect.Bottom - menuHeight) / 2;
                        break;
                    case PlacementMode.AlignedToTheRight:
                        if ((root != null && root.FlowDirection == FlowDirection.LeftToRight) && this.FlowDirection == System.Windows.FlowDirection.RightToLeft)
                        {
                            x += (rect.Left); // the context menu left side is on the right edge of the target rectangle
                        }
                        else if (this.FlowDirection == System.Windows.FlowDirection.LeftToRight || (root != null && root.FlowDirection == FlowDirection.LeftToRight))
                        {
                            x += rect.Right; // the context menu left side is on the right edge of the target rectangle
                        }
                        else if (this.FlowDirection == System.Windows.FlowDirection.RightToLeft && (root != null && root.FlowDirection == FlowDirection.RightToLeft))
                        {
                            x += (rect.Left);
                        }
                        else
                        {
                            x += rect.Right; // the context menu left side is on the right edge of the target rectangle
                        }
                        y += rect.Top;
                        break;
                    case PlacementMode.AlignedToTheLeft:
                        if ((root != null && root.FlowDirection == FlowDirection.LeftToRight) && this.FlowDirection == System.Windows.FlowDirection.RightToLeft)
                        {
                            x += rect.Right + menuWidth;
                        }
                        else if (this.FlowDirection == System.Windows.FlowDirection.LeftToRight || (root != null && root.FlowDirection == FlowDirection.LeftToRight))
                        {
                            x += rect.Left - menuWidth;
                        }
                        else if (this.FlowDirection == System.Windows.FlowDirection.RightToLeft && (root != null && root.FlowDirection == FlowDirection.RightToLeft))
                        {
                            x += rect.Right + menuWidth;
                        }
                        else
                        {
                            x += rect.Left - menuWidth;
                        }
                        // the context menu right side is on the left edge of the target rectangle
                        y += rect.Top;
                        break;
                    case PlacementMode.AlignedAbove:
                        if ((root != null && root.FlowDirection == FlowDirection.LeftToRight) && this.FlowDirection == System.Windows.FlowDirection.RightToLeft)
                        {
                            x = rect.Right;
                        }
                        else if (this.FlowDirection == System.Windows.FlowDirection.LeftToRight || (root != null && root.FlowDirection == FlowDirection.LeftToRight))
                        {
                            x += rect.Left;
                        }
                        else if (this.FlowDirection == System.Windows.FlowDirection.RightToLeft && (root != null && root.FlowDirection == FlowDirection.RightToLeft))
                        {
                            x = rect.Right;
                        }
                        else if (root != null)
                        {
                            x += root.ActualWidth - rect.Left;
                        }
                        else
                        {
                            x += -rect.Left;
                        }
                        y += rect.Top - menuHeight;
                        // the context menu bottom side is on the top edge of the target rectangle
                        break;

                    // inside a target rectangle cases
                    case PlacementMode.AlignedTop:
                        if ((root != null && root.FlowDirection == FlowDirection.LeftToRight) && this.FlowDirection == System.Windows.FlowDirection.RightToLeft)
                        {
                            x = rect.Right;
                        }
                        else if (this.FlowDirection == System.Windows.FlowDirection.LeftToRight || (root != null && root.FlowDirection == FlowDirection.LeftToRight))
                        {
                            x += rect.Left;
                        }
                        else if (this.FlowDirection == System.Windows.FlowDirection.RightToLeft && (root != null && root.FlowDirection == FlowDirection.RightToLeft))
                        {
                            x = rect.Right;
                        }
                        else
                        {
                            x += -rect.Left;
                        }
                        y += rect.Top; // top - top
                        break;
                    case PlacementMode.AlignedBottom:
                        if ((root != null && root.FlowDirection == FlowDirection.LeftToRight) && this.FlowDirection == System.Windows.FlowDirection.RightToLeft)
                        {
                            x = rect.Right;
                        }
                        else if (this.FlowDirection == System.Windows.FlowDirection.LeftToRight || (root != null && root.FlowDirection == FlowDirection.LeftToRight))
                        {
                            x += rect.Left;
                        }
                        else if (this.FlowDirection == System.Windows.FlowDirection.RightToLeft && (root != null && root.FlowDirection == FlowDirection.RightToLeft))
                        {
                            x = rect.Right;
                        }
                        else
                        {
                            x += -rect.Left;
                        }
                        y += rect.Bottom - menuHeight; // bottom - bottom
                        break;
                    case PlacementMode.AlignedLeft:
                        if ((root != null && root.FlowDirection == FlowDirection.LeftToRight) && this.FlowDirection == System.Windows.FlowDirection.RightToLeft)
                        {
                            x += rect.Right; // left - left
                        }
                        else if (this.FlowDirection == System.Windows.FlowDirection.LeftToRight || (root != null && root.FlowDirection == FlowDirection.LeftToRight))
                        {
                            x += rect.Left; // left - left
                        }
                        else if (this.FlowDirection == System.Windows.FlowDirection.RightToLeft && (root != null && root.FlowDirection == FlowDirection.RightToLeft))
                        {
                            x = rect.Right;
                        }
                        else
                        {
                            x += root.ActualWidth - rect.Left;
                        }

                        y += rect.Top;
                        break;
                    case PlacementMode.AlignedRight:
                        if ((root != null && root.FlowDirection == FlowDirection.LeftToRight) && this.FlowDirection == System.Windows.FlowDirection.RightToLeft)
                        {
                            x += rect.Left + menuWidth; // right - right
                        }
                        else if (this.FlowDirection == System.Windows.FlowDirection.LeftToRight || (root != null && root.FlowDirection == FlowDirection.LeftToRight))
                        {
                            x += rect.Right - menuWidth; // right - right
                        }
                        else if (this.FlowDirection == System.Windows.FlowDirection.RightToLeft && (root != null && root.FlowDirection == FlowDirection.RightToLeft))
                        {
                            x += (rect.Left + menuWidth);
                        }
                        else
                        {
                            x += rect.Right - menuWidth; // right - right
                        }
                        y += rect.Top;
                        break;

                    default: // MouseClick and Manual
                        x += rect.Left;
                        y += rect.Top;
                        break;
                }
            }

            return new Point(x, y);
        }


        
#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

        private Point AlignToScreen(Rect menu, Rect screen, Rect target, FrameworkElement root)
        {
            if (this.FlowDirection == FlowDirection.LeftToRight)
            {
                if (menu.Width >= screen.Width)
                {
                    menu.X = screen.X;
                }
                else
                {
                    if (menu.Left < screen.Left)
                    {
                        menu.X = screen.X; // Aligns to the left edge.
                    } // end if - Left edge

                    if (menu.Right > screen.Right)
                    {
                        if (this.Placement == PlacementMode.MouseClick)
                        {
                            menu.X -= menu.Width - target.Width; // on the left side
                        }
                        else
                        {
                            menu.X -= menu.Right - screen.Right; // Aligns to the right edge.
                        }
                    } // end if - Right edge

                    if (menu.X < screen.X)
                    {
                        menu.X = screen.X; // Aligns to the left edge.
                    }

                    if (menu.Right > screen.Right)
                    {
                        menu.X -= menu.Right - screen.Right; // Aligns to the right edge.
                    }
                } // end else - horizontal alignment
            }
            else
            {
                




                


                if (menu.Width >= screen.Width)
                {
                    menu.X = screen.Right;
                }
                else
                {
                    if (menu.Left < screen.Left)
                    {
                        menu.X = screen.Right; // Aligns to the right edge.
                    } // end if - Right edge
                    else
                        if (menu.Left < screen.Left)
                        {
                            if (this.Placement == PlacementMode.MouseClick)
                            {
                                menu.X -= menu.Width - target.Width; // on the left side
                            }
                            else
                            {
                                menu.X -= screen.Left - menu.Left; // Aligns to the left edge.
                            }
                        } // end if - Right edge
                        else if (menu.X - menu.Width < screen.Left)
                        {
                            menu.X += menu.Width - menu.X;
                        }
                } // end else - horizontal alignment
            }

            if (menu.Height >= screen.Height)
            {
                menu.Y = screen.Y;
            }
            else
            {
                if (menu.Top < screen.Top)
                {
                    menu.Y = screen.Y; // Aligns to the top edge.
                } // end if - Top edge

                if (menu.Bottom > screen.Bottom)
                {
                    if (this.Placement == PlacementMode.MouseClick)
                    {
                        menu.Y -= menu.Height - target.Height; // above mouse
                    }
                    else
                    {
                        menu.Y -= menu.Bottom - screen.Bottom; // Aligns to the bottom edge.
                    }
                } // end if - Top edge

                if (menu.Top < screen.Top)
                {
                    menu.Y = screen.Y; // Aligns to the top edge.
                }

                if (menu.Bottom > screen.Bottom)
                {
                    menu.Y -= menu.Bottom - screen.Bottom; // Aligns to the bottom edge.
                }
            } // end else - vertical alignment

            return new Point(menu.Left, menu.Top);
        }

        private Rect GetTargetRect()
        {
            Rect rect = Rect.Empty;



#region Infragistics Source Cleanup (Region)
















#endregion // Infragistics Source Cleanup (Region)

            UIElement targetElement = this.PlacementTargetResolved;
            if (targetElement != null)
            {
                rect = ContextMenuService.GetBounds(targetElement);

                // For some reason the PlacementRectangle wasn't taken into account for WPF originally. 
                // Adding this logic now.  SZ: (56770)
                if (!this.PlacementRectangle.IsEmpty)
                {
                    rect.X += this.PlacementRectangle.Left;
                    rect.Y += this.PlacementRectangle.Top;
                    rect.Width = this.PlacementRectangle.Width;
                    rect.Height = this.PlacementRectangle.Height;
                }
            }

            if (this.Placement == PlacementMode.MouseClick)
            {
                rect.X = this.MouseClickLocation.X - rect.X;
                rect.Y = this.MouseClickLocation.Y - rect.Y;
            }
            else
            {
                rect.X = 0;
                rect.Y = 0;
            }

            this._contextMenuPopup.PlacementTarget = targetElement;
            this._contextMenuPopup.Placement = System.Windows.Controls.Primitives.PlacementMode.Relative;
            this._contextMenuPopup.PlacementRectangle = this.PlacementRectangle;


            return rect;
        }

        private object GetBindingValue(DependencyProperty property)
        {
            BindingExpression be = this.GetBindingExpression(property);
            FrameworkElement fe = this.ParentElement as FrameworkElement;

            if (be != null && fe != null)
            {
                Binding binding = be.ParentBinding;
                fe.SetBinding(BindingHelperProperty, binding);
                return fe.GetValue(BindingHelperProperty);
            }

            return null;
        }

        #endregion //Positioning

        #endregion //Private Methods

        #endregion //Methods

        #region Events

        #region Opening

        /// <summary>
        /// Occurs before a particular instance of a <see cref="XamContextMenu"/> opens.
        /// </summary>
        /// <seealso cref="Opened"/>
        /// <seealso cref="Closing"/>
        /// <seealso cref="Closed"/>
        public event EventHandler<OpeningEventArgs> Opening;

        #endregion //Opened

        #region Opened

        /// <summary>
        /// Occurs after a particular instance of a <see cref="XamContextMenu"/> opens.
        /// </summary>
        /// <seealso cref="Opening"/>
        /// <seealso cref="Closing"/>
        /// <seealso cref="Closed"/>
        public event EventHandler<OpenedEventArgs> Opened;

        #endregion //Opened

        #region Closing

        /// <summary>
        /// Occurs before a particular instance of a <see cref="XamContextMenu"/> closes.
        /// </summary>
        /// <seealso cref="Closed"/>
        /// <seealso cref="Opening"/>
        /// <seealso cref="Opened"/>
        public event EventHandler<CancellableEventArgs> Closing;

        #endregion //Opened

        #region Closed

        /// <summary>
        /// Occurs after a particular instance of a <see cref="XamContextMenu"/> closes.
        /// </summary>
        /// <seealso cref="Closing"/>
        /// <seealso cref="Opening"/>
        /// <seealso cref="Opened"/>
        public event EventHandler<EventArgs> Closed;

        #endregion //Closed

        #endregion //Events

        #region Event Handlers

        


#region Infragistics Source Cleanup (Region)











#endregion // Infragistics Source Cleanup (Region)

        private void Popup_Closed(object sender, EventArgs e)
        {

            if (this._skipPopupEvents)
            {
                return;
            }

            if(this.IsOpen)
            {
                this._popupInitiatedClose = true;
                this.CloseCurrentOpen();
                this._popupInitiatedClose = false;
            }

            this._mustSetPosition = false;
        }

        private void Popup_Opened(object sender, EventArgs e)
        {

            if (this._skipPopupEvents)
            {
                return;
            }

            if (this.GetActualHeight() < 1)
            {
                this.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                this._mustSetPosition = true;
            }

            this.SetPopupPosition();

            this.OnOpened(new OpenedEventArgs(this, this.MouseClickLocation, this.ContextMenuLocation));

            // find and select first enabled menu item; keyboard navigation works on focused element
            if (this.Items.Count > 0)
            {
                XamMenuItem item = this.ItemContainerGenerator.ContainerFromIndex(0) as XamMenuItem;
                if (item != null)
                {
                    if (item.IsEnabled)
                    {
                        item.Focus();
                    }
                    else
                    {
                        item = item.FindSiblingItem(true, false);
                        if (item != null)
                        {
                            item.Focus();
                        }
                    }
                }
            }


        }

        private void XamContextMenu_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this._mustSetPosition && this._contextMenuPopup.IsOpen)
            {
                this.SetPopupPosition();
            }
        }

        #endregion //Event Handlers

        #region Commands

        #region  GetParameter

        /// <summary>
        /// Gets the object that defines the parameters necessary to execute the command.
        /// </summary>
        /// <param name="source">The CommandSource object which defines the command to be executed.</param>
        /// <returns>The object necessary for the command to complete.</returns>
        protected virtual object GetParameter(CommandSource source)
        {
            if (source.Command is XamContextMenuCommandBase)
            {
                return this;
            }

            return null;
        }

        #endregion // GetParameter

        #region SupportsCommand

        /// <summary>
        /// Gets if the object will support a given command type.
        /// </summary>
        /// <param name="command">The command to be validated.</param>
        /// <returns>
        /// True if the object recognizes the command as actionable against it.
        /// </returns>
        protected virtual bool SupportsCommand(ICommand command)
        {
            return command is XamContextMenuCommandBase;
        }

        #endregion //SupportsCommand

        #region ICommandTarget Members

        bool ICommandTarget.SupportsCommand(ICommand command)
        {
            return this.SupportsCommand(command);
        }

        object ICommandTarget.GetParameter(CommandSource source)
        {
            return this.GetParameter(source);
        }

        #endregion

        #endregion //Commands
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