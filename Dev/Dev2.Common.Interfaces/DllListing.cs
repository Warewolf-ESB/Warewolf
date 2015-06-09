using System.Collections.Generic;

namespace Dev2.Common.Interfaces
{
    public class DllListing : IDllListing
    {
        public string Name { get; set; }
        public ICollection<IDllListing> Children { get; set; }
        public string FullName { get; set; }
        public bool IsDirectory { get; set; }
    }
}