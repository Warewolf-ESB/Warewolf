using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.DB;

namespace Dev2.Common
{
    [Serializable]
    public class PluginConstructor : IPluginConstructor
    {
        public PluginConstructor()
        {
            Inputs = new List<IServiceInput>();
        }
        #region Implementation of IConstructor

        public IList<IServiceInput> Inputs { get; set; }
        public string ReturnObject { get; set; }
        public string ConstructorName { get; set; }

        public string GetIdentifier()
        {
            return ConstructorName;
        }

        #endregion

        #region Implementation of IPluginConstructor

        public bool IsExistingObject { get; set; }

        #endregion
    }
}