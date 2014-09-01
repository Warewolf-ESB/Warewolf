using System;
using Dev2.Common.Interfaces.Infrastructure.SharedModels;

namespace Dev2.Runtime.ServiceModel.Data
{
    public class DbColumnList : ResultList<IDbColumn>, IDbColumnList
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
