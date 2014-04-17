using System.Collections.Specialized;

// ReSharper disable CheckNamespace
namespace Dev2.Studio.Core.Interfaces
// ReSharper restore CheckNamespace
{
    public interface IDataMappingViewModelFactory
    {
        IDataMappingViewModel CreateModel(IWebActivity activity,
                                          NotifyCollectionChangedEventHandler mappingCollectionChangedEventHandler =
                                              null);
    }
}
