using Dev2.Runtime.ServiceModel.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Unlimited.Framework.Converters.Graph.Interfaces;

namespace Dev2.Runtime.ServiceModel.Esb.Brokers
{
    /// <summary>
    /// Provides base logic for database brokers.
    /// </summary>
    public abstract class AbstractDatabaseBroker : IEsbEndpoint
    {
        #region Methods

        /// <summary>
        /// Gets the service methods for this esb endpoint.
        /// </summary>
        /// <param name="resource">The resource.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">resource</exception>
        /// <exception cref="System.Exception"></exception>
        public ServiceMethodList GetServiceMethods(Resource resource)
        {
            if (resource == null)
            {
                throw new ArgumentNullException("resource");
            }

            var dbSource = resource as DbSource;
            if (dbSource == null)
            {
                throw new Exception(string.Format("Unexpected source type, recieved '{0}', expected '{1}'.", resource.GetType(), typeof(DbSource)));
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
            using (var conn = CreateConnection(dbSource.ConnectionString))
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

            var dbSource = resource as DbSource;
            if (dbSource == null)
            {
                throw new Exception(string.Format("Unexpected source type, recieved '{0}', expected '{1}'.", resource.GetType(), typeof(DbSource)));
            }

            IOutputDescription result = null;
            using (var conn = CreateConnection(dbSource.ConnectionString))
            {
                conn.Open();
                IDbTransaction transaction = conn.BeginTransaction();

                try
                {
                    var command = CommandFromServiceMethod(conn, serviceMethod);

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
                    catch (Exception e)
                    {
                        TraceWriter.WriteTrace("Transactional Error : " + e.Message + Environment.NewLine + e.StackTrace);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Executes a service method of this ESB endpoint.
        /// </summary>
        /// <param name="resource">The resource.</param>
        /// <param name="serviceMethod">The service method.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public Guid ExecuteServiceMethod(Resource resource, ServiceMethod serviceMethod)
        {
            throw new NotImplementedException();
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

        #region Abstract Methods

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
        /// Override to create a connection.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <returns></returns>
        protected abstract IDbConnection CreateConnection(string connectionString);

        #endregion
    }
}
