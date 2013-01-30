using System;
using System.IO;
using Dev2.Common.ExtMethods;

namespace Dev2.Data.Decisions.Operations
{
    public class IsNotEmail : IDecisionOperation
    {
        public bool Invoke(string[] cols)
        {
            //if (cols.Length < 1 || cols.Length > 1)
            //{
            //    throw new InvalidDataException("Wrong number of columns sent");
            //}

            return !(cols[0].IsEmail());
        }

        public Enum HandlesType()
        {
            return enDecisionType.IsNotEmail;
        }
    }
}
