using System;
using System.Collections.Generic;
using System.Text;
using Dev2.DB_Sanity;
using Dev2.DB_Support;
using System.Data.SqlClient;
using System.Data;
using System.Xml;
using Unlimited.Framework.Converters.Graph;
using Unlimited.Framework.Converters.Graph.Interfaces;

namespace Dev2.DB_Helper
{
    internal class MS_SQL_Helper : IDBHelper
    {
        /// <summary>
        /// Create a DB connection string wrapped by an object
        /// </summary>
        /// <param name="properties"></param>
        /// <returns></returns>
        public DBConnectionString CreateConnectionString(DBConnectionProperties properties)
        {
            string result = string.Empty;

            if (string.IsNullOrWhiteSpace(properties.User))
            {
                result = string.Format("server={0};database={1};integrated security=true", properties.Server, properties.DB);
            }
            else
            {
                result = string.Format("server={0};database={1};integrated security=false;User Id={2};Password={3}", properties.Server, properties.DB, properties.User, properties.Pass);
            }

            return new DBConnectionString(result);
        }

        /// <summary>
        /// Extract the stored procs and functions from the db
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public string ExtractCodedEntities(DBConnectionString str)
        {
            string result = string.Empty;

            Exception sqlCmdEx = null;

            using (SqlConnection cn = new SqlConnection(str.Value))
            {
                try
                {
                    cn.Open();
                    StringBuilder xmlDbResponse = new StringBuilder();
                    StringBuilder procedures = new StringBuilder();
                    StringBuilder functions = new StringBuilder();
                    DataTable procedureDataTable = cn.GetSchema("Procedures");
                    xmlDbResponse.Append("<InformationSchema>");
                    procedures.Append("<Procedures>");
                    functions.Append("<Functions>");
                    DataColumn procedureDataColumn = procedureDataTable.Columns["ROUTINE_NAME"];
                    DataColumn procedureTypeColumn = procedureDataTable.Columns["ROUTINE_TYPE"];
                    DataColumn procedureSchema = procedureDataTable.Columns["SPECIFIC_SCHEMA"]; // ROUTINE_CATALOG - ROUTINE_SCHEMA ,SPECIFIC_SCHEMA

                    StringBuilder pAppend = new StringBuilder("<Procedures>");
                    StringBuilder fAppend = new StringBuilder("<Functions>");

                    StringBuilder toAppend = null;

                    if (procedureDataColumn != null)
                    {
                        foreach (DataRow row in procedureDataTable.Rows)
                        {
                            string tagName = string.Empty;

                            if (row[procedureTypeColumn].ToString().Equals("PROCEDURE"))
                            {
                                tagName = "Procedure";
                                toAppend = pAppend;

                            }
                            else
                            {
                                tagName = "Function";
                                toAppend = fAppend;
                            }



                            string procedureName = row[procedureDataColumn].ToString();
                            string schemaName = row[procedureSchema].ToString();
                            string fullCallName = schemaName + "." + procedureName;
                            SqlCommand cmd = new SqlCommand(fullCallName, cn);
                            cmd.CommandType = CommandType.StoredProcedure;

                            try
                            {
                                bool ok = false;

                                // check for stored proc
                                try
                                {
                                    SqlCommandBuilder.DeriveParameters(cmd);
                                    ok = true;
                                }
                                catch (Exception){
                                    // Left empty to avoid throwing an error when the stored proc does not exist
                                }

                                if (ok)
                                {
                                    toAppend.Append("<" + tagName + ">");
                                    toAppend.Append("<" + tagName + "Name>" + fullCallName + "</" + tagName + "Name>");

                                    foreach (SqlParameter p in cmd.Parameters)
                                    {
                                        if (p.ParameterName.Equals("@RETURN_VALUE"))
                                        {
                                            continue;
                                        }
                                        toAppend.Append("<" + p.ParameterName.Replace("@", "") + ">" + p.DbType + "</" + p.ParameterName.Replace("@", "") + ">");
                                    }

                                    // append body
                                    SqlCommand cmdBody = new SqlCommand("sp_helptext '" + fullCallName + "'", cn);
                                    cmdBody.CommandType = CommandType.Text;

                                    try
                                    {
                                        SqlDataReader sdr = cmdBody.ExecuteReader();
                                        StringBuilder tmpData = new StringBuilder();

                                        if (sdr.HasRows)
                                        {

                                            while (sdr.Read())
                                            {
                                                string t = sdr.GetValue(0).ToString();
                                                tmpData.Append(t);
                                            }
                                            sdr.Close();
                                        }

                                        result = "<Body><![CDATA[" + tmpData.ToString().Replace("\r\n", "<br/>") + "]]></Body>";

                                        toAppend.Append(result);
                                    }
                                    catch (Exception e)
                                    {
                                        sqlCmdEx = new Exception("Error Fetching Body");
                                        TraceWriter.WriteTrace("SQL Intergate : " + e.Message + Environment.NewLine + e.StackTrace);
                                    }
                                    finally
                                    {
                                        cmdBody.Dispose();
                                    }

                                    toAppend.Append("</" + tagName + ">");
                                }

                            }
                            catch (Exception e)
                            {
                                //sqlCmdEx = e;
                                TraceWriter.WriteTrace("SQL Intergate : " + e.Message + Environment.NewLine + e.StackTrace);
                            }
                            finally
                            {
                                cmd.Dispose();
                            }
                        }
                    }

                    pAppend.Append("</Procedures>");
                    fAppend.Append("</Functions>");
                    xmlDbResponse.Append(pAppend.ToString());
                    xmlDbResponse.Append(fAppend.ToString());
                    xmlDbResponse.Append("</InformationSchema>");

                    //Alert the caller that request returned no data
                    if (xmlDbResponse == null || string.IsNullOrEmpty(xmlDbResponse.ToString()))
                    {
                        result = "<Error>The request yielded no response from the data store.</Error>";
                    }
                    else
                    {
                        result = "<Dev2Resources>" + xmlDbResponse.ToString() + "</Dev2Resources>";
                    }

                }
                catch (SqlException sqlEx)
                {
                    sqlCmdEx = sqlEx;
                }
                catch (Exception ex)
                {
                    sqlCmdEx = ex;
                }
                finally
                {
                    cn.Close();
                }

                if (sqlCmdEx != null)
                {
                    result = "<Error>" + sqlCmdEx + "</Error>";
                }
            }

            return result;
        }

