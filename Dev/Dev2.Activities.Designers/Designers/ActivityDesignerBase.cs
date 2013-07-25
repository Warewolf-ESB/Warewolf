using System;
using System.Activities.Presentation;
using System.Activities.Presentation.View;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Dev2.Activities.Adorners;
using Dev2.CustomControls.Behavior;
using Dev2.Studio.AppResources.ExtensionMethods;
using Dev2.Util.ExtensionMethods;

namespace Dev2.Activities.Designers
{
    /// <summary>
    /// The strongly typed baseclass to be used by all activities
    /// </summary>
    /// <typeparam name="TViewModel">The type of the view model.</typeparam>
    /// <author>Jurie.smit</author>
    /// <date>2013/07/24</date>
    public abstract class ActivityDesignerBase<TViewModel> : ActivityDesignerBase
        where TViewModel : ActivityViewModelBase
    {

        public TViewModel ViewModel { get { return (TViewModel)DataContext; } }

        /// <summary>
        /// Initializes the view model.
        /// </summary>
        /// <author>Jurie.smit</author>
        /// <date>2013/07/24</date>
        protected override void InitializeViewModel()
        {
            DataContext = Activator.CreateInstance(typeof(TViewModel), ModelItem);
            var overlayTypeBinding = new Binding
                {
                    Path = new PropertyPath("ActiveOverlay"),
                    Source = ViewModel,
                    Mode = BindingMode.TwoWay
                };
            SetBinding(ActiveOverlayProperty, overlayTypeBinding);
        }
    }

    /// <summary>
    /// The base class to be used by all activities
    /// MUST be non-generic in order for WPF binding to attached properties to work!!!
    /// </summary>
    /// <author>Jurie.smit</author>
    /// <date>2013/07/24</date>
    public abstract class ActivityDesignerBase : ActivityDesigner, IActivityDesigner
    {
        #region fields

        AdornerPresenterCollection _adorners;
        protected AbstractOptionsAdorner OptionsAdorner;
        protected AbstractOverlayAdorner OverlayAdorner;
        protected Selection WorkflowDesignerSelection;

        bool _isOptionsAdornerLoaded;
        bool _isOverlayAdornerLoaded;

        ActualSizeBindingBehavior _overlaySizeBindingBehavior;
        Point _mousedownPoint = new Point(0, 0);
        bool _startManualDrag;
        TextBox _displayNameTextBox;
        IUIElementProvider _uiElementProvider;

        #endregion fields

        #region ctor

        protected ActivityDesignerBase()
        {
            Loaded += OnLoaded;
        }

        #endregion ctor

        #region Dependency Properties

        #region IconLocation

        public string IconLocation 
        {
            get { return (string)GetValue(IconLocationProperty); } 
            set { SetValue(IconLocationProperty, value); } 
        }

        public static readonly DependencyProperty IconLocationProperty =
            DependencyProperty.Register("IconLocation", typeof(string), typeof(ActivityDesignerBase),
                new PropertyMetadata(string.Empty, IconLocationChangedCallback));

        static void IconLocationChangedCallback(DependencyObject o, DependencyPropertyChangedEventArgs args)
        {
            var designer = (ActivityDesignerBase) o;
            var newLocation = (string)args.NewValue;
            var drawingBrush = new DrawingBrush();
            var imageDrawing = new ImageDrawing
            {
                Rect = new Rect(new Point(0, 0), new Size(16, 16)),
                ImageSource = new BitmapImage(new Uri(newLocation))
            };
            drawingBrush.Drawing = imageDrawing;
            designer.Icon = drawingBrush;
        }

        #endregion IconLocation

        #region ActiveOverlay

        public OverlayType ActiveOverlay 
        { 
            get
            {
                return (OverlayType)GetValue(ActiveOverlayProperty);
            } 
            set
            {
                SetValue(ActiveOverlayProperty, value);
            } 
        }

