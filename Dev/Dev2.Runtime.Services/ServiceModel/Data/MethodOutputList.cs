using System.Collections.Generic;
using Dev2.Common.Interfaces.Core.Graph;
using Newtonsoft.Json;

namespace Dev2.Runtime.ServiceModel.Data
{
    public class MethodOutputList : List<IPath>
    {
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
