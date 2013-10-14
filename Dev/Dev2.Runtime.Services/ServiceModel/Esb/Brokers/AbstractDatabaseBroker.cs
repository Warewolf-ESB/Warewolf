using System.Collections.Concurrent;
using System.IO;
using Dev2.Common;
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
        #region ServiceMethodDataRepo

        private static ConcurrentDictionary<DbSource, ServiceMethodList> _theCache = new ConcurrentDictionary<DbSource, ServiceMethodList>();

        #endregion

        #region Methods

        /// <summary>
        /// Gets the service methods for service.
        /// </summary>
        /// <param name="dbSource">The db source.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">resource</exception>
        public ServiceMethodList GetServiceMethods(DbSource dbSource)
        {
            if(dbSource == null)
            {
                throw new ArgumentNullException("dbSource");
            }

            // Check the cache for a value ;)
            if (!dbSource.ReloadActions)
            {
                ServiceMethodList cacheResult;
                _theCache.TryGetValue(dbSource, out cacheResult);
                if (cacheResult != null)
                {
                    return cacheResult;
                }
            }
            // else reload actions ;)

            var serviceMethods = new ServiceMethodList();

            //
            // Function to handle procedures returned by the data broker
            //
            Func<IDbCommand, IList<IDataParameter>, string, bool> procedureFunc = (command, parameters, helpText) =>
            {
                var serviceMethod = CreateServiceMethod(command, parameters, helpText);
                serviceMethods.Add(serviceMethod);
                return true;
            };

            //
            // Function to handle functions returned by the data broker
            //
            Func<IDbCommand, IList<IDataParameter>, string, bool> functionFunc = (command, parameters, helpText) =>
            {
                var serviceMethod = CreateServiceMethod(command, parameters, helpText);
                serviceMethods.Add(serviceMethod);
                return true;
            };

            //
            // Get stored procedures and functions for this database source
            //
            using(var conn = CreateConnection(dbSource.ConnectionString))
            {
                GetStoredProcedures(conn, procedureFunc, functionFunc);
                conn.Close();
            }

            // Add to cache ;)
            _theCache.TryAdd(dbSource, serviceMethods);

            return serviceMethods;
        }

        /// <summary>
        /// Executes a service in test mode
        /// </summary>
        /// <param name="dbService">The service to execute.</param>
        /// <exception cref="System.ArgumentNullException">resource</exception>
        public IOutputDescription TestService(DbService dbService)
        {
            if (dbService == null)
            {
                throw new ArgumentNullException("dbService");
            }

            if (dbService.Source == null)
            {
                throw new ArgumentNullException("dbService.Source");
            }

            IOutputDescription result = null;
            using (var conn = CreateConnection(((DbSource)dbService.Source).ConnectionString))
            {
                IDbTransaction transaction = conn.BeginTransaction();

                try
                {
                    //
                    // Execute command and normalize XML
                    //
                    var command = CommandFromServiceMethod(conn, transaction, dbService.Method);
                    var dataTable = ExecuteSelect(command);
                    string xmlResult = GetXML(dataTable);
                    xmlResult = NormalizeXmlPayload(xmlResult);

                    //
                    // Map shape of XML
                    //
                    result = OutputDescriptionFactory.CreateOutputDescription(OutputFormats.ShapedXML);
                    IDataSourceShape dataSourceShape = DataSourceShapeFactory.CreateDataSourceShape();
                    result.DataSourceShapes.Add(dataSourceShape);

                    IDataBrowser dataBrowser = DataBrowserFactory.CreateDataBrowser();

                    if (!string.IsNullOrEmpty(xmlResult))
                    {
                        dataSourceShape.Paths.AddRange(dataBrowser.Map(xmlResult));
                    }
                    conn.Close();
                }
                catch (Exception e)
                {
                    ServerLogger.LogError(e);
                }
                finally
                {
                    try
                    {
                        transaction.Rollback();
                        conn.Close();
                    }
                    catch(Exception e)
                    {
                        ServerLogger.LogError("Transactional Error : " + e.Message + Environment.NewLine + e.StackTrace);
                    }
                }
            }

            return result;
        }

        string GetXML(DataTable dataTable)
        {
            DataTable dtCloned = dataTable.Clone();
            foreach (DataColumn dc in dtCloned.Columns)
                dc.DataType = typeof(string);
            foreach (DataRow row in dataTable.Rows)
            {
                dtCloned.ImportRow(row);
            }

            foreach (DataRow row in dtCloned.Rows)
            {
                for (int i = 0; i < dtCloned.Columns.Count; i++)
                {
                    dtCloned.Columns[i].ReadOnly = false;

                    if (string.IsNullOrEmpty(row[i].ToString()))
                        row[i] = string.Empty;
                }
            }
            dtCloned.TableName = "Table";
            using (var stringWriter = new StringWriter())
            {
                dtCloned.WriteXml(stringWriter);
                return stringWriter.ToString();
            }
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
            using(var reader = selectCommand.ExecuteReader(commandBehavior))
            {
                if(reader != null)
                {
                    bool read = true;
                    while(read && reader.Read())
                    {
                        try
                        {
                            read = rowProcessor(reader);
                        }
                        catch(Exception e)
                        {
                            if (!continueOnProcessorException)
                            {
                                throw;
                            }
                            else
                            {
                                ServerLogger.LogError(e);
                            }
                        }
                    }
                }
            }
        }


        #region CreateServiceMethod

        /// <summary>
        /// Creates the service method from a IDbCommand.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="sourceCode">The source code.</param>
        /// <returns></returns>
        /// <author>trevor.williams-ros</author>
        /// <date>2013/02/14</date>
        ServiceMethod CreateServiceMethod(IDbCommand command, IEnumerable<IDataParameter> parameters, string sourceCode)
        {
            return new ServiceMethod(command.CommandText, sourceCode, parameters.Select(MethodParameterFromDataParameter), null, null);
        }

        #endregion

        /// <summary>
        /// Create a MethodParameter from a IDataParameter.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        protected MethodParameter MethodParameterFromDataParameter(IDataParameter parameter)
        {
            return new MethodParameter
            {
                Name = parameter.ParameterName.Replace("@", "")
            };
        }

        /// <summary>
        /// Create a IDbCommand from a ServiceMethod.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="serviceMethod">The service method.</param>
        protected IDbCommand CommandFromServiceMethod(IDbConnection connection, ServiceMethod serviceMethod)
        {
            return CommandFromServiceMethod(connection, null, serviceMethod);
        }

        /// <summary>
        /// Create a IDbCommand from a ServiceMethod.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="transaction">The transaction.</param>
        /// <param name="serviceMethod">The service method.</param>
        protected IDbCommand CommandFromServiceMethod(IDbConnection connection, IDbTransaction transaction, ServiceMethod serviceMethod)
        {
            var command = connection.CreateCommand();

            command.Transaction = transaction;
            command.CommandText = serviceMethod.Name;
            command.CommandType = CommandType.StoredProcedure;

            foreach(var methodParameter in serviceMethod.Parameters)
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
        protected abstract DataTable ExecuteSelect(IDbCommand command);

        /// <summary>
        /// Override to create an implementation specific connection.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <returns></returns>
        protected abstract IDbConnection CreateConnection(string connectionString);

        #endregion
    }
}
