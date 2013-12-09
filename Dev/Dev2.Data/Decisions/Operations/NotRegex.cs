using System;
using System.Text.RegularExpressions;

namespace Dev2.Data.Decisions.Operations
{
    public class NotRegEx : IDecisionOperation
    {
        public bool Invoke(string[] cols)
        {
            return !Regex.IsMatch(cols[0], cols[1]);
        }

        public Enum HandlesType()
        {
            return enDecisionType.NotRegEx;
        }
    }
}
