
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Common;

namespace Dev2.Runtime.Execution
{
    public class ExecutableServiceRepository
    {
        #region fields

        private readonly List<IExecutableService> _activeExecutions;

        #endregion

        #region singleton
        private static readonly Lazy<ExecutableServiceRepository> _instance
            = new Lazy<ExecutableServiceRepository>(() => new ExecutableServiceRepository());

        private ExecutableServiceRepository()
        {
            _activeExecutions = new List<IExecutableService>();
        }

        public static ExecutableServiceRepository Instance
        {
            get
            {
                return _instance.Value;
            }
        }
        #endregion

        public int Count
        {
            get { return _activeExecutions.Count; }
        }

        public bool DoesQueueHaveSpace()
        {
            return Count <= GlobalConstants.MaxWorkflowsToExecute;
        }

        public void Add(IExecutableService service)
        {
            ClearNullExecutions();
            var parent = GetParent(service.WorkspaceID, service.ParentID);
            if(parent == null)
            {
                _activeExecutions.Add(service);
            }
            else
            {
                parent.AssociatedServices.Add(service);
            }
        }

        void ClearNullExecutions()
        {
            if(_activeExecutions != null && _activeExecutions.Count>0)
            {
                for (var i = _activeExecutions.Count-1; i>=0; i--)
                {
                    if (_activeExecutions[i] == null)
                    {
                        _activeExecutions.RemoveAt(i);
                    }
                }
            }
        }

        public bool Remove(IExecutableService service)
        {
            var exists = _activeExecutions.Remove(service);
                
            foreach(var executableService in _activeExecutions)
            {
                exists = executableService.AssociatedServices.Remove(service);
                if(exists)
                {
                    return true;
                }
            }

            return exists;
        }

        private IExecutableService GetParent(Guid workspaceID, Guid parentID)
        {
            var service = _activeExecutions.FirstOrDefault(e => e!=null &&e.ID == parentID && e.WorkspaceID == workspaceID);
            return service;
        }

        public IExecutableService Get(Guid workspaceID, Guid id)
        {
            var service = _activeExecutions.FirstOrDefault(e => e.ID == id && e.WorkspaceID == workspaceID);
            return service;
        }

        public void Clear()
        {
            _activeExecutions.Clear();
        }
    }
}
