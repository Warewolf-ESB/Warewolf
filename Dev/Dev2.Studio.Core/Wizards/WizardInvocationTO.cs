using System;
using Dev2.Studio.Core.Wizards.Interfaces;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.Wizards
{
    public class WizardInvocationTO
    {
        public Uri Endpoint { get; set; }
        public Guid TransferDatalistID { get; set; }
        public Guid ExecutionStatusCallbackID { get; set; }
        public IWizardCallbackHandler CallbackHandler { get; set; }
        public string WizardTitle { get; set; }
    }
}
