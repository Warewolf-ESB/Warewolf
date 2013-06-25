using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Dev2.Data.Decisions.Operations
{
    public class IsRegEx : IDecisionOperation
    {
        public bool Invoke(string[] cols)
        {
            //if (cols.Length < 2 || cols.Length > 2)
            //{
            //    throw new InvalidDataException("Wrong number of columns sent");
            //}

            return Regex.IsMatch(cols[0], cols[1]);
        }

        public Enum HandlesType()
        {
            return enDecisionType.IsRegEx;
        }
    }
}
