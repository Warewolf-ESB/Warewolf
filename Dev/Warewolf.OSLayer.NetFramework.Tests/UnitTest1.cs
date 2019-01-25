using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Warewolf.OSLayer.NetFramework.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void Version_Matches_TestFrameworkVersion()
        {
            var dotnet = OSLayerVersion.GetOSFramework();
            Assert.AreEqual(Environment.Version, dotnet);
        }
    }
}
