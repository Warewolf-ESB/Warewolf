/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Activities.Presentation.Model;
using Caliburn.Micro;
using Dev2.Activities.Designers2.Email;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Studio.Interfaces;
using Dev2.Threading;

namespace Dev2.Activities.Designers.Tests.Email
{
    public class TestEmailDesignerViewModel : EmailDesignerViewModel
    {
        public TestEmailDesignerViewModel(ModelItem modelItem, IServer server, IEventAggregator eventPublisher)
            : base(modelItem, new SynchronousAsyncWorker(), server, eventPublisher)
        {
        }

        public EmailSource SelectedEmailSourceModelItemValue
        {
            
            get { return GetProperty<EmailSource>("SelectedEmailSource"); }
            
            set
            {
                
                SetProperty(value, "SelectedEmailSource");
                
            }
        }

        public int OnSelectedEmailSourceChangedHitCount { get; private set; }
        protected override void OnSelectedEmailSourceChanged()
        {
            OnSelectedEmailSourceChangedHitCount++;
            base.OnSelectedEmailSourceChanged();
        }

        public IWebRequestInvoker WebRequestInvoker { get; set; }
        protected override IWebRequestInvoker CreateWebRequestInvoker()
        {
            return WebRequestInvoker;
        }
    }
}
