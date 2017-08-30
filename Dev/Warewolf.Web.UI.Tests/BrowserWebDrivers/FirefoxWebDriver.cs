using OpenQA.Selenium.Firefox;
using System;
using System.IO;

namespace Warewolf.Web.UI.Tests.BrowserWebDrivers
{
    public class FirefoxWebDriver : BaseWebDriver
    {
        static FirefoxOptions firefoxOptions = new FirefoxOptions() { Profile = new FirefoxProfile(Path.Combine(Environment.CurrentDirectory, "WebDriverProfiles", "Firefox")) };
        public FirefoxWebDriver() : base(new FirefoxDriver(FirefoxDriverService.CreateDefaultService(Environment.CurrentDirectory), firefoxOptions, TimeSpan.FromMinutes(3)))
        {
            
        }
    }
}
