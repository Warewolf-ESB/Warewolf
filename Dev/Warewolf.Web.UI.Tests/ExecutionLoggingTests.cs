using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Support.UI;
using System.Net;
using System.Windows.Forms;
using AutoTestSharedTools.VideoRecorder;
using System.IO;

namespace SeleniumTests
{
    [TestClass]
    public class ExecutionLoggingTests
    {
        private IWebDriver driver;
        private StringBuilder verificationErrors;
        private string baseURL;
        private bool acceptNextAlert = true;

        public TestContext TestContext { get; set; }
        private FfMpegVideoRecorder screenRecorder = new FfMpegVideoRecorder();
        private string screenRecordingFilePath;

        [TestInitialize]
        public void SetupTest()
        {
            screenRecordingFilePath = Path.Combine(TestContext.DeploymentDirectory, TestContext.TestName + "_on_" + Environment.MachineName) + "." + FfMpegVideoRecorder.VideoExtention;
            screenRecorder.StartRecord(screenRecordingFilePath);
            WebRequest.Create("http://localhost:3142/secure/Hello%20World.json?Name=Tester");
            driver = new InternetExplorerDriver();
            baseURL = "http://my.warewolf.io";
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
            screenRecorder.StopRecord();
            if (TestContext.CurrentTestOutcome == UnitTestOutcome.Passed)
            {
                File.Delete(screenRecordingFilePath);
            }
            else
            {
                TestContext.AddResultFile(screenRecordingFilePath);
            }
            Assert.AreEqual("", verificationErrors.ToString());
        }
        
        [TestMethod]
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
