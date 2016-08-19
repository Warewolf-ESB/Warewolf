using System.Collections.Generic;

namespace Dev2.Common.Interfaces
{
    public interface IFileResource
    {
        string ResourcePath { get; set; }
        string ParentPath { get; set; }
        string ResourceName { get; set; }
        List<IFileResource> Children { get; set; }
    }
}