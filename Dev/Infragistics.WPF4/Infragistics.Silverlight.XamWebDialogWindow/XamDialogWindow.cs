using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Diagnostics;
using Infragistics.AutomationPeers;
using Infragistics.Controls.Interactions.Primitives;
using System.Collections.Generic;

namespace Infragistics.Controls.Interactions
{

    /// <summary>
    /// A control that provides windowing like behavior to display custom content. 
    /// </summary>
    [TemplatePart(Name = "HeaderArea", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "Resize", Type = typeof(Border))]
    [TemplatePart(Name = "Root", Type = typeof(DialogRootPanel))]
    [TemplatePart(Name = "ContainerContext", Type = typeof(Grid))]
    [TemplatePart(Name = "NWCursor", Type = typeof(DataTemplate))]
    [TemplatePart(Name = "SECursor", Type = typeof(DataTemplate))]
    [TemplatePart(Name = "NECursor", Type = typeof(DataTemplate))]
    [TemplatePart(Name = "SWCursor", Type = typeof(DataTemplate))]

    [TemplateVisualState(GroupName = "ActiveStates", Name = "Inactive")]
    [TemplateVisualState(GroupName = "ActiveStates", Name = "Active")]

    [TemplateVisualState(GroupName = "ModalStates", Name = "NonModal")]
    [TemplateVisualState(GroupName = "ModalStates", Name = "Modal")]

    [TemplateVisualState(GroupName = "HeaderIconStates", Name = "HeaderIconIsVisible")]
    [TemplateVisualState(GroupName = "HeaderIconStates", Name = "HeaderIconIsNotVisible")]

    [TemplateVisualState(GroupName = "WindowStateStates", Name = "Maximized")]
    [TemplateVisualState(GroupName = "WindowStateStates", Name = "Normal")]
    [TemplateVisualState(GroupName = "WindowStateStates", Name = "Minimized")]
    [TemplateVisualState(GroupName = "WindowStateStates", Name = "Disabled")]

    [TemplateVisualState(GroupName = "HiddenStates", Name = "Hidden")]
    [TemplateVisualState(GroupName = "HiddenStates", Name = "Visible")]

    [TemplateVisualState(GroupName = "CloseButtonStates", Name = "CloseButtonIsVisible")]
    [TemplateVisualState(GroupName = "CloseButtonStates", Name = "CloseButtonIsNotVisible")]

    [TemplateVisualState(GroupName = "MaximizeButtonStates", Name = "MaximizeButtonIsVisible")]
    [TemplateVisualState(GroupName = "MaximizeButtonStates", Name = "MaximizeButtonIsNotVisible")]

    [TemplateVisualState(GroupName = "MinimizeButtonStates", Name = "MinimizeButtonIsVisible")]
    [TemplateVisualState(GroupName = "MinimizeButtonStates", Name = "MinimizeButtonIsNotVisible")]

	
	

	// MD 6/14/11
	// Made partial so we can define a nested class in the DialogManager.cs file.
    //public class XamDialogWindow : ContentControl, ICommandTarget, INotifyPropertyChanged, IProvidePropertyPersistenceSettings
	public partial class XamDialogWindow : ContentControl, ICommandTarget, INotifyPropertyChanged, IProvidePropertyPersistenceSettings
    {
        #region Members

        private DialogRootPanel _rootElement;
        private Grid _containerContext;
        private CustomCursors _customCursors;
        private Popup _customCursorPopup;
        private Border _resizeArea;
        private KeyboardSettings _keyboardSettings;
        private Point _lastMouseDownPoint, _clickPoint;
        private DateTime _lastMouseDownTime;
        private Point _oldPos, _previousPos, _resizeDir;

        private WindowState _oldWindowsState = Infragistics.Controls.Interactions.WindowState.Normal,
                            _newWindowState = Infragistics.Controls.Interactions.WindowState.Normal;
        private Rect _restore;
        private FrameworkElement _headerArea, _maximizedParentElement;
        private bool _isDetached, _isDesign, _isResizing, _isMoving;
        private bool _isOut;
        
        private WindowState _startupOldWindowState = Infragistics.Controls.Interactions.WindowState.Normal;
        private WindowState _startupNewWindowState = Infragistics.Controls.Interactions.WindowState.Normal;
        private double _virtualHeight, _virtualWidth;
        private Cursor _previousResizeCursor;
        internal TranslateTransform _moveTransform;
        private DataTemplate _nwCursor;
        private DataTemplate _seCursor;
        private DataTemplate _neCursor;
        private DataTemplate _swCursor;
        private bool _isNewWindowStateCancelled; 
        private bool _isLeftCancelled; 
        private bool _isTopCancelled; 
        private bool _isLoaded;
        private double _left;
        private double _top;
        private bool _isUpdatingWhenMaximized;
        private bool _isMouseOverResizeArea = true;

        private bool _contentChanged;


        
        private Effect _originalEffect; 
        private bool _isModalBackgroundEffect; 
        private bool _startupPositionInitialized;
        private bool _orderUpdated;
        private System.Windows.Shapes.Rectangle _modalLayer;
        private Canvas _contextPanel;
		private FrameworkElement _modalLayerContainer; // AS 6/22/12 TFS115111

        #endregion Members

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="XamDialogWindow"/> class.
        /// </summary>
        public XamDialogWindow()
        {
            this.DefaultStyleKey = typeof(XamDialogWindow);
            this._isDesign = DesignerProperties.GetIsInDesignMode(this);
            this.IsEnabledChanged += new DependencyPropertyChangedEventHandler(XamWebDialogWindow_IsEnabledChanged);
            this._moveTransform = new TranslateTransform();
            this._lastMouseDownPoint = new Point();
            this._lastMouseDownTime = new DateTime();

            this.SizeChanged += new SizeChangedEventHandler(XamWebDialogWindow_SizeChanged);
            this.Unloaded += new RoutedEventHandler(XamDialogWindow_Unloaded);
            this.LayoutUpdated += new EventHandler(XamDialogWindow_LayoutUpdated);


            

            this.RestrictInContainer = true;

            Infragistics.Windows.Utilities.ValidateLicense(typeof(XamDialogWindow), this);            




#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		}

        #endregion Constructors

        #region Overrides

        #region OnApplyTemplate

        /// <summary>
        /// When the template is applied, this loads all the template parts.
        /// </summary>
        public override void OnApplyTemplate()
        {
            if (this._restore.Height != 0)
            {
                this._restore = Rect.Empty;
                this.ClearValue(Control.HeightProperty);
                this.ClearValue(Control.WidthProperty);
            }

            base.OnApplyTemplate();

            if (this._headerArea != null)
            {
                this._headerArea.RemoveHandler(UIElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(this.HeaderArea_MouseLeftButtonDown));
                this._headerArea.RemoveHandler(UIElement.MouseLeftButtonUpEvent, new MouseButtonEventHandler(this.HeaderArea_MouseLeftButtonUp));
                this._headerArea.MouseMove -= this.HeaderArea_MouseMove;
            }

            if (this._resizeArea != null)
            {
                this._resizeArea.MouseLeftButtonDown -= this.ResizeArea_MouseLeftButtonDown;
                this._resizeArea.MouseMove -= this.ResizeArea_MouseMove;
                this._resizeArea.MouseEnter -= this.ResizeArea_MouseEnter;
                this._resizeArea.MouseLeftButtonUp -= this.ResizeArea_MouseLeftButtonUp;
                this._resizeArea.MouseLeave -= this.ResizeArea_MouseLeave;
            }

            if (this._rootElement != null)
            {
                this._rootElement.DialogWindow = null;
                this._rootElement.LayoutUpdated -= this.RootElement_LayoutUpdated;
                this._rootElement.RemoveHandler(UIElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(this.RootElement_MouseLeftButtonDown));

                // Ensure that we're not already registered
                DialogManager.Manager.Unregister(this);
                // Ensure that we're not already registered
                DialogManager.Manager.UnregisterModal(this);

                if (this.MinimizedPanel != null && this.MinimizedPanel.Children.Contains(this._rootElement))
                    this.MinimizedPanel.Children.Remove(this._rootElement);
            }

            this._headerArea = this.GetTemplateChild("HeaderArea") as Border;
            this._resizeArea = this.GetTemplateChild("Resize") as Border;
            DialogRootPanel root = this.GetTemplateChild("Root") as DialogRootPanel;

            if (root == null && this._rootElement != null)
            {
                this.RestrictInContainer = true;
                this.IsModal = false;
            }

            this._rootElement = root;

            this._containerContext = this.GetTemplateChild("ContainerContext") as Grid;

            if (this._containerContext != null)
            {
                this._nwCursor = this._containerContext.Resources["NWCursor"] as DataTemplate;
                this._seCursor = this._containerContext.Resources["SECursor"] as DataTemplate;
                this._neCursor = this._containerContext.Resources["NECursor"] as DataTemplate;
                this._swCursor = this._containerContext.Resources["SWCursor"] as DataTemplate;


                
                Style focusStyle = this._containerContext.Resources["FocusVisualStyle"] as Style;
                if (focusStyle != null)
                {
                    

                    Setter data = new Setter(Control.DataContextProperty, this);
                    focusStyle.Setters.Add(data);

                    this.FocusVisualStyle = focusStyle;
                }

            }

            if (this._rootElement != null)
            {
                this._rootElement.DialogWindow = this;
                this._rootElement.LayoutUpdated += new EventHandler(RootElement_LayoutUpdated);
                this._rootElement.RenderTransform = this._moveTransform;
                this._rootElement.MinHeight = this.MinHeight;
                this._rootElement.MinWidth = this.MinWidth;
                this._rootElement.AddHandler(UIElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(this.RootElement_MouseLeftButtonDown), true);
                this.EnsureWindowState(WindowState.Normal, this.WindowState);

                this._rootElement.Height = this.Height;
                this._rootElement.Width = this.Width;

            }

            if (this._headerArea != null)
            {
                this._headerArea.AddHandler(UIElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(this.HeaderArea_MouseLeftButtonDown), true);
                this._headerArea.AddHandler(UIElement.MouseLeftButtonUpEvent, new MouseButtonEventHandler(this.HeaderArea_MouseLeftButtonUp), true);
                this._headerArea.MouseMove += new MouseEventHandler(this.HeaderArea_MouseMove);
            }

            if (this._resizeArea != null)
            {
                this._resizeArea.MouseLeftButtonDown += new MouseButtonEventHandler(this.ResizeArea_MouseLeftButtonDown);
                this._resizeArea.MouseMove += new MouseEventHandler(this.ResizeArea_MouseMove);
                this._resizeArea.MouseEnter += new MouseEventHandler(this.ResizeArea_MouseEnter);
                this._resizeArea.MouseLeftButtonUp += new MouseButtonEventHandler(this.ResizeArea_MouseLeftButtonUp);
                this._resizeArea.MouseLeave += new MouseEventHandler(this.ResizeArea_MouseLeave);
            }

            if (this._rootElement != null)
            {
                this.AddHandlersToOwningContainer();
                
                // Since the ControlTemplate has changed, make sure we mark this as detached, so everything can be re-registered.
                this._isDetached = false;

                this.DetachFromLayout();
                this.EnsureVisualStates();
                this.Loaded -= this.XamWebDialogWindow_Loaded;
                this.Loaded += this.XamWebDialogWindow_Loaded;
            }

        }

        #endregion // OnApplyTemplate

        #region OnCreateAutomationPeer()
        /// <summary>
        /// When implemented in a derived class, returns class-specific <see cref="T:System.Windows.Automation.Peers.AutomationPeer"/> implementations for the automation infrastructure.
        /// </summary>
        /// <returns>
        /// The class-specific <see cref="T:System.Windows.Automation.Peers.AutomationPeer"/> subclass to return.
        /// </returns>
        protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer()
        {
            return new XamDialogWindowAutomationPeer(this);
        }
        #endregion //OnCreateAutomationPeer()

        #region OnKeyDown

        /// <summary>
        /// Overrides the framework invocation when a user presses a key.
        /// </summary>
        /// <param name="e">Data about the keydown event.</param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (this.KeyboardSettings.AllowKeyboardNavigation == false || e.Handled)
                return;


#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)


            if (this.IsMoveable && this.WindowState == WindowState.Normal && this.IsActive)
            {
                switch (e.Key)
                {
                    case Key.Left:
                        this.Left -= this.KeyboardSettings.HorizontalStep;
                        e.Handled = true;
                        break;

                    case Key.Right:
                        this.Left += this.KeyboardSettings.HorizontalStep;
                        e.Handled = true;
                        break;

                    case Key.Down:
                        this.Top += this.KeyboardSettings.VerticalStep;
                        e.Handled = true;
                        break;

                    case Key.Up:
                        this.Top -= this.KeyboardSettings.VerticalStep;
                        e.Handled = true;
                        break;
                }
            }
        }
        #endregion // OnKeyDown


