using System.Collections.Generic;
using Newtonsoft.Json;

namespace Dev2.Runtime.ServiceModel.Data
{
    public class HelpDictionary : Dictionary<string, object>
    {
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
