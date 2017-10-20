using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Runtime.ServiceModel.Data;
using System.Collections.Generic;

namespace Dev2.Runtime.Interfaces
{
    public interface IPluginBroker<in T, in TS> where T : IResourceSource
        where TS : Service
    {
        List<NamespaceItem> GetNamespaces(T pluginSource);
        ServiceMethodList GetMethods(string assemblyLocation, string assemblyName, string fullName);
        ServiceMethodList GetMethodsWithReturns(string assemblyLocation, string assemblyName, string fullName);
        ServiceConstructorList GetConstructors(string assemblyLocation, string assemblyName, string fullName);
        IOutputDescription TestPlugin(TS pluginService);
    }

    public interface ICOMPluginBroker<in T, in TS> where T : IResourceSource
       where TS : Service
    {
        List<NamespaceItem> GetNamespaces(T pluginSource);
        ServiceMethodList GetMethods(string clsid,bool is32Bit);
        IOutputDescription TestPlugin(TS pluginService);
    }
}