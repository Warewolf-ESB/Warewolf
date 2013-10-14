using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Win32.SafeHandles;
using Dev2.Common;

namespace Dev2.Runtime.ServiceModel.Esb.Brokers
{
    /// <summary>
    /// A Microsoft SQL specific database broker implementation
    /// </summary>
    public class MsSqlBroker : AbstractDatabaseBroker
    {
        const int LOGON32_PROVIDER_DEFAULT = 0;
        //This parameter causes LogonUser to create a primary token. 
        const int LOGON32_LOGON_INTERACTIVE = 2;

        #region Override Methods

        /// <summary>
        /// Returns all stored procedures.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="procedureProcessor">The procedure processor.</param>
        /// <param name="functionProcessor">The function processor.</param>
        /// <param name="continueOnProcessorException">if set to <c>true</c> [continue on processor exception].</param>
        /// <exception cref="System.ArgumentException">Expected type SqlConnection.;connection</exception>
        protected override void GetStoredProcedures(IDbConnection connection, Func<IDbCommand, IList<IDataParameter>, string, bool> procedureProcessor, 
            Func<IDbCommand, IList<IDataParameter>, string, bool> functionProcessor, bool continueOnProcessorException = false)
        {
            var sqlConnection = connection as SqlConnection;

            if (sqlConnection == null)
            {
                throw new ArgumentException(string.Format("Expected type '{0}', received '{1}'.", typeof(SqlConnection), connection), "connection");
            }

            var proceduresDataTable = GetSchema(sqlConnection, "Procedures");
            var procedureDataColumn = GetDataColumn(proceduresDataTable, "ROUTINE_NAME");
            var procedureTypeColumn = GetDataColumn(proceduresDataTable, "ROUTINE_TYPE");
            var procedureSchemaColumn = GetDataColumn(proceduresDataTable, "SPECIFIC_SCHEMA"); // ROUTINE_CATALOG - ROUTINE_SCHEMA ,SPECIFIC_SCHEMA


            // Refactor made this incredablely slow!!!!!!
            foreach(DataRow row in proceduresDataTable.Rows)
            {
                //
                // Create the procedure command
                //
                var fullProcedureName = GetFullProcedureName(row, procedureDataColumn, procedureSchemaColumn);
                var procedureCommand = CreateProcedureCommand(sqlConnection, fullProcedureName);

                //
                // Get procedures parameters
                //
                var parameters = GetProcedureParameters(procedureCommand);

                //
                // Get procedure help text
                //
                var procedureHelpText = GetProcedureHelpText(sqlConnection, fullProcedureName);

                //
                // Determine procedure type, stored procedure or function, and invoke the appropriate processor
                //
                if(IsStoredProcedure(row, procedureTypeColumn))
                {
                    procedureProcessor(procedureCommand, parameters, procedureHelpText);
                }
                else if(IsFunction(row, procedureTypeColumn))
                {
                    functionProcessor(procedureCommand, parameters, procedureHelpText);
                }

            }

        }

        /// <summary>
        /// Executes a select command.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <exception cref="System.ArgumentException">command</exception>
        protected override DataTable ExecuteSelect(IDbCommand command)
        {
            var dataset = new DataTable();

            var sqlCommand = command as SqlCommand;
            if (sqlCommand == null)
            {
                throw new ArgumentException(string.Format("Expected type {0}.", typeof(SqlCommand)), "command");
            }

            var adapter = new SqlDataAdapter(sqlCommand);

            adapter.Fill(dataset);

            return dataset;
        }

        /// <summary>
        /// Creates a connection.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        protected override IDbConnection CreateConnection(string connectionString)
        {
            var result = new SqlConnection();
            string username = null;
            string pass = null;
            connectionString = ImpersonateDomainUser(connectionString, out username, out pass);
            if(!RequiresAuth(username))
            {
                result = new SqlConnection(connectionString);
                result.Open();
            }else
            {
                // handle UNC path
                PathOperations.SafeTokenHandle safeTokenHandle;

                try
                {
                    string user = ExtractUserName(username);
                    string domain = ExtractDomain(username);
                    bool loginOk = LogonUser(user, domain, pass, LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT, out safeTokenHandle);


                    if (loginOk)
                    {
                        using (safeTokenHandle)
                        {

                            WindowsIdentity newID = new WindowsIdentity(safeTokenHandle.DangerousGetHandle());
                            using (WindowsImpersonationContext impersonatedUser = newID.Impersonate())
                            {
                                // Do the operation here
                                result = new SqlConnection(connectionString);
                                result.Open();

                                impersonatedUser.Undo(); // remove impersonation now
                            }
                        }
                    }
                    else
                    {
                        // login failed
                        throw new Exception("Failed to authenticate with user [ " + username + " ].");
                    }
                }
                catch (Exception ex)
                {
                    ServerLogger.LogError(ex);
                    throw;
                }
            }
            return result;
        }

