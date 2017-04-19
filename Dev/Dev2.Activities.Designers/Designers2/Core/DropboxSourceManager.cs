using System.Collections.Generic;
using Dev2.Studio.Core;
using Dev2.Studio.Interfaces;

namespace Dev2.Activities.Designers2.Core
{
    public class DropboxSourceManager : IDropboxSourceManager
    {
        private readonly IServer _targetEnvironment;

        public DropboxSourceManager(IServer targetEnvironment)
        {
            _targetEnvironment = targetEnvironment;
        }

        public DropboxSourceManager()
            : this(ServerRepository.Instance.ActiveServer)
        {

        }

        #region Implementation of ISourceManager

        public IEnumerable<T> FetchSources<T>() where T : new()
        {
            var resourceList = _targetEnvironment.ResourceRepository.GetResourceList<T>(_targetEnvironment);
            return resourceList;
        }

        #endregion
    }

    public interface IDropboxSourceManager : ISourceManager
    {
       
    }

    public interface ISourceManager
    {
        IEnumerable<T> FetchSources<T>() where T : new();
    }
}