        /// <summary>
        /// Tickle a DB's logical entity for information
        /// </summary>
        /// <param name="str"></param>
        /// <param name="proc"></param>
        /// <param name="parameters"> a ; delimited list of data</param>
        /// <returns>the AnythingToXML data </returns>
        public string TickleDBLogic(DBConnectionString str, string proc, string parameters)
        {
            string result = "<Error>There was a problem invoking " + proc + " with [ " + parameters + " ] </Error>";

            string outputXML = string.Empty;
            List<string> parameterNames = new List<string>();
            List<string> parameterValues = new List<string>();
            //Populate parameterNames and parameterValues lists
            for (int i = 0; i < parameters.Split(';').Length; i++)
            {
                try
                {
                    parameterValues.Add(parameters.Split(';')[i].Split('=')[1]); //this throws an exception if parameters.Split(';')[i].Split('=')[1] is an empty string                    
                    parameterNames.Add(parameters.Split(';')[i].Split('=')[0]);
                }
                catch { }
            }

            try
            {
                //Connect to database execute stored procedure and return result
                DataSet dataset = new DataSet();
                using (SqlConnection conn = new SqlConnection(str.Value))
                {

                    conn.Open();
                    SqlCommand cmd = new SqlCommand(proc, conn);
                    string tranName = Guid.NewGuid().ToString().Substring(0, 30);
                    SqlTransaction transaction = conn.BeginTransaction(tranName);
                    
                    cmd.CommandType = CommandType.StoredProcedure;
                    
                    for (int j = 0; j < parameterNames.Count; j++)
                    {
                        SqlParameter p = new SqlParameter();
                        p.ParameterName = string.Format("@{0}", parameterNames[j]);
                        p.SqlValue = parameterValues[j];
                        cmd.Parameters.Add(p);
                    }
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dataset);

                    try
                    {
                        transaction.Rollback();
                    }
                    catch (Exception e)
                    {
                        TraceWriter.WriteTrace("Transactional Error : " + e.Message + Environment.NewLine + e.StackTrace);
                    }
                    conn.Close();
                }
                // Travis.Frisinger : 27.09.2012 - Use the new DB sanitizer to clean the data ;)
                outputXML = DataSanitizerFactory.GenerateNewSanitizer(enSupportedDBTypes.MSSQL).SanitizePayload(dataset.GetXml());

                result = outputXML;
            }
            catch (Exception ex)
            {
                result = "<Error>" + ex.Message + "</Error>";
            }

