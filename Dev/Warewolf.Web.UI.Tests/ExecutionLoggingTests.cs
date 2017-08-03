using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Warewolf.Web.UI.Tests.ScreenRecording;
using OpenQA.Selenium.Support.UI;
using System.Net;
using System.Windows.Forms;
using System.IO;
using OpenQA.Selenium.Remote;

namespace SeleniumTests
{
    [TestClass]
    public class ExecutionLoggingTests
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

            ChromeOptions chromeOptions = new ChromeOptions();
            chromeOptions.AddArguments(new[] { "--test-type"});
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
            screenRecorder.StopRecording(TestContext);
            Assert.AreEqual("", verificationErrors.ToString());
        }
        
        [TestMethod]
        [DeploymentItem(@"avformat-57.dll")]
        [DeploymentItem(@"avutil-55.dll")]
        [DeploymentItem(@"swresample-2.dll")]
        [DeploymentItem(@"swscale-4.dll")]
        [DeploymentItem(@"avcodec-57.dll")]
        public void ExecutionLoggingTest()
        {
            driver.Navigate().GoToUrl(baseURL + "/ExecutionLogging");
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
        
        private bool IsAlertPresent()
        {
            try
            {
                driver.SwitchTo().Alert();
                return true;
            }
            catch (NoAlertPresentException)
            {
                return false;
            }
        }
        
        private string CloseAlertAndGetItsText() {
            try {
                IAlert alert = driver.SwitchTo().Alert();
                string alertText = alert.Text;
                if (acceptNextAlert) {
                    alert.Accept();
                } else {
                    alert.Dismiss();
                }
                return alertText;
            } finally {
                acceptNextAlert = true;
            }
        }
    }
}
