using Dev2.Studio.Core.Activities.Utils;
using Dev2.UI;
using System;
using System.Activities.Presentation.Model;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    public partial class DsfDataMergeActivityDesigner : IDisposable
    {

        private bool _isRegistered = false;
        private ModelItem _activity;
        private dynamic _resultsCollection;
        public DsfDataMergeActivityDesigner()
        {
            InitializeComponent();
        }

        #region Dependancy Properties

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
            if (!_isRegistered)
            {
                // This may be used at a later stage
                //mediatorKey = Mediator.RegisterToReceiveMessage(MediatorMessages.DataListItemSelected, input => Highlight(input as IDataListItemModel));
            }
            _resultsCollection = newItem;
            _activity = newItem as ModelItem;

            if (_resultsCollection.MergeCollection == null || _resultsCollection.MergeCollection.Count <= 0)
            {
                _resultsCollection.MergeCollection.Add(new DataMergeDTO("", "None", "", 1, "", "Left"));
                _resultsCollection.MergeCollection.Add(new DataMergeDTO("", "None", "", 2, "", "Left"));
            }

            if (_activity == null) return;
            ModelItem parent = _activity.Parent;

            while (parent != null)
            {
                if (parent.Properties["Argument"] != null)
                {
                    break;
                }

                parent = parent.Parent;
            }
        }

        // This may be used at a later stage
        //private void Highlight(IDataListItemModel dataListItemViewModel)
        //{          
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
        //}

        public void Dispose()
        {
            // This may be used at a later stage
            //Mediator.DeRegister(MediatorMessages.DataListItemSelected, mediatorKey);
        }

        private void SetValuetxt_KeyUp(object sender, KeyEventArgs e)
        {
            Resultsdg.AddRow();
            ModelProperty modelProperty = ModelItem.Properties["DisplayName"];
            if (modelProperty != null)
            {
                string disName = createDisplayName(modelProperty.ComputedValue as string);
                modelProperty.SetValue(disName);
            }
        }

        private string createDisplayName(string currentName)
        {
            // 6279, CODE REVIEW, Null check needed
            ModelProperty modelProperty = _activity.Properties["DisplayName"];
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

        private void CbxLoad(object sender, RoutedEventArgs e)
        {
            ComboBox cbx = sender as ComboBox;
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

        private void Resultsdg_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var tmpCbx = sender as ComboBox;
                if (tmpCbx != null)
                {
                    ModelItem model = (ModelItem)tmpCbx.DataContext;

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

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Resultsdg.RemoveRow(Resultsdg.SelectedIndex);
            var modelProperty = _activity.Properties["DisplayName"];
            if (modelProperty != null)
                modelProperty.SetValue(createDisplayName(modelProperty.ComputedValue as string));
        }

        private void ContextMenu_ContextMenuOpening(object sender, ContextMenuEventArgs e)
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
    }
}
