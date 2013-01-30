using Dev2.Common;

namespace Dev2.Data.Decisions.Operations
{
    /// <summary>
    /// A common interface that all decision classes must extend ;)
    /// </summary>
    public interface IDecisionOperation : ISpookyLoadable
    {

        bool Invoke(string[] cols);

    }
}
 