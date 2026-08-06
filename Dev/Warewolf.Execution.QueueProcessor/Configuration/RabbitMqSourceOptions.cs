/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*/

using System.Xml.Linq;
using Warewolf.Security.Encryption;

namespace Warewolf.Execution.QueueProcessor.Configuration
{
    /// <summary>
    /// Broker connection settings, read from a Warewolf <c>RabbitMQSource</c> <c>.bite</c>
    /// exactly the way the engine already reads its Elasticsearch source
    /// (<c>ElasticsearchLoggingOptions.FromBiteFile</c>): load the XML, take the
    /// <c>ConnectionString</c> attribute, decrypt it if encrypted, then split on <c>;</c> and
    /// the first <c>=</c>.
    ///
    /// <para>The five fields are exactly those <c>PublishRabbitMQActivity</c> puts on its
    /// <c>ConnectionFactory</c> (<c>PublishRabbitMQActivity.cs:188-192</c>), which is the
    /// parity requirement from decision #3/#5:
    /// <c>HostName;Port;UserName;Password;VirtualHost</c>.</para>
    ///
    /// <para><b>TLS.</b> Neither the activity nor the source format carries a TLS flag, so
    /// TLS is opt-in and defaults OFF for byte-parity (decision #25). It is enabled by a
    /// <c>UseSsl=true</c> token in the connection string, by <c>Port == 5671</c>, or by the
    /// <c>RABBITMQ__USESSL</c> environment override. Production requires it — see the
    /// go-live gate in the migration plan.</para>
    /// </summary>
    public sealed class RabbitMqSourceOptions
    {
        const int DefaultPort = 5672;
        const int DefaultTlsPort = 5671;
        const string DefaultVirtualHost = "/";

        public string HostName { get; init; } = string.Empty;
        public int Port { get; init; } = DefaultPort;
        public string UserName { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;
        public string VirtualHost { get; init; } = DefaultVirtualHost;
        public bool UseSsl { get; init; }

        /// <summary>Source resource id, for logging and to correlate with the trigger.</summary>
        public Guid SourceId { get; init; }
        public string? SourceName { get; init; }

        /// <summary>Safe for logs: never contains the password.</summary>
        public string Describe() =>
            $"{(UseSsl ? "amqps" : "amqp")}://{UserName}@{HostName}:{Port}{VirtualHost}";

        /// <summary>
        /// Parses a <c>{RabbitMQSource}.bite</c> file.
        /// </summary>
        /// <param name="filePath">Absolute path to the source <c>.bite</c>.</param>
        /// <param name="forceSsl">
        /// <c>true</c> forces TLS on regardless of the connection string (the
        /// <c>RABBITMQ__USESSL</c> override).
        /// </param>
        public static RabbitMqSourceOptions FromBiteFile(string filePath, bool? forceSsl = null)
        {
            if (!File.Exists(filePath))
            {
                throw new TriggerConfigurationException(
                    $"RabbitMQ source file not found: '{filePath}'. The trigger's QueueSourceId must " +
                    "resolve to a staged '{QueueSourceId}.bite' in the container's Settings folder.");
            }

            XElement xml;
            try
            {
                xml = XElement.Load(filePath);
            }
            catch (Exception ex)
            {
                throw new TriggerConfigurationException(
                    $"RabbitMQ source file '{filePath}' is not valid XML: {ex.Message}", ex);
            }

            var rawCs = xml.Attribute("ConnectionString")?.Value ?? string.Empty;
            if (string.IsNullOrWhiteSpace(rawCs))
            {
                throw new TriggerConfigurationException(
                    $"RabbitMQ source file '{filePath}' has no ConnectionString attribute.");
            }

            var connectionString = Decrypt(rawCs, filePath);
            var props = Parse(connectionString);

            string Get(string key, string @default = "") =>
                props.TryGetValue(key, out var v) ? v : @default;

            var host = Get("HostName");
            if (string.IsNullOrWhiteSpace(host))
            {
                throw new TriggerConfigurationException(
                    $"RabbitMQ source file '{filePath}' yielded no HostName. If the source was saved " +
                    "on-prem its ConnectionString may be DPAPI-encrypted, which cannot be read in a " +
                    "Linux container - re-stage it as WFAES with Deploy-WwQueueProcessor.ps1.");
            }

            var port = int.TryParse(Get("Port"), out var p) ? p : DefaultPort;
            var vhost = Get("VirtualHost");

            // Parity default: OFF. Enabled by an explicit token, the TLS port, or the override.
            var useSsl = forceSsl
                         ?? (ParseBool(Get("UseSsl")) || ParseBool(Get("Ssl")) || port == DefaultTlsPort);

            return new RabbitMqSourceOptions
            {
                HostName = host,
                Port = port,
                UserName = Get("UserName"),
                Password = Get("Password"),
                VirtualHost = string.IsNullOrWhiteSpace(vhost) ? DefaultVirtualHost : vhost,
                UseSsl = useSsl,
                SourceId = Guid.TryParse(xml.Attribute("ID")?.Value, out var id) ? id : Guid.Empty,
                SourceName = xml.Attribute("Name")?.Value,
            };
        }

        static string Decrypt(string rawCs, string filePath)
        {
            // Mirrors ElasticsearchLoggingOptions.FromBiteFile: CanBeDecrypted() routes WFAES::
            // through the Key Vault hook and returns false for plaintext. On Linux a DPAPI blob
            // also returns false (ProtectedData throws and is swallowed), so the parse below
            // yields no HostName and the caller raises the actionable error.
            if (!rawCs.CanBeDecrypted())
            {
                return rawCs;
            }

            try
            {
                return DpapiWrapper.Decrypt(rawCs);
            }
            catch (Exception ex)
            {
                throw new TriggerConfigurationException(
                    $"RabbitMQ source '{filePath}' has an encrypted ConnectionString that could not be " +
                    "decrypted. Verify Key Vault access and that the AES key has not been rotated.", ex);
            }
        }

        static Dictionary<string, string> Parse(string connectionString)
        {
            var props = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var part in connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries))
            {
                var eq = part.IndexOf('=');
                if (eq > 0)
                {
                    props[part[..eq].Trim()] = part[(eq + 1)..].Trim();
                }
            }
            return props;
        }

        static bool ParseBool(string value)
            => bool.TryParse(value, out var b) && b;
    }
}
