using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dev2.Studio.Core.Interfaces;

// ReSharper disable CheckNamespace
namespace  Dev2.Studio.Core.Interfaces
// ReSharper restore CheckNamespace
{
    public interface IDataMappingViewModelFactory
    {
        IDataMappingViewModel CreateModel(IWebActivity activity,
                                          NotifyCollectionChangedEventHandler mappingCollectionChangedEventHandler =
                                              null);
    }
}
