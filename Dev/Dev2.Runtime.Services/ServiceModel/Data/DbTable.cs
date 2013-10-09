using System.Collections.Generic;

namespace Dev2.Runtime.ServiceModel.Data
{
    public class DbTable
    {
        readonly List<DbColumn> _columns = new List<DbColumn>();

        public string TableName { get; set; }

        public List<DbColumn> Columns { get { return _columns; } }
    }
}