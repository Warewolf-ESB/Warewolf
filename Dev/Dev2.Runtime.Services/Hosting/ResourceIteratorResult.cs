using System.Collections.Generic;

namespace Dev2.Runtime.Hosting
{
    public class ResourceIteratorResult
    {
        public ResourceIteratorResult()
        {
            Values = new Dictionary<int, string>();
        }

        public string Content { get; set; }

        public Dictionary<int, string> Values { get; private set; }
    }
}