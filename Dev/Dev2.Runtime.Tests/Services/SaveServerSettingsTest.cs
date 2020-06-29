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
using Dev2.Data.Interfaces.Enums;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Configuration;

namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    public class SaveServerSettingsTests
    {
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(SaveServerSettings))]
        public void SaveServerSettings_Execute_NullValues_ErrorResult()
        {
            //------------Setup for test--------------------------
            var serializer = new Dev2JsonSerializer();
            var saveServerSettings = new SaveServerSettings();
            //------------Execute Test---------------------------
            var jsonResult = saveServerSettings.Execute(null, null);
            var result = serializer.Deserialize<ExecuteMessage>(jsonResult);
            //------------Assert Results-------------------------
            Assert.IsTrue(result.HasError);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(SaveServerSettings))]
        public void SaveServerSettings_GetResourceID_ShouldReturnEmptyGuid()
        {
            //------------Setup for test--------------------------
            var saveServerSettings = new SaveServerSettings();
            //------------Execute Test---------------------------
            var resId = saveServerSettings.GetResourceID(new Dictionary<string, StringBuilder>());
            //------------Assert Results-------------------------
            Assert.AreEqual(Guid.Empty, resId);
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(SaveServerSettings))]
        public void SaveServerSettings_GetAuthorizationContextForService_ShouldReturnContext()
        {
            //------------Setup for test--------------------------
            var saveServerSettings = new SaveServerSettings();
            //------------Execute Test---------------------------
            var resId = saveServerSettings.GetAuthorizationContextForService();
            //------------Assert Results-------------------------
            Assert.AreEqual(AuthorizationContext.Contribute, resId);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(SaveServerSettings))]
        public void SaveServerSettings_CreateServiceEntry_ExpectActions()
        {
            //------------Setup for test--------------------------
            var saveServerSettings = new SaveServerSettings();
            //------------Execute Test---------------------------
            var dynamicService = saveServerSettings.CreateServiceEntry();
            //------------Assert Results-------------------------
            Assert.IsNotNull(dynamicService);
            Assert.IsNotNull(dynamicService.Actions);
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(SaveServerSettings))]
        public void SaveServerSettings_Execute_SaveServerSettingsData()
        {
            //------------Setup for test--------------------------
            var serializer = new Dev2JsonSerializer();
            var workspaceMock = new Mock<IWorkspace>();
            var settingsData = new ServerSettingsData()
            {
                ExecutionLogLevel = LogLevel.DEBUG.ToString()
            };
            var requestArgs = new Dictionary<string, StringBuilder>();
            requestArgs.Add("ServerSettings", new StringBuilder(serializer.SerializeToBuilder(settingsData).ToString()));

            var saveServerSettings = new SaveServerSettings();
            //------------Execute Test---------------------------
            var jsonResult = saveServerSettings.Execute(requestArgs, workspaceMock.Object);
            var result = serializer.Deserialize<ExecuteMessage>(jsonResult);
            //------------Assert Results-------------------------
            Assert.IsFalse(result.HasError);
        }
    }
}