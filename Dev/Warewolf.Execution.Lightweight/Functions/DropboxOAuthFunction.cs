/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*/

using Dev2.Common;
using Dev2.Runtime.Hosting;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using Warewolf.Execution.Lightweight.Security;

namespace Warewolf.Execution.Lightweight.Functions
{
    /// <summary>
    /// Server-side Dropbox PKCE OAuth 2.0 Authorization Code flow — no
    /// PowerShell helper script required.
    ///
    /// <para><b>One-time Dropbox App Console setup</b></para>
    /// <list type="bullet">
    ///   <item>
    ///     Go to <c>https://www.dropbox.com/developers/apps</c>, open your app,
    ///     click <c>Settings</c> → <c>OAuth 2</c> → <c>Redirect URIs</c> and add:
    ///     <code>https://{your-function-app}.azurewebsites.net/oauth/dropbox/callback</code>
    ///     For local testing also add:
    ///     <code>http://localhost:7071/oauth/dropbox/callback</code>
    ///   </item>
    /// </list>
    ///
    /// <para><b>Usage (per Dropbox source)</b></para>
    /// <list type="number">
    ///   <item>
    ///     Open the start URL in any browser:
    ///     <code>https://{app}.azurewebsites.net/oauth/dropbox/start?sourceId={resourceGuid}&amp;appKey={dropboxAppKey}</code>
    ///     If the AppKey is already stored in the <c>.bite</c> file, the
    ///     <c>appKey</c> query parameter may be omitted.
    ///   </item>
    ///   <item>Approve the app on Dropbox's authorization page.</item>
    ///   <item>
    ///     The server receives the callback automatically, exchanges the code
    ///     for tokens, writes them back to the <c>.bite</c> file on disk, and
    ///     invalidates the in-memory cache — all without a server restart.
    ///   </item>
    /// </list>
    ///
    /// <para><b>Routes (no /api/ prefix — routePrefix is empty)</b></para>
    /// <list type="bullet">
    ///   <item><c>GET /oauth/dropbox/start?sourceId={guid}[&amp;appKey={key}]</c></item>
    ///   <item><c>GET /oauth/dropbox/callback</c> — called by Dropbox after approval</item>
    /// </list>
    ///
    /// <para><b>Security note</b></para>
    /// The <c>/start</c> endpoint should be accessed only by administrators.
    /// Consider restricting it behind Azure App Service authentication or a
    /// function-key if the function app is publicly reachable.
    /// The <c>/callback</c> endpoint is safe to expose publicly: it validates the
    /// PKCE <c>state</c> parameter, which acts as a one-time CSRF token, and
    /// sessions expire after 5 minutes.
    /// </summary>
    public sealed class DropboxOAuthFunction
    {
        // ── Dependencies ──────────────────────────────────────────────────────────
        private readonly ILogger<DropboxOAuthFunction> _logger;
        private readonly string              _workflowsDirectory;
        private readonly FileDecryptionHelper?  _decryptionHelper;
        private readonly KeyVaultSecretManager? _secretManager;

        // ── Shared HTTP client (one per process — avoid socket exhaustion) ────────
        private static readonly HttpClient _http = new(new SocketsHttpHandler
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(10)
        }) { Timeout = TimeSpan.FromSeconds(30) };

        // ── In-memory PKCE session store ──────────────────────────────────────────
        // Key = state (GUID hex string). Values expire after 5 minutes.
        // The ConcurrentDictionary is process-global so /start and /callback share it
        // even when the Functions host creates multiple DropboxOAuthFunction instances.
        private record PkceSession(
            string CodeVerifier,
            string SourceId,
            string AppKey,
            string CallbackUri,
            DateTimeOffset ExpiresAt);

        private static readonly ConcurrentDictionary<string, PkceSession> _sessions = new();

        // ── Dropbox OAuth constants ───────────────────────────────────────────────
        private const string DropboxScopes =
            "account_info.read files.metadata.read files.content.read files.content.write";

