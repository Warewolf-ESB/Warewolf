using System;
using System.Collections.Generic;

// ReSharper disable CheckNamespace
namespace Dev2.Common.Interfaces.Core.Graph
// ReSharper restore CheckNamespace
{
    public interface INavigator : IDisposable
    {
        object Data { get; }
        object SelectScalar(IPath path);
        IEnumerable<object> SelectEnumerable(IPath path);
        Dictionary<IPath, IList<object>> SelectEnumerablesAsRelated(IList<IPath> paths);
    }
}
