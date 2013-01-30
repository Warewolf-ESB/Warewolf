using System;
using Dev2.DataList.Contract;
namespace Dev2.DynamicServices {
    public interface IDynamicServicesInvoker {

        IDynamicServicesHost Host { get; set; }

        Guid Invoke(IDynamicServicesHost resourceDirectory, dynamic xmlRequest, Guid dataListId, out ErrorResultTO error);

        Guid Invoke(DynamicService service, dynamic xmlRequest, Guid dataListId, out ErrorResultTO errors);
    }
}
