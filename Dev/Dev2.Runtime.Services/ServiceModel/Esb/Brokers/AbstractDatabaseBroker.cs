using Dev2.Runtime.ServiceModel.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Unlimited.Framework.Converters.Graph;
using Unlimited.Framework.Converters.Graph.Interfaces;

namespace Dev2.Runtime.ServiceModel.Esb.Brokers
{
    /// <summary>
    /// Provides base logic for database brokers.
    /// </summary>
    public abstract class AbstractDatabaseBroker
    {
        #region Methods

        /// <summary>
        /// Gets the service methods for this esb endpoint.
        /// </summary>
        /// <param name="dbService">The resource.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">resource</exception>
        /// <exception cref="System.Exception"></exception>
        public ServiceMethodList GetServiceMethods(DbService dbService)
        {
            if (dbService == null)
            {
                throw new ArgumentNullException("dbService");
            }

            if (dbService.Source == null)
            {
                throw new ArgumentNullException("dbService.Source");
            }

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
            using (var conn = CreateConnection(dbService.Source.ConnectionString))
            {
                conn.Open();
                GetStoredProcedures(conn, procedureFunc, functionFunc);
            }

            return serviceMethods;
        }

        /// <summary>
        /// Executes a service method of this ESB endpoint in test mode.
        /// </summary>
        /// <param name="resource">The resource.</param>
        /// <param name="serviceMethod">The service method.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">resource</exception>
        /// <exception cref="System.Exception"></exception>
        public IOutputDescription TestServiceMethod(Resource resource, ServiceMethod serviceMethod)
        {
            if (resource == null)
            {
                throw new ArgumentNullException("resource");
            }

            //
            // Check the resource is of the correct type
            //
            var dbSource = resource as DbSource;
            if (dbSource == null)
            {
                throw new Exception(string.Format("Unexpected resource type, recieved '{0}', expected '{1}'.", resource.GetType(), typeof(DbSource)));
            }

            IOutputDescription result = null;
            using (var conn = CreateConnection(dbSource.ConnectionString))
            {
                conn.Open();
                IDbTransaction transaction = conn.BeginTransaction();

                try
                {
                    //
                    // Execute command and normalize XML
                    //
                    var command = CommandFromServiceMethod(conn, serviceMethod);
                    var dataSet = ExecuteSelect(command);
                    string xmlResult = dataSet.GetXml();
                    xmlResult = NormalizeXmlPayload(xmlResult);

                    //
                    // Map shape of XML
                    //
                    IOutputDescription ouputDescription = OutputDescriptionFactory.CreateOutputDescription(OutputFormats.ShapedXML);
                    IDataSourceShape dataSourceShape = DataSourceShapeFactory.CreateDataSourceShape();
                    ouputDescription.DataSourceShapes.Add(dataSourceShape);

                    IDataBrowser dataBrowser = DataBrowserFactory.CreateDataBrowser();

                    if (!string.IsNullOrEmpty(xmlResult))
                    {
                        dataSourceShape.Paths.AddRange(dataBrowser.Map(xmlResult));
                    }
                }
                finally
                {
                    try
                    {
                        transaction.Rollback();
                    }
                    catch (Exception e)
                    {
                        TraceWriter.WriteTrace("Transactional Error : " + e.Message + Environment.NewLine + e.StackTrace);
                    }
                }
            }

            return result;
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Executes a select command with individual row processing support.
        /// </summary>
        /// <param name="selectCommand">The select command.</param>
        /// <param name="rowProcessor">The row processor.</param>
        /// <param name="continueOnProcessorException">if set to <c>true</c> [continue on processor exception].</param>
        /// <param name="commandBehavior">The command behavior.</param>
        protected void ExecuteSelect(IDbCommand selectCommand, Func<IDataReader, bool> rowProcessor, bool continueOnProcessorException = false, CommandBehavior commandBehavior = CommandBehavior.CloseConnection)
        {
            using (var reader = selectCommand.ExecuteReader(commandBehavior))
            {
                if (reader != null)
                {
                    bool read = true;
                    while (read && reader.Read())
                    {
                        try
                        {
                            read = rowProcessor(reader);
                        }
                        catch (Exception)
                        {
                            if (!continueOnProcessorException)
                            {
                                throw;
                            }
                        }

                    }
                }
            }
        }

        /// <summary>
        /// Create a ServiceMethod from a IDbCommand.
        /// </summary>
        /// <param name="command">The command.</param>
        protected ServiceMethod ServiceMethodFromCommand(IDbCommand command)
        {
            return new ServiceMethod(command.CommandText, null, null, null);
        }

        /// <summary>
        /// Create a MethodParameter from a IDataParameter.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        protected MethodParameter MethodParameterFromDataParameter(IDataParameter parameter)
        {
            return new MethodParameter(parameter.ParameterName.Replace("@", ""), false, false, "", "");
        }

        /// <summary>
        /// Create a IDbCommand from a ServiceMethod.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="serviceMethod">The service method.</param>
        /// <returns></returns>
        protected IDbCommand CommandFromServiceMethod(IDbConnection connection, ServiceMethod serviceMethod)
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

        /// <summary>
        /// Create a IDbDataParameter from a MethodParameter.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="methodParameter">The method parameter.</param>
        /// <returns></returns>
        protected IDbDataParameter DataParameterFromMethodParameter(IDbCommand command, MethodParameter methodParameter)
        {
            var parameter = command.CreateParameter();

            parameter.ParameterName = string.Format("@{0}", methodParameter.Name);
            parameter.Value = methodParameter.Value;

            return parameter;
        }

        #endregion Protected Methods

        #region Protected Virtual Methods

        /// <summary>
        /// Override for implementation specific normalization of a XML payload. By Default unescapes triangle bracked characters from &lt; and &gt;.
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <returns></returns>
        protected virtual string NormalizeXmlPayload(string payload)
        {
            //
            // Unescape '<>' characters delimiting
            //
            return (payload.Replace("&lt;", "<").Replace("&gt;", ">"));
        }

        #endregion

        #region Protected Abstract Methods

        /// <summary>
        /// Override to return stored procedures.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="procedureProcessor">The procedure processor.</param>
        /// <param name="functionProcessor">The function processor.</param>
        /// <param name="continueOnProcessorException">if set to <c>true</c> [continue on processor exception].</param>
        protected abstract void GetStoredProcedures(IDbConnection connection, Func<IDbCommand, IList<IDataParameter>, string, bool> procedureProcessor,
            Func<IDbCommand, IList<IDataParameter>, string, bool> functionProcessor, bool continueOnProcessorException = false);

        /// <summary>
        /// Override to execute a select command.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <returns></returns>
        protected abstract DataSet ExecuteSelect(IDbCommand command);

        /// <summary>
        /// Override to create an implementation specific connection.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <returns></returns>
        protected abstract IDbConnection CreateConnection(string connectionString);

        #endregion
    }
}
