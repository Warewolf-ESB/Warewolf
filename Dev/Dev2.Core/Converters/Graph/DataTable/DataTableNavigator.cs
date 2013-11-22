using System;
using System.Collections.Generic;
using Unlimited.Framework.Converters.Graph.Interfaces;

namespace Dev2.Converters.Graph.DataTable
{
    public class DataTableNavigator : INavigator
    {
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public object Data { get; private set; }
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
