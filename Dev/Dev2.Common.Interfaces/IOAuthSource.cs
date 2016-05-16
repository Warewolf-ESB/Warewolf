using System;

namespace Dev2.Common.Interfaces
{
    public interface IOAuthSource : IEquatable<IOAuthSource>
    {
        string AppKey { get; set; }
        string AccessToken { get; set; }
    }
}