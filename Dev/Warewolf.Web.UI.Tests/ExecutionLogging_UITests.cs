using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using System;
using System.Net;

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
                driver.Quit();
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
#if DEBUG
        public void ExecutionLogging_ClickRefresh_UITest()
#else
        public void Release_ExecutionLogging_ClickRefresh_UITest()
#endif
        {
            //Generate some test log data
            WebRequest.Create("http://localhost:3142/secure/Hello%20World.json?Name=Tester");
            driver.driver.Navigate().GoToUrl(driver.baseURL + "/ExecutionLogging/");
            var browserType = TestContext.DataRow[0].ToString();
            switch (browserType)
            {
                case "Firefox":
                    Assert.IsTrue(driver.IsAlertPresent(), browserType + " is expecting the user credentials dialog");
                    string expectedError = "http://localhost:3142 is requesting your username and password.";
                    string assertMessage = browserType + " did not match the user credentials dialog error message.";
                    Assert.AreEqual(expectedError, driver.CloseAlertAndGetItsText(false), assertMessage);
                    break;
                case "InternetExplorer":
                    ValidateBrowser(browserType);
                    driver.ClickUpdateServer();
                    break;
                case "Opera":
                    ValidateBrowser(browserType);
                    driver.ClickUpdateServer();
                    break;
                case "ChromeIncognito":
                    ValidateBrowser(browserType);
                    driver.ClickUpdateServer();
                    break;
                default:
                    ValidateBrowser(browserType);
                    driver.ClickUpdateServer();
                    break;
            }
        }

        private void ValidateBrowser(string browserType)
        {
            Assert.IsTrue(driver.WaitForSpinner());
            Assert.IsTrue(driver.WaitForExecutionList());
            Assert.IsTrue(driver.driver.FindElement(By.Id("executionList")).Displayed);
            string assertMessage = browserType + " should not show the user credentials dialog" + Environment.NewLine + driver.CloseAlertAndGetItsText(false);
            Assert.IsFalse(driver.IsAlertPresent(), assertMessage);
        }
    }
}
