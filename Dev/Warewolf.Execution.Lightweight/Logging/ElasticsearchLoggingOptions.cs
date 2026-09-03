using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Warewolf.Security.Encryption;

namespace Warewolf.Execution.Lightweight.Logging
{
    /// <summary>
    /// Configuration for the Elasticsearch execution logger, read via <see cref="FromBiteFile"/>
    /// from a Warewolf <c>ElasticsearchSource.bite</c> file whose <c>ConnectionString</c> contains
    /// <c>HostName;Port;SearchIndex;AuthenticationType[;Username;Password]</c>.
    ///
    /// WOLF-8516: the legacy <c>Elasticsearch__*</c> environment-variable fallback
    /// (<c>FromEnvironment</c>) was removed — confirmed dead: <c>ServiceCollectionExtensions</c>
    /// only ever called <see cref="FromBiteFile"/>, and no test exercised the env-var path.
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
        /// (<c>AuthenticationType=API_Key</c> � stored in the <c>Password</c> field of the
        /// connection string).
        /// </summary>
        public string? ApiKey { get; set; }

        /// <summary>
        /// <c>true</c> when a URI has been resolved, meaning the logger is ready to use.
        /// </summary>
        public bool IsConfigured => !string.IsNullOrWhiteSpace(Uri);

        /// <summary>
        /// When <c>true</c>, enables Elasticsearch HTTP debug mode which captures full
        /// request/response bodies. <b>Never enable in production</b> � causes large
        /// memory allocations per index call. Default: <c>false</c>.
        /// Set via <c>WAREWOLF_LOGGING_CONFIG</c>'s <c>elasticDebugMode</c> field (WOLF-8516;
        /// formerly the standalone <c>ELASTIC_DEBUG_MODE</c> env var; development only).
        /// </summary>
        public bool EnableDebugMode { get; set; }

        // -------------------------------------------------------------------------
        // Factory � .bite file
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

            // Decrypt if the connection string is AES-encrypted (WFAES:: prefix)
            // or DPAPI-encrypted (base64). The DpapiWrapper.AesDecryptHook must
            // already be wired by KeyVaultStartupExtensions.InitializeKeyVaultAsync
            // before this method is called.
            if (!string.IsNullOrEmpty(rawCs) && rawCs.CanBeDecrypted())
            {
                rawCs = DpapiWrapper.Decrypt(rawCs);
            }

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
    }
}
