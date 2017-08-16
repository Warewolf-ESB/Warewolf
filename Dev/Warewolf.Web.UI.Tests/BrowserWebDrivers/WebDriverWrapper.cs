using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Opera;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using System;
using System.Diagnostics;

namespace Warewolf.Web.UI.Tests
{
    class WebDriverWrapper
    {
        public IWebDriver driver;
#if DEBUG
        public string baseURL = "http://localhost:18405";
        public string userName = "";
        public string password = "";
#else
        public string baseURL = "http://my.warewolf.io";
#endif

        public void InitializeWebDriver(string browserName)
        {
            switch (browserName)
            {
                case "Firefox":
                    {
                        var profileManager = new FirefoxProfileManager();
                        FirefoxProfile myprofile = profileManager.GetProfile("ExecutionLoggingTestUser");
                        var firefoxOptions = new FirefoxOptions();
                        driver = new FirefoxDriver(myprofile);
                        break;
                    }
                case "IE":
                    {
                        driver = new InternetExplorerDriver();
                        break;
                    }
                case "Opera":
                    {
                        driver = new OperaDriver(new OperaOptions() { BinaryLocation = @"C:\Program Files\Opera\47.0.2631.55\opera.exe" });
                        break;
                    }
                default:
                case "Chrome":
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
            driver.Close();
            foreach (var process in Process.GetProcessesByName("opera"))
            {
                process.Kill();
            }
            foreach (var process in Process.GetProcessesByName("operadriver"))
            {
                process.Kill();
            }
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
    }
}
