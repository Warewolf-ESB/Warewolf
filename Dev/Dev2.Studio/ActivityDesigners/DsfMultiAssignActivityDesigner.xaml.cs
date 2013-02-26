using Dev2.Interfaces;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Models.QuickVariableInput;
using Dev2.Studio.ViewModels.QuickVariableInput;
using Dev2.UI;
using System;
using System.Activities.Presentation;
using System.Activities.Presentation.Model;
using System.Activities.Presentation.View;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    public partial class DsfMultiAssignActivityDesigner : IDisposable
    {

        #region Fields

        bool _isRegistered = false;
        string mediatorKey = string.Empty;
        dynamic _fieldsCollection;
        double _initialHeight;
        Point _mousedownPoint = new Point(0, 0);
        bool _startManualDrag;
        Selection _workflowDesignerSelection;

        #endregion

        #region Properties

        public QuickVariableInputViewModel ViewModel { get; set; }

        public bool ShowAdorners
        {
            get { return (bool)GetValue(ShowAdornersProperty); }
            set { SetValue(ShowAdornersProperty, value); }
        }

        #endregion

        #region Ctor

        public DsfMultiAssignActivityDesigner()
        {
            InitializeComponent();
        }

        #endregion

        #region Dependancy Properties

        // Using a DependencyProperty as the backing store for ShowAdorners.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ShowAdornersProperty =
            DependencyProperty.Register("ShowAdorners", typeof(bool), typeof(DsfMultiAssignActivityDesigner), new PropertyMetadata(false));

        public bool ShowQuickVariableInput
        {
            get { return (bool)GetValue(ShowQuickVariableInputProperty); }
            set { SetValue(ShowQuickVariableInputProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ShowQuickVariableInput.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ShowQuickVariableInputProperty =
            DependencyProperty.Register("ShowQuickVariableInput", typeof(bool), typeof(DsfMultiAssignActivityDesigner), new PropertyMetadata(false));

        public bool ShowRightClickOptions
        {
            get { return (bool)GetValue(ShowRightClickOptionsProperty); }
            set { SetValue(ShowRightClickOptionsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ShowRightClickOptions.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ShowRightClickOptionsProperty =
            DependencyProperty.Register("ShowRightClickOptions", typeof(bool), typeof(DsfMultiAssignActivityDesigner), new UIPropertyMetadata(false));

        #endregion Dependancy Properties

        protected override void OnModelItemChanged(object newItem)
        {
            base.OnModelItemChanged(newItem);
            Context.Items.Subscribe<Selection>(SelectionChanged);
            if (!_isRegistered)
            {
                //mediatorKey = Mediator.RegisterToReceiveMessage(MediatorMessages.DataListItemSelected, input => Highlight(input as IDataListItemViewModel));
            }
            _fieldsCollection = newItem;
            if (_fieldsCollection.FieldsCollection == null || _fieldsCollection.FieldsCollection.Count <= 0)
            {
                _fieldsCollection.FieldsCollection.Add(new ActivityDTO("", "", 1));
                _fieldsCollection.FieldsCollection.Add(new ActivityDTO("", "", 2));
            }
            string disName = ModelItem.Properties["DisplayName"].ComputedValue as string;
            ModelItem.Properties["DisplayName"].SetValue(disName);


            ModelItem parent = ModelItem.Parent;

            while (parent != null)
            {
                if (parent.Properties["Argument"] != null)
                {
                    break;
                }

                parent = parent.Parent;
            }

            ICollectionActivity activity = ModelItem.GetCurrentValue() as ICollectionActivity;

            QuickVariableInputModel model = new QuickVariableInputModel(ModelItem, activity);

            ViewModel = new QuickVariableInputViewModel(model);
            //setName();
        }

        /*
        private void Highlight(IDataListItemViewModel dataListItemViewModel)
        {
            Dev2.UI.IntellisenseTextBox bob = new Dev2.UI.IntellisenseTextBox();
            var test = FieldsDataGrid.ItemsSource;
            //ObservableCollection<string> containingFields = new ObservableCollection<string>();
            //border.Visibility = Visibility.Hidden;

            //SetValuetxt.BorderBrush = Brushes.LightGray;
            //SetValuetxt.BorderThickness = new Thickness(1.0);
            //ToValuetxt.BorderBrush = Brushes.LightGray;
            //ToValuetxt.BorderThickness = new Thickness(1.0);

            //containingFields = DsfActivityDataListComparer.ContainsDataListItem(ModelItem, dataListItemViewModel);

            //if (containingFields.Count > 0) {
            //    foreach (string item in containingFields) {
            //        if (item.Equals("FieldName")) {
            //            SetValuetxt.BorderBrush = System.Windows.Media.Brushes.DeepSkyBlue;
            //            SetValuetxt.BorderThickness = new Thickness(2.0);
            //        }
            //        else if (item.Equals("FieldValue")) {
            //            ToValuetxt.BorderBrush = System.Windows.Media.Brushes.DeepSkyBlue;
            //            ToValuetxt.BorderThickness = new Thickness(2.0);
            //        }
            //        var bob = this.BorderBrush;


            //    }
            //}
        }
        */

        #region Dispose

        public void Dispose()
        {
            CleanUp();
            Mediator.DeRegister(MediatorMessages.DataListItemSelected, mediatorKey);
        }

        #endregion

        #region Private Methods

        string createDisplayName(string currentName)
        {
            currentName = ModelItem.Properties["DisplayName"].ComputedValue as string;
            if (currentName.Contains("(") && currentName.Contains(")"))
            {
                if (currentName.Contains(" ("))
                {
                    currentName = currentName.Remove(currentName.IndexOf(" ("));
                }
                else
                {
                    currentName = currentName.Remove(currentName.IndexOf("("));
                }
            }
            currentName = currentName + " (" + (_fieldsCollection.FieldsCollection.Count - 1) + ")";
            return currentName;
        }

        void setName()
        {
            string disName = createDisplayName(ModelItem.Properties["DisplayName"].ComputedValue as string);
            ModelItem.Properties["DisplayName"].SetValue(disName);
        }

        #endregion

        #region Event Handlers

        private void FieldsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FieldsDataGrid.RemoveRow();

            setName();
        }

        void SetValuetxt_KeyUp(object sender, KeyEventArgs e)
        {

            if (_initialHeight == 0.0)
            {
                _initialHeight = ActualHeight;
            }

            FieldsDataGrid.AddRow();

            setName();

        }

        void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            FieldsDataGrid.RemoveRow(FieldsDataGrid.SelectedIndex);
            setName();
        }

        void ActivityDesigner_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (e.Source.GetType() == typeof(Dev2DataGrid))
            {
                ShowRightClickOptions = true;
            }
            else
            {
                ShowRightClickOptions = false;
            }
        }

        void QuickVariableInputControl_OnLoaded(object sender, RoutedEventArgs e)
        {
            ViewModel.CloseAdornersRequested += delegate
            {
                ShowQuickVariableInput = false;
            };
        }

        void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            ShowQuickVariableInput = !ShowQuickVariableInput;
        }

        void UIElement_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            IInputElement inputElement = sender as IInputElement;
            if (inputElement == null)
            {
                return;
            }

            Mouse.Capture(sender as IInputElement, CaptureMode.SubTree);

            if (_workflowDesignerSelection != null && _workflowDesignerSelection.SelectedObjects.FirstOrDefault() != ModelItem)
            {
                Selection.SelectOnly(Context, ModelItem);
            }

            _mousedownPoint = e.GetPosition(sender as IInputElement);
            _startManualDrag = true;
            e.Handled = true;
        }

        void UIElement_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            IInputElement inputElement = sender as IInputElement;
            if (inputElement == null)
            {
                return;
            }

            inputElement.ReleaseMouseCapture();
            Focus();
        }

        void UIElement_OnPreviewMouseMove(object sender, MouseEventArgs e)
        {
            IInputElement inputElement = sender as IInputElement;
            if (inputElement == null)
            {
                return;
            }

            Point tempPoint = e.GetPosition(sender as IInputElement);
            double xDelta = Math.Abs(tempPoint.X - _mousedownPoint.X);
            double yDelta = Math.Abs(tempPoint.Y - _mousedownPoint.Y);

            if (e.LeftButton == MouseButtonState.Pressed && _startManualDrag && Math.Max(xDelta, yDelta) >= 5)
            {
                DragDropHelper.DoDragMove(this, e.GetPosition(this));
                _startManualDrag = false;
                inputElement.ReleaseMouseCapture();
                Focus();
            }
        }

        void UIElement_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _mousedownPoint = e.GetPosition(sender as IInputElement);
            _startManualDrag = true;
        }

        void UIElement_OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _startManualDrag = false;
        }

        void ShowAllAdorners()
        {
            UIElement uiElement = VisualTreeHelper.GetParent(this) as UIElement;
            if (uiElement != null)
            {
                Panel.SetZIndex(uiElement, int.MaxValue);
            }

            ShowAdorners = true;
        }

        void HideAdorners()
        {
            UIElement uiElement = VisualTreeHelper.GetParent(this) as UIElement;
            if (uiElement != null)
            {
                Panel.SetZIndex(uiElement, int.MinValue);
            }

            ShowAdorners = false;
        }

        void DsfMultiAssignActivityDesigner_OnMouseEnter(object sender, MouseEventArgs e)
        {
            ShowAllAdorners();
        }

        void DsfMultiAssignActivityDesigner_OnMouseLeave(object sender, MouseEventArgs e)
        {
            if (_workflowDesignerSelection != null && _workflowDesignerSelection.SelectedObjects.FirstOrDefault() == ModelItem)
            {
                UIElement uiElement = VisualTreeHelper.GetParent(this) as UIElement;
                if (uiElement != null)
                {
                    Panel.SetZIndex(uiElement, int.MaxValue - 1);
                }

                return;
            }

            HideAdorners();
        }
        void SelectionChanged(Selection item)
        {
            _workflowDesignerSelection = item;

            if (_workflowDesignerSelection != null)
            {
                if (_workflowDesignerSelection.PrimarySelection == ModelItem)
                {
                    ShowAllAdorners();
                }
                else
                {
                    HideAdorners();
                }
            }
        }

        #endregion

        #region Clean Up

        void CleanUp()
        {
            Context.Items.Unsubscribe<Selection>(SelectionChanged);
        }

        #endregion
    }
}

