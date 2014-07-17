using System;
using Dev2.Studio.Core.Interfaces;

// ReSharper disable CheckNamespace
namespace Dev2.Studio.Core.Activities.Services
{
    public interface IDesignerManagementService : IDisposable
    {
        IContextualResourceModel GetRootResourceModel();
        void RequestExpandAll();
        void RequestCollapseAll();
        void RequestRestoreAll();

        event EventHandler ExpandAllRequested;
        event EventHandler CollapseAllRequested;
        event EventHandler RestoreAllRequested;
    }
}
