using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Common.Tests.Utils
{
    [TestClass]
    public class ExternalProcessBrowserTests
    {
        [TestMethod]
        public void ExternalProcessBrowser_With_ProcessTimout()
        {
            var mockedUrl = new Mock<Uri>("https://warewolf.io");
            mockedUrl.Setup(uri => uri.ToString()).Throws(new TimeoutException());
            var processExecutor = new ExternalProcessExecutor();
            processExecutor.OpenInBrowser(mockedUrl.Object);
        }

        [TestMethod]
        public void ExternalProcessBrowser_With_ComException()
        {
            var mockedUrl = new Mock<Uri>("https://warewolf.io");
            mockedUrl.Setup(uri => uri.ToString()).Throws(new System.Runtime.InteropServices.COMException());
            var processExecutor = new ExternalProcessExecutor();
            processExecutor.OpenInBrowser(mockedUrl.Object);
        }
    }
}
