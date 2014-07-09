using System.Collections.Generic;

namespace Tu.Rules
{
    public interface IRule
    {
        List<string> Errors { get; }

        bool IsValid(object value);
    }
}