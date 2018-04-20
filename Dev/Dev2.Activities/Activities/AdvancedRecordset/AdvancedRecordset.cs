/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using Dev2.Services.Sql;
using Warewolf.Storage.Interfaces;
using System.Data;
using WarewolfParserInterop;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.DB;
using Dev2.Data.Util;
using Warewolf.Core;
using TSQL.Statements;
using TSQL.Tokens;
using System.Text;

namespace Dev2.Activities
{
    public class AdvancedRecordset : IAdvancedRecordset
    {
        readonly SqliteServer _dbManager = new SqliteServer("Data Source=:memory:");
        public IExecutionEnvironment Environment { get; set; }
        public string RecordsetName { get; set; }
        public IList<INameValue> DeclareVariables { get; set; }
        public AdvancedRecordset(IExecutionEnvironment env)
        {
            Environment = env;
        }
        public AdvancedRecordset()
        {
        }
        public void AddRecordsetAsTable((string recordsetName,List<string> fields) recordSet)
        {
            ExecuteQuery("CREATE TABLE IF NOT EXISTS " + recordSet.recordsetName + "([" + recordSet.recordsetName + "_Primary_Id] INTEGER NOT NULL, CONSTRAINT[PK_" + recordSet.recordsetName + "] PRIMARY KEY([" + recordSet.recordsetName + "_Primary_Id]))");
            foreach (var field in recordSet.fields)
            {
                if (!string.IsNullOrEmpty(field))
                {
                    ExecuteNonQuery("ALTER TABLE  " + recordSet.recordsetName + " ADD COLUMN " + field + " string;");
                }
            }
        }

        public DataSet ExecuteStatement(TSQLStatement sqlStatement,string query)
        {
            if(sqlStatement.Type == TSQLStatementType.Select)
            {
                return ExecuteQuery(query);
            }

            var recordset = new DataTable();
            recordset.Columns.Add("records_affected", typeof(int));
            recordset.Rows.Add(ExecuteNonQuery(query));
            var ds = new DataSet();
            ds.Tables.Add(recordset);
            return ds;
        }
        public string ReturnSql(List<TSQLToken> tokens)
        {
            var tokenString = "";
            foreach (TSQLToken token in tokens)
            {
                tokenString = string.Concat(tokenString, " ", token.Text);
            }
            return tokenString.Replace(" ( ", "(").Replace(" ) ", ") ").Replace(" )", ")").Replace(" . ", ".").Trim();
        }

