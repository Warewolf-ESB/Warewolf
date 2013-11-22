using System;
using System.Collections.Generic;
using Unlimited.Framework.Converters.Graph;
using Unlimited.Framework.Converters.Graph.Interfaces;

namespace Dev2.Converters.Graph.DataTable
{
    [Serializable]
    public class DataTablePath : BasePath
    {

        public DataTablePath(string tblName, string colName)
        {
            var tmp = colName;
            
            if (!string.IsNullOrEmpty(tblName))
            {
                tmp = string.Concat(tblName, "().", colName);
            }

            DisplayPath = tmp;
            ActualPath = tmp;
        }

        public override IEnumerable<IPathSegment> GetSegements()
        {
            throw new NotImplementedException();
        }

        public override IPathSegment CreatePathSegment(string pathSegmentString)
        {
            throw new NotImplementedException();
        }
    }
}
