using Dev2.Runtime.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dev2.Runtime.Hosting
{
    public class ResourceCatalogFactory : IResourceCatalogFactory
    {
        public IResourceCatalog New()
        {
            return ResourceCatalog.Instance;
        }
    }
}
