using Dev2.DynamicServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Xml.Linq;
using Unlimited.Framework.Converters.Graph.Interfaces;

namespace Dev2.Runtime.ServiceModel.Data
{
    public class DbSource : Resource
    {
        #region CTOR

        public DbSource()
        {
        }

        public DbSource(XElement xml)
            : base(xml)
        {
            Server = xml.AttributeSafe("Server");
            Database = xml.AttributeSafe("Database");

            int port;
            Port = Int32.TryParse(xml.AttributeSafe("Port"), out port) ? port : 0;

            AuthenticationType authType;
            AuthenticationType = Enum.TryParse(xml.AttributeSafe("AuthenticationType"), true, out authType) ? authType : AuthenticationType.User;

            UserID = xml.AttributeSafe("UserID");
            Password = xml.AttributeSafe("Password");
        }

        #endregion

        #region Properties

        public string Server { get; set; }

        public string Database { get; set; }

        public int Port { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public AuthenticationType AuthenticationType { get; set; }

        public string UserID { get; set; }

        public string Password { get; set; }

        #endregion

        #region ConnectionString - calculated

        public string ConnectionString
        {
            get
            {
                switch(ResourceType)
                {
                    case enSourceType.SqlDatabase:
                        return string.Format("Data Source={0}{2};Initial Catalog={1};{3}", Server, Database,
                            (Port > 0 ? "," + Port : string.Empty),
                            AuthenticationType == AuthenticationType.Windows
                                ? "Integrated Security=SSPI;"
                                : string.Format("User ID={0};Password={1};", UserID, Password));

                    case enSourceType.MySqlDatabase:
                        return string.Format("Server={0};{4}Database={1};Uid={2};Pwd={3};",
                            Server, Database, UserID, Password,
                            (Port > 0 ? string.Format("Port={0};", Port) : string.Empty));
                }
                return string.Empty;
            }
        }

        #endregion

        #region ToXml

        public override XElement ToXml()
        {
            var result = base.ToXml();
            result.Add(new XAttribute("ConnectionString", ConnectionString));
            result.Add(new XAttribute("Server", Server ?? string.Empty));
            result.Add(new XAttribute("Database", Database ?? string.Empty));
            result.Add(new XAttribute("Port", Port));
            result.Add(new XAttribute("AuthenticationType", AuthenticationType));
            result.Add(new XAttribute("UserID", UserID ?? string.Empty));
            result.Add(new XAttribute("Password", Password ?? string.Empty));

            return result;
        }

        #endregion

        #region Get Methods

        public ServiceMethodList GetServiceMethods()
        {
            var serviceMethods = new ServiceMethodList();

            //
            // Function to handle procedures returned by the data broker
            //
            Func<IDbCommand, IList<IDataParameter>, string, bool> procedureFunc = (command, parameters, helpText) =>
            {
                ServiceMethod serviceMethod = ServiceMethodFromCommand(command);
                IEnumerable<MethodParameter> methodParameters = parameters.Select(MethodParameterFromDataParameter);
                serviceMethod.Parameters.AddRange(methodParameters);
                return true;
            };

            //
            // Function to handle functions returned by the data broker
            //
            Func<IDbCommand, IList<IDataParameter>, string, bool> functionFunc = (command, parameters, helpText) =>
            {
                ServiceMethod serviceMethod = ServiceMethodFromCommand(command);
                IEnumerable<MethodParameter> methodParameters = parameters.Select(MethodParameterFromDataParameter);
                serviceMethod.Parameters.AddRange(methodParameters);
                return true;
            };

            //
            // Get stored procedures and functions for this database source
            //
            using (var conn = CreateConnection())
            {
                conn.Open();
                var msSqlDataBroker = CreateDataBroker();
                msSqlDataBroker.GetStoredProcedures(conn, procedureFunc, functionFunc);
            }

            return serviceMethods;
        }

        public IOutputDescription TestServiceMethod(ServiceMethod serviceMethod)
        {
            IOutputDescription result = null;
            using (var conn = CreateConnection())
            {
                conn.Open();
                IDbTransaction transaction = conn.BeginTransaction();

                try
                {
                    //var msSqlDataBroker = CreateDataBroker();
                    //var command = CommandFromServiceMethod(conn, serviceMethod);

                    //
                    // Execute command and extract XML
                    //
                    //var dataSet = msSqlDataBroker.ExecuteSelect(command);
                    //string resultXML = dataSet.GetXml();
                    //resultXML = DataSanitizerFactory.GenerateNewSanitizer(enSupportedDBTypes.MSSQL).SanitizePayload(dataset.GetXml())
                }
                finally
                {
                    try
                    {
                        transaction.Rollback();
                    }
                    catch(Exception e)
                    {
                        TraceWriter.WriteTrace("Transactional Error : " + e.Message + Environment.NewLine + e.StackTrace);
                    }
                }
            }

            return result;
        }

        #endregion

        #region Private Methods

        private DataBroker CreateDataBroker()
        {
            if (ResourceType == enSourceType.SqlDatabase)
            {
                return new MsSqlDataBroker();
            }

            throw new Exception(string.Format("Cant create a data broker for the resource type '{0}'.", ResourceType));
        }

        private IDbConnection CreateConnection()
        {
            if (ResourceType == enSourceType.SqlDatabase)
            {
                return new SqlConnection(ConnectionString);
            }

            throw new Exception(string.Format("Can't create a database connection for the resource type '{0}'.", ResourceType));
        }

        private IDbCommand CommandFromServiceMethod(IDbConnection connection, ServiceMethod serviceMethod)
        {
            if (ResourceType == enSourceType.SqlDatabase)
            {
                var command = connection.CreateCommand();

                command.CommandText = serviceMethod.Name;
                command.CommandType = CommandType.StoredProcedure;

                foreach (var methodParameter in serviceMethod.Parameters)
                {
                    IDbDataParameter dataParameter = DataParameterFromMethodParameter(command, methodParameter);
                    command.Parameters.Add(dataParameter);
                }

                return command;
            }

            throw new Exception(string.Format("Can't create a database command for the resource type '{0}'.", ResourceType));
        }

        private IDbDataParameter DataParameterFromMethodParameter(IDbCommand command, MethodParameter methodParameter)
        {
            if (ResourceType == enSourceType.SqlDatabase)
            {
                var parameter = command.CreateParameter();

                parameter.ParameterName = string.Format("@{0}", methodParameter.Name);
                parameter.Value = methodParameter.Value;

                return parameter;
            }

            throw new Exception(string.Format("Can't create a command parameter for the resource type '{0}'.", ResourceType));
        }

        private ServiceMethod ServiceMethodFromCommand(IDbCommand command)
        {
            return new ServiceMethod(command.CommandText, null, null, null);
        }

        private MethodParameter MethodParameterFromDataParameter(IDataParameter parameter)
        {
            return new MethodParameter(parameter.ParameterName.Replace("@", ""), false, false, "", "");
        }

        #endregion Private Methods
    }
}
