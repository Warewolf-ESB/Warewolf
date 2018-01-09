using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Warewolf.Web.UI.Tests.BrowserWebDrivers;
using Warewolf.Web.UI.Tests.ScreenRecording;

namespace Warewolf.Web.UI.Tests.ExecutionLoggingTests
{
    [TestClass]
    public class Chrome_ExecutionLogging_UITests
    {
        private BaseWebDriver driver;
        string browserName = "Chrome";
        public TestContext TestContext { get; set; }
        private FfMpegVideoRecorder screenRecorder = new FfMpegVideoRecorder();

        [TestInitialize]
        public void SetupTest()
        {
            driver = new ChromeWebDriver();
            screenRecorder.StartRecording(TestContext, browserName);
        }

        [TestCleanup]
        public void TeardownTest()
        {
            try
            {
                driver.Quit();
                driver.Close();
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
        [DeploymentItem(@"chromedriver.exe")]
        [DeploymentItem(@"WebDriverProfiles", @"WebDriverProfiles")]
        [TestCategory("ExecutionLogging")]
        public void Chrome_ExecutionLogging_ClickRefresh_UITest()
        {
            //Generate some test log data
            driver.CreateWebRequest();
            driver.GoToUrl();

            Assert.IsTrue(driver.WaitForSpinner());
            Assert.IsTrue(driver.WaitForExecutionList());
            Assert.IsTrue(driver.IsExecutionListVisible());
            string assertMessage = string.Format(GlobalConstants.UserCredentialsShowingError, browserName) + Environment.NewLine + driver.CloseAlertAndGetItsText(false);
            Assert.IsFalse(driver.IsAlertPresent(), assertMessage);

            driver.ClickUpdateServer();
        }

        [TestMethod]
        [DeploymentItem(@"avformat-57.dll")]
        [DeploymentItem(@"avutil-55.dll")]
        [DeploymentItem(@"swresample-2.dll")]
        [DeploymentItem(@"swscale-4.dll")]
        [DeploymentItem(@"avcodec-57.dll")]
        [DeploymentItem(@"chromedriver.exe")]
        [DeploymentItem(@"WebDriverProfiles", @"WebDriverProfiles")]
        [TestCategory("NoWarewolfServer")]
        public void Chrome_NoWarewolfServer_UITest()
        {
            Assert.IsTrue(driver.KillServerIfRunning(), GlobalConstants.LocalWarewolfServerExpectedDownError);
            driver.GoToUrl();
            Assert.IsTrue(driver.IsAlertPresent(), GlobalConstants.IsAlertPresentError);
            Assert.AreEqual(GlobalConstants.LocalWarewolfServerError, driver.CloseAlertAndGetItsText(false), GlobalConstants.AlertText);
        }
    }
}
