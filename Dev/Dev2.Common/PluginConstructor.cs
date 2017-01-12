using System.Collections.Generic;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.DB;

namespace Dev2.Common
{
    public class PluginConstructor : IPluginConstructor
    {
        #region Implementation of IConstructor

        public IList<IServiceInput> Inputs { get; set; }
        public string ReturnType { get; set; }
        public IList<INameValue> Variables { get; set; }
        public string ConstructorName { get; set; }

        public string GetIdentifier()
        {
            return ConstructorName;
        }

        #endregion
    }
}