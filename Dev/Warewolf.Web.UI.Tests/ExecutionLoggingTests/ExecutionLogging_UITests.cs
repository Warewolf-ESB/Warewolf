using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Warewolf.Web.Tests;

namespace Warewolf.Web.UI.Tests.ExecutionLoggingTests
{
    class ExecutionLogging_UITests
    {
        public static void ClickRefresh_UITest(IWebDriver driver, string baseURL)
        {
            driver.Navigate().GoToUrl(baseURL + "/ExecutionLogging");
            Assert.IsFalse(driver.IsAlertPresent(), driver.CloseAlertAndGetItsText(false));
            driver.FindElement(By.Id("updateServer")).Click();
        }
    }
}
