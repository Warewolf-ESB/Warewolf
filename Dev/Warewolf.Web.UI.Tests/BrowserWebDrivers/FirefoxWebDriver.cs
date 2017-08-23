using OpenQA.Selenium.Firefox;

namespace Warewolf.Web.UI.Tests.BrowserWebDrivers
{
    public class FirefoxWebDriver : BaseWebDriver
    {
        public FirefoxWebDriver() : base(new FirefoxDriver(GetFirefoxProfile(Path.Combine(Environment.CurrentDirectory, "WebDriverProfiles", "Firefox"))))
        {
            
        }
    }
}
