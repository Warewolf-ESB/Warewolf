using System;
using System.Activities.Presentation.Model;
using System.Windows;
using Caliburn.Micro;
using Dev2.Services.Events;
using Dev2.Studio.Core.Interfaces.DataList;
using Dev2.Studio.Core.Messages;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    public partial class DsfWebPageActivityDesigner : IDisposable, IHandle<DataListItemSelectedMessage>
    {
        public DsfWebPageActivityDesigner()
        {
            InitializeComponent();
            EventPublishers.Aggregator.Subscribe(this);
        }

        protected override void OnModelItemChanged(object newItem)
        {
            base.OnModelItemChanged(newItem);


            ModelItem item = newItem as ModelItem;

            if(item != null)
            {
                ModelItem parent = item.Parent;

                while(parent != null)
                {
                    if(parent.Properties["Argument"] != null)
                    {
                        break;
                    }

                    parent = parent.Parent;
                }
            }
        }

        private void Highlight(IDataListItemModel dataListItemViewModel)
        {
            border.Visibility = Visibility.Hidden;
        }

        public void Handle(DataListItemSelectedMessage message)
        {
            Highlight(message.DataListItemModel);
        }

        public void Dispose()
        {
            EventPublishers.Aggregator.Unsubscribe(this);
        }
    }
}
