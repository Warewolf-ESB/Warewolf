using System;
using System.Collections.Generic;
using System.Emission.Meta;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace System.Emission.Collectors
{
    internal sealed class NetworkInterfaceProxyMemberCollector : EmissionMemberCollector
    {
        public NetworkInterfaceProxyMemberCollector(string dynamicAssemblyName, Type interfaceType)
            : base(interfaceType, dynamicAssemblyName)
        {
        }

        protected override MetaMethod GetMethodToGenerate(MethodInfo method, IEmissionProxyHook hook, bool isStandalone, MetaMethodSource source)
        {
            if (method.IsAccessible(DynamicAsssemblyName) == false)
            {
                return null;
            }

            var proxyable = AcceptMethod(method, false, hook);
            return new MetaMethod(DynamicAsssemblyName, method, method, isStandalone, proxyable, false, source);
        }
    }
}
