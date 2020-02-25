using System;
using Warewolf.Options;

namespace Warewolf.Data.Decisions.Operations
{
    /// <summary>
    /// Is Base64 Decision
    /// </summary>
    public class IsNull : IDecisionOperation
    {
        public bool Invoke(string[] cols)
        {
            if(cols == null)
            {
                return false;
            }
            return cols[0] == null;
        }

        public Enum HandlesType() => enDecisionType.IsNull;
    }
}