
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Data;
using Dev2.Common.Interfaces.Infrastructure.SharedModels;

namespace Dev2.Runtime.ServiceModel.Data
{
    public class DbColumn : IDbColumn
    {
        SqlDbType _sqlDataType;

        public DbColumn()
        {
        }

        public DbColumn(DataColumn dataColumn)
        {
            ColumnName = dataColumn.ColumnName;
            DataType = dataColumn.DataType;
            MaxLength = dataColumn.MaxLength;
            IsNullable = dataColumn.AllowDBNull;
            IsAutoIncrement = dataColumn.AutoIncrement;
        }

        public string ColumnName { get; set; }

        public bool IsNullable { get; set; }

        public Type DataType { get; set; }

        public bool IsAutoIncrement { get; set; }

        public SqlDbType SqlDataType
        {
            get
            {
                return _sqlDataType;
            }
            set
            {
                _sqlDataType = value;
                DataType = ConvertSqlDbType(value);
            }
        }

        public int MaxLength { get; set; }

        public string DataTypeName
        {
            get
            {
                if(SqlDataType == SqlDbType.VarChar || SqlDataType == SqlDbType.Char || SqlDataType == SqlDbType.NVarChar || SqlDataType == SqlDbType.NChar || SqlDataType == SqlDbType.VarBinary || SqlDataType == SqlDbType.Binary)
                {
                    return string.Format("{0} ({1})", SqlDataType, MaxLength).ToLower();
                }

                return SqlDataType.ToString().ToLower();

            }
        }

        public string SystemDataType
        {
            get
            {
                if(DataType == null)
                {
                    return string.Empty;
                }

                // TODO: Implement Mapping CLR Parameter Data: http://technet.microsoft.com/en-us/library/ms131092.aspx
                if(DataType == typeof(string))
                {
                    return string.Format("{0}({1})", DataType.Name, MaxLength);
                }

                return DataType.Name;

            }
        }

        public static Type ConvertSqlDbType(SqlDbType sqlDbType)
        {
            // http://msdn.microsoft.com/en-us/library/system.data.sqldbtype.aspx
            switch(sqlDbType)
            {
                case SqlDbType.BigInt:
                    return typeof(long);
                case SqlDbType.Binary:
                    return typeof(byte[]);
                case SqlDbType.Bit:
                    return typeof(bool);
                case SqlDbType.Char:
                    return typeof(char);
                case SqlDbType.DateTime:
                    return typeof(DateTime);
                case SqlDbType.Decimal:
                    return typeof(decimal);
                case SqlDbType.Float:
                    return typeof(double);
                case SqlDbType.Image:
                    return typeof(byte[]);
                case SqlDbType.Int:
                    return typeof(int);
                case SqlDbType.Money:
                    return typeof(decimal);
                case SqlDbType.NChar:
                    return typeof(string);
                case SqlDbType.NText:
                    return typeof(string);
                case SqlDbType.NVarChar:
                    return typeof(string);
                case SqlDbType.Real:
                    return typeof(Single);
                case SqlDbType.UniqueIdentifier:
                    return typeof(Guid);
                case SqlDbType.SmallDateTime:
                    return typeof(DateTime);
                case SqlDbType.SmallInt:
                    return typeof(short);
                case SqlDbType.SmallMoney:
                    return typeof(decimal);
                case SqlDbType.Text:
                    return typeof(string);
                case SqlDbType.Timestamp:
                    return typeof(byte[]);
                case SqlDbType.TinyInt:
                    return typeof(byte);
                case SqlDbType.VarBinary:
                    return typeof(byte[]);
                case SqlDbType.VarChar:
                    return typeof(string);
                case SqlDbType.Variant:
                    return typeof(object);
                case SqlDbType.Xml:
                    return typeof(string);
                case SqlDbType.Udt:
                    return typeof(object);
                case SqlDbType.Structured:
                    return typeof(object);
                case SqlDbType.Date:
                    return typeof(DateTime);
                case SqlDbType.Time:
                    return typeof(TimeSpan);
                case SqlDbType.DateTime2:
                    return typeof(DateTime);
                case SqlDbType.DateTimeOffset:
                    return typeof(DateTimeOffset);
            }

            return typeof(object);
        }
    }
}
