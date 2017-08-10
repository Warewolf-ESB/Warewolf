using System;
using System.Text;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Opera;
using Warewolf.Web.UI.Tests.ScreenRecording;
using System.Net;
using Warewolf.Web.Tests;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using System.Diagnostics;
using Warewolf.Web.UI.Tests.ExecutionLoggingTests;

namespace Warewolf.Web.UI.Tests.ExecutionLoggingTests.Local
{
    [TestClass]
    public class Opera
    {
        public TestContext TestContext { get; set; }
        private FfMpegVideoRecorder screenRecorder = new FfMpegVideoRecorder();
        private IWebDriver driver;
        private StringBuilder verificationErrors;
        private string baseURL;
        private bool acceptNextAlert = true;

        [TestInitialize]
        public void SetupTest()
        {
            //Generate some test log data
            WebRequest.Create("http://localhost:3142/secure/Hello%20World.json?Name=Tester");
            
            driver = new OperaDriver(new OperaOptions() { BinaryLocation = @"C:\Program Files\Opera\47.0.2631.39\opera.exe" });
            baseURL = "http://localhost:18405";
            screenRecorder.StartRecording(TestContext);
            verificationErrors = new StringBuilder();
        }

        [TestCleanup]
        public void TeardownTest()
        {
            try
            {
                driver.Quit();
                driver.Dispose();
                Process[] processes = Process.GetProcessesByName("opera");
                foreach (var process in processes)
                {
                    process.Kill();
                }
            }
            catch (Exception)
            {
                // Ignore errors if unable to close the browser
            }
            screenRecorder.StopRecording(TestContext);
            Assert.AreEqual("", verificationErrors.ToString());
        }
        
        [TestMethod]
        [DeploymentItem(@"avformat-57.dll")]
        [DeploymentItem(@"avutil-55.dll")]
        [DeploymentItem(@"swresample-2.dll")]
        [DeploymentItem(@"swscale-4.dll")]
        [DeploymentItem(@"avcodec-57.dll")]
        public void ExecutionLogging_ClickRefresh_Opera_UITest()
        {
            ExecutionLogging_UITests.ClickRefresh_UITest(driver, baseURL);
        }

        private bool IsElementPresent(By by)
        {
            try
            {
                driver.FindElement(by);
                return true;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        }
    }
}
