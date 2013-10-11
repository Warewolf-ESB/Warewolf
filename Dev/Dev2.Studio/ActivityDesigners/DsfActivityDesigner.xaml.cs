using System;
using System.Activities.Presentation;
using System.Activities.Presentation.Model;
using System.Activities.Presentation.View;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;
using Dev2.Activities.Adorners;
using Dev2.Data.Interfaces;
using Dev2.Studio.ActivityDesigners.Singeltons;
using Dev2.Studio.AppResources.Behaviors;
using Dev2.Studio.AppResources.ExtensionMethods;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Activities.Services;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.ViewModels.ActivityViewModels;
using Dev2.Util.ExtensionMethods;
using Dev2.Utilities;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    // Interaction logic for DsfActivityDesigner.xaml
    public partial class DsfActivityDesigner : IDisposable
    {
        #region Class Members

        private IDesignerManagementService _designerManagementService;
        private bool _inputOutPutMappingHeightInitialized;
        private Point _mousedownPoint = new Point(0, 0);
        private bool _startManualDrag;
        private DsfActivityViewModel _viewModel;
        private Selection _workflowDesignerSelection;

        #endregion Class Members

        #region dependency properties

        public bool ShowAdorners
        {
            get { return (bool)GetValue(ShowAdornersProperty); }
            set { SetValue(ShowAdornersProperty, value); }
        }

        public static readonly DependencyProperty ShowAdornersProperty =
            DependencyProperty.Register("ShowAdorners", typeof(bool), typeof(DsfActivityDesigner),
            new PropertyMetadata(false, ShowAdornersPropertyPropertyChangedCallback));

        private static void ShowAdornersPropertyPropertyChangedCallback(DependencyObject o, DependencyPropertyChangedEventArgs args)
        {
            var designer = o as DsfActivityDesigner;
            if(designer != null)
            {
                var behavior = Interaction.GetBehaviors(designer)
                    .FirstOrDefault(d => d.GetType() == typeof(ActivityDesignerAugmentationBehavior))
                    as ActivityDesignerAugmentationBehavior;
                if(behavior != null)
                {
                    behavior.SupressConnectorNodes =
                        (bool)args.NewValue && !designer.IsSelected && !designer.IsMouseOver
                        || designer.IsAdornerOpen;
                }
            }
        }

        public bool IsAdornerOpen
        {
            get { return (bool)GetValue(IsAdornerOpenProperty); }
            set
            {
                SetValue(IsAdornerOpenProperty, value);
            }
        }

        public static readonly DependencyProperty IsAdornerOpenProperty =
            DependencyProperty.Register("IsAdornerOpen", typeof(bool), typeof(DsfActivityDesigner),
            new PropertyMetadata(false, PropertyChangedCallback));

        private static void PropertyChangedCallback(DependencyObject o, DependencyPropertyChangedEventArgs args)
        {
            var designer = o as DsfActivityDesigner;
            if(designer != null)
            {
                var openMappings =
                    designer.Descendents().FirstOrDefault(d => d.GetType() == typeof(AdornerToggleButton))
                    as AdornerToggleButton;
                if(openMappings != null)
                {
                    openMappings.IsChecked = args.NewValue as bool?;
                }
            }
        }

        #endregion dependency properties

        #region Constructor

        public DsfActivityDesigner()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        #endregion Constructor

        #region Override Methods

        protected override void OnModelItemChanged(object newItem)
        {
            base.OnModelItemChanged(newItem);

            Context.Items.Subscribe<Selection>(SelectionChanged);
            Context.Services.Subscribe<IDesignerManagementService>(SetDesignerManagementService);

            ModelItem = newItem as ModelItem;
            if(ModelItem != null)
            {
                ModelProperty modelProperty = ModelItem.Properties["ServiceName"];
                if(modelProperty != null && modelProperty.ComputedValue != null)
                {
                    string disName = modelProperty.ComputedValue.ToString();
                    ModelProperty property = ModelItem.Properties["DisplayName"];
                    if(property != null)
                    {
                        property.SetValue(disName);
                    }
                }
            }
        }

        #endregion Override Methods

        #region Private Methods

        private void InitializeViewModel()
        {
            if(_viewModel != null)
            {
                _viewModel.Dispose();
            }

            if(_designerManagementService == null)
            {
                return;
            }

            var rootModel = _designerManagementService.GetRootResourceModel();

            // BUG 9634 - 2013.07.17 - TWR : internalized validation service
            // BUG 9940 - 2013.07.30 - TWR : internalized DataMappingViewModel
            _viewModel = new DsfActivityViewModel(ModelItem, rootModel, EnvironmentRepository.Instance);

            DataContext = _viewModel;

            var binding = new Binding("ShowMapping")
            {
                Mode = BindingMode.TwoWay,
                Source = _viewModel
            };
            SetBinding(IsAdornerOpenProperty, binding);
        }

        private void SetDesignerManagementService(IDesignerManagementService designerManagementService)
        {
            if(_designerManagementService != null)
            {
                _designerManagementService.CollapseAllRequested -= _designerManagementService_CollapseAllRequested;
                _designerManagementService.ExpandAllRequested -= _designerManagementService_ExpandAllRequested;
                _designerManagementService.RestoreAllRequested -= _designerManagementService_RestoreAllRequested;
                _designerManagementService = null;
            }

            if(designerManagementService != null)
            {
                _designerManagementService = designerManagementService;
                _designerManagementService.CollapseAllRequested += _designerManagementService_CollapseAllRequested;
                _designerManagementService.ExpandAllRequested += _designerManagementService_ExpandAllRequested;
                _designerManagementService.RestoreAllRequested += _designerManagementService_RestoreAllRequested;
            }
        }

        private void SetInitialInputOutPutMappingHeight()
        {
            if(_inputOutPutMappingHeightInitialized)
            {
                return;
            }

            //
            // Get the controls
            //
            FrameworkElement contentHost = this.FindNameAcrossNamescopes("contentHost");
            var inputMappings = this.FindNameAcrossNamescopes("inputMappings") as DataGrid;
            var outputMappings = this.FindNameAcrossNamescopes("outputMappings") as DataGrid;
            var contentPresenter = this.FindNameAcrossNamescopes("resizeContent") as ContentControl;

            if(inputMappings == null || outputMappings == null || contentPresenter == null || contentHost == null ||
                _viewModel == null)
            {
                return;
            }

            //
            // Get input row height
            //
            double inputRowHeight = 0;
            double inputHeaderHeight = 0;
            int inputRowCount = 0;
            if(_viewModel.DataMappingViewModel != null && _viewModel.DataMappingViewModel.Inputs != null &&
                _viewModel.DataMappingViewModel.Inputs.Count > 0)
            {
                var dataGridRow =
                    inputMappings.ItemContainerGenerator.ContainerFromItem(_viewModel.DataMappingViewModel.Inputs[0]) as
                    DataGridRow;

                if(dataGridRow == null)
                {
                    return;
                }

                var dataGridColumnHeadersPresenter = inputMappings
                                                         .FindNameAcrossNamescopes("PART_ColumnHeadersPresenter") as
                                                     DataGridColumnHeadersPresenter;

                if(dataGridColumnHeadersPresenter == null)
                {
                    return;
                }

                inputHeaderHeight = dataGridColumnHeadersPresenter.ActualHeight;
                inputRowHeight = dataGridRow.ActualHeight;
                inputRowCount = _viewModel.DataMappingViewModel.Inputs.Count;
            }

            //
            // Get output row height
            //
            double outputRowHeight = 0;
            double outputHeaderHeight = 0;
            int outputRowCount = 0;
            if(_viewModel.DataMappingViewModel != null && _viewModel.DataMappingViewModel.Outputs != null &&
                _viewModel.DataMappingViewModel.Outputs.Count > 0)
            {
                var dgr =
                    outputMappings.ItemContainerGenerator.ContainerFromItem(_viewModel.DataMappingViewModel.Outputs[0])
                    as
                    DataGridRow;

                if(dgr == null)
                {
                    return;
                }

                var dataGridColumnHeadersPresenter = outputMappings
                                                         .FindNameAcrossNamescopes("PART_ColumnHeadersPresenter") as
                                                     DataGridColumnHeadersPresenter;

                if(dataGridColumnHeadersPresenter == null)
                {
                    return;
                }

                outputHeaderHeight = dataGridColumnHeadersPresenter.ActualHeight;
                outputRowHeight = dgr.ActualHeight;
                outputRowCount = _viewModel.DataMappingViewModel.Outputs.Count;
            }

            //
            // Set initial height
            //
            if(outputRowCount + inputRowCount > 12)
            {
                double difference = contentHost.ActualHeight - inputMappings.ActualHeight - outputMappings.ActualHeight;
                contentPresenter.Height = difference + (outputRowHeight * 5) + (inputRowHeight * 5) + outputHeaderHeight +
                                          inputHeaderHeight;
            }

            _inputOutPutMappingHeightInitialized = true;
        }

        //2013.03.20: Ashley Lewis - Bug 9233 carefully find title bar textbox and add two events
        private void AttachEventsToTitleBox()
        {
            var deepChild = this.FindChildByToString("Border", true); // Pass the border layer
            if(deepChild != null)
            {
                deepChild = deepChild.FindChildByToString("Adorner", true); // Pass the adorner layer
                if(deepChild != null)
                {
                    deepChild = deepChild.FindChildByToString("Grid", true); // Pass the grid layer
                    if(deepChild != null)
                    {
                        deepChild = deepChild.FindChildByToString("TextBox", true);
                        if(deepChild != null)
                        {
                            (deepChild as UIElement).GotFocus += ChildOnGotFocus; // Add events to this object
                            (deepChild as UIElement).LostFocus += ChildOnLostFocus;
                        }
                    }
                }
            }
        }

        #endregion Private Methods

        #region Event Handlers

        private void OnUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            CleanUp();
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            InitializeViewModel();
            Loaded -= OnLoaded;
            DataContext = _viewModel;
            AttachEventsToTitleBox();

            if(IsItemDragged.Instance.IsDragged)
            {
                var openMappings =
                this.Descendents().FirstOrDefault(d => d.GetType() == typeof(AdornerToggleButton))
                as AdornerToggleButton;
                if(openMappings != null)
                {
                    if(openMappings.IsChecked != null)
                    {
                        if(_viewModel.DataMappingViewModel != null && (_viewModel.DataMappingViewModel.Inputs.Any() || _viewModel.DataMappingViewModel.Outputs.Any()))
                        {
                            openMappings.IsChecked = true;
                            IsAdornerOpen = true;
                        }
                    }
                }
                IsItemDragged.Instance.IsDragged = false;
            }
        }

        void SelectionChanged(Selection item)
        {
            _workflowDesignerSelection = item;

            if(_workflowDesignerSelection != null)
            {
                if(_workflowDesignerSelection.PrimarySelection == ModelItem)
                {
                    IsSelected = true;
                    BringToFront();
                    ShowAllAdorners();
                }
                else
                {
                    IsSelected = false;
                    HideAdorners();
                }
            }
        }

        private void ResizeContent_OnLoaded(object sender, RoutedEventArgs e)
        {
            SetInitialInputOutPutMappingHeight();
        }

        private void DsfActivityDesigner_OnMouseLeave(object sender, MouseEventArgs e)
        {
            HideAdorners();
        }

        void ShowAllAdorners()
        {
            BringToFront();
            ShowAdorners = true;
        }

        private void BringToFront()
        {
            try
            {
                var fElement = VisualTreeHelper.GetParent(this) as FrameworkElement;
                if(fElement != null)
                {
                    fElement.BringToFront();
                }
            }
            catch
            {
            }
        }

        void HideAdorners(bool forceHide = false)
        {
            if((!IsAdornerOpen || forceHide) && !IsSelected)
            {
                UIElement uiElement = VisualTreeHelper.GetParent(this) as UIElement;
                if(uiElement != null)
                {
                    Panel.SetZIndex(uiElement, int.MinValue);
                }

                IsAdornerOpen = false;
                ShowAdorners = false;
            }
        }

        void DsfActivityDesigner_OnMouseEnter(object sender, MouseEventArgs e)
        {
            if(e.LeftButton == MouseButtonState.Pressed)
            {
                HideAdorners(true);
            }
            else
            {
                ShowAllAdorners();
            }
        }

        private void OutputTxt_KeyUp(object sender, KeyEventArgs e)
        {
            var viewModel = DataContext as DsfActivityViewModel;
            if(viewModel != null)
            {
                viewModel.SetOuputs();
            }
        }

        private void InputTxt_KeyUp(object sender, KeyEventArgs e)
        {
            var viewModel = DataContext as DsfActivityViewModel;
            if(viewModel != null)
            {
                viewModel.SetInputs();
            }

            TextBox tb = sender as TextBox;
            if(tb != null)
            {
                IInputOutputViewModel inputOutputViewModel = tb.DataContext as IInputOutputViewModel;
                if(inputOutputViewModel != null && inputOutputViewModel.Required)
                {
                    if(!string.IsNullOrEmpty(tb.Text) && inputOutputViewModel.RequiredMissing)
                    {
                        inputOutputViewModel.RequiredMissing = false;
                    }
                    else if(string.IsNullOrEmpty(tb.Text) && !inputOutputViewModel.RequiredMissing)
                    {
                        inputOutputViewModel.RequiredMissing = true;
                    }
                }
            }           
        }

        private void _designerManagementService_RestoreAllRequested(object sender, EventArgs e)
        {
            var viewModel = DataContext as DsfActivityViewModel;
            if(viewModel == null)
            {
                return;
            }

            viewModel.ShowAdorners = viewModel.ShowAdornersPreviousValue;
        }

        private void _designerManagementService_ExpandAllRequested(object sender, EventArgs e)
        {
            var viewModel = DataContext as DsfActivityViewModel;
            if(viewModel == null)
            {
                return;
            }

            viewModel.ShowAdornersPreviousValue = viewModel.ShowAdorners;

            viewModel.ShowAdorners = true;
        }

        private void _designerManagementService_CollapseAllRequested(object sender, EventArgs e)
        {
            var viewModel = DataContext as DsfActivityViewModel;
            if(viewModel == null)
            {
                return;
            }

            viewModel.ShowAdornersPreviousValue = viewModel.ShowAdorners;
            viewModel.ShowMappingPreviousValue = viewModel.ShowMapping;

            viewModel.ShowAdorners = false;
        }

        private void UIElement_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var inputElement = sender as IInputElement;
            if(inputElement == null)
            {
                return;
            }

            Mouse.Capture(sender as IInputElement, CaptureMode.SubTree);

            if(_workflowDesignerSelection != null &&
                _workflowDesignerSelection.SelectedObjects.FirstOrDefault() != ModelItem)
            {
                Selection.SelectOnly(Context, ModelItem);
            }

            _mousedownPoint = e.GetPosition(sender as IInputElement);
            _startManualDrag = true;
            e.Handled = true;
        }

        private void UIElement_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
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

        private void UIElement_OnPreviewMouseMove(object sender, MouseEventArgs e)
        {
            var inputElement = sender as IInputElement;
            if(inputElement == null)
            {
                return;
            }

            Point tempPoint = e.GetPosition(sender as IInputElement);
            double xDelta = Math.Abs(tempPoint.X - _mousedownPoint.X);
            double yDelta = Math.Abs(tempPoint.Y - _mousedownPoint.Y);

            if(e.LeftButton == MouseButtonState.Pressed && _startManualDrag && Math.Max(xDelta, yDelta) >= 5)
            {
                DragDropHelper.DoDragMove(this, e.GetPosition(this));
                _startManualDrag = false;
                inputElement.ReleaseMouseCapture();
                Focus();
            }
        }

        private void UIElement_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _mousedownPoint = e.GetPosition(sender as IInputElement);
            _startManualDrag = true;
        }

        private void UIElement_OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _startManualDrag = false;
        }

        //2013.03.19: Ashley Lewis - Bug 9233 Hide adorners when editting title
        private void ChildOnGotFocus(object sender, RoutedEventArgs routedEventArgs)
        {
            HideAdorners();
        }

        private void ChildOnLostFocus(object sender, RoutedEventArgs e)
        {
            ShowAdorners = false;
        }

        //DONT TAKE OUT... This has been done so that the drill down doesnt happen.
        void DsfActivityDesigner_OnPreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ActivityHelper.HandleMouseDoubleClick(e);
        }

        protected bool IsSelected { get; set; }

        #endregion Event Handlers

        #region Tear Down

        public void Dispose()
        {
            CleanUp();
        }

        private void CleanUp()
        {
            Loaded -= OnLoaded;
            Unloaded -= OnUnloaded;

            if(_viewModel != null)
            {
                _viewModel.Dispose();
            }

            Context.Items.Unsubscribe<Selection>(SelectionChanged);
            Context.Services.Unsubscribe<IDesignerManagementService>(SetDesignerManagementService);

            SetDesignerManagementService(null);
            _workflowDesignerSelection = null;
            _viewModel = null;

            DataContext = null;
        }

        #endregion Tear Down

        private void DsfActivityDesigner_OnPreviewDragEnter(object sender, DragEventArgs e)
        {
            HideAdorners(true);
        }

        private void OpenMappingsToggle_OnChecked(object sender, RoutedEventArgs e)
        {
            var openMappings =
                this.Descendents().FirstOrDefault(d => d.GetType() == typeof(AdornerToggleButton))
                as AdornerToggleButton;
            if(openMappings != null)
            {
                if(openMappings.IsChecked != null)
                {
                    IsAdornerOpen = openMappings.IsChecked.Value;
                }
            }
        }

        protected override void OnPreviewDragEnter(DragEventArgs e)
        {
            ActivityHelper.HandleDragEnter(e);
        }
    }
}
