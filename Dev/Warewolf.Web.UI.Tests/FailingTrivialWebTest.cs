using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Interactions;
using Warewolf.Web.Tests;

namespace SeleniumTests
{
    [TestClass]
    public class AnotherTrivialWebTest
    {
        private IWebDriver driver;
        private StringBuilder verificationErrors;
        private string baseURL;
        private bool acceptNextAlert = true;
        
        [TestInitialize]
        public void SetupTest()
        {
            driver = new FirefoxDriver();
            baseURL = "https://warewolf.io";
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
            Assert.AreEqual("", verificationErrors.ToString());
        }

        [TestMethod]
        public void TheFailingTrivialWebTest()
        {
            driver.Navigate().GoToUrl(baseURL + "/");
            driver.FindElement(By.LinkText("Contact Us")).Click();
            driver.FindElement(By.Id("Field105")).Clear();
            driver.FindElement(By.Id("Field105")).SendKeys("Ashley Lewis");
            driver.FindElement(By.Id("Field2")).Clear();
            driver.FindElement(By.Id("Field2")).SendKeys("ashley.lewis@dev2.co.za");
            driver.FindElement(By.Id("Field3")).Clear();
            driver.FindElement(By.Id("Field3")).SendKeys("I love Warewolf :D:D:D");
            driver.FindElement(By.ClassName("fake-checkbox-label")).Click();
            driver.FindElement(By.LinkText("More info")).JavascriptScrollIntoView(driver);
            driver.FindElement(By.LinkText("More info")).Click();
            Assert.Fail("This test is failing but in the expected way.");
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
