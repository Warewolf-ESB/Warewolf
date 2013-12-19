using System.Collections.Generic;
using System.Collections.Specialized;

namespace Dev2.Collections
{
    public interface IObservableReadOnlyList<out T> : IReadOnlyList<T>, INotifyCollectionChanged
    {
    }
}
