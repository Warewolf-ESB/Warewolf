/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*/

using System;
using System.Xml.Linq;
using Dev2.Data.ServiceModel;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.UnitTestUtils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Security.Encryption;

namespace Dev2.Data.Tests.ServiceModel
{
    /// <summary>
    /// Coverage for <see cref="SharepointSource"/> XElement constructor
    /// (Crap Score 600, Cyclomatic Complexity 24 on the coverage dashboard).
    ///
    /// The XElement ctor has multiple guard branches around the
    /// ConnectionString attribute (WFAES prefix without hook, DPAPI-only
    /// blob, decrypt failure) plus property parsing and enum fallbacks.
    /// Each branch is hit explicitly below.
    /// </summary>
    [TestClass]
    [DoNotParallelize]
    public class SharepointSourceCtorTests : DpapiTestBase
    {
        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------

        private const string ResId = "11111111-2222-3333-4444-555555555555";

        private static XElement MakeXml(string connectionString, string isSharepointOnline = "false")
        {
            // The ResourceBase(xml) ctor pulls ID, ResourceType, Name, etc.
            // SharepointSource then reads ConnectionString + IsSharepointOnline.
            return new XElement("Source",
                new XAttribute("ID", ResId),
                new XAttribute("Name", "test-sp"),
                new XAttribute("ResourceType", "SharepointServerSource"),
                new XAttribute("ConnectionString", connectionString ?? string.Empty),
                new XAttribute("IsSharepointOnline", isSharepointOnline),
                new XElement("TypeOf", "SharepointServerSource"),
                new XElement("DisplayName", "test-sp"));
        }