        #region ArrangeOverride
        /// <summary>
        /// Called to arrange and size the content of a <see cref="T:System.Windows.Controls.Control"/> object.
        /// </summary>
        /// <param name="arrangeBounds">The computed size that is used to arrange the content.</param>
        /// <returns>The size of the control.</returns>
        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            
            FrameworkElement container = XamDialogWindow.ResolveContainer(this);
            if (container == null)
            {
                this._startupPositionInitialized = false;
            }
            else
            {
                if (this._isDetached &&
                    (this.HorizontalAlignment != System.Windows.HorizontalAlignment.Left || this.VerticalAlignment != System.Windows.VerticalAlignment.Top))
                {
                    
                    Point origin = this.GetContainerPanelCoords(container);
                    if (this.IsModal)
                    {
                        this.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                        this.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                        this.SetPosition(origin.X, origin.Y);
                    }
                    else
                    {
                        if (this.HorizontalAlignment != System.Windows.HorizontalAlignment.Left)
                            this.Left = 0;
                        if (this.VerticalAlignment != System.Windows.VerticalAlignment.Top)
                            this.Top = 0;
                    }
                }
            }

            if (!this._startupPositionInitialized)
            {
                if (container != null)
                {
                    this.UpdateZOrder();
                    if (container.ActualHeight > 0 && container.ActualWidth > 0 && this.ActualHeight > 0 && this.ActualWidth > 0)
                    {
                        this._startupPositionInitialized = true;
                        this.SetStartupPosition();
                    }
                    else
                    {
                        container.LayoutUpdated += Container_LayoutUpdated;
                    }
                }
            }

            return base.ArrangeOverride(arrangeBounds);
        }

        #endregion //ArrangeOverride

        #endregion Overrides

        #region Properties

        #region Public Properties

        #region CloseButtonVisibility

        /// <summary>
        /// Identifies the <see cref="CloseButtonVisibility"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty CloseButtonVisibilityProperty = DependencyProperty.Register("CloseButtonVisibility", typeof(Visibility), typeof(XamDialogWindow), new PropertyMetadata(Visibility.Visible, new PropertyChangedCallback(CloseButtonVisibilityPropertyChanged)));

        /// <summary>
        /// Gets/Sets whether the Close button is visible on the <see cref="XamDialogWindow"/>
        /// </summary>
        public Visibility CloseButtonVisibility
        {
            get { return (Visibility)this.GetValue(CloseButtonVisibilityProperty); }
            set { this.SetValue(CloseButtonVisibilityProperty, value); }
        }

        private static void CloseButtonVisibilityPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamDialogWindow dialog = (XamDialogWindow)obj;
            dialog.EnsureVisualStates();
        }

        #endregion // CloseButtonVisibility

        #region MaximizeButtonVisibility

        /// <summary>
        /// Identifies the <see cref="MaximizeButtonVisibility"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty MaximizeButtonVisibilityProperty = DependencyProperty.Register("MaximizeButtonVisibility", typeof(Visibility), typeof(XamDialogWindow), new PropertyMetadata(Visibility.Visible, new PropertyChangedCallback(MaximizeButtonVisibilityPropertyChanged)));

        /// <summary>
        /// Gets/Sets whether the Maximize button is visible on the <see cref="XamDialogWindow"/>
        /// </summary>
        public Visibility MaximizeButtonVisibility
        {
            get { return (Visibility)this.GetValue(MaximizeButtonVisibilityProperty); }
            set { this.SetValue(MaximizeButtonVisibilityProperty, value); }
        }

        private static void MaximizeButtonVisibilityPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamDialogWindow dialog = (XamDialogWindow)obj;
            dialog.EnsureVisualStates();
        }

        #endregion // MaximizeButtonVisibility

        #region MinimizeButtonVisibility

        /// <summary>
        /// Identifies the <see cref="MinimizeButtonVisibility"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty MinimizeButtonVisibilityProperty = DependencyProperty.Register("MinimizeButtonVisibility", typeof(Visibility), typeof(XamDialogWindow), new PropertyMetadata(Visibility.Visible, new PropertyChangedCallback(MinimizeButtonVisibilityPropertyChanged)));

        /// <summary>
        /// Gets/Sets whether the Minimize button is visible on the <see cref="XamDialogWindow"/>
        /// </summary>
        public Visibility MinimizeButtonVisibility
        {
            get { return (Visibility)this.GetValue(MinimizeButtonVisibilityProperty); }
            set { this.SetValue(MinimizeButtonVisibilityProperty, value); }
        }

        private static void MinimizeButtonVisibilityPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamDialogWindow dialog = (XamDialogWindow)obj;
            dialog.EnsureVisualStates();
        }

        #endregion // MinimizeButtonVisibility

        #region CustomCursors

        /// <summary>
        /// Gets/Sets the <see cref="CustomCursors"/> object that contains the cursors that will be used for the <see cref="XamDialogWindow"/>
        /// </summary>
        public CustomCursors CustomCursors
        {
            get
            {
                if (this._customCursors == null)
                {
                    this._customCursors = new CustomCursors();
                }

                return this._customCursors;
            }

            set
            {
                this._customCursors = value;
            }
        }
        #endregion // CustomCursors

        #region IsActive

        /// <summary>
        /// Identifies the <see cref="IsActive"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register("IsActive", typeof(bool), typeof(XamDialogWindow), new PropertyMetadata(new PropertyChangedCallback(IsActivePropertyChanged)));

        /// <summary>
        /// Gets or sets the IsActive property
        /// Property Is Active is true when the Instance of XamDialogWindow
        /// has got the focus and control goes to Active state,
        /// otherwise control is not in active state.        
        /// </summary>
        public bool IsActive
        {
            get { return (bool)this.GetValue(IsActiveProperty); }
            set { this.SetValue(IsActiveProperty, value); }
        }

        private static void IsActivePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamDialogWindow dialog = (XamDialogWindow)obj;

            XamDialogWindow currentActiveWindow = DialogManager.Manager.GetActiveDialogWindow();
            bool newValue = (bool)e.NewValue;
            bool oldValue = (bool)e.OldValue;

            if (newValue)
            {
                // The previously ActiveWindow should no longer be active. 
                if ((currentActiveWindow != dialog) && (currentActiveWindow != null))
                {
                    currentActiveWindow.IsActive = false;
                }

                DialogManager.Manager.SetActiveDialogWindow(dialog);
                dialog.BringToFront();
            }
            else if (currentActiveWindow == dialog)
            {
                DialogManager.Manager.SetActiveDialogWindow(null);
            }

            dialog.OnIsActiveChanged(newValue, oldValue);
            dialog.EnsureVisualStates();
        }

        #endregion // IsActive

        #region IsModal

        /// <summary>
        /// Identifies the <see cref="IsModal"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty IsModalProperty = DependencyProperty.Register("IsModal", typeof(bool), typeof(XamDialogWindow), new PropertyMetadata(false, new PropertyChangedCallback(IsModalChanged)));

        /// <summary>
        /// Gets or sets a value indicating whether this instance is modal.
        /// </summary>
        public bool IsModal
        {
            get { return (bool)this.GetValue(IsModalProperty); }
            set { this.SetValue(IsModalProperty, value); }
        }

        private static void IsModalChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamDialogWindow dialog = (XamDialogWindow)obj;


            if (dialog.IsModal)
            {
                KeyboardNavigation.SetTabNavigation(dialog, KeyboardNavigationMode.Cycle);
            }


            if (!dialog._isDesign && dialog.ContainerContext != null)
            {
                if (dialog.IsModal)
                {
                    if (dialog.RestrictInContainer)
                    {


#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

                    }

                    if (dialog.WindowState != WindowState.Hidden)
                    {
                        dialog.WindowState = WindowState.Normal;
                        DialogManager.Manager.RegisterModal(dialog, true);
                    }
                    else
                        DialogManager.Manager.RegisterModal(dialog, false);

                }
                else
                {
                    DialogManager.Manager.UnregisterModal(dialog);
                    if (dialog.RestrictInContainer)
                    {



                        FrameworkElement root = PlatformProxy.GetRootParent(dialog) as FrameworkElement;

                        GeneralTransform transform = XamDialogWindow.ResolveContainer(dialog).TransformToVisual(root);
                        Point p = transform.Transform(new Point(0, 0));

                        dialog.Left -= p.X;
                        dialog.Top -= p.Y;
                    }
                }

                // Raises an automation event
                XamDialogWindowAutomationPeer peer = FrameworkElementAutomationPeer.FromElement(dialog) as XamDialogWindowAutomationPeer;

                if (peer != null)
                {
                    bool oldValue = (bool)e.OldValue;
                    bool newValue = (bool)e.NewValue;
                    peer.RaiseIsModalPropertyChangedEvent(oldValue, newValue);

                    bool oldCanMaximize;
                    bool newCanMaximize;

                    bool oldCanMinimize;
                    bool newCanMinimize;

                    if ((newValue == false) && (dialog.WindowState != WindowState.Maximized))
                    {
                        newCanMaximize = true;
                    }
                    else
                    {
                        newCanMaximize = false;
                    }

                    if ((oldValue == false) && (dialog.WindowState != WindowState.Maximized))
                    {
                        oldCanMaximize = true;
                    }
                    else
                    {
                        oldCanMaximize = false;
                    }

                    if ((newValue == false) && (dialog.WindowState != WindowState.Minimized))
                    {
                        newCanMinimize = true;
                    }
                    else
                    {
                        newCanMinimize = false;
                    }

                    if ((oldValue == false) && (dialog.WindowState != WindowState.Minimized))
                    {
                        oldCanMinimize = true;
                    }
                    else
                    {
                        oldCanMinimize = false;
                    }

                    peer.RaiseCanMaximizePropertyChangedEvent(oldCanMaximize, newCanMaximize);
                    peer.RaiseCanMinimizePropertyChangedEvent(oldCanMinimize, newCanMinimize);
                }
                
            }
            dialog.EnsureVisualStates();
        }

        #endregion // IsModal

        #region ModalBackground

        /// <summary>
        /// Identifies the <see cref="ModalBackground"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty ModalBackgroundProperty = DependencyProperty.Register("ModalBackground", typeof(Brush), typeof(XamDialogWindow), new PropertyMetadata(new PropertyChangedCallback(ModalBackgroundChanged)));

        /// <summary>
        /// Gets/Sets the <see cref="Brush"/> that is applied to the background of the nonclickable area behdind a Modal Dialog.
        /// </summary>
        public Brush ModalBackground
        {
            get { return (Brush)this.GetValue(ModalBackgroundProperty); }
            set { this.SetValue(ModalBackgroundProperty, value); }
        }

        private static void ModalBackgroundChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamDialogWindow dialog = (XamDialogWindow)obj;
            DialogManager.Manager.UpdateModalLayer(dialog);
        }

        #endregion // ModalBackground

        #region ModalBackgroundEffect

        /// <summary>
        /// Identifies the <see cref="ModalBackgroundEffect"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty ModalBackgroundEffectProperty = DependencyProperty.Register("ModalBackgroundEffect", typeof(Effect), typeof(XamDialogWindow), new PropertyMetadata(new PropertyChangedCallback(ModalBackgroundEffectChanged)));


        /// <summary>
        /// Gets/Sets the <see cref="Effect"/> that is applied the nonclickable area behdind a Modal Dialog.
        /// </summary>
        public Effect ModalBackgroundEffect
        {
            get { return (Effect)this.GetValue(ModalBackgroundEffectProperty); }
            set { this.SetValue(ModalBackgroundEffectProperty, value); }
        }

        private static void ModalBackgroundEffectChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamDialogWindow dialog = (XamDialogWindow)obj;
            DialogManager.Manager.UpdateModalLayerEffect(dialog);
        }

        #endregion // ModalBackgroundEffect

        #region ModalBackgroundOpacity

        /// <summary>
        /// Identifies the <see cref="ModalBackgroundOpacity"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty ModalBackgroundOpacityProperty = DependencyProperty.Register("ModalBackgroundOpacity", typeof(double), typeof(XamDialogWindow), new PropertyMetadata(0.5, new PropertyChangedCallback(ModalBackgroundOpacityChanged)));

        /// <summary>
        /// Gets/Sets the Opacity that is applied the nonclickable area behdind a Modal Dialog.
        /// </summary>
        public double ModalBackgroundOpacity
        {
            get { return (double)this.GetValue(ModalBackgroundOpacityProperty); }
            set { this.SetValue(ModalBackgroundOpacityProperty, value); }
        }


        private static void ModalBackgroundOpacityChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamDialogWindow dialog = (XamDialogWindow)obj;
            DialogManager.Manager.UpdateModalLayer(dialog);
        }

        #endregion // ModalBackgroundOpacity

        #region IsMoveable

        /// <summary>
        /// Identifies the <see cref="IsMoveable"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty IsMoveableProperty = DependencyProperty.Register("IsMoveable", typeof(bool), typeof(XamDialogWindow), new PropertyMetadata(true, new PropertyChangedCallback(IsMoveablePropertyChanged)));

        /// <summary>
        /// Gets or sets a value indicating whether XamDialogWindow instance is moveable.
        /// </summary>
        public bool IsMoveable
        {
            get { return (bool)this.GetValue(IsMoveableProperty); }
            set { this.SetValue(IsMoveableProperty, value); }
        }

        private static void IsMoveablePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamDialogWindow dialog = obj as XamDialogWindow;
            if (dialog != null)
            {
                dialog.OnPropertyChanged("IsMoveable");
            }
        }

        #endregion // IsMoveable

        #region IsResizable

        /// <summary>
        /// Identifies the <see cref="IsResizable"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty IsResizableProperty = DependencyProperty.Register("IsResizable", typeof(bool), typeof(XamDialogWindow), new PropertyMetadata(true, new PropertyChangedCallback(IsResizablePropertyChanged)));

        /// <summary>
        /// Gets or sets the IsResizable property
        /// When the property is true , <see cref="XamDialogWindow"/> instance can be 
        /// resized via drag it resize area (Border) 
        /// </summary>
        public bool IsResizable
        {
            get { return (bool)this.GetValue(IsResizableProperty); }
            set { this.SetValue(IsResizableProperty, value); }
        }

        private static void IsResizablePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamDialogWindow dialog = (XamDialogWindow)obj;
            if (dialog != null)
            {
                bool newValue = (bool)e.NewValue;
                if (!newValue && dialog._resizeArea != null)
                {
                    dialog._resizeArea.Cursor = dialog._previousResizeCursor;
                }

                dialog.OnPropertyChanged("IsResizable");
            }


        }

        #endregion // IsResizable

        #region KeyboardSettings

        /// <summary>
        /// Gets/Sets the settings that pertain to moving the <see cref="XamDialogWindow"/> via the Keyboard.
        /// </summary>
        public KeyboardSettings KeyboardSettings
        {
            get
            {
                if (this._keyboardSettings == null)
                {
                    this._keyboardSettings = new KeyboardSettings();
                }
                return this._keyboardSettings;
            }
            set
            {
                this._keyboardSettings = value;
            }
        }


        #endregion // KeyboardSettings

        #region Left

        /// <summary>
        /// Identifies the <see cref="Left"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty LeftProperty = DependencyProperty.Register("Left", typeof(double), typeof(XamDialogWindow), new PropertyMetadata(0.0, new PropertyChangedCallback(LeftPropertyChanged)));

        /// <summary>
        /// Gets/Sets the X coordinate of the <see cref="XamDialogWindow"/>.
        /// </summary>
        public double Left
        {
            get { return (double)this.GetValue(LeftProperty); }
            set { this.SetValue(LeftProperty, value); }
        }

        private static void LeftPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamDialogWindow dialog = (XamDialogWindow)obj;


            if (!dialog._startupPositionInitialized)
            {
                
                if (dialog._left == 0)
                {
                    dialog._left = (double)e.NewValue;
                    dialog.Left = (double)e.OldValue;
                }

                return;
            }


            double oldValue = (double)e.OldValue;
            double newValue = (double)e.NewValue;

            if (double.IsNaN(newValue))
            {
                dialog.Left = 0;
                return;
            }

            
            if (dialog._isLeftCancelled)
            {
                dialog._isLeftCancelled = false;
                return;
            }

            dialog.Left = dialog.EnsureXCoordinatesRestricted();

            if (double.IsNaN(oldValue))
            {
                oldValue = 0;
            }

            if (dialog.OnMoving())
            {
                dialog._isLeftCancelled = true; 
                // Canceled, reset old value
                dialog.Left = oldValue;
            }
            else
            {
                dialog.MoveDialogByOffset(newValue - oldValue, 0);
                dialog.OnMoved();
            }
        }

        #endregion // Left

        #region Top

        /// <summary>
        /// Identifies the <see cref="Top"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty TopProperty = DependencyProperty.Register("Top", typeof(double), typeof(XamDialogWindow), new PropertyMetadata(0.0, new PropertyChangedCallback(TopPropertyChanged)));

        /// <summary>
        /// Gets/Sets the Y coordinate of the <see cref="XamDialogWindow"/>.
        /// </summary>
        public double Top
        {
            get { return (double)this.GetValue(TopProperty); }
            set { this.SetValue(TopProperty, value); }
        }

        private static void TopPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamDialogWindow dialog = (XamDialogWindow)obj;

            double oldValue = (double)e.OldValue;
            double newValue = (double)e.NewValue;


            if (!dialog._startupPositionInitialized)
            {
                
                if (dialog._top == 0)
                {
                    dialog._top = (double)e.NewValue;
                    dialog.Top = (double)e.OldValue;
                }

                return;
            }


            if (double.IsNaN(newValue))
            {
                dialog.Top = 0;
                return;
            }

            
            if (dialog._isTopCancelled)
            {
                dialog._isTopCancelled = false;
                return;
            }

            dialog.Top = dialog.EnsureYCoordinatesRestricted();

            if (double.IsNaN(oldValue))
            {
                oldValue = 0;
            }

            if (dialog.OnMoving())
            {
                dialog._isTopCancelled = true; 
                dialog.Top = oldValue;
            }
            else
            {
                dialog.MoveDialogByOffset(0, newValue - oldValue);
                dialog.OnMoved();
            }
        }

        #endregion // Top

        #region MinimizedHeaderTemplate

        /// <summary>
        /// Identifies the <see cref="MinimizedHeaderTemplate"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty MinimizedHeaderTemplateProperty = DependencyProperty.Register("MinimizedHeaderTemplate", typeof(DataTemplate), typeof(XamDialogWindow), null);

        /// <summary>
        /// Gets/Sets the <see cref="DataTemplate"/> of the <see cref="XamDialogWindow"/>'s Header when the <see cref="XamDialogWindow.WindowState"/> is set to <see cref="Infragistics.Controls.Interactions.WindowState.Minimized "/>.
        /// </summary>
        public DataTemplate MinimizedHeaderTemplate
        {
            get { return (DataTemplate)this.GetValue(MinimizedHeaderTemplateProperty); }
            set { this.SetValue(MinimizedHeaderTemplateProperty, value); }
        }

        #endregion // MinimizedHeaderTemplate

        #region MinimizedHeight

        /// <summary>
        /// Identifies the <see cref="MinimizedHeight"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty MinimizedHeightProperty = DependencyProperty.Register("MinimizedHeight", typeof(double), typeof(XamDialogWindow), null);

        /// <summary>
        /// Gets/Sets the Height of the <see cref="XamDialogWindow"/> when <see cref="XamDialogWindow.WindowState"/> is set to <see cref="Infragistics.Controls.Interactions.WindowState.Minimized"/>.
        /// </summary>
        public double MinimizedHeight
        {
            get { return (double)this.GetValue(MinimizedHeightProperty); }
            set { this.SetValue(MinimizedHeightProperty, value); }
        }

        #endregion // MinimizedHeight

        #region MinimizedPanel

        /// <summary>
        /// Identifies the <see cref="MinimizedPanel"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty MinimizedPanelProperty = DependencyProperty.Register("MinimizedPanel", typeof(Panel), typeof(XamDialogWindow), new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the MinimizedPanel property 
        /// MinimizedPanel is a Panel, used to arrange <see cref="XamDialogWindow"/> 
        /// instances in minimized state
        /// </summary>
        public Panel MinimizedPanel
        {
            get { return (Panel)this.GetValue(MinimizedPanelProperty); }
            set { this.SetValue(MinimizedPanelProperty, value); }
        }

        #endregion // MinimizedPanel

        #region MinimizedWidth

        /// <summary>
        /// Identifies the <see cref="MinimizedWidth"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty MinimizedWidthProperty = DependencyProperty.Register("MinimizedWidth", typeof(double), typeof(XamDialogWindow), new PropertyMetadata(double.NaN));

        /// <summary>
        /// Gets or sets the MinimizedWidth
        /// Minimized width specifies the width of the <see cref="XamDialogWindow"/>
        /// in minimized state
        /// </summary>
        public double MinimizedWidth
        {
            get { return (double)this.GetValue(MinimizedWidthProperty); }
            set { this.SetValue(MinimizedWidthProperty, value); }
        }


        #endregion // MinimizedWidth

        #region RestrictInContainer


