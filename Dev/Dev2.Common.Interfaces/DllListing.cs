using System.Collections.Generic;

namespace Dev2.Common.Interfaces
{
    public class DllListing
    {
        public string Name { get; set; }
        public List<DllListing> Children { get; set; }
        public string FullName { get; set; }
    }
}