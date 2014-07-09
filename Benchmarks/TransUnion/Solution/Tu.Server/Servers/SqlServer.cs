using System;
using System.Data;
using System.Data.SqlClient;

namespace Tu.Servers
{
    public class SqlServer : DisposableObject, ISqlServer
    {
        #region CTOR

        public SqlServer(string connectionString)
        {
            if(string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }
            Connection = new SqlConnection(connectionString);
        }

        #endregion

        public SqlConnection Connection { get; private set; }

        #region FetchDataTable

        public DataTable FetchDataTable(string commandText, CommandType commandType, params SqlParameter[] parameters)
        {
            if(string.IsNullOrWhiteSpace(commandText))
            {
                throw new ArgumentNullException("commandText");
            }

            DataTable result;
            using(var command = Connection.CreateCommand())
            {
                command.CommandText = commandText;
                command.CommandType = commandType;
                AddParameters(command, parameters);
                try
                {
                    Connection.Open();

                    result = ExecuteReader(command, (CommandBehavior.SchemaOnly & CommandBehavior.KeyInfo),
                        delegate(SqlDataReader reader)
                        {
                            var table = new DataTable();
                            table.Load(reader, LoadOption.OverwriteChanges);
                            return table;
                        });
                }
                finally
                {
                    if(Connection.State == ConnectionState.Open)
                    {
                        Connection.Close();
                    }
                }
            }
            return result;
        }

        #endregion

        #region ExecuteNonQuery

        public void ExecuteNonQuery(string commandText, CommandType commandType, params SqlParameter[] parameters)
        {
            if(string.IsNullOrWhiteSpace(commandText))
            {
                throw new ArgumentNullException("commandText");
            }
            using(var command = Connection.CreateCommand())
            {
                command.CommandText = commandText;
                command.CommandType = commandType;
                AddParameters(command, parameters);
                try
                {
                    Connection.Open();
                    command.ExecuteNonQuery();
                }
                finally
                {
                    if(Connection.State == ConnectionState.Open)
                    {
                        Connection.Close();
                    }
                }
            }
        }

        #endregion

        #region AddParameters

        static void AddParameters(SqlCommand command, SqlParameter[] parameters)
        {
            command.Parameters.Clear();
            if(parameters != null && parameters.Length > 0)
            {
                command.Parameters.AddRange(parameters);
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

        #region OnDisposed

        protected override void OnDisposed()
        {
            if(Connection != null)
            {
                if(Connection.State != ConnectionState.Closed)
                {
                    Connection.Close();
                }
                Connection.Dispose();
            }
        }

        #endregion


    }
}
