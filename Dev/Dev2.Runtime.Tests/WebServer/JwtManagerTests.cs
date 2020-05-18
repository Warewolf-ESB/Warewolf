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
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Runtime.WebServer;
using Dev2.Services.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Data;

namespace Dev2.Tests.Runtime.WebServer
{
    [TestClass]
    [TestCategory(nameof(JwtManager))]
    public class JwtManagerTests
    {
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(JwtManager))]
        public void JwtManager_GenerateToken_ValidateToken()
        {
            //------------Setup for test-------------------------
            if(File.Exists(EnvironmentVariables.ServerSecuritySettingsFile))
            {
                File.Delete(EnvironmentVariables.ServerSecuritySettingsFile);
            }

            var overrideResource = new NamedGuid
            {
                Name = "Resourcename",
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
                    ResourceID = Guid.NewGuid(), ResourceName = "Category1\\Workflow1",
                    WindowsGroup = "Windows Group 1", View = false, Execute = true, Contribute = false
                }
            };
            var hmac = new HMACSHA256();
            var secretKey = Convert.ToBase64String(hmac.Key);
            var securitySettingsTO = new SecuritySettingsTO(permissions, overrideResource,secretKey);

            SecurityWrite.Write(securitySettingsTO);
            var payload = "<DataList><UserGroups Description='' IsEditable='True' ColumnIODirection='Output'><Name Description='' IsEditable='True' ColumnIODirection='Output'>public</Name></UserGroups></DataList>";
            //------------Execute Test---------------------------
            var encryptedPayload = JwtManager.GenerateToken(payload);
            var response = JwtManager.ValidateToken(encryptedPayload);
            //------------Assert Results-------------------------
            Assert.IsNotNull(encryptedPayload);
            Assert.AreEqual(payload, response);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(JwtManager))]
        public void JwtManager_GenerateToken_ValidateToken_Fails()
        {
            //------------Setup for test-------------------------
            //------------Execute Test---------------------------
            var response = JwtManager.ValidateToken( "321654");
            //------------Assert Results-------------------------
            Assert.IsNull(response);
        }
    }
}