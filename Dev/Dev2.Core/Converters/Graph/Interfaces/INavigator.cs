using System;
using System.Collections.Generic;

// ReSharper disable CheckNamespace
namespace Unlimited.Framework.Converters.Graph.Interfaces
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
