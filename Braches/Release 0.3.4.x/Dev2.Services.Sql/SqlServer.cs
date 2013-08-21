using System;
using System.Data;
using System.Data.SqlClient;

namespace Dev2.Services.Sql
{
    public class SqlServer : IDisposable
    {
        SqlConnection _connection;
        SqlCommand _command;
        Impersonator _impersonator;

        #region Connect

        public bool Connect(string commandText, CommandType commandType, string server, string databaseName, int port = 0, string userID = null, string password = null)
        {
            bool isIntegrated;
            if(string.IsNullOrEmpty(userID))
            {
                isIntegrated = true;
            }
            else
            {
                var user = userID.Split('\\');
                isIntegrated = user.Length > 1;
                if(isIntegrated)
                {
                    _impersonator = new Impersonator();
                    if(!_impersonator.Impersonate(user[1], user[0], password))
                    {
                        return false;
                    }
                }
            }

            var connectionString = string.Format("Data Source={0}{2};Initial Catalog={1};{3}", server, databaseName, (port > 0 ? "," + port : string.Empty),
                            isIntegrated ? "Integrated Security=SSPI;" : string.Format("User ID={0};Password={1};", userID, password));

            _connection = new SqlConnection(connectionString);
            _command = _connection.CreateCommand();
            _command = new SqlCommand(commandText, _connection)
            {
                CommandType = commandType
            };
            _connection.Open();
            return true;
        }

        #endregion

        #region FetchDataTable

        public DataTable FetchDataTable(params SqlParameter[] parameters)
        {
            VerifyConnection();
            AddParameters(parameters);

            return ExecuteReader(_command, (CommandBehavior.SchemaOnly & CommandBehavior.KeyInfo),
                delegate(SqlDataReader reader)
                {
                    var table = new DataTable();
                    table.Load(reader, LoadOption.OverwriteChanges);
                    return table;
                });
        }

        #endregion

        #region FetchDataSet

        public DataSet FetchDataSet(params SqlParameter[] parameters)
        {
            VerifyConnection();
            AddParameters(parameters);

            using(var dataSet = new DataSet())
            {
                using(var adapter = new SqlDataAdapter(_command))
                {
                    adapter.Fill(dataSet);
                }
                return dataSet;
            }
        }

        #endregion

        #region AddParameters

        void AddParameters(SqlParameter[] parameters)
        {
            _command.Parameters.Clear();
            if(parameters != null && parameters.Length > 0)
            {
                _command.Parameters.AddRange(parameters);
            }
        }

        #endregion

        #region VerifyConnection

        void VerifyConnection()
        {
            if(_connection == null)
            {
                throw new Exception("Please connect first.");
            }

        }

        #endregion

        #region ExecuteReader

        static T ExecuteReader<T>(SqlCommand command, CommandBehavior commandBehavior, Func<SqlDataReader, T> handler)
        {
            using(var reader = command.ExecuteReader(commandBehavior))
            {
                return handler(reader);
            }
        }

        #endregion

        #region IDisposable

        ~SqlServer()
        {
            // Do not re-create Dispose clean-up code here. 
            // Calling Dispose(false) is optimal in terms of 
            // readability and maintainability.
            Dispose(false);
        }

        bool _disposed;

        // Implement IDisposable. 
        // Do not make this method virtual. 
        // A derived class should not be able to override this method. 
        public void Dispose()
        {
            Dispose(true);
            // This object will be cleaned up by the Dispose method. 
            // Therefore, you should call GC.SupressFinalize to 
            // take this object off the finalization queue 
            // and prevent finalization code for this object 
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        // Dispose(bool disposing) executes in two distinct scenarios. 
        // If disposing equals true, the method has been called directly 
        // or indirectly by a user's code. Managed and unmanaged resources 
        // can be disposed. 
        // If disposing equals false, the method has been called by the 
        // runtime from inside the finalizer and you should not reference 
        // other objects. Only unmanaged resources can be disposed. 
        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called. 
            if(!_disposed)
            {
                // If disposing equals true, dispose all managed 
                // and unmanaged resources. 
                if(disposing)
                {
                    // Dispose managed resources.
                    if(_command != null)
                    {
                        _command.Dispose();
                    }

                    if(_connection != null)
                    {
                        if(_connection.State != ConnectionState.Closed)
                        {
                            _connection.Close();
                        }
                        _connection.Dispose();
                    }

                    if(_impersonator != null)
                    {
                        _impersonator.Dispose();
                    }

                }

                // Call the appropriate methods to clean up 
                // unmanaged resources here. 
                // If disposing is false, 
                // only the following code is executed.

                // Note disposing has been done.
                _disposed = true;
            }
        }

        #endregion

    }
}
