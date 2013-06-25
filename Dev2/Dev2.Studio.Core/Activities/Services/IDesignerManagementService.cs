using Dev2.Studio.Core.Interfaces;
using System;
using System.Activities.Presentation.Model;

namespace Dev2.Studio.Core.Activities.Services
{
    public interface IDesignerManagementService : IDisposable
    {
        IContextualResourceModel GetResourceModel(ModelItem modelItem);
        IContextualResourceModel GetRootResourceModel(ModelItem modelItem);
        void RequestExpandAll();
        void RequestCollapseAll();
        void RequestRestoreAll();

        event EventHandler ExpandAllRequested;
        event EventHandler CollapseAllRequested;
        event EventHandler RestoreAllRequested;
    }
}
