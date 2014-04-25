using System;
using System.Activities.Presentation.Model;
using Caliburn.Micro;
using Dev2.Providers.Logs;
using Dev2.Services.Events;
using Dev2.Studio.Core.Messages;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    // Interaction logic for DsfTransformActivityDesigner.xaml
    public partial class DsfTransformActivityDesigner : IDisposable, IHandle<DataListItemSelectedMessage>
    {
        public DsfTransformActivityDesigner()
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

        public void Handle(DataListItemSelectedMessage message)
        {
            this.TraceInfo(message.GetType().Name);
        }

        public void Dispose()
        {
            EventPublishers.Aggregator.Unsubscribe(this);
        }
    }
}
