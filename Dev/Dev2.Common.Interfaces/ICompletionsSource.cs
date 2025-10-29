using System;

namespace Dev2.Common.Interfaces
{
    public interface ICompletionsSource : IEquatable<ICompletionsSource>
    {
        string ApiKey { get; set; }
        string CompletionsEndpoint { get; set; }
    }
}