            IOutputDescription ouputDescription = OutputDescriptionFactory.CreateOutputDescription(OutputFormats.ShapedXML);
            IDataSourceShape dataSourceShape = DataSourceShapeFactory.CreateDataSourceShape();

            ouputDescription.DataSourceShapes.Add(dataSourceShape);

            IDataBrowser dataBrowser = DataBrowserFactory.CreateDataBrowser();

            if (!string.IsNullOrEmpty(outputXML))
            {
                dataSourceShape.Paths.AddRange(dataBrowser.Map(outputXML));

                IOutputDescriptionSerializationService outputDescriptionSerializationService =
                    OutputDescriptionSerializationServiceFactory.CreateOutputDescriptionSerializationService();
                result = outputDescriptionSerializationService.Serialize(ouputDescription);
            }


            return result;
        }

        public string ListDatabases(DBConnectionString str)
        {

            string result = "<Error>Could not list databases</Error>";

            //dynamic returnData = new UnlimitedObject();
            //string returnVal = string.Empty;
            //dynamic serviceData = new UnlimitedObject("Dev2Resources");
            Exception sqlCmdEx = null;

            // CreateMsSqlConnectionString(serverName, "master", username, password)
            using (SqlConnection cn = new SqlConnection(str.Value))
            {
                try
                {
                    //Create a SqlCommand to execute at the source
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = cn;
                    cmd.CommandText = "sp_databases";

                    cn.Open();
                    StringBuilder xmlDbResponse = new StringBuilder();
                    try
                    {
                        XmlReader xmlDbResponseReader = cmd.ExecuteXmlReader();
                        while (xmlDbResponseReader.Read())
                        {
                            xmlDbResponse.Append(xmlDbResponseReader.ReadOuterXml());
                        }
                    }
                    catch (InvalidOperationException)
                    {
                        SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                        DataSet r = new DataSet();
                        adapter.Fill(r);

                        xmlDbResponse.Append(r.GetXml());
                    }

                    cn.Close();

                    //Alert the caller that request returned no data
                    if (xmlDbResponse == null || string.IsNullOrEmpty(xmlDbResponse.ToString()))
                    {
                        result = "<Error>The request yielded no response from the data store.</Error>";
                    }
                    else
                    {
                        result = xmlDbResponse.ToString();
                    }

                    cmd.Dispose();


                    result = "<Dev2Resources>" + result + "</Dev2Resources>";

                    //serviceData.AddResponse(returnData);
                    //returnVal = GetResourceNameAsXml(returnData.XmlString, serviceData);

                    //cmd.Dispose();
                }
                catch (SqlException sqlEx)
                {
                    sqlCmdEx = sqlEx;
                }
                catch (Exception ex)
                {
                    sqlCmdEx = ex;
                }

                if (sqlCmdEx != null)
                {
                    result = "<Error>" + sqlCmdEx.Message + "</Error>";
                }
            }

            return result; 
        }

        public enSupportedDBTypes HandlesType()
        {
            return enSupportedDBTypes.MSSQL;
        }

        
    }
}
