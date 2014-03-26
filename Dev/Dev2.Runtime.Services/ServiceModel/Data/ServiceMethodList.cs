using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Dev2.Runtime.ServiceModel.Data
{
    [Serializable]
    public class ServiceMethodList : List<ServiceMethod>
    {
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
