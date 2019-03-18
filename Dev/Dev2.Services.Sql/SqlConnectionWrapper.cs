#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
using System.Data;
using System.Data.SqlClient;

namespace Dev2.Services.Sql
{
    public class SqlConnectionWrapper : ISqlConnection
    {
        private string _actualConnectionString;
        SqlConnection _connection;
        public SqlConnectionWrapper()
        {
        }

        public string CreateConnectionString(string connString)
        {
            var conStrBuilder = new SqlConnectionStringBuilder(connString)
            {
                ConnectTimeout = 30,
                MaxPoolSize = 100,
                Pooling = true,
                ApplicationName = "Warewolf Service"
            };
            _actualConnectionString = conStrBuilder.ConnectionString;
            return _actualConnectionString;
        }

        public SqlConnectionWrapper(string connString)
        {
            CreateConnectionString(connString);
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

        public string ActualConnectionString => _actualConnectionString;

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