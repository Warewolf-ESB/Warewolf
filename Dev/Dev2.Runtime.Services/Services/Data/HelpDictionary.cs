using Newtonsoft.Json;
using System.Collections.Generic;

namespace Dev2.Runtime.Services.Data
{
    public class HelpDictionary : Dictionary<string, object>
    {
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
