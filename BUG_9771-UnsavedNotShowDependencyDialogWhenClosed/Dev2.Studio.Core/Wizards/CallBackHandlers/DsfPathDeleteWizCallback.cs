using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using Dev2.Studio.Core.Wizards.Interfaces;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;


namespace Dev2.Studio.Core.Wizards.CallBackHandlers
{
    [Export(typeof(IActivitySpecificSettingsWizardCallbackHandler<DsfPathDelete>))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class DsfPathDeleteWizCallback : DsfBaseWizCallback<DsfPathDelete>
    {
    }
}
