
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Communication;
using Dev2.Data.Settings;
using Dev2.DynamicServices;
using Dev2.Runtime.ESB.Management;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Services.Security;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
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
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SettingsRead_Execute")]
        public void SettingsRead_Execute_SecurityReadDoesNotThrowException_HasErrorsIsFalseAndSecurityPermissionsAreAssigned()
        {
            //------------Setup for test--------------------------
            var securityPermissions = new List<WindowsGroupPermission>
            {
                new WindowsGroupPermission { IsServer = true, WindowsGroup = "TestGroup", Permissions = AuthorizationContext.DeployFrom.ToPermissions() },
                new WindowsGroupPermission { IsServer = true, WindowsGroup = "NETWORK SERVICE", Permissions = AuthorizationContext.DeployTo.ToPermissions() }
            };

            var securitySettingsTO = new SecuritySettingsTO(securityPermissions);

            var securityRead = new Func<IEsbManagementEndpoint>(() =>
            {
                var endpoint = new Mock<IEsbManagementEndpoint>();
                endpoint.Setup(e => e.Execute(It.IsAny<Dictionary<string, StringBuilder>>(), It.IsAny<IWorkspace>()))
                    .Returns(new Dev2JsonSerializer().SerializeToBuilder(securitySettingsTO));

                return endpoint.Object;
            });

            var settingsRead = new TestSettingsRead(securityRead);

            //------------Execute Test---------------------------
            var jsonPermissions = settingsRead.Execute(null, null);
            var settings = JsonConvert.DeserializeObject<Settings>(jsonPermissions.ToString());

            //------------Assert Results-------------------------
            Assert.IsNotNull(settings);
            Assert.IsNotNull(settings.Security);
            Assert.IsFalse(settings.HasError);

            var comparer = new WindowsGroupPermissionEqualityComparer();
            Assert.AreEqual(2, settings.Security.WindowsGroupPermissions.Count);

            for(var i = 0; i < securityPermissions.Count; i++)
            {
                var result = comparer.Equals(securityPermissions[i], settings.Security.WindowsGroupPermissions[i]);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SettingsRead_Execute")]
        public void SettingsRead_Execute_SecurityReadDoesThrowException_HasErrorsIsTrueAndDefaultPermissionsAreAssigned()
        {
            //------------Setup for test--------------------------
            const string invalidFormat = "Invalid format.";
            var securityRead = new Func<IEsbManagementEndpoint>(() =>
            {
                throw new JsonException(invalidFormat);
            });

            var settingsRead = new TestSettingsRead(securityRead);

            //------------Execute Test---------------------------
            var jsonPermissions = settingsRead.Execute(null, null);
            var settings = JsonConvert.DeserializeObject<Settings>(jsonPermissions.ToString());

            //------------Assert Results-------------------------
            Assert.IsNotNull(settings);
            Assert.IsNotNull(settings.Security);
            Assert.IsTrue(settings.HasError);
            StringAssert.Contains(settings.Error, invalidFormat);

            var expected = SecurityRead.DefaultPermissions[0];
            var actual = settings.Security.WindowsGroupPermissions[0];

            var result = new WindowsGroupPermissionEqualityComparer().Equals(expected, actual);
            Assert.IsTrue(result);
        }

        #endregion

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SettingsRead_CreateSecurityReadEndPoint")]
        public void SettingsRead_CreateSecurityReadEndPoint_IsInstanceOfSecurityRead()
        {
            //------------Setup for test--------------------------
            var securityRead = new Func<IEsbManagementEndpoint>(() => null);

            var settingsRead = new TestSettingsRead(securityRead);

            //------------Execute Test---------------------------
            var endpoint = settingsRead.TestCreateSecurityReadEndPoint();

            //------------Assert Results-------------------------
            Assert.IsNotNull(endpoint);
            Assert.IsInstanceOfType(endpoint, typeof(SecurityRead));
        }

        #region HandlesType

        [TestMethod]
        public void SettingsRead_HandlesType_ReturnsSettingsReadService()
        {
            var esb = new SettingsRead();
            var result = esb.HandlesType();
            Assert.AreEqual("SettingsReadService", result);
        }

        #endregion

        #region CreateServiceEntry

        [TestMethod]
        public void SettingsRead_CreateServiceEntry_ReturnsDynamicService()
        {
            var esb = new SettingsRead();
            var result = esb.CreateServiceEntry();
            Assert.AreEqual(esb.HandlesType(), result.Name);
            Assert.AreEqual("<DataList><Settings ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>", result.DataListSpecification);
            Assert.AreEqual(1, result.Actions.Count);

            var serviceAction = result.Actions[0];
            Assert.AreEqual(esb.HandlesType(), serviceAction.Name);
            Assert.AreEqual(enActionType.InvokeManagementDynamicService, serviceAction.ActionType);
            Assert.AreEqual(esb.HandlesType(), serviceAction.SourceMethod);
        }

        #endregion
    }
}
