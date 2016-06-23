

using System;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Dev2.Runtime.DynamicProxy
{
    public class DynamicProxy : DynamicObject
    {
        public DynamicProxy(Type proxyType, Binding binding, 
                EndpointAddress address)
            : base(proxyType)
        {
            Type[] paramTypes = new Type[2];
            paramTypes[0] = typeof(Binding);
            paramTypes[1] = typeof(EndpointAddress);

            object[] paramValues = new object[2];
            paramValues[0] = binding;
            paramValues[1] = address;

            CallConstructor(paramTypes, paramValues);
        }

        public Type ProxyType => ObjectType;

        public object Proxy => ObjectInstance;
    }
}
