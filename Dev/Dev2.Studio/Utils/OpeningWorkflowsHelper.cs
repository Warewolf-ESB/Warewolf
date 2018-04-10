/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using Dev2.Common.Interfaces;
using Dev2.Studio.AppResources.Comparers;

namespace Dev2.Utils
{
    public static class OpeningWorkflowsHelper
    {

        static readonly List<IWorkSurfaceKey> _resourcesCurrentlyInOpeningState = new List<IWorkSurfaceKey>();
        static readonly List<IWorkSurfaceKey> _resourcesCurrentlyInOpeningStateWaitingForLoad = new List<IWorkSurfaceKey>();
        static readonly IDictionary<string, bool> _resourcesCurrentlyWaitingForFirstFocusLoss = new Dictionary<string, bool>();
        static readonly List<IWorkSurfaceKey> _resourceCurrentlyWaitingForWaterMarkUpdates = new List<WorkSurfaceKey>();
        
        public static void AddWorkflow(IWorkSurfaceKey workSurfaceKey)
        {
            _resourcesCurrentlyInOpeningState.Add(workSurfaceKey);
            _resourcesCurrentlyInOpeningStateWaitingForLoad.Add(workSurfaceKey);
        }
        
        public static void RemoveWorkflow(IWorkSurfaceKey workSurfaceKey)
        {
            _resourcesCurrentlyInOpeningState.Remove(workSurfaceKey);
        }

        public static void RemoveWorkflowWaitingForDesignerLoad(IWorkSurfaceKey workSurfaceKey)
        {
            _resourcesCurrentlyInOpeningStateWaitingForLoad.Remove(workSurfaceKey);
        }
        
        public static List<IWorkSurfaceKey> FetchOpeningKeys() => _resourcesCurrentlyInOpeningState;
        
        public static bool IsWorkflowWaitingforDesignerLoad(IWorkSurfaceKey workSurfaceKey) => _resourcesCurrentlyInOpeningStateWaitingForLoad.Contains(workSurfaceKey);
        
        public static void RemoveWorkflowWaitingForFirstFocusLoss(IWorkSurfaceKey workSurfaceKey)
        {
            _resourcesCurrentlyWaitingForFirstFocusLoss[workSurfaceKey.ToString()] = false;
        }
        
        public static bool IsWaitingForFistFocusLoss(IWorkSurfaceKey workSurfaceKey)
        {
            if(_resourcesCurrentlyWaitingForFirstFocusLoss.ContainsKey(workSurfaceKey.ToString()))
            {
                return _resourcesCurrentlyWaitingForFirstFocusLoss[workSurfaceKey.ToString()];
            }

            return false;
        }
        
        public static void PruneWorkflowFromCaches(IWorkSurfaceKey workSurfaceKey)
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