        /// <summary>
        /// Mormalizes a data payload.
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <returns></returns>
        protected override string NormalizeXmlPayload(string payload)
        {
            StringBuilder result = new StringBuilder();

            XmlDocument xDoc = new XmlDocument();
            xDoc.LoadXml(payload);
            XmlNodeList nl = xDoc.SelectNodes("//NewDataSet/Table/*[starts-with(local-name(),'XML_')]");
            int foundXMLFrags = 0;

            foreach (XmlNode n in nl)
            {
                string tmp = n.InnerXml;
                result = result.Append(tmp);
                foundXMLFrags++;
            }

            string res = result.ToString();

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

        //07.03.2013: Ashley Lewis - PBI 8720
        public DataTable GetDatabasesSchema(string connectionString)
        {
            using (var conn = (CreateConnection(connectionString) as SqlConnection))
            {
                DataTable tblDatabases = null;
                if(conn != null)
                {
                    tblDatabases = conn.GetSchema("Databases");
                    conn.Close();
                }
                return tblDatabases;
            }
        }

        public List<string> GetDatabases(DataTable tblDatabases)
        {
            if(tblDatabases == null)
            {
                throw new ArgumentNullException();
            }
            var result = new List<string>();
            result.AddRange(from DataRow row in tblDatabases.Rows
                            select (row["database_name"] ?? string.Empty).ToString());

            //2013.07.10: Ashley Lewis for bug 9933 - sort database list
            result.Sort();
            return result;
        }

        #endregion

        public sealed class SafeTokenHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            private SafeTokenHandle()
                : base(true)
            {
            }

            [DllImport("kernel32.dll")]
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            [SuppressUnmanagedCodeSecurity]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool CloseHandle(IntPtr handle);

            protected override bool ReleaseHandle()
            {
                return CloseHandle(handle);
            }
        }

        #region Private Methods

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool LogonUser(String lpszUsername, String lpszDomain, String lpszPassword,
            int dwLogonType, int dwLogonProvider, out PathOperations.SafeTokenHandle phToken);

        public static string ExtractUserName(string username)
        {
            string result = string.Empty;

            int idx = username.IndexOf("\\", StringComparison.InvariantCulture);

            if (idx > 0)
            {
                result = username.Substring((idx + 1));
            }

            return result;
        }

        public static string ExtractDomain(string username)
        {
            string result = string.Empty;

            int idx = username.IndexOf("\\", StringComparison.InvariantCulture);

            if (idx > 0)
            {
                result = username.Substring(0, idx);
            }

            return result;
        }

        public static bool RequiresAuth(string userName)
        {

            bool result = false;

            if (!string.IsNullOrEmpty(userName))
            {
                result = true;
            }

            return result;
        }

        /// <summary>
        /// Determines whether the specified row represents a stored procedure.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <param name="procedureTypeColumn">The procedure type column.</param>
        /// <returns>
        ///   <c>true</c> if the specified row is a stored procedure; otherwise, <c>false</c>.
        /// </returns>
        private bool IsStoredProcedure(DataRow row, DataColumn procedureTypeColumn)
        {
            if (row == null || procedureTypeColumn == null)
            {
                return false;
            }

            return row[procedureTypeColumn].ToString().Equals("PROCEDURE");
        }

        /// <summary>
        /// Determines whether the specified row represents a stored function.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <param name="procedureTypeColumn">The procedure type column.</param>
        /// <returns>
        ///   <c>true</c> if the specified row is a stored function; otherwise, <c>false</c>.
        /// </returns>
        private bool IsFunction(DataRow row, DataColumn procedureTypeColumn)
        {
            if (row == null || procedureTypeColumn == null)
            {
                return false;
            }

            return !row[procedureTypeColumn].ToString().Equals("PROCEDURE");
        }

