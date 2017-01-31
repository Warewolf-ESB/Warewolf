using System;
using Dev2.Common.Interfaces;

namespace Dev2.Runtime.ServiceModel.Data
{
    /// <summary>
    /// A Method Parameter
    /// </summary>
    [Serializable]
    public class ConstructorParameter : MethodParameter, IConstructorParameter
    {
    }
}