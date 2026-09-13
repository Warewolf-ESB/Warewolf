/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using Dev2.Common.Common;
using ModelContextProtocol;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Warewolf.Execution.Lightweight.Auth;
using Warewolf.Execution.Lightweight.Auth.Models;
using Warewolf.Execution.Lightweight.Infrastructure;
using Warewolf.Execution.Lightweight.Mcp.Secrets;
using Warewolf.Security.Encryption;

namespace Warewolf.Execution.Lightweight.Mcp.ToolHandlers;

/// <summary>
/// Implements the <c>add_source</c> MCP tool: creates a new connection source (database,
/// cache, email, or message-broker) as a <c>.bite</c> file, provided the caller has Contribute
/// permission and <c>name</c> does not already exist — the source counterpart of
/// <see cref="CreateWorkflowTool"/>.
///
/// <para>
/// <b>Supported source types</b> — see <see cref="SourceCatalog"/> for the exact field set
/// per <c>sourceType</c>. Covers every <see cref="ToolCatalog"/> entry whose
/// <c>RequiresSource</c> flag is <c>true</c>.
/// </para>
///
/// <para>
/// <b>Secrets never travel as literal JSON.</b> Any string field in <c>config</c> may contain one
/// or more <c>${NAME}</c> placeholders, resolved via <see cref="IMcpSecretResolver"/> at save time
/// — never from a value the caller supplies. This lets a calling agent build the request JSON
/// referencing e.g. <c>"${db-password}"</c> for a password field instead of ever having to embed,
/// log, or transmit the real secret value. In production <c>NAME</c> is a secret already staged in
/// Azure Key Vault (see <see cref="Secrets.KeyVaultMcpSecretResolver"/>); resolving it purely
/// against this server's own environment variables (client-side resolution before the request is
/// sent doesn't help either — the real secret would still cross the wire in the JSON) only happens
/// as a local-development fallback when Key Vault isn't configured (see
/// <see cref="Secrets.EnvironmentMcpSecretResolver"/>). An unresolvable placeholder is a hard error
/// (fails closed rather than silently persisting the literal <c>${...}</c> text). The response's
/// <see cref="AddSourceResult.ResolvedSecretFields"/> reports which field <i>names</i> (never
/// values) were substituted, so a caller can confirm the intended fields were sourced from a
/// secret reference without the response leaking anything sensitive.
/// </para>
///
/// <para>
/// <b>Encryption.</b> The built connection string is encrypted via
/// <see cref="Warewolf.Security.Encryption.DpapiWrapper.Encrypt"/> — exactly the call
/// <c>DbSource</c>/<c>RedisSource</c>/<c>EmailSource</c>/<c>RabbitMQSource</c>'s own
/// <c>ToXml()</c> methods already make. That method transparently produces Key Vault-backed
/// <c>WFAES::</c> AES-256-GCM ciphertext when <see cref="Security.FileEncryptionHelper"/> is
/// registered as <see cref="Warewolf.Security.Encryption.DpapiWrapper.AesEncryptHook"/> (this
/// host's Azure/Key-Vault configuration), or falls back to Windows DPAPI otherwise — this tool
/// does not need to know or choose which, keeping its output decryptable by the exact same
/// runtime path a Studio-saved source already uses (see
/// <c>docs/KB-Deploying-Encrypted-Sources.md</c>).
/// </para>
///
/// <para>
/// <b>Deliberately does not reuse <c>Dev2.Runtime.Services</c>/<c>Dev2.Data</c>'s source
/// classes</b> for XML construction — same "Lightweight-local, no heavier dependency" choice
/// <see cref="EnvelopeBiteWriter"/> already made for workflows. The connection-string shape and
/// <c>&lt;Source&gt;</c> XML shape are reproduced field-for-field from those classes'
/// <c>ToXml()</c> methods so a source this tool creates is byte-for-byte parseable by the same
/// runtime loader as one saved from Studio.
/// </para>
///
/// <para>
/// <b>Shared with <see cref="EditSourceTool"/>.</b> Config parsing/secret resolution
/// (<see cref="ResolveConfigAsync"/>), cross-field validation (<see cref="ApplyConditionalRequirements"/>),
/// connection-string building (<see cref="BuildConnectionString"/>), and <c>.bite</c> XML
/// construction (<see cref="BuildSourceXml"/>) are exposed <c>internal</c> so <c>edit_source</c>
/// can reuse them verbatim rather than duplicating this tool's field-for-field logic.
/// </para>
/// </summary>
internal static class AddSourceTool
{
    internal const string ToolName = "add_source";

