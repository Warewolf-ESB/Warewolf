using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Warewolf.Execution.Lightweight.Logging
{
    /// <summary>
    /// Configuration for the Elasticsearch execution logger.
    ///
    /// Two factory methods are provided:
    /// <list type="bullet">
    ///   <item>
    ///     <see cref="FromBiteFile"/> — reads a Warewolf <c>ElasticsearchSource.bite</c>
    ///     file whose <c>ConnectionString</c> contains
    ///     <c>HostName;Port;SearchIndex;AuthenticationType[;Username;Password]</c>.
    ///   </item>
    ///   <item>
    ///     <see cref="FromEnvironment"/> — legacy fallback via <c>Elasticsearch__*</c>
    ///     environment variables.
    ///   </item>
    /// </list>
    /// </summary>
    public sealed class ElasticsearchLoggingOptions
    {
        /// <summary>Full Elasticsearch node URI, e.g. <c>http://localhost:9200</c>.</summary>
        public string? Uri { get; set; }

        /// <summary>Target index name. Defaults to <c>warewolf-execution-logs</c>.</summary>
        public string IndexName { get; set; } = "warewolf-execution-logs";

        /// <summary>Optional basic-auth username (<c>AuthenticationType=Password</c>).</summary>
        public string? Username { get; set; }

        /// <summary>Optional basic-auth password (<c>AuthenticationType=Password</c>).</summary>
        public string? Password { get; set; }

        /// <summary>
        /// Optional API key, base-64 encoded <c>id:api_key</c>
        /// (<c>AuthenticationType=API_Key</c> — stored in the <c>Password</c> field of the
        /// connection string).
        /// </summary>
        public string? ApiKey { get; set; }

        /// <summary>
        /// <c>true</c> when a URI has been resolved, meaning the logger is ready to use.
        /// </summary>
        public bool IsConfigured => !string.IsNullOrWhiteSpace(Uri);

        // -------------------------------------------------------------------------
        // Factory — .bite file
        // -------------------------------------------------------------------------

        /// <summary>
        /// Parses a Warewolf <c>ElasticsearchSource.bite</c> XML file and returns the
        /// corresponding options.
        /// </summary>
        /// <remarks>
        /// The <c>ConnectionString</c> attribute uses semicolon-separated key=value pairs:
        /// <code>HostName=http://localhost;Port=9200;SearchIndex=warewolftestlogs;AuthenticationType=Password;Username=u;Password=p</code>
        /// <c>AuthenticationType</c> values: <c>Anonymous</c>, <c>Password</c>, <c>API_Key</c>.
        /// For <c>API_Key</c> the key itself is stored in the <c>Password</c> field.
        /// </remarks>
        /// <param name="filePath">Absolute path to the <c>.bite</c> file.</param>
        public static ElasticsearchLoggingOptions FromBiteFile(string filePath)
        {
            var xml = XElement.Load(filePath);
            var rawCs = xml.Attribute("ConnectionString")?.Value ?? string.Empty;

            // Parse semicolon-delimited key=value pairs (split on first '=' only).
            var props = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var part in rawCs.Split(';', StringSplitOptions.RemoveEmptyEntries))
            {
                var eq = part.IndexOf('=');
                if (eq > 0)
                    props[part[..eq].Trim()] = part[(eq + 1)..].Trim();
            }

            string Get(string key, string @default = "") =>
                props.TryGetValue(key, out var v) ? v : @default;

            var host     = Get("HostName", "http://localhost");
            var port     = Get("Port",     "9200");
            var index    = Get("SearchIndex", "warewolf-execution-logs");
            var authType = Get("AuthenticationType", "Anonymous");
            var username = Get("Username");
            var password = Get("Password");

            var options = new ElasticsearchLoggingOptions
            {
                Uri       = $"{host}:{port}",
                IndexName = index,
            };

            if (string.Equals(authType, "Password", StringComparison.OrdinalIgnoreCase))
            {
                options.Username = username;
                options.Password = password;
            }
            else if (string.Equals(authType, "API_Key", StringComparison.OrdinalIgnoreCase))
            {
                // Warewolf stores the API key in the Password field.
                options.ApiKey = password;
            }

            return options;
        }

        // -------------------------------------------------------------------------
        // Factory — environment variables (legacy / fallback)
        // -------------------------------------------------------------------------

        /// <summary>Reads config from the <c>Elasticsearch__*</c> environment variables.</summary>
        public static ElasticsearchLoggingOptions FromEnvironment() => new()
        {
            Uri       = Environment.GetEnvironmentVariable("Elasticsearch__Uri"),
            IndexName = Environment.GetEnvironmentVariable("Elasticsearch__IndexName")
                        ?? "warewolf-execution-logs",
            Username  = Environment.GetEnvironmentVariable("Elasticsearch__Username"),
            Password  = Environment.GetEnvironmentVariable("Elasticsearch__Password"),
            ApiKey    = Environment.GetEnvironmentVariable("Elasticsearch__ApiKey"),
        };
    }
}
