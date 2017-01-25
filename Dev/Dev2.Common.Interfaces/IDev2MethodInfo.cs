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
    }
}