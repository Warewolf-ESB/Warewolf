using System.Collections.Generic;
using Dev2.Common.Interfaces;

namespace Dev2.Common
{
    public class FileResource : IFileResource
    {
        public string ResourcePath { get; set; }
        public string ParentPath { get; set; }
        public string ResourceName { get; set; }
        public List<IFileResource> Children { get; set; }
    }
}