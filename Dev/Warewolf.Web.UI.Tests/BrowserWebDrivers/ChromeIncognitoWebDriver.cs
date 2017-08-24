using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;

namespace Warewolf.Web.UI.Tests.BrowserWebDrivers
{
    public class ChromeIncognitoWebDriver : BaseWebDriver
    {
        static ChromeOptions chromeOptions = new ChromeOptions();
        public ChromeIncognitoWebDriver() : base(new ChromeDriver(chromeOptions))
        {
            chromeOptions.AddArguments(new[] { "--test-type", "start-maximized", "--incognito" });
        }
    }
}
