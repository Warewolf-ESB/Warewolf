using System;

namespace Warewolf.Execution.Lightweight.Infrastructure
{
    /// <summary>
    /// Disables Microsoft.Data.SqlClient's SQL Server alias-name resolution, which reads the
    /// Windows registry (TdsParserStaticMethods.AliasRegistryLookup -> ADP.LocalMachineRegistryValue)
    /// and throws a NullReferenceException on Linux — confirmed live (build 30775, 2026-09-22) the
    /// first time a SQL-backed workflow ran on the Flex Consumption (Linux-only) engine: every
    /// activity that opened a SqlConnection failed before it could do anything else. Matches the
    /// same registry-based alias lookup reported in microsoft/azuredatastudio#26127 for .NET 8 on
    /// Linux, and Microsoft's own documented fix is exactly this switch (see
    /// https://learn.microsoft.com/en-us/sql/connect/ado-net/appcontext-switches).
    ///
    /// Safe on Windows too: nothing in this codebase's connection strings configures a SQL Server
    /// client alias (cliconfg.exe), so disabling alias resolution changes no observable behaviour
    /// there — it only removes a lookup that would otherwise always miss.
    /// </summary>
    public static class SqlClientCompatibilityConfigurator
    {
        internal const string DisableUseOfAliasSwitch = "Switch.Microsoft.Data.SqlClient.DisableUseOfAlias";

        /// <summary>
        /// Sets the AppContext switch. Must run before the first <c>SqlConnection.Open()</c> call
        /// in this process — SqlClient reads the switch once and caches it internally.
        /// </summary>
        public static void Configure()
        {
            AppContext.SetSwitch(DisableUseOfAliasSwitch, true);
        }
    }
}
