using System.Collections.Generic;
using Dev2.Common;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;

namespace Dev2.Activities.Designers2.Core
{
    public class DropboxSourceManager : IDropboxSourceManager
    {
        private readonly IEnvironmentModel _targetEnvironment;

        public DropboxSourceManager(IEnvironmentModel targetEnvironment)
        {
            _targetEnvironment = targetEnvironment;
        }

        public DropboxSourceManager()
            : this(EnvironmentRepository.Instance.ActiveEnvironment)
        {

        }

        #region Implementation of ISourceManager

        public IEnumerable<T> FetchSources<T>() where T : new()
        {
            var resourceList = _targetEnvironment.ResourceRepository.GetResourceList<T>(_targetEnvironment, GlobalConstants.ServerWorkspaceID);
            return resourceList;
        }

        #endregion
    }

    public interface IDropboxSourceManager : ISourceManager
    {
        IEnumerable<T> FetchSources<T>() where T : new();
    }

    public interface ISourceManager
    {
       
    }
}
