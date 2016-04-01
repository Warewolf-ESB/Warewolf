using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.ToolBase.ExchangeEmail;

namespace Dev2.Common.Interfaces.Core
{
    public class ExchangeSourceDefinition : IExchangeSource
    {
        public bool Equals(IExchangeSource other)
        {
            return string.Equals(Name, other.Name) && ResourceID.Equals(other.ResourceID) && string.Equals(Path, other.Path);
        }

        public Guid ResourceID { get; set; }
        public string Name { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public enSourceType Type { get; set; }
        public string Path { get; set; }
        public int Timeout { get; set; }
    }
}
