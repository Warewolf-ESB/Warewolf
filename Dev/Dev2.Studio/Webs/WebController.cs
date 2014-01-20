using Caliburn.Micro;
using Dev2.Providers.Logs;
using Dev2.Services.Events;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using System;
using System.ComponentModel.Composition;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Webs
{

    [Export(typeof(IWebController))]
    public class WebController : IHandle<CloseWizardMessage>, IWebController
    {
        [ImportingConstructor]
        public WebController()
        {
            EventPublishers.Aggregator.Subscribe(this);
        }

        public void DisplayDialogue(IContextualResourceModel resourceModel, bool includeArgs)
        {
            if(RootWebSite.ShowDialog(resourceModel))
            {
            }
        }

        public void CloseWizard()
        {
            throw new NotImplementedException();
        }

        #region IHandle

        public void Handle(ShowWebpartWizardMessage message)
        {
            this.TraceInfo(message.GetType().Name);
        }

        public void Handle(CloseWizardMessage message)
        {
            this.TraceInfo(message.GetType().Name);
            CloseWizard();
        }

        public void Handle(SetActivePageMessage message)
        {
            this.TraceInfo(message.GetType().Name);
        }
        #endregion IHandle

    }
}
