using System;
using System.IO;
using Dev2.DataList.Contract;

namespace Dev2.Data.Decisions.Operations
{
    public class IsXml : IDecisionOperation
    {
        public Enum HandlesType()
        {
            return enDecisionType.IsXML;
        }

        public bool Invoke(string[] cols)
        {
            //if (cols.Length < 1 || cols.Length > 1)
            //{
            //    throw new InvalidDataException("Wrong number of columns sent");
            //}

            return DataListUtil.IsXml(cols[0]);
        }
    }
}
