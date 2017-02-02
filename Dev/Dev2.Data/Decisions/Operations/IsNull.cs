using System;

namespace Dev2.Data.Decisions.Operations
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

        public Enum HandlesType()
        {
            return enDecisionType.IsNull;
        }
    }
}