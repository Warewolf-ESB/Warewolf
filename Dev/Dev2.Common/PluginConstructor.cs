using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces;

namespace Dev2.Common
{
    [Serializable]
    public class PluginConstructor : IPluginConstructor
    {
        public PluginConstructor()
        {
            Inputs = new List<IConstructorParameter>();
        }
        #region Implementation of IConstructor

        public IList<IConstructorParameter> Inputs { get; set; }
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