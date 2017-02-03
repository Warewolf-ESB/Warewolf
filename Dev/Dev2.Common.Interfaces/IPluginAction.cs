using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces.DB;

namespace Dev2.Common.Interfaces
{
    public interface IPluginAction
    {
        string FullName { get; set; }
        string Method { get; set; }
        Guid ID { get; set; }
        IList<IServiceInput> Inputs { get; set; }
        Type ReturnType { get; set; }
        IList<INameValue> Variables { get; set; }
        string Dev2ReturnType { get; set; }
        string GetIdentifier();
        string MethodResult { get; set; }
        string OutputVariable { get; set; }
        bool IsObject { get; set; }
        bool IsVoid { get; set; }
        string ErrorMessage { get; set; }
        bool HasError { get; set; }
        bool IsProperty { get; set; }
    }
}
