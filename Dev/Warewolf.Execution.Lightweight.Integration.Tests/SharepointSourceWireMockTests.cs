/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*/

using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using Dev2.Common.Interfaces;
using Dev2.Data.ServiceModel;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Security.Encryption;
using Warewolf.Sharepoint;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace Warewolf.Execution.Lightweight.Integration.Tests
{
    /// <summary>
    /// WireMock-driven tests covering the high-CRAP-score constructor branches
    /// of <see cref="SharepointSource"/> (decryption error detection, connection
    /// string parsing, IsSharepointOnline flag) and end-to-end flow into
    /// <see cref="SharepointHelper"/>'s REST surface backed by a WireMock server.
    ///
    /// The <see cref="SharepointSource(XElement, ISharepointHelperFactory)"/>
    /// ctor was the single highest risk hotspot on the coverage dashboard
    /// (Crap Score 600, Cyclomatic Complexity 24). These tests pin down every
    /// distinct decision branch in that ctor plus the parsing logic that
    /// populates Server / UserName / Password / AuthenticationType from the
    /// decrypted ConnectionString.
    /// </summary>
    [TestClass]
    public class SharepointSourceWireMockTests
    {
        private WireMockServer? _wireMock;
        private string SpUrl => $"http://localhost:{_wireMock!.Port}";

        // Test-only AES-256-CBC hook matching the format produced by the
        // hook installed by Dev2.UnitTestUtils.DpapiTestBase. We re-install it
        // locally to avoid a cross-project test reference.
        private static readonly byte[] AesKey =
            Encoding.UTF8.GetBytes("WarewolfTestHardcodedAESKey12345");

        private Func<string, string>? _savedEncryptHook;
        private Func<string, string>? _savedDecryptHook;

        [TestInitialize]
        public void Setup()
        {
            _wireMock = WireMockServer.Start();

            // Snapshot any globally-installed hooks so we can scope per-test
            // changes (e.g. clearing AesDecryptHook to exercise the error path).
            _savedEncryptHook = DpapiWrapper.AesEncryptHook;
            _savedDecryptHook = DpapiWrapper.AesDecryptHook;

            InstallTestAesHooks();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _wireMock?.Stop();
            DpapiWrapper.AesEncryptHook = _savedEncryptHook;
            DpapiWrapper.AesDecryptHook = _savedDecryptHook;
        }

        private static void InstallTestAesHooks()
        {
            DpapiWrapper.AesEncryptHook = plain =>
            {
                using var aes = Aes.Create();
                aes.Key = AesKey;
                aes.GenerateIV();
                using var enc = aes.CreateEncryptor();
                var data = Encoding.Unicode.GetBytes(plain);
                var cipher = enc.TransformFinalBlock(data, 0, data.Length);
                var combined = new byte[aes.IV.Length + cipher.Length];
                aes.IV.CopyTo(combined, 0);
                cipher.CopyTo(combined, aes.IV.Length);
                return "WFAES::" + Convert.ToBase64String(combined);
            };
            DpapiWrapper.AesDecryptHook = cipher =>
            {
                var payload = Convert.FromBase64String(cipher.Substring("WFAES::".Length));
                var iv = payload[..16];
                var body = payload[16..];
                using var aes = Aes.Create();
                aes.Key = AesKey;
                aes.IV = iv;
                using var dec = aes.CreateDecryptor();
                var plain = dec.TransformFinalBlock(body, 0, body.Length);
                return Encoding.Unicode.GetString(plain);
            };
        }

        /// <summary>
        /// Builds the minimum Resource &lt;Source&gt; XML required by
        /// <see cref="ResourceBase(XElement)"/> with a caller-supplied
        /// ConnectionString attribute and optional IsSharepointOnline flag.
        /// </summary>
        private static XElement BuildSourceXml(string connectionString, string? isSharepointOnline = null)
        {
            var source = new XElement("Source",
                new XAttribute("ID", Guid.NewGuid()),
                new XAttribute("Name", "test sp source"),
                new XAttribute("Type", "SharepointServerSource"),
                new XAttribute("ConnectionString", connectionString),
                new XAttribute("Version", "1.0"),
                new XAttribute("ResourceType", "SharepointServerSource"),
                new XAttribute("ServerID", Guid.NewGuid()));

            if (isSharepointOnline != null)
            {
                source.Add(new XAttribute("IsSharepointOnline", isSharepointOnline));
            }
            source.Add(new XElement("DisplayName", "test sp source"));
            source.Add(new XElement("Category", "TEST"));
            return source;
        }

        // -----------------------------------------------------------------
        // Default ctor
        // -----------------------------------------------------------------

        [TestMethod]
        [TestCategory("LiveIntegration_SharePoint")]
        public void TC_SharepointSource_DefaultCtor_SetsExpectedDefaults()
        {
            var src = new SharepointSource();

            Assert.AreEqual(Guid.Empty, src.ResourceID);
            Assert.AreEqual("SharepointServerSource", src.ResourceType);
            Assert.AreEqual(AuthenticationType.Windows, src.AuthenticationType);
            Assert.IsFalse(src.IsSharepointOnline);
            Assert.IsTrue(src.IsSource);
            Assert.IsFalse(src.IsService);
        }

        // -----------------------------------------------------------------
        // XElement ctor: connection-string parsing
        // -----------------------------------------------------------------

        [TestMethod]
        [TestCategory("LiveIntegration_SharePoint")]
        public void TC_SharepointSource_XmlCtor_PlainConnectionString_ParsesAllProperties()
        {
            var xml = BuildSourceXml(
                "Server=http://example/site;AuthenticationType=User;UserName=alice;Password=s3cret");

            var src = new SharepointSource(xml);

            Assert.AreEqual("http://example/site", src.Server);
            Assert.AreEqual(AuthenticationType.User, src.AuthenticationType);
            Assert.AreEqual("alice", src.UserName);
            Assert.AreEqual("s3cret", src.Password);
            Assert.AreEqual("SharepointServerSource", src.ResourceType);
        }

        [TestMethod]
        [TestCategory("LiveIntegration_SharePoint")]
        public void TC_SharepointSource_XmlCtor_MissingConnectionString_LeavesPropertiesEmpty()
        {
            var xml = BuildSourceXml(string.Empty);

            var src = new SharepointSource(xml);

            Assert.AreEqual(string.Empty, src.Server);
            Assert.AreEqual(string.Empty, src.UserName);
            Assert.AreEqual(string.Empty, src.Password);
            Assert.AreEqual(AuthenticationType.Windows, src.AuthenticationType);
        }

        [TestMethod]
        [TestCategory("LiveIntegration_SharePoint")]
        public void TC_SharepointSource_XmlCtor_UnknownAuthenticationType_DefaultsToWindows()
        {
            // Enum.TryParse returns false for "Banana" so ctor falls back to Windows.
            var xml = BuildSourceXml("Server=http://x;AuthenticationType=Banana");

            var src = new SharepointSource(xml);

            Assert.AreEqual(AuthenticationType.Windows, src.AuthenticationType);
            Assert.AreEqual("http://x", src.Server);
        }

        [TestMethod]
        [TestCategory("LiveIntegration_SharePoint")]
        public void TC_SharepointSource_XmlCtor_IsSharepointOnlineTrue_FlagSet()
        {
            var xml = BuildSourceXml("Server=http://x;AuthenticationType=Windows",
                                     isSharepointOnline: "true");

            var src = new SharepointSource(xml);

            Assert.IsTrue(src.IsSharepointOnline);
        }

        [TestMethod]
        [TestCategory("LiveIntegration_SharePoint")]
        public void TC_SharepointSource_XmlCtor_IsSharepointOnlineFalse_FlagCleared()
        {
            var xml = BuildSourceXml("Server=http://x;AuthenticationType=Windows",
                                     isSharepointOnline: "false");

            var src = new SharepointSource(xml);

            Assert.IsFalse(src.IsSharepointOnline);
        }

        [TestMethod]
        [TestCategory("LiveIntegration_SharePoint")]
        public void TC_SharepointSource_XmlCtor_IsSharepointOnlineInvalid_StaysFalse()
        {
            // bool.TryParse fails ⇒ branch stays at default (false).
            var xml = BuildSourceXml("Server=http://x", isSharepointOnline: "not-a-bool");

            var src = new SharepointSource(xml);

            Assert.IsFalse(src.IsSharepointOnline);
        }

        // -----------------------------------------------------------------
        // XElement ctor: WFAES error / success branches
        // -----------------------------------------------------------------

        [TestMethod]
        [TestCategory("LiveIntegration_SharePoint")]
        public void TC_SharepointSource_XmlCtor_WfaesPrefix_WithoutHook_ThrowsHelpfulError()
        {
            // Clear the hook to drive the "AES hook not registered" branch.
            DpapiWrapper.AesDecryptHook = null;
            var xml = BuildSourceXml("WFAES::deadbeefdeadbeefdeadbeefdeadbeef");

            var ex = Assert.ThrowsException<InvalidOperationException>(() => new SharepointSource(xml));

            StringAssert.Contains(ex.Message, "WFAES::");
            StringAssert.Contains(ex.Message, "AesDecryptHook");
            StringAssert.Contains(ex.Message, "AZURE_KEYVAULT_NAME");
        }

        [TestMethod]
        [TestCategory("LiveIntegration_SharePoint")]
        public void TC_SharepointSource_XmlCtor_WfaesPrefix_HookThrows_WrapsWithContext()
        {
            // Hook present but throws → ctor must wrap with operation context.
            DpapiWrapper.AesDecryptHook = _ => throw new CryptographicException("bad key");
            var xml = BuildSourceXml("WFAES::Zm9v");  // valid base64 after the prefix

            var ex = Assert.ThrowsException<InvalidOperationException>(() => new SharepointSource(xml));

            StringAssert.Contains(ex.Message, "Failed to decrypt");
            StringAssert.Contains(ex.Message, "WFAES::");
            StringAssert.Contains(ex.Message, "bad key");
            Assert.IsInstanceOfType(ex.InnerException, typeof(CryptographicException));
        }

        [TestMethod]
        [TestCategory("LiveIntegration_SharePoint")]
        public void TC_SharepointSource_XmlCtor_WfaesPrefix_ValidHook_DecryptsAndParses()
        {
            // Encrypt using the test hook so the ctor's decrypt branch succeeds.
            var encrypted = DpapiWrapper.AesEncryptHook!(
                "Server=http://wfaes.example/site;AuthenticationType=User;UserName=bob;Password=p@ss");
            var xml = BuildSourceXml(encrypted);

            var src = new SharepointSource(xml);

            Assert.AreEqual("http://wfaes.example/site", src.Server);
            Assert.AreEqual(AuthenticationType.User, src.AuthenticationType);
            Assert.AreEqual("bob", src.UserName);
            Assert.AreEqual("p@ss", src.Password);
        }

        // -----------------------------------------------------------------
        // XElement ctor: DPAPI base64 detection branch
        // -----------------------------------------------------------------

        [TestMethod]
        [TestCategory("LiveIntegration_SharePoint")]
        public void TC_SharepointSource_XmlCtor_DpapiLikeBase64NoPrefix_ThrowsClearError()
        {
            // A non-WFAES value that "looks like base64" but cannot be DPAPI-decrypted
            // on this platform — covers the explicit error branch added to ctor.
            const string fakeDpapi = "QUJDREVGR0hJSktMTU5PUFFSU1RVVldYWVo=";  // base64 of plain ASCII

            // Sanity: the test environment must not be able to decrypt this.
            // (On Linux DPAPI throws; on Windows the value isn't real DPAPI cipher.)
            DpapiWrapper.AesDecryptHook = null;  // ensure WFAES branch is irrelevant

            var xml = BuildSourceXml(fakeDpapi);

            var ex = Assert.ThrowsException<InvalidOperationException>(() => new SharepointSource(xml));
            StringAssert.Contains(ex.Message, "DPAPI-encrypted");
            StringAssert.Contains(ex.Message, "AesDecryptHook");
        }

        // -----------------------------------------------------------------
        // End-to-end: SharepointSource → SharepointHelper → WireMock
        // -----------------------------------------------------------------

        [TestMethod]
        [TestCategory("LiveIntegration_SharePoint")]
        public void TC_SharepointSource_LoadLists_DelegatesToHelperOverHttp()
        {
            _wireMock!.Given(Request.Create().WithPath("/_api/web/lists").UsingGet())
                .RespondWith(Response.Create().WithStatusCode(200)
                    .WithHeader("Content-Type", "application/json")
                    .WithBody("""{"d":{"results":[{"Title":"Alpha"},{"Title":"Beta"}]}}"""));

            var xml = BuildSourceXml($"Server={SpUrl};AuthenticationType=Windows");
            var src = new SharepointSource(xml, new SharepointHelperFactory());

            var lists = src.LoadLists();

            Assert.AreEqual(2, lists.Count);
            Assert.AreEqual("Alpha", lists[0].FullName);
            Assert.AreEqual("Beta", lists[1].FullName);
        }

        [TestMethod]
        [TestCategory("LiveIntegration_SharePoint")]
        public void TC_SharepointSource_LoadFieldsForList_PassesEditableFlag()
        {
            _wireMock!.Given(Request.Create()
                    .WithPath("/_api/web/lists/getbytitle('Tasks')/fields")
                    .WithParam("$filter", "Hidden eq false and ReadOnlyField eq false")
                    .UsingGet())
                .RespondWith(Response.Create().WithStatusCode(200)
                    .WithHeader("Content-Type", "application/json")
                    .WithBody("""{"d":{"results":[{"Title":"T","InternalName":"T","FieldTypeKind":2}]}}"""));

            var xml = BuildSourceXml($"Server={SpUrl};AuthenticationType=User;UserName=u;Password=p");
            var src = new SharepointSource(xml, new SharepointHelperFactory());

            var fields = src.LoadFieldsForList("Tasks", editableFieldsOnly: true);

            Assert.AreEqual(1, fields.Count);
            Assert.AreEqual("T", fields[0].Name);
        }

        [TestMethod]
        [TestCategory("LiveIntegration_SharePoint")]
        public void TC_SharepointSource_LoadFieldsForList_DefaultOverload_NonEditable()
        {
            _wireMock!.Given(Request.Create()
                    .WithPath("/_api/web/lists/getbytitle('Tasks')/fields")
                    .WithParam("$filter", "Hidden eq false")
                    .UsingGet())
                .RespondWith(Response.Create().WithStatusCode(200)
                    .WithHeader("Content-Type", "application/json")
                    .WithBody("""{"d":{"results":[]}}"""));

            var xml = BuildSourceXml($"Server={SpUrl};AuthenticationType=Windows");
            var src = new SharepointSource(xml, new SharepointHelperFactory());

            // Single-arg overload should route to editableFieldsOnly = false.
            var fields = src.LoadFieldsForList("Tasks");

            Assert.AreEqual(0, fields.Count);
        }

        [TestMethod]
        [TestCategory("LiveIntegration_SharePoint")]
        public void TC_SharepointSource_LoadLists_ServerError_PropagatesHttpException()
        {
            _wireMock!.Given(Request.Create().WithPath("/_api/web/lists").UsingGet())
                .RespondWith(Response.Create().WithStatusCode(500).WithBody("nope"));

            var xml = BuildSourceXml($"Server={SpUrl};AuthenticationType=Windows");
            var src = new SharepointSource(xml, new SharepointHelperFactory());

            var ex = Assert.ThrowsException<HttpRequestException>(() => src.LoadLists());
            StringAssert.Contains(ex.Message, "LoadLists");
            StringAssert.Contains(ex.Message, "500");
        }

        // -----------------------------------------------------------------
        // ToXml round-trip
        // -----------------------------------------------------------------

        [TestMethod]
        [TestCategory("LiveIntegration_SharePoint")]
        public void TC_SharepointSource_ToXml_WindowsAuth_OmitsCredentialsInConnectionString()
        {
            var src = new SharepointSource
            {
                Server = "http://srv/site",
                AuthenticationType = AuthenticationType.Windows,
                UserName = "should-not-appear",
                Password = "should-not-appear",
                IsSharepointOnline = true,
                ResourceID = Guid.NewGuid(),
                ResourceName = "spSrc",
            };

            var xml = src.ToXml();

            var conn = (string)xml.Attribute("ConnectionString")!;
            var decrypted = DpapiWrapper.DecryptIfEncrypted(conn);
            StringAssert.Contains(decrypted, "Server=http://srv/site");
            StringAssert.Contains(decrypted, "AuthenticationType=Windows");
            Assert.IsFalse(decrypted.Contains("UserName="), "UserName must not be serialized for Windows auth");
            Assert.IsFalse(decrypted.Contains("Password="), "Password must not be serialized for Windows auth");
            Assert.IsTrue(
                bool.Parse((string)xml.Attribute("IsSharepointOnline")!),
                "IsSharepointOnline attribute must serialize as a truthy boolean");
        }

        [TestMethod]
        [TestCategory("LiveIntegration_SharePoint")]
        public void TC_SharepointSource_ToXml_UserAuth_IncludesCredentials()
        {
            var src = new SharepointSource
            {
                Server = "http://srv/site",
                AuthenticationType = AuthenticationType.User,
                UserName = "carol",
                Password = "c@rol",
                ResourceID = Guid.NewGuid(),
                ResourceName = "spSrc",
            };

            var xml = src.ToXml();
            var decrypted = DpapiWrapper.DecryptIfEncrypted((string)xml.Attribute("ConnectionString")!);

            StringAssert.Contains(decrypted, "Server=http://srv/site");
            StringAssert.Contains(decrypted, "AuthenticationType=User");
            StringAssert.Contains(decrypted, "UserName=carol");
            StringAssert.Contains(decrypted, "Password=c@rol");
        }

        [TestMethod]
        [TestCategory("LiveIntegration_SharePoint")]
        public void TC_SharepointSource_RoundTrip_ToXml_ThenXmlCtor_PreservesProperties()
        {
            var original = new SharepointSource
            {
                Server = $"{SpUrl}",
                AuthenticationType = AuthenticationType.User,
                UserName = "dora",
                Password = "p@ss",
                IsSharepointOnline = true,
                ResourceID = Guid.NewGuid(),
                ResourceName = "spSrc",
            };

            var xml = original.ToXml();
            var rehydrated = new SharepointSource(xml);

            Assert.AreEqual(original.Server, rehydrated.Server);
            Assert.AreEqual(original.UserName, rehydrated.UserName);
            Assert.AreEqual(original.Password, rehydrated.Password);
            Assert.AreEqual(original.AuthenticationType, rehydrated.AuthenticationType);
            Assert.AreEqual(original.IsSharepointOnline, rehydrated.IsSharepointOnline);
        }
    }
}
