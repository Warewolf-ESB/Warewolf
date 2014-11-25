
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Activities.Presentation.Model;
using Caliburn.Micro;
using Dev2.Common;
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
            Dev2Logger.Log.Info(message.GetType().Name);
        }

        public void Dispose()
        {
            EventPublishers.Aggregator.Unsubscribe(this);
        }
    }
}
