using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Activities.Presentation.Model;
using Dev2;
using Dev2.Converters;
using Dev2.Studio.Core;
using Dev2.UI;
using Dev2.Common;
using Dev2.Studio.Core.Interfaces.DataList;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    public partial class DsfBaseConvertActivityDesigner : IDisposable
    {

        private bool _isRegistered = false;
        private string mediatorKey = string.Empty;
        private ModelItem activity;
        private dynamic _convertCollection;
        public DsfBaseConvertActivityDesigner()
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
            DependencyProperty.Register("ShowRightClickOptions", typeof(bool), typeof(DsfBaseConvertActivityDesigner), new UIPropertyMetadata(false));

        #endregion Dependancy Properties

        protected override void OnModelItemChanged(object newItem)
        {
            base.OnModelItemChanged(newItem);
            if (!_isRegistered)
            {
                //mediatorKey = Mediator.RegisterToReceiveMessage(MediatorMessages.DataListItemSelected, input => Highlight(input as IDataListItemModel));
            }
            _convertCollection = newItem;
            activity = newItem as ModelItem;

            if (_convertCollection.ConvertCollection == null || _convertCollection.ConvertCollection.Count <= 0)
            {
                _convertCollection.ConvertCollection.Add(new BaseConvertTO("", "Text", "Base 64", "", 1));
                _convertCollection.ConvertCollection.Add(new BaseConvertTO("", "Text", "Base 64", "", 2));
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
        }

        private void Highlight(IDataListItemModel dataListItemViewModel)
        {

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
            List<BaseConvertTO> collection = ModelItem.Properties["ConvertCollection"].ComputedValue as List<BaseConvertTO>;
            if (collection != null)
            {
                int result = -1;
                BaseConvertTO lastItem = collection.LastOrDefault(c => c.FromExpression != string.Empty);
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

        private string createDisplayName()
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
            currentName = currentName + " (" + (_convertCollection.ConvertCollection.Count - 1) + ")";
            return currentName;
        }

        private void CbxLoad(object sender, RoutedEventArgs e)
        {
            ComboBox cbx = sender as ComboBox;
            dynamic temp = cbx.DataContext;
            string selectedVal = temp.ConvertType;
            if (cbx != null)
            {
                if (cbx.Items.Count == 0)
                {
                    cbx.ItemsSource = Dev2EnumConverter.ConvertEnumsToStringList<enDev2BaseConvertType>();
                }
                //if (string.IsNullOrWhiteSpace(selectedVal))
                //{
                //    cbx.SelectedIndex = 0;
                //}
                //else
                //{
                //    cbx.SelectedValue = selectedVal;
                //}
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var test = Resultsdg.SelectedIndex;
            Resultsdg.RemoveRow(Resultsdg.SelectedIndex);
            ModelItem.Properties["DisplayName"].SetValue(createDisplayName());
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
    }
}



