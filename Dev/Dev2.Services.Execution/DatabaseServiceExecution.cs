using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using Dev2.Common;
using Dev2.DataList.Contract;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Services.Sql;

namespace Dev2.Services.Execution
{
    public class DatabaseServiceExecution:ServiceExecutionAbstract<DbService,DbSource>
    {
        SqlServer _sqlServer;

        public string InstanceOutputDefintions { get; set; }

        #region Constuctors

        public DatabaseServiceExecution(IDSFDataObject dataObj)
            : base(dataObj, true,false)
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
                var connected = SqlServer.Connect(Service.Method.Name, CommandType.StoredProcedure,
                    Source.Server,
                    Source.DatabaseName,
                    Source.Port,
                    Source.UserID,
                    Source.Password);
                if(!connected)
                {
                    ServerLogger.LogError(string.Format("Failed to connect with the following connection string: '{0}'", Source.ConnectionString));
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
                ServerLogger.LogError(errorMessages.ToString());
            }
            catch(Exception ex)
            {
                // 2013.06.24 - TWR - added errors logging
                errors.AddError(string.Format("{0}{1}{2}", ex.Message, Environment.NewLine, ex.StackTrace));
                ServerLogger.LogError(ex);
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

        protected override object ExecuteService()
        {
            var errors = new ErrorResultTO();
            object executeService;
            var result =  SqlExecution(errors, out executeService) ? executeService : string.Empty;

            _errorResult.MergeErrors(errors);

            return result;
        }

        bool SqlExecution(ErrorResultTO errors, out object executeService)
        {
            try
            {
                if(SqlServer!=null)
                {
                    var parameters = GetSqlParameters(Service.Method.Parameters);

                    var dataSet = SqlServer.FetchDataTable(parameters.ToArray());
                    {

                        IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();

                        executeService = compiler.PopulateDataList(DataListFormat.CreateFormat(GlobalConstants._DATATABLE), dataSet, InstanceOutputDefintions, DataObj.DataListID, out errors);
                        return true;
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
                var pos = 0;
                foreach (var parameter in methodParameters)
                {

                    if (parameter.EmptyToNull && (parameter.Value == null || string.Compare(parameter.Value, string.Empty, StringComparison.InvariantCultureIgnoreCase) == 0))
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


    }
}
