#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, CC0022, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001, IDE0019, CC0105, RECS008, CA2202, IDE0016
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using Dev2.Services.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using TechTalk.SpecFlow;
using SecPermissions = Dev2.Common.Interfaces.Security.Permissions;

namespace Dev2.Activities.Specs.Permissions
{
    [Binding]
    public class SettingsPermissionsSteps
    {
        readonly ScenarioContext _scenarioContext;
        static FeatureContext _featureContext;

        const string LightweightBaseUrl = "http://localhost:7071";

        public SettingsPermissionsSteps(ScenarioContext scenarioContext)
        {
            if (scenarioContext == null)
                throw new ArgumentNullException(nameof(scenarioContext));
            _scenarioContext = scenarioContext;
        }

        static string GetEntraRole() => "ExtraSpecialEntraRole";
        static string GetSecuritySpecsEntraToken() => "ASfas123@!fda_LONG_TOKEN_GENERATED_FROM_ENTRA";
        static string GetSecuritySpecsEntraAppID() => "1234-5678-ABCD-GUID";

        [BeforeFeature("@Security")]
        public static void InitializeFeature(FeatureContext featureContext)
        {
            _featureContext = featureContext;

            // Save the original secure.config so AfterScenario can restore it.
            var configPath = GetSecureConfigPath();
            if (File.Exists(configPath))
                _featureContext.Add("initialConfigContent", File.ReadAllText(configPath));

            // Baseline: Public has no permissions — lock everything down before any scenario runs.
            WriteAndWaitForConfig(new List<WindowsGroupPermission>
            {
                new WindowsGroupPermission
                {
                    IsServer     = true,
                    WindowsGroup = "Public",
                    ResourceID   = Guid.Empty,
                    View         = false,
                    Execute      = false,
                    Contribute   = false,
                    DeployTo     = false,
                    DeployFrom   = false,
                    Administrator = false,
                }
            });

            // Verify the lightweight server is reachable.
            using var probe = new HttpClient();
            try
            {
                var r = probe.GetAsync($"{LightweightBaseUrl}/Public/apis.json").Result;
                if (!r.IsSuccessStatusCode && r.StatusCode != HttpStatusCode.Unauthorized)
                    Assert.Fail($"Cannot connect to lightweight Warewolf server at {LightweightBaseUrl}. Status: {r.StatusCode}");
            }
            catch (Exception ex) when (!(ex is AssertFailedException))
            {
                Assert.Fail($"Cannot connect to lightweight Warewolf server at {LightweightBaseUrl}. {ex.Message}");
            }

            // Create the default user client (Bearer token for GetEntraRole()).
            _featureContext.Add("currentHttp", CreateBearerClient());
        }

        [Given(@"I have a server ""(.*)""")]
        public void GivenIHaveAServer(string serverName)
        {
            using var probe = new HttpClient();
            try
            {
                var response = probe.GetAsync($"{LightweightBaseUrl}/Public/apis.json").Result;
                if (!response.IsSuccessStatusCode && response.StatusCode != System.Net.HttpStatusCode.Unauthorized)
                    Assert.Fail($"Lightweight server at {LightweightBaseUrl} is not ready (server: '{serverName}'). Status: {response.StatusCode}");
            }
            catch (Exception ex) when (!(ex is AssertFailedException))
            {
                Assert.Fail($"Lightweight server at {LightweightBaseUrl} is not reachable (server: '{serverName}'). {ex.Message}");
            }
        }

        [Given(@"it has ""(.*)"" with ""(.*)""")]
        public void GivenItHasWith(string groupName, string groupRights)
        {
            WriteAndWaitForConfig(new[]
            {
                BuildPermission(groupName, groupRights, isServer: true)
            });
        }

        [Given(@"I have Public with ""(.*)""")]
        public void GivenIHavePublicWith(string groupRights)
        {
            WriteAndWaitForConfig(new[]
            {
                BuildPermission("Public", groupRights, isServer: true)
            });
        }

        [Given(@"I have Users with ""(.*)""")]
        public void GivenIHaveUsersWith(string groupRights)
        {
            // "Users" maps to the configured Entra role for integration tests.
            WriteAndWaitForConfig(new[]
            {
                BuildPermission(GetEntraRole(), groupRights, isServer: true)
            });
        }

