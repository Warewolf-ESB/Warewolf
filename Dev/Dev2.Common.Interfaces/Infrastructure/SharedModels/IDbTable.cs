using System.Collections.Generic;

namespace Dev2.Common.Interfaces.Infrastructure.SharedModels
{
    public interface IDbTable
    {
        string Schema { get; set; }
        string TableName { get; set; }
        string FullName { get; }
        List<IDbColumn> Columns { get; set; }
    }
}