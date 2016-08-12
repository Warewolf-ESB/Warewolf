using System.Collections.Generic;

namespace Dev2.Runtime
{
    public class FileResource
    {
        public string ResourcePath { get; set; }
        public string ParentPath { get; set; }
        public string ResourceName { get; set; }
        public List<FileResource> Children { get; set; }
    }
}