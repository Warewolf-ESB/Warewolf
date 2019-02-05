using System;
using System.Diagnostics;
using System.IO.Ports;
using Dev2.Common.Wrappers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Common.Tests
{
    [TestClass]
    public class ExternalProcessExecutorTests
    {
        //[TestMethod]
        //[Owner("Siphamandla Dube")]
        //[TestCategory(nameof(ExternalProcessExecutor))]
        //public void ExternalProcessExecutor_Start_ProcessStartInfo_VerifyCalls_ToBeOnce_ExpectTrue()
        //{
        //    //---------------------Arrange-------------------------
        //    var mockProcessWrapper = new Mock<IProcessFactory>();

        //    var processStartInfo = new ProcessStartInfo();

        //    var externalProcessExecutor = new ExternalProcessExecutor(mockProcessWrapper.Object);
        //    //---------------------Act-----------------------------
        //     externalProcessExecutor.Start(processStartInfo);
        //    //---------------------Assert--------------------------
        //    mockProcessWrapper.Verify(o => o.Start(It.IsAny<ProcessStartInfo>()), Times.Once);
        //}

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ExternalProcessExecutor))]
        public void ExternalProcessExecutor_Start_OpenInBrowser_VerifyCalls_ToBeOnce_ExpectTrue()
        {
            //---------------------Arrange-------------------------
            var mockProcessWrapper = new Mock<IProcessFactory>();

            var uri = new Uri("https://warewolf.io");

            var externalProcessExecutor = new ExternalProcessExecutor(mockProcessWrapper.Object);
            //---------------------Act-----------------------------
            externalProcessExecutor.OpenInBrowser(uri);
            //---------------------Assert--------------------------
            mockProcessWrapper.Verify(o => o.Start(uri.ToString()), Times.Once);
        }

        //[TestMethod]
        //[Owner("Siphamandla Dube")]
        //[TestCategory(nameof(ExternalProcessExecutor))]
        //public void ExternalProcessExecutor_Start_Catch_TimeoutException_ToBeOnce_ExpectTrue()
        //{
        //    //---------------------Arrange-------------------------
        //    var mockProcessWrapper = new Mock<IProcessFactory>();

        //    var uri = new Uri("www.google.com:81");
            
        //    var externalProcessExecutor = new ExternalProcessExecutor(mockProcessWrapper.Object);
        //    //---------------------Act-----------------------------
        //    externalProcessExecutor.OpenInBrowser(uri);
        //    //---------------------Assert--------------------------
        //    mockProcessWrapper.Setup(o => o.Start(uri.ToString())).Throws<TimeoutException>().Verifiable();

        //    mockProcessWrapper.VerifyAll();
        //}
    }
}
