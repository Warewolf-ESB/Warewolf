using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces.Core.Graph;

namespace Dev2.Converters.Graph.DataTable
{
    public class DataTableNavigator : INavigator
    {
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        // ReSharper disable UnusedAutoPropertyAccessor.Local
        public object Data { get; private set; }
        // ReSharper restore UnusedAutoPropertyAccessor.Local
        public object SelectScalar(IPath path)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<object> SelectEnumerable(IPath path)
        {
            throw new NotImplementedException();
        }

        public Dictionary<IPath, IList<object>> SelectEnumerablesAsRelated(IList<IPath> paths)
        {
            throw new NotImplementedException();
        }
    }
}
