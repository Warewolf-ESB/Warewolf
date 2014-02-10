using System.Activities.Presentation.Model;
using Caliburn.Micro;
using Dev2.Activities.Designers2.Email;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Studio.Core.Interfaces;

namespace Dev2.Activities.Designers.Tests.Email
{
    public class TestEmailDesignerViewModel : EmailDesignerViewModel
    {
        public TestEmailDesignerViewModel(ModelItem modelItem, IEnvironmentModel environmentModel, IEventAggregator eventPublisher)
            : base(modelItem, new TestAsyncWorker(), environmentModel, eventPublisher)
        {
        }

        public EmailSource SelectedEmailSourceModelItemValue
        {
            // ReSharper disable ExplicitCallerInfoArgument
            get { return GetProperty<EmailSource>("SelectedEmailSource"); }
            // ReSharper restore ExplicitCallerInfoArgument
            set
            {
                // ReSharper disable ExplicitCallerInfoArgument
                SetProperty(value, "SelectedEmailSource");
                // ReSharper restore ExplicitCallerInfoArgument
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