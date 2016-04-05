using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dev2.Common.Interfaces
{
    public interface IExchangeServiceSource : IEquatable<IExchangeServiceSource>
    {
        string AutoDiscoverUrl { get; set; }
        string UserName { get; set; }
        string Password { get; set; }
        int Timeout { get; set; }
        string EmailFrom { get; set; }
        string EmailTo { get; set; }
        string Path { get; set; }
        Guid Id { get; set; }
        string ResourceName { get; set; }
    }
}
