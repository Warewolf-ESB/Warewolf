using System;

namespace Dev2.Services.Sql
{
    public class ConnectionBuilder : IConnectionBuilder,IDisposable
    {
        SqlConnectionWrapper _sqlConnectionWrapper;                        

        public ISqlConnection BuildConnection(string connectionString)
        {
            GetSqlConnectionWrapper(connectionString);
            return _sqlConnectionWrapper;
        }

        static public string ConnectionString(string connectionString)
        {
            GetSqlConnectionWrapper(connectionString);
            return _sqlConnectionWrapper.ActualConnectionString;
        }

        private void GetSqlConnectionWrapper(string connectionString)
        {
            if (_sqlConnectionWrapper == null)
            {
                _sqlConnectionWrapper = new SqlConnectionWrapper(connectionString);
            }
        }

        private bool _disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _sqlConnectionWrapper.Dispose();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}