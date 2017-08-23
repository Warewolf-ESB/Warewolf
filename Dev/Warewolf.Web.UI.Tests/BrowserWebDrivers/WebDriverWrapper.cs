using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Opera;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using System;
using System.Diagnostics;
using System.IO;

namespace Warewolf.Web.UI.Tests
{
    class WebDriverWrapper
    {
        public IWebDriver driver;
#if DEBUG
        public string baseURL = "http://localhost:18405";
#else
        public string baseURL = "http://my.warewolf.io";
#endif

        public void InitializeWebDriver(string browserName)
        {
            switch (browserName)
            {
                case "Firefox":
                    {
                        FirefoxProfile profile = new FirefoxProfile(Path.Combine(Environment.CurrentDirectory, "WebDriverProfiles", "Firefox"));
                        driver = new FirefoxDriver(profile);
                        break;
                    }
                case "IE":
                    {
                        driver = new InternetExplorerDriver();
                        break;
                    }
                case "Opera":
                    {
                        string path = @"C:\Program Files\Opera";
                        var operaPath = string.Empty;

                        string[] files = Directory.GetFiles(path, "*opera.exe", System.IO.SearchOption.AllDirectories);
                        foreach (var file in files)
                        {
                            operaPath = file;
                            break;
                        }

                        OperaOptions operaOptions = new OperaOptions(){ BinaryLocation = operaPath };
                        operaOptions.AddArguments(new[] { "user-data-dir=" + Path.Combine(Environment.CurrentDirectory, "WebDriverProfiles", "Opera"), "start-maximized" });
                        driver = new OperaDriver(operaOptions);
                        break;
                    }
                case "Chrome":
                    {
                        ChromeOptions chromeOptions = new ChromeOptions();
                        chromeOptions.AddArguments(new[] { "user-data-dir=" + Path.Combine(Environment.CurrentDirectory, "WebDriverProfiles", "Chrome"), "start-maximized" });
                        driver = new ChromeDriver(chromeOptions);
                        break;
                    }
                default:
                case "ChromeIncognito":
                    {
                        ChromeOptions chromeOptions = new ChromeOptions();
                        chromeOptions.AddArguments(new[] { "--test-type" });
                        chromeOptions.AddArgument("start-maximized");
                        DesiredCapabilities capabilities = DesiredCapabilities.Chrome();
                        capabilities.SetCapability("chrome.switches", new[] { "--incognito" });
                        capabilities.SetCapability(ChromeOptions.Capability, chromeOptions);
                        driver = new ChromeDriver();
                        driver.Manage().Cookies.AddCookie(new Cookie("baseUrl", baseURL));
                        break;
                    }
            }
        }

        public void Quit()
        {
            driver.Quit();
            foreach (var process in Process.GetProcessesByName("opera"))
            {
                process.Kill();
            }
            foreach (var process in Process.GetProcessesByName("operadriver"))
            {
                process.Kill();
            }
            driver.Close();
        }

        public IOptions Manage()
        {
            return driver.Manage();
        }

        public INavigation Navigate()
        {
            return driver.Navigate();
        }

        public ITargetLocator SwitchTo()
        {
            return driver.SwitchTo();
        }

        public void JavascriptScrollIntoView(IWebElement Element, bool scrollDown = true)
        {
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(" + (scrollDown ? "true" : "false") + ");", Element);
            throw new System.Exception("Failed to scroll " + Element.TagName + " into view.");
        }

        public bool IsAlertPresent()
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

        public string CloseAlertAndGetItsText(bool acceptAlert)
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

        public bool WaitForSpinner()
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
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
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
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
            driver.FindElement(By.Id("updateServer")).Click();
        }
    }
}
