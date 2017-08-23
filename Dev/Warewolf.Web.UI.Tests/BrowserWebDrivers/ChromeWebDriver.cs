using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;

namespace Warewolf.Web.UI.Tests.BrowserWebDrivers
{
    public class ChromeWebDriver : BaseWebDriver
    {
        public ChromeWebDriver() : base(new ChromeDriver(chromeOptions))
        {
            ChromeOptions chromeOptions = new ChromeOptions();
            chromeOptions.AddArguments(new[] { "user-data-dir=" + Path.Combine(Environment.CurrentDirectory, "WebDriverProfiles", "Chrome"), "start-maximized" });
        }
    }
}
