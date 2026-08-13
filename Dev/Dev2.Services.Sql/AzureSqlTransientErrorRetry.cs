using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Data.SqlClient;

namespace Dev2.Services.Sql
{
    // Retry-with-backoff for Azure SQL connection attempts that fail with a known transient
    // error - most notably a serverless database still resuming from Paused, which Azure SQL
    // rejects fast with error 40613 ("Database is not currently available") rather than the
    // client timing out. A single failed attempt does not mean the login/config is wrong; a
    // short retry usually succeeds once the database finishes resuming/scaling.
    public static class AzureSqlTransientErrorRetry
    {
        // Number of attempts (including the first) and base backoff used for the Microsoft
        // Entra Managed Identity connection attempt specifically - shared by
        // SqlConnectionWrapper.EnsureOpen and DatabaseServiceExecution.MssqlSqlExecution so
        // both give Managed Identity the same fair chance before falling back to SQL auth.
        public const int ManagedIdentityMaxAttempts = 3;
        public static readonly TimeSpan ManagedIdentityRetryBaseDelay = TimeSpan.FromSeconds(5);

        // Azure SQL Database error numbers Microsoft documents as transient/retryable - e.g.
        // serverless auto-resume or elastic-pool scaling in progress, throttling, or a
        // transient network blip - where the recommended client behaviour is to wait briefly
        // and retry rather than treat the failure as a permanent authentication/config error.
        // https://learn.microsoft.com/azure/azure-sql/database/troubleshoot-common-errors-issues
        private static readonly HashSet<int> TransientErrorNumbers = new HashSet<int>
        {
            4060,  // Cannot open database "%.*ls" requested by the login
            10928, // Resource ID: %d. The %s limit for the database is %d and has been reached
            10929, // Resource ID: %d. The %s minimum guarantee is %d, maximum limit is %d
            10053, // A transport-level error occurred (connection aborted)
            10054, // A transport-level error occurred (connection reset)
            10060, // A network-related or instance-specific error (connect timeout)
            40197, // The service has encountered an error processing your request
            40501, // The service is currently busy
            40540, // The service has encountered an error processing your request
            40613, // Database is not currently available (e.g. paused/scaling - auto-resume)
            49918, // Cannot process request. Not enough resources to process request
            49919, // Cannot process create or update request at this time
            49920, // Cannot process request. Too many operations in progress
            15197, // "There is no text for object '%s'." - not officially documented as
                   // transient by Microsoft, but observed in practice immediately after a
                   // serverless database auto-resumes from Paused: sp_helptext (used by
                   // DatabaseServiceExecution.MssqlGetSqlForProcedure to detect a FOR XML
                   // result shape) can briefly fail this way before the resumed database's
                   // system catalogs are fully warm, even though VIEW DEFINITION is granted
                   // and the procedure is neither dropped nor encrypted. A short retry
                   // clears it; a genuinely encrypted/missing/permission-denied procedure
                   // will still fail the same way after exhausting the retry budget.
        };

        public static bool IsTransientErrorNumber(int errorNumber) => TransientErrorNumbers.Contains(errorNumber);

        public static bool IsTransient(Exception ex)
        {
            return ex is SqlException sqlEx && sqlEx.Errors.Cast<SqlError>().Any(e => IsTransientErrorNumber(e.Number));
        }

        // Invokes openConnection, retrying up to maxAttempts times (exponential backoff:
        // baseDelay * 2^(attempt-1)) while isTransient(ex) holds. Rethrows immediately on a
        // non-transient failure (a real auth/config error should fail fast, not be retried),
        // or after the final attempt. delay defaults to Thread.Sleep; tests can inject a fake
        // to observe backoff values without actually waiting.
        public static void Retry(
            Action openConnection,
            Func<Exception, bool> isTransient,
            int maxAttempts,
            TimeSpan baseDelay,
            Action<int, int, Exception> onRetry = null,
            Action<TimeSpan> delay = null)
        {
            if (openConnection is null)
            {
                throw new ArgumentNullException(nameof(openConnection));
            }
            if (isTransient is null)
            {
                throw new ArgumentNullException(nameof(isTransient));
            }

            var sleep = delay ?? Thread.Sleep;

            for (var attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    openConnection?.Invoke();
                    return;
                }
                catch (Exception ex) when (attempt < maxAttempts && isTransient?.Invoke(ex) == true)
                {
                    onRetry?.Invoke(attempt, maxAttempts, ex);
                    var backoff = TimeSpan.FromMilliseconds(baseDelay.TotalMilliseconds * Math.Pow(2, attempt - 1));
                    sleep(backoff);
                }
            }
        }
    }
}
