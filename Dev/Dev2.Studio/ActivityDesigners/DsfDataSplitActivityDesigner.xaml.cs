using System.Windows.Controls.Primitives;
using Dev2.Activities.Designers2.Core.Controls;
using Dev2.Interfaces;
using Dev2.Studio.Core;
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
using Dev2.Utilities;
using Dev2.ViewModels.QuickVariableInput;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    public partial class DsfDataSplitActivityDesigner : IDisposable
    {

        #region Fields

        ModelItem activity;
        dynamic _resultsCollection;
        Point _mousedownPoint = new Point(0, 0);
        bool _startManualDrag;
        Selection _workflowDesignerSelection;

        #endregion

        #region Ctor

        public DsfDataSplitActivityDesigner()
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
            DependencyProperty.Register("IsAdornerOpen", typeof(bool), typeof(DsfDataSplitActivityDesigner),
            new PropertyMetadata(false));


        // Using a DependencyProperty as the backing store for ShowAdorners.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ShowAdornersProperty =
            DependencyProperty.Register("ShowAdorners", typeof(bool), typeof(DsfDataSplitActivityDesigner), new PropertyMetadata(false));

        public bool ShowQuickVariableInput
        {
            get { return (bool)GetValue(ShowQuickVariableInputProperty); }
            set { SetValue(ShowQuickVariableInputProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ShowQuickVariableInput.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ShowQuickVariableInputProperty =
            DependencyProperty.Register("ShowQuickVariableInput", typeof(bool), typeof(DsfDataSplitActivityDesigner), new PropertyMetadata(false));

        public bool ShowRightClickOptions
        {
            get { return (bool)GetValue(ShowRightClickOptionsProperty); }
            set { SetValue(ShowRightClickOptionsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ShowRightClickOptions.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ShowRightClickOptionsProperty =
            DependencyProperty.Register("ShowRightClickOptions", typeof(bool), typeof(DsfDataSplitActivityDesigner), new UIPropertyMetadata(false));

        #endregion Dependancy Properties

        protected override void OnModelItemChanged(object newItem)
        {
            base.OnModelItemChanged(newItem);
            Context.Items.Subscribe<Selection>(SelectionChanged);
            _resultsCollection = newItem;
            activity = newItem as ModelItem;

            if (_resultsCollection.ResultsCollection == null || _resultsCollection.ResultsCollection.Count <= 0)
            {
                var test = _resultsCollection.ResultsCollection;
                _resultsCollection.ResultsCollection.Add(new DataSplitDTO("", "Index", "", 1));
                _resultsCollection.ResultsCollection.Add(new DataSplitDTO("", "Index", "", 2));
            }
            //string disName = createDisplayName(activity.Properties["DisplayName"].ComputedValue as string);
            string disName = activity.Properties["DisplayName"].ComputedValue as string;
            activity.Properties["DisplayName"].SetValue(disName);

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
        }

        #endregion

        #region Private Methods

        string createDisplayName(string currentName)
        {
            currentName = activity.Properties["DisplayName"].ComputedValue as string;
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
            currentName = currentName + " (" + (_resultsCollection.ResultsCollection.Count - 1) + ")";
            return currentName;
        }

        #endregion

        #region Event Handlers

        //DONT TAKE OUT... This has been done so that the drill down doesnt happen.
        void DsfDataSplitActivityDesigner_OnPreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ActivityHelper.HandleMouseDoubleClick(e);
        }

        protected override void OnPreviewDragEnter(DragEventArgs e)
        {
            ActivityHelper.HandleDragEnter(e);
        }

        void SetValuetxt_KeyUp(object sender, KeyEventArgs e)
        {
            Resultsdg.AddRow();

            string disName = createDisplayName(ModelItem.Properties["DisplayName"].ComputedValue as string);
            ModelItem.Properties["DisplayName"].SetValue(disName);

            setName();
        }

        void CbxLoad(object sender, RoutedEventArgs e)
        {
            ComboBox cbx = sender as ComboBox;
            
            if (cbx != null)
            {
                dynamic temp = cbx.DataContext;
                if (temp != null)
                {
                    string selectedVal = temp.SplitType;

                    if(cbx.Items.Count == 0)
                    {
                        cbx.Items.Add("Index");
                        cbx.Items.Add("Chars");
                        cbx.Items.Add("New Line");
                        cbx.Items.Add("Space");
                        cbx.Items.Add("Tab");
                        cbx.Items.Add("End");
                    }
                    cbx.SelectedValue = selectedVal;    
                }
            }
        }

        void setName()
        {
            string disName = createDisplayName(ModelItem.Properties["DisplayName"].ComputedValue as string);
            ModelItem.Properties["DisplayName"].SetValue(disName);
        }

        void Resultsdg_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var tmpCbx = sender as ComboBox;
                dynamic tmpDTO = tmpCbx.DataContext;
                if (tmpCbx.SelectedItem.ToString() == "Index" || tmpCbx.SelectedItem.ToString() == "Chars")
                {
                    tmpDTO.EnableAt = true;
                }
                else
                {
                    tmpDTO.At = string.Empty;
                    tmpDTO.EnableAt = false;
                }
                //setName();
            }
            catch (Exception)
            {

            }
        }

        void DeleteRow_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Resultsdg.RemoveRow(Resultsdg.SelectedIndex);
            setName();
        }

        void InsertRow_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Resultsdg.InsertRow(Resultsdg.SelectedIndex);
            setName();
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

        void doCustomScroll(object sender, MouseWheelEventArgs e)
        {
            Dev2DataGrid theGrid = Resultsdg;
            if (e.Delta > 0)
            {
                if (theGrid.SelectedIndex > 1)
                {
                    theGrid.SelectedIndex -= 2;
                }
                else if (theGrid.SelectedIndex > 0)
                {
                    theGrid.SelectedIndex -= 1;
                }
            }
            else if (e.Delta < 0)
            {
                if (theGrid.SelectedIndex == theGrid.Items.Count - 2)
                {
                    theGrid.SelectedIndex++;
                }
                else
                {
                    theGrid.SelectedIndex += 2;
                }
            }
            else
            {
                // Mouse was not scrolled
                return;
            }
            if (theGrid.SelectedIndex < 0) return;
            theGrid.UpdateLayout();
            theGrid.ScrollIntoView(theGrid.CurrentItem);
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
            IInputElement inputElement = sender as IInputElement;
            if (inputElement == null)
            {
                return;
            }

            inputElement.ReleaseMouseCapture();
            Focus();
            BringToFront();
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
            BringToFront();
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

        void DsfDataSplitActivityDesigner_OnMouseEnter(object sender, MouseEventArgs e)
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

        void DsfDataSplitActivityDesigner_OnMouseLeave(object sender, MouseEventArgs e)
            {
            HideAdorners();
            }

        void ShowAllAdorners()
        {
            //BringToFront();
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
            }
            catch
            {
            }
        }

        void HideAdorners(bool forceHide = false)
        {
            if ((!IsAdornerOpen || forceHide) && !IsSelected)
            {
                UIElement uiElement = VisualTreeHelper.GetParent(this) as UIElement;
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
            activity = null;
            DataContext = null;
            ViewModel = null;
        }

        #endregion        

        private void DsfDataSplitActivityDesigner_OnPreviewDragEnter(object sender, DragEventArgs e)
        {
            HideAdorners(true);
         }
    }
}
