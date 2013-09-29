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
using Caliburn.Micro;
using Dev2.Composition;
using Dev2.Providers.Logs;
using Dev2.Services.Events;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio;
using System.Activities.Presentation;
using Dev2.Studio.Core.Interfaces.DataList;
using Dev2.Studio.Core.Messages;

namespace Unlimited.Applications.BusinessDesignStudio.Activities {
    public partial class DsfAssignActivityDesigner : IDisposable,IHandle<DataListItemSelectedMessage>
    {
        public DsfAssignActivityDesigner() {
            InitializeComponent();
            EventPublishers.Aggregator.Subscribe(this);
        }

        protected override void OnModelItemChanged(object newItem) {
            base.OnModelItemChanged(newItem);

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
            border.Visibility = Visibility.Hidden;

            SetValuetxt.BorderBrush = Brushes.LightGray;
            SetValuetxt.BorderThickness = new Thickness(1.0);
            ToValuetxt.BorderBrush = Brushes.LightGray;
            ToValuetxt.BorderThickness = new Thickness(1.0);

            containingFields = DsfActivityDataListComparer.ContainsDataListItem(ModelItem, dataListItemViewModel);

            if (containingFields.Count > 0) {
                foreach (string item in containingFields) {
                    if (item.Equals("FieldName")) {
                        SetValuetxt.BorderBrush = System.Windows.Media.Brushes.DeepSkyBlue;
                        SetValuetxt.BorderThickness = new Thickness(2.0);
                    }
                    else if (item.Equals("FieldValue")) {
                        ToValuetxt.BorderBrush = System.Windows.Media.Brushes.DeepSkyBlue;
                        ToValuetxt.BorderThickness = new Thickness(2.0);
                    }
                    var bob = this.BorderBrush;
                    
                    
                }
            }
        }

        public void Dispose() {
            EventPublishers.Aggregator.Unsubscribe(this);
        }

        #region Implementation of IHandle<DataListItemSelectedMessage>

        public void Handle(DataListItemSelectedMessage message)
        {
            Logger.TraceInfo(message.GetType().Name);
            Highlight(message.DataListItemModel);
        }

        #endregion
    }

    
}
