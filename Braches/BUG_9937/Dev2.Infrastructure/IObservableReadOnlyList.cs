using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Dev2
{
    public interface IObservableReadOnlyList<out T> : IReadOnlyList<T>, INotifyCollectionChanged
    {
    }
}
