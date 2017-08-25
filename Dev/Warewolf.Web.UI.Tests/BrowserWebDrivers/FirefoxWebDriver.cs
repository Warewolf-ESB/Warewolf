using OpenQA.Selenium.Firefox;
using System;
using System.IO;

namespace Warewolf.Web.UI.Tests.BrowserWebDrivers
{
    public class FirefoxWebDriver : BaseWebDriver
    {
        static FirefoxProfile firefoxProfile = new FirefoxProfile(Path.Combine(Environment.CurrentDirectory, "WebDriverProfiles", "Firefox"));
        public FirefoxWebDriver() : base(new FirefoxDriver(firefoxProfile))
        {
            
        }
    }
}
