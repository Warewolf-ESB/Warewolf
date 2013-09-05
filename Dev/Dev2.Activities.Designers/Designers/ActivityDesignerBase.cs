using System;
using System.Activities.Presentation;
using System.Activities.Presentation.Model;
using System.Activities.Presentation.View;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
using Dev2.Activities.Annotations;
using Dev2.CustomControls.Behavior;
using Dev2.Providers.Errors;
using Dev2.Studio.AppResources.ExtensionMethods;
using Dev2.Studio.Core.Activities.Services;
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
        private IDesignerManagementService _designerManagementService;
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
            SubscribeToServices();
        }

        protected override void OnModelItemChanged(object newItem)
        {
            base.OnModelItemChanged(newItem);
            SubscribeToServices();
        }

        protected virtual void SubscribeToServices()
        {
            if(Context != null)
            {
                Context.Services.Subscribe<IDesignerManagementService>(SetDesignerManagementService);
            }
        }

        private void SetDesignerManagementService(IDesignerManagementService designerManagementService)
        {
            if(_designerManagementService != null)
            {
                _designerManagementService.CollapseAllRequested -= DesignerManagementService_CollapseAllRequested;
                _designerManagementService.ExpandAllRequested -= DesignerManagementService_ExpandAllRequested;
                _designerManagementService.RestoreAllRequested -= DesignerManagementService_RestoreAllRequested;
                _designerManagementService = null;
            }

            if(designerManagementService != null)
            {
                _designerManagementService = designerManagementService;
                _designerManagementService.CollapseAllRequested += DesignerManagementService_CollapseAllRequested;
                _designerManagementService.ExpandAllRequested += DesignerManagementService_ExpandAllRequested;
                _designerManagementService.RestoreAllRequested += DesignerManagementService_RestoreAllRequested;
            }
        }

        #region expand/collapse all

        protected void DesignerManagementService_RestoreAllRequested(object sender, EventArgs e)
        {
            if(ViewModel == null)
            {
                return;
            }

            ViewModel.ActiveOverlay = ViewModel.PreviousOverlayType;
        }

        protected void DesignerManagementService_ExpandAllRequested(object sender, EventArgs e)
        {
            if(ViewModel == null)
            {
                return;
            }

            ViewModel.PreviousOverlayType = ViewModel.ActiveOverlay;
            ViewModel.ActiveOverlay = OverlayType.LargeView;
        }

        protected void DesignerManagementService_CollapseAllRequested(object sender, EventArgs e)
        {
            if(ViewModel == null)
            {
                return;
            }

            ViewModel.PreviousOverlayType = ViewModel.ActiveOverlay;
            ViewModel.ActiveOverlay = OverlayType.None;
        }

        #endregion

    }

    /// <summary>
    /// The base class to be used by all activities
    /// MUST be non-generic in order for WPF binding to attached properties to work!!!
    /// </summary>
    /// <author>Jurie.smit</author>
    /// <date>2013/07/24</date>
    public abstract class ActivityDesignerBase : ActivityDesigner,
        IActivityDesigner, INotifyPropertyChanged
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
        private static OverlayType _fromOverLayType;
        AdornerLayer _layer;
        Grid _adornedHostGrid;

        #endregion fields

        #region ctor

        protected ActivityDesignerBase()
        {
            Loaded += OnLoaded;
            DragEnter += OnDragEnter;
            DragLeave += OnDragLeave;
            MouseEnter += OnMouseEnter;
            MouseLeave += OnMouseLeave;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            UpdateLayout();
            Initialize(new UIElementProvider());
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
            var designer = (ActivityDesignerBase)o;
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
            var designer = (ActivityDesignerBase)o;
            _fromOverLayType = (OverlayType)e.OldValue;
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
            var isShown = (bool)e.NewValue;
            if(isShown)
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
            typeof(ActivityDesignerBase), new PropertyMetadata(string.Empty, HelpTextChangedCallback));


        public List<IActionableErrorInfo> Errors
        {
            get { return (List<IActionableErrorInfo>)GetValue(ErrorsProperty); }
            set { SetValue(ErrorsProperty, value); }
        }

        public static readonly DependencyProperty ErrorsProperty =
            DependencyProperty.Register("Errors", typeof(List<IActionableErrorInfo>),
            typeof(ActivityDesignerBase), new PropertyMetadata(new List<IActionableErrorInfo>()));


        private OverlayType _dragEnterOverlayType;

        private static void HelpTextChangedCallback(DependencyObject o, DependencyPropertyChangedEventArgs args)
        {
            var designer = (ActivityDesignerBase)o;
            var oldText = args.OldValue.ToString();
            var newText = args.NewValue.ToString();

            if(oldText != newText && !string.IsNullOrWhiteSpace(newText))
            {
                designer.OverlayAdorner.UpdateContentSize();
            }
        }

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
            BeginInit();
            InitializeViewModel();
            InitializeAdornerLayer();
            InsertOverlayAdorner();
            InsertOptionsAdorner();
            InsertHelpButton();
            EndInit();
            InitOverlayState();
        }

        private void InsertHelpButton()
        {
            Adorners.Add(new HelpCommandAdornerPresenter(ModelItem));
        }

        private void InitOverlayState()
        {
            if(Context == null)
            {
                return;
            }
            var overlayService = Context.Services.GetService<OverlayService>();
            if(overlayService == null)
            {
                return;
            }
            ActiveOverlay = overlayService.OnLoadOverlayType;
            overlayService.OnLoadOverlayType = OverlayType.None;
        }

        public virtual void HideContent()
        {
            if(ActiveOverlay != OverlayType.None)
            {
                //Make sure that the quick variable and large view returns to where it was initiated
                ActiveOverlay = ActiveOverlay == OverlayType.QuickVariableInput ? _fromOverLayType : OverlayType.None;
            }
            else
            {
                if(OverlayAdorner != null)
                {
                    OverlayAdorner.HideContent();
                }
                AddConnectorNodeAdorners();
                if(OptionsAdorner != null)
                {
                    OptionsAdorner.ResetSelection();
                }
            }
        }

        public void ShowContent(IAdornerPresenter adornerPresenter)
        {
            if(adornerPresenter == null || adornerPresenter.Content == null)
            {
                HideContent();
                return;
            }

            if(!ReferenceEquals(OverlayAdorner.Content, adornerPresenter.Content))
            {
                OverlayAdorner.ChangeContent(adornerPresenter.Content,
                                             adornerPresenter.OverlayType.GetContentAutomationId());
            }
            else
            {
                OverlayAdorner.ShowContent();
            }

            if(OptionsAdorner != null)
            {
                OptionsAdorner.SelectButton(adornerPresenter.Button);
            }
            ActiveOverlay = adornerPresenter.OverlayType;
            RemoveConnectorNodeAdorners();
            BringToFront();
            SelectThis();
        }

        public void ShowContent(OverlayType overlayType)
        {
            if(overlayType == OverlayType.None)
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
            if(selectedOption == null)
            {
                HideContent();
                return;
            }

            if(!(selectedOption is AdornerToggleButton))
            {
                HideContent();
                ToggleOptionsAdorner();
                return;
            }

            IAdornerPresenter adornerPresenter = Adorners.FirstOrDefault(a => ReferenceEquals(a.Button, selectedOption));
            var selectedAdornerWrapper = adornerPresenter as AdornerPresenterBase;
            if(selectedAdornerWrapper != null)
            {
                ShowContent(selectedAdornerWrapper);
            }
        }

        protected void SelectionChanged(Selection item)
        {
            WorkflowDesignerSelection = item;

            if(WorkflowDesignerSelection == null)
            {
                return;
            }

            if(WorkflowDesignerSelection.PrimarySelection == ModelItem)
            {
                IsSelected = true;
                IsAdornerButtonsShown = true;
                BringToFront();
            }
            else
            {
                IsSelected = false;
                IsAdornerButtonsShown = false;
                SendToBack();
            }
        }

        #endregion

        #region protected overrides
        protected void OnDragEnter(object sender, DragEventArgs dragEventArgs)
        {
            _dragEnterOverlayType = ActiveOverlay;
            ActiveOverlay = OverlayType.None;
        }

        protected void OnDragLeave(object sender, DragEventArgs dragEventArgs)
        {
            ActiveOverlay = _dragEnterOverlayType;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            ToggleActivityOptions(e);
            base.OnMouseMove(e);
        }

        protected void OnMouseEnter(object sender, MouseEventArgs e)
        {
            ToggleActivityOptions(e);
            BringToFront();
        }

        protected void OnMouseLeave(object sender, MouseEventArgs e)
        {
            ToggleActivityOptions(e);
            if(!IsSelected && !IsMouseOver)
            {
                SendToBack();
            }
        }

        protected override void OnModelItemChanged(object newItem)
        {
            if(newItem is ModelItem && (newItem as ModelItem).Properties != null)
            {
                base.OnModelItemChanged(newItem);
                if(Context != null)
                {
                    Context.Items.Subscribe<Selection>(SelectionChanged);
                }
            }
        }

        protected override void OnPreviewDragEnter(DragEventArgs e)
        {
            HideAdornerButtons();
            ActiveOverlay = OverlayType.None;
            base.OnPreviewDragEnter(e);
        }

        //DONT TAKE OUT... This has been done so that the drill down doesnt happen when you double click.
        protected override void OnPreviewMouseDoubleClick(MouseButtonEventArgs e)
        {
            if(!(e.OriginalSource is IScrollInfo))
            {
                e.Handled = true;
            }
        }

        #endregion protected overrides

        #region event handlers

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
            ToggleOptionsAdorner();
        }

        private void ToggleOptionsAdorner()
        {
            IsAdornerButtonsShown = (IsMouseOver || (OptionsAdorner != null && OptionsAdorner.IsMouseOver));
            if(IsAdornerButtonsShown)
            {
                if(ActiveOverlay != OverlayType.None)
                {
                    RemoveConnectorNodeAdorners();
                }
            }
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

        void InsertOptionsAdorner()
        {
            if(_isOptionsAdornerLoaded)
            {
                return;
            }

            var border = _uiElementProvider.GetColoursBorder(this);
            var rectangle = _uiElementProvider.GetDisplayNameWidthSetter(this);

            _displayNameTextBox = _uiElementProvider.GetTitleTextBox(this);
            if(_displayNameTextBox != null)
            {
                _displayNameTextBox.GotKeyboardFocus += OnDisplayNameTextBoxGotKeyboardFocus;
            }
            if(_adornedHostGrid != null && _overlaySizeBindingBehavior != null)
            {
                OptionsAdorner = new OptionsAdorner(_adornedHostGrid, _overlaySizeBindingBehavior, border, rectangle);

                OptionsAdorner.PreviewMouseMove += OnActivityOptionsAdornerPreviewMouseMove;
                OptionsAdorner.PreviewMouseLeftButtonDown += OnActivityOptionsAdornerPreviewMouseLeftButtonDown;
                OptionsAdorner.MouseLeave += (o, e) => ToggleActivityOptions(e);
                OptionsAdorner.SelectionChanged += (o, e) => ShowContent(e.SelectedOption);
                OptionsAdorner.MouseLeftButtonDown += OnActivityOptionsAdornerMouseLeftButtonDown;
                OptionsAdorner.MouseLeftButtonUp += OnActivityOptionsAdornerMouseLeftButtonUp;
                OptionsAdorner.DragEnter += OnDragEnter;
                OptionsAdorner.DragLeave += OnDragEnter;
                OptionsAdorner.MouseEnter += OnMouseEnter;
                OptionsAdorner.MouseLeave += OnMouseLeave;

                AddAdorner(OptionsAdorner);
                Adorners.ToList().ForEach(AddAdornerOption);
                _isOptionsAdornerLoaded = true;
            }
            else
            {
                _isOptionsAdornerLoaded = false;
            }
        }

        void InsertOverlayAdorner()
        {
            if(_isOverlayAdornerLoaded)
            {
                return;
            }

            var border = _uiElementProvider.GetColoursBorder(this);
            if(_adornedHostGrid != null)
            {
                OverlayAdorner = new OverlayAdorner(_adornedHostGrid, border);

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

                var errorsTextBindings = new Binding
                    {
                        Path = new PropertyPath("Errors"),
                        Source = this
                    };

                OverlayAdorner.SetBinding(Activities.Adorners.OverlayAdorner.HelpTextProperty, helpTextBinding);
                OverlayAdorner.SetBinding(Activities.Adorners.OverlayAdorner.ErrorsProperty, errorsTextBindings);

                OverlayAdorner.UpdateComplete += (o, e) => HideContent();
                OverlayAdorner.DragEnter += OnDragEnter;
                OverlayAdorner.DragLeave += OnDragEnter;
                OverlayAdorner.MouseEnter += OnMouseEnter;
                OverlayAdorner.MouseLeave += OnMouseLeave;

                AddAdorner(OverlayAdorner);

                _isOverlayAdornerLoaded = true;
            }
        }

        void AddAdorner(ActivityAdorner adorner)
        {
            if(_layer != null)
            {
                _layer.Add(adorner);
                _layer.InvalidateArrange();
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
            ni.AssociatedActivityDesigner = this;
            if(OptionsAdorner != null)
            {
                OptionsAdorner.AddButton(ni.Button, ni.OverlayType != OverlayType.Help);
            }
            ni.Button.MouseLeftButtonUp += OnActivityOptionsAdornerMouseLeftButtonUp;
            ni.Button.MouseLeftButtonDown += OnActivityOptionsAdornerMouseLeftButtonDown;
        }

        void BringToFront()
        {
            var fElement = VisualTreeHelper.GetParent(this) as FrameworkElement;
            if(fElement != null)
            {
                fElement.BringToMaxFront();
            }
        }

        void SendToBack()
        {
            if(ActiveOverlay == OverlayType.None ||
                WorkflowDesignerSelection.PrimarySelection != ModelItem)
            {
                var fElement = VisualTreeHelper.GetParent(this) as FrameworkElement;
                if(fElement != null)
                {
                    fElement.SendToBack();
                }
            }
        }

        void RemoveConnectorNodeAdorners()
        {
            var layer = GetAdornerLayer();
            if(layer == null || Parent as UIElement == null)
            {
                return;
            }

            var adorners = layer.GetAdorners(Parent as UIElement);
            if(adorners != null)
            {
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
            var layer = GetAdornerLayer();
            if(layer == null)
            {
                return;
            }

            var adorners = layer.GetAdorners(this);
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

        public void InitializeAdornerLayer()
        {
            if(_layer == null)
            {
                _adornedHostGrid = this.FindVisualChildren<Grid>().FirstOrDefault();
                if(_adornedHostGrid != null)
                {
                    Border border = _adornedHostGrid.Parent as Border;
                    if(border != null)
                    {
                        _layer = InitDecorator(border, _adornedHostGrid);
                        _adornedHostGrid.UpdateLayout();
                        border.UpdateLayout();
                    }
                }
            }
        }

        static AdornerLayer InitDecorator(Border border, Grid grid)
        {
            AdornerDecorator dec = new AdornerDecorator();
            border.Child = null;
            dec.Child = grid;
            border.Child = dec;
            return AdornerLayer.GetAdornerLayer(grid);
        }

        #endregion

        #endregion  helpers

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if(handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
