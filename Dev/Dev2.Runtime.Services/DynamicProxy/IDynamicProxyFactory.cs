using System;
using System.Collections.Generic;
using System.ServiceModel.Description;

namespace Dev2.Runtime.DynamicProxy
{
    public interface IDynamicProxyFactory
    {
        IDynamicProxy CreateProxy(string contractName);
        IEnumerable<ContractDescription> Contracts { get; set; }
        Type ProxyType { get; set; }
        object CallMethod(string method, params object[] parameters);
    }
}
