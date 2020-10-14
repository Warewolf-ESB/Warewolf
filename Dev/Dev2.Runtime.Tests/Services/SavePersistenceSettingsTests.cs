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
    public class SavePersistenceSettingsTests
    {
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(SavePersistenceSettings))]
        public void SavePersistenceSettings_Execute_nullValues_Fails()
        {
            //------------Setup for test--------------------------
            var serializer = new Dev2JsonSerializer();
            var savePersistenceSettings = new SavePersistenceSettings();
            //------------Execute Test---------------------------
            var jsonResult = savePersistenceSettings.Execute(null, null);
            var result = serializer.Deserialize<ExecuteMessage>(jsonResult);
            //------------Assert Results-------------------------
            Assert.IsTrue(result.HasError);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(SavePersistenceSettings))]
        public void SavePersistenceSettings_GetResourceID_ShouldReturnEmptyGuid()
        {
            //------------Setup for test--------------------------
            var savePersistenceSettings = new SavePersistenceSettings();
            //------------Execute Test---------------------------
            var resId = savePersistenceSettings.GetResourceID(new Dictionary<string, StringBuilder>());
            //------------Assert Results-------------------------
            Assert.AreEqual(Guid.Empty, resId);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(SavePersistenceSettings))]
        public void SavePersistenceSettings_GetAuthorizationContextForService_ShouldReturnContext()
        {
            //------------Setup for test--------------------------
            var savePersistenceSettings = new SavePersistenceSettings();
            //------------Execute Test---------------------------
            var resId = savePersistenceSettings.GetAuthorizationContextForService();
            //------------Assert Results-------------------------
            Assert.AreEqual(AuthorizationContext.Contribute, resId);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(SavePersistenceSettings))]
        public void SavePersistenceSettings_CreateServiceEntry_ExpectActions()
        {
            //------------Setup for test--------------------------
            var savePersistenceSettings = new SavePersistenceSettings();
            //------------Execute Test---------------------------
            var dynamicService = savePersistenceSettings.CreateServiceEntry();
            //------------Assert Results-------------------------
            Assert.IsNotNull(dynamicService);
            Assert.IsNotNull(dynamicService.Actions);
        }


        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(SavePersistenceSettings))]
        public void SavePersistenceSettings_Execute()
        {
            //------------Setup for test--------------------------
            var serializer = new Dev2JsonSerializer();
            var workspaceMock = new Mock<IWorkspace>();
            var settingsData = new PersistenceSettingsData
            {
                PersistenceScheduler = "Hangfire",
                Enable = true,
                PersistenceDataSource = new NamedGuidWithEncryptedPayload
                {
                    Name = "Data Source",
                    Value = Guid.Empty,
                    Payload = "foo"
                },
                EncryptDataSource = true,
                DashboardHostname = "DashboardHostname",
                DashboardName = "Dashboardname",
                DashboardPort = "5001",
                PrepareSchemaIfNecessary = true,
                ServerName = "servername"
            };
            var requestArgs = new Dictionary<string, StringBuilder>();
            requestArgs.Add("PersistenceSettings", new StringBuilder(serializer.SerializeToBuilder(settingsData).ToString()));

            var savePersistenceSettings = new SavePersistenceSettings();
            //------------Execute Test---------------------------
            var jsonResult = savePersistenceSettings.Execute(requestArgs, workspaceMock.Object);
            var result = serializer.Deserialize<ExecuteMessage>(jsonResult);
            //------------Assert Results-------------------------
            Assert.IsFalse(result.HasError);
        }
    }
}
