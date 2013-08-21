using System.Collections.Generic;
using Newtonsoft.Json;

namespace Dev2.Runtime.ServiceModel.Data
{
    public class NamespaceList : List<NamespaceItem>
    {
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    public class NamespaceItem
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