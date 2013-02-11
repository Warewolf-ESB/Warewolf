using System.Collections.Generic;
using Newtonsoft.Json;

namespace Dev2.Runtime.ServiceModel.Data
{
    public class MethodParameterList : List<MethodParameter>
    {
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
