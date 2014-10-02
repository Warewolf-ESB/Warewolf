
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.Interfaces;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Models.QuickVariableInput;
using Dev2.Studio.CustomControls;
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
using Dev2.Util.ExtensionMethods;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    public partial class DsfMultiAssignActivityDesigner
    {

        //#region Fields

        //bool _isRegistered = false;
        //string mediatorKey = string.Empty;
        //dynamic _fieldsCollection;
        //double _initialHeight;
        //Point _mousedownPoint = new Point(0, 0);
        //bool _startManualDrag;
        //Selection _workflowDesignerSelection;

        //#endregion

        //#region Properties

        //public QuickVariableInputViewModel ViewModel { get; set; }

        //public bool ShowAdorners
        //{
        //    get { return (bool)GetValue(ShowAdornersProperty); }
        //    set { SetValue(ShowAdornersProperty, value); }
        //}

        //#endregion

        //#region Ctor

        //public DsfMultiAssignActivityDesigner()
        //{
        //    InitializeComponent();
        //}

        //#endregion

        //#region Dependancy Properties


        //public bool IsAdornerOpen
        //{
        //    get { return (bool)GetValue(IsAdornerOpenProperty); }
        //    set
        //    {
        //        SetValue(IsAdornerOpenProperty, value);
        //    }
        //}

        //public static readonly DependencyProperty IsAdornerOpenProperty =
        //    DependencyProperty.Register("IsAdornerOpen", typeof(bool), typeof(DsfMultiAssignActivityDesigner), 
        //    new PropertyMetadata(false));


        //// Using a DependencyProperty as the backing store for ShowAdorners.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty ShowAdornersProperty =
        //    DependencyProperty.Register("ShowAdorners", typeof(bool), typeof(DsfMultiAssignActivityDesigner), new PropertyMetadata(false));

        //public bool ShowQuickVariableInput
        //{
        //    get { return (bool)GetValue(ShowQuickVariableInputProperty); }
        //    set { SetValue(ShowQuickVariableInputProperty, value); }
        //}

        //// Using a DependencyProperty as the backing store for ShowQuickVariableInput.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty ShowQuickVariableInputProperty =
        //    DependencyProperty.Register("ShowQuickVariableInput", typeof(bool), typeof(DsfMultiAssignActivityDesigner), new PropertyMetadata(false));

        //public bool ShowRightClickOptions
        //{
        //    get { return (bool)GetValue(ShowRightClickOptionsProperty); }
        //    set { SetValue(ShowRightClickOptionsProperty, value); }
        //}

        //// Using a DependencyProperty as the backing store for ShowRightClickOptions.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty ShowRightClickOptionsProperty =
        //    DependencyProperty.Register("ShowRightClickOptions", typeof(bool), typeof(DsfMultiAssignActivityDesigner), new UIPropertyMetadata(false));

        //#endregion Dependancy Properties

        //protected override void OnModelItemChanged(object newItem)
        //{
        //    base.OnModelItemChanged(newItem);
        //    Context.Items.Subscribe<Selection>(SelectionChanged);
        //    if (!_isRegistered)
        //    {
        //        //mediatorKey = Mediator.RegisterToReceiveMessage(MediatorMessages.DataListItemSelected, input => Highlight(input as IDataListItemViewModel));
        //    }
        //    _fieldsCollection = newItem;
        //    if (_fieldsCollection.FieldsCollection == null || _fieldsCollection.FieldsCollection.Count <= 0)
        //    {
        //        _fieldsCollection.FieldsCollection.Add(new ActivityDTO("", "", 1));
        //        _fieldsCollection.FieldsCollection.Add(new ActivityDTO("", "", 2));
        //    }
        //    string disName = ModelItem.Properties["DisplayName"].ComputedValue as string;
        //    ModelItem.Properties["DisplayName"].SetValue(disName);


        //    ModelItem parent = ModelItem.Parent;

        //    while (parent != null)
        //    {
        //        if (parent.Properties["Argument"] != null)
        //        {
        //            break;
        //        }

        //        parent = parent.Parent;
        //    }

        //    ICollectionActivity activity = ModelItem.GetCurrentValue() as ICollectionActivity;

        //    QuickVariableInputModel model = new QuickVariableInputModel(ModelItem, activity);

        //    ViewModel = new QuickVariableInputViewModel(model);
        //    //setName();
        //}

        ///*
        //private void Highlight(IDataListItemViewModel dataListItemViewModel)
        //{
        //    Dev2.UI.IntellisenseTextBox bob = new Dev2.UI.IntellisenseTextBox();
        //    var test = FieldsDataGrid.ItemsSource;
        //    //ObservableCollection<string> containingFields = new ObservableCollection<string>();
        //    //border.Visibility = Visibility.Hidden;

        //    //SetValuetxt.BorderBrush = Brushes.LightGray;
        //    //SetValuetxt.BorderThickness = new Thickness(1.0);
        //    //ToValuetxt.BorderBrush = Brushes.LightGray;
        //    //ToValuetxt.BorderThickness = new Thickness(1.0);

        //    //containingFields = DsfActivityDataListComparer.ContainsDataListItem(ModelItem, dataListItemViewModel);

        //    //if (containingFields.Count > 0) {
        //    //    foreach (string item in containingFields) {
        //    //        if (item.Equals("FieldName")) {
        //    //            SetValuetxt.BorderBrush = System.Windows.Media.Brushes.DeepSkyBlue;
        //    //            SetValuetxt.BorderThickness = new Thickness(2.0);
        //    //        }
        //    //        else if (item.Equals("FieldValue")) {
        //    //            ToValuetxt.BorderBrush = System.Windows.Media.Brushes.DeepSkyBlue;
        //    //            ToValuetxt.BorderThickness = new Thickness(2.0);
        //    //        }
        //    //        var bob = this.BorderBrush;


        //    //    }
        //    //}
        //}
        //*/

        //#region Dispose

        //public void Dispose()
        //{
        //    CleanUp();
            
        //}

        //#endregion

        //#region Private Methods

        //string createDisplayName(string currentName)
        //{
        //    currentName = ModelItem.Properties["DisplayName"].ComputedValue as string;
        //    if (currentName.Contains("(") && currentName.Contains(")"))
        //    {
        //        if (currentName.Contains(" ("))
        //        {
        //            currentName = currentName.Remove(currentName.IndexOf(" ("));
        //        }
        //        else
        //        {
        //            currentName = currentName.Remove(currentName.IndexOf("("));
        //        }
        //    }
        //    currentName = currentName + " (" + (_fieldsCollection.FieldsCollection.Count - 1) + ")";
        //    return currentName;
        //}

        //void setName()
        //{
        //    string disName = createDisplayName(ModelItem.Properties["DisplayName"].ComputedValue as string);
        //    ModelItem.Properties["DisplayName"].SetValue(disName);
        //}

        //#endregion

        //#region Event Handlers

        ////DONT TAKE OUT... This has been done so that the drill down doesnt happen.
        //void DsfMultiAssignActivityDesigner_OnPreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        //{
        //    e.Handled = true;
        //}

        //private void FieldsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    FieldsDataGrid.RemoveRow();

        //    setName();
        //}

        //void SetValuetxt_KeyUp(object sender, KeyEventArgs e)
        //{

        //    if (_initialHeight == 0.0)
        //    {
        //        _initialHeight = ActualHeight;
        //    }

        //    FieldsDataGrid.AddRow();

        //    if (e.Key == Key.Tab)
        //    {
        //        // adjust the tab ordering ;)

        //    }

        //    TextBox textBox = sender as TextBox;
        //    if (textBox != null )
        //    {
        //        ModelItem modelItem = textBox.DataContext as ModelItem;
        //        if (modelItem != null)
        //        {
        //            ActivityDTO activityDto = modelItem.GetCurrentValue() as ActivityDTO;
        //            if (activityDto != null && activityDto.Inserted)
        //            {
        //                activityDto.Inserted = false;
        //            }
        //        }

        //    }

        //    setName();
        //}

        //void DeleteRow_MenuItem_Click(object sender, RoutedEventArgs e)
        //{
        //    FieldsDataGrid.RemoveRow(FieldsDataGrid.SelectedIndex);
        //    setName();
        //}

        //void InsertRow_MenuItem_Click(object sender, RoutedEventArgs e)
        //{
        //    FieldsDataGrid.InsertRow(FieldsDataGrid.SelectedIndex);
        //    setName();
        //}

        //void ActivityDesigner_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        //{
        //    if (e.Source.GetType() == typeof(Dev2DataGrid))
        //    {
        //        ShowRightClickOptions = true;
        //    }
        //    else
        //    {
        //        ShowRightClickOptions = false;
        //    }
        //}

        //void QuickVariableInputControl_OnLoaded(object sender, RoutedEventArgs e)
        //{
        //    ViewModel.CloseAdornersRequested += OnViewModelOnCloseAdornersRequested;
        //}

        //void OnViewModelOnCloseAdornersRequested(object sender, EventArgs e)
        //{
        //    ShowQuickVariableInput = false;
        //}

        //void UIElement_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        //{
        //    IInputElement inputElement = sender as IInputElement;
        //    if (inputElement == null)
        //    {
        //        return;
        //    }

        //    Mouse.Capture(sender as IInputElement, CaptureMode.SubTree);

        //    if (_workflowDesignerSelection != null && _workflowDesignerSelection.SelectedObjects.FirstOrDefault() != ModelItem)
        //    {
        //        Selection.SelectOnly(Context, ModelItem);
        //    }

        //    _mousedownPoint = e.GetPosition(sender as IInputElement);
        //    _startManualDrag = true;
        //    e.Handled = true;
        //}

        //void UIElement_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        //{
        //    IInputElement inputElement = sender as IInputElement;
        //    if (inputElement == null)
        //    {
        //        return;
        //    }

        //    inputElement.ReleaseMouseCapture();
        //    Focus();
        //    BringToFront();
        //}

        //void UIElement_OnPreviewMouseMove(object sender, MouseEventArgs e)
        //{
        //    IInputElement inputElement = sender as IInputElement;
        //    if (inputElement == null)
        //    {
        //        return;
        //    }

        //    Point tempPoint = e.GetPosition(sender as IInputElement);
        //    double xDelta = Math.Abs(tempPoint.X - _mousedownPoint.X);
        //    double yDelta = Math.Abs(tempPoint.Y - _mousedownPoint.Y);

        //    if (e.LeftButton == MouseButtonState.Pressed && _startManualDrag && Math.Max(xDelta, yDelta) >= 5)
        //    {
        //        DragDropHelper.DoDragMove(this, e.GetPosition(this));
        //        _startManualDrag = false;
        //        inputElement.ReleaseMouseCapture();
        //        Focus();
        //        BringToFront();
        //    }
        //}

        //void UIElement_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        //{
        //    _mousedownPoint = e.GetPosition(sender as IInputElement);
        //    _startManualDrag = true;
        //}

        //void UIElement_OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        //{
        //    _startManualDrag = false;
        //}

        //void ShowAllAdorners()
        //{
        //    //BringToFront();
        //    ShowAdorners = true;
        //}

        //private void BringToFront()
        //{
        //    try
        //    {
        //        var fElement = VisualTreeHelper.GetParent(this) as FrameworkElement;
        //        if (fElement != null)
        //        {
        //            fElement.BringToFront();
        //        }
        //    }
        //    catch
        //    {
        //    }
        //}

        //void HideAdorners(bool forceHide = false)
        //{
        //    if ((!IsAdornerOpen || forceHide) && !IsSelected )
        //    {
        //        UIElement uiElement = VisualTreeHelper.GetParent(this) as UIElement;
        //        if (uiElement != null)
        //        {
        //            Panel.SetZIndex(uiElement, int.MinValue);
        //        }

        //        ShowAdorners = false;
        //        IsAdornerOpen = false;
        //    }
        //}

        //void DsfMultiAssignActivityDesigner_OnMouseEnter(object sender, MouseEventArgs e)
        //{
        //    if (e.LeftButton == MouseButtonState.Pressed)
        //    {
        //        HideAdorners(true);       
        //    }
        //    else
        //    {
        //        ShowAllAdorners();
        //    }
        //}

        //void SelectionChanged(Selection item)
        //{
        //    _workflowDesignerSelection = item;

        //    if (_workflowDesignerSelection != null)
        //    {
        //        if (_workflowDesignerSelection.PrimarySelection == ModelItem)
        //        {
        //            IsSelected = true;
        //            BringToFront();
        //            ShowAllAdorners();
        //        }
        //        else
        //        {
        //            IsSelected = false;
        //            HideAdorners();
        //        }
        //    }
        //}

        //protected bool IsSelected { get; set; }

        //#endregion

        //#region Clean Up

        //void CleanUp()
        //{
        //    Context.Items.Unsubscribe<Selection>(SelectionChanged);
        //}

        //#endregion

        //private void DsfMultiAssignActivityDesigner_OnMouseLeave(object sender, MouseEventArgs e)
        //{
        //    HideAdorners();
        //}

        //private void DsfMultiAssignActivityDesigner_OnPreviewDragEnter(object sender, DragEventArgs e)
        //{
        //    HideAdorners(true);
        //}
    }
}

