using System;

namespace Dev2.Runtime.ServiceModel.Data
{
    /// <summary>
    /// A Method Parameter
    /// </summary>
    [Serializable]
    public class ConstructorParameter : MethodParameter
    {
        public string ShortTypeName { get; set; }
    }
}