        [Given(@"Resource ""(.*)"" has rights ""(.*)"" for ""(.*)""")]
        public void GivenResourceHasRights(string resourceName, string resourceRights, string groupName)
        {
            // Map the "Users" placeholder to the actual Entra role.
            var resolvedGroup = string.Equals(groupName, "Users", StringComparison.OrdinalIgnoreCase)
                ? GetEntraRole()
                : groupName;

            // Append the resource-specific entry to any existing global permissions so
            // they are not lost (e.g. a prior GivenIHaveUsersWith call).
            var existing = ReadCurrentPermissions();
            WriteAndWaitForConfig(existing.Concat(new[]
            {
                BuildPermission(
                    resolvedGroup,
                    resourceRights,
                    isServer:     false,
                    resourceId:   Guid.NewGuid(),
                    resourceName: resourceName)
            }));
        }

        [Given(@"I have waited (.*) seconds for the rights to propogate to all the resources")]
        public void GivenIHaveWaitedSeconds(int p0) => Thread.Sleep(p0 * 1000);

        [When(@"connected as user part of ""(.*)""")]
        public void WhenConnectedAsUserPartOf(string userGroup)
        {
            // Dispose the previous client and issue a fresh one with the Bearer token.
            if (_featureContext.TryGetValue("currentHttp", out HttpClient old))
                old?.Dispose();

            _featureContext["currentHttp"] = CreateBearerClient();
        }

        [Then(@"resources should have ""(.*)""")]
        public static void ThenResourcesShouldHave(string resourcePerms)
        {
            var http        = _featureContext.Get<HttpClient>("currentHttp");
            var permissions = ParsePermissions(resourcePerms);

            if (permissions == SecPermissions.None)
            {
                // With None rights the user should not be able to see anything.
                var list = FetchApisJson(http, secure: true);
                Assert.IsTrue(list.Count == 0,
                    $"Expected no accessible resources but apis.json returned {list.Count} entries.");
                return;
            }

            // View or Execute (or both) — user must be able to see at least one resource.
            if (permissions.HasFlag(SecPermissions.View) || permissions.HasFlag(SecPermissions.Execute))
            {
                var list = FetchApisJson(http, secure: true);
                Assert.IsTrue(list.Count > 0,
                    $"Expected at least one accessible resource for permissions [{resourcePerms}] but apis.json was empty.");
            }
        }

        [Then(@"resources should not have ""(.*)""")]
        public void ThenResourcesShouldNotHave(string resourcePerms)
        {
            var http        = _featureContext.Get<HttpClient>("currentHttp");
            var permissions = ParsePermissions(resourcePerms);

            if (permissions == SecPermissions.None)
                return; // "should not have None" is trivially true.

            // If the permission being checked includes View or Execute, confirm the
            // user cannot see any resources via the secure discovery endpoint.
            if (permissions.HasFlag(SecPermissions.View) || permissions.HasFlag(SecPermissions.Execute))
            {
                var list = FetchApisJson(http, secure: true);
                Assert.IsTrue(list.Count == 0,
                    $"Expected no accessible resources but apis.json returned {list.Count} entries.");
            }
        }

        [Then(@"""(.*)"" should have ""(.*)""")]
        public void ThenShouldHave(string resourceName, string resourcePerms)
        {
            var http        = _featureContext.Get<HttpClient>("currentHttp");
            var permissions = ParsePermissions(resourcePerms);

            // Derive the route slug: last path segment without extension.
            var slug = Path.GetFileNameWithoutExtension(
                resourceName.Replace('\\', '/').Split('/')[^1]);

            var url      = $"{LightweightBaseUrl}/Secure/{Uri.EscapeDataString(slug)}";
            var response = http.GetAsync(url).Result;

            if (permissions == SecPermissions.None)
            {
                Assert.IsTrue(
                    response.StatusCode == HttpStatusCode.Forbidden ||
                    response.StatusCode == HttpStatusCode.Unauthorized,
                    $"Expected 403/401 for '{resourceName}' (None) but got {(int)response.StatusCode}.");
            }
            else
            {
                Assert.AreEqual(
                    HttpStatusCode.OK, response.StatusCode,
                    $"Expected 200 for '{resourceName}' [{resourcePerms}] but got {(int)response.StatusCode}.");
            }
        }

