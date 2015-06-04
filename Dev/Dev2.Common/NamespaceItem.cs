using System;
using Dev2.Common.Interfaces;
using Newtonsoft.Json;

namespace Dev2.Common
{
    [Serializable]
    public class NamespaceItem : INamespaceItem
    {
        #region ToString

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        #endregion

        public string AssemblyLocation { get; set; }

        public string AssemblyName { get; set; }

        public string FullName { get; set; }

        public string MethodName { get; set; }
    }
}