using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Studio.Core.AppResources;
using Dev2.Studio.Core.Interfaces;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using Dev2.Studio.Core.Models;

namespace Dev2.Studio.Core.Factories {
    public static class ServiceDebugInfoModelFactory
    {
        public static IServiceDebugInfoModel CreateServiceDebugInfoModel(IContextualResourceModel resourceModel, string serviceInputData, int waitTransitionTime, DebugMode debugSetting) 
        {
            IServiceDebugInfoModel serviceDebugInfoModel = new ServiceDebugInfoModel();
            serviceDebugInfoModel.ResourceModel = resourceModel;
            serviceDebugInfoModel.DebugModeSetting = debugSetting;
            serviceDebugInfoModel.WaitTimeForTransition = waitTransitionTime;
            serviceDebugInfoModel.ServiceInputData = serviceInputData;
            return serviceDebugInfoModel;
        }
    }
}
