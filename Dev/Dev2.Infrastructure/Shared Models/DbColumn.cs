#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
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

#pragma warning disable S1541 // Methods and properties should not be too complex
        public static Type ConvertSqlDbType(SqlDbType sqlDbType)
#pragma warning restore S1541 // Methods and properties should not be too complex
        {
            // http://msdn.microsoft.com/en-us/library/system.data.sqldbtype.aspx
            switch (sqlDbType)
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
                default:
                    break;
            }

            return typeof(object);
        }

        public bool Equals(IDbColumn other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return SqlDataType == other.SqlDataType
                   && string.Equals(ColumnName, other.ColumnName)
                   && IsNullable == other.IsNullable
                   && DataType == other.DataType
                   && MaxLength == other.MaxLength
                   && IsAutoIncrement == other.IsAutoIncrement;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((IDbColumn) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int) SqlDataType;
                hashCode = (hashCode * 397) ^ (ColumnName != null ? ColumnName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ IsNullable.GetHashCode();
                hashCode = (hashCode * 397) ^ (DataType != null ? DataType.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ IsAutoIncrement.GetHashCode();
                hashCode = (hashCode * 397) ^ MaxLength;
                return hashCode;
            }
        }
    }
}
