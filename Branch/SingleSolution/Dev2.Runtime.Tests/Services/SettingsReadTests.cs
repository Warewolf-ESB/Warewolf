using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using Dev2.Data.Settings;
using Dev2.DynamicServices;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Services.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

// ReSharper disable InconsistentNaming
namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class SettingsReadTests
    {

        #region ClassInitialize

        [ClassInitialize]
        public static void MyClassInitialize(TestContext context)
        {
        }

        #endregion

        #region Execute


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SettingsRead_Execute")]
        public void SettingsRead_Execute_WhenSecureConfigDoesExist_SettingsContainsSecurity()
        {
            //------------Setup for test--------------------------
            var permission = new WindowsGroupPermission { Administrator = true, IsServer = true, WindowsGroup = Environment.UserName };
            var permission2 = new WindowsGroupPermission { Administrator = false, DeployFrom = false, IsServer = true, WindowsGroup = "NETWORK SERVICE" };
            var windowsGroupPermissions = new List<WindowsGroupPermission> { permission, permission2 };
            var serializeObject = JsonConvert.SerializeObject(windowsGroupPermissions);
            var securityWrite = new SecurityWrite();
            securityWrite.Execute(new Dictionary<string, StringBuilder> { { "Permissions", new StringBuilder(serializeObject) } }, null);
            var settingsRead = new SettingsRead();
            //------------Assert Preconditions-------------------------
            Assert.IsTrue(File.Exists("secure.config"));
            //------------Execute Test---------------------------
            var jsonPermissions = settingsRead.Execute(null, null);
            File.Delete("secure.config");
            var settings = JsonConvert.DeserializeObject<Settings>(jsonPermissions.ToString());
            //------------Assert Results-------------------------
            Assert.IsNotNull(settings);
            Assert.IsNotNull(settings.Security);
            Assert.AreEqual(2, settings.Security.Count);
            Assert.AreEqual(Environment.UserName, settings.Security[0].WindowsGroup);
            Assert.AreEqual(true, settings.Security[0].IsServer);
            Assert.AreEqual(false, settings.Security[0].View);
            Assert.AreEqual(false, settings.Security[0].Execute);
            Assert.AreEqual(false, settings.Security[0].Contribute);
            Assert.AreEqual(false, settings.Security[0].DeployTo);
            Assert.AreEqual(false, settings.Security[0].DeployFrom);
            Assert.AreEqual(true, settings.Security[0].Administrator);
            Assert.AreEqual("NETWORK SERVICE", settings.Security[1].WindowsGroup);
            Assert.AreEqual(true, settings.Security[1].IsServer);
            Assert.AreEqual(false, settings.Security[1].View);
            Assert.AreEqual(false, settings.Security[1].Execute);
            Assert.AreEqual(false, settings.Security[1].Contribute);
            Assert.AreEqual(false, settings.Security[1].DeployTo);
            Assert.AreEqual(false, settings.Security[1].DeployFrom);
            Assert.AreEqual(false, settings.Security[1].Administrator);

        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SettingsRead_Execute")]
        public void SettingsRead_Execute_WhenBadSecurityData_SettingsErrorPopulated()
        {
            //------------Setup for test--------------------------
            File.WriteAllText("secure.config", @"some bogus data");
            var settingsRead = new SettingsRead();
            //------------Assert Preconditions-------------------------
            Assert.IsTrue(File.Exists("secure.config"));
            //------------Execute Test---------------------------
            var jsonPermissions = settingsRead.Execute(null, null);
            File.Delete("secure.config");
            var settings = JsonConvert.DeserializeObject<Settings>(jsonPermissions.ToString());
            //------------Assert Results-------------------------
            Assert.IsNotNull(settings);
            Assert.IsNull(settings.Security);
            Assert.IsTrue(settings.HasError);
            StringAssert.Contains(settings.Error, "Error reading settings configuration : ");
        }

        #endregion Exeute

        #region HandlesType

        [TestMethod]
        public void SettingsReadHandlesTypeExpectedReturnsSecurityWriteService()
        {
            var esb = new SettingsRead();
            var result = esb.HandlesType();
            Assert.AreEqual("SettingsReadService", result);
        }

        #endregion

        #region CreateServiceEntry

        [TestMethod]
        public void SettingsReadCreateServiceEntryExpectedReturnsDynamicService()
        {
            var esb = new SettingsRead();
            var result = esb.CreateServiceEntry();
            Assert.AreEqual(esb.HandlesType(), result.Name);
            Assert.AreEqual("<DataList><Settings/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>", result.DataListSpecification);
            Assert.AreEqual(1, result.Actions.Count);

            var serviceAction = result.Actions[0];
            Assert.AreEqual(esb.HandlesType(), serviceAction.Name);
            Assert.AreEqual(enActionType.InvokeManagementDynamicService, serviceAction.ActionType);
            Assert.AreEqual(esb.HandlesType(), serviceAction.SourceMethod);
        }

        #endregion

    }
}