        // ── Constructor ───────────────────────────────────────────────────────────
        public DropboxOAuthFunction(
            IServiceProvider serviceProvider,
            ILogger<DropboxOAuthFunction> logger)
        {
            _logger = logger;

            // _workflowsDirectory is used as a fallback scan location when the
            // SourceLoader's index doesn't yet have the file (e.g. the first
            // workflow execution hasn't happened yet).  It can also point to a
            // source Resources\ folder so that token updates persist across
            // dotnet builds.  The SourceLoader's indexed path always takes
            // precedence — see UpdateBiteFile.
            // WOLF-8516: no env-var fallback — WorkflowsDirectory comes solely from
            // HostEnvironmentConfig (deploy-bundled settings file), which DI always supplies.
            _workflowsDirectory = serviceProvider.GetService<Infrastructure.HostEnvironmentConfig>()?.WorkflowsDirectory
                ?? Path.Combine(AppContext.BaseDirectory, "Resources");

            _logger.LogWarning("[OAuth] WorkflowsDirectory='{Dir}' (encryption={EncryptionEnabled})",
                _workflowsDirectory,
                _secretManager is not null ? "Key Vault" : "none");

            // Both services are optional — they are only registered when Key Vault
            // encryption is enabled. When absent the function still works, but can
            // only update unencrypted .bite files.
            _decryptionHelper = serviceProvider.GetService<FileDecryptionHelper>();
            _secretManager    = serviceProvider.GetService<KeyVaultSecretManager>();
        }

        // ── /oauth/dropbox/start ──────────────────────────────────────────────────

