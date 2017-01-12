using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Dev2.Runtime.ServiceModel.Data
{
    [Serializable]
    public class ServiceConstructorList : List<ServiceConstructor>
    {
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}