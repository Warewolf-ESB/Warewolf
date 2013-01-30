using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Studio.Core.Interfaces;

namespace Dev2.Studio.Core.Activities.Services
{
    public interface IDesignerManagementService
    {
        IContextualResourceModel GetResourceModel(ModelItem modelItem);
        void RequestExpandAll();
        void RequestCollapseAll();
        void RequestRestoreAll();

        event EventHandler ExpandAllRequested;
        event EventHandler CollapseAllRequested;
        event EventHandler RestoreAllRequested;
    }
}
