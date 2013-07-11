using System;
using System.Net;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests.Helpers
{
    [TestClass]
    public class VersionCheckerTests
    {
        [TestMethod]
        public void VersionCheckerStartPageUriWithCurrentIsLatestExpectedTake5()
        {
            var checker = new Mock<VersionChecker>();
            checker.Setup(c => c.Latest).Returns(new Version(1, 0, 0, 0));
            checker.Setup(c => c.Current).Returns(new Version(1, 0, 0, 0));

            var startPage = checker.Object.StartPageUri;
            Assert.AreEqual(StringResources.Warewolf_Homepage_Take5, startPage);
        }

        [TestMethod]
        public void VersionCheckerStartPageUriWithCurrentIsNotLatestExpectedStart()
        {
            var checker = new Mock<VersionChecker>();
            checker.Setup(c => c.Latest).Returns(new Version(2, 0, 0, 0));
            checker.Setup(c => c.Current).Returns(new Version(1, 0, 0, 0));

            var startPage = checker.Object.StartPageUri;
            Assert.AreEqual(StringResources.Warewolf_Homepage_Start, startPage);
        }

        [TestMethod]
        [Ignore] // This is ignored as it needs to call out to the web. This needs to move into a nightly regression pack
        public void VersionCheckerStartPageUriExpectedChecksOnlineForLatestVersion()
        {
            Version expected;
            using(var client = new WebClient())
            {
                var version = client.DownloadString(StringResources.Warewolf_Version);
                expected = new Version(version);
            }
            var checker = new VersionChecker();
            var actual = checker.Latest;

            Assert.AreEqual(expected, actual);
        }
    }
}
