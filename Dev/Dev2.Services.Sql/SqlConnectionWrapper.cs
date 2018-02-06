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
                if (_connection == null)
                {
                    Open();
                }
                return _connection.FireInfoMessageEventOnUserErrors;
            }
            set
            {
                if (_connection == null)
                {
                    Open();
                }
                _connection.FireInfoMessageEventOnUserErrors = value;
            }
        }

        public bool StatisticsEnabled
        {
            get
            {
                if (_connection == null)
                {
                    Open();
                }
                return _connection.StatisticsEnabled;
            }
            set
            {
                if (_connection == null)
                {
                    Open();
                }
                _connection.StatisticsEnabled = value;
            }
        }
        public event SqlInfoMessageEventHandler InfoMessage;

        public ConnectionState State
        {
            get
            {
                if (_connection == null)
                {
                    Open();
                }
                return _connection.State;
            }
        }

        public IDbTransaction BeginTransaction()
        {
            if (_connection == null)
            {
                Open();
            }
            return _connection.BeginTransaction();
        }

        public void Open()
        {
            if (_connection == null)
            {
                CreateConnection();
            }
            _connection.Open();
        }

        public DataTable GetSchema(string table)
        {
            if (_connection == null)
            {
                Open();
            }
            return _connection.GetSchema(table);
        }

        public IDbCommand CreateCommand()
        {
            if (_connection == null)
            {
                Open();
            }
            return _connection.CreateCommand();
        }

        public void SetInfoMessage(SqlInfoMessageEventHandler a)
        {
            if (_connection == null)
            {
                Open();
            }
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