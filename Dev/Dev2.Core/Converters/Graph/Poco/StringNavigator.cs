using System.Collections.Generic;
using Dev2.Common.Interfaces.Core.Graph;


namespace Unlimited.Framework.Converters.Graph.Poco
    
{
    public class StringNavigator : INavigator {
        readonly object _data;

        public StringNavigator(object data)
        {
            _data = data;
        }

        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
        }

        #endregion

        #region Implementation of INavigator

        public object Data => _data;

        public object SelectScalar(IPath path)
        {
            return _data.ToString();
        }

        public IEnumerable<object> SelectEnumerable(IPath path)
        {
            yield break;
        }

        public Dictionary<IPath, IList<object>> SelectEnumerablesAsRelated(IList<IPath> paths)
        {
            return new Dictionary<IPath, IList<object>>();
        }

        #endregion
    }
}