    internal static readonly Regex SecretPlaceholder = new(@"\$\{([A-Za-z][A-Za-z0-9_-]*)\}", RegexOptions.Compiled);

    internal static async Task<AddSourceResult> Handle(
        HostEnvironmentConfig hostConfig,
        IWorkflowAuthPolicyLoader authPolicyLoader,
        IMcpSecretResolver secretResolver,
        ClaimsPrincipal? user,
        [Description("The new source's name — a relative path (forward slashes), no extension. Must not already exist.")]
        string name,
        [Description("The kind of source to create. One of: SqlDatabase, MySqlDatabase, PostgreSQL, Oracle, ODBC, Redis, Email, RabbitMQ, Web.")]
        string sourceType,
        [Description("The source's connection fields as a flat JSON object of name/value pairs — the accepted field names, types, defaults and " +
            "required-ness depend on sourceType (e.g. SqlDatabase: Server*, DatabaseName*, Port, AuthenticationType [Windows|User], UserID, Password, " +
            "ConnectionTimeout, TrustServerCertificate — * = required; Redis: HostName*, Port, AuthenticationType [Anonymous|Password], Password; " +
            "Email: Host*, Port, UserName, Password, EnableSsl, Timeout; RabbitMQ: HostName*, Port, UserName, Password, VirtualHost; " +
            "Web: Address*, DefaultQuery, AuthenticationType [Anonymous|User], UserName, Password [required when AuthenticationType is User]). " +
            "IMPORTANT — for any password/secret-shaped field, do not put the literal secret in this JSON. Instead set its value to " +
            "\"${secret-name}\", where secret-name is a secret already staged in this server's Key Vault (e.g. via `az keyvault secret set`) " +
            "ahead of time — the server fetches the real value itself when saving and never echoes it back. (Only on a Key-Vault-less local " +
            "dev host does \"${NAME}\" instead fall back to that host's own environment variable NAME.)")]
        JsonElement config,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new McpException("`name` is required.");
        }

        if (string.IsNullOrWhiteSpace(sourceType))
        {
            throw new McpException("`sourceType` is required.");
        }

        var entry = SourceCatalog.Resolve(sourceType)
            ?? throw new McpException($"`sourceType` '{sourceType}' is not supported. Supported values: {SourceCatalog.SupportedTypesList}.");

        var principal = user as WorkflowClaimsPrincipal;
        var workflowsDirectory = hostConfig.WorkflowsDirectory;
        var relativePath = name.Replace('\\', '/').TrimStart('/');

        if (WorkflowNameResolver.Resolve(workflowsDirectory, relativePath) is not null)
        {
            throw new McpException($"A resource named '{name}' already exists.");
        }

        if (!ListWorkflowsTool.HasPermission(authPolicyLoader, principal, relativePath, WorkflowPermission.Contribute))
        {
            throw new McpException($"You do not have permission to create source '{name}'.");
        }

        var (values, resolvedSecretFields) = await ResolveConfigAsync(entry, config, secretResolver, cancellationToken).ConfigureAwait(false);
        ApplyConditionalRequirements(entry, values);

        var connectionString = BuildConnectionString(entry, values);
        var encryptedConnectionString = DpapiWrapper.Encrypt(connectionString);

        var resourceId = Guid.NewGuid().ToString();
        var displayName = Path.GetFileName(relativePath.TrimEnd('/'));
        var callerIdentity = principal?.CallerIdentity is { Length: > 0 } identity ? identity : "Anonymous";

        string xmlContents;
        try
        {
            xmlContents = BuildSourceXml(entry, resourceId, displayName, encryptedConnectionString, versionNumber: 1, timestampUtc: DateTimeOffset.UtcNow, user: callerIdentity);
        }
        catch (Exception ex)
        {
            throw new McpException($"the source could not be composed into a .bite file: {ex.Message}");
        }

        var fullPath = Path.Combine(workflowsDirectory, relativePath.Replace('/', Path.DirectorySeparatorChar) + ".bite");
        var directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(fullPath, xmlContents);

        // Without this, LightweightSourceLoader.EnsureSourceLoaded can never find this source on
        // an instance whose directory index was already built (e.g. by an earlier request) before
        // this file existed — its per-directory index is a Lazy built at most once, with no
        // staleness check of its own. See LightweightSourceLoader.InvalidateDirectory's XML doc.
        LightweightSourceLoader.Instance.InvalidateDirectory(workflowsDirectory);

        // Keeps name-based resolution (WorkflowNameResolver) as fast as a freshly-created
        // workflow's, mirroring EditSourceTool's own equivalent call.
        WorkflowIndex.Instance.AddOrUpdate(workflowsDirectory, relativePath, relativePath + ".bite");

