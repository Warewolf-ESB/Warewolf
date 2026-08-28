/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace Warewolf.Execution.Lightweight.Mcp;

/// <summary>
/// The static "source types this server can create" table backing the <c>add_source</c> MCP
/// tool — one entry per <see cref="ToolCatalog"/> row whose <c>RequiresSource</c> flag is
/// <c>true</c> (SQL Server/PostgreSQL/MySQL/Oracle/ODBC Database, Redis Cache/Remove, RabbitMQ
/// Publish/Consume, Send Email (SMTP)), grouped by the underlying source shape rather than by
/// activity (several activities share one source type, e.g. every DB engine is a <c>DbSource</c>,
/// both RabbitMQ activities share one <c>RabbitMQSource</c>).
///
/// <para>
/// <b>Deliberate exception: <c>Web</c>.</b> The four Web Method tools (GET/POST/PUT/DELETE) keep
/// <c>RequiresSource: false</c> — an ad-hoc <c>querystring</c> still works standalone, with no
/// source — but they also accept an optional <c>sourceId</c> referencing a saved <c>WebSource</c>.
/// Before this entry existed, that second path was permanently dead: nothing could ever create a
/// <c>WebSource</c> for a caller to reference (F2). So <c>Web</c> is here despite its tools
/// reporting <c>RequiresSource: false</c>, which is the one entry that breaks the "one entry per
/// `RequiresSource: true` row" rule above.
/// </para>
///
/// <para>
/// Each entry's <see cref="Entry.Fields"/> mirrors the same property set the corresponding
/// hand-authored source class in <c>Dev2.Runtime.ServiceModel.Data</c>/<c>Dev2.Data.ServiceModel</c>
/// exposes (<c>DbSource</c>, <c>RedisSource</c>, <c>EmailSource</c>, <c>RabbitMQSource</c>,
/// <c>WebSource</c>) — see those classes' own <c>ToXml()</c>/constructor-from-XML pairs, which this
/// catalog was cross-checked against field-for-field. <see cref="AddSourceTool"/> does not reuse
/// those classes directly (same "Lightweight-local, no heavier dependency" choice
/// <see cref="EnvelopeBiteWriter"/> already made for workflows) but reproduces their
/// connection-string shape exactly, so a source this tool creates decrypts and runs identically
/// to one saved from Studio.
/// </para>
/// </summary>
internal static class SourceCatalog
{
    internal enum FieldKind
    {
        String,
        Int,
        Bool,
    }

    /// <summary>One configurable field on a source type.</summary>
    internal sealed record Field(
        string Name,
        FieldKind Kind,
        bool Required,
        bool IsSecret,
        string? Default,
        string Description);

    /// <summary>One entry in the catalog — a creatable source "shape".</summary>
    internal sealed record Entry(
        string SourceType,
        string ResourceType,
        string Category,
        string Description,
        IReadOnlyList<Field> Fields);

