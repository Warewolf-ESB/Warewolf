using System;
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
        [TestCategory("WashingMachine_Constructor")]
        [Description("WashingMachine Constructor with null ImportProcessor throws ArgumentNullException.")]
        [Owner("Trevor Williams-Ros")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WashingMachine_UnitTest_ConstructorWithNullImportProcessor_ThrowsArgumentNullException()
        {
            var machine = new WashingMachine(null, null, null, null, null, null);
        }

        [TestMethod]
        [TestCategory("WashingMachine_Constructor")]
        [Description("WashingMachine Constructor with null SqlServer throws ArgumentNullException.")]
        [Owner("Trevor Williams-Ros")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WashingMachine_UnitTest_ConstructorWithNullSqlServer_ThrowsArgumentNullException()
        {
            var machine = new WashingMachine(new Mock<IImportProcessor>().Object, null, null, null, null, null);
        }

        [TestMethod]
        [TestCategory("WashingMachine_Constructor")]
        [Description("WashingMachine Constructor with null EmailClient throws ArgumentNullException.")]
        [Owner("Trevor Williams-Ros")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WashingMachine_UnitTest_ConstructorWithNullEmailClient_ThrowsArgumentNullException()
        {
            var machine = new WashingMachine(new Mock<IImportProcessor>().Object, new Mock<ISqlServer>().Object, null, null, null, null);
        }

        [TestMethod]
        [TestCategory("WashingMachine_Constructor")]
        [Description("WashingMachine Constructor with null FtpServer throws ArgumentNullException.")]
        [Owner("Trevor Williams-Ros")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WashingMachine_UnitTest_ConstructorWithNullFtpServer_ThrowsArgumentNullException()
        {
            var machine = new WashingMachine(new Mock<IImportProcessor>().Object, new Mock<ISqlServer>().Object, new Mock<IEmailServer>().Object, null, null, null);
        }

        [TestMethod]
        [TestCategory("WashingMachine_Constructor")]
        [Description("WashingMachine Constructor with null errors FileServer throws ArgumentNullException.")]
        [Owner("Trevor Williams-Ros")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WashingMachine_UnitTest_ConstructorWithNullErrorsFileServer_ThrowsArgumentNullException()
        {
            var machine = new WashingMachine(new Mock<IImportProcessor>().Object, new Mock<ISqlServer>().Object, new Mock<IEmailServer>().Object, new Mock<IFtpServer>().Object, null, null);
        }

        [TestMethod]
        [TestCategory("WashingMachine_Constructor")]
        [Description("WashingMachine Constructor with null local FileServer throws ArgumentNullException.")]
        [Owner("Trevor Williams-Ros")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WashingMachine_UnitTest_ConstructorWithNullLocalFileServer_ThrowsArgumentNullException()
        {
            var machine = new WashingMachine(new Mock<IImportProcessor>().Object, new Mock<ISqlServer>().Object, new Mock<IEmailServer>().Object, new Mock<IFtpServer>().Object, new Mock<IFileServer>().Object, null);
        }

    }
}
