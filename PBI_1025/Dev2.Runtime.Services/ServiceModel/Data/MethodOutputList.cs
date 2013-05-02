using System.Collections.Generic;
using Newtonsoft.Json;
using Unlimited.Framework.Converters.Graph.Interfaces;

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
