using Dev2.Studio.Core.Wizards.Interfaces;
using System.ComponentModel.Composition;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Studio.Core.Wizards.CallBackHandlers
{
    [Export(typeof(IActivitySpecificSettingsWizardCallbackHandler<DsfDateTimeActivity>))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class DsfNumberFormatActivityWizCallback : DsfBaseWizCallback<DsfNumberFormatActivity>
    {
    }
}
