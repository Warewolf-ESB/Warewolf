using System;

namespace Dev2.Common.Interfaces
{
    public interface IChatCompletionsSource : IEquatable<IChatCompletionsSource>
    {
        string ApiKey { get; set; }
        string CompletionsEndpoint { get; set; }
    }
}
