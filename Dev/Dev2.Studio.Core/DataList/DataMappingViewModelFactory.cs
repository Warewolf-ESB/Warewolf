using System.Collections.Specialized;
using Dev2.Studio.Core.Interfaces;

namespace Dev2.Studio.ViewModels.DataList
{
    public class DataMappingViewModelFactory:IDataMappingViewModelFactory
    {
        public IDataMappingViewModel CreateModel(IWebActivity activity,
                                                 NotifyCollectionChangedEventHandler mappingCollectionChangedEventHandler = null)
        {
            return  new DataMappingViewModel(activity,mappingCollectionChangedEventHandler);
        }
    }
}