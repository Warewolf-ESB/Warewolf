using System;

namespace Dev2.Data.Decisions.Operations
{
    /// <summary>
    /// Is Base64 Decision
    /// </summary>
    public class IsNotNull : IDecisionOperation
    {
        public bool Invoke(string[] cols) => cols?[0] != null;

        public Enum HandlesType() => enDecisionType.IsNotNull;
    }
}