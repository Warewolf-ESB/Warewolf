using System;
using Dev2.Studio.AppResources.Comparers;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Factory;
using Dev2.Studio.ViewModels.Diagnostics;

namespace Dev2.Studio.AppResources.Messages
{
    public class SetDebugStatusMessage
    {
        public WorkSurfaceKey WorkSurfaceKey { get; set; }
        public DebugStatus DebugStatus { get; set; }

        public SetDebugStatusMessage(IContextualResourceModel resourceModel, DebugStatus debugStatus)
        {
            WorkSurfaceKey = WorkSurfaceKeyFactory.CreateKey(resourceModel);
            DebugStatus = debugStatus;
        }

        public SetDebugStatusMessage(Guid serverID, Guid resourceID, DebugStatus debugStatus)
        {
            WorkSurfaceKey = WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.Workflow, resourceID, serverID);
            DebugStatus = debugStatus;
        }
    }
}
