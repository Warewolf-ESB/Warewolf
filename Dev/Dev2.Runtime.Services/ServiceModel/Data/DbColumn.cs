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

        public SqlDbType SqlDataType { get; set; }

        public int MaxLength { get; set; }

        public string DataTypeName
        {
            get
            {
                if(DataType == null)
                {
                    return string.Empty;
                }

                // TODO: Implement Mapping CLR Parameter Data: http://technet.microsoft.com/en-us/library/ms131092.aspx
                if(SqlDataType == SqlDbType.VarChar)
                {
                    return string.Format("{0} ({1})", SqlDataType, MaxLength);
                }

                return SqlDataType.ToString();

            }
        }
    }
}