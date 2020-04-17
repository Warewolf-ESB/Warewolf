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
using System.Collections.Generic;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Enums;
using Dev2.Communication;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Configuration;
using Warewolf.Data;

namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    public class SaveAuditingSettingsTests
    {
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(SaveAuditingSettings))]
        public void SaveAuditingSettings_Execute_nullValues_Fails()
        {
            //------------Setup for test--------------------------
            var serializer = new Dev2JsonSerializer();

            var saveAuditingSettings = new SaveAuditingSettings();
            //------------Execute Test---------------------------
            var jsonResult = saveAuditingSettings.Execute(null, null);
            var result = serializer.Deserialize<ExecuteMessage>(jsonResult);
            //------------Assert Results-------------------------
            Assert.IsTrue(result.HasError);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(SaveAuditingSettings))]
        public void SaveAuditingSettings_GetResourceID_ShouldReturnEmptyGuid()
        {
            //------------Setup for test--------------------------
            var saveAuditingSettings = new SaveAuditingSettings();
            //------------Execute Test---------------------------
            var resId = saveAuditingSettings.GetResourceID(new Dictionary<string, StringBuilder>());
            //------------Assert Results-------------------------
            Assert.AreEqual(Guid.Empty, resId);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(SaveAuditingSettings))]
        public void SaveAuditingSettings_GetAuthorizationContextForService_ShouldReturnContext()
        {
            //------------Setup for test--------------------------
            var saveAuditingSettings = new SaveAuditingSettings();
            //------------Execute Test---------------------------
            var resId = saveAuditingSettings.GetAuthorizationContextForService();
            //------------Assert Results-------------------------
            Assert.AreEqual(AuthorizationContext.Contribute, resId);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(SaveAuditingSettings))]
        public void SaveAuditingSettings_CreateServiceEntry_ExpectActions()
        {
            //------------Setup for test--------------------------
            var saveAuditingSettings = new SaveAuditingSettings();
            //------------Execute Test---------------------------
            var dynamicService = saveAuditingSettings.CreateServiceEntry();
            //------------Assert Results-------------------------
            Assert.IsNotNull(dynamicService);
            Assert.IsNotNull(dynamicService.Actions);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(SaveAuditingSettings))]
        public void SaveAuditingSettings_Execute_LegacySettingsData()
        {
            //------------Setup for test--------------------------
            var serializer = new Dev2JsonSerializer();
            var workspaceMock = new Mock<IWorkspace>();
            var settingsData = new LegacySettingsData()
            {
                AuditFilePath = @"C:\ProgramData\Warewolf\Audits",
            };
            var requestArgs = new Dictionary<string, StringBuilder>();
            requestArgs.Add("AuditingSettings", new StringBuilder(serializer.SerializeToBuilder(settingsData).ToString()));
            requestArgs.Add("SinkType", new StringBuilder("LegacySettingsData"));

            var saveAuditingSettings = new SaveAuditingSettings();
            //------------Execute Test---------------------------
            var jsonResult = saveAuditingSettings.Execute(requestArgs, workspaceMock.Object);
            var result = serializer.Deserialize<ExecuteMessage>(jsonResult);
            //------------Assert Results-------------------------
            Assert.IsFalse(result.HasError);
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(SaveAuditingSettings))]
        public void SaveAuditingSettings_Execute_AuditingSettingsData()
        {
            //------------Setup for test--------------------------
            var serializer = new Dev2JsonSerializer();
            var workspaceMock = new Mock<IWorkspace>();
            var settingsData = new AuditingSettingsData()
            {
                LoggingDataSource = new NamedGuidWithEncryptedPayload(),
            };
            var requestArgs = new Dictionary<string, StringBuilder>();
            requestArgs.Add("AuditingSettings", new StringBuilder(serializer.SerializeToBuilder(settingsData).ToString()));
            requestArgs.Add("SinkType", new StringBuilder("AuditingSettingsData"));

            var saveAuditingSettings = new SaveAuditingSettings();
            //------------Execute Test---------------------------
            var jsonResult = saveAuditingSettings.Execute(requestArgs, workspaceMock.Object);
            var result = serializer.Deserialize<ExecuteMessage>(jsonResult);
            //------------Assert Results-------------------------
            Assert.IsFalse(result.HasError);
        }
    }
}
