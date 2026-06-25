#pragma warning disable
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
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Runtime.Serialization;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;
using Oracle.ManagedDataAccess.Client;
using System.Data.Odbc;
using System.Data.SQLite;
using MySql.Data.MySqlClient;
using Warewolf.Resource.Errors;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class GetDatabaseColumnsForTable : DefaultEsbManagementEndpoint
    {
        #region Implementation of DefaultEsbManagementEndpoint
        
        public override StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            if (values == null)
            {
                throw new InvalidDataContractException(ErrorResource.NoParameter);
            }
            string database = null;
            string tableName = null;
            string schema = null;
            values.TryGetValue("Database", out StringBuilder tmp);
            if (tmp != null)
            {
                database = tmp.ToString();
            }
            values.TryGetValue("TableName", out tmp);
            if (tmp != null)
            {
                tableName = tmp.ToString();
            }

            values.TryGetValue("Schema", out tmp);
            if (tmp != null)
            {
                schema = tmp.ToString();
            }

            var serializer = new Dev2JsonSerializer();

            if (string.IsNullOrEmpty(database))
            {
                var res = new DbColumnList("No database set.");
                Dev2Logger.Debug("No database set.", GlobalConstants.WarewolfDebug);
                return serializer.SerializeToBuilder(res);
            }
            if (string.IsNullOrEmpty(tableName))
            {
                var res = new DbColumnList("No table name set.");
                Dev2Logger.Debug("No table name set.", GlobalConstants.WarewolfDebug);
                return serializer.SerializeToBuilder(res);
            }
            Dev2Logger.Info($"Get Database Columns For Table. Database:{database} Schema:{schema} Table{tableName}", GlobalConstants.WarewolfInfo);
            try
            {
                var dbSource = serializer.Deserialize<DbSource>(database);
                // Fall back to the supplied source when the catalog lookup returns nothing,
                // otherwise the connection string dereference below throws a NullReferenceException.
                var runtTimedbSource = ResourceCatalog.Instance.GetResource<DbSource>(theWorkspace.ID, dbSource.ResourceID) ?? dbSource;
                DataTable columnInfo = GetColumnInfo(dbSource, runtTimedbSource, tableName, schema);

                var dbColumns = new DbColumnList();

                if (columnInfo != null)
                {
                    foreach (DataRow row in columnInfo.Rows)
                    {
                        AddDbColumn(dbSource, dbColumns, row);
                    }
                }
                return serializer.SerializeToBuilder(dbColumns);
            }
            catch (Exception ex)
            {
                Dev2Logger.Error(ex, GlobalConstants.WarewolfError);
                var res = new DbColumnList(ex);
                return serializer.SerializeToBuilder(res);
            }
        }

        DataTable GetColumnInfo(DbSource dbSource, DbSource runtimeDbSource, string tableName, string schema)
        {
            switch (dbSource.ServerType)
            {
                case enSourceType.MySqlDatabase:
                    return GetMySqlColumns(runtimeDbSource, tableName);
                case enSourceType.SQLiteDatabase:
                    return GetSQLiteColumns(runtimeDbSource, tableName);
                case enSourceType.Oracle:
                    return GetOracleColumns(runtimeDbSource, tableName);
                case enSourceType.ODBC:
                    return GetOdbcColumns(runtimeDbSource, tableName);
                default:
                    return GetSqlServerColumns(runtimeDbSource, tableName, schema);
            }
        }

        DataTable GetMySqlColumns(DbSource runtimeDbSource, string tableName)
        {
            using (var connection = new MySqlConnection(runtimeDbSource.ConnectionString))
            {
                connection.Open();
                var sql = @"select  * from  " + tableName.Trim('"').Replace("[", "").Replace("]", "") + " Limit 1 ";
                using (var sqlcmd = new MySqlCommand(sql, connection))
                using (var sdr = sqlcmd.ExecuteReader(CommandBehavior.CloseConnection))
                    return sdr.GetSchemaTable();
            }
        }

        DataTable GetSQLiteColumns(DbSource runtimeDbSource, string tableName)
        {
            using (var connection = new SQLiteConnection(runtimeDbSource.ConnectionString))
            {
                connection.Open();
                var sql = @"select  * from  " + tableName.Trim('"').Replace("[", "").Replace("]", "") + " Limit 1 ";
                using (var sqlcmd = new SQLiteCommand(sql, connection))
                using (var sdr = sqlcmd.ExecuteReader(CommandBehavior.CloseConnection))
                    return sdr.GetSchemaTable();
            }
        }

        DataTable GetOracleColumns(DbSource runtimeDbSource, string tableName)
        {
            using (var connection = new OracleConnection(runtimeDbSource.ConnectionString))
            {
                connection.Open();
                var sql = @"select  * from  " + tableName.Trim('"').Replace("[", "").Replace("]", "") + " Limit 1 ";
                using (var sqlcmd = new OracleCommand(sql, connection))
                using (var sdr = sqlcmd.ExecuteReader(CommandBehavior.CloseConnection))
                    return sdr.GetSchemaTable();
            }
        }

        DataTable GetOdbcColumns(DbSource runtimeDbSource, string tableName)
        {
            using (var connection = new OdbcConnection(runtimeDbSource.ConnectionString))
            {
                connection.Open();
                var sql = @"select  * from  " + tableName.Trim('"').Replace("[", "").Replace("]", "") + " Limit 1 ";
                using (var sqlcmd = new OdbcCommand(sql, connection))
                using (var sdr = sqlcmd.ExecuteReader(CommandBehavior.CloseConnection))
                    return sdr.GetSchemaTable();
            }
        }

        DataTable GetSqlServerColumns(DbSource runtimeDbSource, string tableName, string schema)
        {
            using (var connection = new SqlConnection(runtimeDbSource.ConnectionString))
            {
                connection.Open();
                if (schema == null)
                {
                    schema = string.Empty;
                }
                var sql = @"select top 1 * from " + schema.Trim('"') + "." + tableName.Trim('"');
                using (var sqlcmd = new SqlCommand(sql, connection))
                using (var sdr = sqlcmd.ExecuteReader(CommandBehavior.CloseConnection))
                    return sdr.GetSchemaTable();
            }
        }

        static void AddDbColumn(DbSource dbSource, DbColumnList dbColumns, DataRow row)
        {
            var columnName = row["ColumnName"] as string;
            var isNullable = row["AllowDBNull"] is bool && (bool)row["AllowDBNull"];
            var isIdentity = row["IsIdentity"] is bool && (bool)row["IsIdentity"];
            var dbColumn = new DbColumn { ColumnName = columnName, IsNullable = isNullable, IsAutoIncrement = isIdentity };

            var typeValue = dbSource.ServerType == enSourceType.SqlDatabase ? row["DataTypeName"] as string : ((Type)row["DataType"]).Name;
            if (Enum.TryParse(typeValue, true, out SqlDbType sqlDataType))
            {
                dbColumn.SqlDataType = sqlDataType;
            }

            var columnLength = row["ColumnSize"] as int? ?? -1;
            dbColumn.MaxLength = columnLength;
            dbColumns.Items.Add(dbColumn);
        }

        #endregion

        public override DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><Database ColumnIODirection=\"Input\"/><TableName ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public override string HandlesType() => "GetDatabaseColumnsForTableService";
    }
}
