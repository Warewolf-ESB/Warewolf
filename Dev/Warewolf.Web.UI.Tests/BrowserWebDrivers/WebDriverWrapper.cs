using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Opera;
using OpenQA.Selenium.Remote;

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
                        driver = new FirefoxDriver();
                        break;
                    }
                case "IE":
                    {
                        driver = new InternetExplorerDriver();
                        break;
                    }
                case "Opera":
                    {
                        driver = new OperaDriver(new OperaOptions() { BinaryLocation = @"C:\Program Files\Opera\47.0.2631.39\opera.exe" });
                        break;
                    }
                default:
                case "ChromeIncognito":
                    {
                        ChromeOptions chromeOptions = new ChromeOptions();
                        chromeOptions.AddArguments(new[] { "--test-type" });
                        DesiredCapabilities capabilities = DesiredCapabilities.Chrome();
                        capabilities.SetCapability("chrome.switches", new[] { "--incognito" });
                        capabilities.SetCapability(ChromeOptions.Capability, chromeOptions);
                        driver = new ChromeDriver();
                        driver.Manage().Cookies.AddCookie(new Cookie("baseUrl", baseURL));
                        break;
                    }
            }
        }
    }
}
