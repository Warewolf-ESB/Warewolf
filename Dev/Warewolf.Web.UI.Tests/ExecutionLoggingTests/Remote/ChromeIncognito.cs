using System;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Warewolf.Web.UI.Tests.ScreenRecording;
using System.Net;
using OpenQA.Selenium.Remote;
using Warewolf.Web.Tests;
using Warewolf.Web.UI.Tests.ExecutionLoggingTests;

namespace Warewolf.Web.UI.Tests.ExecutionLoggingTests.Remote
{
    [TestClass]
    public class ChromeIncognito
    {
        public TestContext TestContext { get; set; }
        private FfMpegVideoRecorder screenRecorder = new FfMpegVideoRecorder();
        private IWebDriver driver;
        private StringBuilder verificationErrors;
        private string baseURL;

        [TestInitialize]
        public void SetupTest()
        {
            //Generate some test log data
            WebRequest.Create("http://localhost:3142/secure/Hello%20World.json?Name=Tester");

            ChromeOptions chromeOptions = new ChromeOptions();
            chromeOptions.AddArguments(new[] { "--test-type" });
            DesiredCapabilities capabilities = DesiredCapabilities.Chrome();
            capabilities.SetCapability("chrome.switches", new[] { "--incognito" });
            capabilities.SetCapability(ChromeOptions.Capability, chromeOptions);
            driver = new ChromeDriver();
            baseURL = "http://my.warewolf.io";
            screenRecorder.StartRecording(TestContext);
            verificationErrors = new StringBuilder();
        }

        [TestCleanup]
        public void TeardownTest()
        {
            try
            {
                driver.Quit();
            }
            catch (Exception)
            {
                // Ignore errors if unable to close the browser
            }
            screenRecorder.StopRecording(TestContext.CurrentTestOutcome);
            Assert.AreEqual("", verificationErrors.ToString());
        }
        
        [TestMethod]
        [DeploymentItem(@"avformat-57.dll")]
        [DeploymentItem(@"avutil-55.dll")]
        [DeploymentItem(@"swresample-2.dll")]
        [DeploymentItem(@"swscale-4.dll")]
        [DeploymentItem(@"avcodec-57.dll")]
        public void ExecutionLogging_Remote_ChromeIncognito_ClickRefresh_UITest()
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
