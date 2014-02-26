
using System.Collections.Generic;
using Dev2.Studio.AppResources.Comparers;

namespace Dev2.Utils
{


    /// <summary>
    /// Hold currently opening workflows ;)
    /// </summary>
    public static class OpeningWorkflowsHelper
    {

        static readonly List<WorkSurfaceKey> _resourcesCurrentlyInOpeningState = new List<WorkSurfaceKey>();
        static readonly List<WorkSurfaceKey> _resourcesCurrentlyInOpeningStateWaitingForLoad = new List<WorkSurfaceKey>();

        public static void AddWorkflow(WorkSurfaceKey workSurfaceKey)
        {
            _resourcesCurrentlyInOpeningState.Add(workSurfaceKey);
            _resourcesCurrentlyInOpeningStateWaitingForLoad.Add(workSurfaceKey);
        }

        public static void RemoveWorkflow(WorkSurfaceKey workSurfaceKey)
        {
            _resourcesCurrentlyInOpeningState.Remove(workSurfaceKey);
        }

        public static void RemoveWorkflowWaitingForDesignerLoad(WorkSurfaceKey workSurfaceKey)
        {
            _resourcesCurrentlyInOpeningStateWaitingForLoad.Remove(workSurfaceKey);
        }

        public static List<WorkSurfaceKey> FetchOpeningKeys()
        {
            return _resourcesCurrentlyInOpeningState;
        }

        public static bool IsWorkflowWaitingforDesignerLoad(WorkSurfaceKey workSurfaceKey)
        {
            return _resourcesCurrentlyInOpeningStateWaitingForLoad.Contains(workSurfaceKey);
        }

    }
}
