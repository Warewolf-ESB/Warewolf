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
using System.Activities.Statements;
using Dev2.Studio;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Interfaces.DataList;

namespace Unlimited.Applications.BusinessDesignStudio.Activities {
    public partial class DsfWebPageActivityDesigner : IDisposable {
        private bool _isRegistered = false;
        private string mediatorKey = string.Empty;
        public DsfWebPageActivityDesigner()
        {
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
            bool setVisible = false;
            border.Visibility = Visibility.Hidden;
            if (string.IsNullOrEmpty(dataListItemViewModel.Name)) {
                setVisible = false;
            }
            else {
                //setVisible = DsfActivityDataListComparer.ContainsDataListItem(ModelItem, dataListItemViewModel);
            }
            if (setVisible) {
                border.Visibility = Visibility.Visible;
            }
        }

        public void Dispose() {
            Mediator.DeRegister(MediatorMessages.DataListItemSelected, mediatorKey);
        }
    }
}