    /// <summary>
    /// <c>sourceType</c> values <c>add_source</c> accepts, each mapped to its configurable
    /// <c>config</c> fields. Field names/casing match the on-disk connection-string keys the
    /// matching hand-authored source class already parses (e.g. <c>DbSource.ConnectionString</c>'s
    /// setter, <c>RedisSource</c>'s <c>ParseProperties</c> dictionary), so a source this tool
    /// writes is byte-for-byte readable by the same runtime code path a Studio-saved source uses.
    /// </summary>
    internal static readonly IReadOnlyList<Entry> Entries = new List<Entry>
    {
        new("SqlDatabase", "DbSource", "Database", "Microsoft SQL Server.", new[]
        {
            new Field("Server", FieldKind.String, true, false, null, "Host name (optionally `host\\instance` for a named instance)."),
            new Field("DatabaseName", FieldKind.String, true, false, null, "The initial catalog / database name."),
            new Field("Port", FieldKind.Int, false, false, "1433", "TCP port. Ignored for a named-instance Server."),
            new Field("AuthenticationType", FieldKind.String, false, false, "Windows", "\"Windows\" (integrated security) or \"User\" (SQL login)."),
            new Field("UserID", FieldKind.String, false, false, null, "SQL login user name. Required when AuthenticationType is \"User\"."),
            new Field("Password", FieldKind.String, false, true, null, "SQL login password. Required when AuthenticationType is \"User\". Use ${secret-name} — never a literal secret."),
            new Field("ConnectionTimeout", FieldKind.Int, false, false, "30", "Connection timeout, in seconds."),
            new Field("TrustServerCertificate", FieldKind.Bool, false, false, "false", "Skip TLS certificate-chain validation. Only for trusted self-signed test servers."),
        }),
        new("MySqlDatabase", "DbSource", "Database", "MySQL.", new[]
        {
            new Field("Server", FieldKind.String, true, false, null, "Host name."),
            new Field("DatabaseName", FieldKind.String, true, false, null, "Database name."),
            new Field("Port", FieldKind.Int, false, false, "3306", "TCP port."),
            new Field("UserID", FieldKind.String, true, false, null, "User name."),
            new Field("Password", FieldKind.String, true, true, null, "Password. Use ${secret-name} — never a literal secret."),
            new Field("ConnectionTimeout", FieldKind.Int, false, false, "30", "Connection timeout, in seconds."),
        }),
        new("PostgreSQL", "DbSource", "Database", "PostgreSQL.", new[]
        {
            new Field("Server", FieldKind.String, true, false, null, "Host name."),
            new Field("DatabaseName", FieldKind.String, false, false, "", "Database name."),
            new Field("Port", FieldKind.Int, false, false, "5432", "TCP port."),
            new Field("UserID", FieldKind.String, true, false, null, "User name."),
            new Field("Password", FieldKind.String, true, true, null, "Password. Use ${secret-name} — never a literal secret."),
            new Field("ConnectionTimeout", FieldKind.Int, false, false, "30", "Connection timeout, in seconds."),
        }),
        new("Oracle", "DbSource", "Database", "Oracle.", new[]
        {
            new Field("Server", FieldKind.String, true, false, null, "Host name."),
            new Field("DatabaseName", FieldKind.String, false, false, null, "Database/service name."),
            new Field("Port", FieldKind.Int, false, false, "1521", "TCP port."),
            new Field("UserID", FieldKind.String, true, false, null, "User name."),
            new Field("Password", FieldKind.String, true, true, null, "Password. Use ${secret-name} — never a literal secret."),
            new Field("ConnectionTimeout", FieldKind.Int, false, false, "30", "Connection timeout, in seconds."),
        }),
        new("ODBC", "DbSource", "Database", "ODBC via a named DSN.", new[]
        {
            new Field("DatabaseName", FieldKind.String, true, false, null, "The ODBC DSN name."),
        }),
        new("Redis", "RedisSource", "Cache", "Redis cache.", new[]
        {
            new Field("HostName", FieldKind.String, true, false, null, "Host name."),
            new Field("Port", FieldKind.Int, false, false, "6379", "TCP port."),
            new Field("AuthenticationType", FieldKind.String, false, false, "Anonymous", "\"Anonymous\" or \"Password\"."),
            new Field("Password", FieldKind.String, false, true, null, "Required when AuthenticationType is \"Password\". Use ${secret-name} — never a literal secret."),
        }),
        new("Email", "EmailSource", "Communication", "SMTP email.", new[]
        {
            new Field("Host", FieldKind.String, true, false, null, "SMTP host name."),
            new Field("Port", FieldKind.Int, false, false, "25", "SMTP port."),
            new Field("UserName", FieldKind.String, false, false, null, "SMTP account user name."),
            new Field("Password", FieldKind.String, false, true, null, "SMTP account password. Use ${secret-name} — never a literal secret."),
            new Field("EnableSsl", FieldKind.Bool, false, false, "false", "Whether to negotiate TLS/SSL."),
            new Field("Timeout", FieldKind.Int, false, false, "100000", "Send timeout, in milliseconds."),
        }),
        new("RabbitMQ", "RabbitMQSource", "Communication", "RabbitMQ broker.", new[]
        {
            new Field("HostName", FieldKind.String, true, false, null, "Broker host name."),
            new Field("Port", FieldKind.Int, false, false, "5672", "TCP port."),
            new Field("UserName", FieldKind.String, false, false, null, "Broker user name."),
            new Field("Password", FieldKind.String, false, true, null, "Broker password. Use ${secret-name} — never a literal secret."),
            new Field("VirtualHost", FieldKind.String, false, false, "/", "Broker virtual host."),
        }),
        new("Web", "WebSource", "Integration", "An ad-hoc HTTP/HTTPS endpoint for the GET/POST/PUT/DELETE Web Method tools.", new[]
        {
            new Field("Address", FieldKind.String, true, false, null, "Base URL, e.g. https://api.example.com."),
            new Field("DefaultQuery", FieldKind.String, false, false, "", "Default query string/path appended to Address when the calling tool's own querystring is omitted."),
            new Field("AuthenticationType", FieldKind.String, false, false, "Anonymous", "\"Anonymous\" or \"User\"."),
            new Field("UserName", FieldKind.String, false, false, null, "Required when AuthenticationType is \"User\"."),
            new Field("Password", FieldKind.String, false, true, null, "Required when AuthenticationType is \"User\". Use ${secret-name} — never a literal secret."),
        }),
    };

    /// <summary>
    /// Resolves <paramref name="sourceType"/> case-insensitively to its catalog entry, or
    /// <c>null</c> when it names no supported source type.
    /// </summary>
    internal static Entry? Resolve(string sourceType)
    {
        if (string.IsNullOrWhiteSpace(sourceType))
        {
            return null;
        }

        return Entries.FirstOrDefault(entry => string.Equals(entry.SourceType, sourceType, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>A comma-separated list of every supported <c>sourceType</c> value, for error messages.</summary>
    internal static string SupportedTypesList => string.Join(", ", Entries.Select(e => e.SourceType));
}
