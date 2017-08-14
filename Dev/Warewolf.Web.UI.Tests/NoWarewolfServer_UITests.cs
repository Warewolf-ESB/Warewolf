using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

namespace Warewolf.Web.UI.Tests.NoWarewolfServer
{
    [TestClass]
    public class NoWarewolfServer_UITests
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
                driver.driver.Dispose();
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
            screenRecorder.StopRecording(TestContext.CurrentTestOutcome);
        }
        
        [TestMethod]
        [DeploymentItem(@"avformat-57.dll")]
        [DeploymentItem(@"avutil-55.dll")]
        [DeploymentItem(@"swresample-2.dll")]
        [DeploymentItem(@"swscale-4.dll")]
        [DeploymentItem(@"avcodec-57.dll")]
        [TestCategory("NoWarewolfServer")]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "|DataDirectory|\\SupportedBrowsers.csv", "SupportedBrowsers#csv", DataAccessMethod.Sequential), DeploymentItem("SupportedBrowsers.csv")]
        public void NoWarewolfServer_ClickRefresh_UITest()
        {
            driver.driver.Navigate().GoToUrl(driver.baseURL + "/ExecutionLogging");
            Assert.IsTrue(driver.driver.IsAlertPresent(), "No alert that local Warewolf server is not running.");
            Assert.AreEqual("Local Warewolf Server Not Found", driver.driver.CloseAlertAndGetItsText(false), "Alert text is not correct");
        }
    }
}
