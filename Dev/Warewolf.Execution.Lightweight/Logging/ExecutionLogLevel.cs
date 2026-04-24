using Dev2.Data.Interfaces.Enums;
using System;

namespace Warewolf.Execution.Lightweight.Logging
{
    /// <summary>
    /// Reads the <c>ExecutionLogLevel</c> environment variable and exposes it as a
    /// <see cref="LogLevel"/> value that can be passed to every <see cref="IExecutionLogger"/>
    /// implementation.
    ///
    /// Accepted values (case-insensitive string or numeric):
    /// <list type="table">
    ///   <listheader><term>Number</term><term>Name</term><term>Meaning</term></listheader>
    ///   <item><term>0</term><term>OFF</term>   <term>No logging</term></item>
    ///   <item><term>1</term><term>FATAL</term> <term>Fatal events only</term></item>
    ///   <item><term>2</term><term>ERROR</term> <term>Error and above</term></item>
    ///   <item><term>3</term><term>WARN</term>  <term>Warning and above</term></item>
    ///   <item><term>4</term><term>INFO</term>  <term>Info and above (default)</term></item>
    ///   <item><term>5</term><term>DEBUG</term> <term>Debug and above</term></item>
    ///   <item><term>6</term><term>TRACE</term> <term>Everything</term></item>
    /// </list>
    ///
    /// Defaults to <see cref="LogLevel.INFO"/> when absent or unrecognised.
    /// </summary>
    public static class ExecutionLogLevel
    {
        /// <summary>Fallback when the env var is absent or contains an unrecognised value.</summary>
        public const LogLevel Default = LogLevel.INFO;

        /// <summary>
        /// Reads <c>ExecutionLogLevel</c> from <see cref="Environment.GetEnvironmentVariable"/>.
        /// </summary>
        public static LogLevel Read() => Parse(Environment.GetEnvironmentVariable("EXECUTIONLOGLEVEL"));

        /// <summary>
        /// Converts a raw string to a <see cref="LogLevel"/>.
        /// Accepts both numeric values (<c>"4"</c>) and names (<c>"INFO"</c>).
        /// Returns <see cref="Default"/> when the value is null, empty, or unrecognised.
        /// </summary>
        public static LogLevel Parse(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return Default;

            // Numeric: "0" – "6"
            if (int.TryParse(raw, out var num) && Enum.IsDefined(typeof(LogLevel), num))
                return (LogLevel)num;

            // Named: "INFO", "Debug", etc.
            if (Enum.TryParse<LogLevel>(raw, ignoreCase: true, out var named))
                return named;

            return Default;
        }

        /// <summary>
        /// Returns <c>true</c> when a log entry at <paramref name="messageLevel"/> should be
        /// emitted given <paramref name="minimumLevel"/>.
        ///
        /// Uses the Dev2 convention: higher numeric value = more verbose.
        /// The entry is emitted when <c>minimumLevel &gt;= messageLevel</c>.
        /// </summary>
        public static bool ShouldLog(LogLevel messageLevel, LogLevel minimumLevel) =>
            minimumLevel != LogLevel.OFF && minimumLevel >= messageLevel;
    }
}

