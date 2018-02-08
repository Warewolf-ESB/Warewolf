using System.Data;
using System.Data.SqlClient;

namespace Dev2.Services.Sql
{
    public class SqlConnectionWrapper : ISqlConnection
    {
        private readonly string _actualConnectionString;
        SqlConnection _connection;
        public SqlConnectionWrapper(string connString)
        {
            var conStrBuilder = new SqlConnectionStringBuilder(connString)
            {
                ConnectTimeout = 20,
                MaxPoolSize = 100,
                MultipleActiveResultSets = true,
                Pooling = true,
                ApplicationName = "Warewolf Service"
            };
            _actualConnectionString = conStrBuilder.ConnectionString;
            CreateConnection();
        }

        private void CreateConnection()
        {
            _connection = new SqlConnection(_actualConnectionString);
        }

        public bool FireInfoMessageEventOnUserErrors
        {
            get
            {

                EnsureOpen();
                return _connection.FireInfoMessageEventOnUserErrors;
            }
            set
            {
                EnsureOpen();
                _connection.FireInfoMessageEventOnUserErrors = value;
            }
        }

        public bool StatisticsEnabled
        {
            get
            {
                EnsureOpen();
                return _connection.StatisticsEnabled;
            }
            set
            {
                EnsureOpen();
                _connection.StatisticsEnabled = value;
            }
        }
        public event SqlInfoMessageEventHandler InfoMessage;

        public ConnectionState State
        {
            get
            {
                EnsureOpen();
                return _connection.State;
            }
        }

        public IDbTransaction BeginTransaction()
        {
            EnsureOpen();
            return _connection.BeginTransaction();
        }

        public void EnsureOpen()
        {
            if (_connection == null)
            {
                CreateConnection();
            }
            if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }
        }

        public DataTable GetSchema(string table)
        {
            EnsureOpen();
            return _connection.GetSchema(table);
        }

        public IDbCommand CreateCommand()
        {
            EnsureOpen();
            return _connection.CreateCommand();
        }

        public void SetInfoMessage(SqlInfoMessageEventHandler a)
        {
            EnsureOpen();
            _connection.InfoMessage += a;
        }

        public void Dispose()
        {
            if (_connection != null)
            {
                _connection.Dispose();
                _connection = null;
            }
        }
    }
}