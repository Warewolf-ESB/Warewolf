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

        public string ConnectionString(string connectionString)
        {
            GetSqlConnectionWrapper(connectionString);
            return _sqlConnectionWrapper.ActualConnectionString;
        }

        // Microsoft Entra Managed Identity fallback connection string (plain SQL-auth, credentials
        // retained), for callers that open their own SqlConnection directly instead of going through
        // BuildConnection/ISqlConnection.EnsureOpen. Null when no fallback is available.
        public string FallbackConnectionString(string connectionString)
        {
            GetSqlConnectionWrapper(connectionString);
            return _sqlConnectionWrapper.EntraFallbackConnectionString;
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