        public DataSet ExecuteQuery(string sqlQuery)
        {
            try
            {
                var command = _dbManager.CreateCommand();
                command.CommandText = sqlQuery;
                command.CommandType = CommandType.Text;
                var ds = _dbManager.FetchDataSet(command);
                return ds;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
        public int ExecuteScalar(string sqlQuery)
        {
            try
            {
                using (var cmd = _dbManager.CreateCommand())
                {
                    cmd.CommandText = sqlQuery;
                    cmd.CommandType = CommandType.Text;
                    return _dbManager.ExecuteScalar(cmd);
                }

            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
        public int ExecuteNonQuery(string sqlQuery)
        {
            try
            {
                using (var cmd = _dbManager.CreateCommand())
                {
                    cmd.CommandText = sqlQuery;
                    cmd.CommandType = CommandType.Text;
                    return _dbManager.ExecuteNonQuery(cmd);
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
        public void LoadRecordsetAsTable(string recordsetName)
        {
            var table = Environment.EvalAsTable("[[" + recordsetName + "(*)]]", 0);
            LoadIntoSqliteDb(recordsetName, table);
        }
        public void CreateVariableTable()
        {
            using (var cmd = _dbManager.CreateCommand())
            {
                cmd.CommandText = "CREATE TABLE IF NOT EXISTS Variables (Name TEXT PRIMARY KEY, Value TEXT)";
                cmd.CommandType = CommandType.Text;
                _dbManager.ExecuteNonQuery(cmd);
            }
        }
        public void InsertIntoVariableTable(string variableName, string variableValue)
        {
            using (var cmd = _dbManager.CreateCommand())
            {
                var sql = "INSERT OR REPLACE INTO Variables VALUES ('" + variableName + "', '" + variableValue + "');";
                cmd.CommandText = sql;
                cmd.CommandType = CommandType.Text;
                _dbManager.ExecuteNonQuery(cmd);
            }
        }
        public string GetVariableValue(string variableName)
        {
            var command = _dbManager.CreateCommand();
            var sql = "SELECT Value FROM Variables WHERE Name = '" + variableName.Replace("@", "").Trim() + "'";
            command.CommandText = sql;
            command.CommandType = CommandType.Text;
            var dt = _dbManager.FetchDataTable(command);
            var value = dt.Rows[0]["Value"].ToString();
            var isNumber = int.TryParse(value, out int Num);

            if (isNumber)
            {
                return value;
            }
            else
            {
                var newVariableValue = new StringBuilder();
                var arrayString = value.Split(',');
                foreach (var str in arrayString)
                {
                    var s = newVariableValue.Length > 0 ? ",'{0}'" : "'{0}'";
                    newVariableValue.AppendFormat(s, str);
                }
                return newVariableValue.ToString();
            }
        }
        public void DeleteTableInSqlite(string recordsetName)
        {
            ExecuteNonQuery("DROP TABLE IF EXISTS " + recordsetName + ";");
        }
        void CreateTableInSqlite(string recordsetName)
        {
            DeleteTableInSqlite(recordsetName);
            ExecuteNonQuery("CREATE TABLE IF NOT EXISTS " + recordsetName + "([" + recordsetName + "_Primary_Id] INTEGER NOT NULL, CONSTRAINT[PK_" + recordsetName + "] PRIMARY KEY([" + recordsetName + "_Primary_Id]))");
        }
        static string BuildInsertStatement(string key, DataStorage.WarewolfAtom value)
        {
            var _insertSql = "";
            var colType = value.GetType().Name;

            if (!key.Contains("_Primary_Id"))
            {
                if (colType == "Int")
                {
                    _insertSql = value + ",";
                }
                else if (colType == "DataString")
                {
                    _insertSql = "'" + value + "',";
                }
                else if (colType == "Float")
                {
                    _insertSql = value + ",";
                }
                else
                {
                    if (colType == "Int" && colType != "DataString")
                    {
                        _insertSql = value + ",";
                    }
                }
            }
            return _insertSql;
        }
        void LoadIntoSqliteDb(string recordsetName, IEnumerable<Tuple<string, DataStorage.WarewolfAtom>[]> tableData)
        {
            CreateTableInSqlite(recordsetName);
            var enumerator = tableData.GetEnumerator();
            if (enumerator.MoveNext())
            {
                int i = 0;

                do
                {
                    var insertSql = "INSERT INTO " + recordsetName + " select " + i + ",";

                    foreach (var (key, value) in enumerator.Current)
                    {
                        insertSql += BuildInsertStatement(key, value);
                        if (i == 0 && !key.Contains("_Primary_Id"))
                        {
                            ExecuteNonQuery("ALTER TABLE  " + recordsetName + " ADD COLUMN " + key + " " + value.GetType().Name + ";");
                        }
                    }
                    ExecuteNonQuery(insertSql.Remove(insertSql.Length - 1));
                    i++;
                } while (enumerator.MoveNext());
            }
        }
        public void ApplyResultToEnvironment(string returnRecordsetName, ICollection<IServiceOutputMapping> outputs, List<DataRow> rows, bool updated, int update,ref bool started)
        {
            var rowIdx = 1;            
            if (updated)
            {
                var recSet = DataListUtil.ReplaceRecordBlankWithStar(DataListUtil.AddBracketsToValueIfNotExist(DataListUtil.MakeValueIntoHighLevelRecordset(returnRecordsetName)));
                Environment.EvalDelete(recSet, 0);
                foreach (DataRow row in rows)
                {
                    ApplyResultToEnvironmentForEachColumn(returnRecordsetName, update, ref started, ref rowIdx, row);
                    rowIdx++;
                }
            }
            else
            {
                foreach (DataRow row in rows)
                {
                    foreach (var serviceOutputMapping in outputs)
                    {                        
                        ExecutionEnvironmentUtils.ProcessOutputMapping(Environment, update, ref started, ref rowIdx, row, serviceOutputMapping);
                    }
                    started = true;
                    rowIdx++;
                }
            }
        }

        private void ApplyResultToEnvironmentForEachColumn(string returnRecordsetName, int update, ref bool started, ref int rowIdx, DataRow row)
        {
            foreach (var col in row.Table.Columns)
            {
                var column = col as DataColumn;
                if (!column.ColumnName.Contains("_Primary_Id"))
                {
                    var serviceOutputMapping = new ServiceOutputMapping(column.ColumnName, column.ColumnName, returnRecordsetName);
                    ExecutionEnvironmentUtils.ProcessOutputMapping(Environment, update, ref started, ref rowIdx, row, serviceOutputMapping);
                }
            }
        }

        public void ApplyScalarResultToEnvironment(string returnRecordsetName, List<DataRow> recordset)
        {
            var l = new List<AssignValue>();
            if (DataListUtil.IsEvaluated(returnRecordsetName))
            {
                l.Add(new AssignValue(returnRecordsetName, recordset[0].ItemArray[0].ToString()));
            }
            Environment.AssignWithFrame(l, 0);
            Environment.CommitAssign();
        }
        public void Dispose()
        {
            _dbManager.Dispose();
        }
    }
}
