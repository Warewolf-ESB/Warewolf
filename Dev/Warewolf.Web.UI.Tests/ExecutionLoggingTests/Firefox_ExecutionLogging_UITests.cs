using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Warewolf.Web.UI.Tests.BrowserWebDrivers;
using Warewolf.Web.UI.Tests.ScreenRecording;

namespace Warewolf.Web.UI.Tests
{
    [TestClass]
    public class Firefox_ExecutionLogging_UITests
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
        public void ExecutionLogging_ClickFirefoxRefresh_UITest()
        {
            //Generate some test log data
            driver.CreateWebRequest();
            driver.GoToUrl();

            Assert.IsTrue(driver.IsAlertPresent(), string.Format(GlobalConstants.UserCredentialsNotShowingError, browserName));
            Assert.AreEqual(GlobalConstants.ExpectedUserAuthError, driver.CloseAlertAndGetItsText(false), string.Format(GlobalConstants.UserAuthAssertMessage, browserName));
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
            driver.GoToUrl();
            Assert.IsTrue(driver.IsAlertPresent(), GlobalConstants.IsAlertPresentError);
            Assert.AreEqual(GlobalConstants.LocalWarewolfServerError, driver.CloseAlertAndGetItsText(false), GlobalConstants.AlertText);
        }
    }
}
