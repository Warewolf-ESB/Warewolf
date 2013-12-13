using System.Collections.Generic;

namespace Dev2.Runtime.ServiceModel.Data
{
    public class DbTable
    {
        public string TableName { get; set; }

        public List<DbColumn> Columns { get; set; }
    }
}