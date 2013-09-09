using Dev2.Studio.Core.Wizards.Interfaces;
using System.ComponentModel.Composition;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Studio.Core.Wizards.CallBackHandlers
{
    // Old Wizard Functionlity
    //[Export(typeof(IActivitySpecificSettingsWizardCallbackHandler<DsfDateTimeActivity>))]
    //[PartCreationPolicy(CreationPolicy.NonShared)]
    public class DsfNumberFormatActivityWizCallback : DsfBaseWizCallback<DsfNumberFormatActivity>
    {
    }
}
