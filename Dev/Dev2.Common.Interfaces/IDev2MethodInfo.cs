using System;
using System.Collections.Generic;

namespace Dev2.Common.Interfaces
{
    public interface IDev2MethodInfo
    {
        string Method { get; set; }
        List<IMethodParameter> Parameters { get; set; }
        string MethodResult { get; set; }
        string OutputVariable { get; set; }
        bool IsObject { get; set; }
        bool IsVoid { get; set; }
        Guid ID { get; set; }
        bool HasError { get; set; }
        string ErrorMessage { get; set; }
        bool IsProperty { get; set; }
    }
}