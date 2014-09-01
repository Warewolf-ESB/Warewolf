using System;

namespace Dev2.Common.Interfaces.Hosting
{
    public interface IHostContext
    {
        Guid AccountID { get; }
        Guid SessionID { get; }
    }
}
