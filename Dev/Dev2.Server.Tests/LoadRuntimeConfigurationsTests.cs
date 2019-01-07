using System;
using Dev2.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Server.Tests
{
    [TestClass]
    public class LoadRuntimeConfigurationsTests
    {
        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(LoadRuntimeConfigurations))]
        public void LoadRuntimeConfigurations_WebServerUri_Null_ExpectFail()
        {
            //-------------------Arrange------------------
            var mockWriter = new Mock<IWriter>();
            //-------------------Act----------------------
            new LoadRuntimeConfigurations(mockWriter.Object).Execute();
            //-------------------Assert-------------------
            mockWriter.Verify(o => o.Write("fail."), Times.Exactly(2));
            mockWriter.Verify(o => o.WriteLine("Value cannot be null.\r\nParameter name: webServerUri"), Times.Once);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(LoadRuntimeConfigurations))]
        public void LoadRuntimeConfigurations_WebServerUri_NotNull_ExpectFail()
        {
            //-------------------Arrange------------------
            var mockWriter = new Mock<IWriter>();
            EnvironmentVariables.WebServerUri = "http://TESTUSERNAME:8080/";
            //-------------------Act----------------------
            new LoadRuntimeConfigurations(mockWriter.Object).Execute();
            //-------------------Assert-------------------
            mockWriter.Verify(o => o.WriteLine("done."), Times.Exactly(2));
        }
    }
}
