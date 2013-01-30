using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces.DataList;
using Dev2.UI;
using System;
using System.Activities.Presentation.Model;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    public partial class DsfDataSplitActivityDesigner : IDisposable
    {

        private bool _isRegistered = false;
        private string mediatorKey = string.Empty;
        private ModelItem activity;
        private dynamic _resultsCollection;
        public DsfDataSplitActivityDesigner()
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
            DependencyProperty.Register("ShowRightClickOptions", typeof(bool), typeof(DsfDataSplitActivityDesigner), new UIPropertyMetadata(false));

        #endregion Dependancy Properties

        protected override void OnModelItemChanged(object newItem)
        {
            base.OnModelItemChanged(newItem);
            if (!_isRegistered)
            {
                //mediatorKey = Mediator.RegisterToReceiveMessage(MediatorMessages.DataListItemSelected, input => Highlight(input as IDataListItemModel));
            }
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
            //setName();
        }

        private void Highlight(IDataListItemModel dataListItemViewModel)
        {
            Dev2.UI.IntellisenseTextBox bob = new Dev2.UI.IntellisenseTextBox();

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

        public void Dispose()
        {
            Mediator.DeRegister(MediatorMessages.DataListItemSelected, mediatorKey);
        }

        private void SetValuetxt_KeyUp(object sender, KeyEventArgs e)
        {
            Resultsdg.AddRow();

            string disName = createDisplayName(ModelItem.Properties["DisplayName"].ComputedValue as string);
            ModelItem.Properties["DisplayName"].SetValue(disName);

            setName();
        }

        private string createDisplayName(string currentName)
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

        private void CbxLoad(object sender, RoutedEventArgs e)
        {
            ComboBox cbx = sender as ComboBox;
            dynamic temp = cbx.DataContext;
            string selectedVal = temp.SplitType;
            if (cbx != null)
            {
                if (cbx.Items.Count == 0)
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

        private void setName()
        {
            string disName = createDisplayName(ModelItem.Properties["DisplayName"].ComputedValue as string);
            ModelItem.Properties["DisplayName"].SetValue(disName);
        }

        private void Resultsdg_SelectionChanged(object sender, SelectionChangedEventArgs e)
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

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var test = Resultsdg.SelectedIndex;
            Resultsdg.RemoveRow(Resultsdg.SelectedIndex);
            setName();
        }

        private void ContextMenu_ContextMenuOpening(object sender, ContextMenuEventArgs e)
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
    }
}