#region Infragistics Source Cleanup (Region)

















#endregion // Infragistics Source Cleanup (Region)

        


        private static readonly DependencyProperty RestrictInContainerProperty = DependencyProperty.Register(
            "RestrictInContainer", typeof(bool), typeof(XamDialogWindow),
            new PropertyMetadata(false, new PropertyChangedCallback(RestrictInContainerChanged), new CoerceValueCallback(CoerceRestrictInContainer))
            );

        private static object CoerceRestrictInContainer(DependencyObject d, object baseValue)
        {
            
            return true;
        }







        internal bool RestrictInContainer
        {
            get { return (bool)this.GetValue(RestrictInContainerProperty); }
            private set { this.SetValue(RestrictInContainerProperty, value); }
        }


        private static void RestrictInContainerChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamDialogWindow dialog = (XamDialogWindow)obj;

            // Don't set the Top and Left until after the xamDialog is loaded. 
            if (!dialog._isLoaded)
                return;

            if (!dialog._isDesign)
            {
                if ((bool)e.NewValue)
                {
                    if (!dialog.IsModal)
                    {
                        Point p = dialog.GetContainerPanelCoords(PlatformProxy.GetRootVisual(dialog) as FrameworkElement);
                        DialogManager.Manager.RegisterRestricted(dialog);
                        if (p.X == 0)
                        {
                            dialog.Left = dialog.EnsureXCoordinatesRestricted();
                        }
                        else
                        {
                            dialog.Left -= p.X;
                        }
                        if (p.Y == 0)
                        {
                            dialog.Top = dialog.EnsureYCoordinatesRestricted();
                        }
                        else
                        {
                            dialog.Top -= p.Y;
                        }
                        dialog.AddHandlersToOwningContainer();
                    }
                    else
                    {
                        dialog.Left = dialog.EnsureXCoordinatesRestricted();
                        dialog.Top = dialog.EnsureYCoordinatesRestricted();
                    }
                }
                else
                {
                    Point p = dialog.GetContainerPanelCoords(PlatformProxy.GetRootVisual(dialog) as FrameworkElement);
                    DialogManager.Manager.Register(dialog);
                    dialog.Left += p.X;
                    dialog.Top += p.Y;
                    dialog.RemoveHandlersFromOwningContainer();
                }

                if (dialog.IsActive)
                    dialog.BringToFront();

                dialog.EnsureMaximizeSize();
            }
        }
        #endregion // RestrictInContainer

        #region StartUpPosition

        /// <summary>
        /// Identifies the <see cref="StartupPosition"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty StartupPositionProperty = DependencyProperty.Register("StartupPosition", typeof(StartupPosition), typeof(XamDialogWindow), new PropertyMetadata(StartupPosition.Manual));

        /// <summary>
        /// Gets or sets the StartUpPosition property that refelect to position of the XamDialogWindow instance in Show method
        /// When it is ManualRelativeToApplication - instance uses values from Left and Top properties, relative to Application,
        /// When it is ManualRelativeToContainer - instance uses values from Left and Top properties, relative to ContainerPanel /Parent,
        /// when it is CenterContainer - instance is centered on the Canvas, 
        /// when it is CenterApplication is centered on the RootVisual, 
        /// when it is Default - instance uses Left=0 and Top = 0 in Show method
        /// otherwise it is collapsed
        /// </summary>
        public StartupPosition StartupPosition
        {
            get { return (StartupPosition)this.GetValue(StartupPositionProperty); }
            set { this.SetValue(StartupPositionProperty, value); }
        }

        #endregion // StartUpPosition

        #region WindowState

        /// <summary>
        /// Identifies the <see cref="WindowState"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty WindowStateProperty = DependencyProperty.Register("WindowState", typeof(WindowState), typeof(XamDialogWindow), new PropertyMetadata(WindowState.Normal, new PropertyChangedCallback(WindowStatePropertyChanged)));

        /// <summary>
        /// Gets/Sets whether the <see cref="XamDialogWindow"/> is Maximized, Minimized, Hidden, or Normal.
        /// </summary>
        public WindowState WindowState
        {
            get { return (WindowState)this.GetValue(WindowStateProperty); }
            set { this.SetValue(WindowStateProperty, value); }
        }

        private static void WindowStatePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamDialogWindow dialog = (XamDialogWindow)obj;
            WindowState newWinState = (WindowState)e.NewValue;
            WindowState oldWinState = (WindowState)e.OldValue;
            bool hasRoot = false;

            dialog._isUpdatingWhenMaximized = false;

            
            if (dialog._isNewWindowStateCancelled)
            {
                dialog._isNewWindowStateCancelled = false;
                return;
            }

            if (dialog._rootElement != null)
            {
                dialog.EnsureWindowState(oldWinState, newWinState);
                hasRoot = true;
            }
            
            else if (oldWinState == Infragistics.Controls.Interactions.WindowState.Hidden && newWinState != oldWinState)
            {
                dialog._startupOldWindowState = oldWinState;
                dialog._startupNewWindowState = newWinState;
            }

            dialog.EnsureVisualStates();

            if (hasRoot)
            {
                if (dialog.WindowState == newWinState)
                {
                    dialog.OnWindowStateChanged(newWinState, oldWinState);
                }
            }
            else if (oldWinState == WindowState.Hidden && newWinState != oldWinState)
            {
                ////MM 2010.04.09
                dialog._oldWindowsState = oldWinState;
                dialog._newWindowState = newWinState;
            }
        }

        #endregion // WindowState

        #region Header

        /// <summary>
        /// Identifies the <see cref="Header"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register("Header", typeof(object), typeof(XamDialogWindow), new PropertyMetadata(new PropertyChangedCallback(HeaderChanged)));

        /// <summary>
        /// Gets/Sets the content that will be displayed in the Header of this <see cref="XamDialogWindow"/>
        /// </summary>
        public object Header
        {
            get { return (object)this.GetValue(HeaderProperty); }
            set { this.SetValue(HeaderProperty, value); }
        }

        private static void HeaderChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {

        }

        #endregion // Header

        #region HeaderTemplate

        /// <summary>
        /// Identifies the <see cref="HeaderTemplate"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty HeaderTemplateProperty = DependencyProperty.Register("HeaderTemplate", typeof(DataTemplate), typeof(XamDialogWindow), new PropertyMetadata(new PropertyChangedCallback(HeaderTemplateChanged)));

        /// <summary>
        /// Gets/Sets the <see cref="DataTemplate"/> that will be applied to the Header of this <see cref="XamDialogWindow"/>
        /// </summary>
        public DataTemplate HeaderTemplate
        {
            get { return (DataTemplate)this.GetValue(HeaderTemplateProperty); }
            set { this.SetValue(HeaderTemplateProperty, value); }
        }

        private static void HeaderTemplateChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {

        }

        #endregion // HeaderTemplate

        #region HeaderBackground

        /// <summary>
        /// Identifies the <see cref="HeaderBackground"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty HeaderBackgroundProperty = DependencyProperty.Register("HeaderBackground", typeof(Brush), typeof(XamDialogWindow), null);

        /// <summary>
        /// Gets/Sets the <see cref="Brush"/> that will be applied to the Header of the <see cref="XamDialogWindow"/>
        /// </summary>
        public Brush HeaderBackground
        {
            get { return (Brush)this.GetValue(HeaderBackgroundProperty); }
            set { this.SetValue(HeaderBackgroundProperty, value); }
        }

        #endregion // HeaderBackground

        #region HeaderForeground

        /// <summary>
        /// Identifies the <see cref="HeaderForeground"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty HeaderForegroundProperty = DependencyProperty.Register("HeaderForeground", typeof(Brush), typeof(XamDialogWindow), null);

        /// <summary>
        /// Gets/Sets the <see cref="Brush"/> that will be applied to the Foreground of the Header of the <see cref="XamDialogWindow"/>
        /// </summary>
        public Brush HeaderForeground
        {
            get { return (Brush)this.GetValue(HeaderForegroundProperty); }
            set { this.SetValue(HeaderForegroundProperty, value); }
        }

        #endregion // HeaderForeground

        #region HeaderHorizontalContentAlignment

        /// <summary>
        /// Identifies the <see cref="HeaderHorizontalContentAlignment"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty HeaderHorizontalContentAlignmentProperty = DependencyProperty.Register("HeaderHorizontalContentAlignment", typeof(HorizontalAlignment), typeof(XamDialogWindow), null);

        /// <summary>
        /// Gets/Sets the <see cref="HorizontalAlignment"/> that will be applied to the content of the Header of the <see cref="XamDialogWindow"/>
        /// </summary>
        public HorizontalAlignment HeaderHorizontalContentAlignment
        {
            get { return (HorizontalAlignment)this.GetValue(HeaderHorizontalContentAlignmentProperty); }
            set { this.SetValue(HeaderHorizontalContentAlignmentProperty, value); }
        }

        #endregion // HeaderHorizontalContentAlignment

        #region HeaderVerticalContentAlignment

        /// <summary>
        /// Identifies the <see cref="HeaderVerticalContentAlignment"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty HeaderVerticalContentAlignmentProperty = DependencyProperty.Register("HeaderVerticalContentAlignment", typeof(VerticalAlignment), typeof(XamDialogWindow), null);

        /// <summary>
        /// Gets/Sets the <see cref="VerticalAlignment"/> that will be applied to the content of the Header of the <see cref="XamDialogWindow"/>
        /// </summary>
        public VerticalAlignment HeaderVerticalContentAlignment
        {
            get { return (VerticalAlignment)this.GetValue(HeaderVerticalContentAlignmentProperty); }
            set { this.SetValue(HeaderVerticalContentAlignmentProperty, value); }
        }

        #endregion // HeaderVerticalContentAlignment

        #region HeaderIconTemplate

        /// <summary>
        /// Identifies the <see cref="HeaderIconTemplate"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty HeaderIconTemplateProperty = DependencyProperty.Register("HeaderIconTemplate", typeof(DataTemplate), typeof(XamDialogWindow), null);

        /// <summary>
        /// Gets or sets the HeaderIconTemplate property
        /// HeaderIconTemplate is used like a DataTemplate for the image, placed in 
        /// XamDialogWindow title bar (drag area)
        /// </summary>
        public DataTemplate HeaderIconTemplate
        {
            get { return (DataTemplate)this.GetValue(HeaderIconTemplateProperty); }
            set { this.SetValue(HeaderIconTemplateProperty, value); }
        }

        #endregion // HeaderIconTemplate

        #region HeaderIconVisibility

        /// <summary>
        /// Identifies the <see cref="HeaderIconVisibility"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty HeaderIconVisibilityProperty = DependencyProperty.Register("HeaderIconVisibility", typeof(Visibility), typeof(XamDialogWindow), new PropertyMetadata(Visibility.Visible, new PropertyChangedCallback(HeaderIconVisibilityChanged)));

        /// <summary>
        /// Gets or sets the HeaderIconVisibility property
        /// When property is Visible , Image is visible, otherwise Image is collapsed
        /// </summary>
        public Visibility HeaderIconVisibility
        {
            get { return (Visibility)this.GetValue(HeaderIconVisibilityProperty); }
            set { this.SetValue(HeaderIconVisibilityProperty, value); }
        }

        private static void HeaderIconVisibilityChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamDialogWindow dialog = (XamDialogWindow)obj;
            dialog.EnsureVisualStates();
        }

        #endregion // HeaderIconVisibility

        #endregion // Public

        #region Internal

        internal TranslateTransform MoveTransform
        {
            get { return this._moveTransform; }
        }

        internal Grid ContainerContext
        {
            get { return this._containerContext; }
        }


        
        internal System.Windows.Shapes.Rectangle ModalLayer
        {
            get
            {
                if (this._modalLayer == null)
                    this._modalLayer = new System.Windows.Shapes.Rectangle();

                return this._modalLayer;
            }
        }

        internal Canvas ContextPanel
        {
            get
            {
                if (this._contextPanel == null)
                    this._contextPanel = new Canvas();

                return this._contextPanel;
            }
        }

		// AS 6/22/12 TFS115111
		internal FrameworkElement ModalLayerContainer
		{
			get { return _modalLayerContainer; }
			set
			{
				if (value != _modalLayerContainer)
				{
					var old = _modalLayerContainer;
					_modalLayerContainer = value;

					if (old != null)
						Infragistics.Windows.Internal.ModalWindowHelper.SetModalDialogWindowCount(old, Infragistics.Windows.Internal.ModalWindowHelper.GetModalDialogWindowCount(old) - 1);

					if (value != null)
						Infragistics.Windows.Internal.ModalWindowHelper.SetModalDialogWindowCount(value, Infragistics.Windows.Internal.ModalWindowHelper.GetModalDialogWindowCount(value) + 1);
				}
			}
		}



        #region ContainerElement

        internal FrameworkElement ContainerElement { get; private set; }

        #endregion // ContainerElement

        #endregion // Internal

        #endregion Properties

        #region Methods

        #region Public

        #region BringToFront

        /// <summary>
        /// Brings the control to the front of the z-order.
        /// </summary>
        public void BringToFront()
        {
            DialogManager.Manager.BringToFront(this);
        }

        #endregion // BringToFront


        #region CenterDialogWindow






        /// <summary>
        /// Moves the <see cref="XamDialogWindow"/> to the center of either the container.
        /// </summary>

        public void CenterDialogWindow()
        {
            if (!this._isDesign)
            {
                FrameworkElement container = null;
                if (this.RestrictInContainer)
                {
                    container = XamDialogWindow.ResolveContainer(this);
                }

                if (container == null)
                    container = PlatformProxy.GetRootVisual(this) as FrameworkElement;

                if (container != null)
                {
                    Point containerOffset = this.GetContainerPanelCoords(container);

                    double width = this.ActualWidth;
                    double height = this.ActualHeight;

                    if ((this._rootElement != null) && (this._rootElement.ActualWidth != this.MinWidth) & (this._rootElement.ActualHeight != this.MinHeight))
                    {

                        // oh wpf....
                        // even though we set the Visibility directly, it ignores it...for some reason it'll apply it later
                        // so just listen for the event to fire and call CenterDialogWindow then.
                        if (this._rootElement.Visibility == System.Windows.Visibility.Collapsed)
                        {
                            this._rootElement.IsVisibleChanged -= RootElement_IsVisibleChanged;
                            this._rootElement.IsVisibleChanged += new DependencyPropertyChangedEventHandler(RootElement_IsVisibleChanged);
                        }


                        width = this._rootElement.ActualWidth;
                        height = this._rootElement.ActualHeight;
                    }

                    if (width == 0 || height == 0)
                    {
                        height = this._rootElement.DesiredSize.Height;
                        width = this._rootElement.DesiredSize.Width;
                    }

                    double parentWidth = container.ActualWidth;
                    double parentHeight = container.ActualHeight;

                    if (width == 0 && !double.IsNaN(this.Width))
                        width = this.Width;

                    if (height == 0 && !double.IsNaN(this.Height))
                        height = this.Height;

                    if (height == 0 && width == 0)
                    {
                        if (this.DesiredSize.Height != 0 && !double.IsNaN(this.DesiredSize.Height))
                            height = this.DesiredSize.Height;
                        if (this.DesiredSize.Width != 0 && !double.IsNaN(this.DesiredSize.Width))
                            width = this.DesiredSize.Width;
                    }

                    if ((_contentChanged || (height == 0 && width == 0)) && (this._rootElement != null))
                    {
                        _contentChanged = false;
                        this._rootElement.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                        height = this._rootElement.DesiredSize.Height;
                        width = this._rootElement.DesiredSize.Width;
                    }

                    this.Left = Math.Max(0, Math.Round((parentWidth - width) / 2.0 + containerOffset.X));
                    this.Top = Math.Max(0, Math.Round((parentHeight - height) / 2.0 + containerOffset.Y));
                }
            }
        }

        #endregion // CenterDialogWindow

        #region Close
        /// <summary>
        /// Sets the <see cref="XamDialogWindow.WindowState"/> property to Hidden, causing the dialog window to no longer be visible.
        /// </summary>
        public void Close()
        {
            this.WindowState = WindowState.Hidden;
        }
        #endregion // Close

        #region Show
        /// <summary>
        /// Sets the <see cref="XamDialogWindow.WindowState"/> property to Normal, causing the dialog window to appear.
        /// </summary>
        public void Show()
        {
            this.WindowState = WindowState.Normal;
            this.Focus();
        }
        #endregion // Show

        #region Maximize
        /// <summary>
        /// Sets the <see cref="XamDialogWindow.WindowState"/> property to Maximize, causing the dialog window to expand to it's maximum state.
        /// </summary>
        public void Maximize()
        {
            this.WindowState = WindowState.Maximized;
        }
        #endregion // Maximize

        #region Minimize
        /// <summary>
        /// Sets the <see cref="XamDialogWindow.WindowState"/> property to Minimized, causing the dialog window to collapse.
        /// </summary>
        public void Minimize()
        {
            this.WindowState = WindowState.Minimized;
        }
        #endregion // Minimize

        #endregion // Public

        #region Protected

        #region ShowCustomCursor
        /// <summary>
        /// Shows the specified <see cref="DataTemplate"/> as a custom cursor. 
        /// </summary>
        /// <param name="cursorTemplate"></param>
        /// <returns>False if the custom cursor was not applied. </returns>
        protected bool ShowCustomCursor(DataTemplate cursorTemplate)
        {
            if (cursorTemplate != null)
            {
                FrameworkElement customCursor = cursorTemplate.LoadContent() as FrameworkElement;
                if (customCursor != null)
                {
                    customCursor.IsHitTestVisible = false;
                    this._resizeArea.Cursor = Cursors.None;

                    if (this._customCursorPopup == null)
                    {
                        this._customCursorPopup = new Popup();



                        this._customCursorPopup.PlacementTarget = this;
                        this._customCursorPopup.Placement = PlacementMode.Relative;
                        this._customCursorPopup.AllowsTransparency = true;



                        
                    }

                    if (this._customCursorPopup.Child == null)
                    {
                        this._customCursorPopup.Child = customCursor;
                    }
                    else if (!this._customCursorPopup.Child.Equals(customCursor))
                    {
                        this._customCursorPopup.Child = customCursor;
                    }

                    this._customCursorPopup.IsOpen = true;

                    return true;
                }
            }

            return false;
        }
        #endregion // ShowCustomCursor

        #region HideCustomCursor

        /// <summary>
        /// Hides the Custom cursor. 
        /// </summary>
        protected void HideCustomCursor()
        {
            if (this._customCursorPopup != null && this._customCursorPopup.Child != null)
            {
                this._customCursorPopup.IsOpen = false;
            }
        }
        #endregion // HideCustomCursor

        #region MoveDialogByOffset

        /// <summary>
        /// Updates the location of the <see cref="XamDialogWindow"/>
        /// </summary>
        /// <param name="offsetX">The x offset to move the dialog by.</param>
        /// <param name="offsetY">The y offset to move the dialog by.</param>
        protected virtual void MoveDialogByOffset(double offsetX, double offsetY)
        {
            this._moveTransform.X += offsetX;
            this._moveTransform.Y += offsetY;
        }
        #endregion // MoveDialogByOffset

        #region EnsureVisualStates
        /// <summary>
        /// Ensures that the <see cref="XamDialogWindow"/> has the proper VisualStates applied.
        /// </summary>
        protected virtual void EnsureVisualStates()
        {

            if (this.IsActive)
            {
                VisualStateManager.GoToState(this, "Active", true);
            }
            else
            {
                VisualStateManager.GoToState(this, "Inactive", true);
            }

            if (this.CloseButtonVisibility == Visibility.Visible)
            {
                VisualStateManager.GoToState(this, "CloseButtonIsVisible", true);
            }
            else
            {
                VisualStateManager.GoToState(this, "CloseButtonIsNotVisible", true);
            }


            if (this.HeaderIconVisibility == Visibility.Visible)
            {
                VisualStateManager.GoToState(this, "HeaderIconIsVisible", true);
            }
            else
            {
                VisualStateManager.GoToState(this, "HeaderIconIsNotVisible", true);
            }

            if (this.MaximizeButtonVisibility == Visibility.Visible && !this.IsModal)
            {
                VisualStateManager.GoToState(this, "MaximizeButtonIsVisible", true);
            }
            else
            {
                VisualStateManager.GoToState(this, "MaximizeButtonIsNotVisible", true);
            }

            if (this.MinimizeButtonVisibility == Visibility.Visible && !this.IsModal)
            {
                VisualStateManager.GoToState(this, "MinimizeButtonIsVisible", true);
            }
            else
            {
                VisualStateManager.GoToState(this, "MinimizeButtonIsNotVisible", true);
            }

            if (this.IsModal)
            {
                VisualStateManager.GoToState(this, "Modal", true);
            }
            else
            {
                VisualStateManager.GoToState(this, "NonModal", true);
            }

            if (this.RootElement != null)
            {
                if (this.WindowState == WindowState.Hidden)
                {
                    VisualStateManager.GoToState(this, "Hidden", true);
                }
                else
                {
                    VisualStateManager.GoToState(this, "Visible", true);
                }
            }

            if (this.IsEnabled)
            {
                if (this._rootElement != null)
                {
                    switch (this.WindowState)
                    {
                        case WindowState.Normal:
                            this.Visibility = this._rootElement.Visibility = Visibility.Visible;
                            VisualStateManager.GoToState(this, "Normal", true);
                            break;
                        case WindowState.Maximized:
                            this.Visibility = this._rootElement.Visibility = Visibility.Visible;
                            VisualStateManager.GoToState(this, "Maximized", true);
                            break;
                        case WindowState.Minimized:
                            this.Visibility = this._rootElement.Visibility = Visibility.Visible;
                            VisualStateManager.GoToState(this, "Minimized", true);
                            break;
                        case WindowState.Hidden:
                            this.Visibility = this._rootElement.Visibility = Visibility.Collapsed;
                            break;
                    }
                }
                else
                    this.Visibility = (this.WindowState == WindowState.Hidden) ? Visibility.Collapsed : Visibility.Visible;
            }
            else
                VisualStateManager.GoToState(this, "Disabled", true);

        }
        #endregion // EnsureVisualStates

        #region  GetParameter
        /// <summary>
        /// Returns the object that defines the parameters necessary to execute the command.
        /// </summary>
        /// <param name="source">The CommandSource object which defines the command to be executed.</param>
        /// <returns>The object necessary for the command to complete.</returns>
        protected virtual object GetParameter(CommandSource source)
        {
            if (source.Command is XamDialogWindowCommandBase)
                return this;

            return null;
        }
        #endregion // GetParameter

        #region SupportsCommand

        /// <summary>
        /// Returns if the object will support a given command type.
        /// </summary>
        /// <param name="command">The command to be validated.</param>
        /// <returns>True if the object recognizes the command as actionable against it.</returns>
        protected virtual bool SupportsCommand(ICommand command)
        {
            return command is XamDialogWindowCommandBase;
        }
        #endregion // SupportsCommand

        #region RootElement

        /// <summary>
        /// Gets the <see cref="DialogRootPanel"/> that is in the Template of the <see cref="XamDialogWindow"/>
        /// </summary>
        protected internal DialogRootPanel RootElement
        {
            get { return this._rootElement; }
        }
        #endregion // RootElement


        #endregion // Protected

        #region Internal

        
        #region SetEffect







        internal void SetEffect(System.Windows.Media.Effects.Effect effect)
        {
            this._originalEffect = this.Effect;
            if (!this._isModalBackgroundEffect)
            {
                this.Effect = effect;
                this._isModalBackgroundEffect = true;
            }
        }

        #endregion //SetEffect

        #region RestoreEffect






        internal void RestoreEffect()
        {
            this.Effect = this._originalEffect;
            this._isModalBackgroundEffect = false;
        }

        #endregion //RestoreEffect



        #region SetHeight

        /// <summary>
        /// We should always use these method to set the height to ensure it's set properly
        /// </summary>
        /// <param name="height"></param>
        internal void SetHeight(double height)
        {
            if (this._rootElement != null)
            {
                this.Height = height;
                this._rootElement.Height = height;
            }
        }
        #endregion // SetHeight

        #region SetWidth

        /// <summary>
        /// We should always use these method to set the width to ensure it's set properly
        /// </summary>
        /// <param name="width"></param>
        internal void SetWidth(double width)
        {
            if (this._rootElement != null)
            {
                this.Width = width;
                this._rootElement.Width = width;
            }
        }

        #endregion // SetWidth

        #endregion //Internal

        #region Private

        #region ResolveResizeDirection
        /// <summary>
        /// Calculates a direction of resizing: it can be a Point with X=+1, X=-1. Y=+1 or Y = -1
        /// </summary>
        /// <param name="pos">position of teh point</param>
        /// <returns>Point with direction</returns>
        private Point ResolveResizeDirection(Point pos)
        {
            Point point = new Point();

            if (pos.X < this._resizeArea.BorderThickness.Left)
            {
                point.X = -1.0;
            }
            else if (pos.X >= (this._resizeArea.ActualWidth - this._resizeArea.BorderThickness.Right))
            {
                point.X = 1.0;
            }

            if (pos.Y < this._resizeArea.BorderThickness.Top)
            {
                point.Y = -1.0;
                return point;
            }

            if (pos.Y >= (this._resizeArea.ActualHeight - this._resizeArea.BorderThickness.Bottom))
            {
                point.Y = 1.0;
            }

            return point;
        }
        #endregion // ResolveResizeDirection

        #region EnsureXCoordinatesRestricted
        private double EnsureXCoordinatesRestricted()
        {
            if (this.RestrictInContainer)
            {
                FrameworkElement container = XamDialogWindow.ResolveContainer(this);
                if (container != null)
                {
                    Point containerCoords = this.GetContainerPanelCoords(container);

                    if (this.IsModal)
                    {
                        containerCoords.X *= -1;
                        containerCoords.Y *= -1;
                    }

                    if (this.Left + containerCoords.X < 0)
                    {
                        return -containerCoords.X;
                    }

                    double actualWidth = this.ActualWidth;
                    if (this._rootElement != null)
                        actualWidth = this._rootElement.ActualWidth;

                    if (this.Left + actualWidth + containerCoords.X > container.ActualWidth)
                    {
                        //Fixed Bug 29458 - Mihail Mateev - 2010.03.14
                        double left = this.Left;
                        if (!this._isResizing)
                        {
                            left = this.Left - (this.Left + actualWidth - container.ActualWidth) -
                                   containerCoords.X;
                        }
                        if (left + containerCoords.X < 0)
                        {
                            left = -containerCoords.X;
                            if (left + actualWidth + containerCoords.X > container.ActualWidth)
                            {
                                this.SetWidth((container.ActualWidth) - left - containerCoords.X);
                                this.UpdateLayout();
                            }
                        }
                        return left;
                    }
                }
            }

            return this.Left;

        }
        #endregion // EnsureXCoordinatesRestricted

        #region EnsureYCoordinatesRestricted
        private double EnsureYCoordinatesRestricted()
        {
            if (this.RestrictInContainer)
            {
                FrameworkElement container = XamDialogWindow.ResolveContainer(this);
                if (container != null)
                {
                    Point containerCoords = this.GetContainerPanelCoords(container);

                    if (this.IsModal)
                    {
                        containerCoords.X *= -1;
                        containerCoords.Y *= -1;
                    }

                    if (this.Top + containerCoords.Y < 0)
                    {
                        return -containerCoords.Y;
                    }

                    double actualHeight = this.ActualHeight;
                    if (this._rootElement != null)
                        actualHeight = this._rootElement.ActualHeight;

                    if (this.Top + actualHeight + containerCoords.Y > container.ActualHeight)
                    {
                        //Fixed Bug 33850 - Mihail Mateev - 2010.06.08
                        double top = this.Top;
                        if (!this._isResizing)
                        {
                            top = this.Top - (this.Top + actualHeight - container.ActualHeight) - containerCoords.Y;
                        }
                        if (top + containerCoords.Y < 0)
                        {
                            top = -containerCoords.Y;
                            if (top + actualHeight + containerCoords.Y > container.ActualHeight)
                            {
                                this.SetHeight((container.ActualHeight) - top - containerCoords.Y);
                                this.UpdateLayout();
                            }
                        }

                        return top;
                    }
                }
            }

            return this.Top;

        }
        #endregion // EnsureYCoordinatesRestricted

        #region DetachFromLayout
        private void DetachFromLayout()
        {
            if (this.IsEnabled && !this._isDetached && this.WindowState != Infragistics.Controls.Interactions.WindowState.Minimized && this.WindowState != Interactions.WindowState.Hidden)
            {
                if (this._containerContext != null && (this._containerContext.ActualHeight > 0 || this._containerContext.ActualWidth > 0))
                {
                    if (this.RestrictInContainer)
                    {
                        DialogManager.Manager.RegisterRestricted(this);

                        if (this.HorizontalAlignment != System.Windows.HorizontalAlignment.Left || this.VerticalAlignment != System.Windows.VerticalAlignment.Top)
                        {
                            

                            FrameworkElement elem = XamDialogWindow.ResolveContainer(this);
                            Point origin = this.GetContainerPanelCoords(elem);
                            this.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                            this.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                            this.SetPosition(origin.X, origin.Y);
                        }
                        else

                        {
                            this.Left = this.EnsureXCoordinatesRestricted();
                            this.Top = this.EnsureYCoordinatesRestricted();
                        }
                    }
                    else if (!this._isDesign)
                    {   
                        DialogManager.Manager.Register(this);
                    }

                    if (this.IsModal && !this._isDesign)
                    {   
                        DialogManager.Manager.RegisterModal(this, true);
                    }

                    if (this.StartupPosition == StartupPosition.Center)
                        this.CenterDialogWindow();

                    this._isDetached = true;
                }
            }
        }

        #endregion // DetachFromLayoutDetachFromLayout

        #region AddHandlersToOwningContainer
        private void AddHandlersToOwningContainer()
        {
            if (this.RestrictInContainer)
            {
                if (this.ContainerElement != null)
                {
                    this.ContainerElement.SizeChanged -= RestrictInContainer_SizeChanged;
                }

                this.ContainerElement = XamDialogWindow.ResolveContainer(this);

                if (this.ContainerElement != null)
                {
                    this.ContainerElement.SizeChanged -= RestrictInContainer_SizeChanged;
                    this.ContainerElement.SizeChanged += new SizeChangedEventHandler(RestrictInContainer_SizeChanged);
                }
            }
        }
        #endregion // AddHandlersToOwningContainer

        #region RemoveHandlersFromOwningContainer
        private void RemoveHandlersFromOwningContainer()
        {
            if (this.RestrictInContainer)
            {
                if (this.ContainerElement != null)
                {
                    this.ContainerElement.SizeChanged -= RestrictInContainer_SizeChanged;
                    this.ContainerElement = null;
                }
            }
        }
        #endregion // RemoveHandlersFromOwningContainer

        #region EnsureWindowState
        private void EnsureWindowState(WindowState oldWinState, WindowState newWinState)
        {
            if (oldWinState != newWinState && this._rootElement != null)
            {

                // If the Dialog Is Modal and its previous state wasn't hidden
                // And the newState is Maximized or Minimized, then the Dialog should be put into its normal state. 
                if (this.IsModal && oldWinState != WindowState.Hidden && (newWinState == WindowState.Minimized || newWinState == WindowState.Maximized))
                {
                    this.WindowState = WindowState.Normal;
                    return;
                }

                if (this.OnWindowStateChanging(oldWinState, newWinState))
                {
                    this._isNewWindowStateCancelled = true; 
                    // The Changing event was cancelled, so we need to reset the value. 
                    this.WindowState = oldWinState;
                }
                else // The event wasn't cancelled. 
                {
                    //Check if visibility is Collapsed but state is not Hidden
                    if (newWinState != Infragistics.Controls.Interactions.WindowState.Hidden && this.Visibility == System.Windows.Visibility.Collapsed)
                    {
                        this.Visibility = System.Windows.Visibility.Visible;
                    }

                    if (this.IsModal)
                    {
                        if (newWinState == WindowState.Hidden)
                            DialogManager.Manager.UnregisterModal(this);
                        else if (oldWinState == WindowState.Hidden)
                            DialogManager.Manager.RegisterModal(this, true);
                    }

                    if (this._maximizedParentElement != null)
                        this._maximizedParentElement.SizeChanged -= this.MaximizedParentElement_SizeChanged;

                    switch (newWinState)
                    {
                        case WindowState.Normal:
                            this.OnRestore(oldWinState);
                            break;
                        case WindowState.Maximized:
                            this.OnMaximize(oldWinState);
                            break;
                        case WindowState.Minimized:
                            this.OnMinimize(oldWinState);
                            break;
                        case WindowState.Hidden:
                            if (oldWinState == WindowState.Normal)
                                this._restore = new Rect(this.Left, this.Top, this.Width, this.Height);
                            break;
                    }

                    this.EnsureVisualStates();
                    //this.OnWindowStateChanged(newWinState, oldWinState);
                }
            }
            
            else if (this._startupNewWindowState == WindowState.Normal && this._startupNewWindowState != this._startupOldWindowState)
            {
                
                if (!this.OnWindowStateChanging(this._startupOldWindowState, this._startupNewWindowState))
                {
                    this.OnWindowStateChanged(this._startupNewWindowState, this._startupOldWindowState);
                    this._startupOldWindowState = this._startupNewWindowState;
                }

            }
        }
        #endregion // EnsureWindowState

        #region OnMaximize

        private void OnMaximize(WindowState oldState)
        {
            if (oldState == WindowState.Minimized)
            {
                this.RestoreFromMinimizedState();
            }
            else
            {
                if (this._rootElement != null)
                    this._restore = new Rect(this.Left, this.Top, this._rootElement.ActualWidth, this._rootElement.ActualHeight);
            }

            this.EnsureMaximizeSize();

            if (this._resizeArea != null)
                this._resizeArea.Cursor = this._previousResizeCursor;
        }

        #endregion // OnMaximize

        #region EnsureMaximizeSize
        private void EnsureMaximizeSize()
        {
            if (this.WindowState == WindowState.Maximized)
            {
                if (this._maximizedParentElement != null)
                    this._maximizedParentElement.SizeChanged -= this.MaximizedParentElement_SizeChanged;

                FrameworkElement elem = XamDialogWindow.ResolveContainer(this);
                if (elem != null && this.RestrictInContainer)
                {
                    this._maximizedParentElement = elem;
                    this._maximizedParentElement.SizeChanged += new SizeChangedEventHandler(MaximizedParentElement_SizeChanged);
                    Point origin = this.GetContainerPanelCoords(elem);

                    if (this._isUpdatingWhenMaximized)
                    {
                        
                        this._isUpdatingWhenMaximized = false;
                        return;
                    }

                    this._isUpdatingWhenMaximized = true;

                    this.Left = -origin.X;
                    this.Top = -origin.Y;
                    this.SetWidth(elem.ActualWidth);
                    this.SetHeight(elem.ActualHeight);
                    this.UpdateLayout();
                    this._isUpdatingWhenMaximized = Math.Abs(this.Width - elem.ActualWidth) > 1 || Math.Abs(this.Height - elem.ActualHeight) > 1;
                }
                else
                {
                    elem = PlatformProxy.GetRootVisual(this) as FrameworkElement;
                    if (elem != null)
                    {
                        this._maximizedParentElement = elem;
                        this._maximizedParentElement.SizeChanged += new SizeChangedEventHandler(MaximizedParentElement_SizeChanged);
                        this.Left = 0;
                        this.Top = 0;
                        this.SetWidth(elem.ActualWidth);
                        this.SetHeight(elem.ActualHeight);
                        this.UpdateLayout();
                    }
                }
            }
        }
        #endregion // EnsureMaximizeSize

        #region OnMinimize
        private void OnMinimize(WindowState oldState)
        {
            if (oldState != WindowState.Maximized)
            {
                if (this._rootElement != null)
                    this._restore = new Rect(this.Left, this.Top, this._rootElement.ActualWidth, this._rootElement.ActualHeight);
            }

            this.Left = 0;
            this.Top = 0;
            this.SetWidth(this.MinimizedWidth);
            this.SetHeight(this.MinimizedHeight);

            if (this.MinimizedPanel != null && !this.MinimizedPanel.Children.Contains(this))
            {
                if (this._containerContext != null)
                {
                    if (!this.RestrictInContainer)
                        DialogManager.Manager.Unregister(this);

                    if (this._containerContext.Children.Contains(this._rootElement))
                        this._containerContext.Children.Remove(this._rootElement);

                    this.MinimizedPanel.Children.Add(this._rootElement);
                }
            }

            if (this._resizeArea != null)
                this._resizeArea.Cursor = this._previousResizeCursor;
        }
        #endregion // OnMinimize

        #region OnRestore
        private void OnRestore(WindowState oldState)
        {
            if (this._restore.Width > 0 && this._restore.Height > 0)
            {
                this.SetWidth(this._restore.Width);
                this.SetHeight(this._restore.Height);
                this.UpdateLayout();
            }

            if (oldState == WindowState.Minimized)
            {
                this.RestoreFromMinimizedState();
                this.Left = this._restore.Left;
                this.Top = this._restore.Top;
            }
            else if (oldState == WindowState.Hidden && this.StartupPosition == StartupPosition.Center)
            {
                this.CenterDialogWindow();
            }
            else
            {
                this.Left = this._restore.Left;
                this.Top = this._restore.Top;
            }


#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

        }
        #endregion // OnRestore

        #region RestoreFromMinimizedState
        private void RestoreFromMinimizedState()
        {
            if (this.MinimizedPanel != null && this._containerContext != null)
            {
                if (this.MinimizedPanel.Children.Contains(this._rootElement))
                {
                    this.MinimizedPanel.Children.Remove(this._rootElement);
                }

                if (this.RestrictInContainer)
                {
                    if (!this._containerContext.Children.Contains(this._rootElement))
                        this._containerContext.Children.Add(this._rootElement);
                }
                else
                    DialogManager.Manager.Register(this);
            }
        }
        #endregion // RestoreFromMinimizedState

        #region GetContainerPanelCoords
        private Point GetContainerPanelCoords(FrameworkElement container)
        {
            Point retPoint = new Point(0, 0);
            if (container != null && this.RestrictInContainer)
            {

                

                if (this.ActualHeight > 0 && this.ActualWidth > 0 && this.Visibility != Visibility.Collapsed)
				{
					GeneralTransform transform = this.TransformToVisual(container);
					retPoint = transform.Transform(retPoint);
				}


#region Infragistics Source Cleanup (Region)











#endregion // Infragistics Source Cleanup (Region)

            }
            return retPoint;
        }
        #endregion // GetContainerPanelCoords



        private void Container_LayoutUpdated(object sender, EventArgs e)
        {
            FrameworkElement container = XamDialogWindow.ResolveContainer(this);
            container.LayoutUpdated -= Container_LayoutUpdated;

            if (!this._startupPositionInitialized)
            {
                this._startupPositionInitialized = true;
                this.SetStartupPosition();
            }
        }

        private void SetStartupPosition()
        {
            if (this.StartupPosition == Interactions.StartupPosition.Center)
            {
                this.CenterWindow();
            }
            else
            {
                this.SetPosition(this._left, this._top);
            }
        }

        private void SetPosition(double left, double top)
        {
            this._moveTransform.X = 0;
            this._moveTransform.Y = 0;

            this.Left = left;
            this.Top = top;
        }

        private void CenterWindow()
        {
            this._moveTransform.X = 0;
            this._moveTransform.Y = 0;
            this.CenterDialogWindow();
        }

        private void UpdateZOrder()
        {
            
            if (!this._orderUpdated)
            {
                this._orderUpdated = true;
                if (Canvas.GetZIndex(this) == 0)
                {
                    this.BringToFront();
                }
            }
        }


        #region EndDrag Methods

        
        private void EndResize()
        {
            if (this._isResizing)
            {
                this._resizeArea.LostMouseCapture -= this.ResizeArea_LostMouseCapture;
                this._resizeArea.ReleaseMouseCapture();
                this._isResizing = false;
                this._resizeDir = new Point();
                this.HideCustomCursor();
            }
        }

        
        private void EndMove()
        {
            if (this._isMoving)
            {
                this._headerArea.LostMouseCapture -= this.HeaderArea_LostMouseCapture;
                this._headerArea.ReleaseMouseCapture();
                this._isMoving = false;
            }
        }

        #endregion //EndDrag Methods

        #region DetermineMousePosition

        private Point DetermineMousePosition(MouseEventArgs e)
        {
            Point mousePosition;


#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

            mousePosition = e.GetPosition(null);
            mousePosition.X -= this.Left;
            mousePosition.Y -= this.Top;



            return mousePosition;
        }

        #endregion // DetermineMousePosition

        #region ShowCursor

        private void ShowCursor(Point mousePosition)
        {
            if (!this.IsResizable)
                return;

            // We aren't resizing, the mouse is just over the resize area. 
            // So lets determine the cursor that should be displayed. 
            Point resizeType = this.ResolveResizeDirection(mousePosition);

            this._resizeArea.Cursor = this._previousResizeCursor;
            this._previousResizeCursor = this._resizeArea.Cursor;

            if ((resizeType.X != 0.0) && (resizeType.Y != 0.0))
            {
                if (resizeType.X < 0 && resizeType.Y < 0)
                {
                    if (!this.ShowCustomCursor(this.CustomCursors.DiagonalResizeCursor))
                    {


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

                        
                        this._resizeArea.Cursor = Cursors.SizeNWSE;
                        this.HideCustomCursor();

                    }
                }
                else if (resizeType.X > 0 && resizeType.Y > 0)
                {
                    if (!this.ShowCustomCursor(this.CustomCursors.DiagonalResizeCursor))
                    {


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

                        
                        this._resizeArea.Cursor = Cursors.SizeNWSE;
                        this.HideCustomCursor();

                    }
                }
                else if (resizeType.X > 0 && resizeType.Y < 0)
                {
                    if (!this.ShowCustomCursor(this.CustomCursors.RightDiagonalResizeCursor))
                    {


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

                        
                        this._resizeArea.Cursor = Cursors.SizeNESW;
                        this.HideCustomCursor();

                    }
                }
                else if (resizeType.X < 0 && resizeType.Y > 0)
                {
                    if (!this.ShowCustomCursor(this.CustomCursors.RightDiagonalResizeCursor))
                    {


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

                        
                        this._resizeArea.Cursor = Cursors.SizeNESW;
                        this.HideCustomCursor();

                    }
                }
            }
            else if (resizeType.X != 0.0)
            {
                if (!this.ShowCustomCursor(this.CustomCursors.HorizontalResizeCursor))
                {




                    
                    this._resizeArea.Cursor = Cursors.SizeWE;
                    this.HideCustomCursor();

                }
            }
            else if (resizeType.Y != 0.0)
            {
                if (!this.ShowCustomCursor(this.CustomCursors.VerticalResizeCursor))
                {




                    
                    this._resizeArea.Cursor = Cursors.SizeNS;
                    this.HideCustomCursor();

                }
            }
            else
            {
                this.HideCustomCursor();
            }
        }

        #endregion // ShowCursor

        #endregion // Private

        #region Static

        internal static FrameworkElement ResolveContainer(XamDialogWindow dialog)
        {
            return VisualTreeHelper.GetParent(dialog) as FrameworkElement;
        }


        #endregion // Static

        /// <summary>
        /// When the content changes, it's going to cause issues with the Centering Logic since it will be looking at the old values.
        /// We set a flag here so centering know that it should re-measure the dialog.
        /// </summary>
        /// <param name="oldContent"></param>
        /// <param name="newContent"></param>
        protected override void OnContentChanged(object oldContent, object newContent)
        {
            _contentChanged = true;
            base.OnContentChanged(oldContent, newContent);
        }

        #endregion // Methods

        #region EventHandlers

        #region XamDialogWindow IsEnabledChanged

        void XamWebDialogWindow_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            this.EnsureVisualStates();
        }

        #endregion // XamDialogWindow IsEnabledChanged

        #region RootElement_LayoutUpdated
        void RootElement_LayoutUpdated(object sender, EventArgs e)
        {
            this.DetachFromLayout();
        }
        #endregion // RootElement_LayoutUpdated

        #region XamWebDialogWindow_SizeChanged

        void XamWebDialogWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // We used to change the rootElement's height and width here, however to allow for the contentSize to be set, we are no longer doing this. 
            if(!double.IsNaN(this.Height))
                this._rootElement.Height = e.NewSize.Height;

            if (!double.IsNaN(this.Width))
                this._rootElement.Width = e.NewSize.Width;           

        }
        #endregion // XamWebDialogWindow_SizeChanged

        #region XamDialogWindow_Unloaded

        void XamDialogWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            DialogManager.Manager.UnregisterModal(this);
            DialogManager.Manager.Unregister(this);
            DialogManager.Manager.UnregisterRestricted(this);
            CommandSourceManager.UnregisterCommandTarget(this);

            // It's important to call this method last so we can use the internal ContainerElement property in
            // the Unregister methods of the DialogManager.
            this.RemoveHandlersFromOwningContainer();
        }

        #endregion // XamDialogWindow_Unloaded

        #region HeaderArea_MouseLeftButtonDown

        private void HeaderArea_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.Handled)
            {
                
                return;
            }

            DateTime now = DateTime.Now;
            Point position = e.GetPosition(this._headerArea);
            if ((now.Subtract(_lastMouseDownTime).TotalMilliseconds <= 400.0) &&
                (Math.Sqrt(Math.Pow(this._lastMouseDownPoint.X - position.X, 2.0) +
                           Math.Pow(this._lastMouseDownPoint.Y - position.Y, 2.0)) < 4.0))
            {
                if (sender.Equals(this._headerArea))
                {
                    if (this.IsEnabled && !this.IsModal)
                    {
                        if (this.MaximizeButtonVisibility == Visibility.Visible)
                        {
                            switch (this.WindowState)
                            {
                                case WindowState.Normal:
                                    this.WindowState = WindowState.Maximized;
                                    break;
                                case WindowState.Maximized:
                                    this.WindowState = WindowState.Normal;
                                    break;
                            }
                        }
                    }

                    this.OnHeaderDoubleClick(e);
                    return;
                }
            }
            else
            {
                this._lastMouseDownTime = now;
                this._lastMouseDownPoint = position;
            }

            this.IsActive = true;

            if (e.OriginalSource == this._headerArea)
                this.Focus();

            if (this.IsMoveable && this.WindowState != WindowState.Maximized && this.WindowState != WindowState.Minimized)
            {
                this._headerArea.CaptureMouse();

                
                this._headerArea.LostMouseCapture += this.HeaderArea_LostMouseCapture;
                this._isMoving = true;
                //this._clickPoint = e.GetPosition(this._rootElement);
                this._clickPoint = e.GetPosition(PlatformProxy.GetRootVisual(this));
                this._oldPos = new Point(this.Left, this.Top);
            }
        }

        #endregion // HeaderArea_MouseLeftButtonDown

        #region HeaderArea_LostMouseCapture

        private void HeaderArea_LostMouseCapture(object sender, MouseEventArgs e)
        {
            this.EndMove();
        }

        #endregion //HeaderArea_LostMouseCapture

        #region HeaderArea_MouseLeftButtonUp
        private void HeaderArea_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.EndMove();
        }
        #endregion // HeaderArea_MouseLeftButtonUp

        #region HeaderArea_MouseMove
        private void HeaderArea_MouseMove(object sender, MouseEventArgs e)
        {
            if (this._isMoving && this._rootElement != null)
            {
                UIElement rootVisualElement = PlatformProxy.GetRootVisual(this);
                Point mousePosition = e.GetPosition(rootVisualElement);

                Point previousMousePosition = this._clickPoint;
                double deltaX = previousMousePosition.X - mousePosition.X;
                double deltaY = previousMousePosition.Y - mousePosition.Y;

                
                if (mousePosition.X < 0 || mousePosition.Y < 0 || mousePosition.X > rootVisualElement.RenderSize.Width ||
                    mousePosition.Y > rootVisualElement.RenderSize.Height)
                {
                    return;
                }

                



                FrameworkElement fe = rootVisualElement as FrameworkElement;
                if (fe != null && fe.FlowDirection == System.Windows.FlowDirection.RightToLeft)
                {
                    deltaX = -deltaX;
                }

                bool canMoveLeft = true, canMoveTop = true;

                if (this.RestrictInContainer && !this.IsModal)
                {
                    fe = ResolveContainer(this);

                    Point mousePositionRestricted = e.GetPosition(fe);

                    if (mousePositionRestricted.X < 0 || mousePositionRestricted.X > fe.ActualWidth)
                        canMoveLeft = false;

                    if (mousePositionRestricted.Y < 0 || mousePositionRestricted.Y > fe.ActualHeight)
                        canMoveTop = false;

                    if (fe != null && fe.FlowDirection == System.Windows.FlowDirection.RightToLeft ||
                        this.FlowDirection == System.Windows.FlowDirection.RightToLeft)
                    {
                        deltaX = -deltaX;
                    }
                }

                if(canMoveLeft)
                    this.Left -= deltaX;

                if(canMoveTop)
                    this.Top -= deltaY;
                this._clickPoint = mousePosition;
            }
        }

        #endregion // HeaderArea_MouseMove

        #region ResizeArea_MouseLeftButtonUp
        private void ResizeArea_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            this.EndResize();

            if (this._isMouseOverResizeArea)
                this.ShowCursor(this.DetermineMousePosition(e));
        }
        #endregion // ResizeArea_MouseLeftButtonUp

        #region ResizeArea_MouseLeave
        private void ResizeArea_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!this._isResizing)
            {
                this.HideCustomCursor();
            }

            this._isMouseOverResizeArea = false;
        }
        #endregion // ResizeArea_MouseLeave

        #region ResizeArea_MouseEnter
        private void ResizeArea_MouseEnter(object sender, MouseEventArgs e)
        {
            this._isMouseOverResizeArea = true;
        }
        #endregion // ResizeArea_MouseEnter

        #region ResizeArea_MouseLeftButtonDown

        private void ResizeArea_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.IsActive = true;
            this.Focus();

            if (this.IsResizable && this.WindowState == WindowState.Normal)
            {
                Point pos = this.DetermineMousePosition(e);

                this._resizeArea.CaptureMouse();
                
                this._resizeArea.LostMouseCapture += this.ResizeArea_LostMouseCapture;
                this._previousPos = pos;
                this._isOut = false;
                this._isResizing = true;


                this._resizeDir = this.ResolveResizeDirection(e.GetPosition(this._resizeArea));



                double height = this._virtualHeight = this._rootElement.ActualHeight;
                double width = this._virtualWidth = this._rootElement.ActualWidth;

                this.SetWidth(width);
                this.SetHeight(height);

                e.Handled = true;
            }
        }

        #endregion // ResizeArea_MouseLeftButtonDown

        #region ResizeArea_LostMouseCapture

        private void ResizeArea_LostMouseCapture(object sender, MouseEventArgs e)
        {
            this.EndResize();
        }

        #endregion //ResizeArea_LostMouseCapture

        #region ResizeArea_MouseMove
        private void ResizeArea_MouseMove(object sender, MouseEventArgs e)
        {
            if (this.IsResizable && this.WindowState == WindowState.Normal)
            {
                Point mousePosition;

                double zoom = PlatformProxy.GetZoomFactor();
                if (zoom == 0 || PlatformProxy.IsVersionSupported("4.0"))
                {
                    zoom = 1;
                }

                mousePosition = this.DetermineMousePosition(e);

                if (this._isResizing)
                {



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)


                    double left = this.Left, top = this.Top;
                    bool allowResizeLogic = true;

                    // Previously we were returning out, with the fixes for (29458 and 29460) which actually
                    // Caused some very undesirable behavior, where the mouse would freeze in place while resizing. 
                    // When in fact, instead of returning out, we want to just disable resizing logic, b/c we've reached the outer bounds
                    // So instead, lets move this logic above where the virtual sizing occurs, and determine right now, whether we should
                    // perform a resize, or just update the cursor position.



#region Infragistics Source Cleanup (Region)































#endregion // Infragistics Source Cleanup (Region)


                    if (allowResizeLogic)
                    {
                        Point virtualPosDiff = new Point(this._resizeDir.X * (mousePosition.X - this._previousPos.X), this._resizeDir.Y * (mousePosition.Y - this._previousPos.Y));

                        this._virtualWidth += virtualPosDiff.X;
                        this._virtualHeight += virtualPosDiff.Y;

                        // Make sure that the width doesn't exceed the Max and go under the Min
                        double constrainedWidth = Math.Min(Math.Max(this._virtualWidth, this.MinWidth), this.MaxWidth);

                        // Make sure that the height doesn't exceed the Max and go under the Min
                        double constrainedHeight = Math.Min(Math.Max(this._virtualHeight, this.MinHeight), this.MaxHeight);

                        // Update the Height and Width 
                        Point posDiff = new Point(constrainedWidth - this.ActualWidth, constrainedHeight - this.ActualHeight);

                        double newWidth = this.ActualWidth + posDiff.X;
                        double newHeight = this.ActualHeight + posDiff.Y;

                        if (this._resizeDir.X < 0.0)
                        {
                            left -= posDiff.X;
                        }

                        if (this._resizeDir.Y < 0.0)
                        {
                            top -= posDiff.Y;
                        }



                        if (this.RestrictInContainer)
                        {
                            FrameworkElement container = XamDialogWindow.ResolveContainer(this);
                            if (container != null)
                            {
                                Point containerCoords = this.GetContainerPanelCoords(container);

                                if (this.IsModal)
                                {
                                    containerCoords.X *= -1;
                                    containerCoords.Y *= -1;
                                }

                                if (left + containerCoords.X < 0)
                                {
                                    left = -containerCoords.X;
                                    newWidth = this.Width + this.Left;
                                }
                                else if (left + newWidth + containerCoords.X > container.ActualWidth)
                                {
                                    newWidth = (container.ActualWidth) - left - containerCoords.X;
                                }

                                if (top + containerCoords.Y < 0)
                                {
                                    top = -containerCoords.Y;
                                    newHeight = this.Height + this.Top;
                                }
                                else if (top + newHeight + containerCoords.Y > container.ActualHeight)
                                {
                                    newHeight = (container.ActualHeight) - top - containerCoords.Y;
                                }
                            }
                        }

                        double leftDiff = this.Left - left;
                        double topDiff = this.Top - top;


                        bool canMoveTop = true, canMoveLeft = true;


#region Infragistics Source Cleanup (Region)
















#endregion // Infragistics Source Cleanup (Region)

                        if (canMoveLeft)
                        {
                            this.Left = left;
                            this.SetWidth(newWidth);
                        }
                        else
                        {
                            leftDiff = 0; 
                        }

                        if (canMoveTop)
                        {
                            this.Top = top;
                            this.SetHeight(newHeight);
                        }
                        else
                        {
                            topDiff = 0; 
                        }

                        this.UpdateLayout();

                        // Store off the last position, so that we can accurately calculate the height and width the next time this method is triggered
                        double previousX = mousePosition.X + ((this._resizeDir.X < 0.0) ? leftDiff : 0.0);
                        double previousY = mousePosition.Y + ((this._resizeDir.Y < 0.0) ? topDiff : 0.0);

                        if (!this._isOut)
                        {
                            this._previousPos = new Point(previousX, previousY);
                        }
                    }
                }
                else
                {



                    this.ShowCursor(e.GetPosition(this._resizeArea));

                }

                if (this._customCursorPopup != null && this._customCursorPopup.IsOpen)
                {
                    FrameworkElement root = PlatformProxy.GetRootVisual(this) as FrameworkElement;

                    if (root != null)
                    {
                        Point p = e.GetPosition(root);

                        Point resizeType = this.ResolveResizeDirection(mousePosition);

                        double offsetX = 8.5;
                        double offsetY = 8.5;

                        FrameworkElement fe = this._customCursorPopup.Child as FrameworkElement;
                        if (fe != null)
                        {
                            if (fe.DesiredSize.Height == 0 || fe.DesiredSize.Width == 0)
                            {
                                fe.UpdateLayout();
                            }
                            offsetX = fe.DesiredSize.Width / 2;
                            offsetY = fe.DesiredSize.Height / 2;
                        }

                        if (root.FlowDirection == System.Windows.FlowDirection.RightToLeft)
                        {
                            p.X = root.ActualWidth - p.X;
                            //If the flow direction is RtoL we need to reverse the sign of X
                            resizeType.X *= -1;
                        }

                        //8.5 is half the height/width of the cursor.
                        if (resizeType.Y > 0)
                        {
                            if (resizeType.X > 0)
                                p.X -= offsetX;
                            else
                                p.X += offsetX;

                            p.Y -= offsetY;
                        }

                        this._customCursorPopup.HorizontalOffset = p.X * zoom;
                        this._customCursorPopup.VerticalOffset = p.Y * zoom;
                    }
                }
            }
        }
        #endregion // ResizeArea_MouseMove

        #region RestrictInContainer_SizeChanged

        //private bool _isResize = false;
        void RestrictInContainer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //_isResize = true;
            this.Left = this.EnsureXCoordinatesRestricted();
            this.Top = this.EnsureYCoordinatesRestricted();
            //_isResize = false;
        }
        #endregion // RestrictInContainer_SizeChanged

        #region RootElement_MouseLeftButtonDown
        void RootElement_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.IsActive = true;
        }
        #endregion // RootElement_MouseLeftButtonDown

        #region MaximizedParentElement_SizeChanged
        void MaximizedParentElement_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.EnsureMaximizeSize();
        }
        #endregion // MaximizedParentElement_SizeChanged

        #region XamWebDialogWindow_Loaded
        private void XamWebDialogWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this._isLoaded = true;

            this.Focus();

            this.AddHandlersToOwningContainer();

            if (!this._isDesign)
            {
                if (!this.RestrictInContainer)
                {
                    if(this.WindowState != Interactions.WindowState.Minimized || this.MinimizedPanel == null)
                        DialogManager.Manager.Register(this);

                }
                CommandSourceManager.RegisterCommandTarget(this);

                if (this.IsModal)
                {
                    if (this.WindowState != WindowState.Hidden)
                        DialogManager.Manager.RegisterModal(this, true);
                    else
                        DialogManager.Manager.RegisterModal(this, false);
                }

                if (this.StartupPosition == StartupPosition.Center)
                {
                    _contentChanged = true;
                    this.CenterDialogWindow();
                }
            }
        }
        #endregion //XamWebDialogWindow_Loaded

        #region HeaderDoubleClick

        /// <summary>
        /// Occurs as mouse is doubleclick over the <see cref="XamDialogWindow"/>.
        /// </summary>
        public event MouseEventHandler HeaderDoubleClick;


        /// <summary>
        /// Called when is doubleclicked over header .
        /// </summary>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
        protected virtual void OnHeaderDoubleClick(MouseButtonEventArgs e)
        {
            if (this.HeaderDoubleClick != null)
            {
                this.HeaderDoubleClick(this._headerArea, e);
            }
        }

        #endregion HeaderDoubleClick

        #region IsActiveChanged

        /// <summary>
        /// Occurs when a <see cref="IsActive"/> property has changed.
        /// </summary>
        public event EventHandler<EventArgs> IsActiveChanged;

        /// <summary>
        /// Called when IsActive property has changed.
        /// </summary>
        protected virtual void OnIsActiveChanged(bool oldValue, bool newValue)
        {
            if (this.IsActiveChanged != null)
            {
                this.IsActiveChanged(this, EventArgs.Empty);
            }
        }

        #endregion IsActiveChanged

        #region Moving

        /// <summary>
        /// Occurs when a <see cref="XamDialogWindow"/> starting to move.
        /// </summary>
        public event EventHandler<MovingEventArgs> Moving;

        /// <summary>
        /// Called when <see cref="XamDialogWindow"/> is moving.
        /// </summary>
        /// <returns></returns>
        protected virtual bool OnMoving()
        {
            if (this.Moving != null)
            {
                MovingEventArgs arg = new MovingEventArgs { Left = this.Left, Top = this.Top };

                this.Moving(this, arg);

                return arg.Cancel;

            }
            return false;
        }

        #endregion Moving

        #region Moved

        /// <summary>
        /// Occurs when a <see cref="XamDialogWindow"/> has moved.
        /// </summary>
        public event EventHandler<MovedEventArgs> Moved;

        /// <summary>
        /// Called when <see cref="XamDialogWindow"/> is moved.
        /// </summary>
        protected virtual void OnMoved()
        {
            if (this.Moved != null)
            {
                this.Moved(this, new MovedEventArgs { Left = this.Left, Top = this.Top });
            }

            this.InvalidateArrange();
        }

        #endregion Moved

        #region WindowStateChanging
        /// <summary>
        /// Occurs before the <see cref="WindowState"/> of the <see cref="XamDialogWindow"/> changes.
        /// </summary>
        /// <remarks>This event can be cancelled.</remarks>
        public event EventHandler<WindowStateChangingEventArgs> WindowStateChanging;

        /// <summary>
        /// Called when <see cref="XamDialogWindow"/> WindowState is changing.
        /// </summary>
        /// <param name="currentWindowState">Current state of the window.</param>
        /// <param name="newWindowState">New state of the window.</param>
        /// <returns>Cancel property value</returns>
        protected virtual bool OnWindowStateChanging(WindowState currentWindowState, WindowState newWindowState)
        {
            if (this.WindowStateChanging != null)
            {
                WindowStateChangingEventArgs arg = new WindowStateChangingEventArgs(currentWindowState, newWindowState);
                this.WindowStateChanging(this, arg);
                return arg.Cancel;
            }
            return false;
        }
        #endregion WindowStateChanging

        #region WindowStateChanged

        /// <summary>
        /// Occurs when a <see cref="WindowState"/> property has changed.
        /// </summary>
        public event EventHandler<WindowStateChangedEventArgs> WindowStateChanged;

        /// <summary>
        /// Called when WindowState property changed.
        /// </summary>
        /// <param name="newWindowState">New state of the window.</param>
        /// <param name="previousWindowState">Previous state of the window.</param>
        protected virtual void OnWindowStateChanged(WindowState newWindowState, WindowState previousWindowState)
        {
            if (this.WindowStateChanged != null)
            {
                WindowStateChangedEventArgs args = new WindowStateChangedEventArgs(newWindowState, previousWindowState);
                this.WindowStateChanged(this, args);
            }

        }

        #endregion WindowStateChanged

        #region XamDialogWindow_LayoutUpdated
        void XamDialogWindow_LayoutUpdated(object sender, EventArgs e)
        {
            if (this.ActualHeight != 0 && this.ActualWidth != 0)
            {
                if (!this._isDetached)
                {
                    this.DetachFromLayout();
                    if (this.StartupPosition == StartupPosition.Center)
                    {
                        this.CenterDialogWindow();
                    }
                }
            }
        }
        #endregion // XamDialogWindow_LayoutUpdated

        #region RootElement_IsVisibleChanged


        void RootElement_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                this.CenterDialogWindow();
            }));
            this._rootElement.IsVisibleChanged -= RootElement_IsVisibleChanged;
        }

        #endregion // RootElement_IsVisibleChanged

        #endregion Events

        #region ICommandTarget Members

        bool ICommandTarget.SupportsCommand(ICommand command)
        {
            return this.SupportsCommand(command);
        }

        object ICommandTarget.GetParameter(CommandSource source)
        {
            return this.GetParameter(source);
        }

        #endregion ICommandTarget

        #region INotifyPropertyChanged Members

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The name of the property that has changed.</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion INotifyPropertyChanged Members

        #region IProvidePropertyPersistenceSettings Members

        #region PropertiesToIgnore

        /// <summary>
        /// Gets a List of properties that shouldn't be saved when the PersistenceManager goes to save them.
        /// </summary>
        protected virtual List<string> PropertiesToIgnore
        {
            get
            {
                List<string> list = new List<string>()
                {
                    "MinimizedPanel"
                };

                return list;
            }
        }

        List<string> IProvidePropertyPersistenceSettings.PropertiesToIgnore
        {
            get
            {
                return this.PropertiesToIgnore;
            }
        }

        #endregion // PropertiesToIgnore

        #region PriorityProperties

        /// <summary>
        /// Gets a List of properties that should be applied, before even trying to look at any other property on the object.
        /// </summary>
        protected virtual List<string> PriorityProperties
        {
            get
            {
                return null;
            }
        }
        List<string> IProvidePropertyPersistenceSettings.PriorityProperties
        {
            get { return this.PriorityProperties; }
        }


        #endregion // PriorityProperties

        #region FinishedLoadingPersistence
        /// <summary>
        /// Allows an object to perform an operation, after it's been loaded.
        /// </summary>
        protected virtual void FinishedLoadingPersistence()
        {

        }

        void IProvidePropertyPersistenceSettings.FinishedLoadingPersistence()
        {
            this.FinishedLoadingPersistence();
        }
        #endregion // FinishedLoadingPersistence

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