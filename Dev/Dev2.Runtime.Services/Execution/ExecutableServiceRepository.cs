using System;
using System.Collections.Generic;
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

        public void Add(IExecutableService service)
        {
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
            var service = _activeExecutions.ToList().FirstOrDefault(e => e.ID == parentID && e.WorkspaceID == workspaceID);
            return service;
        }

        public IExecutableService Get(Guid workspaceID, Guid id)
        {
            var service = _activeExecutions.ToList().FirstOrDefault(e => e.ID == id && e.WorkspaceID == workspaceID);
            return service;
        }

        public void Clear()
        {
            _activeExecutions.Clear();
        }
    }
}
