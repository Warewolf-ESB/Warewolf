using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.ObjectModel;
using System.Net;

namespace Warewolf.Web.UI.Tests
{
    public class BaseWebDriver
    {
        IWebDriver _driver;

        public BaseWebDriver(IWebDriver driver)
        {
            _driver = driver;
        }

        private BaseWebDriver() { }

#if DEBUG
        public string baseURL = "http://localhost:18405";
#else
        public string baseURL = "http://my.warewolf.io";
#endif
       
        public void Close()
        {
            _driver.Close();
            foreach (var process in Process.GetProcessesByName("opera"))
            {
                process.Kill();
            }
            foreach (var process in Process.GetProcessesByName("operadriver"))
            {
                process.Kill();
            }
            foreach (var process in Process.GetProcessesByName("ieserverdriver"))
            {
                process.Kill();
            }
        }

        public void Quit()
        {
            _driver.Quit();
        }

        public IOptions Manage()
        {
            return _driver.Manage();
        }

        public INavigation Navigate()
        {
            return _driver.Navigate();
        }

        public void GoToUrl()
        {
            Navigate().GoToUrl(baseURL + "/ExecutionLogging/");
        }

        public void CreateWebRequest()
        {
            WebRequest.Create("http://localhost:3142/secure/Hello%20World.json?Name=Tester");
        }

        public ITargetLocator SwitchTo()
        {
            return _driver.SwitchTo();
        }

        public IWebElement FindElement(By by)
        {
            return _driver.FindElement(by);
        }

        public ReadOnlyCollection<IWebElement> FindElements(By by)
        {
            return _driver.FindElements(by);
        }

        public void Dispose()
        {
            _driver.Dispose();
        }

        public void JavascriptScrollIntoView(IWebElement Element, bool scrollDown = true)
        {
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView(" + (scrollDown ? "true" : "false") + ");", Element);
            throw new Exception("Failed to scroll " + Element.TagName + " into view.");
        }

        public bool IsAlertPresent()
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(5));
                wait.Until(ExpectedConditions.AlertIsPresent());
                SwitchTo().Alert();
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

        public string CloseAlertAndGetItsText(bool acceptAlert)
        {
            try
            {
                IAlert alert = SwitchTo().Alert();
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

        public bool WaitForSpinner()
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(5));
                wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.Id("loader")));
                return true;
            }
            catch (InvalidElementStateException)
            {
                return false;
            }
        }

        public bool WaitForExecutionList()
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(5));
                wait.Until(ExpectedConditions.ElementIsVisible(By.Id("executionList")));
                return true;
            }
            catch (InvalidElementStateException)
            {
                return false;
            }
        }

        public void ClickUpdateServer()
        {
            FindElement(By.Id("updateServer")).Click();
        }

        public bool IsExecutionListVisible()
        {
            return FindElement(By.Id("executionList")).Displayed;
        }

        public static string GetOperaPath()
        {
            string path = @"C:\Program Files\Opera";
            var operaPath = string.Empty;

            string[] files = System.IO.Directory.GetFiles(path, "*.exe", System.IO.SearchOption.AllDirectories);
            foreach (var file in files)
            {
                if (file.EndsWith("opera.exe"))
                {
                    operaPath = file;
                    break;
                }
            }

            return operaPath;
        }
    }
}
