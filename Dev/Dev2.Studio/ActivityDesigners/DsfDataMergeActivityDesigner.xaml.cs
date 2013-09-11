using Dev2.Interfaces;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Core.Models.QuickVariableInput;
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
using Dev2.Util.ExtensionMethods;
using QuickVariableInputViewModel = Dev2.ViewModels.QuickVariableInput.QuickVariableInputViewModel;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    public partial class DsfDataMergeActivityDesigner : IDisposable
    {

        #region Fields

        ModelItem _activity;
        dynamic _resultsCollection;
        Point _mousedownPoint = new Point(0, 0);
        bool _startManualDrag;
        Selection _workflowDesignerSelection;

        #endregion

        #region Ctor

        public DsfDataMergeActivityDesigner()
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

        public bool IsAdornerOpen
        {
            get { return (bool)GetValue(IsAdornerOpenProperty); }
            set { SetValue(IsAdornerOpenProperty, value); }
        }

        public static readonly DependencyProperty IsAdornerOpenProperty =
            DependencyProperty.Register("IsAdornerOpen", typeof(bool), typeof(DsfDataMergeActivityDesigner),
            new PropertyMetadata(false));


        // Using a DependencyProperty as the backing store for ShowAdorners.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ShowAdornersProperty =
            DependencyProperty.Register("ShowAdorners", typeof(bool), typeof(DsfDataMergeActivityDesigner), new PropertyMetadata(false));

        public bool ShowQuickVariableInput
        {
            get { return (bool)GetValue(ShowQuickVariableInputProperty); }
            set { SetValue(ShowQuickVariableInputProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ShowQuickVariableInput.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ShowQuickVariableInputProperty =
            DependencyProperty.Register("ShowQuickVariableInput", typeof(bool), typeof(DsfDataMergeActivityDesigner), new PropertyMetadata(false));

        public bool ShowOtherRightClickOptions
        {
            get { return (bool)GetValue(ShowRightClickOptionsProperty); }
            set { SetValue(ShowRightClickOptionsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ShowRightClickOptions.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ShowRightClickOptionsProperty =
            DependencyProperty.Register("ShowOtherRightClickOptions", typeof(bool), typeof(DsfDataMergeActivityDesigner), new UIPropertyMetadata(false));

        #endregion Dependancy Properties

        protected override void OnModelItemChanged(object newItem)
        {
            base.OnModelItemChanged(newItem);
            Context.Items.Subscribe<Selection>(SelectionChanged);
            _resultsCollection = newItem;
            _activity = newItem as ModelItem;

            if (_resultsCollection.MergeCollection == null || _resultsCollection.MergeCollection.Count <= 0)
            {
                _resultsCollection.MergeCollection.Add(new DataMergeDTO("", "None", "", 1, "", "Left"));
                _resultsCollection.MergeCollection.Add(new DataMergeDTO("", "None", "", 2, "", "Left"));
            }

            if (_activity == null) return;
            var parent = _activity.Parent;

            while (parent != null)
            {
                if (parent.Properties["Argument"] != null)
                {
                    break;
                }

                parent = parent.Parent;
            }

            var activity = ModelItem.GetCurrentValue() as ICollectionActivity;

            var model = new QuickVariableInputModel(ModelItem, activity);

            ViewModel = new QuickVariableInputViewModel(model);
        }

        #region Dispose

        public void Dispose()
        {
            CleanUp();
        }

        #endregion

        #region Private Methods

        string createDisplayName(string currentName)
        {
            // 6279, CODE REVIEW, Null check needed
            var modelProperty = _activity.Properties["DisplayName"];
            if (modelProperty != null)
                currentName = modelProperty.ComputedValue as string;
            if (currentName != null && (currentName.Contains("(") && currentName.Contains(")")))
            {
                if (currentName.Contains(" ("))
                {
                    currentName = currentName.Remove(currentName.IndexOf(" (", StringComparison.Ordinal));
                }
                else
                {
                    currentName = currentName.Remove(currentName.IndexOf("(", StringComparison.Ordinal));
                }
            }
            currentName = currentName + " (" + (_resultsCollection.MergeCollection.Count - 1) + ")";
            return currentName;
        }

        #endregion

        #region Event Handlers

        //DONT TAKE OUT... This has been done so that the drill down doesnt happen.
        void DsfDataMergeActivityDesigner_OnPreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void SetValuetxt_KeyUp(object sender, KeyEventArgs e)
        {
            Resultsdg.AddRow();
            var modelProperty = ModelItem.Properties["DisplayName"];
            if (modelProperty != null)
            {
                var disName = createDisplayName(modelProperty.ComputedValue as string);
                modelProperty.SetValue(disName);
            }
        }

        void CbxLoad(object sender, RoutedEventArgs e)
        {
            var cbx = sender as ComboBox;
            if (cbx != null)
            {
                if (cbx.Items.Count == 0)
                {
                    cbx.Items.Add("None");
                    cbx.Items.Add("Index");
                    cbx.Items.Add("Chars");
                    cbx.Items.Add("New Line");
                    cbx.Items.Add("Tab");
                }
            }
        }

        void Resultsdg_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var tmpCbx = sender as ComboBox;
                if (tmpCbx != null)
                {
                    var model = (ModelItem)tmpCbx.DataContext;

                    if (tmpCbx.SelectedItem.ToString() == "Index" || tmpCbx.SelectedItem.ToString() == "Chars")
                    {
                        ModelItemUtils.SetProperty("EnableAt", true, model);
                    }
                    else
                    {
                        ModelItemUtils.SetProperty("At", string.Empty, model);
                        ModelItemUtils.SetProperty("EnableAt", false, model);
                    }
                }
            }
            catch (Exception)
            {
                // 6279, CODE REVIEW, EMPTY EXCEPTION WITH NO COMMENT, EVIL !!!!!!!!!!!!!!!
            }
        }

        void DeleteRow_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Resultsdg.RemoveRow(Resultsdg.SelectedIndex);
            var modelProperty = _activity.Properties["DisplayName"];
            if (modelProperty != null)
                modelProperty.SetValue(createDisplayName(modelProperty.ComputedValue as string));
        }

        void InsertRow_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Resultsdg.InsertRow(Resultsdg.SelectedIndex);
            var modelProperty = _activity.Properties["DisplayName"];
            if (modelProperty != null)
                modelProperty.SetValue(createDisplayName(modelProperty.ComputedValue as string));
        }

        void ContextMenu_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (e.Source.GetType() == typeof(Dev2DataGrid))
            {
                ShowOtherRightClickOptions = true;
            }
            else
            {
                ShowOtherRightClickOptions = false;
            }
        }

        void QuickVariableInputControl_OnLoaded(object sender, RoutedEventArgs e)
        {
            ViewModel.CloseAdornersRequested += delegate
            {
                ShowQuickVariableInput = false;
            };
        }

        void UIElement_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
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

        void UIElement_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var inputElement = sender as IInputElement;
            if (inputElement == null)
            {
                return;
            }

            inputElement.ReleaseMouseCapture();
            Focus();
            BringToFront();
        }


        void UIElement_OnPreviewMouseMove(object sender, MouseEventArgs e)
        {
            var inputElement = sender as IInputElement;
            if (inputElement == null)
            {
                return;
            }

            var tempPoint = e.GetPosition(sender as IInputElement);
            var xDelta = Math.Abs(tempPoint.X - _mousedownPoint.X);
            var yDelta = Math.Abs(tempPoint.Y - _mousedownPoint.Y);

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


        void DsfDataMergeActivityDesigner_OnMouseEnter(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                HideAdorners(true);
            }
            else
            {
                ShowAllAdorners();
        }
        }

        void DsfDataMergeActivityDesigner_OnMouseLeave(object sender, MouseEventArgs e)
            {
            HideAdorners();
            }

        void ShowAllAdorners()
        {
            ShowAdorners = true;
        }

        private void BringToFront()
        {
            try
            {
                var fElement = VisualTreeHelper.GetParent(this) as FrameworkElement;
                if (fElement != null)
                {
                    fElement.BringToFront();
                }
            }catch{}
        }

        void HideAdorners(bool forceHide = false)
        {
            if ((!IsAdornerOpen || forceHide) && !IsSelected)
            {
                var uiElement = VisualTreeHelper.GetParent(this) as UIElement;
                if (uiElement != null)
                {
                    Panel.SetZIndex(uiElement, int.MinValue);
                }

                ShowAdorners = false;
                IsAdornerOpen = false;
            }
            }

        protected bool IsSelected { get; set; }

        void SelectionChanged(Selection item)
        {
            _workflowDesignerSelection = item;

            if (_workflowDesignerSelection != null)
            {
                if (_workflowDesignerSelection.PrimarySelection == ModelItem)
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

        #endregion

        #region Clean Up

        void CleanUp()
        {
            Context.Items.Unsubscribe<Selection>(SelectionChanged);
        }

        #endregion        
       
        private void DsfDataMergeActivityDesigner_OnPreviewDragEnter(object sender, DragEventArgs e)
        {
            HideAdorners(true);
        }
       
    }
}
