using System.Data;
using System.Data.SqlClient;

namespace Dev2.Services.Sql
{
    public class SqlConnectionWrapper : ISqlConnection
    {
        private readonly SqlConnection _connection;
        public SqlConnectionWrapper(string connString)
        {
            _connection = UniqueDbConnectionGenerator.GetConnection(connString);
        }

        public bool FireInfoMessageEventOnUserErrors
        {
            get => _connection.FireInfoMessageEventOnUserErrors;
            set => _connection.FireInfoMessageEventOnUserErrors = value;
        }

        public bool StatisticsEnabled
        {
            get => _connection.StatisticsEnabled;
            set => _connection.StatisticsEnabled = value;
        }
        public event SqlInfoMessageEventHandler InfoMessage;

        public ConnectionState State => _connection.State;

        public IDbTransaction BeginTransaction()
        {
            return _connection.BeginTransaction();
        }

        public void Open()
        {
            _connection.Open();
        }

        public DataTable GetSchema(string table)
        {
            return _connection.GetSchema(table);
        }

        public IDbCommand CreateCommand()
        {
            return _connection.CreateCommand();
        }

        public void SetInfoMessage(SqlInfoMessageEventHandler a)
        {
            _connection.InfoMessage += a;
        }

        public void Dispose()
        {
            _connection.Dispose();
        }
    }
}