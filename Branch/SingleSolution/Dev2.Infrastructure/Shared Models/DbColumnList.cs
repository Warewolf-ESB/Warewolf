
using System;

namespace Dev2.Runtime.ServiceModel.Data
{
    public class DbColumnList : ResultList<DbColumn>
    {
        public DbColumnList()
        {
        }

        public DbColumnList(string errorFormat, params object[] args)
            : base(errorFormat, args)
        {
        }

        public DbColumnList(Exception ex)
            : base(ex)
        {
        }
    }
}
