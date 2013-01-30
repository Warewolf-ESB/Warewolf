using System;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Activities;
using System.Activities.Presentation.Model;
using System.Activities.Presentation;
using Dev2.Studio;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Interfaces.DataList;

namespace Unlimited.Applications.BusinessDesignStudio.Activities {
    public partial class DsfForEachActivityDesigner : IDisposable {
        private bool _isRegistered = false;
        private string mediatorKey = string.Empty;
        public DsfForEachActivityDesigner() {
            InitializeComponent();
        }

        protected override void OnModelItemChanged(object newItem) {
            base.OnModelItemChanged(newItem);
            if (!_isRegistered) {
                mediatorKey = Mediator.RegisterToReceiveMessage(MediatorMessages.DataListItemSelected, input => Highlight(input as IDataListItemModel));
            }
            ModelItem item = newItem as ModelItem;

            ModelItem parent = item.Parent;

            while (parent != null)
            {
                if (parent.Properties["Argument"] != null)
                {
                    break;
                }

                parent = parent.Parent;
            }
        }

        private void Highlight(IDataListItemModel dataListItemViewModel) {
            List<string> containingFields = new List<string>();

            ForEverytxt.BorderBrush = Brushes.LightGray;
            ForEverytxt.BorderThickness = new Thickness(1.0);

            containingFields = DsfActivityDataListComparer.ContainsDataListItem(ModelItem, dataListItemViewModel);

            if (containingFields.Count > 0) {
                foreach (string item in containingFields) {
                    if (item.Equals("foreachElementName")) {
                        ForEverytxt.BorderBrush = System.Windows.Media.Brushes.Aqua;
                        ForEverytxt.BorderThickness = new Thickness(2.0);
                    }
                }
            }
        }

        public void Dispose() {
            Mediator.DeRegister(MediatorMessages.DataListItemSelected, mediatorKey);
        }

        private void ForEverytxt_LostFocus(object sender, RoutedEventArgs e) {
            //var textBox = sender as TextBox;
            //char[] token = { ']' };
            //string[] tokens = textBox.Text.Split(token);
            //if (!String.IsNullOrEmpty(textBox.Text)) {
            //    if (textBox.Text.EndsWith("]]") && tokens.Count() == 3) {
            //        if (!textBox.Text.Contains("()")) {
            //            textBox.Text = textBox.Text.Insert(textBox.Text.IndexOf("]"), "()");
            //        }
            //    }
                //else if (textBox.Text.EndsWith("]]") && tokens.Count() > 3) {
                // we have a recursive evaluation happening, only scalars or recordset().field are allowed

                //}
            //}
        }
    }
}
