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
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Common.Interfaces.Data;
using Dev2.DataList.Contract;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Services.Sql;
using Warewolf.Storage;

namespace Dev2.Services.Execution
{
    public class DatabaseServiceExecution : ServiceExecutionAbstract<DbService, DbSource>
    {
        private SqlServer _sqlServer;

        #region Constuctors

        public DatabaseServiceExecution(IDSFDataObject dataObj)
            : base(dataObj, true, false)
        {
        }

        public SqlServer SqlServer
        {
            get { return _sqlServer; }
        }

        #endregion

        #region SqlServer

        private void SetupSqlServer(ErrorResultTO errors)
        {
            try
            {
                _sqlServer = new SqlServer();
                bool connected = SqlServer.Connect(Source.ConnectionString, CommandType.StoredProcedure,
                    String.IsNullOrEmpty(Service.Method.ExecuteAction)
                        ? Service.Method.Name
                        : Service.Method.ExecuteAction);
                if (!connected)
                {
                    Dev2Logger.Log.Error(string.Format("Failed to connect with the following connection string: '{0}'",
                        Source.ConnectionString));
                }
            }
            catch (SqlException sex)
            {
                // 2013.06.24 - TWR - added errors logging
                var errorMessages = new StringBuilder();
                for (int i = 0; i < sex.Errors.Count; i++)
                {
                    errorMessages.Append("Index #" + i + Environment.NewLine +
                                         "Message: " + sex.Errors[i].Message + Environment.NewLine +
                                         "LineNumber: " + sex.Errors[i].LineNumber + Environment.NewLine +
                                         "Source: " + sex.Errors[i].Source + Environment.NewLine +
                                         "Procedure: " + sex.Errors[i].Procedure + Environment.NewLine);
                }
                errors.AddError(errorMessages.ToString());
                Dev2Logger.Log.Error(errorMessages.ToString());
            }
            catch (Exception ex)
            {
                // 2013.06.24 - TWR - added errors logging
                errors.AddError(string.Format("{0}{1}{2}", ex.Message, Environment.NewLine, ex.StackTrace));
                Dev2Logger.Log.Error(ex);
            }
        }

        private void DestroySqlServer()
        {
            if (SqlServer == null)
            {
                return;
            }
            SqlServer.Dispose();
            _sqlServer = null;
        }

        #endregion

        #region Execute

        public override void BeforeExecution(ErrorResultTO errors)
        {
            SetupSqlServer(errors);
        }

        public override void AfterExecution(ErrorResultTO errors)
        {
            DestroySqlServer();
        }

        protected override object ExecuteService(List<MethodParameter> methodParameters, out ErrorResultTO errors, IOutputFormatter formater = null)
        {
            errors = new ErrorResultTO();
            var invokeErrors = new ErrorResultTO();
            SqlExecution(invokeErrors);
            
            ErrorResult.MergeErrors(invokeErrors);

            return Guid.NewGuid();
        }

