using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.Data.PathOperations;
using Dev2.PathOperations;
using Moq;
using Warewolf.Resource.Errors;
using System;

namespace Dev2.Data.Tests.PathOperations
{
    [TestClass]
    public class LogonProviderTests
    {
        [TestMethod]
        public void LogonProvider_Construct()
        {
            var provider = new LogonProvider();

            var ioPath = new Dev2ActivityIOPath(Interfaces.Enums.enActivityIOPathType.FileSystem, @"C:\", @"DEV2\IntegrationTester", "I73573r0", false, null);
            provider.DoLogon(ioPath);
        }

        [TestMethod]
        public void LogonProvider_DoLogon_LogonInteractive()
        {
            bool loginReturnStatus = true;

            var mockLoginImpl = new Mock<ILoginApi>();

            var provider = new LogonProvider(mockLoginImpl.Object);

            var v = It.IsAny<SafeTokenHandle>();
            mockLoginImpl.Setup(o => o.LogonUser("IntegrationTester", "DEV2", "I73573r0", 2, 0, out v))
                .Returns(loginReturnStatus);


            var ioPath = new Dev2ActivityIOPath(Interfaces.Enums.enActivityIOPathType.FileSystem, @"C:\", @"DEV2\IntegrationTester", "I73573r0", false, null);
            provider.DoLogon(ioPath);

            mockLoginImpl.Verify(o => o.LogonUser("IntegrationTester", "DEV2", "I73573r0", 2, 0, out v), Times.Once);
        }

        [TestMethod]
        public void LogonProvider_DoLogon_LogonNetwork()
        {
            bool loginReturnStatus = true;

            var mockLoginImpl = new Mock<ILoginApi>();

            var provider = new LogonProvider(mockLoginImpl.Object);

            var v = It.IsAny<SafeTokenHandle>();
            mockLoginImpl.Setup(o => o.LogonUser("IntegrationTester", "DEV2", "I73573r0", 3, 3, out v))
                .Returns(loginReturnStatus);


            var ioPath = new Dev2ActivityIOPath(Interfaces.Enums.enActivityIOPathType.FileSystem, @"C:\", @"DEV2\IntegrationTester", "I73573r0", false, null);
            provider.DoLogon(ioPath);

            mockLoginImpl.Verify(o => o.LogonUser("IntegrationTester", "DEV2", "I73573r0", 3, 3, out v), Times.Once);
        }

        [TestMethod]
        public void LogonProvider_DoLogon_ErrorThrowsMessage()
        {
            bool loginReturnStatus = true;

            var mockLoginImpl = new Mock<ILoginApi>();

            var provider = new LogonProvider(mockLoginImpl.Object);

            var v = It.IsAny<SafeTokenHandle>();
            mockLoginImpl.Setup(o => o.LogonUser("IntegrationTester", "DEV2", "I73573r0", 3, 3, out v))
                .Throws(new Exception("some exception"));


            var ioPath = new Dev2ActivityIOPath(Interfaces.Enums.enActivityIOPathType.FileSystem, @"C:\", @"DEV2\IntegrationTester", "I73573r0", false, null);
            var expectedMessage = string.Format(ErrorResource.FailedToAuthenticateUser, "IntegrationTester", ioPath.Path);

            var hadException = false;
            try
            {
                provider.DoLogon(ioPath);
            } catch (Exception e)
            {
                hadException = true;
                Assert.AreEqual(expectedMessage, e.Message);
            }
            Assert.IsTrue(hadException, "expected exception");

            mockLoginImpl.Verify(o => o.LogonUser("IntegrationTester", "DEV2", "I73573r0", 3, 3, out v), Times.Once);
        }
    }
}
