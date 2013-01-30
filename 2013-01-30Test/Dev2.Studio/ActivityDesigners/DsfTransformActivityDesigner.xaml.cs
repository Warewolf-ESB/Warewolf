using System;
using System.Activities;
using System.Activities.Presentation.Model;
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
using Dev2.Studio;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Interfaces.DataList;

namespace Unlimited.Applications.BusinessDesignStudio.Activities{
    // Interaction logic for DsfTransformActivityDesigner.xaml
    public partial class DsfTransformActivityDesigner : IDisposable {
        private bool _isRegistered = false;
        private string mediatorKey = string.Empty;
        public DsfTransformActivityDesigner() {
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
            //List<string> containingFields = new List<string>();
            //border.Visibility = Visibility.Hidden;

            //MatchTagtxt.BorderBrush = Brushes.LightGray;
            //MatchTagtxt.BorderThickness = new Thickness(1.0);
            //ForEachTranformtxt.BorderBrush = Brushes.LightGray;
            //ForEachTranformtxt.BorderThickness = new Thickness(1.0);
            //RootTagtxt.BorderBrush = Brushes.LightGray;
            //RootTagtxt.BorderThickness = new Thickness(1.0);

            //containingFields = DsfActivityDataListComparer.ContainsDataListItem(ModelItem, dataListItemViewModel);

            //if (containingFields.Count > 0) {
            //    foreach (string item in containingFields) {
            //        if (item.Equals("TransformElementName")) {
            //            MatchTagtxt.BorderBrush = System.Windows.Media.Brushes.Aqua;
            //            MatchTagtxt.BorderThickness = new Thickness(2.0);
            //        }
            //        else if (item.Equals("Transformation")) {
            //            ForEachTranformtxt.BorderBrush = System.Windows.Media.Brushes.Aqua;
            //            ForEachTranformtxt.BorderThickness = new Thickness(2.0);
            //        }
            //        else if (item.Equals("RootTag")) {
            //            RootTagtxt.BorderBrush = System.Windows.Media.Brushes.Aqua;
            //            RootTagtxt.BorderThickness = new Thickness(2.0);
            //        }
            //    }

            //}
        }

        public void Dispose() {
            Mediator.DeRegister(MediatorMessages.DataListItemSelected, mediatorKey);
        }
    }
}
