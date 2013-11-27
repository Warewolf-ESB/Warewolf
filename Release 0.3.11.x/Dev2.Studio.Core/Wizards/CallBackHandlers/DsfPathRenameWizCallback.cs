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
    // Old Wizard Functionlity
    //[Export(typeof(IActivitySpecificSettingsWizardCallbackHandler<DsfPathRename>))]
    //[PartCreationPolicy(CreationPolicy.NonShared)]
    public class DsfPathRenameWizCallback : DsfBaseWizCallback<DsfPathRename>
    {
    }
}
