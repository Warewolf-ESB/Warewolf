/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

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

        public static ExecutableServiceRepository Instance => _instance.Value;

        #endregion

        public int Count => _activeExecutions.Count;

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
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

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
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

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public void Clear()
        {
            _activeExecutions.Clear();
        }
    }
}
