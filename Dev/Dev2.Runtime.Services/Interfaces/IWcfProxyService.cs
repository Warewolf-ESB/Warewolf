using System.Collections.Generic;
using System.Reflection;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Runtime.ServiceModel.Data;

namespace Dev2.Runtime.Interfaces
{
    public interface IWcfProxyService
    {
        IOutputDescription ExecuteWebService(WcfService src);

        object ExcecuteMethod(IWcfAction action, string endpointUrl);

        Dictionary<MethodInfo, ParameterInfo[]> GetMethods(string endpoint);
    }
}
