/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*/

using System;
using System.Xml.Linq;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Data.ServiceModel;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.UnitTestUtils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Data.Tests.ServiceModel
{
    /// <summary>
    /// Coverage for <see cref="Connection"/> ctor
    /// (Crap Score 156, Cyclomatic Complexity 12).
    ///
    /// The XElement ctor decrypts the ConnectionString (when DPAPI-encrypted)
    /// and parses each <c>key=value;</c> pair. Each switch case (and its
    /// fall-back parsing branch) needs explicit coverage.
    /// </summary>
    [TestClass]
    public class ConnectionCtorTests : DpapiTestBase
    {
        private static XElement BuildSourceXml(string connectionString) =>
            new XElement("Source",
                new XAttribute("ID", Guid.NewGuid()),
                new XAttribute("Name", "test conn"),
                new XAttribute("ResourceType", "Server"),
                new XAttribute("ConnectionString", connectionString),
                new XElement("DisplayName", "test conn"),
                new XElement("Category", "TEST"));

        // -----------------------------------------------------------------
        // Default ctor
        // -----------------------------------------------------------------

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(Connection))]
        public void Ctor_Default_SetsServerResourceTypeAndVersionInfo()
        {
            var c = new Connection();

            Assert.AreEqual(enSourceType.Dev2Server.ToString(), c.ResourceType);
            Assert.IsNotNull(c.VersionInfo);
            Assert.IsTrue(c.IsServer);
            Assert.IsFalse(c.IsSource);
            Assert.IsFalse(c.IsService);
            Assert.IsFalse(c.IsFolder);
            Assert.IsFalse(c.IsReservedService);
            Assert.IsFalse(c.IsResourceVersion);
        }

        // -----------------------------------------------------------------
        // XElement ctor parsing branches
        // -----------------------------------------------------------------

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(Connection))]
        public void Ctor_AllConnectionStringKeys_PopulatesEveryProperty()
        {
            var xml = BuildSourceXml(
                "AppServerUri=http://srv:3142/dsf;WebServerPort=3142;AuthenticationType=User;UserName=alice;Password=s3cret");

            var c = new Connection(xml);

            Assert.AreEqual("http://srv:3142/dsf", c.Address);
            Assert.AreEqual(3142, c.WebServerPort);
            Assert.AreEqual(AuthenticationType.User, c.AuthenticationType);
            Assert.AreEqual("alice", c.UserName);
            Assert.AreEqual("s3cret", c.Password);
            Assert.AreEqual(enSourceType.Dev2Server.ToString(), c.ResourceType);
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(Connection))]
        public void Ctor_InvalidPort_FallsBackToDefaultWebServerPort()
        {
            // Int32.TryParse fails ⇒ uses DefaultWebServerPort (3142).
            var xml = BuildSourceXml("AppServerUri=http://srv;WebServerPort=NotANumber");

            var c = new Connection(xml);

            Assert.AreEqual(3142, c.WebServerPort);
            Assert.AreEqual("http://srv", c.Address);
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(Connection))]
        public void Ctor_InvalidAuthenticationType_FallsBackToWindows()
        {
            // Enum.TryParse fails ⇒ AuthenticationType.Windows.
            var xml = BuildSourceXml("AppServerUri=http://srv;AuthenticationType=NotARealAuth");

            var c = new Connection(xml);

            Assert.AreEqual(AuthenticationType.Windows, c.AuthenticationType);
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(Connection))]
        public void Ctor_AuthenticationTypePublic_ParsesCaseInsensitively()
        {
            var xml = BuildSourceXml("AppServerUri=http://srv;authenticationtype=public");

            var c = new Connection(xml);

            Assert.AreEqual(AuthenticationType.Public, c.AuthenticationType);
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(Connection))]
        public void Ctor_UnknownConnectionStringKey_IgnoredSilently()
        {
            // Unknown keys hit the implicit-default switch branch.
            var xml = BuildSourceXml("AppServerUri=http://srv;BogusKey=somevalue");

            var c = new Connection(xml);

            Assert.AreEqual("http://srv", c.Address);
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(Connection))]
        public void Ctor_EncryptedConnectionString_DecryptsTransparently()
        {
            var encrypted = Warewolf.Security.Encryption.DpapiWrapper.Encrypt(
                "AppServerUri=http://encrypted/dsf;WebServerPort=4242;AuthenticationType=Windows");
            var xml = BuildSourceXml(encrypted);

            var c = new Connection(xml);

            Assert.AreEqual("http://encrypted/dsf", c.Address);
            Assert.AreEqual(4242, c.WebServerPort);
            Assert.AreEqual(AuthenticationType.Windows, c.AuthenticationType);
        }

        // -----------------------------------------------------------------
        // FetchTestConnectionAddress
        // -----------------------------------------------------------------

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(Connection))]
        public void FetchTestConnectionAddress_AddressWithoutDsf_AppendsDsfWithSlash()
        {
            var c = new Connection { Address = "http://srv:3142" };

            Assert.AreEqual("http://srv:3142/dsf", c.FetchTestConnectionAddress());
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(Connection))]
        public void FetchTestConnectionAddress_AddressEndingInSlash_AppendsDsf()
        {
            var c = new Connection { Address = "http://srv:3142/" };

            Assert.AreEqual("http://srv:3142/dsf", c.FetchTestConnectionAddress());
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(Connection))]
        public void FetchTestConnectionAddress_AddressAlreadyContainsDsf_LeftUnchanged()
        {
            var c = new Connection { Address = "http://srv:3142/dsf" };

            Assert.AreEqual("http://srv:3142/dsf", c.FetchTestConnectionAddress());
        }

        // -----------------------------------------------------------------
        // ToXml round-trip
        // -----------------------------------------------------------------

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(Connection))]
        public void ToXml_WindowsAuth_OmitsCredentials()
        {
            var c = new Connection
            {
                ResourceID = Guid.NewGuid(),
                ResourceName = "test",
                Address = "http://srv",
                WebServerPort = 3142,
                AuthenticationType = AuthenticationType.Windows,
                UserName = "ignored",
                Password = "ignored",
            };

            var xml = c.ToXml();
            var conn = (string)xml.Attribute("ConnectionString")!;
            var decrypted = Warewolf.Security.Encryption.DpapiWrapper.DecryptIfEncrypted(conn);

            StringAssert.Contains(decrypted, "AppServerUri=http://srv");
            StringAssert.Contains(decrypted, "WebServerPort=3142");
            StringAssert.Contains(decrypted, "AuthenticationType=Windows");
            Assert.IsFalse(decrypted.Contains("UserName="));
            Assert.IsFalse(decrypted.Contains("Password="));
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(Connection))]
        public void RoundTrip_ToXml_ThenXmlCtor_PreservesAllProperties()
        {
            var original = new Connection
            {
                ResourceID = Guid.NewGuid(),
                ResourceName = "round",
                Address = "http://rt:3142",
                WebServerPort = 3142,
                AuthenticationType = AuthenticationType.User,
                UserName = "rt-user",
                Password = "rt-pass",
            };

            var rehydrated = new Connection(original.ToXml());

            Assert.AreEqual(original.Address, rehydrated.Address);
            Assert.AreEqual(original.WebServerPort, rehydrated.WebServerPort);
            Assert.AreEqual(original.AuthenticationType, rehydrated.AuthenticationType);
            Assert.AreEqual(original.UserName, rehydrated.UserName);
            Assert.AreEqual(original.Password, rehydrated.Password);
            Assert.IsTrue(original.Equals(rehydrated));
            Assert.AreEqual(original.GetHashCode(), rehydrated.GetHashCode());
        }

        // -----------------------------------------------------------------
        // Equals / GetHashCode
        // -----------------------------------------------------------------

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(Connection))]
        public void Equals_NonConnectionObject_ReturnsFalse()
        {
            var a = new Connection();

            Assert.IsFalse(a.Equals("not a connection"));
        }

        // -----------------------------------------------------------------
        // WebAddress
        // -----------------------------------------------------------------

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(Connection))]
        public void WebAddress_BuildsUriWithExplicitPort()
        {
            var c = new Connection { Address = "http://srv:3142/dsf", WebServerPort = 8080 };

            StringAssert.Contains(c.WebAddress, ":8080");
            StringAssert.Contains(c.WebAddress, "srv");
        }
    }
}
