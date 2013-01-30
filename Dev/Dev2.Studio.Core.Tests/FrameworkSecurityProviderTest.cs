using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Framework;
using Dev2.Studio.Core;
using Moq;
using System.ComponentModel.Composition.Hosting;
using System.Reflection;
using Dev2.Core.Tests.ProperMoqs;
using System.ComponentModel.Composition;
using Dev2.Studio.Core.ViewModels;
using System.Diagnostics;
using System.Threading;
using System.ComponentModel.Composition.Primitives;
using Dev2.Composition;

namespace Dev2.Core.Tests {

    /// <summary>
    /// Summary description for FrameworkSecurityProviderTest
    /// </summary>
    [TestClass]
    public class FrameworkSecurityProviderTest 
    {
        private TestContext testContextInstance;
        

        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            
        }

        [TestInitialize()]
        public void TestInitialize()
        {
            ImportService.CurrentContext = CompositionInitializer.InitializeForFrameworkSecurityProviderTests(); 
        }

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext {
            get {
                return testContextInstance;
            }
            set {
                testContextInstance = value;
            }
        }

        #region LDAP Tests

        [TestMethod]
        public void ValidLDAP_ExpectValidRoles() {

            string[] key = { "Dev2StudioSecurityMode", "Dev2StudioLDAPEndpoint" };
            string[] val = { "LDAP", "LDAP://dev2.local" };

            FrameworkSecurityProvider s = new FrameworkSecurityProvider();
            ImportService.SatisfyImports(s);
            // boot strap the config reader
            (s.ConfigProvider as MoqConfigurationReader).Init(key, val);

            string[] roles = s.AllRoles;

            Assert.IsTrue(roles != null);
        }

        [TestMethod]
        public void InvalidLDAP_ExpectNullRoles() {
            
            string[] key = { "Dev2StudioSecurityMode", "Dev2StudioLDAPEndpoint" };
            string[] val = { "LDAP", "LDAP://dev2.local2" };

            FrameworkSecurityProvider s = new FrameworkSecurityProvider();
            ImportService.SatisfyImports(s);
            // boot strap the config reader
            (s.ConfigProvider as MoqConfigurationReader).Init(key, val);

            Assert.IsTrue(s.AllRoles == null);
        }

        #endregion LDAP Tests

        #region Authentication Options Tests

        [TestMethod]
        public void InvalidAuthOption_ExpectNullRoles() {

            string[] key = { "Dev2StudioSecurityMode" };
            string[] val = { "Invalid"};

            FrameworkSecurityProvider s = new FrameworkSecurityProvider();
            ImportService.SatisfyImports(s);
            // boot strap the config reader
            (s.ConfigProvider as MoqConfigurationReader).Init(key, val);

            Assert.IsTrue(s.AllRoles == null);
        }

        #endregion Authentication Options Tests

        #region Dictionary Key Tests

        [TestMethod]
        public void InvalidKey_ExpectNullRoles() {

            string[] key = { "Dev2StudioSecurityMode2" };
            string[] val = { "Offline"};

            FrameworkSecurityProvider s = new FrameworkSecurityProvider();
            ImportService.SatisfyImports(s);
            // boot strap the config reader
            (s.ConfigProvider as MoqConfigurationReader).Init(key, val);

            Assert.IsTrue(s.AllRoles == null);
        }

        [TestMethod] 
        public void ValidOffline_ExpectValidRoles() {
            
            string[] key = { "Dev2StudioSecurityMode" };
            string[] val = { "Offline" };

            FrameworkSecurityProvider s = new FrameworkSecurityProvider();
            ImportService.SatisfyImports(s);
            // boot strap the config reader
            (s.ConfigProvider as MoqConfigurationReader).Init(key, val);

            string[] roles = s.AllRoles;

            Assert.IsTrue(roles != null);
        }

        #endregion Dictionary Key Tests
    }
}
