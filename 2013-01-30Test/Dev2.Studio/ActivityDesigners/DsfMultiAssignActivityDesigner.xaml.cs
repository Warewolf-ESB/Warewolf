using Dev2.Studio.Core;
using Dev2.UI;
using System;
using System.Activities.Presentation.Model;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    public partial class DsfMultiAssignActivityDesigner : IDisposable
    {

        private bool _isRegistered = false;
        private string mediatorKey = string.Empty;
        private dynamic _fieldsCollection;
        private double _initialFieldHeight;
        private double _initialHeight;

        public DsfMultiAssignActivityDesigner()
        {

            InitializeComponent();
        }

        #region Dependancy Properties

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

        public void Dispose()
        {
            Mediator.DeRegister(MediatorMessages.DataListItemSelected, mediatorKey);
        }

        private void SetValuetxt_KeyUp(object sender, KeyEventArgs e)
        {

            if (_initialHeight == 0.0)
            {
                _initialFieldHeight = FieldsDataGrid.ActualHeight;
                _initialHeight = ActualHeight;
            }

            FieldsDataGrid.AddRow();

            setName();

        }

        //private string createDisplayName(string currentName)
        //{
        //    currentName = ModelItem.Properties["DisplayName"].ComputedValue as string;

        //    string actualName = string.Empty;
        //    if (currentName.Contains('(') && currentName.Contains(')'))
        //    {
        //        actualName = currentName.Substring(0, currentName.IndexOf('('));
        //    }
        //    else
        //    {
        //        actualName = currentName;
        //    }
        //    actualName = actualName.Trim();

        //    int rows = FieldsDataGrid.CountRows();
        //    if (rows == -1)
        //    {
        //        if (currentName.Split(' ').Count() == 1)
        //        {
        //            currentName = actualName + " (0)";
        //        }
        //        else
        //        {
        //            currentName = ModelItem.Properties["DisplayName"].ComputedValue as string;
        //        }
        //    }
        //    else
        //    {
        //        currentName = actualName + " (" + (rows) + ")";
        //    }
        //    return currentName;
        //}

        private string createDisplayName(string currentName)
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

        private void setName()
        {
            string disName = createDisplayName(ModelItem.Properties["DisplayName"].ComputedValue as string);
            ModelItem.Properties["DisplayName"].SetValue(disName);
        }

        private void FieldsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FieldsDataGrid.RemoveRow();

            EnsureHeight();

            setName();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var test = FieldsDataGrid.SelectedIndex;
            FieldsDataGrid.RemoveRow(FieldsDataGrid.SelectedIndex);
            setName();
            EnsureHeight();
        }

        private void EnsureHeight()
        {
            //InvalidateMeasure();
            //InvalidateArrange();
            //FieldsDataGrid.InvalidateMeasure();
            //FieldsDataGrid.InvalidateArrange();
            //FieldsDataGrid.InvalidateVisual();
            //FieldsDataGrid.UpdateLayout();
            //UpdateLayout();
        }

        private void ActivityDesigner_ContextMenuOpening(object sender, ContextMenuEventArgs e)
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

        private void doCustomScroll(object sender, MouseWheelEventArgs e)
        {
            Dev2DataGrid theGrid = FieldsDataGrid;
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
            object theItem = theGrid.Items[theGrid.SelectedIndex];
            theGrid.ScrollIntoView(theItem);
        }
    }
}

