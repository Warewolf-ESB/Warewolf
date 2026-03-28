using System;
using System.Runtime.Serialization;
using Dev2.Common.Interfaces;

namespace Dev2.Runtime.ServiceModel.Data
{
    /// <summary>
    /// A Method Parameter
    /// </summary>
    [DataContract]
    public class ConstructorParameter : MethodParameter, IConstructorParameter
    {
    }
}