using System;
using System.Data;

namespace Dev2.Common.Interfaces.Infrastructure.SharedModels
{
    public interface IDbColumn
    {
        string ColumnName { get; set; }
        bool IsNullable { get; set; }
        Type DataType { get; set; }
        bool IsAutoIncrement { get; set; }
        SqlDbType SqlDataType { get; set; }
        int MaxLength { get; set; }
        string DataTypeName { get; }
        string SystemDataType { get; }
    }
}