using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Studio.Core.Interfaces;

namespace Dev2.Studio.Webs
{
    public interface IWebController
    {
        void DisplayDialogue(IContextualResourceModel resourceModel, bool includeArgs, bool isSaveDialogStandAlone = false);
        void CloseWizard();
    }
}
