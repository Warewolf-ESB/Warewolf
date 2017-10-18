using Dev2.Common.Interfaces;
using Dev2.Factory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Diagnostics;

namespace Dev2.Core.Tests.Factories
{
    [TestClass]
    public class CustomGitOpsTests
    {
        [TestMethod]
        public void CustomGitOps_ExecutesSixCommands()
        {
            //------------Setup for test--------------------------
            var executor = new Mock<IExternalProcessExecutor>();
            CustomGitOps.SetCustomGitTool(executor.Object);
            executor.Verify(p => p.Start(It.IsAny<ProcessStartInfo>()), Times.Exactly(6));
        }

        [TestMethod]
        public void CustomGitOps_ExecutesSixCommands_WithExceptions()
        {
            //------------Setup for test--------------------------
            var executor = new Mock<IExternalProcessExecutor>();
            executor.Setup(p => p.Start(It.IsAny<ProcessStartInfo>())).Throws(new System.Exception());
            CustomGitOps.SetCustomGitTool(executor.Object);
            executor.Verify(p => p.Start(It.IsAny<ProcessStartInfo>()), Times.Exactly(6));
        }

        [TestMethod]
        public void GitPath_ExecutesThreeCommands()
        {
            //------------Setup for test--------------------------
            var executor = new Mock<IExternalProcessExecutor>();
            PrivateType privateType = new PrivateType(typeof(CustomGitOps));
            var gitPath = privateType.InvokeStatic("GetGitExePath");
            StringAssert.EndsWith(gitPath.ToString(), "git.exe");

        }
    }
}