        /// <summary>
        /// Generates a PKCE code-verifier/challenge pair, caches the session keyed
        /// by <c>state</c>, then issues a <c>302</c> redirect to Dropbox's
        /// authorization page.  After the user approves, Dropbox redirects to
        /// <c>/oauth/dropbox/callback</c>.
        /// </summary>
        [Function("DropboxOAuthStart")]
        public async Task<HttpResponseData> Start(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get",
                         Route = "oauth/dropbox/start")] HttpRequestData req)
        {
            // The /start query string carries ?appKey={dropboxAppKey} — a client credential —
            // so the URL is never logged. Only the route and the outcome are recorded.
            _logger.LogInformation("[OAuth] /start called.");

            var query    = HttpUtility.ParseQueryString(req.Url.Query);
            var sourceId = (query["sourceId"] ?? string.Empty).Trim();
            var appKey   = (query["appKey"]   ?? string.Empty).Trim();

            // If the caller only supplied sourceId, try to read AppKey from the .bite file.
            if (!string.IsNullOrEmpty(sourceId) && string.IsNullOrEmpty(appKey))
                appKey = TryReadAppKey(sourceId) ?? string.Empty;

            if (string.IsNullOrWhiteSpace(appKey))
            {
                return await HtmlErrorAsync(req, HttpStatusCode.BadRequest,
                    "Missing Dropbox App Key.",
                    "Provide <code>?appKey={key}</code> or " +
                    "<code>?sourceId={guid}</code> where the .bite file already " +
                    "contains the App Key in its ConnectionString.");
            }

            // ── PKCE generation ───────────────────────────────────────────────────
            var raw          = new byte[32];
            RandomNumberGenerator.Fill(raw);
            var codeVerifier  = ToBase64Url(raw);
            var challengeHash = SHA256.HashData(Encoding.ASCII.GetBytes(codeVerifier));
            var codeChallenge = ToBase64Url(challengeHash);

            // ── Session storage ───────────────────────────────────────────────────
            var state       = Guid.NewGuid().ToString("N");
            var callbackUri = BuildUri(req, "/oauth/dropbox/callback");

            PurgeExpiredSessions();
            _sessions[state] = new PkceSession(
                CodeVerifier: codeVerifier,
                SourceId:     sourceId,
                AppKey:       appKey,
                CallbackUri:  callbackUri,
                ExpiresAt:    DateTimeOffset.UtcNow.AddMinutes(5));

            // ── Build Dropbox authorization URL ───────────────────────────────────
            var authUrl =
                "https://www.dropbox.com/oauth2/authorize" +
                $"?client_id={Uri.EscapeDataString(appKey)}" +
                "&response_type=code" +
                $"&redirect_uri={Uri.EscapeDataString(callbackUri)}" +
                $"&state={state}" +
                $"&code_challenge={codeChallenge}" +
                "&code_challenge_method=S256" +
                "&token_access_type=offline" +
                $"&scope={Uri.EscapeDataString(DropboxScopes)}";

            var response = req.CreateResponse(HttpStatusCode.Found);
            response.Headers.Add("Location", authUrl);
            return response;
        }

        // ── /oauth/dropbox/callback ───────────────────────────────────────────────

        /// <summary>
        /// Receives the authorization code from Dropbox, validates the <c>state</c>
        /// against the PKCE session cache, exchanges the code for tokens, and writes
        /// the new <c>AccessToken</c> and <c>RefreshToken</c> back to the matching
        /// <c>.bite</c> file on disk.
        /// </summary>
        [Function("DropboxOAuthCallback")]
        public async Task<HttpResponseData> Callback(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get",
                         Route = "oauth/dropbox/callback")] HttpRequestData req)
        {
            // The /callback query string carries ?code={authorization_code} and ?state={csrf}
            // — both security tokens — so the URL is never logged.
            _logger.LogInformation("[OAuth] /callback called.");

            var query = HttpUtility.ParseQueryString(req.Url.Query);
            var code  = (query["code"]  ?? string.Empty).Trim();
            var state = (query["state"] ?? string.Empty).Trim();
            var error = (query["error"] ?? string.Empty).Trim();

            // ── User denied ───────────────────────────────────────────────────────
            if (!string.IsNullOrEmpty(error))
            {
                return await HtmlErrorAsync(req, HttpStatusCode.OK,
                    "Authorization Denied",
                    $"Dropbox returned an error: <code>{HtmlEnc(error)}</code>");
            }

            if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(state))
            {
                return await HtmlErrorAsync(req, HttpStatusCode.BadRequest,
                    "Missing Parameters",
                    "Both <code>code</code> and <code>state</code> are required.");
            }

            // ── PKCE session lookup ───────────────────────────────────────────────
            // The OAuth `state` is a one-time CSRF token — not logged. The session count is
            // non-sensitive and is what actually diagnoses a lookup miss.
            _logger.LogInformation("[OAuth] Session lookup (active sessions={Count})", _sessions.Count);

            if (!_sessions.TryRemove(state, out var session))
            {
                _logger.LogWarning("[OAuth] State '{State}' not found in session store.", state);
                return await HtmlErrorAsync(req, HttpStatusCode.BadRequest,
                    "Session Expired or Invalid",
                    "The OAuth session has expired (5-minute limit) or the " +
                    "<code>state</code> parameter doesn't match.<br/>" +
                    "Please <a href=\"/oauth/dropbox/start\">start a new flow</a>.");
            }

            if (DateTimeOffset.UtcNow > session.ExpiresAt)
            {
                _logger.LogWarning("[OAuth] State '{State}' found but expired at {ExpiresAt}.", state, session.ExpiresAt);
                return await HtmlErrorAsync(req, HttpStatusCode.BadRequest,
                    "Session Expired or Invalid",
                    "The OAuth session has expired (5-minute limit).<br/>" +
                    "Please <a href=\"/oauth/dropbox/start\">start a new flow</a>.");
            }

            _logger.LogWarning("[OAuth] Session found — sourceId={SourceId} appKey={AppKey}",
                session.SourceId, session.AppKey[..Math.Min(4, session.AppKey.Length)] + "…");

            // ── Token exchange ────────────────────────────────────────────────────
            var form = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["code"]          = code,
                ["grant_type"]    = "authorization_code",
                ["client_id"]     = session.AppKey,
                ["redirect_uri"]  = session.CallbackUri,
                ["code_verifier"] = session.CodeVerifier,
            });

            HttpResponseMessage tokenResp;
            try
            {
                tokenResp = await _http.PostAsync(
                    "https://api.dropboxapi.com/oauth2/token", form);
            }
            catch (Exception ex)
            {
                return await HtmlErrorAsync(req, HttpStatusCode.BadGateway,
                    "Network Error",
                    $"Could not reach Dropbox token endpoint: <code>{HtmlEnc(ex.Message)}</code>");
            }

            var body = await tokenResp.Content.ReadAsStringAsync();

            if (!tokenResp.IsSuccessStatusCode)
            {
                return await HtmlErrorAsync(req, HttpStatusCode.BadGateway,
                    $"Dropbox Token Exchange Failed (HTTP {(int)tokenResp.StatusCode})",
                    $"<pre style='overflow:auto;background:#f5f5f5;padding:8px'>" +
                    $"{HtmlEnc(body)}</pre>");
            }

            JObject json;
            try { json = JObject.Parse(body); }
            catch
            {
                return await HtmlErrorAsync(req, HttpStatusCode.BadGateway,
                    "Invalid Token Response",
                    $"Dropbox returned non-JSON: <pre>{HtmlEnc(body)}</pre>");
            }

            var accessToken  = json["access_token"]?.Value<string>() ?? string.Empty;
            var refreshToken = json["refresh_token"]?.Value<string>() ?? string.Empty;
            var expiresIn    = json["expires_in"]?.Value<int>() ?? 0;

            DateTime? expiresAt = expiresIn > 0
                ? DateTime.UtcNow.AddSeconds(expiresIn)
                : null;

            _logger.LogWarning(
                "[OAuth] Token exchange succeeded — accessToken len={AtLen} refreshToken={RtPresent} expiresIn={ExpiresIn}s (expiresAt={ExpiresAt})",
                accessToken.Length,
                string.IsNullOrEmpty(refreshToken) ? "MISSING" : $"present (len={refreshToken.Length})",
                expiresIn,
                expiresAt?.ToString("O") ?? "never");

            if (string.IsNullOrEmpty(accessToken))
            {
                return await HtmlErrorAsync(req, HttpStatusCode.BadGateway,
                    "Missing access_token",
                    $"Dropbox response contained no access_token.<br/>" +
                    $"<pre>{HtmlEnc(body)}</pre>");
            }

            if (string.IsNullOrEmpty(refreshToken))
            {
                return await HtmlErrorAsync(req, HttpStatusCode.BadGateway,
                    "Missing refresh_token",
                    "Dropbox returned no refresh_token — the app must have " +
                    "<code>token_access_type=offline</code> enabled.<br/>" +
                    "In the Dropbox App Console → Permissions, ensure the app is " +
                    "allowed to request offline access.");
            }

            // ── Write tokens back to .bite file ───────────────────────────────────
            string updateDetail;
            if (!string.IsNullOrEmpty(session.SourceId))
            {
                try
                {
                    updateDetail = UpdateBiteFile(
                        session.SourceId, accessToken, refreshToken, session.AppKey, expiresAt);
                }
                catch (Exception ex)
                {
                    // Tokens obtained but file write failed — show them so the
                    // operator can use Get-DropboxTokens.ps1 as fallback.
                    var atPreview = accessToken.Length > 8
                        ? accessToken[..8] + "…"
                        : accessToken;
                    var rtPreview = refreshToken.Length > 8
                        ? refreshToken[..8] + "…"
                        : refreshToken;

                    return await HtmlErrorAsync(req, HttpStatusCode.InternalServerError,
                        "Tokens Retrieved, But .bite Update Failed",
                        $"<b>Error:</b> <code>{HtmlEnc(ex.Message)}</code><br/><br/>" +
                        $"You can update the .bite file manually using " +
                        $"<code>Get-DropboxTokens.ps1</code> with this connection string:<br/>" +
                        $"<pre>AccessToken={HtmlEnc(accessToken)};" +
                        $"AppKey={HtmlEnc(session.AppKey)};" +
                        $"RefreshToken={HtmlEnc(refreshToken)}</pre>" +
                        $"<small>AccessToken preview: {atPreview} | " +
                        $"RefreshToken preview: {rtPreview}</small>");
                }
            }
            else
            {
                updateDetail = "No <code>sourceId</code> was supplied — " +
                               "tokens were not written to any .bite file.";
            }

            // ── Success page ──────────────────────────────────────────────────────
            var atPrev = accessToken.Length > 8  ? accessToken[..8]  + "…" : accessToken;
            var rtPrev = refreshToken.Length > 8 ? refreshToken[..8] + "…" : refreshToken;

            return await HtmlSuccessAsync(req,
                $"<table style='border-collapse:collapse;width:100%'>" +
                $"<tr><td style='padding:4px 8px;font-weight:bold'>AccessToken</td>" +
                $"    <td style='padding:4px 8px'><code>{HtmlEnc(atPrev)}</code> " +
                $"        (length {accessToken.Length})</td></tr>" +
                $"<tr><td style='padding:4px 8px;font-weight:bold'>RefreshToken</td>" +
                $"    <td style='padding:4px 8px'><code>{HtmlEnc(rtPrev)}</code> " +
                $"        (length {refreshToken.Length})</td></tr>" +
                $"<tr><td style='padding:4px 8px;font-weight:bold'>Expires In</td>" +
                $"    <td style='padding:4px 8px'>{expiresIn}s</td></tr>" +
                $"</table>" +
                $"<hr style='margin:16px 0'/>" +
                $"<p>{updateDetail}</p>");
        }

        // ─────────────────────────────────────────────────────────────────────────
        // Helpers: .bite file update
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Locates ALL copies of the .bite file for <paramref name="sourceId"/> and
        /// writes new tokens into each one:
        /// <list type="bullet">
        ///   <item>
        ///     The path already recorded in <see cref="LightweightSourceLoader"/>'s
        ///     directory index — this is the authoritative file the source loader will
        ///     re-read from after cache invalidation (typically the build output
        ///     <c>bin\Debug\net8.0\Resources\</c> or the publish directory).
        ///   </item>
        ///   <item>
        ///     The file found by scanning <c>_workflowsDirectory</c> (which may be the
        ///     source <c>Resources\</c> folder so the change persists across
        ///     <c>dotnet build</c> rebuilds).
        ///   </item>
        /// </list>
        /// Updating both ensures that (a) the running server sees the new tokens
        /// immediately and (b) they are not lost the next time the project is rebuilt.
        /// </summary>
        private string UpdateBiteFile(
            string sourceId, string accessToken, string refreshToken, string appKey, DateTime? expiresAt)
        {
            if (!Guid.TryParse(sourceId, out var guid))
                throw new ArgumentException($"sourceId '{sourceId}' is not a valid GUID.");

            // ── Collect every file path that needs updating ────────────────────────
            // Priority 1: the path the SourceLoader's index already knows about.
            //             This is what EnsureSourceLoaded will re-read after Invalidate().
            // Priority 2: a scan of _workflowsDirectory (may be the source Resources\
            //             folder — different from the SourceLoader's output directory).
            var pathsToUpdate = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var indexedPath = LightweightSourceLoader.Instance.GetIndexedFilePath(guid);
            if (indexedPath is not null)
            {
                pathsToUpdate.Add(indexedPath);
                _logger.LogWarning("[OAuth] SourceLoader indexed path: '{Path}'", indexedPath);
            }
            else
            {
                _logger.LogWarning("[OAuth] SourceLoader index has no entry for {Guid} yet " +
                                   "(no workflow has executed against this source since startup). " +
                                   "Falling back to _workflowsDirectory scan.", guid);
            }

            var scannedPath = FindBiteFile(guid);
            if (scannedPath is not null)
            {
                if (pathsToUpdate.Add(scannedPath))
                    _logger.LogWarning("[OAuth] Additional path from directory scan: '{Path}'", scannedPath);
                else
                    _logger.LogWarning("[OAuth] Directory scan found same path as index — single update for '{Path}'.", scannedPath);
            }

            if (pathsToUpdate.Count == 0)
                throw new FileNotFoundException(
                    $"No .bite file found for ResourceID={sourceId} " +
                    $"in the SourceLoader index or in '{_workflowsDirectory}'.");

            // ── Build the new connection string once ───────────────────────────────
            // We read/decrypt the first file we can access to preserve any extra
            // key=value pairs that are not the three token fields.
            string newPlain;
            bool   wasEncrypted = false;

            var referenceFile = pathsToUpdate.First();
            var refXe         = XElement.Load(referenceFile);
            var refConnAttr   = refXe.Attribute("ConnectionString")
                ?? throw new InvalidOperationException(
                       $"'{Path.GetFileName(referenceFile)}' has no ConnectionString attribute.");

            var raw = refConnAttr.Value;
            wasEncrypted = FileDecryptionHelper.IsAesEncrypted(raw);

            string plainText;
            if (wasEncrypted)
            {
                if (_decryptionHelper is not null)
                {
                    plainText = _decryptionHelper.DecryptConnectionString(raw);
                    _logger.LogInformation("[OAuth] Decrypted existing AES connection string.");
                }
                else
                {
                    // Key Vault not configured — can't decrypt, but we're overwriting
                    // all three token fields anyway so the old values don't matter.
                    plainText = string.Empty;
                    _logger.LogWarning("[OAuth] Key Vault not configured — starting from empty props " +
                                       "(AccessToken/RefreshToken/AppKey will be overwritten).");
                }
            }
            else
            {
                plainText = raw;
            }

            var props = ParseConnectionString(plainText);
            props["AccessToken"]  = accessToken;
            props["RefreshToken"] = refreshToken;
            if (!string.IsNullOrEmpty(appKey))
                props["AppKey"] = appKey;
            if (expiresAt.HasValue)
                props["ExpiresAt"] = expiresAt.Value.ToString("O");

            newPlain = string.Join(";", props.Select(kv => $"{kv.Key}={kv.Value}"));

            string newConnString;
            string encNote;
            if (wasEncrypted && _secretManager is not null)
            {
                newConnString = AesGcmEncrypt(newPlain, _secretManager.GetKeyBytes());
                encNote = " (re-encrypted with AES-256-GCM)";
            }
            else if (wasEncrypted)
            {
                newConnString = newPlain;
                encNote = " ⚠️ written as <b>plain text</b> — " +
                          "Key Vault is not configured on this host. " +
                          "Run <code>Encrypt-Config.ps1</code> before deploying to Azure.";
                _logger.LogWarning("[OAuth] Writing plain-text tokens (Key Vault not configured).");
            }
            else
            {
                newConnString = newPlain;
                encNote = " (plain text)";
            }

            // ── Write to every file path ───────────────────────────────────────────
            foreach (var biteFile in pathsToUpdate)
            {
                _logger.LogWarning("[OAuth] Writing updated tokens to '{BiteFile}'", biteFile);
                var xe       = XElement.Load(biteFile);
                var connAttr = xe.Attribute("ConnectionString")
                    ?? throw new InvalidOperationException(
                           $"'{Path.GetFileName(biteFile)}' has no ConnectionString attribute.");
                connAttr.Value = newConnString;
                xe.Save(biteFile);
                _logger.LogWarning("[OAuth] Saved '{BiteFile}'{EncNote}", biteFile, encNote);
            }

            // ── Invalidate the in-memory source cache ──────────────────────────────
            LightweightSourceLoader.Instance.Invalidate(guid);
            _logger.LogWarning("[OAuth] Source cache invalidated for {Guid}", guid);

            var updatedList = string.Join("<br/>",
                pathsToUpdate.Select(p => $"• <b>{HtmlEnc(Path.GetFileName(p))}</b> " +
                                          $"<small style='color:#666'>({HtmlEnc(Path.GetDirectoryName(p)!)})</small>"));

            return
                $"Updated {pathsToUpdate.Count} file(s){encNote}:<br/>{updatedList}<br/><br/>" +
                "The in-memory source cache has been invalidated — " +
                "the next workflow execution will use the new tokens automatically.";
        }

        /// <summary>
        /// Scans <see cref="_workflowsDirectory"/> for the .bite file whose root-element
        /// <c>ResourceID</c> matches <paramref name="sourceId"/>.
        /// Only the root element is read (XmlReader peek) — no full document load.
        /// Returns <c>null</c> when the directory doesn't exist or no file matches.
        /// </summary>
        private string? FindBiteFile(Guid sourceId)
        {
            if (!Directory.Exists(_workflowsDirectory))
            {
                _logger.LogWarning("[OAuth] _workflowsDirectory '{Dir}' does not exist — scan skipped.", _workflowsDirectory);
                return null;
            }

            var settings = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Ignore,
                XmlResolver   = null,
                IgnoreWhitespace           = true,
                IgnoreComments             = true,
                IgnoreProcessingInstructions = true,
            };

            foreach (var file in Directory.EnumerateFiles(
                         _workflowsDirectory, "*.bite", SearchOption.AllDirectories))
            {
                try
                {
                    using var reader = XmlReader.Create(file, settings);
                    while (reader.Read())
                    {
                        if (reader.NodeType != XmlNodeType.Element)
                            continue;

                        var idStr = reader.GetAttribute("ResourceID")
                                 ?? reader.GetAttribute("ID");
                        if (Guid.TryParse(idStr, out var id) && id == sourceId)
                            return file;

                        break; // Root element only — stop after first element.
                    }
                }
                catch { /* inaccessible or malformed file — skip silently */ }
            }

            _logger.LogWarning("[OAuth] FindBiteFile: no file with ResourceID={SourceId} found in '{Dir}'",
                sourceId, _workflowsDirectory);
            return null;
        }

        /// <summary>
        /// Reads the AppKey from the .bite file for <paramref name="sourceId"/>.
        /// Returns <c>null</c> when the file is not found or does not contain an AppKey.
        /// </summary>
        private string? TryReadAppKey(string sourceId)
        {
            if (!Guid.TryParse(sourceId, out var guid))
                return null;

            var biteFile = FindBiteFile(guid);
            if (biteFile is null)
                return null;

            try
            {
                var xe       = XElement.Load(biteFile);
                var raw      = xe.Attribute("ConnectionString")?.Value ?? string.Empty;
                var plainText = FileDecryptionHelper.IsAesEncrypted(raw)
                    ? (_decryptionHelper?.DecryptConnectionString(raw) ?? raw)
                    : raw;

                var props = ParseConnectionString(plainText);
                return props.TryGetValue("AppKey", out var key) ? key : null;
            }
            catch
            {
                return null;
            }
        }

        // ─────────────────────────────────────────────────────────────────────────
        // Static helpers
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Parses a semicolon-delimited <c>Key=Value</c> connection string into
        /// a case-insensitive dictionary.  Values may contain '=' (only the first
        /// '=' in each segment is treated as the key/value separator).
        /// </summary>
        private static Dictionary<string, string> ParseConnectionString(string connectionString)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(connectionString))
                return result;

            foreach (var segment in connectionString.Split(';',
                         StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                var eq = segment.IndexOf('=');
                if (eq > 0)
                    result[segment[..eq].Trim()] = segment[(eq + 1)..];
            }

            return result;
        }

        /// <summary>
        /// Encodes <paramref name="data"/> as URL-safe base64 without padding
        /// (the format required by OAuth 2.0 PKCE).
        /// </summary>
        private static string ToBase64Url(byte[] data) =>
            Convert.ToBase64String(data)
                   .TrimEnd('=')
                   .Replace('+', '-')
                   .Replace('/', '_');

        /// <summary>
        /// AES-256-GCM encrypts <paramref name="plainText"/> with <paramref name="keyBytes"/>
        /// and returns a <c>WFAES::</c>-prefixed base64 string in the same format
        /// that <c>Encrypt-Config.ps1</c> produces.
        /// Layout: <c>[12-byte nonce][ciphertext][16-byte GCM tag]</c>
        /// </summary>
        private static string AesGcmEncrypt(string plainText, byte[] keyBytes)
        {
            const int NonceSize = 12;
            const int TagSize   = 16;

            var plaintextBytes = Encoding.UTF8.GetBytes(plainText);
            var nonce          = new byte[NonceSize];
            RandomNumberGenerator.Fill(nonce);

            var ciphertext = new byte[plaintextBytes.Length];
            var tag        = new byte[TagSize];

            using var aes = new AesGcm(keyBytes, TagSize);
            aes.Encrypt(nonce, plaintextBytes, ciphertext, tag);

            var combined = new byte[NonceSize + plaintextBytes.Length + TagSize];
            nonce     .CopyTo(combined, 0);
            ciphertext.CopyTo(combined, NonceSize);
            tag       .CopyTo(combined, NonceSize + plaintextBytes.Length);

            return FileDecryptionHelper.WfAesPrefix + Convert.ToBase64String(combined);
        }

        /// <summary>
        /// Constructs an absolute URI from the request URL, replacing the path
        /// segment after the last <c>/</c>-delimited route part with
        /// <paramref name="newPath"/> (which must start with <c>/</c>).
        /// </summary>
        private static string BuildUri(HttpRequestData req, string newPath)
        {
            var requestUrl = req.Url;
            var scheme = requestUrl.Scheme;
            if (req.Headers.TryGetValues("X-Forwarded-Proto", out var protoHeaders))
            {
                var proto = protoHeaders.FirstOrDefault();
                if (!string.IsNullOrEmpty(proto))
                    scheme = proto;
            }

            var host = requestUrl.Host;
            if (req.Headers.TryGetValues("X-Forwarded-Host", out var hostHeaders))
            {
                var fwHost = hostHeaders.FirstOrDefault();
                if (!string.IsNullOrEmpty(fwHost))
                    host = fwHost;
            }

            var builder = new UriBuilder(scheme, host);
            
            // Retain original port only if not overridden by reverse proxy
            if (!req.Headers.Contains("X-Forwarded-Proto") && !req.Headers.Contains("X-Forwarded-Host"))
            {
                if (!requestUrl.IsDefaultPort)
                    builder.Port = requestUrl.Port;
            }
            else
            {
                builder.Port = -1; // Use default port for the scheme
            }

            builder.Path  = newPath;
            builder.Query = string.Empty;
            return builder.Uri.ToString().TrimEnd('/');
        }

        /// <summary>
        /// Removes sessions from <see cref="_sessions"/> that have passed their
        /// expiry time.  Called on each <c>/start</c> hit.
        /// </summary>
        private static void PurgeExpiredSessions()
        {
            var now     = DateTimeOffset.UtcNow;
            var expired = _sessions
                .Where(kv => now > kv.Value.ExpiresAt)
                .Select(kv => kv.Key)
                .ToList();
            foreach (var key in expired)
                _sessions.TryRemove(key, out _);
        }

        private static string HtmlEnc(string? s) =>
            System.Net.WebUtility.HtmlEncode(s ?? string.Empty);

        // ─────────────────────────────────────────────────────────────────────────
        // HTML response builders
        // ─────────────────────────────────────────────────────────────────────────

        private static async Task<HttpResponseData> HtmlErrorAsync(
            HttpRequestData req,
            HttpStatusCode  statusCode,
            string          title,
            string          detail)
        {
            var resp = req.CreateResponse(statusCode);
            resp.Headers.Add("Content-Type", "text/html; charset=utf-8");
            await resp.WriteStringAsync(RenderPage(
                title:   $"&#x274C; {HtmlEnc(title)}",
                content: $"<p style='color:#c0392b'>{detail}</p>",
                color:   "#c0392b"));
            return resp;
        }

        private static async Task<HttpResponseData> HtmlSuccessAsync(
            HttpRequestData req,
            string          detail)
        {
            var resp = req.CreateResponse(HttpStatusCode.OK);
            resp.Headers.Add("Content-Type", "text/html; charset=utf-8");
            await resp.WriteStringAsync(RenderPage(
                title:   "&#x2705; Dropbox Tokens Updated",
                content: detail,
                color:   "#27ae60"));
            return resp;
        }

        private static string RenderPage(string title, string content, string color) => $$"""
            <!DOCTYPE html>
            <html lang="en">
            <head>
              <meta charset="utf-8"/>
              <meta name="viewport" content="width=device-width,initial-scale=1"/>
              <title>Dropbox OAuth — Warewolf</title>
              <style>
                body { font-family: -apple-system,BlinkMacSystemFont,'Segoe UI',sans-serif;
                       max-width: 760px; margin: 48px auto; padding: 0 16px;
                       color: #222; background: #fafafa; }
                h1   { color: {{color}}; font-size: 1.4rem; margin-bottom: .5rem; }
                code { background:#eee; padding:2px 5px; border-radius:3px; font-size:.9em; }
                pre  { background:#f4f4f4; padding:12px; border-radius:4px;
                       overflow-x:auto; font-size:.85em; }
                hr   { border:none; border-top:1px solid #ddd; }
                a    { color:#2980b9; }
              </style>
            </head>
            <body>
              <h1>{{title}}</h1>
              {{content}}
              <hr/>
              <small style="color:#888">Warewolf Lightweight Execution Server — Dropbox OAuth 2.0</small>
            </body>
            </html>
            """;
    }
}
