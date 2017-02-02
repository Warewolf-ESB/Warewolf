using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces.DB;

namespace Dev2.Common.Interfaces
{
    public interface IWcfAction
    {
        string FullName { get; set; }
        string Method { get; set; }
        IList<IServiceInput> Inputs { get; set; }
        Type ReturnType { get; set; }
        IList<INameValue> Variables { get; set; }
        string GetHashCodeBySource();
    }

    public class WcfAction : IWcfAction
    {
        public string FullName { get; set; }
        public string Method { get; set; }
        public IList<IServiceInput> Inputs { get; set; }
        public Type ReturnType { get; set; }
        public IList<INameValue> Variables { get; set; }
        public string GetHashCodeBySource()
        {
            return FullName + Method;
        }
    }
}
