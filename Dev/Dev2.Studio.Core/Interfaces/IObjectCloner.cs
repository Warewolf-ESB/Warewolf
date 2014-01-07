using System.Collections.ObjectModel;

namespace Dev2.Studio.Core
{
    public interface IObjectCloner<T>
    {

        ObservableCollection<T> CloneObservableCollection(ObservableCollection<T> src);
    }
}
