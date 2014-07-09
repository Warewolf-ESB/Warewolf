using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Tu.Imports;
using Tu.Servers;
using Tu.Washing;

namespace Tu.Server.Tests
{
    public partial class WashingMachineTests
    {
        [TestMethod]
        [TestCategory("WashingMachine_OnDispose")]
        [Description("WashingMachine OnDispose disposes child objects.")]
        [Owner("Trevor Williams-Ros")]
        public void WashingMachine_UnitTest_OnDispose_Disposes()
        {
            var sqlServer = new Mock<ISqlServer>();
            sqlServer.Setup(s => s.Dispose()).Verifiable();

            var emailServer = new Mock<IEmailServer>();
            emailServer.Setup(s => s.Dispose()).Verifiable();

            var machine = new WashingMachine(new Mock<IImportProcessor>().Object, sqlServer.Object, emailServer.Object, new Mock<IFtpServer>().Object, new Mock<IFileServer>().Object, new Mock<IFileServer>().Object);
            machine.Dispose();

            sqlServer.Verify(s => s.Dispose());
            emailServer.Verify(s => s.Dispose());
        }
    }
}
