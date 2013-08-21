using System;

namespace Dev2.Hosting
{
    public interface IHostContext
    {
        Guid AccountID { get; }
        Guid SessionID { get; }
    }
}
