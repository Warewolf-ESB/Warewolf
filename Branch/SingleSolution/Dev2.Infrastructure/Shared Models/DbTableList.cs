
using System;

namespace Dev2.Runtime.ServiceModel.Data
{
    public class DbTableList : ResultList<DbTable>
    {
        public DbTableList()
        {
        }

        public DbTableList(string errorFormat, params object[] args)
            : base(errorFormat, args)
        {
        }

        public DbTableList(Exception ex)
            : base(ex)
        {
        }
    }
}
