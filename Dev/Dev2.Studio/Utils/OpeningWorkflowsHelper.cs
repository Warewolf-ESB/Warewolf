
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

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
        static readonly IDictionary<string, bool> _resourcesCurrentlyWaitingForFirstFocusLoss = new Dictionary<string, bool>();
        static readonly List<WorkSurfaceKey> _resourceCurrentlyWaitingForWaterMarkUpdates = new List<WorkSurfaceKey>();


        /// <summary>
        /// Adds the workflow.
        /// </summary>
        /// <param name="workSurfaceKey">The work surface key.</param>
        public static void AddWorkflow(WorkSurfaceKey workSurfaceKey)
        {
            _resourcesCurrentlyInOpeningState.Add(workSurfaceKey);
            _resourcesCurrentlyInOpeningStateWaitingForLoad.Add(workSurfaceKey);
        }

        /// <summary>
        /// Removes the workflow.
        /// </summary>
        /// <param name="workSurfaceKey">The work surface key.</param>
        public static void RemoveWorkflow(WorkSurfaceKey workSurfaceKey)
        {
            _resourcesCurrentlyInOpeningState.Remove(workSurfaceKey);
        }

        /// <summary>
        /// Removes the workflow waiting for designer load.
        /// </summary>
        /// <param name="workSurfaceKey">The work surface key.</param>
        public static void RemoveWorkflowWaitingForDesignerLoad(WorkSurfaceKey workSurfaceKey)
        {
            _resourcesCurrentlyInOpeningStateWaitingForLoad.Remove(workSurfaceKey);
        }

        /// <summary>
        /// Fetches the opening keys.
        /// </summary>
        /// <returns></returns>
        public static List<WorkSurfaceKey> FetchOpeningKeys()
        {
            return _resourcesCurrentlyInOpeningState;
        }

        /// <summary>
        /// Determines whether [is workflow waitingfor designer load] [the specified work surface key].
        /// </summary>
        /// <param name="workSurfaceKey">The work surface key.</param>
        /// <returns></returns>
        public static bool IsWorkflowWaitingforDesignerLoad(WorkSurfaceKey workSurfaceKey)
        {
            return _resourcesCurrentlyInOpeningStateWaitingForLoad.Contains(workSurfaceKey);
        }

        /// <summary>
        /// Adds the workflow waiting for first focus loss.
        /// </summary>
        /// <param name="workSurfaceKey">The work surface key.</param>
        public static void AddWorkflowWaitingForFirstFocusLoss(WorkSurfaceKey workSurfaceKey)
        {
            _resourcesCurrentlyWaitingForFirstFocusLoss[workSurfaceKey.ToString()] = true;
        }

        /// <summary>
        /// Removes the workflow waiting for first focus loss.
        /// </summary>
        /// <param name="workSurfaceKey">The work surface key.</param>
        public static void RemoveWorkflowWaitingForFirstFocusLoss(WorkSurfaceKey workSurfaceKey)
        {
            _resourcesCurrentlyWaitingForFirstFocusLoss[workSurfaceKey.ToString()] = false;
        }

        /// <summary>
        /// Determines whether [is waiting for fist focus loss] [the specified work surface key].
        /// </summary>
        /// <param name="workSurfaceKey">The work surface key.</param>
        /// <returns></returns>
        public static bool IsWaitingForFistFocusLoss(WorkSurfaceKey workSurfaceKey)
        {
            if(_resourcesCurrentlyWaitingForFirstFocusLoss.ContainsKey(workSurfaceKey.ToString()))
            {
                return _resourcesCurrentlyWaitingForFirstFocusLoss[workSurfaceKey.ToString()];
            }

            return false;
        }

        /// <summary>
        /// Determines whether [is loaded information focus loss catalog] [the specified work surface key].
        /// </summary>
        /// <param name="workSurfaceKey">The work surface key.</param>
        /// <returns></returns>
        public static bool IsLoadedInFocusLossCatalog(WorkSurfaceKey workSurfaceKey)
        {
            return _resourcesCurrentlyWaitingForFirstFocusLoss.ContainsKey(workSurfaceKey.ToString());
        }

        /// <summary>
        /// Adds the workflow waiting for water mark updates.
        /// </summary>
        /// <param name="workSurfaceKey">The work surface key.</param>
        public static void AddWorkflowWaitingForWaterMarkUpdates(WorkSurfaceKey workSurfaceKey)
        {
            _resourceCurrentlyWaitingForWaterMarkUpdates.Add(workSurfaceKey);
        }

        /// <summary>
        /// Removes the workflow waiting for water mark updates.
        /// </summary>
        /// <param name="workSurfaceKey">The work surface key.</param>
        public static void RemoveWorkflowWaitingForWaterMarkUpdates(WorkSurfaceKey workSurfaceKey)
        {
            _resourceCurrentlyWaitingForWaterMarkUpdates.Remove(workSurfaceKey);
        }

        /// <summary>
        /// Determines whether [is workflow waiting for water mark updates] [the specified work surface key].
        /// </summary>
        /// <param name="workSurfaceKey">The work surface key.</param>
        /// <returns></returns>
        public static bool IsWorkflowWaitingForWaterMarkUpdates(WorkSurfaceKey workSurfaceKey)
        {
            return _resourceCurrentlyWaitingForWaterMarkUpdates.Contains(workSurfaceKey);
        }

        /// <summary>
        /// Prunes the workflow from caches.
        /// </summary>
        /// <param name="workSurfaceKey">The work surface key.</param>
        public static void PruneWorkflowFromCaches(WorkSurfaceKey workSurfaceKey)
        {
            if(_resourcesCurrentlyWaitingForFirstFocusLoss.ContainsKey(workSurfaceKey.ToString()))
            {
                _resourcesCurrentlyWaitingForFirstFocusLoss.Remove(workSurfaceKey.ToString());
            }

            if(_resourcesCurrentlyInOpeningState.Contains(workSurfaceKey))
            {
                _resourcesCurrentlyInOpeningState.Remove(workSurfaceKey);
            }

            if(_resourcesCurrentlyInOpeningStateWaitingForLoad.Contains(workSurfaceKey))
            {
                _resourcesCurrentlyInOpeningStateWaitingForLoad.Remove(workSurfaceKey);
            }

            if(_resourceCurrentlyWaitingForWaterMarkUpdates.Contains(workSurfaceKey))
            {
                _resourceCurrentlyWaitingForWaterMarkUpdates.Remove(workSurfaceKey);
            }
        }

    }
}
