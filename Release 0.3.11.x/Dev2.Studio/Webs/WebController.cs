using System;
using System.ComponentModel.Composition;
using Caliburn.Micro;
using Dev2.Providers.Logs;
using Dev2.Services.Events;
using Dev2.Studio.Core.Controller;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;

namespace Dev2.Studio.Webs
{

    [Export(typeof(IWebController))]
    public class WebController : IHandle<CloseWizardMessage>, IWebController
    {
        private readonly IPopupController _popupProvider;

        [ImportingConstructor]
        public WebController([Import]IPopupController popupProvider)
        {
            _popupProvider = popupProvider;
            EventPublishers.Aggregator.Subscribe(this);
        }

        public void DisplayDialogue(IContextualResourceModel resourceModel, bool includeArgs)
        {
            if (RootWebSite.ShowDialog(resourceModel))
            {
                return;
            }
        }

        public void CloseWizard()
        {
            throw new NotImplementedException();
        }

        #region IHandle

        public void Handle(ShowWebpartWizardMessage message)
        {
            Logger.TraceInfo(message.GetType().Name);
        }

        public void Handle(CloseWizardMessage message)
        {
            Logger.TraceInfo(message.GetType().Name);
            CloseWizard();
        }

        public void Handle(SetActivePageMessage message)
        {
            Logger.TraceInfo(message.GetType().Name);
        }
        #endregion IHandle

    }
}
