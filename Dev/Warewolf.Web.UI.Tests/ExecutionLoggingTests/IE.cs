using System;
using System.Text;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.IE;
using Warewolf.Web.UI.Tests.ScreenRecording;
using System.Net;
using Warewolf.Web.Tests;

namespace SeleniumTests
{
    [TestClass]
    public class IE
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
            
            driver = new InternetExplorerDriver();
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
            screenRecorder.StopRecording(TestContext);
            Assert.AreEqual("", verificationErrors.ToString());
        }
        
        [TestMethod]
        [DeploymentItem(@"avformat-57.dll")]
        [DeploymentItem(@"avutil-55.dll")]
        [DeploymentItem(@"swresample-2.dll")]
        [DeploymentItem(@"swscale-4.dll")]
        [DeploymentItem(@"avcodec-57.dll")]
        public void ExecutionLogging_ClickRefresh_IE_UITest()
        {
            driver.Navigate().GoToUrl(baseURL + "/ExecutionLogging");
            Assert.IsFalse(driver.IsAlertPresent(), driver.CloseAlertAndGetItsText(false));
            driver.FindElement(By.Id("updateServer")).Click();
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
