using System;
using Dev2.Common.Interfaces.Patterns;

namespace Dev2.Data.Decisions.Operations
{
    /// <summary>
    /// A common interface that all decision classes must extend ;)
    /// </summary>
    public interface IDecisionOperation : ISpookyLoadable<Enum>
    {

        bool Invoke(string[] cols);

    }
}
 