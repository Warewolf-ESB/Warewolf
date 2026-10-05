/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.IO;
using System.Security.Cryptography;
using Dev2.Common;
using Dev2.Common.Interfaces.Data;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Runtime.Interfaces;
using Dev2.Services.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Data;
using Warewolf.Security;
using Warewolf.Services;

namespace Dev2.Tests.Runtime.WebServer
{
    [TestClass]
    [TestCategory(nameof(JwtManager))]
    public class JwtManagerTests
    {
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(JwtManager))]
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
        public void JwtManager_GenerateToken_ValidateToken()
        {
            //------------Setup for test-------------------------
            if(File.Exists(EnvironmentVariables.ServerSecuritySettingsFile))
            {
                File.Delete(EnvironmentVariables.ServerSecuritySettingsFile);
            }
            var resourceId = Guid.NewGuid();
            var resourceName = @"Hello World";

            var overrideResource = new NamedGuid
            {
                Name = "appAuth",
                Value = Guid.NewGuid()
            };
            var permissions = new[]
            {
                new WindowsGroupPermission
                {
                    IsServer = true, WindowsGroup = "Deploy Admins",
                    View = false, Execute = false, Contribute = false, DeployTo = true, DeployFrom = true, Administrator = false
                },

                new WindowsGroupPermission
                {
                    ResourceID = resourceId,
                    ResourceName = "Category1\\Workflow1",
                    WindowsGroup = "Public",
                    View = true,
                    Execute = true,
                    Contribute = false
                }
            };
            var hmac = new HMACSHA256();
            var secretKey = Convert.ToBase64String(hmac.Key);
            var securitySettingsTO = new SecuritySettingsTO(permissions, overrideResource,secretKey);
            SecurityWrite.Write(securitySettingsTO);
            var payload = "<DataList><UserGroups Description='' IsEditable='True' ColumnIODirection='Output'><Name Description='' IsEditable='True' ColumnIODirection='Output'>public</Name></UserGroups></DataList>";

            // Injected rather than registered in CustomContainer: the default ResourceNameProvider
            // resolves ResourceCatalog.Instance, which would cache a registered mock catalog for
            // the rest of the test run (CI build 30903).
            var mockResourceNameProvider = new Mock<IResourceNameProvider>();
            mockResourceNameProvider.Setup(a => a.GetResourceNameById(It.IsAny<Guid>())).Returns(resourceName);

            //------------Execute Test---------------------------
            var securitySettings = new SecuritySettings();
            var jwtManager = new JwtManager(securitySettings, mockResourceNameProvider.Object);
            var encryptedPayload = jwtManager.GenerateToken(payload);
            var response = jwtManager.ValidateToken(encryptedPayload);
            //------------Assert Results-------------------------
            Assert.IsNotNull(encryptedPayload);
            Assert.AreEqual(payload, response);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(JwtManager))]
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
        public void JwtManager_GenerateToken_ValidateToken_Fails()
        {
            //------------Setup for test-------------------------
            var securitySettings = new SecuritySettings();
            var jwtManager = new JwtManager(securitySettings, new Mock<IResourceNameProvider>().Object);

            //------------Execute Test---------------------------
            var response = jwtManager.ValidateToken( "321654");
            //------------Assert Results-------------------------
            Assert.IsNull(response);
        }
    }
}