        void TranslateDataTableToEnvironment(DataTable executeService, IExecutionEnvironment environment)
        {
            if (executeService != null && InstanceOutputDefintions != null)
            {
                var defs = DataListFactory.CreateOutputParser().Parse(InstanceOutputDefintions);
                HashSet<string> processedRecNames = new HashSet<string>();

                foreach (var def in defs)
                {
                    var expression = def.Value;
                    var rs = def.RawValue;
                    var rsName = Data.Util.DataListUtil.ExtractRecordsetNameFromValue(expression);
                    var rsType = Data.Util.DataListUtil.GetRecordsetIndexType(expression);
                    var rowIndex = Data.Util.DataListUtil.ExtractIndexRegionFromRecordset(expression);
                    var rsNameUse = def.RecordSetName;

                    if (string.IsNullOrEmpty(rsName))
                    {
                        rsName = rsNameUse;
                    }
                    if (string.IsNullOrEmpty(rsName))
                    {
                        rsName = def.Name;
                    }

                    if (processedRecNames.Contains(rsName))
                    {
                        continue;
                    }

                    processedRecNames.Add(rsName);

                    // build up the columns ;)

                    if (Data.Util.DataListUtil.IsValueRecordset(rs))
                    {

                        IDictionary<int, string> colMapping = BuildColumnNameToIndexMap(
                                                                                        executeService.Columns,
                                                                                        defs);

                        // now convert to binary datalist ;)
                        int rowIdx = 1;
                        try
                        {
                            rowIdx = environment.GetLength(rs);
                        }
                        catch (Exception e)
                        {
                            Dev2Logger.Log.Error(e.Message, e);
                        }
                        if (rsType == enRecordsetIndexType.Star)
                        {
                            rowIdx = 1;
                        }
                        if (rsType == enRecordsetIndexType.Numeric)
                        {
                            rowIdx = int.Parse(rowIndex);
                        }

                        if (executeService.Rows != null)
                        {
                            foreach (DataRow row in executeService.Rows)
                            {
                                // build up the row
                                int idx = 0;

                                foreach (var item in row.ItemArray)
                                {
                                    string colName;

                                    if (colMapping.TryGetValue(idx, out colName))
                                    {
                                        var displayExpression = Data.Util.DataListUtil.AddBracketsToValueIfNotExist(Data.Util.DataListUtil.CreateRecordsetDisplayValue(Data.Util.DataListUtil.ExtractRecordsetNameFromValue(def.Value), Data.Util.DataListUtil.ExtractFieldNameFromValue(def.Value), rowIdx.ToString()));
                                        environment.Assign(displayExpression, item.ToString());
                                        //items.Add(new BinaryDataListItem(item.ToString(), rsNameUse, colName, rowIdx));
                                    }

                                    idx++;
                                }

                                rowIdx++;
                            }
                        }
                    }
                    else
                    {
                        // handle a scalar coming out ;)
                        if (executeService.Rows != null && executeService.Rows.Count == 1)
                        {
                            var row = executeService.Rows[0].ItemArray;
                            // Look up the correct index from the columns ;)

                            int pos = 0;
                            var cols = executeService.Columns;
                            int idx = -1;

                            while (pos < cols.Count && idx == -1)
                            {
                                if (cols[pos].ColumnName == rsName)
                                {
                                    idx = pos;
                                }
                                pos++;
                            }
                            environment.Assign(rsName, row[idx].ToString());
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Builds the column name to index map.
        /// </summary>
        /// <param name="dtCols">The dt cols.</param>
        /// <param name="defs">Defs to use</param>
        /// <returns></returns>
        // ReSharper disable ParameterTypeCanBeEnumerable.Local
        private IDictionary<int, string> BuildColumnNameToIndexMap(DataColumnCollection dtCols, IList<IDev2Definition> defs)
        // ReSharper restore ParameterTypeCanBeEnumerable.Local
        {
            Dictionary<int, string> result = new Dictionary<int, string>();

//
//            foreach (var dlC in dlCols)
//            {
//                var idx = dtCols.IndexOf(dlC.ColumnName);
//
//                if (idx != -1)
//                {
//                    result.Add(idx, dlC.ColumnName);
//                }
//            }

            if (result.Count == 0 && defs != null)
            {
                // use positional adjustment
                foreach (var def in defs)
                {
                    var idx = dtCols.IndexOf(def.Name);

                    if (idx != -1)
                    {
                        result.Add(idx, def.Name);
                    }
                }
            }

            return result;
        }
        private void SqlExecution(ErrorResultTO errors)
        {
            try
            {
                if (SqlServer != null)
                {
                    List<SqlParameter> parameters = GetSqlParameters(Service.Method.Parameters);

                    if (parameters != null)
                    {
                        // ReSharper disable CoVariantArrayConversion
                        using (DataTable dataSet = SqlServer.FetchDataTable(parameters.ToArray()))
                            // ReSharper restore CoVariantArrayConversion
                        {
                            ApplyColumnMappings(dataSet);
                            TranslateDataTableToEnvironment(dataSet, DataObj.Environment);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errors.AddError(string.Format("{0}{1}{2}", ex.Message, Environment.NewLine, ex.StackTrace));
            }
        }

        #endregion

        #region GetSqlParameters

        private static List<SqlParameter> GetSqlParameters(IList<MethodParameter> methodParameters)
        {
            var sqlParameters = new List<SqlParameter>();

            if (methodParameters.Count > 0)
            {
#pragma warning disable 219
                // ReSharper disable once NotAccessedVariable
                int pos = 0;
#pragma warning restore 219
                foreach (MethodParameter parameter in methodParameters)
                {
                    if (parameter.EmptyToNull &&
                        (parameter.Value == null ||
                         string.Compare(parameter.Value, string.Empty, StringComparison.InvariantCultureIgnoreCase) == 0))
                    {
                        sqlParameters.Add(new SqlParameter(string.Format("@{0}", parameter.Name), DBNull.Value));
                    }
                    else
                    {
                        sqlParameters.Add(new SqlParameter(string.Format("@{0}", parameter.Name), parameter.Value));
                    }
                    pos++;
                }
            }
            return sqlParameters;
        }

        #endregion

        private void ApplyColumnMappings(DataTable dataTable)
        {
            if (string.IsNullOrEmpty(Service.OutputSpecification))
            {
                return;
            }

            XElement outputs = XElement.Parse(Service.OutputSpecification);
            foreach (XElement output in outputs.Elements("Output"))
            {
                string originalName = output.AttributeSafe("OriginalName", true);
                string mappedName = output.AttributeSafe("Name", true);
                if (originalName != null && mappedName != null)
                {
                    DataColumn dc = dataTable.Columns[originalName];
                    dc.ColumnName = mappedName;
                }
            }
        }
    }
}
