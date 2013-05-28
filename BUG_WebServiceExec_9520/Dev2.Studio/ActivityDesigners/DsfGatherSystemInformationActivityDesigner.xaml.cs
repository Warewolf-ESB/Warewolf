using System;
using System.Activities.Presentation;
using System.Activities.Presentation.Model;
using System.Activities.Presentation.View;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Dev2;
using Dev2.Common;
using Dev2.Converters;
using Dev2.Data.Enums;
using Dev2.Interfaces;
using Dev2.Studio.Core.AppResources.Converters;
using Dev2.Studio.Core.Models.QuickVariableInput;
using Dev2.Studio.ViewModels.QuickVariableInput;
using Dev2.UI;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    /// <summary>
    /// Interaction logic for DsfGatherSystemInformationActivityDesinger.xaml
    /// </summary>
    public partial class DsfGatherSystemInformationActivityDesigner : IDisposable
    {
        #region Fields

        bool _isRegistered = false;        
        ModelItem activity;
        dynamic _dataCollection;
        Point _mousedownPoint = new Point(0, 0);
        bool _startManualDrag;
        Selection _workflowDesignerSelection;

        #endregion

        #region Ctor

        public DsfGatherSystemInformationActivityDesigner()
        {
            InitializeComponent();
        }

        #endregion

        #region Properties

        public QuickVariableInputViewModel ViewModel { get; set; }

        public bool ShowAdorners
        {
            get { return (bool)GetValue(ShowAdornersProperty); }
            set { SetValue(ShowAdornersProperty, value); }
        }

        #endregion

        #region Dependancy Properties

        // Using a DependencyProperty as the backing store for ShowAdorners.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ShowAdornersProperty =
            DependencyProperty.Register("ShowAdorners", typeof(bool), typeof(DsfGatherSystemInformationActivityDesigner), new PropertyMetadata(false));



        public bool ShowQuickVariableInput
        {
            get { return (bool)GetValue(ShowQuickVariableInputProperty); }
            set { SetValue(ShowQuickVariableInputProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ShowQuickVariableInput.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ShowQuickVariableInputProperty =
            DependencyProperty.Register("ShowQuickVariableInput", typeof(bool), typeof(DsfGatherSystemInformationActivityDesigner), new PropertyMetadata(false));

        public bool ShowRightClickOptions
        {
            get { return (bool)GetValue(ShowRightClickOptionsProperty); }
            set { SetValue(ShowRightClickOptionsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ShowRightClickOptions.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ShowRightClickOptionsProperty =
            DependencyProperty.Register("ShowRightClickOptions", typeof(bool), typeof(DsfGatherSystemInformationActivityDesigner), new UIPropertyMetadata(false));

        #endregion Dependancy Properties

        protected override void OnModelItemChanged(object newItem)
        {
            base.OnModelItemChanged(newItem);

            Context.Items.Subscribe<Selection>(SelectionChanged);

            if (!_isRegistered)
            {
                //mediatorKey = Mediator.RegisterToReceiveMessage(MediatorMessages.DataListItemSelected, input => Highlight(input as IDataListItemModel));
            }
            _dataCollection = newItem;
            activity = newItem as ModelItem;

            if (_dataCollection.SystemInformationCollection == null || _dataCollection.SystemInformationCollection.Count <= 0)
            {
                _dataCollection.SystemInformationCollection.Add(new GatherSystemInformationTO(enTypeOfSystemInformationToGather.FullDateTime, string.Empty, 1));
                _dataCollection.SystemInformationCollection.Add(new GatherSystemInformationTO(enTypeOfSystemInformationToGather.FullDateTime, string.Empty, 2));
            }
            activity.Properties["DisplayName"].SetValue(createDisplayName());

            ModelItem parent = activity.Parent;

            while (parent != null)
            {
                if (parent.Properties["Argument"] != null)
                {
                    break;
                }

                parent = parent.Parent;
            }

            ICollectionActivity modelItemActivity = ModelItem.GetCurrentValue() as ICollectionActivity;

            QuickVariableInputModel model = new QuickVariableInputModel(ModelItem, modelItemActivity);

            ViewModel = new QuickVariableInputViewModel(model);


        }

        #region Dispose

        public void Dispose()
        {
            CleanUp();
            //Mediator.DeRegister(MediatorMessages.DataListItemSelected, mediatorKey);
        }

        #endregion

        #region Private Methods

        string createDisplayName()
        {
            string currentName = activity.Properties["DisplayName"].ComputedValue as string;
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
            currentName = currentName + " (" + (_dataCollection.SystemInformationCollection.Count - 1) + ")";
            return currentName;
        }

        #endregion

        #region Event Handlers

        private void SetValuetxt_KeyUp(object sender, KeyEventArgs e)
        {
            List<GatherSystemInformationTO> collection = ModelItem.Properties["SystemInformationCollection"].ComputedValue as List<GatherSystemInformationTO>;
            if (collection != null)
            {
                int result = -1;
                GatherSystemInformationTO lastItem = collection.LastOrDefault(c => c.Result != string.Empty);
                if (lastItem != null)
                {
                    result = collection.IndexOf(lastItem) + 2;

                    if (result > -1)
                    {
                        while (collection.Count > result)
                        {
                            Resultsdg.RemoveRow(collection.Count - 1);
                        }
                    }
                }
            }

            Resultsdg.AddRow();
            ModelItem.Properties["DisplayName"].SetValue(createDisplayName());
        }

        void CbxLoad(object sender, RoutedEventArgs e)
        {
            ComboBox cbx = sender as ComboBox;            
            if (cbx != null)
            {
                if (cbx.Items.Count == 0)
                {
                    cbx.ItemsSource = Dev2EnumConverter.ConvertEnumsTypeToStringList<enTypeOfSystemInformationToGather>();
                }
            }
        }

        void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Resultsdg.RemoveRow(Resultsdg.SelectedIndex);
            ModelItem.Properties["DisplayName"].SetValue(createDisplayName());
        }

        void ContextMenu_ContextMenuOpening(object sender, ContextMenuEventArgs e)
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

        void GatherSystemInformationActivityDesinger_OnMouseEnter(object sender, MouseEventArgs e)
        {
            ShowAllAdorners();
        }

        void GatherSystemInformationActivityDesinger_OnMouseLeave(object sender, MouseEventArgs e)
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
