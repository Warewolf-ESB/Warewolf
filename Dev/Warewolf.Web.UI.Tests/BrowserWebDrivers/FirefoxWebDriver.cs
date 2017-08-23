using OpenQA.Selenium.Firefox;
using System;
using System.IO;

namespace Warewolf.Web.UI.Tests.BrowserWebDrivers
{
    public class FirefoxWebDriver : BaseWebDriver
    {
        public FirefoxWebDriver() : base(new FirefoxDriver(new FirefoxProfile(Path.Combine(Environment.CurrentDirectory, "WebDriverProfiles", "Firefox"))))
        {
            
        }
    }
}
