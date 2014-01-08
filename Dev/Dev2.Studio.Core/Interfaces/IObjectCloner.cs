using System.Collections.ObjectModel;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core
{
    public interface IObjectCloner<T>
    {

        ObservableCollection<T> CloneObservableCollection(ObservableCollection<T> src);
    }
}
