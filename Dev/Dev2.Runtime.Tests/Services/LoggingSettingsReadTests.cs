/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using System.Text;
using Dev2.Communication;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Services.Security;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    public class LoggingSettingsReadTests
    {
        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("LoggingSettingsRead")]
        public void LoggingSettingsRead_HandlesType_ReturnsServiceName()
        {
            var service = new LoggingSettingsRead();

            Assert.AreEqual("LoggingSettingsReadService", service.HandlesType());
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("LoggingSettingsRead")]
        public void LoggingSettingsRead_CreateServiceEntry_ReturnsServiceNamedAfterHandlesType()
        {
            var service = new LoggingSettingsRead();

            var dynamicService = service.CreateServiceEntry();

            Assert.IsNotNull(dynamicService);
            Assert.AreEqual(service.HandlesType(), dynamicService.Name);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("LoggingSettingsRead")]
        public void LoggingSettingsRead_Execute_ReturnsSerializedLoggingSettings()
        {
            var service = new LoggingSettingsRead();
            var ws = new Mock<IWorkspace>();

            var result = service.Execute(new Dictionary<string, StringBuilder>(), ws.Object);

            Assert.IsNotNull(result);
            var settings = new Dev2JsonSerializer().Deserialize<LoggingSettingsTo>(result);
            Assert.IsNotNull(settings);
        }
    }
}
