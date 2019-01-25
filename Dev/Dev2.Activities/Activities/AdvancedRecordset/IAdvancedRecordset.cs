using Dev2.Common.Interfaces.DB;
using System;
using System.Collections.Generic;
using System.Data;
using TSQL.Statements;
using TSQL.Tokens;

namespace Dev2.Activities
{
    public interface IAdvancedRecordset : IDisposable
    {
        
        void LoadRecordsetAsTable(string recordsetName);
        string ReturnSql(List<TSQLToken> tokens);
        DataSet ExecuteQuery(string sqlQuery);
        int ExecuteScalar(string sqlQuery);
        int ExecuteNonQuery(string sqlQuery);
        void CreateVariableTable();
        string GetVariableValue(string variableName);
        void InsertIntoVariableTable(string variableName, string variableValue);
        string UpdateSqlWithHashCodes(TSQLStatement statement);
        void ApplyResultToEnvironment(string returnRecordsetName, ICollection<IServiceOutputMapping> outputs, List<DataRow> rows, bool updated, int update, ref bool started);
        void ApplyScalarResultToEnvironment(string returnRecordsetName, int recordsAffected);
        List<(string hashCode, string recSet)> HashedRecSets { get; }
        void DeleteTableInSqlite(string recordsetName);
    }
}
