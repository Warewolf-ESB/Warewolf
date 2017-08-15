using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using System;
using System.Net;
using Warewolf.Web.UI.Tests;

namespace Warewolf.Web.UI.Tests.ExecutionLoggingTests
{
    [TestClass]
    public class ExecutionLogging_UITests
    {
        public TestContext TestContext { get; set; }
        private WebDriverWrapper driver = new WebDriverWrapper();
        private ScreenRecording.FfMpegVideoRecorder screenRecorder = new ScreenRecording.FfMpegVideoRecorder();

        [TestInitialize]
        public void SetupTest()
        {
            driver.InitializeWebDriver(TestContext.DataRow[0].ToString());
            screenRecorder.StartRecording(TestContext);
        }

        [TestCleanup]
        public void TeardownTest()
        {
            try
            {
                driver.driver.Quit();
            }
            catch (Exception)
            {
                // Ignore errors if unable to close the browser
            }
            screenRecorder.StopRecording(TestContext.CurrentTestOutcome);
        }

        [TestMethod]
        [DeploymentItem(@"avformat-57.dll")]
        [DeploymentItem(@"avutil-55.dll")]
        [DeploymentItem(@"swresample-2.dll")]
        [DeploymentItem(@"swscale-4.dll")]
        [DeploymentItem(@"avcodec-57.dll")]
        [TestCategory("ExecutionLogging")]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "|DataDirectory|\\SupportedBrowsers.csv", "SupportedBrowsers#csv", DataAccessMethod.Sequential), DeploymentItem("SupportedBrowsers.csv")]
        public void ExecutionLogging_ClickRefresh_UITest()
        {
            //Generate some test log data
            WebRequest.Create("http://localhost:3142/secure/Hello%20World.json?Name=Tester");
            driver.driver.Navigate().GoToUrl(driver.baseURL + "/ExecutionLogging/");
            var browserType = driver.driver.GetType().Name.ToString();
            switch (browserType)
            {
                case "FirefoxDriver":
                    Assert.AreEqual("http://localhost:3142 is requesting your username and password.", driver.driver.CloseAlertAndGetItsText(false));
                    break;
                case "InternetExplorerDriver":
                    Assert.IsFalse(driver.driver.IsAlertPresent(), driver.driver.CloseAlertAndGetItsText(false));
                    //Assert.AreEqual("http://localhost:3142 is requesting your username and password.", driver.driver.CloseAlertAndGetItsText(false));
                    break;
                case "OperaDriver":
                    Assert.IsFalse(driver.driver.IsAlertPresent(), driver.driver.CloseAlertAndGetItsText(false));
                    break;
                case "ChromeDriver":
                    Assert.IsFalse(driver.driver.IsAlertPresent(), driver.driver.CloseAlertAndGetItsText(false));
                    break;
                default:
                    Assert.IsFalse(driver.driver.IsAlertPresent(), driver.driver.CloseAlertAndGetItsText(false));
                    break;
            }
            //Assert.IsFalse(driver.driver.IsAlertPresent(), driver.driver.CloseAlertAndGetItsText(false));
            driver.driver.FindElement(By.Id("updateServer")).Click();
        }
    }
}