        /// <summary>
        /// Gets the help text for a procedure/function.
        /// </summary>
        /// <param name="sqlConnection">The SQL connection.</param>
        /// <param name="fullProcedureName">Full name of the procedure.</param>
        private string GetProcedureHelpText(SqlConnection sqlConnection, string fullProcedureName)
        {
            StringBuilder sb = new StringBuilder();

            SqlCommand helpTextCommand = CreateHelpTextCommand(sqlConnection, fullProcedureName);
            try
            {
                Func<IDataReader, bool> helptextProcessor = reader =>
                {
                    object value = reader.GetValue(0);
                    if (value != null)
                    {
                        sb.Append(value.ToString());
                    }

                    return true;
                };

                ExecuteSelect(helpTextCommand, helptextProcessor, true, CommandBehavior.Default);
            }
            catch (Exception e)
            {
                ServerLogger.LogError(e);   
            }finally
            {
                helpTextCommand.Dispose();
            }

            if (sb.Length == 0)
            {
                sb.Append("Error : Could not fetch source.");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Gets the parameters for a procedure/function.
        /// </summary>
        /// <param name="procedureCommand">The procedure command.</param>
        private List<IDataParameter> GetProcedureParameters(SqlCommand procedureCommand)
        {
            List<IDataParameter> parameters = new List<IDataParameter>();

            if (TryLoadCommandParameters(procedureCommand))
            {
                foreach (SqlParameter parameter in procedureCommand.Parameters)
                {
                    if (!parameter.ParameterName.Equals("@RETURN_VALUE"))
                    {
                        parameters.Add(parameter);
                    }
                }
            }

            return parameters;
        }

        /// <summary>
        /// Gets the data column from a data table.
        /// </summary>
        /// <param name="dataTable">The data table.</param>
        /// <param name="columnName">Name of the column.</param>
        /// <exception cref="System.Exception"></exception>
        private DataColumn GetDataColumn(DataTable dataTable, string columnName)
        {
            DataColumn dataColumn = dataTable.Columns[columnName];
            if (dataColumn == null)
            {
                throw new Exception(string.Format("MSSQL Intergate : Unable to load '{0}' column of '{1}'.", columnName, dataTable.TableName));
            }
            return dataColumn;
        }

        /// <summary>
        /// Gets the schema from a data table.
        /// </summary>
        /// <param name="sqlConnection">The SQL connection.</param>
        /// <param name="collectionName">Name of the collection.</param>
        /// <exception cref="System.Exception"></exception>
        private DataTable GetSchema(SqlConnection sqlConnection, string collectionName)
        {
            DataTable proceduresDataTable = sqlConnection.GetSchema(collectionName);
            if (proceduresDataTable == null)
            {
                throw new Exception(string.Format("MSSQL Intergate : Unable to load the '{0}' schema.", collectionName));
            }
            return proceduresDataTable;
        }

        /// <summary>
        /// Creates a command for retrieving the help text of a procedure/function.
        /// </summary>
        /// <param name="sqlConnection">The SQL connection.</param>
        /// <param name="fullProcedureName">Full name of the procedure.</param>
        private SqlCommand CreateHelpTextCommand(SqlConnection sqlConnection, string fullProcedureName)    
        {
            SqlCommand cmd = new SqlCommand("sp_helptext '" + fullProcedureName + "'", sqlConnection) 
            { 
                CommandType = CommandType.Text 
            };

            return cmd;
        }

        /// <summary>
        /// Creates a command for executing a procedure/function.
        /// </summary>
        /// <param name="sqlConnection">The SQL connection.</param>
        /// <param name="fullCallName">Full name of the call.</param>
        private SqlCommand CreateProcedureCommand(SqlConnection sqlConnection, string fullCallName)    
        {
            SqlCommand cmd = new SqlCommand(fullCallName, sqlConnection) 
            { 
                CommandType = CommandType.StoredProcedure 
            };

            return cmd;
        }

        /// <summary>
        /// Gets the full name of the procedure/function.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <param name="procedureDataColumn">The procedure data column.</param>
        /// <param name="procedureSchemaColumn">The procedure schema column.</param>
        private string GetFullProcedureName(DataRow row, DataColumn procedureDataColumn, DataColumn procedureSchemaColumn)
        {
            string procedureName = row[procedureDataColumn].ToString();
            string schemaName = row[procedureSchemaColumn].ToString();
            return schemaName + "." + procedureName;
        }

        /// <summary>
        /// Load parameters of a command.
        /// </summary>
        /// <param name="command">The command.</param>
        private bool TryLoadCommandParameters(SqlCommand command)
        {
            try
            {
                SqlCommandBuilder.DeriveParameters(command);
                return true;
            }
            catch (Exception ex)
            {
                ServerLogger.LogError(ex);
                return false;
            }
        }

        /// <summary>
        /// Load domain of a connection string.
        /// </summary>
        /// <param name="connectionString">A connection string.</param>
        /// <param name="domain">The domain described by the connection string.</param>
        private static bool TryLoadDomainParameters(string connectionString, out string UsernameAndPassword)
        {
            try
            {
                if (connectionString.IndexOf("User ID=", System.StringComparison.Ordinal) >= 0)
                {
                    var userNameIndex = connectionString.IndexOf("User ID=", System.StringComparison.Ordinal);
                    var passwordIndex = connectionString.IndexOf("Password=", System.StringComparison.Ordinal);
                    if (connectionString.IndexOf(';', passwordIndex) >= 0 && connectionString.IndexOf(';', passwordIndex) > userNameIndex)
                    {
                        int end = connectionString.IndexOf(';', passwordIndex) + 1;
                        if (end > 0)
                        {
                            UsernameAndPassword = connectionString.Substring(userNameIndex, (end - userNameIndex));
                            return true;
            }
                    }
                }
            }
            catch (Exception e)
            {
                ServerLogger.LogError(e);                
                UsernameAndPassword = null;
                return false;
            }
            UsernameAndPassword = null;
                return false;
            }

        /// <summary>
        /// Inject domain parameters into a connection string.
        /// </summary>
        /// <param name="connectionString">A connection string.</param>
        private static string InjectDomainParams(string connectionString)
        {
            const string InjectTokenStart = ";Initial Catalog=";
            const string ToInject = "Integrated Security=SSPI;";
            var injectIndex = connectionString.IndexOf(';', connectionString.IndexOf(InjectTokenStart, System.StringComparison.Ordinal)) + InjectTokenStart.Length;
            var end = connectionString.IndexOf(";", injectIndex);
            var result = connectionString.Remove( (end+1) );
            result = result.Insert((end+1), ToInject);
            return result;
        }

        /// <summary>
        /// Alter connection string
        /// </summary>
        /// <param name="connectionString">A connection string.</param>
        public static string ImpersonateDomainUser(string connectionString, out string Username, out string Password)
        {
            // Inpersonate HERE
            int userID = connectionString.IndexOf("User ID=", StringComparison.Ordinal);
            if (userID != -1 && connectionString.IndexOf("\\", userID, StringComparison.Ordinal) >= 0)
            {
                string tryGetParams = null;
                if (TryLoadDomainParameters(connectionString, out tryGetParams))
                {
                    if (!string.IsNullOrEmpty(tryGetParams))
                    {
                        int userNameLen = (tryGetParams.IndexOf("Password=", StringComparison.Ordinal) - "User ID=".Length)-1;

                        Username = tryGetParams.Substring("User ID=".Length, userNameLen);
                        int passwdStart = tryGetParams.IndexOf("Password=", StringComparison.Ordinal) + "Password=".Length;
                        int passwdEnd = tryGetParams.IndexOf(";", passwdStart, StringComparison.Ordinal);
                        Password = tryGetParams.Substring(passwdStart, (passwdEnd - passwdStart));
                        return InjectDomainParams(connectionString);
                    }
                }

            }
            Username = null;
            Password = null;
            return connectionString;
        }

        /// <summary>
        /// Get username from conneciton string
        /// </summary>
        /// <param name="connectionString">A connection string.</param>
        private static string GetUserName(string connectionString)
        {
            const string UsernameToken = "User ID=";
            var UsernameIndex = connectionString.IndexOf(UsernameToken, System.StringComparison.Ordinal) + UsernameToken.Length;
            //get from username token end to password token start
            return connectionString.Substring(UsernameIndex, connectionString.IndexOf(";Password=", System.StringComparison.Ordinal) - UsernameIndex);
        }

        /// <summary>
        /// Get password from connection string
        /// </summary>
        /// <param name="connectionString">A connection string.</param>
        private static string GetPassword(string connectionString)
        {
            const string PassToken = "Password=";
            var PassIndex = connectionString.IndexOf(PassToken, System.StringComparison.Ordinal) + PassToken.Length;
            //get from username token end to password token start
            return connectionString.Substring(PassIndex, connectionString.IndexOf(';', PassIndex) - PassIndex);
        }

        #endregion
    }
}
