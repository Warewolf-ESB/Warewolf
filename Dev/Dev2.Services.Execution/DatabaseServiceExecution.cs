using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.DataList.Contract;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Services.Sql;
using Unlimited.Framework.Converters.Graph.Interfaces;

namespace Dev2.Services.Execution
{
    public class DatabaseServiceExecution : ServiceExecutionAbstract<DbService, DbSource>
    {
        SqlServer _sqlServer;


        #region Constuctors

        public DatabaseServiceExecution(IDSFDataObject dataObj)
            : base(dataObj, true, false)
        {
        }

        public SqlServer SqlServer
        {
            get
            {
                return _sqlServer;
            }
        }

        #endregion

        #region SqlServer

        void SetupSqlServer(ErrorResultTO errors)
        {
            try
            {
                _sqlServer = new SqlServer();
                var connected = SqlServer.Connect(Source.ConnectionString, CommandType.StoredProcedure, Service.Method.Name);
                if(!connected)
                {
                    this.LogError(string.Format("Failed to connect with the following connection string: '{0}'", Source.ConnectionString));
                }
            }
            catch(SqlException sex)
            {
                // 2013.06.24 - TWR - added errors logging
                var errorMessages = new StringBuilder();
                for(var i = 0; i < sex.Errors.Count; i++)
                {
                    errorMessages.Append("Index #" + i + Environment.NewLine +
                                         "Message: " + sex.Errors[i].Message + Environment.NewLine +
                                         "LineNumber: " + sex.Errors[i].LineNumber + Environment.NewLine +
                                         "Source: " + sex.Errors[i].Source + Environment.NewLine +
                                         "Procedure: " + sex.Errors[i].Procedure + Environment.NewLine);
                }
                errors.AddError(errorMessages.ToString());
                this.LogError(errorMessages.ToString());
            }
            catch(Exception ex)
            {
                // 2013.06.24 - TWR - added errors logging
                errors.AddError(string.Format("{0}{1}{2}", ex.Message, Environment.NewLine, ex.StackTrace));
                this.LogError(ex);
            }
        }

        void DestroySqlServer()
        {
            if(SqlServer == null)
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

        protected override object ExecuteService(out ErrorResultTO errors, IOutputFormatter formater = null)
        {
            errors = new ErrorResultTO();
            var invokeErrors = new ErrorResultTO();
            object executeService;
            var result = SqlExecution(invokeErrors, out executeService) ? executeService : string.Empty;

            _errorResult.MergeErrors(invokeErrors);

            return result;
        }

        bool SqlExecution(ErrorResultTO errors, out object executeService)
        {
            try
            {
                if(SqlServer != null)
                {
                    var parameters = GetSqlParameters(Service.Method.Parameters);

                    if(parameters != null)
                    {
                        // ReSharper disable CoVariantArrayConversion
                        using(var dataSet = SqlServer.FetchDataTable(parameters.ToArray()))
                        // ReSharper restore CoVariantArrayConversion
                        {
                            ApplyColumnMappings(dataSet);
                            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();

                            executeService = compiler.PopulateDataList(DataListFormat.CreateFormat(GlobalConstants._DATATABLE), dataSet, InstanceOutputDefintions, DataObj.DataListID, out errors);
                            return true;
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                errors.AddError(string.Format("{0}{1}{2}", ex.Message, Environment.NewLine, ex.StackTrace));
            }
            executeService = null;
            return false;
        }

        #endregion

        #region GetSqlParameters
        static List<SqlParameter> GetSqlParameters(IList<MethodParameter> methodParameters)
        {
            var sqlParameters = new List<SqlParameter>();

            if(methodParameters.Count > 0)
            {
#pragma warning disable 219
                var pos = 0;
#pragma warning restore 219
                foreach(var parameter in methodParameters)
                {

                    if(parameter.EmptyToNull && (parameter.Value == null || string.Compare(parameter.Value, string.Empty, StringComparison.InvariantCultureIgnoreCase) == 0))
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

        void ApplyColumnMappings(DataTable dataTable)
        {
            if(string.IsNullOrEmpty(Service.OutputSpecification))
            {
                return;
            }

            var outputs = XElement.Parse(Service.OutputSpecification);
            foreach(var output in outputs.Elements("Output"))
            {
                var originalName = output.AttributeSafe("OriginalName", true);
                var mappedName = output.AttributeSafe("Name", true);
                if(originalName != null && mappedName != null)
                {
                    var dc = dataTable.Columns[originalName];
                    dc.ColumnName = mappedName;
                }
            }
        }
    }
}