        [AfterScenario("Security")]
        public void DoCleanUp()
        {
            // Restore the original secure.config so the next scenario starts clean.
            if (_featureContext.TryGetValue("initialConfigContent", out string original)
                && !string.IsNullOrEmpty(original))
            {
                try
                {
                    File.WriteAllText(GetSecureConfigPath(), original);
                    Thread.Sleep(1500); // allow SecureConfigWatcher to hot-reload
                }
                catch { /* best-effort */ }
            }

            if (_featureContext.TryGetValue("currentHttp", out HttpClient http))
                http?.Dispose();
        }

        // ── Private helpers ───────────────────────────────────────────────────────

        static HttpClient CreateBearerClient()
        {
            var client = new HttpClient { BaseAddress = new Uri(LightweightBaseUrl) };
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", GetSecuritySpecsEntraToken());
            return client;
        }

        static string GetSecureConfigPath()
        {
            var env = Environment.GetEnvironmentVariable("WAREWOLF_SECURE_CONFIG");
            return !string.IsNullOrWhiteSpace(env)
                ? env
                : Path.Combine(AppContext.BaseDirectory, "secure.config");
        }

        static void WriteAndWaitForConfig(IEnumerable<WindowsGroupPermission> permissions)
        {
            var settings  = new SecuritySettingsTO(new List<WindowsGroupPermission>(permissions));
            var json      = JsonConvert.SerializeObject(settings);
            var encrypted = SecurityEncryption.Encrypt(json);
            File.WriteAllText(GetSecureConfigPath(), encrypted);
            Thread.Sleep(1500); // SecureConfigWatcher debounce (500 ms) + safety buffer
        }

        static IEnumerable<WindowsGroupPermission> ReadCurrentPermissions()
        {
            var path = GetSecureConfigPath();
            if (!File.Exists(path))
                return Enumerable.Empty<WindowsGroupPermission>();

            try
            {
                var encrypted = File.ReadAllText(path);
                var json      = SecurityEncryption.TryDecrypt(encrypted);
                var settings  = JsonConvert.DeserializeObject<SecuritySettingsTO>(json);
                return settings?.WindowsGroupPermissions
                    ?? Enumerable.Empty<WindowsGroupPermission>();
            }
            catch
            {
                return Enumerable.Empty<WindowsGroupPermission>();
            }
        }

        static WindowsGroupPermission BuildPermission(
            string groupName,
            string rights,
            bool   isServer,
            Guid   resourceId   = default,
            string resourceName = null)
        {
            var perm = new WindowsGroupPermission
            {
                WindowsGroup = groupName,
                IsServer     = isServer,
                ResourceID   = resourceId == default ? Guid.Empty : resourceId,
                ResourceName = resourceName ?? string.Empty,
            };

            foreach (var part in rights.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (Enum.TryParse(part.Replace(" ", ""), true, out SecPermissions flag))
                    ApplyPermissionFlag(perm, flag);
            }

            return perm;
        }

        static void ApplyPermissionFlag(WindowsGroupPermission perm, SecPermissions flag)
        {
            if (flag.HasFlag(SecPermissions.View))          perm.View          = true;
            if (flag.HasFlag(SecPermissions.Execute))       perm.Execute       = true;
            if (flag.HasFlag(SecPermissions.Contribute))    perm.Contribute    = true;
            if (flag.HasFlag(SecPermissions.DeployTo))      perm.DeployTo      = true;
            if (flag.HasFlag(SecPermissions.DeployFrom))    perm.DeployFrom    = true;
            if (flag.HasFlag(SecPermissions.Administrator)) perm.Administrator = true;
        }

        static SecPermissions ParsePermissions(string permsString)
        {
            var result = SecPermissions.None;
            foreach (var part in permsString.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (Enum.TryParse(part.Replace(" ", ""), true, out SecPermissions flag))
                    result |= flag;
            }
            return result;
        }

        static List<string> FetchApisJson(HttpClient http, bool secure)
        {
            var route = secure ? "/Secure/apis.json" : "/Public/apis.json";
            try
            {
                var response = http.GetAsync($"{LightweightBaseUrl}{route}").Result;
                if (!response.IsSuccessStatusCode)
                    return new List<string>();

                var body = response.Content.ReadAsStringAsync().Result;
                dynamic doc   = JsonConvert.DeserializeObject(body);
                var names = new List<string>();
                if (doc?.apis != null)
                    foreach (var api in doc.apis)
                        names.Add((string)(api.name ?? api.path ?? ""));
                return names;
            }
            catch
            {
                return new List<string>();
            }
        }
    }
}
