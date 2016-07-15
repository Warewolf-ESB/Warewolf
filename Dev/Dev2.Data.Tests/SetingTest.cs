using System;
using System.Collections.Generic;
using System.Text;
using Dev2.Common.Interfaces.Monitoring;
using Dev2.Communication;
using Dev2.PerformanceCounters.Management;
using Dev2.Runtime.ESB.Management;
using Dev2.Services.Security;
using Dev2.Tests.Runtime.Services;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Data.Tests
{
    [TestClass]
    public class SetingTest
    {
        [TestMethod]
        public void Settings_ShouldAddSecurity()
        {            
            var serializer = new Dev2JsonSerializer();
            var securityPermissions = new List<WindowsGroupPermission>
            {
                new WindowsGroupPermission { IsServer = true, WindowsGroup = "TestGroup", Permissions = AuthorizationContext.DeployFrom.ToPermissions() },
                new WindowsGroupPermission { IsServer = true, WindowsGroup = "NETWORK SERVICE", Permissions = AuthorizationContext.DeployTo.ToPermissions() }
            };

            var securitySettingsTo = new SecuritySettingsTO(securityPermissions);
            var securityRead = new Func<IEsbManagementEndpoint>(() =>
            {
                var endpoint = new Mock<IEsbManagementEndpoint>();
                endpoint.Setup(e => e.Execute(It.IsAny<Dictionary<string, StringBuilder>>(), It.IsAny<IWorkspace>()))
                    .Returns(new Dev2JsonSerializer().SerializeToBuilder(securitySettingsTo));

                return endpoint.Object;
            });
            var mockPerfCounterRepo = new Mock<IPerformanceCounterRepository>();
            mockPerfCounterRepo.Setup(repository => repository.Counters).Returns(new PerformanceCounterTo());

            var settingsRead = new TestSettingsRead(securityRead);
            var jsonPermissions = settingsRead.Execute(null, null);
            var settings = serializer.Deserialize<Settings.Settings>(jsonPermissions.ToString());

            var serialized = settings.ToString();
            Assert.IsNotNull(settings);
            Assert.IsNotNull(settings.Security);
            Assert.AreEqual(2, settings.Security.WindowsGroupPermissions.Count);
        }
    }
}