        public static readonly DependencyProperty ActiveOverlayProperty =
            DependencyProperty.Register("ActiveOverlay", typeof(OverlayType),
                typeof(ActivityDesignerBase), new PropertyMetadata(OverlayType.None, ActiveOverlayChanged));

        private static void ActiveOverlayChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var designer = (ActivityDesignerBase) o;
            var newType = (OverlayType)e.NewValue;
            designer.ShowContent(newType);
        }

        #endregion ActiveOverlay

        #region IsSelected

        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), 
            typeof(ActivityDesignerBase), new PropertyMetadata(false));

        #endregion

        #region IsAdornerButtonsShown

        public bool IsAdornerButtonsShown
        {
            get { return (bool)GetValue(IsAdornerButtonsShownProperty); }
            set { SetValue(IsAdornerButtonsShownProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsAdornerButtonsShown.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsAdornerButtonsShownProperty =
            DependencyProperty.Register("IsAdornerButtonsShown", typeof(bool), typeof(ActivityDesignerBase), new PropertyMetadata(false, IsAdornerButtonsShownChangedCallback));

        private static void IsAdornerButtonsShownChangedCallback(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var designer = (ActivityDesignerBase)o;
            var isShown = (bool) e.NewValue;
            if (isShown)
            {
                designer.ShowAdornerButtons();
            }
            else
            {
                designer.HideAdornerButtons();
            }
        }

        #endregion IsAdornerButtonsShown

        #region HelpText

        public string HelpText
        {
            get { return (string)GetValue(HelpTextProperty); }
            set { SetValue(HelpTextProperty, value); }
        }

        public static readonly DependencyProperty HelpTextProperty =
            DependencyProperty.Register("HelpText", typeof(string), 
            typeof(ActivityDesignerBase), new PropertyMetadata(string.Empty));

        #endregion HelpText

        #endregion

        #region public properties

        public AdornerPresenterCollection Adorners
        {
            get
            {
                if(_adorners == null)
                {
                    _adorners = new AdornerPresenterCollection();
                    _adorners.CollectionChanged += AdornersCollectionChanged;
                }

                return _adorners;
            }
        }

        #endregion

        #region public methods

        public void Initialize(IUIElementProvider uiElementProvider)
        {
            _uiElementProvider = uiElementProvider;
            InitializeViewModel();
            InsertOverlayAdorner();
            InsertAdornerButtonPanel();
        }
        public void HideContent()
        {
            OverlayAdorner.HideContent();
            AddConnectorNodeAdorners();
            ActiveOverlay = OverlayType.None;
        }

        public void ShowContent(IAdornerPresenter adornerPresenter)
        {
            if (adornerPresenter == null || adornerPresenter.Content == null)
            {
                ActiveOverlay = OverlayType.None;
                return;
            }

            if (!ReferenceEquals(OverlayAdorner.Content, adornerPresenter.Content))
            {
                OverlayAdorner.ChangeContent(adornerPresenter.Content,
                                             adornerPresenter.OverlayType.GetContentAutomationId());
            }
            else
            {
                OverlayAdorner.ShowContent();
            }

            ActiveOverlay = adornerPresenter.OverlayType;
            RemoveConnectorNodeAdorners();
            SelectThis();
        }

        public void ShowContent(OverlayType overlayType)
        {
            if (overlayType == OverlayType.None)
            {
                HideContent();
                return;
            }

            var adornerWrapper = Adorners.FirstOrDefault(a => a.OverlayType == overlayType);
            ShowContent(adornerWrapper);
        }

        #endregion

        #region protected

        protected abstract void InitializeViewModel();

        protected void ShowContent(ButtonBase selectedOption)
        {
            if (selectedOption == null)
            {
                HideContent();
            }

            if (!(selectedOption is AdornerToggleButton))
            {
                HideContent();
                return;
            }

            IAdornerPresenter adornerPresenter = Adorners.FirstOrDefault(a => ReferenceEquals(a.Button, selectedOption));
            var selectedAdornerWrapper = adornerPresenter as AdornerPresenterBase;
            if (selectedAdornerWrapper != null)
            {
                ShowContent(selectedAdornerWrapper);
            }
        }

        protected void SelectionChanged(Selection item)
        {
            WorkflowDesignerSelection = item;

            if (WorkflowDesignerSelection == null)
            {
                return;
            }

            if (WorkflowDesignerSelection.PrimarySelection == ModelItem)
            {
                IsSelected = true;
                IsAdornerButtonsShown = true;
            }
            else
            {
                IsSelected = false;
                IsAdornerButtonsShown = false;
            }
        }

        #endregion

        #region protected overrides

        protected override void OnMouseMove(MouseEventArgs e)
        {
            ToggleActivityOptions(e);
            base.OnMouseMove(e);
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            var uiElement = VisualTreeHelper.GetParent(this) as UIElement;
            if(uiElement != null)
            {
                Panel.SetZIndex(uiElement, int.MaxValue);
            }
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            ToggleActivityOptions(e);
            var uiElement = VisualTreeHelper.GetParent(this) as UIElement;
            if(uiElement != null)
            {
                Panel.SetZIndex(uiElement, int.MinValue);
            }
            base.OnMouseLeave(e);
        }

        protected override void OnModelItemChanged(object newItem)
        {
            base.OnModelItemChanged(newItem);
            if (Context != null)
            {
                Context.Items.Subscribe<Selection>(SelectionChanged);
            }
        }

        protected override void OnPreviewDragEnter(DragEventArgs e)
        {
            HideAdornerButtons();
            base.OnPreviewDragEnter(e);
        }

        //DONT TAKE OUT... This has been done so that the drill down doesnt happen when you double click.
        protected override void OnPreviewMouseDoubleClick(MouseButtonEventArgs e)
        {
            if (!(e.OriginalSource is IScrollInfo))
            {
                e.Handled = true;
            }
            //base.OnPreviewMouseDoubleClick(e);
        }

        #endregion protected overrides

        #region event handlers

        void OnLoaded(object sender, RoutedEventArgs e)
        {
            Initialize(new UIElementProvider());
        }


        void OnDisplayNameTextBoxGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            HideAdornerButtons();
        }

        void OnActivityOptionsAdornerMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SelectThis();

            _mousedownPoint = e.GetPosition(sender as IInputElement);
            _startManualDrag = true;
            e.Handled = true;
        }

        void OnActivityOptionsAdornerMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var inputElement = sender as IInputElement;
            if(inputElement == null)
            {
                return;
            }

            inputElement.ReleaseMouseCapture();
            Focus();
            BringToFront();
        }

        void OnActivityOptionsAdornerPreviewMouseMove(object sender, MouseEventArgs e)
        {
            if(e.LeftButton != MouseButtonState.Pressed)
            {
                return;
            }

            var inputElement = sender as IInputElement;
            if(inputElement == null)
            {
                return;
            }

            //if the user hasnt moved the mouse we should not do a fragmove, that will swallow the click.
            var tempPoint = e.GetPosition(sender as IInputElement);
            var xDelta = Math.Abs(tempPoint.X - _mousedownPoint.X);
            var yDelta = Math.Abs(tempPoint.Y - _mousedownPoint.Y);

            if(!_startManualDrag || !(Math.Max(xDelta, yDelta) >= 3))
            {
                return;
            }

            ToggleActivityOptions(e);

            //TODO dont use obsolete
            DragDropHelper.DoDragMove(this, e.GetPosition(this));
            _startManualDrag = false;
            inputElement.ReleaseMouseCapture();
            e.Handled = true;
        }

        void OnActivityOptionsAdornerPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _mousedownPoint = e.GetPosition(sender as IInputElement);
            _startManualDrag = true;
        }

        #endregion  handlers

        #region  helpers   

        void ToggleActivityOptions(MouseEventArgs e)
        {
            IsAdornerButtonsShown = (IsMouseOver || (OptionsAdorner != null && OptionsAdorner.IsMouseOver));
            //if(e != null)
            //{
            //    IsAdornerButtonsShown = IsAdornerButtonsShown && (e.LeftButton != MouseButtonState.Pressed);
            //}
        }

        void AdornersCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if(!IsInitialized)
            {
                return;
            }

            if(e.NewItems != null)
            {
                e.NewItems.Cast<IAdornerPresenter>().ToList().ForEach(AddAdornerOption);
            }

            if(e.OldItems != null)
            {
                e.OldItems.Cast<IAdornerPresenter>().ToList().ForEach(RemoveAdorner);
            }
        }

        void InsertAdornerButtonPanel()
        {
            if(_isOptionsAdornerLoaded)
            {
                return;
            }

            var border = _uiElementProvider.GetColoursBorder(this);
            var rectangle = _uiElementProvider.GetDisplayNameWidthSetter(this);
            _displayNameTextBox = _uiElementProvider.GetTitleTextBox(this);
            _displayNameTextBox.GotKeyboardFocus += OnDisplayNameTextBoxGotKeyboardFocus;
            OptionsAdorner = new OptionsAdorner(this, _overlaySizeBindingBehavior, border, rectangle);

            OptionsAdorner.PreviewMouseMove += OnActivityOptionsAdornerPreviewMouseMove;
            OptionsAdorner.PreviewMouseLeftButtonDown += OnActivityOptionsAdornerPreviewMouseLeftButtonDown;
            OptionsAdorner.MouseLeave += (o, e) => ToggleActivityOptions(e);
            OptionsAdorner.SelectionChanged += (o, e) => ShowContent(e.SelectedOption);
            OptionsAdorner.MouseLeftButtonDown += OnActivityOptionsAdornerMouseLeftButtonDown;
            OptionsAdorner.MouseLeftButtonUp += OnActivityOptionsAdornerMouseLeftButtonUp;

            AddAdorner(OptionsAdorner);
            Adorners.ToList().ForEach(AddAdornerOption);
            _isOptionsAdornerLoaded = true;
        }

        void InsertOverlayAdorner()
        {
            if(_isOverlayAdornerLoaded)
            {
                return;
            }

            var border = _uiElementProvider.GetColoursBorder(this);
            OverlayAdorner = new OverlayAdorner(this, border);

            _overlaySizeBindingBehavior = new ActualSizeBindingBehavior
            {
                HorizontalOffset = 1
            };

            Interaction.GetBehaviors(OverlayAdorner).Add(_overlaySizeBindingBehavior);

            //set the binding to show the helptext on the overlay
            var helpTextBinding = new Binding
                {
                    Path = new PropertyPath("HelpText"),
                    Source = this
                };
            OverlayAdorner.SetBinding(Activities.Adorners.OverlayAdorner.HelpTextProperty, helpTextBinding);

            AddAdorner(OverlayAdorner);

            _isOverlayAdornerLoaded = true;
        }

        void AddAdorner(ActivityAdorner adorner)
        {
            var adornerLayer = GetAdornerLayer();
            if (adornerLayer != null)
            {
                adornerLayer.Add(adorner);
                adornerLayer.InvalidateArrange();
            }

            adorner.HideContent();
            adorner.MouseLeave += (o, e) => ToggleActivityOptions(e);
        }

        void SelectThis()
        {
            if(WorkflowDesignerSelection != null &&
               WorkflowDesignerSelection.SelectedObjects.FirstOrDefault() != ModelItem)
            {
                Selection.SelectOnly(Context, ModelItem);
                IsAdornerButtonsShown = true;
            }
        }

        void HideAdornerButtons()
        {
            if(ActiveOverlay != OverlayType.None || IsSelected)
            {
                IsAdornerButtonsShown = true;
                return;
            }

            if(OptionsAdorner != null)
            {
                OptionsAdorner.HideContent();
                IsAdornerButtonsShown = false;
            }
        }

        void ShowAdornerButtons()
        {
            if(_displayNameTextBox == null || _displayNameTextBox.IsKeyboardFocusWithin)
            {
                return;
            }

            if(OptionsAdorner != null)
            {
                OptionsAdorner.ShowContent();
                IsAdornerButtonsShown = true;
            }

            if(ActiveOverlay != OverlayType.None)
            {
                RemoveConnectorNodeAdorners();
            }
        }

        void RemoveAdorner(IAdornerPresenter ni)
        {
            OptionsAdorner.RemoveButton(ni.Button);
        }

        void AddAdornerOption(IAdornerPresenter ni)
        {
            OptionsAdorner.AddButton(ni.Button);
            ni.Button.MouseLeftButtonUp += OnActivityOptionsAdornerMouseLeftButtonUp;
            ni.Button.MouseLeftButtonDown += OnActivityOptionsAdornerMouseLeftButtonDown;
        }

        void BringToFront()
        {
            var fElement = VisualTreeHelper.GetParent(this) as FrameworkElement;
            if(fElement != null)
            {
                fElement.BringToFront();
            }
        }

        void RemoveConnectorNodeAdorners()
        {
            var adornerLayer = GetAdornerLayer();
            if(adornerLayer == null || Parent as UIElement == null)
            {
                return;
            }

            var adorners = adornerLayer.GetAdorners(Parent as UIElement);
            if(adorners != null)
            {
                //FlowChartConnectionPointsAdorner
                foreach(var adorner in adorners)
                {
                    if(!(adorner is ActivityAdorner))
                    {
                        adorner.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }

        void AddConnectorNodeAdorners()
        {
            var adornerLayer = GetAdornerLayer();
            if (adornerLayer == null)
            {
                return;
            }

            var adorners = adornerLayer.GetAdorners(this);
            if(adorners == null)
            {
                return;
            }

            foreach(var adorner in adorners)
            {
                if(!(adorner is ActivityAdorner))
                {
                    adorner.Visibility = Visibility.Visible;
                }
            }
        }

        #region get ui elements

        public UIElement GetContainingElement()
        {
            return DependencyObjectExtensionMethods.GetContainingElement(this);
        }

        public AdornerLayer GetAdornerLayer()
        {
            return DependencyObjectExtensionMethods.GetAdornerLayer(this);
        }

        #endregion

        #endregion  helpers

        #region Commented out - dont remove - might be needed to handle drag-drop issues

        // void UIElement_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        //{
        //    var inputElement = sender as IInputElement;
        //    if (inputElement == null)
        //    {
        //        return;
        //    }

        //    Mouse.Capture(sender as IInputElement, CaptureMode.SubTree);

        //    if (_workflowDesignerSelection != null &&
        //        _workflowDesignerSelection.SelectedObjects.FirstOrDefault() != ModelItem)
        //    {
        //        Selection.SelectOnly(Context, ModelItem);
        //    }

        //    _mousedownPoint = e.GetPosition(sender as IInputElement);
        //    _startManualDrag = true;
        //    e.Handled = true;
        //}

        // void UIElement_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        //{
        //    var inputElement = sender as IInputElement;
        //    if (inputElement == null)
        //    {
        //        return;
        //    }

        //    inputElement.ReleaseMouseCapture();
        //    Focus();
        //    BringToFront();
        //}

        // void UIElement_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        //{
        //    var inputElement = sender as IInputElement;
        //    if (inputElement == null)
        //    {
        //        return;
        //    }

        //    Mouse.Capture(sender as IInputElement, CaptureMode.SubTree);

        //    if (_workflowDesignerSelection != null &&
        //        _workflowDesignerSelection.SelectedObjects.FirstOrDefault() != ModelItem)
        //    {
        //        Selection.Select(Context, ModelItem);
        //    }
        //}

        #endregion
    }
}
