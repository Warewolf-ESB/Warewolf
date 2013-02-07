using System.Collections.Generic;

namespace Dev2.Runtime.ServiceModel.Data
{
    internal class ResourceIteratorResult
    {
        public ResourceIteratorResult()
        {
            Values = new Dictionary<int, string>();
        }

        public string Content { get; set; }

        public Dictionary<int, string> Values { get; private set; }
    }
}