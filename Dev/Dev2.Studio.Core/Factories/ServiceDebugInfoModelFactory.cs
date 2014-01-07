using Dev2.Studio.Core.AppResources;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Models;

namespace Dev2.Studio.Core.Factories
{
    public static class ServiceDebugInfoModelFactory
    {
        public static IServiceDebugInfoModel CreateServiceDebugInfoModel(IContextualResourceModel resourceModel, string serviceInputData, DebugMode debugSetting)
        {
            IServiceDebugInfoModel serviceDebugInfoModel = new ServiceDebugInfoModel();
            serviceDebugInfoModel.ResourceModel = resourceModel;
            serviceDebugInfoModel.DebugModeSetting = debugSetting;
            serviceDebugInfoModel.ServiceInputData = serviceInputData;
            serviceDebugInfoModel.RememberInputs = true;
            return serviceDebugInfoModel;
        }
    }
}