        return new AddSourceResult(name, entry.SourceType, true, resolvedSecretFields);
    }

    // ── config parsing / secret-reference substitution ────────────────────────

    internal static async Task<(Dictionary<string, string?> Values, IReadOnlyList<string> ResolvedSecretFields)> ResolveConfigAsync(
        SourceCatalog.Entry entry, JsonElement config, IMcpSecretResolver secretResolver, CancellationToken cancellationToken)
    {
        if (config.ValueKind != JsonValueKind.Object)
        {
            throw new McpException("`config` must be a JSON object.");
        }

        var knownFieldNames = new HashSet<string>(entry.Fields.Select(f => f.Name), StringComparer.Ordinal);
        var unknown = config.EnumerateObject().Select(p => p.Name).Where(n => !knownFieldNames.Contains(n)).ToList();
        if (unknown.Count > 0)
        {
            throw new McpException(
                $"`config` has unrecognised field(s) for sourceType '{entry.SourceType}': {string.Join(", ", unknown)}. " +
                $"Supported fields: {string.Join(", ", knownFieldNames)}.");
        }

        var values = new Dictionary<string, string?>(StringComparer.Ordinal);
        var resolvedSecretFields = new List<string>();

        foreach (var field in entry.Fields)
        {
            string? raw;
            if (config.TryGetProperty(field.Name, out var element) && element.ValueKind != JsonValueKind.Null)
            {
                raw = RawStringOf(element, field.Name);
            }
            else
            {
                raw = field.Default;
            }

            if (raw is not null)
            {
                raw = await ResolveSecretPlaceholdersAsync(raw, field.Name, secretResolver, resolvedSecretFields, cancellationToken).ConfigureAwait(false);
            }

            if (field.Required && string.IsNullOrEmpty(raw))
            {
                throw new McpException($"`config.{field.Name}` is required for sourceType '{entry.SourceType}'.");
            }

            values[field.Name] = raw;
        }

        return (values, resolvedSecretFields);
    }

    static string RawStringOf(JsonElement element, string fieldName) =>
        element.ValueKind switch
        {
            JsonValueKind.String => element.GetString() ?? string.Empty,
            JsonValueKind.Number => element.GetRawText(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            _ => throw new McpException($"`config.{fieldName}` must be a string, number, or boolean."),
        };

    /// <summary>
    /// Replaces every <c>${NAME}</c> occurrence in <paramref name="raw"/> with
    /// <c>await secretResolver.ResolveAsync(NAME, ...)</c> — a Key Vault secret in production, or
    /// (dev-only fallback) an environment variable on this host when Key Vault isn't configured.
    /// Never resolved from anything the caller supplies. Throws when a referenced name cannot be
    /// resolved, rather than silently persisting the literal placeholder text into the saved
    /// source. <c>internal</c> so other secret-shaped-field tools (e.g. <see cref="SetLicenseTool"/>)
    /// share this exact resolution logic instead of re-implementing it.
    /// </summary>
    internal static async Task<string> ResolveSecretPlaceholdersAsync(
        string raw, string fieldName, IMcpSecretResolver secretResolver, ICollection<string> resolvedSecretFields, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(raw) || !raw.Contains("${", StringComparison.Ordinal))
        {
            return raw;
        }

        var substituted = false;
        var matches = SecretPlaceholder.Matches(raw);
        var result = raw;

        // Replace back-to-front so earlier match indices/lengths in `raw` stay valid as we edit `result`.
        for (var i = matches.Count - 1; i >= 0; i--)
        {
            var match = matches[i];
            var secretName = match.Groups[1].Value;

            string value;
            try
            {
                value = await secretResolver.ResolveAsync(secretName, cancellationToken).ConfigureAwait(false);
            }
            catch (McpException ex)
            {
                throw new McpException($"`config.{fieldName}` references \"${{{secretName}}}\": {ex.Message}");
            }

            result = result.Remove(match.Index, match.Length).Insert(match.Index, value);
            substituted = true;
        }

        if (substituted)
        {
            resolvedSecretFields.Add(fieldName);
        }

        return result;
    }

    // ── cross-field conditional requirements (not expressible in SourceCatalog's flat field list) ──

    internal static void ApplyConditionalRequirements(SourceCatalog.Entry entry, IReadOnlyDictionary<string, string?> values)
    {
        switch (entry.SourceType)
        {
            case "SqlDatabase":
                if (string.Equals(values.GetValueOrDefault("AuthenticationType"), "User", StringComparison.OrdinalIgnoreCase) &&
                    string.IsNullOrEmpty(values.GetValueOrDefault("Password")))
                {
                    throw new McpException("`config.Password` is required when `config.AuthenticationType` is \"User\".");
                }

                break;

            case "Redis":
                if (string.Equals(values.GetValueOrDefault("AuthenticationType"), "Password", StringComparison.OrdinalIgnoreCase) &&
                    string.IsNullOrEmpty(values.GetValueOrDefault("Password")))
                {
                    throw new McpException("`config.Password` is required when `config.AuthenticationType` is \"Password\".");
                }

                break;

            case "Web":
                if (string.Equals(values.GetValueOrDefault("AuthenticationType"), "User", StringComparison.OrdinalIgnoreCase) &&
                    string.IsNullOrEmpty(values.GetValueOrDefault("Password")))
                {
                    throw new McpException("`config.Password` is required when `config.AuthenticationType` is \"User\".");
                }

                break;
        }
    }

    // ── connection-string construction (mirrors DbSource/RedisSource/EmailSource/RabbitMQSource) ──

    internal static string BuildConnectionString(SourceCatalog.Entry entry, IReadOnlyDictionary<string, string?> v)
    {
        string Get(string fieldName) => v.GetValueOrDefault(fieldName) ?? string.Empty;

        switch (entry.SourceType)
        {
            case "SqlDatabase":
            {
                var server = Get("Server");
                var isNamedInstance = server.Contains('\\');
                var port = ParseIntOrDefault(Get("Port"), 1433);
                if (isNamedInstance && port == 1433)
                {
                    port = 0;
                }

                var portString = port > 0 ? "," + port : string.Empty;
                var authType = Get("AuthenticationType");
                var isWindows = string.IsNullOrEmpty(authType) || string.Equals(authType, "Windows", StringComparison.OrdinalIgnoreCase);
                var authString = isWindows ? "Integrated Security=SSPI;" : $"User ID={Get("UserID")};Password={Get("Password")};";
                var trust = ParseBoolOrDefault(Get("TrustServerCertificate"), false);
                var trustString = trust ? ";TrustServerCertificate=True" : string.Empty;
                var timeout = ParseIntOrDefault(Get("ConnectionTimeout"), 30);
                return $"Data Source={server}{portString};Initial Catalog={Get("DatabaseName")};{authString};Connection Timeout={timeout}{trustString}";
            }

            case "MySqlDatabase":
            {
                var port = ParseIntOrDefault(Get("Port"), 3306);
                var portString = port > 0 ? $"Port={port};" : string.Empty;
                var timeout = ParseIntOrDefault(Get("ConnectionTimeout"), 30);
                return $"Server={Get("Server")};{portString}Database={Get("DatabaseName")};Uid={Get("UserID")};Pwd={Get("Password")};Connect Timeout={timeout};SslMode=Preferred;";
            }

            case "PostgreSQL":
            {
                var port = ParseIntOrDefault(Get("Port"), 5432);
                var portString = port > 0 ? $"Port={port};" : string.Empty;
                var timeout = ParseIntOrDefault(Get("ConnectionTimeout"), 30);
                return $"Host={Get("Server")};{portString}Username={Get("UserID")};Password={Get("Password")};Database={Get("DatabaseName")};Timeout={timeout}";
            }

            case "Oracle":
            {
                var port = ParseIntOrDefault(Get("Port"), 1521);
                var portString = port > 0 ? $":{port}" : string.Empty;
                var databaseName = Get("DatabaseName");
                var dbString = !string.IsNullOrEmpty(databaseName) ? $"Database={databaseName};" : string.Empty;
                var timeout = ParseIntOrDefault(Get("ConnectionTimeout"), 30);
                return $"User Id={Get("UserID")};Password={Get("Password")};Data Source={Get("Server")}{portString};{dbString}Connection Timeout={timeout};";
            }

            case "ODBC":
                return $"DSN={Get("DatabaseName")};";

            case "Redis":
            {
                var port = Get("Port");
                var authType = string.IsNullOrEmpty(Get("AuthenticationType")) ? "Anonymous" : Get("AuthenticationType");
                var parts = new List<string>
                {
                    $"HostName={Get("HostName")}",
                    $"Port={port}",
                    $"AuthenticationType={authType}",
                };

                if (string.Equals(authType, "Password", StringComparison.OrdinalIgnoreCase))
                {
                    parts.Add($"Password={Get("Password")}");
                }

                return string.Join(";", parts).EscapeString();
            }

            case "Email":
            {
                var port = ParseIntOrDefault(Get("Port"), 25);
                var enableSsl = ParseBoolOrDefault(Get("EnableSsl"), false);
                var timeout = ParseIntOrDefault(Get("Timeout"), 100000);
                return string.Join(";",
                    $"Host={Get("Host")}",
                    $"UserName={Get("UserName")}",
                    $"Password={Get("Password")}",
                    $"Port={port}",
                    $"EnableSsl={enableSsl}",
                    $"Timeout={timeout}");
            }

            case "RabbitMQ":
            {
                var port = ParseIntOrDefault(Get("Port"), 5672);
                var virtualHost = string.IsNullOrEmpty(Get("VirtualHost")) ? "/" : Get("VirtualHost");
                return string.Join(";",
                    $"HostName={Get("HostName")}",
                    $"Port={port}",
                    $"UserName={Get("UserName")}",
                    $"Password={Get("Password")}",
                    $"VirtualHost={virtualHost}");
            }

            case "Web":
            {
                // Mirrors WebSource.ToXml() exactly (Dev2.Data.ServiceModel.WebSource): key=value
                // segments in this order, UserName/Password appended only for "User" auth, then
                // EscapeString() on the whole joined string before the caller's DpapiWrapper.Encrypt.
                var authType = string.IsNullOrEmpty(Get("AuthenticationType")) ? "Anonymous" : Get("AuthenticationType");
                var parts = new List<string>
                {
                    $"Address={Get("Address")}",
                    $"DefaultQuery={Get("DefaultQuery")}",
                    $"AuthenticationType={authType}",
                };

                if (string.Equals(authType, "User", StringComparison.OrdinalIgnoreCase))
                {
                    parts.Add($"UserName={Get("UserName")}");
                    parts.Add($"Password={Get("Password")}");
                }

                return string.Join(";", parts).EscapeString();
            }

            default:
                throw new McpException($"sourceType '{entry.SourceType}' has no connection-string builder.");
        }
    }

    static int ParseIntOrDefault(string? raw, int fallback) =>
        int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) ? value : fallback;

    static bool ParseBoolOrDefault(string? raw, bool fallback) =>
        bool.TryParse(raw, out var value) ? value : fallback;

    // ── .bite XML construction (mirrors ResourceBase.ToXml()/DbSource.ToXml()/etc.'s shape) ──

    internal static string BuildSourceXml(
        SourceCatalog.Entry entry,
        string resourceId,
        string displayName,
        string encryptedConnectionString,
        int versionNumber,
        DateTimeOffset timestampUtc,
        string user)
    {
        var isDbEntry = entry.ResourceType == "DbSource";

        var root = new XElement("Source",
            new XAttribute("ID", resourceId),
            new XAttribute("Name", displayName),
            new XAttribute("ResourceType", isDbEntry ? entry.SourceType : entry.ResourceType),
            new XAttribute("IsValid", "false"),
            new XAttribute("Type", entry.ResourceType),
            new XAttribute("ConnectionString", encryptedConnectionString));

        if (isDbEntry)
        {
            // Mirrors DbSource.ToXml() — DB sources additionally carry the specific engine
            // (ServerType), distinct from the shared "DbSource" Type every engine uses.
            root.Add(new XAttribute("ServerType", entry.SourceType));
        }

        root.Add(
            // No Studio server identity exists for this MCP-authored save — Guid.Empty mirrors
            // EnvelopeBiteWriter's same placeholder choice for create_workflow.
            new XAttribute("ServerID", Guid.Empty.ToString()),
            new XElement("DisplayName", displayName),
            new XElement("Category", displayName),
            new XElement("AuthorRoles", string.Empty),
            new XElement("ErrorMessages"),
            new XElement("Comment", string.Empty),
            new XElement("HelpLink", string.Empty),
            new XElement("Tags", string.Empty),
            new XElement("UnitTestTargetWorkflowService", string.Empty),
            new XElement("BizRule", string.Empty),
            new XElement("WorkflowActivityDef", string.Empty),
            new XElement("VersionInfo",
                new XAttribute("DateTimeStamp", timestampUtc.UtcDateTime.ToString("O")),
                new XAttribute("Reason", "Save"),
                new XAttribute("User", user),
                new XAttribute("VersionNumber", versionNumber.ToString()),
                new XAttribute("ResourceId", resourceId),
                new XAttribute("VersionId", resourceId)));

        return root.ToString(SaveOptions.DisableFormatting);
    }
}

/// <summary>The full <c>add_source</c> response payload. Never includes the connection string or any secret value.</summary>
internal sealed record AddSourceResult(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("sourceType")] string SourceType,
    [property: JsonPropertyName("created")] bool Created,
    [property: JsonPropertyName("resolvedSecretFields")] IReadOnlyList<string> ResolvedSecretFields);
