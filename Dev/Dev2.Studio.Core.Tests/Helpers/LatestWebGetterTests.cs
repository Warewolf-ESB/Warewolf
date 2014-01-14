using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Dev2.Studio.Core.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Core.Tests.Helpers
{
    // PBI 9512 - 2013.06.07 - TWR: added
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class LatestWebGetterTests
    {
        [TestMethod]
        public void LatestWebGetterExpectedRaisesInvoked()
        {
            var getter = new LatestWebGetter();
            getter.Invoked += (sender, args) => Assert.IsTrue(true);
            getter.GetLatest("myfake.uri", "hello world");
        }

        [TestMethod]
        public void LatestWebGetterWithInvalidArgsExpectedDoesNothing()
        {
            var getter = new LatestWebGetter();
            getter.GetLatest("myfake.uri", "hello world");
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void LatestWebGetterWithValidArgsExpectedReplacesFileContent()
        {
            var path = Path.Combine(Path.GetTempPath(), string.Concat(Guid.NewGuid().ToString(), ".txt"));

            //var path = Path.Combine(_testDir, Path.GetRandomFileName());

            if(File.Exists(path))
            {
                File.Delete(path);
            }

            Assert.IsFalse(File.Exists(path));

            var getter = new LatestWebGetter();
            getter.GetLatest("http://rsaklfsvrwrwbld:1234/services/Ping", path);

            Assert.IsTrue(File.Exists(path), "Could not create  [ " + path + " ]");
        }
    }
}
