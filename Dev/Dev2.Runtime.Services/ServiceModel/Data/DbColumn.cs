using System;
using System.Data;

namespace Dev2.Runtime.ServiceModel.Data
{
    public class DbColumn
    {
        public DbColumn()
        {
        }

        public DbColumn(DataColumn dataColumn)
        {
            ColumnName = dataColumn.ColumnName;
            DataType = dataColumn.DataType;
            MaxLength = dataColumn.MaxLength;
        }

        public string ColumnName { get; set; }

        public Type DataType { get; set; }

        public int MaxLength { get; set; }
    }
}