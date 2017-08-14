using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Xml;
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Services.Sql;
using Oracle.ManagedDataAccess.Client;
using Unlimited.Framework.Converters.Graph;

namespace Dev2.Runtime.ServiceModel.Esb.Brokers
{
    /// <summary>
    /// A Oracle SQL specific database broker implementation
    /// </summary>
    public class OracleDatabaseBroker : AbstractDatabaseBroker<OracleServer>
    {
        protected override string NormalizeXmlPayload(string payload)
        {
            var result = new StringBuilder();

            var xDoc = new XmlDocument();
            xDoc.LoadXml(payload);
            var nl = xDoc.SelectNodes("//NewDataSet/Table/*[starts-with(local-name(),'XML_')]");
            var foundXMLFrags = 0;

            if (nl != null)
            {
                foreach (XmlNode n in nl)
                {
                    var tmp = n.InnerXml;
                    result = result.Append(tmp);
                    foundXMLFrags++;
                }
            }

            var res = result.ToString();

            if (foundXMLFrags >= 1)
            {
                res = "<FromXMLPayloads>" + res + "</FromXMLPayloads>";
            }
            else if (foundXMLFrags == 0)
            {
                res = payload;
            }

            return base.NormalizeXmlPayload(res);
        }
        public override List<string> GetDatabases(DbSource dbSource)
        {
            VerifyArgument.IsNotNull("dbSource", dbSource);
            using (var server = CreateDbServer(dbSource))
            {
                server.Connect(dbSource.ConnectionString);
                return server.FetchDatabases();
            }
        }

        #region Overrides of AbstractDatabaseBroker<MySqlServer>

        public override ServiceMethodList GetServiceMethods(DbSource dbSource)
        {
            VerifyArgument.IsNotNull("dbSource", dbSource);


            var serviceMethods = new ServiceMethodList();

            //
            // Function to handle procedures returned by the data broker
            //
            Func<IDbCommand, IList<IDbDataParameter>, IList<IDbDataParameter>, string, string, bool> procedureFunc = (command, parameters, outparameters, helpText, executeAction) =>
            {
                var serviceMethod = CreateServiceMethod(command, parameters, outparameters, helpText, executeAction);
                serviceMethods.Add(serviceMethod);
                return true;
            };

            //
            // Function to handle functions returned by the data broker
            //
            Func<IDbCommand, IList<IDbDataParameter>, IList<IDbDataParameter>, string, string, bool> functionFunc = (command, parameters, outparameters, helpText, executeAction) =>
            {
                var serviceMethod = CreateServiceMethod(command, parameters, outparameters, helpText, executeAction);
                serviceMethods.Add(serviceMethod);
                return true;
            };

            //
            // Get stored procedures and functions for this database source
            //
            using(var server = CreateDbServer(dbSource))
            {
                server.Connect(dbSource.ConnectionString);
                server.FetchStoredProcedures(procedureFunc, functionFunc, false, dbSource.DatabaseName);
            }

            // Add to cache ;)

            return serviceMethods;
        }

        #region Overrides of AbstractDatabaseBroker<MySqlServer>

        protected override OracleServer CreateDbServer(DbSource dbSource)
        {
            return new OracleServer();
        }
        private static ServiceMethod CreateServiceMethod(IDbCommand command, IEnumerable<IDataParameter> parameters, IEnumerable<IDataParameter> outParameters, string sourceCode, string executeAction)
        {
            return new ServiceMethod(command.CommandText, sourceCode, parameters.Select(MethodParameterFromDataParameter), null, null, executeAction)
            {
                OutParameters = outParameters.Select(MethodParameterFromDataParameter).ToList()
            };
        }
        #endregion

        #endregion

        public override IOutputDescription TestService(DbService dbService)
        {
            VerifyArgument.IsNotNull("dbService", dbService);
            VerifyArgument.IsNotNull("dbService.Source", dbService.Source);

            IOutputDescription result;
            using (var server = CreateDbServer(dbService.Source as DbSource))
            {
                server.Connect(((DbSource)dbService.Source).ConnectionString);
                server.BeginTransaction();
                try
                {
                    //
                    // Execute command and normalize XML
                    //
                    IDbCommand command = CommandFromServiceMethod(server, dbService.Method);

                    

                    var databaseName = (dbService.Source as DbSource).DatabaseName;
                    var fullProcedureName = dbService.Method.ExecuteAction.Substring(dbService.Method.ExecuteAction.IndexOf(".", StringComparison.Ordinal) + 1);

                    
                    var outParams = server.GetProcedureOutParams(fullProcedureName, databaseName);
                    var countRefCursors = outParams.Count(parameter => parameter.OracleDbType == OracleDbType.RefCursor);
                    var countSingleParams = outParams.Count(parameter => parameter.OracleDbType != OracleDbType.RefCursor);
                    if (countRefCursors > 1)
                    {
                        throw new Exception("Multiple Ref Cursor are not currently supported.");
                    }
                    if (countRefCursors >= 1 && countSingleParams >= 1)
                    {
                        throw new Exception("Mixing single return values and Ref Cursors are not currently supported.");
                    }
                    var dbDataParameters = server.GetProcedureInputParameters(command, databaseName, fullProcedureName);
                    IDbCommand cmd = command.Connection.CreateCommand();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = databaseName +"."+ fullProcedureName;
                    var parameters = dbService.Method.Parameters;
                    foreach (var dbDataParameter in dbDataParameters)
                    {
                        var foundParameter = parameters.FirstOrDefault(parameter => parameter.Name == dbDataParameter.ParameterName);
                        if (foundParameter != null)
                        {
                            dbDataParameter.Value = foundParameter.Value;                            
                        }
                       cmd.Parameters.Add(dbDataParameter);
                    }

                    

                    
                    foreach (var dbDataParameter in outParams)
                    {
                        cmd.Parameters.Add(dbDataParameter);
                    }
                    var dataTable = server.FetchDataTable(cmd);

                    //
                    // Map shape of XML
                    //
                    result = OutputDescriptionFactory.CreateOutputDescription(OutputFormats.ShapedXML);
                    var dataSourceShape = DataSourceShapeFactory.CreateDataSourceShape();
                    result.DataSourceShapes.Add(dataSourceShape);

                    var dataBrowser = DataBrowserFactory.CreateDataBrowser();
                    dataSourceShape.Paths.AddRange(dataBrowser.Map(dataTable));
                    cmd.Dispose();
                }
                finally
                {
                    server.RollbackTransaction();
                }
            }
            
            return result;
        }
    }
}