        // ------------------------------------------------------------------
        // Default ctor + ToXml branches
        // ------------------------------------------------------------------

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(SharepointSource))]
        public void Ctor_Default_SetsResourceTypeAndAuthDefaults()
        {
            var sut = new SharepointSource();

            Assert.AreEqual(Guid.Empty, sut.ResourceID);
            Assert.AreEqual("SharepointServerSource", sut.ResourceType);
            Assert.AreEqual(AuthenticationType.Windows, sut.AuthenticationType);
            Assert.IsTrue(sut.IsSource);
            Assert.IsFalse(sut.IsService);
            Assert.IsFalse(sut.IsFolder);
            Assert.IsFalse(sut.IsReservedService);
            Assert.IsFalse(sut.IsServer);
            Assert.IsFalse(sut.IsResourceVersion);
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(SharepointSource))]
        public void ToXml_AuthTypeUser_IncludesUserNameAndPassword()
        {
            var sut = new SharepointSource
            {
                Server = "https://sp.example.com",
                AuthenticationType = AuthenticationType.User,
                UserName = "alice",
                Password = "secret",
                IsSharepointOnline = true
            };

            var xml = sut.ToXml();

            var conString = (string)xml.Attribute("ConnectionString");
            Assert.IsNotNull(conString);
            // ConnectionString is encrypted via AES hook -> "WFAES::..."
            var decrypted = DpapiWrapper.Decrypt(conString);
            StringAssert.Contains(decrypted, "Server=https://sp.example.com");
            StringAssert.Contains(decrypted, "AuthenticationType=User");
            StringAssert.Contains(decrypted, "UserName=alice");
            StringAssert.Contains(decrypted, "Password=secret");
            Assert.IsTrue(bool.Parse((string)xml.Attribute("IsSharepointOnline")));
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(SharepointSource))]
        public void ToXml_AuthTypeWindows_OmitsUserNameAndPassword()
        {
            var sut = new SharepointSource
            {
                Server = "https://sp.example.com",
                AuthenticationType = AuthenticationType.Windows,
                UserName = "should-not-leak",
                Password = "should-not-leak",
            };

            var xml = sut.ToXml();

            var decrypted = DpapiWrapper.Decrypt((string)xml.Attribute("ConnectionString"));
            Assert.IsFalse(decrypted.Contains("UserName="), "Windows auth must not serialise UserName.");
            Assert.IsFalse(decrypted.Contains("Password="), "Windows auth must not serialise Password.");
            StringAssert.Contains(decrypted, "AuthenticationType=Windows");
        }

        // ------------------------------------------------------------------
        // XElement ctor: plaintext (non-base64) connection string branch
        // ------------------------------------------------------------------

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(SharepointSource))]
        public void Ctor_XElement_PlaintextConnectionString_ParsesAllProperties()
        {
            // ';' is not a base64 character, so IsBase64() = false and the
            // string is consumed as-is by ParseProperties.
            var conStr = "Server=spserver;AuthenticationType=User;UserName=bob;Password=hunter2";
            var sut = new SharepointSource(MakeXml(conStr, isSharepointOnline: "true"));

            Assert.AreEqual(new Guid(ResId), sut.ResourceID);
            Assert.AreEqual("SharepointServerSource", sut.ResourceType);
            Assert.AreEqual("spserver", sut.Server);
            Assert.AreEqual("bob", sut.UserName);
            Assert.AreEqual("hunter2", sut.Password);
            Assert.AreEqual(AuthenticationType.User, sut.AuthenticationType);
            Assert.IsTrue(sut.IsSharepointOnline);
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(SharepointSource))]
        public void Ctor_XElement_InvalidAuthenticationType_FallsBackToWindows()
        {
            var conStr = "Server=foo;AuthenticationType=NotARealValue;UserName=x;Password=y";
            var sut = new SharepointSource(MakeXml(conStr));

            Assert.AreEqual(AuthenticationType.Windows, sut.AuthenticationType);
            Assert.AreEqual("foo", sut.Server);
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(SharepointSource))]
        public void Ctor_XElement_InvalidIsSharepointOnline_KeepsDefault()
        {
            // "not-a-bool" -> bool.TryParse fails -> IsSharepointOnline keeps its current value (false).
            var sut = new SharepointSource(
                MakeXml("Server=foo;AuthenticationType=Windows", isSharepointOnline: "not-a-bool"));

            Assert.IsFalse(sut.IsSharepointOnline);
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(SharepointSource))]
        public void Ctor_XElement_EmptyConnectionString_LeavesServerEmpty()
        {
            var sut = new SharepointSource(MakeXml(string.Empty));

            // No conString -> all the WFAES / base64 guards skipped, ParseProperties
            // gets "" and leaves the dictionary defaults in place.
            Assert.AreEqual(string.Empty, sut.Server);
            Assert.AreEqual(string.Empty, sut.UserName);
            Assert.AreEqual(string.Empty, sut.Password);
            Assert.AreEqual(AuthenticationType.Windows, sut.AuthenticationType);
        }

        // ------------------------------------------------------------------
        // XElement ctor: AES (WFAES::) encryption round-trip
        // ------------------------------------------------------------------

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(SharepointSource))]
        public void Ctor_XElement_WfaesEncryptedConnectionString_DecryptsAndParses()
        {
            var plain = "Server=secure-sp;AuthenticationType=User;UserName=carol;Password=p@ss";
            var encrypted = DpapiWrapper.Encrypt(plain);
            Assert.IsTrue(encrypted.StartsWith("WFAES::", StringComparison.Ordinal),
                "DpapiTestBase should install the AES encrypt hook.");

            var sut = new SharepointSource(MakeXml(encrypted));

            Assert.AreEqual("secure-sp", sut.Server);
            Assert.AreEqual("carol", sut.UserName);
            Assert.AreEqual("p@ss", sut.Password);
            Assert.AreEqual(AuthenticationType.User, sut.AuthenticationType);
        }

        // ------------------------------------------------------------------
        // XElement ctor: WFAES:: with the hook missing -> hard error
        // ------------------------------------------------------------------

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(SharepointSource))]
        public void Ctor_XElement_WfaesPrefix_NoDecryptHook_ThrowsInvalidOperation()
        {
            var savedHook = DpapiWrapper.AesDecryptHook;
            try
            {
                DpapiWrapper.AesDecryptHook = null;
                var ex = Assert.ThrowsException<InvalidOperationException>(() =>
                    new SharepointSource(MakeXml("WFAES::dGhpc2lzbm90cmVhbA==")));
                StringAssert.Contains(ex.Message, "WFAES::");
                StringAssert.Contains(ex.Message, "AesDecryptHook");
            }
            finally
            {
                DpapiWrapper.AesDecryptHook = savedHook;
            }
        }

        // ------------------------------------------------------------------
        // XElement ctor: WFAES:: with a payload the hook cannot decode
        // ------------------------------------------------------------------

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(SharepointSource))]
        public void Ctor_XElement_WfaesPrefix_CorruptPayload_ThrowsWrappedInvalidOperation()
        {
            // AesDecryptHook is registered (CanBeDecrypted returns true for any
            // WFAES:: payload), so the ctor reaches the try/Decrypt block and
            // the hook itself throws (bad base64 / bad cipher). The ctor
            // re-wraps that as InvalidOperationException carrying the
            // "WFAES::" prefix tag.
            var ex = Assert.ThrowsException<InvalidOperationException>(() =>
                new SharepointSource(MakeXml("WFAES::!!!not-base64!!!")));

            StringAssert.Contains(ex.Message, "Failed to decrypt SharepointSource ConnectionString");
            StringAssert.Contains(ex.Message, "prefix=WFAES::");
            Assert.IsNotNull(ex.InnerException);
        }

        // ------------------------------------------------------------------
        // XElement ctor: DPAPI-style (base64 only, no WFAES:: prefix)
        // can never be decrypted on Linux -> dedicated error
        // ------------------------------------------------------------------

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(SharepointSource))]
        public void Ctor_XElement_LooksLikeDpapiBase64ButCannotBeDecrypted_ThrowsClearError()
        {
            // 16 valid base64 chars (length % 4 == 0, no whitespace), no WFAES::
            // prefix, and the DPAPI unprotect can't decode it -> the dedicated
            // DPAPI-on-Linux branch must throw.
            var ex = Assert.ThrowsException<InvalidOperationException>(() =>
                new SharepointSource(MakeXml("QUJDREVGR0hJSktMTU5PUFFSU1RVVldYWVo=")));

            StringAssert.Contains(ex.Message, "DPAPI-encrypted");
            StringAssert.Contains(ex.Message, "AesDecryptHook");
        }

        // ------------------------------------------------------------------
        // Null-xml guard inherited from ResourceBase(xml)
        // ------------------------------------------------------------------

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(SharepointSource))]
        public void Ctor_XElement_NullXml_ThrowsArgumentNullException()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new SharepointSource((XElement)null));
        }
    }
}
