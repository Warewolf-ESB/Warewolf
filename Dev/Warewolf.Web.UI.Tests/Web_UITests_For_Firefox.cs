using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Warewolf.Web.UI.Tests.BrowserWebDrivers;
using Warewolf.Web.UI.Tests.ScreenRecording;

namespace Warewolf.Web.UI.Tests
{
    [TestClass]
    public class Web_UITests_For_Firefox
    {
        private BaseWebDriver driver;
        string browserName = "Firefox";
        public TestContext TestContext { get; set; }
        private FfMpegVideoRecorder screenRecorder = new FfMpegVideoRecorder();

        [TestInitialize]
        public void SetupTest()
        {
            driver = new FirefoxWebDriver();
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
        [DeploymentItem(@"WebDriverProfiles", @"WebDriverProfiles")]
        [TestCategory("ExecutionLogging")]
        public void Firefox_ExecutionLogging_ClickRefresh_UITest()
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
        [DeploymentItem(@"WebDriverProfiles", @"WebDriverProfiles")]
        [TestCategory("NoWarewolfServer")]
        public void NoWarewolfServer_ClickFirefoxRefresh_UITest()
        {
            Assert.IsTrue(driver.KillServerIfRunning(), GlobalConstants.LocalWarewolfServerExpectedDownError);
            driver.GoToUrl();
            Assert.IsTrue(driver.IsAlertPresent(), GlobalConstants.IsAlertPresentError);
            Assert.AreEqual(GlobalConstants.LocalWarewolfServerError, driver.CloseAlertAndGetItsText(false), GlobalConstants.AlertText);
        }
    }
}
