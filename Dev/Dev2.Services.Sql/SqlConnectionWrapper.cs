#pragma warning disable
using System;
using System.Data;
using Dev2.Common;
using Microsoft.Data.SqlClient;

namespace Dev2.Services.Sql
{
    public class SqlConnectionWrapper : ISqlConnection
    {
        private string _actualConnectionString;

        // Microsoft Entra Managed Identity fallback: Microsoft.Data.SqlClient rejects
        // 'Authentication=Active Directory Managed Identity' when User ID/Password are also present
        // (they cannot be combined in a single connection attempt), so when both are supplied on the
        // source we precompute a plain SQL-auth connection string (Authentication removed, credentials
        // retained) here. EnsureOpen tries Managed Identity first and retries once with this fallback
        // if that attempt fails. Null when no fallback is available (Managed Identity only, or non-Entra
        // connection string) — in which case a failed open is simply propagated as before.
        private string _entraFallbackConnectionString;
        SqlConnection _connection;

        private const int DefaultConnectTimeoutSeconds = 30;
        private const int DefaultEntraManagedIdentityConnectTimeoutSeconds = 60;

        public SqlConnectionWrapper()
        {
        }

        public string CreateConnectionString(string connString)
        {
            var conStrBuilder = new SqlConnectionStringBuilder(connString)
            {
                ConnectTimeout = DefaultConnectTimeoutSeconds,
                MaxPoolSize = 100,
                Pooling = true,
                ApplicationName = "Warewolf Service"
            };
            // In non-DPAPI / cloud hosts (e.g. the lightweight Azure Function) the target SQL Server
            // may present a self-signed or otherwise untrusted certificate. Microsoft.Data.SqlClient
            // defaults to Encrypt=true with full certificate-chain validation, which rejects such certs
            // ("The certificate chain was issued by an authority that is not trusted."). Opt in to
            // trusting the server certificate only when WAREWOLF_SQL_TRUST_SERVER_CERT is set, so
            // production keeps strict validation by default.
            if (bool.TryParse(Environment.GetEnvironmentVariable("WAREWOLF_SQL_TRUST_SERVER_CERT"), out var trustServerCertificate) && trustServerCertificate)
            {
                conStrBuilder.TrustServerCertificate = true;
            }

            // Microsoft Entra Managed Identity connections need more headroom than the default
            // 30s ConnectTimeout: acquiring the Managed Identity token (IMDS/Entra ID) and, for
            // serverless Azure SQL databases, waiting for auto-resume from a paused state can
            // together take well over 30 seconds - even though a subsequent plain SQL-auth
            // fallback attempt (started once the database is already resuming) then succeeds
            // quickly, masking the fact that Managed Identity never got a fair chance to
            // connect. Use a longer fixed timeout for Managed Identity connections only.
            if (conStrBuilder.Authentication == SqlAuthenticationMethod.ActiveDirectoryManagedIdentity)
            {
                conStrBuilder.ConnectTimeout = DefaultEntraManagedIdentityConnectTimeoutSeconds;
            }

            _entraFallbackConnectionString = null;
            if (conStrBuilder.Authentication == SqlAuthenticationMethod.ActiveDirectoryManagedIdentity
                && (!string.IsNullOrEmpty(conStrBuilder.UserID) || !string.IsNullOrEmpty(conStrBuilder.Password)))
            {
                var fallbackBuilder = new SqlConnectionStringBuilder(conStrBuilder.ConnectionString)
                {
                    Authentication = SqlAuthenticationMethod.NotSpecified
                };
                _entraFallbackConnectionString = fallbackBuilder.ConnectionString;

                // Managed Identity does not accept explicit credentials on the primary attempt.
                conStrBuilder.Remove("User ID");
                conStrBuilder.Remove("Password");
            }

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

        // Exposed so callers that build their own SqlConnection directly (bypassing EnsureOpen's
        // built-in retry, e.g. DatabaseServiceExecution.MssqlSqlExecution) can implement the same
        // Managed Identity -> SQL-auth fallback themselves. Null when no fallback is available.
        public string EntraFallbackConnectionString => _entraFallbackConnectionString;

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
            if (_connection.State == ConnectionState.Open)
            {
                return;
            }

            if (string.IsNullOrEmpty(_entraFallbackConnectionString))
            {
                _connection.Open();
                return;
            }

            try
            {
                // Give Managed Identity a fair chance against transient failures (most notably
                // a serverless Azure SQL database still resuming from Paused, which Azure SQL
                // rejects fast with error 40613 rather than the client timing out) before
                // falling back to the SQL-auth credentials.
                AzureSqlTransientErrorRetry.Retry(
                    () => _connection.Open(),
                    AzureSqlTransientErrorRetry.IsTransient,
                    AzureSqlTransientErrorRetry.ManagedIdentityMaxAttempts,
                    AzureSqlTransientErrorRetry.ManagedIdentityRetryBaseDelay,
                    (attempt, maxAttempts, ex) => Dev2Logger.Warn(
                        $"SQL Server: Microsoft Entra Managed Identity authentication attempt {attempt}/{maxAttempts} hit a transient error ({ex.Message}). Retrying...",
                        GlobalConstants.WarewolfWarn));
            }
            catch (Exception ex)
            {
                Dev2Logger.Warn(
                    $"SQL Server: Microsoft Entra Managed Identity authentication failed ({ex.Message}). Falling back to SQL Server username/password authentication.",
                    GlobalConstants.WarewolfWarn);

                _connection.Dispose();
                _actualConnectionString = _entraFallbackConnectionString;
                // Only ever attempt the fallback once per connection.
                _entraFallbackConnectionString = null;
                CreateConnection();
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