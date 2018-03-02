using Dev2.Data.Interfaces;
using System.Collections.Generic;
using System.IO;

namespace Dev2.Data.PathOperations.Interfaces
{
    public class IPutOperationArguments
    {
        public Stream Src { get; set; }
        public IActivityIOPath Dst { get; set; }
        public IDev2CRUDOperationTO Args{ get; set; }
        public string WhereToPut { get; set; }
        public List<string> FilesToCleanup{ get; set; }
    }
}
