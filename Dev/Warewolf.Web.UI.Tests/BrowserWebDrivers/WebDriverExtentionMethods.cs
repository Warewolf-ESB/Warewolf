using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using Warewolf.Web.UI.Tests.ScreenRecording;
using OpenQA.Selenium.Support.UI;
using System;
using System.Net;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Opera;

namespace Warewolf.Web.UI.Tests
{
    public static class WebDriverExtentionMethods
    {
        public static void JavascriptScrollIntoView(this IWebElement Element, IWebDriver driver, bool scrollDown = true)
        {
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(" + (scrollDown ? "true" : "false") + ");", Element);
            Assert.IsTrue(Element.Displayed, "Failed to scroll " + Element.TagName + " into view.");
        }

        public static bool IsAlertPresent(this IWebDriver driver)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
                wait.Until(ExpectedConditions.AlertIsPresent());
                driver.SwitchTo().Alert();
                return true;
            }
            catch (NoAlertPresentException)
            {
                return false;
            }
            catch (WebDriverTimeoutException)
            {
                return false;
            }
        }

        public static string CloseAlertAndGetItsText(this IWebDriver driver, bool acceptAlert)
        {
            try
            {
                IAlert alert = driver.SwitchTo().Alert();
                string alertText = alert.Text;
                if (acceptAlert)
                {
                    alert.Accept();
                }
                else
                {
                    alert.Dismiss();
                }
                return alertText;
            }
            catch (NoAlertPresentException)
            {
                return string.Empty;
            }
        }
    }
}
